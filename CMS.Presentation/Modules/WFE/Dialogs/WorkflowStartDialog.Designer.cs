namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    partial class WorkflowStartDialog
    {
        private System.ComponentModel.IContainer components = null;

        // ── Title bar ────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitleIcon;
        private System.Windows.Forms.Label lblTitleText;
        private System.Windows.Forms.Label lblTitleSub;
        private System.Windows.Forms.Button btnClose;

        // ── Body (left + right) ──────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Panel pnlDivider;

        // ── Left: Template list ──────────────────────────────────────────────
        private System.Windows.Forms.Label lblStep1;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Panel pnlTemplateList;

        // ── Right: Selected template preview ────────────────────────────────
        private System.Windows.Forms.Label lblStep2;
        private System.Windows.Forms.Panel pnlTemplatePreview;

        // ── Right: Context form ──────────────────────────────────────────────
        private System.Windows.Forms.Label lblStep3;
        private System.Windows.Forms.Label lblTitleField;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblPriority;
        private System.Windows.Forms.ComboBox cmbPriority;
        private System.Windows.Forms.Label lblCRAF;
        private System.Windows.Forms.ComboBox cmbCRAF;
        private System.Windows.Forms.Label lblEntity;
        private System.Windows.Forms.Panel pnlEntityBadge;
        private System.Windows.Forms.Label lblEntityValue;

        // ── Footer ───────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Label lblFooterHint;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnStart;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.pnlTitleBar      = new System.Windows.Forms.Panel();
            this.lblTitleIcon     = new System.Windows.Forms.Label();
            this.lblTitleText     = new System.Windows.Forms.Label();
            this.lblTitleSub      = new System.Windows.Forms.Label();
            this.btnClose         = new System.Windows.Forms.Button();
            this.pnlBody          = new System.Windows.Forms.Panel();
            this.pnlLeft          = new System.Windows.Forms.Panel();
            this.pnlRight         = new System.Windows.Forms.Panel();
            this.pnlDivider       = new System.Windows.Forms.Panel();
            this.lblStep1         = new System.Windows.Forms.Label();
            this.txtSearch        = new System.Windows.Forms.TextBox();
            this.pnlTemplateList  = new System.Windows.Forms.Panel();
            this.lblStep2         = new System.Windows.Forms.Label();
            this.pnlTemplatePreview = new System.Windows.Forms.Panel();
            this.lblStep3         = new System.Windows.Forms.Label();
            this.lblTitleField    = new System.Windows.Forms.Label();
            this.txtTitle         = new System.Windows.Forms.TextBox();
            this.lblDescription   = new System.Windows.Forms.Label();
            this.txtDescription   = new System.Windows.Forms.TextBox();
            this.lblPriority      = new System.Windows.Forms.Label();
            this.cmbPriority      = new System.Windows.Forms.ComboBox();
            this.lblCRAF          = new System.Windows.Forms.Label();
            this.cmbCRAF          = new System.Windows.Forms.ComboBox();
            this.lblEntity        = new System.Windows.Forms.Label();
            this.pnlEntityBadge   = new System.Windows.Forms.Panel();
            this.lblEntityValue   = new System.Windows.Forms.Label();
            this.pnlFooter        = new System.Windows.Forms.Panel();
            this.lblFooterHint    = new System.Windows.Forms.Label();
            this.btnCancel        = new System.Windows.Forms.Button();
            this.btnStart         = new System.Windows.Forms.Button();

            this.SuspendLayout();

            // ── Form ─────────────────────────────────────────────────────────
            this.Text            = "เริ่มใช้งาน Workflow";
            this.ClientSize      = new System.Drawing.Size(1020, 720);
            this.MinimumSize     = new System.Drawing.Size(900, 640);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode   = System.Windows.Forms.AutoScaleMode.Font;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.BackColor       = System.Drawing.Color.White;
            this.Load           += new System.EventHandler(this.WorkflowStartDialog_Load);

            // ── Title bar ─────────────────────────────────────────────────────
            this.pnlTitleBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Size      = new System.Drawing.Size(1020, 52);
            this.pnlTitleBar.BackColor = System.Drawing.Color.FromArgb(22, 36, 62);
            this.pnlTitleBar.Name      = "pnlTitleBar";

            this.lblTitleIcon.Text      = "W";
            this.lblTitleIcon.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitleIcon.ForeColor = System.Drawing.Color.White;
            this.lblTitleIcon.BackColor = System.Drawing.Color.FromArgb(59, 91, 219);
            this.lblTitleIcon.Location  = new System.Drawing.Point(14, 10);
            this.lblTitleIcon.Size      = new System.Drawing.Size(32, 32);
            this.lblTitleIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTitleIcon.Name      = "lblTitleIcon";

            this.lblTitleText.Text      = "เริ่มใช้งาน Workflow";
            this.lblTitleText.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitleText.ForeColor = System.Drawing.Color.White;
            this.lblTitleText.Location  = new System.Drawing.Point(54, 8);
            this.lblTitleText.AutoSize  = true;
            this.lblTitleText.Name      = "lblTitleText";

            this.lblTitleSub.Text      = "เลือก Template และกรอกข้อมูลเพื่อเริ่มกระบวนการ";
            this.lblTitleSub.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTitleSub.ForeColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.lblTitleSub.Location  = new System.Drawing.Point(54, 30);
            this.lblTitleSub.AutoSize  = true;
            this.lblTitleSub.Name      = "lblTitleSub";

            this.btnClose.Text      = "×";
            this.btnClose.Font      = new System.Drawing.Font("Segoe UI", 14F);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(180, 200, 235);
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.Location  = new System.Drawing.Point(984, 8);
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
            this.pnlFooter.Size      = new System.Drawing.Size(1020, 56);
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(248, 249, 252);
            this.pnlFooter.Name      = "pnlFooter";

            this.lblFooterHint.Text      = "ⓘ  คำจะถูก Freeze Definition ณ เวลาเริ่มต้น";
            this.lblFooterHint.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblFooterHint.ForeColor = System.Drawing.Color.FromArgb(110, 120, 140);
            this.lblFooterHint.Location  = new System.Drawing.Point(16, 18);
            this.lblFooterHint.AutoSize  = true;
            this.lblFooterHint.Name      = "lblFooterHint";

            this.btnCancel.Text      = "ยกเลิก";
            this.btnCancel.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnCancel.Location  = new System.Drawing.Point(820, 12);
            this.btnCancel.Size      = new System.Drawing.Size(88, 32);
            this.btnCancel.Name      = "btnCancel";
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this.btnCancel.Click    += new System.EventHandler(this.btnCancel_Click);

            this.btnStart.Text      = "  เริ่ม Workflow";
            this.btnStart.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnStart.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnStart.Location  = new System.Drawing.Point(916, 12);
            this.btnStart.Size      = new System.Drawing.Size(140, 32);
            this.btnStart.Name      = "btnStart";
            this.btnStart.FlatAppearance.BorderSize = 0;
            this.btnStart.Click    += new System.EventHandler(this.btnStart_Click);

            this.pnlFooter.Controls.Add(this.lblFooterHint);
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Controls.Add(this.btnStart);

            // ── Body ──────────────────────────────────────────────────────────
            this.pnlBody.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.BackColor = System.Drawing.Color.White;
            this.pnlBody.Name      = "pnlBody";

            // ── Left panel ────────────────────────────────────────────────────
            this.pnlLeft.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Width     = 460;
            this.pnlLeft.BackColor = System.Drawing.Color.White;
            this.pnlLeft.Padding   = new System.Windows.Forms.Padding(20, 16, 16, 0);
            this.pnlLeft.Name      = "pnlLeft";

            this.lblStep1.Text      = "1. เลือก TEMPLATE";
            this.lblStep1.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblStep1.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblStep1.Location  = new System.Drawing.Point(20, 16);
            this.lblStep1.AutoSize  = true;
            this.lblStep1.Name      = "lblStep1";

            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtSearch.ForeColor   = System.Drawing.Color.FromArgb(140, 150, 165);
            this.txtSearch.Text        = "🔍  ค้นหา Template...";
            this.txtSearch.Location    = new System.Drawing.Point(20, 38);
            this.txtSearch.Size        = new System.Drawing.Size(420, 28);
            this.txtSearch.Name        = "txtSearch";

            this.pnlTemplateList.Location   = new System.Drawing.Point(20, 74);
            this.pnlTemplateList.Size       = new System.Drawing.Size(420, 520);
            this.pnlTemplateList.BackColor  = System.Drawing.Color.White;
            this.pnlTemplateList.AutoScroll = true;
            this.pnlTemplateList.Name       = "pnlTemplateList";

            this.pnlLeft.Controls.Add(this.lblStep1);
            this.pnlLeft.Controls.Add(this.txtSearch);
            this.pnlLeft.Controls.Add(this.pnlTemplateList);

            // ── Divider ───────────────────────────────────────────────────────
            this.pnlDivider.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlDivider.Width     = 1;
            this.pnlDivider.BackColor = System.Drawing.Color.FromArgb(225, 228, 235);
            this.pnlDivider.Name      = "pnlDivider";

            // ── Right panel ───────────────────────────────────────────────────
            this.pnlRight.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.BackColor = System.Drawing.Color.White;
            this.pnlRight.Padding   = new System.Windows.Forms.Padding(20, 16, 20, 0);
            this.pnlRight.AutoScroll = true;
            this.pnlRight.Name      = "pnlRight";

            // Step 2
            this.lblStep2.Text      = "2. TEMPLATE ที่เลือก";
            this.lblStep2.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblStep2.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblStep2.Location  = new System.Drawing.Point(20, 16);
            this.lblStep2.AutoSize  = true;
            this.lblStep2.Name      = "lblStep2";

            this.pnlTemplatePreview.Location  = new System.Drawing.Point(20, 36);
            this.pnlTemplatePreview.Size      = new System.Drawing.Size(500, 110);
            this.pnlTemplatePreview.BackColor = System.Drawing.Color.FromArgb(240, 245, 255);
            this.pnlTemplatePreview.Name      = "pnlTemplatePreview";

            // Step 3
            this.lblStep3.Text      = "3. ข้อมูลเพิ่มเติม (CONTEXT)";
            this.lblStep3.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblStep3.ForeColor = System.Drawing.Color.FromArgb(100, 110, 130);
            this.lblStep3.Location  = new System.Drawing.Point(20, 158);
            this.lblStep3.AutoSize  = true;
            this.lblStep3.Name      = "lblStep3";

            // หัวข้อ/เรื่อง *
            this.lblTitleField.Text      = "หัวข้อ/เรื่อง *";
            this.lblTitleField.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTitleField.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblTitleField.Location  = new System.Drawing.Point(20, 180);
            this.lblTitleField.AutoSize  = true;
            this.lblTitleField.Name      = "lblTitleField";

            this.txtTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTitle.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtTitle.Location    = new System.Drawing.Point(20, 198);
            this.txtTitle.Size        = new System.Drawing.Size(500, 28);
            this.txtTitle.Name        = "txtTitle";

            // รายละเอียด
            this.lblDescription.Text      = "รายละเอียด";
            this.lblDescription.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblDescription.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblDescription.Location  = new System.Drawing.Point(20, 236);
            this.lblDescription.AutoSize  = true;
            this.lblDescription.Name      = "lblDescription";

            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDescription.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtDescription.Multiline   = true;
            this.txtDescription.ScrollBars  = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Location    = new System.Drawing.Point(20, 254);
            this.txtDescription.Size        = new System.Drawing.Size(500, 72);
            this.txtDescription.Name        = "txtDescription";

            // ระดับความสำคัญ + CRAF (same row)
            this.lblPriority.Text      = "ระดับความสำคัญ";
            this.lblPriority.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPriority.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblPriority.Location  = new System.Drawing.Point(20, 338);
            this.lblPriority.AutoSize  = true;
            this.lblPriority.Name      = "lblPriority";

            this.cmbPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPriority.FlatStyle     = System.Windows.Forms.FlatStyle.Flat;
            this.cmbPriority.Font          = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbPriority.Location      = new System.Drawing.Point(20, 356);
            this.cmbPriority.Size          = new System.Drawing.Size(240, 28);
            this.cmbPriority.Name          = "cmbPriority";
            this.cmbPriority.Items.AddRange(new object[] { "ปกติ", "สูง", "ด่วน", "ด่วนมาก" });
            this.cmbPriority.SelectedIndex = 0;

            this.lblCRAF.Text      = "CRAF Classification";
            this.lblCRAF.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCRAF.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblCRAF.Location  = new System.Drawing.Point(270, 338);
            this.lblCRAF.AutoSize  = true;
            this.lblCRAF.Name      = "lblCRAF";

            this.cmbCRAF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCRAF.FlatStyle     = System.Windows.Forms.FlatStyle.Flat;
            this.cmbCRAF.Font          = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbCRAF.Location      = new System.Drawing.Point(270, 356);
            this.cmbCRAF.Size          = new System.Drawing.Size(250, 28);
            this.cmbCRAF.Name          = "cmbCRAF";
            this.cmbCRAF.Items.AddRange(new object[] {
                "1HC — Highly Confidential",
                "2SC — Secret",
                "3NC — Non-Confidential",
                "4PC — Public/Common" });
            this.cmbCRAF.SelectedIndex = 2;

            // Entity ที่เชื่อมโยง
            this.lblEntity.Text      = "Entity ที่เชื่อมโยง (Auto-filled)";
            this.lblEntity.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblEntity.ForeColor = System.Drawing.Color.FromArgb(55, 65, 90);
            this.lblEntity.Location  = new System.Drawing.Point(20, 398);
            this.lblEntity.AutoSize  = true;
            this.lblEntity.Name      = "lblEntity";

            this.pnlEntityBadge.Location  = new System.Drawing.Point(20, 416);
            this.pnlEntityBadge.Size      = new System.Drawing.Size(500, 36);
            this.pnlEntityBadge.BackColor = System.Drawing.Color.FromArgb(243, 246, 251);
            this.pnlEntityBadge.Name      = "pnlEntityBadge";

            this.lblEntityValue.Text      = "(ยังไม่ได้เลือก Entity)";
            this.lblEntityValue.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEntityValue.ForeColor = System.Drawing.Color.FromArgb(110, 120, 140);
            this.lblEntityValue.Location  = new System.Drawing.Point(10, 8);
            this.lblEntityValue.AutoSize  = true;
            this.lblEntityValue.Name      = "lblEntityValue";
            this.pnlEntityBadge.Controls.Add(this.lblEntityValue);

            this.pnlRight.Controls.Add(this.lblStep2);
            this.pnlRight.Controls.Add(this.pnlTemplatePreview);
            this.pnlRight.Controls.Add(this.lblStep3);
            this.pnlRight.Controls.Add(this.lblTitleField);
            this.pnlRight.Controls.Add(this.txtTitle);
            this.pnlRight.Controls.Add(this.lblDescription);
            this.pnlRight.Controls.Add(this.txtDescription);
            this.pnlRight.Controls.Add(this.lblPriority);
            this.pnlRight.Controls.Add(this.cmbPriority);
            this.pnlRight.Controls.Add(this.lblCRAF);
            this.pnlRight.Controls.Add(this.cmbCRAF);
            this.pnlRight.Controls.Add(this.lblEntity);
            this.pnlRight.Controls.Add(this.pnlEntityBadge);

            // ── Assemble body ─────────────────────────────────────────────────
            this.pnlBody.Controls.Add(this.pnlRight);
            this.pnlBody.Controls.Add(this.pnlDivider);
            this.pnlBody.Controls.Add(this.pnlLeft);

            // ── Form controls ─────────────────────────────────────────────────
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlTitleBar);

            this.ResumeLayout(false);
        }
    }
}
