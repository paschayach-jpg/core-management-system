using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Form for displaying access logs and audit trails for external shares.
    /// Satisfies Requirements 9.1, 9.2, 9.3, 9.4
    /// </summary>
    public partial class AccessLogView : Form
    {
        private readonly IShareService _shareService;
        private readonly UserContext _currentUser;
        private int _externalShareId;

        private DataGridView _dgvAccessLog;
        private DateTimePicker _dtpStartDate;
        private DateTimePicker _dtpEndDate;
        private ComboBox _cboAccessType;
        private Dictionary<string, object> _controls;

        public AccessLogView(IShareService shareService, UserContext currentUser, int externalShareId)
        {
            InitializeComponent();
            _shareService = shareService;
            _currentUser = currentUser;
            _externalShareId = externalShareId;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Access Log Viewer";
            this.Width = 900;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterParent;

            // Filtering controls
            var lblStartDate = new Label { Text = "Start Date:", Left = 10, Top = 10, Width = 80 };
            _dtpStartDate = new DateTimePicker { Left = 100, Top = 10, Width = 150, Format = DateTimePickerFormat.Short };
            _dtpStartDate.Value = DateTime.Now.AddDays(-30);

            var lblEndDate = new Label { Text = "End Date:", Left = 270, Top = 10, Width = 80 };
            _dtpEndDate = new DateTimePicker { Left = 360, Top = 10, Width = 150, Format = DateTimePickerFormat.Short };
            _dtpEndDate.Value = DateTime.Now;

            var lblAccessType = new Label { Text = "Access Type:", Left = 530, Top = 10, Width = 80 };
            _cboAccessType = new ComboBox { Left = 620, Top = 10, Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboAccessType.Items.AddRange(new[] { "All", "View", "Download" });
            _cboAccessType.SelectedIndex = 0;

            var btnFilter = new Button { Text = "Filter", Left = 730, Top = 10, Width = 70 };
            btnFilter.Click += (s, e) => LoadAccessLog();

            // DataGridView for access log
            _dgvAccessLog = new DataGridView
            {
                Left = 10,
                Top = 50,
                Width = 870,
                Height = 450,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                ScrollBars = ScrollBars.Both
            };

            _dgvAccessLog.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Recipient", DataPropertyName = "RecipientIdentifier" },
                new DataGridViewTextBoxColumn { HeaderText = "Access Type", DataPropertyName = "AccessType" },
                new DataGridViewTextBoxColumn { HeaderText = "Timestamp", DataPropertyName = "AccessTimestamp", DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } },
                new DataGridViewTextBoxColumn { HeaderText = "Success", DataPropertyName = "IsSuccessful" },
                new DataGridViewTextBoxColumn { HeaderText = "Failure Reason", DataPropertyName = "FailureReason" },
                new DataGridViewTextBoxColumn { HeaderText = "File Size (bytes)", DataPropertyName = "FileSizeBytes" }
            );

            // Close button
            var btnClose = new Button { Text = "Close", Left = 800, Top = 510, Width = 80, DialogResult = DialogResult.OK };

            this.Controls.AddRange(new Control[]
            {
                lblStartDate, _dtpStartDate,
                lblEndDate, _dtpEndDate,
                lblAccessType, _cboAccessType,
                btnFilter,
                _dgvAccessLog,
                btnClose
            });

            _controls = new Dictionary<string, object>
            {
                { "dtpStartDate", _dtpStartDate },
                { "dtpEndDate", _dtpEndDate },
                { "cboAccessType", _cboAccessType }
            };

            this.ResumeLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadAccessLog();
        }

        private void LoadAccessLog()
        {
            try
            {
                var logs = _shareService.GetAccessLog(_externalShareId, _currentUser);

                // Filter by date range
                var startDate = _dtpStartDate.Value.Date;
                var endDate = _dtpEndDate.Value.Date.AddDays(1);
                logs = logs.Where(l => l.AccessTimestamp >= startDate && l.AccessTimestamp < endDate);

                // Filter by access type
                var accessType = _cboAccessType.SelectedItem?.ToString();
                if (accessType != "All")
                {
                    logs = logs.Where(l => l.AccessType == accessType);
                }

                var logData = logs.Select(l => new
                {
                    RecipientIdentifier = l.RecipientIdentifier ?? "Anonymous",
                    AccessType = l.AccessType,
                    AccessTimestamp = l.AccessTimestamp,
                    IsSuccessful = l.IsSuccessful ? "Yes" : "No",
                    FailureReason = l.FailureReason ?? "-",
                    FileSizeBytes = l.FileSizeBytes.HasValue ? l.FileSizeBytes.Value.ToString() : "-"
                }).ToList();

                _dgvAccessLog.DataSource = logData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading access log: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
