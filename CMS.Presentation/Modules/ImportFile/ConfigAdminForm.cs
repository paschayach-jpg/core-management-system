using System;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Controllers.DMS.Document;
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
            IFileService   fileService            = null,
            IUserAccountRepository userAccountRepository = null)
        {
            if (configManager == null) throw new ArgumentNullException("configManager");
            if (auditService  == null) throw new ArgumentNullException("auditService");
            if (user          == null) throw new ArgumentNullException("user");

            InitializeComponent();

            _presenter = new ConfigAdminPresenter(this, configManager, auditService, user, fileService, userAccountRepository);
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
