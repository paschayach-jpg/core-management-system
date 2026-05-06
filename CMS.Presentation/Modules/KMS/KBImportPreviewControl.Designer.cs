namespace CMS.Presentation.Modules.KMS
{
    partial class KBImportPreviewControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support — do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageGrid = new System.Windows.Forms.TabPage();
            this.dgvPreview = new System.Windows.Forms.DataGridView();
            this.colNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCatName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSummary = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colContentHtml = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPublished = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExpiry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNote = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlGridPager = new System.Windows.Forms.Panel();
            this.lblGridPage = new System.Windows.Forms.Label();
            this.btnGridPrev = new System.Windows.Forms.Button();
            this.btnGridNext = new System.Windows.Forms.Button();
            this.tabPageCard = new System.Windows.Forms.TabPage();
            this.flowCards = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlCardPager = new System.Windows.Forms.Panel();
            this.lblCardPage = new System.Windows.Forms.Label();
            this.btnCardPrev = new System.Windows.Forms.Button();
            this.btnCardNext = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabPageGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).BeginInit();
            this.pnlGridPager.SuspendLayout();
            this.tabPageCard.SuspendLayout();
            this.pnlCardPager.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageGrid);
            this.tabControl.Controls.Add(this.tabPageCard);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.Padding = new System.Drawing.Point(12, 4);
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1597, 480);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageGrid
            // 
            this.tabPageGrid.BackColor = System.Drawing.Color.White;
            this.tabPageGrid.Controls.Add(this.dgvPreview);
            this.tabPageGrid.Controls.Add(this.pnlGridPager);
            this.tabPageGrid.Location = new System.Drawing.Point(4, 31);
            this.tabPageGrid.Name = "tabPageGrid";
            this.tabPageGrid.Size = new System.Drawing.Size(1589, 445);
            this.tabPageGrid.TabIndex = 0;
            this.tabPageGrid.Text = "📋  Grid View";
            // 
            // dgvPreview
            // 
            this.dgvPreview.AllowUserToAddRows = false;
            this.dgvPreview.AllowUserToDeleteRows = false;
            this.dgvPreview.BackgroundColor = System.Drawing.Color.White;
            this.dgvPreview.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvPreview.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPreview.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colNo,
            this.colCode,
            this.colCatName,
            this.colTitle,
            this.colSummary,
            this.colContentHtml,
            this.colTags,
            this.colPublished,
            this.colExpiry,
            this.colStatus,
            this.colNote});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvPreview.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPreview.EnableHeadersVisualStyles = false;
            this.dgvPreview.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.dgvPreview.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.dgvPreview.Location = new System.Drawing.Point(0, 0);
            this.dgvPreview.Name = "dgvPreview";
            this.dgvPreview.ReadOnly = true;
            this.dgvPreview.RowHeadersVisible = false;
            this.dgvPreview.RowHeadersWidth = 51;
            this.dgvPreview.RowTemplate.Height = 26;
            this.dgvPreview.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPreview.Size = new System.Drawing.Size(1589, 409);
            this.dgvPreview.TabIndex = 0;
            // 
            // colNo
            // 
            this.colNo.HeaderText = "#";
            this.colNo.MinimumWidth = 30;
            this.colNo.Name = "colNo";
            this.colNo.ReadOnly = true;
            this.colNo.Width = 40;
            // 
            // colCode
            // 
            this.colCode.HeaderText = "รหัสบทความ";
            this.colCode.MinimumWidth = 40;
            this.colCode.Name = "colCode";
            this.colCode.ReadOnly = true;
            this.colCode.Width = 160;
            // 
            // colCatName
            // 
            this.colCatName.HeaderText = "ชื่อกฎหมาย (C)";
            this.colCatName.MinimumWidth = 40;
            this.colCatName.Name = "colCatName";
            this.colCatName.ReadOnly = true;
            this.colCatName.Width = 150;
            // 
            // colTitle
            // 
            this.colTitle.HeaderText = "ชื่อบทความ (E)";
            this.colTitle.MinimumWidth = 40;
            this.colTitle.Name = "colTitle";
            this.colTitle.ReadOnly = true;
            this.colTitle.Width = 180;
            // 
            // colSummary
            // 
            this.colSummary.HeaderText = "Summary";
            this.colSummary.MinimumWidth = 40;
            this.colSummary.Name = "colSummary";
            this.colSummary.ReadOnly = true;
            this.colSummary.Width = 180;
            // 
            // colContentHtml
            // 
            this.colContentHtml.HeaderText = "ContentHTML (T)";
            this.colContentHtml.MinimumWidth = 40;
            this.colContentHtml.Name = "colContentHtml";
            this.colContentHtml.ReadOnly = true;
            this.colContentHtml.Width = 120;
            // 
            // colTags
            // 
            this.colTags.HeaderText = "Tags";
            this.colTags.MinimumWidth = 40;
            this.colTags.Name = "colTags";
            this.colTags.ReadOnly = true;
            this.colTags.Width = 140;
            // 
            // colPublished
            // 
            this.colPublished.HeaderText = "วันที่เผยแพร่ (R)";
            this.colPublished.MinimumWidth = 40;
            this.colPublished.Name = "colPublished";
            this.colPublished.ReadOnly = true;
            this.colPublished.Width = 110;
            // 
            // colExpiry
            // 
            this.colExpiry.HeaderText = "วันหมดอายุ (S)";
            this.colExpiry.MinimumWidth = 40;
            this.colExpiry.Name = "colExpiry";
            this.colExpiry.ReadOnly = true;
            this.colExpiry.Width = 110;
            // 
            // colStatus
            // 
            this.colStatus.HeaderText = "สถานะ";
            this.colStatus.MinimumWidth = 40;
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            this.colStatus.Width = 80;
            // 
            // colNote
            // 
            this.colNote.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colNote.HeaderText = "หมายเหตุ";
            this.colNote.MinimumWidth = 40;
            this.colNote.Name = "colNote";
            this.colNote.ReadOnly = true;
            // 
            // pnlGridPager
            // 
            this.pnlGridPager.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(253)))));
            this.pnlGridPager.Controls.Add(this.lblGridPage);
            this.pnlGridPager.Controls.Add(this.btnGridPrev);
            this.pnlGridPager.Controls.Add(this.btnGridNext);
            this.pnlGridPager.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlGridPager.Location = new System.Drawing.Point(0, 409);
            this.pnlGridPager.Name = "pnlGridPager";
            this.pnlGridPager.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.pnlGridPager.Size = new System.Drawing.Size(1589, 36);
            this.pnlGridPager.TabIndex = 1;
            // 
            // lblGridPage
            // 
            this.lblGridPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGridPage.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblGridPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblGridPage.Location = new System.Drawing.Point(108, 4);
            this.lblGridPage.Name = "lblGridPage";
            this.lblGridPage.Size = new System.Drawing.Size(1373, 28);
            this.lblGridPage.TabIndex = 1;
            this.lblGridPage.Text = "หน้า 1 / 1  (0 รายการ)";
            this.lblGridPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnGridPrev
            // 
            this.btnGridPrev.BackColor = System.Drawing.Color.White;
            this.btnGridPrev.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGridPrev.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnGridPrev.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnGridPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGridPrev.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnGridPrev.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnGridPrev.Location = new System.Drawing.Point(8, 4);
            this.btnGridPrev.Name = "btnGridPrev";
            this.btnGridPrev.Size = new System.Drawing.Size(100, 28);
            this.btnGridPrev.TabIndex = 0;
            this.btnGridPrev.Text = "◀  ก่อนหน้า";
            this.btnGridPrev.UseVisualStyleBackColor = false;
            this.btnGridPrev.Click += new System.EventHandler(this.btnGridPrev_Click);
            // 
            // btnGridNext
            // 
            this.btnGridNext.BackColor = System.Drawing.Color.White;
            this.btnGridNext.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGridNext.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnGridNext.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnGridNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGridNext.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnGridNext.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnGridNext.Location = new System.Drawing.Point(1481, 4);
            this.btnGridNext.Name = "btnGridNext";
            this.btnGridNext.Size = new System.Drawing.Size(100, 28);
            this.btnGridNext.TabIndex = 2;
            this.btnGridNext.Text = "ถัดไป  ▶";
            this.btnGridNext.UseVisualStyleBackColor = false;
            this.btnGridNext.Click += new System.EventHandler(this.btnGridNext_Click);
            // 
            // tabPageCard
            // 
            this.tabPageCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.tabPageCard.Controls.Add(this.flowCards);
            this.tabPageCard.Controls.Add(this.pnlCardPager);
            this.tabPageCard.Location = new System.Drawing.Point(4, 31);
            this.tabPageCard.Name = "tabPageCard";
            this.tabPageCard.Size = new System.Drawing.Size(1576, 445);
            this.tabPageCard.TabIndex = 1;
            this.tabPageCard.Text = "🃏  Card View";
            // 
            // flowCards
            // 
            this.flowCards.AutoScroll = true;
            this.flowCards.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.flowCards.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowCards.Location = new System.Drawing.Point(0, 0);
            this.flowCards.Name = "flowCards";
            this.flowCards.Padding = new System.Windows.Forms.Padding(8);
            this.flowCards.Size = new System.Drawing.Size(1576, 409);
            this.flowCards.TabIndex = 0;
            // 
            // pnlCardPager
            // 
            this.pnlCardPager.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(253)))));
            this.pnlCardPager.Controls.Add(this.lblCardPage);
            this.pnlCardPager.Controls.Add(this.btnCardPrev);
            this.pnlCardPager.Controls.Add(this.btnCardNext);
            this.pnlCardPager.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlCardPager.Location = new System.Drawing.Point(0, 409);
            this.pnlCardPager.Name = "pnlCardPager";
            this.pnlCardPager.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.pnlCardPager.Size = new System.Drawing.Size(1576, 36);
            this.pnlCardPager.TabIndex = 1;
            // 
            // lblCardPage
            // 
            this.lblCardPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCardPage.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCardPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblCardPage.Location = new System.Drawing.Point(108, 4);
            this.lblCardPage.Name = "lblCardPage";
            this.lblCardPage.Size = new System.Drawing.Size(1360, 28);
            this.lblCardPage.TabIndex = 1;
            this.lblCardPage.Text = "หน้า 1 / 1  (0 รายการ)";
            this.lblCardPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnCardPrev
            // 
            this.btnCardPrev.BackColor = System.Drawing.Color.White;
            this.btnCardPrev.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCardPrev.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnCardPrev.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnCardPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCardPrev.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnCardPrev.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnCardPrev.Location = new System.Drawing.Point(8, 4);
            this.btnCardPrev.Name = "btnCardPrev";
            this.btnCardPrev.Size = new System.Drawing.Size(100, 28);
            this.btnCardPrev.TabIndex = 0;
            this.btnCardPrev.Text = "◀  ก่อนหน้า";
            this.btnCardPrev.UseVisualStyleBackColor = false;
            this.btnCardPrev.Click += new System.EventHandler(this.btnCardPrev_Click);
            // 
            // btnCardNext
            // 
            this.btnCardNext.BackColor = System.Drawing.Color.White;
            this.btnCardNext.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCardNext.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCardNext.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnCardNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCardNext.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnCardNext.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnCardNext.Location = new System.Drawing.Point(1468, 4);
            this.btnCardNext.Name = "btnCardNext";
            this.btnCardNext.Size = new System.Drawing.Size(100, 28);
            this.btnCardNext.TabIndex = 2;
            this.btnCardNext.Text = "ถัดไป  ▶";
            this.btnCardNext.UseVisualStyleBackColor = false;
            this.btnCardNext.Click += new System.EventHandler(this.btnCardNext_Click);
            // 
            // KBImportPreviewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tabControl);
            this.Name = "KBImportPreviewControl";
            this.Size = new System.Drawing.Size(1597, 480);
            this.tabControl.ResumeLayout(false);
            this.tabPageGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).EndInit();
            this.pnlGridPager.ResumeLayout(false);
            this.tabPageCard.ResumeLayout(false);
            this.pnlCardPager.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        // ── Designer-managed fields ──────────────────────────────────────────────
        private System.Windows.Forms.TabControl            tabControl;
        private System.Windows.Forms.TabPage               tabPageGrid;
        private System.Windows.Forms.TabPage               tabPageCard;

        // Grid tab
        private System.Windows.Forms.DataGridView          dgvPreview;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCatName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSummary;
        private System.Windows.Forms.DataGridViewTextBoxColumn colContentHtml;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTags;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPublished;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExpiry;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNote;
        private System.Windows.Forms.Panel                 pnlGridPager;
        private System.Windows.Forms.Button                btnGridPrev;
        private System.Windows.Forms.Button                btnGridNext;
        private System.Windows.Forms.Label                 lblGridPage;

        // Card tab
        private System.Windows.Forms.FlowLayoutPanel       flowCards;
        private System.Windows.Forms.Panel                 pnlCardPager;
        private System.Windows.Forms.Button                btnCardPrev;
        private System.Windows.Forms.Button                btnCardNext;
        private System.Windows.Forms.Label                 lblCardPage;
    }
}
