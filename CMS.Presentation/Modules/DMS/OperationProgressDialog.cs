using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.DMS
{
    public sealed class OperationProgressItem
    {
        public string Title { get; set; }
        public object Tag { get; set; }
    }

    public enum OperationItemStatus
    {
        Waiting,
        Processing,
        Success,
        Error
    }

    public sealed partial class OperationProgressDialog : Form
    {
        private sealed class ItemProgress
        {
            public int Index;
            public OperationItemStatus Status;
            public string ErrorMessage;
        }

        private struct RowControls
        {
            public Label Icon;
            public Label Name;
            public Label Status;
        }

        private readonly List<OperationProgressItem> _items;
        private readonly Func<OperationProgressItem, string> _workerFunc;
        private readonly string _dialogTitle;
        private readonly string _headerText;
        private readonly string _processingText;

        private int _successCount;
        private int _failCount;
        private readonly List<string> _errors = new List<string>();
        private readonly List<RowControls> _rows = new List<RowControls>();

        public OperationProgressDialog(
            string dialogTitle,
            string headerText,
            string processingText,
            IEnumerable<OperationProgressItem> items,
            Func<OperationProgressItem, string> workerFunc)
        {
            _dialogTitle = dialogTitle ?? "กำลังดำเนินการ";
            _headerText = headerText ?? "กำลังดำเนินการ";
            _processingText = processingText ?? "กำลังดำเนินการ...";
            _items = (items ?? Enumerable.Empty<OperationProgressItem>()).ToList();
            _workerFunc = workerFunc ?? throw new ArgumentNullException(nameof(workerFunc));

            InitializeComponent();

            Text = _dialogTitle;
            lblHeader.Text = _headerText;
            lblSubHeader.Text = _processingText;

            BuildRows();
        }

        private void BuildRows()
        {
            pnlItemsList.Controls.Clear();
            _rows.Clear();

            int y = 8;
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var pnlRow = new Panel
                {
                    Location = new Point(12, y),
                    Size = new Size(524, 34),
                    BackColor = Color.White
                };
                var lblIcon = new Label
                {
                    Text = "⏳",
                    Location = new Point(0, 5),
                    Size = new Size(28, 22),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                var lblName = new Label
                {
                    Text = item.Title ?? string.Format("รายการ #{0}", i + 1),
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(30, 30, 30),
                    Location = new Point(32, 5),
                    Size = new Size(350, 22),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                };
                var lblStatus = new Label
                {
                    Text = "รอดำเนินการ",
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Location = new Point(386, 5),
                    Size = new Size(130, 22),
                    TextAlign = ContentAlignment.MiddleRight,
                    AutoEllipsis = true
                };
                pnlRow.Controls.Add(lblIcon);
                pnlRow.Controls.Add(lblName);
                pnlRow.Controls.Add(lblStatus);
                pnlItemsList.Controls.Add(pnlRow);
                _rows.Add(new RowControls { Icon = lblIcon, Name = lblName, Status = lblStatus });

                var sep = new Panel
                {
                    Location = new Point(12, y + 34),
                    Size = new Size(524, 1),
                    BackColor = Color.FromArgb(240, 242, 245)
                };
                pnlItemsList.Controls.Add(sep);
                y += 38;
            }

            progressBar.Minimum = 0;
            progressBar.Maximum = Math.Max(1, _items.Count);
            progressBar.Value = 0;
            lblProgress.Text = string.Format("0 / {0}", _items.Count);

            int listHeight = Math.Min(_items.Count * 38 + 16, 280);
            ClientSize = new Size(560, 60 + listHeight + 68);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartExecution();
        }

        private void StartExecution()
        {
            var worker = new BackgroundWorker { WorkerReportsProgress = true };
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_Completed;
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                worker.ReportProgress(0, new ItemProgress { Index = i, Status = OperationItemStatus.Processing });

                var result = new ItemProgress { Index = i };
                try
                {
                    string err = _workerFunc(item);
                    if (string.IsNullOrEmpty(err))
                    {
                        result.Status = OperationItemStatus.Success;
                    }
                    else
                    {
                        result.Status = OperationItemStatus.Error;
                        result.ErrorMessage = err;
                    }
                }
                catch (Exception ex)
                {
                    result.Status = OperationItemStatus.Error;
                    result.ErrorMessage = ex.Message;
                }

                worker.ReportProgress(0, result);
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var p = (ItemProgress)e.UserState;
            if (p.Index < 0 || p.Index >= _rows.Count) return;

            var row = _rows[p.Index];

            switch (p.Status)
            {
                case OperationItemStatus.Processing:
                    row.Icon.Text = "🔄";
                    row.Status.Text = "กำลังดำเนินการ...";
                    row.Status.ForeColor = Color.FromArgb(37, 99, 235);
                    break;

                case OperationItemStatus.Success:
                    row.Icon.Text = "✅";
                    row.Status.Text = "สำเร็จ";
                    row.Status.ForeColor = Color.FromArgb(21, 128, 61);
                    _successCount++;
                    progressBar.Value = Math.Min(_successCount + _failCount, _items.Count);
                    lblProgress.Text = string.Format("{0} / {1}", progressBar.Value, _items.Count);
                    break;

                case OperationItemStatus.Error:
                    row.Icon.Text = "❌";
                    row.Status.Text = string.IsNullOrEmpty(p.ErrorMessage) ? "ล้มเหลว" : "ล้มเหลว: " + p.ErrorMessage;
                    row.Status.ForeColor = Color.FromArgb(185, 28, 28);
                    _failCount++;
                    _errors.Add(string.Format("• {0}: {1}", _items[p.Index].Title, p.ErrorMessage ?? "ไม่ทราบสาเหตุ"));
                    progressBar.Value = Math.Min(_successCount + _failCount, _items.Count);
                    lblProgress.Text = string.Format("{0} / {1}", progressBar.Value, _items.Count);
                    break;
            }
        }

        private void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = _items.Count;
            lblProgress.Text = string.Format("{0} / {1}", _items.Count, _items.Count);

            if (_failCount == 0)
            {
                lblSubHeader.Text = "เสร็จสิ้น";
                lblSubHeader.ForeColor = Color.FromArgb(134, 239, 172);
                lblSummary.Text = string.Format("สำเร็จทั้งหมด {0} รายการ", _successCount);
                lblSummary.ForeColor = Color.FromArgb(21, 128, 61);
                btnClose.BackColor = Color.FromArgb(21, 128, 61);
                DialogResult = DialogResult.OK;
            }
            else
            {
                lblSubHeader.Text = "เสร็จสิ้น (มีข้อผิดพลาด)";
                lblSubHeader.ForeColor = Color.FromArgb(254, 202, 202);
                lblSummary.Text = string.Format("สำเร็จ {0} รายการ  •  ล้มเหลว {1} รายการ", _successCount, _failCount);
                lblSummary.ForeColor = Color.FromArgb(185, 28, 28);
                btnClose.BackColor = Color.FromArgb(107, 114, 128);
            }

            btnClose.Text = "ปิด";
            btnClose.Enabled = true;

            if (_errors.Count > 0)
            {
                MessageBox.Show(this,
                    "ดำเนินการไม่สำเร็จ " + _errors.Count + " รายการ\n\n" + string.Join("\n", _errors),
                    _dialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (_failCount > 0)
                DialogResult = DialogResult.Cancel;
        }

        private void PnlFooter_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(229, 231, 235)))
                e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
