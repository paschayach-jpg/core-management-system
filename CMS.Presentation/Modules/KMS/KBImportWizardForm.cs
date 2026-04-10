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
        private KBImportWizardController _presenter;
        private readonly UserContext    _user;
        private int      _currentStep = 1;

        // ── File state ──────────────────────────────────────────────────────────
        private string   _uploadedFilePath;
        private string   _uploadedFileName;
        private long     _uploadedFileSize;
        private DateTime _uploadedTime;

        // ── Constructor ─────────────────────────────────────────────────────────
        public KBImportWizardForm(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
            InitializeComponent();
        }

        /// <summary>
        /// Called by the composition root after the presenter is fully wired.
        /// Must be called before ShowDialog().
        /// </summary>
        public void AttachPresenter(KBImportWizardController presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        // ── Form Load ───────────────────────────────────────────────────────────
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

            // Resize all step panels to match the content container
            pnlContent.Resize += (s, ev) => SyncStepPanelSizes();
            SyncStepPanelSizes();

            ShowStep(1);
        }

        // ── IKBImportWizardView — properties (View → Presenter) ─────────────────

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

        // ── IKBImportWizardView — methods (Presenter → View) ────────────────────

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

        // ── Sync step-panel sizes to fill pnlContent ────────────────────────────
        private void SyncStepPanelSizes()
        {
            var sz = pnlContent.ClientSize;
            pnlStep1.Size = sz;
            pnlStep2.Size = sz;
            pnlStep3.Size = sz;
        }

        // ── Template combo ──────────────────────────────────────────────────────
        private void InitTemplateCombo()
        {
            cboTemplate.Items.Clear();
            cboTemplate.Items.Add("ความรู้ประกันภัย / Knowledge Base");
            cboTemplate.Items.Add("SOP & แนวทาง / Procedures");
            cboTemplate.Items.Add("FAQ / คำถามที่พบบ่อย");
            cboTemplate.Items.Add("แนวทางปฏิบัติ / Guidelines");
            cboTemplate.SelectedIndex = 0;
        }

        // ── Column reference ListView ────────────────────────────────────────────
        private void PopulateColumnReference()
        {
            lvColumnRef.Items.Clear();
            var cols = new[]
            {
                new { L="A", F="ชื่อบทความ",          T="Text (max 200)",                R=true,  E="นโยบายการรับสินไหมทดแทน" },
                new { L="B", F="ประเภทบทความ",        T="Enum: SOP/FAQ/แนวทาง/Template", R=true,  E="SOP" },
                new { L="C", F="หมวดหมู่",            T="Text — ต้องมีในระบบ",            R=true,  E="ความรู้ประกันภัย > UW" },
                new { L="D", F="ระดับความลับ (CRAF)", T="Enum: 1HC/2SC/3NC/4PC",         R=true,  E="3NC" },
                new { L="E", F="แท็ก",                T="Text คั่นด้วย comma",            R=false, E="UW, Claims, OIC" },
                new { L="F", F="ฝ่าย/แผนก",          T="Text คั่นด้วย comma",            R=false, E="สินไหมทดแทน, UW" },
                new { L="G", F="วันหมดอายุ",          T="Date (DD/MM/YYYY)",              R=false, E="20/03/2570" },
                new { L="H", F="เนื้อหาบทความ",      T="Text (HTML หรือ Plain Text)",    R=false, E="เนื้อหาบทความ..." },
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

        // ── Validation items (Step 2) — built programmatically ──────────────────
        private void BuildValidationItems()
        {
            pnlValidationItems.Controls.Clear();
            var items = new[]
            {
                new { Icon="✅", Title="รูปแบบไฟล์: Excel (.xlsx)",       Detail="ไฟล์ถูกต้องและสามารถเปิดได้",                                              Kind="ok"   },
                new { Icon="✅", Title="โครงสร้าง Template: ถูกต้อง",    Detail="พบคอลัมน์ครบตาม Template \"ความรู้ประกันภัย\" — 8 คอลัมน์ (A-H)",        Kind="ok"   },
                new { Icon="✅", Title="คอลัมน์จำเป็น: ครบถ้วน",         Detail="คอลัมน์ ชื่อบทความ, ประเภท, หมวดหมู่, CRAF ครบทุกแถว",                   Kind="ok"   },
                new { Icon="⚠️", Title="พบ 2 คำเตือน",                   Detail="แถวที่ 5: ไม่มีวันหมดอายุ  |  แถวที่ 12: หมวดหมู่ใหม่ (จะสร้างอัตโนมัติ)", Kind="warn" },
                new { Icon="✅", Title="ข้อมูล CRAF: ถูกต้องทุกแถว",    Detail="ค่า CRAF ทุกแถวเป็น 1HC/2SC/3NC/4PC ตามที่กำหนด",                        Kind="ok"   },
            };

            int y = 0;
            foreach (var v in items)
            {
                var pnl = new Panel
                {
                    Location  = new Point(0, y),
                    Size      = new Size(pnlValidationItems.Width, 48),
                    Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    BackColor = v.Kind == "ok"
                        ? Color.FromArgb(240, 255, 244)
                        : Color.FromArgb(254, 249, 231),
                };
                var stripe = new Panel
                {
                    Dock      = DockStyle.Left,
                    Width     = 4,
                    BackColor = v.Kind == "ok"
                        ? Color.FromArgb(39, 174, 96)
                        : Color.FromArgb(243, 156, 18),
                };
                var lblIcon = new Label
                {
                    Location  = new Point(14, 14),
                    Size      = new Size(22, 22),
                    Font      = new Font("Segoe UI", 11f),
                    Text      = v.Icon,
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                var lblTitle = new Label
                {
                    Location  = new Point(40, 6),
                    Size      = new Size(pnl.Width - 50, 18),
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(26, 35, 50),
                    Text      = v.Title,
                    Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                };
                var lblDetail = new Label
                {
                    Location  = new Point(40, 25),
                    Size      = new Size(pnl.Width - 50, 16),
                    Font      = new Font("Segoe UI", 8f),
                    ForeColor = Color.FromArgb(74, 85, 104),
                    Text      = v.Detail,
                    Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                };
                pnl.Controls.Add(stripe);
                pnl.Controls.Add(lblIcon);
                pnl.Controls.Add(lblTitle);
                pnl.Controls.Add(lblDetail);
                pnl.Controls.Add(new Panel
                {
                    Dock      = DockStyle.Bottom,
                    Height    = 1,
                    BackColor = Color.FromArgb(208, 215, 222),
                });
                pnlValidationItems.Controls.Add(pnl);
                y += 50;
            }
            pnlValidationItems.Height = y;
        }

        // ── Step navigation ──────────────────────────────────────────────────────
        private void ShowStep(int step)
        {
            _currentStep = step;

            pnlStep1.Visible = (step == 1);
            pnlStep2.Visible = (step == 2);
            pnlStep3.Visible = (step == 3);

            pnlActStep1.Visible = (step == 1);
            pnlActStep2.Visible = (step == 2);
            pnlActStep3.Visible = (step == 3);

            UpdateStepIndicators();

            lblStepStatus.Text = string.Format("📋  Step {0}/3: {1}", step,
                step == 1 ? "อัพโหลดไฟล์" :
                step == 2 ? "ตรวจสอบข้อมูล" : "นำเข้าเสร็จสิ้น");
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
                circle.BackColor = Color.FromArgb(39, 174, 96);
                circle.ForeColor = Color.White;
                circle.Text      = "✓";
                label.ForeColor  = Color.FromArgb(39, 174, 96);
                label.Font       = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            }
            else if (_currentStep == n)
            {
                circle.BackColor = Color.FromArgb(46, 116, 181);
                circle.ForeColor = Color.White;
                circle.Text      = n.ToString();
                label.ForeColor  = Color.FromArgb(46, 116, 181);
                label.Font       = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            }
            else
            {
                circle.BackColor = Color.White;
                circle.ForeColor = Color.FromArgb(127, 140, 141);
                circle.Text      = n.ToString();
                label.ForeColor  = Color.FromArgb(127, 140, 141);
                label.Font       = new Font("Segoe UI", 8.5f);
            }
        }

        // ── Upload Zone — dashed border paint ───────────────────────────────────
        private void PnlUploadZone_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(180, 195, 215), 2f))
            {
                pen.DashStyle = DashStyle.Dash;
                e.Graphics.DrawRectangle(pen, 1, 1,
                    pnlUploadZone.Width - 3, pnlUploadZone.Height - 3);
            }
        }

        // ── Drag & Drop ──────────────────────────────────────────────────────────
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

        // ── File selection handlers ──────────────────────────────────────────────
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

        // ── Step 1 → Step 2 (delegates to presenter) ─────────────────────────────
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

        private void BtnBack_Click(object sender, EventArgs e) { ShowStep(1); }

        // ── Import (delegates to presenter after animation) ───────────────────────
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

            StartImportAnimation();
        }

        /// <summary>
        /// Plays the progress-bar animation (UI-only), then asks the presenter to
        /// execute the actual import. The presenter calls ShowImportComplete() when done.
        /// </summary>
        private void StartImportAnimation()
        {
            pnlProgress.Visible  = true;
            progressBar.Value    = 0;
            lblProgressText.Text = "กำลังเตรียมข้อมูล...";
            btnImport.Enabled    = false;
            btnBack.Enabled      = false;

            var timer    = new Timer { Interval = 80 };
            int progress = 0;
            timer.Tick += (s, ev) =>
            {
                progress         += 5;
                progressBar.Value = Math.Min(progress, 100);
                lblProgressText.Text = string.Format(
                    "กำลังนำเข้าข้อมูล... {0}%", progressBar.Value);

                if (progress >= 100)
                {
                    timer.Stop();
                    timer.Dispose();
                    pnlProgress.Visible = false;
                    btnImport.Enabled   = true;
                    btnBack.Enabled     = true;

                    // Hand off to presenter — calls ShowImportComplete() on success
                    _presenter.ExecuteImport();
                }
            };
            timer.Start();
        }

        // ── Preview grid ────────────────────────────────────────────────────────
        private void PopulatePreviewGrid(IList<KBImportRow> rows)
        {
            dgvPreview.Rows.Clear();
            if (rows == null) return;

            for (int i = 0; i < rows.Count; i++)
            {
                var r   = rows[i];
                int idx = dgvPreview.Rows.Add(
                    r.RowNumber.ToString(), r.Title, r.ArticleType,
                    r.Category, r.CrafLevel, r.Tags, r.ExpiryDate,
                    r.Status == "Valid" ? "✅  Valid" : "⚠️  Warn");

                var row = dgvPreview.Rows[idx];
                if (r.Status == "Warn")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 249, 231);
                else if (i % 2 == 1)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
            }
        }

        // ── Step 3 buttons ───────────────────────────────────────────────────────
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
            dgvPreview.Rows.Clear();

            ShowStep(1);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────
        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024L)        return bytes + " B";
            if (bytes < 1024L * 1024) return (bytes / 1024.0).ToString("0.0") + " KB";
            return (bytes / (1024.0 * 1024)).ToString("0.0") + " MB";
        }

        // ── Action bar bottom border ─────────────────────────────────────────────
        private void PnlActions_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(208, 215, 222), 1))
                e.Graphics.DrawLine(pen, 0, 0, pnlActions.Width, 0);
        }

        // ── Step progress bottom border ──────────────────────────────────────────
        private void PnlStepProgress_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(208, 215, 222), 1))
                e.Graphics.DrawLine(pen, 0, pnlStepProgress.Height - 1,
                    pnlStepProgress.Width, pnlStepProgress.Height - 1);
        }
    }
}
