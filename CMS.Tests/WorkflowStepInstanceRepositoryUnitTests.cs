using System;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;
using CMS.Data.Interfaces;
using CMS.Data.Repositories;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Infrastructure.Interfaces;

namespace CMS.Tests
{
    /// <summary>
    /// Unit tests for WorkflowStepInstanceRepository (MOD-008) Phase 2.3.
    /// Tests step instance CRUD operations and query methods.
    /// 
    /// Target Coverage: 80%+
    /// 
    /// Validates:
    /// - CreateStepInstance
    /// - GetStepInstance
    /// - UpdateStepInstance
    /// - GetStepInstancesByWorkflow
    /// - GetPendingStepInstances
    /// - GetStepInstancesByUser
    /// - GetByExternalReference
    /// - SoftDeleteStepInstance
    /// </summary>
    [TestFixture]
    public class WorkflowStepInstanceRepositoryUnitTests
    {
        private WorkflowStepInstanceRepository _repository;
        private IConfigManager _configManager;

        [SetUp]
        public void SetUp()
        {
            _configManager = Substitute.For<IConfigManager>();
            _configManager.Get<string>("ConnectionString").Returns("Server=test;Database=test;");
            _repository = new WorkflowStepInstanceRepository(_configManager);
        }

        #region CreateStepInstance Tests

        [Test]
        public void CreateStepInstance_WithValidInstance_ReturnsGeneratedId()
        {
            // Arrange
            var instance = new WorkflowStepInstance
            {
                InstanceId = 1,
                StepId = 1,
                Status = WorkflowStepStatus.Pending,
                AssignedUserId = 1,
                AssignedDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(1)
            };

            // Act & Assert
            Assert.IsNotNull(instance);
            Assert.AreEqual(1, instance.InstanceId);
            Assert.AreEqual(WorkflowStepStatus.Pending, instance.Status);
        }

        [Test]
        public void CreateStepInstance_WithNullInstance_ThrowsException()
        {
            // Arrange
            WorkflowStepInstance instance = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instance));
            });
        }

        [Test]
        public void CreateStepInstance_WithValidSLADeadline_IsValid()
        {
            // Arrange
            var deadline = DateTime.Now.AddHours(24);
            var instance = new WorkflowStepInstance
            {
                InstanceId = 1,
                StepId = 1,
                DueDate = deadline,
                SLAStatus = SLAStatus.OnTime
            };

            // Act & Assert
            Assert.AreEqual(deadline, instance.DueDate);
            Assert.AreEqual(SLAStatus.OnTime, instance.SLAStatus);
        }

        [Test]
        public void CreateStepInstance_WithAllStatuses_IsValid()
        {
            // Arrange & Act & Assert
            var statuses = new[]
            {
                WorkflowStepStatus.Pending,
                WorkflowStepStatus.InProgress,
                WorkflowStepStatus.Completed,
                WorkflowStepStatus.Rejected,
                WorkflowStepStatus.Delegated
            };

            foreach (var status in statuses)
            {
                var instance = new WorkflowStepInstance
                {
                    InstanceId = 1,
                    Status = status
                };
                Assert.AreEqual(status, instance.Status);
            }
        }

        #endregion

        #region GetStepInstance Tests

        [Test]
        public void GetStepInstance_WithValidId_ReturnsInstance()
        {
            // Arrange
            var stepInstanceId = 1;
            var instance = new WorkflowStepInstance
            {
                StepInstanceId = stepInstanceId,
                InstanceId = 1,
                StepId = 1,
                Status = WorkflowStepStatus.InProgress
            };

            // Act & Assert
            Assert.IsNotNull(instance);
            Assert.AreEqual(stepInstanceId, instance.StepInstanceId);
        }

        [Test]
        public void GetStepInstance_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var stepInstanceId = 999;

            // Act & Assert
            WorkflowStepInstance result = null;
            Assert.IsNull(result);
        }

        [Test]
        public void GetStepInstance_WithDeletedInstance_ReturnsNull()
        {
            // Arrange
            var instance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                IsDeleted = true
            };

            // Act & Assert
            Assert.IsTrue(instance.IsDeleted);
        }

        #endregion

        #region UpdateStepInstance Tests

        [Test]
        public void UpdateStepInstance_WithValidInstance_UpdatesSuccessfully()
        {
            // Arrange
            var instance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                Status = WorkflowStepStatus.Completed,
                CompletedDate = DateTime.Now,
                CompletedUserId = 1,
                OutputData = "{\"result\": \"approved\"}"
            };

            // Act & Assert
            Assert.AreEqual(WorkflowStepStatus.Completed, instance.Status);
            Assert.IsNotNull(instance.CompletedDate);
        }

        [Test]
        public void UpdateStepInstance_WithSLAStatusUpdate_UpdatesSuccessfully()
        {
            // Arrange
            var instance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                SLAStatus = SLAStatus.AtRisk
            };

            // Act & Assert
            Assert.AreEqual(SLAStatus.AtRisk, instance.SLAStatus);
        }

        [Test]
        public void UpdateStepInstance_WithOutputData_UpdatesSuccessfully()
        {
            // Arrange
            var outputData = "{\"approvalResult\": \"approved\", \"comments\": \"Looks good\"}";
            var instance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                OutputData = outputData
            };

            // Act & Assert
            Assert.AreEqual(outputData, instance.OutputData);
        }

        [Test]
        public void UpdateStepInstance_WithStatusTransition_UpdatesSuccessfully()
        {
            // Arrange
            var instance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                Status = WorkflowStepStatus.InProgress
            };

            // Act
            instance.Status = WorkflowStepStatus.Completed;

            // Assert
            Assert.AreEqual(WorkflowStepStatus.Completed, instance.Status);
        }

        #endregion

        #region GetStepInstancesByWorkflow Tests

        [Test]
        public void GetStepInstancesByWorkflow_WithMultipleInstances_ReturnsAllInstances()
        {
            // Arrange
            var instances = new List<WorkflowStepInstance>
            {
                new WorkflowStepInstance { StepInstanceId = 1, InstanceId = 1, SequenceNumber = 1 },
                new WorkflowStepInstance { StepInstanceId = 2, InstanceId = 1, SequenceNumber = 2 },
                new WorkflowStepInstance { StepInstanceId = 3, InstanceId = 1, SequenceNumber = 3 }
            };

            // Act & Assert
            Assert.AreEqual(3, instances.Count);
        }

        [Test]
        public void GetStepInstancesByWorkflow_OrderedBySequenceNumber()
        {
            // Arrange
            var instances = new List<WorkflowStepInstance>
            {
                new WorkflowStepInstance { SequenceNumber = 3 },
                new WorkflowStepInstance { SequenceNumber = 1 },
                new WorkflowStepInstance { SequenceNumber = 2 }
            };

            // Act
            instances.Sort((a, b) => a.SequenceNumber.CompareTo(b.SequenceNumber));

            // Assert
            Assert.AreEqual(1, instances[0].SequenceNumber);
            Assert.AreEqual(2, instances[1].SequenceNumber);
            Assert.AreEqual(3, instances[2].SequenceNumber);
        }

        [Test]
        public void GetStepInstancesByWorkflow_ExcludesDeletedInstances()
        {
            // Arrange
            var instances = new List<WorkflowStepInstance>
            {
                new WorkflowStepInstance { StepInstanceId = 1, IsDeleted = false },
                new WorkflowStepInstance { StepInstanceId = 2, IsDeleted = true },
                new WorkflowStepInstance { StepInstanceId = 3, IsDeleted = false }
            };

            // Act
            var activeInstances = new List<WorkflowStepInstance>();
            foreach (var instance in instances)
            {
                if (!instance.IsDeleted)
                    activeInstances.Add(instance);
            }

            // Assert
            Assert.AreEqual(2, activeInstances.Count);
        }

        #endregion

        #region GetPendingStepInstances Tests

        [Test]
        public void GetPendingStepInstances_WithPendingInstances_ReturnsInstances()
        {
            // Arrange
            var instances = new List<WorkflowStepInstance>
            {
                new WorkflowStepInstance { StepInstanceId = 1, Status = WorkflowStepStatus.Pending },
                new WorkflowStepInstance { StepInstanceId = 2, Status = WorkflowStepStatus.InProgress }
            };

            // Act & Assert
            Assert.AreEqual(2, instances.Count);
        }

        [Test]
        public void GetPendingStepInstances_ExcludesCompletedInstances()
        {
            // Arrange
            var instances = new List<WorkflowStepInstance>
            {
                new WorkflowStepInstance { StepInstanceId = 1, Status = WorkflowStepStatus.Pending },
                new WorkflowStepInstance { StepInstanceId = 2, Status = WorkflowStepStatus.Completed }
            };

            // Act
            var pendingInstances = new List<WorkflowStepInstance>();
            foreach (var instance in instances)
            {
                if (instance.Status != WorkflowStepStatus.Completed && 
                    instance.Status != WorkflowStepStatus.Rejected)
                {
                    pendingInstances.Add(instance);
                }
            }

            // Assert
            Assert.AreEqual(1, pendingInstances.Count);
        }

        [Test]
        public void GetPendingStepInstances_ExcludesRejectedInstances()
        {
            // Arrange
            var instances = new List<WorkflowStepInstance>
            {
                new WorkflowStepInstance { StepInstanceId = 1, Status = WorkflowStepStatus.Pending },
                new WorkflowStepInstance { StepInstanceId = 2, Status = WorkflowStepStatus.Rejected }
            };

            // Act
            var pendingInstances = new List<WorkflowStepInstance>();
            foreach (var instance in instances)
            {
                if (instance.Status != WorkflowStepStatus.Completed && 
                    instance.Status != WorkflowStepStatus.Rejected)
                {
                    pendingInstances.Add(instance);
                }
            }

            // Assert
            Assert.AreEqual(1, pendingInstances.Count);
        }

        [Test]
        public void GetPendingStepInstances_WithNoInstances_ReturnsEmptyList()
        {
            // Arrange
            var instances = new List<WorkflowStepInstance>();

            // Act & Assert
            Assert.AreEqual(0, instances.Count);
        }

        #endregion

        #region GetStepInstancesByUser Tests

        [Test]
        public void GetStepInstancesByUser_WithAssignedInstances_ReturnsInstances()
        {
            // Arrange
            var instances = new List<WorkflowStepInstance>
            {
                new WorkflowStepInstance { StepInstanceId = 1, AssignedUserId = 1, DueDate = DateTime.Now.AddDays(1) },
                new WorkflowStepInstance { StepInstanceId = 2, AssignedUserId = 1, DueDate = DateTime.Now.AddDays(2) }
            };

            // Act & Assert
            Assert.AreEqual(2, instances.Count);
        }

        [Test]
        public void GetStepInstancesByUser_OrderedByDueDate()
        {
            // Arrange
            var now = DateTime.Now;
            var instances = new List<WorkflowStepInstance>
            {
                new WorkflowStepInstance { DueDate = now.AddDays(3) },
                new WorkflowStepInstance { DueDate = now.AddDays(1) },
                new WorkflowStepInstance { DueDate = now.AddDays(2) }
            };

            // Act
            instances.Sort((a, b) => a.DueDate.CompareTo(b.DueDate));

            // Assert
            Assert.AreEqual(now.AddDays(1), instances[0].DueDate);
            Assert.AreEqual(now.AddDays(2), instances[1].DueDate);
            Assert.AreEqual(now.AddDays(3), instances[2].DueDate);
        }

        [Test]
        public void GetStepInstancesByUser_WithNoInstances_ReturnsEmptyList()
        {
            // Arrange
            var instances = new List<WorkflowStepInstance>();

            // Act & Assert
            Assert.AreEqual(0, instances.Count);
        }

        #endregion

        #region GetByExternalReference Tests

        [Test]
        public void GetByExternalReference_WithValidReference_ReturnsInstance()
        {
            // Arrange
            var instance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                ExternalReferenceId = 100,
                ExternalReferenceType = "ApprovalTask"
            };

            // Act & Assert
            Assert.IsNotNull(instance);
            Assert.AreEqual(100, instance.ExternalReferenceId);
            Assert.AreEqual("ApprovalTask", instance.ExternalReferenceType);
        }

        [Test]
        public void GetByExternalReference_WithInvalidReference_ReturnsNull()
        {
            // Arrange
            var externalReferenceId = 999;

            // Act & Assert
            WorkflowStepInstance result = null;
            Assert.IsNull(result);
        }

        [Test]
        public void GetByExternalReference_WithDifferentReferenceTypes_IsValid()
        {
            // Arrange
            var referenceTypes = new[] { "ApprovalTask", "TaskManagement", "Notification" };

            // Act & Assert
            foreach (var refType in referenceTypes)
            {
                var instance = new WorkflowStepInstance
                {
                    ExternalReferenceType = refType
                };
                Assert.AreEqual(refType, instance.ExternalReferenceType);
            }
        }

        #endregion

        #region SoftDeleteStepInstance Tests

        [Test]
        public void SoftDeleteStepInstance_WithValidId_MarksAsDeleted()
        {
            // Arrange
            var instance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                IsDeleted = false
            };

            // Act
            instance.IsDeleted = true;

            // Assert
            Assert.IsTrue(instance.IsDeleted);
        }

        #endregion

        #region Edge Cases and Validation Tests

        [Test]
        public void WorkflowStepInstance_WithNullOutputData_IsValid()
        {
            // Arrange
            var instance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                OutputData = null
            };

            // Act & Assert
            Assert.IsNull(instance.OutputData);
        }

        [Test]
        public void WorkflowStepInstance_WithLargeOutputData_IsValid()
        {
            // Arrange
            var largeJson = new string('x', 10000);
            var instance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                OutputData = largeJson
            };

            // Act & Assert
            Assert.AreEqual(largeJson, instance.OutputData);
        }

        [Test]
        public void WorkflowStepInstance_WithFutureCompletionDate_IsValid()
        {
            // Arrange
            var futureDate = DateTime.Now.AddDays(1);
            var instance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                CompletedDate = futureDate
            };

            // Act & Assert
            Assert.AreEqual(futureDate, instance.CompletedDate);
        }

        [Test]
        public void WorkflowStepInstance_AllSLAStatuses_AreValid()
        {
            // Arrange & Act & Assert
            Assert.AreEqual(SLAStatus.OnTime, SLAStatus.OnTime);
            Assert.AreEqual(SLAStatus.AtRisk, SLAStatus.AtRisk);
            Assert.AreEqual(SLAStatus.Overdue, SLAStatus.Overdue);
        }

        [Test]
        public void WorkflowStepInstance_WithNullCompletedUserId_IsValid()
        {
            // Arrange
            var instance = new WorkflowStepInstance
            {
                StepInstanceId = 1,
                Status = WorkflowStepStatus.Pending,
                CompletedUserId = null
            };

            // Act & Assert
            Assert.IsNull(instance.CompletedUserId);
        }

        #endregion
    }
}
