namespace CMS.Presentation
{
    partial class RecycleBinForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Controls ────────────────────────────────────────────────────────────
        private System.Windows.Forms.ToolStrip            _toolbar;
        private System.Windows.Forms.ToolStripButton      _btnRestore;
        private System.Windows.Forms.ToolStripButton      _btnDelete;
        private System.Windows.Forms.ToolStripButton      _btnRefresh;
        private System.Windows.Forms.ToolStripButton      _btnEmptyBin;
        private System.Windows.Forms.ToolStripButton      _btnClose;
        private System.Windows.Forms.ToolStripSeparator   _sep1;
        private System.Windows.Forms.ToolStripSeparator   _sep2;
        private System.Windows.Forms.ToolStripLabel       _lblOrgUnit;
        private System.Windows.Forms.ToolStripComboBox    _cmbOrgUnit;
        private System.Windows.Forms.ToolStripLabel       _lblDeletedBy;
        private System.Windows.Forms.ToolStripComboBox    _cmbDeletedBy;
        private System.Windows.Forms.ToolStripLabel       _lblType;
        private System.Windows.Forms.ToolStripComboBox    _cmbType;
        private System.Windows.Forms.ListView             _listView;
        private System.Windows.Forms.StatusStrip          _statusBar;
        private System.Windows.Forms.ToolStripStatusLabel _lblStatus;
        private System.Windows.Forms.Panel                _emptyPanel;
        private System.Windows.Forms.Label                _iconLabel;
        private System.Windows.Forms.Label                _msgLabel;
        private System.Windows.Forms.Label                _subLabel;

        // ── Loading Overlay ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel _loadingPanel;
        private System.Windows.Forms.Label _loadingLabel;

        // ── Context Menu ─────────────────────────────────────────────────────────
        private System.Windows.Forms.ContextMenuStrip   _contextMenu;
        private System.Windows.Forms.ToolStripMenuItem  _ctxRestore;
        private System.Windows.Forms.ToolStripMenuItem  _ctxDelete;
        private System.Windows.Forms.ToolStripSeparator _ctxSep1;
        private System.Windows.Forms.ToolStripMenuItem  _ctxSelectAll;
        private System.Windows.Forms.ToolStripSeparator _ctxSep2;
        private System.Windows.Forms.ToolStripMenuItem  _ctxRefresh;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components   = new System.ComponentModel.Container();
            this._toolbar     = new System.Windows.Forms.ToolStrip();
            this._btnRestore  = new System.Windows.Forms.ToolStripButton();
            this._btnDelete   = new System.Windows.Forms.ToolStripButton();
            this._sep1        = new System.Windows.Forms.ToolStripSeparator();
            this._sep2        = new System.Windows.Forms.ToolStripSeparator();
            this._btnEmptyBin = new System.Windows.Forms.ToolStripButton();
            this._btnClose    = new System.Windows.Forms.ToolStripButton();
            this._btnRefresh  = new System.Windows.Forms.ToolStripButton();
            this._lblOrgUnit   = new System.Windows.Forms.ToolStripLabel();
            this._cmbOrgUnit   = new System.Windows.Forms.ToolStripComboBox();
            this._lblDeletedBy = new System.Windows.Forms.ToolStripLabel();
            this._cmbDeletedBy = new System.Windows.Forms.ToolStripComboBox();
            this._lblType      = new System.Windows.Forms.ToolStripLabel();
            this._cmbType      = new System.Windows.Forms.ToolStripComboBox();
            this._listView    = new System.Windows.Forms.ListView();
            this._emptyPanel  = new System.Windows.Forms.Panel();
            this._iconLabel   = new System.Windows.Forms.Label();
            this._msgLabel    = new System.Windows.Forms.Label();
            this._subLabel    = new System.Windows.Forms.Label();
            this._statusBar   = new System.Windows.Forms.StatusStrip();
            this._lblStatus   = new System.Windows.Forms.ToolStripStatusLabel();

            this._loadingPanel = new System.Windows.Forms.Panel();
            this._loadingLabel = new System.Windows.Forms.Label();
            this._contextMenu  = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._ctxRestore   = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxDelete    = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxSep1      = new System.Windows.Forms.ToolStripSeparator();
            this._ctxSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxSep2      = new System.Windows.Forms.ToolStripSeparator();
            this._ctxRefresh   = new System.Windows.Forms.ToolStripMenuItem();

            this._toolbar.SuspendLayout();
            this._emptyPanel.SuspendLayout();
            this._statusBar.SuspendLayout();
            this._contextMenu.SuspendLayout();
            this._loadingPanel.SuspendLayout();
            this.SuspendLayout();

            // ── Toolbar ───────────────────────────────────────────────────────
            this._toolbar.BackColor = System.Drawing.Color.FromArgb(240, 244, 250);
            this._toolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolbar.Padding   = new System.Windows.Forms.Padding(6, 2, 0, 2);
            this._toolbar.Font      = new System.Drawing.Font("Segoe UI", 10f);
            this._toolbar.Dock      = System.Windows.Forms.DockStyle.Top;
            this._toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this._btnRestore, this._btnDelete, this._sep1, this._btnEmptyBin,
                this._sep2, this._btnClose,
                this._btnRefresh, this._lblType, this._cmbType, this._lblDeletedBy, this._cmbDeletedBy, this._lblOrgUnit, this._cmbOrgUnit });

            // ── _btnRestore ───────────────────────────────────────────────────
            this._btnRestore.Text        = "♻️  คืนค่า (Restore)";
            this._btnRestore.ToolTipText = "คืน Folder ที่เลือกกลับไปยังตำแหน่งเดิม";
            this._btnRestore.Enabled     = false;
            this._btnRestore.Padding     = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnRestore.Click      += new System.EventHandler(this.BtnRestore_Click);

            // ── _btnDelete ────────────────────────────────────────────────────
            this._btnDelete.Text        = "🗑️  ลบถาวร";
            this._btnDelete.ToolTipText = "ลบ Folder ที่เลือกออกจากระบบถาวร (ไม่สามารถกู้คืนได้)";
            this._btnDelete.Enabled     = false;
            this._btnDelete.ForeColor   = System.Drawing.Color.FromArgb(180, 30, 30);
            this._btnDelete.Padding     = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnDelete.Click      += new System.EventHandler(this.BtnDelete_Click);

            // ── _btnEmptyBin ──────────────────────────────────────────────────
            this._btnEmptyBin.Text        = "🗑️  ล้างถังขยะทั้งหมด";
            this._btnEmptyBin.ToolTipText = "ลบ Folder ทั้งหมดในถังขยะออกถาวร";
            this._btnEmptyBin.ForeColor   = System.Drawing.Color.FromArgb(180, 30, 30);
            this._btnEmptyBin.Enabled     = false;
            this._btnEmptyBin.Padding     = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnEmptyBin.Click      += new System.EventHandler(this.BtnEmptyBin_Click);

            // ── _sep2 ─────────────────────────────────────────────────────────
            // (separator before Close button)

            // ── _btnClose ─────────────────────────────────────────────────────
            this._btnClose.Text        = "✕  ปิด";
            this._btnClose.ToolTipText = "ปิดหน้าต่างถังขยะ";
            this._btnClose.ForeColor   = System.Drawing.Color.FromArgb(90, 90, 90);
            this._btnClose.Padding     = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnClose.Click      += new System.EventHandler(this.BtnClose_Click);

            // ── _btnRefresh ───────────────────────────────────────────────────
            this._btnRefresh.Text        = "↻  Refresh";
            this._btnRefresh.ToolTipText = "โหลดข้อมูลใหม่";
            this._btnRefresh.Alignment   = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._btnRefresh.Padding     = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnRefresh.Click      += new System.EventHandler(this.BtnRefresh_Click);

            // ── _lblOrgUnit ───────────────────────────────────────────────────
            this._lblOrgUnit.Text      = "หน่วยงาน:";
            this._lblOrgUnit.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;

            // ── _cmbOrgUnit ───────────────────────────────────────────────────
            this._cmbOrgUnit.DropDownStyle          = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOrgUnit.Size                   = new System.Drawing.Size(170, 25);
            this._cmbOrgUnit.Alignment              = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._cmbOrgUnit.ToolTipText            = "กรองตามหน่วยงาน";
            this._cmbOrgUnit.SelectedIndexChanged  += new System.EventHandler(this.CmbOrgUnit_SelectedIndexChanged);

            // ── _lblDeletedBy ─────────────────────────────────────────────────
            this._lblDeletedBy.Text      = "ลบโดย:";
            this._lblDeletedBy.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;

            // ── _cmbDeletedBy ─────────────────────────────────────────────────
            this._cmbDeletedBy.DropDownStyle         = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbDeletedBy.Size                  = new System.Drawing.Size(140, 25);
            this._cmbDeletedBy.Alignment             = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._cmbDeletedBy.ToolTipText           = "กรองตามผู้ลบ";
            this._cmbDeletedBy.SelectedIndexChanged += new System.EventHandler(this.CmbDeletedBy_SelectedIndexChanged);

            // ── _lblType ──────────────────────────────────────────────────────
            this._lblType.Text      = "ประเภท:";
            this._lblType.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;

            // ── _cmbType ──────────────────────────────────────────────────────
            this._cmbType.DropDownStyle         = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbType.Size                  = new System.Drawing.Size(110, 25);
            this._cmbType.Alignment             = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._cmbType.ToolTipText           = "กรองตามประเภท (Folder / File)";
            this._cmbType.Items.AddRange(new object[] { "— ทั้งหมด —", "📁  Folder", "📄  File" });
            this._cmbType.SelectedIndex         = 0;
            this._cmbType.SelectedIndexChanged += new System.EventHandler(this.CmbType_SelectedIndexChanged);

            // ── _loadingPanel ─────────────────────────────────────────────────────
            this._loadingPanel.Dock      = System.Windows.Forms.DockStyle.Fill;
            this._loadingPanel.BackColor = System.Drawing.Color.White;
            this._loadingPanel.Visible   = true;
            this._loadingPanel.Controls.Add(this._loadingLabel);

            // ── _loadingLabel ─────────────────────────────────────────────────────
            this._loadingLabel.Text      = "⏳  กำลังโหลดข้อมูล...";
            this._loadingLabel.Font      = new System.Drawing.Font("Segoe UI", 14f);
            this._loadingLabel.ForeColor = System.Drawing.Color.FromArgb(100, 130, 170);
            this._loadingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._loadingLabel.Dock      = System.Windows.Forms.DockStyle.Fill;

            // ── _listView ─────────────────────────────────────────────────────
            this._listView.Dock             = System.Windows.Forms.DockStyle.Fill;
            this._listView.View             = System.Windows.Forms.View.Details;
            this._listView.FullRowSelect    = true;
            this._listView.GridLines        = false;
            this._listView.MultiSelect      = true;
            this._listView.CheckBoxes       = false;
            this._listView.Font             = new System.Drawing.Font("Segoe UI", 10f);
            this._listView.BorderStyle      = System.Windows.Forms.BorderStyle.None;
            this._listView.BackColor        = System.Drawing.Color.White;
            this._listView.OwnerDraw        = true;
            this._listView.Visible          = false;
            this._listView.ContextMenuStrip = this._contextMenu;
            this._listView.Columns.Add("ชื่อ Folder",  280);
            this._listView.Columns.Add("หน่วยงาน",      160);
            this._listView.Columns.Add("ตำแหน่งเดิม",   200);
            this._listView.Columns.Add("ลบโดย",          150);
            this._listView.Columns.Add("วันที่ลบ",      140);
            this._listView.Columns.Add("คงเหลือ (วัน)",  90);
            this._listView.Columns.Add("ระดับ",           70);
            this._listView.DrawColumnHeader     += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.ListView_DrawColumnHeader);
            this._listView.DrawItem             += new System.Windows.Forms.DrawListViewItemEventHandler(this.ListView_DrawItem);
            this._listView.DrawSubItem          += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.ListViewDrawSubItem);
            this._listView.SelectedIndexChanged += new System.EventHandler(this.ListView_SelectedIndexChanged);
            this._listView.DoubleClick          += new System.EventHandler(this.ListView_DoubleClick);
            this._listView.ColumnClick          += new System.Windows.Forms.ColumnClickEventHandler(this.ListView_ColumnClick);
            this._listView.MouseDown            += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDown);
            this._listView.ListViewItemSorter    = new ListViewColumnSorter();

            // ── Context Menu ──────────────────────────────────────────────────
            this._contextMenu.Font = new System.Drawing.Font("Segoe UI", 10f);
            this._contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this._ctxRestore, this._ctxDelete,
                this._ctxSep1,
                this._ctxSelectAll,
                this._ctxSep2,
                this._ctxRefresh });
            this._contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);

            // ── _ctxRestore ───────────────────────────────────────────────────
            this._ctxRestore.Text    = "♻️   คืนค่า (Restore)";
            this._ctxRestore.Font    = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this._ctxRestore.Enabled = false;
            this._ctxRestore.Click  += new System.EventHandler(this.CtxRestore_Click);

            // ── _ctxDelete ────────────────────────────────────────────────────
            this._ctxDelete.Text      = "🗑️   ลบถาวร";
            this._ctxDelete.ForeColor = System.Drawing.Color.FromArgb(180, 30, 30);
            this._ctxDelete.Enabled   = false;
            this._ctxDelete.Visible   = false;
            this._ctxDelete.Click    += new System.EventHandler(this.CtxDelete_Click);

            // ── _ctxSep1 ──────────────────────────────────────────────────────
            this._ctxSep1.Visible = false;

            // ── _ctxSelectAll ─────────────────────────────────────────────────
            this._ctxSelectAll.Text    = "☑️   เลือกทั้งหมด";
            this._ctxSelectAll.Enabled = false;
            this._ctxSelectAll.Click  += new System.EventHandler(this.CtxSelectAll_Click);

            // ── _ctxRefresh ───────────────────────────────────────────────────
            this._ctxRefresh.Text   = "↻   รีเฟรช";
            this._ctxRefresh.Click += new System.EventHandler(this.CtxRefresh_Click);

            // ── _emptyPanel ───────────────────────────────────────────────────
            this._emptyPanel.Dock      = System.Windows.Forms.DockStyle.Fill;
            this._emptyPanel.BackColor = System.Drawing.Color.White;
            this._emptyPanel.Visible   = false;
            this._emptyPanel.Controls.Add(this._subLabel);
            this._emptyPanel.Controls.Add(this._msgLabel);
            this._emptyPanel.Controls.Add(this._iconLabel);

            // ── _iconLabel ────────────────────────────────────────────────────
            this._iconLabel.Text      = "🗑️";
            this._iconLabel.Font      = new System.Drawing.Font("Segoe UI", 52f);
            this._iconLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._iconLabel.Dock      = System.Windows.Forms.DockStyle.Top;
            this._iconLabel.Height    = 130;

            // ── _msgLabel ─────────────────────────────────────────────────────
            this._msgLabel.Text      = "ถังขยะว่างเปล่า";
            this._msgLabel.Font      = new System.Drawing.Font("Segoe UI", 16f);
            this._msgLabel.ForeColor = System.Drawing.Color.Gray;
            this._msgLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._msgLabel.Dock      = System.Windows.Forms.DockStyle.Top;
            this._msgLabel.Height    = 42;

            // ── _subLabel ─────────────────────────────────────────────────────
            this._subLabel.Text      = "Folder ที่ถูกลบจะปรากฏที่นี่";
            this._subLabel.Font      = new System.Drawing.Font("Segoe UI", 10.5f);
            this._subLabel.ForeColor = System.Drawing.Color.LightGray;
            this._subLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._subLabel.Dock      = System.Windows.Forms.DockStyle.Top;
            this._subLabel.Height    = 28;

            // ── _statusBar ────────────────────────────────────────────────────
            this._statusBar.BackColor = System.Drawing.Color.FromArgb(240, 244, 250);
            this._statusBar.Font      = new System.Drawing.Font("Segoe UI", 9.5f);
            this._statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this._lblStatus });

            // ── _lblStatus ────────────────────────────────────────────────────
            this._lblStatus.Text      = "พร้อมใช้งาน";
            this._lblStatus.Spring    = true;
            this._lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── Form ──────────────────────────────────────────────────────────
            this.Text          = "🗑️ ถังขยะ — Recycle Bin";
            this.Size          = new System.Drawing.Size(1200, 650);
            this.MinimumSize   = new System.Drawing.Size(980, 510);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Font          = new System.Drawing.Font("Segoe UI", 10f);
            this.BackColor     = System.Drawing.Color.White;
            this.Name          = "RecycleBinForm";
            this.Shown        += new System.EventHandler(this.RecycleBinForm_Shown);
            this.Controls.Add(this._loadingPanel);
            this.Controls.Add(this._listView);
            this.Controls.Add(this._emptyPanel);
            this.Controls.Add(this._toolbar);
            this.Controls.Add(this._statusBar);

            this._toolbar.ResumeLayout(false);
            this._toolbar.PerformLayout();
            this._emptyPanel.ResumeLayout(false);
            this._loadingPanel.ResumeLayout(false);
            this._statusBar.ResumeLayout(false);
            this._statusBar.PerformLayout();
            this._contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
