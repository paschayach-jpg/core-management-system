using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    /// <summary>
    /// Repository for managing KB article links to entities (Folder/Task/Program/Law/Project)
    /// </summary>
    public interface IKBArticleLinkRepository
    {
        /// <summary>
        /// Get all links for an article, optionally filtered by entity type
        /// </summary>
        /// <param name="articleId">Article ID</param>
        /// <param name="entityType">Optional entity type filter (1=Folder, 2=Task, 3=Program, 4=Law, 5=Project, 6=Article, null=all)</param>
        /// <returns>Collection of article links</returns>
        IEnumerable<KBArticleLink> GetLinksForArticle(int articleId, byte? entityType = null);

        /// <summary>
        /// Insert a new article link
        /// </summary>
        /// <param name="link">Article link to insert</param>
        /// <returns>LinkID of the inserted record</returns>
        int Insert(KBArticleLink link);

        /// <summary>
        /// Delete a specific link by LinkID
        /// </summary>
        /// <param name="linkId">Link ID to delete</param>
        void Delete(int linkId);

        /// <summary>
        /// Delete all links for an article, optionally filtered by entity type
        /// </summary>
        /// <param name="articleId">Article ID</param>
        /// <param name="entityType">Optional entity type filter</param>
        void DeleteAllForArticle(int articleId, byte? entityType = null);
    }
}
