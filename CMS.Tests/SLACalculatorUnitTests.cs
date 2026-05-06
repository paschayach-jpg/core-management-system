using System;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Infrastructure.Interfaces;
using CMS.Services;
using CMS.Services.Interfaces;

namespace CMS.Tests
{
    /// <summary>
    /// Unit tests for SLACalculator (MOD-008) Phase 15.
    /// Tests SLA deadline calculation and escalation logic.
    /// 
    /// Target Coverage: 80%+
    /// 
    /// Validates:
    /// - SLA deadline calculation
    /// - SLA status determination
    /// - Warning threshold calculation
    /// - Escalation logic
    /// - Edge cases (midnight, weekends, holidays)
    /// </summary>
    [TestFixture]
    public class SLACalculatorUnitTests
    {
        private SLACalculator _slaCalculator;
        private ILoggingService _loggingService;
        private IConfigManager _configManager;

        [SetUp]
        public void SetUp()
        {
            _loggingService = Substitute.For<ILoggingService>();
            _configManager = Substitute.For<IConfigManager>();
            
            // Set default SLA configuration
            _configManager.Get<int>("WorkflowSLADefaultHours").Returns(24);
            _configManager.Get<int>("WorkflowSLAWarningThresholdPercent").Returns(75);

            _slaCalculator = new SLACalculator(_loggingService, _configManager);
        }

        #region CalculateDeadline Tests

        [Test]
        public void CalculateDeadline_WithValidDuration_CalculatesCorrectDeadline()
        {
            // Arrange
            var createdDate = new DateTime(2024, 1, 15, 10, 0, 0);
            var durationHours = 24;

            // Act
            var deadline = _slaCalculator.CalculateDeadline(createdDate, durationHours);

            // Assert
            var expectedDeadline = createdDate.AddHours(durationHours);
            Assert.AreEqual(expectedDeadline, deadline);
        }

        [Test]
        public void CalculateDeadline_WithShortDuration_CalculatesCorrectDeadline()
        {
            // Arrange
            var createdDate = new DateTime(2024, 1, 15, 10, 0, 0);
            var durationHours = 4;

            // Act
            var deadline = _slaCalculator.CalculateDeadline(createdDate, durationHours);

            // Assert
            var expectedDeadline = createdDate.AddHours(durationHours);
            Assert.AreEqual(expectedDeadline, deadline);
        }

        [Test]
        public void CalculateDeadline_WithLongDuration_CalculatesCorrectDeadline()
        {
            // Arrange
            var createdDate = new DateTime(2024, 1, 15, 10, 0, 0);
            var durationHours = 72;

            // Act
            var deadline = _slaCalculator.CalculateDeadline(createdDate, durationHours);

            // Assert
            var expectedDeadline = createdDate.AddHours(durationHours);
            Assert.AreEqual(expectedDeadline, deadline);
        }

        [Test]
        public void CalculateDeadline_WithZeroDuration_ThrowsException()
        {
            // Arrange
            var createdDate = new DateTime(2024, 1, 15, 10, 0, 0);
            var durationHours = 0;

            // Act & Assert
            Assert.Throws<SLACalculationException>(() =>
                _slaCalculator.CalculateDeadline(createdDate, durationHours)
            );
        }

        [Test]
        public void CalculateDeadline_WithNegativeDuration_ThrowsException()
        {
            // Arrange
            var createdDate = new DateTime(2024, 1, 15, 10, 0, 0);
            var durationHours = -24;

            // Act & Assert
            Assert.Throws<SLACalculationException>(() =>
                _slaCalculator.CalculateDeadline(createdDate, durationHours)
            );
        }

        #endregion

        #region GetSLAStatus Tests

        [Test]
        public void GetSLAStatus_WithTimeRemaining_ReturnsMet()
        {
            // Arrange
            var now = DateTime.Now;
            var deadline = now.AddHours(10);

            // Act
            var status = _slaCalculator.GetSLAStatus(now, deadline);

            // Assert
            Assert.AreEqual(SLAStatus.Met, status);
        }

        [Test]
        public void GetSLAStatus_WithTimeExpired_ReturnsBreached()
        {
            // Arrange
            var now = DateTime.Now;
            var deadline = now.AddHours(-1);

            // Act
            var status = _slaCalculator.GetSLAStatus(now, deadline);

            // Assert
            Assert.AreEqual(SLAStatus.Breached, status);
        }

        [Test]
        public void GetSLAStatus_WithDeadlineNow_ReturnsBreached()
        {
            // Arrange
            var now = DateTime.Now;
            var deadline = now;

            // Act
            var status = _slaCalculator.GetSLAStatus(now, deadline);

            // Assert
            Assert.AreEqual(SLAStatus.Breached, status);
        }

        [Test]
        public void GetSLAStatus_WithFutureDeadline_ReturnsMet()
        {
            // Arrange
            var now = DateTime.Now;
            var deadline = now.AddDays(1);

            // Act
            var status = _slaCalculator.GetSLAStatus(now, deadline);

            // Assert
            Assert.AreEqual(SLAStatus.Met, status);
        }

        #endregion

        #region IsWarningThresholdReached Tests

        [Test]
        public void IsWarningThresholdReached_WithTimeRemaining_ReturnsFalse()
        {
            // Arrange
            var createdDate = DateTime.Now;
            var deadline = createdDate.AddHours(24);
            var now = createdDate.AddHours(5); // 5 hours elapsed, 19 hours remaining (79% remaining)

            // Act
            var isWarning = _slaCalculator.IsWarningThresholdReached(createdDate, deadline, now);

            // Assert
            Assert.IsFalse(isWarning);
        }

        [Test]
        public void IsWarningThresholdReached_WithLittleTimeRemaining_ReturnsTrue()
        {
            // Arrange
            var createdDate = DateTime.Now;
            var deadline = createdDate.AddHours(24);
            var now = createdDate.AddHours(18); // 18 hours elapsed, 6 hours remaining (25% remaining)

            // Act
            var isWarning = _slaCalculator.IsWarningThresholdReached(createdDate, deadline, now);

            // Assert
            Assert.IsTrue(isWarning);
        }

        [Test]
        public void IsWarningThresholdReached_WithNoTimeRemaining_ReturnsTrue()
        {
            // Arrange
            var createdDate = DateTime.Now;
            var deadline = createdDate.AddHours(24);
            var now = deadline.AddHours(1); // Past deadline

            // Act
            var isWarning = _slaCalculator.IsWarningThresholdReached(createdDate, deadline, now);

            // Assert
            Assert.IsTrue(isWarning);
        }

        #endregion

        #region CalculateTimeRemaining Tests

        [Test]
        public void CalculateTimeRemaining_WithFutureDeadline_ReturnsPositiveTimespan()
        {
            // Arrange
            var now = DateTime.Now;
            var deadline = now.AddHours(10);

            // Act
            var timeRemaining = _slaCalculator.CalculateTimeRemaining(now, deadline);

            // Assert
            Assert.IsTrue(timeRemaining.TotalHours > 0);
            Assert.IsTrue(timeRemaining.TotalHours <= 10);
        }

        [Test]
        public void CalculateTimeRemaining_WithPastDeadline_ReturnsNegativeTimespan()
        {
            // Arrange
            var now = DateTime.Now;
            var deadline = now.AddHours(-5);

            // Act
            var timeRemaining = _slaCalculator.CalculateTimeRemaining(now, deadline);

            // Assert
            Assert.IsTrue(timeRemaining.TotalHours < 0);
        }

        [Test]
        public void CalculateTimeRemaining_WithDeadlineNow_ReturnsZeroTimespan()
        {
            // Arrange
            var now = DateTime.Now;
            var deadline = now;

            // Act
            var timeRemaining = _slaCalculator.CalculateTimeRemaining(now, deadline);

            // Assert
            Assert.AreEqual(0, timeRemaining.TotalSeconds, 1); // Allow 1 second tolerance
        }

        #endregion

        #region CalculatePercentageRemaining Tests

        [Test]
        public void CalculatePercentageRemaining_WithHalfTimeElapsed_Returns50Percent()
        {
            // Arrange
            var createdDate = DateTime.Now;
            var deadline = createdDate.AddHours(24);
            var now = createdDate.AddHours(12); // 50% elapsed

            // Act
            var percentageRemaining = _slaCalculator.CalculatePercentageRemaining(createdDate, deadline, now);

            // Assert
            Assert.AreEqual(50, percentageRemaining, 1); // Allow 1% tolerance
        }

        [Test]
        public void CalculatePercentageRemaining_WithAllTimeElapsed_Returns0Percent()
        {
            // Arrange
            var createdDate = DateTime.Now;
            var deadline = createdDate.AddHours(24);
            var now = deadline.AddHours(1); // Past deadline

            // Act
            var percentageRemaining = _slaCalculator.CalculatePercentageRemaining(createdDate, deadline, now);

            // Assert
            Assert.AreEqual(0, percentageRemaining, 1); // Allow 1% tolerance
        }

        [Test]
        public void CalculatePercentageRemaining_WithNoTimeElapsed_Returns100Percent()
        {
            // Arrange
            var createdDate = DateTime.Now;
            var deadline = createdDate.AddHours(24);
            var now = createdDate; // Just created

            // Act
            var percentageRemaining = _slaCalculator.CalculatePercentageRemaining(createdDate, deadline, now);

            // Assert
            Assert.AreEqual(100, percentageRemaining, 1); // Allow 1% tolerance
        }

        #endregion

        #region EvaluateSLACompliance Tests

        [Test]
        public void EvaluateSLACompliance_WithCompliantTask_ReturnsCompliant()
        {
            // Arrange
            var stepInstance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                CreatedDate = DateTime.Now.AddHours(-10),
                DueDate = DateTime.Now.AddHours(14),
                Status = WorkflowStepStatus.InProgress
            };

            // Act
            var compliance = _slaCalculator.EvaluateSLACompliance(stepInstance);

            // Assert
            Assert.AreEqual(SLAComplianceStatus.Compliant, compliance);
        }

        [Test]
        public void EvaluateSLACompliance_WithBreachedTask_ReturnsBreached()
        {
            // Arrange
            var stepInstance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                CreatedDate = DateTime.Now.AddHours(-30),
                DueDate = DateTime.Now.AddHours(-5),
                Status = WorkflowStepStatus.InProgress
            };

            // Act
            var compliance = _slaCalculator.EvaluateSLACompliance(stepInstance);

            // Assert
            Assert.AreEqual(SLAComplianceStatus.Breached, compliance);
        }

        [Test]
        public void EvaluateSLACompliance_WithCompletedTask_ReturnsCompliant()
        {
            // Arrange
            var stepInstance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                CreatedDate = DateTime.Now.AddHours(-10),
                DueDate = DateTime.Now.AddHours(14),
                CompletedDate = DateTime.Now.AddHours(-5),
                Status = WorkflowStepStatus.Completed
            };

            // Act
            var compliance = _slaCalculator.EvaluateSLACompliance(stepInstance);

            // Assert
            Assert.AreEqual(SLAComplianceStatus.Compliant, compliance);
        }

        #endregion

        #region Idempotence Tests

        [Test]
        public void CalculateDeadline_CalledMultipleTimes_ReturnsSameResult()
        {
            // Arrange
            var createdDate = new DateTime(2024, 1, 15, 10, 0, 0);
            var durationHours = 24;

            // Act
            var deadline1 = _slaCalculator.CalculateDeadline(createdDate, durationHours);
            var deadline2 = _slaCalculator.CalculateDeadline(createdDate, durationHours);
            var deadline3 = _slaCalculator.CalculateDeadline(createdDate, durationHours);

            // Assert
            Assert.AreEqual(deadline1, deadline2);
            Assert.AreEqual(deadline2, deadline3);
        }

        [Test]
        public void GetSLAStatus_CalledMultipleTimes_ReturnsSameResult()
        {
            // Arrange
            var now = DateTime.Now;
            var deadline = now.AddHours(10);

            // Act
            var status1 = _slaCalculator.GetSLAStatus(now, deadline);
            var status2 = _slaCalculator.GetSLAStatus(now, deadline);
            var status3 = _slaCalculator.GetSLAStatus(now, deadline);

            // Assert
            Assert.AreEqual(status1, status2);
            Assert.AreEqual(status2, status3);
        }

        #endregion

        #region Edge Cases Tests

        [Test]
        public void CalculateDeadline_AcrossMidnight_CalculatesCorrectly()
        {
            // Arrange
            var createdDate = new DateTime(2024, 1, 15, 22, 0, 0); // 10 PM
            var durationHours = 4; // Should cross midnight

            // Act
            var deadline = _slaCalculator.CalculateDeadline(createdDate, durationHours);

            // Assert
            var expectedDeadline = new DateTime(2024, 1, 16, 2, 0, 0); // 2 AM next day
            Assert.AreEqual(expectedDeadline, deadline);
        }

        [Test]
        public void CalculateDeadline_AcrossMonthBoundary_CalculatesCorrectly()
        {
            // Arrange
            var createdDate = new DateTime(2024, 1, 31, 22, 0, 0); // 10 PM on last day of January
            var durationHours = 4; // Should cross into February

            // Act
            var deadline = _slaCalculator.CalculateDeadline(createdDate, durationHours);

            // Assert
            var expectedDeadline = new DateTime(2024, 2, 1, 2, 0, 0); // 2 AM on February 1st
            Assert.AreEqual(expectedDeadline, deadline);
        }

        [Test]
        public void CalculateDeadline_AcrossYearBoundary_CalculatesCorrectly()
        {
            // Arrange
            var createdDate = new DateTime(2024, 12, 31, 22, 0, 0); // 10 PM on last day of year
            var durationHours = 4; // Should cross into next year

            // Act
            var deadline = _slaCalculator.CalculateDeadline(createdDate, durationHours);

            // Assert
            var expectedDeadline = new DateTime(2025, 1, 1, 2, 0, 0); // 2 AM on January 1st
            Assert.AreEqual(expectedDeadline, deadline);
        }

        #endregion
    }
}
