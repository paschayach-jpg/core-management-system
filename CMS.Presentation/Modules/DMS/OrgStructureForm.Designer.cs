namespace CMS.Presentation
{
    partial class OrgStructureForm
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
            this.pnlBreadcrumb = new System.Windows.Forms.Panel();
            this.lblBreadcrumb = new System.Windows.Forms.Label();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnCreate = new System.Windows.Forms.ToolStripButton();
            this.btnEdit = new System.Windows.Forms.ToolStripButton();
            this.btnMove = new System.Windows.Forms.ToolStripButton();
            this.btnDelete = new System.Windows.Forms.ToolStripButton();
            this.separator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnDeactivate = new System.Windows.Forms.ToolStripButton();
            this.separator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnExpandAll = new System.Windows.Forms.ToolStripButton();
            this.btnCollapseAll = new System.Windows.Forms.ToolStripButton();
            this.separator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnClose = new System.Windows.Forms.ToolStripButton();
            this.splitterMain = new System.Windows.Forms.Splitter();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.pnlConfLevels = new System.Windows.Forms.Panel();
            this.chk1HC = new System.Windows.Forms.CheckBox();
            this.chk2SC = new System.Windows.Forms.CheckBox();
            this.chk3NC = new System.Windows.Forms.CheckBox();
            this.chk4PC = new System.Windows.Forms.CheckBox();
            this.lblConfLevel = new System.Windows.Forms.Label();
            this.viewPanel = new System.Windows.Forms.Panel();
            this.btnViewOrg = new System.Windows.Forms.Button();
            this.btnViewConf = new System.Windows.Forms.Button();
            this.lblViewMode = new System.Windows.Forms.Label();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.treeView = new System.Windows.Forms.TreeView();
            this.pnlBreadcrumb.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.pnlConfLevels.SuspendLayout();
            this.viewPanel.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBreadcrumb
            // 
            this.pnlBreadcrumb.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.pnlBreadcrumb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBreadcrumb.Controls.Add(this.lblBreadcrumb);
            this.pnlBreadcrumb.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBreadcrumb.Location = new System.Drawing.Point(0, 0);
            this.pnlBreadcrumb.Margin = new System.Windows.Forms.Padding(4);
            this.pnlBreadcrumb.Name = "pnlBreadcrumb";
            this.pnlBreadcrumb.Padding = new System.Windows.Forms.Padding(11, 7, 11, 7);
            this.pnlBreadcrumb.Size = new System.Drawing.Size(1333, 43);
            this.pnlBreadcrumb.TabIndex = 3;
            // 
            // lblBreadcrumb
            // 
            this.lblBreadcrumb.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblBreadcrumb.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.lblBreadcrumb.Location = new System.Drawing.Point(11, 7);
            this.lblBreadcrumb.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBreadcrumb.Name = "lblBreadcrumb";
            this.lblBreadcrumb.Size = new System.Drawing.Size(267, 27);
            this.lblBreadcrumb.TabIndex = 0;
            this.lblBreadcrumb.Text = "🏠 หน้าแรก > 📁 จัดการโครงสร้างองค์กร";
            this.lblBreadcrumb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStrip
            // 
            this.toolStrip.BackColor = System.Drawing.Color.White;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnCreate,
            this.btnEdit,
            this.btnMove,
            this.btnDelete,
            this.separator1,
            this.btnDeactivate,
            this.separator2,
            this.btnExpandAll,
            this.btnCollapseAll,
            this.separator3,
            this.btnClose});
            this.toolStrip.Location = new System.Drawing.Point(0, 43);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1333, 27);
            this.toolStrip.TabIndex = 0;
            // 
            // btnCreate
            // 
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(49, 24);
            this.btnCreate.Text = "+ เพิ่ม";
            this.btnCreate.ToolTipText = "สร้างหน่วยงานใหม่";
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(69, 24);
            this.btnEdit.Text = "✏️ แก้ไข";
            this.btnEdit.ToolTipText = "แก้ไขชื่อหน่วยงาน";
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnMove
            // 
            this.btnMove.Name = "btnMove";
            this.btnMove.Size = new System.Drawing.Size(61, 24);
            this.btnMove.Text = "📁 ย้าย";
            this.btnMove.ToolTipText = "ย้ายหน่วยงาน";
            this.btnMove.Click += new System.EventHandler(this.btnMove_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(55, 24);
            this.btnDelete.Text = "🗑️ ลบ";
            this.btnDelete.ToolTipText = "ลบหน่วยงาน";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // separator1
            // 
            this.separator1.Name = "separator1";
            this.separator1.Size = new System.Drawing.Size(6, 27);
            //
            // btnDeactivate
            //
            this.btnDeactivate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnDeactivate.Name = "btnDeactivate";
            this.btnDeactivate.Size = new System.Drawing.Size(91, 24);
            this.btnDeactivate.Text = "⛔ ปิดใช้งาน";
            this.btnDeactivate.ToolTipText = "ปิดใช้งานหน่วยงานที่เลือก";
            this.btnDeactivate.Click += new System.EventHandler(this.btnDeactivate_Click);
            //
            // separator2
            //
            this.separator2.Name = "separator2";
            this.separator2.Size = new System.Drawing.Size(6, 27);
            //
            // btnExpandAll
            //
            this.btnExpandAll.Name = "btnExpandAll";
            this.btnExpandAll.Size = new System.Drawing.Size(75, 24);
            this.btnExpandAll.Text = "➕ ขยายทั้งหมด";
            this.btnExpandAll.ToolTipText = "ขยายรายการทั้งหมด";
            this.btnExpandAll.Click += new System.EventHandler(this.btnExpandAll_Click);
            //
            // btnCollapseAll
            //
            this.btnCollapseAll.Name = "btnCollapseAll";
            this.btnCollapseAll.Size = new System.Drawing.Size(75, 24);
            this.btnCollapseAll.Text = "➖ ย่อทั้งหมด";
            this.btnCollapseAll.ToolTipText = "ย่อรายการทั้งหมด";
            this.btnCollapseAll.Click += new System.EventHandler(this.btnCollapseAll_Click);
            //
            // separator3
            //
            this.separator3.Name = "separator3";
            this.separator3.Size = new System.Drawing.Size(6, 27);
            //
            // btnClose
            //
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(55, 24);
            this.btnClose.Text = "✖ ปิด";
            this.btnClose.ToolTipText = "ปิดหน้าต่างนี้";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // splitterMain
            // 
            this.splitterMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.splitterMain.Location = new System.Drawing.Point(247, 70);
            this.splitterMain.Margin = new System.Windows.Forms.Padding(4);
            this.splitterMain.Name = "splitterMain";
            this.splitterMain.Size = new System.Drawing.Size(5, 668);
            this.splitterMain.TabIndex = 1;
            this.splitterMain.TabStop = false;
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.pnlLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLeft.Controls.Add(this.txtSearch);
            this.pnlLeft.Controls.Add(this.lblSearch);
            this.pnlLeft.Controls.Add(this.pnlConfLevels);
            this.pnlLeft.Controls.Add(this.lblConfLevel);
            this.pnlLeft.Controls.Add(this.viewPanel);
            this.pnlLeft.Controls.Add(this.lblViewMode);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 70);
            this.pnlLeft.Margin = new System.Windows.Forms.Padding(4);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(11, 10, 11, 10);
            this.pnlLeft.Size = new System.Drawing.Size(247, 668);
            this.pnlLeft.TabIndex = 2;
            this.pnlLeft.Visible = true;
            // 
            // txtSearch
            // 
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.ForeColor = System.Drawing.Color.Gray;
            this.txtSearch.Location = new System.Drawing.Point(11, 297);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(223, 27);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.Text = "ค้นหาหน่วยงาน...";
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.Enter += new System.EventHandler(this.txtSearch_Enter);
            this.txtSearch.Leave += new System.EventHandler(this.txtSearch_Leave);
            //
            // lblSearch
            // 
            this.lblSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.lblSearch.Location = new System.Drawing.Point(11, 267);
            this.lblSearch.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(223, 30);
            this.lblSearch.TabIndex = 1;
            this.lblSearch.Text = "ค้นหา:";
            // 
            // pnlConfLevels
            // 
            this.pnlConfLevels.BackColor = System.Drawing.Color.Transparent;
            this.pnlConfLevels.Controls.Add(this.chk1HC);
            this.pnlConfLevels.Controls.Add(this.chk2SC);
            this.pnlConfLevels.Controls.Add(this.chk3NC);
            this.pnlConfLevels.Controls.Add(this.chk4PC);
            this.pnlConfLevels.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlConfLevels.Location = new System.Drawing.Point(11, 144);
            this.pnlConfLevels.Margin = new System.Windows.Forms.Padding(4);
            this.pnlConfLevels.Name = "pnlConfLevels";
            this.pnlConfLevels.Padding = new System.Windows.Forms.Padding(11, 10, 11, 10);
            this.pnlConfLevels.Size = new System.Drawing.Size(223, 123);
            this.pnlConfLevels.TabIndex = 2;
            // 
            // chk1HC
            // 
            this.chk1HC.AutoSize = true;
            this.chk1HC.Checked = true;
            this.chk1HC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk1HC.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chk1HC.Location = new System.Drawing.Point(11, 10);
            this.chk1HC.Margin = new System.Windows.Forms.Padding(4);
            this.chk1HC.Name = "chk1HC";
            this.chk1HC.Size = new System.Drawing.Size(84, 24);
            this.chk1HC.TabIndex = 0;
            this.chk1HC.Text = "☑️ 1HC";
            this.chk1HC.CheckedChanged += new System.EventHandler(this.OnConfidentialityLevelChanged);
            // 
            // chk2SC
            // 
            this.chk2SC.AutoSize = true;
            this.chk2SC.Checked = true;
            this.chk2SC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk2SC.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chk2SC.Location = new System.Drawing.Point(11, 34);
            this.chk2SC.Margin = new System.Windows.Forms.Padding(4);
            this.chk2SC.Name = "chk2SC";
            this.chk2SC.Size = new System.Drawing.Size(81, 24);
            this.chk2SC.TabIndex = 1;
            this.chk2SC.Text = "☑️ 2SC";
            this.chk2SC.CheckedChanged += new System.EventHandler(this.OnConfidentialityLevelChanged);
            // 
            // chk3NC
            // 
            this.chk3NC.AutoSize = true;
            this.chk3NC.Checked = true;
            this.chk3NC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk3NC.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chk3NC.Location = new System.Drawing.Point(11, 59);
            this.chk3NC.Margin = new System.Windows.Forms.Padding(4);
            this.chk3NC.Name = "chk3NC";
            this.chk3NC.Size = new System.Drawing.Size(84, 24);
            this.chk3NC.TabIndex = 2;
            this.chk3NC.Text = "☑️ 3NC";
            this.chk3NC.CheckedChanged += new System.EventHandler(this.OnConfidentialityLevelChanged);
            // 
            // chk4PC
            // 
            this.chk4PC.AutoSize = true;
            this.chk4PC.Checked = true;
            this.chk4PC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk4PC.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chk4PC.Location = new System.Drawing.Point(11, 84);
            this.chk4PC.Margin = new System.Windows.Forms.Padding(4);
            this.chk4PC.Name = "chk4PC";
            this.chk4PC.Size = new System.Drawing.Size(81, 24);
            this.chk4PC.TabIndex = 3;
            this.chk4PC.Text = "☑️ 4PC";
            this.chk4PC.CheckedChanged += new System.EventHandler(this.OnConfidentialityLevelChanged);
            // 
            // lblConfLevel
            // 
            this.lblConfLevel.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblConfLevel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblConfLevel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.lblConfLevel.Location = new System.Drawing.Point(11, 114);
            this.lblConfLevel.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.lblConfLevel.Name = "lblConfLevel";
            this.lblConfLevel.Size = new System.Drawing.Size(223, 30);
            this.lblConfLevel.TabIndex = 3;
            this.lblConfLevel.Text = "ระดับระดับ:";
            //
            // viewPanel
            //
            this.viewPanel.BackColor = System.Drawing.Color.Transparent;
            this.viewPanel.Controls.Add(this.btnViewOrg);
            this.viewPanel.Controls.Add(this.btnViewConf);
            this.viewPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.viewPanel.Location = new System.Drawing.Point(11, 40);
            this.viewPanel.Margin = new System.Windows.Forms.Padding(4);
            this.viewPanel.Name = "viewPanel";
            this.viewPanel.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.viewPanel.Size = new System.Drawing.Size(223, 84);
            this.viewPanel.TabIndex = 4;
            //
            // btnViewOrg
            //
            this.btnViewOrg.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnViewOrg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewOrg.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnViewOrg.ForeColor = System.Drawing.Color.White;
            this.btnViewOrg.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.btnViewOrg.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnViewOrg.Name = "btnViewOrg";
            this.btnViewOrg.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.btnViewOrg.Size = new System.Drawing.Size(223, 36);
            this.btnViewOrg.TabIndex = 0;
            this.btnViewOrg.Text = "🏢  ตามองค์กร";
            this.btnViewOrg.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnViewOrg.UseVisualStyleBackColor = false;
            this.btnViewOrg.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewOrg.Click += new System.EventHandler(this.btnViewOrg_Click);
            //
            // btnViewConf
            //
            this.btnViewConf.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnViewConf.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewConf.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnViewConf.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.btnViewConf.BackColor = System.Drawing.Color.White;
            this.btnViewConf.Name = "btnViewConf";
            this.btnViewConf.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.btnViewConf.Size = new System.Drawing.Size(223, 36);
            this.btnViewConf.TabIndex = 1;
            this.btnViewConf.Text = "🔒  ตามความลับ";
            this.btnViewConf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnViewConf.UseVisualStyleBackColor = false;
            this.btnViewConf.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewConf.Click += new System.EventHandler(this.btnViewConf_Click);
            // 
            // lblViewMode
            // 
            this.lblViewMode.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblViewMode.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblViewMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.lblViewMode.Location = new System.Drawing.Point(11, 10);
            this.lblViewMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblViewMode.Name = "lblViewMode";
            this.lblViewMode.Size = new System.Drawing.Size(223, 30);
            this.lblViewMode.TabIndex = 5;
            this.lblViewMode.Text = "มุมมอง:";
            // 
            // pnlRight
            // 
            this.pnlRight.BackColor = System.Drawing.Color.White;
            this.pnlRight.Controls.Add(this.treeView);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(252, 70);
            this.pnlRight.Margin = new System.Windows.Forms.Padding(4);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Padding = new System.Windows.Forms.Padding(11, 10, 11, 10);
            this.pnlRight.Size = new System.Drawing.Size(1081, 668);
            this.pnlRight.TabIndex = 0;
            // 
            // treeView
            // 
            this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.treeView.Location = new System.Drawing.Point(11, 10);
            this.treeView.Margin = new System.Windows.Forms.Padding(4);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(1059, 648);
            this.treeView.TabIndex = 0;
            // 
            // OrgStructureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1333, 738);
            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.splitterMain);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.pnlBreadcrumb);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "OrgStructureForm";
            this.Text = "จัดการโครงสร้างองค์กร";
            this.Load += new System.EventHandler(this.OrgStructureForm_Load);
            this.pnlBreadcrumb.ResumeLayout(false);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeft.PerformLayout();
            this.pnlConfLevels.ResumeLayout(false);
            this.pnlConfLevels.PerformLayout();
            this.viewPanel.ResumeLayout(false);
            this.viewPanel.PerformLayout();
            this.pnlRight.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Panel              pnlBreadcrumb;
        private System.Windows.Forms.Label              lblBreadcrumb;
        private System.Windows.Forms.ToolStrip          toolStrip;
        private System.Windows.Forms.ToolStripButton    btnCreate;
        private System.Windows.Forms.ToolStripButton    btnEdit;
        private System.Windows.Forms.ToolStripButton    btnMove;
        private System.Windows.Forms.ToolStripButton    btnDelete;
        private System.Windows.Forms.ToolStripButton    btnDeactivate;
        private System.Windows.Forms.ToolStripSeparator separator1;
        private System.Windows.Forms.ToolStripSeparator separator2;
        private System.Windows.Forms.ToolStripButton    btnExpandAll;
        private System.Windows.Forms.ToolStripButton    btnCollapseAll;
        private System.Windows.Forms.ToolStripSeparator separator3;
        private System.Windows.Forms.ToolStripButton    btnClose;
        private System.Windows.Forms.Splitter           splitterMain;
        private System.Windows.Forms.Panel              pnlLeft;
        private System.Windows.Forms.Label              lblViewMode;
        private System.Windows.Forms.Panel              viewPanel;
        private System.Windows.Forms.Button             btnViewOrg;
        private System.Windows.Forms.Button             btnViewConf;
        private System.Windows.Forms.Label              lblConfLevel;
        private System.Windows.Forms.Panel              pnlConfLevels;
        private System.Windows.Forms.CheckBox           chk1HC;
        private System.Windows.Forms.CheckBox           chk2SC;
        private System.Windows.Forms.CheckBox           chk3NC;
        private System.Windows.Forms.CheckBox           chk4PC;
        private System.Windows.Forms.Label              lblSearch;
        private System.Windows.Forms.TextBox            txtSearch;
        private System.Windows.Forms.Panel              pnlRight;
        private System.Windows.Forms.TreeView           treeView;
    }
}
