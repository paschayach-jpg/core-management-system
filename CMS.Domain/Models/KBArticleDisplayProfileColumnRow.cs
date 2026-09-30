namespace CMS.Domain.Models
{
    /// <summary>
    /// แถวข้อมูลสำหรับ grid หน้า config Article Display Profile (1 แถวต่อ 1 slot จาก
    /// KBArticleDisplaySlotCatalog) ใช้ข้าม boundary ระหว่าง Services/Controllers/Presentation
    /// เหมือน KBImportTemplateColumnRow
    /// </summary>
    public class KBArticleDisplayProfileColumnRow
    {
        public string SlotKey         { get; set; }
        public string SlotDescription { get; set; }
        public string DisplayLabel    { get; set; }
        public bool   IsVisible       { get; set; }
    }
}
