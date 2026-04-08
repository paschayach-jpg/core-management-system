using System;

namespace CMS.Controllers.DMS.Document
{
    /// <summary>
    /// View contract for the ConfigAdminPresenter.
    /// Implemented by ConfigAdminForm.
    /// Satisfies Requirements 14.1–14.5.
    /// </summary>
    public interface IConfigAdminView
    {
        string NetworkDrivePath       { get; set; }
        string MaxFileSizeBytes       { get; set; }
        string MaxFilesPerUpload      { get; set; }
        string AllowedExtensions      { get; set; }
        string DefaultQuotaBytes      { get; set; }
        string ApprovalTimeoutMinutes { get; set; }
        string AuditRetentionDays     { get; set; }
        string VirusScanTimeoutSeconds { get; set; }

        event EventHandler SaveRequested;

        void ShowError(string message);
        void ShowSuccess(string message);

        event EventHandler ProcessRetentionRequested;
        void ShowRetentionResult(string message);

        string PdpaReportUserId { get; set; }
        event EventHandler GeneratePdpaReportRequested;
        void ShowPdpaReport(object report);  // receives PdpaReport, cast in form
    }
}
