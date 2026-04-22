using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IApprovalFolderController
    {
        void RequestFolderDelete(int folderId, string reason, UserContext user);
    }
}
