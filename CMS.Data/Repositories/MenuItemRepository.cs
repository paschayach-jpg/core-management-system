using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    /// <summary>
    /// Repository for menu item management.
    /// Handles all CRUD operations with database persistence.
    /// </summary>
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly string _connectionString;

        public MenuItemRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public List<MenuItem> GetAllMenuItems()
        {
            var menuItems = new List<MenuItem>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT * FROM EMS.MenuItem ORDER BY DisplayOrder ASC", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            menuItems.Add(MapMenuItem(reader));
                        }
                    }
                }
            }

            // Build parent-child relationships
            BuildHierarchy(menuItems);
            return menuItems;
        }

        public List<MenuItem> GetRootMenuItems()
        {
            var menuItems = new List<MenuItem>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT * FROM EMS.MenuItem WHERE ParentMenuItemID IS NULL ORDER BY DisplayOrder ASC", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            menuItems.Add(MapMenuItem(reader));
                        }
                    }
                }
            }

            // Load children for each root
            foreach (var root in menuItems)
            {
                root.ChildMenuItems = GetChildMenuItems(root.MenuItemID);
            }

            return menuItems;
        }

        public List<MenuItem> GetChildMenuItems(int parentMenuItemID)
        {
            var menuItems = new List<MenuItem>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT * FROM EMS.MenuItem WHERE ParentMenuItemID = @ParentID ORDER BY DisplayOrder ASC", conn))
                {
                    cmd.Parameters.AddWithValue("@ParentID", parentMenuItemID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            menuItems.Add(MapMenuItem(reader));
                        }
                    }
                }
            }

            // Recursively load children
            foreach (var item in menuItems)
            {
                item.ChildMenuItems = GetChildMenuItems(item.MenuItemID);
            }

            return menuItems;
        }

        public MenuItem GetMenuItemByID(int menuItemID)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT * FROM EMS.MenuItem WHERE MenuItemID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", menuItemID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var menuItem = MapMenuItem(reader);
                            // Load child items
                            menuItem.ChildMenuItems = GetChildMenuItems(menuItemID);
                            return menuItem;
                        }
                    }
                }
            }
            return null;
        }

        public MenuItem GetMenuItemByKey(string menuKey)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT * FROM EMS.MenuItem WHERE MenuKey = @Key", conn))
                {
                    cmd.Parameters.AddWithValue("@Key", menuKey);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapMenuItem(reader);
                        }
                    }
                }
            }
            return null;
        }

        public List<MenuItem> GetMenuItemsByRole(int userAppRole)
        {
            var menuItems = new List<MenuItem>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT * FROM EMS.MenuItem 
                    WHERE IsActive = 1 
                    AND (RequiredAppRole IS NULL OR RequiredAppRole <= @Role)
                    ORDER BY DisplayOrder ASC", conn))
                {
                    cmd.Parameters.AddWithValue("@Role", userAppRole);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            menuItems.Add(MapMenuItem(reader));
                        }
                    }
                }
            }

            // Build hierarchy with role filtering
            var rootItems = menuItems.Where(m => m.ParentMenuItemID == null).ToList();
            foreach (var root in rootItems)
            {
                root.ChildMenuItems = GetFilteredChildren(root.MenuItemID, menuItems);
            }

            return rootItems;
        }

        public int CreateMenuItem(MenuItem menuItem)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EMS.sp_CreateMenuItem", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ParentMenuItemID", (object)menuItem.ParentMenuItemID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MenuName", menuItem.MenuName);
                    cmd.Parameters.AddWithValue("@MenuKey", menuItem.MenuKey);
                    cmd.Parameters.AddWithValue("@FormClassName", (object)menuItem.FormClassName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IconKey", (object)menuItem.IconKey ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DisplayOrder", menuItem.DisplayOrder);
                    cmd.Parameters.AddWithValue("@IsActive", menuItem.IsActive);
                    cmd.Parameters.AddWithValue("@RequiredAppRole", (object)menuItem.RequiredAppRole ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", (object)menuItem.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ModuleCode", (object)menuItem.ModuleCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Url", (object)menuItem.Url ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedByUserID", menuItem.CreatedByUserID);

                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        public void UpdateMenuItem(MenuItem menuItem)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EMS.sp_UpdateMenuItem", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MenuItemID", menuItem.MenuItemID);
                    cmd.Parameters.AddWithValue("@ParentMenuItemID", (object)menuItem.ParentMenuItemID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MenuName", menuItem.MenuName);
                    cmd.Parameters.AddWithValue("@MenuKey", menuItem.MenuKey);
                    cmd.Parameters.AddWithValue("@FormClassName", (object)menuItem.FormClassName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IconKey", (object)menuItem.IconKey ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DisplayOrder", menuItem.DisplayOrder);
                    cmd.Parameters.AddWithValue("@IsActive", menuItem.IsActive);
                    cmd.Parameters.AddWithValue("@RequiredAppRole", (object)menuItem.RequiredAppRole ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", (object)menuItem.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ModuleCode", (object)menuItem.ModuleCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Url", (object)menuItem.Url ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ModifiedByUserID", menuItem.ModifiedByUserID);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteMenuItem(int menuItemID)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EMS.sp_DeleteMenuItem", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MenuItemID", menuItemID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateMenuItemOrder(int menuItemID, int newDisplayOrder)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE EMS.MenuItem SET DisplayOrder = @Order WHERE MenuItemID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@Order", newDisplayOrder);
                    cmd.Parameters.AddWithValue("@ID", menuItemID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<MenuItem> GetMenuHierarchy()
        {
            return GetRootMenuItems();
        }

        public bool MenuKeyExists(string menuKey)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM EMS.MenuItem WHERE MenuKey = @Key", conn))
                {
                    cmd.Parameters.AddWithValue("@Key", menuKey);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        // ───────────────────────────────────────────────────────────────────
        // Helper Methods
        // ───────────────────────────────────────────────────────────────────

        private MenuItem MapMenuItem(SqlDataReader reader)
        {
            return new MenuItem
            {
                MenuItemID = Convert.ToInt32(reader["MenuItemID"]),
                ParentMenuItemID = reader["ParentMenuItemID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ParentMenuItemID"]),
                MenuName = reader["MenuName"].ToString(),
                MenuKey = reader["MenuKey"].ToString(),
                FormClassName = reader["FormClassName"] == DBNull.Value ? null : reader["FormClassName"].ToString(),
                IconKey = reader["IconKey"] == DBNull.Value ? null : reader["IconKey"].ToString(),
                DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                RequiredAppRole = reader["RequiredAppRole"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["RequiredAppRole"]),
                Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                ModuleCode = reader["ModuleCode"] == DBNull.Value ? null : reader["ModuleCode"].ToString(),
                Url = reader["Url"] == DBNull.Value ? null : reader["Url"].ToString(),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                CreatedByUserID = Convert.ToInt32(reader["CreatedByUserID"]),
                ModifiedDate = reader["ModifiedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ModifiedDate"]),
                ModifiedByUserID = reader["ModifiedByUserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ModifiedByUserID"])
            };
        }

        private void BuildHierarchy(List<MenuItem> allItems)
        {
            var itemsById = allItems.ToDictionary(m => m.MenuItemID);
            foreach (var item in allItems)
            {
                if (item.ParentMenuItemID.HasValue && itemsById.ContainsKey(item.ParentMenuItemID.Value))
                {
                    var parent = itemsById[item.ParentMenuItemID.Value];
                    parent.ChildMenuItems.Add(item);
                    item.ParentMenuItem = parent;
                }
            }
        }

        private List<MenuItem> GetFilteredChildren(int parentID, List<MenuItem> allMenuItems)
        {
            return allMenuItems
                .Where(m => m.ParentMenuItemID == parentID && m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToList();
        }
    }
}

