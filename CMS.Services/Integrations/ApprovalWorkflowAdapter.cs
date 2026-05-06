using CMS.Domain.Models;
using CMS.Services.Interfaces;
using CMS.Infrastructure.Interfaces;
using System;

namespace CMS.Services.Integrations
{
    /// <summary>
    /// Adapter for integrating with Approval Workflow module (MOD-003)
    /// </summary>
    public class ApprovalWorkflowAdapter
    {
        private readonly IApprovalService _approvalService;
        private readonly ILoggingService _loggingService;

        public ApprovalWorkflowAdapter(IApprovalService approvalService, ILoggingService loggingService)
        {
            _approvalService = approvalService;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Create an approval task when workflow reaches an approval step
        /// </summary>
        public int CreateApprovalTask(WorkflowStepInstance stepInstance, WorkflowStep step, UserContext userContext)
        {
            try
            {
                if (stepInstance == null)
                    throw new ArgumentNullException(nameof(stepInstance));

                if (step == null)
                    throw new ArgumentNullException(nameof(step));

                // TODO: Parse step configuration to extract approval criteria
                // Create approval task through IApprovalService
                // Return approval ID for tracking

                _loggingService.Info($"Approval task created for step {step.StepName} in workflow instance {stepInstance.WorkflowInstanceId}");
                return 0;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error creating approval task: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Handle approval completion and route workflow accordingly
        /// </summary>
        public void HandleApprovalCompletion(int approvalId, bool isApproved, string reason, UserContext userContext)
        {
            try
            {
                // TODO: Retrieve step instance associated with approval
                // If approved: complete the step and move to next step
                // If rejected: reject the step and route to rejection handler

                _loggingService.Info($"Approval {approvalId} handled: {(isApproved ? "Approved" : "Rejected")}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error handling approval completion: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Check if a step is an approval step
        /// </summary>
        public bool IsApprovalStep(WorkflowStep step)
        {
            return step != null && step.StepType == Domain.Enums.WorkflowStepType.Approval;
        }
    }
}
