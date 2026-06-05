using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Form for viewing workflow instance details and execution history
    /// </summary>
    public partial class WorkflowInstanceDetailForm : Form, IWorkflowInstanceDetailView
    {
        private readonly WorkflowInstanceDetailPresenter _presenter;
        private readonly UserContext _userContext;
        private int _workflowInstanceId;

        public WorkflowInstanceDetailForm(
            int workflowInstanceId,
            IWorkflowService workflowService,
            IAuditService auditService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            InitializeComponent();
            _workflowInstanceId = workflowInstanceId;
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));

            _presenter = new WorkflowInstanceDetailPresenter(
                this,
                workflowService,
                auditService,
                loggingService,
                userContext);
        }

        private void WorkflowInstanceDetailForm_Load(object sender, EventArgs e)
        {
            InitializeControls();
            _presenter.LoadInstanceDetails(_workflowInstanceId);
        }

        private void InitializeControls()
        {
            pnlContent.Controls.Clear();
            pnlContent.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);

            var pnlInfo = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = System.Drawing.Color.White };
            var lblInstanceId = new Label { Text = "Instance ID:", Location = new System.Drawing.Point(15, 10), AutoSize = true };
            var lblInstanceIdValue = new Label { Name = "lblInstanceIdValue", Location = new System.Drawing.Point(150, 10), AutoSize = true };
            var lblTemplate = new Label { Text = "Template:", Location = new System.Drawing.Point(15, 35), AutoSize = true };
            var lblTemplateValue = new Label { Name = "lblTemplateValue", Location = new System.Drawing.Point(150, 35), AutoSize = true };
            var lblStatus = new Label { Text = "Status:", Location = new System.Drawing.Point(15, 60), AutoSize = true };
            var lblStatusValue = new Label { Name = "lblStatusValue", Location = new System.Drawing.Point(150, 60), AutoSize = true };
            var lblInitiatedBy = new Label { Text = "Initiated By:", Location = new System.Drawing.Point(15, 85), AutoSize = true };
            var lblInitiatedByValue = new Label { Name = "lblInitiatedByValue", Location = new System.Drawing.Point(150, 85), AutoSize = true };

            pnlInfo.Controls.AddRange(new Control[] { lblInstanceId, lblInstanceIdValue, lblTemplate, lblTemplateValue, lblStatus, lblStatusValue, lblInitiatedBy, lblInitiatedByValue });
            pnlContent.Controls.Add(pnlInfo);

            var grdHistory = new DataGridView
            {
                Name = "grdHistory",
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = true
            };
            grdHistory.Columns.Add("StepName", "Step Name");
            grdHistory.Columns.Add("AssignedTo", "Assigned To");
            grdHistory.Columns.Add("Status", "Status");
            grdHistory.Columns.Add("CompletedDate", "Completed");
            grdHistory.Columns[0].Width = 200;
            grdHistory.Columns[1].Width = 150;
            grdHistory.Columns[2].Width = 100;
            grdHistory.Columns[3].Width = 150;

            pnlContent.Controls.Add(grdHistory);
        }

        public void DisplayInstanceDetails(WorkflowInstanceResult instance)
        {
            if (instance == null) return;

            var lblInstanceIdValue = pnlContent.Controls["lblInstanceIdValue"] as Label;
            var lblTemplateValue = pnlContent.Controls["lblTemplateValue"] as Label;
            var lblStatusValue = pnlContent.Controls["lblStatusValue"] as Label;
            var lblInitiatedByValue = pnlContent.Controls["lblInitiatedByValue"] as Label;

            if (lblInstanceIdValue != null) lblInstanceIdValue.Text = $"WFI-{instance.WorkflowInstanceId:D6}";
            if (lblTemplateValue != null) lblTemplateValue.Text = $"{instance.WorkflowName ?? "-"} v{instance.TemplateVersion}";
            if (lblStatusValue != null) lblStatusValue.Text = instance.Status.ToString();
            if (lblInitiatedByValue != null) lblInitiatedByValue.Text = instance.InitiatedBy ?? "-";

            lblTitle.Text = $"WFI-{instance.WorkflowInstanceId:D6} · {instance.WorkflowName}";
        }

        public void DisplayStepHistory(List<WorkflowStepInstanceResult> history)
        {
            var grd = pnlContent.Controls["grdHistory"] as DataGridView;
            if (grd == null) return;

            grd.Rows.Clear();
            if (history == null || history.Count == 0) return;

            foreach (var step in history)
            {
                int rowIdx = grd.Rows.Add();
                grd.Rows[rowIdx].Cells[0].Value = step.StepName ?? "-";
                grd.Rows[rowIdx].Cells[1].Value = step.AssignedTo ?? "-";
                grd.Rows[rowIdx].Cells[2].Value = step.Status.ToString();
                grd.Rows[rowIdx].Cells[3].Value = step.CompletedDate?.ToString("dd/MM/yyyy HH:mm") ?? "-";
            }
        }

        public void DisplayCurrentStep(WorkflowStepInstanceResult currentStep)
        {
            if (currentStep == null) return;

            MessageBox.Show($"Current Step: {currentStep.StepName}\nAssigned To: {currentStep.AssignedTo}\nStatus: {currentStep.Status}",
                "Current Step Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SetActionButtonsEnabled(bool canSuspend, bool canResume, bool canCancel)
        {
            btnSuspend.Enabled = canSuspend;
            btnResume.Enabled = canResume;
            btnCancel.Enabled = canCancel;
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSuspend_Click(object sender, EventArgs e)
        {
            _presenter.SuspendInstance();
        }

        private void btnResume_Click(object sender, EventArgs e)
        {
            _presenter.ResumeInstance();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _presenter.CancelInstance();
        }
    }
}
