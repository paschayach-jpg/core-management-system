namespace CMS.Domain.Models
{
    /// <summary>
    /// การ map คอลัมน์ปลายทางใน KMS.t_KBArticle กับคอลัมน์/หัวข้อฝั่ง Excel
    /// สำหรับ Template หนึ่งรายการ (KBImportTemplate)
    /// Maps to KMS.m_KBImportTemplateColumn
    /// </summary>
    public class KBImportTemplateColumnMapping
    {
        public int    TemplateColumnID { get; set; }
        public int    TemplateID       { get; set; }

        /// <summary>ชื่อคอลัมน์จริงใน KMS.t_KBArticle เช่น "Title", "Summary"</summary>
        public string TargetColumnName { get; set; }

        /// <summary>คอลัมน์/หัวข้อฝั่ง Excel ที่ admin กำหนดให้ map มา เช่น "C" หรือ "หัวข้อเรื่อง"</summary>
        public string ExcelColumnRef  { get; set; }

        public int    DisplayOrder    { get; set; }
    }
}
