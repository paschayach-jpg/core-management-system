namespace CMS.Presentation.UserControls
{
    partial class LoadingOverlayControl
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
            this.lblMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();

            this.lblMessage.AutoSize  = false;
            this.lblMessage.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblMessage.Font      = new System.Drawing.Font("Segoe UI", 13f, System.Drawing.FontStyle.Regular);
            this.lblMessage.ForeColor = System.Drawing.Color.FromArgb(70, 110, 160);
            this.lblMessage.Name      = "lblMessage";
            this.lblMessage.TabIndex  = 0;
            this.lblMessage.Text      = "⏳  กำลังโหลดข้อมูล...";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.Controls.Add(this.lblMessage);
            this.Name      = "LoadingOverlayControl";
            this.Size      = new System.Drawing.Size(300, 100);
            this.Visible   = false;
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblMessage;
    }
}
