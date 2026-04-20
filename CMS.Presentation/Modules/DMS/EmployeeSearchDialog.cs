using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    public partial class EmployeeSearchDialog : Form
    {
        private readonly IEmployeeDirectoryController _employeeDirectoryController;

        public EmployeeSummary SelectedEmployee { get; private set; }

        private List<EmployeeSummary> _allEmployees = new List<EmployeeSummary>();

        public EmployeeSearchDialog(IEmployeeDirectoryController employeeDirectoryController)
        {
            if (employeeDirectoryController == null) throw new ArgumentNullException("employeeDirectoryController");
            _employeeDirectoryController = employeeDirectoryController;
            InitializeComponent();

            // Placeholder behavior
            txtSearch.GotFocus  += (s, e) => {
                if (txtSearch.Text == "พิมพ์ชื่อ นามสกุล หรือรหัสพนักงาน...")
                { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; }
            };
            txtSearch.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                { txtSearch.Text = "พิมพ์ชื่อ นามสกุล หรือรหัสพนักงาน..."; txtSearch.ForeColor = Color.Gray; }
            };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoSearch(); };
        }

        private void EmployeeSearchDialog_Load(object sender, EventArgs e)
        {
            LoadAllEmployees();
        }

        private void LoadAllEmployees()
        {
            lblStatus.Text      = "⏳ กำลังโหลดข้อมูลพนักงาน...";
            lblStatus.ForeColor = Color.FromArgb(43, 87, 154);
            Application.DoEvents();

            try
            {
                _allEmployees = _employeeDirectoryController.GetAllEmployees().ToList();

                cmbDepartment.Items.Clear();
                cmbDepartment.Items.Add(new ComboItem(0, "— ทุกแผนก —"));
                foreach (var d in _employeeDirectoryController.GetDepartments())
                    cmbDepartment.Items.Add(new ComboItem(d.DepartmentId, d.DisplayName));
                cmbDepartment.SelectedIndex = 0;

                cmbDivision.Items.Clear();
                cmbDivision.Items.Add(new ComboItem(0, "— ทุกฝ่าย —"));
                foreach (var v in _employeeDirectoryController.GetDivisions())
                    cmbDivision.Items.Add(new ComboItem(v.DivisionId, v.DisplayName));
                cmbDivision.SelectedIndex = 0;

                PopulateList(_allEmployees);
                lblStatus.Text      = string.Format("โหลดข้อมูลสำเร็จ {0} คน — พิมพ์เพื่อกรอง", _allEmployees.Count);
                lblStatus.ForeColor = Color.FromArgb(0, 120, 0);

                txtSearch.TextChanged += TxtSearch_TextChanged;
            }
            catch (Exception ex)
            {
                lblStatus.Text      = "⚠ โหลดข้อมูลไม่สำเร็จ: " + ex.Message;
                lblStatus.ForeColor = Color.DarkRed;
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e) => DoSearch();

        private void CmbDepartment_Changed(object sender, EventArgs e) => ApplyFilter();

        private void CmbDivision_Changed(object sender, EventArgs e) => ApplyFilter();

        private void TxtSearch_TextChanged(object sender, EventArgs e) => ApplyFilter();

        private void LvResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSelect.Enabled = lvResults.SelectedItems.Count > 0;
        }

        private void LvResults_DoubleClick(object sender, EventArgs e) => ConfirmSelection();

        private void BtnSelect_Click(object sender, EventArgs e) => ConfirmSelection();

        private void ApplyFilter()
        {
            string kw    = txtSearch.Text.Trim().ToLower();
            bool   hasKw = !string.IsNullOrEmpty(kw) && kw != "พิมพ์ชื่อ นามสกุล หรือรหัสพนักงาน...";

            var deptItem = cmbDepartment.SelectedItem as ComboItem;
            var divItem  = cmbDivision.SelectedItem  as ComboItem;
            int deptId   = deptItem != null ? deptItem.Id : 0;
            int divId    = divItem  != null ? divItem.Id  : 0;

            var filtered = _allEmployees.Where(emp =>
            {
                if (hasKw)
                {
                    bool match = (emp.EmployeeCode     ?? "").ToLower().Contains(kw) ||
                                 (emp.DisplayName      ?? "").ToLower().Contains(kw) ||
                                 (emp.DepartmentNameTh ?? "").ToLower().Contains(kw) ||
                                 (emp.DivisionNameTh   ?? "").ToLower().Contains(kw);
                    if (!match) return false;
                }
                if (deptId > 0 && emp.DepartmentId != deptId) return false;
                if (divId  > 0 && emp.DivisionId   != divId)  return false;
                return true;
            }).ToList();

            PopulateList(filtered);
            lblStatus.Text      = string.Format("พบ {0} คน", filtered.Count);
            lblStatus.ForeColor = filtered.Count > 0 ? Color.FromArgb(0, 100, 0) : Color.DarkRed;
        }

        private void DoSearch()
        {
            string kw = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(kw) && kw != "พิมพ์ชื่อ นามสกุล หรือรหัสพนักงาน...")
            {
                try
                {
                    var byCode = _employeeDirectoryController.GetByCode(kw);
                    if (byCode != null)
                    {
                        var summary = new EmployeeSummary
                        {
                            EmployeeCode     = byCode.EmployeeCode,
                            DisplayName      = byCode.DisplayName,
                            Company          = byCode.Company,
                            DepartmentId     = byCode.PrimaryPosition != null ? byCode.PrimaryPosition.DepartmentId     : 0,
                            DepartmentCode   = byCode.PrimaryPosition != null ? byCode.PrimaryPosition.DepartmentCode   : null,
                            DepartmentNameTh = byCode.PrimaryPosition != null ? byCode.PrimaryPosition.DepartmentNameTh : null,
                            DepartmentNameEn = byCode.PrimaryPosition != null ? byCode.PrimaryPosition.DepartmentNameEn : null,
                            DivisionId       = byCode.PrimaryPosition != null ? byCode.PrimaryPosition.DivisionId       : 0,
                            DivisionCode     = byCode.PrimaryPosition != null ? byCode.PrimaryPosition.DivisionCode     : null,
                            DivisionNameTh   = byCode.PrimaryPosition != null ? byCode.PrimaryPosition.DivisionNameTh   : null,
                            DivisionNameEn   = byCode.PrimaryPosition != null ? byCode.PrimaryPosition.DivisionNameEn   : null,
                        };
                        PopulateList(new[] { summary });
                        lblStatus.Text      = "พบ 1 คน";
                        lblStatus.ForeColor = Color.FromArgb(0, 100, 0);
                        lvResults.Focus();
                        return;
                    }
                }
                catch { }
            }
            ApplyFilter();
            if (lvResults.Items.Count > 0) lvResults.Focus();
        }

        private void PopulateList(IEnumerable<EmployeeSummary> employees)
        {
            lvResults.BeginUpdate();
            lvResults.Items.Clear();

            int row = 0;
            foreach (var emp in employees
                .OrderBy(e => e.DivisionNameTh  ?? e.DivisionNameEn  ?? "")
                .ThenBy( e => e.DepartmentNameTh ?? e.DepartmentNameEn ?? "")
                .ThenBy( e => e.DisplayName ?? ""))
            {
                string nameTh = emp.DisplayName ?? string.Empty;
                string nameEn = string.Empty;
                int open = nameTh.LastIndexOf('(');
                if (open > 0 && nameTh.EndsWith(")"))
                {
                    nameEn = nameTh.Substring(open + 1, nameTh.Length - open - 2).Trim();
                    nameTh = nameTh.Substring(0, open).Trim();
                }

                var item = new ListViewItem(emp.EmployeeCode ?? "—");
                item.SubItems.Add(nameTh);
                item.SubItems.Add(nameEn);
                item.SubItems.Add(emp.DivisionCode     ?? "—");
                item.SubItems.Add(emp.DivisionNameTh   ?? emp.DivisionNameEn   ?? "—");
                item.SubItems.Add(emp.DepartmentCode   ?? "—");
                item.SubItems.Add(emp.DepartmentNameTh ?? emp.DepartmentNameEn ?? "—");
                item.Tag       = emp;
                item.BackColor = row % 2 == 0 ? Color.White : Color.FromArgb(248, 249, 252);
                row++;
                lvResults.Items.Add(item);
            }

            lvResults.EndUpdate();
            btnSelect.Enabled = false;
        }

        private void ConfirmSelection()
        {
            if (lvResults.SelectedItems.Count == 0) return;
            SelectedEmployee = lvResults.SelectedItems[0].Tag as EmployeeSummary;
            if (SelectedEmployee == null) return;
            DialogResult = DialogResult.OK;
            Close();
        }

        private class ComboItem
        {
            public int    Id   { get; private set; }
            public string Name { get; private set; }
            public ComboItem(int id, string name) { Id = id; Name = name; }
            public override string ToString() { return Name; }
        }
    }
}
