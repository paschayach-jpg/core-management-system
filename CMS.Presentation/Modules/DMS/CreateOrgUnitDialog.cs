using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation
{
    internal partial class CreateOrgUnitDialog : Form
    {
        private readonly List<OrgUnit>           _allUnits;
        private readonly IEmployeeDirectoryController _employeeDirectoryController;
        private readonly OrgUnitTier              _fixedTier;
        private readonly OrgUnit                  _selectedParent;
        private readonly IAuthorityOrgLookupController _authorityOrgLookupController;

        private int           _selectedDivisionID = 0;
        private OrgUnitData   _pendingDivision;
        private List<OrgUnitData> _pendingDepartments;

        public CreateOrgUnitRequest       Result            { get; private set; }
        public List<CreateOrgUnitRequest> BatchResults      { get; private set; }
        public HierarchicalBatchResult    HierarchicalResult { get; private set; }

        private List<OrgUnitData> _batchSelections;

        public class HierarchicalBatchResult
        {
            public CreateOrgUnitRequest       DivisionRequest    { get; set; }
            public List<CreateOrgUnitRequest> DepartmentRequests { get; set; }
        }

        public CreateOrgUnitDialog(IEnumerable<OrgUnit> allUnits,
            IEmployeeDirectoryController employeeDirectoryController = null,
            OrgUnitTier? fixedTier = null,
            OrgUnit selectedParent = null,
            IAuthorityOrgLookupController authorityOrgLookupController = null)
        {
            // Include ALL units (active, inactive, deleted) to avoid code conflicts
            _allUnits              = allUnits?.ToList() ?? new List<OrgUnit>();
            _employeeDirectoryController = employeeDirectoryController;
            _fixedTier             = fixedTier ?? OrgUnitTier.SubCommittee;
            _selectedParent        = selectedParent;
            _authorityOrgLookupController = authorityOrgLookupController;

            // Debug: Show total units received
            System.Diagnostics.Debug.WriteLine($"CreateOrgUnitDialog received {_allUnits.Count} units");
            var divCodes = _allUnits.Where(u => u.OrgUnitCode != null && u.OrgUnitCode.StartsWith("DIV-"))
                                   .Select(u => $"{u.OrgUnitCode} (Active:{u.IsActive}, Deleted:{u.IsDeleted})")
                                   .ToList();
            if (divCodes.Any())
            {
                System.Diagnostics.Debug.WriteLine($"Existing DIV codes: {string.Join(", ", divCodes)}");
            }

            InitializeComponent();
            SetupGoldBorderEffect(); // เพิ่ม gold border effect

            cboTier.Items.Clear();
            cboTier.Items.Add(OrgUnitTier.SubCommittee);
            cboTier.Items.Add(OrgUnitTier.Division);
            cboTier.Items.Add(OrgUnitTier.Department);

            if (fixedTier.HasValue)
            {
                cboTier.SelectedItem = fixedTier.Value;
                cboTier.Enabled      = false;
                RefreshParentList(fixedTier.Value);
                ApplyTierUiRules(fixedTier.Value);
            }
            else
            {
                cboTier.SelectedIndex = 0;
                var initialTier = (OrgUnitTier)cboTier.Items[0];
                RefreshParentList(initialTier);
                ApplyTierUiRules(initialTier);
            }
            
            // Add real-time validation for code field
            txtCode.TextChanged += TxtCode_TextChanged;
            
            // Add event handler for tier changes
            cboTier.SelectedIndexChanged += OnTierChanged;
            
            // Add event handler for parent selection changes
            cboParent.SelectedIndexChanged += OnParentChanged;

            // เปิดตัวเลือก ฝ่าย→แผนก ทันทีเมื่อสร้างฝ่าย (ผู้ใช้ยังกด "ค้นหา" เพื่อเปิดซ้ำได้)
            // เลือกฝ่ายในต้นไม้แล้วสร้างแผนก (Div → Dep) ก็เปิดตัวเลือกแผนกทันทีเช่นกัน
            bool autoPick = fixedTier == OrgUnitTier.Division ||
                            (fixedTier == OrgUnitTier.Department &&
                             selectedParent != null && selectedParent.Tier == OrgUnitTier.Division);
            if (autoPick &&
                (_authorityOrgLookupController != null || _employeeDirectoryController != null))
            {
                Shown += (s, e) => OnSearchNameClick(this, EventArgs.Empty);
            }
        }

        // ── Gold Border Effect ────────────────────────────────────────────────

        private void SetupGoldBorderEffect()
        {
            // เพิ่ม Paint event สำหรับ gradient effect
            pnlGoldBorder.Paint += PnlGoldBorder_Paint;
        }

        private void PnlGoldBorder_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null) return;

            // สร้าง gradient brush สีทองหรูหรา
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(0, 0, panel.Width, panel.Height),
                Color.FromArgb(255, 223, 0),    // ทองสว่าง (Gold)
                Color.FromArgb(184, 134, 11),   // ทองเข้ม (DarkGoldenrod)
                System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, 0, 0, panel.Width, panel.Height);
            }

            // เพิ่ม highlight ด้านบน
            using (var highlightPen = new Pen(Color.FromArgb(255, 235, 59), 1))
            {
                e.Graphics.DrawLine(highlightPen, 0, 0, panel.Width, 0);
            }
        }

        /// <summary>ปรับ UI ตาม tier</summary>
        private void ApplyTierUiRules(OrgUnitTier tier)
        {
            switch (tier)
            {
                case OrgUnitTier.SubCommittee:
                    lblHeader.Text      = "สร้างหน่วยงานใหม่\n🏢  กำลังสร้าง SubCommittee  (ระดับสูงสุด - ไม่มีหน่วยงานแม่)";
                    btnSearchTH.Visible = false;
                    btnSearchEN.Visible = false;
                    break;
                case OrgUnitTier.Division:
                    lblHeader.Text      = "สร้างหน่วยงานใหม่\n🏬  กำลังสร้างฝ่าย  (สามารถเลือกสังกัด SubCommittee หรือเป็นอิสระได้)";
                    btnSearchTH.Visible = true;
                    btnSearchEN.Visible = false;
                    break;
                default: // Department
                    lblHeader.Text      = "สร้างหน่วยงานใหม่\n🏪  กำลังสร้างแผนก  (ต้องสังกัดฝ่าย)";
                    btnSearchTH.Visible = true;
                    btnSearchEN.Visible = false;
                    break;
            }
            lblHeader.Font   = new Font("Segoe UI", 9f, FontStyle.Regular);
            pnlHeader.Height = 56;

            // Auto-generate รหัสจากจำนวน OrgUnit ที่มีอยู่ใน DB
            txtCode.Text = GenerateCode(tier);
        }

        /// <summary>
        /// SubCommittee → auto-generate SC-001, SC-002, ... ต่อเนื่อง
        /// Division     → ใช้ DivisionCode จาก HR_Division (user เลือกจาก dialog)
        /// Department   → ใช้ DepartmentCode จาก HR_Department (user เลือกจาก dialog)
        /// </summary>
        private string GenerateCode(OrgUnitTier tier)
        {
            if (tier == OrgUnitTier.Division || tier == OrgUnitTier.Department)
            {
                // ให้ user เลือกจาก HR table ผ่าน dialog "ค้นหา" — คืนค่าว่างไว้ก่อน
                return string.Empty;
            }

            // SubCommittee → auto-generate SC-NNN ต่อเนื่อง
            const string prefix = "SC";
            List<OrgUnit> allUnits = _allUnits;

            var existingNumbers = allUnits
                .Where(u => u.OrgUnitCode != null && u.OrgUnitCode.StartsWith(prefix + "-"))
                .Select(u =>
                {
                    var parts = u.OrgUnitCode.Split('-');
                    int n;
                    return parts.Length == 2 && int.TryParse(parts[1], out n) ? n : 0;
                })
                .Where(n => n > 0)
                .ToList();

            int next = existingNumbers.Count > 0 ? existingNumbers.Max() + 1 : 1;
            return string.Format("{0}-{1:D3}", prefix, next);
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void TxtCode_TextChanged(object sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            if (string.IsNullOrWhiteSpace(code)) { txtCode.BackColor = Color.White; txtCode.ForeColor = Color.Black; return; }

            // แสดงสีแดงเฉพาะ SubCommittee เท่านั้น
            var currentTier = cboTier.SelectedItem is OrgUnitTier t ? t : OrgUnitTier.SubCommittee;
            if (currentTier != OrgUnitTier.SubCommittee)
            {
                txtCode.BackColor = Color.White;
                txtCode.ForeColor = Color.Black;
                return;
            }

            List<OrgUnit> allUnits = _allUnits;

            var exists = allUnits.Any(u => u.OrgUnitCode == code && u.IsActive && !u.IsDeleted);
            txtCode.BackColor = exists ? Color.FromArgb(255, 235, 235) : Color.White;
            txtCode.ForeColor = exists ? Color.FromArgb(220, 53, 69)   : Color.Black;
        }

        private void OnTierChanged(object sender, EventArgs e)
        {
            if (cboTier.SelectedItem == null) return;
            _pendingDivision    = null;
            _pendingDepartments = null;
            var selectedTier = (OrgUnitTier)cboTier.SelectedItem;
            RefreshParentList(selectedTier);
            ApplyTierUiRules(selectedTier);
        }

        private void OnParentChanged(object sender, EventArgs e)
        {
            // ไม่ต้อง lock tier เมื่อเลือก parent - ให้ผู้ใช้เลือกเองได้
            // เพียงแค่ตรวจสอบความถูกต้องเมื่อกด OK
        }

        private void OnSearchNameClick(object sender, EventArgs e)
        {
            var currentTier = cboTier.SelectedItem != null ? (OrgUnitTier)cboTier.SelectedItem : OrgUnitTier.Division;
            bool   isDivision = currentTier == OrgUnitTier.Division;

            // ต้องมี data source อย่างน้อย 1 อย่าง
            if (isDivision && _authorityOrgLookupController == null && _employeeDirectoryController == null)
            {
                MessageBox.Show("ไม่มีแหล่งข้อมูลฝ่าย กรุณาตรวจสอบการตั้งค่า",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!isDivision && _authorityOrgLookupController == null && _employeeDirectoryController == null)
            {
                MessageBox.Show("ไม่มีแหล่งข้อมูลแผนก กรุณาตรวจสอบการตั้งค่า",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isDivision)
                ShowDivisionTreePicker();
            else
                ShowDepartmentFlatPicker();
        }

        // ── Division Tree Picker (Approach A) ────────────────────────────────
        // แสดง TreeView: Division (root) → Departments (children, auto-checked)
        // User เลือก 1 Division เท่านั้น สามารถ uncheck Dept ที่ไม่ต้องการออกได้
        private void ShowDivisionTreePicker()
        {
            if (_authorityOrgLookupController == null && _employeeDirectoryController == null)
            {
                MessageBox.Show("ไม่มีแหล่งข้อมูลฝ่าย กรุณาตรวจสอบการตั้งค่า",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // โหลด Divisions และ Departments ทั้งหมด (grouped by DivisionID)
            List<HrDivision>   hrDivisions   = null;
            List<HrDepartment> hrDepartments = null;

            try
            {
                if (_authorityOrgLookupController != null)
                {
                    hrDivisions   = _authorityOrgLookupController.GetDivisions()
                                       .OrderBy(d => d.DivisionCode).ToList();
                    hrDepartments = _authorityOrgLookupController.GetDepartments()
                                       .OrderBy(d => d.DepartmentCode).ToList();
                }
                else
                {
                    hrDivisions = _employeeDirectoryController.GetDivisions()
                                      .Select(d => new HrDivision {
                                          DivisionID   = 0,
                                          DivisionCode = d.DivisionCode,
                                          DivisionName = d.NameTh
                                      }).OrderBy(d => d.DivisionCode).ToList();
                    hrDepartments = new List<HrDepartment>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // หา parent context เพื่อตรวจสอบรายการที่มีแล้ว
            int? subCommitteeId = null;
            string scName = "สังกัดนี้";
            if (cboParent.SelectedItem is OrgUnitItem pi && pi.OrgUnit != null)
            {
                subCommitteeId = pi.OrgUnit.OrgUnitId;
                scName = pi.OrgUnit.OrgUnitNameTH ?? pi.OrgUnit.Name;
            }

            // สร้าง lookup sets ของ code และ name ที่มีอยู่แล้วใต้ SubCommittee นี้
            // (match ทั้ง OrgUnitCode และ OrgUnitNameTH เพื่อรองรับกรณีที่ code ไม่ตรงกับ HR)
            var existingDivCodes = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var existingDivNames = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (subCommitteeId.HasValue)
            {
                foreach (var eu in _allUnits)
                {
                    if (!eu.IsActive || eu.IsDeleted) continue;
                    if (eu.Tier != OrgUnitTier.Division) continue;
                    if (eu.ParentId != subCommitteeId.Value) continue;
                    if (!string.IsNullOrEmpty(eu.OrgUnitCode))
                        existingDivCodes.Add(eu.OrgUnitCode.Trim());
                    if (!string.IsNullOrEmpty(eu.OrgUnitNameTH))
                        existingDivNames.Add(eu.OrgUnitNameTH.Trim());
                    if (!string.IsNullOrEmpty(eu.OrgUnitNameEN))
                        existingDivNames.Add(eu.OrgUnitNameEN.Trim());
                }
            }

            var deptsByDivId = hrDepartments
                .GroupBy(d => d.DivisionID)
                .ToDictionary(g => g.Key, g => g.ToList());

            using (var dlg = new Form())
            {
                dlg.Text            = "เลือกฝ่ายและแผนก";
                dlg.ClientSize      = new System.Drawing.Size(560, 480);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;

                // ── Header ────────────────────────────────────────────────────
                var pnlHdr = new Panel { Dock = DockStyle.Top, Height = 36,
                    BackColor = Color.FromArgb(0, 51, 102) };
                pnlHdr.Controls.Add(new Label {
                    Text      = "🔍  เลือกฝ่าย — แผนกในฝ่ายที่เลือกจะถูกเพิ่มอัตโนมัติ",
                    Dock      = DockStyle.Fill,
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(10, 0, 0, 0)
                });

                // ── Footer ────────────────────────────────────────────────────
                var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 48,
                    BackColor = Color.FromArgb(243, 243, 243) };

                var btnOk = new Button {
                    Text      = "✔ สร้าง",
                    Size      = new System.Drawing.Size(110, 30),
                    BackColor = Color.FromArgb(0, 51, 102),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Enabled   = false
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCnl = new Button {
                    Text        = "ยกเลิก",
                    Size        = new System.Drawing.Size(80, 30),
                    BackColor   = Color.White,
                    ForeColor   = Color.FromArgb(80, 80, 80),
                    FlatStyle   = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel
                };
                btnCnl.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 190);

                var lblSummary = new Label {
                    Text      = "ไม่พบฝ่าย? กด 'ยกเลิก' แล้วพิมพ์รหัส/ชื่อเอง",
                    AutoSize  = false,
                    Font      = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(100, 100, 100),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock      = DockStyle.Left,
                    Width     = 300,
                    Padding   = new Padding(8, 0, 0, 0)
                };

                pnlFooter.Controls.Add(lblSummary);
                pnlFooter.Controls.Add(btnOk);
                pnlFooter.Controls.Add(btnCnl);
                pnlFooter.Resize += (s, e2) => {
                    btnCnl.Left = pnlFooter.ClientSize.Width - btnCnl.Width - 8;
                    btnOk.Left  = btnCnl.Left - btnOk.Width - 8;
                    btnOk.Top   = btnCnl.Top = (pnlFooter.Height - btnOk.Height) / 2;
                };

                // ── Search box ────────────────────────────────────────────────
                var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 36,
                    BackColor = Color.White, Padding = new Padding(6, 4, 6, 4) };
                bool isPlaceholder = true;
                var txtSearch = new TextBox {
                    Dock      = DockStyle.Fill,
                    Font      = new Font("Segoe UI", 10f),
                    ForeColor = Color.Gray,
                    Text      = "ค้นหาฝ่าย/แผนก..."
                };
                txtSearch.GotFocus  += (s, e2) => {
                    if (!isPlaceholder) return;
                    isPlaceholder       = false;
                    txtSearch.Text      = "";
                    txtSearch.ForeColor = Color.Black;
                };
                txtSearch.LostFocus += (s, e2) => {
                    if (!string.IsNullOrWhiteSpace(txtSearch.Text)) return;
                    isPlaceholder       = true;
                    txtSearch.Text      = "ค้นหาฝ่าย/แผนก...";
                    txtSearch.ForeColor = Color.Gray;
                };
                pnlSearch.Controls.Add(txtSearch);

                // ── TreeView ──────────────────────────────────────────────────
                var tv = new TreeView {
                    Dock          = DockStyle.Fill,
                    CheckBoxes    = true,
                    Font          = new Font("Segoe UI", 9.5f),
                    ShowLines     = true,
                    ShowPlusMinus = true,
                    FullRowSelect = false
                };

                // สร้าง TreeNode สำหรับแต่ละ Division
                foreach (var div in hrDivisions)
                {
                    bool divAlreadyExists = subCommitteeId.HasValue && (
                        (!string.IsNullOrEmpty(div.DivisionCode) && existingDivCodes.Contains(div.DivisionCode.Trim())) ||
                        (!string.IsNullOrEmpty(div.DivisionName) && existingDivNames.Contains(div.DivisionName.Trim())));

                    var divNode = new TreeNode(
                        string.Format("{0}  {1}{2}",
                            div.DivisionCode ?? "",
                            div.DivisionName ?? "",
                            divAlreadyExists ? "  (มีอยู่ใต้ " + scName + " แล้ว — เลือกไม่ได้)" : ""))
                    {
                        Tag = new OrgUnitData {
                            Type = "ฝ่าย",
                            Code = div.DivisionCode,
                            NameTh = div.DivisionName,
                            AlreadyExistsInParent = divAlreadyExists,
                            DivisionId = div.DivisionID
                        }
                    };
                    if (divAlreadyExists)
                    {
                        divNode.ForeColor = Color.FromArgb(150, 150, 150);
                        divNode.BackColor = Color.FromArgb(245, 245, 245);
                    }

                    // เพิ่ม Department child nodes
                    List<HrDepartment> childDepts;
                    if (deptsByDivId.TryGetValue(div.DivisionID, out childDepts))
                    {
                        foreach (var dept in childDepts)
                        {
                            var deptName = !string.IsNullOrEmpty(dept.DepartmentNameTh)
                                ? dept.DepartmentNameTh
                                : dept.DepartmentName;

                            var deptNode = new TreeNode(
                                string.Format("  {0}  {1}", dept.DepartmentCode ?? "", deptName ?? ""))
                            {
                                Tag = new OrgUnitData {
                                    Type   = "แผนก",
                                    Code   = dept.DepartmentCode,
                                    NameTh = deptName,
                                    AlreadyExistsInParent = false,
                                    DivisionId = div.DivisionID
                                }
                            };
                            divNode.Nodes.Add(deptNode);
                        }
                    }

                    tv.Nodes.Add(divNode);
                }

                // ── BeforeCheck: ป้องกันการเลือกรายการที่มีอยู่แล้ว (ก่อน AfterCheck) ──
                tv.BeforeCheck += (s, e2) => {
                    if (e2.Node.Level == 0)
                    {
                        var data = e2.Node.Tag as OrgUnitData;
                        if (data != null && data.AlreadyExistsInParent && e2.Node.Checked == false)
                        {
                            e2.Cancel = true;
                            MessageBox.Show(
                                string.Format("ฝ่าย '{0}' มีอยู่ในสังกัดนี้แล้ว ไม่สามารถเลือกซ้ำได้", data.Code),
                                "พบข้อมูลซ้ำ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                };

                // ── TreeView check logic: single Division, cascade to Departments ──
                bool suppressCheck = false;

                Action updateSummary = () => {
                    var sel = tv.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Checked);
                    if (sel == null)
                    {
                        lblSummary.Text      = "ไม่พบฝ่าย? กด 'ยกเลิก' แล้วพิมพ์รหัส/ชื่อเอง";
                        lblSummary.ForeColor = Color.FromArgb(100, 100, 100);
                        btnOk.Text           = "✔ สร้าง";
                        btnOk.Enabled        = false;
                        return;
                    }
                    int checkedDeptCount = sel.Nodes.Cast<TreeNode>().Count(n => n.Checked);
                    int totalDeptCount   = sel.Nodes.Count;
                    var selData = sel.Tag as OrgUnitData;

                    lblSummary.Text = string.Format("{0} + {1}/{2} แผนก",
                        selData?.Code ?? sel.Text,
                        checkedDeptCount,
                        totalDeptCount);
                    lblSummary.ForeColor = Color.FromArgb(0, 100, 0);
                    btnOk.Text = string.Format("✔ สร้าง {0} หน่วยงาน",
                        1 + checkedDeptCount);
                    btnOk.Enabled = true;
                };

                tv.AfterCheck += (s, e2) => {
                    if (suppressCheck || e2.Action == TreeViewAction.Unknown) return;
                    suppressCheck = true;
                    try
                    {
                        if (e2.Node.Level == 0) // Division node
                        {
                            var divData = e2.Node.Tag as OrgUnitData;
                            if (divData != null && divData.AlreadyExistsInParent && e2.Node.Checked)
                            {
                                e2.Node.Checked = false;
                                MessageBox.Show(
                                    string.Format("ฝ่าย {0} มีอยู่ในสังกัดนี้แล้ว", divData.Code),
                                    "พบข้อมูลซ้ำ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            // Single-Division enforcement: uncheck previous selection
                            foreach (TreeNode other in tv.Nodes)
                            {
                                if (other == e2.Node || !other.Checked) continue;
                                other.Checked = false;
                                foreach (TreeNode child in other.Nodes)
                                    child.Checked = false;
                                other.Collapse();
                            }

                            // Cascade check/uncheck to all child Departments
                            foreach (TreeNode child in e2.Node.Nodes)
                                child.Checked = e2.Node.Checked;

                            if (e2.Node.Checked)
                                e2.Node.Expand();
                            else
                                e2.Node.Collapse();
                        }
                        // Department level: no cascade needed, just update summary
                        updateSummary();
                    }
                    finally { suppressCheck = false; }
                };

                // Search: filter Division nodes (collapse/hide non-matching)
                txtSearch.TextChanged += (s, e2) => {
                    if (isPlaceholder) return;
                    string kw = (txtSearch.Text ?? "").Trim().ToLower();
                    tv.BeginUpdate();
                    foreach (TreeNode divNode in tv.Nodes)
                    {
                        bool divMatch = string.IsNullOrEmpty(kw) ||
                                        divNode.Text.ToLower().Contains(kw);
                        bool anyChildMatch = divNode.Nodes.Cast<TreeNode>()
                                                .Any(cn => cn.Text.ToLower().Contains(kw));
                        divNode.ForeColor = divNode.Checked
                            ? Color.FromArgb(0, 51, 102)
                            : (divNode.Tag is OrgUnitData od && od.AlreadyExistsInParent)
                                ? Color.DarkGray
                                : Color.Black;
                        // ซ่อน node ที่ไม่ match โดย set ForeColor = BackColor เป็น trick
                        // (WinForms TreeView ไม่รองรับ hide node โดยตรง)
                        // เปลี่ยนเป็น approach ที่ถูกต้อง: rebuild nodes
                        divNode.ForeColor = (!divMatch && !anyChildMatch && !string.IsNullOrEmpty(kw))
                            ? Color.LightGray
                            : (divNode.Tag is OrgUnitData od2 && od2.AlreadyExistsInParent)
                                ? Color.DarkGray
                                : Color.Black;
                    }
                    tv.EndUpdate();
                };

                // ── Confirm action ────────────────────────────────────────────
                Action confirm = () => {
                    var selectedDivNode = tv.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Checked);
                    if (selectedDivNode == null)
                    {
                        MessageBox.Show("กรุณาเลือกฝ่ายก่อน", "แจ้งเตือน",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var divData = selectedDivNode.Tag as OrgUnitData;
                    var checkedDepts = selectedDivNode.Nodes.Cast<TreeNode>()
                        .Where(n => n.Checked)
                        .Select(n => n.Tag as OrgUnitData)
                        .Where(d => d != null)
                        .ToList();

                    _pendingDivision    = divData;
                    _pendingDepartments = checkedDepts;

                    // Update outer dialog UI
                    txtCode.Text = divData?.Code   ?? string.Empty;
                    txtName.Text = divData?.NameTh ?? string.Empty;
                    _batchSelections = null;

                    int deptCount = checkedDepts.Count;
                    lblHeader.Text = string.Format(
                        "สร้างหน่วยงานใหม่\n🏬  {0} — จะสร้างแผนก {1} รายการอัตโนมัติ",
                        divData?.Code ?? "", deptCount);

                    dlg.DialogResult = DialogResult.OK;
                    dlg.Close();
                };

                btnOk.Click    += (s, e2) => confirm();
                tv.DoubleClick += (s, e2) => {
                    // Double-click Division node: toggle check and confirm if a Division is selected
                    var node = tv.SelectedNode;
                    if (node != null && node.Level == 0)
                    {
                        node.Checked = !node.Checked;
                        if (node.Checked) confirm();
                    }
                };

                dlg.Controls.Add(tv);
                dlg.Controls.Add(pnlSearch);
                dlg.Controls.Add(pnlFooter);
                dlg.Controls.Add(pnlHdr);
                dlg.Shown += (s, e2) => txtSearch.Focus();
                dlg.ShowDialog(this);
            }
        }

        // ── Department Flat Picker (unchanged behavior) ───────────────────────
        private void ShowDepartmentFlatPicker()
        {
            if (_authorityOrgLookupController == null && _employeeDirectoryController == null)
            {
                MessageBox.Show("ไม่มีแหล่งข้อมูลแผนก กรุณาตรวจสอบการตั้งค่า",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedParentDivisionId = 0;
            if (cboParent.SelectedItem is OrgUnitItem selParent && selParent.OrgUnit != null)
                selectedParentDivisionId = selParent.OrgUnit.OrgUnitId;

            var allItems = new List<ListViewItem>();

            try
            {
                if (_authorityOrgLookupController != null)
                {
                    string selectedDivCode = null;
                    string selectedDivName = null;
                    if (cboParent.SelectedItem is OrgUnitItem parentItem && parentItem.OrgUnit != null)
                    {
                        selectedDivCode = parentItem.OrgUnit.OrgUnitCode;
                        selectedDivName = parentItem.OrgUnit.OrgUnitNameTH;
                    }

                    var hrDivisions = _authorityOrgLookupController.GetDivisions();
                    int divisionId  = 0;

                    if (!string.IsNullOrEmpty(selectedDivCode))
                    {
                        var div = hrDivisions.FirstOrDefault(d =>
                            string.Equals(d.DivisionCode, selectedDivCode, StringComparison.OrdinalIgnoreCase));
                        if (div != null) divisionId = div.DivisionID;
                    }
                    if (divisionId == 0 && !string.IsNullOrEmpty(selectedDivName))
                    {
                        var div = hrDivisions.FirstOrDefault(d =>
                            string.Equals((d.DivisionName ?? "").Trim(), selectedDivName.Trim(),
                                StringComparison.OrdinalIgnoreCase));
                        if (div != null) divisionId = div.DivisionID;
                    }

                    if (divisionId == 0)
                    {
                        MessageBox.Show("กรุณาเลือกฝ่าย (Division) ที่ผูกกับข้อมูล HR ก่อนค้นหาแผนก",
                            "ไม่พบฝ่ายที่เลือก", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var depts = _authorityOrgLookupController.GetDepartments()
                        .Where(d => d.DivisionID == divisionId)
                        .OrderBy(d => d.DepartmentCode);

                    foreach (var d in depts)
                    {
                        bool alreadyExists = selectedParentDivisionId > 0 && _allUnits.Any(u =>
                            u.IsActive && !u.IsDeleted &&
                            u.Tier == OrgUnitTier.Department &&
                            u.ParentId == selectedParentDivisionId &&
                            string.Equals(u.OrgUnitCode, d.DepartmentCode, StringComparison.OrdinalIgnoreCase));

                        var depName = !string.IsNullOrEmpty(d.DepartmentNameTh)
                            ? d.DepartmentNameTh : d.DepartmentName;

                        var row = new ListViewItem(d.DepartmentCode ?? "");
                        row.SubItems.Add(alreadyExists
                            ? string.Format("{0} (มีแล้วในสังกัดนี้)", depName ?? "")
                            : depName ?? "");
                        row.Tag = new OrgUnitData {
                            Type   = "แผนก",
                            Code   = d.DepartmentCode,
                            NameTh = depName,
                            AlreadyExistsInParent = alreadyExists
                        };
                        if (alreadyExists) row.ForeColor = Color.DarkGray;
                        allItems.Add(row);
                    }
                }
                else if (_employeeDirectoryController != null)
                {
                    foreach (var d in _employeeDirectoryController.GetDepartments().OrderBy(x => x.DepartmentCode))
                    {
                        var row = new ListViewItem(d.DepartmentCode ?? "");
                        row.SubItems.Add(d.NameTh ?? "");
                        row.Tag = new OrgUnitData { Type = "แผนก", Code = d.DepartmentCode, NameTh = d.NameTh };
                        allItems.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dlg = new Form())
            {
                dlg.Text            = "เลือกแผนก";
                dlg.ClientSize      = new System.Drawing.Size(420, 380);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;

                var pnlHdr = new Panel { Dock = DockStyle.Top, Height = 36,
                    BackColor = Color.FromArgb(0, 51, 102) };
                pnlHdr.Controls.Add(new Label {
                    Text      = "🔍  เลือกแผนก",
                    Dock      = DockStyle.Fill,
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(10, 0, 0, 0)
                });

                var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 44,
                    BackColor = Color.FromArgb(243, 243, 243) };

                var btnOk = new Button {
                    Text      = "✔ เลือก",
                    Size      = new System.Drawing.Size(90, 28),
                    BackColor = Color.FromArgb(0, 51, 102),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold)
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCnl = new Button {
                    Text         = "ยกเลิก",
                    Size         = new System.Drawing.Size(80, 28),
                    BackColor    = Color.White,
                    ForeColor    = Color.FromArgb(80, 80, 80),
                    FlatStyle    = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel
                };
                btnCnl.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 190);

                var lblSelCount = new Label {
                    Text      = "เลือกแล้ว: 0 รายการ",
                    AutoSize  = false,
                    Width     = 160,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 51, 102),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock      = DockStyle.Left,
                    Padding   = new Padding(8, 0, 0, 0)
                };

                pnlFooter.Controls.Add(lblSelCount);
                pnlFooter.Controls.Add(btnOk);
                pnlFooter.Controls.Add(btnCnl);
                pnlFooter.Resize += (s, e2) => {
                    btnCnl.Left = pnlFooter.ClientSize.Width - btnCnl.Width - 8;
                    btnOk.Left  = btnCnl.Left - btnOk.Width - 8;
                    btnOk.Top   = btnCnl.Top = (pnlFooter.Height - btnOk.Height) / 2;
                };

                var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 36,
                    BackColor = Color.White, Padding = new Padding(6, 4, 6, 4) };
                bool isPlaceholder = true;
                var txtSearch = new TextBox {
                    Dock      = DockStyle.Fill,
                    Font      = new Font("Segoe UI", 10f),
                    ForeColor = Color.Gray,
                    Text      = "ค้นหารหัสหรือชื่อ..."
                };
                txtSearch.GotFocus  += (s, e2) => {
                    if (!isPlaceholder) return;
                    isPlaceholder       = false;
                    txtSearch.Text      = "";
                    txtSearch.ForeColor = Color.Black;
                };
                txtSearch.LostFocus += (s, e2) => {
                    if (!string.IsNullOrWhiteSpace(txtSearch.Text)) return;
                    isPlaceholder       = true;
                    txtSearch.Text      = "ค้นหารหัสหรือชื่อ...";
                    txtSearch.ForeColor = Color.Gray;
                };
                pnlSearch.Controls.Add(txtSearch);

                var lv = new ListView {
                    Dock          = DockStyle.Fill,
                    View          = View.Details,
                    FullRowSelect = true,
                    GridLines     = true,
                    MultiSelect   = true,
                    CheckBoxes    = true,
                    Font          = new Font("Segoe UI", 9.5f)
                };
                lv.Columns.Add("รหัส",     70);
                lv.Columns.Add("ชื่อ (TH)", 295);

                lv.ItemChecked += (s, e2) => {
                    int cnt = lv.CheckedItems.Count;
                    lblSelCount.Text      = cnt > 0
                        ? string.Format("เลือกแล้ว: {0} รายการ", cnt)
                        : "เลือกแล้ว: 0 รายการ";
                    lblSelCount.ForeColor = cnt > 0 ? Color.FromArgb(0, 120, 0) : Color.FromArgb(0, 51, 102);
                    btnOk.Text            = cnt > 1
                        ? string.Format("✔ เลือก ({0})", cnt)
                        : "✔ เลือก";
                };
                lv.ItemCheck += (s, e2) => {
                    if (e2.NewValue != CheckState.Checked) return;
                    var d = lv.Items[e2.Index].Tag as OrgUnitData;
                    if (d == null || !d.AlreadyExistsInParent) return;
                    e2.NewValue = CheckState.Unchecked;
                    MessageBox.Show(
                        string.Format("รหัส {0} มีอยู่แล้วในสังกัดนี้", d.Code),
                        "พบข้อมูลซ้ำ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                };

                lv.Items.AddRange(allItems.ToArray());

                txtSearch.TextChanged += (s, e2) => {
                    if (isPlaceholder) return;
                    string kw = txtSearch.Text.Trim().ToLower();
                    lv.BeginUpdate();
                    lv.Items.Clear();
                    var filtered = string.IsNullOrEmpty(kw)
                        ? allItems
                        : allItems.Where(r =>
                            r.Text.ToLower().Contains(kw) ||
                            (r.SubItems.Count > 1 && r.SubItems[1].Text.ToLower().Contains(kw))).ToList();
                    lv.Items.AddRange(filtered.ToArray());
                    lv.EndUpdate();
                };

                Action confirm = () => {
                    var checkedItems = lv.CheckedItems.Cast<ListViewItem>().ToList();
                    if (checkedItems.Count == 0 && lv.SelectedItems.Count > 0)
                        checkedItems = lv.SelectedItems.Cast<ListViewItem>().ToList();
                    if (checkedItems.Count == 0) return;

                    var existing = new List<string>();
                    var toCreate = new List<OrgUnitData>();

                    foreach (var row in checkedItems)
                    {
                        var d = row.Tag as OrgUnitData;
                        if (d == null) continue;

                        int? parentId = null;
                        if (cboParent.SelectedItem is OrgUnitItem cpi && cpi.OrgUnit != null)
                            parentId = cpi.OrgUnit.OrgUnitId;

                        bool dup = (parentId.HasValue && _allUnits.Any(u =>
                                u.IsActive && !u.IsDeleted &&
                                u.Tier == OrgUnitTier.Department &&
                                u.ParentId == parentId.Value &&
                                string.Equals(u.OrgUnitCode, d.Code, StringComparison.OrdinalIgnoreCase))) ||
                            _allUnits.Any(u =>
                                string.Equals(u.OrgUnitCode, d.Code, StringComparison.OrdinalIgnoreCase)
                                && u.IsActive && !u.IsDeleted);

                        if (dup) existing.Add(d.Code);
                        else     toCreate.Add(d);
                    }

                    if (existing.Count > 0)
                        MessageBox.Show(
                            string.Format("พบรหัสซ้ำ:\n  • {0}\n\nรายการซ้ำจะไม่ถูกเลือก",
                                string.Join("\n  • ", existing.Distinct())),
                            "พบข้อมูลซ้ำ", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    if (toCreate.Count == 0) return;

                    if (toCreate.Count == 1)
                    {
                        txtCode.Text     = toCreate[0].Code   ?? string.Empty;
                        txtName.Text     = toCreate[0].NameTh ?? string.Empty;
                        _batchSelections = null;
                    }
                    else
                    {
                        _batchSelections = toCreate;
                        txtCode.Text = string.Format("({0} รายการ)", toCreate.Count);
                        txtName.Text = string.Join(", ", toCreate.Take(3).Select(d => d.Code));
                    }
                    dlg.DialogResult = DialogResult.OK;
                    dlg.Close();
                };

                btnOk.Click    += (s, e2) => confirm();
                lv.DoubleClick += (s, e2) => confirm();

                dlg.Controls.Add(lv);
                dlg.Controls.Add(pnlSearch);
                dlg.Controls.Add(pnlFooter);
                dlg.Controls.Add(pnlHdr);
                dlg.Shown += (s, e2) => txtSearch.Focus();
                dlg.ShowDialog(this);
            }
        }

        private class OrgUnitData
        {
            public string Type                 { get; set; }
            public string Code                 { get; set; }
            public string NameTh               { get; set; }
            public bool   AlreadyExistsInParent { get; set; }
            public int    DivisionId           { get; set; }
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            // Hierarchical mode: Division + Departments selected from TreeView picker
            if (_pendingDivision != null)
            {
                int? subCommitteeId = null;
                if (cboParent.SelectedItem is OrgUnitItem hpi && hpi.OrgUnit != null)
                    subCommitteeId = hpi.OrgUnit.OrgUnitId;

                HierarchicalResult = new HierarchicalBatchResult
                {
                    DivisionRequest = new CreateOrgUnitRequest {
                        OrgUnitCode     = _pendingDivision.Code,
                        OrgUnitNameTH   = _pendingDivision.NameTh,
                        OrgUnitNameEN   = _pendingDivision.NameTh,
                        OrgUnitType     = (int)OrgUnitTier.Division,
                        ParentOrgUnitID = subCommitteeId
                    },
                    DepartmentRequests = (_pendingDepartments ?? new List<OrgUnitData>())
                        .Select(d => new CreateOrgUnitRequest {
                            OrgUnitCode     = d.Code,
                            OrgUnitNameTH   = d.NameTh,
                            OrgUnitNameEN   = d.NameTh,
                            OrgUnitType     = (int)OrgUnitTier.Department,
                            ParentOrgUnitID = null  // Presenter fills this after Division is created
                        }).ToList()
                };
                return;
            }

            // Batch mode: multiple items selected from the department flat picker
            if (_batchSelections != null && _batchSelections.Count > 0)
            {
                var batchTier = cboTier.SelectedItem is OrgUnitTier bt ? bt : OrgUnitTier.Division;
                int? batchParentId = null;
                if (cboParent.SelectedItem is OrgUnitItem batchSel && batchSel.OrgUnit != null)
                    batchParentId = batchSel.OrgUnit.OrgUnitId;

                BatchResults = _batchSelections.Select(d => new CreateOrgUnitRequest {
                    OrgUnitNameTH   = d.NameTh,
                    OrgUnitNameEN   = d.NameTh,
                    OrgUnitCode     = d.Code,
                    OrgUnitType     = (int)batchTier,
                    ParentOrgUnitID = batchParentId
                }).ToList();
                return;
            }

            string nameTH = txtName.Text.Trim();
            string nameEN = txtNameEN.Text.Trim();
            string code   = txtCode.Text.Trim();

            if (string.IsNullOrEmpty(nameTH))
            {
                MessageBox.Show("กรุณาระบุชื่อหน่วยงาน (ไทย)", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("กรุณาระบุรหัสหน่วยงาน (OrgUnitCode)", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            // SubCommittee ห้ามซ้ำ (auto-generate)
            // Division/Department ซ้ำได้ เพราะใช้รหัสจาก HR และอาจอยู่ใต้หลาย parent
            var tier = (OrgUnitTier)cboTier.SelectedItem;

            if (tier == OrgUnitTier.SubCommittee)
            {
                List<OrgUnit> allUnitsForCheck;
                allUnitsForCheck = _allUnits;

                var existingByCode = allUnitsForCheck.FirstOrDefault(u =>
                    u.OrgUnitCode == code && u.IsActive && !u.IsDeleted);

                if (existingByCode != null)
                {
                    MessageBox.Show(
                        string.Format("รหัส \"{0}\" มีอยู่ในระบบแล้ว\nหน่วยงาน: {1}\n\nกรุณาใช้รหัสอื่น",
                            code, existingByCode.OrgUnitNameTH ?? existingByCode.Name),
                        "รหัสซ้ำ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCode.Focus();
                    DialogResult = DialogResult.None;
                    return;
                }
            }

            // ตรวจสอบ hierarchy rules
            if (cboParent.SelectedItem is OrgUnitItem parentItem && parentItem.OrgUnit != null)
            {
                var parentTier = parentItem.OrgUnit.Tier;
                
                // ตรวจสอบ hierarchy ที่ถูกต้อง
                if (parentTier == OrgUnitTier.SubCommittee && tier != OrgUnitTier.Division)
                {
                    MessageBox.Show("SubCommittee สามารถมีได้แค่ Division เท่านั้น", "Hierarchy Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }
                
                if (parentTier == OrgUnitTier.Division && tier != OrgUnitTier.Department)
                {
                    MessageBox.Show("Division สามารถมีได้แค่ Department เท่านั้น", "Hierarchy Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }
                
                if (parentTier == OrgUnitTier.Department)
                {
                    MessageBox.Show("ไม่สามารถสร้างหน่วยงานใต้ Department ได้", "Hierarchy Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }
            }

            // Only Department requires parent, Division and SubCommittee are optional
            if (tier == OrgUnitTier.Department && cboParent.SelectedItem == null)
            {
                MessageBox.Show("แผนกต้องมีฝ่ายแม่ กรุณาเลือก", "Validation", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            int? parentId = null;
            if (cboParent.SelectedItem is OrgUnitItem selectedParent && selectedParent.OrgUnit != null)
                parentId = selectedParent.OrgUnit.OrgUnitId;

            if (string.IsNullOrEmpty(nameEN))
                nameEN = nameTH;

            Result = new CreateOrgUnitRequest
            {
                OrgUnitNameTH        = nameTH,
                OrgUnitNameEN        = nameEN,
                OrgUnitCode          = code,
                OrgUnitType          = (int)tier,
                ParentOrgUnitID      = parentId
            };
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void RefreshParentList(OrgUnitTier tier)
        {
            // ปิด event handler ชั่วคราวเพื่อไม่ให้ trigger ระหว่าง refresh
            cboParent.SelectedIndexChanged -= OnParentChanged;
            
            cboParent.Items.Clear();

            if (tier == OrgUnitTier.SubCommittee)
            {
                // SubCommittee ไม่มี parent
                cboParent.Enabled = false;
                cboParent.SelectedIndexChanged += OnParentChanged;
                return;
            }

            if (tier == OrgUnitTier.Division)
            {
                // Division can choose to belong to a SubCommittee or be independent
                cboParent.Enabled = true;
                cboParent.Items.Add(new OrgUnitItem(null) { DisplayText = "🔗 เป็นอิสระ (ไม่สังกัด SubCommittee ใดโดยเฉพาะ)" });
                
                // แสดง SubCommittee ทั้งหมดเสมอ
                // เพื่อรองรับกรณี 1 Division เชื่อมได้หลาย SubCommittee
                List<OrgUnit> availableSubCommittees = _allUnits
                    .Where(u => u.Tier == OrgUnitTier.SubCommittee && u.IsActive)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Found {availableSubCommittees.Count} available SubCommittees:");
                foreach (var sc in availableSubCommittees)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {sc.OrgUnitCode}: {sc.Name} (Active: {sc.IsActive}, Deleted: {sc.IsDeleted})");
                    cboParent.Items.Add(new OrgUnitItem(sc));
                }

                // Default to "เป็นอิสระ"
                cboParent.SelectedIndex = 0;

                // ถ้า selected node ใน tree เป็น SubCommittee ให้ pre-select ตัวนั้นเป็นค่าเริ่มต้น
                if (_selectedParent != null && _selectedParent.Tier == OrgUnitTier.SubCommittee)
                {
                    for (int i = 1; i < cboParent.Items.Count; i++)
                    {
                        var item = cboParent.Items[i] as OrgUnitItem;
                        if (item?.OrgUnit?.OrgUnitId == _selectedParent.OrgUnitId)
                        {
                            cboParent.SelectedIndex = i;
                            break;
                        }
                    }
                }

                // Debug: แสดงจำนวน items ใน combobox
                System.Diagnostics.Debug.WriteLine($"Parent ComboBox has {cboParent.Items.Count} items");
                
                cboParent.SelectedIndexChanged += OnParentChanged;
                return;
            }

            if (tier == OrgUnitTier.Department)
            {
                // Department must belong to a Division
                cboParent.Enabled = true;

                List<OrgUnit> availableDivisions;
                
                if (_selectedParent != null && _selectedParent.Tier == OrgUnitTier.Division)
                {
                    // ถ้าเลือก Division ใน TreeView ให้แสดงเฉพาะ Division นั้น
                    availableDivisions = new List<OrgUnit> { _selectedParent };
                    System.Diagnostics.Debug.WriteLine($"Selected Division context: {_selectedParent.Name}");
                }
                else if (_selectedParent != null && _selectedParent.Tier == OrgUnitTier.SubCommittee)
                {
                    // ถ้าเลือก SubCommittee ใน TreeView ให้แสดงเฉพาะ Division ใต้ SubCommittee นั้น
                    availableDivisions = _allUnits
                        .Where(u => u.Tier == OrgUnitTier.Division && u.IsActive && u.ParentId == _selectedParent.OrgUnitId)
                        .ToList();
                    System.Diagnostics.Debug.WriteLine($"Selected SubCommittee context: {_selectedParent.Name}, Divisions: {availableDivisions.Count}");
                }
                else
                {
                    // ถ้าไม่ได้เลือก Division ให้แสดงทั้งหมด
                    availableDivisions = _allUnits.Where(u => u.Tier == OrgUnitTier.Division && u.IsActive).ToList();
                }

                foreach (var p in availableDivisions.OrderBy(u => u.Name))
                    cboParent.Items.Add(new OrgUnitItem(p));

                if (cboParent.Items.Count > 0)
                    cboParent.SelectedIndex = 0;
                    
                cboParent.SelectedIndexChanged += OnParentChanged;
                return;
            }
                
            // เปิด event handler กลับ
            cboParent.SelectedIndexChanged += OnParentChanged;
        }

        private class OrgUnitItem
        {
            public OrgUnit OrgUnit { get; private set; }
            public string DisplayText { get; set; }
            
            public OrgUnitItem(OrgUnit unit) 
            { 
                OrgUnit = unit; 
                DisplayText = unit?.Name;
            }
            
            public override string ToString() 
            { 
                return DisplayText ?? OrgUnit?.Name ?? ""; 
            }
        }
    }
}
