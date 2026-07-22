namespace RG_Project.Project.Forms
{
    partial class Frm_Select_RefProject
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader  = new System.Windows.Forms.Panel();
            this.lblTitle   = new System.Windows.Forms.Label();
            this.pnlAccent  = new System.Windows.Forms.Panel();
            this.pnlSearch  = new System.Windows.Forms.Panel();
            this.lblSearch  = new System.Windows.Forms.Label();
            this.txtSearch  = new System.Windows.Forms.TextBox();
            this.dgv        = new System.Windows.Forms.DataGridView();
            this.colNo      = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName    = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStat    = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPct     = new RG_Project.Project.Forms.DataGridViewProgressBarColumn();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnOk      = new System.Windows.Forms.Button();
            this.btnCancel  = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(30, 58, 95);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock     = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name     = "pnlHeader";
            this.pnlHeader.Size     = new System.Drawing.Size(664, 44);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location  = new System.Drawing.Point(0, 0);
            this.lblTitle.Name      = "lblTitle";
            this.lblTitle.Padding   = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblTitle.Size      = new System.Drawing.Size(664, 44);
            this.lblTitle.TabIndex  = 0;
            this.lblTitle.Text      = "เลือกโครงการที่ ref";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlAccent
            // 
            this.pnlAccent.BackColor = System.Drawing.Color.FromArgb(255, 185, 0);
            this.pnlAccent.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlAccent.Location  = new System.Drawing.Point(0, 44);
            this.pnlAccent.Name      = "pnlAccent";
            this.pnlAccent.Size      = new System.Drawing.Size(664, 3);
            this.pnlAccent.TabIndex  = 1;
            // 
            // pnlSearch
            // 
            this.pnlSearch.BackColor = System.Drawing.Color.FromArgb(229, 236, 248);
            this.pnlSearch.Controls.Add(this.lblSearch);
            this.pnlSearch.Controls.Add(this.txtSearch);
            this.pnlSearch.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Location  = new System.Drawing.Point(0, 47);
            this.pnlSearch.Name      = "pnlSearch";
            this.pnlSearch.Padding   = new System.Windows.Forms.Padding(8, 7, 8, 0);
            this.pnlSearch.Size      = new System.Drawing.Size(664, 40);
            this.pnlSearch.TabIndex  = 2;
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize  = true;
            this.lblSearch.ForeColor = System.Drawing.Color.FromArgb(42, 57, 80);
            this.lblSearch.Location  = new System.Drawing.Point(8, 11);
            this.lblSearch.Name      = "lblSearch";
            this.lblSearch.Size      = new System.Drawing.Size(44, 15);
            this.lblSearch.TabIndex  = 0;
            this.lblSearch.Text      = "ค้นหา:";
            // 
            // txtSearch
            // 
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.Location    = new System.Drawing.Point(60, 8);
            this.txtSearch.Name        = "txtSearch";
            this.txtSearch.Size        = new System.Drawing.Size(300, 23);
            this.txtSearch.TabIndex    = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // dgv
            // 
            this.dgv.AllowUserToAddRows    = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.AutoGenerateColumns   = false;
            this.dgv.BackgroundColor       = System.Drawing.Color.White;
            this.dgv.BorderStyle           = System.Windows.Forms.BorderStyle.None;
            this.dgv.CellBorderStyle       = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgv.ColumnHeadersHeight   = 30;
            this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colNo, this.colName, this.colStat, this.colPct });
            this.dgv.Dock                  = System.Windows.Forms.DockStyle.Fill;
            this.dgv.EnableHeadersVisualStyles = false;
            this.dgv.GridColor             = System.Drawing.Color.FromArgb(208, 220, 240);
            this.dgv.Location              = new System.Drawing.Point(0, 87);
            this.dgv.MultiSelect           = false;
            this.dgv.Name                  = "dgv";
            this.dgv.ReadOnly              = true;
            this.dgv.RowHeadersVisible     = false;
            this.dgv.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv.Size                  = new System.Drawing.Size(664, 334);
            this.dgv.TabIndex              = 3;
            this.dgv.CellDoubleClick      += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellDoubleClick);
            // header style
            this.dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(30, 58, 95);
            this.dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.dgv.ColumnHeadersDefaultCellStyle.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.dgv.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(236, 244, 255);
            this.dgv.RowTemplate.Height = 28;
            // 
            // colNo
            // 
            this.colNo.DataPropertyName = "c_project_no";
            this.colNo.HeaderText       = "เลขที่โครงการ";
            this.colNo.Name             = "colNo";
            this.colNo.Width            = 140;
            // 
            // colName
            // 
            this.colName.AutoSizeMode    = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colName.DataPropertyName = "n_project_name";
            this.colName.HeaderText      = "ชื่อโครงการ";
            this.colName.Name            = "colName";
            // 
            // colStat
            // 
            this.colStat.DataPropertyName = "c_status";
            this.colStat.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.colStat.HeaderText       = "สถานะ";
            this.colStat.Name             = "colStat";
            this.colStat.Width            = 100;
            // 
            // colPct
            // 
            this.colPct.DataPropertyName = "i_pct_overall";
            this.colPct.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.colPct.HeaderText       = "% เสร็จ";
            this.colPct.Name             = "colPct";
            this.colPct.Width            = 90;
            // 
            // pnlButtons
            // 
            this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(229, 236, 248);
            this.pnlButtons.Controls.Add(this.btnOk);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location  = new System.Drawing.Point(0, 421);
            this.pnlButtons.Name      = "pnlButtons";
            this.pnlButtons.Size      = new System.Drawing.Size(664, 48);
            this.pnlButtons.TabIndex  = 4;
            // 
            // btnOk
            // 
            this.btnOk.BackColor = System.Drawing.Color.FromArgb(43, 87, 154);
            this.btnOk.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnOk.FlatAppearance.BorderSize = 0;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.Location  = new System.Drawing.Point(484, 9);
            this.btnOk.Name      = "btnOk";
            this.btnOk.Size      = new System.Drawing.Size(90, 30);
            this.btnOk.TabIndex  = 0;
            this.btnOk.Text      = "เลือก";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click    += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(100, 130, 168);
            this.btnCancel.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location  = new System.Drawing.Point(582, 9);
            this.btnCancel.Name      = "btnCancel";
            this.btnCancel.Size      = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex  = 1;
            this.btnCancel.Text      = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click    += new System.EventHandler(this.btnCancel_Click);
            // 
            // Frm_Select_RefProject
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(242, 246, 252);
            this.ClientSize          = new System.Drawing.Size(664, 469);
            this.Controls.Add(this.dgv);
            this.Controls.Add(this.pnlSearch);
            this.Controls.Add(this.pnlAccent);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlButtons);
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "Frm_Select_RefProject";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "เลือกโครงการที่ ref";
            this.Load           += new System.EventHandler(this.Frm_Select_RefProject_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel                          pnlHeader;
        private System.Windows.Forms.Label                          lblTitle;
        private System.Windows.Forms.Panel                          pnlAccent;
        private System.Windows.Forms.Panel                          pnlSearch;
        private System.Windows.Forms.Label                          lblSearch;
        private System.Windows.Forms.TextBox                        txtSearch;
        private System.Windows.Forms.DataGridView                   dgv;
        private System.Windows.Forms.DataGridViewTextBoxColumn      colNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn      colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn      colStat;
        private RG_Project.Project.Forms.DataGridViewProgressBarColumn colPct;
        private System.Windows.Forms.Panel                          pnlButtons;
        private System.Windows.Forms.Button                         btnOk;
        private System.Windows.Forms.Button                         btnCancel;
    }
}
