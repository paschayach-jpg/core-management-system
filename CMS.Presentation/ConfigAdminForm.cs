using System;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Presenters;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Admin-only form for managing system configuration parameters.
    /// Implements IConfigAdminView and delegates all logic to ConfigAdminPresenter.
    /// </summary>
    public partial class ConfigAdminForm : Form, IConfigAdminView
    {
        private ConfigAdminPresenter _presenter;

        // ── IConfigAdminView — properties ────────────────────────────────────

        public string NetworkDrivePath
        {
            get { return txtNetworkDrivePath.Text; }
            set { txtNetworkDrivePath.Text = value; }
        }

        public string MaxFileSizeBytes
        {
            get { return txtMaxFileSizeBytes.Text; }
            set { txtMaxFileSizeBytes.Text = value; }
        }

        public string MaxFilesPerUpload
        {
            get { return txtMaxFilesPerUpload.Text; }
            set { txtMaxFilesPerUpload.Text = value; }
        }

        public string AllowedExtensions
        {
            get { return txtAllowedExtensions.Text; }
            set { txtAllowedExtensions.Text = value; }
        }

        public string DefaultQuotaBytes
        {
            get { return txtDefaultQuotaBytes.Text; }
            set { txtDefaultQuotaBytes.Text = value; }
        }

        public string ApprovalTimeoutMinutes
        {
            get { return txtApprovalTimeoutMinutes.Text; }
            set { txtApprovalTimeoutMinutes.Text = value; }
        }

        public string AuditRetentionDays
        {
            get { return txtAuditRetentionDays.Text; }
            set { txtAuditRetentionDays.Text = value; }
        }

        public string VirusScanTimeoutSeconds
        {
            get { return txtVirusScanTimeoutSeconds.Text; }
            set { txtVirusScanTimeoutSeconds.Text = value; }
        }

        public string PdpaReportUserId
        {
            get { return txtPdpaReportUserId.Text; }
            set { txtPdpaReportUserId.Text = value; }
        }

        // ── IConfigAdminView — events ─────────────────────────────────────────

        public event EventHandler SaveRequested;
        public event EventHandler ProcessRetentionRequested;
        public event EventHandler GeneratePdpaReportRequested;

        // ── IConfigAdminView — methods ────────────────────────────────────────

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Configuration", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowRetentionResult(string message)
        {
            MessageBox.Show(message, "PDPA Retention", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowPdpaReport(object report)
        {
            var pdpaReport = (PdpaReport)report;
            var form = new PdpaReportForm(pdpaReport);
            form.ShowDialog(this);
        }

        // ── Constructor ───────────────────────────────────────────────────────

        public ConfigAdminForm(
            IConfigManager configManager,
            IAuditService  auditService,
            UserContext    user,
            IFileService   fileService = null,
            IUserAccountRepository userAccountRepository = null,
            CMS.Services.Interfaces.IEmployeeApiService employeeApiService = null)
        {
            if (configManager == null) throw new ArgumentNullException("configManager");
            if (auditService  == null) throw new ArgumentNullException("auditService");
            if (user          == null) throw new ArgumentNullException("user");

            _employeeApiService    = employeeApiService;
            _userAccountRepository = userAccountRepository;

            InitializeComponent();

            _presenter = new ConfigAdminPresenter(this, configManager, auditService, user, fileService, userAccountRepository);

            // เพิ่มปุ่ม Sync Employee Info
            AddSyncEmployeeButton();
        }

        private readonly CMS.Services.Interfaces.IEmployeeApiService _employeeApiService;
        private readonly IUserAccountRepository _userAccountRepository;

        private void AddSyncEmployeeButton()
        {
            var btnSync = new Button
            {
                Text      = "🔄 Sync ข้อมูลพนักงานจาก API",
                Width     = 220,
                Height    = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(31, 56, 100),
                ForeColor = System.Drawing.Color.White,
                Font      = new System.Drawing.Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand,
                Dock      = DockStyle.Bottom,
                Margin    = new Padding(10)
            };
            btnSync.FlatAppearance.BorderSize = 0;
            btnSync.Click += BtnSyncEmployee_Click;
            this.Controls.Add(btnSync);
            btnSync.BringToFront();
        }

        private void BtnSyncEmployee_Click(object sender, EventArgs e)
        {
            if (_employeeApiService == null || _userAccountRepository == null)
            {
                MessageBox.Show("Employee API service ไม่พร้อมใช้งาน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                "ต้องการ Sync ข้อมูลพนักงานจาก API สำหรับ users ทั้งหมดหรือไม่?\n\n" +
                "ระบบจะ match ADUserName กับ EmployeeCode จาก API\n" +
                "และอัพเดท Department/Division/Position",
                "Sync Employee Info",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                int updated = 0, skipped = 0, failed = 0;

                var allUsers = _userAccountRepository.GetAll().ToList();
                var allEmployees = _employeeApiService.GetAllEmployees().ToList();

                // สร้าง lookup: employeeCode → EmployeeSummary
                var empLookup = new System.Collections.Generic.Dictionary<string, CMS.Domain.Models.EmployeeSummary>(
                    System.StringComparer.OrdinalIgnoreCase);
                foreach (var emp in allEmployees)
                    if (!string.IsNullOrEmpty(emp.EmployeeCode))
                        empLookup[emp.EmployeeCode] = emp;

                foreach (var user in allUsers)
                {
                    try
                    {
                        string adUser = user.ADUserName ?? "";
                        CMS.Domain.Models.EmployeeSummary emp = null;

                        // ลอง match ตรงๆ
                        if (!empLookup.TryGetValue(adUser, out emp))
                        {
                            // ลอง strip domain prefix
                            if (adUser.Contains("\\"))
                            {
                                string username = adUser.Substring(adUser.LastIndexOf('\\') + 1);
                                empLookup.TryGetValue(username, out emp);
                            }
                        }

                        if (emp == null) { skipped++; continue; }

                        _userAccountRepository.UpdateEmployeeInfo(
                            user.UserID,
                            emp.EmployeeCode,
                            emp.DepartmentId,   emp.DepartmentCode, emp.DepartmentNameTh,
                            emp.DivisionId,     emp.DivisionCode,   emp.DivisionNameTh,
                            null); // PositionName ไม่มีใน EmployeeSummary

                        updated++;
                    }
                    catch { failed++; }
                }

                Cursor = Cursors.Default;
                MessageBox.Show(
                    string.Format("Sync เสร็จสิ้น\n\n✓ อัพเดท: {0} คน\n— ไม่พบใน API: {1} คน\n✗ ผิดพลาด: {2} คน",
                        updated, skipped, failed),
                    "Sync Employee Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void OnSaveClick(object sender, EventArgs e)
        {
            if (SaveRequested != null)
                SaveRequested(this, EventArgs.Empty);
        }

        private void OnBrowsePathClick(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description         = "เลือก Storage Path สำหรับเก็บไฟล์";
                dlg.ShowNewFolderButton = true;
                if (!string.IsNullOrEmpty(txtNetworkDrivePath.Text) &&
                    System.IO.Directory.Exists(txtNetworkDrivePath.Text))
                    dlg.SelectedPath = txtNetworkDrivePath.Text;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    txtNetworkDrivePath.Text = dlg.SelectedPath;
            }
        }

        private void OnProcessRetentionClick(object sender, EventArgs e)
        {
            if (ProcessRetentionRequested != null)
                ProcessRetentionRequested(this, EventArgs.Empty);
        }

        private void OnGenerateReportClick(object sender, EventArgs e)
        {
            if (GeneratePdpaReportRequested != null)
                GeneratePdpaReportRequested(this, EventArgs.Empty);
        }
    }
}
