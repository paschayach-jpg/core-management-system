using System;
using System.Collections.Generic;
using CMS.Services.DTOs;
using CMS.Domain.Models;
using CMS.Domain.Enums;

namespace CMS.Services.Validators
{
    /// <summary>
    /// Validator for WorkflowStep model.
    /// Validates that workflow steps contain all required data and meet business rules.
    /// </summary>
    public class WorkflowStepValidator
    {
        /// <summary>
        /// Validates a WorkflowStep model.
        /// </summary>
        /// <param name="step">The workflow step to validate.</param>
        /// <returns>A ValidationResult containing any validation errors.</returns>
        public ValidationResult Validate(WorkflowStep step)
        {
            var result = new ValidationResult();

            if (step == null)
            {
                result.Errors.Add("WorkflowStep cannot be null");
                result.IsValid = false;
                return result;
            }

            // Validate StepName
            if (string.IsNullOrWhiteSpace(step.StepName))
            {
                result.Errors.Add("Step name is required");
            }

            // Validate StepType is valid enum value
            if (!IsValidStepType(step.StepType))
            {
                result.Errors.Add($"Invalid step type: {step.StepType}");
            }

            // Validate SLA values if specified
            if (step.SLAHours.HasValue && step.SLAHours < 0)
            {
                result.Errors.Add("SLA hours cannot be negative");
            }

            if (step.SLADays.HasValue && step.SLADays < 0)
            {
                result.Errors.Add("SLA days cannot be negative");
            }

            // Validate MaxDelegationDepth
            if (step.MaxDelegationDepth < 0)
            {
                result.Errors.Add("MaxDelegationDepth cannot be negative");
            }

            // Validate SequenceNumber
            if (step.SequenceNumber <= 0)
            {
                result.Errors.Add("SequenceNumber must be greater than 0");
            }

            // Validate that at least one assignment method is specified
            if (string.IsNullOrWhiteSpace(step.AssignedRole) &&
                string.IsNullOrWhiteSpace(step.AssignedUser) &&
                string.IsNullOrWhiteSpace(step.AssignedOrgUnit))
            {
                result.Errors.Add("At least one of AssignedRole, AssignedUser, or AssignedOrgUnit must be specified");
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        /// <summary>
        /// Checks if a step type is a valid enum value.
        /// </summary>
        private bool IsValidStepType(WorkflowStepType stepType)
        {
            // Check if the value is a defined enum value
            return Enum.IsDefined(typeof(WorkflowStepType), stepType);
        }
    }
}
