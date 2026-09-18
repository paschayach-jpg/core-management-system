// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using CMS.Controllers.KMS.KB;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// Wizard นำเข้า ประกาศ/คำสั่ง จาก Excel — 3 step: upload → validate → confirm
    /// โครงหน้าจอยึดแนวทางเดียวกับ KBImportWizardForm
    /// </summary>
    public partial class KBOrderImportWizardForm : Form, IKBOrderImportWizardView
    {
        private KBOrderImportWizardController _presenter;
        private readonly UserContext _user;

        public event EventHandler BackRequested;

        private int    _currentStep = 1;
        private string _filePath;

        private IList<KBSectionGroup> _sectionGroups = new List<KBSectionGroup>();

        private static readonly Color ColorNavy   = Color.FromArgb(30, 58, 95);
        private static readonly Color ColorBlue   = Color.FromArgb(46, 116, 181);
        private static readonly Color ColorGreen  = Color.FromArgb(39, 174, 96);
        private static readonly Color ColorRed    = Color.FromArgb(192, 57, 43);
        private static readonly Color ColorAmber  = Color.FromArgb(243, 156, 18);
        private static readonly Color ColorMuted  = Color.FromArgb(127, 140, 141);
        private static readonly Color RowRed      = Color.FromArgb(253, 237, 236);
        private static readonly Color RowAmber    = Color.FromArgb(254, 249, 231);
        private static readonly Color RowZebra    = Color.FromArgb(248, 250, 253);

        public KBOrderImportWizardForm(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;

            InitializeComponent();
            InitPreviewGrid();
            InitSectionsGrid();
            InitFailedGrid();
        }

        /// <summary>เรียกโดย composition root ก่อนแสดงฟอร์ม</summary>
        public void AttachPresenter(KBOrderImportWizardController presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        private void KBOrderImportWizardForm_Load(object sender, EventArgs e)
        {
            pnlUploadZone.Paint     += PnlUploadZone_Paint;
            pnlUploadZone.DragEnter += PnlUploadZone_DragEnter;
            pnlUploadZone.DragLeave += PnlUploadZone_DragLeave;
            pnlUploadZone.DragDrop  += PnlUploadZone_DragDrop;

            dgvSections.CellValueChanged   += DgvSections_CellValueChanged;
            dgvSections.CurrentCellDirtyStateChanged += DgvSections_CurrentCellDirtyStateChanged;

            ShowStep(1);
        }

        // ── IKBOrderImportWizardView ─────────────────────────────────────────

        public string SelectedFilePath { get { return _filePath; } }

        public string SelectedSheetName
        {
            get { return cboSheet.SelectedItem == null ? null : cboSheet.SelectedItem.ToString(); }
        }

        public bool OptSkipRowOnDownloadFailure { get { return chkSkipDownloadFail.Checked; } }

        public void ShowSheetNames(IList<string> sheetNames)
        {
            cboSheet.Items.Clear();
            if (sheetNames == null || sheetNames.Count == 0)
            {
                pnlSheetRow.Visible = false;
                return;
            }

            foreach (var name in sheetNames)
                cboSheet.Items.Add(name);

            cboSheet.SelectedIndex = 0;
            pnlSheetRow.Visible = true;

            // มี sheet เดียวก็ไม่ต้องให้เลือก
            cboSheet.Enabled = sheetNames.Count > 1;

            SetStatus(sheetNames.Count > 1
                ? string.Format("ไฟล์นี้มี {0} sheet — กรุณาเลือก sheet ที่ต้องการนำเข้า", sheetNames.Count)
                : "ไฟล์นี้มี 1 sheet");
        }

        public void ShowValidationResult(KBOrderImportResult result, IList<KBSectionGroup> sectionGroups)
        {
            _sectionGroups = sectionGroups ?? new List<KBSectionGroup>();

            lblPillTotal.Text = string.Format("ทั้งหมด  {0}", result.TotalRecords);
            lblPillValid.Text = string.Format("พร้อมนำเข้า  {0}", result.ValidRecords);
            lblPillError.Text = string.Format("ข้อผิดพลาด  {0}", result.ErrorRecords);

            PopulateSectionsGrid(_sectionGroups);
            PopulatePreviewGrid(result.Rows);

            btnImport.Enabled = result.ValidRecords > 0;
            ShowStep(2);
        }

        public void ShowImportComplete(KBOrderImportResult summary)
        {
            bool hasProblem = summary.ErrorRecords > 0 || summary.WarningRecords > 0;

            lblCompleteTitle.Text      = hasProblem ? "⚠️  นำเข้าเสร็จสิ้น (มีรายการที่ต้องตรวจสอบ)" : "✅  นำเข้าข้อมูลเรียบร้อย";
            lblCompleteTitle.ForeColor = hasProblem ? ColorAmber : ColorGreen;
            lblCompleteSub.Text = string.Format(
                "ทั้งหมด {0} รายการ  •  สำเร็จ {1}  •  เตือน {2}  •  ข้าม/ผิดพลาด {3}",
                summary.TotalRecords, summary.ValidRecords, summary.WarningRecords, summary.ErrorRecords);

            PopulateFailedGrid(summary);
            ShowStep(3);
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SetStatus(string message)
        {
            lblStatusMessage.Text = message ?? string.Empty;
            Application.DoEvents();
        }

        public void SetProgress(int percent, string message)
        {
            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;

            progressBar.Visible = true;
            lblProgress.Visible = true;
            progressBar.Value   = percent;
            lblProgress.Text    = string.Format("{0}%", percent);

            SetStatus(message);
        }

        // ── Step navigation ──────────────────────────────────────────────────

        private void ShowStep(int step)
        {
            _currentStep = step;

            pnlStep1.Visible = (step == 1);
            pnlStep2.Visible = (step == 2);
            pnlStep3.Visible = (step == 3);

            if (step == 1) pnlStep1.BringToFront();
            if (step == 2) pnlStep2.BringToFront();
            if (step == 3) pnlStep3.BringToFront();

            StyleStepLabel(lblStep1, 1);
            StyleStepLabel(lblStep2, 2);
            StyleStepLabel(lblStep3, 3);
        }

        private void StyleStepLabel(Label label, int step)
        {
            if (step < _currentStep)
            {
                label.ForeColor = ColorGreen;
                label.Font = new Font(label.Font, FontStyle.Bold);
            }
            else if (step == _currentStep)
            {
                label.ForeColor = ColorBlue;
                label.Font = new Font(label.Font, FontStyle.Bold);
            }
            else
            {
                label.ForeColor = ColorMuted;
                label.Font = new Font(label.Font, FontStyle.Regular);
            }
        }

        // ── Step 1 — file selection ──────────────────────────────────────────

        private void PnlUploadZone_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(180, 195, 215), 2f))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.DrawRectangle(pen, 1, 1, pnlUploadZone.Width - 3, pnlUploadZone.Height - 3);
            }
        }

        private void PnlUploadZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                pnlUploadZone.BackColor = Color.FromArgb(214, 228, 240);
                pnlUploadZone.Invalidate();
            }
        }

        private void PnlUploadZone_DragLeave(object sender, EventArgs e)
        {
            pnlUploadZone.BackColor = Color.FromArgb(245, 247, 250);
            pnlUploadZone.Invalidate();
        }

        private void PnlUploadZone_DragDrop(object sender, DragEventArgs e)
        {
            pnlUploadZone.BackColor = Color.FromArgb(245, 247, 250);
            pnlUploadZone.Invalidate();

            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length == 0) return;

            string ext = Path.GetExtension(files[0]).ToLowerInvariant();
            if (ext != ".xlsx")
            {
                ShowError("รองรับเฉพาะไฟล์ .xlsx เท่านั้น");
                return;
            }

            HandleFileSelection(files[0]);
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel Files|*.xlsx";
                ofd.Title  = "เลือกไฟล์ Excel ประกาศ/คำสั่ง";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                    HandleFileSelection(ofd.FileName);
            }
        }

        private void HandleFileSelection(string path)
        {
            var info = new FileInfo(path);
            if (!info.Exists)
            {
                ShowError("ไม่พบไฟล์ที่เลือก");
                return;
            }

            _filePath = path;

            lblUploadedFileName.Text = string.Format("📊  {0}   ({1})", info.Name, FormatFileSize(info.Length));
            pnlUploadedFile.Visible  = true;
            pnlUploadZone.Visible    = false;
            btnNext.Enabled          = true;

            if (_presenter != null) _presenter.LoadSheetNames();
        }

        private void BtnRemoveFile_Click(object sender, EventArgs e)
        {
            _filePath = null;

            pnlUploadedFile.Visible = false;
            pnlUploadZone.Visible   = true;
            pnlSheetRow.Visible     = false;
            cboSheet.Items.Clear();
            btnNext.Enabled         = false;

            SetStatus("พร้อมใช้งาน");
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;

            if (string.IsNullOrWhiteSpace(_filePath))
            {
                ShowError("กรุณาเลือกไฟล์ Excel ก่อน");
                return;
            }

            if (pnlSheetRow.Visible && cboSheet.SelectedItem == null)
            {
                ShowError("กรุณาเลือก sheet ที่ต้องการนำเข้า");
                return;
            }

            Cursor = Cursors.WaitCursor;
            try { _presenter.ValidateFile(); }
            finally { Cursor = Cursors.Default; }
        }

        // ── Step 2 — grids ───────────────────────────────────────────────────

        private void InitSectionsGrid()
        {
            dgvSections.AutoGenerateColumns = false;
            dgvSections.AllowUserToResizeRows = false;
            dgvSections.RowTemplate.Height = 30;
            dgvSections.EnableHeadersVisualStyles = false;
            dgvSections.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 244, 249);
            dgvSections.ColumnHeadersDefaultCellStyle.ForeColor = ColorNavy;

            dgvSections.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSectionName", HeaderText = "มาตราที่อ้างถึง", Width = 200, ReadOnly = true
            });
            dgvSections.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSectionFound", HeaderText = "พบในระบบ", Width = 120, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            dgvSections.Columns.Add(new DataGridViewComboBoxColumn
            {
                Name = "colSectionPick", HeaderText = "เลือกปี (พ.ศ.)", Width = 320,
                FlatStyle = FlatStyle.Flat, DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            });
            dgvSections.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSectionCode", HeaderText = "ArticleCode ที่เลือก", ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private void PopulateSectionsGrid(IList<KBSectionGroup> groups)
        {
            dgvSections.Rows.Clear();

            foreach (var group in groups)
            {
                int rowIndex = dgvSections.Rows.Add();
                var row = dgvSections.Rows[rowIndex];
                row.Tag = group;

                row.Cells["colSectionName"].Value  = group.SectionRaw;
                row.Cells["colSectionFound"].Value = group.Candidates.Count;

                var cell = (DataGridViewComboBoxCell)row.Cells["colSectionPick"];
                cell.Items.Clear();

                if (group.Candidates.Count == 0)
                {
                    row.DefaultCellStyle.BackColor = RowRed;
                    cell.ReadOnly = true;
                    row.Cells["colSectionCode"].Value = "ไม่พบมาตรานี้ในระบบ";
                    continue;
                }

                foreach (var candidate in group.Candidates)
                    cell.Items.Add(candidate);

                cell.DisplayMember = "YearDisplay";

                var selected = group.Candidates.FirstOrDefault(c => c.ArticleID == group.SelectedArticleId);
                if (selected != null)
                {
                    cell.Value = selected;
                    row.Cells["colSectionCode"].Value = selected.ArticleCode;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = RowAmber;
                    row.Cells["colSectionCode"].Value = "— กรุณาเลือกปี —";
                }
            }
        }

        private void DgvSections_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // commit ค่าทันทีที่เลือกใน ComboBox แทนที่จะรอย้ายเซลล์
            if (dgvSections.IsCurrentCellDirty)
                dgvSections.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void DgvSections_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvSections.Columns[e.ColumnIndex].Name != "colSectionPick") return;

            var row   = dgvSections.Rows[e.RowIndex];
            var group = row.Tag as KBSectionGroup;
            if (group == null) return;

            var candidate = row.Cells["colSectionPick"].Value as KBSectionCandidate;
            if (candidate == null) return;

            group.SelectedArticleId = candidate.ArticleID;
            row.Cells["colSectionCode"].Value = candidate.ArticleCode;
            row.DefaultCellStyle.BackColor = Color.White;
        }

        private void InitPreviewGrid()
        {
            dgvPreview.AutoGenerateColumns = false;
            dgvPreview.RowTemplate.Height = 26;
            dgvPreview.EnableHeadersVisualStyles = false;
            dgvPreview.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 244, 249);
            dgvPreview.ColumnHeadersDefaultCellStyle.ForeColor = ColorNavy;

            var left = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNo",      HeaderText = "แถว",            Width = 60 });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus",  HeaderText = "สถานะ",           Width = 90 });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCode",    HeaderText = "ArticleCode (D)", Width = 220, DefaultCellStyle = left });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTitle",   HeaderText = "ชื่อ (E)",         Width = 260, DefaultCellStyle = left });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCat",     HeaderText = "คำสั่งที่ (F)",     Width = 130, DefaultCellStyle = left });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPub",     HeaderText = "วันที่ประกาศ (G)",  Width = 120 });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEff",     HeaderText = "วันที่มีผล (H)",    Width = 120 });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFile",    HeaderText = "ไฟล์แนบ (K)",      Width = 180, DefaultCellStyle = left });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSection", HeaderText = "กฎหมายอ้างอิง (L)", Width = 160, DefaultCellStyle = left });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNote", HeaderText = "หมายเหตุ",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = left
            });
        }

        private void PopulatePreviewGrid(IList<KBOrderImportRow> rows)
        {
            dgvPreview.Rows.Clear();
            if (rows == null) return;

            var ordered = rows
                .OrderBy(r => r.Status == "Error" ? 0 : (r.Status == "Warn" ? 1 : 2))
                .ThenBy(r => r.RowNumber)
                .ToList();

            dgvPreview.SuspendLayout();
            for (int i = 0; i < ordered.Count; i++)
            {
                var r = ordered[i];
                int idx = dgvPreview.Rows.Add(
                    r.RowNumber,
                    StatusLabel(r.Status),
                    r.FinalArticleCode,
                    Truncate(r.Title, 200),
                    r.TitleCategory,
                    r.PublishedDate.HasValue ? r.PublishedDate.Value.ToString("dd/MM/yyyy") : "-",
                    r.EffectiveDate.HasValue ? r.EffectiveDate.Value.ToString("dd/MM/yyyy") : "-",
                    r.AttachmentFileName,
                    r.SectionsRaw,
                    Truncate(r.StatusNote, 300));

                var gridRow = dgvPreview.Rows[idx];
                if (r.Status == "Error")     gridRow.DefaultCellStyle.BackColor = RowRed;
                else if (r.Status == "Warn") gridRow.DefaultCellStyle.BackColor = RowAmber;
                else if (i % 2 == 1)         gridRow.DefaultCellStyle.BackColor = RowZebra;
            }
            dgvPreview.ResumeLayout();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (_currentStep == 2) ShowStep(1);
            else if (_currentStep == 3) ShowStep(2);
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;

            var confirm = MessageBox.Show(
                string.Format("ยืนยันการนำเข้าข้อมูล?\n\n{0}\n{1}",
                    lblPillValid.Text, lblPillError.Text),
                "ยืนยันการนำเข้า", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            btnImport.Enabled = false;
            btnBack.Enabled   = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                _presenter.ExecuteImport();
            }
            finally
            {
                Cursor = Cursors.Default;
                btnImport.Enabled   = true;
                btnBack.Enabled     = true;
                progressBar.Visible = false;
                lblProgress.Visible = false;
            }
        }

        // ── Step 3 ───────────────────────────────────────────────────────────

        private void InitFailedGrid()
        {
            dgvFailed.AutoGenerateColumns = false;
            dgvFailed.RowTemplate.Height = 26;
            dgvFailed.EnableHeadersVisualStyles = false;
            dgvFailed.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236);
            dgvFailed.ColumnHeadersDefaultCellStyle.ForeColor = ColorRed;

            var left = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            dgvFailed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFRow",   HeaderText = "แถว Excel",      Width = 90 });
            dgvFailed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFCode",  HeaderText = "ArticleCode (D)", Width = 200, DefaultCellStyle = left });
            dgvFailed.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFTitle", HeaderText = "ชื่อ (E)",        Width = 320, DefaultCellStyle = left });
            dgvFailed.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFReason", HeaderText = "สาเหตุ",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = left
            });
        }

        private void PopulateFailedGrid(KBOrderImportResult summary)
        {
            dgvFailed.Rows.Clear();

            var problems = new List<KBOrderImportRow>();
            problems.AddRange(summary.SkippedRows);
            problems.AddRange(summary.ImportFailedRows);
            problems.AddRange(summary.Rows.Where(r => r.Status == "Warn"));

            foreach (var r in problems.OrderBy(r => r.RowNumber))
            {
                int idx = dgvFailed.Rows.Add(
                    r.RowNumber,
                    r.FinalArticleCode,
                    Truncate(r.Title, 200),
                    Truncate(r.StatusNote, 400));

                dgvFailed.Rows[idx].DefaultCellStyle.BackColor =
                    r.Status == "Warn" ? RowAmber : RowRed;
            }

            dgvFailed.Visible = dgvFailed.Rows.Count > 0;
        }

        private void BtnImportNew_Click(object sender, EventArgs e)
        {
            _filePath = null;
            _sectionGroups = new List<KBSectionGroup>();

            pnlUploadedFile.Visible = false;
            pnlUploadZone.Visible   = true;
            pnlSheetRow.Visible     = false;
            cboSheet.Items.Clear();
            btnNext.Enabled         = false;

            dgvPreview.Rows.Clear();
            dgvSections.Rows.Clear();
            dgvFailed.Rows.Clear();

            SetStatus("พร้อมใช้งาน");
            ShowStep(1);
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            RaiseBackOrClose();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            RaiseBackOrClose();
        }

        private void BtnBackToMain_Click(object sender, EventArgs e)
        {
            RaiseBackOrClose();
        }

        private void RaiseBackOrClose()
        {
            if (BackRequested != null)
            {
                BackRequested(this, EventArgs.Empty);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string StatusLabel(string status)
        {
            switch (status)
            {
                case "Error": return "❌  Error";
                case "Warn":  return "⚠️  Warn";
                case "Valid": return "✅  Valid";
                default:      return status;
            }
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= max ? value : value.Substring(0, max) + "…";
        }

        private static string FormatFileSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < units.Length - 1) { order++; len /= 1024; }
            return string.Format("{0:0.##} {1}", len, units[order]);
        }
    }
}
