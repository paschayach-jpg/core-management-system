using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IKBExcelReaderService
    {
        /// <summary>
        /// อ่านไฟล์ Excel คืนแถวข้อมูล + layout ที่ตรวจพบจากไฟล์ + รายการ mapping ของ Template ที่ชี้ไปคอลัมน์/หัวตารางที่ไม่มีในไฟล์
        /// (<see cref="KBExcelReadResult.MappingProblems"/> — ผู้เรียกควรหยุดก่อน validate รายแถวถ้าไม่ว่าง)
        /// </summary>
        /// <param name="templateColumns">
        /// Column mapping จาก Template ที่เลือก (KBImportTemplateService.GetTemplateColumnGrid) —
        /// ArticleCode/Title/TitleCategory/Summary/ContentHTML/PublishedDate/EffectiveDate/Attachment ที่มี ExcelColumnRef
        /// กำหนดไว้จะถูกอ่านตาม mapping นี้ (ไฟล์กฎหมายที่ไม่ได้ map ฟิลด์เนื้อหายังใช้ตำแหน่งคอลัมน์ตายตัวเป็นค่าเริ่มต้น)
        /// </param>
        KBExcelReadResult ReadRows(string filePath, IList<KBImportTemplateColumnRow> templateColumns);
    }
}
