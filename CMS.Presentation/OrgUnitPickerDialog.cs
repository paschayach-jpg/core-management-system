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
    /// Dialog แสดงโครงสร้างองค์กรแบบ TreeView พร้อม Checkbox
    /// ให้เลือกได้หลายหน่วยงาน (Division / Department)
    /// Returns: List of selected OrgUnit with PrincipalType (3=Department, 4=Division)
    /// </summary>
    public partial class OrgUnitPickerDialog : Form
    {
        private readonly IEnumerable<OrgUnit> _allUnits;

        public List<OrgUnitSelection> SelectedUnits { get; private set; } = new List<OrgUnitSelection>();

        public OrgUnitPickerDialog(IEnumerable<OrgUnit> allUnits)
        {
            _allUnits = allUnits ?? Enumerable.Empty<OrgUnit>();
            InitializeComponent();
        }

        private void OrgUnitPickerDialog_Load(object sender, EventArgs e)
        {
            BuildTree();
        }

        private void BuildTree()
        {
            treeOrg.BeginUpdate();
            treeOrg.Nodes.Clear();

            var units = _allUnits.Where(u => u.IsActive && !u.IsDeleted).ToList();

            var subCommittees = units
                .Where(u => u.OrgUnitType == 1)
                .OrderBy(u => u.OrgUnitNameTH ?? u.Name);

            foreach (var sc in subCommittees)
            {
                var scNode = MakeNode(sc, "🏢");
                scNode.ForeColor  = Color.FromArgb(31, 56, 100);
                scNode.NodeFont   = new Font(treeOrg.Font, FontStyle.Bold);

                var divisions = units
                    .Where(u => u.OrgUnitType == 2 && u.ParentOrgUnitID == sc.OrgUnitID)
                    .OrderBy(u => u.OrgUnitNameTH ?? u.Name);

                foreach (var div in divisions)
                {
                    var divNode = MakeNode(div, "📁");
                    divNode.ForeColor = Color.FromArgb(0, 80, 160);

                    var departments = units
                        .Where(u => u.OrgUnitType == 3 && u.ParentOrgUnitID == div.OrgUnitID)
                        .OrderBy(u => u.OrgUnitNameTH ?? u.Name);

                    foreach (var dept in departments)
                    {
                        var deptNode = MakeNode(dept, "📋");
                        deptNode.ForeColor = Color.FromArgb(60, 60, 60);
                        divNode.Nodes.Add(deptNode);
                    }

                    scNode.Nodes.Add(divNode);
                }

                treeOrg.Nodes.Add(scNode);
            }

            treeOrg.ExpandAll();
            treeOrg.EndUpdate();
            UpdateStatus();
        }

        private static TreeNode MakeNode(OrgUnit unit, string icon)
        {
            string tierLabel = unit.OrgUnitType == 1 ? "SubCommittee"
                             : unit.OrgUnitType == 2 ? "Division"
                             : "Department";
            string display = string.Format("{0} {1} [{2}]",
                icon,
                unit.OrgUnitNameTH ?? unit.Name,
                tierLabel);

            return new TreeNode(display)
            {
                Tag      = unit,
                Checked  = false
            };
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void treeOrg_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // Propagate check to children
            if (e.Action == TreeViewAction.Unknown) return;
            SetChildChecks(e.Node, e.Node.Checked);
            UpdateStatus();
        }

        private static void SetChildChecks(TreeNode node, bool check)
        {
            foreach (TreeNode child in node.Nodes)
            {
                child.Checked = check;
                SetChildChecks(child, check);
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            SetAllChecks(treeOrg.Nodes, true);
            UpdateStatus();
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            SetAllChecks(treeOrg.Nodes, false);
            UpdateStatus();
        }

        private static void SetAllChecks(TreeNodeCollection nodes, bool check)
        {
            foreach (TreeNode n in nodes)
            {
                n.Checked = check;
                SetAllChecks(n.Nodes, check);
            }
        }

        private void UpdateStatus()
        {
            int count = CountChecked(treeOrg.Nodes);
            lblStatus.Text = string.Format("เลือกแล้ว {0} หน่วยงาน", count);
            btnOk.Enabled  = count > 0;
        }

        private static int CountChecked(TreeNodeCollection nodes)
        {
            int c = 0;
            foreach (TreeNode n in nodes)
            {
                var unit = n.Tag as OrgUnit;
                if (n.Checked && unit != null && unit.OrgUnitType != 1) // ไม่นับ SubCommittee
                    c++;
                c += CountChecked(n.Nodes);
            }
            return c;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SelectedUnits = CollectChecked(treeOrg.Nodes);
            if (SelectedUnits.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกอย่างน้อย 1 หน่วยงาน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private static List<OrgUnitSelection> CollectChecked(TreeNodeCollection nodes)
        {
            var result = new List<OrgUnitSelection>();
            foreach (TreeNode n in nodes)
            {
                var unit = n.Tag as OrgUnit;
                if (n.Checked && unit != null && unit.OrgUnitType != 1)
                {
                    result.Add(new OrgUnitSelection
                    {
                        OrgUnit       = unit,
                        PrincipalType = unit.OrgUnitType == 2 ? 4 : 3  // 4=Division, 3=Department
                    });
                }
                result.AddRange(CollectChecked(n.Nodes));
            }
            return result;
        }

        /// <summary>ข้อมูลหน่วยงานที่เลือก พร้อม PrincipalType สำหรับ FolderPermission</summary>
        public class OrgUnitSelection
        {
            public OrgUnit OrgUnit       { get; set; }
            public int     PrincipalType { get; set; }  // 3=Department, 4=Division
            public string  DisplayName
            {
                get
                {
                    string tier = PrincipalType == 4 ? "ฝ่าย" : "แผนก";
                    return string.Format("[{0}] {1}", tier, OrgUnit.OrgUnitNameTH ?? OrgUnit.Name);
                }
            }
        }
    }
}
