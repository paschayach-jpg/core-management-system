using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Presentation.Modules.DMS;
using CMS.Presentation.Modules.ImportFile;
using CMS.Controllers.DMS.Document;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Services.DTOs;
using MenuItem = CMS.Domain.Models.MenuItem;

namespace CMS.Presentation
{
    /// <summary>
    /// MOD-002: Folder Structure Form.
    /// Left panel: TreeView with Confidentiality_Level badge (colored node text).
    /// Right panel: ListView showing subfolders of the selected folder.
    /// Bottom panel: Recycle bin showing soft-deleted folders with Restore / Permanently Delete.
    /// All actions are routed through FolderPresenter (MVP pattern).
    /// Satisfies Requirements 4.1–4.14, 7.6.
    /// </summary>
    public partial class FolderForm : Form, IFolderView
    {
        private FolderPresenter _presenter;

        // Backing lists for selection lookups
        private List<Folder> _treeRoots = new List<Folder>();

        // OrgUnit scope passed in from MDI_Container
        private readonly int? _orgUnitId;

        private readonly IFolderCommandController _folderCommandController;
        private readonly IFileBrowseController _fileBrowseController;
        private readonly IFileCommandController _fileCommandController;
        private readonly IUserAccessOrgLookupController _orgLookupController;
        private readonly IApprovalFolderController _approvalFolderController;
        private readonly IFolderTemplateController _folderTemplateController;
        private readonly IUploadCheckpointController _uploadCheckpointController;
        private readonly IQuickAccessController _quickAccessController;
        private readonly IPermissionCheckController _permissionCheckController;
        private readonly IFolderReadLookupController _folderReadLookupController;
        private readonly IAuditLogController _auditLogController;
        private readonly IShareCommandController _shareCommandController;
        private readonly IMenuManagementController _menuManagementController;
        private readonly IApprovalInboxController _approvalInboxController;
        private readonly IRecycleBinController _recycleBinController;
        private readonly IFileUploadController _fileUploadController;
        private readonly IShareManagementController _shareManagementController;
        private readonly IShareLookupController _shareLookupController;
        private readonly IFolderSettingsController _folderSettingsController;
        private readonly IFolderPermissionLookupController _folderPermissionLookupController;
        private readonly IUserAccountLookupController _userAccountLookupController;
        private readonly IUserAdminController _userAdminController;
        private readonly IUserOrgVisibilityController _userOrgVisibilityController;
        private readonly CMS.Services.Interfaces.IEmployeeApiService _employeeApiService;
        private readonly UserContext _user;

        // Quick Access panel (Req 7)
        private Panel _pnlQuickAccess;
        private ListBox _lstQuickAccess;
        private List<QuickAccessFolder> _quickAccessItems = new List<QuickAccessFolder>();

        // Cache subfolders of currently selected folder for bottom-left sync panel
        private List<Folder> _currentSubFolders = new List<Folder>();

        // Cache OrgUnit display names — populated once, reused in ShowFolderContents
        private Dictionary<int, string> _orgUnitNameCache = null;

        // Currently selected OrgUnit filter (null = all)
        private int? _selectedOrgUnitId;
        private string _selectedOrgUnitName = "— ทั้งหมด —";
        private OrgUnit _selectedOrgUnit = null; // เก็บ OrgUnit object เพื่อตรวจ Tier

        // Cache last selected folder so MDI toolbar can access it even after focus change
        private Folder _lastSelectedFolder;

        // Track selected file in content list
        private List<FileDocument> _currentFiles = new List<FileDocument>();

        // Inline rename (เหมือน Windows Explorer)
        private TextBox _inlineRenameBox;
        private ListViewItem _renamingItem;

        // Track active confidentiality level filter (0 = no filter, 1-4 = filtered level)
        private int _activeConfidentialityFilter = 0;

        // View mode toggle (Details / Large Icons / Tile)
        private View _currentView = View.Details;
        // OrgFilter view switcher (ตามองค์กร / ตามความลับ)
        private Panel _pnlViewButtons;
        private Button _btnOrgView;
        private Button _btnConfView;
        private Button _btnMainMenu;
        private TreeView _treeConfidentiality;

        // Sidebar toggle state
        private bool _sidebarCollapsed = false;
        private int _sidebarExpandedWidth = 303; // Default width
        private TreeView _treeMainMenu;
        private bool _showingConfView = false;
        private bool _showingMainMenuView = false;
        private bool _folderExplorerControlsVisible = true;
        private int _orgFilterDefaultHeight = 0;
        private bool _orgTreeLoaded = false;

        // Tracks which TreeNode is currently active (highlight)
        private TreeNode _activeTreeNode;

        // ป้องกัน PromptToCreateFirstFolder ยิงซ้ำระหว่างสร้าง Auto 4 Folders
        private bool _suppressCreatePrompt = false;
        // ป้องกัน MessageBox ซ้อน/เด้งซ้ำจาก event ที่วิ่งทับกัน
        private bool _isCreatePromptOpen = false;
        // ตรวจจับว่ามี HC folder pending approval อยู่ → reload เมื่อ form กลับมา focus
        private bool _pendingHCApproval = false;

        // Per-folder view-access cache — keyed by FolderId, valid for lifetime of this form instance.
        // Invalidated when permissions change (see InvalidateViewAccessCache).
        private readonly Dictionary<int, bool> _viewAccessCache = new Dictionary<int, bool>();

        // Drop zone overlay — shown when content list is empty to prompt drag-and-drop upload
        private Panel _pnlDropHint;
        private bool _isDragOverContent = false;

        // Allowed extensions for folder-drop extraction (BR-003-02)
        private static readonly HashSet<string> _folderDropAllowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".jpg", ".png", ".gif", ".zip"
        };

        // Clipboard state for Cut / Copy / Paste
        private enum ClipboardMode { None, Cut, Copy }
        private ClipboardMode _clipboardMode = ClipboardMode.None;
        private List<int> _clipboardFileIds = new List<int>();
        private List<FileDocument> _clipboardFiles = new List<FileDocument>();
        private readonly Dictionary<ListViewItem, Color> _cutHighlightedItems = new Dictionary<ListViewItem, Color>();
        private ToolTip _navToolTip;

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        public FolderForm(UserContext user, int? orgUnitId = null,
                          IFileBrowseController fileBrowseController = null,
                  IFileCommandController fileCommandController = null,
                  IFolderCommandController folderCommandController = null,
                  IUserAccessOrgLookupController orgLookupController = null,
                          IFolderTemplateController folderTemplateController = null,
                          IUploadCheckpointController uploadCheckpointController = null,
                  IQuickAccessController quickAccessController = null,
                  IApprovalFolderController approvalFolderController = null,
                  IPermissionCheckController permissionCheckController = null,
                  IAuditLogController auditLogController = null,
                          IShareCommandController shareCommandController = null,
                          IMenuManagementController menuManagementController = null,
                          IApprovalInboxController approvalInboxController = null,
                          IRecycleBinController recycleBinController = null,
                          IFileUploadController fileUploadController = null,
                          IShareManagementController shareManagementController = null,
                          IShareLookupController shareLookupController = null,
                          IFolderSettingsController folderSettingsController = null,
                          IFolderPermissionLookupController folderPermissionLookupController = null,
                          IUserAccountLookupController userAccountLookupController = null,
                          IUserAdminController userAdminController = null,
                          IUserOrgVisibilityController userOrgVisibilityController = null,
                          IFolderReadLookupController folderReadLookupController = null,
                          CMS.Services.Interfaces.IEmployeeApiService employeeApiService = null)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (folderCommandController == null) throw new ArgumentNullException("folderCommandController");
            if (folderReadLookupController == null) throw new ArgumentNullException("folderReadLookupController");

            _orgUnitId = orgUnitId;
            _selectedOrgUnitId = orgUnitId;
            _folderCommandController = folderCommandController;
            _fileBrowseController = fileBrowseController ?? new FileBrowseController(null);
            _fileCommandController = fileCommandController ?? new FileCommandController(null);
            _orgLookupController = orgLookupController;
            _folderTemplateController = folderTemplateController;
            _uploadCheckpointController = uploadCheckpointController ?? new UploadCheckpointController(null);
            _quickAccessController = quickAccessController;
            _approvalFolderController = approvalFolderController;
            _permissionCheckController = permissionCheckController;
            _auditLogController = auditLogController;
            _shareCommandController = shareCommandController;
            _menuManagementController = menuManagementController;
            _approvalInboxController = approvalInboxController;
            _recycleBinController = recycleBinController;
            _fileUploadController = fileUploadController;
            _shareManagementController = shareManagementController;
            _shareLookupController = shareLookupController ?? new ShareLookupController(null, null, null, null, null);
            _folderSettingsController = folderSettingsController ?? new FolderSettingsController(null);
            _folderPermissionLookupController = folderPermissionLookupController ?? new FolderPermissionLookupController();
            _userAccountLookupController = userAccountLookupController ?? new UserAccountLookupController();
            _userAdminController = userAdminController ?? new UserAdminController();
            _userOrgVisibilityController = userOrgVisibilityController;
            _folderReadLookupController = folderReadLookupController ?? new FolderReadLookupController(null);
            _employeeApiService = employeeApiService;
            _user = user;

            InitializeComponent();

            folderTreeView.HideSelection = false;
            folderTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            folderTreeView.DrawNode += FolderTreeView_DrawNode;

            _orgFilterDefaultHeight = pnlOrgFilter.Height;

            BuildContentImageList();

            _presenter = new FolderPresenter(
                this,
                user,
                _fileBrowseController,
                _folderCommandController,
                _folderReadLookupController);

            // Manager/Admin only — ซ่อนปุ่มผู้ดูแลสำหรับ user ทั่วไป
            bool canManage = user.Role == UserRole.Manager || user.Role == UserRole.Admin;

            // Archive — ผู้ดูแลเท่านั้น
            btnArchive.Visible = canManage;
            sep1.Visible = canManage;   // separator หน้า Archive

            // เปลี่ยนระดับ / ตัดลอกโครงสร้าง — ผู้ดูแลเท่านั้น
            btnChangeLevel.Visible = canManage;
            btnCopyStructure.Visible = canManage;
            sep3.Visible = canManage;   // separator หน้า ChangeLevel


            // Wire content list selection for detail panel
            contentsListView.SelectedIndexChanged += ContentsListView_SelectedIndexChanged;

            // Configure ListView for better selection behavior
            contentsListView.FullRowSelect = true;
            contentsListView.HideSelection = false;
            contentsListView.MultiSelect = true;

            // Wire mouse events for better interaction
            contentsListView.MouseDown += ContentsListView_MouseDown;
            contentsListView.MouseDoubleClick += ContentsListView_MouseDoubleClick;

            // F2 = inline rename (เหมือน Windows Explorer)
            contentsListView.KeyDown += (s, e2) =>
            {
                if (e2.Control && e2.KeyCode == Keys.X)
                {
                    CtxFileCut_Click(s, EventArgs.Empty);
                    e2.Handled = true;
                    return;
                }
                if (e2.Control && e2.KeyCode == Keys.C)
                {
                    CtxFileCopy_Click(s, EventArgs.Empty);
                    e2.Handled = true;
                    return;
                }
                if (e2.Control && e2.KeyCode == Keys.V)
                {
                    CtxFilePaste_Click(s, EventArgs.Empty);
                    e2.Handled = true;
                    return;
                }

                if (e2.KeyCode == Keys.F2)
                    btnRename_Click(s, EventArgs.Empty);
                else if (e2.KeyCode == Keys.Enter)
                {
                    // Enter key opens selected item (like double-click)
                    if (contentsListView.SelectedItems.Count > 0)
                    {
                        var selectedItem = contentsListView.SelectedItems[0];
                        var file = selectedItem.Tag as FileDocument;
                        var folder = selectedItem.Tag as Folder;

                        if (file != null)
                        {
                            CtxFileOpen_Click(s, EventArgs.Empty);
                        }
                        else if (folder != null)
                        {
                            NavigateToFolder(folder);
                        }
                    }
                }
            };

            // Alternating row colors for readability
            contentsListView.OwnerDraw = true;
            contentsListView.DrawColumnHeader += (s, e) => e.DrawDefault = true;
            contentsListView.DrawSubItem += ContentsListView_DrawSubItem;
            contentsListView.DrawItem += (s, e) => { /* handled in DrawSubItem */ };

            // ปรับ colName ให้เต็มพื้นที่เมื่อขนาดเปลี่ยน
            contentsListView.Resize += (s, e) => AdjustNameColumnWidth();
            this.Load += (s, e) => AdjustNameColumnWidth();

            // Defer OrgUnit loading until form is first shown to improve first paint.
            this.Shown += FolderForm_Shown;

            // Build right-click context menu for folderTreeView
            BuildFolderTreeContextMenu();

            // Build right-click context menu for contentsListView
            BuildContentsListViewContextMenu();

            // Build drag-and-drop upload zone overlay over content list
            BuildDropZoneOverlay();

            // Wire folder search (Req 3.5)
            txtFolderSearch.TextChanged += TxtFolderSearch_TextChanged;

            // Wire column sort (Req 4.5)
            contentsListView.ColumnClick += ContentsListView_ColumnClick;

            // Lock column widths to avoid jitter/flicker from runtime auto-resize.
            contentsListView.ColumnWidthChanging += (s, e2) =>
            {
                e2.Cancel = true;
                if (e2.ColumnIndex >= 0 && e2.ColumnIndex < contentsListView.Columns.Count)
                    e2.NewWidth = contentsListView.Columns[e2.ColumnIndex].Width;
            };

            // Wire btnMove (Req 9.1, 9.2, 9.4) — event already wired in Designer.cs, do NOT add again here
            folderTreeView.AfterSelect += (s, e2) => { UpdateMoveButtonState(); OnSelectionChanged(); };
            folderTreeView.BeforeSelect += FolderTreeView_BeforeSelect;

            // Handle คลิก node ที่ selected อยู่แล้ว (AfterSelect จะไม่ fire ในกรณีนี้)
            folderTreeView.NodeMouseClick += FolderTreeView_NodeMouseClick;

            // Handle คลิก OrgUnit node ที่ selected อยู่แล้ว → force reload content
            treeOrgUnit.NodeMouseClick += treeOrgUnit_NodeMouseClick;
            contentsListView.SelectedIndexChanged += (s, e2) => { UpdateMoveButtonState(); OnSelectionChanged(); };

            // Initialize Quick Access panel (Req 7)
            InitializeQuickAccessPanel();

            // Hide recycle bin components
            HideRecycleBinComponents();

            // Add view switcher tabs to OrgUnit panel
            InitializeOrgFilterSwitcher();

            // เมนูหลักย้ายไปอยู่ที่ MDI_Container left nav แล้ว — ไม่ต้องแสดงใน FolderForm

            // Wire navigation buttons
            btnNavBack.Click += BtnNavBack_Click;
            btnNavForward.Click += BtnNavForward_Click;
            btnNavUp.Click += BtnNavUp_Click;
            btnNavBack.Enabled = false;
            btnNavForward.Enabled = false;
            btnNavUp.Enabled = false;

            _navToolTip = new ToolTip();
            _navToolTip.SetToolTip(btnNavRefresh, "รีโหลดข้อมูลล่าสุด (นับจำนวนไฟล์จากข้อมูลจริง)");

            this.KeyPreview = true;
            this.KeyDown += (s, e2) =>
            {
                if (e2.KeyCode == Keys.F5)
                {
                    btnNavRefresh_Click(s, EventArgs.Empty);
                    e2.Handled = true;
                }
            };

            // Detail panel ขนาด 160px — พอดีกับปุ่มและข้อมูล
            splitContent.Panel2MinSize = 160;
            splitContent.SplitterDistance = Math.Max(this.Width - 165, 500);
        }

        private void FolderForm_Shown(object sender, EventArgs e)
        {
            if (_orgTreeLoaded)
                return;

            _orgTreeLoaded = true;
            BeginInvoke(new Action(() => LoadOrgUnitTree()));
        }

        // -----------------------------------------------------------------------
        // Navigation history (Back / Forward / Up — เหมือน Windows Explorer)
        // -----------------------------------------------------------------------

        private readonly Stack<int?> _backStack = new Stack<int?>();
        private readonly Stack<int?> _forwardStack = new Stack<int?>();
        private readonly Stack<string> _backBreadcrumbs = new Stack<string>();
        private readonly Stack<string> _forwardBreadcrumbs = new Stack<string>();
        private int? _currentNavFolderId = null;   // null = OrgUnit root view
        private string _currentBreadcrumb = "";
        private bool _isNavigating = false;  // ป้องกัน push ซ้ำขณะ navigate
        private bool _isRightClickSelecting = false;  // ข้าม BeforeSelect DB check ตอน right-click

        // Breadcrumb navigation data - เก็บ folder ID สำหรับแต่ละส่วนใน breadcrumb
        private readonly List<int?> _breadcrumbFolderIds = new List<int?>();

        // -----------------------------------------------------------------------
        // Sort state (Req 4.1–4.5)
        // -----------------------------------------------------------------------

        private int _sortColumn = -1;   // -1 = no active sort
        private bool _sortAscending = true;


        // -----------------------------------------------------------------------
        private void BuildFolderTreeContextMenu()
        {
            var ctx = new ContextMenuStrip();

            var mnuReload = new ToolStripMenuItem("🔄  รีโหลด", null, CtxReload_Click);
            var mnuNewFolder = new ToolStripMenuItem("📁  Folder ใหม่...", null, (s, e) => btnNewFolder_Click(s, e));
            var mnuOpen = new ToolStripMenuItem("📂  เปิด", null, CtxOpen_Click);
            var mnuDownload = new ToolStripMenuItem("⬇️  ดาวน์โหลด", null, (s, e) => OpenUploadForSelectedFolder());
            var mnuCut = new ToolStripMenuItem("✂️  ตัด", null, CtxCut_Click) { Enabled = false };
            var mnuCopy = new ToolStripMenuItem("📋  คัดลอก", null, CtxCopy_Click) { Enabled = false };
            var mnuPaste = new ToolStripMenuItem("📄  วาง", null, CtxPaste_Click) { Enabled = false };
            var mnuRename = new ToolStripMenuItem("✏️  เปลี่ยนชื่อ", null, (s, e) => btnRename_Click(s, e));
            var mnuMove = new ToolStripMenuItem("📦  ย้าย", null, BtnMove_Click);
            var mnuDelete = new ToolStripMenuItem("🗑️  ลบ Folder", null, (s, e) => btnDelete_Click(s, e));
            var mnuRecycleBin = new ToolStripMenuItem("🗑️  ถังขยะ...", null, (s, e) => OnMenuActionRequested("RecycleBin"));
            var mnuArchive = new ToolStripMenuItem("📦  Archive", null, (s, e) => btnArchive_Click(s, e));
            var mnuShare = new ToolStripMenuItem("🔗  แชร์...", null, CtxShare_Click);
            var mnuPermission = new ToolStripMenuItem("🔒  ตั้งค่าสิทธิ์...", null, CtxPermission_Click);
            var mnuMyPermission = new ToolStripMenuItem("🔐  สิทธิ์ของฉัน", null, CtxMyPermissionSummary_Click);
            var mnuSharedWithMe = new ToolStripMenuItem("📥  แบ่งปันให้ฉัน", null, CtxSharedWithMe_Click);
            var mnuProperties = new ToolStripMenuItem("ℹ️  รายละเอียด", null, CtxProperties_Click);
            var mnuAddQuickAccess = new ToolStripMenuItem("⭐  เพิ่มใน Quick Access", null, CtxAddQuickAccess_Click);

            ctx.Items.Add(mnuReload);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuNewFolder);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuOpen);
            //ctx.Items.Add(mnuDownload);
            ctx.Items.Add(new ToolStripSeparator());
            //ctx.Items.Add(mnuCut);
            //ctx.Items.Add(mnuCopy);
            //ctx.Items.Add(mnuPaste);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuRename);
            ctx.Items.Add(mnuMove);
            ctx.Items.Add(mnuDelete);
            ctx.Items.Add(mnuRecycleBin);
            ctx.Items.Add(new ToolStripSeparator());
            //ctx.Items.Add(mnuArchive);
            ctx.Items.Add(mnuShare);
            ctx.Items.Add(mnuPermission);
            ctx.Items.Add(mnuMyPermission);
            ctx.Items.Add(mnuSharedWithMe);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuAddQuickAccess);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuProperties);

            // enable/disable ตาม selection + FolderAuthorityConfig
            ctx.Opening += (s, e) =>
            {
                bool hasFolder = (SelectedFolder != null);
                int level = hasFolder ? SelectedFolder.ConfidentialityLevel : 0;
                bool isPrivileged = _user.AppRole >= 3;

                bool canView = isPrivileged || (hasFolder && _user.CanViewFolder(level));
                bool canAdd = isPrivileged || (hasFolder && _user.CanAddToFolder(level));
                bool canEdit = isPrivileged || (hasFolder && _user.CanEditFolder(level));

                // ถ้า selected folder เป็น Auto4 root folder (1HC/2SC/3NC/4PC) → ปิดเมนูแก้ไข folder เอง (BR-001-07)
                // แต่ยังอนุญาตให้สร้าง sub-folder ข้างใน ถ้ามีสิทธิ์ Add
                bool isAuto4Root = hasFolder && IsAuto4Folder(SelectedFolder)
                    && (!SelectedFolder.ParentFolderID.HasValue || SelectedFolder.ParentFolderID == 0);
                if (isAuto4Root)
                {
                    mnuNewFolder.Enabled = canAdd;
                    mnuOpen.Enabled = true;
                    mnuDownload.Enabled = false;
                    mnuRename.Enabled = false;
                    mnuMove.Enabled = false;
                    mnuDelete.Enabled = false;
                    mnuArchive.Enabled = false;
                    mnuShare.Enabled = false;
                    mnuPermission.Enabled = isPrivileged;
                    mnuMyPermission.Enabled = true;
                    mnuSharedWithMe.Enabled = true;
                    mnuProperties.Enabled = true;
                    mnuAddQuickAccess.Enabled = hasFolder && _quickAccessController != null;
                    return;
                }

                bool canDeleteFolder = hasFolder && canEdit && CanDeleteFolder(SelectedFolder);

                mnuNewFolder.Enabled = hasFolder && canAdd;
                mnuOpen.Enabled = hasFolder && canView;
                mnuDownload.Enabled = hasFolder && canView;
                mnuRename.Enabled = hasFolder && canEdit;
                mnuMove.Enabled = hasFolder && canEdit && _folderCommandController != null;
                mnuDelete.Enabled = canDeleteFolder;
                mnuRecycleBin.Enabled = true;
                mnuArchive.Enabled = hasFolder && canEdit;
                mnuShare.Enabled = hasFolder && canEdit && _shareCommandController != null
                    && SelectedFolder.ConfidentialityLevel != 1;
                mnuPermission.Enabled = hasFolder && isPrivileged;
                mnuMyPermission.Enabled = true;
                mnuSharedWithMe.Enabled = true;
                mnuProperties.Enabled = hasFolder && canView;
                mnuAddQuickAccess.Enabled = hasFolder && canView && _quickAccessController != null;
                // mnuReload ใช้ได้เสมอ
            };

            folderTreeView.ContextMenuStrip = ctx;

            // right-click ให้ select node ก่อนแสดง menu โดยข้าม BeforeSelect DB check
            // (context menu Opening event เช็ค permission เองอยู่แล้ว)
            folderTreeView.NodeMouseClick += (s, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right
                    && folderTreeView.SelectedNode != e.Node)
                {
                    _isRightClickSelecting = true;
                    try { folderTreeView.SelectedNode = e.Node; }
                    finally { _isRightClickSelecting = false; }
                }
            };
        }

        private void CtxReload_Click(object sender, EventArgs e)
        {
            RefreshCurrentViewAfterMove();
        }

        private void BuildContentsListViewContextMenu()
        {
            var ctx = new ContextMenuStrip();

            var mnuUpload = new ToolStripMenuItem("⬆️  อัปโหลดไฟล์...", null, (s, e) => OpenUploadForSelectedFolder());
            var mnuNewFolder = new ToolStripMenuItem("📁  Folder ใหม่...", null, (s, e) => btnNewFolder_Click(s, e));
            var mnuOpen = new ToolStripMenuItem("เปิดดู", null, CtxFileOpen_Click);
            var mnuDownload = new ToolStripMenuItem("ดาวน์โหลด", null, CtxFileDownload_Click);
            var mnuCut = new ToolStripMenuItem("ตัด", null, CtxFileCut_Click) { ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X, ShowShortcutKeys = true };
            var mnuCopy = new ToolStripMenuItem("คัดลอก", null, CtxFileCopy_Click) { ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C, ShowShortcutKeys = true };
            var mnuPaste = new ToolStripMenuItem("วาง", null, CtxFilePaste_Click) { ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V, ShowShortcutKeys = true };
            var mnuRename = new ToolStripMenuItem("เปลี่ยนชื่อ", null, (s, e) => btnRename_Click(s, e)) { ShortcutKeys = System.Windows.Forms.Keys.F2, ShowShortcutKeys = true };
            var mnuMove = new ToolStripMenuItem("ย้าย", null, BtnMove_Click);
            var mnuDelete = new ToolStripMenuItem("ลบ", null, (s, e) => btnDelete_Click(s, e)) { ShortcutKeys = System.Windows.Forms.Keys.Delete, ShowShortcutKeys = true };
            var mnuClearFolder = new ToolStripMenuItem("🗑️  ล้างไฟล์ทั้งหมดใน Folder...", null, (s, e) => ClearFolderContents());
            var mnuRecycleBin = new ToolStripMenuItem("🗑️  ถังขยะ...", null, (s, e) => OnMenuActionRequested("RecycleBin"));
            var mnuArchive = new ToolStripMenuItem("Archive", null, (s, e) => btnArchive_Click(s, e));
            var mnuHide = new ToolStripMenuItem("🔒 ซ่อน", null, (s, e) => HideSelectedFiles());
            var mnuUnhide = new ToolStripMenuItem("👁️ แสดง", null, (s, e) => UnhideSelectedFiles());
            var mnuShare = new ToolStripMenuItem("แชร์...", null, CtxShare_Click);
            var mnuPermission = new ToolStripMenuItem("ตั้งค่าสิทธิ์...", null, CtxPermission_Click);
            var mnuMyPermission = new ToolStripMenuItem("สิทธิ์ของฉัน", null, CtxMyPermissionSummary_Click);
            var mnuSharedWithMe = new ToolStripMenuItem("แบ่งปันให้ฉัน", null, CtxSharedWithMe_Click);
            var mnuProperties = new ToolStripMenuItem("รายละเอียด", null, CtxFileProperties_Click) { ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Enter, ShowShortcutKeys = true };

            ctx.Items.Add(mnuUpload);
            ctx.Items.Add(mnuNewFolder);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuOpen);
            ctx.Items.Add(mnuDownload);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuCut);
            ctx.Items.Add(mnuCopy);
            ctx.Items.Add(mnuPaste);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuRename);
            ctx.Items.Add(mnuMove);
            ctx.Items.Add(mnuDelete);
            ctx.Items.Add(mnuClearFolder);
            ctx.Items.Add(mnuRecycleBin);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuArchive);
            ctx.Items.Add(mnuHide);
            ctx.Items.Add(mnuUnhide);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuShare);
            ctx.Items.Add(mnuPermission);
            ctx.Items.Add(mnuMyPermission);
            ctx.Items.Add(mnuSharedWithMe);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuProperties);

            // enable/disable based on selection + FolderAuthorityConfig
            ctx.Opening += (s, e) =>
            {
                var selectedFiles = GetSelectedFilesFromContents();
                bool hasFileSelection = selectedFiles.Count > 0;
                bool hasFolderSelection = contentsListView.SelectedItems
                    .Cast<ListViewItem>()
                    .Any(item => item.Tag is Folder);
                bool singleFileSelected = hasFileSelection && !hasFolderSelection && selectedFiles.Count == 1;
                bool isAtRoot = !_currentNavFolderId.HasValue;
                bool hasFolder = !isAtRoot && (_currentNavFolderId.HasValue || SelectedFolder != null);

                // อยู่ที่ org level (มองเห็น 4 folder หลักเป็น content) → ยกเลิก menu ทั้งหมด
                // upload/create folder ต้องอยู่ใน 4 folder หลักเท่านั้น
                if (isAtRoot)
                {
                    e.Cancel = true;
                    return;
                }

                // right-click บน Auto4 root folder ใน content list → ยกเลิก menu (ห้ามแก้ไข root folder)
                var clickedFolders = contentsListView.SelectedItems
                    .Cast<ListViewItem>()
                    .Select(item => item.Tag as Folder)
                    .Where(f => f != null)
                    .ToList();
                bool clickedOnAuto4Root = !hasFileSelection
                    && clickedFolders.Count > 0
                    && clickedFolders.All(f => IsAuto4Folder(f) && (!f.ParentFolderID.HasValue || f.ParentFolderID == 0));
                if (clickedOnAuto4Root)
                {
                    e.Cancel = true;
                    return;
                }

                // level ของ folder ที่กำลังดูอยู่
                int level = _lastSelectedFolder != null ? _lastSelectedFolder.ConfidentialityLevel : 0;
                bool isPrivileged = _user.AppRole >= 3;

                bool canView = isPrivileged || _user.CanViewFolder(level);
                bool canAdd = isPrivileged || _user.CanAddToFolder(level);
                bool canEdit = isPrivileged || _user.CanEditFolder(level);

                // folders ที่เลือกใน content list (รองรับหลาย folder)
                var selectedFoldersInContent = contentsListView.SelectedItems
                    .Cast<ListViewItem>()
                    .Select(item => item.Tag as Folder)
                    .Where(f => f != null)
                    .ToList();
                Folder contentFolder = selectedFoldersInContent.Count > 0 ? selectedFoldersInContent[0] : null;

                // ลบได้ถ้า canEdit และมีไฟล์หรือ folder ที่ลบได้อยู่อย่างน้อย 1 รายการ
                bool canDeleteContent = canEdit && (hasFileSelection ||
                    selectedFoldersInContent.Any(f => CanDeleteFolder(f)));

                // Check if any item (file or folder) is selected in content list
                bool hasSelection = contentsListView.SelectedItems.Count > 0 &&
                                   (hasFileSelection || selectedFoldersInContent.Count > 0);

                bool canUseFileClipboard = hasFileSelection && !hasFolderSelection;
                bool canMove = hasSelection
                               && canEdit
                               && ((hasFileSelection && !hasFolderSelection && _fileCommandController != null)
                                   || (contentFolder != null && _folderCommandController != null));

                bool hasPendingPaste = _clipboardMode != ClipboardMode.None
                                     && _clipboardFileIds.Count > 0
                                     && hasFolder;

                mnuUpload.Enabled = hasFolder && canAdd && _fileUploadController != null;
                mnuNewFolder.Enabled = hasFolder && canAdd;
                mnuOpen.Enabled = singleFileSelected && canView;
                mnuDownload.Enabled = singleFileSelected && canView && _fileBrowseController != null;
                mnuCut.Enabled = canUseFileClipboard && canEdit && _fileCommandController != null;
                mnuCopy.Enabled = canUseFileClipboard && canView && _fileCommandController != null;
                mnuPaste.Enabled = hasPendingPaste && canAdd && _fileCommandController != null;
                mnuRename.Enabled = hasSelection && canEdit;
                mnuMove.Enabled = canMove;
                mnuDelete.Enabled = canDeleteContent;
                mnuClearFolder.Enabled = _fileBrowseController != null && _fileCommandController != null && _currentNavFolderId.HasValue && canEdit;
                mnuRecycleBin.Enabled = true;
                mnuArchive.Enabled = singleFileSelected && canEdit;
                mnuHide.Enabled = hasFileSelection && !hasFolderSelection && canEdit && _fileCommandController != null;
                mnuUnhide.Enabled = hasFileSelection && !hasFolderSelection && canEdit && _fileCommandController != null;
                bool singleFolderInContent = contentFolder != null && selectedFoldersInContent.Count == 1
                    && !hasFileSelection && contentFolder.ConfidentialityLevel != 1;
                mnuShare.Enabled = ((hasFileSelection && !hasFolderSelection) || singleFolderInContent) && canEdit && _shareCommandController != null;
                mnuPermission.Enabled = isPrivileged;
                mnuMyPermission.Enabled = true;
                mnuSharedWithMe.Enabled = true;
                mnuProperties.Enabled = singleFileSelected && canView;
            };

            contentsListView.ContextMenuStrip = ctx;
        }

        // -----------------------------------------------------------------------
        // Drop Zone Overlay — drag-and-drop upload from Windows Explorer
        // -----------------------------------------------------------------------

        private void BuildDropZoneOverlay()
        {
            // Overlay panel sits on top of contentsListView when folder is empty
            _pnlDropHint = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Cursor = Cursors.Default,
                AllowDrop = true,
                Visible = false
            };

            var lbl = new Label
            {
                Text = "ลากไฟล์มาวาง",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 43, 87, 154),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _pnlDropHint.Controls.Add(lbl);

            _pnlDropHint.Paint += (s, e) =>
            {
                var r = new Rectangle(16, 16, _pnlDropHint.Width - 33, _pnlDropHint.Height - 33);
                var color = _isDragOverContent
                    ? Color.FromArgb(43, 87, 154)
                    : Color.FromArgb(160, 43, 87, 154);
                using (var pen = new Pen(color, 2f))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, r);
                }
                lbl.ForeColor = _isDragOverContent
                    ? Color.FromArgb(43, 87, 154)
                    : Color.FromArgb(180, 43, 87, 154);
            };

            _pnlDropHint.DragEnter += ContentArea_DragEnter;
            _pnlDropHint.DragLeave += ContentArea_DragLeave;
            _pnlDropHint.DragDrop += ContentArea_DragDrop;

            // Also wire drag events directly to contentsListView (non-empty folders)
            contentsListView.AllowDrop = true;
            contentsListView.DragEnter += ContentArea_DragEnter;
            contentsListView.DragLeave += ContentArea_DragLeave;
            contentsListView.DragDrop += ContentArea_DragDrop;

            // Insert overlay into the same panel as contentsListView
            // It must be added AFTER contentsListView so BringToFront works correctly
            pnlContent.Controls.Add(_pnlDropHint);
            _pnlDropHint.BringToFront();

            // Share the same context menu so right-click works even when the folder is empty
            // and the drop-zone panel is covering contentsListView
            _pnlDropHint.ContextMenuStrip = contentsListView.ContextMenuStrip;
        }

        private void ContentArea_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop) || _showingMainMenuView
                || !_currentNavFolderId.HasValue || _fileUploadController == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            bool hasContent = paths != null && paths.Any(p => File.Exists(p) || Directory.Exists(p));
            e.Effect = hasContent ? DragDropEffects.Copy : DragDropEffects.None;
            _isDragOverContent = hasContent;
            _pnlDropHint.Invalidate();
        }

        private void ContentArea_DragLeave(object sender, EventArgs e)
        {
            _isDragOverContent = false;
            _pnlDropHint.Invalidate();
        }

        private void ContentArea_DragDrop(object sender, DragEventArgs e)
        {
            _isDragOverContent = false;
            _pnlDropHint.Invalidate();

            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (paths == null || paths.Length == 0) return;

            if (_showingMainMenuView)
            {
                ShowError("ไม่สามารถวางไฟล์ได้\nกรุณาเลือก Folder ปลายทางจากแถบโฟลเดอร์ก่อนอัปโหลด");
                return;
            }

            if (!_currentNavFolderId.HasValue)
            {
                ShowError("กรุณาเปิด Folder ก่อนอัปโหลด\nไม่สามารถอัปโหลดไฟล์ที่ระดับ Root ได้");
                return;
            }

            var droppedFolders = paths.Where(p => Directory.Exists(p)).ToArray();
            var droppedFiles   = paths.Where(p => File.Exists(p)).ToArray();

            if (droppedFolders.Length == 0)
            {
                OpenUploadWithFiles(droppedFiles);
                return;
            }

            // ---- Folder drop: extract files → show import dialog → upload ----
            int skipped;
            var extractedFiles = ExtractFilesFromFolders(droppedFolders, out skipped);
            var allFiles = droppedFiles.Concat(extractedFiles).Distinct().ToArray();

            if (allFiles.Length == 0)
            {
                string emptyMsg = skipped > 0
                    ? string.Format(
                        "ไม่พบไฟล์ที่รองรับภายในโฟลเดอร์\n\nข้ามไฟล์ที่ไม่รองรับ: {0} ไฟล์\n" +
                        "(รองรับเฉพาะ PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX, JPG, PNG, GIF, ZIP)", skipped)
                    : "โฟลเดอร์ที่เลือกไม่มีไฟล์ภายใน";
                MessageBox.Show(emptyMsg, "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new FolderDropImportDialog(droppedFolders, droppedFiles, extractedFiles, skipped))
            {
                if (dlg.ShowDialog(this.MdiParent ?? this) != DialogResult.OK) return;

                if ((dlg.CreateRootFolder || dlg.CreateFolderStructure) && droppedFolders.Length > 0)
                    ExecuteStructuredDrop(droppedFolders, dlg.PerSourceFilesDict,
                                          dlg.CreateRootFolder, dlg.CreateFolderStructure);
                else
                    OpenUploadWithFiles(dlg.AllFiles);
            }
        }

        private static string[] ExtractFilesFromFolders(string[] folderPaths, out int skippedCount)
        {
            var allowed = new List<string>();
            int skipped = 0;

            foreach (var folder in folderPaths)
            {
                try
                {
                    foreach (var file in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories))
                    {
                        if (_folderDropAllowedExt.Contains(Path.GetExtension(file)))
                            allowed.Add(file);
                        else
                            skipped++;
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }

            skippedCount = skipped;
            return allowed.ToArray();
        }

        private void SetDropHintVisible(bool visible)
        {
            if (_pnlDropHint == null) return;
            _pnlDropHint.Visible = visible;
            if (visible) _pnlDropHint.BringToFront();
        }

        private void CtxMyPermissionSummary_Click(object sender, EventArgs e)
        {
            if (OnMenuActionRequested("UserPermissionSummary"))
                return;

            ShowInfo("ไม่สามารถเปิดเมนู สิทธิ์ของฉัน ได้ในขณะนี้");
        }

        private void CtxSharedWithMe_Click(object sender, EventArgs e)
        {
            if (OnMenuActionRequested("SharedWithMe"))
                return;

            ShowInfo("ไม่สามารถเปิดเมนู แบ่งปันให้ฉัน ได้ในขณะนี้");
        }

        private void CtxShare_Click(object sender, EventArgs e)
        {
            if (OnMenuActionRequested("ShareDialog"))
                return;

            ShowInfo("ไม่สามารถเปิดเมนู แชร์ ได้ในขณะนี้");
        }

        private async void CtxFileOpen_Click(object sender, EventArgs e)
        {
            var file = SelectedFile;
            if (file == null) return;

            try
            {
                if (_fileBrowseController == null)
                {
                    ShowError("File browse controller is not available.");
                    return;
                }

                string filePath;
                string resolveError;
                if (!TryResolveOpenableFilePath(file, out filePath, out resolveError))
                {
                    ShowError(string.Format("ไม่พบไฟล์: {0}\n{1}", file.OriginalFileName, resolveError));
                    return;
                }

                string ext = (file.FileExtension ?? System.IO.Path.GetExtension(filePath) ?? string.Empty)
                             .ToLowerInvariant().TrimStart('.');

                if (ext == "pdf")
                {
                    // Print and Save are always blocked at the PDF encryption layer.
                    // CanCopy (text selection) is allowed only for users with write-level permission or above.
                    bool canCopy = false;
                    if (_permissionCheckController != null)
                    {
                        var eff = _permissionCheckController.GetEffectivePermission(_user.UserID, file.FolderID);
                        bool isManager = _user != null && _user.AppRole >= 3;
                        bool hasWrite = eff != null && (eff.HasFullControl || eff.CanModify || eff.CanWrite);
                        canCopy = isManager || hasWrite;
                    }

                    var options = new PdfViewOptions
                    {
                        CanPrint = false,   // always blocked
                        CanSave = false,   // always blocked
                        CanCopy = false    // always blocked
                    };

                    await PdfSecurityHelper.OpenSecurePdfAsync(filePath, this, options);
                }
                else
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                ShowError(string.Format("ไม่สามารถเปิดไฟล์ได้: {0}\nข้อผิดพลาด: {1}",
                    file.OriginalFileName, ex.Message));
            }
        }

        private bool TryResolveOpenableFilePath(FileDocument file, out string resolvedPath, out string errorMessage)
        {
            resolvedPath = null;
            errorMessage = null;

            try
            {
                // Fast path: direct/constructed physical path.
                string directPath = GetFilePhysicalPath(file);
                if (!string.IsNullOrWhiteSpace(directPath) && System.IO.File.Exists(directPath))
                {
                    resolvedPath = directPath;
                    return true;
                }

                // Fallback path: fetch verified stream from service and materialize to temp.
                if (_fileBrowseController == null)
                {
                    errorMessage = "File browse controller is not available.";
                    return false;
                }

                var result = _fileBrowseController.DownloadFile(file.FileID, _user);
                if (result == null)
                {
                    errorMessage = "ไม่พบข้อมูลไฟล์จาก service.";
                    return false;
                }

                if (!result.Success || result.FileStream == null)
                {
                    errorMessage = string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? "ดาวน์โหลดไฟล์เพื่อเปิดไม่สำเร็จ"
                        : result.ErrorMessage;
                    return false;
                }

                string safeName = BuildSafeTempFileName(file, result.FileName);
                string tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CMS", "OpenFiles");
                System.IO.Directory.CreateDirectory(tempDir);

                string tempPath = System.IO.Path.Combine(tempDir, safeName);
                using (result.FileStream)
                using (var fs = new System.IO.FileStream(tempPath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read))
                {
                    result.FileStream.CopyTo(fs);
                }

                resolvedPath = tempPath;
                return System.IO.File.Exists(tempPath);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private static string BuildSafeTempFileName(FileDocument file, string serviceFileName)
        {
            string fallback = !string.IsNullOrWhiteSpace(serviceFileName)
                ? serviceFileName
                : (!string.IsNullOrWhiteSpace(file.OriginalFileName)
                    ? file.OriginalFileName
                    : string.Format("file_{0}", file.FileID));

            string extension = System.IO.Path.GetExtension(fallback);
            if (string.IsNullOrWhiteSpace(extension) && !string.IsNullOrWhiteSpace(file.FileExtension))
                extension = "." + file.FileExtension.Trim().TrimStart('.');

            string baseName = System.IO.Path.GetFileNameWithoutExtension(fallback);
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = string.Format("file_{0}", file.FileID);

            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                baseName = baseName.Replace(c, '_');

            return string.Format("{0}_{1:yyyyMMddHHmmssfff}{2}", baseName, DateTime.Now, extension);
        }

        private void CtxFileDownload_Click(object sender, EventArgs e)
        {
            var file = SelectedFile;
            if (file == null) return;

            if (_fileBrowseController == null)
            {
                ShowError("File browse controller is not available.");
                return;
            }

            // ─── ขั้นที่ 1: ตรวจสอบว่ามี approved download request อยู่แล้วหรือไม่ ───
            ApprovalRequest approved = null;
            if (_approvalFolderController != null)
            {
                try { approved = _approvalFolderController.GetApprovedDownloadRequest(file.FileID, _user.UserID); }
                catch { /* ถ้า query fail ให้ผ่านไปขั้นส่งคำขอแทน */ }
            }

            if (approved == null)
            {
                // ─── ขั้นที่ 2: ยังไม่มี approval → ให้ส่งคำขอก่อน ───────────────
                using (var dlg = new DownloadApprovalRequestDialog(
                    file, _user, _approvalFolderController, _userAccountLookupController))
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        ShowInfo(string.Format(
                            "ส่งคำขอดาวน์โหลด '{0}' เรียบร้อยแล้ว\r\nกรุณารอการอนุมัติจาก Manager",
                            file.OriginalFileName));
                    }
                }
                return;
            }

            // ─── ขั้นที่ 3: มี approved → ให้ดาวน์โหลดได้ทันที ──────────────────
            using (var sfd = new SaveFileDialog())
            {
                sfd.Title    = "บันทึกไฟล์";
                sfd.FileName = file.OriginalFileName;

                string ext = (file.FileExtension ?? "").TrimStart('.').ToLower();
                sfd.Filter     = string.Format("{0} files (*.{1})|*.{1}|All files (*.*)|*.*", ext.ToUpper(), ext);
                sfd.DefaultExt = ext;

                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    var result = _fileBrowseController.DownloadFile(file.FileID, _user);

                    if (result == null || !result.Success)
                    {
                        ShowError(string.Format("ดาวน์โหลดไม่สำเร็จ: {0}",
                            result != null ? result.ErrorMessage : "ไม่ทราบสาเหตุ"));
                        return;
                    }

                    using (result.FileStream)
                    using (var fs = new System.IO.FileStream(
                        sfd.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                    {
                        result.FileStream.CopyTo(fs);
                    }

                    // บันทึกว่า approval ถูก consume แล้ว (status 2→5)
                    if (_approvalFolderController != null)
                        try { _approvalFolderController.ConsumeDownloadApproval(approved.RequestID, _user); }
                        catch { /* consume ล้มเหลวไม่ block การ download */ }

                    ShowInfo(string.Format("ดาวน์โหลด '{0}' เรียบร้อยแล้ว", file.OriginalFileName));
                }
                catch (UnauthorizedAccessException ex)
                {
                    ShowError("ไม่มีสิทธิ์ดาวน์โหลดไฟล์นี้: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ShowError("ดาวน์โหลดไม่สำเร็จ: " + ex.Message);
                }
            }
        }

        private void CtxFileProperties_Click(object sender, EventArgs e)
        {
            var file = SelectedFile;
            if (file != null)
            {
                string info = string.Format(
                    "ชื่อ: {0}\nประเภท: {1}\nขนาด: {2}\nอัปโหลด: {3}\nVersion: {4}\nสถานะ Scan: {5}",
                    file.OriginalFileName,
                    file.FileExtension.TrimStart('.').ToUpper(),
                    FormatBytes(file.FileSizeBytes),
                    file.UploadedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    file.Version,
                    VirusScanLabel(file.VirusScanStatus));
                MessageBox.Show(info, string.Format("รายละเอียด — {0}", file.OriginalFileName),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var folder = contentsListView.SelectedItems.Count > 0
                ? contentsListView.SelectedItems[0].Tag as Folder : null;
            if (folder != null)
            {
                string info = string.Format(
                    "ชื่อ: {0}\nระดับ: {1}\nไฟล์: {2} รายการ\nขนาด: {3}\nสร้างเมื่อ: {4}\nสถานะ: {5}",
                    folder.Name,
                    ConfidentialityLabel(folder.ConfidentialityLevel),
                    folder.FileCount,
                    FormatBytes(folder.FolderSizeBytes),
                    folder.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    folder.IsArchived ? "Archived" : "Active");
                MessageBox.Show(info, string.Format("รายละเอียด — {0}", folder.Name),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // -----------------------------------------------------------------------
        // Context menu handlers
        // -----------------------------------------------------------------------

        private void CtxOpen_Click(object sender, EventArgs e)
        {
            var folder = SelectedFolder;
            if (folder == null) return;
            if (!HasFolderViewAccess(folder))
            {
                ShowNoAccessFolderHint(folder.FolderName);
                return;
            }
            _presenter.GetFolderContents(folder.FolderId);
        }

        private void CtxCut_Click(object sender, EventArgs e)
        {
            // placeholder — ยังไม่ implement move สำหรับ Folder tree
            ShowInfo("ฟีเจอร์ตัด/ย้าย Folder จะพร้อมใช้งานในเวอร์ชันถัดไป");
        }

        private void CtxCopy_Click(object sender, EventArgs e)
        {
            ShowInfo("ฟีเจอร์คัดลอก Folder จะพร้อมใช้งานในเวอร์ชันถัดไป");
        }

        private void CtxPaste_Click(object sender, EventArgs e)
        {
            ShowInfo("ฟีเจอร์วาง Folder จะพร้อมใช้งานในเวอร์ชันถัดไป");
        }

        // ── File list: Cut / Copy / Paste ──────────────────────────────────────

        private void CtxFileCut_Click(object sender, EventArgs e)
        {
            var files = GetSelectedFilesFromContents();
            if (files.Count == 0) return;

            ClearCutHighlight();

            _clipboardMode = ClipboardMode.Cut;
            _clipboardFileIds = files.Select(f => f.FileID).Distinct().ToList();
            _clipboardFiles = files;

            ApplyCutHighlightToSelectedFiles();
        }

        private void CtxFileCopy_Click(object sender, EventArgs e)
        {
            var files = GetSelectedFilesFromContents();
            if (files.Count == 0) return;

            ClearCutHighlight();

            _clipboardMode = ClipboardMode.Copy;
            _clipboardFileIds = files.Select(f => f.FileID).Distinct().ToList();
            _clipboardFiles = files;
        }

        private void CtxFilePaste_Click(object sender, EventArgs e)
        {
            if (_clipboardMode == ClipboardMode.None || _clipboardFileIds.Count == 0) return;
            if (!_currentNavFolderId.HasValue) { ShowError("กรุณาเลือก Folder ปลายทางก่อนวาง"); return; }
            if (_fileCommandController == null) return;

            int destFolderId = _currentNavFolderId.Value;

            if (_clipboardMode == ClipboardMode.Cut)
            {
                try
                {
                    var req = new CMS.Services.DTOs.MoveFileRequest
                    {
                        FileIds = new List<int>(_clipboardFileIds),
                        DestinationFolderId = destFolderId
                    };
                    _fileCommandController.MoveFiles(req, _user);

                    ClearCutHighlight();
                    _clipboardMode = ClipboardMode.None;
                    _clipboardFileIds.Clear();
                    _clipboardFiles.Clear();

                    if (_currentNavFolderId.HasValue)
                        _presenter.GetFolderContents(_currentNavFolderId.Value);
                }
                catch (Exception ex)
                {
                    ClearCutHighlight();
                    ShowError("ย้ายไฟล์ไม่สำเร็จ: " + ex.Message);
                }
            }
            else
            {
                PasteCopyWithDuplicateCheck(destFolderId);
            }
        }

        private void PasteCopyWithDuplicateCheck(int destFolderId)
        {
            // Resolve file list from stored clipboard files (matched by ID)
            var clipIds = new HashSet<int>(_clipboardFileIds);
            var filesToCopy = _clipboardFiles
                .Where(f => clipIds.Contains(f.FileID))
                .ToList();

            // Fallback: if stored objects are missing, show an error
            if (filesToCopy.Count == 0)
            {
                ShowError("ไม่พบข้อมูลไฟล์ที่ต้องการคัดลอก กรุณา Copy ใหม่อีกครั้ง");
                return;
            }

            // ── Duplicate check ───────────────────────────────────────────────
            var fileNames = filesToCopy.Select(f => f.OriginalFileName).ToList();
            var duplicateNames = new List<string>();

            if (_fileUploadController != null)
            {
                try
                {
                    duplicateNames = _fileUploadController
                        .FindDuplicateNames(fileNames, destFolderId, _user)
                        .ToList();
                }
                catch { /* ถ้าเช็คไม่ได้ ให้ทำต่อโดยไม่ข้าม */ }
            }

            var duplicateChoice = DuplicateFileChoiceDialog.Choice.SaveCopy;
            var duplicateSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (duplicateNames.Count > 0)
            {
                using (var dlg = new DuplicateFileChoiceDialog("โฟลเดอร์ปลายทาง", duplicateNames))
                {
                    var owner = this.MdiParent ?? this;
                    if (dlg.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK
                        || dlg.SelectedChoice == DuplicateFileChoiceDialog.Choice.Cancel)
                        return;

                    duplicateChoice = dlg.SelectedChoice;

                    if (dlg.SelectedChoice == DuplicateFileChoiceDialog.Choice.SkipDuplicates)
                    {
                        duplicateSet = new HashSet<string>(duplicateNames, StringComparer.OrdinalIgnoreCase);
                        filesToCopy = filesToCopy
                            .Where(f => !duplicateSet.Contains(f.OriginalFileName))
                            .ToList();

                        if (filesToCopy.Count == 0)
                        {
                            ShowInfo("ข้ามไฟล์ซ้ำทั้งหมดแล้ว ไม่มีไฟล์ที่ต้องคัดลอก");
                            return;
                        }
                    }
                    else if (dlg.SelectedChoice == DuplicateFileChoiceDialog.Choice.Replace)
                    {
                        duplicateSet = new HashSet<string>(duplicateNames, StringComparer.OrdinalIgnoreCase);
                    }
                    // Choice.SaveCopy → copy ทุกไฟล์ โดย service จะ rename อัตโนมัติ
                }
            }

            // ── Progress dialog ───────────────────────────────────────────────
            var copyItems = filesToCopy.Select(f => new FileCopyInput
            {
                FileId = f.FileID,
                FileName = f.OriginalFileName,
                Overwrite = duplicateChoice == DuplicateFileChoiceDialog.Choice.Replace
                            && duplicateSet.Contains(f.OriginalFileName)
            });
            using (var progressForm = new FileCopyProgressForm(copyItems, destFolderId, _fileCommandController, _user))
            {
                progressForm.ShowDialog(this.MdiParent ?? this);
            }

            // Refresh folder view
            if (_currentNavFolderId.HasValue)
                _presenter.GetFolderContents(_currentNavFolderId.Value);
        }

        private List<FileDocument> GetSelectedFilesFromContents()
        {
            if (contentsListView.SelectedItems.Count == 0)
                return new List<FileDocument>();

            return contentsListView.SelectedItems
                .Cast<ListViewItem>()
                .Select(item => item.Tag as FileDocument)
                .Where(file => file != null)
                .ToList();
        }

        private void ApplyCutHighlightToSelectedFiles()
        {
            foreach (var item in contentsListView.SelectedItems.Cast<ListViewItem>())
            {
                if (!(item.Tag is FileDocument))
                    continue;

                if (!_cutHighlightedItems.ContainsKey(item))
                    _cutHighlightedItems[item] = item.ForeColor;

                item.ForeColor = Color.Gray;
            }
        }

        private void ClearCutHighlight()
        {
            foreach (var pair in _cutHighlightedItems.ToList())
            {
                if (pair.Key != null && pair.Key.ListView != null && !pair.Key.ListView.IsDisposed)
                    pair.Key.ForeColor = pair.Value;
            }

            _cutHighlightedItems.Clear();
        }

        // Event สำหรับแจ้ง MDI_Container ให้เปิด PermissionForm (folder)
        public event EventHandler<Folder> OpenPermissionRequested;

        // Event สำหรับแจ้ง MDI_Container ให้เปิด FilePermissionForm (file)
        public event EventHandler<FileDocument> OpenFilePermissionRequested;

        // Event สำหรับแจ้ง MDI_Container เมื่อ selection เปลี่ยน (Req 9.4)
        public event EventHandler SelectionChanged;

        public delegate bool MenuActionRequestHandler(object sender, string actionKey);
        public event MenuActionRequestHandler MenuActionRequested;

        private void OnSelectionChanged()
        {
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
        }

        private bool OnMenuActionRequested(string actionKey)
        {
            if (MenuActionRequested == null)
                return false;

            bool handled = false;
            foreach (MenuActionRequestHandler handler in MenuActionRequested.GetInvocationList())
                handled = handler(this, actionKey) || handled;

            return handled;
        }

        private void CtxPermission_Click(object sender, EventArgs e)
        {
            // ถ้ามี file ที่เลือกอยู่ใน contentsListView → เปิด file permission
            var file = SelectedFile;
            if (file != null)
            {
                if (OpenFilePermissionRequested != null)
                    OpenFilePermissionRequested(this, file);
                else
                    ShowInfo("กรุณาเปิดผ่าน MDI Container เพื่อตั้งค่าสิทธิ์");
                return;
            }

            // ไม่มี file → ใช้ folder ที่เลือกใน tree
            var folder = SelectedFolder;
            if (folder == null) return;

            if (OpenPermissionRequested != null)
                OpenPermissionRequested(this, folder);
            else
                ShowInfo("กรุณาเปิดผ่าน MDI Container เพื่อตั้งค่าสิทธิ์");
        }

        private void CtxProperties_Click(object sender, EventArgs e)
        {
            var folder = SelectedFolder;
            if (folder == null) return;

            string info = string.Format(
                "ชื่อ: {0}\nระดับ: {1}\nไฟล์: {2} รายการ\nขนาด: {3}\nสร้างเมื่อ: {4}\nสถานะ: {5}",
                folder.Name,
                ConfidentialityLabel(folder.ConfidentialityLevel),
                folder.FileCount,
                FormatBytes(folder.FolderSizeBytes),
                folder.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                folder.IsArchived ? "Archived" : "Active");

            MessageBox.Show(info, string.Format("รายละเอียด — {0}", folder.Name),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // -----------------------------------------------------------------------
        // OrgFilter view switcher — ตามองค์กร / ตามความลับ
        // -----------------------------------------------------------------------

        private void InitializeOrgFilterSwitcher()
        {
            var accent = Color.FromArgb(31, 56, 100);
            var inactive = Color.FromArgb(240, 244, 250);

            // Button bar
            _pnlViewButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(220, 230, 245),
                Padding = new Padding(2, 5, 2, 0)
            };

            int btnWidth = 90;

            // _btnMainMenu ย้ายไป MDI_Container left nav — ยังสร้างเพื่อป้องกัน null ref แต่ไม่แสดง
            _btnMainMenu = new Button { Visible = false };

            _btnOrgView = new Button
            {
                Text = "ตามองค์กร",
                Width = btnWidth,
                Height = 28,
                Location = new Point(5, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnOrgView.FlatAppearance.BorderSize = 0;

            _btnConfView = new Button
            {
                Text = "🔒 ตามชั้นความลับ",
                Width = 120,
                Height = 28,
                Location = new Point(5 + btnWidth + 2, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = inactive,
                ForeColor = accent,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnConfView.FlatAppearance.BorderSize = 0;

            _pnlViewButtons.Controls.Add(_btnOrgView);
            _pnlViewButtons.Controls.Add(_btnConfView);

            // Confidentiality tree (initially hidden)
            _treeConfidentiality = new TreeView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10f),
                HotTracking = true,
                Visible = false,
                Indent = 16,
                HideSelection = false  // คง highlight node ที่เลือกไว้แม้ focus อยู่ที่ control อื่น
            };

            // Main Menu tree (initially hidden)
            _treeMainMenu = new TreeView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10.5f),
                HotTracking = true,
                Visible = false,
                Indent = 16,
                HideSelection = false,
                ShowLines = true

            };

            // Re-parent treeOrgUnit into pnlOrgFilter alongside the conf tree
            // Both use DockStyle.Fill and overlap — only one is visible at a time
            pnlOrgFilter.Controls.Remove(treeOrgUnit);
            treeOrgUnit.Dock = DockStyle.Fill;
            treeOrgUnit.HideSelection = false;  // คง highlight node ที่เลือกไว้แม้ focus อยู่ที่ control อื่น

            // Expand / Collapse all — row below tab buttons
            var pnlExpandCollapse = new Panel
            {
                Dock = DockStyle.Top,
                Height = 26,
                BackColor = Color.FromArgb(225, 233, 245),
                Padding = new Padding(4, 2, 4, 2)
            };

            var btnExpandAll = new Button
            {
                Text = "▷ ขยายทั้งหมด",
                Width = 104,
                Height = 22,
                Location = new Point(4, 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 244, 250),
                ForeColor = Color.FromArgb(31, 56, 100),
                Font = new Font("Segoe UI", 8f, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnExpandAll.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 230);
            btnExpandAll.FlatAppearance.BorderSize = 1;

            var btnCollapseAll = new Button
            {
                Text = "◁ ย่อทั้งหมด",
                Width = 100,
                Height = 22,
                Location = new Point(112, 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 244, 250),
                ForeColor = Color.FromArgb(31, 56, 100),
                Font = new Font("Segoe UI", 8f, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnCollapseAll.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 230);
            btnCollapseAll.FlatAppearance.BorderSize = 1;

            btnExpandAll.Click += (s, e) => GetActiveOrgTree()?.ExpandAll();
            btnCollapseAll.Click += (s, e) => GetActiveOrgTree()?.CollapseAll();

            pnlExpandCollapse.Controls.Add(btnExpandAll);
            pnlExpandCollapse.Controls.Add(btnCollapseAll);

            pnlOrgFilter.Controls.Add(treeOrgUnit);            // index 0 (front)
            pnlOrgFilter.Controls.Add(_treeConfidentiality);   // index 1
            pnlOrgFilter.Controls.Add(pnlExpandCollapse);      // index 2 → row ใต้ tab buttons
            pnlOrgFilter.Controls.Add(_pnlViewButtons);        // index 3 (back) → docked Top first

            // Wire events
            _btnOrgView.Click += (s, e) => SwitchToOrgView();
            _btnConfView.Click += (s, e) => SwitchToConfView();
            _treeConfidentiality.BeforeSelect += ConfidentialityTree_BeforeSelect;
            _treeConfidentiality.NodeMouseClick += ConfTree_NodeMouseClick;
        }

        private TreeView GetActiveOrgTree()
        {
            if (treeOrgUnit.Visible) return treeOrgUnit;
            if (_treeConfidentiality != null && _treeConfidentiality.Visible) return _treeConfidentiality;
            return null;
        }

        private void LoadMainMenuTree()
        {
            var expandedTexts = new HashSet<string>();
            CollectConfExpandedTexts(_treeMainMenu.Nodes, expandedTexts);

            _treeMainMenu.Nodes.Clear();
            _treeMainMenu.BeginUpdate();

            try
            {
                TryLoadMainMenuFromManagement();
            }
            finally
            {
                _treeMainMenu.EndUpdate();
            }

            if (expandedTexts.Count > 0)
                RestoreConfExpandedTexts(_treeMainMenu.Nodes, expandedTexts);
        }

        private bool TryLoadMainMenuFromManagement()
        {
            if (_menuManagementController == null)
                return false;

            var menus = GetMainMenuHierarchy();
            if (menus == null || menus.Count == 0)
                return false;

            var activeMenus = FlattenMenuHierarchy(menus)
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.MenuName)
                .ToList();

            if (activeMenus.Count == 0)
                return false;

            var roots = activeMenus.Where(m => !m.ParentMenuItemID.HasValue).ToList();
            if (roots.Count == 0)
                return false;

            foreach (var root in roots)
                _treeMainMenu.Nodes.Add(BuildMenuNodeFromManagement(root, activeMenus));

            return _treeMainMenu.Nodes.Count > 0;
        }

        private static bool HasRequiredDefaultDmsActions(ISet<string> availableActions)
        {
            if (availableActions == null || availableActions.Count == 0)
                return false;

            string[] requiredActions =
            {
                "Home",
                "SharedWithMe",
                "ShareManagement",
                "OrgStructure",
                "UserManagement",
                "ApprovalInbox",
                "ConfigAdmin",
                "FolderTemplate"
            };

            return requiredActions.All(availableActions.Contains);
        }

        private static bool IsNonDmsManagedRoot(MenuItem root)
        {
            if (root == null)
                return false;

            return IsKnowledgeBaseMenuTree(root);
        }

        private static bool IsKnowledgeBaseMenuTree(MenuItem menu)
        {
            if (menu == null)
                return false;

            string moduleCode = (menu.ModuleCode ?? string.Empty).Trim();
            if (moduleCode.Equals("KMS", StringComparison.OrdinalIgnoreCase))
                return true;

            string action = ResolveMenuActionTag(menu);
            if (string.Equals(action, "KnowledgeBaseForm", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "KBCategoryForm", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (menu.ChildMenuItems == null || menu.ChildMenuItems.Count == 0)
                return false;

            return menu.ChildMenuItems.All(IsKnowledgeBaseMenuTree);
        }

        private List<MenuItem> GetMainMenuHierarchy()
        {
            // Build full visible tree recursively for all roles.
            // This avoids shallow results from GetVisibleMenus() that may include only one child level.
            var roots = _menuManagementController.GetRootMenus(_user)?.ToList() ?? new List<MenuItem>();
            foreach (var root in roots)
                PopulateVisibleChildrenRecursive(root);

            return roots;
        }

        private void PopulateVisibleChildrenRecursive(MenuItem parent)
        {
            if (parent == null)
                return;

            var children = _menuManagementController.GetChildMenus(parent.MenuItemID, _user)?.ToList() ?? new List<MenuItem>();
            parent.ChildMenuItems = children;

            foreach (var child in children)
                PopulateVisibleChildrenRecursive(child);
        }

        private static List<MenuItem> FlattenMenuHierarchy(IEnumerable<MenuItem> roots)
        {
            var result = new List<MenuItem>();
            var visited = new HashSet<int>();

            foreach (var menu in roots)
                FlattenMenuHierarchyRecursive(menu, result, visited);

            return result;
        }

        private static void FlattenMenuHierarchyRecursive(MenuItem menu, List<MenuItem> result, HashSet<int> visited)
        {
            if (menu == null || visited.Contains(menu.MenuItemID))
                return;

            visited.Add(menu.MenuItemID);
            result.Add(menu);

            if (menu.ChildMenuItems == null || menu.ChildMenuItems.Count == 0)
                return;

            foreach (var child in menu.ChildMenuItems)
                FlattenMenuHierarchyRecursive(child, result, visited);
        }

        private TreeNode BuildMenuNodeFromManagement(MenuItem menu, List<MenuItem> allMenus)
        {
            var children = allMenus
                .Where(m => m.ParentMenuItemID == menu.MenuItemID)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.MenuName)
                .ToList();

            bool hasChildren = children.Count > 0;
            var node = new TreeNode(menu.MenuName)
            {
                Tag = menu,
                ImageKey = ResolveMainMenuIconKey(menu, hasChildren),
                SelectedImageKey = ResolveMainMenuIconKey(menu, hasChildren)
            };

            foreach (var child in children)
                node.Nodes.Add(BuildMenuNodeFromManagement(child, allMenus));

            if (node.Nodes.Count == 0)
            {
                // Priority 1: Use FormClassName if provided
                if (!string.IsNullOrEmpty(menu.FormClassName))
                {
                    node.Tag = menu.FormClassName.Trim();
                }
                else
                {
                    // Priority 2: Try to map MenuKey to action tag
                    string actionTag = MapMenuKeyToActionTag(menu.MenuKey);
                    if (!string.IsNullOrEmpty(actionTag))
                        node.Tag = actionTag;
                }
            }

            return node;
        }

        private static string ResolveMainMenuIconKey(MenuItem menu, bool hasChildren)
        {
            if (menu == null)
                return "folder";

            string explicitIconKey = NormalizeMainMenuIconKey(menu.IconKey);
            if (!string.IsNullOrEmpty(explicitIconKey))
                return explicitIconKey;

            string action = MapConfiguredActionToTag(menu.FormClassName)
                ?? MapMenuKeyToActionTag(menu.MenuKey)
                ?? MapMenuNameToActionTag(menu.MenuName);

            if (hasChildren)
                return ResolveMainMenuGroupIconKey(action, menu.MenuName);

            return ResolveMainMenuLeafIconKey(action, menu.MenuName);
        }

        private static string ResolveMainMenuGroupIconKey(string action, string menuName)
        {
            switch ((action ?? string.Empty).Trim())
            {
                case "OrgGroup":
                case "UserGroup":
                    return "organization";
                case "PermissionGroup":
                    return "book_orange";
                case "ReportGroup":
                    return "book_blue";
                case "DocumentGroup":
                    return "file_docs";
                case "SystemGroup":
                case "SupportGroup":
                    return "lightning_bolt";
                case "GeneralGroup":
                    return "folder";
                default:
                    break;
            }

            string lowerName = (menuName ?? string.Empty).ToLowerInvariant();
            if (lowerName.Contains("เอกสาร") || lowerName.Contains("document") || lowerName.Contains("docs"))
                return "file_docs";
            if (lowerName.Contains("ผู้ใช้") || lowerName.Contains("บัญชี") || lowerName.Contains("org") || lowerName.Contains("user") || lowerName.Contains("องค์กร"))
                return "organization";
            if (lowerName.Contains("สิทธิ") || lowerName.Contains("permission") || lowerName.Contains("auth"))
                return "book_orange";
            if (lowerName.Contains("รายงาน") || lowerName.Contains("report") || lowerName.Contains("log"))
                return "book_blue";
            if (lowerName.Contains("support") || lowerName.Contains("tool") || lowerName.Contains("เครื่องมือ") || lowerName.Contains("sync") || lowerName.Contains("workflow"))
                return "lightning_bolt";
            if (lowerName.Contains("ทั่วไป") || lowerName.Contains("general") || lowerName.Contains("menu"))
                return "folder";
            if (lowerName.Contains("ระบบ") || lowerName.Contains("config") || lowerName.Contains("setting") || lowerName.Contains("ตั้งค่า"))
                return "book_green";

            return "folder";
        }

        private static string ResolveMainMenuLeafIconKey(string action, string menuName)
        {
            switch ((action ?? string.Empty).Trim())
            {
                case "Home":
                    return "folder";
                case "ShareDialog":
                case "SharedWithMe":
                case "ShareManagement":
                case "UserPermissionSummary":
                    return "book_orange";
                case "KnowledgeBaseForm":
                case "KBMainForm":
                case "KBCategoryForm":
                case "ReportExcel":
                case "AuditLog":
                case "AccessLog":
                case "ApprovalInbox":
                case "PdpaReport":
                case "SLAReport":
                case "WorkflowDetail":
                case "WorkflowTemplateList":
                case "WorkflowDashboard":
                case "WorkflowVisualFlow":
                    return "book_blue";
                case "OrgStructure":
                    return "folder";
                case "UserManagement":
                case "UserAccess":
                case "FolderSettings":
                case "AuthorityConfig":
                    return "organization";
                case "PermissionAdmin":
                    return "book_orange";
                case "MenuManagement":
                case "ConfigAdmin":
                    return "folder";
                case "FolderTemplate":
                    return "folder";
                case "SyncEmployee":
                case "WorkflowDesigner":
                case "WorkflowDelegation":
                    return "lightning_bolt";
                case "RecycleBin":
                case "ArchiveFolder":
                    return "cat_trash";
                default:
                    break;
            }

            string lowerName = (menuName ?? string.Empty).ToLowerInvariant();
            if (lowerName.Contains("แชร์") || lowerName.Contains("share"))
                return "book_orange";
            if (lowerName.Contains("template") || lowerName.Contains("เทมเพลต") || lowerName.Contains("แม่แบบ"))
                return "folder";
            if (lowerName.Contains("ตั้งค่า") || lowerName.Contains("config") || lowerName.Contains("menu"))
                return "book_green";
            if (lowerName.Contains("workflow") || lowerName.Contains("sync") || lowerName.Contains("มอบหมาย"))
                return "lightning_bolt";
            if (lowerName.Contains("ถังขยะ") || lowerName.Contains("archive") || lowerName.Contains("recycle") || lowerName.Contains("trash"))
                return "cat_trash";
            if (lowerName.Contains("รายงาน") || lowerName.Contains("report") || lowerName.Contains("log"))
                return "book_blue";
            if (lowerName.Contains("สิทธิ") || lowerName.Contains("permission") || lowerName.Contains("auth"))
                return "book_orange";
            if (lowerName.Contains("องค์กร") || lowerName.Contains("ผู้ใช้") || lowerName.Contains("org") || lowerName.Contains("user"))
                return "organization";

            return "file_docs";
        }

        private static string NormalizeMainMenuIconKey(string iconKey)
        {
            if (string.IsNullOrWhiteSpace(iconKey))
                return null;

            switch (iconKey.Trim().ToLowerInvariant())
            {
                case "folder":
                case "open_folder":
                case "organization":
                case "file_docs":
                case "book_red":
                case "book_orange":
                case "book_green":
                case "book_blue":
                case "lightning_bolt":
                case "cat_trash":
                    return iconKey.Trim();
                case "book_bule":
                    return "book_blue";
                default:
                    return null;
            }
        }

        private static string MapConfiguredActionToTag(string configuredValue)
        {
            string resolvedAction = MenuActionKeyCatalog.ResolveConfiguredActionKey(configuredValue);
            string token = string.IsNullOrWhiteSpace(resolvedAction) ? configuredValue : resolvedAction;
            if (string.IsNullOrWhiteSpace(token))
                return null;

            token = token.Trim();
            int lastDot = token.LastIndexOf('.');
            if (lastDot >= 0 && lastDot < token.Length - 1)
                token = token.Substring(lastDot + 1);

            token = token.Replace("_", string.Empty)
                         .Replace("-", string.Empty)
                         .Replace(" ", string.Empty)
                         .ToLowerInvariant();

            switch (token)
            {
                case "admin":
                    return "Admin";
                case "home":
                case "folder":
                case "folderhome":
                case "folderexplorer":
                    return "Home";
                case "sharedialog":
                case "share":
                    return "ShareDialog";
                case "sharedwithme":
                case "sharedwithmeform":
                    return "SharedWithMe";
                case "sharemanagement":
                case "sharemanagementform":
                case "manageshare":
                    return "ShareManagement";
                case "documentgroup":
                    return "DocumentGroup";
                case "orggroup":
                    return "OrgGroup";
                case "permissiongroup":
                    return "PermissionGroup";
                case "reportgroup":
                    return "ReportGroup";
                case "systemgroup":
                    return "SystemGroup";
                case "orgstructure":
                case "orgstructureform":
                    return "OrgStructure";
                case "usermanagement":
                case "usermanagementform":
                    return "UserManagement";
                case "useraccess":
                case "useraccessform":
                    return "UserAccess";
                case "permissionadmin":
                case "permissionform":
                    return "PermissionAdmin";
                case "foldersettings":
                    return "FolderSettings";
                case "userpermissionsummary":
                case "userpermissionsummaryform":
                    return "UserPermissionSummary";
                case "approvalinbox":
                case "approvalinboxform":
                    return "ApprovalInbox";
                case "reportexcel":
                    return "ReportExcel";
                case "pdpaconsent":
                    return "PdpaConsent";
                case "pdpareport":
                    return "PdpaReport";
                case "accesslog":
                case "accesslogview":
                    return "AccessLog";
                case "auditlog":
                case "auditlogform":
                    return "AuditLog";
                case "menumanagement":
                    return "MenuManagement";
                case "configadmin":
                    return "ConfigAdmin";
                case "authorityconfig":
                case "authorityconfigform":
                    return "AuthorityConfig";
                case "foldertemplate":
                case "foldertemplateform":
                    return "FolderTemplate";
                case "archivefolder":
                    return "ArchiveFolder";
                case "copystructure":
                    return "CopyStructure";
                case "changelevel":
                    return "ChangeLevel";
                case "syncemployee":
                    return "SyncEmployee";
                case "knowledgebaseform":
                case "kbmainform":
                    return "KBMainForm";
                case "knowledgebase":
                case "kms":
                    return "KBMainForm";
                case "kbcategoryform":
                case "kbcategory":
                    return "KBCategoryForm";
                case "workflowdashboard":
                case "workflowdashboardform":
                case "workflowdashboardformnew":
                case "workflow":
                case "wke":
                    return "WorkflowDashboard";
                case "workflowdetail":
                case "workflowdetailform":
                case "workflowdetailformnew":
                    return "WorkflowDetail";
                case "workflowtemplatelist":
                case "workflowtemplatelistform":
                    return "WorkflowTemplateList";
                case "workflowdesigner":
                case "workflowdesignerform":
                    return "WorkflowDesigner";
                case "workflowdelegation":
                case "workflowdelegationform":
                    return "WorkflowDelegation";
                case "slareport":
                case "slareportform":
                    return "SLAReport";
                case "workflowmonitor":
                case "workflowmonitorform":
                    return "WorkflowMonitor";
                case "recyclebin":
                case "recyclebinform":
                case "trash":
                    return "RecycleBin";
                default:
                    // Catalog already resolved the key — trust it for any action not in this switch
                    return resolvedAction;
            }
        }

        private static string MapMenuNameToActionTag(string menuName)
        {
            if (string.IsNullOrWhiteSpace(menuName))
                return null;

            string name = menuName.Trim().ToLowerInvariant();

            if (name.Contains("หน้าหลัก") || name.Contains("home")) return "Home";
            if (name.Contains("แชร์กับฉัน") || name.Contains("shared with me")) return "SharedWithMe";
            if (name.Contains("ลิงก์แชร์") || name.Contains("share management") || name.Contains("manage share")) return "ShareManagement";
            if (name == "share" || name.Contains("แชร์")) return "ShareDialog";
            if (name.Contains("โครงสร้าง") || name.Contains("org structure")) return "OrgStructure";
            if (name.Contains("บัญชีผู้ใช้") || name.Contains("ผู้ใช้งาน") || name.Contains("user management")) return "UserManagement";
            if (name.Contains("จัดการสิทธิ์") || name.Contains("permission")) return "PermissionAdmin";
            if (name.Contains("สรุปสิทธิ์")) return "UserPermissionSummary";
            if (name.Contains("อนุมัติ") || name.Contains("approval")) return "ApprovalInbox";
            if (name.Contains("เข้าถึง") || name.Contains("access log")) return "AccessLog";
            if (name.Contains("audit")) return "AuditLog";
            if (name.Contains("ตั้งค่าระบบ") || name.Contains("config")) return "ConfigAdmin";
            if (name.Contains("template") || name.Contains("แม่แบบ")) return "FolderTemplate";
            if (name.Contains("sync") || name.Contains("พนักงาน")) return "SyncEmployee";
            if (name.Contains("ถังขยะ") || name.Contains("recycle") || name.Contains("trash")) return "RecycleBin";
            if (name.Contains("delegation") || name.Contains("มอบหมาย")) return "WorkflowDelegation";

            return null;
        }

        private static string ResolveMenuActionTag(MenuItem menu)
        {
            if (menu == null)
                return null;

            string action = MapConfiguredActionToTag(menu.FormClassName);
            if (!string.IsNullOrEmpty(action))
                return action;

            action = MapMenuKeyToActionTag(menu.MenuKey);
            if (!string.IsNullOrEmpty(action))
                return action;

            return MapMenuNameToActionTag(menu.MenuName);
        }

        private static string MapMenuKeyToActionTag(string menuKey)
        {
            if (string.IsNullOrWhiteSpace(menuKey))
                return null;

            switch (menuKey.Trim().ToLowerInvariant())
            {
                case "home":
                case "folder":
                case "folder_home":
                case "folder_explorer":
                    return "Home";
                case "share_dialog":
                case "sharedialog":
                case "share":
                    return "ShareDialog";
                case "shared_with_me":
                case "sharedwithme":
                    return "SharedWithMe";
                case "share_management":
                case "sharemanagement":
                case "manage_share":
                case "manageshare":
                    return "ShareManagement";
                case "user_access":
                case "useraccess":
                    return "UserAccess";
                case "org_structure":
                case "orgstructure":
                    return "OrgStructure";
                case "user_management":
                case "usermanagement":
                    return "UserManagement";
                case "permission_admin":
                case "permissionadmin":
                    return "PermissionAdmin";
                case "folder_settings":
                case "foldersettings":
                    return "FolderSettings";
                case "user_permission_summary":
                case "userpermissionsummary":
                    return "UserPermissionSummary";
                case "approval_inbox":
                case "approvalinbox":
                    return "ApprovalInbox";
                case "report_excel":
                case "reportexcel":
                    return "ReportExcel";
                case "pdpa_consent":
                case "pdpaconsent":
                    return "PdpaConsent";
                case "pdpa_report":
                case "pdpareport":
                    return "PdpaReport";
                case "access_log":
                case "accesslog":
                    return "AccessLog";
                case "audit_log":
                case "auditlog":
                    return "AuditLog";
                case "menu_management":
                case "menumanagement":
                    return "MenuManagement";
                case "config_admin":
                case "configadmin":
                    return "ConfigAdmin";
                case "authority_config":
                case "authorityconfig":
                    return "AuthorityConfig";
                case "folder_template":
                case "foldertemplate":
                    return "FolderTemplate";
                case "archive_folder":
                case "archivefolder":
                    return "ArchiveFolder";
                case "copy_structure":
                case "copystructure":
                    return "CopyStructure";
                case "change_level":
                case "changelevel":
                    return "ChangeLevel";
                case "sync_employee":
                case "syncemployee":
                    return "SyncEmployee";
                case "knowledge_base":
                case "knowledgebase":
                case "knowledgebaseform":
                case "kbmainform":
                case "kms":
                    return "KBMainForm";
                case "kb_category":
                case "kbcategory":
                    return "KBCategoryForm";
                case "recycle_bin":
                case "recyclebin":
                case "trash":
                    return "RecycleBin";
                case "workflow_dashboard":
                case "workflowdashboard":
                    return "WorkflowDashboard";
                case "workflow_detail":
                case "workflowdetail":
                    return "WorkflowDetail";
                case "workflow_template_list":
                case "workflowtemplatelist":
                    return "WorkflowTemplateList";
                case "workflow_designer":
                case "workflowdesigner":
                    return "WorkflowDesigner";
                case "workflow_delegation":
                case "workflowdelegation":
                case "delegation":
                    return "WorkflowDelegation";
                case "sla_report":
                case "slareport":
                    return "SLAReport";
                case "workflow_monitor":
                case "workflowmonitor":
                    return "WorkflowMonitor";
                default:
                    // Fall through to Catalog for any key not explicitly mapped here
                    return MenuActionKeyCatalog.ResolveConfiguredActionKey(menuKey);
            }
        }

        private void LoadDefaultMainMenuTree()
        {

            // 1. จัดการเอกสาร
            var docNode = new TreeNode("📂 จัดการเอกสาร");
            docNode.Nodes.Add(new TreeNode("🏠 หน้าหลัก/โฟลเดอร์") { Tag = "Home" });
            docNode.Nodes.Add(new TreeNode("📄 ไฟล์ที่แชร์กับฉัน") { Tag = "SharedWithMe" });
            docNode.Nodes.Add(new TreeNode("🔗 จัดการลิงก์แชร์") { Tag = "ShareManagement" });
            docNode.Nodes.Add(new TreeNode("🗑️ ถังขยะ") { Tag = "RecycleBin" });
            _treeMainMenu.Nodes.Add(docNode);

            // 2. บริหารองค์กร
            var orgNode = new TreeNode("🏢 บริหารองค์กร");
            orgNode.Nodes.Add(new TreeNode("🌳 ผังโครงสร้างองค์กร") { Tag = "OrgStructure" });
            orgNode.Nodes.Add(new TreeNode("👥 จัดการผู้ใช้งาน") { Tag = "UserManagement" });
            orgNode.Nodes.Add(new TreeNode("🔄 Sync ข้อมูลพนักงาน") { Tag = "SyncEmployee" }); // เมนูใหม่
            orgNode.Nodes.Add(new TreeNode("🛡️ จัดการสิทธิ์การใช้งาน") { Tag = "PermissionAdmin" });
            orgNode.Nodes.Add(new TreeNode("👤 สรุปสิทธิ์รายบุคคล") { Tag = "UserPermissionSummary" });
            _treeMainMenu.Nodes.Add(orgNode);

            // 3. ตรวจสอบระบบ
            var auditNode = new TreeNode("🔍 ตรวจสอบระบบ");
            auditNode.Nodes.Add(new TreeNode("📜 ประวัติการเข้าถึง (Log)") { Tag = "AccessLog" });
            auditNode.Nodes.Add(new TreeNode("📥 รายการรออนุมัติ (Inbox)") { Tag = "ApprovalInbox" });
            _treeMainMenu.Nodes.Add(auditNode);

            // 4. ตั้งค่า
            var settingNode = new TreeNode("⚙️ ตั้งค่า");
            settingNode.Nodes.Add(new TreeNode("🛠️ ตั้งค่าระบบ") { Tag = "ConfigAdmin" });
            settingNode.Nodes.Add(new TreeNode("📋 ตั้งค่าแม่แบบโฟลเดอร์") { Tag = "FolderTemplate" });
            _treeMainMenu.Nodes.Add(settingNode);
        }

        private void MainMenuTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // PlusMinus (+/-) click: let TreeView handle expand/collapse natively, never execute action
            var hit = _treeMainMenu.HitTest(e.Location);
            if ((hit.Location & TreeViewHitTestLocations.PlusMinus) == TreeViewHitTestLocations.PlusMinus)
                return;

            string tag = MapConfiguredActionToTag(e.Node.Tag as string);

            // Try to map MenuItem if Tag is not a direct action string.
            if (string.IsNullOrEmpty(tag) && e.Node.Tag is MenuItem menuItem)
                tag = ResolveMenuActionTag(menuItem);

            if (string.IsNullOrWhiteSpace(tag))
            {
                // Group node — expand/collapse only when clicking the label text or icon
                if (hit.Location == TreeViewHitTestLocations.Label ||
                    hit.Location == TreeViewHitTestLocations.Image ||
                    hit.Location == TreeViewHitTestLocations.StateImage)
                {
                    if (e.Node.IsExpanded)
                        e.Node.Collapse();
                    else
                        e.Node.Expand();
                }
                return;
            }

            // Show selected menu label as feedback in the status bar
            lblOrgUnitStatus.Text = "☰ " + e.Node.Text;

            string errorMessage;
            MenuActionRegistry.TryExecute(tag, BuildMenuActionContext(e.Node.Text), out errorMessage);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                MessageBox.Show("ไม่สามารถเปิดหน้าจอ " + e.Node.Text + " ได้ในขณะนี้\nเนื่องจาก: " + errorMessage,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Refresh menu tree after every action to pick up latest data
            LoadMainMenuTree();
        }

        private MenuActionContext BuildMenuActionContext(string menuText)
        {
            return new MenuActionContext
            {
                Owner = this,
                User = _user,
                MenuText = menuText,
                SelectedFolder = (contentsListView.SelectedItems.Count > 0
                    ? contentsListView.SelectedItems[0].Tag as Folder
                    : null) ?? SelectedFolder,
                SelectedFile = SelectedFile,
                SelectedFiles = GetSelectedFilesFromContents(),
                FolderService = null,
                OrgStructureService = null,
                OrgLookupController = _orgLookupController,
                ApprovalService = null,
                FolderTemplateController = _folderTemplateController,
                PermissionService = null,
                FolderSettingsController = _folderSettingsController,
                FolderPermissionLookupController = _folderPermissionLookupController,
                AuditService = null,
                ShareCommandController = _shareCommandController,
                ShareManagementController = _shareManagementController,
                UserAccountLookupController = _userAccountLookupController,
                UserAdminController = _userAdminController,
                UserOrgVisibilityController = _userOrgVisibilityController,
                FolderReadLookupController = _folderReadLookupController,
                MenuManagementController = _menuManagementController,
                ApprovalInboxController = _approvalInboxController,
                RecycleBinController = _recycleBinController,
                ShareLookupController = _shareLookupController,
                EmployeeApiService = _employeeApiService,
                SwitchToOrgView = SwitchToOrgView,
                SwitchToMainMenuView = SwitchToMainMenuView,
                TriggerArchive = TriggerArchive,
                TriggerChangeLevel = TriggerChangeLevel,
                TriggerCopyStructure = TriggerCopyStructure,
                ClearContentPanel = () => ClearContentPanel(),
                SetContentInfo = text => lblContentInfo.Text = "กำลังเปิด: " + text,
                RequestMdiAction = OnMenuActionRequested
            };
        }

        /// <summary>
        /// Called by MDI_Container left nav to dispatch a menu action into this form's context.
        /// Uses the full MenuItem object so FormClassName/MenuKey/MenuName all participate in resolution.
        /// </summary>
        public void DispatchNavAction(CMS.Domain.Models.MenuItem menuItem)
        {
            if (menuItem == null) return;

            string tag = ResolveMenuActionTag(menuItem);
            if (string.IsNullOrWhiteSpace(tag)) return;

            string errorMessage;
            MenuActionRegistry.TryExecute(tag, BuildMenuActionContext(menuItem.MenuName ?? string.Empty), out errorMessage);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                MessageBox.Show("ไม่สามารถเปิดหน้าจอ " + menuItem.MenuName + " ได้\nเนื่องจาก: " + errorMessage,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Fallback overload for callers that only have a raw action key string (no MenuItem object).
        /// </summary>
        public void DispatchNavAction(string actionKey, string menuText = "")
        {
            if (string.IsNullOrWhiteSpace(actionKey)) return;

            string tag = MapConfiguredActionToTag(actionKey);
            if (string.IsNullOrWhiteSpace(tag)) tag = actionKey.Trim();

            string errorMessage;
            MenuActionRegistry.TryExecute(tag, BuildMenuActionContext(menuText), out errorMessage);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                MessageBox.Show("ไม่สามารถเปิดหน้าจอ " + menuText + " ได้\nเนื่องจาก: " + errorMessage,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void SetFolderExplorerControlsVisibility(bool isVisible)
        {
            lblOrgUnitStatus.Visible = isVisible;
            txtFolderSearch.Visible = isVisible;
            folderTreeView.Visible = isVisible;
            _folderExplorerControlsVisible = isVisible;
        }

        private void ToggleFolderExplorerControlsVisibility()
        {
            SetFolderExplorerControlsVisibility(!_folderExplorerControlsVisible);
        }

        private void SetMainMenuTreeFullHeight(bool isFullHeight)
        {
            if (isFullHeight)
            {
                pnlOrgFilter.Dock = DockStyle.Fill;
                return;
            }

            pnlOrgFilter.Dock = DockStyle.Top;
            if (_orgFilterDefaultHeight > 0)
                pnlOrgFilter.Height = _orgFilterDefaultHeight;
        }

        private void SwitchToOrgView()
        {
            if (!_showingConfView && !_showingMainMenuView) return;
            _showingConfView = false;
            _showingMainMenuView = false;
            SetFolderExplorerControlsVisibility(true);
            SetMainMenuTreeFullHeight(false);

            treeOrgUnit.Visible = true;
            _treeConfidentiality.Visible = false;
            _treeMainMenu.Visible = false;

            _btnOrgView.BackColor = Color.FromArgb(31, 56, 100);
            _btnOrgView.ForeColor = Color.White;
            _btnConfView.BackColor = Color.FromArgb(240, 244, 250);
            _btnConfView.ForeColor = Color.FromArgb(31, 56, 100);
            _btnMainMenu.BackColor = Color.FromArgb(240, 244, 250);
            _btnMainMenu.ForeColor = Color.FromArgb(31, 56, 100);

            contentsListView.ShowGroups = false;

            // Re-load folder tree if org unit is selected
            if (treeOrgUnit.SelectedNode != null && treeOrgUnit.SelectedNode.Tag is OrgUnit ou)
            {
                _isNavigating = true;
                try { _presenter.LoadFolderTree(ou.OrgUnitID); }
                finally { _isNavigating = false; }
            }
        }

        private void SwitchToConfView()
        {
            if (_showingConfView) return;
            _showingConfView = true;
            _showingMainMenuView = false;
            SetFolderExplorerControlsVisibility(true);
            SetMainMenuTreeFullHeight(false);

            treeOrgUnit.Visible = false;
            _treeConfidentiality.Visible = true;
            _treeMainMenu.Visible = false;

            _btnOrgView.BackColor = Color.FromArgb(240, 244, 250);
            _btnOrgView.ForeColor = Color.FromArgb(31, 56, 100);
            _btnConfView.BackColor = Color.FromArgb(31, 56, 100);
            _btnConfView.ForeColor = Color.White;
            _btnMainMenu.BackColor = Color.FromArgb(240, 244, 250);
            _btnMainMenu.ForeColor = Color.FromArgb(31, 56, 100);

            BuildConfidentialityTreeFromAllFoldersAsync();
        }

        private void SwitchToMainMenuView()
        {
            if (_showingMainMenuView)
            {
                // Always keep folder explorer hidden and refresh menu data
                SetFolderExplorerControlsVisibility(false);
                LoadMainMenuTree();
                return;
            }

            _showingConfView = false;
            _showingMainMenuView = true;
            SetFolderExplorerControlsVisibility(false);
            SetMainMenuTreeFullHeight(true);

            LoadMainMenuTree();

            treeOrgUnit.Visible = false;
            _treeConfidentiality.Visible = false;
            _treeMainMenu.Visible = true;

            _btnOrgView.BackColor = Color.FromArgb(240, 244, 250);
            _btnOrgView.ForeColor = Color.FromArgb(31, 56, 100);
            _btnConfView.BackColor = Color.FromArgb(240, 244, 250);
            _btnConfView.ForeColor = Color.FromArgb(31, 56, 100);
            _btnMainMenu.BackColor = Color.FromArgb(31, 56, 100);
            _btnMainMenu.ForeColor = Color.White;

            ClearContentPanel();
            lblOrgUnitStatus.Text = "☰ เมนูหลัก";
            ApplyBreadcrumb("หน้าแรก", new List<int?>());
        }

        private async void BuildConfidentialityTreeFromAllFoldersAsync()
        {
            // บันทึก expanded state และ selected folder ก่อน rebuild
            var expandedTexts = new HashSet<string>();
            var selectedFolderId = -1;
            CollectConfExpandedTexts(_treeConfidentiality.Nodes, expandedTexts);
            var selNode = _treeConfidentiality.SelectedNode;
            if (selNode != null)
            {
                var selFolder = selNode.Tag as Folder;
                if (selFolder != null) selectedFolderId = selFolder.FolderId;
            }

            _treeConfidentiality.BeginUpdate();
            _treeConfidentiality.Nodes.Clear();
            lblOrgUnitStatus.Text = "⏳ กำลังโหลด ตามความลับ...";
            this.Cursor = Cursors.WaitCursor;

            // Admin/Manager (privileged) เห็นทุก Folder ในทุก OrgUnit — ไม่ต้อง filter scope
            bool isPrivilegedUser = _user != null && _user.AppRole >= 3;

            // Capture OrgUnit selection on UI thread to avoid cross-thread access in Task.Run
            int? selectedOrgUnitId = isPrivilegedUser ? (int?)null : _selectedOrgUnitId;
            if (!isPrivilegedUser && !selectedOrgUnitId.HasValue && treeOrgUnit != null)
            {
                var selectedNode = treeOrgUnit.SelectedNode;
                var selectedUnit = selectedNode != null ? selectedNode.Tag as OrgUnit : null;
                if (selectedUnit != null)
                    selectedOrgUnitId = selectedUnit.OrgUnitID;

                // First entry fallback: if user hasn't selected org yet, pick first OrgUnit node.
                if (!selectedOrgUnitId.HasValue)
                {
                    var firstOrgNode = FindFirstOrgUnitNode(treeOrgUnit.Nodes);
                    var firstOrg = firstOrgNode != null ? firstOrgNode.Tag as OrgUnit : null;
                    if (firstOrg != null)
                        selectedOrgUnitId = firstOrg.OrgUnitID;
                }
            }

            try
            {
                // ดึงข้อมูลทั้งหมดแบบ async เพื่อไม่ block UI
                var result = await System.Threading.Tasks.Task.Run(() =>
                {
                    var allOrgUnits = new List<OrgUnit>();
                    if (_orgLookupController != null)
                        allOrgUnits = _orgLookupController.GetHierarchy()
                            .Where(u => u.IsActive && !u.IsDeleted)
                            .ToList();

                    // Process 1: load raw folders
                    var rawAllFolders = (_folderReadLookupController != null
                        ? _folderReadLookupController.GetAllFolders(null, _user)
                        : Enumerable.Empty<Folder>())
                        .Where(f => !f.IsDeleted && !f.IsArchived)
                        .ToList();

                    var allFolders = rawAllFolders;

                    // Process 2: apply OrgUnit scope (selected + descendants + ancestors when available)
                    if (selectedOrgUnitId.HasValue)
                    {
                        Func<int, IEnumerable<int>> getSubtreeIds = null;
                        getSubtreeIds = (parentId) =>
                        {
                            var ids = new List<int> { parentId };
                            foreach (var child in allOrgUnits.Where(u => u.ParentOrgUnitID == parentId))
                                ids.AddRange(getSubtreeIds(child.OrgUnitID));
                            return ids;
                        };

                        var allowedOrgIds = new HashSet<int>(getSubtreeIds(selectedOrgUnitId.Value));
                        var byId = allOrgUnits.ToDictionary(u => u.OrgUnitID);
                        int currentId = selectedOrgUnitId.Value;
                        var guard = new HashSet<int>();
                        while (currentId > 0 && guard.Add(currentId) && byId.ContainsKey(currentId))
                        {
                            allowedOrgIds.Add(currentId);
                            var parentId = byId[currentId].ParentOrgUnitID;
                            currentId = parentId.HasValue ? parentId.Value : 0;
                        }

                        allFolders = rawAllFolders.Where(f => allowedOrgIds.Contains(f.OrgUnitID)).ToList();

                        // Fallback: if hierarchy data is incomplete, at least keep directly selected OrgUnit folders
                        if (allFolders.Count == 0)
                            allFolders = rawAllFolders.Where(f => f.OrgUnitID == selectedOrgUnitId.Value).ToList();

                        // Process 3: add folder ancestors so visible descendants are not orphaned
                        var folderById = rawAllFolders.ToDictionary(f => f.FolderId);
                        var includeFolderIds = new HashSet<int>(allFolders.Select(f => f.FolderId));
                        var pending = new Queue<int>(includeFolderIds);
                        while (pending.Count > 0)
                        {
                            int id = pending.Dequeue();
                            Folder node;
                            if (!folderById.TryGetValue(id, out node))
                                continue;

                            if (node.ParentFolderID.HasValue && node.ParentFolderID.Value > 0)
                            {
                                int parentId = node.ParentFolderID.Value;
                                if (includeFolderIds.Add(parentId))
                                    pending.Enqueue(parentId);
                            }
                        }

                        allFolders = rawAllFolders.Where(f => includeFolderIds.Contains(f.FolderId)).ToList();

                        // Keep only org units that appear in composed folder set
                        var usedOrgIds = new HashSet<int>(allFolders.Select(f => f.OrgUnitID));
                        allOrgUnits = allOrgUnits.Where(u => usedOrgIds.Contains(u.OrgUnitID)).ToList();
                    }

                    var childrenByParent = allFolders
                        .Where(f => f.ParentFolderID.HasValue && f.ParentFolderID.Value > 0)
                        .GroupBy(f => f.ParentFolderID.Value)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    var allRootFolders = allFolders
                        .Where(f => !f.ParentFolderID.HasValue || f.ParentFolderID.Value <= 0)
                        .ToList();

                    return new
                    {
                        AllFolders = allFolders,
                        ChildrenByParent = childrenByParent,
                        AllRootFolders = allRootFolders,
                        AllOrgUnits = allOrgUnits
                    };
                });

                BuildConfidentialityTreeFromAllFolders(result.AllRootFolders, result.AllOrgUnits, result.ChildrenByParent);

                // Restore expanded state ที่เคย expand ไว้
                if (expandedTexts.Count > 0)
                    RestoreConfExpandedTexts(_treeConfidentiality.Nodes, expandedTexts);

                // Restore selected folder (ถ้ามี)
                if (selectedFolderId > 0)
                    RestoreConfSelectedFolder(_treeConfidentiality.Nodes, selectedFolderId);

                lblOrgUnitStatus.Text = _treeConfidentiality.Nodes.Count > 0
                    ? "✓ โหลดเสร็จ"
                    : "ℹ ไม่พบรายการที่มีสิทธิ์";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("BuildConfidentialityTreeFromAllFoldersAsync error: " + ex.Message);
                lblOrgUnitStatus.Text = "✗ เกิดข้อผิดพลาด";
            }
            finally
            {
                this.Cursor = Cursors.Default;
                _treeConfidentiality.EndUpdate();
            }
        }

        /// <summary>เก็บ text ของ node ที่ expand ไว้ใน conf tree</summary>
        private void CollectConfExpandedTexts(TreeNodeCollection nodes, HashSet<string> result)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.IsExpanded)
                {
                    result.Add(node.Text);
                    CollectConfExpandedTexts(node.Nodes, result);
                }
            }
        }

        /// <summary>Restore expanded state ใน conf tree โดยใช้ node text เป็น key</summary>
        private void RestoreConfExpandedTexts(TreeNodeCollection nodes, HashSet<string> expandedTexts)
        {
            foreach (TreeNode node in nodes)
            {
                if (expandedTexts.Contains(node.Text))
                {
                    node.Expand();
                    RestoreConfExpandedTexts(node.Nodes, expandedTexts);
                }
            }
        }

        /// <summary>Restore selected folder ใน conf tree โดย FolderId</summary>
        private void RestoreConfSelectedFolder(TreeNodeCollection nodes, int folderId)
        {
            foreach (TreeNode node in nodes)
            {
                var f = node.Tag as Folder;
                if (f != null && f.FolderId == folderId)
                {
                    _treeConfidentiality.SelectedNode = node;
                    node.EnsureVisible();
                    return;
                }
                if (node.Nodes.Count > 0)
                    RestoreConfSelectedFolder(node.Nodes, folderId);
            }
        }

        /// <summary>
        /// สร้าง "ตามความลับ" tree แสดงโครงสร้างองค์กร Sub → Div → Dep แบบ Windows Explorer
        /// จัดกลุ่มตาม 1HC/2SC/3NC/4PC แล้วแสดง SubCommittee → Division → Department → Folder
        /// </summary>
        private void BuildConfidentialityTreeFromAllFolders(List<Folder> allRootFolders, List<OrgUnit> allOrgUnits,
            Dictionary<int, List<Folder>> childrenByParent)
        {
            try
            {

                // Group folders by OrgUnitID for quick lookup
                var foldersByOrgUnit = allRootFolders
                    .GroupBy(f => f.OrgUnitID)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Helper: collect all OrgUnitIDs in a subtree (self + all descendants)
                Func<int, IEnumerable<int>> getSubtreeIds = null;
                getSubtreeIds = (parentId) =>
                {
                    var ids = new List<int> { parentId };
                    foreach (var child in allOrgUnits.Where(u => u.ParentOrgUnitID == parentId))
                        ids.AddRange(getSubtreeIds(child.OrgUnitID));
                    return ids;
                };

                // Helper: does this OrgUnit subtree have any folders at this conf level?
                Func<int, int, bool> subtreeHasFolders = (orgId, level) =>
                    getSubtreeIds(orgId).Any(id =>
                    {
                        List<Folder> fl;
                        return foldersByOrgUnit.TryGetValue(id, out fl) &&
                               fl.Any(f => f.ConfidentialityLevel == level);
                    });

                var groups = new[]
                {
                    new { Level = 1, Label = "1HC — High Confidential",   Color = Color.FromArgb(180, 0, 0),    BookKey = "lightning_bolt" },
                    new { Level = 2, Label = "2SC — Semi Confidential",   Color = Color.FromArgb(200, 80, 0),   BookKey = "book_orange"    },
                    new { Level = 3, Label = "3NC — Normal Confidential", Color = Color.FromArgb(0, 80, 160),   BookKey = "book_bule"      },
                    new { Level = 4, Label = "4PC — Public / เปิดเผย",   Color = Color.FromArgb(0, 130, 60),   BookKey = "book_green"     },
                };

                foreach (var group in groups)
                {
                    var foldersInGroup = allRootFolders.Where(f => f.ConfidentialityLevel == group.Level).ToList();
                    if (!foldersInGroup.Any()) continue;

                    int totalFiles = foldersInGroup.Sum(f => f.FileCount);
                    string header = totalFiles > 0
                        ? string.Format("{0}   {1} ไฟล์", group.Label, totalFiles)
                        : group.Label;

                    var groupNode = new TreeNode(header)
                    {
                        ForeColor = group.Color,
                        NodeFont = new Font(_treeConfidentiality.Font, FontStyle.Bold),
                        ImageKey = group.BookKey,
                        SelectedImageKey = group.BookKey,
                        Tag = null // group header — no Tag
                    };

                    // ── SubCommittee nodes ────────────────────────────────────────
                    var subCommittees = allOrgUnits
                        .Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.SubCommittee &&
                                    subtreeHasFolders(u.OrgUnitID, group.Level))
                        .OrderBy(u => u.SortOrder).ThenBy(u => u.Name)
                        .ToList();

                    if (subCommittees.Any())
                    {
                        foreach (var sc in subCommittees)
                        {
                            string scCode = string.IsNullOrEmpty(sc.OrgUnitCode) ? string.Empty
                                           : string.Format(" ({0})", sc.OrgUnitCode);
                            var scNode = new TreeNode(string.Format("♦ {0}{1}", sc.Name, scCode))
                            {
                                Tag = sc.OrgUnitID,
                                ForeColor = group.Color,
                                NodeFont = new Font(_treeConfidentiality.Font, FontStyle.Regular),
                                ImageKey = "folder",
                                SelectedImageKey = "open_folder"
                            };

                            // ── Division nodes ───────────────────────────────────
                            var divisions = allOrgUnits
                                .Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.Division &&
                                            u.ParentOrgUnitID == sc.OrgUnitID &&
                                            subtreeHasFolders(u.OrgUnitID, group.Level))
                                .OrderBy(u => u.SortOrder).ThenBy(u => u.Name)
                                .ToList();

                            foreach (var div in divisions)
                            {
                                string divCode = string.IsNullOrEmpty(div.OrgUnitCode) ? string.Empty
                                               : string.Format(" ({0})", div.OrgUnitCode);
                                var divNode = new TreeNode(string.Format("◇ {0}{1}", div.Name, divCode))
                                {
                                    Tag = div.OrgUnitID,
                                    ForeColor = Color.FromArgb(0, 80, 160),
                                    ImageKey = "folder",
                                    SelectedImageKey = "open_folder"
                                };

                                // ── Department nodes ─────────────────────────────
                                var departments = allOrgUnits
                                    .Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.Department &&
                                                u.ParentOrgUnitID == div.OrgUnitID &&
                                                subtreeHasFolders(u.OrgUnitID, group.Level))
                                    .OrderBy(u => u.SortOrder).ThenBy(u => u.Name)
                                    .ToList();

                                foreach (var dept in departments)
                                {
                                    string deptCode = string.IsNullOrEmpty(dept.OrgUnitCode) ? string.Empty
                                                    : string.Format(" ({0})", dept.OrgUnitCode);
                                    var deptNode = new TreeNode(dept.Name + deptCode)
                                    {
                                        Tag = dept.OrgUnitID,
                                        ForeColor = Color.FromArgb(60, 60, 60),
                                        ImageKey = "folder",
                                        SelectedImageKey = "open_folder"
                                    };
                                    BuildConfFolderNodes(deptNode, dept.OrgUnitID, group.Level, group.Color, foldersByOrgUnit, childrenByParent);
                                    if (deptNode.Nodes.Count > 0)
                                        divNode.Nodes.Add(deptNode);
                                }

                                // Folders owned directly by this Division (no Dept layer)
                                BuildConfFolderNodes(divNode, div.OrgUnitID, group.Level, group.Color, foldersByOrgUnit, childrenByParent);

                                if (divNode.Nodes.Count > 0)
                                    scNode.Nodes.Add(divNode);
                            }

                            // Folders owned directly by this SubCommittee
                            BuildConfFolderNodes(scNode, sc.OrgUnitID, group.Level, group.Color, foldersByOrgUnit, childrenByParent);

                            if (scNode.Nodes.Count > 0)
                                groupNode.Nodes.Add(scNode);
                        }
                    }
                    else
                    {
                        // Fallback: no org hierarchy available — flat OrgUnit grouping
                        var byOrgUnit = foldersInGroup.GroupBy(f => f.OrgUnitID).OrderBy(g => g.Key);
                        foreach (var orgGroup in byOrgUnit)
                        {
                            string ouName;
                            if (_orgUnitNameCache == null || !_orgUnitNameCache.TryGetValue(orgGroup.Key, out ouName))
                                ouName = string.Format("OrgUnit {0}", orgGroup.Key);

                            int ouFiles = orgGroup.Sum(f => f.FileCount);
                            string ouLabel = ouFiles > 0
                                ? string.Format("🏢 {0}   {1} ไฟล์", ouName, ouFiles)
                                : string.Format("🏢 {0}", ouName);

                            var ouNode = new TreeNode(ouLabel)
                            {
                                Tag = orgGroup.Key,
                                ForeColor = group.Color,
                                NodeFont = new Font(_treeConfidentiality.Font, FontStyle.Regular),
                                ImageKey = "folder",
                                SelectedImageKey = "open_folder"
                            };
                            foreach (var rootFolder in orgGroup)
                            {
                                int fc = rootFolder.FileCount;
                                string rlabel = fc > 0
                                    ? string.Format("{0}  [{1}]", rootFolder.FolderName, fc)
                                    : rootFolder.FolderName;

                                bool rootHasAccess = HasFolderViewAccessConfTree(rootFolder);
                                var rootNode = new TreeNode(rlabel)
                                {
                                    Tag = rootHasAccess ? (object)rootFolder : null,
                                    ForeColor = group.Color,
                                    ImageKey = "folder",
                                    SelectedImageKey = "open_folder"
                                };

                                bool hasVisibleChildren = AddConfChildNodes(rootNode, rootFolder.FolderId, group.Color, childrenByParent);
                                if (rootHasAccess || hasVisibleChildren)
                                    ouNode.Nodes.Add(rootNode);
                            }

                            if (ouNode.Nodes.Count > 0)
                                groupNode.Nodes.Add(ouNode);
                        }
                    }

                    if (groupNode.Nodes.Count > 0)
                        _treeConfidentiality.Nodes.Add(groupNode);
                }

                // ไม่ CollapseAll — ให้ BuildConfidentialityTreeFromAllFoldersAsync restore expanded state เองหลัง build
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("BuildConfidentialityTreeFromAllFolders error: " + ex.Message);
            }
        }

        /// <summary>
        /// เพิ่ม folder nodes (ของ OrgUnit นี้ที่มี conf level ตรง) เข้าไปใน parentNode
        /// </summary>
        private void BuildConfFolderNodes(TreeNode parentNode, int orgUnitId, int confLevel,
            Color color, Dictionary<int, List<Folder>> foldersByOrgUnit,
            Dictionary<int, List<Folder>> childrenByParent = null)
        {
            List<Folder> folders;
            if (!foldersByOrgUnit.TryGetValue(orgUnitId, out folders)) return;
            foreach (var f in folders.Where(f => f.ConfidentialityLevel == confLevel).OrderBy(f => f.FolderName))
            {
                bool hasViewAccess = HasFolderViewAccessConfTree(f);
                string lbl = f.FolderName;
                var fNode = new TreeNode(lbl)
                {
                    Tag = hasViewAccess ? (object)f : null,
                    ForeColor = color,
                    ImageKey = "folder",
                    SelectedImageKey = "open_folder"
                };

                bool hasVisibleChildren = AddConfChildNodes(fNode, f.FolderId, color, childrenByParent);
                if (hasViewAccess || hasVisibleChildren)
                    parentNode.Nodes.Add(fNode);
            }
        }

        private bool AddConfChildNodes(TreeNode parentNode, int folderId, Color color,
            Dictionary<int, List<Folder>> childrenByParent = null)
        {
            bool addedAny = false;
            try
            {
                List<Folder> children;

                if (childrenByParent != null)
                {
                    // Fast path: ใช้ in-memory lookup แทน DB query
                    if (!childrenByParent.TryGetValue(folderId, out children))
                        return false;
                }
                else
                {
                    if (_folderReadLookupController == null) return false;
                    children = _folderReadLookupController.GetChildren(folderId)
                        .Where(f => !f.IsDeleted && !f.IsArchived)
                        .ToList();
                }

                foreach (var child in children)
                {
                    bool hasViewAccess = HasFolderViewAccessConfTree(child);
                    string label = child.FolderName;
                    var childNode = new TreeNode(label)
                    {
                        Tag = hasViewAccess ? (object)child : null,
                        ForeColor = color
                    };

                    bool hasVisibleChildren = AddConfChildNodes(childNode, child.FolderId, color, childrenByParent);
                    if (!hasViewAccess && !hasVisibleChildren)
                        continue;

                    parentNode.Nodes.Add(childNode);
                    addedAny = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("AddConfChildNodes error: " + ex.Message);
            }

            return addedAny;
        }

        private string GetConfidentialityGroupLabel(Folder f)
        {
            if (IsAuto4Folder(f))
            {
                switch (f.ConfidentialityLevel)
                {
                    case 1: return "1HC — High Confidential";
                    case 2: return "2SC — Semi Confidential";
                    case 3: return "3NC — Normal Confidential";
                    case 4: return "4PC — Public / เปิดเผย";
                }
            }
            return string.IsNullOrEmpty(f.Name) ? ConfidentialityLabel(f.ConfidentialityLevel) : f.Name;
        }

        private int CountFilesInSubtree(TreeNode node)
        {
            int count = 0;
            var f = node.Tag as Folder;
            if (f != null) count += f.FileCount;
            foreach (TreeNode child in node.Nodes)
                count += CountFilesInSubtree(child);
            return count;
        }

        private void CloneTreeNodesForConf(TreeNodeCollection src, TreeNodeCollection dst)
        {
            foreach (TreeNode sn in src)
            {
                var folder = sn.Tag as Folder;
                string text = folder != null ? folder.Name : sn.Text;
                int fc = folder != null ? folder.FileCount : 0;
                if (fc > 0) text = string.Format("{0}   [{1}]", text, fc);

                var dn = new TreeNode(text)
                {
                    Tag = sn.Tag,
                    ForeColor = sn.ForeColor
                };
                CloneTreeNodesForConf(sn.Nodes, dn.Nodes);
                dst.Add(dn);
            }
        }

        private void ConfTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (e.Node == null) return;

            var confHit = _treeConfidentiality.HitTest(e.Location);
            if ((confHit.Location & TreeViewHitTestLocations.PlusMinus) == TreeViewHitTestLocations.PlusMinus)
                return;

            var folder = e.Node.Tag as Folder;

            if (folder != null)
            {
                // คลิก Folder node → แสดงเนื้อหาภายใน
                contentsListView.ShowGroups = false;
                if (!_isNavigating) PushNavigation(folder.FolderId);
                _lastSelectedFolder = folder;
                _currentNavFolderId = folder.FolderId;
                UpdateNavButtons();

                _presenter.GetFolderContents(folder.FolderId);

                ApplyBreadcrumb("🏠 PC  ›  " + folder.Name, new List<int?> { folder.FolderId });
                lblConfidentialityBadge.Text = ConfidentialityLabel(folder.ConfidentialityLevel);
                lblConfidentialityBadge.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
                lblContentInfo.Text = string.Format("กำลังดู: {0}", folder.Name);
                lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", folder.Name);
            }
            else if (e.Node.Tag is int)
            {
                int orgUnitId = (int)e.Node.Tag;

                // ตัด prefix ♦ / ◇ / 🏢 และ suffix (code) / filecount ออก
                string nodeName = e.Node.Text;
                if (nodeName.Length >= 2 && (nodeName.StartsWith("♦ ") || nodeName.StartsWith("◇ ")))
                    nodeName = nodeName.Substring(2).Trim();
                else if (nodeName.StartsWith("🏢 "))
                    nodeName = nodeName.Substring(2).Trim();
                int parenIdx = nodeName.IndexOf(" (");
                if (parenIdx > 0) nodeName = nodeName.Substring(0, parenIdx).Trim();
                int spaceIdx = nodeName.IndexOf("   ");
                if (spaceIdx > 0) nodeName = nodeName.Substring(0, spaceIdx).Trim();

                // โหลด Folder จาก DB → ซ้ายขวา sync กัน (เหมือนคลิก OrgUnit ใน org view)
                _selectedOrgUnitId = orgUnitId;
                _selectedOrgUnitName = nodeName;
                _selectedOrgUnit = null;
                contentsListView.ShowGroups = false;

                ApplyBreadcrumb("🏠 PC  ›  " + nodeName, new List<int?>());
                lblOrgUnitStatus.Text = string.Format("⏳ กำลังโหลด: {0}...", nodeName);

                // แจ้งว่ากำลังนำทาง เพื่อไม่ให้ ShowFolderTree ไป Rebuild Conf Tree อีกรอบ (ล้าง focus/selection)
                _isNavigating = true;
                try { _presenter.LoadFolderTree(orgUnitId); }
                finally { _isNavigating = false; }

                lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", nodeName);
                lblContentInfo.Text = "";
            }
            else
            {
                // คลิก Group node (1HC/2SC/3NC/4PC) → แสดง Folder แยก Group ตาม OrgUnit
                // ทำให้ฝั่งซ้าย (N OrgUnit) และฝั่งขวา (N กลุ่ม) ตรงกัน
                string nodeName = e.Node.Text;
                int spaceIdx = nodeName.IndexOf("   ");
                if (spaceIdx > 0) nodeName = nodeName.Substring(0, spaceIdx).Trim();

                contentsListView.BeginUpdate();
                contentsListView.Items.Clear();
                contentsListView.Groups.Clear();
                contentsListView.ShowGroups = true;

                int totalFolders = 0;
                foreach (TreeNode ouNode in e.Node.Nodes)
                {
                    // ชื่อ OrgUnit — ตัด prefix ♦/◇/🏢 และ suffix (code/count) ออก
                    string ouName = ouNode.Text;
                    if (ouName.Length >= 2 && (ouName.StartsWith("♦ ") || ouName.StartsWith("◇ ")))
                        ouName = ouName.Substring(2).Trim();
                    else if (ouName.StartsWith("🏢 "))
                        ouName = ouName.Substring(2).Trim();
                    int pi = ouName.IndexOf(" (");
                    if (pi > 0) ouName = ouName.Substring(0, pi).Trim();
                    int si = ouName.IndexOf("   ");
                    if (si > 0) ouName = ouName.Substring(0, si).Trim();

                    var grpFolders = new List<Folder>();
                    CollectFoldersFromConfNode(ouNode, grpFolders);
                    grpFolders = grpFolders.Where(f => !f.IsDeleted).ToList();
                    if (!grpFolders.Any()) continue;

                    var lvGroup = new ListViewGroup(ouName, HorizontalAlignment.Left);
                    contentsListView.Groups.Add(lvGroup);

                    foreach (var f in grpFolders)
                    {
                        bool isRoot = !f.ParentFolderID.HasValue || f.ParentFolderID == 0;
                        string iconKey = isRoot ? ConfidentialityBookIcon(f.ConfidentialityLevel) : "folder";
                        var item = new ListViewItem(f.Name) { ImageKey = iconKey, Group = lvGroup };
                        item.SubItems.Add(f.FileCount > 0
                            ? string.Format("Folder  {0} ไฟล์", f.FileCount)
                            : "Folder");
                        item.SubItems.Add(ConfidentialityShort(f.ConfidentialityLevel));
                        item.SubItems.Add(FormatBytes(f.FolderSizeBytes));
                        item.SubItems.Add(f.CreatedAt.ToString("dd/MM/yy HH:mm"));
                        item.SubItems.Add("—");
                        item.SubItems.Add("—");
                        item.SubItems.Add(ouName);
                        item.Tag = f;
                        item.ForeColor = ConfidentialityColor(f.ConfidentialityLevel);
                        item.UseItemStyleForSubItems = false;
                        item.SubItems[2].ForeColor = ConfidentialityColor(f.ConfidentialityLevel);
                        contentsListView.Items.Add(item);
                        totalFolders++;
                    }
                }

                contentsListView.EndUpdate();

                ApplyBreadcrumb("🏠 PC  ›  " + nodeName, new List<int?>());
                lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", nodeName);
                lblContentInfo.Text = string.Format("{0} Folder", totalFolders);

                // Adjust column widths to fit content
                AdjustNameColumnWidth();
            }
        }

        private void CollectFoldersFromConfNode(TreeNode node, List<Folder> result)
        {
            foreach (TreeNode child in node.Nodes)
            {
                var folder = child.Tag as Folder;
                if (folder != null)
                    result.Add(folder);
                else
                    CollectFoldersFromConfNode(child, result);
            }
        }

        // -----------------------------------------------------------------------
        // OrgUnit inline TreeView
        // -----------------------------------------------------------------------

        private async void LoadOrgUnitTree()
        {
            if (_orgLookupController == null) return;

            try
            {
                // ใช้ GetVisibleHierarchy เพื่อให้ explicit grants มีผล
                // ถ้า user มี explicit grants → คืนเฉพาะ granted set
                // ถ้าไม่มี → fallback role-based (เหมือนเดิม)
                var units = await System.Threading.Tasks.Task.Run(() =>
                    _orgLookupController.GetVisibleHierarchy(_user)
                        .Where(u => u.IsActive && !u.IsDeleted)
                        .ToList());

                // Populate OrgUnit name cache for use in ShowFolderContents
                _orgUnitNameCache = units.ToDictionary(
                    u => u.OrgUnitID,
                    u => u.OrgUnitNameTH ?? u.OrgUnitNameEN ?? u.OrgUnitCode);

                // visibleUnits คือผลลัพธ์จาก GetVisibleHierarchy แล้ว ไม่ต้องกรองซ้ำ
                var visibleUnits = units;

                treeOrgUnit.BeginUpdate();
                treeOrgUnit.Nodes.Clear();

                // "ทั้งหมด" root node — Admin เห็นทั้งหมด, role อื่นเห็นเฉพาะของตัวเอง
                var allNode = new System.Windows.Forms.TreeNode("— ทั้งหมด —")
                {
                    Tag = null,
                    NodeFont = new System.Drawing.Font(treeOrgUnit.Font, System.Drawing.FontStyle.Bold),
                    ForeColor = System.Drawing.Color.FromArgb(31, 56, 100),
                    ImageKey = "folder",
                    SelectedImageKey = "open_folder"
                };
                treeOrgUnit.Nodes.Add(allNode);

                foreach (var sc in visibleUnits.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.SubCommittee).OrderBy(u => u.Name))
                {
                    var scNode = new System.Windows.Forms.TreeNode(sc.Name)
                    {
                        Tag = sc,
                        ForeColor = System.Drawing.Color.FromArgb(31, 56, 100),
                        ImageKey = "organization",
                        SelectedImageKey = "organization"
                    };

                    foreach (var div in visibleUnits.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.Division && u.ParentId == sc.OrgUnitId).OrderBy(u => u.Name))
                    {
                        var divNode = new System.Windows.Forms.TreeNode(div.Name)
                        {
                            Tag = div,
                            ForeColor = System.Drawing.Color.FromArgb(0, 80, 160),
                            ImageKey = "folder",
                            SelectedImageKey = "open_folder"
                        };

                        foreach (var dept in visibleUnits.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.Department && u.ParentId == div.OrgUnitId).OrderBy(u => u.Name))
                        {
                            var deptNode = new System.Windows.Forms.TreeNode(dept.Name)
                            {
                                Tag = dept,
                                ForeColor = System.Drawing.Color.FromArgb(60, 60, 60),
                                ImageKey = "folder",
                                SelectedImageKey = "open_folder"
                            };
                            divNode.Nodes.Add(deptNode);
                        }
                        scNode.Nodes.Add(divNode);
                    }
                    treeOrgUnit.Nodes.Add(scNode);
                }

                treeOrgUnit.CollapseAll();
                treeOrgUnit.EndUpdate();

                // ไม่ auto-select อะไร ให้ user เลือกเอง
                // treeOrgUnit.SelectedNode = allNode;
            }
            catch (Exception ex)
            {
                ShowError("โหลดหน่วยงานไม่สำเร็จ: " + ex.Message);
            }
        }

        /// <summary>
        /// กรอง OrgUnit ที่ current user มีสิทธิ์มองเห็น:
        /// Admin(4)        → ทั้งหมด
        /// Manager(3)      → OrgUnit ของตัวเองและ sub-units ทั้งหมดใต้มัน
        /// Officer/Sr(1,2) → เฉพาะ OrgUnit ของตัวเองเท่านั้น
        /// </summary>
        private List<OrgUnit> FilterVisibleOrgUnits(List<OrgUnit> all)
        {
            if (_user == null || _user.AppRole >= 4)
                return all; // Admin เห็นทั้งหมด

            if (_user.OrgUnitID <= 0)
                return new List<OrgUnit>();

            var visibleIds = new HashSet<int> { _user.OrgUnitID };

            if (_user.AppRole >= 3) // Manager — เพิ่ม sub-units ทั้งหมด
            {
                var queue = new Queue<int>();
                queue.Enqueue(_user.OrgUnitID);
                while (queue.Count > 0)
                {
                    int parentId = queue.Dequeue();
                    foreach (var child in all.Where(o => o.ParentOrgUnitID == parentId))
                        if (visibleIds.Add(child.OrgUnitID))
                            queue.Enqueue(child.OrgUnitID);
                }
            }

            return all.Where(o => visibleIds.Contains(o.OrgUnitID)).ToList();
        }

        private void treeOrgUnit_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            LoadOrgUnitNode(e.Node);
            ApplyBreadcrumb(BuildBreadcrumbPath(e.Node, null), new List<int?>());
        }

        /// <summary>
        /// กรณีคลิก OrgUnit node ที่ selected อยู่แล้ว — AfterSelect จะไม่ fire ดังนั้นต้อง force reload
        /// </summary>
        private void treeOrgUnit_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            // ถ้า node ที่คลิกเป็น node ที่ selected อยู่แล้ว → AfterSelect จะไม่ fire → force reload
            if (treeOrgUnit.SelectedNode == e.Node)
            {
                LoadOrgUnitNode(e.Node);
                ApplyBreadcrumb(BuildBreadcrumbPath(e.Node, null), new List<int?>());
            }
        }

        /// <summary>
        /// โหลดข้อมูล Folder สำหรับ OrgUnit node ที่เลือก
        /// แสดง cursor รอและข้อความ "กำลังโหลด..." ระหว่างที่ระบบดึงข้อมูล
        /// </summary>
        private async void LoadOrgUnitNode(System.Windows.Forms.TreeNode node)
        {
            var unit = node != null ? node.Tag as OrgUnit : null;

            // "— ทั้งหมด —" node (tag=null) — ไม่ load รายการ ให้ user เลือก OrgUnit ก่อน
            if (unit == null) return;

            _selectedOrgUnitId = (int?)unit.OrgUnitId;
            _selectedOrgUnitName = unit.Name;
            _selectedOrgUnit = unit;
            _pendingHCApproval = false; // reset ทุกครั้งที่เลือก OrgUnit ใหม่

            // Show loading feedback immediately (visible because we yield to UI via await)
            lblOrgUnitStatus.Text = string.Format("⏳ กำลังโหลด: {0}...", _selectedOrgUnitName);
            this.Cursor = Cursors.WaitCursor;

            try
            {
                PushNavigation(null);
                _currentNavFolderId = null;
                UpdateNavButtons();

                folderTreeView.AfterSelect -= folderTreeView_AfterSelect;
                folderTreeView.SelectedNode = null;
                folderTreeView.AfterSelect += folderTreeView_AfterSelect;

                // Fetch ALL folders in one DB round-trip, off the UI thread
                var orgUnitId = _selectedOrgUnitId;
                var allFolders = await System.Threading.Tasks.Task.Run(() =>
                {
                    // ถ้าเลือก OrgUnit ให้ค้นหา Folder ของ OrgUnit นั้นและ parent OrgUnit
                    if (orgUnitId.HasValue)
                    {
                        // ค้นหา Folder ของ OrgUnit ที่เลือก
                        var folders = _folderReadLookupController.GetFoldersByOrgUnit(orgUnitId.Value, _user).ToList();
                        // ไม่ fallback ไป OrgUnit อื่น — ถ้าไม่มี folder ให้คืน list ว่าง
                        // เพื่อให้ PromptToCreateFirstFolder ทำงานถูกต้องสำหรับ OrgUnit นี้โดยเฉพาะ
                        return folders;
                    }
                    else
                    {
                        // ถ้าไม่เลือก OrgUnit ให้ค้นหา Folder ทั้งหมด
                        return _presenter.FetchAllFolders(null);
                    }
                });

                // Build tree and content panel on UI thread (no further DB calls)
                ShowFolderTree(allFolders);

                var roots = allFolders
                    .Where(f => !f.ParentFolderID.HasValue || f.ParentFolderID == 0)
                    .ToList();
                ShowFolderContents(roots, Enumerable.Empty<FileDocument>());
            }
            finally
            {
                this.Cursor = Cursors.Default;
                lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", _selectedOrgUnitName);
            }
        }

        /// <summary>
        /// ค้นหา node ใน treeOrgUnit ที่ตรงกับ orgUnitId แล้ว select
        /// การ select จะ trigger treeOrgUnit_AfterSelect → reload folder tree อัตโนมัติ
        /// </summary>
        private void SelectOrgUnitNode(int orgUnitId)
        {
            var node = FindOrgUnitNode(treeOrgUnit.Nodes, orgUnitId);
            if (node != null)
            {
                treeOrgUnit.SelectedNode = node;
                node.EnsureVisible();
            }
            else
            {
                // ไม่พบ node → reload ด้วย orgUnitId โดยตรง
                _selectedOrgUnitId = orgUnitId;
                _isNavigating = true;
                try { _presenter.LoadFolderTree(_selectedOrgUnitId); }
                finally { _isNavigating = false; }
            }
        }

        private System.Windows.Forms.TreeNode FindOrgUnitNode(System.Windows.Forms.TreeNodeCollection nodes, int orgUnitId)
        {
            foreach (System.Windows.Forms.TreeNode node in nodes)
            {
                var unit = node.Tag as OrgUnit;
                if (unit != null && unit.OrgUnitId == orgUnitId) return node;
                var found = FindOrgUnitNode(node.Nodes, orgUnitId);
                if (found != null) return found;
            }
            return null;
        }

        private System.Windows.Forms.TreeNode FindFirstOrgUnitNode(System.Windows.Forms.TreeNodeCollection nodes)
        {
            foreach (System.Windows.Forms.TreeNode node in nodes)
            {
                if (node.Tag is OrgUnit)
                    return node;

                var found = FindFirstOrgUnitNode(node.Nodes);
                if (found != null)
                    return found;
            }

            return null;
        }

        // -----------------------------------------------------------------------
        // Form load
        // -----------------------------------------------------------------------

        private void FolderForm_Load(object sender, EventArgs e)
        {
            ApplyPerformanceTweaks();
            this.WindowState = FormWindowState.Maximized;

            pnlContentHeader.Visible = true;
            pnlContentHeader.Height = 36;

            // Set up folder open/closed icons from Resources (shared ImageList)
            var folderImageList = new ImageList { ImageSize = new System.Drawing.Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };
            folderImageList.Images.Add("folder", Properties.Resources.folder);
            folderImageList.Images.Add("open_folder", Properties.Resources.open_folder);
            folderImageList.Images.Add("organization", Properties.Resources.organization);

            // Try to load lightning_bolt1, fallback to book_red if not available
            try
            {
                if (Properties.Resources.lightning_bolt1 != null)
                {
                    folderImageList.Images.Add("lightning_bolt", Properties.Resources.lightning_bolt1);
                }
                else
                {
                    folderImageList.Images.Add("lightning_bolt", Properties.Resources.book_red1.ToBitmap());
                }
            }
            catch
            {
                folderImageList.Images.Add("lightning_bolt", Properties.Resources.book_red1.ToBitmap());
            }

            folderImageList.Images.Add("book_red", Properties.Resources.lightning_bolt1);
            folderImageList.Images.Add("book_orange", Properties.Resources.book_orange);
            folderImageList.Images.Add("book_bule", Properties.Resources.book_bule);
            folderImageList.Images.Add("book_green", Properties.Resources.book_green);

            // TreeView — closed/open on expand/collapse
            folderTreeView.ImageList = folderImageList;
            folderTreeView.ImageKey = "folder";
            folderTreeView.SelectedImageKey = "open_folder";
            folderTreeView.BeforeExpand += (s, ev) =>
            {
                // Root folder ใช้ book icon คงที่ ไม่เปลี่ยนตาม expand/collapse
                var f = ev.Node.Tag as Folder;
                bool isRoot = f != null && (!f.ParentFolderID.HasValue || f.ParentFolderID == 0);
                if (!isRoot) ev.Node.ImageKey = ev.Node.SelectedImageKey = "open_folder";
            };
            folderTreeView.BeforeCollapse += (s, ev) =>
            {
                var f = ev.Node.Tag as Folder;
                bool isRoot = f != null && (!f.ParentFolderID.HasValue || f.ParentFolderID == 0);
                if (!isRoot) ev.Node.ImageKey = ev.Node.SelectedImageKey = "folder";
            };

            // OrgUnit TreeView — same icons
            treeOrgUnit.ImageList = folderImageList;
            treeOrgUnit.ImageKey = "folder";
            treeOrgUnit.SelectedImageKey = "open_folder";
            treeOrgUnit.BeforeExpand += (s, ev) => { if (ev.Node.ImageKey != "organization") ev.Node.ImageKey = ev.Node.SelectedImageKey = "open_folder"; };
            treeOrgUnit.BeforeCollapse += (s, ev) => { if (ev.Node.ImageKey != "organization") ev.Node.ImageKey = ev.Node.SelectedImageKey = "folder"; };

            // Confidentiality TreeView — same image list (WinForms requires a SEPARATE ImageList instance per TreeView)
            var confImageList = new ImageList { ImageSize = new System.Drawing.Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };
            confImageList.Images.Add("folder", Properties.Resources.folder);
            confImageList.Images.Add("open_folder", Properties.Resources.open_folder);
            confImageList.Images.Add("organization", Properties.Resources.organization);

            // Try to load lightning_bolt1, fallback to book_red if not available
            try
            {
                if (Properties.Resources.lightning_bolt1 != null)
                {
                    confImageList.Images.Add("lightning_bolt", Properties.Resources.lightning_bolt1);
                }
                else
                {
                    confImageList.Images.Add("lightning_bolt", Properties.Resources.book_red1.ToBitmap());
                }
            }
            catch
            {
                confImageList.Images.Add("lightning_bolt", Properties.Resources.book_red1.ToBitmap());
            }

            confImageList.Images.Add("book_red", Properties.Resources.lightning_bolt);
            confImageList.Images.Add("book_orange", Properties.Resources.book_orange);
            confImageList.Images.Add("book_bule", Properties.Resources.book_bule);
            confImageList.Images.Add("book_green", Properties.Resources.book_green1);
            _treeConfidentiality.ImageList = confImageList;
            _treeConfidentiality.ImageKey = "folder";
            _treeConfidentiality.SelectedImageKey = "open_folder";
            _treeConfidentiality.BeforeExpand += (s, ev) =>
            {
                // Level-header nodes (group) and OrgUnit nodes keep their icon; only folder nodes change
                if (ev.Node.Tag is Folder)
                    ev.Node.ImageKey = ev.Node.SelectedImageKey = "open_folder";
            };
            _treeConfidentiality.BeforeCollapse += (s, ev) =>
            {
                if (ev.Node.Tag is Folder)
                    ev.Node.ImageKey = ev.Node.SelectedImageKey = "folder";
            };

            // Main Menu TreeView — dedicated icon list for modern left-nav appearance
            var mainMenuImageList = new ImageList { ImageSize = new Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };
            mainMenuImageList.Images.Add("folder", Properties.Resources.folder);
            mainMenuImageList.Images.Add("open_folder", Properties.Resources.open_folder);
            mainMenuImageList.Images.Add("organization", Properties.Resources.organization);
            mainMenuImageList.Images.Add("file_docs", Properties.Resources.File_docs.ToBitmap());
            mainMenuImageList.Images.Add("book_red", Properties.Resources.book_red);
            mainMenuImageList.Images.Add("book_orange", Properties.Resources.book_orange);
            mainMenuImageList.Images.Add("book_blue", Properties.Resources.book_bule);
            mainMenuImageList.Images.Add("book_green", Properties.Resources.book_green);
            mainMenuImageList.Images.Add("cat_trash", MenuCategoryIcons.CreateTrashIcon());
            try
            {
                mainMenuImageList.Images.Add("lightning_bolt", Properties.Resources.lightning_bolt1);
            }
            catch
            {
                mainMenuImageList.Images.Add("lightning_bolt", Properties.Resources.book_red1.ToBitmap());
            }
            _treeMainMenu.ImageList = mainMenuImageList;
            _treeMainMenu.ImageKey = "folder";
            _treeMainMenu.SelectedImageKey = "open_folder";

            // ListView needs its own separate ImageList (WinForms does not allow sharing with TreeView)
            var listImageList = new ImageList { ImageSize = new System.Drawing.Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };
            listImageList.Images.Add("folder", Properties.Resources.folder);
            try { listImageList.Images.Add("lightning_bolt", Properties.Resources.lightning_bolt1); }
            catch { listImageList.Images.Add("lightning_bolt", Properties.Resources.book_red1.ToBitmap()); }
            listImageList.Images.Add("book_red", Properties.Resources.book_red1.ToBitmap());
            listImageList.Images.Add("book_orange", Properties.Resources.book_orange);
            listImageList.Images.Add("book_bule", Properties.Resources.book_bule);
            listImageList.Images.Add("book_green", Properties.Resources.book_green);
            listImageList.Images.Add("file_docs", Properties.Resources.File_docs.ToBitmap());
            contentsListView.SmallImageList = listImageList;

            // ── LargeImageList (48×48) for Large Icon view ────────────────────
            var largeImageList = new ImageList { ImageSize = new Size(48, 48), ColorDepth = ColorDepth.Depth32Bit };
            largeImageList.Images.Add("folder", new Bitmap(Properties.Resources.folder, 48, 48));
            try { largeImageList.Images.Add("lightning_bolt", new Bitmap(Properties.Resources.lightning_bolt1, 48, 48)); }
            catch { largeImageList.Images.Add("lightning_bolt", new Bitmap(Properties.Resources.book_red1.ToBitmap(), 48, 48)); }
            largeImageList.Images.Add("book_red", new Bitmap(Properties.Resources.lightning_bolt1, 48, 48));
            largeImageList.Images.Add("book_orange", new Bitmap(Properties.Resources.book_orange, 48, 48));
            largeImageList.Images.Add("book_bule", new Bitmap(Properties.Resources.book_bule, 48, 48));
            largeImageList.Images.Add("book_green", new Bitmap(Properties.Resources.book_green, 48, 48));
            largeImageList.Images.Add("file_docs", new Bitmap(Properties.Resources.File_docs.ToBitmap(), 48, 48));
            contentsListView.LargeImageList = largeImageList;

            // ── View toggle buttons (Details / Tile / Large Icons) ───────────
            Color activeColor = Color.FromArgb(31, 56, 100);
            Color inactiveColor = Color.FromArgb(220, 220, 220);
            Color activeFg = Color.White;
            Color inactiveFg = Color.FromArgb(60, 60, 60);

            var btnViewDetails = new Button
            {
                Name = "btnViewDetails",
                Text = "รายการ",
                Size = new Size(64, 26),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f),
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                BackColor = activeColor,
                ForeColor = activeFg,
                TabStop = false
            };
            btnViewDetails.FlatAppearance.BorderSize = 0;

            var btnViewLarge = new Button
            {
                Name = "btnViewLarge",
                Text = "ไอคอน",
                Size = new Size(62, 26),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f),
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                BackColor = inactiveColor,
                ForeColor = inactiveFg,
                TabStop = false
            };
            btnViewLarge.FlatAppearance.BorderSize = 0;

            var viewTip = new ToolTip();
            viewTip.SetToolTip(btnViewDetails, "แสดงแบบรายการ");
            viewTip.SetToolTip(btnViewLarge, "แสดงแบบไอคอน");

            // helper: reset all buttons then highlight active one
            Action<Button> setActive = (active) =>
            {
                foreach (var b in new[] { btnViewDetails, btnViewLarge })
                {
                    b.BackColor = (b == active) ? activeColor : inactiveColor;
                    b.ForeColor = (b == active) ? activeFg : inactiveFg;
                }
            };

            btnViewDetails.Click += (s, ev) =>
            {
                _currentView = View.Details;
                contentsListView.View = View.Details;
                contentsListView.OwnerDraw = true;
                setActive(btnViewDetails);
            };

            btnViewLarge.Click += (s, ev) =>
            {
                _currentView = View.LargeIcon;
                contentsListView.OwnerDraw = false;
                contentsListView.View = View.LargeIcon;
                setActive(btnViewLarge);
            };

            var btnToggleFilter = new Button
            {
                Name = "btnToggleFilter",
                Text = "🔍 ค้นหา",
                Size = new Size(88, 26),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 9f),
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                BackColor = inactiveColor,
                ForeColor = inactiveFg,
                TabStop = false
            };
            btnToggleFilter.FlatAppearance.BorderSize = 0;
            viewTip.SetToolTip(btnToggleFilter, "ค้นหาและกรองขั้นสูง: นามสกุล, วันที่, ผู้อัปโหลด");

            pnlContentHeader.Controls.Add(btnViewDetails);
            pnlContentHeader.Controls.Add(btnViewLarge);
            pnlContentHeader.Controls.Add(btnToggleFilter);

            // ─── UX: Filter toggle button ────────────────────────────────────
            // ซ่อน filter bar ไว้ก่อน — แสดงเฉพาะเมื่อ user ต้องการกรอง
            pnlFilter.Visible = false;

            btnToggleFilter.Click += (s, ev) =>
            {
                pnlFilter.Visible = !pnlFilter.Visible;
                if (pnlFilter.Visible)
                {
                    btnToggleFilter.BackColor = activeColor;
                    btnToggleFilter.ForeColor = activeFg;
                }
                else
                {
                    bool filterActive = lblFilterStatus.Visible;
                    btnToggleFilter.BackColor = filterActive ? Color.FromArgb(200, 220, 255) : inactiveColor;
                    btnToggleFilter.ForeColor = filterActive ? Color.FromArgb(43, 87, 154) : inactiveFg;
                }
            };

            // ─── UX: ซ่อน column ที่ไม่จำเป็น ────────────────────────────────
            // ผู้ใช้ทั่วไปต้องการ: ชื่อ, ประเภท, ขนาด, วันที่, โดย เท่านั้น
            colVersion.Width = 0;
            colOrgUnit.Width = 0;
            colPermissions.Width = 0;

            // Wire confidentiality level filter checkboxes
            chk1HC.CheckedChanged += new EventHandler(this.OnConfidentialityLevelChanged);
            chk2SC.CheckedChanged += new EventHandler(this.OnConfidentialityLevelChanged);
            chk3NC.CheckedChanged += new EventHandler(this.OnConfidentialityLevelChanged);
            chk4PC.CheckedChanged += new EventHandler(this.OnConfidentialityLevelChanged);

            // Force detail panel to be very narrow - prioritize list view
            splitContent.Panel2MinSize = 100;
            splitContent.SplitterDistance = this.Width - 110;

            // ไม่ load folder tree ทันที ให้รอ user เลือก OrgUnit ก่อน
            // _presenter.LoadFolderTree(_selectedOrgUnitId);

            // แสดงข้อความให้ user เลือก OrgUnit และ clear ทุกอย่าง
            folderTreeView.Nodes.Clear();
            ClearContentPanel();

            // เพิ่มข้อความแนะนำ
            var instructionNode = new TreeNode("กรุณาเลือกหน่วยงานจากด้านซ้าย")
            {
                ForeColor = Color.Gray
            };
            folderTreeView.Nodes.Add(instructionNode);

            // แสดงสถานะว่ายังไม่ได้เลือกหน่วยงาน
            lblOrgUnitStatus.Text = "📂 กรุณาเลือกหน่วยงาน";

            // _presenter.LoadRecycleBin(_selectedOrgUnitId); // Hidden - recycle bin disabled
        }

        private void ApplyPerformanceTweaks()
        {
            // Reduce redraw flicker while heavy controls are being populated.
            SetDoubleBuffered(folderTreeView);
            SetDoubleBuffered(treeOrgUnit);
            if (_treeConfidentiality != null)
                SetDoubleBuffered(_treeConfidentiality);
            SetDoubleBuffered(contentsListView);
            SetDoubleBuffered(splitContent);
            SetDoubleBuffered(splitMain);
        }

        private static void SetDoubleBuffered(Control control)
        {
            if (control == null)
                return;

            var prop = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (prop != null)
                prop.SetValue(control, true, null);
        }

        // -----------------------------------------------------------------------
        // IFolderView — ShowFolderTree  (Req 4.1, 7.6)
        // -----------------------------------------------------------------------

        public void ShowFolderTree(IEnumerable<Folder> rootFolders)
        {
            // Accept either a flat list of all folders OR roots-only list.
            // Build an in-memory parent→children map so no per-node DB calls are needed.
            var allFolders = rootFolders == null
                ? new List<Folder>()
                : rootFolders.Where(f => !f.IsDeleted && !f.IsArchived).ToList();

            // Group by parent: key 0 = root (ParentFolderID null or 0)
            var byParent = allFolders
                .GroupBy(f => f.ParentFolderID.HasValue && f.ParentFolderID.Value != 0
                    ? f.ParentFolderID.Value : 0)
                .ToDictionary(g => g.Key, g => g.OrderBy(f => f.FolderName).ToList());

            _treeRoots = byParent.ContainsKey(0) ? byParent[0] : new List<Folder>();

            // จำ folderId ที่เลือกอยู่ก่อน rebuild tree
            var previousFolder = SelectedFolder;
            int? previousFolderId = previousFolder != null ? (int?)previousFolder.FolderId : null;

            // จำ node ที่ expand ไว้ก่อน rebuild เพื่อคืน state กลับหลัง build
            var expandedFolderIds = new HashSet<int>();
            CollectExpandedNodes(folderTreeView.Nodes, expandedFolderIds);

            _activeTreeNode = null;
            folderTreeView.BeginUpdate();
            folderTreeView.Nodes.Clear();

            foreach (var folder in _treeRoots)
            {
                var node = CreateFolderNode(folder);
                folderTreeView.Nodes.Add(node);
                BuildChildNodes(node, folder.FolderID, byParent); // in-memory, no DB
            }

            if (_treeRoots.Count == 0)
            {
                var emptyNode = new TreeNode("(ไม่พบโฟลเดอร์)")
                {
                    ForeColor = Color.Gray
                };
                folderTreeView.Nodes.Add(emptyNode);

                // ถ้าเลือก OrgUnit แล้วไม่มีโฟลเดอร์ → ถามว่าต้องการสร้างโฟลเดอร์หรือไม่
                if (_selectedOrgUnitId.HasValue)
                {
                    PromptToCreateFirstFolder();
                }
            }

            // คืน expanded state เดิม
            // ถ้ามี expanded nodes → restore กลับ
            // ถ้าไม่มี (โหลดครั้งแรก) → auto-expand ถึง Level 2 เพื่อให้เห็น subfolders ทันที
            if (expandedFolderIds.Count > 0)
                RestoreExpandedNodes(folderTreeView.Nodes, expandedFolderIds);
            else
                ExpandTreeToLevel(folderTreeView.Nodes, 0, 2);

            folderTreeView.EndUpdate();

            // ถ้ามี folder ที่เลือกอยู่ก่อน → restore selection เดิม
            // ยกเว้นเมื่อ _isNavigating = true (เช่น หลัง upload) เพื่อไม่ให้ override content
            if (_treeRoots.Count == 0)
            {
                ClearContentPanel();
            }
            else if (previousFolderId.HasValue && !_isNavigating)
            {
                RestoreSelectedNode(folderTreeView.Nodes, previousFolderId.Value);
            }
            else if (!previousFolderId.HasValue)
            {
                folderTreeView.AfterSelect -= folderTreeView_AfterSelect;
                folderTreeView.SelectedNode = null;
                folderTreeView.AfterSelect += folderTreeView_AfterSelect;
                if (!_isNavigating) ClearContentPanel();
            }
            // _isNavigating = true → ไม่ทำอะไร ให้ caller จัดการ content เอง

            // ถ้ากำลังแสดง "ตามความลับ" → rebuild conf tree ด้วย (เฉพาะเมื่อไม่ใช่การ navigation ทั่วไป เพื่อไม่ให้ล้าง selection/focus)
            if (_showingConfView && _treeConfidentiality != null && !_isNavigating)
                BuildConfidentialityTreeFromAllFoldersAsync();
        }

        /// <summary>
        /// Recursively builds child nodes from an in-memory parent→children map.
        /// No database calls — all data already fetched in one shot.
        /// </summary>
        private void BuildChildNodes(TreeNode parentNode, int parentFolderId,
            Dictionary<int, List<Folder>> byParent)
        {
            List<Folder> children;
            if (!byParent.TryGetValue(parentFolderId, out children)) return;
            foreach (var child in children)
            {
                var childNode = CreateFolderNode(child);
                parentNode.Nodes.Add(childNode);
                BuildChildNodes(childNode, child.FolderID, byParent);
            }
        }

        // -----------------------------------------------------------------------
        // IFolderView — ShowFolderContents
        // -----------------------------------------------------------------------

        public void ShowFolderContents(IEnumerable<Folder> subFolders, IEnumerable<FileDocument> files)
        {
            // clear เฉพาะเมื่อไม่มี folder tree เลือก และไม่มีอะไรส่งมาแสดง
            bool hasContent = (subFolders != null && subFolders.Any())
                           || (files != null && files.Any());
            if (folderTreeView.SelectedNode == null && !hasContent)
            {
                ClearContentPanel();
                return;
            }

            // Reset quick-search when navigating to a new folder
            txtSearch.Text = "ค้นหาใน Folder...";
            txtSearch.ForeColor = Color.Gray;

            contentsListView.SuspendLayout();
            contentsListView.BeginUpdate();
            contentsListView.Items.Clear();
            _currentFiles.Clear();

            // Filter out deleted items
            var folderList = subFolders == null ? new List<Folder>() : subFolders.Where(f => !f.IsDeleted).ToList();
            var fileList = files == null ? new List<FileDocument>() : files.Where(f => !f.IsDeleted).ToList();

            // Use cached OrgUnit lookup (populated by LoadOrgUnitTree) — no DB call here
            Dictionary<int, string> orgUnitNames = _orgUnitNameCache ?? new Dictionary<int, string>();
            var actualFileCountByFolderId = new Dictionary<int, int>();

            // ── Sub-folders first ──────────────────────────────────────────
            foreach (var folder in folderList)
            {
                int actualFileCount = GetActualFileCount(folder, actualFileCountByFolderId);
                folder.FileCount = actualFileCount;

                // Populate OrgUnitName
                string ouName;
                folder.OrgUnitName = orgUnitNames.TryGetValue(folder.OrgUnitID, out ouName) ? ouName : "—";

                // Name column: name + subtitle hint (icon comes from ImageList)
                string nameText = folder.Name;
                // 4 Folder หลัก (root) ใช้ book icon ตามระดับความลับ; folder ลูกใช้ folder icon ทั่วไป
                bool isFolderRoot = !folder.ParentFolderID.HasValue || folder.ParentFolderID == 0;
                string iconKey = isFolderRoot
                    ? ConfidentialityBookIcon(folder.ConfidentialityLevel)
                    : "folder";
                var item = new ListViewItem(nameText) { ImageKey = iconKey };
                item.SubItems.Add(actualFileCount > 0
                    ? string.Format("Folder  {0} ไฟล์", actualFileCount)
                    : "Folder");                                          // ประเภท
                item.SubItems.Add(ConfidentialityShort(folder.ConfidentialityLevel)); // ระดับ
                item.SubItems.Add(FormatBytes(folder.FolderSizeBytes));              // ขนาด
                item.SubItems.Add(folder.CreatedAt.ToString("dd/MM/yy HH:mm")); // วันที่
                item.SubItems.Add(folder.CreatedByName ?? "—");                      // โดย
                item.SubItems.Add("—");                                              // Ver
                item.SubItems.Add(folder.OrgUnitName ?? "—");                        // หน่วยงาน

                // Check permissions for this folder
                string permissionText = GetFolderPermissionText(folder);
                bool hasViewAccess = HasFolderViewAccess(folder);
                var permSubItem = item.SubItems.Add(permissionText);                 // สิทธิ์
                permSubItem.ForeColor = GetPermissionColor(permissionText);

                item.Tag = folder;
                item.ForeColor = hasViewAccess
                    ? ConfidentialityColor(folder.ConfidentialityLevel)
                    : Color.FromArgb(140, 140, 140);
                item.UseItemStyleForSubItems = false;
                // Confidentiality badge sub-item colored
                item.SubItems[2].ForeColor = hasViewAccess
                    ? ConfidentialityColor(folder.ConfidentialityLevel)
                    : Color.FromArgb(140, 140, 140);
                contentsListView.Items.Add(item);
            }

            // ── Files ──────────────────────────────────────────────────────
            foreach (var file in fileList)
            {
                _currentFiles.Add(file);

                // Populate OrgUnitName from parent folder's OrgUnit
                string ouName;
                // Try to get OrgUnit from the folder that contains this file
                // We use the current selected folder's OrgUnitID
                var selectedFolder = SelectedFolder;
                if (selectedFolder != null)
                {
                    file.OrgUnitName = orgUnitNames.TryGetValue(selectedFolder.OrgUnitID, out ouName) ? ouName : "—";
                }
                else
                {
                    file.OrgUnitName = "—";
                }

                string icon = GetFileImageKey(file);
                var item = new ListViewItem(file.OriginalFileName) { ImageKey = icon };
                item.SubItems.Add(file.FileExtension.TrimStart('.').ToUpper()); // ประเภท
                item.SubItems.Add(ConfidentialityShort(file.ConfidentialityLevel)); // ระดับ
                item.SubItems.Add(FormatBytes(file.FileSizeBytes));              // ขนาด
                item.SubItems.Add(file.UploadedAt.ToString("dd/MM/yy HH:mm")); // วันที่
                item.SubItems.Add(file.UploadedByName ?? "—");                  // โดย
                item.SubItems.Add(file.Version.ToString());                      // Ver
                item.SubItems.Add(file.OrgUnitName ?? "—");                      // หน่วยงาน
                item.SubItems.Add("—");                                          // สิทธิ์ (files inherit from folder)
                item.Tag = file;
                item.UseItemStyleForSubItems = false;

                // ── Display hidden status with RED circle ──────────────────────
                if (file.IsHidden)
                {
                    // Mark hidden file with red circle: 🔴
                    item.SubItems[0].Text = "🔴 " + item.SubItems[0].Text;
                }

                // Confidentiality badge colored
                item.SubItems[2].ForeColor = ConfidentialityColor(file.ConfidentialityLevel);

                // Virus scan status color on name
                if (file.VirusScanStatus == 2)
                    item.ForeColor = Color.DarkRed;
                else if (file.VirusScanStatus == 3)
                    item.ForeColor = Color.OrangeRed;
                else
                    item.ForeColor = SystemColors.WindowText;

                contentsListView.Items.Add(item);
            }

            contentsListView.EndUpdate();
            contentsListView.ResumeLayout();

            // Apply Tile columns when in Tile view (ชื่อ + ประเภท + ขนาด)
            if (_currentView == View.Tile)
            {
                contentsListView.TileSize = new Size(260, 42);
                if (contentsListView.Columns.Count >= 4)
                {
                    contentsListView.Columns[1].Width = 80;  // ประเภท
                    contentsListView.Columns[3].Width = 70;  // ขนาด
                }
            }

            // Populate extension ComboBox with distinct extensions from current files (Req 6.1)
            cmbExtension.Items.Clear();
            cmbExtension.Items.Add("ทั้งหมด"); // "all" option
            foreach (var ext in fileList.Select(f => f.FileExtension).Distinct().OrderBy(e => e))
                if (!string.IsNullOrEmpty(ext))
                    cmbExtension.Items.Add(ext);
            cmbExtension.Text = "ทั้งหมด";

            // Clear filter status when folder changes (Req 6.3)
            lblFilterStatus.Text = string.Empty;
            lblFilterStatus.Visible = false;

            // Update content header info using actual loaded rows (prevents stale Folder.FileCount)
            int totalItems = folderList.Count + fileList.Count;
            long totalFileSize = fileList.Sum(f => f.FileSizeBytes);
            var currentFolder = SelectedFolder ?? _lastSelectedFolder;
            string currentLevel = currentFolder != null
                ? ConfidentialityLabel(currentFolder.ConfidentialityLevel)
                : "ทุกระดับ";
            string currentName = currentFolder != null
                ? currentFolder.FolderName
                : (_selectedOrgUnitName ?? "รายการทั้งหมด");

            lblContentInfo.ForeColor = SystemColors.ControlText;
            lblContentInfo.Text = string.Format(
                "{0} รายการ ({1} Folder, {2} ไฟล์ ขนาด {3}) | {4} | {5}",
                totalItems, folderList.Count, fileList.Count, FormatBytes(totalFileSize), currentLevel, currentName);

            // Adjust column widths to fit content
            AdjustNameColumnWidth();

            _currentSubFolders = folderList;

            // Show drop hint only when folder is empty — allows drag-drop upload even in empty folders
            bool isEmpty = folderList.Count == 0 && fileList.Count == 0;
            SetDropHintVisible(isEmpty && _currentNavFolderId.HasValue);
        }

        private int GetActualFileCount(Folder folder, Dictionary<int, int> cache)
        {
            if (folder == null)
                return 0;

            int cached;
            if (cache != null && cache.TryGetValue(folder.FolderID, out cached))
                return cached;

            int count = Math.Max(0, folder.FileCount);

            if (_fileBrowseController != null)
            {
                try
                {
                    var liveFiles = _fileBrowseController.GetFilesInFolder(folder.FolderID, _user);
                    count = liveFiles == null
                        ? 0
                        : liveFiles.Count(f => f != null && !f.IsDeleted);
                }
                catch
                {
                    // Fallback to Folder.FileCount when live query fails.
                    count = Math.Max(0, folder.FileCount);
                }
            }

            if (cache != null)
                cache[folder.FolderID] = count;

            return count;
        }

        /// <summary>
        /// อัปเดต panel ล่างซ้ายให้แสดง folders + files เดียวกับ content panel ด้านขวา
        /// </summary>
        private void RefreshSubfolderPanel(List<Folder> folders = null, List<FileDocument> files = null)
        {
            if (_lstQuickAccess == null) return;

            _lstQuickAccess.Items.Clear();

            bool hasFolders = folders != null && folders.Count > 0;
            bool hasFiles = files != null && files.Count > 0;

            if (!hasFolders && !hasFiles)
            {
                _lstQuickAccess.Items.Add("(ไม่มีรายการ)");
                return;
            }

            if (hasFolders)
            {
                foreach (var folder in folders)
                {
                    string label = string.Format("📁 {0} ({1}) [{2}]",
                        folder.Name,
                        folder.FileCount,
                        ConfidentialityShort(folder.ConfidentialityLevel));
                    _lstQuickAccess.Items.Add(label);
                }
            }

            if (hasFiles)
            {
                foreach (var file in files)
                {
                    string label = string.Format("📄 {0}", file.OriginalFileName);
                    _lstQuickAccess.Items.Add(label);
                }
            }
        }

        // -----------------------------------------------------------------------
        // IFolderView — ShowRecycleBin  (Req 4.9–4.13)
        // -----------------------------------------------------------------------

        public void ShowRecycleBin(IEnumerable<Folder> deletedFolders)
        {
            // Recycle bin is now a dedicated RecycleBinForm — nothing to do here
        }

        // -----------------------------------------------------------------------
        // IFolderView — messages
        // -----------------------------------------------------------------------

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowInfo(string message)
        {
            // Access denied messages go to the content header label — no popup
            if (message != null && message.StartsWith("🔒"))
            {
                lblContentInfo.Text = message;
                lblContentInfo.ForeColor = Color.DarkRed;
                return;
            }
            // Force UI to repaint before showing the dialog
            this.Refresh();
            Application.DoEvents();
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowAccessDenied()
        {
            lblContentInfo.Text = "🔒 ไม่มีสิทธิ์เข้าถึง Folder นี้";
            lblContentInfo.ForeColor = Color.DarkRed;
        }

        // -----------------------------------------------------------------------
        // IFolderView — SelectedFolder / SelectedRecycleBinFolder
        // -----------------------------------------------------------------------

        public Folder SelectedFolder
        {
            get
            {
                // ใช้ SelectedNode ก่อน ถ้า null ให้ fallback ไปที่ cache
                if (folderTreeView.SelectedNode != null)
                {
                    var f = folderTreeView.SelectedNode.Tag as Folder;
                    if (f != null) return f;
                }
                return _lastSelectedFolder;
            }
        }

        /// <summary>
        /// Returns the first Folder found in the tree (depth-first).
        /// Used as fallback when no node is explicitly selected.
        /// </summary>
        public Folder FirstFolderInTree
        {
            get { return FindFirstFolder(folderTreeView.Nodes); }
        }

        private static Folder FindFirstFolder(System.Windows.Forms.TreeNodeCollection nodes)
        {
            foreach (System.Windows.Forms.TreeNode node in nodes)
            {
                var f = node.Tag as Folder;
                if (f != null) return f;
                var child = FindFirstFolder(node.Nodes);
                if (child != null) return child;
            }
            return null;
        }

        public Folder SelectedFolderInContents
        {
            get
            {
                if (contentsListView.SelectedItems.Count > 0)
                    return contentsListView.SelectedItems[0].Tag as Folder;
                return null;
            }
        }

        public Folder SelectedRecycleBinFolder => null;  // Recycle bin moved to RecycleBinForm

        public FileDocument SelectedFile
        {
            get
            {
                if (contentsListView.SelectedItems.Count == 0) return null;
                return contentsListView.SelectedItems[0].Tag as FileDocument;
            }
        }

        public List<FileDocument> SelectedFiles => GetSelectedFilesFromContents();

        // -----------------------------------------------------------------------
        // Mouse event handlers for better ListView interaction
        // -----------------------------------------------------------------------

        private void ContentsListView_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // Ensure ListView gets focus
                if (!contentsListView.Focused)
                {
                    contentsListView.Focus();
                }

                // Handle item selection on mouse down
                var hitTest = contentsListView.HitTest(e.Location);
                if (hitTest.Item != null)
                {
                    var hitFolder = hitTest.Item.Tag as Folder;
                    if (hitFolder != null && !HasFolderViewAccess(hitFolder))
                    {
                        contentsListView.SelectedItems.Clear();
                        ShowNoAccessFolderHint(hitFolder.FolderName);
                        return;
                    }

                    bool hasModifier = (Control.ModifierKeys & (Keys.Control | Keys.Shift)) != 0;

                    if (e.Button == MouseButtons.Right)
                    {
                        if (!hitTest.Item.Selected)
                        {
                            contentsListView.SelectedItems.Clear();
                            hitTest.Item.Selected = true;
                        }
                        hitTest.Item.Focused = true;
                        return;
                    }

                    if (!hasModifier)
                    {
                        if (!hitTest.Item.Selected || contentsListView.SelectedItems.Count > 1)
                            contentsListView.SelectedItems.Clear();

                        hitTest.Item.Selected = true;
                        hitTest.Item.Focused = true;
                    }
                }
                else
                {
                    // Clicked on empty area - clear selection only on plain left-click.
                    bool hasModifier = (Control.ModifierKeys & (Keys.Control | Keys.Shift)) != 0;
                    if (e.Button == MouseButtons.Left && !hasModifier)
                        contentsListView.SelectedItems.Clear();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ContentsListView_MouseDown error: {ex.Message}");
            }
        }

        private void ContentsListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                // Get item at double-click position
                var hitTest = contentsListView.HitTest(e.Location);
                if (hitTest.Item == null)
                {
                    return;
                }

                // Ensure the item is selected
                if (!hitTest.Item.Selected)
                {
                    contentsListView.SelectedItems.Clear();
                    hitTest.Item.Selected = true;
                    hitTest.Item.Focused = true;
                }

                var tag = hitTest.Item.Tag;
                var file = tag as FileDocument;
                var folder = tag as Folder;

                if (file != null)
                {
                    CtxFileOpen_Click(sender, new EventArgs());
                }
                else if (folder != null)
                {
                    if (!HasFolderViewAccess(folder))
                    {
                        ShowNoAccessFolderHint(folder.FolderName);
                        return;
                    }
                    NavigateToFolder(folder);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ContentsListView_MouseDoubleClick error: {ex.Message}");
                ShowError($"เกิดข้อผิดพลาดในการเปิด: {ex.Message}");
            }
        }

        private void NavigateToFolder(Folder folder)
        {
            try
            {
                if (folder == null || !HasFolderViewAccess(folder))
                {
                    ShowNoAccessFolderHint(folder != null ? folder.FolderName : null);
                    return;
                }

                PushNavigation(folder.FolderId);
                _lastSelectedFolder = folder;
                _currentNavFolderId = folder.FolderId;

                lblContentInfo.Text = string.Format("📁 {0} | กำลังโหลด...", folder.FolderName);
                _presenter.GetFolderContents(folder.FolderId);

                // Build breadcrumb จาก tree hierarchy แทน string-append
                // เพื่อป้องกัน breadcrumb ยาวซ้ำเมื่อ navigate หลายชั้น
                bool found = RestoreSelectedNode(folderTreeView.Nodes, folder.FolderId);
                SetActiveNode(folderTreeView.SelectedNode);
                string newBreadcrumb;
                if (found && folderTreeView.SelectedNode != null)
                {
                    newBreadcrumb = _selectedOrgUnit != null
                        ? BuildBreadcrumbPathFromOrgUnit(_selectedOrgUnit, folderTreeView.SelectedNode)
                        : BuildBreadcrumbPath(treeOrgUnit.SelectedNode, folderTreeView.SelectedNode);
                }
                else
                {
                    // Fallback: node ไม่อยู่ใน tree (เช่น conf view) → append เหมือนเดิม
                    newBreadcrumb = string.IsNullOrEmpty(_currentBreadcrumb)
                        ? "🏠 PC  ›  " + folder.FolderName
                        : _currentBreadcrumb + "  ›  " + folder.FolderName;
                }

                var newParts = newBreadcrumb.Split(new string[] { "  ›  " }, StringSplitOptions.RemoveEmptyEntries);
                var newIds = new List<int?>();
                for (int i = 0; i < newParts.Length - 1; i++)
                    newIds.Add(i < _breadcrumbFolderIds.Count ? _breadcrumbFolderIds[i] : (int?)null);
                newIds.Add(folder.FolderId);

                _breadcrumbFolderIds.Clear();
                _breadcrumbFolderIds.AddRange(newIds);

                ApplyBreadcrumb(newBreadcrumb, newIds);
                UpdateNavButtons();

                lblConfidentialityBadge.Text = ConfidentialityLabel(folder.ConfidentialityLevel);
                lblConfidentialityBadge.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
                lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", folder.FolderName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NavigateToFolder error: {ex.Message}");
                ShowError($"ไม่สามารถเข้าถึงโฟลเดอร์ได้: {ex.Message}");
            }
        }

        // -----------------------------------------------------------------------
        // Content list selection — update detail panel
        // -----------------------------------------------------------------------

        private void ContentsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Defer to next message-loop tick so all WinForms internal selection events settle first.
            // Without this, MouseDown's Clear()+Select() fires two SelectedIndexChanged events and
            // the WinForms WM_LBUTTONDOWN processing fires a third (count=0) after ours, leaving
            // the detail panel collapsed even though the item visually appears selected.
            this.BeginInvoke(new Action(UpdateDetailPanel));
        }

        private void UpdateDetailPanel()
        {
            if (contentsListView.SelectedItems.Count == 0)
            {
                splitContent.Panel2Collapsed = true;

                lblDetailHeader.Text = "📄 รายละเอียด";
                lblDetailContent.Text = "เลือกรายการเพื่อดูรายละเอียด";
                lblContentInfo.Text = lblContentInfo.Text.Contains("|")
                    ? lblContentInfo.Text.Substring(0, lblContentInfo.Text.LastIndexOf('|') + 1)
                      + " ไม่มีรายการที่เลือก"
                    : lblContentInfo.Text;
                return;
            }

            splitContent.Panel2Collapsed = false;

            var tag = contentsListView.SelectedItems[0].Tag;
            var file = tag as FileDocument;
            var folder = tag as Folder;

            if (file != null)
                CreateModernDetailPanel(file);
            else if (folder != null)
                CreateModernDetailPanel(folder);
        }

        private void CreateModernDetailPanel(FileDocument file)
        {
            pnlDetail.Controls.Clear();
            pnlDetail.BackColor = Color.White;

            int pw = pnlDetail.ClientSize.Width > 0 ? pnlDetail.ClientSize.Width : 160;

            // ── Header (Top) ─────────────────────────────────────────
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(pw, 42),
                BackColor = Color.FromArgb(31, 56, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            headerPanel.Controls.Add(new Label
            {
                Text = "📄",
                Font = new Font("Segoe UI", 13f),
                ForeColor = Color.White,
                Location = new Point(4, 6),
                Size = new Size(26, 30)
            });
            headerPanel.Controls.Add(new Label
            {
                Text = file.OriginalFileName,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(32, 4),
                Size = new Size(pw - 36, 34),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            });

            // ── Details rows ─────────────────────────────────────────
            var details = new[]
            {
                new { Label = "ชนิด",  Value = file.FileExtension.TrimStart('.').ToUpper() },
                new { Label = "ระดับ", Value = ConfidentialityShort(file.ConfidentialityLevel) },
                new { Label = "ขนาด",  Value = FormatBytes(file.FileSizeBytes) },
                new { Label = "วันที่", Value = file.UploadedAt.ToString("dd/MM/yyyy") }
            };

            int rowH = 22;
            int startY = 48;
            foreach (var d in details)
            {
                pnlDetail.Controls.Add(new Label
                {
                    Text = d.Label + ":",
                    Font = new Font("Segoe UI", 8f),
                    Location = new Point(6, startY),
                    Size = new Size(38, rowH),
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.Gray
                });
                pnlDetail.Controls.Add(new Label
                {
                    Text = d.Value,
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    Location = new Point(46, startY),
                    Size = new Size(pw - 50, rowH),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                });
                startY += rowH;
            }

            // ── Action buttons (Bottom) ───────────────────────────────
            var btnOpen = new Button
            {
                Text = "🔍  เปิดดู",
                Location = new Point(6, startY + 8),
                Size = new Size(pw - 12, 30),
                BackColor = Color.FromArgb(43, 87, 154),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            btnOpen.FlatAppearance.BorderSize = 0;
            btnOpen.Click += CtxFileOpen_Click;

            var btnDownload = new Button
            {
                Text = "⬇  ดาวน์โหลด",
                Location = new Point(6, startY + 46),
                Size = new Size(pw - 12, 30),
                BackColor = Color.FromArgb(56, 142, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            btnDownload.FlatAppearance.BorderSize = 0;
            btnDownload.Click += CtxFileDownload_Click;

            pnlDetail.Controls.Add(headerPanel);
            pnlDetail.Controls.Add(btnOpen);
            pnlDetail.Controls.Add(btnDownload);
        }

        private void CreateModernDetailPanel(Folder folder)
        {
            pnlDetail.Controls.Clear();
            pnlDetail.BackColor = Color.White;

            int pw = pnlDetail.ClientSize.Width > 0 ? pnlDetail.ClientSize.Width : 160;

            // ── Header ───────────────────────────────────────────────
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(pw, 42),
                BackColor = Color.FromArgb(31, 56, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            headerPanel.Controls.Add(new Label
            {
                Text = "📁",
                Font = new Font("Segoe UI", 13f),
                ForeColor = Color.White,
                Location = new Point(4, 6),
                Size = new Size(26, 30)
            });
            var lblFolderName = new Label
            {
                Text = folder.FolderName,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(32, 4),
                Size = new Size(pw - 36, 34),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };
            var _clickFolder = folder; // capture for closure
            EventHandler folderClickHandler = (s, ev) =>
            {
                // Click บน Part name → navigate เข้า folder นั้น (เหมือน double-click)
                bool found = RestoreSelectedNode(folderTreeView.Nodes, _clickFolder.FolderId);
                if (!found)
                {
                    string newBreadcrumb = string.IsNullOrEmpty(_currentBreadcrumb)
                        ? "🏠 PC  ›  " + _clickFolder.Name
                        : _currentBreadcrumb + "  ›  " + _clickFolder.Name;
                    PushNavigation(_clickFolder.FolderId);
                    _currentNavFolderId = _clickFolder.FolderId;
                    _presenter.GetFolderContents(_clickFolder.FolderId);
                    ApplyBreadcrumb(newBreadcrumb, new List<int?> { _clickFolder.FolderId });
                    UpdateNavButtons();
                }
            };
            lblFolderName.Click += folderClickHandler;
            var lblHint = new Label
            {
                Text = "คลิกเพื่อเปิด",
                Font = new Font("Segoe UI", 6f, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 220, 255),
                Location = new Point(32, 28),
                Size = new Size(pw - 36, 12),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };
            lblHint.Click += folderClickHandler;
            headerPanel.Controls.Add(lblFolderName);
            headerPanel.Controls.Add(lblHint);

            // ── Details rows ─────────────────────────────────────────
            var details = new[]
            {
                new { Label = "ระดับ", Value = ConfidentialityShort(folder.ConfidentialityLevel) },
                new { Label = "ไฟล์",  Value = folder.FileCount + " รายการ" },
                new { Label = "ขนาด",  Value = FormatBytes(folder.FolderSizeBytes) },
                new { Label = "สร้าง",  Value = folder.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy") }
            };

            int rowH = 22;
            int startY = 48;
            foreach (var d in details)
            {
                pnlDetail.Controls.Add(new Label
                {
                    Text = d.Label + ":",
                    Font = new Font("Segoe UI", 8f),
                    Location = new Point(6, startY),
                    Size = new Size(38, rowH),
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.Gray
                });
                pnlDetail.Controls.Add(new Label
                {
                    Text = d.Value,
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    Location = new Point(46, startY),
                    Size = new Size(pw - 50, rowH),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                });
                startY += rowH;
            }

            pnlDetail.Controls.Add(headerPanel);
        }

        private void ContentsListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            // Skip custom drawing when not in Details mode
            if (_currentView != View.Details)
            {
                e.DrawDefault = true;
                return;
            }
            // Alternating row background
            bool isSelected = e.Item.Selected;
            bool isEven = e.ItemIndex % 2 == 0;

            Color backColor = isSelected
                ? Color.FromArgb(66, 133, 244)
                : isEven ? Color.White : Color.FromArgb(245, 247, 252);

            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            Color textColor = isSelected ? Color.White : e.SubItem.ForeColor;
            if (!isSelected && (textColor == Color.Empty || textColor == SystemColors.WindowText))
                textColor = e.Item.ForeColor;
            if (textColor == Color.Empty) textColor = Color.Black;

            // ── Column 0: draw icon + name ────────────────────────────────────
            if (e.ColumnIndex == 0)
            {
                var il = contentsListView.SmallImageList;
                int iconW = 0;
                int iconX = e.Bounds.X + 2;
                int iconY = e.Bounds.Y + (e.Bounds.Height - 16) / 2;

                if (il != null && !string.IsNullOrEmpty(e.Item.ImageKey)
                    && il.Images.ContainsKey(e.Item.ImageKey))
                {
                    int imgIdx = il.Images.IndexOfKey(e.Item.ImageKey);
                    if (imgIdx >= 0)
                        il.Draw(e.Graphics, iconX, iconY, 16, 16, imgIdx);
                    iconW = 20; // 16px icon + 4px gap
                }

                var textBounds = new Rectangle(
                    e.Bounds.X + 2 + iconW,
                    e.Bounds.Y,
                    e.Bounds.Width - 2 - iconW,
                    e.Bounds.Height);

                var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                            TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text,
                                      e.Item.ListView.Font, textBounds, textColor, flags);
                return;
            }

            // ── Other columns ─────────────────────────────────────────────────
            var flags2 = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                         TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;
            var bounds2 = Rectangle.Inflate(e.Bounds, -2, 0);
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text,
                                  e.Item.ListView.Font, bounds2, textColor, flags2);
        }

        // -----------------------------------------------------------------------
        // Column sort (Req 4.1–4.5)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Handles ColumnClick: toggles sort direction when clicking the same column,
        /// resets to ascending when clicking a new column, then applies the comparer
        /// and updates the sort indicator in the column header.
        /// </summary>
        private void ContentsListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (_sortColumn == e.Column)
            {
                // Same column — toggle direction (Req 4.2)
                _sortAscending = !_sortAscending;
            }
            else
            {
                // New column — start ascending (Req 4.1)
                _sortColumn = e.Column;
                _sortAscending = true;
            }

            // Apply comparer (Req 4.4, 4.5)
            contentsListView.ListViewItemSorter =
                new ListViewItemComparer(_sortColumn, _sortAscending);

            // Update sort indicator on column headers (Req 4.3)
            for (int i = 0; i < contentsListView.Columns.Count; i++)
            {
                string baseText = GetColumnBaseText(i);
                if (i == _sortColumn)
                    contentsListView.Columns[i].Text = baseText + (_sortAscending ? " ▲" : " ▼");
                else
                    contentsListView.Columns[i].Text = baseText;
            }
        }

        /// <summary>Returns the base (indicator-free) text for a column header.</summary>
        private static string GetColumnBaseText(int columnIndex)
        {
            switch (columnIndex)
            {
                case 0: return "ชื่อ";
                case 1: return "ประเภท";
                case 2: return "ระดับ";
                case 3: return "ขนาด";
                case 4: return "วันที่";
                case 5: return "โดย";
                case 6: return "Ver";
                case 7: return "หน่วยงาน";
                case 8: return "สิทธิ์";
                default: return string.Empty;
            }
        }

        // -----------------------------------------------------------------------
        // TreeView — node selection loads contents
        // -----------------------------------------------------------------------

        private bool _isAdjustingColumns = false;

        private void AdjustNameColumnWidth()
        {
            if (contentsListView == null || contentsListView.Columns.Count == 0) return;
            if (_isAdjustingColumns) return;
            if (contentsListView.View != View.Details) return;

            _isAdjustingColumns = true;
            try
            {
                contentsListView.SuspendLayout();

                // Fixed widths for non-name columns (index 1..8)
                // Keep layout stable and only stretch Name column to fill remaining space.
                int[] fixedWidths = { 0, 95, 70, 85, 125, 160, 48, 220, 85 };
                int fixedTotal = 0;

                for (int i = 1; i < contentsListView.Columns.Count && i < fixedWidths.Length; i++)
                {
                    int target = fixedWidths[i];
                    if (contentsListView.Columns[i].Width != target)
                        contentsListView.Columns[i].Width = target;

                    fixedTotal += target;
                }

                int available = contentsListView.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8;
                int minNameWidth = 260;
                int targetNameWidth = Math.Max(minNameWidth, available - fixedTotal);

                if (contentsListView.Columns[0].Width != targetNameWidth)
                    contentsListView.Columns[0].Width = targetNameWidth;
            }
            finally
            {
                contentsListView.ResumeLayout();
                _isAdjustingColumns = false;
            }
        }

        private void ClearContentPanel(bool showPanel = true)
        {
            contentsListView.Items.Clear();
            _currentFiles.Clear();
            _currentSubFolders.Clear();
            txtBreadcrumb.Text = "";
            lblContentInfo.Text = "";
            lblConfidentialityBadge.Text = "";
            lblFilterStatus.Text = string.Empty;
            lblFilterStatus.Visible = false;
            cmbExtension.Items.Clear();

            SetDropHintVisible(!_showingMainMenuView && _currentNavFolderId.HasValue);

            // ซ่อน Section 3 เสมอเมื่ออยู่ใน "แบบหมวดหมู่" (Main Menu view)
            splitContent.Panel2Collapsed = _showingMainMenuView || !showPanel;
        }

        private void FolderTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var hit = folderTreeView.HitTest(e.Location);
            if ((hit.Location & TreeViewHitTestLocations.PlusMinus) == TreeViewHitTestLocations.PlusMinus)
                return;

            var folder = e.Node?.Tag as Folder;
            if (folder == null) return;
            if (!HasFolderViewAccess(folder))
            {
                ShowNoAccessFolderHint(folder.FolderName);
                return;
            }

            // PushNavigation ก่อน — ต้อง push _currentNavFolderId เก่าเข้า stack ก่อนที่จะ update
            if (!_isNavigating)
                PushNavigation(folder.FolderId);

            _lastSelectedFolder = folder;
            _currentNavFolderId = folder.FolderId;
            UpdateNavButtons();

            ClearContentPanel();
            // แสดงรายการไฟล์เฉพาะใน "ตามองค์กร" และ "ตามความสำคัญ" เท่านั้น
            if (!_showingMainMenuView)
            {
                _presenter.GetFolderContents(folder.FolderId);
            }

            // Build breadcrumb จาก tree hierarchy พร้อม OrgUnit prefix (ไม่ใช้ string-append)
            string breadcrumbPath = _selectedOrgUnit != null
                ? BuildBreadcrumbPathFromOrgUnit(_selectedOrgUnit, e.Node)
                : BuildBreadcrumbPath(treeOrgUnit.SelectedNode, e.Node);

            var folderIdPath = BuildFolderIdPathAligned(breadcrumbPath, e.Node);
            _breadcrumbFolderIds.Clear();
            _breadcrumbFolderIds.AddRange(folderIdPath);
            ApplyBreadcrumb(breadcrumbPath, folderIdPath);

            lblConfidentialityBadge.Text = ConfidentialityLabel(folder.ConfidentialityLevel);
            lblConfidentialityBadge.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
            lblContentInfo.Text = string.Format("กำลังดู: {0}", folder.Name);
            lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", folder.Name);
        }

        private void folderTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // NodeMouseClick จัดการ navigation แล้ว
            // AfterSelect ใช้สำหรับ sync state เท่านั้น
            if (e.Node == null) { ClearContentPanel(); return; }

            var folder = e.Node.Tag as Folder;
            if (folder == null) { ClearContentPanel(); return; }

            // อัพเดท state เท่านั้น ไม่ reload content (NodeMouseClick ทำแล้ว)
            _lastSelectedFolder = folder;
            _currentNavFolderId = folder.FolderId;
            SetActiveNode(e.Node);
            UpdateNavButtons();
        }

        private void FolderTreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            // ข้ามการเช็ค DB สำหรับ right-click: context menu Opening event จัดการ permission เองอยู่แล้ว
            if (_isRightClickSelecting) return;

            var folder = e.Node != null ? e.Node.Tag as Folder : null;
            if (folder != null && !HasFolderViewAccess(folder))
            {
                e.Cancel = true;
                ShowNoAccessFolderHint(folder.FolderName);
            }
        }

        private void ConfidentialityTree_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            var folder = e.Node != null ? e.Node.Tag as Folder : null;
            if (folder != null && !HasFolderViewAccessConfTree(folder))
            {
                e.Cancel = true;
                ShowNoAccessFolderHint(folder.FolderName);
            }
        }

        private void ShowNoAccessFolderHint(string folderName)
        {
            string name = string.IsNullOrWhiteSpace(folderName) ? "Folder นี้" : folderName;
            lblContentInfo.ForeColor = Color.DarkRed;
            lblContentInfo.Text = string.Format("🔒 ไม่มีสิทธิ์เข้าถึง {0}", name);
        }

        // -----------------------------------------------------------------------
        // NavBar button handlers
        // -----------------------------------------------------------------------

        private void btnNavRefresh_Click(object sender, EventArgs e)
        {
            btnNavRefresh.Text = "↻";
            btnNavRefresh.Enabled = false;

            try
            {
                lblContentInfo.ForeColor = SystemColors.ControlDarkDark;
                lblContentInfo.Text = "กำลังรีโหลดข้อมูลล่าสุด...";
                RefreshCurrentViewAfterMove();
            }
            finally
            {
                btnNavRefresh.Enabled = true;
                btnNavRefresh.Text = "↺";
            }
        }

        private void btnViewList_Click(object sender, EventArgs e)
        {
            _currentView = View.Details;
            contentsListView.View = View.Details;
            btnViewList.BackColor = Color.FromArgb(43, 87, 154);
            btnViewList.ForeColor = Color.White;
            btnViewGrid.BackColor = Color.FromArgb(240, 244, 250);
            btnViewGrid.ForeColor = Color.FromArgb(43, 87, 154);
        }

        private void btnViewGrid_Click(object sender, EventArgs e)
        {
            _currentView = View.LargeIcon;
            contentsListView.View = View.LargeIcon;
            btnViewGrid.BackColor = Color.FromArgb(43, 87, 154);
            btnViewGrid.ForeColor = Color.White;
            btnViewList.BackColor = Color.FromArgb(240, 244, 250);
            btnViewList.ForeColor = Color.FromArgb(43, 87, 154);
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "ค้นหาใน Folder...")
            {
                txtSearch.Text = string.Empty;
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                txtSearch.Text = "ค้นหาใน Folder...";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text == "ค้นหาใน Folder...") return;
            var keyword = txtSearch.Text.Trim().ToLower();
            contentsListView.BeginUpdate();
            foreach (ListViewItem item in contentsListView.Items)
            {
                bool match = string.IsNullOrEmpty(keyword) ||
                             item.Text.ToLower().Contains(keyword);
                item.ForeColor = match ? Color.Black : Color.LightGray;
            }
            contentsListView.EndUpdate();
        }

        private void btnToggleSidebar_Click(object sender, EventArgs e)
        {
            // Toggle sidebar visibility and update button icon
            _sidebarCollapsed = !_sidebarCollapsed;

            if (_sidebarCollapsed)
            {
                // Collapse: save current width, collapse to 20px (leaving button visible)
                _sidebarExpandedWidth = splitLR.SplitterDistance;
                splitLR.SplitterDistance = 20;  // Keep 20px for button edge only
                pnlExplorer.Visible = false;
                btnToggleSidebar.Text = "▶"; // Right-pointing arrow when collapsed
                btnToggleSidebar.BackColor = Color.FromArgb(220, 230, 245); // Light blue for collapsed state
            }
            else
            {
                // Expand: restore previous width
                pnlExplorer.Visible = true;
                splitLR.SplitterDistance = _sidebarExpandedWidth;
                btnToggleSidebar.Text = "◀"; // Left-pointing arrow when expanded
                btnToggleSidebar.BackColor = Color.FromArgb(238, 242, 248); // Normal color when expanded
            }
        }

        private void BtnNavBack_Click(object sender, EventArgs e)
        {
            if (_backStack.Count == 0) return;

            // เก็บ current ลง forward stack ก่อน
            _forwardStack.Push(_currentNavFolderId);
            _forwardBreadcrumbs.Push(_currentBreadcrumb);

            // pop previous state
            var prevFolderId = _backStack.Pop();
            var prevBreadcrumb = _backBreadcrumbs.Count > 0 ? _backBreadcrumbs.Pop() : "";

            _isNavigating = true;
            try
            {
                _currentNavFolderId = prevFolderId;
                _currentBreadcrumb = prevBreadcrumb;
                ExecuteNavigation(_currentNavFolderId, _currentBreadcrumb);
            }
            finally { _isNavigating = false; UpdateNavButtons(); }
        }

        private void BtnNavForward_Click(object sender, EventArgs e)
        {
            if (_forwardStack.Count == 0) return;

            // เก็บ current ลง back stack ก่อน
            _backStack.Push(_currentNavFolderId);
            _backBreadcrumbs.Push(_currentBreadcrumb);

            // pop next state
            var nextFolderId = _forwardStack.Pop();
            var nextBreadcrumb = _forwardBreadcrumbs.Count > 0 ? _forwardBreadcrumbs.Pop() : "";

            _isNavigating = true;
            try
            {
                _currentNavFolderId = nextFolderId;
                _currentBreadcrumb = nextBreadcrumb;
                ExecuteNavigation(_currentNavFolderId, _currentBreadcrumb);
            }
            finally { _isNavigating = false; UpdateNavButtons(); }
        }

        private void BtnNavUp_Click(object sender, EventArgs e)
        {
            if (!_currentNavFolderId.HasValue) return;  // อยู่ที่ root แล้ว

            // หา parent จาก node ที่เลือกอยู่ใน folderTreeView
            int? parentFolderId = null;
            var currentNode = folderTreeView.SelectedNode;
            if (currentNode != null && currentNode.Parent != null)
            {
                var parentFolder = currentNode.Parent.Tag as Folder;
                if (parentFolder != null)
                    parentFolderId = parentFolder.FolderId;
                // else parent node ไม่ใช่ folder → parentFolderId stays null (OrgUnit root)
            }

            _backStack.Push(_currentNavFolderId);
            _backBreadcrumbs.Push(_currentBreadcrumb);
            _forwardStack.Clear();
            _forwardBreadcrumbs.Clear();
            _currentNavFolderId = parentFolderId;
            // Build parent breadcrumb by trimming last segment
            string parentBreadcrumb = TrimLastBreadcrumbSegment(_currentBreadcrumb);
            _currentBreadcrumb = parentBreadcrumb;
            _isNavigating = true;
            try { ExecuteNavigation(parentFolderId, parentBreadcrumb); }
            finally { _isNavigating = false; UpdateNavButtons(); }
        }

        /// <summary>Navigate ไปยัง folderId (null = OrgUnit root view) และ restore breadcrumb</summary>
        private void ExecuteNavigation(int? folderId, string breadcrumbOverride = null)
        {
            if (folderId.HasValue)
            {
                _currentNavFolderId = folderId.Value;

                // Sync tree selection (ไม่ trigger content reload เพราะ AfterSelect แค่ sync state)
                RestoreSelectedNode(folderTreeView.Nodes, folderId.Value);

                var node = FindNodeByFolderId(folderTreeView.Nodes, folderId.Value);
                var folder = node != null ? node.Tag as Folder : null;
                if (folder != null && !HasFolderViewAccess(folder))
                {
                    ShowNoAccessFolderHint(folder.FolderName);
                    return;
                }

                // โหลดทั้ง sub-folders และ files เหมือน Windows Explorer (Back/Forward ต้องแสดงไฟล์ด้วย)
                _presenter.GetFolderContents(folderId.Value);
                ApplyBreadcrumb(breadcrumbOverride ?? _currentBreadcrumb, null);

                // Update UI
                if (folder != null)
                {
                    lblConfidentialityBadge.Text = ConfidentialityLabel(folder.ConfidentialityLevel);
                    lblConfidentialityBadge.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
                    lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", folder.Name);
                }
            }
            else
            {
                // Navigate กลับไป OrgUnit root — restore breadcrumb และ content ให้ตรงกัน
                folderTreeView.AfterSelect -= folderTreeView_AfterSelect;
                folderTreeView.SelectedNode = null;
                folderTreeView.AfterSelect += folderTreeView_AfterSelect;
                _currentNavFolderId = null;
                _lastSelectedFolder = null;

                // ถ้า breadcrumbOverride มีค่า (restore จาก back/forward) ให้ใช้ค่านั้น
                // และแสดง root folders ของ OrgUnit ที่ selected อยู่
                string bc = breadcrumbOverride ?? BuildBreadcrumbPath(treeOrgUnit.SelectedNode, null);
                ApplyBreadcrumb(bc, new List<int?>());

                // แสดง root folders ของ OrgUnit ที่ selected (ไม่ใช่ ShowOrgUnitRootFolders ที่แสดง 4 folders)
                // ถ้า _selectedOrgUnitId มีค่า → แสดง folders ของ OrgUnit นั้น
                // ถ้าไม่มี → แสดง root folders ทั้งหมด
                _presenter.ShowOrgUnitRootFolders(_selectedOrgUnitId);
            }
        }

        /// <summary>ตั้ง breadcrumb text และสร้าง clickable parts เหมือน Windows Explorer</summary>
        private void ApplyBreadcrumb(string path, List<int?> folderIds = null)
        {
            _currentBreadcrumb = path ?? "";

            // อัพเดท _breadcrumbFolderIds เฉพาะเมื่อมีการส่ง folderIds มา
            if (folderIds != null)
            {
                _breadcrumbFolderIds.Clear();
                _breadcrumbFolderIds.AddRange(folderIds);
            }

            txtBreadcrumb.Visible = false;

            if (pnlNavBar.Controls["pnlClickableBreadcrumb"] == null)
                CreateClickableBreadcrumb();

            var pnlClickable = pnlNavBar.Controls["pnlClickableBreadcrumb"] as Panel;
            if (pnlClickable != null)
                UpdateClickableBreadcrumb(pnlClickable, _currentBreadcrumb, _breadcrumbFolderIds);
        }

        private void CreateClickableBreadcrumb()
        {
            var pnlClickable = new Panel
            {
                Name = "pnlClickableBreadcrumb",
                Location = txtBreadcrumb.Location,
                Size = txtBreadcrumb.Size,
                Anchor = txtBreadcrumb.Anchor,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
            };

            // Add click handler to switch to text mode when clicking on empty area
            pnlClickable.Click += (s, e) =>
            {
                // Switch to text breadcrumb for editing
                pnlClickable.Visible = false;
                txtBreadcrumb.Visible = true;
                txtBreadcrumb.Focus();
                txtBreadcrumb.SelectAll();
            };

            pnlNavBar.Controls.Add(pnlClickable);
            pnlClickable.BringToFront();

            // Add handler to switch back to clickable when text breadcrumb loses focus
            txtBreadcrumb.Leave += (s, e) =>
            {
                txtBreadcrumb.Visible = false;
                pnlClickable.Visible = true;
            };
        }

        private void UpdateClickableBreadcrumb(Panel panel, string breadcrumbPath, List<int?> folderIds = null)
        {
            panel.Controls.Clear();

            if (string.IsNullOrEmpty(breadcrumbPath))
                return;

            var parts = breadcrumbPath.Split(new string[] { "  ›  " }, StringSplitOptions.RemoveEmptyEntries);
            int x = 5;
            int y = (panel.Height - 20) / 2;

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i].Trim();
                bool isLast = (i == parts.Length - 1);

                // เก็บ folder ID ตรงๆ ใน Tag ของ label
                int? folderId = (folderIds != null && i < folderIds.Count) ? folderIds[i] : null;

                var lblPart = new Label
                {
                    Text = part,
                    AutoSize = true,
                    Location = new Point(x, y),
                    Font = new Font("Segoe UI", 10f, isLast ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = isLast ? Color.Black : Color.FromArgb(0, 102, 204),
                    Cursor = isLast ? Cursors.Default : Cursors.Hand,
                    Tag = folderId  // เก็บ folder ID โดยตรง
                };

                if (!isLast)
                {
                    int capturedIndex = i;
                    int? capturedFolderId = folderId;
                    string capturedText = part;

                    lblPart.Click += (s, e) => OnBreadcrumbPartClick(capturedIndex, capturedFolderId, capturedText);
                    lblPart.MouseEnter += (s, e) =>
                    {
                        ((Label)s).ForeColor = Color.FromArgb(0, 60, 160);
                        ((Label)s).Font = new Font("Segoe UI", 10f, FontStyle.Underline);
                    };
                    lblPart.MouseLeave += (s, e) =>
                    {
                        ((Label)s).ForeColor = Color.FromArgb(0, 102, 204);
                        ((Label)s).Font = new Font("Segoe UI", 10f, FontStyle.Regular);
                    };
                }

                panel.Controls.Add(lblPart);
                x += lblPart.PreferredWidth + 4;

                if (!isLast)
                {
                    var lblSep = new Label
                    {
                        Text = "›",
                        AutoSize = true,
                        Location = new Point(x, y),
                        Font = new Font("Segoe UI", 10f),
                        ForeColor = Color.Gray
                    };
                    panel.Controls.Add(lblSep);
                    x += lblSep.PreferredWidth + 4;
                }
            }
        }

        private void OnBreadcrumbPartClick(int index, int? folderId, string partText)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Breadcrumb click: index={index}, folderId={folderId}, text='{partText}'");
                System.Diagnostics.Debug.WriteLine($"  _breadcrumbFolderIds count: {_breadcrumbFolderIds.Count}");
                for (int i = 0; i < _breadcrumbFolderIds.Count; i++)
                    System.Diagnostics.Debug.WriteLine($"  [{i}] = {_breadcrumbFolderIds[i]}");

                // index 0 = 🏠 PC → root
                if (index == 0)
                {
                    NavigateToOrgUnitRoot();
                    return;
                }

                // มี folder ID → navigate ตรงๆ
                if (folderId.HasValue)
                {
                    NavigateToFolderById(folderId.Value);
                    return;
                }

                // ไม่มี folder ID → ลองหาจาก OrgUnit tree
                var orgNode = FindOrgUnitNodeByName(treeOrgUnit.Nodes, partText);
                if (orgNode != null)
                {
                    orgNode.EnsureVisible();
                    if (treeOrgUnit.SelectedNode != orgNode)
                    {
                        // เปลี่ยน selection → AfterSelect จะ fire และโหลด content อัตโนมัติ
                        treeOrgUnit.SelectedNode = orgNode;
                    }
                    else
                    {
                        // Node ถูกเลือกอยู่แล้ว → AfterSelect ไม่ fire → โหลด content เอง
                        LoadOrgUnitNode(orgNode);
                        ApplyBreadcrumb(BuildBreadcrumbPath(orgNode, null), new List<int?>());
                    }
                    return;
                }

                // ลองหาจาก folder tree
                var folderNode = FindNodeByName(folderTreeView.Nodes, partText);
                if (folderNode != null)
                {
                    var f = folderNode.Tag as Folder;
                    if (f != null)
                    {
                        if (folderTreeView.SelectedNode != folderNode)
                            folderTreeView.SelectedNode = folderNode;
                        else
                            _presenter.GetFolderContents(f.FolderId);
                        folderNode.EnsureVisible();
                    }
                    return;
                }

                // fallback → root
                NavigateToOrgUnitRoot();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnBreadcrumbPartClick error: {ex.Message}");
            }
        }

        private void BreadcrumbPart_Click(object sender, EventArgs e, string[] allParts, int clickedIndex)
        {
            try
            {
                string clickedText = allParts[clickedIndex].Trim();
                System.Diagnostics.Debug.WriteLine($"Breadcrumb clicked: '{clickedText}' (index {clickedIndex})");

                // Index 0 = 🏠 PC → go to root
                if (clickedIndex == 0)
                {
                    NavigateToOrgUnitRoot();
                    return;
                }

                // Try folder ID first (most accurate)
                if (clickedIndex < _breadcrumbFolderIds.Count && _breadcrumbFolderIds[clickedIndex].HasValue)
                {
                    NavigateToFolderById(_breadcrumbFolderIds[clickedIndex].Value);
                    return;
                }

                // Fallback: try to find folder node in tree by name
                var folderNode = FindNodeByName(folderTreeView.Nodes, clickedText);
                if (folderNode != null)
                {
                    folderTreeView.SelectedNode = folderNode;
                    folderNode.EnsureVisible();
                    return;
                }

                // Fallback: try to find OrgUnit node in treeOrgUnit by name
                var orgNode = FindOrgUnitNodeByName(treeOrgUnit.Nodes, clickedText);
                if (orgNode != null)
                {
                    treeOrgUnit.SelectedNode = orgNode;
                    orgNode.EnsureVisible();
                    return;
                }

                // Nothing found — just reload root folders silently
                _presenter.ShowOrgUnitRootFolders(_selectedOrgUnitId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BreadcrumbPart_Click error: {ex.Message}");
            }
        }

        private TreeNode FindOrgUnitNodeByName(TreeNodeCollection nodes, string name)
        {
            foreach (TreeNode node in nodes)
            {
                // Strip emoji prefix from node text before comparing
                string nodeText = CleanNodeText(node.Text);
                if (string.Equals(nodeText, name, StringComparison.OrdinalIgnoreCase))
                    return node;

                var found = FindOrgUnitNodeByName(node.Nodes, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>Public entry point — เปิด folder โดยตรงจาก FolderID ใช้โดย SharedWithMeForm</summary>
        public void NavigateToFolderById(int folderId)
        {
            Folder folder = null;

            if (_folderReadLookupController != null)
                folder = _folderReadLookupController.GetById(folderId);

            if (folder != null)
            {
                // bring to front ก่อน navigate
                if (WindowState == FormWindowState.Minimized)
                    WindowState = FormWindowState.Normal;
                BringToFront();
                Focus();
                NavigateToFolder(folder);
                return;
            }

            // fallback: ลองหาใน tree
            var node = FindNodeByFolderId(folderTreeView.Nodes, folderId);
            if (node != null) folder = node.Tag as Folder;

            if (folder == null) return;
            if (!HasFolderViewAccess(folder))
            {
                ShowNoAccessFolderHint(folder.FolderName);
                return;
            }

            _lastSelectedFolder = folder;
            PushNavigation(folderId);
            _currentNavFolderId = folderId;
            UpdateNavButtons();

            _presenter.GetFolderContents(folderId);

            // Trim breadcrumb และ IDs ให้เหลือแค่ถึง folder นี้
            var parts = _currentBreadcrumb.Split(new string[] { "  ›  " }, StringSplitOptions.RemoveEmptyEntries);
            int idx = -1;
            for (int i = 0; i < _breadcrumbFolderIds.Count; i++)
            {
                if (_breadcrumbFolderIds[i].HasValue && _breadcrumbFolderIds[i].Value == folderId)
                {
                    idx = i;
                    break;
                }
            }

            if (idx >= 0 && idx < parts.Length)
            {
                string newBreadcrumb = string.Join("  ›  ", parts.Take(idx + 1));
                var newIds = _breadcrumbFolderIds.Take(idx + 1).ToList();
                _breadcrumbFolderIds.Clear();
                _breadcrumbFolderIds.AddRange(newIds);
                ApplyBreadcrumb(newBreadcrumb, newIds);
            }

            lblConfidentialityBadge.Text = ConfidentialityLabel(folder.ConfidentialityLevel);
            lblConfidentialityBadge.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
            lblContentInfo.Text = string.Format("กำลังดู: {0}", folder.FolderName);
            lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", folder.FolderName);
        }

        private TreeNode FindNodeByFolderId(TreeNodeCollection nodes, int folderId)
        {
            foreach (TreeNode node in nodes)
            {
                var folder = node.Tag as Folder;
                if (folder != null && folder.FolderId == folderId)
                {
                    return node;
                }

                // Search in child nodes
                var found = FindNodeByFolderId(node.Nodes, folderId);
                if (found != null) return found;
            }
            return null;
        }

        private Folder GetFolderById(int folderId)
        {
            // Try to get folder from current tree first
            var node = FindNodeByFolderId(folderTreeView.Nodes, folderId);
            if (node?.Tag is Folder folder)
            {
                return folder;
            }

            // If not found, could query from service (but this might be expensive)
            // For now, return null
            return null;
        }

        private void NavigateToOrgUnitRoot()
        {
            // Clear folder tree selection and show OrgUnit root folders
            folderTreeView.AfterSelect -= folderTreeView_AfterSelect;
            folderTreeView.SelectedNode = null;
            folderTreeView.AfterSelect += folderTreeView_AfterSelect;

            _presenter.ShowOrgUnitRootFolders(_selectedOrgUnitId);

            // Update breadcrumb to OrgUnit only
            string orgUnitPath = "🏠 PC";
            if (!string.IsNullOrEmpty(_selectedOrgUnitName) && _selectedOrgUnitName != "— ทั้งหมด —")
            {
                orgUnitPath += "  ›  " + _selectedOrgUnitName;
            }

            PushNavigation(null); // null = OrgUnit root
            _currentNavFolderId = null;
            ApplyBreadcrumb(orgUnitPath, new List<int?>());
            UpdateNavButtons();
            lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", _selectedOrgUnitName);
        }

        private TreeNode FindNodeByName(TreeNodeCollection nodes, string folderName)
        {
            foreach (TreeNode node in nodes)
            {
                var folder = node.Tag as Folder;
                if (folder != null &&
                    (folder.FolderName == folderName || folder.Name == folderName ||
                     node.Text.Contains(folderName)))
                {
                    return node;
                }

                // Search in child nodes
                var found = FindNodeByName(node.Nodes, folderName);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>ตัด segment สุดท้ายออกจาก breadcrumb path (ใช้ปุ่ม Up)</summary>
        private static string TrimLastBreadcrumbSegment(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            int idx = path.LastIndexOf("  ›  ");
            return idx > 0 ? path.Substring(0, idx) : path;
        }

        /// <summary>Push เพิ่มเข้า back stack (เรียกเมื่อ user navigate ด้วยตัวเอง ไม่ใช่ปุ่ม back/fwd)</summary>
        private void PushNavigation(int? newFolderId)
        {
            if (_isNavigating) return;
            // ไม่ push initial state (null, "") — ไม่มีประโยชน์ที่จะ back ไปหน้าว่าง
            if (_currentNavFolderId == null && string.IsNullOrEmpty(_currentBreadcrumb)) return;
            // ไม่ push ถ้า current state เหมือนกับ top ของ stack (ป้องกัน duplicate entries)
            if (_backStack.Count > 0 && _backStack.Peek() == _currentNavFolderId
                && _backBreadcrumbs.Count > 0 && _backBreadcrumbs.Peek() == _currentBreadcrumb)
                return;
            // Push ค่า current (เก่า) เข้า back stack ก่อนที่ caller จะ update _currentNavFolderId
            _backStack.Push(_currentNavFolderId);
            _backBreadcrumbs.Push(_currentBreadcrumb);
            _forwardStack.Clear();
            _forwardBreadcrumbs.Clear();
            // ไม่ set _currentNavFolderId ที่นี่ — caller จัดการเอง
        }

        private void UpdateNavButtons()
        {
            btnNavBack.Enabled = _backStack.Count > 0;
            btnNavForward.Enabled = _forwardStack.Count > 0;
            btnNavUp.Enabled = _currentNavFolderId.HasValue;
        }

        /// <summary>
        /// Collects FolderIds of all currently expanded tree nodes.
        /// </summary>
        private void CollectExpandedNodes(TreeNodeCollection nodes, HashSet<int> expandedIds)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.IsExpanded)
                {
                    var f = node.Tag as Folder;
                    if (f != null) expandedIds.Add(f.FolderId);
                }
                if (node.Nodes.Count > 0)
                    CollectExpandedNodes(node.Nodes, expandedIds);
            }
        }

        /// <summary>
        /// Re-expands nodes whose FolderIds are in the provided set (restores previous expanded state).
        /// </summary>
        private void RestoreExpandedNodes(TreeNodeCollection nodes, HashSet<int> expandedIds)
        {
            foreach (TreeNode node in nodes)
            {
                var f = node.Tag as Folder;
                if (f != null && expandedIds.Contains(f.FolderId))
                    node.Expand();
                if (node.Nodes.Count > 0)
                    RestoreExpandedNodes(node.Nodes, expandedIds);
            }
        }

        /// <summary>
        /// Auto-expands all nodes up to the specified depth level.
        /// Level 0 nodes are expanded to show Level 1 children, etc.
        /// Equivalent to FolderPickerDialog.ExpandToLevel but for folderTreeView.
        /// </summary>
        private static void ExpandTreeToLevel(TreeNodeCollection nodes, int current, int maxLevel)
        {
            if (current >= maxLevel) return;
            foreach (TreeNode node in nodes)
            {
                node.Expand();
                if (node.Nodes.Count > 0)
                    ExpandTreeToLevel(node.Nodes, current + 1, maxLevel);
            }
        }

        /// <summary>
        /// Walks the tree nodes to find and re-select the node with the given folderId.
        /// </summary>
        private bool RestoreSelectedNode(TreeNodeCollection nodes, int folderId)
        {
            foreach (TreeNode node in nodes)
            {
                var f = node.Tag as Folder;
                if (f != null && f.FolderId == folderId)
                {
                    folderTreeView.SelectedNode = node;
                    node.EnsureVisible();
                    return true;
                }
                if (RestoreSelectedNode(node.Nodes, folderId))
                    return true;
            }
            return false;
        }

        // -----------------------------------------------------------------------
        // Toolbar button handlers
        // -----------------------------------------------------------------------

        private void btnNewFolder_Click(object sender, EventArgs e)
        {
            // ใช้ folder ที่กำลังดูใน content panel เป็น parent ก่อน
            // ถ้าไม่มี (_currentNavFolderId == null) ค่อย fallback ไปที่ tree selection
            Folder parent = null;
            if (_currentNavFolderId.HasValue)
            {
                var node = FindNodeByFolderId(folderTreeView.Nodes, _currentNavFolderId.Value);
                if (node != null) parent = node.Tag as Folder;
                if (parent == null) parent = _lastSelectedFolder;
            }
            if (parent == null) parent = SelectedFolder;

            // ถ้า content panel อยู่ที่ root level → ห้ามสร้าง folder จาก context menu ของ content
            if (!_currentNavFolderId.HasValue && parent != null
                && (!parent.ParentFolderID.HasValue || parent.ParentFolderID == 0))
            {
                ShowError("กรุณาเปิด Folder ก่อนสร้าง Sub-folder\nไม่สามารถสร้าง Folder ที่ระดับ Root ได้จากรายการนี้");
                return;
            }

            if (parent == null)
            {
                if (!_selectedOrgUnitId.HasValue) return;

                if (HasAuto4RootFolders())
                {
                    // มี 4 folder หลักแล้ว → ต้องสร้าง sub-folder ภายใต้ 1HC/2SC/3NC/4PC เท่านั้น
                    ShowError("กรุณาเลือก Folder ภายใต้ 1HC, 2SC, 3NC หรือ 4PC ก่อนสร้าง Sub-folder");
                    return;
                }

                // ยังไม่มี 4 folder หลัก → สร้างก่อน
                string baseName = PromptForBaseName("สร้าง 4 Folder หลัก (1HC, 2SC, 3NC, 4PC)");
                if (baseName == null) return;
                CreateAuto4Folders(baseName, null, _selectedOrgUnitId.Value);
                return;
            }

            // มี parent folder ถูกเลือกอยู่
            if (HasAuto4RootFolders())
            {
                // มี Auto4 root folders → ต้องสร้างภายใต้ 4 folders เท่านั้น
                if (IsAuto4Folder(parent))
                {
                    // parent คือ 1HC/2SC/3NC/4PC → สร้าง sub-folder ได้ทันที ล็อค level ตาม parent
                    CreateSubFolderWithChoice(parent, parent.ConfidentialityLevel);
                }
                else if (IsUnderAuto4Folder(parent))
                {
                    // parent อยู่ภายใต้ Auto4 → สร้าง sub-folder ได้ ล็อค level ตาม parent
                    CreateSubFolderWithChoice(parent, parent.ConfidentialityLevel);
                }
                else
                {
                    // parent ไม่ได้อยู่ภายใต้ Auto4 → ห้ามสร้าง
                    ShowError("ไม่สามารถสร้าง Folder ได้\nกรุณาเลือก Folder ภายใต้ 1HC, 2SC, 3NC หรือ 4PC ก่อนสร้าง");
                }
                return;
            }

            // ไม่มี Auto4 root folders → สร้างได้ทั้ง root และ sub-folder
            var mainChoice = MessageBox.Show(
                string.Format("ต้องการสร้าง Folder ใหม่ที่ไหน?\n\n[Yes]  ใต้ \"{0}\" (Sub-folder)\n[No]   ระดับ Root (Root Folder)", parent.FolderName),
                "สร้าง Folder ใหม่",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (mainChoice == DialogResult.Cancel) return;

            if (mainChoice == DialogResult.No)
            {
                if (!_selectedOrgUnitId.HasValue) return;
                string folderName; int folderLevel;
                if (!PromptForNameAndLevel("สร้าง Root Folder", out folderName, out folderLevel)) return;
                _presenter.CreateRootFolder(folderName, _selectedOrgUnitId.Value, folderLevel);
            }
            else
            {
                CreateSubFolderWithChoice(parent, null);
            }
        }

        /// <summary>
        /// ถามผู้ใช้ว่าต้องการสร้าง Sub-folder แบบ Blank หรือจาก Template
        /// ถ้า template controller ใช้งานได้ จะแสดงตัวเลือก Template ให้
        /// </summary>
        private void CreateSubFolderWithChoice(Folder parent, int? lockedLevel)
        {
            // ถ้า template controller ใช้งานได้ → เสนอทางเลือก
            if (_folderTemplateController != null && _folderTemplateController.IsAvailable && _folderCommandController != null && _orgLookupController != null)
            {
                using (var dlg = new FolderCreationChoiceDialog(parent.FolderName))
                {
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;

                    if (dlg.SelectedChoice == FolderCreationChoiceDialog.Choice.Template)
                    {
                        OpenApplyTemplateFor(parent);
                        return;
                    }
                    // Choice.Blank → fall through to blank folder creation below
                }
            }

            // Blank folder
            string folderName; int folderLevel;
            if (!PromptForNameAndLevel(
                string.Format("สร้าง Folder ใหม่ใต้ \"{0}\"", parent.FolderName),
                out folderName, out folderLevel, lockedLevel)) return;
            _presenter.CreateFolder(parent.FolderId, folderName, parent.OrgUnitId, folderLevel);
        }

        /// <summary>
        /// เปิด ApplyTemplateDialog โดยกำหนด parent folder ไว้ล่วงหน้า
        /// </summary>
        private void OpenApplyTemplateFor(Folder parent)
        {
            using (var dlg = new ApplyTemplateDialog(_folderTemplateController, _orgLookupController, _user, parent, _currentBreadcrumb))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _presenter.LoadFolderTree(_selectedOrgUnitId);
                    _presenter.GetFolderContents(parent.FolderId);
                    lblContentInfo.Text = string.Format("กำลังดู: {0}", parent.FolderName);
                    lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", parent.FolderName);
                    ShowInfo("✅ สร้าง Folder จาก Template เรียบร้อยแล้ว");
                }
            }
        }

        private void btnRootFolder_Click(object sender, EventArgs e)
        {
            if (!_selectedOrgUnitId.HasValue) return;

            if (HasAuto4RootFolders())
            {
                ShowError("ไม่สามารถสร้าง Root Folder ได้\nเนื่องจากมี 1HC/2SC/3NC/4PC อยู่แล้ว");
                return;
            }

            string folderName;
            int level;
            if (!PromptForNameAndLevel("สร้าง Root Folder", out folderName, out level)) return;

            _presenter.CreateRootFolder(folderName, _selectedOrgUnitId.Value, level);
        }

        private void btnAuto4Folders_Click(object sender, EventArgs e)
        {
            try
            {
                var selected = SelectedFolder;

                // Debug information
                if (folderTreeView.SelectedNode == null)
                {
                    // ไม่มี folder เลือก - ลองสร้าง root folders
                    if (!_selectedOrgUnitId.HasValue)
                    {
                        ShowError("กรุณาเลือกหน่วยงานก่อนสร้าง Auto 4 Folders");
                        return;
                    }

                    string baseName = "";
                    CreateAuto4FoldersQuiet(baseName, null, _selectedOrgUnitId.Value);
                    return;
                }

                if (selected == null)
                {
                    ShowError("ไม่สามารถดึงข้อมูล folder ที่เลือกได้ กรุณาลองใหม่");
                    return;
                }

                // ตรวจสอบว่า folder ที่เลือกเป็น Auto 4 Folder หรือไม่
                bool isAuto4Folder = IsAuto4Folder(selected);

                if (isAuto4Folder)
                {
                    // ถ้าเป็น Auto 4 Folder ให้สร้างที่ระดับเดียวกัน (sibling) แทนที่จะเป็น child
                    string baseName = GetCleanBaseName(selected);
                    CreateAuto4FoldersQuiet(baseName, selected.ParentFolderID, selected.OrgUnitId);
                }
                else
                {
                    // ถ้าไม่ใช่ Auto 4 Folder ให้สร้างเป็น child ปกติ
                    string baseName = selected.FolderName ?? selected.Name ?? "";
                    CreateAuto4FoldersQuiet(baseName, selected.FolderId, selected.OrgUnitId);
                }
            }
            catch (Exception ex)
            {
                ShowError($"เกิดข้อผิดพลาด: {ex.Message}");
            }
        }

        /// <summary>
        /// แสดงข้อความถามผู้ใช้ว่าต้องการสร้างโฟลเดอร์แรกหรือไม่ เมื่อเลือก OrgUnit ที่ไม่มีโฟลเดอร์
        /// </summary>
        private void PromptToCreateFirstFolder()
        {
            // ป้องกัน re-entry ระหว่างกำลังสร้าง Auto 4 Folders อยู่
            if (_suppressCreatePrompt) return;
            if (_isCreatePromptOpen) return;
            if (!_selectedOrgUnitId.HasValue) return;

            // ตรวจสอบว่ามี 4 folder หลัก (1HC/2SC/3NC/4PC) หรือยัง
            bool has4Folders = HasAuto4RootFolders();
            if (has4Folders) return; // มีแล้ว ไม่ต้องถาม

            // ถ้ามี HC folder รอ approve อยู่ → ไม่ถามซ้ำ แสดงสถานะแทน
            if (_pendingHCApproval)
            {
                lblOrgUnitStatus.Text = "⏳ กำลังรอ Admin อนุมัติ 1HC Folder — จะแสดงหลังจากอนุมัติแล้ว";
                return;
            }
            // ตรวจสอบ DB โดยตรง (ป้องกัน stale tree จาก async OrgUnit AfterSelect)
            // ถ้า DB มี folder ใดๆ สำหรับ OrgUnit นี้ → ไม่ต้องถาม
            if (_folderReadLookupController != null)
            {
                try
                {
                    var dbFolders = _folderReadLookupController.GetAllFolders(_selectedOrgUnitId, _user);
                    if (dbFolders != null && dbFolders.Any(f => !f.IsDeleted && !f.IsArchived))
                        return; // มีอยู่แล้วใน DB
                }
                catch { /* ถ้าเช็ค DB ไม่ได้ ให้ผ่านไป */ }
            }

            try
            {
                _isCreatePromptOpen = true;

                string orgName = _selectedOrgUnitName ?? "หน่วยงานนี้";
                var result = MessageBox.Show(
                    string.Format(
                        "หน่วยงาน \"{0}\" ยังไม่มีโฟลเดอร์หลัก\n\n" +
                        "ต้องการสร้าง 4 Folder หลัก (1HC, 2SC, 3NC, 4PC) หรือไม่?\n\n" +
                        "หมายเหตุ: ระบบจะสร้างทั้ง 4 Folder ทันที",
                        orgName),
                    "สร้างโฟลเดอร์หลัก",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // suppress ทันทีเพื่อตัดวงจรถามซ้ำจาก event ที่วิ่งค้างอยู่
                    _suppressCreatePrompt = true;
                    CreateAuto4FoldersQuiet("", null, _selectedOrgUnitId.Value);
                }
            }
            finally
            {
                _isCreatePromptOpen = false;
            }
        }

        // -----------------------------------------------------------------------
        // Upload — เปิด FileUploadForm ด้วย folder ที่เลือกอยู่
        // -----------------------------------------------------------------------

        /// <summary>
        /// เปิด FileUploadForm สำหรับ folder ที่เลือกอยู่ใน tree
        /// เรียกได้จาก MDI ribbon หรือ context menu
        /// </summary>
        public void OpenUploadForSelectedFolder()
        {
            // ห้าม upload เมื่ออยู่ที่ root level (content panel แสดง 4 folder หลัก)
            if (!_currentNavFolderId.HasValue)
            {
                ShowError("กรุณาเปิด Folder ก่อนอัปโหลด\nไม่สามารถอัปโหลดไฟล์ที่ระดับ Root ได้");
                return;
            }

            // หา folder ที่กำลังดูอยู่จริงๆ จาก _currentNavFolderId ก่อน
            Folder folder = null;

            if (_currentNavFolderId.HasValue)
            {
                var node = FindNodeByFolderId(folderTreeView.Nodes, _currentNavFolderId.Value);
                if (node != null)
                    folder = node.Tag as Folder;

                if (folder == null && _lastSelectedFolder != null
                    && _lastSelectedFolder.FolderId == _currentNavFolderId.Value)
                    folder = _lastSelectedFolder;
            }

            if (folder == null)
                folder = SelectedFolder;

            if (folder == null)
            {
                ShowError("กรุณาเลือก Folder ปลายทางก่อนอัปโหลด");
                return;
            }

            if (_fileUploadController == null)
            {
                ShowError("Upload service is not available.");
                return;
            }

            // เก็บ folder ID ที่กำลังดูอยู่ก่อน upload
            int uploadFolderId = folder.FolderId;

            var uploadForm = new FileUploadForm(_fileUploadController, _user, folder, _uploadCheckpointController, _auditLogController);
            var result = uploadForm.ShowDialog(this.MdiParent ?? this);

            if (result == DialogResult.OK)
            {
                _lastSelectedFolder = folder;
                RefreshCurrentViewAfterFileMutation(uploadFolderId);
            }
        }

        private void OpenUploadWithFiles(string[] paths)
        {
            if (!_currentNavFolderId.HasValue) return;

            Folder folder = null;
            var node = FindNodeByFolderId(folderTreeView.Nodes, _currentNavFolderId.Value);
            if (node != null) folder = node.Tag as Folder;
            if (folder == null && _lastSelectedFolder != null
                && _lastSelectedFolder.FolderId == _currentNavFolderId.Value)
                folder = _lastSelectedFolder;
            if (folder == null) folder = SelectedFolder;

            if (folder == null || _fileUploadController == null) return;

            int uploadFolderId = folder.FolderId;
            var uploadForm = new FileUploadForm(
                _fileUploadController, _user, folder,
                _uploadCheckpointController, _auditLogController,
                initialFiles: paths);

            var result = uploadForm.ShowDialog(this.MdiParent ?? this);
            if (result == DialogResult.OK)
            {
                _lastSelectedFolder = folder;
                RefreshCurrentViewAfterFileMutation(uploadFolderId);
            }
        }

        private void btnUploadFile_Click(object sender, EventArgs e)
        {
            OpenUploadForSelectedFolder();
        }

        private void OpenUploadForFolder(Folder folder, string[] paths)
        {
            if (folder == null || _fileUploadController == null || paths == null || paths.Length == 0)
                return;

            var uploadForm = new FileUploadForm(
                _fileUploadController, _user, folder,
                _uploadCheckpointController, _auditLogController,
                initialFiles: paths);

            var result = uploadForm.ShowDialog(this.MdiParent ?? this);
            if (result == DialogResult.OK)
            {
                _lastSelectedFolder = folder;
                RefreshCurrentViewAfterFileMutation(folder.FolderId);
            }
        }

        /// <summary>
        /// จัดการ folder drop แบบ structured:
        ///   createRootFolder = true  → สร้าง CMS folder ชื่อเดียวกับ source folder ก่อน
        ///   createSubFolders = true  → สร้าง sub-folder ตาม tree ภายใน
        /// </summary>
        private void ExecuteStructuredDrop(
            string[]                                              sourceFolders,
            Dictionary<string, Dictionary<string, List<string>>> perSourceFiles,
            bool                                                  createRootFolder,
            bool                                                  createSubFolders)
        {
            Folder cmsBase = GetCurrentNavFolder();
            if (cmsBase == null || _fileUploadController == null) return;

            // ── Direct files (not from any source folder) ────────────────────────
            if (perSourceFiles.TryGetValue("", out var directDict))
            {
                var directFiles = directDict.SelectMany(kv => kv.Value).ToArray();
                if (directFiles.Length > 0)
                    OpenUploadForFolder(cmsBase, directFiles);
            }

            // ── Files from each source folder ────────────────────────────────────
            foreach (var sf in sourceFolders)
            {
                string sfName = Path.GetFileName(
                    sf.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                if (!perSourceFiles.TryGetValue(sfName, out var sfDict) || sfDict.Count == 0)
                    continue;

                // Determine root CMS folder for this source
                Folder sfRoot = cmsBase;
                if (createRootFolder)
                    sfRoot = CreateCmsFolderSafe(sfName, cmsBase) ?? cmsBase;

                if (!createSubFolders)
                {
                    // Flat: all files in this source folder → sfRoot
                    OpenUploadForFolder(sfRoot, sfDict.SelectMany(kv => kv.Value).ToArray());
                    continue;
                }

                // Build sub-folder map under sfRoot
                var folderMap = new Dictionary<string, Folder>(StringComparer.OrdinalIgnoreCase)
                {
                    [""] = sfRoot
                };

                var subPaths = sfDict.Keys
                    .Where(k => !string.IsNullOrEmpty(k))
                    .OrderBy(k => k.Split(new[] { Path.DirectorySeparatorChar,
                                                  Path.AltDirectorySeparatorChar }).Length)
                    .ThenBy(k => k);

                foreach (var relPath in subPaths)
                {
                    var segments = relPath.Split(
                        new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                        StringSplitOptions.RemoveEmptyEntries);

                    string curRel  = "";
                    Folder parent  = sfRoot;

                    foreach (var seg in segments)
                    {
                        string childRel = string.IsNullOrEmpty(curRel)
                            ? seg
                            : curRel + Path.DirectorySeparatorChar + seg;

                        if (!folderMap.TryGetValue(childRel, out Folder child))
                        {
                            child = CreateCmsFolderSafe(seg, parent) ?? parent;
                            folderMap[childRel] = child;
                        }
                        parent = child;
                        curRel = childRel;
                    }
                }

                foreach (var kv in sfDict)
                {
                    if (kv.Value == null || kv.Value.Count == 0) continue;
                    Folder target = folderMap.TryGetValue(kv.Key, out Folder f) ? f : sfRoot;
                    OpenUploadForFolder(target, kv.Value.ToArray());
                }
            }
        }

        private Folder CreateCmsFolderSafe(string name, Folder parent)
        {
            if (_folderCommandController == null) return null;
            try
            {
                var req = new CreateFolderRequest
                {
                    Name                 = name,
                    ParentFolderId       = parent.FolderId,
                    OrgUnitId            = parent.OrgUnitId,
                    ConfidentialityLevel = parent.ConfidentialityLevel
                };
                return _folderCommandController.CreateFolder(req, _user);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("ไม่สามารถสร้าง Folder \"{0}\" ได้\n{1}\n\nไฟล์จะถูกอัปโหลดไปที่ Folder หลักแทน",
                        name, ex.Message),
                    "สร้าง Folder ไม่สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private Folder GetCurrentNavFolder()
        {
            if (!_currentNavFolderId.HasValue) return null;
            var node   = FindNodeByFolderId(folderTreeView.Nodes, _currentNavFolderId.Value);
            var folder = node?.Tag as Folder;
            if (folder == null && _lastSelectedFolder?.FolderId == _currentNavFolderId.Value)
                folder = _lastSelectedFolder;
            if (folder == null)
                folder = SelectedFolder;
            return folder;
        }

        // -----------------------------------------------------------------------
        // Move — BtnMove_Click, TriggerMove, helpers (Req 9.1, 9.2, 9.3, 9.4, 10.1–10.4)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Updates btnMove enabled state based on current selection.
        /// Disabled when neither a Folder nor a FileDocument is selected.
        /// </summary>
        private void UpdateMoveButtonState()
        {
            bool folderSelected = folderTreeView.SelectedNode != null
                                  && folderTreeView.SelectedNode.Tag is Folder;
            bool fileSelected = contentsListView.SelectedItems.Count > 0
                                  && contentsListView.SelectedItems.Cast<ListViewItem>()
                                     .Any(i => i.Tag is FileDocument);
            btnMove.Enabled = folderSelected || fileSelected;
        }

        /// <summary>
        /// Returns true when a Folder or FileDocument is currently selected,
        /// allowing MDI_Container to enable/disable menuItemMove and btnRibbonMove.
        /// </summary>
        public bool HasActiveSelection
        {
            get
            {
                bool folderSelected = folderTreeView.SelectedNode != null
                                      && folderTreeView.SelectedNode.Tag is Folder;
                bool fileSelected = contentsListView.SelectedItems.Count > 0
                                      && contentsListView.SelectedItems.Cast<ListViewItem>()
                                         .Any(i => i.Tag is FileDocument);
                return folderSelected || fileSelected;
            }
        }

        /// <summary>
        /// Public entry point for MDI_Container to trigger Move on the active FolderForm.
        /// </summary>
        public void TriggerMove()
        {
            BtnMove_Click(this, EventArgs.Empty);
        }

        /// <summary>Public entry point for MDI_Container — Admin tab: Archive</summary>
        public void TriggerArchive()
        {
            btnArchive_Click(this, EventArgs.Empty);
        }

        /// <summary>Public entry point for MDI_Container — Admin tab: เปลี่ยนระดับ</summary>
        public void TriggerChangeLevel()
        {
            btnChangeLevel_Click(this, EventArgs.Empty);
        }

        /// <summary>Public entry point for MDI_Container — Admin tab: คัดลอกโครงสร้าง</summary>
        public void TriggerCopyStructure()
        {
            BtnCopyStructure_Click(this, EventArgs.Empty);
        }

        /// <summary>Public entry point for MDI_Container — สร้าง Folder จาก Template</summary>
        public void TriggerApplyTemplate()
        {
            btnApplyTemplate_Click(this, EventArgs.Empty);
        }

        /// <summary>
        /// Reload folder tree — เรียกจาก MDI_Container หลัง ApprovalInboxForm ปิด
        /// เพื่อแสดง 1HC folder ที่เพิ่งได้รับการอนุมัติ
        /// </summary>
        public void ReloadFolderTree()
        {
            _pendingHCApproval = false; // ล้าง flag
            _currentNavFolderId = null;
            _lastSelectedFolder = null;
            _presenter.LoadFolderTree(_selectedOrgUnitId);
            if (_selectedOrgUnitId.HasValue)
                lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", _selectedOrgUnitName ?? "—");
        }

        private IEnumerable<CMS.Domain.Models.OrgUnit> GetOrgUnits()
        {
            if (_orgLookupController == null) return Enumerable.Empty<CMS.Domain.Models.OrgUnit>();
            try { return _orgLookupController.GetHierarchy(); }
            catch { return Enumerable.Empty<CMS.Domain.Models.OrgUnit>(); }
        }

        /// <summary>
        /// Refresh tree + current content view to keep folder/file counts in sync after move.
        /// </summary>
        private void RefreshCurrentViewAfterMove()
        {
            _presenter.LoadFolderTree(_selectedOrgUnitId);

            if (_currentNavFolderId.HasValue)
            {
                _presenter.GetFolderContents(_currentNavFolderId.Value);
                return;
            }

            if (SelectedFolder != null)
            {
                _presenter.GetFolderContents(SelectedFolder.FolderID);
                return;
            }

            _presenter.ShowOrgUnitRootFolders(_selectedOrgUnitId);
        }

        /// <summary>
        /// Refresh tree + current content view to keep folder file counts in sync after add/delete.
        /// </summary>
        private void RefreshCurrentViewAfterFileMutation(int folderId)
        {
            _isNavigating = true;
            try
            {
                _presenter.LoadFolderTree(_selectedOrgUnitId);
            }
            finally
            {
                _isNavigating = false;
            }

            _currentNavFolderId = folderId;
            _presenter.GetFolderContents(folderId);

            // Sync tree selection and breadcrumb to match folderId after tree reload
            bool found = RestoreSelectedNode(folderTreeView.Nodes, folderId);
            SetActiveNode(folderTreeView.SelectedNode);
            if (found && folderTreeView.SelectedNode != null)
            {
                string bc = _selectedOrgUnit != null
                    ? BuildBreadcrumbPathFromOrgUnit(_selectedOrgUnit, folderTreeView.SelectedNode)
                    : BuildBreadcrumbPath(treeOrgUnit.SelectedNode, folderTreeView.SelectedNode);
                var parts = bc.Split(new[] { "  ›  " }, StringSplitOptions.None);
                var ids = new List<int?>();
                for (int i = 0; i < parts.Length - 1; i++)
                    ids.Add(i < _breadcrumbFolderIds.Count ? _breadcrumbFolderIds[i] : (int?)null);
                ids.Add(folderId);
                _breadcrumbFolderIds.Clear();
                _breadcrumbFolderIds.AddRange(ids);
                ApplyBreadcrumb(bc, ids);
                UpdateNavButtons();
            }

            var navFolder = _lastSelectedFolder != null && _lastSelectedFolder.FolderId == folderId
                ? _lastSelectedFolder
                : GetCurrentNavFolder();
            if (navFolder != null)
            {
                lblConfidentialityBadge.Text = ConfidentialityLabel(navFolder.ConfidentialityLevel);
                lblConfidentialityBadge.ForeColor = ConfidentialityColor(navFolder.ConfidentialityLevel);
                lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", navFolder.FolderName);
            }
        }

        private void BtnMove_Click(object sender, EventArgs e)
        {
            // Case 1: ถ้ามีไฟล์ถูกเลือกใน contentsListView ให้ move file
            var selectedFileIds = contentsListView.SelectedItems.Cast<ListViewItem>()
                .Select(i => i.Tag as FileDocument)
                .Where(f => f != null)
                .Select(f => f.FileID)
                .ToList();

            if (selectedFileIds.Any())
            {
                if (_fileCommandController == null)
                {
                    ShowError("File command controller ไม่พร้อมใช้งาน กรุณาติดต่อผู้ดูแลระบบ");
                    return;
                }

                using (var picker = new FolderPickerDialog())
                {
                    picker.LoadFolders(_folderReadLookupController.GetAccessibleFolders(null, _user), new HashSet<int>(), GetOrgUnits());
                    if (picker.ShowDialog(this) == DialogResult.OK && picker.SelectedFolderId.HasValue)
                    {
                        try
                        {
                            _fileCommandController.MoveFiles(new MoveFileRequest
                            {
                                FileIds = selectedFileIds,
                                DestinationFolderId = picker.SelectedFolderId.Value
                            }, _user);
                            MessageBox.Show(
                                string.Format("ย้าย {0} ไฟล์ สำเร็จ", selectedFileIds.Count), "สำเร็จ",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            contentsListView.SelectedItems.Clear();
                            RefreshCurrentViewAfterMove();
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            ShowError("ไม่มีสิทธิ์: " + ex.Message);
                        }
                        catch (InvalidOperationException ex)
                        {
                            ShowError(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            ShowUnexpectedError(ex);
                        }
                    }
                }
                return;
            }

            // Case 2: ถ้ามี Folder ถูกเลือกใน contentsListView ให้ move folder นั้น
            var selectedFolderFromList = contentsListView.SelectedItems.Count > 0
                ? contentsListView.SelectedItems[0].Tag as Folder : null;

            if (selectedFolderFromList != null)
            {
                var descendants = _folderCommandController.GetDescendants(selectedFolderFromList.FolderID);
                var excluded = new HashSet<int>(descendants.Select(d => d.FolderID));
                excluded.Add(selectedFolderFromList.FolderID);

                using (var picker = new FolderPickerDialog())
                {
                    picker.SourceFolderId = selectedFolderFromList.FolderID;
                    picker.LoadFolders(_folderReadLookupController.GetAccessibleFolders(null, _user), excluded, GetOrgUnits());
                    if (picker.ShowDialog(this) == DialogResult.OK && picker.SelectedFolderId.HasValue)
                    {
                        try
                        {
                            _folderCommandController.MoveFolder(new MoveFolderRequest
                            {
                                SourceFolderId = selectedFolderFromList.FolderID,
                                DestinationParentFolderId = picker.SelectedFolderId.Value
                            }, _user);
                            MessageBox.Show("ย้าย Folder สำเร็จ", "สำเร็จ",
                                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Reload tree ของ OrgUnit ปลายทาง (อาจต่างจาก OrgUnit ต้นทาง)
                            // ใช้ null เพื่อ reload ทั้งหมดให้ครอบคลุมทั้ง 2 OrgUnit
                            _selectedOrgUnitId = null;
                            _selectedOrgUnitName = "— ทั้งหมด —";
                            RefreshCurrentViewAfterMove();
                        }
                        catch (UnauthorizedAccessException ex) { ShowError("ไม่มีสิทธิ์: " + ex.Message); }
                        catch (InvalidOperationException ex) { ShowError(ex.Message); }
                        catch (Exception ex) { ShowUnexpectedError(ex); }
                    }
                }
                return;
            }

            // Case 3: ไม่มีอะไรเลือกใน list → ดู Folder ที่เลือกใน folderTreeView
            var selectedFolder = folderTreeView.SelectedNode != null
                ? folderTreeView.SelectedNode.Tag as Folder : null;

            if (selectedFolder != null)
            {
                var descendants = _folderCommandController.GetDescendants(selectedFolder.FolderID);
                var excluded = new HashSet<int>(descendants.Select(d => d.FolderID));
                excluded.Add(selectedFolder.FolderID);

                using (var picker = new FolderPickerDialog())
                {
                    picker.SourceFolderId = selectedFolder.FolderID;
                    picker.LoadFolders(_folderReadLookupController.GetAccessibleFolders(null, _user), excluded, GetOrgUnits());
                    if (picker.ShowDialog(this) == DialogResult.OK && picker.SelectedFolderId.HasValue)
                    {
                        try
                        {
                            _folderCommandController.MoveFolder(new MoveFolderRequest
                            {
                                SourceFolderId = selectedFolder.FolderID,
                                DestinationParentFolderId = picker.SelectedFolderId.Value
                            }, _user);
                            MessageBox.Show("ย้าย Folder สำเร็จ", "สำเร็จ",
                                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Reload ทั้งหมดเพื่อให้เห็น folder ในทั้ง OrgUnit ต้นทางและปลายทาง
                            _selectedOrgUnitId = null;
                            _selectedOrgUnitName = "— ทั้งหมด —";
                            RefreshCurrentViewAfterMove();
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            ShowError("ไม่มีสิทธิ์: " + ex.Message);
                        }
                        catch (InvalidOperationException ex)
                        {
                            ShowError(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            ShowUnexpectedError(ex);
                        }
                    }
                }
                return;
            }
        }

        private void ShowUnexpectedError(Exception ex)
        {
            MessageBox.Show(
                string.Format("เกิดข้อผิดพลาดที่ไม่คาดคิด: {0}", ex.Message),
                "ข้อผิดพลาด",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            // ถ้ามีรายการเลือกใน contentsListView → inline rename
            if (contentsListView.SelectedItems.Count > 0)
            {
                var selectedItem = contentsListView.SelectedItems[0];
                string currentName;
                var fileTag = selectedItem.Tag as FileDocument;
                var folderTag = selectedItem.Tag as Folder;

                if (fileTag != null)
                    currentName = fileTag.OriginalFileName;
                else if (folderTag != null)
                    currentName = folderTag.Name;
                else
                    return;

                StartInlineRename(selectedItem, currentName);
                return;
            }

            // ไม่มีรายการใน content list — ตรวจ folderTreeView
            var treeFolder = SelectedFolder;
            if (treeFolder == null)
            {
                ShowError("กรุณาเลือกไฟล์หรือโฟลเดอร์ที่ต้องการเปลี่ยนชื่อ");
                return;
            }

            string folderNewName = PromptForName("เปลี่ยนชื่อโฟลเดอร์", "ชื่อใหม่:", treeFolder.Name);
            if (folderNewName == null) return;
            _presenter.RenameFolder(treeFolder.FolderId, folderNewName);
        }

        // ── Inline rename (เหมือน Windows Explorer) ────────────────────────────

        private void StartInlineRename(ListViewItem item, string currentName)
        {
            // สร้าง TextBox ครั้งแรกที่ใช้
            if (_inlineRenameBox == null)
            {
                _inlineRenameBox = new TextBox
                {
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = contentsListView.Font,
                };
                _inlineRenameBox.KeyDown += InlineRenameBox_KeyDown;
                _inlineRenameBox.LostFocus += (s, ev) => CommitInlineRename();
                contentsListView.Controls.Add(_inlineRenameBox);
            }

            _renamingItem = item;

            // วาง TextBox ทับ label ของ item (ชื่อ column แรก)
            var labelBounds = item.GetBounds(ItemBoundsPortion.Label);
            int x = labelBounds.X + 2;
            int y = labelBounds.Y;
            int w = Math.Max(200, Math.Min(labelBounds.Width + 60,
                             contentsListView.ClientSize.Width - x - 4));
            int h = labelBounds.Height + 2;

            _inlineRenameBox.SetBounds(x, y, w, h);
            _inlineRenameBox.Text = currentName;
            _inlineRenameBox.SelectAll();
            _inlineRenameBox.Visible = true;
            _inlineRenameBox.BringToFront();
            _inlineRenameBox.Focus();
        }

        private void InlineRenameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.SuppressKeyPress = true;
                CommitInlineRename();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                CancelInlineRename();
            }
        }

        private void CommitInlineRename()
        {
            if (_inlineRenameBox == null || !_inlineRenameBox.Visible) return;

            string newName = _inlineRenameBox.Text.Trim();
            _inlineRenameBox.Visible = false;

            if (string.IsNullOrWhiteSpace(newName) || _renamingItem == null) return;

            var fileTag = _renamingItem.Tag as FileDocument;
            var folderTag = _renamingItem.Tag as Folder;
            _renamingItem = null;

            if (fileTag != null)
            {
                if (newName == fileTag.OriginalFileName) return;  // ไม่มีอะไรเปลี่ยน
                try
                {
                    _fileCommandController.RenameFile(fileTag.FileID, newName, _user);
                    var currentFolder = SelectedFolder;
                    if (currentFolder != null)
                        _presenter.GetFolderContents(currentFolder.FolderId);
                }
                catch (Exception ex) { ShowError("ไม่สามารถเปลี่ยนชื่อไฟล์ได้: " + ex.Message); }
            }
            else if (folderTag != null)
            {
                if (newName == folderTag.Name) return;  // ไม่มีอะไรเปลี่ยน
                _presenter.RenameFolder(folderTag.FolderId, newName);
                // Reload content list to show updated folder name
                if (_currentNavFolderId.HasValue)
                    _presenter.GetFolderContents(_currentNavFolderId.Value);
            }
        }

        private void CancelInlineRename()
        {
            if (_inlineRenameBox != null)
                _inlineRenameBox.Visible = false;
            _renamingItem = null;
        }

        private void btnArchive_Click(object sender, EventArgs e)
        {
            // Check if a file is selected first
            var file = SelectedFile;
            if (file != null)
            {
                ShowInfo("ฟีเจอร์ Archive ไฟล์จะพร้อมใช้งานในเวอร์ชันถัดไป");
                return;
            }

            // If no file selected, check for folder
            var folder = SelectedFolder;
            if (folder == null)
            {
                ShowError("กรุณาเลือกโฟลเดอร์ที่ต้องการ Archive");
                return;
            }

            var confirm = MessageBox.Show(
                string.Format("Archive '{0}' และโฟลเดอร์ย่อยทั้งหมดหรือไม่?", folder.Name),
                "ยืนยัน Archive",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
                _presenter.ArchiveFolder(folder.FolderId);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // ตรวจสอบ item ที่เลือกใน contentsListView ก่อน (File หรือ Folder ในรายการขวา)
            if (contentsListView.SelectedItems.Count > 0)
            {
                var selectedFiles = contentsListView.SelectedItems.Cast<ListViewItem>()
                    .Select(item => item.Tag as FileDocument)
                    .Where(f => f != null)
                    .GroupBy(f => f.FileDocumentId)
                    .Select(group => group.First())
                    .ToList();

                var selectedFolders = contentsListView.SelectedItems.Cast<ListViewItem>()
                    .Select(item => item.Tag as Folder)
                    .Where(f => f != null)
                    .GroupBy(f => f.FolderId)
                    .Select(group => group.First())
                    .ToList();

                if (selectedFiles.Count > 0 || selectedFolders.Count > 0)
                {
                    DeleteSelectedItems(selectedFiles, selectedFolders);
                    return;
                }
            }

            // ── Fallback: ไม่มีอะไรเลือกใน content → ลบ Folder ที่เลือกใน Tree ──
            var treeFolder = SelectedFolder;
            if (treeFolder != null)
            {
                DeleteSelectedFolder(treeFolder);
                return;
            }

            ShowError("กรุณาเลือกไฟล์หรือโฟลเดอร์ที่ต้องการลบ");
        }

        /// <summary>
        /// ลบหลาย items พร้อมกัน (files, folders หรือ mixed selection)
        /// </summary>
        private void DeleteSelectedItems(IList<FileDocument> files, IList<Folder> folders)
        {
            // ── กรณีมีแค่ไฟล์ → ใช้ logic เดิม ──────────────────────────────
            if (folders.Count == 0)
            {
                DeleteSelectedFiles(files);
                return;
            }

            // ── กรณีมี folder เดียว ไม่มีไฟล์ → ใช้ logic เดิม ───────────────
            if (files.Count == 0 && folders.Count == 1)
            {
                DeleteSelectedFolder(folders[0]);
                return;
            }

            // ── หลาย items ────────────────────────────────────────────────────
            // ตรวจสอบ folder ที่ห้ามลบ (root confidentiality folders)
            var undeletable = folders.Where(f => !CanDeleteFolder(f)).ToList();
            if (undeletable.Count > 0)
            {
                MessageBox.Show(
                    string.Format(
                        "Folder '{0}' เป็น Folder หลักของระบบ ไม่สามารถรวมในชุดลบนี้ได้\n\nกรุณาตรวจสอบการเลือกอีกครั้ง",
                        undeletable[0].Name),
                    "ไม่สามารถลบได้",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
                return;
            }

            var foldersWithFiles = folders.Where(f => f.FileCount > 0).ToList();
            var emptyFolders = folders.Where(f => f.FileCount == 0).ToList();

            // สร้างข้อความยืนยันรวม
            var sb = new System.Text.StringBuilder();
            if (files.Count > 0)
                sb.AppendLine(string.Format("• ไฟล์  {0} รายการ  → ย้ายไปถังขยะ", files.Count));
            if (emptyFolders.Count > 0)
                sb.AppendLine(string.Format("• Folder ว่าง  {0} รายการ  → ย้ายไปถังขยะ", emptyFolders.Count));
            if (foldersWithFiles.Count > 0)
                sb.AppendLine(string.Format("• Folder ที่มีไฟล์  {0} รายการ  → ยื่นคำขออนุมัติ", foldersWithFiles.Count));

            var confirm = MessageBox.Show(
                string.Format("ต้องการดำเนินการกับรายการที่เลือกต่อไปนี้หรือไม่?\n\n{0}", sb.ToString().TrimEnd()),
                "ยืนยันการลบหลายรายการ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            // 1. ลบไฟล์ (ไม่ต้อง refresh ระหว่างกลาง — จะ refresh รวมตอนท้าย)
            if (files.Count > 0 && _fileCommandController != null)
            {
                int deletedFiles = 0, failedFiles = 0;
                string firstFileError = null;
                foreach (var file in files)
                {
                    try
                    {
                        _fileCommandController.DeleteFile(file.FileDocumentId, _user);
                        deletedFiles++;
                    }
                    catch (Exception ex)
                    {
                        failedFiles++;
                        if (firstFileError == null)
                            firstFileError = ex.Message;
                    }
                }
                if (failedFiles > 0)
                    ShowError(string.Format("ลบไฟล์ไม่สำเร็จ {0} รายการ: {1}", failedFiles, firstFileError));
            }

            // 2. ลบ empty folders
            foreach (var folder in emptyFolders)
            {
                try { _presenter.DeleteFolder(folder.FolderId); }
                catch (Exception ex)
                {
                    ShowError(string.Format("ลบ Folder '{0}' ไม่สำเร็จ: {1}", folder.Name, ex.Message));
                }
            }

            // 3. Folder ที่มีไฟล์ → ยื่นคำขออนุมัติ
            if (foldersWithFiles.Count > 0)
            {
                if (_approvalFolderController == null)
                {
                    MessageBox.Show(
                        string.Format("Folder ที่มีไฟล์ {0} รายการ ต้องผ่านการอนุมัติจาก Admin", foldersWithFiles.Count),
                        "ต้องขออนุมัติก่อนลบ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    string reason = PromptForReason(
                        string.Format("กรุณาระบุเหตุผลในการลบ Folder ที่มีไฟล์ {0} รายการ:", foldersWithFiles.Count));
                    if (reason != null)
                    {
                        int approvalSent = 0;
                        foreach (var folder in foldersWithFiles)
                        {
                            try
                            {
                                _approvalFolderController.RequestFolderDelete(folder.FolderId, reason, _user);
                                approvalSent++;
                            }
                            catch (Exception ex)
                            {
                                ShowError(string.Format("ส่งคำขออนุมัติ Folder '{0}' ไม่สำเร็จ: {1}", folder.Name, ex.Message));
                            }
                        }
                        if (approvalSent > 0)
                            ShowInfo(string.Format("ส่งคำขออนุมัติลบ Folder {0} รายการเรียบร้อยแล้ว", approvalSent));
                    }
                }
            }

            // Refresh view
            if (_currentNavFolderId.HasValue)
                RefreshCurrentViewAfterFileMutation(_currentNavFolderId.Value);
            else if (files.Count > 0)
                RefreshCurrentViewAfterFileMutation(files[0].FolderID);
        }

        private void DeleteSelectedFile(FileDocument file)
        {
            DeleteSelectedFiles(new List<FileDocument> { file });
        }

        private void DeleteSelectedFiles(IList<FileDocument> files)
        {
            if (files == null || files.Count == 0)
            {
                ShowError("กรุณาเลือกไฟล์ที่ต้องการลบ");
                return;
            }

            if (_fileCommandController == null)
            {
                ShowError("File command controller is not available.");
                return;
            }

            string confirmMessage = files.Count == 1
                ? string.Format("ลบไฟล์ '{0}' หรือไม่?\n\nไฟล์จะถูกย้ายไปยังถังขยะ", files[0].OriginalFileName)
                : string.Format("ลบไฟล์ที่เลือกทั้งหมด {0} ไฟล์ หรือไม่?\n\nไฟล์จะถูกย้ายไปยังถังขยะ", files.Count);

            var confirm = MessageBox.Show(
                confirmMessage,
                files.Count == 1 ? "ยืนยันการลบไฟล์" : "ยืนยันการลบหลายไฟล์",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            int deleted = 0;
            int failed = 0;
            string firstError = null;

            foreach (var file in files)
            {
                try
                {
                    _fileCommandController.DeleteFile(file.FileDocumentId, _user);
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

            int? refreshFolderId = null;
            var currentFolder = SelectedFolder;
            if (currentFolder != null)
                refreshFolderId = currentFolder.FolderId;
            else if (files.Count > 0)
                refreshFolderId = files[0].FolderID;

            if (refreshFolderId.HasValue)
                RefreshCurrentViewAfterFileMutation(refreshFolderId.Value);

            if (failed == 0)
            {
                if (files.Count == 1)
                    ShowInfo(string.Format("ลบไฟล์ '{0}' เรียบร้อยแล้ว", files[0].OriginalFileName));
                else
                    ShowInfo(string.Format("ลบไฟล์เรียบร้อยแล้ว {0} ไฟล์", deleted));
                return;
            }

            MessageBox.Show(
                string.Format("ลบไฟล์เสร็จสิ้น\nสำเร็จ {0} ไฟล์\nล้มเหลว {1} ไฟล์\n\n{2}", deleted, failed, firstError ?? "กรุณาตรวจสอบรายการที่ลบไม่สำเร็จอีกครั้ง"),
                "ผลการลบไฟล์",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void DeleteSelectedFolder(Folder folder)
        {
            // 4 Folder หลัก (root folder ที่ ParentFolderID == null) ลบได้เฉพาะ Admin เท่านั้น
            if (!CanDeleteFolder(folder))
            {
                MessageBox.Show(
                    string.Format(
                        "Folder '{0}' เป็น Folder หลักของระบบ\n\nเฉพาะ Admin เท่านั้นที่มีสิทธิ์ลบ Folder หลัก\nกรุณาติดต่อ Admin หากต้องการดำเนินการ",
                        folder.Name),
                    "ไม่มีสิทธิ์ลบ Folder หลัก",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
                return;
            }

            // Folder ที่มีไฟล์อยู่ต้องผ่านการอนุมัติ
            if (folder.FileCount > 0)
            {
                if (_approvalFolderController == null)
                {
                    MessageBox.Show(
                        string.Format(
                            "Folder '{0}' มีไฟล์อยู่ {1} รายการ\nกรุณาติดต่อ Admin เพื่อขออนุมัติการลบ",
                            folder.Name, folder.FileCount),
                        "ต้องขออนุมัติก่อนลบ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    string.Format(
                        "Folder '{0}' มีไฟล์อยู่ {1} รายการ\n\n" +
                        "การลบ Folder ที่มีไฟล์ต้องผ่านการอนุมัติจาก Admin\n\n" +
                        "ต้องการยื่นคำขออนุมัติการลบหรือไม่?",
                        folder.Name, folder.FileCount),
                    "ต้องขออนุมัติก่อนลบ",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                string reason = PromptForReason(
                    string.Format("กรุณาระบุเหตุผลในการลบ Folder '{0}':", folder.Name));
                if (reason == null) return;

                try
                {
                    if (_approvalFolderController == null)
                    {
                        ShowError("Approval controller is not available.");
                        return;
                    }

                    _approvalFolderController.RequestFolderDelete(folder.FolderId, reason, _user);
                    ShowInfo(string.Format(
                        "ส่งคำขออนุมัติลบ Folder '{0}' เรียบร้อยแล้ว\n\n" +
                        "โปรดรอ Admin อนุมัติ ระบบจะลบ Folder โดยอัตโนมัติหลังได้รับการอนุมัติ",
                        folder.Name));
                }
                catch (Exception ex) { ShowError("ไม่สามารถส่งคำขออนุมัติได้: " + ex.Message); }
                return;
            }

            // Folder ว่างเปล่า → ย้ายเข้าถังขยะ
            var confirmDelete = MessageBox.Show(
                string.Format("ย้าย Folder '{0}' ไปยังถังขยะหรือไม่?", folder.Name),
                "ยืนยันการลบโฟลเดอร์",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmDelete == DialogResult.Yes)
                _presenter.DeleteFolder(folder.FolderId);
        }

        // -----------------------------------------------------------------------
        // Clear Folder — ลบไฟล์ทั้งหมดใน folder ปัจจุบัน (รวม sub-folders)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Soft-deletes ไฟล์ทุกไฟล์ใน folder ปัจจุบันและ sub-folders ทั้งหมด
        /// โดยไม่ลบ folder structure (เหลือแค่ folder เปล่า)
        /// เฉพาะ Manager/Admin เท่านั้น
        /// </summary>
        private void ClearFolderContents()
        {
            if (_fileBrowseController == null || _fileCommandController == null || !_currentNavFolderId.HasValue) return;

            int rootFolderId = _currentNavFolderId.Value;

            // Refresh the grid before counting so the dialog always matches what the user sees.
            // Without this, a stale grid (loaded before other users uploaded files) would show
            // a different count than the confirm dialog, causing confusion.
            _presenter.GetFolderContents(rootFolderId);

            // รวบรวม folder IDs ทั้งหมด (ตัวเองและ descendants)
            // sp_GetFolderDescendants คืน root folder ใน result ด้วย จึงต้อง Distinct() เพื่อกัน double-count
            var allFolderIds = new List<int> { rootFolderId };
            try
            {
                var descendants = _folderCommandController.GetDescendants(rootFolderId);
                if (descendants != null)
                    foreach (var d in descendants)
                        allFolderIds.Add(d.FolderID);
            }
            catch { /* ถ้า GetDescendants ไม่ได้ ใช้แค่ folder ปัจจุบัน */ }

            allFolderIds = allFolderIds.Distinct().ToList();

            // นับไฟล์ทั้งหมดก่อนแสดง confirm
            int totalFiles = 0;
            var filesByFolder = new Dictionary<int, List<FileDocument>>();
            foreach (int fid in allFolderIds)
            {
                try
                {
                    var files = _fileBrowseController.GetFilesInFolder(fid, _user).ToList();
                    if (files.Count > 0)
                    {
                        filesByFolder[fid] = files;
                        totalFiles += files.Count;
                    }
                }
                catch { }
            }

            if (totalFiles == 0)
            {
                MessageBox.Show("ไม่มีไฟล์ใน Folder นี้", "ล้าง Folder",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Confirm
            var confirm = MessageBox.Show(
                string.Format(
                    "ต้องการลบไฟล์ทั้งหมด {0} ไฟล์\nใน {1} Folder (รวม sub-folders) ใช่หรือไม่?\n\n" +
                    "⚠️  ไฟล์จะถูกย้ายไป Recycle Bin (Soft Delete)\nFolder structure จะยังคงอยู่",
                    totalFiles, filesByFolder.Count),
                "ยืนยันการล้าง Folder",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            // ลบทีละไฟล์
            int deleted = 0, failed = 0;
            foreach (var kvp in filesByFolder)
            {
                foreach (var file in kvp.Value)
                {
                    try
                    {
                        _fileCommandController.DeleteFile(file.FileID, _user);
                        deleted++;
                    }
                    catch
                    {
                        failed++;
                    }
                }
            }

            // Reload tree + content to keep counts in sync immediately
            RefreshCurrentViewAfterFileMutation(rootFolderId);

            string msg = failed == 0
                ? string.Format("ล้าง Folder สำเร็จ — ลบ {0} ไฟล์", deleted)
                : string.Format("ล้าง Folder เสร็จสิ้น — สำเร็จ {0} ไฟล์, ล้มเหลว {1} ไฟล์", deleted, failed);

            MessageBox.Show(msg, "ล้าง Folder",
                MessageBoxButtons.OK,
                failed == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        /// <summary>
        /// ตรวจสอบว่า user ปัจจุบันมีสิทธิ์ลบ folder นี้หรือไม่
        /// 4 Folder หลัก (root folder ที่ ParentFolderID == null) ลบได้เฉพาะ Admin เท่านั้น
        /// </summary>
        private bool CanDeleteFolder(Folder folder)
        {
            if (folder == null) return false;
            bool isRootFolder = !folder.ParentFolderID.HasValue || folder.ParentFolderID == 0;
            if (!isRootFolder) return true;
            return _user.AppRole >= (int)CMS.Domain.Enums.UserRole.Admin;
        }

        /// <summary>Shows a navy-styled dialog to collect a reason string. Returns null if user cancels.</summary>
        private string PromptForReason(string prompt)
        {
            Color navyDark = Color.FromArgb(25, 55, 105);
            Color navyLight = Color.FromArgb(215, 228, 248);
            Color bodyBg = Color.FromArgb(243, 246, 251);
            Color footerBg = Color.FromArgb(228, 234, 245);

            using (var dlg = new Form())
            {
                dlg.Text = "ระบุเหตุผล";
                dlg.ClientSize = new Size(420, 220);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = bodyBg;
                dlg.Font = new System.Drawing.Font("Segoe UI", 9f);

                // ── Header bar ────────────────────────────────────────────────
                var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = navyDark };
                var lblHeader = new Label
                {
                    Text = "🗑  ระบุเหตุผล",
                    Font = new System.Drawing.Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(14, 14),
                };
                pnlHeader.Controls.Add(lblHeader);

                // ── Sub-header hint bar ───────────────────────────────────────
                var lblHint = new Label
                {
                    Text = "  คำขออนุมัติจะถูกส่งให้ Admin พิจารณา",
                    Font = new System.Drawing.Font("Segoe UI", 8.5f),
                    ForeColor = Color.FromArgb(50, 70, 110),
                    BackColor = navyLight,
                    Size = new Size(420, 24),
                    Location = new Point(0, 52),
                    TextAlign = ContentAlignment.MiddleLeft,
                };

                // ── Body ──────────────────────────────────────────────────────
                var lblPrompt = new Label
                {
                    Text = prompt,
                    Font = new System.Drawing.Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(30, 40, 65),
                    Size = new Size(392, 36),
                    Location = new Point(14, 88),
                    AutoSize = false,
                };

                var txt = new TextBox
                {
                    Location = new Point(14, 128),
                    Size = new Size(392, 24),
                    Font = new System.Drawing.Font("Segoe UI", 9.5f),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                };

                // ── Footer bar ────────────────────────────────────────────────
                var pnlFooter = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 46,
                    BackColor = footerBg,
                };

                var btnOk = new Button
                {
                    Text = "ตกลง",
                    Size = new Size(88, 30),
                    Location = new Point(228, 8),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = navyDark,
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9f, FontStyle.Bold),
                    DialogResult = DialogResult.OK,
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text = "ยกเลิก",
                    Size = new Size(88, 30),
                    Location = new Point(322, 8),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(220, 228, 245),
                    ForeColor = navyDark,
                    Font = new System.Drawing.Font("Segoe UI", 9f),
                    DialogResult = DialogResult.Cancel,
                };
                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
                btnCancel.FlatAppearance.BorderSize = 1;

                pnlFooter.Controls.AddRange(new Control[] { btnOk, btnCancel });

                dlg.Controls.AddRange(new Control[] { pnlHeader, lblHint, lblPrompt, txt, pnlFooter });
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                txt.Select();
                return dlg.ShowDialog(this) == DialogResult.OK ? txt.Text.Trim() : null;
            }
        }

        private void btnApplyTemplate_Click(object sender, EventArgs e)
        {
            if (_folderTemplateController == null || !_folderTemplateController.IsAvailable || _orgLookupController == null)
            {
                // Fallback: prompt for template folder ID (legacy behavior)
                var parent = SelectedFolder;
                if (parent == null) { ShowError("Please select a parent folder to apply the template to."); return; }

                string templateIdStr = PromptForName("Apply Template", "Template folder ID:", string.Empty);
                if (templateIdStr == null) return;

                int templateId;
                if (!int.TryParse(templateIdStr, out templateId)) { ShowError("Please enter a valid numeric template folder ID."); return; }

                _presenter.ApplyTemplate(parent.FolderId, templateId);
                return;
            }

            using (var dlg = new ApplyTemplateDialog(_folderTemplateController, _orgLookupController, _user, SelectedFolder, _currentBreadcrumb))
            {
                // Validate: ต้องเลือก folder ที่อยู่ภายใต้ 4 folders หลักเท่านั้น
                var selectedFolder = SelectedFolder;
                if (HasAuto4RootFolders())
                {
                    if (selectedFolder == null)
                    {
                        ShowError("กรุณาเลือก Folder ภายใต้ 1HC, 2SC, 3NC หรือ 4PC ก่อนสร้าง Template");
                        return;
                    }
                    if (!IsAuto4Folder(selectedFolder) && !IsUnderAuto4Folder(selectedFolder))
                    {
                        ShowError("ไม่สามารถสร้าง Template ที่ Root ได้\nกรุณาเลือก Folder ภายใต้ 1HC, 2SC, 3NC หรือ 4PC");
                        return;
                    }
                }
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    // Reload folder tree เสมอ
                    _presenter.LoadFolderTree(_selectedOrgUnitId);

                    if (dlg.SelectedOrgUnitId.HasValue)
                    {
                        // Legacy mode: สร้างจาก OrgUnit → navigate ไปยัง OrgUnit นั้น
                        SelectOrgUnitNode(dlg.SelectedOrgUnitId.Value);
                    }
                    else if (selectedFolder != null)
                    {
                        // สร้างภายใต้ folder ที่เลือก → reload content ของ folder นั้น
                        _isNavigating = true;
                        try { _presenter.LoadFolderTree(_selectedOrgUnitId); }
                        finally { _isNavigating = false; }

                        _presenter.GetFolderContents(selectedFolder.FolderId);

                        // Update UI
                        lblContentInfo.Text = string.Format("กำลังดู: {0}", selectedFolder.FolderName);
                        lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", selectedFolder.FolderName);
                    }

                    ShowInfo("✅ สร้าง Folder จาก Template เรียบร้อยแล้ว");
                }
            }
        }

        // -----------------------------------------------------------------------
        // Hide/Unhide File Feature (MOD-003)
        // -----------------------------------------------------------------------

        private void btnHideFile_Click(object sender, EventArgs e)
        {
            HideSelectedFiles();
        }

        private void btnUnhideFile_Click(object sender, EventArgs e)
        {
            UnhideSelectedFiles();
        }

        private void HideSelectedFiles()
        {
            var selectedFiles = GetSelectedFilesFromContents();
            if (selectedFiles.Count == 0)
            {
                ShowError("กรุณาเลือกไฟล์ที่ต้องการซ่อน");
                return;
            }

            var fileIds = selectedFiles.Select(f => f.FileID).ToList();
            if (MessageBox.Show(
                string.Format("ต้องการซ่อนไฟล์ {0} ไฟล์หรือไม่?", fileIds.Count),
                "ซ่อนไฟล์",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                _fileCommandController.HideFiles(fileIds, _user);
                ShowInfo(string.Format("✅ ซ่อนไฟล์ {0} ไฟล์เรียบร้อยแล้ว", fileIds.Count));
                var folderId = SelectedFolder?.FolderId ?? (_currentNavFolderId.HasValue ? _currentNavFolderId.Value : 0);
                if (folderId > 0)
                    _presenter.GetFolderContents(folderId);
            }
            catch (Exception ex)
            {
                ShowError($"ไม่สามารถซ่อนไฟล์: {ex.Message}");
            }
        }

        private void UnhideSelectedFiles()
        {
            var selectedFiles = GetSelectedFilesFromContents();
            if (selectedFiles.Count == 0)
            {
                ShowError("กรุณาเลือกไฟล์ที่ต้องการแสดง");
                return;
            }

            var fileIds = selectedFiles.Select(f => f.FileID).ToList();
            if (MessageBox.Show(
                string.Format("ต้องการแสดงไฟล์ {0} ไฟล์หรือไม่?", fileIds.Count),
                "แสดงไฟล์",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                _fileCommandController.UnhideFiles(fileIds, _user);
                ShowInfo(string.Format("✅ แสดงไฟล์ {0} ไฟล์เรียบร้อยแล้ว", fileIds.Count));
                var folderId = SelectedFolder?.FolderId ?? (_currentNavFolderId.HasValue ? _currentNavFolderId.Value : 0);
                if (folderId > 0)
                    _presenter.GetFolderContents(folderId);
            }
            catch (Exception ex)
            {
                ShowError($"ไม่สามารถแสดงไฟล์: {ex.Message}");
            }
        }

        // -----------------------------------------------------------------------
        // Confidentiality level change  (Req 7.5, 7.6)
        // -----------------------------------------------------------------------

        private void btnChangeLevel_Click(object sender, EventArgs e)
        {
            var folder = SelectedFolder;
            if (folder == null)
            {
                ShowError("Please select a folder to change its confidentiality level.");
                return;
            }

            int? newLevel = PromptForConfidentialityLevel(folder.ConfidentialityLevel);
            if (!newLevel.HasValue) return;

            var confirm = MessageBox.Show(
                string.Format(
                    "Change confidentiality level of '{0}' to {1}?\n\n" +
                    "This will propagate to all inheriting descendant folders and files.",
                    folder.Name,
                    ConfidentialityLabel(newLevel.Value)),
                "Confirm Level Change",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                _folderCommandController.ChangeConfidentialityLevel(folder.FolderId, newLevel.Value, _user);
                ShowInfo(string.Format(
                    "Confidentiality level changed to {0} and propagated to all inheriting descendants.",
                    ConfidentialityLabel(newLevel.Value)));
                _presenter.LoadFolderTree(_selectedOrgUnitId);
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowError("Access denied: " + ex.Message);
            }
            catch (Exception ex)
            {
                ShowError("Failed to change confidentiality level: " + ex.Message);
            }
        }

        // -----------------------------------------------------------------------
        // Copy folder structure  (Req 8.5, 8.6)
        // -----------------------------------------------------------------------

        private void BtnCopyStructure_Click(object sender, EventArgs e)
        {
            var sourceFolder = SelectedFolder;
            if (sourceFolder == null)
            {
                ShowError("กรุณาเลือก Folder ต้นทางที่ต้องการคัดลอกโครงสร้าง");
                return;
            }

            // Show dialog to select destination parent folder
            int? destinationParentId = PromptForDestinationFolder(sourceFolder.FolderId);
            if (!destinationParentId.HasValue) return;

            try
            {
                _folderCommandController.CopyFolderStructure(sourceFolder.FolderId, destinationParentId.Value, _user);

                // Reload tree and select the destination parent so user can see the new folder
                _presenter.LoadFolderTree(_selectedOrgUnitId);

                if (!RestoreSelectedNode(folderTreeView.Nodes, destinationParentId.Value))
                {
                    // ไม่ auto-select โฟลเดอร์แรก ให้ user เลือกเอง
                    // Disable events ชั่วคราวเพื่อป้องกัน auto-trigger
                    folderTreeView.AfterSelect -= folderTreeView_AfterSelect;
                    folderTreeView.SelectedNode = null;
                    folderTreeView.AfterSelect += folderTreeView_AfterSelect;
                    ClearContentPanel();
                }

                ShowInfo(string.Format("คัดลอกโครงสร้าง Folder '{0}' เรียบร้อยแล้ว", sourceFolder.FolderName));
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowError("ไม่มีสิทธิ์คัดลอกโครงสร้าง Folder: " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ShowError("ไม่สามารถคัดลอกโครงสร้าง Folder ได้: " + ex.Message);
            }
            catch (ArgumentException ex)
            {
                ShowError("ข้อมูลไม่ถูกต้อง: " + ex.Message);
            }
            catch (Exception ex)
            {
                ShowError("เกิดข้อผิดพลาด: " + ex.Message);
            }
        }

        /// <summary>
        /// Shows a dialog for the user to select a destination parent folder.
        /// Returns the selected FolderId, or null if cancelled.
        /// </summary>
        private int? PromptForDestinationFolder(int sourceFolderId)
        {
            using (var dlg = new Form())
            {
                dlg.Text = "เลือก Folder ปลายทาง";
                dlg.ClientSize = new Size(400, 480);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;

                var lblInstruction = new Label
                {
                    Text = "เลือก Folder ปลายทางที่จะวางโครงสร้างที่คัดลอก:",
                    Dock = DockStyle.Top,
                    Height = 30,
                    Padding = new Padding(4, 6, 4, 0),
                    Font = new Font("Microsoft Sans Serif", 9f)
                };

                var tree = new TreeView
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Microsoft Sans Serif", 10f),
                    HotTracking = true
                };

                // Populate tree from current folderTreeView nodes, excluding source folder
                CloneTreeNodes(folderTreeView.Nodes, tree.Nodes, sourceFolderId);
                tree.ExpandAll();

                var pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 36 };
                var btnOk = new Button { Text = "ตกลง", DialogResult = DialogResult.OK, Width = 80, Height = 26 };
                var btnCancel = new Button { Text = "ยกเลิก", DialogResult = DialogResult.Cancel, Width = 80, Height = 26 };
                btnOk.Location = new Point(236, 5);
                btnCancel.Location = new Point(320, 5);
                pnlButtons.Controls.Add(btnOk);
                pnlButtons.Controls.Add(btnCancel);
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                var pnlContent = new Panel { Dock = DockStyle.Fill };
                pnlContent.Controls.Add(tree);

                dlg.Controls.Add(pnlContent);
                dlg.Controls.Add(pnlButtons);
                dlg.Controls.Add(lblInstruction);

                if (dlg.ShowDialog(this) != DialogResult.OK) return null;

                var selectedNode = tree.SelectedNode;
                if (selectedNode == null)
                {
                    ShowError("กรุณาเลือก Folder ปลายทาง");
                    return null;
                }

                var folder = selectedNode.Tag as Folder;
                if (folder == null)
                {
                    ShowError("กรุณาเลือก Folder ที่ถูกต้อง");
                    return null;
                }

                return folder.FolderId;
            }
        }

        /// <summary>
        /// Clones tree nodes from source into destination, excluding the folder with excludeFolderId.
        /// </summary>
        private static void CloneTreeNodes(TreeNodeCollection source, TreeNodeCollection dest, int excludeFolderId)
        {
            foreach (TreeNode node in source)
            {
                var folder = node.Tag as Folder;
                if (folder != null && folder.FolderId == excludeFolderId) continue;

                var clone = new TreeNode(node.Text) { Tag = node.Tag, ForeColor = node.ForeColor };
                dest.Add(clone);
                CloneTreeNodes(node.Nodes, clone.Nodes, excludeFolderId);
            }
        }

        /// <summary>
        /// Opens a dialog to select a new confidentiality level.
        /// Returns null if the user cancels.
        /// </summary>
        private static int? PromptForConfidentialityLevel(int current)
        {
            Color navyDark = Color.FromArgb(25, 55, 105);
            Color navyLight = Color.FromArgb(215, 228, 248);
            Color bodyBg = Color.FromArgb(243, 246, 251);
            Color footerBg = Color.FromArgb(228, 234, 245);

            var levelColors = new[]
            {
                Color.FromArgb(204,   0,   0),  // 1HC — Red
                Color.FromArgb(220,  90,   0),  // 2SC — Orange
                Color.FromArgb(  0, 100, 200),  // 3NC — Blue
                Color.FromArgb(  0, 150,  50),  // 4PC — Green
            };

            using (var dlg = new Form())
            {
                dlg.Text = "เปลี่ยนระดับความลับ";
                dlg.ClientSize = new Size(400, 218);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = bodyBg;
                dlg.Font = new System.Drawing.Font("Segoe UI", 9f);

                // ── Header ────────────────────────────────────────────────────
                var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = navyDark };
                var lblHeader = new Label
                {
                    Text = "🔒  เปลี่ยนระดับความลับ",
                    Font = new System.Drawing.Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(14, 14),
                };
                pnlHeader.Controls.Add(lblHeader);

                // ── Hint bar ──────────────────────────────────────────────────
                var lblHint = new Label
                {
                    Text = "  การเปลี่ยนระดับจะมีผลกับ Folder และไฟล์ภายใน",
                    Font = new System.Drawing.Font("Segoe UI", 8.5f),
                    ForeColor = Color.FromArgb(50, 70, 110),
                    BackColor = navyLight,
                    Size = new Size(400, 24),
                    Location = new Point(0, 52),
                    TextAlign = ContentAlignment.MiddleLeft,
                };

                // ── Body ──────────────────────────────────────────────────────
                var lblLevel = new Label
                {
                    Text = "ระดับใหม่:",
                    Font = new System.Drawing.Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 40, 65),
                    Location = new Point(14, 92),
                    AutoSize = true,
                };

                var cboLevel = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    DrawMode = DrawMode.OwnerDrawFixed,
                    ItemHeight = 22,
                    Location = new Point(14, 114),
                    Size = new Size(372, 22),
                    Font = new System.Drawing.Font("Segoe UI", 9f),
                };
                cboLevel.Items.AddRange(new object[]
                {
                    "1HC — High Confidential",
                    "2SC — Semi Confidential",
                    "3NC — Normal Confidential",
                    "4PC — Public Confidential",
                });
                cboLevel.SelectedIndex = current - 1;
                cboLevel.DrawItem += (s2, e2) =>
                {
                    if (e2.Index < 0) return;
                    e2.DrawBackground();
                    var rc = e2.Bounds;
                    var sq = new Rectangle(rc.X + 4, rc.Y + 4, 14, 14);
                    using (var b = new SolidBrush(levelColors[e2.Index]))
                        e2.Graphics.FillRectangle(b, sq);
                    e2.Graphics.DrawRectangle(Pens.DimGray, sq);
                    using (var b = new SolidBrush(
                        (e2.State & DrawItemState.Disabled) != 0
                            ? SystemColors.GrayText : e2.ForeColor))
                        e2.Graphics.DrawString(
                            cboLevel.Items[e2.Index].ToString(), e2.Font, b,
                            new RectangleF(rc.X + 24, rc.Y + 3, rc.Width - 28, rc.Height));
                    if ((e2.State & DrawItemState.Focus) != 0) e2.DrawFocusRectangle();
                };

                // ── Footer ────────────────────────────────────────────────────
                var pnlFooter = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 46,
                    BackColor = footerBg,
                };

                var btnOk = new Button
                {
                    Text = "ตกลง",
                    Size = new Size(88, 30),
                    Location = new Point(210, 8),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = navyDark,
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9f, FontStyle.Bold),
                    DialogResult = DialogResult.OK,
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text = "ยกเลิก",
                    Size = new Size(88, 30),
                    Location = new Point(304, 8),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(220, 228, 245),
                    ForeColor = navyDark,
                    Font = new System.Drawing.Font("Segoe UI", 9f),
                    DialogResult = DialogResult.Cancel,
                };
                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
                btnCancel.FlatAppearance.BorderSize = 1;

                pnlFooter.Controls.AddRange(new Control[] { btnOk, btnCancel });

                dlg.Controls.AddRange(new Control[] { pnlHeader, lblHint, lblLevel, cboLevel, pnlFooter });
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                if (dlg.ShowDialog() != DialogResult.OK) return null;
                return cboLevel.SelectedIndex + 1;
            }
        }

        // Recycle bin button handlers removed — functionality moved to RecycleBinForm

        /// <summary>Dialog สำหรับรับชื่อพื้นฐานสำหรับ Auto 4 Folders</summary>
        private string PromptForBaseName(string title)
        {
            using (var form = new Form())
            using (var label = new Label())
            using (var input = new TextBox())
            using (var lblInfo = new Label())
            using (var ok = new Button())
            using (var cancel = new Button())
            {
                form.Text = title;
                form.ClientSize = new Size(400, 180);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                label.Text = "ชื่อพื้นฐาน (Base Name):";
                label.Location = new Point(12, 12);
                label.Size = new Size(376, 20);

                input.Location = new Point(12, 36);
                input.Size = new Size(376, 23);

                lblInfo.Text = "ระบบจะสร้าง 4 Folders อัตโนมัติ:\n" +
                              "• 1HC - [ชื่อ] [1HC] - สีแดง\n" +
                              "• 2SC - [ชื่อ] [2SC] - สีส้ม\n" +
                              "• 3NC - [ชื่อ] [3NC] - สีน้ำเงิน\n" +
                              "• 4PC - [ชื่อ] [4PC] - สีเขียว";
                lblInfo.Location = new Point(12, 72);
                lblInfo.Size = new Size(376, 80);
                lblInfo.ForeColor = Color.FromArgb(60, 60, 60);

                ok.Text = "สร้าง";
                ok.DialogResult = DialogResult.OK;
                ok.Location = new Point(232, 148);
                ok.Size = new Size(75, 26);
                form.AcceptButton = ok;

                cancel.Text = "ยกเลิก";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Location = new Point(313, 148);
                cancel.Size = new Size(75, 26);
                form.CancelButton = cancel;

                form.Controls.AddRange(new Control[] { label, input, lblInfo, ok, cancel });

                if (form.ShowDialog(this) != DialogResult.OK) return null;

                string value = input.Text.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }
        }

        /// <summary>สร้าง Auto 4 Folders ตามระดับความลับ</summary>
        private void CreateAuto4Folders(string baseName, int? parentFolderId, int orgUnitId)
        {
            try
            {
                var folderConfigs = new[]
                {
                    new { Level = 1, Code = "1HC", Name = "High Confidential", Color = "#CC0000" },
                    new { Level = 2, Code = "2SC", Name = "Semi Confidential", Color = "#FF6600" },
                    new { Level = 3, Code = "3NC", Name = "Normal Confidential", Color = "#0066CC" },
                    new { Level = 4, Code = "4PC", Name = "Public Confidential", Color = "#009933" }
                };

                int successCount = 0;
                var errors = new List<string>();

                foreach (var config in folderConfigs)
                {
                    try
                    {
                        string folderName = string.IsNullOrWhiteSpace(baseName)
                            ? string.Format("{0} - ({1})", config.Code, config.Name)
                            : string.Format("{0} - {1} [{0}]", config.Code, baseName);

                        if (parentFolderId.HasValue)
                            _presenter.CreateFolder(parentFolderId.Value, folderName, orgUnitId, config.Level, false);
                        else
                            _presenter.CreateRootFolder(folderName, orgUnitId, config.Level, false);

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add(string.Format("{0}: {1}", config.Code, ex.Message));
                    }
                }

                // แสดงผลลัพธ์ (ไม่แสดง message เมื่อสำเร็จทั้ง 4 folders)
                if (successCount == 4)
                {
                    // สำเร็จ — ไม่แสดง message
                }
                else if (successCount > 0)
                {
                    string errorMsg = errors.Count > 0 ? "\n\nข้อผิดพลาด:\n" + string.Join("\n", errors) : "";
                    ShowInfo(string.Format("⚠️ สร้างได้บางส่วน: {0}/{1} Folders{2}", successCount, 4, errorMsg));
                }
                else
                {
                    string errorMsg = errors.Count > 0 ? "\n\nข้อผิดพลาด:\n" + string.Join("\n", errors) : "";
                    ShowError(string.Format("❌ ไม่สามารถสร้าง Auto 4 Folders ได้{0}", errorMsg));
                }

                // Refresh folder tree
                _presenter.LoadFolderTree(_selectedOrgUnitId);
            }
            catch (Exception ex)
            {
                ShowError("เกิดข้อผิดพลาดในการสร้าง Auto 4 Folders: " + ex.Message);
            }
        }

        /// <summary>ดึงชื่อพื้นฐานที่สะอาดจาก folder name โดยตัดส่วน Auto 4 Folders format ออก</summary>
        private bool IsAuto4Folder(Folder folder)
        {
            if (folder == null) return false;

            string folderName = folder.FolderName ?? folder.Name ?? "";

            // ตรวจสอบว่าเป็น Auto 4 Folders format หรือไม่
            var auto4Patterns = new[]
            {
                @"^1HC\s*-\s*.+?\s*\[1HC\]",
                @"^2SC\s*-\s*.+?\s*\[2SC\]",
                @"^3NC\s*-\s*.+?\s*\[3NC\]",
                @"^4PC\s*-\s*.+?\s*\[4PC\]",
                // รองรับ format เก่า
                @"^1HC\s*-\s*.+?\s*\(High Confidential\)",
                @"^2SC\s*-\s*.+?\s*\(Semi Confidential\)",
                @"^3NC\s*-\s*.+?\s*\(Normal Confidential\)",
                @"^4PC\s*-\s*.+?\s*\(Public Confidential\)"
            };

            foreach (var pattern in auto4Patterns)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(folderName, pattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>ตรวจสอบว่า OrgUnit ที่เลือกมี Auto4 root folders (1HC/2SC/3NC/4PC) อยู่แล้วหรือยัง</summary>
        private bool HasAuto4RootFolders()
        {
            var levels = new HashSet<int>();

            foreach (System.Windows.Forms.TreeNode node in folderTreeView.Nodes)
            {
                var folder = node.Tag as Folder;
                if (folder == null) continue;

                bool isRoot = !folder.ParentFolderID.HasValue || folder.ParentFolderID == 0;
                if (!isRoot) continue;

                if (IsAuto4Folder(folder))
                    levels.Add(folder.ConfidentialityLevel);
            }

            return levels.Contains(1)
                && levels.Contains(2)
                && levels.Contains(3)
                && levels.Contains(4);
        }

        /// <summary>ตรวจสอบว่า folder นี้อยู่ภายใต้ Auto4 root folder (1HC/2SC/3NC/4PC) หรือไม่</summary>
        private bool IsUnderAuto4Folder(Folder folder)
        {
            if (folder == null) return false;
            // ตัว folder เองเป็น Auto4 root → อนุญาต
            if (IsAuto4Folder(folder) && folder.ParentFolderID == null)
                return true;
            // หา root ancestor จาก folderTreeView
            var current = folderTreeView.SelectedNode;
            while (current != null)
            {
                if (current.Parent == null)
                {
                    var rootFolder = current.Tag as Folder;
                    return rootFolder != null && IsAuto4Folder(rootFolder);
                }
                current = current.Parent;
            }
            return false;
        }

        private string GetCleanBaseName(Folder folder)
        {
            string folderName = folder.FolderName ?? folder.Name ?? "เอกสาร";

            // ตรวจสอบว่าเป็น Auto 4 Folders format หรือไม่
            // Format ใหม่: "1HC - ชื่อ [1HC]"  |  Format เก่า: "1HC - ชื่อ (High Confidential)"
            var auto4Patterns = new[]
            {
                @"^1HC\s*-\s*(.+?)\s*\[1HC\]",
                @"^2SC\s*-\s*(.+?)\s*\[2SC\]",
                @"^3NC\s*-\s*(.+?)\s*\[3NC\]",
                @"^4PC\s*-\s*(.+?)\s*\[4PC\]",
                @"^1HC\s*-\s*(.+?)\s*\(High Confidential\)",
                @"^2SC\s*-\s*(.+?)\s*\(Semi Confidential\)",
                @"^3NC\s*-\s*(.+?)\s*\(Normal Confidential\)",
                @"^4PC\s*-\s*(.+?)\s*\(Public Confidential\)"
            };

            foreach (var pattern in auto4Patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(folderName, pattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    // ดึงชื่อพื้นฐานออกมา
                    string baseName = match.Groups[1].Value.Trim();

                    // ตรวจสอบว่าชื่อพื้นฐานไม่ใช่ Auto 4 format ซ้อนอีกชั้น
                    foreach (var innerPattern in auto4Patterns)
                    {
                        var innerMatch = System.Text.RegularExpressions.Regex.Match(baseName, innerPattern,
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (innerMatch.Success)
                        {
                            baseName = innerMatch.Groups[1].Value.Trim();
                            break;
                        }
                    }

                    return string.IsNullOrWhiteSpace(baseName) ? "" : baseName;
                }
            }

            // ถ้าไม่ใช่ Auto 4 format ให้ใช้ชื่อเดิม
            return string.IsNullOrWhiteSpace(folderName) ? "" : folderName;
        }

        /// <summary>ตรวจสอบว่ามี folder ชื่อนี้อยู่แล้วหรือไม่</summary>
        private bool IsFolderExists(string folderName, int? parentFolderId, int orgUnitId)
        {
            try
            {
                if (_folderReadLookupController == null) return false;

                // ใช้ service เพื่อตรวจสอบความถูกต้อง
                var allFolders = _folderReadLookupController.GetAllFolders(null, _user);

                if (parentFolderId.HasValue)
                {
                    // ตรวจสอบ sub-folders
                    return allFolders.Any(f =>
                        !f.IsDeleted &&
                        f.ParentFolderID == parentFolderId.Value &&
                        string.Equals(f.Name, folderName, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    // ตรวจสอบ root folders
                    return allFolders.Any(f =>
                        !f.IsDeleted &&
                        f.ParentFolderID == null &&
                        f.OrgUnitID == orgUnitId &&
                        string.Equals(f.Name, folderName, StringComparison.OrdinalIgnoreCase));
                }
            }
            catch
            {
                return false; // ถ้าตรวจสอบไม่ได้ ให้ถือว่าไม่มี
            }
        }

        /// <summary>สร้าง Auto 4 Folders แบบเงียบ (ไม่แสดงข้อความใดๆ เลย)</summary>
        private void CreateAuto4FoldersQuiet(string baseName, int? parentFolderId, int orgUnitId)
        {
            // ตั้ง flag ป้องกัน PromptToCreateFirstFolder ยิงซ้ำระหว่างสร้าง
            _suppressCreatePrompt = true;
            int createdCount = 0;
            var skippedFolders = new List<string>();
            var errorFolders = new List<string>();

            try
            {
                var folderConfigs = new[]
                {
                    new { Level = 1, Code = "1HC", Name = "High Confidential",  Color = "#CC0000" },
                    new { Level = 2, Code = "2SC", Name = "Semi Confidential",  Color = "#FF6600" },
                    new { Level = 3, Code = "3NC", Name = "Normal Confidential",Color = "#0066CC" },
                    new { Level = 4, Code = "4PC", Name = "Public Confidential", Color = "#009933" }
                };

                foreach (var config in folderConfigs)
                {
                    try
                    {
                        if (_folderCommandController == null)
                        {
                            errorFolders.Add(string.Format("{0}: Folder command controller is not available", config.Code));
                            continue;
                        }

                        string folderName = string.IsNullOrWhiteSpace(baseName)
                            ? string.Format("{0} - ({1})", config.Code, config.Name)
                            : string.Format("{0} - {1} [{0}]", config.Code, baseName);

                        if (IsFolderExists(folderName, parentFolderId, orgUnitId))
                        {
                            skippedFolders.Add(config.Code);
                            continue;
                        }

                        var request = new CMS.Services.DTOs.CreateFolderRequest
                        {
                            ParentFolderId = parentFolderId,
                            OrgUnitId = orgUnitId,
                            Name = folderName,
                            ConfidentialityLevel = config.Level
                        };

                        var created = _folderCommandController.CreateFolder(request, _user);
                        if (created == null)
                        {
                            errorFolders.Add(string.Format("{0}: ระบบไม่สามารถสร้างโฟลเดอร์ได้ (ผลลัพธ์เป็นค่าว่าง)", config.Code));
                            continue;
                        }

                        if (created.FolderID > 0)
                            createdCount++;
                        else
                            errorFolders.Add(string.Format("{0}: ไม่สามารถยืนยันการสร้างโฟลเดอร์ได้", config.Code));
                    }
                    catch (Exception ex)
                    {
                        errorFolders.Add(string.Format("{0}: {1}", config.Code, ex.Message));
                    }
                }
            }
            finally
            {
                // คืน flag ก่อน reload เสมอ (ป้องกัน finally leak)
                _suppressCreatePrompt = false;
            }

            _pendingHCApproval = false;

            // Reload tree ครั้งเดียวหลังสร้างครบ
            _presenter.LoadFolderTree(_selectedOrgUnitId);

            // ── แสดงผลสรุปที่ชัดเจน ─────────────────────────────────────────
            var sb = new System.Text.StringBuilder();

            if (createdCount > 0)
                sb.AppendLine(string.Format("✅ สร้าง Folder สำเร็จ {0} folder(s)", createdCount));

            if (skippedFolders.Count > 0)
                sb.AppendLine(string.Format("ℹ️ มีอยู่แล้ว (ข้าม): {0}", string.Join(", ", skippedFolders)));

            if (errorFolders.Count > 0)
                sb.AppendLine(string.Format("⚠️ เกิดข้อผิดพลาด:\n   {0}", string.Join("\n   ", errorFolders)));

            if (sb.Length > 0)
                ShowInfo(sb.ToString().TrimEnd());
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Builds the full breadcrumb path combining the OrgUnit tree path and the folder tree path.
        /// e.g. "ทดสอบการทำงาน › การตลาด › 1HC - เอกสาร › SubFolder"
        /// orgUnitNode = currently selected node in treeOrgUnit (may be null)
        /// folderNode  = currently selected node in folderTreeView (may be null)
        /// </summary>
        private static List<int?> BuildFolderIdPath(TreeNode folderNode)
        {
            var folderIds = new List<int?>();

            if (folderNode == null) return folderIds;

            var pathNodes = new List<TreeNode>();
            var current = folderNode;
            while (current != null)
            {
                pathNodes.Insert(0, current);
                current = current.Parent;
            }

            foreach (var node in pathNodes)
            {
                var folder = node.Tag as Folder;
                if (folder != null)
                    folderIds.Add(folder.FolderId);
            }

            return folderIds;
        }

        /// <summary>
        /// สร้าง folderIds ที่มี index ตรงกับ breadcrumb parts
        /// null สำหรับ parts ที่ไม่ใช่ folder (PC, OrgUnit)
        /// </summary>
        private List<int?> BuildFolderIdPathAligned(string breadcrumbPath, TreeNode folderNode)
        {
            var parts = breadcrumbPath.Split(new string[] { "  ›  " }, StringSplitOptions.RemoveEmptyEntries);
            var result = new List<int?>();

            // สร้าง folder path จาก tree node
            var folderPath = new List<Folder>();
            var current = folderNode;
            while (current != null)
            {
                var f = current.Tag as Folder;
                if (f != null) folderPath.Insert(0, f);
                current = current.Parent;
            }

            // map parts → folder IDs
            // parts แรกๆ เป็น PC/OrgUnit → null
            // parts ท้ายๆ เป็น folder names → ใช้ folderPath
            int folderStartIndex = parts.Length - folderPath.Count;
            for (int i = 0; i < parts.Length; i++)
            {
                int folderIndex = i - folderStartIndex;
                if (folderIndex >= 0 && folderIndex < folderPath.Count)
                    result.Add(folderPath[folderIndex].FolderId);
                else
                    result.Add(null);
            }

            return result;
        }

        private static string BuildBreadcrumbPath(TreeNode orgUnitNode, TreeNode folderNode)
        {
            var parts = new System.Collections.Generic.List<string>();

            // 1) Walk up folderTreeView to get folder path (innermost → root)
            var cur = folderNode;
            while (cur != null)
            {
                parts.Insert(0, CleanNodeText(cur.Text));
                cur = cur.Parent;
            }

            // 2) Prepend OrgUnit hierarchy (walk up treeOrgUnit)
            var ouParts = new System.Collections.Generic.List<string>();
            var ouCur = orgUnitNode;
            while (ouCur != null && ouCur.Tag is OrgUnit)
            {
                ouParts.Insert(0, CleanNodeText(ouCur.Text));
                ouCur = ouCur.Parent;
            }
            parts.InsertRange(0, ouParts);

            if (parts.Count == 0) return "🏠 PC";
            return "🏠 PC  ›  " + string.Join("  ›  ", parts);
        }

        // Overload for backward-compat (folder-only, no OrgUnit prefix)
        private static string BuildBreadcrumbPath(TreeNode node)
        {
            return BuildBreadcrumbPath(null, node);
        }

        /// <summary>
        /// Build breadcrumb using OrgUnit object (not TreeNode) + folder TreeNode.
        /// ใช้เมื่อ treeOrgUnit.SelectedNode อาจเป็น null เนื่องจาก async load
        /// แต่ _selectedOrgUnit ยังคงมีค่าถูกต้องอยู่
        /// </summary>
        private string BuildBreadcrumbPathFromOrgUnit(OrgUnit orgUnit, TreeNode folderNode)
        {
            var parts = new List<string>();

            // 1) Walk up folderTreeView to get folder path
            var cur = folderNode;
            while (cur != null)
            {
                parts.Insert(0, CleanNodeText(cur.Text));
                cur = cur.Parent;
            }

            // 2) Prepend OrgUnit path — ค้นหา node ใน treeOrgUnit ที่ตรงกับ _selectedOrgUnit
            //    แล้ว walk up เหมือน BuildBreadcrumbPath ปกติ
            //    ถ้าหา node ไม่เจอ ใช้ชื่อ OrgUnit ตรงๆ แทน
            if (orgUnit != null)
            {
                var ouNode = FindOrgUnitNode(treeOrgUnit.Nodes, orgUnit.OrgUnitID);
                if (ouNode != null)
                {
                    // Walk up treeOrgUnit node เหมือน BuildBreadcrumbPath
                    var ouParts = new List<string>();
                    var ouCur = ouNode;
                    while (ouCur != null && ouCur.Tag is OrgUnit)
                    {
                        ouParts.Insert(0, CleanNodeText(ouCur.Text));
                        ouCur = ouCur.Parent;
                    }
                    parts.InsertRange(0, ouParts);
                }
                else
                {
                    // Fallback: ใช้ชื่อ OrgUnit ตรงๆ
                    parts.Insert(0, orgUnit.Name);
                }
            }

            if (parts.Count == 0) return "🏠 PC";
            return "🏠 PC  ›  " + string.Join("  ›  ", parts);
        }

        /// <summary>Strips badge/count decorations from a tree node label.</summary>
        private static string CleanNodeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            // Strip " (N)" file-count suffix: e.g. "1HC - เอกสาร (1)" → "1HC - เอกสาร"
            int parenIdx = text.LastIndexOf(" (");
            if (parenIdx > 0 && text.EndsWith(")")) text = text.Substring(0, parenIdx);
            // Strip " [badge]" confidentiality badge suffix
            int bracketIdx = text.LastIndexOf(" [");
            if (bracketIdx > 0) text = text.Substring(0, bracketIdx);
            // Strip leading emoji/icon prefix (e.g. "📁 ", "🏢 ")
            text = text.Trim();
            if (text.Length > 2 && char.IsHighSurrogate(text[0]))
                text = text.Substring(2).TrimStart();
            return text.Trim();
        }

        /// <summary>
        /// Creates a TreeNode for a folder with a Confidentiality_Level badge in the label
        /// and a color matching the classification (Req 7.6).
        /// </summary>
        private void FolderTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node != _activeTreeNode || e.Node.NodeFont == null)
            {
                e.DrawDefault = true;
                return;
            }

            // วาด text พร้อม background สีน้ำเงิน + text สีขาว ไม่ถูก clip
            e.DrawDefault = false;
            var tv = (TreeView)sender;
            var rect = new Rectangle(e.Bounds.X, e.Bounds.Y,
                tv.ClientSize.Width - e.Bounds.X, e.Bounds.Height);

            using (var bg = new SolidBrush(Color.FromArgb(66, 133, 244)))
                e.Graphics.FillRectangle(bg, rect);

            TextRenderer.DrawText(e.Graphics, e.Node.Text, e.Node.NodeFont, rect,
                Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }

        private void SetActiveNode(TreeNode node)
        {
            if (_activeTreeNode != null && _activeTreeNode != node)
                _activeTreeNode.NodeFont = null;

            _activeTreeNode = node;
            if (node != null)
            {
                var baseFont = folderTreeView.Font;
                node.NodeFont = new Font(baseFont, FontStyle.Bold);
                folderTreeView.SelectedNode = node;
            }
        }

        private TreeNode CreateFolderNode(Folder folder)
        {
            string badge = ConfidentialityBadge(folder.ConfidentialityLevel);
            bool hasViewAccess = HasFolderViewAccess(folder);
            string label = string.Format("{0} {1}{2}{3}",
                folder.Name,
                badge,
                folder.IsArchived ? " [Archived]" : string.Empty,
                hasViewAccess ? string.Empty : " [ไม่มีสิทธิ์]");

            // 4 Folder หลัก (root) ใช้ book icon ตามระดับความลับ; folder ลูกใช้ folder icon ทั่วไป
            bool isRoot = !folder.ParentFolderID.HasValue || folder.ParentFolderID == 0;
            string imageKey = isRoot ? ConfidentialityBookIcon(folder.ConfidentialityLevel) : "folder";

            return new TreeNode(label)
            {
                Tag = folder,
                ForeColor = hasViewAccess
                    ? ConfidentialityColor(folder.ConfidentialityLevel)
                    : Color.FromArgb(140, 140, 140),
                ImageKey = imageKey,
                SelectedImageKey = imageKey
            };
        }

        /// <summary>
        /// Gets the physical file path for a FileDocument.
        /// This method uses the PhysicalPath property or constructs the path based on the file's storage location.
        /// </summary>
        private string GetFilePhysicalPath(FileDocument file)
        {
            if (file == null) return null;

            try
            {
                var candidates = new List<string>();

                // 1) DB path (preferred)
                if (!string.IsNullOrWhiteSpace(file.PhysicalPath))
                {
                    candidates.Add(file.PhysicalPath.Trim());
                }

                string baseStoragePath = ConfigurationManager.AppSettings["NetworkDrivePath"];
                if (string.IsNullOrWhiteSpace(baseStoragePath))
                    baseStoragePath = @"\\150.1.8.23\AppForUser\Cmd_LmportFile";

                string fileName = !string.IsNullOrWhiteSpace(file.StoredFileName)
                    ? file.StoredFileName
                    : file.OriginalFileName;

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    // 2) Current standard storage layout: {base}\{orgUnitId}\{folderId}\{fileName}
                    int? orgUnitId = null;

                    if (SelectedFolder != null && SelectedFolder.FolderID == file.FolderID)
                        orgUnitId = SelectedFolder.OrgUnitID;

                    if (!orgUnitId.HasValue && _folderReadLookupController != null)
                    {
                        try
                        {
                            var folder = _folderReadLookupController.GetFolder(file.FolderID, _user);
                            if (folder != null)
                                orgUnitId = folder.OrgUnitID;
                        }
                        catch { }
                    }

                    if (orgUnitId.HasValue)
                    {
                        candidates.Add(System.IO.Path.Combine(
                            baseStoragePath,
                            orgUnitId.Value.ToString(),
                            file.FolderID.ToString(),
                            fileName));
                    }

                    // 3) Legacy/fallback layouts seen in older data
                    candidates.Add(System.IO.Path.Combine(baseStoragePath, string.Format("Folder_{0}", file.FolderID), fileName));
                    candidates.Add(System.IO.Path.Combine(baseStoragePath, file.FolderID.ToString(), fileName));
                }

                foreach (var path in candidates.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (System.IO.File.Exists(path))
                        return path;
                }

                // Return first candidate for diagnostics if none exists.
                return candidates.FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string ConfidentialityBadge(int level)
        {
            switch (level)
            {
                case 1: return "[1HC]";
                case 2: return "[2SC]";
                case 3: return "[3NC]";
                case 4: return "[4PC]";
                default: return string.Empty;
            }
        }

        /// <summary>Returns the ImageList key for a root folder based on confidentiality level.</summary>
        private static string ConfidentialityBookIcon(int level)
        {
            switch (level)
            {
                case 1: return "lightning_bolt";
                case 2: return "book_orange";
                case 3: return "book_bule";
                case 4: return "book_green";
                default: return "folder";
            }
        }

        private static string ConfidentialityShort(int level)
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

        private static string ConfidentialityLabel(int level)
        {
            switch (level)
            {
                case 1: return "1HC - High Confidential";
                case 2: return "2SC - Semi Confidential";
                case 3: return "3NC - Normal Confidential";
                case 4: return "4PC - Public Confidential";
                default: return level.ToString();
            }
        }

        private static Color ConfidentialityColor(int level)
        {
            switch (level)
            {
                case 1: return Color.FromArgb(204, 0, 0);     // #CC0000 - Red
                case 2: return Color.FromArgb(255, 102, 0);   // #FF6600 - Orange  
                case 3: return Color.FromArgb(0, 102, 204);   // #0066CC - Blue
                case 4: return Color.FromArgb(0, 153, 51);    // #009933 - Green
                default: return SystemColors.WindowText;
            }
        }

        /// <summary>Checks whether current user can view the folder.</summary>
        private bool HasFolderViewAccess(Folder folder)
        {
            if (folder == null) return false;

            bool isPrivileged = _user.AppRole >= 3; // Manager or Admin
            if (isPrivileged)
                return true;

            bool cached;
            if (_viewAccessCache.TryGetValue(folder.FolderId, out cached))
                return cached;

            // Check authority-based permissions (FolderAuthorityConfig)
            bool canView = _user.CanViewFolder(folder.ConfidentialityLevel);

            // Check ACL permissions — short-circuit: Read ก่อน ถ้าได้แล้วไม่ต้องเช็ค Write/Modify
            if (!canView && _permissionCheckController != null)
            {
                try
                {
                    if (_permissionCheckController.CheckPermission(_user.UserID, folder.FolderId, "Read")
                     || _permissionCheckController.CheckPermission(_user.UserID, folder.FolderId, "Write")
                     || _permissionCheckController.CheckPermission(_user.UserID, folder.FolderId, "Modify"))
                        canView = true;
                }
                catch
                {
                    // If permission check fails, fall back to authority-based permissions
                }
            }

            _viewAccessCache[folder.FolderId] = canView;
            return canView;
        }

        private void InvalidateViewAccessCache(int folderId)
        {
            _viewAccessCache.Remove(folderId);
        }

        /// <summary>
        /// สิทธิ์สำหรับแท็บ "ความลับ": ตรวจสอบทั้ง authority-level (FolderAuthorityConfig)
        /// และ ACL (FolderPermission) เหมือน HasFolderViewAccess เพื่อให้แสดงรายการที่ user มีสิทธิ์จริง
        /// </summary>
        private bool HasFolderViewAccessConfTree(Folder folder)
        {
            return HasFolderViewAccess(folder);
        }

        /// <summary>Gets permission text for a folder based on user's access rights.</summary>
        private string GetFolderPermissionText(Folder folder)
        {
            if (folder == null) return "—";

            bool isPrivileged = _user.AppRole >= 3; // Manager or Admin
            bool canView = HasFolderViewAccess(folder);
            bool canAdd = isPrivileged || _user.CanAddToFolder(folder.ConfidentialityLevel);
            bool canEdit = isPrivileged || _user.CanEditFolder(folder.ConfidentialityLevel);

            // If ACL grants are present, elevate add/edit labels accordingly
            if (_permissionCheckController != null && !isPrivileged)
            {
                try
                {
                    bool hasWrite = _permissionCheckController.CheckPermission(_user.UserID, folder.FolderId, "Write");
                    bool hasModify = _permissionCheckController.CheckPermission(_user.UserID, folder.FolderId, "Modify");

                    if (hasWrite || hasModify)
                    {
                        canAdd = hasWrite || hasModify;
                        canEdit = hasModify;
                    }
                }
                catch
                {
                    // If permission check fails, use authority-based permissions
                }
            }

            // Build permission text
            if (!canView)
                return "🔒 ไม่มีสิทธิ์";

            if (isPrivileged)
                return "👑 เต็ม";

            if (canEdit)
                return "✏️ แก้ไข";

            if (canAdd)
                return "➕ เพิ่ม";

            return "👁️ อ่าน";
        }

        /// <summary>Gets color for permission text.</summary>
        private static Color GetPermissionColor(string permissionText)
        {
            if (permissionText.Contains("ไม่มีสิทธิ์"))
                return Color.FromArgb(180, 0, 0); // Red

            if (permissionText.Contains("เต็ม"))
                return Color.FromArgb(0, 120, 0); // Dark green

            if (permissionText.Contains("แก้ไข"))
                return Color.FromArgb(0, 100, 200); // Blue

            if (permissionText.Contains("เพิ่ม"))
                return Color.FromArgb(200, 100, 0); // Orange

            return Color.FromArgb(100, 100, 100); // Gray for read-only
        }

        /// <summary>Returns the ImageList key for a file based on its extension.</summary>
        private static string FileIcon(string ext)
        {
            return "file_docs";
        }

        private static readonly System.Collections.Generic.HashSet<string> _imageExtensions =
            new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
            { "jpg", "jpeg", "png", "gif", "bmp", "webp", "tiff", "tif" };

        /// <summary>
        /// Returns the ImageList key for a file.
        /// For image files with a valid PhysicalPath, loads a thumbnail and registers it
        /// in both SmallImageList (16×16) and LargeImageList (64×64) on demand.
        /// Falls back to "file_docs" on any error.
        /// </summary>
        private string GetFileImageKey(FileDocument file)
        {
            string ext = (file.FileExtension ?? string.Empty).TrimStart('.').ToLowerInvariant();

            if (!_imageExtensions.Contains(ext))
                return "file_docs";

            string thumbKey = "thumb_" + file.FileID;

            // Already registered — reuse
            if (contentsListView.SmallImageList != null &&
                contentsListView.SmallImageList.Images.ContainsKey(thumbKey))
                return thumbKey;

            string physPath = file.PhysicalPath;
            if (string.IsNullOrWhiteSpace(physPath) || !System.IO.File.Exists(physPath))
                return "file_docs";

            try
            {
                // Skip very large files to avoid slow load (> 20 MB)
                var fi = new System.IO.FileInfo(physPath);
                if (fi.Length > 20L * 1024 * 1024) return "file_docs";

                using (var original = Image.FromFile(physPath))
                {
                    if (contentsListView.SmallImageList != null &&
                        !contentsListView.SmallImageList.Images.ContainsKey(thumbKey))
                    {
                        var thumb16 = new Bitmap(original, new Size(16, 16));
                        contentsListView.SmallImageList.Images.Add(thumbKey, thumb16);
                    }

                    if (contentsListView.LargeImageList != null &&
                        !contentsListView.LargeImageList.Images.ContainsKey(thumbKey))
                    {
                        // Use 64×64 for meaningful large-icon preview
                        var largeSize = contentsListView.LargeImageList.ImageSize;
                        var thumb64 = new Bitmap(original, largeSize);
                        contentsListView.LargeImageList.Images.Add(thumbKey, thumb64);
                    }
                }

                return thumbKey;
            }
            catch
            {
                return "file_docs";
            }
        }

        // -----------------------------------------------------------------------
        // ImageList for contentsListView icons
        // -----------------------------------------------------------------------

        /// <summary>
        /// Builds and assigns a 16×16 ImageList to contentsListView.
        /// Uses PNG resources for book/folder icons and draws file-type icons
        /// programmatically so no external icon files are required.
        /// </summary>
        private void BuildContentImageList()
        {
            var il = new ImageList { ImageSize = new Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };

            // ── Folder icons from embedded PNG resources ──────────────────────
            il.Images.Add("folder", LoadPng("folder"));

            // Load lightning bolt - try embedded resource first, then file, then fallback
            Image lightningIcon = null;
            try
            {
                // Try embedded resource (PNG)
                if (Properties.Resources.lightning_bolt1 != null)
                {
                    lightningIcon = new Bitmap(Properties.Resources.lightning_bolt1, 16, 16);
                }
            }
            catch { }

            if (lightningIcon == null)
            {
                try
                {
                    // Try embedded resource (ICO)
                    if (Properties.Resources.lightning_bolt != null)
                    {
                        lightningIcon = Properties.Resources.lightning_bolt.ToBitmap();
                        lightningIcon = new Bitmap(lightningIcon, 16, 16);
                    }
                }
                catch { }
            }

            if (lightningIcon == null)
            {
                // Final fallback: load from file
                lightningIcon = LoadPng("lightning-bolt");
            }

            il.Images.Add("lightning_bolt", lightningIcon);
            il.Images.Add("book_red", LoadPng("book-red"));
            il.Images.Add("book_orange", LoadPng("book-orange"));
            il.Images.Add("book_bule", LoadPng("book-bule"));
            il.Images.Add("book_green", LoadPng("book-green"));

            // ── File icon from File-docs.ico resource ─────────────────────────
            il.Images.Add("file_docs", LoadIco("File-docs"));

            contentsListView.SmallImageList = il;
        }

        /// <summary>Loads a 16×16 PNG from the Resources folder by base name.</summary>
        private static Image LoadPng(string baseName)
        {
            try
            {
                string path = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "Resources", baseName + ".png");

                if (System.IO.File.Exists(path))
                    return new Bitmap(Image.FromFile(path), 16, 16);
            }
            catch { /* fall through to fallback */ }

            // Fallback: draw a plain yellow folder
            return DrawFileIcon(Color.FromArgb(255, 200, 50), "DIR");
        }

        /// <summary>Loads a 16×16 image from an .ico file in the Resources folder.</summary>
        private static Image LoadIco(string baseName)
        {
            try
            {
                string path = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "Resources", baseName + ".ico");

                if (System.IO.File.Exists(path))
                {
                    using (var icon = new Icon(path, 16, 16))
                        return icon.ToBitmap();
                }
            }
            catch { /* fall through to fallback */ }

            return DrawFileIcon(Color.FromArgb(120, 120, 140), "FILE");
        }

        /// <summary>
        /// Draws a compact 16×16 file-type badge:
        /// a white page with a folded corner, filled label bar at bottom.
        /// </summary>
        private static Image DrawFileIcon(Color labelColor, string label)
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Page body
                var pageRect = new Rectangle(1, 0, 12, 15);
                using (var fill = new SolidBrush(Color.White))
                    g.FillRectangle(fill, pageRect);

                // Folded corner (top-right)
                var corner = new Point[] {
                    new Point(9, 0), new Point(13, 0),
                    new Point(13, 4), new Point(9, 4)
                };
                using (var fold = new SolidBrush(Color.FromArgb(200, 200, 200)))
                    g.FillPolygon(fold, corner);

                // Page border
                using (var pen = new Pen(Color.FromArgb(160, 160, 160), 1))
                    g.DrawRectangle(pen, pageRect);

                // Color label bar at bottom
                var bar = new Rectangle(1, 10, 12, 5);
                using (var barFill = new SolidBrush(labelColor))
                    g.FillRectangle(barFill, bar);

                // Label text (tiny, 5pt)
                if (!string.IsNullOrEmpty(label) && label.Length <= 4)
                {
                    using (var font = new Font("Arial", 4.5f, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(Color.White))
                    {
                        var sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString(label, font, textBrush, new RectangleF(1, 10, 12, 5), sf);
                    }
                }
            }
            return bmp;
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1024L * 1024 * 1024)
                return string.Format("{0:N1} GB", bytes / (1024.0 * 1024 * 1024));
            if (bytes >= 1024L * 1024)
                return string.Format("{0:N1} MB", bytes / (1024.0 * 1024));
            if (bytes >= 1024)
                return string.Format("{0:N1} KB", bytes / 1024.0);
            return bytes > 0 ? string.Format("{0} B", bytes) : "—";
        }

        private static string VirusScanLabel(int status)
        {
            switch (status)
            {
                case 0: return "⏳ รอสแกน";
                case 1: return "✅ สะอาด";
                case 2: return "🦠 พบไวรัส";
                case 3: return "⚠️ สแกนผิดพลาด";
                default: return "—";
            }
        }

        /// <summary>Dialog ที่รับชื่อ Folder พร้อมเลือกระดับ — navy palette style</summary>
        private bool PromptForNameAndLevel(string title, out string name, out int confidentialityLevel, int? lockedLevel = null)
        {
            name = null;
            confidentialityLevel = lockedLevel ?? 1;

            Color navyDark = Color.FromArgb(25, 55, 105);
            Color navyLight = Color.FromArgb(215, 228, 248);
            Color bodyBg = Color.FromArgb(243, 246, 251);
            Color footerBg = Color.FromArgb(228, 234, 245);

            var levelColors = new[]
            {
                Color.FromArgb(204,   0,   0),  // 1HC — Red
                Color.FromArgb(220,  90,   0),  // 2SC — Orange
                Color.FromArgb(  0, 100, 200),  // 3NC — Blue
                Color.FromArgb(  0, 150,  50),  // 4PC — Green
            };

            using (var form = new Form())
            {
                form.Text = title;
                form.ClientSize = new Size(400, 262);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.BackColor = bodyBg;
                form.Font = new System.Drawing.Font("Segoe UI", 9f);

                // ── Header ────────────────────────────────────────────────────
                var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = navyDark };
                var lblHeader = new Label
                {
                    Text = "📁  สร้าง Folder ใหม่",
                    Font = new System.Drawing.Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(14, 14),
                };
                pnlHeader.Controls.Add(lblHeader);

                // ── Sub-header hint bar ───────────────────────────────────────
                var lblHint = new Label
                {
                    Text = "  " + title,
                    Font = new System.Drawing.Font("Segoe UI", 8.5f),
                    ForeColor = Color.FromArgb(50, 70, 110),
                    BackColor = navyLight,
                    Size = new Size(400, 24),
                    Location = new Point(0, 52),
                    TextAlign = ContentAlignment.MiddleLeft,
                };

                // ── Body ──────────────────────────────────────────────────────
                var lblName = new Label
                {
                    Text = "ชื่อ Folder:",
                    Font = new System.Drawing.Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 40, 65),
                    Location = new Point(14, 90),
                    AutoSize = true,
                };

                var input = new TextBox
                {
                    Location = new Point(14, 110),
                    Size = new Size(372, 24),
                    Font = new System.Drawing.Font("Segoe UI", 9.5f),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                };

                // ── Level label ───────────────────────────────────────────────
                var lblLv = new Label
                {
                    Text = lockedLevel.HasValue
                                    ? "ระดับ: (กำหนดโดย Folder แม่ — ไม่สามารถเปลี่ยนได้)"
                                    : "ระดับความลับ:",
                    Font = new System.Drawing.Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 40, 65),
                    Location = new Point(14, 146),
                    AutoSize = true,
                };

                // ── Color-coded combo ─────────────────────────────────────────
                var combo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    DrawMode = DrawMode.OwnerDrawFixed,
                    ItemHeight = 22,
                    Location = new Point(14, 166),
                    Size = new Size(372, 22),
                    Font = new System.Drawing.Font("Segoe UI", 9f),
                };
                combo.Items.AddRange(new object[]
                {
                    "1HC — High Confidential",
                    "2SC — Semi Confidential",
                    "3NC — Normal Confidential",
                    "4PC — Public Confidential",
                });
                combo.SelectedIndex = (lockedLevel.HasValue ? lockedLevel.Value : 1) - 1;
                combo.DrawItem += (s2, e2) =>
                {
                    if (e2.Index < 0) return;
                    e2.DrawBackground();
                    var rc = e2.Bounds;
                    var sq = new Rectangle(rc.X + 4, rc.Y + 4, 14, 14);
                    using (var b = new SolidBrush(levelColors[e2.Index]))
                        e2.Graphics.FillRectangle(b, sq);
                    e2.Graphics.DrawRectangle(Pens.DimGray, sq);
                    using (var b = new SolidBrush(
                        (e2.State & DrawItemState.Disabled) != 0
                            ? SystemColors.GrayText : e2.ForeColor))
                        e2.Graphics.DrawString(
                            combo.Items[e2.Index].ToString(), e2.Font, b,
                            new RectangleF(rc.X + 24, rc.Y + 3, rc.Width - 28, rc.Height));
                    if ((e2.State & DrawItemState.Focus) != 0) e2.DrawFocusRectangle();
                };

                if (lockedLevel.HasValue)
                {
                    combo.Enabled = false;
                    combo.BackColor = Color.FromArgb(235, 238, 245);
                }

                // ── Footer ────────────────────────────────────────────────────
                var pnlFooter = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 46,
                    BackColor = footerBg,
                };

                var btnOk = new Button
                {
                    Text = "ตกลง",
                    Size = new Size(88, 30),
                    Location = new Point(210, 8),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = navyDark,
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9f, FontStyle.Bold),
                    DialogResult = DialogResult.OK,
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text = "ยกเลิก",
                    Size = new Size(88, 30),
                    Location = new Point(304, 8),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(220, 228, 245),
                    ForeColor = navyDark,
                    Font = new System.Drawing.Font("Segoe UI", 9f),
                    DialogResult = DialogResult.Cancel,
                };
                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
                btnCancel.FlatAppearance.BorderSize = 1;

                pnlFooter.Controls.AddRange(new Control[] { btnOk, btnCancel });

                form.Controls.AddRange(new Control[] { pnlHeader, lblHint, lblName, input, lblLv, combo, pnlFooter });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                input.Select();
                if (form.ShowDialog(this) != DialogResult.OK) return false;

                string value = input.Text.Trim();
                if (string.IsNullOrEmpty(value)) return false;

                name = value;
                confidentialityLevel = lockedLevel.HasValue ? lockedLevel.Value : combo.SelectedIndex + 1;
                return true;
            }
        }

        /// <summary>Simple single-line input dialog.</summary>
        private string PromptForName(string title, string prompt, string defaultValue)
        {
            // Color palette
            var navyDark = Color.FromArgb(25, 55, 105);  // #193769
            var navyLight = Color.FromArgb(215, 228, 248); // #D7E4F8
            var bodyBg = Color.FromArgb(243, 246, 251);    // #F3F6FB

            using (var form = new Form())
            using (var pnlHeader = new Panel())
            using (var lblTitle = new Label())
            using (var pnlHint = new Panel())
            using (var lblHint = new Label())
            using (var pnlBody = new Panel())
            using (var label = new Label())
            using (var input = new TextBox())
            using (var ok = new Button())
            using (var cancel = new Button())
            {
                // Form settings
                form.Text = title;
                form.ClientSize = new Size(500, 240);
                form.FormBorderStyle = FormBorderStyle.None;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.BackColor = bodyBg;
                form.Padding = new Padding(1);

                // Header panel (navy dark background + white text bold)
                pnlHeader.Dock = DockStyle.Top;
                pnlHeader.Height = 60;
                pnlHeader.BackColor = navyDark;
                pnlHeader.Padding = new Padding(20, 0, 20, 0);

                lblTitle.Text = "📝 " + title;
                lblTitle.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
                lblTitle.ForeColor = Color.White;
                lblTitle.Dock = DockStyle.Fill;
                lblTitle.TextAlign = ContentAlignment.MiddleLeft;
                pnlHeader.Controls.Add(lblTitle);

                // Hint bar (navy light background + navy dark text)
                pnlHint.Dock = DockStyle.Top;
                pnlHint.Height = 40;
                pnlHint.BackColor = navyLight;
                pnlHint.Padding = new Padding(20, 0, 20, 0);

                lblHint.Text = "💡 คำขอจะส่งไปยัง Admin เพื่อพิจารณาอนุมัติ";
                lblHint.Font = new Font("Segoe UI", 9f);
                lblHint.ForeColor = navyDark;
                lblHint.Dock = DockStyle.Fill;
                lblHint.TextAlign = ContentAlignment.MiddleLeft;
                pnlHint.Controls.Add(lblHint);

                // Body panel (light background)
                pnlBody.Dock = DockStyle.Fill;
                pnlBody.BackColor = bodyBg;
                pnlBody.Padding = new Padding(20);

                label.Text = prompt;
                label.Location = new Point(0, 10);
                label.Size = new Size(440, 25);
                label.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
                label.ForeColor = navyDark;

                input.Text = defaultValue ?? string.Empty;
                input.Location = new Point(0, 40);
                input.Size = new Size(440, 28);
                input.Font = new Font("Segoe UI", 10f);
                input.BorderStyle = BorderStyle.FixedSingle;
                input.SelectAll();

                // OK button (navy dark background + white text bold)
                ok.Text = "ตกลง";
                ok.DialogResult = DialogResult.OK;
                ok.Location = new Point(240, 85);
                ok.Size = new Size(95, 38);
                ok.BackColor = navyDark;
                ok.ForeColor = Color.White;
                ok.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
                ok.FlatStyle = FlatStyle.Flat;
                ok.FlatAppearance.BorderSize = 0;
                ok.Cursor = Cursors.Hand;
                form.AcceptButton = ok;

                // Cancel button (navy light background + navy dark text)
                cancel.Text = "ยกเลิก";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Location = new Point(345, 85);
                cancel.Size = new Size(95, 38);
                cancel.BackColor = navyLight;
                cancel.ForeColor = navyDark;
                cancel.Font = new Font("Segoe UI", 10f);
                cancel.FlatStyle = FlatStyle.Flat;
                cancel.FlatAppearance.BorderSize = 0;
                cancel.Cursor = Cursors.Hand;
                form.CancelButton = cancel;

                pnlBody.Controls.AddRange(new Control[] { label, input, ok, cancel });
                form.Controls.Add(pnlBody);
                form.Controls.Add(pnlHint);
                form.Controls.Add(pnlHeader);

                // Add shadow effect
                form.Paint += (s, e) =>
                {
                    using (var pen = new Pen(Color.FromArgb(100, 100, 100), 1))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, form.Width - 1, form.Height - 1);
                    }
                };

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    string value = input.Text.Trim();
                    return string.IsNullOrEmpty(value) ? null : value;
                }
                return null;
            }
        }

        /// <summary>Handle confidentiality level filter checkbox changes (radio button behavior).</summary>
        private void OnConfidentialityLevelChanged(object sender, EventArgs e)
        {
            // Ensure only one checkbox is checked (radio button behavior)
            if (sender == chk1HC && chk1HC.Checked)
            {
                chk2SC.Checked = false;
                chk3NC.Checked = false;
                chk4PC.Checked = false;
                _activeConfidentialityFilter = 1;
                FilterByConfidentialityLevel(1);
            }
            else if (sender == chk2SC && chk2SC.Checked)
            {
                chk1HC.Checked = false;
                chk3NC.Checked = false;
                chk4PC.Checked = false;
                _activeConfidentialityFilter = 2;
                FilterByConfidentialityLevel(2);
            }
            else if (sender == chk3NC && chk3NC.Checked)
            {
                chk1HC.Checked = false;
                chk2SC.Checked = false;
                chk4PC.Checked = false;
                _activeConfidentialityFilter = 3;
                FilterByConfidentialityLevel(3);
            }
            else if (sender == chk4PC && chk4PC.Checked)
            {
                chk1HC.Checked = false;
                chk2SC.Checked = false;
                chk3NC.Checked = false;
                _activeConfidentialityFilter = 4;
                FilterByConfidentialityLevel(4);
            }
        }

        /// <summary>Filter folder tree by confidentiality level.</summary>
        private void FilterByConfidentialityLevel(int level)
        {
            try
            {
                // จำ folderId ที่เลือกอยู่ก่อน rebuild tree
                var previousFolder = SelectedFolder;
                int? previousFolderId = previousFolder != null ? (int?)previousFolder.FolderId : null;

                folderTreeView.BeginUpdate();
                folderTreeView.Nodes.Clear();

                // Get all folders and filter by selected confidentiality level and exclude deleted folders
                var allFolders = _presenter.GetAllFolders();
                var filteredFolders = allFolders
                    .Where(f => f.ConfidentialityLevel == level && !f.IsDeleted)
                    .OrderBy(f => f.Name)
                    .ToList();

                // Build in-memory parent→children map (no per-node DB calls)
                var byParentFiltered = filteredFolders
                    .GroupBy(f => f.ParentFolderID.HasValue && f.ParentFolderID.Value != 0
                        ? f.ParentFolderID.Value : 0)
                    .ToDictionary(g => g.Key, g => g.OrderBy(f => f.FolderName).ToList());

                foreach (var rootFolder in filteredFolders.Where(f => f.ParentFolderID == null).OrderBy(f => f.Name))
                {
                    var rootNode = CreateFolderNode(rootFolder);
                    BuildChildNodes(rootNode, rootFolder.FolderID, byParentFiltered);
                    folderTreeView.Nodes.Add(rootNode);
                }

                // Auto-expand ถึง Level 2 หลัง filter rebuild
                ExpandTreeToLevel(folderTreeView.Nodes, 0, 2);
                folderTreeView.EndUpdate();

                // Restore selection เดิม → triggers AfterSelect → refreshes right panel
                if (previousFolderId.HasValue)
                    RestoreSelectedNode(folderTreeView.Nodes, previousFolderId.Value);
                else
                {
                    // ไม่ auto-select โฟลเดอร์แรก ให้ user เลือกเอง
                    // Disable events ชั่วคราวเพื่อป้องกัน auto-trigger
                    folderTreeView.AfterSelect -= folderTreeView_AfterSelect;
                    folderTreeView.SelectedNode = null;
                    folderTreeView.AfterSelect += folderTreeView_AfterSelect;
                    ClearContentPanel();
                }
            }
            catch (Exception ex)
            {
                ShowError(string.Format("ข้อผิดพลาดในการกรองข้อมูล: {0}", ex.Message));
            }
        }

        /// <summary>Reload folder tree while respecting the active confidentiality level filter.</summary>
        public void ReloadFolderTreeWithFilter()
        {
            if (_activeConfidentialityFilter > 0)
            {
                // Reload with active filter
                FilterByConfidentialityLevel(_activeConfidentialityFilter);
            }
            else
            {
                // Reload all folders (no filter)
                _presenter.LoadFolderTree(_selectedOrgUnitId);
            }
        }

        // -----------------------------------------------------------------------
        // File list filter — Req 6.1, 6.2, 6.3, 6.4, 6.5
        // -----------------------------------------------------------------------

        private void btnApplyFilter_Click(object sender, EventArgs e)
        {
            ApplyFileFilter();
        }

        private void btnClearFilter_Click(object sender, EventArgs e)
        {
            ClearFileFilter();
        }

        /// <summary>
        /// Filter _currentFiles with AND logic: extension, date range, uploader name.
        /// Sub-folder items in contentsListView are NOT affected (Req 6.5).
        /// Updates lblFilterStatus with count of displayed file items (Req 6.4).
        /// </summary>
        internal void ApplyFileFilter()
        {
            // Collect filter criteria
            string extFilter = cmbExtension.Text.Trim();
            bool filterFrom = dtpFrom.Checked;
            bool filterTo = dtpTo.Checked;
            DateTime fromDate = filterFrom ? dtpFrom.Value.Date : DateTime.MinValue;
            DateTime toDate = filterTo ? dtpTo.Value.Date.AddDays(1).AddTicks(-1) : DateTime.MaxValue;
            string uploaderFilter = txtUploadedBy.Text.Trim();

            // Apply AND logic to _currentFiles
            var filtered = _currentFiles.AsEnumerable();

            if (!string.IsNullOrEmpty(extFilter) && extFilter != "ทั้งหมด")
            {
                string ext = extFilter.StartsWith(".") ? extFilter : "." + extFilter;
                filtered = filtered.Where(f =>
                    string.Equals(f.FileExtension, ext, StringComparison.OrdinalIgnoreCase));
            }

            if (filterFrom)
                filtered = filtered.Where(f => f.UploadedDate.Date >= fromDate);

            if (filterTo)
                filtered = filtered.Where(f => f.UploadedDate <= toDate);

            if (!string.IsNullOrEmpty(uploaderFilter))
                filtered = filtered.Where(f =>
                    f.UploadedByName != null &&
                    f.UploadedByName.IndexOf(uploaderFilter, StringComparison.OrdinalIgnoreCase) >= 0);

            var filteredList = filtered.ToList();

            RefreshFileItemsInListView(filteredList);

            // Show filter status indicator (Req 6.4)
            lblFilterStatus.Text = string.Format("🔍 กรองแล้ว: {0} รายการ", filteredList.Count);
            lblFilterStatus.Visible = true;

            // UX: อัปเดตสีปุ่ม toggle เป็น "กรองอยู่" เมื่อซ่อน filter panel
            var btnToggleFilter = pnlContentHeader.Controls.OfType<Button>()
                .FirstOrDefault(b => b.Name == "btnToggleFilter");
            if (btnToggleFilter != null && !pnlFilter.Visible)
            {
                btnToggleFilter.BackColor = Color.FromArgb(200, 220, 255);
                btnToggleFilter.ForeColor = Color.FromArgb(43, 87, 154);
            }
        }

        /// <summary>
        /// Restore all FileDocument items for the selected folder.
        /// Sub-folder items are preserved (Req 6.5).
        /// Hides lblFilterStatus (Req 6.3).
        /// </summary>
        internal void ClearFileFilter()
        {
            // Clear filter controls
            cmbExtension.Text = "ทั้งหมด";
            dtpFrom.Checked = false;
            dtpTo.Checked = false;
            txtUploadedBy.Text = string.Empty;

            RefreshFileItemsInListView(_currentFiles);

            lblFilterStatus.Text = string.Empty;
            lblFilterStatus.Visible = false;

            // UX: auto-collapse filter bar หลัง clear เพื่อลด cognitive load
            var btnToggleFilter = pnlContentHeader.Controls.OfType<Button>()
                .FirstOrDefault(b => b.Name == "btnToggleFilter");
            if (btnToggleFilter != null)
            {
                pnlFilter.Visible = false;
                btnToggleFilter.BackColor = Color.FromArgb(220, 220, 220);
                btnToggleFilter.ForeColor = Color.FromArgb(60, 60, 60);
            }
        }

        /// <summary>
        /// Rebuild only the FileDocument rows in contentsListView from the given list.
        /// Folder (sub-folder) items are kept intact at the top (Req 6.5).
        /// </summary>
        private void RefreshFileItemsInListView(IEnumerable<FileDocument> filesToShow)
        {
            contentsListView.BeginUpdate();

            // Collect existing sub-folder items (Tag is Folder)
            var folderItems = new List<ListViewItem>();
            foreach (ListViewItem item in contentsListView.Items)
            {
                if (item.Tag is Folder)
                    folderItems.Add(item);
            }

            contentsListView.Items.Clear();

            // Re-add folder items first
            foreach (var fi in folderItems)
                contentsListView.Items.Add(fi);

            // Build OrgUnit lookup for display names
            Dictionary<int, string> orgUnitNames = new Dictionary<int, string>();
            if (_orgLookupController != null)
            {
                try
                {
                    foreach (var ou in _orgLookupController.GetHierarchy())
                        orgUnitNames[ou.OrgUnitID] = ou.OrgUnitNameTH ?? ou.OrgUnitNameEN ?? ou.OrgUnitCode;
                }
                catch { }
            }

            // Add filtered file items
            foreach (var file in filesToShow)
            {
                string icon = GetFileImageKey(file);
                var item = new ListViewItem(file.OriginalFileName) { ImageKey = icon };
                item.SubItems.Add(file.FileExtension.TrimStart('.').ToUpper());
                item.SubItems.Add(ConfidentialityShort(file.ConfidentialityLevel));
                item.SubItems.Add(FormatBytes(file.FileSizeBytes));
                item.SubItems.Add(file.UploadedAt.ToString("dd/MM/yy HH:mm"));
                item.SubItems.Add(file.UploadedByName ?? "—");
                item.SubItems.Add(file.Version.ToString());
                item.SubItems.Add(file.OrgUnitName ?? "—");
                item.Tag = file;
                item.UseItemStyleForSubItems = false;
                item.SubItems[2].ForeColor = ConfidentialityColor(file.ConfidentialityLevel);

                if (file.VirusScanStatus == 2)
                    item.ForeColor = Color.DarkRed;
                else if (file.VirusScanStatus == 3)
                    item.ForeColor = Color.OrangeRed;
                else
                    item.ForeColor = SystemColors.WindowText;

                contentsListView.Items.Add(item);
            }

            contentsListView.EndUpdate();

            // Adjust column widths to fit content
            AdjustNameColumnWidth();
        }

        // -----------------------------------------------------------------------
        // Tree search — Req 3.1, 3.2, 3.3, 3.4, 3.5
        // -----------------------------------------------------------------------

        private void TxtFolderSearch_TextChanged(object sender, EventArgs e)
        {
            // Ignore placeholder text
            string searchText = txtFolderSearch.Text;
            if (searchText == "ค้นหา Folder...")
                searchText = string.Empty;

            FilterFolderTree(searchText);
        }

        /// <summary>
        /// Filter folderTreeView client-side based on searchText.
        /// Shows only nodes whose FolderName contains searchText (case-insensitive) plus all ancestor nodes.
        /// When searchText is empty, restores the full tree.
        /// When no match is found, shows a special "(ไม่พบ Folder)" node.
        /// </summary>
        internal void FilterFolderTree(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                // Req 3.2 — restore full tree
                ReloadFolderTreeWithFilter();
                return;
            }

            // Collect all nodes currently in the tree (full tree must already be loaded)
            var allNodes = new List<TreeNode>();
            CollectAllNodes(folderTreeView.Nodes, allNodes);

            // Find nodes that match the search text (Req 3.1)
            var matchingNodes = new HashSet<TreeNode>();
            foreach (var node in allNodes)
            {
                var folder = node.Tag as Folder;
                if (folder == null) continue;
                if (folder.FolderName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    matchingNodes.Add(node);
            }

            // Include all ancestor nodes for context (Req 3.3)
            var nodesToShow = new HashSet<TreeNode>(matchingNodes);
            foreach (var node in matchingNodes)
            {
                var parent = node.Parent;
                while (parent != null)
                {
                    nodesToShow.Add(parent);
                    parent = parent.Parent;
                }
            }

            folderTreeView.BeginUpdate();
            folderTreeView.Nodes.Clear();

            if (nodesToShow.Count == 0)
            {
                // Req 3.4 — no match: show special node
                var notFoundNode = new TreeNode("(ไม่พบ Folder)")
                {
                    ForeColor = Color.Gray
                };
                folderTreeView.Nodes.Add(notFoundNode);
            }
            else
            {
                // Rebuild tree preserving hierarchy for matched nodes
                RebuildFilteredTree(allNodes, nodesToShow, null, folderTreeView.Nodes);
                folderTreeView.ExpandAll();
            }

            folderTreeView.EndUpdate();
        }

        /// <summary>Collect all TreeNode objects recursively into a flat list.</summary>
        private static void CollectAllNodes(TreeNodeCollection nodes, List<TreeNode> result)
        {
            foreach (TreeNode node in nodes)
            {
                result.Add(node);
                CollectAllNodes(node.Nodes, result);
            }
        }

        /// <summary>
        /// Rebuild a filtered tree by cloning only nodes in nodesToShow,
        /// preserving the original parent-child hierarchy.
        /// </summary>
        private static void RebuildFilteredTree(
            List<TreeNode> allNodes,
            HashSet<TreeNode> nodesToShow,
            TreeNode originalParent,
            TreeNodeCollection targetCollection)
        {
            // Get direct children of originalParent that should be shown
            IEnumerable<TreeNode> children;
            if (originalParent == null)
                children = allNodes.Where(n => n.Parent == null && nodesToShow.Contains(n));
            else
                children = originalParent.Nodes.Cast<TreeNode>().Where(n => nodesToShow.Contains(n));

            foreach (var original in children)
            {
                var clone = new TreeNode(original.Text)
                {
                    Tag = original.Tag,
                    ForeColor = original.ForeColor
                };
                targetCollection.Add(clone);
                RebuildFilteredTree(allNodes, nodesToShow, original, clone.Nodes);
            }
        }

        // -----------------------------------------------------------------------
        // Quick Access / Favorite Folder — Req 7.1, 7.2, 7.3, 7.4, 7.6
        // -----------------------------------------------------------------------

        /// <summary>
        /// สร้าง Quick Access panel ทางด้านซ้ายล่างของ pnlExplorer
        /// แสดงรายการ Folder ที่ user กำหนดเป็น Favorite
        /// </summary>
        private void InitializeQuickAccessPanel()
        {
            // Header label
            var lblHeader = new Label
            {
                Text = "⭐ Quick Access",
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 56, 100),
                Padding = new Padding(4, 3, 0, 0)
            };

            // ListBox แสดงรายการ Quick Access
            _lstQuickAccess = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft Sans Serif", 10f),
                BorderStyle = BorderStyle.None,
                IntegralHeight = false
            };
            _lstQuickAccess.Click += LstQuickAccess_Click;
            _lstQuickAccess.MouseDown += LstQuickAccess_MouseDown;

            // Context menu สำหรับ Quick Access panel
            var ctxQA = new ContextMenuStrip();
            var mnuRemoveQA = new ToolStripMenuItem("🗑️  ลบออกจาก Quick Access", null, CtxRemoveQuickAccess_Click);
            ctxQA.Items.Add(mnuRemoveQA);
            ctxQA.Opening += (s, e) =>
            {
                mnuRemoveQA.Enabled = _lstQuickAccess.SelectedIndex >= 0;
            };
            _lstQuickAccess.ContextMenuStrip = ctxQA;

            // Panel container
            _pnlQuickAccess = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 140,
                BackColor = Color.FromArgb(245, 248, 255),
                Padding = new Padding(0)
            };
            _pnlQuickAccess.Controls.Add(_lstQuickAccess);
            _pnlQuickAccess.Controls.Add(lblHeader);

            // เพิ่มเข้า pnlExplorer (ด้านซ้าย)
            pnlExplorer.Controls.Add(_pnlQuickAccess);

            // โหลดรายการ Quick Access
            LoadQuickAccessPanel();
        }

        /// <summary>โหลดรายการ Quick Access จาก service และแสดงใน _lstQuickAccess</summary>
        private void LoadQuickAccessPanel()
        {
            if (_quickAccessController == null || _lstQuickAccess == null) return;

            try
            {
                _quickAccessItems = _quickAccessController.GetByUser(_user.UserID).ToList();
                _lstQuickAccess.Items.Clear();

                // สร้าง lookup สำหรับ OrgUnit names
                Dictionary<int, string> orgUnitNames = new Dictionary<int, string>();
                if (_orgLookupController != null)
                {
                    try
                    {
                        foreach (var ou in _orgLookupController.GetHierarchy())
                            orgUnitNames[ou.OrgUnitId] = ou.Name ?? ou.OrgUnitCode;
                    }
                    catch { /* ถ้าโหลดไม่ได้ก็ไม่แสดงชื่อ OrgUnit */ }
                }

                foreach (var qa in _quickAccessItems)
                {
                    string displayText;
                    string orgUnitInfo = "";

                    // เพิ่มข้อมูล OrgUnit ถ้ามี
                    if (qa.Folder != null && orgUnitNames.ContainsKey(qa.Folder.OrgUnitID))
                    {
                        orgUnitInfo = $" [{orgUnitNames[qa.Folder.OrgUnitID]}]";
                    }

                    if (qa.Folder == null)
                    {
                        // Folder ถูกลบออกจาก DB แล้ว (cascade delete)
                        displayText = string.Format("⚠️ [ID:{0}] (ไม่พบ Folder)", qa.FolderID);
                    }
                    else if (qa.Folder.IsDeleted)
                    {
                        // Req 7.6: visual indicator สำหรับ folder ที่ถูกลบ
                        displayText = string.Format("🗑️ {0}{1} (ถูกลบ)", qa.Folder.FolderName, orgUnitInfo);
                    }
                    else if (qa.Folder.IsArchived)
                    {
                        // Req 7.6: visual indicator สำหรับ folder ที่ archive
                        displayText = string.Format("📦 {0}{1} (Archived)", qa.Folder.FolderName, orgUnitInfo);
                    }
                    else
                    {
                        displayText = string.Format("📁 {0}{1}", qa.Folder.FolderName, orgUnitInfo);
                    }
                    _lstQuickAccess.Items.Add(displayText);
                }
            }
            catch (Exception)
            {
                // ไม่ให้ error ใน Quick Access ทำให้ form crash
                if (_lstQuickAccess != null)
                {
                    _lstQuickAccess.Items.Clear();
                    _lstQuickAccess.Items.Add("⚠️ โหลด Quick Access ไม่สำเร็จ");
                }
            }
        }

        /// <summary>
        /// Context menu handler: เพิ่ม Folder ที่เลือกใน folderTreeView เข้า Quick Access (Req 7.2)
        /// </summary>
        private void CtxAddQuickAccess_Click(object sender, EventArgs e)
        {
            if (_quickAccessController == null) return;

            var folder = SelectedFolder;
            if (folder == null) return;

            try
            {
                _quickAccessController.Add(_user.UserID, folder.FolderID);
                LoadQuickAccessPanel();
            }
            catch (Exception ex)
            {
                ShowError("ไม่สามารถเพิ่มใน Quick Access ได้: " + ex.Message);
            }
        }

        /// <summary>
        /// Context menu handler: ลบ Folder ที่เลือกออกจาก Quick Access (Req 7.4)
        /// </summary>
        private void CtxRemoveQuickAccess_Click(object sender, EventArgs e)
        {
            if (_quickAccessController == null) return;

            int idx = _lstQuickAccess.SelectedIndex;
            if (idx < 0 || idx >= _quickAccessItems.Count) return;

            var qa = _quickAccessItems[idx];
            try
            {
                _quickAccessController.Remove(_user.UserID, qa.FolderID);
                LoadQuickAccessPanel();
            }
            catch (Exception ex)
            {
                ShowError("ไม่สามารถลบออกจาก Quick Access ได้: " + ex.Message);
            }
        }

        /// <summary>
        /// Updates state and content panel after navigating to a folder via Quick Access.
        /// Mirrors the logic in FolderTreeView_NodeMouseClick so that the right panel refreshes.
        /// </summary>
        private void NavigateToQuickAccessFolder(Folder folder)
        {
            _lastSelectedFolder = folder;
            PushNavigation(folder.FolderID);
            _currentNavFolderId = folder.FolderID;
            UpdateNavButtons();
            _presenter.GetFolderContents(folder.FolderID);
            lblConfidentialityBadge.Text = ConfidentialityLabel(folder.ConfidentialityLevel);
            lblConfidentialityBadge.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
            lblContentInfo.Text = string.Format("กำลังดู: {0}", folder.FolderName);
            lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", folder.FolderName);
        }

        /// <summary>
        /// Click handler: navigate folderTreeView ไปยัง Folder ที่เลือกใน Quick Access (Req 7.3)
        /// </summary>
        private void LstQuickAccess_Click(object sender, EventArgs e)
        {
            int idx = _lstQuickAccess.SelectedIndex;
            if (idx < 0 || idx >= _quickAccessItems.Count) return;

            var qa = _quickAccessItems[idx];
            if (qa.Folder == null || qa.Folder.IsDeleted || qa.Folder.IsArchived)
            {
                string status = qa.Folder == null ? "ไม่พบ Folder นี้ในระบบ"
                    : qa.Folder.IsDeleted ? "Folder นี้ถูกลบแล้ว"
                    : "Folder นี้ถูก Archive แล้ว";
                ShowInfo(string.Format("⚠️ {0} ไม่สามารถนำทางไปได้", status));
                return;
            }

            if (_selectedOrgUnitId != qa.Folder.OrgUnitID)
            {
                _selectedOrgUnitId = qa.Folder.OrgUnitID;
                SelectOrgUnitNode(qa.Folder.OrgUnitID);
                if (_orgLookupController != null)
                {
                    try
                    {
                        var orgUnit = _orgLookupController.GetHierarchy()
                            .FirstOrDefault(u => u.OrgUnitId == qa.Folder.OrgUnitID);
                        _selectedOrgUnitName = orgUnit?.Name ?? "ไม่ทราบชื่อ";
                        lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", _selectedOrgUnitName);
                    }
                    catch
                    {
                        lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: OrgUnit ID {0}", qa.Folder.OrgUnitID);
                    }
                }
                _isNavigating = true;
                try { _presenter.LoadFolderTree(_selectedOrgUnitId); }
                finally { _isNavigating = false; }

                var t = new System.Windows.Forms.Timer { Interval = 100 };
                t.Tick += (s, args) =>
                {
                    t.Stop(); t.Dispose();
                    if (RestoreSelectedNode(folderTreeView.Nodes, qa.Folder.FolderID))
                        NavigateToQuickAccessFolder(qa.Folder);
                    else
                        ShowInfo(string.Format("ไม่สามารถนำทางไปยังโฟลเดอร์ '{0}' ได้", qa.Folder.FolderName));
                };
                t.Start();
            }
            else
            {
                if (RestoreSelectedNode(folderTreeView.Nodes, qa.Folder.FolderID))
                {
                    NavigateToQuickAccessFolder(qa.Folder);
                }
                else
                {
                    _isNavigating = true;
                    try { _presenter.LoadFolderTree(_selectedOrgUnitId); }
                    finally { _isNavigating = false; }

                    var t = new System.Windows.Forms.Timer { Interval = 100 };
                    t.Tick += (s, args) =>
                    {
                        t.Stop(); t.Dispose();
                        if (RestoreSelectedNode(folderTreeView.Nodes, qa.Folder.FolderID))
                            NavigateToQuickAccessFolder(qa.Folder);
                        else
                            ShowInfo(string.Format("ไม่สามารถนำทางไปยังโฟลเดอร์ '{0}' ได้", qa.Folder.FolderName));
                    };
                    t.Start();
                }
            }
        }

        /// <summary>Select item on right-click before showing context menu</summary>
        private void LstQuickAccess_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int idx = _lstQuickAccess.IndexFromPoint(e.Location);
                if (idx >= 0)
                    _lstQuickAccess.SelectedIndex = idx;
            }
        }

        // -----------------------------------------------------------------------
        // Hide Recycle Bin Components
        // -----------------------------------------------------------------------

        /// <summary>
        /// Hides all recycle bin related components from the UI
        /// </summary>
        private void HideRecycleBinComponents()
        {
            // Hide the recycle bin node in the explorer panel
            pnlRecycleBinNode.Visible = false;

            // Fully collapse the bottom panel — set min size to 0 first so it collapses completely
            splitMain.Panel2MinSize = 0;
            splitMain.Panel2Collapsed = true;
            splitMain.IsSplitterFixed = true;
        }

        // -----------------------------------------------------------------------
        // ListViewItemComparer — column sort with folders-before-files invariant
        // (Req 4.1, 4.2, 4.4)
        // -----------------------------------------------------------------------

        /// <summary>
        /// IComparer for contentsListView that:
        ///   1. Always places Folder items before FileDocument items (Req 4.4).
        ///   2. Within each group, sorts by the specified column in the specified direction.
        ///   Supported sort columns: 0 (Name), 3 (Size), 4 (Date).
        ///   All other columns fall back to name comparison.
        /// </summary>
        private sealed class ListViewItemComparer : System.Collections.IComparer
        {
            private readonly int _column;
            private readonly bool _ascending;

            public ListViewItemComparer(int column, bool ascending)
            {
                _column = column;
                _ascending = ascending;
            }

            public int Compare(object x, object y)
            {
                var itemX = (ListViewItem)x;
                var itemY = (ListViewItem)y;

                bool xIsFolder = itemX.Tag is Folder;
                bool yIsFolder = itemY.Tag is Folder;

                // Folders always before files (Req 4.4)
                if (xIsFolder && !yIsFolder) return -1;
                if (!xIsFolder && yIsFolder) return 1;

                // Same type — compare by column
                int result = CompareByColumn(itemX, itemY);
                return _ascending ? result : -result;
            }

            private int CompareByColumn(ListViewItem x, ListViewItem y)
            {
                // Column 3 = Size: compare numerically via Tag
                if (_column == 3)
                {
                    long sizeX = GetSizeBytes(x);
                    long sizeY = GetSizeBytes(y);
                    return sizeX.CompareTo(sizeY);
                }

                // Column 4 = Date: compare via Tag
                if (_column == 4)
                {
                    DateTime dateX = GetDate(x);
                    DateTime dateY = GetDate(y);
                    return dateX.CompareTo(dateY);
                }

                // Default (column 0 = Name, or any other column): string comparison
                string textX = _column < x.SubItems.Count ? x.SubItems[_column].Text : x.Text;
                string textY = _column < y.SubItems.Count ? y.SubItems[_column].Text : y.Text;
                return string.Compare(textX, textY, StringComparison.CurrentCultureIgnoreCase);
            }

            private static long GetSizeBytes(ListViewItem item)
            {
                if (item.Tag is Folder)
                    return ((Folder)item.Tag).FolderSizeBytes;
                if (item.Tag is FileDocument)
                    return ((FileDocument)item.Tag).FileSizeBytes;
                return 0L;
            }

            private static DateTime GetDate(ListViewItem item)
            {
                if (item.Tag is Folder)
                    return ((Folder)item.Tag).CreatedAt;
                if (item.Tag is FileDocument)
                    return ((FileDocument)item.Tag).UploadedAt;
                return DateTime.MinValue;
            }
        }

        private void recycleBinToolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }

    /// <summary>
    /// Helper class for ComboBox items to avoid dynamic type usage
    /// </summary>
    public class OrgUnitItem
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }
}
