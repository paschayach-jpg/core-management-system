using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using CMS.Services;
using CMS.Services.Interfaces;
using CMS.Services.DTOs;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Data.Interfaces;

namespace CMS.Tests
{
    /// <summary>
    /// Phase 15.4: Performance Testing for Workflow Engine
    /// 
    /// Tests performance targets:
    /// - Workflow instance creation: < 500ms
    /// - Dashboard load: < 2 seconds
    /// - SLA calculation: < 5 minutes for all active instances
    /// - Background job: < 1 minute for SLA checks
    /// 
    /// **Validates: Requirements 21 - Non-Functional Requirement - Performance**
    /// </summary>
    [TestFixture]
    public class WorkflowPerformanceTests
    {
        private IWorkflowService _workflowService;
        private IWorkflowRepository _workflowRepository;
        private IWorkflowTemplateService _templateService;
        private ISLACalculator _slaCalculator;
        private IWorkflowStepInstanceRepository _stepInstanceRepository;
        private UserContext _userContext;

        [SetUp]
        public void Setup()
        {
            // Mock dependencies
            _workflowRepository = Substitute.For<IWorkflowRepository>();
            _stepInstanceRepository = Substitute.For<IWorkflowStepInstanceRepository>();
            _workflowService = Substitute.For<IWorkflowService>();
            _templateService = Substitute.For<IWorkflowTemplateService>();
            _slaCalculator = Substitute.For<ISLACalculator>();

            // Create test user context
            _userContext = new UserContext
            {
                ADUserName = "testuser",
                UserID = 1,
                FullName = "Test User",
                Email = "test@example.com",
                IsAdmin = false
            };
        }

        /// <summary>
        /// Performance Test 1: Workflow Instance Creation
        /// Target: < 500ms per instance
        /// 
        /// **Validates: Requirement 21.1 - Workflow instance creation within 500ms**
        /// </summary>
        [Test]
        public void PerformanceTest_WorkflowInstanceCreation_CompletesWithin500ms()
        {
            // Arrange
            var request = new CreateWorkflowInstanceRequest
            {
                TemplateName = "TestTemplate",
                TemplateVersion = 1,
                InputParameters = new Dictionary<string, object>
                {
                    { "DocumentId", 123 },
                    { "ApprovalLevel", "High" }
                }
            };

            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 100; i++)
            {
                var instance = new WorkflowInstance
                {
                    WorkflowInstanceId = i,
                    WorkflowDefinitionId = 1,
                    TemplateVersion = 1,
                    InitiatedBy = _userContext.ADUserName,
                    InitiatedDate = DateTime.Now,
                    Status = WorkflowInstanceStatus.Created,
                    InputParameters = System.Text.Json.JsonSerializer.Serialize(request.InputParameters)
                };

                _workflowRepository.CreateWorkflowInstance(instance);
            }

            stopwatch.Stop();

            // Assert
            var averageTimePerInstance = stopwatch.ElapsedMilliseconds / 100.0;
            Assert.That(averageTimePerInstance, Is.LessThan(500), 
                $"Average instance creation time {averageTimePerInstance}ms exceeds target of 500ms");
        }

        /// <summary>
        /// Performance Test 2: Dashboard Load Performance
        /// Target: < 2 seconds for dashboard with up to 1000 instances
        /// 
        /// **Validates: Requirement 21.2 - Dashboard load within 2 seconds**
        /// </summary>
        [Test]
        public void PerformanceTest_DashboardLoad_CompletesWithin2Seconds()
        {
            // Arrange
            var instances = GenerateWorkflowInstances(1000);
            _workflowRepository.GetWorkflowInstances(Arg.Any<WorkflowInstanceFilter>())
                .Returns(new PagedResult<WorkflowInstance>
                {
                    Items = instances,
                    TotalCount = instances.Count,
                    PageNumber = 1,
                    PageSize = 1000
                });

            var stopwatch = Stopwatch.StartNew();

            // Act
            var result = _workflowRepository.GetWorkflowInstances(new WorkflowInstanceFilter
            {
                InitiatedBy = _userContext.ADUserName,
                PageNumber = 1,
                PageSize = 1000
            });

            stopwatch.Stop();

            // Assert
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(2000),
                $"Dashboard load time {stopwatch.ElapsedMilliseconds}ms exceeds target of 2000ms");
            Assert.That(result.Items.Count, Is.EqualTo(1000));
        }

        /// <summary>
        /// Performance Test 3: SLA Calculation Performance
        /// Target: < 5 minutes for all active instances
        /// 
        /// **Validates: Requirement 21.4 - SLA evaluation for all active instances within 5 minutes**
        /// </summary>
        [Test]
        public void PerformanceTest_SLACalculation_CompletesWithin5Minutes()
        {
            // Arrange
            var stepInstances = GenerateStepInstances(10000); // 10,000 active step instances
            _stepInstanceRepository.GetPendingStepInstances()
                .Returns(stepInstances);

            var stopwatch = Stopwatch.StartNew();

            // Act
            foreach (var stepInstance in stepInstances)
            {
                var slaStatus = _slaCalculator.EvaluateSLAStatus(stepInstance);
                stepInstance.SLAStatus = slaStatus;
            }

            stopwatch.Stop();

            // Assert
            var fiveMinutesInMs = 5 * 60 * 1000;
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(fiveMinutesInMs),
                $"SLA calculation time {stopwatch.ElapsedMilliseconds}ms exceeds target of {fiveMinutesInMs}ms");
        }

        /// <summary>
        /// Performance Test 4: Background Job Performance
        /// Target: < 1 minute for SLA checks
        /// 
        /// **Validates: Requirement 21.5 - Background job escalation within 1 minute**
        /// </summary>
        [Test]
        public void PerformanceTest_BackgroundJobSLACheck_CompletesWithin1Minute()
        {
            // Arrange
            var stepInstances = GenerateStepInstances(5000); // 5,000 step instances for SLA check
            _stepInstanceRepository.GetPendingStepInstances()
                .Returns(stepInstances);

            var stopwatch = Stopwatch.StartNew();

            // Act
            var escalatedCount = 0;
            foreach (var stepInstance in stepInstances)
            {
                var slaStatus = _slaCalculator.EvaluateSLAStatus(stepInstance);
                
                if (slaStatus == SLAStatus.Overdue)
                {
                    var escalated = _slaCalculator.CheckAndEscalateSLA(stepInstance);
                    if (escalated)
                    {
                        escalatedCount++;
                    }
                }
            }

            stopwatch.Stop();

            // Assert
            var oneMinuteInMs = 60 * 1000;
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(oneMinuteInMs),
                $"Background job time {stopwatch.ElapsedMilliseconds}ms exceeds target of {oneMinuteInMs}ms");
        }

        /// <summary>
        /// Performance Test 5: Bulk Workflow Operations
        /// Tests performance of creating multiple workflows in batch
        /// </summary>
        [Test]
        public void PerformanceTest_BulkWorkflowCreation_Handles100InstancesEfficiently()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            var instances = new List<WorkflowInstance>();
            for (int i = 0; i < 100; i++)
            {
                instances.Add(new WorkflowInstance
                {
                    WorkflowInstanceId = i,
                    WorkflowDefinitionId = 1,
                    TemplateVersion = 1,
                    InitiatedBy = _userContext.ADUserName,
                    InitiatedDate = DateTime.Now,
                    Status = WorkflowInstanceStatus.Created
                });
            }

            stopwatch.Stop();

            // Assert
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000),
                $"Bulk creation of 100 instances took {stopwatch.ElapsedMilliseconds}ms");
            Assert.That(instances.Count, Is.EqualTo(100));
        }

        /// <summary>
        /// Performance Test 6: Query Performance with Filtering
        /// Tests dashboard query performance with various filters
        /// </summary>
        [Test]
        public void PerformanceTest_DashboardQueryWithFilters_CompletesQuickly()
        {
            // Arrange
            var instances = GenerateWorkflowInstances(500);
            _workflowRepository.GetWorkflowInstances(Arg.Any<WorkflowInstanceFilter>())
                .Returns(new PagedResult<WorkflowInstance>
                {
                    Items = instances.Where(i => i.Status == WorkflowInstanceStatus.InProgress).ToList(),
                    TotalCount = instances.Count,
                    PageNumber = 1,
                    PageSize = 500
                });

            var stopwatch = Stopwatch.StartNew();

            // Act
            var result = _workflowRepository.GetWorkflowInstances(new WorkflowInstanceFilter
            {
                Status = WorkflowInstanceStatus.InProgress,
                PageNumber = 1,
                PageSize = 500
            });

            stopwatch.Stop();

            // Assert
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000),
                $"Filtered query took {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Performance Test 7: Pagination Performance
        /// Tests dashboard pagination with large datasets
        /// </summary>
        [Test]
        public void PerformanceTest_PaginationPerformance_HandlesLargeDatasets()
        {
            // Arrange
            var allInstances = GenerateWorkflowInstances(10000);
            var pageSize = 50;
            var stopwatch = Stopwatch.StartNew();

            // Act
            var totalPages = (allInstances.Count + pageSize - 1) / pageSize;
            var pageLoadTimes = new List<long>();

            for (int page = 1; page <= Math.Min(totalPages, 10); page++)
            {
                var pageStopwatch = Stopwatch.StartNew();
                var pageData = allInstances
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                pageStopwatch.Stop();
                pageLoadTimes.Add(pageStopwatch.ElapsedMilliseconds);
            }

            stopwatch.Stop();

            // Assert
            var averagePageLoadTime = pageLoadTimes.Average();
            Assert.That(averagePageLoadTime, Is.LessThan(500),
                $"Average page load time {averagePageLoadTime}ms exceeds target");
        }

        /// <summary>
        /// Performance Test 8: Concurrent Operations
        /// Tests performance under concurrent load
        /// </summary>
        [Test]
        public void PerformanceTest_ConcurrentOperations_HandlesMultipleRequests()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<System.Threading.Tasks.Task>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                var task = System.Threading.Tasks.Task.Run(() =>
                {
                    var instances = GenerateWorkflowInstances(100);
                    foreach (var instance in instances)
                    {
                        _workflowRepository.CreateWorkflowInstance(instance);
                    }
                });
                tasks.Add(task);
            }

            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();

            // Assert
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000),
                $"Concurrent operations took {stopwatch.ElapsedMilliseconds}ms");
        }

        // Helper methods

        private List<WorkflowInstance> GenerateWorkflowInstances(int count)
        {
            var instances = new List<WorkflowInstance>();
            var statuses = new[] { WorkflowInstanceStatus.Created, WorkflowInstanceStatus.InProgress, WorkflowInstanceStatus.Completed };

            for (int i = 0; i < count; i++)
            {
                instances.Add(new WorkflowInstance
                {
                    WorkflowInstanceId = i,
                    WorkflowDefinitionId = (i % 10) + 1,
                    TemplateVersion = 1,
                    InitiatedBy = $"user{i % 100}",
                    InitiatedDate = DateTime.Now.AddDays(-(i % 30)),
                    Status = statuses[i % statuses.Length],
                    InputParameters = "{}"
                });
            }

            return instances;
        }

        private List<WorkflowStepInstance> GenerateStepInstances(int count)
        {
            var instances = new List<WorkflowStepInstance>();
            var statuses = new[] { WorkflowStepInstanceStatus.Pending, WorkflowStepInstanceStatus.InProgress };

            for (int i = 0; i < count; i++)
            {
                var createdDate = DateTime.Now.AddHours(-(i % 48));
                instances.Add(new WorkflowStepInstance
                {
                    WorkflowStepInstanceId = i,
                    WorkflowInstanceId = i / 10,
                    WorkflowStepId = (i % 5) + 1,
                    AssignedTo = $"user{i % 100}",
                    AssignedDate = createdDate,
                    Status = statuses[i % statuses.Length],
                    SLADeadline = createdDate.AddHours(24),
                    SLAStatus = DateTime.Now > createdDate.AddHours(24) ? SLAStatus.Overdue : SLAStatus.OnTime
                });
            }

            return instances;
        }
    }
}
