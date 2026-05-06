using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NSubstitute;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Infrastructure.Interfaces;
using CMS.Services;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using CMS.Services.Validators;

namespace CMS.Tests
{
    /// <summary>
    /// Unit tests for WorkflowService (MOD-008) Phase 15.
    /// Tests core workflow functionality with mocked dependencies.
    /// 
    /// Target Coverage: 80%+
    /// 
    /// Validates:
    /// - Workflow instance creation
    /// - Workflow step completion
    /// - Task assignment
    /// - State transitions
    /// - Permission checks
    /// - Validation logic
    /// </summary>
    [TestFixture]
    public class WorkflowServiceUnitTests
    {
        private WorkflowService _workflowService;
        private IWorkflowRepository _workflowRepo;
        private IWorkflowTemplateRepository _templateRepo;
        private IWorkflowStepRepository _stepRepo;
        private IWorkflowStepInstanceRepository _stepInstanceRepo;
        private IDelegationRepository _delegationRepo;
        private IAuditRepository _auditRepo;
        private INotificationRepository _notificationRepo;
        private IWorkflowPermissionService _permissionService;
        private IStepResolverService _stepResolverService;
        private IConditionEvaluatorService _conditionEvaluatorService;
        private IWorkflowNotificationService _notificationService;
        private ISLACalculator _slaCalculator;
        private ILoggingService _loggingService;
        private UserContext _testUserContext;

        [SetUp]
        public void SetUp()
        {
            // Create mock dependencies
            _workflowRepo = Substitute.For<IWorkflowRepository>();
            _templateRepo = Substitute.For<IWorkflowTemplateRepository>();
            _stepRepo = Substitute.For<IWorkflowStepRepository>();
            _stepInstanceRepo = Substitute.For<IWorkflowStepInstanceRepository>();
            _delegationRepo = Substitute.For<IDelegationRepository>();
            _auditRepo = Substitute.For<IAuditRepository>();
            _notificationRepo = Substitute.For<INotificationRepository>();
            _permissionService = Substitute.For<IWorkflowPermissionService>();
            _stepResolverService = Substitute.For<IStepResolverService>();
            _conditionEvaluatorService = Substitute.For<IConditionEvaluatorService>();
            _notificationService = Substitute.For<IWorkflowNotificationService>();
            _slaCalculator = Substitute.For<ISLACalculator>();
            _loggingService = Substitute.For<ILoggingService>();

            // Create service instance
            _workflowService = new WorkflowService(
                _workflowRepo,
                _templateRepo,
                _stepRepo,
                _stepInstanceRepo,
                _delegationRepo,
                _auditRepo,
                _notificationRepo,
                _permissionService,
                _stepResolverService,
                _conditionEvaluatorService,
                _notificationService,
                _slaCalculator,
                _loggingService
            );

            // Create test user context
            _testUserContext = new UserContext
            {
                ADUserName = "testuser",
                UserID = 1,
                UserRole = "Approver"
            };
        }

        #region CreateWorkflowInstance Tests

        [Test]
        public void CreateWorkflowInstance_WithValidRequest_CreatesInstanceSuccessfully()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);
            var request = new CreateWorkflowInstanceRequest
            {
                TemplateId = templateId,
                InitiatorId = _testUserContext.UserID,
                Parameters = new Dictionary<string, object> { { "key", "value" } }
            };

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanCreateWorkflowInstance(template, _testUserContext).Returns(true);
            _stepRepo.GetByTemplateId(templateId).Returns(new List<WorkflowStep> { CreateTestStep(1, templateId) });
            _workflowRepo.Create(Arg.Any<WorkflowInstance>()).Returns(1);
            _stepResolverService.ResolveFirstStep(Arg.Any<WorkflowDefinition>()).Returns(CreateTestStep(1, templateId));

            // Act
            var result = _workflowService.CreateWorkflowInstance(request, _testUserContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.InstanceId);
            _workflowRepo.Received(1).Create(Arg.Any<WorkflowInstance>());
            _auditRepo.Received(1).LogAction(Arg.Any<AuditLog>());
        }

        [Test]
        public void CreateWorkflowInstance_WithoutPermission_ThrowsException()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);
            var request = new CreateWorkflowInstanceRequest
            {
                TemplateId = templateId,
                InitiatorId = _testUserContext.UserID
            };

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanCreateWorkflowInstance(template, _testUserContext).Returns(false);

            // Act & Assert
            Assert.Throws<WorkflowPermissionException>(() =>
                _workflowService.CreateWorkflowInstance(request, _testUserContext)
            );
        }

        [Test]
        public void CreateWorkflowInstance_WithInvalidTemplate_ThrowsException()
        {
            // Arrange
            var templateId = 999;
            var request = new CreateWorkflowInstanceRequest
            {
                TemplateId = templateId,
                InitiatorId = _testUserContext.UserID
            };

            _templateRepo.GetById(templateId).Returns((WorkflowDefinition)null);

            // Act & Assert
            Assert.Throws<WorkflowValidationException>(() =>
                _workflowService.CreateWorkflowInstance(request, _testUserContext)
            );
        }

        [Test]
        public void CreateWorkflowInstance_WithMissingRequiredParameters_ThrowsException()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);
            template.RequiredParameters = new List<string> { "requiredParam" };
            var request = new CreateWorkflowInstanceRequest
            {
                TemplateId = templateId,
                InitiatorId = _testUserContext.UserID,
                Parameters = new Dictionary<string, object>() // Missing required parameter
            };

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanCreateWorkflowInstance(template, _testUserContext).Returns(true);

            // Act & Assert
            Assert.Throws<WorkflowValidationException>(() =>
                _workflowService.CreateWorkflowInstance(request, _testUserContext)
            );
        }

        #endregion

        #region GetWorkflowInstance Tests

        [Test]
        public void GetWorkflowInstance_WithValidId_ReturnsInstance()
        {
            // Arrange
            var instanceId = 1;
            var instance = CreateTestInstance(instanceId);
            var steps = new List<WorkflowStep> { CreateTestStep(1, 1) };

            _workflowRepo.GetById(instanceId).Returns(instance);
            _stepRepo.GetByTemplateId(instance.TemplateId).Returns(steps);
            _permissionService.CanViewWorkflowInstance(instance, _testUserContext).Returns(true);

            // Act
            var result = _workflowService.GetWorkflowInstance(instanceId, _testUserContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(instanceId, result.InstanceId);
        }

        [Test]
        public void GetWorkflowInstance_WithoutPermission_ThrowsException()
        {
            // Arrange
            var instanceId = 1;
            var instance = CreateTestInstance(instanceId);

            _workflowRepo.GetById(instanceId).Returns(instance);
            _permissionService.CanViewWorkflowInstance(instance, _testUserContext).Returns(false);

            // Act & Assert
            Assert.Throws<WorkflowPermissionException>(() =>
                _workflowService.GetWorkflowInstance(instanceId, _testUserContext)
            );
        }

        [Test]
        public void GetWorkflowInstance_WithInvalidId_ThrowsException()
        {
            // Arrange
            var instanceId = 999;
            _workflowRepo.GetById(instanceId).Returns((WorkflowInstance)null);

            // Act & Assert
            Assert.Throws<WorkflowValidationException>(() =>
                _workflowService.GetWorkflowInstance(instanceId, _testUserContext)
            );
        }

        #endregion

        #region CompleteWorkflowStep Tests

        [Test]
        public void CompleteWorkflowStep_WithValidRequest_CompletesStepSuccessfully()
        {
            // Arrange
            var stepInstanceId = 1;
            var stepInstance = CreateTestStepInstance(stepInstanceId, 1, WorkflowStepStatus.InProgress);
            var nextStep = CreateTestStep(2, 1);
            var request = new CompleteWorkflowStepRequest
            {
                StepInstanceId = stepInstanceId,
                OutputData = new Dictionary<string, object> { { "result", "approved" } }
            };

            _stepInstanceRepo.GetById(stepInstanceId).Returns(stepInstance);
            _permissionService.CanCompleteWorkflowStep(stepInstance, _testUserContext).Returns(true);
            _conditionEvaluatorService.EvaluateConditions(Arg.Any<List<WorkflowConnection>>(), Arg.Any<Dictionary<string, object>>())
                .Returns(new List<WorkflowConnection> { CreateTestConnection(2, 1) });
            _stepResolverService.ResolveNextStep(Arg.Any<WorkflowConnection>()).Returns(nextStep);

            // Act
            var result = _workflowService.CompleteWorkflowStep(request, _testUserContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(stepInstanceId, result.StepInstanceId);
            _stepInstanceRepo.Received(1).Update(Arg.Any<WorkflowStepInstance>());
            _auditRepo.Received(1).LogAction(Arg.Any<AuditLog>());
        }

        [Test]
        public void CompleteWorkflowStep_WithoutPermission_ThrowsException()
        {
            // Arrange
            var stepInstanceId = 1;
            var stepInstance = CreateTestStepInstance(stepInstanceId, 1, WorkflowStepStatus.InProgress);
            var request = new CompleteWorkflowStepRequest
            {
                StepInstanceId = stepInstanceId,
                OutputData = new Dictionary<string, object>()
            };

            _stepInstanceRepo.GetById(stepInstanceId).Returns(stepInstance);
            _permissionService.CanCompleteWorkflowStep(stepInstance, _testUserContext).Returns(false);

            // Act & Assert
            Assert.Throws<WorkflowPermissionException>(() =>
                _workflowService.CompleteWorkflowStep(request, _testUserContext)
            );
        }

        [Test]
        public void CompleteWorkflowStep_WithInvalidStatus_ThrowsException()
        {
            // Arrange
            var stepInstanceId = 1;
            var stepInstance = CreateTestStepInstance(stepInstanceId, 1, WorkflowStepStatus.Completed);
            var request = new CompleteWorkflowStepRequest
            {
                StepInstanceId = stepInstanceId,
                OutputData = new Dictionary<string, object>()
            };

            _stepInstanceRepo.GetById(stepInstanceId).Returns(stepInstance);

            // Act & Assert
            Assert.Throws<WorkflowStateException>(() =>
                _workflowService.CompleteWorkflowStep(request, _testUserContext)
            );
        }

        #endregion

        #region RejectWorkflowStep Tests

        [Test]
        public void RejectWorkflowStep_WithValidRequest_RejectsStepSuccessfully()
        {
            // Arrange
            var stepInstanceId = 1;
            var stepInstance = CreateTestStepInstance(stepInstanceId, 1, WorkflowStepStatus.InProgress);
            var request = new RejectWorkflowStepRequest
            {
                StepInstanceId = stepInstanceId,
                RejectionReason = "Does not meet requirements"
            };

            _stepInstanceRepo.GetById(stepInstanceId).Returns(stepInstance);
            _permissionService.CanCompleteWorkflowStep(stepInstance, _testUserContext).Returns(true);

            // Act
            var result = _workflowService.RejectWorkflowStep(request, _testUserContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(stepInstanceId, result.StepInstanceId);
            _stepInstanceRepo.Received(1).Update(Arg.Any<WorkflowStepInstance>());
            _auditRepo.Received(1).LogAction(Arg.Any<AuditLog>());
        }

        [Test]
        public void RejectWorkflowStep_WithoutPermission_ThrowsException()
        {
            // Arrange
            var stepInstanceId = 1;
            var stepInstance = CreateTestStepInstance(stepInstanceId, 1, WorkflowStepStatus.InProgress);
            var request = new RejectWorkflowStepRequest
            {
                StepInstanceId = stepInstanceId,
                RejectionReason = "Does not meet requirements"
            };

            _stepInstanceRepo.GetById(stepInstanceId).Returns(stepInstance);
            _permissionService.CanCompleteWorkflowStep(stepInstance, _testUserContext).Returns(false);

            // Act & Assert
            Assert.Throws<WorkflowPermissionException>(() =>
                _workflowService.RejectWorkflowStep(request, _testUserContext)
            );
        }

        #endregion

        #region GetMyTasks Tests

        [Test]
        public void GetMyTasks_WithAssignedTasks_ReturnsTaskList()
        {
            // Arrange
            var tasks = new List<WorkflowTaskResult>
            {
                new WorkflowTaskResult { TaskId = 1, Title = "Task 1", Status = "InProgress" },
                new WorkflowTaskResult { TaskId = 2, Title = "Task 2", Status = "InProgress" }
            };

            _stepInstanceRepo.GetByAssignedUser(_testUserContext.UserID).Returns(
                new List<WorkflowStepInstance>
                {
                    CreateTestStepInstance(1, 1, WorkflowStepStatus.InProgress),
                    CreateTestStepInstance(2, 1, WorkflowStepStatus.InProgress)
                }
            );

            // Act
            var result = _workflowService.GetMyTasks(_testUserContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetMyTasks_WithNoTasks_ReturnsEmptyList()
        {
            // Arrange
            _stepInstanceRepo.GetByAssignedUser(_testUserContext.UserID).Returns(new List<WorkflowStepInstance>());

            // Act
            var result = _workflowService.GetMyTasks(_testUserContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region SuspendResumeWorkflow Tests

        [Test]
        public void SuspendWorkflowInstance_WithValidId_SuspendsSuccessfully()
        {
            // Arrange
            var instanceId = 1;
            var instance = CreateTestInstance(instanceId);
            instance.Status = WorkflowInstanceStatus.InProgress;

            _workflowRepo.GetById(instanceId).Returns(instance);
            _permissionService.CanViewWorkflowInstance(instance, _testUserContext).Returns(true);

            // Act
            var result = _workflowService.SuspendWorkflowInstance(instanceId, _testUserContext);

            // Assert
            Assert.IsNotNull(result);
            _workflowRepo.Received(1).Update(Arg.Any<WorkflowInstance>());
            _auditRepo.Received(1).LogAction(Arg.Any<AuditLog>());
        }

        [Test]
        public void ResumeWorkflowInstance_WithValidId_ResumesSuccessfully()
        {
            // Arrange
            var instanceId = 1;
            var instance = CreateTestInstance(instanceId);
            instance.Status = WorkflowInstanceStatus.Suspended;

            _workflowRepo.GetById(instanceId).Returns(instance);
            _permissionService.CanViewWorkflowInstance(instance, _testUserContext).Returns(true);

            // Act
            var result = _workflowService.ResumeWorkflowInstance(instanceId, _testUserContext);

            // Assert
            Assert.IsNotNull(result);
            _workflowRepo.Received(1).Update(Arg.Any<WorkflowInstance>());
            _auditRepo.Received(1).LogAction(Arg.Any<AuditLog>());
        }

        #endregion

        #region Helper Methods

        private WorkflowDefinition CreateTestTemplate(int templateId)
        {
            return new WorkflowDefinition
            {
                TemplateId = templateId,
                TemplateName = "Test Template",
                Description = "Test workflow template",
                CreatedDate = DateTime.Now,
                CreatedBy = "testuser",
                IsActive = true,
                RequiredParameters = new List<string>()
            };
        }

        private WorkflowInstance CreateTestInstance(int instanceId)
        {
            return new WorkflowInstance
            {
                InstanceId = instanceId,
                TemplateId = 1,
                Status = WorkflowInstanceStatus.InProgress,
                CreatedDate = DateTime.Now,
                CreatedBy = "testuser",
                InitiatorId = 1
            };
        }

        private WorkflowStep CreateTestStep(int stepId, int templateId)
        {
            return new WorkflowStep
            {
                StepId = stepId,
                TemplateId = templateId,
                StepName = $"Step {stepId}",
                Description = $"Test step {stepId}",
                AssignedRole = "Approver",
                StepType = WorkflowStepType.Approval
            };
        }

        private WorkflowStepInstance CreateTestStepInstance(int stepInstanceId, int instanceId, WorkflowStepStatus status)
        {
            return new WorkflowStepInstance
            {
                StepInstanceId = stepInstanceId,
                InstanceId = instanceId,
                StepId = 1,
                Status = status,
                AssignedUserId = 1,
                CreatedDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(1)
            };
        }

        private WorkflowConnection CreateTestConnection(int targetStepId, int templateId)
        {
            return new WorkflowConnection
            {
                ConnectionId = 1,
                TemplateId = templateId,
                SourceStepId = 1,
                TargetStepId = targetStepId,
                Condition = "result == 'approved'"
            };
        }

        #endregion
    }
}
