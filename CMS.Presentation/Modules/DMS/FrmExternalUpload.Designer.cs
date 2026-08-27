namespace CMS.Presentation
{
    partial class FrmExternalUpload
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblExplorerHeader;
        private CMS.Presentation.UserControls.FolderExplorerControl folderExplorer;
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
        private System.Windows.Forms.Button btnDone;
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
            this.lblExplorerHeader = new System.Windows.Forms.Label();
            this.folderExplorer = new CMS.Presentation.UserControls.FolderExplorerControl();
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
            this.btnDone = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.line = new System.Windows.Forms.Panel();
            this.pnlDragDrop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).BeginInit();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            //
            // lblExplorerHeader
            //
            this.lblExplorerHeader.AutoSize = true;
            this.lblExplorerHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExplorerHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblExplorerHeader.Location = new System.Drawing.Point(16, 12);
            this.lblExplorerHeader.Name = "lblExplorerHeader";
            this.lblExplorerHeader.Size = new System.Drawing.Size(120, 15);
            this.lblExplorerHeader.TabIndex = 0;
            this.lblExplorerHeader.Text = "📂 โครงสร้างโฟลเดอร์";
            //
            // folderExplorer
            //
            this.folderExplorer.BackColor = System.Drawing.Color.White;
            this.folderExplorer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.folderExplorer.Location = new System.Drawing.Point(16, 34);
            this.folderExplorer.Name = "folderExplorer";
            this.folderExplorer.Size = new System.Drawing.Size(430, 505);
            this.folderExplorer.TabIndex = 1;
            //
            // lblTargetFolder
            //
            this.lblTargetFolder.AutoSize = true;
            this.lblTargetFolder.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.lblTargetFolder.Location = new System.Drawing.Point(462, 15);
            this.lblTargetFolder.Name = "lblTargetFolder";
            this.lblTargetFolder.Size = new System.Drawing.Size(91, 15);
            this.lblTargetFolder.TabIndex = 2;
            this.lblTargetFolder.Text = "โฟลเดอร์ปลายทาง:";
            //
            // lblFolderValue
            //
            this.lblFolderValue.AutoSize = true;
            this.lblFolderValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolderValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblFolderValue.Location = new System.Drawing.Point(575, 15);
            this.lblFolderValue.MaximumSize = new System.Drawing.Size(390, 0);
            this.lblFolderValue.Name = "lblFolderValue";
            this.lblFolderValue.Size = new System.Drawing.Size(12, 15);
            this.lblFolderValue.TabIndex = 3;
            this.lblFolderValue.Text = "-";
            //
            // lblLevel
            //
            this.lblLevel.AutoSize = true;
            this.lblLevel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLevel.Location = new System.Drawing.Point(462, 45);
            this.lblLevel.Name = "lblLevel";
            this.lblLevel.Size = new System.Drawing.Size(78, 15);
            this.lblLevel.TabIndex = 4;
            this.lblLevel.Text = "ระดับความลับ:";
            //
            // cboLevel
            //
            this.cboLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLevel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboLevel.FormattingEnabled = true;
            this.cboLevel.Location = new System.Drawing.Point(575, 42);
            this.cboLevel.Name = "cboLevel";
            this.cboLevel.Size = new System.Drawing.Size(300, 23);
            this.cboLevel.TabIndex = 5;
            this.cboLevel.SelectedIndexChanged += new System.EventHandler(this.CboLevel_SelectedIndexChanged);
            //
            // pnlDragDrop
            //
            this.pnlDragDrop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(251)))), ((int)(((byte)(253)))));
            this.pnlDragDrop.Controls.Add(this.lblDragDropHint);
            this.pnlDragDrop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlDragDrop.Location = new System.Drawing.Point(462, 80);
            this.pnlDragDrop.Name = "pnlDragDrop";
            this.pnlDragDrop.Size = new System.Drawing.Size(500, 80);
            this.pnlDragDrop.TabIndex = 6;
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
            this.lblDragDropHint.Size = new System.Drawing.Size(500, 80);
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
            this.dgvFiles.Location = new System.Drawing.Point(462, 175);
            this.dgvFiles.Name = "dgvFiles";
            this.dgvFiles.Size = new System.Drawing.Size(500, 320);
            this.dgvFiles.TabIndex = 7;
            //
            // progressBar
            //
            this.progressBar.Location = new System.Drawing.Point(462, 505);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(500, 15);
            this.progressBar.TabIndex = 8;
            this.progressBar.Visible = false;
            //
            // lblStatus
            //
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.lblStatus.Location = new System.Drawing.Point(462, 523);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(500, 20);
            this.lblStatus.TabIndex = 9;
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
            this.btnUpload.Location = new System.Drawing.Point(673, 11);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(100, 30);
            this.btnUpload.TabIndex = 10;
            this.btnUpload.Text = "อัปโหลด";
            this.btnUpload.UseVisualStyleBackColor = false;
            this.btnUpload.Click += new System.EventHandler(this.BtnUpload_Click);
            //
            // btnDone
            //
            this.btnDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(155)))), ((int)(((byte)(88)))));
            this.btnDone.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnDone.Enabled = false;
            this.btnDone.FlatAppearance.BorderSize = 0;
            this.btnDone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDone.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDone.ForeColor = System.Drawing.Color.White;
            this.btnDone.Location = new System.Drawing.Point(783, 11);
            this.btnDone.Name = "btnDone";
            this.btnDone.Size = new System.Drawing.Size(90, 30);
            this.btnDone.TabIndex = 11;
            this.btnDone.Text = "เสร็จสิ้น";
            this.btnDone.UseVisualStyleBackColor = false;
            this.btnDone.Click += new System.EventHandler(this.BtnDone_Click);
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
            this.btnCancel.Location = new System.Drawing.Point(883, 11);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            //
            // pnlFooter
            //
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(240)))), ((int)(((byte)(245)))));
            this.pnlFooter.Controls.Add(this.btnUpload);
            this.pnlFooter.Controls.Add(this.btnDone);
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 548);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(978, 52);
            this.pnlFooter.TabIndex = 12;
            //
            // line
            //
            this.line.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(215)))), ((int)(((byte)(225)))));
            this.line.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.line.Location = new System.Drawing.Point(0, 547);
            this.line.Name = "line";
            this.line.Size = new System.Drawing.Size(978, 1);
            this.line.TabIndex = 13;
            //
            // FrmExternalUpload
            //
            this.AcceptButton = this.btnUpload;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(978, 600);
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
            this.Controls.Add(this.folderExplorer);
            this.Controls.Add(this.lblExplorerHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmExternalUpload";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "อัปโหลดไฟล์ (โปรแกรมภายนอก)";
            this.Load += new System.EventHandler(this.FrmExternalUpload_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmExternalUpload_FormClosing);
            this.pnlDragDrop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFiles)).EndInit();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
