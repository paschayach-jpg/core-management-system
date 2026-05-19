using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Presenters;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    /// <summary>
    /// Plugin Registry Form (WFE-07) — Admin จัดการ s_WorkflowPlugin
    /// แสดง, register, enable/disable, ลบ Custom Plugin
    /// SRS FR-008-22, FR-008-25, UC-008-12, BR-008-11
    /// </summary>
    public partial class PluginRegistryForm : Form, IPluginRegistryView
    {
        private readonly PluginRegistryPresenter _presenter;
        private readonly UserContext             _userContext;

        private readonly List<WorkflowPluginResult> _currentItems = new List<WorkflowPluginResult>();
        private bool _searchPlaceholderActive = true;

        private readonly Color _navyDark = Color.FromArgb(31, 56, 100);
        private readonly Color _navHover = Color.FromArgb(245, 249, 255);

        private static readonly Dictionary<bool, Color> StatusColors = new Dictionary<bool, Color>
        {
            { true,  Color.FromArgb(23, 133, 49)  },  // Active  — green
            { false, Color.FromArgb(120, 130, 150) },  // Inactive — grey
        };

        public PluginRegistryForm(
            IWorkflowPluginService pluginService,
            IAuditService          auditService,
            ILoggingService        loggingService,
            UserContext            userContext)
        {
            InitializeComponent();
            _userContext = userContext;

            if (pluginService != null && loggingService != null && userContext != null)
            {
                _presenter = new PluginRegistryPresenter(
                    this, pluginService, auditService, loggingService, userContext);
            }
        }

        private void PluginRegistryForm_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            try
            {
                BuildRibbonTabs();
                BuildToolbar();
                Panel initialNav;
                WorkflowNavigationBuilder.BuildSidebar(
                    pnlSidebarContainer, "plugins", OnNavItemClick, out initialNav);
                BuildStatusBar();
                SetupListView();
                SetupFilterBar();
                UpdateSummaryCards(0, 0, 0);
                if (_presenter != null)
                    _presenter.LoadPlugins();
            }
            catch (Exception ex)
            {
                ShowError("Error loading form: " + ex.Message);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // IPluginRegistryView implementation
        // ════════════════════════════════════════════════════════════════════

        public void DisplayPlugins(List<WorkflowPluginResult> plugins)
        {
            _currentItems.Clear();
            if (plugins != null) _currentItems.AddRange(plugins);

            lvPlugins.Items.Clear();
            foreach (var p in _currentItems)
            {
                var lvi = new ListViewItem(p.PluginCode ?? "-");
                lvi.SubItems.Add(p.PluginName ?? "-");
                lvi.SubItems.Add(p.Description ?? "-");
                lvi.SubItems.Add(p.ClassName ?? "-");
                lvi.SubItems.Add(p.IsBuiltIn ? "Built-in" : "Custom");
                lvi.SubItems.Add(p.RegisteredDate.ToString("dd/MM/yy"));
                lvi.SubItems.Add(p.RegisteredBy ?? "-");
                lvi.SubItems.Add(p.IsActive ? "Active" : "Inactive");
                lvi.Tag = p;
                lvPlugins.Items.Add(lvi);
            }

            lblResultCount.Text = string.Format("พบ {0} Plugin ในระบบ", _currentItems.Count);
            SetActionButtonsEnabled(false, false, false, false);
        }

        public void SetActionButtonsEnabled(bool canEnable, bool canDisable, bool canDelete, bool canDetail)
        {
            foreach (Control c in pnlToolbarContainer.Controls)
            {
                var pnl = c as Panel;
                if (pnl == null) continue;
                var tag = pnl.Tag as string ?? string.Empty;
                if (tag == "enable")  pnl.Enabled = canEnable;
                if (tag == "disable") pnl.Enabled = canDisable;
                if (tag == "delete")  pnl.Enabled = canDelete;
                if (tag == "detail")  pnl.Enabled = canDetail;
            }
        }

        public void UpdateSummaryCards(int totalPlugins, int activePlugins, int inactivePlugins)
        {
            flpCards.Controls.Clear();
            flpCards.Controls.Add(BuildCard("🔌", "Plugin ทั้งหมด",  totalPlugins,   Color.FromArgb(31, 56, 100)));
            flpCards.Controls.Add(BuildCard("✔",  "ใช้งานอยู่",     activePlugins,  Color.FromArgb(23, 133, 49)));
            flpCards.Controls.Add(BuildCard("⏸",  "ปิดใช้งาน",      inactivePlugins, Color.FromArgb(120, 130, 150)));
            lblSubtitle.Text = string.Format("Plugin Registry — {0} รายการ ({1} Active)", totalPlugins, activePlugins);
        }

        public void ShowError(string message)
            => MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void ShowInfo(string message)
            => MessageBox.Show(message, "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public string GetSearchFilter()
            => _searchPlaceholderActive ? string.Empty : txtSearch.Text.Trim();

        public string GetStatusFilter()
        {
            var sel = cmbStatus.SelectedItem as string;
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
                btn.FlatAppearance.BorderSize         = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 80, 130);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 70, 120);
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
                new { Header = "Plugin", Items = new[]
                {
                    new { Icon = "➕", Text = "ลงทะเบียน", Tag = "register" },
                    new { Icon = "✔",  Text = "เปิดใช้งาน", Tag = "enable"   },
                    new { Icon = "⏸",  Text = "ปิดใช้งาน",  Tag = "disable"  },
                    new { Icon = "🗑",  Text = "ลบ Plugin",   Tag = "delete"   },
                }},
                new { Header = "รายการ", Items = new[]
                {
                    new { Icon = "📋", Text = "รายละเอียด", Tag = "detail"  },
                    new { Icon = "🔄", Text = "รีเฟรช",     Tag = "refresh" },
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
                        Size      = new Size(62, 60),
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
                        Size      = new Size(62, 28),
                        TextAlign = ContentAlignment.MiddleCenter,
                    };
                    var lblText = new Label
                    {
                        Text      = item.Text,
                        Font      = new Font("Segoe UI", 7.5F),
                        ForeColor = Color.FromArgb(55, 65, 90),
                        Location  = new Point(0, 34),
                        Size      = new Size(62, 20),
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
                    x += 64;
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
                case "register": ShowRegisterDialog();                     break;
                case "enable":   EnableSelected();                         break;
                case "disable":  DisableSelected();                        break;
                case "delete":   DeleteSelected();                         break;
                case "detail":   ShowDetail();                             break;
                case "refresh":  if (_presenter != null) _presenter.LoadPlugins(); break;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // BuildStatusBar — wfe-form-ui.md §12
        // ════════════════════════════════════════════════════════════════════

        private void BuildStatusBar()
        {
            pnlStatusBar.Paint += (s, pe) =>
                pe.Graphics.DrawLine(new Pen(Color.FromArgb(210, 215, 225)), 0, 0, pnlStatusBar.Width, 0);

            var items = new[]
            {
                "  พร้อมใช้งาน  ",
                string.Format("  โดย: {0}  ", _userContext != null ? _userContext.ADUserName : "-"),
                "  Zoom 100%  ",
            };
            int x = 8;
            foreach (var text in items)
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
            lvPlugins.SmallImageList = img;

            lvPlugins.Columns.Add("Plugin Code",   130);
            lvPlugins.Columns.Add("ชื่อ Plugin",    200);
            lvPlugins.Columns.Add("คำอธิบาย",      250);
            lvPlugins.Columns.Add("Class Name",     200);
            lvPlugins.Columns.Add("ประเภท",         80);
            lvPlugins.Columns.Add("วันที่ลงทะเบียน", 110);
            lvPlugins.Columns.Add("ลงทะเบียนโดย",   130);
            lvPlugins.Columns.Add("สถานะ",          90);

            lvPlugins.DrawColumnHeader     += LvPlugins_DrawColumnHeader;
            lvPlugins.DrawItem             += LvPlugins_DrawItem;
            lvPlugins.DrawSubItem          += LvPlugins_DrawSubItem;
            lvPlugins.SelectedIndexChanged += LvPlugins_SelectedIndexChanged;
        }

        private void LvPlugins_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
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

        private void LvPlugins_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            bool sel = e.Item.Selected;
            var bg = sel ? Color.FromArgb(232, 241, 255)
                        : (e.ItemIndex % 2 == 0 ? Color.White : Color.FromArgb(250, 251, 253));
            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
            e.Graphics.DrawLine(new Pen(Color.FromArgb(235, 238, 244)),
                e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private void LvPlugins_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.Item == null) return;
            var r = e.Bounds;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            switch (e.ColumnIndex)
            {
                case 0: // PluginCode — bold navy
                    using (var f = new Font("Segoe UI", 9F, FontStyle.Bold))
                    using (var b = new SolidBrush(Color.FromArgb(0, 90, 170)))
                        g.DrawString(e.SubItem.Text, f, b, PadRect(r, 10, 0),
                            new StringFormat { LineAlignment = StringAlignment.Center });
                    break;

                case 4: // ประเภท — Built-in badge
                    bool isBuiltIn = string.Equals(e.SubItem.Text, "Built-in", StringComparison.Ordinal);
                    Color typeColor = isBuiltIn
                        ? Color.FromArgb(31, 56, 100)
                        : Color.FromArgb(94, 106, 133);
                    DrawTextBadge(g, r, e.SubItem.Text, typeColor);
                    break;

                case 7: // สถานะ — Active/Inactive badge
                    bool active = string.Equals(e.SubItem.Text, "Active", StringComparison.Ordinal);
                    DrawTextBadge(g, r, e.SubItem.Text, active ? StatusColors[true] : StatusColors[false]);
                    break;

                default:
                    using (var f = new Font("Segoe UI", 9F))
                    using (var b = new SolidBrush(Color.FromArgb(55, 65, 90)))
                        g.DrawString(e.SubItem.Text, f, b, PadRect(r, 10, 0),
                            new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
                    break;
            }
        }

        private void LvPlugins_SelectedIndexChanged(object sender, EventArgs e)
        {
            var plugin = GetSelectedPlugin();
            bool has = plugin != null;

            if (has)
            {
                bool isActive   = plugin.IsActive;
                bool isBuiltIn  = plugin.IsBuiltIn;
                SetActionButtonsEnabled(
                    canEnable:  has && !isActive,
                    canDisable: has && isActive && !isBuiltIn,
                    canDelete:  has && !isBuiltIn,
                    canDetail:  has);
            }
            else
            {
                SetActionButtonsEnabled(false, false, false, false);
            }

            lblSelectionInfo.Text = has
                ? string.Format("เลือก: {0}", plugin.PluginCode)
                : string.Format("แสดง {0} รายการ", _currentItems.Count);
        }

        // ════════════════════════════════════════════════════════════════════
        // SetupFilterBar — wfe-form-ui.md §6
        // ════════════════════════════════════════════════════════════════════

        private void SetupFilterBar()
        {
            cmbStatus.Items.Clear();
            foreach (var s in new[] { "ทั้งหมด", "Active", "Inactive" })
                cmbStatus.Items.Add(s);
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => { if (_presenter != null) _presenter.Search(); };

            btnSearch.Click += (s, e) => { if (_presenter != null) _presenter.Search(); };
            btnClearFilter.Click += (s, e) =>
            {
                cmbStatus.SelectedIndex = 0;
                txtSearch.Text = "ค้นหา Plugin Code / ชื่อ...";
                _searchPlaceholderActive = true;
                txtSearch.ForeColor = Color.FromArgb(140, 150, 165);
                if (_presenter != null) _presenter.LoadPlugins();
            };

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
                    txtSearch.Text      = "ค้นหา Plugin Code / ชื่อ...";
                    txtSearch.ForeColor = Color.FromArgb(140, 150, 165);
                }
            };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter && _presenter != null) _presenter.Search(); };
        }

        // ════════════════════════════════════════════════════════════════════
        // Actions
        // ════════════════════════════════════════════════════════════════════

        private void ShowRegisterDialog()
        {
            if (_presenter == null) { ShowError("ไม่สามารถดำเนินการได้"); return; }

            using (var dlg = BuildRegisterDialog())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                var req = dlg.Tag as RegisterPluginRequest;
                if (req == null) return;
                _presenter.RegisterPlugin(req);
            }
        }

        private void EnableSelected()
        {
            var p = GetSelectedPlugin();
            if (p == null) { ShowError("กรุณาเลือก Plugin ก่อน"); return; }
            if (_presenter == null) return;
            if (MessageBox.Show(
                    string.Format("ยืนยันเปิดใช้งาน Plugin '{0}' ?", p.PluginCode),
                    "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes)
                _presenter.EnablePlugin(p);
        }

        private void DisableSelected()
        {
            var p = GetSelectedPlugin();
            if (p == null) { ShowError("กรุณาเลือก Plugin ก่อน"); return; }
            if (_presenter == null) return;
            if (MessageBox.Show(
                    string.Format("ยืนยันปิดใช้งาน Plugin '{0}' ?\n\nDefinition ที่อ้างถึง Plugin นี้จะ Activate ไม่ได้จนกว่าจะเปิดใช้งานอีกครั้ง", p.PluginCode),
                    "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                == DialogResult.Yes)
                _presenter.DisablePlugin(p);
        }

        private void DeleteSelected()
        {
            var p = GetSelectedPlugin();
            if (p == null) { ShowError("กรุณาเลือก Plugin ก่อน"); return; }
            if (_presenter == null) return;
            if (MessageBox.Show(
                    string.Format("ยืนยันลบ Plugin '{0}' ?\n\nไม่สามารถยกเลิกได้ และ Plugin จะใช้ไม่ได้ใน Definition ใหม่", p.PluginCode),
                    "ยืนยันลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                == DialogResult.Yes)
                _presenter.DeletePlugin(p);
        }

        private void ShowDetail()
        {
            var p = GetSelectedPlugin();
            if (p == null) { ShowError("กรุณาเลือก Plugin ก่อน"); return; }
            MessageBox.Show(
                string.Format(
                    "Plugin Code : {0}\nชื่อ        : {1}\nคำอธิบาย   : {2}\nClass Name  : {3}\nAssembly    : {4}\nประเภท      : {5}\nสถานะ       : {6}\nลงทะเบียนโดย: {7}\nวันที่       : {8}",
                    p.PluginCode,
                    p.PluginName ?? "-",
                    p.Description ?? "-",
                    p.ClassName ?? "-",
                    p.AssemblyName ?? "(same assembly)",
                    p.IsBuiltIn ? "Built-in" : "Custom",
                    p.IsActive ? "Active" : "Inactive",
                    p.RegisteredBy ?? "-",
                    p.RegisteredDate.ToString("dd/MM/yyyy HH:mm")),
                "รายละเอียด Plugin",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OnNavItemClick(string tag)
        {
            if (tag == "plugins") return;
            WorkflowNavigationHandler.HandleNavigation(
                tag, this, null, null, null, null, _userContext, null);
        }

        // ════════════════════════════════════════════════════════════════════
        // Register Dialog
        // ════════════════════════════════════════════════════════════════════

        private Form BuildRegisterDialog()
        {
            var dlg = new Form
            {
                Text            = "ลงทะเบียน Plugin ใหม่",
                ClientSize      = new Size(480, 320),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition   = FormStartPosition.CenterParent,
                MaximizeBox     = false,
                Font            = new Font("Segoe UI", 9F),
            };

            int y = 16;
            var fields = new[]
            {
                new { Label = "Plugin Code *",  Name = "code",     Hint = "เช่น SEND_EMAIL"           },
                new { Label = "Plugin Name *",  Name = "name",     Hint = "ชื่อที่แสดงในระบบ"          },
                new { Label = "Class Name *",   Name = "class",    Hint = "เช่น MyPlugin.SendEmailPlugin" },
                new { Label = "Assembly Name",  Name = "assembly", Hint = "เว้นว่างถ้า Class อยู่ใน Assembly หลัก" },
                new { Label = "คำอธิบาย",       Name = "desc",     Hint = "อธิบายสิ่งที่ Plugin ทำ"    },
            };

            var boxes = new Dictionary<string, TextBox>();
            foreach (var f in fields)
            {
                dlg.Controls.Add(new Label
                {
                    Text      = f.Label,
                    AutoSize  = true,
                    Location  = new Point(16, y + 3),
                    ForeColor = Color.FromArgb(55, 65, 90),
                });
                var txt = new TextBox
                {
                    Name        = f.Name,
                    Location    = new Point(150, y),
                    Size        = new Size(310, 26),
                    BorderStyle = BorderStyle.FixedSingle,
                    ForeColor   = Color.FromArgb(140, 150, 165),
                    Text        = f.Hint,
                };
                string hint = f.Hint;
                txt.Enter += (s, e) => { if (txt.Text == hint) { txt.Text = string.Empty; txt.ForeColor = Color.FromArgb(30, 36, 55); } };
                txt.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txt.Text)) { txt.Text = hint; txt.ForeColor = Color.FromArgb(140, 150, 165); } };
                dlg.Controls.Add(txt);
                boxes[f.Name] = txt;
                y += 44;
            }

            var btnOk = new Button
            {
                Text         = "ลงทะเบียน",
                DialogResult = DialogResult.None,
                Location     = new Point(290, 276),
                Size         = new Size(90, 30),
                FlatStyle    = FlatStyle.Flat,
                ForeColor    = Color.White,
                BackColor    = Color.FromArgb(31, 56, 100),
            };
            btnOk.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text         = "ยกเลิก",
                DialogResult = DialogResult.Cancel,
                Location     = new Point(388, 276),
                Size         = new Size(80, 30),
                FlatStyle    = FlatStyle.Flat,
                ForeColor    = Color.FromArgb(55, 65, 90),
            };

            btnOk.Click += (s, e) =>
            {
                string code     = GetHintFreeValue(boxes["code"],     "เช่น SEND_EMAIL");
                string name     = GetHintFreeValue(boxes["name"],     "ชื่อที่แสดงในระบบ");
                string className = GetHintFreeValue(boxes["class"],   "เช่น MyPlugin.SendEmailPlugin");
                string assembly = GetHintFreeValue(boxes["assembly"], "เว้นว่างถ้า Class อยู่ใน Assembly หลัก");
                string desc     = GetHintFreeValue(boxes["desc"],     "อธิบายสิ่งที่ Plugin ทำ");

                if (string.IsNullOrWhiteSpace(code))      { MessageBox.Show("กรุณาระบุ Plugin Code",  "ข้อมูลไม่ครบ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(name))      { MessageBox.Show("กรุณาระบุ Plugin Name",  "ข้อมูลไม่ครบ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(className)) { MessageBox.Show("กรุณาระบุ Class Name",   "ข้อมูลไม่ครบ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                dlg.Tag = new RegisterPluginRequest
                {
                    PluginCode   = code.ToUpperInvariant().Trim(),
                    PluginName   = name.Trim(),
                    ClassName    = className.Trim(),
                    AssemblyName = string.IsNullOrWhiteSpace(assembly) ? null : assembly.Trim(),
                    Description  = desc,
                };
                dlg.DialogResult = DialogResult.OK;
                dlg.Close();
            };

            dlg.Controls.Add(btnOk);
            dlg.Controls.Add(btnCancel);
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;
            return dlg;
        }

        private static string GetHintFreeValue(TextBox txt, string hint)
        {
            return string.Equals(txt.Text.Trim(), hint.Trim(), StringComparison.Ordinal)
                ? string.Empty : txt.Text.Trim();
        }

        // ════════════════════════════════════════════════════════════════════
        // Summary Cards — wfe-form-ui.md §5
        // ════════════════════════════════════════════════════════════════════

        private Panel BuildCard(string icon, string title, int count, Color color)
        {
            var pnl = new Panel
            {
                Size        = new Size(200, 82),
                BorderStyle = BorderStyle.FixedSingle,
                Margin      = new Padding(0, 0, 10, 0),
                Padding     = new Padding(14, 10, 14, 8),
            };
            pnl.Controls.Add(new Label { Text = icon, Font = new Font("Segoe UI", 14F), ForeColor = color, AutoSize = true, Location = new Point(14, 8) });
            pnl.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(80, 90, 110), AutoSize = true, Location = new Point(40, 12) });
            pnl.Controls.Add(new Label { Text = count.ToString(), Font = new Font("Segoe UI", 22F, FontStyle.Bold), ForeColor = color, AutoSize = true, Location = new Point(14, 28) });
            return pnl;
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers — wfe-form-ui.md §8
        // ════════════════════════════════════════════════════════════════════

        private WorkflowPluginResult GetSelectedPlugin()
            => lvPlugins.SelectedItems.Count == 0
                ? null
                : lvPlugins.SelectedItems[0].Tag as WorkflowPluginResult;

        private void DrawTextBadge(Graphics g, Rectangle r, string text, Color color)
        {
            var sz = g.MeasureString(text, new Font("Segoe UI", 8.5F, FontStyle.Bold));
            int bw = (int)sz.Width + 18, bh = 20;
            var br = new Rectangle(r.X + 10, r.Y + (r.Height - bh) / 2, bw, bh);

            using (var bg   = new SolidBrush(Color.FromArgb(30, color)))
            using (var path = RoundedRect(br, 4))
                g.FillPath(bg, path);

            using (var pen  = new Pen(Color.FromArgb(80, color)))
            using (var path = RoundedRect(br, 4))
                g.DrawPath(pen, path);

            g.DrawString(text,
                new Font("Segoe UI", 8.5F, FontStyle.Bold),
                new SolidBrush(color),
                new RectangleF(br.X, br.Y, br.Width, br.Height),
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private static RectangleF PadRect(Rectangle r, int hPad, int vPad)
            => new RectangleF(r.X + hPad, r.Y + vPad, r.Width - hPad * 2, r.Height - vPad * 2);

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d    = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X,         r.Y,          d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y,          d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d,   0, 90);
            path.AddArc(r.X,         r.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
