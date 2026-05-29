namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    partial class DelegationDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblWorkflow;
        private System.Windows.Forms.Label lblWorkflowValue;
        private System.Windows.Forms.Label lblStep;
        private System.Windows.Forms.Label lblStepValue;
        private System.Windows.Forms.Label lblAssignedDate;
        private System.Windows.Forms.Label lblAssignedDateValue;
        private System.Windows.Forms.Label lblDueDateLabel;
        private System.Windows.Forms.Label lblDueDateValue;
        private System.Windows.Forms.Label lblDelegateToLabel;
        private System.Windows.Forms.ComboBox cmbDelegateTo;
        private System.Windows.Forms.Label lblDelegationReason;
        private System.Windows.Forms.TextBox txtDelegationReason;
        private System.Windows.Forms.Button btnDelegate;
        private System.Windows.Forms.Button btnCancel;

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
            this.lblWorkflow = new System.Windows.Forms.Label();
            this.lblWorkflowValue = new System.Windows.Forms.Label();
            this.lblStep = new System.Windows.Forms.Label();
            this.lblStepValue = new System.Windows.Forms.Label();
            this.lblAssignedDate = new System.Windows.Forms.Label();
            this.lblAssignedDateValue = new System.Windows.Forms.Label();
            this.lblDueDateLabel = new System.Windows.Forms.Label();
            this.lblDueDateValue = new System.Windows.Forms.Label();
            this.lblDelegateToLabel = new System.Windows.Forms.Label();
            this.cmbDelegateTo = new System.Windows.Forms.ComboBox();
            this.lblDelegationReason = new System.Windows.Forms.Label();
            this.txtDelegationReason = new System.Windows.Forms.TextBox();
            this.btnDelegate = new System.Windows.Forms.Button();
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
            this.lblTitle.Size = new System.Drawing.Size(106, 20);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Delegate Task";

            // lblWorkflow
            this.lblWorkflow.AutoSize = true;
            this.lblWorkflow.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblWorkflow.Location = new System.Drawing.Point(15, 65);
            this.lblWorkflow.Name = "lblWorkflow";
            this.lblWorkflow.Size = new System.Drawing.Size(67, 15);
            this.lblWorkflow.TabIndex = 1;
            this.lblWorkflow.Text = "Workflow:";

            // lblWorkflowValue
            this.lblWorkflowValue.AutoSize = true;
            this.lblWorkflowValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblWorkflowValue.Location = new System.Drawing.Point(150, 65);
            this.lblWorkflowValue.Name = "lblWorkflowValue";
            this.lblWorkflowValue.Size = new System.Drawing.Size(11, 15);
            this.lblWorkflowValue.TabIndex = 2;
            this.lblWorkflowValue.Text = "-";

            // lblStep
            this.lblStep.AutoSize = true;
            this.lblStep.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStep.Location = new System.Drawing.Point(15, 90);
            this.lblStep.Name = "lblStep";
            this.lblStep.Size = new System.Drawing.Size(37, 15);
            this.lblStep.TabIndex = 3;
            this.lblStep.Text = "Step:";

            // lblStepValue
            this.lblStepValue.AutoSize = true;
            this.lblStepValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStepValue.Location = new System.Drawing.Point(150, 90);
            this.lblStepValue.Name = "lblStepValue";
            this.lblStepValue.Size = new System.Drawing.Size(11, 15);
            this.lblStepValue.TabIndex = 4;
            this.lblStepValue.Text = "-";

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

            // lblDueDateLabel
            this.lblDueDateLabel.AutoSize = true;
            this.lblDueDateLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDueDateLabel.Location = new System.Drawing.Point(15, 140);
            this.lblDueDateLabel.Name = "lblDueDateLabel";
            this.lblDueDateLabel.Size = new System.Drawing.Size(70, 15);
            this.lblDueDateLabel.TabIndex = 7;
            this.lblDueDateLabel.Text = "Due Date:";

            // lblDueDateValue
            this.lblDueDateValue.AutoSize = true;
            this.lblDueDateValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDueDateValue.Location = new System.Drawing.Point(150, 140);
            this.lblDueDateValue.Name = "lblDueDateValue";
            this.lblDueDateValue.Size = new System.Drawing.Size(11, 15);
            this.lblDueDateValue.TabIndex = 8;
            this.lblDueDateValue.Text = "-";

            // lblDelegateToLabel
            this.lblDelegateToLabel.AutoSize = true;
            this.lblDelegateToLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDelegateToLabel.Location = new System.Drawing.Point(15, 170);
            this.lblDelegateToLabel.Name = "lblDelegateToLabel";
            this.lblDelegateToLabel.Size = new System.Drawing.Size(81, 15);
            this.lblDelegateToLabel.TabIndex = 9;
            this.lblDelegateToLabel.Text = "Delegate To:";

            // cmbDelegateTo
            this.cmbDelegateTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDelegateTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbDelegateTo.Location = new System.Drawing.Point(150, 166);
            this.cmbDelegateTo.Name = "cmbDelegateTo";
            this.cmbDelegateTo.Size = new System.Drawing.Size(335, 23);
            this.cmbDelegateTo.TabIndex = 10;

            // lblDelegationReason
            this.lblDelegationReason.AutoSize = true;
            this.lblDelegationReason.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDelegationReason.Location = new System.Drawing.Point(15, 200);
            this.lblDelegationReason.Name = "lblDelegationReason";
            this.lblDelegationReason.Size = new System.Drawing.Size(122, 15);
            this.lblDelegationReason.TabIndex = 11;
            this.lblDelegationReason.Text = "Delegation Reason:";

            // txtDelegationReason
            this.txtDelegationReason.Location = new System.Drawing.Point(15, 218);
            this.txtDelegationReason.Multiline = true;
            this.txtDelegationReason.Name = "txtDelegationReason";
            this.txtDelegationReason.Size = new System.Drawing.Size(470, 70);
            this.txtDelegationReason.TabIndex = 12;

            // btnDelegate
            this.btnDelegate.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnDelegate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnDelegate.ForeColor = System.Drawing.Color.White;
            this.btnDelegate.Location = new System.Drawing.Point(240, 305);
            this.btnDelegate.Name = "btnDelegate";
            this.btnDelegate.Size = new System.Drawing.Size(100, 30);
            this.btnDelegate.TabIndex = 13;
            this.btnDelegate.Text = "✓ Delegate";
            this.btnDelegate.UseVisualStyleBackColor = false;
            this.btnDelegate.Click += new System.EventHandler(this.btnDelegate_Click);

            // btnCancel
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(350, 305);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 30);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // DelegationDialog
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 350);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnDelegate);
            this.Controls.Add(this.txtDelegationReason);
            this.Controls.Add(this.lblDelegationReason);
            this.Controls.Add(this.cmbDelegateTo);
            this.Controls.Add(this.lblDelegateToLabel);
            this.Controls.Add(this.lblDueDateValue);
            this.Controls.Add(this.lblDueDateLabel);
            this.Controls.Add(this.lblAssignedDateValue);
            this.Controls.Add(this.lblAssignedDate);
            this.Controls.Add(this.lblStepValue);
            this.Controls.Add(this.lblStep);
            this.Controls.Add(this.lblWorkflowValue);
            this.Controls.Add(this.lblWorkflow);
            this.Controls.Add(this.pnlHeader);
            this.Name = "DelegationDialog";
            this.Text = "Delegate Task";
            this.Load += new System.EventHandler(this.DelegationDialog_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
