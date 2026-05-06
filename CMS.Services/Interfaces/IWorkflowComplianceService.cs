using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Workflow compliance service for Thai regulations and data classification.
    /// Implements Requirements 14.2 (OIC Circular 2566, CRAF data classification).
    /// Ensures compliance with Thai regulatory requirements.
    /// </summary>
    public interface IWorkflowComplianceService
    {
        /// <summary>
        /// Check if workflow complies with OIC Circular 2566 (Thai financial regulations).
        /// </summary>
        bool IsCompliantWithOICCircular2566(WorkflowInstance instance);

        /// <summary>
        /// Get OIC Circular 2566 compliance status for a workflow.
        /// </summary>
        OICComplianceStatus GetOICComplianceStatus(int workflowInstanceId);

        /// <summary>
        /// Classify workflow data according to CRAF (Confidentiality Rating and Handling).
        /// </summary>
        CRAFClassification ClassifyWorkflowData(string workflowData);

        /// <summary>
        /// Get CRAF classification for a workflow instance.
        /// </summary>
        CRAFClassification GetCRAFClassification(int workflowInstanceId);

        /// <summary>
        /// Set CRAF classification for a workflow instance.
        /// </summary>
        void SetCRAFClassification(int workflowInstanceId, CRAFClassification classification, UserContext userContext);

        /// <summary>
        /// Check if user has access to data with specific CRAF classification.
        /// </summary>
        bool CanAccessCRAFClassifiedData(UserContext userContext, CRAFClassification classification);

        /// <summary>
        /// Get list of workflows that violate OIC Circular 2566.
        /// </summary>
        List<WorkflowInstance> GetOICNonCompliantWorkflows();

        /// <summary>
        /// Get list of workflows with sensitive CRAF classification.
        /// </summary>
        List<WorkflowInstance> GetSensitivelyClassifiedWorkflows();

        /// <summary>
        /// Generate compliance audit report.
        /// </summary>
        ComplianceAuditReport GenerateComplianceReport(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Log compliance event for audit trail.
        /// </summary>
        void LogComplianceEvent(string eventType, int workflowInstanceId, string details, UserContext userContext);

        /// <summary>
        /// Validate workflow against all compliance requirements.
        /// </summary>
        ComplianceValidationResult ValidateWorkflowCompliance(WorkflowInstance instance, UserContext userContext);
    }

    /// <summary>
    /// OIC Circular 2566 compliance status.
    /// </summary>
    public class OICComplianceStatus
    {
        public int WorkflowInstanceId { get; set; }
        public bool IsCompliant { get; set; }
        public List<string> ViolatedRequirements { get; set; }
        public DateTime LastCheckedDate { get; set; }
        public string CheckedBy { get; set; }
    }

    /// <summary>
    /// CRAF (Confidentiality Rating and Handling) classification.
    /// </summary>
    public class CRAFClassification
    {
        public int ClassificationId { get; set; }
        public int WorkflowInstanceId { get; set; }
        public string ConfidentialityLevel { get; set; } // HC (Highly Confidential), SC (Sensitive Confidential), NC (Normal Confidential), PC (Public)
        public string HandlingInstructions { get; set; }
        public bool RequiresEncryption { get; set; }
        public bool RequiresAuditLog { get; set; }
        public bool RestrictedDistribution { get; set; }
        public List<string> AllowedRoles { get; set; }
        public DateTime ClassificationDate { get; set; }
        public string ClassifiedBy { get; set; }
    }

    /// <summary>
    /// Compliance validation result.
    /// </summary>
    public class ComplianceValidationResult
    {
        public int WorkflowInstanceId { get; set; }
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
        public DateTime ValidationDate { get; set; }
        public string ValidatedBy { get; set; }
    }

    /// <summary>
    /// Compliance audit report.
    /// </summary>
    public class ComplianceAuditReport
    {
        public DateTime ReportDate { get; set; }
        public int TotalWorkflows { get; set; }
        public int OICCompliantWorkflows { get; set; }
        public int OICNonCompliantWorkflows { get; set; }
        public int HighlyConfidentialWorkflows { get; set; }
        public int SensitiveConfidentialWorkflows { get; set; }
        public int NormalConfidentialWorkflows { get; set; }
        public int PublicWorkflows { get; set; }
        public int UnclassifiedWorkflows { get; set; }
        public List<ComplianceViolation> Violations { get; set; }
    }

    /// <summary>
    /// Compliance violation detail.
    /// </summary>
    public class ComplianceViolation
    {
        public int WorkflowInstanceId { get; set; }
        public string ViolationType { get; set; } // OIC, CRAF, PDPA, etc.
        public string Description { get; set; }
        public DateTime ViolationDate { get; set; }
        public string ResolvedBy { get; set; }
        public DateTime? ResolvedDate { get; set; }
    }
}
