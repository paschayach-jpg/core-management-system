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

        /// <summary>อ่านค่าที่ admin กรอกในช่อง mapping ของ grid กลับออกมา</summary>
        IList<KBImportTemplateColumnRow> ReadColumnGridEdits();

        void ClearForm();
        void ShowError(string message);
        void ShowSuccess(string message);
    }
}
