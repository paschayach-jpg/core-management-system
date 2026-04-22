using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IRecycleBinController
    {
        IList<OrgUnit> GetOrgHierarchy();
        IList<Folder> GetDeletedFolders(int? orgUnitId, UserContext user);
        void RestoreFolder(int folderId, int? overrideParentFolderId, UserContext user);
        void PermanentlyDeleteFolder(int folderId, UserContext user);
    }
}
