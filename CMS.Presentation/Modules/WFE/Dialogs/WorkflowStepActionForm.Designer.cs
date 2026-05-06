namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    partial class WorkflowStepActionForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Title bar ────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitleIcon;
        private System.Windows.Forms.Label lblTitleText;
        private System.Windows.Forms.Label lblTitleSub;
        private System.Windows.Forms.Label lblSLABadge;
        private System.Windows.Forms.Button btnClose;

        // ── Scrollable body ──────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlScrollOuter;
        private System.Windows.Forms.Panel pnlScrollContent;

        // ── Info cards row ───────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlInfoRow;
        private System.Windows.Forms.Panel pnlInfoLeft;
        private System.Windows.Forms.Panel pnlInfoRight;

        // ── Designer instruction ─────────────────────────────────────────────
        private System.Windows.Forms.Label lblInstructionHeader;
        private System.Windows.Forms.Panel pnlInstruction;
        private System.Windows.Forms.Label lblInstructionText;

        // ── Attachments ──────────────────────────────────────────────────────
        private System.Windows.Forms.Label lblAttachHeader;
        private System.Windows.Forms.Panel pnlAttachments;

        // ── Previous step history ────────────────────────────────────────────
        private System.Windows.Forms.Label lblHistoryHeader;
        private System.Windows.Forms.Panel pnlHistory;

        // ── Action selection ─────────────────────────────────────────────────
        private System.Windows.Forms.Label lblActionHeader;
        private System.Windows.Forms.Panel pnlActions;

        // ── Comment / reason ─────────────────────────────────────────────────
        private System.Windows.Forms.Label lblCommentHeader;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.Label lblCharCount;

        // ── Work Instruction warning ─────────────────────────────────────────
        private System.Windows.Forms.Panel pnlWIWarning;
        private System.Windows.Forms.Label lblWIWarningText;
        private System.Windows.Forms.Button btnOpenWI;

        // ── Footer ───────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnConfirm;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.pnlTitleBar       = new System.Windows.Forms.Panel();
            this.lblTitleIcon      = new System.Windows.Forms.Label();
            this.lblTitleText      = new System.Windows.Forms.Label();
            this.lblTitleSub       = new System.Windows.Forms.Label();
            this.lblSLABadge       = new System.Windows.Forms.Label();
            this.btnClose          = new System.Windows.Forms.Button();
            this.pnlScrollOuter    = new System.Windows.Forms.Panel();
            this.pnlScrollContent  = new System.Windows.Forms.Panel();
            this.pnlInfoRow        = new System.Windows.Forms.Panel();
            this.pnlInfoLeft       = new System.Windows.Forms.Panel();
            this.pnlInfoRight      = new System.Windows.Forms.Panel();
            this.lblInstructionHeader = new System.Windows.Forms.Label();
            this.pnlInstruction    = new System.Windows.Forms.Panel();
            this.lblInstructionText = new System.Windows.Forms.Label();
            this.lblAttachHeader   = new System.Windows.Forms.Label();
            this.pnlAttachments    = new System.Windows.Forms.Panel();
            this.lblHistoryHeader  = new System.Windows.Forms.Label();
            this.pnlHistory        = new System.Windows.Forms.Panel();
            this.lblActionHeader   = new System.Windows.Forms.Label();
            this.pnlActions        = new System.Windows.Forms.Panel();
            this.lblCommentHeader  = new System.Windows.Forms.Label();
            this.txtComment        = new System.Windows.Forms.TextBox();
            this.lblCharCount      = new System.Windows.Forms.Label();
            this.pnlWIWarning      = new System.Windows.Forms.Panel();
            this.lblWIWarningText  = new System.Windows.Forms.Label();
            this.btnOpenWI         = new System.Windows.Forms.Button();
            this.pnlFooter         = new System.Windows.Forms.Panel();
            this.btnCancel         = new System.Windows.Forms.Button();
            this.btnConfirm        = new System.Windows.Forms.Button();

            this.SuspendLayout();

            // ── Form ─────────────────────────────────────────────────────────
            this.Text            = "ดำเนินการ Step";
            this.ClientSize      = new System.Drawing.Size(760, 820);
            this.MinimumSize     = new System.Drawing.Size(680, 600);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.BackColor       = System.Drawing.Color.White;
            this.Load           += new System.EventHandler(this.WorkflowStepActionForm_Load);

            // ── Title bar ─────────────────────────────────────────────────────
            this.pnlTitleBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Height    = 56;
            this.pnlTitleBar.BackColor = System.Drawing.Color.White;
            this.pnlTitleBar.Name      = "pnlTitleBar";

            this.lblTitleIcon.Text      = "✓";
            this.lblTitleIcon.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitleIcon.ForeColor = System.Drawing.Color.FromArgb(23, 133, 49);
            this.lblTitleIcon.BackColor = System.Drawing.Color.FromArgb(220, 252, 231);
            this.lblTitleIcon.Location  = new System.Drawing.Point(16, 12);
            this.lblTitleIcon.Size      = new System.Drawing.Size(32, 32);
            this.lblTitleIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTitleIcon.Name      = "lblTitleIcon";

            this.lblTitleText.Text      = "ดำเนินการ Step — อนุมัติ KB Article";
            this.lblTitleText.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitleText.ForeColor = System.Drawing.Color.FromArgb(22, 36, 62);
            this.lblTitleText.Location  = new System.Drawing.Point(56, 10);
            this.lblTitleText.AutoSize  = true;
            this.lblTitleText.Name      = "lblTitleText";

            this.lblTitleSub.Text      = "WFI-2569-00012  ·  Step 2 จาก 3  ·  ผู้จัดการตรวจสอบเนื้อหา";
            this.lblTitleSub.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTitleSub.ForeColor = System.Drawing.Color.FromArgb(110, 120, 140);
            this.lblTitleSub.Location  = new System.Drawing.Point(56, 32);
            this.lblTitleSub.AutoSize  = true;
            this.lblTitleSub.Name      = "lblTitleSub";

            this.lblSLABadge.Text      = "⏱  เหลือ 3 วัน";
            this.lblSLABadge.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSLABadge.ForeColor = System.Drawing.Color.FromArgb(197, 120, 16);
            this.lblSLABadge.BackColor = System.Drawing.Color.FromArgb(254, 243, 199);
            this.lblSLABadge.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblSLABadge.Location  = new System.Drawing.Point(600, 18);
            this.lblSLABadge.Size      = new System.Drawing.Size(100, 22);
            this.lblSLABadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSLABadge.Name      = "lblSLABadge";

            this.btnClose.Text      = "×";
            this.btnClose.Font      = new System.Drawing.Font("Segoe UI", 14F);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(140, 150, 165);
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.Location  = new System.Drawing.Point(724, 12);
            this.btnClose.Size      = new System.Drawing.Size(32, 32);
            this.btnClose.Name      = "btnClose";
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.Click    += new System.EventHandler(this.btnCancel_Click);

            this.pnlTitleBar.Controls.Add(this.lblTitleIcon);
            this.pnlTitleBar.Controls.Add(this.lblTitleText);
            this.pnlTitleBar.Controls.Add(this.lblTitleSub);
            this.pnlTitleBar.Controls.Add(this.lblSLABadge);
            this.pnlTitleBar.Controls.Add(this.btnClose);

            // ── Footer ────────────────────────────────────────────────────────
            this.pnlFooter.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height    = 56;
            this.pnlFooter.BackColor = System.Drawing.Color.White;
            this.pnlFooter.Name      = "pnlFooter";

            this.btnCancel.Text      = "ยกเลิก";
            this.btnCancel.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnCancel.Location  = new System.Drawing.Point(560, 12);
            this.btnCancel.Size      = new System.Drawing.Size(88, 32);
            this.btnCancel.Name      = "btnCancel";
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.btnCancel.Click    += new System.EventHandler(this.btnCancel_Click);

            this.btnConfirm.Text      = "  ✓  ยืนยันการดำเนินการ";
            this.btnConfirm.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnConfirm.ForeColor = System.Drawing.Color.White;
            this.btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirm.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnConfirm.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnConfirm.Location  = new System.Drawing.Point(656, 12);
            this.btnConfirm.Size      = new System.Drawing.Size(188, 32);
            this.btnConfirm.Name      = "btnConfirm";
            this.btnConfirm.FlatAppearance.BorderSize = 0;
            this.btnConfirm.Click    += new System.EventHandler(this.btnComplete_Click);

            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Controls.Add(this.btnConfirm);

            // ── Scrollable body ───────────────────────────────────────────────
            this.pnlScrollOuter.Dock       = System.Windows.Forms.DockStyle.Fill;
            this.pnlScrollOuter.AutoScroll = true;
            this.pnlScrollOuter.BackColor  = System.Drawing.Color.FromArgb(248, 249, 252);
            this.pnlScrollOuter.Name       = "pnlScrollOuter";

            this.pnlScrollContent.AutoSize     = true;
            this.pnlScrollContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlScrollContent.BackColor    = System.Drawing.Color.FromArgb(248, 249, 252);
            this.pnlScrollContent.Padding      = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.pnlScrollContent.Name         = "pnlScrollContent";

            // ── Info row ──────────────────────────────────────────────────────
            this.pnlInfoRow.Size      = new System.Drawing.Size(720, 110);
            this.pnlInfoRow.Location  = new System.Drawing.Point(0, 0);
            this.pnlInfoRow.BackColor = System.Drawing.Color.Transparent;
            this.pnlInfoRow.Name      = "pnlInfoRow";

            this.pnlInfoLeft.Location  = new System.Drawing.Point(0, 0);
            this.pnlInfoLeft.Size      = new System.Drawing.Size(350, 106);
            this.pnlInfoLeft.BackColor = System.Drawing.Color.White;
            this.pnlInfoLeft.Name      = "pnlInfoLeft";

            this.pnlInfoRight.Location  = new System.Drawing.Point(358, 0);
            this.pnlInfoRight.Size      = new System.Drawing.Size(362, 106);
            this.pnlInfoRight.BackColor = System.Drawing.Color.White;
            this.pnlInfoRight.Name      = "pnlInfoRight";

            this.pnlInfoRow.Controls.Add(this.pnlInfoLeft);
            this.pnlInfoRow.Controls.Add(this.pnlInfoRight);

            // ── Designer instruction ──────────────────────────────────────────
            this.lblInstructionHeader.Text      = "คำแนะนำจากผู้ออกแบบ";
            this.lblInstructionHeader.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblInstructionHeader.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblInstructionHeader.Location  = new System.Drawing.Point(0, 122);
            this.lblInstructionHeader.AutoSize  = true;
            this.lblInstructionHeader.Name      = "lblInstructionHeader";

            this.pnlInstruction.Location  = new System.Drawing.Point(0, 142);
            this.pnlInstruction.Size      = new System.Drawing.Size(720, 72);
            this.pnlInstruction.BackColor = System.Drawing.Color.FromArgb(240, 245, 255);
            this.pnlInstruction.Name      = "pnlInstruction";

            this.lblInstructionText.Text      = "(คำแนะนำจากผู้ออกแบบ Workflow)";
            this.lblInstructionText.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblInstructionText.ForeColor = System.Drawing.Color.FromArgb(44, 62, 100);
            this.lblInstructionText.Location  = new System.Drawing.Point(14, 10);
            this.lblInstructionText.Size      = new System.Drawing.Size(692, 52);
            this.lblInstructionText.Name      = "lblInstructionText";
            this.pnlInstruction.Controls.Add(this.lblInstructionText);

            // ── Attachments ───────────────────────────────────────────────────
            this.lblAttachHeader.Text      = "ไฟล์แนบ";
            this.lblAttachHeader.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAttachHeader.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblAttachHeader.Location  = new System.Drawing.Point(0, 226);
            this.lblAttachHeader.AutoSize  = true;
            this.lblAttachHeader.Name      = "lblAttachHeader";

            this.pnlAttachments.Location  = new System.Drawing.Point(0, 246);
            this.pnlAttachments.Size      = new System.Drawing.Size(720, 10);
            this.pnlAttachments.BackColor = System.Drawing.Color.Transparent;
            this.pnlAttachments.AutoSize  = true;
            this.pnlAttachments.Name      = "pnlAttachments";

            // ── History ───────────────────────────────────────────────────────
            this.lblHistoryHeader.Text      = "ประวัติ STEP ก่อนหน้า";
            this.lblHistoryHeader.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblHistoryHeader.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblHistoryHeader.Location  = new System.Drawing.Point(0, 260);
            this.lblHistoryHeader.AutoSize  = true;
            this.lblHistoryHeader.Name      = "lblHistoryHeader";

            this.pnlHistory.Location  = new System.Drawing.Point(0, 280);
            this.pnlHistory.Size      = new System.Drawing.Size(720, 44);
            this.pnlHistory.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.pnlHistory.Name      = "pnlHistory";

            // ── Action selection ──────────────────────────────────────────────
            this.lblActionHeader.Text      = "การดำเนินการ";
            this.lblActionHeader.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblActionHeader.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblActionHeader.Location  = new System.Drawing.Point(0, 336);
            this.lblActionHeader.AutoSize  = true;
            this.lblActionHeader.Name      = "lblActionHeader";

            this.pnlActions.Location  = new System.Drawing.Point(0, 356);
            this.pnlActions.Size      = new System.Drawing.Size(720, 72);
            this.pnlActions.BackColor = System.Drawing.Color.Transparent;
            this.pnlActions.Name      = "pnlActions";

            // ── Comment ───────────────────────────────────────────────────────
            this.lblCommentHeader.Text      = "หมายเหตุ / เหตุผล  (บังคับถ้าปฏิเสธ)";
            this.lblCommentHeader.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCommentHeader.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblCommentHeader.Location  = new System.Drawing.Point(0, 440);
            this.lblCommentHeader.AutoSize  = true;
            this.lblCommentHeader.Name      = "lblCommentHeader";

            this.txtComment.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtComment.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtComment.Multiline   = true;
            this.txtComment.ScrollBars  = System.Windows.Forms.ScrollBars.Vertical;
            this.txtComment.Location    = new System.Drawing.Point(0, 460);
            this.txtComment.Size        = new System.Drawing.Size(720, 80);
            this.txtComment.MaxLength   = 500;
            this.txtComment.Name        = "txtComment";

            this.lblCharCount.Text      = "0 / 500 ตัวอักษร";
            this.lblCharCount.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCharCount.ForeColor = System.Drawing.Color.FromArgb(140, 150, 165);
            this.lblCharCount.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblCharCount.Location  = new System.Drawing.Point(580, 544);
            this.lblCharCount.Size      = new System.Drawing.Size(140, 18);
            this.lblCharCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblCharCount.Name      = "lblCharCount";

            // ── WI Warning ────────────────────────────────────────────────────
            this.pnlWIWarning.Location  = new System.Drawing.Point(0, 572);
            this.pnlWIWarning.Size      = new System.Drawing.Size(720, 44);
            this.pnlWIWarning.BackColor = System.Drawing.Color.FromArgb(255, 251, 235);
            this.pnlWIWarning.Name      = "pnlWIWarning";
            this.pnlWIWarning.Visible   = false;

            this.lblWIWarningText.Text      = "⚠  Work Instruction บังคับ: (ยังไม่ทำครบ)";
            this.lblWIWarningText.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblWIWarningText.ForeColor = System.Drawing.Color.FromArgb(146, 64, 14);
            this.lblWIWarningText.Location  = new System.Drawing.Point(12, 12);
            this.lblWIWarningText.AutoSize  = true;
            this.lblWIWarningText.Name      = "lblWIWarningText";

            this.btnOpenWI.Text      = "เปิด WI";
            this.btnOpenWI.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnOpenWI.ForeColor = System.Drawing.Color.FromArgb(146, 64, 14);
            this.btnOpenWI.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenWI.BackColor = System.Drawing.Color.White;
            this.btnOpenWI.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnOpenWI.Location  = new System.Drawing.Point(644, 10);
            this.btnOpenWI.Size      = new System.Drawing.Size(68, 24);
            this.btnOpenWI.Name      = "btnOpenWI";
            this.btnOpenWI.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(146, 64, 14);

            this.pnlWIWarning.Controls.Add(this.lblWIWarningText);
            this.pnlWIWarning.Controls.Add(this.btnOpenWI);

            // ── Assemble scroll content ───────────────────────────────────────
            this.pnlScrollContent.Controls.Add(this.pnlInfoRow);
            this.pnlScrollContent.Controls.Add(this.lblInstructionHeader);
            this.pnlScrollContent.Controls.Add(this.pnlInstruction);
            this.pnlScrollContent.Controls.Add(this.lblAttachHeader);
            this.pnlScrollContent.Controls.Add(this.pnlAttachments);
            this.pnlScrollContent.Controls.Add(this.lblHistoryHeader);
            this.pnlScrollContent.Controls.Add(this.pnlHistory);
            this.pnlScrollContent.Controls.Add(this.lblActionHeader);
            this.pnlScrollContent.Controls.Add(this.pnlActions);
            this.pnlScrollContent.Controls.Add(this.lblCommentHeader);
            this.pnlScrollContent.Controls.Add(this.txtComment);
            this.pnlScrollContent.Controls.Add(this.lblCharCount);
            this.pnlScrollContent.Controls.Add(this.pnlWIWarning);

            this.pnlScrollOuter.Controls.Add(this.pnlScrollContent);

            // ── Form controls ─────────────────────────────────────────────────
            this.Controls.Add(this.pnlScrollOuter);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlTitleBar);

            this.ResumeLayout(false);
        }
    }
}
