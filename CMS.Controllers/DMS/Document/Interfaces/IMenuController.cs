using System.Threading.Tasks;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IMenuController
    {
        /// <summary>
        /// ดึง UserMenuModel จาก Menu API ด้วย access token
        /// </summary>
        Task<UserMenuModel> GetUserMenuAsync(string accessToken);
    }
}
