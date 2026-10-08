using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    public interface IAuthorityImportService
    {
        /// <summary>Blank authority matrix template (.xlsx bytes) in the layout the import reads. Admin only.</summary>
        byte[] GenerateTemplate(UserContext user);

        /// <summary>Sheet names of the workbook. Admin only.</summary>
        IList<string> GetSheetNames(string filePath, UserContext user);

        /// <summary>
        /// Parse + validate + diff against current data. Writes nothing.
        /// importPositions=false ignores everything about positions/permissions (user-level import only).
        /// </summary>
        AuthorityImportPreview BuildPreview(string filePath, string sheetName, bool updateUserLevel, bool importPositions, UserContext user);

        /// <summary>Apply a preview that has no Errors. Compensates on failure. Admin only.</summary>
        AuthorityImportResult Commit(AuthorityImportPreview preview, UserContext user);
    }
}
