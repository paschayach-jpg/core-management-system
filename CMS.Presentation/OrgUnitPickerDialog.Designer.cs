namespace CMS.Presentation
{
    partial class OrgUnitPickerDialog
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

            this.pnlHeader   = new System.Windows.Forms.Panel();
            this.lblHeader   = new System.Windows.Forms.Label();
            this.pnlToolbar  = new System.Windows.Forms.Panel();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnClearAll  = new System.Windows.Forms.Button();
            this.lblStatus   = new System.Windows.Forms.Label();
            this.treeOrg     = new System.Windows.Forms.TreeView();
            this.pnlFooter   = new System.Windows.Forms.Panel();
            this.btnOk       = new System.Windows.Forms.Button();
            this.btnCancel   = new System.Windows.Forms.Button();

            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Dock   = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 44;
            this.pnlHeader.Name   = "pnlHeader";

            this.lblHeader.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font      = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Name      = "lblHeader";
            this.lblHeader.Padding   = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblHeader.Text      = "🏢  เลือกหน่วยงาน (เลือกได้หลายรายการ)";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // pnlToolbar
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(240, 243, 250);
            this.pnlToolbar.Controls.Add(this.btnSelectAll);
            this.pnlToolbar.Controls.Add(this.btnClearAll);
            this.pnlToolbar.Controls.Add(this.lblStatus);
            this.pnlToolbar.Dock   = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 40;
            this.pnlToolbar.Name   = "pnlToolbar";
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);

            this.btnSelectAll.BackColor = System.Drawing.Color.FromArgb(43, 87, 154);
            this.btnSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectAll.FlatAppearance.BorderSize = 0;
            this.btnSelectAll.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnSelectAll.ForeColor = System.Drawing.Color.White;
            this.btnSelectAll.Location  = new System.Drawing.Point(8, 6);
            this.btnSelectAll.Name      = "btnSelectAll";
            this.btnSelectAll.Size      = new System.Drawing.Size(90, 26);
            this.btnSelectAll.Text      = "✔ เลือกทั้งหมด";
            this.btnSelectAll.Click    += new System.EventHandler(this.btnSelectAll_Click);

            this.btnClearAll.BackColor = System.Drawing.Color.White;
            this.btnClearAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
            this.btnClearAll.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnClearAll.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.btnClearAll.Location  = new System.Drawing.Point(106, 6);
            this.btnClearAll.Name      = "btnClearAll";
            this.btnClearAll.Size      = new System.Drawing.Size(80, 26);
            this.btnClearAll.Text      = "✕ ล้างทั้งหมด";
            this.btnClearAll.Click    += new System.EventHandler(this.btnClearAll_Click);

            this.lblStatus.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.lblStatus.Location  = new System.Drawing.Point(220, 10);
            this.lblStatus.Name      = "lblStatus";
            this.lblStatus.Size      = new System.Drawing.Size(200, 20);
            this.lblStatus.Text      = "เลือกแล้ว 0 หน่วยงาน";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // treeOrg
            this.treeOrg.CheckBoxes  = true;
            this.treeOrg.Dock        = System.Windows.Forms.DockStyle.Fill;
            this.treeOrg.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            this.treeOrg.Name        = "treeOrg";
            this.treeOrg.ShowLines   = true;
            this.treeOrg.ShowPlusMinus = true;
            this.treeOrg.TabIndex    = 0;
            this.treeOrg.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeOrg_AfterCheck);

            // pnlFooter
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlFooter.Controls.Add(this.btnOk);
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Dock   = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height = 52;
            this.pnlFooter.Name   = "pnlFooter";
            this.pnlFooter.Resize += (s, e) => {
                this.btnCancel.Left = this.pnlFooter.ClientSize.Width - this.btnCancel.Width - 8;
                this.btnOk.Left     = this.btnCancel.Left - this.btnOk.Width - 8;
                this.btnOk.Top      = this.btnCancel.Top = (this.pnlFooter.Height - this.btnOk.Height) / 2;
            };

            this.btnOk.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.btnOk.Enabled   = false;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.FlatAppearance.BorderSize = 0;
            this.btnOk.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.Location  = new System.Drawing.Point(232, 9);
            this.btnOk.Name      = "btnOk";
            this.btnOk.Size      = new System.Drawing.Size(120, 34);
            this.btnOk.Text      = "✔  ยืนยันการเลือก";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click    += new System.EventHandler(this.btnOk_Click);

            this.btnCancel.Anchor       = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor    = System.Drawing.Color.White;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
            this.btnCancel.Font         = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCancel.ForeColor    = System.Drawing.Color.FromArgb(80, 80, 80);
            this.btnCancel.Location     = new System.Drawing.Point(360, 9);
            this.btnCancel.Name         = "btnCancel";
            this.btnCancel.Size         = new System.Drawing.Size(90, 34);
            this.btnCancel.Text         = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton        = this.btnCancel;
            this.ClientSize          = new System.Drawing.Size(460, 520);
            // DockStyle order: Bottom ก่อน → Fill → Top (topmost last)
            this.Controls.Add(this.treeOrg);    // Fill — add first
            this.Controls.Add(this.pnlFooter);  // Bottom — add before Top items
            this.Controls.Add(this.pnlToolbar); // Top
            this.Controls.Add(this.pnlHeader);  // Top (topmost — add last)
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "OrgUnitPickerDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "เลือกหน่วยงาน";
            this.Load           += new System.EventHandler(this.OrgUnitPickerDialog_Load);

            this.pnlHeader.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel    pnlHeader;
        private System.Windows.Forms.Label    lblHeader;
        private System.Windows.Forms.Panel    pnlToolbar;
        private System.Windows.Forms.Button   btnSelectAll;
        private System.Windows.Forms.Button   btnClearAll;
        private System.Windows.Forms.Label    lblStatus;
        private System.Windows.Forms.TreeView treeOrg;
        private System.Windows.Forms.Panel    pnlFooter;
        private System.Windows.Forms.Button   btnOk;
        private System.Windows.Forms.Button   btnCancel;
    }
}
