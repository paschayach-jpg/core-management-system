using System;
using System.Collections.Generic;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow.Presenters
{
    /// <summary>
    /// Presenter for Workflow Designer (Visual template editor)
    /// </summary>
    public class WorkflowDesignerPresenter
    {
        private readonly IWorkflowDesignerView _view;
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;
        private int? _currentTemplateId;

        public WorkflowDesignerPresenter(
            IWorkflowDesignerView view,
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        /// <summary>
        /// Load template for editing
        /// </summary>
        public void LoadTemplate(int templateId)
        {
            try
            {
                _currentTemplateId = templateId;
                var template = _templateService.GetTemplate(templateId, _userContext);
                _view.DisplayTemplate(template);
                _view.ClearCanvas();

                var steps = _templateService.GetTemplateSteps(templateId, _userContext);
                if (steps != null && steps.Count > 0)
                {
                    foreach (var step in steps)
                        _view.DrawStep(step);

                    var connections = _templateService.GetStepConnections(templateId, _userContext);
                    if (connections != null)
                    {
                        foreach (var conn in connections)
                            _view.DrawConnection(conn.FromStepId, conn.ToStepId, conn.Condition);
                    }
                }

                _loggingService.Info($"Loaded template {templateId} for editing");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading template: {ex.Message}", ex);
                _view.ShowError($"Failed to load template: {ex.Message}");
            }
        }

        /// <summary>
        /// Create new template
        /// </summary>
        public void CreateNewTemplate()
        {
            try
            {
                _currentTemplateId = null;
                _view.ClearCanvas();
                _loggingService.Info("Created new template for editing");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error creating new template: {ex.Message}", ex);
                _view.ShowError($"Failed to create new template: {ex.Message}");
            }
        }

        /// <summary>
        /// Add step to canvas
        /// </summary>
        public void AddStep(WorkflowStepDesignData step)
        {
            try
            {
                _view.DrawStep(step);
                _loggingService.Info($"Added step {step.StepName} to canvas");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error adding step: {ex.Message}", ex);
                _view.ShowError($"Failed to add step: {ex.Message}");
            }
        }

        /// <summary>
        /// Add connection between steps
        /// </summary>
        public void AddConnection(int fromStepId, int toStepId, string condition)
        {
            try
            {
                _view.DrawConnection(fromStepId, toStepId, condition);
                _loggingService.Info($"Added connection from step {fromStepId} to step {toStepId}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error adding connection: {ex.Message}", ex);
                _view.ShowError($"Failed to add connection: {ex.Message}");
            }
        }

        /// <summary>
        /// Edit step configuration
        /// </summary>
        public void EditStep(WorkflowStepDesignData step)
        {
            try
            {
                _view.ShowStepConfigDialog(step);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error editing step: {ex.Message}", ex);
                _view.ShowError($"Failed to edit step: {ex.Message}");
            }
        }

        /// <summary>
        /// Edit connection configuration
        /// </summary>
        public void EditConnection(int fromStepId, int toStepId)
        {
            try
            {
                _view.ShowConnectionConfigDialog(fromStepId, toStepId);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error editing connection: {ex.Message}", ex);
                _view.ShowError($"Failed to edit connection: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate template design
        /// </summary>
        public bool ValidateTemplate()
        {
            try
            {
                var errors = new List<string>();

                if (!_currentTemplateId.HasValue)
                {
                    errors.Add("No template loaded");
                }
                else
                {
                    var steps = _templateService.GetTemplateSteps(_currentTemplateId.Value, _userContext);
                    var connections = _templateService.GetStepConnections(_currentTemplateId.Value, _userContext);

                    if (steps == null || steps.Count == 0)
                        errors.Add("Template has no steps");
                    else
                    {
                        bool hasStart = steps.Any(s => s.StepType == "Start");
                        bool hasEnd = steps.Any(s => s.StepType == "End");

                        if (!hasStart)
                            errors.Add("Template must have a Start step");
                        if (!hasEnd)
                            errors.Add("Template must have an End step");

                        if (connections == null || connections.Count == 0)
                            errors.Add("Template has no connections between steps");
                    }
                }

                if (errors.Count > 0)
                {
                    _view.DisplayValidationErrors(errors);
                    return false;
                }

                _view.ShowInfo("Template validation passed");
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error validating template: {ex.Message}", ex);
                _view.ShowError($"Validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Save template
        /// </summary>
        public void SaveTemplate(string templateName, string description)
        {
            try
            {
                if (!ValidateTemplate())
                    return;

                if (_currentTemplateId.HasValue)
                {
                    // Update existing template
                    var request = new UpdateTemplateRequest
                    {
                        WorkflowDefinitionId = _currentTemplateId.Value,
                        Name = templateName,
                        Description = description,
                        IsActive = true,
                        ChangeDescription = "Updated via designer"
                    };

                    _templateService.UpdateTemplate(request, _userContext);
                    _view.ShowInfo("Template saved successfully");
                }
                else
                {
                    // Create new template
                    var request = new CreateTemplateRequest
                    {
                        Name = templateName,
                        Description = description,
                        IsActive = false
                    };

                    var result = _templateService.CreateTemplate(request, _userContext);
                    _currentTemplateId = result.WorkflowDefinitionId;
                    _view.ShowInfo("Template created successfully");
                }

                _loggingService.Info($"Template {templateName} saved successfully");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error saving template: {ex.Message}", ex);
                _view.ShowError($"Failed to save template: {ex.Message}");
            }
        }
    }
}
