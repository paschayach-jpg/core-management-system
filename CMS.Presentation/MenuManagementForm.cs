using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
using MenuItem = CMS.Domain.Models.MenuItem;

namespace CMS.Presentation
{
    public partial class MenuManagementForm : Form
    {
        private readonly IMenuService _menuService;
        private readonly UserContext _currentUser;
        private System.Collections.Generic.List<MenuItem> _allMenus = new System.Collections.Generic.List<MenuItem>();

        public MenuManagementForm(IMenuService menuService, UserContext currentUser)
        {
            if (menuService == null) throw new ArgumentNullException(nameof(menuService));
            if (currentUser == null) throw new ArgumentNullException(nameof(currentUser));

            InitializeComponent();

            _menuService = menuService;
            _currentUser = currentUser;

            if (currentUser.AppRole != 4)
                throw new UnauthorizedAccessException("Only Admin users can access Menu Management.");

            this.Icon = SystemIcons.Information;
        }

        private void MenuManagementForm_Load(object sender, EventArgs e)
        {
            try
            {
                LoadMenus();
                RefreshMenuStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading menu management: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void LoadMenus()
        {
            try
            {
                _allMenus = _menuService.GetAllMenusForManagement(_currentUser);
                RefreshMenuTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading menus: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshMenuTree()
        {
            treeMenu.Nodes.Clear();

            var rootMenus = _allMenus.Where(m => m.ParentMenuItemID == null).OrderBy(m => m.DisplayOrder);
            foreach (var menu in rootMenus)
            {
                var node = CreateTreeNode(menu);
                treeMenu.Nodes.Add(node);
                LoadChildNodes(node, menu.MenuItemID);
            }
        }

        private TreeNode CreateTreeNode(MenuItem menu)
        {
            string icon = menu.IsActive ? "✓" : "✗";
            string text = $"{icon} {menu.MenuName}";
            return new TreeNode(text) { Tag = menu };
        }

        private void LoadChildNodes(TreeNode parentNode, int parentMenuItemID)
        {
            var children = _allMenus.Where(m => m.ParentMenuItemID == parentMenuItemID).OrderBy(m => m.DisplayOrder);
            foreach (var child in children)
            {
                var node = CreateTreeNode(child);
                parentNode.Nodes.Add(node);
                LoadChildNodes(node, child.MenuItemID);
            }
        }

        private void TreeMenu_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is MenuItem menu)
            {
                DisplayMenuDetails(menu);
            }
        }

        private void DisplayMenuDetails(MenuItem menu)
        {
            txtName.Text = menu.MenuName;
            txtKey.Text = menu.MenuKey;
            txtFormClass.Text = menu.FormClassName ?? string.Empty;
            numOrder.Value = menu.DisplayOrder;

            int roleIndex = menu.RequiredAppRole ?? 0;
            if (roleIndex < 0 || roleIndex >= cmbRole.Items.Count)
                roleIndex = 0;
            cmbRole.SelectedIndex = roleIndex;

            chkActive.Checked = menu.IsActive;
            lblID.Text = menu.MenuItemID.ToString();
        }

        private void AddNewMenu()
        {
            ClearForm();
            lblID.Text = "(New)";
            txtName.Focus();
        }

        private void EditMenu()
        {
            if (!(treeMenu.SelectedNode?.Tag is MenuItem menu))
            {
                MessageBox.Show("Please select a menu item to edit.");
                return;
            }

            DisplayMenuDetails(menu);
        }

        private void DeleteMenu()
        {
            if (!(treeMenu.SelectedNode?.Tag is MenuItem menu))
            {
                MessageBox.Show("Please select a menu item to delete.");
                return;
            }

            if (MessageBox.Show($"Delete menu '{menu.MenuName}' and all child menus?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                _menuService.DeleteMenu(menu.MenuItemID, _currentUser);
                MessageBox.Show("Menu deleted successfully.");
                LoadMenus();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting menu: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveMenu()
        {
            try
            {
                string idText = lblID.Text;
                string name = txtName.Text != null ? txtName.Text.Trim() : string.Empty;
                string key = txtKey.Text != null ? txtKey.Text.Trim() : string.Empty;
                string formClass = txtFormClass.Text != null ? txtFormClass.Text.Trim() : string.Empty;
                int order = (int)numOrder.Value;
                int? role = cmbRole.SelectedIndex > 0 ? cmbRole.SelectedIndex : (int?)null;
                bool isActive = chkActive.Checked;

                // Validation
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(key))
                {
                    MessageBox.Show("Name and Key are required.");
                    return;
                }

                if (idText == "(New)")
                {
                    // Create new
                    var newMenu = new MenuItem
                    {
                        MenuName = name,
                        MenuKey = key,
                        FormClassName = formClass,
                        DisplayOrder = order,
                        RequiredAppRole = role,
                        IsActive = isActive,
                        CreatedByUserID = _currentUser.UserID
                    };

                    int newID = _menuService.CreateMenu(newMenu, _currentUser);
                    MessageBox.Show($"Menu created successfully (ID: {newID})");
                }
                else
                {
                    // Edit existing
                    int id = int.Parse(idText);
                    var existingMenu = _allMenus.FirstOrDefault(m => m.MenuItemID == id);
                    if (existingMenu != null)
                    {
                        existingMenu.MenuName = name;
                        existingMenu.MenuKey = key;
                        existingMenu.FormClassName = formClass;
                        existingMenu.DisplayOrder = order;
                        existingMenu.RequiredAppRole = role;
                        existingMenu.IsActive = isActive;

                        _menuService.UpdateMenu(existingMenu, _currentUser);
                        MessageBox.Show("Menu updated successfully.");
                    }
                }

                LoadMenus();
                ClearForm();
                RefreshMenuStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving menu: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            lblID.Text = "-";
            txtName.Text = string.Empty;
            txtKey.Text = string.Empty;
            txtFormClass.Text = string.Empty;
            numOrder.Value = 0;
            cmbRole.SelectedIndex = 0;
            chkActive.Checked = true;
        }

        private void RefreshMenuStatistics()
        {
            try
            {
                var stats = _menuService.GetMenuStatistics();
                string statsText = $"📊 Total: {stats.TotalMenuItems} | Active: {stats.ActiveMenuItems} | Admin: {stats.AdminOnlyMenus}";
                lblStats.Text = statsText;
            }
            catch { }
        }

        private void btnAdd_Click(object sender, EventArgs e) { AddNewMenu(); }
        private void btnEdit_Click(object sender, EventArgs e) { EditMenu(); }
        private void btnDelete_Click(object sender, EventArgs e) { DeleteMenu(); }
        private void btnSave_Click(object sender, EventArgs e) { SaveMenu(); }
        private void btnCancel_Click(object sender, EventArgs e) { ClearForm(); }
    }
}
