using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Presenters;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    /// <summary>
    /// Visual designer for creating/editing workflow templates
    /// </summary>
    public partial class WorkflowDesignerForm : Form, IWorkflowDesignerView
    {
        private readonly WorkflowDesignerPresenter _presenter;
        private readonly UserContext _userContext;
        private int? _templateId;
        private List<WorkflowStepDesignData> _steps;
        private List<WorkflowConnectionDesignData> _connections;
        private WorkflowStepDesignData _selectedStep;
        private WorkflowConnectionDesignData _selectedConnection;

        public WorkflowDesignerForm(
            int? templateId,
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            InitializeComponent();
            _templateId = templateId;
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));

            _presenter = new WorkflowDesignerPresenter(
                this,
                templateService,
                loggingService,
                userContext);

            _steps = new List<WorkflowStepDesignData>();
            _connections = new List<WorkflowConnectionDesignData>();
        }

        private void WorkflowDesignerForm_Load(object sender, EventArgs e)
        {
            try
            {
                InitializeControls();

                if (_templateId.HasValue)
                {
                    _presenter.LoadTemplate(_templateId.Value);
                }
                else
                {
                    _presenter.CreateNewTemplate();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading designer: {ex.Message}");
            }
        }

        private void InitializeControls()
        {
            // Initialize step type dropdown
            cmbStepType.Items.Clear();
            cmbStepType.Items.Add("Start");
            cmbStepType.Items.Add("Approval");
            cmbStepType.Items.Add("Review");
            cmbStepType.Items.Add("Notification");
            cmbStepType.Items.Add("Parallel");
            cmbStepType.Items.Add("Decision");
            cmbStepType.Items.Add("Merge");
            cmbStepType.Items.Add("End");
            cmbStepType.SelectedIndex = 0;

            // Disable edit/delete buttons initially
            btnEditStep.Enabled = false;
            btnDeleteStep.Enabled = false;
            btnEditConnection.Enabled = false;
            btnDeleteConnection.Enabled = false;
        }

        public void DisplayTemplate(WorkflowTemplateResult template)
        {
            try
            {
                if (template == null)
                    return;

                txtTemplateName.Text = template.Name;
                txtTemplateDescription.Text = template.Description;
            }
            catch (Exception ex)
            {
                ShowError($"Error displaying template: {ex.Message}");
            }
        }

        public void DrawStep(WorkflowStepDesignData step)
        {
            try
            {
                if (step == null)
                    return;

                _steps.Add(step);
                RefreshStepsList();
                pnlCanvas.Invalidate();
            }
            catch (Exception ex)
            {
                ShowError($"Error drawing step: {ex.Message}");
            }
        }

        public void DrawConnection(int fromStepId, int toStepId, string condition)
        {
            try
            {
                var connection = new WorkflowConnectionDesignData
                {
                    FromStepId = fromStepId,
                    ToStepId = toStepId,
                    Condition = condition
                };

                _connections.Add(connection);
                RefreshConnectionsList();
                pnlCanvas.Invalidate();
            }
            catch (Exception ex)
            {
                ShowError($"Error drawing connection: {ex.Message}");
            }
        }

        public void ClearCanvas()
        {
            _steps.Clear();
            _connections.Clear();
            RefreshStepsList();
            RefreshConnectionsList();
            pnlCanvas.Invalidate();
        }

        public void ShowStepConfigDialog(WorkflowStepDesignData step)
        {
            // TODO: Show step configuration dialog
        }

        public void ShowConnectionConfigDialog(int fromStepId, int toStepId)
        {
            // TODO: Show connection configuration dialog
        }

        public void DisplayValidationErrors(List<string> errors)
        {
            try
            {
                if (errors == null || errors.Count == 0)
                {
                    ShowInfo("Template validation passed successfully!");
                    return;
                }

                var errorMessage = "Validation errors found:\n\n" + string.Join("\n", errors);
                ShowError(errorMessage);
            }
            catch (Exception ex)
            {
                ShowError($"Error displaying validation errors: {ex.Message}");
            }
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RefreshStepsList()
        {
            lstSteps.Items.Clear();
            foreach (var step in _steps)
            {
                lstSteps.Items.Add($"{step.StepName} ({step.StepType})");
            }
        }

        private void RefreshConnectionsList()
        {
            lstConnections.Items.Clear();
            foreach (var conn in _connections)
            {
                var fromStep = _steps.FirstOrDefault(s => s.WorkflowStepId == conn.FromStepId);
                var toStep = _steps.FirstOrDefault(s => s.WorkflowStepId == conn.ToStepId);
                var fromName = fromStep?.StepName ?? "Unknown";
                var toName = toStep?.StepName ?? "Unknown";
                lstConnections.Items.Add($"{fromName} → {toName}");
            }
        }

        private void btnAddStep_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cmbStepType.SelectedItem?.ToString()))
                {
                    ShowError("Please select a step type.");
                    return;
                }

                var stepType = cmbStepType.SelectedItem.ToString();
                var stepId = _steps.Count + 1;
                var stepName = $"{stepType}_{stepId}";

                var step = new WorkflowStepDesignData
                {
                    WorkflowStepId = stepId,
                    StepName = stepName,
                    StepType = stepType,
                    CanvasX = 50 + (stepId * 100),
                    CanvasY = 50,
                    AssignedUser = _userContext.ADUserName,
                    Description = ""
                };

                DrawStep(step);
                ShowInfo($"Step '{stepName}' added successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error adding step: {ex.Message}");
            }
        }

        private void btnAddConnection_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedStep == null)
                {
                    ShowError("Please select a source step first.");
                    return;
                }

                // TODO: Show dialog to select target step and condition
                ShowInfo("Select target step by clicking on the canvas.");
            }
            catch (Exception ex)
            {
                ShowError($"Error adding connection: {ex.Message}");
            }
        }

        private void btnEditStep_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedStep == null)
                {
                    ShowError("Please select a step to edit.");
                    return;
                }

                // TODO: Show step configuration dialog
                ShowInfo($"Editing step: {_selectedStep.StepName}");
            }
            catch (Exception ex)
            {
                ShowError($"Error editing step: {ex.Message}");
            }
        }

        private void btnDeleteStep_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedStep == null)
                {
                    ShowError("Please select a step to delete.");
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete step '{_selectedStep.StepName}'?",
                    "Confirm Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _steps.Remove(_selectedStep);
                    _connections.RemoveAll(c => c.FromStepId == _selectedStep.WorkflowStepId || c.ToStepId == _selectedStep.WorkflowStepId);
                    RefreshStepsList();
                    RefreshConnectionsList();
                    pnlCanvas.Invalidate();
                    _selectedStep = null;
                    btnEditStep.Enabled = false;
                    btnDeleteStep.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting step: {ex.Message}");
            }
        }

        private void btnEditConnection_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedConnection == null)
                {
                    ShowError("Please select a connection to edit.");
                    return;
                }

                // TODO: Show connection configuration dialog
                ShowInfo("Editing connection...");
            }
            catch (Exception ex)
            {
                ShowError($"Error editing connection: {ex.Message}");
            }
        }

        private void btnDeleteConnection_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedConnection == null)
                {
                    ShowError("Please select a connection to delete.");
                    return;
                }

                var result = MessageBox.Show(
                    "Are you sure you want to delete this connection?",
                    "Confirm Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _connections.Remove(_selectedConnection);
                    RefreshConnectionsList();
                    pnlCanvas.Invalidate();
                    _selectedConnection = null;
                    btnEditConnection.Enabled = false;
                    btnDeleteConnection.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting connection: {ex.Message}");
            }
        }

        private void lstSteps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstSteps.SelectedIndex >= 0 && lstSteps.SelectedIndex < _steps.Count)
            {
                _selectedStep = _steps[lstSteps.SelectedIndex];
                btnEditStep.Enabled = true;
                btnDeleteStep.Enabled = true;
            }
            else
            {
                _selectedStep = null;
                btnEditStep.Enabled = false;
                btnDeleteStep.Enabled = false;
            }
        }

        private void lstConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstConnections.SelectedIndex >= 0 && lstConnections.SelectedIndex < _connections.Count)
            {
                _selectedConnection = _connections[lstConnections.SelectedIndex];
                btnEditConnection.Enabled = true;
                btnDeleteConnection.Enabled = true;
            }
            else
            {
                _selectedConnection = null;
                btnEditConnection.Enabled = false;
                btnDeleteConnection.Enabled = false;
            }
        }

        private void pnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                // Draw connections first (so they appear behind steps)
                foreach (var conn in _connections)
                {
                    var fromStep = _steps.FirstOrDefault(s => s.WorkflowStepId == conn.FromStepId);
                    var toStep = _steps.FirstOrDefault(s => s.WorkflowStepId == conn.ToStepId);

                    if (fromStep != null && toStep != null)
                    {
                        var pen = new Pen(Color.Black, 2);
                        e.Graphics.DrawLine(pen, fromStep.CanvasX + 40, fromStep.CanvasY + 20, toStep.CanvasX, toStep.CanvasY + 20);
                        pen.Dispose();
                    }
                }

                // Draw steps
                foreach (var step in _steps)
                {
                    var brush = _selectedStep == step ? Brushes.LightBlue : Brushes.LightGray;
                    e.Graphics.FillRectangle(brush, step.CanvasX, step.CanvasY, 80, 40);
                    e.Graphics.DrawRectangle(Pens.Black, step.CanvasX, step.CanvasY, 80, 40);

                    var font = new Font("Arial", 8);
                    e.Graphics.DrawString(step.StepName, font, Brushes.Black, step.CanvasX + 5, step.CanvasY + 5);
                    font.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error painting canvas: {ex.Message}");
            }
        }

        private void pnlCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                // Check if clicked on a step
                foreach (var step in _steps)
                {
                    if (e.X >= step.CanvasX && e.X <= step.CanvasX + 80 && e.Y >= step.CanvasY && e.Y <= step.CanvasY + 40)
                    {
                        _selectedStep = step;
                        lstSteps.SelectedIndex = _steps.IndexOf(step);
                        pnlCanvas.Invalidate();
                        return;
                    }
                }

                _selectedStep = null;
                lstSteps.SelectedIndex = -1;
                pnlCanvas.Invalidate();
            }
            catch (Exception ex)
            {
                ShowError($"Error handling canvas click: {ex.Message}");
            }
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            _presenter.ValidateTemplate();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTemplateName.Text))
                {
                    ShowError("Please enter a template name.");
                    return;
                }

                _presenter.SaveTemplate(txtTemplateName.Text, txtTemplateDescription.Text);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Error saving template: {ex.Message}");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }



    /// <summary>
    /// Helper class for workflow connection design data
    /// </summary>
    public class WorkflowConnectionDesignData
    {
        public int FromStepId { get; set; }
        public int ToStepId { get; set; }
        public string Condition { get; set; }
    }
}
