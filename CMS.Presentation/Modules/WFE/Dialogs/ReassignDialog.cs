using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    /// <summary>
    /// Dialog "Reassign / Escalate งาน" — เลือกผู้รับผิดชอบใหม่พร้อม Match Score.
    /// Layout: title bar navy | current assignee + reason | mode cards | candidate list | message | info banner | footer.
    /// </summary>
    public partial class ReassignDialog : Form
    {
        // ── Input ─────────────────────────────────────────────────────────────
        public WorkflowTaskResult Task { get; set; }

        // ── Injected data ─────────────────────────────────────────────────────
        /// <summary>ผู้รับผิดชอบปัจจุบัน</summary>
        public AssigneeInfo CurrentAssignee { get; set; }
        /// <summary>รายชื่อผู้สมัครที่ระบบแนะนำ</summary>
        public List<CandidateInfo> Candidates { get; set; } = new List<CandidateInfo>();
        /// <summary>Role filter options (e.g. "Role: Legal")</summary>
        public List<string> RoleOptions { get; set; } = new List<string>();

        // ── Output ────────────────────────────────────────────────────────────
        public string SelectedUserId    { get; private set; }
        public string SelectedUserName  { get; private set; }
        public string ReassignMode      { get; private set; }  // "ThisStep","AllSteps","Escalate"
        public string ReassignReason    { get; private set; }
        public string MessageToNew      { get; private set; }

        // ── State ─────────────────────────────────────────────────────────────
        private string _currentMode = "ThisStep";
        private CandidateInfo _selectedCandidate;
        private bool _searchPlaceholderActive = true;
        private List<CandidateInfo> _filteredCandidates;

        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color NavyDark    = Color.FromArgb(22,  36,  62);
        private static readonly Color AccentBlue  = Color.FromArgb(37,  99,  235);
        private static readonly Color GreenOk     = Color.FromArgb(23,  133, 49);
        private static readonly Color AmberWarn   = Color.FromArgb(197, 120, 16);
        private static readonly Color RedBad      = Color.FromArgb(185, 40,  40);
        private static readonly Color SelectedBg  = Color.FromArgb(240, 245, 255);
        private static readonly Color SelectedBdr = Color.FromArgb(59,  91,  219);

        // ── Avatar palette ────────────────────────────────────────────────────
        private static readonly Color[] AvatarPalette =
        {
            Color.FromArgb(59,  91,  219),
            Color.FromArgb(155, 89,  182),
            Color.FromArgb(22,  160, 133),
            Color.FromArgb(192, 57,  43),
            Color.FromArgb(41,  128, 185),
            Color.FromArgb(230, 126, 34),
        };

        // ── Constructor ───────────────────────────────────────────────────────

        public ReassignDialog(WorkflowTaskResult task)
        {
            InitializeComponent();
            Task = task;

            // Fix docking Z-order to prevent pnlScrollOuter from overlapping pnlTitleBar
            this.pnlScrollOuter.SendToBack();
            this.pnlTitleBar.BringToFront();
            this.pnlFooter.BringToFront();
        }

        // ════════════════════════════════════════════════════════════════════
        // Load
        // ════════════════════════════════════════════════════════════════════

        private void ReassignDialog_Load(object sender, EventArgs e)
        {
            try
            {
                PopulateTitleBar();
                PopulateCurrentAssignee();
                PopulateRoleFilter();
                BuildModeCards();
                WireSearchEvents();
                _filteredCandidates = new List<CandidateInfo>(Candidates);
                RenderCandidateList(_filteredCandidates);
                RecalcLayout();

                pnlScrollContent.Width = pnlScrollOuter.ClientSize.Width - 2;
                pnlScrollOuter.Resize += (s, ev) =>
                    pnlScrollContent.Width = pnlScrollOuter.ClientSize.Width - 2;

                // Reset AutoScrollPosition to (0, 0) when form is shown to override automatic focus scrolling
                this.Shown += (s, ev) =>
                {
                    pnlScrollOuter.AutoScrollPosition = new Point(0, 0);
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Title bar
        // ════════════════════════════════════════════════════════════════════

        private void PopulateTitleBar()
        {
            if (Task == null) return;
            lblTitleSub.Text = string.Format("WFI-{0:D4}-{1:D5}  ·  Step: {2}",
                DateTime.Now.Year + 543,
                Task.WorkflowInstanceId,
                Task.StepName ?? Task.StepDescription ?? "-");
        }

        // ════════════════════════════════════════════════════════════════════
        // Current assignee card
        // ════════════════════════════════════════════════════════════════════

        private void PopulateCurrentAssignee()
        {
            pnlCurrentAssignee.Controls.Clear();
            pnlCurrentAssignee.Controls.Add(lblCurrentHeader);
            pnlCurrentAssignee.Paint += (s, pe) => DrawCardBorder(pe.Graphics, pnlCurrentAssignee);

            var info = CurrentAssignee ?? new AssigneeInfo
            {
                Name     = Task?.InitiatedBy ?? "ผู้รับผิดชอบ",
                Role     = "Manager",
                Dept     = "Legal",
                IsOverSLA = true,
                SLADaysOver = 2,
            };

            // Avatar circle
            var avatar = new Panel
            {
                Location  = new Point(12, 32),
                Size      = new Size(36, 36),
                BackColor = Color.Transparent,
            };
            string initial = info.Name.Length > 0 ? info.Name.Substring(0, 1).ToUpper() : "?";
            Color avatarColor = AvatarPalette[Math.Abs(info.Name.GetHashCode()) % AvatarPalette.Length];
            avatar.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new SolidBrush(avatarColor))
                    pe.Graphics.FillEllipse(br, 0, 0, 35, 35);
                using (var f = new Font("Segoe UI", 13F, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    pe.Graphics.DrawString(initial, f, Brushes.White, new RectangleF(0, 0, 35, 35), sf);
            };
            pnlCurrentAssignee.Controls.Add(avatar);

            pnlCurrentAssignee.Controls.Add(new Label
            {
                Text      = info.Name,
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = NavyDark,
                Location  = new Point(56, 32),
                AutoSize  = true,
            });
            pnlCurrentAssignee.Controls.Add(new Label
            {
                Text      = string.Format("{0}  ·  {1}", info.Role, info.Dept),
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(110, 120, 140),
                Location  = new Point(56, 54),
                AutoSize  = true,
            });

            if (info.IsOverSLA)
            {
                var badge = new Label
                {
                    Text      = string.Format("● เกิน SLA {0} วัน", info.SLADaysOver),
                    Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(185, 40, 40),
                    BackColor = Color.FromArgb(254, 226, 226),
                    Location  = new Point(200, 38),
                    AutoSize  = true,
                    Padding   = new Padding(6, 2, 6, 2),
                };
                pnlCurrentAssignee.Controls.Add(badge);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Role filter
        // ════════════════════════════════════════════════════════════════════

        private void PopulateRoleFilter()
        {
            cmbRole.Items.Clear();
            cmbRole.Items.Add("Role: ทั้งหมด");
            if (RoleOptions != null)
                foreach (var r in RoleOptions) cmbRole.Items.Add(r);
            if (cmbRole.Items.Count == 1)
                cmbRole.Items.Add("Role: Legal (แนะนำ)");
            cmbRole.SelectedIndex = cmbRole.Items.Count > 1 ? 1 : 0;
            cmbRole.SelectedIndexChanged += (s, e) => FilterAndRender();
            chkSortByMatch.CheckedChanged += (s, e) => FilterAndRender();
        }

        // ════════════════════════════════════════════════════════════════════
        // Mode cards
        // ════════════════════════════════════════════════════════════════════

        private void BuildModeCards()
        {
            pnlModes.Controls.Clear();
            var modes = new[]
            {
                new { Key = "ThisStep",  Label = "Reassign แค่ Step นี้",  Sub = "Step ถัดไปยังตาม Template" },
                new { Key = "AllSteps",  Label = "Reassign Step ที่เหลือ", Sub = "สำหรับ Role เดียวกัน"      },
                new { Key = "Escalate",  Label = "Escalate ให้หัวหน้า",    Sub = "Head of Legal"              },
            };

            int x = 0;
            foreach (var m in modes)
            {
                var card = BuildModeCard(m.Key, m.Label, m.Sub);
                card.Location = new Point(x, 0);
                pnlModes.Controls.Add(card);
                x += card.Width + 8;
            }
        }

        private Panel BuildModeCard(string key, string label, string sub)
        {
            bool isSelected = key == _currentMode;
            var pnl = new Panel
            {
                Size      = new Size(232, 68),
                BackColor = isSelected ? SelectedBg : Color.White,
                Cursor    = Cursors.Hand,
                Tag       = key,
            };
            pnl.Paint += (s, pe) =>
            {
                bool sel = (string)pnl.Tag == _currentMode;
                using (var pen = new Pen(sel ? SelectedBdr : Color.FromArgb(210, 215, 225), sel ? 1.5f : 1f))
                    pe.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            var radio = new Panel
            {
                Location  = new Point(10, 26),
                Size      = new Size(18, 18),
                BackColor = Color.Transparent,
                Tag       = key,
            };
            radio.Paint += (s, pe) => DrawRadio(pe.Graphics, (string)radio.Tag == _currentMode);

            var lblMain = new Label
            {
                Text      = label,
                Font      = new Font("Segoe UI", 9.5F, isSelected ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = isSelected ? AccentBlue : Color.FromArgb(30, 36, 55),
                Location  = new Point(34, 12),
                AutoSize  = true,
                Tag       = key,
            };
            var lblSub = new Label
            {
                Text      = sub,
                Font      = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(110, 120, 140),
                Location  = new Point(34, 34),
                AutoSize  = true,
                Tag       = key,
            };

            pnl.Controls.Add(radio);
            pnl.Controls.Add(lblMain);
            pnl.Controls.Add(lblSub);

            EventHandler click = (s, ev) => { _currentMode = key; BuildModeCards(); };
            foreach (Control c in new Control[] { pnl, radio, lblMain, lblSub })
                c.Click += click;

            return pnl;
        }

        private static void DrawRadio(Graphics g, bool filled)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(1, 1, 14, 14);
            using (var pen = new Pen(filled ? SelectedBdr : Color.FromArgb(180, 185, 200), 1.5f))
                g.DrawEllipse(pen, r);
            if (filled)
            {
                using (var br = new SolidBrush(SelectedBdr))
                    g.FillEllipse(br, new Rectangle(4, 4, 8, 8));
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Search / filter
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
                    txtSearch.Text      = "🔍  ค้นหาชื่อหรือ AD Group...";
                    txtSearch.ForeColor = Color.FromArgb(140, 150, 165);
                }
            };
            txtSearch.TextChanged += (s, e) =>
            {
                if (!_searchPlaceholderActive) FilterAndRender();
            };
        }

        private void FilterAndRender()
        {
            string kw = _searchPlaceholderActive ? string.Empty : txtSearch.Text.Trim();
            var filtered = Candidates.Where(c =>
                string.IsNullOrEmpty(kw) ||
                (c.Name ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                (c.Role ?? "").IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            if (chkSortByMatch.Checked)
                filtered = filtered.OrderByDescending(c => c.MatchScore).ToList();

            _filteredCandidates = filtered;
            RenderCandidateList(filtered);
            RecalcLayout();
        }

        // ════════════════════════════════════════════════════════════════════
        // Candidate list
        // ════════════════════════════════════════════════════════════════════

        private void RenderCandidateList(List<CandidateInfo> candidates)
        {
            pnlCandidateList.SuspendLayout();
            pnlCandidateList.Controls.Clear();

            if (candidates == null || candidates.Count == 0)
            {
                pnlCandidateList.Controls.Add(new Label
                {
                    Text      = "ไม่พบผู้สมัครที่ตรงกัน",
                    Font      = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(140, 150, 165),
                    AutoSize  = true,
                    Location  = new Point(12, 16),
                });
                pnlCandidateList.Height = 48;
                pnlCandidateList.ResumeLayout();
                return;
            }

            int y = 0;
            foreach (var c in candidates)
            {
                var row = BuildCandidateRow(c);
                row.Location = new Point(0, y);
                pnlCandidateList.Controls.Add(row);
                y += row.Height + 4;
            }
            pnlCandidateList.Height = y;
            pnlCandidateList.ResumeLayout();
        }

        private Panel BuildCandidateRow(CandidateInfo c)
        {
            bool isSelected = _selectedCandidate != null && _selectedCandidate.UserId == c.UserId;
            var pnl = new Panel
            {
                Size      = new Size(720, 64),
                BackColor = isSelected ? SelectedBg : Color.White,
                Cursor    = Cursors.Hand,
                Tag       = c,
            };
            pnl.Paint += (s, pe) =>
            {
                bool sel = _selectedCandidate != null && ((CandidateInfo)pnl.Tag).UserId == _selectedCandidate.UserId;
                using (var pen = new Pen(sel ? SelectedBdr : Color.FromArgb(220, 225, 235), sel ? 1.5f : 1f))
                    pe.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            // Radio
            var radio = new Panel
            {
                Location  = new Point(10, 24),
                Size      = new Size(18, 18),
                BackColor = Color.Transparent,
                Tag       = c,
            };
            radio.Paint += (s, pe) =>
            {
                bool sel = _selectedCandidate != null && ((CandidateInfo)radio.Tag).UserId == _selectedCandidate.UserId;
                DrawRadio(pe.Graphics, sel);
            };

            // Avatar
            string initial = c.Name.Length > 0 ? c.Name.Substring(0, 1).ToUpper() : "?";
            Color avatarColor = AvatarPalette[Math.Abs(c.Name.GetHashCode()) % AvatarPalette.Length];
            var avatar = new Panel
            {
                Location  = new Point(36, 14),
                Size      = new Size(36, 36),
                BackColor = Color.Transparent,
                Tag       = c,
            };
            avatar.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new SolidBrush(avatarColor))
                    pe.Graphics.FillEllipse(br, 0, 0, 35, 35);
                using (var f = new Font("Segoe UI", 12F, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    pe.Graphics.DrawString(initial, f, Brushes.White, new RectangleF(0, 0, 35, 35), sf);
            };

            pnl.Controls.Add(new Label
            {
                Text      = c.Name,
                Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = NavyDark,
                Location  = new Point(80, 12),
                Size      = new Size(280, 20),
                AutoEllipsis = true,
                Tag       = c,
            });
            pnl.Controls.Add(new Label
            {
                Text      = c.Role ?? "-",
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(110, 120, 140),
                Location  = new Point(80, 34),
                AutoSize  = true,
                Tag       = c,
            });

            // Workload + Match (right side)
            int rx = 400;
            pnl.Controls.Add(new Label
            {
                Text      = string.Format("Workload {0} Active", c.WorkloadActive),
                Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 90),
                Location  = new Point(rx, 10),
                AutoSize  = true,
                Tag       = c,
            });
            pnl.Controls.Add(new Label
            {
                Text      = string.Format("เฉลี่ย {0}ว", c.AvgDays.ToString("0.0")),
                Font      = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(110, 120, 140),
                Location  = new Point(rx, 30),
                AutoSize  = true,
                Tag       = c,
            });

            // Match label + bar + badge
            pnl.Controls.Add(new Label
            {
                Text      = "Match",
                Font      = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(110, 120, 140),
                Location  = new Point(rx + 120, 10),
                AutoSize  = true,
                Tag       = c,
            });
            pnl.Controls.Add(new Label
            {
                Text      = string.Format("{0}%", c.MatchScore),
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = MatchColor(c.MatchScore),
                Location  = new Point(rx + 160, 10),
                AutoSize  = true,
                Tag       = c,
            });

            // Match bar
            var barBg = new Panel
            {
                Location  = new Point(rx + 120, 32),
                Size      = new Size(120, 8),
                BackColor = Color.FromArgb(228, 232, 240),
                Tag       = c,
            };
            int fill = (int)(120 * c.MatchScore / 100.0);
            barBg.Controls.Add(new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(fill, 8),
                BackColor = MatchColor(c.MatchScore),
            });
            pnl.Controls.Add(barBg);

            // Quality badge
            string badgeText = c.MatchScore >= 90 ? "ดีที่สุด" : c.MatchScore >= 70 ? "ใช้ได้" : "ใช้ได้";
            Color badgeColor = c.MatchScore >= 90 ? GreenOk : AmberWarn;
            var badge = new Label
            {
                Text      = badgeText,
                Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = badgeColor,
                BackColor = c.MatchScore >= 90
                    ? Color.FromArgb(220, 252, 231)
                    : Color.FromArgb(254, 243, 199),
                Location  = new Point(rx + 248, 22),
                AutoSize  = true,
                Padding   = new Padding(6, 2, 6, 2),
                Tag       = c,
            };
            pnl.Controls.Add(badge);

            pnl.Controls.Add(radio);
            pnl.Controls.Add(avatar);

            // Click handler
            EventHandler click = (s, ev) =>
            {
                _selectedCandidate = c;
                RenderCandidateList(_filteredCandidates);
            };
            foreach (Control ctrl in pnl.Controls)
                ctrl.Click += click;
            pnl.Click += click;

            return pnl;
        }

        private static Color MatchColor(int score)
            => score >= 90 ? Color.FromArgb(23, 133, 49)
             : score >= 70 ? Color.FromArgb(197, 120, 16)
                           : Color.FromArgb(185, 40, 40);

        // ════════════════════════════════════════════════════════════════════
        // Layout recalc (shift sections below candidate list)
        // ════════════════════════════════════════════════════════════════════

        private void RecalcLayout()
        {
            int candidateBottom = pnlCandidateList.Location.Y + pnlCandidateList.Height + 16;
            lblMessageHeader.Location = new Point(16, candidateBottom);
            txtMessage.Location       = new Point(16, candidateBottom + 20);
            pnlInfoBanner.Location    = new Point(16, candidateBottom + 112);

            int contentHeight = pnlInfoBanner.Location.Y + pnlInfoBanner.Height + 14;
            pnlScrollContent.Height = Math.Max(contentHeight, pnlScrollOuter.ClientSize.Height);
        }

        // ════════════════════════════════════════════════════════════════════
        // Buttons
        // ════════════════════════════════════════════════════════════════════

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            SelectedUserId   = _selectedCandidate.UserId;
            SelectedUserName = _selectedCandidate.Name;
            ReassignMode     = _currentMode;
            ReassignReason   = cmbReason.SelectedItem as string ?? string.Empty;
            MessageToNew     = txtMessage.Text.Trim();

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
            if (_selectedCandidate == null)
            {
                MessageBox.Show("กรุณาเลือกผู้รับผิดชอบใหม่", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════

        private static void DrawCardBorder(Graphics g, Panel p)
        {
            using (var pen = new Pen(Color.FromArgb(220, 225, 235), 1f))
                g.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        }

        // ════════════════════════════════════════════════════════════════════
        // Data classes
        // ════════════════════════════════════════════════════════════════════

        public class AssigneeInfo
        {
            public string Name        { get; set; }
            public string Role        { get; set; }
            public string Dept        { get; set; }
            public bool   IsOverSLA   { get; set; }
            public int    SLADaysOver { get; set; }
        }

        public class CandidateInfo
        {
            public string UserId        { get; set; }
            public string Name          { get; set; }
            public string Role          { get; set; }
            public int    WorkloadActive { get; set; }
            public double AvgDays       { get; set; }
            public int    MatchScore    { get; set; }  // 0–100
        }
    }

    public class NonAutoScrollPanel : System.Windows.Forms.Panel
    {
        protected override System.Drawing.Point ScrollToControl(System.Windows.Forms.Control activeControl)
        {
            return this.AutoScrollPosition;
        }
    }
}
