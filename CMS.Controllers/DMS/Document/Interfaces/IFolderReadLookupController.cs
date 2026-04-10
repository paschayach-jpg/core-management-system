using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IFolderReadLookupController
    {
        IList<Folder> GetActiveFolders();
        IList<Folder> GetChildren(int folderId);
        IList<Folder> GetChildrenWithPermission(int folderId, UserContext user);
        Folder GetById(int folderId);
        IList<Folder> GetAllFolders(int? orgUnitId, UserContext user);
        IList<Folder> GetRootFolders(int? orgUnitId, UserContext user);
        IList<Folder> GetFoldersByOrgUnit(int orgUnitId, UserContext user);
        Folder GetFolder(int folderId, UserContext user);
        IList<Folder> GetDescendantsByUser(int folderId);
    }
}
