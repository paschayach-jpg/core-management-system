using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// แสดงไฟล์ทั้งหมดจาก storage.FileDocument แยกตาม Folder
    /// เรียกใช้โดย Admin/Manager เท่านั้น
    /// </summary>
    public partial class FilesByFolderForm : Form
    {
        private readonly IFileBrowseController _fileBrowseController;
        private readonly UserContext  _user;

        private List<FileDocument> _allFiles = new List<FileDocument>();

        public FilesByFolderForm(IFileBrowseController fileBrowseController, UserContext user)
        {
            if (fileBrowseController == null) throw new ArgumentNullException("fileBrowseController");
            if (user        == null) throw new ArgumentNullException("user");
            _fileBrowseController = fileBrowseController;
            _user        = user;
            InitializeComponent();
        }

        private void FilesByFolderForm_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _allFiles = _fileBrowseController.GetAllFilesGroupedByFolder(_user).ToList();
                PopulateTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถโหลดข้อมูล: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ── Build left TreeView: one node per Folder ─────────────────────────

        private void PopulateTree()
        {
            treeView.BeginUpdate();
            treeView.Nodes.Clear();

            var byFolder = _allFiles
                .GroupBy(f => f.FolderID)
                .OrderBy(g => g.Key);

            foreach (var group in byFolder)
            {
                string folderName = group.First().FolderName ?? string.Format("Folder {0}", group.Key);
                string nodeLabel  = string.Format("📁 {0}  ({1} ไฟล์)", folderName, group.Count());

                var node = new TreeNode(nodeLabel) { Tag = group.Key };
                treeView.Nodes.Add(node);
            }

            treeView.ExpandAll();
            treeView.EndUpdate();

            // Auto-select first node
            if (treeView.Nodes.Count > 0)
                treeView.SelectedNode = treeView.Nodes[0];
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || !(e.Node.Tag is int)) return;
            int folderId = (int)e.Node.Tag;
            ShowFilesForFolder(folderId);
        }

        private void ShowFilesForFolder(int folderId)
        {
            listView.BeginUpdate();
            listView.Items.Clear();

            var files = _allFiles.Where(f => f.FolderID == folderId).ToList();

            foreach (var file in files)
            {
                var item = new ListViewItem(FileIcon(file.FileExtension));
                item.SubItems.Add(file.OriginalFileName);
                item.SubItems.Add(FormatBytes(file.FileSizeBytes));
                item.SubItems.Add(file.FileExtension.TrimStart('.').ToUpper());
                item.SubItems.Add(file.UploadedDate.ToString("dd/MM/yyyy HH:mm"));
                item.SubItems.Add(VirusScanLabel(file.VirusScanStatus));
                item.SubItems.Add(file.Version.ToString());
                item.Tag = file;

                if (file.VirusScanStatus == 2)
                    item.ForeColor = Color.DarkRed;
                else if (file.VirusScanStatus == 0)
                    item.ForeColor = Color.DarkOrange;

                listView.Items.Add(item);
            }

            listView.EndUpdate();
            lblInfo.Text = string.Format("Folder ID: {0}  |  {1} ไฟล์", folderId, files.Count);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

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
            if (bytes >= 1024L * 1024) return string.Format("{0:N1} MB", bytes / (1024.0 * 1024));
            if (bytes >= 1024)         return string.Format("{0:N1} KB", bytes / 1024.0);
            return string.Format("{0} B", bytes);
        }

        private static string VirusScanLabel(int status)
        {
            switch (status)
            {
                case 0: return "⏳ รอสแกน";
                case 1: return "✅ สะอาด";
                case 2: return "🦠 พบไวรัส";
                case 3: return "⚠️ ผิดพลาด";
                default: return "—";
            }
        }
    }
}
