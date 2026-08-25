namespace CMS.Presentation
{
    partial class FrmExternalUpload
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTargetFolder;
        private System.Windows.Forms.Label lblFolderValue;
        private System.Windows.Forms.Label lblLevel;
        private System.Windows.Forms.ComboBox cboLevel;
        private System.Windows.Forms.Panel pnlDragDrop;
        private System.Windows.Forms.Label lblDragDropHint;
        private System.Windows.Forms.DataGridView dgvFiles;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Panel line;

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
            this.lblTargetFolder = new System.Windows.Forms.Label();
            this.lblFolderValue = new System.Windows.Forms.Label();
            this.lblLevel = new System.Windows.Forms.Label();
            this.cboLevel = new System.Windows.Forms.ComboBox();
            this.pnlDragDrop = new System.Windows.Forms.Panel();
            this.lblDragDropHint = new System.Windows.Forms.Label();
            this.dgvFiles = new System.Windows.Forms.DataGridView();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnUpload = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.line = new System.Windows.Forms.Panel();
            this.pnlDragDrop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).BeginInit();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTargetFolder
            // 
            this.lblTargetFolder.AutoSize = true;
            this.lblTargetFolder.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.lblTargetFolder.Location = new System.Drawing.Point(20, 15);
            this.lblTargetFolder.Name = "lblTargetFolder";
            this.lblTargetFolder.Size = new System.Drawing.Size(91, 15);
            this.lblTargetFolder.TabIndex = 0;
            this.lblTargetFolder.Text = "โฟลเดอร์ปลายทาง:";
            // 
            // lblFolderValue
            // 
            this.lblFolderValue.AutoSize = true;
            this.lblFolderValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolderValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblFolderValue.Location = new System.Drawing.Point(125, 15);
            this.lblFolderValue.Name = "lblFolderValue";
            this.lblFolderValue.Size = new System.Drawing.Size(12, 15);
            this.lblFolderValue.TabIndex = 1;
            this.lblFolderValue.Text = "-";
            // 
            // lblLevel
            // 
            this.lblLevel.AutoSize = true;
            this.lblLevel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLevel.Location = new System.Drawing.Point(20, 45);
            this.lblLevel.Name = "lblLevel";
            this.lblLevel.Size = new System.Drawing.Size(78, 15);
            this.lblLevel.TabIndex = 2;
            this.lblLevel.Text = "ระดับความลับ:";
            // 
            // cboLevel
            // 
            this.cboLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLevel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboLevel.FormattingEnabled = true;
            this.cboLevel.Location = new System.Drawing.Point(125, 42);
            this.cboLevel.Name = "cboLevel";
            this.cboLevel.Size = new System.Drawing.Size(250, 23);
            this.cboLevel.TabIndex = 3;
            this.cboLevel.SelectedIndexChanged += new System.EventHandler(this.CboLevel_SelectedIndexChanged);
            // 
            // pnlDragDrop
            // 
            this.pnlDragDrop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(251)))), ((int)(((byte)(253)))));
            this.pnlDragDrop.Controls.Add(this.lblDragDropHint);
            this.pnlDragDrop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlDragDrop.Location = new System.Drawing.Point(20, 78);
            this.pnlDragDrop.Name = "pnlDragDrop";
            this.pnlDragDrop.Size = new System.Drawing.Size(510, 80);
            this.pnlDragDrop.TabIndex = 4;
            this.pnlDragDrop.Click += new System.EventHandler(this.PnlDragDrop_Click);
            this.pnlDragDrop.DragDrop += new System.Windows.Forms.DragEventHandler(this.PnlDragDrop_DragDrop);
            this.pnlDragDrop.DragEnter += new System.Windows.Forms.DragEventHandler(this.PnlDragDrop_DragEnter);
            this.pnlDragDrop.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlDragDrop_Paint);
            // 
            // lblDragDropHint
            // 
            this.lblDragDropHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDragDropHint.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDragDropHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(120)))), ((int)(((byte)(150)))));
            this.lblDragDropHint.Location = new System.Drawing.Point(0, 0);
            this.lblDragDropHint.Name = "lblDragDropHint";
            this.lblDragDropHint.Size = new System.Drawing.Size(510, 80);
            this.lblDragDropHint.TabIndex = 0;
            this.lblDragDropHint.Text = "ลากไฟล์มาวางที่นี่ หรือ คลิกเพื่อค้นหาไฟล์...";
            this.lblDragDropHint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDragDropHint.Click += new System.EventHandler(this.PnlDragDrop_Click);
            // 
            // dgvFiles
            // 
            this.dgvFiles.AllowUserToAddRows = false;
            this.dgvFiles.AllowUserToDeleteRows = true;
            this.dgvFiles.BackgroundColor = System.Drawing.Color.White;
            this.dgvFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFiles.Location = new System.Drawing.Point(20, 175);
            this.dgvFiles.Name = "dgvFiles";
            this.dgvFiles.Size = new System.Drawing.Size(510, 160);
            this.dgvFiles.TabIndex = 5;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(20, 345);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(510, 15);
            this.progressBar.TabIndex = 6;
            this.progressBar.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.lblStatus.Location = new System.Drawing.Point(20, 365);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(510, 20);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "พร้อมอัปโหลด";
            // 
            // btnUpload
            // 
            this.btnUpload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.btnUpload.FlatAppearance.BorderSize = 0;
            this.btnUpload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpload.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUpload.ForeColor = System.Drawing.Color.White;
            this.btnUpload.Location = new System.Drawing.Point(340, 11);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(95, 30);
            this.btnUpload.TabIndex = 8;
            this.btnUpload.Text = "อัปโหลด";
            this.btnUpload.UseVisualStyleBackColor = false;
            this.btnUpload.Click += new System.EventHandler(this.BtnUpload_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(190)))), ((int)(((byte)(190)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.btnCancel.Location = new System.Drawing.Point(445, 11);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(85, 30);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(240)))), ((int)(((byte)(245)))));
            this.pnlFooter.Controls.Add(this.btnUpload);
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 398);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(550, 52);
            this.pnlFooter.TabIndex = 10;
            // 
            // line
            // 
            this.line.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(215)))), ((int)(((byte)(225)))));
            this.line.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.line.Location = new System.Drawing.Point(0, 397);
            this.line.Name = "line";
            this.line.Size = new System.Drawing.Size(550, 1);
            this.line.TabIndex = 11;
            // 
            // FrmExternalUpload
            // 
            this.AcceptButton = this.btnUpload;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(550, 450);
            this.Controls.Add(this.line);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.dgvFiles);
            this.Controls.Add(this.pnlDragDrop);
            this.Controls.Add(this.cboLevel);
            this.Controls.Add(this.lblLevel);
            this.Controls.Add(this.lblFolderValue);
            this.Controls.Add(this.lblTargetFolder);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmExternalUpload";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "อัปโหลดไฟล์ (โปรแกรมภายนอก)";
            this.Load += new System.EventHandler(this.FrmExternalUpload_Load);
            this.pnlDragDrop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).EndInit();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
