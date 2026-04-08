using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Service for menu management with business logic and authorization enforcement.
    /// </summary>
    public class MenuService : IMenuService
    {
        private readonly IMenuItemRepository _menuRepository;
        private readonly ILoggingService _logger;
        private readonly IAuditService _auditService;

        public MenuService(IMenuItemRepository menuRepository, ILoggingService logger, IAuditService auditService)
        {
            _menuRepository = menuRepository;
            _logger = logger;
            _auditService = auditService;
        }

        public List<MenuItem> GetVisibleMenus(UserContext currentUser)
        {
            if (currentUser == null)
                throw new ArgumentNullException("currentUser");

            try
            {
                return _menuRepository.GetMenuItemsByRole(currentUser.AppRole);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting visible menus for user {currentUser.UserID}: {ex.Message}", ex);
                return new List<MenuItem>();
            }
        }

        public List<MenuItem> GetRootMenus(UserContext currentUser)
        {
            if (currentUser == null)
                throw new ArgumentNullException("currentUser");

            try
            {
                var allMenus = _menuRepository.GetRootMenuItems();
                // Filter by role
                return allMenus
                    .Where(m => m.IsActive && (m.RequiredAppRole == null || m.RequiredAppRole <= currentUser.AppRole))
                    .OrderBy(m => m.DisplayOrder)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting root menus: {ex.Message}", ex);
                return new List<MenuItem>();
            }
        }

        public List<MenuItem> GetChildMenus(int parentMenuItemID, UserContext currentUser)
        {
            if (currentUser == null)
                throw new ArgumentNullException("currentUser");

            try
            {
                var childMenus = _menuRepository.GetChildMenuItems(parentMenuItemID);
                // Filter by role and active status
                return childMenus
                    .Where(m => m.IsActive && (m.RequiredAppRole == null || m.RequiredAppRole <= currentUser.AppRole))
                    .OrderBy(m => m.DisplayOrder)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting child menus for parent {parentMenuItemID}: {ex.Message}", ex);
                return new List<MenuItem>();
            }
        }

        public MenuItem GetMenuByKey(string menuKey, UserContext currentUser)
        {
            if (string.IsNullOrWhiteSpace(menuKey))
                throw new ArgumentNullException("menuKey");
            if (currentUser == null)
                throw new ArgumentNullException("currentUser");

            try
            {
                var menu = _menuRepository.GetMenuItemByKey(menuKey);
                
                // Check if user has access
                if (menu == null || (!menu.IsActive || (menu.RequiredAppRole != null && menu.RequiredAppRole > currentUser.AppRole)))
                {
                    return null;
                }

                return menu;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting menu by key {menuKey}: {ex.Message}", ex);
                return null;
            }
        }

        public int CreateMenu(MenuItem menuItem, UserContext admin)
        {
            if (menuItem == null)
                throw new ArgumentNullException("menuItem");
            if (admin == null)
                throw new ArgumentNullException("admin");

            // Authorization check
            if (admin.AppRole != 4) // 4 = Admin
                throw new UnauthorizedAccessException("Only Admin users can create menus.");

            // Validation
            if (string.IsNullOrWhiteSpace(menuItem.MenuName))
                throw new ArgumentException("MenuName is required.");
            if (string.IsNullOrWhiteSpace(menuItem.MenuKey))
                throw new ArgumentException("MenuKey is required.");
            if (!IsMenuKeyUnique(menuItem.MenuKey))
                throw new ArgumentException($"MenuKey '{menuItem.MenuKey}' already exists.");

            try
            {
                menuItem.CreatedByUserID = admin.UserID;
                menuItem.CreatedDate = DateTime.Now;
                
                int newMenuID = _menuRepository.CreateMenuItem(menuItem);

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Module = "MenuManagement",
                    Action = "CREATE_MENU",
                    UserID = admin.UserID,
                    UserRole = admin.AppRole,
                    TableName = "MenuItem",
                    RecordID = newMenuID,
                    NewValue = $"Created menu: {menuItem.MenuName}",
                    Result = 1
                });

                _logger.Info($"Menu created by {admin.AdIdentity}: {menuItem.MenuName} (ID: {newMenuID})");
                return newMenuID;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating menu: {ex.Message}", ex);
                throw;
            }
        }

        public void UpdateMenu(MenuItem menuItem, UserContext admin)
        {
            if (menuItem == null)
                throw new ArgumentNullException("menuItem");
            if (admin == null)
                throw new ArgumentNullException("admin");

            // Authorization check
            if (admin.AppRole != 4) // 4 = Admin
                throw new UnauthorizedAccessException("Only Admin users can update menus.");

            // Validation
            if (string.IsNullOrWhiteSpace(menuItem.MenuName))
                throw new ArgumentException("MenuName is required.");
            if (string.IsNullOrWhiteSpace(menuItem.MenuKey))
                throw new ArgumentException("MenuKey is required.");
            if (!IsMenuKeyUnique(menuItem.MenuKey, menuItem.MenuItemID))
                throw new ArgumentException($"MenuKey '{menuItem.MenuKey}' already exists.");

            try
            {
                menuItem.ModifiedByUserID = admin.UserID;
                menuItem.ModifiedDate = DateTime.Now;

                _menuRepository.UpdateMenuItem(menuItem);

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Module = "MenuManagement",
                    Action = "UPDATE_MENU",
                    UserID = admin.UserID,
                    UserRole = admin.AppRole,
                    TableName = "MenuItem",
                    RecordID = menuItem.MenuItemID,
                    NewValue = $"Updated menu: {menuItem.MenuName}",
                    Result = 1
                });

                _logger.Info($"Menu updated by {admin.AdIdentity}: {menuItem.MenuName}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating menu: {ex.Message}", ex);
                throw;
            }
        }

        public void DeleteMenu(int menuItemID, UserContext admin)
        {
            if (admin == null)
                throw new ArgumentNullException("admin");

            // Authorization check
            if (admin.AppRole != 4) // 4 = Admin
                throw new UnauthorizedAccessException("Only Admin users can delete menus.");

            try
            {
                var menu = _menuRepository.GetMenuItemByID(menuItemID);
                if (menu == null)
                    throw new KeyNotFoundException($"Menu item {menuItemID} not found.");

                _menuRepository.DeleteMenuItem(menuItemID);

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Module = "MenuManagement",
                    Action = "DELETE_MENU",
                    UserID = admin.UserID,
                    UserRole = admin.AppRole,
                    TableName = "MenuItem",
                    RecordID = menuItemID,
                    NewValue = $"Deleted menu: {menu.MenuName}",
                    Result = 1
                });

                _logger.Info($"Menu deleted by {admin.AdIdentity}: {menu.MenuName}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting menu: {ex.Message}", ex);
                throw;
            }
        }

        public void ReorderMenus(List<MenuItemOrder> orders, UserContext admin)
        {
            if (orders == null || orders.Count == 0)
                throw new ArgumentException("Orders list cannot be empty.");
            if (admin == null)
                throw new ArgumentNullException("admin");

            // Authorization check
            if (admin.AppRole != 4) // 4 = Admin
                throw new UnauthorizedAccessException("Only Admin users can reorder menus.");

            try
            {
                foreach (var order in orders)
                {
                    _menuRepository.UpdateMenuItemOrder(order.MenuItemID, order.DisplayOrder);
                }

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Module = "MenuManagement",
                    Action = "REORDER_MENUS",
                    UserID = admin.UserID,
                    UserRole = admin.AppRole,
                    TableName = "MenuItem",
                    NewValue = $"Reordered {orders.Count} menu items",
                    Result = 1
                });

                _logger.Info($"Menus reordered by {admin.AdIdentity}: {orders.Count} items");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error reordering menus: {ex.Message}", ex);
                throw;
            }
        }

        public void SetMenuActive(int menuItemID, bool isActive, UserContext admin)
        {
            if (admin == null)
                throw new ArgumentNullException("admin");

            // Authorization check
            if (admin.AppRole != 4) // 4 = Admin
                throw new UnauthorizedAccessException("Only Admin users can change menu status.");

            try
            {
                var menu = _menuRepository.GetMenuItemByID(menuItemID);
                if (menu == null)
                    throw new KeyNotFoundException($"Menu item {menuItemID} not found.");

                menu.IsActive = isActive;
                menu.ModifiedByUserID = admin.UserID;
                menu.ModifiedDate = DateTime.Now;

                _menuRepository.UpdateMenuItem(menu);

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Module = "MenuManagement",
                    Action = isActive ? "ACTIVATE_MENU" : "DEACTIVATE_MENU",
                    UserID = admin.UserID,
                    UserRole = admin.AppRole,
                    TableName = "MenuItem",
                    RecordID = menuItemID,
                    NewValue = $"{(isActive ? "Activated" : "Deactivated")} menu: {menu.MenuName}",
                    Result = 1
                });

                _logger.Info($"Menu {(isActive ? "activated" : "deactivated")} by {admin.AdIdentity}: {menu.MenuName}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error changing menu status: {ex.Message}", ex);
                throw;
            }
        }

        public List<MenuItem> GetAllMenusForManagement(UserContext admin)
        {
            if (admin == null)
                throw new ArgumentNullException("admin");

            // Authorization check
            if (admin.AppRole != 4) // 4 = Admin
                throw new UnauthorizedAccessException("Only Admin users can access menu management.");

            try
            {
                return _menuRepository.GetMenuHierarchy();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting all menus: {ex.Message}", ex);
                return new List<MenuItem>();
            }
        }

        public bool IsMenuKeyUnique(string menuKey, int? excludeMenuItemID = null)
        {
            if (string.IsNullOrWhiteSpace(menuKey))
                return false;

            try
            {
                var existing = _menuRepository.GetMenuItemByKey(menuKey);
                if (existing == null)
                    return true;

                // If we're updating, it's okay if it's the same item
                if (excludeMenuItemID.HasValue && existing.MenuItemID == excludeMenuItemID.Value)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error checking menu key uniqueness: {ex.Message}", ex);
                return false;
            }
        }

        public MenuStatistics GetMenuStatistics()
        {
            try
            {
                var allMenus = _menuRepository.GetAllMenuItems();
                var stats = new MenuStatistics
                {
                    TotalMenuItems = allMenus.Count,
                    ActiveMenuItems = allMenus.Count(m => m.IsActive),
                    AdminOnlyMenus = allMenus.Count(m => m.RequiredAppRole == 4),
                    ManagerOnlyMenus = allMenus.Count(m => m.RequiredAppRole == 2 || m.RequiredAppRole == 3),
                    PublicMenus = allMenus.Count(m => m.RequiredAppRole == null || m.RequiredAppRole == 1),
                    LastModified = allMenus.Max(m => m.ModifiedDate ?? m.CreatedDate)
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting menu statistics: {ex.Message}", ex);
                return new MenuStatistics();
            }
        }
    }
}
