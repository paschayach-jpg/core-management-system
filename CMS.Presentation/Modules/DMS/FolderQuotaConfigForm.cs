using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    public partial class FolderQuotaConfigForm : Form
    {
        private readonly IFolderQuotaService _quotaService;
        private readonly UserContext _user;

        private List<FolderQuotaConfig> _configs = new List<FolderQuotaConfig>();
        private Dictionary<int, OrgUnitFileStat> _fileStats
            = new Dictionary<int, OrgUnitFileStat>();
        private FolderQuotaConfig _selected;
        private bool _bindingMasterList;

        // drill-down state: Division → Department
        private int?   _drillDownParentId   = null;
        private string _drillDownParentName = null;
        private Panel  _pnlBreadcrumb;
        private Label  _lblBreadcrumb;

        public FolderQuotaConfigForm(IFolderQuotaService quotaService, UserContext user)
        {
            if (quotaService == null) throw new ArgumentNullException("quotaService");
            if (user == null) throw new ArgumentNullException("user");

            _quotaService = quotaService;
            _user = user;

            InitializeComponent();
            SetupDrillDown();
            SetupComboBox();
            SetupContextMenu();
            LoadData();
        }

        private void SetupDrillDown()
        {
            // Refresh button in footer
            var btnRefresh = new Button
            {
                Text      = "รีเฟรช",
                Width     = 90,
                Dock      = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(26, 58, 108),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => { LoadData(); };
            pnlFooter.Controls.Add(btnRefresh);

            // Breadcrumb bar — shown when drilled into Departments of a Division
            _pnlBreadcrumb = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 34,
                BackColor = Color.FromArgb(42, 90, 150),
                Visible   = false
            };

            var btnBack = new Button
            {
                Text      = "  ← กลับ",
                Width     = 90,
                Dock      = DockStyle.Left,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 110, 175),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => DrillBack();

            _lblBreadcrumb = new Label
            {
                Dock      = DockStyle.Fill,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(12, 0, 0, 0)
            };

            _pnlBreadcrumb.Controls.Add(_lblBreadcrumb);
            _pnlBreadcrumb.Controls.Add(btnBack);

            // Insert breadcrumb between pnlEditHost (top) and dgvConfigs (fill)
            // pnlRight.Controls: 0=dgvConfigs(Fill), 1=pnlEditHost(Top)
            // After insert: 0=dgvConfigs, 1=_pnlBreadcrumb, 2=pnlEditHost → pnlEditHost stays on top
            pnlRight.Controls.Add(_pnlBreadcrumb);
            pnlRight.Controls.SetChildIndex(_pnlBreadcrumb, 1);

            // Double-click on grid row to drill down
            dgvConfigs.CellDoubleClick += DgvConfigs_CellDoubleClick;
        }

        private void DgvConfigs_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var cfg = dgvConfigs.Rows[e.RowIndex].Tag as FolderQuotaConfig;
            if (cfg == null) return;
            if (GetOrgUnitType(cfg.OrgUnitTypeName) != 2) return; // Division only
            DrillDown(cfg);
        }

        private void DrillDown(FolderQuotaConfig divisionCfg)
        {
            _drillDownParentId   = divisionCfg.OrgUnitID;
            _drillDownParentName = divisionCfg.OrgUnitName;

            _lblBreadcrumb.Text = string.Format(
                "Division  ▶  {0}  ▶  Department  ({1} รายการ)",
                _drillDownParentName,
                _configs.Count(c => GetOrgUnitType(c.OrgUnitTypeName) == 3
                                 && c.ParentOrgUnitID == _drillDownParentId));

            _pnlBreadcrumb.Visible = true;

            // Switch ComboBox to Department silently
            _bindingMasterList = true;
            cmbMasterType.SelectedIndex = 2;
            _bindingMasterList = false;

            BindMasterList();
            RefreshGrid();
        }

        private void DrillBack()
        {
            _drillDownParentId   = null;
            _drillDownParentName = null;
            _pnlBreadcrumb.Visible = false;

            // Switch ComboBox back to Division silently
            _bindingMasterList = true;
            cmbMasterType.SelectedIndex = 1;
            _bindingMasterList = false;

            BindMasterList();
            RefreshGrid();
        }

        private ContextMenuStrip _chartMenu;

        private void SetupContextMenu()
        {
            _chartMenu = new ContextMenuStrip();
            var mnuChart = new ToolStripMenuItem("ดูการใช้พื้นที่ (กราฟ)");
            mnuChart.Click += MnuChart_Click;
            _chartMenu.Items.Add(mnuChart);

            // ListBox ซ้าย
            lstOrgUnits.MouseDown += LstOrgUnits_MouseDown;

            // DataGridView ขวา
            dgvConfigs.CellMouseDown += DgvConfigs_CellMouseDown;
        }

        private void LstOrgUnits_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            int idx = lstOrgUnits.IndexFromPoint(e.Location);
            if (idx < 0) return;
            lstOrgUnits.SelectedIndex = idx;
            _chartMenu.Show(lstOrgUnits, e.Location);
        }

        private void DgvConfigs_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0) return;
            dgvConfigs.ClearSelection();
            dgvConfigs.Rows[e.RowIndex].Selected = true;
            _chartMenu.Show(dgvConfigs, dgvConfigs.PointToClient(Cursor.Position));
        }

        private void MnuChart_Click(object sender, EventArgs e)
        {
            // หา config จาก ListBox หรือ DataGridView ที่ถูกเลือก
            FolderQuotaConfig cfg = null;

            if (dgvConfigs.SelectedRows.Count > 0)
                cfg = dgvConfigs.SelectedRows[0].Tag as FolderQuotaConfig;

            if (cfg == null)
                cfg = _selected;

            if (cfg == null)
            {
                MessageBox.Show("กรุณาเลือกหน่วยงานก่อน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var usage     = _quotaService.GetUsage(cfg.OrgUnitID, cfg.ParentOrgUnitID);
                usage.OrgUnitName = cfg.OrgUnitName;
                var fileStats = _quotaService.GetFileTypeStats(cfg.OrgUnitID);
                var files     = _quotaService.GetFilesByOrgUnit(cfg.OrgUnitID).ToList();
                using (var frm = new QuotaUsageChartForm(usage, fileStats, files))
                    frm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupComboBox()
        {
            cmbMasterType.DisplayMember = "Text";
            cmbMasterType.ValueMember = "OrgUnitType";
            cmbMasterType.Items.Clear();
            cmbMasterType.Items.Add(new MasterTypeItem(1, "Sub Committee", "EMS.m_SubCommittee"));
            cmbMasterType.Items.Add(new MasterTypeItem(2, "Division", "EMS.m_Division"));
            cmbMasterType.Items.Add(new MasterTypeItem(3, "Department", "EMS.m_Department"));
            cmbMasterType.SelectedIndex = 0;
        }

        private void LoadData()
        {
            _configs   = _quotaService.GetAllConfigs().ToList();
            try { _fileStats = _quotaService.GetAllFileStats(); }
            catch { _fileStats = new Dictionary<int, OrgUnitFileStat>(); }
            BindMasterList();
            RefreshGrid();
        }

        private void cmbMasterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bindingMasterList) return;
            // User changed type manually — clear any active drill-down
            _drillDownParentId   = null;
            _drillDownParentName = null;
            if (_pnlBreadcrumb != null) _pnlBreadcrumb.Visible = false;
            BindMasterList();
            RefreshGrid();
        }

        private const string SearchPlaceholder = "ค้นหาหน่วยงาน...";

        private void txtMasterSearch_Enter(object sender, EventArgs e)
        {
            if (txtMasterSearch.ForeColor == Color.Silver)
            {
                txtMasterSearch.Text = "";
                txtMasterSearch.ForeColor = SystemColors.WindowText;
            }
        }

        private void txtMasterSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMasterSearch.Text))
            {
                txtMasterSearch.Text = SearchPlaceholder;
                txtMasterSearch.ForeColor = Color.Silver;
            }
        }

        private void txtMasterSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtMasterSearch.ForeColor == Color.Silver) return;
            BindMasterList();
        }

        private void lstOrgUnits_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bindingMasterList) return;
            SelectConfig(lstOrgUnits.SelectedItem as FolderQuotaConfig);
        }

        private void lstOrgUnits_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var cfg = lstOrgUnits.Items[e.Index] as FolderQuotaConfig;
            bool hasQuota = cfg != null && cfg.QuotaBytes > 0;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Color bg = selected ? Color.FromArgb(26, 58, 108) : Color.FromArgb(245, 247, 250);
            Color fg = selected ? Color.White : Color.FromArgb(40, 40, 40);

            using (var bgBrush = new SolidBrush(bg))
                e.Graphics.FillRectangle(bgBrush, e.Bounds);

            if (hasQuota)
            {
                using (var bar = new SolidBrush(Color.FromArgb(52, 120, 47)))
                    e.Graphics.FillRectangle(bar, new Rectangle(e.Bounds.X, e.Bounds.Y, 4, e.Bounds.Height));
            }

            string name = cfg != null ? cfg.OrgUnitName : string.Empty;
            using (var textBrush = new SolidBrush(fg))
                e.Graphics.DrawString(name, e.Font, textBrush,
                    new RectangleF(e.Bounds.X + 10, e.Bounds.Y + (e.Bounds.Height - e.Font.Height) / 2f,
                        e.Bounds.Width - 14, e.Bounds.Height));

            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
                e.DrawFocusRectangle();
        }

        private void BindMasterList()
        {
            int orgUnitType = GetSelectedMasterType();
            string keyword = txtMasterSearch.ForeColor == Color.Silver
                ? string.Empty
                : (txtMasterSearch.Text ?? string.Empty).Trim();

            var list = _configs
                .Where(c => GetOrgUnitType(c.OrgUnitTypeName) == orgUnitType)
                .Where(c => _drillDownParentId == null || c.ParentOrgUnitID == _drillDownParentId)
                .Where(c => keyword.Length == 0
                         || (c.OrgUnitName ?? string.Empty).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(c => c.OrgUnitName)
                .ToList();

            _bindingMasterList = true;
            lstOrgUnits.DataSource = null;
            lstOrgUnits.DisplayMember = "OrgUnitName";
            lstOrgUnits.ValueMember = "OrgUnitID";
            lstOrgUnits.DataSource = list;
            lstOrgUnits.ClearSelected();
            _bindingMasterList = false;

            SelectConfig(null);
            lblTreeHeader.Text = string.Format("  {0} ({1})", GetSelectedMasterText(), list.Count);
        }

        private void SelectConfig(FolderQuotaConfig config)
        {
            _selected = config;
            if (_selected == null)
            {
                SetOrgPlaceholder();
                nudQuotaGB.Value = 40;
                btnSave.Text = "บันทึก";
                return;
            }

            string tier = GetMasterTypeShortName(GetOrgUnitType(_selected.OrgUnitTypeName));
            lblSelectedOrg.Text = string.Format("[{0}] {1}", tier, _selected.OrgUnitName);
            lblSelectedOrg.ForeColor = Color.FromArgb(26, 58, 108);
            lblSelectedOrg.Font = new Font(lblSelectedOrg.Font, FontStyle.Bold);
            SetMasterTargetLabel(_selected.OrgUnitTypeName);

            if (_selected.QuotaBytes > 0)
            {
                nudQuotaGB.Value = (decimal)Math.Round(_selected.QuotaGB, 1);
                btnSave.Text = "แก้ไข";
            }
            else
            {
                nudQuotaGB.Value = 40;
                btnSave.Text = "บันทึก";
            }
        }

        private void RefreshGrid()
        {
            int selectedType = GetSelectedMasterType();
            dgvConfigs.Rows.Clear();

            foreach (var c in _configs
                .Where(c => GetOrgUnitType(c.OrgUnitTypeName) == selectedType)
                .Where(c => _drillDownParentId == null || c.ParentOrgUnitID == _drillDownParentId))
            {
                OrgUnitFileStat stat;
                _fileStats.TryGetValue(c.OrgUnitID, out stat);

                int    fileCount = stat != null ? stat.FileCount : 0;
                double usedGB    = stat != null ? stat.UsedGB    : 0;
                double usedPct   = c.QuotaBytes > 0
                    ? Math.Min(usedGB / c.QuotaGB * 100.0, 999)
                    : 0;

                int row = dgvConfigs.Rows.Add(
                    c.QuotaConfigID,
                    c.OrgUnitTypeName,
                    c.OrgUnitName,
                    string.Format("{0:F1} GB", c.QuotaGB),
                    GetMasterTableName(c.OrgUnitTypeName),
                    "quota_size",
                    c.UpdatedDate.HasValue
                        ? c.UpdatedDate.Value.ToString("dd/MM/yyyy")
                        : c.CreatedDate.ToString("dd/MM/yyyy"),
                    fileCount > 0 ? fileCount.ToString("N0") : "-",
                    usedGB    > 0 ? string.Format("{0:F2} GB", usedGB) : "-",
                    usedPct   > 0 ? string.Format("{0:F1}%", usedPct)  : "-"
                );

                dgvConfigs.Rows[row].Tag = c;

                // Color-code % ใช้งาน
                if (usedPct >= 95)
                    dgvConfigs.Rows[row].Cells[colCfgUsedPct.Index].Style.ForeColor = Color.FromArgb(180, 30, 30);
                else if (usedPct >= 80)
                    dgvConfigs.Rows[row].Cells[colCfgUsedPct.Index].Style.ForeColor = Color.FromArgb(200, 120, 0);
                else if (usedPct > 0)
                    dgvConfigs.Rows[row].Cells[colCfgUsedPct.Index].Style.ForeColor = Color.FromArgb(39, 130, 70);
            }

            lblStatus.Text = _drillDownParentId != null
                ? string.Format("Department ภายใต้ {0}: {1} รายการ", _drillDownParentName, dgvConfigs.Rows.Count)
                : string.Format("ทั้งหมด {0} รายการ | ประเภท {1}", dgvConfigs.Rows.Count, GetSelectedMasterText());
        }

        private void dgvConfigs_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvConfigs.SelectedRows.Count == 0) return;
            var cfg = dgvConfigs.SelectedRows[0].Tag as FolderQuotaConfig;
            if (cfg == null) return;

            SelectConfigInList(cfg.OrgUnitID);
        }

        private void ClearForm()
        {
            _selected = null;
            lstOrgUnits.ClearSelected();
            SetOrgPlaceholder();
            nudQuotaGB.Value = 40;
            btnSave.Text = "บันทึก";
        }

        private void SetOrgPlaceholder()
        {
            lblSelectedOrg.Text = "เลือกประเภทหน่วยงาน แล้วคลิกเลือกรายการด้านซ้าย";
            lblSelectedOrg.ForeColor = Color.Gray;
            lblSelectedOrg.Font = new Font(lblSelectedOrg.Font, FontStyle.Italic);
            lblWarn.Text = "บันทึกลง: -";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_selected == null)
            {
                MessageBox.Show("กรุณาเลือกหน่วยงานที่ต้องการกำหนด quota", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _selected.QuotaBytes = (long)(nudQuotaGB.Value * 1073741824m);
            _selected.WarnThresholdPct = 80;
            _selected.CriticalThresholdPct = 95;
            _selected.IsActive = true;
            _selected.CreatedBy = _user.UserID;
            _selected.UpdatedBy = _user.UserID;

            try
            {
                _quotaService.SaveConfig(_selected, _user);
                MessageBox.Show("บันทึกสำเร็จ", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                int orgUnitId = _selected.OrgUnitID;
                LoadData();
                SelectConfigInList(orgUnitId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selected == null || _selected.QuotaBytes == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการที่มีการกำหนด quota แล้ว");
                return;
            }

            if (MessageBox.Show("ต้องการล้างค่า quota ของรายการนี้เป็น 0?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                int orgUnitId = _selected.OrgUnitID;
                _quotaService.DeleteConfig(_selected.QuotaConfigID, _selected.OrgUnitTypeName, _user);
                LoadData();
                SelectConfigInList(orgUnitId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e) { ClearForm(); }
        private void btnClose_Click(object sender, EventArgs e) { Close(); }

        private int GetSelectedMasterType()
        {
            var item = cmbMasterType.SelectedItem as MasterTypeItem;
            return item == null ? 1 : item.OrgUnitType;
        }

        private string GetSelectedMasterText()
        {
            var item = cmbMasterType.SelectedItem as MasterTypeItem;
            return item == null ? "Sub Committee" : item.Text;
        }

        private void SelectConfigInList(int orgUnitId)
        {
            for (int i = 0; i < lstOrgUnits.Items.Count; i++)
            {
                var cfg = lstOrgUnits.Items[i] as FolderQuotaConfig;
                if (cfg == null || cfg.OrgUnitID != orgUnitId) continue;
                lstOrgUnits.SelectedIndex = i;
                return;
            }
        }

        private void SetMasterTargetLabel(int orgUnitType)
        {
            lblWarn.Text = "บันทึกลง: " + GetMasterTableName(orgUnitType);
        }

        private void SetMasterTargetLabel(string orgUnitTypeName)
        {
            lblWarn.Text = "บันทึกลง: " + GetMasterTableName(orgUnitTypeName);
        }

        private static string GetMasterTypeShortName(int orgUnitType)
        {
            switch (orgUnitType)
            {
                case 1: return "Sub";
                case 2: return "Div";
                case 3: return "Dep";
                default: return "-";
            }
        }

        private static string GetMasterTableName(int orgUnitType)
        {
            switch (orgUnitType)
            {
                case 1: return "EMS.m_SubCommittee";
                case 2: return "EMS.m_Division";
                case 3: return "EMS.m_Department";
                default: return "-";
            }
        }

        private static string GetMasterTableName(string orgUnitTypeName)
        {
            return GetMasterTableName(GetOrgUnitType(orgUnitTypeName));
        }

        private static int GetOrgUnitType(string orgUnitTypeName)
        {
            if (string.IsNullOrWhiteSpace(orgUnitTypeName)) return 0;
            if (orgUnitTypeName.IndexOf("Sub", StringComparison.OrdinalIgnoreCase) >= 0) return 1;
            if (orgUnitTypeName.IndexOf("Division", StringComparison.OrdinalIgnoreCase) >= 0) return 2;
            if (orgUnitTypeName.IndexOf("Department", StringComparison.OrdinalIgnoreCase) >= 0) return 3;
            return 0;
        }

        private sealed class MasterTypeItem
        {
            public MasterTypeItem(int orgUnitType, string text, string tableName)
            {
                OrgUnitType = orgUnitType;
                Text = text;
                TableName = tableName;
            }

            public int OrgUnitType { get; private set; }
            public string Text { get; private set; }
            public string TableName { get; private set; }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}

