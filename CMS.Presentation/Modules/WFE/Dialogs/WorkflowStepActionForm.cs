using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    /// <summary>
    /// Dialog "ดำเนินการ Step" — แสดงรายละเอียด step และรับ action (อนุมัติ/ปฏิเสธ/ส่งต่อ/รับทราบ).
    /// Scrollable body: info cards | designer instruction | attachments | history | action | comment | WI warning.
    /// </summary>
    public partial class WorkflowStepActionForm : Form
    {
        // ── Input ─────────────────────────────────────────────────────────────
        public WorkflowTaskResult Task { get; set; }

        // ── Optional extra data (injected by caller) ──────────────────────────
        /// <summary>คำแนะนำจากผู้ออกแบบ Workflow</summary>
        public string DesignerInstruction { get; set; }
        /// <summary>รายการไฟล์แนบ (FileName, FileSize, FileType)</summary>
        public List<AttachmentInfo> Attachments { get; set; } = new List<AttachmentInfo>();
        /// <summary>ประวัติ step ก่อนหน้า</summary>
        public List<StepHistoryItem> PreviousHistory { get; set; } = new List<StepHistoryItem>();
        /// <summary>Work Instruction ที่บังคับ (null = ไม่มี)</summary>
        public string WorkInstructionRef { get; set; }
        /// <summary>SLA % ที่ใช้ไปแล้ว (0–100)</summary>
        public int SlaPercent { get; set; } = 65;
        /// <summary>วันที่กำหนด (deadline)</summary>
        public DateTime? DueDate { get; set; }
        /// <summary>ผู้ยื่น</summary>
        public string InitiatorName { get; set; }
        /// <summary>Entity ที่เชื่อมโยง</summary>
        public string LinkedEntityLabel { get; set; }
        /// <summary>Actions ที่อนุญาต</summary>
        public bool AllowApprove  { get; set; } = true;
        public bool AllowReject   { get; set; } = true;
        public bool AllowForward  { get; set; } = false;
        public bool AllowAck      { get; set; } = false;

        // ── Output ────────────────────────────────────────────────────────────
        public string SelectedAction { get; private set; }  // "Approve","Reject","Forward","Ack"
        public string Comments       { get; private set; }
        public bool   IsApproved     => SelectedAction == "Approve";

        // ── State ─────────────────────────────────────────────────────────────
        private string _currentAction = "Approve";
        private Panel  _activeActionCard;

        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color NavyDark    = Color.FromArgb(22,  36,  62);
        private static readonly Color AccentBlue  = Color.FromArgb(37,  99,  235);
        private static readonly Color GreenOk     = Color.FromArgb(23,  133, 49);
        private static readonly Color RedReject   = Color.FromArgb(185, 40,  40);
        private static readonly Color AmberSLA    = Color.FromArgb(197, 120, 16);
        private static readonly Color SelectedBg  = Color.FromArgb(240, 245, 255);
        private static readonly Color SelectedBdr = Color.FromArgb(59,  91,  219);

        // ── Constructor ───────────────────────────────────────────────────────

        public WorkflowStepActionForm(WorkflowTaskResult task)
        {
            InitializeComponent();
            Task = task;
        }

        // ════════════════════════════════════════════════════════════════════
        // Load
        // ════════════════════════════════════════════════════════════════════

        private void WorkflowStepActionForm_Load(object sender, EventArgs e)
        {
            try
            {
                FitDialogToOwner();
                PopulateTitleBar();
                PopulateInfoCards();
                PopulateInstruction();
                PopulateAttachments();
                PopulateHistory();
                BuildActionCards();
                WireCommentEvents();
                PopulateWIWarning();

                // Set scroll content width to match outer panel
                pnlScrollContent.Width = pnlScrollOuter.ClientSize.Width - 2;
                pnlScrollOuter.Resize += (s, ev) =>
                    pnlScrollContent.Width = pnlScrollOuter.ClientSize.Width - 2;
                pnlFooter.Resize += (s, ev) => LayoutFooterButtons();
                this.Resize += (s, ev) => LayoutFooterButtons();
                LayoutFooterButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FitDialogToOwner()
        {
            var owner = this.Owner ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null);
            if (owner == null) return;

            int maxWidth = Math.Max(MinimumSize.Width, owner.ClientSize.Width - 32);
            int maxHeight = Math.Max(MinimumSize.Height, owner.ClientSize.Height - 32);

            if (Width > maxWidth) Width = maxWidth;
            if (Height > maxHeight) Height = maxHeight;

            var ownerScreen = owner.RectangleToScreen(owner.ClientRectangle);
            Left = ownerScreen.Left + Math.Max(0, (ownerScreen.Width - Width) / 2);
            Top = ownerScreen.Top + Math.Max(0, (ownerScreen.Height - Height) / 2);
        }

        private void LayoutFooterButtons()
        {
            const int gap = 8;
            const int rightMargin = 16;
            int y = Math.Max(8, (pnlFooter.Height - btnConfirm.Height) / 2);

            btnConfirm.Location = new Point(
                Math.Max(rightMargin, pnlFooter.ClientSize.Width - rightMargin - btnConfirm.Width),
                y);
            btnCancel.Location = new Point(
                Math.Max(rightMargin, btnConfirm.Left - gap - btnCancel.Width),
                y);
        }

        // ════════════════════════════════════════════════════════════════════
        // Title bar
        // ════════════════════════════════════════════════════════════════════

        private void PopulateTitleBar()
        {
            if (Task == null) return;

            lblTitleText.Text = string.Format("ดำเนินการ Step — {0}", Task.StepName ?? Task.WorkflowName ?? "-");
            lblTitleSub.Text  = string.Format("WFI-{0:D4}-{1:D5}  ·  {2}",
                DateTime.Now.Year + 543,
                Task.WorkflowInstanceId,
                Task.StepDescription ?? "ผู้จัดการตรวจสอบเนื้อหา");

            // SLA badge
            int daysLeft = Task.TimeRemainingMinutes.HasValue
                ? Task.TimeRemainingMinutes.Value / (60 * 24)
                : 3;
            lblSLABadge.Text = string.Format("⏱  เหลือ {0} วัน", Math.Max(0, daysLeft));
            if (daysLeft <= 1)
            {
                lblSLABadge.ForeColor = Color.FromArgb(185, 40, 40);
                lblSLABadge.BackColor = Color.FromArgb(254, 226, 226);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Info cards (left + right)
        // ════════════════════════════════════════════════════════════════════

        private void PopulateInfoCards()
        {
            // Left card
            pnlInfoLeft.Controls.Clear();
            pnlInfoLeft.Paint += (s, pe) => DrawCardBorder(pe.Graphics, pnlInfoLeft);

            AddInfoRow(pnlInfoLeft, "Workflow",  Task?.WorkflowName ?? "-",  10, bold: true);
            AddInfoRow(pnlInfoLeft, "ผู้ยื่น",   InitiatorName ?? Task?.InitiatedBy ?? "-", 36, bold: true);
            AddInfoRow(pnlInfoLeft, "เชื่อมโยง", LinkedEntityLabel ?? "-",  62, isLink: true);

            // Right card
            pnlInfoRight.Controls.Clear();
            pnlInfoRight.Paint += (s, pe) => DrawCardBorder(pe.Graphics, pnlInfoRight);

            string dueStr = DueDate.HasValue
                ? DueDate.Value.ToString("dd/MM/yyyy HH:mm")
                : (Task?.SLADeadline != default ? Task.SLADeadline.ToString("dd/MM/yyyy HH:mm") : "-");

            AddInfoRow(pnlInfoRight, "กำหนด", dueStr, 10, bold: true);
            AddSLABar(pnlInfoRight, SlaPercent, 36);
            AddWIBadge(pnlInfoRight, 62);
        }

        private static void AddInfoRow(Panel parent, string label, string value, int y,
            bool bold = false, bool isLink = false)
        {
            parent.Controls.Add(new Label
            {
                Text      = label,
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(110, 120, 140),
                Location  = new Point(12, y),
                Size      = new Size(80, 20),
            });

            if (isLink)
            {
                var lnk = new Label
                {
                    Text      = value,
                    Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    ForeColor = AccentBlue,
                    BackColor = Color.FromArgb(235, 242, 255),
                    Location  = new Point(96, y),
                    AutoSize  = true,
                    Padding   = new Padding(4, 2, 4, 2),
                    Cursor    = Cursors.Hand,
                };
                parent.Controls.Add(lnk);
            }
            else
            {
                parent.Controls.Add(new Label
                {
                    Text      = value,
                    Font      = new Font("Segoe UI", 8.5F, bold ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = NavyDark,
                    Location  = new Point(96, y),
                    Size      = new Size(240, 20),
                    AutoEllipsis = true,
                });
            }
        }

        private void AddSLABar(Panel parent, int percent, int y)
        {
            parent.Controls.Add(new Label
            {
                Text      = "SLA",
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(110, 120, 140),
                Location  = new Point(12, y),
                Size      = new Size(80, 20),
            });

            var pnlBar = new Panel
            {
                Location  = new Point(96, y + 4),
                Size      = new Size(180, 10),
                BackColor = Color.FromArgb(228, 232, 240),
            };
            int fill = (int)(180 * Math.Min(1.0, percent / 100.0));
            Color barColor = percent >= 90 ? RedReject : percent >= 70 ? AmberSLA : GreenOk;
            var pnlFill = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(fill, 10),
                BackColor = barColor,
            };
            pnlBar.Controls.Add(pnlFill);
            parent.Controls.Add(pnlBar);

            parent.Controls.Add(new Label
            {
                Text      = string.Format("{0}% ใช้ไป", percent),
                Font      = new Font("Segoe UI", 8F),
                ForeColor = barColor,
                Location  = new Point(282, y),
                AutoSize  = true,
            });
        }

        private void AddWIBadge(Panel parent, int y)
        {
            parent.Controls.Add(new Label
            {
                Text      = "WI บังคับ",
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(110, 120, 140),
                Location  = new Point(12, y),
                Size      = new Size(80, 20),
            });

            if (!string.IsNullOrEmpty(WorkInstructionRef))
            {
                var badge = new Label
                {
                    Text      = "● ต้องทำก่อน Approve",
                    Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(146, 64, 14),
                    BackColor = Color.FromArgb(254, 243, 199),
                    Location  = new Point(96, y),
                    AutoSize  = true,
                    Padding   = new Padding(6, 2, 6, 2),
                };
                parent.Controls.Add(badge);
            }
        }

        private static void DrawCardBorder(Graphics g, Panel p)
        {
            using (var pen = new Pen(Color.FromArgb(220, 225, 235), 1f))
                g.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        }

        // ════════════════════════════════════════════════════════════════════
        // Designer instruction
        // ════════════════════════════════════════════════════════════════════

        private void PopulateInstruction()
        {
            if (!string.IsNullOrEmpty(DesignerInstruction))
            {
                lblInstructionText.Text = DesignerInstruction;
            }
            else if (Task != null && !string.IsNullOrEmpty(Task.StepDescription))
            {
                lblInstructionText.Text = Task.StepDescription;
            }
            else
            {
                lblInstructionHeader.Visible = false;
                pnlInstruction.Visible       = false;
                ShiftUp(lblAttachHeader, 94);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Attachments
        // ════════════════════════════════════════════════════════════════════

        private void PopulateAttachments()
        {
            pnlAttachments.Controls.Clear();
            if (Attachments == null || Attachments.Count == 0)
            {
                lblAttachHeader.Visible = false;
                pnlAttachments.Visible  = false;
                ShiftUp(lblHistoryHeader, 56);
                return;
            }

            int y = 0;
            foreach (var att in Attachments)
            {
                var row = BuildAttachmentRow(att);
                row.Location = new Point(0, y);
                pnlAttachments.Controls.Add(row);
                y += row.Height + 4;
            }
            pnlAttachments.Height = y;
        }

        private static Panel BuildAttachmentRow(AttachmentInfo att)
        {
            bool isPdf = (att.FileType ?? "").IndexOf("pdf", StringComparison.OrdinalIgnoreCase) >= 0;
            var pnl = new Panel
            {
                Size      = new Size(720, 52),
                BackColor = Color.White,
            };
            pnl.Paint += (s, pe) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 225, 235), 1f))
                    pe.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            // File icon
            pnl.Controls.Add(new Label
            {
                Text      = isPdf ? "📄" : "📝",
                Font      = new Font("Segoe UI", 14F),
                ForeColor = isPdf ? Color.FromArgb(185, 40, 40) : Color.FromArgb(37, 99, 235),
                Location  = new Point(12, 10),
                Size      = new Size(28, 32),
                TextAlign = ContentAlignment.MiddleCenter,
            });

            pnl.Controls.Add(new Label
            {
                Text      = att.FileName ?? "-",
                Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = NavyDark,
                Location  = new Point(46, 8),
                Size      = new Size(500, 20),
                AutoEllipsis = true,
            });
            pnl.Controls.Add(new Label
            {
                Text      = string.Format("{0}  ·  {1}", att.FileType ?? "-", att.FileSize ?? "-"),
                Font      = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(110, 120, 140),
                Location  = new Point(46, 28),
                AutoSize  = true,
            });

            var btnDl = new Button
            {
                Text      = "⬇  ดาวน์โหลด",
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = AccentBlue,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(600, 14),
                Size      = new Size(108, 24),
                Tag       = att,
            };
            btnDl.FlatAppearance.BorderSize = 0;
            btnDl.Click += (s, e) =>
                MessageBox.Show("ดาวน์โหลด: " + att.FileName, "ดาวน์โหลด",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            pnl.Controls.Add(btnDl);

            return pnl;
        }

        // ════════════════════════════════════════════════════════════════════
        // History
        // ════════════════════════════════════════════════════════════════════

        private void PopulateHistory()
        {
            pnlHistory.Controls.Clear();
            if (PreviousHistory == null || PreviousHistory.Count == 0)
            {
                lblHistoryHeader.Visible = false;
                pnlHistory.Visible       = false;
                ShiftUp(lblActionHeader, 56);
                return;
            }

            int y = 0;
            foreach (var h in PreviousHistory)
            {
                var lbl = new Label
                {
                    Text      = string.Format("✓  {0}  ·  {1} — {2}: \"{3}\"",
                        h.ActorName, h.ActionDate.ToString("dd/MM/yy HH:mm"),
                        h.Action, h.Comment ?? ""),
                    Font      = new Font("Segoe UI", 8.5F),
                    ForeColor = Color.FromArgb(55, 65, 90),
                    Location  = new Point(12, y + 10),
                    Size      = new Size(696, 20),
                    AutoEllipsis = true,
                };
                pnlHistory.Controls.Add(lbl);
                y += 24;
            }
            pnlHistory.Height = Math.Max(44, y + 20);
        }

        // ════════════════════════════════════════════════════════════════════
        // Action cards
        // ════════════════════════════════════════════════════════════════════

        private void BuildActionCards()
        {
            pnlActions.Controls.Clear();

            var actions = new[]
            {
                new { Key = "Approve",  Label = "อนุมัติ",  Sub = "ผ่าน · ไปขั้นถัดไป", Color = GreenOk,   Enabled = AllowApprove },
                new { Key = "Reject",   Label = "ปฏิเสธ",   Sub = "ต้องกรอกเหตุผล",     Color = RedReject, Enabled = AllowReject  },
                new { Key = "Forward",  Label = "ส่งต่อ",   Sub = "เลือกผู้รับใหม่",     Color = AccentBlue,Enabled = AllowForward },
                new { Key = "Ack",      Label = "รับทราบ",  Sub = "Acknowledge",          Color = Color.FromArgb(80, 90, 110), Enabled = AllowAck },
            };

            int x = 0;
            foreach (var a in actions)
            {
                var card = BuildActionCard(a.Key, a.Label, a.Sub, a.Color, a.Enabled);
                card.Location = new Point(x, 0);
                pnlActions.Controls.Add(card);
                if (a.Key == _currentAction) SetActiveActionCard(card, a.Color);
                x += card.Width + 8;
            }
        }

        private Panel BuildActionCard(string key, string label, string sub, Color color, bool enabled)
        {
            bool isSelected = key == _currentAction;
            var pnl = new Panel
            {
                Size      = new Size(168, 68),
                BackColor = isSelected ? SelectedBg : Color.White,
                Cursor    = enabled ? Cursors.Hand : Cursors.Default,
                Enabled   = enabled,
                Tag       = key,
            };
            pnl.Paint += (s, pe) =>
            {
                bool sel = (string)pnl.Tag == _currentAction;
                using (var pen = new Pen(sel ? SelectedBdr : Color.FromArgb(210, 215, 225), sel ? 1.5f : 1f))
                    pe.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            // Radio
            var radio = new Panel
            {
                Location  = new Point(10, 26),
                Size      = new Size(18, 18),
                BackColor = Color.Transparent,
                Tag       = key,
            };
            radio.Paint += (s, pe) => DrawRadio(pe.Graphics, (string)radio.Tag == _currentAction, color);

            var lblMain = new Label
            {
                Text      = label,
                Font      = new Font("Segoe UI", 9.5F, isSelected ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = isSelected ? color : Color.FromArgb(30, 36, 55),
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

            if (enabled)
            {
                EventHandler click = (s, ev) => OnActionSelected(key, color);
                foreach (Control c in new Control[] { pnl, radio, lblMain, lblSub })
                    c.Click += click;
            }

            return pnl;
        }

        private void OnActionSelected(string key, Color color)
        {
            _currentAction = key;
            BuildActionCards(); // re-render
            // Comment required only for Reject
            lblCommentHeader.Text = key == "Reject"
                ? "หมายเหตุ / เหตุผล  (บังคับถ้าปฏิเสธ)"
                : "หมายเหตุ / เหตุผล  (ไม่บังคับ)";
        }

        private static void SetActiveActionCard(Panel card, Color color) { /* handled in BuildActionCards */ }

        private static void DrawRadio(Graphics g, bool filled, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(1, 1, 14, 14);
            using (var pen = new Pen(filled ? color : Color.FromArgb(180, 185, 200), 1.5f))
                g.DrawEllipse(pen, r);
            if (filled)
            {
                var inner = new Rectangle(4, 4, 8, 8);
                using (var br = new SolidBrush(color))
                    g.FillEllipse(br, inner);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Comment
        // ════════════════════════════════════════════════════════════════════

        private void WireCommentEvents()
        {
            txtComment.TextChanged += (s, e) =>
            {
                int len = txtComment.Text.Length;
                lblCharCount.Text = string.Format("{0} / 500 ตัวอักษร", len);
                lblCharCount.ForeColor = len > 450
                    ? Color.FromArgb(185, 40, 40)
                    : Color.FromArgb(140, 150, 165);
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // WI Warning
        // ════════════════════════════════════════════════════════════════════

        private void PopulateWIWarning()
        {
            if (!string.IsNullOrEmpty(WorkInstructionRef))
            {
                pnlWIWarning.Visible   = true;
                lblWIWarningText.Text  = string.Format("⚠  Work Instruction บังคับ: {0} — ยังไม่ทำครบ", WorkInstructionRef);
                btnOpenWI.Click       += (s, e) =>
                    MessageBox.Show("เปิด Work Instruction: " + WorkInstructionRef, "WI",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Buttons
        // ════════════════════════════════════════════════════════════════════

        private void btnComplete_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            SelectedAction = _currentAction;
            Comments       = txtComment.Text.Trim();
            DialogResult   = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateInput()
        {
            if (_currentAction == "Reject" && string.IsNullOrWhiteSpace(txtComment.Text))
            {
                MessageBox.Show("กรุณากรอกเหตุผลการปฏิเสธ", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtComment.Focus();
                return false;
            }
            return true;
        }

        // ════════════════════════════════════════════════════════════════════
        // Layout helper
        // ════════════════════════════════════════════════════════════════════

        private void ShiftUp(Control from, int pixels)
        {
            // Shift all controls from 'from' upward by pixels
            bool shifting = false;
            foreach (Control c in pnlScrollContent.Controls)
            {
                if (c == from) shifting = true;
                if (shifting)
                    c.Location = new Point(c.Location.X, c.Location.Y - pixels);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Data classes
        // ════════════════════════════════════════════════════════════════════

        public class AttachmentInfo
        {
            public string FileName { get; set; }
            public string FileType { get; set; }
            public string FileSize { get; set; }
            public string FilePath { get; set; }
        }

        public class StepHistoryItem
        {
            public string   ActorName  { get; set; }
            public DateTime ActionDate { get; set; }
            public string   Action     { get; set; }
            public string   Comment    { get; set; }
        }
    }
}
