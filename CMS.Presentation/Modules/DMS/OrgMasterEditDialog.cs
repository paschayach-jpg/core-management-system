using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.DMS
{
    public class OrgMasterEditDialog : Form
    {
        private static readonly Color NavyColor = Color.FromArgb(31, 56, 100);

        public OrgMasterType MasterType { get; }
        public bool IsEdit { get; }
        public object ResultItem { get; private set; }

        private Panel pnlHeader;
        private Label lblTitle;
        private TableLayoutPanel tblContent;
        private FlowLayoutPanel pnlButtons;
        private Button btnSave;
        private Button btnCancel;

        // Form Fields
        private TextBox txtCode;
        private TextBox txtName;
        private TextBox txtNameEn;
        private ComboBox cboDivision;
        private ComboBox cboDepartment;
        private ComboBox cboPosition;
        private ComboBox cboLevel;
        private TextBox txtCategory;
        private NumericUpDown numSort;
        private NumericUpDown numHeadCount;
        private CheckBox chkActive;

        public OrgMasterEditDialog(
            OrgMasterType masterType,
            bool isEdit,
            object initialData,
            IEnumerable<HrDivision> divisions = null,
            IEnumerable<HrDepartment> departments = null,
            IEnumerable<HrPosition> positions = null,
            IEnumerable<HrPositionLevel> levels = null)
        {
            MasterType = masterType;
            IsEdit = isEdit;

            InitializeUI();
            BindDropdowns(divisions, departments, positions, levels);
            LoadData(initialData);
        }

        private void InitializeUI()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.ClientSize = new Size(520, 360);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Tahoma", 9.5f);

            // Header
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = NavyColor };
            lblTitle = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 0, 0, 0),
                Text = (IsEdit ? "✏ แก้ไขข้อมูล " : "+ เพิ่มข้อมูล ") + GetTypeName(MasterType),
                Font = new Font("Tahoma", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblTitle);

            // Content Table
            tblContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 14, 20, 10),
                ColumnCount = 2,
                RowCount = 6,
                AutoSize = true
            };
            tblContent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tblContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Buttons
            pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(240, 244, 249),
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(16, 9, 16, 9)
            };
            btnCancel = new Button { Text = "ยกเลิก", Size = new Size(90, 32), BackColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            btnSave = new Button { Text = "บันทึก", Size = new Size(90, 32), BackColor = Color.FromArgb(30, 110, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Tahoma", 9.5f, FontStyle.Bold) };
            btnSave.Click += btnSave_Click;

            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnSave);

            this.Controls.Add(tblContent);
            this.Controls.Add(pnlButtons);
            this.Controls.Add(pnlHeader);
        }

        private static string GetTypeName(OrgMasterType type)
        {
            switch (type)
            {
                case OrgMasterType.Division: return "ฝ่าย (Division)";
                case OrgMasterType.Department: return "แผนก (Department)";
                case OrgMasterType.Position: return "ตำแหน่ง (Position)";
                case OrgMasterType.Level: return "ระดับตำแหน่ง (Level)";
                case OrgMasterType.Mapping: return "ผังโครงสร้างตำแหน่ง (Mapping)";
                default: return "Master Data";
            }
        }

        private void BindDropdowns(
            IEnumerable<HrDivision> divisions,
            IEnumerable<HrDepartment> departments,
            IEnumerable<HrPosition> positions,
            IEnumerable<HrPositionLevel> levels)
        {
            txtCode = new TextBox { Width = 280 };
            txtName = new TextBox { Width = 280 };
            txtNameEn = new TextBox { Width = 280 };
            txtCategory = new TextBox { Width = 280 };
            numSort = new NumericUpDown { Width = 120, Minimum = 0, Maximum = 9999, Value = 1 };
            numHeadCount = new NumericUpDown { Width = 120, Minimum = 1, Maximum = 999, Value = 1 };
            chkActive = new CheckBox { Text = "เปิดใช้งาน", Checked = true, AutoSize = true };

            cboDivision = new ComboBox { Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
            cboDepartment = new ComboBox { Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
            cboPosition = new ComboBox { Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
            cboLevel = new ComboBox { Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };

            if (divisions != null)
            {
                cboDivision.Items.Add(new ComboItem { Id = 0, Text = "(ไม่ระบุ)" });
                foreach (var d in divisions.OrderBy(x => x.DivisionCode))
                    cboDivision.Items.Add(new ComboItem { Id = d.DivisionID, Code = d.DivisionCode, Text = $"[{d.DivisionCode}] {d.DivisionName}" });
                cboDivision.SelectedIndex = 0;
            }

            if (departments != null)
            {
                cboDepartment.Items.Add(new ComboItem { Id = 0, Text = "(ไม่ระบุ)" });
                foreach (var d in departments.OrderBy(x => x.DepartmentCode))
                    cboDepartment.Items.Add(new ComboItem { Id = d.DepartmentID, Code = d.DepartmentCode, Text = $"[{d.DepartmentCode}] {d.DepartmentNameTh ?? d.DepartmentName}" });
                cboDepartment.SelectedIndex = 0;
            }

            if (positions != null)
            {
                cboPosition.Items.Add(new ComboItem { Id = 0, Text = "(ไม่ระบุ)" });
                foreach (var p in positions.OrderBy(x => x.PositionName))
                    cboPosition.Items.Add(new ComboItem { Id = p.PositionID, Code = p.PositionCode, Text = p.PositionName });
                cboPosition.SelectedIndex = 0;
            }

            if (levels != null)
            {
                cboLevel.Items.Add("");
                foreach (var l in levels.OrderBy(x => x.SortOrder ?? 999).ThenBy(x => x.LevelCode))
                    cboLevel.Items.Add(l.LevelCode);
                cboLevel.SelectedIndex = 0;
            }

            // Build dynamic fields based on MasterType
            tblContent.Controls.Clear();
            int row = 0;

            if (MasterType == OrgMasterType.Division)
            {
                AddRow("รหัสฝ่าย (Code):*", txtCode, row++);
                AddRow("ชื่อฝ่าย (Name):*", txtName, row++);
                AddRow("สถานะ:", chkActive, row++);
            }
            else if (MasterType == OrgMasterType.Department)
            {
                AddRow("สังกัดฝ่าย:*", cboDivision, row++);
                AddRow("รหัสแผนก (Code):*", txtCode, row++);
                AddRow("ชื่อแผนก (ไทย):*", txtName, row++);
                AddRow("ชื่อแผนก (Eng):", txtNameEn, row++);
                AddRow("สถานะ:", chkActive, row++);
            }
            else if (MasterType == OrgMasterType.Position)
            {
                AddRow("รหัสตำแหน่ง:", txtCode, row++);
                AddRow("ชื่อตำแหน่ง (Name):*", txtName, row++);
                AddRow("ระดับตำแหน่ง (Level):", cboLevel, row++);
                AddRow("สถานะ:", chkActive, row++);
            }
            else if (MasterType == OrgMasterType.Level)
            {
                AddRow("รหัสระดับ (LevelCode):*", txtCode, row++);
                AddRow("ชื่อระดับ (LevelName):", txtName, row++);
                AddRow("หมวดหมู่ (Category):", txtCategory, row++);
                AddRow("ลำดับการเรียง (Sort):", numSort, row++);
            }
            else if (MasterType == OrgMasterType.Mapping)
            {
                AddRow("ฝ่าย (Division):*", cboDivision, row++);
                AddRow("แผนก (Department):*", cboDepartment, row++);
                AddRow("ตำแหน่ง (Position):*", cboPosition, row++);
                AddRow("จำนวนกรอบ (HeadCount):", numHeadCount, row++);
                AddRow("สถานะ:", chkActive, row++);
            }
        }

        private void AddRow(string labelText, Control inputControl, int row)
        {
            var lbl = new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Tahoma", 9.5f, FontStyle.Bold) };
            tblContent.Controls.Add(lbl, 0, row);
            tblContent.Controls.Add(inputControl, 1, row);
        }

        private void LoadData(object data)
        {
            if (data == null) return;

            if (data is HrDivision div)
            {
                txtCode.Text = div.DivisionCode ?? "";
                txtName.Text = div.DivisionName ?? "";
                chkActive.Checked = div.IsActive;
                if (IsEdit) txtCode.ReadOnly = true;
            }
            else if (data is HrDepartment dept)
            {
                txtCode.Text = dept.DepartmentCode ?? "";
                txtName.Text = dept.DepartmentNameTh ?? dept.DepartmentName ?? "";
                txtNameEn.Text = dept.DepartmentNameEn ?? "";
                chkActive.Checked = dept.IsActive;
                SelectComboById(cboDivision, dept.DivisionID);
                if (IsEdit) txtCode.ReadOnly = true;
            }
            else if (data is HrPosition pos)
            {
                txtCode.Text = pos.PositionCode ?? "";
                txtName.Text = pos.PositionName ?? "";
                chkActive.Checked = pos.IsActive;
                cboLevel.SelectedItem = pos.LevelCode;
            }
            else if (data is HrPositionLevel lvl)
            {
                txtCode.Text = lvl.LevelCode ?? "";
                txtName.Text = lvl.LevelName ?? "";
                txtCategory.Text = lvl.LevelCategory ?? "";
                numSort.Value = lvl.SortOrder ?? 1;
                if (IsEdit) txtCode.ReadOnly = true;
            }
            else if (data is HrPositionMapping map)
            {
                SelectComboById(cboDivision, map.DivisionID);
                SelectComboById(cboDepartment, map.DepartmentID ?? 0);
                SelectComboById(cboPosition, map.PositionID);
                numHeadCount.Value = map.HeadCount > 0 ? map.HeadCount : 1;
                chkActive.Checked = map.IsActive;
            }
        }

        private void SelectComboById(ComboBox cbo, int id)
        {
            for (int i = 0; i < cbo.Items.Count; i++)
            {
                if (cbo.Items[i] is ComboItem item && item.Id == id)
                {
                    cbo.SelectedIndex = i;
                    break;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (MasterType == OrgMasterType.Division)
            {
                if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("กรุณากรอกรหัสฝ่ายและชื่อฝ่าย", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                ResultItem = new HrDivision
                {
                    DivisionCode = txtCode.Text.Trim(),
                    DivisionName = txtName.Text.Trim(),
                    IsActive = chkActive.Checked
                };
            }
            else if (MasterType == OrgMasterType.Department)
            {
                var selDiv = cboDivision.SelectedItem as ComboItem;
                if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text) || selDiv == null || selDiv.Id == 0)
                {
                    MessageBox.Show("กรุณาเลือกฝ่าย และกรอกรหัส/ชื่อแผนก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                ResultItem = new HrDepartment
                {
                    DivisionID = selDiv.Id,
                    DepartmentCode = txtCode.Text.Trim(),
                    DepartmentName = txtName.Text.Trim(),
                    DepartmentNameTh = txtName.Text.Trim(),
                    DepartmentNameEn = txtNameEn.Text.Trim(),
                    Level = "DEP",
                    IsActive = chkActive.Checked
                };
            }
            else if (MasterType == OrgMasterType.Position)
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("กรุณากรอกชื่อตำแหน่ง", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                ResultItem = new HrPosition
                {
                    PositionCode = txtCode.Text.Trim(),
                    PositionName = txtName.Text.Trim(),
                    LevelCode = cboLevel.SelectedItem?.ToString() ?? "",
                    IsActive = chkActive.Checked
                };
            }
            else if (MasterType == OrgMasterType.Level)
            {
                if (string.IsNullOrWhiteSpace(txtCode.Text))
                {
                    MessageBox.Show("กรุณากรอกรหัสระดับ (LevelCode)", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                ResultItem = new HrPositionLevel
                {
                    LevelCode = txtCode.Text.Trim().ToUpper(),
                    LevelName = txtName.Text.Trim(),
                    LevelCategory = txtCategory.Text.Trim(),
                    SortOrder = (int)numSort.Value,
                    IsActive = true
                };
            }
            else if (MasterType == OrgMasterType.Mapping)
            {
                var selDiv = cboDivision.SelectedItem as ComboItem;
                var selDept = cboDepartment.SelectedItem as ComboItem;
                var selPos = cboPosition.SelectedItem as ComboItem;

                if (selDiv == null || selDiv.Id == 0 || selDept == null || selDept.Id == 0 || selPos == null || selPos.Id == 0)
                {
                    MessageBox.Show("กรุณาเลือก ฝ่าย, แผนก และตำแหน่ง ให้ครบถ้วน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ResultItem = new HrPositionMapping
                {
                    DivisionID = selDiv.Id,
                    DepartmentID = selDept.Id,
                    PositionID = selPos.Id,
                    HeadCount = (int)numHeadCount.Value,
                    IsActive = chkActive.Checked
                };
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private class ComboItem
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Text { get; set; }
            public override string ToString() => Text;
        }
    }
}
