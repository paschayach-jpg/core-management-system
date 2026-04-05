using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Authentication ผ่าน AD REST API
    /// Config keys ที่ต้องการ:
    ///   AdAuthApiBaseUrl   — base URL ของ AD API  (เช่น http://auth.company.local)
    ///   AdAuthLoginPath    — path สำหรับ login     (เช่น /api/auth/login)
    ///   AdAuthValidatePath — path สำหรับ validate  (เช่น /api/auth/validate)
    /// </summary>
    public class AdAuthService : IAdAuthService
    {
        // ใช้ static HttpClient เพื่อหลีกเลี่ยง socket exhaustion
        private static readonly HttpClient _http = new HttpClient();

        private readonly string _baseUrl;
        private readonly string _loginPath;
        private readonly string _validatePath;

        public AdAuthService(IConfigManager configManager)
        {
            if (configManager == null) throw new ArgumentNullException("configManager");

            try { _baseUrl      = (configManager.Get<string>("AdAuthApiBaseUrl")   ?? "").TrimEnd('/'); } catch { _baseUrl = ""; }
            try { _loginPath    = configManager.Get<string>("AdAuthLoginPath")    ?? "/api/auth/login"; }    catch { _loginPath = "/api/auth/login"; }
            try { _validatePath = configManager.Get<string>("AdAuthValidatePath") ?? "/api/auth/validate"; } catch { _validatePath = "/api/auth/validate"; }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Public
        // ─────────────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<bool> CheckLoginAsync(string username, string password)
        {
            try
            {
                string json = Serialize(new LoginRequestDto { UserName = username, Password = password });
                using (var content  = new StringContent(json, Encoding.UTF8, "application/json"))
                using (var response = await _http.PostAsync(_baseUrl + _loginPath, content).ConfigureAwait(false))
                    return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("AdAuthService.CheckLoginAsync error: " + ex.Message);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<AdAuthResult> LoginAsync(string username, string password)
        {
            try
            {
                string json = Serialize(new LoginRequestDto { UserName = username, Password = password });

                using (var content  = new StringContent(json, Encoding.UTF8, "application/json"))
                using (var response = await _http.PostAsync(_baseUrl + _loginPath, content).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                        return new AdAuthResult { Success = false, ErrorMessage = "HTTP " + (int)response.StatusCode };

                    string body      = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    LoginResponse lr = Deserialize<LoginResponse>(body);

                    return new AdAuthResult
                    {
                        Success             = true,
                        AccessToken         = lr.AccessToken,
                        RefreshToken        = lr.RefreshToken,
                        RefreshTokenExpired = ParseDate(lr.RefreshTokenExpired),
                        EmployeeInfo        = lr.EmployeeInfo,
                        AdUserName          = username   // ใช้ username ที่ผู้ใช้กรอก ไม่ใช่ EmployeeCode
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("AdAuthService.LoginAsync error: " + ex.Message);
                return new AdAuthResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        /// <inheritdoc/>
        public async Task<AdUserLoginInfo> GetUserInfoAsync(string accessToken)
        {
            try
            {
                string json = Serialize(new ValidateRequestDto { AccessToken = accessToken });

                using (var content  = new StringContent(json, Encoding.UTF8, "application/json"))
                using (var response = await _http.PostAsync(_baseUrl + _validatePath, content).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode) return null;

                    string body         = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var dto             = Deserialize<ValidateResponseDto>(body);

                    if (dto == null || dto.UserInfo == null) return null;

                    return new AdUserLoginInfo
                    {
                        UserId   = dto.UserInfo.UserId,
                        UserName = dto.UserInfo.UserName
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("AdAuthService.GetUserInfoAsync error: " + ex.Message);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<AdAuthResult> ValidateTokenAsync(string accessToken)
        {
            try
            {
                string json = Serialize(new ValidateRequestDto { AccessToken = accessToken });

                using (var content  = new StringContent(json, Encoding.UTF8, "application/json"))
                using (var response = await _http.PostAsync(_baseUrl + _validatePath, content).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                        return new AdAuthResult { Success = false, ErrorMessage = "Token ไม่ valid (HTTP " + (int)response.StatusCode + ")" };

                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var dto     = Deserialize<ValidateResponseDto>(body);

                    if (dto == null)
                        return new AdAuthResult { Success = false, ErrorMessage = "ไม่สามารถอ่าน response ได้" };

                    // Map EmployeeInfo จาก validate response
                    EmployeeInfo empInfo = null;
                    if (dto.EmployeeInfo != null)
                    {
                        empInfo = new EmployeeInfo
                        {
                            EmployeeCode        = dto.EmployeeInfo.EmployeeCode,
                            EmployeeTitleNameEn = dto.EmployeeInfo.EmployeeTitleNameEn,
                            EmployeeFirstNameEn = dto.EmployeeInfo.EmployeeFirstNameEn,
                            EmployeeLastNameEn  = dto.EmployeeInfo.EmployeeLastNameEn,
                            EmployeeTitleNameTh = dto.EmployeeInfo.EmployeeTitleNameTh,
                            EmployeeFirstNameTh = dto.EmployeeInfo.EmployeeFirstNameTh,
                            EmployeeLastNameTh  = dto.EmployeeInfo.EmployeeLastNameTh,
                            Position            = dto.EmployeeInfo.Position,
                            DivisionCode        = dto.EmployeeInfo.DivisionCode,
                            DivisionNameEn      = dto.EmployeeInfo.DivisionNameEn,
                            DivisionNameTh      = dto.EmployeeInfo.DivisionNameTh,
                            DepartmentCode      = dto.EmployeeInfo.DepartmentCode,
                            DepartmentNameEn    = dto.EmployeeInfo.DepartmentNameEn,
                            DepartmentNameTh    = dto.EmployeeInfo.DepartmentNameTh
                        };
                    }

                    // AdUserName มาจาก userInfo.UserName (AD domain\username)
                    string adUserName = dto.UserInfo != null ? dto.UserInfo.UserName : null;

                    return new AdAuthResult
                    {
                        Success      = true,
                        AccessToken  = accessToken,
                        AdUserName   = adUserName,
                        EmployeeInfo = empInfo
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("AdAuthService.ValidateTokenAsync error: " + ex.Message);
                return new AdAuthResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        private static string Serialize<T>(T obj)
        {
            var ser = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                ser.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private static T Deserialize<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            var ser = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                return (T)ser.ReadObject(ms);
        }

        private static DateTime? ParseDate(string raw)
        {
            DateTime dt;
            return DateTime.TryParse(raw, out dt) ? dt : (DateTime?)null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Internal DTOs (request payloads เท่านั้น — response ใช้ public models)
        // ─────────────────────────────────────────────────────────────────────

        [DataContract]
        private class LoginRequestDto
        {
            [DataMember(Name = "username")] public string UserName { get; set; }
            [DataMember(Name = "password")] public string Password { get; set; }
        }

        [DataContract]
        private class ValidateRequestDto
        {
            [DataMember(Name = "accessToken")] public string AccessToken { get; set; }
        }

        [DataContract]
        private class ValidateResponseDto
        {
            [DataMember(Name = "userInfo")]    public UserInfoDto    UserInfo     { get; set; }
            [DataMember(Name = "employee_Info")] public EmployeeInfoValidateDto EmployeeInfo { get; set; }
        }

        [DataContract]
        private class UserInfoDto
        {
            [DataMember(Name = "userId")]   public string UserId   { get; set; }
            [DataMember(Name = "userName")] public string UserName { get; set; }
        }

        [DataContract]
        private class EmployeeInfoValidateDto
        {
            [DataMember(Name = "employee_Code")]           public string EmployeeCode       { get; set; }
            [DataMember(Name = "employee_Title_Name_En")]  public string EmployeeTitleNameEn { get; set; }
            [DataMember(Name = "employee_First_Name_En")]  public string EmployeeFirstNameEn { get; set; }
            [DataMember(Name = "employee_Last_Name_En")]   public string EmployeeLastNameEn  { get; set; }
            [DataMember(Name = "employee_Title_Name_Th")]  public string EmployeeTitleNameTh { get; set; }
            [DataMember(Name = "employee_First_Name_Th")]  public string EmployeeFirstNameTh { get; set; }
            [DataMember(Name = "employee_Last_Name_Th")]   public string EmployeeLastNameTh  { get; set; }
            [DataMember(Name = "position")]                public string Position            { get; set; }
            [DataMember(Name = "division_Code")]           public string DivisionCode        { get; set; }
            [DataMember(Name = "division_Name_En")]        public string DivisionNameEn      { get; set; }
            [DataMember(Name = "division_Name_Th")]        public string DivisionNameTh      { get; set; }
            [DataMember(Name = "department_Code")]         public string DepartmentCode      { get; set; }
            [DataMember(Name = "department_Name_En")]      public string DepartmentNameEn    { get; set; }
            [DataMember(Name = "department_Name_Th")]      public string DepartmentNameTh    { get; set; }
        }
    }
}
