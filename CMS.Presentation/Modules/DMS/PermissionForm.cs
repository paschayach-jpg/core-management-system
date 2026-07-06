using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Controllers.DMS.Document;
using CMS.Controllers.DMS.Document.Interfaces;
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
        private readonly IAuditLogController _auditLogController;
        private UserContext _currentUser;
        private Folder _folder;
        private readonly IFolderPermissionLookupController _folderPermissionLookupController;
        private List<FolderPermission> _aclEntries = new List<FolderPermission>();
        private List<FolderPermission> _allAclEntries = new List<FolderPermission>(); // includes revoked
        private bool _showAllEntries = false;
        private readonly IUserAccountLookupController _userAccountController;
        private readonly IFolderSettingsController _folderSettingsController;
        private List<UserAccount> _userList = new List<UserAccount>();
        private readonly IApprovalWorkflowController _approvalWorkflowController;
        private Dictionary<int, string> _userDisplayMap = new Dictionary<int, string>();
        private CMS.Services.Interfaces.IEmployeeApiService _employeeApiService;
        private readonly IUserAccessOrgLookupController _orgLookupController;
        private Dictionary<int, string> _departmentNameMap = new Dictionary<int, string>(); // DepartmentId -> Name (Employee API)
        private Dictionary<int, string> _divisionNameMap = new Dictionary<int, string>();   // DivisionId -> Name (Employee API)
        private Dictionary<int, string> _orgUnitNameMap = new Dictionary<int, string>();    // OrgUnitID -> Name (DB)

        // Parent folder — used to determine whether the conf-level dropdown is editable.
        private readonly Folder _parentFolder;

        // ── Constructor ──────────────────────────────────────────────────────

        public PermissionForm(
            IPermissionService permissionService,
            IApprovalService approvalService,
            IFolderService folderService,
            IFolderPermissionLookupController folderPermissionLookupController,
            UserContext user,
            Folder folder,
            IUserAccountLookupController userAccountLookupController = null,
            IFolderSettingsController folderSettingsController = null,
            CMS.Services.Interfaces.IEmployeeApiService employeeApiService = null,
            IUserAccessOrgLookupController orgLookupController = null,
            IFileDocumentLookupController fileDocumentLookupController = null,
            IAuditService auditService = null,
            Folder parentFolder = null)
        {
            if (permissionService == null) throw new ArgumentNullException("permissionService");
            if (approvalService == null) throw new ArgumentNullException("approvalService");
            if (folderService == null) throw new ArgumentNullException("folderService");
            _folderPermissionLookupController = folderPermissionLookupController ?? throw new ArgumentNullException("folderPermissionLookupController");
            if (user == null) throw new ArgumentNullException("user");

            _folder = folder;
            _parentFolder = parentFolder;
            _auditLogController = new AuditLogController(auditService);
            _currentUser = user;
            _userAccountController = userAccountLookupController;
            _folderSettingsController = folderSettingsController;
            _userList = new List<UserAccount>();
            _approvalWorkflowController = new ApprovalWorkflowController(approvalService);
            _employeeApiService = employeeApiService;
            _orgLookupController = orgLookupController;

            // Pre-load user display map
            if (_userAccountController != null)
            {
                try
                {
                    foreach (var u in _userAccountController.GetAllUsers())
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
            if (_orgLookupController != null)
            {
                try
                {
                    foreach (var u in _orgLookupController.GetHierarchy())
                        _orgUnitNameMap[u.OrgUnitID] = u.OrgUnitNameTH ?? u.OrgUnitNameEN ?? u.OrgUnitCode ?? u.OrgUnitID.ToString();
                }
                catch { }
            }

            // เพิ่มชื่อ Department/Division จาก Employee API เข้า _orgUnitNameMap
            // เพื่อให้แสดงชื่อแทน "Department ID: X"
            if (_employeeApiService != null)
            {
                try
                {
                    foreach (var dept in _employeeApiService.GetDepartments())
                        if (!_orgUnitNameMap.ContainsKey(dept.DepartmentId))
                            _orgUnitNameMap[dept.DepartmentId] = dept.DisplayName ?? dept.NameTh ?? dept.DepartmentCode;

                    foreach (var div in _employeeApiService.GetDivisions())
                        if (!_orgUnitNameMap.ContainsKey(div.DivisionId))
                            _orgUnitNameMap[div.DivisionId] = div.DisplayName ?? div.NameTh ?? div.DivisionCode;
                }
                catch { }
            }

            InitializeComponent();
            BuildTracePanel();
            // ซ่อน column Full Control — ไม่มีการใช้งาน
            colFull.Width = 0;

            _presenter = new PermissionPresenter(
                permissionService, approvalService, folderService, folderPermissionLookupController, this, user,
                fileDocumentLookupController, auditService);
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
                lblColorText.Text = ConfidentialityColorLabel(lvl);

                // ── Confidentiality-level edit rules ─────────────────────────
                // Rule: เปลี่ยนระดับความลับได้เฉพาะ Folder ที่อยู่ภายใต้ Conf-Root โดยตรง
                //  - Conf-Root เอง (ParentFolderID = null) → Lock
                //  - Sub-folder ลึกกว่า 1 ชั้น (parent ไม่ใช่ Conf-Root) → Lock
                bool isConfRoot = _folder.ParentFolderID == null;
                bool parentIsConfRoot = _parentFolder != null && _parentFolder.ParentFolderID == null;
                bool canChangeLevel = !isConfRoot && (_parentFolder == null || parentIsConfRoot);
                if (!canChangeLevel)
                {
                    cboConfLevel.Enabled = false;
                    cboConfLevel.BackColor = System.Drawing.SystemColors.Control;
                    string reason = isConfRoot
                        ? "Conf-Root Folder ระดับความลับถูก Lock ไม่สามารถเปลี่ยนได้"
                        : "เปลี่ยนระดับความลับได้เฉพาะ Folder ที่อยู่ภายใต้ Conf-Root โดยตรงเท่านั้น";
                    var tip = new ToolTip();
                    tip.SetToolTip(cboConfLevel, reason);
                    tip.SetToolTip(lblLevelLabel, reason);
                }

                txtFolderName.Text = _folder.FolderName ?? string.Empty;
                txtPhysicalPath.Text = _folder.PhysicalPath ?? string.Empty;

                // Size + quota
                string sizeStr = FormatBytes(_folder.FolderSizeBytes);
                if (_folder.QuotaBytes.HasValue && _folder.QuotaBytes.Value > 0)
                    lblSizeValue.Text = string.Format("{0} (จาก {1} quota)", sizeStr, FormatBytes(_folder.QuotaBytes.Value));
                else
                    lblSizeValue.Text = sizeStr;

                chkInheritPerm.Checked = _folder.InheritPermission;

                // ── Tab สิทธิ์ — โหลดข้อมูลสิทธิ์ทันที ────────────────────────────
                DiagLog(string.Format("=== FORM LOAD: FolderID={0}  Name={1}  Level={2} ===",
                    _folder.FolderID, _folder.FolderName, _folder.ConfidentialityLevel));
                LoadPermissionsImmediate();
            }

            tabControl.SelectedTab = tabGeneral;
        }

        /// <summary>
        /// โหลดข้อมูลสิทธิ์ทันทีแบบง่าย ๆ โดยไม่ผ่าน presenter
        /// </summary>
        private void LoadPermissionsImmediate()
        {
            int folderId = 0;

            if (_folder == null)
            {
                DiagLog("ERROR: _folder is null — cannot load permissions");
                return;
            }

            folderId = _folder.FolderID;
            DiagLog(string.Format("── LoadPermissionsImmediate START: FolderID={0} ──", folderId));

            // ── ตรวจสอบ User ──────────────────────────────────────────────────
            var currentUser = _presenter?.User;
            if (currentUser == null)
            {
                DiagLog("  [CHECK-USER] ❌ ไม่พบข้อมูล User — presenter.User เป็น null");
            }
            else
            {
                DiagLog(string.Format("  [CHECK-USER] ✅ UserID={0}  Name={1}  Role={2}",
                    currentUser.UserID,
                    currentUser.ADUserName,
                    currentUser.AppRole));
            }

            // ── ตรวจสอบ Folder และ Set ให้ Presenter ─────────────────────────
            if (_presenter != null)
            {
                _presenter.SetCurrentFolder(_folder);
                DiagLog(string.Format("  [CHECK-FOLDER] ✅ SetCurrentFolder: FolderID={0}  Name={1}  Level={2}",
                    _folder.FolderID, _folder.FolderName, _folder.ConfidentialityLevel));
            }
            else
            {
                DiagLog("  [CHECK-FOLDER] ❌ _presenter เป็น null");
            }

            try
            {
                // Clear existing data
                aclListView.Items.Clear();
                _allAclEntries = new List<FolderPermission>();

                // Step 1: Query DB first
                DiagLog("Step 1: Querying DB via PermissionRepository.GetAllByFolder...");
                var allPermissions = _folderPermissionLookupController.GetAllByFolder(folderId).ToList();

                if (allPermissions.Count == 0)
                {
                    DiagLog(string.Format("  [SP-RESULT] ⚠ ไม่พบข้อมูลสิทธิ์ใน DB สำหรับ FolderID={0} (0 รายการ)", folderId));
                    DiagLog("  → ยังไม่มีการตั้งสิทธิ์ หรือ FolderID อาจไม่ถูกต้อง");
                    DiagLog("  → กด [+ เพิ่ม] เพื่อเพิ่มรายการสิทธิ์ใหม่");
                }
                else
                {
                    DiagLog(string.Format("  [SP-RESULT] ✅ พบ {0} รายการ — กำลังแสดงข้อมูล...", allPermissions.Count));
                }

                foreach (var p in allPermissions)
                    DiagLog(string.Format("  [DB Row] PermID={0} Type={1} PrincipalID={2} Read={3} Write={4} Revoked={5}",
                        p.PermissionID, p.PrincipalType, p.PrincipalID,
                        p.CanRead, p.CanWrite, p.RevokedDate.HasValue ? p.RevokedDate.Value.ToString("dd/MM/yy") : "No"));

                // Step 2: store entries then load display names
                _allAclEntries = allPermissions;
                DiagLog("Step 2: Loading display name maps (users, org units)...");
                LoadDisplayNames();
                DiagLog(string.Format("  UserDisplayMap={0} entries, OrgUnitNameMap={1} entries",
                    _userDisplayMap.Count, _orgUnitNameMap.Count));

                // Active = not revoked, not expired
                var active = allPermissions
                    .Where(p => p.RevokedDate == null
                             && !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today))
                    .ToList();
                var revoked = allPermissions.Where(p => p.RevokedDate != null).ToList();
                var expired = allPermissions.Where(p => p.RevokedDate == null
                                                      && p.ExpiryDate.HasValue
                                                      && p.ExpiryDate.Value.Date < DateTime.Today).ToList();

                DiagLog(string.Format("Step 3: Categorised → Active={0}, Revoked={1}, Expired={2}",
                    active.Count, revoked.Count, expired.Count));

                // _allAclEntries = ทั้งหมด (ใช้เมื่อกด "แสดงทั้งหมด")
                // _aclEntries    = เฉพาะที่ใช้งาน (ค่าเริ่มต้นที่แสดง)
                _allAclEntries = allPermissions;
                _aclEntries = active;

                // Populate ListView — แสดงเฉพาะรายการที่ใช้งาน
                DiagLog(string.Format("Step 4: Populating ListView (active only: {0} รายการ)...", active.Count));
                int addedOk = 0, addedFail = 0;
                foreach (var perm in _aclEntries)
                {
                    try
                    {
                        AddPermissionItemToListView(perm);
                        addedOk++;
                    }
                    catch (Exception rowEx)
                    {
                        addedFail++;
                        DiagLog(string.Format("  ERROR adding PermID={0}: {1}", perm.PermissionID, rowEx.Message));
                    }
                }

                DiagLog(string.Format("Step 4 result: added={0}, failed={1}, ListView.Items.Count={2}",
                    addedOk, addedFail, aclListView.Items.Count));

                // Update status
                UpdatePermissionStatusLabel(active.Count, 0);
                EnsureShowAllCheckbox();

                DiagLog(string.Format("── LoadPermissionsImmediate END ──────────────────────────────────"));
            }
            catch (Exception ex)
            {
                DiagLog(string.Format("EXCEPTION: {0}: {1}", ex.GetType().Name, ex.Message));
                DiagLog("StackTrace: " + ex.StackTrace);
                ShowError("ไม่สามารถโหลดข้อมูลสิทธิ์ได้: " + ex.Message);
            }
        }

        // ── IPermissionView ──────────────────────────────────────────────────

        public void ShowAclEntries(IEnumerable<FolderPermission> entries)
        {
            try
            {
                var list = entries == null ? new List<FolderPermission>() : entries.ToList();
                DiagLog(string.Format("── ShowAclEntries called with {0} entries ──", list.Count));

                _allAclEntries = list;
                // แสดงเฉพาะรายการที่ใช้งาน (ไม่ถูก revoke และไม่หมดอายุ)
                _aclEntries = list
                    .Where(p => p.RevokedDate == null
                             && !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today))
                    .ToList();

                DiagLog(string.Format("  Active={0}, Hidden(revoked/expired)={1}",
                    _aclEntries.Count, list.Count - _aclEntries.Count));

                LoadDisplayNames();

                aclListView.Items.Clear();

                int ok = 0, fail = 0;
                foreach (var perm in _aclEntries)
                {
                    try { AddPermissionItemToListView(perm); ok++; }
                    catch (Exception ex)
                    {
                        fail++;
                        DiagLog(string.Format("  ERROR row PermID={0}: {1}", perm.PermissionID, ex.Message));
                    }
                }

                DiagLog(string.Format("  ListView after render: added={0} failed={1} total={2}",
                    ok, fail, aclListView.Items.Count));

                UpdatePermissionStatusLabel(_allAclEntries.Count, 0);
                EnsureShowAllCheckbox();
            }
            catch (Exception ex)
            {
                DiagLog(string.Format("EXCEPTION in ShowAclEntries: {0}", ex.Message));
                ShowError("เกิดข้อผิดพลาดในการแสดงรายการสิทธิ์: " + ex.Message);
            }
        }

        private void RenderAclList()
        {
            aclListView.Items.Clear();
            var toShow = _showAllEntries ? _allAclEntries : _aclEntries;

            foreach (var entry in toShow)
            {
                bool isDeny = entry.IsDeny;
                bool isRevoked = entry.RevokedDate.HasValue;

                string principalName;
                if (entry.PrincipalType == PrincipalType.User)
                {
                    // User — try map first, then GetById (covers inactive users)
                    if (!_userDisplayMap.TryGetValue(entry.PrincipalId, out principalName))
                    {
                        principalName = ResolveUserName(entry.PrincipalId);
                        _userDisplayMap[entry.PrincipalId] = principalName; // cache
                    }
                }
                else if (entry.PrincipalType == PrincipalType.Department)
                {
                    // Department — PrincipalId is OrgUnitID
                    if (!_orgUnitNameMap.TryGetValue(entry.PrincipalId, out principalName))
                        principalName = string.Format("DeptID: {0}", entry.PrincipalId);
                }
                else if (entry.PrincipalType == PrincipalType.Division)
                {
                    // Division — PrincipalId is OrgUnitID
                    if (!_orgUnitNameMap.TryGetValue(entry.PrincipalId, out principalName))
                        principalName = string.Format("DivID: {0}", entry.PrincipalId);
                }
                else
                {
                    principalName = string.Format("{0} #{1}", PrincipalTypeLabel((int)entry.PrincipalType), entry.PrincipalId);
                }

                var item = new ListViewItem(isDeny ? "⊘ " + principalName : principalName);
                item.SubItems.Add(PrincipalTypeLabel((int)entry.PrincipalType));
                item.SubItems.Add(PermCheck(entry.CanRead, isDeny));
                item.SubItems.Add(PermCheck(entry.CanWrite, isDeny));
                item.SubItems.Add(PermCheck(entry.CanDelete, isDeny));
                item.SubItems.Add(PermCheck(entry.CanModify, isDeny));
                item.SubItems.Add("—"); // colFull — ซ่อน (Width=0)
                item.SubItems.Add(entry.ExpiryDate.HasValue ? entry.ExpiryDate.Value.ToString("dd/MM/yy") : "—");

                // Status column
                bool isExpired = !isRevoked && entry.ExpiryDate.HasValue && entry.ExpiryDate.Value.Date < DateTime.Today;

                if (isRevoked)
                {
                    item.SubItems.Add("ยกเลิกแล้ว");
                    item.ForeColor = Color.Gray;
                    item.Font = new Font(aclListView.Font, FontStyle.Strikeout);
                }
                else if (isExpired)
                {
                    item.SubItems.Add("⚠ หมดอายุ");
                    item.ForeColor = Color.FromArgb(160, 90, 0);
                    item.Font = new Font(aclListView.Font, FontStyle.Strikeout);
                    item.BackColor = Color.FromArgb(255, 248, 215);
                }
                else if (isDeny)
                {
                    item.SubItems.Add("✔ ใช้งาน (Deny)");
                    item.ForeColor = Color.Crimson;
                    item.Font = new Font(aclListView.Font, FontStyle.Bold);
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
            int totalCount = _allAclEntries.Count;
            if (activeCount == 0 && !_showAllEntries)
            {
                lblAclStatus.Text = "ยังไม่มีสิทธิ์ — กด + เพิ่ม เพื่อเพิ่มรายการ";
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

        // ── Trace log ─────────────────────────────────────────────────────────
        private readonly System.Text.StringBuilder _diagLog = new System.Text.StringBuilder();
        private System.Windows.Forms.RichTextBox _rtbTrace;

        private void DiagLog(string msg)
        {
            string line = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss.fff"), msg);
            _diagLog.AppendLine(line);
            System.Diagnostics.Debug.WriteLine("[VIEW] " + msg);

            if (_rtbTrace != null)
            {
                _rtbTrace.AppendText(line + "\r\n");
                _rtbTrace.ScrollToCaret();
            }
        }

        private void BuildTracePanel()
        {
            // Collapsible trace panel docked to Bottom of tabPermission
            var pnlTrace = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Bottom,
                Height = 110,
                BackColor = System.Drawing.Color.FromArgb(240, 240, 245),
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            };

            var lblHeader = new System.Windows.Forms.Label
            {
                Text = "📋 Trace Log (Debug Output)",
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 20,
                Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(0, 51, 102),
                Padding = new System.Windows.Forms.Padding(4, 2, 0, 0)
            };

            var btnClear = new System.Windows.Forms.Button
            {
                Text = "Clear",
                Width = 50,
                Height = 18,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 7f),
                Dock = System.Windows.Forms.DockStyle.None,
                Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right
            };
            btnClear.Location = new System.Drawing.Point(pnlTrace.Width - 56, 1);
            btnClear.Click += (s, e) => { _rtbTrace.Clear(); _diagLog.Clear(); };

            _rtbTrace = new System.Windows.Forms.RichTextBox
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                ReadOnly = true,
                Font = new System.Drawing.Font("Consolas", 7.5f),
                BackColor = System.Drawing.Color.FromArgb(248, 248, 255),
                BorderStyle = System.Windows.Forms.BorderStyle.None,
                ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical
            };

            pnlTrace.Controls.Add(_rtbTrace);
            pnlTrace.Controls.Add(btnClear);
            pnlTrace.Controls.Add(lblHeader);
            tabPermission.Controls.Add(pnlTrace);
        }
        private void EnsureShowAllCheckbox()
        {
            if (_chkShowAll != null) return; // already added

            _chkShowAll = new CheckBox
            {
                Text = "แสดงทั้งหมด (รวมที่ยกเลิก)",
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(80, 80, 80),
                Checked = false
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
                item.Font = new Font(aclListView.Font, FontStyle.Italic);
                item.Tag = req;  // tag เป็น ApprovalRequest ไม่ใช่ FolderPermission

                aclListView.Items.Add(item);
            }

            // Update count label — only show "มีคำขอรออนุมัติ" when pending requests actually exist
            int pendingCount = aclListView.Items.Cast<ListViewItem>().Count(i => i.Tag is ApprovalRequest);
            if (pendingCount > 0)
            {
                lblAclStatus.Text = string.Format("{0} รายการ  (รออนุมัติ {1})", _aclEntries.Count, pendingCount);
                lblAclStatus.ForeColor = Color.FromArgb(160, 90, 0);
            }
            else
            {
                UpdatePermissionStatusLabel(_allAclEntries.Count, 0);
            }
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
            if (_folder == null)
            {
                ShowError("กรุณาเลือก Folder ก่อนเพิ่มสิทธิ์");
                return;
            }

            var reqs = PromptForPermissionRequests(_folder.FolderId, null);
            if (reqs != null)
            {
                foreach (var req in reqs)
                    _presenter.SetPermission(req);

                // Refresh permissions immediately after adding
                LoadPermissionsImmediate();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (aclListView.SelectedItems.Count == 0)
            {
                ShowError("กรุณาเลือกรายการที่ต้องการแก้ไข");
                return;
            }

            var entry = aclListView.SelectedItems[0].Tag as FolderPermission;
            if (entry == null)
            {
                ShowError("ไม่สามารถแก้ไขรายการที่รออนุมัติได้");
                return;
            }

            if (_folder == null)
            {
                // Fallback: ใช้ FolderId จาก entry ที่เลือก
                var editReqs = PromptForPermissionRequests(entry.FolderID, entry);
                if (editReqs != null)
                {
                    foreach (var req in editReqs)
                        _presenter.SetPermission(req);

                    // Refresh permissions immediately after editing
                    LoadPermissionsImmediate();
                }
                return;
            }

            var reqs = PromptForPermissionRequests(_folder.FolderId, entry);
            if (reqs != null)
            {
                foreach (var req in reqs)
                    _presenter.SetPermission(req);

                // Refresh permissions immediately after editing
                LoadPermissionsImmediate();
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (_folder != null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== REFRESH PERMISSIONS START ===");
                    System.Diagnostics.Debug.WriteLine($"Folder ID: {_folder.FolderID}");

                    // Use the new immediate loading system
                    LoadPermissionsImmediate();

                    ShowInfo("รีเฟรชข้อมูลสิทธิ์เรียบร้อยแล้ว");
                    System.Diagnostics.Debug.WriteLine($"=== REFRESH PERMISSIONS END ===");
                }
                catch (Exception ex)
                {
                    ShowError("ไม่สามารถรีเฟรชข้อมูลได้: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine($"btnRefresh_Click Error: {ex.Message}");
                }
            }
        }

        private void ForceReloadPermissions()
        {
            if (_folder == null) return;

            try
            {
                // Get all permissions for this folder (including revoked)
                var allPermissions = _folderPermissionLookupController.GetAllByFolder(_folder.FolderID).ToList();

                // Get pending approval requests
                var pendingApprovals = _approvalWorkflowController.GetAllApprovalsByFolder(_folder.FolderID)
                    .Where(r => r.RequestStatus == 1 && r.RequestType == "CHANGE_PERMISSION")
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"ForceReloadPermissions: Found {allPermissions.Count} permissions, {pendingApprovals.Count} pending approvals for folder {_folder.FolderID}");

                // Update the internal collections
                _allAclEntries = allPermissions;
                _aclEntries = allPermissions
                    .Where(e => e.RevokedDate == null
                             && !(e.ExpiryDate.HasValue && e.ExpiryDate.Value.Date < DateTime.Today))
                    .ToList();

                // Load display names BEFORE rendering so principal names are resolved
                LoadDisplayNames();

                // Render the permission list
                RenderAclList();

                // Add pending approvals to the display
                ShowPendingApprovals(pendingApprovals);

                // Ensure show all checkbox is available
                EnsureShowAllCheckbox();
            }
            catch (Exception ex)
            {
                ShowError("เกิดข้อผิดพลาดในการดึงข้อมูลจากฐานข้อมูล: " + ex.Message);
                System.Diagnostics.Debug.WriteLine($"ForceReloadPermissions Error: {ex.Message}");
            }
        }

        private void LoadDisplayNames()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== LoadDisplayNames START ===");

                // Initialize display maps if they don't exist
                if (_userDisplayMap == null) _userDisplayMap = new Dictionary<int, string>();
                if (_orgUnitNameMap == null) _orgUnitNameMap = new Dictionary<int, string>();

                if (_allAclEntries == null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadDisplayNames: _allAclEntries is null, returning");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"LoadDisplayNames: Processing {_allAclEntries.Count} ACL entries");

                // Reload user names from repository
                if (_userAccountController != null)
                {
                    try
                    {
                        var users = _userAccountController.GetAllUsers().ToList();
                        System.Diagnostics.Debug.WriteLine($"LoadDisplayNames: Found {users.Count} users from repository");

                        foreach (var u in users)
                        {
                            string displayName = string.IsNullOrEmpty(u.DisplayName) ? u.ADUserName : u.DisplayName;
                            _userDisplayMap[u.UserID] = displayName;
                            System.Diagnostics.Debug.WriteLine($"  User {u.UserID}: {displayName}");
                        }

                        System.Diagnostics.Debug.WriteLine($"LoadDisplayNames: _userDisplayMap now has {_userDisplayMap.Count} entries");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"LoadDisplayNames - user repo error: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("LoadDisplayNames: _userAccountController is null");
                }

                // Reload org unit names from service
                if (_orgLookupController != null)
                {
                    try
                    {
                        var orgUnits = _orgLookupController.GetHierarchy().ToList();
                        System.Diagnostics.Debug.WriteLine($"LoadDisplayNames: Found {orgUnits.Count} org units from service");

                        foreach (var u in orgUnits)
                        {
                            string displayName = u.OrgUnitNameTH ?? u.OrgUnitNameEN ?? u.OrgUnitCode ?? u.OrgUnitID.ToString();
                            _orgUnitNameMap[u.OrgUnitID] = displayName;
                            System.Diagnostics.Debug.WriteLine($"  OrgUnit {u.OrgUnitID}: {displayName}");
                        }

                        System.Diagnostics.Debug.WriteLine($"LoadDisplayNames: _orgUnitNameMap now has {_orgUnitNameMap.Count} entries");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"LoadDisplayNames - org service error: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("LoadDisplayNames: _orgStructureService is null");
                }

                // Fill in any remaining unknown IDs — try GetById first (covers inactive users)
                foreach (var e in _allAclEntries)
                {
                    if (e.PrincipalType == PrincipalType.User && !_userDisplayMap.ContainsKey(e.PrincipalID))
                    {
                        string resolved = null;
                        if (_userAccountController != null)
                        {
                            try
                            {
                                var u = _userAccountController.GetUserById(e.PrincipalID);
                                if (u != null)
                                    resolved = string.IsNullOrEmpty(u.DisplayName) ? u.ADUserName : u.DisplayName;
                            }
                            catch { }
                        }
                        string fallback = resolved ?? string.Format("UserID: {0}", e.PrincipalID);
                        _userDisplayMap[e.PrincipalID] = fallback;
                        System.Diagnostics.Debug.WriteLine($"  Resolved user {e.PrincipalID}: {fallback}");
                    }
                    else if ((e.PrincipalType == PrincipalType.Department || e.PrincipalType == PrincipalType.Division)
                             && !_orgUnitNameMap.ContainsKey(e.PrincipalID))
                    {
                        string fallback = string.Format("{0} ID: {1}", e.PrincipalType, e.PrincipalID);
                        _orgUnitNameMap[e.PrincipalID] = fallback;
                        System.Diagnostics.Debug.WriteLine($"  Added fallback org unit {e.PrincipalID}: {fallback}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"=== LoadDisplayNames END - Users: {_userDisplayMap.Count}, OrgUnits: {_orgUnitNameMap.Count} ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDisplayNames Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        private void RefreshPermissionDisplay()
        {
            if (_folder == null) return;

            try
            {
                // Get all permissions for this folder (including revoked ones)
                var allPermissions = _folderPermissionLookupController.GetAllByFolder(_folder.FolderID).ToList();

                // Get all approval requests for this folder
                var allApprovals = _approvalWorkflowController.GetAllApprovalsByFolder(_folder.FolderID).ToList();
                var pendingApprovals = allApprovals
                    .Where(r => r.RequestStatus == 1 && r.RequestType == "CHANGE_PERMISSION")
                    .OrderByDescending(r => r.RequestedDate)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"RefreshPermissionDisplay: Found {allPermissions.Count} permissions, {pendingApprovals.Count} pending approvals");

                // Update internal collections - DON'T FILTER, SHOW ALL
                _allAclEntries = allPermissions;
                _aclEntries = allPermissions; // Show ALL permissions, not just active ones

                // Load display names BEFORE rendering so principal names are resolved
                LoadDisplayNames();

                // Clear ListView and manually add all items
                aclListView.Items.Clear();

                // Add all permissions (including revoked ones)
                foreach (var perm in allPermissions)
                {
                    AddPermissionItemToListView(perm);
                }

                // Add pending approvals
                foreach (var approval in pendingApprovals)
                {
                    AddApprovalItemToListView(approval);
                }

                // Update status
                UpdatePermissionStatusLabel(allPermissions.Count, pendingApprovals.Count);

                // Log for debugging
                System.Diagnostics.Debug.WriteLine($"RefreshPermissionDisplay: UI now shows {aclListView.Items.Count} items");
            }
            catch (Exception ex)
            {
                ShowError("เกิดข้อผิดพลาดในการดึงข้อมูล: " + ex.Message);
                System.Diagnostics.Debug.WriteLine($"RefreshPermissionDisplay Error: {ex.Message}");
            }
        }

        private void AddPermissionItemToListView(FolderPermission perm)
        {
            string principalName = GetPrincipalName(perm.PrincipalType, perm.PrincipalID);
            bool isDeny = perm.IsDeny;
            bool isRevoked = perm.RevokedDate.HasValue;
            bool isExpired = !isRevoked && perm.ExpiryDate.HasValue && perm.ExpiryDate.Value.Date < DateTime.Today;

            DiagLog(string.Format("  Row PermID={0} {1}#{2} Name={3} Revoked={4} Expired={5} Deny={6}",
                perm.PermissionID, perm.PrincipalType, perm.PrincipalID,
                principalName, isRevoked, isExpired, isDeny));

            var item = new ListViewItem(isDeny ? "⊘ " + principalName : principalName);
            item.SubItems.Add(GetPrincipalTypeText(perm.PrincipalType));
            item.SubItems.Add(perm.CanRead ? (isDeny ? "—" : "✓") : "—");
            item.SubItems.Add(perm.CanWrite ? (isDeny ? "—" : "✓") : "—");
            item.SubItems.Add(perm.CanDelete ? (isDeny ? "—" : "✓") : "—");
            item.SubItems.Add(perm.CanModify ? (isDeny ? "—" : "✓") : "—");
            item.SubItems.Add("—"); // colFull — ซ่อน (Width=0)
            item.SubItems.Add(perm.ExpiryDate.HasValue ? perm.ExpiryDate.Value.ToString("dd/MM/yy") : "—");

            if (isRevoked)
            {
                item.SubItems.Add("ยกเลิกแล้ว");
                item.ForeColor = Color.Gray;
                item.Font = new Font(aclListView.Font, FontStyle.Strikeout);
            }
            else if (isExpired)
            {
                item.SubItems.Add("⚠ หมดอายุ");
                item.ForeColor = Color.FromArgb(160, 90, 0);
                item.Font = new Font(aclListView.Font, FontStyle.Strikeout);
                item.BackColor = Color.FromArgb(255, 248, 215);
            }
            else if (isDeny)
            {
                item.SubItems.Add("✔ ใช้งาน (Deny)");
                item.ForeColor = Color.Crimson;
                item.Font = new Font(aclListView.Font, FontStyle.Bold);
            }
            else
            {
                item.SubItems.Add("✔ ใช้งาน");
                item.ForeColor = Color.FromArgb(0, 120, 0);
            }

            item.Tag = perm;
            aclListView.Items.Add(item);
        }

        private void AddApprovalItemToListView(ApprovalRequest approval)
        {
            try
            {
                string dateStr = approval.RequestedDate.ToString("dd/MM/yy HH:mm");
                string subject = approval.Subject ?? approval.RequestType;

                var item = new ListViewItem($"⏳ {subject}");
                item.SubItems.Add("รออนุมัติ");
                item.SubItems.Add("—");
                item.SubItems.Add("—");
                item.SubItems.Add("—");
                item.SubItems.Add("—");
                item.SubItems.Add("—");
                item.SubItems.Add(dateStr);
                item.SubItems.Add("รออนุมัติ");

                item.BackColor = Color.FromArgb(255, 248, 215);
                item.ForeColor = Color.FromArgb(160, 90, 0);
                item.Font = new Font(aclListView.Font, FontStyle.Italic);
                item.Tag = approval;

                aclListView.Items.Add(item);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddApprovalItemToListView Error: {ex.Message}");
            }
        }

        private string ResolveUserName(int userId)
        {
            if (_userAccountController != null)
            {
                try
                {
                    var u = _userAccountController.GetUserById(userId);
                    if (u != null)
                        return string.IsNullOrEmpty(u.DisplayName) ? u.ADUserName : u.DisplayName;
                }
                catch { }
            }
            return string.Format("UserID: {0}", userId);
        }

        private string GetPrincipalName(PrincipalType principalType, int principalId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetPrincipalName: Type={principalType}, ID={principalId}");

                switch (principalType)
                {
                    case PrincipalType.User:
                        if (!_userDisplayMap.TryGetValue(principalId, out string userName))
                        {
                            userName = ResolveUserName(principalId);
                            _userDisplayMap[principalId] = userName; // cache
                        }
                        System.Diagnostics.Debug.WriteLine($"  User lookup: Result={userName}");
                        return userName;

                    case PrincipalType.Department:
                        bool deptFound = _orgUnitNameMap.TryGetValue(principalId, out string deptName);
                        string deptResult = deptFound ? deptName : $"Department {principalId}";
                        System.Diagnostics.Debug.WriteLine($"  Department lookup: Found={deptFound}, Result={deptResult}");
                        return deptResult;

                    case PrincipalType.Division:
                        bool divFound = _orgUnitNameMap.TryGetValue(principalId, out string divName);
                        string divResult = divFound ? divName : $"Division {principalId}";
                        System.Diagnostics.Debug.WriteLine($"  Division lookup: Found={divFound}, Result={divResult}");
                        return divResult;

                    default:
                        string defaultResult = $"{principalType} {principalId}";
                        System.Diagnostics.Debug.WriteLine($"  Default result: {defaultResult}");
                        return defaultResult;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetPrincipalName Error: {ex.Message}");
                return $"ID: {principalId}";
            }
        }

        private string GetPrincipalTypeText(PrincipalType principalType)
        {
            switch (principalType)
            {
                case PrincipalType.User: return "ผู้ใช้";
                case PrincipalType.Department: return "แผนก";
                case PrincipalType.Division: return "ฝ่าย";
                default: return principalType.ToString();
            }
        }

        private void UpdatePermissionStatusLabel(int permissionCount, int pendingCount)
        {
            try
            {
                int activeCount = _aclEntries?.Count(p => p.RevokedDate == null) ?? 0;
                int revokedCount = permissionCount - activeCount;

                if (permissionCount == 0 && pendingCount == 0)
                {
                    lblAclStatus.Text = "ยังไม่มีสิทธิ์ — กด + เพิ่ม เพื่อเพิ่มรายการ";
                    lblAclStatus.ForeColor = Color.Gray;
                }
                else
                {
                    string statusText = $"{permissionCount + pendingCount} รายการ";
                    if (activeCount > 0) statusText += $" (ใช้งาน {activeCount}";
                    if (revokedCount > 0) statusText += $" / ยกเลิก {revokedCount}";
                    if (pendingCount > 0) statusText += $" / รออนุมัติ {pendingCount}";
                    if (activeCount > 0 || revokedCount > 0) statusText += ")";

                    lblAclStatus.Text = statusText;
                    lblAclStatus.ForeColor = pendingCount > 0 ? Color.FromArgb(160, 90, 0) : Color.FromArgb(0, 51, 102);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePermissionStatusLabel Error: {ex.Message}");
            }
        }

        private void btnForceLoad_Click(object sender, EventArgs e)
        {
            int folderId = 0;
            string folderName = "Unknown";

            // Try to get folder info from _folder first
            if (_folder != null)
            {
                folderId = _folder.FolderID;
                folderName = _folder.FolderName ?? _folder.Name ?? "Unknown";
            }
            else
            {
                // If _folder is null, prompt user for Folder ID
                using (var inputForm = new Form())
                {
                    inputForm.Text = "ป้อน Folder ID";
                    inputForm.Size = new Size(300, 150);
                    inputForm.StartPosition = FormStartPosition.CenterParent;
                    inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    inputForm.MaximizeBox = false;
                    inputForm.MinimizeBox = false;

                    var label = new Label() { Text = "กรุณาป้อน Folder ID:", Location = new Point(10, 20), Size = new Size(200, 20) };
                    var textBox = new TextBox() { Location = new Point(10, 45), Size = new Size(260, 20) };
                    var okButton = new Button() { Text = "OK", Location = new Point(115, 75), Size = new Size(75, 23), DialogResult = DialogResult.OK };
                    var cancelButton = new Button() { Text = "Cancel", Location = new Point(195, 75), Size = new Size(75, 23), DialogResult = DialogResult.Cancel };

                    inputForm.Controls.AddRange(new Control[] { label, textBox, okButton, cancelButton });
                    inputForm.AcceptButton = okButton;
                    inputForm.CancelButton = cancelButton;

                    if (inputForm.ShowDialog(this) != DialogResult.OK ||
                        string.IsNullOrEmpty(textBox.Text) ||
                        !int.TryParse(textBox.Text, out folderId))
                    {
                        ShowError("กรุณาป้อน Folder ID ที่ถูกต้อง");
                        return;
                    }
                }
                folderName = $"Folder ID: {folderId}";
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"=== FORCE LOAD SIMPLE START ===");
                System.Diagnostics.Debug.WriteLine($"Folder ID: {folderId}, Name: {folderName}");

                // Simple direct database query - show ALL permissions including revoked
                var allPermissions = _folderPermissionLookupController.GetAllByFolder(folderId).ToList();
                System.Diagnostics.Debug.WriteLine($"Found {allPermissions.Count} total permissions in database");

                if (allPermissions.Count == 0)
                {
                    ShowInfo($"ไม่พบข้อมูลสิทธิ์ในฐานข้อมูลสำหรับ {folderName} (ID: {folderId})");
                    return;
                }

                // Clear and populate ListView with ALL permissions (including revoked)
                aclListView.Items.Clear();
                _allAclEntries = allPermissions;
                _aclEntries = allPermissions; // Show ALL including revoked

                // Load display names
                LoadDisplayNames();

                // Populate ListView
                foreach (var perm in allPermissions)
                {
                    AddPermissionItemToListView(perm);
                }

                // Update status
                int activeCount = allPermissions.Count(p => p.RevokedDate == null);
                int revokedCount = allPermissions.Count(p => p.RevokedDate.HasValue);
                UpdatePermissionStatusLabel(allPermissions.Count, 0);

                // Show result
                string resultMessage = $"ดึงข้อมูลเรียบร้อยแล้ว!\n\n" +
                                     $"📁 {folderName}\n" +
                                     $"📊 สถิติข้อมูล:\n" +
                                     $"• รายการทั้งหมด: {allPermissions.Count}\n" +
                                     $"• สิทธิ์ที่ใช้งาน: {activeCount}\n" +
                                     $"• สิทธิ์ที่ยกเลิก: {revokedCount}\n\n" +
                                     $"✅ แสดงข้อมูลทั้งหมดในแท็บสิทธิ์แล้ว";

                ShowInfo(resultMessage);
                System.Diagnostics.Debug.WriteLine($"=== FORCE LOAD SIMPLE END ===");
            }
            catch (Exception ex)
            {
                ShowError("เกิดข้อผิดพลาดในการบังคับดึงข้อมูล: " + ex.Message);
                System.Diagnostics.Debug.WriteLine($"btnForceLoad_Click Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        // Helper method to parse approval remark (similar to ApprovalService.ParsePermissionRemark)
        private SetPermissionRequest ParseApprovalRemark(string remark, int folderId)
        {
            if (string.IsNullOrEmpty(remark)) return null;

            try
            {
                var parts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var token in remark.Split(' '))
                {
                    var idx = token.IndexOf(':');
                    if (idx > 0)
                    {
                        string key = token.Substring(0, idx);
                        string value = token.Substring(idx + 1);
                        parts[key] = value;
                    }
                }

                return new SetPermissionRequest
                {
                    FolderID = folderId,
                    PrincipalType = (PrincipalType)GetIntValue(parts, "PrincipalType", 1),
                    PrincipalID = GetIntValue(parts, "PrincipalID", 0),
                    CanRead = GetBoolValue(parts, "Read", false),
                    CanWrite = GetBoolValue(parts, "Write", false),
                    CanDelete = GetBoolValue(parts, "Delete", false),
                    CanModify = GetBoolValue(parts, "Modify", false),
                    CanFullControl = GetBoolValue(parts, "FullControl", false),
                    IsDeny = GetBoolValue(parts, "IsDeny", false)
                };
            }
            catch
            {
                return null;
            }
        }

        private int GetIntValue(Dictionary<string, string> parts, string key, int defaultValue)
        {
            if (parts.ContainsKey(key))
            {
                int result;
                if (int.TryParse(parts[key], out result))
                    return result;
            }
            return defaultValue;
        }

        private bool GetBoolValue(Dictionary<string, string> parts, string key, bool defaultValue)
        {
            if (parts.ContainsKey(key))
            {
                bool result;
                if (bool.TryParse(parts[key], out result))
                    return result;
            }
            return defaultValue;
        }

        private void ShowDiagnosticDialog(string summary)
        {
            var dlg = new Form
            {
                Text = "🔍 Diagnostic & Trace Log",
                Width = 760,
                Height = 560,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = true,
                FormBorderStyle = FormBorderStyle.Sizable
            };

            var tabCtrl = new System.Windows.Forms.TabControl { Dock = DockStyle.Fill };

            // Tab 1 — Summary
            var tabSum = new System.Windows.Forms.TabPage("📊 สรุป");
            var rtbSum = new System.Windows.Forms.RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9f),
                Text = summary
            };
            tabSum.Controls.Add(rtbSum);

            // Tab 2 — Full trace log
            var tabLog = new System.Windows.Forms.TabPage("📋 Trace Log");
            var rtbLog = new System.Windows.Forms.RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 8.5f),
                Text = _diagLog.Length > 0 ? _diagLog.ToString() : "(ยังไม่มี log)"
            };
            tabLog.Controls.Add(rtbLog);

            tabCtrl.TabPages.Add(tabSum);
            tabCtrl.TabPages.Add(tabLog);

            var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 36 };
            var btnClose = new Button { Text = "ปิด", Width = 80, Height = 26, DialogResult = DialogResult.OK };
            btnClose.Location = new System.Drawing.Point(dlg.ClientSize.Width - 90, 5);
            btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            var btnCopyLog = new Button { Text = "Copy Log", Width = 90, Height = 26 };
            btnCopyLog.Location = new System.Drawing.Point(dlg.ClientSize.Width - 186, 5);
            btnCopyLog.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnCopyLog.Click += (s, e2) => { Clipboard.SetText(_diagLog.ToString()); };
            pnlBtn.Controls.Add(btnClose);
            pnlBtn.Controls.Add(btnCopyLog);

            dlg.Controls.Add(tabCtrl);
            dlg.Controls.Add(pnlBtn);
            dlg.ShowDialog(this);
        }

        private void btnDiagnostic_Click(object sender, EventArgs e)
        {
            if (_folder == null)
            {
                ShowError("ไม่มี Folder ที่เลือก");
                return;
            }

            try
            {
                DiagLog("── Manual Diagnostic triggered ──");

                // Run a fresh DB query and log every step
                // Test both methods to see which one works
                var allPermissions = _folderPermissionLookupController.GetAllByFolder(_folder.FolderID);
                var activePermissions = _folderPermissionLookupController.GetByFolder(_folder.FolderID);
                var allApprovals = _approvalWorkflowController.GetAllApprovalsByFolder(_folder.FolderID);
                var pendingApprovals = allApprovals.Where(r => r.RequestStatus == 1 && r.RequestType == "CHANGE_PERMISSION");
                var approvedApprovals = allApprovals.Where(r => r.RequestStatus == 2 && r.RequestType == "CHANGE_PERMISSION");

                // Debug: Check what's in _presenter
                System.Diagnostics.Debug.WriteLine($"=== DIAGNOSTIC DEBUG ===");
                System.Diagnostics.Debug.WriteLine($"Folder ID: {_folder.FolderID}");
                System.Diagnostics.Debug.WriteLine($"Direct GetAllByFolder: {allPermissions.Count()} permissions");
                System.Diagnostics.Debug.WriteLine($"Direct GetByFolder: {activePermissions.Count()} permissions");
                System.Diagnostics.Debug.WriteLine($"All Approvals: {allApprovals.Count()}");
                System.Diagnostics.Debug.WriteLine($"Pending Approvals: {pendingApprovals.Count()}");
                System.Diagnostics.Debug.WriteLine($"Approved Approvals: {approvedApprovals.Count()}");
                System.Diagnostics.Debug.WriteLine($"Current _allAclEntries: {_allAclEntries?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"Current _aclEntries: {_aclEntries?.Count ?? 0}");

                // Log approved approvals details
                foreach (var approval in approvedApprovals)
                {
                    System.Diagnostics.Debug.WriteLine($"Approved Request ID: {approval.RequestID}, Remark: {approval.Remark}");
                }

                // Force reload through presenter to see what happens
                System.Diagnostics.Debug.WriteLine("=== FORCING PRESENTER RELOAD ===");
                _presenter.LoadAclEntries(_folder);
                System.Diagnostics.Debug.WriteLine($"After presenter reload - _allAclEntries: {_allAclEntries?.Count ?? 0}");

                string diagnosticInfo = string.Format(
                    "=== ข้อมูลการวินิจฉัยและดึงข้อมูล ===\n\n" +
                    "Folder ID: {0}\n" +
                    "Folder Name: {1}\n" +
                    "Confidentiality Level: {2} ({3})\n" +
                    "OrgUnit ID: {4}\n\n" +
                    "=== ข้อมูลจากฐานข้อมูล (Direct Query) ===\n" +
                    "GetAllByFolder(): {5} รายการ\n" +
                    "GetByFolder() (Active only): {6} รายการ\n" +
                    "จำนวน Approval Requests ทั้งหมด: {7}\n" +
                    "จำนวน Pending Approvals: {8}\n" +
                    "จำนวน Approved Approvals: {9}\n\n" +
                    "=== สถานะการแสดงผลใน UI ===\n" +
                    "จำนวนรายการใน ListView: {10}\n" +
                    "จำนวนรายการที่เก็บไว้ (_aclEntries): {11}\n" +
                    "จำนวนรายการทั้งหมด (_allAclEntries): {12}\n\n" +
                    "=== รายละเอียด Permissions (GetAllByFolder) ===\n" +
                    "{13}\n\n" +
                    "=== รายละเอียด Active Permissions (GetByFolder) ===\n" +
                    "{14}\n\n" +
                    "=== รายละเอียด Approved Approvals ===\n" +
                    "{15}\n\n" +
                    "=== รายละเอียด Pending Approvals ===\n" +
                    "{16}\n\n" +
                    "🔄 กำลังดึงข้อมูลและแสดงผลอัตโนมัติ...",
                    _folder.FolderID,
                    _folder.FolderName,
                    _folder.ConfidentialityLevel,
                    _folder.ConfidentialityLevel == 1 ? "High Confidential" :
                    _folder.ConfidentialityLevel == 2 ? "Secret" :
                    _folder.ConfidentialityLevel == 3 ? "Normal Confidential" : "Public",
                    _folder.OrgUnitID,
                    allPermissions.Count(),
                    activePermissions.Count(),
                    allApprovals.Count(),
                    pendingApprovals.Count(),
                    approvedApprovals.Count(),
                    aclListView.Items.Count,
                    _aclEntries?.Count ?? 0,
                    _allAclEntries?.Count ?? 0,
                    allPermissions.Any() ?
                        string.Join("\n", allPermissions.Take(5).Select(p =>
                            string.Format("- ID:{0}, Principal:{1}({2}), Read:{3}, Write:{4}, Revoked:{5}",
                                p.PermissionID, p.PrincipalID, p.PrincipalType, p.CanRead, p.CanWrite, p.RevokedDate.HasValue))) :
                        "ไม่มี Permissions",
                    activePermissions.Any() ?
                        string.Join("\n", activePermissions.Take(5).Select(p =>
                            string.Format("- ID:{0}, Principal:{1}({2}), Read:{3}, Write:{4}, Revoked:{5}",
                                p.PermissionID, p.PrincipalID, p.PrincipalType, p.CanRead, p.CanWrite, p.RevokedDate.HasValue))) :
                        "ไม่มี Active Permissions",
                    approvedApprovals.Any() ?
                        string.Join("\n", approvedApprovals.Take(5).Select(a =>
                            string.Format("- ID:{0}, Status:{1}, Date:{2}, Remark:{3}",
                                a.RequestID, a.RequestStatus, a.RequestedDate.ToString("dd/MM/yy HH:mm"),
                                (a.Remark != null ? a.Remark.Substring(0, Math.Min(50, a.Remark.Length)) : "") + "..."))) :
                        "ไม่มี Approved Approvals",
                    pendingApprovals.Any() ?
                        string.Join("\n", pendingApprovals.Take(5).Select(a =>
                            string.Format("- ID:{0}, Type:{1}, Status:{2}, Date:{3}",
                                a.RequestID, a.RequestType, a.RequestStatus, a.RequestedDate.ToString("dd/MM/yy HH:mm")))) :
                        "ไม่มี Pending Approvals"
                );

                // Append fresh diagnostic summary to log
                DiagLog("── Fresh DB query results ──");
                DiagLog(string.Format("  FolderID={0}  GetAllByFolder={1}  GetByFolder(Active)={2}",
                    _folder.FolderID, allPermissions.Count(), activePermissions.Count()));
                DiagLog(string.Format("  Approvals: Total={0} Pending={1} Approved={2}",
                    allApprovals.Count(), pendingApprovals.Count(), approvedApprovals.Count()));
                DiagLog(string.Format("  UI State: ListView={0} _aclEntries={1} _allAclEntries={2}",
                    aclListView.Items.Count, _aclEntries?.Count ?? 0, _allAclEntries?.Count ?? 0));

                // Show diagnostic info + full trace log in a scrollable dialog
                ShowDiagnosticDialog(diagnosticInfo);

                // If we have pending approvals, offer to auto-approve them
                if (pendingApprovals.Any())
                {
                    var autoApproveResult = MessageBox.Show(
                        $"พบ Pending Approval {pendingApprovals.Count()} รายการ\n\n" +
                        "ต้องการอนุมัติอัตโนมัติเพื่อให้สิทธิ์มีผลหรือไม่?\n\n" +
                        "หมายเหตุ: การอนุมัติจะทำให้สิทธิ์ถูก apply เข้าฐานข้อมูลทันที",
                        "Auto-Approve Pending Requests",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (autoApproveResult == DialogResult.Yes)
                    {
                        try
                        {
                            int approvedCount = 0;
                            foreach (var pendingApproval in pendingApprovals)
                            {
                                // Get all steps for this approval
                                var steps = _approvalWorkflowController.GetApprovalSteps(pendingApproval.RequestID);
                                var pendingSteps = steps.Where(s => s.StepStatus == 1).OrderBy(s => s.StepOrder);

                                foreach (var step in pendingSteps)
                                {
                                    // Auto-approve each step
                                    _approvalWorkflowController.ApproveStep(step.StepID, "Auto-approved for testing", _presenter.User);
                                    approvedCount++;
                                    System.Diagnostics.Debug.WriteLine($"Auto-approved step {step.StepID} of request {pendingApproval.RequestID}");
                                }
                            }

                            ShowInfo($"อนุมัติอัตโนมัติเรียบร้อยแล้ว {approvedCount} steps\n\nกำลังรีเฟรชข้อมูลสิทธิ์...");

                            // Refresh permissions after approval
                            LoadPermissionsImmediate();
                        }
                        catch (Exception ex)
                        {
                            ShowError($"เกิดข้อผิดพลาดในการอนุมัติอัตโนมัติ: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Auto-approve error: {ex.Message}");
                        }
                    }
                }
                else if (allPermissions.Any() && (_allAclEntries == null || _allAclEntries.Count == 0))
                {
                    System.Diagnostics.Debug.WriteLine("=== FORCE POPULATING _allAclEntries ===");
                    ShowAclEntries(allPermissions);
                    ShowInfo($"บังคับดึงข้อมูลเรียบร้อยแล้ว - พบ {allPermissions.Count()} รายการ");
                }
                else if (approvedApprovals.Any() && allPermissions.Count() == 0)
                {
                    ShowInfo($"พบ Approval ที่อนุมัติแล้ว {approvedApprovals.Count()} รายการ แต่ไม่มี Permission ในฐานข้อมูล\nอาจจะมีปัญหาในการ apply permission หลังจากอนุมัติ");
                }
                else
                {
                    ShowInfo("ดึงข้อมูลจากฐานข้อมูลเรียบร้อยแล้ว");
                }
            }
            catch (Exception ex)
            {
                ShowError("เกิดข้อผิดพลาดในการวินิจฉัย: " + ex.Message);
                System.Diagnostics.Debug.WriteLine($"Diagnostic Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
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

                // Save confidentiality level — only when the dropdown is enabled (hierarchy allows change)
                if (cboConfLevel.Enabled && cboConfLevel.SelectedIndex >= 0)
                {
                    int newLevel = cboConfLevel.SelectedIndex + 1;
                    if (_folder.ConfidentialityLevel != newLevel)
                    {
                        // Guard: conf-root folders and deep sub-folders cannot change level.
                        bool isConfRoot = _folder.ParentFolderID == null;
                        bool parentIsConfRoot = _parentFolder != null && _parentFolder.ParentFolderID == null;
                        if (isConfRoot || (_parentFolder != null && !parentIsConfRoot))
                        {
                            ShowError("เปลี่ยนระดับความลับได้เฉพาะ Folder ที่อยู่ภายใต้ Conf-Root โดยตรงเท่านั้น");
                            return;
                        }

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
                if (changed && _folderSettingsController != null)
                {
                    _folder.UpdatedBy = null; // set to current user ID if available
                    _folderSettingsController.UpdateFolder(_folder);
                    if (_auditLogController != null && _currentUser != null)
                        _auditLogController.Log(new AuditEntry
                        {
                            Module = "Permission",
                            Action = "SAVE_FOLDER_SETTINGS",
                            UserID = _currentUser.UserID,
                            TableName = "Folder",
                            RecordID = _folder.FolderID,
                            NewValue = string.Format("Name:{0} Level:{1} Inherit:{2}",
                                             _folder.FolderName, _folder.ConfidentialityLevel, _folder.InheritPermission),
                            MachineName = _currentUser.MachineName,
                            IPAddress = _currentUser.IPAddress,
                            SessionID = _currentUser.SessionID,
                            Result = 1,
                            AuditDate = DateTime.Now
                        });
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
        /// Shows the permission dialog and returns SetPermissionRequest entries
        /// that handle mixed Allow/Deny permissions in a unified way
        /// </summary>
        private List<SetPermissionRequest> PromptForPermissionRequests(int folderId, FolderPermission existing)
        {
            bool[] permissionChecked = new bool[5];
            int principalType = 1;
            int principalId = 0;

            // ShowPermissionDialog returns dlg with Tag = List<OrgUnitSelection> when multi-org selected
            List<OrgUnitSelection> multiOrg = null;

            bool confirmed = ShowPermissionDialog(folderId, existing,
                out permissionChecked, out principalType, out principalId,
                out multiOrg);

            if (!confirmed) return null;

            var results = new List<SetPermissionRequest>();

            // ── Multi-org: สร้าง entry สำหรับแต่ละหน่วยงานที่เลือก ──────────
            if (multiOrg != null && multiOrg.Count > 0)
            {
                foreach (var sel in multiOrg)
                {
                    // สร้าง entry สำหรับ permissions ที่เลือก (เฉพาะ Allow)
                    var permissionEntry = CreatePermissionRequest(
                        folderId, (PrincipalType)sel.PrincipalType, sel.OrgUnit.OrgUnitID,
                        permissionChecked);
                    if (permissionEntry != null) results.Add(permissionEntry);
                }
                return results.Count > 0 ? results : null;
            }

            // ── Single principal ──────────────────────────────────────────────
            if (principalId <= 0) return null;

            // สร้าง entry สำหรับ permissions ที่เลือก (เฉพาะ Allow)
            var allowRequest = CreatePermissionRequest(
                folderId, (PrincipalType)principalType, principalId,
                permissionChecked);
            if (allowRequest != null) results.Add(allowRequest);

            return results.Count > 0 ? results : null;
        }

        /// <summary>
        /// สร้าง SetPermissionRequest สำหรับ permissions ที่เลือก (เฉพาะ Allow)
        /// </summary>
        private SetPermissionRequest CreatePermissionRequest(
            int folderId, PrincipalType principalType, int principalId,
            bool[] permissions)
        {
            // ตรวจสอบว่ามี permission ใดถูกเลือกบ้าง
            bool hasAnyPermission = permissions[0] || permissions[1] || permissions[2] || permissions[3];

            if (!hasAnyPermission) return null;

            return new SetPermissionRequest
            {
                FolderID = folderId,
                PrincipalType = principalType,
                PrincipalID = principalId,
                CanRead = permissions[0],
                CanWrite = permissions[1],
                CanDelete = permissions[2],
                CanModify = permissions[3],
                CanFullControl = false,
                IsDeny = false  // Always Allow since we removed Deny option
            };
        }

        private SetPermissionRequest PromptForPermissionRequest(int folderId, FolderPermission existing)
        {
            bool[] permissionChecked = new bool[5];
            int pt = 1, pid = 0;
            List<OrgUnitSelection> multiOrg;
            if (!ShowPermissionDialog(folderId, existing, out permissionChecked, out pt, out pid, out multiOrg))
                return null;

            return new SetPermissionRequest
            {
                FolderID = folderId,
                PrincipalType = (PrincipalType)pt,
                PrincipalID = pid,
                CanRead = permissionChecked[0],
                CanWrite = permissionChecked[1],
                CanDelete = permissionChecked[2],
                CanModify = permissionChecked[3],
                CanFullControl = false,
                IsDeny = false  // Always Allow since we removed Deny option
            };
        }

        private bool ShowPermissionDialog(int folderId, FolderPermission existing,
            out bool[] permissionChecked, out int principalType, out int principalId,
            out List<OrgUnitSelection> multiOrg)
        {
            multiOrg = null;
            using (var dlg = new Form())
            {
                dlg.Text = "🔒 เพิ่ม/แก้ไข Permission";
                dlg.ClientSize = new Size(460, 530);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.White;

                // ── Header bar (navy) ────────────────────────────────────────
                var pnlHdr = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(460, 44),
                    BackColor = Color.FromArgb(31, 56, 100)
                };
                var lblHdrTitle = new Label
                {
                    Text = "🔒 เพิ่ม/แก้ไข Permission",
                    Location = new Point(12, 0),
                    Size = new Size(400, 44),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                var btnHdrClose = new Button
                {
                    Text = "✕",
                    Location = new Point(416, 0),
                    Size = new Size(44, 44),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    DialogResult = DialogResult.Cancel
                };
                btnHdrClose.FlatAppearance.BorderSize = 0;
                pnlHdr.Controls.Add(lblHdrTitle);
                pnlHdr.Controls.Add(btnHdrClose);
                dlg.Controls.Add(pnlHdr);

                // ── Folder info box ──────────────────────────────────────────
                var pnlFolder = new Panel
                {
                    Location = new Point(12, 52),
                    Size = new Size(436, 36),
                    BackColor = Color.FromArgb(232, 240, 254),
                    BorderStyle = BorderStyle.FixedSingle
                };
                string folderPath = _folder != null ? _folder.Name : "—";
                int confLevel = _folder != null ? _folder.ConfidentialityLevel : 3;
                string badgeText = ConfidentialityBadge(confLevel);

                var lblFolderPath = new Label
                {
                    Text = "Folder:  " + folderPath,
                    Location = new Point(8, 0),
                    Size = new Size(340, 34),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(31, 56, 100),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                var lblBadge = new Label
                {
                    Text = badgeText,
                    Location = new Point(354, 7),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    BackColor = ConfidentialityColor(confLevel),
                    ForeColor = Color.White,
                    Padding = new Padding(5, 2, 5, 2)
                };
                pnlFolder.Controls.Add(lblFolderPath);
                pnlFolder.Controls.Add(lblBadge);
                dlg.Controls.Add(pnlFolder);

                // ── ประเภทผู้ใช้ ─────────────────────────────────────────────
                dlg.Controls.Add(new Label
                {
                    Text = "ประเภทผู้ใช้:",
                    Location = new Point(12, 100),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50)
                });

                var rdUser = new RadioButton { Text = "ผู้ใช้เฉพาะราย", Location = new Point(24, 122), AutoSize = true, Font = new Font("Segoe UI", 9) };
                var rdGroup = new RadioButton { Text = "กลุ่ม/แผนก", Location = new Point(160, 122), AutoSize = true, Font = new Font("Segoe UI", 9) };
                var rdRole = new RadioButton { Text = "Role", Location = new Point(280, 122), AutoSize = true, Font = new Font("Segoe UI", 9) };
                dlg.Controls.Add(rdUser);
                dlg.Controls.Add(rdGroup);
                dlg.Controls.Add(rdRole);

                // ── ปุ่มเปิด OrgUnit Picker (แสดงเฉพาะตอน rdGroup checked) ──
                var btnOrgPicker = new Button
                {
                    Text = "🏢  เลือกจากโครงสร้างองค์กร...",
                    Location = new Point(12, 148),
                    Size = new Size(220, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(43, 87, 154),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Visible = false
                };
                btnOrgPicker.FlatAppearance.BorderSize = 0;

                // Label แสดงหัวข้อรายการที่เลือก
                var lblSelectedOrg = new Label
                {
                    Text = "หน่วยงานที่เลือก:",
                    Location = new Point(12, 182),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50),
                    Visible = false
                };

                // ListBox แสดงรายการที่เลือกจาก OrgUnitPickerDialog
                var lstSelectedOrg = new ListBox
                {
                    Location = new Point(12, 200),
                    Size = new Size(430, 60),
                    Font = new Font("Segoe UI", 9),
                    BorderStyle = BorderStyle.FixedSingle,
                    Visible = false,
                    BackColor = Color.FromArgb(248, 249, 252)
                };

                // เก็บ OrgUnitSelection list ที่เลือก
                var pickedOrgUnits = new List<OrgUnitSelection>();

                btnOrgPicker.Click += (s, e) =>
                {
                    var allUnits = new List<OrgUnit>();
                    if (_orgLookupController != null)
                    {
                        try { allUnits = _orgLookupController.GetHierarchy().ToList(); }
                        catch { }
                    }

                    using (var picker = _employeeApiService != null
                        ? (Form)new EmployeeOrgSelectionDialog(new EmployeeDirectoryController(_employeeApiService))
                        : (Form)new OrgStructureSelectionDialog(allUnits))
                    {
                        if (picker.ShowDialog(dlg) == DialogResult.OK)
                        {
                            pickedOrgUnits.Clear();
                            lstSelectedOrg.Items.Clear();

                            // รองรับทั้ง EmployeeOrgSelectionDialog และ OrgStructureSelectionDialog
                            var empDlg = picker as EmployeeOrgSelectionDialog;
                            var orgDlg = picker as OrgStructureSelectionDialog;

                            if (empDlg != null && empDlg.SelectedUnits.Count > 0)
                            {
                                foreach (var sel in empDlg.SelectedUnits)
                                {
                                    // สร้าง OrgUnit stub เพื่อให้ OrgUnitSelection.OrgUnit ไม่เป็น null
                                    var orgUnitStub = new CMS.Domain.Models.OrgUnit
                                    {
                                        OrgUnitID = sel.Id,
                                        OrgUnitCode = sel.Code,
                                        OrgUnitNameTH = sel.NameTh
                                    };
                                    pickedOrgUnits.Add(new OrgUnitSelection
                                    {
                                        OrgUnit = orgUnitStub,
                                        DisplayName = string.Format("[{0}] {1}", sel.Code, sel.NameTh),
                                        PrincipalType = sel.Type == "Division" ? 4 : 3
                                    });
                                    lstSelectedOrg.Items.Add(string.Format("[{0}] {1}", sel.Code, sel.NameTh));
                                }
                            }
                            else if (orgDlg != null && orgDlg.SelectedUnits.Count > 0)
                            {
                                pickedOrgUnits.AddRange(orgDlg.SelectedUnits);
                                foreach (var sel in orgDlg.SelectedUnits)
                                    lstSelectedOrg.Items.Add(sel.DisplayName);
                            }

                            if (pickedOrgUnits.Count > 0)
                            {
                                lblSelectedOrg.Visible = true;
                                lstSelectedOrg.Visible = true;
                                int rows = Math.Min(pickedOrgUnits.Count, 4);
                                lstSelectedOrg.Height = rows * 18 + 8;
                            }
                        }
                    }
                };

                dlg.Controls.Add(btnOrgPicker);
                dlg.Controls.Add(lblSelectedOrg);
                dlg.Controls.Add(lstSelectedOrg);

                // ── เลือกหน่วยงาน / ผู้ใช้ ──────────────────────────────────
                var lblSelect = new Label
                {
                    Text = "เลือกหน่วยงาน:",
                    Location = new Point(12, 158),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(50, 50, 50)
                };
                var cboSelect = new ComboBox
                {
                    Location = new Point(132, 154),
                    Size = new Size(240, 24),
                    DropDownStyle = ComboBoxStyle.DropDown,
                    Font = new Font("Segoe UI", 9)
                };

                // ── ปุ่มค้นหาพนักงาน (Employee API) ─────────────────────────
                var btnSearchEmp = new Button
                {
                    Text = "🔍 ค้นหา",
                    Location = new Point(378, 152),
                    Size = new Size(70, 26),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(43, 87, 154),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Visible = false   // แสดงเฉพาะตอน rdUser checked
                };
                btnSearchEmp.FlatAppearance.BorderSize = 0;
                btnSearchEmp.Click += (s, e) =>
                {
                    // Ensure user list is loaded from UserAccount table
                    if ((_userList == null || _userList.Count == 0) && _userAccountController != null)
                    {
                        try { _userList = _userAccountController.GetAllUsers().ToList(); }
                        catch { }
                    }

                    if (_userList == null || _userList.Count == 0)
                    {
                        MessageBox.Show("ไม่พบข้อมูลผู้ใช้งาน กรุณาตรวจสอบการเชื่อมต่อฐานข้อมูล",
                            "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    using (var searchDlg = new Form())
                    {
                        searchDlg.Text = "ค้นหาผู้ใช้งาน";
                        searchDlg.ClientSize = new Size(540, 420);
                        searchDlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                        searchDlg.StartPosition = FormStartPosition.CenterParent;
                        searchDlg.MaximizeBox = false;
                        searchDlg.MinimizeBox = false;

                        // Header
                        var pnlSearchHdr = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(0, 51, 102) };
                        pnlSearchHdr.Controls.Add(new Label
                        {
                            Text = "🔍  ค้นหาผู้ใช้งาน",
                            Dock = DockStyle.Fill,
                            ForeColor = Color.White,
                            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                            TextAlign = ContentAlignment.MiddleLeft,
                            Padding = new Padding(10, 0, 0, 0)
                        });

                        // Footer
                        var pnlFt = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.FromArgb(243, 243, 243) };
                        var lblFtCount = new Label { Dock = DockStyle.Left, Width = 150, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0), Font = new Font("Segoe UI", 9f) };
                        var btnSel = new Button
                        {
                            Text = "✔ เลือก",
                            Size = new Size(90, 28),
                            BackColor = Color.FromArgb(0, 51, 102),
                            ForeColor = Color.White,
                            FlatStyle = FlatStyle.Flat,
                            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                            Anchor = AnchorStyles.Right | AnchorStyles.Top
                        };
                        btnSel.FlatAppearance.BorderSize = 0;
                        var btnCnl2 = new Button
                        {
                            Text = "ยกเลิก",
                            Size = new Size(80, 28),
                            BackColor = Color.White,
                            ForeColor = Color.FromArgb(80, 80, 80),
                            FlatStyle = FlatStyle.Flat,
                            DialogResult = DialogResult.Cancel,
                            Anchor = AnchorStyles.Right | AnchorStyles.Top
                        };
                        btnCnl2.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 190);
                        pnlFt.Controls.Add(lblFtCount);
                        pnlFt.Controls.Add(btnSel);
                        pnlFt.Controls.Add(btnCnl2);
                        pnlFt.Resize += (ps, pe) =>
                        {
                            btnCnl2.Left = pnlFt.ClientSize.Width - btnCnl2.Width - 8;
                            btnSel.Left = btnCnl2.Left - btnSel.Width - 8;
                            btnSel.Top = btnCnl2.Top = (pnlFt.Height - btnSel.Height) / 2;
                        };

                        // Search box
                        var pnlSrch = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.White, Padding = new Padding(6, 4, 6, 4) };
                        bool isHolder = true;
                        var txtSrch = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f), ForeColor = Color.Gray, Text = "ค้นหาชื่อผู้ใช้ ชื่อ-นามสกุล หรือแผนก..." };
                        txtSrch.GotFocus += (ts, te) => { if (!isHolder) return; isHolder = false; txtSrch.Text = ""; txtSrch.ForeColor = Color.Black; };
                        txtSrch.LostFocus += (ts, te) => { if (!string.IsNullOrWhiteSpace(txtSrch.Text)) return; isHolder = true; txtSrch.Text = "ค้นหาชื่อผู้ใช้ ชื่อ-นามสกุล หรือแผนก..."; txtSrch.ForeColor = Color.Gray; };
                        pnlSrch.Controls.Add(txtSrch);

                        // ListView
                        var lv = new ListView
                        {
                            Dock = DockStyle.Fill,
                            View = View.Details,
                            FullRowSelect = true,
                            GridLines = true,
                            MultiSelect = false,
                            Font = new Font("Segoe UI", 9.5f)
                        };
                        lv.Columns.Add("Username", 130);
                        lv.Columns.Add("ชื่อ-นามสกุล (TH)", 170);
                        lv.Columns.Add("แผนก / ฝ่าย", 195);

                        var activeUsers = _userList.Where(u => u.IsActive).OrderBy(u => u.ADUserName).ToList();
                        var allLvItems = activeUsers.Select(u =>
                        {
                            var row = new ListViewItem(u.ADUserName ?? "");
                            string name = !string.IsNullOrEmpty(u.FullNameTh) ? u.FullNameTh : (u.DisplayName ?? "");
                            string org = !string.IsNullOrEmpty(u.DepartmentName) ? u.DepartmentName
                                        : (!string.IsNullOrEmpty(u.DivisionName) ? u.DivisionName : "");
                            row.SubItems.Add(name);
                            row.SubItems.Add(org);
                            row.Tag = u;
                            return row;
                        }).ToList();

                        lv.Items.AddRange(allLvItems.ToArray());
                        lblFtCount.Text = string.Format("{0} รายการ", allLvItems.Count);

                        txtSrch.TextChanged += (ts, te) =>
                        {
                            if (isHolder) return;
                            string kw = txtSrch.Text.Trim().ToLower();
                            lv.BeginUpdate();
                            lv.Items.Clear();
                            var filtered = string.IsNullOrEmpty(kw) ? allLvItems : allLvItems.Where(r =>
                                r.Text.ToLower().Contains(kw) ||
                                (r.SubItems.Count > 1 && r.SubItems[1].Text.ToLower().Contains(kw)) ||
                                (r.SubItems.Count > 2 && r.SubItems[2].Text.ToLower().Contains(kw))).ToList();
                            lv.Items.AddRange(filtered.ToArray());
                            lblFtCount.Text = string.Format("{0} รายการ", filtered.Count);
                            lv.EndUpdate();
                        };

                        Action confirmSel = () =>
                        {
                            if (lv.SelectedItems.Count == 0) return;
                            var selUser = lv.SelectedItems[0].Tag as UserAccount;
                            if (selUser == null) return;
                            string label = string.Format("{0} — {1} (ID:{2})",
                                selUser.ADUserName,
                                !string.IsNullOrEmpty(selUser.FullNameTh) ? selUser.FullNameTh : selUser.DisplayName ?? "",
                                selUser.UserID);
                            cboSelect.Text = label;
                            cboSelect.Tag = selUser;
                            searchDlg.DialogResult = DialogResult.OK;
                            searchDlg.Close();
                        };
                        btnSel.Click += (bs, be) => confirmSel();
                        lv.DoubleClick += (ls, le) => confirmSel();

                        searchDlg.Controls.Add(lv);
                        searchDlg.Controls.Add(pnlSrch);
                        searchDlg.Controls.Add(pnlFt);
                        searchDlg.Controls.Add(pnlSearchHdr);
                        searchDlg.Shown += (ss, se) => txtSrch.Focus();
                        searchDlg.ShowDialog(dlg);
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
                        if (_userAccountController == null) return;
                        try
                        {
                            _userList = _userAccountController.GetAllUsers().ToList();
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

                rdUser.CheckedChanged += (s, e) =>
                {
                    if (rdUser.Checked)
                    {
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
                rdGroup.CheckedChanged += (s, e) =>
                {
                    if (rdGroup.Checked)
                    {
                        lblSelect.Text = "เลือกหน่วยงาน:";
                        btnSearchEmp.Visible = false;
                        btnOrgPicker.Visible = true;
                        lstSelectedOrg.Visible = pickedOrgUnits.Count > 0;
                        lblSelectedOrg.Visible = pickedOrgUnits.Count > 0;
                        lblSelect.Visible = false;
                        cboSelect.Visible = false;
                    }
                };
                rdRole.CheckedChanged += (s, e) =>
                {
                    if (rdRole.Checked)
                    {
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
                        case PrincipalType.User: rdUser.Checked = true; break;
                        case PrincipalType.Department:  // Department
                        case PrincipalType.Division:  // Division
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
                    if (existing.PrincipalType == PrincipalType.User && _userList != null && _userList.Count > 0)
                    {
                        var eu = _userList.FirstOrDefault(u => u.UserID == existing.PrincipalId);
                        cboSelect.Text = eu != null
                            ? string.Format("{0} — {1} (ID:{2})", eu.ADUserName, eu.DisplayName, eu.UserID)
                            : existing.PrincipalId.ToString();
                    }
                    else if (existing.PrincipalType == PrincipalType.Department)
                    {
                        // Department
                        string deptName;
                        if (_departmentNameMap.TryGetValue(existing.PrincipalId, out deptName))
                            cboSelect.Text = string.Format("[แผนก] {0}", deptName);
                        else
                            cboSelect.Text = string.Format("DeptID: {0}", existing.PrincipalId);
                    }
                    else if (existing.PrincipalType == PrincipalType.Division)
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
                    Text = "สิทธิการเข้าถึง:",
                    Location = new Point(12, 192),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50)
                };
                dlg.Controls.Add(lblPermHeader);

                // ── Permission table ──────────────────────────────────────────
                var pnlPerm = new Panel
                {
                    Location = new Point(12, 212),
                    Size = new Size(436, 177),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Table header row
                var pnlPermHdr = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(436, 27),
                    BackColor = Color.FromArgb(240, 244, 250)
                };
                pnlPermHdr.Controls.Add(new Label { Text = "สิทธิ์", Location = new Point(10, 5), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60) });
                pnlPermHdr.Controls.Add(new Label { Text = "เลือก", Location = new Point(320, 5), Size = new Size(60, 18), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60), TextAlign = ContentAlignment.MiddleCenter });
                pnlPerm.Controls.Add(pnlPermHdr);

                // Row data: label, existing permission (checked if has permission and not denied)
                var permDefs = new[]
                {
                    new { Label = "📖 อ่าน (Read)",      Checked = existing != null && existing.CanRead   && !existing.IsDeny },
                    new { Label = "✏️ เขียน (Write)",    Checked = existing != null && existing.CanWrite  && !existing.IsDeny },
                    new { Label = "🗑 ลบ (Delete)",      Checked = existing != null && existing.CanDelete && !existing.IsDeny },
                    new { Label = "🔧 แก้ไข (Modify)",  Checked = existing != null && existing.CanModify && !existing.IsDeny },
                };

                var chkPermissions = new CheckBox[4];

                for (int i = 0; i < permDefs.Length; i++)
                {
                    int rowY = 28 + i * 29;
                    var rowPnl = new Panel
                    {
                        Location = new Point(0, rowY),
                        Size = new Size(434, 29),
                        BackColor = i % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 255)
                    };

                    rowPnl.Controls.Add(new Label
                    {
                        Text = permDefs[i].Label,
                        Location = new Point(10, 5),
                        Size = new Size(280, 20),
                        Font = new Font("Segoe UI", 9),
                        ForeColor = Color.FromArgb(40, 40, 40)
                    });

                    // Single checkbox for permission
                    chkPermissions[i] = new CheckBox
                    {
                        Location = new Point(335, 5),
                        Size = new Size(20, 20),
                        Checked = permDefs[i].Checked
                    };
                    rowPnl.Controls.Add(chkPermissions[i]);
                    pnlPerm.Controls.Add(rowPnl);
                }
                dlg.Controls.Add(pnlPerm);

                // ── วันหมดอายุ ─────────────────────────────────────────────
                int expiryY = 400;
                var lblExpiry = new Label
                {
                    Text = "วันหมดอายุ:",
                    Location = new Point(12, expiryY + 5),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(50, 50, 50)
                };
                dlg.Controls.Add(lblExpiry);
                var dtpExpiry = new DateTimePicker
                {
                    Location = new Point(100, expiryY),
                    Size = new Size(154, 24),
                    Format = DateTimePickerFormat.Short,
                    Font = new Font("Segoe UI", 9),
                    Enabled = existing != null && existing.ExpiryDate.HasValue
                };
                if (existing != null && existing.ExpiryDate.HasValue)
                    dtpExpiry.Value = existing.ExpiryDate.Value;
                var chkNoExpiry = new CheckBox
                {
                    Text = "ไม่มีวันหมดอายุ",
                    Location = new Point(264, expiryY + 3),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9),
                    Checked = existing == null || !existing.ExpiryDate.HasValue
                };
                chkNoExpiry.CheckedChanged += (s, e) => dtpExpiry.Enabled = !chkNoExpiry.Checked;
                dlg.Controls.Add(dtpExpiry);
                dlg.Controls.Add(chkNoExpiry);

                // ── Warning box ───────────────────────────────────────────────
                var pnlWarn = new Panel
                {
                    Location = new Point(12, 434),
                    Size = new Size(436, 34),
                    BackColor = Color.FromArgb(255, 248, 215),
                    BorderStyle = BorderStyle.FixedSingle
                };
                pnlWarn.Controls.Add(new Label
                {
                    Text = "⚠ การเปลี่ยนแปลงนี้ต้องได้รับอนุมัติจาก Admin (เลือก = มีสิทธิ์, ไม่เลือก = ไม่มีสิทธิ์)",
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(160, 90, 0),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(6, 0, 0, 0)
                });
                dlg.Controls.Add(pnlWarn);

                // ── Footer buttons ────────────────────────────────────────────
                var pnlFooter = new Panel
                {
                    Location = new Point(0, 478),
                    Size = new Size(460, 52),
                    BackColor = Color.FromArgb(248, 250, 255)
                };
                var btnOk = new Button
                {
                    Text = "✉ ส่งขออนุมัติ",
                    Location = new Point(316, 9),
                    Size = new Size(130, 34),
                    BackColor = Color.FromArgb(43, 87, 154),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    DialogResult = DialogResult.OK
                };
                btnOk.FlatAppearance.BorderSize = 0;
                var btnCnl = new Button
                {
                    Text = "ยกเลิก",
                    Location = new Point(228, 9),
                    Size = new Size(80, 34),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(80, 80, 80),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9),
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
                    int permY = 192 + orgListExtra;
                    int pnlPermY = permY + 20;
                    int expY = pnlPermY + pnlPerm.Height + 12;
                    int warnY = expY + 44;
                    int footerY = warnY + 44;

                    lblPermHeader.Location = new Point(12, permY);
                    pnlPerm.Location = new Point(12, pnlPermY);
                    lblExpiry.Location = new Point(12, expY + 5);
                    dtpExpiry.Location = new Point(100, expY);
                    chkNoExpiry.Location = new Point(264, expY + 3);
                    pnlWarn.Location = new Point(12, warnY);
                    pnlFooter.Location = new Point(0, footerY);
                    dlg.ClientSize = new Size(460, footerY + pnlFooter.Height);
                };

                // คำนวณ layout เริ่มต้น (lstSelectedOrg ซ่อนอยู่)
                updateLayout();

                // เรียก updateLayout ทุกครั้งที่ org list เปลี่ยน
                lstSelectedOrg.VisibleChanged += (s, e) => updateLayout();
                lstSelectedOrg.SizeChanged += (s, e) => updateLayout();

                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCnl;

                // ── Show & validate ───────────────────────────────────────────
                while (true)
                {
                    if (dlg.ShowDialog() != DialogResult.OK)
                    {
                        permissionChecked = new bool[5];
                        principalType = 1;
                        principalId = 0;
                        multiOrg = null;
                        return false;
                    }

                    // ── กรณี rdGroup + pickedOrgUnits → ข้าม validation ปกติ ──
                    if (rdGroup.Checked && pickedOrgUnits.Count > 0)
                    {
                        // ตรวจสอบว่าเลือกสิทธิ์อย่างน้อย 1 รายการ
                        bool anyPerm = chkPermissions[0].Checked || chkPermissions[1].Checked || chkPermissions[2].Checked || chkPermissions[3].Checked;
                        if (!anyPerm)
                        {
                            MessageBox.Show("กรุณาเลือกสิทธิ์อย่างน้อย 1 รายการ",
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
                    permissionChecked = new bool[] { chkPermissions[0].Checked, chkPermissions[1].Checked, chkPermissions[2].Checked, chkPermissions[3].Checked };
                    var first = pickedOrgUnits[0];
                    principalType = first.PrincipalType;
                    principalId = first.OrgUnit.OrgUnitID;
                    multiOrg = pickedOrgUnits;
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

                // Set out parameters with per-permission checked state
                permissionChecked = new bool[] { chkPermissions[0].Checked, chkPermissions[1].Checked, chkPermissions[2].Checked, chkPermissions[3].Checked };

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
                multiOrg = dlg.Tag as List<OrgUnitSelection>;
                return true;
            }
        }

        private static void AddRow(TableLayoutPanel t, string label, Control ctrl, int row)
        {
            if (!string.IsNullOrEmpty(label))
                t.Controls.Add(new Label
                {
                    Text = label,
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft
                }, 0, row);
            t.Controls.Add(ctrl, 1, row);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string PermCheck(bool value, bool isDeny)
        {
            if (!value) return "—";
            return isDeny ? "—" : "✓";  // Show hyphen (—) for Deny, checkmark (✓) for Allow
        }

        /// <summary>
        /// หา UserID จาก DB โดยใช้ ADUserName = "BUI_DOMAIN1\{FullNameEn}"
        /// ถ้าไม่มีให้สร้าง UserAccount ใหม่อัตโนมัติ
        /// </summary>
        private int ResolveOrCreateUserAccount(EmployeeSummary emp)
        {
            if (_userAccountController == null) return 0;

            // สร้าง ADUserName จาก FullNameEn
            string fullNameEn = !string.IsNullOrEmpty(emp.FullNameEn) ? emp.FullNameEn : ExtractEnglishName(emp.DisplayName);
            string adUserName = GenerateAdUserName(fullNameEn, emp.EmployeeCode);

            // ค้นหาใน DB ก่อน
            try
            {
                var existing = _userAccountController.GetUserByAdUserName(adUserName);
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
                    ADUserName = adUserName,
                    DisplayName = emp.DisplayName ?? fullNameEn,
                    Email = null,
                    AppRole = 1,       // Officer (ระดับต่ำสุด)
                    OrgUnitID = orgUnitId,
                    ConsentGiven = false,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                int newId = _userAccountController.CreateUser(newAccount);

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

        private static string GenerateAdUserName(string fullNameEn, string employeeCode)
        {
            if (string.IsNullOrEmpty(fullNameEn)) 
                return employeeCode;

            // Remove any placeholder values like "- -", "-", etc.
            string clean = fullNameEn.Replace("-", "").Replace(".", "").Trim();
            if (string.IsNullOrEmpty(clean))
                return employeeCode;

            return fullNameEn.Replace(" ", ".").ToLower();
        }

        /// <summary>แยกชื่อภาษาอังกฤษออกจาก DisplayName เช่น "ขนิษฐา ไชยมงค์ (Khanittha Chaimong)" → "Khanittha Chaimong"</summary>
        private static string ExtractEnglishName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return string.Empty;
            int open = displayName.LastIndexOf('(');
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
            public int PrincipalType { get; private set; }
            public int Id { get; private set; }
            public string Name { get; private set; }
            public OrgUnitItem(int principalType, int id, string name)
            {
                PrincipalType = principalType;
                Id = id;
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
