using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    /// <summary>
    /// Repository interface for menu management - CRUD operations.
    /// </summary>
    public interface IMenuItemRepository
    {
        /// <summary>
        /// Gets all menu items including child items, ordered by display order.
        /// </summary>
        List<MenuItem> GetAllMenuItems();

        /// <summary>
        /// Gets root level menu items (ParentMenuItemID = null).
        /// </summary>
        List<MenuItem> GetRootMenuItems();

        /// <summary>
        /// Gets immediate child menu items for a parent.
        /// </summary>
        List<MenuItem> GetChildMenuItems(int parentMenuItemID);

        /// <summary>
        /// Gets a single menu item by ID with child items populated.
        /// </summary>
        MenuItem GetMenuItemByID(int menuItemID);

        /// <summary>
        /// Gets a menu item by its key identifier.
        /// </summary>
        MenuItem GetMenuItemByKey(string menuKey);

        /// <summary>
        /// Gets all menu items visible to a specific user role.
        /// </summary>
        List<MenuItem> GetMenuItemsByRole(int userAppRole);

        /// <summary>
        /// Creates a new menu item.
        /// </summary>
        int CreateMenuItem(MenuItem menuItem);

        /// <summary>
        /// Updates an existing menu item.
        /// </summary>
        void UpdateMenuItem(MenuItem menuItem);

        /// <summary>
        /// Deletes a menu item and its child items.
        /// </summary>
        void DeleteMenuItem(int menuItemID);

        /// <summary>
        /// Updates display order of menu items.
        /// </summary>
        void UpdateMenuItemOrder(int menuItemID, int newDisplayOrder);

        /// <summary>
        /// Gets full hierarchy of menus as a tree structure.
        /// </summary>
        List<MenuItem> GetMenuHierarchy();

        /// <summary>
        /// Checks if a menu key exists.
        /// </summary>
        bool MenuKeyExists(string menuKey);
    }
}
