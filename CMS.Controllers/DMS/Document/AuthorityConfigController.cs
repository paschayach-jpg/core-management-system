using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class AuthorityConfigController : IAuthorityConfigController
    {
        private readonly IFolderAuthorityService _folderAuthorityService;

        public AuthorityConfigController(IFolderAuthorityService folderAuthorityService = null)
        {
            _folderAuthorityService = folderAuthorityService;
        }

        public IList<FolderAuthorityConfig> GetAll()
        {
            if (_folderAuthorityService == null)
                return new List<FolderAuthorityConfig>();

            return _folderAuthorityService.GetAll().ToList();
        }

        public int Save(FolderAuthorityConfig config, int userId)
        {
            if (_folderAuthorityService == null)
                return 0;

            return _folderAuthorityService.Save(config, userId);
        }

        public void Delete(string configLabel, int userId)
        {
            if (_folderAuthorityService == null)
                return;

            _folderAuthorityService.Delete(configLabel, userId);
        }

        public Dictionary<int, FolderAuthorityConfig> ResolveForUser(UserAccount user)
        {
            if (_folderAuthorityService == null || user == null)
                return new Dictionary<int, FolderAuthorityConfig>();

            return _folderAuthorityService.ResolveForUser(user);
        }
    }
}
