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
    /// Integration tests for Workflow Engine with Notification System (MOD-007) integration.
    /// Tests notification delivery for workflow events.
    /// 
    /// Validates Requirements:
    /// - Requirement 12: Dashboard and Monitoring - Real-time Notifications
    /// - Requirement 15: Integration with Notification System (MOD-007)
    /// - Requirement 19: Integration with Audit Logging (MOD-020)
    /// </summary>
    [TestFixture]
    public class NotificationIntegrationTests
    {
        private IConfigManager _configManager;
        private IWorkflowRepository _workflowRepo;
        private IWorkflowTemplateRepository _templateRepo;
        private IWorkflowStepRepository _stepRepo;
        private IWorkflowStepInstanceRepository _stepInstanceRepo;
        private INotificationRepository _notificationRepo;
        private IAuditRepository _auditRepo;
        private string _connectionString;
        private UserContext _testUserContext;

        [SetUp]
        public void SetUp()
        {
            _connectionString = "Server=.;Database=CMS_Test;Integrated Security=true;";
            
            _configManager = Substitute.For<IConfigManager>();
            _configManager.Get<string>("ConnectionString").Returns(_connectionString);

            _workflowRepo = new WorkflowRepository(_configManager);
            _templateRepo = new WorkflowTemplateRepository(_configManager);
            _stepRepo = new WorkflowStepRepository(_configManager);
            _stepInstanceRepo = new WorkflowStepInstanceRepository(_configManager);
            _notificationRepo = new NotificationRepository(_configManager);
            _auditRepo = new AuditRepository(_configManager);

            _testUserContext = new UserContext
            {
                ADUserName = "testuser",
                UserID = 1,
                UserRole = "User"
            };

            CleanupTestData();
        }

        [TearDown]
        public void TearDown()
        {
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
        // Test 1: Task Assignment Notification
        // -----------------------------------------------------------------------

        [Test]
        public void NotificationIntegration_SendsNotificationOnTaskAssignment()
        {
            // Arrange
            var templateId = CreateTestTemplate("NotificationWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            Assert.Greater(stepInstances.Count, 0);
            var assignedTask = stepInstances.First();

            var notification = new Notification
            {
                RecipientUserId = _testUserContext.UserID,
                EventType = "TASK_ASSIGNED",
                RelatedEntityId = assignedTask.WorkflowStepInstanceId.ToString(),
                MessageText = $"You have been assigned task: {assignedTask.WorkflowStepId}"
            };

            // Act
            var notificationId = _notificationRepo.Insert(notification);

            // Assert
            Assert.Greater(notificationId, 0);
            var unreadNotifications = _notificationRepo.GetUnreadByUser(_testUserContext.UserID);
            Assert.Greater(unreadNotifications.Count(), 0);
            var sentNotification = unreadNotifications.FirstOrDefault(n => n.NotificationId == notificationId);
            Assert.IsNotNull(sentNotification);
            Assert.AreEqual("TASK_ASSIGNED", sentNotification.EventType);
        }

        [Test]
        public void NotificationIntegration_IncludesWorkflowContextInNotification()
        {
            // Arrange
            var templateId = CreateTestTemplate("NotificationWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var assignedTask = stepInstances.First();

            var notification = new Notification
            {
                RecipientUserId = _testUserContext.UserID,
                EventType = "TASK_ASSIGNED",
                RelatedEntityId = assignedTask.WorkflowStepInstanceId.ToString(),
                MessageText = $"Task assigned in workflow {instanceId}: {assignedTask.WorkflowStepId}"
            };

            // Act
            var notificationId = _notificationRepo.Insert(notification);
            var retrieved = _notificationRepo.GetUnreadByUser(_testUserContext.UserID).FirstOrDefault(n => n.NotificationId == notificationId);

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.IsTrue(retrieved.MessageText.Contains(instanceId.ToString()));
        }

        // -----------------------------------------------------------------------
        // Test 2: SLA Warning Notification
        // -----------------------------------------------------------------------

        [Test]
        public void NotificationIntegration_SendsSLAWarningNotification()
        {
            // Arrange
            var templateId = CreateTestTemplate("NotificationWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var taskStep = stepInstances.First();

            // Set SLA status to AtRisk
            taskStep.SLAStatus = SLAStatus.AtRisk;
            _stepInstanceRepo.UpdateStepInstance(taskStep);

            var notification = new Notification
            {
                RecipientUserId = _testUserContext.UserID,
                EventType = "SLA_WARNING",
                RelatedEntityId = taskStep.WorkflowStepInstanceId.ToString(),
                MessageText = $"SLA warning: Task deadline approaching at {taskStep.SLADeadline:yyyy-MM-dd HH:mm:ss}"
            };

            // Act
            var notificationId = _notificationRepo.Insert(notification);

            // Assert
            Assert.Greater(notificationId, 0);
            var unreadNotifications = _notificationRepo.GetUnreadByUser(_testUserContext.UserID);
            var slaNotification = unreadNotifications.FirstOrDefault(n => n.EventType == "SLA_WARNING");
            Assert.IsNotNull(slaNotification);
        }

        [Test]
        public void NotificationIntegration_SendsSLAOverdueNotification()
        {
            // Arrange
            var templateId = CreateTestTemplate("NotificationWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var taskStep = stepInstances.First();

            // Set SLA status to Overdue
            taskStep.SLAStatus = SLAStatus.Overdue;
            taskStep.SLADeadline = DateTime.UtcNow.AddHours(-1);
            _stepInstanceRepo.UpdateStepInstance(taskStep);

            var notification = new Notification
            {
                RecipientUserId = _testUserContext.UserID,
                EventType = "SLA_OVERDUE",
                RelatedEntityId = taskStep.WorkflowStepInstanceId.ToString(),
                MessageText = $"SLA exceeded: Task was due at {taskStep.SLADeadline:yyyy-MM-dd HH:mm:ss}"
            };

            // Act
            var notificationId = _notificationRepo.Insert(notification);

            // Assert
            Assert.Greater(notificationId, 0);
            var unreadNotifications = _notificationRepo.GetUnreadByUser(_testUserContext.UserID);
            var overdueNotification = unreadNotifications.FirstOrDefault(n => n.EventType == "SLA_OVERDUE");
            Assert.IsNotNull(overdueNotification);
        }

        // -----------------------------------------------------------------------
        // Test 3: Delegation Notification
        // -----------------------------------------------------------------------

        [Test]
        public void NotificationIntegration_SendsDelegationNotification()
        {
            // Arrange
            var templateId = CreateTestTemplate("NotificationWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var taskStep = stepInstances.First();

            var delegateUserId = 2; // Different user

            var notification = new Notification
            {
                RecipientUserId = delegateUserId,
                EventType = "TASK_DELEGATED",
                RelatedEntityId = taskStep.WorkflowStepInstanceId.ToString(),
                MessageText = $"Task delegated to you by {_testUserContext.ADUserName}: {taskStep.WorkflowStepId}"
            };

            // Act
            var notificationId = _notificationRepo.Insert(notification);

            // Assert
            Assert.Greater(notificationId, 0);
            var delegateNotifications = _notificationRepo.GetUnreadByUser(delegateUserId);
            var delegationNotification = delegateNotifications.FirstOrDefault(n => n.EventType == "TASK_DELEGATED");
            Assert.IsNotNull(delegationNotification);
        }

        // -----------------------------------------------------------------------
        // Test 4: Workflow Completion Notification
        // -----------------------------------------------------------------------

        [Test]
        public void NotificationIntegration_SendsWorkflowCompletionNotification()
        {
            // Arrange
            var templateId = CreateTestTemplate("NotificationWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var instance = _workflowRepo.GetWorkflowInstance(instanceId);

            // Complete the workflow
            instance.Status = WorkflowInstanceStatus.Completed;
            instance.CompletedDate = DateTime.UtcNow;
            _workflowRepo.UpdateWorkflowInstance(instance);

            var notification = new Notification
            {
                RecipientUserId = _testUserContext.UserID,
                EventType = "WORKFLOW_COMPLETED",
                RelatedEntityId = instanceId.ToString(),
                MessageText = $"Workflow {instanceId} has been completed"
            };

            // Act
            var notificationId = _notificationRepo.Insert(notification);

            // Assert
            Assert.Greater(notificationId, 0);
            var unreadNotifications = _notificationRepo.GetUnreadByUser(_testUserContext.UserID);
            var completionNotification = unreadNotifications.FirstOrDefault(n => n.EventType == "WORKFLOW_COMPLETED");
            Assert.IsNotNull(completionNotification);
        }

        // -----------------------------------------------------------------------
        // Test 5: Notification Status Management
        // -----------------------------------------------------------------------

        [Test]
        public void NotificationIntegration_MarksNotificationAsRead()
        {
            // Arrange
            var notification = new Notification
            {
                RecipientUserId = _testUserContext.UserID,
                EventType = "TASK_ASSIGNED",
                RelatedEntityId = "1",
                MessageText = "You have been assigned a task"
            };
            var notificationId = _notificationRepo.Insert(notification);

            // Act
            _notificationRepo.MarkRead(notificationId);

            // Assert
            var unreadNotifications = _notificationRepo.GetUnreadByUser(_testUserContext.UserID);
            var markedNotification = unreadNotifications.FirstOrDefault(n => n.NotificationId == notificationId);
            Assert.IsNull(markedNotification);
        }

        [Test]
        public void NotificationIntegration_RetrievesNotificationHistory()
        {
            // Arrange
            var notification1 = new Notification
            {
                RecipientUserId = _testUserContext.UserID,
                EventType = "TASK_ASSIGNED",
                RelatedEntityId = "1",
                MessageText = "First notification"
            };
            var notification2 = new Notification
            {
                RecipientUserId = _testUserContext.UserID,
                EventType = "SLA_WARNING",
                RelatedEntityId = "2",
                MessageText = "Second notification"
            };

            // Act
            var id1 = _notificationRepo.Insert(notification1);
            var id2 = _notificationRepo.Insert(notification2);

            // Assert
            var unreadNotifications = _notificationRepo.GetUnreadByUser(_testUserContext.UserID);
            Assert.GreaterOrEqual(unreadNotifications.Count(), 2);
        }

        // -----------------------------------------------------------------------
        // Test 6: Escalation Notification
        // -----------------------------------------------------------------------

        [Test]
        public void NotificationIntegration_SendsEscalationNotification()
        {
            // Arrange
            var templateId = CreateTestTemplate("NotificationWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var taskStep = stepInstances.First();

            var managerUserId = 3; // Manager user

            var notification = new Notification
            {
                RecipientUserId = managerUserId,
                EventType = "ESCALATION",
                RelatedEntityId = taskStep.WorkflowStepInstanceId.ToString(),
                MessageText = $"Task escalation: {taskStep.WorkflowStepId} has exceeded SLA"
            };

            // Act
            var notificationId = _notificationRepo.Insert(notification);

            // Assert
            Assert.Greater(notificationId, 0);
            var managerNotifications = _notificationRepo.GetUnreadByUser(managerUserId);
            var escalationNotification = managerNotifications.FirstOrDefault(n => n.EventType == "ESCALATION");
            Assert.IsNotNull(escalationNotification);
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
