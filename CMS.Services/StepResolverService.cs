using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Services.Interfaces;
using CMS.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Services
{
    /// <summary>
    /// Workflow step resolution service implementation
    /// </summary>
    public class StepResolverService : IStepResolverService
    {
        private readonly IWorkflowStepRepository _stepRepository;
        private readonly IConditionEvaluatorService _conditionEvaluator;
        private readonly ILoggingService _loggingService;

        public StepResolverService(
            IWorkflowStepRepository stepRepository,
            IConditionEvaluatorService conditionEvaluator,
            ILoggingService loggingService)
        {
            _stepRepository = stepRepository;
            _conditionEvaluator = conditionEvaluator;
            _loggingService = loggingService;
        }

        public List<WorkflowStep> ResolveNextSteps(WorkflowStep currentStep, string outputData, WorkflowDefinition template)
        {
            try
            {
                if (currentStep == null)
                    throw new ArgumentNullException(nameof(currentStep));

                if (template == null)
                    throw new ArgumentNullException(nameof(template));

                var nextSteps = new List<WorkflowStep>();

                // Get all steps in template
                var allSteps = _stepRepository.GetStepsByTemplate(template.WorkflowDefinitionId);

                // Find connections from current step
                // TODO: Get connections from repository
                // For now, return empty list (connections need to be fetched from DB)

                return nextSteps;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error resolving next steps: {ex.Message}", ex);
                throw;
            }
        }

        public bool EvaluateConditions(string conditionExpression, string outputData)
        {
            try
            {
                if (string.IsNullOrEmpty(conditionExpression))
                    return true; // No condition means always proceed

                return _conditionEvaluator.EvaluateCondition(conditionExpression, outputData);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error evaluating conditions: {ex.Message}", ex);
                throw;
            }
        }

        public bool ResolveDelegationEligibility(WorkflowStep step, string delegateUserName)
        {
            try
            {
                if (step == null)
                    throw new ArgumentNullException(nameof(step));

                if (string.IsNullOrEmpty(delegateUserName))
                    throw new ArgumentNullException(nameof(delegateUserName));

                // Check if delegation is allowed for this step
                if (!step.AllowDelegation)
                    return false;

                // TODO: Check if user has required role
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error resolving delegation eligibility: {ex.Message}", ex);
                throw;
            }
        }

        public List<WorkflowStep> GetPossibleNextSteps(WorkflowStep currentStep, WorkflowDefinition template)
        {
            try
            {
                if (currentStep == null)
                    throw new ArgumentNullException(nameof(currentStep));

                if (template == null)
                    throw new ArgumentNullException(nameof(template));

                var nextSteps = new List<WorkflowStep>();

                // Get all steps in template
                var allSteps = _stepRepository.GetStepsByTemplate(template.WorkflowDefinitionId);

                // TODO: Get connections from repository and find next steps
                // For now, return empty list

                return nextSteps;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting possible next steps: {ex.Message}", ex);
                throw;
            }
        }
    }
}
