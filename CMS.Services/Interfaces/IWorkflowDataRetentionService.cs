using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Workflow data retention policy service.
    /// Implements Requirements 14.2 (Data retention policies).
    /// Manages archival and deletion of workflow data according to retention policies.
    /// </summary>
    public interface IWorkflowDataRetentionService
    {
        /// <summary>
        /// Get retention policy for a workflow template.
        /// </summary>
        WorkflowRetentionPolicy GetRetentionPolicy(int workflowDefinitionId);

        /// <summary>
        /// Set retention policy for a workflow template.
        /// </summary>
        void SetRetentionPolicy(int workflowDefinitionId, WorkflowRetentionPolicy policy, UserContext userContext);

        /// <summary>
        /// Check if a workflow instance should be archived based on retention policy.
        /// </summary>
        bool ShouldArchiveInstance(WorkflowInstance instance, WorkflowRetentionPolicy policy);

        /// <summary>
        /// Check if a workflow instance should be deleted based on retention policy.
        /// </summary>
        bool ShouldDeleteInstance(WorkflowInstance instance, WorkflowRetentionPolicy policy);

        /// <summary>
        /// Archive a workflow instance to long-term storage.
        /// </summary>
        void ArchiveWorkflowInstance(int workflowInstanceId, UserContext userContext);

        /// <summary>
        /// Delete a workflow instance (soft delete with audit trail).
        /// </summary>
        void DeleteWorkflowInstance(int workflowInstanceId, string reason, UserContext userContext);

        /// <summary>
        /// Get list of instances eligible for archival.
        /// </summary>
        List<WorkflowInstance> GetInstancesEligibleForArchival();

        /// <summary>
        /// Get list of instances eligible for deletion.
        /// </summary>
        List<WorkflowInstance> GetInstancesEligibleForDeletion();

        /// <summary>
        /// Process archival and deletion according to retention policies.
        /// Called by background job.
        /// </summary>
        void ProcessRetentionPolicies();

        /// <summary>
        /// Get retention policy compliance report.
        /// </summary>
        RetentionPolicyReport GetComplianceReport(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Restore an archived workflow instance.
        /// </summary>
        void RestoreArchivedInstance(int workflowInstanceId, UserContext userContext);

        /// <summary>
        /// Get archival status for a workflow instance.
        /// </summary>
        ArchivalStatus GetArchivalStatus(int workflowInstanceId);
    }

    /// <summary>
    /// Workflow retention policy.
    /// </summary>
    public class WorkflowRetentionPolicy
    {
        public int PolicyId { get; set; }
        public int WorkflowDefinitionId { get; set; }
        public int ArchivalDaysAfterCompletion { get; set; } // Days before archival
        public int DeletionDaysAfterArchival { get; set; } // Days before deletion after archival
        public bool ArchiveToLongTermStorage { get; set; }
        public string ArchiveStorageLocation { get; set; }
        public bool EncryptArchivedData { get; set; }
        public bool MaintainAuditTrail { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// Archival status for a workflow instance.
    /// </summary>
    public class ArchivalStatus
    {
        public int WorkflowInstanceId { get; set; }
        public string Status { get; set; } // Active, Archived, Deleted
        public DateTime? ArchivedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string ArchiveLocation { get; set; }
        public bool IsEncrypted { get; set; }
        public long SizeInBytes { get; set; }
    }

    /// <summary>
    /// Retention policy compliance report.
    /// </summary>
    public class RetentionPolicyReport
    {
        public DateTime ReportDate { get; set; }
        public int TotalInstances { get; set; }
        public int ActiveInstances { get; set; }
        public int ArchivedInstances { get; set; }
        public int DeletedInstances { get; set; }
        public int InstancesEligibleForArchival { get; set; }
        public int InstancesEligibleForDeletion { get; set; }
        public long TotalArchivedDataSize { get; set; }
        public List<PolicyComplianceDetail> PolicyDetails { get; set; }
    }

    /// <summary>
    /// Policy compliance detail.
    /// </summary>
    public class PolicyComplianceDetail
    {
        public int WorkflowDefinitionId { get; set; }
        public string WorkflowName { get; set; }
        public int ArchivalDays { get; set; }
        public int DeletionDays { get; set; }
        public int CompliancePercentage { get; set; }
        public int NonCompliantInstances { get; set; }
    }
}
