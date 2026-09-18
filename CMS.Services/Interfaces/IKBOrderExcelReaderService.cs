using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>อ่านไฟล์ Excel ตาราง ประกาศ/คำสั่ง (column A–I)</summary>
    public interface IKBOrderExcelReaderService
    {
        /// <summary>ชื่อ worksheet ทั้งหมดในไฟล์ ตามลำดับที่อยู่ในไฟล์</summary>
        IList<string> GetSheetNames(string filePath);

        /// <summary>
        /// อ่านข้อมูลจาก worksheet ที่ระบุ (หัวตารางแถว 1, ข้อมูลเริ่มแถว 2)
        /// ไม่ตรวจความถูกต้อง — validate อยู่ที่ KBOrderImportProcessor
        /// </summary>
        IList<KBOrderImportRow> ReadRows(string filePath, string sheetName);
    }
}
