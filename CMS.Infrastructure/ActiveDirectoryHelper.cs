using System;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Security.Principal;
using CMS.Domain.Models;

namespace CMS.Infrastructure
{
    /// <summary>
    /// Resolves the current Windows identity to a lightweight <see cref="AdUserInfo"/> value
    /// that the authentication flow can use to look up the corresponding <see cref="UserAccount"/>
    /// in the CMS database (Requirements 1.1, 1.2).
    /// </summary>
    public class ActiveDirectoryHelper
    {
        /// <summary>
        /// Returns the AD identity string (domain\username) and display name for the
        /// currently logged-in Windows user without prompting for credentials.
        /// </summary>
        /// <returns>An <see cref="AdUserInfo"/> populated from the current Windows identity.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the current process has no Windows identity (e.g. running as a service account
        /// with no impersonation token).
        /// </exception>
        public AdUserInfo GetCurrentUser()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            if (identity == null || string.IsNullOrEmpty(identity.Name))
                throw new InvalidOperationException("No Windows identity is available for the current process.");

            string adIdentity = identity.Name; // e.g. "DOMAIN\username"
            string displayName = ResolveDisplayName(adIdentity);
            string workstation = Dns.GetHostName();

            return new AdUserInfo
            {
                AdIdentity  = adIdentity,
                DisplayName = displayName,
                WorkstationId = workstation
            };
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static string ResolveDisplayName(string adIdentity)
        {
            try
            {
                // Split "DOMAIN\username" to get just the SAM account name
                string samAccountName = adIdentity.Contains("\\")
                    ? adIdentity.Substring(adIdentity.IndexOf('\\') + 1)
                    : adIdentity;

                using (var ctx = new PrincipalContext(ContextType.Domain))
                using (var user = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, samAccountName))
                {
                    if (user != null && !string.IsNullOrEmpty(user.DisplayName))
                        return user.DisplayName;
                }
            }
            catch
            {
                // AD query failed (e.g. no domain controller reachable) — fall back to identity string
            }

            return adIdentity;
        }
    }

    /// <summary>
    /// Lightweight value object returned by <see cref="ActiveDirectoryHelper.GetCurrentUser"/>.
    /// </summary>
    public class AdUserInfo
    {
        /// <summary>Windows identity in domain\username format.</summary>
        public string AdIdentity { get; set; }

        /// <summary>Display name from Active Directory, or the AdIdentity string as fallback.</summary>
        public string DisplayName { get; set; }

        /// <summary>Machine name of the workstation the user is running on.</summary>
        public string WorkstationId { get; set; }
    }
}
