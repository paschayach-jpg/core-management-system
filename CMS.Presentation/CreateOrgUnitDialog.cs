using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    internal partial class CreateOrgUnitDialog : Form
    {
        private readonly List<OrgUnit>       _allUnits;
        private readonly IEmployeeApiService _employeeApiService;
        private readonly IOrgStructureService _orgService;
        private readonly OrgUnitTier         _fixedTier;
        private readonly OrgUnit             _selectedParent; // Context จาก TreeView

        public CreateOrgUnitRequest Result { get; private set; }

        public CreateOrgUnitDialog(IEnumerable<OrgUnit> allUnits,
            IEmployeeApiService employeeApiService = null,
            OrgUnitTier? fixedTier = null,
            IOrgStructureService orgService = null,
            OrgUnit selectedParent = null)
        {
            // Include ALL units (active, inactive, deleted) to avoid code conflicts
            _allUnits           = allUnits?.ToList() ?? new List<OrgUnit>();
            _employeeApiService = employeeApiService;
            _orgService         = orgService;
            _fixedTier          = fixedTier ?? OrgUnitTier.SubCommittee;
            _selectedParent     = selectedParent; // เก็บ context

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

        /// <summary>สร้างรหัสอัตโนมัติ เช่น SC-001, DIV-003, DEPT-007</summary>
        private string GenerateCode(OrgUnitTier tier)
        {
            string prefix;
            switch (tier)
            {
                case OrgUnitTier.SubCommittee: prefix = "SC";   break;
                case OrgUnitTier.Division:     prefix = "DIV";  break;
                default:                       prefix = "DEPT"; break;
            }

            // Get fresh data from database if service is available
            List<OrgUnit> allUnits;
            if (_orgService != null)
            {
                try
                {
                    allUnits = _orgService.GetHierarchy().ToList();
                    System.Diagnostics.Debug.WriteLine($"GenerateCode: Got fresh data from service - {allUnits.Count} units");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"GenerateCode: Failed to get fresh data, using cached: {ex.Message}");
                    allUnits = _allUnits;
                }
            }
            else
            {
                allUnits = _allUnits;
                System.Diagnostics.Debug.WriteLine($"GenerateCode: No service available, using cached data - {allUnits.Count} units");
            }

            // Get all existing codes with this prefix (including deleted ones to avoid reuse)
            var existingCodes = new HashSet<string>(allUnits
                .Where(u => u.OrgUnitCode != null && u.OrgUnitCode.StartsWith(prefix + "-"))
                .Select(u => u.OrgUnitCode)); // Use HashSet for faster lookup

            // Debug: Show existing codes
            if (existingCodes.Count > 0)
            {
                var codeList = string.Join(", ", existingCodes.OrderBy(c => c));
                System.Diagnostics.Debug.WriteLine($"Existing {prefix} codes: {codeList}");
            }

            // Extract numbers from existing codes to find the highest
            var existingNumbers = new List<int>();
            foreach (var code in existingCodes)
            {
                var parts = code.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int number))
                {
                    existingNumbers.Add(number);
                }
            }

            // Find the next available number
            int nextNumber = existingNumbers.Count > 0 ? existingNumbers.Max() + 1 : 1;
            
            // Double-check that the generated code doesn't exist
            string newCode;
            do
            {
                newCode = string.Format("{0}-{1:D3}", prefix, nextNumber);
                nextNumber++;
            }
            while (existingCodes.Contains(newCode) && nextNumber <= 999);

            // Additional safety check - if we somehow reach 999, use timestamp
            if (nextNumber > 999)
            {
                var timestamp = DateTime.Now.ToString("MMddHHmm");
                newCode = string.Format("{0}-{1}", prefix, timestamp);
            }

            System.Diagnostics.Debug.WriteLine($"Generated code: {newCode}");
            return newCode;
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void TxtCode_TextChanged(object sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            if (string.IsNullOrWhiteSpace(code)) return;

            // Get fresh data for validation
            List<OrgUnit> allUnits;
            if (_orgService != null)
            {
                try
                {
                    allUnits = _orgService.GetHierarchy().ToList();
                }
                catch
                {
                    allUnits = _allUnits;
                }
            }
            else
            {
                allUnits = _allUnits;
            }

            // Check for duplicate code
            var existingUnit = allUnits.FirstOrDefault(u => 
                u.OrgUnitCode == code && u.IsActive && !u.IsDeleted);

            if (existingUnit != null)
            {
                txtCode.BackColor = Color.FromArgb(255, 235, 235); // Light red
                txtCode.ForeColor = Color.FromArgb(220, 53, 69);   // Red text
            }
            else
            {
                txtCode.BackColor = Color.White;
                txtCode.ForeColor = Color.Black;
            }
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
            if (_employeeApiService == null)
            {
                MessageBox.Show("Employee API service ไม่พร้อมใช้งาน",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ใช้ tier ที่เลือกจริงแทนที่จะใช้ _fixedTier
            var currentTier = cboTier.SelectedItem != null ? (OrgUnitTier)cboTier.SelectedItem : OrgUnitTier.Division;
            bool   isDivision = currentTier == OrgUnitTier.Division;
            string dlgTitle   = isDivision ? "เลือกฝ่าย" : "เลือกแผนก";

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
                var txtSearch = new TextBox {
                    Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f),
                    ForeColor = Color.Gray, Text = "ค้นหารหัสหรือชื่อ..."
                };
                txtSearch.GotFocus  += (s, e2) => {
                    if (txtSearch.ForeColor == Color.Gray) { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; }
                };
                txtSearch.LostFocus += (s, e2) => {
                    if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "ค้นหารหัสหรือชื่อ..."; txtSearch.ForeColor = Color.Gray; }
                };
                pnlSearch.Controls.Add(txtSearch);

                // ── ListView ──────────────────────────────────────────────────
                var lv = new ListView {
                    Dock = DockStyle.Fill, View = View.Details,
                    FullRowSelect = true, GridLines = true,
                    MultiSelect = false, Font = new Font("Segoe UI", 9.5f)
                };
                lv.Columns.Add("รหัส",     70);
                lv.Columns.Add("ชื่อ (TH)", 320);

                // Keep all items for filtering
                var allItems = new List<ListViewItem>();

                try
                {
                    if (isDivision)
                    {
                        foreach (var v in _employeeApiService.GetDivisions().OrderBy(x => x.DivisionCode))
                        {
                            var row = new ListViewItem(v.DivisionCode ?? "");
                            row.SubItems.Add(v.NameTh ?? "");
                            row.Tag = new OrgUnitData { Type = "ฝ่าย", Code = v.DivisionCode, NameTh = v.NameTh };
                            allItems.Add(row);
                        }
                    }
                    else
                    {
                        foreach (var d in _employeeApiService.GetDepartments().OrderBy(x => x.DepartmentCode))
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
                }

                // Populate ListView initially
                lv.Items.AddRange(allItems.ToArray());

                // Filter on text change
                txtSearch.TextChanged += (s, e2) => {
                    if (txtSearch.ForeColor == Color.Gray) return; // placeholder active
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
                    if (lv.SelectedItems.Count == 0) return;
                    var data = lv.SelectedItems[0].Tag as OrgUnitData;
                    if (data == null) return;
                    txtName.Text     = data.NameTh;
                    // ไม่ override txtCode — ใช้ค่าที่ generate ไว้แล้ว
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

            // Check for duplicate code - get fresh data
            List<OrgUnit> allUnits;
            if (_orgService != null)
            {
                try
                {
                    allUnits = _orgService.GetHierarchy().ToList();
                }
                catch
                {
                    allUnits = _allUnits;
                }
            }
            else
            {
                allUnits = _allUnits;
            }

            var existingUnit = allUnits.FirstOrDefault(u => 
                u.OrgUnitCode == code && u.IsActive && !u.IsDeleted);
            
            if (existingUnit != null)
            {
                MessageBox.Show(
                    string.Format("รหัสหน่วยงาน '{0}' มีอยู่ในระบบแล้ว\nหน่วยงาน: {1}\n\nกรุณาใช้รหัสอื่น", 
                        code, existingUnit.OrgUnitNameTH ?? existingUnit.Name), 
                    "รหัสซ้ำ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCode.Focus();
                DialogResult = DialogResult.None;
                return;
            }

            var tier = (OrgUnitTier)cboTier.SelectedItem;

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
