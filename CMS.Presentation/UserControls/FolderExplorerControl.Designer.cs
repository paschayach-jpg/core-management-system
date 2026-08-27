namespace CMS.Presentation.UserControls
{
    partial class FolderExplorerControl
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.ToolStrip tsToolbar;
        private System.Windows.Forms.ToolStripButton btnNewFolder;
        private System.Windows.Forms.ToolStripButton btnDeleteFolder;
        private System.Windows.Forms.ToolStripSeparator sep0;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.ToolStripButton btnVerify;
        private System.Windows.Forms.ToolStripSeparator sep1;
        private System.Windows.Forms.ContextMenuStrip cmsFiles;
        private System.Windows.Forms.ToolStripMenuItem miDeleteFile;
        private System.Windows.Forms.ToolStripButton btnExpand;
        private System.Windows.Forms.ToolStripButton btnCollapse;
        private System.Windows.Forms.SplitContainer split;
        private System.Windows.Forms.TreeView treeFolders;
        private System.Windows.Forms.Label lblFilesHeader;
        private System.Windows.Forms.ListView lstFiles;
        private System.Windows.Forms.ColumnHeader colIcon;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colSize;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colDate;
        private System.Windows.Forms.ColumnHeader colFileState;

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
            this.tsToolbar = new System.Windows.Forms.ToolStrip();
            this.btnNewFolder = new System.Windows.Forms.ToolStripButton();
            this.btnDeleteFolder = new System.Windows.Forms.ToolStripButton();
            this.sep0 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.btnVerify = new System.Windows.Forms.ToolStripButton();
            this.sep1 = new System.Windows.Forms.ToolStripSeparator();
            this.cmsFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miDeleteFile = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExpand = new System.Windows.Forms.ToolStripButton();
            this.btnCollapse = new System.Windows.Forms.ToolStripButton();
            this.split = new System.Windows.Forms.SplitContainer();
            this.treeFolders = new System.Windows.Forms.TreeView();
            this.lblFilesHeader = new System.Windows.Forms.Label();
            this.lstFiles = new System.Windows.Forms.ListView();
            this.colIcon = new System.Windows.Forms.ColumnHeader();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colSize = new System.Windows.Forms.ColumnHeader();
            this.colType = new System.Windows.Forms.ColumnHeader();
            this.colDate = new System.Windows.Forms.ColumnHeader();
            this.colFileState = new System.Windows.Forms.ColumnHeader();
            this.tsToolbar.SuspendLayout();
            this.cmsFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split)).BeginInit();
            this.split.Panel1.SuspendLayout();
            this.split.Panel2.SuspendLayout();
            this.split.SuspendLayout();
            this.SuspendLayout();
            //
            // tsToolbar
            //
            this.tsToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.btnNewFolder,
                this.btnDeleteFolder,
                this.sep0,
                this.btnRefresh,
                this.btnVerify,
                this.sep1,
                this.btnExpand,
                this.btnCollapse});
            this.tsToolbar.Location = new System.Drawing.Point(0, 0);
            this.tsToolbar.Name = "tsToolbar";
            this.tsToolbar.Size = new System.Drawing.Size(400, 25);
            this.tsToolbar.TabIndex = 0;
            //
            // btnNewFolder
            //
            this.btnNewFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnNewFolder.Name = "btnNewFolder";
            this.btnNewFolder.Size = new System.Drawing.Size(90, 22);
            this.btnNewFolder.Text = "＋ โฟลเดอร์ย่อย";
            this.btnNewFolder.Click += new System.EventHandler(this.BtnNewFolder_Click);
            //
            // btnDeleteFolder
            //
            this.btnDeleteFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnDeleteFolder.Enabled = false;
            this.btnDeleteFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnDeleteFolder.Name = "btnDeleteFolder";
            this.btnDeleteFolder.Size = new System.Drawing.Size(80, 22);
            this.btnDeleteFolder.Text = "🗑 ลบโฟลเดอร์";
            this.btnDeleteFolder.Click += new System.EventHandler(this.BtnDeleteFolder_Click);
            //
            // sep0
            //
            this.sep0.Name = "sep0";
            this.sep0.Size = new System.Drawing.Size(6, 25);
            //
            // btnRefresh
            //
            this.btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(60, 22);
            this.btnRefresh.Text = "⟳ รีเฟรช";
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            //
            // btnVerify
            //
            this.btnVerify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new System.Drawing.Size(90, 22);
            this.btnVerify.Text = "🔍 ตรวจสอบไฟล์";
            this.btnVerify.Visible = false;
            this.btnVerify.Click += new System.EventHandler(this.BtnVerify_Click);
            //
            // sep1
            //
            this.sep1.Name = "sep1";
            this.sep1.Size = new System.Drawing.Size(6, 25);
            //
            // cmsFiles
            //
            this.cmsFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.miDeleteFile});
            this.cmsFiles.Name = "cmsFiles";
            this.cmsFiles.Size = new System.Drawing.Size(120, 26);
            this.cmsFiles.Opening += new System.ComponentModel.CancelEventHandler(this.CmsFiles_Opening);
            //
            // miDeleteFile
            //
            this.miDeleteFile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.miDeleteFile.Name = "miDeleteFile";
            this.miDeleteFile.Size = new System.Drawing.Size(119, 22);
            this.miDeleteFile.Text = "🗑 ลบไฟล์";
            this.miDeleteFile.Click += new System.EventHandler(this.MiDeleteFile_Click);
            //
            // btnExpand
            //
            this.btnExpand.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnExpand.Name = "btnExpand";
            this.btnExpand.Size = new System.Drawing.Size(60, 22);
            this.btnExpand.Text = "ขยายหมด";
            this.btnExpand.Click += new System.EventHandler(this.BtnExpand_Click);
            //
            // btnCollapse
            //
            this.btnCollapse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnCollapse.Name = "btnCollapse";
            this.btnCollapse.Size = new System.Drawing.Size(60, 22);
            this.btnCollapse.Text = "ยุบหมด";
            this.btnCollapse.Click += new System.EventHandler(this.BtnCollapse_Click);
            //
            // split
            //
            this.split.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split.Location = new System.Drawing.Point(0, 25);
            this.split.Name = "split";
            this.split.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // split.Panel1
            //
            this.split.Panel1.Controls.Add(this.treeFolders);
            this.split.Panel1MinSize = 120;
            //
            // split.Panel2
            //
            this.split.Panel2.Controls.Add(this.lstFiles);
            this.split.Panel2.Controls.Add(this.lblFilesHeader);
            this.split.Panel2MinSize = 100;
            this.split.Size = new System.Drawing.Size(400, 475);
            this.split.SplitterDistance = 260;
            this.split.TabIndex = 1;
            //
            // treeFolders
            //
            this.treeFolders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeFolders.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.treeFolders.FullRowSelect = true;
            this.treeFolders.HideSelection = false;
            this.treeFolders.Location = new System.Drawing.Point(0, 0);
            this.treeFolders.Name = "treeFolders";
            this.treeFolders.ShowRootLines = true;
            this.treeFolders.Size = new System.Drawing.Size(400, 260);
            this.treeFolders.TabIndex = 0;
            this.treeFolders.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeFolders_AfterSelect);
            //
            // lblFilesHeader
            //
            this.lblFilesHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(240)))), ((int)(((byte)(245)))));
            this.lblFilesHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblFilesHeader.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblFilesHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblFilesHeader.Location = new System.Drawing.Point(0, 0);
            this.lblFilesHeader.Name = "lblFilesHeader";
            this.lblFilesHeader.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.lblFilesHeader.Size = new System.Drawing.Size(400, 22);
            this.lblFilesHeader.TabIndex = 1;
            this.lblFilesHeader.Text = "ไฟล์ในโฟลเดอร์นี้";
            this.lblFilesHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lstFiles
            //
            this.lstFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colIcon,
                this.colName,
                this.colSize,
                this.colType,
                this.colDate,
                this.colFileState});
            this.lstFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstFiles.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstFiles.FullRowSelect = true;
            this.lstFiles.HideSelection = false;
            this.lstFiles.Location = new System.Drawing.Point(0, 22);
            this.lstFiles.MultiSelect = false;
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.Size = new System.Drawing.Size(400, 189);
            this.lstFiles.TabIndex = 0;
            this.lstFiles.UseCompatibleStateImageBehavior = false;
            this.lstFiles.View = System.Windows.Forms.View.Details;
            this.lstFiles.ContextMenuStrip = this.cmsFiles;
            this.lstFiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LstFiles_MouseDown);
            this.lstFiles.SelectedIndexChanged += new System.EventHandler(this.LstFiles_SelectedIndexChanged);
            this.lstFiles.DoubleClick += new System.EventHandler(this.LstFiles_DoubleClick);
            //
            // colIcon
            //
            this.colIcon.Text = "";
            this.colIcon.Width = 28;
            //
            // colName
            //
            this.colName.Text = "ชื่อไฟล์";
            this.colName.Width = 180;
            //
            // colSize
            //
            this.colSize.Text = "ขนาด";
            this.colSize.Width = 80;
            //
            // colType
            //
            this.colType.Text = "ประเภท";
            this.colType.Width = 60;
            //
            // colDate
            //
            this.colDate.Text = "อัปโหลดเมื่อ";
            this.colDate.Width = 120;
            //
            // colFileState
            //
            this.colFileState.Text = "สถานะ";
            this.colFileState.Width = 110;
            //
            // FolderExplorerControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.split);
            this.Controls.Add(this.tsToolbar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FolderExplorerControl";
            this.Size = new System.Drawing.Size(400, 500);
            this.tsToolbar.ResumeLayout(false);
            this.tsToolbar.PerformLayout();
            this.cmsFiles.ResumeLayout(false);
            this.split.Panel1.ResumeLayout(false);
            this.split.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.split)).EndInit();
            this.split.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
