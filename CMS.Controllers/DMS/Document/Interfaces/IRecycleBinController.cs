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

        IList<FileDocument> GetDeletedFiles(int? orgUnitId, UserContext user);
        void RestoreFile(int fileId, UserContext user);
        void PermanentlyDeleteFile(int fileId, UserContext user);
    }
}
