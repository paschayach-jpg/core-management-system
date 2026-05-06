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
    /// Form for approving/rejecting Workflow Templates (Draft status).
    /// Follows WFE Form UI pattern from wfe-form-ui.md.
    /// </summary>
    public partial class TemplateApprovalForm : Form, ITemplateApprovalView
    {
        private readonly TemplateApprovalPresenter _presenter;
        private readonly UserContext _userContext;
        private readonly List<WorkflowTemplateResult> _currentItems = new List<WorkflowTemplateResult>();
        private int _totalCount;
        private bool _searchPlaceholderActive = true;
        private Panel _activeNavItem;

        private readonly Color _navyDark = Color.FromArgb(31, 56, 100);
        private readonly Color _navHover = Color.FromArgb(245, 249, 255);

        private static readonly Dictionary<string, Color> StatusColors = new Dictionary<string, Color>
        {
            { "Draft",    Color.FromArgb(197, 120, 16) },
            { "Active",   Color.FromArgb(23,  133, 49) },
            { "Inactive", Color.FromArgb(120, 130, 150) },
        };

        public TemplateApprovalForm(
            IWorkflowTemplateService templateService,
            IAuditService auditService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            InitializeComponent();
            _userContext = userContext;

            // Only create presenter if services are available
            if (templateService != null && loggingService != null && userContext != null)
            {
                _presenter = new TemplateApprovalPresenter(
                    this, templateService, auditService, loggingService, userContext);
            }
        }

        private void TemplateApprovalForm_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            try
            {
                BuildRibbonTabs();
                BuildToolbar();
                Panel initialNav;
                WorkflowNavigationBuilder.BuildSidebar(
                    pnlSidebarContainer, "approval", OnNavItemClick, out initialNav);
                _activeNavItem = initialNav;
                BuildStatusBar();
                SetupListView();
                SetupFilterBar();
                BuildSummaryCards(0, 0, 0);
                if (_presenter != null)
                    _presenter.LoadPendingTemplates();
            }
            catch (Exception ex)
            {
                ShowError("Error loading form: " + ex.Message);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // ITemplateApprovalView implementation
        // ════════════════════════════════════════════════════════════════════

        public void DisplayPendingTemplates(PagedResult<WorkflowTemplateResult> result)
        {
            var items = result != null && result.Items != null
                ? result.Items.ToList()
                : new List<WorkflowTemplateResult>();

            _currentItems.Clear();
            _currentItems.AddRange(items);
            _totalCount = result != null ? result.TotalCount : 0;

            lvTemplates.Items.Clear();
            foreach (var t in items)
            {
                var lvi = new ListViewItem(string.Format("WFD-{0:D3}", t.WorkflowDefinitionId));
                lvi.SubItems.Add(t.Name ?? "-");
                lvi.SubItems.Add(t.Category ?? "-");
                lvi.SubItems.Add(string.Format("v{0}", t.Version));
                lvi.SubItems.Add(t.CreatedBy ?? "-");
                lvi.SubItems.Add(t.ModifiedDate.ToString("dd/MM/yy"));
                lvi.SubItems.Add(t.StatusLabel ?? "Draft");
                lvi.Tag = t;
                lvTemplates.Items.Add(lvi);
            }

            lblResultCount.Text = string.Format("พบ {0} รายการรออนุมัติ", _totalCount);
            lblSelectionInfo.Text = string.Format("แสดง {0} รายการ", items.Count);
            SetActionButtonsEnabled(false, false, false);
        }

        public void SetActionButtonsEnabled(bool canApprove, bool canReject, bool canPreview)
        {
            foreach (Control c in pnlToolbarContainer.Controls)
            {
                var pnl = c as Panel;
                if (pnl == null) continue;
                var tag = pnl.Tag as string ?? string.Empty;
                if (tag == "approve") pnl.Enabled = canApprove;
                if (tag == "reject")  pnl.Enabled = canReject;
                if (tag == "preview") pnl.Enabled = canPreview;
            }
        }

        public void UpdatePendingCount(int count)
        {
            lblSubtitle.Text = string.Format("รายการ Template ที่รออนุมัติ ({0} รายการ)", count);
        }

        public void ShowError(string message)
            => MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void ShowInfo(string message)
            => MessageBox.Show(message, "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public string GetSearchFilter()
            => _searchPlaceholderActive ? string.Empty : txtSearch.Text.Trim();

        public string GetCategoryFilter()
        {
            var sel = cmbCategory.SelectedItem as string;
            return string.Equals(sel, "ทั้งหมด", StringComparison.Ordinal) ? null : sel;
        }

        // ════════════════════════════════════════════════════════════════════
        // BuildRibbonTabs — wfe-form-ui.md §1
        // ════════════════════════════════════════════════════════════════════

        private void BuildRibbonTabs()
        {
            var tabs = new[]
            {
                new { Text = "แฟ้ม",       Active = false },
                new { Text = "หน้าแรก",    Active = true  },
                new { Text = "ดำเนินการ",  Active = false },
                new { Text = "มุมมอง",     Active = false },
                new { Text = "เครื่องมือ", Active = false },
                new { Text = "ช่วยเหลือ",  Active = false },
            };
            int x = 8;
            foreach (var t in tabs)
            {
                var btn = new Button
                {
                    Text      = t.Text,
                    Location  = new Point(x, 0),
                    Size      = new Size(MeasureTabWidth(t.Text) + 24, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = t.Active ? Color.White : Color.FromArgb(190, 210, 245),
                    Font      = t.Active
                        ? new Font("Segoe UI", 9.5F, FontStyle.Bold)
                        : new Font("Segoe UI", 9.5F),
                    Cursor  = Cursors.Hand,
                    TabStop = false,
                };
                btn.FlatAppearance.BorderSize           = 0;
                btn.FlatAppearance.MouseOverBackColor   = Color.FromArgb(50, 80, 130);
                btn.FlatAppearance.MouseDownBackColor   = Color.FromArgb(40, 70, 120);
                if (t.Active)
                    btn.Paint += (s, pe) =>
                        pe.Graphics.FillRectangle(Brushes.White, 0, btn.Height - 3, btn.Width, 3);
                pnlRibbonTabs.Controls.Add(btn);
                x += btn.Width + 2;
            }
        }

        private int MeasureTabWidth(string text)
        {
            using (var g = CreateGraphics())
            using (var f = new Font("Segoe UI", 9.5F))
                return (int)g.MeasureString(text, f).Width;
        }

        // ════════════════════════════════════════════════════════════════════
        // BuildToolbar — wfe-form-ui.md §2
        // ════════════════════════════════════════════════════════════════════

        private void BuildToolbar()
        {
            var groups = new[]
            {
                new { Header = "การอนุมัติ", Items = new[]
                {
                    new { Icon = "✔", Text = "อนุมัติ",       Tag = "approve" },
                    new { Icon = "✖", Text = "ปฏิเสธ",        Tag = "reject"  },
                    new { Icon = "👁", Text = "ดูรายละเอียด", Tag = "preview" },
                }},
                new { Header = "รายการ", Items = new[]
                {
                    new { Icon = "🔄", Text = "รีเฟรช", Tag = "refresh" },
                    new { Icon = "📊", Text = "ส่งออก", Tag = "export"  },
                }},
            };

            int x = 8;
            foreach (var group in groups)
            {
                int groupStartX = x;
                foreach (var item in group.Items)
                {
                    var tag = item.Tag;
                    var pnlBtn = new Panel
                    {
                        Location  = new Point(x, 2),
                        Size      = new Size(58, 60),
                        BackColor = Color.White,
                        Cursor    = Cursors.Hand,
                        Tag       = tag,
                    };
                    var lblIcon = new Label
                    {
                        Text      = item.Icon,
                        Font      = new Font("Segoe UI", 16F),
                        ForeColor = _navyDark,
                        Location  = new Point(0, 6),
                        Size      = new Size(58, 28),
                        TextAlign = ContentAlignment.MiddleCenter,
                    };
                    var lblText = new Label
                    {
                        Text      = item.Text,
                        Font      = new Font("Segoe UI", 7.5F),
                        ForeColor = Color.FromArgb(55, 65, 90),
                        Location  = new Point(0, 34),
                        Size      = new Size(58, 20),
                        TextAlign = ContentAlignment.MiddleCenter,
                    };
                    pnlBtn.Controls.Add(lblIcon);
                    pnlBtn.Controls.Add(lblText);

                    EventHandler enter = (s, ev) => { pnlBtn.BackColor = lblIcon.BackColor = lblText.BackColor = _navHover; };
                    EventHandler leave = (s, ev) => { pnlBtn.BackColor = lblIcon.BackColor = lblText.BackColor = Color.White; };
                    EventHandler click = (s, ev) => OnToolbarClick(tag);
                    foreach (Control c in new Control[] { pnlBtn, lblIcon, lblText })
                    { c.MouseEnter += enter; c.MouseLeave += leave; c.Click += click; }

                    pnlToolbarContainer.Controls.Add(pnlBtn);
                    x += 60;
                }

                pnlToolbarContainer.Controls.Add(new Label
                {
                    Text      = group.Header,
                    Font      = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(120, 130, 150),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location  = new Point(groupStartX, 62),
                    Size      = new Size(x - groupStartX, 10),
                });

                if (group != groups[groups.Length - 1])
                {
                    pnlToolbarContainer.Controls.Add(new Panel
                    {
                        BackColor = Color.FromArgb(220, 225, 235),
                        Location  = new Point(x + 2, 8),
                        Size      = new Size(1, 54),
                    });
                    x += 10;
                }
            }

            pnlToolbarContainer.Controls.Add(new Panel
                { BackColor = Color.FromArgb(220, 225, 235), Dock = DockStyle.Bottom, Height = 1 });
        }

        private void OnToolbarClick(string tag)
        {
            switch (tag)
            {
                case "approve": ApproveSelected();  break;
                case "reject":  RejectSelected();   break;
                case "preview": PreviewSelected();  break;
                case "refresh":
                    if (_presenter != null) _presenter.LoadPendingTemplates();
                    break;
                case "export":  ShowInfo("Export: ยังไม่ได้ Implement"); break;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // BuildStatusBar — wfe-form-ui.md §12
        // ════════════════════════════════════════════════════════════════════

        private void BuildStatusBar()
        {
            pnlStatusBar.Paint += (s, pe) =>
                pe.Graphics.DrawLine(new Pen(Color.FromArgb(210, 215, 225)), 0, 0, pnlStatusBar.Width, 0);

            var statusItems = new[]
            {
                "  พร้อมใช้งาน  ",
                string.Format("  โดย: {0}  ", _userContext.ADUserName ?? _userContext.AdIdentity),
                "  Zoom 100%  ",
            };
            int x = 8;
            foreach (var text in statusItems)
            {
                var lbl = new Label
                {
                    Text      = text,
                    Font      = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(80, 90, 110),
                    AutoSize  = true,
                    Location  = new Point(x, 4),
                };
                pnlStatusBar.Controls.Add(lbl);
                x += lbl.PreferredWidth + 4;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // SetupListView — OwnerDraw, row 46px — wfe-form-ui.md §7
        // ════════════════════════════════════════════════════════════════════

        private void SetupListView()
        {
            var img = new ImageList { ImageSize = new Size(1, 46) };
            lvTemplates.SmallImageList = img;

            lvTemplates.Columns.Add("ID",           70);
            lvTemplates.Columns.Add("ชื่อ Template", 280);
            lvTemplates.Columns.Add("หมวดหมู่",      110);
            lvTemplates.Columns.Add("เวอร์ชั่น",     70);
            lvTemplates.Columns.Add("สร้างโดย",      140);
            lvTemplates.Columns.Add("แก้ไขล่าสุด",   100);
            lvTemplates.Columns.Add("สถานะ",         100);

            lvTemplates.DrawColumnHeader     += LvTemplates_DrawColumnHeader;
            lvTemplates.DrawItem             += LvTemplates_DrawItem;
            lvTemplates.DrawSubItem          += LvTemplates_DrawSubItem;
            lvTemplates.SelectedIndexChanged += LvTemplates_SelectedIndexChanged;
        }

        private void LvTemplates_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(246, 248, 251)), e.Bounds);
            e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 224, 232)),
                e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
            using (var f = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                var r = new RectangleF(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 14, e.Bounds.Height);
                e.Graphics.DrawString(e.Header.Text, f, new SolidBrush(Color.FromArgb(55, 65, 90)), r, sf);
            }
        }

        private void LvTemplates_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            bool sel = e.Item.Selected;
            var bg = sel ? Color.FromArgb(232, 241, 255)
                         : (e.ItemIndex % 2 == 0 ? Color.White : Color.FromArgb(250, 251, 253));
            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
            e.Graphics.DrawLine(new Pen(Color.FromArgb(235, 238, 244)),
                e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private void LvTemplates_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.Item == null) return;
            var r = e.Bounds;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            switch (e.ColumnIndex)
            {
                case 0: // ID — blue link
                    using (var f = new Font("Segoe UI", 9F))
                    using (var b = new SolidBrush(Color.FromArgb(0, 90, 170)))
                        g.DrawString(e.SubItem.Text, f, b, PadRect(r, 10, 0),
                            new StringFormat { LineAlignment = StringAlignment.Center });
                    break;

                case 1: // Name — bold
                    using (var f = new Font("Segoe UI", 9.5F, FontStyle.Bold))
                    using (var b = new SolidBrush(Color.FromArgb(30, 36, 55)))
                        g.DrawString(e.SubItem.Text, f, b, PadRect(r, 10, 0),
                            new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
                    break;

                case 6: // Status badge
                    DrawStatusBadge(g, r, e.SubItem.Text);
                    break;

                default:
                    using (var f = new Font("Segoe UI", 9F))
                    using (var b = new SolidBrush(Color.FromArgb(55, 65, 90)))
                        g.DrawString(e.SubItem.Text, f, b, PadRect(r, 10, 0),
                            new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
                    break;
            }
        }

        private void LvTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool has = lvTemplates.SelectedItems.Count > 0;
            SetActionButtonsEnabled(has, has, has);
            int sel = lvTemplates.SelectedItems.Count;
            lblSelectionInfo.Text = sel > 0
                ? string.Format("เลือก {0} รายการจาก {1}", sel, _totalCount)
                : string.Format("แสดง {0} รายการ", _currentItems.Count);
        }

        // ════════════════════════════════════════════════════════════════════
        // SetupFilterBar — wfe-form-ui.md §6
        // ════════════════════════════════════════════════════════════════════

        private void SetupFilterBar()
        {
            cmbCategory.Items.Clear();
            foreach (var s in new[] { "ทั้งหมด", "เอกสาร", "IT", "Knowledge", "HR" })
                cmbCategory.Items.Add(s);
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += (s, e) => { if (_presenter != null) _presenter.Search(); };

            btnClearFilter.Location = new Point(pnlFilterBar.Width - 130, 8);
            btnClearFilter.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnPrev.Location = new Point(pnlPagination.Width - 290, 9);
            lblPage.Location = new Point(pnlPagination.Width - 185, 9);
            btnNext.Location = new Point(pnlPagination.Width - 100, 9);

            txtSearch.Enter += (s, e) =>
            {
                if (_searchPlaceholderActive)
                {
                    _searchPlaceholderActive = false;
                    txtSearch.Text = string.Empty;
                    txtSearch.ForeColor = Color.FromArgb(30, 36, 55);
                }
            };
            txtSearch.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    _searchPlaceholderActive = true;
                    txtSearch.Text = "ค้นหาชื่อ Template...";
                    txtSearch.ForeColor = Color.FromArgb(140, 150, 165);
                }
            };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter && _presenter != null) _presenter.Search(); };

            btnSearch.Click += (s, e) => { if (_presenter != null) _presenter.Search(); };
            btnClearFilter.Click += (s, e) =>
            {
                cmbCategory.SelectedIndex = 0;
                txtSearch.Text = "ค้นหาชื่อ Template...";
                _searchPlaceholderActive = true;
                txtSearch.ForeColor = Color.FromArgb(140, 150, 165);
                if (_presenter != null) _presenter.LoadPendingTemplates();
            };
            btnPrev.Click += (s, e) => { if (_presenter != null) _presenter.PreviousPage(); };
            btnNext.Click += (s, e) => { if (_presenter != null) _presenter.NextPage(); };
        }

        // ════════════════════════════════════════════════════════════════════
        // Summary Cards — wfe-form-ui.md §5
        // ════════════════════════════════════════════════════════════════════

        public void BuildSummaryCards(int pending, int approvedToday, int rejectedToday)
        {
            flpCards.Controls.Clear();
            flpCards.Controls.Add(BuildCard("⏳", "รออนุมัติ",     pending,       Color.FromArgb(197, 120, 16)));
            flpCards.Controls.Add(BuildCard("✔",  "อนุมัติวันนี้", approvedToday, Color.FromArgb(23,  133, 49)));
            flpCards.Controls.Add(BuildCard("✖",  "ปฏิเสธวันนี้",  rejectedToday, Color.FromArgb(194, 38,  54)));
        }

        private Panel BuildCard(string icon, string title, int count, Color color)
        {
            var pnl = new Panel
            {
                Size        = new Size(200, 82),
                BorderStyle = BorderStyle.FixedSingle,
                Margin      = new Padding(0, 0, 10, 0),
                Padding     = new Padding(14, 10, 14, 8),
            };
            pnl.Controls.Add(new Label
            {
                Text = icon, Font = new Font("Segoe UI", 14F),
                ForeColor = color, AutoSize = true, Location = new Point(14, 8),
            });
            pnl.Controls.Add(new Label
            {
                Text = title, Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(80, 90, 110), AutoSize = true, Location = new Point(40, 12),
            });
            pnl.Controls.Add(new Label
            {
                Text = count.ToString(), Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = color, AutoSize = true, Location = new Point(14, 28),
            });
            return pnl;
        }

        // ════════════════════════════════════════════════════════════════════
        // Actions
        // ════════════════════════════════════════════════════════════════════

        private void ApproveSelected()
        {
            var t = GetSelectedTemplate();
            if (t == null) { ShowError("กรุณาเลือก Template ที่ต้องการอนุมัติ"); return; }
            if (_presenter == null) { ShowError("ไม่สามารถดำเนินการได้ เนื่องจากไม่มีการเชื่อมต่อกับระบบ"); return; }
            if (MessageBox.Show(
                    string.Format("ยืนยันอนุมัติ Template '{0}' v{1} ?", t.Name, t.Version),
                    "ยืนยันการอนุมัติ", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes)
            {
                _presenter.ApproveTemplate(t);
            }
        }

        private void RejectSelected()
        {
            var t = GetSelectedTemplate();
            if (t == null) { ShowError("กรุณาเลือก Template ที่ต้องการปฏิเสธ"); return; }
            if (_presenter == null) { ShowError("ไม่สามารถดำเนินการได้ เนื่องจากไม่มีการเชื่อมต่อกับระบบ"); return; }
            string reason = PromptReason();
            if (reason == null) return;
            _presenter.RejectTemplate(t, reason);
        }

        private void PreviewSelected()
        {
            var t = GetSelectedTemplate();
            if (t == null) { ShowError("กรุณาเลือก Template ก่อน"); return; }
            ShowInfo(string.Format(
                "Preview: {0} v{1}\nหมวดหมู่: {2}\nสร้างโดย: {3}\nสถานะ: {4}",
                t.Name, t.Version, t.Category ?? "-", t.CreatedBy ?? "-", t.StatusLabel));
        }

        private string PromptReason()
        {
            using (var dlg = new Form())
            {
                dlg.Text            = "ระบุเหตุผลการปฏิเสธ";
                dlg.ClientSize      = new Size(400, 160);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;

                var lbl       = new Label { Text = "เหตุผล:", AutoSize = true, Location = new Point(16, 16) };
                var txt       = new TextBox { Location = new Point(16, 36), Size = new Size(368, 60), Multiline = true };
                var btnOk     = new Button { Text = "ยืนยัน",  DialogResult = DialogResult.OK,     Location = new Point(220, 110), Size = new Size(80, 28) };
                var btnCancel = new Button { Text = "ยกเลิก", DialogResult = DialogResult.Cancel, Location = new Point(308, 110), Size = new Size(76, 28) };

                dlg.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                return dlg.ShowDialog(this) == DialogResult.OK ? txt.Text.Trim() : null;
            }
        }

        private void OnNavItemClick(string tag)
        {
            // Handle local navigation first
            if (tag == "approval")
            {
                // Already on this page
                return;
            }

            // Delegate to navigation handler for other forms
            // Note: TemplateApprovalForm doesn't store service references directly
            WorkflowNavigationHandler.HandleNavigation(
                tag,
                this,
                null, // workflowService
                null, // templateService - managed by presenter
                null, // slaCalculator
                null, // loggingService - managed by presenter
                _userContext,
                null); // auditService - managed by presenter
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers — wfe-form-ui.md §8, §12
        // ════════════════════════════════════════════════════════════════════

        private WorkflowTemplateResult GetSelectedTemplate()
            => lvTemplates.SelectedItems.Count == 0
                ? null
                : lvTemplates.SelectedItems[0].Tag as WorkflowTemplateResult;

        /// <summary>Draw semi-transparent status badge — wfe-form-ui.md §8</summary>
        private void DrawStatusBadge(Graphics g, Rectangle r, string status)
        {
            Color c;
            if (!StatusColors.TryGetValue(status, out c)) c = Color.FromArgb(120, 130, 150);

            var sz = g.MeasureString(status, new Font("Segoe UI", 8.5F, FontStyle.Bold));
            int bw = (int)sz.Width + 18, bh = 20;
            var br = new Rectangle(r.X + 10, r.Y + (r.Height - bh) / 2, bw, bh);

            using (var bg   = new SolidBrush(Color.FromArgb(30, c)))
            using (var path = RoundedRect(br, 4))
                g.FillPath(bg, path);

            using (var pen  = new Pen(Color.FromArgb(80, c)))
            using (var path = RoundedRect(br, 4))
                g.DrawPath(pen, path);

            g.DrawString(status,
                new Font("Segoe UI", 8.5F, FontStyle.Bold),
                new SolidBrush(c),
                new RectangleF(br.X, br.Y, br.Width, br.Height),
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private static RectangleF PadRect(Rectangle r, int hPad, int vPad)
            => new RectangleF(r.X + hPad, r.Y + vPad, r.Width - hPad * 2, r.Height - vPad * 2);

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X,          r.Y,          d, d, 180, 90);
            path.AddArc(r.Right - d,  r.Y,          d, d, 270, 90);
            path.AddArc(r.Right - d,  r.Bottom - d, d, d,   0, 90);
            path.AddArc(r.X,          r.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
