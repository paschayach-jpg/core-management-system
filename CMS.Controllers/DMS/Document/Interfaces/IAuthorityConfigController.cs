using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IAuthorityConfigController
    {
        IList<FolderAuthorityConfig> GetAll();
        int Save(FolderAuthorityConfig config, int userId);
        void Delete(string configLabel, int userId);

        /// <summary>Resolve สิทธิ์ทั้ง 4 Level ของ user ที่ระบุ (สำหรับ diagnostic view)</summary>
        Dictionary<int, FolderAuthorityConfig> ResolveForUser(UserAccount user);
    }
}
