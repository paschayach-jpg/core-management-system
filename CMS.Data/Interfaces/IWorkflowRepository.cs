using System.Collections.Generic;
using CMS.Data.Models;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    /// <summary>
    /// Repository interface for WorkflowInstance data access.
    /// All database operations are performed via stored procedures.
    /// </summary>
    public interface IWorkflowRepository
    {
        /// <summary>Creates a new workflow instance and returns its generated ID.</summary>
        int CreateWorkflowInstance(WorkflowInstance instance);

        /// <summary>
        /// Retrieves a single workflow instance by ID.
        /// Returns null when not found or soft-deleted.
        /// </summary>
        WorkflowInstance GetWorkflowInstance(int workflowInstanceId);

        /// <summary>Updates the status, current step, and completion date of a workflow instance.</summary>
        void UpdateWorkflowInstance(WorkflowInstance instance);

        /// <summary>Returns a paged list of workflow instances matching the given filter.</summary>
        PagedResult<WorkflowInstance> GetWorkflowInstances(WorkflowInstanceFilter filter);

        /// <summary>
        /// Returns all non-deleted, non-terminal workflow instances.
        /// Used by the SLA background job.
        /// </summary>
        IList<WorkflowInstance> GetActiveWorkflowInstances();

        /// <summary>
        /// Soft-deletes a workflow instance.
        /// Only allowed when the instance is in a terminal state (Completed or Rejected).
        /// </summary>
        void SoftDeleteWorkflowInstance(int workflowInstanceId, string deletedBy);
    }

    /// <summary>Filter parameters for querying workflow instances.</summary>
    public class WorkflowInstanceFilter
    {
        public int?   WorkflowDefinitionId { get; set; }
        public string InitiatedBy          { get; set; }
        public byte?  Status               { get; set; }
        public System.DateTime? FromDate   { get; set; }
        public System.DateTime? ToDate     { get; set; }
        public int    PageNumber           { get; set; }
        public int    PageSize             { get; set; }

        public WorkflowInstanceFilter() { PageNumber = 1; PageSize = 50; }
    }
}
