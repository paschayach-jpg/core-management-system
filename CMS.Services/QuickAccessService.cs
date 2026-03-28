using System;
using System.Collections.Generic;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Service สำหรับ Quick Access / Favorite Folder
    /// Populates the transient Folder property on each QuickAccessFolder by calling IFolderRepository.GetById.
    /// Requirements: 7.1, 7.2, 7.3, 7.4, 7.6
    /// </summary>
    public class QuickAccessService : IQuickAccessService
    {
        private readonly IQuickAccessRepository _quickAccessRepository;
        private readonly IFolderRepository      _folderRepository;

        public QuickAccessService(
            IQuickAccessRepository quickAccessRepository,
            IFolderRepository      folderRepository)
        {
            if (quickAccessRepository == null) throw new ArgumentNullException("quickAccessRepository");
            if (folderRepository      == null) throw new ArgumentNullException("folderRepository");

            _quickAccessRepository = quickAccessRepository;
            _folderRepository      = folderRepository;
        }

        /// <summary>
        /// คืนรายการ QuickAccessFolder ของ user พร้อม Folder property ที่ populate แล้ว
        /// Folder ที่ถูกลบหรือ archive จะยังถูกคืนมา (Folder.IsDeleted / IsArchived = true)
        /// เพื่อให้ FolderForm แสดง visual indicator ได้ (Req 7.6)
        /// </summary>
        public IEnumerable<QuickAccessFolder> GetByUser(int userId)
        {
            var items = _quickAccessRepository.GetByUser(userId);
            foreach (var item in items)
            {
                // Populate transient Folder property
                item.Folder = _folderRepository.GetById(item.FolderID);
            }
            return items;
        }

        /// <summary>
        /// เพิ่ม Folder เข้า Quick Access ของ user
        /// Duplicate (UserID + FolderID เดิม) ถูก ignore gracefully โดย stored procedure
        /// </summary>
        public void Add(int userId, int folderId)
        {
            _quickAccessRepository.Add(userId, folderId);
        }

        /// <summary>ลบ Folder ออกจาก Quick Access ของ user</summary>
        public void Remove(int userId, int folderId)
        {
            _quickAccessRepository.Remove(userId, folderId);
        }
    }
}
