// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Controllers.KMS.KB;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// หน้า config Excel Import Template สำหรับ Knowledge Base (MOD-006)
    /// รองรับ: เพิ่ม / แก้ไข / Soft-Delete Template และ mapping คอลัมน์ Excel -> KMS.t_KBArticle
    /// เปิดเป็น MDI child ผ่านเมนู "⚙️ ระบบ" (เหมือน KBCategoryForm)
    /// </summary>
    public partial class KBImportTemplateConfigForm : Form, IKBImportTemplateConfigController
    {
        private KBImportTemplateConfigController _presenter;
        private readonly UserContext _user;

        private List<KBImportTemplate> _templates = new List<KBImportTemplate>();

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
            var confirm = MessageBox.Show(
                string.Format("ต้องการปิดใช้งาน Template \"{0}\" ใช่หรือไม่?", TemplateName),
                "ยืนยันการปิดใช้งาน", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
                _presenter.Delete();
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

        public void BindTemplateList(IEnumerable<KBImportTemplate> templates)
        {
            _templates = new List<KBImportTemplate>(templates);
            dgvTemplates.Rows.Clear();
            foreach (var t in _templates)
            {
                int rowIndex = dgvTemplates.Rows.Add(t.TemplateName + (t.IsActive ? string.Empty : "  (ปิด)"));
                dgvTemplates.Rows[rowIndex].Tag = t;
            }
        }

        public void BindColumnGrid(IEnumerable<KBImportTemplateColumnRow> rows)
        {
            dgvColumnMapping.Rows.Clear();
            foreach (var row in rows)
            {
                int rowIndex = dgvColumnMapping.Rows.Add(row.TargetColumnName, row.ColumnDescription, row.ExcelColumnRef);
                dgvColumnMapping.Rows[rowIndex].Tag = row;
            }
        }

        public IList<KBImportTemplateColumnRow> ReadColumnGridEdits()
        {
            var result = new List<KBImportTemplateColumnRow>();
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
            dgvTemplates.ClearSelection();
            dgvColumnMapping.Rows.Clear();
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
        }
    }
}
