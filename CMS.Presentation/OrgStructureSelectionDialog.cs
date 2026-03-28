using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Enums;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog สำหรับเลือกหน่วยงานจากโครงสร้างองค์กร
    /// แสดงในรูปแบบ TreeView พร้อม Checkbox
    /// </summary>
    public partial class OrgStructureSelectionDialog : Form
    {
        private readonly IEnumerable<OrgUnit> _allUnits;
        private TreeView _treeView;
        private Button _btnSelectAll;
        private Button _btnDeselectAll;
        private Button _btnOk;
        private Button _btnCancel;
        private Label _lblTitle;
        private Label _lblSelectedCount;

        public List<OrgUnitSelection> SelectedUnits { get; private set; } = new List<OrgUnitSelection>();

        public OrgStructureSelectionDialog(IEnumerable<OrgUnit> allUnits)
        {
            _allUnits = allUnits ?? Enumerable.Empty<OrgUnit>();
            InitializeComponent();
            LoadOrganizationStructure();
        }

        private void InitializeComponent()
        {
            this.Text = "เลือกหน่วยงาน";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // Header
            var pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(600, 50),
                BackColor = Color.FromArgb(31, 56, 100),
                Dock = DockStyle.Top
            };

            _lblTitle = new Label
            {
                Text = "🏢 เลือกหน่วยงาน (เลือกได้หลายรายการ)",
                Location = new Point(15, 0),
                Size = new Size(500, 50),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            pnlHeader.Controls.Add(_lblTitle);
            this.Controls.Add(pnlHeader);

            // Selection buttons
            var pnlButtons = new Panel
            {
                Location = new Point(15, 60),
                Size = new Size(570, 35),
                BackColor = Color.FromArgb(248, 249, 252)
            };

            _btnSelectAll = new Button
            {
                Text = "✓ เลือกทั้งหมด",
                Location = new Point(5, 5),
                Size = new Size(120, 25),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            _btnSelectAll.FlatAppearance.BorderSize = 0;
            _btnSelectAll.Click += BtnSelectAll_Click;

            _btnDeselectAll = new Button
            {
                Text = "✗ ล้างทั้งหมด",
                Location = new Point(130, 5),
                Size = new Size(120, 25),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            _btnDeselectAll.FlatAppearance.BorderSize = 0;
            _btnDeselectAll.Click += BtnDeselectAll_Click;

            _lblSelectedCount = new Label
            {
                Text = "เลือกแล้ว: 0 รายการ",
                Location = new Point(260, 8),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(108, 117, 125)
            };

            pnlButtons.Controls.Add(_btnSelectAll);
            pnlButtons.Controls.Add(_btnDeselectAll);
            pnlButtons.Controls.Add(_lblSelectedCount);
            this.Controls.Add(pnlButtons);

            // TreeView
            _treeView = new TreeView
            {
                Location = new Point(15, 105),
                Size = new Size(570, 300),
                CheckBoxes = true,
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ShowLines = true,
                ShowPlusMinus = true,
                ShowRootLines = true,
                FullRowSelect = true,
                HideSelection = false
            };
            _treeView.AfterCheck += TreeView_AfterCheck;
            this.Controls.Add(_treeView);

            // Footer buttons
            var pnlFooter = new Panel
            {
                Location = new Point(0, 415),
                Size = new Size(600, 55),
                BackColor = Color.FromArgb(248, 250, 255),
                Dock = DockStyle.Bottom
            };

            _btnOk = new Button
            {
                Text = "ตกลง",
                Location = new Point(430, 10),
                Size = new Size(80, 35),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            _btnOk.FlatAppearance.BorderSize = 0;
            _btnOk.Click += BtnOk_Click;

            _btnCancel = new Button
            {
                Text = "ยกเลิก",
                Location = new Point(520, 10),
                Size = new Size(70, 35),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                DialogResult = DialogResult.Cancel
            };
            _btnCancel.FlatAppearance.BorderSize = 0;

            pnlFooter.Controls.Add(_btnOk);
            pnlFooter.Controls.Add(_btnCancel);
            this.Controls.Add(pnlFooter);

            this.AcceptButton = _btnOk;
            this.CancelButton = _btnCancel;
        }

        private void LoadOrganizationStructure()
        {
            _treeView.Nodes.Clear();

            var activeUnits = _allUnits.Where(u => u.IsActive && !u.IsDeleted).ToList();

            // Group by hierarchy: SubCommittee -> Division -> Department
            var subCommittees = activeUnits.Where(u => u.Tier == OrgUnitTier.SubCommittee).OrderBy(u => u.Name);

            foreach (var subCommittee in subCommittees)
            {
                var scNode = new TreeNode($"🏢 {subCommittee.Name} [SubCommittee]")
                {
                    Tag = new OrgUnitSelection
                    {
                        OrgUnit = subCommittee,
                        PrincipalType = (int)PrincipalType.Department, // SubCommittee maps to Department type
                        DisplayName = $"{subCommittee.Name} [SubCommittee]"
                    },
                    ForeColor = Color.FromArgb(31, 56, 100),
                    NodeFont = new Font(_treeView.Font, FontStyle.Bold)
                };

                // Add Divisions under this SubCommittee
                var divisions = activeUnits.Where(u => u.Tier == OrgUnitTier.Division && u.ParentId == subCommittee.OrgUnitId).OrderBy(u => u.Name);
                foreach (var division in divisions)
                {
                    var divNode = new TreeNode($"📁 {division.Name} [Division]")
                    {
                        Tag = new OrgUnitSelection
                        {
                            OrgUnit = division,
                            PrincipalType = (int)PrincipalType.Division,
                            DisplayName = $"{division.Name} [Division]"
                        },
                        ForeColor = Color.FromArgb(0, 80, 160)
                    };

                    // Add Departments under this Division
                    var departments = activeUnits.Where(u => u.Tier == OrgUnitTier.Department && u.ParentId == division.OrgUnitId).OrderBy(u => u.Name);
                    foreach (var department in departments)
                    {
                        var deptNode = new TreeNode($"📋 {department.Name} [Department]")
                        {
                            Tag = new OrgUnitSelection
                            {
                                OrgUnit = department,
                                PrincipalType = (int)PrincipalType.Department,
                                DisplayName = $"{department.Name} [Department]"
                            },
                            ForeColor = Color.FromArgb(60, 60, 60)
                        };
                        divNode.Nodes.Add(deptNode);
                    }

                    scNode.Nodes.Add(divNode);
                }

                _treeView.Nodes.Add(scNode);
            }

            // Add independent Divisions (no parent SubCommittee)
            var independentDivisions = activeUnits.Where(u => u.Tier == OrgUnitTier.Division && !u.ParentId.HasValue).OrderBy(u => u.Name);
            foreach (var division in independentDivisions)
            {
                var divNode = new TreeNode($"📁 {division.Name} [Division]")
                {
                    Tag = new OrgUnitSelection
                    {
                        OrgUnit = division,
                        PrincipalType = (int)PrincipalType.Division,
                        DisplayName = $"{division.Name} [Division]"
                    },
                    ForeColor = Color.FromArgb(0, 80, 160)
                };

                // Add Departments under this Division
                var departments = activeUnits.Where(u => u.Tier == OrgUnitTier.Department && u.ParentId == division.OrgUnitId).OrderBy(u => u.Name);
                foreach (var department in departments)
                {
                    var deptNode = new TreeNode($"📋 {department.Name} [Department]")
                    {
                        Tag = new OrgUnitSelection
                        {
                            OrgUnit = department,
                            PrincipalType = (int)PrincipalType.Department,
                            DisplayName = $"{department.Name} [Department]"
                        },
                        ForeColor = Color.FromArgb(60, 60, 60)
                    };
                    divNode.Nodes.Add(deptNode);
                }

                _treeView.Nodes.Add(divNode);
            }

            _treeView.ExpandAll();
            UpdateSelectedCount();
        }

        private void TreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // Prevent recursive calls
            _treeView.AfterCheck -= TreeView_AfterCheck;

            try
            {
                // If checking a parent, check all children
                if (e.Node.Checked)
                {
                    CheckAllChildren(e.Node, true);
                }
                else
                {
                    CheckAllChildren(e.Node, false);
                }

                // Update parent nodes based on children
                UpdateParentCheckState(e.Node);
            }
            finally
            {
                _treeView.AfterCheck += TreeView_AfterCheck;
                UpdateSelectedCount();
            }
        }

        private void CheckAllChildren(TreeNode node, bool isChecked)
        {
            foreach (TreeNode child in node.Nodes)
            {
                child.Checked = isChecked;
                CheckAllChildren(child, isChecked);
            }
        }

        private void UpdateParentCheckState(TreeNode node)
        {
            if (node.Parent != null)
            {
                bool allChecked = true;

                foreach (TreeNode sibling in node.Parent.Nodes)
                {
                    if (!sibling.Checked)
                        allChecked = false;
                }

                node.Parent.Checked = allChecked;
                UpdateParentCheckState(node.Parent);
            }
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            _treeView.AfterCheck -= TreeView_AfterCheck;
            try
            {
                CheckAllNodes(_treeView.Nodes, true);
            }
            finally
            {
                _treeView.AfterCheck += TreeView_AfterCheck;
                UpdateSelectedCount();
            }
        }

        private void BtnDeselectAll_Click(object sender, EventArgs e)
        {
            _treeView.AfterCheck -= TreeView_AfterCheck;
            try
            {
                CheckAllNodes(_treeView.Nodes, false);
            }
            finally
            {
                _treeView.AfterCheck += TreeView_AfterCheck;
                UpdateSelectedCount();
            }
        }

        private void CheckAllNodes(TreeNodeCollection nodes, bool isChecked)
        {
            foreach (TreeNode node in nodes)
            {
                node.Checked = isChecked;
                CheckAllNodes(node.Nodes, isChecked);
            }
        }

        private void UpdateSelectedCount()
        {
            int count = CountCheckedNodes(_treeView.Nodes);
            _lblSelectedCount.Text = $"เลือกแล้ว: {count} รายการ";
            _lblSelectedCount.ForeColor = count > 0 ? Color.FromArgb(40, 167, 69) : Color.FromArgb(108, 117, 125);
        }

        private int CountCheckedNodes(TreeNodeCollection nodes)
        {
            int count = 0;
            foreach (TreeNode node in nodes)
            {
                if (node.Checked && node.Tag is OrgUnitSelection)
                    count++;
                count += CountCheckedNodes(node.Nodes);
            }
            return count;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            SelectedUnits.Clear();
            CollectSelectedUnits(_treeView.Nodes);

            if (SelectedUnits.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกหน่วยงานอย่างน้อย 1 รายการ", "แจ้งเตือน", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CollectSelectedUnits(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked && node.Tag is OrgUnitSelection selection)
                {
                    SelectedUnits.Add(selection);
                }
                CollectSelectedUnits(node.Nodes);
            }
        }
    }

    /// <summary>
    /// Represents a selected organizational unit with its type information
    /// </summary>
    public class OrgUnitSelection
    {
        public OrgUnit OrgUnit { get; set; }
        public int PrincipalType { get; set; }
        public string DisplayName { get; set; }
    }
}