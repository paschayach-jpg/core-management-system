using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.Workflow
{
    /// <summary>
    /// Shared navigation builder for Workflow module forms.
    /// Provides dynamic sidebar construction with standardized dark navy theme.
    /// Single source of truth for WFE navigation menu structure.
    /// </summary>
    public delegate void NavClickHandler(string tag);

    public struct WfeNavItem
    {
        public string Icon;        // Segoe UI emoji
        public string MdlIcon;     // Segoe MDL2 Assets character (not used in basic version)
        public string Text;
        public int Badge;          // 0 = no badge
        public Color BadgeColor;
        public string Tag;         // Handler key
        public bool IsDefault;     // True = this item is active on form load
    }

    public struct WfeNavSection
    {
        public string Header;
        public WfeNavItem[] Items;
    }

    public static class WorkflowNavigationBuilder
    {
        // ── Standardized colors (dark navy theme) ────────────────────────────
        private static readonly Color SidebarBg = Color.FromArgb(26, 35, 67);
        private static readonly Color ActiveBg = Color.FromArgb(59, 91, 219);
        private static readonly Color ActiveFg = Color.White;
        private static readonly Color InactiveFg = Color.FromArgb(196, 207, 232);
        private static readonly Color HeaderFg = Color.FromArgb(136, 146, 164);
        private static readonly Color HoverBg = Color.FromArgb(38, 50, 90);
        private static readonly Color AccentBar = Color.FromArgb(59, 91, 219);
        private static readonly Color OrangeBadge = Color.FromArgb(230, 115, 0);
        private static readonly Color SeparatorColor = Color.FromArgb(42, 55, 95);

        // ── Canonical menu definition (single source of truth) ───────────────
        /// <summary>
        /// Returns the standardized Workflow menu structure.
        /// This is the only place the menu is defined; all forms reference this.
        /// </summary>
        public static WfeNavSection[] GetDefaultSections(
            int myTasksBadge = 8,
            int monitorBadge = 24,
            int approvalBadge = 3)
        {
            return new[]
            {
                new WfeNavSection
                {
                    Header = "การทำงาน",
                    Items = new[]
                    {
                        new WfeNavItem { Icon="🏠", Text="งานของฉัน",         Badge=myTasksBadge,  BadgeColor=OrangeBadge, Tag="mytasks",  IsDefault=true  },
                        new WfeNavItem { Icon="✓",  Text="ตรวจสอบ Workflow",  Badge=monitorBadge, BadgeColor=OrangeBadge, Tag="monitor",  IsDefault=false },
                        new WfeNavItem { Icon="+",  Text="เริ่ม Workflow",    Badge=0,           BadgeColor=SidebarBg,   Tag="create",   IsDefault=false },
                        new WfeNavItem { Icon="📋", Text="ประวัติ Workflow",  Badge=0,           BadgeColor=SidebarBg,   Tag="history",  IsDefault=false },
                    }
                },
                new WfeNavSection
                {
                    Header = "จัดการ",
                    Items = new[]
                    {
                        new WfeNavItem { Icon="📄", Text="จัดการ Template",  Badge=0,           BadgeColor=SidebarBg,   Tag="template", IsDefault=false },
                        new WfeNavItem { Icon="🎨", Text="Workflow Designer", Badge=0,           BadgeColor=SidebarBg,   Tag="designer", IsDefault=false },
                        new WfeNavItem { Icon="☑",  Text="อนุมัติ Template", Badge=approvalBadge, BadgeColor=OrangeBadge, Tag="approval", IsDefault=false },
                    }
                },
                new WfeNavSection
                {
                    Header = "ตั้งค่า",
                    Items = new[]
                    {
                        new WfeNavItem { Icon="👥", Text="Delegation",  Badge=0, BadgeColor=SidebarBg, Tag="delegation", IsDefault=false },
                        new WfeNavItem { Icon="📊", Text="รายงาน SLA", Badge=0, BadgeColor=SidebarBg, Tag="sla",        IsDefault=false },
                        new WfeNavItem { Icon="⚙",  Text="ตั้งค่า",   Badge=0, BadgeColor=SidebarBg, Tag="settings",   IsDefault=false },
                    }
                }
            };
        }

        // ── Main public entry point ──────────────────────────────────────────
        /// <summary>
        /// Dynamically builds a sidebar inside the given container.
        /// Respects container width and docks controls accordingly.
        /// </summary>
        public static void BuildSidebar(
            Panel container,
            string activeTag,
            NavClickHandler onNavClick,
            out Panel initialActivePanel)
        {
            container.BackColor = SidebarBg;
            container.Controls.Clear();

            var sections = GetDefaultSections();
            Panel activePanel = null;
            int y = 4;

            foreach (var section in sections)
            {
                // Section header label
                var lblHeader = new Label
                {
                    Text = section.Header,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = HeaderFg,
                    Location = new Point(16, y + 4),
                    Size = new Size(container.Width - 32, 20),
                    AutoSize = false,
                };
                container.Controls.Add(lblHeader);
                y += 26;

                foreach (var item in section.Items)
                {
                    var navPanel = BuildNavPanel(item, onNavClick, container.Width);
                    navPanel.Location = new Point(0, y);
                    container.Controls.Add(navPanel);

                    if (item.Tag == activeTag)
                    {
                        activePanel = navPanel;
                    }

                    y += 34;
                }

                y += 4;
            }

            // Initialize the active panel if found; otherwise use first default
            if (activePanel == null)
            {
                foreach (var section in sections)
                {
                    foreach (var item in section.Items)
                    {
                        if (item.IsDefault)
                        {
                            // Find the panel by tag
                            foreach (Control ctrl in container.Controls)
                            {
                                if (ctrl is Panel p && p.Tag is string && (string)p.Tag == item.Tag)
                                {
                                    activePanel = p;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    if (activePanel != null) break;
                }
            }

            initialActivePanel = activePanel;
            if (activePanel != null)
            {
                SetActiveNav(container, activePanel);
            }
        }

        // ── Change active state programmatically ─────────────────────────────
        /// <summary>
        /// Changes the active navigation item by tag.
        /// </summary>
        public static void SetActiveByTag(
            Panel container,
            string tag,
            ref Panel activeNavItemField)
        {
            foreach (Control ctrl in container.Controls)
            {
                if (ctrl is Panel p && p.Tag is string && (string)p.Tag == tag)
                {
                    SetActiveNav(container, p);
                    activeNavItemField = p;
                    return;
                }
            }
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private static Panel BuildNavPanel(WfeNavItem item, NavClickHandler onNavClick, int containerWidth)
        {
            var pnl = new Panel
            {
                Size = new Size(containerWidth, 34),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = item.Tag,
            };

            var pnlAccent = new Panel
            {
                Location = new Point(0, 4),
                Size = new Size(3, 26),
                BackColor = Color.Transparent,
                Name = "accent",
            };

            var lblIcon = new Label
            {
                Text = item.Icon,
                Font = new Font("Segoe UI", 11F),
                Location = new Point(14, 7),
                Size = new Size(22, 22),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            var lblText = new Label
            {
                Text = item.Text,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(44, 52, 70),
                Location = new Point(40, 8),
                Size = new Size(containerWidth - 110, 20),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
            };

            pnl.Controls.Add(pnlAccent);
            pnl.Controls.Add(lblIcon);
            pnl.Controls.Add(lblText);

            // Badge if needed
            if (item.Badge > 0)
            {
                var badgeColor = item.BadgeColor;
                var lblBadge = new Label
                {
                    Text = item.Badge.ToString(),
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = badgeColor,
                    Location = new Point(containerWidth - 40, 10),
                    Size = new Size(item.Badge >= 10 ? 26 : 20, 16),
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                // Rounded badge paint
                lblBadge.Paint += (s, pe) =>
                {
                    var bc = badgeColor;
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var r = new Rectangle(0, 0, lblBadge.Width - 1, lblBadge.Height - 1);
                    using (var b = new SolidBrush(bc))
                    using (var path = RoundedRect(r, 8))
                        pe.Graphics.FillPath(b, path);
                    pe.Graphics.DrawString(lblBadge.Text, lblBadge.Font, Brushes.White,
                        new RectangleF(0, 0, lblBadge.Width, lblBadge.Height),
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                };
                pnl.Controls.Add(lblBadge);
            }

            // Hover/click behavior
            var tag = item.Tag;
            Action<Color, Color, bool> applyState = (bg, fg, active) =>
            {
                pnl.BackColor = bg;
                foreach (Control c in pnl.Controls)
                {
                    if (c.Name != "accent") c.BackColor = bg;
                    if (c is Label lbl && c.Name != "accent") lbl.ForeColor = active ? ActiveFg : fg;
                }
                var accent = pnl.Controls["accent"];
                if (accent != null)
                    accent.BackColor = active ? AccentBar : Color.Transparent;
            };

            EventHandler enterH = (s, e) =>
            {
                applyState(HoverBg, InactiveFg, false);
            };
            EventHandler leaveH = (s, e) =>
            {
                if (pnl.BackColor != ActiveBg)
                    applyState(Color.White, Color.FromArgb(44, 52, 70), false);
            };
            EventHandler clickH = (s, e) =>
            {
                // Find parent container and set active
                Control parent = pnl.Parent;
                if (parent != null)
                {
                    foreach (Control sibling in parent.Controls)
                    {
                        if (sibling is Panel sib && sib != pnl)
                            applyState(Color.White, Color.FromArgb(44, 52, 70), false);
                    }
                    SetActiveNav(parent as Panel, pnl);
                }
                onNavClick?.Invoke(tag);
            };

            foreach (Control c in pnl.Controls)
            {
                c.MouseEnter += enterH;
                c.MouseLeave += leaveH;
                c.Click += clickH;
            }
            pnl.MouseEnter += enterH;
            pnl.MouseLeave += leaveH;
            pnl.Click += clickH;

            return pnl;
        }

        private static void SetActiveNav(Panel container, Panel navPanel)
        {
            // Deactivate all other panels
            foreach (Control ctrl in container.Controls)
            {
                if (ctrl is Panel p && p != navPanel && p.Tag is string)
                {
                    p.BackColor = Color.White;
                    foreach (Control c in p.Controls)
                    {
                        if (c.Name != "accent") c.BackColor = Color.White;
                        if (c is Label lbl && c.Name != "accent") lbl.ForeColor = Color.FromArgb(44, 52, 70);
                        if (c.Name == "accent") c.BackColor = Color.Transparent;
                    }
                }
            }

            // Activate this panel
            navPanel.BackColor = ActiveBg;
            foreach (Control c in navPanel.Controls)
            {
                if (c.Name == "accent") { c.BackColor = AccentBar; continue; }
                c.BackColor = ActiveBg;
                if (c is Label lbl)
                {
                    lbl.ForeColor = ActiveFg;
                    if (lbl.Text != "" && lbl.Text.Length > 2) // text label (not icon)
                        lbl.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                }
            }
        }

        // ── Graphics helper ──────────────────────────────────────────────────
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
    }
}
