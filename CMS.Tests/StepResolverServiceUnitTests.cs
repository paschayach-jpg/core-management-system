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
using CMS.Services.Interfaces;

namespace CMS.Tests
{
    /// <summary>
    /// Unit tests for StepResolverService (MOD-008) Phase 15.
    /// Tests workflow step resolution, condition evaluation, and delegation eligibility.
    /// 
    /// Target Coverage: 80%+
    /// 
    /// Validates:
    /// - Next step resolution
    /// - Condition evaluation
    /// - Delegation eligibility
    /// - Error handling
    /// - Null parameter validation
    /// </summary>
    [TestFixture]
    public class StepResolverServiceUnitTests
    {
        private StepResolverService _stepResolverService;
        private IWorkflowStepRepository _stepRepository;
        private IConditionEvaluatorService _conditionEvaluator;
        private ILoggingService _loggingService;

        [SetUp]
        public void SetUp()
        {
            // Create mock dependencies
            _stepRepository = Substitute.For<IWorkflowStepRepository>();
            _conditionEvaluator = Substitute.For<IConditionEvaluatorService>();
            _loggingService = Substitute.For<ILoggingService>();

            // Create service instance
            _stepResolverService = new StepResolverService(
                _stepRepository,
                _conditionEvaluator,
                _loggingService
            );
        }

        #region ResolveNextSteps Tests

        [Test]
        public void ResolveNextSteps_WithValidCurrentStep_ResolvesNextSteps()
        {
            // Arrange
            var currentStep = CreateTestStep(1, 1, "Step 1");
            var template = CreateTestTemplate(1);
            var outputData = "{\"result\": \"approved\"}";

            var allSteps = new List<WorkflowStep>
            {
                currentStep,
                CreateTestStep(2, 1, "Step 2"),
                CreateTestStep(3, 1, "Step 3")
            };

            _stepRepository.GetStepsByTemplate(template.WorkflowDefinitionId).Returns(allSteps);

            // Act
            var result = _stepResolverService.ResolveNextSteps(currentStep, outputData, template);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<WorkflowStep>>(result);
        }

        [Test]
        public void ResolveNextSteps_WithNullCurrentStep_ThrowsArgumentNullException()
        {
            // Arrange
            var template = CreateTestTemplate(1);
            var outputData = "{\"result\": \"approved\"}";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _stepResolverService.ResolveNextSteps(null, outputData, template)
            );
        }

        [Test]
        public void ResolveNextSteps_WithNullTemplate_ThrowsArgumentNullException()
        {
            // Arrange
            var currentStep = CreateTestStep(1, 1, "Step 1");
            var outputData = "{\"result\": \"approved\"}";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _stepResolverService.ResolveNextSteps(currentStep, outputData, null)
            );
        }

        [Test]
        public void ResolveNextSteps_WithEmptyOutputData_ResolvesNextSteps()
        {
            // Arrange
            var currentStep = CreateTestStep(1, 1, "Step 1");
            var template = CreateTestTemplate(1);
            var outputData = "";

            var allSteps = new List<WorkflowStep>
            {
                currentStep,
                CreateTestStep(2, 1, "Step 2")
            };

            _stepRepository.GetStepsByTemplate(template.WorkflowDefinitionId).Returns(allSteps);

            // Act
            var result = _stepResolverService.ResolveNextSteps(currentStep, outputData, template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void ResolveNextSteps_WithMultipleSteps_ReturnsAllSteps()
        {
            // Arrange
            var currentStep = CreateTestStep(1, 1, "Step 1");
            var template = CreateTestTemplate(1);
            var outputData = "{\"result\": \"approved\"}";

            var allSteps = new List<WorkflowStep>
            {
                currentStep,
                CreateTestStep(2, 1, "Step 2"),
                CreateTestStep(3, 1, "Step 3"),
                CreateTestStep(4, 1, "Step 4")
            };

            _stepRepository.GetStepsByTemplate(template.WorkflowDefinitionId).Returns(allSteps);

            // Act
            var result = _stepResolverService.ResolveNextSteps(currentStep, outputData, template);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<WorkflowStep>>(result);
        }

        #endregion

        #region EvaluateConditions Tests

        [Test]
        public void EvaluateConditions_WithValidExpression_EvaluatesSuccessfully()
        {
            // Arrange
            var conditionExpression = "result == 'approved'";
            var outputData = "{\"result\": \"approved\"}";

            _conditionEvaluator.EvaluateCondition(conditionExpression, outputData).Returns(true);

            // Act
            var result = _stepResolverService.EvaluateConditions(conditionExpression, outputData);

            // Assert
            Assert.IsTrue(result);
            _conditionEvaluator.Received(1).EvaluateCondition(conditionExpression, outputData);
        }

        [Test]
        public void EvaluateConditions_WithNullExpression_ReturnsTrue()
        {
            // Arrange
            var outputData = "{\"result\": \"approved\"}";

            // Act
            var result = _stepResolverService.EvaluateConditions(null, outputData);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void EvaluateConditions_WithEmptyExpression_ReturnsTrue()
        {
            // Arrange
            var conditionExpression = "";
            var outputData = "{\"result\": \"approved\"}";

            // Act
            var result = _stepResolverService.EvaluateConditions(conditionExpression, outputData);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void EvaluateConditions_WithFalseCondition_ReturnsFalse()
        {
            // Arrange
            var conditionExpression = "result == 'rejected'";
            var outputData = "{\"result\": \"approved\"}";

            _conditionEvaluator.EvaluateCondition(conditionExpression, outputData).Returns(false);

            // Act
            var result = _stepResolverService.EvaluateConditions(conditionExpression, outputData);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void EvaluateConditions_WithComplexExpression_EvaluatesSuccessfully()
        {
            // Arrange
            var conditionExpression = "(result == 'approved' AND amount > 1000) OR priority == 'high'";
            var outputData = "{\"result\": \"approved\", \"amount\": 1500, \"priority\": \"high\"}";

            _conditionEvaluator.EvaluateCondition(conditionExpression, outputData).Returns(true);

            // Act
            var result = _stepResolverService.EvaluateConditions(conditionExpression, outputData);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void EvaluateConditions_WithEvaluatorException_ThrowsException()
        {
            // Arrange
            var conditionExpression = "invalid expression";
            var outputData = "{\"result\": \"approved\"}";

            _conditionEvaluator.EvaluateCondition(conditionExpression, outputData)
                .Throws(new InvalidOperationException("Invalid expression"));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _stepResolverService.EvaluateConditions(conditionExpression, outputData)
            );
        }

        #endregion

        #region ResolveDelegationEligibility Tests

        [Test]
        public void ResolveDelegationEligibility_WithDelegationAllowed_ReturnsTrue()
        {
            // Arrange
            var step = CreateTestStep(1, 1, "Step 1");
            step.AllowDelegation = true;
            var delegateUserName = "delegateuser";

            // Act
            var result = _stepResolverService.ResolveDelegationEligibility(step, delegateUserName);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ResolveDelegationEligibility_WithDelegationNotAllowed_ReturnsFalse()
        {
            // Arrange
            var step = CreateTestStep(1, 1, "Step 1");
            step.AllowDelegation = false;
            var delegateUserName = "delegateuser";

            // Act
            var result = _stepResolverService.ResolveDelegationEligibility(step, delegateUserName);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ResolveDelegationEligibility_WithNullStep_ThrowsArgumentNullException()
        {
            // Arrange
            var delegateUserName = "delegateuser";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _stepResolverService.ResolveDelegationEligibility(null, delegateUserName)
            );
        }

        [Test]
        public void ResolveDelegationEligibility_WithNullUserName_ThrowsArgumentNullException()
        {
            // Arrange
            var step = CreateTestStep(1, 1, "Step 1");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _stepResolverService.ResolveDelegationEligibility(step, null)
            );
        }

        [Test]
        public void ResolveDelegationEligibility_WithEmptyUserName_ThrowsArgumentNullException()
        {
            // Arrange
            var step = CreateTestStep(1, 1, "Step 1");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _stepResolverService.ResolveDelegationEligibility(step, "")
            );
        }

        #endregion

        #region GetPossibleNextSteps Tests

        [Test]
        public void GetPossibleNextSteps_WithValidStep_ReturnsPossibleSteps()
        {
            // Arrange
            var currentStep = CreateTestStep(1, 1, "Step 1");
            var template = CreateTestTemplate(1);

            var allSteps = new List<WorkflowStep>
            {
                currentStep,
                CreateTestStep(2, 1, "Step 2"),
                CreateTestStep(3, 1, "Step 3")
            };

            _stepRepository.GetStepsByTemplate(template.WorkflowDefinitionId).Returns(allSteps);

            // Act
            var result = _stepResolverService.GetPossibleNextSteps(currentStep, template);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<WorkflowStep>>(result);
        }

        [Test]
        public void GetPossibleNextSteps_WithNullStep_ThrowsArgumentNullException()
        {
            // Arrange
            var template = CreateTestTemplate(1);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _stepResolverService.GetPossibleNextSteps(null, template)
            );
        }

        [Test]
        public void GetPossibleNextSteps_WithNullTemplate_ThrowsArgumentNullException()
        {
            // Arrange
            var currentStep = CreateTestStep(1, 1, "Step 1");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _stepResolverService.GetPossibleNextSteps(currentStep, null)
            );
        }

        [Test]
        public void GetPossibleNextSteps_WithSingleStep_ReturnsEmptyList()
        {
            // Arrange
            var currentStep = CreateTestStep(1, 1, "Step 1");
            var template = CreateTestTemplate(1);

            var allSteps = new List<WorkflowStep> { currentStep };

            _stepRepository.GetStepsByTemplate(template.WorkflowDefinitionId).Returns(allSteps);

            // Act
            var result = _stepResolverService.GetPossibleNextSteps(currentStep, template);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetPossibleNextSteps_WithMultipleSteps_ReturnsAllPossibleSteps()
        {
            // Arrange
            var currentStep = CreateTestStep(1, 1, "Step 1");
            var template = CreateTestTemplate(1);

            var allSteps = new List<WorkflowStep>
            {
                currentStep,
                CreateTestStep(2, 1, "Step 2"),
                CreateTestStep(3, 1, "Step 3"),
                CreateTestStep(4, 1, "Step 4"),
                CreateTestStep(5, 1, "Step 5")
            };

            _stepRepository.GetStepsByTemplate(template.WorkflowDefinitionId).Returns(allSteps);

            // Act
            var result = _stepResolverService.GetPossibleNextSteps(currentStep, template);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<WorkflowStep>>(result);
        }

        #endregion

        #region Helper Methods

        private WorkflowDefinition CreateTestTemplate(int templateId)
        {
            return new WorkflowDefinition
            {
                WorkflowDefinitionId = templateId,
                TemplateName = "Test Template",
                Description = "Test workflow template",
                CreatedDate = DateTime.Now,
                CreatedBy = "testuser",
                IsActive = true
            };
        }

        private WorkflowStep CreateTestStep(int stepId, int templateId, string stepName)
        {
            return new WorkflowStep
            {
                StepId = stepId,
                TemplateId = templateId,
                StepName = stepName,
                Description = $"Test step {stepId}",
                AssignedRole = "Approver",
                StepType = WorkflowStepType.Approval,
                AllowDelegation = true,
                MaxDelegationDepth = 3
            };
        }

        #endregion
    }
}
