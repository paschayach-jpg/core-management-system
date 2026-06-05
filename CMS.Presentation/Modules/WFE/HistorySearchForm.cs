using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CMS.Data.Models;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Presenters;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    /// <summary>
    /// Form for searching and viewing workflow history.
    /// </summary>
    public partial class HistorySearchForm : Form, IHistorySearchView
    {
        private readonly HistorySearchPresenter _presenter;
        private readonly IWorkflowService _workflowService;
        private readonly IWorkflowTemplateService _templateService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;

        private ComboBox _cmbTemplate;
        private ComboBox _cmbStatus;
        private TextBox _txtInitiatedBy;
        private TextBox _txtAssignedTo;
        private DateTimePicker _dtFromDate;
        private DateTimePicker _dtToDate;
        private DataGridView _grdResults;
        private Label _lblResultCount;
        private Button _btnDetail;
        private Button _btnClose;
        private Panel _activeNavItem;
        private PagedResult<WorkflowInstanceResult> _lastResults;

        private readonly Color _navyDark = Color.FromArgb(31, 56, 100);
        private readonly Color _surface = Color.FromArgb(245, 248, 252);
        private readonly Color _line = Color.FromArgb(218, 225, 235);
        private readonly Color _text = Color.FromArgb(35, 45, 65);
        private readonly Color _mutedText = Color.FromArgb(95, 107, 130);

        public HistorySearchForm(
            IWorkflowService workflowService,
            IAuditService auditService,
            ILoggingService loggingService,
            UserContext userContext,
            IWorkflowTemplateService templateService = null)
        {
            InitializeComponent();

            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _templateService = templateService;
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));

            _presenter = new HistorySearchPresenter(
                this,
                workflowService,
                auditService,
                loggingService,
                userContext,
                templateService);
        }

        private void HistorySearchForm_Load(object sender, EventArgs e)
        {
            InitializeControls();
            LoadTemplates();
            _presenter.SearchHistory();
        }

        private void InitializeControls()
        {
            Text = "Workflow History";
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.FromArgb(250, 252, 255);
            Controls.Clear();

            var ribbonTabs = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = _navyDark,
                Padding = new Padding(4, 0, 4, 0)
            };
            BuildRibbonTabs(ribbonTabs);

            var ribbonBorder = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(200, 215, 240)
            };

            var toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = Color.White,
                Padding = new Padding(6, 0, 6, 0)
            };
            BuildToolbar(toolbarPanel);

            var appBody = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(243, 246, 251)
            };

            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 202,
                BackColor = Color.White,
                AutoScroll = true,
                Padding = new Padding(0, 8, 0, 8)
            };

            var navBorder = new Panel
            {
                Dock = DockStyle.Left,
                Width = 1,
                BackColor = Color.FromArgb(225, 228, 235)
            };

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 252, 255)
            };

            WorkflowNavigationBuilder.BuildSidebar(sidebar, "history", OnNavClick, out _activeNavItem);
            appBody.Controls.Add(contentPanel);
            appBody.Controls.Add(navBorder);
            appBody.Controls.Add(sidebar);

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.White,
                Padding = new Padding(22, 10, 22, 8)
            };
            headerPanel.Controls.Add(new Label
            {
                Text = "CMS › Workflow Engine › History",
                AutoSize = true,
                ForeColor = Color.FromArgb(75, 92, 125),
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(22, 10)
            });
            headerPanel.Controls.Add(new Label
            {
                Text = "ประวัติ Workflow",
                AutoSize = true,
                ForeColor = Color.FromArgb(10, 25, 55),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                Location = new Point(22, 30)
            });

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 118,
                BackColor = _surface,
                Padding = new Padding(22, 12, 22, 12)
            };
            topPanel.Controls.Add(new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = _line
            });

            btnSearch.Text = "ค้นหา";
            btnSearch.Location = new Point(22, 14);
            btnSearch.Size = new Size(86, 30);
            btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            StyleCommandButton(btnSearch, true);

            btnExport.Text = "ส่งออก";
            btnExport.Location = new Point(116, 14);
            btnExport.Size = new Size(86, 30);
            btnExport.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            StyleCommandButton(btnExport, false);

            btnClear.Text = "ล้าง";
            btnClear.Location = new Point(210, 14);
            btnClear.Size = new Size(76, 30);
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            StyleCommandButton(btnClear, false);

            _btnDetail = new Button
            {
                Text = "รายละเอียด",
                Location = new Point(294, 14),
                Size = new Size(98, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            StyleCommandButton(_btnDetail, false);
            _btnDetail.Click += (sender, args) => OpenSelectedDetail();

            _btnClose = new Button
            {
                Text = "ปิด",
                Location = new Point(400, 14),
                Size = new Size(76, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            StyleCommandButton(_btnClose, false);
            _btnClose.Click += (sender, args) => Close();

            _lblResultCount = new Label
            {
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(540, 14),
                Size = new Size(320, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                ForeColor = _mutedText,
                Font = new Font("Segoe UI", 9F),
                Text = "พร้อมค้นหา"
            };

            var lblTemplate = CreateLabel("Template", 22, 64);
            _cmbTemplate = new ComboBox
            {
                Location = new Point(92, 60),
                Width = 198,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblStatus = CreateLabel("Status", 310, 64);
            _cmbStatus = new ComboBox
            {
                Location = new Point(368, 60),
                Width = 152,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbStatus.Items.AddRange(new object[]
            {
                new StatusOption(null, "(All Status)"),
                new StatusOption(WorkflowInstanceStatus.Created, "Created"),
                new StatusOption(WorkflowInstanceStatus.InProgress, "In Progress"),
                new StatusOption(WorkflowInstanceStatus.Completed, "Completed"),
                new StatusOption(WorkflowInstanceStatus.Rejected, "Rejected"),
                new StatusOption(WorkflowInstanceStatus.Suspended, "Suspended")
            });
            _cmbStatus.SelectedIndex = 0;

            var lblInitBy = CreateLabel("Initiated By", 22, 94);
            _txtInitiatedBy = new TextBox
            {
                Location = new Point(92, 90),
                Width = 198
            };

            var lblAssignTo = CreateLabel("Assigned To", 310, 94);
            _txtAssignedTo = new TextBox
            {
                Location = new Point(392, 90),
                Width = 168
            };

            var lblFromDate = CreateLabel("From", 584, 64);
            _dtFromDate = new DateTimePicker
            {
                Location = new Point(636, 60),
                Width = 154,
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };

            var lblToDate = CreateLabel("To", 584, 94);
            _dtToDate = new DateTimePicker
            {
                Location = new Point(636, 90),
                Width = 154,
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };
            StyleInput(_cmbTemplate);
            StyleInput(_cmbStatus);
            StyleInput(_txtInitiatedBy);
            StyleInput(_txtAssignedTo);
            StyleInput(_dtFromDate);
            StyleInput(_dtToDate);

            topPanel.Controls.AddRange(new Control[]
            {
                btnSearch, btnExport, btnClear, _btnDetail, _btnClose, _lblResultCount,
                lblTemplate, _cmbTemplate, lblStatus, _cmbStatus,
                lblInitBy, _txtInitiatedBy, lblAssignTo, _txtAssignedTo,
                lblFromDate, _dtFromDate, lblToDate, _dtToDate
            });

            _grdResults = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(224, 230, 240),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 34,
                RowTemplate = { Height = 30 }
            };
            _grdResults.ColumnHeadersDefaultCellStyle.BackColor = _navyDark;
            _grdResults.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grdResults.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _grdResults.ColumnHeadersDefaultCellStyle.SelectionBackColor = _navyDark;
            _grdResults.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            _grdResults.DefaultCellStyle.BackColor = Color.White;
            _grdResults.DefaultCellStyle.ForeColor = _text;
            _grdResults.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 233, 255);
            _grdResults.DefaultCellStyle.SelectionForeColor = Color.FromArgb(10, 40, 90);
            _grdResults.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            _grdResults.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 251, 254);
            _grdResults.Columns.Add(CreateColumn("InstanceId", "รหัส", 90, DataGridViewAutoSizeColumnMode.None));
            _grdResults.Columns.Add(CreateColumn("WorkflowName", "Workflow", 220, DataGridViewAutoSizeColumnMode.Fill));
            _grdResults.Columns.Add(CreateColumn("Status", "สถานะ", 110, DataGridViewAutoSizeColumnMode.None));
            _grdResults.Columns.Add(CreateColumn("InitiatedBy", "ผู้เริ่ม", 140, DataGridViewAutoSizeColumnMode.None));
            _grdResults.Columns.Add(CreateColumn("InitiatedDate", "วันที่เริ่ม", 140, DataGridViewAutoSizeColumnMode.None));
            _grdResults.Columns.Add(CreateColumn("CurrentStep", "Step ปัจจุบัน", 170, DataGridViewAutoSizeColumnMode.None));
            _grdResults.Columns.Add(CreateColumn("Progress", "ความคืบหน้า", 110, DataGridViewAutoSizeColumnMode.None));
            _grdResults.CellDoubleClick += (sender, args) => OpenSelectedDetail();
            _grdResults.KeyDown += (sender, args) =>
            {
                if (args.KeyCode == Keys.Enter)
                {
                    args.Handled = true;
                    OpenSelectedDetail();
                }
            };

            contentPanel.Controls.Add(_grdResults);
            contentPanel.Controls.Add(topPanel);
            contentPanel.Controls.Add(headerPanel);

            Controls.Add(appBody);
            Controls.Add(toolbarPanel);
            Controls.Add(ribbonBorder);
            Controls.Add(ribbonTabs);
        }

        private void BuildRibbonTabs(Panel ribbonTabs)
        {
            var tabs = new[] { "แฟ้ม", "หน้าแรก", "ดำเนินการ", "มุมมอง", "เครื่องมือ", "ช่วยเหลือ" };
            var x = 8;
            foreach (var text in tabs)
            {
                var active = text == "หน้าแรก";
                var button = new Button
                {
                    Text = text,
                    Location = new Point(x, 0),
                    Size = new Size(MeasureTextWidth(text, new Font("Segoe UI", 9.5F)) + 24, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = active ? Color.White : Color.FromArgb(190, 210, 245),
                    Font = active ? new Font("Segoe UI", 9.5F, FontStyle.Bold) : new Font("Segoe UI", 9.5F),
                    Cursor = Cursors.Hand,
                    TabStop = false
                };
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 80, 130);
                if (active)
                {
                    button.Paint += (sender, args) =>
                        args.Graphics.FillRectangle(Brushes.White, 0, button.Height - 3, button.Width, 3);
                }
                ribbonTabs.Controls.Add(button);
                x += button.Width + 2;
            }
        }

        private void BuildToolbar(Panel toolbarPanel)
        {
            var groups = new[]
            {
                new { Header = "คำสั่ง", Items = new[]
                {
                    new { Icon = "🔎", Text = "ค้นหา", Tag = "search" },
                    new { Icon = "👁", Text = "รายละเอียด", Tag = "detail" },
                    new { Icon = "↻", Text = "ล้าง", Tag = "clear" },
                }},
                new { Header = "ส่งออก", Items = new[]
                {
                    new { Icon = "▦", Text = "CSV", Tag = "export" },
                    new { Icon = "✕", Text = "ปิด", Tag = "close" },
                }},
            };

            var x = 8;
            for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                var group = groups[groupIndex];
                var groupStartX = x;

                foreach (var item in group.Items)
                {
                    var buttonPanel = new Panel
                    {
                        Location = new Point(x, 2),
                        Size = new Size(64, 60),
                        BackColor = Color.White,
                        Cursor = Cursors.Hand,
                        Tag = item.Tag
                    };
                    var icon = new Label
                    {
                        Text = item.Icon,
                        Font = new Font("Segoe UI", 16F),
                        ForeColor = _navyDark,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(0, 6),
                        Size = new Size(64, 30)
                    };
                    var label = new Label
                    {
                        Text = item.Text,
                        Font = new Font("Segoe UI", 7.5F),
                        ForeColor = Color.FromArgb(55, 65, 90),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(0, 36),
                        Size = new Size(64, 20)
                    };

                    buttonPanel.Controls.Add(icon);
                    buttonPanel.Controls.Add(label);

                    EventHandler enter = (sender, args) =>
                    {
                        buttonPanel.BackColor = icon.BackColor = label.BackColor = Color.FromArgb(245, 249, 255);
                    };
                    EventHandler leave = (sender, args) =>
                    {
                        buttonPanel.BackColor = icon.BackColor = label.BackColor = Color.White;
                    };
                    EventHandler click = (sender, args) => OnToolbarClick((string)buttonPanel.Tag);

                    buttonPanel.MouseEnter += enter;
                    icon.MouseEnter += enter;
                    label.MouseEnter += enter;
                    buttonPanel.MouseLeave += leave;
                    icon.MouseLeave += leave;
                    label.MouseLeave += leave;
                    buttonPanel.Click += click;
                    icon.Click += click;
                    label.Click += click;

                    toolbarPanel.Controls.Add(buttonPanel);
                    x += 66;
                }

                toolbarPanel.Controls.Add(new Label
                {
                    Text = group.Header,
                    Font = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(120, 130, 150),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(groupStartX, 62),
                    Size = new Size(x - groupStartX, 12)
                });

                if (groupIndex < groups.Length - 1)
                {
                    toolbarPanel.Controls.Add(new Panel
                    {
                        BackColor = Color.FromArgb(220, 225, 235),
                        Location = new Point(x + 2, 8),
                        Size = new Size(1, 54)
                    });
                    x += 12;
                }
            }

            toolbarPanel.Controls.Add(new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = _line
            });
        }

        private int MeasureTextWidth(string text, Font font)
        {
            using (var graphics = CreateGraphics())
            {
                return (int)graphics.MeasureString(text, font).Width;
            }
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = _mutedText,
                Font = new Font("Segoe UI", 8.5F)
            };
        }

        private void StyleCommandButton(Button button, bool primary)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.BackColor = primary ? _navyDark : Color.White;
            button.ForeColor = primary ? Color.White : _navyDark;
            button.FlatAppearance.BorderColor = primary ? _navyDark : Color.FromArgb(185, 198, 218);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(42, 74, 128) : Color.FromArgb(236, 243, 255);
            button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(24, 45, 82) : Color.FromArgb(224, 235, 252);
        }

        private void StyleInput(Control control)
        {
            control.Font = new Font("Segoe UI", 9F);
            control.ForeColor = _text;
            control.BackColor = Color.White;
        }

        private static DataGridViewTextBoxColumn CreateColumn(
            string name,
            string headerText,
            int width,
            DataGridViewAutoSizeColumnMode autoSizeMode)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = headerText,
                Width = width,
                AutoSizeMode = autoSizeMode,
                SortMode = DataGridViewColumnSortMode.Automatic
            };
        }

        private void LoadTemplates()
        {
            _cmbTemplate.Items.Clear();
            _cmbTemplate.Items.Add(new TemplateOption(null, "(All Templates)"));
            _cmbTemplate.SelectedIndex = 0;

            if (_templateService == null)
            {
                _cmbTemplate.Enabled = false;
                return;
            }

            try
            {
                var templates = _templateService.GetTemplates(
                    new WorkflowTemplateFilter
                    {
                        PageNumber = 1,
                        PageSize = 500,
                        SortBy = "Name",
                        SortDirection = "ASC"
                    },
                    _userContext);

                foreach (var template in templates.Items ?? Enumerable.Empty<WorkflowTemplateResult>())
                {
                    _cmbTemplate.Items.Add(new TemplateOption(template.WorkflowDefinitionId, template.Name));
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error("Failed to load workflow templates for history search: " + ex.Message, ex);
                _cmbTemplate.Enabled = false;
            }
        }

        public void DisplayResults(PagedResult<WorkflowInstanceResult> results)
        {
            _lastResults = results ?? new PagedResult<WorkflowInstanceResult>
            {
                Items = Enumerable.Empty<WorkflowInstanceResult>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 50
            };

            _grdResults.Rows.Clear();

            foreach (var item in _lastResults.Items ?? Enumerable.Empty<WorkflowInstanceResult>())
            {
                var rowIndex = _grdResults.Rows.Add(
                    item.WorkflowInstanceId,
                    string.IsNullOrWhiteSpace(item.WorkflowName) ? item.TemplateName : item.WorkflowName,
                    FormatStatus(item.Status),
                    item.InitiatedBy,
                    item.InitiatedDate == DateTime.MinValue ? string.Empty : item.InitiatedDate.ToString("dd/MM/yyyy HH:mm"),
                    item.CurrentStepName,
                    item.ProgressPercentage + "%");

                _grdResults.Rows[rowIndex].Tag = item;
            }

            _lblResultCount.Text = string.Format("พบ {0:N0} รายการ", _lastResults.TotalCount);
        }

        public void ShowDetailView(WorkflowInstanceResult instance)
        {
            if (instance == null) return;

            WorkflowNavigationHandler.HandleNavigation(
                "detail",
                this,
                _workflowService,
                _templateService,
                null,
                _loggingService,
                _userContext,
                _auditService,
                instance.WorkflowInstanceId);
        }

        public void ExportResults(string filePath)
        {
            if (_grdResults == null || _grdResults.Rows.Count == 0)
            {
                throw new InvalidOperationException("No data to export.");
            }

            using (var writer = new StreamWriter(filePath, false, new UTF8Encoding(true)))
            {
                writer.WriteLine(string.Join(",", _grdResults.Columns
                    .Cast<DataGridViewColumn>()
                    .Select(column => Csv(column.HeaderText))));

                foreach (DataGridViewRow row in _grdResults.Rows)
                {
                    writer.WriteLine(string.Join(",", row.Cells
                        .Cast<DataGridViewCell>()
                        .Select(cell => Csv(cell.Value == null ? string.Empty : cell.Value.ToString()))));
                }
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

        public WorkflowHistorySearchCriteria GetSearchCriteria()
        {
            var template = _cmbTemplate.SelectedItem as TemplateOption;
            var status = _cmbStatus.SelectedItem as StatusOption;

            return new WorkflowHistorySearchCriteria
            {
                TemplateId = template == null ? null : template.TemplateId,
                Status = status != null && status.Status.HasValue ? (byte?)status.Status.Value : null,
                InitiatedBy = string.IsNullOrWhiteSpace(_txtInitiatedBy.Text) ? null : _txtInitiatedBy.Text.Trim(),
                AssignedTo = string.IsNullOrWhiteSpace(_txtAssignedTo.Text) ? null : _txtAssignedTo.Text.Trim(),
                FromDate = _dtFromDate.Checked ? _dtFromDate.Value.Date : (DateTime?)null,
                ToDate = _dtToDate.Checked ? _dtToDate.Value.Date.AddDays(1).AddTicks(-1) : (DateTime?)null,
                PageNumber = 1,
                PageSize = 50
            };
        }

        public void ClearResults()
        {
            _txtInitiatedBy.Clear();
            _txtAssignedTo.Clear();
            _dtFromDate.Checked = false;
            _dtToDate.Checked = false;

            if (_cmbTemplate.Items.Count > 0) _cmbTemplate.SelectedIndex = 0;
            if (_cmbStatus.Items.Count > 0) _cmbStatus.SelectedIndex = 0;

            _lastResults = null;
            _grdResults.Rows.Clear();
            _lblResultCount.Text = "พร้อมค้นหา";
        }

        private void OpenSelectedDetail()
        {
            if (_grdResults.CurrentRow == null)
            {
                ShowInfo("Please select a workflow instance first.");
                return;
            }

            _presenter.ViewInstanceDetails(_grdResults.CurrentRow.Tag as WorkflowInstanceResult);
        }

        private void OnToolbarClick(string tag)
        {
            switch (tag)
            {
                case "search":
                    _presenter.SearchHistory();
                    break;
                case "detail":
                    OpenSelectedDetail();
                    break;
                case "clear":
                    _presenter.ClearSearch();
                    break;
                case "export":
                    btnExport_Click(this, EventArgs.Empty);
                    break;
                case "close":
                    Close();
                    break;
            }
        }

        private void OnNavClick(string tag)
        {
            if (tag == "history")
                return;

            WorkflowNavigationHandler.HandleNavigation(
                tag,
                this,
                _workflowService,
                _templateService,
                null,
                _loggingService,
                _userContext,
                _auditService);
        }

        private static string Csv(string value)
        {
            value = value ?? string.Empty;
            if (value.Contains("\"") || value.Contains(",") || value.Contains("\r") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }

        private static string FormatStatus(WorkflowInstanceStatus status)
        {
            return Enum.IsDefined(typeof(WorkflowInstanceStatus), status)
                ? status.ToString()
                : "Unknown";
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            _presenter.SearchHistory();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = "workflow-history.csv"
            };

            if (saveDialog.ShowDialog(this) == DialogResult.OK)
            {
                _presenter.ExportResults(saveDialog.FileName);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _presenter.ClearSearch();
        }

        private sealed class TemplateOption
        {
            public TemplateOption(int? templateId, string name)
            {
                TemplateId = templateId;
                Name = name;
            }

            public int? TemplateId { get; private set; }
            public string Name { get; private set; }
            public override string ToString() => Name;
        }

        private sealed class StatusOption
        {
            public StatusOption(WorkflowInstanceStatus? status, string text)
            {
                Status = status;
                Text = text;
            }

            public WorkflowInstanceStatus? Status { get; private set; }
            public string Text { get; private set; }
            public override string ToString() => Text;
        }
    }
}
