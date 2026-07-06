using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Presentation.Modules.ImportFile;

namespace CMS.Presentation
{
    public partial class AuthorityConfigForm : Form
    {
        private readonly IAuthorityConfigController _authorityConfigController;
        private readonly IAuthorityOrgLookupController _authorityOrgLookupController;
        private readonly UserContext                _user;

        private List<FolderAuthorityConfig> _allConfigs = new List<FolderAuthorityConfig>();

        // Lookup สำหรับแสดงชื่อใน grid
        private Dictionary<string, string> _deptLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _divLookup  = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public AuthorityConfigForm()
        {
            InitializeComponent();
        }

        public AuthorityConfigForm(IAuthorityConfigController authorityConfigController, UserContext user,
            IAuthorityOrgLookupController authorityOrgLookupController = null)
        {
            if (authorityConfigController == null) throw new ArgumentNullException("authorityConfigController");
            if (user    == null) throw new ArgumentNullException("user");
            _authorityConfigController = authorityConfigController;
            _user             = user;
            _authorityOrgLookupController = authorityOrgLookupController;

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

            if (_authorityOrgLookupController != null)
            {
                var divs  = _authorityOrgLookupController.GetDivisions().OrderBy(d => d.DivisionCode).ToList();
                var depts = _authorityOrgLookupController.GetDepartments().ToList();

                // Pre-load HR_Position ครั้งเดียว → lookup PositionName → LevelCode
                // ใช้ LevelCode ก่อน ถ้าว่างใช้ Level เดิม
                var posLevelMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in _authorityOrgLookupController.GetPositions())
                {
                        if (!string.IsNullOrEmpty(p.PositionName))
                            posLevelMap[p.PositionName] = !string.IsNullOrEmpty(p.LevelCode) ? p.LevelCode
                                                        : !string.IsNullOrEmpty(p.PositionLevel) ? p.PositionLevel
                                                        : "";
                }

                // Pre-load HR_PositionMapping → DepartmentID → List<PositionName>
                var deptIdPosMap = new Dictionary<int, List<string>>();
                var divIdPosMap  = new Dictionary<int, List<string>>();
                try
                {
                    var allMappings = _authorityOrgLookupController.GetPositionMappings().ToList();
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
                        if (posNames == null)
                        {
                            try { posNames = _authorityOrgLookupController.GetPositionsByDepartmentCode(dep.DepartmentCode); }
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
                    var divCode = tag.Code?.Trim();
                    HashSet<string> deptCodes;
                    HashSet<string> positionNames;
                    GetDepartmentsAndPositionsForDiv(divCode, out deptCodes, out positionNames);

                    filtered = _allConfigs.Where(c =>
                        string.Equals(c.DivisionCode?.Trim(), divCode, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(c.DepartmentCode) && deptCodes.Contains(c.DepartmentCode.Trim())) ||
                        (!string.IsNullOrEmpty(c.PositionName) && positionNames.Contains(c.PositionName.Trim())));
                }
                else if (tag.Type == "DEP")
                {
                    // แสดง config ของ Department + Position ที่สังกัด Department นั้น
                    var deptCode = tag.Code?.Trim();
                    var posInDept = GetPositionNamesForDept(deptCode);
                    filtered = _allConfigs.Where(c =>
                        string.Equals(c.DepartmentCode?.Trim(), deptCode, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(c.PositionName) && posInDept.Contains(c.PositionName.Trim())));
                }
                else if (tag.Type == "POS" || tag.Type == "POS_IN_DEP")
                {
                    filtered = _allConfigs.Where(c => string.Equals(c.PositionName?.Trim(), tag.Code?.Trim(), StringComparison.OrdinalIgnoreCase));
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
            // หา DepartmentID ผ่าน lookup controller
            if (_authorityOrgLookupController == null) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var dept = _authorityOrgLookupController.GetDepartments()
                    .FirstOrDefault(d => string.Equals(d.DepartmentCode?.Trim(), departmentCode?.Trim(), StringComparison.OrdinalIgnoreCase));
                if (dept == null) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // ดึง Position จาก HR_PositionMapping
                var names = _authorityOrgLookupController.GetPositionMappingsByDepartmentId(dept.DepartmentID)
                    .Select(m => m.PositionName?.Trim())
                    .Where(n => !string.IsNullOrEmpty(n));
                return new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
            }
            catch { }
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private void GetDepartmentsAndPositionsForDiv(string divisionCode, out HashSet<string> deptCodes, out HashSet<string> positionNames)
        {
            deptCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            positionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (_authorityOrgLookupController == null) return;
            try
            {
                var div = _authorityOrgLookupController.GetDivisions()
                    .FirstOrDefault(d => string.Equals(d.DivisionCode?.Trim(), divisionCode?.Trim(), StringComparison.OrdinalIgnoreCase));
                if (div == null) return;

                var depts = _authorityOrgLookupController.GetDepartments()
                    .Where(d => d.DivisionID == div.DivisionID)
                    .ToList();

                foreach (var dep in depts)
                {
                    if (!string.IsNullOrEmpty(dep.DepartmentCode))
                    {
                        var depTrim = dep.DepartmentCode.Trim();
                        deptCodes.Add(depTrim);

                        // ดึง Position ของแผนกนี้
                        var posInDept = GetPositionNamesForDept(depTrim);
                        foreach (var pos in posInDept)
                            positionNames.Add(pos.Trim());
                    }
                }

                // ดึง Position ที่อยู่ใน Division โดยตรง (ไม่สังกัด Dept)
                var mappingsInDiv = _authorityOrgLookupController.GetPositionMappings()
                    .Where(m => m.DivisionID == div.DivisionID && (!m.DepartmentID.HasValue || m.DepartmentID == 0))
                    .Select(m => m.PositionName?.Trim())
                    .Where(n => !string.IsNullOrEmpty(n));
                foreach (var pos in mappingsInDiv)
                    positionNames.Add(pos);
            }
            catch { }
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

        private sealed class HeaderGroup
        {
            public int Start { get; private set; }
            public int Count { get; private set; }
            public string Label { get; private set; }
            public string SubLabel { get; private set; }
            public Color Color { get; private set; }

            public HeaderGroup(int start, int count, string label, string subLabel, Color color)
            {
                Start = start;
                Count = count;
                Label = label;
                SubLabel = subLabel;
                Color = color;
            }
        }

        private static readonly HeaderGroup[] _groups =
        {
            new HeaderGroup(4,  4, "HC", "สูงสุด (Highly Confidential)",      Color.FromArgb(180, 30,  30)),
            new HeaderGroup(8,  4, "SC", "ลับมาก (Strictly Confidential)",    Color.FromArgb(200, 100,  0)),
            new HeaderGroup(12, 4, "NC", "ปกติ (Normal Confidential)",        Color.FromArgb(0,   90, 160)),
            new HeaderGroup(16, 4, "PC", "ทั่วไป (Public)",                   Color.FromArgb(0,  130,  60)),
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
                if (e.ColumnIndex >= g.Start && e.ColumnIndex < g.Start + g.Count)
                {
                    using (var bg = new SolidBrush(g.Color))
                        e.Graphics.FillRectangle(bg, topRect);

                    // Draw label every column — position text relative to the full group rect
                    // so each cell contributes its slice (natural cell clip handles the rest)
                    int xOffset  = Enumerable.Range(g.Start, e.ColumnIndex - g.Start).Sum(i => dgv.Columns[i].Width);
                    int totalW   = Enumerable.Range(g.Start, g.Count).Sum(i => dgv.Columns[i].Width);
                    var groupRect = new Rectangle(e.CellBounds.X - xOffset, e.CellBounds.Y, totalW, groupH);

                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    using (var br = new SolidBrush(Color.White))
                    {
                        // Line 1: code (bold)
                        var topLine = new Rectangle(groupRect.X, groupRect.Y, groupRect.Width, groupH / 2);
                        using (var f = new Font("Segoe UI", 9f, FontStyle.Bold))
                            e.Graphics.DrawString(g.Label, f, br, topLine, sf);

                        // Line 2: full name
                        var botLine = new Rectangle(groupRect.X, groupRect.Y + groupH / 2, groupRect.Width, groupH / 2);
                        using (var f = new Font("Segoe UI", 7f))
                            e.Graphics.DrawString(g.SubLabel, f, br, botLine, sf);
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

            if (_authorityOrgLookupController != null)
            {
                foreach (var d in _authorityOrgLookupController.GetDepartments())
                    if (!string.IsNullOrEmpty(d.DepartmentCode))
                        _deptLookup[d.DepartmentCode.Trim()] = string.Format("{0} — {1}", d.DepartmentCode.Trim(), d.DisplayName?.Trim());

                foreach (var d in _authorityOrgLookupController.GetDivisions())
                    if (!string.IsNullOrEmpty(d.DivisionCode))
                        _divLookup[d.DivisionCode.Trim()] = string.Format("{0} — {1}", d.DivisionCode.Trim(), d.DivisionName?.Trim());
            }

            var allRows = _authorityConfigController.GetAll().ToList();
            _allConfigs = new List<FolderAuthorityConfig>();

            foreach (var grp in allRows.GroupBy(r => (r.ConfigLabel ?? "").Trim()))
            {
                var first = grp.First();

                // PositionName เก็บค่าจริงจาก DB เท่านั้น
                // การแสดงผลใน grid จัดการใน FilterGrid โดยใช้ _deptLookup/_divLookup
                string displayLabel = first.PositionName?.Trim();
                if (string.IsNullOrEmpty(displayLabel) && !string.IsNullOrEmpty(first.DepartmentCode))
                    _deptLookup.TryGetValue(first.DepartmentCode.Trim(), out displayLabel);
                if (string.IsNullOrEmpty(displayLabel) && !string.IsNullOrEmpty(first.DivisionCode))
                    _divLookup.TryGetValue(first.DivisionCode.Trim(), out displayLabel);
                if (string.IsNullOrEmpty(displayLabel))
                    displayLabel = grp.Key;

                var flat = new FolderAuthorityConfig
                {
                    ConfigLabel    = grp.Key,
                    PositionName   = first.PositionName?.Trim(),
                    LevelCode      = first.LevelCode?.Trim(),
                    DepartmentCode = first.DepartmentCode?.Trim(),
                    DivisionCode   = first.DivisionCode?.Trim()
                };

                // Enrich display names
                string deptName, divName;
                if (!string.IsNullOrEmpty(first.DepartmentCode) && _deptLookup.TryGetValue(first.DepartmentCode.Trim(), out deptName))
                    flat.DepartmentName = deptName;
                if (!string.IsNullOrEmpty(first.DivisionCode) && _divLookup.TryGetValue(first.DivisionCode.Trim(), out divName))
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new AuthorityConfigEditDialog(new FolderAuthorityConfig(), false))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    SaveConfig(dialog.Config);
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการที่ต้องการแก้ไข", "แจ้งเตือน", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var config = (FolderAuthorityConfig)dgv.SelectedRows[0].Tag;
            using (var dialog = new AuthorityConfigEditDialog(config, true))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    SaveConfig(dialog.Config);
                }
            }
        }

        private void SaveConfig(FolderAuthorityConfig config)
        {
            try
            {
                _authorityConfigController.Delete(config.ConfigLabel, _user.UserID);
                foreach (var lv in new[] {
                    new { L=1, C="1HC", V=config.HC_View, A=config.HC_Add, E=config.HC_Edit, Ap=config.HC_Approve },
                    new { L=2, C="2SC", V=config.SC_View, A=config.SC_Add, E=config.SC_Edit, Ap=config.SC_Approve },
                    new { L=3, C="3NC", V=config.NC_View, A=config.NC_Add, E=config.NC_Edit, Ap=config.NC_Approve },
                    new { L=4, C="4PC", V=config.PC_View, A=config.PC_Add, E=config.PC_Edit, Ap=config.PC_Approve },
                })
                {
                    _authorityConfigController.Save(new FolderAuthorityConfig
                    {
                        ConfigLabel = config.ConfigLabel,
                        PositionName = config.PositionName,
                        DepartmentCode = config.DepartmentCode,
                        DivisionCode = config.DivisionCode,
                        ConfidentialityLevel = lv.L,
                        LevelCode = lv.C,
                        CanView = lv.V,
                        CanAdd = lv.A,
                        CanEdit = lv.E,
                        CanApprove = lv.Ap
                    }, _user.UserID);
                }
                LoadData();
                MessageBox.Show("บันทึกสำเร็จ", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ผิดพลาด", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)  { Close(); }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            var c = (FolderAuthorityConfig)dgv.SelectedRows[0].Tag;
            if (MessageBox.Show(string.Format("ลบ \"{0}\" ใช่หรือไม่?", c.ConfigLabel),
                "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _authorityConfigController.Delete(c.ConfigLabel, _user.UserID);
            LoadData();
        }

        // ── Dummy event handlers for Designer (not used anymore) ─────────────
        private void btnSaveRow_Click(object sender, EventArgs e) { }
        private void btnCancelRow_Click(object sender, EventArgs e) { }

        private void AuthorityConfigForm_Load(object sender, EventArgs e)
        {
            // Keep this form fullscreen when opened from menu (MDI or standalone).
            if (this.MdiParent == null)
                this.StartPosition = FormStartPosition.CenterScreen;

            this.WindowState = FormWindowState.Maximized;
            
            // Hide old edit panel (now using dialog)
            if (pnlEdit != null)
                pnlEdit.Visible = false;
        }
    }
}
