using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Controllers.KMS.KB;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.ImportFile;
using CMS.Presentation.Modules.KMS;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// The WinForms Multiple Document Interface host form.
    /// Hosts all four module forms as MDI children, enforces role-based navigation,
    /// and provides a notification inbox panel.
    /// Satisfies Requirements 2.1–2.4, 13.3, 13.4.
    /// </summary>
    public partial class MDI_Container : Form
    {
        private readonly ILoggingService      _logger;
        private readonly IAuditService        _auditService;
        private readonly UserContext          _currentUser;
        private readonly INotificationService _notificationService;
        private readonly IApprovalService     _approvalService;
        private readonly IFolderAuthorityService _folderAuthorityService;
        private readonly IOrgStructureService _orgStructureService;
        private readonly IFolderService       _folderService;
        private readonly IConfigManager       _configManager;
        private readonly IPermissionService       _permissionService;
        private readonly IFilePermissionService   _filePermissionService;
        private readonly IFolderTemplateController _folderTemplateController;
        private readonly IUploadCheckpointController _uploadCheckpointController;
        private readonly IQuickAccessService       _quickAccessService;
        private readonly IShareCommandController _shareCommandController;
        private readonly IShareLookupController _shareLookupController;
        private readonly IAccessShareController _accessShareController;
        private readonly IShareManagementController _shareManagementController;
        private readonly IFolderSettingsController _folderSettingsController;
        private readonly IFolderPermissionLookupController _folderPermissionLookupController;
        private readonly IUserAccountLookupController _userAccountLookupController;
        private readonly IUserAccessOrgLookupController _userAccessOrgLookupController;
        private readonly IUserAdminController _userAdminController;
        private readonly IUserAdminController _userAdminWithHrController;
        private readonly IMenuManagementController _menuManagementController;
        private readonly IApprovalInboxController _approvalInboxController;
        private readonly IRecycleBinController _recycleBinController;
        private readonly IFileUploadController _fileUploadController;
        private readonly IFileBrowseController _fileBrowseController;
        private readonly IFileCommandController _fileCommandController;
        private readonly INotificationController _notificationController;
        private readonly IFolderCommandController _folderCommandController;
        private readonly IQuickAccessController _quickAccessController;
        private readonly IApprovalFolderController _approvalFolderController;
        private readonly IPermissionCheckController _permissionCheckController;
        private readonly IAuditLogController _auditLogController;
        private readonly IFolderReadLookupController _folderReadLookupController;
        private readonly IFileDocumentLookupController _fileDocumentLookupController;
        private readonly IFilePermissionAclController _filePermissionAclController;
        private readonly IFilePermissionCommandController _filePermissionCommandController;
        private readonly IAuthorityOrgLookupController _authorityOrgLookupController;
        private readonly IAuthorityConfigController _authorityConfigController;
        private readonly IEmployeeDirectoryController _employeeDirectoryController;
        private readonly IUserOrgVisibilityController _userOrgVisibilityController;
        private readonly CMS.Services.Interfaces.IKBCategoryService _kbCategoryService;
        private readonly CMS.Services.Interfaces.IKBArticleService  _kbArticleService;

        // Notification inbox controls (built programmatically)
        private Panel    _notificationPanel;
        private ListBox  _notificationListBox;
        private Button   _markReadButton;
        private Button   _refreshButton;

        // Background timer for escalations and inbox refresh
        private Timer _escalationTimer;

        // Backing list for the inbox
        private List<Notification> _unreadNotifications = new List<Notification>();

        // Track expanded folder IDs in lvRecentFiles
        private readonly HashSet<int> _expandedFolderIds = new HashSet<int>();

        // Cache ข้อมูลที่ใช้บ่อยในหน้า Dashboard เพื่อลด DB round-trips
        private List<OrgUnit> _cachedOrgUnits;
        private List<FileDocument> _cachedAllFiles;

        // Tracks whether the logout audit has already been recorded
        private bool _logoutRecorded;

        /// <summary>Set to true เมื่อ user กด Logout — Program.cs ใช้ตรวจว่าต้อง re-login หรือไม่</summary>
        public bool LogoutRequested { get; private set; }

        // ── Grouped admin toolbar buttons (built in BuildGroupedAdminToolbar) ──
        private ToolStripDropDownButton _grpDocument;
        private ToolStripButton         _grpApproval;
        private ToolStripDropDownButton _grpOrg;
        private ToolStripDropDownButton _grpPermission;
        private ToolStripDropDownButton _grpReport;
        private ToolStripDropDownButton _grpSystem;
        // Admin-only sub-items inside _grpPermission
        private ToolStripMenuItem _mnuUserAccess;
        private ToolStripMenuItem _mnuUserMgmt;
        // Ribbon collapse toggle
        private ToolStripMenuItem _mnuToggleRibbon;
        private bool              _ribbonCollapsed  = false;
        private bool              _adminTabActive   = false;

        // ── "Shared With Me" badge (Home ribbon) ────────────────────────────
        private ToolStripLabel _lblSharedBadge;

        // File detail panel controls
        private Panel _fileDetailPanel;
        private Label _lblDetailFileName;
        private Label _lblDetailType;
        private Label _lblDetailConfidentiality;
        private Label _lblDetailSize;
        private Label _lblDetailVersion;
        private Label _lblDetailDate;
        private Label _lblDetailUploadedBy;
        private Button _btnDetailOpen;
        private Button _btnDetailDownload;
        private Button _btnDetailSecurity;

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        public MDI_Container(
            ILoggingService           logger,
            IAuditService             auditService,
            UserContext               currentUser,
            INotificationService      notificationService,
            IApprovalService          approvalService,
            IFolderAuthorityService   folderAuthorityService     = null,
            IOrgStructureService      orgStructureService       = null,
            IFolderService            folderService             = null,
            IConfigManager            configManager             = null,
            IPermissionService        permissionService         = null,
            IFilePermissionService    filePermissionService     = null,
            IFolderTemplateController folderTemplateController  = null,
            IUploadCheckpointController uploadCheckpointController = null,
            IQuickAccessService quickAccessService = null,
            IShareCommandController shareCommandController = null,
            IShareLookupController shareLookupController = null,
            IAccessShareController accessShareController = null,
            IShareManagementController shareManagementController = null,
            IFolderSettingsController folderSettingsController = null,
            IFolderPermissionLookupController folderPermissionLookupController = null,
            IUserAccountLookupController userAccountLookupController = null,
            IUserAccessOrgLookupController userAccessOrgLookupController = null,
            IUserAdminController userAdminController = null,
            IUserAdminController userAdminWithHrController = null,
            IMenuManagementController menuManagementController = null,
            IApprovalInboxController approvalInboxController = null,
            IRecycleBinController recycleBinController = null,
            IFileUploadController fileUploadController = null,
            IFileBrowseController fileBrowseController = null,
            IFileCommandController fileCommandController = null,
            INotificationController notificationController = null,
            IFolderCommandController folderCommandController = null,
            IQuickAccessController quickAccessController = null,
            IApprovalFolderController approvalFolderController = null,
            IPermissionCheckController permissionCheckController = null,
            IAuditLogController auditLogController = null,
            IFolderReadLookupController folderReadLookupController = null,
            IFileDocumentLookupController fileDocumentLookupController = null,
            IFilePermissionAclController filePermissionAclController = null,
            IFilePermissionCommandController filePermissionCommandController = null,
            IAuthorityOrgLookupController authorityOrgLookupController = null,
            IAuthorityConfigController authorityConfigController = null,
            IEmployeeDirectoryController employeeDirectoryController = null,
            IUserOrgVisibilityController userOrgVisibilityController = null,
            Data.Repositories.FolderRepository folderRepository = null,
            CMS.Services.Interfaces.IKBCategoryService kbCategoryService = null,
            CMS.Services.Interfaces.IKBArticleService  kbArticleService  = null)
        {
            if (logger              == null) throw new ArgumentNullException("logger");
            if (auditService        == null) throw new ArgumentNullException("auditService");
            if (currentUser         == null) throw new ArgumentNullException("currentUser");
            if (notificationService == null) throw new ArgumentNullException("notificationService");
            if (approvalService     == null) throw new ArgumentNullException("approvalService");

            _logger                   = logger;
            _auditService             = auditService;
            _currentUser              = currentUser;
            _notificationService      = notificationService;
            _approvalService          = approvalService;
            _folderAuthorityService   = folderAuthorityService;
            _orgStructureService      = orgStructureService;
            _folderService            = folderService;
            _configManager            = configManager;
            _permissionService        = permissionService;
            _filePermissionService    = filePermissionService;
            _folderTemplateController = folderTemplateController;
            _uploadCheckpointController = uploadCheckpointController;
            _quickAccessService       = quickAccessService;
            _shareCommandController = shareCommandController;
            _shareLookupController = shareLookupController;
            _accessShareController = accessShareController;
            _shareManagementController = shareManagementController;
            _folderSettingsController = folderSettingsController;
            _folderPermissionLookupController = folderPermissionLookupController;
            _userAccountLookupController = userAccountLookupController;
            _userAccessOrgLookupController = userAccessOrgLookupController ?? new UserAccessOrgLookupController(_orgStructureService);
            _userAdminController = userAdminController;
            _userAdminWithHrController = userAdminWithHrController;
            _menuManagementController = menuManagementController;
            _approvalInboxController = approvalInboxController;
            _recycleBinController = recycleBinController;
            _fileUploadController = fileUploadController;
            _fileBrowseController = fileBrowseController ?? new FileBrowseController(null);
            _fileCommandController = fileCommandController ?? new FileCommandController(null);
            _notificationController = notificationController ?? new NotificationController(_notificationService);
            _folderCommandController = folderCommandController ?? new FolderCommandController(_folderService);
            _quickAccessController = quickAccessController ?? new QuickAccessController(_quickAccessService);
            _approvalFolderController = approvalFolderController ?? new ApprovalFolderController(_approvalService);
            _permissionCheckController = permissionCheckController ?? new PermissionCheckController(_permissionService);
            _auditLogController = auditLogController ?? new AuditLogController(_auditService);
            _folderReadLookupController = folderReadLookupController ?? new FolderReadLookupController(folderRepository, _folderService);
            _fileDocumentLookupController = fileDocumentLookupController;
            _filePermissionAclController = filePermissionAclController;
            _filePermissionCommandController = filePermissionCommandController;
            _authorityOrgLookupController = authorityOrgLookupController;
            _authorityConfigController = authorityConfigController;
            _employeeDirectoryController = employeeDirectoryController;
            _userOrgVisibilityController = userOrgVisibilityController;
            _kbCategoryService = kbCategoryService;
            _kbArticleService  = kbArticleService;

            InitializeComponent();
            BuildFileDetailPanel();
            WireMenuHandlers();
            BuildEscalationTimer();
            
            // Hide recycle bin menu items since recycle bin functionality is disabled
            HideRecycleBinMenuItems();
        }

        // -----------------------------------------------------------------------
        // Form lifecycle
        // -----------------------------------------------------------------------

        private void MDI_Container_Load(object sender, EventArgs e)
        {
            try
            {
                // Set form to full screen
                this.WindowState = FormWindowState.Maximized;

                Text = string.Format("CMS — {0}", _currentUser.AdIdentity);
                BuildGroupedAdminToolbar();
                ApplyRoleBasedMenuVisibility();
                WireRibbonHandlers();
                WireDashboardHandlers();
                SetupRecentFilesColumns();
                LoadDashboardFolderTree();
                LoadRecentFiles();
                UpdateStatusBar();
                LoadDashboardStats();
                _escalationTimer.Start();

                // ซ่อน detail panel จนกว่าจะมีการคลิกไฟล์
                _fileDetailPanel.Visible = false;

                // Add refresh button for recent files
                AddRefreshButton();

                // Wire treeViewFolders selection → show files
                treeViewFolders.AfterSelect += TreeViewFolders_AfterSelect;

                // Wire lvRecentFiles selection → show detail panel
                lvRecentFiles.SelectedIndexChanged += LvRecentFiles_SelectedIndexChanged;

                // Double-click folder row → toggle expand/collapse files
                lvRecentFiles.DoubleClick += LvRecentFiles_DoubleClick;

                // Right-click context menu for lvRecentFiles
                BuildRecentFilesContextMenu();

                // Auto-resize columns when form is resized
                this.Resize += (s, ev) => AutoResizeColumnsProportional();

                // Clock timer for status bar
                var clockTimer = new Timer { Interval = 30000 };
                clockTimer.Tick += (s, ev) => UpdateStatusBarDateTime();
                clockTimer.Start();

                // Handle MDI child activation to show/hide dashboard
                this.MdiChildActivate += (s, ev) => OnMdiChildActivate();

                // Auto-open FolderForm as MDI child
                // Requirement: FolderForm (MOD-002) should open automatically on startup
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        OpenFolderStructure();
                        _logger.Info("FolderForm opened successfully on startup.");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error opening FolderForm: {ex.Message}", ex);
                        MessageBox.Show($"Error opening Folder Manager: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            }
            catch (Exception ex)
            {
                _logger.Error($"Error during MDI_Container initialization: {ex.Message}", ex);
                MessageBox.Show($"Initialization error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnMdiChildActivate()
        {
            bool showDashboard = (this.ActiveMdiChild == null);
            pnlDashboard.Visible = showDashboard;

            ApplyAdminSectionVisibilityForActiveChild();
            SetTopMenuVisibilityForAuthorityConfig(HasOpenAuthorityConfigForm());

            // Reload lvRecentFiles ทุกครั้งที่ dashboard กลับมาแสดง
            if (showDashboard)
                RefreshDashboardFiles();

            // Update move controls enabled state based on active child (Req 9.4)
            UpdateMoveControlsState();
        }

        /// <summary>
        /// Enables menuItemMove and btnRibbonMove only when the active MDI child
        /// is a FolderForm that has a Folder or FileDocument selected (Req 9.3, 9.4).
        /// </summary>
        private void UpdateMoveControlsState()
        {
            var activeFolder = ActiveMdiChild as FolderForm;
            bool enabled = activeFolder != null && activeFolder.HasActiveSelection;
            menuItemMove.Enabled    = enabled;
            btnRibbonMove.Enabled   = enabled;
        }

        /// <summary>
        /// Handles menuItemMove and btnRibbonMove clicks.
        /// Delegates to the active FolderForm's TriggerMove() (Req 9.3).
        /// </summary>
        private void MenuItemMove_Click(object sender, EventArgs e)
        {
            var activeFolder = ActiveMdiChild as FolderForm;
            if (activeFolder != null)
                activeFolder.TriggerMove();
        }

        /// <summary>
        /// Find the active FolderForm MDI child and run an action on it.
        /// Shows a message if no FolderForm is open.
        /// </summary>
        private void TriggerOnActiveFolderForm(Action<FolderForm> action)
        {
            var ff = ActiveMdiChild as FolderForm
                  ?? MdiChildren.OfType<FolderForm>().FirstOrDefault();
            if (ff == null)
            {
                MessageBox.Show("กรุณาเปิด Folder Explorer ก่อน",
                    "ไม่พบ Folder Explorer",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            action(ff);
        }

        /// <summary>
        /// Reload lvRecentFiles ตาม context ที่เลือกอยู่:
        /// - ถ้า treeViewFolders มี node เลือกอยู่ → โหลดไฟล์ตาม OrgUnit นั้น
        /// - ถ้าไม่มี → โหลดไฟล์ทั้งหมด (recent files)
        /// </summary>
        private void RefreshDashboardFiles()
        {
            // Invalidate file cache เพื่อให้ดึงข้อมูลล่าสุดจาก DB
            _cachedAllFiles = null;
            var selectedNode = treeViewFolders.SelectedNode;
            if (selectedNode != null)
            {
                // Mode: ตามองค์กร
                var orgUnit = selectedNode.Tag as OrgUnit;
                if (orgUnit != null)
                {
                    lblRecentFiles.Text = string.Format("📄 ไฟล์ใน {0}", orgUnit.Name);
                    LoadFilesForOrgUnit(orgUnit.OrgUnitID);
                    return;
                }

                // Mode: ตามความลับ
                if (selectedNode.Tag is int)
                {
                    int level = (int)selectedNode.Tag;
                    string[] labels = { "", "1HC — Highly Confidential", "2SC — Secret", "3NC — Non-Confidential", "4PC — Public/Common" };
                    string labelText = (level >= 1 && level <= 4) ? labels[level] : "ระดับ " + level;
                    lblRecentFiles.Text = string.Format("📄 ไฟล์ระดับ {0}", labelText);
                    LoadFilesByConfidentialityLevel(level);
                    return;
                }
            }

            lblRecentFiles.Text = "📄 ไฟล์ล่าสุด";
            LoadRecentFiles();
        }

        /// <summary>
        /// Reloads lvRecentFiles with the same data as the initial load.
        /// Clears the list and reloads recent files from the file service.
        /// </summary>
        private void ReloadRecentFilesData()
        {
            // Invalidate caches เพื่อให้ดึงข้อมูลใหม่จาก DB
            _cachedAllFiles = null;
            _cachedOrgUnits = null;
            lvRecentFiles.Items.Clear();
            _fileDetailPanel.Visible = false;
            LoadRecentFiles();
            AutoResizeColumnsProportional();
        }

        /// <summary>
        /// Adds a refresh button to the dashboard for reloading recent files.
        /// </summary>
        private void AddRefreshButton()
        {
            // Create refresh button
            var btnRefresh = new Button
            {
                Text      = "🔄 รีเฟรช",
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(43, 87, 154),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(110, 32),
                Location  = new Point(36, 217),
                Cursor    = Cursors.Hand,
                Name      = "btnRefreshFiles"
            };
            btnRefresh.Click += (s, e) => ReloadRecentFilesData();
            
            // Add to pnlDashContent
            pnlDashContent.Controls.Add(btnRefresh);
            btnRefresh.BringToFront();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            _escalationTimer.Stop();

            if (!_logoutRecorded)
            {
                _logoutRecorded = true;
                try
                {
                    _auditService.Log(new AuditEntry
                    {
                        EventType        = "Logout",
                        ActorUserId      = _currentUser.UserId,
                        WorkstationId    = _currentUser.WorkstationId,
                        Outcome          = "Success",
                        TargetEntityType = "UserAccount",
                        TargetEntityId   = _currentUser.UserId.ToString()
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to record logout audit on form close.", ex);
                }
            }
        }

        // -----------------------------------------------------------------------
        // 16.1 — Role-based menu visibility (Req 2.1, 2.4)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Enables/disables menu items based on the current user's role.
        /// Officer and SrOfficer: MOD-002 and MOD-003 only.
        /// Manager: MOD-001 through MOD-004.
        /// Admin: all four modules.
        /// </summary>
        private void ApplyRoleBasedMenuVisibility()
        {
            bool isOfficer  = _currentUser.Role == UserRole.Officer || _currentUser.Role == UserRole.SrOfficer;
            bool isManager  = _currentUser.Role == UserRole.Manager;
            bool isAdmin    = _currentUser.Role == UserRole.Admin;
            bool canManage  = isManager || isAdmin;

            // เครื่องมือ — ทุก role ใช้ได้
            menuItemTools.Enabled  = true;
            menuItemTools.Visible  = true;

            // มุมมอง — ทุก role
            menuItemView.Enabled   = true;
            menuItemView.Visible   = true;

            // ผู้ดูแล — Manager/Admin เท่านั้น
            menuItemAdmin.Enabled  = canManage;
            menuItemAdmin.Visible  = canManage;

            // Admin ribbon visibility
            ribbonStripAdmin.Visible = canManage && ribbonStripAdmin.Visible;

            // ── Grouped admin toolbar visibility ──────────────────────────
            if (_grpDocument   != null) _grpDocument.Visible   = canManage;
            if (_grpApproval   != null) _grpApproval.Visible   = canManage;
            if (_grpOrg        != null) _grpOrg.Visible        = canManage;
            if (_grpPermission != null) _grpPermission.Visible = canManage;
            if (_grpReport     != null) _grpReport.Visible     = canManage;
            if (_grpSystem     != null) _grpSystem.Visible     = isAdmin;
            // Admin-only sub-items inside สิทธิ์ group
            if (_mnuUserAccess != null) _mnuUserAccess.Visible = isAdmin;
            if (_mnuUserMgmt   != null) _mnuUserMgmt.Visible   = isAdmin;

            // User access rights menu item — Admin only
            menuItemMod001.Visible = isAdmin;

            // legacy hidden items
            menuItemMod002.Visible = false;
            menuItemMod003.Visible = false;
            menuItemMod004.Visible = false;

            // Initialize with home ribbon active
            SwitchToHomeRibbon();
            ApplyAdminSectionVisibilityForActiveChild();
        }

        private void ApplyAdminSectionVisibilityForActiveChild()
        {
            if (HasOpenAuthorityConfigForm())
            {
                SetTopMenuVisibilityForAuthorityConfig(true);
                if (_adminTabActive)
                    SwitchToHomeRibbon();
                return;
            }

            bool isManager = _currentUser.Role == UserRole.Manager;
            bool isAdmin = _currentUser.Role == UserRole.Admin;
            bool canManageByRole = isManager || isAdmin;
            bool hideAdminTab = ActiveMdiChild is FolderForm;
            bool showAdminTab = canManageByRole && !hideAdminTab;

            menuItemAdmin.Enabled = showAdminTab;
            menuItemAdmin.Visible = showAdminTab;

            // Keep main form layout/ribbons unchanged; only hide the top Admin tab.
            if (hideAdminTab && _adminTabActive)
                SwitchToHomeRibbon();
        }

        private bool HasOpenAuthorityConfigForm()
        {
            return this.MdiChildren.Any(f => f is AuthorityConfigForm && !f.IsDisposed);
        }

        private void SetTopMenuVisibilityForAuthorityConfig(bool isAuthorityConfigOpen)
        {
            if (isAuthorityConfigOpen)
            {
                menuItemHome.Visible = false;
                menuItemAdmin.Visible = false;
                menuItemShare.Visible = false;
                menuItemManageShares.Visible = false;
                return;
            }

            // Restore default menu state when leaving Authority Config.
            menuItemHome.Enabled = true;
            menuItemHome.Visible = true;

            menuItemShare.Enabled = true;
            menuItemShare.Visible = true;

            menuItemManageShares.Enabled = true;
            menuItemManageShares.Visible = true;

            ApplyRoleBasedMenuVisibility();
            ApplyAdminSectionVisibilityForActiveChild();
        }

        // -----------------------------------------------------------------------
        // 16.2 — Child window management (Req 2.2, 2.3)
        // -----------------------------------------------------------------------

        private void WireMenuHandlers()
        {
            // Tab switching for ribbons
            menuItemHome.Click  += (s, e) => SwitchToHomeRibbon();
            menuItemAdmin.Click += (s, e) => SwitchToAdminRibbon();

            // หน้าหลัก
            menuItemUpload.Click         += (s, e) => OpenFileUpload();
            menuItemNewFolder.Click      += (s, e) => OpenFolderStructure();
            menuItemDownload.Click       += (s, e) => OpenFolderStructure();
            menuItemRename.Click         += (s, e) => OpenFolderStructure();
            menuItemMove.Click           += MenuItemMove_Click;
            menuItemDelete.Click         += (s, e) => OpenFolderStructure();
            menuItemTemplate.Click       += (s, e) => OpenFolderTemplateManagement();
            menuItemFavorites.Click      += (s, e) => OpenFolderStructure();
            menuItemFolderSettings.Click += (s, e) => OpenFolderSettings();
            menuItemPermission.Click     += (s, e) => OpenFolderSettings();
            menuItemAuditLog.Click       += (s, e) => OpenAuditLog();
            menuItemApproval.Click       += (s, e) => OpenApprovalInbox();
            // Share menu items
            menuItemShare.Click          += (s, e) => OpenShareDialog();
            menuItemManageShares.Click   += (s, e) => OpenShareManagement();
            // แทรก
            menuItemInsertDoc.Click      += (s, e) => OpenFileUpload();
            menuItemInsertImage.Click    += (s, e) => OpenFileUpload();
            menuItemInsertZip.Click      += (s, e) => OpenFileUpload();
            // มุมมอง
            menuItemViewList.Click       += (s, e) => OpenFolderStructure();
            menuItemViewGrid.Click       += (s, e) => OpenFolderStructure();
            menuItemViewOrg.Click        += (s, e) => OpenOrgStructure();
            // เครื่องมือ
            menuItemVirusScan.Click      += (s, e) => OpenFolderStructure();
            menuItemChecksum.Click       += (s, e) => OpenFolderStructure();
            menuItemRecycleBin.Click     += (s, e) => OpenFolderStructure();
            menuItemArchive.Click        += (s, e) => OpenFolderStructure();
            // ผู้ดูแล
            menuItemConfig.Click         += (s, e) => OpenSysConfig();
            menuItemOrgAdmin.Click       += (s, e) => OpenOrgStructure();
            menuItemReportExcel.Click    += (s, e) => OpenSysConfig();
            menuItemPDPA.Click           += (s, e) => OpenPdpaConsent();

            // User access rights management (Admin only)
            menuItemMod001.Text   = "จัดการสิทธิ์ผู้ใช้";
            menuItemMod001.Click += (s, e) => OpenUserAccess();
            btnRibbonUserAccess.Click += (s, e) => OpenUserAccess();
            btnRibbonUserMgmt.Click   += (s, e) => OpenUserManagement();
        }

        private void SwitchToHomeRibbon()
        {
            _adminTabActive          = false;
            ribbonStrip.Visible      = !_ribbonCollapsed;
            ribbonStripAdmin.Visible = false;

            // Update tab appearance - Office 2021 Navy Blue Light style
            menuItemHome.BackColor = System.Drawing.Color.White;
            menuItemHome.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            menuItemAdmin.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            menuItemAdmin.ForeColor = System.Drawing.Color.FromArgb(180, 200, 230);
        }

        private void SwitchToAdminRibbon()
        {
            _adminTabActive          = true;
            ribbonStrip.Visible      = false;
            ribbonStripAdmin.Visible = !_ribbonCollapsed;

            // Update tab appearance - Office 2021 Navy Blue Light style
            menuItemHome.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            menuItemHome.ForeColor = System.Drawing.Color.FromArgb(180, 200, 230);
            menuItemAdmin.BackColor = System.Drawing.Color.White;
            menuItemAdmin.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
        }

        /// <summary>
        /// แทนที่ flat buttons ใน ribbonStripAdmin ด้วย ToolStripDropDownButton แบบจัดกลุ่ม
        /// กลุ่ม: เอกสาร | อนุมัติ | องค์กร | สิทธิ์ | รายงาน | ระบบ
        /// </summary>
        private void BuildGroupedAdminToolbar()
        {
            ribbonStripAdmin.Items.Clear();

            // ── กลุ่ม 1: เอกสาร ──────────────────────────────────────────────
            _grpDocument = new ToolStripDropDownButton("📁 เอกสาร");
            _grpDocument.AutoToolTip = false;
            _grpDocument.ToolTipText = "จัดการโครงสร้างและ Template";
            _grpDocument.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("📋 Template",          null, (s, e) => OpenFolderTemplateManagement()),
                new ToolStripMenuItem("📦 Archive Folder",    null, (s, e) => TriggerOnActiveFolderForm(ff => ff.TriggerArchive())),
                new ToolStripMenuItem("📂 คัดลอกโครงสร้าง",  null, (s, e) => TriggerOnActiveFolderForm(ff => ff.TriggerCopyStructure())),
                new ToolStripSeparator(),
                new ToolStripMenuItem("🗑️ ถังขยะ",            null, (s, e) => OpenRecycleBin()),
            });

            // ── กลุ่ม 2: อนุมัติ (direct button) ────────────────────────────
            _grpApproval = new ToolStripButton("✅ อนุมัติ");
            _grpApproval.ToolTipText = "รายการรออนุมัติ";
            _grpApproval.Click += (s, e) => OpenApprovalInbox();

            // ── กลุ่ม 3: องค์กร ──────────────────────────────────────────────
            _grpOrg = new ToolStripDropDownButton("🏢 องค์กร");
            _grpOrg.AutoToolTip = false;
            _grpOrg.ToolTipText = "จัดการโครงสร้างองค์กร";
            _grpOrg.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("🏢 โครงสร้างองค์กร",  null, (s, e) => OpenOrgStructure()),
                new ToolStripMenuItem("🔒 เปลี่ยนระดับความลับ", null, (s, e) => TriggerOnActiveFolderForm(ff => ff.TriggerChangeLevel())),
            });

            // ── กลุ่ม 4: สิทธิ์ ──────────────────────────────────────────────
            _mnuUserAccess = new ToolStripMenuItem("👤 จัดการสิทธิ์ผู้ใช้",  null, (s, e) => OpenUserAccess());
            _mnuUserMgmt   = new ToolStripMenuItem("🗂️ จัดการบัญชีผู้ใช้",   null, (s, e) => OpenUserManagement());

            _grpPermission = new ToolStripDropDownButton("🔐 สิทธิ์");
            _grpPermission.AutoToolTip = false;
            _grpPermission.ToolTipText = "จัดการสิทธิ์การเข้าถึง";
            _grpPermission.DropDownItems.AddRange(new ToolStripItem[]
            {
                _mnuUserAccess,
                _mnuUserMgmt,
                new ToolStripSeparator(),
                new ToolStripMenuItem("🔒 สิทธิ์ Folder",    null, (s, e) => OpenFolderSettings()),
                new ToolStripMenuItem("⚙️ ตั้งค่า Folder",   null, (s, e) => OpenFolderSettings()),
            });

            // ── กลุ่ม 5: รายงาน ──────────────────────────────────────────────
            _grpReport = new ToolStripDropDownButton("📊 รายงาน");
            _grpReport.AutoToolTip = false;
            _grpReport.ToolTipText = "รายงานและ Log การใช้งาน";
            _grpReport.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("📊 รายงาน Excel",  null, (s, e) => OpenSysConfig()),
                new ToolStripMenuItem("🔏 PDPA Consent",  null, (s, e) => OpenPdpaConsent()),
                new ToolStripMenuItem("📋 PDPA Report",   null, (s, e) => OpenPdpaReport()),
                new ToolStripMenuItem("📜 Audit Log",     null, (s, e) => OpenAuditLog()),
            });

            // ── กลุ่ม 6: ระบบ (Admin only) ───────────────────────────────────
            _grpSystem = new ToolStripDropDownButton("⚙️ ระบบ");
            _grpSystem.AutoToolTip = false;
            _grpSystem.ToolTipText = "ตั้งค่าระบบ (Admin เท่านั้น)";
            _grpSystem.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("🛠️ จัดการเมนู",            null, (s, e) => OpenMenuManagement()),
                new ToolStripMenuItem("⚙️ ตั้งค่าระบบ",            null, (s, e) => OpenSysConfig()),
                new ToolStripMenuItem("🔐 จัดการสิทธิ์ Authority", null, (s, e) => OpenAuthorityConfig()),
                new ToolStripMenuItem("🔄 Sync ข้อมูลพนักงาน",    null, (s, e) => SyncEmployeeFromApi()),
                new ToolStripSeparator(),
                new ToolStripMenuItem("📚 ห้องสมุดความรู้ (KMS)",  null, (s, e) => OpenKnowledgeBase()),
                new ToolStripMenuItem("⚙️ ตั้งค่า KB Category",    null, (s, e) => OpenKBCategorySettings()),
            });

            ribbonStripAdmin.Items.AddRange(new ToolStripItem[]
            {
                _grpDocument,
                new ToolStripSeparator(),
                _grpApproval,
                new ToolStripSeparator(),
                _grpOrg,
                new ToolStripSeparator(),
                _grpPermission,
                new ToolStripSeparator(),
                _grpReport,
                new ToolStripSeparator(),
                _grpSystem,
            });

            // ── ปุ่มซ่อน/แสดงแถบ ribbon (ชิดขวาของ menuStrip) ──────────────
            // ลบปุ่มเดิมออกก่อน (กรณี BuildGroupedAdminToolbar ถูกเรียกซ้ำ)
            if (_mnuToggleRibbon != null)
                menuStrip.Items.Remove(_mnuToggleRibbon);

            _mnuToggleRibbon = new ToolStripMenuItem("▲ ซ่อนแถบ");
            _mnuToggleRibbon.Alignment  = ToolStripItemAlignment.Right;
            _mnuToggleRibbon.ForeColor  = System.Drawing.Color.FromArgb(180, 200, 230);
            _mnuToggleRibbon.BackColor  = System.Drawing.Color.Transparent;
            _mnuToggleRibbon.AutoToolTip = false;
            _mnuToggleRibbon.ToolTipText = "ซ่อน/แสดงแถบเครื่องมือ";
            _mnuToggleRibbon.Click += (s, e) =>
            {
                _ribbonCollapsed = !_ribbonCollapsed;
                if (_adminTabActive)
                {
                    ribbonStrip.Visible      = false;
                    ribbonStripAdmin.Visible = !_ribbonCollapsed;
                }
                else
                {
                    ribbonStrip.Visible      = !_ribbonCollapsed;
                    ribbonStripAdmin.Visible = false;
                }
                _mnuToggleRibbon.Text = _ribbonCollapsed ? "▼ แสดงแถบ" : "▲ ซ่อนแถบ";
            };
            menuStrip.Items.Add(_mnuToggleRibbon);

            // ── ปุ่ม Logout (ชิดขวา ถัดจาก toggle ribbon) ───────────────────
            var mnuLogout = new ToolStripMenuItem(
                string.Format("🚪 ออกจากระบบ ({0})", _currentUser.ADUserName ?? _currentUser.AdIdentity));
            mnuLogout.Alignment   = ToolStripItemAlignment.Right;
            mnuLogout.ForeColor   = System.Drawing.Color.FromArgb(255, 180, 180);
            mnuLogout.BackColor   = System.Drawing.Color.Transparent;
            mnuLogout.AutoToolTip = false;
            mnuLogout.ToolTipText = "ออกจากระบบและ Login ด้วยผู้ใช้อื่น";
            mnuLogout.Click += (s, e) => PerformLogout();
            menuStrip.Items.Add(mnuLogout);
        }

        /// <summary>Logout: audit → ปิด form → Program.cs จะแสดง LoginForm ใหม่</summary>
        private void PerformLogout()
        {
            var confirm = MessageBox.Show(
                string.Format("ต้องการออกจากระบบ?\n\nผู้ใช้: {0}", _currentUser.ADUserName ?? _currentUser.AdIdentity),
                "ยืนยันการออกจากระบบ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            LogoutRequested = true;

            // บันทึก audit logout ก่อนปิด
            if (!_logoutRecorded)
            {
                _logoutRecorded = true;
                try
                {
                    _auditService.Log(new AuditEntry
                    {
                        Module      = "Auth",
                        Action      = "LOGOUT",
                        UserID      = _currentUser.UserID,
                        UserName    = _currentUser.ADUserName ?? string.Empty,
                        UserRole    = _currentUser.AppRole,
                        TableName   = "core.UserAccount",
                        RecordID    = _currentUser.UserID,
                        MachineName = _currentUser.MachineName,
                        IPAddress   = _currentUser.IPAddress,
                        Result      = 1,
                        AuditDate   = DateTime.Now
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to record logout audit.", ex);
                }
            }

            this.Close();
        }

        private void WireRibbonHandlers()
        {
            // หน้าหลัก
            btnRibbonUpload.Click         += (s, e) => OpenFileUpload();
            btnRibbonNewFolder.Click      += (s, e) => OpenFolderStructure();
            btnRibbonDownload.Click       += (s, e) => OpenFolderStructure();
            btnRibbonRename.Click         += (s, e) => OpenFolderStructure();
            btnRibbonMove.Click           += MenuItemMove_Click;
            btnRibbonDelete.Click         += (s, e) => OpenFolderStructure();
            btnRibbonTemplate.Click       += (s, e) => OpenFolderTemplateManagement();
            btnRibbonFavorites.Click      += (s, e) => OpenFolderStructure();
            btnRibbonFolderSettings.Click += (s, e) => OpenFolderSettings();
            btnRibbonPermission.Click     += (s, e) => OpenFolderSettings();
            btnRibbonAuditLog.Click       += (s, e) => OpenAuditLog();
            btnRibbonApproval.Click       += (s, e) => OpenApprovalInbox();
            // มุมมอง
            btnRibbonViewList.Click       += (s, e) => OpenFolderStructure();
            btnRibbonViewGrid.Click       += (s, e) => OpenFolderStructure();
            btnRibbonViewOrg.Click        += (s, e) => OpenOrgStructure();
            // เครื่องมือ
            btnRibbonVirusScan.Click      += (s, e) => OpenFolderStructure();
            btnRibbonChecksum.Click       += (s, e) => OpenFolderStructure();
            btnRibbonRecycleBin.Click     += (s, e) => OpenRecycleBin();
            btnRibbonArchive.Click        += (s, e) => OpenFolderStructure();
            // ผู้ดูแล — Folder actions
            btnRibbonFolderArchive.Click  += (s, e) => TriggerOnActiveFolderForm(ff => ff.TriggerArchive());
            btnRibbonChangeLevel.Click    += (s, e) => TriggerOnActiveFolderForm(ff => ff.TriggerChangeLevel());
            btnRibbonCopyStructure.Click  += (s, e) => TriggerOnActiveFolderForm(ff => ff.TriggerCopyStructure());
            // ผู้ดูแล — ระบบ
            btnRibbonSysConfig.Click      += (s, e) => OpenSysConfig();
            btnRibbonOrgAdmin.Click       += (s, e) => OpenOrgStructure();
            btnRibbonReportExcel.Click    += (s, e) => OpenSysConfig();
            btnRibbonPDPA.Click           += (s, e) => OpenPdpaConsent();

            //ตั้งค่า Category ของระบบ KMS
            toolStripButton1.Click       += (s, e) => OpenConfigCategoryKMS();

            // ── ปุ่ม "📋 Template" — สร้าง Folder จาก Template สำหรับทุก user ที่มีสิทธิ์ ──
            var sepTemplate = new ToolStripSeparator();
            var btnTemplate = new ToolStripButton
            {
                Name         = "btnApplyTemplateHome",
                Text         = "📋 Template",
                ToolTipText  = "สร้าง Folder จาก Template",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font         = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor    = Color.FromArgb(25, 55, 105),
            };
            btnTemplate.Click += (s, e) => TriggerOnActiveFolderForm(ff => ff.TriggerApplyTemplate());
            ribbonStrip.Items.Add(sepTemplate);
            ribbonStrip.Items.Add(btnTemplate);

            // ── ปุ่ม "สิทธิ์ของฉัน" — แสดงใน ribbonStrip (Home tab) สำหรับทุก user ──
            var sepMyPerm = new ToolStripSeparator();
            var btnMyPerm = new ToolStripButton
            {
                Name         = "btnMyPermission",
                Text         = "🔐 สิทธิ์ของฉัน",
                ToolTipText  = "แสดงสิทธิ์การเข้าใช้งาน folder ของคุณ",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font         = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor    = Color.FromArgb(25, 55, 105),
            };
            btnMyPerm.Click += (s, e) =>
            {
                RefreshCurrentUserAuthority();
                using (var frm = new UserPermissionSummaryForm(_currentUser, _shareLookupController))
                    frm.ShowDialog(this);
            };
            ribbonStrip.Items.Add(sepMyPerm);
            ribbonStrip.Items.Add(btnMyPerm);

            // ── ปุ่ม "📥 แบ่งปันให้ฉัน" + badge ────────────────────────────
            var sepShared = new ToolStripSeparator();
            var btnShared = new ToolStripButton
            {
                Name         = "btnSharedWithMe",
                Text         = "📥 แบ่งปันให้ฉัน",
                ToolTipText  = "ดูรายการ Folder / File ที่แบ่งปันให้คุณ",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font         = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor    = Color.FromArgb(25, 55, 105),
            };
            btnShared.Click += (s, e) => OpenSharedWithMe();

            _lblSharedBadge = new ToolStripLabel
            {
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                Font      = new Font("Microsoft Sans Serif", 7.5f, FontStyle.Bold),
                Padding   = new Padding(3, 1, 3, 1),
                Visible   = false,
                ToolTipText = "รายการแบ่งปันใหม่ภายใน 7 วัน",
            };

            ribbonStrip.Items.Add(sepShared);
            ribbonStrip.Items.Add(btnShared);
            ribbonStrip.Items.Add(_lblSharedBadge);

            // โหลด badge count ตอนเริ่มต้น
            RefreshSharedWithMeBadge();
        }

        private void RefreshCurrentUserAuthority()
        {
            if (_currentUser == null || _folderAuthorityService == null || _userAccountLookupController == null)
                return;

            try
            {
                var latest = _userAccountLookupController.GetUserById(_currentUser.UserID);
                if (latest == null) return;

                _currentUser.LevelCode = latest.LevelCode;
                _currentUser.PositionName = latest.PositionName;
                _currentUser.DepartmentCode = latest.DepartmentCode;
                _currentUser.DepartmentName = latest.DepartmentName;
                _currentUser.Authority = _folderAuthorityService.ResolveForUser(latest);
            }
            catch (Exception ex)
            {
                _logger.Warn("RefreshCurrentUserAuthority failed. Using cached authority. " + ex.Message);
            }
        }

        private void OpenSharedWithMe()
        {
            if (_shareLookupController == null)
            {
                MessageBox.Show("ไม่สามารถโหลดข้อมูลได้ — Share lookup service ไม่พร้อมใช้งาน",
                    "แบ่งปันให้ฉัน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var frm = new SharedWithMeForm(_currentUser, _shareLookupController))
            {
                // เมื่อ user ดับเบิลคลิก Folder: navigate FolderForm
                frm.OpenFolderRequested += folderId =>
                {
                    var ff = MdiChildren.OfType<FolderForm>().FirstOrDefault();
                    if (ff == null)
                    {
                        OpenFolderStructure();
                        ff = MdiChildren.OfType<FolderForm>().FirstOrDefault();
                    }
                    if (ff != null)
                    {
                        frm.Hide();
                        ff.NavigateToFolderById(folderId);
                        ff.BringToFront();
                        ff.Focus();
                    }
                };

                // เมื่อ user ดับเบิลคลิก ไฟล์: เปิด SharedFileViewForm
                frm.OpenFileRequested += (folderId, fileId) =>
                {
                    using (var fileView = new SharedFileViewForm(
                        fileId, _fileBrowseController, _shareLookupController, _currentUser))
                    {
                        fileView.ShowDialog(this);
                    }
                };

                frm.ShowDialog(this);
                // อัพเดต badge หลังปิด
                UpdateSharedWithMeBadge(frm.NewItemCount);
            }
        }

        /// <summary>โหลด badge count จากฐานข้อมูลโดยไม่เปิด form</summary>
        private void RefreshSharedWithMeBadge()
        {
            if (_shareLookupController == null) return;
            try
            {
                var cutoff = DateTime.Today.AddDays(-7);
                int count = _shareLookupController
                    .GetFolderPermissionsForUser(_currentUser.UserID)
                    .Count(p => p.RevokedDate == null
                             && !p.IsDeny
                             && p.GrantedDate.Date >= cutoff
                             && (!p.ExpiryDate.HasValue || p.ExpiryDate.Value.Date >= DateTime.Today));
                UpdateSharedWithMeBadge(count);
            }
            catch { /* ไม่ให้ crash ถ้า DB ไม่พร้อม */ }
        }

        private void UpdateSharedWithMeBadge(int count)
        {
            if (_lblSharedBadge == null) return;
            if (count > 0)
            {
                _lblSharedBadge.Text    = count.ToString();
                _lblSharedBadge.Visible = true;
            }
            else
            {
                _lblSharedBadge.Visible = false;
            }
        }

        private void OpenFolderTemplateManagement()
        {
            if (_folderTemplateController == null || !_folderTemplateController.IsAvailable)
            {
                MessageBox.Show("Folder Template service is not available.", "Template",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var dlg = new FolderTemplateForm(_folderTemplateController, _currentUser))
                dlg.ShowDialog(this);
        }

        private void OpenOrgStructure()
        {
            if (_orgStructureService != null)
                OpenChildForm(new OrgStructureForm(_orgStructureService, _currentUser, _authorityOrgLookupController));
            else
                MessageBox.Show("Org Structure service is not available.", "MOD-001",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void OpenFolderStructure()
        {
            // Validate all required services are available
            if (_folderReadLookupController == null || _folderCommandController == null)
            {
                _logger.Error("Folder controllers are not available");
                MessageBox.Show("Folder controllers are not available.", "MOD-002",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Create FolderForm with all required dependencies
                var ff = new FolderForm(
                    user: _currentUser,
                    orgUnitId: null,
                    fileBrowseController: _fileBrowseController,
                    fileCommandController: _fileCommandController,
                    folderCommandController: _folderCommandController,
                    orgLookupController: _userAccessOrgLookupController,
                    folderTemplateController: _folderTemplateController,
                    uploadCheckpointController: _uploadCheckpointController,
                    quickAccessController: _quickAccessController,
                    approvalFolderController: _approvalFolderController,
                    permissionCheckController: _permissionCheckController,
                    auditLogController: _auditLogController,
                    shareCommandController: _shareCommandController,
                    menuManagementController: _menuManagementController,
                    approvalInboxController: _approvalInboxController,
                    recycleBinController: _recycleBinController,
                    fileUploadController: _fileUploadController,
                    shareManagementController: _shareManagementController,
                    shareLookupController: _shareLookupController,
                    folderSettingsController: _folderSettingsController,
                    folderPermissionLookupController: _folderPermissionLookupController,
                    userAccountLookupController: _userAccountLookupController,
                    userAdminController: _userAdminController,
                    folderReadLookupController: _folderReadLookupController
                );

                // Wire event handlers
                ff.OpenPermissionRequested     += (s, folder) => OpenFolderSettingsForFolder(folder);
                ff.OpenFilePermissionRequested += (s, file)   => OpenFilePermissionDialog(file);
                ff.SelectionChanged            += (s, ev)     => UpdateMoveControlsState();
                ff.MenuActionRequested         += HandleFolderMenuActionRequested;

                // Open as MDI child
                OpenChildForm(ff);

                _logger.Info("FolderForm (MOD-002) opened successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating FolderForm: {ex.Message}", ex);
                MessageBox.Show($"Error opening Folder Manager: {ex.Message}", "MOD-002",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenFolderSettings()
        {
            // Get selected folder from active FolderForm
            Folder selectedFolder = null;
            FolderForm activeFolderForm = null;
            foreach (Form child in MdiChildren)
            {
                var ff = child as FolderForm;
                if (ff != null) { activeFolderForm = ff; selectedFolder = ff.SelectedFolder; break; }
            }

            // Fallback: use first folder in tree when none is explicitly selected
            if (selectedFolder == null && activeFolderForm != null)
                selectedFolder = activeFolderForm.FirstFolderInTree;

            if (selectedFolder == null)
            {
                MessageBox.Show(
                    "กรุณาเลือก Folder ใน Folder Explorer ก่อน",
                    "ตั้งค่า Folder",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                OpenFolderStructure();
                return;
            }

            OpenFolderSettingsForFolder(selectedFolder);
        }

        /// <summary>เปิด PermissionForm สำหรับ folder ที่ระบุ — เรียกได้จาก FolderForm context menu</summary>
        public void OpenFolderSettingsForFolder(Folder selectedFolder)
        {
            if (selectedFolder == null) return;

            if (_permissionService == null || _folderService == null || _folderPermissionLookupController == null || _folderSettingsController == null)
            {
                MessageBox.Show("Permission service is not available.", "ตั้งค่า Folder",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            } 

            using (var dlg = new PermissionForm(
                permissionService: _permissionService,
                approvalService: _approvalService,
                folderService: _folderService,
                folderPermissionLookupController: _folderPermissionLookupController,
                user: _currentUser,
                folder: selectedFolder,
                userAccountLookupController: _userAccountLookupController,
                folderSettingsController: _folderSettingsController,
                employeeApiService: null,
                orgLookupController: _userAccessOrgLookupController,
                fileDocumentLookupController: _fileDocumentLookupController,
                auditService: _auditService))
            {
                dlg.ShowDialog(this);
            }
        }

        /// <summary>เปิด FilePermissionForm สำหรับ file ที่ระบุ — เรียกได้จาก FolderForm context menu และ detail panel</summary>
        public void OpenFilePermissionDialog(FileDocument file)
        {
            if (file == null) return;

            if (_filePermissionAclController == null || _filePermissionCommandController == null)
            {
                MessageBox.Show("File permission controller is not available.", "ตั้งค่าสิทธิ์ไฟล์",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dlg = new FilePermissionForm(
                filePermissionCommandController: _filePermissionCommandController,
                filePermissionAclController: _filePermissionAclController,
                currentUser: _currentUser,
                file: file,
                userAccountLookupController: _userAccountLookupController,
                orgLookupController: _userAccessOrgLookupController))
            {
                dlg.ShowDialog(this);
            }
        }

        public void OpenApprovalInbox()
        {
            if (_approvalInboxController == null)
            {
                MessageBox.Show("Approval inbox controller is not available.", "Approval Inbox",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dlg = new ApprovalInboxForm(_approvalInboxController, _currentUser))
            {
                dlg.ShowDialog(this);

                // หลัง ApprovalInboxForm ปิด → reload FolderForm เพื่อแสดง 1HC ที่เพิ่งอนุมัติ
                var ff = MdiChildren.OfType<FolderForm>().FirstOrDefault();
                if (ff != null)
                    ff.ReloadFolderTree();
            }
        }

        private void OpenConfigCategoryKMS()
        {
            OpenKnowledgeBase();
        }

        private void OpenKnowledgeBase()
        {
            if (_kbCategoryService == null || _kbArticleService == null)
            {
                MessageBox.Show("Knowledge Base service ไม่พร้อมใช้งาน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var form      = new KBMainForm();
            var presenter = new KBMainController(
                form, _kbCategoryService, _kbArticleService, _currentUser);

            form.AttachPresenter(presenter, _currentUser);
            form.AttachFactories(
                // viewerFactory — สร้าง ArticleViewerForm พร้อม presenter
                article =>
                {
                    var viewer          = new ArticleViewerForm();
                    var viewerPresenter = new ArticleViewerController(
                        viewer, _kbArticleService, _currentUser, article);
                    viewer.AttachPresenter(viewerPresenter);
                    viewer.AttachEditorFactory(a => OpenKBEditorDialog(a));
                    return viewer;
                },
                // editorFactory — เปิด KBEditorDialog พร้อม presenter
                article => OpenKBEditorDialog(article),
                // categorySettingsFactory — เปิดหน้าตั้งค่า Category
                () => OpenKBCategorySettings());

            // importWizardFactory — สร้าง KBImportWizardForm พร้อม presenter
            form.AttachImportWizardFactory(() =>
            {
                var wizard          = new KBImportWizardForm(_currentUser);
                var wizardPresenter = new KBImportWizardController(
                    wizard, _kbArticleService, _kbCategoryService, _currentUser);
                wizard.AttachPresenter(wizardPresenter);
                return wizard.ShowDialog(this);
            });

            OpenChildForm(form);
        }

        /// <summary>
        /// Composition-root helper: สร้าง KBEditorDialog พร้อม presenter ครบชุด
        /// แล้ว ShowDialog() คืนค่า DialogResult กลับมา
        /// </summary>
        private DialogResult OpenKBEditorDialog(KBArticle article)
        {
            var dlg       = new KBEditorDialog(_currentUser);
            var presenter = new KBEditorController(
                dlg, _kbCategoryService, _kbArticleService, _currentUser, article);
            dlg.AttachPresenter(presenter);
            return dlg.ShowDialog(this);
        }

        private void OpenShareDialog()
        {
            if (_shareCommandController == null)
            {
                MessageBox.Show("Share command controller is not available.", "Share File",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get selected file/folder from active FolderForm
            FileDocument selectedFile = null;
            Folder selectedFolder = null;

            foreach (Form child in MdiChildren)
            {
                var ff = child as FolderForm;
                if (ff != null)
                {
                    selectedFile = ff.SelectedFile;
                    selectedFolder = ff.SelectedFolder;
                    break;
                }
            }

            if (selectedFile == null && selectedFolder == null)
            {
                MessageBox.Show(
                    "Please select a file or folder in Folder Explorer first.",
                    "Select File/Folder",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Show dialog to choose share type — modern style
            using (var dlg = new Form())
            {
                Color navyDark  = Color.FromArgb(25, 55, 105);
                Color navyMid   = Color.FromArgb(50, 90, 150);
                Color navyLight = Color.FromArgb(215, 228, 248);
                Color bodyBg    = Color.FromArgb(243, 246, 251);

                dlg.Text            = "เลือกประเภทการแบ่งปัน";
                dlg.ClientSize      = new System.Drawing.Size(340, 170);
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.BackColor       = bodyBg;
                dlg.Font            = new Font("Segoe UI", 9f);

                // Header bar
                var pnlH = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = navyDark };
                var lblH = new Label
                {
                    Text      = "🔗  เลือกประเภทการแบ่งปัน",
                    Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize  = true,
                    Location  = new Point(14, 14),
                };
                pnlH.Controls.Add(lblH);

                // Hint label
                var lblHint = new Label
                {
                    Text      = "  เลือกแบ่งปันภายในองค์กร หรือสร้าง link สำหรับบุคคลภายนอก",
                    Font      = new Font("Segoe UI", 8.5f),
                    ForeColor = Color.FromArgb(70, 85, 110),
                    BackColor = navyLight,
                    Dock      = DockStyle.None,
                    Size      = new Size(340, 26),
                    Location  = new Point(0, 52),
                    TextAlign = ContentAlignment.MiddleLeft,
                };

                // Internal button
                var btnInternal = new Button
                {
                    Text         = "🏢  ภายใน (Internal)",
                    Size         = new Size(148, 56),
                    Location     = new Point(14, 88),
                    FlatStyle    = FlatStyle.Flat,
                    BackColor    = navyDark,
                    ForeColor    = Color.White,
                    Font         = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    DialogResult = DialogResult.Yes,
                };
                btnInternal.FlatAppearance.BorderSize = 0;

                // External button
                var btnExternal = new Button
                {
                    Text         = "🌐  ภายนอก (External)",
                    Size         = new Size(148, 56),
                    Location     = new Point(174, 88),
                    FlatStyle    = FlatStyle.Flat,
                    BackColor    = Color.FromArgb(220, 228, 245),
                    ForeColor    = navyDark,
                    Font         = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    DialogResult = DialogResult.No,
                };
                btnExternal.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
                btnExternal.FlatAppearance.BorderSize  = 1;

                dlg.Controls.AddRange(new Control[] { pnlH, lblHint, btnInternal, btnExternal });
                dlg.AcceptButton = btnInternal;
                dlg.CancelButton = btnExternal;

                if (dlg.ShowDialog(this) == DialogResult.Yes)
                {
                    // Internal share
                    using (var shareDialog = new ShareDialogInternal(
                        _shareCommandController,
                        _userAccountLookupController, _userAccessOrgLookupController, _currentUser,
                        selectedFile, selectedFolder))
                    {
                        shareDialog.ShowDialog(this);
                    }
                }
                else
                {
                    // External share
                    using (var shareDialog = new ShareDialogExternal(
                        _shareCommandController, _currentUser,
                        selectedFile, selectedFolder))
                    {
                        shareDialog.ShowDialog(this);
                    }
                }
            }
        }

        private void OpenShareManagement()
        {
            if (_shareManagementController == null)
            {
                MessageBox.Show("Share management controller is not available.", "Manage Shares",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dlg = new ShareManagementForm(_shareManagementController, _currentUser, _shareLookupController))
            {
                dlg.ShowDialog(this);
            }
        }

        public void OpenFileUpload()
        {
            if (_fileUploadController == null)
            {
                new FileUploadForm().ShowDialog(this);
                return;
            }

            // ถ้า FolderForm เปิดอยู่แล้ว
            foreach (Form child in MdiChildren)
            {
                var ff = child as FolderForm;
                if (ff != null)
                {
                    if (ff.SelectedFolder != null)
                    {
                        ff.OpenUploadForSelectedFolder();
                        return;
                    }
                    else
                    {
                        ff.Activate();
                        MessageBox.Show(
                            "กรุณาเลือก Folder ปลายทางใน Folder Explorer ก่อนอัพโหลด\n\n" +
                            "1. กด 🏢 เลือกหน่วยงาน เพื่อกรอง\n" +
                            "2. คลิก Folder ใน tree ด้านซ้าย\n" +
                            "3. กด 📤 ปุ่มอัพโหลด อีกครั้ง",
                            "เลือก Folder ก่อน",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }
                }
            }

            // ยังไม่มี FolderForm — เปิดให้เลย พร้อมแจ้ง
            var folderForm = new FolderForm(
                user: _currentUser,
                orgUnitId: null,
                fileBrowseController: _fileBrowseController,
                fileCommandController: _fileCommandController,
                folderCommandController: _folderCommandController,
                orgLookupController: _userAccessOrgLookupController,
                folderTemplateController: _folderTemplateController,
                uploadCheckpointController: _uploadCheckpointController,
                quickAccessController: _quickAccessController,
                approvalFolderController: _approvalFolderController,
                permissionCheckController: _permissionCheckController,
                auditLogController: _auditLogController,
                shareCommandController: _shareCommandController,
                menuManagementController: _menuManagementController,
                approvalInboxController: _approvalInboxController,
                recycleBinController: _recycleBinController,
                fileUploadController: _fileUploadController,
                shareManagementController: _shareManagementController,
                shareLookupController: _shareLookupController,
                folderSettingsController: _folderSettingsController,
                folderPermissionLookupController: _folderPermissionLookupController,
                userAccountLookupController: _userAccountLookupController,
                userAdminController: _userAdminController,
                folderReadLookupController: _folderReadLookupController);
            folderForm.OpenPermissionRequested     += (s, folder) => OpenFolderSettingsForFolder(folder);
            folderForm.OpenFilePermissionRequested += (s, file)   => OpenFilePermissionDialog(file);
            folderForm.SelectionChanged            += (s, ev)     => UpdateMoveControlsState();
            folderForm.MenuActionRequested         += HandleFolderMenuActionRequested;
            OpenChildForm(folderForm);
        }

        private bool HandleFolderMenuActionRequested(object sender, string actionKey)
        {
            switch ((actionKey ?? string.Empty).Trim())
            {
                case "ConfigAdmin":
                case "ReportExcel":
                    OpenSysConfig();
                    return true;
                case "PdpaReport":
                    OpenPdpaReport();
                    return true;
                case "AuthorityConfig":
                    OpenAuthorityConfig();
                    return true;
                case "SyncEmployee":
                    SyncEmployeeFromApi();
                    return true;
                case "KnowledgeBaseForm":
                case "KBMainForm":
                    OpenKnowledgeBase();
                    return true;
                case "KBCategoryForm":
                    OpenKBCategorySettings();
                    return true;
                case "KBCategoryConfig":
                    OpenKBCategorySettings();
                    return true;
                case "AuditLog":
                    OpenAuditLog();
                    return true;
                case "OrgStructure":
                    OpenOrgStructure();
                    return true;
                case "UserManagement":
                    OpenUserManagement();
                    return true;
                case "UserAccess":
                    OpenUserAccess();
                    return true;
                case "MenuManagement":
                    OpenMenuManagement();
                    return true;
                case "PermissionAdmin":
                case "FolderSettings":
                    OpenFolderSettings();
                    return true;
                default:
                    return false;
            }
        }

        public void OpenAuditLog()
        {
            OpenChildForm(new AuditLogForm(_auditService, _currentUser));
        }

        public void OpenSysConfig()
        {
            if (_configManager != null)
            {
                var form = new ConfigAdminForm(_configManager, _auditService, _currentUser);
                form.GeneratePdpaReportRequested += (s, e) => OpenPdpaReport();
                OpenChildForm(form);
            }
            else
                MessageBox.Show("Config service is not available.", "Admin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void OpenAuthorityConfig()
        {
            if (_authorityConfigController != null)
            {
                var form = new AuthorityConfigForm(
                    authorityConfigController: _authorityConfigController,
                    user: _currentUser,
                    authorityOrgLookupController: _authorityOrgLookupController);

                SetTopMenuVisibilityForAuthorityConfig(true);
                form.FormClosed += (s, e) =>
                {
                    if (!HasOpenAuthorityConfigForm())
                        SetTopMenuVisibilityForAuthorityConfig(false);
                };

                OpenChildForm(form, true);
            }
            else
                MessageBox.Show("FolderAuthority service ไม่พร้อมใช้งาน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void OpenKBCategorySettings()
        {
            if (_kbCategoryService == null)
            {
                MessageBox.Show("KB Category service ไม่พร้อมใช้งาน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var form      = new KBCategoryForm(_currentUser);
            var presenter = new KBCategoryController(form, _kbCategoryService, _currentUser);
            form.AttachPresenter(presenter);
            OpenChildForm(form);
        }
        private void SyncEmployeeFromApi()
        {
            if (_employeeDirectoryController == null || _userAdminController == null)
            {
                MessageBox.Show("Employee API service ไม่พร้อมใช้งาน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                "ต้องการ Sync ข้อมูลพนักงานจาก API สำหรับ users ทั้งหมดหรือไม่?\n\n" +
                "ระบบจะ match ADUserName กับ EmployeeCode จาก API\n" +
                "และอัพเดท Department / Division / Position",
                "Sync Employee Info",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                int updated = 0, skipped = 0, failed = 0;

                var allUsers     = _userAdminController.GetAllUsers().ToList();
                var allEmployees = _employeeDirectoryController.GetAllEmployees().ToList();

                var empLookup = new Dictionary<string, CMS.Domain.Models.EmployeeSummary>(
                    StringComparer.OrdinalIgnoreCase);
                foreach (var emp in allEmployees)
                    if (!string.IsNullOrEmpty(emp.EmployeeCode))
                        empLookup[emp.EmployeeCode] = emp;

                foreach (var u in allUsers)
                {
                    try
                    {
                        string adUser = u.ADUserName ?? "";
                        CMS.Domain.Models.EmployeeSummary emp = null;

                        if (!empLookup.TryGetValue(adUser, out emp) && adUser.Contains("\\"))
                        {
                            string username = adUser.Substring(adUser.LastIndexOf('\\') + 1);
                            empLookup.TryGetValue(username, out emp);
                        }

                        if (emp == null) { skipped++; continue; }

                        _userAdminController.UpdateEmployeeInfo(
                            u.UserID, emp.EmployeeCode,
                            null, null, null,
                            null, null, null,
                            emp.DepartmentId, emp.DepartmentCode, emp.DepartmentNameEn, emp.DepartmentNameTh,
                            emp.DivisionId,   emp.DivisionCode,   emp.DivisionNameEn,   emp.DivisionNameTh,
                            null);
                        updated++;
                    }
                    catch { failed++; }
                }

                Cursor = Cursors.Default;
                MessageBox.Show(
                    string.Format("Sync เสร็จสิ้น\n\n✓ อัพเดท: {0} คน\n— ไม่พบใน API: {1} คน\n✗ ผิดพลาด: {2} คน",
                        updated, skipped, failed),
                    "Sync Employee Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenUserAccess()
        {
            if (_userAdminController == null || _userAccessOrgLookupController == null)
            {
                MessageBox.Show("ไม่พบ service สำหรับจัดการสิทธิ์ผู้ใช้", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var auditLogController = new AuditLogController(_auditService);

            OpenChildForm(new UserAccessForm(
                _userAdminController,
                _userAccessOrgLookupController,
                _currentUser,
                _userOrgVisibilityController,
                auditLogController));
        }

        private void OpenMenuManagement()
        {
            if (_currentUser.AppRole != 4)  // Admin only
            {
                MessageBox.Show("⛔ เฉพาะ Admin เท่านั้นที่สามารถจัดการเมนูได้", "ไม่มีสิทธิ์",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_menuManagementController == null)
            {
                MessageBox.Show("❌ ไม่พบ Menu Management Controller สำหรับจัดการเมนู", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _logger.Info($"ผู้ใช้ {_currentUser.AdIdentity} เปิด Menu Management Form");
                OpenChildForm(new MenuManagementForm(_menuManagementController, _currentUser));
            }
            catch (Exception ex)
            {
                _logger.Error($"เกิดข้อผิดพลาดในการเปิด Menu Management: {ex.Message}", ex);
                MessageBox.Show($"❌ เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenUserManagement()
        {
            if (_userAdminWithHrController == null)
            {
                MessageBox.Show("ไม่พบ service สำหรับจัดการบัญชีผู้ใช้", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            OpenChildForm(new UserManagementForm(
                _userAdminWithHrController,
                _currentUser,
                _auditService));
        }

        private void OpenPdpaConsent()
        {
            try
            {
                using (var pdpaDialog = new PdpaConsentDialog())
                {
                    var result = pdpaDialog.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        MessageBox.Show("ขอบคุณสำหรับการยืนยันนโยบายข้อมูลส่วนบุคคล",
                            "PDPA Consent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการเปิด PDPA Consent: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenPdpaReport()
        {
            if (_userAccountLookupController == null || _fileDocumentLookupController == null || _auditService == null)
            {
                MessageBox.Show("ไม่พบ service ที่จำเป็นสำหรับ PDPA Report", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var users = _userAccountLookupController.GetAllUsers().OrderBy(u => u.DisplayName).ToList();
            if (!users.Any())
            {
                MessageBox.Show("ไม่พบข้อมูลผู้ใช้", "PDPA Report", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UserAccount selectedUser = null;
            using (var dlg = new Form())
            {
                dlg.Text            = "เลือกผู้ใช้สำหรับ PDPA Report";
                dlg.ClientSize      = new Size(480, 380);
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.BackColor       = Color.FromArgb(245, 247, 250);
                dlg.Font            = new Font("Segoe UI", 9f);

                // ── Header ────────────────────────────────────────────────────
                var pnlHdr = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(0, 51, 102) };
                pnlHdr.Controls.Add(new Label
                {
                    Text      = "📋  เลือกผู้ใช้สำหรับ PDPA Report",
                    Dock      = DockStyle.Fill,
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(16, 0, 0, 0)
                });

                // ── Footer ────────────────────────────────────────────────────
                var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.FromArgb(238, 240, 245) };
                var footerLine = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(210, 215, 225) };

                var btnOk = new Button
                {
                    Text      = "ตกลง",
                    Size      = new Size(90, 30),
                    BackColor = Color.FromArgb(0, 51, 102),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Enabled   = false,
                    Anchor    = AnchorStyles.Right | AnchorStyles.Top
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text         = "ยกเลิก",
                    Size         = new Size(80, 30),
                    BackColor    = Color.White,
                    ForeColor    = Color.FromArgb(60, 60, 60),
                    FlatStyle    = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel,
                    Anchor       = AnchorStyles.Right | AnchorStyles.Top
                };
                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 190);

                pnlFooter.Controls.Add(footerLine);
                pnlFooter.Controls.Add(btnOk);
                pnlFooter.Controls.Add(btnCancel);
                pnlFooter.Resize += (s, e) =>
                {
                    btnCancel.Left = pnlFooter.ClientSize.Width - btnCancel.Width - 12;
                    btnOk.Left     = btnCancel.Left - btnOk.Width - 8;
                    btnOk.Top      = btnCancel.Top = (pnlFooter.Height - btnOk.Height) / 2 + 1;
                };

                // ── Search box ────────────────────────────────────────────────
                var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Color.White, Padding = new Padding(8, 5, 8, 5) };
                bool isPlaceholder = true;
                var txtSearch = new TextBox
                {
                    Dock      = DockStyle.Fill,
                    Font      = new Font("Segoe UI", 10f),
                    ForeColor = Color.Gray,
                    Text      = "🔍  ค้นหาชื่อผู้ใช้..."
                };
                txtSearch.GotFocus  += (s, e) => { if (!isPlaceholder) return; isPlaceholder = false; txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; };
                txtSearch.LostFocus += (s, e) => { if (!string.IsNullOrWhiteSpace(txtSearch.Text)) return; isPlaceholder = true; txtSearch.Text = "🔍  ค้นหาชื่อผู้ใช้..."; txtSearch.ForeColor = Color.Gray; };
                pnlSearch.Controls.Add(txtSearch);

                // ── ListView ──────────────────────────────────────────────────
                var lv = new ListView
                {
                    Dock          = DockStyle.Fill,
                    View          = View.Details,
                    FullRowSelect = true,
                    GridLines     = false,
                    MultiSelect   = false,
                    Font          = new Font("Segoe UI", 10f),
                    BackColor     = Color.White,
                    BorderStyle   = BorderStyle.None
                };
                lv.Columns.Add("ชื่อ-นามสกุล", 220);
                lv.Columns.Add("AD Username",  140);
                lv.Columns.Add("ตำแหน่ง",      100);

                var allItems = new List<ListViewItem>();
                foreach (var u in users)
                {
                    string displayName = !string.IsNullOrEmpty(u.FullNameTh) ? u.FullNameTh
                                       : !string.IsNullOrEmpty(u.DisplayName) ? u.DisplayName
                                       : u.ADUserName ?? "-";
                    var item = new ListViewItem(displayName);
                    item.SubItems.Add(u.ADUserName ?? "-");
                    item.SubItems.Add(!string.IsNullOrEmpty(u.PositionName) ? u.PositionName
                                     : u.AppRole == 4 ? "Admin" : "User");
                    item.Tag = u;
                    allItems.Add(item);
                }
                lv.Items.AddRange(allItems.ToArray());

                // search filter
                txtSearch.TextChanged += (s, e) =>
                {
                    if (isPlaceholder) return;
                    string kw = txtSearch.Text.Trim().ToLower();
                    lv.BeginUpdate();
                    lv.Items.Clear();
                    var filtered = string.IsNullOrEmpty(kw)
                        ? allItems
                        : allItems.Where(r =>
                            r.Text.ToLower().Contains(kw) ||
                            (r.SubItems.Count > 1 && r.SubItems[1].Text.ToLower().Contains(kw))).ToList();
                    lv.Items.AddRange(filtered.ToArray());
                    lv.EndUpdate();
                    btnOk.Enabled = lv.SelectedItems.Count > 0;
                };

                lv.SelectedIndexChanged += (s, e) => btnOk.Enabled = lv.SelectedItems.Count > 0;

                Action confirm = () =>
                {
                    if (lv.SelectedItems.Count == 0) return;
                    dlg.DialogResult = DialogResult.OK;
                    dlg.Close();
                };
                btnOk.Click    += (s, e) => confirm();
                lv.DoubleClick += (s, e) => confirm();

                // ── Assemble ──────────────────────────────────────────────────
                var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1) };
                pnlBody.Controls.Add(lv);
                pnlBody.Controls.Add(pnlSearch);

                dlg.Controls.Add(pnlBody);
                dlg.Controls.Add(pnlFooter);
                dlg.Controls.Add(pnlHdr);
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;
                dlg.Shown += (s, e) => txtSearch.Focus();

                if (dlg.ShowDialog(this) != DialogResult.OK || lv.SelectedItems.Count == 0) return;
                selectedUser = (UserAccount)lv.SelectedItems[0].Tag;
            }

            try
            {
                var files = _fileDocumentLookupController.GetAllGroupedByFolder()
                    .Where(f => f.UploadedBy == selectedUser.UserID)
                    .ToList();

                var auditResult = _auditService.Query(new CMS.Data.Models.AuditQuery
                {
                    UserID     = selectedUser.UserID,
                    FromDate   = DateTime.MinValue,
                    ToDate     = DateTime.Now,
                    PageNumber = 1,
                    PageSize   = 5000
                }, _currentUser);

                var report = new CMS.Services.DTOs.PdpaReport
                {
                    Subject       = selectedUser,
                    FileDocuments = files,
                    AuditLogs     = auditResult?.Items ?? Enumerable.Empty<AuditLog>()
                };

                OpenChildForm(new PdpaReportForm(report));
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการสร้าง PDPA Report: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens a form as an MDI child. Multiple instances may be open concurrently.
        /// </summary>
        private void OpenChildForm(Form childForm, bool maximize = false)
        {
            if (childForm == null)
                throw new ArgumentNullException(nameof(childForm));

            try
            {
                // Hide dashboard when opening MDI child forms
                pnlDashboard.Visible = false;

                // Configure form as MDI child (not as dialog)
                childForm.MdiParent = this;
                childForm.WindowState = maximize ? FormWindowState.Maximized : FormWindowState.Normal;

                // Show as MDI child form (not ShowDialog)
                childForm.Show();

                // Activate to bring to front
                childForm.Activate();

                _logger.Info($"Opened MDI child form: {childForm.GetType().Name}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error opening child form: {ex.Message}", ex);
                MessageBox.Show($"Error opening form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // -----------------------------------------------------------------------
        // Status bar helpers
        // -----------------------------------------------------------------------

        private void UpdateStatusBar()
        {
            // แสดง: 👤 username (Role) | LevelCode
            string levelInfo = !string.IsNullOrEmpty(_currentUser.LevelCode)
                ? string.Format(" | Level: {0}", _currentUser.LevelCode)
                : "";
            lblStatusUser.Text = string.Format("👤 {0} ({1}){2}",
                _currentUser.AdIdentity, _currentUser.Role, levelInfo);

            // แสดง: 🏢 ตำแหน่ง — แผนก
            string deptInfo = "";
            if (!string.IsNullOrEmpty(_currentUser.PositionName) || !string.IsNullOrEmpty(_currentUser.DepartmentName))
            {
                string pos  = _currentUser.PositionName   ?? "";
                string dept = _currentUser.DepartmentName ?? _currentUser.DepartmentCode ?? "";
                deptInfo = string.Format("🏢 {0}{1}",
                    pos,
                    !string.IsNullOrEmpty(dept) ? " — " + dept : "");
            }
            else
            {
                deptInfo = "🏢 —";
            }
            lblStatusDept.Text   = deptInfo;
            lblStatusFolder.Text = "📁 —";
            lblStatusPath.Text   = "🖥️ —";
            UpdateStatusBarDateTime();
        }

        private void UpdateStatusBarDateTime()
        {
            lblStatusDateTime.Text = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        /// <summary>Updates the approval badge count on the ribbon.</summary>
        public void UpdateApprovalBadge(int count)
        {
            if (count > 0)
            {
                lblApprovalBadge.Text    = count.ToString();
                lblApprovalBadge.Visible = true;
            }
            else
            {
                lblApprovalBadge.Visible = false;
            }
        }

        /// <summary>Updates the folder info in the status bar.</summary>
        public void UpdateStatusBarFolder(string folderName, int fileCount, string physicalPath)
        {
            lblStatusFolder.Text = string.Format("📁 {0} — {1} รายการ", folderName, fileCount);
            lblStatusPath.Text   = string.Format("🖥️ {0}", physicalPath);
        }

        /// <summary>
        /// Loads and displays dashboard statistics.
        /// Shows total files, folders, pending approvals, and users.
        /// Renders as a styled card panel replacing the plain lblStatsContent label.
        /// </summary>
        private void LoadDashboardStats()
        {
            try
            {
                int totalFiles = 0;
                int totalFolders = 0;
                int pendingApprovals = 0;
                int totalUsers = 0;

                if (_fileBrowseController != null)
                {
                    try
                    {
                        // ใช้ cache จาก LoadRecentFiles ถ้ามี เพื่อหลีกเลี่ยง DB call ซ้ำ
                        var files = _cachedAllFiles
                            ?? _fileBrowseController.GetAllFilesGroupedByFolder(_currentUser).Where(f => !f.IsDeleted).ToList();
                        totalFiles = files.Count;
                    }
                    catch { }
                }

                if (_folderReadLookupController != null)
                {
                    try
                    {
                        totalFolders = _folderReadLookupController.GetAllFolders(null, _currentUser)
                            .Where(f => !f.IsDeleted).Count();
                    }
                    catch { }
                }

                if (_approvalInboxController != null)
                {
                    try
                    {
                        pendingApprovals = _approvalInboxController.GetPendingForApprover(_currentUser.UserId).Count;
                    }
                    catch { }
                }

                if (_userAdminController != null)
                {
                    try
                    {
                        totalUsers = _userAdminController.GetAllUsers().Count();
                    }
                    catch { }
                }

                // Update plain label (fallback)
                lblStatsContent.Text = string.Format(
                    "• ไฟล์ทั้งหมด: {0}\n• Folder ทั้งหมด: {1}\n• รายการรออนุมัติ: {2}\n• ผู้ใช้งาน: {3}",
                    totalFiles, totalFolders, pendingApprovals, totalUsers);

                // นับจำนวนไฟล์แยกตามประเภท
                var fileCounts = CountFilesByType(null);

                // Build / refresh the styled stats card
                BuildStatsCard(totalFiles, totalFolders, pendingApprovals, totalUsers, "สถิติระบบ", fileCounts);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load dashboard stats: " + ex.Message, ex);
                lblStatsContent.Text = "• ไฟล์ทั้งหมด: --\n• Folder ทั้งหมด: --\n• รายการรออนุมัติ: --\n• ผู้ใช้งาน: --";
            }
        }

        /// <summary>
        /// สร้าง / อัปเดต stats card panel ที่มีกรอบสวยงามแทน lblStats + lblStatsContent
        /// </summary>
        private void BuildStatsCard(int files, int folders, int pending, int users,
                                    string title = "สถิติระบบ",
                                    Dictionary<string, int> fileCounts = null)
        {
            // ลบ card เก่าออกก่อน (ถ้ามี)
            var existing = splitContainer1.Panel1.Controls["pnlStatsCard"] as Panel;
            if (existing != null)
                splitContainer1.Panel1.Controls.Remove(existing);

            lblStats.Visible        = false;
            lblStatsContent.Visible = false;

            // คำนวณความสูง: 4 แถวหลัก + separator + แถวประเภทไฟล์
            int typeRowCount = (fileCounts != null && fileCounts.Count > 0) ? fileCounts.Count + 1 : 0; // +1 = separator
            int cardW    = 220;
            int cardH    = 30 + 4 + (4 * 24) + (typeRowCount * 22) + 8; // header + padding + rows
            int rightMargin = 12;
            int topMargin   = 8;

            var card = new Panel
            {
                Name        = "pnlStatsCard",
                Size        = new Size(cardW, cardH),
                BackColor   = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            card.Location = new Point(
                splitContainer1.Panel1.ClientSize.Width - cardW - rightMargin,
                topMargin);

            splitContainer1.Panel1.Resize += (s, e) =>
            {
                var c = splitContainer1.Panel1.Controls["pnlStatsCard"] as Panel;
                if (c != null)
                    c.Left = splitContainer1.Panel1.ClientSize.Width - c.Width - rightMargin;
            };

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = Color.FromArgb(43, 87, 154) };
            header.Controls.Add(new Label
            {
                Text      = "📊  " + title,
                Dock      = DockStyle.Fill,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(6, 0, 0, 0)
            });

            // Body
            var body = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(8, 4, 8, 4) };

            // helper: เพิ่มแถว icon + label + value
            Action<string, string, string, Color, int> addRow = (icon, label, value, valColor, yy) =>
            {
                body.Controls.Add(new Label
                {
                    Text      = icon,
                    Location  = new Point(0, yy + 1),
                    Size      = new Size(20, 20),
                    Font      = new Font("Segoe UI Emoji", 8.5f),
                    ForeColor = Color.FromArgb(60, 60, 60)
                });
                body.Controls.Add(new Label
                {
                    Text      = label + ":",
                    Location  = new Point(22, yy + 2),
                    Size      = new Size(120, 18),
                    Font      = new Font("Segoe UI", 8.5f),
                    ForeColor = Color.FromArgb(80, 80, 80)
                });
                body.Controls.Add(new Label
                {
                    Text      = value,
                    Location  = new Point(148, yy + 2),
                    Size      = new Size(44, 18),
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = valColor,
                    TextAlign = ContentAlignment.TopRight
                });
            };

            var blue = Color.FromArgb(43, 87, 154);
            int y = 4;

            addRow("📄", "ไฟล์ทั้งหมด",     files.ToString(),   blue, y); y += 24;
            addRow("📁", "Folder ทั้งหมด",  folders.ToString(), blue, y); y += 24;
            addRow("✅", "รายการรออนุมัติ", pending.ToString(), pending > 0 ? Color.OrangeRed : blue, y); y += 24;
            if (users >= 0)
            {
                addRow("👤", "ผู้ใช้งาน", users.ToString(), blue, y);
                y += 24;
            }

            // ── แถวประเภทไฟล์ ────────────────────────────────────────────────
            if (fileCounts != null && fileCounts.Count > 0)
            {
                // separator line
                body.Controls.Add(new Panel
                {
                    Location  = new Point(0, y + 4),
                    Size      = new Size(cardW - 20, 1),
                    BackColor = Color.FromArgb(220, 220, 220)
                });
                y += 12;

                // sub-header
                body.Controls.Add(new Label
                {
                    Text      = "แยกตามประเภท",
                    Location  = new Point(0, y),
                    Size      = new Size(180, 16),
                    Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 100, 100)
                });
                y += 18;

                foreach (var kv in fileCounts.OrderByDescending(k => k.Value))
                {
                    string extIcon = FileIcon("." + kv.Key.ToLower());
                    addRow(extIcon, kv.Key.ToUpper(), kv.Value.ToString(), blue, y);
                    y += 22;
                }
            }

            card.Controls.Add(body);
            card.Controls.Add(header);

            splitContainer1.Panel1.Controls.Add(card);
            card.BringToFront();
        }

        // -----------------------------------------------------------------------
        // 16.3 — Notification inbox panel (Req 13.3, 13.4)
        // -----------------------------------------------------------------------

        private void BuildNotificationPanel()
        {
            _notificationPanel = new Panel
            {
                Dock      = DockStyle.Right,
                Width     = 280,
                BackColor = SystemColors.Control,
                Padding   = new Padding(4)
            };

            var headerLabel = new Label
            {
                Text      = "Notifications",
                Dock      = DockStyle.Top,
                Height    = 24,
                Font      = new Font(Font, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _notificationListBox = new ListBox
            {
                Dock          = DockStyle.Fill,
                SelectionMode = SelectionMode.One,
                HorizontalScrollbar = true
            };

            var buttonPanel = new Panel
            {
                Dock   = DockStyle.Bottom,
                Height = 32
            };

            _markReadButton = new Button
            {
                Text   = "Mark as Read",
                Dock   = DockStyle.Left,
                Width  = 120
            };
            _markReadButton.Click += OnMarkReadClick;

            _refreshButton = new Button
            {
                Text  = "Refresh",
                Dock  = DockStyle.Right,
                Width = 80
            };
            _refreshButton.Click += (s, e) => LoadNotifications();

            buttonPanel.Controls.Add(_markReadButton);
            buttonPanel.Controls.Add(_refreshButton);

            _notificationPanel.Controls.Add(_notificationListBox);
            _notificationPanel.Controls.Add(buttonPanel);
            _notificationPanel.Controls.Add(headerLabel);

            Controls.Add(_notificationPanel);
        }

        /// <summary>
        /// Builds the file detail panel on the right side of the form.
        /// Shows file metadata when a file is selected in lvRecentFiles.
        /// </summary>
        private void BuildFileDetailPanel()
        {
            _fileDetailPanel = new Panel
            {
                Dock        = DockStyle.Right,
                Width       = 240,
                BackColor   = Color.White,
                Padding     = new Padding(0),
                Visible     = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            // ── Header: icon (centered) + filename ──────────────────────────
            var headerPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 110,
                BackColor = Color.White,
                Padding   = new Padding(12, 12, 12, 8)
            };

            var fileIconLabel = new Label
            {
                Text      = "📄",
                Font      = new Font("Segoe UI Emoji", 28),
                Dock      = DockStyle.Top,
                Height    = 48,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White
            };

            _lblDetailFileName = new Label
            {
                Text      = string.Empty,
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock      = DockStyle.Fill,
                AutoSize  = false,
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(32, 32, 32)
            };

            headerPanel.Controls.Add(_lblDetailFileName);
            headerPanel.Controls.Add(fileIconLabel);

            // ── Red separator ────────────────────────────────────────────────
            var separator = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 4,
                BackColor = Color.FromArgb(204, 0, 0)
            };

            // ── Detail rows ──────────────────────────────────────────────────
            var detailsPanel = new Panel
            {
                Dock       = DockStyle.Fill,
                BackColor  = Color.White,
                Padding    = new Padding(14, 10, 14, 6),
                AutoScroll = true
            };

            int y   = 0;
            int w   = 200;
            int rowH = 46;

            // helper: add one label+value row
            Action<string, Label, int> addRow = (caption, valueLabel, yy) =>
            {
                var cap = new Label
                {
                    Text      = caption,
                    Font      = new Font("Segoe UI", 8),
                    Location  = new Point(0, yy),
                    Size      = new Size(w, 16),
                    ForeColor = Color.FromArgb(120, 120, 120),
                    AutoSize  = false
                };
                valueLabel.Font      = new Font("Segoe UI", 10, FontStyle.Bold);
                valueLabel.Location  = new Point(0, yy + 18);
                valueLabel.Size      = new Size(w, 22);
                valueLabel.AutoSize  = false;
                valueLabel.TextAlign = ContentAlignment.TopLeft;
                detailsPanel.Controls.Add(cap);
                detailsPanel.Controls.Add(valueLabel);
            };

            _lblDetailType            = new Label { ForeColor = Color.FromArgb(32, 32, 32) };
            _lblDetailConfidentiality = new Label { ForeColor = Color.DarkRed };
            _lblDetailSize            = new Label { ForeColor = Color.FromArgb(32, 32, 32) };
            _lblDetailVersion         = new Label { ForeColor = Color.FromArgb(32, 32, 32) };
            _lblDetailDate            = new Label { ForeColor = Color.FromArgb(32, 32, 32) };
            _lblDetailUploadedBy      = new Label { ForeColor = Color.FromArgb(32, 32, 32) };

            addRow("ชนิด",    _lblDetailType,            y); y += rowH;
            addRow("ระดับ",   _lblDetailConfidentiality, y); y += rowH;
            addRow("ขนาด",    _lblDetailSize,             y); y += rowH;
            addRow("เวอร์ชัน", _lblDetailVersion,          y); y += rowH;
            addRow("วันที่",   _lblDetailDate,             y); y += rowH;
            addRow("โดย",     _lblDetailUploadedBy,       y);

            // ── Buttons ──────────────────────────────────────────────────────
            var buttonsPanel = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 136,
                BackColor = Color.White,
                Padding   = new Padding(12, 8, 12, 12)
            };

            _btnDetailOpen = new Button
            {
                Text      = "👁  เปิดดู",
                Dock      = DockStyle.Top,
                Height    = 36,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(43, 87, 154),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            _btnDetailOpen.FlatAppearance.BorderSize = 0;
            _btnDetailOpen.Click += OnDetailOpenClick;

            _btnDetailDownload = new Button
            {
                Text      = "⬇  ดาวน์โหลด",
                Dock      = DockStyle.Top,
                Height    = 36,
                Font      = new Font("Segoe UI", 10),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(43, 87, 154),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            _btnDetailDownload.FlatAppearance.BorderColor = Color.FromArgb(43, 87, 154);
            _btnDetailDownload.FlatAppearance.BorderSize  = 1;
            _btnDetailDownload.Click += OnDetailDownloadClick;

            _btnDetailSecurity = new Button
            {
                Text      = "🔒  สิทธิ์",
                Dock      = DockStyle.Top,
                Height    = 36,
                Font      = new Font("Segoe UI", 10),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(43, 87, 154),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            _btnDetailSecurity.FlatAppearance.BorderColor = Color.FromArgb(43, 87, 154);
            _btnDetailSecurity.FlatAppearance.BorderSize  = 1;
            _btnDetailSecurity.Click += OnDetailSecurityClick;

            // Add in reverse order (Dock=Top stacks bottom-up)
            buttonsPanel.Controls.Add(_btnDetailSecurity);
            buttonsPanel.Controls.Add(_btnDetailDownload);
            buttonsPanel.Controls.Add(_btnDetailOpen);

            _fileDetailPanel.Controls.Add(buttonsPanel);
            _fileDetailPanel.Controls.Add(detailsPanel);
            _fileDetailPanel.Controls.Add(separator);
            _fileDetailPanel.Controls.Add(headerPanel);

            Controls.Add(_fileDetailPanel);
        }

        /// <summary>
        /// Loads unread notifications ordered by DeliveredAt descending and populates the list box.
        /// </summary>
        private void LoadNotifications()
        {
            try
            {
                var notifications = _notificationController.GetUnread(_currentUser.UserId);

                _unreadNotifications.Clear();
                _notificationListBox.Items.Clear();

                // GetUnread already returns ordered by DeliveredAt desc (per NotificationService contract)
                foreach (var n in notifications)
                {
                    _unreadNotifications.Add(n);
                    _notificationListBox.Items.Add(
                        string.Format("[{0:yyyy-MM-dd HH:mm}] {1}",
                            n.DeliveredAt.ToLocalTime(), n.MessageText));
                }
            }
            catch (Exception ex)
            {
                _logger.Error("MDI_Container: Failed to load notifications.", ex);
            }
        }

        private void OnMarkReadClick(object sender, EventArgs e)
        {
            int selectedIndex = _notificationListBox.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= _unreadNotifications.Count)
                return;

            var notification = _unreadNotifications[selectedIndex];

            try
            {
                _notificationController.MarkRead(notification.NotificationId, _currentUser);
                LoadNotifications();
            }
            catch (Exception ex)
            {
                _logger.Error(
                    string.Format("MDI_Container: Failed to mark notification {0} as read.",
                        notification.NotificationId), ex);
                MessageBox.Show(
                    "Failed to mark notification as read. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // -----------------------------------------------------------------------
        // Background escalation timer
        // -----------------------------------------------------------------------

        private void BuildEscalationTimer()
        {
            // Req 8.5: read interval from ConfigManager; default to 60 seconds
            int intervalSeconds = 60;
            if (_configManager != null)
            {
                try
                {
                    intervalSeconds = _configManager.Get<int>("EscalationTimerIntervalSeconds");
                }
                catch
                {
                    // Key not configured — use default
                }
            }

            int intervalMs = intervalSeconds * 1000;
            if (intervalMs <= 0)
                intervalMs = 60000;

            _escalationTimer = new Timer { Interval = intervalMs };
            _escalationTimer.Tick += OnEscalationTimerTick;
        }

        private void OnEscalationTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (_approvalInboxController != null)
                    _approvalInboxController.ProcessEscalations();
            }
            catch (Exception ex)
            {
                _logger.Error("MDI_Container: ProcessEscalations failed.", ex);
            }
        }

        // -----------------------------------------------------------------------
        // Hide Recycle Bin Menu Items
        // -----------------------------------------------------------------------

        /// <summary>
        /// Hides the top-menu shortcut for recycle bin (access is via Admin ribbon tab instead).
        /// The Admin ribbon button btnRibbonRecycleBin stays visible.
        /// </summary>
        private void HideRecycleBinMenuItems()
        {
            // Hide the old top-level menu shortcut only
            if (menuItemRecycleBin != null)
                menuItemRecycleBin.Visible = false;

            // btnRibbonRecycleBin is intentionally left visible — it lives in the Admin tab
        }

        private void OpenRecycleBin()
        {
            if (_recycleBinController == null)
            {
                MessageBox.Show("Recycle bin controller is not available.", "ถังขยะ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            OpenChildForm(new RecycleBinForm(_recycleBinController, _currentUser));
        }

        // ── Dashboard Handlers ────────────────────────────────────────────────
        private void WireDashboardHandlers()
        {
            btnDashUpload.Click   += (s, e) => OpenFileUpload();
            btnDashFolder.Click   += (s, e) => OpenFolderStructure();
            btnDashApproval.Click += (s, e) => OpenApprovalInbox();
            btnDashOrgUnit.Click  += (s, e) => OpenOrgStructure();

            // Toggle buttons: ตามองค์กร / ตามความลับ
            btnToggleByOrg.Click             += (s, e) => SetTreeViewMode(byOrg: true);
            btnToggleByConfidentiality.Click += (s, e) => SetTreeViewMode(byOrg: false);

            // Search box placeholder behavior
            txtSearchFolder.GotFocus  += (s, e) =>
            {
                if (txtSearchFolder.Text == "🔍 ค้นหา Folder...")
                {
                    _suppressTreeReload   = true;
                    txtSearchFolder.Text      = "";
                    txtSearchFolder.ForeColor = System.Drawing.Color.Black;
                    _suppressTreeReload   = false;
                }
            };
            txtSearchFolder.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearchFolder.Text))
                {
                    _suppressTreeReload   = true;
                    txtSearchFolder.Text      = "🔍 ค้นหา Folder...";
                    txtSearchFolder.ForeColor = System.Drawing.Color.Gray;
                    _suppressTreeReload   = false;
                }
            };
            txtSearchFolder.TextChanged += TxtSearchFolder_TextChanged;

            // Refresh button for recent files (if it exists in designer)
            if (Controls.ContainsKey("btnRefreshFiles"))
            {
                var btnRefresh = Controls["btnRefreshFiles"] as Button;
                if (btnRefresh != null)
                    btnRefresh.Click += (s, e) => ReloadRecentFilesData();
            }
        }

        private bool _treeViewByOrg      = true;
        private bool _treeResetPending   = false;
        private bool _suppressTreeReload = false;

        private void SetTreeViewMode(bool byOrg)
        {
            _treeViewByOrg = byOrg;

            // Update button styles - Office 2021 Navy Blue Light style
            var sidebarBg = System.Drawing.Color.FromArgb(238, 242, 248);
            btnToggleByOrg.BackColor             = byOrg ? System.Drawing.Color.FromArgb(43, 87, 154) : sidebarBg;
            btnToggleByOrg.ForeColor             = byOrg ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(43, 87, 154);
            btnToggleByOrg.Font                  = new System.Drawing.Font("Segoe UI", 9F, byOrg ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular);

            btnToggleByConfidentiality.BackColor = byOrg ? sidebarBg : System.Drawing.Color.FromArgb(43, 87, 154);
            btnToggleByConfidentiality.ForeColor = byOrg ? System.Drawing.Color.FromArgb(43, 87, 154) : System.Drawing.Color.White;
            btnToggleByConfidentiality.Font      = new System.Drawing.Font("Segoe UI", 9F, byOrg ? System.Drawing.FontStyle.Regular : System.Drawing.FontStyle.Bold);

            // Update title label
            lblTreeTitle.Text = byOrg ? "📁 ตามองค์กร" : "🔒 ตามความลับ";

            // Reload tree — force rebuild เมื่อ user กด toggle
            if (byOrg)
                LoadDashboardFolderTree(force: true);
            else
                LoadDashboardConfidentialityTree(force: true);
        }

        private void LoadDashboardConfidentialityTree(bool force = false)
        {
            // Skip rebuild ถ้า tree มี nodes อยู่แล้วและไม่ได้ force reload
            if (!force && treeViewFolders.Nodes.Count > 0 && !_treeViewByOrg)
                return;

            treeViewFolders.AfterSelect -= TreeViewFolders_AfterSelect;
            treeViewFolders.Nodes.Clear();

            if (_folderService == null)
            {
                treeViewFolders.AfterSelect += TreeViewFolders_AfterSelect;
                return;
            }

            try
            {
                var levels = new[]
                {
                    new { Level = 1, Label = "1HC — High Confidential" },
                    new { Level = 2, Label = "2SC — Semi Confidential" },
                    new { Level = 3, Label = "3NC — Normal Confidential" },
                    new { Level = 4, Label = "4PC — Public / เปิดเผย" },
                };

                var allFiles = _cachedAllFiles ?? _fileBrowseController.GetAllFilesGroupedByFolder(_currentUser)
                    .Where(f => !f.IsDeleted).ToList();

                var folderLookup = new Dictionary<int, Folder>();
                try
                {
                    foreach (var f in _folderReadLookupController.GetAllFolders(null, _currentUser))
                        folderLookup[f.FolderID] = f;
                }
                catch { }

                var fileCountByLevel = new Dictionary<int, int>();
                foreach (var file in allFiles)
                {
                    Folder folder;
                    if (!folderLookup.TryGetValue(file.FolderID, out folder)) continue;
                    int lvl = folder.ConfidentialityLevel;
                    if (!fileCountByLevel.ContainsKey(lvl)) fileCountByLevel[lvl] = 0;
                    fileCountByLevel[lvl]++;
                }

                var orgUnitsByLevel = new Dictionary<int, HashSet<int>>();
                foreach (var file in allFiles)
                {
                    Folder folder;
                    if (!folderLookup.TryGetValue(file.FolderID, out folder)) continue;
                    int lvl = folder.ConfidentialityLevel;
                    if (!orgUnitsByLevel.ContainsKey(lvl)) orgUnitsByLevel[lvl] = new HashSet<int>();
                    orgUnitsByLevel[lvl].Add(folder.OrgUnitID);
                }

                if (_cachedOrgUnits == null && _orgStructureService != null)
                    _cachedOrgUnits = _orgStructureService.GetHierarchy().ToList();
                var orgLookup = new Dictionary<int, OrgUnit>();
                if (_cachedOrgUnits != null)
                    foreach (var ou in _cachedOrgUnits)
                        orgLookup[ou.OrgUnitID] = ou;

                treeViewFolders.BeginUpdate();
                try
                {
                    foreach (var lvl in levels)
                    {
                        int count;
                        fileCountByLevel.TryGetValue(lvl.Level, out count);
                        string label = count > 0
                            ? string.Format("{0}   {1} ไฟล์", lvl.Label, count)
                            : lvl.Label;

                        var levelNode = new TreeNode(label)
                        {
                            ForeColor = ConfidentialityColor(lvl.Level),
                            Tag       = lvl.Level
                        };

                        HashSet<int> orgIds;
                        if (orgUnitsByLevel.TryGetValue(lvl.Level, out orgIds))
                        {
                            foreach (int orgId in orgIds)
                            {
                                OrgUnit ou;
                                if (!orgLookup.TryGetValue(orgId, out ou)) continue;
                                var orgNode = new TreeNode(string.Format("📁 {0}", ou.Name))
                                {
                                    Tag       = ou,
                                    ForeColor = System.Drawing.Color.FromArgb(0, 102, 51)
                                };
                                levelNode.Nodes.Add(orgNode);
                            }
                        }

                        treeViewFolders.Nodes.Add(levelNode);
                    }
                }
                finally
                {
                    treeViewFolders.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load confidentiality tree: " + ex.Message, ex);
            }
            finally
            {
                treeViewFolders.AfterSelect += TreeViewFolders_AfterSelect;
            }
        }

        private void TxtSearchFolder_TextChanged(object sender, EventArgs e)
        {
            if (_suppressTreeReload) return;

            string keyword = txtSearchFolder.Text.Trim();
            if (keyword == "🔍 ค้นหา Folder..." || string.IsNullOrEmpty(keyword))
            {
                // restore หลัง search — force rebuild เพื่อให้ได้ tree ที่ถูกต้อง
                if (_treeViewByOrg) LoadDashboardFolderTree(force: true);
                else LoadDashboardConfidentialityTree(force: true);
                return;
            }

            FilterTreeNodes(treeViewFolders.Nodes, keyword.ToLower());
        }

        private bool FilterTreeNodes(TreeNodeCollection nodes, string keyword)
        {
            bool anyVisible = false;
            foreach (TreeNode node in nodes)
            {
                bool childVisible = FilterTreeNodes(node.Nodes, keyword);
                bool selfMatch    = node.Text.ToLower().Contains(keyword);
                node.ForeColor    = selfMatch ? System.Drawing.Color.FromArgb(0, 102, 204) : System.Drawing.Color.FromArgb(32, 32, 32);
                if (selfMatch || childVisible)
                {
                    node.Expand();
                    anyVisible = true;
                }
            }
            return anyVisible;
        }

        private void LoadDashboardFolderTree(bool force = false)
        {
            // Skip rebuild ถ้า tree มี nodes อยู่แล้วและไม่ได้ force reload
            if (!force && treeViewFolders.Nodes.Count > 0 && _treeViewByOrg)
                return;

            treeViewFolders.AfterSelect -= TreeViewFolders_AfterSelect;

            try
            {
                treeViewFolders.Nodes.Clear();

                if (_orgStructureService == null)
                    return;

                if (_cachedOrgUnits == null)
                    _cachedOrgUnits = _orgStructureService.GetHierarchy().ToList();

                var allUnits = _cachedOrgUnits;
                if (allUnits.Count == 0)
                    return;

                var subCommittees = allUnits
                    .Where(u => u.Tier == OrgUnitTier.SubCommittee && u.IsActive)
                    .OrderBy(u => u.Name);

                treeViewFolders.BeginUpdate();
                try
                {
                    foreach (var sc in subCommittees)
                    {
                        var scNode = new TreeNode(string.Format("🏢 {0} [SubCommittee]", sc.Name))
                        {
                            Tag = sc,
                            ImageIndex = 0,
                            SelectedImageIndex = 0
                        };

                        var divisions = allUnits
                            .Where(u => u.Tier == OrgUnitTier.Division && u.ParentId == sc.OrgUnitId && u.IsActive)
                            .OrderBy(u => u.Name);

                        foreach (var div in divisions)
                        {
                            var divNode = new TreeNode(string.Format("📂 {0} [Division]", div.Name))
                            {
                                Tag = div,
                                ImageIndex = 1,
                                SelectedImageIndex = 1
                            };

                            var departments = allUnits
                                .Where(u => u.Tier == OrgUnitTier.Department && u.ParentId == div.OrgUnitId && u.IsActive)
                                .OrderBy(u => u.Name);

                            foreach (var dept in departments)
                            {
                                var deptNode = new TreeNode(string.Format("📋 {0} [Department]", dept.Name))
                                {
                                    Tag = dept,
                                    ImageIndex = 2,
                                    SelectedImageIndex = 2
                                };
                                divNode.Nodes.Add(deptNode);
                            }

                            scNode.Nodes.Add(divNode);
                        }

                        treeViewFolders.Nodes.Add(scNode);
                    }
                }
                finally
                {
                    treeViewFolders.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load dashboard org structure tree: " + ex.Message, ex);
            }
            finally
            {
                treeViewFolders.AfterSelect += TreeViewFolders_AfterSelect;
            }
        }

        /// <summary>
        /// Setup columns for lvRecentFiles once on load.
        /// </summary>
        private void SetupRecentFilesColumns()
        {
            if (lvRecentFiles.Columns.Count > 0) return;
            lvRecentFiles.Columns.Add("ชื่อ",      340);
            lvRecentFiles.Columns.Add("ประเภท",     70);
            lvRecentFiles.Columns.Add("ระดับ",      60);
            lvRecentFiles.Columns.Add("ขนาด",       80);
            lvRecentFiles.Columns.Add("วันที่",     120);
            lvRecentFiles.Columns.Add("โดย",        80);
            lvRecentFiles.Columns.Add("Ver",        36);
            lvRecentFiles.Columns.Add("หน่วยงาน",  120);

            AutoResizeColumnsProportional();
        }

        /// <summary>
        /// Resizes columns proportionally to fit the available ListView width.
        /// </summary>
        private void AutoResizeColumnsProportional()
        {
            if (lvRecentFiles.Columns.Count == 0) return;

            // Get available width (account for scrollbar)
            int availableWidth = lvRecentFiles.Width - SystemInformation.VerticalScrollBarWidth - 4;
            if (availableWidth <= 0) return;

            // Define proportions for each column (total = 906)
            int[] proportions = { 340, 70, 60, 80, 120, 80, 36, 120 };
            int totalProportion = 0;
            foreach (int p in proportions) totalProportion += p;

            // Resize each column proportionally
            for (int i = 0; i < lvRecentFiles.Columns.Count; i++)
            {
                int newWidth = (availableWidth * proportions[i]) / totalProportion;
                lvRecentFiles.Columns[i].Width = newWidth;
            }
        }

        // ── treeViewFolders selection → load files ────────────────────────────

        private void TreeViewFolders_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;

            // Mode: ตามองค์กร — tag เป็น OrgUnit
            var orgUnit = e.Node.Tag as OrgUnit;
            if (orgUnit != null)
            {
                lblRecentFiles.Text = string.Format("📄 ไฟล์ใน {0}", orgUnit.Name);
                LoadFilesForOrgUnit(orgUnit.OrgUnitID);
                UpdateStatsCardForOrgUnit(orgUnit);
                // คง expand state ไว้ — ไม่ให้ tree collapse
                e.Node.EnsureVisible();
                return;
            }

            // Mode: ตามความลับ — tag เป็น int (confidentiality level)
            if (e.Node.Tag is int)
            {
                int level = (int)e.Node.Tag;
                string[] labels = { "", "1HC — Highly Confidential", "2SC — Secret", "3NC — Non-Confidential", "4PC — Public/Common" };
                string labelText = (level >= 1 && level <= 4) ? labels[level] : "ระดับ " + level;
                lblRecentFiles.Text = string.Format("📄 ไฟล์ระดับ {0}", labelText);
                LoadFilesByConfidentialityLevel(level);
                UpdateStatsCardForConfidentialityLevel(level, labelText);
                // คง expand state ไว้
                e.Node.EnsureVisible();
            }
        }

        /// <summary>อัปเดต stats card ให้แสดงยอดตาม OrgUnit ที่เลือก (รวม child OrgUnit)</summary>
        private void UpdateStatsCardForOrgUnit(OrgUnit orgUnit)
        {
            int files   = 0;
            int folders = 0;
            int pending = 0;

            // รวบรวม OrgUnitID ทั้งหมด (ตัวเองและ child ทั้งหมด)
            var orgUnitIds = new List<int> { orgUnit.OrgUnitID };
            if (_orgStructureService != null)
            {
                try
                {
                    var all = _orgStructureService.GetHierarchy().ToList();
                    // เพิ่ม child ทุกระดับ
                    var queue = new Queue<int>();
                    queue.Enqueue(orgUnit.OrgUnitID);
                    while (queue.Count > 0)
                    {
                        int parentId = queue.Dequeue();
                        foreach (var child in all.Where(u => u.ParentId == parentId))
                        {
                            if (!orgUnitIds.Contains(child.OrgUnitID))
                            {
                                orgUnitIds.Add(child.OrgUnitID);
                                queue.Enqueue(child.OrgUnitID);
                            }
                        }
                    }
                }
                catch { }
            }

            // นับไฟล์และ folder จากทุก OrgUnit ที่รวบรวมได้
            try
            {
                if (_fileBrowseController != null)
                {
                    // ใช้ OrgUnit แรก (ตัวเอง) เป็น root สำหรับ GetFilesByOrgUnit
                    // SP ใช้ recursive CTE ครอบคลุม child อยู่แล้ว
                    files = _fileBrowseController.GetFilesByOrgUnit(orgUnit.OrgUnitID, _currentUser)
                        .Where(f => !f.IsDeleted).Count();
                }
            }
            catch { }

            try
            {
                if (_folderReadLookupController != null)
                {
                    // นับ folder จากทุก OrgUnit ใน orgUnitIds
                    foreach (var ouId in orgUnitIds)
                    {
                        try
                        {
                            folders += _folderReadLookupController.GetAllFolders(ouId, _currentUser)
                                .Where(f => !f.IsDeleted).Count();
                        }
                        catch { }
                    }
                }
            }
            catch { }

            try
            {
                if (_approvalInboxController != null)
                    pending = _approvalInboxController.GetPendingForApprover(_currentUser.UserId).Count;
            }
            catch { }

            var fileCounts = CountFilesByType(orgUnit.OrgUnitID);
            BuildStatsCard(files, folders, pending, -1,
                string.Format("{0}", orgUnit.Name), fileCounts);
        }

        /// <summary>อัปเดต stats card ให้แสดงยอดตาม Confidentiality Level ที่เลือก</summary>
        private void UpdateStatsCardForConfidentialityLevel(int level, string labelText)
        {
            int files   = 0;
            int folders = 0;

            try
            {
                if (_fileBrowseController != null && _folderReadLookupController != null)
                {
                    var allFiles = _fileBrowseController.GetAllFilesGroupedByFolder(_currentUser)
                        .Where(f => !f.IsDeleted).ToList();

                    // นับ folder ที่มี level ตรงกัน
                    var folderIds = allFiles.Select(f => f.FolderID).Distinct().ToList();
                    foreach (var fid in folderIds)
                    {
                        try
                        {
                            var folder = _folderReadLookupController.GetFolder(fid, _currentUser);
                            if (folder != null && folder.ConfidentialityLevel == level)
                            {
                                folders++;
                                files += allFiles.Count(f => f.FolderID == fid);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            BuildStatsCard(files, folders, 0, -1, labelText,
                CountFilesByType(null, level));
        }

        /// <summary>
        /// โหลดไฟล์ทั้งหมดที่มี confidentiality level ตรงกับที่เลือก
        /// โดย filter จาก folder's ConfidentialityLevel
        /// </summary>
        private void LoadFilesByConfidentialityLevel(int level)
        {
            lvRecentFiles.Items.Clear();
            _fileDetailPanel.Visible = false;

            if (_fileBrowseController == null || _folderReadLookupController == null) return;

            try
            {
                var orgNames = BuildOrgUnitNameLookup();

                var allFiles = _fileBrowseController.GetAllFilesGroupedByFolder(_currentUser)
                    .Where(f => !f.IsDeleted)
                    .ToList();

                if (allFiles.Count == 0) return;

                var byFolder = allFiles
                    .GroupBy(f => f.FolderID)
                    .OrderBy(g => g.First().FolderName ?? string.Format("Folder {0}", g.Key));

                foreach (var group in byFolder)
                {
                    var firstFile = group.First();

                    Folder headerFolder;
                    try
                    {
                        headerFolder = _folderReadLookupController.GetFolder(firstFile.FolderID, _currentUser);
                        if (headerFolder == null) continue;
                        headerFolder.FileCount   = group.Count();
                        headerFolder.OrgUnitName = orgNames.TryGetValue(headerFolder.OrgUnitID, out var ouN) ? ouN : "—";
                    }
                    catch { continue; }

                    if (headerFolder.ConfidentialityLevel != level) continue;

                    AddFolderHeaderRow(headerFolder);

                    if (_expandedFolderIds.Contains(headerFolder.FolderID))
                    {
                        foreach (var file in group.OrderBy(f => f.OriginalFileName))
                        {
                            file.OrgUnitName = headerFolder.OrgUnitName;
                            AddFileRow(file, headerFolder.ConfidentialityLevel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("LoadFilesByConfidentialityLevel failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// ดึงไฟล์ทั้งหมดใน OrgUnit ที่ระบุ (รวม child OrgUnit ทั้งหมด) ผ่าน SP โดยตรง
        /// SP ใช้ recursive CTE: OrgUnit → Folder → FileDocument
        /// </summary>
        private void LoadFilesForOrgUnit(int orgUnitId)
        {
            lvRecentFiles.Items.Clear();
            _fileDetailPanel.Visible = false;

            if (_fileBrowseController == null || _folderService == null) return;

            try
            {
                var orgNames = BuildOrgUnitNameLookup();

                var files = _fileBrowseController.GetFilesByOrgUnit(orgUnitId, _currentUser)
                    .Where(f => !f.IsDeleted)
                    .ToList();

                if (files.Count == 0) return;

                var byFolder = files
                    .GroupBy(f => f.FolderID)
                    .OrderBy(g => g.First().FolderName ?? string.Format("Folder {0}", g.Key));

                foreach (var group in byFolder)
                {
                    var first = group.First();

                    Folder headerFolder;
                    try
                    {
                        headerFolder = _folderReadLookupController.GetFolder(first.FolderID, _currentUser);
                        if (headerFolder == null)
                        {
                            headerFolder = new Folder
                            {
                                FolderID             = first.FolderID,
                                FolderName           = first.FolderName ?? string.Format("Folder {0}", first.FolderID),
                                ConfidentialityLevel = 0,
                                FileCount            = group.Count()
                            };
                        }
                        else
                        {
                            headerFolder.FileCount = group.Count();
                        }
                    }
                    catch
                    {
                        headerFolder = new Folder
                        {
                            FolderID             = first.FolderID,
                            FolderName           = first.FolderName ?? string.Format("Folder {0}", first.FolderID),
                            ConfidentialityLevel = 0,
                            FileCount            = group.Count()
                        };
                    }

                    headerFolder.OrgUnitName = orgNames.TryGetValue(headerFolder.OrgUnitID, out var ouN2) ? ouN2 : "—";

                    AddFolderHeaderRow(headerFolder);

                    if (_expandedFolderIds.Contains(headerFolder.FolderID))
                    {
                        foreach (var file in group.OrderBy(f => f.OriginalFileName))
                        {
                            file.OrgUnitName = headerFolder.OrgUnitName;
                            AddFileRow(file, headerFolder.ConfidentialityLevel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("LoadFilesForOrgUnit failed: " + ex.Message, ex);
            }
        }

        private void AddFolderHeaderRow(Folder folder)
        {
            string badge    = ConfidentialityBadge(folder.ConfidentialityLevel);
            bool   expanded = _expandedFolderIds.Contains(folder.FolderID);
            string arrow    = expanded ? "▼" : "▶";
            string label    = string.Format("{0} 📁  {1}  {2}", arrow, folder.FolderName, badge);

            var item = new ListViewItem(label);
            item.SubItems.Add("Folder");
            item.SubItems.Add(badge);
            item.SubItems.Add(folder.FileCount > 0
                ? string.Format("{0} ไฟล์", folder.FileCount) : "—");
            item.SubItems.Add("—");
            item.SubItems.Add("—");
            item.SubItems.Add("—");
            item.SubItems.Add(folder.OrgUnitName ?? "—");
            item.Tag       = folder;
            item.BackColor = Color.FromArgb(230, 238, 250);
            item.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
            item.Font      = new Font(lvRecentFiles.Font, FontStyle.Bold);

            item.UseItemStyleForSubItems = false;
            item.SubItems[2].ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);

            lvRecentFiles.Items.Add(item);
        }

        private void AddFileRow(FileDocument file, int folderConfidentialityLevel = 0)
        {
            file.ConfidentialityLevel = folderConfidentialityLevel;

            string ext   = file.FileExtension.TrimStart('.').ToUpper();
            string icon  = FileIcon(file.FileExtension);
            string badge = ConfidentialityBadge(folderConfidentialityLevel);

            var item = new ListViewItem(string.Format("      {0}  {1}", icon, file.OriginalFileName));
            item.SubItems.Add(ext);
            item.SubItems.Add(badge);
            item.SubItems.Add(FormatFileSize(file.FileSizeBytes));
            item.SubItems.Add(file.UploadedDate.ToString("dd/MM/yy HH:mm"));
            item.SubItems.Add(file.UploadedBy.ToString());
            item.SubItems.Add(file.Version.ToString());
            item.SubItems.Add(file.OrgUnitName ?? "—");
            item.Tag = file;

            if (file.VirusScanStatus == 2)
                item.ForeColor = Color.DarkRed;
            else if (file.VirusScanStatus == 0)
                item.ForeColor = Color.DarkOrange;
            else if (folderConfidentialityLevel > 0)
            {
                item.UseItemStyleForSubItems = false;
                item.SubItems[2].ForeColor = ConfidentialityColor(folderConfidentialityLevel);
            }

            lvRecentFiles.Items.Add(item);
        }

        /// <summary>
        /// นับจำนวนไฟล์แยกตามนามสกุล (extension)
        /// orgUnitId = null → นับทั้งหมด, confidentialityLevel = null → ไม่ filter ระดับ
        /// </summary>
        private Dictionary<string, int> CountFilesByType(int? orgUnitId, int? confidentialityLevel = null)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (_fileBrowseController == null) return dict;

            try
            {
                IEnumerable<FileDocument> files;
                if (orgUnitId.HasValue)
                    files = _fileBrowseController.GetFilesByOrgUnit(orgUnitId.Value, _currentUser);
                else if (_cachedAllFiles != null)
                    // ใช้ cache ถ้ามี เพื่อหลีกเลี่ยง DB call ซ้ำจาก LoadDashboardStats
                    files = _cachedAllFiles;
                else
                    files = _fileBrowseController.GetAllFilesGroupedByFolder(_currentUser);

                foreach (var f in files.Where(f => !f.IsDeleted))
                {
                    // filter ตาม confidentiality level ถ้าระบุ
                    if (confidentialityLevel.HasValue && f.ConfidentialityLevel != confidentialityLevel.Value)
                        continue;

                    string ext = (f.FileExtension ?? "").TrimStart('.').ToUpper();
                    if (string.IsNullOrEmpty(ext)) ext = "อื่นๆ";

                    if (dict.ContainsKey(ext))
                        dict[ext]++;
                    else
                        dict[ext] = 1;
                }
            }
            catch { }

            return dict;
        }

        /// <summary>สร้าง dictionary OrgUnitID → Name สำหรับ lookup ชื่อหน่วยงาน</summary>
        private Dictionary<int, string> BuildOrgUnitNameLookup()
        {
            var dict = new Dictionary<int, string>();
            if (_orgStructureService == null) return dict;
            try
            {
                // ใช้ cache ที่โหลดไว้แล้วใน LoadDashboardFolderTree เพื่อหลีกเลี่ยง DB call ซ้ำ
                var units = _cachedOrgUnits ?? _orgStructureService.GetHierarchy().ToList();
                foreach (var ou in units)
                    if (!dict.ContainsKey(ou.OrgUnitID))
                        dict[ou.OrgUnitID] = ou.Name;
            }
            catch { }
            return dict;
        }

        private static string ConfidentialityBadge(int level)
        {
            switch (level)
            {
                case 1: return "1HC";
                case 2: return "2SC";
                case 3: return "3NC";
                case 4: return "4PC";
                default: return "—";
            }
        }

        private static Color ConfidentialityColor(int level)
        {
            switch (level)
            {
                case 1: return Color.FromArgb(192, 0, 0);        // 1HC — แดงเข้ม
                case 2: return Color.FromArgb(210, 105, 30);     // 2SC — ส้ม
                case 3: return Color.FromArgb(0, 84, 166);       // 3NC — น้ำเงิน
                case 4: return Color.FromArgb(0, 128, 0);        // 4PC — เขียว
                default: return Color.FromArgb(32, 32, 32);
            }
        }

        private static string FileIcon(string ext)
        {
            if (ext == null) return "📄";
            switch (ext.ToLower().TrimStart('.'))
            {
                case "pdf":  return "📕";
                case "doc": case "docx": return "📘";
                case "xls": case "xlsx": return "📗";
                case "ppt": case "pptx": return "📙";
                case "jpg": case "jpeg": case "png": case "gif": return "🖼️";
                case "zip": return "🗜️";
                default:    return "📄";
            }
        }

        /// <summary>
        /// Loads recently uploaded files (top 10 folders) into lvRecentFiles on initial load.
        /// </summary>
        private void LoadRecentFiles()
        {
            lvRecentFiles.Items.Clear();
            _fileDetailPanel.Visible = false;

            if (_fileBrowseController == null || _folderReadLookupController == null) return;

            try
            {
                var orgNames = BuildOrgUnitNameLookup();

                // Cache allFiles เพื่อให้ LoadDashboardStats/CountFilesByType ใช้ต่อได้โดยไม่ต้องเรียก DB ซ้ำ
                _cachedAllFiles = _fileBrowseController.GetAllFilesGroupedByFolder(_currentUser)
                    .Where(f => !f.IsDeleted)
                    .ToList();

                var byFolder = _cachedAllFiles
                    .GroupBy(f => f.FolderID)
                    .OrderByDescending(g => g.Max(f => f.UploadedDate))
                    .Take(10)
                    .ToList();

                // Build folder lookup ด้วย DB call เดียว แทนที่จะเรียก GetFolder() ทีละ folder (N+1 fix)
                var folderLookup = new Dictionary<int, Folder>();
                try
                {
                    foreach (var f in _folderReadLookupController.GetAllFolders(null, _currentUser))
                        folderLookup[f.FolderID] = f;
                }
                catch { }

                lvRecentFiles.BeginUpdate();
                try
                {
                    foreach (var group in byFolder)
                    {
                        var firstFile = group.First();

                        Folder headerFolder;
                        if (!folderLookup.TryGetValue(firstFile.FolderID, out headerFolder) || headerFolder == null)
                        {
                            headerFolder = new Folder
                            {
                                FolderID             = firstFile.FolderID,
                                FolderName           = firstFile.FolderName ?? string.Format("Folder {0}", firstFile.FolderID),
                                ConfidentialityLevel = 0,
                                FileCount            = group.Count()
                            };
                        }
                        else
                        {
                            headerFolder.FileCount = group.Count();
                        }

                        headerFolder.OrgUnitName = orgNames.TryGetValue(headerFolder.OrgUnitID, out var ouN3) ? ouN3 : "—";

                        AddFolderHeaderRow(headerFolder);

                        if (_expandedFolderIds.Contains(headerFolder.FolderID))
                        {
                            foreach (var file in group.OrderBy(f => f.OriginalFileName))
                            {
                                file.OrgUnitName = headerFolder.OrgUnitName;
                                AddFileRow(file, headerFolder.ConfidentialityLevel);
                            }
                        }
                    }
                }
                finally
                {
                    lvRecentFiles.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load recent files: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Formats file size in human-readable format (B, KB, MB, GB).
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }

        // -----------------------------------------------------------------------
        // File detail panel handlers
        // -----------------------------------------------------------------------

        private void LvRecentFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvRecentFiles.SelectedItems.Count != 1)
            {
                _fileDetailPanel.Visible = false;
                return;
            }

            var selectedItem = lvRecentFiles.SelectedItems[0];
            var file = selectedItem.Tag as FileDocument;
            var folder = selectedItem.Tag as Folder;

            if (file != null)
            {
                ShowFileDetail(file);
                _fileDetailPanel.Visible = true;
            }
            else if (folder != null)
            {
                _fileDetailPanel.Visible = false;
            }
        }

        private void ShowFileDetail(FileDocument file)
        {
            _lblDetailFileName.Text = file.OriginalFileName;

            string ext = file.FileExtension.TrimStart('.').ToUpper();
            _lblDetailType.Text = ext;

            int level = file.ConfidentialityLevel;
            _lblDetailConfidentiality.Text     = GetConfidentialityShort(level) + "  " + GetConfidentialityDesc(level);
            _lblDetailConfidentiality.ForeColor = GetConfidentialityColor(level);

            _lblDetailSize.Text         = FormatFileSize(file.FileSizeBytes);
            _lblDetailVersion.Text      = string.Format("v{0}", file.Version);
            _lblDetailDate.Text         = file.UploadedDate.ToString("dd/MM/yy HH:mm");
            _lblDetailUploadedBy.Text   = file.UploadedBy.ToString();

            _fileDetailPanel.Tag = file;
        }

        private static string GetConfidentialityShort(int level)
        {
            switch (level)
            {
                case 1: return "1HC";
                case 2: return "2SC";
                case 3: return "3NC";
                case 4: return "4PC";
                default: return "—";
            }
        }

        private static string GetConfidentialityDesc(int level)
        {
            switch (level)
            {
                case 1: return "High Confidential";
                case 2: return "Secret";
                case 3: return "Non-Confidential";
                case 4: return "Public/Common";
                default: return string.Empty;
            }
        }

        private static Color GetConfidentialityColor(int level)
        {
            switch (level)
            {
                case 1: return Color.FromArgb(192, 0, 0);
                case 2: return Color.FromArgb(210, 105, 30);
                case 3: return Color.FromArgb(0, 84, 166);
                case 4: return Color.FromArgb(0, 128, 0);
                default: return Color.FromArgb(32, 32, 32);
            }
        }

        private async void OnDetailOpenClick(object sender, EventArgs e)
        {
            // เฉพาะ FileDocument เท่านั้น — Folder item จะถูกกรองออกที่นี่
            var file = _fileDetailPanel.Tag as FileDocument;
            if (file == null) return;

            try
            {
                string pathToOpen = ResolveFilePhysicalPath(file);

                if (string.IsNullOrEmpty(pathToOpen) || !System.IO.File.Exists(pathToOpen))
                {
                    // Fallback: ดึง stream จาก service แล้วบันทึกไป temp file
                    pathToOpen = OpenViaDownloadStream(file);
                }

                if (string.IsNullOrEmpty(pathToOpen))
                {
                    MessageBox.Show(
                        string.Format("ไม่พบไฟล์: {0}", file.OriginalFileName),
                        "เปิดไฟล์ไม่ได้", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string ext = (file.FileExtension ?? System.IO.Path.GetExtension(pathToOpen) ?? string.Empty)
                             .ToLowerInvariant().TrimStart('.');

                if (ext == "pdf")
                {
                    PdfViewOptions options = PdfViewOptions.ViewOnly;
                    if (_permissionCheckController != null)
                    {
                        var eff = _permissionCheckController.GetEffectivePermission(_currentUser.UserID, file.FolderID);
                        options = PdfViewOptions.FromEffective(eff, _currentUser);
                    }
                    await PdfSecurityHelper.OpenSecurePdfAsync(pathToOpen, this, options);
                }
                else
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName        = pathToOpen,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to open file: " + ex.Message, ex);
                MessageBox.Show(
                    string.Format("ไม่สามารถเปิดไฟล์ได้\n\n{0}", ex.Message),
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// คืน path จาก PhysicalPath ของไฟล์ (ถ้าเป็น absolute path ที่มีอยู่จริง)
        /// </summary>
        private static string ResolveFilePhysicalPath(FileDocument file)
        {
            if (string.IsNullOrEmpty(file.PhysicalPath)) return null;

            // PhysicalPath อาจเป็น absolute path จาก NetworkDriveHelper.BuildPath
            if (System.IO.Path.IsPathRooted(file.PhysicalPath) &&
                System.IO.File.Exists(file.PhysicalPath))
                return file.PhysicalPath;

            return null;
        }

        /// <summary>
        /// ดึง stream จาก _fileService แล้วเขียนไป temp file เพื่อเปิดด้วย default application
        /// คืน path ของ temp file (หรือ null ถ้าล้มเหลว)
        /// </summary>
        private string OpenViaDownloadStream(FileDocument file)
        {
            if (_fileBrowseController == null) return null;

            var result = _fileBrowseController.DownloadFile(file.FileID, _currentUser);
            if (result == null || !result.Success || result.FileStream == null) return null;

            // ชื่อ temp file ใช้ original name เพื่อให้ application เปิดถูก type
            string tempDir  = System.IO.Path.GetTempPath();
            string tempPath = System.IO.Path.Combine(tempDir, result.FileName ?? file.OriginalFileName);

            using (var fs = new System.IO.FileStream(tempPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                result.FileStream.CopyTo(fs);

            result.FileStream.Dispose();
            return tempPath;
        }

        private void OnDetailDownloadClick(object sender, EventArgs e)
        {
            var file = _fileDetailPanel.Tag as FileDocument;
            if (file == null) return;

            if (_fileBrowseController == null)
            {
                MessageBox.Show("File service is not available.", "ดาวน์โหลด",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title    = "บันทึกไฟล์";
                sfd.FileName = file.OriginalFileName;
                string ext   = (file.FileExtension ?? "").TrimStart('.').ToLower();
                sfd.Filter   = string.Format("{0} files (*.{1})|*.{1}|All files (*.*)|*.*",
                    ext.ToUpper(), ext);
                sfd.DefaultExt = ext;

                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    var result = _fileBrowseController.DownloadFile(file.FileID, _currentUser);
                    if (result == null || !result.Success)
                    {
                        MessageBox.Show(
                            string.Format("ดาวน์โหลดไม่สำเร็จ: {0}",
                                result != null ? result.ErrorMessage : "ไม่ทราบสาเหตุ"),
                            "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    using (result.FileStream)
                    using (var fs = new System.IO.FileStream(
                        sfd.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                    {
                        result.FileStream.CopyTo(fs);
                    }

                    MessageBox.Show(
                        string.Format("ดาวน์โหลด '{0}' เรียบร้อยแล้ว", file.OriginalFileName),
                        "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.Error("Download unauthorized: " + ex.Message, ex);
                    MessageBox.Show("ไม่มีสิทธิ์ดาวน์โหลดไฟล์นี้: " + ex.Message,
                        "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to download file: " + ex.Message, ex);
                    MessageBox.Show("ดาวน์โหลดไม่สำเร็จ: " + ex.Message,
                        "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnDetailSecurityClick(object sender, EventArgs e)
        {
            var file = _fileDetailPanel.Tag as FileDocument;
            if (file == null) return;

            try
            {
                OpenFilePermissionDialog(file);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to open security settings: " + ex.Message, ex);
                MessageBox.Show("ไม่สามารถเปิดตั้งค่าสิทธิ์ได้", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuildRecentFilesContextMenu()
        {
            var ctx = new ContextMenuStrip();

            var mnuOpen       = new ToolStripMenuItem("👁  เปิดดู",           null, CtxDash_Open_Click);
            var mnuDownload   = new ToolStripMenuItem("⬇  ดาวน์โหลด",        null, CtxDash_Download_Click);
            var mnuSeparator1 = new ToolStripSeparator();
            var mnuRename     = new ToolStripMenuItem("✏️  เปลี่ยนชื่อ",       null, CtxDash_Rename_Click);
            var mnuDelete     = new ToolStripMenuItem("🗑️  ลบ",               null, CtxDash_Delete_Click);
            var mnuSeparator2 = new ToolStripSeparator();
            var mnuPermission = new ToolStripMenuItem("🔒  ตั้งค่าสิทธิ์...", null, CtxDash_Permission_Click);
            var mnuSeparator3 = new ToolStripSeparator();
            var mnuProperties = new ToolStripMenuItem("ℹ️  คุณสมบัติ",         null, CtxDash_Properties_Click);

            ctx.Items.AddRange(new ToolStripItem[]
            {
                mnuOpen, mnuDownload,
                mnuSeparator1,
                mnuRename, mnuDelete,
                mnuSeparator2,
                mnuPermission,
                mnuSeparator3,
                mnuProperties
            });

            // enable/disable ตาม selection
            ctx.Opening += (s, e) =>
            {
                bool hasSelection = lvRecentFiles.SelectedItems.Count > 0;
                bool isSingleFile = lvRecentFiles.SelectedItems.Count == 1 && GetDashSelectedFile() != null;
                mnuOpen.Enabled       = isSingleFile;
                mnuDownload.Enabled   = isSingleFile;
                mnuRename.Enabled     = isSingleFile;
                mnuDelete.Enabled     = hasSelection;
                mnuPermission.Enabled = isSingleFile;
                mnuProperties.Enabled = lvRecentFiles.SelectedItems.Count > 0;

                // ถ้าไม่มีไฟล์เลือก ซ่อน menu ทั้งหมด
                if (lvRecentFiles.SelectedItems.Count == 0)
                    e.Cancel = true;
            };

            lvRecentFiles.ContextMenuStrip = ctx;

            // select item on right-click
            lvRecentFiles.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hit = lvRecentFiles.HitTest(e.Location);
                    if (hit.Item != null)
                    {
                        hit.Item.Selected = true;
                        hit.Item.Focused  = true;
                    }
                }
            };
        }

        /// <summary>คืน FileDocument ที่เลือกอยู่ใน lvRecentFiles (null ถ้าเลือก folder หรือไม่มี)</summary>
        private FileDocument GetDashSelectedFile()
        {
            if (lvRecentFiles.SelectedItems.Count == 0) return null;
            return lvRecentFiles.SelectedItems[0].Tag as FileDocument;
        }

        private List<FileDocument> GetDashSelectedFiles()
        {
            return lvRecentFiles.SelectedItems.Cast<ListViewItem>()
                .Select(item => item.Tag as FileDocument)
                .Where(file => file != null)
                .GroupBy(file => file.FileID)
                .Select(group => group.First())
                .ToList();
        }

        private async void CtxDash_Open_Click(object sender, EventArgs e)
        {
            var file = GetDashSelectedFile();
            if (file == null) return;

            try
            {
                string path = ResolveFilePhysicalPath(file) ?? OpenViaDownloadStream(file);
                if (string.IsNullOrEmpty(path))
                {
                    MessageBox.Show(string.Format("ไม่พบไฟล์: {0}", file.OriginalFileName),
                        "เปิดไฟล์ไม่ได้", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string ext = (file.FileExtension ?? System.IO.Path.GetExtension(path) ?? string.Empty)
                             .ToLowerInvariant().TrimStart('.');

                if (ext == "pdf")
                {
                    PdfViewOptions options = PdfViewOptions.ViewOnly;
                    if (_permissionCheckController != null)
                    {
                        var eff = _permissionCheckController.GetEffectivePermission(_currentUser.UserID, file.FolderID);
                        options = PdfViewOptions.FromEffective(eff, _currentUser);
                    }
                    await PdfSecurityHelper.OpenSecurePdfAsync(path, this, options);
                }
                else
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = path, UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถเปิดไฟล์ได้: " + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CtxDash_Download_Click(object sender, EventArgs e)
        {
            var file = GetDashSelectedFile();
            if (file == null || _fileBrowseController == null) return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title      = "บันทึกไฟล์";
                sfd.FileName   = file.OriginalFileName;
                string ext     = (file.FileExtension ?? "").TrimStart('.').ToLower();
                sfd.Filter     = string.Format("{0} files (*.{1})|*.{1}|All files (*.*)|*.*", ext.ToUpper(), ext);
                sfd.DefaultExt = ext;

                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    var result = _fileBrowseController.DownloadFile(file.FileID, _currentUser);
                    if (result == null || !result.Success)
                    {
                        MessageBox.Show(string.Format("ดาวน์โหลดไม่สำเร็จ: {0}",
                            result != null ? result.ErrorMessage : "ไม่ทราบสาเหตุ"),
                            "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    using (result.FileStream)
                    using (var fs = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                        result.FileStream.CopyTo(fs);

                    MessageBox.Show(string.Format("ดาวน์โหลด '{0}' เรียบร้อยแล้ว", file.OriginalFileName),
                        "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ดาวน์โหลดไม่สำเร็จ: " + ex.Message,
                        "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CtxDash_Rename_Click(object sender, EventArgs e)
        {
            var file = GetDashSelectedFile();
            if (file == null || _fileCommandController == null) return;

            using (var form   = new Form())
            using (var lbl    = new Label())
            using (var input  = new TextBox())
            using (var btnOk  = new Button())
            using (var btnCan = new Button())
            {
                form.Text            = "เปลี่ยนชื่อไฟล์";
                form.ClientSize      = new Size(360, 110);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition   = FormStartPosition.CenterParent;
                form.MaximizeBox     = false; form.MinimizeBox = false;

                lbl.Text = "ชื่อใหม่:"; lbl.Location = new Point(12, 12); lbl.Size = new Size(336, 20);
                input.Text = file.OriginalFileName; input.Location = new Point(12, 36); input.Size = new Size(336, 23); input.SelectAll();
                btnOk.Text = "OK"; btnOk.DialogResult = DialogResult.OK; btnOk.Location = new Point(192, 72); btnOk.Size = new Size(75, 26);
                btnCan.Text = "Cancel"; btnCan.DialogResult = DialogResult.Cancel; btnCan.Location = new Point(273, 72); btnCan.Size = new Size(75, 26);
                form.AcceptButton = btnOk; form.CancelButton = btnCan;
                form.Controls.AddRange(new Control[] { lbl, input, btnOk, btnCan });

                if (form.ShowDialog(this) != DialogResult.OK) return;
                string newName = input.Text.Trim();
                if (string.IsNullOrEmpty(newName)) return;

                try
                {
                    _fileCommandController.RenameFile(file.FileID, newName, _currentUser);
                    RefreshDashboardFiles();
                    MessageBox.Show(string.Format("เปลี่ยนชื่อเป็น '{0}' เรียบร้อยแล้ว", newName),
                        "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ไม่สามารถเปลี่ยนชื่อได้: " + ex.Message,
                        "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CtxDash_Delete_Click(object sender, EventArgs e)
        {
            var files = GetDashSelectedFiles();
            if (files.Count == 0 || _fileCommandController == null) return;

            var confirm = MessageBox.Show(
                files.Count == 1
                    ? string.Format("ลบไฟล์ '{0}' หรือไม่?\nไฟล์จะถูกย้ายไปยังถังขยะ", files[0].OriginalFileName)
                    : string.Format("ลบไฟล์ที่เลือกทั้งหมด {0} ไฟล์ หรือไม่?\nไฟล์จะถูกย้ายไปยังถังขยะ", files.Count),
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                int deleted = 0;
                int failed = 0;
                string firstError = null;

                foreach (var file in files)
                {
                    try
                    {
                        _fileCommandController.DeleteFile(file.FileID, _currentUser);
                        deleted++;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        failed++;
                        if (string.IsNullOrEmpty(firstError))
                            firstError = "ไม่มีสิทธิ์ลบไฟล์บางรายการ: " + ex.Message;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        if (string.IsNullOrEmpty(firstError))
                            firstError = "ไม่สามารถลบไฟล์บางรายการได้: " + ex.Message;
                    }
                }

                RefreshDashboardFiles();
                LoadDashboardStats();

                if (failed > 0)
                {
                    MessageBox.Show(
                        string.Format("ลบไฟล์เสร็จสิ้น\nสำเร็จ {0} ไฟล์\nล้มเหลว {1} ไฟล์\n\n{2}", deleted, failed, firstError ?? "กรุณาตรวจสอบรายการที่ลบไม่สำเร็จอีกครั้ง"),
                        "ผลการลบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("ไม่มีสิทธิ์ลบไฟล์นี้: " + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถลบไฟล์ได้: " + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CtxDash_Permission_Click(object sender, EventArgs e)
        {
            var file = GetDashSelectedFile();
            if (file != null)
                OpenFilePermissionDialog(file);
        }

        private void CtxDash_Properties_Click(object sender, EventArgs e)
        {
            if (lvRecentFiles.SelectedItems.Count == 0) return;
            var tag = lvRecentFiles.SelectedItems[0].Tag;

            var file   = tag as FileDocument;
            var folder = tag as Folder;

            if (file != null)
            {
                MessageBox.Show(
                    string.Format("ชื่อ: {0}\nประเภท: {1}\nขนาด: {2}\nอัพโหลด: {3}\nVersion: {4}",
                        file.OriginalFileName,
                        file.FileExtension.TrimStart('.').ToUpper(),
                        FormatFileSize(file.FileSizeBytes),
                        file.UploadedDate.ToString("dd/MM/yyyy HH:mm"),
                        file.Version),
                    string.Format("คุณสมบัติ — {0}", file.OriginalFileName),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (folder != null)
            {
                MessageBox.Show(
                    string.Format("ชื่อ: {0}\nระดับ: {1}\nไฟล์: {2} รายการ\nขนาด: {3}",
                        folder.FolderName,
                        ConfidentialityBadge(folder.ConfidentialityLevel),
                        folder.FileCount,
                        FormatFileSize(folder.FolderSizeBytes)),
                    string.Format("คุณสมบัติ — {0}", folder.FolderName),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LvRecentFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lvRecentFiles.SelectedItems.Count == 0) return;

            var folder = lvRecentFiles.SelectedItems[0].Tag as Folder;
            if (folder == null) return; // double-click on file row → ignore

            // Toggle expand/collapse
            if (_expandedFolderIds.Contains(folder.FolderID))
                _expandedFolderIds.Remove(folder.FolderID);
            else
                _expandedFolderIds.Add(folder.FolderID);

            // Rebuild the list preserving scroll position
            int topIndex = lvRecentFiles.TopItem != null ? lvRecentFiles.TopItem.Index : 0;
            RefreshDashboardFiles();
            if (topIndex < lvRecentFiles.Items.Count)
                lvRecentFiles.EnsureVisible(topIndex);
        }

        private void lblStatsContent_Click(object sender, EventArgs e)
        {

        }
    }
}
