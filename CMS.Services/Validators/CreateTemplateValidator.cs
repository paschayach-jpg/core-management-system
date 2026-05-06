using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;

namespace CMS.Services.Validators
{
    /// <summary>
    /// Validator for CreateTemplateRequest.
    /// Validates that the template request contains all required data and meets naming/format requirements.
    /// </summary>
    public class CreateTemplateValidator
    {
        private readonly IWorkflowTemplateRepository _templateRepository;
        private const int MaxNameLength = 255;

        /// <summary>
        /// Initializes a new instance of the CreateTemplateValidator class.
        /// </summary>
        /// <param name="templateRepository">Repository for accessing workflow templates.</param>
        public CreateTemplateValidator(IWorkflowTemplateRepository templateRepository)
        {
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        }

        /// <summary>
        /// Validates a CreateTemplateRequest.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <param name="userContext">The current user context for permission checks.</param>
        /// <returns>A ValidationResult containing any validation errors.</returns>
        public ValidationResult Validate(CreateTemplateRequest request, UserContext userContext)
        {
            var result = new ValidationResult();

            if (request == null)
            {
                result.Errors.Add("Request cannot be null");
                result.IsValid = false;
                return result;
            }

            // Validate Name
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                result.Errors.Add("Template name is required");
            }
            else
            {
                // Check name length
                if (request.Name.Length > MaxNameLength)
                {
                    result.Errors.Add($"Template name cannot exceed {MaxNameLength} characters");
                }

                // Check name uniqueness
                if (TemplateNameExists(request.Name))
                {
                    result.Errors.Add($"A template with the name '{request.Name}' already exists");
                }
            }

            // Validate Description
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                result.Errors.Add("Template description is required");
            }

            // Validate TemplateDefinition
            if (string.IsNullOrWhiteSpace(request.TemplateDefinition))
            {
                result.Errors.Add("Template definition is required");
            }

            // Validate user has admin role
            if (userContext != null && !IsUserAdmin(userContext))
            {
                result.Errors.Add($"User '{userContext.ADUserName}' does not have admin role required to create templates");
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        /// <summary>
        /// Checks if a template name already exists in the database.
        /// </summary>
        private bool TemplateNameExists(string name)
        {
            try
            {
                var filter = new CMS.Data.Interfaces.WorkflowTemplateFilter { NameContains = name };
                var result = _templateRepository.GetTemplates(filter);
                return result != null && result.Items != null && result.Items.Any();
            }
            catch
            {
                // If there's an error checking, assume it doesn't exist
                return false;
            }
        }

        /// <summary>
        /// Checks if a user has admin role.
        /// </summary>
        private bool IsUserAdmin(UserContext userContext)
        {
            // Check if user has UserRole.Admin (AppRole = 4)
            return userContext != null && userContext.Role == UserRole.Admin;
        }
    }
}
