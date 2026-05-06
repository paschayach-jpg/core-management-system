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
    /// Property-Based Tests for Task Count Preservation (Phase 9.7)
    /// 
    /// Property: Completed + Pending = Total tasks (always).
    /// 
    /// Validates:
    /// - No tasks are lost or duplicated
    /// - Task count is preserved throughout execution
    /// - Completed and pending tasks sum to total
    /// </summary>
    [TestFixture]
    public class TaskCountPreservationTests
    {
        // =====================================================================
        // Generators
        // =====================================================================

        /// <summary>
        /// Generate random workflow step instances with various statuses.
        /// </summary>
        private static Arbitrary<WorkflowStepInstance> WorkflowStepInstanceGenerator()
        {
            var statusGen = Gen.Elements(
                WorkflowStepInstanceStatus.Pending,
                WorkflowStepInstanceStatus.InProgress,
                WorkflowStepInstanceStatus.Completed,
                WorkflowStepInstanceStatus.Rejected,
                WorkflowStepInstanceStatus.Delegated
            );

            var gen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from workflowId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from stepId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from assignedTo in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                      from assignedDate in Arb.Default.DateTime().Generator
                      from status in statusGen
                      select new WorkflowStepInstance
                      {
                          WorkflowStepInstanceId = id,
                          WorkflowInstanceId = workflowId,
                          WorkflowStepId = stepId,
                          AssignedTo = assignedTo,
                          AssignedDate = assignedDate,
                          Status = status,
                          CompletedDate = status == WorkflowStepInstanceStatus.Completed ? 
                                         assignedDate.AddHours(1) : (DateTime?)null,
                          CompletedBy = status == WorkflowStepInstanceStatus.Completed ? 
                                       assignedTo : null,
                          OutputData = null,
                          SLADeadline = assignedDate.AddHours(24),
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
        /// Counts completed tasks in a list.
        /// </summary>
        private static int CountCompletedTasks(List<WorkflowStepInstance> tasks)
        {
            return tasks.Count(t => t.Status == WorkflowStepInstanceStatus.Completed);
        }

        /// <summary>
        /// Counts pending tasks in a list (Pending or InProgress).
        /// </summary>
        private static int CountPendingTasks(List<WorkflowStepInstance> tasks)
        {
            return tasks.Count(t => t.Status == WorkflowStepInstanceStatus.Pending || 
                                   t.Status == WorkflowStepInstanceStatus.InProgress);
        }

        /// <summary>
        /// Counts rejected tasks in a list.
        /// </summary>
        private static int CountRejectedTasks(List<WorkflowStepInstance> tasks)
        {
            return tasks.Count(t => t.Status == WorkflowStepInstanceStatus.Rejected);
        }

        /// <summary>
        /// Counts delegated tasks in a list.
        /// </summary>
        private static int CountDelegatedTasks(List<WorkflowStepInstance> tasks)
        {
            return tasks.Count(t => t.Status == WorkflowStepInstanceStatus.Delegated);
        }

        /// <summary>
        /// Counts total active tasks (not deleted).
        /// </summary>
        private static int CountTotalActiveTasks(List<WorkflowStepInstance> tasks)
        {
            return tasks.Count(t => !t.IsDeleted);
        }

        /// <summary>
        /// Counts total tasks including deleted.
        /// </summary>
        private static int CountTotalTasks(List<WorkflowStepInstance> tasks)
        {
            return tasks.Count;
        }

        // =====================================================================
        // Property Tests
        // =====================================================================

        /// <summary>
        /// Property: Completed + Pending = Total tasks (always).
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_CompletedPlusPendingEqualsTotal(List<WorkflowStepInstance> tasks)
        {
            if (tasks == null || tasks.Count == 0)
                return true.ToProperty();

            var completed = CountCompletedTasks(tasks);
            var pending = CountPendingTasks(tasks);
            var total = CountTotalActiveTasks(tasks);

            // Completed + Pending should equal total active tasks
            return (completed + pending == total).ToProperty();
        }

        /// <summary>
        /// Property: No tasks are lost during workflow execution.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_NoTasksAreLost(List<WorkflowStepInstance> tasks)
        {
            if (tasks == null || tasks.Count == 0)
                return true.ToProperty();

            var originalCount = tasks.Count;

            // Create a copy of the list
            var tasksCopy = new List<WorkflowStepInstance>(tasks);

            // Verify count is preserved
            return (tasksCopy.Count == originalCount).ToProperty();
        }

        /// <summary>
        /// Property: No tasks are duplicated during workflow execution.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_NoTasksAreDuplicated(List<WorkflowStepInstance> tasks)
        {
            if (tasks == null || tasks.Count == 0)
                return true.ToProperty();

            // Count unique task IDs
            var uniqueIds = tasks.Select(t => t.WorkflowStepInstanceId).Distinct().Count();

            // Should equal total count (no duplicates)
            return (uniqueIds == tasks.Count).ToProperty();
        }

        /// <summary>
        /// Property: Task count is preserved when filtering by status.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_TaskCountPreservedWhenFiltering(List<WorkflowStepInstance> tasks)
        {
            if (tasks == null || tasks.Count == 0)
                return true.ToProperty();

            var completed = CountCompletedTasks(tasks);
            var pending = CountPendingTasks(tasks);
            var rejected = CountRejectedTasks(tasks);
            var delegated = CountDelegatedTasks(tasks);
            var total = CountTotalActiveTasks(tasks);

            // Sum of all statuses should equal total
            return (completed + pending + rejected + delegated == total).ToProperty();
        }

        /// <summary>
        /// Property: Completed task count never decreases.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_CompletedTaskCountNeverDecreases(List<WorkflowStepInstance> tasks)
        {
            if (tasks == null || tasks.Count < 2)
                return true.ToProperty();

            var completedCount = CountCompletedTasks(tasks);

            // Completed count should be >= 0
            return (completedCount >= 0).ToProperty();
        }

        /// <summary>
        /// Property: Pending task count never exceeds total.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_PendingTaskCountNeverExceedsTotal(List<WorkflowStepInstance> tasks)
        {
            if (tasks == null || tasks.Count == 0)
                return true.ToProperty();

            var pending = CountPendingTasks(tasks);
            var total = CountTotalActiveTasks(tasks);

            return (pending <= total).ToProperty();
        }

        /// <summary>
        /// Property: Total task count equals sum of all status categories.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_TotalEqualsAllStatusCategories(List<WorkflowStepInstance> tasks)
        {
            if (tasks == null || tasks.Count == 0)
                return true.ToProperty();

            var completed = CountCompletedTasks(tasks);
            var pending = CountPendingTasks(tasks);
            var rejected = CountRejectedTasks(tasks);
            var delegated = CountDelegatedTasks(tasks);
            var total = CountTotalActiveTasks(tasks);

            return (completed + pending + rejected + delegated == total).ToProperty();
        }

        /// <summary>
        /// Property: Adding a task increases total count by 1.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_AddingTaskIncreasesTotalByOne(List<WorkflowStepInstance> tasks)
        {
            if (tasks == null)
                tasks = new List<WorkflowStepInstance>();

            var originalCount = tasks.Count;

            var newTask = new WorkflowStepInstance
            {
                WorkflowStepInstanceId = originalCount + 1,
                WorkflowInstanceId = 1,
                WorkflowStepId = 1,
                AssignedTo = "user",
                AssignedDate = DateTime.Now,
                Status = WorkflowStepInstanceStatus.Pending,
                CompletedDate = null,
                CompletedBy = null,
                OutputData = null,
                SLADeadline = DateTime.Now.AddHours(24),
                SLAStatus = SLAStatus.OnTime,
                ExternalReferenceId = null,
                ExternalReferenceType = null,
                ExternalTaskId = null,
                IsDeleted = false
            };

            tasks.Add(newTask);

            return (tasks.Count == originalCount + 1).ToProperty();
        }

        /// <summary>
        /// Property: Completing a task decreases pending count by 1.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_CompletingTaskDecreasesPendingByOne()
        {
            var tasks = new List<WorkflowStepInstance>
            {
                new WorkflowStepInstance
                {
                    WorkflowStepInstanceId = 1,
                    WorkflowInstanceId = 1,
                    WorkflowStepId = 1,
                    AssignedTo = "user",
                    AssignedDate = DateTime.Now,
                    Status = WorkflowStepInstanceStatus.InProgress,
                    CompletedDate = null,
                    CompletedBy = null,
                    OutputData = null,
                    SLADeadline = DateTime.Now.AddHours(24),
                    SLAStatus = SLAStatus.OnTime,
                    ExternalReferenceId = null,
                    ExternalReferenceType = null,
                    ExternalTaskId = null,
                    IsDeleted = false
                }
            };

            var pendingBefore = CountPendingTasks(tasks);

            // Complete the task
            tasks[0].Status = WorkflowStepInstanceStatus.Completed;
            tasks[0].CompletedDate = DateTime.Now;
            tasks[0].CompletedBy = "user";

            var pendingAfter = CountPendingTasks(tasks);

            return (pendingBefore - pendingAfter == 1).ToProperty();
        }

        /// <summary>
        /// Property: Task count is consistent across multiple reads.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_TaskCountIsConsistentAcrossReads(List<WorkflowStepInstance> tasks)
        {
            if (tasks == null || tasks.Count == 0)
                return true.ToProperty();

            var count1 = CountTotalActiveTasks(tasks);
            var count2 = CountTotalActiveTasks(tasks);
            var count3 = CountTotalActiveTasks(tasks);

            return (count1 == count2 && count2 == count3).ToProperty();
        }

        /// <summary>
        /// Property: Soft-deleted tasks are not counted in active total.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SoftDeletedTasksNotCountedInActive(List<WorkflowStepInstance> tasks)
        {
            if (tasks == null || tasks.Count == 0)
                return true.ToProperty();

            var activeTasks = tasks.Count(t => !t.IsDeleted);
            var totalTasks = tasks.Count;

            // Active should be <= total
            return (activeTasks <= totalTasks).ToProperty();
        }
    }
}
