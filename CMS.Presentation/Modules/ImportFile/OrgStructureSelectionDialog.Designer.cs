namespace CMS.Presentation
{
    partial class OrgStructureSelectionDialog
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
            this.pnlHeader       = new System.Windows.Forms.Panel();
            this.lblTitle        = new System.Windows.Forms.Label();
            this.pnlButtons      = new System.Windows.Forms.Panel();
            this._btnSelectAll   = new System.Windows.Forms.Button();
            this._btnDeselectAll = new System.Windows.Forms.Button();
            this._lblSelectedCount = new System.Windows.Forms.Label();
            this._treeView       = new System.Windows.Forms.TreeView();
            this.pnlFooter       = new System.Windows.Forms.Panel();
            this._btnOk          = new System.Windows.Forms.Button();
            this._btnCancel      = new System.Windows.Forms.Button();

            this.pnlHeader.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // ── pnlHeader ────────────────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock     = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height   = 50;
            this.pnlHeader.Name     = "pnlHeader";
            this.pnlHeader.TabIndex = 0;

            // ── lblTitle ─────────────────────────────────────────────────────
            this.lblTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Name      = "lblTitle";
            this.lblTitle.Padding   = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.lblTitle.Text      = "🏢 เลือกหน่วยงาน (เลือกได้หลายรายการ)";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitle.TabIndex  = 0;

            // ── pnlButtons ───────────────────────────────────────────────────
            this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(248, 249, 252);
            this.pnlButtons.Controls.Add(this._btnSelectAll);
            this.pnlButtons.Controls.Add(this._btnDeselectAll);
            this.pnlButtons.Controls.Add(this._lblSelectedCount);
            this.pnlButtons.Dock     = System.Windows.Forms.DockStyle.Top;
            this.pnlButtons.Height   = 38;
            this.pnlButtons.Name     = "pnlButtons";
            this.pnlButtons.Padding  = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.pnlButtons.TabIndex = 1;

            // ── _btnSelectAll ────────────────────────────────────────────────
            this._btnSelectAll.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this._btnSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSelectAll.Font      = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this._btnSelectAll.ForeColor = System.Drawing.Color.White;
            this._btnSelectAll.Location  = new System.Drawing.Point(10, 5);
            this._btnSelectAll.Name      = "_btnSelectAll";
            this._btnSelectAll.Size      = new System.Drawing.Size(120, 26);
            this._btnSelectAll.TabIndex  = 0;
            this._btnSelectAll.Text      = "✓ เลือกทั้งหมด";
            this._btnSelectAll.UseVisualStyleBackColor = false;
            this._btnSelectAll.FlatAppearance.BorderSize = 0;
            this._btnSelectAll.Click += new System.EventHandler(this.BtnSelectAll_Click);

            // ── _btnDeselectAll ──────────────────────────────────────────────
            this._btnDeselectAll.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this._btnDeselectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnDeselectAll.Font      = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this._btnDeselectAll.ForeColor = System.Drawing.Color.White;
            this._btnDeselectAll.Location  = new System.Drawing.Point(140, 5);
            this._btnDeselectAll.Name      = "_btnDeselectAll";
            this._btnDeselectAll.Size      = new System.Drawing.Size(120, 26);
            this._btnDeselectAll.TabIndex  = 1;
            this._btnDeselectAll.Text      = "✗ ล้างทั้งหมด";
            this._btnDeselectAll.UseVisualStyleBackColor = false;
            this._btnDeselectAll.FlatAppearance.BorderSize = 0;
            this._btnDeselectAll.Click += new System.EventHandler(this.BtnDeselectAll_Click);

            // ── _lblSelectedCount ────────────────────────────────────────────
            this._lblSelectedCount.AutoSize  = true;
            this._lblSelectedCount.Font      = new System.Drawing.Font("Segoe UI", 9f);
            this._lblSelectedCount.ForeColor = System.Drawing.Color.FromArgb(108, 117, 125);
            this._lblSelectedCount.Location  = new System.Drawing.Point(275, 10);
            this._lblSelectedCount.Name      = "_lblSelectedCount";
            this._lblSelectedCount.Text      = "เลือกแล้ว: 0 รายการ";
            this._lblSelectedCount.TabIndex  = 2;

            // ── _treeView ────────────────────────────────────────────────────
            this._treeView.BackColor    = System.Drawing.Color.White;
            this._treeView.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this._treeView.CheckBoxes   = true;
            this._treeView.Dock         = System.Windows.Forms.DockStyle.Fill;
            this._treeView.Font         = new System.Drawing.Font("Segoe UI", 9f);
            this._treeView.FullRowSelect = true;
            this._treeView.HideSelection = false;
            this._treeView.Name         = "_treeView";
            this._treeView.ShowLines    = true;
            this._treeView.ShowPlusMinus = true;
            this._treeView.ShowRootLines = true;
            this._treeView.TabIndex     = 2;
            this._treeView.AfterCheck  += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_AfterCheck);

            // ── pnlFooter ────────────────────────────────────────────────────
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(248, 250, 255);
            this.pnlFooter.Controls.Add(this._btnOk);
            this.pnlFooter.Controls.Add(this._btnCancel);
            this.pnlFooter.Dock     = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height   = 50;
            this.pnlFooter.Name     = "pnlFooter";
            this.pnlFooter.TabIndex = 3;

            // ── _btnOk ───────────────────────────────────────────────────────
            this._btnOk.Anchor       = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this._btnOk.BackColor    = System.Drawing.Color.FromArgb(0, 123, 255);
            this._btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._btnOk.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this._btnOk.Font         = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this._btnOk.ForeColor    = System.Drawing.Color.White;
            this._btnOk.Location     = new System.Drawing.Point(430, 8);
            this._btnOk.Name         = "_btnOk";
            this._btnOk.Size         = new System.Drawing.Size(80, 34);
            this._btnOk.TabIndex     = 0;
            this._btnOk.Text         = "ตกลง";
            this._btnOk.UseVisualStyleBackColor = false;
            this._btnOk.FlatAppearance.BorderSize = 0;
            this._btnOk.Click += new System.EventHandler(this.BtnOk_Click);

            // ── _btnCancel ───────────────────────────────────────────────────
            this._btnCancel.Anchor       = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this._btnCancel.BackColor    = System.Drawing.Color.FromArgb(108, 117, 125);
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Font         = new System.Drawing.Font("Segoe UI", 9f);
            this._btnCancel.ForeColor    = System.Drawing.Color.White;
            this._btnCancel.Location     = new System.Drawing.Point(520, 8);
            this._btnCancel.Name         = "_btnCancel";
            this._btnCancel.Size         = new System.Drawing.Size(70, 34);
            this._btnCancel.TabIndex     = 1;
            this._btnCancel.Text         = "ยกเลิก";
            this._btnCancel.UseVisualStyleBackColor = false;
            this._btnCancel.FlatAppearance.BorderSize = 0;

            // ── Form ─────────────────────────────────────────────────────────
            this.AcceptButton        = this._btnOk;
            this.CancelButton        = this._btnCancel;
            this.BackColor           = System.Drawing.Color.White;
            this.ClientSize          = new System.Drawing.Size(600, 500);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.Name                = "OrgStructureSelectionDialog";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "เลือกหน่วยงาน";

            this.Controls.Add(this._treeView);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlFooter);

            this.pnlHeader.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.pnlButtons.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        // ── Controls ─────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel          pnlHeader;
        private System.Windows.Forms.Label          lblTitle;
        private System.Windows.Forms.Panel          pnlButtons;
        private System.Windows.Forms.Button         _btnSelectAll;
        private System.Windows.Forms.Button         _btnDeselectAll;
        private System.Windows.Forms.Label          _lblSelectedCount;
        private System.Windows.Forms.TreeView       _treeView;
        private System.Windows.Forms.Panel          pnlFooter;
        private System.Windows.Forms.Button         _btnOk;
        private System.Windows.Forms.Button         _btnCancel;
    }
}
