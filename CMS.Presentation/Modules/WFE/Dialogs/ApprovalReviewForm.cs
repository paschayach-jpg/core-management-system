using System;
using System.Windows.Forms;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    /// <summary>
    /// Form to review and approve/reject approval tasks
    /// </summary>
    public partial class ApprovalReviewForm : Form
    {
        public WorkflowStepInstanceResult StepInstance { get; set; }
        public bool IsApproved { get; private set; }
        public string RejectionReason { get; private set; }

        public ApprovalReviewForm(WorkflowStepInstanceResult stepInstance)
        {
            InitializeComponent();
            StepInstance = stepInstance;
        }

        private void ApprovalReviewForm_Load(object sender, EventArgs e)
        {
            InitializeControls();
            DisplayApprovalDetails();
        }

        private void InitializeControls()
        {
            txtRejectionReason.Visible = false;
            lblRejectionReason.Visible = false;
        }

        private void DisplayApprovalDetails()
        {
            if (StepInstance == null) return;

            lblStepNameValue.Text = StepInstance.StepName ?? "-";
            lblAssignedToValue.Text = StepInstance.AssignedTo ?? "-";
            lblAssignedDateValue.Text = StepInstance.AssignedDate != DateTime.MinValue
                ? StepInstance.AssignedDate.ToString("dd/MM/yyyy HH:mm")
                : "-";
            lblSLADeadlineValue.Text = StepInstance.SLADeadline != DateTime.MinValue
                ? StepInstance.SLADeadline.ToString("dd/MM/yyyy HH:mm")
                : "-";
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {
            IsApproved = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            txtRejectionReason.Visible = true;
            lblRejectionReason.Visible = true;

            if (ValidateRejectionReason())
            {
                IsApproved = false;
                RejectionReason = GetRejectionReason();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateRejectionReason()
        {
            if (string.IsNullOrWhiteSpace(txtRejectionReason.Text))
            {
                MessageBox.Show("Please provide a rejection reason", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRejectionReason.Focus();
                return false;
            }
            return true;
        }

        private string GetRejectionReason()
        {
            return txtRejectionReason.Text.Trim();
        }
    }
}
