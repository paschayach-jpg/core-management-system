using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    /// <summary>
    /// Repository interface for WorkflowStep and WorkflowConnection data access.
    /// Steps and connections are part of the template definition and are versioned
    /// together with their parent WorkflowDefinition.
    /// </summary>
    public interface IWorkflowStepRepository
    {
        /// <summary>
        /// Retrieves a single workflow step by ID.
        /// Returns null when not found or soft-deleted.
        /// </summary>
        WorkflowStep GetStep(int workflowStepId);

        /// <summary>
        /// Returns all non-deleted steps for a given template definition,
        /// ordered by SequenceNumber ascending.
        /// </summary>
        IList<WorkflowStep> GetStepsByTemplate(int workflowDefinitionId);

        /// <summary>Inserts a new workflow step and returns its generated ID.</summary>
        int CreateStep(WorkflowStep step);

        /// <summary>Updates an existing workflow step.</summary>
        void UpdateStep(WorkflowStep step);

        /// <summary>Soft-deletes a workflow step.</summary>
        void SoftDeleteStep(int workflowStepId);

        /// <summary>
        /// Returns all non-deleted connections for a given template definition,
        /// ordered by SequenceNumber ascending.
        /// </summary>
        IList<WorkflowConnection> GetConnectionsByTemplate(int workflowDefinitionId);

        /// <summary>
        /// Returns all non-deleted outgoing connections from a specific step,
        /// ordered by SequenceNumber ascending (evaluation order for condition matching).
        /// </summary>
        IList<WorkflowConnection> GetConnectionsFromStep(int fromStepId);

        /// <summary>Inserts a new workflow connection and returns its generated ID.</summary>
        int CreateConnection(WorkflowConnection connection);

        /// <summary>Updates an existing workflow connection.</summary>
        void UpdateConnection(WorkflowConnection connection);

        /// <summary>Soft-deletes a workflow connection.</summary>
        void SoftDeleteConnection(int workflowConnectionId);
    }
}
