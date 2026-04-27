namespace CMS.Presentation.Modules.Workflow
{
    partial class WorkflowDashboardForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnNewWorkflow;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabMyTasks;
        private System.Windows.Forms.TabPage tabMyWorkflows;
        private System.Windows.Forms.TabPage tabSLAMetrics;
        
        // My Tasks Tab Controls
        private System.Windows.Forms.DataGridView dgvMyTasks;
        private System.Windows.Forms.ComboBox cmbTaskStatus;
        private System.Windows.Forms.ComboBox cmbTaskPriority;
        private System.Windows.Forms.Label lblTaskStatus;
        private System.Windows.Forms.Label lblTaskPriority;
        private System.Windows.Forms.Button btnCompleteTask;
        private System.Windows.Forms.Button btnDelegateTask;
        private System.Windows.Forms.Button btnRejectTask;
        
        // My Workflows Tab Controls
        private System.Windows.Forms.DataGridView dgvMyWorkflows;
        private System.Windows.Forms.ComboBox cmbWorkflowStatus;
        private System.Windows.Forms.Label lblWorkflowStatus;
        private System.Windows.Forms.Button btnViewDetails;
        
        // SLA Metrics Tab Controls
        private System.Windows.Forms.Panel pnlSLAMetrics;
        private System.Windows.Forms.Label lblOnTimeCount;
        private System.Windows.Forms.Label lblAtRiskCount;
        private System.Windows.Forms.Label lblOverdueCount;
        private System.Windows.Forms.Label lblCompliancePercentage;
        private System.Windows.Forms.ProgressBar prgCompliancePercentage;
        private System.Windows.Forms.Label lblOnTimeLabel;
        private System.Windows.Forms.Label lblAtRiskLabel;
        private System.Windows.Forms.Label lblOverdueLabel;
        private System.Windows.Forms.Label lblComplianceLabel;

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
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnNewWorkflow = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabMyTasks = new System.Windows.Forms.TabPage();
            this.tabMyWorkflows = new System.Windows.Forms.TabPage();
            this.tabSLAMetrics = new System.Windows.Forms.TabPage();
            
            // My Tasks Tab
            this.cmbTaskStatus = new System.Windows.Forms.ComboBox();
            this.cmbTaskPriority = new System.Windows.Forms.ComboBox();
            this.lblTaskStatus = new System.Windows.Forms.Label();
            this.lblTaskPriority = new System.Windows.Forms.Label();
            this.dgvMyTasks = new System.Windows.Forms.DataGridView();
            this.btnCompleteTask = new System.Windows.Forms.Button();
            this.btnDelegateTask = new System.Windows.Forms.Button();
            this.btnRejectTask = new System.Windows.Forms.Button();
            
            // My Workflows Tab
            this.cmbWorkflowStatus = new System.Windows.Forms.ComboBox();
            this.lblWorkflowStatus = new System.Windows.Forms.Label();
            this.dgvMyWorkflows = new System.Windows.Forms.DataGridView();
            this.btnViewDetails = new System.Windows.Forms.Button();
            
            // SLA Metrics Tab
            this.pnlSLAMetrics = new System.Windows.Forms.Panel();
            this.lblOnTimeCount = new System.Windows.Forms.Label();
            this.lblAtRiskCount = new System.Windows.Forms.Label();
            this.lblOverdueCount = new System.Windows.Forms.Label();
            this.lblCompliancePercentage = new System.Windows.Forms.Label();
            this.prgCompliancePercentage = new System.Windows.Forms.ProgressBar();
            this.lblOnTimeLabel = new System.Windows.Forms.Label();
            this.lblAtRiskLabel = new System.Windows.Forms.Label();
            this.lblOverdueLabel = new System.Windows.Forms.Label();
            this.lblComplianceLabel = new System.Windows.Forms.Label();
            
            this.tabControl.SuspendLayout();
            this.tabMyTasks.SuspendLayout();
            this.tabMyWorkflows.SuspendLayout();
            this.tabSLAMetrics.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMyTasks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMyWorkflows)).BeginInit();
            this.pnlSLAMetrics.SuspendLayout();
            this.SuspendLayout();

            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(12, 12);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // btnNewWorkflow
            this.btnNewWorkflow.Location = new System.Drawing.Point(93, 12);
            this.btnNewWorkflow.Name = "btnNewWorkflow";
            this.btnNewWorkflow.Size = new System.Drawing.Size(100, 23);
            this.btnNewWorkflow.TabIndex = 1;
            this.btnNewWorkflow.Text = "New Workflow";
            this.btnNewWorkflow.UseVisualStyleBackColor = true;
            this.btnNewWorkflow.Click += new System.EventHandler(this.btnNewWorkflow_Click);

            // tabControl
            this.tabControl.Controls.Add(this.tabMyTasks);
            this.tabControl.Controls.Add(this.tabMyWorkflows);
            this.tabControl.Controls.Add(this.tabSLAMetrics);
            this.tabControl.Location = new System.Drawing.Point(12, 41);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(976, 647);
            this.tabControl.TabIndex = 2;

            // ===== MY TASKS TAB =====
            this.tabMyTasks.Controls.Add(this.lblTaskStatus);
            this.tabMyTasks.Controls.Add(this.cmbTaskStatus);
            this.tabMyTasks.Controls.Add(this.lblTaskPriority);
            this.tabMyTasks.Controls.Add(this.cmbTaskPriority);
            this.tabMyTasks.Controls.Add(this.dgvMyTasks);
            this.tabMyTasks.Controls.Add(this.btnCompleteTask);
            this.tabMyTasks.Controls.Add(this.btnDelegateTask);
            this.tabMyTasks.Controls.Add(this.btnRejectTask);
            this.tabMyTasks.Location = new System.Drawing.Point(4, 22);
            this.tabMyTasks.Name = "tabMyTasks";
            this.tabMyTasks.Padding = new System.Windows.Forms.Padding(3);
            this.tabMyTasks.Size = new System.Drawing.Size(968, 621);
            this.tabMyTasks.TabIndex = 0;
            this.tabMyTasks.Text = "My Tasks";
            this.tabMyTasks.UseVisualStyleBackColor = true;

            // lblTaskStatus
            this.lblTaskStatus.AutoSize = true;
            this.lblTaskStatus.Location = new System.Drawing.Point(6, 9);
            this.lblTaskStatus.Name = "lblTaskStatus";
            this.lblTaskStatus.Size = new System.Drawing.Size(38, 13);
            this.lblTaskStatus.TabIndex = 0;
            this.lblTaskStatus.Text = "Status:";

            // cmbTaskStatus
            this.cmbTaskStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTaskStatus.FormattingEnabled = true;
            this.cmbTaskStatus.Location = new System.Drawing.Point(50, 6);
            this.cmbTaskStatus.Name = "cmbTaskStatus";
            this.cmbTaskStatus.Size = new System.Drawing.Size(120, 21);
            this.cmbTaskStatus.TabIndex = 1;
            this.cmbTaskStatus.SelectedIndexChanged += new System.EventHandler(this.cmbTaskStatus_SelectedIndexChanged);

            // lblTaskPriority
            this.lblTaskPriority.AutoSize = true;
            this.lblTaskPriority.Location = new System.Drawing.Point(176, 9);
            this.lblTaskPriority.Name = "lblTaskPriority";
            this.lblTaskPriority.Size = new System.Drawing.Size(41, 13);
            this.lblTaskPriority.TabIndex = 2;
            this.lblTaskPriority.Text = "Priority:";

            // cmbTaskPriority
            this.cmbTaskPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTaskPriority.FormattingEnabled = true;
            this.cmbTaskPriority.Location = new System.Drawing.Point(223, 6);
            this.cmbTaskPriority.Name = "cmbTaskPriority";
            this.cmbTaskPriority.Size = new System.Drawing.Size(120, 21);
            this.cmbTaskPriority.TabIndex = 3;
            this.cmbTaskPriority.SelectedIndexChanged += new System.EventHandler(this.cmbTaskPriority_SelectedIndexChanged);

            // dgvMyTasks
            this.dgvMyTasks.AllowUserToAddRows = false;
            this.dgvMyTasks.AllowUserToDeleteRows = false;
            this.dgvMyTasks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMyTasks.Location = new System.Drawing.Point(6, 33);
            this.dgvMyTasks.Name = "dgvMyTasks";
            this.dgvMyTasks.ReadOnly = true;
            this.dgvMyTasks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMyTasks.Size = new System.Drawing.Size(956, 530);
            this.dgvMyTasks.TabIndex = 4;
            this.dgvMyTasks.SelectionChanged += new System.EventHandler(this.dgvMyTasks_SelectionChanged);

            // btnCompleteTask
            this.btnCompleteTask.Location = new System.Drawing.Point(6, 569);
            this.btnCompleteTask.Name = "btnCompleteTask";
            this.btnCompleteTask.Size = new System.Drawing.Size(100, 23);
            this.btnCompleteTask.TabIndex = 5;
            this.btnCompleteTask.Text = "Complete";
            this.btnCompleteTask.UseVisualStyleBackColor = true;
            this.btnCompleteTask.Click += new System.EventHandler(this.btnCompleteTask_Click);

            // btnDelegateTask
            this.btnDelegateTask.Location = new System.Drawing.Point(112, 569);
            this.btnDelegateTask.Name = "btnDelegateTask";
            this.btnDelegateTask.Size = new System.Drawing.Size(100, 23);
            this.btnDelegateTask.TabIndex = 6;
            this.btnDelegateTask.Text = "Delegate";
            this.btnDelegateTask.UseVisualStyleBackColor = true;
            this.btnDelegateTask.Click += new System.EventHandler(this.btnDelegateTask_Click);

            // btnRejectTask
            this.btnRejectTask.Location = new System.Drawing.Point(218, 569);
            this.btnRejectTask.Name = "btnRejectTask";
            this.btnRejectTask.Size = new System.Drawing.Size(100, 23);
            this.btnRejectTask.TabIndex = 7;
            this.btnRejectTask.Text = "Reject";
            this.btnRejectTask.UseVisualStyleBackColor = true;
            this.btnRejectTask.Click += new System.EventHandler(this.btnRejectTask_Click);

            // ===== MY WORKFLOWS TAB =====
            this.tabMyWorkflows.Controls.Add(this.lblWorkflowStatus);
            this.tabMyWorkflows.Controls.Add(this.cmbWorkflowStatus);
            this.tabMyWorkflows.Controls.Add(this.dgvMyWorkflows);
            this.tabMyWorkflows.Controls.Add(this.btnViewDetails);
            this.tabMyWorkflows.Location = new System.Drawing.Point(4, 22);
            this.tabMyWorkflows.Name = "tabMyWorkflows";
            this.tabMyWorkflows.Padding = new System.Windows.Forms.Padding(3);
            this.tabMyWorkflows.Size = new System.Drawing.Size(968, 621);
            this.tabMyWorkflows.TabIndex = 1;
            this.tabMyWorkflows.Text = "My Workflows";
            this.tabMyWorkflows.UseVisualStyleBackColor = true;

            // lblWorkflowStatus
            this.lblWorkflowStatus.AutoSize = true;
            this.lblWorkflowStatus.Location = new System.Drawing.Point(6, 9);
            this.lblWorkflowStatus.Name = "lblWorkflowStatus";
            this.lblWorkflowStatus.Size = new System.Drawing.Size(38, 13);
            this.lblWorkflowStatus.TabIndex = 0;
            this.lblWorkflowStatus.Text = "Status:";

            // cmbWorkflowStatus
            this.cmbWorkflowStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbWorkflowStatus.FormattingEnabled = true;
            this.cmbWorkflowStatus.Location = new System.Drawing.Point(50, 6);
            this.cmbWorkflowStatus.Name = "cmbWorkflowStatus";
            this.cmbWorkflowStatus.Size = new System.Drawing.Size(120, 21);
            this.cmbWorkflowStatus.TabIndex = 1;
            this.cmbWorkflowStatus.SelectedIndexChanged += new System.EventHandler(this.cmbWorkflowStatus_SelectedIndexChanged);

            // dgvMyWorkflows
            this.dgvMyWorkflows.AllowUserToAddRows = false;
            this.dgvMyWorkflows.AllowUserToDeleteRows = false;
            this.dgvMyWorkflows.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMyWorkflows.Location = new System.Drawing.Point(6, 33);
            this.dgvMyWorkflows.Name = "dgvMyWorkflows";
            this.dgvMyWorkflows.ReadOnly = true;
            this.dgvMyWorkflows.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMyWorkflows.Size = new System.Drawing.Size(956, 530);
            this.dgvMyWorkflows.TabIndex = 2;
            this.dgvMyWorkflows.SelectionChanged += new System.EventHandler(this.dgvMyWorkflows_SelectionChanged);

            // btnViewDetails
            this.btnViewDetails.Location = new System.Drawing.Point(6, 569);
            this.btnViewDetails.Name = "btnViewDetails";
            this.btnViewDetails.Size = new System.Drawing.Size(100, 23);
            this.btnViewDetails.TabIndex = 3;
            this.btnViewDetails.Text = "View Details";
            this.btnViewDetails.UseVisualStyleBackColor = true;
            this.btnViewDetails.Click += new System.EventHandler(this.btnViewDetails_Click);

            // ===== SLA METRICS TAB =====
            this.tabSLAMetrics.Controls.Add(this.pnlSLAMetrics);
            this.tabSLAMetrics.Location = new System.Drawing.Point(4, 22);
            this.tabSLAMetrics.Name = "tabSLAMetrics";
            this.tabSLAMetrics.Padding = new System.Windows.Forms.Padding(3);
            this.tabSLAMetrics.Size = new System.Drawing.Size(968, 621);
            this.tabSLAMetrics.TabIndex = 2;
            this.tabSLAMetrics.Text = "SLA Metrics";
            this.tabSLAMetrics.UseVisualStyleBackColor = true;

            // pnlSLAMetrics
            this.pnlSLAMetrics.Controls.Add(this.lblOnTimeLabel);
            this.pnlSLAMetrics.Controls.Add(this.lblOnTimeCount);
            this.pnlSLAMetrics.Controls.Add(this.lblAtRiskLabel);
            this.pnlSLAMetrics.Controls.Add(this.lblAtRiskCount);
            this.pnlSLAMetrics.Controls.Add(this.lblOverdueLabel);
            this.pnlSLAMetrics.Controls.Add(this.lblOverdueCount);
            this.pnlSLAMetrics.Controls.Add(this.lblComplianceLabel);
            this.pnlSLAMetrics.Controls.Add(this.lblCompliancePercentage);
            this.pnlSLAMetrics.Controls.Add(this.prgCompliancePercentage);
            this.pnlSLAMetrics.Location = new System.Drawing.Point(6, 6);
            this.pnlSLAMetrics.Name = "pnlSLAMetrics";
            this.pnlSLAMetrics.Size = new System.Drawing.Size(956, 609);
            this.pnlSLAMetrics.TabIndex = 0;

            // lblOnTimeLabel
            this.lblOnTimeLabel.AutoSize = true;
            this.lblOnTimeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblOnTimeLabel.Location = new System.Drawing.Point(20, 20);
            this.lblOnTimeLabel.Name = "lblOnTimeLabel";
            this.lblOnTimeLabel.Size = new System.Drawing.Size(70, 17);
            this.lblOnTimeLabel.TabIndex = 0;
            this.lblOnTimeLabel.Text = "On Time:";

            // lblOnTimeCount
            this.lblOnTimeCount.AutoSize = true;
            this.lblOnTimeCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblOnTimeCount.ForeColor = System.Drawing.Color.Green;
            this.lblOnTimeCount.Location = new System.Drawing.Point(100, 20);
            this.lblOnTimeCount.Name = "lblOnTimeCount";
            this.lblOnTimeCount.Size = new System.Drawing.Size(16, 17);
            this.lblOnTimeCount.TabIndex = 1;
            this.lblOnTimeCount.Text = "0";

            // lblAtRiskLabel
            this.lblAtRiskLabel.AutoSize = true;
            this.lblAtRiskLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblAtRiskLabel.Location = new System.Drawing.Point(20, 50);
            this.lblAtRiskLabel.Name = "lblAtRiskLabel";
            this.lblAtRiskLabel.Size = new System.Drawing.Size(62, 17);
            this.lblAtRiskLabel.TabIndex = 2;
            this.lblAtRiskLabel.Text = "At Risk:";

            // lblAtRiskCount
            this.lblAtRiskCount.AutoSize = true;
            this.lblAtRiskCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblAtRiskCount.ForeColor = System.Drawing.Color.Orange;
            this.lblAtRiskCount.Location = new System.Drawing.Point(100, 50);
            this.lblAtRiskCount.Name = "lblAtRiskCount";
            this.lblAtRiskCount.Size = new System.Drawing.Size(16, 17);
            this.lblAtRiskCount.TabIndex = 3;
            this.lblAtRiskCount.Text = "0";

            // lblOverdueLabel
            this.lblOverdueLabel.AutoSize = true;
            this.lblOverdueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblOverdueLabel.Location = new System.Drawing.Point(20, 80);
            this.lblOverdueLabel.Name = "lblOverdueLabel";
            this.lblOverdueLabel.Size = new System.Drawing.Size(68, 17);
            this.lblOverdueLabel.TabIndex = 4;
            this.lblOverdueLabel.Text = "Overdue:";

            // lblOverdueCount
            this.lblOverdueCount.AutoSize = true;
            this.lblOverdueCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblOverdueCount.ForeColor = System.Drawing.Color.Red;
            this.lblOverdueCount.Location = new System.Drawing.Point(100, 80);
            this.lblOverdueCount.Name = "lblOverdueCount";
            this.lblOverdueCount.Size = new System.Drawing.Size(16, 17);
            this.lblOverdueCount.TabIndex = 5;
            this.lblOverdueCount.Text = "0";

            // lblComplianceLabel
            this.lblComplianceLabel.AutoSize = true;
            this.lblComplianceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblComplianceLabel.Location = new System.Drawing.Point(20, 120);
            this.lblComplianceLabel.Name = "lblComplianceLabel";
            this.lblComplianceLabel.Size = new System.Drawing.Size(100, 17);
            this.lblComplianceLabel.TabIndex = 6;
            this.lblComplianceLabel.Text = "Compliance %:";

            // lblCompliancePercentage
            this.lblCompliancePercentage.AutoSize = true;
            this.lblCompliancePercentage.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblCompliancePercentage.Location = new System.Drawing.Point(130, 120);
            this.lblCompliancePercentage.Name = "lblCompliancePercentage";
            this.lblCompliancePercentage.Size = new System.Drawing.Size(16, 17);
            this.lblCompliancePercentage.TabIndex = 7;
            this.lblCompliancePercentage.Text = "0";

            // prgCompliancePercentage
            this.prgCompliancePercentage.Location = new System.Drawing.Point(20, 150);
            this.prgCompliancePercentage.Name = "prgCompliancePercentage";
            this.prgCompliancePercentage.Size = new System.Drawing.Size(300, 23);
            this.prgCompliancePercentage.TabIndex = 8;
            this.prgCompliancePercentage.Value = 0;

            // WorkflowDashboardForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.btnNewWorkflow);
            this.Controls.Add(this.btnRefresh);
            this.Name = "WorkflowDashboardForm";
            this.Text = "My Tasks";
            this.Load += new System.EventHandler(this.WorkflowDashboardForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WorkflowDashboardForm_FormClosing);
            
            this.tabControl.ResumeLayout(false);
            this.tabMyTasks.ResumeLayout(false);
            this.tabMyTasks.PerformLayout();
            this.tabMyWorkflows.ResumeLayout(false);
            this.tabMyWorkflows.PerformLayout();
            this.tabSLAMetrics.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMyTasks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMyWorkflows)).EndInit();
            this.pnlSLAMetrics.ResumeLayout(false);
            this.pnlSLAMetrics.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
