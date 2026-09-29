using System;
using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Master data for Knowledge Base article categories.
    /// Maps to KMS.m_KBCategory in the database.
    /// Supports self-referencing hierarchy via ParentCategoryID.
    /// </summary>
    public class KBCategory
    {
        public int      CategoryID       { get; set; }
        public int?     ParentCategoryID { get; set; }
        public string   CategoryCode     { get; set; }
        public string   CategoryName     { get; set; }

        /// <summary>Icon type stored as a string representation of the enum value (e.g., "1", "2", etc.).</summary>
        public string   Icon             { get; set; }

        public string   Description      { get; set; }
        public int      SortOrder        { get; set; }
        public bool     IsActive         { get; set; }
        public string   CreatedBy        { get; set; }
        public DateTime CreatedDate      { get; set; }

        /// <summary>
        /// FK → KMS.m_KBArticleDisplayProfile (nullable) — profile ที่กำหนด header/การซ่อนแสดงคอลัมน์ของ
        /// dgvArticles/ArticleCardControl สำหรับหมวดนี้ ถ้าเป็น null จะ inherit จาก ancestor ที่ใกล้ที่สุด
        /// ที่มีค่านี้ตั้งไว้ (ดู KBArticleDisplayProfileService.GetEffectiveDisplaySettings) ถ้าไม่มีเลย
        /// ตลอดสายจะใช้ default ตาม KBArticleDisplaySlotCatalog
        /// </summary>
        public int?     DisplayProfileID { get; set; }

        // Display helper — set by repository when joining parent name
        public string ParentCategoryName { get; set; }

        /// <summary>Returns the emoji symbol for the icon value.</summary>
        public string IconSymbol
        {
            get
            {
                byte iconValue;
                if (byte.TryParse(Icon, out iconValue) && iconValue >= 1 && iconValue <= 9)
                    return KBCategoryIconHelper.ToSymbol((KBCategoryIcon)iconValue);
                return "📂"; // default: Folder
            }
        }
    }
}
