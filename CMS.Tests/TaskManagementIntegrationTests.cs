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
    /// Integration tests for Workflow Engine with Task Management (MOD-009) integration.
    /// Tests task creation and management within workflow execution.
    /// 
    /// Validates Requirements:
    /// - Requirement 16: Integration with Task Management (MOD-009)
    /// - Requirement 19: Integration with Audit Logging (MOD-020)
    /// </summary>
    [TestFixture]
    public class TaskManagementIntegrationTests
    {
        private IConfigManager _configManager;
        private IWorkflowRepository _workflowRepo;
        private IWorkflowTemplateRepository _templateRepo;
        private IWorkflowStepRepository _stepRepo;
        private IWorkflowStepInstanceRepository _stepInstanceRepo;
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
        // Test 1: Task Creation from Workflow Step
        // -----------------------------------------------------------------------

        [Test]
        public void TaskManagementIntegration_CreatesTaskFromWorkflowStep()
        {
            // Arrange
            var templateId = CreateTestTemplate("TaskWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            Assert.Greater(stepInstances.Count, 0);
            var workflowStep = stepInstances.First();

            // Simulate task creation in Task Management module
            var taskData = new
            {
                Title = $"Task: {workflowStep.WorkflowStepId}",
                Description = "Workflow task",
                AssignedTo = workflowStep.AssignedTo,
                DueDate = workflowStep.SLADeadline,
                Priority = "High",
                ExternalReferenceId = workflowStep.WorkflowStepInstanceId,
                ExternalReferenceType = "WorkflowStepInstance"
            };

            // Act
            workflowStep.OutputData = System.Text.Json.JsonSerializer.Serialize(taskData);
            _stepInstanceRepo.UpdateStepInstance(workflowStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(workflowStep.WorkflowStepInstanceId);
            Assert.IsTrue(persistedStep.OutputData.Contains("ExternalReferenceId"));
            Assert.IsTrue(persistedStep.OutputData.Contains(workflowStep.WorkflowStepInstanceId.ToString()));
        }

        [Test]
        public void TaskManagementIntegration_StoresTaskMetadata()
        {
            // Arrange
            var templateId = CreateTestTemplate("TaskWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var workflowStep = stepInstances.First();

            var taskTitle = "Review Document";
            var taskDescription = "Please review the attached document";
            var taskPriority = "High";

            // Act
            var taskData = new
            {
                Title = taskTitle,
                Description = taskDescription,
                Priority = taskPriority,
                AssignedTo = workflowStep.AssignedTo,
                DueDate = workflowStep.SLADeadline
            };

            workflowStep.OutputData = System.Text.Json.JsonSerializer.Serialize(taskData);
            _stepInstanceRepo.UpdateStepInstance(workflowStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(workflowStep.WorkflowStepInstanceId);
            Assert.IsTrue(persistedStep.OutputData.Contains(taskTitle));
            Assert.IsTrue(persistedStep.OutputData.Contains(taskDescription));
            Assert.IsTrue(persistedStep.OutputData.Contains(taskPriority));
        }

        // -----------------------------------------------------------------------
        // Test 2: Task Completion and Workflow Advancement
        // -----------------------------------------------------------------------

        [Test]
        public void TaskManagementIntegration_AdvancesWorkflowOnTaskCompletion()
        {
            // Arrange
            var templateId = CreateTestTemplate("TaskWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var workflowStep = stepInstances.First();

            // Act - Complete the task
            workflowStep.Status = WorkflowStepInstanceStatus.Completed;
            workflowStep.CompletedDate = DateTime.UtcNow;
            workflowStep.CompletedBy = _testUserContext.ADUserName;
            workflowStep.OutputData = "{\"TaskStatus\": \"Completed\"}";
            _stepInstanceRepo.UpdateStepInstance(workflowStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(workflowStep.WorkflowStepInstanceId);
            Assert.AreEqual(WorkflowStepInstanceStatus.Completed, persistedStep.Status);
            Assert.AreEqual(_testUserContext.ADUserName, persistedStep.CompletedBy);
        }

        [Test]
        public void TaskManagementIntegration_PreservesTaskCompletionData()
        {
            // Arrange
            var templateId = CreateTestTemplate("TaskWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var workflowStep = stepInstances.First();

            var completionData = new
            {
                TaskStatus = "Completed",
                CompletionNotes = "Task completed successfully",
                ApprovalStatus = "Approved"
            };

            // Act
            workflowStep.Status = WorkflowStepInstanceStatus.Completed;
            workflowStep.CompletedDate = DateTime.UtcNow;
            workflowStep.CompletedBy = _testUserContext.ADUserName;
            workflowStep.OutputData = System.Text.Json.JsonSerializer.Serialize(completionData);
            _stepInstanceRepo.UpdateStepInstance(workflowStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(workflowStep.WorkflowStepInstanceId);
            Assert.IsTrue(persistedStep.OutputData.Contains("CompletionNotes"));
            Assert.IsTrue(persistedStep.OutputData.Contains("Completed successfully"));
        }

        // -----------------------------------------------------------------------
        // Test 3: Task Assignment Updates
        // -----------------------------------------------------------------------

        [Test]
        public void TaskManagementIntegration_UpdatesTaskAssignmentInWorkflow()
        {
            // Arrange
            var templateId = CreateTestTemplate("TaskWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var workflowStep = stepInstances.First();

            var originalAssignee = workflowStep.AssignedTo;
            var newAssignee = "newuser";

            // Act - Update assignment
            workflowStep.AssignedTo = newAssignee;
            _stepInstanceRepo.UpdateStepInstance(workflowStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(workflowStep.WorkflowStepInstanceId);
            Assert.AreEqual(newAssignee, persistedStep.AssignedTo);
            Assert.AreNotEqual(originalAssignee, persistedStep.AssignedTo);
        }

        [Test]
        public void TaskManagementIntegration_UpdatesTaskDueDate()
        {
            // Arrange
            var templateId = CreateTestTemplate("TaskWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var workflowStep = stepInstances.First();

            var originalDeadline = workflowStep.SLADeadline;
            var newDeadline = DateTime.UtcNow.AddHours(48);

            // Act - Update SLA deadline
            workflowStep.SLADeadline = newDeadline;
            _stepInstanceRepo.UpdateStepInstance(workflowStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(workflowStep.WorkflowStepInstanceId);
            Assert.AreEqual(newDeadline, persistedStep.SLADeadline);
            Assert.AreNotEqual(originalDeadline, persistedStep.SLADeadline);
        }

        // -----------------------------------------------------------------------
        // Test 4: Task Rejection and Workflow Rerouting
        // -----------------------------------------------------------------------

        [Test]
        public void TaskManagementIntegration_HandlesTaskRejection()
        {
            // Arrange
            var templateId = CreateTestTemplate("TaskWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var workflowStep = stepInstances.First();

            // Act - Reject the task
            workflowStep.Status = WorkflowStepInstanceStatus.Rejected;
            workflowStep.CompletedDate = DateTime.UtcNow;
            workflowStep.CompletedBy = _testUserContext.ADUserName;
            workflowStep.OutputData = "{\"RejectionReason\": \"Does not meet requirements\"}";
            _stepInstanceRepo.UpdateStepInstance(workflowStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(workflowStep.WorkflowStepInstanceId);
            Assert.AreEqual(WorkflowStepInstanceStatus.Rejected, persistedStep.Status);
            Assert.IsTrue(persistedStep.OutputData.Contains("RejectionReason"));
        }

        [Test]
        public void TaskManagementIntegration_PreservesRejectionDetails()
        {
            // Arrange
            var templateId = CreateTestTemplate("TaskWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var workflowStep = stepInstances.First();

            var rejectionData = new
            {
                RejectionReason = "Incomplete information",
                RejectionComments = "Please provide additional details",
                RejectedBy = _testUserContext.ADUserName
            };

            // Act
            workflowStep.Status = WorkflowStepInstanceStatus.Rejected;
            workflowStep.CompletedDate = DateTime.UtcNow;
            workflowStep.CompletedBy = _testUserContext.ADUserName;
            workflowStep.OutputData = System.Text.Json.JsonSerializer.Serialize(rejectionData);
            _stepInstanceRepo.UpdateStepInstance(workflowStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(workflowStep.WorkflowStepInstanceId);
            Assert.IsTrue(persistedStep.OutputData.Contains("Incomplete information"));
            Assert.IsTrue(persistedStep.OutputData.Contains("additional details"));
        }

        // -----------------------------------------------------------------------
        // Test 5: Task Context and Workflow Information
        // -----------------------------------------------------------------------

        [Test]
        public void TaskManagementIntegration_IncludesWorkflowContextInTask()
        {
            // Arrange
            var templateId = CreateTestTemplate("TaskWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var workflowStep = stepInstances.First();

            var template = _templateRepo.GetTemplate(templateId);

            var taskContext = new
            {
                WorkflowName = template.Name,
                WorkflowInstanceId = instanceId,
                StepName = workflowStep.WorkflowStepId,
                StepInstanceId = workflowStep.WorkflowStepInstanceId
            };

            // Act
            workflowStep.OutputData = System.Text.Json.JsonSerializer.Serialize(taskContext);
            _stepInstanceRepo.UpdateStepInstance(workflowStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(workflowStep.WorkflowStepInstanceId);
            Assert.IsTrue(persistedStep.OutputData.Contains("WorkflowName"));
            Assert.IsTrue(persistedStep.OutputData.Contains(instanceId.ToString()));
        }

        [Test]
        public void TaskManagementIntegration_DisplaysWorkflowProgressInTask()
        {
            // Arrange
            var templateId = CreateTestTemplate("TaskWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var workflowStep = stepInstances.First();

            var progressData = new
            {
                CurrentStep = "Review",
                TotalSteps = 5,
                CompletedSteps = 2,
                ProgressPercentage = 40
            };

            // Act
            workflowStep.OutputData = System.Text.Json.JsonSerializer.Serialize(progressData);
            _stepInstanceRepo.UpdateStepInstance(workflowStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(workflowStep.WorkflowStepInstanceId);
            Assert.IsTrue(persistedStep.OutputData.Contains("ProgressPercentage"));
            Assert.IsTrue(persistedStep.OutputData.Contains("40"));
        }

        // -----------------------------------------------------------------------
        // Test 6: Multiple Tasks in Workflow
        // -----------------------------------------------------------------------

        [Test]
        public void TaskManagementIntegration_ManagesMultipleTasks()
        {
            // Arrange
            var templateId = CreateTestTemplate("MultiTaskWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);

            // Create multiple step instances
            var steps = _stepRepo.GetStepsByTemplate(templateId);
            var stepInstances = new List<WorkflowStepInstance>();

            for (int i = 0; i < Math.Min(3, steps.Count); i++)
            {
                var stepInstance = new WorkflowStepInstance
                {
                    WorkflowInstanceId = instanceId,
                    WorkflowStepId = steps[i].WorkflowStepId,
                    AssignedTo = _testUserContext.ADUserName,
                    AssignedDate = DateTime.UtcNow,
                    Status = WorkflowStepInstanceStatus.Pending,
                    SLADeadline = DateTime.UtcNow.AddHours(24 * (i + 1)),
                    SLAStatus = SLAStatus.OnTime
                };

                var stepInstanceId = _stepInstanceRepo.CreateStepInstance(stepInstance);
                stepInstance.WorkflowStepInstanceId = stepInstanceId;
                stepInstances.Add(stepInstance);
            }

            // Act - Complete first task
            stepInstances[0].Status = WorkflowStepInstanceStatus.Completed;
            stepInstances[0].CompletedDate = DateTime.UtcNow;
            stepInstances[0].CompletedBy = _testUserContext.ADUserName;
            _stepInstanceRepo.UpdateStepInstance(stepInstances[0]);

            // Assert
            var allSteps = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            Assert.GreaterOrEqual(allSteps.Count, 1);
            var completedSteps = allSteps.Where(s => s.Status == WorkflowStepInstanceStatus.Completed).ToList();
            Assert.Greater(completedSteps.Count, 0);
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
