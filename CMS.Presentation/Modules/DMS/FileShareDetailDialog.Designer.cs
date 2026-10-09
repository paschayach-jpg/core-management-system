namespace CMS.Presentation.Modules.DMS
{
    partial class FileShareDetailDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle headerStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle cellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle altStyle = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSub = new System.Windows.Forms.Label();
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.lblResourceNameTitle = new System.Windows.Forms.Label();
            this.lblResourceName = new System.Windows.Forms.Label();
            this.lblSharerTitle = new System.Windows.Forms.Label();
            this.lblSharer = new System.Windows.Forms.Label();
            this.lblRecipientCountTitle = new System.Windows.Forms.Label();
            this.lblRecipientCount = new System.Windows.Forms.Label();
            this.pnlGrid = new System.Windows.Forms.Panel();
            this.dgvRecipients = new System.Windows.Forms.DataGridView();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRights = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExpiry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNote = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlActionBar = new System.Windows.Forms.Panel();
            this.btnAddRecipient = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnRevoke = new System.Windows.Forms.Button();
            this.btnWithdraw = new System.Windows.Forms.Button();
            this.lblHint = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.pnlInfo.SuspendLayout();
            this.pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipients)).BeginInit();
            this.pnlActionBar.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSub);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(860, 60);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(16, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(262, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "📋  รายละเอียดผู้รับการแชร์ (Share Details)";
            // 
            // lblSub
            // 
            this.lblSub.AutoSize = true;
            this.lblSub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(210)))), ((int)(((byte)(245)))));
            this.lblSub.Location = new System.Drawing.Point(17, 34);
            this.lblSub.Name = "lblSub";
            this.lblSub.Size = new System.Drawing.Size(248, 15);
            this.lblSub.TabIndex = 1;
            this.lblSub.Text = "รายการผู้รับสิทธิ์และสถานะการแชร์สำหรับรายการนี้";
            // 
            // pnlInfo
            // 
            this.pnlInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(242)))), ((int)(((byte)(252)))));
            this.pnlInfo.Controls.Add(this.lblResourceNameTitle);
            this.pnlInfo.Controls.Add(this.lblResourceName);
            this.pnlInfo.Controls.Add(this.lblSharerTitle);
            this.pnlInfo.Controls.Add(this.lblSharer);
            this.pnlInfo.Controls.Add(this.lblRecipientCountTitle);
            this.pnlInfo.Controls.Add(this.lblRecipientCount);
            this.pnlInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlInfo.Location = new System.Drawing.Point(0, 60);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(860, 48);
            this.pnlInfo.TabIndex = 1;
            this.pnlInfo.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlInfo_Paint);
            // 
            // lblResourceNameTitle
            // 
            this.lblResourceNameTitle.AutoSize = true;
            this.lblResourceNameTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblResourceNameTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(70)))), ((int)(((byte)(100)))));
            this.lblResourceNameTitle.Location = new System.Drawing.Point(16, 16);
            this.lblResourceNameTitle.Name = "lblResourceNameTitle";
            this.lblResourceNameTitle.Size = new System.Drawing.Size(73, 15);
            this.lblResourceNameTitle.TabIndex = 0;
            this.lblResourceNameTitle.Text = "ชื่อไฟล์/โฟลเดอร์:";
            // 
            // lblResourceName
            // 
            this.lblResourceName.AutoEllipsis = true;
            this.lblResourceName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblResourceName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.lblResourceName.Location = new System.Drawing.Point(95, 16);
            this.lblResourceName.Name = "lblResourceName";
            this.lblResourceName.Size = new System.Drawing.Size(320, 18);
            this.lblResourceName.TabIndex = 1;
            this.lblResourceName.Text = "-";
            // 
            // lblSharerTitle
            // 
            this.lblSharerTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSharerTitle.AutoSize = true;
            this.lblSharerTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSharerTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(100)))), ((int)(((byte)(120)))));
            this.lblSharerTitle.Location = new System.Drawing.Point(440, 16);
            this.lblSharerTitle.Name = "lblSharerTitle";
            this.lblSharerTitle.Size = new System.Drawing.Size(47, 15);
            this.lblSharerTitle.TabIndex = 2;
            this.lblSharerTitle.Text = "ผู้แชร์ :";
            // 
            // lblSharer
            // 
            this.lblSharer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSharer.AutoEllipsis = true;
            this.lblSharer.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSharer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(80)))), ((int)(((byte)(95)))));
            this.lblSharer.Location = new System.Drawing.Point(490, 16);
            this.lblSharer.Name = "lblSharer";
            this.lblSharer.Size = new System.Drawing.Size(170, 18);
            this.lblSharer.TabIndex = 3;
            this.lblSharer.Text = "-";
            // 
            // lblRecipientCountTitle
            // 
            this.lblRecipientCountTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRecipientCountTitle.AutoSize = true;
            this.lblRecipientCountTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblRecipientCountTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(100)))), ((int)(((byte)(120)))));
            this.lblRecipientCountTitle.Location = new System.Drawing.Point(680, 16);
            this.lblRecipientCountTitle.Name = "lblRecipientCountTitle";
            this.lblRecipientCountTitle.Size = new System.Drawing.Size(73, 15);
            this.lblRecipientCountTitle.TabIndex = 4;
            this.lblRecipientCountTitle.Text = "จำนวนผู้รับ :";
            // 
            // lblRecipientCount
            // 
            this.lblRecipientCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRecipientCount.AutoEllipsis = true;
            this.lblRecipientCount.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblRecipientCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.lblRecipientCount.Location = new System.Drawing.Point(755, 16);
            this.lblRecipientCount.Name = "lblRecipientCount";
            this.lblRecipientCount.Size = new System.Drawing.Size(90, 18);
            this.lblRecipientCount.TabIndex = 5;
            this.lblRecipientCount.Text = "0 รายการ";
            // 
            // pnlActionBar
            // 
            this.pnlActionBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(253)))));
            this.pnlActionBar.Controls.Add(this.btnAddRecipient);
            this.pnlActionBar.Controls.Add(this.btnEdit);
            this.pnlActionBar.Controls.Add(this.btnRevoke);
            this.pnlActionBar.Controls.Add(this.btnWithdraw);
            this.pnlActionBar.Controls.Add(this.lblHint);
            this.pnlActionBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlActionBar.Location = new System.Drawing.Point(0, 108);
            this.pnlActionBar.Name = "pnlActionBar";
            this.pnlActionBar.Size = new System.Drawing.Size(860, 42);
            this.pnlActionBar.TabIndex = 2;
            // 
            // btnAddRecipient
            // 
            this.btnAddRecipient.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.btnAddRecipient.FlatAppearance.BorderSize = 0;
            this.btnAddRecipient.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddRecipient.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnAddRecipient.ForeColor = System.Drawing.Color.White;
            this.btnAddRecipient.Location = new System.Drawing.Point(12, 6);
            this.btnAddRecipient.Name = "btnAddRecipient";
            this.btnAddRecipient.Size = new System.Drawing.Size(125, 30);
            this.btnAddRecipient.TabIndex = 0;
            this.btnAddRecipient.Text = "➕  เพิ่มผู้รับสิทธิ์";
            this.btnAddRecipient.UseVisualStyleBackColor = false;
            this.btnAddRecipient.Click += new System.EventHandler(this.BtnAddRecipient_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.btnEdit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(235)))));
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnEdit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.btnEdit.Location = new System.Drawing.Point(144, 6);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(110, 30);
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Text = "⚙️  ขอปรับสิทธิ์";
            this.btnEdit.UseVisualStyleBackColor = false;
            this.btnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
            // 
            // btnRevoke
            // 
            this.btnRevoke.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.btnRevoke.FlatAppearance.BorderSize = 0;
            this.btnRevoke.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRevoke.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnRevoke.ForeColor = System.Drawing.Color.White;
            this.btnRevoke.Location = new System.Drawing.Point(260, 6);
            this.btnRevoke.Name = "btnRevoke";
            this.btnRevoke.Size = new System.Drawing.Size(120, 30);
            this.btnRevoke.TabIndex = 2;
            this.btnRevoke.Text = "🚫  ยกเลิกการแชร์";
            this.btnRevoke.UseVisualStyleBackColor = false;
            this.btnRevoke.Click += new System.EventHandler(this.BtnRevoke_Click);
            // 
            // btnWithdraw
            // 
            this.btnWithdraw.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(228)))), ((int)(((byte)(245)))));
            this.btnWithdraw.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(235)))));
            this.btnWithdraw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWithdraw.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnWithdraw.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.btnWithdraw.Location = new System.Drawing.Point(386, 6);
            this.btnWithdraw.Name = "btnWithdraw";
            this.btnWithdraw.Size = new System.Drawing.Size(100, 30);
            this.btnWithdraw.TabIndex = 3;
            this.btnWithdraw.Text = "↩️  ถอนคำขอ";
            this.btnWithdraw.UseVisualStyleBackColor = false;
            this.btnWithdraw.Click += new System.EventHandler(this.BtnWithdraw_Click);
            // 
            // lblHint
            // 
            this.lblHint.AutoSize = true;
            this.lblHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(110)))), ((int)(((byte)(130)))));
            this.lblHint.Location = new System.Drawing.Point(495, 14);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(262, 13);
            this.lblHint.TabIndex = 4;
            this.lblHint.Text = "💡 สามารถเลือกหลายรายการพร้อมกันได้ (Ctrl/Shift + คลิก)";
            // 
            // pnlGrid
            // 
            this.pnlGrid.Controls.Add(this.dgvRecipients);
            this.pnlGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGrid.Location = new System.Drawing.Point(0, 150);
            this.pnlGrid.Name = "pnlGrid";
            this.pnlGrid.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this.pnlGrid.Size = new System.Drawing.Size(860, 270);
            this.pnlGrid.TabIndex = 3;
            // 
            // 
            // dgvRecipients
            // 
            this.dgvRecipients.AllowUserToAddRows = false;
            this.dgvRecipients.AllowUserToDeleteRows = false;
            this.dgvRecipients.AllowUserToResizeColumns = true;
            this.dgvRecipients.AllowUserToResizeRows = false;
            this.dgvRecipients.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;
            this.dgvRecipients.BackgroundColor = System.Drawing.Color.White;
            this.dgvRecipients.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvRecipients.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvRecipients.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            headerStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            headerStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            headerStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            headerStyle.ForeColor = System.Drawing.Color.White;
            headerStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            headerStyle.SelectionForeColor = System.Drawing.Color.White;
            headerStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRecipients.ColumnHeadersDefaultCellStyle = headerStyle;
            this.dgvRecipients.ColumnHeadersHeight = 32;
            this.dgvRecipients.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvRecipients.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colType,
            this.colName,
            this.colRights,
            this.colExpiry,
            this.colStatus,
            this.colDate,
            this.colNote});
            cellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            cellStyle.BackColor = System.Drawing.Color.White;
            cellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            cellStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            cellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(235)))), ((int)(((byte)(252)))));
            cellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(40)))), ((int)(((byte)(80)))));
            cellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvRecipients.DefaultCellStyle = cellStyle;
            this.dgvRecipients.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRecipients.EnableHeadersVisualStyles = false;
            this.dgvRecipients.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(235)))), ((int)(((byte)(245)))));
            this.dgvRecipients.Location = new System.Drawing.Point(12, 4);
            this.dgvRecipients.Name = "dgvRecipients";
            this.dgvRecipients.ReadOnly = true;
            this.dgvRecipients.RowHeadersVisible = false;
            this.dgvRecipients.RowTemplate.Height = 28;
            this.dgvRecipients.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.dgvRecipients.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRecipients.Size = new System.Drawing.Size(836, 262);
            this.dgvRecipients.TabIndex = 0;
            // 
            // colType
            // 
            this.colType.HeaderText = "ประเภท";
            this.colType.MinimumWidth = 60;
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            this.colType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colType.Width = 90;
            // 
            // colName
            // 
            this.colName.HeaderText = "ผู้รับการแชร์";
            this.colName.MinimumWidth = 100;
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            this.colName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colName.Width = 200;
            // 
            // colRights
            // 
            this.colRights.HeaderText = "สิทธิ์";
            this.colRights.MinimumWidth = 70;
            this.colRights.Name = "colRights";
            this.colRights.ReadOnly = true;
            this.colRights.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colRights.Width = 130;
            // 
            // colExpiry
            // 
            this.colExpiry.HeaderText = "วันหมดอายุ";
            this.colExpiry.MinimumWidth = 80;
            this.colExpiry.Name = "colExpiry";
            this.colExpiry.ReadOnly = true;
            this.colExpiry.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colExpiry.Width = 100;
            // 
            // colStatus
            // 
            this.colStatus.HeaderText = "สถานะ";
            this.colStatus.MinimumWidth = 70;
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            this.colStatus.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colStatus.Width = 110;
            // 
            // colDate
            // 
            this.colDate.HeaderText = "วันที่แชร์/ขอ";
            this.colDate.MinimumWidth = 80;
            this.colDate.Name = "colDate";
            this.colDate.ReadOnly = true;
            this.colDate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colDate.Width = 120;
            // 
            // colNote
            // 
            this.colNote.HeaderText = "หมายเหตุ";
            this.colNote.MinimumWidth = 80;
            this.colNote.Name = "colNote";
            this.colNote.ReadOnly = true;
            this.colNote.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colNote.Width = 180;
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(234)))), ((int)(((byte)(245)))));
            this.pnlFooter.Controls.Add(this.btnClose);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 420);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(860, 48);
            this.pnlFooter.TabIndex = 4;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(55)))), ((int)(((byte)(105)))));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(760, 8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 32);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "ปิด";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // FileShareDetailDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(253)))));
            this.ClientSize = new System.Drawing.Size(860, 468);
            this.Controls.Add(this.pnlGrid);
            this.Controls.Add(this.pnlActionBar);
            this.Controls.Add(this.pnlInfo);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FileShareDetailDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "📋 รายละเอียดการแชร์ (Share Details)";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlInfo.ResumeLayout(false);
            this.pnlInfo.PerformLayout();
            this.pnlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipients)).EndInit();
            this.pnlActionBar.ResumeLayout(false);
            this.pnlActionBar.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.Panel pnlInfo;
        private System.Windows.Forms.Label lblResourceNameTitle;
        private System.Windows.Forms.Label lblResourceName;
        private System.Windows.Forms.Label lblSharerTitle;
        private System.Windows.Forms.Label lblSharer;
        private System.Windows.Forms.Label lblRecipientCountTitle;
        private System.Windows.Forms.Label lblRecipientCount;
        private System.Windows.Forms.Panel pnlActionBar;
        private System.Windows.Forms.Button btnAddRecipient;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnRevoke;
        private System.Windows.Forms.Button btnWithdraw;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Panel pnlGrid;
        private System.Windows.Forms.DataGridView dgvRecipients;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRights;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExpiry;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNote;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Button btnClose;
    }
}
