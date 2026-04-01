using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    public partial class EmployeeOrgSelectionDialog : Form
    {
        public List<EmployeeOrgUnit> SelectedUnits { get; private set; } = new List<EmployeeOrgUnit>();

        private readonly IEmployeeApiService _apiService;

        private List<DivisionGroup>   _divisionGroups = new List<DivisionGroup>();
        private List<EmployeeOrgUnit> _filteredItems  = new List<EmployeeOrgUnit>();

        public EmployeeOrgSelectionDialog(IEmployeeApiService apiService)
        {
            if (apiService == null) throw new ArgumentNullException("apiService");
            _apiService = apiService;
            InitializeComponent();
            
            // ให้ปุ่มชิดขวาเสมอเมื่อ resize
            pnlBottom.SizeChanged += (s, e) =>
            {
                btnCancel.Location = new System.Drawing.Point(pnlBottom.Width - 90, 8);
                btnOK.Location     = new System.Drawing.Point(pnlBottom.Width - 200, 8);
            };
            
            this.Load += (s, e) => LoadData();
        }

        // ── Load & Group from API ─────────────────────────────────────────────
        private void LoadData()
        {
            try
            {
                lblStatus.Text = "กำลังโหลดข้อมูลพนักงานจาก API...";
                Application.DoEvents();

                var employees = _apiService.GetAllEmployees().ToList();

                _divisionGroups = employees
                    .Where(e => !string.IsNullOrEmpty(e.DivisionCode))
                    .GroupBy(e => e.DivisionCode)
                    .Select(divGroup => new DivisionGroup
                    {
                        DivisionId   = divGroup.First().DivisionId,
                        DivisionCode = divGroup.Key,
                        DivisionName = divGroup.First().DivisionNameTh
                                    ?? divGroup.First().DivisionNameEn
                                    ?? divGroup.Key,
                        Departments  = divGroup
                            .Where(e => !string.IsNullOrEmpty(e.DepartmentCode))
                            .GroupBy(e => e.DepartmentCode)
                            .Select(deptGroup => new DepartmentGroup
                            {
                                DepartmentId   = deptGroup.First().DepartmentId,
                                DepartmentCode = deptGroup.Key,
                                DepartmentName = deptGroup.First().DepartmentNameTh
                                              ?? deptGroup.First().DepartmentNameEn
                                              ?? deptGroup.Key
                            })
                            .OrderBy(d => d.DepartmentCode)
                            .ToList()
                    })
                    .OrderBy(d => d.DivisionCode)
                    .ToList();

                lblStatus.Text = string.Format("โหลดสำเร็จ: {0} ฝ่าย | {1} แผนก",
                    _divisionGroups.Count,
                    _divisionGroups.Sum(d => d.Departments.Count));

                // Populate Division combobox
                cmbDivision.Items.Clear();
                cmbDivision.Items.Add(new ComboItem { Id = 0, Code = "", Name = "— ทุกฝ่าย —" });
                foreach (var div in _divisionGroups)
                    cmbDivision.Items.Add(new ComboItem
                    {
                        Id   = div.DivisionId,
                        Code = div.DivisionCode,
                        Name = string.Format("[{0}] {1}", div.DivisionCode, div.DivisionName)
                    });
                cmbDivision.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "เกิดข้อผิดพลาด: " + ex.Message;
                MessageBox.Show("ไม่สามารถโหลดข้อมูลได้:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ── Division changed ──────────────────────────────────────────────────
        private void cmbDivision_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sel = cmbDivision.SelectedItem as ComboItem;

            cmbDepartment.Items.Clear();
            cmbDepartment.Items.Add(new ComboItem { Id = 0, Code = "", Name = "— ทุกแผนก —" });

            if (sel != null && sel.Id != 0)
            {
                var divGroup = _divisionGroups.FirstOrDefault(d => d.DivisionId == sel.Id);
                if (divGroup != null)
                    foreach (var dept in divGroup.Departments)
                        cmbDepartment.Items.Add(new ComboItem
                        {
                            Id   = dept.DepartmentId,
                            Code = dept.DepartmentCode,
                            Name = string.Format("[{0}] {1}", dept.DepartmentCode, dept.DepartmentName)
                        });
            }
            else
            {
                foreach (var div in _divisionGroups)
                    foreach (var dept in div.Departments)
                        cmbDepartment.Items.Add(new ComboItem
                        {
                            Id   = dept.DepartmentId,
                            Code = dept.DepartmentCode,
                            Name = string.Format("[{0}] {1}", dept.DepartmentCode, dept.DepartmentName)
                        });
            }

            cmbDepartment.SelectedIndex = 0;
        }

        // ── Department changed ────────────────────────────────────────────────
        private void cmbDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            lstResult.Items.Clear();
            _filteredItems.Clear();

            var selDiv  = cmbDivision.SelectedItem   as ComboItem;
            var selDept = cmbDepartment.SelectedItem as ComboItem;

            if (selDiv != null && selDiv.Id != 0 && selDept != null && selDept.Id != 0)
            {
                var divGroup  = _divisionGroups.FirstOrDefault(d => d.DivisionId == selDiv.Id);
                var deptGroup = divGroup != null
                    ? divGroup.Departments.FirstOrDefault(d => d.DepartmentId == selDept.Id)
                    : null;
                if (deptGroup != null) AddDeptItem(deptGroup, divGroup);
            }
            else if (selDiv != null && selDiv.Id != 0)
            {
                var divGroup = _divisionGroups.FirstOrDefault(d => d.DivisionId == selDiv.Id);
                if (divGroup != null)
                {
                    AddDivItem(divGroup);
                    foreach (var dept in divGroup.Departments)
                        AddDeptItem(dept, divGroup);
                }
            }
            else
            {
                foreach (var div in _divisionGroups)
                {
                    AddDivItem(div);
                    foreach (var dept in div.Departments)
                        AddDeptItem(dept, div);
                }
            }

            lblStatus.Text = string.Format("แสดง {0} รายการ", _filteredItems.Count);
            UpdateCount();
        }

        private void AddDivItem(DivisionGroup div)
        {
            lstResult.Items.Add(string.Format("📁 ฝ่าย  [{0}] {1}", div.DivisionCode, div.DivisionName));
            _filteredItems.Add(new EmployeeOrgUnit
            {
                Id = div.DivisionId, Code = div.DivisionCode,
                NameTh = div.DivisionName, Type = "Division"
            });
        }

        private void AddDeptItem(DepartmentGroup dept, DivisionGroup parentDiv)
        {
            lstResult.Items.Add(string.Format("    📋 แผนก  [{0}] {1}", dept.DepartmentCode, dept.DepartmentName));
            _filteredItems.Add(new EmployeeOrgUnit
            {
                Id = dept.DepartmentId, Code = dept.DepartmentCode,
                NameTh = dept.DepartmentName, Type = "Department",
                ParentCode = parentDiv != null ? parentDiv.DivisionCode : null
            });
        }

        // ── Button handlers ───────────────────────────────────────────────────
        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            SetAllChecked(true);
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            SetAllChecked(false);
        }

        private void SetAllChecked(bool check)
        {
            for (int i = 0; i < lstResult.Items.Count; i++)
                lstResult.SetItemChecked(i, check);
            UpdateCount();
        }

        private void lstResult_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((Action)UpdateCount);
        }

        private void UpdateCount()
        {
            lblCount.Text = string.Format("เลือกแล้ว: {0} รายการ", lstResult.CheckedIndices.Count);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SelectedUnits.Clear();
            foreach (int idx in lstResult.CheckedIndices)
                if (idx < _filteredItems.Count)
                    SelectedUnits.Add(_filteredItems[idx]);

            if (SelectedUnits.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกหน่วยงานอย่างน้อย 1 รายการ", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        // ── Inner classes ─────────────────────────────────────────────────────
        private class ComboItem
        {
            public int    Id   { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public override string ToString() { return Name; }
        }

        private class DivisionGroup
        {
            public int    DivisionId   { get; set; }
            public string DivisionCode { get; set; }
            public string DivisionName { get; set; }
            public List<DepartmentGroup> Departments { get; set; } = new List<DepartmentGroup>();
        }

        private class DepartmentGroup
        {
            public int    DepartmentId   { get; set; }
            public string DepartmentCode { get; set; }
            public string DepartmentName { get; set; }
        }
    }

    // ── Public model ─────────────────────────────────────────────────────────
    public class EmployeeOrgUnit
    {
        public int    Id         { get; set; }
        public string Code       { get; set; }
        public string NameTh     { get; set; }
        public string Type       { get; set; }
        public string ParentCode { get; set; }
        public override string ToString() { return string.Format("[{0}] {1}", Code, NameTh); }
    }
}
