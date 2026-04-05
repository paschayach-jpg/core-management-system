using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// ผลลัพธ์จากการ Login ผ่าน AD API
    /// </summary>
    public class AdAuthResult
    {
        /// <summary>Login สำเร็จหรือไม่</summary>
        public bool Success { get; set; }

        /// <summary>Access Token สำหรับเรียก API ต่อ</summary>
        public string AccessToken { get; set; }

        /// <summary>Refresh Token สำหรับต่ออายุ session</summary>
        public string RefreshToken { get; set; }

        /// <summary>วันหมดอายุของ Refresh Token</summary>
        public DateTime? RefreshTokenExpired { get; set; }

        /// <summary>AD Username (domain\username) — ได้จาก validate หรือจาก LoginAsync</summary>
        public string AdUserName { get; set; }

        /// <summary>ข้อมูลพนักงานเต็มจาก AD API</summary>
        public EmployeeInfo EmployeeInfo { get; set; }

        /// <summary>ข้อความ error (ถ้า Success = false)</summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// ข้อมูล User ที่ดึงจาก token validation
    /// </summary>
    public class AdUserLoginInfo
    {
        public string UserId   { get; set; }
        public string UserName { get; set; }
    }
}
