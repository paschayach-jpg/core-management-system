using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Presentation.Presenters;
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
        private List<Folder> _treeRoots      = new List<Folder>();
        private List<Folder> _recycleBinList = new List<Folder>();

        // OrgUnit scope passed in from MDI_Container
        private readonly int? _orgUnitId;

        // Keep a reference to the service for confidentiality level changes (Req 7.5)
        private readonly IFolderService       _folderService;
        private readonly IFileService         _fileService;
        private readonly IOrgStructureService _orgStructureService;
        private readonly IFolderTemplateRepository _folderTemplateRepository;
        private readonly UserContext          _user;

        // Currently selected OrgUnit filter (null = all)
        private int? _selectedOrgUnitId;
        private string _selectedOrgUnitName = "— ทั้งหมด —";

        // Cache last selected folder so MDI toolbar can access it even after focus change
        private Folder _lastSelectedFolder;

        // Track selected file in content list
        private List<FileDocument> _currentFiles = new List<FileDocument>();

        // Track active confidentiality level filter (0 = no filter, 1-4 = filtered level)
        private int _activeConfidentialityFilter = 0;

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        public FolderForm(IFolderService service, UserContext user, int? orgUnitId = null,
                          IFileService fileService = null, IOrgStructureService orgStructureService = null,
                          IFolderTemplateRepository folderTemplateRepository = null)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (user    == null) throw new ArgumentNullException("user");

            _orgUnitId                = orgUnitId;
            _selectedOrgUnitId        = orgUnitId;
            _folderService            = service;
            _fileService              = fileService;
            _orgStructureService      = orgStructureService;
            _folderTemplateRepository = folderTemplateRepository;
            _user                     = user;

            InitializeComponent();

            _presenter = new FolderPresenter(service, this, user, fileService);

            // Manager/Admin can change confidentiality level (Req 7.5)
            bool canChangeLevel = user.Role == UserRole.Manager || user.Role == UserRole.Admin;
            btnChangeLevel.Visible = canChangeLevel;

            // Root folder button only for Manager/Admin
            btnRootFolder.Visible = canChangeLevel;

            // Wire content list selection for detail panel
            contentsListView.SelectedIndexChanged += ContentsListView_SelectedIndexChanged;
            
            // Wire double-click to open files
            contentsListView.DoubleClick += ContentsListView_DoubleClick;

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

            // Wire OrgUnit button
            btnSelectOrgUnit.Click += BtnSelectOrgUnit_Click;

            // Build right-click context menu for folderTreeView
            BuildFolderTreeContextMenu();

            // Build right-click context menu for contentsListView
            BuildContentsListViewContextMenu();
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
            _presenter.LoadRecycleBin(_selectedOrgUnitId);
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

        private void BtnSelectOrgUnit_Click(object sender, EventArgs e)
        {
            if (_orgStructureService == null)
            {
                ShowError("OrgStructure service is not available.");
                return;
            }

            using (var dlg = new OrgUnitSelectorDialog(_orgStructureService))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _selectedOrgUnitId = dlg.SelectedOrgUnitId;
                    _selectedOrgUnitName = dlg.SelectedOrgUnitName ?? "— ทั้งหมด —";
                    lblSelectedOrgUnit.Text = _selectedOrgUnitName;
                    _presenter.LoadFolderTree(_selectedOrgUnitId);
                    _presenter.LoadRecycleBin(_selectedOrgUnitId);
                }
            }
        }

        // -----------------------------------------------------------------------
        // Form load
        // -----------------------------------------------------------------------

        private void FolderForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            lblSelectedOrgUnit.Text = _selectedOrgUnitName;
            
            // Wire confidentiality level filter checkboxes
            chk1HC.CheckedChanged += new EventHandler(this.OnConfidentialityLevelChanged);
            chk2SC.CheckedChanged += new EventHandler(this.OnConfidentialityLevelChanged);
            chk3NC.CheckedChanged += new EventHandler(this.OnConfidentialityLevelChanged);
            chk4PC.CheckedChanged += new EventHandler(this.OnConfidentialityLevelChanged);
            
            _presenter.LoadFolderTree(_selectedOrgUnitId);
            _presenter.LoadRecycleBin(_selectedOrgUnitId);
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
            }

            folderTreeView.ExpandAll();
            folderTreeView.EndUpdate();

            // ถ้ามี folder ที่เลือกอยู่ก่อน → restore selection เดิม
            // ถ้าไม่มี (เปิด form ครั้งแรก) → auto-select first root folder
            if (_treeRoots.Count == 0)
            {
                // ไม่มี folder เลย → clear ด้านขวาทันที
                ClearContentPanel();
            }
            else if (previousFolderId.HasValue)
            {
                RestoreSelectedNode(folderTreeView.Nodes, previousFolderId.Value);
            }
            else if (folderTreeView.Nodes.Count > 0 && folderTreeView.Nodes[0].Tag is Folder)
            {
                folderTreeView.SelectedNode = folderTreeView.Nodes[0];
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
            _recycleBinList = deletedFolders == null ? new List<Folder>() : deletedFolders.ToList();

            recycleBinListView.Items.Clear();

            foreach (var folder in _recycleBinList)
            {
                var item = new ListViewItem(folder.Name);
                item.SubItems.Add(ConfidentialityLabel(folder.ConfidentialityLevel));
                item.SubItems.Add(folder.DeletedAt.HasValue
                    ? folder.DeletedAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                    : string.Empty);
                item.Tag = folder;
                recycleBinListView.Items.Add(item);
            }

            // Update recycle bin count badge in explorer panel
            lblRecycleBinCount.Text    = _recycleBinList.Count.ToString();
            lblRecycleBinCount.Visible = _recycleBinList.Count > 0;
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

        public Folder SelectedRecycleBinFolder
        {
            get
            {
                if (recycleBinListView.SelectedItems.Count == 0) return null;
                return recycleBinListView.SelectedItems[0].Tag as Folder;
            }
        }

        /// <summary>Returns checked folders, falling back to selected if none checked.</summary>
        private List<Folder> GetTargetRecycleBinFolders()
        {
            var checked_ = recycleBinListView.CheckedItems.Cast<ListViewItem>()
                .Select(i => i.Tag as Folder).Where(f => f != null).ToList();
            if (checked_.Count > 0) return checked_;
            var sel = SelectedRecycleBinFolder;
            return sel != null ? new List<Folder> { sel } : new List<Folder>();
        }

        private void recycleBinListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            int count = recycleBinListView.CheckedItems.Count;
            lblRbSelCount.Text = string.Format("เลือกแล้ว {0} รายการ", count);
            lblRbSelCount.ForeColor = count > 0
                ? System.Drawing.Color.FromArgb(0, 100, 0)
                : System.Drawing.Color.FromArgb(0, 51, 102);

            // sync select-all button without re-triggering
            btnRbSelectAll.CheckedChanged -= btnRbSelectAll_CheckedChanged;
            btnRbSelectAll.Checked = count > 0 && count == recycleBinListView.Items.Count;
            btnRbSelectAll.CheckedChanged += btnRbSelectAll_CheckedChanged;
        }

        private void btnRbSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in recycleBinListView.Items)
                item.Checked = btnRbSelectAll.Checked;
        }

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
                lblDetailHeader.Text  = "📋 ตัวอย่าง";
                lblDetailContent.Text = "เลือกรายการเพื่อดูรายละเอียด";
                lblContentInfo.Text   = lblContentInfo.Text.Contains("|")
                    ? lblContentInfo.Text.Substring(0, lblContentInfo.Text.LastIndexOf('|') + 1)
                      + " ไม่มีรายการที่เลือก"
                    : lblContentInfo.Text;
                return;
            }

            var tag = contentsListView.SelectedItems[0].Tag;
            var file = tag as FileDocument;
            var folder = tag as Folder;

            if (file != null)
            {
                lblDetailHeader.Text  = string.Format("📄 {0}", file.OriginalFileName);
                lblDetailContent.Text = string.Format(
                    "ประเภท: {0}\nขนาด: {1}\nอัพโหลด: {2}\nVersion: {3}\nChecksum: {4}\nสถานะ Scan: {5}",
                    file.FileExtension.TrimStart('.').ToUpper(),
                    FormatBytes(file.FileSizeBytes),
                    file.UploadedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    file.Version,
                    file.Checksum != null && file.Checksum.Length > 12
                        ? file.Checksum.Substring(0, 12) + "..." : file.Checksum,
                    VirusScanLabel(file.VirusScanStatus));
            }
            else if (folder != null)
            {
                lblDetailHeader.Text  = string.Format("📁 {0}", folder.Name);
                lblDetailContent.Text = string.Format(
                    "ระดับ: {0}\nไฟล์: {1} รายการ\nขนาด: {2}\nสร้าง: {3}\nสถานะ: {4}",
                    ConfidentialityLabel(folder.ConfidentialityLevel),
                    folder.FileCount,
                    FormatBytes(folder.FolderSizeBytes),
                    folder.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    folder.IsArchived ? "Archived" : "Active");
            }
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
                    _presenter.GetFolderContents(folder.FolderId);
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
        // TreeView — node selection loads contents
        // -----------------------------------------------------------------------

        private void ClearContentPanel()
        {
            contentsListView.Items.Clear();
            _currentFiles.Clear();
            txtBreadcrumb.Text            = "";
            lblContentInfo.Text           = "";
            lblConfidentialityBadge.Text  = "";
        }

        private void folderTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var folder = e.Node.Tag as Folder;
            if (folder == null) { ClearContentPanel(); return; }  // placeholder node
            _lastSelectedFolder = folder;   // cache for MDI toolbar access
            _presenter.GetFolderContents(folder.FolderId);

            // Update confidentiality badge in toolbar (Req 7.6)
            lblConfidentialityBadge.Text      = ConfidentialityLabel(folder.ConfidentialityLevel);
            lblConfidentialityBadge.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);

            // Update breadcrumb and content header
            txtBreadcrumb.Text  = BuildBreadcrumbPath(e.Node);
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
            _presenter.LoadRecycleBin(_selectedOrgUnitId);

            btnNavRefresh.Enabled = true;
            btnNavRefresh.Text    = "↺";
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
                    string rootName;
                    int    rootLevel;
                    if (!PromptForNameAndLevel("สร้าง Root Folder", out rootName, out rootLevel)) return;
                    _presenter.CreateRootFolder(rootName, _selectedOrgUnitId.Value, rootLevel);
                }
                return;
            }

            // มี folder ถูกเลือกอยู่ — ถามว่าจะสร้างที่ไหน
            var choice = MessageBox.Show(
                string.Format("ต้องการสร้าง Folder ใหม่ที่ไหน?\n\n[Yes]  ใต้ \"{0}\" (Sub-folder)\n[No]   ระดับ Root (Root Folder)", parent.FolderName),
                "สร้าง Folder ใหม่",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (choice == DialogResult.Cancel) return;

            string folderName;
            int    folderLevel;

            if (choice == DialogResult.No)
            {
                // สร้าง root folder
                if (!_selectedOrgUnitId.HasValue) return;
                if (!PromptForNameAndLevel("สร้าง Root Folder", out folderName, out folderLevel)) return;
                _presenter.CreateRootFolder(folderName, _selectedOrgUnitId.Value, folderLevel);
            }
            else
            {
                // สร้าง sub-folder ใต้ parent ที่เลือก
                if (!PromptForNameAndLevel(string.Format("สร้าง Folder ใหม่ใต้ \"{0}\"", parent.FolderName), out folderName, out folderLevel)) return;
                _presenter.CreateFolder(parent.FolderId, folderName, parent.OrgUnitId, folderLevel);
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

            var uploadForm = new FileUploadForm(_fileService, _folderService, _user, folder);
            uploadForm.ShowDialog(this.MdiParent ?? this);
        }

        private void btnUploadFile_Click(object sender, EventArgs e)
        {
            OpenUploadForSelectedFolder();
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            // Check if a file is selected first
            var file = SelectedFile;
            if (file != null)
            {
                string newName = PromptForName("เปลี่ยนชื่อไฟล์", "ชื่อใหม่:", file.OriginalFileName);
                if (string.IsNullOrWhiteSpace(newName)) return;

                try
                {
                    _fileService.RenameFile(file.FileID, newName, _user);
                    // Reload contents to reflect new name
                    var currentFolder = SelectedFolder;
                    if (currentFolder != null)
                        _presenter.GetFolderContents(currentFolder.FolderId);
                    ShowInfo(string.Format("เปลี่ยนชื่อเป็น '{0}' เรียบร้อยแล้ว", newName.Trim()));
                }
                catch (Exception ex)
                {
                    ShowError("ไม่สามารถเปลี่ยนชื่อไฟล์ได้: " + ex.Message);
                }
                return;
            }

            // If no file selected, check for folder
            var folder = SelectedFolder;
            if (folder == null)
            {
                ShowError("กรุณาเลือกโฟลเดอร์ที่ต้องการเปลี่ยนชื่อ");
                return;
            }

            string folderNewName = PromptForName("เปลี่ยนชื่อโฟลเดอร์", "ชื่อใหม่:", folder.Name);
            if (folderNewName == null) return;

            _presenter.RenameFolder(folder.FolderId, folderNewName);
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
            // Check if a file is selected first
            var file = SelectedFile;
            if (file != null)
            {
                // Delete file
                var confirm = MessageBox.Show(
                    string.Format("ลบไฟล์ '{0}' หรือไม่?\n\nไฟล์จะถูกย้ายไปยังถังขยะ", file.OriginalFileName),
                    "ยืนยันการลบไฟล์",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        if (_fileService != null)
                        {
                            _fileService.DeleteFile(file.FileDocumentId, _user);
                            ShowInfo(string.Format("ลบไฟล์ '{0}' เรียบร้อยแล้ว", file.OriginalFileName));
                            
                            // Refresh the current folder contents
                            var currentFolder = SelectedFolder;
                            if (currentFolder != null)
                            {
                                _presenter.GetFolderContents(currentFolder.FolderId);
                            }
                        }
                        else
                        {
                            ShowError("File service is not available.");
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        ShowError("ไม่มีสิทธิ์ลบไฟล์นี้: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        ShowError("ไม่สามารถลบไฟล์ได้: " + ex.Message);
                    }
                }
                return;
            }

            // If no file selected, check for folder
            var folder = SelectedFolder;
            if (folder == null)
            {
                ShowError("กรุณาเลือกไฟล์หรือโฟลเดอร์ที่ต้องการลบ");
                return;
            }

            var confirmFolder = MessageBox.Show(
                string.Format("ย้าย '{0}' ไปยังถังขยะหรือไม่?", folder.Name),
                "ยืนยันการลบโฟลเดอร์",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmFolder == DialogResult.Yes)
                _presenter.DeleteFolder(folder.FolderId);
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
                    // Reload tree to show newly created folders
                    _presenter.LoadFolderTree(_selectedOrgUnitId);
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

        // -----------------------------------------------------------------------
        // Recycle bin button handlers  (Req 4.12–4.13)
        // -----------------------------------------------------------------------

        private void btnRestore_Click(object sender, EventArgs e)
        {
            var targets = GetTargetRecycleBinFolders();
            if (targets.Count == 0) { ShowError("กรุณาเลือก Folder ที่ต้องการกู้คืน"); return; }

            string msg = targets.Count == 1
                ? string.Format("กู้คืน '{0}' ไปยัง Parent เดิม?", targets[0].Name)
                : string.Format("กู้คืน {0} รายการที่เลือกไปยัง Parent เดิม?", targets.Count);

            if (MessageBox.Show(msg, "ยืนยันการกู้คืน",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            foreach (var folder in targets)
                _presenter.RestoreFolder(folder.FolderId, null);
        }

        private void btnRestoreToParent_Click(object sender, EventArgs e)
        {
            var targets = GetTargetRecycleBinFolders();
            if (targets.Count == 0) { ShowError("กรุณาเลือก Folder ที่ต้องการกู้คืน"); return; }

            string parentIdStr = PromptForName(
                "กู้คืนไปยัง Parent อื่น",
                string.Format("กรอก ID ของ Parent Folder ที่ต้องการกู้คืน {0} รายการ:", targets.Count),
                string.Empty);
            if (parentIdStr == null) return;

            int parentId;
            if (!int.TryParse(parentIdStr, out parentId)) { ShowError("กรุณากรอก ID ที่ถูกต้อง"); return; }

            foreach (var folder in targets)
                _presenter.RestoreFolder(folder.FolderId, parentId);
        }

        private void btnPermanentDelete_Click(object sender, EventArgs e)
        {
            var targets = GetTargetRecycleBinFolders();
            if (targets.Count == 0) { ShowError("กรุณาเลือก Folder ที่ต้องการลบถาวร"); return; }

            string msg = targets.Count == 1
                ? string.Format("ลบถาวร '{0}' และเนื้อหาทั้งหมด? ไม่สามารถกู้คืนได้", targets[0].Name)
                : string.Format("ลบถาวร {0} รายการที่เลือก? ไม่สามารถกู้คืนได้", targets.Count);

            if (MessageBox.Show(msg, "ยืนยันการลบถาวร",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            foreach (var folder in targets)
                _presenter.PermanentlyDeleteFolder(folder.FolderId);
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Builds the full breadcrumb path from a TreeNode up to the root.
        /// e.g. "🏠 ราก › Core Management › คลังเอกสาร › 1HC - High Confidential"
        /// </summary>
        private static string BuildBreadcrumbPath(TreeNode node)
        {
            var parts = new System.Collections.Generic.List<string>();
            var current = node;
            while (current != null)
            {
                string text = current.Text;
                // Strip badge suffix like " [1HC]" or " [Archived]"
                int bracketIdx = text.LastIndexOf(" [");
                if (bracketIdx > 0) text = text.Substring(0, bracketIdx);
                parts.Insert(0, text.Trim());
                current = current.Parent;
            }
            parts.Insert(0, "🏠 ราก");
            return string.Join(" › ", parts);
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
                case 1: return "1HC - Highly Confidential";
                case 2: return "2SC - Secret";
                case 3: return "3NC - Non-Confidential";
                case 4: return "4PC - Public/Common";
                default: return level.ToString();
            }
        }

        private static Color ConfidentialityColor(int level)
        {
            switch (level)
            {
                case 1: return Color.DarkRed;
                case 2: return Color.OrangeRed;
                case 3: return Color.DarkGoldenrod;
                case 4: return Color.DarkGreen;
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

        /// <summary>Dialog ที่รับชื่อ Folder พร้อมเลือกระดับความลับ คืนค่า null เมื่อ Cancel</summary>
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

                lblLv.Text     = "ระดับความลับ:";
                lblLv.Location = new Point(12, 72);
                lblLv.Size     = new Size(336, 20);

                combo.DropDownStyle = ComboBoxStyle.DropDownList;
                combo.Location      = new Point(12, 94);
                combo.Size          = new Size(336, 23);
                combo.Items.AddRange(new object[]
                {
                    "1HC — Highly Confidential",
                    "2SC — Secret",
                    "3NC — Non-Confidential",
                    "4PC — Public/Common"
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
                else if (folderTreeView.Nodes.Count > 0 && folderTreeView.Nodes[0].Tag is Folder)
                    folderTreeView.SelectedNode = folderTreeView.Nodes[0];
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
    }
}
