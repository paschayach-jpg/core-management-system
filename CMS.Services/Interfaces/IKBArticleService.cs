using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Service interface สำหรับ Knowledge Base Article Management (MOD-006)
    /// </summary>
    public interface IKBArticleService
    {
        // ── Read ──────────────────────────────────────────────────────────────
        IEnumerable<KBArticle> GetAll();
        IEnumerable<KBArticle> GetByCategory(int categoryId);
        IEnumerable<KBArticle> Search(string keyword, int? categoryId, byte? status, byte? articleType);
        KBArticle GetById(int articleId);

        /// <summary>
        /// ค้นหาบทความที่เผยแพร่แล้ว (Status = 3) โดยใช้ Article Code หรือ Title
        /// ใช้สำหรับ inline search ใน KB Editor
        /// </summary>
        IEnumerable<KBArticle> SearchPublishedArticles(string searchText);

        // ── Write ─────────────────────────────────────────────────────────────
        /// <summary>
        /// สร้าง ArticleCode ถัดไปในรูปแบบ KB-YYYY-NNN (เช่น KB-2566-001)
        /// ใช้แสดง Preview ในฟอร์มก่อน Save จริง
        /// </summary>
        string GenerateNextArticleCode();

        /// <summary>สร้างบทความใหม่ คืน ArticleID (เริ่มต้นที่ Status=Draft)</summary>
        int Create(KBArticle article);

        /// <summary>แก้ไขบทความ + บันทึก Version Snapshot</summary>
        void Update(KBArticle article, string changeDescription);

        /// <summary>เปลี่ยน Status บทความ (Submit for Review / Publish / Archive)</summary>
        void ChangeStatus(int articleId, byte newStatus, string actionBy);

        // ── Version History ───────────────────────────────────────────────────
        IEnumerable<KBVersion> GetVersionHistory(int articleId);

        // ── Engagement ────────────────────────────────────────────────────────
        void IncrementViewCount(int articleId);
        void IncrementLikeCount(int articleId);
    }
}
