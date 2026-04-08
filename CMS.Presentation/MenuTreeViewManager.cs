using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
using MenuItem = CMS.Domain.Models.MenuItem;

namespace CMS.Presentation
{
    /// <summary>
    /// Manages the menu tree view (_treeMainMenu) - populates, refreshes, and handles selections.
    /// Provides methods for displaying menu hierarchy in a TreeView control.
    /// </summary>
    public class MenuTreeViewManager
    {
        private readonly TreeView _treeView;
        private readonly IMenuService _menuService;
        private readonly UserContext _currentUser;
        private Dictionary<string, Image> _iconCache = new Dictionary<string, Image>();

        // Event for menu selection
        public event EventHandler<MenuSelectedEventArgs> MenuSelected;

        public MenuTreeViewManager(TreeView treeView, IMenuService menuService, UserContext currentUser)
        {
            _treeView = treeView ?? throw new ArgumentNullException(nameof(treeView));
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            // Wire up tree view events
            _treeView.AfterSelect += TreeView_AfterSelect;
            _treeView.DoubleClick += TreeView_DoubleClick;
            _treeView.NodeMouseClick += TreeView_NodeMouseClick;
        }

        /// <summary>
        /// Populates the tree view with menu items for the current user.
        /// </summary>
        public void PopulateMenuTree()
        {
            try
            {
                _treeView.BeginUpdate();
                _treeView.Nodes.Clear();

                var rootMenus = _menuService.GetRootMenus(_currentUser);

                foreach (var menu in rootMenus)
                {
                    var rootNode = CreateTreeNode(menu);
                    _treeView.Nodes.Add(rootNode);
                    LoadChildNodes(rootNode, menu.MenuItemID);
                }

                _treeView.EndUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading menu: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Refreshes the menu tree from database.
        /// </summary>
        public void RefreshMenuTree()
        {
            PopulateMenuTree();
        }

        /// <summary>
        /// Expands all menu nodes in the tree.
        /// </summary>
        public void ExpandAllNodes()
        {
            _treeView.ExpandAll();
        }

        /// <summary>
        /// Collapses all menu nodes in the tree.
        /// </summary>
        public void CollapseAllNodes()
        {
            _treeView.CollapseAll();
        }

        /// <summary>
        /// Selects a specific menu by MenuKey.
        /// </summary>
        public bool SelectMenuByKey(string menuKey)
        {
            if (string.IsNullOrWhiteSpace(menuKey))
                return false;

            return FindAndSelectNode(_treeView.Nodes, menuKey);
        }

        /// <summary>
        /// Gets the currently selected menu item data.
        /// </summary>
        public MenuItem GetSelectedMenu()
        {
            if (_treeView.SelectedNode == null)
                return null;

            return _treeView.SelectedNode.Tag as MenuItem;
        }

        /// <summary>
        /// Gets all visible menu items.
        /// </summary>
        public List<MenuItem> GetAllVisibleMenus()
        {
            return _menuService.GetVisibleMenus(_currentUser);
        }

        // ────────────────────────────────────────────────────────────────────
        // Private Helper Methods
        // ────────────────────────────────────────────────────────────────────

        private TreeNode CreateTreeNode(MenuItem menu)
        {
            var node = new TreeNode(menu.MenuName)
            {
                Tag = menu,
                ToolTipText = menu.Description ?? menu.MenuName,
                ForeColor = menu.IsActive ? Color.Black : Color.Gray
            };

            // Set icon if available
            if (!string.IsNullOrEmpty(menu.IconKey))
            {
                try
                {
                    // Icon loading logic - implement based on your icon management
                    // node.ImageKey = menu.IconKey;
                    // node.SelectedImageKey = menu.IconKey;
                }
                catch { /* Icon not found - use default */ }
            }

            return node;
        }

        private void LoadChildNodes(TreeNode parentNode, int parentMenuItemID)
        {
            var childMenus = _menuService.GetChildMenus(parentMenuItemID, _currentUser);

            foreach (var childMenu in childMenus)
            {
                var childNode = CreateTreeNode(childMenu);
                parentNode.Nodes.Add(childNode);

                // Recursively load grandchildren
                if (childMenu.ChildMenuItems.Count > 0)
                {
                    LoadChildNodes(childNode, childMenu.MenuItemID);
                }
            }
        }

        private bool FindAndSelectNode(TreeNodeCollection nodes, string menuKey)
        {
            foreach (TreeNode node in nodes)
            {
                var menu = node.Tag as MenuItem;
                if (menu != null && menu.MenuKey == menuKey)
                {
                    _treeView.SelectedNode = node;
                    _treeView.SelectedNode?.EnsureVisible();
                    return true;
                }

                // Search in child nodes
                if (FindAndSelectNode(node.Nodes, menuKey))
                    return true;
            }

            return false;
        }

        // ────────────────────────────────────────────────────────────────────
        // Event Handlers
        // ────────────────────────────────────────────────────────────────────

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var menu = e.Node?.Tag as MenuItem;
            if (menu != null)
            {
                MenuSelected?.Invoke(this, new MenuSelectedEventArgs { SelectedMenu = menu });
            }
        }

        private void TreeView_DoubleClick(object sender, EventArgs e)
        {
            var menu = GetSelectedMenu();
            if (menu != null && !string.IsNullOrEmpty(menu.FormClassName))
            {
                // Trigger form opening
                MenuSelected?.Invoke(this, new MenuSelectedEventArgs 
                { 
                    SelectedMenu = menu, 
                    IsDoubleClick = true 
                });
            }
        }

        private void TreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _treeView.SelectedNode = e.Node;
                // Show context menu if needed
            }
        }

        /// <summary>
        /// Registers an icon for use in the tree view.
        /// </summary>
        public void RegisterIcon(string iconKey, Image iconImage)
        {
            if (_iconCache.ContainsKey(iconKey))
                _iconCache[iconKey] = iconImage;
            else
                _iconCache.Add(iconKey, iconImage);
        }

        /// <summary>
        /// Gets a registered icon.
        /// </summary>
        public Image GetIcon(string iconKey)
        {
            return _iconCache.ContainsKey(iconKey) ? _iconCache[iconKey] : null;
        }
    }

    /// <summary>
    /// Event arguments for menu selection events.
    /// </summary>
    public class MenuSelectedEventArgs : EventArgs
    {
        public MenuItem SelectedMenu { get; set; }
        public bool IsDoubleClick { get; set; }
    }
}
