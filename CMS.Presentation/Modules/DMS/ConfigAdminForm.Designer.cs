namespace CMS.Presentation
{
    partial class ConfigAdminForm
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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.tblConfig = new System.Windows.Forms.TableLayoutPanel();
            this.lblPath = new System.Windows.Forms.Label();
            this.pnlPathRow = new System.Windows.Forms.Panel();
            this.txtNetworkDrivePath = new System.Windows.Forms.TextBox();
            this.btnBrowsePath = new System.Windows.Forms.Button();
            this.lblMaxSize = new System.Windows.Forms.Label();
            this.txtMaxFileSizeBytes = new System.Windows.Forms.TextBox();
            this.lblMaxFiles = new System.Windows.Forms.Label();
            this.txtMaxFilesPerUpload = new System.Windows.Forms.TextBox();
            this.lblExtensions = new System.Windows.Forms.Label();
            this.txtAllowedExtensions = new System.Windows.Forms.TextBox();
            this.lblQuota = new System.Windows.Forms.Label();
            this.txtDefaultQuotaBytes = new System.Windows.Forms.TextBox();
            this.lblApproval = new System.Windows.Forms.Label();
            this.txtApprovalTimeoutMinutes = new System.Windows.Forms.TextBox();
            this.lblAudit = new System.Windows.Forms.Label();
            this.txtAuditRetentionDays = new System.Windows.Forms.TextBox();
            this.lblVirusScan = new System.Windows.Forms.Label();
            this.txtVirusScanTimeoutSeconds = new System.Windows.Forms.TextBox();
            this.btnSaveConfig = new System.Windows.Forms.Button();
            this.grpPdpa = new System.Windows.Forms.GroupBox();
            this.lblPdpaDesc = new System.Windows.Forms.Label();
            this.btnRetention = new System.Windows.Forms.Button();
            this.lblPdpaUser = new System.Windows.Forms.Label();
            this.txtPdpaReportUserId = new System.Windows.Forms.TextBox();
            this.btnPdpaReport = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.tblConfig.SuspendLayout();
            this.pnlPathRow.SuspendLayout();
            this.grpPdpa.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.pnlHeader.Size = new System.Drawing.Size(620, 40);
            this.pnlHeader.TabIndex = 2;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(10, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(610, 40);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "⚙ System Configuration (Admin Only)";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tblConfig
            // 
            this.tblConfig.ColumnCount = 2;
            this.tblConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 240F));
            this.tblConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblConfig.Controls.Add(this.lblPath, 0, 0);
            this.tblConfig.Controls.Add(this.pnlPathRow, 1, 0);
            this.tblConfig.Controls.Add(this.lblMaxSize, 0, 1);
            this.tblConfig.Controls.Add(this.txtMaxFileSizeBytes, 1, 1);
            this.tblConfig.Controls.Add(this.lblMaxFiles, 0, 2);
            this.tblConfig.Controls.Add(this.txtMaxFilesPerUpload, 1, 2);
            this.tblConfig.Controls.Add(this.lblExtensions, 0, 3);
            this.tblConfig.Controls.Add(this.txtAllowedExtensions, 1, 3);
            this.tblConfig.Controls.Add(this.lblQuota, 0, 4);
            this.tblConfig.Controls.Add(this.txtDefaultQuotaBytes, 1, 4);
            this.tblConfig.Controls.Add(this.lblApproval, 0, 5);
            this.tblConfig.Controls.Add(this.txtApprovalTimeoutMinutes, 1, 5);
            this.tblConfig.Controls.Add(this.lblAudit, 0, 6);
            this.tblConfig.Controls.Add(this.txtAuditRetentionDays, 1, 6);
            this.tblConfig.Controls.Add(this.lblVirusScan, 0, 7);
            this.tblConfig.Controls.Add(this.txtVirusScanTimeoutSeconds, 1, 7);
            this.tblConfig.Controls.Add(this.btnSaveConfig, 1, 8);
            this.tblConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblConfig.Location = new System.Drawing.Point(0, 40);
            this.tblConfig.Name = "tblConfig";
            this.tblConfig.Padding = new System.Windows.Forms.Padding(12, 8, 12, 4);
            this.tblConfig.RowCount = 9;
            this.tblConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblConfig.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tblConfig.Size = new System.Drawing.Size(620, 350);
            this.tblConfig.TabIndex = 0;
            // 
            // lblPath
            // 
            this.lblPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPath.Location = new System.Drawing.Point(15, 8);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(234, 36);
            this.lblPath.TabIndex = 0;
            this.lblPath.Text = "Storage Path (NetworkDrivePath):";
            this.lblPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlPathRow
            // 
            this.pnlPathRow.Controls.Add(this.txtNetworkDrivePath);
            this.pnlPathRow.Controls.Add(this.btnBrowsePath);
            this.pnlPathRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPathRow.Location = new System.Drawing.Point(255, 11);
            this.pnlPathRow.Name = "pnlPathRow";
            this.pnlPathRow.Size = new System.Drawing.Size(350, 30);
            this.pnlPathRow.TabIndex = 1;
            // 
            // txtNetworkDrivePath
            // 
            this.txtNetworkDrivePath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNetworkDrivePath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtNetworkDrivePath.Location = new System.Drawing.Point(0, 0);
            this.txtNetworkDrivePath.Name = "txtNetworkDrivePath";
            this.txtNetworkDrivePath.Size = new System.Drawing.Size(270, 23);
            this.txtNetworkDrivePath.TabIndex = 0;
            // 
            // btnBrowsePath
            // 
            this.btnBrowsePath.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnBrowsePath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnBrowsePath.Location = new System.Drawing.Point(270, 0);
            this.btnBrowsePath.Name = "btnBrowsePath";
            this.btnBrowsePath.Size = new System.Drawing.Size(80, 30);
            this.btnBrowsePath.TabIndex = 1;
            this.btnBrowsePath.Text = "Browse...";
            this.btnBrowsePath.Click += new System.EventHandler(this.OnBrowsePathClick);
            // 
            // lblMaxSize
            // 
            this.lblMaxSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMaxSize.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMaxSize.Location = new System.Drawing.Point(15, 44);
            this.lblMaxSize.Name = "lblMaxSize";
            this.lblMaxSize.Size = new System.Drawing.Size(234, 36);
            this.lblMaxSize.TabIndex = 2;
            this.lblMaxSize.Text = "Max File Size (bytes):";
            this.lblMaxSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMaxFileSizeBytes
            // 
            this.txtMaxFileSizeBytes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMaxFileSizeBytes.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtMaxFileSizeBytes.Location = new System.Drawing.Point(255, 47);
            this.txtMaxFileSizeBytes.Name = "txtMaxFileSizeBytes";
            this.txtMaxFileSizeBytes.Size = new System.Drawing.Size(350, 23);
            this.txtMaxFileSizeBytes.TabIndex = 3;
            // 
            // lblMaxFiles
            // 
            this.lblMaxFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMaxFiles.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMaxFiles.Location = new System.Drawing.Point(15, 80);
            this.lblMaxFiles.Name = "lblMaxFiles";
            this.lblMaxFiles.Size = new System.Drawing.Size(234, 36);
            this.lblMaxFiles.TabIndex = 4;
            this.lblMaxFiles.Text = "Max Files Per Upload:";
            this.lblMaxFiles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMaxFilesPerUpload
            // 
            this.txtMaxFilesPerUpload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMaxFilesPerUpload.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtMaxFilesPerUpload.Location = new System.Drawing.Point(255, 83);
            this.txtMaxFilesPerUpload.Name = "txtMaxFilesPerUpload";
            this.txtMaxFilesPerUpload.Size = new System.Drawing.Size(350, 23);
            this.txtMaxFilesPerUpload.TabIndex = 5;
            // 
            // lblExtensions
            // 
            this.lblExtensions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblExtensions.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblExtensions.Location = new System.Drawing.Point(15, 116);
            this.lblExtensions.Name = "lblExtensions";
            this.lblExtensions.Size = new System.Drawing.Size(234, 36);
            this.lblExtensions.TabIndex = 6;
            this.lblExtensions.Text = "Allowed Extensions (comma-separated):";
            this.lblExtensions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtAllowedExtensions
            // 
            this.txtAllowedExtensions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAllowedExtensions.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtAllowedExtensions.Location = new System.Drawing.Point(255, 119);
            this.txtAllowedExtensions.Name = "txtAllowedExtensions";
            this.txtAllowedExtensions.Size = new System.Drawing.Size(350, 23);
            this.txtAllowedExtensions.TabIndex = 7;
            // 
            // lblQuota
            // 
            this.lblQuota.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblQuota.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblQuota.Location = new System.Drawing.Point(15, 152);
            this.lblQuota.Name = "lblQuota";
            this.lblQuota.Size = new System.Drawing.Size(234, 36);
            this.lblQuota.TabIndex = 8;
            this.lblQuota.Text = "Default Quota (bytes, 0=unlimited):";
            this.lblQuota.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtDefaultQuotaBytes
            // 
            this.txtDefaultQuotaBytes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDefaultQuotaBytes.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtDefaultQuotaBytes.Location = new System.Drawing.Point(255, 155);
            this.txtDefaultQuotaBytes.Name = "txtDefaultQuotaBytes";
            this.txtDefaultQuotaBytes.Size = new System.Drawing.Size(350, 23);
            this.txtDefaultQuotaBytes.TabIndex = 9;
            // 
            // lblApproval
            // 
            this.lblApproval.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblApproval.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblApproval.Location = new System.Drawing.Point(15, 188);
            this.lblApproval.Name = "lblApproval";
            this.lblApproval.Size = new System.Drawing.Size(234, 36);
            this.lblApproval.TabIndex = 10;
            this.lblApproval.Text = "Approval Timeout (minutes):";
            this.lblApproval.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtApprovalTimeoutMinutes
            // 
            this.txtApprovalTimeoutMinutes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtApprovalTimeoutMinutes.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtApprovalTimeoutMinutes.Location = new System.Drawing.Point(255, 191);
            this.txtApprovalTimeoutMinutes.Name = "txtApprovalTimeoutMinutes";
            this.txtApprovalTimeoutMinutes.Size = new System.Drawing.Size(350, 23);
            this.txtApprovalTimeoutMinutes.TabIndex = 11;
            // 
            // lblAudit
            // 
            this.lblAudit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAudit.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAudit.Location = new System.Drawing.Point(15, 224);
            this.lblAudit.Name = "lblAudit";
            this.lblAudit.Size = new System.Drawing.Size(234, 36);
            this.lblAudit.TabIndex = 12;
            this.lblAudit.Text = "Audit Retention (days):";
            this.lblAudit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtAuditRetentionDays
            // 
            this.txtAuditRetentionDays.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAuditRetentionDays.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtAuditRetentionDays.Location = new System.Drawing.Point(255, 227);
            this.txtAuditRetentionDays.Name = "txtAuditRetentionDays";
            this.txtAuditRetentionDays.Size = new System.Drawing.Size(350, 23);
            this.txtAuditRetentionDays.TabIndex = 13;
            // 
            // lblVirusScan
            // 
            this.lblVirusScan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVirusScan.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblVirusScan.Location = new System.Drawing.Point(15, 260);
            this.lblVirusScan.Name = "lblVirusScan";
            this.lblVirusScan.Size = new System.Drawing.Size(234, 36);
            this.lblVirusScan.TabIndex = 14;
            this.lblVirusScan.Text = "Virus Scan Timeout (seconds):";
            this.lblVirusScan.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtVirusScanTimeoutSeconds
            // 
            this.txtVirusScanTimeoutSeconds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtVirusScanTimeoutSeconds.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtVirusScanTimeoutSeconds.Location = new System.Drawing.Point(255, 263);
            this.txtVirusScanTimeoutSeconds.Name = "txtVirusScanTimeoutSeconds";
            this.txtVirusScanTimeoutSeconds.Size = new System.Drawing.Size(350, 23);
            this.txtVirusScanTimeoutSeconds.TabIndex = 15;
            // 
            // btnSaveConfig
            // 
            this.btnSaveConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.btnSaveConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveConfig.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSaveConfig.ForeColor = System.Drawing.Color.White;
            this.btnSaveConfig.Location = new System.Drawing.Point(465, 299);
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.Size = new System.Drawing.Size(140, 30);
            this.btnSaveConfig.TabIndex = 16;
            this.btnSaveConfig.Text = "บันทึกการตั้งค่า";
            this.btnSaveConfig.UseVisualStyleBackColor = false;
            this.btnSaveConfig.Click += new System.EventHandler(this.OnSaveClick);
            // 
            // grpPdpa
            // 
            this.grpPdpa.Controls.Add(this.lblPdpaDesc);
            this.grpPdpa.Controls.Add(this.btnRetention);
            this.grpPdpa.Controls.Add(this.lblPdpaUser);
            this.grpPdpa.Controls.Add(this.txtPdpaReportUserId);
            this.grpPdpa.Controls.Add(this.btnPdpaReport);
            this.grpPdpa.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.grpPdpa.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpPdpa.Location = new System.Drawing.Point(0, 390);
            this.grpPdpa.Name = "grpPdpa";
            this.grpPdpa.Padding = new System.Windows.Forms.Padding(8);
            this.grpPdpa.Size = new System.Drawing.Size(620, 130);
            this.grpPdpa.TabIndex = 1;
            this.grpPdpa.TabStop = false;
            this.grpPdpa.Text = "PDPA Retention Management";
            // 
            // lblPdpaDesc
            // 
            this.lblPdpaDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPdpaDesc.Location = new System.Drawing.Point(8, 20);
            this.lblPdpaDesc.Name = "lblPdpaDesc";
            this.lblPdpaDesc.Size = new System.Drawing.Size(460, 18);
            this.lblPdpaDesc.TabIndex = 0;
            this.lblPdpaDesc.Text = "Flag FileDocuments whose retention period has expired and notify Managers.";
            // 
            // btnRetention
            // 
            this.btnRetention.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRetention.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRetention.Location = new System.Drawing.Point(8, 44);
            this.btnRetention.Name = "btnRetention";
            this.btnRetention.Size = new System.Drawing.Size(160, 28);
            this.btnRetention.TabIndex = 1;
            this.btnRetention.Text = "Process Retention Flags";
            this.btnRetention.Click += new System.EventHandler(this.OnProcessRetentionClick);
            // 
            // lblPdpaUser
            // 
            this.lblPdpaUser.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPdpaUser.Location = new System.Drawing.Point(8, 84);
            this.lblPdpaUser.Name = "lblPdpaUser";
            this.lblPdpaUser.Size = new System.Drawing.Size(200, 20);
            this.lblPdpaUser.TabIndex = 2;
            this.lblPdpaUser.Text = "Generate PDPA Report — User ID:";
            // 
            // txtPdpaReportUserId
            // 
            this.txtPdpaReportUserId.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPdpaReportUserId.Location = new System.Drawing.Point(214, 81);
            this.txtPdpaReportUserId.Name = "txtPdpaReportUserId";
            this.txtPdpaReportUserId.Size = new System.Drawing.Size(80, 23);
            this.txtPdpaReportUserId.TabIndex = 3;
            // 
            // btnPdpaReport
            // 
            this.btnPdpaReport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.btnPdpaReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPdpaReport.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnPdpaReport.ForeColor = System.Drawing.Color.White;
            this.btnPdpaReport.Location = new System.Drawing.Point(302, 79);
            this.btnPdpaReport.Name = "btnPdpaReport";
            this.btnPdpaReport.Size = new System.Drawing.Size(130, 28);
            this.btnPdpaReport.TabIndex = 4;
            this.btnPdpaReport.Text = "Generate Report";
            this.btnPdpaReport.UseVisualStyleBackColor = false;
            this.btnPdpaReport.Click += new System.EventHandler(this.OnGenerateReportClick);
            // 
            // ConfigAdminForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 520);
            this.Controls.Add(this.tblConfig);
            this.Controls.Add(this.grpPdpa);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigAdminForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "System Configuration (Admin Only)";
            this.pnlHeader.ResumeLayout(false);
            this.tblConfig.ResumeLayout(false);
            this.tblConfig.PerformLayout();
            this.pnlPathRow.ResumeLayout(false);
            this.pnlPathRow.PerformLayout();
            this.grpPdpa.ResumeLayout(false);
            this.grpPdpa.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel              pnlHeader;
        private System.Windows.Forms.Label              lblTitle;
        private System.Windows.Forms.TableLayoutPanel   tblConfig;
        private System.Windows.Forms.Label              lblPath;
        private System.Windows.Forms.Panel              pnlPathRow;
        private System.Windows.Forms.TextBox            txtNetworkDrivePath;
        private System.Windows.Forms.Button             btnBrowsePath;
        private System.Windows.Forms.Label              lblMaxSize;
        private System.Windows.Forms.TextBox            txtMaxFileSizeBytes;
        private System.Windows.Forms.Label              lblMaxFiles;
        private System.Windows.Forms.TextBox            txtMaxFilesPerUpload;
        private System.Windows.Forms.Label              lblExtensions;
        private System.Windows.Forms.TextBox            txtAllowedExtensions;
        private System.Windows.Forms.Label              lblQuota;
        private System.Windows.Forms.TextBox            txtDefaultQuotaBytes;
        private System.Windows.Forms.Label              lblApproval;
        private System.Windows.Forms.TextBox            txtApprovalTimeoutMinutes;
        private System.Windows.Forms.Label              lblAudit;
        private System.Windows.Forms.TextBox            txtAuditRetentionDays;
        private System.Windows.Forms.Label              lblVirusScan;
        private System.Windows.Forms.TextBox            txtVirusScanTimeoutSeconds;
        private System.Windows.Forms.Button             btnSaveConfig;
        private System.Windows.Forms.GroupBox           grpPdpa;
        private System.Windows.Forms.Label              lblPdpaDesc;
        private System.Windows.Forms.Button             btnRetention;
        private System.Windows.Forms.Label              lblPdpaUser;
        private System.Windows.Forms.TextBox            txtPdpaReportUserId;
        private System.Windows.Forms.Button             btnPdpaReport;
    }
}
