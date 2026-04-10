using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IMenuManagementController
    {
        IList<MenuItem> GetVisibleMenus(UserContext currentUser);
        IList<MenuItem> GetRootMenus(UserContext currentUser);
        IList<MenuItem> GetChildMenus(int parentMenuItemId, UserContext currentUser);
        IList<MenuItem> GetAllMenusForManagement(UserContext admin);
        int CreateMenu(MenuItem menuItem, UserContext admin);
        void UpdateMenu(MenuItem menuItem, UserContext admin);
        void DeleteMenu(int menuItemId, UserContext admin);
        MenuStatistics GetMenuStatistics();
    }
}
