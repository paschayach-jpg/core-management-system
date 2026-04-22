using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IAuthorityConfigController
    {
        IList<FolderAuthorityConfig> GetAll();
        int Save(FolderAuthorityConfig config, int userId);
        void Delete(string configLabel, int userId);
    }
}
