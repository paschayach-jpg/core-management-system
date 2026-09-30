// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Controllers.KMS.KB;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// หน้า config Excel Import Template สำหรับ Knowledge Base (MOD-006)
    /// รองรับ: เพิ่ม / แก้ไข / Soft-Delete Template, อัปโหลด Excel เพื่อดูตัวอย่างคอลัมน์
    /// และคลิกเลือกคอลัมน์ Excel เพื่อ mapping เข้ากับคอลัมน์ปลายทาง (มี highlight สีให้เห็นคู่ที่จับกัน)
    /// เปิดจากปุ่มในหัวฟอร์ม KBMainForm (ไม่ผ่านเมนู admin ใดๆ)
    /// </summary>
    public partial class KBImportTemplateConfigForm : Form, IKBImportTemplateConfigController
    {
        private KBImportTemplateConfigController _presenter;
        private readonly UserContext _user;

        private List<KBImportTemplate> _templates = new List<KBImportTemplate>();
        private List<ExcelColumnPreview> _excelColumns = new List<ExcelColumnPreview>();
        private List<KBCategory> _categoryOptions = new List<KBCategory>();
        private List<KBArticleTypeOption> _articleTypeOptions = new List<KBArticleTypeOption>();

        /// <summary>DisplayOrder เดิมของแถว CategoryID/ArticleType ที่โหลดมา — เก็บไว้ round-trip กลับตอน save เฉยๆ ไม่มีผลต่อการแสดงผล เพราะย้ายออกจาก grid ไปเป็น dropdown แยกแล้ว</summary>
        private int _categoryColumnDisplayOrder = 0;
        private int _articleTypeColumnDisplayOrder = 0;

        /// <summary>ชื่อคอลัมน์เป้าหมายที่ใช้ dropdown เลือกค่าคงที่ (ไม่ใช่ mapping จาก Excel — ย้ายออกจาก dgvColumnMapping ไปเป็น cboCategoryConfig/cboArticleTypeConfig บน pnlEditCard แล้ว)</summary>
        private static readonly HashSet<string> FixedValueTargetColumns =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CategoryID", "ArticleType" };

        // ── Click-to-map bookkeeping: TargetColumnName -> excel column index / highlight color ──
        private readonly Dictionary<string, int>   _targetToExcelColumnIndex = new Dictionary<string, int>();
        private readonly Dictionary<string, Color> _targetHighlightColor     = new Dictionary<string, Color>();
        private int _nextColorIndex = 0;

        /// <summary>กันการวน sync ซ้ำระหว่าง dgvColumnMapping กับ dgvExcelPreview เวลาโค้ดตั้งค่า cell เอง</summary>
        private bool _suppressMappingSync = false;

        private static readonly Color[] HighlightPalette =
        {
            Color.FromArgb(200, 230, 201), // green
            Color.FromArgb(255, 224, 178), // orange
            Color.FromArgb(187, 222, 251), // blue
            Color.FromArgb(255, 205, 210), // pink
            Color.FromArgb(225, 190, 231), // purple
            Color.FromArgb(255, 249, 196), // yellow
            Color.FromArgb(178, 235, 242), // cyan
            Color.FromArgb(215, 204, 200), // brown
        };

        public event EventHandler BackRequested;

        public KBImportTemplateConfigForm(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
            InitializeComponent();
        }

        public void AttachPresenter(KBImportTemplateConfigController presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        // ── Form events ──────────────────────────────────────────────────────

        private void KBImportTemplateConfigForm_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\n\nกรุณาเรียก AttachPresenter() ก่อนเปิดฟอร์ม",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            SetupColumnMappingGrid();
            SetupExcelPreviewGrid();
            _presenter.Initialize();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (BackRequested != null)
                BackRequested(this, EventArgs.Empty);
        }

        private void BtnNewTemplate_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            dgvTemplates.ClearSelection();
            _presenter.OnNewTemplate();
        }

        private void DgvTemplates_SelectionChanged(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (dgvTemplates.SelectedRows.Count == 0) return;
            var template = dgvTemplates.SelectedRows[0].Tag as KBImportTemplate;
            if (template == null) return;
            _presenter.OnTemplateSelected(template);
        }

        private void BtnUploadExcel_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "เลือกไฟล์ Excel Template";
                dlg.Filter = "Excel Files (*.xlsx;*.xlsm)|*.xlsx;*.xlsm";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                _presenter.OnExcelFileSelected(dlg.FileName);
            }
        }

        private void DgvExcelPreview_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            HandleExcelColumnClick(e.ColumnIndex, e.Clicks, 0);
        }

        private void DgvExcelPreview_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            HandleExcelColumnClick(e.ColumnIndex, e.Clicks, e.RowIndex);
        }

        /// <summary>
        /// จุดรวมของการคลิกเลือกคอลัมน์ Excel — ใช้ร่วมกันทั้งคลิกที่หัวคอลัมน์และคลิกที่ cell ข้อมูล
        /// คลิกครั้งเดียว = เลือก/mapping, ดับเบิลคลิก = ยกเลิก mapping
        /// </summary>
        private void HandleExcelColumnClick(int columnIndex, int clicks, int rowIndex)
        {
            if (columnIndex < 0 || columnIndex >= _excelColumns.Count) return;

            if (clicks >= 2)
            {
                UnassignExcelColumn(columnIndex);
                return;
            }

            bool assigned = AssignExcelColumnToActiveTarget(columnIndex);
            if (!assigned) return;

            MoveExcelPreviewFocusRight(columnIndex, rowIndex);
            MoveColumnMappingFocusDown();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (string.IsNullOrWhiteSpace(txtTemplateName.Text))
            {
                MessageBox.Show("กรุณากรอกชื่อ Template", "กรุณากรอกข้อมูล",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTemplateName.Focus();
                return;
            }
            _presenter.Save();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (!EditingTemplateId.HasValue) return;
            if (_recordIsActive)
            {
                var confirm = MessageBox.Show(
                    string.Format("ต้องการปิดใช้งาน Template \"{0}\" ใช่หรือไม่?", TemplateName),
                    "ยืนยันการปิดใช้งาน", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm == DialogResult.Yes)
                    _presenter.Delete();
            }
            else
            {
                var confirm = MessageBox.Show(
                    string.Format("ต้องการเปิดใช้งาน Template \"{0}\" อีกครั้งใช่หรือไม่?", TemplateName),
                    "ยืนยันการเปิดใช้งาน", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                    _presenter.Activate();
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        // ── IKBImportTemplateConfigController ───────────────────────────────

        public int? EditingTemplateId { get; set; }

        public string TemplateName
        {
            get { return txtTemplateName.Text.Trim(); }
            set { txtTemplateName.Text = value ?? string.Empty; }
        }

        public string TemplateDescription
        {
            get { return txtTemplateDescription.Text.Trim(); }
            set { txtTemplateDescription.Text = value ?? string.Empty; }
        }

        public bool DeleteEnabled
        {
            set { btnDelete.Enabled = value; }
        }

        private bool _recordIsActive = true;

        public bool RecordIsActive
        {
            set
            {
                _recordIsActive = value;
                btnDelete.Text = value ? "🗑️ ปิดใช้งาน" : "♻️ เปิดใช้งาน";
                btnDelete.BackColor = value
                    ? System.Drawing.Color.FromArgb(200, 50, 50)
                    : System.Drawing.Color.FromArgb(40, 150, 80);
            }
        }

        public void BindTemplateList(IEnumerable<KBImportTemplate> templates)
        {
            _templates = new List<KBImportTemplate>(templates);
            dgvTemplates.Rows.Clear();
            foreach (var t in _templates)
            {
                int rowIndex = dgvTemplates.Rows.Add(t.TemplateName + (t.IsActive ? string.Empty : "  (ปิด)"));
                dgvTemplates.Rows[rowIndex].Tag = t;
                dgvTemplates.Rows[rowIndex].DefaultCellStyle.BackColor = t.IsActive
                    ? Color.FromArgb(223, 247, 227)   // เขียวอ่อน — ใช้งานอยู่
                    : Color.FromArgb(253, 223, 221);  // แดงอ่อน — ปิดใช้งานแล้ว
            }
        }

        public void BindColumnGrid(IEnumerable<KBImportTemplateColumnRow> rows)
        {
            ResetAllMappingBookkeeping();

            dgvColumnMapping.Rows.Clear();

            KBImportTemplateColumnRow categoryRow = null;
            KBImportTemplateColumnRow articleTypeRow = null;

            foreach (var row in rows)
            {
                // CategoryID/ArticleType ไม่ใช่แถวใน grid นี้แล้ว — ย้ายไปเป็น dropdown บน pnlEditCard
                if (string.Equals(row.TargetColumnName, "CategoryID", StringComparison.OrdinalIgnoreCase))
                {
                    categoryRow = row;
                    continue;
                }
                if (string.Equals(row.TargetColumnName, "ArticleType", StringComparison.OrdinalIgnoreCase))
                {
                    articleTypeRow = row;
                    continue;
                }

                int rowIndex = dgvColumnMapping.Rows.Add(row.TargetColumnName, row.ColumnDescription, row.ExcelColumnRef);
                dgvColumnMapping.Rows[rowIndex].Tag = row;
            }

            ApplyCategoryConfigSelection(categoryRow);
            ApplyArticleTypeConfigSelection(articleTypeRow);
        }

        public void BindCategoryOptions(IEnumerable<KBCategory> categories)
        {
            _categoryOptions = new List<KBCategory>(categories);

            var withAll = new List<KBCategory> { new KBCategory { CategoryID = 0, CategoryName = "ทั้งหมด" } };
            withAll.AddRange(_categoryOptions);

            cboCategoryConfig.DataSource = null;
            cboCategoryConfig.DataSource = withAll;
            cboCategoryConfig.DisplayMember = "CategoryName";
            cboCategoryConfig.ValueMember = "CategoryID";
        }

        public void BindArticleTypeOptions(IEnumerable<KBArticleTypeOption> options)
        {
            _articleTypeOptions = new List<KBArticleTypeOption>(options);

            var withAll = new List<KBArticleTypeOption> { new KBArticleTypeOption { ArticleType = 0, Description = "ทั้งหมด" } };
            withAll.AddRange(_articleTypeOptions);

            cboArticleTypeConfig.DataSource = null;
            cboArticleTypeConfig.DataSource = withAll;
            cboArticleTypeConfig.DisplayMember = "Description";
            cboArticleTypeConfig.ValueMember = "ArticleType";
        }

        /// <summary>เลือกค่าใน cboCategoryConfig ตามที่ Template เดิมตั้งไว้ (ถ้ามี) ไม่งั้น default = "ทั้งหมด" (index 0)</summary>
        private void ApplyCategoryConfigSelection(KBImportTemplateColumnRow row)
        {
            _categoryColumnDisplayOrder = row != null ? row.DisplayOrder : 0;

            int categoryId;
            if (row != null && int.TryParse(row.ExcelColumnRef, out categoryId) && _categoryOptions.Any(c => c.CategoryID == categoryId))
                cboCategoryConfig.SelectedValue = categoryId;
            else
                cboCategoryConfig.SelectedIndex = 0;
        }

        /// <summary>เลือกค่าใน cboArticleTypeConfig ตามที่ Template เดิมตั้งไว้ (ถ้ามี) ไม่งั้น default = "ทั้งหมด" (index 0)</summary>
        private void ApplyArticleTypeConfigSelection(KBImportTemplateColumnRow row)
        {
            _articleTypeColumnDisplayOrder = row != null ? row.DisplayOrder : 0;

            byte articleType;
            if (row != null && byte.TryParse(row.ExcelColumnRef, out articleType) && _articleTypeOptions.Any(o => o.ArticleType == articleType))
                cboArticleTypeConfig.SelectedValue = articleType;
            else
                cboArticleTypeConfig.SelectedIndex = 0;
        }

        public void BindExcelPreview(IEnumerable<ExcelColumnPreview> columns)
        {
            _excelColumns = new List<ExcelColumnPreview>(columns);

            // ไฟล์ใหม่ = ดัชนีคอลัมน์เดิมใช้อ้างอิงไม่ได้แล้ว ล้างเฉพาะสถานะ/สีของการคลิกเลือก
            // (ไม่ล้างค่าข้อความ mapping ที่กรอกไว้แล้ว เผื่อ admin พิมพ์เองหรืออัปโหลดไฟล์ผิดแล้วอัปใหม่)
            _targetToExcelColumnIndex.Clear();
            _targetHighlightColor.Clear();
            _nextColorIndex = 0;
            foreach (DataGridViewRow row in dgvColumnMapping.Rows)
                ResetTargetRowHighlight(row);

            dgvExcelPreview.Columns.Clear();
            dgvExcelPreview.Rows.Clear();

            foreach (var col in _excelColumns)
            {
                var gridCol = new DataGridViewTextBoxColumn
                {
                    Name = "colExcel_" + col.ColumnIndex,
                    HeaderText = col.ColumnRef,
                    ReadOnly = true,
                    Width = 110,
                };
                dgvExcelPreview.Columns.Add(gridCol);
            }

            if (_excelColumns.Count == 0) return;

            int headerRowIndex = dgvExcelPreview.Rows.Add();
            int sampleRowIndex = dgvExcelPreview.Rows.Add();
            dgvExcelPreview.Rows[headerRowIndex].HeaderCell.Value = "หัวตาราง";
            dgvExcelPreview.Rows[sampleRowIndex].HeaderCell.Value = "ตัวอย่างข้อมูล";

            for (int i = 0; i < _excelColumns.Count; i++)
            {
                dgvExcelPreview.Rows[headerRowIndex].Cells[i].Value = _excelColumns[i].HeaderText;
                dgvExcelPreview.Rows[sampleRowIndex].Cells[i].Value = _excelColumns[i].SampleValue;
            }
        }

        public IList<KBImportTemplateColumnRow> ReadColumnGridEdits()
        {
            var result = new List<KBImportTemplateColumnRow>();

            var categoryId = cboCategoryConfig.SelectedIndex > 0 ? (int)cboCategoryConfig.SelectedValue : 0;
            result.Add(new KBImportTemplateColumnRow
            {
                TargetColumnName  = "CategoryID",
                ColumnDescription = KBArticleColumnCatalog.DescribeColumn("CategoryID"),
                ExcelColumnRef    = categoryId > 0 ? categoryId.ToString() : string.Empty,
                DisplayOrder      = _categoryColumnDisplayOrder,
            });

            var articleType = cboArticleTypeConfig.SelectedIndex > 0 ? (byte)cboArticleTypeConfig.SelectedValue : (byte)0;
            result.Add(new KBImportTemplateColumnRow
            {
                TargetColumnName  = "ArticleType",
                ColumnDescription = KBArticleColumnCatalog.DescribeColumn("ArticleType"),
                ExcelColumnRef    = articleType > 0 ? articleType.ToString() : string.Empty,
                DisplayOrder      = _articleTypeColumnDisplayOrder,
            });

            foreach (DataGridViewRow gridRow in dgvColumnMapping.Rows)
            {
                var source = gridRow.Tag as KBImportTemplateColumnRow;
                if (source == null) continue;
                result.Add(new KBImportTemplateColumnRow
                {
                    TargetColumnName  = source.TargetColumnName,
                    ColumnDescription = source.ColumnDescription,
                    ExcelColumnRef    = Convert.ToString(gridRow.Cells[colExcelMapping.Index].Value ?? string.Empty).Trim(),
                    DisplayOrder      = source.DisplayOrder,
                });
            }
            return result;
        }

        public void ClearForm()
        {
            EditingTemplateId = null;
            txtTemplateName.Text = string.Empty;
            txtTemplateDescription.Text = string.Empty;
            btnDelete.Enabled = false;
            RecordIsActive = true;
            dgvTemplates.ClearSelection();
            dgvColumnMapping.Rows.Clear();
            if (cboCategoryConfig.Items.Count > 0) cboCategoryConfig.SelectedIndex = 0;
            if (cboArticleTypeConfig.Items.Count > 0) cboArticleTypeConfig.SelectedIndex = 0;
            _categoryColumnDisplayOrder = 0;
            _articleTypeColumnDisplayOrder = 0;
            txtTemplateName.Focus();
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowSuccess(string message)
        {
            MessageBox.Show(message, "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Private UI helpers ────────────────────────────────────────────────

        private void SetupColumnMappingGrid()
        {
            dgvColumnMapping.AutoGenerateColumns = false;
            dgvColumnMapping.AllowUserToAddRows = false;
            dgvColumnMapping.RowHeadersVisible = false;
            dgvColumnMapping.CellValueChanged += DgvColumnMapping_CellValueChanged;

            colExcelMapping.ToolTipText =
                "พิมพ์ชื่อคอลัมน์ Excel ตรง ๆ (เช่น C) หรือหัวตาราง เพื่อ map แบบตรง\n" +
                "หรือผสมข้อความคงที่กับคอลัมน์ในรูปแบบ {X} สำหรับค่าที่ต้องคำนวณ เช่น \"มาตรา {C}\"";
        }

        private void SetupExcelPreviewGrid()
        {
            dgvExcelPreview.AutoGenerateColumns = false;
            dgvExcelPreview.AllowUserToAddRows = false;
            dgvExcelPreview.AllowUserToDeleteRows = false;
            dgvExcelPreview.RowHeadersVisible = true;
            dgvExcelPreview.RowHeadersWidth = 90;
            dgvExcelPreview.ColumnHeaderMouseClick += DgvExcelPreview_ColumnHeaderMouseClick;
            dgvExcelPreview.CellMouseClick += DgvExcelPreview_CellMouseClick;
        }

        /// <summary>
        /// Two-way binding: ถ้า admin ลบ/แก้ข้อความในช่อง mapping ของ dgvColumnMapping โดยตรง
        /// (พิมพ์ลบเอง หรือกด Delete) ให้ล้าง highlight ฝั่ง dgvExcelPreview ตามไปด้วยเมื่อค่าว่างลง
        /// </summary>
        private void DgvColumnMapping_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_suppressMappingSync) return;
            if (e.RowIndex < 0 || e.ColumnIndex != colExcelMapping.Index) return;

            var row = dgvColumnMapping.Rows[e.RowIndex];
            var targetRowData = row.Tag as KBImportTemplateColumnRow;
            if (targetRowData == null) return;

            string cellText = Convert.ToString(row.Cells[colExcelMapping.Index].Value ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(cellText))
            {
                string targetKey = targetRowData.TargetColumnName;
                if (_targetToExcelColumnIndex.ContainsKey(targetKey))
                    ClearTargetMapping(targetKey);
            }
        }

        private void ResetAllMappingBookkeeping()
        {
            _targetToExcelColumnIndex.Clear();
            _targetHighlightColor.Clear();
            _nextColorIndex = 0;
            for (int i = 0; i < dgvExcelPreview.Columns.Count; i++)
                ResetExcelColumnHighlight(i);
        }

        /// <summary>
        /// Map คอลัมน์ Excel ที่ index ที่คลิก เข้ากับแถวเป้าหมายที่กำลัง active อยู่ใน dgvColumnMapping
        /// (แถวที่ admin คลิกเลือกไว้ล่าสุด) พร้อม highlight สีให้เห็นคู่ที่จับกัน
        /// คืนค่า true เมื่อ mapping ใหม่เกิดขึ้นจริง (ใช้ตัดสินใจว่าจะขยับ focus ต่อหรือไม่)
        /// </summary>
        private bool AssignExcelColumnToActiveTarget(int excelColumnIndex)
        {
            var targetRow = dgvColumnMapping.CurrentRow != null
                ? dgvColumnMapping.CurrentRow.Tag as KBImportTemplateColumnRow
                : null;
            if (targetRow == null)
            {
                MessageBox.Show(
                    "กรุณาคลิกเลือกแถวคอลัมน์ปลายทางในตาราง Mapping ก่อน แล้วค่อยคลิกเลือกคอลัมน์ Excel ด้านล่าง",
                    "เลือกคอลัมน์ปลายทางก่อน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            string targetKey = targetRow.TargetColumnName;

            if (FixedValueTargetColumns.Contains(targetKey))
            {
                MessageBox.Show(
                    "คอลัมน์ปลายทางนี้เลือกค่าจาก dropdown ในตาราง Mapping โดยตรง ไม่ต้อง map จากไฟล์ Excel",
                    "ไม่ต้อง Map จาก Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            // คอลัมน์ Excel นี้ถูก target อื่นเลือกไปแล้ว → ห้ามเลือกซ้ำ (1 คอลัมน์ Excel ใช้ได้ครั้งเดียว)
            foreach (var kvp in _targetToExcelColumnIndex)
            {
                if (kvp.Value == excelColumnIndex && kvp.Key != targetKey)
                {
                    MessageBox.Show(
                        string.Format("คอลัมน์ Excel \"{0}\" ถูกเลือกไปแล้วสำหรับคอลัมน์ปลายทาง \"{1}\"\n\nกรุณาเลือกคอลัมน์อื่น หรือดับเบิลคลิกที่คอลัมน์นี้เพื่อยกเลิกการเลือกเดิมก่อน",
                            GetExcelColumnDisplayName(excelColumnIndex), kvp.Key),
                        "คอลัมน์นี้ถูกเลือกไปแล้ว", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            // target ที่ active อยู่มี mapping เดิมอยู่แล้ว
            int oldExcelIndex;
            if (_targetToExcelColumnIndex.TryGetValue(targetKey, out oldExcelIndex))
            {
                if (oldExcelIndex == excelColumnIndex) return false; // คลิกคอลัมน์เดิมซ้ำ ไม่ต้องทำอะไร
                ResetExcelColumnHighlight(oldExcelIndex); // เปลี่ยนไปคอลัมน์ใหม่ → ล้าง highlight คอลัมน์เดิมออกก่อน
            }

            Color color;
            if (!_targetHighlightColor.TryGetValue(targetKey, out color))
            {
                color = HighlightPalette[_nextColorIndex % HighlightPalette.Length];
                _nextColorIndex++;
                _targetHighlightColor[targetKey] = color;
            }

            _targetToExcelColumnIndex[targetKey] = excelColumnIndex;

            _suppressMappingSync = true;
            try { dgvColumnMapping.CurrentRow.Cells[colExcelMapping.Index].Value = _excelColumns[excelColumnIndex].ColumnRef; }
            finally { _suppressMappingSync = false; }
            HighlightTargetRow(dgvColumnMapping.CurrentRow, color);

            HighlightExcelColumn(excelColumnIndex, color);
            return true;
        }

        private void UnassignExcelColumn(int excelColumnIndex)
        {
            string targetKey = null;
            foreach (var kvp in _targetToExcelColumnIndex)
            {
                if (kvp.Value == excelColumnIndex) { targetKey = kvp.Key; break; }
            }
            if (targetKey == null) return;
            ClearTargetMapping(targetKey);
        }

        private void ClearTargetMapping(string targetKey)
        {
            int excelIndex;
            if (_targetToExcelColumnIndex.TryGetValue(targetKey, out excelIndex))
            {
                ResetExcelColumnHighlight(excelIndex);
                _targetToExcelColumnIndex.Remove(targetKey);
            }
            _targetHighlightColor.Remove(targetKey);

            foreach (DataGridViewRow row in dgvColumnMapping.Rows)
            {
                var r = row.Tag as KBImportTemplateColumnRow;
                if (r != null && r.TargetColumnName == targetKey)
                {
                    _suppressMappingSync = true;
                    try { row.Cells[colExcelMapping.Index].Value = string.Empty; }
                    finally { _suppressMappingSync = false; }
                    ResetTargetRowHighlight(row);
                    break;
                }
            }
        }

        /// <summary>ขยับ CurrentCell ของ dgvExcelPreview ไปคอลัมน์ถัดขวา 1 คอลัมน์ — ไม่ขยับถ้าอยู่คอลัมน์สุดท้ายแล้ว</summary>
        private void MoveExcelPreviewFocusRight(int currentColumnIndex, int rowIndex)
        {
            int nextColumnIndex = currentColumnIndex + 1;
            if (nextColumnIndex >= dgvExcelPreview.Columns.Count) return;
            if (dgvExcelPreview.Rows.Count == 0) return;
            int targetRowIndex = (rowIndex >= 0 && rowIndex < dgvExcelPreview.Rows.Count) ? rowIndex : 0;
            dgvExcelPreview.CurrentCell = dgvExcelPreview.Rows[targetRowIndex].Cells[nextColumnIndex];
        }

        /// <summary>ขยับ CurrentCell ของ dgvColumnMapping ไปแถวถัดลง 1 แถว — ไม่ขยับถ้าอยู่แถวสุดท้ายแล้ว</summary>
        private void MoveColumnMappingFocusDown()
        {
            if (dgvColumnMapping.CurrentCell == null) return;
            int nextRowIndex = dgvColumnMapping.CurrentCell.RowIndex + 1;
            if (nextRowIndex >= dgvColumnMapping.Rows.Count) return;
            int columnIndex = dgvColumnMapping.CurrentCell.ColumnIndex;
            dgvColumnMapping.CurrentCell = dgvColumnMapping.Rows[nextRowIndex].Cells[columnIndex];
        }

        private string GetExcelColumnDisplayName(int excelColumnIndex)
        {
            var col = _excelColumns[excelColumnIndex];
            return string.IsNullOrWhiteSpace(col.HeaderText)
                ? col.ColumnRef
                : string.Format("{0} ({1})", col.ColumnRef, col.HeaderText);
        }

        private void HighlightTargetRow(DataGridViewRow row, Color color)
        {
            row.Cells[colTargetColumn.Index].Style.BackColor = color;
            row.Cells[colDescription.Index].Style.BackColor = color;
            row.Cells[colExcelMapping.Index].Style.BackColor = color;
        }

        private void ResetTargetRowHighlight(DataGridViewRow row)
        {
            row.Cells[colTargetColumn.Index].Style.BackColor = dgvColumnMapping.DefaultCellStyle.BackColor;
            row.Cells[colDescription.Index].Style.BackColor = dgvColumnMapping.DefaultCellStyle.BackColor;
            row.Cells[colExcelMapping.Index].Style.BackColor = dgvColumnMapping.DefaultCellStyle.BackColor;
        }

        private void HighlightExcelColumn(int excelColumnIndex, Color color)
        {
            foreach (DataGridViewRow row in dgvExcelPreview.Rows)
                row.Cells[excelColumnIndex].Style.BackColor = color;
            dgvExcelPreview.Columns[excelColumnIndex].HeaderCell.Style.BackColor = color;
        }

        private void ResetExcelColumnHighlight(int excelColumnIndex)
        {
            if (excelColumnIndex < 0 || excelColumnIndex >= dgvExcelPreview.Columns.Count) return;
            foreach (DataGridViewRow row in dgvExcelPreview.Rows)
                row.Cells[excelColumnIndex].Style.BackColor = dgvExcelPreview.DefaultCellStyle.BackColor;
            dgvExcelPreview.Columns[excelColumnIndex].HeaderCell.Style.BackColor =
                dgvExcelPreview.ColumnHeadersDefaultCellStyle.BackColor;
        }
    }
}
