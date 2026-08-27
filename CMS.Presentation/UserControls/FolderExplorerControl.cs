using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Infrastructure.Interfaces;

namespace CMS.Presentation.UserControls
{
    /// <summary>
    /// Explorer-style folder browser: left TreeView of a folder subtree + right ListView
    /// of files in the selected folder, with a toolbar for creating sub-folders.
    /// Used by FrmExternalUpload (KMS "เพิ่มบทความ" attachment upload).
    /// </summary>
    public partial class FolderExplorerControl : UserControl
    {
        private const int MaxFolderDepth = 10; // mirrors CMS.Services.FolderService.MaxFolderDepth

        private IFolderReadLookupController _folderReadLookup;
        private IFolderCommandController _folderCommand;
        private IFileBrowseController _fileBrowse;
        private IFileCommandController _fileCommand;
        private INetworkDriveHelper _networkDrive;
        private UserContext _user;

        private int _rootFolderId;
        private int _rootLevel;
        private readonly Dictionary<int, Folder> _folderMap = new Dictionary<int, Folder>();
        private readonly Dictionary<int, FileDocument> _fileMap = new Dictionary<int, FileDocument>();
        private bool _filePickMode;

        // Session scope: only folders created / files uploaded through this dialog may be deleted here.
        private readonly HashSet<int> _sessionFolderIds = new HashSet<int>();
        private readonly HashSet<int> _sessionFileIds = new HashSet<int>();

        /// <summary>Currently selected folder id (null = nothing selected yet).</summary>
        public int? SelectedFolderId { get; private set; }

        /// <summary>Depth of the currently selected folder (root folder = 1).</summary>
        public int? SelectedFolderDepth { get; private set; }

        /// <summary>Name of the currently selected folder (null = nothing selected).</summary>
        public string SelectedFolderName { get; private set; }

        /// <summary>Raised whenever the selected folder changes (user click or programmatic).</summary>
        public event EventHandler SelectedFolderChanged;

        /// <summary>Raised after a session-uploaded file is deleted here (arg = FileID).</summary>
        public event EventHandler<int> FileDeleted;

        /// <summary>Raised when the file-list selection changes (file-pick mode).</summary>
        public event EventHandler FileSelectionChanged;

        /// <summary>Raised when a file row is double-clicked (file-pick mode).</summary>
        public event EventHandler FilesDoubleClicked;

        public FolderExplorerControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// File-pick mode: multi-select files to return, no create/delete toolbar, no context menu.
        /// Set before Initialize/LoadRoot.
        /// </summary>
        public bool FilePickMode
        {
            get { return _filePickMode; }
            set
            {
                _filePickMode = value;
                if (value)
                {
                    lstFiles.MultiSelect = true;
                    lstFiles.ContextMenuStrip = null;
                    btnNewFolder.Visible = false;
                    btnDeleteFolder.Visible = false;
                    sep0.Visible = false;
                }
            }
        }

        /// <summary>File documents currently selected in the file list (file-pick mode).</summary>
        public IList<FileDocument> GetSelectedFiles()
        {
            var result = new List<FileDocument>();
            foreach (ListViewItem item in lstFiles.SelectedItems)
            {
                if (item.Tag is int id && _fileMap.TryGetValue(id, out var doc))
                    result.Add(doc);
            }
            return result;
        }

        /// <summary>
        /// Resolve the DMS root folder id for a confidentiality level (1-4) from app.config
        /// (<c>ExternalUpload.FolderId.Level{n}</c>) with 434/435/436/437 fallbacks.
        /// </summary>
        public static int ResolveLevelFolderId(int level)
        {
            string configVal = System.Configuration.ConfigurationManager.AppSettings[
                string.Format("ExternalUpload.FolderId.Level{0}", level)];
            if (!string.IsNullOrEmpty(configVal) && int.TryParse(configVal, out int parsed))
                return parsed;
            switch (level)
            {
                case 1: return 434;
                case 2: return 435;
                case 3: return 436;
                default: return 437;
            }
        }

        public void Initialize(
            IFolderReadLookupController folderReadLookup,
            IFolderCommandController folderCommand,
            IFileBrowseController fileBrowse,
            IFileCommandController fileCommand,
            UserContext user,
            INetworkDriveHelper networkDrive = null)
        {
            _folderReadLookup = folderReadLookup;
            _folderCommand = folderCommand;
            _fileBrowse = fileBrowse;
            _fileCommand = fileCommand;
            _networkDrive = networkDrive;
            _user = user;

            bool ready = _folderReadLookup != null && _folderCommand != null && _user != null;
            btnNewFolder.Enabled = ready;
            btnDeleteFolder.Enabled = false;
            btnVerify.Visible = _networkDrive != null && _fileBrowse != null;
            if (!ready)
                lblFilesHeader.Text = "ไม่พร้อมใช้งาน (composition root ไม่ครบ)";
        }

        /// <summary>Register file ids just uploaded through this dialog so they become deletable here.</summary>
        public void MarkFilesUploaded(IEnumerable<int> fileIds)
        {
            if (fileIds == null) return;
            foreach (var id in fileIds)
                _sessionFileIds.Add(id);
        }

        /// <summary>
        /// Load the tree rooted at <paramref name="levelFolderId"/> (one of the 1HC/2SC/3NC/4PC roots).
        /// </summary>
        public void LoadRoot(int levelFolderId, int level)
        {
            _rootFolderId = levelFolderId;
            _rootLevel = level;
            SelectedFolderId = null;
            SelectedFolderDepth = null;
            SelectedFolderName = null;
            _folderMap.Clear();

            treeFolders.BeginUpdate();
            treeFolders.Nodes.Clear();
            lstFiles.Items.Clear();
            lblFilesHeader.Text = "ไฟล์ในโฟลเดอร์นี้";

            if (_folderReadLookup == null)
            {
                treeFolders.EndUpdate();
                return;
            }

            Folder root;
            try
            {
                root = _folderReadLookup.GetFolder(levelFolderId, _user);
            }
            catch (Exception ex)
            {
                treeFolders.EndUpdate();
                MessageBox.Show("โหลดโฟลเดอร์ระดับหลักไม่สำเร็จ: " + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (root == null)
            {
                treeFolders.EndUpdate();
                MessageBox.Show(string.Format("ไม่พบโฟลเดอร์ระดับ {0} (ID: {1})", level, levelFolderId),
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _folderMap[root.FolderID] = root;

            IList<Folder> descendants;
            try
            {
                descendants = _folderReadLookup.GetDescendantsByUser(levelFolderId) ?? new List<Folder>();
            }
            catch (Exception ex)
            {
                descendants = new List<Folder>();
                System.Diagnostics.Debug.WriteLine("GetDescendantsByUser failed: " + ex.Message);
            }

            foreach (var f in descendants)
            {
                if (f != null && !f.IsDeleted && !f.IsArchived)
                    _folderMap[f.FolderID] = f;
            }

            var rootNode = CreateNode(root);
            treeFolders.Nodes.Add(rootNode);
            AddChildren(rootNode, root.FolderID);
            ExpandToLevel(treeFolders.Nodes, 0, 2);
            treeFolders.SelectedNode = rootNode;
            treeFolders.EndUpdate();

            // AfterSelect fires for the programmatic selection above and drives the rest.
        }

        /// <summary>Reload the file list for the currently selected folder.</summary>
        public void ReloadFiles()
        {
            LoadFiles(SelectedFolderId);
        }

        // ── Tree building ─────────────────────────────────────────────────────

        private void AddChildren(TreeNode parentNode, int parentId)
        {
            var children = _folderMap.Values
                .Where(f => f.ParentFolderID == parentId)
                .OrderBy(f => f.FolderName, StringComparer.CurrentCultureIgnoreCase);

            foreach (var child in children)
            {
                var node = CreateNode(child);
                parentNode.Nodes.Add(node);
                AddChildren(node, child.FolderID);
            }
        }

        private TreeNode CreateNode(Folder f)
        {
            string badge = LevelBadge(f.ConfidentialityLevel);
            bool badgeInName = !string.IsNullOrEmpty(f.FolderName)
                && f.FolderName.IndexOf(badge, StringComparison.OrdinalIgnoreCase) >= 0;
            string label = badgeInName ? f.FolderName : string.Format("{0}  {1}", f.FolderName, badge);
            return new TreeNode(label)
            {
                Tag = f,
                ForeColor = LevelColor(f.ConfidentialityLevel)
            };
        }

        private static void ExpandToLevel(TreeNodeCollection nodes, int cur, int max)
        {
            if (cur >= max) return;
            foreach (TreeNode n in nodes)
            {
                n.Expand();
                ExpandToLevel(n.Nodes, cur + 1, max);
            }
        }

        // ── Events ────────────────────────────────────────────────────────────

        private void TreeFolders_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var folder = e.Node?.Tag as Folder;
            if (folder == null)
            {
                SelectedFolderId = null;
                SelectedFolderDepth = null;
                SelectedFolderName = null;
            }
            else
            {
                SelectedFolderId = folder.FolderID;
                SelectedFolderDepth = folder.FolderDepth;
                SelectedFolderName = folder.FolderName;
            }

            UpdateToolbarState();
            LoadFiles(SelectedFolderId);
            SelectedFolderChanged?.Invoke(this, EventArgs.Empty);
        }

        private void LoadFiles(int? folderId)
        {
            lstFiles.BeginUpdate();
            lstFiles.Items.Clear();
            _fileMap.Clear();

            if (folderId == null || _fileBrowse == null)
            {
                lblFilesHeader.Text = "ไฟล์ในโฟลเดอร์นี้";
                lstFiles.EndUpdate();
                return;
            }

            IList<FileDocument> files;
            try
            {
                files = _fileBrowse.GetFilesInFolder(folderId.Value, _user) ?? new List<FileDocument>();
            }
            catch (Exception ex)
            {
                files = new List<FileDocument>();
                System.Diagnostics.Debug.WriteLine("GetFilesInFolder failed: " + ex.Message);
            }

            foreach (var f in files.OrderBy(f => f.OriginalFileName, StringComparer.CurrentCultureIgnoreCase))
            {
                var item = new ListViewItem(FileIcon(f.FileExtension));
                item.SubItems.Add(f.OriginalFileName);
                item.SubItems.Add(FormatBytes(f.FileSizeBytes));
                item.SubItems.Add((f.FileExtension ?? "").TrimStart('.').ToUpperInvariant());
                item.SubItems.Add(f.UploadedDate == DateTime.MinValue ? "" : f.UploadedDate.ToString("dd/MM/yyyy HH:mm"));
                item.SubItems.Add(""); // colFileState — filled by BtnVerify_Click
                item.Tag = f.FileID;
                lstFiles.Items.Add(item);
                _fileMap[f.FileID] = f;
            }

            lblFilesHeader.Text = string.Format("ไฟล์ในโฟลเดอร์นี้ ({0})", lstFiles.Items.Count);
            lstFiles.EndUpdate();
        }

        private void BtnNewFolder_Click(object sender, EventArgs e)
        {
            if (_folderCommand == null || _folderReadLookup == null || _user == null)
            {
                MessageBox.Show("ระบบสร้างโฟลเดอร์ยังไม่ถูกตั้งค่าผ่าน composition root",
                    "ไม่พร้อมใช้งาน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var parentNode = treeFolders.SelectedNode;
            var parent = parentNode?.Tag as Folder;
            if (parent == null)
            {
                MessageBox.Show("กรุณาเลือกโฟลเดอร์ที่จะสร้างโฟลเดอร์ย่อยข้างใต้",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (parent.FolderDepth >= MaxFolderDepth)
            {
                MessageBox.Show(string.Format("โฟลเดอร์นี้อยู่ชั้นลึกสุดแล้ว (สูงสุด {0} ชั้น) — สร้างย่อยต่อไม่ได้", MaxFolderDepth),
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name;
            using (var dlg = new FolderNameInputDialog())
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK)
                    return;
                name = dlg.FolderName;
            }
            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                var created = _folderCommand.CreateFolder(new CreateFolderRequest
                {
                    ParentFolderId = parent.FolderID,
                    OrgUnitId = parent.OrgUnitID,
                    Name = name.Trim(),
                    ConfidentialityLevel = null // inherit from parent (InheritPermission = true)
                }, _user);

                _folderMap[created.FolderID] = created;
                _sessionFolderIds.Add(created.FolderID);
                var newNode = CreateNode(created);
                parentNode.Nodes.Add(newNode);
                parentNode.Expand();
                treeFolders.SelectedNode = newNode;
                newNode.EnsureVisible();
            }
            catch (Exception ex)
            {
                MessageBox.Show("สร้างโฟลเดอร์ไม่สำเร็จ:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteFolder_Click(object sender, EventArgs e)
        {
            var node = treeFolders.SelectedNode;
            var folder = node?.Tag as Folder;
            if (folder == null) return;

            if (!_sessionFolderIds.Contains(folder.FolderID))
            {
                MessageBox.Show("ลบได้เฉพาะโฟลเดอร์ที่สร้างในหน้าต่างนี้เท่านั้น",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (node.Nodes.Count > 0)
            {
                MessageBox.Show("โฟลเดอร์นี้มีโฟลเดอร์ย่อย กรุณาลบโฟลเดอร์ย่อยก่อน",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Files currently in this folder
            var fileIds = new List<int>();
            foreach (ListViewItem item in lstFiles.Items)
            {
                if (item.Tag is int fid) fileIds.Add(fid);
            }
            var sessionFiles = fileIds.Where(id => _sessionFileIds.Contains(id)).ToList();
            var foreignFiles = fileIds.Where(id => !_sessionFileIds.Contains(id)).ToList();

            if (foreignFiles.Count > 0)
            {
                MessageBox.Show(
                    string.Format("โฟลเดอร์นี้มีไฟล์ {0} รายการที่ไม่ได้อัปโหลดในหน้าต่างนี้ — ลบไม่ได้", foreignFiles.Count),
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string msg = sessionFiles.Count > 0
                ? string.Format("ลบโฟลเดอร์ \"{0}\" และไฟล์ {1} รายการที่เพิ่งอัปโหลด?", folder.FolderName, sessionFiles.Count)
                : string.Format("ลบโฟลเดอร์ \"{0}\"?", folder.FolderName);
            if (MessageBox.Show(msg, "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                foreach (var fid in sessionFiles)
                {
                    _fileCommand.DeleteFile(fid, _user);
                    _sessionFileIds.Remove(fid);
                    FileDeleted?.Invoke(this, fid);
                }

                _folderCommand.DeleteFolder(folder.FolderID, _user);

                _sessionFolderIds.Remove(folder.FolderID);
                _folderMap.Remove(folder.FolderID);
                var parentNode = node.Parent;
                node.Remove();
                if (parentNode != null) treeFolders.SelectedNode = parentNode;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ลบโฟลเดอร์ไม่สำเร็จ:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadFiles(SelectedFolderId); // resync after a partial delete
            }
        }

        private void LstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            FileSelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void LstFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItems.Count > 0)
                FilesDoubleClicked?.Invoke(this, EventArgs.Empty);
        }

        private void LstFiles_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var hit = lstFiles.GetItemAt(e.X, e.Y);
            if (hit != null)
            {
                hit.Selected = true;
                hit.Focused = true;
            }
        }

        private void CmsFiles_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (lstFiles.SelectedItems.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            bool deletable = _fileCommand != null
                && lstFiles.SelectedItems.Count == 1
                && lstFiles.SelectedItems[0].Tag is int fid
                && _sessionFileIds.Contains(fid);
            miDeleteFile.Enabled = deletable;
        }

        private void MiDeleteFile_Click(object sender, EventArgs e)
        {
            if (_fileCommand == null || lstFiles.SelectedItems.Count != 1) return;
            if (!(lstFiles.SelectedItems[0].Tag is int fileId)) return;
            if (!_sessionFileIds.Contains(fileId))
            {
                MessageBox.Show("ลบได้เฉพาะไฟล์ที่อัปโหลดในหน้าต่างนี้เท่านั้น",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string fileName = lstFiles.SelectedItems[0].SubItems.Count > 1
                ? lstFiles.SelectedItems[0].SubItems[1].Text : ("ID " + fileId);
            if (MessageBox.Show(string.Format("ลบไฟล์ \"{0}\"?", fileName), "ยืนยันการลบ",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                _fileCommand.DeleteFile(fileId, _user);
                _sessionFileIds.Remove(fileId);
                FileDeleted?.Invoke(this, fileId);
                LoadFiles(SelectedFolderId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ลบไฟล์ไม่สำเร็จ:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (_rootFolderId > 0)
                LoadRoot(_rootFolderId, _rootLevel);
        }

        private void BtnVerify_Click(object sender, EventArgs e)
        {
            if (_networkDrive == null || lstFiles.Items.Count == 0) return;

            // Snapshot rows (item + FileDocument) on the UI thread
            var rows = new List<Tuple<ListViewItem, FileDocument>>();
            foreach (ListViewItem item in lstFiles.Items)
            {
                if (item.Tag is int fid && _fileMap.TryGetValue(fid, out var doc))
                    rows.Add(Tuple.Create(item, doc));
            }
            if (rows.Count == 0) return;

            btnVerify.Enabled = false;
            btnRefresh.Enabled = false;
            lblFilesHeader.Text = "กำลังตรวจสอบไฟล์...";

            var results = new List<Tuple<ListViewItem, string, Color, bool>>(); // item, state, color, isProblem
            int problems = 0;

            var worker = new System.ComponentModel.BackgroundWorker();
            worker.DoWork += (s, ev) =>
            {
                foreach (var row in rows)
                {
                    var doc = row.Item2;
                    string state;
                    Color color;
                    bool problem = true;

                    if (string.IsNullOrEmpty(doc.PhysicalPath))
                    {
                        state = "✖ ไม่มี path"; color = Color.FromArgb(180, 0, 0);
                    }
                    else if (!_networkDrive.FileExists(doc.PhysicalPath))
                    {
                        state = "✖ ไม่พบไฟล์"; color = Color.FromArgb(180, 0, 0);
                    }
                    else
                    {
                        long size = _networkDrive.GetFileSize(doc.PhysicalPath);
                        if (size == 0 && doc.FileSizeBytes > 0)
                        {
                            state = "⚠ 0 bytes"; color = Color.FromArgb(200, 80, 0);
                        }
                        else if (size >= 0 && size != doc.FileSizeBytes)
                        {
                            state = string.Format("⚠ ขนาดไม่ตรง ({0})", FormatBytes(size));
                            color = Color.FromArgb(200, 80, 0);
                        }
                        else
                        {
                            state = "✔ ปกติ"; color = Color.FromArgb(0, 130, 60); problem = false;
                        }
                    }
                    if (problem) problems++;
                    results.Add(Tuple.Create(row.Item1, state, color, problem));
                }
            };
            worker.RunWorkerCompleted += (s, ev) =>
            {
                lstFiles.BeginUpdate();
                foreach (var r in results)
                {
                    while (r.Item1.SubItems.Count <= 5) r.Item1.SubItems.Add("");
                    r.Item1.UseItemStyleForSubItems = false;
                    r.Item1.SubItems[5].Text = r.Item2;
                    r.Item1.SubItems[5].ForeColor = r.Item3;
                    if (r.Item4) r.Item1.BackColor = Color.FromArgb(255, 244, 244);
                }
                lstFiles.EndUpdate();

                lblFilesHeader.Text = problems > 0
                    ? string.Format("ไฟล์ {0} — พบปัญหา {1} รายการ", rows.Count, problems)
                    : string.Format("ไฟล์ {0} — ตรวจแล้วปกติทั้งหมด", rows.Count);

                btnVerify.Enabled = true;
                btnRefresh.Enabled = true;

                if (ev.Error != null)
                    MessageBox.Show("ตรวจสอบไฟล์ผิดพลาด: " + ev.Error.Message,
                        "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            worker.RunWorkerAsync();
        }

        private void BtnExpand_Click(object sender, EventArgs e)
        {
            treeFolders.BeginUpdate();
            treeFolders.ExpandAll();
            treeFolders.EndUpdate();
            if (treeFolders.Nodes.Count > 0) treeFolders.Nodes[0].EnsureVisible();
        }

        private void BtnCollapse_Click(object sender, EventArgs e)
        {
            treeFolders.BeginUpdate();
            treeFolders.CollapseAll();
            if (treeFolders.Nodes.Count > 0) treeFolders.Nodes[0].Expand();
            treeFolders.EndUpdate();
        }

        private void UpdateToolbarState()
        {
            bool ready = _folderReadLookup != null && _folderCommand != null && _user != null;
            var folder = treeFolders.SelectedNode?.Tag as Folder;
            btnNewFolder.Enabled = ready && folder != null && folder.FolderDepth < MaxFolderDepth;
            btnDeleteFolder.Enabled = ready && _fileCommand != null
                && folder != null && _sessionFolderIds.Contains(folder.FolderID);
        }

        // ── Helpers (mirrors FolderPickerDialog / FilesByFolderForm) ──────────

        private static Color LevelColor(int level)
        {
            switch (level)
            {
                case 1: return Color.FromArgb(180, 0, 0);
                case 2: return Color.FromArgb(200, 80, 0);
                case 3: return Color.FromArgb(0, 80, 160);
                default: return Color.FromArgb(0, 130, 60);
            }
        }

        private static string LevelBadge(int level)
        {
            switch (level)
            {
                case 1: return "[1HC]";
                case 2: return "[2SC]";
                case 3: return "[3NC]";
                default: return "[4PC]";
            }
        }

        private static string FileIcon(string ext)
        {
            switch ((ext ?? "").ToLowerInvariant())
            {
                case ".pdf": return "📕";
                case ".doc":
                case ".docx": return "📘";
                case ".xls":
                case ".xlsx":
                case ".csv": return "📗";
                case ".ppt":
                case ".pptx": return "📙";
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".bmp": return "🖼️";
                case ".zip":
                case ".rar":
                case ".7z": return "🗜️";
                default: return "📄";
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1024 * 1024L) return string.Format("{0:0.##} MB", (double)bytes / (1024 * 1024));
            if (bytes >= 1024L) return string.Format("{0:0.##} KB", (double)bytes / 1024);
            return string.Format("{0} Bytes", bytes);
        }
    }
}
