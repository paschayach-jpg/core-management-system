using Microsoft.Win32;

namespace CMS.Presentation
{
    /// <summary>
    /// Session singleton สำหรับ UI layer — เก็บ Token และข้อมูล User หลัง login สำเร็จ
    /// HTTP auth calls ทำผ่าน IAdAuthService (AdAuthService) ใน CMS.Services
    /// </summary>
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

        // ─────────────────────────────────────────────────────────────────────
        // Setters (เรียกหลัง login สำเร็จจาก LoginForm / Program)
        // ─────────────────────────────────────────────────────────────────────

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

        // ─────────────────────────────────────────────────────────────────────
        // Registry persistence (optional — ใช้เมื่อต้องการ remember session)
        // ─────────────────────────────────────────────────────────────────────

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
