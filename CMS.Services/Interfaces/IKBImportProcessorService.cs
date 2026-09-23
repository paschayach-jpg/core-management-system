using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IKBImportProcessorService
    {
        void ValidateRows(IList<KBImportRow> rows);

        /// <param name="user">
        /// Current user — needed (not just a "createdBy" string) because Order rows upload
        /// attachment files via <c>IFileService</c>, which requires the full <c>UserContext</c>.
        /// </param>
        /// <param name="skipRowOnDownloadFailure">
        /// Order rows only: when an attachment download/upload fails, <c>true</c> fails the
        /// whole row (no insert); <c>false</c> inserts the article anyway with a warning note.
        /// Ignored for Law rows (no attachment involved).
        /// </param>
        void InsertArticles(
            IList<KBImportRow> rows,
            UserContext user,
            bool skipRowOnDownloadFailure,
            out int successCount,
            out int errorCount,
            Action<int, int> onProgress);
    }
}
