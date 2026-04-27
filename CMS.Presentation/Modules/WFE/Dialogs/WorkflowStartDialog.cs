using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    /// <summary>
    /// Dialog to initiate new workflow instances
    /// </summary>
    public partial class WorkflowStartDialog : Form
    {
        public int SelectedTemplateId { get; private set; }
        public string InputParameters { get; private set; }

        public WorkflowStartDialog()
        {
            InitializeComponent();
        }

        private void WorkflowStartDialog_Load(object sender, EventArgs e)
        {
            InitializeControls();
        }

        private void InitializeControls()
        {
            // TODO: Initialize controls
            // - Template selection dropdown
            // - Input parameters form (dynamic based on template)
            // - Validation and submit
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
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
            // TODO: Implement validation
            return true;
        }
    }
}
