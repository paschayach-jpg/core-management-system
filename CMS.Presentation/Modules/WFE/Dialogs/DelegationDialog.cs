using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    /// <summary>
    /// Dialog to delegate tasks to other users
    /// </summary>
    public partial class DelegationDialog : Form
    {
        public WorkflowTaskResult Task { get; set; }
        public string DelegateToUser { get; private set; }
        public string DelegationReason { get; private set; }

        public DelegationDialog(WorkflowTaskResult task)
        {
            InitializeComponent();
            Task = task;
        }

        private void DelegationDialog_Load(object sender, EventArgs e)
        {
            InitializeControls();
            DisplayTaskDetails();
        }

        private void InitializeControls()
        {
            PopulateUserDropdown();
        }

        private void DisplayTaskDetails()
        {
            if (Task == null) return;

            lblWorkflowValue.Text = Task.WorkflowName ?? "-";
            lblStepValue.Text = Task.StepName ?? "-";
            lblAssignedDateValue.Text = Task.AssignedDate != DateTime.MinValue
                ? Task.AssignedDate.ToString("dd/MM/yyyy HH:mm")
                : "-";
            lblDueDateValue.Text = Task.SLADeadline != DateTime.MinValue
                ? Task.SLADeadline.ToString("dd/MM/yyyy HH:mm")
                : "-";
        }

        private void PopulateUserDropdown()
        {
            cmbDelegateTo.Items.Clear();
            cmbDelegateTo.Items.Add("User 1 (Manager)");
            cmbDelegateTo.Items.Add("User 2 (Officer)");
            cmbDelegateTo.Items.Add("User 3 (Supervisor)");
            if (cmbDelegateTo.Items.Count > 0)
                cmbDelegateTo.SelectedIndex = 0;
        }

        private void btnDelegate_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                DelegateToUser = GetSelectedUser();
                DelegationReason = GetDelegationReason();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateInput()
        {
            if (cmbDelegateTo.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a user to delegate to", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbDelegateTo.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDelegationReason.Text))
            {
                MessageBox.Show("Please provide a delegation reason", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDelegationReason.Focus();
                return false;
            }

            if (Task?.CurrentDelegationDepth >= Task?.MaxDelegationDepth)
            {
                MessageBox.Show(
                    string.Format("Maximum delegation depth ({0}) reached", Task?.MaxDelegationDepth),
                    "Delegation Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private string GetSelectedUser()
        {
            return cmbDelegateTo.SelectedItem?.ToString() ?? string.Empty;
        }

        private string GetDelegationReason()
        {
            return txtDelegationReason.Text.Trim();
        }
    }
}
