using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
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
        private readonly IMenuManagementController _menuManagementController;
        private readonly UserContext _currentUser;
        private Dictionary<string, Image> _iconCache = new Dictionary<string, Image>();

        // Event for menu selection
        public event EventHandler<MenuSelectedEventArgs> MenuSelected;

        public MenuTreeViewManager(TreeView treeView, IMenuManagementController menuManagementController, UserContext currentUser)
        {
            _treeView = treeView ?? throw new ArgumentNullException(nameof(treeView));
            _menuManagementController = menuManagementController ?? throw new ArgumentNullException(nameof(menuManagementController));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            EnsureTreeImageList();

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

                var rootMenus = _menuManagementController.GetRootMenus(_currentUser);

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
            return new List<MenuItem>(_menuManagementController.GetVisibleMenus(_currentUser));
        }

        // ────────────────────────────────────────────────────────────────────
        // Private Helper Methods
        // ────────────────────────────────────────────────────────────────────

        private TreeNode CreateTreeNode(MenuItem menu)
        {
            string iconKey = ResolveMenuIconKey(menu);
            var node = new TreeNode(menu.MenuName)
            {
                Tag = menu,
                ToolTipText = menu.Description ?? menu.MenuName,
                ForeColor = menu.IsActive ? Color.Black : Color.Gray,
                ImageKey = iconKey,
                SelectedImageKey = iconKey
            };

            return node;
        }

        private void EnsureTreeImageList()
        {
            if (_treeView.ImageList != null)
                return;

            var imageList = new ImageList
            {
                ImageSize = new Size(16, 16),
                ColorDepth = ColorDepth.Depth32Bit
            };

            // --- resource-based icons (existing) ---
            imageList.Images.Add("folder",        Properties.Resources.folder);
            imageList.Images.Add("open_folder",   Properties.Resources.open_folder);
            imageList.Images.Add("organization",  Properties.Resources.organization);
            imageList.Images.Add("file_docs",     Properties.Resources.File_docs.ToBitmap());
            imageList.Images.Add("book_red",      Properties.Resources.book_red);
            imageList.Images.Add("book_orange",   Properties.Resources.book_orange);
            imageList.Images.Add("book_blue",     Properties.Resources.book_bule);
            imageList.Images.Add("book_green",    Properties.Resources.book_green);
            try   { imageList.Images.Add("lightning_bolt", Properties.Resources.lightning_bolt1); }
            catch { imageList.Images.Add("lightning_bolt", Properties.Resources.book_red1.ToBitmap()); }

            // --- category icons generated via GDI+ ---
            imageList.Images.Add("cat_report",     MenuCategoryIcons.CreateReportIcon());
            imageList.Images.Add("cat_permission", MenuCategoryIcons.CreatePermissionIcon());
            imageList.Images.Add("cat_approval",   MenuCategoryIcons.CreateApprovalIcon());
            imageList.Images.Add("cat_trash",      MenuCategoryIcons.CreateTrashIcon());
            imageList.Images.Add("cat_share",      MenuCategoryIcons.CreateShareIcon());
            imageList.Images.Add("cat_config",     MenuCategoryIcons.CreateConfigIcon());
            imageList.Images.Add("cat_sync",       MenuCategoryIcons.CreateSyncIcon());
            imageList.Images.Add("cat_workflow",   MenuCategoryIcons.CreateWorkflowIcon());
            imageList.Images.Add("cat_menu",       MenuCategoryIcons.CreateMenuAdminIcon());
            imageList.Images.Add("cat_knowledge",  MenuCategoryIcons.CreateKnowledgeIcon());

            _treeView.ImageList = imageList;
            _treeView.ImageKey = "folder";
            _treeView.SelectedImageKey = "folder";
        }

        private static string ResolveMenuIconKey(MenuItem menu)
        {
            string explicitKey = NormalizeIconKey(menu?.IconKey);
            if (!string.IsNullOrEmpty(explicitKey))
                return explicitKey;

            string text = ((menu?.MenuName ?? string.Empty) + " " + (menu?.MenuKey ?? string.Empty)).ToLowerInvariant();

            // --- specific leaf items (checked before broader categories) ---
            if (text.Contains("ถังขยะ") || text.Contains("recycle") || text.Contains("trash") || text.Contains("bin"))
                return "cat_trash";
            if (text.Contains("อนุมัติ") || text.Contains("approval") || text.Contains("approve") || text.Contains("inbox"))
                return "cat_approval";
            if (text.Contains("sync") || text.Contains("ซิงค์") || text.Contains("ข้อมูลพนักงาน"))
                return "cat_sync";
            if (text.Contains("share") || text.Contains("sharing") || text.Contains("แชร์") || text.Contains("manage share"))
                return "cat_share";
            if (text.Contains("จัดการเมนู") || text.Contains("menu management"))
                return "cat_menu";
            if (text.Contains("ตั้งค่า") || text.Contains("config") || text.Contains("setting") || text.Contains("ตั้งค่าระบบ") || text.Contains("configadmin"))
                return "cat_config";
            if (text.Contains("workflow") || text.Contains("wfe") || text.Contains("ตรวจสอบ workflow") || text.Contains("กระบวนการ"))
                return "cat_workflow";
            if (text.Contains("audit") || text.Contains("pdpa") || text.Contains("excel") || text.Contains("รายงาน") || text.Contains("report") || text.Contains("auditlog"))
                return "cat_report";
            if (text.Contains("สิทธิ") || text.Contains("permission") || text.Contains("auth") || text.Contains("authority") || text.Contains("access") || text.Contains("useraccess"))
                return "cat_permission";

            // --- category / module level ---
            if (text.Contains("เอกสาร") || text.Contains("document") || text.Contains("docs") || text.Contains("file") || text.Contains("ไฟล์"))
                return "file_docs";
            if (text.Contains("ผู้ใช้") || text.Contains("user") || text.Contains("ผู้ดูแล") || text.Contains("usermgmt") || text.Contains("usermanage"))
                return "organization";
            if (text.Contains("org") || text.Contains("องค์กร") || text.Contains("โครงสร้าง") || text.Contains("orgunit") || text.Contains("template"))
                return "organization";
            if (text.Contains("knowledge") || text.Contains("kms") || text.Contains("คลังความรู้") || text.Contains("knowledgebase"))
                return "cat_knowledge";
            if (text.Contains("ระบบ") || text.Contains("system") || text.Contains("admin") || text.Contains("support") || text.Contains("เครื่องมือ"))
                return "cat_config";
            if (text.Contains("log") || text.Contains("รายงาน"))
                return "cat_report";
            if (text.Contains("ทั่วไป") || text.Contains("general"))
                return "folder";

            return "folder";
        }

        private static string NormalizeIconKey(string iconKey)
        {
            if (string.IsNullOrWhiteSpace(iconKey))
                return null;

            switch (iconKey.Trim().ToLowerInvariant())
            {
                case "folder":         return "folder";
                case "open_folder":    return "open_folder";
                case "organization":   return "organization";
                case "file_docs":      return "file_docs";
                case "book_red":       return "book_red";
                case "book_orange":    return "book_orange";
                case "book_green":     return "book_green";
                case "book_blue":
                case "book_bule":      return "book_blue";
                case "lightning_bolt": return "lightning_bolt";
                case "cat_report":     return "cat_report";
                case "cat_permission": return "cat_permission";
                case "cat_approval":   return "cat_approval";
                case "cat_trash":      return "cat_trash";
                case "cat_share":      return "cat_share";
                case "cat_config":     return "cat_config";
                case "cat_sync":       return "cat_sync";
                case "cat_workflow":   return "cat_workflow";
                case "cat_menu":       return "cat_menu";
                case "cat_knowledge":  return "cat_knowledge";
                default:               return null;
            }
        }

        private void LoadChildNodes(TreeNode parentNode, int parentMenuItemID)
        {
            var childMenus = _menuManagementController.GetChildMenus(parentMenuItemID, _currentUser);

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

    /// <summary>
    /// Generates 16×16 category icons programmatically using GDI+.
    /// Each icon has a colored background and a white symbol so categories
    /// are immediately recognizable in the menu tree.
    /// </summary>
    internal static class MenuCategoryIcons
    {
        // ── palette ──────────────────────────────────────────────────────────
        private static readonly Color ColReport     = Color.FromArgb(57,  73, 171); // indigo
        private static readonly Color ColPermission = Color.FromArgb(255, 143,  0); // amber
        private static readonly Color ColApproval   = Color.FromArgb(56, 142,  60); // green
        private static readonly Color ColTrash      = Color.FromArgb(97, 97,   97); // grey
        private static readonly Color ColShare      = Color.FromArgb(0,  172, 193); // cyan
        private static readonly Color ColConfig     = Color.FromArgb(78, 52,   46); // brown
        private static readonly Color ColSync       = Color.FromArgb(123, 31, 162); // purple
        private static readonly Color ColWorkflow   = Color.FromArgb(230, 74,  25); // deep-orange
        private static readonly Color ColMenu       = Color.FromArgb(69, 90, 100);  // blue-grey
        private static readonly Color ColKnowledge  = Color.FromArgb(0,  137, 123); // teal

        // ── public factory methods ────────────────────────────────────────────

        /// <summary>รายงาน / Report — 3 vertical bars (bar chart) on indigo</summary>
        public static Bitmap CreateReportIcon()
        {
            return Draw(ColReport, (g, bg) =>
            {
                using (var w = new SolidBrush(Color.White))
                {
                    g.FillRectangle(w, 2,  12, 3, 2); // left  bar (short)
                    g.FillRectangle(w, 7,   8, 3, 6); // mid   bar (medium)
                    g.FillRectangle(w, 12,  4, 3, 10); // right bar (tall)
                    g.FillRectangle(w, 1,  14, 13, 1); // baseline
                }
            });
        }

        /// <summary>สิทธิ์ / Permission — padlock on amber</summary>
        public static Bitmap CreatePermissionIcon()
        {
            return Draw(ColPermission, (g, bg) =>
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.White, 1.8f))
                    g.DrawArc(pen, 4, 1, 8, 8, 180, 180); // shackle arch
                using (var w = new SolidBrush(Color.White))
                    g.FillRectangle(w, 3, 8, 10, 6);      // lock body
                using (var b = new SolidBrush(ColPermission))
                    g.FillEllipse(b, 6, 10, 4, 3);        // keyhole cutout
            });
        }

        /// <summary>อนุมัติ / Approval — checkmark on green</summary>
        public static Bitmap CreateApprovalIcon()
        {
            return Draw(ColApproval, (g, bg) =>
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.White, 2.2f) { LineJoin = LineJoin.Round })
                {
                    g.DrawLine(pen, 2, 9,  6, 13);
                    g.DrawLine(pen, 6, 13, 14, 4);
                }
            });
        }

        /// <summary>ถังขยะ / Trash — trash can on grey</summary>
        public static Bitmap CreateTrashIcon()
        {
            return Draw(ColTrash, (g, bg) =>
            {
                using (var w = new SolidBrush(Color.White))
                {
                    g.FillRectangle(w, 5,  1, 6, 2);  // handle
                    g.FillRectangle(w, 2,  4, 12, 1); // lid
                    g.FillRectangle(w, 3,  6, 10, 8); // body
                }
                using (var b = new SolidBrush(ColTrash))
                {
                    g.FillRectangle(b, 5,  7, 1, 6);  // stripe 1
                    g.FillRectangle(b, 7,  7, 1, 6);  // stripe 2
                    g.FillRectangle(b, 9,  7, 1, 6);  // stripe 3
                }
            });
        }

        /// <summary>Share — 3 dots connected with lines on cyan</summary>
        public static Bitmap CreateShareIcon()
        {
            return Draw(ColShare, (g, bg) =>
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.White, 1.5f))
                {
                    g.DrawLine(pen, 4, 8, 11, 4);
                    g.DrawLine(pen, 4, 8, 11, 12);
                }
                using (var w = new SolidBrush(Color.White))
                {
                    g.FillEllipse(w, 1,  6, 4, 4);   // left dot
                    g.FillEllipse(w, 10, 2, 4, 4);   // top-right dot
                    g.FillEllipse(w, 10, 10, 4, 4);  // bottom-right dot
                }
            });
        }

        /// <summary>ตั้งค่า / Config — gear on brown</summary>
        public static Bitmap CreateConfigIcon()
        {
            return Draw(ColConfig, (g, bg) =>
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var w = new SolidBrush(Color.White))
                {
                    // teeth
                    g.FillRectangle(w, 6,  1, 4, 3);
                    g.FillRectangle(w, 6, 12, 4, 3);
                    g.FillRectangle(w, 1,  6, 3, 4);
                    g.FillRectangle(w, 12, 6, 3, 4);
                    // main circle
                    g.FillEllipse(w, 4, 4, 8, 8);
                }
                // centre hole
                using (var b = new SolidBrush(ColConfig))
                    g.FillEllipse(b, 6, 6, 4, 4);
            });
        }

        /// <summary>Sync — two circular arrows on purple</summary>
        public static Bitmap CreateSyncIcon()
        {
            return Draw(ColSync, (g, bg) =>
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.White, 1.8f))
                {
                    g.DrawArc(pen, 2, 3, 10, 10, 200, 240);
                    g.DrawArc(pen, 4, 3, 10, 10,  20, 240);
                }
                using (var pen = new Pen(Color.White, 1.5f))
                {
                    // arrowhead top-right
                    g.DrawLine(pen, 12, 6, 14, 4);
                    g.DrawLine(pen, 14, 4, 12, 2);
                    // arrowhead bottom-left
                    g.DrawLine(pen, 4, 10, 2, 12);
                    g.DrawLine(pen, 2, 12, 4, 14);
                }
            });
        }

        /// <summary>Workflow / WFE — diamond on deep-orange</summary>
        public static Bitmap CreateWorkflowIcon()
        {
            return Draw(ColWorkflow, (g, bg) =>
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var pts = new[] {
                    new Point(8, 2), new Point(14, 8),
                    new Point(8, 14), new Point(2, 8)
                };
                using (var w = new SolidBrush(Color.White))
                    g.FillPolygon(w, pts);
                // inner diamond (bg colour creates outline effect)
                var inner = new[] {
                    new Point(8, 5), new Point(11, 8),
                    new Point(8, 11), new Point(5, 8)
                };
                using (var b = new SolidBrush(ColWorkflow))
                    g.FillPolygon(b, inner);
            });
        }

        /// <summary>จัดการเมนู / Menu Admin — 2×2 tile grid on blue-grey</summary>
        public static Bitmap CreateMenuAdminIcon()
        {
            return Draw(ColMenu, (g, bg) =>
            {
                using (var w = new SolidBrush(Color.White))
                {
                    g.FillRectangle(w, 2, 2, 5, 5);
                    g.FillRectangle(w, 9, 2, 5, 5);
                    g.FillRectangle(w, 2, 9, 5, 5);
                    g.FillRectangle(w, 9, 9, 5, 5);
                }
            });
        }

        /// <summary>KMS / Knowledge Base — open book on teal</summary>
        public static Bitmap CreateKnowledgeIcon()
        {
            return Draw(ColKnowledge, (g, bg) =>
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.White, 1.5f))
                {
                    // left page
                    g.DrawLine(pen, 8, 4, 3, 4);
                    g.DrawLine(pen, 3, 4, 3, 13);
                    g.DrawLine(pen, 3, 13, 8, 13);
                    // right page
                    g.DrawLine(pen, 8, 4, 13, 4);
                    g.DrawLine(pen, 13, 4, 13, 13);
                    g.DrawLine(pen, 13, 13, 8, 13);
                    // spine
                    g.DrawLine(pen, 8, 4, 8, 13);
                    // text lines on left page
                    g.DrawLine(pen, 4, 7, 7, 7);
                    g.DrawLine(pen, 4, 9, 7, 9);
                    g.DrawLine(pen, 4, 11, 7, 11);
                    // text lines on right page
                    g.DrawLine(pen, 9, 7, 12, 7);
                    g.DrawLine(pen, 9, 9, 12, 9);
                    g.DrawLine(pen, 9, 11, 12, 11);
                }
            });
        }

        // ── shared helper ────────────────────────────────────────────────────

        private static Bitmap Draw(Color bgColor, Action<Graphics, Color> paint)
        {
            var bmp = new Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.Clear(Color.Transparent);
                using (var bg = new SolidBrush(bgColor))
                    g.FillRectangle(bg, 0, 0, 16, 16);
                paint(g, bgColor);
            }
            return bmp;
        }
    }
}
