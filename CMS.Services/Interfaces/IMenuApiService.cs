using System.Threading.Tasks;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IMenuApiService
    {
        /// <summary>
        /// เรียก Menu API ด้วย access token แล้วคืน UserMenuModel พร้อม MenuList ทั้งหมด
        /// </summary>
        Task<UserMenuModel> GetUserMenuAsync(string accessToken);
    }
}
