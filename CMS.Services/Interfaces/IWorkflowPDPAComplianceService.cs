using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// PDPA (Personal Data Protection Act) Thailand 2562 compliance service.
    /// Implements Requirements 14.1 (PDPA compliance checks) and 14.2 (PDPA Thailand 2562 compliance).
    /// Ensures personal data is handled according to PDPA requirements.
    /// </summary>
    public interface IWorkflowPDPAComplianceService
    {
        /// <summary>
        /// Check if PDPA consent has been obtained for processing personal data.
        /// </summary>
        bool HasPDPAConsent(string userId);

        /// <summary>
        /// Record PDPA consent for a user.
        /// </summary>
        void RecordPDPAConsent(string userId, string consentType, DateTime consentDate, UserContext userContext);

        /// <summary>
        /// Revoke PDPA consent for a user.
        /// </summary>
        void RevokePDPAConsent(string userId, string consentType, UserContext userContext);

        /// <summary>
        /// Get PDPA consent history for a user.
        /// </summary>
        List<PDPAConsentRecord> GetPDPAConsentHistory(string userId);

        /// <summary>
        /// Check if workflow data contains personal data that requires PDPA compliance.
        /// </summary>
        bool ContainsPersonalData(string workflowData);

        /// <summary>
        /// Get list of personal data fields in workflow data.
        /// </summary>
        List<string> GetPersonalDataFields(string workflowData);

        /// <summary>
        /// Request data deletion for a user (right to be forgotten).
        /// </summary>
        void RequestDataDeletion(string userId, UserContext userContext);

        /// <summary>
        /// Get data deletion request status.
        /// </summary>
        DataDeletionStatus GetDataDeletionStatus(string userId);

        /// <summary>
        /// Export personal data for a user (data portability).
        /// </summary>
        string ExportPersonalData(string userId, UserContext userContext);

        /// <summary>
        /// Check if workflow operation complies with PDPA requirements.
        /// </summary>
        bool IsCompliantWithPDPA(WorkflowInstance instance, UserContext userContext);

        /// <summary>
        /// Log PDPA-related audit events.
        /// </summary>
        void LogPDPAEvent(string eventType, string userId, string details, UserContext userContext);

        /// <summary>
        /// Get PDPA compliance report for a date range.
        /// </summary>
        PDPAComplianceReport GetComplianceReport(DateTime fromDate, DateTime toDate);
    }

    /// <summary>
    /// PDPA consent record.
    /// </summary>
    public class PDPAConsentRecord
    {
        public int ConsentId { get; set; }
        public string UserId { get; set; }
        public string ConsentType { get; set; }
        public DateTime ConsentDate { get; set; }
        public DateTime? RevokedDate { get; set; }
        public bool IsActive { get; set; }
        public string RecordedBy { get; set; }
    }

    /// <summary>
    /// Data deletion status.
    /// </summary>
    public class DataDeletionStatus
    {
        public string UserId { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } // Pending, InProgress, Completed, Failed
        public int RecordsDeleted { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// PDPA compliance report.
    /// </summary>
    public class PDPAComplianceReport
    {
        public DateTime ReportDate { get; set; }
        public int TotalWorkflowInstances { get; set; }
        public int InstancesWithPersonalData { get; set; }
        public int ConsentRecordsCreated { get; set; }
        public int ConsentRecordsRevoked { get; set; }
        public int DataDeletionRequests { get; set; }
        public int DataExportRequests { get; set; }
        public int ComplianceViolations { get; set; }
        public List<string> ViolationDetails { get; set; }
    }
}
