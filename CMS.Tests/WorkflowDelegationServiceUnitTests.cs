using System;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Infrastructure.Interfaces;
using CMS.Services;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Tests
{
    /// <summary>
    /// Unit tests for WorkflowDelegationService (MOD-008) Phase 15.
    /// Tests task delegation, delegation rules, and restrictions.
    /// 
    /// Target Coverage: 80%+
    /// 
    /// Validates:
    /// - Task delegation
    /// - Delegation rules enforcement
    /// - Delegation depth limits
    /// - Permission checks
    /// - Audit logging
    /// </summary>
    [TestFixture]
    public class WorkflowDelegationServiceUnitTests
    {
        private WorkflowDelegationService _delegationService;
        private IDelegationRepository _delegationRepo;
        private IWorkflowStepInstanceRepository _stepInstanceRepo;
        private IAuditRepository _auditRepo;
        private IWorkflowPermissionService _permissionService;
        private IWorkflowNotificationService _notificationService;
        private ILoggingService _loggingService;
        private UserContext _testUserContext;

        [SetUp]
        public void SetUp()
        {
            // Create mock dependencies
            _delegationRepo = Substitute.For<IDelegationRepository>();
            _stepInstanceRepo = Substitute.For<IWorkflowStepInstanceRepository>();
            _auditRepo = Substitute.For<IAuditRepository>();
            _permissionService = Substitute.For<IWorkflowPermissionService>();
            _notificationService = Substitute.For<IWorkflowNotificationService>();
            _loggingService = Substitute.For<ILoggingService>();

            // Create service instance
            _delegationService = new WorkflowDelegationService(
                _delegationRepo,
                _stepInstanceRepo,
                _auditRepo,
                _permissionService,
                _notificationService,
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

        #region DelegateTask Tests

        [Test]
        public void DelegateTask_WithValidRequest_DelegatesSuccessfully()
        {
            // Arrange
            var stepInstanceId = 1;
            var delegateToUserId = 2;
            var stepInstance = CreateTestStepInstance(stepInstanceId, 1, _testUserContext.UserID);
            var request = new DelegateTaskRequest
            {
                StepInstanceId = stepInstanceId,
                DelegateToUserId = delegateToUserId,
                Reason = "On vacation"
            };

            _stepInstanceRepo.GetById(stepInstanceId).Returns(stepInstance);
            _permissionService.CanDelegateTask(stepInstance, _testUserContext).Returns(true);
            _delegationRepo.Create(Arg.Any<WFDelegation>()).Returns(1);

            // Act
            var result = _delegationService.DelegateTask(request, _testUserContext);

            // Assert
            Assert.IsNotNull(result);
            _delegationRepo.Received(1).Create(Arg.Any<WFDelegation>());
            _stepInstanceRepo.Received(1).Update(Arg.Any<WorkflowStepInstance>());
            _auditRepo.Received(1).LogAction(Arg.Any<AuditLog>());
        }

        [Test]
        public void DelegateTask_WithoutPermission_ThrowsException()
        {
            // Arrange
            var stepInstanceId = 1;
            var delegateToUserId = 2;
            var stepInstance = CreateTestStepInstance(stepInstanceId, 1, _testUserContext.UserID);
            var request = new DelegateTaskRequest
            {
                StepInstanceId = stepInstanceId,
                DelegateToUserId = delegateToUserId,
                Reason = "On vacation"
            };

            _stepInstanceRepo.GetById(stepInstanceId).Returns(stepInstance);
            _permissionService.CanDelegateTask(stepInstance, _testUserContext).Returns(false);

            // Act & Assert
            Assert.Throws<WorkflowPermissionException>(() =>
                _delegationService.DelegateTask(request, _testUserContext)
            );
        }

        [Test]
        public void DelegateTask_WithInvalidStepInstance_ThrowsException()
        {
            // Arrange
            var stepInstanceId = 999;
            var delegateToUserId = 2;
            var request = new DelegateTaskRequest
            {
                StepInstanceId = stepInstanceId,
                DelegateToUserId = delegateToUserId,
                Reason = "On vacation"
            };

            _stepInstanceRepo.GetById(stepInstanceId).Returns((WorkflowStepInstance)null);

            // Act & Assert
            Assert.Throws<WorkflowValidationException>(() =>
                _delegationService.DelegateTask(request, _testUserContext)
            );
        }

        [Test]
        public void DelegateTask_ToSameUser_ThrowsException()
        {
            // Arrange
            var stepInstanceId = 1;
            var stepInstance = CreateTestStepInstance(stepInstanceId, 1, _testUserContext.UserID);
            var request = new DelegateTaskRequest
            {
                StepInstanceId = stepInstanceId,
                DelegateToUserId = _testUserContext.UserID, // Same user
                Reason = "On vacation"
            };

            _stepInstanceRepo.GetById(stepInstanceId).Returns(stepInstance);
            _permissionService.CanDelegateTask(stepInstance, _testUserContext).Returns(true);

            // Act & Assert
            Assert.Throws<WorkflowValidationException>(() =>
                _delegationService.DelegateTask(request, _testUserContext)
            );
        }

        [Test]
        public void DelegateTask_WithEmptyReason_ThrowsException()
        {
            // Arrange
            var stepInstanceId = 1;
            var delegateToUserId = 2;
            var stepInstance = CreateTestStepInstance(stepInstanceId, 1, _testUserContext.UserID);
            var request = new DelegateTaskRequest
            {
                StepInstanceId = stepInstanceId,
                DelegateToUserId = delegateToUserId,
                Reason = "" // Empty reason
            };

            _stepInstanceRepo.GetById(stepInstanceId).Returns(stepInstance);
            _permissionService.CanDelegateTask(stepInstance, _testUserContext).Returns(true);

            // Act & Assert
            Assert.Throws<WorkflowValidationException>(() =>
                _delegationService.DelegateTask(request, _testUserContext)
            );
        }

        #endregion

        #region RecallDelegation Tests

        [Test]
        public void RecallDelegation_WithValidId_RecallsSuccessfully()
        {
            // Arrange
            var delegationId = 1;
            var delegation = CreateTestDelegation(delegationId, _testUserContext.UserID);
            var stepInstance = CreateTestStepInstance(1, 1, 2); // Delegated to user 2

            _delegationRepo.GetById(delegationId).Returns(delegation);
            _stepInstanceRepo.GetById(delegation.StepInstanceId).Returns(stepInstance);
            _permissionService.CanRecallDelegation(delegation, _testUserContext).Returns(true);

            // Act
            _delegationService.RecallDelegation(delegationId, _testUserContext);

            // Assert
            _delegationRepo.Received(1).Update(Arg.Any<WFDelegation>());
            _stepInstanceRepo.Received(1).Update(Arg.Any<WorkflowStepInstance>());
            _auditRepo.Received(1).LogAction(Arg.Any<AuditLog>());
        }

        [Test]
        public void RecallDelegation_WithoutPermission_ThrowsException()
        {
            // Arrange
            var delegationId = 1;
            var delegation = CreateTestDelegation(delegationId, _testUserContext.UserID);

            _delegationRepo.GetById(delegationId).Returns(delegation);
            _permissionService.CanRecallDelegation(delegation, _testUserContext).Returns(false);

            // Act & Assert
            Assert.Throws<WorkflowPermissionException>(() =>
                _delegationService.RecallDelegation(delegationId, _testUserContext)
            );
        }

        [Test]
        public void RecallDelegation_WithInvalidId_ThrowsException()
        {
            // Arrange
            var delegationId = 999;
            _delegationRepo.GetById(delegationId).Returns((WFDelegation)null);

            // Act & Assert
            Assert.Throws<WorkflowValidationException>(() =>
                _delegationService.RecallDelegation(delegationId, _testUserContext)
            );
        }

        #endregion

        #region GetDelegationHistory Tests

        [Test]
        public void GetDelegationHistory_WithValidStepInstanceId_ReturnsDelegationHistory()
        {
            // Arrange
            var stepInstanceId = 1;
            var delegations = new List<WFDelegation>
            {
                CreateTestDelegation(1, 1),
                CreateTestDelegation(2, 1)
            };

            _delegationRepo.GetByStepInstanceId(stepInstanceId).Returns(delegations);

            // Act
            var result = _delegationService.GetDelegationHistory(stepInstanceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetDelegationHistory_WithNoDelegations_ReturnsEmptyList()
        {
            // Arrange
            var stepInstanceId = 1;
            _delegationRepo.GetByStepInstanceId(stepInstanceId).Returns(new List<WFDelegation>());

            // Act
            var result = _delegationService.GetDelegationHistory(stepInstanceId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region ValidateDelegationDepth Tests

        [Test]
        public void ValidateDelegationDepth_WithinLimit_ReturnsTrue()
        {
            // Arrange
            var stepInstanceId = 1;
            var delegations = new List<WFDelegation>
            {
                CreateTestDelegation(1, 1),
                CreateTestDelegation(2, 1)
            };

            _delegationRepo.GetByStepInstanceId(stepInstanceId).Returns(delegations);

            // Act
            var isValid = _delegationService.ValidateDelegationDepth(stepInstanceId);

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void ValidateDelegationDepth_ExceedsLimit_ReturnsFalse()
        {
            // Arrange
            var stepInstanceId = 1;
            var delegations = new List<WFDelegation>();
            
            // Create 6 delegations (exceeds typical limit of 5)
            for (int i = 1; i <= 6; i++)
            {
                delegations.Add(CreateTestDelegation(i, 1));
            }

            _delegationRepo.GetByStepInstanceId(stepInstanceId).Returns(delegations);

            // Act
            var isValid = _delegationService.ValidateDelegationDepth(stepInstanceId);

            // Assert
            Assert.IsFalse(isValid);
        }

        #endregion

        #region CanDelegateToUser Tests

        [Test]
        public void CanDelegateToUser_WithEligibleUser_ReturnsTrue()
        {
            // Arrange
            var stepInstance = CreateTestStepInstance(1, 1, 1);
            var delegateToUserId = 2;

            _permissionService.CanUserCompleteStep(delegateToUserId, stepInstance).Returns(true);

            // Act
            var canDelegate = _delegationService.CanDelegateToUser(stepInstance, delegateToUserId);

            // Assert
            Assert.IsTrue(canDelegate);
        }

        [Test]
        public void CanDelegateToUser_WithIneligibleUser_ReturnsFalse()
        {
            // Arrange
            var stepInstance = CreateTestStepInstance(1, 1, 1);
            var delegateToUserId = 2;

            _permissionService.CanUserCompleteStep(delegateToUserId, stepInstance).Returns(false);

            // Act
            var canDelegate = _delegationService.CanDelegateToUser(stepInstance, delegateToUserId);

            // Assert
            Assert.IsFalse(canDelegate);
        }

        #endregion

        #region Helper Methods

        private WorkflowStepInstance CreateTestStepInstance(int stepInstanceId, int instanceId, int assignedUserId)
        {
            return new WorkflowStepInstance
            {
                StepInstanceId = stepInstanceId,
                InstanceId = instanceId,
                StepId = 1,
                Status = WorkflowStepStatus.InProgress,
                AssignedUserId = assignedUserId,
                CreatedDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(1)
            };
        }

        private WFDelegation CreateTestDelegation(int delegationId, int delegatedByUserId)
        {
            return new WFDelegation
            {
                DelegationId = delegationId,
                StepInstanceId = 1,
                DelegatedByUserId = delegatedByUserId,
                DelegatedToUserId = delegatedByUserId + 1,
                DelegationDate = DateTime.Now,
                Reason = "On vacation",
                IsRecalled = false
            };
        }

        #endregion
    }
}
