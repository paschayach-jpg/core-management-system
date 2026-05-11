namespace CMS.Presentation
{
    partial class FileUploadForm
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
            this.pnlFolderPath = new System.Windows.Forms.Panel();
            this.lblBadge = new System.Windows.Forms.Label();
            this.lblFolderPath = new System.Windows.Forms.Label();
            this.lblFolderLabel = new System.Windows.Forms.Label();
            this.pnlUploadZone = new System.Windows.Forms.Panel();
            this.lblUploadIcon = new System.Windows.Forms.Label();
            this.lblUploadLine1 = new System.Windows.Forms.Label();
            this.lblUploadLine2 = new System.Windows.Forms.Label();
            this.pnlFileList = new System.Windows.Forms.Panel();
            this.pnlFileRows = new System.Windows.Forms.Panel();
            this.lblSummary = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnClearErrors = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgressText = new System.Windows.Forms.Label();
            this.lblQuotaWarning = new System.Windows.Forms.Label();
            this.btnPickFiles = new System.Windows.Forms.Button();
            this.btnUpload = new System.Windows.Forms.Button();
            this.btnDownload = new System.Windows.Forms.Button();
            this.pnlFolderPath.SuspendLayout();
            this.pnlUploadZone.SuspendLayout();
            this.pnlFileList.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlFolderPath
            // 
            this.pnlFolderPath.BackColor = System.Drawing.Color.White;
            this.pnlFolderPath.Controls.Add(this.lblBadge);
            this.pnlFolderPath.Controls.Add(this.lblFolderPath);
            this.pnlFolderPath.Controls.Add(this.lblFolderLabel);
            this.pnlFolderPath.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFolderPath.Location = new System.Drawing.Point(0, 0);
            this.pnlFolderPath.Name = "pnlFolderPath";
            this.pnlFolderPath.Size = new System.Drawing.Size(540, 40);
            this.pnlFolderPath.TabIndex = 2;
            // 
            // lblBadge
            // 
            this.lblBadge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.lblBadge.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblBadge.ForeColor = System.Drawing.Color.White;
            this.lblBadge.Location = new System.Drawing.Point(488, 11);
            this.lblBadge.Name = "lblBadge";
            this.lblBadge.Size = new System.Drawing.Size(36, 18);
            this.lblBadge.TabIndex = 0;
            this.lblBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBadge.Visible = false;
            // 
            // lblFolderPath
            // 
            this.lblFolderPath.AutoSize = true;
            this.lblFolderPath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFolderPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblFolderPath.Location = new System.Drawing.Point(122, 12);
            this.lblFolderPath.Name = "lblFolderPath";
            this.lblFolderPath.Size = new System.Drawing.Size(19, 15);
            this.lblFolderPath.TabIndex = 1;
            this.lblFolderPath.Text = "—";
            // 
            // lblFolderLabel
            // 
            this.lblFolderLabel.AutoSize = true;
            this.lblFolderLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFolderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.lblFolderLabel.Location = new System.Drawing.Point(12, 12);
            this.lblFolderLabel.Name = "lblFolderLabel";
            this.lblFolderLabel.Size = new System.Drawing.Size(87, 15);
            this.lblFolderLabel.TabIndex = 2;
            this.lblFolderLabel.Text = "Folder ปลายทาง:";
            // 
            // pnlUploadZone
            // 
            this.pnlUploadZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(251)))), ((int)(((byte)(253)))));
            this.pnlUploadZone.Controls.Add(this.lblUploadIcon);
            this.pnlUploadZone.Controls.Add(this.lblUploadLine1);
            this.pnlUploadZone.Controls.Add(this.lblUploadLine2);
            this.pnlUploadZone.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlUploadZone.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlUploadZone.Location = new System.Drawing.Point(0, 40);
            this.pnlUploadZone.Name = "pnlUploadZone";
            this.pnlUploadZone.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.pnlUploadZone.Size = new System.Drawing.Size(540, 105);
            this.pnlUploadZone.TabIndex = 1;
            this.pnlUploadZone.Click += new System.EventHandler(this.btnPickFiles_Click);
            this.pnlUploadZone.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlUploadZone_Paint);
            // 
            // lblUploadIcon
            // 
            this.lblUploadIcon.BackColor = System.Drawing.Color.Transparent;
            this.lblUploadIcon.Font = new System.Drawing.Font("Segoe UI Emoji", 20F);
            this.lblUploadIcon.Location = new System.Drawing.Point(248, 4);
            this.lblUploadIcon.Name = "lblUploadIcon";
            this.lblUploadIcon.Size = new System.Drawing.Size(44, 44);
            this.lblUploadIcon.TabIndex = 0;
            this.lblUploadIcon.Text = "📁";
            this.lblUploadIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUploadIcon.Click += new System.EventHandler(this.btnPickFiles_Click);
            // 
            // lblUploadLine1
            // 
            this.lblUploadLine1.BackColor = System.Drawing.Color.Transparent;
            this.lblUploadLine1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblUploadLine1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblUploadLine1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.lblUploadLine1.Location = new System.Drawing.Point(8, 65);
            this.lblUploadLine1.Name = "lblUploadLine1";
            this.lblUploadLine1.Size = new System.Drawing.Size(524, 22);
            this.lblUploadLine1.TabIndex = 1;
            this.lblUploadLine1.Text = "ลากและวางไฟล์ที่นี่ หรือคลิกเพื่อเลือกไฟล์";
            this.lblUploadLine1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUploadLine1.Click += new System.EventHandler(this.btnPickFiles_Click);
            // 
            // lblUploadLine2
            // 
            this.lblUploadLine2.BackColor = System.Drawing.Color.Transparent;
            this.lblUploadLine2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblUploadLine2.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lblUploadLine2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this.lblUploadLine2.Location = new System.Drawing.Point(8, 87);
            this.lblUploadLine2.Name = "lblUploadLine2";
            this.lblUploadLine2.Size = new System.Drawing.Size(524, 18);
            this.lblUploadLine2.TabIndex = 2;
                this.lblUploadLine2.Text = "รองรับ: PDF, DOC/X, XLS/X, PPT/X, JPG, PNG, GIF, ZIP (สูงสุด 100 MB/ไฟล์ · 40 ไฟล" +
    "์ต่อครั้ง)";
            this.lblUploadLine2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUploadLine2.Click += new System.EventHandler(this.btnPickFiles_Click);
            // 
            // pnlFileList
            // 
            this.pnlFileList.BackColor = System.Drawing.Color.White;
            this.pnlFileList.Controls.Add(this.pnlFileRows);
            this.pnlFileList.Controls.Add(this.lblSummary);
            this.pnlFileList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFileList.Location = new System.Drawing.Point(0, 145);
            this.pnlFileList.Name = "pnlFileList";
            this.pnlFileList.Size = new System.Drawing.Size(540, 303);
            this.pnlFileList.TabIndex = 0;
            // 
            // pnlFileRows
            // 
            this.pnlFileRows.AutoScroll = true;
            this.pnlFileRows.BackColor = System.Drawing.Color.White;
            this.pnlFileRows.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFileRows.Location = new System.Drawing.Point(0, 0);
            this.pnlFileRows.Name = "pnlFileRows";
            this.pnlFileRows.Size = new System.Drawing.Size(540, 279);
            this.pnlFileRows.TabIndex = 0;
            // 
            // lblSummary
            // 
            this.lblSummary.BackColor = System.Drawing.Color.White;
            this.lblSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblSummary.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSummary.Location = new System.Drawing.Point(0, 279);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Padding = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.lblSummary.Size = new System.Drawing.Size(540, 24);
            this.lblSummary.TabIndex = 1;
            this.lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlFooter
            //
            this.pnlFooter.BackColor = System.Drawing.Color.White;
            this.pnlFooter.Controls.Add(this.btnClearErrors);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 448);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(540, 52);
            this.pnlFooter.TabIndex = 3;
            //
            // btnClearErrors
            //
            this.btnClearErrors.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(100)))), ((int)(((byte)(0)))));
            this.btnClearErrors.FlatAppearance.BorderSize = 0;
            this.btnClearErrors.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearErrors.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnClearErrors.ForeColor = System.Drawing.Color.White;
            this.btnClearErrors.Location = new System.Drawing.Point(12, 10);
            this.btnClearErrors.Name = "btnClearErrors";
            this.btnClearErrors.Size = new System.Drawing.Size(160, 32);
            this.btnClearErrors.TabIndex = 0;
            this.btnClearErrors.Text = "🗑 Clear รายการ Error";
            this.btnClearErrors.UseVisualStyleBackColor = false;
            this.btnClearErrors.Visible = false;
            this.btnClearErrors.Click += new System.EventHandler(this.btnClearErrors_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(0, 0);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(0, 0);
            this.progressBar.TabIndex = 4;
            this.progressBar.Visible = false;
            // 
            // lblProgressText
            // 
            this.lblProgressText.Location = new System.Drawing.Point(0, 0);
            this.lblProgressText.Name = "lblProgressText";
            this.lblProgressText.Size = new System.Drawing.Size(0, 0);
            this.lblProgressText.TabIndex = 5;
            this.lblProgressText.Visible = false;
            // 
            // lblQuotaWarning
            // 
            this.lblQuotaWarning.ForeColor = System.Drawing.Color.DarkOrange;
            this.lblQuotaWarning.Location = new System.Drawing.Point(0, 0);
            this.lblQuotaWarning.Name = "lblQuotaWarning";
            this.lblQuotaWarning.Size = new System.Drawing.Size(0, 0);
            this.lblQuotaWarning.TabIndex = 6;
            this.lblQuotaWarning.Visible = false;
            // 
            // btnPickFiles
            // 
            this.btnPickFiles.Location = new System.Drawing.Point(0, 0);
            this.btnPickFiles.Name = "btnPickFiles";
            this.btnPickFiles.Size = new System.Drawing.Size(0, 0);
            this.btnPickFiles.TabIndex = 7;
            this.btnPickFiles.Visible = false;
            this.btnPickFiles.Click += new System.EventHandler(this.btnPickFiles_Click);
            // 
            // btnUpload
            // 
            this.btnUpload.Location = new System.Drawing.Point(0, 0);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(0, 0);
            this.btnUpload.TabIndex = 8;
            this.btnUpload.Visible = false;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(0, 0);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(0, 0);
            this.btnDownload.TabIndex = 9;
            this.btnDownload.Visible = false;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // FileUploadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(540, 500);
            this.Controls.Add(this.pnlFileList);
            this.Controls.Add(this.pnlUploadZone);
            this.Controls.Add(this.pnlFolderPath);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblProgressText);
            this.Controls.Add(this.lblQuotaWarning);
            this.Controls.Add(this.btnPickFiles);
            this.Controls.Add(this.btnUpload);
            this.Controls.Add(this.btnDownload);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FileUploadForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "อัปโหลดไฟล์";
            this.Load += new System.EventHandler(this.FileUploadForm_Load);
            this.pnlFolderPath.ResumeLayout(false);
            this.pnlFolderPath.PerformLayout();
            this.pnlUploadZone.ResumeLayout(false);
            this.pnlFileList.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        // ── Field declarations ────────────────────────────────────────────────
        private System.Windows.Forms.Panel  pnlFolderPath;
        private System.Windows.Forms.Label  lblFolderLabel;
        private System.Windows.Forms.Label  lblFolderPath;
        private System.Windows.Forms.Label  lblBadge;

        private System.Windows.Forms.Panel  pnlUploadZone;
        private System.Windows.Forms.Label  lblUploadIcon;
        private System.Windows.Forms.Label  lblUploadLine1;
        private System.Windows.Forms.Label  lblUploadLine2;

        private System.Windows.Forms.Panel  pnlFileList;
        private System.Windows.Forms.Panel  pnlFileRows;
        private System.Windows.Forms.Label  lblSummary;

        private System.Windows.Forms.Panel  pnlFooter;
        private System.Windows.Forms.Button btnClearErrors;

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label       lblProgressText;
        private System.Windows.Forms.Label       lblQuotaWarning;
        private System.Windows.Forms.Button      btnPickFiles;
        private System.Windows.Forms.Button      btnUpload;
        private System.Windows.Forms.Button      btnDownload;
    }
}
