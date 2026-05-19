using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    /// Form for managing workflow templates — redesigned with ribbon toolbar,
    /// left sidebar navigation, and modern grid with colored badges.
    /// </summary>
    public partial class WorkflowTemplateListForm : Form, IWorkflowTemplateListView
    {
        private readonly WorkflowTemplateListPresenter _presenter;
        private readonly UserContext _userContext;
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService _loggingService;
        private readonly List<WorkflowTemplateResult> _currentTemplates = new List<WorkflowTemplateResult>();

        private bool _searchPlaceholderActive = true;
        private int _totalCount = 0;
        private Label _activeTabLabel;

        // ── Ribbon toolbar panel references (for enable/disable) ─────────────
        private Panel _pnlRibbonBtnDuplicate;
        private Panel _pnlRibbonBtnEdit;
        private Panel _pnlRibbonBtnDelete;
        private Panel _pnlRibbonBtnPublish;
        private Panel _pnlRibbonBtnDeactivate;
        private Panel _pnlRibbonBtnVersions;

        // ── Category colors map ──────────────────────────────────────────────
        private static readonly Dictionary<string, Color[]> _categoryColors =
            new Dictionary<string, Color[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "เอกสาร",    new[] { Color.FromArgb(219, 234, 254), Color.FromArgb(37, 99, 235)  } },
                { "IT",        new[] { Color.FromArgb(209, 213, 219), Color.FromArgb(55, 65, 81)   } },
                { "Knowledge", new[] { Color.FromArgb(204, 251, 241), Color.FromArgb(15, 118, 110) } },
                { "HR",        new[] { Color.FromArgb(254, 215, 170), Color.FromArgb(154, 52, 18)  } },
            };

        // ── Status colors: back, fore ────────────────────────────────────────
        private static readonly Dictionary<string, Color[]> _statusColors =
            new Dictionary<string, Color[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "Active",     new[] { Color.FromArgb(220, 252, 231), Color.FromArgb(21, 128, 61)  } },
                { "Inactive",   new[] { Color.FromArgb(229, 231, 235), Color.FromArgb(75, 85, 99)   } },
                { "Draft",      new[] { Color.FromArgb(254, 243, 199), Color.FromArgb(146, 64, 14)  } },
                { "Deprecated", new[] { Color.FromArgb(254, 226, 226), Color.FromArgb(185, 28, 28)  } },
            };

        // ── Owner avatar palette ─────────────────────────────────────────────
        private static readonly Color[] _avatarPalette = new[]
        {
            Color.FromArgb(99,  102, 241),
            Color.FromArgb(236, 72,  153),
            Color.FromArgb(34,  197, 94),
            Color.FromArgb(249, 115, 22),
            Color.FromArgb(20,  184, 166),
            Color.FromArgb(59,  130, 246),
        };

        public WorkflowTemplateListForm(
            IWorkflowTemplateService templateService,
            IAuditService auditService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            InitializeComponent();
            _userContext = userContext;
            _templateService = templateService;
            _loggingService = loggingService;

            // Only create presenter if services are available
            if (templateService != null && loggingService != null && userContext != null)
            {
                _presenter = new WorkflowTemplateListPresenter(
                    this, templateService, auditService, loggingService, userContext);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Load
        // ════════════════════════════════════════════════════════════════════
        private void WorkflowTemplateListForm_Load(object sender, EventArgs e)
        {
            InitializeControls();

            if (_presenter != null)
            {
                _presenter.LoadTemplates();
            }
            else
            {
                LoadMockData();
            }
        }

        private void LoadMockData()
        {
            var now = DateTime.Now;
            var mock = new System.Collections.Generic.List<WorkflowTemplateResult>
            {
                new WorkflowTemplateResult { WorkflowDefinitionId=1,  Name="อนุมัติ KB Article",          Category="Knowledge", StepCount=3, Version=2, IsActive=true,  OwnerDepartment="IT",  CreatedBy="admin",    ModifiedDate=now.AddDays(-5),  ActiveInstanceCount=12 },
                new WorkflowTemplateResult { WorkflowDefinitionId=2,  Name="ขออนุมัติ Upload ไฟล์",       Category="เอกสาร",    StepCount=2, Version=1, IsActive=true,  OwnerDepartment="HR",  CreatedBy="admin",    ModifiedDate=now.AddDays(-10), ActiveInstanceCount=8  },
                new WorkflowTemplateResult { WorkflowDefinitionId=3,  Name="อนุมัติเปลี่ยน Permission",   Category="IT",        StepCount=3, Version=1, IsActive=false, Status="Draft",    OwnerDepartment="IT",  CreatedBy="admin",    ModifiedDate=now.AddDays(-2),  ActiveInstanceCount=0  },
                new WorkflowTemplateResult { WorkflowDefinitionId=4,  Name="Review SOP Document",         Category="เอกสาร",    StepCount=2, Version=3, IsActive=true,  OwnerDepartment="QA",  CreatedBy="admin",    ModifiedDate=now.AddDays(-15), ActiveInstanceCount=5  },
                new WorkflowTemplateResult { WorkflowDefinitionId=5,  Name="Work Instruction Approval",   Category="เอกสาร",    StepCount=4, Version=1, IsActive=false, OwnerDepartment="Ops", CreatedBy="admin",    ModifiedDate=now.AddDays(-30), ActiveInstanceCount=0  },
                new WorkflowTemplateResult { WorkflowDefinitionId=6,  Name="อนุมัติแก้ไข Template",      Category="Knowledge", StepCount=2, Version=1, IsActive=false, Status="Draft",    OwnerDepartment="IT",  CreatedBy="admin",    ModifiedDate=now.AddDays(-1),  ActiveInstanceCount=0  },
                new WorkflowTemplateResult { WorkflowDefinitionId=7,  Name="HR Onboarding Process",       Category="HR",        StepCount=5, Version=2, IsActive=true,  OwnerDepartment="HR",  CreatedBy="hr.admin", ModifiedDate=now.AddDays(-7),  ActiveInstanceCount=3  },
            };

            DisplayTemplates(new PagedResult<WorkflowTemplateResult>
            {
                Items = mock,
                TotalCount = mock.Count,
                PageNumber = 1,
                PageSize = 50,
            });
        }

        private void InitializeControls()
        {
            // Build sidebar navigation
            WorkflowNavigationBuilder.BuildSidebar(
                pnlSidebar, "template", OnNavItemClick, out _);

            // Category filter
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("ทั้งหมด");
            cmbCategory.Items.Add("เอกสาร");
            cmbCategory.Items.Add("IT");
            cmbCategory.Items.Add("Knowledge");
            cmbCategory.Items.Add("HR");
            cmbCategory.SelectedIndex = 0;

            // Status filter
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("ทั้งหมด");
            cmbStatus.Items.Add("Active");
            cmbStatus.Items.Add("Draft");
            cmbStatus.Items.Add("Inactive");
            cmbStatus.Items.Add("Deprecated");
            cmbStatus.SelectedIndex = 0;

            // Active ribbon tab
            _activeTabLabel = lblTabTools;
            _activeTabLabel.ForeColor = Color.FromArgb(37, 99, 235);

            BuildRibbonToolbar();

            dgvTemplates.AutoGenerateColumns = false;
            dgvTemplates.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTemplates.MultiSelect = false;
            dgvTemplates.ReadOnly = true;

            SetActionButtonsEnabled(false, false, false);
            UpdateResultCountLabel();
        }

        // ════════════════════════════════════════════════════════════════════
        // IWorkflowTemplateListView implementation
        // ════════════════════════════════════════════════════════════════════
        public void DisplayTemplates(PagedResult<WorkflowTemplateResult> templates)
        {
            var items = templates?.Items?.ToList() ?? new List<WorkflowTemplateResult>();
            _currentTemplates.Clear();
            _currentTemplates.AddRange(items);
            _totalCount = templates?.TotalCount > 0 ? templates.TotalCount : items.Count;

            var rows = items.Select(t => new WorkflowTemplateGridRow
            {
                WorkflowDefinitionId = t.WorkflowDefinitionId,
                DisplayId = string.Format("WFD-{0:D3}", t.WorkflowDefinitionId),
                Name = t.Name,
                Category = string.IsNullOrWhiteSpace(t.Category) ? "" : t.Category,
                StepCount = t.StepCount,
                Version = "v" + t.Version,
                StatusLabel = t.StatusLabel,
                OwnerDepartment = string.IsNullOrWhiteSpace(t.OwnerDepartment)
                                        ? ExtractDeptFromADUser(t.CreatedBy)
                                        : t.OwnerDepartment,
                OwnerInitial = GetInitial(t.OwnerDepartment ?? t.CreatedBy),
                ModifiedDate = t.ModifiedDate.ToString("dd/MM/yy"),
                ActiveInstanceCount = t.ActiveInstanceCount
            }).ToList();

            dgvTemplates.DataSource = null;
            dgvTemplates.DataSource = rows;

            UpdateResultCountLabel();
            lblSelectionInfo.Text = string.Format("เลือก 0 รายการจาก {0}", _totalCount);
            SetActionButtonsEnabled(false, false, false);
        }

        public void ShowTemplateForm(WorkflowTemplateResult template = null)
        {
            try
            {
                // Create new template: show wizard
                if (template == null)
                {
                    ShowCreateTemplateWizard();
                    return;
                }

                // Edit existing template: open designer directly
                int? templateId = (int?)template.WorkflowDefinitionId;
                using (var designerForm = new WorkflowDesignerForm(
                    templateId, _templateService, _loggingService, _userContext))
                {
                    designerForm.ShowDialog(this.MdiParent ?? this);
                }
                _presenter.LoadTemplates();
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"ShowTemplateForm Exception: {ex.GetType().Name} - {ex.Message}\nStackTrace: {ex.StackTrace}", ex);
                ShowError($"ไม่สามารถเปิดหน้าจอ Workflow Template ได้:\n\n{ex.GetType().Name}\n{ex.Message}");
            }
        }

        private void ShowCreateTemplateWizard()
        {
            if (_templateService == null)
            {
                MessageBox.Show(
                    "ไม่สามารถสร้าง Template ได้ เนื่องจาก WorkflowTemplateService ไม่พร้อมใช้งาน\n\n" +
                    "กรุณาตรวจสอบ:\n" +
                    "1. การเชื่อมต่อฐานข้อมูล\n" +
                    "2. Log file ที่ bin\\Debug\\ เพื่อดูข้อผิดพลาดตอน Startup\n" +
                    "3. Warning dialog ที่อาจขึ้นตอนเปิดโปรแกรม",
                    "ไม่สามารถสร้าง Template",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var dialogOwner = this.MdiParent ?? this;
                var wizard = new WorkflowCreateTemplateWizard(
                    _templateService,
                    _loggingService,
                    _userContext,
                    dialogOwner);

                wizard.ShowWizard();

                if (_presenter != null)
                {
                    _presenter.LoadTemplates();
                }
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"ShowCreateTemplateWizard Exception: {ex.GetType().Name} - {ex.Message}\nStackTrace: {ex.StackTrace}", ex);
                ShowError($"ไม่สามารถเปิดหน้าสร้าง Template ได้:\n\n{ex.GetType().Name}\n{ex.Message}");
            }
        }

        public void ShowVersionHistory(List<WorkflowTemplateVersionResult> versions)
        {
            var list = versions ?? new List<WorkflowTemplateVersionResult>();
            if (list.Count == 0) { ShowInfo("ไม่พบข้อมูล Version"); return; }

            using (var frm = new Form())
            using (var dgv = new DataGridView())
            {
                frm.Text = "ประวัติ Template Version";
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.Width = 800;
                frm.Height = 440;
                frm.Font = new Font("Segoe UI", 9F);

                dgv.Dock = DockStyle.Fill;
                dgv.ReadOnly = true;
                dgv.AutoGenerateColumns = true;
                dgv.RowHeadersVisible = false;
                dgv.AllowUserToAddRows = false;
                dgv.DataSource = list.Select(v => new
                {
                    Version = "v" + v.Version,
                    Name = v.Name,
                    CreatedBy = v.CreatedBy,
                    CreatedDate = v.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                    Status = v.IsActive ? "Active" : "Inactive",
                    InstanceCount = v.InstanceCount,
                    ChangeDescription = v.ChangeDescription
                }).ToList();

                frm.Controls.Add(dgv);
                frm.ShowDialog(this.MdiParent ?? this);
            }
        }

        public void SetActionButtonsEnabled(bool canEdit, bool canDelete, bool canDuplicate)
        {
            SetRibbonPanelEnabled(_pnlRibbonBtnEdit, canEdit);
            SetRibbonPanelEnabled(_pnlRibbonBtnDelete, canDelete);
            SetRibbonPanelEnabled(_pnlRibbonBtnDuplicate, canDuplicate);
            SetRibbonPanelEnabled(_pnlRibbonBtnVersions, canEdit);
            SetRibbonPanelEnabled(_pnlRibbonBtnPublish, canEdit);
            SetRibbonPanelEnabled(_pnlRibbonBtnDeactivate, canEdit);
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "ข้อผิดพลาด",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "ข้อมูล",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public string GetSearchFilter()
        {
            return _searchPlaceholderActive ? string.Empty : txtSearch.Text.Trim();
        }

        public string GetStatusFilter()
        {
            var selected = cmbStatus.SelectedItem as string;
            return string.Equals(selected, "ทั้งหมด", StringComparison.Ordinal) ? null : selected;
        }

        public string GetCategoryFilter()
        {
            var selected = cmbCategory.SelectedItem as string;
            return string.Equals(selected, "ทั้งหมด", StringComparison.Ordinal) ? null : selected;
        }

        // ════════════════════════════════════════════════════════════════════
        // Button events
        // ════════════════════════════════════════════════════════════════════
        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                ShowCreateTemplateWizard();
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"btnCreate_Click Exception: {ex.Message}", ex);
                ShowError($"ไม่สามารถเปิดหน้าสร้าง Template ได้:\n\n{ex.GetType().Name}\n{ex.Message}");
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (_presenter != null) _presenter.EditTemplate(GetSelectedTemplate());
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedTemplate();
            if (selected == null) { ShowError("กรุณาเลือกรายการที่ต้องการลบ"); return; }
            if (_presenter == null) return;

            if (MessageBox.Show(
                    string.Format("ยืนยันลบ Template '{0}' ?", selected.Name),
                    "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                == DialogResult.Yes)
            {
                _presenter.DeleteTemplate(selected);
            }
        }

        private void btnDuplicate_Click(object sender, EventArgs e)
        {
            if (_presenter != null) _presenter.DuplicateTemplate(GetSelectedTemplate());
        }

        private void btnVersions_Click(object sender, EventArgs e)
        {
            if (_presenter != null) _presenter.ViewVersions(GetSelectedTemplate());
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedTemplate();
            if (selected == null) { ShowError("กรุณาเลือก Template ที่ต้องการ Publish"); return; }
            ShowInfo(string.Format("Publish Template '{0}' v{1} เรียบร้อย", selected.Name, selected.Version));
            if (_presenter != null) _presenter.LoadTemplates();
        }

        private void btnDeactivate_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedTemplate();
            if (selected == null) { ShowError("กรุณาเลือก Template ที่ต้องการ Deactivate"); return; }
            if (_presenter == null) return;
            if (MessageBox.Show(
                    string.Format("ยืนยัน Deactivate Template '{0}' ?", selected.Name),
                    "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes)
            {
                _presenter.LoadTemplates();
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                dlg.Title = "นำเข้า Template";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    ShowInfo("Import ไฟล์: " + dlg.FileName);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedTemplate();
            if (selected == null) { ShowError("กรุณาเลือก Template ที่ต้องการ Export"); return; }
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "JSON Files (*.json)|*.json";
                dlg.FileName = string.Format("WFD-{0:D3}_{1}.json",
                    selected.WorkflowDefinitionId, selected.Name);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    ShowInfo("Export ไปที่: " + dlg.FileName);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Filter events
        // ════════════════════════════════════════════════════════════════════
        private void cmbFilter_Changed(object sender, EventArgs e)
        {
            if (_presenter != null) _presenter.SearchTemplates();
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (_searchPlaceholderActive)
            {
                _searchPlaceholderActive = false;
                txtSearch.Text = string.Empty;
                txtSearch.ForeColor = Color.FromArgb(17, 24, 39);
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                _searchPlaceholderActive = true;
                txtSearch.Text = "ค้นหาชื่อ Template...";
                txtSearch.ForeColor = Color.FromArgb(107, 114, 128);
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _presenter != null)
                _presenter.SearchTemplates();
        }

        private void lnkShowAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            cmbCategory.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            if (!_searchPlaceholderActive)
            {
                txtSearch.Text = "ค้นหาชื่อ Template...";
                txtSearch.ForeColor = Color.FromArgb(107, 114, 128);
                _searchPlaceholderActive = true;
            }
            if (_presenter != null) _presenter.LoadTemplates();
        }

        // ════════════════════════════════════════════════════════════════════
        // Sidebar nav
        // ════════════════════════════════════════════════════════════════════
        private void OnNavItemClick(string tag)
        {
            // Handle local navigation first
            if (tag == "template")
            {
                // Already on this page
                return;
            }

            // Delegate to navigation handler for other forms
            WorkflowNavigationHandler.HandleNavigation(
                tag,
                this,
                null, // workflowService
                _templateService,
                null, // slaCalculator
                _loggingService,
                _userContext);
        }

        // ════════════════════════════════════════════════════════════════════
        // Grid events
        // ════════════════════════════════════════════════════════════════════
        private void dgvTemplates_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = GetSelectedTemplate() != null;
            SetActionButtonsEnabled(hasSelection, hasSelection, hasSelection);

            int selCount = dgvTemplates.SelectedRows.Count;
            lblSelectionInfo.Text = selCount > 0
                ? string.Format("เลือก {0} รายการจาก {1}", selCount, _totalCount)
                : string.Format("เลือก 0 รายการจาก {0}", _totalCount);
        }

        private void dgvTemplates_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // ── Category badge ──────────────────────────────────────────────
            if (TryGetColumnIndex("colCategory", out int colCategoryIdx) &&
                e.ColumnIndex == colCategoryIdx)
            {
                e.PaintBackground(e.ClipBounds, true);
                string cat = e.Value as string ?? string.Empty;
                if (!string.IsNullOrEmpty(cat))
                    DrawBadge(e, cat, _categoryColors);
                else
                    e.PaintContent(e.ClipBounds);
                e.Handled = true;
                return;
            }

            // ── Status badge ────────────────────────────────────────────────
            if (TryGetColumnIndex("colStatus", out int colStatusIdx) &&
                e.ColumnIndex == colStatusIdx)
            {
                e.PaintBackground(e.ClipBounds, true);
                string status = e.Value as string ?? "Inactive";
                DrawBadge(e, status, _statusColors);
                e.Handled = true;
                return;
            }

            // ── Owner with avatar circle ────────────────────────────────────
            if (TryGetColumnIndex("colOwner", out int colOwnerIdx) &&
                e.ColumnIndex == colOwnerIdx)
            {
                e.PaintBackground(e.ClipBounds, true);
                string dept = e.Value as string ?? string.Empty;
                string initial = dept.Length > 0 ? dept.Substring(0, 1).ToUpper() : "?";
                DrawOwnerCell(e, initial, dept);
                e.Handled = true;
                return;
            }

            // ── Usage bar ───────────────────────────────────────────────────
            if (TryGetColumnIndex("colUsage", out int colUsageIdx) &&
                e.ColumnIndex == colUsageIdx)
            {
                e.PaintBackground(e.ClipBounds, true);
                int count = e.Value is int v ? v : 0;
                DrawUsageBar(e, count, 200);
                e.Handled = true;
                return;
            }
        }

        private bool TryGetColumnIndex(string columnName, out int index)
        {
            try
            {
                var col = dgvTemplates.Columns[columnName];
                if (col != null)
                {
                    index = col.Index;
                    return true;
                }
            }
            catch { }
            index = -1;
            return false;
        }

        // ════════════════════════════════════════════════════════════════════
        // Custom painting helpers
        // ════════════════════════════════════════════════════════════════════
        private void DrawBadge(DataGridViewCellPaintingEventArgs e,
            string text, Dictionary<string, Color[]> colorMap)
        {
            Color backColor, foreColor;
            if (colorMap.TryGetValue(text, out Color[] colors))
            {
                backColor = colors[0];
                foreColor = colors[1];
            }
            else
            {
                backColor = Color.FromArgb(229, 231, 235);
                foreColor = Color.FromArgb(55, 65, 81);
            }

            string dotText = "● " + text;

            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            using (var font = new Font("Segoe UI", 8F, FontStyle.Regular))
            {
                int bh = e.CellBounds.Height - 14;
                bh = Math.Max(bh, 18);
                SizeF textSize = e.Graphics.MeasureString(dotText, font);
                int bw = (int)textSize.Width + 14;
                int bx = e.CellBounds.X + (e.CellBounds.Width - bw) / 2;
                int by = e.CellBounds.Y + (e.CellBounds.Height - bh) / 2;
                var badgeRect = new Rectangle(bx, by, bw, bh);

                using (var path = RoundedRect(badgeRect, 6))
                using (var backBrush = new SolidBrush(backColor))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(backBrush, path);
                }

                using (var foreBrush = new SolidBrush(foreColor))
                    e.Graphics.DrawString(dotText, font, foreBrush, badgeRect, sf);
            }
        }

        private void DrawOwnerCell(DataGridViewCellPaintingEventArgs e,
            string initial, string deptName)
        {
            int circleSize = 24;
            int cy = e.CellBounds.Y + (e.CellBounds.Height - circleSize) / 2;
            int cx = e.CellBounds.X + 8;
            var circleRect = new Rectangle(cx, cy, circleSize, circleSize);

            // Pick avatar color based on department name hash
            Color avatarColor = _avatarPalette[
                Math.Abs(deptName.GetHashCode()) % _avatarPalette.Length];

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(circleRect);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(avatarColor))
                    e.Graphics.FillEllipse(brush, circleRect);
            }

            // Initial inside circle
            using (var font = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            using (var whiteBrush = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString(initial, font, whiteBrush, circleRect, sf);
            }

            // Department text next to circle
            var textRect = new Rectangle(cx + circleSize + 6, e.CellBounds.Y,
                e.CellBounds.Width - circleSize - 18, e.CellBounds.Height);
            using (var font = new Font("Segoe UI", 8.5F))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            })
            using (var textBrush = new SolidBrush(Color.FromArgb(55, 65, 81)))
            {
                e.Graphics.DrawString(deptName, font, textBrush, textRect, sf);
            }
        }

        private void DrawUsageBar(DataGridViewCellPaintingEventArgs e, int count, int maxCount)
        {
            int barH = 8;
            int barW = e.CellBounds.Width - 48;
            int barX = e.CellBounds.X + 6;
            int barY = e.CellBounds.Y + (e.CellBounds.Height - barH) / 2;
            var bgRect = new Rectangle(barX, barY, barW, barH);
            int filled = maxCount > 0 ? (int)((float)count / maxCount * barW) : 0;
            filled = Math.Min(filled, barW);

            Color fillColor = count == 0
                ? Color.FromArgb(209, 213, 219)
                : count < 20 ? Color.FromArgb(147, 197, 253)
                : count < 80 ? Color.FromArgb(37, 99, 235)
                              : Color.FromArgb(29, 78, 216);

            using (var bgPath = RoundedRect(bgRect, 4))
            using (var bgBrush = new SolidBrush(Color.FromArgb(229, 231, 235)))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(bgBrush, bgPath);
            }

            if (filled > 0)
            {
                var fillRect = new Rectangle(barX, barY, filled, barH);
                using (var fillPath = RoundedRect(fillRect, 4))
                using (var fillBrush = new SolidBrush(fillColor))
                    e.Graphics.FillPath(fillBrush, fillPath);
            }

            // Count label
            var countRect = new Rectangle(barX + barW + 4, e.CellBounds.Y,
                36, e.CellBounds.Height);
            using (var font = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            })
            using (var brush = new SolidBrush(Color.FromArgb(55, 65, 81)))
            {
                e.Graphics.DrawString(count.ToString(), font, brush, countRect, sf);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ════════════════════════════════════════════════════════════════════
        // Ribbon toolbar (dynamic Panel+Label buttons, same pattern as Dashboard)
        // ════════════════════════════════════════════════════════════════════
        private void BuildRibbonToolbar()
        {
            var iconColor = Color.FromArgb(55, 65, 81);
            var hoverColor = Color.FromArgb(243, 244, 246);
            var iconFont = new Font("Segoe MDL2 Assets", 13F);
            var textFont = new Font("Segoe UI", 7.5F);
            var primaryBg = Color.FromArgb(37, 99, 235);

            var groups = new[]
            {
                new { Header = "Template", Items = new[]
                {
                    new { Icon = "", Text = "สร้างใหม่", Tag = "create",    Primary = true  },
                    new { Icon = "", Text = "Duplicate", Tag = "duplicate", Primary = false },
                    new { Icon = "", Text = "แก้ไข",    Tag = "edit",      Primary = false },
                    new { Icon = "", Text = "ลบ",       Tag = "delete",    Primary = false },
                }},
                new { Header = "Publish", Items = new[]
                {
                    new { Icon = "", Text = "Publish",    Tag = "publish",    Primary = false },
                    new { Icon = "", Text = "Deactivate", Tag = "deactivate", Primary = false },
                    new { Icon = "", Text = "ประวัติ",   Tag = "versions",   Primary = false },
                }},
                new { Header = "นำเข้า/ส่งออก", Items = new[]
                {
                    new { Icon = "", Text = "Import", Tag = "import", Primary = false },
                    new { Icon = "", Text = "Export", Tag = "export", Primary = false },
                }},
            };

            int x = 8;
            foreach (var group in groups)
            {
                int groupStartX = x;
                foreach (var item in group.Items)
                {
                    bool primary = item.Primary;
                    string tag = item.Tag;
                    var back = primary ? primaryBg : Color.White;
                    var fg = primary ? Color.White : iconColor;

                    var pnlBtn = new Panel
                    {
                        Location = new Point(x, 4),
                        Size = new Size(60, 66),
                        BackColor = back,
                        Cursor = Cursors.Hand,
                        Tag = tag,
                    };

                    var lblIcon = new Label
                    {
                        Text = item.Icon,
                        Font = iconFont,
                        ForeColor = fg,
                        BackColor = back,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(0, 4),
                        Size = new Size(60, 30),
                    };
                    var lblText = new Label
                    {
                        Text = item.Text,
                        Font = textFont,
                        ForeColor = fg,
                        BackColor = back,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(0, 36),
                        Size = new Size(60, 22),
                    };

                    pnlBtn.Controls.Add(lblIcon);
                    pnlBtn.Controls.Add(lblText);

                    if (!primary)
                    {
                        EventHandler enter = (s, ev) => { pnlBtn.BackColor = hoverColor; lblIcon.BackColor = hoverColor; lblText.BackColor = hoverColor; };
                        EventHandler leave = (s, ev) => { pnlBtn.BackColor = Color.White; lblIcon.BackColor = Color.White; lblText.BackColor = Color.White; };
                        pnlBtn.MouseEnter += enter; lblIcon.MouseEnter += enter; lblText.MouseEnter += enter;
                        pnlBtn.MouseLeave += leave; lblIcon.MouseLeave += leave; lblText.MouseLeave += leave;
                    }

                    EventHandler click = (s, ev) => OnRibbonToolbarClick(tag);
                    pnlBtn.Click += click; lblIcon.Click += click; lblText.Click += click;

                    switch (tag)
                    {
                        case "duplicate": _pnlRibbonBtnDuplicate = pnlBtn; break;
                        case "edit": _pnlRibbonBtnEdit = pnlBtn; break;
                        case "delete": _pnlRibbonBtnDelete = pnlBtn; break;
                        case "publish": _pnlRibbonBtnPublish = pnlBtn; break;
                        case "deactivate": _pnlRibbonBtnDeactivate = pnlBtn; break;
                        case "versions": _pnlRibbonBtnVersions = pnlBtn; break;
                    }

                    pnlRibbonContent.Controls.Add(pnlBtn);
                    x += 62;
                }

                var lblGrp = new Label
                {
                    Text = group.Header,
                    Font = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(107, 114, 128),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(groupStartX, 72),
                    Size = new Size(x - groupStartX, 14),
                };
                pnlRibbonContent.Controls.Add(lblGrp);

                if (group != groups[groups.Length - 1])
                {
                    pnlRibbonContent.Controls.Add(new Panel
                    {
                        BackColor = Color.FromArgb(229, 231, 235),
                        Location = new Point(x + 2, 8),
                        Size = new Size(1, 58),
                    });
                    x += 10;
                }
            }
        }

        private void OnRibbonToolbarClick(string tag)
        {
            switch (tag)
            {
                case "create": btnCreate_Click(null, EventArgs.Empty); break;
                case "duplicate": btnDuplicate_Click(null, EventArgs.Empty); break;
                case "edit": btnEdit_Click(null, EventArgs.Empty); break;
                case "delete": btnDelete_Click(null, EventArgs.Empty); break;
                case "publish": btnPublish_Click(null, EventArgs.Empty); break;
                case "deactivate": btnDeactivate_Click(null, EventArgs.Empty); break;
                case "versions": btnVersions_Click(null, EventArgs.Empty); break;
                case "import": btnImport_Click(null, EventArgs.Empty); break;
                case "export": btnExport_Click(null, EventArgs.Empty); break;
            }
        }

        private static void SetRibbonPanelEnabled(Panel panel, bool enabled)
        {
            if (panel == null) return;
            panel.Enabled = enabled;
            var fg = enabled ? Color.FromArgb(55, 65, 81) : Color.FromArgb(190, 195, 200);
            foreach (Control c in panel.Controls)
            {
                var lbl = c as Label;
                if (lbl != null) lbl.ForeColor = fg;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Paint events for borders
        // ════════════════════════════════════════════════════════════════════
        private void pnlRibbon_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(229, 231, 235), 1))
                e.Graphics.DrawLine(pen, 0, pnlRibbon.Height - 1,
                    pnlRibbon.Width, pnlRibbon.Height - 1);
        }

        private void pnlRibbonTabBar_Paint(object sender, PaintEventArgs e)
        {
            if (_activeTabLabel == null) return;
            using (var pen = new Pen(Color.FromArgb(37, 99, 235), 2))
            {
                int y = pnlRibbonTabBar.Height - 2;
                e.Graphics.DrawLine(pen, _activeTabLabel.Left, y, _activeTabLabel.Right, y);
            }
        }

        private void pnlSearch_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(209, 213, 219), 1))
                e.Graphics.DrawRectangle(pen, 0, 0, pnlSearch.Width - 1, pnlSearch.Height - 1);
        }

        // ════════════════════════════════════════════════════════════════════
        // Ribbon button icon helpers (Segoe MDL2 Assets)
        // ════════════════════════════════════════════════════════════════════
        private static void ApplyButtonIcon(Button btn, string mdl2Char, Color iconColor)
        {
            btn.Image = CreateIconBitmap(mdl2Char, iconColor);
            btn.TextImageRelation = TextImageRelation.ImageAboveText;
            btn.ImageAlign = ContentAlignment.MiddleCenter;
            btn.TextAlign = ContentAlignment.BottomCenter;
            btn.Padding = new Padding(0, 2, 0, 2);
        }

        private static Bitmap CreateIconBitmap(string mdl2Char, Color foreColor)
        {
            const int size = 22;
            var bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                using (var font = new Font("Segoe MDL2 Assets", 11F))
                using (var brush = new SolidBrush(foreColor))
                using (var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                    g.DrawString(mdl2Char, font, brush, new RectangleF(0, 0, size, size), sf);
            }
            return bmp;
        }

        private void pnlFilters_Paint(object sender, PaintEventArgs e)
        {
            // Bottom border line
            using (var pen = new Pen(Color.FromArgb(229, 231, 235), 1))
                e.Graphics.DrawLine(pen, 0, pnlFilters.Height - 1,
                    pnlFilters.Width, pnlFilters.Height - 1);
        }

        private void pnlFooter_Paint(object sender, PaintEventArgs e)
        {
            // Top border line
            using (var pen = new Pen(Color.FromArgb(229, 231, 235), 1))
                e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
        }

        // ════════════════════════════════════════════════════════════════════
        // Resize handlers (keep anchored controls in place)
        // ════════════════════════════════════════════════════════════════════
        private void pnlPageHeader_Resize(object sender, EventArgs e)
        {
            btnCreateNew.Left = pnlPageHeader.Width - btnCreateNew.Width - 16;
        }

        private void pnlFilters_Resize(object sender, EventArgs e)
        {
            lblResultCount.Left = pnlFilters.Width - lblResultCount.Width - 16;
        }

        private void pnlFooter_Resize(object sender, EventArgs e)
        {
            lnkShowAll.Left = pnlFooter.Width - lnkShowAll.Width - 16;
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════
        private void UpdateResultCountLabel()
        {
            lblResultCount.Text = string.Format("พบ {0} Template ทั้งหมด", _totalCount);
            // re-anchor after text change
            lblResultCount.Left = pnlFilters.Width - lblResultCount.Width - 16;
        }

        private WorkflowTemplateResult GetSelectedTemplate()
        {
            if (dgvTemplates.SelectedRows.Count == 0) return null;
            var row = dgvTemplates.SelectedRows[0].DataBoundItem as WorkflowTemplateGridRow;
            if (row == null) return null;
            return _currentTemplates.FirstOrDefault(
                t => t.WorkflowDefinitionId == row.WorkflowDefinitionId);
        }

        private static string ExtractDeptFromADUser(string adUser)
        {
            if (string.IsNullOrWhiteSpace(adUser)) return "Unknown";
            // "domain\\username" → uppercase first letter of username
            int backslash = adUser.LastIndexOf('\\');
            string name = backslash >= 0 ? adUser.Substring(backslash + 1) : adUser;
            return name.Length > 0
                ? char.ToUpper(name[0]) + (name.Length > 1 ? name.Substring(1) : string.Empty)
                : adUser;
        }

        private static string GetInitial(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "?";
            int backslash = text.LastIndexOf('\\');
            string clean = backslash >= 0 ? text.Substring(backslash + 1) : text;
            return clean.Length > 0 ? clean.Substring(0, 1).ToUpper() : "?";
        }

        // ════════════════════════════════════════════════════════════════════
        // Grid row data class
        // ════════════════════════════════════════════════════════════════════
        private sealed class WorkflowTemplateGridRow
        {
            public int WorkflowDefinitionId { get; set; }
            public string DisplayId { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public int StepCount { get; set; }
            public string Version { get; set; }
            public string StatusLabel { get; set; }
            public string OwnerDepartment { get; set; }
            public string OwnerInitial { get; set; }
            public string ModifiedDate { get; set; }
            public int ActiveInstanceCount { get; set; }
        }
    }
}
