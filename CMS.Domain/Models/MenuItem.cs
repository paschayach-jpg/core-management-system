using System;
using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Represents a menu item in the system hierarchy.
    /// Supports nested menus with role-based visibility.
    /// Requirement: Dynamic menu management with database persistence.
    /// </summary>
    public class MenuItem
    {
        /// <summary>Unique menu item identifier</summary>
        public int MenuItemID { get; set; }

        /// <summary>Parent menu item ID (null for root level menus)</summary>
        public int? ParentMenuItemID { get; set; }

        /// <summary>Display name of the menu (appears in tree)</summary>
        public string MenuName { get; set; }

        /// <summary>MenuKey for identifying menu programmatically</summary>
        public string MenuKey { get; set; }

        /// <summary>Full class name to instantiate (e.g., "CMS.Presentation.Modules.ImportFile.AuditLogForm")</summary>
        public string FormClassName { get; set; }

        /// <summary>Icon name/resource key for tree node display</summary>
        public string IconKey { get; set; }

        /// <summary>Display order of menu items (sorted ascending)</summary>
        public int DisplayOrder { get; set; }

        /// <summary>Indicates if this menu item is active/enabled</summary>
        public bool IsActive { get; set; }

        /// <summary>Required minimum AppRole to see this menu (1=User, 2=Manager, 3=Approver, 4=Admin)</summary>
        public int? RequiredAppRole { get; set; }

        /// <summary>Description/tooltip for the menu item</summary>
        public string Description { get; set; }

        /// <summary>Module code for audit logging (e.g., "AuditLog", "FileShare", "UserMgmt")</summary>
        public string ModuleCode { get; set; }

        /// <summary>URL for web-based menus or API endpoints</summary>
        public string Url { get; set; }

        /// <summary>Creation timestamp</summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>User ID who created the menu</summary>
        public int CreatedByUserID { get; set; }

        /// <summary>Last modification timestamp</summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>User ID who last modified the menu</summary>
        public int? ModifiedByUserID { get; set; }

        /// <summary>Navigation property for child menu items</summary>
        public virtual List<MenuItem> ChildMenuItems { get; set; } = new List<MenuItem>();

        /// <summary>Navigation property for parent menu item</summary>
        public virtual MenuItem ParentMenuItem { get; set; }
    }
}
