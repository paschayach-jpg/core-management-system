using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly UserContext          _user;

        // Quick Access panel (Req 7)
        private Panel    _pnlQuickAccess;
        private ListBox  _lstQuickAccess;
        private List<QuickAccessFolder> _quickAccessItems = new List<QuickAccessFolder>();

        // Currently selected OrgUnit filter (null = all)
        private int? _selectedOrgUnitId;
        private string _selectedOrgUnitName = "— ทั้งหมด —";

        // Cache last selected folder so MDI toolbar can access it even after focus change
        private Folder _lastSelectedFolder;

        // Track selected file in content list
        private List<FileDocument> _currentFiles = new List<FileDocument>();

        // File preview panel (Req 9)
        private FilePreviewPanel _previewPanel;
        private Button _btnTogglePreview;

        // Inline rename (เหมือน Windows Explorer)
        private TextBox      _inlineRenameBox;
        private ListViewItem _renamingItem;

        // Track active confidentiality level filter (0 = no filter, 1-4 = filtered level)
        private int _activeConfidentialityFilter = 0;

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        public FolderForm(IFolderService service, UserContext user, int? orgUnitId = null,
                          IFileService fileService = null, IOrgStructureService orgStructureService = null,
                          IFolderTemplateRepository folderTemplateRepository = null,
                          IUploadCheckpointService checkpointService = null,
                          IQuickAccessService quickAccessService = null,
                          IApprovalService approvalService = null)
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
            _user                     = user;

            InitializeComponent();

            _presenter = new FolderPresenter(service, this, user, fileService);

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
            
            // Wire double-click to open files
            contentsListView.DoubleClick += ContentsListView_DoubleClick;

            // F2 = inline rename (เหมือน Windows Explorer)
            contentsListView.KeyDown += (s, e2) =>
            {
                if (e2.KeyCode == Keys.F2)
                    btnRename_Click(s, EventArgs.Empty);
            };

            // Red border on breadcrumb textbox via parent panel Paint
            pnlNavBar.Paint += (s, e) =>
            {
                var r = new Rectangle(
                    txtBreadcrumb.Left - 1, txtBreadcrumb.Top - 1,
                    txtBreadcrumb.Width + 1, txtBreadcrumb.Height + 1);
                using (var pen = new Pen(Color.Red, 1))
                    e.Graphics.DrawRectangle(pen, r);
            };

            // Alternating row colors for readability
            contentsListView.OwnerDraw = true;
            contentsListView.DrawColumnHeader += (s, e) => e.DrawDefault = true;
            contentsListView.DrawSubItem      += ContentsListView_DrawSubItem;
            contentsListView.DrawItem         += (s, e) => { /* handled in DrawSubItem */ };

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
            contentsListView.SelectedIndexChanged += (s, e2) => { UpdateMoveButtonState(); OnSelectionChanged(); };

            // Initialize file preview panel (Req 9)
            InitializePreviewPanel();

            // Initialize Quick Access panel (Req 7)
            InitializeQuickAccessPanel();

            // Hide recycle bin components
            HideRecycleBinComponents();

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

        // -----------------------------------------------------------------------
        // Sort state (Req 4.1–4.5)
        // -----------------------------------------------------------------------

        private int  _sortColumn    = -1;   // -1 = no active sort
        private bool _sortAscending = true;

        // -----------------------------------------------------------------------
        // File Preview Panel initialisation (Req 9)
        // -----------------------------------------------------------------------

        private void InitializePreviewPanel()
        {
            // Preview panel — create as a separate form for popup display
            _previewPanel = new FilePreviewPanel
            {
                Width   = 400,
                Height  = 500,
                Visible = false
            };

            // Toggle button — added to pnlContentHeader with icon only
            _btnTogglePreview = new Button
            {
                Text      = "👁",
                FlatStyle = FlatStyle.Flat,
                Font      = new System.Drawing.Font("Microsoft Sans Serif", 12f),
                Size      = new System.Drawing.Size(30, 22),
                Dock      = DockStyle.Right,
                Cursor    = Cursors.Hand
            };
            _btnTogglePreview.FlatAppearance.BorderSize = 0;
            _btnTogglePreview.Click += BtnTogglePreview_Click;

            // Add tooltip
            var tooltip = new ToolTip();
            tooltip.SetToolTip(_btnTogglePreview, "แสดง Preview");

            pnlContentHeader.Controls.Add(_btnTogglePreview);
        }

        private void BtnTogglePreview_Click(object sender, EventArgs e)
        {
            var file = SelectedFile;
            if (file == null)
            {
                MessageBox.Show("กรุณาเลือกไฟล์ก่อนดู Preview", "แจ้งเตือน", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Create popup form for preview
            var previewForm = new Form
            {
                Text = $"Preview - {file.OriginalFileName}",
                Size = new Size(500, 600),
                StartPosition = FormStartPosition.CenterParent,
                ShowInTaskbar = false,
                MaximizeBox = true,
                MinimizeBox = false
            };

            var previewPanel = new FilePreviewPanel
            {
                Dock = DockStyle.Fill
            };

            previewForm.Controls.Add(previewPanel);
            
            string path = GetFilePhysicalPath(file);
            previewPanel.ShowFile(file, path);
            
            previewForm.ShowDialog(this);
        }

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
            ctx.Items.Add(mnuDownload);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuCut);
            ctx.Items.Add(mnuCopy);
            ctx.Items.Add(mnuPaste);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuRename);
            ctx.Items.Add(mnuDelete);
            ctx.Items.Add(mnuArchive);
            ctx.Items.Add(mnuPermission);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuAddQuickAccess);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuProperties);

            // enable/disable ตาม selection
            ctx.Opening += (s, e) =>
            {
                bool hasFolder        = (SelectedFolder != null);
                mnuOpen.Enabled       = hasFolder;
                mnuDownload.Enabled   = hasFolder;
                mnuRename.Enabled     = hasFolder;
                mnuDelete.Enabled     = hasFolder;
                mnuArchive.Enabled    = hasFolder;
                mnuPermission.Enabled = hasFolder;
                mnuProperties.Enabled = hasFolder;
                mnuAddQuickAccess.Enabled = hasFolder && _quickAccessService != null;
                // mnuReload และ mnuNewFolder ใช้ได้เสมอ
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

            var mnuOpen       = new ToolStripMenuItem("เปิดดู",              null, CtxFileOpen_Click);
            var mnuDownload   = new ToolStripMenuItem("ดาวน์โหลด",           null, CtxFileDownload_Click);
            var mnuCut        = new ToolStripMenuItem("ตัด",                 null, (s, e) => ShowInfo("ฟีเจอร์ตัด/ย้ายไฟล์จะพร้อมใช้งานในเวอร์ชันถัดไป")) { ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X, ShowShortcutKeys = true, Enabled = false };
            var mnuCopy       = new ToolStripMenuItem("คัดลอก",              null, (s, e) => ShowInfo("ฟีเจอร์คัดลอกไฟล์จะพร้อมใช้งานในเวอร์ชันถัดไป")) { ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C, ShowShortcutKeys = true, Enabled = false };
            var mnuPaste      = new ToolStripMenuItem("วาง",                 null, (s, e) => ShowInfo("ฟีเจอร์วางไฟล์จะพร้อมใช้งานในเวอร์ชันถัดไป"))   { ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V, ShowShortcutKeys = true, Enabled = false };
            var mnuRename     = new ToolStripMenuItem("เปลี่ยนชื่อ",         null, (s, e) => btnRename_Click(s, e))                                      { ShortcutKeys = System.Windows.Forms.Keys.F2, ShowShortcutKeys = true };
            var mnuDelete     = new ToolStripMenuItem("ลบ",                  null, (s, e) => btnDelete_Click(s, e))                                       { ShortcutKeys = System.Windows.Forms.Keys.Delete, ShowShortcutKeys = true };
            var mnuArchive    = new ToolStripMenuItem("Archive",             null, (s, e) => btnArchive_Click(s, e));
            var mnuPermission = new ToolStripMenuItem("ตั้งค่าสิทธิ์...",   null, CtxPermission_Click);
            var mnuProperties = new ToolStripMenuItem("คุณสมบัติ",           null, CtxFileProperties_Click)                                               { ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Enter, ShowShortcutKeys = true };

            ctx.Items.Add(mnuOpen);
            ctx.Items.Add(mnuDownload);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuCut);
            ctx.Items.Add(mnuCopy);
            ctx.Items.Add(mnuPaste);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuRename);
            ctx.Items.Add(mnuDelete);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuArchive);
            ctx.Items.Add(mnuPermission);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuProperties);

            // enable/disable based on selection
            ctx.Opening += (s, e) =>
            {
                bool hasFile          = (SelectedFile != null);
                mnuOpen.Enabled       = hasFile;
                mnuDownload.Enabled   = hasFile;
                mnuRename.Enabled     = hasFile;
                mnuDelete.Enabled     = hasFile;
                mnuArchive.Enabled    = hasFile;
                mnuPermission.Enabled = hasFile;
                mnuProperties.Enabled = hasFile;
            };

            contentsListView.ContextMenuStrip = ctx;

            // Select item on right-click before showing menu
            contentsListView.MouseDown += (s, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    var hit = contentsListView.HitTest(e.Location);
                    if (hit.Item != null)
                    {
                        hit.Item.Selected = true;
                        hit.Item.Focused  = true;
                    }
                }
            };
        }

        private void CtxFileOpen_Click(object sender, EventArgs e)
        {
            var file = SelectedFile;
            if (file == null) return;
            
            try
            {
                // Get the file path from the file service
                if (_fileService != null)
                {
                    // Assuming the file service has a method to get the physical file path
                    // or we can construct it from the file's stored path
                    string filePath = GetFilePhysicalPath(file);
                    
                    if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
                    {
                        // Open the file with the default application
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        ShowError(string.Format("ไม่พบไฟล์: {0}", file.OriginalFileName));
                    }
                }
                else
                {
                    ShowError("File service is not available.");
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
        // OrgUnit inline TreeView
        // -----------------------------------------------------------------------

        private void LoadOrgUnitTree()
        {
            if (_orgStructureService == null) return;

            try
            {
                var units = _orgStructureService.GetHierarchy()
                    .Where(u => u.IsActive && !u.IsDeleted)
                    .ToList();

                treeOrgUnit.BeginUpdate();
                treeOrgUnit.Nodes.Clear();

                // "ทั้งหมด" root node (null tag = no filter)
                var allNode = new System.Windows.Forms.TreeNode("— ทั้งหมด —")
                {
                    Tag      = null,
                    NodeFont = new System.Drawing.Font(treeOrgUnit.Font, System.Drawing.FontStyle.Bold),
                    ForeColor = System.Drawing.Color.FromArgb(31, 56, 100)
                };
                treeOrgUnit.Nodes.Add(allNode);

                foreach (var sc in units.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.SubCommittee).OrderBy(u => u.Name))
                {
                    var scNode = new System.Windows.Forms.TreeNode("🏢 " + sc.Name) { Tag = sc };
                    scNode.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);

                    foreach (var div in units.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.Division && u.ParentId == sc.OrgUnitId).OrderBy(u => u.Name))
                    {
                        var divNode = new System.Windows.Forms.TreeNode("📁 " + div.Name) { Tag = div };
                        divNode.ForeColor = System.Drawing.Color.FromArgb(0, 80, 160);

                        foreach (var dept in units.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.Department && u.ParentId == div.OrgUnitId).OrderBy(u => u.Name))
                        {
                            var deptNode = new System.Windows.Forms.TreeNode("📋 " + dept.Name) { Tag = dept };
                            deptNode.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
                            divNode.Nodes.Add(deptNode);
                        }
                        scNode.Nodes.Add(divNode);
                    }
                    treeOrgUnit.Nodes.Add(scNode);
                }

                treeOrgUnit.ExpandAll();
                treeOrgUnit.EndUpdate();

                // ไม่ auto-select อะไร ให้ user เลือกเอง
                // treeOrgUnit.SelectedNode = allNode;
            }
            catch (Exception ex)
            {
                ShowError("โหลดหน่วยงานไม่สำเร็จ: " + ex.Message);
            }
        }

        private void treeOrgUnit_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            var unit = e.Node != null ? e.Node.Tag as OrgUnit : null;
            _selectedOrgUnitId   = unit != null ? (int?)unit.OrgUnitId : null;
            _selectedOrgUnitName = unit != null ? unit.Name : "— ทั้งหมด —";
            lblOrgUnitStatus.Text = string.Format("📂 กำลังดู: {0}", _selectedOrgUnitName);

            // Push navigation (OrgUnit root = null entry)
            PushNavigation(null);
            UpdateNavButtons();

            _presenter.LoadFolderTree(_selectedOrgUnitId);
            // แสดง root folders ของ OrgUnit ที่เลือกใน content area ทางขวา
            _presenter.ShowOrgUnitRootFolders(_selectedOrgUnitId);

            // Update breadcrumb to show OrgUnit path
            ApplyBreadcrumb(BuildBreadcrumbPath(e.Node, null));
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
                _presenter.LoadFolderTree(_selectedOrgUnitId);
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
            // Filter out deleted folders
            _treeRoots = rootFolders == null ? new List<Folder>() : rootFolders.Where(f => !f.IsDeleted).ToList();

            // จำ folderId ที่เลือกอยู่ก่อน rebuild tree
            var previousFolder = SelectedFolder;
            int? previousFolderId = previousFolder != null ? (int?)previousFolder.FolderId : null;

            folderTreeView.BeginUpdate();
            folderTreeView.Nodes.Clear();

            foreach (var folder in _treeRoots)
            {
                var node = CreateFolderNode(folder);
                folderTreeView.Nodes.Add(node);
                LoadChildFolders(node, folder.FolderId);
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

            folderTreeView.ExpandAll();
            folderTreeView.EndUpdate();

            // ถ้ามี folder ที่เลือกอยู่ก่อน → restore selection เดิม
            // ถ้าไม่มี (เปิด form ครั้งแรก) → ไม่ auto-select อะไร ให้ user เลือกเอง
            if (_treeRoots.Count == 0)
            {
                // ไม่มี folder เลย → clear ด้านขวาทันที
                ClearContentPanel();
            }
            else if (previousFolderId.HasValue)
            {
                RestoreSelectedNode(folderTreeView.Nodes, previousFolderId.Value);
            }
            else
            {
                // เปิดครั้งแรก → ไม่เลือกโฟลเดอร์ใดๆ และ clear content panel
                // Disable events ชั่วคราวเพื่อป้องกัน auto-trigger
                folderTreeView.AfterSelect -= folderTreeView_AfterSelect;
                folderTreeView.SelectedNode = null;
                folderTreeView.AfterSelect += folderTreeView_AfterSelect;
                ClearContentPanel();
            }
        }

        /// <summary>
        /// Recursively loads child folders for the given parent node.
        /// </summary>
        private void LoadChildFolders(TreeNode parentNode, int parentFolderId)
        {
            try
            {
                if (_presenter == null) return;

                // Get child folders from service
                var children = _presenter.GetFolderChildren(parentFolderId);
                if (children == null) return;

                // Filter out deleted folders
                var activeChildren = children.Where(f => !f.IsDeleted);

                foreach (var childFolder in activeChildren)
                {
                    var childNode = CreateFolderNode(childFolder);
                    parentNode.Nodes.Add(childNode);
                    // Recursively load grandchildren
                    LoadChildFolders(childNode, childFolder.FolderId);
                }
            }
            catch
            {
                // Silently skip if unable to load children
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

            // Build OrgUnit lookup for display names
            Dictionary<int, string> orgUnitNames = new Dictionary<int, string>();
            if (_orgStructureService != null)
            {
                try
                {
                    foreach (var ou in _orgStructureService.GetHierarchy())
                        orgUnitNames[ou.OrgUnitID] = ou.OrgUnitNameTH ?? ou.OrgUnitNameEN ?? ou.OrgUnitCode;
                }
                catch { /* ถ้าโหลดไม่ได้ก็แสดง — แทน */ }
            }

            // ── Sub-folders first ──────────────────────────────────────────
            foreach (var folder in folderList)
            {
                // Populate OrgUnitName
                string ouName;
                folder.OrgUnitName = orgUnitNames.TryGetValue(folder.OrgUnitID, out ouName) ? ouName : "—";

                // Name column: icon + name + subtitle hint
                string nameText = string.Format("📁  {0}", folder.Name);
                var item = new ListViewItem(nameText);
                item.SubItems.Add(folder.FileCount > 0
                    ? string.Format("Folder  {0} ไฟล์", folder.FileCount)
                    : "Folder");                                          // ประเภท
                item.SubItems.Add(ConfidentialityShort(folder.ConfidentialityLevel)); // ระดับ
                item.SubItems.Add(FormatBytes(folder.FolderSizeBytes));              // ขนาด
                item.SubItems.Add(folder.CreatedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")); // วันที่
                item.SubItems.Add("—");                                              // โดย
                item.SubItems.Add("—");                                              // Ver
                item.SubItems.Add(folder.OrgUnitName ?? "—");                        // หน่วยงาน
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
                string nameText = string.Format("{0}  {1}", icon, file.OriginalFileName);
                var item = new ListViewItem(nameText);
                item.SubItems.Add(file.FileExtension.TrimStart('.').ToUpper()); // ประเภท
                item.SubItems.Add(ConfidentialityShort(file.ConfidentialityLevel)); // ระดับ
                item.SubItems.Add(FormatBytes(file.FileSizeBytes));              // ขนาด
                item.SubItems.Add(file.UploadedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")); // วันที่
                item.SubItems.Add(file.UploadedByName ?? "—");                  // โดย
                item.SubItems.Add(file.Version.ToString());                      // Ver
                item.SubItems.Add(file.OrgUnitName ?? "—");                      // หน่วยงาน
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
        // Content list selection — update detail panel
        // -----------------------------------------------------------------------

        private void ContentsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (contentsListView.SelectedItems.Count == 0)
            {
                // Hide detail panel when no selection
                splitContent.Panel2Collapsed = true;
                
                lblDetailHeader.Text  = "📋 ตัวอย่าง";
                lblDetailContent.Text = "เลือกรายการเพื่อดูรายละเอียด";
                lblContentInfo.Text   = lblContentInfo.Text.Contains("|")
                    ? lblContentInfo.Text.Substring(0, lblContentInfo.Text.LastIndexOf('|') + 1)
                      + " ไม่มีรายการที่เลือก"
                    : lblContentInfo.Text;

                return;
            }

            // Show detail panel when something is selected
            splitContent.Panel2Collapsed = false;

            var tag = contentsListView.SelectedItems[0].Tag;
            var file = tag as FileDocument;
            var folder = tag as Folder;

            if (file != null)
            {
                // Create modern detail panel for file
                CreateModernDetailPanel(file);
            }
            else if (folder != null)
            {
                // Create modern detail panel for folder  
                CreateModernDetailPanel(folder);
            }
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
            headerPanel.Controls.Add(new Label
            {
                Text = folder.FolderName, Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.White, Location = new Point(32, 4), Size = new Size(pw - 36, 34),
                TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
            });

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

        private void ContentsListView_DoubleClick(object sender, EventArgs e)
        {
            var file = SelectedFile;
            if (file != null)
            {
                // Double-click on file opens it
                CtxFileOpen_Click(sender, e);
            }
            else
            {
                // Double-click on folder navigates into it
                var folder = contentsListView.SelectedItems.Count > 0
                    ? contentsListView.SelectedItems[0].Tag as Folder : null;
                if (folder != null)
                {
                    // พยายาม select node ใน folderTreeView ให้ sync
                    // (ถ้าเจอ node จะ trigger folderTreeView_AfterSelect → GetFolderContents อัตโนมัติ)
                    bool found = RestoreSelectedNode(folderTreeView.Nodes, folder.FolderId);
                    if (!found)
                    {
                        // ไม่เจอ node ใน tree → โหลด content โดยตรง + push navigation เอง
                        // สร้าง breadcrumb โดยต่อชื่อ folder ต่อจาก path ปัจจุบัน (เหมือน Windows Explorer)
                        string newBreadcrumb = string.IsNullOrEmpty(_currentBreadcrumb)
                            ? "🏠 PC  ›  " + folder.Name
                            : _currentBreadcrumb + "  ›  " + folder.Name;
                        PushNavigation(folder.FolderId);
                        _presenter.GetFolderContents(folder.FolderId);
                        lblContentInfo.Text = string.Format("{0} รายการ | {1} | {2}",
                            folder.FileCount,
                            ConfidentialityLabel(folder.ConfidentialityLevel),
                            folder.Name);
                        ApplyBreadcrumb(newBreadcrumb);
                        UpdateNavButtons();
                    }
                    // ถ้า found → folderTreeView_AfterSelect จะ push navigation + ApplyBreadcrumb เอง
                }
            }
        }

        private void ContentsListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            // Alternating row background
            bool isSelected = e.Item.Selected;
            bool isEven     = e.ItemIndex % 2 == 0;

            Color backColor = isSelected
                ? Color.FromArgb(204, 228, 247)                          // selected: light blue
                : isEven ? Color.White : Color.FromArgb(248, 249, 252);  // alternating white / very light gray

            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            // Determine text color: use item's sub-item color if set, else item color
            Color textColor = isSelected ? Color.Black : e.SubItem.ForeColor;
            if (textColor == Color.Empty || textColor == SystemColors.WindowText)
                textColor = isSelected ? Color.Black : e.Item.ForeColor;
            if (textColor == Color.Empty) textColor = Color.Black;

            var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                        TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;
            var textBounds = Rectangle.Inflate(e.Bounds, -2, 0);
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.ListView.Font,
                                  textBounds, textColor, flags);
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
                default: return string.Empty;
            }
        }

        // -----------------------------------------------------------------------
        // TreeView — node selection loads contents
        // -----------------------------------------------------------------------

        private void ClearContentPanel()
        {
            contentsListView.Items.Clear();
            _currentFiles.Clear();
            txtBreadcrumb.Text            = "";
            lblContentInfo.Text           = "";
            lblConfidentialityBadge.Text  = "";
            lblFilterStatus.Text          = string.Empty;
            lblFilterStatus.Visible       = false;
            cmbExtension.Items.Clear();
            
            // Hide detail panel when clearing content
            splitContent.Panel2Collapsed = true;
        }

        private void folderTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // ถ้าไม่มี node ที่เลือก ให้ clear content panel
            if (e.Node == null)
            {
                ClearContentPanel();
                return;
            }
            
            var folder = e.Node.Tag as Folder;
            if (folder == null) 
            { 
                ClearContentPanel(); 
                return; 
            }  // placeholder node
            
            _lastSelectedFolder = folder;   // cache for MDI toolbar access

            // Push navigation history (skip ถ้ากำลัง navigate via back/forward)
            PushNavigation(folder.FolderId);
            UpdateNavButtons();

            // เรียก GetFolderContents เฉพาะเมื่อมีการเลือกโฟลเดอร์จริงๆ
            _presenter.GetFolderContents(folder.FolderId);

            // Update confidentiality badge in toolbar (Req 7.6)
            lblConfidentialityBadge.Text      = ConfidentialityLabel(folder.ConfidentialityLevel);
            lblConfidentialityBadge.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);

            // Update breadcrumb and content header (full path = OrgUnit › ... › Folder)
            ApplyBreadcrumb(BuildBreadcrumbPath(treeOrgUnit.SelectedNode, e.Node));
            lblContentInfo.Text = string.Format("{0} รายการ | {1} | {2}",
                folder.FileCount,
                ConfidentialityLabel(folder.ConfidentialityLevel),
                folder.Name);
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
            _forwardStack.Push(_currentNavFolderId);
            _forwardBreadcrumbs.Push(_currentBreadcrumb);
            _currentNavFolderId = _backStack.Pop();
            _currentBreadcrumb  = _backBreadcrumbs.Count > 0 ? _backBreadcrumbs.Pop() : "";
            _isNavigating = true;
            try   { ExecuteNavigation(_currentNavFolderId, _currentBreadcrumb); }
            finally { _isNavigating = false; UpdateNavButtons(); }
        }

        private void BtnNavForward_Click(object sender, EventArgs e)
        {
            if (_forwardStack.Count == 0) return;
            _backStack.Push(_currentNavFolderId);
            _backBreadcrumbs.Push(_currentBreadcrumb);
            _currentNavFolderId = _forwardStack.Pop();
            _currentBreadcrumb  = _forwardBreadcrumbs.Count > 0 ? _forwardBreadcrumbs.Pop() : "";
            _isNavigating = true;
            try   { ExecuteNavigation(_currentNavFolderId, _currentBreadcrumb); }
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
                // ถ้าเจอ node ใน tree → select มัน (AfterSelect จะ update breadcrumb)
                bool found = RestoreSelectedNode(folderTreeView.Nodes, folderId.Value);
                if (!found)
                {
                    _presenter.GetFolderContents(folderId.Value);
                    // AfterSelect ไม่ถูกเรียก → update breadcrumb เอง
                    ApplyBreadcrumb(breadcrumbOverride ?? _currentBreadcrumb);
                }
                // found → folderTreeView_AfterSelect จะ update breadcrumb ผ่าน ApplyBreadcrumb
            }
            else
            {
                // Navigate กลับไป OrgUnit root — deselect folder tree
                folderTreeView.AfterSelect -= folderTreeView_AfterSelect;
                folderTreeView.SelectedNode = null;
                folderTreeView.AfterSelect += folderTreeView_AfterSelect;
                _presenter.ShowOrgUnitRootFolders(_selectedOrgUnitId);
                ApplyBreadcrumb(breadcrumbOverride ?? BuildBreadcrumbPath(treeOrgUnit.SelectedNode, null));
            }
        }

        /// <summary>ตั้ง breadcrumb text แล้ว scroll ให้ส่วนขวา (ชื่อปัจจุบัน) มองเห็น เหมือน Windows Explorer</summary>
        private void ApplyBreadcrumb(string path)
        {
            _currentBreadcrumb           = path ?? "";
            txtBreadcrumb.Text           = _currentBreadcrumb;
            txtBreadcrumb.SelectionStart = txtBreadcrumb.Text.Length;
            txtBreadcrumb.ScrollToCaret();
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
            _backStack.Push(_currentNavFolderId);
            _backBreadcrumbs.Push(_currentBreadcrumb);
            _forwardStack.Clear();
            _forwardBreadcrumbs.Clear();
            _currentNavFolderId = newFolderId;
        }

        private void UpdateNavButtons()
        {
            btnNavBack.Enabled    = _backStack.Count > 0;
            btnNavForward.Enabled = _forwardStack.Count > 0;
            btnNavUp.Enabled      = _currentNavFolderId.HasValue;
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
            var parent = SelectedFolder;
            if (parent == null)
            {
                // ไม่มี parent — ถ้ามี OrgUnit ที่เลือกอยู่ ให้สร้าง root folder
                if (_selectedOrgUnitId.HasValue)
                {
                    // เพิ่มตัวเลือกสำหรับสร้าง Auto 4 Folders
                    var choice = MessageBox.Show(
                        "เลือกประเภทการสร้าง Folder:\n\n" +
                        "[Yes] สร้าง Folder เดี่ยว\n" +
                        "[No] สร้าง Auto 4 Folders (1HC, 2SC, 3NC, 4PC)\n" +
                        "[Cancel] ยกเลิก",
                        "สร้าง Root Folder",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (choice == DialogResult.Cancel) return;

                    if (choice == DialogResult.Yes)
                    {
                        // สร้าง folder เดี่ยว
                        string rootName;
                        int rootLevel;
                        if (!PromptForNameAndLevel("สร้าง Root Folder", out rootName, out rootLevel)) return;
                        _presenter.CreateRootFolder(rootName, _selectedOrgUnitId.Value, rootLevel);
                    }
                    else
                    {
                        // สร้าง Auto 4 Folders
                        string baseName = PromptForBaseName("สร้าง Auto 4 Folders");
                        if (baseName == null) return;
                        CreateAuto4Folders(baseName, null, _selectedOrgUnitId.Value);
                    }
                }
                return;
            }

            // มี folder ถูกเลือกอยู่ — ถามว่าจะสร้างที่ไหนและประเภทใด
            var mainChoice = MessageBox.Show(
                string.Format("ต้องการสร้าง Folder ใหม่ที่ไหน?\n\n[Yes]  ใต้ \"{0}\" (Sub-folder)\n[No]   ระดับ Root (Root Folder)", parent.FolderName),
                "สร้าง Folder ใหม่",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (mainChoice == DialogResult.Cancel) return;

            if (mainChoice == DialogResult.No)
            {
                // สร้าง root folder - เพิ่มตัวเลือก Auto 4 Folders
                if (!_selectedOrgUnitId.HasValue) return;
                
                var typeChoice = MessageBox.Show(
                    "เลือกประเภทการสร้าง Root Folder:\n\n" +
                    "[Yes] สร้าง Folder เดี่ยว\n" +
                    "[No] สร้าง Auto 4 Folders (1HC, 2SC, 3NC, 4PC)",
                    "สร้าง Root Folder",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (typeChoice == DialogResult.Cancel) return;

                if (typeChoice == DialogResult.Yes)
                {
                    string folderName;
                    int folderLevel;
                    if (!PromptForNameAndLevel("สร้าง Root Folder", out folderName, out folderLevel)) return;
                    _presenter.CreateRootFolder(folderName, _selectedOrgUnitId.Value, folderLevel);
                }
                else
                {
                    string baseName = PromptForBaseName("สร้าง Auto 4 Root Folders");
                    if (baseName == null) return;
                    CreateAuto4Folders(baseName, null, _selectedOrgUnitId.Value);
                }
            }
            else
            {
                // สร้าง sub-folder ใต้ parent ที่เลือก - เพิ่มตัวเลือก Auto 4 Folders
                var typeChoice = MessageBox.Show(
                    string.Format("เลือกประเภทการสร้าง Sub-folder ใต้ \"{0}\":\n\n", parent.FolderName) +
                    "[Yes] สร้าง Folder เดี่ยว\n" +
                    "[No] สร้าง Auto 4 Folders (1HC, 2SC, 3NC, 4PC)",
                    "สร้าง Sub-folder",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (typeChoice == DialogResult.Cancel) return;

                if (typeChoice == DialogResult.Yes)
                {
                    string folderName;
                    int folderLevel;
                    if (!PromptForNameAndLevel(string.Format("สร้าง Folder ใหม่ใต้ \"{0}\"", parent.FolderName), out folderName, out folderLevel)) return;
                    _presenter.CreateFolder(parent.FolderId, folderName, parent.OrgUnitId, folderLevel);
                }
                else
                {
                    string baseName = PromptForBaseName(string.Format("สร้าง Auto 4 Sub-folders ใต้ \"{0}\"", parent.FolderName));
                    if (baseName == null) return;
                    CreateAuto4Folders(baseName, parent.FolderId, parent.OrgUnitId);
                }
            }
        }

        private void btnRootFolder_Click(object sender, EventArgs e)
        {
            if (!_selectedOrgUnitId.HasValue) return;

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
                    
                    string baseName = "เอกสาร";
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
                    string baseName = selected.FolderName ?? selected.Name ?? "เอกสาร";
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

            var result = MessageBox.Show(
                $"หน่วยงาน \"{_selectedOrgUnitName}\" ยังไม่มีโฟลเดอร์\n\nต้องการสร้างโฟลเดอร์แรกหรือไม่?",
                "สร้างโฟลเดอร์แรก",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // เสนอตัวเลือกการสร้าง
                var choice = MessageBox.Show(
                    "เลือกประเภทการสร้างโฟลเดอร์:\n\n" +
                    "[Yes] สร้างโฟลเดอร์เดี่ยว\n" +
                    "[No] สร้าง Auto 4 Folders (1HC, 2SC, 3NC, 4PC)",
                    "ประเภทโฟลเดอร์",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (choice == DialogResult.Yes)
                {
                    // สร้างโฟลเดอร์เดี่ยว
                    string folderName;
                    int level;
                    if (PromptForNameAndLevel("สร้างโฟลเดอร์แรก", out folderName, out level))
                    {
                        _presenter.CreateRootFolder(folderName, _selectedOrgUnitId.Value, level);
                    }
                }
                else if (choice == DialogResult.No)
                {
                    // สร้าง Auto 4 Folders แบบเงียบ - ไม่แสดงข้อความใดๆ
                    string baseName = "เอกสาร";
                    CreateAuto4FoldersQuiet(baseName, null, _selectedOrgUnitId.Value);
                    
                    // แสดงข้อความสั้นๆ แล้วซ่อนทันที (ไม่ต้องรอ user กด OK)
                    Task.Run(async () =>
                    {
                        await Task.Delay(1500); // แสดง 1.5 วินาที
                        // ข้อความจะหายไปเอง
                    });
                    
                    // หรือแสดงใน status bar แทน
                    if (lblOrgUnitStatus != null)
                    {
                        var originalText = lblOrgUnitStatus.Text;
                        lblOrgUnitStatus.Text = "✅ สร้าง Auto 4 Folders เรียบร้อยแล้ว";
                        lblOrgUnitStatus.ForeColor = Color.Green;
                        
                        // คืนค่าเดิมหลัง 3 วินาที
                        Task.Run(async () =>
                        {
                            await Task.Delay(3000);
                            this.Invoke((Action)(() =>
                            {
                                lblOrgUnitStatus.Text = originalText;
                                lblOrgUnitStatus.ForeColor = SystemColors.ControlText;
                            }));
                        });
                    }
                }
                // ถ้า choice == DialogResult.Cancel ไม่ทำอะไร
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
            var folder = SelectedFolder;
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

            var uploadForm = new FileUploadForm(_fileService, _folderService, _user, folder, _checkpointService);
            var result = uploadForm.ShowDialog(this.MdiParent ?? this);
            
            // ถ้าอัปโหลดสำเร็จ ให้ reload รายการไฟล์และโฟลเดอร์
            if (result == DialogResult.OK)
            {
                // Reload folder contents เพื่อแสดงไฟล์ใหม่
                if (_presenter != null && folder != null)
                {
                    _presenter.GetFolderContents(folder.FolderId);
                }
                
                // Reload folder tree เพื่ออัพเดท file count
                if (_presenter != null)
                {
                    _presenter.LoadFolderTree(_selectedOrgUnitId);
                }
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
                            var currentFolder = SelectedFolder;
                            if (currentFolder != null)
                                _presenter.GetFolderContents(currentFolder.FolderId);
                            _presenter.LoadFolderTree(_selectedOrgUnitId);
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
                ShowInfo(string.Format("ลบไฟล์ '{0}' เรียบร้อยแล้ว", file.OriginalFileName));

                var currentFolder = SelectedFolder;
                if (currentFolder != null)
                    _presenter.GetFolderContents(currentFolder.FolderId);
            }
            catch (UnauthorizedAccessException ex) { ShowError("ไม่มีสิทธิ์ลบไฟล์นี้: " + ex.Message); }
            catch (Exception ex)                   { ShowError("ไม่สามารถลบไฟล์ได้: " + ex.Message); }
        }

        private void DeleteSelectedFolder(Folder folder)
        {
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

        /// <summary>Shows a simple dialog to collect a reason string. Returns null if user cancels.</summary>
        private string PromptForReason(string prompt)
        {
            using (var dlg = new Form())
            {
                dlg.Text            = "ระบุเหตุผล";
                dlg.Width           = 420;
                dlg.Height          = 180;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;

                var lbl       = new Label    { Text = prompt, Left = 12, Top = 12,  Width = 380, Height = 32, AutoSize = false };
                var txt       = new TextBox  {                Left = 12, Top = 48,  Width = 380, Height = 24 };
                var btnOk     = new Button   { Text = "ตกลง",   Left = 220, Top = 100, Width = 80, DialogResult = DialogResult.OK };
                var btnCancel = new Button   { Text = "ยกเลิก", Left = 308, Top = 100, Width = 80, DialogResult = DialogResult.Cancel };

                dlg.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

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

            using (var dlg = new ApplyTemplateDialog(_folderService, _folderTemplateRepository, _orgStructureService, _user))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    // Switch treeOrgUnit to the OrgUnit that was just created
                    if (dlg.SelectedOrgUnitId.HasValue)
                    {
                        SelectOrgUnitNode(dlg.SelectedOrgUnitId.Value);
                    }
                    else
                    {
                        _presenter.LoadFolderTree(_selectedOrgUnitId);
                    }
                    // _presenter.LoadRecycleBin(_selectedOrgUnitId); // Hidden - recycle bin disabled
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
            using (var dlg    = new Form())
            using (var layout = new TableLayoutPanel())
            {
                dlg.Text            = "Change Confidentiality Level";
                dlg.ClientSize      = new Size(320, 120);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;

                layout.Dock        = DockStyle.Fill;
                layout.ColumnCount = 2;
                layout.RowCount    = 2;
                layout.Padding     = new Padding(8);
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                var lblLevel = new Label { Text = "New Level:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
                var cboLevel = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
                cboLevel.Items.AddRange(new object[]
                {
                    "1HC — Highly Confidential",
                    "2SC — Secret",
                    "3NC — Non-Confidential",
                    "4PC — Public/Common"
                });
                cboLevel.SelectedIndex = current - 1;

                var pnlButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
                var btnOk     = new Button { Text = "OK",     DialogResult = DialogResult.OK,     Width = 75 };
                var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 75 };
                pnlButtons.Controls.Add(btnCancel);
                pnlButtons.Controls.Add(btnOk);
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                layout.Controls.Add(lblLevel,   0, 0); layout.Controls.Add(cboLevel,   1, 0);
                layout.Controls.Add(pnlButtons, 0, 1);
                layout.SetColumnSpan(pnlButtons, 2);

                dlg.Controls.Add(layout);

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
                              "• 1HC - [ชื่อ] (High Confidential) - สีแดง\n" +
                              "• 2SC - [ชื่อ] (Semi Confidential) - สีส้ม\n" +
                              "• 3NC - [ชื่อ] (Normal Confidential) - สีน้ำเงิน\n" +
                              "• 4PC - [ชื่อ] (Public Confidential) - สีเขียว";
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
                        string folderName = string.Format("{0} - {1} ({2})", config.Code, baseName, config.Name);
                        
                        if (parentFolderId.HasValue)
                        {
                            // สร้าง sub-folder
                            _presenter.CreateFolder(parentFolderId.Value, folderName, orgUnitId, config.Level);
                        }
                        else
                        {
                            // สร้าง root folder
                            _presenter.CreateRootFolder(folderName, orgUnitId, config.Level);
                        }
                        
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add(string.Format("{0}: {1}", config.Code, ex.Message));
                    }
                }

                // แสดงผลลัพธ์
                if (successCount == 4)
                {
                    ShowInfo(string.Format("✅ สร้าง Auto 4 Folders สำเร็จ\n\nชื่อพื้นฐาน: {0}\n" +
                                         "• 1HC - {0} (High Confidential)\n" +
                                         "• 2SC - {0} (Semi Confidential)\n" +
                                         "• 3NC - {0} (Normal Confidential)\n" +
                                         "• 4PC - {0} (Public Confidential)", baseName));
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

        private string GetCleanBaseName(Folder folder)
        {
            string folderName = folder.FolderName ?? folder.Name ?? "เอกสาร";
            
            // ตรวจสอบว่าเป็น Auto 4 Folders format หรือไม่
            // Format: "1HC - ชื่อ (High Confidential)" หรือ "2SC - ชื่อ (Semi Confidential)" เป็นต้น
            var auto4Patterns = new[]
            {
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
                    
                    return string.IsNullOrWhiteSpace(baseName) ? "เอกสาร" : baseName;
                }
            }

            // ถ้าไม่ใช่ Auto 4 format ให้ใช้ชื่อเดิม
            return string.IsNullOrWhiteSpace(folderName) ? "เอกสาร" : folderName;
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
                        string folderName = string.Format("{0} - {1} ({2})", config.Code, baseName, config.Name);
                        
                        // Debug: Show folder name being created
                        System.Diagnostics.Debug.WriteLine($"Attempting to create folder: '{folderName}'");
                        
                        // ตรวจสอบว่ามี folder นี้อยู่แล้วหรือไม่
                        if (IsFolderExists(folderName, parentFolderId, orgUnitId))
                        {
                            System.Diagnostics.Debug.WriteLine($"Folder '{folderName}' already exists, skipping");
                            continue; // ข้าม folder ที่มีอยู่แล้ว (เงียบ)
                        }

                        if (parentFolderId.HasValue)
                        {
                            // สร้าง sub-folder
                            _presenter.CreateFolder(parentFolderId.Value, folderName, orgUnitId, config.Level);
                            System.Diagnostics.Debug.WriteLine($"Created sub-folder: '{folderName}'");
                        }
                        else
                        {
                            // สร้าง root folder
                            _presenter.CreateRootFolder(folderName, orgUnitId, config.Level);
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
            string fileCountText = string.Format(" ({0})", folder.FileCount);
            string label = string.Format("{0}{1} {2}{3}",
                folder.Name,
                fileCountText,
                badge,
                folder.IsArchived ? " [Archived]" : string.Empty);

            return new TreeNode(label)
            {
                Tag       = folder,
                ForeColor = ConfidentialityColor(folder.ConfidentialityLevel)
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
                string baseStoragePath = @"C:\FileStorage"; // Default path - adjust as needed
                
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

        /// <summary>Dialog ที่รับชื่อ Folder พร้อมเลือกระดับ คืนค่า null เมื่อ Cancel</summary>
        private bool PromptForNameAndLevel(string title, out string name, out int confidentialityLevel)
        {
            name                = null;
            confidentialityLevel = 1;

            using (var form   = new Form())
            using (var label  = new Label())
            using (var input  = new TextBox())
            using (var lblLv  = new Label())
            using (var combo  = new ComboBox())
            using (var ok     = new Button())
            using (var cancel = new Button())
            {
                form.Text            = title;
                form.ClientSize      = new Size(360, 155);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition   = FormStartPosition.CenterParent;
                form.MaximizeBox     = false;
                form.MinimizeBox     = false;

                label.Text     = "ชื่อ Folder:";
                label.Location = new Point(12, 12);
                label.Size     = new Size(336, 20);

                input.Location = new Point(12, 36);
                input.Size     = new Size(336, 23);

                lblLv.Text     = "ระดับ:";
                lblLv.Location = new Point(12, 72);
                lblLv.Size     = new Size(336, 20);

                combo.DropDownStyle = ComboBoxStyle.DropDownList;
                combo.Location      = new Point(12, 94);
                combo.Size          = new Size(336, 23);
                combo.Items.AddRange(new object[]
                {
                    "1HC — High Confidential",
                    "2SC — Semi Confidential", 
                    "3NC — Normal Confidential",
                    "4PC — Public Confidential"
                });
                combo.SelectedIndex = 0;

                ok.Text           = "OK";
                ok.DialogResult   = DialogResult.OK;
                ok.Location       = new Point(192, 118);
                ok.Size           = new Size(75, 26);
                form.AcceptButton = ok;

                cancel.Text         = "Cancel";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Location     = new Point(273, 118);
                cancel.Size         = new Size(75, 26);
                form.CancelButton   = cancel;

                form.Controls.AddRange(new Control[] { label, input, lblLv, combo, ok, cancel });

                if (form.ShowDialog(this) != DialogResult.OK) return false;

                string value = input.Text.Trim();
                if (string.IsNullOrEmpty(value)) return false;

                name                = value;
                confidentialityLevel = combo.SelectedIndex + 1;
                return true;
            }
        }

        /// <summary>Simple single-line input dialog.</summary>
        private string PromptForName(string title, string prompt, string defaultValue)
        {
            using (var form   = new Form())
            using (var label  = new Label())
            using (var input  = new TextBox())
            using (var ok     = new Button())
            using (var cancel = new Button())
            {
                form.Text            = title;
                form.ClientSize      = new Size(360, 110);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition   = FormStartPosition.CenterParent;
                form.MaximizeBox     = false;
                form.MinimizeBox     = false;

                label.Text     = prompt;
                label.Location = new Point(12, 12);
                label.Size     = new Size(336, 20);

                input.Text     = defaultValue ?? string.Empty;
                input.Location = new Point(12, 36);
                input.Size     = new Size(336, 23);
                input.SelectAll();

                ok.Text           = "OK";
                ok.DialogResult   = DialogResult.OK;
                ok.Location       = new Point(192, 72);
                ok.Size           = new Size(75, 26);
                form.AcceptButton = ok;

                cancel.Text         = "Cancel";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Location     = new Point(273, 72);
                cancel.Size         = new Size(75, 26);
                form.CancelButton   = cancel;

                form.Controls.AddRange(new Control[] { label, input, ok, cancel });

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

                // Build hierarchy for filtered folders (root folders only)
                foreach (var rootFolder in filteredFolders.Where(f => f.ParentFolderID == null).OrderBy(f => f.Name))
                {
                    var rootNode = CreateFolderNode(rootFolder);
                    LoadChildFolders(rootNode, rootFolder.FolderId);
                    folderTreeView.Nodes.Add(rootNode);
                }

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
                string icon     = FileIcon(file.FileExtension);
                string nameText = string.Format("{0}  {1}", icon, file.OriginalFileName);
                var item        = new ListViewItem(nameText);
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
                Font      = new Font("Microsoft Sans Serif", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 56, 100),
                Padding   = new Padding(4, 3, 0, 0)
            };

            // ListBox แสดงรายการ Quick Access
            _lstQuickAccess = new ListBox
            {
                Dock          = DockStyle.Fill,
                Font          = new Font("Microsoft Sans Serif", 9f),
                BorderStyle   = BorderStyle.None,
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
        /// Click handler: navigate folderTreeView ไปยัง Folder ที่เลือกใน Quick Access (Req 7.3)
        /// </summary>
        private void LstQuickAccess_Click(object sender, EventArgs e)
        {
            int idx = _lstQuickAccess.SelectedIndex;
            if (idx < 0 || idx >= _quickAccessItems.Count) return;

            var qa = _quickAccessItems[idx];
            if (qa.Folder == null || qa.Folder.IsDeleted || qa.Folder.IsArchived)
            {
                // Req 7.6: folder ไม่พร้อมใช้งาน — แจ้ง user
                string status = qa.Folder == null ? "ไม่พบ Folder นี้ในระบบ"
                    : qa.Folder.IsDeleted   ? "Folder นี้ถูกลบแล้ว"
                    : "Folder นี้ถูก Archive แล้ว";
                ShowInfo(string.Format("⚠️ {0} ไม่สามารถนำทางไปได้", status));
                return;
            }

            // ตรวจสอบว่า folder อยู่ใน OrgUnit เดียวกันหรือไม่
            if (_selectedOrgUnitId != qa.Folder.OrgUnitID)
            {
                // ต้องเปลี่ยน OrgUnit ก่อน
                _selectedOrgUnitId = qa.Folder.OrgUnitID;
                
                // อัพเดท OrgUnit selection ใน tree
                SelectOrgUnitNode(qa.Folder.OrgUnitID);
                
                // อัพเดท status label
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
                
                // โหลด folder tree ของ OrgUnit ใหม่
                _presenter.LoadFolderTree(_selectedOrgUnitId);
                
                // รอให้ tree โหลดเสร็จแล้วค่อยนำทาง (ใช้ timer เพื่อให้ UI อัพเดท)
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 100; // 100ms
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    timer.Dispose();
                    
                    // ลองนำทางไปยัง folder
                    if (!RestoreSelectedNode(folderTreeView.Nodes, qa.Folder.FolderID))
                    {
                        // ถ้ายังหาไม่เจอ ให้แสดงข้อความ
                        ShowInfo($"ไม่สามารถนำทางไปยังโฟลเดอร์ '{qa.Folder.FolderName}' ได้");
                    }
                };
                timer.Start();
            }
            else
            {
                // อยู่ใน OrgUnit เดียวกัน — นำทางตรงไป
                if (!RestoreSelectedNode(folderTreeView.Nodes, qa.Folder.FolderID))
                {
                    // ถ้าหา node ไม่เจอ (อาจเพราะ filter) ให้ reload tree ก่อน
                    _presenter.LoadFolderTree(_selectedOrgUnitId);
                    
                    // รอให้ tree โหลดเสร็จแล้วค่อยลองอีกครั้ง
                    var timer = new System.Windows.Forms.Timer();
                    timer.Interval = 100;
                    timer.Tick += (s, args) =>
                    {
                        timer.Stop();
                        timer.Dispose();
                        
                        if (!RestoreSelectedNode(folderTreeView.Nodes, qa.Folder.FolderID))
                        {
                            ShowInfo($"ไม่สามารถนำทางไปยังโฟลเดอร์ '{qa.Folder.FolderName}' ได้");
                        }
                    };
                    timer.Start();
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
            
            // Hide the entire bottom panel containing the recycle bin
            splitMain.Panel2Collapsed = true;
            
            // Optionally, you can also set the splitter to be fixed to prevent resizing
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
