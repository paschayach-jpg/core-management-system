using CMS.Services.Interfaces;
using CMS.Services.DTOs;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Data.Models;
using System;
using System.Collections.Generic;

namespace CMS.Controllers
{
    /// <summary>
    /// Workflow template controller for template management
    /// </summary>
    public class WorkflowTemplateController
    {
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService _loggingService;

        public WorkflowTemplateController(IWorkflowTemplateService templateService = null, ILoggingService loggingService = null)
        {
            _templateService = templateService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Create a new workflow template
        /// </summary>
        public WorkflowTemplateResult CreateTemplate(CreateTemplateRequest request, UserContext userContext)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Creating workflow template: {request.Name}");
                return _templateService.CreateTemplate(request, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error creating workflow template: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Update an existing workflow template
        /// </summary>
        public WorkflowTemplateResult UpdateTemplate(UpdateTemplateRequest request, UserContext userContext)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Updating workflow template {request.WorkflowDefinitionId}");
                return _templateService.UpdateTemplate(request, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error updating workflow template: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get a specific workflow template
        /// </summary>
        public WorkflowTemplateResult GetTemplate(int templateId, UserContext userContext)
        {
            try
            {
                if (templateId <= 0)
                    throw new ArgumentException("Invalid template ID");

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Getting workflow template {templateId}");
                return _templateService.GetTemplate(templateId, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error getting workflow template: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get workflow templates with filtering
        /// </summary>
        public PagedResult<WorkflowTemplateResult> GetTemplates(WorkflowTemplateFilter filter, UserContext userContext)
        {
            try
            {
                if (filter == null)
                    throw new ArgumentNullException(nameof(filter));

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Getting workflow templates");
                return _templateService.GetTemplates(filter, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error getting workflow templates: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Delete a workflow template
        /// </summary>
        public void DeleteTemplate(int templateId, UserContext userContext)
        {
            try
            {
                if (templateId <= 0)
                    throw new ArgumentException("Invalid template ID");

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Deleting workflow template {templateId}");
                _templateService.DeleteTemplate(templateId, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error deleting workflow template: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get all versions of a workflow template
        /// </summary>
        public List<WorkflowTemplateVersionResult> GetTemplateVersions(int templateId, UserContext userContext)
        {
            try
            {
                if (templateId <= 0)
                    throw new ArgumentException("Invalid template ID");

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Getting versions for template {templateId}");
                return _templateService.GetTemplateVersions(templateId, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error getting template versions: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Compare two versions of a workflow template
        /// </summary>
        public TemplateComparisonResult CompareTemplateVersions(int templateId, int version1, int version2, UserContext userContext)
        {
            try
            {
                if (templateId <= 0)
                    throw new ArgumentException("Invalid template ID");

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Comparing template {templateId} versions {version1} and {version2}");
                return _templateService.CompareTemplateVersions(templateId, version1, version2, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error comparing template versions: {ex.Message}", ex);
                throw;
            }
        }
    }
}
