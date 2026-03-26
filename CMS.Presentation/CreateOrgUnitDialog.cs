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
        private readonly OrgUnitTier         _fixedTier;

        public CreateOrgUnitRequest Result { get; private set; }

        public CreateOrgUnitDialog(IEnumerable<OrgUnit> allUnits,
            IEmployeeApiService employeeApiService = null,
            OrgUnitTier? fixedTier = null)
        {
            _allUnits           = allUnits == null ? new List<OrgUnit>() : allUnits.Where(u => u.IsActive).ToList();
            _employeeApiService = employeeApiService;
            _fixedTier          = fixedTier ?? OrgUnitTier.SubCommittee;

            InitializeComponent();

            cboTier.Items.Clear();
            cboTier.Items.Add(OrgUnitTier.SubCommittee);
            cboTier.Items.Add(OrgUnitTier.Division);
            cboTier.Items.Add(OrgUnitTier.Department);

            if (fixedTier.HasValue)
            {
                cboTier.SelectedItem = fixedTier.Value;
                cboTier.Enabled      = false;
            }
            else
            {
                cboTier.SelectedIndex = 0;
            }

            RefreshParentList(_fixedTier);
            ApplyTierUiRules(_fixedTier);
        }

        // ── ปรับ UI ตาม tier ──────────────────────────────────────────────────
        private void ApplyTierUiRules(OrgUnitTier tier)
        {
            switch (tier)
            {
                case OrgUnitTier.SubCommittee:
                    lblHeader.Text      = "สร้างหน่วยงานใหม่\n🏢  กำลังสร้าง SubCommittee  (ระดับสูงสุด)";
                    btnSearchTH.Visible = false;
                    btnSearchEN.Visible = false;
                    break;
                case OrgUnitTier.Division:
                    lblHeader.Text      = "สร้างหน่วยงานใหม่\n🏬  กำลังสร้างฝ่าย  (ค้นหาได้เฉพาะฝ่ายจาก API)";
                    btnSearchTH.Visible = true;
                    btnSearchEN.Visible = false;
                    break;
                default: // Department
                    lblHeader.Text      = "สร้างหน่วยงานใหม่\n🏪  กำลังสร้างแผนก  (ค้นหาได้เฉพาะแผนกจาก API)";
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

            int existing = _allUnits.Count(u => u.Tier == tier);
            return string.Format("{0}-{1:D3}", prefix, existing + 1);
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void OnTierChanged(object sender, EventArgs e)
        {
            if (cboTier.SelectedItem == null) return;
            RefreshParentList((OrgUnitTier)cboTier.SelectedItem);
        }

        private void OnSearchNameClick(object sender, EventArgs e)
        {
            if (_employeeApiService == null)
            {
                MessageBox.Show("Employee API service ไม่พร้อมใช้งาน",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool   isDivision = _fixedTier == OrgUnitTier.Division;
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

                // ── ListView ──────────────────────────────────────────────────
                var lv = new ListView {
                    Dock = DockStyle.Fill, View = View.Details,
                    FullRowSelect = true, GridLines = true,
                    MultiSelect = false, Font = new Font("Segoe UI", 9.5f)
                };
                lv.Columns.Add("รหัส",     70);
                lv.Columns.Add("ชื่อ (TH)", 320);

                try
                {
                    if (isDivision)
                    {
                        foreach (var v in _employeeApiService.GetDivisions().OrderBy(x => x.DivisionCode))
                        {
                            var row = new ListViewItem(v.DivisionCode ?? "");
                            row.SubItems.Add(v.NameTh ?? "");
                            row.Tag = new OrgUnitData { Type = "ฝ่าย", Code = v.DivisionCode, NameTh = v.NameTh };
                            lv.Items.Add(row);
                        }
                    }
                    else
                    {
                        foreach (var d in _employeeApiService.GetDepartments().OrderBy(x => x.DepartmentCode))
                        {
                            var row = new ListViewItem(d.DepartmentCode ?? "");
                            row.SubItems.Add(d.NameTh ?? "");
                            row.Tag = new OrgUnitData { Type = "แผนก", Code = d.DepartmentCode, NameTh = d.NameTh };
                            lv.Items.Add(row);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

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
                dlg.Controls.Add(pnlFooter);
                dlg.Controls.Add(pnlHdr);
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

            var tier = (OrgUnitTier)cboTier.SelectedItem;

            if (tier != OrgUnitTier.SubCommittee && cboParent.SelectedItem == null)
            {
                MessageBox.Show(
                    string.Format("ระดับ {0} ต้องมีหน่วยงานแม่ กรุณาเลือก", tier),
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            int? parentId = null;
            if (tier != OrgUnitTier.SubCommittee && cboParent.SelectedItem is OrgUnitItem)
                parentId = ((OrgUnitItem)cboParent.SelectedItem).OrgUnit.OrgUnitId;

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
            cboParent.Items.Clear();

            if (tier == OrgUnitTier.SubCommittee)
            {
                cboParent.Enabled = false;
                return;
            }

            cboParent.Enabled = true;

            OrgUnitTier requiredParentTier = tier == OrgUnitTier.Division
                ? OrgUnitTier.SubCommittee
                : OrgUnitTier.Division;

            foreach (var p in _allUnits.Where(u => u.Tier == requiredParentTier).OrderBy(u => u.Name))
                cboParent.Items.Add(new OrgUnitItem(p));

            if (cboParent.Items.Count > 0)
                cboParent.SelectedIndex = 0;
        }

        private class OrgUnitItem
        {
            public OrgUnit OrgUnit { get; private set; }
            public OrgUnitItem(OrgUnit unit) { OrgUnit = unit; }
            public override string ToString() { return OrgUnit.Name; }
        }
    }
}
