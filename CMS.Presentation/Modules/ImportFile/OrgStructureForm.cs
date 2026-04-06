using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Data.Interfaces;
using CMS.Presentation.Presenters;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// MOD-001: Organizational Structure Form.
    /// Displays OrgUnits in Organization View (hierarchy) or Confidentiality View (by level).
    /// Switching views reloads only the data panel — no full form reload (Req 3.8, 3.9).
    /// Wires Create / Edit / Deactivate actions to OrgStructurePresenter (Req 3.2–3.7).
    /// </summary>
    public partial class OrgStructureForm : Form, IOrgStructureView
    {
        private readonly OrgStructurePresenter  _presenter;
        private readonly IOrgStructureService   _service;
        private readonly UserContext            _currentUser;
        private List<OrgUnit> _allUnits = new List<OrgUnit>();
        private readonly IEmployeeApiService    _employeeApiService;
        private readonly IHrDivisionRepository   _hrDivisionRepo;
        private readonly IHrDepartmentRepository _hrDepartmentRepo;

        // ตัวเลือกมุมมองปัจจุบัน: false = ตามองค์กร, true = ตามความลับ
        private bool _isConfView = false;

        public OrgStructureForm(IOrgStructureService service, UserContext user,
            IEmployeeApiService employeeApiService = null,
            IHrDivisionRepository hrDivisionRepo = null,
            IHrDepartmentRepository hrDepartmentRepo = null)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (user    == null) throw new ArgumentNullException("user");

            _service            = service;
            _currentUser        = user;
            _employeeApiService = employeeApiService;
            _hrDivisionRepo     = hrDivisionRepo;
            _hrDepartmentRepo   = hrDepartmentRepo;

            InitializeComponent();
            _presenter = new OrgStructurePresenter(service, this, user);
            ApplyViewButtonStyle(orgSelected: true);
        }

        // -----------------------------------------------------------------------
        // Form load
        // -----------------------------------------------------------------------

        private void OrgStructureForm_Load(object sender, EventArgs e)
        {
            // Set up folder open/closed icons from Resources
            var folderImageList = new ImageList { ImageSize = new Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };
            folderImageList.Images.Add("folder",      Properties.Resources.folder);
            folderImageList.Images.Add("open_folder", Properties.Resources.open_folder);
            treeView.ImageList        = folderImageList;
            treeView.ImageKey         = "folder";
            treeView.SelectedImageKey = "open_folder";
            treeView.BeforeExpand  += (s, ev) => { ev.Node.ImageKey = ev.Node.SelectedImageKey = "open_folder"; };
            treeView.BeforeCollapse += (s, ev) => { ev.Node.ImageKey = ev.Node.SelectedImageKey = "folder"; };

            // ── Admin-only: ปุ่มแก้ไขโครงสร้างเปิดได้เฉพาะ Admin (AppRole == 4) ──
            bool isAdmin = _currentUser != null && _currentUser.AppRole == 4;
            btnCreate.Enabled     = isAdmin;
            btnEdit.Enabled       = isAdmin;
            btnMove.Enabled       = isAdmin;
            btnDelete.Enabled     = isAdmin;
            btnDeactivate.Enabled = isAdmin;

            if (!isAdmin)
            {
                btnCreate.ToolTipText     = "เฉพาะ Admin เท่านั้น";
                btnEdit.ToolTipText       = "เฉพาะ Admin เท่านั้น";
                btnMove.ToolTipText       = "เฉพาะ Admin เท่านั้น";
                btnDelete.ToolTipText     = "เฉพาะ Admin เท่านั้น";
                btnDeactivate.ToolTipText = "เฉพาะ Admin เท่านั้น";
            }

            // placeholder สี
            txtSearch.ForeColor = Color.Gray;

            _presenter.SetActiveView(false);
            _presenter.LoadOrganizationView();
        }

        // -----------------------------------------------------------------------
        // View-switch handlers (Req 3.9 — reload data panel only)
        // -----------------------------------------------------------------------

        private void btnViewOrg_Click(object sender, EventArgs e)
        {
            if (_isConfView == false) return;   // already selected
            _isConfView = false;
            ApplyViewButtonStyle(orgSelected: true);
            _presenter.SetActiveView(false);
            _presenter.LoadOrganizationView();
        }

        private void btnViewConf_Click(object sender, EventArgs e)
        {
            if (_isConfView == true) return;    // already selected
            _isConfView = true;
            ApplyViewButtonStyle(orgSelected: false);
            _presenter.SetActiveView(true);
            _presenter.LoadConfidentialityView();
        }

        /// <summary>ปรับสีปุ่ม view ให้แสดง selected / unselected</summary>
        private void ApplyViewButtonStyle(bool orgSelected)
        {
            // ─ Selected: สีน้ำเงินเข้ม พร้อมไฮไลต์ ─────────────────────────
            var selBack  = System.Drawing.Color.FromArgb(0, 51, 102);
            var selFore  = System.Drawing.Color.White;
            var selFont  = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

            // ─ Normal: ขาว ─────────────────────────────────────────────────
            var normBack = System.Drawing.Color.White;
            var normFore = System.Drawing.Color.FromArgb(60, 60, 60);
            var normFont = new System.Drawing.Font("Segoe UI", 9f);

            if (orgSelected)
            {
                btnViewOrg .BackColor = selBack;  btnViewOrg .ForeColor = selFore;  btnViewOrg .Font = selFont;
                btnViewConf.BackColor = normBack; btnViewConf.ForeColor = normFore; btnViewConf.Font = normFont;
            }
            else
            {
                btnViewConf.BackColor = selBack;  btnViewConf.ForeColor = selFore;  btnViewConf.Font = selFont;
                btnViewOrg .BackColor = normBack; btnViewOrg .ForeColor = normFore; btnViewOrg .Font = normFont;
            }

            btnViewOrg .FlatAppearance.BorderColor = orgSelected
                ? System.Drawing.Color.FromArgb(0, 84, 166)
                : System.Drawing.Color.FromArgb(200, 200, 200);
            btnViewConf.FlatAppearance.BorderColor = !orgSelected
                ? System.Drawing.Color.FromArgb(0, 84, 166)
                : System.Drawing.Color.FromArgb(200, 200, 200);
        }

        // -----------------------------------------------------------------------
        // IOrgStructureView — ShowOrganizationView  (Req 3.8)
        // -----------------------------------------------------------------------

        public void ShowOrganizationView(IEnumerable<OrgUnit> units)
        {
            _allUnits = units == null ? new List<OrgUnit>() : units.ToList();

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"ShowOrganizationView: Processing {_allUnits.Count} units");
            var activeUnits = _allUnits.Where(u => u.IsActive).ToList();
            System.Diagnostics.Debug.WriteLine($"Active units: {activeUnits.Count}");
            
            foreach (var unit in activeUnits.OrderBy(u => u.OrgUnitType).ThenBy(u => u.Name))
            {
                System.Diagnostics.Debug.WriteLine($"  {unit.OrgUnitCode}: {unit.Name} (Type: {unit.OrgUnitType}, Parent: {unit.ParentId})");
            }

            treeView.BeginUpdate();
            treeView.Nodes.Clear();

            // Build SubCommittee → Division → Department hierarchy
            var subCommittees = _allUnits
                .Where(u => u.Tier == OrgUnitTier.SubCommittee && u.IsActive)
                .OrderBy(u => u.Name);

            System.Diagnostics.Debug.WriteLine($"Found {subCommittees.Count()} active SubCommittees");

            foreach (var sc in subCommittees)
            {
                var scNode = CreateNode(sc);

                // Get ALL divisions that belong to this SubCommittee (both with and without parent relationship)
                var directDivisions = _allUnits
                    .Where(u => u.Tier == OrgUnitTier.Division && u.IsActive && u.ParentId == sc.OrgUnitId)
                    .OrderBy(u => u.Name);

                System.Diagnostics.Debug.WriteLine($"SubCommittee {sc.Name} has {directDivisions.Count()} direct divisions");

                foreach (var div in directDivisions)
                {
                    var divNode = CreateNode(div);

                    var departments = _allUnits
                        .Where(u => u.Tier == OrgUnitTier.Department && u.IsActive && u.ParentId == div.OrgUnitId)
                        .OrderBy(u => u.Name);

                    foreach (var dept in departments)
                        divNode.Nodes.Add(CreateNode(dept));

                    scNode.Nodes.Add(divNode);
                }

                treeView.Nodes.Add(scNode);
            }

            // Show standalone Divisions (without any parent) as a separate group
            var standaloneDivisions = _allUnits
                .Where(u => u.Tier == OrgUnitTier.Division && u.IsActive && u.ParentId == null)
                .OrderBy(u => u.Name);

            System.Diagnostics.Debug.WriteLine($"Found {standaloneDivisions.Count()} standalone divisions");

            foreach (var div in standaloneDivisions)
            {
                var divNode = CreateNode(div);
                divNode.Text = "🔗 " + divNode.Text; // Add link icon to show it's flexible
                divNode.ForeColor = Color.FromArgb(0, 100, 0); // Dark green
                divNode.NodeFont = new Font(SystemFonts.DefaultFont, FontStyle.Bold);

                var departments = _allUnits
                    .Where(u => u.Tier == OrgUnitTier.Department && u.IsActive && u.ParentId == div.OrgUnitId)
                    .OrderBy(u => u.Name);

                foreach (var dept in departments)
                    divNode.Nodes.Add(CreateNode(dept));

                treeView.Nodes.Add(divNode); // Add directly to root, not under a group
            }

            treeView.ExpandAll();
            treeView.EndUpdate();
            
            System.Diagnostics.Debug.WriteLine($"Tree updated with {treeView.Nodes.Count} root nodes");
        }

        // -----------------------------------------------------------------------
        // IOrgStructureView — ShowConfidentialityView  (Req 3.8)
        // -----------------------------------------------------------------------

        public void ShowConfidentialityView(IEnumerable<OrgUnit> units)
        {
            _allUnits = units == null ? new List<OrgUnit>() : units.ToList();

            treeView.BeginUpdate();
            treeView.Nodes.Clear();

            var levels = new[]
            {
                ConfidentialityLevel.HC,
                ConfidentialityLevel.SC,
                ConfidentialityLevel.NC,
                ConfidentialityLevel.PC
            };

            foreach (var level in levels)
            {
                var grouped = _allUnits
                    .Where(u => u.ConfidentialityLevelValue == (int)level)
                    .OrderBy(u => u.Tier)
                    .ThenBy(u => u.Name)
                    .ToList();

                string header = string.Format("{0}  ({1} หน่วยงาน)",
                    ConfidentialityLevelLabel(level), grouped.Count);

                var groupNode = new TreeNode(header)
                {
                    ForeColor = ConfidentialityLevelColor(level),
                    NodeFont  = new Font(treeView.Font, FontStyle.Bold),
                    Tag       = null
                };

                if (grouped.Count == 0)
                {
                    groupNode.Nodes.Add(new TreeNode("(ไม่มีหน่วยงานในระดับนี้)")
                    {
                        ForeColor = Color.Gray,
                        NodeFont  = new Font(treeView.Font, FontStyle.Italic)
                    });
                }
                else
                {
                    // SubCommittee → Division → Department
                    foreach (var sc in grouped.Where(u => u.Tier == OrgUnitTier.SubCommittee))
                    {
                        var scNode = CreateConfNode(sc);
                        foreach (var div in grouped.Where(u => u.Tier == OrgUnitTier.Division && u.ParentId == sc.OrgUnitId))
                        {
                            var divNode = CreateConfNode(div);
                            foreach (var dept in grouped.Where(u => u.Tier == OrgUnitTier.Department && u.ParentId == div.OrgUnitId))
                                divNode.Nodes.Add(CreateConfNode(dept));
                            scNode.Nodes.Add(divNode);
                        }
                        groupNode.Nodes.Add(scNode);
                    }

                    // Division ที่ไม่มี SubCommittee parent
                    foreach (var div in grouped.Where(u => u.Tier == OrgUnitTier.Division && u.ParentId == null))
                    {
                        var divNode = CreateConfNode(div);
                        foreach (var dept in grouped.Where(u => u.Tier == OrgUnitTier.Department && u.ParentId == div.OrgUnitId))
                            divNode.Nodes.Add(CreateConfNode(dept));
                        groupNode.Nodes.Add(divNode);
                    }
                }

                treeView.Nodes.Add(groupNode);
            }

            treeView.ExpandAll();
            treeView.EndUpdate();
        }

        /// <summary>สร้าง TreeNode สำหรับ Confidentiality view พร้อม tier badge</summary>
        private static TreeNode CreateConfNode(OrgUnit unit)
        {
            string tierBadge = unit.Tier == OrgUnitTier.SubCommittee ? "🏢"
                             : unit.Tier == OrgUnitTier.Division      ? "🏬"
                             : "🏪";

            string label = string.Format("{0} {1}  [{2}]",
                tierBadge, unit.Name, unit.OrgUnitCode ?? "-");

            return new TreeNode(label)
            {
                Tag              = unit,
                ForeColor        = unit.IsActive ? SystemColors.WindowText : SystemColors.GrayText,
                ImageKey         = "folder",
                SelectedImageKey = "open_folder"
            };
        }

        // -----------------------------------------------------------------------
        // IOrgStructureView — messages
        // -----------------------------------------------------------------------

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // -----------------------------------------------------------------------
        // IOrgStructureView — SelectedOrgUnit
        // -----------------------------------------------------------------------

        public OrgUnit SelectedOrgUnit
        {
            get
            {
                if (treeView.SelectedNode == null) return null;
                return treeView.SelectedNode.Tag as OrgUnit;
            }
        }

        // -----------------------------------------------------------------------
        // Toolbar button handlers
        // -----------------------------------------------------------------------

        private void btnCreate_Click(object sender, EventArgs e)
        {
            // Step 1: ตรวจสอบ selected node เพื่อกำหนด context
            TreeNode selectedNode = treeView.SelectedNode;
            OrgUnit selectedOrgUnit = null;
            
            if (selectedNode?.Tag is OrgUnit unit)
            {
                selectedOrgUnit = unit;
            }

            // Step 2: เลือกประเภทหน่วยงานที่ต้องการสร้าง (ตาม context ของ parent)
            OrgUnitTier? selectedTier = PromptForTier(selectedOrgUnit);
            if (!selectedTier.HasValue) return;

            // Step 3: ตรวจสอบว่ามี parent ที่จำเป็นหรือไม่ (เฉพาะ Department เท่านั้น)
            if (selectedTier.Value == OrgUnitTier.Department)
            {
                var divisions = _allUnits.Where(u => u.Tier == OrgUnitTier.Division && u.IsActive).ToList();
                if (divisions.Count == 0)
                {
                    ShowError("ยังไม่มีฝ่ายในระบบ\nกรุณาสร้างฝ่ายก่อนสร้างแผนก");
                    return;
                }
            }

            // Step 4: เปิด dialog สร้างหน่วยงาน พร้อมส่ง context ของ selected node
            using (var dlg = new CreateOrgUnitDialog(_allUnits, _employeeApiService, null, _service, selectedOrgUnit, _hrDivisionRepo, _hrDepartmentRepo))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                    _presenter.CreateOrgUnit(dlg.Result);
            }
        }

        /// <summary>แสดง dialog เลือกประเภทหน่วยงาน</summary>
        private OrgUnitTier? PromptForTier(OrgUnit selectedParent = null)
        {
            using (var dlg = new Form())
            {
                dlg.Text            = "เลือกประเภทหน่วยงาน";
                dlg.ClientSize      = new Size(340, 220);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.BackColor       = Color.White;

                // Header
                var pnlHdr = new Panel
                {
                    Dock      = DockStyle.Top,
                    Height    = 44,
                    BackColor = Color.FromArgb(0, 51, 102)
                };
                
                string headerText = "เลือกประเภทหน่วยงานที่ต้องการสร้าง";
                if (selectedParent != null)
                {
                    headerText = $"สร้างหน่วยงานใต้: {selectedParent.Name}";
                }
                
                pnlHdr.Controls.Add(new Label
                {
                    Text      = headerText,
                    Dock      = DockStyle.Fill,
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(12, 0, 0, 0)
                });
                dlg.Controls.Add(pnlHdr);

                OrgUnitTier result = OrgUnitTier.SubCommittee;
                int yPos = 60;

                // กำหนดตัวเลือกตาม parent type
                List<OrgUnitTier> availableTiers = new List<OrgUnitTier>();
                
                if (selectedParent == null)
                {
                    // ไม่ได้เลือก parent - แสดงทุกตัวเลือก
                    availableTiers.Add(OrgUnitTier.SubCommittee);
                    availableTiers.Add(OrgUnitTier.Division);
                    availableTiers.Add(OrgUnitTier.Department);
                }
                else if (selectedParent.Tier == OrgUnitTier.SubCommittee)
                {
                    // Parent เป็น SubCommittee - สร้างได้แค่ Division
                    availableTiers.Add(OrgUnitTier.Division);
                    result = OrgUnitTier.Division; // Set default
                }
                else if (selectedParent.Tier == OrgUnitTier.Division)
                {
                    // Parent เป็น Division - สร้างได้ทั้ง SubCommittee และ Department
                    availableTiers.Add(OrgUnitTier.SubCommittee);
                    availableTiers.Add(OrgUnitTier.Department);
                    result = OrgUnitTier.Department; // Set default เป็น Department
                }
                else
                {
                    // Parent เป็น Department - ไม่สามารถสร้างอะไรใต้ได้
                    ShowError("ไม่สามารถสร้างหน่วยงานใต้แผนกได้");
                    return null;
                }

                // สร้างปุ่มตามตัวเลือกที่มี
                foreach (var tier in availableTiers)
                {
                    Button btn = null;
                    
                    switch (tier)
                    {
                        case OrgUnitTier.SubCommittee:
                            btn = new Button
                            {
                                Text      = "🏢  SubCommittee  (ระดับสูงสุด)",
                                Location  = new Point(20, yPos),
                                Size      = new Size(300, 38),
                                FlatStyle = FlatStyle.Flat,
                                Font      = new Font("Segoe UI", 10f),
                                BackColor = Color.FromArgb(232, 240, 254),
                                ForeColor = Color.FromArgb(0, 51, 102),
                                TextAlign = ContentAlignment.MiddleLeft,
                                Padding   = new Padding(8, 0, 0, 0),
                                Cursor    = Cursors.Hand
                            };
                            btn.FlatAppearance.BorderColor = Color.FromArgb(0, 51, 102);
                            btn.Click += (s, ev) => { result = OrgUnitTier.SubCommittee; dlg.DialogResult = DialogResult.OK; };
                            break;
                            
                        case OrgUnitTier.Division:
                            btn = new Button
                            {
                                Text      = "🏬  ฝ่าย  (ยืดหยุ่น - สามารถสังกัด SubCommittee หรือเป็นอิสระ)",
                                Location  = new Point(20, yPos),
                                Size      = new Size(300, 38),
                                FlatStyle = FlatStyle.Flat,
                                Font      = new Font("Segoe UI", 10f),
                                BackColor = Color.FromArgb(232, 248, 240),
                                ForeColor = Color.FromArgb(0, 80, 40),
                                TextAlign = ContentAlignment.MiddleLeft,
                                Padding   = new Padding(8, 0, 0, 0),
                                Cursor    = Cursors.Hand
                            };
                            btn.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 60);
                            btn.Click += (s, ev) => { result = OrgUnitTier.Division; dlg.DialogResult = DialogResult.OK; };
                            break;
                            
                        case OrgUnitTier.Department:
                            btn = new Button
                            {
                                Text      = "🏪  แผนก  (สังกัดฝ่าย)",
                                Location  = new Point(20, yPos),
                                Size      = new Size(300, 38),
                                FlatStyle = FlatStyle.Flat,
                                Font      = new Font("Segoe UI", 10f),
                                BackColor = Color.FromArgb(255, 248, 230),
                                ForeColor = Color.FromArgb(120, 70, 0),
                                TextAlign = ContentAlignment.MiddleLeft,
                                Padding   = new Padding(8, 0, 0, 0),
                                Cursor    = Cursors.Hand
                            };
                            btn.FlatAppearance.BorderColor = Color.FromArgb(180, 100, 0);
                            btn.Click += (s, ev) => { result = OrgUnitTier.Department; dlg.DialogResult = DialogResult.OK; };
                            break;
                    }
                    
                    if (btn != null)
                    {
                        dlg.Controls.Add(btn);
                        yPos += 46;
                    }
                }

                // ปรับขนาด dialog ตามจำนวนปุ่ม
                dlg.ClientSize = new Size(340, Math.Max(160, yPos + 20));
                dlg.CancelButton = null;

                return dlg.ShowDialog(this) == DialogResult.OK ? (OrgUnitTier?)result : null;
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var selected = SelectedOrgUnit;
            if (selected == null)
            {
                ShowError("Please select an OrgUnit to edit.");
                return;
            }

            string newName = PromptForName("Edit OrgUnit", "New name:", selected.Name);
            if (newName == null) return; // cancelled

            _presenter.UpdateOrgUnit(selected.OrgUnitId, newName);
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            var selected = SelectedOrgUnit;
            if (selected == null)
            {
                ShowError("Please select an OrgUnit to move.");
                return;
            }

            // Get valid parent candidates (units that can be parents for this unit's tier)
            var validParents = GetValidParentCandidates(selected);
            if (validParents.Count == 0)
            {
                ShowError("No valid parent units available for this OrgUnit.");
                return;
            }

            using (var dlg = new OrgUnitSelectorDialog(validParents, "Select New Parent"))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var newParent = dlg.SelectedOrgUnit;
                    _presenter.MoveOrgUnit(selected.OrgUnitId, newParent.OrgUnitId);
                }
            }
        }

        private List<OrgUnit> GetValidParentCandidates(OrgUnit unit)
        {
            // Determine valid parent tier based on current unit's tier
            OrgUnitTier? validParentTier = null;
            switch (unit.Tier)
            {
                case OrgUnitTier.Division:
                    validParentTier = OrgUnitTier.SubCommittee;
                    break;
                case OrgUnitTier.Department:
                    validParentTier = OrgUnitTier.Division;
                    break;
            }

            if (!validParentTier.HasValue)
                return new List<OrgUnit>();

            // Return all active units of the valid parent tier, excluding the unit itself
            return _allUnits
                .Where(u => u.Tier == validParentTier.Value && u.OrgUnitId != unit.OrgUnitId && u.IsActive)
                .OrderBy(u => u.Name)
                .ToList();
        }

        private void btnDeactivate_Click(object sender, EventArgs e)
        {
            var selected = SelectedOrgUnit;
            if (selected == null)
            {
                ShowError("Please select an OrgUnit to deactivate.");
                return;
            }

            var confirm = MessageBox.Show(
                string.Format("Deactivate '{0}'? This cannot be undone.", selected.Name),
                "Confirm Deactivation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
                _presenter.DeactivateOrgUnit(selected.OrgUnitId);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var selected = SelectedOrgUnit;
            if (selected == null)
            {
                ShowError("กรุณาเลือกหน่วยงานที่ต้องการลบ");
                return;
            }

            var confirm = MessageBox.Show(
                string.Format("คุณต้องการลบหน่วยงาน '{0}' หรือไม่?\n\nการลบนี้ไม่สามารถยกเลิกได้", 
                    selected.OrgUnitNameTH ?? selected.Name),
                "ยืนยันการลบหน่วยงาน",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                _presenter.DeleteOrgUnit(selected.OrgUnitId);
            }
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        private static TreeNode CreateNode(OrgUnit unit)
        {
            string tierName = unit.Tier == OrgUnitTier.SubCommittee ? "SubCommittee"
                            : unit.Tier == OrgUnitTier.Division ? "Division"
                            : "Department";
            
            string parentInfo = "";
            if (unit.Tier == OrgUnitTier.Division && unit.ParentId == null)
            {
                parentInfo = " (Many-to-Many)";
            }
            else if (unit.ParentId.HasValue)
            {
                parentInfo = $" (Parent: {unit.ParentId})";
            }

            string label = string.Format("{0} [{1}]{2}{3}",
                unit.Name,
                tierName,
                parentInfo,
                unit.IsActive ? string.Empty : " (inactive)");

            var node = new TreeNode(label)
            {
                Tag              = unit,
                ForeColor        = unit.IsActive ? SystemColors.WindowText : SystemColors.GrayText,
                ImageKey         = "folder",
                SelectedImageKey = "open_folder"
            };

            // Add special styling for Many-to-Many divisions
            if (unit.Tier == OrgUnitTier.Division && unit.ParentId == null)
            {
                node.ForeColor = Color.FromArgb(0, 100, 0); // Dark green
                node.NodeFont = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
            }

            return node;
        }

        private static string ConfidentialityLevelLabel(ConfidentialityLevel level)
        {
            switch (level)
            {
                case ConfidentialityLevel.HC: return "1HC — Highly Confidential";
                case ConfidentialityLevel.SC: return "2SC — Secret";
                case ConfidentialityLevel.NC: return "3NC — Non-Confidential";
                case ConfidentialityLevel.PC: return "4PC — Public/Common";
                default:                      return level.ToString();
            }
        }

        private static Color ConfidentialityLevelColor(ConfidentialityLevel level)
        {
            switch (level)
            {
                case ConfidentialityLevel.HC: return Color.DarkRed;
                case ConfidentialityLevel.SC: return Color.OrangeRed;
                case ConfidentialityLevel.NC: return Color.DarkBlue;
                default:                      return SystemColors.WindowText;
            }
        }

        /// <summary>Simple single-line input dialog.</summary>
        private static string PromptForName(string title, string prompt, string defaultValue)
        {
            using (var form  = new Form())
            using (var label = new Label())
            using (var input = new TextBox())
            using (var ok    = new Button())
            using (var cancel = new Button())
            {
                form.Text          = title;
                form.ClientSize    = new Size(360, 110);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MaximizeBox   = false;
                form.MinimizeBox   = false;

                label.Text     = prompt;
                label.Location = new Point(12, 12);
                label.Size     = new Size(336, 20);

                input.Text     = defaultValue ?? string.Empty;
                input.Location = new Point(12, 36);
                input.Size     = new Size(336, 23);
                input.SelectAll();

                ok.Text           = "OK";
                ok.DialogResult   = DialogResult.OK;
                ok.Location       = new Point(192, 72);
                ok.Size           = new Size(75, 26);
                form.AcceptButton = ok;

                cancel.Text           = "Cancel";
                cancel.DialogResult   = DialogResult.Cancel;
                cancel.Location       = new Point(273, 72);
                cancel.Size           = new Size(75, 26);
                form.CancelButton     = cancel;

                form.Controls.AddRange(new Control[] { label, input, ok, cancel });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    string value = input.Text.Trim();
                    return string.IsNullOrEmpty(value) ? null : value;
                }
                return null;
            }
        }

        // ── Search ───────────────────────────────────────────────────────────
        private const string SearchPlaceholder = "ค้นหาหน่วยงาน...";

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == SearchPlaceholder)
            {
                txtSearch.Text      = string.Empty;
                txtSearch.ForeColor = SystemColors.WindowText;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text      = SearchPlaceholder;
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            if (keyword == SearchPlaceholder || string.IsNullOrEmpty(keyword))
            {
                // ล้าง search → แสดง tree ปกติ
                if (_isConfView)
                    _presenter.LoadConfidentialityView();
                else
                    _presenter.LoadOrganizationView();
                return;
            }

            // กรอง _allUnits ตาม keyword (ชื่อหน่วยงาน)
            var matched = _allUnits
                .Where(u => u.IsActive
                    && (u.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
                     || (u.OrgUnitCode != null && u.OrgUnitCode.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)))
                .ToList();

            treeView.BeginUpdate();
            treeView.Nodes.Clear();

            if (matched.Count == 0)
            {
                treeView.Nodes.Add(new TreeNode("(ไม่พบหน่วยงานที่ค้นหา)")
                {
                    ForeColor = Color.Gray,
                    NodeFont  = new Font(treeView.Font, FontStyle.Italic)
                });
            }
            else
            {
                foreach (var u in matched.OrderBy(x => x.Tier).ThenBy(x => x.Name))
                {
                    var node = CreateNode(u);
                    // เน้นข้อความที่ match
                    node.BackColor = Color.FromArgb(255, 255, 176);
                    treeView.Nodes.Add(node);
                }
            }

            treeView.ExpandAll();
            treeView.EndUpdate();
        }

        // ── Confidentiality Level Filter ──────────────────────────────────
        private void OnConfidentialityLevelChanged(object sender, EventArgs e)
        {
            // Ensure only one checkbox is checked (radio button behavior)
            if (sender == chk1HC && chk1HC.Checked)
            {
                chk2SC.Checked = false;
                chk3NC.Checked = false;
                chk4PC.Checked = false;
                FilterByConfidentialityLevel(ConfidentialityLevel.HC);
            }
            else if (sender == chk2SC && chk2SC.Checked)
            {
                chk1HC.Checked = false;
                chk3NC.Checked = false;
                chk4PC.Checked = false;
                FilterByConfidentialityLevel(ConfidentialityLevel.SC);
            }
            else if (sender == chk3NC && chk3NC.Checked)
            {
                chk1HC.Checked = false;
                chk2SC.Checked = false;
                chk4PC.Checked = false;
                FilterByConfidentialityLevel(ConfidentialityLevel.NC);
            }
            else if (sender == chk4PC && chk4PC.Checked)
            {
                chk1HC.Checked = false;
                chk2SC.Checked = false;
                chk3NC.Checked = false;
                FilterByConfidentialityLevel(ConfidentialityLevel.PC);
            }
        }

        private void FilterByConfidentialityLevel(ConfidentialityLevel level)
        {
            try
            {
                treeView.BeginUpdate();
                treeView.Nodes.Clear();

                // Get all units and filter by selected confidentiality level
                var allUnits = _allUnits;
                var filteredUnits = allUnits
                    .Where(u => u.ConfidentialityLevel == level && !u.IsDeleted)
                    .OrderBy(u => u.Name)
                    .ToList();

                // Build hierarchy for filtered units
                foreach (var sc in filteredUnits.Where(u => u.Tier == OrgUnitTier.SubCommittee).OrderBy(u => u.Name))
                {
                    var scNode = CreateNode(sc);
                    foreach (var div in filteredUnits.Where(u => u.Tier == OrgUnitTier.Division && u.ParentId == sc.OrgUnitId).OrderBy(u => u.Name))
                    {
                        var divNode = CreateNode(div);
                        foreach (var dept in filteredUnits.Where(u => u.Tier == OrgUnitTier.Department && u.ParentId == div.OrgUnitId).OrderBy(u => u.Name))
                            divNode.Nodes.Add(CreateNode(dept));
                        scNode.Nodes.Add(divNode);
                    }
                    treeView.Nodes.Add(scNode);
                }

                treeView.ExpandAll();
                treeView.EndUpdate();
            }
            catch (Exception ex)
            {
                ShowError("ไม่สามารถกรองข้อมูลตามระดับได้: " + ex.Message);
            }
        }
    }
}
