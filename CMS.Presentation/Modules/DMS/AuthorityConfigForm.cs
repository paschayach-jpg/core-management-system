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
        private readonly IAuthorityImportController _authorityImportController;
        private readonly UserContext                _user;

        private List<FolderAuthorityConfig> _allConfigs = new List<FolderAuthorityConfig>();

        // Lookup สำหรับแสดงชื่อใน grid
        private Dictionary<string, string> _deptLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _divLookup  = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // ชื่ออย่างเดียว (ไม่มีรหัส) สำหรับแสดงใน grid แบบประหยัดพื้นที่
        private Dictionary<string, string> _deptNameLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _divNameLookup  = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // ── Detail panel state ──
        private CheckBox[,]           _permChecks;          // [level 0..3 = HC/SC/NC/PC, perm 0..4 = V/A/E/Ap/Dl]
        private FolderAuthorityConfig _detailConfig;        // row currently bound to the detail panel (saved values)
        private NodeTag               _currentTag;
        private string                _pendingSelectLabel;  // row to select after the next reload
        private bool                  _bindingDetail;
        private bool                  _suppressSelection;
        private bool                  _promptPending;

        private const int LevelColStart = 4;

        public AuthorityConfigForm()
        {
            InitializeComponent();
            InitPermChecks();
        }

        public AuthorityConfigForm(IAuthorityConfigController authorityConfigController, UserContext user,
            IAuthorityOrgLookupController authorityOrgLookupController = null,
            IAuthorityImportController authorityImportController = null)
        {
            if (authorityConfigController == null) throw new ArgumentNullException("authorityConfigController");
            if (user    == null) throw new ArgumentNullException("user");
            _authorityConfigController = authorityConfigController;
            _user             = user;
            _authorityOrgLookupController = authorityOrgLookupController;
            _authorityImportController = authorityImportController;

            InitializeComponent();
            InitPermChecks();
            btnImport.Visible = _authorityImportController != null && _user.AppRole == 4;
            this.Load += (s, e) =>
            {
                SetupGrid();
                tvOrg.AfterSelect += tvOrg_AfterSelect;
                LoadTree();
                LoadData();
            };
        }

        private void InitPermChecks()
        {
            _permChecks = new CheckBox[,]
            {
                { chkHC_V, chkHC_A, chkHC_E, chkHC_Ap, chkHC_Dl },
                { chkSC_V, chkSC_A, chkSC_E, chkSC_Ap, chkSC_Dl },
                { chkNC_V, chkNC_A, chkNC_E, chkNC_Ap, chkNC_Dl },
                { chkPC_V, chkPC_A, chkPC_E, chkPC_Ap, chkPC_Dl },
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

        private void tvOrg_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (_promptPending) return;
            if (!ResolvePendingChanges(null))
                e.Cancel = true;
        }

        private void tvOrg_AfterSelect(object sender, TreeViewEventArgs e)
        {
            FilterGrid(e.Node == null ? null : e.Node.Tag as NodeTag);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterGrid(_currentTag);
        }

        private void FilterGrid(NodeTag tag)
        {
            _currentTag = tag;
            string search = txtSearch.Text.Trim();

            _suppressSelection = true;
            try
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
                    string deptFull, divFull;
                    _deptLookup.TryGetValue(c.DepartmentCode ?? "", out deptFull);
                    _divLookup.TryGetValue(c.DivisionCode   ?? "", out divFull);

                    string posDisplay  = GetDisplayName(c);
                    string deptDisplay = LookupName(_deptNameLookup, c.DepartmentCode);
                    string divDisplay  = LookupName(_divNameLookup,  c.DivisionCode);

                    if (search.Length > 0 && !MatchesSearch(search, posDisplay, c.LevelCode, deptFull ?? deptDisplay, divFull ?? divDisplay, c.ConfigLabel))
                        continue;

                    int idx = dgv.Rows.Add(posDisplay, c.LevelCode, deptDisplay, divDisplay, "", "", "", "");
                    var row = dgv.Rows[idx];
                    row.Tag = c;
                    row.Cells[0].ToolTipText = posDisplay;
                    row.Cells[2].ToolTipText = deptFull ?? deptDisplay;
                    row.Cells[3].ToolTipText = divFull  ?? divDisplay;
                    for (int lv = 0; lv < 4; lv++)
                        row.Cells[LevelColStart + lv].ToolTipText = DescribeLevel(c, lv);
                }

                // ซ่อนคอลัมน์ฝ่ายเมื่อกรองตามฝ่าย/แผนกอยู่แล้ว (ค่าจะซ้ำทุกแถว)
                dgv.Columns["colDiv"].Visible = tag == null
                    || (tag.Type != "DIV" && tag.Type != "DEP" && tag.Type != "POS_IN_DEP");

                string filterText = tag == null
                    ? "แสดง: ทั้งหมด"
                    : tag.Type == "POS_GROUP"
                        ? "แสดง: ตำแหน่งงานทั้งหมด"
                        : tag.Type == "POS_IN_DEP"
                            ? string.Format("แสดง: ตำแหน่ง {0} ใน {1}", tag.Name, tag.ParentCode)
                            : string.Format("แสดง: {0} — {1}", tag.Code, tag.Name);
                lblFilter.Text = string.Format("{0}  ({1} รายการ)", filterText, dgv.Rows.Count);

                RestoreSelection();
            }
            finally
            {
                _suppressSelection = false;
            }

            // Bind the detail panel to the (re)selected row. Unsaved edits are kept even when
            // the edited row is filtered out, so a search never silently drops them.
            if (!IsDetailChanged())
                BindDetail(SelectedConfig());
        }

        private static bool MatchesSearch(string search, params string[] values)
        {
            foreach (var v in values)
                if (!string.IsNullOrEmpty(v) && v.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }

        private static string LookupName(Dictionary<string, string> lookup, string code)
        {
            if (string.IsNullOrEmpty(code)) return "";
            string name;
            return lookup.TryGetValue(code, out name) && !string.IsNullOrEmpty(name) ? name : code;
        }

        /// <summary>ชื่อที่แสดงของ config: ตำแหน่ง → แผนก → ฝ่าย</summary>
        private string GetDisplayName(FolderAuthorityConfig c)
        {
            if (!string.IsNullOrEmpty(c.PositionName))   return c.PositionName;
            if (!string.IsNullOrEmpty(c.DepartmentCode)) return LookupName(_deptNameLookup, c.DepartmentCode);
            if (!string.IsNullOrEmpty(c.DivisionCode))   return LookupName(_divNameLookup, c.DivisionCode);
            return c.ConfigLabel ?? "";
        }

        private void RestoreSelection()
        {
            string target = _pendingSelectLabel ?? (_detailConfig != null ? _detailConfig.ConfigLabel : null);
            _pendingSelectLabel = null;
            if (!SelectRowByLabel(target))
            {
                dgv.ClearSelection();
                dgv.CurrentCell = null;
            }
        }

        private bool SelectRowByLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) return false;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                var c = row.Tag as FolderAuthorityConfig;
                if (c == null || c.ConfigLabel != label) continue;

                bool old = _suppressSelection;
                _suppressSelection = true;
                try
                {
                    dgv.ClearSelection();
                    dgv.CurrentCell = row.Cells[0];
                    row.Selected = true;
                    if (!row.Displayed)
                        dgv.FirstDisplayedScrollingRowIndex = row.Index;
                }
                finally
                {
                    _suppressSelection = old;
                }
                return true;
            }
            return false;
        }

        private FolderAuthorityConfig SelectedConfig()
        {
            return dgv.SelectedRows.Count > 0 ? dgv.SelectedRows[0].Tag as FolderAuthorityConfig : null;
        }

        private static bool SameConfig(FolderAuthorityConfig a, FolderAuthorityConfig b)
        {
            if (a == null || b == null) return a == b;
            return a.ConfigLabel == b.ConfigLabel;
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
                    .FirstOrDefault(d => string.Equals(d.DepartmentCode, departmentCode, StringComparison.OrdinalIgnoreCase));
                if (dept == null) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // ดึง Position จาก HR_PositionMapping
                var names = _authorityOrgLookupController.GetPositionMappingsByDepartmentId(dept.DepartmentID)
                    .Select(m => m.PositionName)
                    .Where(n => !string.IsNullOrEmpty(n));
                return new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
            }
            catch { }
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        // ── Grid Setup ────────────────────────────────────────────────────────

        private sealed class LevelInfo
        {
            public string Code    { get; private set; }
            public string Thai    { get; private set; }
            public string English { get; private set; }
            public Color  Color   { get; private set; }

            public LevelInfo(string code, string thai, string english, Color color)
            {
                Code = code; Thai = thai; English = english; Color = color;
            }
        }

        // index 0..3 = ConfidentialityLevel 1..4
        private static readonly LevelInfo[] _levels =
        {
            new LevelInfo("HC", "สูงสุด", "Highly Confidential",   Color.FromArgb(180, 30,  30)),
            new LevelInfo("SC", "ลับมาก", "Strictly Confidential", Color.FromArgb(200, 100,  0)),
            new LevelInfo("NC", "ปกติ",   "Normal Confidential",   Color.FromArgb(0,   90, 160)),
            new LevelInfo("PC", "ทั่วไป", "Public",                Color.FromArgb(0,  130,  60)),
        };

        private static readonly string[] _permChips = { "ด", "เ", "แ", "อ", "⬇" };
        private static readonly string[] _permNames = { "ดู", "เพิ่ม", "แก้ไข", "อนุมัติ", "ดาวน์โหลดได้ทันที" };

        private static readonly Color _headerColor = Color.FromArgb(31, 56, 100);
        private static readonly Font  _chipFont    = new Font("Segoe UI", 7f);
        private static readonly Font  _hdrCodeFont = new Font("Segoe UI", 9f, FontStyle.Bold);
        private static readonly Font  _hdrSubFont  = new Font("Segoe UI", 7.5f);

        private void SetupGrid()
        {
            dgv.AutoGenerateColumns  = false;
            dgv.AllowUserToAddRows   = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.ReadOnly             = true;
            dgv.MultiSelect          = false;
            dgv.SelectionMode        = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowHeadersVisible    = false;
            dgv.AutoSizeColumnsMode  = DataGridViewAutoSizeColumnsMode.None;
            dgv.BackgroundColor      = Color.White;
            dgv.GridColor            = Color.FromArgb(225, 225, 225);
            dgv.CellBorderStyle      = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.Font                 = new Font("Segoe UI", 9f);
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight  = 40;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = _headerColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            dgv.DefaultCellStyle.SelectionBackColor     = Color.FromArgb(204, 228, 247);
            dgv.DefaultCellStyle.SelectionForeColor     = Color.Black;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 252);
            dgv.ShowCellToolTips = true;

            AddTextCol("colPosition",  "ตำแหน่ง", DataGridViewAutoSizeColumnMode.Fill, 40, 140);
            AddTextCol("colLevelCode", "ระดับ",   DataGridViewAutoSizeColumnMode.None, 0,  52);
            AddTextCol("colDept",      "แผนก",    DataGridViewAutoSizeColumnMode.Fill, 30, 100);
            AddTextCol("colDiv",       "ฝ่าย",    DataGridViewAutoSizeColumnMode.Fill, 25, 100);

            foreach (var lv in _levels)
            {
                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "col" + lv.Code, HeaderText = lv.Code, Width = 100,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    Resizable = DataGridViewTriState.False,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    ReadOnly = true,
                    ToolTipText = string.Format("{0} — {1} ({2})\nด=ดู  เ=เพิ่ม  แ=แก้ไข  อ=อนุมัติ  ⬇=ดาวน์โหลดได้ทันที",
                        lv.Code, lv.Thai, lv.English)
                });
            }

            dgv.CellPainting += Dgv_CellPainting;
        }

        private void AddTextCol(string name, string header, DataGridViewAutoSizeColumnMode mode, int fillWeight, int width)
        {
            var col = new DataGridViewTextBoxColumn
            {
                Name = name, HeaderText = header, AutoSizeMode = mode, ReadOnly = true,
                MinimumWidth = Math.Min(width, 50),
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.False, Padding = new Padding(4, 0, 4, 0) }
            };
            if (mode == DataGridViewAutoSizeColumnMode.Fill)
            {
                col.FillWeight   = fillWeight;
                col.MinimumWidth = width;
            }
            else
            {
                col.Width = width;
            }
            dgv.Columns.Add(col);
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            if (e.RowIndex == -1)
            {
                PaintHeaderCell(e);
                return;
            }

            if (e.ColumnIndex < LevelColStart) return;
            var c = dgv.Rows[e.RowIndex].Tag as FolderAuthorityConfig;
            if (c == null) return;

            int lv = e.ColumnIndex - LevelColStart;
            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border
                                | DataGridViewPaintParts.SelectionBackground);

            var bits = GetLevel(c, lv);
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                if (!bits.Any(b => b))
                {
                    using (var br = new SolidBrush(Color.FromArgb(180, 180, 180)))
                        e.Graphics.DrawString("—", dgv.Font, br, e.CellBounds, sf);
                    e.Handled = true;
                    return;
                }

                const int chip = 16, gap = 2;
                int total = chip * 5 + gap * 4;
                int x = e.CellBounds.X + (e.CellBounds.Width - total) / 2;
                int y = e.CellBounds.Y + (e.CellBounds.Height - chip) / 2;
                var color = _levels[lv].Color;

                for (int i = 0; i < 5; i++)
                {
                    var r = new Rectangle(x + i * (chip + gap), y, chip, chip);
                    if (bits[i])
                    {
                        using (var bg = new SolidBrush(color))
                            e.Graphics.FillRectangle(bg, r);
                        e.Graphics.DrawString(_permChips[i], _chipFont, Brushes.White, r, sf);
                    }
                    else
                    {
                        using (var pen = new Pen(Color.FromArgb(215, 215, 215)))
                            e.Graphics.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
                        using (var br = new SolidBrush(Color.FromArgb(200, 200, 200)))
                            e.Graphics.DrawString(_permChips[i], _chipFont, br, r, sf);
                    }
                }
            }
            e.Handled = true;
        }

        private void PaintHeaderCell(DataGridViewCellPaintingEventArgs e)
        {
            bool isLevel = e.ColumnIndex >= LevelColStart && e.ColumnIndex < LevelColStart + _levels.Length;
            Color bgColor = isLevel ? _levels[e.ColumnIndex - LevelColStart].Color : _headerColor;

            using (var bg = new SolidBrush(bgColor))
                e.Graphics.FillRectangle(bg, e.CellBounds);
            using (var pen = new Pen(Color.FromArgb(70, 90, 130)))
                e.Graphics.DrawLine(pen, e.CellBounds.Right - 1, e.CellBounds.Y, e.CellBounds.Right - 1, e.CellBounds.Bottom);

            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                if (isLevel)
                {
                    var lv = _levels[e.ColumnIndex - LevelColStart];
                    var top = new Rectangle(e.CellBounds.X, e.CellBounds.Y + 2, e.CellBounds.Width, e.CellBounds.Height / 2);
                    var bot = new Rectangle(e.CellBounds.X, e.CellBounds.Y + e.CellBounds.Height / 2 - 1, e.CellBounds.Width, e.CellBounds.Height / 2);
                    e.Graphics.DrawString(lv.Code, _hdrCodeFont, Brushes.White, top, sf);
                    e.Graphics.DrawString(lv.Thai, _hdrSubFont,  Brushes.White, bot, sf);
                }
                else
                {
                    string text = e.Value == null ? "" : e.Value.ToString();
                    e.Graphics.DrawString(text, dgv.ColumnHeadersDefaultCellStyle.Font, Brushes.White, e.CellBounds, sf);
                }
            }
            e.Handled = true;
        }

        private static bool[] GetLevel(FolderAuthorityConfig c, int lv)
        {
            switch (lv)
            {
                case 0:  return new[] { c.HC_View, c.HC_Add, c.HC_Edit, c.HC_Approve, c.HC_Download };
                case 1:  return new[] { c.SC_View, c.SC_Add, c.SC_Edit, c.SC_Approve, c.SC_Download };
                case 2:  return new[] { c.NC_View, c.NC_Add, c.NC_Edit, c.NC_Approve, c.NC_Download };
                default: return new[] { c.PC_View, c.PC_Add, c.PC_Edit, c.PC_Approve, c.PC_Download };
            }
        }

        private static void SetLevel(FolderAuthorityConfig c, int lv, bool[] v)
        {
            switch (lv)
            {
                case 0:  c.HC_View = v[0]; c.HC_Add = v[1]; c.HC_Edit = v[2]; c.HC_Approve = v[3]; c.HC_Download = v[4]; break;
                case 1:  c.SC_View = v[0]; c.SC_Add = v[1]; c.SC_Edit = v[2]; c.SC_Approve = v[3]; c.SC_Download = v[4]; break;
                case 2:  c.NC_View = v[0]; c.NC_Add = v[1]; c.NC_Edit = v[2]; c.NC_Approve = v[3]; c.NC_Download = v[4]; break;
                default: c.PC_View = v[0]; c.PC_Add = v[1]; c.PC_Edit = v[2]; c.PC_Approve = v[3]; c.PC_Download = v[4]; break;
            }
        }

        private static string DescribeLevel(FolderAuthorityConfig c, int lv)
        {
            var bits  = GetLevel(c, lv);
            var names = new List<string>();
            for (int i = 0; i < bits.Length; i++)
                if (bits[i]) names.Add(_permNames[i]);
            return string.Format("{0} {1}: {2}", _levels[lv].Code, _levels[lv].Thai,
                names.Count == 0 ? "ไม่มีสิทธิ์" : string.Join(", ", names));
        }

        // ── Detail panel ──────────────────────────────────────────────────────

        private void BindDetail(FolderAuthorityConfig c)
        {
            _detailConfig = c;
            pnlDetailBody.Visible = c != null;
            lblDetailEmpty.Visible = c == null;
            lblDetailStatus.Text = "";
            if (c == null) return;

            lblDetailTitle.Text = GetDisplayName(c);
            string deptFull, divFull;
            _deptLookup.TryGetValue(c.DepartmentCode ?? "", out deptFull);
            _divLookup.TryGetValue(c.DivisionCode   ?? "", out divFull);
            lblDetailSub.Text = string.Format("ระดับ: {0}\nแผนก: {1}\nฝ่าย: {2}",
                string.IsNullOrEmpty(c.LevelCode) ? "-" : c.LevelCode,
                deptFull ?? (string.IsNullOrEmpty(c.DepartmentCode) ? "-" : c.DepartmentCode),
                divFull  ?? (string.IsNullOrEmpty(c.DivisionCode)   ? "-" : c.DivisionCode));

            _bindingDetail = true;
            try
            {
                for (int lv = 0; lv < 4; lv++)
                {
                    var bits = GetLevel(c, lv);
                    for (int p = 0; p < 5; p++)
                        _permChecks[lv, p].Checked = bits[p];
                }
            }
            finally
            {
                _bindingDetail = false;
            }
            UpdateDetailState();
        }

        private bool[] ReadChecks(int lv)
        {
            var v = new bool[5];
            for (int p = 0; p < 5; p++) v[p] = _permChecks[lv, p].Checked;
            return v;
        }

        private bool IsDetailChanged()
        {
            if (_detailConfig == null) return false;
            for (int lv = 0; lv < 4; lv++)
                if (!GetLevel(_detailConfig, lv).SequenceEqual(ReadChecks(lv))) return true;
            return false;
        }

        private void UpdateDetailState()
        {
            bool changed = IsDetailChanged();
            btnDetailSave.Enabled  = changed;
            btnDetailReset.Enabled = changed;
            if (changed)
                ShowStatus("● มีการเปลี่ยนแปลงที่ยังไม่บันทึก", Color.FromArgb(200, 100, 0));
            else if (lblDetailStatus.ForeColor != Color.FromArgb(0, 130, 60))
                lblDetailStatus.Text = "";
        }

        private void ShowStatus(string text, Color color)
        {
            lblDetailStatus.ForeColor = color;
            lblDetailStatus.Text = text;
        }

        private FolderAuthorityConfig BuildDetailConfig()
        {
            var src = _detailConfig;
            var c = new FolderAuthorityConfig
            {
                ConfigLabel    = src.ConfigLabel,
                PositionName   = src.PositionName,
                DepartmentCode = src.DepartmentCode,
                DivisionCode   = src.DivisionCode,
                LevelCode      = src.LevelCode,
                DepartmentName = src.DepartmentName,
                DivisionName   = src.DivisionName
            };
            for (int lv = 0; lv < 4; lv++)
                SetLevel(c, lv, ReadChecks(lv));
            return c;
        }

        /// <summary>
        /// ถ้ามีการแก้ไขสิทธิ์ค้างอยู่ ถามผู้ใช้ว่าจะบันทึกหรือไม่
        /// คืน false เมื่อผู้ใช้กดยกเลิก (หรือบันทึกไม่สำเร็จ) — caller ต้องหยุดการกระทำนั้น
        /// </summary>
        private bool ResolvePendingChanges(string selectAfterSave)
        {
            if (!IsDetailChanged()) return true;

            var answer = MessageBox.Show(this,
                string.Format("สิทธิ์ของ \"{0}\" ยังไม่ได้บันทึก\nต้องการบันทึกก่อนหรือไม่?", GetDisplayName(_detailConfig)),
                "ยืนยัน", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (answer == DialogResult.Cancel) return false;
            if (answer == DialogResult.Yes)
                return SaveConfig(BuildDetailConfig(), selectAfterSave ?? _detailConfig.ConfigLabel);

            BindDetail(_detailConfig);   // No → discard edits
            return true;
        }

        private void dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (_suppressSelection || _promptPending) return;

            var selected = SelectedConfig();
            if (SameConfig(selected, _detailConfig)) return;

            if (!IsDetailChanged())
            {
                BindDetail(selected);
                return;
            }

            // Unsaved edits: ask once the grid has finished changing the current row
            // (reloading or reselecting inside SelectionChanged is a reentrant call).
            _promptPending = true;
            string keepLabel = _detailConfig.ConfigLabel;
            BeginInvoke((Action)(() =>
            {
                try
                {
                    var target = SelectedConfig();
                    if (!ResolvePendingChanges(target != null ? target.ConfigLabel : null))
                    {
                        SelectRowByLabel(keepLabel);
                        return;
                    }
                    var now = SelectedConfig();
                    if (!SameConfig(now, _detailConfig))
                        BindDetail(now);
                }
                finally
                {
                    _promptPending = false;
                }
            }));
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && pnlDetailBody.Visible)
                _permChecks[0, 0].Focus();
        }

        private void chkPerm_CheckedChanged(object sender, EventArgs e)
        {
            if (_bindingDetail || _permChecks == null) return;
            UpdateDetailState();
        }

        private void SetChecks(Func<int, int, bool> value)
        {
            _bindingDetail = true;
            try
            {
                for (int lv = 0; lv < 4; lv++)
                    for (int p = 0; p < 5; p++)
                        _permChecks[lv, p].Checked = value(lv, p);
            }
            finally
            {
                _bindingDetail = false;
            }
            UpdateDetailState();
        }

        private void lblLevel_Click(object sender, EventArgs e)
        {
            int level = Convert.ToInt32(((Control)sender).Tag);
            bool newValue = !ReadChecks(level).All(b => b);
            SetChecks((lv, p) => lv == level ? newValue : _permChecks[lv, p].Checked);
        }

        private void btnSelectAll_Click(object sender, EventArgs e) { SetChecks((lv, p) => true); }
        private void btnClearAll_Click(object sender, EventArgs e)  { SetChecks((lv, p) => false); }

        private void btnDetailReset_Click(object sender, EventArgs e)
        {
            BindDetail(_detailConfig);
        }

        private void btnDetailSave_Click(object sender, EventArgs e)
        {
            if (_detailConfig == null || !IsDetailChanged()) return;
            SaveConfig(BuildDetailConfig(), _detailConfig.ConfigLabel);
        }

        // ── Load Data ─────────────────────────────────────────────────────────

        private void LoadData()
        {
            // Build lookup: code → display name จาก HR tables (เก็บเป็น field เพื่อใช้ใน FilterGrid ด้วย)
            _deptLookup.Clear();
            _divLookup.Clear();
            _deptNameLookup.Clear();
            _divNameLookup.Clear();

            if (_authorityOrgLookupController != null)
            {
                foreach (var d in _authorityOrgLookupController.GetDepartments())
                    if (!string.IsNullOrEmpty(d.DepartmentCode))
                    {
                        _deptLookup[d.DepartmentCode]     = string.Format("{0} — {1}", d.DepartmentCode, d.DisplayName);
                        _deptNameLookup[d.DepartmentCode] = d.DisplayName;
                    }

                foreach (var d in _authorityOrgLookupController.GetDivisions())
                    if (!string.IsNullOrEmpty(d.DivisionCode))
                    {
                        _divLookup[d.DivisionCode]     = string.Format("{0} — {1}", d.DivisionCode, d.DivisionName);
                        _divNameLookup[d.DivisionCode] = d.DivisionName;
                    }
            }

            var allRows = _authorityConfigController.GetAll().ToList();
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
                    // Job level (e.g. 3A) from any row; rows saved by older code hold the confidentiality
                    // badge ("1HC".."4PC") instead, which must not be shown or saved back as the level.
                    LevelCode      = grp.Select(r => (r.LevelCode ?? "").Trim().ToUpper())
                                        .FirstOrDefault(code => System.Text.RegularExpressions.Regex.IsMatch(code, "^[1-9][A-Z]$")),
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
                        case 1: flat.HC_View=row.CanView; flat.HC_Add=row.CanAdd; flat.HC_Edit=row.CanEdit; flat.HC_Approve=row.CanApprove; flat.HC_Download=row.CanDownload; break;
                        case 2: flat.SC_View=row.CanView; flat.SC_Add=row.CanAdd; flat.SC_Edit=row.CanEdit; flat.SC_Approve=row.CanApprove; flat.SC_Download=row.CanDownload; break;
                        case 3: flat.NC_View=row.CanView; flat.NC_Add=row.CanAdd; flat.NC_Edit=row.CanEdit; flat.NC_Approve=row.CanApprove; flat.NC_Download=row.CanDownload; break;
                        case 4: flat.PC_View=row.CanView; flat.PC_Add=row.CanAdd; flat.PC_Edit=row.CanEdit; flat.PC_Approve=row.CanApprove; flat.PC_Download=row.CanDownload; break;
                    }
                }
                _allConfigs.Add(flat);
            }

            FilterGrid(_currentTag);
        }

        // ── Toolbar ───────────────────────────────────────────────────────────

        private List<string> GetAvailableLevelList()
        {
            var levels = new List<string>();
            if (_authorityOrgLookupController != null)
            {
                try
                {
                    var posLevels = _authorityOrgLookupController.GetPositionLevels();
                    if (posLevels != null && posLevels.Count > 0)
                    {
                        foreach (var pl in posLevels)
                        {
                            if (!string.IsNullOrWhiteSpace(pl.LevelCode))
                            {
                                string code = pl.LevelCode.Trim().ToUpper();
                                if (!levels.Contains(code, StringComparer.OrdinalIgnoreCase))
                                    levels.Add(code);
                            }
                        }
                    }
                }
                catch { }

                if (levels.Count == 0)
                {
                    try
                    {
                        foreach (var p in _authorityOrgLookupController.GetPositions())
                        {
                            string code = !string.IsNullOrWhiteSpace(p.LevelCode) ? p.LevelCode : p.PositionLevel;
                            if (!string.IsNullOrWhiteSpace(code))
                            {
                                code = code.Trim().ToUpper();
                                if (!levels.Contains(code, StringComparer.OrdinalIgnoreCase))
                                    levels.Add(code);
                            }
                        }
                    }
                    catch { }
                }
            }

            if (_allConfigs != null)
            {
                foreach (var c in _allConfigs)
                {
                    if (!string.IsNullOrWhiteSpace(c.LevelCode))
                    {
                        string code = c.LevelCode.Trim().ToUpper();
                        if (!levels.Contains(code, StringComparer.OrdinalIgnoreCase))
                            levels.Add(code);
                    }
                }
            }

            return levels;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ResolvePendingChanges(null)) return;

            var levels = GetAvailableLevelList();
            using (var dialog = new AuthorityConfigEditDialog(new FolderAuthorityConfig(), false, _authorityOrgLookupController, levels))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    SaveConfig(dialog.Config, dialog.Config.ConfigLabel);
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var config = SelectedConfig();
            if (config == null)
            {
                MessageBox.Show("กรุณาเลือกรายการที่ต้องการแก้ไข", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ResolvePendingChanges(null)) return;
            config = SelectedConfig() ?? config;   // a save above reloads the rows

            var levels = GetAvailableLevelList();
            using (var dialog = new AuthorityConfigEditDialog(config, true, _authorityOrgLookupController, levels))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    SaveConfig(dialog.Config, dialog.Config.ConfigLabel);
                }
            }
        }

        /// <summary>บันทึก config (ลบแล้วสร้าง 4 แถวตามระดับความลับ) แล้วโหลดใหม่และเลือกแถว selectLabel</summary>
        private bool SaveConfig(FolderAuthorityConfig config, string selectLabel)
        {
            try
            {
                // LevelCode is the job level chosen in the dialog (e.g. 3A) and is the same for all four rows;
                // it used to be overwritten with the confidentiality badge (1HC..4PC), so the chosen level was lost.
                string levelCode = string.IsNullOrWhiteSpace(config.LevelCode) ? null : config.LevelCode.Trim().ToUpper();

                _authorityConfigController.Delete(config.ConfigLabel, _user.UserID);
                foreach (var lv in new[] {
                    new { L=1, V=config.HC_View, A=config.HC_Add, E=config.HC_Edit, Ap=config.HC_Approve, Dl=config.HC_Download },
                    new { L=2, V=config.SC_View, A=config.SC_Add, E=config.SC_Edit, Ap=config.SC_Approve, Dl=config.SC_Download },
                    new { L=3, V=config.NC_View, A=config.NC_Add, E=config.NC_Edit, Ap=config.NC_Approve, Dl=config.NC_Download },
                    new { L=4, V=config.PC_View, A=config.PC_Add, E=config.PC_Edit, Ap=config.PC_Approve, Dl=config.PC_Download },
                })
                {
                    _authorityConfigController.Save(new FolderAuthorityConfig
                    {
                        ConfigLabel = config.ConfigLabel,
                        PositionName = config.PositionName,
                        DepartmentCode = config.DepartmentCode,
                        DivisionCode = config.DivisionCode,
                        ConfidentialityLevel = lv.L,
                        LevelCode = levelCode,
                        CanView = lv.V,
                        CanAdd = lv.A,
                        CanEdit = lv.E,
                        CanApprove = lv.Ap,
                        CanDownload = lv.Dl
                    }, _user.UserID);
                }

                // Saved values are now the baseline, so the reload binds the target row cleanly.
                _detailConfig = null;
                _pendingSelectLabel = selectLabel;
                LoadData();
                if (_detailConfig != null)
                    ShowStatus(string.Format("✔ บันทึกแล้ว {0:HH:mm:ss}", DateTime.Now), Color.FromArgb(0, 130, 60));
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)  { Close(); }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (_authorityImportController == null) return;
            if (!ResolvePendingChanges(null)) return;
            using (var dlg = new AuthorityImportForm(_authorityImportController, _user))
            {
                dlg.ShowDialog(this);
                if (dlg.ImportCompleted) LoadData();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var c = SelectedConfig();
            if (c == null) return;
            if (MessageBox.Show(string.Format("ลบ \"{0}\" ใช่หรือไม่?", c.ConfigLabel),
                "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _authorityConfigController.Delete(c.ConfigLabel, _user.UserID);
            _detailConfig = null;
            LoadData();
        }

        private void AuthorityConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ResolvePendingChanges(null))
                e.Cancel = true;
        }

        private void AuthorityConfigForm_Load(object sender, EventArgs e)
        {
            // Keep this form fullscreen when opened from menu (MDI or standalone).
            if (this.MdiParent == null)
                this.StartPosition = FormStartPosition.CenterScreen;

            this.WindowState = FormWindowState.Maximized;
        }
    }
}
