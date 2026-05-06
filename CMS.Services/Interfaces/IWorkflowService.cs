using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Data.Models;
using System;
using System.Collections.Generic;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Main workflow orchestration service
    /// Handles workflow instance creation, task execution, and state management
    /// </summary>
    public interface IWorkflowService
    {
        /// <summary>
        /// Create a new workflow instance from a template
        /// </summary>
        WorkflowInstanceResult CreateWorkflowInstance(CreateWorkflowInstanceRequest request, UserContext userContext);

        /// <summary>
        /// Get a specific workflow instance
        /// </summary>
        WorkflowInstanceResult GetWorkflowInstance(int workflowInstanceId, UserContext userContext);

        /// <summary>
        /// Get workflow instances with filtering and pagination
        /// </summary>
        PagedResult<WorkflowInstanceResult> GetWorkflowInstances(WorkflowInstanceFilter filter, UserContext userContext);

        /// <summary>
        /// Complete a workflow step and advance to next step(s)
        /// </summary>
        WorkflowStepInstanceResult CompleteWorkflowStep(CompleteWorkflowStepRequest request, UserContext userContext);

        /// <summary>
        /// Reject a workflow step and route to rejection handler
        /// </summary>
        WorkflowStepInstanceResult RejectWorkflowStep(RejectWorkflowStepRequest request, UserContext userContext);

        /// <summary>
        /// Suspend a workflow instance
        /// </summary>
        WorkflowInstanceResult SuspendWorkflowInstance(int workflowInstanceId, UserContext userContext);

        /// <summary>
        /// Resume a suspended workflow instance
        /// </summary>
        WorkflowInstanceResult ResumeWorkflowInstance(int workflowInstanceId, UserContext userContext);

        /// <summary>
        /// Get all tasks assigned to the current user
        /// </summary>
        List<WorkflowTaskResult> GetMyTasks(UserContext userContext);

        /// <summary>
        /// Get all workflow instances (admin view)
        /// </summary>
        PagedResult<WorkflowInstanceResult> GetAllWorkflows(WorkflowInstanceFilter filter, UserContext userContext);
    }
}
