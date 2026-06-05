using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class WorkflowCreateTemplateStep4Form : Form
    {
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;
        private readonly WorkflowWizardState _wizardState;

        private static readonly Color Blue      = Color.FromArgb(37,  99, 235);
        private static readonly Color NavyDark  = Color.FromArgb(31,  56, 100);
        private static readonly Color GreenOk   = Color.FromArgb(21, 128,  61);
        private static readonly Color OrangeWarn = Color.FromArgb(146, 64,  14);
        private static readonly Color GrayText  = Color.FromArgb( 80,  90, 110);

        public WorkflowCreateTemplateStep4Form(
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext,
            WorkflowWizardState wizardState)
        {
            InitializeComponent();
            _templateService = templateService;
            _loggingService  = loggingService;
            _userContext     = userContext;
            _wizardState     = wizardState;

            this.Load    += WorkflowCreateTemplateStep4Form_Load;
            this.Closing += (s, e) =>
            {
                if (this.DialogResult == DialogResult.None)
                    this.DialogResult = DialogResult.Cancel;
            };
        }

        private void WorkflowCreateTemplateStep4Form_Load(object sender, EventArgs e)
        {
            // ── Rebrand header ─────────────────────────────────────────────────
            lblTitle.Text = "4) Validate & ส่งอนุมัติ";
            lblSteps.Text = "1) ข้อมูลพื้นฐาน  ✓    2) ออกแบบ Flow  ✓    3) ออกแบบฟอร์ม  ✓    4) Validate & ส่งอนุมัติ  ←";

            // ── Wire nav buttons ───────────────────────────────────────────────
            btnPrev.Click    += (s, ev) => { this.DialogResult = DialogResult.Retry; this.Close(); };
            btnComplete.Text  = "ส่งอนุมัติ";
            btnComplete.BackColor = Blue;
            btnComplete.ForeColor = Color.White;
            btnComplete.Click += (s, ev) => OnSubmitForApprovalClick();

            // ── Build content (replaces old pnlContent children) ───────────────
            BuildContent();
        }

        // ════════════════════════════════════════════════════════════════════════
        // Build the full content area in code (pnlContent is the Fill panel)
        // ════════════════════════════════════════════════════════════════════════

        private void BuildContent()
        {
            // Clear designer controls; we build everything in code
            pnlContent.Controls.Clear();
            pnlContent.BackColor = Color.FromArgb(243, 246, 251);
            pnlContent.Padding   = new Padding(0);

            // Button bar rebuild (right-align, inside pnlButtons)
            RebuildButtonBar();

            // Two-column body
            var pnlBody = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(243, 246, 251), Padding = new Padding(16, 10, 16, 10) };
            pnlContent.Controls.Add(pnlBody);

            // Right panel — actions
            var pnlRight = BuildActionPanel();
            pnlRight.Dock  = DockStyle.Right;
            pnlRight.Width = 300;
            pnlBody.Controls.Add(pnlRight);

            // Left scroll — summary + validation
            var pnlLeft = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(243, 246, 251), Padding = new Padding(0, 0, 12, 0) };
            pnlBody.Controls.Add(pnlLeft);

            var pnlSummary    = BuildSummaryCard();    pnlSummary.Dock    = DockStyle.Top; pnlLeft.Controls.Add(pnlSummary);
            var pnlValidation = BuildValidationCard(); pnlValidation.Dock = DockStyle.Top; pnlLeft.Controls.Add(pnlValidation);

            // Bring validation above summary (Top controls stack bottom-up)
            pnlLeft.Controls.SetChildIndex(pnlValidation, 0);
            pnlLeft.Controls.SetChildIndex(pnlSummary,    1);
        }

        private void RebuildButtonBar()
        {
            pnlButtons.Controls.Clear();

            var flow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                Width         = 320,
                Padding       = new Padding(0, 9, 8, 0),
                BackColor     = pnlButtons.BackColor,
            };

            var btnBack = new Button
            {
                Text      = "← ย้อนกลับ",
                Size      = new Size(90, 30),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9F),
                ForeColor = NavyDark,
                Margin    = new Padding(0, 0, 8, 0),
                Cursor    = Cursors.Hand,
            };
            btnBack.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnBack.Click += (s, e) => { this.DialogResult = DialogResult.Retry; this.Close(); };

            var btnDraft = new Button
            {
                Text      = "บันทึก Draft",
                Size      = new Size(100, 30),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9F),
                ForeColor = NavyDark,
                Margin    = new Padding(0, 0, 8, 0),
                Cursor    = Cursors.Hand,
            };
            btnDraft.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnDraft.Click += (s, e) => OnSaveDraftClick();

            var btnSubmit = new Button
            {
                Text      = "✔  ส่งอนุมัติ",
                Size      = new Size(110, 30),
                BackColor = Blue,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Margin    = new Padding(0),
                Cursor    = Cursors.Hand,
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += (s, e) => OnSubmitForApprovalClick();

            flow.Controls.Add(btnBack);
            flow.Controls.Add(btnDraft);
            flow.Controls.Add(btnSubmit);
            pnlButtons.Controls.Add(flow);

            // Hide old Designer buttons
            btnComplete.Visible = false;
            btnPrev.Visible     = false;
        }

        // ── Summary card ─────────────────────────────────────────────────────────

        private Panel BuildSummaryCard()
        {
            var card = MakeCard();
            card.Margin = new Padding(0, 10, 0, 0);

            var hdr = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.White };
            hdr.Controls.Add(new Label { Text = "📋  สรุปข้อมูล Template", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = NavyDark, AutoSize = true, Location = new Point(14, 9) });
            hdr.Paint += BorderBottom;
            card.Controls.Add(hdr);

            var grid = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, BackColor = Color.White, Padding = new Padding(14, 8, 14, 12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var rows = new[]
            {
                new { L = "ชื่อ Template",    V = _wizardState.TemplateName   ?? "-" },
                new { L = "หมวดหมู่",          V = _wizardState.Category       ?? "-" },
                new { L = "เจ้าของ",           V = _wizardState.Owner          ?? "-" },
                new { L = "SLA (วัน)",         V = _wizardState.SlaDays.ToString() },
                new { L = "Steps ที่ออกแบบ",  V = _wizardState.Steps.Count + " ขั้นตอน" },
                new { L = "Form Fields",       V = _wizardState.FormFields.Count + " field" },
                new { L = "Approvers",         V = _wizardState.Approvers.Count + " กลุ่ม" },
            };

            foreach (var row in rows)
            {
                grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                grid.Controls.Add(new Label { Text = row.L, Font = new Font("Segoe UI", 8.5F), ForeColor = GrayText,  AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Left, Padding = new Padding(0, 4, 8, 4) });
                grid.Controls.Add(new Label { Text = row.V, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = NavyDark, AutoSize = false, Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 4) });
            }
            card.Controls.Add(grid);
            card.Height = 36 + rows.Length * 26 + 20;
            return card;
        }

        // ── Validation card ──────────────────────────────────────────────────────

        private Panel BuildValidationCard()
        {
            var checks = RunValidation();

            var card = MakeCard();
            card.Margin = new Padding(0, 0, 0, 0);

            var hdr = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.White };
            hdr.Controls.Add(new Label { Text = "✅  Validation Check", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = NavyDark, AutoSize = true, Location = new Point(14, 9) });
            bool allOk = checks.All(c => c.Ok);
            var badge = new Label
            {
                Text      = allOk ? "✓ ผ่านทั้งหมด" : string.Format("⚠ ไม่ผ่าน {0} รายการ", checks.Count(c => !c.Ok)),
                Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = allOk ? GreenOk : OrangeWarn,
                BackColor = allOk ? Color.FromArgb(220, 252, 231) : Color.FromArgb(254, 243, 199),
                AutoSize  = true,
                Padding   = new Padding(8, 3, 8, 3),
                Location  = new Point(210, 8),
            };
            hdr.Controls.Add(badge);
            hdr.Paint += BorderBottom;
            card.Controls.Add(hdr);

            var stack = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = Color.White, Padding = new Padding(14, 8, 14, 12) };
            foreach (var ch in checks)
            {
                var row = new Panel { Height = 28, BackColor = Color.White, Width = 580, Margin = new Padding(0, 0, 0, 2) };
                var icon = new Label { Text = ch.Ok ? "✓" : "⚠", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = ch.Ok ? GreenOk : OrangeWarn, Location = new Point(0, 4), Size = new Size(22, 20) };
                var lbl  = new Label { Text = ch.Text, Font = new Font("Segoe UI", 9F), ForeColor = ch.Ok ? Color.FromArgb(30, 36, 55) : OrangeWarn, Location = new Point(26, 5), Size = new Size(500, 20) };
                row.Controls.Add(icon); row.Controls.Add(lbl);
                stack.Controls.Add(row);
            }
            card.Controls.Add(stack);
            card.Height = 36 + checks.Count * 30 + 24;
            return card;
        }

        private struct CheckItem { public bool Ok; public string Text; }

        private List<CheckItem> RunValidation()
        {
            var list = new List<CheckItem>();
            list.Add(new CheckItem { Ok = !string.IsNullOrWhiteSpace(_wizardState.TemplateName), Text = "ชื่อ Template ไม่ว่างเปล่า" });
            list.Add(new CheckItem { Ok = !string.IsNullOrWhiteSpace(_wizardState.Category),    Text = "ระบุหมวดหมู่แล้ว" });
            list.Add(new CheckItem { Ok = _wizardState.Steps.Count > 0,                          Text = string.Format("มี Step อย่างน้อย 1 รายการ (ปัจจุบัน {0})", _wizardState.Steps.Count) });
            list.Add(new CheckItem { Ok = _wizardState.FormFields.Count > 0,                     Text = string.Format("มี Form Field อย่างน้อย 1 รายการ (ปัจจุบัน {0})", _wizardState.FormFields.Count) });
            list.Add(new CheckItem { Ok = _wizardState.SlaDays > 0 && _wizardState.SlaDays <= 30, Text = string.Format("SLA อยู่ในช่วง 1-30 วัน (ปัจจุบัน {0} วัน)", _wizardState.SlaDays) });
            return list;
        }

        // ── Right panel — action buttons + info ──────────────────────────────────

        private Panel BuildActionPanel()
        {
            var pnl = new Panel { BackColor = Color.Transparent, Padding = new Padding(8, 0, 0, 0) };

            // Submit card
            var cardAction = MakeCard();
            cardAction.Dock   = DockStyle.Top;
            cardAction.Height = 220;
            cardAction.Margin = new Padding(0, 0, 0, 10);

            cardAction.Controls.Add(new Label { Text = "พร้อมส่งอนุมัติ?", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = NavyDark, AutoSize = true, Location = new Point(14, 14) });

            var lblDesc = new Label
            {
                Text    = "เมื่อส่งแล้ว Template จะเปลี่ยนสถานะเป็น\n\"รออนุมัติ\" และปรากฏใน Queue\nของ Workflow Owner Committee",
                Font    = new Font("Segoe UI", 8.5F),
                ForeColor = GrayText,
                Location  = new Point(14, 38),
                Size      = new Size(256, 60),
            };
            cardAction.Controls.Add(lblDesc);

            var btnSubmit = new Button
            {
                Text      = "✔  ส่งอนุมัติ Template นี้",
                Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Blue,
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(14, 110),
                Size      = new Size(260, 36),
                Cursor    = Cursors.Hand,
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += (s, e) => OnSubmitForApprovalClick();
            cardAction.Controls.Add(btnSubmit);

            var lnkDraft = new LinkLabel { Text = "💾  บันทึก Draft ก่อน (ยังไม่ส่งอนุมัติ)", Font = new Font("Segoe UI", 9F), Location = new Point(14, 158), AutoSize = true, LinkColor = GrayText };
            lnkDraft.LinkClicked += (s, e) => OnSaveDraftClick();
            cardAction.Controls.Add(lnkDraft);

            pnl.Controls.Add(cardAction);

            // Info card
            var cardInfo = MakeCard();
            cardInfo.Dock   = DockStyle.Top;
            cardInfo.Height = 130;

            var infoItems = new[]
            {
                "ต้องผ่านอย่างน้อย 2/3 เสียงของ Committee",
                "Approver จะได้รับ Notification ทันที",
                "สามารถถอนคืนได้ก่อน Approver ดำเนินการ",
            };
            int iy = 12;
            cardInfo.Controls.Add(new Label { Text = "ℹ  ขั้นตอนหลังส่ง", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Blue, AutoSize = true, Location = new Point(14, iy) });
            iy += 22;
            foreach (var info in infoItems)
            {
                cardInfo.Controls.Add(new Label { Text = "•  " + info, Font = new Font("Segoe UI", 8.5F), ForeColor = GrayText, Location = new Point(14, iy), Size = new Size(260, 18) });
                iy += 20;
            }
            pnl.Controls.Add(cardInfo);
            pnl.Controls.SetChildIndex(cardInfo,  0);
            pnl.Controls.SetChildIndex(cardAction, 1);

            return pnl;
        }

        // ════════════════════════════════════════════════════════════════════════
        // Actions
        // ════════════════════════════════════════════════════════════════════════

        private void OnSubmitForApprovalClick()
        {
            var errors = RunValidation().Where(c => !c.Ok).Select(c => c.Text).ToList();
            if (errors.Any())
            {
                MessageBox.Show(
                    "ไม่สามารถส่งอนุมัติได้ กรุณาแก้ไขรายการต่อไปนี้ก่อน:\n\n• " + string.Join("\n• ", errors),
                    "Validation ไม่ผ่าน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(
                    string.Format("ยืนยันส่ง Template '{0}' เข้า Approval Queue?", _wizardState.TemplateName),
                    "ยืนยันการส่งอนุมัติ", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != DialogResult.Yes) return;

            _wizardState.SubmitForApproval = true;
            _wizardState.IsPublished       = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnSaveDraftClick()
        {
            _wizardState.SubmitForApproval = false;
            _wizardState.IsPublished       = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // ════════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════════

        private static Panel MakeCard()
        {
            return new Panel { BackColor = Color.White, Margin = new Padding(0, 0, 0, 10) };
        }

        private static void BorderBottom(object sender, PaintEventArgs e)
        {
            var p = sender as Panel;
            if (p == null) return;
            using (var pen = new Pen(Color.FromArgb(235, 238, 244)))
                e.Graphics.DrawLine(pen, 0, p.Height - 1, p.Width, p.Height - 1);
        }
    }
}
