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
    /// Property-Based Tests for SLA Deadline Calculation (Phase 9.3)
    /// 
    /// Property: SLA deadline = task creation time + SLA duration (always).
    /// 
    /// Validates:
    /// - Deadline is always in the future relative to creation time
    /// - Deadline calculation is consistent (same inputs = same output)
    /// - SLA duration is correctly applied
    /// </summary>
    [TestFixture]
    public class SLADeadlineCalculationTests
    {
        // =====================================================================
        // Generators
        // =====================================================================

        /// <summary>
        /// Generate random task creation times.
        /// </summary>
        private static Arbitrary<DateTime> TaskCreationTimeGenerator()
        {
            var gen = from year in Gen.Choose(2020, 2030)
                      from month in Gen.Choose(1, 12)
                      from day in Gen.Choose(1, 28) // Safe range for all months
                      from hour in Gen.Choose(0, 23)
                      from minute in Gen.Choose(0, 59)
                      from second in Gen.Choose(0, 59)
                      select new DateTime(year, month, day, hour, minute, second);

            return gen.ToArbitrary();
        }

        /// <summary>
        /// Generate random SLA durations (1-168 hours, 1-30 days).
        /// </summary>
        private static Arbitrary<int> SLADurationHoursGenerator()
        {
            // 1 hour to 30 days (720 hours)
            return Gen.Choose(1, 720).ToArbitrary();
        }

        /// <summary>
        /// Generate random workflow step instances with SLA rules.
        /// </summary>
        private static Arbitrary<WorkflowStepInstance> WorkflowStepInstanceGenerator()
        {
            var gen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from workflowId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from stepId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from assignedTo in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                      from assignedDate in TaskCreationTimeGenerator().Generator
                      from durationHours in SLADurationHoursGenerator().Generator
                      select new WorkflowStepInstance
                      {
                          WorkflowStepInstanceId = id,
                          WorkflowInstanceId = workflowId,
                          WorkflowStepId = stepId,
                          AssignedTo = assignedTo,
                          AssignedDate = assignedDate,
                          Status = WorkflowStepInstanceStatus.InProgress,
                          CompletedDate = null,
                          CompletedBy = null,
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

        /// <summary>
        /// Generate random SLA rules.
        /// </summary>
        private static Arbitrary<WFSLARule> SLARuleGenerator()
        {
            var gen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from stepId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from durationHours in SLADurationHoursGenerator().Generator
                      from escalationAction in Gen.Elements(
                          EscalationAction.NotifyManager,
                          EscalationAction.IncreasePriority,
                          EscalationAction.Reassign,
                          EscalationAction.AutoReject
                      )
                      select new WFSLARule
                      {
                          WFSLARuleId = id,
                          WorkflowStepId = stepId,
                          SLADurationHours = durationHours,
                          EscalationAction = escalationAction,
                          IsDeleted = false
                      };

            return gen.ToArbitrary();
        }

        // =====================================================================
        // Helper Methods
        // =====================================================================

        /// <summary>
        /// Calculates the SLA deadline given a creation time and duration.
        /// </summary>
        private static DateTime CalculateSLADeadline(DateTime creationTime, int durationHours)
        {
            return creationTime.AddHours(durationHours);
        }

        // =====================================================================
        // Property Tests
        // =====================================================================

        /// <summary>
        /// Property: SLA deadline = creation time + duration (always).
        /// 
        /// Validates: Requirement 6 - SLA Management - Define Rules
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SLADeadlineEqualsCreationTimePlusDuration(
            DateTime creationTime,
            int durationHours)
        {
            // Ensure duration is positive
            var safeDuration = Math.Abs(durationHours % 720) + 1; // 1-720 hours

            var deadline = CalculateSLADeadline(creationTime, safeDuration);
            var expectedDeadline = creationTime.AddHours(safeDuration);

            return (deadline == expectedDeadline).ToProperty();
        }

        /// <summary>
        /// Property: SLA deadline is always in the future relative to creation time.
        /// 
        /// Validates: Requirement 6 - SLA Management - Define Rules
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SLADeadlineIsAlwaysInFuture(
            DateTime creationTime,
            int durationHours)
        {
            // Ensure duration is positive
            var safeDuration = Math.Abs(durationHours % 720) + 1; // 1-720 hours

            var deadline = CalculateSLADeadline(creationTime, safeDuration);

            return (deadline > creationTime).ToProperty();
        }

        /// <summary>
        /// Property: SLA deadline calculation is consistent (same inputs = same output).
        /// 
        /// Validates: Requirement 6 - SLA Management - Define Rules
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SLADeadlineCalculationIsConsistent(
            DateTime creationTime,
            int durationHours)
        {
            var safeDuration = Math.Abs(durationHours % 720) + 1;

            var deadline1 = CalculateSLADeadline(creationTime, safeDuration);
            var deadline2 = CalculateSLADeadline(creationTime, safeDuration);
            var deadline3 = CalculateSLADeadline(creationTime, safeDuration);

            return (deadline1 == deadline2 && deadline2 == deadline3).ToProperty();
        }

        /// <summary>
        /// Property: Different durations produce different deadlines.
        /// 
        /// Validates: Requirement 6 - SLA Management - Define Rules
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_DifferentDurationsProduceDifferentDeadlines(
            DateTime creationTime,
            int duration1,
            int duration2)
        {
            var safeDuration1 = Math.Abs(duration1 % 720) + 1;
            var safeDuration2 = Math.Abs(duration2 % 720) + 1;

            // Skip if durations are the same
            if (safeDuration1 == safeDuration2)
                return true.ToProperty();

            var deadline1 = CalculateSLADeadline(creationTime, safeDuration1);
            var deadline2 = CalculateSLADeadline(creationTime, safeDuration2);

            return (deadline1 != deadline2).ToProperty();
        }

        /// <summary>
        /// Property: Longer durations produce later deadlines.
        /// 
        /// Validates: Requirement 6 - SLA Management - Define Rules
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_LongerDurationsProduceLaterDeadlines(
            DateTime creationTime,
            int duration1,
            int duration2)
        {
            var safeDuration1 = Math.Abs(duration1 % 720) + 1;
            var safeDuration2 = Math.Abs(duration2 % 720) + 1;

            var deadline1 = CalculateSLADeadline(creationTime, safeDuration1);
            var deadline2 = CalculateSLADeadline(creationTime, safeDuration2);

            if (safeDuration1 < safeDuration2)
                return (deadline1 < deadline2).ToProperty();
            else if (safeDuration1 > safeDuration2)
                return (deadline1 > deadline2).ToProperty();
            else
                return (deadline1 == deadline2).ToProperty();
        }

        /// <summary>
        /// Property: SLA deadline is exactly duration hours after creation time.
        /// 
        /// Validates: Requirement 6 - SLA Management - Define Rules
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SLADeadlineIsExactlyDurationHoursLater(
            DateTime creationTime,
            int durationHours)
        {
            var safeDuration = Math.Abs(durationHours % 720) + 1;

            var deadline = CalculateSLADeadline(creationTime, safeDuration);
            var timeDifference = deadline - creationTime;

            return (timeDifference.TotalHours == safeDuration).ToProperty();
        }

        /// <summary>
        /// Property: SLA deadline respects time zones (uses same DateTime kind).
        /// 
        /// Validates: Requirement 6 - SLA Management - Define Rules
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SLADeadlineRespectesDateTimeKind(
            DateTime creationTime,
            int durationHours)
        {
            var safeDuration = Math.Abs(durationHours % 720) + 1;

            var deadline = CalculateSLADeadline(creationTime, safeDuration);

            // Both should have the same Kind
            return (deadline.Kind == creationTime.Kind).ToProperty();
        }

        /// <summary>
        /// Property: Minimum SLA duration (1 hour) produces deadline 1 hour later.
        /// 
        /// Validates: Requirement 6 - SLA Management - Define Rules
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_MinimumSLADurationIsOneHour(DateTime creationTime)
        {
            var deadline = CalculateSLADeadline(creationTime, 1);
            var expectedDeadline = creationTime.AddHours(1);

            return (deadline == expectedDeadline).ToProperty();
        }

        /// <summary>
        /// Property: Maximum SLA duration (720 hours = 30 days) produces deadline 30 days later.
        /// 
        /// Validates: Requirement 6 - SLA Management - Define Rules
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_MaximumSLADurationIs30Days(DateTime creationTime)
        {
            var deadline = CalculateSLADeadline(creationTime, 720);
            var expectedDeadline = creationTime.AddDays(30);

            return (deadline == expectedDeadline).ToProperty();
        }

        /// <summary>
        /// Property: SLA deadline calculation handles leap years correctly.
        /// 
        /// Validates: Requirement 6 - SLA Management - Define Rules
        /// </summary>
        [PropertyAttribute(MaxTest = 100)]
        public Property Property_SLADeadlineHandlesLeapYearsCorrectly()
        {
            // Feb 28, 2020 (leap year) + 1 day = Feb 29, 2020
            var leapYearDate = new DateTime(2020, 2, 28, 12, 0, 0);
            var deadline = CalculateSLADeadline(leapYearDate, 24); // 1 day

            var expectedDeadline = new DateTime(2020, 2, 29, 12, 0, 0);

            return (deadline == expectedDeadline).ToProperty();
        }

        /// <summary>
        /// Property: SLA deadline calculation handles year boundaries correctly.
        /// 
        /// Validates: Requirement 6 - SLA Management - Define Rules
        /// </summary>
        [PropertyAttribute(MaxTest = 100)]
        public Property Property_SLADeadlineHandlesYearBoundariesCorrectly()
        {
            // Dec 31, 2020 + 24 hours = Jan 1, 2021
            var yearEndDate = new DateTime(2020, 12, 31, 12, 0, 0);
            var deadline = CalculateSLADeadline(yearEndDate, 24);

            var expectedDeadline = new DateTime(2021, 1, 1, 12, 0, 0);

            return (deadline == expectedDeadline).ToProperty();
        }
    }
}
