using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IFolderReadLookupController
    {
        IList<Folder> GetActiveFolders();
        IList<Folder> GetChildren(int folderId);
        Folder GetById(int folderId);
    }
}
