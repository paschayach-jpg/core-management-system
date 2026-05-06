using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Implements IWorkflowComplianceService: ensures compliance with Thai regulations and data classification.
    /// Supports Requirements 14.2 (OIC Circular 2566, CRAF data classification).
    /// </summary>
    public class WorkflowComplianceService : IWorkflowComplianceService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;
        private readonly IWorkflowDataEncryptionService _encryptionService;

        public WorkflowComplianceService(
            IWorkflowRepository workflowRepository,
            IAuditService auditService,
            ILoggingService loggingService,
            IWorkflowDataEncryptionService encryptionService)
        {
            if (workflowRepository == null) throw new ArgumentNullException(nameof(workflowRepository));
            if (auditService == null) throw new ArgumentNullException(nameof(auditService));
            if (loggingService == null) throw new ArgumentNullException(nameof(loggingService));
            if (encryptionService == null) throw new ArgumentNullException(nameof(encryptionService));

            _workflowRepository = workflowRepository;
            _auditService = auditService;
            _loggingService = loggingService;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Check if workflow complies with OIC Circular 2566 (Thai financial regulations).
        /// </summary>
        public bool IsCompliantWithOICCircular2566(WorkflowInstance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            try
            {
                // OIC Circular 2566 requirements:
                // 1. Proper audit trail for all transactions
                // 2. Data integrity and non-repudiation
                // 3. Access control and authentication
                // 4. Encryption of sensitive data
                // 5. Regular compliance checks

                // Check if audit trail exists
                if (string.IsNullOrEmpty(instance.InputParameters))
                {
                    _loggingService.Warn($"Workflow {instance.WorkflowInstanceId} missing input parameters");
                    return false;
                }

                // Check if sensitive data is encrypted
                if (_encryptionService.ContainsSensitiveData(instance.InputParameters))
                {
                    // Sensitive data should be encrypted
                    // TODO: Verify encryption status
                }

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error checking OIC Circular 2566 compliance: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get OIC Circular 2566 compliance status for a workflow.
        /// </summary>
        public OICComplianceStatus GetOICComplianceStatus(int workflowInstanceId)
        {
            try
            {
                var instance = _workflowRepository.GetWorkflowInstance(workflowInstanceId);
                if (instance == null)
                    throw new Exception($"Workflow instance {workflowInstanceId} not found");

                var isCompliant = IsCompliantWithOICCircular2566(instance);

                return new OICComplianceStatus
                {
                    WorkflowInstanceId = workflowInstanceId,
                    IsCompliant = isCompliant,
                    ViolatedRequirements = isCompliant ? new List<string>() : new List<string> { "Audit trail incomplete" },
                    LastCheckedDate = DateTime.Now,
                    CheckedBy = "SYSTEM"
                };
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting OIC compliance status: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Classify workflow data according to CRAF (Confidentiality Rating and Handling).
        /// </summary>
        public CRAFClassification ClassifyWorkflowData(string workflowData)
        {
            if (string.IsNullOrEmpty(workflowData))
                throw new ArgumentNullException(nameof(workflowData));

            try
            {
                var classification = new CRAFClassification
                {
                    AllowedRoles = new List<string>(),
                    ClassificationDate = DateTime.Now,
                    ClassifiedBy = "SYSTEM"
                };

                // Analyze data to determine classification
                if (_encryptionService.ContainsSensitiveData(workflowData))
                {
                    // Contains personal data - at least Sensitive Confidential
                    classification.ConfidentialityLevel = "SC"; // Sensitive Confidential
                    classification.RequiresEncryption = true;
                    classification.RequiresAuditLog = true;
                    classification.AllowedRoles.Add("Admin");
                    classification.AllowedRoles.Add("Manager");
                }
                else
                {
                    // No sensitive data - Normal Confidential
                    classification.ConfidentialityLevel = "NC"; // Normal Confidential
                    classification.RequiresEncryption = false;
                    classification.RequiresAuditLog = true;
                    classification.AllowedRoles.Add("User");
                }

                return classification;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error classifying workflow data: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get CRAF classification for a workflow instance.
        /// </summary>
        public CRAFClassification GetCRAFClassification(int workflowInstanceId)
        {
            try
            {
                var instance = _workflowRepository.GetWorkflowInstance(workflowInstanceId);
                if (instance == null)
                    throw new Exception($"Workflow instance {workflowInstanceId} not found");

                // TODO: Query classification from database
                // For now, classify based on data
                return ClassifyWorkflowData(instance.InputParameters);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting CRAF classification: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Set CRAF classification for a workflow instance.
        /// </summary>
        public void SetCRAFClassification(int workflowInstanceId, CRAFClassification classification, UserContext userContext)
        {
            if (classification == null) throw new ArgumentNullException(nameof(classification));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            try
            {
                _loggingService.Info($"Setting CRAF classification for workflow {workflowInstanceId}: {classification.ConfidentialityLevel}");

                // TODO: Save classification to database

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Action = "CRAFClassificationSet",
                    TableName = "WorkflowCRAFClassification",
                    RecordID = workflowInstanceId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = $"Classification: {classification.ConfidentialityLevel}, Encryption: {classification.RequiresEncryption}"
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error setting CRAF classification: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Check if user has access to data with specific CRAF classification.
        /// </summary>
        public bool CanAccessCRAFClassifiedData(UserContext userContext, CRAFClassification classification)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            if (classification == null) throw new ArgumentNullException(nameof(classification));

            try
            {
                // Admin can access all classifications
                if (userContext.AppRole >= 4)
                    return true;

                // Check if user's role is in allowed roles
                var userRoles = GetUserRoles(userContext);
                return classification.AllowedRoles.Any(role => userRoles.Contains(role));
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error checking CRAF access: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get list of workflows that violate OIC Circular 2566.
        /// </summary>
        public List<WorkflowInstance> GetOICNonCompliantWorkflows()
        {
            try
            {
                var allInstances = _workflowRepository.GetWorkflowInstances(
                    new Data.Interfaces.WorkflowInstanceFilter { PageSize = 10000 })
                    .Items
                    .ToList();

                var nonCompliant = new List<WorkflowInstance>();

                foreach (var instance in allInstances)
                {
                    if (!IsCompliantWithOICCircular2566(instance))
                    {
                        nonCompliant.Add(instance);
                    }
                }

                return nonCompliant;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting OIC non-compliant workflows: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get list of workflows with sensitive CRAF classification.
        /// </summary>
        public List<WorkflowInstance> GetSensitivelyClassifiedWorkflows()
        {
            try
            {
                var allInstances = _workflowRepository.GetWorkflowInstances(
                    new Data.Interfaces.WorkflowInstanceFilter { PageSize = 10000 })
                    .Items
                    .ToList();

                var sensitiveWorkflows = new List<WorkflowInstance>();

                foreach (var instance in allInstances)
                {
                    var classification = GetCRAFClassification(instance.WorkflowInstanceId);
                    if (classification.ConfidentialityLevel == "HC" || classification.ConfidentialityLevel == "SC")
                    {
                        sensitiveWorkflows.Add(instance);
                    }
                }

                return sensitiveWorkflows;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting sensitively classified workflows: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Generate compliance audit report.
        /// </summary>
        public ComplianceAuditReport GenerateComplianceReport(DateTime fromDate, DateTime toDate)
        {
            try
            {
                _loggingService.Info($"Generating compliance report for {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");

                var allInstances = _workflowRepository.GetWorkflowInstances(
                    new Data.Interfaces.WorkflowInstanceFilter { PageSize = 10000 })
                    .Items
                    .ToList();

                var report = new ComplianceAuditReport
                {
                    ReportDate = DateTime.Now,
                    TotalWorkflows = allInstances.Count,
                    OICCompliantWorkflows = allInstances.Count(i => IsCompliantWithOICCircular2566(i)),
                    OICNonCompliantWorkflows = 0,
                    HighlyConfidentialWorkflows = 0,
                    SensitiveConfidentialWorkflows = 0,
                    NormalConfidentialWorkflows = 0,
                    PublicWorkflows = 0,
                    UnclassifiedWorkflows = 0,
                    Violations = new List<ComplianceViolation>()
                };

                report.OICNonCompliantWorkflows = report.TotalWorkflows - report.OICCompliantWorkflows;

                // Count by CRAF classification
                foreach (var instance in allInstances)
                {
                    var classification = GetCRAFClassification(instance.WorkflowInstanceId);
                    switch (classification.ConfidentialityLevel)
                    {
                        case "HC":
                            report.HighlyConfidentialWorkflows++;
                            break;
                        case "SC":
                            report.SensitiveConfidentialWorkflows++;
                            break;
                        case "NC":
                            report.NormalConfidentialWorkflows++;
                            break;
                        case "PC":
                            report.PublicWorkflows++;
                            break;
                        default:
                            report.UnclassifiedWorkflows++;
                            break;
                    }
                }

                return report;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error generating compliance report: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Log compliance event for audit trail.
        /// </summary>
        public void LogComplianceEvent(string eventType, int workflowInstanceId, string details, UserContext userContext)
        {
            if (string.IsNullOrEmpty(eventType)) throw new ArgumentNullException(nameof(eventType));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            try
            {
                _auditService.Log(new AuditEntry
                {
                    Action = $"Compliance{eventType}",
                    TableName = "WorkflowCompliance",
                    RecordID = workflowInstanceId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = details
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error logging compliance event: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Validate workflow against all compliance requirements.
        /// </summary>
        public ComplianceValidationResult ValidateWorkflowCompliance(WorkflowInstance instance, UserContext userContext)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            try
            {
                var result = new ComplianceValidationResult
                {
                    WorkflowInstanceId = instance.WorkflowInstanceId,
                    IsValid = true,
                    Errors = new List<string>(),
                    Warnings = new List<string>(),
                    ValidationDate = DateTime.Now,
                    ValidatedBy = userContext.ADUserName
                };

                // Check OIC Circular 2566 compliance
                if (!IsCompliantWithOICCircular2566(instance))
                {
                    result.IsValid = false;
                    result.Errors.Add("OIC Circular 2566 compliance check failed");
                }

                // Check CRAF classification
                var classification = GetCRAFClassification(instance.WorkflowInstanceId);
                if (string.IsNullOrEmpty(classification.ConfidentialityLevel))
                {
                    result.Warnings.Add("CRAF classification not set");
                }

                // Check encryption for sensitive data
                if (_encryptionService.ContainsSensitiveData(instance.InputParameters) && !classification.RequiresEncryption)
                {
                    result.Warnings.Add("Sensitive data detected but encryption not required");
                }

                return result;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error validating workflow compliance: {ex.Message}", ex);
                throw;
            }
        }

        // Helper methods

        private List<string> GetUserRoles(UserContext userContext)
        {
            var roles = new List<string>();

            if (userContext.AppRole >= 4)
                roles.Add("Admin");
            else if (userContext.AppRole >= 3)
                roles.Add("Manager");
            else if (userContext.AppRole >= 2)
                roles.Add("User");
            else
                roles.Add("Guest");

            return roles;
        }
    }
}
