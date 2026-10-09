using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.DMS
{
    /// <summary>
    /// Admin screen: name the file-download approvers ('D') or internal file-share approvers ('S') per HR division / department.
    /// Left grid lists every division with its departments and the configured approver names;
    /// right side edits the list (order = order shown in the dialog) of the selected org.
    /// </summary>
    public partial class DownloadApproverConfigForm : Form
    {
        private readonly IDownloadApproverConfigController _configController;
        private readonly IAuthorityOrgLookupController _orgLookupController;
        private readonly IUserAccountLookupController _userLookupController;
        private readonly UserContext _user;

        private string _currentApprovalType = DownloadApproverConfig.TypeDownload;
        private List<OrgRow> _orgs = new List<OrgRow>();
        private Dictionary<string, List<DownloadApproverConfig>> _configsByOrg =
            new Dictionary<string, List<DownloadApproverConfig>>(StringComparer.OrdinalIgnoreCase);
        private List<UserAccount> _activeUsers = new List<UserAccount>();
        private List<UserAccount> _pickAll = new List<UserAccount>();   // ทั้งหมด (คนในหน่วยงานขึ้นก่อน)
        private List<UserAccount> _pickList = new List<UserAccount>();  // ที่แสดงใน dropdown ตอนนี้ (หลังค้นหา)

        private OrgRow _selectedOrg;
        private List<ApproverItem> _editing = new List<ApproverItem>();
        private bool _dirty;
        private bool _suppressSelection;
        private Font _boldFont;

        private sealed class OrgRow
        {
            public string ScopeType;
            public string Code;
            public string Name;
            public string Key { get { return ScopeType + ":" + Code; } }
            public bool IsDivision { get { return ScopeType == DownloadApproverConfig.ScopeDivision; } }
        }

        private sealed class ApproverItem
        {
            public int UserID;
            public string Name;
            public string Position;
            public string Level;
            public string Department;
        }

        public DownloadApproverConfigForm(
            IDownloadApproverConfigController configController,
            IAuthorityOrgLookupController orgLookupController,
            IUserAccountLookupController userLookupController,
            UserContext user)
        {
            if (configController     == null) throw new ArgumentNullException("configController");
            if (orgLookupController  == null) throw new ArgumentNullException("orgLookupController");
            if (userLookupController == null) throw new ArgumentNullException("userLookupController");
            if (user                 == null) throw new ArgumentNullException("user");

            _configController     = configController;
            _orgLookupController  = orgLookupController;
            _userLookupController = userLookupController;
            _user                 = user;

            InitializeComponent();
            Load  += (s, e) => LoadAll();
            Shown += (s, e) => ApplySplitLayout();
        }

        // ── Loading ──────────────────────────────────────────────────────────

        private void LoadAll()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                _activeUsers = _userLookupController.GetAllUsers()
                    .Where(u => u.IsActive)
                    .OrderBy(u => DisplayNameOf(u))
                    .ToList();

                BuildOrgRows();
                UpdateUiMode();
                LoadConfigs();
                BindOrgGrid();
                SetEditorEnabled(false);
                SetStatus(string.Format("ฝ่าย/แผนกทั้งหมด {0} รายการ — ตั้งผู้อนุมัติแล้ว {1} รายการ",
                    _orgs.Count, _configsByOrg.Count), Color.FromArgb(31, 56, 100));
            }
            catch (Exception ex)
            {
                SetStatus("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, Color.Red);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BuildOrgRows()
        {
            _orgs = new List<OrgRow>();
            var divisions   = _orgLookupController.GetDivisions().Where(d => d.IsActive).OrderBy(d => d.DivisionName).ToList();
            var departments = _orgLookupController.GetDepartments().Where(d => d.IsActive).ToList();

            foreach (var div in divisions)
            {
                _orgs.Add(new OrgRow
                {
                    ScopeType = DownloadApproverConfig.ScopeDivision,
                    Code      = (div.DivisionCode ?? "").Trim(),
                    Name      = div.DivisionName
                });
                foreach (var dep in departments.Where(d => d.DivisionID == div.DivisionID).OrderBy(d => d.DisplayName))
                    _orgs.Add(DepartmentRow(dep));
            }

            // แผนกที่ไม่มีฝ่าย (หรือฝ่ายถูกปิด) แสดงต่อท้าย
            var divisionIds = new HashSet<int>(divisions.Select(d => d.DivisionID));
            foreach (var dep in departments.Where(d => !divisionIds.Contains(d.DivisionID)).OrderBy(d => d.DisplayName))
                _orgs.Add(DepartmentRow(dep));

            _orgs.RemoveAll(o => string.IsNullOrEmpty(o.Code));
        }

        private static OrgRow DepartmentRow(HrDepartment dep)
        {
            return new OrgRow
            {
                ScopeType = DownloadApproverConfig.ScopeDepartment,
                Code      = (dep.DepartmentCode ?? "").Trim(),
                Name      = dep.DisplayName
            };
        }

        private void LoadConfigs()
        {
            _configsByOrg = _configController.GetAll(_currentApprovalType)
                .GroupBy(c => c.ScopeType + ":" + c.OrgCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderBy(c => c.SortOrder).ToList(), StringComparer.OrdinalIgnoreCase);
        }

        private void BindOrgGrid()
        {
            string keepKey = _selectedOrg != null ? _selectedOrg.Key : null;
            string search  = (txtSearch.Text ?? "").Trim();
            string fallbackText = _currentApprovalType == DownloadApproverConfig.TypeShare
                ? "— ยังไม่ได้ตั้ง (ใช้ตามสิทธิ์ Folder) —"
                : "— ยังไม่ได้ตั้ง (ใช้ตาม Level) —";

            _suppressSelection = true;
            dgvOrgs.Rows.Clear();
            foreach (var org in _orgs)
            {
                string approvers = ApproverNames(org);
                if (chkConfiguredOnly.Checked && approvers.Length == 0) continue;
                if (search.Length > 0
                    && org.Code.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0
                    && (org.Name ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0
                    && approvers.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                int idx = dgvOrgs.Rows.Add(
                    org.IsDivision ? "ฝ่าย" : "แผนก",
                    org.Code,
                    org.IsDivision ? org.Name : "    └ " + org.Name,
                    approvers.Length > 0 ? approvers : fallbackText);
                dgvOrgs.Rows[idx].Tag = org;
            }
            dgvOrgs.ClearSelection();
            _suppressSelection = false;

            if (keepKey != null)
            {
                foreach (DataGridViewRow row in dgvOrgs.Rows)
                {
                    var org = row.Tag as OrgRow;
                    if (org != null && org.Key == keepKey)
                    {
                        _suppressSelection = true;
                        row.Selected = true;
                        dgvOrgs.CurrentCell = row.Cells[0];
                        _suppressSelection = false;
                        break;
                    }
                }
            }
        }

        private string ApproverNames(OrgRow org)
        {
            List<DownloadApproverConfig> list;
            if (!_configsByOrg.TryGetValue(org.Key, out list) || list.Count == 0)
                return "";
            return string.Join(", ", list.Select(c => c.ApproverName ?? ("UserID " + c.ApproverUserID)));
        }

        private void UpdateUiMode()
        {
            bool isShare = _currentApprovalType == DownloadApproverConfig.TypeShare;
            if (isShare)
            {
                Text = "ตั้งค่าผู้อนุมัติแชร์ไฟล์ภายใน";
                lblTitle.Text = "🔗 ตั้งค่าผู้อนุมัติแชร์ไฟล์ภายใน — กำหนดรายชื่อผู้อนุมัติตามแผนก / ฝ่าย";
                colOrgApprovers.HeaderText = "ผู้อนุมัติแชร์ไฟล์";
                lblStatus.Text = "ไฟล์ในโฟลเดอร์ของแผนก → ส่งคำขอให้ผู้อนุมัติแชร์ของแผนก ต่อด้วยของฝ่าย  |  ไม่ได้ตั้ง → ใช้กฎสิทธิ์ Folder ตามปกติ";

                btnModeShare.BackColor    = Color.FromArgb(31, 56, 100);
                btnModeShare.ForeColor    = Color.White;
                btnModeDownload.BackColor = Color.FromArgb(240, 244, 250);
                btnModeDownload.ForeColor = Color.FromArgb(60, 60, 60);
            }
            else
            {
                Text = "ตั้งค่าผู้อนุมัติดาวน์โหลดไฟล์";
                lblTitle.Text = "⬇ ตั้งค่าผู้อนุมัติดาวน์โหลดไฟล์ — กำหนดรายชื่อผู้อนุมัติตามแผนก / ฝ่าย";
                colOrgApprovers.HeaderText = "ผู้อนุมัติดาวน์โหลด";
                lblStatus.Text = "ไฟล์ในโฟลเดอร์ของแผนก → แสดงผู้อนุมัติของแผนก ต่อด้วยของฝ่าย  |  ไม่ได้ตั้ง → ใช้คนที่ Level สูงกว่าผู้ขอ + Admin";

                btnModeDownload.BackColor = Color.FromArgb(31, 56, 100);
                btnModeDownload.ForeColor = Color.White;
                btnModeShare.BackColor    = Color.FromArgb(240, 244, 250);
                btnModeShare.ForeColor    = Color.FromArgb(60, 60, 60);
            }
        }

        // ── Editor ───────────────────────────────────────────────────────────

        private void SelectOrg(OrgRow org)
        {
            _selectedOrg = org;
            _dirty = false;
            _editing = new List<ApproverItem>();

            if (org == null)
            {
                lblSelectedOrg.Text = "เลือกฝ่าย / แผนก ทางซ้าย";
                BindApprovers();
                SetEditorEnabled(false);
                return;
            }

            lblSelectedOrg.Text = string.Format("{0}: {1} ({2})", org.IsDivision ? "ฝ่าย" : "แผนก", org.Name, org.Code);

            List<DownloadApproverConfig> list;
            if (_configsByOrg.TryGetValue(org.Key, out list))
            {
                foreach (var cfg in list)
                {
                    var u = _activeUsers.FirstOrDefault(x => x.UserID == cfg.ApproverUserID);
                    _editing.Add(u != null ? ToItem(u) : new ApproverItem
                    {
                        UserID     = cfg.ApproverUserID,
                        Name       = (cfg.ApproverName ?? ("UserID " + cfg.ApproverUserID)) + " (ปิดใช้งาน)",
                        Position   = cfg.PositionName,
                        Level      = cfg.LevelCode,
                        Department = cfg.DepartmentName
                    });
                }
            }

            BindApprovers();
            BindUserPicker(org);
            SetEditorEnabled(true);
        }

        private void BindApprovers()
        {
            dgvApprovers.Rows.Clear();
            for (int i = 0; i < _editing.Count; i++)
            {
                var a = _editing[i];
                dgvApprovers.Rows.Add(i + 1, a.Name, a.Position, a.Level, a.Department);
            }
        }

        /// <summary>คนในฝ่าย/แผนกที่เลือกขึ้นก่อน แล้วตามด้วยคนอื่นทั้งหมด</summary>
        private void BindUserPicker(OrgRow org)
        {
            Func<UserAccount, bool> isMember = u => org.IsDivision
                ? SameCode(u.DivisionCode, org.Code)
                : SameCode(u.DepartmentCode, org.Code) || SameCode(u.DepartmentName, org.Name);

            _pickAll = _activeUsers.Where(isMember)
                .Concat(_activeUsers.Where(u => !isMember(u)))
                .ToList();

            FillPicker(_pickAll);
            cboUser.Text = "";
        }

        private void FillPicker(List<UserAccount> users)
        {
            _pickList = users;
            cboUser.BeginUpdate();
            cboUser.Items.Clear();
            foreach (var u in _pickList)
                cboUser.Items.Add(PickerLabel(u));
            cboUser.EndUpdate();
        }

        private static bool MatchesUser(UserAccount u, string query)
        {
            return CMS.Presentation.Helpers.UserDisplayHelper.Matches(u, query);
        }

        private void SetEditorEnabled(bool enabled)
        {
            cboUser.Enabled = enabled;
            btnAdd.Enabled = enabled;
            btnMoveUp.Enabled = enabled;
            btnMoveDown.Enabled = enabled;
            btnRemove.Enabled = enabled;
            btnSave.Enabled = enabled;
        }

        private int SelectedApproverIndex()
        {
            return dgvApprovers.CurrentRow != null ? dgvApprovers.CurrentRow.Index : -1;
        }

        private void SelectApproverRow(int index)
        {
            if (index < 0 || index >= dgvApprovers.Rows.Count) return;
            dgvApprovers.ClearSelection();
            dgvApprovers.Rows[index].Selected = true;
            dgvApprovers.CurrentCell = dgvApprovers.Rows[index].Cells[0];
        }

        private void MarkDirty()
        {
            _dirty = true;
            SetStatus("มีการแก้ไขที่ยังไม่บันทึก — กด 💾 บันทึก", Color.DarkOrange);
        }

        private bool ConfirmDiscardChanges()
        {
            if (!_dirty) return true;
            return MessageBox.Show("รายชื่อผู้อนุมัติที่แก้ไขยังไม่ได้บันทึก ต้องการยกเลิกการแก้ไขหรือไม่?",
                "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        // ── Event handlers ───────────────────────────────────────────────────

        private void btnModeDownload_Click(object sender, EventArgs e)
        {
            SwitchApprovalType(DownloadApproverConfig.TypeDownload);
        }

        private void btnModeShare_Click(object sender, EventArgs e)
        {
            SwitchApprovalType(DownloadApproverConfig.TypeShare);
        }

        private void SwitchApprovalType(string newType)
        {
            if (newType == _currentApprovalType) return;

            if (!ConfirmDiscardChanges())
                return;

            _dirty = false;
            _currentApprovalType = newType;
            UpdateUiMode();
            LoadConfigs();
            BindOrgGrid();
            SelectOrg(_selectedOrg != null ? _orgs.FirstOrDefault(o => o.Key == _selectedOrg.Key) : null);
        }

        private void dgvOrgs_SelectionChanged(object sender, EventArgs e)
        {
            if (_suppressSelection || dgvOrgs.CurrentRow == null) return;

            var org = dgvOrgs.CurrentRow.Tag as OrgRow;
            if (_selectedOrg != null && org != null && org.Key == _selectedOrg.Key) return;

            if (!ConfirmDiscardChanges())
            {
                // กลับไปแถวเดิม
                BindOrgGrid();
                return;
            }
            SelectOrg(org);
        }

        private void dgvOrgs_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var org = dgvOrgs.Rows[e.RowIndex].Tag as OrgRow;
            if (org == null) return;

            if (org.IsDivision)
            {
                if (_boldFont == null) _boldFont = new Font(dgvOrgs.Font, FontStyle.Bold);
                e.CellStyle.Font = _boldFont;
            }

            if (e.ColumnIndex == colOrgApprovers.Index && !_configsByOrg.ContainsKey(org.Key))
                e.CellStyle.ForeColor = Color.Gray;
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            BindOrgGrid();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscardChanges()) return;
            _dirty = false;
            var keep = _selectedOrg;
            LoadAll();
            SelectOrg(keep != null ? _orgs.FirstOrDefault(o => o.Key == keep.Key) : null);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscardChanges()) return;
            _dirty = false;
            Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (_selectedOrg == null) return;

            int idx = cboUser.SelectedIndex;
            if (idx < 0)
                idx = cboUser.Items.IndexOf(cboUser.Text);
            if (idx < 0 && _pickList.Count == 1)
                idx = 0;   // ค้นหาแล้วเหลือคนเดียว → เพิ่มคนนั้นเลย
            if (idx < 0 || idx >= _pickList.Count)
            {
                SetStatus(_pickList.Count > 1
                    ? "พบหลายคน — กรุณาเลือกจากรายการ"
                    : "ไม่พบผู้ใช้ — ค้นหาด้วยชื่อ หรือ AD username", Color.OrangeRed);
                cboUser.Focus();
                return;
            }

            var user = _pickList[idx];
            if (_editing.Any(a => a.UserID == user.UserID))
            {
                SetStatus(DisplayNameOf(user) + " อยู่ในรายชื่อแล้ว", Color.OrangeRed);
                return;
            }

            _editing.Add(ToItem(user));
            BindApprovers();
            SelectApproverRow(_editing.Count - 1);
            FillPicker(_pickAll);
            cboUser.Text = "";
            MarkDirty();
        }

        private void cboUser_TextUpdate(object sender, EventArgs e)
        {
            string query = cboUser.Text ?? "";
            var matches = query.Trim().Length == 0
                ? _pickAll
                : _pickAll.Where(u => MatchesUser(u, query.Trim())).ToList();

            FillPicker(matches);

            // เปิด dropdown แล้วคืนข้อความที่พิมพ์ (การเปิด dropdown อาจแทนที่ข้อความ)
            if (matches.Count > 0 && query.Length > 0)
                cboUser.DroppedDown = true;
            cboUser.Text = query;
            cboUser.SelectionStart = query.Length;
            cboUser.SelectionLength = 0;
            Cursor.Current = Cursors.Default;
        }

        private void cboUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            btnAdd_Click(sender, EventArgs.Empty);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            int i = SelectedApproverIndex();
            if (i < 0 || i >= _editing.Count) return;
            _editing.RemoveAt(i);
            BindApprovers();
            SelectApproverRow(Math.Min(i, _editing.Count - 1));
            MarkDirty();
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            int i = SelectedApproverIndex();
            if (i <= 0 || i >= _editing.Count) return;
            var item = _editing[i];
            _editing.RemoveAt(i);
            _editing.Insert(i - 1, item);
            BindApprovers();
            SelectApproverRow(i - 1);
            MarkDirty();
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            int i = SelectedApproverIndex();
            if (i < 0 || i >= _editing.Count - 1) return;
            var item = _editing[i];
            _editing.RemoveAt(i);
            _editing.Insert(i + 1, item);
            BindApprovers();
            SelectApproverRow(i + 1);
            MarkDirty();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_selectedOrg == null) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                _configController.SaveForOrg(_selectedOrg.ScopeType, _selectedOrg.Code,
                    _editing.Select(a => a.UserID).ToList(), _user, _currentApprovalType);

                _dirty = false;
                LoadConfigs();
                BindOrgGrid();
                SetStatus(string.Format("บันทึกผู้อนุมัติของ {0} เรียบร้อย ({1} คน)",
                    _selectedOrg.Name, _editing.Count), Color.Green);
            }
            catch (Exception ex)
            {
                SetStatus("บันทึกไม่สำเร็จ: " + ex.Message, Color.Red);
                MessageBox.Show("บันทึกไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !ConfirmDiscardChanges())
                e.Cancel = true;
            base.OnFormClosing(e);
        }

        // ── Layout ───────────────────────────────────────────────────────────

        private const int EditorPanelWidth = 640;
        private const int OrgPanelMinWidth = 320;

        private void splitMain_SizeChanged(object sender, EventArgs e)
        {
            ApplySplitLayout();
        }

        /// <summary>
        /// ฝั่งขวา (แก้ไขรายชื่อ) กว้างคงที่ ~640px ฝั่งซ้ายยืดตามจอ; จอแคบ → แบ่งครึ่ง.
        /// คำนวณใหม่ทุกครั้งที่ขนาดเปลี่ยน เพราะ MDI child ถูก maximize หลัง Load.
        /// </summary>
        private void ApplySplitLayout()
        {
            int width = splitMain.Width;
            if (width <= 0) return;

            int distance = width - EditorPanelWidth - splitMain.SplitterWidth;
            if (distance < OrgPanelMinWidth)
                distance = width / 2;

            distance = Math.Max(splitMain.Panel1MinSize,
                       Math.Min(distance, width - splitMain.Panel2MinSize - splitMain.SplitterWidth));
            if (distance > 0 && splitMain.SplitterDistance != distance)
                splitMain.SplitterDistance = distance;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static ApproverItem ToItem(UserAccount u)
        {
            return new ApproverItem
            {
                UserID     = u.UserID,
                Name       = DisplayNameOf(u),
                Position   = u.PositionName,
                Level      = string.IsNullOrWhiteSpace(u.LevelCode) ? null : u.LevelCode.Trim(),
                Department = u.DepartmentName
            };
        }

        private static string PickerLabel(UserAccount u)
        {
            string level = string.IsNullOrWhiteSpace(u.LevelCode) ? "-" : u.LevelCode.Trim();
            return string.Format("{0}  ({1})  [{2} · {3}]",
                DisplayNameOf(u), u.ADUserName, level, u.DepartmentName ?? "-");
        }

        private static string DisplayNameOf(UserAccount u)
        {
            return !string.IsNullOrEmpty(u.DisplayName) ? u.DisplayName : u.ADUserName;
        }

        private static bool SameCode(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }
    }
}
