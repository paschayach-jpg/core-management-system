using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IAuthorityImportController
    {
        IList<string> GetSheetNames(string filePath, UserContext user);
        AuthorityImportPreview BuildPreview(string filePath, string sheetName, bool updateUserLevel, bool importPositions, UserContext user);
        AuthorityImportResult Commit(AuthorityImportPreview preview, UserContext user);
    }
}
