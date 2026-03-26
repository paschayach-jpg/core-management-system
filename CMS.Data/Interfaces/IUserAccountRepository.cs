using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IUserAccountRepository
    {
        UserAccount GetByADUserName(string adUserName);
        UserAccount GetById(int userId);
        IEnumerable<UserAccount> GetByRole(int appRole);
        IEnumerable<UserAccount> GetAll();
        int Create(UserAccount account);
        void Update(UserAccount account);
        void UpdateLastLogin(int userId);
        void UpdateConsent(int userId, string email, bool consentGiven);
        void Deactivate(int userId);
    }
}
