namespace CMS.Presentation
{
    partial class FolderPickerDialog
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
            this._pnlHeader      = new System.Windows.Forms.Panel();
            this._lblHeader      = new System.Windows.Forms.Label();
            this._pnlSourceInfo  = new System.Windows.Forms.Panel();
            this._lblSourceInfo  = new System.Windows.Forms.Label();
            this._split          = new System.Windows.Forms.SplitContainer();
            this._lblTreeHeader  = new System.Windows.Forms.Label();
            this._txtSearch      = new System.Windows.Forms.TextBox();
            this._pnlTreeTools   = new System.Windows.Forms.Panel();
            this._btnExpandAll   = new System.Windows.Forms.Button();
            this._btnCollapseAll = new System.Windows.Forms.Button();
            this._btnExpandLevel1 = new System.Windows.Forms.Button();
            this._treeView       = new System.Windows.Forms.TreeView();
            this._lblListHeader  = new System.Windows.Forms.Label();
            this._listView       = new System.Windows.Forms.ListView();
            this._colName        = new System.Windows.Forms.ColumnHeader();
            this._colPath        = new System.Windows.Forms.ColumnHeader();
            this._colLevel       = new System.Windows.Forms.ColumnHeader();
            this._pnlSummary     = new System.Windows.Forms.Panel();
            this._sepLine        = new System.Windows.Forms.Panel();
            this._lblSummaryFolder = new System.Windows.Forms.Label();
            this._lblSummaryDept   = new System.Windows.Forms.Label();
            this._lblSummaryLevel  = new System.Windows.Forms.Label();
            this._lblSummaryPath   = new System.Windows.Forms.Label();
            this._pnlFooter      = new System.Windows.Forms.Panel();
            this._btnOk          = new System.Windows.Forms.Button();
            this._btnCancel      = new System.Windows.Forms.Button();

            this._pnlHeader.SuspendLayout();
            this._pnlSourceInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._split)).BeginInit();
            this._split.Panel1.SuspendLayout();
            this._split.Panel2.SuspendLayout();
            this._split.SuspendLayout();
            this._pnlTreeTools.SuspendLayout();
            this._pnlSummary.SuspendLayout();
            this._pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // ── _pnlHeader ───────────────────────────────────────────────────
            this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this._pnlHeader.Controls.Add(this._lblHeader);
            this._pnlHeader.Dock     = System.Windows.Forms.DockStyle.Top;
            this._pnlHeader.Height   = 44;
            this._pnlHeader.Name     = "_pnlHeader";
            this._pnlHeader.TabIndex = 0;

            // ── _lblHeader ───────────────────────────────────────────────────
            this._lblHeader.Dock      = System.Windows.Forms.DockStyle.Fill;
            this._lblHeader.Font      = new System.Drawing.Font("Segoe UI", 10.5f, System.Drawing.FontStyle.Bold);
            this._lblHeader.ForeColor = System.Drawing.Color.White;
            this._lblHeader.Name      = "_lblHeader";
            this._lblHeader.Padding   = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this._lblHeader.Text      = "📁  เลือก Folder ปลายทาง";
            this._lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblHeader.TabIndex  = 0;

            // ── _pnlSourceInfo ───────────────────────────────────────────────
            this._pnlSourceInfo.BackColor = System.Drawing.Color.FromArgb(219, 234, 254);
            this._pnlSourceInfo.Controls.Add(this._lblSourceInfo);
            this._pnlSourceInfo.Dock     = System.Windows.Forms.DockStyle.Top;
            this._pnlSourceInfo.Height   = 26;
            this._pnlSourceInfo.Name     = "_pnlSourceInfo";
            this._pnlSourceInfo.TabIndex = 1;

            // ── _lblSourceInfo ───────────────────────────────────────────────
            this._lblSourceInfo.Dock      = System.Windows.Forms.DockStyle.Fill;
            this._lblSourceInfo.Font      = new System.Drawing.Font("Segoe UI", 9f);
            this._lblSourceInfo.ForeColor = System.Drawing.Color.FromArgb(30, 64, 120);
            this._lblSourceInfo.Name      = "_lblSourceInfo";
            this._lblSourceInfo.Padding   = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this._lblSourceInfo.Text      = "ย้ายจาก: —";
            this._lblSourceInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblSourceInfo.TabIndex  = 0;

            // ── _split ───────────────────────────────────────────────────────
            this._split.BorderStyle     = System.Windows.Forms.BorderStyle.None;
            this._split.Dock            = System.Windows.Forms.DockStyle.Fill;
            this._split.Name            = "_split";
            this._split.Panel2Collapsed = true;
            this._split.TabIndex        = 2;

            // Panel1: TreeHeader + Search + Toolbar + TreeView
            this._split.Panel1.Controls.Add(this._treeView);
            this._split.Panel1.Controls.Add(this._pnlTreeTools);
            this._split.Panel1.Controls.Add(this._txtSearch);
            this._split.Panel1.Controls.Add(this._lblTreeHeader);

            // Panel2: ListView (hidden)
            this._split.Panel2.Controls.Add(this._listView);
            this._split.Panel2.Controls.Add(this._lblListHeader);

            // ── _lblTreeHeader ───────────────────────────────────────────────
            this._lblTreeHeader.BackColor = System.Drawing.Color.FromArgb(240, 244, 252);
            this._lblTreeHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this._lblTreeHeader.Font      = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this._lblTreeHeader.ForeColor = System.Drawing.Color.FromArgb(60, 80, 120);
            this._lblTreeHeader.Height    = 24;
            this._lblTreeHeader.Name      = "_lblTreeHeader";
            this._lblTreeHeader.Padding   = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this._lblTreeHeader.Text      = "🗂 โครงสร้างหน่วยงาน";
            this._lblTreeHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblTreeHeader.TabIndex  = 0;

            // ── _txtSearch ───────────────────────────────────────────────────
            this._txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtSearch.Dock        = System.Windows.Forms.DockStyle.Top;
            this._txtSearch.Font        = new System.Drawing.Font("Segoe UI", 9f);
            this._txtSearch.ForeColor   = System.Drawing.Color.FromArgb(80, 80, 80);
            this._txtSearch.Name        = "_txtSearch";
            this._txtSearch.Text        = "🔍 พิมพ์ค้นหา folder...";
            this._txtSearch.TabIndex    = 1;
            this._txtSearch.TextChanged += new System.EventHandler(this.TxtSearch_TextChanged);

            // ── _pnlTreeTools ────────────────────────────────────────────────
            this._pnlTreeTools.BackColor = System.Drawing.Color.FromArgb(235, 240, 250);
            this._pnlTreeTools.Controls.Add(this._btnExpandLevel1);
            this._pnlTreeTools.Controls.Add(this._btnCollapseAll);
            this._pnlTreeTools.Controls.Add(this._btnExpandAll);
            this._pnlTreeTools.Dock      = System.Windows.Forms.DockStyle.Top;
            this._pnlTreeTools.Height    = 30;
            this._pnlTreeTools.Name      = "_pnlTreeTools";
            this._pnlTreeTools.Padding   = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._pnlTreeTools.TabIndex  = 10;

            // ── _btnExpandAll ────────────────────────────────────────────────
            this._btnExpandAll.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this._btnExpandAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnExpandAll.Font      = new System.Drawing.Font("Segoe UI", 8f);
            this._btnExpandAll.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this._btnExpandAll.Location  = new System.Drawing.Point(4, 3);
            this._btnExpandAll.Name      = "_btnExpandAll";
            this._btnExpandAll.Size      = new System.Drawing.Size(90, 24);
            this._btnExpandAll.TabIndex  = 0;
            this._btnExpandAll.Text      = "+ ขยายทั้งหมด";
            this._btnExpandAll.UseVisualStyleBackColor = false;
            this._btnExpandAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 195, 220);
            this._btnExpandAll.Click += new System.EventHandler(this.BtnExpandAll_Click);

            // ── _btnCollapseAll ──────────────────────────────────────────────
            this._btnCollapseAll.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this._btnCollapseAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCollapseAll.Font      = new System.Drawing.Font("Segoe UI", 8f);
            this._btnCollapseAll.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this._btnCollapseAll.Location  = new System.Drawing.Point(98, 3);
            this._btnCollapseAll.Name      = "_btnCollapseAll";
            this._btnCollapseAll.Size      = new System.Drawing.Size(90, 24);
            this._btnCollapseAll.TabIndex  = 1;
            this._btnCollapseAll.Text      = "− ย่อทั้งหมด";
            this._btnCollapseAll.UseVisualStyleBackColor = false;
            this._btnCollapseAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 195, 220);
            this._btnCollapseAll.Click += new System.EventHandler(this.BtnCollapseAll_Click);

            // ── _btnExpandLevel1 ─────────────────────────────────────────────
            this._btnExpandLevel1.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this._btnExpandLevel1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnExpandLevel1.Font      = new System.Drawing.Font("Segoe UI", 8f);
            this._btnExpandLevel1.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this._btnExpandLevel1.Location  = new System.Drawing.Point(192, 3);
            this._btnExpandLevel1.Name      = "_btnExpandLevel1";
            this._btnExpandLevel1.Size      = new System.Drawing.Size(90, 24);
            this._btnExpandLevel1.TabIndex  = 2;
            this._btnExpandLevel1.Text      = "⊕ ระดับ 1";
            this._btnExpandLevel1.UseVisualStyleBackColor = false;
            this._btnExpandLevel1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 195, 220);
            this._btnExpandLevel1.Click += new System.EventHandler(this.BtnExpandLevel1_Click);

            // ── _treeView ────────────────────────────────────────────────────
            this._treeView.BorderStyle   = System.Windows.Forms.BorderStyle.None;
            this._treeView.Dock          = System.Windows.Forms.DockStyle.Fill;
            this._treeView.Font          = new System.Drawing.Font("Segoe UI", 9f);
            this._treeView.FullRowSelect = true;
            this._treeView.HideSelection = false;
            this._treeView.Indent        = 20;
            this._treeView.ItemHeight    = 22;
            this._treeView.Name          = "_treeView";
            this._treeView.ShowLines     = true;
            this._treeView.ShowPlusMinus = true;
            this._treeView.TabIndex      = 2;
            this._treeView.AfterSelect  += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_AfterSelect);

            // ── _lblListHeader ───────────────────────────────────────────────
            this._lblListHeader.BackColor = System.Drawing.Color.FromArgb(240, 244, 252);
            this._lblListHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this._lblListHeader.Font      = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this._lblListHeader.ForeColor = System.Drawing.Color.FromArgb(60, 80, 120);
            this._lblListHeader.Height    = 24;
            this._lblListHeader.Name      = "_lblListHeader";
            this._lblListHeader.Padding   = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this._lblListHeader.Text      = "📋 รายการ Folder ทั้งหมด";
            this._lblListHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblListHeader.TabIndex  = 0;

            // ── _listView ────────────────────────────────────────────────────
            this._listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this._colName, this._colPath, this._colLevel });
            this._listView.Dock          = System.Windows.Forms.DockStyle.Fill;
            this._listView.Font          = new System.Drawing.Font("Segoe UI", 9f);
            this._listView.FullRowSelect = true;
            this._listView.GridLines     = true;
            this._listView.HideSelection = false;
            this._listView.MultiSelect   = false;
            this._listView.Name          = "_listView";
            this._listView.TabIndex      = 1;
            this._listView.View          = System.Windows.Forms.View.Details;
            this._listView.SelectedIndexChanged += new System.EventHandler(this.ListView_SelectedIndexChanged);

            this._colName.Text  = "ชื่อ Folder";
            this._colName.Width = 160;
            this._colPath.Text  = "Path";
            this._colPath.Width = 260;
            this._colLevel.Text = "ระดับ";
            this._colLevel.Width = 60;

            // ── _pnlSummary ──────────────────────────────────────────────────
            this._pnlSummary.BackColor = System.Drawing.Color.FromArgb(248, 250, 255);
            this._pnlSummary.Controls.Add(this._lblSummaryPath);
            this._pnlSummary.Controls.Add(this._lblSummaryLevel);
            this._pnlSummary.Controls.Add(this._lblSummaryDept);
            this._pnlSummary.Controls.Add(this._lblSummaryFolder);
            this._pnlSummary.Controls.Add(this._sepLine);
            this._pnlSummary.Dock     = System.Windows.Forms.DockStyle.Bottom;
            this._pnlSummary.Height   = 80;
            this._pnlSummary.Name     = "_pnlSummary";
            this._pnlSummary.Padding  = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this._pnlSummary.TabIndex = 3;

            // ── _sepLine ─────────────────────────────────────────────────────
            this._sepLine.BackColor = System.Drawing.Color.FromArgb(200, 212, 235);
            this._sepLine.Dock      = System.Windows.Forms.DockStyle.Top;
            this._sepLine.Height    = 1;
            this._sepLine.Name      = "_sepLine";
            this._sepLine.TabIndex  = 0;

            // ── _lblSummaryFolder ────────────────────────────────────────────
            this._lblSummaryFolder.Dock      = System.Windows.Forms.DockStyle.Top;
            this._lblSummaryFolder.Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this._lblSummaryFolder.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this._lblSummaryFolder.Height    = 22;
            this._lblSummaryFolder.Name      = "_lblSummaryFolder";
            this._lblSummaryFolder.Text      = "กรุณาเลือก Folder ปลายทาง";
            this._lblSummaryFolder.TabIndex  = 1;

            // ── _lblSummaryDept ──────────────────────────────────────────────
            this._lblSummaryDept.Dock      = System.Windows.Forms.DockStyle.Top;
            this._lblSummaryDept.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            this._lblSummaryDept.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70);
            this._lblSummaryDept.Height    = 17;
            this._lblSummaryDept.Name      = "_lblSummaryDept";
            this._lblSummaryDept.TabIndex  = 2;

            // ── _lblSummaryLevel ─────────────────────────────────────────────
            this._lblSummaryLevel.Dock      = System.Windows.Forms.DockStyle.Top;
            this._lblSummaryLevel.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            this._lblSummaryLevel.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70);
            this._lblSummaryLevel.Height    = 17;
            this._lblSummaryLevel.Name      = "_lblSummaryLevel";
            this._lblSummaryLevel.TabIndex  = 3;

            // ── _lblSummaryPath ──────────────────────────────────────────────
            this._lblSummaryPath.Dock      = System.Windows.Forms.DockStyle.Top;
            this._lblSummaryPath.Font      = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Italic);
            this._lblSummaryPath.ForeColor = System.Drawing.Color.FromArgb(130, 130, 130);
            this._lblSummaryPath.Height    = 15;
            this._lblSummaryPath.Name      = "_lblSummaryPath";
            this._lblSummaryPath.TabIndex  = 4;

            // ── _pnlFooter ───────────────────────────────────────────────────
            this._pnlFooter.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this._pnlFooter.Controls.Add(this._btnOk);
            this._pnlFooter.Controls.Add(this._btnCancel);
            this._pnlFooter.Dock     = System.Windows.Forms.DockStyle.Bottom;
            this._pnlFooter.Height   = 50;
            this._pnlFooter.Name     = "_pnlFooter";
            this._pnlFooter.TabIndex = 4;

            // ── _btnOk ───────────────────────────────────────────────────────
            this._btnOk.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this._btnOk.Enabled   = false;
            this._btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOk.Font      = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this._btnOk.ForeColor = System.Drawing.Color.White;
            this._btnOk.Location  = new System.Drawing.Point(232, 8);
            this._btnOk.Name      = "_btnOk";
            this._btnOk.Size      = new System.Drawing.Size(130, 34);
            this._btnOk.TabIndex  = 0;
            this._btnOk.Text      = "✔  ย้ายมาที่นี่";
            this._btnOk.UseVisualStyleBackColor = false;
            this._btnOk.FlatAppearance.BorderSize = 0;
            this._btnOk.Click += new System.EventHandler(this.BtnOk_Click);

            // ── _btnCancel ───────────────────────────────────────────────────
            this._btnCancel.BackColor = System.Drawing.Color.White;
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Font      = new System.Drawing.Font("Segoe UI", 9.5f);
            this._btnCancel.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this._btnCancel.Location  = new System.Drawing.Point(370, 8);
            this._btnCancel.Name      = "_btnCancel";
            this._btnCancel.Size      = new System.Drawing.Size(90, 34);
            this._btnCancel.TabIndex  = 1;
            this._btnCancel.Text      = "ยกเลิก";
            this._btnCancel.UseVisualStyleBackColor = false;
            this._btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
            this._btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);

            // ── Form ─────────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton        = this._btnCancel;
            this.ClientSize          = new System.Drawing.Size(480, 600);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.Name                = "FolderPickerDialog";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "เลือก Folder ปลายทาง";

            this.Controls.Add(this._split);
            this.Controls.Add(this._pnlSummary);
            this.Controls.Add(this._pnlFooter);
            this.Controls.Add(this._pnlSourceInfo);
            this.Controls.Add(this._pnlHeader);

            this._pnlHeader.ResumeLayout(false);
            this._pnlSourceInfo.ResumeLayout(false);
            this._split.Panel1.ResumeLayout(false);
            this._split.Panel1.PerformLayout();
            this._split.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._split)).EndInit();
            this._split.ResumeLayout(false);
            this._pnlTreeTools.ResumeLayout(false);
            this._pnlSummary.ResumeLayout(false);
            this._pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        // ── Controls ─────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel          _pnlHeader;
        private System.Windows.Forms.Label          _lblHeader;
        private System.Windows.Forms.Panel          _pnlSourceInfo;
        private System.Windows.Forms.Label          _lblSourceInfo;
        private System.Windows.Forms.SplitContainer _split;
        private System.Windows.Forms.Label          _lblTreeHeader;
        private System.Windows.Forms.TextBox        _txtSearch;
        private System.Windows.Forms.TreeView       _treeView;
        private System.Windows.Forms.Label          _lblListHeader;
        private System.Windows.Forms.ListView       _listView;
        private System.Windows.Forms.ColumnHeader   _colName;
        private System.Windows.Forms.ColumnHeader   _colPath;
        private System.Windows.Forms.ColumnHeader   _colLevel;
        private System.Windows.Forms.Panel          _pnlSummary;
        private System.Windows.Forms.Panel          _sepLine;
        private System.Windows.Forms.Label          _lblSummaryFolder;
        private System.Windows.Forms.Label          _lblSummaryDept;
        private System.Windows.Forms.Label          _lblSummaryLevel;
        private System.Windows.Forms.Label          _lblSummaryPath;
        private System.Windows.Forms.Panel          _pnlTreeTools;
        private System.Windows.Forms.Button         _btnExpandAll;
        private System.Windows.Forms.Button         _btnCollapseAll;
        private System.Windows.Forms.Button         _btnExpandLevel1;
        private System.Windows.Forms.Panel          _pnlFooter;
        private System.Windows.Forms.Button         _btnOk;
        private System.Windows.Forms.Button         _btnCancel;
    }
}
