using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    public sealed class UserAccountLookupController : IUserAccountLookupController
    {
        private readonly IUserAccountRepository _userAccountRepository;

        public UserAccountLookupController(IUserAccountRepository userAccountRepository = null)
        {
            _userAccountRepository = userAccountRepository;
        }

        public IList<UserAccount> GetAllUsers()
        {
            if (_userAccountRepository == null)
                return new List<UserAccount>();

            try
            {
                return _userAccountRepository.GetAll().ToList();
            }
            catch
            {
                return new List<UserAccount>();
            }
        }

        public UserAccount GetUserById(int userId)
        {
            if (_userAccountRepository == null)
                return null;

            try
            {
                return _userAccountRepository.GetById(userId);
            }
            catch
            {
                return null;
            }
        }

        public UserAccount GetUserByAdUserName(string adUserName)
        {
            if (_userAccountRepository == null || string.IsNullOrWhiteSpace(adUserName))
                return null;

            try
            {
                return _userAccountRepository.GetByADUserName(adUserName);
            }
            catch
            {
                return null;
            }
        }

        public int CreateUser(UserAccount account)
        {
            if (_userAccountRepository == null)
                return 0;

            if (account == null)
                throw new ArgumentNullException("account");

            try
            {
                return _userAccountRepository.Create(account);
            }
            catch
            {
                return 0;
            }
        }
    }
}
