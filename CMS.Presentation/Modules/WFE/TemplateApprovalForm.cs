using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
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
    public partial class TemplateApprovalForm : Form, ITemplateApprovalView
    {
        // ── Dependencies ─────────────────────────────────────────────────────────
        private readonly TemplateApprovalPresenter _presenter;
        private readonly UserContext _userContext;
        private readonly IWorkflowTemplateService _templateService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;

        // ── State ────────────────────────────────────────────────────────────────
        private readonly List<WorkflowTemplateResult> _currentItems = new List<WorkflowTemplateResult>();
        private int _totalCount;
        private bool _searchPlaceholderActive = true;
        private Panel _activeNavItem;

        // ── View mode ────────────────────────────────────────────────────────────
        private Panel _pnlDetailView;
        private WorkflowTemplateResult _reviewItem;
        private int _queueIndex;      // 0-based index in _currentItems
        private int _queueTotal;
        private int _hotItemIndex = -1;

        // ── Colors ───────────────────────────────────────────────────────────────
        private static readonly Color Blue       = Color.FromArgb(37,  99,  235);
        private static readonly Color NavyDark   = Color.FromArgb(31,  56,  100);
        private static readonly Color GreenOk    = Color.FromArgb(21,  128,  61);
        private static readonly Color OrangeWarn = Color.FromArgb(146,  64,  14);
        private static readonly Color RedDanger  = Color.FromArgb(185,  28,  28);
        private static readonly Color GrayText   = Color.FromArgb( 80,  90, 110);

        private static readonly Dictionary<string, Color> StatusColors = new Dictionary<string, Color>
        {
            { "Draft",    Color.FromArgb(197, 120, 16) },
            { "Active",   Color.FromArgb(23,  133, 49) },
            { "Inactive", Color.FromArgb(120, 130, 150) },
        };

        // ════════════════════════════════════════════════════════════════════════
        // Constructor
        // ════════════════════════════════════════════════════════════════════════

        public TemplateApprovalForm(
            IWorkflowTemplateService templateService,
            IAuditService auditService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            InitializeComponent();
            _userContext = userContext;
            _templateService = templateService;
            _auditService = auditService;
            _loggingService = loggingService;
            if (templateService != null && auditService != null && loggingService != null && userContext != null)
                _presenter = new TemplateApprovalPresenter(this, templateService, auditService, loggingService, userContext);
        }

        // ════════════════════════════════════════════════════════════════════════
        // Form Load
        // ════════════════════════════════════════════════════════════════════════

        private void TemplateApprovalForm_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            try
            {
                BuildRibbonTabs();
                BuildToolbar();
                Panel initialNav;
                int pendingCount = _presenter != null ? GetPendingCountSafe() : 0;
                WorkflowNavigationBuilder.BuildSidebar(pnlSidebarContainer, "approval", OnNavItemClick, out initialNav,
                    approvalBadge: pendingCount);
                _activeNavItem = initialNav;
                BuildStatusBar();
                SetupListView();
                SetupFilterBar();
                BuildSummaryCards(0, 0, 0);
                lvTemplates.DoubleClick += (s, ev) => OpenDetailView();

                if (_presenter != null)
                    _presenter.LoadPendingTemplates();
                else
                    LoadMockApprovalData();
            }
            catch (Exception ex) { ShowError("Error loading form: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════════════════════════
        // Mock Data Fallback
        // ════════════════════════════════════════════════════════════════════════

        private void LoadMockApprovalData()
        {
            var now = DateTime.Now;
            var mock = new List<WorkflowTemplateResult>
            {
                new WorkflowTemplateResult { WorkflowDefinitionId=3,  Name="อนุมัติ KB Article",        Category="Knowledge", StepCount=3, Version=2, IsActive=false, Status="PendingApproval", OwnerDepartment="IT",  CreatedBy="supaph.jan", ModifiedDate=now.AddDays(-2),  ActiveInstanceCount=0 },
                new WorkflowTemplateResult { WorkflowDefinitionId=7,  Name="HR Onboarding Process",      Category="HR",        StepCount=5, Version=3, IsActive=false, Status="PendingApproval", OwnerDepartment="HR",  CreatedBy="hr.admin",   ModifiedDate=now.AddDays(-5),  ActiveInstanceCount=0 },
                new WorkflowTemplateResult { WorkflowDefinitionId=12, Name="ขออนุมัติ Upload ไฟล์ NC",  Category="เอกสาร",    StepCount=2, Version=2, IsActive=false, Status="PendingApproval", OwnerDepartment="Ops", CreatedBy="ops.user",   ModifiedDate=now.AddDays(-1),  ActiveInstanceCount=0 },
            };
            DisplayPendingTemplates(new PagedResult<WorkflowTemplateResult>
            {
                Items = mock, TotalCount = mock.Count, PageNumber = 1, PageSize = 50,
            });
        }

        // ════════════════════════════════════════════════════════════════════════
        // ITemplateApprovalView
        // ════════════════════════════════════════════════════════════════════════

        public void DisplayPendingTemplates(PagedResult<WorkflowTemplateResult> result)
        {
            var items = result?.Items?.ToList() ?? new List<WorkflowTemplateResult>();
            _currentItems.Clear();
            _currentItems.AddRange(items);
            _totalCount = result?.TotalCount ?? items.Count;

            lvTemplates.Items.Clear();
            foreach (var t in items)
            {
                var lvi = new ListViewItem(string.Format("WFD-{0:D3}", t.WorkflowDefinitionId));
                lvi.SubItems.Add(t.Name ?? "-");
                lvi.SubItems.Add(t.Category ?? "-");
                lvi.SubItems.Add(string.Format("v{0} {1}", t.Version, GetTemplateStatusLabel(t)));
                lvi.SubItems.Add(ExtractName(t.CreatedBy));
                lvi.SubItems.Add(t.ModifiedDate.ToString("dd/MM/yy") + "  ·  รอ " + (int)(DateTime.Now - t.ModifiedDate).TotalDays + " วัน");
                lvi.Tag = t;
                lvTemplates.Items.Add(lvi);
            }

            lblResultCount.Text = string.Format("พบ {0} รายการรออนุมัติ", _totalCount);
            lblSelectionInfo.Text = string.Format("แสดง {0} รายการ", items.Count);
            int pendingRows = items.Count(IsPendingTemplate);
            lblResultCount.Text = pendingRows == items.Count
                ? string.Format("พบ {0} รายการรออนุมัติ", _totalCount)
                : string.Format("ไม่มีรายการรออนุมัติ - แสดง Template ทั้งหมด {0} รายการ", _totalCount);
            BuildSummaryCards(pendingRows, 0, 0);
            SetActionButtonsEnabled(false, false, false);
        }

        public void SetActionButtonsEnabled(bool canApprove, bool canReject, bool canPreview)
        {
            foreach (Control c in pnlToolbarContainer.Controls)
            {
                if (!(c is Panel pnl)) continue;
                var tag = pnl.Tag as string ?? string.Empty;
                if (tag == "approve") pnl.Enabled = canApprove;
                else if (tag == "reject") pnl.Enabled = canReject;
                else if (tag == "preview") pnl.Enabled = canPreview;
            }
        }

        public void UpdatePendingCount(int count) =>
            lblSubtitle.Text = string.Format("รายการ Template ที่รออนุมัติ ({0} รายการ)", count);

        public void ShowError(string msg) =>
            MessageBox.Show(msg, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void ShowInfo(string msg) =>
            MessageBox.Show(msg, "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public string GetSearchFilter() => _searchPlaceholderActive ? string.Empty : txtSearch.Text.Trim();
        public string GetCategoryFilter()
        {
            var sel = cmbCategory.SelectedItem as string;
            return string.Equals(sel, "ทั้งหมด", StringComparison.Ordinal) ? null : sel;
        }

        // ════════════════════════════════════════════════════════════════════════
        // Ribbon Tabs
        // ════════════════════════════════════════════════════════════════════════

        private void BuildRibbonTabs()
        {
            var tabs = new[] { "แฟ้ม", "หน้าแรก", "ดำเนินการ", "มุมมอง", "เครื่องมือ", "ช่วยเหลือ" };
            int x = 8;
            foreach (var text in tabs)
            {
                bool active = text == "ดำเนินการ";
                var btn = new Button
                {
                    Text = text, Location = new Point(x, 0),
                    Size = new Size(MeasureText(text, new Font("Segoe UI", 9.5F)) + 24, 32),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent,
                    ForeColor = active ? Color.White : Color.FromArgb(190, 210, 245),
                    Font = active ? new Font("Segoe UI", 9.5F, FontStyle.Bold) : new Font("Segoe UI", 9.5F),
                    Cursor = Cursors.Hand, TabStop = false,
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 80, 130);
                if (active)
                    btn.Paint += (s, pe) => pe.Graphics.FillRectangle(Brushes.White, 0, btn.Height - 3, btn.Width, 3);
                pnlRibbonTabs.Controls.Add(btn);
                x += btn.Width + 2;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // Toolbar (changes based on view mode)
        // ════════════════════════════════════════════════════════════════════════

        private void BuildToolbar()
        {
            pnlToolbarContainer.Controls.Clear();
            var groups = new[]
            {
                new { Header = "การอนุมัติ", Items = new[]
                {
                    new { Icon = "✔", Text = "อนุมัติ Template", Tag = "approve", Primary = true  },
                    new { Icon = "✎", Text = "แก้ไขก่อน",       Tag = "edit",    Primary = false },
                    new { Icon = "✖", Text = "ปฏิเสธ",           Tag = "reject",  Primary = false },
                }},
                new { Header = "เปรียบเทียบ", Items = new[]
                {
                    new { Icon = "⇔", Text = "Diff กับ v1",      Tag = "diff",    Primary = false },
                    new { Icon = "▶", Text = "Preview Run",       Tag = "preview", Primary = false },
                }},
                new { Header = "อ้างอิง", Items = new[]
                {
                    new { Icon = "📄", Text = "เอกสารอ้างอิง",   Tag = "docs",    Primary = false },
                    new { Icon = "🕐", Text = "ประวัติแก้ไข",     Tag = "history", Primary = false },
                    new { Icon = "🔄", Text = "รีเฟรช",           Tag = "refresh", Primary = false },
                }},
            };

            int x = 8;
            var blue = Color.FromArgb(37, 99, 235);
            foreach (var group in groups)
            {
                int gx = x;
                foreach (var item in group.Items)
                {
                    var tag = item.Tag;
                    bool primary = item.Primary;
                    var bg = primary ? blue : Color.White;
                    var fg = primary ? Color.White : NavyDark;

                    var pnlBtn = new Panel { Location = new Point(x, 2), Size = new Size(62, 66), BackColor = bg, Cursor = Cursors.Hand, Tag = tag };
                    var lblI   = new Label { Text = item.Icon, Font = new Font("Segoe UI", 13F), ForeColor = fg, BackColor = bg, Location = new Point(0, 6), Size = new Size(62, 28), TextAlign = ContentAlignment.MiddleCenter };
                    var lblT   = new Label { Text = item.Text, Font = new Font("Segoe UI", 7.5F), ForeColor = fg, BackColor = bg, Location = new Point(0, 36), Size = new Size(62, 22), TextAlign = ContentAlignment.MiddleCenter };
                    pnlBtn.Controls.Add(lblI); pnlBtn.Controls.Add(lblT);

                    if (!primary)
                    {
                        var hoverBg = Color.FromArgb(243, 246, 252);
                        EventHandler enter = (s, ev) => { pnlBtn.BackColor = lblI.BackColor = lblT.BackColor = hoverBg; };
                        EventHandler leave = (s, ev) => { pnlBtn.BackColor = lblI.BackColor = lblT.BackColor = Color.White; };
                        pnlBtn.MouseEnter += enter; lblI.MouseEnter += enter; lblT.MouseEnter += enter;
                        pnlBtn.MouseLeave += leave; lblI.MouseLeave += leave; lblT.MouseLeave += leave;
                    }
                    EventHandler click = (s, ev) => OnToolbarClick(tag);
                    pnlBtn.Click += click; lblI.Click += click; lblT.Click += click;
                    pnlToolbarContainer.Controls.Add(pnlBtn);
                    x += 64;
                }

                pnlToolbarContainer.Controls.Add(new Label
                {
                    Text = group.Header, Font = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(120, 130, 150), TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(gx, 70), Size = new Size(x - gx, 10),
                });
                if (group != groups[groups.Length - 1])
                {
                    pnlToolbarContainer.Controls.Add(new Panel { BackColor = Color.FromArgb(220, 225, 235), Location = new Point(x + 2, 8), Size = new Size(1, 58) });
                    x += 10;
                }
            }
            pnlToolbarContainer.Controls.Add(new Panel { BackColor = Color.FromArgb(220, 225, 235), Dock = DockStyle.Bottom, Height = 1 });
        }

        private void OnToolbarClick(string tag)
        {
            switch (tag)
            {
                case "approve": DoApprove(); break;
                case "reject":  DoReject();  break;
                case "edit":    ShowInfo("แก้ไขก่อน: จะส่งกลับให้ผู้ส่งแก้ไขก่อน Publish"); break;
                case "diff":    ShowInfo("Diff กับ v1: อยู่ระหว่างพัฒนา"); break;
                case "preview": ShowInfo("Preview Run: อยู่ระหว่างพัฒนา"); break;
                case "docs":    ShowInfo("เอกสารอ้างอิง: อยู่ระหว่างพัฒนา"); break;
                case "history": ShowInfo("ประวัติการแก้ไข: อยู่ระหว่างพัฒนา"); break;
                case "refresh":
                    if (_reviewer != null && _pnlDetailView != null && _pnlDetailView.Visible) break;
                    if (_presenter != null) _presenter.LoadPendingTemplates(); else LoadMockApprovalData(); break;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // Status Bar
        // ════════════════════════════════════════════════════════════════════════

        private void BuildStatusBar()
        {
            pnlStatusBar.Paint += (s, pe) =>
                pe.Graphics.DrawLine(new Pen(Color.FromArgb(210, 215, 225)), 0, 0, pnlStatusBar.Width, 0);
            var items = new[]
            {
                "  พร้อมใช้งาน  ",
                string.Format("  เชื่อมต่อ: CMS-DB01  "),
                string.Format("  โดย: {0}  ", _userContext?.ADUserName ?? ""),
                "  Zoom 100%  ",
                "  TH  ",
                "  ออนไลน์  ",
            };
            int x = 8;
            foreach (var text in items)
            {
                var lbl = new Label { Text = text, Font = new Font("Segoe UI", 7.5F), ForeColor = GrayText, AutoSize = true, Location = new Point(x, 4) };
                pnlStatusBar.Controls.Add(lbl);
                x += lbl.PreferredWidth + 4;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // List View Setup (Queue List)
        // ════════════════════════════════════════════════════════════════════════

        private void SetupListView()
        {
            var img = new ImageList { ImageSize = new Size(1, 50) };
            lvTemplates.SmallImageList = img;
            lvTemplates.Columns.Add("ID",             80);
            lvTemplates.Columns.Add("ชื่อ Template",  260);
            lvTemplates.Columns.Add("หมวดหมู่",       100);
            lvTemplates.Columns.Add("เวอร์ชั่น",       90);
            lvTemplates.Columns.Add("ผู้ส่ง",          150);
            lvTemplates.Columns.Add("ส่งเมื่อ / รอ",  200);

            typeof(ListView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, lvTemplates, new object[] { true });

            lvTemplates.DrawColumnHeader     += (s, e) => DrawColHeader(e);
            lvTemplates.DrawItem             += (s, e) => { /* background drawn per-cell in DrawSubItem */ };
            lvTemplates.DrawSubItem          += (s, e) => DrawListSubItem(e);

            lvTemplates.MouseMove += (s, e) =>
            {
                int idx = lvTemplates.HitTest(e.Location).Item?.Index ?? -1;
                if (idx == _hotItemIndex) return;
                int old = _hotItemIndex;
                _hotItemIndex = idx;
                InvalidateTemplateListRow(old);
                InvalidateTemplateListRow(idx);
            };
            lvTemplates.MouseLeave += (s, e) =>
            {
                if (_hotItemIndex == -1) return;
                int old = _hotItemIndex;
                _hotItemIndex = -1;
                InvalidateTemplateListRow(old);
            };

            lvTemplates.SelectedIndexChanged += (s, e) =>
            {
                bool has = lvTemplates.SelectedItems.Count > 0;
                var selected = GetSelectedListTemplate();
                bool canReview = has && IsPendingTemplate(selected);
                SetActionButtonsEnabled(canReview, canReview, has);
                int sel = lvTemplates.SelectedItems.Count;
                lblSelectionInfo.Text = sel > 0
                    ? string.Format("เลือก {0} รายการจาก {1}", sel, _totalCount)
                    : string.Format("แสดง {0} รายการ", _currentItems.Count);
            };
        }

        private void DrawColHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(246, 248, 251)), e.Bounds);
            e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 224, 232)), e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            using (var f = new Font("Segoe UI", 9F, FontStyle.Bold))
            using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                e.Graphics.DrawString(e.Header.Text, f, new SolidBrush(Color.FromArgb(55, 65, 90)),
                    new RectangleF(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 14, e.Bounds.Height), sf);
        }

        private void DrawListSubItem(DrawListViewSubItemEventArgs e)
        {
            if (e.Item == null || e.Bounds.Width == 0) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = e.Bounds;

            // Each cell fills its own background so hover/hot repaints can't leave text on wrong bg
            var bg = e.Item.Selected
                ? Color.FromArgb(232, 241, 255)
                : e.ItemIndex == _hotItemIndex ? Color.FromArgb(240, 245, 255)
                : (e.ItemIndex % 2 == 0 ? Color.White : Color.FromArgb(250, 251, 253));
            using (var br = new SolidBrush(bg))
                g.FillRectangle(br, r);

            // Bottom separator line
            using (var p = new Pen(Color.FromArgb(235, 238, 244)))
                g.DrawLine(p, r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);

            var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
            switch (e.ColumnIndex)
            {
                case 0:
                    using (var f = new Font("Segoe UI", 9F))
                    using (var br = new SolidBrush(Blue))
                        g.DrawString(e.SubItem.Text, f, br, PadRect(r, 10, 0), sf);
                    break;
                case 1:
                    using (var f = new Font("Segoe UI", 9.5F, FontStyle.Bold))
                    using (var br = new SolidBrush(Color.FromArgb(30, 36, 55)))
                        g.DrawString(e.SubItem.Text, f, br, PadRect(r, 10, 0), sf);
                    break;
                default:
                    using (var f = new Font("Segoe UI", 9F))
                    using (var br = new SolidBrush(Color.FromArgb(55, 65, 90)))
                        g.DrawString(e.SubItem.Text, f, br, PadRect(r, 10, 0), sf);
                    break;
            }
        }

        private void SetupFilterBar()
        {
            cmbCategory.Items.Clear();
            foreach (var s in new[] { "ทั้งหมด", "เอกสาร", "IT", "Knowledge", "HR" })
                cmbCategory.Items.Add(s);
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += (s, e) => { if (_presenter != null) _presenter.Search(); };

            txtSearch.Enter += (s, e) =>
            {
                if (_searchPlaceholderActive) { _searchPlaceholderActive = false; txtSearch.Text = string.Empty; txtSearch.ForeColor = Color.FromArgb(30, 36, 55); }
            };
            txtSearch.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text)) { _searchPlaceholderActive = true; txtSearch.Text = "ค้นหาชื่อ Template..."; txtSearch.ForeColor = Color.FromArgb(140, 150, 165); }
            };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter && _presenter != null) _presenter.Search(); };
            btnSearch.Click += (s, e) => { if (_presenter != null) _presenter.Search(); };
            btnClearFilter.Click += (s, e) =>
            {
                cmbCategory.SelectedIndex = 0; txtSearch.Text = "ค้นหาชื่อ Template...";
                _searchPlaceholderActive = true; txtSearch.ForeColor = Color.FromArgb(140, 150, 165);
                if (_presenter != null) _presenter.LoadPendingTemplates(); else LoadMockApprovalData();
            };
            btnPrev.Click += (s, e) => { if (_presenter != null) _presenter.PreviousPage(); };
            btnNext.Click += (s, e) => { if (_presenter != null) _presenter.NextPage(); };
        }

        public void BuildSummaryCards(int pending, int approvedToday, int rejectedToday)
        {
            flpCards.Controls.Clear();
            flpCards.Controls.Add(MakeCard("⏳", "รออนุมัติ",     pending,       Color.FromArgb(197, 120, 16)));
            flpCards.Controls.Add(MakeCard("✔",  "อนุมัติวันนี้", approvedToday, GreenOk));
            flpCards.Controls.Add(MakeCard("✖",  "ปฏิเสธวันนี้",  rejectedToday, Color.FromArgb(194, 38, 54)));
        }

        private Panel MakeCard(string icon, string title, int count, Color color)
        {
            var pnl = new Panel { Size = new Size(200, 82), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 10, 0) };
            pnl.Controls.Add(new Label { Text = icon,             Font = new Font("Segoe UI", 14F), ForeColor = color,                            AutoSize = true, Location = new Point(14, 8)  });
            pnl.Controls.Add(new Label { Text = title,            Font = new Font("Segoe UI", 9.5F),ForeColor = GrayText,                         AutoSize = true, Location = new Point(40, 12) });
            pnl.Controls.Add(new Label { Text = count.ToString(), Font = new Font("Segoe UI", 22F, FontStyle.Bold), ForeColor = color,            AutoSize = true, Location = new Point(14, 28) });
            return pnl;
        }

        // ════════════════════════════════════════════════════════════════════════
        // DETAIL VIEW — matches Image 1
        // ════════════════════════════════════════════════════════════════════════

        private object _reviewer; // placeholder flag

        private void OpenDetailView()
        {
            if (lvTemplates.SelectedItems.Count == 0) return;
            _reviewItem = lvTemplates.SelectedItems[0].Tag as WorkflowTemplateResult;
            if (_reviewItem == null) return;
            _queueIndex = _currentItems.IndexOf(_reviewItem);
            _queueTotal = _currentItems.Count;

            pnlDashboard.Visible = false;
            BuildDetailView();
            _reviewer = new object();
        }

        private void CloseDetailView()
        {
            if (_pnlDetailView != null) { _pnlDetailView.Dispose(); _pnlDetailView = null; }
            _reviewer = null;
            pnlDashboard.Visible = true;
            lblTitle.Text    = "☑  อนุมัติ Workflow Template";
            lblSubtitle.Text = string.Format("รายการ Template ที่รออนุมัติ ({0} รายการ)", _totalCount);
            lblBreadcrumb.Text = "CMS  ›  Workflow Engine  ›  อนุมัติ Template";
        }

        private void BuildDetailView()
        {
            if (_pnlDetailView != null) { _pnlDetailView.Dispose(); _pnlDetailView = null; }

            var t = _reviewItem;
            var wfdId = string.Format("WFD-{0:D3}", t.WorkflowDefinitionId);

            // Update chrome
            lblTitle.Text    = string.Format("อนุมัติ Template — {0} v{1}", wfdId, t.Version);
            lblSubtitle.Text = "รอการอนุมัติจาก Workflow Owner Committee ก่อน Publish เป็นเวอร์ชันใช้งานจริง";
            lblBreadcrumb.Text = string.Format("CMS  ›  Workflow Engine  ›  Approval Queue  ›  {0} v{1}", wfdId, t.Version);

            // Root detail panel fills pnlContent
            _pnlDetailView = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(243, 246, 251) };
            pnlContent.Controls.Add(_pnlDetailView);
            _pnlDetailView.BringToFront();

            // ── Stepper (top) ────────────────────────────────────────────────
            var pnlStepper = BuildStepper();
            pnlStepper.Dock = DockStyle.Top;
            _pnlDetailView.Controls.Add(pnlStepper);

            // ── Body: left + right ───────────────────────────────────────────
            var pnlBody = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(243, 246, 251), Padding = new Padding(16, 10, 16, 10) };
            _pnlDetailView.Controls.Add(pnlBody);
            _pnlDetailView.Controls.SetChildIndex(pnlBody, 1);

            // Right panel (approval actions + committee + validation)
            var pnlRight = BuildRightPanel(t);
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Width = 318;
            pnlBody.Controls.Add(pnlRight);

            // Left scroll panel
            var pnlLeftScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(243, 246, 251), Padding = new Padding(0, 0, 12, 0) };
            pnlBody.Controls.Add(pnlLeftScroll);

            // Queue header inside left scroll
            var pnlQueueHdr = BuildQueueHeader(t, wfdId);
            pnlQueueHdr.Dock = DockStyle.Top;
            pnlLeftScroll.Controls.Add(pnlQueueHdr);

            // Info card
            var pnlInfo = BuildInfoCard(t, wfdId);
            pnlInfo.Dock = DockStyle.Top;
            pnlLeftScroll.Controls.Add(pnlInfo);

            // Change summary
            var pnlChanges = BuildChangeSummary(t);
            pnlChanges.Dock = DockStyle.Top;
            pnlLeftScroll.Controls.Add(pnlChanges);

            // Flow comparison placeholder
            var pnlFlow = BuildFlowSection();
            pnlFlow.Dock = DockStyle.Top;
            pnlLeftScroll.Controls.Add(pnlFlow);
        }

        // ── Stepper ──────────────────────────────────────────────────────────────

        private Panel BuildStepper()
        {
            var pnl = new Panel { Height = 56, BackColor = Color.White };
            pnl.Paint += (s, pe) =>
                pe.Graphics.DrawLine(new Pen(Color.FromArgb(225, 228, 238)), 0, pnl.Height - 1, pnl.Width, pnl.Height - 1);

            var steps = new[]
            {
                new { Num = "1", Text = "ข้อมูลพื้นฐาน",    Sub = "ชื่อ · หมวดหมู่ · เจ้าของ",  Done = true,  Active = false },
                new { Num = "2", Text = "ออกแบบ Flow",       Sub = "Designer · Steps · เงื่อนไข", Done = true,  Active = false },
                new { Num = "3", Text = "ออกแบบฟอร์ม",      Sub = "Form Builder · ข้อมูลกรอก",   Done = true,  Active = false },
                new { Num = "4", Text = "ส่งอนุมัติ/Publish", Sub = "Validate · Approve · เผยแพร่", Done = false, Active = true  },
            };

            int x = 24;
            foreach (var step in steps)
            {
                int sx = x;
                bool done = step.Done, active = step.Active;

                // Circle
                var circle = new Panel { Location = new Point(sx, 10), Size = new Size(28, 28), BackColor = Color.Transparent };
                circle.Paint += (s, pe) =>
                {
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    Color fill = done ? GreenOk : active ? Blue : Color.FromArgb(209, 213, 219);
                    using (var b = new SolidBrush(fill))
                        pe.Graphics.FillEllipse(b, 0, 0, 27, 27);
                    string label = done ? "✓" : step.Num;
                    using (var f = new Font("Segoe UI", 9F, done || active ? FontStyle.Bold : FontStyle.Regular))
                    using (var b = new SolidBrush(Color.White))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        pe.Graphics.DrawString(label, f, b, new RectangleF(0, 0, 28, 28), sf);
                };
                pnl.Controls.Add(circle);

                // Labels
                var lblName = new Label { Text = step.Text, Font = new Font("Segoe UI", 8.5F, active ? FontStyle.Bold : FontStyle.Regular), ForeColor = active ? Blue : done ? Color.FromArgb(30, 36, 55) : Color.FromArgb(150, 155, 165), AutoSize = true, Location = new Point(sx + 34, 10) };
                var lblSub  = new Label { Text = step.Sub,  Font = new Font("Segoe UI", 7.5F), ForeColor = Color.FromArgb(120, 130, 145), AutoSize = true, Location = new Point(sx + 34, 28) };
                pnl.Controls.Add(lblName);
                pnl.Controls.Add(lblSub);

                // Connector line
                if (step.Num != "4")
                {
                    int connX = sx + 28 + (int)CreateGraphics().MeasureString(step.Text, lblName.Font).Width + 40;
                    pnl.Paint += (s, pe) =>
                        pe.Graphics.DrawLine(new Pen(Color.FromArgb(209, 213, 219), 1), connX, 24, connX + 30, 24);
                }

                x += 220;
            }

            return pnl;
        }

        // ── Queue header (status badge + back) ──────────────────────────────────

        private Panel BuildQueueHeader(WorkflowTemplateResult t, string wfdId)
        {
            var pnl = new Panel { Height = 38, BackColor = Color.Transparent, Margin = new Padding(0, 0, 0, 6) };

            // Status badge
            var badge = new Label
            {
                Text = string.Format("● รอตรวจสอบ  ·  คิวที่ {0}/{1}", _queueIndex + 1, _queueTotal),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(146, 64, 14),
                BackColor = Color.FromArgb(254, 243, 199),
                AutoSize = true,
                Location = new Point(0, 8),
                Padding = new Padding(8, 3, 8, 3),
            };
            pnl.Controls.Add(badge);

            // Back button
            var btnBack = new Button
            {
                Text = "← กลับคิว",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Blue,
                BackColor = Color.White,
                AutoSize = true,
                Location = new Point(badge.PreferredWidth + 32, 5),
                Cursor = Cursors.Hand,
            };
            btnBack.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnBack.Click += (s, e) => CloseDetailView();
            pnl.Controls.Add(btnBack);

            return pnl;
        }

        // ── Info card ────────────────────────────────────────────────────────────

        private Panel BuildInfoCard(WorkflowTemplateResult t, string wfdId)
        {
            var card = MakeCard();
            card.Margin = new Padding(0, 0, 0, 10);

            // Card header row
            var hdr = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.White };
            var lblHdr = new Label { Text = "ข้อมูลการส่งเพื่ออนุมัติ", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(22, 36, 62), AutoSize = true, Location = new Point(16, 9) };
            var badgeDraft = new Label { Text = "● Draft v" + t.Version + " · Base v" + (t.Version - 1) + " Published", Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = Blue, BackColor = Color.FromArgb(219, 234, 254), AutoSize = true, Padding = new Padding(6, 2, 6, 2), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            badgeDraft.Location = new Point(card.Width - badgeDraft.PreferredWidth - 20, 8);
            hdr.Controls.Add(lblHdr);
            hdr.Controls.Add(badgeDraft);
            hdr.Paint += (s, pe) => pe.Graphics.DrawLine(new Pen(Color.FromArgb(235, 238, 244)), 0, hdr.Height - 1, hdr.Width, hdr.Height - 1);
            card.Controls.Add(hdr);

            // Fields grid
            var days = (int)(DateTime.Now - t.ModifiedDate).TotalDays;
            var submitterName = ExtractFullName(t.CreatedBy);
            var rows = new[]
            {
                new { Label = "Template",      Value = string.Format("{0} · {1}", wfdId, t.Name) },
                new { Label = "เวอร์ชั่น",    Value = string.Format("v{0} Draft  →  v{1} Active", t.Version, t.Version - 1) },
                new { Label = "ผู้ส่ง",        Value = string.Format("{0}  ·  Workflow Designer · {1}", submitterName, t.Category ?? "Unknown") },
                new { Label = "ส่งเมื่อ",      Value = string.Format("{0}  ·  รอมาแล้ว {1} วัน", t.ModifiedDate.ToString("dd/MM/yyyy HH:mm"), days) },
                new { Label = "เหตุผลที่ทำ",  Value = "ปรับให้สอดคล้อง PDPA 2566 + เพิ่ม Peer Review สำหรับงานเชิงเทคนิค" },
                new { Label = "Impact",        Value = "Medium  ·  กระทบ Instance ใหม่ 24 รายการ/เดือน  ·  ไม่กระทบของเดิม (Frozen)" },
            };

            var grid = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = rows.Length, BackColor = Color.White, Padding = new Padding(16, 8, 16, 12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < rows.Length; i++)
            {
                grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                grid.Controls.Add(new Label { Text = rows[i].Label, Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(100, 110, 130), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Left, Padding = new Padding(0, 4, 8, 4) }, 0, i);
                grid.Controls.Add(new Label { Text = rows[i].Value, Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(22, 36, 62), AutoSize = false, Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 4) }, 1, i);
            }
            card.Controls.Add(grid);

            card.Height = 34 + rows.Length * 26 + 20;
            return card;
        }

        // ── Change summary ───────────────────────────────────────────────────────

        private Panel BuildChangeSummary(WorkflowTemplateResult t)
        {
            var card = MakeCard();
            card.Margin = new Padding(0, 0, 0, 10);

            // Header
            var hdr = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.White };
            hdr.Controls.Add(new Label { Text = "สรุปการเปลี่ยนแปลงจาก v" + (t.Version - 1), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(22, 36, 62), AutoSize = true, Location = new Point(16, 9) });
            var badges = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.White, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            badges.Location = new Point(240, 6);
            badges.Controls.Add(MakePill("2 เพิ่ม", GreenOk));
            badges.Controls.Add(MakePill("2 แก้",   Color.FromArgb(161, 98,  7)));
            badges.Controls.Add(MakePill("1 ลบ",    Color.FromArgb(185, 28, 28)));
            hdr.Controls.Add(badges);
            hdr.Paint += (s, pe) => pe.Graphics.DrawLine(new Pen(Color.FromArgb(235, 238, 244)), 0, hdr.Height - 1, hdr.Width, hdr.Height - 1);
            card.Controls.Add(hdr);

            // Change items
            var changes = new[]
            {
                new { Type = "เพิ่ม", Desc = "เพิ่ม Step \"Peer Review\" (parallel กับ Manager อนุมัติ)",            Color = Color.FromArgb(220, 252, 231), Fg = GreenOk   },
                new { Type = "เพิ่ม", Desc = "เพิ่มเงื่อนไข CRAF = 1HC/2SC → บังคับผ่าน Legal",                    Color = Color.FromArgb(220, 252, 231), Fg = GreenOk   },
                new { Type = "แก้",   Desc = "เปลี่ยน SLA ของ Step \"Manager อนุมัติ\" จาก 5 วัน → 3 วัน",         Color = Color.FromArgb(255, 237, 213), Fg = Color.FromArgb(154, 52, 18) },
                new { Type = "แก้",   Desc = "เปลี่ยน Assignee ของ \"Legal ตรวจ\" จาก User → AD Group: CMS-Legal-Managers", Color = Color.FromArgb(255, 237, 213), Fg = Color.FromArgb(154, 52, 18) },
                new { Type = "ลบ",   Desc = "ลบ Step \"Secondary Review\" (เดิมเป็น optional)",                     Color = Color.FromArgb(254, 226, 226), Fg = Color.FromArgb(185, 28, 28) },
            };

            var stack = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = Color.White, Padding = new Padding(16, 8, 16, 12) };
            foreach (var ch in changes)
            {
                var row = new Panel { Height = 36, BackColor = ch.Color, Margin = new Padding(0, 0, 0, 4) };
                row.Width = 600; // will auto-resize
                var pill = MakePill(ch.Type, ch.Fg);
                pill.Location = new Point(10, 10);
                var lbl = new Label { Text = ch.Desc, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(22, 36, 62), AutoSize = true, Location = new Point(62, 11) };
                row.Controls.Add(pill); row.Controls.Add(lbl);
                stack.Controls.Add(row);
            }
            card.Controls.Add(stack);

            card.Height = 34 + changes.Length * 40 + 28;
            return card;
        }

        // ── Flow section ─────────────────────────────────────────────────────────

        private Panel BuildFlowSection()
        {
            var card = MakeCard();
            card.Margin = new Padding(0, 0, 0, 10);

            var hdr = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.White };
            hdr.Controls.Add(new Label
            {
                Text = "Flow ของ Template v" + _reviewItem.Version,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 36, 62),
                AutoSize = true,
                Location = new Point(16, 9)
            });
            hdr.Paint += (s, pe) =>
                pe.Graphics.DrawLine(new Pen(Color.FromArgb(235, 238, 244)), 0, hdr.Height - 1, hdr.Width, hdr.Height - 1);
            card.Controls.Add(hdr);

            // Load steps from DB
            var steps = new List<CMS.Domain.Models.WorkflowStep>();
            var connections = new List<CMS.Domain.Models.WorkflowConnection>();
            try
            {
                var cfg = new CMS.Infrastructure.ConfigManager();
                var stepRepo = new CMS.Data.Repositories.WorkflowStepRepository(cfg);
                steps = stepRepo.GetStepsByTemplate(_reviewItem.WorkflowDefinitionId)?.ToList()
                        ?? new List<CMS.Domain.Models.WorkflowStep>();
                connections = stepRepo.GetConnectionsByTemplate(_reviewItem.WorkflowDefinitionId)?.ToList()
                              ?? new List<CMS.Domain.Models.WorkflowConnection>();
            }
            catch { /* DB not available — canvas will show placeholder text */ }

            // Pre-compute node positions
            var nodeBounds = new Dictionary<int, Rectangle>();
            const int nodeW = 110, nodeH = 42, xSpacing = 150, ySpacing = 26, yMid = 90;
            if (steps.Count > 0)
            {
                var grouped = steps.Where(s => !s.IsDeleted)
                    .GroupBy(s => s.SequenceNumber)
                    .OrderBy(g => g.Key).ToList();
                for (int ci = 0; ci < grouped.Count; ci++)
                {
                    var group = grouped[ci].ToList();
                    int x = 30 + ci * xSpacing;
                    int k = group.Count;
                    int totalH = k * nodeH + (k - 1) * ySpacing;
                    int startY = yMid - totalH / 2;
                    for (int ri = 0; ri < k; ri++)
                        nodeBounds[group[ri].WorkflowStepId] = new Rectangle(x, startY + ri * (nodeH + ySpacing), nodeW, nodeH);
                }
            }

            int canvasH = steps.Count > 0 ? 200 : 60;
            var canvas = new Panel { Dock = DockStyle.Top, Height = canvasH, BackColor = Color.FromArgb(246, 248, 251) };

            var capturedSteps = steps;
            var capturedConns = connections;
            var capturedBounds = nodeBounds;

            canvas.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                if (capturedSteps.Count == 0)
                {
                    using (var f = new Font("Segoe UI", 9F))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString("ไม่พบข้อมูล Step สำหรับ Template นี้", f,
                            new SolidBrush(Color.FromArgb(140, 150, 165)),
                            new RectangleF(0, 0, canvas.Width, canvas.Height), sf);
                    return;
                }

                // Connections
                foreach (var conn in capturedConns)
                {
                    if (conn.IsDeleted) continue;
                    if (capturedBounds.TryGetValue(conn.FromStepId, out var rf) &&
                        capturedBounds.TryGetValue(conn.ToStepId,   out var rt))
                    {
                        int x1 = rf.Right, y1 = rf.Top + rf.Height / 2;
                        int x2 = rt.Left,  y2 = rt.Top  + rt.Height / 2;
                        int cx = (x1 + x2) / 2;
                        using (var pen = new Pen(Color.FromArgb(160, 170, 190), 1.5f) { EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor })
                            g.DrawBezier(pen, x1, y1, cx, y1, cx, y2, x2, y2);
                        if (!string.IsNullOrEmpty(conn.ConditionExpression))
                            using (var f = new Font("Segoe UI", 7F))
                                g.DrawString(conn.ConditionExpression, f, new SolidBrush(Color.FromArgb(100, 110, 130)),
                                    new PointF((x1 + x2) / 2f, (y1 + y2) / 2f - 10));
                    }
                }

                // Nodes
                foreach (var step in capturedSteps)
                {
                    if (step.IsDeleted) continue;
                    if (!capturedBounds.TryGetValue(step.WorkflowStepId, out var rect)) continue;

                    // Shadow
                    using (var shadowBr = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                        g.FillRectangle(shadowBr, rect.X + 2, rect.Y + 2, rect.Width, rect.Height);

                    // Body
                    using (var bodyBr = new SolidBrush(Color.White))
                    using (var path = RoundedRect(rect, 6))
                        g.FillPath(bodyBr, path);

                    // Top accent
                    using (var accentBr = new SolidBrush(NavyDark))
                    {
                        var accentRect = new Rectangle(rect.X, rect.Y, rect.Width, 5);
                        using (var accentPath = RoundedRect(accentRect, 3))
                            g.FillPath(accentBr, accentPath);
                    }

                    // Border
                    using (var borderPen = new Pen(Color.FromArgb(210, 215, 225), 1f))
                    using (var path = RoundedRect(rect, 6))
                        g.DrawPath(borderPen, path);

                    // Label
                    using (var f = new Font("Segoe UI", 8F, FontStyle.Bold))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                        g.DrawString(step.StepName, f, new SolidBrush(Color.FromArgb(30, 40, 65)),
                            new RectangleF(rect.X + 4, rect.Y + 6, rect.Width - 8, rect.Height - 6), sf);
                }
            };

            card.Controls.Add(canvas);
            card.Height = 34 + canvasH;
            return card;
        }

        // ── Right panel (approval + committee + validation) ──────────────────────

        private Panel BuildRightPanel(WorkflowTemplateResult t)
        {
            var pnl = new Panel { BackColor = Color.Transparent, Padding = new Padding(8, 0, 0, 0) };

            // ── Section: การอนุมัติของคุณ ───────────────────────────────────
            var cardAction = MakeCard();
            cardAction.Dock = DockStyle.Top;
            cardAction.Height = 180;
            cardAction.Margin = new Padding(0, 0, 0, 10);

            cardAction.Controls.Add(new Label { Text = "การอนุมัติของคุณ", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(22, 36, 62), AutoSize = true, Location = new Point(14, 12) });

            var lblNote = new Label { Text = "ต้องผ่านอย่างน้อย 2/3 เสียงจึงจะ Publish ได้", Font = new Font("Segoe UI", 8F), ForeColor = Blue, AutoSize = true, Location = new Point(14, 34), MaximumSize = new Size(270, 0) };
            cardAction.Controls.Add(lblNote);

            // Approve button
            var btnApprove = new Button { Text = "✔  อนุมัติ Template นี้", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.White, BackColor = Blue, FlatStyle = FlatStyle.Flat, Location = new Point(14, 58), Size = new Size(270, 34), Cursor = Cursors.Hand };
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.Click += (s, e) => DoApprove();
            cardAction.Controls.Add(btnApprove);

            // Edit link
            var lnkEdit = new LinkLabel { Text = "✎  แก้ไขก่อน", Font = new Font("Segoe UI", 9F), Location = new Point(14, 100), AutoSize = true, LinkColor = GrayText };
            lnkEdit.LinkClicked += (s, e) => ShowInfo("แก้ไขก่อน: จะส่งกลับให้ผู้ส่งแก้ไข");
            cardAction.Controls.Add(lnkEdit);

            // Reject link
            var lnkReject = new LinkLabel { Text = "ปฏิเสธ", Font = new Font("Segoe UI", 9F), Location = new Point(14, 124), AutoSize = true, LinkColor = Color.FromArgb(185, 28, 28) };
            lnkReject.LinkClicked += (s, e) => DoReject();
            cardAction.Controls.Add(lnkReject);

            pnl.Controls.Add(cardAction);

            // ── Section: คณะกรรมการ ────────────────────────────────────────
            var cardCommittee = MakeCard();
            cardCommittee.Dock = DockStyle.Top;
            cardCommittee.Margin = new Padding(0, 0, 0, 10);

            var committee = new[]
            {
                new { Initials = "ปอ", Name = "ปิยะ อุทัย",      Role = "Head of Knowledge", Approved = true,  Time = "15/04 11:20" },
                new { Initials = "อส", Name = "คุณ (อรรถพล ส.)", Role = "Legal Manager",     Approved = false, Time = ""            },
                new { Initials = "ดน", Name = "ดนัย เอกพงษ์",   Role = "IT Director",       Approved = false, Time = ""            },
            };
            int cy = 10;
            cardCommittee.Controls.Add(new Label { Text = "คณะกรรมการ", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(22, 36, 62), AutoSize = true, Location = new Point(14, cy) });
            cy += 26;

            foreach (var m in committee)
            {
                var avColor = _avatarPalette[Math.Abs(m.Name.GetHashCode()) % _avatarPalette.Length];
                var row = new Panel { Location = new Point(14, cy), Size = new Size(270, 32), BackColor = Color.White };

                var av = new Panel { Location = new Point(0, 2), Size = new Size(26, 26), BackColor = Color.Transparent };
                av.Paint += (s, pe) =>
                {
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    pe.Graphics.FillEllipse(new SolidBrush(avColor), 0, 0, 25, 25);
                    using (var f = new Font("Segoe UI", 7.5F, FontStyle.Bold))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        pe.Graphics.DrawString(m.Initials, f, Brushes.White, new RectangleF(0, 0, 26, 26), sf);
                };
                row.Controls.Add(av);
                row.Controls.Add(new Label { Text = m.Name, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(22, 36, 62), AutoSize = true, Location = new Point(32, 2) });
                row.Controls.Add(new Label { Text = m.Role, Font = new Font("Segoe UI", 7.5F), ForeColor = GrayText, AutoSize = true, Location = new Point(32, 16) });

                if (m.Approved)
                {
                    row.Controls.Add(new Label { Text = "● อนุมัติ", Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = GreenOk, AutoSize = true, Location = new Point(180, 4) });
                    row.Controls.Add(new Label { Text = m.Time, Font = new Font("Segoe UI", 7.5F), ForeColor = GrayText, AutoSize = true, Location = new Point(180, 16) });
                }
                else
                {
                    row.Controls.Add(new Label { Text = "● รอ", Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = Color.FromArgb(197, 120, 16), AutoSize = true, Location = new Point(218, 10) });
                }
                cardCommittee.Controls.Add(row);
                cy += 34;
            }
            cardCommittee.Height = cy + 8;
            pnl.Controls.Add(cardCommittee);

            // ── Section: Validation Check ──────────────────────────────────
            var cardValid = MakeCard();
            cardValid.Dock = DockStyle.Top;
            cardValid.Margin = new Padding(0, 0, 0, 10);

            var checks = new[]
            {
                new { Ok = true,  Text = "ไม่มี Step ค้าง (No orphan)"            },
                new { Ok = true,  Text = "ทุก Decision มี Default path"           },
                new { Ok = true,  Text = "SLA รวม ≤ 14 วัน (ผ่าน 8 วัน)"         },
                new { Ok = true,  Text = "Assignee ทุก Step มีอยู่ใน AD"          },
                new { Ok = false, Text = "Preview Run — ยัง 0/3"                  },
            };
            int vcy = 10;
            cardValid.Controls.Add(new Label { Text = "Validation Check", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(22, 36, 62), AutoSize = true, Location = new Point(14, vcy) });
            vcy += 26;
            foreach (var ch in checks)
            {
                cardValid.Controls.Add(new Label
                {
                    Text = (ch.Ok ? "✓ " : "⚠ ") + ch.Text,
                    Font = new Font("Segoe UI", 8.5F),
                    ForeColor = ch.Ok ? GreenOk : Color.FromArgb(197, 120, 16),
                    AutoSize = true,
                    Location = new Point(14, vcy),
                });
                vcy += 20;
            }
            cardValid.Height = vcy + 10;
            pnl.Controls.Add(cardValid);

            pnl.Height = cardAction.Height + cardCommittee.Height + cardValid.Height + 30;
            return pnl;
        }

        // ── Helpers for building card containers ─────────────────────────────────

        private static Panel MakeCard()
        {
            return new Panel
            {
                BackColor = Color.White,
                Margin    = new Padding(0, 0, 0, 10),
                Padding   = new Padding(0),
            };
        }

        private static Label MakePill(string text, Color fg)
        {
            return new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = fg,
                BackColor = Color.FromArgb(30, fg),
                AutoSize  = true,
                Padding   = new Padding(6, 2, 6, 2),
                Margin    = new Padding(0, 0, 4, 0),
            };
        }

        private static readonly Color[] _avatarPalette =
        {
            Color.FromArgb(99,  102, 241), Color.FromArgb(236, 72,  153),
            Color.FromArgb(34,  197,  94), Color.FromArgb(249, 115,  22),
            Color.FromArgb(20,  184, 166), Color.FromArgb(59,  130, 246),
        };

        // ════════════════════════════════════════════════════════════════════════
        // Actions
        // ════════════════════════════════════════════════════════════════════════

        private void DoApprove()
        {
            var t = _reviewItem ?? GetSelectedListTemplate();
            if (t == null) { ShowError("กรุณาเลือก Template ที่ต้องการอนุมัติ"); return; }
            if (!IsPendingTemplate(t)) { ShowError("Template นี้ไม่ได้อยู่ในสถานะ PendingApproval"); return; }
            if (MessageBox.Show(
                    string.Format("ยืนยันอนุมัติ Template '{0}' v{1} ?", t.Name, t.Version),
                    "ยืนยันการอนุมัติ", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes)
            {
                if (_presenter != null) _presenter.ApproveTemplate(t);
                else ShowInfo(string.Format("✔ อนุมัติ '{0}' v{1} เรียบร้อย (Mock)", t.Name, t.Version));
                CloseDetailView();
            }
        }

        private void DoReject()
        {
            var t = _reviewItem ?? GetSelectedListTemplate();
            if (t == null) { ShowError("กรุณาเลือก Template ที่ต้องการปฏิเสธ"); return; }
            if (!IsPendingTemplate(t)) { ShowError("Template นี้ไม่ได้อยู่ในสถานะ PendingApproval"); return; }
            string reason = PromptReason();
            if (reason == null) return;
            if (_presenter != null) _presenter.RejectTemplate(t, reason);
            else ShowInfo(string.Format("✖ ปฏิเสธ '{0}' v{1} เหตุผล: {2} (Mock)", t.Name, t.Version, reason));
            CloseDetailView();
        }

        private WorkflowTemplateResult GetSelectedListTemplate()
            => lvTemplates.SelectedItems.Count > 0 ? lvTemplates.SelectedItems[0].Tag as WorkflowTemplateResult : null;

        private static bool IsPendingTemplate(WorkflowTemplateResult template)
        {
            return template != null
                && (template.ApprovalStatus == WorkflowTemplateStatus.PendingApproval
                    || string.Equals(template.StatusLabel, "PendingApproval", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(template.Status, "PendingApproval", StringComparison.OrdinalIgnoreCase));
        }

        private static string GetTemplateStatusLabel(WorkflowTemplateResult template)
        {
            if (template == null) return "-";
            return string.IsNullOrWhiteSpace(template.StatusLabel) ? "-" : template.StatusLabel;
        }

        private string PromptReason()
        {
            using (var dlg = new Form())
            {
                dlg.Text = "ระบุเหตุผลการปฏิเสธ"; dlg.ClientSize = new Size(420, 168);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog; dlg.StartPosition = FormStartPosition.CenterParent; dlg.MaximizeBox = false;
                var lbl   = new Label   { Text = "เหตุผล:", AutoSize = true, Location = new Point(16, 16) };
                var txt   = new TextBox { Location = new Point(16, 36), Size = new Size(388, 64), Multiline = true };
                var ok    = new Button  { Text = "ยืนยัน",  DialogResult = DialogResult.OK,     Location = new Point(236, 116), Size = new Size(80, 28) };
                var cancel= new Button  { Text = "ยกเลิก", DialogResult = DialogResult.Cancel, Location = new Point(324, 116), Size = new Size(80, 28) };
                dlg.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
                dlg.AcceptButton = ok; dlg.CancelButton = cancel;
                return dlg.ShowDialog(this) == DialogResult.OK ? txt.Text.Trim() : null;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // Sidebar Navigation
        // ════════════════════════════════════════════════════════════════════════

        private int GetPendingCountSafe()
        {
            try
            {
                var result = _presenter != null
                    ? _presenter.GetPendingCount()
                    : 0;
                return result;
            }
            catch { return 0; }
        }

        private void OnNavItemClick(string tag)
        {
            if (tag == "approval") { CloseDetailView(); return; }
            WorkflowNavigationHandler.HandleNavigation(tag, this, null, _templateService, null, _loggingService, _userContext, _auditService);
        }

        // ════════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════════

        private void InvalidateTemplateListRow(int index)
        {
            if (index < 0 || index >= lvTemplates.Items.Count) return;
            var r = lvTemplates.GetItemRect(index, ItemBoundsPortion.Entire);
            r.Width = lvTemplates.ClientSize.Width;
            lvTemplates.Invalidate(r);
        }

        private static RectangleF PadRect(Rectangle r, int hp, int vp)
            => new RectangleF(r.X + hp, r.Y + vp, r.Width - hp * 2, r.Height - vp * 2);

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

        private int MeasureText(string text, Font f)
        {
            using (var g = CreateGraphics()) return (int)g.MeasureString(text, f).Width;
        }

        private static string ExtractName(string adUser)
        {
            if (string.IsNullOrWhiteSpace(adUser)) return "ไม่ระบุ";
            int bs = adUser.LastIndexOf('\\');
            return bs >= 0 ? adUser.Substring(bs + 1) : adUser;
        }

        private static string ExtractFullName(string adUser)
        {
            var name = ExtractName(adUser);
            // Simple mock mapping
            if (name.StartsWith("supaph")) return "สุภาพร จันทร์ฉาย";
            if (name.StartsWith("hr"))     return "HR Administrator";
            if (name.StartsWith("ops"))    return "Ops Officer";
            return name;
        }
    }
}
