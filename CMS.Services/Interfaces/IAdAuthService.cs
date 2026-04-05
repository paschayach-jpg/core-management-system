using System.Threading.Tasks;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Service สำหรับ Authentication ผ่าน AD API
    /// </summary>
    public interface IAdAuthService
    {
        /// <summary>
        /// ตรวจสอบ username/password กับ AD API
        /// </summary>
        /// <returns>true ถ้า credentials ถูกต้อง</returns>
        Task<bool> CheckLoginAsync(string username, string password);

        /// <summary>
        /// Login และรับ Token พร้อมข้อมูลพนักงาน
        /// </summary>
        /// <returns><see cref="AdAuthResult"/> ที่มี Token และข้อมูล EmployeeCode</returns>
        Task<AdAuthResult> LoginAsync(string username, string password);

        /// <summary>
        /// Validate Token และดึงข้อมูล User (userId, userName)
        /// </summary>
        /// <param name="accessToken">Token ที่ได้จาก LoginAsync</param>
        /// <returns><see cref="AdUserLoginInfo"/> หรือ null ถ้า Token ไม่ valid</returns>
        Task<AdUserLoginInfo> GetUserInfoAsync(string accessToken);

        /// <summary>
        /// Validate Token แล้วคืน AdAuthResult พร้อม EmployeeInfo ครบ
        /// ใช้เมื่อรับ Token มาจากโปรแกรมภายนอกผ่าน command-line
        /// </summary>
        /// <param name="accessToken">Token ที่ส่งมาจากโปรแกรมอื่น</param>
        /// <returns><see cref="AdAuthResult"/> หรือ null ถ้า Token ไม่ valid</returns>
        Task<AdAuthResult> ValidateTokenAsync(string accessToken);
    }
}
