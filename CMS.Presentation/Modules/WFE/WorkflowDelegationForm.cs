using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    /// <summary>
    /// Delegation settings page — delegate tasks to another user for a date range.
    /// </summary>
    public partial class WorkflowDelegationForm : Form
    {
        private readonly IWorkflowDelegationService _delegationService;
        private readonly IWorkflowService _workflowService;
        private readonly IEmployeeApiService _employeeApiService;
        private readonly ILdapUserSearchService _ldapSearchService;
        private readonly ILoggingService _loggingService;
        private readonly IAuditService _auditService;
        private readonly UserContext _userContext;

        private readonly Color _navyDark = Color.FromArgb(31, 56, 100);
        private readonly Color _navHover = Color.FromArgb(245, 249, 255);

        private List<EmployeeSummary> _employees = new List<EmployeeSummary>();
        private List<WorkflowTaskResult> _myTasks = new List<WorkflowTaskResult>();

        // Tracks last LDAP search results (for save validation)
        private List<CMS.Services.DTOs.LdapUserResult> _ldapResults = new List<CMS.Services.DTOs.LdapUserResult>();
        private CMS.Services.DTOs.LdapUserResult _selectedLdapUser = null;

        public WorkflowDelegationForm(
            IWorkflowDelegationService delegationService,
            IWorkflowService workflowService,
            IEmployeeApiService employeeApiService,
            ILoggingService loggingService,
            IAuditService auditService,
            UserContext userContext,
            ILdapUserSearchService ldapSearchService = null)
        {
            InitializeComponent();
            _delegationService  = delegationService  ?? throw new ArgumentNullException(nameof(delegationService));
            _workflowService    = workflowService    ?? throw new ArgumentNullException(nameof(workflowService));
            _employeeApiService = employeeApiService ?? throw new ArgumentNullException(nameof(employeeApiService));
            _loggingService     = loggingService     ?? throw new ArgumentNullException(nameof(loggingService));
            _auditService       = auditService       ?? throw new ArgumentNullException(nameof(auditService));
            _userContext        = userContext         ?? throw new ArgumentNullException(nameof(userContext));
            _ldapSearchService  = ldapSearchService; // optional — falls back to EmployeeApi
        }

        private void WorkflowDelegationForm_Load(object sender, EventArgs e)
        {
            BuildRibbonTabs();
            BuildToolbar();
            Panel initialNav;
            WorkflowNavigationBuilder.BuildSidebar(
                pnlSidebarContainer, "delegation", OnNavItemClick, out initialNav);

            LoadEmployees();
            LoadMyTasks();
            InitializeFormData();
            LoadHistoryRows();
        }

        // ─── Data loading ────────────────────────────────────────────────────────

        private void LoadEmployees()
        {
            try
            {
                _employees = _employeeApiService
                    .GetAllEmployees()
                    .Where(emp => emp.EmployeeCode != _userContext.ADUserName)
                    .OrderBy(emp => emp.DisplayName)
                    .ToList();
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading employees: {ex.Message}", ex);
                _employees = new List<EmployeeSummary>();
            }
        }

        private void LoadMyTasks()
        {
            try
            {
                _myTasks = _workflowService.GetMyTasks(_userContext);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading my tasks: {ex.Message}", ex);
                _myTasks = new List<WorkflowTaskResult>();
            }
        }

        private void InitializeFormData()
        {
            // From-user: read-only, built from UserContext
            var fromDisplay = _userContext.ADUserName;
            if (!string.IsNullOrEmpty(_userContext.PositionName))
                fromDisplay += " (" + _userContext.PositionName;
            if (!string.IsNullOrEmpty(_userContext.DepartmentName))
                fromDisplay += " - " + _userContext.DepartmentName;
            if (!string.IsNullOrEmpty(_userContext.PositionName))
                fromDisplay += ")";
            txtFromUser.Text = fromDisplay;

            // Delegate-to: searchable combo (type to filter) + autocomplete from employee list
            cmbDelegateTo.Items.Clear();
            cmbDelegateTo.DropDownStyle = ComboBoxStyle.DropDown;
            cmbDelegateTo.AutoCompleteMode   = AutoCompleteMode.SuggestAppend;
            cmbDelegateTo.AutoCompleteSource = AutoCompleteSource.CustomSource;

            var acSource = new AutoCompleteStringCollection();
            foreach (var emp in _employees)
            {
                cmbDelegateTo.Items.Add(emp.DropdownLabel);
                acSource.Add(emp.DropdownLabel);
            }
            cmbDelegateTo.AutoCompleteCustomSource = acSource;
            cmbDelegateTo.Text = string.Empty;

            // Attach search-as-you-type filter on TextChanged
            cmbDelegateTo.TextChanged -= cmbDelegateTo_TextChanged;
            cmbDelegateTo.TextChanged += cmbDelegateTo_TextChanged;

            // Scope dropdown: all + unique workflow names from current tasks
            cmbScope.Items.Clear();
            cmbScope.Items.Add("ทุก Workflow ของฉัน");
            var names = _myTasks
                .Select(t => t.WorkflowName)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .OrderBy(n => n);
            foreach (var name in names)
                cmbScope.Items.Add(name);
            cmbScope.SelectedIndex = 0;

            dtpStart.Value = DateTime.Now;
            dtpEnd.Value   = DateTime.Now.AddDays(7);

            chkEmail.Checked         = true;
            chkOutOfOffice.Checked   = true;
            chkRequireReason.Checked = false;

            txtReason.Text = string.Empty;
        }

        private void LoadHistoryRows()
        {
            dgvDelegations.Rows.Clear();

            if (_myTasks.Count == 0)
                return;

            try
            {
                var allDelegations = new List<WFDelegationResult>();

                foreach (var task in _myTasks)
                {
                    try
                    {
                        var history = _delegationService.GetDelegationHistory(task.WorkflowStepInstanceId);
                        if (history != null)
                            allDelegations.AddRange(history);
                    }
                    catch { /* skip individual step errors */ }
                }

                foreach (var d in allDelegations.OrderByDescending(d => d.DelegationDate))
                {
                    string status = d.DelegationDate > DateTime.Now ? "กำลังใช้งาน" : "สิ้นสุดแล้ว";
                    dgvDelegations.Rows.Add(
                        string.Format("{0} → {1}", d.DelegatedFrom, d.DelegatedTo),
                        "-",
                        d.DelegationDate.ToString("dd/MM/yy"),
                        "-",
                        status,
                        d.DelegationReason ?? "-"
                    );
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading delegation history: {ex.Message}", ex);
            }
        }

        // ─── Button handlers ─────────────────────────────────────────────────────

        // ─── Delegate-to LDAP auto-filter ───────────────────────────────────────

        private bool _suppressTextChanged = false;

        private void cmbDelegateTo_TextChanged(object sender, EventArgs e)
        {
            if (_suppressTextChanged) return;

            string keyword = cmbDelegateTo.Text.Trim();

            // Clear confirmed selection when text changes
            _selectedLdapUser = null;

            if (keyword.Length < 2)
            {
                cmbDelegateTo.Items.Clear();
                return;
            }

            if (_ldapSearchService != null)
            {
                // LDAP live search
                try
                {
                    _ldapResults = _ldapSearchService.Search(keyword, maxResults: 30);
                }
                catch (Exception ex)
                {
                    _loggingService.Error($"LDAP search error: {ex.Message}", ex);
                    _ldapResults = new List<CMS.Services.DTOs.LdapUserResult>();
                }

                _suppressTextChanged = true;
                int caretPos = cmbDelegateTo.SelectionStart;
                cmbDelegateTo.Items.Clear();
                foreach (var u in _ldapResults)
                    cmbDelegateTo.Items.Add(u.DropdownLabel);
                cmbDelegateTo.Text = keyword;
                cmbDelegateTo.SelectionStart  = caretPos;
                cmbDelegateTo.SelectionLength = 0;
                _suppressTextChanged = false;

                if (_ldapResults.Count > 0 && !cmbDelegateTo.DroppedDown)
                    cmbDelegateTo.DroppedDown = true;
            }
            else
            {
                // Fallback: filter local EmployeeApi list
                var matches = _employees
                    .Where(emp =>
                        emp.DropdownLabel.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        emp.DisplayName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        emp.EmployeeCode.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                _suppressTextChanged = true;
                int caretPos = cmbDelegateTo.SelectionStart;
                cmbDelegateTo.Items.Clear();
                foreach (var emp in matches)
                    cmbDelegateTo.Items.Add(emp.DropdownLabel);
                cmbDelegateTo.Text = keyword;
                cmbDelegateTo.SelectionStart  = caretPos;
                cmbDelegateTo.SelectionLength = 0;
                _suppressTextChanged = false;

                if (matches.Count > 0 && !cmbDelegateTo.DroppedDown)
                    cmbDelegateTo.DroppedDown = true;
            }
        }

        private void cmbDelegateTo_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // User picked an item — confirm selection
            int idx = cmbDelegateTo.SelectedIndex;
            if (idx < 0) return;

            if (_ldapSearchService != null && idx < _ldapResults.Count)
            {
                _selectedLdapUser = _ldapResults[idx];
            }
        }

        private string GetDelegateToUsername()
        {
            // LDAP path: use SamAccountName
            if (_selectedLdapUser != null)
                return _selectedLdapUser.SamAccountName;

            // EmployeeApi fallback: try matching text to employee code
            string text = cmbDelegateTo.Text.Trim();
            if (string.IsNullOrEmpty(text)) return null;

            var emp = _employees.FirstOrDefault(e2 =>
                string.Equals(e2.DropdownLabel, text, StringComparison.OrdinalIgnoreCase));
            return emp?.EmployeeCode;
        }

        private string GetDelegateToDisplayName()
        {
            if (_selectedLdapUser != null)
                return _selectedLdapUser.DisplayName;

            string text = cmbDelegateTo.Text.Trim();
            var emp = _employees.FirstOrDefault(e2 =>
                string.Equals(e2.DropdownLabel, text, StringComparison.OrdinalIgnoreCase));
            return emp?.DisplayName ?? text;
        }

        // ─── Button handlers ─────────────────────────────────────────────────────

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validate
            string delegateToUsername = GetDelegateToUsername();
            if (string.IsNullOrEmpty(delegateToUsername))
            {
                MessageBox.Show(
                    "กรุณาเลือกผู้รับมอบหมายให้ถูกต้อง\n(พิมพ์ชื่อหรือ Username แล้วเลือกจากรายการ)",
                    "ข้อมูลไม่ครบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbDelegateTo.Focus();
                return;
            }
            string delegateToDisplay = GetDelegateToDisplayName();

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("กรุณากรอกเหตุผลการมอบหมาย", "ข้อมูลไม่ครบ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtReason.Focus();
                return;
            }

            if (dtpEnd.Value <= dtpStart.Value)
            {
                MessageBox.Show("วันสิ้นสุดต้องมากกว่าวันเริ่มต้น", "ข้อมูลไม่ถูกต้อง",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpEnd.Focus();
                return;
            }

            bool scopeAll = cmbScope.SelectedIndex == 0;
            string scopeName = scopeAll ? null : cmbScope.SelectedItem.ToString();

            // Filter tasks that can be delegated and match scope
            var tasksToDelegate = _myTasks
                .Where(t => t.CanDelegate &&
                            t.CurrentDelegationDepth < t.MaxDelegationDepth &&
                            (scopeAll || t.WorkflowName == scopeName))
                .ToList();

            if (tasksToDelegate.Count == 0)
            {
                MessageBox.Show(
                    "ไม่มีงานที่สามารถมอบหมายได้ในขอบเขตที่เลือก\n" +
                    "(อาจถึงขีดจำกัดการมอบหมายแล้ว หรือไม่มีงานที่ยังค้างอยู่)",
                    "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                string.Format(
                    "ยืนยันมอบหมาย {0} รายการ\nถึง: {1} [{2}]\nช่วง: {3} - {4}",
                    tasksToDelegate.Count,
                    delegateToDisplay,
                    delegateToUsername,
                    dtpStart.Value.ToString("dd/MM/yyyy"),
                    dtpEnd.Value.ToString("dd/MM/yyyy")),
                "ยืนยันการมอบหมาย",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            int success = 0;
            int failed  = 0;
            string remarks = string.Format("ช่วงเวลา: {0} - {1}",
                dtpStart.Value.ToString("dd/MM/yyyy HH:mm"),
                dtpEnd.Value.ToString("dd/MM/yyyy HH:mm"));

            if (chkEmail.Checked)
                remarks += " | แจ้ง Email";
            if (chkOutOfOffice.Checked)
                remarks += " | Out-of-Office";
            if (chkRequireReason.Checked)
                remarks += " | ต้องยืนยัน";

            foreach (var task in tasksToDelegate)
            {
                try
                {
                    var request = new DelegateTaskRequest
                    {
                        WorkflowStepInstanceId = task.WorkflowStepInstanceId,
                        DelegateToUserName     = delegateToUsername,
                        DelegationReason       = txtReason.Text.Trim(),
                        Remarks                = remarks
                    };

                    _delegationService.DelegateTask(request, _userContext);

                    _auditService.Log(new AuditEntry
                    {
                        Action    = "WorkflowTaskDelegated",
                        TableName = "WorkflowStepInstance",
                        RecordID  = task.WorkflowStepInstanceId,
                        UserName  = _userContext.ADUserName,
                        AuditDate = DateTime.Now,
                        NewValue  = string.Format("Delegated to {0} ({1}): {2}",
                            delegateToUsername, delegateToDisplay, txtReason.Text.Trim())
                    });

                    success++;
                }
                catch (Exception ex)
                {
                    _loggingService.Error(
                        $"Error delegating task {task.WorkflowStepInstanceId}: {ex.Message}", ex);
                    failed++;
                }
            }

            _loggingService.Info(
                $"Delegation complete: {success} succeeded, {failed} failed. To: {delegateToUsername}");

            if (failed == 0)
            {
                MessageBox.Show(
                    $"มอบหมายงานสำเร็จ {success} รายการ",
                    "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    $"มอบหมายสำเร็จ {success} รายการ  |  ล้มเหลว {failed} รายการ\nดูรายละเอียดใน Application Log",
                    "ผลลัพธ์", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Reload to reflect new state
            LoadMyTasks();
            LoadHistoryRows();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            InitializeFormData();
        }

        // ─── Ribbon / Toolbar ────────────────────────────────────────────────────

        private void BuildRibbonTabs()
        {
            var tabs = new[]
            {
                new { Text = "แฟ้ม",    Active = false },
                new { Text = "หน้าแรก", Active = false },
                new { Text = "ดำเนินการ", Active = false },
                new { Text = "มุมมอง",  Active = false },
                new { Text = "เครื่องมือ", Active = true },
                new { Text = "ช่วยเหลือ", Active = false },
            };

            int x = 8;
            foreach (var t in tabs)
            {
                var btn = new Button
                {
                    Text      = t.Text,
                    Location  = new Point(x, 0),
                    Size      = new Size(MeasureTabWidth(t.Text) + 24, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = t.Active ? Color.White : Color.FromArgb(190, 210, 245),
                    Font      = t.Active ? new Font("Segoe UI", 9.5F, FontStyle.Bold) : new Font("Segoe UI", 9.5F),
                    Cursor    = Cursors.Hand,
                    TabStop   = false,
                };
                btn.FlatAppearance.BorderSize             = 0;
                btn.FlatAppearance.MouseOverBackColor     = Color.FromArgb(50, 80, 130);
                btn.FlatAppearance.MouseDownBackColor     = Color.FromArgb(40, 70, 120);

                if (t.Active)
                {
                    btn.Paint += (s, pe) =>
                        pe.Graphics.FillRectangle(Brushes.White, 0, btn.Height - 3, btn.Width, 3);
                }

                pnlRibbonTabs.Controls.Add(btn);
                x += btn.Width + 2;
            }
        }

        private int MeasureTabWidth(string text)
        {
            using (var g = CreateGraphics())
            using (var f = new Font("Segoe UI", 9.5F))
                return (int)g.MeasureString(text, f).Width;
        }

        private void BuildToolbar()
        {
            var groups = new[]
            {
                new { Header = "เพิ่ม", Items = new[]
                {
                    new { Icon = "+", Text = "สร้างใหม่", Tag = "new"     },
                    new { Icon = "✎", Text = "แก้ไข",    Tag = "edit"    },
                    new { Icon = "⌫", Text = "ยกเลิก",   Tag = "cancel"  },
                }},
                new { Header = "มุมมอง", Items = new[]
                {
                    new { Icon = "◷", Text = "ประวัติ", Tag = "history" },
                    new { Icon = "✓", Text = "Active",  Tag = "active"  },
                }},
            };

            int x = 8;
            foreach (var group in groups)
            {
                int groupStartX = x;
                foreach (var item in group.Items)
                {
                    var tag = item.Tag;
                    var pnlBtn = new Panel
                    {
                        Location  = new Point(x, 2),
                        Size      = new Size(58, 60),
                        BackColor = Color.White,
                        Cursor    = Cursors.Hand,
                        Tag       = tag,
                    };

                    var lblIcon = new Label
                    {
                        Text      = item.Icon,
                        Font      = new Font("Segoe UI", 16F),
                        ForeColor = _navyDark,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location  = new Point(0, 6),
                        Size      = new Size(58, 30),
                    };
                    var lblText = new Label
                    {
                        Text      = item.Text,
                        Font      = new Font("Segoe UI", 7.5F),
                        ForeColor = Color.FromArgb(55, 65, 90),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location  = new Point(0, 36),
                        Size      = new Size(58, 20),
                    };

                    EventHandler enter = (s, ev) =>
                    {
                        pnlBtn.BackColor  = _navHover;
                        lblIcon.BackColor = _navHover;
                        lblText.BackColor = _navHover;
                    };
                    EventHandler leave = (s, ev) =>
                    {
                        pnlBtn.BackColor  = Color.White;
                        lblIcon.BackColor = Color.White;
                        lblText.BackColor = Color.White;
                    };
                    EventHandler click = (s, ev) => OnToolbarClick(tag);

                    pnlBtn.MouseEnter += enter; lblIcon.MouseEnter += enter; lblText.MouseEnter += enter;
                    pnlBtn.MouseLeave += leave; lblIcon.MouseLeave += leave; lblText.MouseLeave += leave;
                    pnlBtn.Click      += click; lblIcon.Click      += click; lblText.Click      += click;

                    pnlBtn.Controls.Add(lblIcon);
                    pnlBtn.Controls.Add(lblText);
                    pnlToolbarContainer.Controls.Add(pnlBtn);
                    x += 60;
                }

                var lblGroup = new Label
                {
                    Text      = group.Header,
                    Font      = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(120, 130, 150),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location  = new Point(groupStartX, 62),
                    Size      = new Size(x - groupStartX, 10),
                };
                pnlToolbarContainer.Controls.Add(lblGroup);

                if (group != groups[groups.Length - 1])
                {
                    pnlToolbarContainer.Controls.Add(new Panel
                    {
                        BackColor = Color.FromArgb(220, 225, 235),
                        Location  = new Point(x + 2, 8),
                        Size      = new Size(1, 54),
                    });
                    x += 10;
                }
            }

            pnlToolbarContainer.Controls.Add(new Panel
            {
                BackColor = Color.FromArgb(220, 225, 235),
                Dock      = DockStyle.Bottom,
                Height    = 1,
            });
        }

        private void OnToolbarClick(string tag)
        {
            switch (tag)
            {
                case "new":
                    InitializeFormData();
                    break;
                case "history":
                    LoadHistoryRows();
                    dgvDelegations.Focus();
                    break;
                default:
                    MessageBox.Show(string.Format("ฟังก์ชัน '{0}' อยู่ระหว่างพัฒนา", tag),
                        "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void OnNavItemClick(string tag)
        {
            if (tag == "delegation")
                return;

            WorkflowNavigationHandler.HandleNavigation(
                tag,
                this,
                _workflowService,
                null,
                null,
                _loggingService,
                _userContext,
                _auditService);
        }

        private void dgvDelegations_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != colStatus.Index || e.Value == null)
                return;

            var status = Convert.ToString(e.Value);
            if (status == "กำลังใช้งาน")
                e.CellStyle.ForeColor = Color.FromArgb(23, 133, 49);
            else if (status == "กำหนดไว้")
                e.CellStyle.ForeColor = Color.FromArgb(197, 120, 16);
            else
                e.CellStyle.ForeColor = Color.FromArgb(107, 114, 128);

            e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        }
    }
}
