namespace CMS.Presentation
{
    partial class PdpaConsentDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components  = new System.ComponentModel.Container();
            this.pnlHeader   = new System.Windows.Forms.Panel();
            this.lblTitle    = new System.Windows.Forms.Label();
            this.pnlContent  = new System.Windows.Forms.Panel();
            this.rtbPolicy   = new System.Windows.Forms.RichTextBox();
            this.btnReadFull = new System.Windows.Forms.Button();
            this.pnlConsent  = new System.Windows.Forms.Panel();
            this.chkConsent  = new System.Windows.Forms.CheckBox();
            this.pnlBottom   = new System.Windows.Forms.Panel();
            this.btnConfirm  = new System.Windows.Forms.Button();
            this.btnExit     = new System.Windows.Forms.Button();

            this.pnlHeader.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlConsent.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();

            // ── pnlHeader ────────────────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 73, 125);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 44;
            this.pnlHeader.Name      = "pnlHeader";
            this.pnlHeader.TabIndex  = 0;

            // ── lblTitle ─────────────────────────────────────────────────────
            this.lblTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Name      = "lblTitle";
            this.lblTitle.Padding   = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblTitle.TabIndex  = 0;
            this.lblTitle.Text      = "🔒  นโยบายข้อมูลส่วนบุคคล (PDPA)";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── pnlContent ────────────────────────────────────────────────────
            this.pnlContent.Controls.Add(this.rtbPolicy);
            this.pnlContent.Controls.Add(this.btnReadFull);
            this.pnlContent.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Name     = "pnlContent";
            this.pnlContent.Padding  = new System.Windows.Forms.Padding(16, 12, 16, 8);
            this.pnlContent.TabIndex = 1;

            // ── rtbPolicy ─────────────────────────────────────────────────────
            this.rtbPolicy.BackColor   = System.Drawing.Color.FromArgb(250, 250, 252);
            this.rtbPolicy.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbPolicy.Dock        = System.Windows.Forms.DockStyle.Fill;
            this.rtbPolicy.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            this.rtbPolicy.Name        = "rtbPolicy";
            this.rtbPolicy.ReadOnly    = true;
            this.rtbPolicy.ScrollBars  = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbPolicy.TabIndex    = 0;
            this.rtbPolicy.Text        = "";

            // ── btnReadFull ───────────────────────────────────────────────────
            this.btnReadFull.BackColor = System.Drawing.Color.White;
            this.btnReadFull.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnReadFull.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.btnReadFull.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReadFull.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(31, 73, 125);
            this.btnReadFull.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnReadFull.ForeColor = System.Drawing.Color.FromArgb(31, 73, 125);
            this.btnReadFull.Height    = 28;
            this.btnReadFull.Margin    = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.btnReadFull.Name      = "btnReadFull";
            this.btnReadFull.TabIndex  = 1;
            this.btnReadFull.Text      = "📄 อ่านนโยบายฉบับเต็ม";
            this.btnReadFull.Click    += new System.EventHandler(this.BtnReadFull_Click);

            // ── pnlConsent ────────────────────────────────────────────────────
            this.pnlConsent.BackColor = System.Drawing.Color.FromArgb(240, 244, 255);
            this.pnlConsent.Controls.Add(this.chkConsent);
            this.pnlConsent.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlConsent.Height    = 42;
            this.pnlConsent.Name      = "pnlConsent";
            this.pnlConsent.Padding   = new System.Windows.Forms.Padding(12, 0, 12, 0);
            this.pnlConsent.TabIndex  = 2;

            // ── chkConsent ────────────────────────────────────────────────────
            this.chkConsent.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.chkConsent.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.chkConsent.Name      = "chkConsent";
            this.chkConsent.TabIndex  = 0;
            this.chkConsent.Text      = "ฉันรับทราบและยินยอมให้ใช้ข้อมูลส่วนบุคคลตามนโยบายข้างต้น";
            this.chkConsent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkConsent.CheckedChanged += new System.EventHandler(this.ChkConsent_CheckedChanged);

            // ── pnlBottom ─────────────────────────────────────────────────────
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(245, 245, 248);
            this.pnlBottom.Controls.Add(this.btnConfirm);
            this.pnlBottom.Controls.Add(this.btnExit);
            this.pnlBottom.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Height    = 52;
            this.pnlBottom.Name      = "pnlBottom";
            this.pnlBottom.Padding   = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlBottom.TabIndex  = 3;
            this.pnlBottom.Resize   += new System.EventHandler(this.PnlBottom_Resize);

            // ── btnConfirm ────────────────────────────────────────────────────
            this.btnConfirm.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnConfirm.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnConfirm.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnConfirm.Enabled   = false;
            this.btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirm.FlatAppearance.BorderSize = 0;
            this.btnConfirm.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnConfirm.ForeColor = System.Drawing.Color.White;
            this.btnConfirm.Location  = new System.Drawing.Point(374, 9);
            this.btnConfirm.Name      = "btnConfirm";
            this.btnConfirm.Size      = new System.Drawing.Size(110, 34);
            this.btnConfirm.TabIndex  = 0;
            this.btnConfirm.Text      = "✔ ยืนยัน";
            this.btnConfirm.UseVisualStyleBackColor = false;
            this.btnConfirm.Click    += new System.EventHandler(this.BtnConfirm_Click);

            // ── btnExit ───────────────────────────────────────────────────────
            this.btnExit.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(196, 43, 28);
            this.btnExit.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.Location  = new System.Drawing.Point(492, 9);
            this.btnExit.Name      = "btnExit";
            this.btnExit.Size      = new System.Drawing.Size(120, 34);
            this.btnExit.TabIndex  = 1;
            this.btnExit.Text      = "✖ ออกจากระบบ";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click    += new System.EventHandler(this.BtnExit_Click);

            // ── PdpaConsentDialog ─────────────────────────────────────────────
            this.AcceptButton        = this.btnConfirm;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.White;
            this.ClientSize          = new System.Drawing.Size(620, 420);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlConsent);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlHeader);
            this.Font            = new System.Drawing.Font("Segoe UI", 9.5F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "PdpaConsentDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text            = "🔒 นโยบายข้อมูลส่วนบุคคล (PDPA)";

            this.pnlBottom.ResumeLayout(false);
            this.pnlConsent.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #region Designer fields
        private System.Windows.Forms.Panel      pnlHeader;
        private System.Windows.Forms.Label      lblTitle;
        private System.Windows.Forms.Panel      pnlContent;
        private System.Windows.Forms.RichTextBox rtbPolicy;
        private System.Windows.Forms.Button     btnReadFull;
        private System.Windows.Forms.Panel      pnlConsent;
        private System.Windows.Forms.CheckBox   chkConsent;
        private System.Windows.Forms.Panel      pnlBottom;
        private System.Windows.Forms.Button     btnConfirm;
        private System.Windows.Forms.Button     btnExit;
        #endregion
    }
}
