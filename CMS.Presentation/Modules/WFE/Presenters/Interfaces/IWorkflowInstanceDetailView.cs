using System;
using System.Collections.Generic;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow.Presenters.Interfaces
{
    /// <summary>
    /// View interface for Workflow Instance Detail
    /// </summary>
    public interface IWorkflowInstanceDetailView
    {
        /// <summary>
        /// Display workflow instance details
        /// </summary>
        void DisplayInstanceDetails(WorkflowInstanceResult instance);

        /// <summary>
        /// Display step execution history
        /// </summary>
        void DisplayStepHistory(List<WorkflowStepInstanceResult> history);

        /// <summary>
        /// Display current step details
        /// </summary>
        void DisplayCurrentStep(WorkflowStepInstanceResult currentStep);

        /// <summary>
        /// Enable/disable action buttons
        /// </summary>
        void SetActionButtonsEnabled(bool canSuspend, bool canResume, bool canCancel);

        /// <summary>
        /// Show error message
        /// </summary>
        void ShowError(string message);

        /// <summary>
        /// Show info message
        /// </summary>
        void ShowInfo(string message);
    }
}
