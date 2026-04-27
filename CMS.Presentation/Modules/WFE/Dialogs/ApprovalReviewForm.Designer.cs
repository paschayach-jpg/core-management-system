namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    partial class ApprovalReviewForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnApprove;
        private System.Windows.Forms.Button btnReject;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnApprove = new System.Windows.Forms.Button();
            this.btnReject = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // btnApprove
            this.btnApprove.Location = new System.Drawing.Point(150, 200);
            this.btnApprove.Name = "btnApprove";
            this.btnApprove.Size = new System.Drawing.Size(75, 23);
            this.btnApprove.TabIndex = 0;
            this.btnApprove.Text = "Approve";
            this.btnApprove.UseVisualStyleBackColor = true;
            this.btnApprove.Click += new System.EventHandler(this.btnApprove_Click);

            // btnReject
            this.btnReject.Location = new System.Drawing.Point(231, 200);
            this.btnReject.Name = "btnReject";
            this.btnReject.Size = new System.Drawing.Size(75, 23);
            this.btnReject.TabIndex = 1;
            this.btnReject.Text = "Reject";
            this.btnReject.UseVisualStyleBackColor = true;
            this.btnReject.Click += new System.EventHandler(this.btnReject_Click);

            // ApprovalReviewForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(318, 235);
            this.Controls.Add(this.btnReject);
            this.Controls.Add(this.btnApprove);
            this.Name = "ApprovalReviewForm";
            this.Text = "Approval Review";
            this.Load += new System.EventHandler(this.ApprovalReviewForm_Load);
            this.ResumeLayout(false);
        }
    }
}
