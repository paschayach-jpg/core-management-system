using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using Newtonsoft.Json;

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
                LoadTemplateDefinition(template);

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
            return ValidateTemplate(true, false);
        }

        public bool ValidateTemplate(bool showSuccessMsg, bool isSavingNew)
        {
            try
            {
                var errors = new List<string>();
                var definition = GetCurrentDesignerDefinition();

                if (!_currentTemplateId.HasValue)
                {
                    if (!isSavingNew)
                    {
                        errors.Add("No template loaded");
                    }
                }
                else
                {
                    var template = _templateService.GetTemplate(_currentTemplateId.Value, _userContext);
                    if (template == null)
                        errors.Add("Template not found");
                }

                if (definition.Steps.Count == 0)
                {
                    errors.Add("Template has no steps");
                }
                else
                {
                    var hasStart = definition.Steps.Any(s => string.Equals(s.StepType, "Start", StringComparison.OrdinalIgnoreCase));
                    var hasEnd = definition.Steps.Any(s => string.Equals(s.StepType, "End", StringComparison.OrdinalIgnoreCase));
                    if (!hasStart)
                        errors.Add("Template requires a Start step");
                    if (!hasEnd)
                        errors.Add("Template requires an End step");
                }

                if (errors.Count > 0)
                {
                    _view.DisplayValidationErrors(errors);
                    return false;
                }

                if (showSuccessMsg)
                {
                    _view.ShowInfo("Template validation passed");
                }
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
                bool isSavingNew = !_currentTemplateId.HasValue;
                if (!ValidateTemplate(showSuccessMsg: false, isSavingNew: isSavingNew))
                    return;

                if (_currentTemplateId.HasValue)
                {
                    // Update existing template
                    var request = new UpdateTemplateRequest
                    {
                        WorkflowDefinitionId = _currentTemplateId.Value,
                        Name = templateName,
                        Description = description,
                        TemplateDefinition = _view.GetTemplateDefinitionJson(),
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
                        TemplateDefinition = _view.GetTemplateDefinitionJson(),
                        IsActive = false
                    };

                    var result = _templateService.CreateTemplate(request, _userContext);
                    _currentTemplateId = result.WorkflowDefinitionId;
                    _view.ShowInfo("Template created successfully");
                }

                _loggingService.Info($"Template {templateName} saved successfully");

                // Reload template to sync the view and update labels/IDs
                if (_currentTemplateId.HasValue)
                {
                    LoadTemplate(_currentTemplateId.Value);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error saving template: {ex.Message}", ex);
                _view.ShowError($"Failed to save template: {ex.Message}");
            }
        }

        private void LoadTemplateDefinition(WorkflowTemplateResult template)
        {
            if (template == null || string.IsNullOrWhiteSpace(template.TemplateDefinition))
                return;

            try
            {
                var json = template.TemplateDefinition.Trim();
                if (!json.StartsWith("{")) return;

                var designerDefinition = JsonConvert.DeserializeObject<WorkflowDesignerTemplateDefinition>(json);
                if (designerDefinition != null && designerDefinition.Steps != null && designerDefinition.Steps.Count > 0)
                {
                    foreach (var step in designerDefinition.Steps.OrderBy(s => s.WorkflowStepId))
                        _view.DrawStep(step);

                    if (designerDefinition.Connections != null)
                    {
                        foreach (var conn in designerDefinition.Connections)
                            _view.DrawConnection(conn.FromStepId, conn.ToStepId, conn.Condition);
                    }
                    return;
                }

                var wizardState = JsonConvert.DeserializeObject<WorkflowWizardState>(json);
                if (wizardState?.Steps == null || wizardState.Steps.Count == 0)
                    return;

                var stepMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var orderedSteps = wizardState.Steps.OrderBy(s => s.StepNumber).ToList();
                for (int i = 0; i < orderedSteps.Count; i++)
                {
                    var wizardStep = orderedSteps[i];
                    var stepId = i + 1;
                    stepMap[wizardStep.StepName ?? string.Empty] = stepId;
                    _view.DrawStep(new WorkflowStepDesignData
                    {
                        WorkflowStepId = stepId,
                        StepName = string.IsNullOrWhiteSpace(wizardStep.StepName) ? $"Step {stepId}" : wizardStep.StepName,
                        StepType = MapWizardActionToDesignerType(wizardStep.Action),
                        CanvasX = 60 + i * 160,
                        CanvasY = 120,
                        Description = wizardStep.Action
                    });
                }

                foreach (var wizardStep in orderedSteps)
                {
                    if (string.IsNullOrWhiteSpace(wizardStep.NextStep)) continue;
                    if (!stepMap.TryGetValue(wizardStep.StepName ?? string.Empty, out var fromId)) continue;
                    if (!stepMap.TryGetValue(wizardStep.NextStep, out var toId)) continue;
                    _view.DrawConnection(fromId, toId, string.Empty);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Warn($"TemplateDefinition could not be loaded into designer: {ex.Message}");
            }
        }

        private static string MapWizardActionToDesignerType(string action)
        {
            if (string.IsNullOrWhiteSpace(action)) return "Approval";
            if (action.IndexOf("review", StringComparison.OrdinalIgnoreCase) >= 0) return "Review";
            if (action.IndexOf("notify", StringComparison.OrdinalIgnoreCase) >= 0) return "Notification";
            if (action.IndexOf("decision", StringComparison.OrdinalIgnoreCase) >= 0) return "Decision";
            if (action.IndexOf("start", StringComparison.OrdinalIgnoreCase) >= 0) return "Start";
            if (action.IndexOf("end", StringComparison.OrdinalIgnoreCase) >= 0) return "End";
            return "Approval";
        }

        private WorkflowDesignerTemplateDefinition GetCurrentDesignerDefinition()
        {
            try
            {
                var json = _view.GetTemplateDefinitionJson();
                if (string.IsNullOrWhiteSpace(json))
                    return new WorkflowDesignerTemplateDefinition();

                var definition = JsonConvert.DeserializeObject<WorkflowDesignerTemplateDefinition>(json);
                return definition ?? new WorkflowDesignerTemplateDefinition();
            }
            catch (Exception ex)
            {
                _loggingService.Warn($"Unable to read current designer definition: {ex.Message}");
                return new WorkflowDesignerTemplateDefinition();
            }
        }
    }
}
