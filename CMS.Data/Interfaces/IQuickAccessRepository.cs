using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    /// <summary>
    /// Repository interface สำหรับ Quick Access / Favorite Folder
    /// Requirements: 7.2, 7.4, 7.5
    /// </summary>
    public interface IQuickAccessRepository
    {
        IEnumerable<QuickAccessFolder> GetByUser(int userId);
        void Add(int userId, int folderId);
        void Remove(int userId, int folderId);
    }
}
