using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    /// <summary>
    /// Repository interface for managing KB article attachments (KMS.t_KBArticleAttachment)
    /// </summary>
    public interface IKBArticleAttachmentRepository
    {
        /// <summary>
        /// Get all attachments for a specific article
        /// </summary>
        IEnumerable<KBArticleAttachment> GetAttachmentsForArticle(int articleId);

        /// <summary>
        /// Insert a new article attachment
        /// </summary>
        int Insert(KBArticleAttachment attachment);

        /// <summary>
        /// Delete a specific attachment by ID
        /// </summary>
        void Delete(int attachmentId);

        /// <summary>
        /// Delete all attachments for a specific article
        /// </summary>
        void DeleteAllForArticle(int articleId);
    }
}
