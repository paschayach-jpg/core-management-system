using System.Collections.Generic;
using System.Threading.Tasks;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IFolderRepository
    {
        int Insert(Folder folder);
        void Update(Folder folder);
        void UpdateFileCount(int folderId);
        Folder GetById(int folderId);
        IEnumerable<Folder> GetChildren(int parentFolderId);
        IEnumerable<Folder> GetDescendants(int folderId);
        IEnumerable<Folder> GetAllRootFolders();
        IEnumerable<Folder> GetRootFoldersByOrgUnit(int orgUnitId);
        IEnumerable<Folder> GetAllDeletedFolders();
        IEnumerable<Folder> GetDeletedByOrgUnit(int orgUnitId);
        IEnumerable<Folder> GetByOrgUnit(int orgUnitId);
        IEnumerable<Folder> GetAll();
        IEnumerable<Folder> GetActiveFolders();
        IEnumerable<Folder> GetSiblings(int? parentFolderId, int orgUnitId);
        void SoftDelete(int folderId, int deletedBy);
        void Restore(int folderId);
        void PermanentDelete(int folderId);

        Task<Folder> GetByIdAsync(int folderId);
        Task<IEnumerable<Folder>> GetByOrgUnitAsync(int orgUnitId);
        Task<IEnumerable<Folder>> GetAllAsync();
        Task<IEnumerable<Folder>> GetActiveFoldersAsync();
    }
}
