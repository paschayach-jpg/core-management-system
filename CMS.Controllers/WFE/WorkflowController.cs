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
    /// Main workflow controller for workflow instance management
    /// </summary>
    public class WorkflowController
    {
        private readonly IWorkflowService _workflowService;
        private readonly ILoggingService _loggingService;

        public WorkflowController(IWorkflowService workflowService = null, ILoggingService loggingService = null)
        {
            _workflowService = workflowService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Create a new workflow instance
        /// </summary>
        public WorkflowInstanceResult CreateWorkflowInstance(CreateWorkflowInstanceRequest request, UserContext userContext)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Creating workflow instance from template {request.WorkflowDefinitionId}");
                return _workflowService.CreateWorkflowInstance(request, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error creating workflow instance: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get a specific workflow instance
        /// </summary>
        public WorkflowInstanceResult GetWorkflowInstance(int workflowInstanceId, UserContext userContext)
        {
            try
            {
                if (workflowInstanceId <= 0)
                    throw new ArgumentException("Invalid workflow instance ID");

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Getting workflow instance {workflowInstanceId}");
                return _workflowService.GetWorkflowInstance(workflowInstanceId, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error getting workflow instance: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Get workflow instances with filtering
        /// </summary>
        public PagedResult<WorkflowInstanceResult> GetWorkflowInstances(WorkflowInstanceFilter filter, UserContext userContext)
        {
            try
            {
                if (filter == null)
                    throw new ArgumentNullException(nameof(filter));

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Getting workflow instances with filter");
                return _workflowService.GetWorkflowInstances(filter, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error getting workflow instances: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Complete a workflow step
        /// </summary>
        public WorkflowStepInstanceResult CompleteWorkflowStep(CompleteWorkflowStepRequest request, UserContext userContext)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Completing workflow step {request.WorkflowStepInstanceId}");
                return _workflowService.CompleteWorkflowStep(request, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error completing workflow step: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Reject a workflow step
        /// </summary>
        public WorkflowStepInstanceResult RejectWorkflowStep(RejectWorkflowStepRequest request, UserContext userContext)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Rejecting workflow step {request.WorkflowStepInstanceId}");
                return _workflowService.RejectWorkflowStep(request, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error rejecting workflow step: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Suspend a workflow instance
        /// </summary>
        public WorkflowInstanceResult SuspendWorkflowInstance(int workflowInstanceId, UserContext userContext)
        {
            try
            {
                if (workflowInstanceId <= 0)
                    throw new ArgumentException("Invalid workflow instance ID");

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Suspending workflow instance {workflowInstanceId}");
                return _workflowService.SuspendWorkflowInstance(workflowInstanceId, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error suspending workflow instance: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Resume a suspended workflow instance
        /// </summary>
        public WorkflowInstanceResult ResumeWorkflowInstance(int workflowInstanceId, UserContext userContext)
        {
            try
            {
                if (workflowInstanceId <= 0)
                    throw new ArgumentException("Invalid workflow instance ID");

                if (userContext == null)
                    throw new ArgumentNullException(nameof(userContext));

                _loggingService?.Info($"Resuming workflow instance {workflowInstanceId}");
                return _workflowService.ResumeWorkflowInstance(workflowInstanceId, userContext);
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Error resuming workflow instance: {ex.Message}", ex);
                throw;
            }
        }
    }
}
