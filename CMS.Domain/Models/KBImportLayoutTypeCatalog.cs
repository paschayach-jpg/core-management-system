using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// ค่า LayoutType คงที่ ("Law"/"Order") ที่ใช้ร่วมกันระหว่าง KBImportRow.RowKind (ตั้งค่าโดย
    /// KBExcelReader) และค่าที่ KBImportTemplateService.GetEffectiveLayoutType derive จาก CategoryID
    /// — รวมไว้จุดเดียวกันค่า/label หลุดไม่ตรงกันระหว่างไฟล์ ถ้าเพิ่ม layout ใหม่ในอนาคตให้แก้ที่นี่ที่เดียว
    /// </summary>
    public static class KBImportLayoutTypeCatalog
    {
        public const string Law = "Law";
        public const string Order = "Order";

        /// <summary>คำอธิบายภาษาไทยของค่า LayoutType; คืนค่าตัวเองถ้าไม่รู้จัก (กันข้อมูลเพี้ยนพังหน้าจอ)</summary>
        public static string Describe(string layoutType)
        {
            if (string.Equals(layoutType, Law, StringComparison.OrdinalIgnoreCase))
                return "กฎหมาย (มาตรา)";
            if (string.Equals(layoutType, Order, StringComparison.OrdinalIgnoreCase))
                return "ประกาศ/คำสั่ง";
            return layoutType ?? string.Empty;
        }
    }
}
