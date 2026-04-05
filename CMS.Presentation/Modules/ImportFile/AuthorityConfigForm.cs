using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
namespace CMS.Presentation
{
    public partial class AuthorityConfigForm : Form
    {
        private readonly IFolderAuthorityService    _service;
        private readonly IHrDivisionRepository      _hrDivRepo;
        private readonly IHrDepartmentRepository    _hrDeptRepo;
        private readonly IHrPositionRepository      _hrPosRepo;
        private readonly IHrPositionMappingRepository _hrPosMappingRepo;
        private readonly IUserAccountRepository     _userAccountRepo;
        private readonly UserContext                _user;

        private List<FolderAuthorityConfig> _allConfigs = new List<FolderAuthorityConfig>();
        private FolderAuthorityConfig       _editing;

        // Lookup สำหรับแสดงชื่อใน grid
        private Dictionary<string, string> _deptLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _divLookup  = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public AuthorityConfigForm()
        {
            InitializeComponent();
        }

        public AuthorityConfigForm(IFolderAuthorityService service, UserContext user,
            IHrDivisionRepository hrDivRepo = null, IHrDepartmentRepository hrDeptRepo = null,
            IHrPositionRepository hrPosRepo = null, IUserAccountRepository userAccountRepo = null,
            IHrPositionMappingRepository hrPosMappingRepo = null)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (user    == null) throw new ArgumentNullException("user");
            _service          = service;
            _user             = user;
            _hrDivRepo        = hrDivRepo;
            _hrDeptRepo       = hrDeptRepo;
            _hrPosRepo        = hrPosRepo;
            _userAccountRepo  = userAccountRepo;
            _hrPosMappingRepo = hrPosMappingRepo;

            InitializeComponent();
            this.Load += (s, e) =>
            {
                SetupGrid();
                tvOrg.AfterSelect += tvOrg_AfterSelect;
                LoadTree();
                LoadData();
            };
        }

        // ── TreeView ──────────────────────────────────────────────────────────

        private void LoadTree()
        {
            tvOrg.Nodes.Clear();
            tvOrg.BeginUpdate();

            // root node "ทั้งหมด"
            var rootNode = new TreeNode("📋 ทั้งหมด") { Tag = null };
            tvOrg.Nodes.Add(rootNode);

            if (_hrDivRepo != null)
            {
                var divs  = _hrDivRepo.GetAll().OrderBy(d => d.DivisionCode).ToList();
                var depts = _hrDeptRepo != null
                    ? _hrDeptRepo.GetAll().ToList()
                    : new List<HrDepartment>();

                // Pre-load HR_Position ครั้งเดียว → lookup PositionName → LevelCode
                // ใช้ LevelCode ก่อน ถ้าว่างใช้ Level เดิม
                var posLevelMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (_hrPosRepo != null)
                    foreach (var p in _hrPosRepo.GetAll())
                        if (!string.IsNullOrEmpty(p.PositionName))
                            posLevelMap[p.PositionName] = !string.IsNullOrEmpty(p.LevelCode) ? p.LevelCode
                                                        : !string.IsNullOrEmpty(p.PositionLevel) ? p.PositionLevel
                                                        : "";

                // Pre-load HR_PositionMapping → DepartmentID → List<PositionName>
                var deptIdPosMap = new Dictionary<int, List<string>>();
                var divIdPosMap  = new Dictionary<int, List<string>>();
                if (_hrPosMappingRepo != null)
                {
                    try
                    {
                        var allMappings = _hrPosMappingRepo.GetAll().ToList();
                        System.Diagnostics.Debug.WriteLine(
                            string.Format("[AuthorityConfig] HR_PositionMapping loaded: {0} rows", allMappings.Count));

                        foreach (var m in allMappings)
                        {
                            if (string.IsNullOrEmpty(m.PositionName)) continue;

                            // อัปเดต posLevelMap ด้วย LevelCode จาก mapping
                            if (!string.IsNullOrEmpty(m.Level))
                                posLevelMap[m.PositionName] = m.Level;

                            if (m.DepartmentID.HasValue)
                            {
                                if (!deptIdPosMap.ContainsKey(m.DepartmentID.Value))
                                    deptIdPosMap[m.DepartmentID.Value] = new List<string>();
                                if (!deptIdPosMap[m.DepartmentID.Value].Contains(m.PositionName))
                                    deptIdPosMap[m.DepartmentID.Value].Add(m.PositionName);
                            }
                            else
                            {
                                if (!divIdPosMap.ContainsKey(m.DivisionID))
                                    divIdPosMap[m.DivisionID] = new List<string>();
                                if (!divIdPosMap[m.DivisionID].Contains(m.PositionName))
                                    divIdPosMap[m.DivisionID].Add(m.PositionName);
                            }
                        }
                        System.Diagnostics.Debug.WriteLine(
                            string.Format("[AuthorityConfig] deptIdPosMap keys: {0}", string.Join(",", deptIdPosMap.Keys)));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            string.Format("[AuthorityConfig] HR_PositionMapping ERROR: {0}", ex.Message));
                    }
                }

                foreach (var div in divs)
                {
                    var divNode = new TreeNode(string.Format("🏢 {0} — {1}", div.DivisionCode, div.DivisionName))
                    {
                        Tag       = new NodeTag { Type = "DIV", Code = div.DivisionCode, Name = div.DivisionName },
                        ForeColor = Color.FromArgb(31, 56, 100)
                    };

                    foreach (var dep in depts.Where(d => d.DivisionID == div.DivisionID).OrderBy(d => d.DepartmentCode))
                    {
                        string depDisplay = !string.IsNullOrEmpty(dep.DepartmentNameTh)
                            ? dep.DepartmentNameTh : dep.DepartmentName;

                        var depNode = new TreeNode(string.Format("📁 {0} — {1}", dep.DepartmentCode, depDisplay))
                        {
                            Tag       = new NodeTag { Type = "DEP", Code = dep.DepartmentCode, Name = depDisplay, ParentCode = div.DivisionCode },
                            ForeColor = Color.FromArgb(60, 60, 60)
                        };

                        // Position nodes ใต้ Department
                        // Priority: HR_PositionMapping (by DepartmentID) → UserAccount → HR_Position (all)
                        IEnumerable<string> posNames = null;

                        // 1. HR_PositionMapping by DepartmentID (ถูกต้องที่สุด)
                        List<string> mappedPos;
                        if (deptIdPosMap.TryGetValue(dep.DepartmentID, out mappedPos) && mappedPos.Count > 0)
                        {
                            posNames = mappedPos;
                            System.Diagnostics.Debug.WriteLine(
                                string.Format("[AuthorityConfig] {0} (ID={1}): {2} positions from PositionMapping",
                                    dep.DepartmentCode, dep.DepartmentID, mappedPos.Count));
                        }

                        // 2. UserAccount fallback
                        if (posNames == null && _userAccountRepo != null)
                        {
                            try { posNames = _userAccountRepo.GetPositionsByDepartmentCode(dep.DepartmentCode); }
                            catch { }
                        }

                        // 3. HR_Position ทั้งหมด fallback
                        if (posNames == null && posLevelMap.Count > 0)
                            posNames = posLevelMap.Keys.OrderBy(k => k);

                        if (posNames != null)
                        {
                            foreach (var posName in posNames.OrderBy(n => n))
                            {
                                string levelCode;
                                posLevelMap.TryGetValue(posName, out levelCode);
                                levelCode = levelCode ?? "";

                                string posLabel = string.IsNullOrEmpty(levelCode)
                                    ? string.Format("  🏷 {0}", posName)
                                    : string.Format("  🏷 {0}  [{1}]", posName, levelCode);

                                depNode.Nodes.Add(new TreeNode(posLabel)
                                {
                                    Tag       = new NodeTag { Type = "POS_IN_DEP", Code = posName, Name = posName, ParentCode = dep.DepartmentCode, LevelCode = levelCode },
                                    ForeColor = Color.FromArgb(100, 60, 0)
                                });
                            }
                        }

                        divNode.Nodes.Add(depNode);
                    }
                    tvOrg.Nodes.Add(divNode);
                }
            }

            tvOrg.EndUpdate();
            rootNode.Expand();
            tvOrg.SelectedNode = rootNode;
        }

        private void tvOrg_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var tag = e.Node?.Tag as NodeTag;
            FilterGrid(tag);

            // Auto-fill edit panel เมื่อคลิก Position ใน Department
            if (tag != null && tag.Type == "POS_IN_DEP" && pnlEdit.Visible)
            {
                txtPosition.Text  = tag.Name;
                txtDept.Text      = tag.ParentCode;
                txtLevelCode.Text = tag.LevelCode ?? "";
            }
        }

        private void FilterGrid(NodeTag tag)
        {
            dgv.Rows.Clear();
            IEnumerable<FolderAuthorityConfig> filtered = _allConfigs;

            if (tag != null)
            {
                if (tag.Type == "DIV")
                {
                    // แสดง config ของ Division + ทุก Department ใน Division นั้น + Position ที่สังกัด
                    filtered = _allConfigs.Where(c => c.DivisionCode == tag.Code);
                }
                else if (tag.Type == "DEP")
                {
                    // แสดง config ของ Department + Position ที่สังกัด Department นั้น
                    var deptCode = tag.Code;
                    var posInDept = GetPositionNamesForDept(deptCode);
                    filtered = _allConfigs.Where(c =>
                        c.DepartmentCode == deptCode ||
                        (!string.IsNullOrEmpty(c.PositionName) && posInDept.Contains(c.PositionName)));
                }
                else if (tag.Type == "POS" || tag.Type == "POS_IN_DEP")
                {
                    filtered = _allConfigs.Where(c => c.PositionName == tag.Code);
                }
                else if (tag.Type == "POS_GROUP")
                {
                    filtered = _allConfigs.Where(c => !string.IsNullOrEmpty(c.PositionName));
                }
            }

            foreach (var c in filtered)
            {
                string posDisplay, deptDisplay, divDisplay;
                string deptFull, divFull;

                // ── resolve dept/div display names จาก lookup (ทุกประเภท config) ──
                _deptLookup.TryGetValue(c.DepartmentCode ?? "", out deptFull);
                _divLookup.TryGetValue(c.DivisionCode   ?? "", out divFull);
                deptDisplay = deptFull ?? c.DepartmentCode ?? "";
                divDisplay  = divFull  ?? c.DivisionCode   ?? "";

                if (!string.IsNullOrEmpty(c.PositionName))
                {
                    posDisplay = c.PositionName;
                }
                else if (!string.IsNullOrEmpty(c.DepartmentCode))
                {
                    posDisplay = deptFull ?? c.DepartmentCode;
                }
                else
                {
                    posDisplay = divFull ?? c.DivisionCode ?? "";
                }

                dgv.Rows.Add(
                    posDisplay, c.LevelCode, deptDisplay, divDisplay,
                    c.HC_View, c.HC_Add, c.HC_Edit, c.HC_Approve,
                    c.SC_View, c.SC_Add, c.SC_Edit, c.SC_Approve,
                    c.NC_View, c.NC_Add, c.NC_Edit, c.NC_Approve,
                    c.PC_View, c.PC_Add, c.PC_Edit, c.PC_Approve);
                dgv.Rows[dgv.Rows.Count - 1].Tag = c;
            }

            // ปรับความกว้างคอลัมน์ข้อความให้พอดีกับข้อมูล
            dgv.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            lblFilter.Text = tag == null
                ? "แสดง: ทั้งหมด"
                : tag.Type == "POS_GROUP"
                    ? "แสดง: ตำแหน่งงานทั้งหมด"
                    : tag.Type == "POS_IN_DEP"
                        ? string.Format("แสดง: ตำแหน่ง {0} ใน {1}", tag.Name, tag.ParentCode)
                        : string.Format("แสดง: {0} — {1}", tag.Code, tag.Name);
        }

        private class NodeTag
        {
            public string Type       { get; set; } // "DIV" | "DEP" | "POS" | "POS_IN_DEP" | "POS_GROUP"
            public string Code       { get; set; }
            public string Name       { get; set; }
            public string ParentCode { get; set; }
            public string LevelCode  { get; set; }
        }

        /// <summary>
        /// คืน set ของ PositionName ที่สังกัด DepartmentCode นั้น
        /// ใช้ข้อมูลจาก deptIdPosMap ที่ build ไว้ใน LoadTree
        /// </summary>
        private HashSet<string> GetPositionNamesForDept(string departmentCode)
        {
            // หา DepartmentID จาก _hrDeptRepo
            if (_hrDeptRepo == null) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var dept = _hrDeptRepo.GetAll()
                    .FirstOrDefault(d => string.Equals(d.DepartmentCode, departmentCode, StringComparison.OrdinalIgnoreCase));
                if (dept == null) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // ดึง Position จาก HR_PositionMapping
                if (_hrPosMappingRepo != null)
                {
                    var names = _hrPosMappingRepo.GetByDepartmentId(dept.DepartmentID)
                        .Select(m => m.PositionName)
                        .Where(n => !string.IsNullOrEmpty(n));
                    return new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
                }
            }
            catch { }
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        // ── Grid Setup ────────────────────────────────────────────────────────

        private void SetupGrid()
        {
            dgv.AutoGenerateColumns  = false;
            dgv.AllowUserToAddRows   = false;
            dgv.MultiSelect          = false;
            dgv.SelectionMode        = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowHeadersVisible    = false;
            dgv.BackgroundColor      = Color.White;
            dgv.GridColor            = Color.FromArgb(200, 200, 200);
            dgv.Font                 = new Font("Segoe UI", 8.5f);
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight  = 62;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 56, 100);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);

            AddTextCol("colPosition", "ตำแหน่ง",          DataGridViewAutoSizeColumnMode.Fill, 28);
            AddTextCol("colLevelCode","ระดับ",              DataGridViewAutoSizeColumnMode.Fill, 5);
            AddTextCol("colDept",     "แผนก",              DataGridViewAutoSizeColumnMode.Fill, 25);
            AddTextCol("colDiv",      "ฝ่าย",              DataGridViewAutoSizeColumnMode.Fill, 22);

            AddCheckCol("colHC_V","ดู",36); AddCheckCol("colHC_A","เพิ่ม",36);
            AddCheckCol("colHC_E","แก้ไข",36); AddCheckCol("colHC_Ap","อนุมัติ",36);
            AddCheckCol("colSC_V","ดู",36); AddCheckCol("colSC_A","เพิ่ม",36);
            AddCheckCol("colSC_E","แก้ไข",36); AddCheckCol("colSC_Ap","อนุมัติ",36);
            AddCheckCol("colNC_V","ดู",36); AddCheckCol("colNC_A","เพิ่ม",36);
            AddCheckCol("colNC_E","แก้ไข",36); AddCheckCol("colNC_Ap","อนุมัติ",36);
            AddCheckCol("colPC_V","ดู",36); AddCheckCol("colPC_A","เพิ่ม",36);
            AddCheckCol("colPC_E","แก้ไข",36); AddCheckCol("colPC_Ap","อนุมัติ",36);

            dgv.CellPainting += Dgv_CellPainting;
        }

        private static readonly (int start, int count, string label, string sublabel, Color color)[] _groups =
        {
            (4,  4, "HC", "สูงสุด (Highly Confidential)",      Color.FromArgb(180, 30,  30)),
            (8,  4, "SC", "ลับมาก (Strictly Confidential)",    Color.FromArgb(200, 100,  0)),
            (12, 4, "NC", "ปกติ (Normal Confidential)",        Color.FromArgb(0,   90, 160)),
            (16, 4, "PC", "ทั่วไป (Public)",                   Color.FromArgb(0,  130,  60)),
        };

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex != -1) return;
            const int groupH = 30;
            int subH = dgv.ColumnHeadersHeight - groupH;

            e.PaintBackground(e.ClipBounds, false);

            // ── Sub-header (bottom area) ──────────────────────────────────────
            var subRect = new Rectangle(e.CellBounds.X, e.CellBounds.Y + groupH, e.CellBounds.Width, subH);
            using (var bg = new SolidBrush(Color.FromArgb(31, 56, 100)))
                e.Graphics.FillRectangle(bg, subRect);

            using (var pen = new Pen(Color.FromArgb(60, 80, 120)))
            {
                e.Graphics.DrawLine(pen, e.CellBounds.X,      e.CellBounds.Y + groupH, e.CellBounds.Right, e.CellBounds.Y + groupH);
                e.Graphics.DrawLine(pen, e.CellBounds.Right - 1, e.CellBounds.Y + groupH, e.CellBounds.Right - 1, e.CellBounds.Bottom);
            }

            string headerText = e.Value?.ToString() ?? "";
            if (!string.IsNullOrEmpty(headerText))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var br = new SolidBrush(Color.White))
                    e.Graphics.DrawString(headerText, dgv.ColumnHeadersDefaultCellStyle.Font, br, subRect, sf);

            // ── Group band (top area) ─────────────────────────────────────────
            var topRect = new Rectangle(e.CellBounds.X, e.CellBounds.Y, e.CellBounds.Width, groupH);
            foreach (var g in _groups)
            {
                if (e.ColumnIndex >= g.start && e.ColumnIndex < g.start + g.count)
                {
                    using (var bg = new SolidBrush(g.color))
                        e.Graphics.FillRectangle(bg, topRect);

                    // Draw label every column — position text relative to the full group rect
                    // so each cell contributes its slice (natural cell clip handles the rest)
                    int xOffset  = Enumerable.Range(g.start, e.ColumnIndex - g.start).Sum(i => dgv.Columns[i].Width);
                    int totalW   = Enumerable.Range(g.start, g.count).Sum(i => dgv.Columns[i].Width);
                    var groupRect = new Rectangle(e.CellBounds.X - xOffset, e.CellBounds.Y, totalW, groupH);

                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    using (var br = new SolidBrush(Color.White))
                    {
                        // Line 1: code (bold)
                        var topLine = new Rectangle(groupRect.X, groupRect.Y, groupRect.Width, groupH / 2);
                        using (var f = new Font("Segoe UI", 9f, FontStyle.Bold))
                            e.Graphics.DrawString(g.label, f, br, topLine, sf);

                        // Line 2: full name
                        var botLine = new Rectangle(groupRect.X, groupRect.Y + groupH / 2, groupRect.Width, groupH / 2);
                        using (var f = new Font("Segoe UI", 7f))
                            e.Graphics.DrawString(g.sublabel, f, br, botLine, sf);
                    }
                    break;
                }
            }

            // ── Text columns 0-3 (dark blue full cell) ────────────────────────
            if (e.ColumnIndex < 4)
            {
                using (var bg = new SolidBrush(Color.FromArgb(31, 56, 100)))
                    e.Graphics.FillRectangle(bg, e.CellBounds);
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var br = new SolidBrush(Color.White))
                    e.Graphics.DrawString(headerText, dgv.ColumnHeadersDefaultCellStyle.Font, br, e.CellBounds, sf);
            }

            e.Handled = true;
        }

        private void AddTextCol(string name, string header, DataGridViewAutoSizeColumnMode mode, int fw)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name, HeaderText = header, AutoSizeMode = mode, FillWeight = fw, ReadOnly = true,
                MinimumWidth = 50,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.False, Padding = new Padding(4, 0, 4, 0) },
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleLeft } }
            });
        }

        private void AddCheckCol(string name, string header, int width)
        {
            dgv.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = name, HeaderText = header, Width = width,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ReadOnly = true, TrueValue = true, FalseValue = false,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } }
            });
        }

        // ── Load Data ─────────────────────────────────────────────────────────

        private void LoadData()
        {
            // Build lookup: code → display name จาก HR tables (เก็บเป็น field เพื่อใช้ใน FilterGrid ด้วย)
            _deptLookup.Clear();
            _divLookup.Clear();

            if (_hrDeptRepo != null)
                foreach (var d in _hrDeptRepo.GetAll())
                    if (!string.IsNullOrEmpty(d.DepartmentCode))
                        _deptLookup[d.DepartmentCode] = string.Format("{0} — {1}", d.DepartmentCode, d.DisplayName);

            if (_hrDivRepo != null)
                foreach (var d in _hrDivRepo.GetAll())
                    if (!string.IsNullOrEmpty(d.DivisionCode))
                        _divLookup[d.DivisionCode] = string.Format("{0} — {1}", d.DivisionCode, d.DivisionName);

            var allRows = _service.GetAll().ToList();
            _allConfigs = new List<FolderAuthorityConfig>();

            foreach (var grp in allRows.GroupBy(r => r.ConfigLabel))
            {
                var first = grp.First();

                // PositionName เก็บค่าจริงจาก DB เท่านั้น
                // การแสดงผลใน grid จัดการใน FilterGrid โดยใช้ _deptLookup/_divLookup
                string displayLabel = first.PositionName;
                if (string.IsNullOrEmpty(displayLabel) && !string.IsNullOrEmpty(first.DepartmentCode))
                    _deptLookup.TryGetValue(first.DepartmentCode, out displayLabel);
                if (string.IsNullOrEmpty(displayLabel) && !string.IsNullOrEmpty(first.DivisionCode))
                    _divLookup.TryGetValue(first.DivisionCode, out displayLabel);
                if (string.IsNullOrEmpty(displayLabel))
                    displayLabel = grp.Key;

                var flat = new FolderAuthorityConfig
                {
                    ConfigLabel    = grp.Key,
                    PositionName   = first.PositionName,
                    LevelCode      = first.LevelCode,
                    DepartmentCode = first.DepartmentCode,
                    DivisionCode   = first.DivisionCode
                };

                // Enrich display names
                string deptName, divName;
                if (!string.IsNullOrEmpty(first.DepartmentCode) && _deptLookup.TryGetValue(first.DepartmentCode, out deptName))
                    flat.DepartmentName = deptName;
                if (!string.IsNullOrEmpty(first.DivisionCode) && _divLookup.TryGetValue(first.DivisionCode, out divName))
                    flat.DivisionName = divName;
                foreach (var row in grp)
                {
                    switch (row.ConfidentialityLevel)
                    {
                        case 1: flat.HC_View=row.CanView; flat.HC_Add=row.CanAdd; flat.HC_Edit=row.CanEdit; flat.HC_Approve=row.CanApprove; break;
                        case 2: flat.SC_View=row.CanView; flat.SC_Add=row.CanAdd; flat.SC_Edit=row.CanEdit; flat.SC_Approve=row.CanApprove; break;
                        case 3: flat.NC_View=row.CanView; flat.NC_Add=row.CanAdd; flat.NC_Edit=row.CanEdit; flat.NC_Approve=row.CanApprove; break;
                        case 4: flat.PC_View=row.CanView; flat.PC_Add=row.CanAdd; flat.PC_Edit=row.CanEdit; flat.PC_Approve=row.CanApprove; break;
                    }
                }
                _allConfigs.Add(flat);
            }

            FilterGrid(tvOrg.SelectedNode?.Tag as NodeTag);
        }

        // ── Toolbar ───────────────────────────────────────────────────────────

        private void btnAdd_Click(object sender, EventArgs e)    { _editing = new FolderAuthorityConfig(); ShowEditPanel(false); }
        private void btnClose_Click(object sender, EventArgs e)  { Close(); }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            _editing = (FolderAuthorityConfig)dgv.SelectedRows[0].Tag;
            ShowEditPanel(true);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            var c = (FolderAuthorityConfig)dgv.SelectedRows[0].Tag;
            if (MessageBox.Show(string.Format("ลบ \"{0}\" ใช่หรือไม่?", c.ConfigLabel),
                "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _service.Delete(c.ConfigLabel, _user.UserID);
            LoadData();
        }

        // ── Edit Panel ────────────────────────────────────────────────────────

        private void ShowEditPanel(bool isEdit)
        {
            if (isEdit && _editing != null)
            {
                txtLabel.Text     = _editing.ConfigLabel    ?? "";
                txtPosition.Text  = _editing.PositionName   ?? "";
                txtLevelCode.Text = _editing.LevelCode       ?? "";
                txtDept.Text      = _editing.DepartmentCode ?? "";
                txtDiv.Text       = _editing.DivisionCode   ?? "";
                chkHC_V.Checked=_editing.HC_View; chkHC_A.Checked=_editing.HC_Add; chkHC_E.Checked=_editing.HC_Edit; chkHC_Ap.Checked=_editing.HC_Approve;
                chkSC_V.Checked=_editing.SC_View; chkSC_A.Checked=_editing.SC_Add; chkSC_E.Checked=_editing.SC_Edit; chkSC_Ap.Checked=_editing.SC_Approve;
                chkNC_V.Checked=_editing.NC_View; chkNC_A.Checked=_editing.NC_Add; chkNC_E.Checked=_editing.NC_Edit; chkNC_Ap.Checked=_editing.NC_Approve;
                chkPC_V.Checked=_editing.PC_View; chkPC_A.Checked=_editing.PC_Add; chkPC_E.Checked=_editing.PC_Edit; chkPC_Ap.Checked=_editing.PC_Approve;
            }
            else
            {
                txtLabel.Clear(); txtPosition.Clear(); txtLevelCode.Clear(); txtDept.Clear(); txtDiv.Clear();
                foreach (CheckBox cb in new[] { chkHC_V,chkHC_A,chkHC_E,chkHC_Ap, chkSC_V,chkSC_A,chkSC_E,chkSC_Ap,
                                                chkNC_V,chkNC_A,chkNC_E,chkNC_Ap, chkPC_V,chkPC_A,chkPC_E,chkPC_Ap })
                    cb.Checked = false;
            }
            pnlEdit.Visible = true;
            txtLabel.Focus();
        }

        private void btnSaveRow_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLabel.Text))
            {
                MessageBox.Show("กรุณากรอก Label", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _editing.ConfigLabel    = txtLabel.Text.Trim();
            _editing.PositionName   = string.IsNullOrWhiteSpace(txtPosition.Text)  ? null : txtPosition.Text.Trim();
            _editing.LevelCode      = string.IsNullOrWhiteSpace(txtLevelCode.Text) ? null : txtLevelCode.Text.Trim().ToUpper();
            _editing.DepartmentCode = string.IsNullOrWhiteSpace(txtDept.Text)      ? null : txtDept.Text.Trim();
            _editing.DivisionCode   = string.IsNullOrWhiteSpace(txtDiv.Text)       ? null : txtDiv.Text.Trim();
            _editing.HC_View=chkHC_V.Checked; _editing.HC_Add=chkHC_A.Checked; _editing.HC_Edit=chkHC_E.Checked; _editing.HC_Approve=chkHC_Ap.Checked;
            _editing.SC_View=chkSC_V.Checked; _editing.SC_Add=chkSC_A.Checked; _editing.SC_Edit=chkSC_E.Checked; _editing.SC_Approve=chkSC_Ap.Checked;
            _editing.NC_View=chkNC_V.Checked; _editing.NC_Add=chkNC_A.Checked; _editing.NC_Edit=chkNC_E.Checked; _editing.NC_Approve=chkNC_Ap.Checked;
            _editing.PC_View=chkPC_V.Checked; _editing.PC_Add=chkPC_A.Checked; _editing.PC_Edit=chkPC_E.Checked; _editing.PC_Approve=chkPC_Ap.Checked;

            try
            {
                _service.Delete(_editing.ConfigLabel, _user.UserID);
                foreach (var lv in new[] {
                    new { L=1, C="1HC", V=_editing.HC_View, A=_editing.HC_Add, E=_editing.HC_Edit, Ap=_editing.HC_Approve },
                    new { L=2, C="2SC", V=_editing.SC_View, A=_editing.SC_Add, E=_editing.SC_Edit, Ap=_editing.SC_Approve },
                    new { L=3, C="3NC", V=_editing.NC_View, A=_editing.NC_Add, E=_editing.NC_Edit, Ap=_editing.NC_Approve },
                    new { L=4, C="4PC", V=_editing.PC_View, A=_editing.PC_Add, E=_editing.PC_Edit, Ap=_editing.PC_Approve },
                })
                {
                    _service.Save(new FolderAuthorityConfig
                    {
                        ConfigLabel=_editing.ConfigLabel, PositionName=_editing.PositionName,
                        DepartmentCode=_editing.DepartmentCode, DivisionCode=_editing.DivisionCode,
                        ConfidentialityLevel=lv.L, LevelCode=lv.C,
                        CanView=lv.V, CanAdd=lv.A, CanEdit=lv.E, CanApprove=lv.Ap
                    }, _user.UserID);
                }
                pnlEdit.Visible = false; _editing = null;
                LoadData();
                MessageBox.Show("บันทึกสำเร็จ", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnCancelRow_Click(object sender, EventArgs e) { pnlEdit.Visible = false; _editing = null; }

        private void AuthorityConfigForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }
    }
}
