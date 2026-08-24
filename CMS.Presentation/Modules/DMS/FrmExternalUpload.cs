using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Controllers.DMS.Document;
using CMS.Controllers.DMS.Document.Interfaces;

namespace CMS.Presentation
{
    public partial class FrmExternalUpload : Form, IFileUploadView
    {
        private readonly IFileUploadController _fileUploadController;
        private readonly UserContext _user;
        private readonly int _folderId;
        private readonly string _folderName;
        private readonly int? _defaultLevel;
        private readonly IUploadCheckpointController _checkpointController;

        private int _currentFolderId;
        private string _currentFolderName;

        private readonly List<string> _filePaths = new List<string>();
        private readonly List<Stream> _streamsToDispose = new List<Stream>();

        public class UploadedFileDetails
        {
            public int FileId { get; set; }
            public string FileName { get; set; }
            public long FileSize { get; set; }
        }

        public int? UploadedFileId { get; private set; }
        public List<int> UploadedFileIds { get; private set; } = new List<int>();
        public List<UploadedFileDetails> UploadedFiles { get; } = new List<UploadedFileDetails>();
        public int SelectedLevel { get; private set; } = 4; // Default to Level 4 (PC)
        public string UploadedFileName { get; private set; }
        public long UploadedFileSize { get; private set; }

        public FrmExternalUpload(
            IFileUploadController fileUploadController,
            UserContext user,
            int folderId,
            string folderName = null,
            int? defaultLevel = null,
            IUploadCheckpointController checkpointController = null)
        {
            _fileUploadController = fileUploadController ?? throw new ArgumentNullException(nameof(fileUploadController));
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _folderId = folderId;
            _folderName = folderName ?? $"ID: {folderId}";
            _currentFolderId = folderId;
            _currentFolderName = _folderName;
            _defaultLevel = defaultLevel;
            _checkpointController = checkpointController;

            InitializeComponent();
        }

        private void FrmExternalUpload_Load(object sender, EventArgs e)
        {
            // Populate Level ComboBox (Level 1-4)
            cboLevel.Items.Clear();
            cboLevel.Items.Add("Level 1 - Highly Confidential (HC)");
            cboLevel.Items.Add("Level 2 - Secret (SC)");
            cboLevel.Items.Add("Level 3 - Non-Confidential (NC)");
            cboLevel.Items.Add("Level 4 - Public/Common (PC)");

            // Resolve level folders from App.config with OrgUnit 219 fallbacks
            int level1Id = GetConfiguredFolderId(1, 434);
            int level2Id = GetConfiguredFolderId(2, 435);
            int level3Id = GetConfiguredFolderId(3, 436);
            int level4Id = GetConfiguredFolderId(4, 437);

            // Select default level based on current folder ID or default level parameter
            if (_folderId == level1Id)
            {
                cboLevel.SelectedIndex = 0; // Level 1
            }
            else if (_folderId == level2Id)
            {
                cboLevel.SelectedIndex = 1; // Level 2
            }
            else if (_folderId == level3Id)
            {
                cboLevel.SelectedIndex = 2; // Level 3
            }
            else if (_folderId == level4Id)
            {
                cboLevel.SelectedIndex = 3; // Level 4
            }
            else if (_defaultLevel.HasValue && _defaultLevel.Value >= 1 && _defaultLevel.Value <= 4)
            {
                cboLevel.SelectedIndex = _defaultLevel.Value - 1;
            }
            else
            {
                cboLevel.SelectedIndex = 3; // Default to Level 4
            }

            lblFolderValue.Text = _currentFolderName;

            // Initialize Grid
            dgvFiles.AutoGenerateColumns = false;
            dgvFiles.AllowUserToAddRows = false;
            dgvFiles.AllowUserToDeleteRows = true;
            dgvFiles.ReadOnly = true;
            dgvFiles.RowHeadersVisible = false;
            dgvFiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFiles.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 250, 253);

            var colNum = new DataGridViewTextBoxColumn
            {
                Name = "colNum",
                HeaderText = "#",
                Width = 35,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            var colFilename = new DataGridViewTextBoxColumn
            {
                Name = "colFilename",
                HeaderText = "ชื่อไฟล์",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            var colSize = new DataGridViewTextBoxColumn
            {
                Name = "colSize",
                HeaderText = "ขนาด",
                Width = 80,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            var colLevel = new DataGridViewTextBoxColumn
            {
                Name = "colLevel",
                HeaderText = "ระดับความลับ",
                Width = 100,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            var colStatus = new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "สถานะ",
                Width = 120,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };

            dgvFiles.Columns.AddRange(new DataGridViewColumn[] { colNum, colFilename, colSize, colLevel, colStatus });

            pnlDragDrop.AllowDrop = true;
            pnlDragDrop.BackColor = System.Drawing.Color.FromArgb(235, 243, 252);
            pnlDragDrop.DragLeave += new System.EventHandler(this.PnlDragDrop_DragLeave);

            ValidatePermission();
        }

        private void CboLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedLevel = cboLevel.SelectedIndex + 1;

            UpdateTargetFolderForLevel(SelectedLevel);

            // Update level column for all files
            foreach (DataGridViewRow row in dgvFiles.Rows)
            {
                row.Cells["colLevel"].Value = $"Level {SelectedLevel}";
            }

            ValidatePermission();
        }

        private void UpdateTargetFolderForLevel(int level)
        {
            int folderId = GetConfiguredFolderId(level, GetFallbackFolderId(level));
            string folderName = "";

            switch (level)
            {
                case 1:
                    folderName = $"1HC - (High Confidential) [1HC] (ID: {folderId})";
                    break;
                case 2:
                    folderName = $"2SC - (Semi Confidential) [2SC] (ID: {folderId})";
                    break;
                case 3:
                    folderName = $"3NC - (Normal Confidential) [3NC] (ID: {folderId})";
                    break;
                case 4:
                    folderName = $"4PC - (Public Confidential) [4PC] (ID: {folderId})";
                    break;
                default:
                    folderName = $"4PC - (Public Confidential) [4PC] (ID: {folderId})";
                    break;
            }

            _currentFolderId = folderId;
            _currentFolderName = folderName;
            lblFolderValue.Text = _currentFolderName;
        }

        private int GetConfiguredFolderId(int level, int fallbackId)
        {
            string configVal = System.Configuration.ConfigurationManager.AppSettings[$"ExternalUpload.FolderId.Level{level}"];
            if (!string.IsNullOrEmpty(configVal) && int.TryParse(configVal, out int parsedId))
            {
                return parsedId;
            }
            return fallbackId;
        }

        private static int GetFallbackFolderId(int level)
        {
            switch (level)
            {
                case 1: return 434;
                case 2: return 435;
                case 3: return 436;
                case 4: return 437;
                default: return 437;
            }
        }

        private void ValidatePermission()
        {
            if (cboLevel.SelectedIndex < 0) return;

            int level = cboLevel.SelectedIndex + 1;
            bool canAdd = _user.CanAddToFolder(level);

            if (!canAdd)
            {
                lblStatus.Text = $"คุณไม่มีสิทธิ์ในการอัปโหลดไฟล์สำหรับ {cboLevel.SelectedItem}";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                btnUpload.Enabled = false;
            }
            else
            {
                lblStatus.Text = "พร้อมอัปโหลด";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
                btnUpload.Enabled = _filePaths.Count > 0;
            }
        }

        private void PnlDragDrop_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "เลือกไฟล์สำหรับอัปโหลด";
                dialog.Multiselect = true;
                dialog.Filter = "All Files (*.*)|*.*";

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    AddFilesToList(dialog.FileNames);
                }
            }
        }

        private void PnlDragDrop_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                pnlDragDrop.BackColor = System.Drawing.Color.FromArgb(210, 229, 250); // highlighted soft blue
                pnlDragDrop.Invalidate();
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void PnlDragDrop_DragLeave(object sender, EventArgs e)
        {
            pnlDragDrop.BackColor = System.Drawing.Color.FromArgb(235, 243, 252); // default soft blue
            pnlDragDrop.Invalidate();
        }

        private void PnlDragDrop_DragDrop(object sender, DragEventArgs e)
        {
            pnlDragDrop.BackColor = System.Drawing.Color.FromArgb(235, 243, 252); // default soft blue
            pnlDragDrop.Invalidate();

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                var fileList = files.Where(File.Exists).ToArray();
                if (fileList.Length > 0)
                {
                    AddFilesToList(fileList);
                }
            }
        }

        private void PnlDragDrop_Paint(object sender, PaintEventArgs e)
        {
            var borderColor = pnlDragDrop.BackColor == System.Drawing.Color.FromArgb(210, 229, 250)
                ? System.Drawing.Color.FromArgb(31, 117, 203)
                : System.Drawing.Color.FromArgb(127, 178, 229);

            using (var pen = new System.Drawing.Pen(borderColor, 1.5f))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.DrawRectangle(pen, 0, 0, pnlDragDrop.Width - 1, pnlDragDrop.Height - 1);
            }
        }

        private void AddFilesToList(string[] filePaths)
        {
            foreach (var path in filePaths)
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) continue;
                if (_filePaths.Contains(path)) continue; // avoid duplicates in list

                _filePaths.Add(path);

                var fileInfo = new FileInfo(path);
                int newIndex = dgvFiles.Rows.Count + 1;
                int rowIndex = dgvFiles.Rows.Add(
                    newIndex.ToString(),
                    fileInfo.Name,
                    FormatFileSize(fileInfo.Length),
                    $"Level {SelectedLevel}",
                    "Pending"
                );

                dgvFiles.Rows[rowIndex].Tag = path;
            }

            ValidatePermission();
        }

        private void BtnUpload_Click(object sender, EventArgs e)
        {
            if (_filePaths.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกไฟล์ที่ต้องการอัปโหลด", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ── ตรวจสอบไฟล์ซ้ำก่อนเริ่ม upload ──
            var fileNames = _filePaths.Select(Path.GetFileName).ToList();
            var duplicates = _fileUploadController.FindDuplicateNames(fileNames, _currentFolderId, _user).ToList();

            bool overwriteExisting = false;

            if (duplicates.Count > 0)
            {
                DuplicateFileChoiceDialog.Choice choice;
                using (var dialog = new DuplicateFileChoiceDialog(_currentFolderName, duplicates))
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    choice = dialog.SelectedChoice;
                }

                if (choice == DuplicateFileChoiceDialog.Choice.Cancel)
                    return;

                if (choice == DuplicateFileChoiceDialog.Choice.SkipDuplicates)
                {
                    // Filter out duplicates
                    var dupSet = new HashSet<string>(duplicates, StringComparer.OrdinalIgnoreCase);
                    for (int i = dgvFiles.Rows.Count - 1; i >= 0; i--)
                    {
                        var path = dgvFiles.Rows[i].Tag as string;
                        if (path != null && dupSet.Contains(Path.GetFileName(path)))
                        {
                            _filePaths.Remove(path);
                            dgvFiles.Rows.RemoveAt(i);
                        }
                    }

                    // Renumber list
                    for (int i = 0; i < dgvFiles.Rows.Count; i++)
                    {
                        dgvFiles.Rows[i].Cells["colNum"].Value = (i + 1).ToString();
                    }

                    if (_filePaths.Count == 0)
                    {
                        MessageBox.Show("ไม่มีไฟล์ที่จะอัปโหลด (ทุกไฟล์ซ้ำกับไฟล์ในโฟลเดอร์)", "ล้มเหลว", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        ValidatePermission();
                        return;
                    }
                }
                else if (choice == DuplicateFileChoiceDialog.Choice.Replace)
                {
                    overwriteExisting = true;
                }
            }

            // ── บันทึก Checkpoint ก่อนเริ่ม ──
            if (_checkpointController != null)
            {
                foreach (var path in _filePaths)
                {
                    try
                    {
                        _checkpointController.Save(new UploadCheckpoint
                        {
                            FileName = Path.GetFileName(path),
                            FileSizeBytes = new FileInfo(path).Length,
                            FolderID = _currentFolderId,
                            BytesUploaded = 0,
                            CheckpointDate = DateTime.Now
                        });
                    }
                    catch { /* best-effort */ }
                }
            }

            pnlDragDrop.Enabled = false;
            dgvFiles.Enabled = false;
            btnUpload.Enabled = false;
            btnCancel.Enabled = false;
            cboLevel.Enabled = false;
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;
            progressBar.Visible = true;
            lblStatus.Text = "กำลังอัปโหลดไฟล์...";
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);

            _streamsToDispose.Clear();

            var bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.DoWork += (s, ev) =>
            {
                try
                {
                    var requests = new List<FileUploadRequest>();
                    foreach (var path in _filePaths)
                    {
                        var fileInfo = new FileInfo(path);
                        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        _streamsToDispose.Add(stream);

                        requests.Add(new FileUploadRequest
                        {
                            FileName = fileInfo.Name,
                            FileStream = stream,
                            FileSizeBytes = fileInfo.Length,
                            MimeType = MimeMapping(fileInfo.Extension),
                            OverwriteExisting = overwriteExisting,
                            InitialFileStatus = 0 // Draft/Pending
                        });
                    }

                    _fileUploadController.UploadFiles(this, requests, _currentFolderId, _user);
                }
                catch (Exception ex)
                {
                    ev.Result = ex;
                }
            };

            bgWorker.RunWorkerCompleted += (s, ev) =>
            {
                // Dispose streams
                foreach (var stream in _streamsToDispose)
                {
                    try { stream.Dispose(); } catch { }
                }
                _streamsToDispose.Clear();

                progressBar.Visible = false;
                pnlDragDrop.Enabled = true;
                dgvFiles.Enabled = true;
                btnUpload.Enabled = true;
                btnCancel.Enabled = true;
                cboLevel.Enabled = true;

                if (ev.Result is Exception ex)
                {
                    lblStatus.Text = $"เกิดข้อผิดพลาด: {ex.Message}";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    MessageBox.Show($"เกิดข้อผิดพลาดในการอัปโหลด: {ex.Message}", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (UploadedFileId.HasValue)
                {
                    lblStatus.Text = $"อัปโหลดสำเร็จ! จำนวน {UploadedFileIds.Count} ไฟล์";
                    lblStatus.ForeColor = System.Drawing.Color.Green;
                    progressBar.Value = 100;
                    progressBar.Visible = true;

                    MessageBox.Show("อัปโหลดไฟล์สำเร็จเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblStatus.Text = "การอัปโหลดล้มเหลว";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                }
            };

            bgWorker.RunWorkerAsync();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public void PreSelectFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                AddFilesToList(new[] { filePath });
            }
        }

        private static string MimeMapping(string extension)
        {
            if (string.IsNullOrEmpty(extension)) return "application/octet-stream";
            switch (extension.ToLower())
            {
                case ".txt": return "text/plain";
                case ".pdf": return "application/pdf";
                case ".doc": return "application/msword";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls": return "application/vnd.ms-excel";
                case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".zip": return "application/zip";
                default: return "application/octet-stream";
            }
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes >= 1024 * 1024L)
                return string.Format("{0:0.##} MB", (double)bytes / (1024 * 1024));
            if (bytes >= 1024L)
                return string.Format("{0:0.##} KB", (double)bytes / 1024);
            return string.Format("{0} Bytes", bytes);
        }

        // ── IFileUploadView Implementation ──

        public void SetFileStatus(int rowIndex, string status, bool isError)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetFileStatus(rowIndex, status, isError)));
                return;
            }

            if (rowIndex >= 0 && rowIndex < dgvFiles.Rows.Count)
            {
                var row = dgvFiles.Rows[rowIndex];
                row.Cells["colStatus"].Value = status;
                row.Cells["colStatus"].Style.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Blue;
            }
        }

        public void SetFileResult(int rowIndex, UploadResult result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetFileResult(rowIndex, result)));
                return;
            }

            if (rowIndex >= 0 && rowIndex < dgvFiles.Rows.Count)
            {
                var row = dgvFiles.Rows[rowIndex];
                if (result != null && result.Success && result.FileDocumentId.HasValue)
                {
                    row.Cells["colStatus"].Value = "Success";
                    row.Cells["colStatus"].Style.ForeColor = System.Drawing.Color.Green;
                    row.Tag = result.FileDocumentId.Value; // Store the resulting file ID

                    long size = new FileInfo(_filePaths[rowIndex]).Length;
                    string name = result.ActualFileName ?? result.OriginalFileName;

                    UploadedFiles.Add(new UploadedFileDetails
                    {
                        FileId = result.FileDocumentId.Value,
                        FileName = name,
                        FileSize = size
                    });

                    if (UploadedFileId == null)
                    {
                        UploadedFileId = result.FileDocumentId.Value;
                        UploadedFileName = name;
                        UploadedFileSize = size;
                    }
                    UploadedFileIds.Add(result.FileDocumentId.Value);
                }
                else
                {
                    string errMsg = result?.ErrorMessage ?? "Failed";
                    row.Cells["colStatus"].Value = errMsg;
                    row.Cells["colStatus"].Style.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        public void SetProgress(int percent)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetProgress(percent)));
                return;
            }

            if (percent >= 0 && percent <= 100)
            {
                progressBar.Value = percent;
            }
        }

        public void ShowQuotaWarning(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ShowQuotaWarning(message)));
                return;
            }

            lblStatus.Text = "Warning: " + message;
            lblStatus.ForeColor = System.Drawing.Color.Orange;
        }

        public void HideQuotaWarning() { }

        public void ShowError(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ShowError(message)));
                return;
            }

            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ShowInfo(message)));
                return;
            }

            MessageBox.Show(message, "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SetUploadEnabled(bool enabled)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetUploadEnabled(enabled)));
                return;
            }

            btnUpload.Enabled = enabled;
            btnCancel.Enabled = enabled;
            cboLevel.Enabled = enabled;
        }

        public void SetDownloadEnabled(bool enabled) { }

        public int? SelectedFileDocumentId => UploadedFileId;
    }
}
