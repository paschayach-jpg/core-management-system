using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IKBArticleRepository
    {
        IEnumerable<KBArticle> GetAll();
        IEnumerable<KBArticle> GetByCategory(int categoryId);
        IEnumerable<KBArticle> Search(string keyword, int? categoryId, byte? status, byte? articleType);
        KBArticle GetById(int articleId);
        int Insert(KBArticle article);
        void Update(KBArticle article);
        void UpdateStatus(int articleId, byte status, string reviewedBy);
        void IncrementViewCount(int articleId);
        void IncrementLikeCount(int articleId);

        /// <summary>คืน sequence ถัดไปสำหรับปีที่กำหนด เพื่อสร้าง ArticleCode รูปแบบ KB-yyyy-NNN</summary>
        int GetNextCodeSequence(int year);
    }
}
