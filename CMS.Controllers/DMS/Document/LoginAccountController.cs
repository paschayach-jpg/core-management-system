using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    public sealed class LoginAccountController : ILoginAccountController
    {
        private readonly IUserAccountRepository _userAccountRepository;

        public LoginAccountController(IUserAccountRepository userAccountRepository)
        {
            _userAccountRepository = userAccountRepository;
        }

        public UserAccount GetUserByAdUserName(string adUserName)
        {
            if (_userAccountRepository == null || string.IsNullOrEmpty(adUserName))
                return null;
            return _userAccountRepository.GetByADUserName(adUserName);
        }

        public void UpdateLastLoginWithIP(int userId, string ipAddress)
        {
            if (_userAccountRepository == null) return;
            _userAccountRepository.UpdateLastLoginWithIP(userId, ipAddress);
        }

        public int RegisterOrUpdate(UserAccount account, string ipAddress, out bool isNew)
        {
            if (_userAccountRepository == null) { isNew = false; return 0; }
            return _userAccountRepository.RegisterOrUpdate(account, ipAddress, out isNew);
        }
    }
}
