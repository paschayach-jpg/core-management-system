using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.KMS.Interfaces
{
    public interface IArticleViewerView
    {
        /// <summary>
        /// Populate all visible article fields (header, badges, metadata, HTML content).
        /// Called by the presenter on Initialize() and after a successful edit.
        /// </summary>
        void PopulateArticle(KBArticle article);

        /// <summary>Bind the sidebar version-history list.</summary>
        void BindVersionHistory(IEnumerable<KBVersion> versions);

        void ShowError(string message);
        void ShowInfo(string message);
    }
}
