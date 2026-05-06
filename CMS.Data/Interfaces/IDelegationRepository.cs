using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    /// <summary>
    /// Repository interface for WFDelegation data access.
    /// Delegation records are append-only (soft-delete only) to preserve the
    /// full audit trail of task transfers.
    /// </summary>
    public interface IDelegationRepository
    {
        /// <summary>
        /// Records a new delegation event, deactivates the previous active delegation
        /// for the same step instance, and updates the step instance's AssignedTo.
        /// Returns the generated WFDelegationId.
        /// </summary>
        int CreateDelegation(WFDelegation delegation, int maxDelegationDepth);

        /// <summary>
        /// Retrieves a single delegation record by ID.
        /// Returns null when not found or soft-deleted.
        /// </summary>
        WFDelegation GetDelegation(int wfDelegationId);

        /// <summary>
        /// Returns the full delegation chain for a step instance,
        /// ordered by DelegationDepth ascending (oldest first).
        /// </summary>
        IList<WFDelegation> GetDelegationsByStepInstance(int workflowStepInstanceId);

        /// <summary>
        /// Returns the currently active delegation record for a step instance.
        /// Returns null when the step has not been delegated or all delegations are inactive.
        /// </summary>
        WFDelegation GetActiveDelegation(int workflowStepInstanceId);

        /// <summary>
        /// Returns the current delegation depth for a step instance.
        /// Returns 0 when the step has never been delegated.
        /// </summary>
        int GetDelegationDepth(int workflowStepInstanceId);

        /// <summary>
        /// Deactivates all active delegation records for a step instance.
        /// Called when the step reaches a terminal state.
        /// </summary>
        void DeactivateDelegations(int workflowStepInstanceId);

        /// <summary>Updates a delegation record (e.g. to set IsActive = false).</summary>
        void UpdateDelegation(WFDelegation delegation);
    }
}
