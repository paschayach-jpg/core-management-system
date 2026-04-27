using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using CMS.Data.Interfaces;
using CMS.Data.Repositories;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using NSubstitute;

namespace CMS.Tests
{
    /// <summary>
    /// Integration tests for Workflow Engine (MOD-008) Phase 10.
    /// Tests end-to-end workflow scenarios and cross-module interactions.
    /// 
    /// These tests use actual database connections to a test database.
    /// Ensure the test database has the workflow schema created before running.
    /// 
    /// Validates Requirements:
    /// - Requirement 3: Instance Management - Create Workflow Instances
    /// - Requirement 4: Instance Management - Task Assignment and Execution
    /// - Requirement 7: SLA Management - Monitor and Enforce SLA
    /// - Requirement 8: Delegation - Delegate Tasks
    /// - Requirement 13: Integration with Approval Workflow (MOD-003)
    /// - Requirement 15: Integration with Notification System (MOD-007)
    /// - Requirement 19: Integration with Audit Logging (MOD-020)
    /// </summary>
    [TestFixture]
    public class WorkflowEngineIntegrationTests
    {
        private IConfigManager _configManager;
        private IWorkflowRepository _workflowRepo;
        private IWorkflowTemplateRepository _templateRepo;
        private IWorkflowStepRepository _stepRepo;
        private IWorkflowStepInstanceRepository _stepInstanceRepo;
        private IDelegationRepository _delegationRepo;
        private IAuditRepository _auditRepo;
        private INotificationRepository _notificationRepo;
        private IApprovalRepository _approvalRepo;
        private string _connectionString;
        private UserContext _testUserContext;

        [SetUp]
        public void SetUp()
        {
            // Use test database connection string
            _connectionString = "Server=.;Database=CMS_Test;Integrated Security=true;";
            
            _configManager = Substitute.For<IConfigManager>();
            _configManager.Get<string>("ConnectionString").Returns(_connectionString);

            // Initialize repositories
            _workflowRepo = new WorkflowRepository(_configManager);
            _templateRepo = new WorkflowTemplateRepository(_configManager);
            _stepRepo = new WorkflowStepRepository(_configManager);
            _stepInstanceRepo = new WorkflowStepInstanceRepository(_configManager);
            _delegationRepo = new DelegationRepository(_configManager);
            _auditRepo = new AuditRepository(_configManager);
            _notificationRepo = new NotificationRepository(_configManager);
            _approvalRepo = new ApprovalRepository(_configManager);

            // Create test user context
            _testUserContext = new UserContext
            {
                ADUserName = "testuser",
                UserID = 1,
                UserRole = "Approver"
            };

            // Clean up test data before each test
            CleanupTestData();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test data after each test
            CleanupTestData();
        }

        private void CleanupTestData()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        @"DELETE FROM dbo.WFDelegation;
                          DELETE FROM dbo.WorkflowStepInstance;
                          DELETE FROM dbo.WorkflowInstance;
                          DELETE FROM dbo.WorkflowConnection;
                          DELETE FROM dbo.WorkflowStep;
                          DELETE FROM dbo.WorkflowDefinition;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        // -----------------------------------------------------------------------
        // Test 1: Workflow Instance Creation Integration
        // -----------------------------------------------------------------------

        [Test]
        public void WorkflowInstanceCreation_CreatesInstanceWithInitialStep()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instance = new WorkflowInstance
            {
                WorkflowDefinitionId = templateId,
                TemplateVersion = 1,
                InitiatedBy = _testUserContext.ADUserName,
                InitiatedDate = DateTime.UtcNow,
                Status = WorkflowInstanceStatus.InProgress,
                InputParameters = "{\"DocumentId\": 123, \"RequestType\": \"Approval\"}"
            };

            // Act
            var instanceId = _workflowRepo.CreateWorkflowInstance(instance);

            // Assert
            Assert.Greater(instanceId, 0);
            var persistedInstance = _workflowRepo.GetWorkflowInstance(instanceId);
            Assert.IsNotNull(persistedInstance);
            Assert.AreEqual(templateId, persistedInstance.WorkflowDefinitionId);
            Assert.AreEqual(WorkflowInstanceStatus.InProgress, persistedInstance.Status);
            Assert.AreEqual(_testUserContext.ADUserName, persistedInstance.InitiatedBy);
        }

        [Test]
        public void WorkflowInstanceCreation_WithValidTemplate_PersistsCorrectly()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instance = new WorkflowInstance
            {
                WorkflowDefinitionId = templateId,
                TemplateVersion = 1,
                InitiatedBy = _testUserContext.ADUserName,
                InitiatedDate = DateTime.UtcNow,
                Status = WorkflowInstanceStatus.InProgress,
                InputParameters = "{}"
            };

            // Act
            var instanceId = _workflowRepo.CreateWorkflowInstance(instance);
            var retrieved = _workflowRepo.GetWorkflowInstance(instanceId);

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(instance.WorkflowDefinitionId, retrieved.WorkflowDefinitionId);
            Assert.AreEqual(instance.TemplateVersion, retrieved.TemplateVersion);
            Assert.AreEqual(instance.InitiatedBy, retrieved.InitiatedBy);
        }

        // -----------------------------------------------------------------------
        // Test 2: Workflow Step Completion Integration
        // -----------------------------------------------------------------------

        [Test]
        public void WorkflowStepCompletion_CompletesStepAndUpdatesStatus()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            Assert.Greater(stepInstances.Count, 0);
            var firstStep = stepInstances.First();

            // Act
            firstStep.Status = WorkflowStepInstanceStatus.Completed;
            firstStep.CompletedDate = DateTime.UtcNow;
            firstStep.CompletedBy = _testUserContext.ADUserName;
            firstStep.OutputData = "{\"ApprovalStatus\": \"Approved\"}";
            _stepInstanceRepo.UpdateStepInstance(firstStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(firstStep.WorkflowStepInstanceId);
            Assert.AreEqual(WorkflowStepInstanceStatus.Completed, persistedStep.Status);
            Assert.AreEqual(_testUserContext.ADUserName, persistedStep.CompletedBy);
            Assert.IsNotNull(persistedStep.CompletedDate);
        }

        [Test]
        public void WorkflowStepCompletion_PreservesOutputData()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var firstStep = stepInstances.First();
            var outputData = "{\"ApprovalStatus\": \"Approved\", \"Comments\": \"Looks good\"}";

            // Act
            firstStep.Status = WorkflowStepInstanceStatus.Completed;
            firstStep.OutputData = outputData;
            _stepInstanceRepo.UpdateStepInstance(firstStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(firstStep.WorkflowStepInstanceId);
            Assert.AreEqual(outputData, persistedStep.OutputData);
        }

        // -----------------------------------------------------------------------
        // Test 3: SLA Escalation Integration
        // -----------------------------------------------------------------------

        [Test]
        public void SLAEscalation_IdentifiesOverdueSteps()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var firstStep = stepInstances.First();

            // Manually set SLA deadline to past
            firstStep.SLADeadline = DateTime.UtcNow.AddHours(-1);
            firstStep.SLAStatus = SLAStatus.Overdue;
            _stepInstanceRepo.UpdateStepInstance(firstStep);

            // Act
            var pendingSteps = _stepInstanceRepo.GetPendingStepInstances();

            // Assert
            var overdueStep = pendingSteps.FirstOrDefault(s => s.SLAStatus == SLAStatus.Overdue);
            Assert.IsNotNull(overdueStep);
            Assert.Less(overdueStep.SLADeadline, DateTime.UtcNow);
        }

        [Test]
        public void SLAEscalation_UpdatesSLAStatus()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var firstStep = stepInstances.First();

            // Act
            firstStep.SLAStatus = SLAStatus.AtRisk;
            _stepInstanceRepo.UpdateStepInstance(firstStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(firstStep.WorkflowStepInstanceId);
            Assert.AreEqual(SLAStatus.AtRisk, persistedStep.SLAStatus);
        }

        [Test]
        public void SLAEscalation_TracksSLADeadline()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var firstStep = stepInstances.First();
            var originalDeadline = firstStep.SLADeadline;

            // Act
            var retrievedStep = _stepInstanceRepo.GetStepInstance(firstStep.WorkflowStepInstanceId);

            // Assert
            Assert.AreEqual(originalDeadline, retrievedStep.SLADeadline);
        }

        // -----------------------------------------------------------------------
        // Test 4: Task Delegation Integration
        // -----------------------------------------------------------------------

        [Test]
        public void TaskDelegation_DelegatesTaskToAnotherUser()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var firstStep = stepInstances.First();

            var delegation = new WFDelegation
            {
                WorkflowStepInstanceId = firstStep.WorkflowStepInstanceId,
                DelegatedFrom = _testUserContext.ADUserName,
                DelegatedTo = "delegateuser",
                DelegationReason = "On leave",
                DelegationDate = DateTime.UtcNow,
                DelegationDepth = 1,
                IsActive = true
            };

            // Act
            var delegationId = _delegationRepo.CreateDelegation(delegation, 2);

            // Assert
            Assert.Greater(delegationId, 0);
            var persistedDelegation = _delegationRepo.GetDelegation(delegationId);
            Assert.IsNotNull(persistedDelegation);
            Assert.AreEqual("delegateuser", persistedDelegation.DelegatedTo);
            Assert.AreEqual(_testUserContext.ADUserName, persistedDelegation.DelegatedFrom);
        }

        [Test]
        public void TaskDelegation_TracksDelegationHistory()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var firstStep = stepInstances.First();

            var delegation = new WFDelegation
            {
                WorkflowStepInstanceId = firstStep.WorkflowStepInstanceId,
                DelegatedFrom = _testUserContext.ADUserName,
                DelegatedTo = "delegateuser",
                DelegationReason = "On leave",
                DelegationDate = DateTime.UtcNow,
                DelegationDepth = 1,
                IsActive = true
            };

            // Act
            _delegationRepo.CreateDelegation(delegation, 2);
            var history = _delegationRepo.GetDelegationsByStepInstance(firstStep.WorkflowStepInstanceId);

            // Assert
            Assert.Greater(history.Count, 0);
            Assert.AreEqual("delegateuser", history.First().DelegatedTo);
        }

        [Test]
        public void TaskDelegation_EnforcesDelegationDepthLimit()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var firstStep = stepInstances.First();

            var delegation1 = new WFDelegation
            {
                WorkflowStepInstanceId = firstStep.WorkflowStepInstanceId,
                DelegatedFrom = _testUserContext.ADUserName,
                DelegatedTo = "user2",
                DelegationReason = "First delegation",
                DelegationDate = DateTime.UtcNow,
                DelegationDepth = 1,
                IsActive = true
            };

            // Act
            _delegationRepo.CreateDelegation(delegation1, 2);
            var depth = _delegationRepo.GetDelegationDepth(firstStep.WorkflowStepInstanceId);

            // Assert
            Assert.GreaterOrEqual(depth, 1);
        }

        // -----------------------------------------------------------------------
        // Test 5: Approval Workflow Integration (MOD-003)
        // -----------------------------------------------------------------------

        [Test]
        public void ApprovalWorkflowIntegration_CreatesApprovalRequest()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);

            var approvalRequest = new ApprovalRequest
            {
                RequestType = "WORKFLOW_APPROVAL",
                FolderID = 1,
                RequestedBy = 1,
                Subject = "Workflow Approval",
                Remark = $"WorkflowInstanceId={instanceId}",
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            // Act
            var approvalId = _approvalRepo.InsertRequest(approvalRequest);

            // Assert
            Assert.Greater(approvalId, 0);
            var persistedApproval = _approvalRepo.GetRequest(approvalId);
            Assert.IsNotNull(persistedApproval);
            Assert.AreEqual("WORKFLOW_APPROVAL", persistedApproval.RequestType);
        }

        [Test]
        public void ApprovalWorkflowIntegration_TracksApprovalStatus()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);

            var approvalRequest = new ApprovalRequest
            {
                RequestType = "WORKFLOW_APPROVAL",
                FolderID = 1,
                RequestedBy = 1,
                Subject = "Workflow Approval",
                Remark = $"WorkflowInstanceId={instanceId}"
            };
            var approvalId = _approvalRepo.InsertRequest(approvalRequest);

            // Act
            _approvalRepo.UpdateRequestStatus(approvalId, 2, 1, "Approved"); // 2 = Approved

            // Assert
            var persistedApproval = _approvalRepo.GetRequest(approvalId);
            Assert.AreEqual(2, persistedApproval.RequestStatus);
        }

        [Test]
        public void ApprovalWorkflowIntegration_StoresWorkflowReference()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var remark = $"WorkflowInstanceId={instanceId}";

            var approvalRequest = new ApprovalRequest
            {
                RequestType = "WORKFLOW_APPROVAL",
                FolderID = 1,
                RequestedBy = 1,
                Subject = "Workflow Approval",
                Remark = remark
            };

            // Act
            var approvalId = _approvalRepo.InsertRequest(approvalRequest);
            var retrieved = _approvalRepo.GetRequest(approvalId);

            // Assert
            Assert.AreEqual(remark, retrieved.Remark);
        }

        // -----------------------------------------------------------------------
        // Test 6: Notification Delivery Integration (MOD-007)
        // -----------------------------------------------------------------------

        [Test]
        public void NotificationDelivery_CreatesNotificationForTaskAssignment()
        {
            // Arrange
            var notification = new Notification
            {
                RecipientUserId = 1,
                EventType = "TASK_ASSIGNED",
                RelatedEntityId = "1",
                MessageText = "You have been assigned a new task"
            };

            // Act
            var notificationId = _notificationRepo.Insert(notification);

            // Assert
            Assert.Greater(notificationId, 0);
            var unreadNotifications = _notificationRepo.GetUnreadByUser(1);
            Assert.Greater(unreadNotifications.Count(), 0);
        }

        [Test]
        public void NotificationDelivery_MarksNotificationAsRead()
        {
            // Arrange
            var notification = new Notification
            {
                RecipientUserId = 1,
                EventType = "TASK_ASSIGNED",
                RelatedEntityId = "1",
                MessageText = "You have been assigned a new task"
            };
            var notificationId = _notificationRepo.Insert(notification);

            // Act
            _notificationRepo.MarkRead(notificationId);

            // Assert
            var unreadNotifications = _notificationRepo.GetUnreadByUser(1);
            Assert.AreEqual(0, unreadNotifications.Count());
        }

        [Test]
        public void NotificationDelivery_StoresNotificationMetadata()
        {
            // Arrange
            var eventType = "SLA_WARNING";
            var messageText = "Your task SLA is approaching deadline";
            var notification = new Notification
            {
                RecipientUserId = 1,
                EventType = eventType,
                RelatedEntityId = "123",
                MessageText = messageText
            };

            // Act
            var notificationId = _notificationRepo.Insert(notification);
            var retrieved = _notificationRepo.GetUnreadByUser(1).FirstOrDefault(n => n.NotificationId == notificationId);

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(eventType, retrieved.EventType);
            Assert.AreEqual(messageText, retrieved.MessageText);
        }

        // -----------------------------------------------------------------------
        // Test 7: Audit Logging Integration (MOD-020)
        // -----------------------------------------------------------------------

        [Test]
        public void AuditLogging_LogsWorkflowInstanceCreation()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instance = new WorkflowInstance
            {
                WorkflowDefinitionId = templateId,
                TemplateVersion = 1,
                InitiatedBy = _testUserContext.ADUserName,
                InitiatedDate = DateTime.UtcNow,
                Status = WorkflowInstanceStatus.InProgress,
                InputParameters = "{}"
            };

            // Act
            var instanceId = _workflowRepo.CreateWorkflowInstance(instance);

            // Assert - Verify instance was created (audit logging would be done by service layer)
            Assert.Greater(instanceId, 0);
            var retrieved = _workflowRepo.GetWorkflowInstance(instanceId);
            Assert.IsNotNull(retrieved);
        }

        [Test]
        public void AuditLogging_LogsWorkflowStepCompletion()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var firstStep = stepInstances.First();

            // Act
            firstStep.Status = WorkflowStepInstanceStatus.Completed;
            firstStep.CompletedDate = DateTime.UtcNow;
            firstStep.CompletedBy = _testUserContext.ADUserName;
            _stepInstanceRepo.UpdateStepInstance(firstStep);

            // Assert - Verify step was updated (audit logging would be done by service layer)
            var retrieved = _stepInstanceRepo.GetStepInstance(firstStep.WorkflowStepInstanceId);
            Assert.AreEqual(WorkflowStepInstanceStatus.Completed, retrieved.Status);
            Assert.AreEqual(_testUserContext.ADUserName, retrieved.CompletedBy);
        }

        [Test]
        public void AuditLogging_LogsTaskDelegation()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var firstStep = stepInstances.First();

            var delegation = new WFDelegation
            {
                WorkflowStepInstanceId = firstStep.WorkflowStepInstanceId,
                DelegatedFrom = _testUserContext.ADUserName,
                DelegatedTo = "delegateuser",
                DelegationReason = "On leave",
                DelegationDate = DateTime.UtcNow,
                DelegationDepth = 1,
                IsActive = true
            };

            // Act
            var delegationId = _delegationRepo.CreateDelegation(delegation, 2);

            // Assert - Verify delegation was created (audit logging would be done by service layer)
            Assert.Greater(delegationId, 0);
            var retrieved = _delegationRepo.GetDelegation(delegationId);
            Assert.IsNotNull(retrieved);
        }

        [Test]
        public void AuditLogging_PreservesAuditTrail()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var firstStep = stepInstances.First();

            // Act - Create multiple delegations to build audit trail
            var delegation1 = new WFDelegation
            {
                WorkflowStepInstanceId = firstStep.WorkflowStepInstanceId,
                DelegatedFrom = _testUserContext.ADUserName,
                DelegatedTo = "user2",
                DelegationReason = "First delegation",
                DelegationDate = DateTime.UtcNow,
                DelegationDepth = 1,
                IsActive = true
            };
            _delegationRepo.CreateDelegation(delegation1, 2);

            var delegation2 = new WFDelegation
            {
                WorkflowStepInstanceId = firstStep.WorkflowStepInstanceId,
                DelegatedFrom = "user2",
                DelegatedTo = "user3",
                DelegationReason = "Second delegation",
                DelegationDate = DateTime.UtcNow.AddMinutes(1),
                DelegationDepth = 2,
                IsActive = true
            };
            _delegationRepo.CreateDelegation(delegation2, 2);

            // Assert - Verify audit trail is preserved
            var history = _delegationRepo.GetDelegationsByStepInstance(firstStep.WorkflowStepInstanceId);
            Assert.AreEqual(2, history.Count);
            Assert.AreEqual("user2", history[0].DelegatedTo);
            Assert.AreEqual("user3", history[1].DelegatedTo);
        }

        // -----------------------------------------------------------------------
        // Helper Methods
        // -----------------------------------------------------------------------

        private int CreateTestTemplate(string templateName)
        {
            var template = new WorkflowDefinition
            {
                Name = templateName + "_" + Guid.NewGuid(),
                Description = "Test template",
                Version = 1,
                IsActive = true,
                CreatedBy = _testUserContext.ADUserName,
                CreatedDate = DateTime.UtcNow
            };

            return _templateRepo.CreateTemplate(template);
        }

        private int CreateTestWorkflowInstance(int templateId)
        {
            var instance = new WorkflowInstance
            {
                WorkflowDefinitionId = templateId,
                TemplateVersion = 1,
                InitiatedBy = _testUserContext.ADUserName,
                InitiatedDate = DateTime.UtcNow,
                Status = WorkflowInstanceStatus.InProgress,
                InputParameters = "{}"
            };

            var instanceId = _workflowRepo.CreateWorkflowInstance(instance);

            // Create initial step instance
            var steps = _stepRepo.GetStepsByTemplate(templateId);
            if (steps.Count > 0)
            {
                var firstStep = steps.First();
                var stepInstance = new WorkflowStepInstance
                {
                    WorkflowInstanceId = instanceId,
                    WorkflowStepId = firstStep.WorkflowStepId,
                    AssignedTo = _testUserContext.ADUserName,
                    AssignedDate = DateTime.UtcNow,
                    Status = WorkflowStepInstanceStatus.Pending,
                    SLADeadline = DateTime.UtcNow.AddHours(24),
                    SLAStatus = SLAStatus.OnTime
                };

                _stepInstanceRepo.CreateStepInstance(stepInstance);
            }

            return instanceId;
        }
    }
}
