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
            this.components = new System.ComponentModel.Container();
            this._toolbar = new System.Windows.Forms.ToolStrip();
            this._btnRestore = new System.Windows.Forms.ToolStripButton();
            this._btnDelete = new System.Windows.Forms.ToolStripButton();
            this._sep1 = new System.Windows.Forms.ToolStripSeparator();
            this._btnEmptyBin = new System.Windows.Forms.ToolStripButton();
            this._sep2 = new System.Windows.Forms.ToolStripSeparator();
            this._btnClose = new System.Windows.Forms.ToolStripButton();
            this._btnRefresh = new System.Windows.Forms.ToolStripButton();
            this._lblType = new System.Windows.Forms.ToolStripLabel();
            this._cmbType = new System.Windows.Forms.ToolStripComboBox();
            this._lblDeletedBy = new System.Windows.Forms.ToolStripLabel();
            this._cmbDeletedBy = new System.Windows.Forms.ToolStripComboBox();
            this._lblOrgUnit = new System.Windows.Forms.ToolStripLabel();
            this._cmbOrgUnit = new System.Windows.Forms.ToolStripComboBox();
            this._listView = new System.Windows.Forms.ListView();
            this._contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._ctxRestore = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxDelete = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxSep1 = new System.Windows.Forms.ToolStripSeparator();
            this._ctxSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxSep2 = new System.Windows.Forms.ToolStripSeparator();
            this._ctxRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this._emptyPanel = new System.Windows.Forms.Panel();
            this._subLabel = new System.Windows.Forms.Label();
            this._msgLabel = new System.Windows.Forms.Label();
            this._iconLabel = new System.Windows.Forms.Label();
            this._statusBar = new System.Windows.Forms.StatusStrip();
            this._lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this._loadingPanel = new System.Windows.Forms.Panel();
            this._loadingLabel = new System.Windows.Forms.Label();
            this._toolbar.SuspendLayout();
            this._contextMenu.SuspendLayout();
            this._emptyPanel.SuspendLayout();
            this._statusBar.SuspendLayout();
            this._loadingPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _toolbar
            // 
            this._toolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this._toolbar.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._toolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._btnRestore,
            this._btnDelete,
            this._sep1,
            this._btnEmptyBin,
            this._sep2,
            this._btnClose,
            this._btnRefresh,
            this._lblType,
            this._cmbType,
            this._lblDeletedBy,
            this._cmbDeletedBy,
            this._lblOrgUnit,
            this._cmbOrgUnit});
            this._toolbar.Location = new System.Drawing.Point(0, 0);
            this._toolbar.Name = "_toolbar";
            this._toolbar.Padding = new System.Windows.Forms.Padding(6, 2, 0, 2);
            this._toolbar.Size = new System.Drawing.Size(1184, 30);
            this._toolbar.TabIndex = 4;
            // 
            // _btnRestore
            // 
            this._btnRestore.Enabled = false;
            this._btnRestore.Name = "_btnRestore";
            this._btnRestore.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnRestore.Size = new System.Drawing.Size(135, 23);
            this._btnRestore.Text = "♻️  คืนค่า (Restore)";
            this._btnRestore.ToolTipText = "คืน Folder ที่เลือกกลับไปยังตำแหน่งเดิม";
            this._btnRestore.Click += new System.EventHandler(this.BtnRestore_Click);
            // 
            // _btnDelete
            // 
            this._btnDelete.Enabled = false;
            this._btnDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this._btnDelete.Name = "_btnDelete";
            this._btnDelete.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnDelete.Size = new System.Drawing.Size(90, 23);
            this._btnDelete.Text = "🗑️  ลบถาวร";
            this._btnDelete.ToolTipText = "ลบ Folder ที่เลือกออกจากระบบถาวร (ไม่สามารถกู้คืนได้)";
            this._btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // _sep1
            // 
            this._sep1.Name = "_sep1";
            this._sep1.Size = new System.Drawing.Size(6, 26);
            // 
            // _btnEmptyBin
            // 
            this._btnEmptyBin.Enabled = false;
            this._btnEmptyBin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this._btnEmptyBin.Name = "_btnEmptyBin";
            this._btnEmptyBin.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnEmptyBin.Size = new System.Drawing.Size(137, 23);
            this._btnEmptyBin.Text = "🗑️  ล้างถังขยะทั้งหมด";
            this._btnEmptyBin.ToolTipText = "ลบ Folder ทั้งหมดในถังขยะออกถาวร";
            this._btnEmptyBin.Click += new System.EventHandler(this.BtnEmptyBin_Click);
            // 
            // _sep2
            // 
            this._sep2.Name = "_sep2";
            this._sep2.Size = new System.Drawing.Size(6, 26);
            // 
            // _btnClose
            // 
            this._btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this._btnClose.Name = "_btnClose";
            this._btnClose.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnClose.Size = new System.Drawing.Size(56, 23);
            this._btnClose.Text = "✕  ปิด";
            this._btnClose.ToolTipText = "ปิดหน้าต่างถังขยะ";
            this._btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // _btnRefresh
            // 
            this._btnRefresh.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._btnRefresh.Name = "_btnRefresh";
            this._btnRefresh.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnRefresh.Size = new System.Drawing.Size(86, 23);
            this._btnRefresh.Text = "↻  Refresh";
            this._btnRefresh.ToolTipText = "โหลดข้อมูลใหม่";
            this._btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // _lblType
            // 
            this._lblType.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._lblType.Name = "_lblType";
            this._lblType.Size = new System.Drawing.Size(51, 23);
            this._lblType.Text = "ประเภท:";
            // 
            // _cmbType
            // 
            this._cmbType.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbType.Items.AddRange(new object[] {
            "— ทั้งหมด —",
            "📁  Folder",
            "📄  File"});
            this._cmbType.Name = "_cmbType";
            this._cmbType.Size = new System.Drawing.Size(110, 26);
            this._cmbType.Text = "— ทั้งหมด —";
            this._cmbType.ToolTipText = "กรองตามประเภท (Folder / File)";
            this._cmbType.SelectedIndexChanged += new System.EventHandler(this.CmbType_SelectedIndexChanged);
            // 
            // _lblDeletedBy
            // 
            this._lblDeletedBy.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._lblDeletedBy.Name = "_lblDeletedBy";
            this._lblDeletedBy.Size = new System.Drawing.Size(47, 23);
            this._lblDeletedBy.Text = "ลบโดย:";
            // 
            // _cmbDeletedBy
            // 
            this._cmbDeletedBy.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._cmbDeletedBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbDeletedBy.Name = "_cmbDeletedBy";
            this._cmbDeletedBy.Size = new System.Drawing.Size(140, 26);
            this._cmbDeletedBy.ToolTipText = "กรองตามผู้ลบ";
            this._cmbDeletedBy.SelectedIndexChanged += new System.EventHandler(this.CmbDeletedBy_SelectedIndexChanged);
            // 
            // _lblOrgUnit
            // 
            this._lblOrgUnit.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._lblOrgUnit.Name = "_lblOrgUnit";
            this._lblOrgUnit.Size = new System.Drawing.Size(55, 23);
            this._lblOrgUnit.Text = "ชื่อผู้ลบ :";
            // 
            // _cmbOrgUnit
            // 
            this._cmbOrgUnit.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._cmbOrgUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOrgUnit.Name = "_cmbOrgUnit";
            this._cmbOrgUnit.Size = new System.Drawing.Size(170, 26);
            this._cmbOrgUnit.ToolTipText = "กรองตามหน่วยงาน";
            this._cmbOrgUnit.SelectedIndexChanged += new System.EventHandler(this.CmbOrgUnit_SelectedIndexChanged);
            // 
            // _listView
            // 
            this._listView.BackColor = System.Drawing.Color.White;
            this._listView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._listView.ContextMenuStrip = this._contextMenu;
            this._listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listView.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._listView.FullRowSelect = true;
            this._listView.HideSelection = false;
            this._listView.Location = new System.Drawing.Point(0, 30);
            this._listView.Name = "_listView";
            this._listView.OwnerDraw = true;
            this._listView.Size = new System.Drawing.Size(1184, 559);
            this._listView.TabIndex = 2;
            this._listView.UseCompatibleStateImageBehavior = false;
            this._listView.View = System.Windows.Forms.View.Details;
            this._listView.Visible = false;
            this._listView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListView_ColumnClick);
            this._listView.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.ListView_DrawColumnHeader);
            this._listView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.ListView_DrawItem);
            this._listView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.ListViewDrawSubItem);
            this._listView.SelectedIndexChanged += new System.EventHandler(this.ListView_SelectedIndexChanged);
            this._listView.DoubleClick += new System.EventHandler(this.ListView_DoubleClick);
            this._listView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDown);
            // 
            // _contextMenu
            // 
            this._contextMenu.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._ctxRestore,
            this._ctxDelete,
            this._ctxSep1,
            this._ctxSelectAll,
            this._ctxSep2,
            this._ctxRefresh});
            this._contextMenu.Name = "_contextMenu";
            this._contextMenu.Size = new System.Drawing.Size(210, 112);
            this._contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // _ctxRestore
            // 
            this._ctxRestore.Enabled = false;
            this._ctxRestore.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._ctxRestore.Name = "_ctxRestore";
            this._ctxRestore.Size = new System.Drawing.Size(209, 24);
            this._ctxRestore.Text = "♻️   คืนค่า (Restore)";
            this._ctxRestore.Click += new System.EventHandler(this.CtxRestore_Click);
            // 
            // _ctxDelete
            // 
            this._ctxDelete.Enabled = false;
            this._ctxDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this._ctxDelete.Name = "_ctxDelete";
            this._ctxDelete.Size = new System.Drawing.Size(209, 24);
            this._ctxDelete.Text = "🗑️   ลบถาวร";
            this._ctxDelete.Visible = false;
            this._ctxDelete.Click += new System.EventHandler(this.CtxDelete_Click);
            // 
            // _ctxSep1
            // 
            this._ctxSep1.Name = "_ctxSep1";
            this._ctxSep1.Size = new System.Drawing.Size(206, 6);
            this._ctxSep1.Visible = false;
            // 
            // _ctxSelectAll
            // 
            this._ctxSelectAll.Enabled = false;
            this._ctxSelectAll.Name = "_ctxSelectAll";
            this._ctxSelectAll.Size = new System.Drawing.Size(209, 24);
            this._ctxSelectAll.Text = "☑️   เลือกทั้งหมด";
            this._ctxSelectAll.Click += new System.EventHandler(this.CtxSelectAll_Click);
            // 
            // _ctxSep2
            // 
            this._ctxSep2.Name = "_ctxSep2";
            this._ctxSep2.Size = new System.Drawing.Size(206, 6);
            // 
            // _ctxRefresh
            // 
            this._ctxRefresh.Name = "_ctxRefresh";
            this._ctxRefresh.Size = new System.Drawing.Size(209, 24);
            this._ctxRefresh.Text = "↻   รีเฟรช";
            this._ctxRefresh.Click += new System.EventHandler(this.CtxRefresh_Click);
            // 
            // _emptyPanel
            // 
            this._emptyPanel.BackColor = System.Drawing.Color.White;
            this._emptyPanel.Controls.Add(this._subLabel);
            this._emptyPanel.Controls.Add(this._msgLabel);
            this._emptyPanel.Controls.Add(this._iconLabel);
            this._emptyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._emptyPanel.Location = new System.Drawing.Point(0, 30);
            this._emptyPanel.Name = "_emptyPanel";
            this._emptyPanel.Size = new System.Drawing.Size(1184, 559);
            this._emptyPanel.TabIndex = 3;
            this._emptyPanel.Visible = false;
            // 
            // _subLabel
            // 
            this._subLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._subLabel.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this._subLabel.ForeColor = System.Drawing.Color.LightGray;
            this._subLabel.Location = new System.Drawing.Point(0, 172);
            this._subLabel.Name = "_subLabel";
            this._subLabel.Size = new System.Drawing.Size(1184, 28);
            this._subLabel.TabIndex = 0;
            this._subLabel.Text = "Folder ที่ถูกลบจะปรากฏที่นี่";
            this._subLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _msgLabel
            // 
            this._msgLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._msgLabel.Font = new System.Drawing.Font("Segoe UI", 16F);
            this._msgLabel.ForeColor = System.Drawing.Color.Gray;
            this._msgLabel.Location = new System.Drawing.Point(0, 130);
            this._msgLabel.Name = "_msgLabel";
            this._msgLabel.Size = new System.Drawing.Size(1184, 42);
            this._msgLabel.TabIndex = 1;
            this._msgLabel.Text = "ถังขยะว่างเปล่า";
            this._msgLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _iconLabel
            // 
            this._iconLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._iconLabel.Font = new System.Drawing.Font("Segoe UI", 52F);
            this._iconLabel.Location = new System.Drawing.Point(0, 0);
            this._iconLabel.Name = "_iconLabel";
            this._iconLabel.Size = new System.Drawing.Size(1184, 130);
            this._iconLabel.TabIndex = 2;
            this._iconLabel.Text = "🗑️";
            this._iconLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _statusBar
            // 
            this._statusBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this._statusBar.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this._statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._lblStatus});
            this._statusBar.Location = new System.Drawing.Point(0, 589);
            this._statusBar.Name = "_statusBar";
            this._statusBar.Size = new System.Drawing.Size(1184, 22);
            this._statusBar.TabIndex = 5;
            // 
            // _lblStatus
            // 
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(1169, 17);
            this._lblStatus.Spring = true;
            this._lblStatus.Text = "พร้อมใช้งาน";
            this._lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _loadingPanel
            // 
            this._loadingPanel.BackColor = System.Drawing.Color.White;
            this._loadingPanel.Controls.Add(this._loadingLabel);
            this._loadingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._loadingPanel.Location = new System.Drawing.Point(0, 30);
            this._loadingPanel.Name = "_loadingPanel";
            this._loadingPanel.Size = new System.Drawing.Size(1184, 559);
            this._loadingPanel.TabIndex = 1;
            // 
            // _loadingLabel
            // 
            this._loadingLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._loadingLabel.Font = new System.Drawing.Font("Segoe UI", 14F);
            this._loadingLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(130)))), ((int)(((byte)(170)))));
            this._loadingLabel.Location = new System.Drawing.Point(0, 0);
            this._loadingLabel.Name = "_loadingLabel";
            this._loadingLabel.Size = new System.Drawing.Size(1184, 559);
            this._loadingLabel.TabIndex = 0;
            this._loadingLabel.Text = "⏳  กำลังโหลดข้อมูล...";
            this._loadingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RecycleBinForm
            // 
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1184, 611);
            this.Controls.Add(this._loadingPanel);
            this.Controls.Add(this._listView);
            this.Controls.Add(this._emptyPanel);
            this.Controls.Add(this._toolbar);
            this.Controls.Add(this._statusBar);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(980, 510);
            this.Name = "RecycleBinForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "🗑️ ถังขยะ — Recycle Bin";
            this.Shown += new System.EventHandler(this.RecycleBinForm_Shown);
            this._toolbar.ResumeLayout(false);
            this._toolbar.PerformLayout();
            this._contextMenu.ResumeLayout(false);
            this._emptyPanel.ResumeLayout(false);
            this._statusBar.ResumeLayout(false);
            this._statusBar.PerformLayout();
            this._loadingPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
