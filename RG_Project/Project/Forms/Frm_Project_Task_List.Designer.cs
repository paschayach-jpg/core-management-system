namespace RG_Project.Project.Forms
{
    partial class Frm_Project_Task_List
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader         = new System.Windows.Forms.Panel();
            this.lblTitle          = new System.Windows.Forms.Label();
            this.pnlAccent         = new System.Windows.Forms.Panel();
            this.pnlToolbar        = new System.Windows.Forms.Panel();
            this.btnAdd            = new System.Windows.Forms.Button();
            this.btnEdit           = new System.Windows.Forms.Button();
            this.btnDelete         = new System.Windows.Forms.Button();
            this.btnStatus         = new System.Windows.Forms.Button();
            this.btnRefresh        = new System.Windows.Forms.Button();
            this.pnlFilter         = new System.Windows.Forms.Panel();
            this.lblSearchL        = new System.Windows.Forms.Label();
            this.txtSearch         = new System.Windows.Forms.TextBox();
            this.lblStatusL        = new System.Windows.Forms.Label();
            this.cbbFilterStatus   = new System.Windows.Forms.ComboBox();
            this.lblAssigneeL      = new System.Windows.Forms.Label();
            this.cbbFilterAssignee = new System.Windows.Forms.ComboBox();
            this.tabView           = new System.Windows.Forms.TabControl();
            this.tabListView       = new System.Windows.Forms.TabPage();
            this.tabKanbanView     = new System.Windows.Forms.TabPage();
            this.statusBar         = new System.Windows.Forms.StatusStrip();
            this.lblSummary        = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlFilter.SuspendLayout();
            this.tabView.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(30, 58, 95);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock     = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height   = 44;
            this.pnlHeader.Name     = "pnlHeader";
            // 
            // lblTitle
            // 
            this.lblTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Name      = "lblTitle";
            this.lblTitle.Padding   = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblTitle.Text      = "Tasks";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlAccent
            // 
            this.pnlAccent.BackColor = System.Drawing.Color.FromArgb(255, 185, 0);
            this.pnlAccent.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlAccent.Height    = 3;
            this.pnlAccent.Name      = "pnlAccent";
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(242, 246, 252);
            this.pnlToolbar.Controls.Add(this.btnAdd);
            this.pnlToolbar.Controls.Add(this.btnEdit);
            this.pnlToolbar.Controls.Add(this.btnDelete);
            this.pnlToolbar.Controls.Add(this.btnStatus);
            this.pnlToolbar.Controls.Add(this.btnRefresh);
            this.pnlToolbar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height    = 42;
            this.pnlToolbar.Name      = "pnlToolbar";
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(43, 87, 154);
            this.btnAdd.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location  = new System.Drawing.Point(6, 7);
            this.btnAdd.Name      = "btnAdd";
            this.btnAdd.Size      = new System.Drawing.Size(110, 28);
            this.btnAdd.Text      = "+ สร้าง Task";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click    += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.BackColor = System.Drawing.Color.FromArgb(70, 110, 165);
            this.btnEdit.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnEdit.FlatAppearance.BorderSize = 0;
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnEdit.ForeColor = System.Drawing.Color.White;
            this.btnEdit.Location  = new System.Drawing.Point(122, 7);
            this.btnEdit.Name      = "btnEdit";
            this.btnEdit.Size      = new System.Drawing.Size(80, 28);
            this.btnEdit.Text      = "แก้ไข";
            this.btnEdit.Enabled   = false;
            this.btnEdit.UseVisualStyleBackColor = false;
            this.btnEdit.Click    += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(196, 53, 53);
            this.btnDelete.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location  = new System.Drawing.Point(208, 7);
            this.btnDelete.Name      = "btnDelete";
            this.btnDelete.Size      = new System.Drawing.Size(70, 28);
            this.btnDelete.Text      = "ลบ";
            this.btnDelete.Enabled   = false;
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click    += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnStatus
            // 
            this.btnStatus.BackColor = System.Drawing.Color.FromArgb(100, 130, 168);
            this.btnStatus.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnStatus.FlatAppearance.BorderSize = 0;
            this.btnStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStatus.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnStatus.ForeColor = System.Drawing.Color.White;
            this.btnStatus.Location  = new System.Drawing.Point(284, 7);
            this.btnStatus.Name      = "btnStatus";
            this.btnStatus.Size      = new System.Drawing.Size(110, 28);
            this.btnStatus.Text      = "อัปเดตสถานะ";
            this.btnStatus.Enabled   = false;
            this.btnStatus.UseVisualStyleBackColor = false;
            this.btnStatus.Click    += new System.EventHandler(this.btnStatus_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(100, 130, 168);
            this.btnRefresh.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location  = new System.Drawing.Point(400, 7);
            this.btnRefresh.Name      = "btnRefresh";
            this.btnRefresh.Size      = new System.Drawing.Size(70, 28);
            this.btnRefresh.Text      = "🔄 รีเฟรช";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click    += new System.EventHandler(this.btnRefresh_Click);
            // 
            // pnlFilter
            // 
            this.pnlFilter.BackColor = System.Drawing.Color.FromArgb(229, 236, 248);
            this.pnlFilter.Controls.Add(this.lblSearchL);
            this.pnlFilter.Controls.Add(this.txtSearch);
            this.pnlFilter.Controls.Add(this.lblStatusL);
            this.pnlFilter.Controls.Add(this.cbbFilterStatus);
            this.pnlFilter.Controls.Add(this.lblAssigneeL);
            this.pnlFilter.Controls.Add(this.cbbFilterAssignee);
            this.pnlFilter.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlFilter.Height    = 38;
            this.pnlFilter.Name      = "pnlFilter";
            // 
            // lblSearchL
            // 
            this.lblSearchL.Location  = new System.Drawing.Point(6, 10);
            this.lblSearchL.AutoSize  = true;
            this.lblSearchL.Text      = "ค้นหา:";
            this.lblSearchL.ForeColor = System.Drawing.Color.FromArgb(42, 57, 80);
            // 
            // txtSearch
            // 
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Location    = new System.Drawing.Point(55, 7);
            this.txtSearch.Size        = new System.Drawing.Size(180, 23);
            this.txtSearch.Name        = "txtSearch";
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // lblStatusL
            // 
            this.lblStatusL.Location  = new System.Drawing.Point(245, 10);
            this.lblStatusL.AutoSize  = true;
            this.lblStatusL.Text      = "สถานะ:";
            this.lblStatusL.ForeColor = System.Drawing.Color.FromArgb(42, 57, 80);
            // 
            // cbbFilterStatus
            // 
            this.cbbFilterStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbFilterStatus.Location      = new System.Drawing.Point(295, 7);
            this.cbbFilterStatus.Name          = "cbbFilterStatus";
            this.cbbFilterStatus.Size          = new System.Drawing.Size(140, 23);
            this.cbbFilterStatus.SelectedIndexChanged += new System.EventHandler(this.cbbFilterStatus_SelectedIndexChanged);
            // 
            // lblAssigneeL
            // 
            this.lblAssigneeL.Location  = new System.Drawing.Point(445, 10);
            this.lblAssigneeL.AutoSize  = true;
            this.lblAssigneeL.Text      = "ผู้รับผิดชอบ:";
            this.lblAssigneeL.ForeColor = System.Drawing.Color.FromArgb(42, 57, 80);
            // 
            // cbbFilterAssignee
            // 
            this.cbbFilterAssignee.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbFilterAssignee.Location      = new System.Drawing.Point(525, 7);
            this.cbbFilterAssignee.Name          = "cbbFilterAssignee";
            this.cbbFilterAssignee.Size          = new System.Drawing.Size(180, 23);
            this.cbbFilterAssignee.SelectedIndexChanged += new System.EventHandler(this.cbbFilterAssignee_SelectedIndexChanged);
            // 
            // tabView
            // 
            this.tabView.Controls.Add(this.tabListView);
            this.tabView.Controls.Add(this.tabKanbanView);
            this.tabView.Dock          = System.Windows.Forms.DockStyle.Fill;
            this.tabView.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.tabView.Name          = "tabView";
            this.tabView.SelectedIndex = 0;
            this.tabView.SelectedIndexChanged += new System.EventHandler(this.tabView_SelectedIndexChanged);
            // 
            // tabListView
            // 
            this.tabListView.Name = "tabListView";
            this.tabListView.Text = "📋 List View";
            this.tabListView.Padding = new System.Windows.Forms.Padding(0);
            // 
            // tabKanbanView
            // 
            this.tabKanbanView.Name = "tabKanbanView";
            this.tabKanbanView.Text = "🗂️ Kanban";
            this.tabKanbanView.AutoScroll = true;
            // 
            // statusBar
            // 
            this.statusBar.BackColor = System.Drawing.Color.FromArgb(30, 58, 95);
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.lblSummary });
            this.statusBar.Name      = "statusBar";
            this.statusBar.SizingGrip = false;
            // 
            // lblSummary
            // 
            this.lblSummary.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(200, 220, 255);
            this.lblSummary.Name      = "lblSummary";
            this.lblSummary.Text      = "";
            // 
            // Frm_Project_Task_List
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(242, 246, 252);
            this.ClientSize          = new System.Drawing.Size(1100, 660);
            this.Controls.Add(this.tabView);
            this.Controls.Add(this.pnlFilter);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlAccent);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.statusBar);
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize     = new System.Drawing.Size(900, 600);
            this.Name            = "Frm_Project_Task_List";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text            = "Task Management";
            this.Load           += new System.EventHandler(this.Frm_Project_Task_List_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlFilter.ResumeLayout(false);
            this.pnlFilter.PerformLayout();
            this.tabView.ResumeLayout(false);
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
            // setup list + kanban panels is called from Frm_Project_Task_List_Load
        }

        private System.Windows.Forms.Panel             pnlHeader;
        private System.Windows.Forms.Label             lblTitle;
        private System.Windows.Forms.Panel             pnlAccent;
        private System.Windows.Forms.Panel             pnlToolbar;
        private System.Windows.Forms.Button            btnAdd;
        private System.Windows.Forms.Button            btnEdit;
        private System.Windows.Forms.Button            btnDelete;
        private System.Windows.Forms.Button            btnStatus;
        private System.Windows.Forms.Button            btnRefresh;
        private System.Windows.Forms.Panel             pnlFilter;
        private System.Windows.Forms.Label             lblSearchL;
        private System.Windows.Forms.TextBox           txtSearch;
        private System.Windows.Forms.Label             lblStatusL;
        private System.Windows.Forms.ComboBox          cbbFilterStatus;
        private System.Windows.Forms.Label             lblAssigneeL;
        private System.Windows.Forms.ComboBox          cbbFilterAssignee;
        private System.Windows.Forms.TabControl        tabView;
        private System.Windows.Forms.TabPage           tabListView;
        private System.Windows.Forms.TabPage           tabKanbanView;
        private System.Windows.Forms.StatusStrip       statusBar;
        private System.Windows.Forms.ToolStripStatusLabel lblSummary;
    }
}
