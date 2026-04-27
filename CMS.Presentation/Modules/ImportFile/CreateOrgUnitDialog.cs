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

        // เก็บ DivisionID ที่เลือกไว้ เพื่อ filter แผนก
        private int _selectedDivisionID = 0;

        public CreateOrgUnitRequest       Result       { get; private set; }
        public List<CreateOrgUnitRequest> BatchResults { get; private set; }

        private List<OrgUnitData> _batchSelections;

        public CreateOrgUnitDialog(IEnumerable<OrgUnit> allUnits,
            IEmployeeDirectoryController employeeDirectoryController = null,
            OrgUnitTier? fixedTier = null,
            OrgUnit selectedParent = null,
            IAuthorityOrgLookupController authorityOrgLookupController = null)
        {
            // Include ALL units (active, inactive, deleted) to avoid code conflicts
            _allUnits           = allUnits?.ToList() ?? new List<OrgUnit>();
            _employeeDirectoryController = employeeDirectoryController;
            _fixedTier          = fixedTier ?? OrgUnitTier.SubCommittee;
            _selectedParent     = selectedParent;
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

            string dlgTitle = isDivision ? "เลือกฝ่าย" : "เลือกแผนก";

            using (var dlg = new Form())
            {
                dlg.Text            = dlgTitle;
                dlg.ClientSize      = new System.Drawing.Size(420, 380);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;

                // ── Header ────────────────────────────────────────────────────
                var pnlHdr = new Panel { Dock = DockStyle.Top, Height = 36,
                    BackColor = Color.FromArgb(0, 51, 102) };
                pnlHdr.Controls.Add(new Label {
                    Text = "🔍  " + dlgTitle, Dock = DockStyle.Fill,
                    ForeColor = Color.White, Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0)
                });

                // ── Footer ────────────────────────────────────────────────────
                var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 44,
                    BackColor = Color.FromArgb(243, 243, 243) };

                var btnOk = new Button {
                    Text = "✔ เลือก", Size = new System.Drawing.Size(90, 28),
                    BackColor = Color.FromArgb(0, 51, 102), ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Anchor = AnchorStyles.Right | AnchorStyles.Top
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCnl = new Button {
                    Text = "ยกเลิก", Size = new System.Drawing.Size(80, 28),
                    BackColor = Color.White, ForeColor = Color.FromArgb(80, 80, 80),
                    FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel,
                    Anchor = AnchorStyles.Right | AnchorStyles.Top
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

                // ── Search box ───────────────────────────────────────────────
                var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 36,
                    BackColor = Color.White, Padding = new Padding(6, 4, 6, 4) };
                bool isPlaceholder = true;
                var txtSearch = new TextBox {
                    Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f),
                    ForeColor = Color.Gray, Text = "ค้นหารหัสหรือชื่อ..."
                };
                txtSearch.GotFocus  += (s, e2) => {
                    if (!isPlaceholder) return;
                    isPlaceholder = false;
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

                // ── ListView ──────────────────────────────────────────────────
                var lv = new ListView {
                    Dock = DockStyle.Fill, View = View.Details,
                    FullRowSelect = true, GridLines = true,
                    MultiSelect = true, CheckBoxes = true,
                    Font = new Font("Segoe UI", 9.5f)
                };
                lv.Columns.Add("รหัส",     70);
                lv.Columns.Add("ชื่อ (TH)", 295);

                lv.ItemChecked += (s, e2) => {
                    int cnt = lv.CheckedItems.Count;
                    lblSelCount.Text = cnt > 0
                        ? string.Format("เลือกแล้ว: {0} รายการ", cnt)
                        : "เลือกแล้ว: 0 รายการ";
                    lblSelCount.ForeColor = cnt > 0 ? Color.FromArgb(0, 120, 0) : Color.FromArgb(0, 51, 102);
                    btnOk.Text = cnt > 1
                        ? string.Format("✔ เลือก ({0})", cnt)
                        : "✔ เลือก";
                };


                // Keep all items for filtering
                var allItems = new List<ListViewItem>();

                try
                {
                    if (isDivision)
                    {
                        // ดึงจาก authority lookup ถ้ามี controller, fallback ไป Employee API
                        if (_authorityOrgLookupController != null)
                        {
                            foreach (var v in _authorityOrgLookupController.GetDivisions().OrderBy(x => x.DivisionCode))
                            {
                                var row = new ListViewItem(v.DivisionCode ?? "");
                                row.SubItems.Add(v.DivisionName ?? "");
                                row.Tag = new OrgUnitData { Type = "ฝ่าย", Code = v.DivisionCode, NameTh = v.DivisionName };
                                allItems.Add(row);
                            }
                        }
                        else if (_employeeDirectoryController != null)
                        {
                            foreach (var v in _employeeDirectoryController.GetDivisions().OrderBy(x => x.DivisionCode))
                            {
                                var row = new ListViewItem(v.DivisionCode ?? "");
                                row.SubItems.Add(v.NameTh ?? "");
                                row.Tag = new OrgUnitData { Type = "ฝ่าย", Code = v.DivisionCode, NameTh = v.NameTh };
                                allItems.Add(row);
                            }
                        }
                    }
                    else
                    {
                        // หา DivisionID จาก cboParent ที่เลือกไว้
                        int divisionId = 0;
                        if (_authorityOrgLookupController != null)
                        {
                            // ดึง DivisionCode จาก cboParent แล้ว map ไปหา DivisionID ใน HR_Division
                            string selectedDivCode = null;
                            if (cboParent.SelectedItem is OrgUnitItem parentItem && parentItem.OrgUnit != null)
                                selectedDivCode = parentItem.OrgUnit.OrgUnitCode;

                            if (!string.IsNullOrEmpty(selectedDivCode))
                            {
                                var div = _authorityOrgLookupController.GetDivisions()
                                    .FirstOrDefault(d => string.Equals(d.DivisionCode, selectedDivCode,
                                        StringComparison.OrdinalIgnoreCase));
                                if (div != null) divisionId = div.DivisionID;
                            }

                            var allDepartments = _authorityOrgLookupController.GetDepartments();
                            var depts = divisionId > 0
                                ? allDepartments.Where(d => d.DivisionID == divisionId)
                                : allDepartments;

                            foreach (var d in depts.OrderBy(x => x.DepartmentCode))
                            {
                                var row = new ListViewItem(d.DepartmentCode ?? "");
                                row.SubItems.Add(d.DepartmentName ?? "");
                                row.Tag = new OrgUnitData { Type = "แผนก", Code = d.DepartmentCode, NameTh = d.DepartmentName };
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Populate ListView initially
                lv.Items.AddRange(allItems.ToArray());

                // Filter on text change
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

                // ── Confirm ───────────────────────────────────────────────────
                Action confirm = () => {
                    var checkedItems = lv.CheckedItems.Cast<ListViewItem>().ToList();
                    if (checkedItems.Count == 0 && lv.SelectedItems.Count > 0)
                        checkedItems = new List<ListViewItem> { lv.SelectedItems[0] };
                    if (checkedItems.Count == 0) return;

                    var dupeItems = checkedItems
                        .Where(r => {
                            var d = r.Tag as OrgUnitData;
                            return d != null && _allUnits.Any(u =>
                                string.Equals(u.OrgUnitCode, d.Code, StringComparison.OrdinalIgnoreCase)
                                && u.IsActive && !u.IsDeleted);
                        }).ToList();
                    var newItems = checkedItems.Except(dupeItems).ToList();

                    if (dupeItems.Count > 0)
                    {
                        string dupeCodes = string.Join("\n  • ",
                            dupeItems.Select(r => (r.Tag as OrgUnitData)?.Code));
                        string msg = string.Format("รหัสต่อไปนี้มีอยู่ในระบบแล้ว และจะถูกข้าม:\n  • {0}", dupeCodes);
                        if (newItems.Count == 0)
                        {
                            MessageBox.Show(msg + "\n\nไม่มีรายการใหม่ที่จะเพิ่ม",
                                "ซ้ำทั้งหมด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        MessageBox.Show(msg, "พบรายการซ้ำ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if (newItems.Count == 1)
                    {
                        var data = newItems[0].Tag as OrgUnitData;
                        if (data == null) return;
                        txtName.Text = data.NameTh ?? string.Empty;
                        txtCode.Text = data.Code   ?? string.Empty;
                        _batchSelections = null;
                    }
                    else
                    {
                        _batchSelections = newItems
                            .Select(r => r.Tag as OrgUnitData)
                            .Where(d => d != null)
                            .ToList();
                        txtCode.Text = string.Format("({0} รายการ)", _batchSelections.Count);
                        txtName.Text = string.Join(", ",
                            _batchSelections.Take(3).Select(d => d.Code));
                    }
                    dlg.DialogResult = DialogResult.OK;
                    dlg.Close();
                };
                btnOk.Click    += (s, e2) => confirm();
                lv.DoubleClick += (s, e2) => confirm();

                // ── Add controls: Fill ก่อน → Bottom → Top ────────────────────
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
            public string Type   { get; set; }
            public string Code   { get; set; }
            public string NameTh { get; set; }
        }

        private void OnOkClick(object sender, EventArgs e)
        {
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
                OrgUnitNameTH   = nameTH,
                OrgUnitNameEN   = nameEN,
                OrgUnitCode     = code,
                OrgUnitType     = (int)tier,
                ParentOrgUnitID = parentId
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
                
                List<OrgUnit> availableSubCommittees;
                
                if (_selectedParent != null && _selectedParent.Tier == OrgUnitTier.Division)
                {
                    // ถ้าเลือก Division ใน TreeView ให้หา SubCommittee ที่เป็น parent ของ Division นั้น
                    var parentSubCommittee = _allUnits.FirstOrDefault(u => 
                        u.OrgUnitId == _selectedParent.ParentOrgUnitID && 
                        u.Tier == OrgUnitTier.SubCommittee && 
                        u.IsActive);
                    
                    if (parentSubCommittee != null)
                    {
                        availableSubCommittees = new List<OrgUnit> { parentSubCommittee };
                        System.Diagnostics.Debug.WriteLine($"Found parent SubCommittee: {parentSubCommittee.Name} for Division: {_selectedParent.Name}");
                    }
                    else
                    {
                        // ถ้า Division ไม่มี parent SubCommittee (เป็นอิสระ) ให้แสดงทั้งหมด
                        availableSubCommittees = _allUnits.Where(u => u.Tier == OrgUnitTier.SubCommittee && u.IsActive).ToList();
                        System.Diagnostics.Debug.WriteLine($"Division {_selectedParent.Name} is independent, showing all SubCommittees");
                    }
                }
                else
                {
                    // ถ้าไม่ได้เลือก Division ให้แสดงทั้งหมด
                    availableSubCommittees = _allUnits.Where(u => u.Tier == OrgUnitTier.SubCommittee && u.IsActive).ToList();
                }

                System.Diagnostics.Debug.WriteLine($"Found {availableSubCommittees.Count} available SubCommittees:");
                foreach (var sc in availableSubCommittees)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {sc.OrgUnitCode}: {sc.Name} (Active: {sc.IsActive}, Deleted: {sc.IsDeleted})");
                    cboParent.Items.Add(new OrgUnitItem(sc));
                }

                cboParent.SelectedIndex = 0; // Select "independent" by default
                
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
