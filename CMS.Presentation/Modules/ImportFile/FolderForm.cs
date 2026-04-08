using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Presentation.Modules.ImportFile;
using CMS.Controllers.DMS.Document;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

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

        // Keep a reference to the service for confidentiality level changes (Req 7.5)
        private readonly IFolderService       _folderService;
        private readonly IFileService         _fileService;
        private readonly IOrgStructureService _orgStructureService;
        private readonly IApprovalService     _approvalService;
        private readonly IFolderTemplateRepository _folderTemplateRepository;
        private readonly IUploadCheckpointService  _checkpointService;
        private readonly IQuickAccessService  _quickAccessService;
        private readonly IPermissionService    _permissionService;
        private readonly IFolderRepository     _folderRepository;
        private          IPermissionRepository _permissionRepository;
        private readonly IAuditService         _auditService;
        private readonly IShareService         _shareService;
        private readonly IUserAccountRepository _userAccountRepo;
        private readonly UserContext          _user;

        // Quick Access panel (Req 7)
        private Panel    _pnlQuickAccess;
        private ListBox  _lstQuickAccess;
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
        private TextBox      _inlineRenameBox;
        private ListViewItem _renamingItem;

        // Track active confidentiality level filter (0 = no filter, 1-4 = filtered level)
        private int _activeConfidentialityFilter = 0;

        // View mode toggle (Details / Large Icons / Tile)
        private View _currentView = View.Details;
        // OrgFilter view switcher (ตามองค์กร / ตามความลับ)
        private Panel    _pnlViewButtons;
        private Button   _btnOrgView;
        private Button   _btnConfView;
        private Button   _btnMainMenu;
        private TreeView _treeConfidentiality;
        private TreeView _treeMainMenu;
        private bool     _showingConfView = false;
        private bool     _showingMainMenuView = false;

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        public FolderForm(IFolderService service, UserContext user, int? orgUnitId = null,
                          IFileService fileService = null, IOrgStructureService orgStructureService = null,
                          IFolderTemplateRepository folderTemplateRepository = null,
                          IUploadCheckpointService checkpointService = null,
                          IQuickAccessService quickAccessService = null,
                          IApprovalService approvalService = null,
                          IPermissionService permissionService = null,
                          IFolderRepository folderRepository = null,
                          IPermissionRepository permissionRepository = null,
                          IAuditService auditService = null,
                          IShareService shareService = null,
                          IUserAccountRepository userAccountRepository = null)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (user    == null) throw new ArgumentNullException("user");

            _orgUnitId                = orgUnitId;
            _selectedOrgUnitId        = orgUnitId;
            _folderService            = service;
            _fileService              = fileService;
            _orgStructureService      = orgStructureService;
            _folderTemplateRepository = folderTemplateRepository;
            _checkpointService        = checkpointService;
            _quickAccessService       = quickAccessService;
            _approvalService          = approvalService;
            _permissionService        = permissionService;
            _permissionRepository     = permissionRepository;
            _auditService             = auditService;
            _shareService             = shareService;
            _userAccountRepo          = userAccountRepository;
            _user                     = user;

            InitializeComponent();

            BuildContentImageList();

            _presenter = new FolderPresenter(service, this, user, fileService, folderRepository);
            _folderRepository = folderRepository;

            // Manager/Admin only — ซ่อนปุ่มผู้ดูแลสำหรับ user ทั่วไป
            bool canManage = user.Role == UserRole.Manager || user.Role == UserRole.Admin;

            // Archive — ผู้ดูแลเท่านั้น
            btnArchive.Visible = canManage;
            sep1.Visible       = canManage;   // separator หน้า Archive

            // เปลี่ยนระดับ / ตัดลอกโครงสร้าง — ผู้ดูแลเท่านั้น
            btnChangeLevel.Visible    = canManage;
            btnCopyStructure.Visible  = canManage;
            sep3.Visible              = canManage;   // separator หน้า ChangeLevel

            // Wire content list selection for detail panel
            contentsListView.SelectedIndexChanged += ContentsListView_SelectedIndexChanged;
            
            // Configure ListView for better selection behavior
            contentsListView.FullRowSelect = true;
            contentsListView.HideSelection = false;
            contentsListView.MultiSelect = false;
            
            // Wire mouse events for better interaction
            contentsListView.MouseDown += ContentsListView_MouseDown;
            contentsListView.MouseDoubleClick += ContentsListView_MouseDoubleClick;

            // F2 = inline rename (เหมือน Windows Explorer)
            contentsListView.KeyDown += (s, e2) =>
            {
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
            contentsListView.DrawSubItem      += ContentsListView_DrawSubItem;
            contentsListView.DrawItem         += (s, e) => { /* handled in DrawSubItem */ };

            // ปรับ colName ให้เต็มพื้นที่เมื่อขนาดเปลี่ยน
            contentsListView.Resize += (s, e) => AdjustNameColumnWidth();
            this.Load += (s, e) => AdjustNameColumnWidth();

            // Wire OrgUnit inline treeview — load on form Load
            this.Load += (s, e2) => LoadOrgUnitTree();

            // Build right-click context menu for folderTreeView
            BuildFolderTreeContextMenu();

            // Build right-click context menu for contentsListView
            BuildContentsListViewContextMenu();

            // Wire folder search (Req 3.5)
            txtFolderSearch.TextChanged += TxtFolderSearch_TextChanged;

            // Wire column sort (Req 4.5)
            contentsListView.ColumnClick += ContentsListView_ColumnClick;

            // Wire btnMove (Req 9.1, 9.2, 9.4) — event already wired in Designer.cs, do NOT add again here
            folderTreeView.AfterSelect += (s, e2) => { UpdateMoveButtonState(); OnSelectionChanged(); };
            
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

            // Wire navigation buttons
            btnNavBack.Click    += BtnNavBack_Click;
            btnNavForward.Click += BtnNavForward_Click;
            btnNavUp.Click      += BtnNavUp_Click;
            btnNavBack.Enabled    = false;
            btnNavForward.Enabled = false;
            btnNavUp.Enabled      = false;

            // Detail panel ขนาด 160px — พอดีกับปุ่มและข้อมูล
            splitContent.Panel2MinSize = 160;
            splitContent.SplitterDistance = Math.Max(this.Width - 165, 500);
        }

        // -----------------------------------------------------------------------
        // Navigation history (Back / Forward / Up — เหมือน Windows Explorer)
        // -----------------------------------------------------------------------

        private readonly Stack<int?>             _backStack           = new Stack<int?>();
        private readonly Stack<int?>             _forwardStack        = new Stack<int?>();
        private readonly Stack<string>           _backBreadcrumbs     = new Stack<string>();
        private readonly Stack<string>           _forwardBreadcrumbs  = new Stack<string>();
        private int?                             _currentNavFolderId  = null;   // null = OrgUnit root view
        private string                           _currentBreadcrumb   = "";
        private bool                             _isNavigating        = false;  // ป้องกัน push ซ้ำขณะ navigate
        
        // Breadcrumb navigation data - เก็บ folder ID สำหรับแต่ละส่วนใน breadcrumb
        private readonly List<int?>              _breadcrumbFolderIds = new List<int?>();

        // -----------------------------------------------------------------------
        // Sort state (Req 4.1–4.5)
        // -----------------------------------------------------------------------

        private int  _sortColumn    = -1;   // -1 = no active sort
        private bool _sortAscending = true;


        // -----------------------------------------------------------------------
        private void BuildFolderTreeContextMenu()
        {
            var ctx = new ContextMenuStrip();

            var mnuReload     = new ToolStripMenuItem("🔄  รีโหลด",            null, CtxReload_Click);
            var mnuNewFolder  = new ToolStripMenuItem("📁  Folder ใหม่...",     null, (s, e) => btnNewFolder_Click(s, e));
            var mnuOpen       = new ToolStripMenuItem("📂  เปิด",               null, CtxOpen_Click);
            var mnuDownload   = new ToolStripMenuItem("⬇️  ดาวน์โหลด",          null, (s, e) => OpenUploadForSelectedFolder());
            var mnuCut        = new ToolStripMenuItem("✂️  ตัด",                 null, CtxCut_Click)  { Enabled = false };
            var mnuCopy       = new ToolStripMenuItem("📋  คัดลอก",              null, CtxCopy_Click) { Enabled = false };
            var mnuPaste      = new ToolStripMenuItem("📄  วาง",                 null, CtxPaste_Click){ Enabled = false };
            var mnuRename     = new ToolStripMenuItem("✏️  เปลี่ยนชื่อ",         null, (s, e) => btnRename_Click(s, e));
            var mnuDelete     = new ToolStripMenuItem("🗑️  ลบ",                  null, (s, e) => btnDelete_Click(s, e));
            var mnuArchive    = new ToolStripMenuItem("📦  Archive",              null, (s, e) => btnArchive_Click(s, e));
            var mnuPermission = new ToolStripMenuItem("🔒  ตั้งค่าสิทธิ์...",    null, CtxPermission_Click);
            var mnuProperties = new ToolStripMenuItem("ℹ️  คุณสมบัติ",           null, CtxProperties_Click);
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
            ctx.Items.Add(mnuDelete);
            //ctx.Items.Add(mnuArchive);
            ctx.Items.Add(mnuPermission);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuAddQuickAccess);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuProperties);

            // enable/disable ตาม selection + FolderAuthorityConfig
            ctx.Opening += (s, e) =>
            {
                bool hasFolder       = (SelectedFolder != null);
                int  level           = hasFolder ? SelectedFolder.ConfidentialityLevel : 0;
                bool isPrivileged    = _user.AppRole >= 3;

                bool canView = isPrivileged || (hasFolder && _user.CanViewFolder(level));
                bool canAdd  = isPrivileged || (hasFolder && _user.CanAddToFolder(level));
                bool canEdit = isPrivileged || (hasFolder && _user.CanEditFolder(level));

                bool canDeleteFolder = hasFolder && canEdit && CanDeleteFolder(SelectedFolder);

                mnuNewFolder.Enabled      = hasFolder && canAdd;
                mnuOpen.Enabled           = hasFolder && canView;
                mnuDownload.Enabled       = hasFolder && canView;
                mnuRename.Enabled         = hasFolder && canEdit;
                mnuDelete.Enabled         = canDeleteFolder;
                mnuArchive.Enabled        = hasFolder && canEdit;
                mnuPermission.Enabled     = hasFolder && isPrivileged;
                mnuProperties.Enabled     = hasFolder && canView;
                mnuAddQuickAccess.Enabled = hasFolder && canView && _quickAccessService != null;
                // mnuReload ใช้ได้เสมอ
            };

            folderTreeView.ContextMenuStrip = ctx;

            // right-click ให้ select node ก่อนแสดง menu
            folderTreeView.NodeMouseClick += (s, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    folderTreeView.SelectedNode = e.Node;
            };
        }

        private void CtxReload_Click(object sender, EventArgs e)
        {
            _presenter.LoadFolderTree(_selectedOrgUnitId);
            // _presenter.LoadRecycleBin(_selectedOrgUnitId); // Hidden - recycle bin disabled
        }

        private void BuildContentsListViewContextMenu()
        {
            var ctx = new ContextMenuStrip();

            var mnuUpload     = new ToolStripMenuItem("⬆️  อัพโหลดไฟล์...",   null, (s, e) => OpenUploadForSelectedFolder());
            var mnuNewFolder  = new ToolStripMenuItem("📁  Folder ใหม่...",    null, (s, e) => btnNewFolder_Click(s, e));
            var mnuOpen       = new ToolStripMenuItem("เปิดดู",              null, CtxFileOpen_Click);
            var mnuDownload   = new ToolStripMenuItem("ดาวน์โหลด",           null, CtxFileDownload_Click);
            var mnuCut        = new ToolStripMenuItem("ตัด",                 null, (s, e) => ShowInfo("ฟีเจอร์ตัด/ย้ายไฟล์จะพร้อมใช้งานในเวอร์ชันถัดไป")) { ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X, ShowShortcutKeys = true, Enabled = false };
            var mnuCopy       = new ToolStripMenuItem("คัดลอก",              null, (s, e) => ShowInfo("ฟีเจอร์คัดลอกไฟล์จะพร้อมใช้งานในเวอร์ชันถัดไป")) { ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C, ShowShortcutKeys = true, Enabled = false };
            var mnuPaste      = new ToolStripMenuItem("วาง",                 null, (s, e) => ShowInfo("ฟีเจอร์วางไฟล์จะพร้อมใช้งานในเวอร์ชันถัดไป"))   { ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V, ShowShortcutKeys = true, Enabled = false };
            var mnuRename     = new ToolStripMenuItem("เปลี่ยนชื่อ",         null, (s, e) => btnRename_Click(s, e))                                      { ShortcutKeys = System.Windows.Forms.Keys.F2, ShowShortcutKeys = true };
            var mnuDelete     = new ToolStripMenuItem("ลบ",                  null, (s, e) => btnDelete_Click(s, e))                                       { ShortcutKeys = System.Windows.Forms.Keys.Delete, ShowShortcutKeys = true };
            var mnuClearFolder = new ToolStripMenuItem("🗑️  ล้างไฟล์ทั้งหมดใน Folder...", null, (s, e) => ClearFolderContents());
            var mnuArchive    = new ToolStripMenuItem("Archive",             null, (s, e) => btnArchive_Click(s, e));
            var mnuPermission = new ToolStripMenuItem("ตั้งค่าสิทธิ์...",   null, CtxPermission_Click);
            var mnuProperties = new ToolStripMenuItem("คุณสมบัติ",           null, CtxFileProperties_Click)                                               { ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Enter, ShowShortcutKeys = true };

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
            ctx.Items.Add(mnuDelete);
            ctx.Items.Add(mnuClearFolder);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuArchive);
            ctx.Items.Add(mnuPermission);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuProperties);

            // enable/disable based on selection + FolderAuthorityConfig
            ctx.Opening += (s, e) =>
            {
                bool hasFile   = (SelectedFile != null);
                bool isAtRoot  = !_currentNavFolderId.HasValue;
                bool hasFolder = !isAtRoot && (_currentNavFolderId.HasValue || SelectedFolder != null);

                // level ของ folder ที่กำลังดูอยู่
                int  level        = _lastSelectedFolder != null ? _lastSelectedFolder.ConfidentialityLevel : 0;
                bool isPrivileged = _user.AppRole >= 3;

                bool canView = isPrivileged || _user.CanViewFolder(level);
                bool canAdd  = isPrivileged || _user.CanAddToFolder(level);
                bool canEdit = isPrivileged || _user.CanEditFolder(level);

                // folder ที่เลือกใน content list (กรณี tag เป็น Folder)
                Folder contentFolder = null;
                if (contentsListView.SelectedItems.Count > 0)
                    contentFolder = contentsListView.SelectedItems[0].Tag as Folder;
                bool canDeleteContent = canEdit && (hasFile || (contentFolder != null && CanDeleteFolder(contentFolder)));

                // Check if any item (file or folder) is selected in content list
                bool hasSelection = contentsListView.SelectedItems.Count > 0 && 
                                   (hasFile || contentFolder != null);

                mnuUpload.Enabled      = hasFolder && canAdd  && _fileService != null;
                mnuNewFolder.Enabled   = hasFolder && canAdd;
                mnuOpen.Enabled        = hasFile   && canView;
                mnuDownload.Enabled    = hasFile   && canView;
                mnuRename.Enabled      = hasSelection && canEdit;
                mnuDelete.Enabled      = canDeleteContent;
                mnuClearFolder.Enabled = _fileService != null && _currentNavFolderId.HasValue && canEdit;
                mnuArchive.Enabled     = hasFile   && canEdit;
                mnuPermission.Enabled  = isPrivileged;
                mnuProperties.Enabled  = hasFile   && canView;
            };

            contentsListView.ContextMenuStrip = ctx;
        }

        private async void CtxFileOpen_Click(object sender, EventArgs e)
        {
            var file = SelectedFile;
            if (file == null) return;

            try
            {
                if (_fileService == null)
                {
                    ShowError("File service is not available.");
                    return;
                }

                string filePath = GetFilePhysicalPath(file);

                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                {
                    ShowError(string.Format("ไม่พบไฟล์: {0}", file.OriginalFileName));
                    return;
                }

                string ext = (file.FileExtension ?? System.IO.Path.GetExtension(filePath) ?? string.Empty)
                             .ToLowerInvariant().TrimStart('.');

                if (ext == "pdf")
                {
                    PdfViewOptions options = PdfViewOptions.ViewOnly;

                    if (_permissionService != null)
                    {
                        var eff = _permissionService.GetEffectivePermission(_user.UserID, file.FolderID);
                        options = PdfViewOptions.FromEffective(eff, _user);
                    }

                    await PdfSecurityHelper.OpenSecurePdfAsync(filePath, this, options);
                }
                else
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName        = filePath,
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

        private void CtxFileDownload_Click(object sender, EventArgs e)
        {
            var file = SelectedFile;
            if (file == null) return;

            if (_fileService == null)
            {
                ShowError("File service is not available.");
                return;
            }

            // ให้ user เลือก folder ปลายทาง
            using (var sfd = new SaveFileDialog())
            {
                sfd.Title    = "บันทึกไฟล์";
                sfd.FileName = file.OriginalFileName;

                // กำหนด filter ตาม extension
                string ext = (file.FileExtension ?? "").TrimStart('.').ToLower();
                sfd.Filter = string.Format("{0} files (*.{1})|*.{1}|All files (*.*)|*.*",
                    ext.ToUpper(), ext);
                sfd.DefaultExt = ext;

                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    var result = _fileService.DownloadFile(file.FileID, _user);

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
                    "ชื่อ: {0}\nประเภท: {1}\nขนาด: {2}\nอัพโหลด: {3}\nVersion: {4}\nสถานะ Scan: {5}",
                    file.OriginalFileName,
                    file.FileExtension.TrimStart('.').ToUpper(),
                    FormatBytes(file.FileSizeBytes),
                    file.UploadedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    file.Version,
                    VirusScanLabel(file.VirusScanStatus));
                MessageBox.Show(info, string.Format("คุณสมบัติ — {0}", file.OriginalFileName),
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
                MessageBox.Show(info, string.Format("คุณสมบัติ — {0}", folder.Name),
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
            _presenter.GetFolderContents(folder.FolderId);
        }

        private void CtxCut_Click(object sender, EventArgs e)
        {
            // placeholder — ยังไม่ implement move
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

        // Event สำหรับแจ้ง MDI_Container ให้เปิด PermissionForm (folder)
        public event EventHandler<Folder> OpenPermissionRequested;

        // Event สำหรับแจ้ง MDI_Container ให้เปิด FilePermissionForm (file)
        public event EventHandler<FileDocument> OpenFilePermissionRequested;

        // Event สำหรับแจ้ง MDI_Container เมื่อ selection เปลี่ยน (Req 9.4)
        public event EventHandler SelectionChanged;

        private void OnSelectionChanged()
        {
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
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

            MessageBox.Show(info, string.Format("คุณสมบัติ — {0}", folder.Name),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // -----------------------------------------------------------------------
        // OrgFilter view switcher — ตามองค์กร / ตามความลับ
        // -----------------------------------------------------------------------

        private void InitializeOrgFilterSwitcher()
        {
            var accent    = Color.FromArgb(31, 56, 100);
            var inactive  = Color.FromArgb(240, 244, 250);

            // Button bar
            _pnlViewButtons = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 40,
                BackColor = Color.FromArgb(220, 230, 245),
                Padding   = new Padding(2, 5, 2, 0)
            };

            int btnWidth = 90;

            _btnMainMenu = new Button
            {
                Text      = "☰ เมนูหลัก",
                Width     = btnWidth,
                Height    = 28,
                Location  = new Point(5, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = inactive,
                ForeColor = accent,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            _btnMainMenu.FlatAppearance.BorderSize = 0;

            _btnOrgView = new Button
            {
                Text      = "ตามองค์กร",
                Width     = btnWidth,
                Height    = 28,
                Location  = new Point(5 + btnWidth + 2, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = accent,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            _btnOrgView.FlatAppearance.BorderSize = 0;

            _btnConfView = new Button
            {
                Text      = "🔒 ความลับ",
                Width     = btnWidth,
                Height    = 28,
                Location  = new Point(5 + (btnWidth * 2) + 4, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = inactive,
                ForeColor = accent,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            _btnConfView.FlatAppearance.BorderSize = 0;

            _pnlViewButtons.Controls.Add(_btnMainMenu);
            _pnlViewButtons.Controls.Add(_btnOrgView);
            _pnlViewButtons.Controls.Add(_btnConfView);

            // Confidentiality tree (initially hidden)
            _treeConfidentiality = new TreeView
            {
                Dock          = DockStyle.Fill,
                BackColor     = Color.White,
                BorderStyle   = BorderStyle.None,
                Font          = new Font("Segoe UI", 10f),
                HotTracking   = true,
                Visible       = false,
                Indent        = 16,
                HideSelection = false  // คง highlight node ที่เลือกไว้แม้ focus อยู่ที่ control อื่น
            };

            // Main Menu tree (initially hidden)
            _treeMainMenu = new TreeView
            {
                Dock          = DockStyle.Fill,
                BackColor     = Color.White,
                BorderStyle   = BorderStyle.None,
                Font          = new Font("Segoe UI", 10.5f),
                HotTracking   = true,
                Visible       = false,
                Indent        = 16,
                HideSelection = false,
                ShowLines     = false
            };

            // Re-parent treeOrgUnit into pnlOrgFilter alongside the conf tree
            // Both use DockStyle.Fill and overlap — only one is visible at a time
            pnlOrgFilter.Controls.Remove(treeOrgUnit);
            treeOrgUnit.Dock          = DockStyle.Fill;
            treeOrgUnit.HideSelection = false;  // คง highlight node ที่เลือกไว้แม้ focus อยู่ที่ control อื่น

            pnlOrgFilter.Controls.Add(treeOrgUnit);            // index 0 (front)
            pnlOrgFilter.Controls.Add(_treeConfidentiality);   // index 1
            pnlOrgFilter.Controls.Add(_treeMainMenu);          // index 2
            pnlOrgFilter.Controls.Add(_pnlViewButtons);        // index 3 (back) → docked Top first

            // Wire events
            _btnOrgView.Click  += (s, e) => SwitchToOrgView();
            _btnConfView.Click += (s, e) => SwitchToConfView();
            _btnMainMenu.Click += (s, e) => SwitchToMainMenuView();
            _treeConfidentiality.NodeMouseClick += ConfTree_NodeMouseClick;
            _treeMainMenu.NodeMouseClick += MainMenuTree_NodeMouseClick;
            
            LoadMainMenuTree();
        }

        private void LoadMainMenuTree()
        {
            _treeMainMenu.Nodes.Clear();
            _treeMainMenu.BeginUpdate();

            // 1. จัดการเอกสาร
            var docNode = new TreeNode("📂 จัดการเอกสาร");
            docNode.Nodes.Add(new TreeNode("🏠 หน้าหลัก/โฟลเดอร์") { Tag = "Home" });
            docNode.Nodes.Add(new TreeNode("📄 ไฟล์ที่แชร์กับฉัน") { Tag = "SharedWithMe" });
            docNode.Nodes.Add(new TreeNode("🔗 จัดการลิงก์แชร์")   { Tag = "ShareManagement" });
            docNode.Nodes.Add(new TreeNode("🗑️ ถังขยะ")           { Tag = "RecycleBin" });
            _treeMainMenu.Nodes.Add(docNode);

            // 2. บริหารองค์กร
            var orgNode = new TreeNode("🏢 บริหารองค์กร");
            orgNode.Nodes.Add(new TreeNode("🌳 ผังโครงสร้างองค์กร")    { Tag = "OrgStructure" });
            orgNode.Nodes.Add(new TreeNode("👥 จัดการผู้ใช้งาน")      { Tag = "UserManagement" });
            orgNode.Nodes.Add(new TreeNode("🔄 Sync ข้อมูลพนักงาน")  { Tag = "SyncEmployee" }); // เมนูใหม่
            orgNode.Nodes.Add(new TreeNode("🛡️ จัดการสิทธิ์การใช้งาน") { Tag = "PermissionAdmin" });
            orgNode.Nodes.Add(new TreeNode("👤 สรุปสิทธิ์รายบุคคล")    { Tag = "UserPermissionSummary" });
            _treeMainMenu.Nodes.Add(orgNode);

            // 3. ตรวจสอบระบบ
            var auditNode = new TreeNode("🔍 ตรวจสอบระบบ");
            auditNode.Nodes.Add(new TreeNode("📜 ประวัติการเข้าถึง (Log)") { Tag = "AccessLog" });
            auditNode.Nodes.Add(new TreeNode("📥 รายการรออนุมัติ (Inbox)") { Tag = "ApprovalInbox" });
            _treeMainMenu.Nodes.Add(auditNode);

            // 4. ตั้งค่า
            var settingNode = new TreeNode("⚙️ ตั้งค่า");
            settingNode.Nodes.Add(new TreeNode("🛠️ ตั้งค่าระบบ")           { Tag = "ConfigAdmin" });
            settingNode.Nodes.Add(new TreeNode("📋 ตั้งค่าแม่แบบโฟลเดอร์") { Tag = "FolderTemplate" });
            _treeMainMenu.Nodes.Add(settingNode);

            _treeMainMenu.ExpandAll();
            _treeMainMenu.EndUpdate();
        }

        private void MainMenuTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string tag = e.Node.Tag as string;
            if (string.IsNullOrEmpty(tag)) return;

            // นำทางกลับหน้าหลัก
            if (tag == "Home")
            {
                SwitchToOrgView();
                return;
            }

            try
            {
                Form targetForm = null;

                switch (tag)
                {
                    case "SharedWithMe":
                        // ใช้บริการที่มีอยู่ใน FolderForm ส่งต่อไปยังหน้าจออื่นๆ
                        targetForm = new SharedWithMeForm(_user, _permissionRepository, _folderRepository, null, null, null);
                        break;

                    case "ShareManagement":
                        targetForm = new ShareManagementForm(_shareService, _user, _userAccountRepo, null);
                        break;

                    case "OrgStructure":
                        targetForm = new OrgStructureForm(_orgStructureService, _user, null, null, null);
                        break;

                    case "UserManagement":
                        targetForm = new UserManagementForm(_userAccountRepo, _user, _auditService, null, null, null, null);
                        break;

                    case "PermissionAdmin":
                        // เปิดหน้าจอจัดการสิทธิ์ (ถ้ามี Folder ที่เลือกอยู่จะส่งไปด้วย)
                        targetForm = new PermissionForm(_permissionService, _approvalService, _folderService, _permissionRepository, _user, SelectedFolder, null, _folderRepository, null, _orgStructureService, null, null);
                        break;

                    case "UserPermissionSummary":
                        targetForm = new UserPermissionSummaryForm(_user, _permissionRepository, _folderRepository, null, null, null);
                        break;

                    case "ApprovalInbox":
                        targetForm = new ApprovalInboxForm(_approvalService, _user);
                        break;

                    case "AccessLog":
                        targetForm = new AccessLogView(_shareService, _user, 0);
                        break;

                    case "AuditLog":
                        targetForm = new AuditLogForm(_auditService, _user);
                        break;

                    case "ConfigAdmin":
                        // ConfigAdmin should be opened from MDI_Container which has _configManager
                        MessageBox.Show("กรุณาเปิดผ่าน MDI Container เพื่อเข้าถึงตั้งค่าระบบ", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;

                    case "FolderTemplate":
                        targetForm = new FolderTemplateForm(_folderTemplateRepository, _user);
                        break;

                    case "SyncEmployee":
                        // เรียกฟังก์ชัน Sync (ถ้ามีใน MDI Container หรือสร้างใหม่)
                        MessageBox.Show("กำลังเริ่มกระบวนการ Sync ข้อมูลพนักงานจากระบบ HR...", "Sync ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                }

                if (targetForm != null)
                {
                    // แสดงสถานะที่หน้าจอหลักก่อนเปิด
                    ClearContentPanel();
                    lblContentInfo.Text = "กำลังเปิด: " + e.Node.Text;

                    // All forms open as dialogs (Normal window state), not as MDI children
                    targetForm.WindowState = FormWindowState.Normal;
                    targetForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถเปิดหน้าจอ " + e.Node.Text + " ได้ในขณะนี้\nเนื่องจาก: " + ex.Message, 
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SwitchToOrgView()
        {
            if (!_showingConfView && !_showingMainMenuView) return;
            _showingConfView = false;
            _showingMainMenuView = false;
            
            treeOrgUnit.Visible          = true;
            _treeConfidentiality.Visible = false;
            _treeMainMenu.Visible        = false;
            
            _btnOrgView.BackColor  = Color.FromArgb(31, 56, 100);
            _btnOrgView.ForeColor  = Color.White;
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
            
            treeOrgUnit.Visible          = false;
            _treeConfidentiality.Visible = true;
            _treeMainMenu.Visible        = false;
            
            _btnOrgView.BackColor  = Color.FromArgb(240, 244, 250);
            _btnOrgView.ForeColor  = Color.FromArgb(31, 56, 100);
            _btnConfView.BackColor = Color.FromArgb(31, 56, 100);
            _btnConfView.ForeColor = Color.White;
            _btnMainMenu.BackColor = Color.FromArgb(240, 244, 250);
            _btnMainMenu.ForeColor = Color.FromArgb(31, 56, 100);
            
            BuildConfidentialityTreeFromAllFoldersAsync();
        }

        private void SwitchToMainMenuView()
        {
            if (_showingMainMenuView) return;
            _showingConfView = false;
            _showingMainMenuView = true;
            
            treeOrgUnit.Visible          = false;
            _treeConfidentiality.Visible = false;
            _treeMainMenu.Visible        = true;
            
            _btnOrgView.BackColor  = Color.FromArgb(240, 244, 250);
            _btnOrgView.ForeColor  = Color.FromArgb(31, 56, 100);
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
            _treeConfidentiality.BeginUpdate();
            _treeConfidentiality.Nodes.Clear();
            lblOrgUnitStatus.Text = "⏳ กำลังโหลด ตามความลับ...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                // ดึงข้อมูลทั้งหมดแบบ async เพื่อไม่ block UI
                var result = await System.Threading.Tasks.Task.Run(() =>
                {
                    var allFolders = (_folderRepository != null
                        ? _folderRepository.GetActiveFolders()
                        : _folderService.GetRootFolders(null, _user))
                        .ToList();

                    var childrenByParent = allFolders
                        .Where(f => f.ParentFolderID.HasValue)
                        .GroupBy(f => f.ParentFolderID.Value)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    var allRootFolders = allFolders
                        .Where(f => !f.ParentFolderID.HasValue)
                        .ToList();

                    var allOrgUnits = new List<OrgUnit>();
                    if (_orgStructureService != null)
                        allOrgUnits = _orgStructureService.GetHierarchy()
                            .Where(u => u.IsActive && !u.IsDeleted)
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
                lblOrgUnitStatus.Text = "✓ โหลดเสร็จ";
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
                    string header  = totalFiles > 0
                        ? string.Format("{0}   {1} ไฟล์", group.Label, totalFiles)
                        : group.Label;

                    var groupNode = new TreeNode(header)
                    {
                        ForeColor        = group.Color,
                        NodeFont         = new Font(_treeConfidentiality.Font, FontStyle.Bold),
                        ImageKey         = group.BookKey,
                        SelectedImageKey = group.BookKey,
                        Tag              = null // group header — no Tag
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
                            string scCode  = string.IsNullOrEmpty(sc.OrgUnitCode) ? string.Empty
                                           : string.Format(" ({0})", sc.OrgUnitCode);
                            var scNode = new TreeNode(string.Format("♦ {0}{1}", sc.Name, scCode))
                            {
                                Tag              = sc.OrgUnitID,
                                ForeColor        = group.Color,
                                NodeFont         = new Font(_treeConfidentiality.Font, FontStyle.Regular),
                                ImageKey         = "folder",
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
                                    Tag              = div.OrgUnitID,
                                    ForeColor        = Color.FromArgb(0, 80, 160),
                                    ImageKey         = "folder",
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
                                        Tag              = dept.OrgUnitID,
                                        ForeColor        = Color.FromArgb(60, 60, 60),
                                        ImageKey         = "folder",
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
                                Tag              = orgGroup.Key,
                                ForeColor        = group.Color,
                                NodeFont         = new Font(_treeConfidentiality.Font, FontStyle.Regular),
                                ImageKey         = "folder",
                                SelectedImageKey = "open_folder"
                            };
                            foreach (var rootFolder in orgGroup)
                            {
                                int fc = rootFolder.FileCount;
                                string rlabel = fc > 0
                                    ? string.Format("{0}  [{1}]", rootFolder.FolderName, fc)
                                    : rootFolder.FolderName;
                                var rootNode = new TreeNode(rlabel)
                                {
                                    Tag              = rootFolder,
                                    ForeColor        = group.Color,
                                    ImageKey         = "folder",
                                    SelectedImageKey = "open_folder"
                                };
                                AddConfChildNodes(rootNode, rootFolder.FolderId, group.Color, childrenByParent);
                                ouNode.Nodes.Add(rootNode);
                            }
                            groupNode.Nodes.Add(ouNode);
                        }
                    }

                    _treeConfidentiality.Nodes.Add(groupNode);
                }

                _treeConfidentiality.CollapseAll();
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
                string lbl = f.FolderName;
                var fNode = new TreeNode(lbl)
                {
                    Tag              = f,
                    ForeColor        = color,
                    ImageKey         = "folder",
                    SelectedImageKey = "open_folder"
                };
                AddConfChildNodes(fNode, f.FolderId, color, childrenByParent);
                parentNode.Nodes.Add(fNode);
            }
        }

        private void AddConfChildNodes(TreeNode parentNode, int folderId, Color color,
            Dictionary<int, List<Folder>> childrenByParent = null)
        {
            try
            {
                List<Folder> children;

                if (childrenByParent != null)
                {
                    // Fast path: ใช้ in-memory lookup แทน DB query
                    if (!childrenByParent.TryGetValue(folderId, out children))
                        return;
                }
                else
                {
                    if (_folderRepository == null) return;
                    children = _folderRepository.GetChildren(folderId)
                        .Where(f => !f.IsDeleted && !f.IsArchived)
                        .ToList();
                }

                foreach (var child in children)
                {
                    string label = child.FolderName;
                    var childNode = new TreeNode(label)
                    {
                        Tag       = child,
                        ForeColor = color
                    };

                    AddConfChildNodes(childNode, child.FolderId, color, childrenByParent);
                    parentNode.Nodes.Add(childNode);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("AddConfChildNodes error: " + ex.Message);
            }
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
                    Tag       = sn.Tag,
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
                lblConfidentialityBadge.Text      = ConfidentialityLabel(folder.ConfidentialityLevel);
                lblConfidentialityBadge.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
                lblContentInfo.Text   = string.Format("กำลังดู: {0}", folder.Name);
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
                _selectedOrgUnitId   = orgUnitId;
                _selectedOrgUnitName = nodeName;
                _selectedOrgUnit     = null;
                contentsListView.ShowGroups = false;

                ApplyBreadcrumb("🏠 PC  ›  " + nodeName, new List<int?>());
                lblOrgUnitStatus.Text = string.Format("⏳ กำลังโหลด: {0}...", nodeName);
                
                // แจ้งว่ากำลังนำทาง เพื่อไม่ให้ ShowFolderTree ไป Rebuild Conf Tree อีกรอบ (ล้าง focus/selection)
                _isNavigating = true;
                try { _presenter.LoadFolderTree(orgUnitId); }
                finally { _isNavigating = false; }

                lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", nodeName);
                lblContentInfo.Text   = "";
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
                        item.SubItems.Add(f.CreatedAt.ToLocalTime().ToString("dd/MM/yy HH:mm"));
                        item.SubItems.Add("—");
                        item.SubItems.Add("—");
                        item.SubItems.Add(ouName);
                        item.Tag       = f;
                        item.ForeColor = ConfidentialityColor(f.ConfidentialityLevel);
                        item.UseItemStyleForSubItems = false;
                        item.SubItems[2].ForeColor   = ConfidentialityColor(f.ConfidentialityLevel);
                        contentsListView.Items.Add(item);
                        totalFolders++;
                    }
                }

                contentsListView.EndUpdate();

                ApplyBreadcrumb("🏠 PC  ›  " + nodeName, new List<int?>());
                lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", nodeName);
                lblContentInfo.Text   = string.Format("{0} Folder", totalFolders);
                
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
            if (_orgStructureService == null) return;

            try
            {
                // ใช้ GetVisibleHierarchy เพื่อให้ explicit grants มีผล
                // ถ้า user มี explicit grants → คืนเฉพาะ granted set
                // ถ้าไม่มี → fallback role-based (เหมือนเดิม)
                var units = await System.Threading.Tasks.Task.Run(() =>
                    _orgStructureService.GetVisibleHierarchy(_user)
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
                    Tag              = null,
                    NodeFont         = new System.Drawing.Font(treeOrgUnit.Font, System.Drawing.FontStyle.Bold),
                    ForeColor        = System.Drawing.Color.FromArgb(31, 56, 100),
                    ImageKey         = "folder",
                    SelectedImageKey = "open_folder"
                };
                treeOrgUnit.Nodes.Add(allNode);

                foreach (var sc in visibleUnits.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.SubCommittee).OrderBy(u => u.Name))
                {
                    var scNode = new System.Windows.Forms.TreeNode(sc.Name)
                    {
                        Tag              = sc,
                        ForeColor        = System.Drawing.Color.FromArgb(31, 56, 100),
                        ImageKey         = "organization",
                        SelectedImageKey = "organization"
                    };

                    foreach (var div in visibleUnits.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.Division && u.ParentId == sc.OrgUnitId).OrderBy(u => u.Name))
                    {
                        var divNode = new System.Windows.Forms.TreeNode(div.Name)
                        {
                            Tag              = div,
                            ForeColor        = System.Drawing.Color.FromArgb(0, 80, 160),
                            ImageKey         = "folder",
                            SelectedImageKey = "open_folder"
                        };

                        foreach (var dept in visibleUnits.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.Department && u.ParentId == div.OrgUnitId).OrderBy(u => u.Name))
                        {
                            var deptNode = new System.Windows.Forms.TreeNode(dept.Name)
                            {
                                Tag              = dept,
                                ForeColor        = System.Drawing.Color.FromArgb(60, 60, 60),
                                ImageKey         = "folder",
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

            _selectedOrgUnitId   = (int?)unit.OrgUnitId;
            _selectedOrgUnitName = unit.Name;
            _selectedOrgUnit     = unit;

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
                        var folders = _folderService.GetFoldersByOrgUnit(orgUnitId.Value, _user).ToList();
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

        // -----------------------------------------------------------------------
        // Form load
        // -----------------------------------------------------------------------

        private void FolderForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;

            // Set up folder open/closed icons from Resources (shared ImageList)
            var folderImageList = new ImageList { ImageSize = new System.Drawing.Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };
            folderImageList.Images.Add("folder",         Properties.Resources.folder);
            folderImageList.Images.Add("open_folder",    Properties.Resources.open_folder);
            folderImageList.Images.Add("organization",   Properties.Resources.organization);
            
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
            
            folderImageList.Images.Add("book_red",       Properties.Resources.lightning_bolt1);
            folderImageList.Images.Add("book_orange",    Properties.Resources.book_orange);
            folderImageList.Images.Add("book_bule",      Properties.Resources.book_bule);
            folderImageList.Images.Add("book_green",     Properties.Resources.book_green);

            // TreeView — closed/open on expand/collapse
            folderTreeView.ImageList        = folderImageList;
            folderTreeView.ImageKey         = "folder";
            folderTreeView.SelectedImageKey = "open_folder";
            folderTreeView.BeforeExpand  += (s, ev) =>
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
            treeOrgUnit.ImageList        = folderImageList;
            treeOrgUnit.ImageKey         = "folder";
            treeOrgUnit.SelectedImageKey = "open_folder";
            treeOrgUnit.BeforeExpand  += (s, ev) => { if (ev.Node.ImageKey != "organization") ev.Node.ImageKey = ev.Node.SelectedImageKey = "open_folder"; };
            treeOrgUnit.BeforeCollapse += (s, ev) => { if (ev.Node.ImageKey != "organization") ev.Node.ImageKey = ev.Node.SelectedImageKey = "folder"; };

            // Confidentiality TreeView — same image list (WinForms requires a SEPARATE ImageList instance per TreeView)
            var confImageList = new ImageList { ImageSize = new System.Drawing.Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };
            confImageList.Images.Add("folder",         Properties.Resources.folder);
            confImageList.Images.Add("open_folder",    Properties.Resources.open_folder);
            confImageList.Images.Add("organization",   Properties.Resources.organization);
            
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
            
            confImageList.Images.Add("book_red",       Properties.Resources.lightning_bolt);
            confImageList.Images.Add("book_orange",    Properties.Resources.book_orange);
            confImageList.Images.Add("book_bule",      Properties.Resources.book_bule);
            confImageList.Images.Add("book_green",     Properties.Resources.book_green1);
            _treeConfidentiality.ImageList        = confImageList;
            _treeConfidentiality.ImageKey         = "folder";
            _treeConfidentiality.SelectedImageKey = "open_folder";
            _treeConfidentiality.BeforeExpand  += (s, ev) =>
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

            // ListView needs its own separate ImageList (WinForms does not allow sharing with TreeView)
            var listImageList = new ImageList { ImageSize = new System.Drawing.Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };
            listImageList.Images.Add("folder",      Properties.Resources.folder);
            try { listImageList.Images.Add("lightning_bolt", Properties.Resources.lightning_bolt1); }
            catch { listImageList.Images.Add("lightning_bolt", Properties.Resources.book_red1.ToBitmap()); }
            listImageList.Images.Add("book_red",    Properties.Resources.book_red1.ToBitmap());
            listImageList.Images.Add("book_orange", Properties.Resources.book_orange);
            listImageList.Images.Add("book_bule",   Properties.Resources.book_bule);
            listImageList.Images.Add("book_green",  Properties.Resources.book_green);
            listImageList.Images.Add("file_docs",   Properties.Resources.File_docs.ToBitmap());
            contentsListView.SmallImageList = listImageList;

            // ── LargeImageList (32×32) for Large Icon view ────────────────────
            var largeImageList = new ImageList { ImageSize = new Size(32, 32), ColorDepth = ColorDepth.Depth32Bit };
            largeImageList.Images.Add("folder",      new Bitmap(Properties.Resources.folder,      32, 32));
            try { largeImageList.Images.Add("lightning_bolt", new Bitmap(Properties.Resources.lightning_bolt1, 32, 32)); }
            catch { largeImageList.Images.Add("lightning_bolt", new Bitmap(Properties.Resources.book_red1.ToBitmap(), 32, 32)); }
            largeImageList.Images.Add("book_red",    new Bitmap(Properties.Resources.lightning_bolt1,    32, 32));
            largeImageList.Images.Add("book_orange", new Bitmap(Properties.Resources.book_orange, 32, 32));
            largeImageList.Images.Add("book_bule",   new Bitmap(Properties.Resources.book_bule,   32, 32));
            largeImageList.Images.Add("book_green",  new Bitmap(Properties.Resources.book_green,  32, 32));
            largeImageList.Images.Add("file_docs",   new Bitmap(Properties.Resources.File_docs.ToBitmap(),    32, 32));
            contentsListView.LargeImageList = largeImageList;

            // ── View toggle buttons (Details / Tile / Large Icons) ───────────
            Color activeColor   = Color.FromArgb(31, 56, 100);
            Color inactiveColor = Color.FromArgb(220, 220, 220);
            Color activeFg      = Color.White;
            Color inactiveFg    = Color.FromArgb(60, 60, 60);

            var btnViewDetails = new Button
            {
                Text      = "☰",
                Size      = new Size(28, 22),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f),
                Cursor    = Cursors.Hand,
                Dock      = DockStyle.Right,
                BackColor = activeColor,
                ForeColor = activeFg,
                TabStop   = false
            };
            btnViewDetails.FlatAppearance.BorderSize = 0;

            var btnViewTile = new Button
            {
                Text      = "▤",
                Size      = new Size(28, 22),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f),
                Cursor    = Cursors.Hand,
                Dock      = DockStyle.Right,
                BackColor = inactiveColor,
                ForeColor = inactiveFg,
                TabStop   = false
            };
            btnViewTile.FlatAppearance.BorderSize = 0;

            var btnViewLarge = new Button
            {
                Text      = "⊞",
                Size      = new Size(28, 22),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f),
                Cursor    = Cursors.Hand,
                Dock      = DockStyle.Right,
                BackColor = inactiveColor,
                ForeColor = inactiveFg,
                TabStop   = false
            };
            btnViewLarge.FlatAppearance.BorderSize = 0;

            var viewTip = new ToolTip();
            viewTip.SetToolTip(btnViewDetails, "รายการ (Details)");
            viewTip.SetToolTip(btnViewTile,    "Tile");
            viewTip.SetToolTip(btnViewLarge,   "ไอคอนใหญ่");

            // helper: reset all buttons then highlight active one
            Action<Button> setActive = (active) =>
            {
                foreach (var b in new[] { btnViewDetails, btnViewTile, btnViewLarge })
                {
                    b.BackColor = (b == active) ? activeColor   : inactiveColor;
                    b.ForeColor = (b == active) ? activeFg      : inactiveFg;
                }
            };

            btnViewDetails.Click += (s, ev) =>
            {
                _currentView = View.Details;
                contentsListView.View      = View.Details;
                contentsListView.OwnerDraw = true;
                setActive(btnViewDetails);
            };

            btnViewTile.Click += (s, ev) =>
            {
                _currentView = View.Tile;
                contentsListView.OwnerDraw = false;
                contentsListView.View      = View.Tile;
                // Tile size: icon(32) + padding + text area
                contentsListView.TileSize  = new Size(260, 42);
                setActive(btnViewTile);
            };

            btnViewLarge.Click += (s, ev) =>
            {
                _currentView = View.LargeIcon;
                contentsListView.OwnerDraw = false;
                contentsListView.View      = View.LargeIcon;
                setActive(btnViewLarge);
            };

            // Add right-to-left so Details is rightmost
            pnlContentHeader.Controls.Add(btnViewDetails);
            pnlContentHeader.Controls.Add(btnViewTile);
            pnlContentHeader.Controls.Add(btnViewLarge);

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
                           || (files      != null && files.Any());
            if (folderTreeView.SelectedNode == null && !hasContent)
            {
                ClearContentPanel();
                return;
            }
            
            contentsListView.BeginUpdate();
            contentsListView.Items.Clear();
            _currentFiles.Clear();

            // Filter out deleted items
            var folderList = subFolders == null ? new List<Folder>() : subFolders.Where(f => !f.IsDeleted).ToList();
            var fileList   = files      == null ? new List<FileDocument>() : files.Where(f => !f.IsDeleted).ToList();

            // Use cached OrgUnit lookup (populated by LoadOrgUnitTree) — no DB call here
            Dictionary<int, string> orgUnitNames = _orgUnitNameCache ?? new Dictionary<int, string>();

            // ── Sub-folders first ──────────────────────────────────────────
            foreach (var folder in folderList)
            {
                // Populate OrgUnitName
                string ouName;
                folder.OrgUnitName = orgUnitNames.TryGetValue(folder.OrgUnitID, out ouName) ? ouName : "—";

                // Name column: name + subtitle hint (icon comes from ImageList)
                string nameText = folder.Name;
                // 4 Folder หลัก (root) ใช้ book icon ตามระดับความลับ; folder ลูกใช้ folder icon ทั่วไป
                bool isFolderRoot = !folder.ParentFolderID.HasValue || folder.ParentFolderID == 0;
                string iconKey  = isFolderRoot
                    ? ConfidentialityBookIcon(folder.ConfidentialityLevel)
                    : "folder";
                var item = new ListViewItem(nameText) { ImageKey = iconKey };
                item.SubItems.Add(folder.FileCount > 0
                    ? string.Format("Folder  {0} ไฟล์", folder.FileCount)
                    : "Folder");                                          // ประเภท
                item.SubItems.Add(ConfidentialityShort(folder.ConfidentialityLevel)); // ระดับ
                item.SubItems.Add(FormatBytes(folder.FolderSizeBytes));              // ขนาด
                item.SubItems.Add(folder.CreatedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")); // วันที่
                item.SubItems.Add(folder.CreatedByName ?? "—");                      // โดย
                item.SubItems.Add("—");                                              // Ver
                item.SubItems.Add(folder.OrgUnitName ?? "—");                        // หน่วยงาน
                
                // Check permissions for this folder
                string permissionText = GetFolderPermissionText(folder);
                var permSubItem = item.SubItems.Add(permissionText);                 // สิทธิ์
                permSubItem.ForeColor = GetPermissionColor(permissionText);
                
                item.Tag       = folder;
                item.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
                item.UseItemStyleForSubItems = false;
                // Confidentiality badge sub-item colored
                item.SubItems[2].ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
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

                string icon = FileIcon(file.FileExtension);
                var item = new ListViewItem(file.OriginalFileName) { ImageKey = icon };
                item.SubItems.Add(file.FileExtension.TrimStart('.').ToUpper()); // ประเภท
                item.SubItems.Add(ConfidentialityShort(file.ConfidentialityLevel)); // ระดับ
                item.SubItems.Add(FormatBytes(file.FileSizeBytes));              // ขนาด
                item.SubItems.Add(file.UploadedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")); // วันที่
                item.SubItems.Add(file.UploadedByName ?? "—");                  // โดย
                item.SubItems.Add(file.Version.ToString());                      // Ver
                item.SubItems.Add(file.OrgUnitName ?? "—");                      // หน่วยงาน
                item.SubItems.Add("—");                                          // สิทธิ์ (files inherit from folder)
                item.Tag = file;
                item.UseItemStyleForSubItems = false;

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
            cmbExtension.Items.Add(string.Empty); // "all" option
            foreach (var ext in fileList.Select(f => f.FileExtension).Distinct().OrderBy(e => e))
                if (!string.IsNullOrEmpty(ext))
                    cmbExtension.Items.Add(ext);
            cmbExtension.Text = string.Empty;

            // Clear filter status when folder changes (Req 6.3)
            lblFilterStatus.Text    = string.Empty;
            lblFilterStatus.Visible = false;

            // Update content header info
            lblContentInfo.ForeColor = SystemColors.ControlText;
            lblContentInfo.Text = string.Format(
                "{0} Folder, {1} ไฟล์  |  ขนาดรวม {2}  |  ไม่มีรายการที่เลือก",
                folderList.Count, fileList.Count,
                FormatBytes(fileList.Sum(f => f.FileSizeBytes)));

            // Adjust column widths to fit content
            AdjustNameColumnWidth();

            _currentSubFolders = folderList;
        }

        /// <summary>
        /// อัปเดต panel ล่างซ้ายให้แสดง folders + files เดียวกับ content panel ด้านขวา
        /// </summary>
        private void RefreshSubfolderPanel(List<Folder> folders = null, List<FileDocument> files = null)
        {
            if (_lstQuickAccess == null) return;

            _lstQuickAccess.Items.Clear();

            bool hasFolders = folders != null && folders.Count > 0;
            bool hasFiles   = files   != null && files.Count   > 0;

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
                lblContentInfo.Text      = message;
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
            lblContentInfo.Text      = "🔒 ไม่มีสิทธิ์เข้าถึง Folder นี้";
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

        public Folder SelectedRecycleBinFolder => null;  // Recycle bin moved to RecycleBinForm

        public FileDocument SelectedFile
        {
            get
            {
                if (contentsListView.SelectedItems.Count == 0) return null;
                return contentsListView.SelectedItems[0].Tag as FileDocument;
            }
        }

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
                    // Clear existing selection and select the clicked item
                    contentsListView.SelectedItems.Clear();
                    hitTest.Item.Selected = true;
                    hitTest.Item.Focused = true;
                }
                else
                {
                    // Clicked on empty area - clear selection
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
                PushNavigation(folder.FolderId);
                _lastSelectedFolder = folder;
                _currentNavFolderId = folder.FolderId;

                _presenter.GetFolderContents(folder.FolderId);

                // Build breadcrumb จาก tree hierarchy แทน string-append
                // เพื่อป้องกัน breadcrumb ยาวซ้ำเมื่อ navigate หลายชั้น
                bool found = RestoreSelectedNode(folderTreeView.Nodes, folder.FolderId);
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

                lblContentInfo.Text = string.Format("{0} รายการ | {1} | {2}",
                    folder.FileCount, ConfidentialityLabel(folder.ConfidentialityLevel), folder.FolderName);
                lblConfidentialityBadge.Text      = ConfidentialityLabel(folder.ConfidentialityLevel);
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

                lblDetailHeader.Text  = "📋 ตัวอย่าง";
                lblDetailContent.Text = "เลือกรายการเพื่อดูรายละเอียด";
                lblContentInfo.Text   = lblContentInfo.Text.Contains("|")
                    ? lblContentInfo.Text.Substring(0, lblContentInfo.Text.LastIndexOf('|') + 1)
                      + " ไม่มีรายการที่เลือก"
                    : lblContentInfo.Text;
                return;
            }

            splitContent.Panel2Collapsed = false;

            var tag    = contentsListView.SelectedItems[0].Tag;
            var file   = tag as FileDocument;
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
                Location = new Point(0, 0), Size = new Size(pw, 42),
                BackColor = Color.FromArgb(31, 56, 100), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            headerPanel.Controls.Add(new Label
            {
                Text = "📄", Font = new Font("Segoe UI", 13f),
                ForeColor = Color.White, Location = new Point(4, 6), Size = new Size(26, 30)
            });
            headerPanel.Controls.Add(new Label
            {
                Text = file.OriginalFileName, Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.White, Location = new Point(32, 4), Size = new Size(pw - 36, 34),
                TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
            });

            // ── Details rows ─────────────────────────────────────────
            var details = new[]
            {
                new { Label = "ชนิด",  Value = file.FileExtension.TrimStart('.').ToUpper() },
                new { Label = "ระดับ", Value = ConfidentialityShort(file.ConfidentialityLevel) },
                new { Label = "ขนาด",  Value = FormatBytes(file.FileSizeBytes) },
                new { Label = "วันที่", Value = file.UploadedAt.ToLocalTime().ToString("dd/MM/yyyy") }
            };

            int rowH  = 22;
            int startY = 48;
            foreach (var d in details)
            {
                pnlDetail.Controls.Add(new Label
                {
                    Text = d.Label + ":", Font = new Font("Segoe UI", 8f),
                    Location = new Point(6, startY), Size = new Size(38, rowH),
                    TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.Gray
                });
                pnlDetail.Controls.Add(new Label
                {
                    Text = d.Value, Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    Location = new Point(46, startY), Size = new Size(pw - 50, rowH),
                    TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
                });
                startY += rowH;
            }

            // ── Action buttons (Bottom) ───────────────────────────────
            var btnOpen = new Button
            {
                Text = "🔍  เปิดดู",
                Location = new Point(6, startY + 8), Size = new Size(pw - 12, 30),
                BackColor = Color.FromArgb(43, 87, 154), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            btnOpen.FlatAppearance.BorderSize = 0;
            btnOpen.Click += CtxFileOpen_Click;

            var btnDownload = new Button
            {
                Text = "⬇  ดาวน์โหลด",
                Location = new Point(6, startY + 46), Size = new Size(pw - 12, 30),
                BackColor = Color.FromArgb(56, 142, 60), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand,
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
                Location = new Point(0, 0), Size = new Size(pw, 42),
                BackColor = Color.FromArgb(31, 56, 100), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            headerPanel.Controls.Add(new Label
            {
                Text = "📁", Font = new Font("Segoe UI", 13f),
                ForeColor = Color.White, Location = new Point(4, 6), Size = new Size(26, 30)
            });
            var lblFolderName = new Label
            {
                Text = folder.FolderName, Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.White, Location = new Point(32, 4), Size = new Size(pw - 36, 34),
                TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true,
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
                Text = "คลิกเพื่อเปิด", Font = new Font("Segoe UI", 6f, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 220, 255), Location = new Point(32, 28),
                Size = new Size(pw - 36, 12), TextAlign = ContentAlignment.MiddleLeft,
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

            int rowH  = 22;
            int startY = 48;
            foreach (var d in details)
            {
                pnlDetail.Controls.Add(new Label
                {
                    Text = d.Label + ":", Font = new Font("Segoe UI", 8f),
                    Location = new Point(6, startY), Size = new Size(38, rowH),
                    TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.Gray
                });
                pnlDetail.Controls.Add(new Label
                {
                    Text = d.Value, Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    Location = new Point(46, startY), Size = new Size(pw - 50, rowH),
                    TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
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
            bool isEven     = e.ItemIndex % 2 == 0;

            Color backColor = isSelected
                ? Color.FromArgb(204, 228, 247)
                : isEven ? Color.White : Color.FromArgb(248, 249, 252);

            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            Color textColor = isSelected ? Color.Black : e.SubItem.ForeColor;
            if (textColor == Color.Empty || textColor == SystemColors.WindowText)
                textColor = isSelected ? Color.Black : e.Item.ForeColor;
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
                _sortColumn    = e.Column;
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

            _isAdjustingColumns = true;
            try
            {
                contentsListView.SuspendLayout();

                // Auto-resize ทุกคอลัมน์ (รวม index 0) ให้พอดีกับเนื้อหาและหัวข้อ
                for (int i = 0; i < contentsListView.Columns.Count; i++)
                {
                    // วัดความกว้างจากเนื้อหา
                    contentsListView.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                    int contentWidth = contentsListView.Columns[i].Width + 15;

                    // วัดความกว้างจากหัวข้อคอลัมน์ (Header)
                    contentsListView.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
                    int headerWidth = contentsListView.Columns[i].Width;

                    // เลือกค่าที่มากที่สุด (บวก padding เผื่อไว้นิดหน่อยเพื่อให้ดูไม่อึดอัด)
                    int targetWidth = Math.Max(contentWidth, headerWidth) + 10;

                    // กำหนดความกว้างขั้นต่ำของแต่ละคอลัมน์เพื่อไม่ให้แคบจนเกินไป
                    if (targetWidth < 100)
                        targetWidth = 100;
                    
                    contentsListView.Columns[i].Width = targetWidth;
                }
            }
            finally
            {
                contentsListView.ResumeLayout();
                _isAdjustingColumns = false;
            }
        }

        private void ClearContentPanel()
        {
            contentsListView.Items.Clear();
            _currentFiles.Clear();
            _currentSubFolders.Clear();
            txtBreadcrumb.Text            = "";
            lblContentInfo.Text           = "";
            lblConfidentialityBadge.Text  = "";
            lblFilterStatus.Text          = string.Empty;
            lblFilterStatus.Visible       = false;
            cmbExtension.Items.Clear();

            // Hide detail panel when clearing content
            splitContent.Panel2Collapsed = true;
        }

        private void FolderTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            
            var folder = e.Node?.Tag as Folder;
            if (folder == null) return;
            
            // PushNavigation ก่อน — ต้อง push _currentNavFolderId เก่าเข้า stack ก่อนที่จะ update
            if (!_isNavigating)
                PushNavigation(folder.FolderId);

            _lastSelectedFolder = folder;
            _currentNavFolderId = folder.FolderId;
            UpdateNavButtons();
            
            _presenter.GetFolderContents(folder.FolderId);

            // Build breadcrumb จาก tree hierarchy พร้อม OrgUnit prefix (ไม่ใช้ string-append)
            string breadcrumbPath = _selectedOrgUnit != null
                ? BuildBreadcrumbPathFromOrgUnit(_selectedOrgUnit, e.Node)
                : BuildBreadcrumbPath(treeOrgUnit.SelectedNode, e.Node);

            var folderIdPath = BuildFolderIdPathAligned(breadcrumbPath, e.Node);
            _breadcrumbFolderIds.Clear();
            _breadcrumbFolderIds.AddRange(folderIdPath);
            ApplyBreadcrumb(breadcrumbPath, folderIdPath);

            lblConfidentialityBadge.Text      = ConfidentialityLabel(folder.ConfidentialityLevel);
            lblConfidentialityBadge.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
            lblContentInfo.Text   = string.Format("กำลังดู: {0}", folder.Name);
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
            UpdateNavButtons();
        }

        // -----------------------------------------------------------------------
        // NavBar button handlers
        // -----------------------------------------------------------------------

        private void btnNavRefresh_Click(object sender, EventArgs e)
        {
            btnNavRefresh.Text    = "↻";
            btnNavRefresh.Enabled = false;

            _presenter.LoadFolderTree(_selectedOrgUnitId);
            // _presenter.LoadRecycleBin(_selectedOrgUnitId); // Hidden - recycle bin disabled

            btnNavRefresh.Enabled = true;
            btnNavRefresh.Text    = "↺";
        }

        private void BtnNavBack_Click(object sender, EventArgs e)
        {
            if (_backStack.Count == 0) return;

            // เก็บ current ลง forward stack ก่อน
            _forwardStack.Push(_currentNavFolderId);
            _forwardBreadcrumbs.Push(_currentBreadcrumb);

            // pop previous state
            var prevFolderId   = _backStack.Pop();
            var prevBreadcrumb = _backBreadcrumbs.Count > 0 ? _backBreadcrumbs.Pop() : "";

            _isNavigating = true;
            try
            {
                _currentNavFolderId = prevFolderId;
                _currentBreadcrumb  = prevBreadcrumb;
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
            var nextFolderId   = _forwardStack.Pop();
            var nextBreadcrumb = _forwardBreadcrumbs.Count > 0 ? _forwardBreadcrumbs.Pop() : "";

            _isNavigating = true;
            try
            {
                _currentNavFolderId = nextFolderId;
                _currentBreadcrumb  = nextBreadcrumb;
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
            _currentBreadcrumb  = parentBreadcrumb;
            _isNavigating = true;
            try   { ExecuteNavigation(parentFolderId, parentBreadcrumb); }
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
                
                // โหลดทั้ง sub-folders และ files เหมือน Windows Explorer (Back/Forward ต้องแสดงไฟล์ด้วย)
                _presenter.GetFolderContents(folderId.Value);
                ApplyBreadcrumb(breadcrumbOverride ?? _currentBreadcrumb, null);
                
                // Update UI
                var node = FindNodeByFolderId(folderTreeView.Nodes, folderId.Value);
                var folder = node?.Tag as Folder;
                if (folder != null)
                {
                    lblConfidentialityBadge.Text      = ConfidentialityLabel(folder.ConfidentialityLevel);
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
                    Text     = part,
                    AutoSize = true,
                    Location = new Point(x, y),
                    Font     = new Font("Segoe UI", 10f, isLast ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = isLast ? Color.Black : Color.FromArgb(0, 102, 204),
                    Cursor   = isLast ? Cursors.Default : Cursors.Hand,
                    Tag      = folderId  // เก็บ folder ID โดยตรง
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
                        Text = "›", AutoSize = true,
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

            if (_folderRepository != null)
                folder = _folderRepository.GetById(folderId);

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
            
            lblConfidentialityBadge.Text      = ConfidentialityLabel(folder.ConfidentialityLevel);
            lblConfidentialityBadge.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
            lblContentInfo.Text   = string.Format("กำลังดู: {0}", folder.FolderName);
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
            btnNavBack.Enabled    = _backStack.Count > 0;
            btnNavForward.Enabled = _forwardStack.Count > 0;
            btnNavUp.Enabled      = _currentNavFolderId.HasValue;
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
        /// ถ้ามี _folderTemplateRepository จะแสดงตัวเลือก Template ให้
        /// </summary>
        private void CreateSubFolderWithChoice(Folder parent, int? lockedLevel)
        {
            // ถ้ามี template repository → เสนอทางเลือก
            if (_folderTemplateRepository != null && _folderService != null && _orgStructureService != null)
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
            using (var dlg = new ApplyTemplateDialog(_folderService, _folderTemplateRepository, _orgStructureService, _user, parent))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _presenter.LoadFolderTree(_selectedOrgUnitId);
                    _presenter.GetFolderContents(parent.FolderId);
                    lblContentInfo.Text   = string.Format("กำลังดู: {0}", parent.FolderName);
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
            int    level;
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
            if (!_selectedOrgUnitId.HasValue) return;

            // ตรวจสอบว่ามี 4 folder หลัก (1HC/2SC/3NC/4PC) หรือยัง
            bool has4Folders = HasAuto4RootFolders();
            if (has4Folders) return; // มีแล้ว ไม่ต้องถาม

            string orgName = _selectedOrgUnitName ?? "หน่วยงานนี้";
            var result = MessageBox.Show(
                string.Format("หน่วยงาน \"{0}\" ยังไม่มีโฟลเดอร์หลัก\n\nต้องการสร้าง 4 Folder หลัก (1HC, 2SC, 3NC, 4PC) หรือไม่?", orgName),
                "สร้างโฟลเดอร์หลัก",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                CreateAuto4FoldersQuiet("", null, _selectedOrgUnitId.Value);
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
                ShowError("กรุณาเปิด Folder ก่อนอัพโหลด\nไม่สามารถอัพโหลดไฟล์ที่ระดับ Root ได้");
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
                ShowError("กรุณาเลือก Folder ปลายทางก่อนอัพโหลด");
                return;
            }

            if (_fileService == null || _folderService == null)
            {
                ShowError("Upload service is not available.");
                return;
            }

            // เก็บ folder ID ที่กำลังดูอยู่ก่อน upload
            int uploadFolderId = folder.FolderId;

            var uploadForm = new FileUploadForm(_fileService, _folderService, _user, folder, _checkpointService);
            var result = uploadForm.ShowDialog(this.MdiParent ?? this);
            
            if (result == DialogResult.OK)
            {
                // Reload folder tree ก่อน (file count update)
                // แต่ต้องป้องกันไม่ให้ AfterSelect override content ที่จะแสดง
                _isNavigating = true;
                try
                {
                    _presenter.LoadFolderTree(_selectedOrgUnitId);
                }
                finally
                {
                    _isNavigating = false;
                }
                
                // อัพเดท state
                _currentNavFolderId = uploadFolderId;
                _lastSelectedFolder = folder;
                
                // Reload เนื้อหาของ folder ที่ upload ไป (ทำหลังสุดเพื่อไม่ให้ถูก override)
                _presenter.GetFolderContents(uploadFolderId);
            }
        }

        private void btnUploadFile_Click(object sender, EventArgs e)
        {
            OpenUploadForSelectedFolder();
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
            bool fileSelected   = contentsListView.SelectedItems.Count > 0
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
                bool fileSelected   = contentsListView.SelectedItems.Count > 0
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

        private IEnumerable<CMS.Domain.Models.OrgUnit> GetOrgUnits()
        {
            if (_orgStructureService == null) return Enumerable.Empty<CMS.Domain.Models.OrgUnit>();
            try { return _orgStructureService.GetHierarchy(); }
            catch { return Enumerable.Empty<CMS.Domain.Models.OrgUnit>(); }
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
                if (_fileService == null)
                {
                    ShowError("FileService ไม่พร้อมใช้งาน กรุณาติดต่อผู้ดูแลระบบ");
                    return;
                }
                
                using (var picker = new FolderPickerDialog())
                {
                    picker.LoadFolders(_folderService.GetAllFolders(null, _user), new HashSet<int>(), GetOrgUnits());
                    if (picker.ShowDialog(this) == DialogResult.OK && picker.SelectedFolderId.HasValue)
                    {
                        try
                        {
                            _fileService.MoveFiles(new MoveFileRequest
                            {
                                FileIds             = selectedFileIds,
                                DestinationFolderId = picker.SelectedFolderId.Value
                            }, _user);
                            MessageBox.Show(
                                string.Format("ย้าย {0} ไฟล์ สำเร็จ", selectedFileIds.Count), "สำเร็จ",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            contentsListView.SelectedItems.Clear();
                            var currentFolder = SelectedFolder;
                            if (currentFolder != null)
                                _presenter.GetFolderContents(currentFolder.FolderId);
                            else
                                _presenter.LoadFolderTree(_selectedOrgUnitId);
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
                var descendants = _folderService.GetDescendants(selectedFolderFromList.FolderID);
                var excluded    = new HashSet<int>(descendants.Select(d => d.FolderID));
                excluded.Add(selectedFolderFromList.FolderID);

                using (var picker = new FolderPickerDialog())
                {
                    picker.SourceFolderId = selectedFolderFromList.FolderID;
                    picker.LoadFolders(_folderService.GetAllFolders(null, _user), excluded, GetOrgUnits());
                    if (picker.ShowDialog(this) == DialogResult.OK && picker.SelectedFolderId.HasValue)
                    {
                        try
                        {
                            _folderService.MoveFolder(new MoveFolderRequest
                            {
                                SourceFolderId            = selectedFolderFromList.FolderID,
                                DestinationParentFolderId = picker.SelectedFolderId.Value
                            }, _user);
                            MessageBox.Show("ย้าย Folder สำเร็จ", "สำเร็จ",
                                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Reload tree ของ OrgUnit ปลายทาง (อาจต่างจาก OrgUnit ต้นทาง)
                            // ใช้ null เพื่อ reload ทั้งหมดให้ครอบคลุมทั้ง 2 OrgUnit
                            _presenter.LoadFolderTree(null);
                            _selectedOrgUnitId = null;
                            _selectedOrgUnitName = "— ทั้งหมด —";

                            // Reload content panel ของ parent folder ต้นทาง
                            var currentFolder = SelectedFolder;
                            if (currentFolder != null && currentFolder.FolderID != selectedFolderFromList.FolderID)
                                _presenter.GetFolderContents(currentFolder.FolderID);
                            else
                                _presenter.ShowOrgUnitRootFolders(null);
                        }
                        catch (UnauthorizedAccessException ex) { ShowError("ไม่มีสิทธิ์: " + ex.Message); }
                        catch (InvalidOperationException ex)   { ShowError(ex.Message); }
                        catch (Exception ex)                   { ShowUnexpectedError(ex); }
                    }
                }
                return;
            }

            // Case 3: ไม่มีอะไรเลือกใน list → ดู Folder ที่เลือกใน folderTreeView
            var selectedFolder = folderTreeView.SelectedNode != null
                ? folderTreeView.SelectedNode.Tag as Folder : null;

            if (selectedFolder != null)
            {
                var descendants = _folderService.GetDescendants(selectedFolder.FolderID);
                var excluded    = new HashSet<int>(descendants.Select(d => d.FolderID));
                excluded.Add(selectedFolder.FolderID);

                using (var picker = new FolderPickerDialog())
                {
                    picker.SourceFolderId = selectedFolder.FolderID;
                    picker.LoadFolders(_folderService.GetAllFolders(null, _user), excluded, GetOrgUnits());
                    if (picker.ShowDialog(this) == DialogResult.OK && picker.SelectedFolderId.HasValue)
                    {
                        try
                        {
                            _folderService.MoveFolder(new MoveFolderRequest
                            {
                                SourceFolderId            = selectedFolder.FolderID,
                                DestinationParentFolderId = picker.SelectedFolderId.Value
                            }, _user);
                            MessageBox.Show("ย้าย Folder สำเร็จ", "สำเร็จ",
                                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Reload ทั้งหมดเพื่อให้เห็น folder ในทั้ง OrgUnit ต้นทางและปลายทาง
                            _presenter.LoadFolderTree(null);
                            _selectedOrgUnitId = null;
                            _selectedOrgUnitName = "— ทั้งหมด —";
                            _presenter.ShowOrgUnitRootFolders(null);
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
                var fileTag   = selectedItem.Tag as FileDocument;
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
                    Font        = contentsListView.Font,
                };
                _inlineRenameBox.KeyDown   += InlineRenameBox_KeyDown;
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

            var fileTag   = _renamingItem.Tag as FileDocument;
            var folderTag = _renamingItem.Tag as Folder;
            _renamingItem = null;

            if (fileTag != null)
            {
                if (newName == fileTag.OriginalFileName) return;  // ไม่มีอะไรเปลี่ยน
                try
                {
                    _fileService.RenameFile(fileTag.FileID, newName, _user);
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
                var tag = contentsListView.SelectedItems[0].Tag;

                // ── กรณี File ──────────────────────────────────────────────────
                var file = tag as FileDocument;
                if (file != null)
                {
                    DeleteSelectedFile(file);
                    return;
                }

                // ── กรณี Folder ในรายการ (content list) ───────────────────────
                var folderInContent = tag as Folder;
                if (folderInContent != null)
                {
                    DeleteSelectedFolder(folderInContent);
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

        private void DeleteSelectedFile(FileDocument file)
        {
            var confirm = MessageBox.Show(
                string.Format("ลบไฟล์ '{0}' หรือไม่?\n\nไฟล์จะถูกย้ายไปยังถังขยะ", file.OriginalFileName),
                "ยืนยันการลบไฟล์",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                if (_fileService == null) { ShowError("File service is not available."); return; }

                _fileService.DeleteFile(file.FileDocumentId, _user);

                // Reload ก่อนเพื่อยืนยันว่าลบสำเร็จ
                var currentFolder = SelectedFolder;
                if (currentFolder != null)
                    _presenter.GetFolderContents(currentFolder.FolderId);

                // ตรวจสอบว่าไฟล์ไม่ปรากฏในรายการอีกต่อไป
                bool stillExists = false;
                foreach (ListViewItem item in contentsListView.Items)
                {
                    var f = item.Tag as FileDocument;
                    if (f != null && f.FileDocumentId == file.FileDocumentId)
                    {
                        stillExists = true;
                        break;
                    }
                }

                if (!stillExists)
                    ShowInfo(string.Format("ลบไฟล์ '{0}' เรียบร้อยแล้ว", file.OriginalFileName));
                else
                    ShowError(string.Format("ไม่สามารถยืนยันการลบไฟล์ '{0}'\nกรุณาตรวจสอบอีกครั้ง", file.OriginalFileName));
            }
            catch (UnauthorizedAccessException ex) { ShowError("ไม่มีสิทธิ์ลบไฟล์นี้: " + ex.Message); }
            catch (Exception ex)                   { ShowError("ไม่สามารถลบไฟล์ได้: " + ex.Message); }
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
                if (_approvalService == null)
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
                    _approvalService.RequestFolderDelete(folder.FolderId, reason, _user);
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
            if (_fileService == null || !_currentNavFolderId.HasValue) return;

            int rootFolderId = _currentNavFolderId.Value;

            // รวบรวม folder IDs ทั้งหมด (ตัวเองและ descendants)
            var allFolderIds = new List<int> { rootFolderId };
            try
            {
                var descendants = _folderService.GetDescendants(rootFolderId);
                if (descendants != null)
                    foreach (var d in descendants)
                        allFolderIds.Add(d.FolderID);
            }
            catch { /* ถ้า GetDescendants ไม่ได้ ใช้แค่ folder ปัจจุบัน */ }

            // นับไฟล์ทั้งหมดก่อนแสดง confirm
            int totalFiles = 0;
            var filesByFolder = new Dictionary<int, List<FileDocument>>();
            foreach (int fid in allFolderIds)
            {
                try
                {
                    var files = _fileService.GetFilesInFolder(fid, _user).ToList();
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
                        _fileService.DeleteFile(file.FileID, _user);
                        deleted++;
                    }
                    catch
                    {
                        failed++;
                    }
                }
            }

            // Reload content
            _presenter.GetFolderContents(rootFolderId);

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
            Color navyDark  = Color.FromArgb(25,  55,  105);
            Color navyLight = Color.FromArgb(215, 228, 248);
            Color bodyBg    = Color.FromArgb(243, 246, 251);
            Color footerBg  = Color.FromArgb(228, 234, 245);

            using (var dlg = new Form())
            {
                dlg.Text            = "ระบุเหตุผล";
                dlg.ClientSize      = new Size(420, 220);
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.BackColor       = bodyBg;
                dlg.Font            = new System.Drawing.Font("Segoe UI", 9f);

                // ── Header bar ────────────────────────────────────────────────
                var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = navyDark };
                var lblHeader = new Label
                {
                    Text      = "🗑  ระบุเหตุผล",
                    Font      = new System.Drawing.Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize  = true,
                    Location  = new Point(14, 14),
                };
                pnlHeader.Controls.Add(lblHeader);

                // ── Sub-header hint bar ───────────────────────────────────────
                var lblHint = new Label
                {
                    Text      = "  คำขออนุมัติจะถูกส่งให้ Admin พิจารณา",
                    Font      = new System.Drawing.Font("Segoe UI", 8.5f),
                    ForeColor = Color.FromArgb(50, 70, 110),
                    BackColor = navyLight,
                    Size      = new Size(420, 24),
                    Location  = new Point(0, 52),
                    TextAlign = ContentAlignment.MiddleLeft,
                };

                // ── Body ──────────────────────────────────────────────────────
                var lblPrompt = new Label
                {
                    Text      = prompt,
                    Font      = new System.Drawing.Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(30, 40, 65),
                    Size      = new Size(392, 36),
                    Location  = new Point(14, 88),
                    AutoSize  = false,
                };

                var txt = new TextBox
                {
                    Location    = new Point(14, 128),
                    Size        = new Size(392, 24),
                    Font        = new System.Drawing.Font("Segoe UI", 9.5f),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor   = Color.White,
                };

                // ── Footer bar ────────────────────────────────────────────────
                var pnlFooter = new Panel
                {
                    Dock      = DockStyle.Bottom,
                    Height    = 46,
                    BackColor = footerBg,
                };

                var btnOk = new Button
                {
                    Text         = "ตกลง",
                    Size         = new Size(88, 30),
                    Location     = new Point(228, 8),
                    FlatStyle    = FlatStyle.Flat,
                    BackColor    = navyDark,
                    ForeColor    = Color.White,
                    Font         = new System.Drawing.Font("Segoe UI", 9f, FontStyle.Bold),
                    DialogResult = DialogResult.OK,
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text         = "ยกเลิก",
                    Size         = new Size(88, 30),
                    Location     = new Point(322, 8),
                    FlatStyle    = FlatStyle.Flat,
                    BackColor    = Color.FromArgb(220, 228, 245),
                    ForeColor    = navyDark,
                    Font         = new System.Drawing.Font("Segoe UI", 9f),
                    DialogResult = DialogResult.Cancel,
                };
                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
                btnCancel.FlatAppearance.BorderSize  = 1;

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
            if (_folderTemplateRepository == null || _orgStructureService == null)
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

            using (var dlg = new ApplyTemplateDialog(_folderService, _folderTemplateRepository, _orgStructureService, _user, SelectedFolder))
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
                _folderService.ChangeConfidentialityLevel(folder.FolderId, newLevel.Value, _user);
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
                _folderService.CopyFolderStructure(sourceFolder.FolderId, destinationParentId.Value, _user);

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
                dlg.Text            = "เลือก Folder ปลายทาง";
                dlg.ClientSize      = new Size(400, 480);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;

                var lblInstruction = new Label
                {
                    Text    = "เลือก Folder ปลายทางที่จะวางโครงสร้างที่คัดลอก:",
                    Dock    = DockStyle.Top,
                    Height  = 30,
                    Padding = new Padding(4, 6, 4, 0),
                    Font    = new Font("Microsoft Sans Serif", 9f)
                };

                var tree = new TreeView
                {
                    Dock        = DockStyle.Fill,
                    Font        = new Font("Microsoft Sans Serif", 10f),
                    HotTracking = true
                };

                // Populate tree from current folderTreeView nodes, excluding source folder
                CloneTreeNodes(folderTreeView.Nodes, tree.Nodes, sourceFolderId);
                tree.ExpandAll();

                var pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 36 };
                var btnOk      = new Button { Text = "ตกลง",   DialogResult = DialogResult.OK,     Width = 80, Height = 26 };
                var btnCancel  = new Button { Text = "ยกเลิก", DialogResult = DialogResult.Cancel, Width = 80, Height = 26 };
                btnOk.Location     = new Point(236, 5);
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
            Color navyDark  = Color.FromArgb(25,  55,  105);
            Color navyLight = Color.FromArgb(215, 228, 248);
            Color bodyBg    = Color.FromArgb(243, 246, 251);
            Color footerBg  = Color.FromArgb(228, 234, 245);

            var levelColors = new[]
            {
                Color.FromArgb(204,   0,   0),  // 1HC — Red
                Color.FromArgb(220,  90,   0),  // 2SC — Orange
                Color.FromArgb(  0, 100, 200),  // 3NC — Blue
                Color.FromArgb(  0, 150,  50),  // 4PC — Green
            };

            using (var dlg = new Form())
            {
                dlg.Text            = "เปลี่ยนระดับความลับ";
                dlg.ClientSize      = new Size(400, 218);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.BackColor       = bodyBg;
                dlg.Font            = new System.Drawing.Font("Segoe UI", 9f);

                // ── Header ────────────────────────────────────────────────────
                var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = navyDark };
                var lblHeader = new Label
                {
                    Text      = "🔒  เปลี่ยนระดับความลับ",
                    Font      = new System.Drawing.Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize  = true,
                    Location  = new Point(14, 14),
                };
                pnlHeader.Controls.Add(lblHeader);

                // ── Hint bar ──────────────────────────────────────────────────
                var lblHint = new Label
                {
                    Text      = "  การเปลี่ยนระดับจะมีผลกับ Folder และไฟล์ภายใน",
                    Font      = new System.Drawing.Font("Segoe UI", 8.5f),
                    ForeColor = Color.FromArgb(50, 70, 110),
                    BackColor = navyLight,
                    Size      = new Size(400, 24),
                    Location  = new Point(0, 52),
                    TextAlign = ContentAlignment.MiddleLeft,
                };

                // ── Body ──────────────────────────────────────────────────────
                var lblLevel = new Label
                {
                    Text      = "ระดับใหม่:",
                    Font      = new System.Drawing.Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 40, 65),
                    Location  = new Point(14, 92),
                    AutoSize  = true,
                };

                var cboLevel = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    DrawMode      = DrawMode.OwnerDrawFixed,
                    ItemHeight    = 22,
                    Location      = new Point(14, 114),
                    Size          = new Size(372, 22),
                    Font          = new System.Drawing.Font("Segoe UI", 9f),
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
                    Dock      = DockStyle.Bottom,
                    Height    = 46,
                    BackColor = footerBg,
                };

                var btnOk = new Button
                {
                    Text         = "ตกลง",
                    Size         = new Size(88, 30),
                    Location     = new Point(210, 8),
                    FlatStyle    = FlatStyle.Flat,
                    BackColor    = navyDark,
                    ForeColor    = Color.White,
                    Font         = new System.Drawing.Font("Segoe UI", 9f, FontStyle.Bold),
                    DialogResult = DialogResult.OK,
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text         = "ยกเลิก",
                    Size         = new Size(88, 30),
                    Location     = new Point(304, 8),
                    FlatStyle    = FlatStyle.Flat,
                    BackColor    = Color.FromArgb(220, 228, 245),
                    ForeColor    = navyDark,
                    Font         = new System.Drawing.Font("Segoe UI", 9f),
                    DialogResult = DialogResult.Cancel,
                };
                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
                btnCancel.FlatAppearance.BorderSize  = 1;

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
            foreach (System.Windows.Forms.TreeNode node in folderTreeView.Nodes)
            {
                var folder = node.Tag as Folder;
                if (folder != null && IsAuto4Folder(folder))
                    return true;
            }
            return false;
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
                if (_folderService == null) return false;

                // ใช้ service เพื่อตรวจสอบความถูกต้อง
                var allFolders = _folderService.GetAllFolders(null, _user);
                
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
            try
            {
                // Debug: Show what we're trying to create
                string debugMsg = $"Creating Auto 4 Folders with baseName='{baseName}', parentFolderId={parentFolderId}, orgUnitId={orgUnitId}";
                System.Diagnostics.Debug.WriteLine(debugMsg);
                
                var folderConfigs = new[]
                {
                    new { Level = 1, Code = "1HC", Name = "High Confidential", Color = "#CC0000" },
                    new { Level = 2, Code = "2SC", Name = "Semi Confidential", Color = "#FF6600" },
                    new { Level = 3, Code = "3NC", Name = "Normal Confidential", Color = "#0066CC" },
                    new { Level = 4, Code = "4PC", Name = "Public Confidential", Color = "#009933" }
                };

                int createdCount = 0;
                foreach (var config in folderConfigs)
                {
                    try
                    {
                        string folderName = string.IsNullOrWhiteSpace(baseName)
                            ? string.Format("{0} - ({1})", config.Code, config.Name)
                            : string.Format("{0} - {1} [{0}]", config.Code, baseName);
                        
                        // ตรวจสอบว่ามี folder นี้อยู่แล้วหรือไม่
                        if (IsFolderExists(folderName, parentFolderId, orgUnitId))
                        {
                            System.Diagnostics.Debug.WriteLine($"Folder '{folderName}' already exists, skipping");
                            continue; // ข้าม folder ที่มีอยู่แล้ว (เงียบ)
                        }

                        if (parentFolderId.HasValue)
                        {
                            // สร้าง sub-folder
                            _presenter.CreateFolder(parentFolderId.Value, folderName, orgUnitId, config.Level, false);
                            System.Diagnostics.Debug.WriteLine($"Created sub-folder: '{folderName}'");
                        }
                        else
                        {
                            // สร้าง root folder
                            _presenter.CreateRootFolder(folderName, orgUnitId, config.Level, false);
                            System.Diagnostics.Debug.WriteLine($"Created root folder: '{folderName}'");
                        }
                        createdCount++;
                    }
                    catch (Exception ex)
                    {
                        // Debug: Show any errors
                        System.Diagnostics.Debug.WriteLine($"Error creating folder {config.Code}: {ex.Message}");
                        continue;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Auto 4 Folders creation completed. Created {createdCount} folders.");

                // Refresh folder tree เงียบๆ
                _presenter.LoadFolderTree(_selectedOrgUnitId);
            }
            catch (Exception ex)
            {
                // Debug: Show main error
                System.Diagnostics.Debug.WriteLine($"CreateAuto4FoldersQuiet main error: {ex.Message}");
                // ไม่แสดง error - ทำงานเงียบสนิท
            }
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
        private static TreeNode CreateFolderNode(Folder folder)
        {
            string badge = ConfidentialityBadge(folder.ConfidentialityLevel);
            string label = string.Format("{0} {1}{2}",
                folder.Name,
                badge,
                folder.IsArchived ? " [Archived]" : string.Empty);

            // 4 Folder หลัก (root) ใช้ book icon ตามระดับความลับ; folder ลูกใช้ folder icon ทั่วไป
            bool isRoot = !folder.ParentFolderID.HasValue || folder.ParentFolderID == 0;
            string imageKey = isRoot ? ConfidentialityBookIcon(folder.ConfidentialityLevel) : "folder";

            return new TreeNode(label)
            {
                Tag              = folder,
                ForeColor        = ConfidentialityColor(folder.ConfidentialityLevel),
                ImageKey         = imageKey,
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
                // If the file has a PhysicalPath property, use it directly
                if (!string.IsNullOrEmpty(file.PhysicalPath))
                {
                    return file.PhysicalPath;
                }
                
                // Otherwise, construct the path based on the file's properties
                // This assumes files are stored in a structured directory based on folder hierarchy
                // You may need to adjust this based on your actual file storage structure
                
                // Example path construction - adjust according to your storage scheme:
                // C:\FileStorage\Folder_{FolderId}\{StoredFileName}
                string baseStoragePath = @"\\150.1.8.23\AppForUser\Cmd_LmportFile"; // Network storage path
                
                string folderPath = string.Format("Folder_{0}", file.FolderId);
                string fileName = !string.IsNullOrEmpty(file.StoredFileName) ? file.StoredFileName : file.OriginalFileName;
                
                return System.IO.Path.Combine(baseStoragePath, folderPath, fileName);
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

        /// <summary>Gets permission text for a folder based on user's access rights.</summary>
        private string GetFolderPermissionText(Folder folder)
        {
            if (folder == null) return "—";
            
            bool isPrivileged = _user.AppRole >= 3; // Manager or Admin
            
            // Check authority-based permissions (FolderAuthorityConfig)
            bool canView = isPrivileged || _user.CanViewFolder(folder.ConfidentialityLevel);
            bool canAdd = isPrivileged || _user.CanAddToFolder(folder.ConfidentialityLevel);
            bool canEdit = isPrivileged || _user.CanEditFolder(folder.ConfidentialityLevel);
            
            // Check ACL permissions if permission service is available
            if (_permissionService != null && !isPrivileged)
            {
                try
                {
                    bool hasRead = _permissionService.CheckPermission(_user.UserID, folder.FolderId, "Read");
                    bool hasWrite = _permissionService.CheckPermission(_user.UserID, folder.FolderId, "Write");
                    bool hasModify = _permissionService.CheckPermission(_user.UserID, folder.FolderId, "Modify");
                    
                    // ACL overrides authority
                    if (hasRead || hasWrite || hasModify)
                    {
                        canView = hasRead || hasWrite || hasModify;
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
            il.Images.Add("folder",         LoadPng("folder"));
            
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
            il.Images.Add("book_red",       LoadPng("book-red"));
            il.Images.Add("book_orange",    LoadPng("book-orange"));
            il.Images.Add("book_bule",      LoadPng("book-bule"));
            il.Images.Add("book_green",     LoadPng("book-green"));

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
                            Alignment     = StringAlignment.Center,
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
            name                 = null;
            confidentialityLevel = lockedLevel ?? 1;

            Color navyDark  = Color.FromArgb(25,  55,  105);
            Color navyLight = Color.FromArgb(215, 228, 248);
            Color bodyBg    = Color.FromArgb(243, 246, 251);
            Color footerBg  = Color.FromArgb(228, 234, 245);

            var levelColors = new[]
            {
                Color.FromArgb(204,   0,   0),  // 1HC — Red
                Color.FromArgb(220,  90,   0),  // 2SC — Orange
                Color.FromArgb(  0, 100, 200),  // 3NC — Blue
                Color.FromArgb(  0, 150,  50),  // 4PC — Green
            };

            using (var form = new Form())
            {
                form.Text            = title;
                form.ClientSize      = new Size(400, 262);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition   = FormStartPosition.CenterParent;
                form.MaximizeBox     = false;
                form.MinimizeBox     = false;
                form.BackColor       = bodyBg;
                form.Font            = new System.Drawing.Font("Segoe UI", 9f);

                // ── Header ────────────────────────────────────────────────────
                var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = navyDark };
                var lblHeader = new Label
                {
                    Text      = "📁  สร้าง Folder ใหม่",
                    Font      = new System.Drawing.Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize  = true,
                    Location  = new Point(14, 14),
                };
                pnlHeader.Controls.Add(lblHeader);

                // ── Sub-header hint bar ───────────────────────────────────────
                var lblHint = new Label
                {
                    Text      = "  " + title,
                    Font      = new System.Drawing.Font("Segoe UI", 8.5f),
                    ForeColor = Color.FromArgb(50, 70, 110),
                    BackColor = navyLight,
                    Size      = new Size(400, 24),
                    Location  = new Point(0, 52),
                    TextAlign = ContentAlignment.MiddleLeft,
                };

                // ── Body ──────────────────────────────────────────────────────
                var lblName = new Label
                {
                    Text      = "ชื่อ Folder:",
                    Font      = new System.Drawing.Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 40, 65),
                    Location  = new Point(14, 90),
                    AutoSize  = true,
                };

                var input = new TextBox
                {
                    Location    = new Point(14, 110),
                    Size        = new Size(372, 24),
                    Font        = new System.Drawing.Font("Segoe UI", 9.5f),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor   = Color.White,
                };

                // ── Level label ───────────────────────────────────────────────
                var lblLv = new Label
                {
                    Text      = lockedLevel.HasValue
                                    ? "ระดับ: (กำหนดโดย Folder แม่ — ไม่สามารถเปลี่ยนได้)"
                                    : "ระดับความลับ:",
                    Font      = new System.Drawing.Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 40, 65),
                    Location  = new Point(14, 146),
                    AutoSize  = true,
                };

                // ── Color-coded combo ─────────────────────────────────────────
                var combo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    DrawMode      = DrawMode.OwnerDrawFixed,
                    ItemHeight    = 22,
                    Location      = new Point(14, 166),
                    Size          = new Size(372, 22),
                    Font          = new System.Drawing.Font("Segoe UI", 9f),
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
                    combo.Enabled   = false;
                    combo.BackColor = Color.FromArgb(235, 238, 245);
                }

                // ── Footer ────────────────────────────────────────────────────
                var pnlFooter = new Panel
                {
                    Dock      = DockStyle.Bottom,
                    Height    = 46,
                    BackColor = footerBg,
                };

                var btnOk = new Button
                {
                    Text         = "ตกลง",
                    Size         = new Size(88, 30),
                    Location     = new Point(210, 8),
                    FlatStyle    = FlatStyle.Flat,
                    BackColor    = navyDark,
                    ForeColor    = Color.White,
                    Font         = new System.Drawing.Font("Segoe UI", 9f, FontStyle.Bold),
                    DialogResult = DialogResult.OK,
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancel = new Button
                {
                    Text         = "ยกเลิก",
                    Size         = new Size(88, 30),
                    Location     = new Point(304, 8),
                    FlatStyle    = FlatStyle.Flat,
                    BackColor    = Color.FromArgb(220, 228, 245),
                    ForeColor    = navyDark,
                    Font         = new System.Drawing.Font("Segoe UI", 9f),
                    DialogResult = DialogResult.Cancel,
                };
                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
                btnCancel.FlatAppearance.BorderSize  = 1;

                pnlFooter.Controls.AddRange(new Control[] { btnOk, btnCancel });

                form.Controls.AddRange(new Control[] { pnlHeader, lblHint, lblName, input, lblLv, combo, pnlFooter });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                input.Select();
                if (form.ShowDialog(this) != DialogResult.OK) return false;

                string value = input.Text.Trim();
                if (string.IsNullOrEmpty(value)) return false;

                name                 = value;
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
                var previousFolder   = SelectedFolder;
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
            string extFilter      = cmbExtension.Text.Trim();
            bool   filterFrom     = dtpFrom.Checked;
            bool   filterTo       = dtpTo.Checked;
            DateTime fromDate     = filterFrom ? dtpFrom.Value.Date : DateTime.MinValue;
            DateTime toDate       = filterTo   ? dtpTo.Value.Date.AddDays(1).AddTicks(-1) : DateTime.MaxValue;
            string uploaderFilter = txtUploadedBy.Text.Trim();

            // Apply AND logic to _currentFiles
            var filtered = _currentFiles.AsEnumerable();

            if (!string.IsNullOrEmpty(extFilter))
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
            lblFilterStatus.Text    = string.Format("🔍 กรองแล้ว: {0} รายการ", filteredList.Count);
            lblFilterStatus.Visible = true;
        }

        /// <summary>
        /// Restore all FileDocument items for the selected folder.
        /// Sub-folder items are preserved (Req 6.5).
        /// Hides lblFilterStatus (Req 6.3).
        /// </summary>
        internal void ClearFileFilter()
        {
            // Clear filter controls
            cmbExtension.Text   = string.Empty;
            dtpFrom.Checked     = false;
            dtpTo.Checked       = false;
            txtUploadedBy.Text  = string.Empty;

            RefreshFileItemsInListView(_currentFiles);

            lblFilterStatus.Text    = string.Empty;
            lblFilterStatus.Visible = false;
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
            if (_orgStructureService != null)
            {
                try
                {
                    foreach (var ou in _orgStructureService.GetHierarchy())
                        orgUnitNames[ou.OrgUnitID] = ou.OrgUnitNameTH ?? ou.OrgUnitNameEN ?? ou.OrgUnitCode;
                }
                catch { }
            }

            // Add filtered file items
            foreach (var file in filesToShow)
            {
                string icon = FileIcon(file.FileExtension);
                var item    = new ListViewItem(file.OriginalFileName) { ImageKey = icon };
                item.SubItems.Add(file.FileExtension.TrimStart('.').ToUpper());
                item.SubItems.Add(ConfidentialityShort(file.ConfidentialityLevel));
                item.SubItems.Add(FormatBytes(file.FileSizeBytes));
                item.SubItems.Add(file.UploadedAt.ToLocalTime().ToString("dd/MM/yy HH:mm"));
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
                    Tag       = original.Tag,
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
                Text      = "⭐ Quick Access",
                Dock      = DockStyle.Top,
                Height    = 22,
                Font      = new Font("Microsoft Sans Serif", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 56, 100),
                Padding   = new Padding(4, 3, 0, 0)
            };

            // ListBox แสดงรายการ Quick Access
            _lstQuickAccess = new ListBox
            {
                Dock           = DockStyle.Fill,
                Font           = new Font("Microsoft Sans Serif", 10f),
                BorderStyle    = BorderStyle.None,
                IntegralHeight = false
            };
            _lstQuickAccess.Click        += LstQuickAccess_Click;
            _lstQuickAccess.MouseDown    += LstQuickAccess_MouseDown;

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
                Dock        = DockStyle.Bottom,
                Height      = 140,
                BackColor   = Color.FromArgb(245, 248, 255),
                Padding     = new Padding(0)
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
            if (_quickAccessService == null || _lstQuickAccess == null) return;

            try
            {
                _quickAccessItems = _quickAccessService.GetByUser(_user.UserID).ToList();
                _lstQuickAccess.Items.Clear();

                // สร้าง lookup สำหรับ OrgUnit names
                Dictionary<int, string> orgUnitNames = new Dictionary<int, string>();
                if (_orgStructureService != null)
                {
                    try
                    {
                        foreach (var ou in _orgStructureService.GetHierarchy())
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
            if (_quickAccessService == null) return;

            var folder = SelectedFolder;
            if (folder == null) return;

            try
            {
                _quickAccessService.Add(_user.UserID, folder.FolderID);
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
            if (_quickAccessService == null) return;

            int idx = _lstQuickAccess.SelectedIndex;
            if (idx < 0 || idx >= _quickAccessItems.Count) return;

            var qa = _quickAccessItems[idx];
            try
            {
                _quickAccessService.Remove(_user.UserID, qa.FolderID);
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
            lblConfidentialityBadge.Text      = ConfidentialityLabel(folder.ConfidentialityLevel);
            lblConfidentialityBadge.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
            lblContentInfo.Text   = string.Format("กำลังดู: {0}", folder.FolderName);
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
                    : qa.Folder.IsDeleted   ? "Folder นี้ถูกลบแล้ว"
                    : "Folder นี้ถูก Archive แล้ว";
                ShowInfo(string.Format("⚠️ {0} ไม่สามารถนำทางไปได้", status));
                return;
            }

            if (_selectedOrgUnitId != qa.Folder.OrgUnitID)
            {
                _selectedOrgUnitId = qa.Folder.OrgUnitID;
                SelectOrgUnitNode(qa.Folder.OrgUnitID);
                if (_orgStructureService != null)
                {
                    try
                    {
                        var orgUnit = _orgStructureService.GetHierarchy()
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
            splitMain.Panel2MinSize    = 0;
            splitMain.Panel2Collapsed  = true;
            splitMain.IsSplitterFixed  = true;
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
            private readonly int  _column;
            private readonly bool _ascending;

            public ListViewItemComparer(int column, bool ascending)
            {
                _column    = column;
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
                if (!xIsFolder && yIsFolder) return  1;

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
