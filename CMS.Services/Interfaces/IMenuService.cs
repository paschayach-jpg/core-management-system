using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Service interface for menu management business logic.
    /// Provides high-level operations for menu handling with validation and authorization.
    /// </summary>
    public interface IMenuService
    {
        /// <summary>
        /// Gets the complete menu hierarchy visible to the current user.
        /// </summary>
        List<MenuItem> GetVisibleMenus(UserContext currentUser);

        /// <summary>
        /// Gets root level menus visible to the current user.
        /// </summary>
        List<MenuItem> GetRootMenus(UserContext currentUser);

        /// <summary>
        /// Gets child menus of a specific parent menu.
        /// </summary>
        List<MenuItem> GetChildMenus(int parentMenuItemID, UserContext currentUser);

        /// <summary>
        /// Gets a menu item by its key with validation.
        /// </summary>
        MenuItem GetMenuByKey(string menuKey, UserContext currentUser);

        /// <summary>
        /// Creates a new menu item (Admin only).
        /// </summary>
        int CreateMenu(MenuItem menuItem, UserContext admin);

        /// <summary>
        /// Updates a menu item (Admin only).
        /// </summary>
        void UpdateMenu(MenuItem menuItem, UserContext admin);

        /// <summary>
        /// Deletes a menu item and all child items (Admin only).
        /// </summary>
        void DeleteMenu(int menuItemID, UserContext admin);

        /// <summary>
        /// Changes the display order of menu items.
        /// </summary>
        void ReorderMenus(List<MenuItemOrder> orders, UserContext admin);

        /// <summary>
        /// Activates or deactivates a menu item.
        /// </summary>
        void SetMenuActive(int menuItemID, bool isActive, UserContext admin);

        /// <summary>
        /// Gets all menus for admin management (unfiltered).
        /// </summary>
        List<MenuItem> GetAllMenusForManagement(UserContext admin);

        /// <summary>
        /// Validates menu key uniqueness.
        /// </summary>
        bool IsMenuKeyUnique(string menuKey, int? excludeMenuItemID = null);

        /// <summary>
        /// Gets menu statistics for dashboard.
        /// </summary>
        MenuStatistics GetMenuStatistics();
    }

    /// <summary>
    /// Represents a menu item reordering operation.
    /// </summary>
    public class MenuItemOrder
    {
        public int MenuItemID { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Statistics about the menu system.
    /// </summary>
    public class MenuStatistics
    {
        public int TotalMenuItems { get; set; }
        public int ActiveMenuItems { get; set; }
        public int AdminOnlyMenus { get; set; }
        public int ManagerOnlyMenus { get; set; }
        public int PublicMenus { get; set; }
        public DateTime LastModified { get; set; }
    }
}
