using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Service interface สำหรับ Quick Access / Favorite Folder
    /// Requirements: 7.1, 7.2, 7.3, 7.4, 7.6
    /// </summary>
    public interface IQuickAccessService
    {
        /// <summary>คืนรายการ QuickAccessFolder ของ user พร้อม Folder property ที่ populate แล้ว</summary>
        IEnumerable<QuickAccessFolder> GetByUser(int userId);

        /// <summary>เพิ่ม Folder เข้า Quick Access ของ user (duplicate ถูก ignore gracefully)</summary>
        void Add(int userId, int folderId);

        /// <summary>ลบ Folder ออกจาก Quick Access ของ user</summary>
        void Remove(int userId, int folderId);
    }
}
