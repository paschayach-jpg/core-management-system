using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Controllers.DMS.Document;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// MOD-003: File Upload Form.
    /// Folder context is passed directly from FolderForm (no internal TreeView).
    /// Satisfies Requirements 5.1–5.16, 12.5.
    /// </summary>
    public partial class FileUploadForm : Form, IFileUploadView
    {
        // ── Per-row UI controls ───────────────────────────────────────────────
        private class FileRowControls
        {
            public Panel       Row        { get; set; }
            public Label       LblIcon    { get; set; }
            public Label       LblName    { get; set; }
            public Label       LblSize    { get; set; }
            public Label       LblStatus  { get; set; }
            public Label       LblError   { get; set; }
            public ProgressBar Progress   { get; set; }
            public int?        FileDocId  { get; set; }
        }

        private FilePresenter _presenter;

        private int     _folderId;
        private readonly UserContext _user;
        private Folder  _selectedFolder;

        private readonly IFolderService _folderService;
        private readonly IUploadCheckpointService _checkpointService;

        private readonly List<FileUploadRequest>  _pendingFiles = new List<FileUploadRequest>();
        private readonly Dictionary<int, int>     _uploadedIds  = new Dictionary<int, int>();
        private readonly List<FileRowControls>    _fileRows     = new List<FileRowControls>();

        private BackgroundWorker _uploadWorker;
        private int _currentUploadRow = -1;

        // ── Row geometry ─────────────────────────────────────────────────────
        private const int ROW_HEIGHT   = 62;
        private const int ICON_X       = 12;
        private const int NAME_X       = 50;
        private const int RIGHT_MARGIN = 12;

        // ── Constructor ───────────────────────────────────────────────────────

        public FileUploadForm(
            IFileService   fileService,
            IFolderService folderService,
            UserContext    user,
            Folder         targetFolder,
            IUploadCheckpointService checkpointService = null)
        {
            if (fileService   == null) throw new ArgumentNullException("fileService");
            if (folderService == null) throw new ArgumentNullException("folderService");
            if (user          == null) throw new ArgumentNullException("user");
            if (targetFolder  == null) throw new ArgumentNullException("targetFolder");

            _user              = user;
            _folderService     = folderService;
            _selectedFolder    = targetFolder;
            _folderId          = targetFolder.FolderID;
            _checkpointService = checkpointService;

            InitializeComponent();

            _presenter = new FilePresenter(fileService, folderService, this, user);
            BuildUploadWorker();

            // Drag & drop on the upload zone
            pnlUploadZone.AllowDrop  = true;
            pnlUploadZone.DragEnter += PnlUploadZone_DragEnter;
            pnlUploadZone.DragDrop  += PnlUploadZone_DragDrop;

            // Also allow drop on file list area
            pnlFileRows.AllowDrop  = true;
            pnlFileRows.DragEnter += PnlUploadZone_DragEnter;
            pnlFileRows.DragDrop  += PnlUploadZone_DragDrop;
        }

        /// <summary>Parameterless constructor for design-time only.</summary>
        public FileUploadForm()
        {
            InitializeComponent();
        }

        // ── Form load ─────────────────────────────────────────────────────────

        private void FileUploadForm_Load(object sender, EventArgs e)
        {
            // Req 10.6 — purge stale checkpoints before checking for resume
            if (_checkpointService != null)
                _checkpointService.DeleteExpired(TimeSpan.FromHours(24));

            if (_selectedFolder != null)
                ApplyFolderContext(_selectedFolder);
            else
                UpdateCurrentFolderDisplay(null);
        }

        // ── Apply folder context ──────────────────────────────────────────────

        private void ApplyFolderContext(Folder folder)
        {
            _selectedFolder = folder;
            _folderId       = folder.FolderID;

            Text = "อัพโหลดไฟล์";

            UpdateCurrentFolderDisplay(folder);
        }

        private void UpdateCurrentFolderDisplay(Folder folder)
        {
            if (folder != null)
            {
                // Build breadcrumb path
                string badge = GetConfidentialityBadge(folder.ConfidentialityLevel);
                lblFolderPath.Text    = folder.FolderName ?? "";
                lblBadge.Text         = badge ?? "";
                lblBadge.Visible      = true;
                lblBadge.BackColor    = GetConfidentialityColor(folder.ConfidentialityLevel);

                // Position badge just after the path label
                int badgeX = lblFolderPath.Right + 8;
                if (badgeX + lblBadge.Width > pnlFolderPath.Width - 8)
                    badgeX = pnlFolderPath.Width - lblBadge.Width - 8;
                lblBadge.Location = new Point(badgeX, lblBadge.Location.Y);
            }
            else
            {
                lblFolderPath.Text = "— ยังไม่ได้เลือก Folder —" ?? "";
                lblBadge.Visible   = false;
            }
        }

        // ── Public backward-compat method ─────────────────────────────────────

        public void SetDestinationLabel(string folderName)
        {
            Text = "อัพโหลดไฟล์";
        }

        // ── Drop zone paint (dashed border) ───────────────────────────────────

        private void PnlUploadZone_Paint(object sender, PaintEventArgs e)
        {
            var r = new Rectangle(8, 4, pnlUploadZone.Width - 17, pnlUploadZone.Height - 9);
            using (var pen = new Pen(Color.FromArgb(180, 200, 220), 1.5f))
            {
                pen.DashStyle = DashStyle.Dash;
                e.Graphics.DrawRectangle(pen, r);
            }
        }

        // ── Drag & drop ───────────────────────────────────────────────────────

        private void PnlUploadZone_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void PnlUploadZone_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 10) Array.Resize(ref files, 10);
            LoadFilesIntoList(files);
        }

        // ── BackgroundWorker ──────────────────────────────────────────────────

        private void BuildUploadWorker()
        {
            _uploadWorker = new BackgroundWorker
            {
                WorkerReportsProgress      = true,
                WorkerSupportsCancellation = false
            };
            _uploadWorker.DoWork             += UploadWorker_DoWork;
            _uploadWorker.ProgressChanged    += UploadWorker_ProgressChanged;
            _uploadWorker.RunWorkerCompleted += UploadWorker_Completed;
        }

        private void UploadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _presenter.UploadFiles((List<FileUploadRequest>)e.Argument, _folderId);
        }

        private void UploadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = Math.Min(e.ProgressPercentage, 100);
            if (_currentUploadRow >= 0 && _currentUploadRow < _fileRows.Count)
            {
                var pb = _fileRows[_currentUploadRow].Progress;
                if (pb.Style == ProgressBarStyle.Continuous)
                    pb.Value = Math.Min(e.ProgressPercentage, 100);
            }
        }

        private void UploadWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null) 
            {
                ShowError("Unexpected upload error: " + e.Error.Message);
                btnPause.Enabled = false;
                UpdateSummary();
                return;
            }
            
            btnPause.Enabled = false;
            UpdateSummary();
            
            // ตรวจสอบว่าอัปโหลดสำเร็จทั้งหมดหรือไม่
            int successCount = _fileRows.Count(r => r.LblStatus.Text == "สำเร็จ");
            int totalCount = _fileRows.Count;
            
            if (successCount > 0)
            {
                // แสดงข้อความสำเร็จสั้นๆ
                string message = successCount == totalCount 
                    ? $"อัปโหลดสำเร็จ {successCount} ไฟล์"
                    : $"อัปโหลดสำเร็จ {successCount}/{totalCount} ไฟล์";
                
                // แสดงข้อความสำเร็จแล้วปิดหน้าต่างอัตโนมัติ
                MessageBox.Show(message, "อัปโหลดเสร็จสิ้น", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // ปิดหน้าต่างและ reload หน้าหลัก
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        // ── File picker ───────────────────────────────────────────────────────

        private void btnPickFiles_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title       = "เลือกไฟล์ที่ต้องการอัพโหลด (สูงสุด 10 ไฟล์)";
                dlg.Multiselect = true;
                dlg.Filter      =
                    "Permitted files|*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx;" +
                    "*.jpg;*.png;*.gif;*.zip|All files|*.*";

                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                string[] paths = dlg.FileNames;
                if (paths.Length > 10)
                {
                    MessageBox.Show("เลือกได้สูงสุด 10 ไฟล์ต่อครั้ง", "ไฟล์มากเกินไป",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Array.Resize(ref paths, 10);
                }
                LoadFilesIntoList(paths);
            }
        }

        // ── Upload trigger (auto-start after file selection) ──────────────────

        private void btnUpload_Click(object sender, EventArgs e)
        {
            StartUpload();
        }

        private void StartUpload()
        {
            if (_folderId <= 0 || _selectedFolder == null) { ShowError("ไม่พบข้อมูล Folder ปลายทาง"); return; }
            if (_pendingFiles.Count == 0)                  { ShowError("กรุณาเลือกไฟล์ที่ต้องการอัพโหลด"); return; }
            if (_presenter == null)                        { ShowError("บริการอัพโหลดไม่พร้อม"); return; }

            // Req 10.1 — save checkpoint for each file before starting
            if (_checkpointService != null)
            {
                foreach (var req in _pendingFiles)
                {
                    long offset = (req.FileStream != null && req.FileStream != Stream.Null && req.FileStream.CanSeek)
                        ? req.FileStream.Position
                        : 0L;
                    try
                    {
                        _checkpointService.Save(new UploadCheckpoint
                        {
                            FileName       = req.FileName,
                            FileSizeBytes  = req.FileSizeBytes,
                            FolderID       = _folderId,
                            BytesUploaded  = offset,
                            CheckpointDate = DateTime.Now
                        });
                    }
                    catch { /* best-effort */ }
                }
            }

            _uploadedIds.Clear();
            _currentUploadRow = -1;
            btnPause.Enabled  = true;

            if (!_uploadWorker.IsBusy)
                _uploadWorker.RunWorkerAsync(new List<FileUploadRequest>(_pendingFiles));
        }

        // ── Download (kept for compat) ─────────────────────────────────────────

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (_presenter == null) { ShowError("Download service is not available."); return; }

            int? docId = SelectedFileDocumentId;
            if (!docId.HasValue) { ShowError("กรุณาเลือกไฟล์ที่อัพโหลดสำเร็จก่อนดาวน์โหลด"); return; }

            _presenter.DownloadFile(docId.Value, suggestedName =>
            {
                using (var dlg = new SaveFileDialog())
                {
                    dlg.FileName = suggestedName;
                    dlg.Filter   = "All files|*.*";
                    return dlg.ShowDialog(this) == DialogResult.OK ? dlg.FileName : null;
                }
            });
        }

        // ── Cancel all ────────────────────────────────────────────────────────

        private void btnCancelAll_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ── IFileUploadView ───────────────────────────────────────────────────

        public void SetFileStatus(int rowIndex, string status, bool isError)
        {
            if (InvokeRequired) { Invoke(new Action<int, string, bool>(SetFileStatus), rowIndex, status, isError); return; }
            if (rowIndex < 0 || rowIndex >= _fileRows.Count) return;

            var row = _fileRows[rowIndex];
            _currentUploadRow = rowIndex;

            if (isError)
            {
                row.LblIcon.Text      = "❌";
                row.LblIcon.ForeColor = Color.FromArgb(192, 0, 0);
                row.LblStatus.Text    = "Error";
                row.LblStatus.ForeColor = Color.FromArgb(192, 0, 0);
                row.LblError.Text     = status;
                row.LblError.Visible  = true;
                row.Progress.Style    = ProgressBarStyle.Continuous;
                row.Progress.Value    = 0;
                row.Row.BackColor     = Color.FromArgb(255, 248, 248);
            }
            else
            {
                row.LblIcon.Text      = "🔄";
                row.LblStatus.Text    = status;
                row.LblStatus.ForeColor = Color.FromArgb(43, 87, 154);
                row.Progress.Style    = ProgressBarStyle.Marquee;
                row.Progress.MarqueeAnimationSpeed = 30;
            }
        }

        public void SetFileResult(int rowIndex, UploadResult result)
        {
            if (InvokeRequired) { Invoke(new Action<int, UploadResult>(SetFileResult), rowIndex, result); return; }
            if (rowIndex < 0 || rowIndex >= _fileRows.Count) return;

            var row = _fileRows[rowIndex];

            if (result.Success)
            {
                row.LblIcon.Text       = "✅";
                row.LblIcon.ForeColor  = Color.FromArgb(0, 140, 0);
                row.LblStatus.Text     = "สำเร็จ";
                row.LblStatus.ForeColor = Color.FromArgb(0, 140, 0);
                row.LblError.Visible   = false;
                row.Row.BackColor      = Color.White;
                row.Progress.Style     = ProgressBarStyle.Continuous;
                row.Progress.Value     = 100;
                if (result.FileDocumentId.HasValue)
                {
                    row.FileDocId          = result.FileDocumentId.Value;
                    _uploadedIds[rowIndex] = result.FileDocumentId.Value;
                }

                // Req 10.5 — delete checkpoint on successful upload
                if (_checkpointService != null && rowIndex < _pendingFiles.Count)
                {
                    try { _checkpointService.Delete(_folderId, _pendingFiles[rowIndex].FileName); } catch { }
                }
            }
            else
            {
                row.LblIcon.Text       = "❌";
                row.LblIcon.ForeColor  = Color.FromArgb(192, 0, 0);
                row.LblStatus.Text     = "Error";
                row.LblStatus.ForeColor = Color.FromArgb(192, 0, 0);
                row.LblError.Text      = "⚠ " + result.ErrorMessage;
                row.LblError.Visible   = true;
                row.Row.BackColor      = Color.FromArgb(255, 248, 248);
                row.Progress.Style     = ProgressBarStyle.Continuous;
                row.Progress.Value     = 0;
            }
            UpdateSummary();
        }

        public void SetProgress(int percent)
        {
            if (InvokeRequired) { Invoke(new Action<int>(SetProgress), percent); return; }
            progressBar.Value    = Math.Min(Math.Max(percent, 0), 100);
            lblProgressText.Text = percent > 0 && percent < 100
                ? string.Format("กำลังอัพโหลด... {0}%", percent)
                : percent >= 100 ? "อัพโหลดเสร็จสิ้น" : string.Empty;

            if (_currentUploadRow >= 0 && _currentUploadRow < _fileRows.Count)
            {
                var pb = _fileRows[_currentUploadRow].Progress;
                if (pb.Style == ProgressBarStyle.Continuous)
                    pb.Value = Math.Min(percent, 100);
            }
        }

        public void ShowQuotaWarning(string message)
        {
            if (InvokeRequired) { Invoke(new Action<string>(ShowQuotaWarning), message); return; }
            lblQuotaWarning.Text    = message;
            lblQuotaWarning.Visible = true;
        }

        public void HideQuotaWarning()
        {
            if (InvokeRequired) { Invoke(new Action(HideQuotaWarning)); return; }
            lblQuotaWarning.Visible = false;
        }

        public void ShowError(string message)
        {
            if (InvokeRequired) { Invoke(new Action<string>(ShowError), message); return; }
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowInfo(string message)
        {
            if (InvokeRequired) { Invoke(new Action<string>(ShowInfo), message); return; }
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SetUploadEnabled(bool enabled)
        {
            if (InvokeRequired) { Invoke(new Action<bool>(SetUploadEnabled), enabled); return; }
            // No visible upload button; nothing to do
        }

        public void SetDownloadEnabled(bool enabled)
        {
            if (InvokeRequired) { Invoke(new Action<bool>(SetDownloadEnabled), enabled); return; }
            btnDownload.Enabled = enabled;
        }

        public int? SelectedFileDocumentId
        {
            get
            {
                // Return the first completed row's doc id
                foreach (var r in _fileRows)
                    if (r.FileDocId.HasValue) return r.FileDocId;
                return null;
            }
        }

        // ── Build per-row panels ──────────────────────────────────────────────

        private void LoadFilesIntoList(string[] paths)
        {
            // Dispose old streams
            foreach (var req in _pendingFiles)
                if (req.FileStream != null && req.FileStream != Stream.Null)
                    try { req.FileStream.Dispose(); } catch { }

            _pendingFiles.Clear();
            _uploadedIds.Clear();
            _fileRows.Clear();
            _currentUploadRow = -1;
            lblSummary.Text   = "";

            // Remove old row panels
            pnlFileRows.Controls.Clear();

            int y = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                var info    = new FileInfo(paths[i]);
                var request = new FileUploadRequest
                {
                    FileName      = info.Name,
                    FileSizeBytes = info.Length,
                    FileStream    = null
                };
                _pendingFiles.Add(request);

                var rc = BuildFileRow(i, info.Name, info.Length);
                rc.Row.Location = new Point(0, y);
                rc.Row.Width    = pnlFileRows.ClientSize.Width;
                rc.Row.Anchor   = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                pnlFileRows.Controls.Add(rc.Row);
                _fileRows.Add(rc);
                y += ROW_HEIGHT;
            }

            // Open streams
            for (int i = 0; i < _pendingFiles.Count; i++)
            {
                string fullPath = paths[i];
                try
                {
                    _pendingFiles[i].FileStream = new FileStream(
                        fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                catch (Exception ex)
                {
                    _fileRows[i].LblIcon.Text        = "❌";
                    _fileRows[i].LblIcon.ForeColor   = Color.FromArgb(192, 0, 0);
                    _fileRows[i].LblStatus.Text      = "Error";
                    _fileRows[i].LblStatus.ForeColor = Color.FromArgb(192, 0, 0);
                    _fileRows[i].LblError.Text       = "⚠ Cannot open: " + ex.Message;
                    _fileRows[i].LblError.Visible    = true;
                    _fileRows[i].Row.BackColor       = Color.FromArgb(255, 248, 248);
                    _pendingFiles[i].FileStream      = Stream.Null;
                }
            }

            UpdateSummary();

            // Req 10.2 — check for resume checkpoint after files are selected
            if (_checkpointService != null && _pendingFiles.Count > 0 && _selectedFolder != null)
                CheckAndOfferResume(paths);

            // Auto-start upload if folder is ready
            if (_pendingFiles.Count > 0 && _selectedFolder != null)
                StartUpload();
        }

        /// <summary>
        /// Req 10.2, 10.3, 10.4 — For each pending file, check if a checkpoint exists.
        /// If found, ask user whether to resume. Seek stream to BytesUploaded if resuming,
        /// or delete checkpoint if not.
        /// </summary>
        private void CheckAndOfferResume(string[] paths)
        {
            for (int i = 0; i < _pendingFiles.Count; i++)
            {
                var req = _pendingFiles[i];
                UploadCheckpoint cp = null;
                try { cp = _checkpointService.Load(_folderId, req.FileName); }
                catch { /* ignore — treat as no checkpoint */ }

                if (cp == null || cp.BytesUploaded <= 0) continue;

                var answer = MessageBox.Show(
                    string.Format(
                        "พบการอัพโหลดที่ค้างไว้สำหรับไฟล์ \"{0}\"\n" +
                        "อัพโหลดไปแล้ว: {1} / {2}\n\n" +
                        "ต้องการอัพโหลดต่อจากจุดที่ค้างไว้หรือไม่?",
                        req.FileName,
                        FormatBytes(cp.BytesUploaded),
                        FormatBytes(cp.FileSizeBytes)),
                    "Resume Upload",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (answer == DialogResult.Yes)
                {
                    // Req 10.3 — seek stream to BytesUploaded
                    if (_pendingFiles[i].FileStream != null &&
                        _pendingFiles[i].FileStream != Stream.Null &&
                        _pendingFiles[i].FileStream.CanSeek &&
                        cp.BytesUploaded < _pendingFiles[i].FileStream.Length)
                    {
                        _pendingFiles[i].FileStream.Seek(cp.BytesUploaded, SeekOrigin.Begin);
                    }
                }
                else
                {
                    // Req 10.4 — delete checkpoint and start fresh
                    try { _checkpointService.Delete(_folderId, req.FileName); } catch { }
                }
            }
        }

        private FileRowControls BuildFileRow(int index, string fileName, long sizeBytes)
        {
            var rc = new FileRowControls();

            // Row panel
            rc.Row           = new Panel();
            rc.Row.Size      = new Size(pnlFileRows.ClientSize.Width, ROW_HEIGHT);
            rc.Row.BackColor = Color.White;
            rc.Row.Tag       = rc;   // store self-reference for context menu

            // Click to select row (by reference, not index — safe after deletion)
            rc.Row.Click += (s, e) => SelectRow(rc);

            // Right-click context menu
            var ctx       = new ContextMenuStrip();
            var mnuDelete = new ToolStripMenuItem("🗑  ลบไฟล์", null, (s, e) => RemoveFileRow(rc));
            ctx.Opening  += (s, e) => { mnuDelete.Enabled = !_uploadWorker.IsBusy; };
            ctx.Items.Add(mnuDelete);
            rc.Row.ContextMenuStrip = ctx;
            rc.Row.MouseDown += (s, e) => { if (e.Button == MouseButtons.Right) SelectRow(rc); };

            // Icon label
            rc.LblIcon           = new Label();
            rc.LblIcon.Text      = "📄";
            rc.LblIcon.Size      = new Size(28, 28);
            rc.LblIcon.Location  = new Point(ICON_X, (ROW_HEIGHT - 28) / 2 - 4);
            rc.LblIcon.TextAlign = ContentAlignment.MiddleCenter;
            rc.LblIcon.Font      = new Font("Segoe UI Emoji", 14f);
            rc.LblIcon.BackColor = Color.Transparent;

            // File name label
            rc.LblName           = new Label();
            rc.LblName.Text      = fileName;
            rc.LblName.AutoSize  = false;
            rc.LblName.Size      = new Size(320, 20);
            rc.LblName.Location  = new Point(NAME_X, 9);
            rc.LblName.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            rc.LblName.ForeColor = Color.FromArgb(30, 30, 30);
            rc.LblName.TextAlign = ContentAlignment.MiddleLeft;

            // Error label (hidden by default)
            rc.LblError           = new Label();
            rc.LblError.Text      = "";
            rc.LblError.AutoSize  = false;
            rc.LblError.Size      = new Size(320, 18);
            rc.LblError.Location  = new Point(NAME_X, 30);
            rc.LblError.Font      = new Font("Segoe UI", 8f);
            rc.LblError.ForeColor = Color.FromArgb(180, 80, 0);
            rc.LblError.Visible   = false;

            // Size label (right side, top)
            rc.LblSize           = new Label();
            rc.LblSize.Text      = FormatBytes(sizeBytes);
            rc.LblSize.AutoSize  = false;
            rc.LblSize.Size      = new Size(80, 20);
            rc.LblSize.Location  = new Point(pnlFileRows.ClientSize.Width - 80 - RIGHT_MARGIN - 80, 9);
            rc.LblSize.Anchor    = AnchorStyles.Top | AnchorStyles.Right;
            rc.LblSize.Font      = new Font("Segoe UI", 8.5f);
            rc.LblSize.ForeColor = Color.FromArgb(100, 100, 100);
            rc.LblSize.TextAlign = ContentAlignment.MiddleRight;

            // Status label (right side, bottom)
            rc.LblStatus           = new Label();
            rc.LblStatus.Text      = "รอดำเนินการ...";
            rc.LblStatus.AutoSize  = false;
            rc.LblStatus.Size      = new Size(160, 20);
            rc.LblStatus.Location  = new Point(pnlFileRows.ClientSize.Width - 160 - RIGHT_MARGIN, 30);
            rc.LblStatus.Anchor    = AnchorStyles.Top | AnchorStyles.Right;
            rc.LblStatus.Font      = new Font("Segoe UI", 8.5f);
            rc.LblStatus.ForeColor = Color.FromArgb(120, 120, 120);
            rc.LblStatus.TextAlign = ContentAlignment.MiddleRight;

            // Progress bar (bottom of row)
            rc.Progress           = new ProgressBar();
            rc.Progress.Dock      = DockStyle.Bottom;
            rc.Progress.Height    = 4;
            rc.Progress.Style     = ProgressBarStyle.Continuous;
            rc.Progress.Value     = 0;
            rc.Progress.Maximum   = 100;
            rc.Progress.ForeColor = Color.FromArgb(43, 87, 154);

            // Separator line
            var sep           = new Panel();
            sep.Dock          = DockStyle.Bottom;
            sep.Height        = 1;
            sep.BackColor     = Color.FromArgb(230, 235, 240);

            rc.Row.Controls.Add(rc.LblIcon);
            rc.Row.Controls.Add(rc.LblName);
            rc.Row.Controls.Add(rc.LblError);
            rc.Row.Controls.Add(rc.LblSize);
            rc.Row.Controls.Add(rc.LblStatus);
            rc.Row.Controls.Add(rc.Progress);
            rc.Row.Controls.Add(sep);

            // Propagate context menu and right-click selection to all child labels
            foreach (Control child in rc.Row.Controls)
            {
                var lbl = child as Label;
                if (lbl == null) continue;
                lbl.ContextMenuStrip = ctx;
                lbl.Click    += (s, e) => SelectRow(rc);
                lbl.MouseDown += (s, e) => { if (e.Button == MouseButtons.Right) SelectRow(rc); };
            }

            return rc;
        }

        private void SelectRow(FileRowControls target)
        {
            foreach (var r in _fileRows)
                r.Row.BackColor = (r == target)
                    ? Color.FromArgb(232, 240, 254)
                    : (r.LblStatus.Text == "Error"
                        ? Color.FromArgb(255, 248, 248) : Color.White);
        }

        private void RemoveFileRow(FileRowControls rc)
        {
            if (_uploadWorker != null && _uploadWorker.IsBusy)
            {
                ShowInfo("ไม่สามารถลบไฟล์ขณะกำลังอัพโหลด กรุณารอให้อัพโหลดเสร็จก่อน");
                return;
            }

            int idx = _fileRows.IndexOf(rc);
            if (idx < 0) return;

            // Dispose stream
            if (idx < _pendingFiles.Count)
            {
                var req = _pendingFiles[idx];
                if (req.FileStream != null && req.FileStream != Stream.Null)
                    try { req.FileStream.Dispose(); } catch { }
                _pendingFiles.RemoveAt(idx);
            }

            // Re-key _uploadedIds (shift keys > idx down by 1)
            var newIds = new Dictionary<int, int>();
            foreach (var kv in _uploadedIds)
            {
                if (kv.Key == idx) continue;
                newIds[kv.Key > idx ? kv.Key - 1 : kv.Key] = kv.Value;
            }
            _uploadedIds.Clear();
            foreach (var kv in newIds) _uploadedIds[kv.Key] = kv.Value;

            _fileRows.RemoveAt(idx);
            pnlFileRows.Controls.Remove(rc.Row);
            rc.Row.Dispose();

            // Re-layout remaining rows
            for (int i = 0; i < _fileRows.Count; i++)
                _fileRows[i].Row.Location = new Point(0, i * ROW_HEIGHT);

            UpdateSummary();
        }

        // ── Summary label ─────────────────────────────────────────────────────

        private void UpdateSummary()
        {
            if (_fileRows.Count == 0) { lblSummary.Text = ""; return; }
            int success = _fileRows.Count(r => r.LblStatus.Text == "สำเร็จ");
            int errors  = _fileRows.Count(r => r.LblStatus.Text == "Error");
            string text = string.Format("รวม: {0}/{1} ไฟล์ สำเร็จ", success, _fileRows.Count);
            if (errors > 0) text += string.Format(" | {0} Error", errors);
            lblSummary.Text = text;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private string GetConfidentialityBadge(int level)
        {
            switch (level)
            {
                case 1: return "1HC"; case 2: return "2SC";
                case 3: return "3NC"; case 4: return "4PC";
                default: return level.ToString();
            }
        }

        private Color GetConfidentialityColor(int level)
        {
            switch (level)
            {
                case 1: return Color.DarkRed;
                case 2: return Color.OrangeRed;
                case 3: return Color.FromArgb(43, 87, 154);
                case 4: return Color.DarkGreen;
                default: return Color.Gray;
            }
        }

        private string GetConfidentialityLabel(int level)
        {
            switch (level)
            {
                case 1: return "🔴 1HC - Highly Confidential";
                case 2: return "🟠 2SC - Secret";
                case 3: return "🔵 3NC - Non-Confidential";
                case 4: return "🟢 4PC - Public/Common";
                default: return level.ToString();
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1024L * 1024) return string.Format("{0:N1} MB", bytes / (1024.0 * 1024));
            if (bytes >= 1024)         return string.Format("{0:N1} KB", bytes / 1024.0);
            return string.Format("{0} B", bytes);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            foreach (var req in _pendingFiles)
            {
                if (req.FileStream != null && req.FileStream != Stream.Null)
                    try { req.FileStream.Dispose(); } catch { }
            }
        }
    }
}
