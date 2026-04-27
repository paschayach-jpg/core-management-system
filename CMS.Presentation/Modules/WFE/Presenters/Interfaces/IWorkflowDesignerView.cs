using System;
using System.Collections.Generic;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow.Presenters.Interfaces
{
    /// <summary>
    /// View interface for Workflow Designer (Visual template editor)
    /// </summary>
    public interface IWorkflowDesignerView
    {
        /// <summary>
        /// Display template in designer
        /// </summary>
        void DisplayTemplate(WorkflowTemplateResult template);

        /// <summary>
        /// Draw a workflow step on canvas
        /// </summary>
        void DrawStep(WorkflowStepDesignData step);

        /// <summary>
        /// Draw a connection between steps
        /// </summary>
        void DrawConnection(int fromStepId, int toStepId, string condition);

        /// <summary>
        /// Clear the canvas
        /// </summary>
        void ClearCanvas();

        /// <summary>
        /// Show step configuration dialog
        /// </summary>
        void ShowStepConfigDialog(WorkflowStepDesignData step);

        /// <summary>
        /// Show connection configuration dialog
        /// </summary>
        void ShowConnectionConfigDialog(int fromStepId, int toStepId);

        /// <summary>
        /// Display validation errors
        /// </summary>
        void DisplayValidationErrors(List<string> errors);

        /// <summary>
        /// Show error message
        /// </summary>
        void ShowError(string message);

        /// <summary>
        /// Show info message
        /// </summary>
        void ShowInfo(string message);
    }

    /// <summary>
    /// Workflow step design data for canvas rendering
    /// </summary>
    public class WorkflowStepDesignData
    {
        public int WorkflowStepId { get; set; }
        public string StepName { get; set; }
        public string StepType { get; set; }
        public int CanvasX { get; set; }
        public int CanvasY { get; set; }
        public string AssignedUser { get; set; }
        public string Description { get; set; }
    }
}
