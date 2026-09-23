using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    public sealed class PermissionAclLookupController : IPermissionAclLookupController
    {
        private readonly IPermissionRepository _permissionRepository;

        public PermissionAclLookupController(IPermissionRepository permissionRepository = null)
        {
            _permissionRepository = permissionRepository;
        }

        public IList<FolderPermission> GetAllForUser(int userId, int roleId, int? deptId, int? divId)
        {
            if (_permissionRepository == null)
                return new List<FolderPermission>();

            return _permissionRepository.GetAllForUser(userId, roleId, deptId, divId).ToList();
        }
    }
}
