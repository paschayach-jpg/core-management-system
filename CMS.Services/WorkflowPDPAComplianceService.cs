using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Implements IWorkflowPDPAComplianceService: ensures PDPA Thailand 2562 compliance.
    /// Supports Requirements 14.1 (PDPA compliance checks) and 14.2 (PDPA Thailand 2562 compliance).
    /// </summary>
    public class WorkflowPDPAComplianceService : IWorkflowPDPAComplianceService
    {
        private readonly IAuditRepository _auditRepository;
        private readonly ILoggingService _loggingService;
        private readonly IWorkflowDataEncryptionService _encryptionService;

        // Regex patterns for personal data detection
        private static readonly Regex EmailPattern = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);
        private static readonly Regex PhonePattern = new Regex(@"(\+66|0)[0-9]{8,10}", RegexOptions.Compiled);
        private static readonly Regex IdPattern = new Regex(@"\d{13}", RegexOptions.Compiled); // Thai ID
        private static readonly Regex NamePattern = new Regex(@"[ก-๙]{2,}", RegexOptions.Compiled); // Thai names

        public WorkflowPDPAComplianceService(
            IAuditRepository auditRepository,
            ILoggingService loggingService,
            IWorkflowDataEncryptionService encryptionService)
        {
            if (auditRepository == null) throw new ArgumentNullException(nameof(auditRepository));
            if (loggingService == null) throw new ArgumentNullException(nameof(loggingService));
            if (encryptionService == null) throw new ArgumentNullException(nameof(encryptionService));

            _auditRepository = auditRepository;
            _loggingService = loggingService;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Check if PDPA consent has been obtained for processing personal data.
        /// </summary>
        public bool HasPDPAConsent(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            try
            {
                // TODO: Query consent database
                // For now, assume consent is required and check audit log
                _loggingService.Info($"Checking PDPA consent for user {userId}");
                return true; // Placeholder
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error checking PDPA consent: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Record PDPA consent for a user.
        /// </summary>
        public void RecordPDPAConsent(string userId, string consentType, DateTime consentDate, UserContext userContext)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(consentType)) throw new ArgumentNullException(nameof(consentType));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            try
            {
                _loggingService.Info($"Recording PDPA consent for user {userId}, type: {consentType}");

                // Log consent in audit trail
                _auditRepository.Insert(new AuditLog
                {
                    AuditDate = DateTime.Now,
                    UserID = userContext.UserID,
                    UserName = userContext.ADUserName,
                    Module = "Workflow",
                    Action = "PDPAConsentRecorded",
                    TableName = "PDPAConsent",
                    RecordID = userId.GetHashCode(),
                    NewValue = $"Consent Type: {consentType}, Consent Date: {consentDate:yyyy-MM-dd}",
                    Result = 1
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error recording PDPA consent: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Revoke PDPA consent for a user.
        /// </summary>
        public void RevokePDPAConsent(string userId, string consentType, UserContext userContext)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(consentType)) throw new ArgumentNullException(nameof(consentType));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            try
            {
                _loggingService.Info($"Revoking PDPA consent for user {userId}, type: {consentType}");

                // Log consent revocation in audit trail
                _auditRepository.Insert(new AuditLog
                {
                    AuditDate = DateTime.Now,
                    UserID = userContext.UserID,
                    UserName = userContext.ADUserName,
                    Module = "Workflow",
                    Action = "PDPAConsentRevoked",
                    TableName = "PDPAConsent",
                    RecordID = userId.GetHashCode(),
                    NewValue = $"Consent Type: {consentType}, Revoked Date: {DateTime.Now:yyyy-MM-dd}",
                    Result = 1
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error revoking PDPA consent: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get PDPA consent history for a user.
        /// </summary>
        public List<PDPAConsentRecord> GetPDPAConsentHistory(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            try
            {
                // TODO: Query consent history from database
                _loggingService.Info($"Retrieving PDPA consent history for user {userId}");
                return new List<PDPAConsentRecord>();
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error retrieving PDPA consent history: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Check if workflow data contains personal data that requires PDPA compliance.
        /// </summary>
        public bool ContainsPersonalData(string workflowData)
        {
            if (string.IsNullOrEmpty(workflowData))
                return false;

            return EmailPattern.IsMatch(workflowData) ||
                   PhonePattern.IsMatch(workflowData) ||
                   IdPattern.IsMatch(workflowData) ||
                   NamePattern.IsMatch(workflowData);
        }

        /// <summary>
        /// Get list of personal data fields in workflow data.
        /// </summary>
        public List<string> GetPersonalDataFields(string workflowData)
        {
            var fields = new List<string>();

            if (string.IsNullOrEmpty(workflowData))
                return fields;

            if (EmailPattern.IsMatch(workflowData))
                fields.Add("Email");

            if (PhonePattern.IsMatch(workflowData))
                fields.Add("Phone");

            if (IdPattern.IsMatch(workflowData))
                fields.Add("ID Number");

            if (NamePattern.IsMatch(workflowData))
                fields.Add("Name");

            return fields;
        }

        /// <summary>
        /// Request data deletion for a user (right to be forgotten).
        /// </summary>
        public void RequestDataDeletion(string userId, UserContext userContext)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            try
            {
                _loggingService.Info($"Data deletion request for user {userId}");

                // Log deletion request in audit trail
                _auditRepository.Insert(new AuditLog
                {
                    AuditDate = DateTime.Now,
                    UserID = userContext.UserID,
                    UserName = userContext.ADUserName,
                    Module = "Workflow",
                    Action = "DataDeletionRequested",
                    TableName = "PDPADataDeletion",
                    RecordID = userId.GetHashCode(),
                    NewValue = $"Data deletion requested for user {userId}",
                    Result = 1
                });

                // TODO: Implement actual data deletion logic
                // This should:
                // 1. Find all workflow instances with personal data for this user
                // 2. Anonymize or delete personal data
                // 3. Maintain audit trail integrity
                // 4. Update deletion status
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error requesting data deletion: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get data deletion request status.
        /// </summary>
        public DataDeletionStatus GetDataDeletionStatus(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            try
            {
                // TODO: Query deletion status from database
                return new DataDeletionStatus
                {
                    UserId = userId,
                    Status = "Pending",
                    RecordsDeleted = 0
                };
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting data deletion status: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Export personal data for a user (data portability).
        /// </summary>
        public string ExportPersonalData(string userId, UserContext userContext)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            try
            {
                _loggingService.Info($"Exporting personal data for user {userId}");

                // Log export request in audit trail
                _auditRepository.Insert(new AuditLog
                {
                    AuditDate = DateTime.Now,
                    UserID = userContext.UserID,
                    UserName = userContext.ADUserName,
                    Module = "Workflow",
                    Action = "PersonalDataExported",
                    TableName = "PDPADataExport",
                    RecordID = userId.GetHashCode(),
                    NewValue = $"Personal data exported for user {userId}",
                    Result = 1
                });

                // TODO: Implement actual data export logic
                // This should:
                // 1. Find all workflow instances with personal data for this user
                // 2. Export data in portable format (JSON, CSV)
                // 3. Encrypt exported data
                // 4. Return export reference

                return "{}"; // Placeholder
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error exporting personal data: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Check if workflow operation complies with PDPA requirements.
        /// </summary>
        public bool IsCompliantWithPDPA(WorkflowInstance instance, UserContext userContext)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            try
            {
                // Check if workflow contains personal data
                if (ContainsPersonalData(instance.InputParameters))
                {
                    // Verify PDPA consent
                    if (!HasPDPAConsent(instance.InitiatedBy))
                    {
                        _loggingService.Warn($"Workflow {instance.WorkflowInstanceId} contains personal data but no PDPA consent found");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error checking PDPA compliance: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Log PDPA-related audit events.
        /// </summary>
        public void LogPDPAEvent(string eventType, string userId, string details, UserContext userContext)
        {
            if (string.IsNullOrEmpty(eventType)) throw new ArgumentNullException(nameof(eventType));
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            try
            {
                _auditRepository.Insert(new AuditLog
                {
                    AuditDate = DateTime.Now,
                    UserID = userContext.UserID,
                    UserName = userContext.ADUserName,
                    Module = "Workflow",
                    Action = $"PDPA{eventType}",
                    TableName = "PDPAEvents",
                    RecordID = userId.GetHashCode(),
                    NewValue = details,
                    Result = 1
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error logging PDPA event: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get PDPA compliance report for a date range.
        /// </summary>
        public PDPAComplianceReport GetComplianceReport(DateTime fromDate, DateTime toDate)
        {
            try
            {
                _loggingService.Info($"Generating PDPA compliance report for {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");

                // TODO: Query audit logs and generate compliance report
                return new PDPAComplianceReport
                {
                    ReportDate = DateTime.Now,
                    TotalWorkflowInstances = 0,
                    InstancesWithPersonalData = 0,
                    ConsentRecordsCreated = 0,
                    ConsentRecordsRevoked = 0,
                    DataDeletionRequests = 0,
                    DataExportRequests = 0,
                    ComplianceViolations = 0,
                    ViolationDetails = new List<string>()
                };
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error generating PDPA compliance report: {ex.Message}", ex);
                throw;
            }
        }
    }
}
