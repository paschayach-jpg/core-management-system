using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.KMS.KB.Interfaces
{
    /// <summary>
    /// View contract สำหรับหน้า config Excel Import Template (implement โดย KBImportTemplateConfigForm)
    /// </summary>
    public interface IKBImportTemplateConfigController
    {
        int? EditingTemplateId { get; set; }
        string TemplateName { get; set; }
        string TemplateDescription { get; set; }
        bool DeleteEnabled { set; }

        void BindTemplateList(IEnumerable<KBImportTemplate> templates);
        void BindColumnGrid(IEnumerable<KBImportTemplateColumnRow> rows);

        /// <summary>แสดงคอลัมน์ที่อ่านได้จากไฟล์ Excel template ที่ admin อัปโหลด ให้คลิกเลือกเป็น mapping</summary>
        void BindExcelPreview(IEnumerable<ExcelColumnPreview> columns);

        /// <summary>รายการหมวดหมู่ (ไม่รวม CategoryCode = "LW") สำหรับ dropdown เลือก CategoryID ในแถว mapping</summary>
        void BindCategoryOptions(IEnumerable<KBCategory> categories);

        /// <summary>รายการค่า ArticleType คงที่ (1-5) สำหรับ dropdown เลือกในแถว mapping</summary>
        void BindArticleTypeOptions(IEnumerable<KBArticleTypeOption> options);

        /// <summary>อ่านค่าที่ admin กรอกในช่อง mapping ของ grid กลับออกมา</summary>
        IList<KBImportTemplateColumnRow> ReadColumnGridEdits();

        void ClearForm();
        void ShowError(string message);
        void ShowSuccess(string message);
    }
}
