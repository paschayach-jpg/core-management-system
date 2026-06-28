using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Controllers.DMS.Document;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Services.DTOs;

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
        private readonly UserContext            _currentUser;
        private List<OrgUnit>                  _allUnits         = new List<OrgUnit>();
        private readonly IAuthorityOrgLookupController _authorityOrgLookupController;

        // ตัวเลือกมุมมองปัจจุบัน: false = ตามองค์กร, true = ตามความลับ
        private bool _isConfView = false;

        // รูปแบบโครงสร้างที่เลือกไว้ — เก็บตลอด session ไม่ต้องเลือกใหม่ทุกครั้งที่กด เพิ่ม
        private OrgStructurePattern? _currentPattern;

        // ซ่อน/แสดง root nodes — เก็บ OrgUnitId ที่ถูกซ่อน
        private readonly HashSet<int> _hiddenOrgUnitIds = new HashSet<int>();

        /// <summary>ยิงเมื่อ hidden node IDs เปลี่ยน — MDI_Container ใช้ reload Folder Explorer</summary>
        public event EventHandler HiddenNodesChanged;

        public OrgStructureForm(CMS.Services.Interfaces.IOrgStructureService service, UserContext user,
            IAuthorityOrgLookupController authorityOrgLookupController = null)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (user    == null) throw new ArgumentNullException("user");

            _currentUser        = user;
            _authorityOrgLookupController = authorityOrgLookupController;

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
            treeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
            treeView.DrawNode += TvDrawNodeNoFocusRect;

            // ── Admin-only: ปุ่มแก้ไขโครงสร้างเปิดได้เฉพาะ Admin (AppRole == 4) ──
            bool isAdmin = _currentUser != null && _currentUser.AppRole == 4;
            btnSelectPattern.Enabled = isAdmin;
            btnCreate.Enabled        = isAdmin;
            btnEdit.Enabled          = isAdmin;
            btnMove.Enabled          = isAdmin;
            btnDelete.Enabled        = isAdmin;
            btnDeactivate.Enabled    = isAdmin;
            btnClose.Enabled         = true;

            if (!isAdmin)
            {
                btnSelectPattern.ToolTipText = "เฉพาะ Admin เท่านั้น";
                btnCreate.ToolTipText        = "เฉพาะ Admin เท่านั้น";
                btnEdit.ToolTipText          = "เฉพาะ Admin เท่านั้น";
                btnMove.ToolTipText          = "เฉพาะ Admin เท่านั้น";
                btnDelete.ToolTipText        = "เฉพาะ Admin เท่านั้น";
                btnDeactivate.ToolTipText    = "เฉพาะ Admin เท่านั้น";
            }

            SetupTreeViewContextMenu(isAdmin);

            // โหลด hidden node IDs จาก DB
            foreach (var id in _presenter.GetHiddenOrgUnitIds())
                _hiddenOrgUnitIds.Add(id);
            UpdateShowAllButton();

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

            treeView.BeginUpdate();
            treeView.Nodes.Clear();

            var subCommittees = _allUnits
                .Where(u => u.Tier == OrgUnitTier.SubCommittee && u.IsActive)
                .OrderBy(u => u.Name);

            foreach (var sc in subCommittees)
            {
                var scNode = CreateNode(sc);

                // ฝ่ายที่มี ParentId = SC นี้ (FK)
                var directDivisions = _allUnits
                    .Where(u => u.Tier == OrgUnitTier.Division && u.IsActive && u.ParentId == sc.OrgUnitId)
                    .OrderBy(u => u.Name)
                    .ToList();

                foreach (var div in directDivisions)
                {
                    scNode.Nodes.Add(BuildDivisionNode(div));
                }

                treeView.Nodes.Add(scNode);
            }

            // Level 0: ฝ่ายที่ไม่มี parent (standalone / LevelZeroOnly pattern)
            var standaloneDivisions = _allUnits
                .Where(u => u.Tier == OrgUnitTier.Division && u.IsActive && u.ParentId == null)
                .OrderBy(u => u.Name)
                .ToList();

            if (standaloneDivisions.Any())
            {
                var headerNode = new TreeNode("≡  Level 0 (ระดับเดียว)")
                {
                    ForeColor = Color.FromArgb(80, 80, 80),
                    NodeFont  = new Font(SystemFonts.DefaultFont, FontStyle.Bold | FontStyle.Italic),
                    Tag       = null
                };
                foreach (var div in standaloneDivisions)
                {
                    var divNode = BuildDivisionNode(div);
                    divNode.ForeColor = Color.FromArgb(0, 100, 0);
                    divNode.NodeFont  = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
                    headerNode.Nodes.Add(divNode);
                }
                treeView.Nodes.Add(headerNode);
                headerNode.Expand();
            }

            if (_hiddenOrgUnitIds.Count > 0)
            {
                foreach (TreeNode node in treeView.Nodes.Cast<TreeNode>().ToList())
                {
                    if (node.Tag is OrgUnit u && _hiddenOrgUnitIds.Contains(u.OrgUnitId))
                        treeView.Nodes.Remove(node);
                }
            }

            treeView.CollapseAll();
            treeView.EndUpdate();
        }

        private TreeNode BuildDivisionNode(OrgUnit div)
        {
            var divNode = CreateNode(div);
            var departments = _allUnits
                .Where(u => u.Tier == OrgUnitTier.Department && u.IsActive && u.ParentId == div.OrgUnitId)
                .OrderBy(u => u.Name);
            foreach (var dept in departments)
                divNode.Nodes.Add(CreateNode(dept));
            return divNode;
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

            treeView.CollapseAll();
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

        private void btnSelectPattern_Click(object sender, EventArgs e)
        {
            using (var dlg = new OrgStructurePatternDialog())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                _currentPattern = dlg.SelectedPattern.Value;
                UpdatePatternLabel();
            }
        }

        private void UpdatePatternLabel()
        {
            if (!_currentPattern.HasValue)
            {
                lblPatternStatus.Text      = "(ยังไม่เลือก)";
                lblPatternStatus.ForeColor = System.Drawing.SystemColors.GrayText;
                return;
            }
            switch (_currentPattern.Value)
            {
                case OrgStructurePattern.DivisionToDepartment:
                    lblPatternStatus.Text      = "Sub→Div→Dep";
                    lblPatternStatus.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
                    break;
                case OrgStructurePattern.LevelZeroOnly:
                    lblPatternStatus.Text      = "Level 0";
                    lblPatternStatus.ForeColor = System.Drawing.Color.FromArgb(0, 120, 60);
                    break;
                default:
                    lblPatternStatus.Text      = "(ไม่ระบุ)";
                    lblPatternStatus.ForeColor = System.Drawing.SystemColors.GrayText;
                    break;
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            // Step 0: ใช้ pattern ที่เก็บไว้ — ถ้ายังไม่เลือกให้เลือกก่อน
            if (!_currentPattern.HasValue)
            {
                using (var patternDlg = new OrgStructurePatternDialog())
                {
                    if (patternDlg.ShowDialog(this) != DialogResult.OK) return;
                    _currentPattern = patternDlg.SelectedPattern.Value;
                    UpdatePatternLabel();
                }
            }
            OrgStructurePattern pattern = _currentPattern.Value;

            // Step 1: ตรวจสอบ selected node เพื่อกำหนด context
            TreeNode selectedNode = treeView.SelectedNode;
            OrgUnit selectedOrgUnit = null;

            if (selectedNode?.Tag is OrgUnit unit)
                selectedOrgUnit = unit;

            // Step 2: กำหนด tier และ parent ตาม pattern
            OrgUnitTier? selectedTier;
            OrgUnit parentForDialog;

            if (pattern == OrgStructurePattern.LevelZeroOnly)
            {
                // Level 0: Division ที่ root เท่านั้น — ไม่ต้องเลือก tier
                selectedTier    = OrgUnitTier.Division;
                parentForDialog = null;
            }
            else if (pattern == OrgStructurePattern.DivisionToDepartment)
            {
                // Sub→Div→Dep: context-aware based on selected node
                if (selectedOrgUnit == null)
                {
                    // Nothing selected → create SubCommittee as root
                    selectedTier    = OrgUnitTier.SubCommittee;
                    parentForDialog = null;
                }
                else if (selectedOrgUnit.Tier == OrgUnitTier.SubCommittee)
                {
                    // SubCommittee selected → ask: Division under it, or new SubCommittee root?
                    var choice = MessageBox.Show(
                        string.Format(
                            "เลือกสิ่งที่ต้องการสร้าง:\n\n" +
                            "  [Yes]  สร้าง Division ใต้ \"{0}\"\n" +
                            "  [No]   สร้าง SubCommittee ใหม่ที่ root",
                            selectedOrgUnit.OrgUnitNameTH),
                        "เลือกประเภทที่ต้องการสร้าง",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                    if (choice == DialogResult.Cancel) return;
                    if (choice == DialogResult.Yes)
                    {
                        selectedTier    = OrgUnitTier.Division;
                        parentForDialog = selectedOrgUnit;
                    }
                    else
                    {
                        selectedTier    = OrgUnitTier.SubCommittee;
                        parentForDialog = null;
                    }
                }
                else if (selectedOrgUnit.Tier == OrgUnitTier.Division)
                {
                    // Division selected → add Department under it
                    selectedTier    = OrgUnitTier.Department;
                    parentForDialog = selectedOrgUnit;
                }
                else
                {
                    MessageBox.Show(
                        "กรุณาเลือก SubCommittee หรือ Division เพื่อเพิ่มหน่วยงานใหม่\n" +
                        "หรือยกเลิกการเลือกทั้งหมดเพื่อสร้าง SubCommittee ใหม่",
                        "ไม่สามารถเพิ่มได้", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                // Legacy: ให้ user เลือก tier เอง
                selectedTier = PromptForTier(selectedOrgUnit, pattern);
                if (!selectedTier.HasValue) return;
                parentForDialog = selectedTier.Value == OrgUnitTier.SubCommittee ? null : selectedOrgUnit;
            }

            // Step 3: เปิด dialog สร้างหน่วยงาน
            using (var dlg = new CreateOrgUnitDialog(_allUnits, null, selectedTier,
                selectedParent: parentForDialog,
                authorityOrgLookupController: _authorityOrgLookupController))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                // ใส่ pattern ใน request ก่อนส่ง presenter
                if (dlg.HierarchicalResult != null)
                {
                    if (pattern == OrgStructurePattern.LevelZeroOnly)
                    {
                        // Level0: shift tiers up by 1 — HR Division → SubCommittee, HR Department → Division
                        var divReq = dlg.HierarchicalResult.DivisionRequest;
                        divReq.OrgUnitType     = (int)OrgUnitTier.SubCommittee;
                        divReq.ParentOrgUnitID = null;
                        divReq.StructurePattern = pattern;

                        var deptRequests = dlg.HierarchicalResult.DepartmentRequests
                                           ?? new List<CreateOrgUnitRequest>();
                        foreach (var r in deptRequests)
                        {
                            r.OrgUnitType      = (int)OrgUnitTier.Division;
                            r.ParentOrgUnitID  = null;  // presenter fills after SubCommittee created
                            r.StructurePattern = pattern;
                        }
                        _presenter.CreateDivisionWithDepartments(divReq, deptRequests);
                    }
                    else
                    {
                        dlg.HierarchicalResult.DivisionRequest.StructurePattern = pattern;
                        if (dlg.HierarchicalResult.DepartmentRequests != null)
                            foreach (var r in dlg.HierarchicalResult.DepartmentRequests)
                                r.StructurePattern = pattern;
                        _presenter.CreateDivisionWithDepartments(
                            dlg.HierarchicalResult.DivisionRequest,
                            dlg.HierarchicalResult.DepartmentRequests);
                    }
                }
                else if (dlg.BatchResults != null && dlg.BatchResults.Count > 0)
                {
                    foreach (var r in dlg.BatchResults) r.StructurePattern = pattern;
                    _presenter.CreateOrgUnitBatch(dlg.BatchResults);
                }
                else if (dlg.Result != null)
                {
                    dlg.Result.StructurePattern = pattern;
                    _presenter.CreateOrgUnitBatch(new[] { dlg.Result });
                }
            }
        }

        /// <summary>แสดง dialog เลือกประเภทหน่วยงาน กรองตาม pattern ที่เลือก</summary>
        private OrgUnitTier? PromptForTier(OrgUnit selectedParent = null,
            OrgStructurePattern pattern = OrgStructurePattern.Legacy)
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
                    headerText = string.Format("สร้างหน่วยงานใต้: {0}", selectedParent.Name);

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

                OrgUnitTier result = OrgUnitTier.Division;
                int yPos = 60;

                // กำหนดตัวเลือกตาม parent type และ pattern
                List<OrgUnitTier> availableTiers = new List<OrgUnitTier>();

                if (selectedParent == null)
                {
                    if (pattern == OrgStructurePattern.DivisionToDepartment)
                    {
                        // Sub→Div→Dep: Sub เป็น root — ต้องสร้าง SubCommittee ก่อน
                        availableTiers.Add(OrgUnitTier.SubCommittee);
                        result = OrgUnitTier.SubCommittee;
                    }
                    else
                    {
                        // Legacy: แสดงทุกตัวเลือก
                        availableTiers.Add(OrgUnitTier.SubCommittee);
                        availableTiers.Add(OrgUnitTier.Division);
                        availableTiers.Add(OrgUnitTier.Department);
                        result = OrgUnitTier.SubCommittee;
                    }
                }
                else if (selectedParent.Tier == OrgUnitTier.SubCommittee)
                {
                    availableTiers.Add(OrgUnitTier.Division);
                    if (pattern != OrgStructurePattern.DivisionToDepartment)
                        availableTiers.Add(OrgUnitTier.SubCommittee);
                    result = OrgUnitTier.Division;
                }
                else if (selectedParent.Tier == OrgUnitTier.Division)
                {
                    if (pattern != OrgStructurePattern.DivisionToDepartment)
                        availableTiers.Add(OrgUnitTier.SubCommittee);
                    availableTiers.Add(OrgUnitTier.Department);
                    result = OrgUnitTier.Department;
                }
                else
                {
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
                ShowError("กรุณาเลือกหน่วยงานที่ต้องการปิดใช้งาน");
                return;
            }

            string unitName = selected.OrgUnitNameTH ?? selected.Name;

            if (_presenter.HasActiveDependents(selected.OrgUnitId))
            {
                MessageBox.Show(
                    string.Format(
                        "ไม่สามารถปิดใช้งานหน่วยงาน '{0}' ได้\n\n" +
                        "เนื่องจากยังมีโฟลเดอร์หรือบัญชีผู้ใช้งานที่เชื่อมโยงกับหน่วยงานนี้อยู่\n\n" +
                        "กรุณาดำเนินการต่อไปนี้ก่อน:\n" +
                        "  • ย้ายหรือปิดใช้งานโฟลเดอร์ทั้งหมดในหน่วยงานนี้\n" +
                        "  • ย้ายหรือปิดใช้งานบัญชีผู้ใช้งานที่สังกัดหน่วยงานนี้",
                        unitName),
                    "ไม่สามารถปิดใช้งานได้",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                string.Format(
                    "คุณต้องการปิดใช้งานหน่วยงาน '{0}' หรือไม่?\n\n" +
                    "หน่วยงานที่ปิดใช้งานจะไม่ปรากฏในระบบ\n" +
                    "(สามารถเปิดใช้งานอีกครั้งได้ในภายหลัง)",
                    unitName),
                "ยืนยันการปิดใช้งาน",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            _presenter.DeactivateOrgUnit(selected.OrgUnitId);
        }

        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            treeView.BeginUpdate();
            treeView.ExpandAll();
            treeView.EndUpdate();
            if (treeView.Nodes.Count > 0)
                treeView.Nodes[0].EnsureVisible();
        }

        private void btnCollapseAll_Click(object sender, EventArgs e)
        {
            treeView.BeginUpdate();
            treeView.CollapseAll();
            treeView.EndUpdate();
        }

        private void btnShowAll_Click(object sender, EventArgs e)
        {
            _hiddenOrgUnitIds.Clear();
            _presenter.SaveHiddenOrgUnitIds(_hiddenOrgUnitIds);
            _presenter.LoadOrganizationView();
            UpdateShowAllButton();
            HiddenNodesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateShowAllButton()
        {
            int count = _hiddenOrgUnitIds.Count;
            btnShowAll.Enabled = count > 0;
            btnShowAll.Text = count > 0 ? $"👁 แสดงทั้งหมด ({count})" : "👁 แสดงทั้งหมด";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var selected = SelectedOrgUnit;
            if (selected == null)
            {
                ShowError("กรุณาเลือกหน่วยงานที่ต้องการลบ");
                return;
            }

            string unitName = selected.OrgUnitNameTH ?? selected.Name;

            var confirm = MessageBox.Show(
                string.Format("คุณต้องการลบหน่วยงาน '{0}' หรือไม่?\n\nการลบนี้ไม่สามารถยกเลิกได้", unitName),
                "ยืนยันการลบหน่วยงาน",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            // ตรวจสอบ dependents ก่อนลบ
            if (_presenter.HasActiveDependents(selected.OrgUnitId))
            {
                var forceConfirm = MessageBox.Show(
                    string.Format(
                        "หน่วยงาน '{0}' ยังมีโฟลเดอร์หรือผู้ใช้งานที่เชื่อมโยงอยู่\n\n" +
                        "ต้องการลบหน่วยงานนี้หรือไม่?\n" +
                        "  • โฟลเดอร์ทั้งหมดจะถูกย้ายไปถังขยะ\n" +
                        "  • ผู้ใช้งานยังคงมีบัญชีและใช้งานได้ตามปกติ\n" +
                        "    (แต่หน่วยงานจะไม่ปรากฏในระบบอีกต่อไป)\n\n" +
                        "การดำเนินการนี้ไม่สามารถยกเลิกได้",
                        unitName),
                    "ยืนยันการลบหน่วยงาน",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (forceConfirm == DialogResult.Yes)
                    _presenter.ForceDeleteOrgUnit(selected.OrgUnitId);
            }
            else
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
                parentInfo = " [Level 0]";
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

        // ── Right-click context menu ──────────────────────────────────────────
        private void SetupTreeViewContextMenu(bool isAdmin)
        {
            var ctx = new ContextMenuStrip();
            ctx.Font = new Font("Segoe UI", 9.5f);

            var mnuAdd = new ToolStripMenuItem("+ เพิ่มหน่วยงาน");
            mnuAdd.Enabled = isAdmin;
            mnuAdd.Click  += (s, e) => btnCreate_Click(s, e);

            var mnuEdit = new ToolStripMenuItem("✏ แก้ไขชื่อ");
            mnuEdit.Enabled = isAdmin;
            mnuEdit.Click  += (s, e) => btnEdit_Click(s, e);

            var mnuMove = new ToolStripMenuItem("↕ ย้าย");
            mnuMove.Enabled = isAdmin;
            mnuMove.Click  += (s, e) => btnMove_Click(s, e);

            var mnuDelete = new ToolStripMenuItem("🗑 ลบ");
            mnuDelete.Enabled = isAdmin;
            mnuDelete.ForeColor = isAdmin ? Color.Firebrick : SystemColors.GrayText;
            mnuDelete.Click  += (s, e) => btnDelete_Click(s, e);

            var mnuDeactivate = new ToolStripMenuItem("⛔ ปิดใช้งานหน่วยงาน");
            mnuDeactivate.Enabled   = isAdmin;
            mnuDeactivate.ForeColor = isAdmin ? Color.Firebrick : SystemColors.GrayText;
            mnuDeactivate.Click    += (s, e) => btnDeactivate_Click(s, e);

            var mnuHideNode = new ToolStripMenuItem("🙈 ซ่อน node นี้");
            var mnuShowAllCtx = new ToolStripMenuItem("👁 แสดงทั้งหมด");
            mnuHideNode.Click += (s, e) =>
            {
                var node = treeView.SelectedNode;
                if (node != null && node.Level == 0 && node.Tag is OrgUnit unit)
                {
                    _hiddenOrgUnitIds.Add(unit.OrgUnitId);
                    treeView.Nodes.Remove(node);
                    _presenter.SaveHiddenOrgUnitIds(_hiddenOrgUnitIds);
                    UpdateShowAllButton();
                    HiddenNodesChanged?.Invoke(this, EventArgs.Empty);
                }
            };
            mnuShowAllCtx.Click += (s, e) =>
            {
                _hiddenOrgUnitIds.Clear();
                _presenter.SaveHiddenOrgUnitIds(_hiddenOrgUnitIds);
                _presenter.LoadOrganizationView();
                UpdateShowAllButton();
                HiddenNodesChanged?.Invoke(this, EventArgs.Empty);
            };

            ctx.Items.Add(mnuAdd);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuEdit);
            ctx.Items.Add(mnuMove);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuDelete);
            ctx.Items.Add(mnuDeactivate);
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add(mnuHideNode);
            ctx.Items.Add(mnuShowAllCtx);

            // เลือก node ที่ right-click ก่อนเปิด menu
            treeView.NodeMouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                    treeView.SelectedNode = e.Node;
            };

            // ปรับ enable/disable ตาม node ที่เลือกก่อนแสดง menu
            ctx.Opening += (s, e) =>
            {
                bool hasSelection = treeView.SelectedNode?.Tag is OrgUnit;
                bool isRootOrgUnit = hasSelection && treeView.SelectedNode.Level == 0;
                mnuEdit.Enabled       = isAdmin && hasSelection;
                mnuMove.Enabled       = isAdmin && hasSelection;
                mnuDelete.Enabled     = isAdmin && hasSelection;
                mnuDeactivate.Enabled = isAdmin && hasSelection;
                mnuHideNode.Enabled   = isRootOrgUnit;
                mnuHideNode.Text      = isRootOrgUnit
                    ? $"🙈 ซ่อน \"{((OrgUnit)treeView.SelectedNode.Tag).Name}\""
                    : "🙈 ซ่อน node นี้";
                mnuShowAllCtx.Enabled = _hiddenOrgUnitIds.Count > 0;
                mnuShowAllCtx.Text    = _hiddenOrgUnitIds.Count > 0
                    ? $"👁 แสดงทั้งหมด ({_hiddenOrgUnitIds.Count})"
                    : "👁 แสดงทั้งหมด";
            };

            treeView.ContextMenuStrip = ctx;
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

        private void TvDrawNodeNoFocusRect(object sender, DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = true;
            if ((e.State & TreeNodeStates.Focused) != 0)
            {
                Color bg = e.Node.IsSelected
                    ? (treeView.Focused ? SystemColors.Highlight : SystemColors.Control)
                    : (e.Node.BackColor != Color.Empty ? e.Node.BackColor : treeView.BackColor);
                using (var p = new Pen(bg))
                    e.Graphics.DrawRectangle(p, e.Bounds.X, e.Bounds.Y,
                        e.Bounds.Width - 1, e.Bounds.Height - 1);
            }
        }
    }
}
