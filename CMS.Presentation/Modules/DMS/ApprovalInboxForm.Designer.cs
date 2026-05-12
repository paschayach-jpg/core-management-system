namespace CMS.Presentation
{
    partial class ApprovalInboxForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.chkSelectAll = new System.Windows.Forms.CheckBox();
            this.lblSelCount = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.lblRemark = new System.Windows.Forms.Label();
            this.txtRemark = new System.Windows.Forms.TextBox();
            this.btnApprove = new System.Windows.Forms.Button();
            this.btnReject = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lvRequests = new System.Windows.Forms.ListView();
            this.colReqId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSubject = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRequester = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colExpiry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.pnlHeader.Size = new System.Drawing.Size(958, 52);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(12, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(946, 52);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "📋  Approval Inbox — รายการรออนุมัติ";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.pnlToolbar.Controls.Add(this.chkSelectAll);
            this.pnlToolbar.Controls.Add(this.lblSelCount);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 52);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.pnlToolbar.Size = new System.Drawing.Size(958, 32);
            this.pnlToolbar.TabIndex = 3;
            // 
            // chkSelectAll
            // 
            this.chkSelectAll.AutoSize = true;
            this.chkSelectAll.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.chkSelectAll.Location = new System.Drawing.Point(10, 7);
            this.chkSelectAll.Name = "chkSelectAll";
            this.chkSelectAll.Size = new System.Drawing.Size(100, 19);
            this.chkSelectAll.TabIndex = 0;
            this.chkSelectAll.Text = "เลือกทั้งหมด";
            this.chkSelectAll.CheckedChanged += new System.EventHandler(this.chkSelectAll_CheckedChanged);
            // 
            // lblSelCount
            // 
            this.lblSelCount.AutoSize = true;
            this.lblSelCount.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSelCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblSelCount.Location = new System.Drawing.Point(130, 9);
            this.lblSelCount.Name = "lblSelCount";
            this.lblSelCount.Size = new System.Drawing.Size(80, 15);
            this.lblSelCount.TabIndex = 1;
            this.lblSelCount.Text = "เลือกแล้ว 0 รายการ";
            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this.pnlBottom.Controls.Add(this.lblRemark);
            this.pnlBottom.Controls.Add(this.txtRemark);
            this.pnlBottom.Controls.Add(this.btnApprove);
            this.pnlBottom.Controls.Add(this.btnReject);
            this.pnlBottom.Controls.Add(this.btnRefresh);
            this.pnlBottom.Controls.Add(this.lblStatus);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 490);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Padding = new System.Windows.Forms.Padding(10, 8, 10, 6);
            this.pnlBottom.Size = new System.Drawing.Size(958, 110);
            this.pnlBottom.TabIndex = 1;
            // 
            // lblRemark
            // 
            this.lblRemark.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblRemark.Location = new System.Drawing.Point(10, 8);
            this.lblRemark.Name = "lblRemark";
            this.lblRemark.Size = new System.Drawing.Size(70, 18);
            this.lblRemark.TabIndex = 0;
            this.lblRemark.Text = "หมายเหตุ:";
            // 
            // txtRemark
            // 
            this.txtRemark.BackColor = System.Drawing.Color.White;
            this.txtRemark.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.txtRemark.Location = new System.Drawing.Point(10, 28);
            this.txtRemark.Multiline = true;
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRemark.Size = new System.Drawing.Size(440, 64);
            this.txtRemark.TabIndex = 1;
            this.txtRemark.TextChanged += new System.EventHandler(this.txtRemark_TextChanged);
            //
            // btnApprove
            // 
            this.btnApprove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(140)))), ((int)(((byte)(0)))));
            this.btnApprove.Enabled = false;
            this.btnApprove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApprove.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnApprove.ForeColor = System.Drawing.Color.White;
            this.btnApprove.Location = new System.Drawing.Point(462, 28);
            this.btnApprove.Name = "btnApprove";
            this.btnApprove.Size = new System.Drawing.Size(110, 32);
            this.btnApprove.TabIndex = 2;
            this.btnApprove.Text = "✅  อนุมัติ";
            this.btnApprove.UseVisualStyleBackColor = false;
            this.btnApprove.Click += new System.EventHandler(this.btnApprove_Click);
            // 
            // btnReject
            // 
            this.btnReject.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnReject.Enabled = false;
            this.btnReject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReject.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnReject.ForeColor = System.Drawing.Color.White;
            this.btnReject.Location = new System.Drawing.Point(582, 28);
            this.btnReject.Name = "btnReject";
            this.btnReject.Size = new System.Drawing.Size(110, 32);
            this.btnReject.TabIndex = 3;
            this.btnReject.Text = "❌  ปฏิเสธ";
            this.btnReject.UseVisualStyleBackColor = false;
            this.btnReject.Click += new System.EventHandler(this.btnReject_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location = new System.Drawing.Point(702, 28);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(110, 32);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "🔄  รีเฟรช";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblStatus.Location = new System.Drawing.Point(10, 96);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(800, 18);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "พร้อมใช้งาน";
            // 
            // lvRequests
            // 
            this.lvRequests.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvRequests.BackColor = System.Drawing.Color.White;
            this.lvRequests.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvRequests.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colReqId,
            this.colType,
            this.colSubject,
            this.colRequester,
            this.colDate,
            this.colExpiry});
            this.lvRequests.CheckBoxes = true;
            this.lvRequests.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lvRequests.FullRowSelect = true;
            this.lvRequests.GridLines = true;
            this.lvRequests.HideSelection = false;
            this.lvRequests.Location = new System.Drawing.Point(0, 84);
            this.lvRequests.MultiSelect = true;
            this.lvRequests.Name = "lvRequests";
            this.lvRequests.Size = new System.Drawing.Size(958, 400);
            this.lvRequests.TabIndex = 2;
            this.lvRequests.UseCompatibleStateImageBehavior = false;
            this.lvRequests.View = System.Windows.Forms.View.Details;
            this.lvRequests.SelectedIndexChanged += new System.EventHandler(this.lvRequests_SelectedIndexChanged);
            this.lvRequests.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvRequests_ItemChecked);
            // 
            // colReqId
            // 
            this.colReqId.Text = "Request ID";
            this.colReqId.Width = 90;
            // 
            // colType
            // 
            this.colType.Text = "ประเภท";
            this.colType.Width = 140;
            // 
            // colSubject
            // 
            this.colSubject.Text = "เรื่อง";
            this.colSubject.Width = 320;
            // 
            // colRequester
            // 
            this.colRequester.Text = "ผู้ขอ";
            this.colRequester.Width = 160;
            // 
            // colDate
            // 
            this.colDate.Text = "วันที่ขอ";
            this.colDate.Width = 130;
            // 
            // colExpiry
            // 
            this.colExpiry.Text = "หมดอายุ";
            this.colExpiry.Width = 100;
            // 
            // ApprovalInboxForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(958, 600);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.lvRequests);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(820, 500);
            this.Name = "ApprovalInboxForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Approval Inbox — รายการรออนุมัติ";
            this.pnlHeader.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        // ── Field declarations ────────────────────────────────────────────────
        private System.Windows.Forms.Panel      pnlHeader;
        private System.Windows.Forms.Label      lblTitle;

        private System.Windows.Forms.Panel      pnlToolbar;
        private System.Windows.Forms.CheckBox   chkSelectAll;
        private System.Windows.Forms.Label      lblSelCount;

        private System.Windows.Forms.Panel      pnlBottom;
        private System.Windows.Forms.Label      lblRemark;
        private System.Windows.Forms.TextBox    txtRemark;
        private System.Windows.Forms.Button     btnApprove;
        private System.Windows.Forms.Button     btnReject;
        private System.Windows.Forms.Button     btnRefresh;
        private System.Windows.Forms.Label      lblStatus;

        private System.Windows.Forms.ListView   lvRequests;
        private System.Windows.Forms.ColumnHeader colReqId;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colSubject;
        private System.Windows.Forms.ColumnHeader colRequester;
        private System.Windows.Forms.ColumnHeader colDate;
        private System.Windows.Forms.ColumnHeader colExpiry;
    }
}
