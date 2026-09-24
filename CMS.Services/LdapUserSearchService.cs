using System;
using System.Collections.Generic;
using System.DirectoryServices;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Searches Active Directory via LDAP using System.DirectoryServices.
    /// Config keys read from IConfigManager:
    ///   LdapPath      — e.g. LDAP://150.1.8.3/DC=bui_domain1
    ///   LdapUsername  — bind account (sAMAccountName or domain\user)
    ///   LdapPassword  — bind account password
    /// </summary>
    public class LdapUserSearchService : ILdapUserSearchService
    {
        private readonly string _ldapPath;
        private readonly string _ldapUsername;
        private readonly string _ldapPassword;

        public LdapUserSearchService(IConfigManager configManager)
        {
            if (configManager == null) throw new ArgumentNullException("configManager");

            _ldapPath     = configManager.GetOrDefault<string>("LdapPath",     "");
            _ldapUsername = configManager.GetOrDefault<string>("LdapUsername",  "");
            _ldapPassword = configManager.GetOrDefault<string>("LdapPassword",  "");
        }

        public List<LdapUserResult> Search(string keyword, int maxResults = 50)
        {
            var results = new List<LdapUserResult>();

            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
                return results;

            if (string.IsNullOrEmpty(_ldapPath))
                return results;

            try
            {
                // Sanitize keyword — prevent LDAP injection
                keyword = EscapeLdap(keyword);

                string filter = string.Format(
                    "(&(objectClass=user)(objectCategory=person)(!(userAccountControl:1.2.840.113556.1.4.803:=2))" +
                    "(|(sAMAccountName=*{0}*)(displayName=*{0}*)(mail=*{0}*)(cn=*{0}*)))",
                    keyword);

                using (var entry = new DirectoryEntry(_ldapPath, _ldapUsername, _ldapPassword,
                    AuthenticationTypes.Secure))
                using (var searcher = new DirectorySearcher(entry, filter))
                {
                    searcher.PropertiesToLoad.AddRange(new[]
                    {
                        "sAMAccountName", "displayName", "mail", "department", "title", "employeeID"
                    });
                    searcher.SizeLimit = maxResults;
                    searcher.PageSize  = maxResults;

                    using (var found = searcher.FindAll())
                    {
                        foreach (SearchResult sr in found)
                        {
                            var u = MapResult(sr);
                            if (u != null)
                                results.Add(u);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Surface to caller so form can decide to fall back
                throw new InvalidOperationException("LDAP search failed: " + ex.Message, ex);
            }

            return results;
        }

        public LdapUserResult GetByUsername(string samAccountName)
        {
            if (string.IsNullOrWhiteSpace(samAccountName) || string.IsNullOrEmpty(_ldapPath))
                return null;

            try
            {
                string filter = string.Format(
                    "(&(objectClass=user)(sAMAccountName={0}))",
                    EscapeLdap(samAccountName));

                using (var entry = new DirectoryEntry(_ldapPath, _ldapUsername, _ldapPassword,
                    AuthenticationTypes.Secure))
                using (var searcher = new DirectorySearcher(entry, filter))
                {
                    searcher.PropertiesToLoad.AddRange(new[]
                    {
                        "sAMAccountName", "displayName", "mail", "department", "title", "employeeID"
                    });

                    var sr = searcher.FindOne();
                    return sr != null ? MapResult(sr) : null;
                }
            }
            catch
            {
                return null;
            }
        }

        public List<LdapUserResult> SearchAllUsers(int pageSize = 1000, int? maxResults = null)
        {
            var results = new List<LdapUserResult>();

            if (string.IsNullOrEmpty(_ldapPath))
                return results;

            try
            {
                // Includes disabled accounts on purpose — callers need to see IsActive=false
                // for users that already exist, unlike Search() which excludes them.
                const string filter = "(&(objectClass=user)(objectCategory=person)(sAMAccountName=*))";

                using (var entry = new DirectoryEntry(_ldapPath, _ldapUsername, _ldapPassword,
                    AuthenticationTypes.Secure))
                using (var searcher = new DirectorySearcher(entry, filter))
                {
                    searcher.PropertiesToLoad.AddRange(new[]
                    {
                        "sAMAccountName", "displayName", "mail", "department", "title", "employeeID", "userAccountControl"
                    });
                    searcher.PageSize = pageSize;
                    if (maxResults.HasValue)
                        searcher.SizeLimit = maxResults.Value;
                    searcher.ClientTimeout = TimeSpan.FromMinutes(5);
                    searcher.ServerTimeLimit = TimeSpan.FromMinutes(5);

                    using (var found = searcher.FindAll())
                    {
                        foreach (SearchResult sr in found)
                        {
                            var u = MapResult(sr, includeActiveFlag: true);
                            if (u != null)
                                results.Add(u);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("LDAP search failed: " + ex.Message, ex);
            }

            return results;
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private static LdapUserResult MapResult(SearchResult sr)
        {
            return MapResult(sr, includeActiveFlag: false);
        }

        private static LdapUserResult MapResult(SearchResult sr, bool includeActiveFlag)
        {
            string sam = GetProp(sr, "sAMAccountName");
            if (string.IsNullOrEmpty(sam))
                return null;

            return new LdapUserResult
            {
                SamAccountName = sam,
                DisplayName    = GetProp(sr, "displayName") ?? sam,
                Mail           = GetProp(sr, "mail"),
                Department     = GetProp(sr, "department"),
                Title          = GetProp(sr, "title"),
                EmployeeId     = GetProp(sr, "employeeID"),
                IsActive       = includeActiveFlag ? IsAccountActive(sr) : true
            };
        }

        private static bool IsAccountActive(SearchResult sr)
        {
            string uacRaw = GetProp(sr, "userAccountControl");
            int uac;
            if (string.IsNullOrEmpty(uacRaw) || !int.TryParse(uacRaw, out uac))
                return true; // unknown -> assume active rather than silently deactivating
            const int ACCOUNTDISABLE = 0x2;
            return (uac & ACCOUNTDISABLE) == 0;
        }

        private static string GetProp(SearchResult sr, string name)
        {
            if (!sr.Properties.Contains(name) || sr.Properties[name].Count == 0)
                return null;
            return sr.Properties[name][0]?.ToString();
        }

        // Escape special LDAP filter characters (RFC 4515)
        private static string EscapeLdap(string s)
        {
            return s.Replace("\\", "\\5c")
                    .Replace("*",  "\\2a")
                    .Replace("(",  "\\28")
                    .Replace(")",  "\\29")
                    .Replace("\0", "\\00");
        }
    }
}
