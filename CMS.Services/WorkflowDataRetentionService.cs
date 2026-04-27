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
    /// Implements IWorkflowDataRetentionService: manages workflow data archival and deletion.
    /// Supports Requirements 14.2 (Data retention policies).
    /// </summary>
    public class WorkflowDataRetentionService : IWorkflowDataRetentionService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowTemplateRepository _templateRepository;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;

        // Default retention policies (in days)
        private const int DefaultArchivalDays = 365; // 1 year
        private const int DefaultDeletionDays = 2555; // 7 years (PDPA requirement)

        public WorkflowDataRetentionService(
            IWorkflowRepository workflowRepository,
            IWorkflowTemplateRepository templateRepository,
            IAuditService auditService,
            ILoggingService loggingService)
        {
            if (workflowRepository == null) throw new ArgumentNullException(nameof(workflowRepository));
            if (templateRepository == null) throw new ArgumentNullException(nameof(templateRepository));
            if (auditService == null) throw new ArgumentNullException(nameof(auditService));
            if (loggingService == null) throw new ArgumentNullException(nameof(loggingService));

            _workflowRepository = workflowRepository;
            _templateRepository = templateRepository;
            _auditService = auditService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Get retention policy for a workflow template.
        /// </summary>
        public WorkflowRetentionPolicy GetRetentionPolicy(int workflowDefinitionId)
        {
            try
            {
                // TODO: Query retention policy from database
                // For now, return default policy
                return new WorkflowRetentionPolicy
                {
                    WorkflowDefinitionId = workflowDefinitionId,
                    ArchivalDaysAfterCompletion = DefaultArchivalDays,
                    DeletionDaysAfterArchival = DefaultDeletionDays,
                    ArchiveToLongTermStorage = true,
                    EncryptArchivedData = true,
                    MaintainAuditTrail = true,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting retention policy: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Set retention policy for a workflow template.
        /// </summary>
        public void SetRetentionPolicy(int workflowDefinitionId, WorkflowRetentionPolicy policy, UserContext userContext)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            try
            {
                // Validate policy
                if (policy.ArchivalDaysAfterCompletion < 0)
                    throw new ArgumentException("Archival days must be non-negative");

                if (policy.DeletionDaysAfterArchival < 0)
                    throw new ArgumentException("Deletion days must be non-negative");

                _loggingService.Info($"Setting retention policy for workflow {workflowDefinitionId}");

                // TODO: Save policy to database

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Action = "RetentionPolicySet",
                    TableName = "WorkflowRetentionPolicy",
                    RecordID = workflowDefinitionId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = $"Archival: {policy.ArchivalDaysAfterCompletion} days, Deletion: {policy.DeletionDaysAfterArchival} days"
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error setting retention policy: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Check if a workflow instance should be archived based on retention policy.
        /// </summary>
        public bool ShouldArchiveInstance(WorkflowInstance instance, WorkflowRetentionPolicy policy)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (policy == null) throw new ArgumentNullException(nameof(policy));

            // Instance must be completed
            if (instance.Status != WorkflowInstanceStatus.Completed && 
                instance.Status != WorkflowInstanceStatus.Rejected)
                return false;

            // Check if archival period has passed
            if (instance.CompletedDate.HasValue)
            {
                var daysSinceCompletion = (DateTime.Now - instance.CompletedDate.Value).TotalDays;
                return daysSinceCompletion >= policy.ArchivalDaysAfterCompletion;
            }

            return false;
        }

        /// <summary>
        /// Check if a workflow instance should be deleted based on retention policy.
        /// </summary>
        public bool ShouldDeleteInstance(WorkflowInstance instance, WorkflowRetentionPolicy policy)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (policy == null) throw new ArgumentNullException(nameof(policy));

            // Instance must be archived first
            // TODO: Check archival status from database

            // Check if deletion period has passed since archival
            // TODO: Implement deletion check based on archival date

            return false;
        }

        /// <summary>
        /// Archive a workflow instance to long-term storage.
        /// </summary>
        public void ArchiveWorkflowInstance(int workflowInstanceId, UserContext userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            try
            {
                var instance = _workflowRepository.GetWorkflowInstance(workflowInstanceId);
                if (instance == null)
                    throw new Exception($"Workflow instance {workflowInstanceId} not found");

                _loggingService.Info($"Archiving workflow instance {workflowInstanceId}");

                // TODO: Implement archival logic
                // 1. Compress workflow data
                // 2. Encrypt if required
                // 3. Move to long-term storage
                // 4. Update archival status in database
                // 5. Maintain audit trail

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowInstanceArchived",
                    TableName = "WorkflowInstance",
                    RecordID = workflowInstanceId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = $"Workflow instance archived"
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error archiving workflow instance: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Delete a workflow instance (soft delete with audit trail).
        /// </summary>
        public void DeleteWorkflowInstance(int workflowInstanceId, string reason, UserContext userContext)
        {
            if (string.IsNullOrEmpty(reason)) throw new ArgumentNullException(nameof(reason));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            try
            {
                var instance = _workflowRepository.GetWorkflowInstance(workflowInstanceId);
                if (instance == null)
                    throw new Exception($"Workflow instance {workflowInstanceId} not found");

                _loggingService.Info($"Deleting workflow instance {workflowInstanceId}: {reason}");

                // Soft delete
                instance.IsDeleted = true;
                instance.DeletedDate = DateTime.Now;
                _workflowRepository.UpdateWorkflowInstance(instance);

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowInstanceDeleted",
                    TableName = "WorkflowInstance",
                    RecordID = workflowInstanceId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = $"Workflow instance deleted. Reason: {reason}"
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error deleting workflow instance: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get list of instances eligible for archival.
        /// </summary>
        public List<WorkflowInstance> GetInstancesEligibleForArchival()
        {
            try
            {
                var allInstances = _workflowRepository.GetWorkflowInstances(
                    new Data.Interfaces.WorkflowInstanceFilter { PageSize = 10000 })
                    .Items
                    .Where(i => !i.IsDeleted)
                    .ToList();

                var eligibleInstances = new List<WorkflowInstance>();

                foreach (var instance in allInstances)
                {
                    var policy = GetRetentionPolicy(instance.WorkflowDefinitionId);
                    if (ShouldArchiveInstance(instance, policy))
                    {
                        eligibleInstances.Add(instance);
                    }
                }

                return eligibleInstances;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting instances eligible for archival: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get list of instances eligible for deletion.
        /// </summary>
        public List<WorkflowInstance> GetInstancesEligibleForDeletion()
        {
            try
            {
                var allInstances = _workflowRepository.GetWorkflowInstances(
                    new Data.Interfaces.WorkflowInstanceFilter { PageSize = 10000 })
                    .Items
                    .Where(i => !i.IsDeleted)
                    .ToList();

                var eligibleInstances = new List<WorkflowInstance>();

                foreach (var instance in allInstances)
                {
                    var policy = GetRetentionPolicy(instance.WorkflowDefinitionId);
                    if (ShouldDeleteInstance(instance, policy))
                    {
                        eligibleInstances.Add(instance);
                    }
                }

                return eligibleInstances;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting instances eligible for deletion: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Process archival and deletion according to retention policies.
        /// Called by background job.
        /// </summary>
        public void ProcessRetentionPolicies()
        {
            try
            {
                _loggingService.Info("Processing workflow retention policies");

                // Get instances eligible for archival
                var archivalInstances = GetInstancesEligibleForArchival();
                _loggingService.Info($"Found {archivalInstances.Count} instances eligible for archival");

                // Archive instances
                var systemContext = new UserContext
                {
                    ADUserName = "SYSTEM",
                    UserID = 0,
                    AppRole = 4 // Admin
                };

                foreach (var instance in archivalInstances)
                {
                    try
                    {
                        ArchiveWorkflowInstance(instance.WorkflowInstanceId, systemContext);
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Error($"Error archiving instance {instance.WorkflowInstanceId}: {ex.Message}", ex);
                    }
                }

                // Get instances eligible for deletion
                var deletionInstances = GetInstancesEligibleForDeletion();
                _loggingService.Info($"Found {deletionInstances.Count} instances eligible for deletion");

                // Delete instances
                foreach (var instance in deletionInstances)
                {
                    try
                    {
                        DeleteWorkflowInstance(instance.WorkflowInstanceId, "Retention policy expiration", systemContext);
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Error($"Error deleting instance {instance.WorkflowInstanceId}: {ex.Message}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error processing retention policies: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get retention policy compliance report.
        /// </summary>
        public RetentionPolicyReport GetComplianceReport(DateTime fromDate, DateTime toDate)
        {
            try
            {
                _loggingService.Info($"Generating retention policy compliance report for {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");

                var allInstances = _workflowRepository.GetWorkflowInstances(
                    new Data.Interfaces.WorkflowInstanceFilter { PageSize = 10000 })
                    .Items
                    .ToList();

                var report = new RetentionPolicyReport
                {
                    ReportDate = DateTime.Now,
                    TotalInstances = allInstances.Count,
                    ActiveInstances = allInstances.Count(i => !i.IsDeleted),
                    ArchivedInstances = 0, // TODO: Count archived instances
                    DeletedInstances = allInstances.Count(i => i.IsDeleted),
                    InstancesEligibleForArchival = GetInstancesEligibleForArchival().Count,
                    InstancesEligibleForDeletion = GetInstancesEligibleForDeletion().Count,
                    TotalArchivedDataSize = 0, // TODO: Calculate archived data size
                    PolicyDetails = new List<PolicyComplianceDetail>()
                };

                return report;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error generating compliance report: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Restore an archived workflow instance.
        /// </summary>
        public void RestoreArchivedInstance(int workflowInstanceId, UserContext userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));

            try
            {
                _loggingService.Info($"Restoring archived workflow instance {workflowInstanceId}");

                // TODO: Implement restore logic
                // 1. Retrieve from long-term storage
                // 2. Decrypt if required
                // 3. Restore to active storage
                // 4. Update archival status

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowInstanceRestored",
                    TableName = "WorkflowInstance",
                    RecordID = workflowInstanceId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = "Archived workflow instance restored"
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error restoring archived instance: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get archival status for a workflow instance.
        /// </summary>
        public ArchivalStatus GetArchivalStatus(int workflowInstanceId)
        {
            try
            {
                var instance = _workflowRepository.GetWorkflowInstance(workflowInstanceId);
                if (instance == null)
                    throw new Exception($"Workflow instance {workflowInstanceId} not found");

                // TODO: Query archival status from database
                return new ArchivalStatus
                {
                    WorkflowInstanceId = workflowInstanceId,
                    Status = instance.IsDeleted ? "Deleted" : "Active",
                    DeletedDate = instance.DeletedDate,
                    IsEncrypted = false,
                    SizeInBytes = 0
                };
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting archival status: {ex.Message}", ex);
                throw;
            }
        }
    }
}
