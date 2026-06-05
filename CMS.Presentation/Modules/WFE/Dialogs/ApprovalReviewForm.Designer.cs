namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    partial class ApprovalReviewForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblStepName;
        private System.Windows.Forms.Label lblStepNameValue;
        private System.Windows.Forms.Label lblAssignedTo;
        private System.Windows.Forms.Label lblAssignedToValue;
        private System.Windows.Forms.Label lblAssignedDate;
        private System.Windows.Forms.Label lblAssignedDateValue;
        private System.Windows.Forms.Label lblSLADeadline;
        private System.Windows.Forms.Label lblSLADeadlineValue;
        private System.Windows.Forms.Label lblRejectionReason;
        private System.Windows.Forms.TextBox txtRejectionReason;
        private System.Windows.Forms.Button btnApprove;
        private System.Windows.Forms.Button btnReject;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;

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
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblStepName = new System.Windows.Forms.Label();
            this.lblStepNameValue = new System.Windows.Forms.Label();
            this.lblAssignedTo = new System.Windows.Forms.Label();
            this.lblAssignedToValue = new System.Windows.Forms.Label();
            this.lblAssignedDate = new System.Windows.Forms.Label();
            this.lblAssignedDateValue = new System.Windows.Forms.Label();
            this.lblSLADeadline = new System.Windows.Forms.Label();
            this.lblSLADeadlineValue = new System.Windows.Forms.Label();
            this.lblRejectionReason = new System.Windows.Forms.Label();
            this.txtRejectionReason = new System.Windows.Forms.TextBox();
            this.btnApprove = new System.Windows.Forms.Button();
            this.btnReject = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(500, 50);
            this.pnlHeader.TabIndex = 0;

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(15, 13);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(149, 20);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Approval Review";

            // lblStepName
            this.lblStepName.AutoSize = true;
            this.lblStepName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStepName.Location = new System.Drawing.Point(15, 65);
            this.lblStepName.Name = "lblStepName";
            this.lblStepName.Size = new System.Drawing.Size(72, 15);
            this.lblStepName.TabIndex = 1;
            this.lblStepName.Text = "Step Name:";

            // lblStepNameValue
            this.lblStepNameValue.AutoSize = true;
            this.lblStepNameValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStepNameValue.Location = new System.Drawing.Point(150, 65);
            this.lblStepNameValue.Name = "lblStepNameValue";
            this.lblStepNameValue.Size = new System.Drawing.Size(11, 15);
            this.lblStepNameValue.TabIndex = 2;
            this.lblStepNameValue.Text = "-";

            // lblAssignedTo
            this.lblAssignedTo.AutoSize = true;
            this.lblAssignedTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAssignedTo.Location = new System.Drawing.Point(15, 90);
            this.lblAssignedTo.Name = "lblAssignedTo";
            this.lblAssignedTo.Size = new System.Drawing.Size(72, 15);
            this.lblAssignedTo.TabIndex = 3;
            this.lblAssignedTo.Text = "Assigned To:";

            // lblAssignedToValue
            this.lblAssignedToValue.AutoSize = true;
            this.lblAssignedToValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAssignedToValue.Location = new System.Drawing.Point(150, 90);
            this.lblAssignedToValue.Name = "lblAssignedToValue";
            this.lblAssignedToValue.Size = new System.Drawing.Size(11, 15);
            this.lblAssignedToValue.TabIndex = 4;
            this.lblAssignedToValue.Text = "-";

            // lblAssignedDate
            this.lblAssignedDate.AutoSize = true;
            this.lblAssignedDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAssignedDate.Location = new System.Drawing.Point(15, 115);
            this.lblAssignedDate.Name = "lblAssignedDate";
            this.lblAssignedDate.Size = new System.Drawing.Size(86, 15);
            this.lblAssignedDate.TabIndex = 5;
            this.lblAssignedDate.Text = "Assigned Date:";

            // lblAssignedDateValue
            this.lblAssignedDateValue.AutoSize = true;
            this.lblAssignedDateValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAssignedDateValue.Location = new System.Drawing.Point(150, 115);
            this.lblAssignedDateValue.Name = "lblAssignedDateValue";
            this.lblAssignedDateValue.Size = new System.Drawing.Size(11, 15);
            this.lblAssignedDateValue.TabIndex = 6;
            this.lblAssignedDateValue.Text = "-";

            // lblSLADeadline
            this.lblSLADeadline.AutoSize = true;
            this.lblSLADeadline.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSLADeadline.Location = new System.Drawing.Point(15, 140);
            this.lblSLADeadline.Name = "lblSLADeadline";
            this.lblSLADeadline.Size = new System.Drawing.Size(93, 15);
            this.lblSLADeadline.TabIndex = 7;
            this.lblSLADeadline.Text = "SLA Deadline:";

            // lblSLADeadlineValue
            this.lblSLADeadlineValue.AutoSize = true;
            this.lblSLADeadlineValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSLADeadlineValue.Location = new System.Drawing.Point(150, 140);
            this.lblSLADeadlineValue.Name = "lblSLADeadlineValue";
            this.lblSLADeadlineValue.Size = new System.Drawing.Size(11, 15);
            this.lblSLADeadlineValue.TabIndex = 8;
            this.lblSLADeadlineValue.Text = "-";

            // lblRejectionReason
            this.lblRejectionReason.AutoSize = true;
            this.lblRejectionReason.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRejectionReason.Location = new System.Drawing.Point(15, 170);
            this.lblRejectionReason.Name = "lblRejectionReason";
            this.lblRejectionReason.Size = new System.Drawing.Size(113, 15);
            this.lblRejectionReason.TabIndex = 9;
            this.lblRejectionReason.Text = "Rejection Reason:";

            // txtRejectionReason
            this.txtRejectionReason.Location = new System.Drawing.Point(15, 188);
            this.txtRejectionReason.Multiline = true;
            this.txtRejectionReason.Name = "txtRejectionReason";
            this.txtRejectionReason.Size = new System.Drawing.Size(470, 80);
            this.txtRejectionReason.TabIndex = 10;

            // btnApprove
            this.btnApprove.BackColor = System.Drawing.Color.FromArgb(34, 139, 34);
            this.btnApprove.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnApprove.ForeColor = System.Drawing.Color.White;
            this.btnApprove.Location = new System.Drawing.Point(140, 290);
            this.btnApprove.Name = "btnApprove";
            this.btnApprove.Size = new System.Drawing.Size(90, 30);
            this.btnApprove.TabIndex = 11;
            this.btnApprove.Text = "✔ Approve";
            this.btnApprove.UseVisualStyleBackColor = false;
            this.btnApprove.Click += new System.EventHandler(this.btnApprove_Click);

            // btnReject
            this.btnReject.BackColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.btnReject.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnReject.ForeColor = System.Drawing.Color.White;
            this.btnReject.Location = new System.Drawing.Point(240, 290);
            this.btnReject.Name = "btnReject";
            this.btnReject.Size = new System.Drawing.Size(90, 30);
            this.btnReject.TabIndex = 12;
            this.btnReject.Text = "✘ Reject";
            this.btnReject.UseVisualStyleBackColor = false;
            this.btnReject.Click += new System.EventHandler(this.btnReject_Click);

            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(340, 290);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 30);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // ApprovalReviewForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 335);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnReject);
            this.Controls.Add(this.btnApprove);
            this.Controls.Add(this.txtRejectionReason);
            this.Controls.Add(this.lblRejectionReason);
            this.Controls.Add(this.lblSLADeadlineValue);
            this.Controls.Add(this.lblSLADeadline);
            this.Controls.Add(this.lblAssignedDateValue);
            this.Controls.Add(this.lblAssignedDate);
            this.Controls.Add(this.lblAssignedToValue);
            this.Controls.Add(this.lblAssignedTo);
            this.Controls.Add(this.lblStepNameValue);
            this.Controls.Add(this.lblStepName);
            this.Controls.Add(this.pnlHeader);
            this.Name = "ApprovalReviewForm";
            this.Text = "Approval Review";
            this.Load += new System.EventHandler(this.ApprovalReviewForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
