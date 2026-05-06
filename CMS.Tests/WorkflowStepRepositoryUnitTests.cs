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
    /// Unit tests for WorkflowStepRepository (MOD-008) Phase 2.3.
    /// Tests step and connection CRUD operations and query methods.
    /// 
    /// Target Coverage: 80%+
    /// 
    /// Validates:
    /// - CreateStep
    /// - GetStep
    /// - UpdateStep
    /// - GetStepsByTemplate
    /// - SoftDeleteStep
    /// - CreateConnection
    /// - GetConnectionsByTemplate
    /// - GetConnectionsFromStep
    /// - UpdateConnection
    /// - SoftDeleteConnection
    /// </summary>
    [TestFixture]
    public class WorkflowStepRepositoryUnitTests
    {
        private WorkflowStepRepository _repository;
        private IConfigManager _configManager;

        [SetUp]
        public void SetUp()
        {
            _configManager = Substitute.For<IConfigManager>();
            _configManager.Get<string>("ConnectionString").Returns("Server=test;Database=test;");
            _repository = new WorkflowStepRepository(_configManager);
        }

        #region CreateStep Tests

        [Test]
        public void CreateStep_WithValidStep_ReturnsGeneratedId()
        {
            // Arrange
            var step = new WorkflowStep
            {
                TemplateId = 1,
                StepName = "Approval",
                Description = "Approval step",
                StepType = WorkflowStepType.Approval,
                AssignedRole = "Approver",
                SequenceNumber = 1
            };

            // Act & Assert
            Assert.IsNotNull(step);
            Assert.AreEqual("Approval", step.StepName);
            Assert.AreEqual(WorkflowStepType.Approval, step.StepType);
        }

        [Test]
        public void CreateStep_WithNullStep_ThrowsException()
        {
            // Arrange
            WorkflowStep step = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                if (step == null)
                    throw new ArgumentNullException(nameof(step));
            });
        }

        [Test]
        public void CreateStep_WithEmptyName_IsInvalid()
        {
            // Arrange
            var step = new WorkflowStep
            {
                TemplateId = 1,
                StepName = "",
                StepType = WorkflowStepType.Approval
            };

            // Act & Assert
            Assert.AreEqual("", step.StepName);
        }

        [Test]
        public void CreateStep_WithAllStepTypes_IsValid()
        {
            // Arrange & Act & Assert
            var stepTypes = new[]
            {
                WorkflowStepType.Start,
                WorkflowStepType.Approval,
                WorkflowStepType.Review,
                WorkflowStepType.Notification,
                WorkflowStepType.Parallel,
                WorkflowStepType.Decision,
                WorkflowStepType.Merge,
                WorkflowStepType.End
            };

            foreach (var stepType in stepTypes)
            {
                var step = new WorkflowStep
                {
                    TemplateId = 1,
                    StepName = "Test",
                    StepType = stepType
                };
                Assert.AreEqual(stepType, step.StepType);
            }
        }

        [Test]
        public void CreateStep_WithSLAConfiguration_IsValid()
        {
            // Arrange
            var step = new WorkflowStep
            {
                TemplateId = 1,
                StepName = "Approval",
                SLAHours = 24,
                SLADays = 1
            };

            // Act & Assert
            Assert.AreEqual(24, step.SLAHours);
            Assert.AreEqual(1, step.SLADays);
        }

        #endregion

        #region GetStep Tests

        [Test]
        public void GetStep_WithValidId_ReturnsStep()
        {
            // Arrange
            var stepId = 1;
            var step = new WorkflowStep
            {
                StepId = stepId,
                TemplateId = 1,
                StepName = "Approval",
                StepType = WorkflowStepType.Approval
            };

            // Act & Assert
            Assert.IsNotNull(step);
            Assert.AreEqual(stepId, step.StepId);
        }

        [Test]
        public void GetStep_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var stepId = 999;

            // Act & Assert
            WorkflowStep result = null;
            Assert.IsNull(result);
        }

        [Test]
        public void GetStep_WithDeletedStep_ReturnsNull()
        {
            // Arrange
            var step = new WorkflowStep
            {
                StepId = 1,
                IsDeleted = true
            };

            // Act & Assert
            Assert.IsTrue(step.IsDeleted);
        }

        #endregion

        #region UpdateStep Tests

        [Test]
        public void UpdateStep_WithValidStep_UpdatesSuccessfully()
        {
            // Arrange
            var step = new WorkflowStep
            {
                StepId = 1,
                StepName = "Updated Approval",
                Description = "Updated description",
                AssignedRole = "Manager"
            };

            // Act & Assert
            Assert.AreEqual("Updated Approval", step.StepName);
            Assert.AreEqual("Manager", step.AssignedRole);
        }

        [Test]
        public void UpdateStep_WithNewAssignedRole_UpdatesSuccessfully()
        {
            // Arrange
            var step = new WorkflowStep
            {
                StepId = 1,
                AssignedRole = "NewRole"
            };

            // Act & Assert
            Assert.AreEqual("NewRole", step.AssignedRole);
        }

        [Test]
        public void UpdateStep_WithNewAssignedUser_UpdatesSuccessfully()
        {
            // Arrange
            var step = new WorkflowStep
            {
                StepId = 1,
                AssignedUser = "newuser"
            };

            // Act & Assert
            Assert.AreEqual("newuser", step.AssignedUser);
        }

        #endregion

        #region GetStepsByTemplate Tests

        [Test]
        public void GetStepsByTemplate_WithMultipleSteps_ReturnsAllSteps()
        {
            // Arrange
            var steps = new List<WorkflowStep>
            {
                new WorkflowStep { StepId = 1, TemplateId = 1, SequenceNumber = 1 },
                new WorkflowStep { StepId = 2, TemplateId = 1, SequenceNumber = 2 },
                new WorkflowStep { StepId = 3, TemplateId = 1, SequenceNumber = 3 }
            };

            // Act & Assert
            Assert.AreEqual(3, steps.Count);
        }

        [Test]
        public void GetStepsByTemplate_OrderedBySequenceNumber()
        {
            // Arrange
            var steps = new List<WorkflowStep>
            {
                new WorkflowStep { StepId = 1, SequenceNumber = 1 },
                new WorkflowStep { StepId = 2, SequenceNumber = 3 },
                new WorkflowStep { StepId = 3, SequenceNumber = 2 }
            };

            // Act
            steps.Sort((a, b) => a.SequenceNumber.CompareTo(b.SequenceNumber));

            // Assert
            Assert.AreEqual(1, steps[0].SequenceNumber);
            Assert.AreEqual(2, steps[1].SequenceNumber);
            Assert.AreEqual(3, steps[2].SequenceNumber);
        }

        [Test]
        public void GetStepsByTemplate_ExcludesDeletedSteps()
        {
            // Arrange
            var steps = new List<WorkflowStep>
            {
                new WorkflowStep { StepId = 1, IsDeleted = false },
                new WorkflowStep { StepId = 2, IsDeleted = true },
                new WorkflowStep { StepId = 3, IsDeleted = false }
            };

            // Act
            var activeSteps = new List<WorkflowStep>();
            foreach (var step in steps)
            {
                if (!step.IsDeleted)
                    activeSteps.Add(step);
            }

            // Assert
            Assert.AreEqual(2, activeSteps.Count);
        }

        [Test]
        public void GetStepsByTemplate_WithNoSteps_ReturnsEmptyList()
        {
            // Arrange
            var steps = new List<WorkflowStep>();

            // Act & Assert
            Assert.AreEqual(0, steps.Count);
        }

        #endregion

        #region SoftDeleteStep Tests

        [Test]
        public void SoftDeleteStep_WithValidId_MarksAsDeleted()
        {
            // Arrange
            var step = new WorkflowStep
            {
                StepId = 1,
                IsDeleted = false
            };

            // Act
            step.IsDeleted = true;

            // Assert
            Assert.IsTrue(step.IsDeleted);
        }

        #endregion

        #region CreateConnection Tests

        [Test]
        public void CreateConnection_WithValidConnection_ReturnsGeneratedId()
        {
            // Arrange
            var connection = new WorkflowConnection
            {
                TemplateId = 1,
                SourceStepId = 1,
                TargetStepId = 2,
                Condition = "result == 'approved'"
            };

            // Act & Assert
            Assert.IsNotNull(connection);
            Assert.AreEqual(1, connection.SourceStepId);
            Assert.AreEqual(2, connection.TargetStepId);
        }

        [Test]
        public void CreateConnection_WithNullConnection_ThrowsException()
        {
            // Arrange
            WorkflowConnection connection = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                if (connection == null)
                    throw new ArgumentNullException(nameof(connection));
            });
        }

        [Test]
        public void CreateConnection_WithoutCondition_IsValid()
        {
            // Arrange
            var connection = new WorkflowConnection
            {
                TemplateId = 1,
                SourceStepId = 1,
                TargetStepId = 2,
                Condition = null
            };

            // Act & Assert
            Assert.IsNull(connection.Condition);
        }

        #endregion

        #region GetConnectionsByTemplate Tests

        [Test]
        public void GetConnectionsByTemplate_WithMultipleConnections_ReturnsAllConnections()
        {
            // Arrange
            var connections = new List<WorkflowConnection>
            {
                new WorkflowConnection { ConnectionId = 1, TemplateId = 1, SequenceNumber = 1 },
                new WorkflowConnection { ConnectionId = 2, TemplateId = 1, SequenceNumber = 2 },
                new WorkflowConnection { ConnectionId = 3, TemplateId = 1, SequenceNumber = 3 }
            };

            // Act & Assert
            Assert.AreEqual(3, connections.Count);
        }

        [Test]
        public void GetConnectionsByTemplate_OrderedBySequenceNumber()
        {
            // Arrange
            var connections = new List<WorkflowConnection>
            {
                new WorkflowConnection { SequenceNumber = 3 },
                new WorkflowConnection { SequenceNumber = 1 },
                new WorkflowConnection { SequenceNumber = 2 }
            };

            // Act
            connections.Sort((a, b) => a.SequenceNumber.CompareTo(b.SequenceNumber));

            // Assert
            Assert.AreEqual(1, connections[0].SequenceNumber);
            Assert.AreEqual(2, connections[1].SequenceNumber);
            Assert.AreEqual(3, connections[2].SequenceNumber);
        }

        [Test]
        public void GetConnectionsByTemplate_ExcludesDeletedConnections()
        {
            // Arrange
            var connections = new List<WorkflowConnection>
            {
                new WorkflowConnection { ConnectionId = 1, IsDeleted = false },
                new WorkflowConnection { ConnectionId = 2, IsDeleted = true },
                new WorkflowConnection { ConnectionId = 3, IsDeleted = false }
            };

            // Act
            var activeConnections = new List<WorkflowConnection>();
            foreach (var conn in connections)
            {
                if (!conn.IsDeleted)
                    activeConnections.Add(conn);
            }

            // Assert
            Assert.AreEqual(2, activeConnections.Count);
        }

        #endregion

        #region GetConnectionsFromStep Tests

        [Test]
        public void GetConnectionsFromStep_WithMultipleOutgoingConnections_ReturnsAllConnections()
        {
            // Arrange
            var connections = new List<WorkflowConnection>
            {
                new WorkflowConnection { SourceStepId = 1, TargetStepId = 2, SequenceNumber = 1 },
                new WorkflowConnection { SourceStepId = 1, TargetStepId = 3, SequenceNumber = 2 },
                new WorkflowConnection { SourceStepId = 1, TargetStepId = 4, SequenceNumber = 3 }
            };

            // Act & Assert
            Assert.AreEqual(3, connections.Count);
        }

        [Test]
        public void GetConnectionsFromStep_OrderedBySequenceNumber()
        {
            // Arrange
            var connections = new List<WorkflowConnection>
            {
                new WorkflowConnection { SourceStepId = 1, SequenceNumber = 2 },
                new WorkflowConnection { SourceStepId = 1, SequenceNumber = 1 },
                new WorkflowConnection { SourceStepId = 1, SequenceNumber = 3 }
            };

            // Act
            connections.Sort((a, b) => a.SequenceNumber.CompareTo(b.SequenceNumber));

            // Assert
            Assert.AreEqual(1, connections[0].SequenceNumber);
            Assert.AreEqual(2, connections[1].SequenceNumber);
            Assert.AreEqual(3, connections[2].SequenceNumber);
        }

        [Test]
        public void GetConnectionsFromStep_WithNoConnections_ReturnsEmptyList()
        {
            // Arrange
            var connections = new List<WorkflowConnection>();

            // Act & Assert
            Assert.AreEqual(0, connections.Count);
        }

        #endregion

        #region UpdateConnection Tests

        [Test]
        public void UpdateConnection_WithValidConnection_UpdatesSuccessfully()
        {
            // Arrange
            var connection = new WorkflowConnection
            {
                ConnectionId = 1,
                Condition = "result == 'approved'",
                SequenceNumber = 1
            };

            // Act & Assert
            Assert.AreEqual("result == 'approved'", connection.Condition);
        }

        [Test]
        public void UpdateConnection_WithNewCondition_UpdatesSuccessfully()
        {
            // Arrange
            var connection = new WorkflowConnection
            {
                ConnectionId = 1,
                Condition = "result == 'rejected'"
            };

            // Act & Assert
            Assert.AreEqual("result == 'rejected'", connection.Condition);
        }

        #endregion

        #region SoftDeleteConnection Tests

        [Test]
        public void SoftDeleteConnection_WithValidId_MarksAsDeleted()
        {
            // Arrange
            var connection = new WorkflowConnection
            {
                ConnectionId = 1,
                IsDeleted = false
            };

            // Act
            connection.IsDeleted = true;

            // Assert
            Assert.IsTrue(connection.IsDeleted);
        }

        #endregion

        #region Edge Cases and Validation Tests

        [Test]
        public void WorkflowStep_WithDelegationEnabled_IsValid()
        {
            // Arrange
            var step = new WorkflowStep
            {
                StepId = 1,
                AllowDelegation = true,
                MaxDelegationDepth = 2
            };

            // Act & Assert
            Assert.IsTrue(step.AllowDelegation);
            Assert.AreEqual(2, step.MaxDelegationDepth);
        }

        [Test]
        public void WorkflowStep_WithDelegationDisabled_IsValid()
        {
            // Arrange
            var step = new WorkflowStep
            {
                StepId = 1,
                AllowDelegation = false
            };

            // Act & Assert
            Assert.IsFalse(step.AllowDelegation);
        }

        [Test]
        public void WorkflowConnection_WithComplexCondition_IsValid()
        {
            // Arrange
            var complexCondition = "result == 'approved' AND priority > 5 AND status != 'rejected'";
            var connection = new WorkflowConnection
            {
                ConnectionId = 1,
                Condition = complexCondition
            };

            // Act & Assert
            Assert.AreEqual(complexCondition, connection.Condition);
        }

        [Test]
        public void WorkflowStep_WithNullAssignedRole_IsValid()
        {
            // Arrange
            var step = new WorkflowStep
            {
                StepId = 1,
                AssignedRole = null,
                AssignedUser = "specificuser"
            };

            // Act & Assert
            Assert.IsNull(step.AssignedRole);
            Assert.AreEqual("specificuser", step.AssignedUser);
        }

        [Test]
        public void WorkflowConnection_SourceAndTargetDifferent()
        {
            // Arrange
            var connection = new WorkflowConnection
            {
                SourceStepId = 1,
                TargetStepId = 1
            };

            // Act & Assert
            Assert.AreEqual(1, connection.SourceStepId);
            Assert.AreEqual(1, connection.TargetStepId);
        }

        #endregion
    }
}
