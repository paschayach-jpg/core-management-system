namespace CMS.Domain.Models
{
    /// <summary>
    /// ค่า header label + การซ่อน/แสดงของ slot หนึ่งตัว (KBArticleDisplaySlotCatalog) สำหรับ
    /// KBArticleDisplayProfile หนึ่งรายการ
    /// Maps to KMS.m_KBArticleDisplayProfileColumn
    /// </summary>
    public class KBArticleDisplayProfileColumn
    {
        public int    ProfileColumnID { get; set; }
        public int    ProfileID       { get; set; }

        /// <summary>slot key จาก KBArticleDisplaySlotCatalog เช่น "Title", "TitleCategory"</summary>
        public string SlotKey         { get; set; }

        public string DisplayLabel    { get; set; }
        public bool   IsVisible       { get; set; }
    }
}
