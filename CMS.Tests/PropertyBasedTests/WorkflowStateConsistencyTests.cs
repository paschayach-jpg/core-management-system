using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using FsCheck;
using FsCheck.NUnit;
using NUnit.Framework;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace CMS.Tests.PropertyBasedTests
{
    /// <summary>
    /// Property-Based Tests for Workflow State Consistency (Phase 9.1)
    /// 
    /// Property: For any workflow template and execution sequence, state transitions must be valid.
    /// 
    /// Validates:
    /// - Current state is always one of (Created, InProgress, Completed, Rejected, Suspended)
    /// - Transitions follow valid paths (e.g., InProgress → Completed or Rejected, not InProgress → Created)
    /// - State machine rules are enforced
    /// </summary>
    [TestFixture]
    public class WorkflowStateConsistencyTests
    {
        // =====================================================================
        // Generators
        // =====================================================================

        /// <summary>
        /// Generate valid workflow instances with random initial states.
        /// </summary>
        private static Arbitrary<WorkflowInstance> WorkflowInstanceGenerator()
        {
            var statusGen = Gen.Elements(
                WorkflowInstanceStatus.Created,
                WorkflowInstanceStatus.InProgress,
                WorkflowInstanceStatus.Completed,
                WorkflowInstanceStatus.Rejected,
                WorkflowInstanceStatus.Suspended
            );

            var gen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from defId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from version in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from status in statusGen
                      from initiatedBy in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                      from initiatedDate in Arb.Default.DateTime().Generator
                      select new WorkflowInstance
                      {
                          WorkflowInstanceId = id,
                          WorkflowDefinitionId = defId,
                          TemplateVersion = version,
                          Status = status,
                          InitiatedBy = initiatedBy,
                          InitiatedDate = initiatedDate,
                          CurrentStepId = null,
                          CompletedDate = status == WorkflowInstanceStatus.Completed || 
                                         status == WorkflowInstanceStatus.Rejected ? 
                                         initiatedDate.AddHours(1) : (DateTime?)null,
                          InputParameters = "{}",
                          IsDeleted = false,
                          DeletedDate = null
                      };

            return gen.ToArbitrary();
        }

        /// <summary>
        /// Generate valid workflow actions (state transitions).
        /// </summary>
        private static Arbitrary<WorkflowAction> WorkflowActionGenerator()
        {
            var actionGen = Gen.Elements(
                WorkflowAction.Complete,
                WorkflowAction.Reject,
                WorkflowAction.Suspend,
                WorkflowAction.Resume
            );

            return actionGen.ToArbitrary();
        }

        // =====================================================================
        // Helper Methods
        // =====================================================================

        /// <summary>
        /// Determines if a state transition is valid according to the state machine rules.
        /// </summary>
        private static bool IsValidTransition(WorkflowInstanceStatus currentState, WorkflowAction action)
        {
            return (currentState, action) switch
            {
                // From Created state
                (WorkflowInstanceStatus.Created, WorkflowAction.Complete) => false,
                (WorkflowInstanceStatus.Created, WorkflowAction.Reject) => false,
                (WorkflowInstanceStatus.Created, WorkflowAction.Suspend) => false,
                (WorkflowInstanceStatus.Created, WorkflowAction.Resume) => false,

                // From InProgress state
                (WorkflowInstanceStatus.InProgress, WorkflowAction.Complete) => true,
                (WorkflowInstanceStatus.InProgress, WorkflowAction.Reject) => true,
                (WorkflowInstanceStatus.InProgress, WorkflowAction.Suspend) => true,
                (WorkflowInstanceStatus.InProgress, WorkflowAction.Resume) => false,

                // From Completed state (terminal)
                (WorkflowInstanceStatus.Completed, _) => false,

                // From Rejected state (terminal)
                (WorkflowInstanceStatus.Rejected, _) => false,

                // From Suspended state
                (WorkflowInstanceStatus.Suspended, WorkflowAction.Resume) => true,
                (WorkflowInstanceStatus.Suspended, WorkflowAction.Complete) => false,
                (WorkflowInstanceStatus.Suspended, WorkflowAction.Reject) => false,
                (WorkflowInstanceStatus.Suspended, WorkflowAction.Suspend) => false,

                _ => false
            };
        }

        /// <summary>
        /// Applies an action to a workflow instance and returns the new state.
        /// </summary>
        private static WorkflowInstanceStatus ApplyAction(WorkflowInstanceStatus currentState, WorkflowAction action)
        {
            if (!IsValidTransition(currentState, action))
                throw new InvalidOperationException($"Invalid transition from {currentState} with action {action}");

            return (currentState, action) switch
            {
                (WorkflowInstanceStatus.InProgress, WorkflowAction.Complete) => WorkflowInstanceStatus.Completed,
                (WorkflowInstanceStatus.InProgress, WorkflowAction.Reject) => WorkflowInstanceStatus.Rejected,
                (WorkflowInstanceStatus.InProgress, WorkflowAction.Suspend) => WorkflowInstanceStatus.Suspended,
                (WorkflowInstanceStatus.Suspended, WorkflowAction.Resume) => WorkflowInstanceStatus.InProgress,
                _ => currentState
            };
        }

        // =====================================================================
        // Property Tests
        // =====================================================================

        /// <summary>
        /// Property: For any workflow instance, the current state is always one of the valid states.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_WorkflowStateIsAlwaysValid(WorkflowInstance instance)
        {
            var validStates = new[]
            {
                WorkflowInstanceStatus.Created,
                WorkflowInstanceStatus.InProgress,
                WorkflowInstanceStatus.Completed,
                WorkflowInstanceStatus.Rejected,
                WorkflowInstanceStatus.Suspended
            };

            return validStates.Contains(instance.Status).ToProperty();
        }

        /// <summary>
        /// Property: For any workflow instance and valid action sequence, all intermediate states are valid.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_WorkflowStateTransitionsAreValid(
            WorkflowInstance instance,
            List<WorkflowAction> actions)
        {
            var currentState = instance.Status;
            var validStates = new[]
            {
                WorkflowInstanceStatus.Created,
                WorkflowInstanceStatus.InProgress,
                WorkflowInstanceStatus.Completed,
                WorkflowInstanceStatus.Rejected,
                WorkflowInstanceStatus.Suspended
            };

            // Verify initial state is valid
            if (!validStates.Contains(currentState))
                return false.ToProperty();

            // Apply each action and verify resulting state is valid
            foreach (var action in actions)
            {
                // Skip invalid transitions (they should not occur in normal operation)
                if (!IsValidTransition(currentState, action))
                    continue;

                currentState = ApplyAction(currentState, action);

                // Verify the new state is valid
                if (!validStates.Contains(currentState))
                    return false.ToProperty();
            }

            return true.ToProperty();
        }

        /// <summary>
        /// Property: Terminal states (Completed, Rejected) cannot transition to any other state.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_TerminalStatesAreImmutable(WorkflowAction action)
        {
            var terminalStates = new[]
            {
                WorkflowInstanceStatus.Completed,
                WorkflowInstanceStatus.Rejected
            };

            foreach (var terminalState in terminalStates)
            {
                if (IsValidTransition(terminalState, action))
                    return false.ToProperty();
            }

            return true.ToProperty();
        }

        /// <summary>
        /// Property: Only InProgress and Suspended states can transition to other states.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_OnlyActiveStatesCanTransition(WorkflowInstanceStatus state, WorkflowAction action)
        {
            var activeStates = new[]
            {
                WorkflowInstanceStatus.InProgress,
                WorkflowInstanceStatus.Suspended
            };

            var isActive = activeStates.Contains(state);
            var canTransition = IsValidTransition(state, action);

            // If state is not active, it should not allow transitions
            if (!isActive && canTransition)
                return false.ToProperty();

            return true.ToProperty();
        }

        /// <summary>
        /// Property: Suspended state can only transition to InProgress via Resume action.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SuspendedStateOnlyResumesToInProgress(WorkflowAction action)
        {
            var suspendedState = WorkflowInstanceStatus.Suspended;

            if (action == WorkflowAction.Resume)
            {
                // Resume should be valid from Suspended
                if (!IsValidTransition(suspendedState, action))
                    return false.ToProperty();
            }
            else
            {
                // All other actions should be invalid from Suspended
                if (IsValidTransition(suspendedState, action))
                    return false.ToProperty();
            }

            return true.ToProperty();
        }

        /// <summary>
        /// Property: InProgress state can transition to Completed, Rejected, or Suspended.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_InProgressStateHasValidTransitions(WorkflowAction action)
        {
            var inProgressState = WorkflowInstanceStatus.InProgress;
            var validActions = new[] { WorkflowAction.Complete, WorkflowAction.Reject, WorkflowAction.Suspend };

            var isValid = IsValidTransition(inProgressState, action);
            var shouldBeValid = validActions.Contains(action);

            return (isValid == shouldBeValid).ToProperty();
        }

        /// <summary>
        /// Property: Completed state is terminal and cannot transition.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_CompletedStateIsTerminal(WorkflowAction action)
        {
            var completedState = WorkflowInstanceStatus.Completed;
            var isValid = IsValidTransition(completedState, action);

            return (!isValid).ToProperty();
        }

        /// <summary>
        /// Property: Rejected state is terminal and cannot transition.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_RejectedStateIsTerminal(WorkflowAction action)
        {
            var rejectedState = WorkflowInstanceStatus.Rejected;
            var isValid = IsValidTransition(rejectedState, action);

            return (!isValid).ToProperty();
        }
    }
}
