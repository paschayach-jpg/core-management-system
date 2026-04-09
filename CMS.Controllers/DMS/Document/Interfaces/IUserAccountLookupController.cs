using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IUserAccountLookupController
    {
        IList<UserAccount> GetAllUsers();
        UserAccount GetUserById(int userId);
        UserAccount GetUserByAdUserName(string adUserName);
        int CreateUser(UserAccount account);
    }
}
