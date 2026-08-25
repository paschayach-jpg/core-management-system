using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Attachments for KB articles.
    /// Maps to KMS.t_KBArticleAttachment.
    /// </summary>
    public class KBArticleAttachment
    {
        public int AttachmentID { get; set; }  // PK
        public int ArticleID { get; set; }     // FK -> KMS.t_KBArticle
        public int FileID { get; set; }        // FK -> storage.FileDocument
        public string CreatedBy { get; set; }   // ADUserName who attached the file
        public DateTime CreatedDate { get; set; } // When the attachment was made
    }
}
