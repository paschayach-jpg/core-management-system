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
    /// Form for searching and viewing workflow history
    /// </summary>
    public partial class HistorySearchForm : Form, IHistorySearchView
    {
        private readonly HistorySearchPresenter _presenter;
        private readonly UserContext _userContext;

        public HistorySearchForm(
            IWorkflowService workflowService,
            IAuditService auditService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            InitializeComponent();
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));

            _presenter = new HistorySearchPresenter(
                this,
                workflowService,
                auditService,
                loggingService,
                userContext);
        }

        private void HistorySearchForm_Load(object sender, EventArgs e)
        {
            InitializeControls();
        }

        private void InitializeControls()
        {
            // TODO: Initialize controls
            // - Search criteria panel
            //   - Template dropdown
            //   - Status dropdown
            //   - Date range picker
            //   - InitiatedBy textbox
            //   - AssignedTo textbox
            // - Results DataGridView with columns: InstanceID, Template, Status, InitiatedBy, InitiatedDate
            // - Detail view panel
            // - Export button
        }

        public void DisplayResults(PagedResult<WorkflowInstanceResult> results)
        {
            // TODO: Bind results to DataGridView
        }

        public void ShowDetailView(WorkflowInstanceResult instance)
        {
            // TODO: Show detail view for selected instance
        }

        public void ExportResults(string filePath)
        {
            // TODO: Implement export to file
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public WorkflowHistorySearchCriteria GetSearchCriteria()
        {
            // TODO: Get search criteria from controls
            return new WorkflowHistorySearchCriteria
            {
                PageNumber = 1,
                PageSize = 50
            };
        }

        public void ClearResults()
        {
            // TODO: Clear results from DataGridView
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            _presenter.SearchHistory();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx",
                DefaultExt = "csv"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                _presenter.ExportResults(saveDialog.FileName);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _presenter.ClearSearch();
        }
    }
}
