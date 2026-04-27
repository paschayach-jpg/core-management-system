using System;
using System.Collections.Generic;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Services.Validators
{
    /// <summary>
    /// Validator for DelegateTaskRequest.
    /// Validates that delegation is allowed and meets all business rules.
    /// </summary>
    public class DelegationValidator
    {
        private readonly IWorkflowStepInstanceRepository _stepInstanceRepository;
        private readonly IWorkflowStepRepository _stepRepository;
        private readonly IUserAccountRepository _userRepository;
        private readonly IWorkflowDelegationService _delegationService;

        /// <summary>
        /// Initializes a new instance of the DelegationValidator class.
        /// </summary>
        /// <param name="stepInstanceRepository">Repository for accessing workflow step instances.</param>
        /// <param name="stepRepository">Repository for accessing workflow steps.</param>
        /// <param name="userRepository">Repository for accessing user accounts.</param>
        /// <param name="delegationService">Service for checking delegation rules.</param>
        public DelegationValidator(
            IWorkflowStepInstanceRepository stepInstanceRepository,
            IWorkflowStepRepository stepRepository,
            IUserAccountRepository userRepository,
            IWorkflowDelegationService delegationService)
        {
            _stepInstanceRepository = stepInstanceRepository ?? throw new ArgumentNullException(nameof(stepInstanceRepository));
            _stepRepository = stepRepository ?? throw new ArgumentNullException(nameof(stepRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _delegationService = delegationService ?? throw new ArgumentNullException(nameof(delegationService));
        }

        /// <summary>
        /// Validates a DelegateTaskRequest.
        /// </summary>
        /// <param name="request">The delegation request to validate.</param>
        /// <param name="currentUserName">The AD username of the current user (current assignee).</param>
        /// <returns>A ValidationResult containing any validation errors.</returns>
        public ValidationResult Validate(DelegateTaskRequest request, string currentUserName)
        {
            var result = new ValidationResult();

            if (request == null)
            {
                result.Errors.Add("Request cannot be null");
                result.IsValid = false;
                return result;
            }

            // Validate WorkflowStepInstanceId
            if (request.WorkflowStepInstanceId <= 0)
            {
                result.Errors.Add("WorkflowStepInstanceId must be greater than 0");
            }
            else
            {
                var stepInstance = _stepInstanceRepository.GetStepInstance(request.WorkflowStepInstanceId);
                if (stepInstance == null)
                {
                    result.Errors.Add($"Workflow step instance with ID {request.WorkflowStepInstanceId} not found");
                }
                else if (stepInstance.IsDeleted)
                {
                    result.Errors.Add("Workflow step instance has been deleted");
                }
            }

            // Validate DelegateToUserName
            if (string.IsNullOrWhiteSpace(request.DelegateToUserName))
            {
                result.Errors.Add("DelegateToUserName is required");
            }
            else
            {
                // Check if user exists
                var delegateToUser = _userRepository.GetByADUserName(request.DelegateToUserName);
                if (delegateToUser == null)
                {
                    result.Errors.Add($"User '{request.DelegateToUserName}' not found");
                }
            }

            // Validate delegation is allowed for the step
            if (request.WorkflowStepInstanceId > 0)
            {
                var stepInstance = _stepInstanceRepository.GetStepInstance(request.WorkflowStepInstanceId);
                if (stepInstance != null)
                {
                    var step = _stepRepository.GetStep(stepInstance.WorkflowStepId);
                    if (step != null && !step.AllowDelegation)
                    {
                        result.Errors.Add("Delegation is not allowed for this workflow step");
                    }
                }
            }

            // Validate delegation depth not exceeded
            if (request.WorkflowStepInstanceId > 0 && !string.IsNullOrWhiteSpace(request.DelegateToUserName))
            {
                var stepInstance = _stepInstanceRepository.GetStepInstance(request.WorkflowStepInstanceId);
                if (stepInstance != null)
                {
                    var step = _stepRepository.GetStep(stepInstance.WorkflowStepId);
                    if (step != null)
                    {
                        // Check if delegation is allowed using the service
                        var validationResult = _delegationService.ValidateDelegation(stepInstance, request.DelegateToUserName, null);
                        if (!validationResult.IsValid)
                        {
                            result.Errors.AddRange(validationResult.Errors);
                        }
                    }
                }
            }

            // Validate DelegateToUserName != current assignee
            if (!string.IsNullOrWhiteSpace(request.DelegateToUserName) && 
                !string.IsNullOrWhiteSpace(currentUserName) &&
                request.DelegateToUserName.Equals(currentUserName, StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add("Cannot delegate to the current assignee");
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
    }
}
