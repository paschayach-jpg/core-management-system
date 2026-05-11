using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IKBImportProcessorService
    {
        void ValidateRows(IList<KBImportRow> rows);
        void ResolveCategories(IList<KBImportRow> rows, string createdBy);
        void InsertArticles(
            IList<KBImportRow> rows,
            string createdBy,
            out int successCount,
            out int errorCount,
            Action<int, int> onProgress);
    }
}
