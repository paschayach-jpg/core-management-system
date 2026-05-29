using System;
using System.Collections.Generic;
using System.Drawing;
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

            var pnlCriteria = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = System.Drawing.Color.FromArgb(248, 249, 250) };

            var lblTemplate = new Label { Text = "Template:", Location = new System.Drawing.Point(15, 15), AutoSize = true };
            var cmbTemplate = new ComboBox { Name = "cmbTemplate", Location = new System.Drawing.Point(100, 12), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTemplate.Items.Add("(All Templates)");

            var lblStatus = new Label { Text = "Status:", Location = new System.Drawing.Point(280, 15), AutoSize = true };
            var cmbStatus = new ComboBox { Name = "cmbStatus", Location = new System.Drawing.Point(330, 12), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "(All Status)", "Pending", "In Progress", "Completed", "Cancelled" });
            cmbStatus.SelectedIndex = 0;

            var lblInitBy = new Label { Text = "Initiated By:", Location = new System.Drawing.Point(15, 45), AutoSize = true };
            var txtInitBy = new TextBox { Name = "txtInitiatedBy", Location = new System.Drawing.Point(100, 42), Width = 150 };

            var lblAssignTo = new Label { Text = "Assigned To:", Location = new System.Drawing.Point(280, 45), AutoSize = true };
            var txtAssignTo = new TextBox { Name = "txtAssignedTo", Location = new System.Drawing.Point(330, 42), Width = 150 };

            var lblFromDate = new Label { Text = "From:", Location = new System.Drawing.Point(15, 75), AutoSize = true };
            var dtFromDate = new DateTimePicker { Name = "dtFromDate", Location = new System.Drawing.Point(100, 72), Format = DateTimePickerFormat.Short };

            var lblToDate = new Label { Text = "To:", Location = new System.Drawing.Point(280, 75), AutoSize = true };
            var dtToDate = new DateTimePicker { Name = "dtToDate", Location = new System.Drawing.Point(330, 72), Format = DateTimePickerFormat.Short };

            pnlCriteria.Controls.AddRange(new Control[] { lblTemplate, cmbTemplate, lblStatus, cmbStatus, lblInitBy, txtInitBy, lblAssignTo, txtAssignTo, lblFromDate, dtFromDate, lblToDate, dtToDate });
            this.Controls.Add(pnlCriteria);
            this.Controls.SetChildIndex(pnlCriteria, 0);

            var grd = new DataGridView { Name = "grdResults", Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true };
            grd.Columns.Add("InstanceId", "Instance ID");
            grd.Columns.Add("Template", "Template");
            grd.Columns.Add("Status", "Status");
            grd.Columns.Add("InitiatedBy", "Initiated By");
            grd.Columns.Add("InitiatedDate", "Initiated Date");
            grd.Columns[0].Width = 100;
            grd.Columns[1].Width = 150;
            grd.Columns[2].Width = 100;
            grd.Columns[3].Width = 120;
            grd.Columns[4].Width = 150;
            this.Controls.Add(grd);

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
            var cmbTemplate = this.Controls["cmbTemplate"] as ComboBox;
            var cmbStatus = this.Controls["cmbStatus"] as ComboBox;
            var txtInitBy = this.Controls["txtInitiatedBy"] as TextBox;
            var txtAssignTo = this.Controls["txtAssignedTo"] as TextBox;
            var dtFromDate = this.Controls["dtFromDate"] as DateTimePicker;
            var dtToDate = this.Controls["dtToDate"] as DateTimePicker;

            return new WorkflowHistorySearchCriteria
            {
                TemplateId = cmbTemplate?.SelectedIndex > 0 ? cmbTemplate.SelectedIndex : (int?)null,
                Status = cmbStatus?.SelectedIndex > 0 ? (byte?)cmbStatus.SelectedIndex : null,
                InitiatedBy = string.IsNullOrWhiteSpace(txtInitBy?.Text) ? null : txtInitBy.Text,
                AssignedTo = string.IsNullOrWhiteSpace(txtAssignTo?.Text) ? null : txtAssignTo.Text,
                FromDate = dtFromDate?.Checked == true ? dtFromDate.Value : (DateTime?)null,
                ToDate = dtToDate?.Checked == true ? dtToDate.Value : (DateTime?)null,
                PageNumber = 1,
                PageSize = 50
            };
        }

        public void ClearResults()
        {
            var grd = this.Controls["grdResults"] as DataGridView;
            if (grd != null)
                grd.Rows.Clear();

            var txtInitBy = this.Controls["txtInitiatedBy"] as TextBox;
            var txtAssignTo = this.Controls["txtAssignedTo"] as TextBox;
            var cmbTemplate = this.Controls["cmbTemplate"] as ComboBox;
            var cmbStatus = this.Controls["cmbStatus"] as ComboBox;

            if (txtInitBy != null) txtInitBy.Clear();
            if (txtAssignTo != null) txtAssignTo.Clear();
            if (cmbTemplate != null) cmbTemplate.SelectedIndex = 0;
            if (cmbStatus != null) cmbStatus.SelectedIndex = 0;
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
