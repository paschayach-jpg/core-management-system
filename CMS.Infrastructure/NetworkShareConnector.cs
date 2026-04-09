using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using CMS.Infrastructure.Interfaces;

namespace CMS.Infrastructure
{
    /// <summary>
    /// Establishes an authenticated SMB session to a UNC share using credentials from configuration.
    /// This avoids Windows Security credential prompts during file access.
    /// </summary>
    public sealed class NetworkShareConnector
    {
        private readonly IConfigManager _configManager;

        public NetworkShareConnector(IConfigManager configManager)
        {
            if (configManager == null) throw new ArgumentNullException("configManager");
            _configManager = configManager;
        }

        /// <summary>
        /// Tries to connect to configured UNC share.
        /// Returns true when connection succeeds or feature is disabled.
        /// </summary>
        public bool TryEnsureConnected(out string message)
        {
            message = null;

            bool enabled = ReadBool("NetworkShareCredentialEnabled", false);
            if (!enabled)
            {
                return true;
            }

            string configuredPath = ReadString("NetworkDrivePath", null);
            string remoteRoot = NormalizeRemoteRoot(ReadString("NetworkShareTarget", configuredPath));
            if (string.IsNullOrWhiteSpace(remoteRoot))
            {
                message = "Network share is not configured. Please set NetworkDrivePath or NetworkShareTarget.";
                return false;
            }

            string username = ReadString("NetworkShareUser", null);
            string password = ReadPassword();
            string domain = ReadString("NetworkShareDomain", null);

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                message = "Network share credentials are missing. Please set NetworkShareUser and password config.";
                return false;
            }

            string fullUser = string.IsNullOrWhiteSpace(domain) ? username : (domain + "\\" + username);

            // Clear previous cached session for this share to prevent credential conflict (1219).
            WNetCancelConnection2(remoteRoot, 0, true);

            var resource = new NETRESOURCE
            {
                dwType = 1, // RESOURCETYPE_DISK
                lpRemoteName = remoteRoot
            };

            int result = WNetAddConnection2(ref resource, password, fullUser, 0);
            if (result == 0 || result == 1219)
            {
                return true;
            }

            message = string.Format(
                "Unable to connect to network share '{0}' as '{1}'. Win32Error={2} ({3})",
                remoteRoot,
                fullUser,
                result,
                new Win32Exception(result).Message);
            return false;
        }

        private string ReadPassword()
        {
            string protectedValue = ReadString("NetworkSharePasswordProtected", null);
            if (!string.IsNullOrWhiteSpace(protectedValue))
            {
                try
                {
                    byte[] encrypted = Convert.FromBase64String(protectedValue);
                    byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                    return Encoding.UTF8.GetString(decrypted);
                }
                catch
                {
                    // Fall back to plain-text key if protected value is invalid.
                }
            }

            return ReadString("NetworkSharePassword", null);
        }

        private bool ReadBool(string key, bool defaultValue)
        {
            string value = ReadString(key, null);
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            bool parsed;
            if (bool.TryParse(value, out parsed)) return parsed;

            return defaultValue;
        }

        private string ReadString(string key, string defaultValue)
        {
            try
            {
                var value = _configManager.Get<string>(key);
                return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
            }
            catch
            {
                return defaultValue;
            }
        }

        private static string NormalizeRemoteRoot(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            string p = path.Trim();

            if (!p.StartsWith("\\\\", StringComparison.Ordinal))
            {
                return null;
            }

            // Keep only \\server\share from a deeper UNC path.
            string[] parts = p.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return null;

            return "\\\\" + parts[0] + "\\" + parts[1];
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NETRESOURCE
        {
            public int dwScope;
            public int dwType;
            public int dwDisplayType;
            public int dwUsage;
            public string lpLocalName;
            public string lpRemoteName;
            public string lpComment;
            public string lpProvider;
        }

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetAddConnection2(
            ref NETRESOURCE lpNetResource,
            string lpPassword,
            string lpUserName,
            int dwFlags);

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetCancelConnection2(
            string lpName,
            int dwFlags,
            bool fForce);
    }
}
