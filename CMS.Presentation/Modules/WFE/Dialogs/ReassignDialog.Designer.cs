namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    partial class ReassignDialog
    {
        private System.ComponentModel.IContainer components = null;

        // ── Title bar ────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitleIcon;
        private System.Windows.Forms.Label lblTitleText;
        private System.Windows.Forms.Label lblTitleSub;
        private System.Windows.Forms.Button btnClose;

        // ── Scrollable body ──────────────────────────────────────────────────
        private NonAutoScrollPanel pnlScrollOuter;
        private System.Windows.Forms.Panel pnlScrollContent;

        // ── Section 1: Current assignee + reason ─────────────────────────────
        private System.Windows.Forms.Panel pnlSection1;
        private System.Windows.Forms.Panel pnlCurrentAssignee;
        private System.Windows.Forms.Panel pnlReasonBox;
        private System.Windows.Forms.Label lblCurrentHeader;
        private System.Windows.Forms.Label lblReasonHeader;
        private System.Windows.Forms.ComboBox cmbReason;

        // ── Section 2: Reassign mode ─────────────────────────────────────────
        private System.Windows.Forms.Label lblModeHeader;
        private System.Windows.Forms.Panel pnlModes;

        // ── Section 3: Candidate list ─────────────────────────────────────────
        private System.Windows.Forms.Label lblCandidateHeader;
        private System.Windows.Forms.Panel pnlSearchRow;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.ComboBox cmbRole;
        private System.Windows.Forms.CheckBox chkSortByMatch;
        private System.Windows.Forms.Panel pnlCandidateList;

        // ── Section 4: Message ───────────────────────────────────────────────
        private System.Windows.Forms.Label lblMessageHeader;
        private System.Windows.Forms.TextBox txtMessage;

        // ── Section 5: Info banner ───────────────────────────────────────────
        private System.Windows.Forms.Panel pnlInfoBanner;
        private System.Windows.Forms.Label lblInfoBanner;

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

            this.pnlTitleBar        = new System.Windows.Forms.Panel();
            this.lblTitleIcon       = new System.Windows.Forms.Label();
            this.lblTitleText       = new System.Windows.Forms.Label();
            this.lblTitleSub        = new System.Windows.Forms.Label();
            this.btnClose           = new System.Windows.Forms.Button();
            this.pnlScrollOuter     = new NonAutoScrollPanel();
            this.pnlScrollContent   = new System.Windows.Forms.Panel();
            this.pnlSection1        = new System.Windows.Forms.Panel();
            this.pnlCurrentAssignee = new System.Windows.Forms.Panel();
            this.pnlReasonBox       = new System.Windows.Forms.Panel();
            this.lblCurrentHeader   = new System.Windows.Forms.Label();
            this.lblReasonHeader    = new System.Windows.Forms.Label();
            this.cmbReason          = new System.Windows.Forms.ComboBox();
            this.lblModeHeader      = new System.Windows.Forms.Label();
            this.pnlModes           = new System.Windows.Forms.Panel();
            this.lblCandidateHeader = new System.Windows.Forms.Label();
            this.pnlSearchRow       = new System.Windows.Forms.Panel();
            this.txtSearch          = new System.Windows.Forms.TextBox();
            this.cmbRole            = new System.Windows.Forms.ComboBox();
            this.chkSortByMatch     = new System.Windows.Forms.CheckBox();
            this.pnlCandidateList   = new System.Windows.Forms.Panel();
            this.lblMessageHeader   = new System.Windows.Forms.Label();
            this.txtMessage         = new System.Windows.Forms.TextBox();
            this.pnlInfoBanner      = new System.Windows.Forms.Panel();
            this.lblInfoBanner      = new System.Windows.Forms.Label();
            this.pnlFooter          = new System.Windows.Forms.Panel();
            this.btnCancel          = new System.Windows.Forms.Button();
            this.btnConfirm         = new System.Windows.Forms.Button();

            this.SuspendLayout();

            // ── Form ─────────────────────────────────────────────────────────
            this.Text            = "Reassign / Escalate งาน";
            this.ClientSize      = new System.Drawing.Size(780, 600);
            this.MinimumSize     = new System.Drawing.Size(680, 600);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.BackColor       = System.Drawing.Color.White;
            this.Load           += new System.EventHandler(this.ReassignDialog_Load);

            // ── Title bar ─────────────────────────────────────────────────────
            this.pnlTitleBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Size      = new System.Drawing.Size(780, 56);
            this.pnlTitleBar.BackColor = System.Drawing.Color.FromArgb(22, 36, 62);
            this.pnlTitleBar.Name      = "pnlTitleBar";

            this.lblTitleIcon.Text      = "⇄";
            this.lblTitleIcon.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitleIcon.ForeColor = System.Drawing.Color.White;
            this.lblTitleIcon.BackColor = System.Drawing.Color.FromArgb(59, 91, 219);
            this.lblTitleIcon.Location  = new System.Drawing.Point(16, 12);
            this.lblTitleIcon.Size      = new System.Drawing.Size(32, 32);
            this.lblTitleIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTitleIcon.Name      = "lblTitleIcon";

            this.lblTitleText.Text      = "Reassign / Escalate งาน";
            this.lblTitleText.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitleText.ForeColor = System.Drawing.Color.White;
            this.lblTitleText.Location  = new System.Drawing.Point(56, 10);
            this.lblTitleText.AutoSize  = true;
            this.lblTitleText.Name      = "lblTitleText";

            this.lblTitleSub.Text      = "WFI-2569-00012  ·  Step: ผู้จัดการตรวจสอบเนื้อหา";
            this.lblTitleSub.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTitleSub.ForeColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.lblTitleSub.Location  = new System.Drawing.Point(56, 32);
            this.lblTitleSub.AutoSize  = true;
            this.lblTitleSub.Name      = "lblTitleSub";

            this.btnClose.Text      = "✕";
            this.btnClose.Font      = new System.Drawing.Font("Segoe UI", 14F);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.Location  = new System.Drawing.Point(744, 12);
            this.btnClose.Size      = new System.Drawing.Size(32, 32);
            this.btnClose.Name      = "btnClose";
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.Click    += new System.EventHandler(this.btnCancel_Click);
            this.pnlTitleBar.Controls.Add(this.lblTitleIcon);
            this.pnlTitleBar.Controls.Add(this.lblTitleText);
            this.pnlTitleBar.Controls.Add(this.lblTitleSub);
            this.pnlTitleBar.Controls.Add(this.btnClose);

            // ── Footer ────────────────────────────────────────────────────────
            this.pnlFooter.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Size      = new System.Drawing.Size(780, 56);
            this.pnlFooter.BackColor = System.Drawing.Color.White;
            this.pnlFooter.Name      = "pnlFooter";

            this.btnCancel.Text      = "ยกเลิก";
            this.btnCancel.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnCancel.Location  = new System.Drawing.Point(500, 12);
            this.btnCancel.Size      = new System.Drawing.Size(88, 32);
            this.btnCancel.Name      = "btnCancel";
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.btnCancel.Click    += new System.EventHandler(this.btnCancel_Click);

            this.btnConfirm.Text      = "  ✓  ยืนยัน Reassign";
            this.btnConfirm.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnConfirm.ForeColor = System.Drawing.Color.White;
            this.btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirm.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnConfirm.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnConfirm.Location  = new System.Drawing.Point(596, 12);
            this.btnConfirm.Size      = new System.Drawing.Size(168, 32);
            this.btnConfirm.Name      = "btnConfirm";
            this.btnConfirm.FlatAppearance.BorderSize = 0;
            this.btnConfirm.Click    += new System.EventHandler(this.btnConfirm_Click);
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Controls.Add(this.btnConfirm);

            // ── Scrollable body ───────────────────────────────────────────────
            this.pnlScrollOuter.Location   = new System.Drawing.Point(0, 0);
            this.pnlScrollOuter.Size       = new System.Drawing.Size(780, 544);
            this.pnlScrollOuter.Anchor     = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.pnlScrollOuter.AutoScroll = true;
            this.pnlScrollOuter.BackColor  = System.Drawing.Color.FromArgb(248, 249, 252);
            this.pnlScrollOuter.Name       = "pnlScrollOuter";

            this.pnlScrollContent.AutoSize  = false;
            this.pnlScrollContent.BackColor = System.Drawing.Color.FromArgb(248, 249, 252);
            this.pnlScrollContent.Padding      = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.pnlScrollContent.Name         = "pnlScrollContent";

            // ── Section 1: Current assignee + reason ─────────────────────────
            this.pnlSection1.Location  = new System.Drawing.Point(16, 70);
            this.pnlSection1.Size      = new System.Drawing.Size(720, 100);
            this.pnlSection1.BackColor = System.Drawing.Color.Transparent;
            this.pnlSection1.Name      = "pnlSection1";

            this.pnlCurrentAssignee.Location  = new System.Drawing.Point(0, 0);
            this.pnlCurrentAssignee.Size      = new System.Drawing.Size(350, 96);
            this.pnlCurrentAssignee.BackColor = System.Drawing.Color.White;
            this.pnlCurrentAssignee.Name      = "pnlCurrentAssignee";

            this.pnlReasonBox.Location  = new System.Drawing.Point(358, 0);
            this.pnlReasonBox.Size      = new System.Drawing.Size(362, 96);
            this.pnlReasonBox.BackColor = System.Drawing.Color.White;
            this.pnlReasonBox.Name      = "pnlReasonBox";

            this.lblCurrentHeader.Text      = "ผู้รับผิดชอบปัจจุบัน";
            this.lblCurrentHeader.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCurrentHeader.ForeColor = System.Drawing.Color.FromArgb(110, 120, 140);
            this.lblCurrentHeader.Location  = new System.Drawing.Point(12, 10);
            this.lblCurrentHeader.AutoSize  = true;
            this.lblCurrentHeader.Name      = "lblCurrentHeader";
            this.pnlCurrentAssignee.Controls.Add(this.lblCurrentHeader);

            this.lblReasonHeader.Text      = "เหตุผลที่ REASSIGN";
            this.lblReasonHeader.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblReasonHeader.ForeColor = System.Drawing.Color.FromArgb(110, 120, 140);
            this.lblReasonHeader.Location  = new System.Drawing.Point(12, 10);
            this.lblReasonHeader.AutoSize  = true;
            this.lblReasonHeader.Name      = "lblReasonHeader";

            this.cmbReason.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbReason.FlatStyle     = System.Windows.Forms.FlatStyle.Flat;
            this.cmbReason.Font          = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbReason.Location      = new System.Drawing.Point(12, 32);
            this.cmbReason.Size          = new System.Drawing.Size(338, 28);
            this.cmbReason.Name          = "cmbReason";
            this.cmbReason.Items.AddRange(new object[] {
                "ผู้รับผิดชอบไม่สามารถดำเนินการได้",
                "เกิน SLA — ต้องการ Escalate",
                "ผู้รับผิดชอบลาออก / ลาพักร้อน",
                "ต้องการผู้เชี่ยวชาญเฉพาะด้าน",
                "อื่นๆ" });
            this.cmbReason.SelectedIndex = 0;

            this.pnlReasonBox.Controls.Add(this.lblReasonHeader);
            this.pnlReasonBox.Controls.Add(this.cmbReason);
            this.pnlSection1.Controls.Add(this.pnlCurrentAssignee);
            this.pnlSection1.Controls.Add(this.pnlReasonBox);

            // ── Section 2: Mode ───────────────────────────────────────────────
            this.lblModeHeader.Text      = "รูปแบบการ REASSIGN";
            this.lblModeHeader.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblModeHeader.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblModeHeader.Location  = new System.Drawing.Point(16, 186);
            this.lblModeHeader.AutoSize  = true;
            this.lblModeHeader.Name      = "lblModeHeader";
 
            this.pnlModes.Location  = new System.Drawing.Point(16, 206);
            this.pnlModes.Size      = new System.Drawing.Size(720, 72);
            this.pnlModes.BackColor = System.Drawing.Color.Transparent;
            this.pnlModes.Name      = "pnlModes";

            // ── Section 3: Candidate ──────────────────────────────────────────
            this.lblCandidateHeader.Text      = "เลือกผู้รับผิดชอบใหม่";
            this.lblCandidateHeader.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCandidateHeader.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblCandidateHeader.Location  = new System.Drawing.Point(16, 294);
            this.lblCandidateHeader.AutoSize  = true;
            this.lblCandidateHeader.Name      = "lblCandidateHeader";
 
            this.pnlSearchRow.Location  = new System.Drawing.Point(16, 314);
            this.pnlSearchRow.Size      = new System.Drawing.Size(720, 36);
            this.pnlSearchRow.BackColor = System.Drawing.Color.Transparent;
            this.pnlSearchRow.Name      = "pnlSearchRow";

            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtSearch.ForeColor   = System.Drawing.Color.FromArgb(140, 150, 165);
            this.txtSearch.Text        = "🔍  ค้นหาชื่อหรือ AD Group...";
            this.txtSearch.Location    = new System.Drawing.Point(0, 4);
            this.txtSearch.Size        = new System.Drawing.Size(360, 28);
            this.txtSearch.Name        = "txtSearch";

            this.cmbRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRole.FlatStyle     = System.Windows.Forms.FlatStyle.Flat;
            this.cmbRole.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbRole.Location      = new System.Drawing.Point(368, 4);
            this.cmbRole.Size          = new System.Drawing.Size(180, 28);
            this.cmbRole.Name          = "cmbRole";

            this.chkSortByMatch.Text      = "เรียงตาม Match Score";
            this.chkSortByMatch.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.chkSortByMatch.ForeColor = System.Drawing.Color.FromArgb(44, 52, 70);
            this.chkSortByMatch.Location  = new System.Drawing.Point(556, 6);
            this.chkSortByMatch.AutoSize  = true;
            this.chkSortByMatch.Checked   = true;
            this.chkSortByMatch.Name      = "chkSortByMatch";

            this.pnlSearchRow.Controls.Add(this.txtSearch);
            this.pnlSearchRow.Controls.Add(this.cmbRole);
            this.pnlSearchRow.Controls.Add(this.chkSortByMatch);

            this.pnlCandidateList.Location  = new System.Drawing.Point(16, 358);
            this.pnlCandidateList.Size      = new System.Drawing.Size(720, 10);
            this.pnlCandidateList.BackColor = System.Drawing.Color.Transparent;
            this.pnlCandidateList.AutoSize  = true;
            this.pnlCandidateList.Name      = "pnlCandidateList";

            // ── Section 4: Message ────────────────────────────────────────────
            this.lblMessageHeader.Text      = "ข้อความถึงผู้รับใหม่";
            this.lblMessageHeader.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblMessageHeader.ForeColor = System.Drawing.Color.FromArgb(30, 36, 55);
            this.lblMessageHeader.Location  = new System.Drawing.Point(16, 560);
            this.lblMessageHeader.AutoSize  = true;
            this.lblMessageHeader.Name      = "lblMessageHeader";

            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMessage.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtMessage.Multiline   = true;
            this.txtMessage.ScrollBars  = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Location    = new System.Drawing.Point(16, 580);
            this.txtMessage.Size        = new System.Drawing.Size(720, 80);
            this.txtMessage.Name        = "txtMessage";

            // ── Section 5: Info banner ────────────────────────────────────────
            this.pnlInfoBanner.Location  = new System.Drawing.Point(16, 672);
            this.pnlInfoBanner.Size      = new System.Drawing.Size(720, 44);
            this.pnlInfoBanner.BackColor = System.Drawing.Color.FromArgb(240, 245, 255);
            this.pnlInfoBanner.Name      = "pnlInfoBanner";

            this.lblInfoBanner.Text      = "ⓘ  การแจ้งเตือน: ระบบจะส่ง Email + In-App แจ้งผู้รับใหม่, ผู้เดิม, และผู้ยื่นคำขอ · บันทึก Audit Log พร้อมเหตุผล";
            this.lblInfoBanner.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoBanner.ForeColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.lblInfoBanner.Location  = new System.Drawing.Point(12, 12);
            this.lblInfoBanner.Size      = new System.Drawing.Size(696, 20);
            this.lblInfoBanner.AutoEllipsis = true;
            this.lblInfoBanner.Name      = "lblInfoBanner";
            this.pnlInfoBanner.Controls.Add(this.lblInfoBanner);

            // ── Assemble scroll content ───────────────────────────────────────
            this.pnlScrollContent.Controls.Add(this.pnlSection1);
            this.pnlScrollContent.Controls.Add(this.lblModeHeader);
            this.pnlScrollContent.Controls.Add(this.pnlModes);
            this.pnlScrollContent.Controls.Add(this.lblCandidateHeader);
            this.pnlScrollContent.Controls.Add(this.pnlSearchRow);
            this.pnlScrollContent.Controls.Add(this.pnlCandidateList);
            this.pnlScrollContent.Controls.Add(this.lblMessageHeader);
            this.pnlScrollContent.Controls.Add(this.txtMessage);
            this.pnlScrollContent.Controls.Add(this.pnlInfoBanner);

            this.pnlScrollOuter.Controls.Add(this.pnlScrollContent);

            // ── Form controls ─────────────────────────────────────────────────
            this.Controls.Add(this.pnlTitleBar);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlScrollOuter);

            this.ResumeLayout(false);
        }
    }
}
