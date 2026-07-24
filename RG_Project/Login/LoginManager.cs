using RG_Project.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
namespace RG_Project.Login
{

    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public string sid { get; set; }
    }
    //printpalcontext
    public class LoginResponse
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }

        [JsonProperty("refreshTokenExpired")]
        public string RefreshTokenExpired { get; set; }

        [JsonProperty("employee_Info")]
        public EmployeeInfo EmployeeInfo { get; set; }

    }

   
    public class UserInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

    }

    public class LoginResponse_UserInfo
    {
        public UserInfo UserInfo { get; set; }
        [JsonProperty("employee_Info")]
        public EmployeeInfo EmployeeInfo { get; set; }

    }
    public class LoginManager
    {
        private string _apiUrl = "/login";
        private string _apiUrlvalidate = "/validate-token"; 
        private string _apiUrlrefreshtoken = "/refresh-token";
        private string _apiUrllogin_with_sid = "/login_with_sid";
        public EmployeeInfo employeeInfo { get; set; }
        public string Token { get; private set; }
        public string UserID { get; private set; }
        public string _UserName { get; private set; }
        public string _RefreshToken { get; private set; }
        public string _TokenExpired { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(Token);
        public string GetUserID => UserID;
        public string GetUserName => _UserName;
        public string GetRefreshToken => _RefreshToken;
        public string GetTokenExpired => _TokenExpired;

        private static LoginManager _instance;
        private const string RegistryPath = @"Software\BIS";  // เปลี่ยนชื่อแอปตามต้องการ
        private const string TokenKeyName = "accessToken";
        public static LoginManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LoginManager();
                }
                return _instance;
            }
        }
      
        public string setting()
        {
            // ตรวจสอบว่า Environment มีค่าไหม
            string apiUrl = ConfigurationManager.AppSettings["login"];
            // เลือก URL ตาม Environment

            return apiUrl;

        }

        public async Task<bool> CheckLoginAsync(string username, string password)
        {
            var loginData = new LoginRequest
            {
                UserName = username,
                Password = password
            };

            string json = JsonConvert.SerializeObject(loginData);

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync(setting()+_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                        // แสดงผลข้อมูล
                        Console.WriteLine($"AccessToken: {result.AccessToken}");
                        Console.WriteLine($"RefreshToken: {result.RefreshToken}");
                        Console.WriteLine($"RefreshTokenExpired: {result.RefreshTokenExpired}");
                        Token = result.AccessToken; // เก็บ Token สำหรับใช้ในฟอร์มอื่น
                        return true;
                    }
                    else
                    {
                        return false; // Login ล้มเหลว
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }
        
        public async Task<string> LoginAsync(string username, string password)
        {
            var loginData = new LoginRequest
            {
                UserName = username,
                Password = password
            };

            string json = JsonConvert.SerializeObject(loginData);

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync(setting()+_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
                        // แสดงผลข้อมูล
                        Console.WriteLine($"accessToken: {result.AccessToken}");
                        Console.WriteLine($"refreshToken: {result.RefreshToken}");
                        Console.WriteLine($"refreshTokenExpired: {result.RefreshTokenExpired}");

                        Token = result.AccessToken; // เก็บ Token สำหรับใช้ในฟอร์มอื่น
                       
                        _TokenExpired = result.RefreshTokenExpired;
                        _RefreshToken = result.RefreshToken;
                        SetToken(Token);
                        //SaveToken(Token, _RefreshToken, _TokenExpired);
                        SetTokenExpired(_TokenExpired);
                        SetRefreshToken(_RefreshToken);
                        employeeInfo = result.EmployeeInfo;
                        UserID = result.EmployeeInfo.EmployeeCode;
                        SetUserName(username);
                        SetUserID(UserID);
                        return Token;
                    }
                    else
                    {
                        return null; // Login ล้มเหลว
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return null;
                }
            }
        }
        public async Task<string> refreshtoken()
        {
            var request = new LoginRequest
            {
                accessToken = Token,
                refreshToken = _RefreshToken
            };

            string json = JsonConvert.SerializeObject(request);

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync(setting() + _apiUrlrefreshtoken, content);

                    if (response.IsSuccessStatusCode)
                    {

                        string responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
                        // แสดงผลข้อมูล
                        Console.WriteLine($"accessToken: {result.AccessToken}");
                        Console.WriteLine($"refreshToken: {result.RefreshToken}");
                        Console.WriteLine($"refreshTokenExpired: {result.RefreshTokenExpired}");
                        Token = result.AccessToken; // เก็บ Token สำหรับใช้ในฟอร์มอื่น
                        _TokenExpired = result.RefreshTokenExpired;
                        _RefreshToken = result.RefreshToken;
                        SetToken(Token);
                        SetTokenExpired(_TokenExpired);
                        SetRefreshToken(_RefreshToken);
                        return Token;
                    }
                    else
                    {
                        return null; // Login ล้มเหลว
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return null;
                }
            }
        }
        public async Task<List<string>> GetUserNameLogin(string Token)
        {
            var _Token = new LoginRequest
            {
                accessToken = Token
            };

            string json = JsonConvert.SerializeObject(_Token);

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync(setting() + _apiUrlvalidate, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<LoginResponse_UserInfo>(responseContent);

                        // ดึงข้อมูล userId และ userName
                        string userId = result.UserInfo.UserId;
                        string userName = result.UserInfo.UserName;
                        SetUserID(userId);
                        SetUserName(userName);
                        return new List<string> { userName };
                    }
                    else
                    {
                        return (null); // Login ล้มเหลว
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return (null);
                }
            }
        }
       

        public async Task<string> login_with_sid()
        {

            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            string username = windowsIdentity.Name;
            string sid = windowsIdentity.User.Value;

            var loginData = new LoginRequest
            {
                sid = sid
         
            };

            string json = JsonConvert.SerializeObject(loginData);

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync(setting() + _apiUrllogin_with_sid, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
                        // แสดงผลข้อมูล
                        Console.WriteLine($"accessToken: {result.AccessToken}");
                        Console.WriteLine($"refreshToken: {result.RefreshToken}");
                        Console.WriteLine($"refreshTokenExpired: {result.RefreshTokenExpired}");

                        Token = result.AccessToken; // เก็บ Token สำหรับใช้ในฟอร์มอื่น
                        _TokenExpired = result.RefreshTokenExpired;
                        _RefreshToken = result.RefreshToken;
                        employeeInfo = result.EmployeeInfo;
                        //SaveToken(Token, _RefreshToken, _TokenExpired);
                        SetToken(Token);
                        await GetUserNameLogin(Token);
                        SetTokenExpired(_TokenExpired);
                        SetRefreshToken(_RefreshToken);
                        UserID = result.EmployeeInfo.EmployeeCode;
                        SetUserID(UserID);
                        return Token;
                    }
                    else
                    {
                        return null; // Login ล้มเหลว
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<string> login_with_sid_1(string Token)
        {
            // 1. ใช้ Token ดึง user ก่อน
            var user = await GetUserNameLogin(Token);

            if (user != null)
            {
                string username = user.FirstOrDefault();

                //// 2. login เพื่อดึง EmployeeInfo
                //WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
                //string sid = windowsIdentity.User.Value;

                //var loginData = new LoginRequest
                //{
                //    sid = sid
                //};
                NTAccount account = new NTAccount(username);
                SecurityIdentifier sid =
                    (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));

                var loginData = new LoginRequest
                {
                    sid = sid.Value
                };

                string json = JsonConvert.SerializeObject(loginData);

                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(setting() + _apiUrllogin_with_sid, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        var result = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                        employeeInfo = result.EmployeeInfo;

                        //if (employeeInfo != null)
                        //{
                        //    string department = employeeInfo.DepartmentNameTh;
                        //    string namefull = employeeInfo.EmployeeFirstNameTh + " " + employeeInfo.EmployeeLastNameTh;
                        //    string empCode = employeeInfo.EmployeeCode;

                        //    //SetUserID(empCode);
                        //    //SetUserName(namefull);
                        //    //SetUserID(department);
                        //}

                        return Token;
                    }
                }
            }

            return null;
        }

        public static void SaveToken(string token,string refreshToken ,string refreshTokenExpired)
        {
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(RegistryPath))
            {
                key.SetValue("accessToken", token);
                key.SetValue("refreshToken", refreshToken);
                key.SetValue("refreshTokenExpired", refreshTokenExpired);
            }
        }
        public static string LoadToken()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(RegistryPath))
            {
                if (key != null)
                {
                    return key.GetValue(TokenKeyName)?.ToString();
                }
                return null;
            }
        }

        public static void DeleteToken()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(RegistryPath, writable: true))
            {
                if (key != null && key.GetValue(TokenKeyName) != null)
                {
                    key.DeleteValue("accessToken");
                    key.DeleteValue("refreshToken");
                    key.DeleteValue("refreshTokenExpired");
                }
            }
        }

        public void SetToken(string token)
        {
            Token = token;
        }
        public void SetUserName(  string username)
        {
            _UserName = username;
        }
        public void SetUserID(string userid)
        {
            UserID = userid;
        }
        public void SetTokenExpired(string TokenExpired)
        {
            _TokenExpired = TokenExpired;
        }
        public void SetRefreshToken(string RefreshToken)
        {
            _RefreshToken = RefreshToken;
        }

        public void Logout()
        {
            Token = null;
            DeleteToken();
        }
    }
}
