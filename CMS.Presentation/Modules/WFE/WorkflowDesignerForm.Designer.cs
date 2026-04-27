namespace CMS.Presentation.Modules.Workflow
{
    partial class WorkflowDesignerForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnAddStep;
        private System.Windows.Forms.Button btnValidate;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtTemplateName;
        private System.Windows.Forms.TextBox txtTemplateDescription;
        private System.Windows.Forms.Label lblTemplateName;
        private System.Windows.Forms.Label lblTemplateDescription;
        private System.Windows.Forms.Panel pnlCanvas;
        private System.Windows.Forms.Panel pnlToolbox;
        private System.Windows.Forms.Label lblStepType;
        private System.Windows.Forms.ComboBox cmbStepType;
        private System.Windows.Forms.Button btnAddConnection;
        private System.Windows.Forms.ListBox lstSteps;
        private System.Windows.Forms.Label lblSteps;
        private System.Windows.Forms.Button btnDeleteStep;
        private System.Windows.Forms.Button btnEditStep;
        private System.Windows.Forms.ListBox lstConnections;
        private System.Windows.Forms.Label lblConnections;
        private System.Windows.Forms.Button btnDeleteConnection;
        private System.Windows.Forms.Button btnEditConnection;

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
            this.btnAddStep = new System.Windows.Forms.Button();
            this.btnValidate = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtTemplateName = new System.Windows.Forms.TextBox();
            this.txtTemplateDescription = new System.Windows.Forms.TextBox();
            this.lblTemplateName = new System.Windows.Forms.Label();
            this.lblTemplateDescription = new System.Windows.Forms.Label();
            this.pnlCanvas = new System.Windows.Forms.Panel();
            this.pnlToolbox = new System.Windows.Forms.Panel();
            this.lblStepType = new System.Windows.Forms.Label();
            this.cmbStepType = new System.Windows.Forms.ComboBox();
            this.btnAddConnection = new System.Windows.Forms.Button();
            this.lstSteps = new System.Windows.Forms.ListBox();
            this.lblSteps = new System.Windows.Forms.Label();
            this.btnDeleteStep = new System.Windows.Forms.Button();
            this.btnEditStep = new System.Windows.Forms.Button();
            this.lstConnections = new System.Windows.Forms.ListBox();
            this.lblConnections = new System.Windows.Forms.Label();
            this.btnDeleteConnection = new System.Windows.Forms.Button();
            this.btnEditConnection = new System.Windows.Forms.Button();
            
            this.pnlToolbox.SuspendLayout();
            this.SuspendLayout();

            // lblTemplateName
            this.lblTemplateName.AutoSize = true;
            this.lblTemplateName.Location = new System.Drawing.Point(12, 12);
            this.lblTemplateName.Name = "lblTemplateName";
            this.lblTemplateName.Size = new System.Drawing.Size(88, 13);
            this.lblTemplateName.TabIndex = 0;
            this.lblTemplateName.Text = "Template Name:";

            // txtTemplateName
            this.txtTemplateName.Location = new System.Drawing.Point(106, 9);
            this.txtTemplateName.Name = "txtTemplateName";
            this.txtTemplateName.Size = new System.Drawing.Size(200, 20);
            this.txtTemplateName.TabIndex = 1;

            // lblTemplateDescription
            this.lblTemplateDescription.AutoSize = true;
            this.lblTemplateDescription.Location = new System.Drawing.Point(312, 12);
            this.lblTemplateDescription.Name = "lblTemplateDescription";
            this.lblTemplateDescription.Size = new System.Drawing.Size(63, 13);
            this.lblTemplateDescription.TabIndex = 2;
            this.lblTemplateDescription.Text = "Description:";

            // txtTemplateDescription
            this.txtTemplateDescription.Location = new System.Drawing.Point(381, 9);
            this.txtTemplateDescription.Name = "txtTemplateDescription";
            this.txtTemplateDescription.Size = new System.Drawing.Size(300, 20);
            this.txtTemplateDescription.TabIndex = 3;

            // pnlToolbox
            this.pnlToolbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlToolbox.Controls.Add(this.lblStepType);
            this.pnlToolbox.Controls.Add(this.cmbStepType);
            this.pnlToolbox.Controls.Add(this.btnAddStep);
            this.pnlToolbox.Controls.Add(this.btnAddConnection);
            this.pnlToolbox.Controls.Add(this.lblSteps);
            this.pnlToolbox.Controls.Add(this.lstSteps);
            this.pnlToolbox.Controls.Add(this.btnEditStep);
            this.pnlToolbox.Controls.Add(this.btnDeleteStep);
            this.pnlToolbox.Controls.Add(this.lblConnections);
            this.pnlToolbox.Controls.Add(this.lstConnections);
            this.pnlToolbox.Controls.Add(this.btnEditConnection);
            this.pnlToolbox.Controls.Add(this.btnDeleteConnection);
            this.pnlToolbox.Location = new System.Drawing.Point(12, 35);
            this.pnlToolbox.Name = "pnlToolbox";
            this.pnlToolbox.Size = new System.Drawing.Size(250, 600);
            this.pnlToolbox.TabIndex = 4;

            // lblStepType
            this.lblStepType.AutoSize = true;
            this.lblStepType.Location = new System.Drawing.Point(5, 5);
            this.lblStepType.Name = "lblStepType";
            this.lblStepType.Size = new System.Drawing.Size(62, 13);
            this.lblStepType.TabIndex = 0;
            this.lblStepType.Text = "Step Type:";

            // cmbStepType
            this.cmbStepType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStepType.FormattingEnabled = true;
            this.cmbStepType.Location = new System.Drawing.Point(5, 21);
            this.cmbStepType.Name = "cmbStepType";
            this.cmbStepType.Size = new System.Drawing.Size(235, 21);
            this.cmbStepType.TabIndex = 1;

            // btnAddStep
            this.btnAddStep.Location = new System.Drawing.Point(5, 48);
            this.btnAddStep.Name = "btnAddStep";
            this.btnAddStep.Size = new System.Drawing.Size(235, 23);
            this.btnAddStep.TabIndex = 2;
            this.btnAddStep.Text = "Add Step";
            this.btnAddStep.UseVisualStyleBackColor = true;
            this.btnAddStep.Click += new System.EventHandler(this.btnAddStep_Click);

            // btnAddConnection
            this.btnAddConnection.Location = new System.Drawing.Point(5, 77);
            this.btnAddConnection.Name = "btnAddConnection";
            this.btnAddConnection.Size = new System.Drawing.Size(235, 23);
            this.btnAddConnection.TabIndex = 3;
            this.btnAddConnection.Text = "Add Connection";
            this.btnAddConnection.UseVisualStyleBackColor = true;
            this.btnAddConnection.Click += new System.EventHandler(this.btnAddConnection_Click);

            // lblSteps
            this.lblSteps.AutoSize = true;
            this.lblSteps.Location = new System.Drawing.Point(5, 105);
            this.lblSteps.Name = "lblSteps";
            this.lblSteps.Size = new System.Drawing.Size(38, 13);
            this.lblSteps.TabIndex = 4;
            this.lblSteps.Text = "Steps:";

            // lstSteps
            this.lstSteps.FormattingEnabled = true;
            this.lstSteps.Location = new System.Drawing.Point(5, 121);
            this.lstSteps.Name = "lstSteps";
            this.lstSteps.Size = new System.Drawing.Size(235, 95);
            this.lstSteps.TabIndex = 5;
            this.lstSteps.SelectedIndexChanged += new System.EventHandler(this.lstSteps_SelectedIndexChanged);

            // btnEditStep
            this.btnEditStep.Location = new System.Drawing.Point(5, 222);
            this.btnEditStep.Name = "btnEditStep";
            this.btnEditStep.Size = new System.Drawing.Size(110, 23);
            this.btnEditStep.TabIndex = 6;
            this.btnEditStep.Text = "Edit";
            this.btnEditStep.UseVisualStyleBackColor = true;
            this.btnEditStep.Click += new System.EventHandler(this.btnEditStep_Click);

            // btnDeleteStep
            this.btnDeleteStep.Location = new System.Drawing.Point(130, 222);
            this.btnDeleteStep.Name = "btnDeleteStep";
            this.btnDeleteStep.Size = new System.Drawing.Size(110, 23);
            this.btnDeleteStep.TabIndex = 7;
            this.btnDeleteStep.Text = "Delete";
            this.btnDeleteStep.UseVisualStyleBackColor = true;
            this.btnDeleteStep.Click += new System.EventHandler(this.btnDeleteStep_Click);

            // lblConnections
            this.lblConnections.AutoSize = true;
            this.lblConnections.Location = new System.Drawing.Point(5, 250);
            this.lblConnections.Name = "lblConnections";
            this.lblConnections.Size = new System.Drawing.Size(66, 13);
            this.lblConnections.TabIndex = 8;
            this.lblConnections.Text = "Connections:";

            // lstConnections
            this.lstConnections.FormattingEnabled = true;
            this.lstConnections.Location = new System.Drawing.Point(5, 266);
            this.lstConnections.Name = "lstConnections";
            this.lstConnections.Size = new System.Drawing.Size(235, 95);
            this.lstConnections.TabIndex = 9;
            this.lstConnections.SelectedIndexChanged += new System.EventHandler(this.lstConnections_SelectedIndexChanged);

            // btnEditConnection
            this.btnEditConnection.Location = new System.Drawing.Point(5, 367);
            this.btnEditConnection.Name = "btnEditConnection";
            this.btnEditConnection.Size = new System.Drawing.Size(110, 23);
            this.btnEditConnection.TabIndex = 10;
            this.btnEditConnection.Text = "Edit";
            this.btnEditConnection.UseVisualStyleBackColor = true;
            this.btnEditConnection.Click += new System.EventHandler(this.btnEditConnection_Click);

            // btnDeleteConnection
            this.btnDeleteConnection.Location = new System.Drawing.Point(130, 367);
            this.btnDeleteConnection.Name = "btnDeleteConnection";
            this.btnDeleteConnection.Size = new System.Drawing.Size(110, 23);
            this.btnDeleteConnection.TabIndex = 11;
            this.btnDeleteConnection.Text = "Delete";
            this.btnDeleteConnection.UseVisualStyleBackColor = true;
            this.btnDeleteConnection.Click += new System.EventHandler(this.btnDeleteConnection_Click);

            // pnlCanvas
            this.pnlCanvas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCanvas.Location = new System.Drawing.Point(268, 35);
            this.pnlCanvas.Name = "pnlCanvas";
            this.pnlCanvas.Size = new System.Drawing.Size(720, 600);
            this.pnlCanvas.TabIndex = 5;
            this.pnlCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlCanvas_Paint);
            this.pnlCanvas.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlCanvas_MouseClick);

            // btnValidate
            this.btnValidate.Location = new System.Drawing.Point(687, 12);
            this.btnValidate.Name = "btnValidate";
            this.btnValidate.Size = new System.Drawing.Size(75, 23);
            this.btnValidate.TabIndex = 6;
            this.btnValidate.Text = "Validate";
            this.btnValidate.UseVisualStyleBackColor = true;
            this.btnValidate.Click += new System.EventHandler(this.btnValidate_Click);

            // btnSave
            this.btnSave.Location = new System.Drawing.Point(768, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(849, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // WorkflowDesignerForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 650);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnValidate);
            this.Controls.Add(this.pnlCanvas);
            this.Controls.Add(this.pnlToolbox);
            this.Controls.Add(this.txtTemplateDescription);
            this.Controls.Add(this.lblTemplateDescription);
            this.Controls.Add(this.txtTemplateName);
            this.Controls.Add(this.lblTemplateName);
            this.Name = "WorkflowDesignerForm";
            this.Text = "Workflow Designer";
            this.Load += new System.EventHandler(this.WorkflowDesignerForm_Load);
            
            this.pnlToolbox.ResumeLayout(false);
            this.pnlToolbox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
