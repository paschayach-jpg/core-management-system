using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class FolderReadLookupController : IFolderReadLookupController
    {
        private readonly IFolderRepository _folderRepository;
        private readonly IFolderService    _folderService;

        public FolderReadLookupController(
            IFolderRepository folderRepository = null,
            IFolderService    folderService    = null)
        {
            _folderRepository = folderRepository;
            _folderService    = folderService;
        }

        public IList<Folder> GetActiveFolders()
        {
            if (_folderRepository == null)
                return new List<Folder>();

            return _folderRepository.GetActiveFolders().ToList();
        }

        public IList<Folder> GetChildren(int folderId)
        {
            if (_folderRepository == null)
                return new List<Folder>();

            return _folderRepository.GetChildren(folderId).ToList();
        }

        public IList<Folder> GetChildrenWithPermission(int folderId, UserContext user)
        {
            if (_folderService == null || user == null)
                return new List<Folder>();

            try
            {
                return _folderService.GetFolderContents(folderId, user).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<Folder>();
            }
        }

        public Folder GetById(int folderId)
        {
            if (_folderRepository == null)
                return null;

            return _folderRepository.GetById(folderId);
        }

        public IList<Folder> GetAllFolders(int? orgUnitId, UserContext user)
        {
            if (_folderService == null || user == null)
                return new List<Folder>();

            return _folderService.GetAllFolders(orgUnitId, user).ToList();
        }

        public IList<Folder> GetRootFolders(int? orgUnitId, UserContext user)
        {
            if (_folderService == null || user == null)
                return new List<Folder>();

            return _folderService.GetRootFolders(orgUnitId, user).ToList();
        }

        public IList<Folder> GetFoldersByOrgUnit(int orgUnitId, UserContext user)
        {
            if (_folderService == null || user == null)
                return new List<Folder>();

            return _folderService.GetFoldersByOrgUnit(orgUnitId, user).ToList();
        }

        public Folder GetFolder(int folderId, UserContext user)
        {
            if (_folderService == null || user == null)
                return null;

            return _folderService.GetFolder(folderId, user);
        }

        public IList<Folder> GetDescendantsByUser(int folderId)
        {
            if (_folderService == null)
                return new List<Folder>();

            return _folderService.GetDescendants(folderId).ToList();
        }

        /// <summary>
        /// ค้นหา Folder ทั้งหมดที่ user มีสิทธิ์เข้าถึง
        /// ใช้สำหรับ FolderPickerDialog และ ShareDialog เพื่อแสดงเฉพาะ folder ที่มีสิทธิ์
        /// </summary>
        public IList<Folder> GetAccessibleFolders(int? orgUnitId, UserContext user)
        {
            if (_folderService == null || user == null)
                return new List<Folder>();

            return _folderService.GetAccessibleFolders(orgUnitId, user).ToList();
        }
    }
}
