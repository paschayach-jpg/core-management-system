using System;
using System.Collections.Generic;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Services.Validators
{
    /// <summary>
    /// Validator for CreateWorkflowInstanceRequest.
    /// Validates that the request contains all required data and that the user has permission to create instances.
    /// </summary>
    public class CreateWorkflowInstanceValidator
    {
        private readonly IWorkflowTemplateRepository _templateRepository;
        private readonly IPermissionService _permissionService;

        /// <summary>
        /// Initializes a new instance of the CreateWorkflowInstanceValidator class.
        /// </summary>
        /// <param name="templateRepository">Repository for accessing workflow templates.</param>
        /// <param name="permissionService">Service for checking user permissions.</param>
        public CreateWorkflowInstanceValidator(IWorkflowTemplateRepository templateRepository, IPermissionService permissionService)
        {
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }

        /// <summary>
        /// Validates a CreateWorkflowInstanceRequest.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <param name="userContext">The current user context for permission checks.</param>
        /// <returns>A ValidationResult containing any validation errors.</returns>
        public ValidationResult Validate(CreateWorkflowInstanceRequest request, UserContext userContext)
        {
            var result = new ValidationResult();

            if (request == null)
            {
                result.Errors.Add("Request cannot be null");
                result.IsValid = false;
                return result;
            }

            // Validate WorkflowDefinitionId
            if (request.WorkflowDefinitionId <= 0)
            {
                result.Errors.Add("WorkflowDefinitionId must be greater than 0");
            }

            // Validate InputParameters
            if (request.InputParameters == null)
            {
                result.Errors.Add("InputParameters cannot be null");
            }

            // Validate template exists and is active
            if (request.WorkflowDefinitionId > 0)
            {
                var template = _templateRepository.GetTemplate(request.WorkflowDefinitionId);
                if (template == null)
                {
                    result.Errors.Add($"Workflow template with ID {request.WorkflowDefinitionId} not found");
                }
                else if (!template.IsActive)
                {
                    result.Errors.Add($"Workflow template '{template.Name}' is not active");
                }
                else if (template.IsDeleted)
                {
                    result.Errors.Add($"Workflow template '{template.Name}' has been deleted");
                }
            }

            // Validate user has permission to create instances
            if (userContext != null && request.WorkflowDefinitionId > 0)
            {
                // Check if user has workflow creation permission
                // This is a placeholder - actual permission check depends on your permission model
                if (!CanUserCreateWorkflowInstance(userContext))
                {
                    result.Errors.Add($"User '{userContext.ADUserName}' does not have permission to create workflow instances");
                }
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        /// <summary>
        /// Checks if a user has permission to create workflow instances.
        /// </summary>
        private bool CanUserCreateWorkflowInstance(UserContext userContext)
        {
            // Placeholder implementation - customize based on your permission model
            // Typically check if user has a specific role or permission
            return userContext != null && !string.IsNullOrEmpty(userContext.ADUserName);
        }
    }
}
