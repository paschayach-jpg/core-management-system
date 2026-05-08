// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Controllers.KMS.KB;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// KMS Import Wizard — นำเข้าข้อมูล Knowledge Base จาก Excel (MOD-006)
    /// 3-Step Wizard:
    ///   Step 1 — อัพโหลดไฟล์ Excel + เลือก Template
    ///   Step 2 — ตรวจสอบข้อมูล + Preview + ตัวเลือกการนำเข้า
    ///   Step 3 — สรุปผลการนำเข้า
    /// Layout ตาม MOD-006/kms_import.html
    /// </summary>
    public partial class KBImportWizardForm : Form, IKBImportWizardController
    {
        #region Fields

        private KBImportWizardController _presenter;
        private readonly UserContext    _user;
        private int      _currentStep = 1;

        // File state
        private string   _uploadedFilePath;
        private string   _uploadedFileName;
        private long     _uploadedFileSize;
        private DateTime _uploadedTime;

        // Step 3 — skipped/failed grid paging
        private IList<KBImportRow> _errAllRows;
        private int                _errCurrentPage;
        private const int          ErrPageSize = 20;

        #endregion

        #region Constructor

        public KBImportWizardForm(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
            InitializeComponent();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called by the composition root after the presenter is fully wired.
        /// Must be called before ShowDialog().
        /// </summary>
        public void AttachPresenter(KBImportWizardController presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        #endregion

        #region Form Events

        private void KBImportWizardForm_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\n\nกรุณาเรียก AttachPresenter() ก่อนเปิดฟอร์ม",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            // Status bar: user info
            lblStatusUser.Text = string.Format("👤  {0}  |  📅  {1}",
                _user.AdIdentity ?? "ผู้ใช้งาน",
                DateTime.Now.ToString("dd/MM/yyyy"));

            InitTemplateCombo();
            PopulateColumnReference();
            BuildValidationItems();

            // Wire upload-zone dashed border and drag events
            pnlUploadZone.Paint     += PnlUploadZone_Paint;
            pnlUploadZone.DragEnter += PnlUploadZone_DragEnter;
            pnlUploadZone.DragDrop  += PnlUploadZone_DragDrop;

            // Distribute pill widths evenly on load and whenever the panel resizes
            pnlSummaryPills.Resize   += (s, ev) => DistributePillWidths();
            pnlCompleteStats.Resize  += (s, ev) => DistributeStatWidths();
            DistributePillWidths();
            DistributeStatWidths();

            // Resize all step panels to match the content container
            // (no longer needed — vertical scroll layout uses DockStyle.Top)

            ShowStep(1);
        }

        #endregion

        #region IKBImportWizardController Implementation

        // Properties

        public string SelectedFilePath { get { return _uploadedFilePath; } }
        public string SelectedTemplate
        {
            get
            {
                return cboTemplate.SelectedItem != null
                    ? cboTemplate.SelectedItem.ToString()
                    : string.Empty;
            }
        }
        public bool OptSkipWarning  { get { return chkSkipWarning.Checked;  } }
        public bool OptAutoCategory { get { return chkAutoCategory.Checked; } }
        public bool OptSendEmail    { get { return chkSendEmail.Checked;    } }
        public bool OptLinkDMS      { get { return chkLinkDMS.Checked;      } }
        public bool OptCreateDraft  { get { return chkCreateDraft.Checked;  } }
        public bool OptAuditLog     { get { return chkAuditLog.Checked;     } }

        // Methods

        /// <summary>
        /// Receives validation results from the presenter, updates summary pills and
        /// preview grid, and advances the wizard to Step 2.
        /// </summary>
        public void ShowValidationResult(KBImportValidationResult result)
        {
            if (result == null) return;

            lblTotalCount.Text = result.TotalRecords.ToString();
            lblValidCount.Text = result.ValidRecords.ToString();
            lblWarnCount.Text  = result.WarningRecords.ToString();
            lblErrorCount.Text = result.ErrorRecords.ToString();

            btnImport.Text = string.Format(
                "📥  นำเข้า ({0} รายการ)", result.ValidRecords + result.WarningRecords);

            PopulatePreviewGrid(result.Rows);
            BindValidationItems(result);
            ShowStep(2);
            SetStatus("ตรวจสอบข้อมูลเรียบร้อย");
        }

        /// <summary>
        /// Receives the final import summary from the presenter, fills the completion
        /// panel, and advances the wizard to Step 3.
        /// </summary>
        public void ShowImportComplete(KBImportValidationResult summary)
        {
            if (summary == null) return;

            lblCompleteTotalNum.Text   = summary.TotalRecords.ToString();
            lblCompleteSuccessNum.Text = summary.ValidRecords.ToString();
            lblCompleteWarnNum.Text    = summary.WarningRecords.ToString();
            lblCompleteSkipNum.Text    = summary.ErrorRecords.ToString();

            var skipped = summary.SkippedRows      ?? new List<KBImportRow>();
            var failed  = summary.ImportFailedRows ?? new List<KBImportRow>();

            var allProblemRows = new List<KBImportRow>(skipped);
            allProblemRows.AddRange(failed);

            bool hasProblemRows = allProblemRows.Count > 0;

            if (hasProblemRows)
            {
                lblCompleteIcon.Text  = "⚠️";
                lblCompleteTitle.Text = string.Format(
                    "นำเข้าสำเร็จ {0} รายการ  /  ข้าม (ก่อนนำเข้า) {1} รายการ  /  ล้มเหลวระหว่างนำเข้า {2} รายการ",
                    summary.ValidRecords, skipped.Count, failed.Count);
                lblCompleteTitle.ForeColor = Color.FromArgb(192, 57, 43);
                lblCompleteSub.Text        = "รายการด้านล่างถูกข้ามหรือล้มเหลว — ตรวจสอบสาเหตุและแก้ไขไฟล์ก่อนนำเข้าใหม่";

                PopulateErrorGrid(allProblemRows);
                pnlErrorDetail.Visible = true;
                pnlStep3.Height        = 800;
            }
            else
            {
                lblCompleteIcon.Text       = "✅";
                lblCompleteTitle.Text      = "นำเข้าข้อมูลสำเร็จ!";
                lblCompleteTitle.ForeColor = Color.FromArgb(30, 132, 73);
                lblCompleteSub.Text        = "บทความทั้งหมดได้ถูกนำเข้าไปยัง Knowledge Base เรียบร้อยแล้ว";

                pnlErrorDetail.Visible = false;
                pnlStep3.Height        = 460;
            }

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
            lblStatusMessage.Text = message;
        }

        public void SetProgress(int percent, string message)
        {
            progressBar.Value    = Math.Max(0, Math.Min(100, percent));
            lblProgressText.Text = string.Format("{0}  ({1}%)", message, percent);
            Application.DoEvents();
        }

        #endregion

        #region Event Handlers

        // Upload Zone Events

        private void PnlUploadZone_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(180, 195, 215), 2f))
            {
                pen.DashStyle = DashStyle.Dash;
                e.Graphics.DrawRectangle(pen, 1, 1,
                    pnlUploadZone.Width - 3, pnlUploadZone.Height - 3);
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

        private void PnlUploadZone_DragDrop(object sender, DragEventArgs e)
        {
            pnlUploadZone.BackColor = Color.FromArgb(245, 247, 250);
            pnlUploadZone.Invalidate();

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;
            string ext = Path.GetExtension(files[0]).ToLower();
            if (ext == ".xlsx" || ext == ".xls")
                HandleFileSelection(files[0]);
            else
                MessageBox.Show("กรุณาเลือกไฟล์ Excel (.xlsx หรือ .xls) เท่านั้น",
                    "ไฟล์ไม่ถูกต้อง", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // File Selection Events

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel Files|*.xlsx;*.xls";
                ofd.Title  = "เลือกไฟล์ Excel สำหรับนำเข้า";
                if (ofd.ShowDialog() == DialogResult.OK)
                    HandleFileSelection(ofd.FileName);
            }
        }

        private void BtnRemoveFile_Click(object sender, EventArgs e)
        {
            _uploadedFilePath = null;
            _uploadedFileName = null;
            _uploadedFileSize = 0;

            pnlUploadedFile.Visible = false;
            pnlUploadZone.Visible   = true;
            btnNext.Enabled         = false;
            lblFileHint.Visible     = true;
            SetStatus("ลบไฟล์แล้ว");
        }

        private void BtnDownloadTemplate_Click(object sender, EventArgs e)
        {
            SetStatus("กำลังดาวน์โหลด Template... (ฟีเจอร์อยู่ระหว่างพัฒนา)");
        }

        private void BtnDemo_Click(object sender, EventArgs e)
        {
            _uploadedFilePath = @"C:\Demo\KnowledgeBase_Import_Demo.xlsx";
            _uploadedFileName = "KnowledgeBase_Import_Demo.xlsx";
            _uploadedFileSize = 1258291L;
            _uploadedTime     = DateTime.Now;
            ShowUploadedFile();
            lblUploadedFileMeta.Text = string.Format(
                "Excel Workbook  •  1.2 MB  •  อัพโหลดเมื่อ {0} น.  (ไฟล์ Demo)",
                _uploadedTime.ToString("HH:mm"));
        }

        // Navigation Events

        private void BtnNext_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (string.IsNullOrEmpty(_uploadedFilePath))
            {
                MessageBox.Show("กรุณาเลือกไฟล์ Excel ก่อน",
                    "กรุณาเลือกไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Presenter validates file; calls ShowValidationResult() on success
            _presenter.ValidateFile();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            // Navigate back to previous step
            if (_currentStep == 2)
            {
                // From Step 2 back to Step 1
                ShowStep(1);
            }
            else if (_currentStep == 3)
            {
                // From Step 3 back to Step 2
                ShowStep(2);
            }
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;

            var confirm = MessageBox.Show(
                string.Format(
                    "ต้องการนำเข้าข้อมูล {0} รายการ ใช่หรือไม่?\n\n" +
                    "✅  พร้อมนำเข้า :  {1} รายการ\n" +
                    "⚠️  มีคำเตือน  :  {2} รายการ\n" +
                    "⛔  ข้อผิดพลาด :  {3} รายการ (จะข้าม)",
                    lblTotalCount.Text, lblValidCount.Text,
                    lblWarnCount.Text,  lblErrorCount.Text),
                "ยืนยันการนำเข้า", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            StartImport();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "ต้องการยกเลิกการนำเข้าข้อมูลใช่หรือไม่?",
                "ยืนยันการยกเลิก", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void BtnImportNew_Click(object sender, EventArgs e)
        {
            // Reset wizard back to Step 1
            _uploadedFilePath = null;
            _uploadedFileName = null;
            _uploadedFileSize = 0;

            pnlUploadedFile.Visible = false;
            pnlUploadZone.Visible   = true;
            btnNext.Enabled         = false;
            lblFileHint.Visible     = true;
            previewControl.Clear();

            pnlStep3.Height        = 460;
            pnlErrorDetail.Visible = false;
            _errAllRows            = null;
            _errCurrentPage        = 0;

            ShowStep(1);
        }

        // Step 3 — error grid pager events

        private void BtnErrPrev_Click(object sender, EventArgs e)
        {
            ShowErrorPage(_errCurrentPage - 1);
        }

        private void BtnErrNext_Click(object sender, EventArgs e)
        {
            ShowErrorPage(_errCurrentPage + 1);
        }

        // Step Progress Paint Event

        private void PnlStepProgress_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // bottom separator
            using (var pen = new Pen(Color.FromArgb(208, 215, 222), 1))
                g.DrawLine(pen, 0, pnlStepProgress.Height - 1,
                    pnlStepProgress.Width, pnlStepProgress.Height - 1);

            // draw rounded circles over each badge label
            foreach (var (lbl, active, done) in new[]
            {
                (lblStep1Circle, _currentStep == 1, _currentStep > 1),
                (lblStep2Circle, _currentStep == 2, _currentStep > 2),
                (lblStep3Circle, _currentStep == 3, _currentStep > 3),
            })
            {
                var r = new Rectangle(
                    lbl.Parent.Left + lbl.Left,
                    lbl.Parent.Top  + lbl.Top,
                    lbl.Width, lbl.Height);

                Color fill = done   ? Color.FromArgb(39, 174, 96)  :
                             active ? Color.FromArgb(46, 116, 181) :
                                      Color.FromArgb(230, 236, 245);

                using (var brush = new System.Drawing.SolidBrush(fill))
                    g.FillEllipse(brush, r);

                // subtle ring for inactive
                if (!active && !done)
                    using (var pen = new Pen(Color.FromArgb(189, 195, 199), 1.5f))
                        g.DrawEllipse(pen, r);
            }
        }

        #endregion

        #region Helper Methods

        // Template Initialization

        private void InitTemplateCombo()
        {
            cboTemplate.Items.Clear();
            cboTemplate.Items.Add("ความรู้ประกันภัย / Knowledge Base");
            cboTemplate.Items.Add("SOP & แนวทาง / Procedures");
            cboTemplate.Items.Add("FAQ / คำถามที่พบบ่อย");
            cboTemplate.Items.Add("แนวทางปฏิบัติ / Guidelines");
            cboTemplate.SelectedIndex = 0;
        }

        // Column Reference Population

        private void PopulateColumnReference()
        {
            lvColumnRef.Items.Clear();

            // Columns A–T as defined in requirements.md (Requirement 1.2)
            // Required columns: A, B, C, D, E, F, H, J, L, N, P, R, T
            var cols = new[]
            {
                new { L="A",  F="รหัสหมวดหมู่หลัก (ParentCategory)",  T="Text",                          R=true,  E="LW" },
                new { L="B",  F="รหัสหมวดหมู่ย่อย (SubCategory)",     T="Numeric",                       R=true,  E="03" },
                new { L="C",  F="ชื่อหมวดหมู่ (CategoryName)",        T="Text",                          R=true,  E="กฎหมายประกันภัย" },
                new { L="D",  F="รหัสส่วนที่ 3 (code segment 3)",      T="Numeric",                       R=true,  E="003" },
                new { L="E",  F="ชื่อบทความ / สรุปส่วนที่ 1",         T="Text",                          R=true,  E="มาตรา 31" },
                new { L="F",  F="รหัสส่วนที่ 4 (code segment 4)",      T="Numeric",                       R=true,  E="1" },
                new { L="G",  F="สรุปส่วนที่ 2 (summary segment 2)",   T="Text หรือ \"-\" / \"ไม่มีค่า\"", R=false, E="-" },
                new { L="H",  F="รหัสส่วนที่ 5 (code segment 5)",      T="Numeric",                       R=true,  E="0002" },
                new { L="I",  F="สรุปส่วนที่ 3 (summary segment 3)",   T="Text หรือ \"-\" / \"ไม่มีค่า\"", R=false, E="ไม่มีค่า" },
                new { L="J",  F="รหัสส่วนที่ 6 (code segment 6)",      T="Numeric",                       R=true,  E="03" },
                new { L="K",  F="สรุปส่วนที่ 4 (summary segment 4)",   T="Text หรือ \"-\" / \"ไม่มีค่า\"", R=false, E="-" },
                new { L="L",  F="รหัสส่วนที่ 7 (code segment 7)",      T="Numeric",                       R=true,  E="0000" },
                new { L="M",  F="สรุปส่วนที่ 5 (summary segment 5)",   T="Text หรือ \"-\" / \"ไม่มีค่า\"", R=false, E="-" },
                new { L="N",  F="รหัสส่วนที่ 8 (code segment 8)",      T="Numeric",                       R=true,  E="000" },
                new { L="O",  F="สรุปส่วนที่ 6 (summary segment 6)",   T="Text หรือ \"-\" / \"ไม่มีค่า\"", R=false, E="-" },
                new { L="P",  F="รหัสส่วนที่ 9 (code segment 9)",      T="Numeric",                       R=true,  E="0" },
                new { L="Q",  F="สรุปส่วนที่ 7 (summary segment 7)",   T="Text หรือ \"-\" / \"ไม่มีค่า\"", R=false, E="-" },
                new { L="R",  F="วันที่เผยแพร่ (PublishedDate)",       T="Date — Thai B.E. text เช่น \"9 เมษายน 2535\"", R=true,  E="9 เมษายน 2535" },
                new { L="S",  F="วันหมดอายุ (ExpiryDate)",             T="Date — Thai B.E. text (ไม่บังคับ)",             R=false, E="31 ธันวาคม 2570" },
                new { L="T",  F="เนื้อหาบทความ (ContentHTML)",        T="Text (HTML หรือ Plain Text)",   R=true,  E="<p>เนื้อหา...</p>" },
            };

            foreach (var c in cols)
            {
                var item = new ListViewItem(c.L);
                item.SubItems.Add(c.F);
                item.SubItems.Add(c.T);
                item.SubItems.Add(c.R ? "✱ จำเป็น" : "ไม่บังคับ");
                item.SubItems.Add(c.E);
                item.UseItemStyleForSubItems = false;
                item.SubItems[3].ForeColor   = c.R
                    ? Color.FromArgb(192, 57, 43)
                    : Color.FromArgb(127, 140, 141);
                lvColumnRef.Items.Add(item);
            }
        }

        // Validation Items Management

        private void BuildValidationItems()
        {
            // Called on form load with placeholder text; real data arrives via BindValidationItems().
            SetValItem(1, "⏳", "รูปแบบไฟล์",          "รอการตรวจสอบ...", false);
            SetValItem(2, "⏳", "โครงสร้าง Template",   "รอการตรวจสอบ...", false);
            SetValItem(3, "⏳", "คอลัมน์จำเป็น",        "รอการตรวจสอบ...", false);
            SetValItem(4, "⏳", "คำเตือน",              "รอการตรวจสอบ...", false);
            SetValItem(5, "⏳", "วันที่ (R/S)",          "รอการตรวจสอบ...", false);
            SetValItem(6, "⏳", "แถวที่มีข้อผิดพลาด",   "รอการตรวจสอบ...", false);
        }

        /// <summary>
        /// Refreshes pnlValidationItems based on the actual validation result.
        /// Called from ShowValidationResult() after PopulatePreviewGrid().
        /// </summary>
        private void BindValidationItems(KBImportValidationResult result)
        {
            var rows = result.Rows ?? new List<KBImportRow>();

            // Item 1 — File format (always passes if we got a result back)
            SetValItem(1, "✅", "รูปแบบไฟล์: Excel (.xlsx)",
                "ไฟล์ถูกต้องและสามารถเปิดได้", true);

            // Item 2 — Template structure: data rows found
            bool structureOk = result.TotalRecords > 0;
            SetValItem(2,
                structureOk ? "✅" : "❌",
                structureOk ? "โครงสร้าง Template: ถูกต้อง" : "โครงสร้าง Template: ไม่ถูกต้อง",
                structureOk
                    ? string.Format("พบ {0} แถวข้อมูล — คอลัมน์ครบตาม Template", result.TotalRecords)
                    : "ไม่พบข้อมูลในไฟล์ หรือ Template ไม่ตรง",
                structureOk);

            // Item 3 — Required columns: count rows with missing-required-field errors
            // Required columns per requirements.md: A, B, C, D, E, F, H, J, L, N, P, R, T
            int missingRequired = 0;
            foreach (var r in rows)
                if (r.Status == "Error" && r.StatusNote != null &&
                    r.StatusNote.IndexOf("จำเป็น", StringComparison.OrdinalIgnoreCase) >= 0)
                    missingRequired++;

            bool reqOk = missingRequired == 0;
            SetValItem(3,
                reqOk ? "✅" : "❌",
                reqOk ? "คอลัมน์จำเป็น: ครบถ้วน"
                      : string.Format("คอลัมน์จำเป็น: ขาด {0} แถว", missingRequired),
                reqOk ? "คอลัมน์ A, B, C, D, E, F, H, J, L, N, P, R, T ครบทุกแถว"
                      : string.Format("{0} แถวขาดข้อมูลในคอลัมน์บังคับ (A/B/C/D/E/F/H/J/L/N/P/R/T)", missingRequired),
                reqOk);

            // Item 4 — Warnings
            int warnCount = result.WarningRecords;
            bool noWarns  = warnCount == 0;
            string warnDetail;
            if (noWarns)
            {
                warnDetail = "ไม่พบคำเตือน — ข้อมูลทุกแถวสมบูรณ์";
            }
            else
            {
                var warnNotes = new System.Text.StringBuilder();
                foreach (var r in rows)
                    if (r.Status == "Warn" && !string.IsNullOrEmpty(r.StatusNote))
                        warnNotes.AppendFormat("แถวที่ {0}: {1}  |  ", r.RowNumber, r.StatusNote);
                string notes = warnNotes.ToString().TrimEnd(' ', '|', ' ');
                warnDetail = string.IsNullOrEmpty(notes)
                    ? string.Format("พบ {0} แถวที่มีคำเตือน", warnCount)
                    : notes;
            }
            SetValItem(4,
                noWarns ? "✅" : "⚠️",
                noWarns ? "ไม่พบคำเตือน" : string.Format("พบ {0} คำเตือน", warnCount),
                warnDetail,
                noWarns);

            // Item 5 — Date parse errors: rows with unparseable PublishedDate or ExpiryDate
            int dateBadCount = 0;
            foreach (var r in rows)
                if (r.Status == "Error" && r.StatusNote != null &&
                    r.StatusNote.IndexOf("แปลงวันที่", StringComparison.OrdinalIgnoreCase) >= 0)
                    dateBadCount++;

            bool dateOk = dateBadCount == 0;
            SetValItem(5,
                dateOk ? "✅" : "❌",
                dateOk ? "วันที่ (R/S): แปลงได้ทุกแถว"
                       : string.Format("วันที่ (R/S): แปลงไม่ได้ {0} แถว", dateBadCount),
                dateOk ? "คอลัมน์ R (PublishedDate) และ S (ExpiryDate) แปลงเป็น ค.ศ. ได้ทุกแถว"
                       : string.Format("{0} แถวมีรูปแบบวันที่ที่ไม่รู้จัก — ตรวจสอบคอลัมน์ R/S", dateBadCount),
                dateOk);

            // Item 6 — Error rows: total count matching dgvPreview red rows
            int errorCount = result.ErrorRecords;
            bool noErrors  = errorCount == 0;

            string errorDetail;
            if (noErrors)
            {
                errorDetail = "ไม่พบแถวที่มีข้อผิดพลาด — ข้อมูลพร้อมนำเข้าทุกแถว";
            }
            else
            {
                // List the first 3 error rows with their notes (same data as dgvPreview red rows)
                var sb = new System.Text.StringBuilder();
                int shown = 0;
                foreach (var r in rows)
                {
                    if (r.Status != "Error") continue;
                    sb.AppendFormat("แถว {0}: {1}  |  ", r.RowNumber,
                        string.IsNullOrEmpty(r.StatusNote) ? "ข้อผิดพลาดไม่ระบุ" : r.StatusNote);
                    if (++shown >= 3) break;
                }
                string preview = sb.ToString().TrimEnd(' ', '|', ' ');
                errorDetail = shown < errorCount
                    ? string.Format("{0} (และอีก {1} แถว)", preview, errorCount - shown)
                    : preview;
            }

            SetValItem(6,
                noErrors ? "✅" : "❌",
                noErrors ? "ข้อผิดพลาด: ไม่พบ"
                         : string.Format("❌  ข้อผิดพลาด: {0} แถว (ดูรายละเอียดใน Gridview)", errorCount),
                errorDetail,
                noErrors);
        }

        /// <summary>Updates one validation item row (icon, title, detail, stripe colour).</summary>
        private void SetValItem(int item, string icon, string title, string detail, bool ok)
        {
            Label lblIcon, lblTitle, lblDetail;
            Panel stripe;
            switch (item)
            {
                case 1: lblIcon=lblValItem1Icon; lblTitle=lblValItem1Title; lblDetail=lblValItem1Detail; stripe=pnlValItem1Stripe; break;
                case 2: lblIcon=lblValItem2Icon; lblTitle=lblValItem2Title; lblDetail=lblValItem2Detail; stripe=pnlValItem2Stripe; break;
                case 3: lblIcon=lblValItem3Icon; lblTitle=lblValItem3Title; lblDetail=lblValItem3Detail; stripe=pnlValItem3Stripe; break;
                case 4: lblIcon=lblValItem4Icon; lblTitle=lblValItem4Title; lblDetail=lblValItem4Detail; stripe=pnlValItem4Stripe; break;
                case 5: lblIcon=lblValItem5Icon; lblTitle=lblValItem5Title; lblDetail=lblValItem5Detail; stripe=pnlValItem5Stripe; break;
                default: lblIcon=lblValItem6Icon; lblTitle=lblValItem6Title; lblDetail=lblValItem6Detail; stripe=pnlValItem6Stripe; break;
            }
            lblIcon.Text   = icon;
            lblTitle.Text  = title;
            lblDetail.Text = detail;

            // stripe colour: green=ok, amber=warn(⚠️), red=error(❌), grey=pending
            if (icon == "✅")      stripe.BackColor = Color.FromArgb(39, 174, 96);
            else if (icon == "⚠️") stripe.BackColor = Color.FromArgb(243, 156, 18);
            else if (icon == "❌") stripe.BackColor = Color.FromArgb(192, 57, 43);
            else                   stripe.BackColor = Color.FromArgb(189, 195, 199);

            // also tint the panel background to match
            Panel pnl;
            switch (item)
            {
                case 1: pnl = pnlValItem1; break;
                case 2: pnl = pnlValItem2; break;
                case 3: pnl = pnlValItem3; break;
                case 4: pnl = pnlValItem4; break;
                case 5: pnl = pnlValItem5; break;
                default: pnl = pnlValItem6; break;
            }
            if (icon == "✅")      pnl.BackColor = Color.FromArgb(240, 255, 244);
            else if (icon == "⚠️") pnl.BackColor = Color.FromArgb(254, 249, 231);
            else if (icon == "❌") pnl.BackColor = Color.FromArgb(253, 237, 236);
            else                   pnl.BackColor = Color.FromArgb(248, 250, 253);
        }

        // Step Navigation

        /// <summary>
        /// Shows only the current step panel and hides others.
        /// Step 1: Show pnlStep1 only
        /// Step 2: Show pnlStep2 only (hide pnlStep1 and pnlStep3)
        /// Step 3: Show pnlStep3 only (hide pnlStep1 and pnlStep2)
        /// </summary>
        private void ShowStep(int step)
        {
            _currentStep = step;

            // Show only the current step panel, hide others
            pnlStep1.Visible = (step == 1);
            pnlStep2.Visible = (step == 2);
            pnlStep3.Visible = (step == 3);

            // Action bar buttons
            pnlActStep1.Visible = (step == 1);
            pnlActStep2.Visible = (step == 2);
            pnlActStep3.Visible = (step == 3);

            UpdateStepIndicators();

            lblStepStatus.Text = string.Format("📋  Step {0}/3: {1}", step,
                step == 1 ? "อัพโหลดไฟล์" :
                step == 2 ? "ตรวจสอบข้อมูล" : "นำเข้าเสร็จสิ้น");

            // Scroll to top when changing steps
            pnlScroll.AutoScrollPosition = new System.Drawing.Point(0, 0);
        }

        private void UpdateStepIndicators()
        {
            UpdateOneStep(lblStep1Circle, lblStep1Label, 1);
            UpdateOneStep(lblStep2Circle, lblStep2Label, 2);
            UpdateOneStep(lblStep3Circle, lblStep3Label, 3);
            pnlLine12.BackColor = _currentStep > 1
                ? Color.FromArgb(39, 174, 96)
                : Color.FromArgb(208, 215, 222);
            pnlLine23.BackColor = _currentStep > 2
                ? Color.FromArgb(39, 174, 96)
                : Color.FromArgb(208, 215, 222);
        }

        private void UpdateOneStep(Label circle, Label label, int n)
        {
            if (_currentStep > n)
            {
                circle.BackColor = Color.Transparent;
                circle.ForeColor = Color.White;
                circle.Text      = "✓";
                label.ForeColor  = Color.FromArgb(39, 174, 96);
                label.Font       = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            }
            else if (_currentStep == n)
            {
                circle.BackColor = Color.Transparent;
                circle.ForeColor = Color.White;
                circle.Text      = n.ToString();
                label.ForeColor  = Color.FromArgb(46, 116, 181);
                label.Font       = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            }
            else
            {
                circle.BackColor = Color.Transparent;
                circle.ForeColor = Color.FromArgb(127, 140, 141);
                circle.Text      = n.ToString();
                label.ForeColor  = Color.FromArgb(127, 140, 141);
                label.Font       = new Font("Segoe UI", 8.5f);
            }
            pnlStepProgress.Invalidate();
        }

        // File Selection Handling

        private void HandleFileSelection(string filePath)
        {
            try
            {
                var fi = new FileInfo(filePath);
                if (fi.Length > 10L * 1024 * 1024)
                {
                    MessageBox.Show("ไฟล์มีขนาดเกิน 10 MB กรุณาเลือกไฟล์ที่มีขนาดเล็กกว่า",
                        "ไฟล์ใหญ่เกินไป", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _uploadedFilePath = filePath;
                _uploadedFileName = fi.Name;
                _uploadedFileSize = fi.Length;
                _uploadedTime     = DateTime.Now;
                ShowUploadedFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการอ่านไฟล์:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowUploadedFile()
        {
            pnlUploadZone.Visible   = false;
            pnlUploadedFile.Visible = true;

            lblUploadedFileName.Text = _uploadedFileName;
            lblUploadedFileMeta.Text = string.Format(
                "Excel Workbook  •  {0}  •  อัพโหลดเมื่อ {1} น.",
                FormatFileSize(_uploadedFileSize),
                _uploadedTime.ToString("HH:mm"));

            btnNext.Enabled     = true;
            lblFileHint.Visible = false;
            SetStatus(string.Format("✅  อัพโหลดไฟล์ \"{0}\" สำเร็จ", _uploadedFileName));
        }

        // Import

        private void StartImport()
        {
            pnlProgress.Visible  = true;
            progressBar.Value    = 0;
            lblProgressText.Text = "กำลังเตรียมข้อมูล...  (0%)";
            btnImport.Enabled    = false;
            btnBack.Enabled      = false;
            Application.DoEvents();

            // Presenter reports real progress via SetProgress(); calls ShowImportComplete() on success.
            _presenter.ExecuteImport();

            // ExecuteImport calls ShowImportComplete → ShowStep(3) which hides pnlStep2.
            // Reset buttons in case an error occurred and we stayed on Step 2.
            pnlProgress.Visible = false;
            btnImport.Enabled   = true;
            btnBack.Enabled     = true;
        }

        // Preview Grid Population

        private void PopulatePreviewGrid(IList<KBImportRow> rows)
        {
            previewControl.BindRows(rows);
        }

        // Utility Methods

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024L)        return bytes + " B";
            if (bytes < 1024L * 1024) return (bytes / 1024.0).ToString("0.0") + " KB";
            return (bytes / (1024.0 * 1024)).ToString("0.0") + " MB";
        }

        /// <summary>
        /// Distributes the four summary pills evenly across the full width of
        /// <c>pnlSummaryPills</c>, so they always fill the same horizontal space
        /// as <c>dgvPreview</c> (both use <c>DockStyle.Top</c> inside the same parent).
        /// </summary>
        private void DistributePillWidths()
        {
            int total     = pnlSummaryPills.ClientSize.Width;
            if (total <= 0) return;

            int quarter   = total / 4;
            int remainder = total - quarter * 4;   // absorb rounding in the last pill

            // Pills use DockStyle.Left, so order matters: Total → Valid → Warn → Error
            pnlPillTotal.Width = quarter;
            pnlPillValid.Width = quarter;
            pnlPillWarn.Width  = quarter;
            pnlPillError.Width = quarter + remainder;
        }

        /// <summary>
        /// Distributes the four Step-3 stat panels evenly across the full width of
        /// <c>pnlCompleteStats</c>, matching the proportions of the summary pills.
        /// </summary>
        private void DistributeStatWidths()
        {
            int total     = pnlCompleteStats.ClientSize.Width;
            if (total <= 0) return;

            int quarter   = total / 4;
            int remainder = total - quarter * 4;   // absorb rounding in the last panel

            // Stat panels use DockStyle.Left: Total → Success → Warn → Skip
            pnlStatTotal.Width   = quarter;
            pnlStatSuccess.Width = quarter;
            pnlStatWarn.Width    = quarter;
            pnlStatSkip.Width    = quarter + remainder;
        }

        // Step 3 — skipped/failed row grid with paging

        private void PopulateErrorGrid(IList<KBImportRow> rows)
        {
            _errAllRows     = rows;
            _errCurrentPage = 0;
            ShowErrorPage(0);
        }

        private void ShowErrorPage(int page)
        {
            int total     = _errAllRows == null ? 0 : _errAllRows.Count;
            int pageCount = total == 0 ? 1 : (total + ErrPageSize - 1) / ErrPageSize;
            page = Math.Max(0, Math.Min(page, pageCount - 1));
            _errCurrentPage = page;

            dgvFailedRows.Rows.Clear();

            int start = page * ErrPageSize;
            int end   = Math.Min(start + ErrPageSize, total);

            for (int i = start; i < end; i++)
            {
                var row = _errAllRows[i];
                string reason, relatedData;

                if (row.Status == "Warn")
                {
                    reason      = "⚠️ ข้ามตามตัวเลือก";
                    relatedData = row.StatusNote ?? string.Empty;
                }
                else
                {
                    ClassifyError(row.StatusNote ?? string.Empty, out reason, out relatedData);
                }

                int idx = dgvFailedRows.Rows.Add(
                    row.RowNumber,
                    row.ArticleCode ?? string.Empty,
                    string.Format("{0} > {1}", row.ColA, row.ColB),
                    row.ColE ?? string.Empty,
                    relatedData,
                    reason);

                Color rowColor = row.Status == "Warn"
                    ? Color.FromArgb(254, 249, 231)
                    : Color.FromArgb(253, 237, 236);
                dgvFailedRows.Rows[idx].DefaultCellStyle.BackColor = rowColor;
            }

            lblErrPage.Text    = string.Format("หน้า {0} / {1}  ({2} รายการ)", page + 1, pageCount, total);
            btnErrPrev.Enabled = page > 0;
            btnErrNext.Enabled = page < pageCount - 1;
        }

        /// <summary>
        /// Maps a raw exception / StatusNote string to a short Thai reason label and
        /// a "related data" snippet extracted from the message where possible.
        /// </summary>
        private static void ClassifyError(string note, out string reason, out string relatedData)
        {
            relatedData = string.Empty;

            if (string.IsNullOrEmpty(note))
            {
                reason = "ข้อผิดพลาดไม่ระบุ";
                return;
            }

            // Duplicate / unique key
            if (note.IndexOf("duplicate key", StringComparison.OrdinalIgnoreCase) >= 0 ||
                note.IndexOf("Violation of UNIQUE KEY", StringComparison.OrdinalIgnoreCase) >= 0 ||
                note.IndexOf("Cannot insert duplicate", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                reason = "❌ Duplicate key — ข้อมูลซ้ำในฐานข้อมูล";
                // Extract the duplicate value from "The duplicate key value is (…)"
                int p = note.IndexOf("(", StringComparison.Ordinal);
                int q = note.IndexOf(")", StringComparison.Ordinal);
                if (p >= 0 && q > p)
                    relatedData = "ค่าซ้ำ: " + note.Substring(p, q - p + 1);
                return;
            }

            // String truncation
            if (note.IndexOf("String or binary data would be truncated", StringComparison.OrdinalIgnoreCase) >= 0 ||
                note.IndexOf("truncated", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                reason      = "❌ ค่าในคอลัมน์ยาวเกินขนาดที่กำหนด";
                relatedData = "ตรวจสอบคอลัมน์ที่มีข้อความยาว (E, C, T)";
                return;
            }

            // NULL constraint
            if (note.IndexOf("Cannot insert the value NULL", StringComparison.OrdinalIgnoreCase) >= 0 ||
                note.IndexOf("null value", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                reason = "❌ ค่า NULL ในคอลัมน์บังคับ";
                int col = note.IndexOf("column '", StringComparison.OrdinalIgnoreCase);
                if (col >= 0)
                {
                    int end = note.IndexOf("'", col + 8, StringComparison.Ordinal);
                    if (end > col + 8)
                        relatedData = "คอลัมน์: " + note.Substring(col + 8, end - col - 8);
                }
                return;
            }

            // Foreign key
            if (note.IndexOf("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) >= 0 ||
                note.IndexOf("conflicted with the", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                reason      = "❌ Foreign key — ค่าอ้างอิงไม่มีในตาราง";
                relatedData = "ตรวจสอบ CategoryID หรือ ArticleType";
                return;
            }

            // Category code not found (from ResolveCategories)
            if (note.IndexOf("ไม่พบรหัสหมวดหมู่", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                reason      = "❌ รหัสหมวดหมู่ไม่ถูกต้อง";
                relatedData = note;
                return;
            }

            // Generic — return raw note, truncated
            reason      = "❌ ข้อผิดพลาดทั่วไป";
            relatedData = note.Length > 200 ? note.Substring(0, 200) + "…" : note;
        }

        #endregion
    }
}
