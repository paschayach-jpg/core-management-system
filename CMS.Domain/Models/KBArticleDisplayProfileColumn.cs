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

        /// <summary>คำอธิบายจุดที่แสดงผลของ slot นี้ (ผู้ใช้แก้ได้ในหน้า config) — NULL = ยังไม่เคยตั้ง ใช้ค่าเริ่มต้นจาก KBArticleDisplaySlotCatalog</summary>
        public string SlotDescription { get; set; }

        public string DisplayLabel    { get; set; }
        public bool   IsVisible       { get; set; }
    }
}
