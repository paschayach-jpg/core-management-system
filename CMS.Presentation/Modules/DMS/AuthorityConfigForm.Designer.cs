namespace CMS.Presentation
{
    partial class AuthorityConfigForm
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
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.split = new System.Windows.Forms.SplitContainer();
            this.tvOrg = new System.Windows.Forms.TreeView();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.splitDetail = new System.Windows.Forms.SplitContainer();
            this.dgv = new System.Windows.Forms.DataGridView();
            this.pnlLevelTabs = new System.Windows.Forms.Panel();
            this.rdoAll = new System.Windows.Forms.RadioButton();
            this.rdoWithLevel = new System.Windows.Forms.RadioButton();
            this.rdoNoLevel = new System.Windows.Forms.RadioButton();
            this.pnlGridTop = new System.Windows.Forms.Panel();
            this.lblFilter = new System.Windows.Forms.Label();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.pnlDetail = new System.Windows.Forms.Panel();
            this.pnlDetailBody = new System.Windows.Forms.Panel();
            this.lblDetailTitle = new System.Windows.Forms.Label();
            this.lblDetailSub = new System.Windows.Forms.Label();
            this.tlpMatrix = new System.Windows.Forms.TableLayoutPanel();
            this.lblMxCorner = new System.Windows.Forms.Label();
            this.lblMxView = new System.Windows.Forms.Label();
            this.lblMxAdd = new System.Windows.Forms.Label();
            this.lblMxEdit = new System.Windows.Forms.Label();
            this.lblMxApprove = new System.Windows.Forms.Label();
            this.lblMxDownload = new System.Windows.Forms.Label();
            this.lblMxHC = new System.Windows.Forms.Label();
            this.lblMxSC = new System.Windows.Forms.Label();
            this.lblMxNC = new System.Windows.Forms.Label();
            this.lblMxPC = new System.Windows.Forms.Label();
            this.chkHC_V = new System.Windows.Forms.CheckBox();
            this.chkHC_A = new System.Windows.Forms.CheckBox();
            this.chkHC_E = new System.Windows.Forms.CheckBox();
            this.chkHC_Ap = new System.Windows.Forms.CheckBox();
            this.chkHC_Dl = new System.Windows.Forms.CheckBox();
            this.chkSC_V = new System.Windows.Forms.CheckBox();
            this.chkSC_A = new System.Windows.Forms.CheckBox();
            this.chkSC_E = new System.Windows.Forms.CheckBox();
            this.chkSC_Ap = new System.Windows.Forms.CheckBox();
            this.chkSC_Dl = new System.Windows.Forms.CheckBox();
            this.chkNC_V = new System.Windows.Forms.CheckBox();
            this.chkNC_A = new System.Windows.Forms.CheckBox();
            this.chkNC_E = new System.Windows.Forms.CheckBox();
            this.chkNC_Ap = new System.Windows.Forms.CheckBox();
            this.chkNC_Dl = new System.Windows.Forms.CheckBox();
            this.chkPC_V = new System.Windows.Forms.CheckBox();
            this.chkPC_A = new System.Windows.Forms.CheckBox();
            this.chkPC_E = new System.Windows.Forms.CheckBox();
            this.chkPC_Ap = new System.Windows.Forms.CheckBox();
            this.chkPC_Dl = new System.Windows.Forms.CheckBox();
            this.lblDetailHint = new System.Windows.Forms.Label();
            this.pnlDetailQuick = new System.Windows.Forms.Panel();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.lblDetailStatus = new System.Windows.Forms.Label();
            this.pnlDetailActions = new System.Windows.Forms.Panel();
            this.btnDetailSave = new System.Windows.Forms.Button();
            this.btnDetailReset = new System.Windows.Forms.Button();
            this.lblDetailEmpty = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split)).BeginInit();
            this.split.Panel1.SuspendLayout();
            this.split.Panel2.SuspendLayout();
            this.split.SuspendLayout();
            this.pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitDetail)).BeginInit();
            this.splitDetail.Panel1.SuspendLayout();
            this.splitDetail.Panel2.SuspendLayout();
            this.splitDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.pnlLevelTabs.SuspendLayout();
            this.pnlGridTop.SuspendLayout();
            this.pnlDetail.SuspendLayout();
            this.pnlDetailBody.SuspendLayout();
            this.tlpMatrix.SuspendLayout();
            this.pnlDetailQuick.SuspendLayout();
            this.pnlDetailActions.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1184, 46);
            this.pnlHeader.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1184, 46);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "  จัดการสิทธิ์ตาม ตำแหน่ง / แผนก / ฝ่าย  (Authority Config)";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlToolbar
            //
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.pnlToolbar.Controls.Add(this.btnAdd);
            this.pnlToolbar.Controls.Add(this.btnEdit);
            this.pnlToolbar.Controls.Add(this.btnDelete);
            this.pnlToolbar.Controls.Add(this.btnImport);
            this.pnlToolbar.Controls.Add(this.btnClose);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 46);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(1184, 44);
            this.pnlToolbar.TabIndex = 1;
            //
            // btnAdd
            //
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location = new System.Drawing.Point(6, 6);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(118, 32);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "+ เพิ่มใหม่";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            //
            // btnEdit
            //
            this.btnEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(100)))), ((int)(((byte)(60)))));
            this.btnEdit.FlatAppearance.BorderSize = 0;
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnEdit.ForeColor = System.Drawing.Color.White;
            this.btnEdit.Location = new System.Drawing.Point(130, 6);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(118, 32);
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Text = "✏ แก้ไขข้อมูล";
            this.btnEdit.UseVisualStyleBackColor = false;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            //
            // btnDelete
            //
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(254, 6);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(118, 32);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "🗑 ลบ";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            //
            // btnImport
            //
            this.btnImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(105)))), ((int)(((byte)(148)))));
            this.btnImport.FlatAppearance.BorderSize = 0;
            this.btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImport.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnImport.ForeColor = System.Drawing.Color.White;
            this.btnImport.Location = new System.Drawing.Point(378, 6);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(118, 32);
            this.btnImport.TabIndex = 3;
            this.btnImport.Text = "📥 นำเข้าจาก Excel";
            this.btnImport.UseVisualStyleBackColor = false;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            //
            // btnClose
            //
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(502, 6);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(118, 32);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "✕ ปิด";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // split
            //
            this.split.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.split.Location = new System.Drawing.Point(0, 90);
            this.split.Name = "split";
            //
            // split.Panel1
            //
            this.split.Panel1.Controls.Add(this.tvOrg);
            //
            // split.Panel2
            //
            this.split.Panel2.Controls.Add(this.pnlRight);
            this.split.Size = new System.Drawing.Size(1184, 521);
            this.split.SplitterDistance = 230;
            this.split.TabIndex = 2;
            //
            // tvOrg
            //
            this.tvOrg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(253)))));
            this.tvOrg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tvOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvOrg.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tvOrg.HideSelection = false;
            this.tvOrg.Location = new System.Drawing.Point(0, 0);
            this.tvOrg.Name = "tvOrg";
            this.tvOrg.Size = new System.Drawing.Size(230, 521);
            this.tvOrg.TabIndex = 0;
            this.tvOrg.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvOrg_BeforeSelect);
            //
            // pnlRight
            //
            this.pnlRight.Controls.Add(this.splitDetail);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(0, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(950, 521);
            this.pnlRight.TabIndex = 0;
            //
            // splitDetail
            //
            this.splitDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitDetail.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitDetail.Location = new System.Drawing.Point(0, 0);
            this.splitDetail.Name = "splitDetail";
            //
            // splitDetail.Panel1
            //
            this.splitDetail.Panel1.Controls.Add(this.dgv);
            this.splitDetail.Panel1.Controls.Add(this.pnlLevelTabs);
            this.splitDetail.Panel1.Controls.Add(this.pnlGridTop);
            //
            // splitDetail.Panel2
            //
            this.splitDetail.Panel2.Controls.Add(this.pnlDetail);
            this.splitDetail.Size = new System.Drawing.Size(950, 521);
            this.splitDetail.Panel2MinSize = 300;
            this.splitDetail.SplitterDistance = 610;
            this.splitDetail.TabIndex = 0;
            //
            // dgv
            //
            this.dgv.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Location = new System.Drawing.Point(0, 68);
            this.dgv.Name = "dgv";
            this.dgv.RowTemplate.Height = 28;
            this.dgv.Size = new System.Drawing.Size(610, 489);
            this.dgv.TabIndex = 1;
            this.dgv.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellDoubleClick);
            this.dgv.SelectionChanged += new System.EventHandler(this.dgv_SelectionChanged);
            //
            // pnlLevelTabs
            //
            this.pnlLevelTabs.Controls.Add(this.rdoNoLevel);
            this.pnlLevelTabs.Controls.Add(this.rdoWithLevel);
            this.pnlLevelTabs.Controls.Add(this.rdoAll);
            this.pnlLevelTabs.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLevelTabs.Location = new System.Drawing.Point(0, 32);
            this.pnlLevelTabs.Name = "pnlLevelTabs";
            this.pnlLevelTabs.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.pnlLevelTabs.Size = new System.Drawing.Size(610, 36);
            this.pnlLevelTabs.TabIndex = 3;
            //
            // rdoAll
            //
            this.rdoAll.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoAll.Checked = true;
            this.rdoAll.Dock = System.Windows.Forms.DockStyle.Left;
            this.rdoAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoAll.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.rdoAll.Name = "rdoAll";
            this.rdoAll.Size = new System.Drawing.Size(130, 32);
            this.rdoAll.TabIndex = 0;
            this.rdoAll.TabStop = true;
            this.rdoAll.Text = "ทั้งหมด";
            this.rdoAll.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoAll.CheckedChanged += new System.EventHandler(this.rdoLevelTab_CheckedChanged);
            //
            // rdoWithLevel
            //
            this.rdoWithLevel.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoWithLevel.Dock = System.Windows.Forms.DockStyle.Left;
            this.rdoWithLevel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoWithLevel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.rdoWithLevel.Name = "rdoWithLevel";
            this.rdoWithLevel.Size = new System.Drawing.Size(150, 32);
            this.rdoWithLevel.TabIndex = 1;
            this.rdoWithLevel.Text = "มีระดับ";
            this.rdoWithLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoWithLevel.CheckedChanged += new System.EventHandler(this.rdoLevelTab_CheckedChanged);
            //
            // rdoNoLevel
            //
            this.rdoNoLevel.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoNoLevel.Dock = System.Windows.Forms.DockStyle.Left;
            this.rdoNoLevel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoNoLevel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.rdoNoLevel.Name = "rdoNoLevel";
            this.rdoNoLevel.Size = new System.Drawing.Size(150, 32);
            this.rdoNoLevel.TabIndex = 2;
            this.rdoNoLevel.Text = "ไม่มีระดับ";
            this.rdoNoLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoNoLevel.CheckedChanged += new System.EventHandler(this.rdoLevelTab_CheckedChanged);
            //
            // pnlGridTop
            //
            this.pnlGridTop.Controls.Add(this.lblFilter);
            this.pnlGridTop.Controls.Add(this.lblSearch);
            this.pnlGridTop.Controls.Add(this.txtSearch);
            this.pnlGridTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGridTop.Location = new System.Drawing.Point(0, 0);
            this.pnlGridTop.Name = "pnlGridTop";
            this.pnlGridTop.Padding = new System.Windows.Forms.Padding(0, 4, 4, 4);
            this.pnlGridTop.Size = new System.Drawing.Size(610, 32);
            this.pnlGridTop.TabIndex = 0;
            //
            // lblFilter
            //
            this.lblFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFilter.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblFilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblFilter.Location = new System.Drawing.Point(0, 4);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.lblFilter.Size = new System.Drawing.Size(316, 24);
            this.lblFilter.TabIndex = 0;
            this.lblFilter.Text = "แสดง: ทั้งหมด";
            this.lblFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblSearch
            //
            this.lblSearch.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSearch.Location = new System.Drawing.Point(316, 4);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(70, 24);
            this.lblSearch.TabIndex = 1;
            this.lblSearch.Text = "🔍 ค้นหา:";
            this.lblSearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // txtSearch
            //
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Right;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.Location = new System.Drawing.Point(386, 4);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(220, 23);
            this.txtSearch.TabIndex = 2;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            //
            // pnlDetail
            //
            this.pnlDetail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(253)))));
            this.pnlDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDetail.Controls.Add(this.pnlDetailBody);
            this.pnlDetail.Controls.Add(this.lblDetailEmpty);
            this.pnlDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetail.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.pnlDetail.Location = new System.Drawing.Point(0, 0);
            this.pnlDetail.Name = "pnlDetail";
            this.pnlDetail.Padding = new System.Windows.Forms.Padding(10);
            this.pnlDetail.Size = new System.Drawing.Size(336, 521);
            this.pnlDetail.TabIndex = 0;
            //
            // pnlDetailBody
            //
            this.pnlDetailBody.Controls.Add(this.pnlDetailQuick);
            this.pnlDetailBody.Controls.Add(this.lblDetailHint);
            this.pnlDetailBody.Controls.Add(this.tlpMatrix);
            this.pnlDetailBody.Controls.Add(this.lblDetailSub);
            this.pnlDetailBody.Controls.Add(this.lblDetailTitle);
            this.pnlDetailBody.Controls.Add(this.lblDetailStatus);
            this.pnlDetailBody.Controls.Add(this.pnlDetailActions);
            this.pnlDetailBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetailBody.Location = new System.Drawing.Point(10, 10);
            this.pnlDetailBody.Name = "pnlDetailBody";
            this.pnlDetailBody.Size = new System.Drawing.Size(314, 499);
            this.pnlDetailBody.TabIndex = 0;
            this.pnlDetailBody.Visible = false;
            //
            // lblDetailTitle
            //
            this.lblDetailTitle.AutoEllipsis = true;
            this.lblDetailTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblDetailTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblDetailTitle.Location = new System.Drawing.Point(0, 0);
            this.lblDetailTitle.Name = "lblDetailTitle";
            this.lblDetailTitle.Size = new System.Drawing.Size(314, 26);
            this.lblDetailTitle.TabIndex = 0;
            this.lblDetailTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblDetailSub
            //
            this.lblDetailSub.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailSub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblDetailSub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.lblDetailSub.Location = new System.Drawing.Point(0, 26);
            this.lblDetailSub.Name = "lblDetailSub";
            this.lblDetailSub.Size = new System.Drawing.Size(314, 56);
            this.lblDetailSub.TabIndex = 1;
            //
            // tlpMatrix
            //
            this.tlpMatrix.BackColor = System.Drawing.Color.White;
            this.tlpMatrix.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tlpMatrix.ColumnCount = 6;
            this.tlpMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tlpMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpMatrix.Controls.Add(this.lblMxCorner, 0, 0);
            this.tlpMatrix.Controls.Add(this.lblMxView, 1, 0);
            this.tlpMatrix.Controls.Add(this.lblMxAdd, 2, 0);
            this.tlpMatrix.Controls.Add(this.lblMxEdit, 3, 0);
            this.tlpMatrix.Controls.Add(this.lblMxApprove, 4, 0);
            this.tlpMatrix.Controls.Add(this.lblMxDownload, 5, 0);
            this.tlpMatrix.Controls.Add(this.lblMxHC, 0, 1);
            this.tlpMatrix.Controls.Add(this.chkHC_V, 1, 1);
            this.tlpMatrix.Controls.Add(this.chkHC_A, 2, 1);
            this.tlpMatrix.Controls.Add(this.chkHC_E, 3, 1);
            this.tlpMatrix.Controls.Add(this.chkHC_Ap, 4, 1);
            this.tlpMatrix.Controls.Add(this.chkHC_Dl, 5, 1);
            this.tlpMatrix.Controls.Add(this.lblMxSC, 0, 2);
            this.tlpMatrix.Controls.Add(this.chkSC_V, 1, 2);
            this.tlpMatrix.Controls.Add(this.chkSC_A, 2, 2);
            this.tlpMatrix.Controls.Add(this.chkSC_E, 3, 2);
            this.tlpMatrix.Controls.Add(this.chkSC_Ap, 4, 2);
            this.tlpMatrix.Controls.Add(this.chkSC_Dl, 5, 2);
            this.tlpMatrix.Controls.Add(this.lblMxNC, 0, 3);
            this.tlpMatrix.Controls.Add(this.chkNC_V, 1, 3);
            this.tlpMatrix.Controls.Add(this.chkNC_A, 2, 3);
            this.tlpMatrix.Controls.Add(this.chkNC_E, 3, 3);
            this.tlpMatrix.Controls.Add(this.chkNC_Ap, 4, 3);
            this.tlpMatrix.Controls.Add(this.chkNC_Dl, 5, 3);
            this.tlpMatrix.Controls.Add(this.lblMxPC, 0, 4);
            this.tlpMatrix.Controls.Add(this.chkPC_V, 1, 4);
            this.tlpMatrix.Controls.Add(this.chkPC_A, 2, 4);
            this.tlpMatrix.Controls.Add(this.chkPC_E, 3, 4);
            this.tlpMatrix.Controls.Add(this.chkPC_Ap, 4, 4);
            this.tlpMatrix.Controls.Add(this.chkPC_Dl, 5, 4);
            this.tlpMatrix.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpMatrix.Location = new System.Drawing.Point(0, 82);
            this.tlpMatrix.Name = "tlpMatrix";
            this.tlpMatrix.RowCount = 5;
            this.tlpMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpMatrix.Size = new System.Drawing.Size(314, 180);
            this.tlpMatrix.TabIndex = 2;
            //
            // lblMxCorner
            //
            this.lblMxCorner.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblMxCorner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMxCorner.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblMxCorner.ForeColor = System.Drawing.Color.White;
            this.lblMxCorner.Margin = new System.Windows.Forms.Padding(0);
            this.lblMxCorner.Name = "lblMxCorner";
            this.lblMxCorner.Text = "ระดับ";
            this.lblMxCorner.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblMxView
            //
            this.lblMxView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblMxView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMxView.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblMxView.ForeColor = System.Drawing.Color.White;
            this.lblMxView.Margin = new System.Windows.Forms.Padding(0);
            this.lblMxView.Name = "lblMxView";
            this.lblMxView.Text = "ดู";
            this.lblMxView.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblMxAdd
            //
            this.lblMxAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblMxAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMxAdd.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblMxAdd.ForeColor = System.Drawing.Color.White;
            this.lblMxAdd.Margin = new System.Windows.Forms.Padding(0);
            this.lblMxAdd.Name = "lblMxAdd";
            this.lblMxAdd.Text = "เพิ่ม";
            this.lblMxAdd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblMxEdit
            //
            this.lblMxEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblMxEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMxEdit.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblMxEdit.ForeColor = System.Drawing.Color.White;
            this.lblMxEdit.Margin = new System.Windows.Forms.Padding(0);
            this.lblMxEdit.Name = "lblMxEdit";
            this.lblMxEdit.Text = "แก้ไข";
            this.lblMxEdit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblMxApprove
            //
            this.lblMxApprove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblMxApprove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMxApprove.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblMxApprove.ForeColor = System.Drawing.Color.White;
            this.lblMxApprove.Margin = new System.Windows.Forms.Padding(0);
            this.lblMxApprove.Name = "lblMxApprove";
            this.lblMxApprove.Text = "อนุมัติ";
            this.lblMxApprove.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblMxDownload
            //
            this.lblMxDownload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblMxDownload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMxDownload.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblMxDownload.ForeColor = System.Drawing.Color.White;
            this.lblMxDownload.Margin = new System.Windows.Forms.Padding(0);
            this.lblMxDownload.Name = "lblMxDownload";
            this.lblMxDownload.Text = "⬇ โหลด";
            this.lblMxDownload.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblMxHC
            //
            this.lblMxHC.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lblMxHC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMxHC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMxHC.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblMxHC.ForeColor = System.Drawing.Color.White;
            this.lblMxHC.Margin = new System.Windows.Forms.Padding(0);
            this.lblMxHC.Name = "lblMxHC";
            this.lblMxHC.Tag = 0;
            this.lblMxHC.Text = "HC\r\nสูงสุด";
            this.lblMxHC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblMxHC.Click += new System.EventHandler(this.lblLevel_Click);
            //
            // lblMxSC
            //
            this.lblMxSC.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(100)))), ((int)(((byte)(0)))));
            this.lblMxSC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMxSC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMxSC.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblMxSC.ForeColor = System.Drawing.Color.White;
            this.lblMxSC.Margin = new System.Windows.Forms.Padding(0);
            this.lblMxSC.Name = "lblMxSC";
            this.lblMxSC.Tag = 1;
            this.lblMxSC.Text = "SC\r\nลับมาก";
            this.lblMxSC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblMxSC.Click += new System.EventHandler(this.lblLevel_Click);
            //
            // lblMxNC
            //
            this.lblMxNC.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(160)))));
            this.lblMxNC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMxNC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMxNC.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblMxNC.ForeColor = System.Drawing.Color.White;
            this.lblMxNC.Margin = new System.Windows.Forms.Padding(0);
            this.lblMxNC.Name = "lblMxNC";
            this.lblMxNC.Tag = 2;
            this.lblMxNC.Text = "NC\r\nปกติ";
            this.lblMxNC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblMxNC.Click += new System.EventHandler(this.lblLevel_Click);
            //
            // lblMxPC
            //
            this.lblMxPC.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(130)))), ((int)(((byte)(60)))));
            this.lblMxPC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMxPC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMxPC.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblMxPC.ForeColor = System.Drawing.Color.White;
            this.lblMxPC.Margin = new System.Windows.Forms.Padding(0);
            this.lblMxPC.Name = "lblMxPC";
            this.lblMxPC.Tag = 3;
            this.lblMxPC.Text = "PC\r\nทั่วไป";
            this.lblMxPC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblMxPC.Click += new System.EventHandler(this.lblLevel_Click);
            //
            // chkHC_V
            //
            this.chkHC_V.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkHC_V.AutoSize = true;
            this.chkHC_V.Name = "chkHC_V";
            this.chkHC_V.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkHC_A
            //
            this.chkHC_A.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkHC_A.AutoSize = true;
            this.chkHC_A.Name = "chkHC_A";
            this.chkHC_A.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkHC_E
            //
            this.chkHC_E.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkHC_E.AutoSize = true;
            this.chkHC_E.Name = "chkHC_E";
            this.chkHC_E.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkHC_Ap
            //
            this.chkHC_Ap.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkHC_Ap.AutoSize = true;
            this.chkHC_Ap.Name = "chkHC_Ap";
            this.chkHC_Ap.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkHC_Dl
            //
            this.chkHC_Dl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkHC_Dl.AutoSize = true;
            this.chkHC_Dl.Name = "chkHC_Dl";
            this.chkHC_Dl.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkSC_V
            //
            this.chkSC_V.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkSC_V.AutoSize = true;
            this.chkSC_V.Name = "chkSC_V";
            this.chkSC_V.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkSC_A
            //
            this.chkSC_A.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkSC_A.AutoSize = true;
            this.chkSC_A.Name = "chkSC_A";
            this.chkSC_A.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkSC_E
            //
            this.chkSC_E.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkSC_E.AutoSize = true;
            this.chkSC_E.Name = "chkSC_E";
            this.chkSC_E.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkSC_Ap
            //
            this.chkSC_Ap.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkSC_Ap.AutoSize = true;
            this.chkSC_Ap.Name = "chkSC_Ap";
            this.chkSC_Ap.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkSC_Dl
            //
            this.chkSC_Dl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkSC_Dl.AutoSize = true;
            this.chkSC_Dl.Name = "chkSC_Dl";
            this.chkSC_Dl.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkNC_V
            //
            this.chkNC_V.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkNC_V.AutoSize = true;
            this.chkNC_V.Name = "chkNC_V";
            this.chkNC_V.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkNC_A
            //
            this.chkNC_A.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkNC_A.AutoSize = true;
            this.chkNC_A.Name = "chkNC_A";
            this.chkNC_A.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkNC_E
            //
            this.chkNC_E.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkNC_E.AutoSize = true;
            this.chkNC_E.Name = "chkNC_E";
            this.chkNC_E.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkNC_Ap
            //
            this.chkNC_Ap.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkNC_Ap.AutoSize = true;
            this.chkNC_Ap.Name = "chkNC_Ap";
            this.chkNC_Ap.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkNC_Dl
            //
            this.chkNC_Dl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkNC_Dl.AutoSize = true;
            this.chkNC_Dl.Name = "chkNC_Dl";
            this.chkNC_Dl.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkPC_V
            //
            this.chkPC_V.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkPC_V.AutoSize = true;
            this.chkPC_V.Name = "chkPC_V";
            this.chkPC_V.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkPC_A
            //
            this.chkPC_A.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkPC_A.AutoSize = true;
            this.chkPC_A.Name = "chkPC_A";
            this.chkPC_A.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkPC_E
            //
            this.chkPC_E.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkPC_E.AutoSize = true;
            this.chkPC_E.Name = "chkPC_E";
            this.chkPC_E.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkPC_Ap
            //
            this.chkPC_Ap.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkPC_Ap.AutoSize = true;
            this.chkPC_Ap.Name = "chkPC_Ap";
            this.chkPC_Ap.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // chkPC_Dl
            //
            this.chkPC_Dl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkPC_Dl.AutoSize = true;
            this.chkPC_Dl.Name = "chkPC_Dl";
            this.chkPC_Dl.CheckedChanged += new System.EventHandler(this.chkPerm_CheckedChanged);
            //
            // lblDetailHint
            //
            this.lblDetailHint.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailHint.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
            this.lblDetailHint.ForeColor = System.Drawing.Color.Gray;
            this.lblDetailHint.Location = new System.Drawing.Point(0, 262);
            this.lblDetailHint.Name = "lblDetailHint";
            this.lblDetailHint.Size = new System.Drawing.Size(314, 22);
            this.lblDetailHint.TabIndex = 3;
            this.lblDetailHint.Text = "คลิกชื่อระดับ (HC/SC/NC/PC) เพื่อเลือก/ยกเลิกทั้งแถว";
            this.lblDetailHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlDetailQuick
            //
            this.pnlDetailQuick.Controls.Add(this.btnSelectAll);
            this.pnlDetailQuick.Controls.Add(this.btnClearAll);
            this.pnlDetailQuick.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDetailQuick.Location = new System.Drawing.Point(0, 284);
            this.pnlDetailQuick.Name = "pnlDetailQuick";
            this.pnlDetailQuick.Size = new System.Drawing.Size(314, 34);
            this.pnlDetailQuick.TabIndex = 4;
            //
            // btnSelectAll
            //
            this.btnSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectAll.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnSelectAll.Location = new System.Drawing.Point(0, 3);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(120, 28);
            this.btnSelectAll.TabIndex = 0;
            this.btnSelectAll.Text = "☑ เลือกทั้งหมด";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            //
            // btnClearAll
            //
            this.btnClearAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearAll.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnClearAll.Location = new System.Drawing.Point(126, 3);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(120, 28);
            this.btnClearAll.TabIndex = 1;
            this.btnClearAll.Text = "☐ ล้างทั้งหมด";
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.btnClearAll_Click);
            //
            // lblDetailStatus
            //
            this.lblDetailStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblDetailStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblDetailStatus.Location = new System.Drawing.Point(0, 433);
            this.lblDetailStatus.Name = "lblDetailStatus";
            this.lblDetailStatus.Size = new System.Drawing.Size(314, 22);
            this.lblDetailStatus.TabIndex = 5;
            this.lblDetailStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlDetailActions
            //
            this.pnlDetailActions.Controls.Add(this.btnDetailSave);
            this.pnlDetailActions.Controls.Add(this.btnDetailReset);
            this.pnlDetailActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlDetailActions.Location = new System.Drawing.Point(0, 455);
            this.pnlDetailActions.Name = "pnlDetailActions";
            this.pnlDetailActions.Size = new System.Drawing.Size(314, 44);
            this.pnlDetailActions.TabIndex = 6;
            //
            // btnDetailSave
            //
            this.btnDetailSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.btnDetailSave.Enabled = false;
            this.btnDetailSave.FlatAppearance.BorderSize = 0;
            this.btnDetailSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDetailSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnDetailSave.ForeColor = System.Drawing.Color.White;
            this.btnDetailSave.Location = new System.Drawing.Point(0, 6);
            this.btnDetailSave.Name = "btnDetailSave";
            this.btnDetailSave.Size = new System.Drawing.Size(140, 34);
            this.btnDetailSave.TabIndex = 0;
            this.btnDetailSave.Text = "💾 บันทึกสิทธิ์";
            this.btnDetailSave.UseVisualStyleBackColor = false;
            this.btnDetailSave.Click += new System.EventHandler(this.btnDetailSave_Click);
            //
            // btnDetailReset
            //
            this.btnDetailReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.btnDetailReset.Enabled = false;
            this.btnDetailReset.FlatAppearance.BorderSize = 0;
            this.btnDetailReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDetailReset.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnDetailReset.ForeColor = System.Drawing.Color.White;
            this.btnDetailReset.Location = new System.Drawing.Point(146, 6);
            this.btnDetailReset.Name = "btnDetailReset";
            this.btnDetailReset.Size = new System.Drawing.Size(100, 34);
            this.btnDetailReset.TabIndex = 1;
            this.btnDetailReset.Text = "↺ คืนค่า";
            this.btnDetailReset.UseVisualStyleBackColor = false;
            this.btnDetailReset.Click += new System.EventHandler(this.btnDetailReset_Click);
            //
            // lblDetailEmpty
            //
            this.lblDetailEmpty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDetailEmpty.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblDetailEmpty.ForeColor = System.Drawing.Color.Gray;
            this.lblDetailEmpty.Location = new System.Drawing.Point(10, 10);
            this.lblDetailEmpty.Name = "lblDetailEmpty";
            this.lblDetailEmpty.Size = new System.Drawing.Size(314, 499);
            this.lblDetailEmpty.TabIndex = 1;
            this.lblDetailEmpty.Text = "เลือกรายการทางซ้าย\r\nเพื่อดู / แก้ไขสิทธิ์";
            this.lblDetailEmpty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // AuthorityConfigForm
            //
            this.ClientSize = new System.Drawing.Size(1184, 611);
            this.Controls.Add(this.split);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);
            this.Name = "AuthorityConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Authority Config";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AuthorityConfigForm_FormClosing);
            this.Load += new System.EventHandler(this.AuthorityConfigForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.split.Panel1.ResumeLayout(false);
            this.split.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.split)).EndInit();
            this.split.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            this.splitDetail.Panel1.ResumeLayout(false);
            this.splitDetail.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitDetail)).EndInit();
            this.splitDetail.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.pnlLevelTabs.ResumeLayout(false);
            this.pnlGridTop.ResumeLayout(false);
            this.pnlGridTop.PerformLayout();
            this.pnlDetail.ResumeLayout(false);
            this.pnlDetailBody.ResumeLayout(false);
            this.tlpMatrix.ResumeLayout(false);
            this.tlpMatrix.PerformLayout();
            this.pnlDetailQuick.ResumeLayout(false);
            this.pnlDetailActions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel             pnlHeader;
        private System.Windows.Forms.Label             lblTitle;
        private System.Windows.Forms.Panel             pnlToolbar;
        private System.Windows.Forms.Button            btnAdd;
        private System.Windows.Forms.Button            btnEdit;
        private System.Windows.Forms.Button            btnDelete;
        private System.Windows.Forms.Button            btnImport;
        private System.Windows.Forms.Button            btnClose;
        private System.Windows.Forms.SplitContainer    split;
        private System.Windows.Forms.TreeView          tvOrg;
        private System.Windows.Forms.Panel             pnlRight;
        private System.Windows.Forms.SplitContainer    splitDetail;
        private System.Windows.Forms.DataGridView      dgv;
        private System.Windows.Forms.Panel             pnlGridTop;
        private System.Windows.Forms.Panel             pnlLevelTabs;
        private System.Windows.Forms.RadioButton       rdoAll;
        private System.Windows.Forms.RadioButton       rdoWithLevel;
        private System.Windows.Forms.RadioButton       rdoNoLevel;
        private System.Windows.Forms.Label             lblFilter;
        private System.Windows.Forms.Label             lblSearch;
        private System.Windows.Forms.TextBox           txtSearch;
        private System.Windows.Forms.Panel             pnlDetail;
        private System.Windows.Forms.Panel             pnlDetailBody;
        private System.Windows.Forms.Label             lblDetailTitle;
        private System.Windows.Forms.Label             lblDetailSub;
        private System.Windows.Forms.TableLayoutPanel  tlpMatrix;
        private System.Windows.Forms.Label             lblMxCorner;
        private System.Windows.Forms.Label             lblMxView;
        private System.Windows.Forms.Label             lblMxAdd;
        private System.Windows.Forms.Label             lblMxEdit;
        private System.Windows.Forms.Label             lblMxApprove;
        private System.Windows.Forms.Label             lblMxDownload;
        private System.Windows.Forms.Label             lblMxHC;
        private System.Windows.Forms.Label             lblMxSC;
        private System.Windows.Forms.Label             lblMxNC;
        private System.Windows.Forms.Label             lblMxPC;
        private System.Windows.Forms.CheckBox          chkHC_V;
        private System.Windows.Forms.CheckBox          chkHC_A;
        private System.Windows.Forms.CheckBox          chkHC_E;
        private System.Windows.Forms.CheckBox          chkHC_Ap;
        private System.Windows.Forms.CheckBox          chkHC_Dl;
        private System.Windows.Forms.CheckBox          chkSC_V;
        private System.Windows.Forms.CheckBox          chkSC_A;
        private System.Windows.Forms.CheckBox          chkSC_E;
        private System.Windows.Forms.CheckBox          chkSC_Ap;
        private System.Windows.Forms.CheckBox          chkSC_Dl;
        private System.Windows.Forms.CheckBox          chkNC_V;
        private System.Windows.Forms.CheckBox          chkNC_A;
        private System.Windows.Forms.CheckBox          chkNC_E;
        private System.Windows.Forms.CheckBox          chkNC_Ap;
        private System.Windows.Forms.CheckBox          chkNC_Dl;
        private System.Windows.Forms.CheckBox          chkPC_V;
        private System.Windows.Forms.CheckBox          chkPC_A;
        private System.Windows.Forms.CheckBox          chkPC_E;
        private System.Windows.Forms.CheckBox          chkPC_Ap;
        private System.Windows.Forms.CheckBox          chkPC_Dl;
        private System.Windows.Forms.Label             lblDetailHint;
        private System.Windows.Forms.Panel             pnlDetailQuick;
        private System.Windows.Forms.Button            btnSelectAll;
        private System.Windows.Forms.Button            btnClearAll;
        private System.Windows.Forms.Label             lblDetailStatus;
        private System.Windows.Forms.Panel             pnlDetailActions;
        private System.Windows.Forms.Button            btnDetailSave;
        private System.Windows.Forms.Button            btnDetailReset;
        private System.Windows.Forms.Label             lblDetailEmpty;
    }
}
