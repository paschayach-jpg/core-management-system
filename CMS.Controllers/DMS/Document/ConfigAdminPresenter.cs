using System;
using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    /// <summary>
    /// Presenter for the System Configuration admin screen.
    /// Loads current config values on construction and persists changes on SaveRequested.
    /// Satisfies Requirements 14.1–14.5.
    /// </summary>
    public class ConfigAdminPresenter
    {
        private readonly IConfigAdminView       _view;
        private readonly IConfigManager         _configManager;
        private readonly IAuditService          _auditService;
        private readonly UserContext            _user;

        // Config keys
        private const string KeyNetworkDrivePath        = "NetworkDrivePath";
        private const string KeyMaxFileSizeBytes        = "MaxFileSizeBytes";
        private const string KeyMaxFilesPerUpload       = "MaxFilesPerUpload";
        private const string KeyAllowedExtensions       = "AllowedExtensions";
        private const string KeyDefaultQuotaBytes       = "DefaultQuotaBytes";
        private const string KeyApprovalTimeoutMinutes  = "ApprovalTimeoutMinutes";
        private const string KeyAuditRetentionDays      = "AuditRetentionDays";
        private const string KeyVirusScanTimeoutSeconds = "VirusScanTimeoutSeconds";

        // Defaults
        private const string DefaultNetworkDrivePath        = @"C:\FileImport";
        private const string DefaultMaxFileSizeBytes        = "104857600";
        private const string DefaultMaxFilesPerUpload       = "10";
        private const string DefaultAllowedExtensions       = "pdf,doc,docx,xls,xlsx,ppt,pptx,jpg,png,gif,zip";
        private const string DefaultDefaultQuotaBytes       = "0";
        private const string DefaultApprovalTimeoutMinutes  = "1440";
        private const string DefaultAuditRetentionDays      = "1825";
        private const string DefaultVirusScanTimeoutSeconds = "30";

        public ConfigAdminPresenter(
            IConfigAdminView       view,
            IConfigManager         configManager,
            IAuditService          auditService,
            UserContext            user)
        {
            if (view          == null) throw new ArgumentNullException("view");
            if (configManager == null) throw new ArgumentNullException("configManager");
            if (auditService  == null) throw new ArgumentNullException("auditService");
            if (user          == null) throw new ArgumentNullException("user");

            _view                  = view;
            _configManager         = configManager;
            _auditService          = auditService;
            _user                  = user;

            LoadCurrentValues();

            _view.SaveRequested              += OnSaveRequested;
            _view.ProcessRetentionRequested  += OnProcessRetentionRequested;
            _view.GeneratePdpaReportRequested += OnGeneratePdpaReportRequested;
        }

        // -----------------------------------------------------------------------
        // Load
        // -----------------------------------------------------------------------

        private void LoadCurrentValues()
        {
            _view.NetworkDrivePath        = GetConfigString(KeyNetworkDrivePath,        DefaultNetworkDrivePath);
            _view.MaxFileSizeBytes        = GetConfigString(KeyMaxFileSizeBytes,        DefaultMaxFileSizeBytes);
            _view.MaxFilesPerUpload       = GetConfigString(KeyMaxFilesPerUpload,       DefaultMaxFilesPerUpload);
            _view.AllowedExtensions       = GetConfigString(KeyAllowedExtensions,       DefaultAllowedExtensions);
            _view.DefaultQuotaBytes       = GetConfigString(KeyDefaultQuotaBytes,       DefaultDefaultQuotaBytes);
            _view.ApprovalTimeoutMinutes  = GetConfigString(KeyApprovalTimeoutMinutes,  DefaultApprovalTimeoutMinutes);
            _view.AuditRetentionDays      = GetConfigString(KeyAuditRetentionDays,      DefaultAuditRetentionDays);
            _view.VirusScanTimeoutSeconds = GetConfigString(KeyVirusScanTimeoutSeconds, DefaultVirusScanTimeoutSeconds);
        }

        private string GetConfigString(string key, string defaultValue)
        {
            try { return _configManager.Get<string>(key) ?? defaultValue; }
            catch { return defaultValue; }
        }

        // -----------------------------------------------------------------------
        // Save
        // -----------------------------------------------------------------------

        private void OnSaveRequested(object sender, EventArgs e)
        {
            try
            {
                string networkDrivePath        = (_view.NetworkDrivePath        ?? string.Empty).Trim();
                string maxFileSizeBytes        = (_view.MaxFileSizeBytes        ?? string.Empty).Trim();
                string maxFilesPerUpload       = (_view.MaxFilesPerUpload       ?? string.Empty).Trim();
                string allowedExtensions       = (_view.AllowedExtensions       ?? string.Empty).Trim();
                string defaultQuotaBytes       = (_view.DefaultQuotaBytes       ?? string.Empty).Trim();
                string approvalTimeoutMinutes  = (_view.ApprovalTimeoutMinutes  ?? string.Empty).Trim();
                string auditRetentionDays      = (_view.AuditRetentionDays      ?? string.Empty).Trim();
                string virusScanTimeoutSeconds = (_view.VirusScanTimeoutSeconds ?? string.Empty).Trim();

                if (string.IsNullOrEmpty(networkDrivePath))        { _view.ShowError("Storage Path (NetworkDrivePath) is required."); return; }

                if (!System.IO.Directory.Exists(networkDrivePath))
                {
                    var result = System.Windows.Forms.MessageBox.Show(
                        string.Format("Path '{0}' does not exist.\nCreate it now?", networkDrivePath),
                        "Path Not Found",
                        System.Windows.Forms.MessageBoxButtons.YesNo,
                        System.Windows.Forms.MessageBoxIcon.Question);

                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        try { System.IO.Directory.CreateDirectory(networkDrivePath); }
                        catch (Exception ex) { _view.ShowError("Cannot create directory: " + ex.Message); return; }
                    }
                    else return;
                }

                if (string.IsNullOrEmpty(maxFileSizeBytes))        { _view.ShowError("Max File Size (bytes) is required.");           return; }
                if (string.IsNullOrEmpty(maxFilesPerUpload))       { _view.ShowError("Max Files Per Upload is required.");            return; }
                if (string.IsNullOrEmpty(allowedExtensions))       { _view.ShowError("Allowed Extensions is required.");              return; }
                if (string.IsNullOrEmpty(defaultQuotaBytes))       { _view.ShowError("Default Quota (bytes) is required.");           return; }
                if (string.IsNullOrEmpty(approvalTimeoutMinutes))  { _view.ShowError("Approval Timeout (minutes) is required.");      return; }
                if (string.IsNullOrEmpty(auditRetentionDays))      { _view.ShowError("Audit Retention (days) is required.");          return; }
                if (string.IsNullOrEmpty(virusScanTimeoutSeconds)) { _view.ShowError("Virus Scan Timeout (seconds) is required.");    return; }

                long parsedLong;
                int  parsedInt;

                if (!long.TryParse(maxFileSizeBytes,        out parsedLong) || parsedLong <= 0) { _view.ShowError("Max File Size (bytes) must be a positive integer.");          return; }
                if (!int.TryParse(maxFilesPerUpload,        out parsedInt)  || parsedInt  <= 0) { _view.ShowError("Max Files Per Upload must be a positive integer.");           return; }
                if (!long.TryParse(defaultQuotaBytes,       out parsedLong) || parsedLong < 0)  { _view.ShowError("Default Quota (bytes) must be zero or a positive integer."); return; }
                if (!int.TryParse(approvalTimeoutMinutes,   out parsedInt)  || parsedInt  <= 0) { _view.ShowError("Approval Timeout (minutes) must be a positive integer.");     return; }
                if (!int.TryParse(auditRetentionDays,       out parsedInt)  || parsedInt  <= 0) { _view.ShowError("Audit Retention (days) must be a positive integer.");         return; }
                if (!int.TryParse(virusScanTimeoutSeconds,  out parsedInt)  || parsedInt  <= 0) { _view.ShowError("Virus Scan Timeout (seconds) must be a positive integer.");   return; }

                var changedKeys = new List<string>();

                PersistIfChanged(KeyNetworkDrivePath,        networkDrivePath,        changedKeys);
                PersistIfChanged(KeyMaxFileSizeBytes,        maxFileSizeBytes,        changedKeys);
                PersistIfChanged(KeyMaxFilesPerUpload,       maxFilesPerUpload,       changedKeys);
                PersistIfChanged(KeyAllowedExtensions,       allowedExtensions,       changedKeys);
                PersistIfChanged(KeyDefaultQuotaBytes,       defaultQuotaBytes,       changedKeys);
                PersistIfChanged(KeyApprovalTimeoutMinutes,  approvalTimeoutMinutes,  changedKeys);
                PersistIfChanged(KeyAuditRetentionDays,      auditRetentionDays,      changedKeys);
                PersistIfChanged(KeyVirusScanTimeoutSeconds, virusScanTimeoutSeconds, changedKeys);

                string details = changedKeys.Count > 0
                    ? string.Format("Changed keys: {0}", string.Join(", ", changedKeys.ToArray()))
                    : "No values changed.";

                _auditService.Log(new AuditEntry
                {
                    Module      = "Config",
                    Action      = "UPDATE",
                    UserID      = _user.UserID,
                    TableName   = "SystemConfig",
                    NewValue    = details,
                    MachineName = _user.MachineName,
                    IPAddress   = _user.IPAddress,
                    SessionID   = _user.SessionID,
                    Result      = 1,
                    AuditDate   = DateTime.Now
                });

                _view.ShowSuccess("Configuration saved successfully.");
            }
            catch (UnauthorizedAccessException ex) { _view.ShowError("Access denied: " + ex.Message); }
            catch (Exception ex)                   { _view.ShowError("Failed to save configuration: " + ex.Message); }
        }

        private void PersistIfChanged(string key, string newValue, List<string> changedKeys)
        {
            string current = GetConfigString(key, null);
            if (current != newValue)
            {
                _configManager.Set(key, newValue, _user);
                changedKeys.Add(key);
            }
        }

        // -----------------------------------------------------------------------
        // PDPA Retention
        // -----------------------------------------------------------------------

        private void OnProcessRetentionRequested(object sender, EventArgs e)
        {
            _view.ShowRetentionResult("Retention processing is not available in this version.");
        }

        private void OnGeneratePdpaReportRequested(object sender, EventArgs e)
        {
            _view.ShowError("PDPA report generation is not available in this version.");
        }
    }
}
