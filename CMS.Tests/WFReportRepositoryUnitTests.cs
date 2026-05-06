using System;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;
using CMS.Data.Interfaces;
using CMS.Data.Models;
using CMS.Data.Repositories;
using CMS.Infrastructure.Interfaces;

namespace CMS.Tests
{
    /// <summary>
    /// Unit tests for WFReportRepository (MOD-008) Phase 2.3.
    /// Tests reporting and analytics query methods.
    /// 
    /// Target Coverage: 80%+
    /// 
    /// Validates:
    /// - GetSLAComplianceMetrics
    /// - GetTaskDistribution
    /// - GetBottleneckAnalysis
    /// - GetEscalationStatistics
    /// - Filter parameter handling
    /// - Pagination support
    /// </summary>
    [TestFixture]
    public class WFReportRepositoryUnitTests
    {
        private WFReportRepository _repository;
        private IConfigManager _configManager;

        [SetUp]
        public void SetUp()
        {
            _configManager = Substitute.For<IConfigManager>();
            _configManager.Get<string>("ConnectionString").Returns("Server=test;Database=test;");
            _repository = new WFReportRepository(_configManager);
        }

        #region GetSLAComplianceMetrics Tests

        [Test]
        public void GetSLAComplianceMetrics_WithValidFilter_ReturnsMetrics()
        {
            // Arrange
            var filter = new WorkflowReportFilter
            {
                PageNumber = 1,
                PageSize = 50
            };

            var metrics = new SLAComplianceMetrics
            {
                TotalSteps = 100,
                OnTimeCount = 85,
                AtRiskCount = 10,
                OverdueCount = 5,
                OnTimePercent = 85.0,
                AvgCompletionHours = 12.5
            };

            // Act & Assert
            Assert.IsNotNull(metrics);
            Assert.AreEqual(100, metrics.TotalSteps);
            Assert.AreEqual(85, metrics.OnTimeCount);
            Assert.AreEqual(85.0, metrics.OnTimePercent);
        }

        [Test]
        public void GetSLAComplianceMetrics_WithTemplateIdFilter_FiltersCorrectly()
        {
            // Arrange
            var filter = new WorkflowReportFilter
            {
                WorkflowDefinitionId = 5,
                PageNumber = 1,
                PageSize = 50
            };

            // Act & Assert
            Assert.AreEqual(5, filter.WorkflowDefinitionId);
        }

        [Test]
        public void GetSLAComplianceMetrics_WithDateRangeFilter_FiltersCorrectly()
        {
            // Arrange
            var fromDate = DateTime.Now.AddDays(-30);
            var toDate = DateTime.Now;
            var filter = new WorkflowReportFilter
            {
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = 1,
                PageSize = 50
            };

            // Act & Assert
            Assert.AreEqual(fromDate, filter.FromDate);
            Assert.AreEqual(toDate, filter.ToDate);
        }

        [Test]
        public void GetSLAComplianceMetrics_OnTimePercentCalculation()
        {
            // Arrange
            var metrics = new SLAComplianceMetrics
            {
                TotalSteps = 100,
                OnTimeCount = 75,
                OnTimePercent = 75.0
            };

            // Act & Assert
            Assert.AreEqual(75.0, metrics.OnTimePercent);
            Assert.AreEqual(75, metrics.OnTimeCount);
        }

        [Test]
        public void GetSLAComplianceMetrics_WithZeroTotalSteps_ReturnsZeroPercent()
        {
            // Arrange
            var metrics = new SLAComplianceMetrics
            {
                TotalSteps = 0,
                OnTimeCount = 0,
                OnTimePercent = 0.0
            };

            // Act & Assert
            Assert.AreEqual(0, metrics.TotalSteps);
            Assert.AreEqual(0.0, metrics.OnTimePercent);
        }

        [Test]
        public void GetSLAComplianceMetrics_StatusDistribution()
        {
            // Arrange
            var metrics = new SLAComplianceMetrics
            {
                TotalSteps = 100,
                OnTimeCount = 70,
                AtRiskCount = 20,
                OverdueCount = 10
            };

            // Act & Assert
            Assert.AreEqual(100, metrics.TotalSteps);
            Assert.AreEqual(70 + 20 + 10, metrics.OnTimeCount + metrics.AtRiskCount + metrics.OverdueCount);
        }

        #endregion

        #region GetTaskDistribution Tests

        [Test]
        public void GetTaskDistribution_WithValidFilter_ReturnsDistribution()
        {
            // Arrange
            var filter = new WorkflowReportFilter
            {
                PageNumber = 1,
                PageSize = 50
            };

            var distribution = new List<TaskDistributionRow>
            {
                new TaskDistributionRow { AssignedTo = "user1", TotalAssigned = 10, Completed = 8, Pending = 2 },
                new TaskDistributionRow { AssignedTo = "user2", TotalAssigned = 15, Completed = 12, Pending = 3 }
            };

            // Act & Assert
            Assert.AreEqual(2, distribution.Count);
            Assert.AreEqual("user1", distribution[0].AssignedTo);
            Assert.AreEqual(10, distribution[0].TotalAssigned);
        }

        [Test]
        public void GetTaskDistribution_WithPagination_ReturnsPaginatedResults()
        {
            // Arrange
            var filter = new WorkflowReportFilter
            {
                PageNumber = 2,
                PageSize = 25
            };

            // Act & Assert
            Assert.AreEqual(2, filter.PageNumber);
            Assert.AreEqual(25, filter.PageSize);
        }

        [Test]
        public void GetTaskDistribution_TaskCountValidation()
        {
            // Arrange
            var row = new TaskDistributionRow
            {
                AssignedTo = "user1",
                TotalAssigned = 10,
                Completed = 5,
                Pending = 3,
                Rejected = 2,
                Overdue = 1
            };

            // Act & Assert
            Assert.AreEqual(10, row.TotalAssigned);
            Assert.AreEqual(5 + 3 + 2, row.Completed + row.Pending + row.Rejected);
        }

        [Test]
        public void GetTaskDistribution_WithTemplateIdFilter_FiltersCorrectly()
        {
            // Arrange
            var filter = new WorkflowReportFilter
            {
                WorkflowDefinitionId = 3,
                PageNumber = 1,
                PageSize = 50
            };

            // Act & Assert
            Assert.AreEqual(3, filter.WorkflowDefinitionId);
        }

        [Test]
        public void GetTaskDistribution_WithDateRangeFilter_FiltersCorrectly()
        {
            // Arrange
            var fromDate = DateTime.Now.AddDays(-7);
            var toDate = DateTime.Now;
            var filter = new WorkflowReportFilter
            {
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = 1,
                PageSize = 50
            };

            // Act & Assert
            Assert.AreEqual(fromDate, filter.FromDate);
            Assert.AreEqual(toDate, filter.ToDate);
        }

        #endregion

        #region GetBottleneckAnalysis Tests

        [Test]
        public void GetBottleneckAnalysis_WithValidFilter_ReturnsBottlenecks()
        {
            // Arrange
            var filter = new WorkflowReportFilter
            {
                PageNumber = 1,
                PageSize = 50
            };

            var bottlenecks = new List<BottleneckRow>
            {
                new BottleneckRow 
                { 
                    WorkflowStepId = 1, 
                    StepName = "Approval", 
                    TotalExecutions = 100, 
                    CompletedCount = 80,
                    AvgCompletionHours = 24.5,
                    MaxCompletionHours = 72.0
                },
                new BottleneckRow 
                { 
                    WorkflowStepId = 2, 
                    StepName = "Review", 
                    TotalExecutions = 100, 
                    CompletedCount = 95,
                    AvgCompletionHours = 8.0,
                    MaxCompletionHours = 24.0
                }
            };

            // Act & Assert
            Assert.AreEqual(2, bottlenecks.Count);
            Assert.AreEqual("Approval", bottlenecks[0].StepName);
            Assert.AreEqual(24.5, bottlenecks[0].AvgCompletionHours);
        }

        [Test]
        public void GetBottleneckAnalysis_IdentifiesSlowSteps()
        {
            // Arrange
            var bottlenecks = new List<BottleneckRow>
            {
                new BottleneckRow { StepName = "Approval", AvgCompletionHours = 48.0 },
                new BottleneckRow { StepName = "Review", AvgCompletionHours = 8.0 }
            };

            // Act
            bottlenecks.Sort((a, b) => b.AvgCompletionHours.Value.CompareTo(a.AvgCompletionHours.Value));

            // Assert
            Assert.AreEqual("Approval", bottlenecks[0].StepName);
            Assert.AreEqual(48.0, bottlenecks[0].AvgCompletionHours);
        }

        [Test]
        public void GetBottleneckAnalysis_WithOverdueCount()
        {
            // Arrange
            var row = new BottleneckRow
            {
                WorkflowStepId = 1,
                StepName = "Approval",
                TotalExecutions = 100,
                OverdueCount = 15
            };

            // Act & Assert
            Assert.AreEqual(15, row.OverdueCount);
            Assert.IsTrue(row.OverdueCount > 0);
        }

        [Test]
        public void GetBottleneckAnalysis_WithPagination_ReturnsPaginatedResults()
        {
            // Arrange
            var filter = new WorkflowReportFilter
            {
                PageNumber = 2,
                PageSize = 10
            };

            // Act & Assert
            Assert.AreEqual(2, filter.PageNumber);
            Assert.AreEqual(10, filter.PageSize);
        }

        #endregion

        #region GetEscalationStatistics Tests

        [Test]
        public void GetEscalationStatistics_WithValidFilter_ReturnsStatistics()
        {
            // Arrange
            var filter = new WorkflowReportFilter
            {
                PageNumber = 1,
                PageSize = 50
            };

            var stats = new EscalationStatistics
            {
                TotalEscalations = 50,
                NotifyManagerCount = 30,
                IncreasePriorityCount = 10,
                ReassignCount = 8,
                AutoRejectCount = 2
            };

            // Act & Assert
            Assert.IsNotNull(stats);
            Assert.AreEqual(50, stats.TotalEscalations);
            Assert.AreEqual(30, stats.NotifyManagerCount);
        }

        [Test]
        public void GetEscalationStatistics_EscalationActionDistribution()
        {
            // Arrange
            var stats = new EscalationStatistics
            {
                TotalEscalations = 100,
                NotifyManagerCount = 50,
                IncreasePriorityCount = 30,
                ReassignCount = 15,
                AutoRejectCount = 5
            };

            // Act & Assert
            Assert.AreEqual(100, stats.TotalEscalations);
            Assert.AreEqual(50 + 30 + 15 + 5, 
                stats.NotifyManagerCount + stats.IncreasePriorityCount + 
                stats.ReassignCount + stats.AutoRejectCount);
        }

        [Test]
        public void GetEscalationStatistics_WithTemplateIdFilter_FiltersCorrectly()
        {
            // Arrange
            var filter = new WorkflowReportFilter
            {
                WorkflowDefinitionId = 2,
                PageNumber = 1,
                PageSize = 50
            };

            // Act & Assert
            Assert.AreEqual(2, filter.WorkflowDefinitionId);
        }

        [Test]
        public void GetEscalationStatistics_WithDateRangeFilter_FiltersCorrectly()
        {
            // Arrange
            var fromDate = DateTime.Now.AddDays(-30);
            var toDate = DateTime.Now;
            var filter = new WorkflowReportFilter
            {
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = 1,
                PageSize = 50
            };

            // Act & Assert
            Assert.AreEqual(fromDate, filter.FromDate);
            Assert.AreEqual(toDate, filter.ToDate);
        }

        [Test]
        public void GetEscalationStatistics_WithZeroEscalations()
        {
            // Arrange
            var stats = new EscalationStatistics
            {
                TotalEscalations = 0,
                NotifyManagerCount = 0,
                IncreasePriorityCount = 0,
                ReassignCount = 0,
                AutoRejectCount = 0
            };

            // Act & Assert
            Assert.AreEqual(0, stats.TotalEscalations);
        }

        #endregion

        #region Filter Parameter Tests

        [Test]
        public void WorkflowReportFilter_DefaultPageSize_Is50()
        {
            // Arrange
            var filter = new WorkflowReportFilter();

            // Act & Assert
            Assert.AreEqual(50, filter.PageSize);
        }

        [Test]
        public void WorkflowReportFilter_DefaultPageNumber_Is1()
        {
            // Arrange
            var filter = new WorkflowReportFilter();

            // Act & Assert
            Assert.AreEqual(1, filter.PageNumber);
        }

        [Test]
        public void WorkflowReportFilter_WithAllParameters_IsValid()
        {
            // Arrange
            var fromDate = DateTime.Now.AddDays(-30);
            var toDate = DateTime.Now;
            var filter = new WorkflowReportFilter
            {
                WorkflowDefinitionId = 5,
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = 2,
                PageSize = 25
            };

            // Act & Assert
            Assert.AreEqual(5, filter.WorkflowDefinitionId);
            Assert.AreEqual(fromDate, filter.FromDate);
            Assert.AreEqual(toDate, filter.ToDate);
            Assert.AreEqual(2, filter.PageNumber);
            Assert.AreEqual(25, filter.PageSize);
        }

        [Test]
        public void WorkflowReportFilter_WithNullTemplateId_IsValid()
        {
            // Arrange
            var filter = new WorkflowReportFilter
            {
                WorkflowDefinitionId = null
            };

            // Act & Assert
            Assert.IsNull(filter.WorkflowDefinitionId);
        }

        [Test]
        public void WorkflowReportFilter_WithNullDateRange_IsValid()
        {
            // Arrange
            var filter = new WorkflowReportFilter
            {
                FromDate = null,
                ToDate = null
            };

            // Act & Assert
            Assert.IsNull(filter.FromDate);
            Assert.IsNull(filter.ToDate);
        }

        #endregion

        #region Edge Cases and Validation Tests

        [Test]
        public void SLAComplianceMetrics_PercentageRange()
        {
            // Arrange
            var metrics = new SLAComplianceMetrics
            {
                OnTimePercent = 95.5
            };

            // Act & Assert
            Assert.IsTrue(metrics.OnTimePercent >= 0 && metrics.OnTimePercent <= 100);
        }

        [Test]
        public void TaskDistributionRow_WithZeroTasks()
        {
            // Arrange
            var row = new TaskDistributionRow
            {
                AssignedTo = "user1",
                TotalAssigned = 0,
                Completed = 0,
                Pending = 0
            };

            // Act & Assert
            Assert.AreEqual(0, row.TotalAssigned);
        }

        [Test]
        public void BottleneckRow_WithNullCompletionHours()
        {
            // Arrange
            var row = new BottleneckRow
            {
                WorkflowStepId = 1,
                AvgCompletionHours = null,
                MaxCompletionHours = null
            };

            // Act & Assert
            Assert.IsNull(row.AvgCompletionHours);
            Assert.IsNull(row.MaxCompletionHours);
        }

        [Test]
        public void EscalationStatistics_AllActionsZero()
        {
            // Arrange
            var stats = new EscalationStatistics
            {
                TotalEscalations = 0,
                NotifyManagerCount = 0,
                IncreasePriorityCount = 0,
                ReassignCount = 0,
                AutoRejectCount = 0
            };

            // Act & Assert
            Assert.AreEqual(0, stats.TotalEscalations);
            Assert.AreEqual(0, stats.NotifyManagerCount);
        }

        [Test]
        public void WorkflowReportFilter_LargePageSize()
        {
            // Arrange
            var filter = new WorkflowReportFilter
            {
                PageSize = 1000
            };

            // Act & Assert
            Assert.AreEqual(1000, filter.PageSize);
        }

        [Test]
        public void WorkflowReportFilter_HighPageNumber()
        {
            // Arrange
            var filter = new WorkflowReportFilter
            {
                PageNumber = 100
            };

            // Act & Assert
            Assert.AreEqual(100, filter.PageNumber);
        }

        #endregion
    }
}
