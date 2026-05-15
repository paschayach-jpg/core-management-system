using System;
using System.Threading.Tasks;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class MenuController : IMenuController
    {
        private readonly IMenuApiService _menuApiService;

        public MenuController(IMenuApiService menuApiService)
        {
            _menuApiService = menuApiService ?? throw new ArgumentNullException("menuApiService");
        }

        public Task<UserMenuModel> GetUserMenuAsync(string accessToken)
        {
            return _menuApiService.GetUserMenuAsync(accessToken);
        }
    }
}
