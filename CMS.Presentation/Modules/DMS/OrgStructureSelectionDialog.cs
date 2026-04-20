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

        public List<OrgUnitSelection> SelectedUnits { get; private set; } = new List<OrgUnitSelection>();

        public OrgStructureSelectionDialog(IEnumerable<OrgUnit> allUnits)
        {
            _allUnits = allUnits ?? Enumerable.Empty<OrgUnit>();
            InitializeComponent();
            LoadOrganizationStructure();
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