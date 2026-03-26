using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Modal dialog for selecting an OrgUnit (SubCommittee / Division / Department).
    /// </summary>
    public partial class OrgUnitSelectorDialog : Form
    {
        private readonly IOrgStructureService _service;
        private readonly List<OrgUnit>        _providedUnits;
        private List<OrgUnit>                 _allUnits = new List<OrgUnit>();

        // ── Results ───────────────────────────────────────────────────────────
        public int?    SelectedOrgUnitId   { get; private set; }
        public string  SelectedOrgUnitName { get; private set; }
        public OrgUnit SelectedOrgUnit     { get; private set; }

        // ── Constructor 1: load from service ─────────────────────────────────
        public OrgUnitSelectorDialog(IOrgStructureService service)
        {
            if (service == null) throw new ArgumentNullException("service");
            _service       = service;
            _providedUnits = null;
            InitializeComponent();
        }

        // ── Constructor 2: use provided list ─────────────────────────────────
        public OrgUnitSelectorDialog(List<OrgUnit> units, string title = "เลือกหน่วยงาน")
        {
            if (units == null) throw new ArgumentNullException("units");
            _service       = null;
            _providedUnits = units;
            InitializeComponent();
            this.Text        = title ?? "เลือกหน่วยงาน";
            lblHeader.Text   = title ?? "เลือกหน่วยงาน";
        }

        // ── Form Load ─────────────────────────────────────────────────────────
        private void OrgUnitSelectorDialog_Load(object sender, EventArgs e)
        {
            LoadOrgUnits();
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void btnOK_Click(object sender, EventArgs e)
        {
            var selected = treeView.SelectedNode != null
                ? treeView.SelectedNode.Tag as OrgUnit
                : null;

            if (selected == null)
            {
                MessageBox.Show("กรุณาเลือกหน่วยงาน", "ยังไม่ได้เลือก",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            SelectedOrgUnitId   = selected.OrgUnitId;
            SelectedOrgUnitName = selected.Name;
            SelectedOrgUnit     = selected;
            DialogResult        = DialogResult.OK;
        }

        private void treeView_DoubleClick(object sender, EventArgs e)
        {
            if (treeView.SelectedNode != null && treeView.SelectedNode.Tag is OrgUnit)
                btnOK_Click(sender, e);
        }

        // ── Data loading ──────────────────────────────────────────────────────
        private void LoadOrgUnits()
        {
            try
            {
                if (_providedUnits != null)
                {
                    _allUnits = _providedUnits
                        .Where(u => u.IsActive)
                        .OrderBy(u => u.OrgUnitType)
                        .ThenBy(u => u.Name)
                        .ToList();
                }
                else
                {
                    _allUnits = _service.GetHierarchy()
                        .Where(u => u.IsActive && !u.IsDeleted)
                        .OrderBy(u => u.OrgUnitType)
                        .ThenBy(u => u.SortOrder)
                        .ToList();
                }

                treeView.BeginUpdate();
                treeView.Nodes.Clear();

                bool isFlat = _providedUnits != null
                    && _providedUnits.Count > 0
                    && _providedUnits.All(u => u.ParentId == null || u.ParentId == 0);

                if (isFlat)
                {
                    foreach (var unit in _allUnits)
                        treeView.Nodes.Add(CreateNode(unit));
                }
                else
                {
                    foreach (var sc in _allUnits.Where(u => u.Tier == OrgUnitTier.SubCommittee).OrderBy(u => u.Name))
                    {
                        var scNode = CreateNode(sc);
                        foreach (var div in _allUnits.Where(u => u.Tier == OrgUnitTier.Division && u.ParentId == sc.OrgUnitId).OrderBy(u => u.Name))
                        {
                            var divNode = CreateNode(div);
                            foreach (var dept in _allUnits.Where(u => u.Tier == OrgUnitTier.Department && u.ParentId == div.OrgUnitId).OrderBy(u => u.Name))
                                divNode.Nodes.Add(CreateNode(dept));
                            scNode.Nodes.Add(divNode);
                        }
                        treeView.Nodes.Add(scNode);
                    }
                }

                treeView.ExpandAll();
                treeView.EndUpdate();

                lblStatus.Text      = string.Format("{0} หน่วยงาน", _allUnits.Count);
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
            }
            catch (Exception ex)
            {
                lblStatus.Text      = "โหลดข้อมูลไม่สำเร็จ";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show("ไม่สามารถโหลดข้อมูลหน่วยงาน: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static TreeNode CreateNode(OrgUnit unit)
        {
            string icon = unit.Tier == OrgUnitTier.SubCommittee ? "🏛 "
                        : unit.Tier == OrgUnitTier.Division     ? "🏢 "
                        : "📂 ";
            return new TreeNode(icon + unit.Name) { Tag = unit };
        }
    }
}
