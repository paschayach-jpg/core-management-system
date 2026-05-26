using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Data.Models;
using System;
using System.Collections.Generic;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Workflow template management service
    /// Handles template creation, versioning, and configuration
    /// </summary>
    public interface IWorkflowTemplateService
    {
        /// <summary>
        /// Create a new workflow template
        /// </summary>
        WorkflowTemplateResult CreateTemplate(CreateTemplateRequest request, UserContext userContext);

        /// <summary>
        /// Update an existing workflow template
        /// </summary>
        WorkflowTemplateResult UpdateTemplate(UpdateTemplateRequest request, UserContext userContext);

        /// <summary>
        /// Get a specific workflow template
        /// </summary>
        WorkflowTemplateResult GetTemplate(int templateId, UserContext userContext);

        /// <summary>
        /// Get workflow templates with filtering and pagination
        /// </summary>
        PagedResult<WorkflowTemplateResult> GetTemplates(WorkflowTemplateFilter filter, UserContext userContext);

        /// <summary>
        /// Delete a workflow template (soft delete)
        /// </summary>
        void DeleteTemplate(int templateId, UserContext userContext);

        /// <summary>
        /// Get all versions of a workflow template
        /// </summary>
        List<WorkflowTemplateVersionResult> GetTemplateVersions(int templateId, UserContext userContext);

        /// <summary>
        /// Compare two versions of a workflow template
        /// </summary>
        TemplateComparisonResult CompareTemplateVersions(int templateId, int version1, int version2, UserContext userContext);

        /// <summary>
        /// Check if a template has active instances
        /// </summary>
        bool HasActiveInstances(int templateId);

        /// <summary>
        /// Submit a Draft template into the Approval Queue (sets ApprovalStatus = PendingApproval).
        /// Only Draft templates may be submitted.
        /// </summary>
        WorkflowTemplateResult SubmitForApproval(int templateId, UserContext userContext);

        /// <summary>
        /// Approve a PendingApproval template (sets ApprovalStatus = Active, IsActive = true).
        /// </summary>
        WorkflowTemplateResult ApproveTemplate(int templateId, UserContext userContext);

        /// <summary>
        /// Reject a PendingApproval template — returns it to Draft with a rejection reason.
        /// Does NOT delete the template.
        /// </summary>
        WorkflowTemplateResult RejectTemplate(int templateId, string reason, UserContext userContext);
    }
}
