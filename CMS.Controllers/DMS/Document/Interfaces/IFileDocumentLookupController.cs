using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IFileDocumentLookupController
    {
        IList<FileDocument> GetByFolder(int folderId);
        IList<FileDocument> GetAllGroupedByFolder();
    }
}
