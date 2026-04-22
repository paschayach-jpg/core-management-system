using System.Collections.Generic;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class MenuManagementController : IMenuManagementController
    {
        private readonly IMenuService _menuService;

        public MenuManagementController(IMenuService menuService = null)
        {
            _menuService = menuService;
        }

        public IList<MenuItem> GetVisibleMenus(UserContext currentUser)
        {
            if (_menuService == null)
                return new List<MenuItem>();

            return _menuService.GetVisibleMenus(currentUser);
        }

        public IList<MenuItem> GetRootMenus(UserContext currentUser)
        {
            if (_menuService == null)
                return new List<MenuItem>();

            return _menuService.GetRootMenus(currentUser);
        }

        public IList<MenuItem> GetChildMenus(int parentMenuItemId, UserContext currentUser)
        {
            if (_menuService == null)
                return new List<MenuItem>();

            return _menuService.GetChildMenus(parentMenuItemId, currentUser);
        }

        public IList<MenuItem> GetAllMenusForManagement(UserContext admin)
        {
            if (_menuService == null)
                return new List<MenuItem>();

            return _menuService.GetAllMenusForManagement(admin);
        }

        public int CreateMenu(MenuItem menuItem, UserContext admin)
        {
            if (_menuService == null || menuItem == null)
                return 0;

            return _menuService.CreateMenu(menuItem, admin);
        }

        public void UpdateMenu(MenuItem menuItem, UserContext admin)
        {
            if (_menuService == null || menuItem == null)
                return;

            _menuService.UpdateMenu(menuItem, admin);
        }

        public void DeleteMenu(int menuItemId, UserContext admin)
        {
            if (_menuService == null)
                return;

            _menuService.DeleteMenu(menuItemId, admin);
        }

        public MenuStatistics GetMenuStatistics()
        {
            if (_menuService == null)
                return new MenuStatistics();

            return _menuService.GetMenuStatistics();
        }
    }
}
