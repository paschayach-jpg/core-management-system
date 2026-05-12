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
        /// <summary>
        /// ค้นหา Folder ทั้งหมดที่ user มีสิทธิ์เข้าถึง
        /// ใช้สำหรับ FolderPickerDialog และ ShareDialog เพื่อแสดงเฉพาะ folder ที่มีสิทธิ์
        /// </summary>
        IList<Folder> GetAccessibleFolders(int? orgUnitId, UserContext user);
        Folder GetFolder(int folderId, UserContext user);
        IList<Folder> GetDescendantsByUser(int folderId);
        void InvalidateFolderCache(int? orgUnitId = null);
    }
}
