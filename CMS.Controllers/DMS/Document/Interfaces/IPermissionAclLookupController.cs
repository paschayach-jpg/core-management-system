using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IPermissionAclLookupController
    {
        /// <summary>ACL ทั้งหมดที่ user มีสิทธิ์อยู่ (ตรงตัว/ตาม Role/Department/Division) — สำหรับ diagnostic view</summary>
        IList<FolderPermission> GetAllForUser(int userId, int roleId, int? deptId, int? divId);
    }
}
