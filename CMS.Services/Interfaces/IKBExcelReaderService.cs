using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IKBExcelReaderService
    {
        /// <param name="templateColumns">
        /// Column mapping จาก Template ที่เลือก (KBImportTemplateService.GetTemplateColumnGrid) —
        /// Title/TitleCategory/Summary/ContentHTML/PublishedDate/EffectiveDate ที่มี ExcelColumnRef
        /// กำหนดไว้จะถูกอ่านตาม mapping นี้แทนตำแหน่งคอลัมน์ตายตัว (ตัวอื่น รวมถึง ArticleCode ยัง
        /// hardcode ต่อ layout เหมือนเดิม)
        /// </param>
        IList<KBImportRow> ReadRows(string filePath, IList<KBImportTemplateColumnRow> templateColumns);
    }
}
