namespace CMS.Presentation
{
    partial class ShareManagementForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ShareManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 537);
            this.MinimumSize = new System.Drawing.Size(688, 456);
            this.Name = "ShareManagementForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "จัดการการแบ่งปัน";
            this.Load += new System.EventHandler(this.ShareManagementForm_Load);
            this.ResumeLayout(false);

        }
    }
}
