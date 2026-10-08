// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Controllers.KMS.KB;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// KMS Import Wizard — นำเข้าข้อมูล Knowledge Base จาก Excel (MOD-006)
    /// 3-Step Wizard:
    ///   Step 1 — อัปโหลดไฟล์ Excel + เลือก Template
    ///   Step 2 — ตรวจสอบข้อมูล + Preview + ตัวเลือกการนำเข้า
    ///   Step 3 — สรุปผลการนำเข้า
    /// Layout ตาม MOD-006/kms_import.html
    /// </summary>
    public partial class KBImportWizardForm : Form, IKBImportWizardController
    {
        public event EventHandler BackRequested;

        #region Fields

        private KBImportWizardController _presenter;
        private readonly UserContext    _user;
        /// <summary>
        /// ขั้นปัจจุบันของ wizard (1-3): 1 = เลือก Template + โฟลเดอร์ไฟล์แนบ/DMS + ไฟล์ Excel (pnlStep1),
        /// 2 = ตรวจสอบข้อมูล (pnlStep2), 3 = นำเข้าเสร็จสิ้น (pnlStep3)
        /// </summary>
        private int      _currentStep = 1;

        // File state
        private string   _uploadedFilePath;
        private string   _uploadedFileName;
        private long     _uploadedFileSize;
        private DateTime _uploadedTime;

        // โฟลเดอร์ไฟล์แนบ (PDF) ที่ผู้ใช้เลือกในเครื่อง — จำเป็นเมื่อ Template ที่เลือก map คอลัมน์ Attachment (ปุ่มนำเข้ากดไม่ได้จนกว่าจะเลือก)
        private string   _attachmentFolderPath;
        private bool     _attachmentFolderRequired;

        // โฟลเดอร์ปลายทางใน DMS ที่ผู้ใช้เลือก (ผ่านการตรวจสิทธิ์ใน KBDmsFolderPickerDialog แล้ว) — จำเป็นพร้อมโฟลเดอร์ไฟล์แนบ
        private int?     _dmsFolderId;
        private string   _dmsFolderPath;
        private Func<Func<int, string>, int, KBDmsFolderPickerDialog> _dmsPickerFactory;

        private static readonly Color ImportButtonEnabledColor  = Color.FromArgb(39, 174, 96);    // เขียวเดิมของ btnImport
        private static readonly Color BrowseButtonEnabledColor  = Color.FromArgb(46, 116, 181);    // น้ำเงินเดิมของปุ่มเลือกโฟลเดอร์/ไฟล์
        private static readonly Color ImportButtonDisabledColor = Color.FromArgb(189, 195, 199);  // เทา — เห็นชัดว่ากดไม่ได้ (FlatStyle.Flat ไม่เปลี่ยนสีพื้นเอง)

        // Preview grid paging
        private const int          GridPageSize = 50;

        /// <summary>ช่องว่างใต้การ์ดแต่ละใบใน Step 1 (ส่วนล่างของ panel ที่ไม่วาดพื้นขาว) และความสูงต่ำสุดของ Step 1 (ส่วนที่เหลือเป็นของตาราง Column)</summary>
        private const int          CardGap = 8;
        private const string       TemplatePlaceholderText = "-- เลือก template --";
        private const string       NoTemplateHint = "เลือก Template เพื่อดูคอลัมน์ที่ต้องมีในไฟล์";
        private const int          Step1MinHeight = 560;
        /// <summary>ความสูงสูงสุดของแถวใน dgvPreview หลังปรับตามข้อความที่ wrap (ราว 4 บรรทัด) — กันเนื้อหายาว ๆ ดันแถวสูงทั้งหน้า ลากขอบแถวขยายเองได้</summary>
        private const int          PreviewMaxRowHeight = 100;
        private IList<KBImportRow> _allRows     = new List<KBImportRow>();
        private int                _gridPage    = 0;

        // Preview grid filter (task 1)
        private IList<KBImportRow> _filteredRows = new List<KBImportRow>();

        // Step 3 — skipped/failed grid paging
        private IList<KBImportRow> _errAllRows;
        private IList<KBImportRow> _errSkipRows;
        private IList<KBImportRow> _errSuccessRows;
        private int                _errCurrentPage;
        private const int          ErrPageSize = 20;

        #endregion

        #region Constructor

        public KBImportWizardForm(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
            InitializeComponent();
            InitPreviewGridColumns();
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

        /// <summary>
        /// Composition root ส่ง factory สร้างหน้าเลือกโฟลเดอร์ DMS (รับ delegate ตรวจสิทธิ์ และ Level เริ่มต้น).
        /// ไม่เรียก = ปุ่มเลือกโฟลเดอร์ DMS แจ้งว่าใช้ไม่ได้
        /// </summary>
        public void AttachDmsFolderPicker(Func<Func<int, string>, int, KBDmsFolderPickerDialog> factory)
        {
            _dmsPickerFactory = factory;
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

            // Template row (label + combo + download button) is hidden by default in the
            // Designer from when the wizard relied purely on layout auto-detection with no
            // Template picker. The file layout (Law/NonLaw) and ArticleType come from the selected Template
            // (see KBImportWizardController.ValidateFile) while CategoryID is resolved per row from the
            // ArticleCode, so the picker must always be shown.
            lblTemplateLabel.Visible    = true;
            cboTemplate.Visible         = true;
            //btnDownloadTemplate.Visible = true;

            // Hint first — the table is filled from the selected Template's mapping once one is chosen
            // (see CboTemplate_SelectedIndexChanged); LoadTemplates() selects the "-- เลือก template --" placeholder.
            PopulateColumnReference(NoTemplateHint);
            cboTemplate.SelectedIndexChanged += CboTemplate_SelectedIndexChanged;
            _presenter.LoadTemplates();

            // Step 1: การ์ด (Template / โฟลเดอร์ไฟล์แนบ / โฟลเดอร์ DMS / ไฟล์ Excel) วาดพื้นขาว + เลขลำดับเอง
            foreach (var card in new[] { pnlTemplateRow, pnlAttachmentFolder, pnlUploadSection })
            {
                card.Paint  += CardPanel_Paint;
                card.Resize += (s, ev) => ((Control)s).Invalidate();
            }
            EnsureCardOrder();
            RenumberCards();
            pnlTemplateRow.Resize          += (s, ev) => LayoutCardRows();
            pnlAttachmentFolder.Resize     += (s, ev) => LayoutCardRows();
            pnlAttachmentFolder.VisibleChanged += (s, ev) => LayoutCardRows();
            LayoutCardRows();
            pnlScroll.Resize    += (s, ev) => AdjustStep1Height();
            lvColumnRef.Resize  += (s, ev) => FitColumnRefColumns();
            AdjustStep1Height();
            FitColumnRefColumns();

            // Wire upload-zone dashed border and drag events
            pnlUploadZone.Paint     += PnlUploadZone_Paint;
            pnlUploadZone.DragEnter += PnlUploadZone_DragEnter;
            pnlUploadZone.DragDrop  += PnlUploadZone_DragDrop;

            // Distribute pill widths evenly on load and whenever the panel resizes
            pnlSummaryPills.Resize   += (s, ev) => DistributePillWidths();
            pnlCompleteStats.Resize  += (s, ev) => DistributeStatWidths();
            DistributePillWidths();
            DistributeStatWidths();

            // Wire summary pill click filters (task 1)
            AddClickRecursive(pnlPillTotal, (s, ev) => ApplyPreviewFilter("Total"));
            AddClickRecursive(pnlPillValid, (s, ev) => ApplyPreviewFilter("Valid"));
            AddClickRecursive(pnlPillSkip,  (s, ev) => ApplyPreviewFilter("Skip"));
            AddClickRecursive(pnlPillError, (s, ev) => ApplyPreviewFilter("Error"));

            // Wire stat panel click filters (task 2)
            AddClickRecursive(pnlStatTotal,   (s, ev) => ApplyStatFilter("Total"));
            AddClickRecursive(pnlStatSuccess, (s, ev) => ApplyStatFilter("Success"));
            AddClickRecursive(pnlStatSkip,    (s, ev) => ApplyStatFilter("Skip"));

            ShowStep(1);
        }

        #endregion

        #region IKBImportWizardController Implementation

        // Properties

        public string SelectedFilePath { get { return _uploadedFilePath; } }
        public int? SelectedTemplateId
        {
            get
            {
                var selected = cboTemplate.SelectedItem as ComboItem;
                return selected != null ? selected.Value : null;
            }
        }

        public string AttachmentFolderPath
        {
            get { return string.IsNullOrWhiteSpace(_attachmentFolderPath) ? null : _attachmentFolderPath; }
        }

        public int? DmsFolderId { get { return _dmsFolderId; } }

        // Methods

        public void SetAttachmentFolderRequired(bool required)
        {
            _attachmentFolderRequired  = required;
            pnlAttachmentFolder.Visible = required;
            EnsureCardOrder();
            RenumberCards();
            UpdateImportButtonState();
        }

        /// <summary>
        /// Binds the KB Import Templates configured in KBImportTemplateConfigForm to
        /// <c>cboTemplate</c>. Called once by the presenter after the form loads.
        /// </summary>
        public void BindTemplateOptions(IEnumerable<KBImportTemplate> templates)
        {
            cboTemplate.Items.Clear();

            // รายการแรกเป็น placeholder (Value = null) — ต้องเลือก Template จริงก่อนจึงไปต่อได้
            cboTemplate.Items.Add(new ComboItem(TemplatePlaceholderText, null));
            foreach (var t in templates)
                cboTemplate.Items.Add(new ComboItem(t.TemplateName, t.TemplateID));

            cboTemplate.SelectedIndex = 0;

            UpdateImportButtonState();   // ยังไม่เลือก Template = เลือกไฟล์ไม่ได้
        }

        /// <summary>
        /// Refreshes the column-reference panel (lvColumnRef) with the selected Template's actual
        /// column mapping. With no template selected (or one that maps no column) it shows a one-line hint instead.
        /// </summary>
        public void BindTemplateColumnReference(IEnumerable<KBImportTemplateColumnRow> columns, string templateName)
        {
            // LayoutType/ArticleType เป็นค่าที่เลือกจาก dropdown ใน Template ไม่ใช่คอลัมน์ Excel จึงไม่แสดงในรายการนี้
            var mapped = columns
                .Where(c => !string.IsNullOrWhiteSpace(c.ExcelColumnRef)
                         && !string.Equals(c.TargetColumnName, KBArticleColumnCatalog.LayoutTypeTargetColumn, StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(c.TargetColumnName, "ArticleType", StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.DisplayOrder).ToList();
            if (string.IsNullOrEmpty(templateName))
            {
                PopulateColumnReference(NoTemplateHint);
                return;
            }
            if (mapped.Count == 0)
            {
                PopulateColumnReference("Template นี้ยังไม่ได้ Map คอลัมน์ Excel — ตั้งค่าที่หน้า config template");
                return;
            }

            lvColumnRef.Items.Clear();
            lvColumnRef.Groups.Clear();

            var group = new ListViewGroup(string.Format("Mapping ของ Template ที่เลือก: {0}", templateName));
            lvColumnRef.Groups.Add(group);

            foreach (var col in mapped)
            {
                var item = new ListViewItem(col.ExcelColumnRef);
                item.Group = group;
                item.SubItems.Add(col.TargetColumnName);
                item.SubItems.Add(string.Empty);
                item.SubItems.Add(string.Empty);
                item.SubItems.Add(col.ColumnDescription);
                lvColumnRef.Items.Add(item);
            }
        }

        /// <summary>
        /// Receives validation results from the presenter, updates summary pills and
        /// preview grid, and advances the wizard to Step 2.
        /// </summary>
        public void ShowValidationResult(KBImportValidationResult result)
        {
            if (result == null) return;

            lblTotalCount.Text = result.TotalRecords.ToString();
            lblValidCount.Text = result.ValidRecords.ToString();
            lblSkipCount.Text  = result.SkippedRecords.ToString();
            lblErrorCount.Text = result.ErrorRecords.ToString();

            btnImport.Text = string.Format(
                "📥  นำเข้า ({0} รายการ)", result.ValidRecords);

            PopulatePreviewGrid(result.Rows);
            ShowStep(2);
            SetStatus("ตรวจสอบข้อมูลเรียบร้อย");
            UpdateImportButtonState();
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

            var skipped = summary.SkippedRows      ?? new List<KBImportRow>();
            var failed  = summary.ImportFailedRows ?? new List<KBImportRow>();
            var warned  = (summary.Rows ?? new List<KBImportRow>()).Where(r => r.Status == "Warn").ToList();

            var allProblemRows = new List<KBImportRow>(skipped);
            allProblemRows.AddRange(failed);
            allProblemRows.AddRange(warned);

            // "Skip" stat panel = everything not fully successful (Skip + Error + Warn rows).
            lblCompleteSkipNum.Text = allProblemRows.Count.ToString();

            // Derive success rows for stat filter (task 2)
            _errSkipRows = allProblemRows;
            // อ้างด้วย instance ไม่ใช่ RowNumber — Excel row เดียวที่แตกเป็นหลาย ArticleCode ใช้ RowNumber เดียวกันแต่ผ่าน/ล้มเหลวแยกกัน
            var problemRows = new HashSet<KBImportRow>(allProblemRows);
            _errSuccessRows = (_allRows ?? new List<KBImportRow>())
                .Where(r => !problemRows.Contains(r))
                .ToList();

            bool hasProblemRows = allProblemRows.Count > 0;

            if (hasProblemRows)
            {
                lblCompleteIcon.Text  = "⚠️";
                lblCompleteTitle.Text = string.Format(
                    "นำเข้าสำเร็จ {0} รายการ  /  ข้าม {1} รายการ",
                    summary.ValidRecords, allProblemRows.Count);
                lblCompleteTitle.ForeColor = Color.FromArgb(192, 57, 43);
                lblCompleteSub.Text        = "รายการด้านล่างถูกข้ามหรือล้มเหลว — ตรวจสอบสาเหตุและแก้ไขไฟล์ก่อนนำเข้าใหม่";

                PopulateErrorGrid(allProblemRows);
                SetPillActive(pnlStatTotal,   false);
                SetPillActive(pnlStatSuccess, false);
                SetPillActive(pnlStatSkip,    true);
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
            if (ext == ".xlsx" || ext == ".xls" || ext == ".xlsm")
                HandleFileSelection(files[0]);
            else
                MessageBox.Show("กรุณาเลือกไฟล์ Excel (.xlsx หรือ .xls หรือ .xlsm) เท่านั้น",
                    "ไฟล์ไม่ถูกต้อง", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // File Selection Events

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel Files|*.xlsx;*.xls;*.xlsm";
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
            SetStatus("ลบไฟล์แล้ว");
        }

        private void CboTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_presenter != null)
                _presenter.OnTemplateSelected();
        }

        private void BtnDownloadTemplate_Click(object sender, EventArgs e)
        {
            // cboTemplate now lists admin-configured Templates (KBImportTemplateConfigForm), not
            // a fixed 2-item layout picker — so which blank sample file to hand out is asked
            // directly here instead, decoupled from the Template selection.
            var layoutChoice = MessageBox.Show(
                "เลือกรูปแบบไฟล์ตัวอย่างที่ต้องการดาวน์โหลด\n\n" +
                "กด \"Yes\" สำหรับ กฎหมาย (มาตรา)\nกด \"No\" สำหรับ เอกสารที่ไม่ใช่กฎหมาย (ประกาศ/คำสั่ง/ระเบียบ ฯลฯ)",
                "เลือกรูปแบบ Template", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (layoutChoice == DialogResult.Cancel) return;
            bool isOrder = layoutChoice == DialogResult.No;

            using (var dlg = new SaveFileDialog())
            {
                dlg.Title            = "บันทึก Template";
                dlg.FileName         = isOrder ? "template_ประกาศคำสั่ง.xlsx" : "template_กฎหมาย.xlsx";
                dlg.Filter           = "Excel Workbook (*.xlsx)|*.xlsx";
                dlg.DefaultExt       = "xlsx";

                if (dlg.ShowDialog() != DialogResult.OK) return;

                var resourceName = isOrder
                    ? "CMS.Presentation.template_order.xlsx"
                    : "CMS.Presentation.template.xlsx";
                using (var src = System.Reflection.Assembly.GetExecutingAssembly()
                                       .GetManifestResourceStream(resourceName))
                {
                    if (src == null)
                    {
                        MessageBox.Show("ไม่พบไฟล์ Template ในระบบ\n\nกรุณาติดต่อผู้ดูแลระบบ",
                            "ไม่พบไฟล์", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var buf = new byte[src.Length];
                    src.Read(buf, 0, buf.Length);
                    System.IO.File.WriteAllBytes(dlg.FileName, buf);
                }

                SetStatus(string.Format("✅  บันทึก Template ไปที่ \"{0}\" เรียบร้อย", dlg.FileName));
            }
        }

        private void BtnBrowseAttachmentFolder_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description         = "เลือกโฟลเดอร์ที่เก็บไฟล์แนบ (PDF) — ชื่อไฟล์ในโฟลเดอร์ต้องตรงกับชื่อไฟล์ในคอลัมน์ Attachment ของ Excel (รวมนามสกุล)";
                dlg.ShowNewFolderButton = false;
                if (!string.IsNullOrWhiteSpace(_attachmentFolderPath) && Directory.Exists(_attachmentFolderPath))
                    dlg.SelectedPath = _attachmentFolderPath;

                if (dlg.ShowDialog() != DialogResult.OK) return;

                _attachmentFolderPath = dlg.SelectedPath;
                UpdateImportButtonState();
                SetStatus(string.Format("📂  เลือกโฟลเดอร์ไฟล์แนบ \"{0}\" แล้ว", _attachmentFolderPath));
            }
        }

        private void BtnBrowseDmsFolder_Click(object sender, EventArgs e)
        {
            if (_dmsPickerFactory == null || _presenter == null)
            {
                MessageBox.Show("ไม่สามารถเลือกโฟลเดอร์ DMS ได้ — ระบบจัดการโฟลเดอร์ยังไม่พร้อมใช้งาน",
                    "ไม่พร้อมใช้งาน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dlg = _dmsPickerFactory(_presenter.CheckDmsFolder, 4))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                _dmsFolderId   = dlg.SelectedFolderId;
                _dmsFolderPath = dlg.SelectedFolderPath;
                UpdateImportButtonState();
                SetStatus(string.Format("🗄  เลือกโฟลเดอร์ปลายทางใน DMS \"{0}\" แล้ว", _dmsFolderPath));
            }
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

            // ปุ่มถูก disable อยู่แล้วเมื่อยังไม่ได้เลือกโฟลเดอร์ — กันซ้ำเผื่อถูกเรียกทางอื่น
            if (_attachmentFolderRequired && string.IsNullOrWhiteSpace(_attachmentFolderPath))
            {
                MessageBox.Show("กรุณาเลือกโฟลเดอร์ไฟล์แนบ (PDF) ก่อนนำเข้า",
                    "ยังไม่ได้เลือกโฟลเดอร์ไฟล์แนบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_attachmentFolderRequired && !_dmsFolderId.HasValue)
            {
                MessageBox.Show("กรุณาเลือกโฟลเดอร์ปลายทางใน DMS ก่อนนำเข้า",
                    "ยังไม่ได้เลือกโฟลเดอร์ปลายทาง", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                string.Format(
                    "ต้องการนำเข้าข้อมูล {0} รายการ ใช่หรือไม่?\n\n" +
                    "✅  พร้อมนำเข้า :  {1} รายการ\n" +
                    "⏭  ข้าม :  {2} รายการ\n" +
                    "⛔  ข้อผิดพลาด :  {3} รายการ (จะข้าม)",
                    lblTotalCount.Text, lblValidCount.Text, lblSkipCount.Text, lblErrorCount.Text),
                "ยืนยันการนำเข้า", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            StartImport();
        }

        private void BtnBackToMain_Click(object sender, EventArgs e)
        {
            if (BackRequested != null)
                BackRequested(this, EventArgs.Empty);
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            if (BackRequested != null)
                BackRequested(this, EventArgs.Empty);
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (BackRequested != null)
            {
                BackRequested(this, EventArgs.Empty);
                return;
            }
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
            // Reset wizard back to the first step (เลือก Template + ไฟล์)
            _uploadedFilePath = null;
            _uploadedFileName = null;
            _uploadedFileSize = 0;

            pnlUploadedFile.Visible = false;
            pnlUploadZone.Visible   = true;
            btnNext.Enabled         = false;
            _allRows      = new List<KBImportRow>();
            _filteredRows = new List<KBImportRow>();
            _gridPage     = 0;
            dgvPreview.Rows.Clear();
            UpdatePagerLabels();

            pnlStep3.Height        = 460;
            pnlErrorDetail.Visible = false;
            _errAllRows            = null;
            _errSkipRows           = null;
            _errSuccessRows        = null;
            _errCurrentPage        = 0;
            SetPillActive(pnlPillTotal,   false);
            SetPillActive(pnlPillValid,   false);
            SetPillActive(pnlPillSkip,    false);
            SetPillActive(pnlPillError,   false);
            SetPillActive(pnlStatTotal,   false);
            SetPillActive(pnlStatSuccess, false);
            SetPillActive(pnlStatSkip,    false);

            ShowStep(1);
        }

        // Preview grid pager events

        private void BtnGridPrev_Click(object sender, EventArgs e)
        {
            if (_gridPage > 0) { _gridPage--; RefreshGridPage(); }
        }

        private void BtnGridNext_Click(object sender, EventArgs e)
        {
            int maxPage = MaxPage(_allRows.Count, GridPageSize);
            if (_gridPage < maxPage) { _gridPage++; RefreshGridPage(); }
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
            DrawStepCircle(g, lblStep1Circle, _currentStep == 1, _currentStep > 1);
            DrawStepCircle(g, lblStep2Circle, _currentStep == 2, _currentStep > 2);
            DrawStepCircle(g, lblStep3Circle, _currentStep == 3, _currentStep > 3);
        }

        /// <summary>วาดวงกลมสีทับ label ตัวเลขของ step หนึ่งขั้นในแถบ step progress (เขียว=เสร็จแล้ว, น้ำเงิน=กำลังทำ, เทา=ยังไม่ถึง)</summary>
        private static void DrawStepCircle(Graphics g, Label lbl, bool active, bool done)
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

        #endregion

        #region Helper Methods

        // Preview Grid Column Initialization

        private void InitPreviewGridColumns()
        {
            var colNo             = new DataGridViewTextBoxColumn { Name = "colNo",             HeaderText = "#",              MinimumWidth = 30,  Width = 90,  ReadOnly = true };   // กว้างพอสำหรับ "5 (2/3)" (ArticleCode ที่แตกจาก Excel row เดียวกัน)
            var colStatus         = new DataGridViewTextBoxColumn { Name = "colStatus",         HeaderText = "สถานะ",          MinimumWidth = 40,  Width = 90,  ReadOnly = true };
            var colKind           = new DataGridViewTextBoxColumn { Name = "colKind",           HeaderText = "ประเภท",         MinimumWidth = 60,  Width = 130, ReadOnly = true, Visible = false };   // ซ่อนไว้ก่อน (แสดงผลอย่างเดียว) — ยังต้องอยู่ในลำดับคอลัมน์เพราะ Rows.Add ส่งค่าตามตำแหน่ง
            var colNote           = new DataGridViewTextBoxColumn { Name = "colNote",           HeaderText = "หมายเหตุ",       MinimumWidth = 60,  Width = 100, ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Resizable = DataGridViewTriState.True};
            var colArticleCode    = new DataGridViewTextBoxColumn { Name = "colArticleCode",    HeaderText = "ArticleCode",    MinimumWidth = 60,  Width = 190, ReadOnly = true };
            var colTitle          = new DataGridViewTextBoxColumn { Name = "colTitle",          HeaderText = "ชื่อ / Tags",     MinimumWidth = 40,  Width = 160, ReadOnly = true };
            var colSummary        = new DataGridViewTextBoxColumn { Name = "colSummary",        HeaderText = "Summary",        MinimumWidth = 80,  Width = 160, ReadOnly = true };
            var colPublished      = new DataGridViewTextBoxColumn { Name = "colPublished",      HeaderText = "วันที่ประกาศ",   MinimumWidth = 60,  Width = 100, ReadOnly = true };
            var colEffective      = new DataGridViewTextBoxColumn { Name = "colEffective",      HeaderText = "วันที่บังคับใช้", MinimumWidth = 60, Width = 110, ReadOnly = true };
            var colTitleCategory  = new DataGridViewTextBoxColumn { Name = "colTitleCategory",  HeaderText = "TitleCategory",  MinimumWidth = 60,  Width = 140, ReadOnly = true };
            var colCategoryId     = new DataGridViewTextBoxColumn { Name = "colCategoryId",     HeaderText = "CategoryID",     MinimumWidth = 40,  Width = 80,  ReadOnly = true, Visible = false };   // ซ่อนไว้ก่อน (แสดงผลอย่างเดียว) — แสดงชื่อหมวดที่ colCategoryName แทน
            var colCategoryName   = new DataGridViewTextBoxColumn { Name = "colCategoryName",   HeaderText = "CategoryName",   MinimumWidth = 60,  Width = 150, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } };
            var colContentHtml    = new DataGridViewTextBoxColumn { Name = "colContentHtml",    HeaderText = "เนื้อหา", MinimumWidth = 60, Width = 300, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } };

            // ลำดับคอลัมน์ต้องตรงกับลำดับค่าที่ส่งเข้า dgvPreview.Rows.Add(...) ใน RefreshGridPage (ส่งค่าตามตำแหน่ง)
            dgvPreview.Columns.AddRange(new DataGridViewColumn[] {
                colNo, colStatus, colKind, colNote, colArticleCode,
                colTitle, colTitleCategory, colContentHtml, colSummary,
                colPublished, colEffective, colCategoryId, colCategoryName
            });

            // หน้า Step 2 เติมแถวตอนยังซ่อนอยู่ — ปรับความสูงแถวอีกครั้งเมื่อ grid ถูกแสดง
            dgvPreview.VisibleChanged += (s, e) => ResizePreviewRows();
        }

        // Template combo item — wraps a KBImportTemplate for display in cboTemplate (see BindTemplateOptions)

        private class ComboItem
        {
            public string Display { get; private set; }
            public int? Value { get; private set; }

            public ComboItem(string display, int? value)
            {
                Display = display;
                Value = value;
            }

            public override string ToString()
            {
                return Display;
            }
        }
        // Column Reference Population

        /// <summary>
        /// แสดงข้อความแนะนำแถวเดียวในตารางรูปแบบ Column (ยังไม่เลือก Template หรือ Template ไม่ได้ Map คอลัมน์ใด) —
        /// ตารางจริงมาจาก mapping ของ Template ที่เลือกเท่านั้น (<see cref="BindTemplateColumnReference"/>)
        /// </summary>
        private void PopulateColumnReference(string hint)
        {
            lvColumnRef.Items.Clear();
            lvColumnRef.Groups.Clear();

            var item = new ListViewItem(string.Empty);
            item.SubItems.Add(hint);
            item.UseItemStyleForSubItems = false;
            item.SubItems[1].ForeColor   = Color.FromArgb(127, 140, 141);
            lvColumnRef.Items.Add(item);
        }

        // Step Navigation

        /// <summary>
        /// Shows only the current step panel and hides others.
        /// Step 1: Show pnlStep1 only (เลือก Template → โฟลเดอร์ไฟล์แนบในเครื่อง → โฟลเดอร์ DMS → ไฟล์ Excel)
        /// Step 2: Show pnlStep2 only (ตรวจสอบข้อมูล)
        /// Step 3: Show pnlStep3 only (นำเข้าเสร็จสิ้น)
        /// </summary>
        private void ShowStep(int step)
        {
            _currentStep = step;
            if (step == 1) AdjustStep1Height();

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
                step == 1 ? "เลือก Template และไฟล์" :
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
            if (!SelectedTemplateId.HasValue)
            {
                MessageBox.Show("กรุณาเลือก Template ก่อนเลือกไฟล์ Excel",
                    "ยังไม่ได้เลือก Template", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!AreAttachmentFoldersChosen())
            {
                MessageBox.Show("กรุณาเลือกโฟลเดอร์ไฟล์แนบในเครื่อง และโฟลเดอร์ปลายทางใน DMS ก่อนเลือกไฟล์ Excel",
                    "ยังไม่ได้เลือกโฟลเดอร์", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
                "Excel Workbook  •  {0}  •  อัปโหลดเมื่อ {1} น.",
                FormatFileSize(_uploadedFileSize),
                _uploadedTime.ToString("HH:mm"));

            UpdateImportButtonState();   // btnNext เปิดเมื่อมีไฟล์ และโฟลเดอร์ที่ต้องใช้เลือกครบ
            SetStatus(string.Format("✅  อัปโหลดไฟล์ \"{0}\" สำเร็จ", _uploadedFileName));
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
            UpdateImportButtonState();
            btnBack.Enabled     = true;
        }

        /// <summary>
        /// ปุ่มนำเข้ากดได้เมื่อไม่ต้องใช้ไฟล์แนบ หรือเลือกครบทั้งโฟลเดอร์ไฟล์แนบ (ในเครื่อง) และโฟลเดอร์ปลายทางใน DMS
        /// (โฟลเดอร์ DMS ผ่านการตรวจสิทธิ์ใน dialog แล้ว และ controller ตรวจซ้ำก่อนนำเข้า); ช่องโฟลเดอร์แสดงที่เลือก (หัวข้อสีแดงเมื่อยังไม่ได้เลือก)
        /// </summary>
        private void UpdateImportButtonState()
        {
            bool folderChosen = !string.IsNullOrWhiteSpace(_attachmentFolderPath);

            txtAttachmentFolder.Text = _attachmentFolderPath ?? string.Empty;
            lblAttachmentFolder.ForeColor = folderChosen
                ? Color.FromArgb(30, 58, 95)
                : Color.FromArgb(192, 57, 43);

            bool dmsChosen = _dmsFolderId.HasValue;

            txtDmsFolder.Text = _dmsFolderPath ?? string.Empty;
            lblDmsFolder.ForeColor = dmsChosen
                ? Color.FromArgb(30, 58, 95)
                : Color.FromArgb(192, 57, 43);

            bool canImport = !_attachmentFolderRequired || (folderChosen && dmsChosen);
            btnImport.Enabled   = canImport;
            btnImport.BackColor = canImport ? ImportButtonEnabledColor : ImportButtonDisabledColor;

            // ลำดับที่ Step 2: โฟลเดอร์ไฟล์แนบในเครื่อง → โฟลเดอร์ DMS → ไฟล์ Excel (แต่ละขั้นเปิดเมื่อขั้นก่อนหน้าเลือกแล้ว)
            bool localOk = !_attachmentFolderRequired || folderChosen;
            btnBrowseDmsFolder.Enabled   = localOk;
            btnBrowseDmsFolder.BackColor = localOk ? BrowseButtonEnabledColor : ImportButtonDisabledColor;

            // ไฟล์ Excel เลือกได้เมื่อมี Template และโฟลเดอร์ที่ต้องใช้ครบ
            bool templateOk = SelectedTemplateId.HasValue;
            lblTemplateLabel.ForeColor = templateOk ? Color.FromArgb(30, 58, 95) : Color.FromArgb(192, 57, 43);   // แดง = ต้องเลือก
            bool foldersOk  = templateOk && AreAttachmentFoldersChosen();
            btnSelectFile.Enabled    = foldersOk;
            btnSelectFile.BackColor  = foldersOk ? BrowseButtonEnabledColor : ImportButtonDisabledColor;
            pnlUploadZone.AllowDrop  = foldersOk;
            btnNext.Enabled          = foldersOk && !string.IsNullOrEmpty(_uploadedFilePath);
        }

        /// <summary>
        /// บังคับลำดับการ์ดใน Step 1 จากบนลงล่าง: Template → โฟลเดอร์ไฟล์แนบ/DMS → ไฟล์ Excel → ตาราง Column (Fill)
        /// (control ที่ Dock = Top เรียงตามลำดับ z — สลับลำดับเองเมื่อแสดง/ซ่อนการ์ด จึงตั้งใหม่ทุกครั้งที่สลับ)
        /// </summary>
        private void EnsureCardOrder()
        {
            pnlTemplateRow.BringToFront();
            pnlAttachmentFolder.BringToFront();
            pnlUploadSection.BringToFront();
            pnlColumnRef.BringToFront();
        }

        /// <summary>เลขลำดับบนการ์ดของ Step 1: Template = 1; ถ้า Template มีไฟล์แนบ โฟลเดอร์ไฟล์แนบ/DMS = 2, ไฟล์ Excel = 3 ไม่งั้นไฟล์ Excel = 2</summary>
        private void RenumberCards()
        {
            pnlTemplateRow.Tag      = 1;
            pnlAttachmentFolder.Tag = 2;
            pnlUploadSection.Tag    = _attachmentFolderRequired ? 3 : 2;

            pnlTemplateRow.Invalidate();
            pnlAttachmentFolder.Invalidate();
            pnlUploadSection.Invalidate();
        }

        /// <summary>
        /// จัดตำแหน่งขวาสุดของแถวในการ์ด (combo Template, ช่องโฟลเดอร์ + ปุ่มเลือก) ให้ห่างขอบการ์ด 16 px เสมอ — Anchor ของ control ในการ์ดที่ซ่อนอยู่
        /// ไม่ตามขนาดที่เปลี่ยนไประหว่างที่ซ่อน จึงคำนวณเองทุกครั้งที่การ์ดเปลี่ยนขนาด/ถูกแสดง
        /// </summary>
        private void LayoutCardRows()
        {
            const int rightMargin = 16;

            cboTemplate.Width = Math.Max(200, pnlTemplateRow.ClientSize.Width - rightMargin - cboTemplate.Left);
            btnDownloadTemplate.Left = pnlTemplateRow.ClientSize.Width - rightMargin - btnDownloadTemplate.Width;

            int right = pnlAttachmentFolder.ClientSize.Width - rightMargin;
            btnBrowseAttachmentFolder.Left = right - btnBrowseAttachmentFolder.Width;
            txtAttachmentFolder.Width      = Math.Max(100, btnBrowseAttachmentFolder.Left - 8 - txtAttachmentFolder.Left);
            btnBrowseDmsFolder.Left        = right - btnBrowseDmsFolder.Width;
            txtDmsFolder.Width             = Math.Max(100, btnBrowseDmsFolder.Left - 8 - txtDmsFolder.Left);
        }

        /// <summary>Step 1 สูงเต็มพื้นที่ที่มี (อย่างน้อย <see cref="Step1MinHeight"/>) เพื่อให้ตารางรูปแบบ Column ใช้พื้นที่ที่เหลือทั้งหมดไม่ต้องเลื่อนในตาราง</summary>
        private void AdjustStep1Height()
        {
            int h = Math.Max(Step1MinHeight, pnlScroll.ClientSize.Height);
            if (pnlStep1.Height != h)
                pnlStep1.Height = h;
        }

        /// <summary>ขยายคอลัมน์สุดท้ายของตารางรูปแบบ Column ให้เต็มความกว้าง</summary>
        private void FitColumnRefColumns()
        {
            int fixedWidth = colColLetter.Width + colColField.Width + colColType.Width + colColRequired.Width;
            int available  = lvColumnRef.ClientSize.Width - fixedWidth;
            if (available > 250)
                colColExample.Width = available;
        }

        /// <summary>
        /// วาดการ์ดของหัวข้อใน Step 1: พื้นขาว ขอบบาง แถบสีน้ำเงินด้านซ้าย และวงกลมเลขลำดับ (เลขจาก <see cref="Control.Tag"/>)
        /// — panel เป็น Transparent ส่วนล่าง <see cref="CardGap"/> px ไม่วาดจึงเป็นช่องว่างระหว่างการ์ด
        /// </summary>
        private void CardPanel_Paint(object sender, PaintEventArgs e)
        {
            var p = (Control)sender;
            int h = p.Height - CardGap;
            if (h <= 8 || p.Width <= 8) return;

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var card = new Rectangle(0, 0, p.Width - 1, h - 1);
            using (var fill = new SolidBrush(Color.White))
                g.FillRectangle(fill, card);
            using (var pen = new Pen(Color.FromArgb(208, 215, 222)))
                g.DrawRectangle(pen, card);
            using (var accent = new SolidBrush(BrowseButtonEnabledColor))
                g.FillRectangle(accent, 0, 0, 4, h);

            if (!(p.Tag is int)) return;

            var badge = new Rectangle(16, 9, 28, 28);
            using (var circle = new SolidBrush(BrowseButtonEnabledColor))
                g.FillEllipse(circle, badge);
            using (var font = new Font("Segoe UI", 11f, FontStyle.Bold))
                TextRenderer.DrawText(g, ((int)p.Tag).ToString(), font, badge, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        /// <summary>true เมื่อไม่ต้องใช้ไฟล์แนบ หรือเลือกครบทั้งโฟลเดอร์ในเครื่องและโฟลเดอร์ปลายทางใน DMS แล้ว</summary>
        private bool AreAttachmentFoldersChosen()
        {
            return !_attachmentFolderRequired
                || (!string.IsNullOrWhiteSpace(_attachmentFolderPath) && _dmsFolderId.HasValue);
        }

        // Preview Grid Population

        private void PopulatePreviewGrid(IList<KBImportRow> rows)
        {
            _allRows = (rows ?? new List<KBImportRow>())
                .OrderBy(r => StatusSortOrder(r.Status))
                .ThenBy(r => r.RowNumber)
                .ThenBy(r => r.CodeIndex)
                .ToList();
            _filteredRows = _allRows;
            _gridPage = 0;
            RefreshGridPage();
            SetPillActive(pnlPillTotal, false);
            SetPillActive(pnlPillValid, false);
            SetPillActive(pnlPillSkip,  false);
            SetPillActive(pnlPillError, false);
        }

        private void RefreshGridPage()
        {
            dgvPreview.SuspendLayout();
            dgvPreview.Rows.Clear();

            try
            {
                int start = _gridPage * GridPageSize;
                int end   = Math.Min(start + GridPageSize, _filteredRows.Count);

                for (int i = start; i < end; i++)
                {
                    var r = _filteredRows[i];
                    try
                    {
                        // DocumentTypeLabel is only set once a row resolves successfully (see
                        // KBImportProcessor.ValidateLawRow/ValidateNonLawRow) — blank for any row
                        // that didn't resolve (empty ColA, bad type digit, etc.) instead of
                        // falling back to the raw RowKind value. DocumentTypeCode = the category found from the
                        // row's ArticleCode (e.g. RVA / OR) — shown so the user can verify the category before importing.
                        string kindDisplay = r.DocumentTypeLabel ?? string.Empty;
                        if (kindDisplay.Length > 0 && !string.IsNullOrEmpty(r.DocumentTypeCode))
                            kindDisplay += " (" + r.DocumentTypeCode + ")";

                        // ค่าต้องเรียงตามลำดับคอลัมน์ใน InitPreviewGridColumns (Rows.Add ส่งค่าตามตำแหน่ง)
                        int idx = dgvPreview.Rows.Add(
                            FormatRowLabel(r),
                            StatusLabel(r.Status),
                            S(kindDisplay),
                            FormatPreviewNote(r),
                            S(r.ArticleCode ?? r.ColA),
                            S(r.Title ?? r.Tags),
                            S(r.TitleCategory),
                            S(r.RawContent, 300),
                            S(r.Summary),
                            // Preview mirrors the Excel cell exactly (RawPublishedDate/RawEffectiveDate,
                            // e.g. พ.ศ. as typed) — PublishedDateParsed/EffectiveDateParsed (already
                            // converted to ค.ศ.) still drive the actual insert into t_KBArticle unchanged.
                            S(r.RawPublishedDate),
                            S(r.RawEffectiveDate),
                            r.ResolvedCategoryID > 0 ? r.ResolvedCategoryID.ToString() : string.Empty,
                            S(r.ResolvedCategoryName));

                        var dgvRow = dgvPreview.Rows[idx];
                        dgvRow.Cells["colStatus"].ToolTipText = FormatPreviewNote(r);   // เหตุผล/หมายเหตุของแถว (คอลัมน์หมายเหตุซ่อนอยู่) — เช่น สิ่งที่จะเติมให้บทความที่มีอยู่แล้ว
                        if (r.Status == "Error")
                            dgvRow.DefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236);
                        else if (r.Status == "Skip")
                            dgvRow.DefaultCellStyle.BackColor = Color.FromArgb(235, 244, 253);
                        else if (r.Status == "Warn")
                            dgvRow.DefaultCellStyle.BackColor = Color.FromArgb(254, 249, 231);
                        else if ((i - start) % 2 == 1)
                            dgvRow.DefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
                    }
                    catch
                    {
                        // One row that fails to render must not stop subsequent rows from
                        // being added. Continue the loop — the grid skips this row silently.
                    }
                }
            }
            finally
            {
                dgvPreview.ResumeLayout();
            }

            ResizePreviewRows();
            UpdatePagerLabels();
        }

        /// <summary>
        /// ปรับความสูงแถวของ dgvPreview ตามข้อความที่ wrap (คอลัมน์ CategoryName, เนื้อหา) ไม่งั้นเห็นแค่บรรทัดแรก (RowTemplate.Height = 26)
        /// แต่จำกัดไม่เกิน <see cref="PreviewMaxRowHeight"/> กันเนื้อหายาว ๆ ดันแถวสูงทั้งหน้า. ทำได้เฉพาะตอน grid มองเห็นอยู่
        /// (ตอนซ่อนอยู่ WinForms ไม่คำนวณความสูงให้) จึงเรียกจาก VisibleChanged ด้วย — ดู InitPreviewGridColumns
        /// </summary>
        private void ResizePreviewRows()
        {
            if (!dgvPreview.Visible || dgvPreview.Rows.Count == 0) return;

            dgvPreview.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
            foreach (DataGridViewRow gridRow in dgvPreview.Rows)
            {
                if (gridRow.Height > PreviewMaxRowHeight)
                    gridRow.Height = PreviewMaxRowHeight;
            }
        }

        private void UpdatePagerLabels()
        {
            int gridTotal = TotalPages(_filteredRows.Count, GridPageSize);

            lblGridPage.Text = string.Format("หน้า {0} / {1}  ({2} รายการ)",
                _gridPage + 1, gridTotal, _filteredRows.Count);

            btnGridPrev.Enabled = _gridPage > 0;
            btnGridNext.Enabled = _gridPage < gridTotal - 1;
        }

        private static int TotalPages(int count, int pageSize)
        {
            return count == 0 ? 1 : (int)Math.Ceiling(count / (double)pageSize);
        }

        private static int MaxPage(int count, int pageSize)
        {
            return Math.Max(0, TotalPages(count, pageSize) - 1);
        }

        private static string StatusLabel(string status)
        {
            if (status == "Valid") return "✅  Valid";
            if (status == "Skip")  return "⏭  ข้าม";
            if (status == "Warn")  return "⚠️  Warn";
            if (status == "Error") return "❌  Error";
            return "⏳";
        }

        private static int StatusSortOrder(string status)
        {
            if (status == "Error") return 0;
            if (status == "Warn")  return 1;
            if (status == "Skip")  return 2;
            return 3;
        }

        private static string S(string value, int maxLen = 500)
        {
            if (value == null) return string.Empty;
            return value.Length <= maxLen ? value : value.Substring(0, maxLen) + "…";
        }

        /// <summary>เลขแถว Excel — Excel row ที่แตกเป็นหลาย ArticleCode แสดงเป็น "5 (2/3)"</summary>
        private static string FormatRowLabel(KBImportRow r)
        {
            return r.CodeCount > 1
                ? string.Format("{0} ({1}/{2})", r.RowNumber, r.CodeIndex, r.CodeCount)
                : r.RowNumber.ToString();
        }

        private static string FormatPreviewNote(KBImportRow r)
        {
            // Valid ปกติไม่มี note; Valid ของบทความที่มีอยู่แล้ว (จะอัปเดต) แสดง note ว่าจะอัปเดต field ไหน
            if (r.Status == "Valid" && r.ExistingArticleId <= 0) return string.Empty;

            string note = r.StatusNote ?? string.Empty;
            if (string.IsNullOrEmpty(note))
                return r.Status == "Error" ? "ข้อผิดพลาดไม่ระบุ" : string.Empty;

            // Skip and Error: StatusNote is already a user-facing, semicolon-joined
            // list of specific Thai validation messages produced by KBImportProcessor —
            // surface it directly.
            return S(note, 300);
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

            // Pills use DockStyle.Left, so order matters: Total → Valid → Skip → Error
            pnlPillTotal.Width = quarter;
            pnlPillValid.Width = quarter;
            pnlPillSkip.Width  = quarter;
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

            int third     = total / 3;
            int remainder = total - third * 3;

            // Stat panels use DockStyle.Left: Total → Success → Skip
            pnlStatTotal.Width   = third;
            pnlStatSuccess.Width = third;
            pnlStatSkip.Width    = third + remainder;
        }

        // ── Pill / Stat click-to-filter helpers ─────────────────────────────

        private static void AddClickRecursive(Control ctrl, EventHandler handler)
        {
            ctrl.Click  += handler;
            ctrl.Cursor  = Cursors.Hand;
            foreach (Control child in ctrl.Controls)
                AddClickRecursive(child, handler);
        }

        private static void SetPillActive(Panel pnl, bool active)
        {
            pnl.BorderStyle = active ? BorderStyle.FixedSingle : BorderStyle.None;
        }

        private void ApplyPreviewFilter(string filter)
        {
            switch (filter)
            {
                case "Valid":
                    _filteredRows = _allRows.Where(r => r.Status == "Valid").ToList();
                    break;
                case "Skip":
                    _filteredRows = _allRows.Where(r => r.Status == "Skip").ToList();
                    break;
                case "Error":
                    _filteredRows = _allRows.Where(r => r.Status == "Error").ToList();
                    break;
                default: // Total
                    _filteredRows = _allRows;
                    break;
            }
            _gridPage = 0;
            RefreshGridPage();
            SetPillActive(pnlPillTotal, filter == "Total");
            SetPillActive(pnlPillValid, filter == "Valid");
            SetPillActive(pnlPillSkip,  filter == "Skip");
            SetPillActive(pnlPillError, filter == "Error");
        }

        private void ApplyStatFilter(string filter)
        {
            switch (filter)
            {
                case "Success":
                    _errAllRows = _errSuccessRows ?? new List<KBImportRow>();
                    break;
                case "Skip":
                    _errAllRows = _errSkipRows ?? new List<KBImportRow>();
                    break;
                default: // Total
                    var all = new List<KBImportRow>(_errSuccessRows ?? new List<KBImportRow>());
                    all.AddRange(_errSkipRows ?? new List<KBImportRow>());
                    _errAllRows = all.OrderBy(r => r.RowNumber).ThenBy(r => r.CodeIndex).ToList();
                    break;
            }
            ShowErrorPage(0);
            SetPillActive(pnlStatTotal,   filter == "Total");
            SetPillActive(pnlStatSuccess, filter == "Success");
            SetPillActive(pnlStatSkip,    filter == "Skip");
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

                if (row.Status == "Valid")
                {
                    bool filledExisting = row.ExistingArticleId > 0;
                    reason      = filledExisting ? "✅ อัปเดตแล้ว" : "✅ นำเข้าสำเร็จ";
                    relatedData = filledExisting ? S(row.StatusNote ?? string.Empty, 300) : string.Empty;
                }
                else if (row.Status == "Skip")
                {
                    reason      = "⏭ ข้ามอัตโนมัติ";
                    relatedData = S(row.StatusNote ?? string.Empty, 300);
                }
                else if (row.Status == "Warn")
                {
                    reason      = "⚠️ นำเข้าสำเร็จแต่มีข้อสังเกต";
                    relatedData = S(row.StatusNote ?? string.Empty, 300);
                }
                else
                {
                    string note = row.StatusNote ?? string.Empty;
                    if (string.IsNullOrEmpty(note))
                    {
                        reason      = "ข้อผิดพลาดไม่ระบุ";
                        relatedData = string.Empty;
                    }
                    else
                    {
                        ClassifyError(note, out reason, out relatedData);

                        // ClassifyError generic fallback means the note is already a
                        // user-facing Thai message (validation errors, business-rule
                        // errors, Thai InvalidOperationException messages). Surface
                        // it directly — consistent with FormatPreviewNote behaviour.
                        if (reason == "❌ ข้อผิดพลาดทั่วไป")
                        {
                            reason      = S(note, 300);
                            relatedData = string.Empty;
                        }
                    }
                }

                int idx = dgvFailedRows.Rows.Add(
                    FormatRowLabel(row),
                    row.ArticleCode ?? row.ColA ?? string.Empty,
                    row.DocumentTypeLabel ?? row.RowKind ?? string.Empty,
                    row.Title ?? row.Tags ?? string.Empty,
                    relatedData,
                    reason);

                Color rowColor = row.Status == "Valid"
                    ? Color.FromArgb(240, 255, 244)
                    : row.Status == "Skip"
                        ? Color.FromArgb(235, 244, 253)
                        : row.Status == "Warn"
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
                relatedData = "ตรวจสอบคอลัมน์ที่มีข้อความยาว (A, P)";
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

            // The category the row needs (e.g. year category OR0002535 / PLI002535) does not exist yet — the user must create it first
            if (note.IndexOf("กรุณาสร้าง", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                reason      = "❌ ยังไม่มีหมวดในระบบ — ต้องสร้างหมวดก่อน";
                relatedData = note;
                return;
            }

            // พระราชกฤษฎีกา: Code กฎหมายอ้างอิง (คอลัมน์ LawReference) ผิดรูปแบบ / หมวดกลุ่มไม่อยู่ใต้หมวดพระราชกฤษฎีกา / ไม่พบกฎหมายที่อ้างอิง
            if (note.IndexOf("Code กฎหมายอ้างอิง", StringComparison.OrdinalIgnoreCase) >= 0 ||
                note.IndexOf("กฎหมายที่อ้างอิง", StringComparison.OrdinalIgnoreCase) >= 0 ||
                note.IndexOf("หมวดพระราชกฤษฎีกา", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                reason      = "❌ พระราชกฤษฎีกา: Code กฎหมายอ้างอิง/หมวด/กฎหมายที่อ้างอิงไม่ถูกต้อง";
                relatedData = note;
                return;
            }

            // CategoryID could not be resolved from the row's ArticleCode (KBImportCategoryResolver): law prefix / document
            // type not in the category table, category inactive, or the code belongs to the wrong group (law vs non-law)
            if (note.IndexOf("ไม่พบ CategoryID", StringComparison.OrdinalIgnoreCase) >= 0 ||
                note.IndexOf("หมวดกฎหมาย", StringComparison.OrdinalIgnoreCase) >= 0 ||
                note.IndexOf("หมวดประเภทเอกสาร", StringComparison.OrdinalIgnoreCase) >= 0 ||
                note.IndexOf("รหัสกฎหมาย", StringComparison.OrdinalIgnoreCase) >= 0 ||
                note.IndexOf("รหัสประเภทเอกสาร", StringComparison.OrdinalIgnoreCase) >= 0 ||
                note.IndexOf("อยู่ใต้หมวด", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                reason      = "❌ หาหมวดจาก ArticleCode ไม่ได้ (รหัสกฎหมาย/ประเภทเอกสารไม่ถูกต้อง)";
                relatedData = note;
                return;
            }

            // PublishedDate / EffectiveDate (columns N/O) could not be parsed
            if (note.IndexOf("ไม่สามารถแปลงวันที่ได้", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                reason      = "❌ รูปแบบวันที่ไม่ถูกต้อง (N/O)";
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
