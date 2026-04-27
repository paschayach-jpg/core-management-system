using System;
using System.Collections.Generic;
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
            // TODO: Initialize controls
            // - Instance info labels
            // - Step history DataGridView with columns: StepName, AssignedTo, Status, CompletedDate
            // - Current step details panel
            // - Action buttons: Suspend, Resume, Cancel
        }

        public void DisplayInstanceDetails(WorkflowInstanceResult instance)
        {
            // TODO: Display instance information
            // - ID, Template, Status, InitiatedBy, InitiatedDate
        }

        public void DisplayStepHistory(List<WorkflowStepInstanceResult> history)
        {
            // TODO: Bind step history to DataGridView
        }

        public void DisplayCurrentStep(WorkflowStepInstanceResult currentStep)
        {
            // TODO: Display current step details
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
