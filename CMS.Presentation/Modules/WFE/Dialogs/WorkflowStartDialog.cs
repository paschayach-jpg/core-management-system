using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using Newtonsoft.Json;

namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    /// <summary>
    /// Dialog "เริ่มใช้งาน Workflow" — เลือก Template และกรอก Context เพื่อสร้าง WorkflowInstance.
    /// Layout: title bar navy | left = template list | right = preview + context form | footer.
    /// </summary>
    public partial class WorkflowStartDialog : Form
    {
        // ── Dependencies ─────────────────────────────────────────────────────
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;

        // ── Optional linked entity (Auto-filled from caller) ─────────────────
        private readonly string _linkedEntityType;
        private readonly int?   _linkedEntityId;
        private readonly string _linkedEntityLabel;

        // ── State ─────────────────────────────────────────────────────────────
        private List<WorkflowTemplateResult> _allTemplates = new List<WorkflowTemplateResult>();
        private WorkflowTemplateResult _selectedTemplate;
        private bool _searchPlaceholderActive = true;

        // ── Input ─────────────────────────────────────────────────────────────
        /// <summary>Template ID ที่ให้เลือกอัตโนมัติเมื่อเปิด Dialog</summary>
        public int? PreSelectedTemplateId { get; set; }

        // ── Output ────────────────────────────────────────────────────────────
        /// <summary>Template ที่ user เลือก (set เมื่อ DialogResult = OK)</summary>
        public int SelectedTemplateId   { get; private set; }
        /// <summary>หัวข้อ/เรื่อง ที่กรอก</summary>
        public string InstanceTitle     { get; private set; }
        /// <summary>รายละเอียด</summary>
        public string InstanceDescription { get; private set; }
        /// <summary>Priority index (0=ปกติ … 3=ด่วนมาก)</summary>
        public int    Priority          { get; private set; }
        /// <summary>CRAF index (0=1HC … 3=4PC)</summary>
        public int    CRAFLevel         { get; private set; }
        /// <summary>Context JSON sent to CreateWorkflowInstanceRequest.InputParameters.</summary>
        public string InputParametersJson { get; private set; }

        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color NavyDark   = Color.FromArgb(22,  36,  62);
        private static readonly Color AccentBlue = Color.FromArgb(37,  99,  235);
        private static readonly Color SelectedBg = Color.FromArgb(240, 245, 255);
        private static readonly Color SelectedBorder = Color.FromArgb(59, 91, 219);
        private static readonly Color HoverBg    = Color.FromArgb(248, 250, 255);

        // ── Category icons ────────────────────────────────────────────────────
        private static readonly Dictionary<string, string> CategoryIcons =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "เอกสาร",    "📄" },
                { "IT",        "💻" },
                { "Knowledge", "📚" },
                { "HR",        "👥" },
            };

        // ── Constructor ───────────────────────────────────────────────────────

        public WorkflowStartDialog(
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext,
            string linkedEntityType  = null,
            int?   linkedEntityId    = null,
            string linkedEntityLabel = null)
        {
            InitializeComponent();
            _templateService    = templateService  ?? throw new ArgumentNullException("templateService");
            _loggingService     = loggingService   ?? throw new ArgumentNullException("loggingService");
            _userContext        = userContext       ?? throw new ArgumentNullException("userContext");
            _linkedEntityType   = linkedEntityType;
            _linkedEntityId     = linkedEntityId;
            _linkedEntityLabel  = linkedEntityLabel;

            AdjustButtonLayout();
            btnStart.Enabled = false;
        }

        private void AdjustButtonLayout()
        {
            try
            {
                // Clear anchors first to avoid any layout adjustment conflicts
                btnClose.Anchor = AnchorStyles.None;
                btnStart.Anchor = AnchorStyles.None;
                btnCancel.Anchor = AnchorStyles.None;

                // Set correct coordinates based on design size (1020)
                btnClose.Location = new Point(pnlTitleBar.Width - btnClose.Width - 16, (pnlTitleBar.Height - btnClose.Height) / 2);
                btnStart.Location = new Point(pnlFooter.Width - btnStart.Width - 16, (pnlFooter.Height - btnStart.Height) / 2);
                btnCancel.Location = new Point(btnStart.Left - btnCancel.Width - 12, (pnlFooter.Height - btnCancel.Height) / 2);

                // Re-apply anchors so they scale/move correctly with resizing
                btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnStart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }
            catch (Exception ex)
            {
                _loggingService.Error("WorkflowStartDialog AdjustButtonLayout failed: " + ex.Message, ex);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Load
        // ════════════════════════════════════════════════════════════════════

        private void WorkflowStartDialog_Load(object sender, EventArgs e)
        {
            try
            {
                WireSearchEvents();
                SetEntityBadge();
                LoadTemplates();

                if (PreSelectedTemplateId.HasValue)
                {
                    // Look for template in the loaded list
                    var match = _allTemplates.FirstOrDefault(t => t.WorkflowDefinitionId == PreSelectedTemplateId.Value);
                    if (match == null)
                    {
                        // If it's not active, LoadTemplates wouldn't return it. In that case, let's load it directly from service
                        try
                        {
                            var directTemplate = _templateService.GetTemplate(PreSelectedTemplateId.Value, _userContext);
                            if (directTemplate != null)
                            {
                                var tempResult = new WorkflowTemplateResult
                                {
                                    WorkflowDefinitionId = directTemplate.WorkflowDefinitionId,
                                    Name = directTemplate.Name,
                                    Description = directTemplate.Description,
                                    Version = directTemplate.Version,
                                    IsActive = directTemplate.IsActive,
                                    ApprovalStatus = directTemplate.ApprovalStatus,
                                    Category = "ทั่วไป",
                                    StepCount = 3 // placeholder/default steps
                                };
                                _allTemplates.Add(tempResult);
                                RenderTemplateList(_allTemplates);
                                match = tempResult;
                            }
                        }
                        catch (Exception ex)
                        {
                            _loggingService.Warn("Pre-selected template fetch failed: " + ex.Message);
                        }
                    }

                    if (match != null)
                    {
                        SelectTemplate(match);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error("WorkflowStartDialog Load: " + ex.Message, ex);
                MessageBox.Show("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Load templates from service
        // ════════════════════════════════════════════════════════════════════

        private void LoadTemplates(string keyword = null)
        {
            try
            {
                var filter = new WorkflowTemplateFilter
                {
                    IsActive      = true,
                    SearchKeyword = keyword,
                    PageNumber    = 1,
                    PageSize      = 200,
                };
                var result = _templateService.GetTemplates(filter, _userContext);
                _allTemplates = result.Items != null ? result.Items.ToList() : new List<WorkflowTemplateResult>();
                RenderTemplateList(_allTemplates);
            }
            catch (Exception ex)
            {
                _loggingService.Warn("WorkflowStartDialog LoadTemplates: " + ex.Message);
                // Show empty list gracefully
                _allTemplates = new List<WorkflowTemplateResult>();
                RenderTemplateList(_allTemplates);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Render template list (grouped by category)
        // ════════════════════════════════════════════════════════════════════

        private void RenderTemplateList(List<WorkflowTemplateResult> templates)
        {
            pnlTemplateList.SuspendLayout();
            pnlTemplateList.Controls.Clear();

            if (templates.Count == 0)
            {
                pnlTemplateList.Controls.Add(new Label
                {
                    Text      = "ไม่พบ Template ที่ตรงกัน",
                    Font      = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(140, 150, 165),
                    AutoSize  = true,
                    Location  = new Point(12, 20),
                });
                pnlTemplateList.ResumeLayout();
                return;
            }

            // Group by category
            var groups = templates
                .GroupBy(t => string.IsNullOrWhiteSpace(t.Category) ? "ทั่วไป" : t.Category)
                .OrderBy(g => g.Key);

            int y = 4;
            foreach (var group in groups)
            {
                // Category header
                string icon = CategoryIcons.ContainsKey(group.Key) ? CategoryIcons[group.Key] : "📁";
                var lblCat = new Label
                {
                    Text      = string.Format("{0}  {1}", icon, group.Key),
                    Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(80, 90, 110),
                    Location  = new Point(4, y),
                    AutoSize  = true,
                };
                pnlTemplateList.Controls.Add(lblCat);
                y += 24;

                foreach (var t in group.OrderBy(x => x.Name))
                {
                    var card = BuildTemplateCard(t);
                    card.Location = new Point(0, y);
                    pnlTemplateList.Controls.Add(card);
                    y += card.Height + 2;
                }
                y += 6;
            }

            pnlTemplateList.ResumeLayout();
        }

        private Panel BuildTemplateCard(WorkflowTemplateResult t)
        {
            bool isSelected = _selectedTemplate != null &&
                              _selectedTemplate.WorkflowDefinitionId == t.WorkflowDefinitionId;

            var pnl = new Panel
            {
                Size      = new Size(418, 56),
                BackColor = isSelected ? SelectedBg : Color.White,
                Cursor    = Cursors.Hand,
                Tag       = t,
                Padding   = new Padding(0),
            };

            // Radio circle
            var radio = new Panel
            {
                Location  = new Point(10, 18),
                Size      = new Size(18, 18),
                BackColor = Color.Transparent,
                Tag       = "radio",
            };
            radio.Paint += (s, pe) => DrawRadio(pe.Graphics, isSelected);

            // Star (popular) — show if ActiveInstanceCount > 5
            string namePrefix = t.ActiveInstanceCount > 5 ? "⭐  " : "";

            var lblName = new Label
            {
                Text      = namePrefix + (t.Name ?? "-"),
                Font      = new Font("Segoe UI", 9.5F, isSelected ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = isSelected ? NavyDark : Color.FromArgb(30, 36, 55),
                Location  = new Point(36, 8),
                Size      = new Size(370, 22),
                AutoEllipsis = true,
            };

            var lblSub = new Label
            {
                Text      = string.Format("WFD-{0:D3}  ·  {1} ขั้นตอน{2}",
                    t.WorkflowDefinitionId,
                    t.StepCount,
                    t.ActiveInstanceCount > 5 ? "  ·  ยอดนิยม" : ""),
                Font      = new Font("Segoe UI", 8F),
                ForeColor = isSelected ? Color.FromArgb(59, 91, 219) : Color.FromArgb(110, 120, 140),
                Location  = new Point(36, 30),
                Size      = new Size(370, 18),
            };

            pnl.Controls.Add(radio);
            pnl.Controls.Add(lblName);
            pnl.Controls.Add(lblSub);

            // Border paint
            pnl.Paint += (s, pe) =>
            {
                if (isSelected)
                {
                    using (var pen = new Pen(SelectedBorder, 1.5f))
                    {
                        var r = new Rectangle(1, 1, pnl.Width - 3, pnl.Height - 3);
                        pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        pe.Graphics.DrawRectangle(pen, r);
                    }
                }
                else
                {
                    using (var pen = new Pen(Color.FromArgb(225, 228, 235), 1f))
                        pe.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                }
            };

            // Hover
            EventHandler enter = (s, ev) => { if (!isSelected) pnl.BackColor = HoverBg; };
            EventHandler leave = (s, ev) => { if (!isSelected) pnl.BackColor = Color.White; };
            EventHandler click = (s, ev) => SelectTemplate(t);

            foreach (Control c in new Control[] { pnl, radio, lblName, lblSub })
            {
                c.MouseEnter += enter;
                c.MouseLeave += leave;
                c.Click      += click;
            }

            return pnl;
        }

        private static void DrawRadio(Graphics g, bool filled)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(1, 1, 14, 14);
            using (var pen = new Pen(filled ? SelectedBorder : Color.FromArgb(180, 185, 200), 1.5f))
                g.DrawEllipse(pen, r);
            if (filled)
            {
                var inner = new Rectangle(4, 4, 8, 8);
                using (var br = new SolidBrush(SelectedBorder))
                    g.FillEllipse(br, inner);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Select template → update preview card
        // ════════════════════════════════════════════════════════════════════

        private void SelectTemplate(WorkflowTemplateResult t)
        {
            _selectedTemplate = t;
            RenderTemplateList(_allTemplates); // re-render to update selection state
            RenderPreviewCard(t);
            btnStart.Enabled = true;
        }

        private void RenderPreviewCard(WorkflowTemplateResult t)
        {
            pnlTemplatePreview.Controls.Clear();
            pnlTemplatePreview.BackColor = SelectedBg;
            pnlTemplatePreview.Paint += (s, pe) =>
            {
                using (var pen = new Pen(SelectedBorder, 1.5f))
                    pe.Graphics.DrawRectangle(pen, 0, 0, pnlTemplatePreview.Width - 1, pnlTemplatePreview.Height - 1);
            };

            // Title
            var lblName = new Label
            {
                Text      = string.Format("WFD-{0:D3} · {1}", t.WorkflowDefinitionId, t.Name),
                Font      = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = NavyDark,
                Location  = new Point(12, 10),
                Size      = new Size(476, 22),
                AutoEllipsis = true,
            };

            int totalSLA = t.StepCount * 2; // estimate
            var lblMeta = new Label
            {
                Text      = string.Format("SLA รวม: {0} วันทำการ  ·  {1} ขั้นตอน  ·  Category: {2}",
                    totalSLA, t.StepCount, t.Category ?? "-"),
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(80, 90, 110),
                Location  = new Point(12, 34),
                Size      = new Size(476, 18),
            };

            // Step flow (simplified — show Start → ... → End)
            var pnlFlow = new Panel
            {
                Location  = new Point(12, 58),
                Size      = new Size(476, 36),
                BackColor = Color.Transparent,
            };
            BuildFlowPreview(pnlFlow, t);

            pnlTemplatePreview.Controls.Add(lblName);
            pnlTemplatePreview.Controls.Add(lblMeta);
            pnlTemplatePreview.Controls.Add(pnlFlow);
        }

        private static void BuildFlowPreview(Panel container, WorkflowTemplateResult t)
        {
            // Build simplified step chips: Start → [steps] → End
            var steps = new List<string> { "Start" };
            if (t.StepCount > 2)
                steps.Add(string.Format("ขั้นตอน · {0}ว", 2));
            if (t.StepCount > 1)
                steps.Add(string.Format("ขั้นตอน · {0}ว", 3));
            steps.Add("End");

            int x = 0;
            for (int i = 0; i < steps.Count; i++)
            {
                bool isStart = i == 0;
                bool isEnd   = i == steps.Count - 1;
                Color bg = isStart ? Color.FromArgb(34, 153, 84)
                         : isEnd   ? Color.FromArgb(185, 40, 40)
                                   : Color.FromArgb(37, 99, 235);

                var chip = new Label
                {
                    Text      = steps[i],
                    Font      = new Font("Segoe UI", 8F),
                    ForeColor = Color.White,
                    BackColor = bg,
                    Location  = new Point(x, 6),
                    Size      = new Size(Math.Max(50, steps[i].Length * 7 + 12), 24),
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                container.Controls.Add(chip);
                x += chip.Width + 2;

                if (i < steps.Count - 1)
                {
                    container.Controls.Add(new Label
                    {
                        Text      = "›",
                        Font      = new Font("Segoe UI", 10F),
                        ForeColor = Color.FromArgb(140, 150, 165),
                        Location  = new Point(x, 6),
                        Size      = new Size(16, 24),
                        TextAlign = ContentAlignment.MiddleCenter,
                    });
                    x += 18;
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Entity badge (Auto-filled)
        // ════════════════════════════════════════════════════════════════════

        private void SetEntityBadge()
        {
            if (!string.IsNullOrEmpty(_linkedEntityLabel))
            {
                lblEntityValue.Text      = string.Format("🔗  {0}", _linkedEntityLabel);
                lblEntityValue.ForeColor = Color.FromArgb(37, 99, 235);
                pnlEntityBadge.BackColor = Color.FromArgb(240, 245, 255);
            }
            else
            {
                lblEntityValue.Text      = "(ยังไม่ได้เลือก Entity)";
                lblEntityValue.ForeColor = Color.FromArgb(140, 150, 165);
                pnlEntityBadge.BackColor = Color.FromArgb(243, 246, 251);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Search
        // ════════════════════════════════════════════════════════════════════

        private void WireSearchEvents()
        {
            txtSearch.Enter += (s, e) =>
            {
                if (_searchPlaceholderActive)
                {
                    _searchPlaceholderActive = false;
                    txtSearch.Text      = string.Empty;
                    txtSearch.ForeColor = Color.FromArgb(30, 36, 55);
                }
            };
            txtSearch.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    _searchPlaceholderActive = true;
                    txtSearch.Text      = "🔍  ค้นหา Template...";
                    txtSearch.ForeColor = Color.FromArgb(140, 150, 165);
                }
            };
            txtSearch.TextChanged += (s, e) =>
            {
                if (_searchPlaceholderActive) return;
                string kw = txtSearch.Text.Trim();
                var filtered = string.IsNullOrEmpty(kw)
                    ? _allTemplates
                    : _allTemplates.Where(t =>
                        (t.Name ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (t.Category ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                      .ToList();
                RenderTemplateList(filtered);
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // Buttons
        // ════════════════════════════════════════════════════════════════════

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            SelectedTemplateId    = _selectedTemplate.WorkflowDefinitionId;
            InstanceTitle         = txtTitle.Text.Trim();
            InstanceDescription   = txtDescription.Text.Trim();
            Priority              = cmbPriority.SelectedIndex;
            CRAFLevel             = cmbCRAF.SelectedIndex;
            InputParametersJson   = BuildInputParametersJson();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateInput()
        {
            if (_selectedTemplate == null)
            {
                MessageBox.Show("กรุณาเลือก Template ก่อน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || txtTitle.Text == "🔍  ค้นหา Template...")
            {
                MessageBox.Show("กรุณากรอกหัวข้อ/เรื่อง", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitle.Focus();
                return false;
            }
            return true;
        }

        private string BuildInputParametersJson()
        {
            return JsonConvert.SerializeObject(new
            {
                Title = InstanceTitle,
                Description = InstanceDescription,
                Priority = Priority,
                PriorityText = cmbPriority.SelectedItem != null ? cmbPriority.SelectedItem.ToString() : null,
                CRAFLevel = CRAFLevel,
                CRAFText = cmbCRAF.SelectedItem != null ? cmbCRAF.SelectedItem.ToString() : null,
                TemplateId = SelectedTemplateId,
                TemplateName = _selectedTemplate != null ? _selectedTemplate.Name : null,
                LinkedEntityType = _linkedEntityType,
                LinkedEntityId = _linkedEntityId,
                LinkedEntityLabel = _linkedEntityLabel,
                StartedBy = _userContext.ADUserName,
                StartedAt = DateTime.Now
            });
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
