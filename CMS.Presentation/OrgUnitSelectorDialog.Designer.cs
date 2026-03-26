namespace CMS.Presentation
{
    partial class OrgUnitSelectorDialog
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
            this.pnlHeader  = new System.Windows.Forms.Panel();
            this.lblHeader  = new System.Windows.Forms.Label();
            this.treeView   = new System.Windows.Forms.TreeView();
            this.pnlFooter  = new System.Windows.Forms.Panel();
            this.lblStatus  = new System.Windows.Forms.Label();
            this.btnOK      = new System.Windows.Forms.Button();
            this.btnCancel  = new System.Windows.Forms.Button();

            this.pnlHeader.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // ── Header ──────────────────────────────────────────────────────
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 40;
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.pnlHeader.Padding   = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.pnlHeader.Controls.Add(this.lblHeader);

            this.lblHeader.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Text      = "เลือกหน่วยงาน";
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Font      = new System.Drawing.Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold);
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── TreeView (Fill) ───────────────────────────────────────────────
            this.treeView.Dock         = System.Windows.Forms.DockStyle.Fill;
            this.treeView.ShowLines    = true;
            this.treeView.ShowPlusMinus = true;
            this.treeView.HotTracking  = true;
            this.treeView.Font         = new System.Drawing.Font("Segoe UI", 9.5f);
            this.treeView.BorderStyle  = System.Windows.Forms.BorderStyle.None;
            this.treeView.BackColor    = System.Drawing.Color.White;
            this.treeView.DoubleClick += new System.EventHandler(this.treeView_DoubleClick);

            // ── Footer ───────────────────────────────────────────────────────
            this.pnlFooter.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height    = 48;
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(243, 243, 243);
            this.pnlFooter.Padding   = new System.Windows.Forms.Padding(8);

            this.lblStatus.Dock      = System.Windows.Forms.DockStyle.Left;
            this.lblStatus.Width     = 200;
            this.lblStatus.Text      = "กรุณาเลือกหน่วยงาน";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            this.lblStatus.ForeColor = System.Drawing.Color.Gray;

            this.btnOK.Text        = "ยืนยัน";
            this.btnOK.Width       = 80;
            this.btnOK.Height      = 30;
            this.btnOK.Location    = new System.Drawing.Point(252, 9);
            this.btnOK.Anchor      = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this.btnOK.BackColor   = System.Drawing.Color.FromArgb(0, 51, 102);
            this.btnOK.ForeColor   = System.Drawing.Color.White;
            this.btnOK.FlatStyle   = System.Windows.Forms.FlatStyle.Flat;
            this.btnOK.FlatAppearance.BorderSize = 0;
            this.btnOK.Font        = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Click       += new System.EventHandler(this.btnOK_Click);

            this.btnCancel.Text        = "ยกเลิก";
            this.btnCancel.Width       = 80;
            this.btnCancel.Height      = 30;
            this.btnCancel.Location    = new System.Drawing.Point(340, 9);
            this.btnCancel.Anchor      = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this.btnCancel.FlatStyle   = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font        = new System.Drawing.Font("Segoe UI", 9f);
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            this.pnlFooter.Controls.Add(this.lblStatus);
            this.pnlFooter.Controls.Add(this.btnOK);
            this.pnlFooter.Controls.Add(this.btnCancel);

            // ── Form ─────────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(410, 460);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.ShowIcon            = false;
            this.Name                = "OrgUnitSelectorDialog";
            this.Text                = "เลือกหน่วยงาน";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.AcceptButton        = this.btnOK;
            this.CancelButton        = this.btnCancel;
            this.Load               += new System.EventHandler(this.OrgUnitSelectorDialog_Load);

            // DockStyle order: Fill first, Bottom, Top last
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);

            this.pnlHeader.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel      pnlHeader;
        private System.Windows.Forms.Label      lblHeader;
        private System.Windows.Forms.TreeView   treeView;
        private System.Windows.Forms.Panel      pnlFooter;
        private System.Windows.Forms.Label      lblStatus;
        private System.Windows.Forms.Button     btnOK;
        private System.Windows.Forms.Button     btnCancel;
    }
}
