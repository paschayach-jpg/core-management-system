namespace RG_Project.Project.Forms
{
    partial class Frm_Task_Dashboard
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader       = new System.Windows.Forms.Panel();
            this.lblDashTitle    = new System.Windows.Forms.Label();
            this.pnlAccent       = new System.Windows.Forms.Panel();
            this.pnlStats        = new System.Windows.Forms.Panel();
            this.pnlFilter       = new System.Windows.Forms.Panel();
            this.lblProjectL     = new System.Windows.Forms.Label();
            this.cbbProject      = new System.Windows.Forms.ComboBox();
            this.lblStatusFL     = new System.Windows.Forms.Label();
            this.cbbStatusFilter = new System.Windows.Forms.ComboBox();
            this.lblAssignFL     = new System.Windows.Forms.Label();
            this.cbbAssignFilter = new System.Windows.Forms.ComboBox();
            this.txtSearch       = new System.Windows.Forms.TextBox();
            this.btnAddTask      = new System.Windows.Forms.Button();
            this.btnRefresh      = new System.Windows.Forms.Button();
            this.tabMain         = new System.Windows.Forms.TabControl();
            this.tabListView     = new System.Windows.Forms.TabPage();
            this.dgvTasks        = new System.Windows.Forms.DataGridView();
            this.tabKanbanView   = new System.Windows.Forms.TabPage();
            this.pnlBottom       = new System.Windows.Forms.Panel();
            this.btnClose        = new System.Windows.Forms.Button();
            this.btnChangeStatus = new System.Windows.Forms.Button();
            this.btnDeleteTask   = new System.Windows.Forms.Button();
            this.btnEditTask     = new System.Windows.Forms.Button();
            this.btnAddSub       = new System.Windows.Forms.Button();

            this.pnlHeader.SuspendLayout();
            this.pnlFilter.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabListView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTasks)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(30, 58, 95);
            this.pnlHeader.Controls.Add(this.lblDashTitle);
            this.pnlHeader.Dock     = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Name     = "pnlHeader";
            this.pnlHeader.Size     = new System.Drawing.Size(1200, 50);
            this.pnlHeader.TabIndex = 0;

            this.lblDashTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblDashTitle.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblDashTitle.ForeColor = System.Drawing.Color.White;
            this.lblDashTitle.Padding   = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblDashTitle.Name      = "lblDashTitle";
            this.lblDashTitle.Text      = "Task Management Dashboard";
            this.lblDashTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // pnlAccent
            this.pnlAccent.BackColor = System.Drawing.Color.FromArgb(255, 185, 0);
            this.pnlAccent.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlAccent.Name      = "pnlAccent";
            this.pnlAccent.Size      = new System.Drawing.Size(1200, 3);
            this.pnlAccent.TabIndex  = 1;

            // pnlStats
            this.pnlStats.BackColor = System.Drawing.Color.FromArgb(225, 234, 248);
            this.pnlStats.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlStats.Name      = "pnlStats";
            this.pnlStats.Size      = new System.Drawing.Size(1200, 70);
            this.pnlStats.TabIndex  = 2;

            // pnlFilter
            this.pnlFilter.BackColor = System.Drawing.Color.FromArgb(242, 246, 252);
            this.pnlFilter.Controls.Add(this.btnRefresh);
            this.pnlFilter.Controls.Add(this.btnAddTask);
            this.pnlFilter.Controls.Add(this.txtSearch);
            this.pnlFilter.Controls.Add(this.cbbAssignFilter);
            this.pnlFilter.Controls.Add(this.lblAssignFL);
            this.pnlFilter.Controls.Add(this.cbbStatusFilter);
            this.pnlFilter.Controls.Add(this.lblStatusFL);
            this.pnlFilter.Controls.Add(this.cbbProject);
            this.pnlFilter.Controls.Add(this.lblProjectL);
            this.pnlFilter.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlFilter.Name      = "pnlFilter";
            this.pnlFilter.Size      = new System.Drawing.Size(1200, 46);
            this.pnlFilter.TabIndex  = 3;

            // Labels and Combos in pnlFilter
            this.lblProjectL.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblProjectL.Location  = new System.Drawing.Point(8, 12);
            this.lblProjectL.Name      = "lblProjectL";
            this.lblProjectL.Size      = new System.Drawing.Size(62, 23);
            this.lblProjectL.Text      = "โครงการ:";
            this.lblProjectL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            this.cbbProject.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbProject.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbProject.Location = new System.Drawing.Point(74, 10);
            this.cbbProject.Name     = "cbbProject";
            this.cbbProject.Size     = new System.Drawing.Size(260, 23);
            this.cbbProject.SelectedIndexChanged += new System.EventHandler(this.cbbProject_SelectedIndexChanged);

            this.lblStatusFL.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatusFL.Location  = new System.Drawing.Point(344, 12);
            this.lblStatusFL.Name      = "lblStatusFL";
            this.lblStatusFL.Size      = new System.Drawing.Size(50, 23);
            this.lblStatusFL.Text      = "สถานะ:";
            this.lblStatusFL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            this.cbbStatusFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbStatusFilter.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbStatusFilter.Location = new System.Drawing.Point(398, 10);
            this.cbbStatusFilter.Name     = "cbbStatusFilter";
            this.cbbStatusFilter.Size     = new System.Drawing.Size(140, 23);
            this.cbbStatusFilter.SelectedIndexChanged += new System.EventHandler(this.cbbStatusFilter_Changed);

            this.lblAssignFL.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAssignFL.Location  = new System.Drawing.Point(548, 12);
            this.lblAssignFL.Name      = "lblAssignFL";
            this.lblAssignFL.Size      = new System.Drawing.Size(72, 23);
            this.lblAssignFL.Text      = "ผู้รับผิดชอบ:";
            this.lblAssignFL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            this.cbbAssignFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbAssignFilter.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.cbbAssignFilter.Location = new System.Drawing.Point(624, 10);
            this.cbbAssignFilter.Name     = "cbbAssignFilter";
            this.cbbAssignFilter.Size     = new System.Drawing.Size(160, 23);
            this.cbbAssignFilter.SelectedIndexChanged += new System.EventHandler(this.cbbAssignFilter_Changed);

            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.Location    = new System.Drawing.Point(794, 10);
            this.txtSearch.Name        = "txtSearch";
            this.txtSearch.Size        = new System.Drawing.Size(180, 23);
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);

            this.btnAddTask.BackColor = System.Drawing.Color.FromArgb(43, 87, 154);
            this.btnAddTask.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAddTask.FlatAppearance.BorderSize = 0;
            this.btnAddTask.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddTask.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAddTask.ForeColor = System.Drawing.Color.White;
            this.btnAddTask.Location  = new System.Drawing.Point(984, 8);
            this.btnAddTask.Name      = "btnAddTask";
            this.btnAddTask.Size      = new System.Drawing.Size(110, 30);
            this.btnAddTask.Text      = "+ สร้าง Task";
            this.btnAddTask.UseVisualStyleBackColor = false;
            this.btnAddTask.Click    += new System.EventHandler(this.btnAddTask_Click);

            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(100, 130, 168);
            this.btnRefresh.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location  = new System.Drawing.Point(1102, 8);
            this.btnRefresh.Name      = "btnRefresh";
            this.btnRefresh.Size      = new System.Drawing.Size(80, 30);
            this.btnRefresh.Text      = "Reload";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click    += new System.EventHandler(this.btnRefresh_Click);

            // tabMain
            this.tabMain.Controls.Add(this.tabListView);
            this.tabMain.Controls.Add(this.tabKanbanView);
            this.tabMain.Dock        = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.DrawMode    = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabMain.Font        = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.tabMain.ItemSize    = new System.Drawing.Size(160, 28);
            this.tabMain.Name        = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.TabIndex    = 4;
            this.tabMain.DrawItem   += new System.Windows.Forms.DrawItemEventHandler(this.tabMain_DrawItem);
            this.tabMain.SelectedIndexChanged += new System.EventHandler(this.tabMain_SelectedIndexChanged);

            // tabListView
            this.tabListView.BackColor = System.Drawing.Color.FromArgb(242, 246, 252);
            this.tabListView.Controls.Add(this.dgvTasks);
            this.tabListView.Name     = "tabListView";
            this.tabListView.Padding  = new System.Windows.Forms.Padding(4);
            this.tabListView.Text     = "  รายการ Task  ";

            // dgvTasks
            this.dgvTasks.AllowUserToAddRows    = false;
            this.dgvTasks.AllowUserToDeleteRows = false;
            this.dgvTasks.BackgroundColor       = System.Drawing.Color.White;
            this.dgvTasks.BorderStyle           = System.Windows.Forms.BorderStyle.None;
            this.dgvTasks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvTasks.Dock             = System.Windows.Forms.DockStyle.Fill;
            this.dgvTasks.MultiSelect      = false;
            this.dgvTasks.Name             = "dgvTasks";
            this.dgvTasks.ReadOnly         = true;
            this.dgvTasks.RowHeadersVisible = false;
            this.dgvTasks.SelectionMode    = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTasks.TabIndex         = 0;
            this.dgvTasks.CellClick       += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTasks_CellClick);
            this.dgvTasks.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTasks_CellDoubleClick);
            this.dgvTasks.CellFormatting  += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvTasks_CellFormatting);
            this.dgvTasks.RowPostPaint    += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgvTasks_RowPostPaint);
            this.dgvTasks.SelectionChanged += new System.EventHandler(this.dgvTasks_SelectionChanged);

            // tabKanbanView
            this.tabKanbanView.BackColor = System.Drawing.Color.FromArgb(242, 246, 252);
            this.tabKanbanView.Name      = "tabKanbanView";
            this.tabKanbanView.Text      = "  Kanban  ";

            // pnlBottom
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(30, 58, 95);
            this.pnlBottom.Controls.Add(this.btnClose);
            this.pnlBottom.Controls.Add(this.btnChangeStatus);
            this.pnlBottom.Controls.Add(this.btnDeleteTask);
            this.pnlBottom.Controls.Add(this.btnEditTask);
            this.pnlBottom.Controls.Add(this.btnAddSub);
            this.pnlBottom.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Name      = "pnlBottom";
            this.pnlBottom.Size      = new System.Drawing.Size(1200, 44);
            this.pnlBottom.TabIndex  = 5;

            // btnAddSub
            this.btnAddSub.BackColor = System.Drawing.Color.FromArgb(70, 110, 165);
            this.btnAddSub.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAddSub.FlatAppearance.BorderSize = 0;
            this.btnAddSub.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddSub.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnAddSub.ForeColor = System.Drawing.Color.White;
            this.btnAddSub.Location  = new System.Drawing.Point(8, 7);
            this.btnAddSub.Name      = "btnAddSub";
            this.btnAddSub.Size      = new System.Drawing.Size(120, 30);
            this.btnAddSub.Text      = "Sub Task";
            this.btnAddSub.Enabled   = false;
            this.btnAddSub.UseVisualStyleBackColor = false;
            this.btnAddSub.Click    += new System.EventHandler(this.btnAddSub_Click);

            // btnEditTask
            this.btnEditTask.BackColor = System.Drawing.Color.FromArgb(100, 130, 168);
            this.btnEditTask.Cursor   = System.Windows.Forms.Cursors.Hand;
            this.btnEditTask.FlatAppearance.BorderSize = 0;
            this.btnEditTask.FlatStyle= System.Windows.Forms.FlatStyle.Flat;
            this.btnEditTask.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.btnEditTask.ForeColor= System.Drawing.Color.White;
            this.btnEditTask.Location = new System.Drawing.Point(136, 7);
            this.btnEditTask.Name     = "btnEditTask";
            this.btnEditTask.Size     = new System.Drawing.Size(90, 30);
            this.btnEditTask.Text     = "แก้ไข";
            this.btnEditTask.Enabled  = false;
            this.btnEditTask.UseVisualStyleBackColor = false;
            this.btnEditTask.Click   += new System.EventHandler(this.btnEditTask_Click);

            // btnDeleteTask
            this.btnDeleteTask.BackColor = System.Drawing.Color.FromArgb(196, 53, 53);
            this.btnDeleteTask.Cursor   = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteTask.FlatAppearance.BorderSize = 0;
            this.btnDeleteTask.FlatStyle= System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteTask.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnDeleteTask.ForeColor= System.Drawing.Color.White;
            this.btnDeleteTask.Location = new System.Drawing.Point(234, 7);
            this.btnDeleteTask.Name     = "btnDeleteTask";
            this.btnDeleteTask.Size     = new System.Drawing.Size(80, 30);
            this.btnDeleteTask.Text     = "ลบ";
            this.btnDeleteTask.Enabled  = false;
            this.btnDeleteTask.UseVisualStyleBackColor = false;
            this.btnDeleteTask.Click   += new System.EventHandler(this.btnDeleteTask_Click);

            // btnChangeStatus
            this.btnChangeStatus.BackColor = System.Drawing.Color.FromArgb(111, 66, 193);
            this.btnChangeStatus.Cursor   = System.Windows.Forms.Cursors.Hand;
            this.btnChangeStatus.FlatAppearance.BorderSize = 0;
            this.btnChangeStatus.FlatStyle= System.Windows.Forms.FlatStyle.Flat;
            this.btnChangeStatus.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.btnChangeStatus.ForeColor= System.Drawing.Color.White;
            this.btnChangeStatus.Location = new System.Drawing.Point(322, 7);
            this.btnChangeStatus.Name     = "btnChangeStatus";
            this.btnChangeStatus.Size     = new System.Drawing.Size(120, 30);
            this.btnChangeStatus.Text     = "เปลี่ยนสถานะ";
            this.btnChangeStatus.Enabled  = false;
            this.btnChangeStatus.UseVisualStyleBackColor = false;
            this.btnChangeStatus.Click   += new System.EventHandler(this.btnChangeStatus_Click);

            // btnClose
            this.btnClose.Anchor   = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.BackColor= System.Drawing.Color.FromArgb(70, 90, 120);
            this.btnClose.Cursor   = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle= System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.btnClose.ForeColor= System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(1090, 7);
            this.btnClose.Name     = "btnClose";
            this.btnClose.Size     = new System.Drawing.Size(100, 30);
            this.btnClose.Text     = "ปิด";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click   += new System.EventHandler(this.btnClose_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor     = System.Drawing.Color.FromArgb(242, 246, 252);
            this.ClientSize    = new System.Drawing.Size(1200, 720);
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlFilter);
            this.Controls.Add(this.pnlStats);
            this.Controls.Add(this.pnlAccent);
            this.Controls.Add(this.pnlHeader);
            this.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize   = new System.Drawing.Size(960, 600);
            this.Name          = "Frm_Task_Dashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text          = "Task Management Dashboard";
            this.WindowState   = System.Windows.Forms.FormWindowState.Maximized;
            this.Load         += new System.EventHandler(this.Frm_Task_Dashboard_Load);

            this.pnlHeader.ResumeLayout(false);
            this.pnlFilter.ResumeLayout(false);
            this.pnlFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTasks)).EndInit();
            this.tabListView.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel          pnlHeader;
        private System.Windows.Forms.Label          lblDashTitle;
        private System.Windows.Forms.Panel          pnlAccent;
        private System.Windows.Forms.Panel          pnlStats;
        private System.Windows.Forms.Panel          pnlFilter;
        private System.Windows.Forms.Label          lblProjectL;
        private System.Windows.Forms.ComboBox       cbbProject;
        private System.Windows.Forms.Label          lblStatusFL;
        private System.Windows.Forms.ComboBox       cbbStatusFilter;
        private System.Windows.Forms.Label          lblAssignFL;
        private System.Windows.Forms.ComboBox       cbbAssignFilter;
        private System.Windows.Forms.TextBox        txtSearch;
        private System.Windows.Forms.Button         btnAddTask;
        private System.Windows.Forms.Button         btnRefresh;
        private System.Windows.Forms.TabControl     tabMain;
        private System.Windows.Forms.TabPage        tabListView;
        private System.Windows.Forms.DataGridView   dgvTasks;
        private System.Windows.Forms.TabPage        tabKanbanView;
        private System.Windows.Forms.Panel          pnlBottom;
        private System.Windows.Forms.Button         btnAddSub;
        private System.Windows.Forms.Button         btnEditTask;
        private System.Windows.Forms.Button         btnDeleteTask;
        private System.Windows.Forms.Button         btnChangeStatus;
        private System.Windows.Forms.Button         btnClose;
    }
}
