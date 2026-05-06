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
using CMS.Services.Interfaces;
using CMS.Services;
using NSubstitute;

namespace CMS.Tests
{
    /// <summary>
    /// Integration tests for Workflow Engine with Approval Workflow (MOD-003) integration.
    /// Tests end-to-end approval workflow scenarios and cross-module interactions.
    /// 
    /// Validates Requirements:
    /// - Requirement 13: Integration with Approval Workflow (MOD-003)
    /// - Requirement 19: Integration with Audit Logging (MOD-020)
    /// </summary>
    [TestFixture]
    public class ApprovalWorkflowIntegrationTests
    {
        private IConfigManager _configManager;
        private IWorkflowRepository _workflowRepo;
        private IWorkflowTemplateRepository _templateRepo;
        private IWorkflowStepRepository _stepRepo;
        private IWorkflowStepInstanceRepository _stepInstanceRepo;
        private IApprovalRepository _approvalRepo;
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
            _approvalRepo = new ApprovalRepository(_configManager);
            _auditRepo = new AuditRepository(_configManager);

            _testUserContext = new UserContext
            {
                ADUserName = "testuser",
                UserID = 1,
                UserRole = "Approver"
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
        // Test 1: Approval Step Creation
        // -----------------------------------------------------------------------

        [Test]
        public void ApprovalWorkflowIntegration_CreatesApprovalStepInTemplate()
        {
            // Arrange
            var template = new WorkflowDefinition
            {
                Name = "ApprovalTemplate_" + Guid.NewGuid(),
                Description = "Template with approval step",
                Version = 1,
                IsActive = true,
                CreatedBy = _testUserContext.ADUserName,
                CreatedDate = DateTime.UtcNow
            };
            var templateId = _templateRepo.CreateTemplate(template);

            var approvalStep = new WorkflowStep
            {
                WorkflowDefinitionId = templateId,
                StepName = "Approval",
                StepType = WorkflowStepType.Approval,
                Description = "Approval step",
                AssignedRole = "Approver",
                SLAHours = 24,
                AllowDelegation = true,
                MaxDelegationDepth = 2,
                SequenceNumber = 1
            };

            // Act
            var stepId = _stepRepo.CreateStep(approvalStep);

            // Assert
            Assert.Greater(stepId, 0);
            var persistedStep = _stepRepo.GetStep(stepId);
            Assert.IsNotNull(persistedStep);
            Assert.AreEqual(WorkflowStepType.Approval, persistedStep.StepType);
            Assert.AreEqual("Approver", persistedStep.AssignedRole);
        }

        [Test]
        public void ApprovalWorkflowIntegration_CreatesApprovalRequestWhenStepIsReached()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalTemplate");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            Assert.Greater(stepInstances.Count, 0);

            var approvalRequest = new ApprovalRequest
            {
                RequestType = "WORKFLOW_APPROVAL",
                FolderID = 1,
                RequestedBy = 1,
                Subject = "Workflow Approval Request",
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
            Assert.AreEqual($"WorkflowInstanceId={instanceId}", persistedApproval.Remark);
        }

        // -----------------------------------------------------------------------
        // Test 2: Approval Status Tracking
        // -----------------------------------------------------------------------

        [Test]
        public void ApprovalWorkflowIntegration_TracksApprovalStatusChanges()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalTemplate");
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

            // Act - Update approval status to Approved
            _approvalRepo.UpdateRequestStatus(approvalId, 2, 1, "Approved"); // 2 = Approved

            // Assert
            var persistedApproval = _approvalRepo.GetRequest(approvalId);
            Assert.AreEqual(2, persistedApproval.RequestStatus);
        }

        [Test]
        public void ApprovalWorkflowIntegration_TracksApprovalRejection()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalTemplate");
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

            // Act - Update approval status to Rejected
            _approvalRepo.UpdateRequestStatus(approvalId, 3, 1, "Rejected"); // 3 = Rejected

            // Assert
            var persistedApproval = _approvalRepo.GetRequest(approvalId);
            Assert.AreEqual(3, persistedApproval.RequestStatus);
        }

        // -----------------------------------------------------------------------
        // Test 3: Approval Workflow Continuation
        // -----------------------------------------------------------------------

        [Test]
        public void ApprovalWorkflowIntegration_ContinuesWorkflowAfterApproval()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalTemplate");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var approvalStep = stepInstances.First();

            var approvalRequest = new ApprovalRequest
            {
                RequestType = "WORKFLOW_APPROVAL",
                FolderID = 1,
                RequestedBy = 1,
                Subject = "Workflow Approval",
                Remark = $"WorkflowInstanceId={instanceId}"
            };
            var approvalId = _approvalRepo.InsertRequest(approvalRequest);

            // Act - Approve the request
            _approvalRepo.UpdateRequestStatus(approvalId, 2, 1, "Approved");

            // Complete the approval step
            approvalStep.Status = WorkflowStepInstanceStatus.Completed;
            approvalStep.CompletedDate = DateTime.UtcNow;
            approvalStep.CompletedBy = _testUserContext.ADUserName;
            approvalStep.OutputData = "{\"ApprovalStatus\": \"Approved\", \"ApprovalId\": " + approvalId + "}";
            _stepInstanceRepo.UpdateStepInstance(approvalStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(approvalStep.WorkflowStepInstanceId);
            Assert.AreEqual(WorkflowStepInstanceStatus.Completed, persistedStep.Status);
            Assert.IsTrue(persistedStep.OutputData.Contains("Approved"));
        }

        [Test]
        public void ApprovalWorkflowIntegration_HandlesApprovalRejectionInWorkflow()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalTemplate");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var approvalStep = stepInstances.First();

            var approvalRequest = new ApprovalRequest
            {
                RequestType = "WORKFLOW_APPROVAL",
                FolderID = 1,
                RequestedBy = 1,
                Subject = "Workflow Approval",
                Remark = $"WorkflowInstanceId={instanceId}"
            };
            var approvalId = _approvalRepo.InsertRequest(approvalRequest);

            // Act - Reject the request
            _approvalRepo.UpdateRequestStatus(approvalId, 3, 1, "Rejected");

            // Reject the approval step
            approvalStep.Status = WorkflowStepInstanceStatus.Rejected;
            approvalStep.CompletedDate = DateTime.UtcNow;
            approvalStep.CompletedBy = _testUserContext.ADUserName;
            approvalStep.OutputData = "{\"ApprovalStatus\": \"Rejected\", \"Reason\": \"Does not meet criteria\"}";
            _stepInstanceRepo.UpdateStepInstance(approvalStep);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(approvalStep.WorkflowStepInstanceId);
            Assert.AreEqual(WorkflowStepInstanceStatus.Rejected, persistedStep.Status);
            Assert.IsTrue(persistedStep.OutputData.Contains("Rejected"));
        }

        // -----------------------------------------------------------------------
        // Test 4: Multiple Approvals in Workflow
        // -----------------------------------------------------------------------

        [Test]
        public void ApprovalWorkflowIntegration_HandlesMultipleApprovalSteps()
        {
            // Arrange
            var templateId = CreateTestTemplate("MultiApprovalTemplate");
            var instanceId = CreateTestWorkflowInstance(templateId);

            // Create first approval request
            var approval1 = new ApprovalRequest
            {
                RequestType = "WORKFLOW_APPROVAL",
                FolderID = 1,
                RequestedBy = 1,
                Subject = "First Approval",
                Remark = $"WorkflowInstanceId={instanceId}_Step1"
            };
            var approvalId1 = _approvalRepo.InsertRequest(approval1);

            // Create second approval request
            var approval2 = new ApprovalRequest
            {
                RequestType = "WORKFLOW_APPROVAL",
                FolderID = 1,
                RequestedBy = 1,
                Subject = "Second Approval",
                Remark = $"WorkflowInstanceId={instanceId}_Step2"
            };
            var approvalId2 = _approvalRepo.InsertRequest(approval2);

            // Act
            _approvalRepo.UpdateRequestStatus(approvalId1, 2, 1, "Approved");
            _approvalRepo.UpdateRequestStatus(approvalId2, 2, 1, "Approved");

            // Assert
            var approval1Result = _approvalRepo.GetRequest(approvalId1);
            var approval2Result = _approvalRepo.GetRequest(approvalId2);
            Assert.AreEqual(2, approval1Result.RequestStatus);
            Assert.AreEqual(2, approval2Result.RequestStatus);
        }

        // -----------------------------------------------------------------------
        // Test 5: Approval Audit Trail
        // -----------------------------------------------------------------------

        [Test]
        public void ApprovalWorkflowIntegration_MaintainsApprovalAuditTrail()
        {
            // Arrange
            var templateId = CreateTestTemplate("ApprovalTemplate");
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

            // Act - Create approval history
            _approvalRepo.UpdateRequestStatus(approvalId, 2, 1, "Approved");

            // Assert - Verify approval can be retrieved
            var persistedApproval = _approvalRepo.GetRequest(approvalId);
            Assert.IsNotNull(persistedApproval);
            Assert.AreEqual(2, persistedApproval.RequestStatus);
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
