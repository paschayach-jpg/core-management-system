using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MenuItem = CMS.Domain.Models.MenuItem;

namespace CMS.Presentation
{
    internal sealed class ParentMenuPickerDialog : Form
    {
        public int? SelectedMenuItemID { get; private set; }

        private readonly List<MenuItem> _allMenus;
        private readonly int? _excludeMenuItemID;
        private TreeView _tree;
        private Button _btnOK;
        private Button _btnCancel;

        public ParentMenuPickerDialog(List<MenuItem> allMenus, int? currentParentID, int? excludeMenuItemID = null)
        {
            _allMenus = allMenus ?? new List<MenuItem>();
            _excludeMenuItemID = excludeMenuItemID;
            SelectedMenuItemID = currentParentID;
            BuildForm();
            PopulateTree();
            PreSelectNode(currentParentID);
        }

        private void BuildForm()
        {
            Text = "เลือก Parent Menu";
            Size = new Size(400, 520);
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.FromArgb(236, 242, 250);
            Icon = SystemIcons.Information;

            var accent = Color.FromArgb(18, 67, 122);
            var accentSoft = Color.FromArgb(225, 236, 248);

            var lblTitle = new Label
            {
                Text = "📂 เลือก Parent Menu",
                Dock = DockStyle.Top,
                Height = 36,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = accentSoft,
                ForeColor = accent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            var lblHint = new Label
            {
                Text = "ดับเบิลคลิกเพื่อเลือก หรือคลิกแล้วกด \"✔ เลือก\"",
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 8F),
                BackColor = accentSoft,
                ForeColor = Color.FromArgb(80, 100, 130),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };

            _tree = new TreeView
            {
                Dock = DockStyle.Fill,
                ShowLines = true,
                ShowRootLines = true,
                ShowPlusMinus = true,
                FullRowSelect = true,
                HideSelection = false,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(24, 37, 56)
            };
            _tree.NodeMouseDoubleClick += (s, e) => ConfirmSelection();

            var pnlButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = accentSoft,
                Padding = new Padding(10, 9, 10, 9)
            };

            _btnOK = new Button
            {
                Text = "✔ เลือก",
                Size = new Size(110, 32),
                Location = new Point(10, 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(21, 112, 74),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9F),
                Cursor = Cursors.Hand
            };
            _btnOK.FlatAppearance.BorderSize = 0;
            _btnOK.Click += (s, e) => ConfirmSelection();

            _btnCancel = new Button
            {
                Text = "✖ ยกเลิก",
                Size = new Size(110, 32),
                Location = new Point(128, 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(118, 132, 154),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9F),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            _btnCancel.FlatAppearance.BorderSize = 0;

            var btnNone = new Button
            {
                Text = "🚫 ไม่มี Parent (Root)",
                Size = new Size(158, 32),
                Location = new Point(246, 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(140, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9F),
                Cursor = Cursors.Hand
            };
            btnNone.FlatAppearance.BorderSize = 0;
            btnNone.Click += (s, e) =>
            {
                SelectedMenuItemID = null;
                DialogResult = DialogResult.OK;
                Close();
            };

            pnlButtons.Controls.Add(_btnOK);
            pnlButtons.Controls.Add(_btnCancel);
            pnlButtons.Controls.Add(btnNone);

            Controls.Add(_tree);
            Controls.Add(pnlButtons);
            Controls.Add(lblHint);
            Controls.Add(lblTitle);

            AcceptButton = _btnOK;
            CancelButton = _btnCancel;
        }

        private void PopulateTree()
        {
            _tree.Nodes.Clear();
            var roots = _allMenus
                .Where(m => m.ParentMenuItemID == null && m.MenuItemID != _excludeMenuItemID)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.MenuName);

            foreach (var root in roots)
            {
                var node = MakeNode(root);
                _tree.Nodes.Add(node);
                AddChildren(node, root.MenuItemID);
            }

            _tree.ExpandAll();
        }

        private void AddChildren(TreeNode parentNode, int parentMenuItemID)
        {
            var children = _allMenus
                .Where(m => m.ParentMenuItemID == parentMenuItemID && m.MenuItemID != _excludeMenuItemID)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.MenuName);

            foreach (var child in children)
            {
                var node = MakeNode(child);
                parentNode.Nodes.Add(node);
                AddChildren(node, child.MenuItemID);
            }
        }

        private static TreeNode MakeNode(MenuItem menu)
        {
            string label = menu.IsActive ? menu.MenuName : $"{menu.MenuName} (Inactive)";
            return new TreeNode($"📁 {label}")
            {
                Tag = menu.MenuItemID,
                ForeColor = menu.IsActive ? Color.FromArgb(24, 37, 56) : Color.Gray
            };
        }

        private void PreSelectNode(int? menuID)
        {
            if (!menuID.HasValue) return;
            foreach (TreeNode root in _tree.Nodes)
            {
                var found = FindNode(root, menuID.Value);
                if (found == null) continue;
                _tree.SelectedNode = found;
                found.EnsureVisible();
                return;
            }
        }

        private static TreeNode FindNode(TreeNode node, int id)
        {
            if (node.Tag is int nodeID && nodeID == id) return node;
            foreach (TreeNode child in node.Nodes)
            {
                var result = FindNode(child, id);
                if (result != null) return result;
            }
            return null;
        }

        private void ConfirmSelection()
        {
            if (!(_tree.SelectedNode?.Tag is int selectedID))
            {
                MessageBox.Show("กรุณาคลิกเลือก Menu ที่ต้องการก่อน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SelectedMenuItemID = selectedID;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
