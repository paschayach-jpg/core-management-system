using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    /// <summary>
    /// Repository interface for WorkflowStepInstance data access.
    /// Step instances represent individual executions of a template step within a
    /// running workflow instance.
    /// </summary>
    public interface IWorkflowStepInstanceRepository
    {
        /// <summary>Creates a new step instance and returns its generated ID.</summary>
        int CreateStepInstance(WorkflowStepInstance instance);

        /// <summary>
        /// Retrieves a single step instance by ID.
        /// Returns null when not found or soft-deleted.
        /// </summary>
        WorkflowStepInstance GetStepInstance(int workflowStepInstanceId);

        /// <summary>Updates an existing step instance (status, SLA status, output data, etc.).</summary>
        void UpdateStepInstance(WorkflowStepInstance instance);

        /// <summary>
        /// Returns all non-deleted step instances for a given workflow instance,
        /// ordered by SequenceNumber then AssignedDate.
        /// </summary>
        IList<WorkflowStepInstance> GetStepInstancesByWorkflow(int workflowInstanceId);

        /// <summary>
        /// Returns all non-deleted, non-terminal step instances across all workflows.
        /// Used by the WFESLABackgroundJob to evaluate SLA status.
        /// </summary>
        IList<WorkflowStepInstance> GetPendingStepInstances();

        /// <summary>
        /// Returns all non-deleted step instances assigned to a specific user,
        /// ordered by SLADeadline ascending (most urgent first).
        /// </summary>
        IList<WorkflowStepInstance> GetStepInstancesByUser(string adUserName);

        /// <summary>
        /// Retrieves a step instance by its external reference ID and type.
        /// Used when receiving callbacks from integrated modules (e.g. Approval, Task Management).
        /// Returns null when not found.
        /// </summary>
        WorkflowStepInstance GetByExternalReference(int externalReferenceId, string externalReferenceType);

        /// <summary>Soft-deletes a step instance.</summary>
        void SoftDeleteStepInstance(int workflowStepInstanceId);
    }
}
