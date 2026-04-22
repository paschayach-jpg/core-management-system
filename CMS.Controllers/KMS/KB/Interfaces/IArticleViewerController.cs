using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.KMS.KB.Interfaces
{
    public interface IArticleViewerController
    {
        /// <summary>
        /// Populate all visible article fields (header, badges, metadata, HTML content).
        /// Called by the presenter on Initialize() and after a successful edit.
        /// </summary>
        void PopulateArticle(KBArticle article);

        /// <summary>Bind the sidebar version-history list.</summary>
        void BindVersionHistory(IEnumerable<KBVersion> versions);

        /// <summary>Bind the related articles tab with articles linked via KMS.t_KBArticleLink (EntityType=6).</summary>
        void BindRelatedArticles(IEnumerable<KBArticle> articles);

        void ShowError(string message);
        void ShowInfo(string message);
    }
}
