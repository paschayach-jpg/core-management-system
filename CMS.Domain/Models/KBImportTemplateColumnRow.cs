namespace CMS.Domain.Models
{
    /// <summary>
    /// แถวข้อมูลสำหรับ grid หน้า config Excel Import Template
    /// รวม 3 คอลัมน์: คอลัมน์ปลายทาง (KMS.t_KBArticle), คำอธิบาย, mapping จาก Excel
    /// ใช้ข้าม boundary ระหว่าง Services/Controllers/Presentation จึงอยู่ที่ Domain เหมือน KBImportValidationResult
    /// </summary>
    public class KBImportTemplateColumnRow
    {
        public string TargetColumnName { get; set; }
        public string ColumnDescription { get; set; }
        public string ExcelColumnRef { get; set; }
        public int DisplayOrder { get; set; }
    }
}
