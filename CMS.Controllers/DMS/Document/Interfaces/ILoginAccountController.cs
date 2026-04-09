using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface ILoginAccountController
    {
        UserAccount GetUserByAdUserName(string adUserName);
        void UpdateLastLoginWithIP(int userId, string ipAddress);
        int RegisterOrUpdate(UserAccount account, string ipAddress, out bool isNew);
    }
}
