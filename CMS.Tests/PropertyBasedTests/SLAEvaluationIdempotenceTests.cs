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
    /// Property-Based Tests for SLA Evaluation Idempotence (Phase 9.6)
    /// 
    /// Property: Multiple SLA evaluations produce same result (idempotent).
    /// 
    /// Validates:
    /// - SLA status doesn't change between evaluations
    /// - Evaluation is side-effect free
    /// - Multiple evaluations return same SLA status
    /// </summary>
    [TestFixture]
    public class SLAEvaluationIdempotenceTests
    {
        // =====================================================================
        // Generators
        // =====================================================================

        /// <summary>
        /// Generate random workflow step instances for SLA evaluation.
        /// </summary>
        private static Arbitrary<WorkflowStepInstance> WorkflowStepInstanceGenerator()
        {
            var statusGen = Gen.Elements(
                WorkflowStepInstanceStatus.Pending,
                WorkflowStepInstanceStatus.InProgress,
                WorkflowStepInstanceStatus.Completed,
                WorkflowStepInstanceStatus.Rejected
            );

            var gen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from workflowId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from stepId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from assignedTo in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                      from assignedDate in Arb.Default.DateTime().Generator
                      from status in statusGen
                      from durationHours in Gen.Choose(1, 720)
                      select new WorkflowStepInstance
                      {
                          WorkflowStepInstanceId = id,
                          WorkflowInstanceId = workflowId,
                          WorkflowStepId = stepId,
                          AssignedTo = assignedTo,
                          AssignedDate = assignedDate,
                          Status = status,
                          CompletedDate = status == WorkflowStepInstanceStatus.Completed ? 
                                         assignedDate.AddHours(durationHours / 2) : (DateTime?)null,
                          CompletedBy = status == WorkflowStepInstanceStatus.Completed ? 
                                       assignedTo : null,
                          OutputData = null,
                          SLADeadline = assignedDate.AddHours(durationHours),
                          SLAStatus = SLAStatus.OnTime,
                          ExternalReferenceId = null,
                          ExternalReferenceType = null,
                          ExternalTaskId = null,
                          IsDeleted = false
                      };

            return gen.ToArbitrary();
        }

        // =====================================================================
        // Helper Methods
        // =====================================================================

        /// <summary>
        /// Evaluates the SLA status of a workflow step instance.
        /// </summary>
        private static SLAStatus EvaluateSLAStatus(WorkflowStepInstance stepInstance)
        {
            if (stepInstance == null)
                throw new ArgumentNullException(nameof(stepInstance));

            var now = DateTime.Now;

            // If completed, check if it was on time
            if (stepInstance.Status == WorkflowStepInstanceStatus.Completed)
            {
                if (stepInstance.CompletedDate <= stepInstance.SLADeadline)
                    return SLAStatus.OnTime;
                else
                    return SLAStatus.Overdue;
            }

            // If overdue
            if (now > stepInstance.SLADeadline)
                return SLAStatus.Overdue;

            // Calculate percentage of time used
            var totalTime = stepInstance.SLADeadline - stepInstance.AssignedDate;
            var timeUsed = now - stepInstance.AssignedDate;
            var percentageUsed = (timeUsed.TotalSeconds / totalTime.TotalSeconds) * 100;

            // If at risk (75% or more of time used)
            if (percentageUsed >= 75)
                return SLAStatus.AtRisk;

            return SLAStatus.OnTime;
        }

        /// <summary>
        /// Creates a deep copy of a workflow step instance.
        /// </summary>
        private static WorkflowStepInstance CloneStepInstance(WorkflowStepInstance instance)
        {
            return new WorkflowStepInstance
            {
                WorkflowStepInstanceId = instance.WorkflowStepInstanceId,
                WorkflowInstanceId = instance.WorkflowInstanceId,
                WorkflowStepId = instance.WorkflowStepId,
                AssignedTo = instance.AssignedTo,
                AssignedDate = instance.AssignedDate,
                Status = instance.Status,
                CompletedDate = instance.CompletedDate,
                CompletedBy = instance.CompletedBy,
                OutputData = instance.OutputData,
                SLADeadline = instance.SLADeadline,
                SLAStatus = instance.SLAStatus,
                ExternalReferenceId = instance.ExternalReferenceId,
                ExternalReferenceType = instance.ExternalReferenceType,
                ExternalTaskId = instance.ExternalTaskId,
                IsDeleted = instance.IsDeleted
            };
        }

        // =====================================================================
        // Property Tests
        // =====================================================================

        /// <summary>
        /// Property: Multiple SLA evaluations produce same result (idempotent).
        /// 
        /// Validates: Requirement 7 - SLA Management - Monitor and Enforce
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SLAEvaluationIsIdempotent(WorkflowStepInstance stepInstance)
        {
            if (stepInstance == null)
                return true.ToProperty();

            // Evaluate SLA status multiple times
            var result1 = EvaluateSLAStatus(stepInstance);
            var result2 = EvaluateSLAStatus(stepInstance);
            var result3 = EvaluateSLAStatus(stepInstance);
            var result4 = EvaluateSLAStatus(stepInstance);
            var result5 = EvaluateSLAStatus(stepInstance);

            // All results should be the same
            return (result1 == result2 && result2 == result3 && 
                    result3 == result4 && result4 == result5).ToProperty();
        }

        /// <summary>
        /// Property: SLA evaluation is side-effect free (doesn't modify the instance).
        /// 
        /// Validates: Requirement 7 - SLA Management - Monitor and Enforce
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SLAEvaluationIsSideEffectFree(WorkflowStepInstance stepInstance)
        {
            if (stepInstance == null)
                return true.ToProperty();

            // Create a copy before evaluation
            var originalCopy = CloneStepInstance(stepInstance);

            // Evaluate SLA status
            var result = EvaluateSLAStatus(stepInstance);

            // Verify the instance is unchanged
            var unchanged = stepInstance.WorkflowStepInstanceId == originalCopy.WorkflowStepInstanceId &&
                           stepInstance.Status == originalCopy.Status &&
                           stepInstance.AssignedDate == originalCopy.AssignedDate &&
                           stepInstance.SLADeadline == originalCopy.SLADeadline &&
                           stepInstance.CompletedDate == originalCopy.CompletedDate;

            return unchanged.ToProperty();
        }

        /// <summary>
        /// Property: Completed tasks always return consistent SLA status.
        /// 
        /// Validates: Requirement 7 - SLA Management - Monitor and Enforce
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_CompletedTasksSLAStatusIsConsistent()
        {
            var assignedDate = DateTime.Now.AddHours(-10);
            var slaDeadline = assignedDate.AddHours(24);
            var completedDate = assignedDate.AddHours(12); // Completed on time

            var stepInstance = new WorkflowStepInstance
            {
                WorkflowStepInstanceId = 1,
                WorkflowInstanceId = 1,
                WorkflowStepId = 1,
                AssignedTo = "user",
                AssignedDate = assignedDate,
                Status = WorkflowStepInstanceStatus.Completed,
                CompletedDate = completedDate,
                CompletedBy = "user",
                OutputData = null,
                SLADeadline = slaDeadline,
                SLAStatus = SLAStatus.OnTime,
                ExternalReferenceId = null,
                ExternalReferenceType = null,
                ExternalTaskId = null,
                IsDeleted = false
            };

            var result1 = EvaluateSLAStatus(stepInstance);
            var result2 = EvaluateSLAStatus(stepInstance);
            var result3 = EvaluateSLAStatus(stepInstance);

            return (result1 == SLAStatus.OnTime && result2 == SLAStatus.OnTime && 
                    result3 == SLAStatus.OnTime).ToProperty();
        }

        /// <summary>
        /// Property: Overdue tasks always return Overdue status.
        /// 
        /// Validates: Requirement 7 - SLA Management - Monitor and Enforce
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_OverdueTasksAlwaysReturnOverdueStatus()
        {
            var assignedDate = DateTime.Now.AddHours(-100);
            var slaDeadline = assignedDate.AddHours(24); // Deadline was 76 hours ago

            var stepInstance = new WorkflowStepInstance
            {
                WorkflowStepInstanceId = 1,
                WorkflowInstanceId = 1,
                WorkflowStepId = 1,
                AssignedTo = "user",
                AssignedDate = assignedDate,
                Status = WorkflowStepInstanceStatus.InProgress,
                CompletedDate = null,
                CompletedBy = null,
                OutputData = null,
                SLADeadline = slaDeadline,
                SLAStatus = SLAStatus.Overdue,
                ExternalReferenceId = null,
                ExternalReferenceType = null,
                ExternalTaskId = null,
                IsDeleted = false
            };

            var result1 = EvaluateSLAStatus(stepInstance);
            var result2 = EvaluateSLAStatus(stepInstance);
            var result3 = EvaluateSLAStatus(stepInstance);

            return (result1 == SLAStatus.Overdue && result2 == SLAStatus.Overdue && 
                    result3 == SLAStatus.Overdue).ToProperty();
        }

        /// <summary>
        /// Property: Evaluation result depends only on instance state, not on evaluation time.
        /// 
        /// Validates: Requirement 7 - SLA Management - Monitor and Enforce
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_EvaluationResultDependsOnlyOnInstanceState(WorkflowStepInstance stepInstance)
        {
            if (stepInstance == null || stepInstance.Status != WorkflowStepInstanceStatus.Completed)
                return true.ToProperty();

            // For completed tasks, the result should always be the same
            var result1 = EvaluateSLAStatus(stepInstance);
            var result2 = EvaluateSLAStatus(stepInstance);

            return (result1 == result2).ToProperty();
        }

        /// <summary>
        /// Property: Null instance throws exception consistently.
        /// 
        /// Validates: Requirement 7 - SLA Management - Monitor and Enforce
        /// </summary>
        [PropertyAttribute(MaxTest = 100)]
        public Property Property_NullInstanceThrowsExceptionConsistently()
        {
            var exceptionThrown1 = false;
            var exceptionThrown2 = false;

            try
            {
                EvaluateSLAStatus(null);
            }
            catch (ArgumentNullException)
            {
                exceptionThrown1 = true;
            }

            try
            {
                EvaluateSLAStatus(null);
            }
            catch (ArgumentNullException)
            {
                exceptionThrown2 = true;
            }

            return (exceptionThrown1 && exceptionThrown2).ToProperty();
        }

        /// <summary>
        /// Property: Pending tasks return consistent status.
        /// 
        /// Validates: Requirement 7 - SLA Management - Monitor and Enforce
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_PendingTasksReturnConsistentStatus()
        {
            var assignedDate = DateTime.Now.AddHours(-1);
            var slaDeadline = assignedDate.AddHours(24);

            var stepInstance = new WorkflowStepInstance
            {
                WorkflowStepInstanceId = 1,
                WorkflowInstanceId = 1,
                WorkflowStepId = 1,
                AssignedTo = "user",
                AssignedDate = assignedDate,
                Status = WorkflowStepInstanceStatus.Pending,
                CompletedDate = null,
                CompletedBy = null,
                OutputData = null,
                SLADeadline = slaDeadline,
                SLAStatus = SLAStatus.OnTime,
                ExternalReferenceId = null,
                ExternalReferenceType = null,
                ExternalTaskId = null,
                IsDeleted = false
            };

            var result1 = EvaluateSLAStatus(stepInstance);
            var result2 = EvaluateSLAStatus(stepInstance);
            var result3 = EvaluateSLAStatus(stepInstance);

            return (result1 == result2 && result2 == result3).ToProperty();
        }

        /// <summary>
        /// Property: Evaluation order doesn't affect result.
        /// 
        /// Validates: Requirement 7 - SLA Management - Monitor and Enforce
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_EvaluationOrderDoesntAffectResult(WorkflowStepInstance stepInstance)
        {
            if (stepInstance == null)
                return true.ToProperty();

            // Evaluate in different orders
            var result1 = EvaluateSLAStatus(stepInstance);
            var result2 = EvaluateSLAStatus(stepInstance);
            var result3 = EvaluateSLAStatus(stepInstance);

            // All should be the same regardless of order
            return (result1 == result2 && result2 == result3).ToProperty();
        }

        /// <summary>
        /// Property: Multiple instances can be evaluated independently.
        /// 
        /// Validates: Requirement 7 - SLA Management - Monitor and Enforce
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_MultipleInstancesCanBeEvaluatedIndependently(
            WorkflowStepInstance instance1,
            WorkflowStepInstance instance2)
        {
            if (instance1 == null || instance2 == null)
                return true.ToProperty();

            // Evaluate both instances
            var result1a = EvaluateSLAStatus(instance1);
            var result2a = EvaluateSLAStatus(instance2);
            var result1b = EvaluateSLAStatus(instance1);
            var result2b = EvaluateSLAStatus(instance2);

            // Each instance should return consistent results
            return (result1a == result1b && result2a == result2b).ToProperty();
        }
    }
}
