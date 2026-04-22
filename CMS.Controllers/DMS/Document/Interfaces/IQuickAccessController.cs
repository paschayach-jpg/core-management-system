using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IQuickAccessController
    {
        IList<QuickAccessFolder> GetByUser(int userId);
        void Add(int userId, int folderId);
        void Remove(int userId, int folderId);
    }
}
