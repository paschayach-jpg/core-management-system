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

        // ── Write ─────────────────────────────────────────────────────────────
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
