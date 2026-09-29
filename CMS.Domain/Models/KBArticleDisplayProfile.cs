using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// ชุด config header/การซ่อนแสดงคอลัมน์ของ dgvArticles/ArticleCardControl ใน KBMainForm — ผูกกับ
    /// KBCategory.DisplayProfileID (1 profile ใช้ร่วมกันได้หลายหมวด ผ่านการ inherit ตาม ancestry)
    /// Maps to KMS.m_KBArticleDisplayProfile
    /// </summary>
    public class KBArticleDisplayProfile
    {
        public int      ProfileID   { get; set; }
        public string   ProfileName { get; set; }
        public string   Description { get; set; }
        public bool     IsActive    { get; set; }
        public string   CreatedBy   { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
