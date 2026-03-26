using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    public interface IFilePermissionService
    {
        void SetFilePermission(SetFilePermissionRequest request, UserContext user);
        void RevokeFilePermission(int filePermissionId, UserContext user, string reason = null);
    }
}
