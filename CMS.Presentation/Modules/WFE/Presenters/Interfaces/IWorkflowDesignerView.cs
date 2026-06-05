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

        /// <summary>
        /// Return current visual design as TemplateDefinition JSON.
        /// </summary>
        string GetTemplateDefinitionJson();
    }

    /// <summary>
    /// Workflow step design data for canvas rendering
    /// </summary>
    public class WorkflowStepDesignData
    {
        public int    WorkflowStepId      { get; set; }
        public string StepName            { get; set; }
        public string StepType            { get; set; }
        public int    CanvasX             { get; set; }
        public int    CanvasY             { get; set; }
        public string AssignedUser        { get; set; }
        public string Description         { get; set; }

        // ── Properties panel fields ──────────────────────────────────────────
        /// <summary>SLA in days (1–365)</summary>
        public int  SLADays            { get; set; } = 2;
        /// <summary>Priority index: 0=ปกติ, 1=สูง, 2=ด่วน, 3=ด่วนมาก</summary>
        public int  Priority           { get; set; } = 0;
        /// <summary>Escalation when SLA exceeded</summary>
        public bool EscalationEnabled  { get; set; } = false;
        /// <summary>Allow Approve action</summary>
        public bool AllowApprove       { get; set; } = true;
        /// <summary>Allow Reject action</summary>
        public bool AllowReject        { get; set; } = true;
        /// <summary>Allow Forward action</summary>
        public bool AllowForward       { get; set; } = false;
        /// <summary>Allow Acknowledge action</summary>
        public bool AllowAck           { get; set; } = false;
        /// <summary>Work Instruction reference (e.g. "WI-002 · การตรวจ Legal")</summary>
        public string WorkInstructionRef { get; set; }
        /// <summary>Condition/Guard expression (multi-line)</summary>
        public string ConditionExpression { get; set; }
    }
}
