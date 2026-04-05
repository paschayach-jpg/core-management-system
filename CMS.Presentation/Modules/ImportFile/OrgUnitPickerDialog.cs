using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Enums;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog สำหรับสร้างหน่วยงานใหม่
    /// แบบ Step-by-Step Form
    /// </summary>
    public partial class OrgUnitPickerDialog : Form
    {
        private readonly IEnumerable<OrgUnit> _allUnits;

        public OrgUnit NewOrgUnit { get; private set; }

        // For backward compatibility with existing code that expects SelectedUnits
        public List<OrgUnitSelection> SelectedUnits { get; private set; } = new List<OrgUnitSelection>();

        public OrgUnitPickerDialog(IEnumerable<OrgUnit> allUnits)
        {
            _allUnits = allUnits ?? Enumerable.Empty<OrgUnit>();
            InitializeComponent();
        }

        private void OrgUnitPickerDialog_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Initialize Level ComboBox
            cmbLevel.Items.Clear();
            cmbLevel.Items.Add(new ComboBoxItem { Text = "คณะกรรมการ (SubCommittee)", Value = 1 });
            cmbLevel.Items.Add(new ComboBoxItem { Text = "ฝ่าย (Division)", Value = 2 });
            cmbLevel.Items.Add(new ComboBoxItem { Text = "แผนก (Department)", Value = 3 });
            cmbLevel.DisplayMember = "Text";
            cmbLevel.ValueMember = "Value";

            // Initialize Parent ComboBox
            LoadParentUnits();

            // Set step label styles
            SetStepLabelStyle(lblStep1);
            SetStepLabelStyle(lblStep2);
            SetStepLabelStyle(lblStep3);
            SetStepLabelStyle(lblStep4);
            SetStepLabelStyle(lblStep5);
            SetStepLabelStyle(lblStep6);

            // Event handlers
            cmbLevel.SelectedIndexChanged += CmbLevel_SelectedIndexChanged;
            txtDivisionCode.TextChanged += txtDivisionCode_TextChanged;
            txtDepartmentCode.TextChanged += txtDepartmentCode_TextChanged;
            
            // Auto-generate organization code when division/department codes change
            UpdateOrgCode();
        }

        private void SetStepLabelStyle(Label stepLabel)
        {
            stepLabel.BackColor = Color.FromArgb(220, 53, 69);
            stepLabel.ForeColor = Color.White;
            stepLabel.TextAlign = ContentAlignment.MiddleCenter;
            stepLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            // Make it circular
            stepLabel.Width = stepLabel.Height = 35;
            stepLabel.FlatStyle = FlatStyle.Flat;
        }

        private void LoadParentUnits()
        {
            cmbParent.Items.Clear();
            var activeUnits = _allUnits.Where(u => u.IsActive && !u.IsDeleted).ToList();
            
            foreach (var unit in activeUnits.OrderBy(u => u.OrgUnitNameTH ?? u.Name))
            {
                string tierName = unit.OrgUnitType == 1 ? "คณะกรรมการ" 
                                : unit.OrgUnitType == 2 ? "ฝ่าย" 
                                : "แผนก";
                
                cmbParent.Items.Add(new ComboBoxItem 
                { 
                    Text = $"[{tierName}] {unit.OrgUnitNameTH ?? unit.Name}", 
                    Value = unit.OrgUnitID 
                });
            }
            cmbParent.DisplayMember = "Text";
            cmbParent.ValueMember = "Value";
        }

        private void CmbLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Filter parent units based on selected level
            if (cmbLevel.SelectedItem is ComboBoxItem selectedLevel)
            {
                int levelValue = (int)selectedLevel.Value;
                LoadParentUnitsForLevel(levelValue);
            }
        }

        private void LoadParentUnitsForLevel(int selectedLevel)
        {
            cmbParent.Items.Clear();
            var activeUnits = _allUnits.Where(u => u.IsActive && !u.IsDeleted).ToList();
            
            // Filter parent units based on hierarchy rules
            IEnumerable<OrgUnit> validParents = Enumerable.Empty<OrgUnit>();
            
            switch (selectedLevel)
            {
                case 1: // SubCommittee - no parent needed
                    cmbParent.Items.Add(new ComboBoxItem { Text = "ไม่มีหน่วยงานแม่", Value = null });
                    return; // Exit early since no parent units needed
                case 2: // Division - parent should be SubCommittee
                    validParents = activeUnits.Where(u => u.OrgUnitType == 1);
                    break;
                case 3: // Department - parent should be Division
                    validParents = activeUnits.Where(u => u.OrgUnitType == 2);
                    break;
                default:
                    validParents = activeUnits;
                    break;
            }

            // Add valid parent units to combobox
            foreach (var unit in validParents.OrderBy(u => u.OrgUnitNameTH ?? u.Name))
            {
                string tierName = unit.OrgUnitType == 1 ? "คณะกรรมการ" 
                                : unit.OrgUnitType == 2 ? "ฝ่าย" 
                                : "แผนก";
                
                cmbParent.Items.Add(new ComboBoxItem 
                { 
                    Text = $"[{tierName}] {unit.OrgUnitNameTH ?? unit.Name}", 
                    Value = unit.OrgUnitID 
                });
            }

            cmbParent.DisplayMember = "Text";
            cmbParent.ValueMember = "Value";
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void txtDivisionCode_TextChanged(object sender, EventArgs e)
        {
            UpdateOrgCode();
        }

        private void txtDepartmentCode_TextChanged(object sender, EventArgs e)
        {
            UpdateOrgCode();
        }

        private void UpdateOrgCode()
        {
            string divCode = txtDivisionCode.Text.PadLeft(2, '0');
            string deptCode = txtDepartmentCode.Text.PadLeft(2, '0');
            
            if (txtDivisionCode.Text.Length > 0 && txtDepartmentCode.Text.Length > 0)
            {
                string newCode = divCode + deptCode;
                txtOrgCode.Text = newCode;
                
                // Check for duplicate code
                CheckForDuplicateCode(newCode);
            }
            else if (txtDivisionCode.Text.Length > 0)
            {
                txtOrgCode.Text = divCode + "00";
            }
            else
            {
                txtOrgCode.Text = "";
            }
        }

        private void CheckForDuplicateCode(string orgCode)
        {
            if (string.IsNullOrWhiteSpace(orgCode)) return;

            // Check if code already exists in the provided units
            var existingUnit = _allUnits.FirstOrDefault(u => 
                u.OrgUnitCode == orgCode && u.IsActive && !u.IsDeleted);

            if (existingUnit != null)
            {
                txtOrgCode.BackColor = Color.FromArgb(255, 235, 235); // Light red
                txtOrgCode.ForeColor = Color.FromArgb(220, 53, 69);   // Red text
                txtOrgCode.Font = new Font(txtOrgCode.Font, FontStyle.Bold);
            }
            else
            {
                txtOrgCode.BackColor = Color.FromArgb(240, 240, 240); // Light gray (original)
                txtOrgCode.ForeColor = Color.Black;
                txtOrgCode.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }
        }

        private void btnSearchTH_Click(object sender, EventArgs e)
        {
            string keyword = txtOrgNameTH.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                MessageBox.Show("กรุณากรอกชื่อหน่วยงาน (ภาษาไทย) ที่ต้องการค้นหา", "ค้นหา",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var matches = _allUnits
                .Where(u => u.OrgUnitNameTH != null &&
                            u.OrgUnitNameTH.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            ShowSearchResults(matches, string.Format("ผลการค้นหา (ไทย): \"{0}\"", keyword));
        }

        private void btnSearchEN_Click(object sender, EventArgs e)
        {
            string keyword = txtOrgNameEN.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                MessageBox.Show("Please enter an English name to search.", "Search",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var matches = _allUnits
                .Where(u => (u.OrgUnitNameEN != null &&
                             u.OrgUnitNameEN.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                            (u.Name != null &&
                             u.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();

            ShowSearchResults(matches, string.Format("Search results (EN): \"{0}\"", keyword));
        }

        private void ShowSearchResults(List<OrgUnit> matches, string title)
        {
            if (!matches.Any())
            {
                MessageBox.Show("ไม่พบหน่วยงานที่ตรงกัน", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format("พบ {0} หน่วยงาน:\n", matches.Count));
            foreach (var u in matches)
            {
                string tier = u.OrgUnitType == 1 ? "คณะกรรมการ" : u.OrgUnitType == 2 ? "ฝ่าย" : "แผนก";
                sb.AppendLine(string.Format("  [{0}] {1}  (รหัส: {2})", tier, u.OrgUnitNameTH ?? u.Name, u.OrgUnitCode));
            }
            MessageBox.Show(sb.ToString(), title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void pnlFooter_Resize(object sender, EventArgs e)
        {
            btnCancel.Left = pnlFooter.ClientSize.Width - btnCancel.Width - 8;
            btnOk.Left = btnCancel.Left - btnOk.Width - 8;
            btnOk.Top = btnCancel.Top = (pnlFooter.Height - btnOk.Height) / 2;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                CreateNewOrgUnit();
                DialogResult = DialogResult.OK;
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtDivisionCode.Text))
            {
                MessageBox.Show("กรุณากรอกรหัสฝ่าย (2 หลัก)", "แจ้งเตือน", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDivisionCode.Focus();
                return false;
            }

            if (cmbLevel.SelectedItem == null)
            {
                MessageBox.Show("กรุณาเลือกระดับหน่วยงาน", "แจ้งเตือน", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbLevel.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDepartmentCode.Text))
            {
                MessageBox.Show("กรุณากรอกรหัสแผนก (2 หลัก)", "แจ้งเตือน", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDepartmentCode.Focus();
                return false;
            }

            // Check for duplicate organization code
            string orgCode = txtOrgCode.Text;
            if (!string.IsNullOrWhiteSpace(orgCode))
            {
                var existingUnit = _allUnits.FirstOrDefault(u => 
                    u.OrgUnitCode == orgCode && u.IsActive && !u.IsDeleted);
                
                if (existingUnit != null)
                {
                    MessageBox.Show(
                        string.Format("รหัสหน่วยงาน '{0}' มีอยู่ในระบบแล้ว\nหน่วยงาน: {1}\n\nกรุณาเปลี่ยนรหัสฝ่ายหรือรหัสแผนก", 
                            orgCode, existingUnit.OrgUnitNameTH ?? existingUnit.Name), 
                        "รหัสซ้ำ", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtDivisionCode.Focus();
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(txtOrgNameEN.Text))
            {
                MessageBox.Show("กรุณากรอกชื่อหน่วยงาน (ภาษาอังกฤษ)", "แจ้งเตือน", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOrgNameEN.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtOrgNameTH.Text))
            {
                MessageBox.Show("กรุณากรอกชื่อหน่วยงาน (ภาษาไทย)", "แจ้งเตือน", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOrgNameTH.Focus();
                return false;
            }

            var selectedLevel = (ComboBoxItem)cmbLevel.SelectedItem;
            if ((int)selectedLevel.Value > 1 && cmbParent.SelectedItem == null)
            {
                MessageBox.Show("กรุณาเลือกหน่วยงานแม่", "แจ้งเตือน", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbParent.Focus();
                return false;
            }

            return true;
        }

        private void CreateNewOrgUnit()
        {
            var selectedLevel = (ComboBoxItem)cmbLevel.SelectedItem;
            var selectedParent = cmbParent.SelectedItem as ComboBoxItem;

            NewOrgUnit = new OrgUnit
            {
                Name = txtOrgNameEN.Text?.Trim(),
                OrgUnitNameTH = txtOrgNameTH.Text?.Trim(),
                OrgUnitCode = txtOrgCode.Text?.Trim(),
                OrgUnitType = (int)selectedLevel.Value,
                ParentOrgUnitID = selectedParent?.Value as int?,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.Now
            };
        }

        /// <summary>Helper class for ComboBox items</summary>
        public class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
        }

        /// <summary>ข้อมูลหน่วยงานที่เลือก พร้อม PrincipalType สำหรับ FolderPermission (for backward compatibility)</summary>
        public class OrgUnitSelection
        {
            public OrgUnit OrgUnit       { get; set; }
            public int     PrincipalType { get; set; }  // 3=Department, 4=Division
            public string  DisplayName
            {
                get
                {
                    string tier = PrincipalType == 4 ? "ฝ่าย" : "แผนก";
                    return string.Format("[{0}] {1}", tier, OrgUnit.OrgUnitNameTH ?? OrgUnit.Name);
                }
            }
        }
    }
}
