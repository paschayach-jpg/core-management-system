using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IKBImportProcessorService
    {
        /// <param name="resolvedCategoryId">
        /// CategoryID configured on the KB Import Template selected in the Wizard — applied to
        /// every row in this batch (both Law and Order layouts). Must be greater than 0.
        /// </param>
        void ValidateRows(IList<KBImportRow> rows, int resolvedCategoryId);

        /// <param name="user">
        /// Current user — needed (not just a "createdBy" string) because Order rows upload
        /// attachment files via <c>IFileService</c>, which requires the full <c>UserContext</c>.
        /// </param>
        /// <param name="skipRowOnDownloadFailure">
        /// Order rows only: when an attachment download/upload fails, <c>true</c> fails the
        /// whole row (no insert); <c>false</c> inserts the article anyway with a warning note.
        /// Ignored for Law rows (no attachment involved).
        /// </param>
        /// <param name="resolvedArticleType">
        /// ArticleType configured on the KB Import Template selected in the Wizard — applied to
        /// every row in this batch (both Law and Order layouts). Must be greater than 0.
        /// </param>
        void InsertArticles(
            IList<KBImportRow> rows,
            UserContext user,
            bool skipRowOnDownloadFailure,
            byte resolvedArticleType,
            out int successCount,
            out int errorCount,
            Action<int, int> onProgress);
    }
}
