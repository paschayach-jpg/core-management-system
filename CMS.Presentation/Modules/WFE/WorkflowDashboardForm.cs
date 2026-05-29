using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Dialogs;
using CMS.Presentation.Modules.Workflow.Presenters;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    /// <summary>
    /// User dashboard showing My Tasks and workflow status
    /// </summary>
    public partial class WorkflowDashboardForm : Form, IWorkflowDashboardView
    {
        private readonly WorkflowDashboardPresenter _presenter;
        private readonly UserContext _userContext;
        private readonly IWorkflowService _workflowService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;
        private Timer _autoRefreshTimer;
        private List<WorkflowTaskResult> _currentTasks;
        private List<WorkflowInstanceResult> _currentInstances;

        public WorkflowDashboardForm(
            IWorkflowService workflowService,
            ISLACalculator slaCalculator,
            IAuditService auditService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            InitializeComponent();
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));

            _presenter = new WorkflowDashboardPresenter(
                this,
                workflowService,
                slaCalculator,
                auditService,
                loggingService,
                userContext);

            _currentTasks = new List<WorkflowTaskResult>();
            _currentInstances = new List<WorkflowInstanceResult>();
        }

        private void WorkflowDashboardForm_Load(object sender, EventArgs e)
        {
            try
            {
                InitializeControls();
                _presenter.LoadMyTasks();
                _presenter.LoadWorkflowInstances();
                _presenter.LoadSLAMetrics();

                // Setup auto-refresh timer (every 30 seconds)
                _autoRefreshTimer = new Timer();
                _autoRefreshTimer.Interval = 30000;
                _autoRefreshTimer.Tick += (s, args) => _presenter.RefreshDashboard();
                _autoRefreshTimer.Start();
            }
            catch (Exception ex)
            {
                ShowError($"Error loading dashboard: {ex.Message}");
            }
        }

        private void InitializeControls()
        {
            // Initialize My Tasks grid columns
            dgvMyTasks.Columns.Clear();
            dgvMyTasks.Columns.Add("TaskId", "Task ID");
            dgvMyTasks.Columns.Add("TaskName", "Task Name");
            dgvMyTasks.Columns.Add("Status", "Status");
            dgvMyTasks.Columns.Add("DueDate", "Due Date");
            dgvMyTasks.Columns.Add("Priority", "Priority");
            dgvMyTasks.Columns.Add("SLAStatus", "SLA Status");
            dgvMyTasks.Columns.Add("AssignedDate", "Assigned Date");

            // Set column widths
            dgvMyTasks.Columns["TaskId"].Width = 50;
            dgvMyTasks.Columns["TaskName"].Width = 200;
            dgvMyTasks.Columns["Status"].Width = 80;
            dgvMyTasks.Columns["DueDate"].Width = 100;
            dgvMyTasks.Columns["Priority"].Width = 80;
            dgvMyTasks.Columns["SLAStatus"].Width = 80;
            dgvMyTasks.Columns["AssignedDate"].Width = 100;

            // Initialize My Workflows grid columns
            dgvMyWorkflows.Columns.Clear();
            dgvMyWorkflows.Columns.Add("InstanceId", "Instance ID");
            dgvMyWorkflows.Columns.Add("TemplateName", "Template Name");
            dgvMyWorkflows.Columns.Add("Status", "Status");
            dgvMyWorkflows.Columns.Add("InitiatedDate", "Initiated Date");
            dgvMyWorkflows.Columns.Add("CurrentStep", "Current Step");
            dgvMyWorkflows.Columns.Add("Progress", "Progress");

            // Set column widths
            dgvMyWorkflows.Columns["InstanceId"].Width = 50;
            dgvMyWorkflows.Columns["TemplateName"].Width = 200;
            dgvMyWorkflows.Columns["Status"].Width = 80;
            dgvMyWorkflows.Columns["InitiatedDate"].Width = 100;
            dgvMyWorkflows.Columns["CurrentStep"].Width = 150;
            dgvMyWorkflows.Columns["Progress"].Width = 80;

            // Initialize filter dropdowns
            cmbTaskStatus.Items.Clear();
            cmbTaskStatus.Items.Add("All");
            cmbTaskStatus.Items.Add("Pending");
            cmbTaskStatus.Items.Add("In Progress");
            cmbTaskStatus.Items.Add("Completed");
            cmbTaskStatus.Items.Add("Rejected");
            cmbTaskStatus.SelectedIndex = 0;

            cmbTaskPriority.Items.Clear();
            cmbTaskPriority.Items.Add("All");
            cmbTaskPriority.Items.Add("Low");
            cmbTaskPriority.Items.Add("Medium");
            cmbTaskPriority.Items.Add("High");
            cmbTaskPriority.Items.Add("Critical");
            cmbTaskPriority.SelectedIndex = 0;

            cmbWorkflowStatus.Items.Clear();
            cmbWorkflowStatus.Items.Add("All");
            cmbWorkflowStatus.Items.Add("Created");
            cmbWorkflowStatus.Items.Add("In Progress");
            cmbWorkflowStatus.Items.Add("Completed");
            cmbWorkflowStatus.Items.Add("Rejected");
            cmbWorkflowStatus.Items.Add("Suspended");
            cmbWorkflowStatus.SelectedIndex = 0;

            // Disable action buttons initially
            btnCompleteTask.Enabled = false;
            btnDelegateTask.Enabled = false;
            btnRejectTask.Enabled = false;
            btnViewDetails.Enabled = false;
        }

        public void DisplayMyTasks(List<WorkflowTaskResult> tasks)
        {
            try
            {
                _currentTasks = tasks ?? new List<WorkflowTaskResult>();
                RefreshTasksGrid();
            }
            catch (Exception ex)
            {
                ShowError($"Error displaying tasks: {ex.Message}");
            }
        }

        public void DisplayWorkflowInstances(PagedResult<WorkflowInstanceResult> instances)
        {
            try
            {
                _currentInstances = instances?.Items?.ToList() ?? new List<WorkflowInstanceResult>();
                RefreshWorkflowsGrid();
            }
            catch (Exception ex)
            {
                ShowError($"Error displaying workflow instances: {ex.Message}");
            }
        }

        public void DisplaySLAMetrics(SLAComplianceMetricsResult metrics)
        {
            try
            {
                if (metrics == null)
                    return;

                lblOnTimeCount.Text = metrics.OnTimeCount.ToString();
                lblAtRiskCount.Text = metrics.AtRiskCount.ToString();
                lblOverdueCount.Text = metrics.OverdueCount.ToString();
                lblCompliancePercentage.Text = $"{metrics.CompliancePercentage}%";
                prgCompliancePercentage.Value = Math.Min((int)metrics.CompliancePercentage, 100);

                // Set progress bar color based on compliance
                if (metrics.CompliancePercentage >= 90)
                    prgCompliancePercentage.ForeColor = System.Drawing.Color.Green;
                else if (metrics.CompliancePercentage >= 75)
                    prgCompliancePercentage.ForeColor = System.Drawing.Color.Orange;
                else
                    prgCompliancePercentage.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                ShowError($"Error displaying SLA metrics: {ex.Message}");
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

        public void SetRefreshEnabled(bool enabled)
        {
            btnRefresh.Enabled = enabled;
        }

        private void RefreshTasksGrid()
        {
            dgvMyTasks.Rows.Clear();

            var filteredTasks = _currentTasks;

            // Apply status filter
            if (cmbTaskStatus.SelectedIndex > 0)
            {
                var selectedStatus = cmbTaskStatus.SelectedItem.ToString();
                filteredTasks = filteredTasks.Where(t => t.Status.ToString() == selectedStatus).ToList();
            }

            // Apply priority filter
            if (cmbTaskPriority.SelectedIndex > 0)
            {
                var selectedPriority = cmbTaskPriority.SelectedItem.ToString();
                filteredTasks = filteredTasks.Where(t => t.Priority.ToString() == selectedPriority).ToList();
            }

            // Bind to grid
            foreach (var task in filteredTasks)
            {
                dgvMyTasks.Rows.Add(
                    task.WorkflowStepInstanceId,
                    task.StepName,
                    task.Status,
                    task.SLADeadline.ToString("yyyy-MM-dd HH:mm"),
                    task.Priority,
                    task.SLAStatus,
                    task.AssignedDate.ToString("yyyy-MM-dd HH:mm")
                );
            }
        }

        private void RefreshWorkflowsGrid()
        {
            dgvMyWorkflows.Rows.Clear();

            var filteredInstances = _currentInstances;

            // Apply status filter
            if (cmbWorkflowStatus.SelectedIndex > 0)
            {
                var selectedStatus = cmbWorkflowStatus.SelectedItem.ToString();
                filteredInstances = filteredInstances.Where(i => i.Status.ToString() == selectedStatus).ToList();
            }

            // Bind to grid
            foreach (var instance in filteredInstances)
            {
                dgvMyWorkflows.Rows.Add(
                    instance.WorkflowInstanceId,
                    instance.TemplateName,
                    instance.Status,
                    instance.InitiatedDate.ToString("yyyy-MM-dd HH:mm"),
                    instance.CurrentStepName ?? "N/A",
                    $"{instance.CompletedStepCount}/{instance.TotalStepCount}"
                );
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _presenter.RefreshDashboard();
        }

        private void btnNewWorkflow_Click(object sender, EventArgs e)
        {
            try
            {
                var dialog = new WorkflowStartDialog(null, null, null);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _presenter.RefreshDashboard();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error creating new workflow: {ex.Message}");
            }
        }

        private void cmbTaskStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshTasksGrid();
        }

        private void cmbTaskPriority_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshTasksGrid();
        }

        private void cmbWorkflowStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshWorkflowsGrid();
        }

        private void dgvMyTasks_SelectionChanged(object sender, EventArgs e)
        {
            btnCompleteTask.Enabled = dgvMyTasks.SelectedRows.Count > 0;
            btnDelegateTask.Enabled = dgvMyTasks.SelectedRows.Count > 0;
            btnRejectTask.Enabled = dgvMyTasks.SelectedRows.Count > 0;
        }

        private void dgvMyWorkflows_SelectionChanged(object sender, EventArgs e)
        {
            btnViewDetails.Enabled = dgvMyWorkflows.SelectedRows.Count > 0;
        }

        private void btnCompleteTask_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMyTasks.SelectedRows.Count == 0)
                {
                    ShowError("Please select a task to complete.");
                    return;
                }

                var selectedRow = dgvMyTasks.SelectedRows[0];
                var stepInstanceId = (int)selectedRow.Cells["TaskId"].Value;
                var task = _currentTasks.FirstOrDefault(t => t.WorkflowStepInstanceId == stepInstanceId);

                if (task == null)
                {
                    ShowError("Task not found.");
                    return;
                }

                var actionForm = new WorkflowStepActionForm(task);
                if (actionForm.ShowDialog() == DialogResult.OK)
                {
                    _presenter.RefreshDashboard();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error completing task: {ex.Message}");
            }
        }

        private void btnDelegateTask_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMyTasks.SelectedRows.Count == 0)
                {
                    ShowError("Please select a task to delegate.");
                    return;
                }

                var selectedRow = dgvMyTasks.SelectedRows[0];
                var stepInstanceId = (int)selectedRow.Cells["TaskId"].Value;
                var task = _currentTasks.FirstOrDefault(t => t.WorkflowStepInstanceId == stepInstanceId);

                if (task == null)
                {
                    ShowError("Task not found.");
                    return;
                }

                var delegationDialog = new DelegationDialog(task);
                if (delegationDialog.ShowDialog() == DialogResult.OK)
                {
                    _presenter.RefreshDashboard();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error delegating task: {ex.Message}");
            }
        }

        private void btnRejectTask_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMyTasks.SelectedRows.Count == 0)
                {
                    ShowError("Please select a task to reject.");
                    return;
                }

                var selectedRow = dgvMyTasks.SelectedRows[0];
                var stepInstanceId = (int)selectedRow.Cells["TaskId"].Value;
                var task = _currentTasks.FirstOrDefault(t => t.WorkflowStepInstanceId == stepInstanceId);

                if (task == null)
                {
                    ShowError("Task not found.");
                    return;
                }

                var result = MessageBox.Show(
                    "Are you sure you want to reject this task?",
                    "Confirm Rejection",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (stepInstanceId > 0)
                    {
                        try
                        {
                            _workflowService.RejectWorkflowStep(new CMS.Services.DTOs.RejectWorkflowStepRequest
                            {
                                WorkflowStepInstanceId = stepInstanceId,
                                RejectionReason = "Rejected from dashboard"
                            }, _userContext);

                            _auditService.Log(new AuditEntry
                            {
                                Action = "WorkflowStepRejected",
                                TableName = "WorkflowStepInstance",
                                RecordID = stepInstanceId,
                                UserName = _userContext.ADUserName,
                                AuditDate = DateTime.Now,
                                NewValue = "Rejected from dashboard"
                            });

                            _loggingService.Info($"Rejected workflow step {stepInstanceId}");
                            ShowInfo("Task rejected successfully.");
                        }
                        catch (Exception ex)
                        {
                            ShowError($"Failed to reject task: {ex.Message}");
                        }
                        _presenter.RefreshDashboard();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error rejecting task: {ex.Message}");
            }
        }

        private void btnViewDetails_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMyWorkflows.SelectedRows.Count == 0)
                {
                    ShowError("Please select a workflow to view.");
                    return;
                }

                var selectedRow = dgvMyWorkflows.SelectedRows[0];
                var instanceId = (int)selectedRow.Cells["InstanceId"].Value;
                var instance = _currentInstances.FirstOrDefault(i => i.WorkflowInstanceId == instanceId);

                if (instance == null)
                {
                    ShowError("Workflow instance not found.");
                    return;
                }

                var detailForm = new WorkflowInstanceDetailForm(
                    instanceId,
                    _workflowService,
                    _auditService,
                    _loggingService,
                    _userContext);
                detailForm.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowError($"Error viewing workflow details: {ex.Message}");
            }
        }

        private void WorkflowDashboardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_autoRefreshTimer != null)
            {
                _autoRefreshTimer.Stop();
                _autoRefreshTimer.Dispose();
            }
        }
    }
}
