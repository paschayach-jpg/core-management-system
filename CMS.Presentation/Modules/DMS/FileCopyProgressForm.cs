using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    internal struct FileCopyInput
    {
        public int FileId;
        public string FileName;
        public bool Overwrite;
    }

    internal sealed class FileCopyProgressForm : Form
    {
        private struct FileCopyItem
        {
            public int FileId;
            public string FileName;
            public bool Overwrite;
        }

        private enum CopyStatus { Waiting, Copying, Success, Error }

        private sealed class CopyProgress
        {
            public int Index;
            public CopyStatus Status;
            public string ErrorMessage;
        }

        private readonly List<FileCopyItem> _files;
        private readonly int _destinationFolderId;
        private readonly IFileCommandController _controller;
        private readonly UserContext _user;

        private int _successCount;
        private int _failCount;

        // UI
        private Label _lblHeader;
        private Label _lblSubHeader;
        private ProgressBar _progressBar;
        private Label _lblProgress;
        private Label _lblSummary;
        private Button _btnClose;
        private readonly List<RowControls> _rows = new List<RowControls>();

        private struct RowControls
        {
            public Label Icon;
            public Label Status;
        }

        public FileCopyProgressForm(
            IEnumerable<FileCopyInput> files,
            int destinationFolderId,
            IFileCommandController controller,
            UserContext user)
        {
            _files = files.Select(f => new FileCopyItem { FileId = f.FileId, FileName = f.FileName, Overwrite = f.Overwrite }).ToList();
            _destinationFolderId = destinationFolderId;
            _controller = controller;
            _user = user;
            BuildUI();
        }

        private void BuildUI()
        {
            Text = "คัดลอกไฟล์";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.White;

            int listHeight = Math.Min(_files.Count * 38 + 16, 280);
            ClientSize = new Size(560, 60 + listHeight + 68);

            // ── Header ────────────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(31, 56, 100)
            };
            _lblHeader = new Label
            {
                Text = string.Format("คัดลอก {0} ไฟล์", _files.Count),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 8)
            };
            _lblSubHeader = new Label
            {
                Text = "กำลังเตรียมการ...",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(180, 210, 255),
                AutoSize = true,
                Location = new Point(16, 36)
            };
            pnlHeader.Controls.Add(_lblHeader);
            pnlHeader.Controls.Add(_lblSubHeader);

            // ── File list ─────────────────────────────────────────────────────
            var pnlFileList = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(0, 4, 0, 4)
            };

            int y = 8;
            foreach (var file in _files)
            {
                var pnlRow = new Panel
                {
                    Location = new Point(12, y),
                    Size = new Size(524, 34),
                    BackColor = Color.White
                };
                var lblIcon = new Label
                {
                    Text = "📄",
                    Location = new Point(0, 5),
                    Size = new Size(28, 22),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                var lblName = new Label
                {
                    Text = file.FileName,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(30, 30, 30),
                    Location = new Point(32, 5),
                    Size = new Size(350, 22),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                var lblStatus = new Label
                {
                    Text = "รอดำเนินการ",
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Location = new Point(386, 5),
                    Size = new Size(130, 22),
                    TextAlign = ContentAlignment.MiddleRight
                };
                pnlRow.Controls.Add(lblIcon);
                pnlRow.Controls.Add(lblName);
                pnlRow.Controls.Add(lblStatus);
                pnlFileList.Controls.Add(pnlRow);
                _rows.Add(new RowControls { Icon = lblIcon, Status = lblStatus });

                var sep = new Panel
                {
                    Location = new Point(12, y + 34),
                    Size = new Size(524, 1),
                    BackColor = Color.FromArgb(240, 242, 245)
                };
                pnlFileList.Controls.Add(sep);
                y += 38;
            }

            // ── Footer ────────────────────────────────────────────────────────
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 68,
                BackColor = Color.FromArgb(248, 249, 252)
            };
            pnlFooter.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(229, 231, 235)))
                    e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
            };

            _progressBar = new ProgressBar
            {
                Location = new Point(16, 10),
                Size = new Size(340, 14),
                Minimum = 0,
                Maximum = _files.Count,
                Style = ProgressBarStyle.Continuous
            };
            _lblProgress = new Label
            {
                Text = string.Format("0 / {0}", _files.Count),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new Point(362, 8),
                AutoSize = true
            };
            _lblSummary = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new Point(16, 30),
                AutoSize = true
            };
            _btnClose = new Button
            {
                Text = "กำลังดำเนินการ...",
                Enabled = false,
                Size = new Size(110, 30),
                Location = new Point(434, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.Click += (s, e) => Close();

            pnlFooter.Controls.Add(_progressBar);
            pnlFooter.Controls.Add(_lblProgress);
            pnlFooter.Controls.Add(_lblSummary);
            pnlFooter.Controls.Add(_btnClose);

            Controls.Add(pnlFileList);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartCopy();
        }

        private void StartCopy()
        {
            _lblSubHeader.Text = "กำลังคัดลอก...";

            var worker = new BackgroundWorker { WorkerReportsProgress = true };
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_Completed;
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;

            for (int i = 0; i < _files.Count; i++)
            {
                var file = _files[i];
                worker.ReportProgress(0, new CopyProgress { Index = i, Status = CopyStatus.Copying });

                var result = new CopyProgress { Index = i };
                try
                {
                    _controller.CopyFile(file.FileId, _destinationFolderId, _user, file.Overwrite);
                    result.Status = CopyStatus.Success;
                }
                catch (Exception ex)
                {
                    result.Status = CopyStatus.Error;
                    result.ErrorMessage = ex.Message;
                }

                worker.ReportProgress(0, result);
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var p = (CopyProgress)e.UserState;
            var row = _rows[p.Index];

            switch (p.Status)
            {
                case CopyStatus.Copying:
                    row.Icon.Text = "🔄";
                    row.Status.Text = "กำลังคัดลอก...";
                    row.Status.ForeColor = Color.FromArgb(37, 99, 235);
                    break;

                case CopyStatus.Success:
                    row.Icon.Text = "✅";
                    row.Status.Text = "สำเร็จ";
                    row.Status.ForeColor = Color.FromArgb(21, 128, 61);
                    _successCount++;
                    _progressBar.Value = Math.Min(_successCount + _failCount, _files.Count);
                    _lblProgress.Text = string.Format("{0} / {1}", _progressBar.Value, _files.Count);
                    break;

                case CopyStatus.Error:
                    row.Icon.Text = "❌";
                    row.Status.Text = "ล้มเหลว";
                    row.Status.ForeColor = Color.FromArgb(185, 28, 28);
                    _failCount++;
                    _progressBar.Value = Math.Min(_successCount + _failCount, _files.Count);
                    _lblProgress.Text = string.Format("{0} / {1}", _progressBar.Value, _files.Count);
                    break;
            }
        }

        private void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            _progressBar.Value = _files.Count;
            _lblProgress.Text = string.Format("{0} / {1}", _files.Count, _files.Count);

            if (_failCount == 0)
            {
                _lblSubHeader.Text = "เสร็จสิ้น";
                _lblSubHeader.ForeColor = Color.FromArgb(134, 239, 172);
                _lblSummary.Text = string.Format("คัดลอกสำเร็จทั้งหมด {0} ไฟล์", _successCount);
                _lblSummary.ForeColor = Color.FromArgb(21, 128, 61);
                _btnClose.BackColor = Color.FromArgb(21, 128, 61);
                DialogResult = DialogResult.OK;
            }
            else
            {
                _lblSubHeader.Text = "เสร็จสิ้น (มีข้อผิดพลาด)";
                _lblSubHeader.ForeColor = Color.FromArgb(254, 202, 202);
                _lblSummary.Text = string.Format("สำเร็จ {0} ไฟล์  •  ล้มเหลว {1} ไฟล์", _successCount, _failCount);
                _lblSummary.ForeColor = Color.FromArgb(185, 28, 28);
                _btnClose.BackColor = Color.FromArgb(107, 114, 128);
                DialogResult = DialogResult.Cancel;
            }

            _btnClose.Text = "ปิด";
            _btnClose.Enabled = true;
        }
    }
}
