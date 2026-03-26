using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Presentation.Presenters;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// MOD-004: Folder Settings Dialog — Tabs: ทั่วไป / สิทธิ์ / Workflow / ประวัติ
    /// </summary>
    public partial class PermissionForm : Form, IPermissionView
    {
        private PermissionPresenter _presenter;
        private Folder _folder;
        private List<FolderPermission> _aclEntries = new List<FolderPermission>();
        private List<FolderPermission> _allAclEntries = new List<FolderPermission>(); // includes revoked
        private bool _showAllEntries = false;
        private IUserAccountRepository _userAccountRepository;
        private List<UserAccount> _userList = new List<UserAccount>();
        private IApprovalService _approvalService;
        private IFolderRepository _folderRepository;
        private Dictionary<int, string> _userDisplayMap = new Dictionary<int, string>();
        private CMS.Services.Interfaces.IEmployeeApiService _employeeApiService;
        private CMS.Services.Interfaces.IOrgStructureService _orgStructureService;
        private Dictionary<int, string> _departmentNameMap = new Dictionary<int, string>(); // DepartmentId -> Name (Employee API)
        private Dictionary<int, string> _divisionNameMap = new Dictionary<int, string>();   // DivisionId -> Name (Employee API)
        private Dictionary<int, string> _orgUnitNameMap = new Dictionary<int, string>();    // OrgUnitID -> Name (DB)

        // ── Constructor ──────────────────────────────────────────────────────

        public PermissionForm(
            IPermissionService  permissionService,
            IApprovalService    approvalService,
            IFolderService      folderService,
            IPermissionRepository permissionRepo,
            UserContext         user,
            Folder              folder,
            IUserAccountRepository userAccountRepository = null,
            IFolderRepository  folderRepository = null,
            CMS.Services.Interfaces.IEmployeeApiService employeeApiService = null,
            CMS.Services.Interfaces.IOrgStructureService orgStructureService = null,
            IFileDocumentRepository fileDocumentRepository = null)
        {
            if (permissionService == null) throw new ArgumentNullException("permissionService");
            if (approvalService   == null) throw new ArgumentNullException("approvalService");
            if (folderService     == null) throw new ArgumentNullException("folderService");
            if (permissionRepo    == null) throw new ArgumentNullException("permissionRepo");
            if (user              == null) throw new ArgumentNullException("user");

            _folder = folder;
            _userAccountRepository = userAccountRepository;
            _userList = new List<UserAccount>();
            _approvalService = approvalService;
            _folderRepository = folderRepository;
            _employeeApiService = employeeApiService;
            _orgStructureService = orgStructureService;

            // Pre-load user display map
            if (_userAccountRepository != null)
            {
                try
                {
                    foreach (var u in _userAccountRepository.GetAll())
                        _userDisplayMap[u.UserID] = string.IsNullOrEmpty(u.DisplayName) ? u.ADUserName : u.DisplayName;
                }
                catch { }
            }

            // Pre-load department and division names from API
            if (_employeeApiService != null)
            {
                try
                {
                    foreach (var d in _employeeApiService.GetDepartments())
                        _departmentNameMap[d.DepartmentId] = d.DisplayName ?? d.NameTh ?? "—";
                    foreach (var v in _employeeApiService.GetDivisions())
                        _divisionNameMap[v.DivisionId] = v.DisplayName ?? v.NameTh ?? "—";
                }
                catch { }
            }

            // Pre-load OrgUnit name map keyed by OrgUnitID (used for display in ACL list)
            if (_orgStructureService != null)
            {
                try
                {
                    foreach (var u in _orgStructureService.GetHierarchy())
                        _orgUnitNameMap[u.OrgUnitID] = u.OrgUnitNameTH ?? u.OrgUnitNameEN ?? u.OrgUnitCode ?? u.OrgUnitID.ToString();
                }
                catch { }
            }

            InitializeComponent();

            _presenter = new PermissionPresenter(
                permissionService, approvalService, folderService, permissionRepo, this, user,
                fileDocumentRepository);
        }

        // ── Form load ────────────────────────────────────────────────────────

        private void PermissionForm_Load(object sender, EventArgs e)
        {
            if (_folder != null)
            {
                // Header title
                string badge = ConfidentialityBadge(_folder.ConfidentialityLevel);
                lblTitle.Text = string.Format("⚙ ตั้งค่า Folder: {0} ({1})", _folder.Name, badge);

                // ── Tab ทั่วไป ────────────────────────────────────────────────
                int lvl = _folder.ConfidentialityLevel;
                cboConfLevel.SelectedIndex = (lvl >= 1 && lvl <= 4) ? lvl - 1 : 0;

                // Color swatch + text
                pnlColorSwatch.BackColor = ConfidentialityColor(lvl);
                lblColorText.Text        = ConfidentialityColorLabel(lvl);

                txtFolderName.Text   = _folder.FolderName ?? string.Empty;
                txtPhysicalPath.Text = _folder.PhysicalPath ?? string.Empty;

                // Size + quota
                string sizeStr = FormatBytes(_folder.FolderSizeBytes);
                if (_folder.QuotaBytes.HasValue && _folder.QuotaBytes.Value > 0)
                    lblSizeValue.Text = string.Format("{0} (จาก {1} quota)", sizeStr, FormatBytes(_folder.QuotaBytes.Value));
                else
                    lblSizeValue.Text = sizeStr;

                chkInheritPerm.Checked = _folder.InheritPermission;

                // ── Tab สิทธิ์ ────────────────────────────────────────────────
                _presenter.LoadAclEntries(_folder);

                // ── Tab Workflow — approvers ──────────────────────────────────
                LoadApprovers();

                // ── Tab ประวัติ (sample rows — replace with real audit service call if available)
                PopulateHistoryTab();
            }

            tabControl.SelectedTab = tabGeneral;
        }

        private void LoadApprovers()
        {
            lvApprovers.Items.Clear();
            if (_folder == null || _approvalService == null) return;

            try
            {
                // ดึงทุก request ของ folder นี้ (ทุก status)
                var allRequests = _approvalService.GetAllApprovalsByFolder(_folder.FolderId)
                    .OrderByDescending(r => r.RequestedDate).ToList();

                // กรองเฉพาะที่อนุมัติแล้ว (RequestStatus = 2)
                var approvedRequests = allRequests.Where(r => r.RequestStatus == 2).ToList();

                if (approvedRequests.Count == 0)
                {
                    var empty = new ListViewItem("—");
                    empty.SubItems.Add(allRequests.Count > 0
                        ? string.Format("มีคำขอ {0} รายการ แต่ยังไม่มีที่อนุมัติแล้ว", allRequests.Count)
                        : "ไม่มีประวัติการอนุมัติ");
                    empty.SubItems.Add(""); empty.SubItems.Add(""); empty.SubItems.Add("");
                    empty.ForeColor = Color.Gray;
                    lvApprovers.Items.Add(empty);
                    return;
                }

                // Build user lookup map
                var userMap = new Dictionary<int, string>();
                if (_userAccountRepository != null)
                {
                    try
                    {
                        foreach (var u in _userAccountRepository.GetAll())
                            userMap[u.UserID] = string.IsNullOrEmpty(u.DisplayName) ? u.ADUserName : u.DisplayName;
                    }
                    catch { }
                }

                foreach (var req in approvedRequests)                {
                    // Header row per request
                    string resolvedDate = req.ResolvedDate.HasValue
                        ? req.ResolvedDate.Value.ToString("dd/MM/yy HH:mm")
                        : req.RequestedDate.ToString("dd/MM/yy HH:mm");

                    var header = new ListViewItem(string.Empty);
                    header.SubItems.Add(string.Format("✅ #{0}  {1}  ({2})",
                        req.RequestID, req.Subject ?? req.RequestType, resolvedDate));
                    header.SubItems.Add(""); header.SubItems.Add(""); header.SubItems.Add("");
                    header.BackColor = Color.FromArgb(230, 245, 230);
                    header.ForeColor = Color.FromArgb(0, 100, 0);
                    header.Font = new Font(lvApprovers.Font, FontStyle.Bold);
                    lvApprovers.Items.Add(header);

                    // Steps ของ request นี้
                    var steps = _approvalService.GetApprovalSteps(req.RequestID)
                        .OrderBy(s => s.StepOrder).ToList();

                    foreach (var step in steps)
                    {
                        string approverName;
                        if (!_userDisplayMap.TryGetValue(step.ApproverID, out approverName))
                            approverName = string.Format("UserID: {0}", step.ApproverID);
                        string actionDate = step.ActionDate.HasValue
                            ? step.ActionDate.Value.ToString("dd/MM/yy HH:mm")
                            : "—";

                        var item = new ListViewItem(step.StepOrder.ToString());
                        item.SubItems.Add(approverName);
                        item.SubItems.Add(StepStatusLabel(step.StepStatus));
                        item.SubItems.Add(actionDate);
                        item.SubItems.Add(step.ActionRemark ?? "—");

                        switch (step.StepStatus)
                        {
                            case 2: item.ForeColor = Color.Green; break;
                            case 3: item.ForeColor = Color.Crimson; break;
                            case 4: item.ForeColor = Color.DarkOrange; break;
                            default: item.ForeColor = Color.RoyalBlue; break;
                        }

                        lvApprovers.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                var errItem = new ListViewItem("!");
                errItem.SubItems.Add("โหลดข้อมูลไม่สำเร็จ: " + ex.Message);
                errItem.SubItems.Add(""); errItem.SubItems.Add(""); errItem.SubItems.Add("");
                errItem.ForeColor = Color.Red;
                lvApprovers.Items.Add(errItem);
            }
        }

        private static string StepStatusLabel(int status)
        {
            switch (status)
            {
                case 1: return "⏳ รออนุมัติ";
                case 2: return "✅ อนุมัติแล้ว";
                case 3: return "❌ ปฏิเสธ";
                case 4: return "↗ โอนสิทธิ์";
                default: return status.ToString();
            }
        }

        private void PopulateHistoryTab()
        {
            historyListView.Items.Clear();
            // Placeholder rows — in production wire to IAuditService.GetFolderHistory(_folder.FolderId)
            if (_folder == null) return;
            var rows = new[]
            {
                new { Date = _folder.UpdatedDate ?? _folder.CreatedDate, User = "—", Action = "UPDATE", Detail = string.Format("Folder: {0}", _folder.FolderName) },
                new { Date = _folder.CreatedDate, User = "—", Action = "CREATE", Detail = "สร้าง Folder ใหม่" }
            };
            foreach (var r in rows)
            {
                var item = new System.Windows.Forms.ListViewItem(r.Date.ToLocalTime().ToString("dd/MM/yy HH:mm"));
                item.SubItems.Add(r.User);
                item.SubItems.Add(r.Action);
                item.SubItems.Add(r.Detail);
                historyListView.Items.Add(item);
            }
        }

        // ── IPermissionView ──────────────────────────────────────────────────

        public void ShowAclEntries(IEnumerable<FolderPermission> entries)
        {
            // Store all entries (including revoked) for "show all" toggle
            _allAclEntries = entries == null ? new List<FolderPermission>() : entries.ToList();
            // Active = not revoked AND not expired
            _aclEntries = _allAclEntries
                .Where(e => e.RevokedDate == null
                         && !(e.ExpiryDate.HasValue && e.ExpiryDate.Value.Date < DateTime.Today))
                .ToList();

            RenderAclList();
            EnsureShowAllCheckbox();
        }

        private void RenderAclList()
        {
            aclListView.Items.Clear();
            var toShow = _showAllEntries ? _allAclEntries : _aclEntries;

            foreach (var entry in toShow)
            {
                bool isDeny    = entry.IsDeny;
                bool isRevoked = entry.RevokedDate.HasValue;

                string principalName;
                if (entry.PrincipalType == 1)
                {
                    // User
                    if (!_userDisplayMap.TryGetValue(entry.PrincipalId, out principalName))
                        principalName = string.Format("UserID: {0}", entry.PrincipalId);
                }
                else if (entry.PrincipalType == 3)
                {
                    // Department — PrincipalId is OrgUnitID
                    if (!_orgUnitNameMap.TryGetValue(entry.PrincipalId, out principalName))
                        principalName = string.Format("DeptID: {0}", entry.PrincipalId);
                }
                else if (entry.PrincipalType == 4)
                {
                    // Division — PrincipalId is OrgUnitID
                    if (!_orgUnitNameMap.TryGetValue(entry.PrincipalId, out principalName))
                        principalName = string.Format("DivID: {0}", entry.PrincipalId);
                }
                else
                {
                    principalName = string.Format("{0} #{1}", PrincipalTypeLabel(entry.PrincipalType), entry.PrincipalId);
                }

                var item = new ListViewItem(isDeny ? "⊘ " + principalName : principalName);
                item.SubItems.Add(PrincipalTypeLabel(entry.PrincipalType));
                item.SubItems.Add(PermCheck(entry.CanRead,        isDeny));
                item.SubItems.Add(PermCheck(entry.CanWrite,       isDeny));
                item.SubItems.Add(PermCheck(entry.CanDelete,      isDeny));
                item.SubItems.Add(PermCheck(entry.CanModify,      isDeny));
                item.SubItems.Add(PermCheck(entry.CanFullControl, isDeny));
                item.SubItems.Add(entry.ExpiryDate.HasValue ? entry.ExpiryDate.Value.ToString("dd/MM/yy") : "—");

                // Status column
                bool isExpired = !isRevoked && entry.ExpiryDate.HasValue && entry.ExpiryDate.Value.Date < DateTime.Today;

                if (isRevoked)
                {
                    item.SubItems.Add("ยกเลิกแล้ว");
                    item.ForeColor = Color.Gray;
                    item.Font      = new Font(aclListView.Font, FontStyle.Strikeout);
                }
                else if (isExpired)
                {
                    item.SubItems.Add("⚠ หมดอายุ");
                    item.ForeColor = Color.FromArgb(160, 90, 0);
                    item.Font      = new Font(aclListView.Font, FontStyle.Strikeout);
                    item.BackColor = Color.FromArgb(255, 248, 215);
                }
                else if (isDeny)
                {
                    item.SubItems.Add("✔ ใช้งาน (Deny)");
                    item.ForeColor = Color.Crimson;
                    item.Font      = new Font(aclListView.Font, FontStyle.Bold);
                }
                else
                {
                    item.SubItems.Add("✔ ใช้งาน");
                    item.ForeColor = Color.FromArgb(0, 120, 0);
                }

                item.Tag = entry;
                aclListView.Items.Add(item);
            }

            int activeCount = _aclEntries.Count;
            int totalCount  = _allAclEntries.Count;
            if (activeCount == 0 && !_showAllEntries)
            {
                lblAclStatus.Text      = "ยังไม่มีสิทธิ์ — กด + เพิ่ม เพื่อเพิ่มรายการ";
                lblAclStatus.ForeColor = Color.Gray;
            }
            else
            {
                lblAclStatus.Text = _showAllEntries
                    ? string.Format("{0} รายการ (ใช้งาน {1} / ยกเลิก {2})", totalCount, activeCount, totalCount - activeCount)
                    : string.Format("{0} รายการ (ใช้งาน)", activeCount);
                lblAclStatus.ForeColor = Color.FromArgb(0, 51, 102);
            }
        }

        private CheckBox _chkShowAll;
        private void EnsureShowAllCheckbox()
        {
            if (_chkShowAll != null) return; // already added

            _chkShowAll = new CheckBox
            {
                Text      = "แสดงทั้งหมด (รวมที่ยกเลิก)",
                AutoSize  = true,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(80, 80, 80),
                Checked   = false
            };

            // Position it in the toolbar panel (pnlPermToolbar) to the right of lblAclStatus
            _chkShowAll.Location = new System.Drawing.Point(
                lblAclStatus.Left - _chkShowAll.PreferredSize.Width - 12,
                (pnlPermToolbar.Height - _chkShowAll.PreferredSize.Height) / 2);

            _chkShowAll.CheckedChanged += (s, e) =>
            {
                _showAllEntries = _chkShowAll.Checked;
                RenderAclList();
            };

            pnlPermToolbar.Controls.Add(_chkShowAll);
            _chkShowAll.BringToFront();
        }

        public void ShowEffectivePermissions(EffectivePermission effective) { /* shown in grid */ }

        public void ShowPendingApprovals(IEnumerable<ApprovalRequest> pendingRequests)
        {
            if (pendingRequests == null) return;
            foreach (var req in pendingRequests)
            {
                string dateStr = req.RequestedDate.ToString("dd/MM/yy HH:mm");
                string subject = req.Subject ?? req.RequestType;

                var item = new ListViewItem(string.Format("⏳ {0}", subject));
                item.SubItems.Add("รออนุมัติ");  // ประเภท
                item.SubItems.Add("—");           // อ่าน
                item.SubItems.Add("—");           // เขียน
                item.SubItems.Add("—");           // ลบ
                item.SubItems.Add("—");           // แก้ไข
                item.SubItems.Add("—");           // Full
                item.SubItems.Add(dateStr);       // หมดอายุ (แสดงวันที่ขอ)

                item.BackColor = Color.FromArgb(255, 248, 215);   // เหลืองอ่อน
                item.ForeColor = Color.FromArgb(160, 90, 0);      // น้ำตาลส้ม
                item.Font      = new Font(aclListView.Font, FontStyle.Italic);
                item.Tag       = req;  // tag เป็น ApprovalRequest ไม่ใช่ FolderPermission

                aclListView.Items.Add(item);
            }

            // Update count label
            int total = _aclEntries.Count + aclListView.Items.Cast<ListViewItem>()
                .Count(i => i.Tag is ApprovalRequest);
            lblAclStatus.Text      = string.Format("{0} รายการ  (มีคำขอรออนุมัติ)", _aclEntries.Count);
            lblAclStatus.ForeColor = Color.FromArgb(160, 90, 0);
        }

        public void ShowConfidentialityLevel(int level)
        {
            if (_folder != null)
                lblTitle.Text = string.Format("⚙ ตั้งค่า Folder: {0} ({1})",
                    _folder.Name, ConfidentialityBadge(level));
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public bool ConfirmPropagateToFiles(int fileCount)
        {
            var result = MessageBox.Show(
                string.Format(
                    "✅ บันทึกสิทธิ์ Folder เรียบร้อยแล้ว\n\n" +
                    "พบไฟล์ {0} รายการใน Folder นี้\n\n" +
                    "ต้องการอัปเดตสิทธิ์ของไฟล์ทั้งหมดให้ตรงกับ Folder ด้วยหรือไม่?\n\n" +
                    "• กด [ใช่] — อัปเดตสิทธิ์ไฟล์ทั้งหมดให้เหมือน Folder\n" +
                    "• กด [ไม่] — คงสิทธิ์ไฟล์เดิมไว้",
                    fileCount),
                "อัปเดตสิทธิ์ไฟล์",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);   // default = No (ปลอดภัยกว่า)
            return result == DialogResult.Yes;
        }

        public int? SelectedPermissionId
        {
            get
            {
                if (aclListView.SelectedItems.Count == 0) return null;
                var entry = aclListView.SelectedItems[0].Tag as FolderPermission;
                return entry != null ? (int?)entry.PermissionId : null;
            }
        }

        public Folder SelectedFolder { get { return _folder; } }

        // ── Button handlers ──────────────────────────────────────────────────

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (_folder == null) return;
            var reqs = PromptForPermissionRequests(_folder.FolderId, null);
            if (reqs != null)
            {
                foreach (var req in reqs)
                    _presenter.SetPermission(req);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (_folder == null || aclListView.SelectedItems.Count == 0) return;
            var entry = aclListView.SelectedItems[0].Tag as FolderPermission;
            if (entry == null)
            {
                ShowError("ไม่สามารถแก้ไขรายการที่รออนุมัติได้");
                return;
            }
            var reqs = PromptForPermissionRequests(_folder.FolderId, entry);
            if (reqs != null)
            {
                foreach (var req in reqs)
                    _presenter.SetPermission(req);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (aclListView.SelectedItems.Count == 0) { ShowError("กรุณาเลือกรายการสิทธิ์ที่ต้องการลบ"); return; }
            // Block delete on pending approval rows
            if (aclListView.SelectedItems[0].Tag is ApprovalRequest)
            {
                ShowError("ไม่สามารถลบรายการที่รออนุมัติได้\nกรุณารอผลการอนุมัติก่อน");
                return;
            }
            int? permId = SelectedPermissionId;
            if (!permId.HasValue) { ShowError("กรุณาเลือกรายการสิทธิ์ที่ต้องการลบ"); return; }
            if (MessageBox.Show("ลบรายการสิทธิ์ที่เลือกใช่ไหม?", "ยืนยันการลบ",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                _presenter.RemovePermission(permId.Value);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (_folder == null) return;

                bool changed = false;

                // Save folder name
                string newName = txtFolderName.Text.Trim();
                if (!string.IsNullOrEmpty(newName) && newName != _folder.FolderName)
                {
                    _folder.FolderName = newName;
                    changed = true;
                }

                // Save confidentiality level
                if (cboConfLevel.SelectedIndex >= 0)
                {
                    int newLevel = cboConfLevel.SelectedIndex + 1;
                    if (_folder.ConfidentialityLevel != newLevel)
                    {
                        _presenter.ChangeConfidentialityLevel(_folder.FolderId, newLevel);
                        _folder.ConfidentialityLevel = newLevel;
                        changed = true;
                    }
                }

                // Save inherit permission
                if (_folder.InheritPermission != chkInheritPerm.Checked)
                {
                    _folder.InheritPermission = chkInheritPerm.Checked;
                    changed = true;
                }

                // Persist to DB
                if (changed && _folderRepository != null)
                {
                    _folder.UpdatedBy = null; // set to current user ID if available
                    _folderRepository.Update(_folder);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ShowError("ไม่สามารถบันทึกข้อมูล: " + ex.Message);
            }
        }

        // ── Permission dialog ────────────────────────────────────────────────

        /// <summary>
        /// Shows the permission dialog and returns 1 or 2 SetPermissionRequest entries:
        /// - If all selected are Allow → 1 Allow entry
        /// - If all selected are Deny  → 1 Deny entry
        /// - If mixed (some Allow, some Deny) → 2 entries: 1 Allow + 1 Deny
        /// </summary>
        private List<SetPermissionRequest> PromptForPermissionRequests(int folderId, FolderPermission existing)
        {
            bool[] allowChecked = new bool[5];
            bool[] denyChecked  = new bool[5];
            int principalType = 1;
            int principalId   = 0;

            // ShowPermissionDialog returns dlg with Tag = List<OrgUnitSelection> when multi-org selected
            List<OrgUnitPickerDialog.OrgUnitSelection> multiOrg = null;

            bool confirmed = ShowPermissionDialog(folderId, existing,
                out allowChecked, out denyChecked, out principalType, out principalId,
                out multiOrg);

            if (!confirmed) return null;

            var results = new List<SetPermissionRequest>();
            bool hasAllow = allowChecked[0] || allowChecked[1] || allowChecked[2] || allowChecked[3] || allowChecked[4];
            bool hasDeny  = denyChecked[0]  || denyChecked[1]  || denyChecked[2]  || denyChecked[3]  || denyChecked[4];

            // ── Multi-org: สร้าง entry สำหรับแต่ละหน่วยงานที่เลือก ──────────
            if (multiOrg != null && multiOrg.Count > 0)
            {
                foreach (var sel in multiOrg)
                {
                    if (hasAllow)
                        results.Add(new SetPermissionRequest
                        {
                            FolderID       = folderId,
                            PrincipalType  = sel.PrincipalType,
                            PrincipalID    = sel.OrgUnit.OrgUnitID,
                            CanRead        = allowChecked[0], CanWrite  = allowChecked[1],
                            CanDelete      = allowChecked[2], CanModify = allowChecked[3],
                            CanFullControl = allowChecked[4], IsDeny    = false
                        });
                    if (hasDeny)
                        results.Add(new SetPermissionRequest
                        {
                            FolderID       = folderId,
                            PrincipalType  = sel.PrincipalType,
                            PrincipalID    = sel.OrgUnit.OrgUnitID,
                            CanRead        = denyChecked[0], CanWrite  = denyChecked[1],
                            CanDelete      = denyChecked[2], CanModify = denyChecked[3],
                            CanFullControl = denyChecked[4], IsDeny    = true
                        });
                }
                return results.Count > 0 ? results : null;
            }

            // ── Single principal ──────────────────────────────────────────────
            if (principalId <= 0) return null;

            if (hasAllow)
                results.Add(new SetPermissionRequest
                {
                    FolderID       = folderId,
                    PrincipalType  = principalType,
                    PrincipalID    = principalId,
                    CanRead        = allowChecked[0], CanWrite  = allowChecked[1],
                    CanDelete      = allowChecked[2], CanModify = allowChecked[3],
                    CanFullControl = allowChecked[4], IsDeny    = false
                });
            if (hasDeny)
                results.Add(new SetPermissionRequest
                {
                    FolderID       = folderId,
                    PrincipalType  = principalType,
                    PrincipalID    = principalId,
                    CanRead        = denyChecked[0], CanWrite  = denyChecked[1],
                    CanDelete      = denyChecked[2], CanModify = denyChecked[3],
                    CanFullControl = denyChecked[4], IsDeny    = true
                });

            return results.Count > 0 ? results : null;
        }

        private SetPermissionRequest PromptForPermissionRequest(int folderId, FolderPermission existing)
        {
            bool[] allowOut = new bool[5];
            bool[] denyOut  = new bool[5];
            int pt = 1, pid = 0;
            List<OrgUnitPickerDialog.OrgUnitSelection> multiOrg;
            if (!ShowPermissionDialog(folderId, existing, out allowOut, out denyOut, out pt, out pid, out multiOrg))
                return null;
            bool anyDeny = denyOut[0] || denyOut[1] || denyOut[2] || denyOut[3] || denyOut[4];
            return new SetPermissionRequest
            {
                FolderID       = folderId,
                PrincipalType  = pt,
                PrincipalID    = pid,
                CanRead        = allowOut[0] || denyOut[0],
                CanWrite       = allowOut[1] || denyOut[1],
                CanDelete      = allowOut[2] || denyOut[2],
                CanModify      = allowOut[3] || denyOut[3],
                CanFullControl = allowOut[4] || denyOut[4],
                IsDeny         = anyDeny
            };
        }

        private bool ShowPermissionDialog(int folderId, FolderPermission existing,
            out bool[] allowChecked, out bool[] denyChecked, out int principalType, out int principalId,
            out List<OrgUnitPickerDialog.OrgUnitSelection> multiOrg)
        {
            multiOrg = null;
            using (var dlg = new Form())
            {
                dlg.Text            = "";
                dlg.ClientSize      = new Size(460, 530);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.BackColor       = Color.White;

                // ── Header bar (navy) ────────────────────────────────────────
                var pnlHdr = new Panel
                {
                    Location  = new Point(0, 0),
                    Size      = new Size(460, 44),
                    BackColor = Color.FromArgb(31, 56, 100)
                };
                var lblHdrTitle = new Label
                {
                    Text      = "🔒 เพิ่ม/แก้ไข Permission",
                    Location  = new Point(12, 0),
                    Size      = new Size(400, 44),
                    Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                var btnHdrClose = new Button
                {
                    Text        = "✕",
                    Location    = new Point(416, 0),
                    Size        = new Size(44, 44),
                    FlatStyle   = FlatStyle.Flat,
                    ForeColor   = Color.White,
                    BackColor   = Color.Transparent,
                    Font        = new Font("Segoe UI", 11, FontStyle.Bold),
                    DialogResult = DialogResult.Cancel
                };
                btnHdrClose.FlatAppearance.BorderSize = 0;
                pnlHdr.Controls.Add(lblHdrTitle);
                pnlHdr.Controls.Add(btnHdrClose);
                dlg.Controls.Add(pnlHdr);

                // ── Folder info box ──────────────────────────────────────────
                var pnlFolder = new Panel
                {
                    Location    = new Point(12, 52),
                    Size        = new Size(436, 36),
                    BackColor   = Color.FromArgb(232, 240, 254),
                    BorderStyle = BorderStyle.FixedSingle
                };
                string folderPath = _folder != null ? _folder.Name : "—";
                int    confLevel  = _folder != null ? _folder.ConfidentialityLevel : 3;
                string badgeText  = ConfidentialityBadge(confLevel);

                var lblFolderPath = new Label
                {
                    Text      = "Folder:  " + folderPath,
                    Location  = new Point(8, 0),
                    Size      = new Size(340, 34),
                    Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(31, 56, 100),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                var lblBadge = new Label
                {
                    Text      = badgeText,
                    Location  = new Point(354, 7),
                    AutoSize  = true,
                    Font      = new Font("Segoe UI", 8, FontStyle.Bold),
                    BackColor = ConfidentialityColor(confLevel),
                    ForeColor = Color.White,
                    Padding   = new Padding(5, 2, 5, 2)
                };
                pnlFolder.Controls.Add(lblFolderPath);
                pnlFolder.Controls.Add(lblBadge);
                dlg.Controls.Add(pnlFolder);

                // ── ประเภทผู้ใช้ ─────────────────────────────────────────────
                dlg.Controls.Add(new Label
                {
                    Text      = "ประเภทผู้ใช้:",
                    Location  = new Point(12, 100),
                    AutoSize  = true,
                    Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50)
                });

                var rdUser  = new RadioButton { Text = "ผู้ใช้เฉพาะราย", Location = new Point(24,  122), AutoSize = true, Font = new Font("Segoe UI", 9) };
                var rdGroup = new RadioButton { Text = "กลุ่ม/แผนก",     Location = new Point(160, 122), AutoSize = true, Font = new Font("Segoe UI", 9) };
                var rdRole  = new RadioButton { Text = "Role",            Location = new Point(280, 122), AutoSize = true, Font = new Font("Segoe UI", 9) };
                dlg.Controls.Add(rdUser);
                dlg.Controls.Add(rdGroup);
                dlg.Controls.Add(rdRole);

                // ── ปุ่มเปิด OrgUnit Picker (แสดงเฉพาะตอน rdGroup checked) ──
                var btnOrgPicker = new Button
                {
                    Text      = "🏢  เลือกจากโครงสร้างองค์กร...",
                    Location  = new Point(12, 148),
                    Size      = new Size(220, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(43, 87, 154),
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor    = Cursors.Hand,
                    Visible   = false
                };
                btnOrgPicker.FlatAppearance.BorderSize = 0;

                // Label แสดงหัวข้อรายการที่เลือก
                var lblSelectedOrg = new Label
                {
                    Text      = "หน่วยงานที่เลือก:",
                    Location  = new Point(12, 182),
                    AutoSize  = true,
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50),
                    Visible   = false
                };

                // ListBox แสดงรายการที่เลือกจาก OrgUnitPickerDialog
                var lstSelectedOrg = new ListBox
                {
                    Location    = new Point(12, 200),
                    Size        = new Size(430, 60),
                    Font        = new Font("Segoe UI", 9),
                    BorderStyle = BorderStyle.FixedSingle,
                    Visible     = false,
                    BackColor   = Color.FromArgb(248, 249, 252)
                };

                // เก็บ OrgUnitSelection list ที่เลือก
                var pickedOrgUnits = new List<OrgUnitPickerDialog.OrgUnitSelection>();

                btnOrgPicker.Click += (s, e) =>
                {
                    var allUnits = new List<OrgUnit>();
                    if (_orgStructureService != null)
                    {
                        try { allUnits = _orgStructureService.GetHierarchy().ToList(); }
                        catch { }
                    }

                    using (var picker = new OrgUnitPickerDialog(allUnits))
                    {
                        if (picker.ShowDialog(dlg) == DialogResult.OK && picker.SelectedUnits.Count > 0)
                        {
                            pickedOrgUnits.Clear();
                            pickedOrgUnits.AddRange(picker.SelectedUnits);
                            lstSelectedOrg.Items.Clear();
                            foreach (var sel in pickedOrgUnits)
                                lstSelectedOrg.Items.Add(sel.DisplayName);
                            lblSelectedOrg.Visible = true;
                            lstSelectedOrg.Visible = true;
                            // ปรับขนาด ListBox ตามจำนวนรายการ (max 4 rows)
                            int rows = Math.Min(pickedOrgUnits.Count, 4);
                            lstSelectedOrg.Height = rows * 18 + 8;
                        }
                    }
                };

                dlg.Controls.Add(btnOrgPicker);
                dlg.Controls.Add(lblSelectedOrg);
                dlg.Controls.Add(lstSelectedOrg);

                // ── เลือกหน่วยงาน / ผู้ใช้ ──────────────────────────────────
                var lblSelect = new Label
                {
                    Text      = "เลือกหน่วยงาน:",
                    Location  = new Point(12, 158),
                    AutoSize  = true,
                    Font      = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(50, 50, 50)
                };
                var cboSelect = new ComboBox
                {
                    Location      = new Point(132, 154),
                    Size          = new Size(240, 24),
                    DropDownStyle = ComboBoxStyle.DropDown,
                    Font          = new Font("Segoe UI", 9)
                };

                // ── ปุ่มค้นหาพนักงาน (Employee API) ─────────────────────────
                var btnSearchEmp = new Button
                {
                    Text      = "🔍 ค้นหา",
                    Location  = new Point(378, 152),
                    Size      = new Size(70, 26),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(43, 87, 154),
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor    = Cursors.Hand,
                    Visible   = false   // แสดงเฉพาะตอน rdUser checked
                };
                btnSearchEmp.FlatAppearance.BorderSize = 0;
                btnSearchEmp.Click += (s, e) =>
                {
                    if (_employeeApiService == null)
                    {
                        MessageBox.Show("Employee API service ไม่พร้อมใช้งาน",
                            "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    using (var searchDlg = new EmployeeSearchDialog(_employeeApiService))
                    {
                        if (searchDlg.ShowDialog(dlg) == DialogResult.OK && searchDlg.SelectedEmployee != null)
                        {
                            var emp = searchDlg.SelectedEmployee;
                            string label = string.Format("{0} — {1} [{2}]",
                                emp.DisplayName,
                                emp.DepartmentNameTh ?? emp.DepartmentNameEn ?? "—",
                                emp.EmployeeCode);
                            cboSelect.Text = label;
                            cboSelect.Tag = emp;
                        }
                    }
                };

                dlg.Controls.Add(lblSelect);
                dlg.Controls.Add(cboSelect);
                dlg.Controls.Add(btnSearchEmp);

                // Helper: reload list based on type
                Action<bool> reloadList = (isUser) =>
                {
                    cboSelect.Items.Clear();
                    cboSelect.Text = "";
                    if (isUser)
                    {
                        if (_userAccountRepository == null) return;
                        try
                        {
                            _userList = _userAccountRepository.GetAll().ToList();
                            foreach (var u in _userList.OrderBy(x => x.ADUserName))
                                cboSelect.Items.Add(string.Format("{0} — {1} (ID:{2})", u.ADUserName, u.DisplayName, u.UserID));
                            if (cboSelect.Items.Count > 0) cboSelect.SelectedIndex = 0;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("ไม่สามารถโหลดรายชื่อผู้ใช้\nกรุณากรอก UserID ด้วยตนเอง\n\n" + ex.Message,
                                "SQL Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else if (rdGroup.Checked)
                    {
                        if (_employeeApiService == null) return;
                        try
                        {
                            cboSelect.Items.Add("── แผนก ──");
                            foreach (var d in _employeeApiService.GetDepartments())
                                cboSelect.Items.Add(new OrgUnitItem(3, d.DepartmentId, string.Format("[แผนก] {0}", d.DisplayName)));

                            cboSelect.Items.Add("── ฝ่าย ──");
                            foreach (var v in _employeeApiService.GetDivisions())
                                cboSelect.Items.Add(new OrgUnitItem(4, v.DivisionId, string.Format("[ฝ่าย] {0}", v.DisplayName)));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("ไม่สามารถโหลดข้อมูลหน่วยงาน\n\n" + ex.Message,
                                "API Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                };

                rdUser.CheckedChanged  += (s, e) => {
                    if (rdUser.Checked)  {
                        lblSelect.Text = "เลือกผู้ใช้:";
                        btnSearchEmp.Visible = true;
                        btnOrgPicker.Visible = false;
                        lstSelectedOrg.Visible = false;
                        lblSelectedOrg.Visible = false;
                        lblSelect.Visible = true;
                        cboSelect.Visible = true;
                        reloadList(true);
                    }
                };
                rdGroup.CheckedChanged += (s, e) => {
                    if (rdGroup.Checked) {
                        lblSelect.Text = "เลือกหน่วยงาน:";
                        btnSearchEmp.Visible = false;
                        btnOrgPicker.Visible = true;
                        lstSelectedOrg.Visible = pickedOrgUnits.Count > 0;
                        lblSelectedOrg.Visible = pickedOrgUnits.Count > 0;
                        lblSelect.Visible = false;
                        cboSelect.Visible = false;
                    }
                };
                rdRole.CheckedChanged  += (s, e) => {
                    if (rdRole.Checked)  {
                        lblSelect.Text = "เลือก Role:";
                        btnSearchEmp.Visible = false;
                        btnOrgPicker.Visible = false;
                        lstSelectedOrg.Visible = false;
                        lblSelectedOrg.Visible = false;
                        lblSelect.Visible = true;
                        cboSelect.Visible = true;
                        reloadList(false);
                    }
                };

                // Set default principal type
                if (existing != null)
                {
                    switch (existing.PrincipalType)
                    {
                        case 1:  rdUser.Checked  = true; break;
                        case 3:  // Department
                        case 4:  // Division
                            rdGroup.Checked = true; break;
                        default: rdGroup.Checked = true; break;
                    }
                }
                else { rdGroup.Checked = true; }

                // Trigger reloadList to populate dropdown
                if (rdUser.Checked)
                    reloadList(true);
                else if (rdGroup.Checked)
                    reloadList(false);

                // Set existing principal value
                if (existing != null)
                {
                    if (existing.PrincipalType == 1 && _userList != null && _userList.Count > 0)
                    {
                        var eu = _userList.FirstOrDefault(u => u.UserID == existing.PrincipalId);
                        cboSelect.Text = eu != null
                            ? string.Format("{0} — {1} (ID:{2})", eu.ADUserName, eu.DisplayName, eu.UserID)
                            : existing.PrincipalId.ToString();
                    }
                    else if (existing.PrincipalType == 3)
                    {
                        // Department
                        string deptName;
                        if (_departmentNameMap.TryGetValue(existing.PrincipalId, out deptName))
                            cboSelect.Text = string.Format("[แผนก] {0}", deptName);
                        else
                            cboSelect.Text = string.Format("DeptID: {0}", existing.PrincipalId);
                    }
                    else if (existing.PrincipalType == 4)
                    {
                        // Division
                        string divName;
                        if (_divisionNameMap.TryGetValue(existing.PrincipalId, out divName))
                            cboSelect.Text = string.Format("[ฝ่าย] {0}", divName);
                        else
                            cboSelect.Text = string.Format("DivID: {0}", existing.PrincipalId);
                    }
                    else
                    {
                        cboSelect.Text = existing.PrincipalId.ToString();
                    }
                }

                // ── สิทธิการเข้าถึง header ────────────────────────────────
                var lblPermHeader = new Label
                {
                    Text      = "สิทธิการเข้าถึง:",
                    Location  = new Point(12, 192),
                    AutoSize  = true,
                    Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50)
                };
                dlg.Controls.Add(lblPermHeader);

                // ── Permission table ──────────────────────────────────────────
                var pnlPerm = new Panel
                {
                    Location    = new Point(12, 212),
                    Size        = new Size(436, 177),
                    BackColor   = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Table header row
                var pnlPermHdr = new Panel
                {
                    Location  = new Point(0, 0),
                    Size      = new Size(436, 27),
                    BackColor = Color.FromArgb(240, 244, 250)
                };
                pnlPermHdr.Controls.Add(new Label { Text = "สิทธิ์",  Location = new Point(10, 5), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) });
                pnlPermHdr.Controls.Add(new Label { Text = "Allow",   Location = new Point(272, 5), Size = new Size(60, 18), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60), TextAlign = ContentAlignment.MiddleCenter });
                pnlPermHdr.Controls.Add(new Label { Text = "Deny",    Location = new Point(365, 5), Size = new Size(55, 18), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60), TextAlign = ContentAlignment.MiddleCenter });
                pnlPerm.Controls.Add(pnlPermHdr);

                // Row data: label, existing allow, existing deny
                var permDefs = new[]
                {
                    new { Label = "📖 อ่าน (Read)",      Allow = existing != null && existing.CanRead        && !existing.IsDeny, Deny = existing != null && existing.CanRead        && existing.IsDeny },
                    new { Label = "✏️ เขียน (Write)",    Allow = existing != null && existing.CanWrite       && !existing.IsDeny, Deny = existing != null && existing.CanWrite       && existing.IsDeny },
                    new { Label = "🗑 ลบ (Delete)",      Allow = existing != null && existing.CanDelete      && !existing.IsDeny, Deny = existing != null && existing.CanDelete      && existing.IsDeny },
                    new { Label = "🔧 แก้ไข (Modify)",  Allow = existing != null && existing.CanModify      && !existing.IsDeny, Deny = existing != null && existing.CanModify      && existing.IsDeny },
                    new { Label = "👑 Full Control",     Allow = existing != null && existing.CanFullControl && !existing.IsDeny, Deny = existing != null && existing.CanFullControl && existing.IsDeny },
                };

                var rdAllow = new RadioButton[5];
                var rdDeny  = new RadioButton[5];

                for (int i = 0; i < permDefs.Length; i++)
                {
                    int rowY = 28 + i * 29;
                    var rowPnl = new Panel
                    {
                        Location  = new Point(0, rowY),
                        Size      = new Size(434, 29),
                        BackColor = i % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 255)
                    };

                    rowPnl.Controls.Add(new Label
                    {
                        Text      = permDefs[i].Label,
                        Location  = new Point(10, 5),
                        Size      = new Size(240, 20),
                        Font      = new Font("Segoe UI", 9),
                        ForeColor = Color.FromArgb(40, 40, 40)
                    });

                    // Allow/Deny radio buttons are inside the rowPnl so they auto-group per row
                    rdAllow[i] = new RadioButton { Location = new Point(291, 5), Size = new Size(20, 20), Checked = permDefs[i].Allow };
                    rdDeny[i]  = new RadioButton { Location = new Point(388, 5), Size = new Size(20, 20), Checked = permDefs[i].Deny  };
                    rowPnl.Controls.Add(rdAllow[i]);
                    rowPnl.Controls.Add(rdDeny[i]);
                    pnlPerm.Controls.Add(rowPnl);
                }
                dlg.Controls.Add(pnlPerm);

                // ── วันหมดอายุ ─────────────────────────────────────────────
                int expiryY = 400;
                var lblExpiry = new Label
                {
                    Text      = "วันหมดอายุ:",
                    Location  = new Point(12, expiryY + 5),
                    AutoSize  = true,
                    Font      = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(50, 50, 50)
                };
                dlg.Controls.Add(lblExpiry);
                var dtpExpiry = new DateTimePicker
                {
                    Location = new Point(100, expiryY),
                    Size     = new Size(154, 24),
                    Format   = DateTimePickerFormat.Short,
                    Font     = new Font("Segoe UI", 9),
                    Enabled  = existing != null && existing.ExpiryDate.HasValue
                };
                if (existing != null && existing.ExpiryDate.HasValue)
                    dtpExpiry.Value = existing.ExpiryDate.Value;
                var chkNoExpiry = new CheckBox
                {
                    Text     = "ไม่มีวันหมดอายุ",
                    Location = new Point(264, expiryY + 3),
                    AutoSize = true,
                    Font     = new Font("Segoe UI", 9),
                    Checked  = existing == null || !existing.ExpiryDate.HasValue
                };
                chkNoExpiry.CheckedChanged += (s, e) => dtpExpiry.Enabled = !chkNoExpiry.Checked;
                dlg.Controls.Add(dtpExpiry);
                dlg.Controls.Add(chkNoExpiry);

                // ── Warning box ───────────────────────────────────────────────
                var pnlWarn = new Panel
                {
                    Location    = new Point(12, 434),
                    Size        = new Size(436, 34),
                    BackColor   = Color.FromArgb(255, 248, 215),
                    BorderStyle = BorderStyle.FixedSingle
                };
                pnlWarn.Controls.Add(new Label
                {
                    Text      = "⚠ การเปลี่ยนแปลงนี้ต้องได้รับอนุมัติจาก Admin",
                    Dock      = DockStyle.Fill,
                    Font      = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(160, 90, 0),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(6, 0, 0, 0)
                });
                dlg.Controls.Add(pnlWarn);

                // ── Footer buttons ────────────────────────────────────────────
                var pnlFooter = new Panel
                {
                    Location  = new Point(0, 478),
                    Size      = new Size(460, 52),
                    BackColor = Color.FromArgb(248, 250, 255)
                };
                var btnOk = new Button
                {
                    Text      = "✉ ส่งขออนุมัติ",
                    Location  = new Point(316, 9),
                    Size      = new Size(130, 34),
                    BackColor = Color.FromArgb(43, 87, 154),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                    DialogResult = DialogResult.OK
                };
                btnOk.FlatAppearance.BorderSize = 0;
                var btnCnl = new Button
                {
                    Text      = "ยกเลิก",
                    Location  = new Point(228, 9),
                    Size      = new Size(80, 34),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(80, 80, 80),
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9),
                    DialogResult = DialogResult.Cancel
                };
                btnCnl.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 190);
                pnlFooter.Controls.Add(btnOk);
                pnlFooter.Controls.Add(btnCnl);
                dlg.Controls.Add(pnlFooter);

                // ── UpdateLayout: เลื่อน permission section ลงตาม org list ──
                // ทุก control ที่อยู่ใต้ btnOrgPicker จะถูก shift ลงเมื่อ lstSelectedOrg แสดง
                Action updateLayout = () =>
                {
                    int orgListExtra = lstSelectedOrg.Visible
                        ? 20 + lstSelectedOrg.Height   // lblSelectedOrg (~20px) + lstSelectedOrg
                        : 0;
                    int permY    = 192 + orgListExtra;
                    int pnlPermY = permY + 20;
                    int expY     = pnlPermY + pnlPerm.Height + 12;
                    int warnY    = expY + 44;
                    int footerY  = warnY + 44;

                    lblPermHeader.Location = new Point(12, permY);
                    pnlPerm.Location       = new Point(12, pnlPermY);
                    lblExpiry.Location     = new Point(12, expY + 5);
                    dtpExpiry.Location     = new Point(100, expY);
                    chkNoExpiry.Location   = new Point(264, expY + 3);
                    pnlWarn.Location       = new Point(12, warnY);
                    pnlFooter.Location     = new Point(0, footerY);
                    dlg.ClientSize         = new Size(460, footerY + pnlFooter.Height);
                };

                // คำนวณ layout เริ่มต้น (lstSelectedOrg ซ่อนอยู่)
                updateLayout();

                // เรียก updateLayout ทุกครั้งที่ org list เปลี่ยน
                lstSelectedOrg.VisibleChanged += (s, e) => updateLayout();
                lstSelectedOrg.SizeChanged    += (s, e) => updateLayout();

                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCnl;

                // ── Show & validate ───────────────────────────────────────────
                while (true)
                {
                    if (dlg.ShowDialog() != DialogResult.OK)
                    {
                        allowChecked  = new bool[5];
                        denyChecked   = new bool[5];
                        principalType = 1;
                        principalId   = 0;
                        multiOrg      = null;
                        return false;
                    }

                    // ── กรณี rdGroup + pickedOrgUnits → ข้าม validation ปกติ ──
                    if (rdGroup.Checked && pickedOrgUnits.Count > 0)
                    {
                        // ตรวจสอบว่าเลือก Allow หรือ Deny อย่างน้อย 1 รายการ
                        bool anyPerm = rdAllow[0].Checked || rdAllow[1].Checked || rdAllow[2].Checked || rdAllow[3].Checked || rdAllow[4].Checked
                                    || rdDeny[0].Checked  || rdDeny[1].Checked  || rdDeny[2].Checked  || rdDeny[3].Checked  || rdDeny[4].Checked;
                        if (!anyPerm)
                        {
                            MessageBox.Show("กรุณาเลือกสิทธิ์อย่างน้อย 1 รายการ (Allow หรือ Deny)",
                                "ข้อมูลไม่ครบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }
                        break;
                    }

                    string selText = cboSelect.Text.Trim();
                    if (string.IsNullOrEmpty(selText) && !(rdGroup.Checked && pickedOrgUnits.Count > 0))
                    {
                        MessageBox.Show("กรุณาเลือกหรือกรอก ผู้ใช้/กลุ่ม/หน่วยงาน", "ข้อมูลไม่ครบ",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    int pid = 0;
                    // ── กรณีเลือก Employee จาก Search Dialog (Tag = EmployeeSummary) ──
                    if (rdUser.Checked && cboSelect.Tag is EmployeeSummary)
                    {
                        var emp = (EmployeeSummary)cboSelect.Tag;
                        pid = ResolveOrCreateUserAccount(emp);
                        if (pid <= 0)
                        {
                            MessageBox.Show("ไม่สามารถสร้างหรือค้นหา UserAccount ได้", "ข้อมูลไม่ถูกต้อง",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }
                        // อัปเดต label ใน cboSelect ให้แสดง ID ที่ได้
                        cboSelect.Text = string.Format("{0} — {1} (ID:{2})",
                            emp.DisplayName,
                            emp.DepartmentNameTh ?? emp.DepartmentNameEn ?? "—",
                            pid);
                        cboSelect.Tag = null;
                    }
                    // ── กรณีเลือกจาก OrgUnitItem (แผนก/ฝ่าย) ──
                    else if (cboSelect.SelectedItem is OrgUnitItem)
                    {
                        pid = ((OrgUnitItem)cboSelect.SelectedItem).Id;
                    }
                    else if (selText.Contains("(ID:"))
                    {
                        var part = selText.Substring(selText.LastIndexOf("ID:") + 3).TrimEnd(')').Trim();
                        if (!int.TryParse(part, out pid))
                        {
                            MessageBox.Show("ไม่สามารถแยก ID ได้", "ข้อมูลไม่ถูกต้อง",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }
                    }
                    else if (!int.TryParse(selText, out pid))
                    {
                        MessageBox.Show("กรุณากรอก ID ที่ถูกต้อง หรือเลือกจากรายชื่อ", "ข้อมูลไม่ถูกต้อง",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    if (pid <= 0)
                    {
                        MessageBox.Show("ID ต้องมากกว่า 0", "ข้อมูลไม่ถูกต้อง",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    // Validate expiry date — must not be in the past
                    if (!chkNoExpiry.Checked)
                    {
                        if (dtpExpiry.Value.Date < DateTime.Today)
                        {
                            MessageBox.Show(
                                string.Format("วันหมดอายุ ({0}) ผ่านไปแล้ว\nกรุณาเลือกวันที่ในอนาคต หรือเลือก 'ไม่มีวันหมดอายุ'",
                                    dtpExpiry.Value.ToString("dd/MM/yyyy")),
                                "วันหมดอายุไม่ถูกต้อง",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }
                        if (dtpExpiry.Value.Date == DateTime.Today)
                        {
                            var confirm = MessageBox.Show(
                                "วันหมดอายุเป็นวันนี้ สิทธิ์จะหมดอายุภายในวันนี้\nต้องการดำเนินการต่อหรือไม่?",
                                "ยืนยันวันหมดอายุ",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (confirm != DialogResult.Yes) continue;
                        }
                    }
                    break;
                }

                // Resolve final principal ID
                int finalId = 0;

                // กรณีเลือกจาก OrgUnitPickerDialog (หลายหน่วยงาน) — ส่ง list กลับผ่าน out parameter
                if (rdGroup.Checked && pickedOrgUnits.Count > 0)
                {
                    allowChecked  = new bool[] { rdAllow[0].Checked, rdAllow[1].Checked, rdAllow[2].Checked, rdAllow[3].Checked, rdAllow[4].Checked };
                    denyChecked   = new bool[] { rdDeny[0].Checked,  rdDeny[1].Checked,  rdDeny[2].Checked,  rdDeny[3].Checked,  rdDeny[4].Checked  };
                    var first     = pickedOrgUnits[0];
                    principalType = first.PrincipalType;
                    principalId   = first.OrgUnit.OrgUnitID;
                    multiOrg      = pickedOrgUnits;
                    return true;
                }

                if (cboSelect.SelectedItem is OrgUnitItem)
                {
                    finalId = ((OrgUnitItem)cboSelect.SelectedItem).Id;
                }
                else
                {
                    string finalText = cboSelect.Text.Trim();
                    if (finalText.Contains("(ID:"))
                    {
                        var part = finalText.Substring(finalText.LastIndexOf("ID:") + 3).TrimEnd(')').Trim();
                        int.TryParse(part, out finalId);
                    }
                    else
                    {
                        int.TryParse(finalText, out finalId);
                    }
                }

                // Set out parameters with per-permission allow/deny state
                allowChecked  = new bool[] { rdAllow[0].Checked, rdAllow[1].Checked, rdAllow[2].Checked, rdAllow[3].Checked, rdAllow[4].Checked };
                denyChecked   = new bool[] { rdDeny[0].Checked,  rdDeny[1].Checked,  rdDeny[2].Checked,  rdDeny[3].Checked,  rdDeny[4].Checked  };
                
                // Determine principalType: 1=User, 3=Department, 4=Division
                if (rdUser.Checked)
                {
                    principalType = 1;
                }
                else if (rdGroup.Checked && cboSelect.SelectedItem is OrgUnitItem)
                {
                    // OrgUnitItem stores the correct type (3=Department or 4=Division)
                    principalType = ((OrgUnitItem)cboSelect.SelectedItem).PrincipalType;
                }
                else
                {
                    principalType = 1; // default to User
                }

                principalId = finalId;
                // ดึง multiOrg จาก dlg.Tag (ถ้ามี)
                multiOrg = dlg.Tag as List<OrgUnitPickerDialog.OrgUnitSelection>;
                return true;
            }
        }

        private static void AddRow(TableLayoutPanel t, string label, Control ctrl, int row)
        {
            if (!string.IsNullOrEmpty(label))
                t.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, row);
            t.Controls.Add(ctrl, 1, row);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string PermCheck(bool value, bool isDeny)
        {
            if (!value) return "—";
            return isDeny ? "⊘" : "✓";
        }

        /// <summary>
        /// หา UserID จาก DB โดยใช้ ADUserName = "BUI_DOMAIN1\{FullNameEn}"
        /// ถ้าไม่มีให้สร้าง UserAccount ใหม่อัตโนมัติ
        /// </summary>
        private int ResolveOrCreateUserAccount(EmployeeSummary emp)
        {
            if (_userAccountRepository == null) return 0;

            // สร้าง ADUserName จาก FullNameEn (เอาเฉพาะชื่อ EN จาก DisplayName)
            string fullNameEn = ExtractEnglishName(emp.DisplayName);
            if (string.IsNullOrEmpty(fullNameEn)) return 0;

            // format: BUI_DOMAIN1\firstname.lastname (lowercase, no spaces)
            string adUserName = string.Format(@"BUI_DOMAIN1\{0}",
                fullNameEn.Replace(" ", ".").ToLower());

            // ค้นหาใน DB ก่อน
            try
            {
                var existing = _userAccountRepository.GetByADUserName(adUserName);
                if (existing != null) return existing.UserID;
            }
            catch { }

            // ไม่มีใน DB → ถามผู้ใช้ก่อนสร้าง
            var confirm = MessageBox.Show(
                string.Format(
                    "ไม่พบ User ในระบบ\n\nจะสร้าง UserAccount ใหม่ให้อัตโนมัติ?\n\n" +
                    "ชื่อ: {0}\nAD Username: {1}",
                    emp.DisplayName, adUserName),
                "สร้าง UserAccount ใหม่",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return 0;

            try
            {
                // หา OrgUnitID จาก DepartmentId (ถ้ามี) หรือใช้ 1 เป็น default
                int orgUnitId = 1;
                if (emp.DepartmentId > 0)
                {
                    // พยายาม map DepartmentId → OrgUnitID จาก _allUnits ที่มีอยู่
                    // ถ้าไม่มีให้ใช้ 1 (root)
                    orgUnitId = 1;
                }

                var newAccount = new UserAccount
                {
                    ADUserName   = adUserName,
                    DisplayName  = emp.DisplayName ?? fullNameEn,
                    Email        = null,
                    AppRole      = 1,       // Officer (ระดับต่ำสุด)
                    OrgUnitID    = orgUnitId,
                    ConsentGiven = false,
                    IsActive     = true,
                    CreatedDate  = DateTime.Now
                };

                int newId = _userAccountRepository.Create(newAccount);

                // อัปเดต cache
                if (newId > 0)
                    _userDisplayMap[newId] = newAccount.DisplayName;

                return newId;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถสร้าง UserAccount ได้\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        /// <summary>แยกชื่อภาษาอังกฤษออกจาก DisplayName เช่น "ขนิษฐา ไชยมงค์ (Khanittha Chaimong)" → "Khanittha Chaimong"</summary>
        private static string ExtractEnglishName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return string.Empty;
            int open  = displayName.LastIndexOf('(');
            int close = displayName.LastIndexOf(')');
            if (open >= 0 && close > open)
                return displayName.Substring(open + 1, close - open - 1).Trim();
            // ถ้าไม่มีวงเล็บ ให้ใช้ทั้งหมด (อาจเป็น EN อยู่แล้ว)
            return displayName.Trim();
        }

        private static string PrincipalTypeLabel(int type)
        {
            switch (type)
            {
                case 1: return "ผู้ใช้";
                case 3: return "แผนก";
                case 4: return "ฝ่าย";
                default: return type.ToString();
            }
        }

        private static string ConfidentialityBadge(int level)
        {
            switch (level)
            {
                case 1: return "1HC - Highly Confidential";
                case 2: return "2SC - Secret";
                case 3: return "3NC - Non-Confidential";
                case 4: return "4PC - Public/Common";
                default: return level.ToString();
            }
        }

        private static System.Drawing.Color ConfidentialityColor(int level)
        {
            switch (level)
            {
                case 1: return System.Drawing.Color.DarkRed;
                case 2: return System.Drawing.Color.OrangeRed;
                case 3: return System.Drawing.Color.FromArgb(0, 102, 204); // Blue #0066CC
                case 4: return System.Drawing.Color.DarkGreen;
                default: return System.Drawing.Color.Gray;
            }
        }

        private static string ConfidentialityColorLabel(int level)
        {
            switch (level)
            {
                case 1: return "Red — 1HC";
                case 2: return "Orange — 2SC";
                case 3: return "Blue (#0066CC) — 3NC";
                case 4: return "Green — 4PC";
                default: return string.Empty;
            }
        }

        // ── Helper class สำหรับ dropdown แผนก/ฝ่าย ─────────────────────────
        private class OrgUnitItem
        {
            public int    PrincipalType { get; private set; }
            public int    Id            { get; private set; }
            public string Name          { get; private set; }
            public OrgUnitItem(int principalType, int id, string name)
            {
                PrincipalType = principalType;
                Id   = id;
                Name = name;
            }
            public override string ToString() { return Name; }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1024L * 1024 * 1024)
                return string.Format("{0:N2} GB", bytes / (1024.0 * 1024 * 1024));
            if (bytes >= 1024L * 1024)
                return string.Format("{0:N2} MB", bytes / (1024.0 * 1024));
            if (bytes >= 1024)
                return string.Format("{0:N1} KB", bytes / 1024.0);
            return bytes > 0 ? string.Format("{0} B", bytes) : "—";
        }
    }
}
