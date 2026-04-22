using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using MenuItem = CMS.Domain.Models.MenuItem;

namespace CMS.Presentation
{
    public partial class MenuManagementForm : Form
    {
        private readonly IMenuManagementController _menuManagementController;
        private readonly UserContext _currentUser;
        private System.Collections.Generic.List<MenuItem> _allMenus = new System.Collections.Generic.List<MenuItem>();
        private ComboBox _cmbLevel;
        private Label _lblLevel;
        private ComboBox _cmbActionKey;

        public MenuManagementForm(IMenuManagementController menuManagementController, UserContext currentUser)
        {
            if (menuManagementController == null) throw new ArgumentNullException(nameof(menuManagementController));
            if (currentUser == null) throw new ArgumentNullException(nameof(currentUser));

            InitializeComponent();

            _menuManagementController = menuManagementController;
            _currentUser = currentUser;

            if (currentUser.AppRole != 4)
                throw new UnauthorizedAccessException("Only Admin users can access Menu Management.");

            this.Icon = SystemIcons.Information;
        }

        private void MenuManagementForm_Load(object sender, EventArgs e)
        {
            try
            {
                ConfigureActionKeySelector();
                ConfigureLevelSelector();
                ApplyOffice2022NavyTheme();
                txtKey.ReadOnly = true;
                txtKey.TabStop = false;
                treeMenu.ShowLines = true;
                treeMenu.ShowRootLines = true;
                treeMenu.ShowPlusMinus = true;
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
                var hierarchyMenus = (_menuManagementController.GetAllMenusForManagement(_currentUser) ?? new List<MenuItem>()).ToList();
                _allMenus = FlattenMenus(hierarchyMenus);
                RefreshMenuTree();
                PopulateParentCombo();
                ConfigureNameAutoComplete();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading menus: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureNameAutoComplete()
        {
            var source = new AutoCompleteStringCollection();
            source.AddRange(_allMenus
                .Where(m => !string.IsNullOrWhiteSpace(m.MenuName))
                .Select(m => m.MenuName.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .ToArray());

            txtName.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtName.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtName.AutoCompleteCustomSource = source;
        }

        private void ConfigureActionKeySelector()
        {
            if (_cmbActionKey != null || grpDetails == null || txtFormClass == null)
                return;

            _cmbActionKey = new ComboBox
            {
                Name = "cmbActionKey",
                DropDownStyle = ComboBoxStyle.DropDown,
                Location = txtFormClass.Location,
                Size = txtFormClass.Size,
                Anchor = txtFormClass.Anchor
            };

            _cmbActionKey.Items.AddRange(MenuActionKeyCatalog.GetAvailableActionKeys());
            _cmbActionKey.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _cmbActionKey.AutoCompleteSource = AutoCompleteSource.ListItems;
            _cmbActionKey.Text = txtFormClass.Text;

            grpDetails.Controls.Add(_cmbActionKey);

            lblFormClassLabel.Text = "Action Key:";
            txtFormClass.Visible = false;
            txtFormClass.TabStop = false;
        }

        private string GetConfiguredActionKey()
        {
            string configuredValue = _cmbActionKey != null
                ? (_cmbActionKey.Text ?? string.Empty).Trim()
                : (txtFormClass.Text ?? string.Empty).Trim();

            string resolved = MenuActionKeyCatalog.ResolveConfiguredActionKey(configuredValue);
            return string.IsNullOrWhiteSpace(resolved) ? configuredValue : resolved;
        }

        private bool ValidateConfiguredActionKey(string configuredValue)
        {
            if (string.IsNullOrWhiteSpace(configuredValue))
                return true;

            string resolved = MenuActionKeyCatalog.ResolveConfiguredActionKey(configuredValue);
            if (!string.IsNullOrWhiteSpace(resolved))
                return true;

            var result = MessageBox.Show(
                "Action Key นี้ยังไม่อยู่ในรายการ built-in หรือ config ปัจจุบัน\n\n" +
                "หากเป็นเมนูจาก module ใหม่ คุณยังสามารถบันทึกได้ แล้วไปเพิ่ม handler/config ภายหลัง\n\n" +
                "ต้องการบันทึกต่อหรือไม่?",
                "Unknown Action Key",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        private void PopulateParentCombo(int? excludeMenuItemID = null)
        {
            cmbParent.Items.Clear();
            cmbParent.Items.Add(new ParentMenuItem(null, "(None - Root)"));
            foreach (var m in _allMenus.OrderBy(m => m.DisplayOrder).ThenBy(m => m.MenuName))
            {
                int level = GetMenuLevel(m);
                string indent = new string(' ', level * 2);
                cmbParent.Items.Add(new ParentMenuItem(m.MenuItemID, $"{indent}[L{level}] [ID:{m.MenuItemID}] {m.MenuName}"));
            }
            cmbParent.SelectedIndex = 0;
            UpdateParentStateByLevel();
        }

        private List<MenuItem> FlattenMenus(List<MenuItem> hierarchyMenus)
        {
            var result = new List<MenuItem>();
            var visited = new HashSet<int>();

            foreach (var menu in hierarchyMenus)
                FlattenMenuRecursive(menu, result, visited);

            return result;
        }

        private void FlattenMenuRecursive(MenuItem menu, List<MenuItem> result, HashSet<int> visited)
        {
            if (menu == null || visited.Contains(menu.MenuItemID))
                return;

            visited.Add(menu.MenuItemID);
            result.Add(menu);

            if (menu.ChildMenuItems == null || menu.ChildMenuItems.Count == 0)
                return;

            foreach (var child in menu.ChildMenuItems)
                FlattenMenuRecursive(child, result, visited);
        }

        private void ConfigureLevelSelector()
        {
            if (_cmbLevel != null || grpDetails == null)
                return;

            _lblLevel = new Label
            {
                Name = "lblLevel",
                Text = "Menu Level:",
                Location = new Point(10, 205),
                Size = new Size(100, 23)
            };

            _cmbLevel = new ComboBox
            {
                Name = "cmbLevel",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(120, 202),
                Size = new Size(200, 23)
            };
            _cmbLevel.Items.AddRange(new object[] { "แม่ (Root)", "ลูก (Child)" });
            _cmbLevel.SelectedIndex = 0;
            _cmbLevel.SelectedIndexChanged += CmbLevel_SelectedIndexChanged;

            grpDetails.Controls.Add(_lblLevel);
            grpDetails.Controls.Add(_cmbLevel);

            // Shift controls down to make room for level selection.
            lblParentLabel.Top = 235;
            cmbParent.Top = 232;
            chkActive.Top = 265;
        }

        private void ApplyOffice2022NavyTheme()
        {
            var formBack = Color.FromArgb(236, 242, 250);
            var panelBack = Color.FromArgb(248, 251, 255);
            var accent = Color.FromArgb(18, 67, 122);
            var accentSoft = Color.FromArgb(225, 236, 248);
            var textMain = Color.FromArgb(24, 37, 56);
            var border = Color.FromArgb(189, 205, 226);

            BackColor = formBack;
            ForeColor = textMain;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            splitMain.BackColor = border;
            splitRight.BackColor = border;

            StyleGroupBox(grpDetails, panelBack, accent, textMain);
            StyleGroupBox(grpActions, panelBack, accent, textMain);

            lblMenuTree.BackColor = accentSoft;
            lblMenuTree.ForeColor = accent;

            treeMenu.BackColor = Color.White;
            treeMenu.ForeColor = textMain;
            treeMenu.BorderStyle = BorderStyle.FixedSingle;
            treeMenu.LineColor = border;

            StyleInput(txtName, border, textMain);
            StyleInput(txtKey, border, textMain);
            StyleInput(txtFormClass, border, textMain);
            if (_cmbActionKey != null)
                StyleInput(_cmbActionKey, border, textMain);
            StyleInput(cmbRole, border, textMain);
            StyleInput(cmbParent, border, textMain);
            StyleInput(numOrder, border, textMain);

            if (_cmbLevel != null)
                StyleInput(_cmbLevel, border, textMain);
            if (_lblLevel != null)
                _lblLevel.ForeColor = textMain;

            chkActive.ForeColor = accent;

            StyleActionButton(btnAdd, Color.FromArgb(35, 92, 157), Color.White);
            StyleActionButton(btnEdit, Color.FromArgb(63, 120, 184), Color.White);
            StyleActionButton(btnDelete, Color.FromArgb(189, 64, 64), Color.White);
            StyleActionButton(btnSave, Color.FromArgb(21, 112, 74), Color.White);
            StyleActionButton(btnCancel, Color.FromArgb(118, 132, 154), Color.White);

            lblStats.ForeColor = accent;
        }

        private static void StyleGroupBox(GroupBox groupBox, Color backColor, Color titleColor, Color textColor)
        {
            if (groupBox == null)
                return;

            groupBox.BackColor = backColor;
            groupBox.ForeColor = titleColor;
            foreach (Control control in groupBox.Controls)
            {
                if (control is Label)
                    control.ForeColor = textColor;
            }
        }

        private static void StyleInput(Control control, Color borderColor, Color textColor)
        {
            if (control == null)
                return;

            control.BackColor = Color.White;
            control.ForeColor = textColor;

            if (control is TextBox textBox)
                textBox.BorderStyle = BorderStyle.FixedSingle;
            if (control is NumericUpDown numeric)
                numeric.BorderStyle = BorderStyle.FixedSingle;
            if (control is ComboBox combo)
                combo.FlatStyle = FlatStyle.Flat;
        }

        private static void StyleActionButton(Button button, Color backColor, Color foreColor)
        {
            if (button == null)
                return;

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Regular);
        }

        private void CmbLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateParentStateByLevel();
        }

        private bool IsChildLevelSelected()
        {
            return _cmbLevel != null && _cmbLevel.SelectedIndex == 1;
        }

        private void UpdateParentStateByLevel()
        {
            bool isChild = IsChildLevelSelected();
            cmbParent.Enabled = isChild;
            lblParentLabel.Text = isChild ? "Parent Menu:*" : "Parent Menu:";

            if (isChild)
            {
                // Child level should point to an actual parent by default.
                if (cmbParent.Items.Count > 1 && cmbParent.SelectedIndex <= 0)
                    cmbParent.SelectedIndex = 1;
            }
            else if (cmbParent.Items.Count > 0)
            {
                cmbParent.SelectedIndex = 0;
            }
        }

        private int GetMenuLevel(MenuItem menu)
        {
            int level = 0;
            int guard = 0;
            int? currentParentId = menu.ParentMenuItemID;

            while (currentParentId.HasValue && guard < 50)
            {
                var parent = _allMenus.FirstOrDefault(m => m.MenuItemID == currentParentId.Value);
                if (parent == null)
                    break;

                level++;
                currentParentId = parent.ParentMenuItemID;
                guard++;
            }

            return level;
        }

        // Helper wrapper so ComboBox can display text while storing the ID
        private class ParentMenuItem
        {
            public int? ID { get; }
            private readonly string _display;
            public ParentMenuItem(int? id, string display) { ID = id; _display = display; }
            public override string ToString() => _display;
        }

        private void RefreshMenuTree()
        {
            treeMenu.Nodes.Clear();

            // Debug: log all menus
            System.Diagnostics.Debug.WriteLine($"=== RefreshMenuTree: Total menus = {_allMenus.Count} ===");
            foreach (var m in _allMenus)
                System.Diagnostics.Debug.WriteLine($"ID:{m.MenuItemID} Name:{m.MenuName} Parent:{m.ParentMenuItemID}");

            var rootMenus = _allMenus.Where(m => m.ParentMenuItemID == null).OrderBy(m => m.DisplayOrder).ToList();
            System.Diagnostics.Debug.WriteLine($"Root menus count: {rootMenus.Count()}");

            foreach (var menu in rootMenus)
            {
                var node = CreateTreeNode(menu, 0);
                treeMenu.Nodes.Add(node);
                LoadChildNodes(node, menu.MenuItemID, 1);
            }

            treeMenu.ExpandAll();
        }

        private TreeNode CreateTreeNode(MenuItem menu, int depth)
        {
            string status = menu.IsActive ? string.Empty : " (Inactive)";
            string text = $"📁 {menu.MenuName}{status}";
            return new TreeNode(text) { Tag = menu };
        }

        private void LoadChildNodes(TreeNode parentNode, int parentMenuItemID, int depth)
        {
            var children = _allMenus.Where(m => m.ParentMenuItemID == parentMenuItemID).OrderBy(m => m.DisplayOrder).ToList();
            System.Diagnostics.Debug.WriteLine($"LoadChildNodes: parentID={parentMenuItemID}, children count={children.Count()}");

            foreach (var child in children)
            {
                var node = CreateTreeNode(child, depth);
                parentNode.Nodes.Add(node);
                LoadChildNodes(node, child.MenuItemID, depth + 1);
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
            if (_cmbActionKey != null)
                _cmbActionKey.Text = txtFormClass.Text;
            numOrder.Value = menu.DisplayOrder;

            int roleIndex = menu.RequiredAppRole ?? 0;
            if (roleIndex < 0 || roleIndex >= cmbRole.Items.Count)
                roleIndex = 0;
            cmbRole.SelectedIndex = roleIndex;

            // Repopulate parent combo excluding this menu itself (prevent circular)
            PopulateParentCombo(menu.MenuItemID);

            if (_cmbLevel != null)
                _cmbLevel.SelectedIndex = menu.ParentMenuItemID.HasValue ? 1 : 0;

            // Select parent in dropdown
            for (int i = 0; i < cmbParent.Items.Count; i++)
            {
                var item = cmbParent.Items[i] as ParentMenuItem;
                if (item != null && item.ID == menu.ParentMenuItemID)
                {
                    cmbParent.SelectedIndex = i;
                    break;
                }
            }

            chkActive.Checked = menu.IsActive;
            lblID.Text = menu.MenuItemID.ToString();
            UpdateParentStateByLevel();
        }

        private void AddNewMenu()
        {
            ClearForm();
            lblID.Text = "(New)";

            // Prepare a key immediately when user clicks Add.
            txtKey.Text = GenerateInitialNewMenuKey();
            
            // Auto-set parent from selected tree node
            if (treeMenu.SelectedNode?.Tag is MenuItem selectedMenu)
            {
                if (_cmbLevel != null)
                    _cmbLevel.SelectedIndex = 1;

                // Set parent to the selected menu
                for (int i = 0; i < cmbParent.Items.Count; i++)
                {
                    var item = cmbParent.Items[i] as ParentMenuItem;
                    if (item != null && item.ID == selectedMenu.MenuItemID)
                    {
                        cmbParent.SelectedIndex = i;
                        break;
                    }
                }
            }
            else if (_cmbLevel != null)
            {
                _cmbLevel.SelectedIndex = 0;
            }

            UpdateParentStateByLevel();
            
            txtName.Focus();
        }

        private string GenerateInitialNewMenuKey()
        {
            return GenerateUniqueKeyFromName("menu");
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
                _menuManagementController.DeleteMenu(menu.MenuItemID, _currentUser);
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
                string rawActionKey = _cmbActionKey != null ? (_cmbActionKey.Text ?? string.Empty).Trim() : (txtFormClass.Text ?? string.Empty).Trim();
                if (!ValidateConfiguredActionKey(rawActionKey))
                    return;

                string formClass = GetConfiguredActionKey();
                int order = (int)numOrder.Value;
                int? role = cmbRole.SelectedIndex > 0 ? cmbRole.SelectedIndex : (int?)null;
                bool isActive = chkActive.Checked;
                int? parentID = IsChildLevelSelected() ? (cmbParent.SelectedItem as ParentMenuItem)?.ID : (int?)null;
                int? savedMenuID = null;

                if (IsChildLevelSelected() && !parentID.HasValue)
                {
                    MessageBox.Show("กรุณาเลือก Parent Menu เมื่อเป็น Level ลูก", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Guard: prevent self-reference
                if (idText != "(New)" && parentID.HasValue && parentID.Value == int.Parse(idText))
                {
                    MessageBox.Show("ไม่สามารถเลือก menu ตัวเองเป็น Parent ได้", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (idText != "(New)" && parentID.HasValue)
                {
                    int currentID = int.Parse(idText);
                    if (IsDescendantMenu(parentID.Value, currentID))
                    {
                        MessageBox.Show("ไม่สามารถเลือกเมนูลูกหลานเป็น Parent ได้", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Validate parent exists in DB
                if (parentID.HasValue && !_allMenus.Any(m => m.MenuItemID == parentID.Value))
                {
                    MessageBox.Show("Parent menu ที่เลือกไม่มีอยู่ในระบบ", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

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
                        ParentMenuItemID = parentID,
                        CreatedByUserID = _currentUser.UserID
                    };

                    int newID = _menuManagementController.CreateMenu(newMenu, _currentUser);
                    savedMenuID = newID;
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
                        existingMenu.ParentMenuItemID = parentID;

                        _menuManagementController.UpdateMenu(existingMenu, _currentUser);
                        savedMenuID = id;
                    }
                }

                LoadMenus();
                RefreshMenuStatistics();

                if (savedMenuID.HasValue)
                    SelectNodeByMenuID(savedMenuID.Value);

                MessageBox.Show("บันทึกสำเร็จ และอัปเดต TreeView เรียบร้อย", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving menu: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsDescendantMenu(int menuID, int possibleAncestorID)
        {
            int? parentID = _allMenus.FirstOrDefault(m => m.MenuItemID == menuID)?.ParentMenuItemID;
            int guard = 0;

            while (parentID.HasValue && guard < 50)
            {
                if (parentID.Value == possibleAncestorID)
                    return true;

                parentID = _allMenus.FirstOrDefault(m => m.MenuItemID == parentID.Value)?.ParentMenuItemID;
                guard++;
            }

            return false;
        }

        private void SelectNodeByMenuID(int menuID)
        {
            foreach (TreeNode root in treeMenu.Nodes)
            {
                var found = FindNodeByMenuID(root, menuID);
                if (found == null)
                    continue;

                treeMenu.SelectedNode = found;
                found.EnsureVisible();
                found.Expand();
                return;
            }
        }

        private TreeNode FindNodeByMenuID(TreeNode node, int menuID)
        {
            if (node?.Tag is MenuItem menu && menu.MenuItemID == menuID)
                return node;

            foreach (TreeNode child in node.Nodes)
            {
                var found = FindNodeByMenuID(child, menuID);
                if (found != null)
                    return found;
            }

            return null;
        }

        private void ClearForm()
        {
            lblID.Text = "-";
            txtName.Text = string.Empty;
            txtKey.Text = string.Empty;
            txtFormClass.Text = string.Empty;
            if (_cmbActionKey != null)
                _cmbActionKey.Text = string.Empty;
            numOrder.Value = 0;
            cmbRole.SelectedIndex = 0;
            if (cmbParent.Items.Count > 0) cmbParent.SelectedIndex = 0;
            chkActive.Checked = true;

            if (_cmbLevel != null)
                _cmbLevel.SelectedIndex = 0;

            UpdateParentStateByLevel();
        }

        private void RefreshMenuStatistics()
        {
            try
            {
                var stats = _menuManagementController.GetMenuStatistics();
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

        private void TxtName_TextChanged(object sender, EventArgs e)
        {
            // Key is always system-generated for new items.
            if (lblID.Text == "(New)")
            {
                string generatedKey = GenerateUniqueKeyFromName(txtName.Text);
                if (!string.IsNullOrWhiteSpace(generatedKey))
                {
                    txtKey.Text = generatedKey;
                }
                else if (string.IsNullOrWhiteSpace(txtKey.Text))
                {
                    txtKey.Text = GenerateInitialNewMenuKey();
                }
            }
        }

        private string GenerateKeyFromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // Convert Thai/Unicode to ASCII-friendly slug
            // Remove spaces, convert to lowercase, keep only alphanumeric and underscore
            var slug = System.Text.RegularExpressions.Regex.Replace(name.Trim().ToLower(), @"[^a-z0-9_]", "_");
            // Remove consecutive underscores
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"_+", "_");
            // Trim leading/trailing underscores
            slug = slug.Trim('_');
            return slug;
        }

        private string GenerateUniqueKeyFromName(string name)
        {
            string baseKey = GenerateKeyFromName(name);
            if (string.IsNullOrWhiteSpace(baseKey))
                return string.Empty;

            var sameBaseKeys = _allMenus
                .Where(m => !string.IsNullOrWhiteSpace(m.MenuKey))
                .Select(m => m.MenuKey)
                .Where(k => k.StartsWith(baseKey + "_", StringComparison.OrdinalIgnoreCase))
                .ToList();

            int maxRunning = 0;
            foreach (var key in sameBaseKeys)
            {
                if (key.Length != baseKey.Length + 5)
                    continue;

                string suffix = key.Substring(baseKey.Length + 1);
                if (int.TryParse(suffix, out int n) && suffix.Length == 4 && n > maxRunning)
                    maxRunning = n;
            }

            int next = maxRunning + 1;
            if (next > 9999)
                next = 9999;

            return $"{baseKey}_{next:D4}";
        }
    }
}
