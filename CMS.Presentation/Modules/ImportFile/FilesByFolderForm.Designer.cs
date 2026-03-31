namespace CMS.Presentation
{
    partial class FilesByFolderForm
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
            this.components = new System.ComponentModel.Container();

            this.toolStrip   = new System.Windows.Forms.ToolStrip();
            this.btnRefresh  = new System.Windows.Forms.ToolStripButton();

            this.splitter    = new System.Windows.Forms.SplitContainer();
            this.treeView    = new System.Windows.Forms.TreeView();
            this.listView    = new System.Windows.Forms.ListView();
            this.colIcon     = new System.Windows.Forms.ColumnHeader();
            this.colName     = new System.Windows.Forms.ColumnHeader();
            this.colSize     = new System.Windows.Forms.ColumnHeader();
            this.colExt      = new System.Windows.Forms.ColumnHeader();
            this.colDate     = new System.Windows.Forms.ColumnHeader();
            this.colScan     = new System.Windows.Forms.ColumnHeader();
            this.colVer      = new System.Windows.Forms.ColumnHeader();

            this.pnlBottom   = new System.Windows.Forms.Panel();
            this.lblInfo     = new System.Windows.Forms.Label();

            this.toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitter)).BeginInit();
            this.splitter.Panel1.SuspendLayout();
            this.splitter.Panel2.SuspendLayout();
            this.splitter.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();

            // ── toolStrip ────────────────────────────────────────────────────
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.btnRefresh });
            this.toolStrip.Dock      = System.Windows.Forms.DockStyle.Top;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.BackColor = System.Drawing.Color.FromArgb(243, 243, 243);
            this.toolStrip.Name      = "toolStrip";
            this.toolStrip.Size      = new System.Drawing.Size(1000, 28);

            this.btnRefresh.Name        = "btnRefresh";
            this.btnRefresh.Text        = "🔄 รีเฟรช";
            this.btnRefresh.ToolTipText = "โหลดข้อมูลใหม่จาก storage.FileDocument";
            this.btnRefresh.Click      += new System.EventHandler(this.btnRefresh_Click);

            // ── splitter ─────────────────────────────────────────────────────
            this.splitter.Dock        = System.Windows.Forms.DockStyle.Fill;
            this.splitter.Name        = "splitter";
            this.splitter.SplitterDistance = 240;
            this.splitter.BackColor   = System.Drawing.Color.FromArgb(0, 51, 102);

            // Panel1 — TreeView (Folder list)
            this.treeView.Dock        = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Name        = "treeView";
            this.treeView.BackColor   = System.Drawing.Color.FromArgb(245, 248, 252);
            this.treeView.Font        = new System.Drawing.Font("Segoe UI", 9f);
            this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            this.splitter.Panel1.Controls.Add(this.treeView);

            // Panel2 — ListView (Files)
            this.listView.Dock          = System.Windows.Forms.DockStyle.Fill;
            this.listView.Name          = "listView";
            this.listView.View          = System.Windows.Forms.View.Details;
            this.listView.FullRowSelect = true;
            this.listView.GridLines     = true;
            this.listView.HideSelection = false;
            this.listView.Font          = new System.Drawing.Font("Segoe UI", 9f);
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
            {
                this.colIcon, this.colName, this.colSize,
                this.colExt, this.colDate, this.colScan, this.colVer
            });
            this.colIcon.Text = "";           this.colIcon.Width = 30;
            this.colName.Text = "ชื่อไฟล์";  this.colName.Width = 360;
            this.colSize.Text = "ขนาด";      this.colSize.Width = 90;
            this.colExt.Text  = "ประเภท";    this.colExt.Width  = 70;
            this.colDate.Text = "วันที่อัพโหลด"; this.colDate.Width = 130;
            this.colScan.Text = "Virus Scan"; this.colScan.Width = 110;
            this.colVer.Text  = "Ver";        this.colVer.Width  = 40;
            this.splitter.Panel2.Controls.Add(this.listView);

            // ── pnlBottom ─────────────────────────────────────────────────────
            this.pnlBottom.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Height    = 28;
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.pnlBottom.Name      = "pnlBottom";

            this.lblInfo.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblInfo.Name      = "lblInfo";
            this.lblInfo.Text      = string.Empty;
            this.lblInfo.ForeColor = System.Drawing.Color.White;
            this.lblInfo.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblInfo.Padding   = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.pnlBottom.Controls.Add(this.lblInfo);

            // ── FilesByFolderForm ─────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.splitter);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.toolStrip);
            this.Name  = "FilesByFolderForm";
            this.Text  = "ไฟล์ทั้งหมด — แยกตาม Folder  [storage.FileDocument]";
            this.Load += new System.EventHandler(this.FilesByFolderForm_Load);

            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.splitter.Panel1.ResumeLayout(false);
            this.splitter.Panel2.ResumeLayout(false);
            this.splitter.ResumeLayout(false);
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ToolStrip          toolStrip;
        private System.Windows.Forms.ToolStripButton    btnRefresh;
        private System.Windows.Forms.SplitContainer     splitter;
        private System.Windows.Forms.TreeView           treeView;
        private System.Windows.Forms.ListView           listView;
        private System.Windows.Forms.ColumnHeader       colIcon;
        private System.Windows.Forms.ColumnHeader       colName;
        private System.Windows.Forms.ColumnHeader       colSize;
        private System.Windows.Forms.ColumnHeader       colExt;
        private System.Windows.Forms.ColumnHeader       colDate;
        private System.Windows.Forms.ColumnHeader       colScan;
        private System.Windows.Forms.ColumnHeader       colVer;
        private System.Windows.Forms.Panel              pnlBottom;
        private System.Windows.Forms.Label              lblInfo;
    }
}
