using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// อ่านแถวหัวตาราง + ตัวอย่างข้อมูล 1 แถว จากไฟล์ Excel template ที่ admin อัปโหลด
    /// สำหรับหน้า config Excel Import Template (ให้คลิกเลือกคอลัมน์เพื่อ mapping)
    /// </summary>
    public interface IExcelTemplatePreviewService
    {
        /// <summary>
        /// อ่าน sheet แรกของไฟล์ที่ <paramref name="filePath"/> คืนคอลัมน์ทั้งหมดจากแถวหัวตาราง (แถวแรก)
        /// พร้อมค่าตัวอย่างจากแถวข้อมูลแถวแรก (แถวที่สอง ถ้ามี)
        /// </summary>
        IList<ExcelColumnPreview> ReadColumnPreview(string filePath);
    }
}
