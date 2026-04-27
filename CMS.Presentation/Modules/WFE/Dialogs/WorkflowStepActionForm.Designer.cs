namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    partial class WorkflowStepActionForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnComplete;
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
            this.btnComplete = new System.Windows.Forms.Button();
            this.btnReject = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // btnComplete
            this.btnComplete.Location = new System.Drawing.Point(150, 200);
            this.btnComplete.Name = "btnComplete";
            this.btnComplete.Size = new System.Drawing.Size(75, 23);
            this.btnComplete.TabIndex = 0;
            this.btnComplete.Text = "Complete";
            this.btnComplete.UseVisualStyleBackColor = true;
            this.btnComplete.Click += new System.EventHandler(this.btnComplete_Click);

            // btnReject
            this.btnReject.Location = new System.Drawing.Point(231, 200);
            this.btnReject.Name = "btnReject";
            this.btnReject.Size = new System.Drawing.Size(75, 23);
            this.btnReject.TabIndex = 1;
            this.btnReject.Text = "Reject";
            this.btnReject.UseVisualStyleBackColor = true;
            this.btnReject.Click += new System.EventHandler(this.btnReject_Click);

            // WorkflowStepActionForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(318, 235);
            this.Controls.Add(this.btnReject);
            this.Controls.Add(this.btnComplete);
            this.Name = "WorkflowStepActionForm";
            this.Text = "Step Action";
            this.Load += new System.EventHandler(this.WorkflowStepActionForm_Load);
            this.ResumeLayout(false);
        }
    }
}
