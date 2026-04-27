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

namespace CMS.Tests
{
    /// <summary>
    /// Unit tests for WorkflowTemplateService (MOD-008) Phase 15.
    /// Tests template creation, editing, versioning, and validation.
    /// 
    /// Target Coverage: 80%+
    /// 
    /// Validates:
    /// - Template creation
    /// - Template editing
    /// - Template versioning
    /// - Template validation
    /// - Permission checks
    /// - State management
    /// </summary>
    [TestFixture]
    public class WorkflowTemplateServiceUnitTests
    {
        private WorkflowTemplateService _templateService;
        private IWorkflowTemplateRepository _templateRepo;
        private IWorkflowStepRepository _stepRepo;
        private IAuditRepository _auditRepo;
        private IWorkflowPermissionService _permissionService;
        private ILoggingService _loggingService;
        private UserContext _testUserContext;

        [SetUp]
        public void SetUp()
        {
            // Create mock dependencies
            _templateRepo = Substitute.For<IWorkflowTemplateRepository>();
            _stepRepo = Substitute.For<IWorkflowStepRepository>();
            _auditRepo = Substitute.For<IAuditRepository>();
            _permissionService = Substitute.For<IWorkflowPermissionService>();
            _loggingService = Substitute.For<ILoggingService>();

            // Create service instance
            _templateService = new WorkflowTemplateService(
                _templateRepo,
                _stepRepo,
                _auditRepo,
                _permissionService,
                _loggingService
            );

            // Create test user context
            _testUserContext = new UserContext
            {
                ADUserName = "admin",
                UserID = 1,
                UserRole = "Administrator"
            };
        }

        #region CreateTemplate Tests

        [Test]
        public void CreateTemplate_WithValidRequest_CreatesTemplateSuccessfully()
        {
            // Arrange
            var request = new CreateTemplateRequest
            {
                TemplateName = "Approval Workflow",
                Description = "Standard approval workflow",
                Steps = new List<WorkflowStepDTO>
                {
                    new WorkflowStepDTO { StepName = "Submit", AssignedRole = "Employee" },
                    new WorkflowStepDTO { StepName = "Approve", AssignedRole = "Manager" }
                }
            };

            _permissionService.CanCreateTemplate(_testUserContext).Returns(true);
            _templateRepo.Create(Arg.Any<WorkflowDefinition>()).Returns(1);

            // Act
            var result = _templateService.CreateTemplate(request, _testUserContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.TemplateId);
            _templateRepo.Received(1).Create(Arg.Any<WorkflowDefinition>());
            _auditRepo.Received(1).LogAction(Arg.Any<AuditLog>());
        }

        [Test]
        public void CreateTemplate_WithoutPermission_ThrowsException()
        {
            // Arrange
            var request = new CreateTemplateRequest
            {
                TemplateName = "Approval Workflow",
                Description = "Standard approval workflow"
            };

            _permissionService.CanCreateTemplate(_testUserContext).Returns(false);

            // Act & Assert
            Assert.Throws<WorkflowPermissionException>(() =>
                _templateService.CreateTemplate(request, _testUserContext)
            );
        }

        [Test]
        public void CreateTemplate_WithEmptyName_ThrowsException()
        {
            // Arrange
            var request = new CreateTemplateRequest
            {
                TemplateName = "",
                Description = "Standard approval workflow"
            };

            _permissionService.CanCreateTemplate(_testUserContext).Returns(true);

            // Act & Assert
            Assert.Throws<WorkflowValidationException>(() =>
                _templateService.CreateTemplate(request, _testUserContext)
            );
        }

        [Test]
        public void CreateTemplate_WithNoSteps_ThrowsException()
        {
            // Arrange
            var request = new CreateTemplateRequest
            {
                TemplateName = "Approval Workflow",
                Description = "Standard approval workflow",
                Steps = new List<WorkflowStepDTO>()
            };

            _permissionService.CanCreateTemplate(_testUserContext).Returns(true);

            // Act & Assert
            Assert.Throws<WorkflowValidationException>(() =>
                _templateService.CreateTemplate(request, _testUserContext)
            );
        }

        [Test]
        public void CreateTemplate_WithDuplicateStepNames_ThrowsException()
        {
            // Arrange
            var request = new CreateTemplateRequest
            {
                TemplateName = "Approval Workflow",
                Description = "Standard approval workflow",
                Steps = new List<WorkflowStepDTO>
                {
                    new WorkflowStepDTO { StepName = "Approve", AssignedRole = "Manager" },
                    new WorkflowStepDTO { StepName = "Approve", AssignedRole = "Director" }
                }
            };

            _permissionService.CanCreateTemplate(_testUserContext).Returns(true);

            // Act & Assert
            Assert.Throws<WorkflowValidationException>(() =>
                _templateService.CreateTemplate(request, _testUserContext)
            );
        }

        #endregion

        #region UpdateTemplate Tests

        [Test]
        public void UpdateTemplate_WithValidRequest_UpdatesTemplateSuccessfully()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);
            var request = new CreateTemplateRequest
            {
                TemplateName = "Updated Workflow",
                Description = "Updated description"
            };

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanModifyTemplate(template, _testUserContext).Returns(true);
            _templateRepo.Update(Arg.Any<WorkflowDefinition>()).Returns(true);

            // Act
            var result = _templateService.UpdateTemplate(templateId, request, _testUserContext);

            // Assert
            Assert.IsNotNull(result);
            _templateRepo.Received(1).Update(Arg.Any<WorkflowDefinition>());
            _auditRepo.Received(1).LogAction(Arg.Any<AuditLog>());
        }

        [Test]
        public void UpdateTemplate_WithoutPermission_ThrowsException()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);
            var request = new CreateTemplateRequest
            {
                TemplateName = "Updated Workflow"
            };

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanModifyTemplate(template, _testUserContext).Returns(false);

            // Act & Assert
            Assert.Throws<WorkflowPermissionException>(() =>
                _templateService.UpdateTemplate(templateId, request, _testUserContext)
            );
        }

        [Test]
        public void UpdateTemplate_WithInvalidId_ThrowsException()
        {
            // Arrange
            var templateId = 999;
            var request = new CreateTemplateRequest
            {
                TemplateName = "Updated Workflow"
            };

            _templateRepo.GetById(templateId).Returns((WorkflowDefinition)null);

            // Act & Assert
            Assert.Throws<WorkflowValidationException>(() =>
                _templateService.UpdateTemplate(templateId, request, _testUserContext)
            );
        }

        [Test]
        public void UpdateTemplate_WithActiveInstances_ThrowsException()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);
            var request = new CreateTemplateRequest
            {
                TemplateName = "Updated Workflow"
            };

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanModifyTemplate(template, _testUserContext).Returns(true);
            _templateRepo.HasActiveInstances(templateId).Returns(true);

            // Act & Assert
            Assert.Throws<WorkflowStateException>(() =>
                _templateService.UpdateTemplate(templateId, request, _testUserContext)
            );
        }

        #endregion

        #region GetTemplate Tests

        [Test]
        public void GetTemplate_WithValidId_ReturnsTemplate()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanViewTemplate(template, _testUserContext).Returns(true);

            // Act
            var result = _templateService.GetTemplate(templateId, _testUserContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(templateId, result.TemplateId);
        }

        [Test]
        public void GetTemplate_WithoutPermission_ThrowsException()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanViewTemplate(template, _testUserContext).Returns(false);

            // Act & Assert
            Assert.Throws<WorkflowPermissionException>(() =>
                _templateService.GetTemplate(templateId, _testUserContext)
            );
        }

        [Test]
        public void GetTemplate_WithInvalidId_ThrowsException()
        {
            // Arrange
            var templateId = 999;
            _templateRepo.GetById(templateId).Returns((WorkflowDefinition)null);

            // Act & Assert
            Assert.Throws<WorkflowValidationException>(() =>
                _templateService.GetTemplate(templateId, _testUserContext)
            );
        }

        #endregion

        #region ListTemplates Tests

        [Test]
        public void ListTemplates_WithActiveTemplates_ReturnsTemplateList()
        {
            // Arrange
            var templates = new List<WorkflowDefinition>
            {
                CreateTestTemplate(1),
                CreateTestTemplate(2),
                CreateTestTemplate(3)
            };

            _templateRepo.GetAll().Returns(templates);

            // Act
            var result = _templateService.ListTemplates(_testUserContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void ListTemplates_WithNoTemplates_ReturnsEmptyList()
        {
            // Arrange
            _templateRepo.GetAll().Returns(new List<WorkflowDefinition>());

            // Act
            var result = _templateService.ListTemplates(_testUserContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region ActivateTemplate Tests

        [Test]
        public void ActivateTemplate_WithValidId_ActivatesSuccessfully()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);
            template.IsActive = false;

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanModifyTemplate(template, _testUserContext).Returns(true);

            // Act
            var result = _templateService.ActivateTemplate(templateId, _testUserContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsActive);
            _templateRepo.Received(1).Update(Arg.Any<WorkflowDefinition>());
            _auditRepo.Received(1).LogAction(Arg.Any<AuditLog>());
        }

        [Test]
        public void ActivateTemplate_WithoutPermission_ThrowsException()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanModifyTemplate(template, _testUserContext).Returns(false);

            // Act & Assert
            Assert.Throws<WorkflowPermissionException>(() =>
                _templateService.ActivateTemplate(templateId, _testUserContext)
            );
        }

        #endregion

        #region DeactivateTemplate Tests

        [Test]
        public void DeactivateTemplate_WithValidId_DeactivatesSuccessfully()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);
            template.IsActive = true;

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanModifyTemplate(template, _testUserContext).Returns(true);

            // Act
            var result = _templateService.DeactivateTemplate(templateId, _testUserContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsActive);
            _templateRepo.Received(1).Update(Arg.Any<WorkflowDefinition>());
            _auditRepo.Received(1).LogAction(Arg.Any<AuditLog>());
        }

        #endregion

        #region DeleteTemplate Tests

        [Test]
        public void DeleteTemplate_WithValidId_DeletesSuccessfully()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanModifyTemplate(template, _testUserContext).Returns(true);
            _templateRepo.HasActiveInstances(templateId).Returns(false);

            // Act
            _templateService.DeleteTemplate(templateId, _testUserContext);

            // Assert
            _templateRepo.Received(1).Delete(templateId);
            _auditRepo.Received(1).LogAction(Arg.Any<AuditLog>());
        }

        [Test]
        public void DeleteTemplate_WithActiveInstances_ThrowsException()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanModifyTemplate(template, _testUserContext).Returns(true);
            _templateRepo.HasActiveInstances(templateId).Returns(true);

            // Act & Assert
            Assert.Throws<WorkflowStateException>(() =>
                _templateService.DeleteTemplate(templateId, _testUserContext)
            );
        }

        [Test]
        public void DeleteTemplate_WithoutPermission_ThrowsException()
        {
            // Arrange
            var templateId = 1;
            var template = CreateTestTemplate(templateId);

            _templateRepo.GetById(templateId).Returns(template);
            _permissionService.CanModifyTemplate(template, _testUserContext).Returns(false);

            // Act & Assert
            Assert.Throws<WorkflowPermissionException>(() =>
                _templateService.DeleteTemplate(templateId, _testUserContext)
            );
        }

        #endregion

        #region Helper Methods

        private WorkflowDefinition CreateTestTemplate(int templateId)
        {
            return new WorkflowDefinition
            {
                TemplateId = templateId,
                TemplateName = $"Template {templateId}",
                Description = $"Test template {templateId}",
                CreatedDate = DateTime.Now,
                CreatedBy = "admin",
                IsActive = true,
                Version = 1,
                RequiredParameters = new List<string>()
            };
        }

        #endregion
    }
}
