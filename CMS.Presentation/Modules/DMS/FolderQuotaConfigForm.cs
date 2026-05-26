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
        private readonly IFolderQuotaService  _quotaService;
        private readonly IOrgStructureService _orgService;
        private readonly UserContext          _user;

        private List<FolderQuotaConfig> _configs = new List<FolderQuotaConfig>();
        private FolderQuotaConfig _selected;
        private OrgUnit           _selectedOrgUnit;

        public FolderQuotaConfigForm(IFolderQuotaService quotaService, IOrgStructureService orgService, UserContext user)
        {
            if (quotaService == null) throw new ArgumentNullException("quotaService");
            if (orgService   == null) throw new ArgumentNullException("orgService");
            if (user         == null) throw new ArgumentNullException("user");

            _quotaService = quotaService;
            _orgService   = orgService;
            _user         = user;

            InitializeComponent();
            LoadOrgTree();
            LoadGrid();
        }

        // ── Tree ──────────────────────────────────────────────────────────────
        private void LoadOrgTree()
        {
            var all = _orgService.GetHierarchy()
                .GroupBy(o => o.OrgUnitID)
                .Select(g => g.First())
                .ToList();

            tvOrgUnits.BeginUpdate();
            tvOrgUnits.Nodes.Clear();

            foreach (var sub in all.Where(o => o.OrgUnitType == 1).OrderBy(o => o.Name))
            {
                var subNode = new TreeNode(sub.Name) { Tag = sub };
                subNode.NodeFont = new Font(tvOrgUnits.Font, FontStyle.Bold);

                foreach (var div in all.Where(o => o.OrgUnitType == 2 && o.ParentOrgUnitID == sub.OrgUnitID).OrderBy(o => o.Name))
                {
                    var divNode = new TreeNode(div.Name) { Tag = div };

                    foreach (var dep in all.Where(o => o.OrgUnitType == 3 && o.ParentOrgUnitID == div.OrgUnitID).OrderBy(o => o.Name))
                        divNode.Nodes.Add(new TreeNode(dep.Name) { Tag = dep });

                    subNode.Nodes.Add(divNode);
                }
                tvOrgUnits.Nodes.Add(subNode);
            }

            tvOrgUnits.ExpandAll();
            tvOrgUnits.EndUpdate();
        }

        private void tvOrgUnits_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _selectedOrgUnit = e.Node != null ? e.Node.Tag as OrgUnit : null;
            if (_selectedOrgUnit != null)
            {
                string tier = _selectedOrgUnit.OrgUnitType == 1 ? "Sub"
                            : _selectedOrgUnit.OrgUnitType == 2 ? "Div" : "Dep";
                lblSelectedOrg.Text      = string.Format("[{0}] {1}", tier, _selectedOrgUnit.Name);
                lblSelectedOrg.ForeColor = Color.FromArgb(26, 58, 108);
                lblSelectedOrg.Font      = new Font(lblSelectedOrg.Font, FontStyle.Bold);
            }
            else
            {
                SetOrgPlaceholder();
            }
        }

        private TreeNode FindNode(TreeNodeCollection nodes, int orgUnitId)
        {
            foreach (TreeNode n in nodes)
            {
                var o = n.Tag as OrgUnit;
                if (o != null && o.OrgUnitID == orgUnitId) return n;
                var child = FindNode(n.Nodes, orgUnitId);
                if (child != null) return child;
            }
            return null;
        }

        // ── Grid ──────────────────────────────────────────────────────────────
        private void LoadGrid()
        {
            _configs = _quotaService.GetAllConfigs().ToList();
            dgvConfigs.Rows.Clear();
            foreach (var c in _configs)
            {
                int row = dgvConfigs.Rows.Add(
                    c.QuotaConfigID,
                    c.OrgUnitTypeName,
                    c.OrgUnitName,
                    string.Format("{0:F1} GB", c.QuotaGB),
                    c.WarnThresholdPct + "%",
                    c.CriticalThresholdPct + "%",
                    c.UpdatedDate.HasValue
                        ? c.UpdatedDate.Value.ToString("dd/MM/yyyy")
                        : c.CreatedDate.ToString("dd/MM/yyyy")
                );
                dgvConfigs.Rows[row].Tag = c;
            }
            lblStatus.Text = string.Format("ทั้งหมด {0} รายการ", _configs.Count);
        }

        private void dgvConfigs_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvConfigs.SelectedRows.Count == 0) return;
            _selected = dgvConfigs.SelectedRows[0].Tag as FolderQuotaConfig;
            if (_selected == null) return;

            var node = FindNode(tvOrgUnits.Nodes, _selected.OrgUnitID);
            if (node != null) { tvOrgUnits.SelectedNode = node; node.EnsureVisible(); }

            nudQuotaGB.Value     = (decimal)Math.Round(_selected.QuotaGB, 1);
            nudWarnPct.Value     = _selected.WarnThresholdPct;
            nudCriticalPct.Value = _selected.CriticalThresholdPct;
            btnSave.Text = "แก้ไข";
        }

        // ── Form actions ─────────────────────────────────────────────────────
        private void ClearForm()
        {
            _selected        = null;
            _selectedOrgUnit = null;
            tvOrgUnits.SelectedNode = null;
            SetOrgPlaceholder();
            nudQuotaGB.Value     = 40;
            nudWarnPct.Value     = 80;
            nudCriticalPct.Value = 95;
            btnSave.Text = "บันทึก";
        }

        private void SetOrgPlaceholder()
        {
            lblSelectedOrg.Text      = "กรุณาเลือก OrgUnit จากรายการด้านซ้าย";
            lblSelectedOrg.ForeColor = Color.Gray;
            lblSelectedOrg.Font      = new Font(lblSelectedOrg.Font, FontStyle.Italic);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_selectedOrgUnit == null)
            {
                MessageBox.Show("กรุณาเลือก OrgUnit จากรายการด้านซ้าย", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (nudCriticalPct.Value <= nudWarnPct.Value)
            {
                MessageBox.Show("% Critical ต้องมากกว่า % Warning", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var config = _selected ?? new FolderQuotaConfig();
            config.OrgUnitID            = _selectedOrgUnit.OrgUnitID;
            config.QuotaBytes           = (long)(nudQuotaGB.Value * 1073741824m);
            config.WarnThresholdPct     = (byte)nudWarnPct.Value;
            config.CriticalThresholdPct = (byte)nudCriticalPct.Value;
            config.IsActive             = true;
            config.CreatedBy            = _user.UserID;
            config.UpdatedBy            = _user.UserID;

            try
            {
                _quotaService.SaveConfig(config, _user);
                MessageBox.Show("บันทึกสำเร็จ", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selected == null) { MessageBox.Show("กรุณาเลือกรายการที่ต้องการลบ"); return; }
            if (MessageBox.Show("ต้องการลบ quota config นี้?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                _quotaService.DeleteConfig(_selected.QuotaConfigID, _user);
                ClearForm();
                LoadGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e) { ClearForm(); }
        private void btnClose_Click(object sender, EventArgs e) { Close(); }
    }
}
