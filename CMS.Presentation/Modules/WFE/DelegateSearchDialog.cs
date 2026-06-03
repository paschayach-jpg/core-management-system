using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using CMS.Infrastructure.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    public class DelegateSearchDialog : Form
    {
        private readonly IEmployeeApiService _employeeApiService;
        private readonly ILdapUserSearchService _ldapSearchService;
        private readonly ILoggingService _loggingService;
        private readonly string _currentUserName;

        private TextBox txtSearch;
        private ListView lstResults;
        private Button btnOK;
        private Button btnCancel;
        private Label lblStatus;

        private List<LdapUserResult> _searchResults = new List<LdapUserResult>();
        public LdapUserResult SelectedUser { get; private set; }

        public DelegateSearchDialog(
            IEmployeeApiService employeeApiService,
            ILdapUserSearchService ldapSearchService,
            ILoggingService loggingService,
            string currentUserName)
        {
            _employeeApiService = employeeApiService;
            _ldapSearchService = ldapSearchService;
            _loggingService = loggingService;
            _currentUserName = currentUserName;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "ค้นหาผู้รับมอบหมาย";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.FromArgb(243, 246, 251);

            var lblPrompt = new Label
            {
                Text = "ค้นหาบุคคล (พิมพ์ชื่อ ภาษาไทย/อังกฤษ หรือ AD Username):",
                Location = new Point(16, 16),
                Size = new Size(400, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 56, 100),
            };

            txtSearch = new TextBox
            {
                Location = new Point(16, 40),
                Size = new Size(440, 26),
                BorderStyle = BorderStyle.FixedSingle,
            };
            txtSearch.KeyDown += TxtSearch_KeyDown;

            var btnSearch = new Button
            {
                Text = "ค้นหา",
                Location = new Point(464, 38),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += BtnSearch_Click;

            lstResults = new ListView
            {
                Location = new Point(16, 80),
                Size = new Size(548, 260),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                BorderStyle = BorderStyle.FixedSingle,
            };
            lstResults.Columns.Add("Username", 100);
            lstResults.Columns.Add("ชื่อ-นามสกุล", 180);
            lstResults.Columns.Add("ตำแหน่ง", 130);
            lstResults.Columns.Add("แผนก/ฝ่าย", 130);
            lstResults.DoubleClick += LstResults_DoubleClick;

            lblStatus = new Label
            {
                Text = "ระบุคำค้นหาอย่างน้อย 2 ตัวอักษร แล้วกดค้นหา",
                Location = new Point(16, 348),
                Size = new Size(400, 20),
                ForeColor = Color.FromArgb(100, 110, 130),
            };

            btnOK = new Button
            {
                Text = "ตกลง",
                Location = new Point(356, 368),
                Size = new Size(100, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK,
                Enabled = false,
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button
            {
                Text = "ยกเลิก",
                Location = new Point(464, 368),
                Size = new Size(100, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(31, 56, 100),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel,
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(220, 225, 235);

            this.Controls.Add(lblPrompt);
            this.Controls.Add(txtSearch);
            this.Controls.Add(btnSearch);
            this.Controls.Add(lstResults);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);

            lstResults.SelectedIndexChanged += LstResults_SelectedIndexChanged;
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                PerformSearch();
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            string keyword = txtSearch.Text.Trim();
            if (keyword.Length < 2)
            {
                MessageBox.Show("กรุณากรอกคำค้นหาอย่างน้อย 2 ตัวอักษร", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblStatus.Text = "กำลังค้นหา...";
            lstResults.Items.Clear();
            _searchResults.Clear();
            btnOK.Enabled = false;

            try
            {
                if (_ldapSearchService != null)
                {
                    var ldapUsers = _ldapSearchService.Search(keyword, maxResults: 30);
                    foreach (var u in ldapUsers)
                    {
                        if (string.Equals(u.SamAccountName, _currentUserName, StringComparison.OrdinalIgnoreCase))
                            continue; // Cannot delegate to self

                        _searchResults.Add(u);
                    }
                }
                else if (_employeeApiService != null)
                {
                    var employees = _employeeApiService.GetAllEmployees()
                        .Where(emp => string.Equals(emp.EmployeeCode, _currentUserName, StringComparison.OrdinalIgnoreCase) == false &&
                                      (emp.DisplayName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                       emp.EmployeeCode.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
                        .ToList();

                    foreach (var emp in employees)
                    {
                        _searchResults.Add(new LdapUserResult
                        {
                            SamAccountName = emp.EmployeeCode,
                            DisplayName = emp.DisplayName,
                            Department = emp.DepartmentNameTh ?? emp.DepartmentNameEn,
                            Title = "พนักงาน",
                            Mail = emp.EmployeeCode + "@company.co.th"
                        });
                    }
                }

                if (_searchResults.Count == 0)
                {
                    lblStatus.Text = "ไม่พบรายชื่อผู้รับมอบหมาย";
                }
                else
                {
                    lblStatus.Text = $"พบรายชื่อ {_searchResults.Count} รายการ";
                    lstResults.BeginUpdate();
                    foreach (var u in _searchResults)
                    {
                        var item = new ListViewItem(u.SamAccountName);
                        item.SubItems.Add(u.DisplayName ?? "");
                        item.SubItems.Add(u.Title ?? "");
                        item.SubItems.Add(u.Department ?? "");
                        item.Tag = u;
                        lstResults.Items.Add(item);
                    }
                    lstResults.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Search delegate failed: {ex.Message}", ex);
                lblStatus.Text = "เกิดข้อผิดพลาดในการค้นหา";
                MessageBox.Show("ไม่สามารถค้นหาข้อมูลได้: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LstResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = lstResults.SelectedItems.Count > 0;
        }

        private void LstResults_DoubleClick(object sender, EventArgs e)
        {
            if (lstResults.SelectedItems.Count > 0)
            {
                SelectAndClose();
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            SelectAndClose();
        }

        private void SelectAndClose()
        {
            if (lstResults.SelectedItems.Count > 0)
            {
                SelectedUser = lstResults.SelectedItems[0].Tag as LdapUserResult;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
