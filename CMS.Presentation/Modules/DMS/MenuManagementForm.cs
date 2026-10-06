using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services;
using MenuItem = CMS.Domain.Models.MenuItem;

namespace CMS.Presentation
{
    public partial class MenuManagementForm : Form
    {
        private readonly IMenuManagementController _menuManagementController;
        private readonly UserContext _currentUser;
        private List<MenuItem> _allMenus = new List<MenuItem>();
        private int? _selectedParentMenuID;
        private bool _isSearchPlaceholder = true;

        public MenuManagementForm(IMenuManagementController menuManagementController, UserContext currentUser)
        {
            _menuManagementController = menuManagementController ?? throw new ArgumentNullException(nameof(menuManagementController));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            if (currentUser.AppRole != 4)
                throw new UnauthorizedAccessException("Only Admin users can access Menu Management.");

            InitializeComponent();
            this.Icon = SystemIcons.Information;
        }

        private void MenuManagementForm_Load(object sender, EventArgs e)
        {
            try
            {
                InitTreeIcons();
                PopulateActionKeys();
                cmbLevel.SelectedIndex = 0;
                cmbRole.SelectedIndex = 0;
                LoadMenus();
                RefreshMenuStatistics();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการโหลดระบบจัดการเมนู: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void InitTreeIcons()
        {
            try
            {
                imgListTree.Images.Clear();
                imgListTree.ImageSize = new Size(16, 16);

                // Add standard & GDI+ generated icons
                imgListTree.Images.Add("folder", Properties.Resources.folder);
                imgListTree.Images.Add("open_folder", Properties.Resources.open_folder);
                imgListTree.Images.Add("organization", Properties.Resources.organization);
                imgListTree.Images.Add("file_docs", Properties.Resources.File_docs.ToBitmap());

                imgListTree.Images.Add("cat_report", MenuCategoryIcons.CreateReportIcon());
                imgListTree.Images.Add("cat_permission", MenuCategoryIcons.CreatePermissionIcon());
                imgListTree.Images.Add("cat_approval", MenuCategoryIcons.CreateApprovalIcon());
                imgListTree.Images.Add("cat_trash", MenuCategoryIcons.CreateTrashIcon());
                imgListTree.Images.Add("cat_share", MenuCategoryIcons.CreateShareIcon());
                imgListTree.Images.Add("cat_config", MenuCategoryIcons.CreateConfigIcon());
                imgListTree.Images.Add("cat_sync", MenuCategoryIcons.CreateSyncIcon());
                imgListTree.Images.Add("cat_workflow", MenuCategoryIcons.CreateWorkflowIcon());
                imgListTree.Images.Add("cat_menu", MenuCategoryIcons.CreateMenuAdminIcon());
                imgListTree.Images.Add("cat_knowledge", MenuCategoryIcons.CreateKnowledgeIcon());
            }
            catch { }
        }

        private void PopulateActionKeys()
        {
            cmbActionKey.Items.Clear();
            var actionKeys = MenuActionKeyCatalog.GetAvailableActionKeys();
            foreach (var key in actionKeys)
            {
                cmbActionKey.Items.Add(key);
            }
        }

        private void LoadMenus()
        {
            try
            {
                var hierarchyMenus = (_menuManagementController.GetAllMenusForManagement(_currentUser) ?? new List<MenuItem>()).ToList();
                _allMenus = FlattenMenus(hierarchyMenus);
                RefreshMenuTree();
                ConfigureNameAutoComplete();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการดึงข้อมูลเมนู: {ex.Message}", "ข้อผิดพลาด",
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

        private void RefreshMenuTree(string filter = null)
        {
            treeMenu.BeginUpdate();
            treeMenu.Nodes.Clear();

            var rootMenus = _allMenus
                .Where(m => m.ParentMenuItemID == null)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.MenuName)
                .ToList();

            foreach (var menu in rootMenus)
            {
                if (MatchesFilterRecursive(menu, filter))
                {
                    var node = CreateTreeNode(menu);
                    treeMenu.Nodes.Add(node);
                    LoadChildNodes(node, menu.MenuItemID, filter);
                }
            }

            treeMenu.ExpandAll();
            treeMenu.EndUpdate();
        }

        private bool MatchesFilterRecursive(MenuItem menu, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter == "🔍 ค้นหาเมนู...")
                return true;

            if ((menu.MenuName ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                || (menu.MenuKey ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                || (menu.FormClassName ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            var children = _allMenus.Where(m => m.ParentMenuItemID == menu.MenuItemID);
            return children.Any(c => MatchesFilterRecursive(c, filter));
        }

        private TreeNode CreateTreeNode(MenuItem menu)
        {
            string iconKey = ResolveMenuIconKey(menu);
            string status = menu.IsActive ? string.Empty : " (ปิดใช้งาน)";
            string orderTag = $"[{menu.DisplayOrder}] ";
            string text = $"{orderTag}{menu.MenuName}{status}";

            var node = new TreeNode(text)
            {
                Tag = menu,
                ImageKey = iconKey,
                SelectedImageKey = iconKey,
                ForeColor = menu.IsActive ? Color.FromArgb(24, 37, 56) : Color.Gray
            };

            return node;
        }

        private static string ResolveMenuIconKey(MenuItem menu)
        {
            string text = ((menu?.MenuName ?? string.Empty) + " " + (menu?.MenuKey ?? string.Empty) + " " + (menu?.FormClassName ?? string.Empty)).ToLowerInvariant();

            if (text.Contains("ถังขยะ") || text.Contains("recycle") || text.Contains("trash") || text.Contains("bin"))
                return "cat_trash";
            if (text.Contains("อนุมัติ") || text.Contains("approval") || text.Contains("approve") || text.Contains("inbox"))
                return "cat_approval";
            if (text.Contains("sync") || text.Contains("ซิงค์") || text.Contains("พนักงาน") || text.Contains("employee"))
                return "cat_sync";
            if (text.Contains("share") || text.Contains("แชร์") || text.Contains("sharing"))
                return "cat_share";
            if (text.Contains("จัดการเมนู") || text.Contains("menu"))
                return "cat_menu";
            if (text.Contains("ตั้งค่า") || text.Contains("config") || text.Contains("setting") || text.Contains("ระบบ"))
                return "cat_config";
            if (text.Contains("workflow") || text.Contains("wfe") || text.Contains("wke") || text.Contains("กระบวนการ"))
                return "cat_workflow";
            if (text.Contains("audit") || text.Contains("pdpa") || text.Contains("excel") || text.Contains("รายงาน") || text.Contains("report"))
                return "cat_report";
            if (text.Contains("สิทธิ") || text.Contains("สิทธิ์") || text.Contains("permission") || text.Contains("auth") || text.Contains("access"))
                return "cat_permission";
            if (text.Contains("knowledge") || text.Contains("kms") || text.Contains("คลังความรู้") || text.Contains("kb"))
                return "cat_knowledge";
            if (text.Contains("เอกสาร") || text.Contains("document") || text.Contains("docs") || text.Contains("file"))
                return "file_docs";
            if (text.Contains("ผู้ใช้") || text.Contains("user") || text.Contains("องค์กร") || text.Contains("org"))
                return "organization";

            return "folder";
        }

        private void LoadChildNodes(TreeNode parentNode, int parentMenuItemID, string filter = null)
        {
            var children = _allMenus
                .Where(m => m.ParentMenuItemID == parentMenuItemID)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.MenuName)
                .ToList();

            foreach (var child in children)
            {
                if (MatchesFilterRecursive(child, filter))
                {
                    var node = CreateTreeNode(child);
                    parentNode.Nodes.Add(node);
                    LoadChildNodes(node, child.MenuItemID, filter);
                }
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
            lblID.Text = menu.MenuItemID.ToString();
            txtName.Text = menu.MenuName;
            txtKey.Text = menu.MenuKey;
            cmbActionKey.Text = menu.FormClassName ?? string.Empty;
            txtFormClass.Text = menu.FormClassName ?? string.Empty;
            numOrder.Value = Math.Max(1, Math.Min(9999, menu.DisplayOrder));

            int roleIndex = menu.RequiredAppRole ?? 0;
            if (roleIndex < 0 || roleIndex >= cmbRole.Items.Count)
                roleIndex = 0;
            cmbRole.SelectedIndex = roleIndex;

            cmbLevel.SelectedIndex = menu.ParentMenuItemID.HasValue ? 1 : 0;
            _selectedParentMenuID = menu.ParentMenuItemID;
            UpdateParentDisplay();

            chkActive.Checked = menu.IsActive;
            txtDescription.Text = menu.Description ?? string.Empty;
            UpdateParentStateByLevel();
        }

        private void AddNewMenu()
        {
            ClearForm();
            lblID.Text = "(New)";
            txtKey.Text = GenerateInitialNewMenuKey();

            if (treeMenu.SelectedNode?.Tag is MenuItem selectedMenu)
            {
                cmbLevel.SelectedIndex = 1; // Default to child under selected node
                _selectedParentMenuID = selectedMenu.MenuItemID;
                UpdateParentDisplay();

                // Set order to next available order among siblings
                int maxOrder = _allMenus.Where(m => m.ParentMenuItemID == selectedMenu.MenuItemID).Select(m => m.DisplayOrder).DefaultIfEmpty(0).Max();
                numOrder.Value = maxOrder + 1;
            }
            else
            {
                cmbLevel.SelectedIndex = 0;
                int maxOrder = _allMenus.Where(m => m.ParentMenuItemID == null).Select(m => m.DisplayOrder).DefaultIfEmpty(0).Max();
                numOrder.Value = maxOrder + 1;
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
                MessageBox.Show("กรุณาเลือกเมนูที่ต้องการแก้ไขจากโครงสร้างด้านซ้าย", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DisplayMenuDetails(menu);
            txtName.Focus();
        }

        private void DeleteMenu()
        {
            if (!(treeMenu.SelectedNode?.Tag is MenuItem menu))
            {
                MessageBox.Show("กรุณาเลือกเมนูที่ต้องการลบจากโครงสร้างด้านซ้าย", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int childCount = _allMenus.Count(m => m.ParentMenuItemID == menu.MenuItemID);
            string warningMessage = childCount > 0
                ? $"คุณต้องการลบเมนู '{menu.MenuName}' และเมนูย่อยทั้งหมด ({childCount} รายการ) หรือไม่?"
                : $"คุณต้องการลบเมนู '{menu.MenuName}' หรือไม่?";

            if (MessageBox.Show(warningMessage, "ยืนยันการลบเมนู",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                _menuManagementController.DeleteMenu(menu.MenuItemID, _currentUser);
                MessageBox.Show("ลบเมนูเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadMenus();
                ClearForm();
                RefreshMenuStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการลบเมนู: {ex.Message}", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveMenu()
        {
            try
            {
                string idText = lblID.Text;
                string name = txtName.Text?.Trim() ?? string.Empty;
                string key = txtKey.Text?.Trim() ?? string.Empty;
                string actionKey = cmbActionKey.Text?.Trim() ?? string.Empty;
                int order = (int)numOrder.Value;
                int? role = cmbRole.SelectedIndex > 0 ? cmbRole.SelectedIndex : (int?)null;
                bool isActive = chkActive.Checked;
                int? parentID = IsChildLevelSelected() ? _selectedParentMenuID : (int?)null;
                string description = txtDescription.Text?.Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("กรุณากรอกชื่อเมนู (Menu Name)", "ข้อผิดพลาด",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    key = GenerateUniqueKeyFromName(name);
                    txtKey.Text = key;
                }

                if (IsChildLevelSelected() && !parentID.HasValue)
                {
                    MessageBox.Show("กรุณาเลือก เมนูแม่ (Parent Menu) เมื่อเลือกระดับเป็น 'ลูก (Child)'", "ข้อผิดพลาด",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    BtnPickParent_Click(this, EventArgs.Empty);
                    return;
                }

                // Self reference guard
                if (idText != "(New)" && int.TryParse(idText, out int currentId) && parentID.HasValue && parentID.Value == currentId)
                {
                    MessageBox.Show("ไม่สามารถเลือกเมนูตัวเองเป็นเมนูแม่ (Parent) ได้", "ข้อผิดพลาด",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int? savedMenuID = null;

                if (idText == "(New)" || idText == "-")
                {
                    var newMenu = new MenuItem
                    {
                        MenuName = name,
                        MenuKey = key,
                        FormClassName = actionKey,
                        DisplayOrder = order,
                        RequiredAppRole = role,
                        IsActive = isActive,
                        ParentMenuItemID = parentID,
                        Description = description
                    };

                    savedMenuID = _menuManagementController.CreateMenu(newMenu, _currentUser);
                    MessageBox.Show("เพิ่มเมนูใหม่สำเร็จ", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (int.TryParse(idText, out int menuID))
                {
                    var existingMenu = _allMenus.FirstOrDefault(m => m.MenuItemID == menuID);
                    if (existingMenu != null)
                    {
                        existingMenu.MenuName = name;
                        existingMenu.MenuKey = key;
                        existingMenu.FormClassName = actionKey;
                        existingMenu.DisplayOrder = order;
                        existingMenu.RequiredAppRole = role;
                        existingMenu.IsActive = isActive;
                        existingMenu.ParentMenuItemID = parentID;
                        existingMenu.Description = description;

                        _menuManagementController.UpdateMenu(existingMenu, _currentUser);
                        savedMenuID = menuID;
                        MessageBox.Show("บันทึกการแก้ไขเมนูสำเร็จ", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                LoadMenus();
                RefreshMenuStatistics();

                if (savedMenuID.HasValue)
                    SelectNodeByMenuID(savedMenuID.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการบันทึก: {ex.Message}", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectNodeByMenuID(int menuID)
        {
            foreach (TreeNode root in treeMenu.Nodes)
            {
                var target = FindNodeByMenuID(root, menuID);
                if (target != null)
                {
                    treeMenu.SelectedNode = target;
                    target.EnsureVisible();
                    return;
                }
            }
        }

        private TreeNode FindNodeByMenuID(TreeNode current, int menuID)
        {
            if (current.Tag is MenuItem item && item.MenuItemID == menuID)
                return current;

            foreach (TreeNode child in current.Nodes)
            {
                var found = FindNodeByMenuID(child, menuID);
                if (found != null)
                    return found;
            }

            return null;
        }

        private void ClearForm()
        {
            lblID.Text = "(New)";
            txtName.Text = string.Empty;
            txtKey.Text = GenerateInitialNewMenuKey();
            cmbActionKey.Text = string.Empty;
            numOrder.Value = 1;
            cmbRole.SelectedIndex = 0;
            cmbLevel.SelectedIndex = 0;
            _selectedParentMenuID = null;
            UpdateParentDisplay();
            chkActive.Checked = true;
            txtDescription.Text = string.Empty;
            UpdateParentStateByLevel();
        }

        private void CmbLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateParentStateByLevel();
        }

        private bool IsChildLevelSelected()
        {
            return cmbLevel != null && cmbLevel.SelectedIndex == 1;
        }

        private void UpdateParentStateByLevel()
        {
            bool isChild = IsChildLevelSelected();
            lblParentLabel.Text = isChild ? "เมนูแม่ (Parent):*" : "เมนูแม่ (Parent):";
            btnPickParent.Enabled = isChild;
            btnClearParent.Enabled = isChild;
            txtParentDisplay.BackColor = isChild ? Color.White : Color.FromArgb(245, 245, 245);
            txtParentDisplay.Cursor = isChild ? Cursors.Hand : Cursors.Default;

            if (!isChild)
            {
                _selectedParentMenuID = null;
                UpdateParentDisplay();
            }
        }

        private void BtnPickParent_Click(object sender, EventArgs e)
        {
            int? excludeID = null;
            if (int.TryParse(lblID.Text, out int id))
                excludeID = id;

            using (var dlg = new ParentMenuPickerDialog(_allMenus, _selectedParentMenuID, excludeID))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _selectedParentMenuID = dlg.SelectedMenuItemID;
                    if (_selectedParentMenuID.HasValue)
                        cmbLevel.SelectedIndex = 1;
                    else
                        cmbLevel.SelectedIndex = 0;

                    UpdateParentDisplay();
                    UpdateParentStateByLevel();
                }
            }
        }

        private void BtnClearParent_Click(object sender, EventArgs e)
        {
            _selectedParentMenuID = null;
            cmbLevel.SelectedIndex = 0;
            UpdateParentDisplay();
            UpdateParentStateByLevel();
        }

        private void TxtParentDisplay_Click(object sender, EventArgs e)
        {
            if (btnPickParent.Enabled)
                BtnPickParent_Click(sender, e);
        }

        private void UpdateParentDisplay()
        {
            if (!_selectedParentMenuID.HasValue)
            {
                txtParentDisplay.Text = "(ไม่มี — Root)";
                return;
            }

            txtParentDisplay.Text = BuildParentPathText(_selectedParentMenuID.Value);
        }

        private string BuildParentPathText(int parentMenuID)
        {
            var segments = new List<string>();
            int? currentID = parentMenuID;
            int guard = 0;

            while (currentID.HasValue && guard < 50)
            {
                var menu = _allMenus.FirstOrDefault(m => m.MenuItemID == currentID.Value);
                if (menu == null) break;
                segments.Insert(0, menu.MenuName);
                currentID = menu.ParentMenuItemID;
                guard++;
            }

            return segments.Count == 0 ? "(ไม่มี — Root)" : string.Join(" › ", segments);
        }

        private void RefreshMenuStatistics()
        {
            try
            {
                var stats = _menuManagementController.GetMenuStatistics();
                lblStats.Text = $"📊 รวมทั้งหมด: {stats.TotalMenuItems} รายการ | 🟢 เปิดใช้งาน: {stats.ActiveMenuItems} | ⚪ ปิดใช้งาน: {stats.TotalMenuItems - stats.ActiveMenuItems} | 👑 สิทธิ์เฉพาะ Admin: {stats.AdminOnlyMenus}";
            }
            catch
            {
                lblStats.Text = $"📊 รวมทั้งหมด: {_allMenus.Count} รายการ | 🟢 เปิดใช้งาน: {_allMenus.Count(m => m.IsActive)} | ⚪ ปิดใช้งาน: {_allMenus.Count(m => !m.IsActive)}";
            }
        }

        // ── Move Up / Move Down Reordering ──────────────────────────────────────
        private void BtnMoveUp_Click(object sender, EventArgs e)
        {
            MoveSelectedMenuOrder(-1);
        }

        private void BtnMoveDown_Click(object sender, EventArgs e)
        {
            MoveSelectedMenuOrder(1);
        }

        private void MoveSelectedMenuOrder(int direction)
        {
            if (!(treeMenu.SelectedNode?.Tag is MenuItem currentMenu))
            {
                MessageBox.Show("กรุณาเลือกเมนูที่ต้องการปรับลำดับ", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var siblings = _allMenus
                .Where(m => m.ParentMenuItemID == currentMenu.ParentMenuItemID)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.MenuName)
                .ToList();

            int currentIndex = siblings.FindIndex(m => m.MenuItemID == currentMenu.MenuItemID);
            if (currentIndex < 0) return;

            int targetIndex = currentIndex + direction;
            if (targetIndex < 0 || targetIndex >= siblings.Count)
                return; // Already at top or bottom

            var targetMenu = siblings[targetIndex];

            // Swap DisplayOrder
            int tempOrder = currentMenu.DisplayOrder;
            currentMenu.DisplayOrder = targetMenu.DisplayOrder;
            targetMenu.DisplayOrder = tempOrder;

            if (currentMenu.DisplayOrder == targetMenu.DisplayOrder)
            {
                // Ensure distinct orders if they were equal
                currentMenu.DisplayOrder = targetIndex + 1;
                targetMenu.DisplayOrder = currentIndex + 1;
            }

            try
            {
                _menuManagementController.UpdateMenu(currentMenu, _currentUser);
                _menuManagementController.UpdateMenu(targetMenu, _currentUser);

                int savedID = currentMenu.MenuItemID;
                LoadMenus();
                SelectNodeByMenuID(savedID);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการสลับลำดับ: {ex.Message}", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Search & Filter ───────────────────────────────────────────────────
        private void TxtSearchTree_TextChanged(object sender, EventArgs e)
        {
            if (_isSearchPlaceholder) return;
            string text = txtSearchTree.Text?.Trim() ?? string.Empty;
            RefreshMenuTree(text);
        }

        private void TxtSearchTree_Enter(object sender, EventArgs e)
        {
            if (_isSearchPlaceholder)
            {
                _isSearchPlaceholder = false;
                txtSearchTree.Text = string.Empty;
                txtSearchTree.ForeColor = Color.FromArgb(24, 37, 56);
            }
        }

        private void TxtSearchTree_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchTree.Text))
            {
                _isSearchPlaceholder = true;
                txtSearchTree.Text = "🔍 ค้นหาเมนู...";
                txtSearchTree.ForeColor = Color.Gray;
                RefreshMenuTree(null);
            }
        }

        // ── Toolbar Actions ───────────────────────────────────────────────────
        private void BtnExpandAll_Click(object sender, EventArgs e) => treeMenu.ExpandAll();
        private void BtnCollapseAll_Click(object sender, EventArgs e) => treeMenu.CollapseAll();
        private void BtnRefreshTree_Click(object sender, EventArgs e)
        {
            LoadMenus();
            RefreshMenuStatistics();
        }

        private void btnAdd_Click(object sender, EventArgs e) => AddNewMenu();
        private void btnEdit_Click(object sender, EventArgs e) => EditMenu();
        private void btnDelete_Click(object sender, EventArgs e) => DeleteMenu();
        private void btnSave_Click(object sender, EventArgs e) => SaveMenu();
        private void btnCancel_Click(object sender, EventArgs e) => ClearForm();

        private void TxtName_TextChanged(object sender, EventArgs e)
        {
            if (lblID.Text == "(New)" || lblID.Text == "-")
            {
                string generatedKey = GenerateUniqueKeyFromName(txtName.Text);
                if (!string.IsNullOrWhiteSpace(generatedKey))
                    txtKey.Text = generatedKey;
                else if (string.IsNullOrWhiteSpace(txtKey.Text))
                    txtKey.Text = GenerateInitialNewMenuKey();
            }
        }

        private string GenerateKeyFromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            var slug = Regex.Replace(name.Trim().ToLower(), @"[^a-z0-9_]", "_");
            slug = Regex.Replace(slug, @"_+", "_");
            return slug.Trim('_');
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

            int next = Math.Min(9999, maxRunning + 1);
            return $"{baseKey}_{next:D4}";
        }
    }
}
