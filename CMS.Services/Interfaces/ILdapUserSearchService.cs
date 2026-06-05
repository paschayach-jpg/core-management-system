using System.Collections.Generic;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Searches Active Directory users via LDAP.
    /// Config keys: LdapPath, LdapUsername, LdapPassword, LdapBaseDN
    /// </summary>
    public interface ILdapUserSearchService
    {
        /// <summary>Search by name, username, or email. Returns up to maxResults entries.</summary>
        List<LdapUserResult> Search(string keyword, int maxResults = 50);

        /// <summary>Lookup single user by sAMAccountName.</summary>
        LdapUserResult GetByUsername(string samAccountName);
    }
}
