using System;
using System.Collections.Generic;
using System.IO;
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
            this.Text = "Workflow History Search";
            _presenter?.LoadTemplates();
        }

        public void DisplayResults(PagedResult<WorkflowInstanceResult> results)
        {
            if (results?.Items == null) return;

            var grid = this.Controls.OfType<DataGridView>().FirstOrDefault();
            if (grid == null) return;

            grid.DataSource = results.Items;
            grid.AutoResizeColumns();
            this.ShowInfo(string.Format("Found {0} records", results.TotalCount));
        }

        public void ShowDetailView(WorkflowInstanceResult instance)
        {
            if (instance == null) return;

            var detailForm = new WorkflowDetailFormNew(
                instance.WorkflowInstanceId,
                _presenter?.GetWorkflowService(),
                _presenter?.GetTemplateService(),
                _presenter?.GetLoggingService(),
                _userContext);

            detailForm.ShowDialog(this);
        }

        public void ExportResults(string filePath)
        {
            try
            {
                var grid = this.Controls.OfType<DataGridView>().FirstOrDefault();
                if (grid == null || grid.Rows.Count == 0)
                {
                    MessageBox.Show("No data to export", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var writer = new System.IO.StreamWriter(filePath))
                {
                    var headers = string.Join(",", grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText));
                    writer.WriteLine(headers);

                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        var values = string.Join(",", row.Cells.Cast<DataGridViewCell>().Select(c => c.Value?.ToString() ?? ""));
                        writer.WriteLine(values);
                    }
                }
                MessageBox.Show("Exported successfully", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError("Export failed: " + ex.Message);
            }
        }

        public WorkflowHistorySearchCriteria GetSearchCriteria()
        {
            return new WorkflowHistorySearchCriteria
            {
                TemplateId = null,
                Status = null,
                FromDate = null,
                ToDate = null,
                InitiatedBy = null,
                AssignedTo = null,
                PageNumber = 1,
                PageSize = 50
            };
        }

        public void ClearResults()
        {
            var grid = this.Controls.OfType<DataGridView>().FirstOrDefault();
            if (grid != null)
                grid.DataSource = null;
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
