using Microsoft.Win32;

namespace CMS.Services
{
    public class SessionContext
    {
        private static SessionContext _current;
        private const string RegistryPath = @"Software\BIS";
        private const string TokenKey     = "accessToken";

        public static SessionContext Current
        {
            get { return _current ?? (_current = new SessionContext()); }
        }

        public string Token        { get; private set; }
        public string UserID       { get; private set; }
        public string ADUserName   { get; private set; }
        public string RefreshToken { get; private set; }
        public string TokenExpired { get; private set; }

        public bool IsLoggedIn => !string.IsNullOrEmpty(Token);

        public void SetToken(string token)           { Token        = token; }
        public void SetUserID(string userId)         { UserID       = userId; }
        public void SetADUserName(string adUserName) { ADUserName   = adUserName; }
        public void SetRefreshToken(string token)    { RefreshToken = token; }
        public void SetTokenExpired(string expired)  { TokenExpired = expired; }

        public void Logout()
        {
            Token = RefreshToken = TokenExpired = UserID = ADUserName = null;
            DeleteRegistryToken();
        }

        public static void SaveToRegistry(string token, string refreshToken, string expired)
        {
            using (var key = Registry.LocalMachine.CreateSubKey(RegistryPath))
            {
                key.SetValue("accessToken",         token);
                key.SetValue("refreshToken",         refreshToken);
                key.SetValue("refreshTokenExpired",  expired);
            }
        }

        public static string LoadTokenFromRegistry()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(RegistryPath))
                return key?.GetValue(TokenKey)?.ToString();
        }

        private static void DeleteRegistryToken()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(RegistryPath, writable: true))
            {
                if (key?.GetValue(TokenKey) != null)
                {
                    key.DeleteValue("accessToken");
                    key.DeleteValue("refreshToken");
                    key.DeleteValue("refreshTokenExpired");
                }
            }
        }
    }
}
