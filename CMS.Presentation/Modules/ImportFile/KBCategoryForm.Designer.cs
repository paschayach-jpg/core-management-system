namespace CMS.Presentation
{
    partial class KBCategoryForm
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
            this.components = new System.ComponentModel.Container();

            // ── Controls declaration ────────────────────────────────────────
            this.pnlHeader       = new System.Windows.Forms.Panel();
            this.lblHeader       = new System.Windows.Forms.Label();

            this.pnlMain         = new System.Windows.Forms.Panel();
            this.pnlLeft         = new System.Windows.Forms.Panel();
            this.pnlLeftHeader   = new System.Windows.Forms.Panel();
            this.lblListTitle    = new System.Windows.Forms.Label();
            this.btnNew          = new System.Windows.Forms.Button();
            this.lstCategories   = new System.Windows.Forms.ListView();
            this.colCode         = new System.Windows.Forms.ColumnHeader();
            this.colName         = new System.Windows.Forms.ColumnHeader();
            this.colParent       = new System.Windows.Forms.ColumnHeader();
            this.colSort         = new System.Windows.Forms.ColumnHeader();
            this.colStatus       = new System.Windows.Forms.ColumnHeader();

            this.divider         = new System.Windows.Forms.Panel();

            this.pnlRight        = new System.Windows.Forms.Panel();
            this.lblFormTitle    = new System.Windows.Forms.Label();
            this.pnlFormLine     = new System.Windows.Forms.Panel();

            this.lblCode         = new System.Windows.Forms.Label();
            this.txtCode         = new System.Windows.Forms.TextBox();
            this.lblName         = new System.Windows.Forms.Label();
            this.txtName         = new System.Windows.Forms.TextBox();
            this.lblParent       = new System.Windows.Forms.Label();
            this.cboParent       = new System.Windows.Forms.ComboBox();
            this.lblIcon         = new System.Windows.Forms.Label();
            this.txtIcon         = new System.Windows.Forms.TextBox();
            this.lblIconHint     = new System.Windows.Forms.Label();
            this.lblDescription  = new System.Windows.Forms.Label();
            this.txtDescription  = new System.Windows.Forms.TextBox();
            this.lblSortOrder    = new System.Windows.Forms.Label();
            this.numSortOrder    = new System.Windows.Forms.NumericUpDown();

            this.pnlButtons      = new System.Windows.Forms.Panel();
            this.btnSave         = new System.Windows.Forms.Button();
            this.btnDelete       = new System.Windows.Forms.Button();
            this.btnClear        = new System.Windows.Forms.Button();

            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.pnlLeftHeader.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.numSortOrder).BeginInit();
            this.SuspendLayout();

            // ════════════════════════════════════════════════════════════════
            // pnlHeader
            // ════════════════════════════════════════════════════════════════
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 52;
            this.pnlHeader.Controls.Add(this.lblHeader);

            this.lblHeader.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Text      = "  📚 ตั้งค่า Category — Knowledge Base (MOD-006)";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ════════════════════════════════════════════════════════════════
            // pnlMain  (Fill)
            // ════════════════════════════════════════════════════════════════
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Controls.Add(this.pnlRight);
            this.pnlMain.Controls.Add(this.divider);
            this.pnlMain.Controls.Add(this.pnlLeft);

            // ── pnlLeft (200px) ─────────────────────────────────────────────
            this.pnlLeft.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Width     = 380;
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlLeft.Controls.Add(this.lstCategories);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);

            this.pnlLeftHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Height    = 44;
            this.pnlLeftHeader.BackColor = System.Drawing.Color.FromArgb(235, 240, 248);
            this.pnlLeftHeader.Controls.Add(this.btnNew);
            this.pnlLeftHeader.Controls.Add(this.lblListTitle);

            this.lblListTitle.AutoSize  = false;
            this.lblListTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblListTitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblListTitle.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.lblListTitle.Text      = "  รายการ Category";
            this.lblListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.btnNew.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnNew.Width     = 100;
            this.btnNew.Text      = "+ เพิ่มใหม่";
            this.btnNew.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnNew.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnNew.ForeColor = System.Drawing.Color.White;
            this.btnNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNew.FlatAppearance.BorderSize = 0;
            this.btnNew.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnNew.Click    += new System.EventHandler(this.BtnNew_Click);

            // ListView
            this.lstCategories.Dock          = System.Windows.Forms.DockStyle.Fill;
            this.lstCategories.View          = System.Windows.Forms.View.Details;
            this.lstCategories.FullRowSelect = true;
            this.lstCategories.GridLines     = false;
            this.lstCategories.HeaderStyle   = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstCategories.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.lstCategories.BorderStyle   = System.Windows.Forms.BorderStyle.None;
            this.lstCategories.MultiSelect   = false;
            this.lstCategories.SelectedIndexChanged += new System.EventHandler(this.LstCategories_SelectedIndexChanged);

            this.colCode.Text  = "รหัส";
            this.colCode.Width = 70;
            this.colName.Text  = "ชื่อ Category";
            this.colName.Width = 140;
            this.colParent.Text  = "Parent";
            this.colParent.Width = 90;
            this.colSort.Text  = "ลำดับ";
            this.colSort.Width = 45;
            this.colStatus.Text  = "สถานะ";
            this.colStatus.Width = 55;

            this.lstCategories.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
            {
                this.colCode, this.colName, this.colParent, this.colSort, this.colStatus
            });

            // ── divider ─────────────────────────────────────────────────────
            this.divider.Dock      = System.Windows.Forms.DockStyle.Left;
            this.divider.Width     = 1;
            this.divider.BackColor = System.Drawing.Color.FromArgb(200, 210, 225);

            // ── pnlRight (Fill) ─────────────────────────────────────────────
            this.pnlRight.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Padding   = new System.Windows.Forms.Padding(20, 16, 20, 8);
            this.pnlRight.BackColor = System.Drawing.Color.White;
            this.pnlRight.Controls.Add(this.pnlButtons);
            this.pnlRight.Controls.Add(this.numSortOrder);
            this.pnlRight.Controls.Add(this.lblSortOrder);
            this.pnlRight.Controls.Add(this.txtDescription);
            this.pnlRight.Controls.Add(this.lblDescription);
            this.pnlRight.Controls.Add(this.lblIconHint);
            this.pnlRight.Controls.Add(this.txtIcon);
            this.pnlRight.Controls.Add(this.lblIcon);
            this.pnlRight.Controls.Add(this.cboParent);
            this.pnlRight.Controls.Add(this.lblParent);
            this.pnlRight.Controls.Add(this.txtName);
            this.pnlRight.Controls.Add(this.lblName);
            this.pnlRight.Controls.Add(this.txtCode);
            this.pnlRight.Controls.Add(this.lblCode);
            this.pnlRight.Controls.Add(this.pnlFormLine);
            this.pnlRight.Controls.Add(this.lblFormTitle);

            // ── Form title ──────────────────────────────────────────────────
            this.lblFormTitle.AutoSize  = false;
            this.lblFormTitle.Location  = new System.Drawing.Point(20, 16);
            this.lblFormTitle.Size      = new System.Drawing.Size(400, 28);
            this.lblFormTitle.Font      = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblFormTitle.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.lblFormTitle.Text      = "รายละเอียด Category";

            this.pnlFormLine.Location  = new System.Drawing.Point(20, 46);
            this.pnlFormLine.Size      = new System.Drawing.Size(500, 1);
            this.pnlFormLine.BackColor = System.Drawing.Color.FromArgb(200, 210, 225);

            // ── CategoryCode ────────────────────────────────────────────────
            this.lblCode.AutoSize  = false;
            this.lblCode.Location  = new System.Drawing.Point(20, 62);
            this.lblCode.Size      = new System.Drawing.Size(130, 22);
            this.lblCode.Text      = "รหัส Category *";
            this.lblCode.Font      = new System.Drawing.Font("Segoe UI", 9F);

            this.txtCode.Location  = new System.Drawing.Point(160, 60);
            this.txtCode.Size      = new System.Drawing.Size(200, 24);
            this.txtCode.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.txtCode.MaxLength = 20;

            // ── CategoryName ────────────────────────────────────────────────
            this.lblName.AutoSize  = false;
            this.lblName.Location  = new System.Drawing.Point(20, 98);
            this.lblName.Size      = new System.Drawing.Size(130, 22);
            this.lblName.Text      = "ชื่อ Category *";
            this.lblName.Font      = new System.Drawing.Font("Segoe UI", 9F);

            this.txtName.Location  = new System.Drawing.Point(160, 96);
            this.txtName.Size      = new System.Drawing.Size(320, 24);
            this.txtName.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.txtName.MaxLength = 100;

            // ── Parent Category ─────────────────────────────────────────────
            this.lblParent.AutoSize  = false;
            this.lblParent.Location  = new System.Drawing.Point(20, 136);
            this.lblParent.Size      = new System.Drawing.Size(130, 22);
            this.lblParent.Text      = "Parent Category";
            this.lblParent.Font      = new System.Drawing.Font("Segoe UI", 9F);

            this.cboParent.Location      = new System.Drawing.Point(160, 133);
            this.cboParent.Size          = new System.Drawing.Size(280, 24);
            this.cboParent.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.cboParent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            // ── Icon ────────────────────────────────────────────────────────
            this.lblIcon.AutoSize  = false;
            this.lblIcon.Location  = new System.Drawing.Point(20, 172);
            this.lblIcon.Size      = new System.Drawing.Size(130, 22);
            this.lblIcon.Text      = "Icon (fa-*)";
            this.lblIcon.Font      = new System.Drawing.Font("Segoe UI", 9F);

            this.txtIcon.Location  = new System.Drawing.Point(160, 170);
            this.txtIcon.Size      = new System.Drawing.Size(160, 24);
            this.txtIcon.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.txtIcon.MaxLength = 50;

            this.lblIconHint.AutoSize  = false;
            this.lblIconHint.Location  = new System.Drawing.Point(328, 174);
            this.lblIconHint.Size      = new System.Drawing.Size(200, 18);
            this.lblIconHint.Text      = "เช่น fa-book, fa-laptop";
            this.lblIconHint.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblIconHint.ForeColor = System.Drawing.Color.Gray;

            // ── Description ─────────────────────────────────────────────────
            this.lblDescription.AutoSize  = false;
            this.lblDescription.Location  = new System.Drawing.Point(20, 208);
            this.lblDescription.Size      = new System.Drawing.Size(130, 22);
            this.lblDescription.Text      = "คำอธิบาย";
            this.lblDescription.Font      = new System.Drawing.Font("Segoe UI", 9F);

            this.txtDescription.Location   = new System.Drawing.Point(160, 206);
            this.txtDescription.Size       = new System.Drawing.Size(360, 72);
            this.txtDescription.Font       = new System.Drawing.Font("Segoe UI", 9F);
            this.txtDescription.Multiline  = true;
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.MaxLength  = 300;

            // ── SortOrder ───────────────────────────────────────────────────
            this.lblSortOrder.AutoSize  = false;
            this.lblSortOrder.Location  = new System.Drawing.Point(20, 292);
            this.lblSortOrder.Size      = new System.Drawing.Size(130, 22);
            this.lblSortOrder.Text      = "ลำดับการแสดงผล";
            this.lblSortOrder.Font      = new System.Drawing.Font("Segoe UI", 9F);

            this.numSortOrder.Location = new System.Drawing.Point(160, 290);
            this.numSortOrder.Size     = new System.Drawing.Size(80, 24);
            this.numSortOrder.Font     = new System.Drawing.Font("Segoe UI", 9F);
            this.numSortOrder.Minimum  = 0;
            this.numSortOrder.Maximum  = 9999;

            // ── pnlButtons ──────────────────────────────────────────────────
            this.pnlButtons.Location  = new System.Drawing.Point(20, 334);
            this.pnlButtons.Size      = new System.Drawing.Size(520, 44);
            this.pnlButtons.BackColor = System.Drawing.Color.White;
            this.pnlButtons.Controls.Add(this.btnClear);
            this.pnlButtons.Controls.Add(this.btnDelete);
            this.pnlButtons.Controls.Add(this.btnSave);

            this.btnSave.Location  = new System.Drawing.Point(0, 6);
            this.btnSave.Size      = new System.Drawing.Size(110, 32);
            this.btnSave.Text      = "💾 บันทึก";
            this.btnSave.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnSave.Click    += new System.EventHandler(this.BtnSave_Click);

            this.btnDelete.Location  = new System.Drawing.Point(120, 6);
            this.btnDelete.Size      = new System.Drawing.Size(110, 32);
            this.btnDelete.Text      = "🗑️ ปิดใช้งาน";
            this.btnDelete.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(200, 50, 50);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.Enabled   = false;
            this.btnDelete.Click    += new System.EventHandler(this.BtnDelete_Click);

            this.btnClear.Location  = new System.Drawing.Point(240, 6);
            this.btnClear.Size      = new System.Drawing.Size(90, 32);
            this.btnClear.Text      = "ล้างค่า";
            this.btnClear.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnClear.BackColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.btnClear.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.FlatAppearance.BorderSize = 0;
            this.btnClear.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnClear.Click    += new System.EventHandler(this.BtnClear_Click);

            // ════════════════════════════════════════════════════════════════
            // Form
            // ════════════════════════════════════════════════════════════════
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.White;
            this.ClientSize          = new System.Drawing.Size(900, 540);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);
            this.Font                = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize         = new System.Drawing.Size(800, 460);
            this.Name                = "KBCategoryForm";
            this.Text                = "ตั้งค่า Category — Knowledge Base";
            this.Load               += new System.EventHandler(this.KBCategoryForm_Load);

            this.pnlHeader.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeftHeader.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            this.pnlRight.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.numSortOrder).EndInit();
            this.ResumeLayout(false);
        }

        // ── Control declarations ─────────────────────────────────────────────
        private System.Windows.Forms.Panel         pnlHeader;
        private System.Windows.Forms.Label         lblHeader;
        private System.Windows.Forms.Panel         pnlMain;
        private System.Windows.Forms.Panel         pnlLeft;
        private System.Windows.Forms.Panel         pnlLeftHeader;
        private System.Windows.Forms.Label         lblListTitle;
        private System.Windows.Forms.Button        btnNew;
        private System.Windows.Forms.ListView      lstCategories;
        private System.Windows.Forms.ColumnHeader  colCode;
        private System.Windows.Forms.ColumnHeader  colName;
        private System.Windows.Forms.ColumnHeader  colParent;
        private System.Windows.Forms.ColumnHeader  colSort;
        private System.Windows.Forms.ColumnHeader  colStatus;
        private System.Windows.Forms.Panel         divider;
        private System.Windows.Forms.Panel         pnlRight;
        private System.Windows.Forms.Label         lblFormTitle;
        private System.Windows.Forms.Panel         pnlFormLine;
        private System.Windows.Forms.Label         lblCode;
        private System.Windows.Forms.TextBox       txtCode;
        private System.Windows.Forms.Label         lblName;
        private System.Windows.Forms.TextBox       txtName;
        private System.Windows.Forms.Label         lblParent;
        private System.Windows.Forms.ComboBox      cboParent;
        private System.Windows.Forms.Label         lblIcon;
        private System.Windows.Forms.TextBox       txtIcon;
        private System.Windows.Forms.Label         lblIconHint;
        private System.Windows.Forms.Label         lblDescription;
        private System.Windows.Forms.TextBox       txtDescription;
        private System.Windows.Forms.Label         lblSortOrder;
        private System.Windows.Forms.NumericUpDown numSortOrder;
        private System.Windows.Forms.Panel         pnlButtons;
        private System.Windows.Forms.Button        btnSave;
        private System.Windows.Forms.Button        btnDelete;
        private System.Windows.Forms.Button        btnClear;
    }
}
