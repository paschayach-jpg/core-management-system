namespace CMS.Presentation
{
    partial class PdpaReportForm
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
            this.lblSubject  = new System.Windows.Forms.Label();
            this.tabControl  = new System.Windows.Forms.TabControl();
            this.tabFiles    = new System.Windows.Forms.TabPage();
            this.gridFiles   = new System.Windows.Forms.DataGridView();
            this.tabAudit    = new System.Windows.Forms.TabPage();
            this.gridAudit   = new System.Windows.Forms.DataGridView();
            this.btnClose    = new System.Windows.Forms.Button();

            this.tabControl.SuspendLayout();
            this.tabFiles.SuspendLayout();
            this.tabAudit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridAudit)).BeginInit();
            this.SuspendLayout();

            // lblSubject
            this.lblSubject.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblSubject.Height    = 30;
            this.lblSubject.Text      = "PDPA Report";
            this.lblSubject.Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this.lblSubject.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblSubject.Padding   = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.lblSubject.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.lblSubject.ForeColor = System.Drawing.Color.White;

            // btnClose
            this.btnClose.Text        = "ปิด";
            this.btnClose.Width       = 80;
            this.btnClose.Height      = 28;
            this.btnClose.Dock        = System.Windows.Forms.DockStyle.Bottom;
            this.btnClose.Anchor      = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.FlatStyle   = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font        = new System.Drawing.Font("Segoe UI", 9f);
            this.btnClose.Click      += new System.EventHandler(this.btnClose_Click);

            // gridFiles
            this.gridFiles.Dock                  = System.Windows.Forms.DockStyle.Fill;
            this.gridFiles.ReadOnly              = true;
            this.gridFiles.AutoGenerateColumns   = false;
            this.gridFiles.AllowUserToAddRows    = false;
            this.gridFiles.AllowUserToDeleteRows = false;
            this.gridFiles.BackgroundColor       = System.Drawing.Color.White;
            this.gridFiles.BorderStyle           = System.Windows.Forms.BorderStyle.None;
            this.gridFiles.Font                  = new System.Drawing.Font("Segoe UI", 9f);

            // gridAudit
            this.gridAudit.Dock                  = System.Windows.Forms.DockStyle.Fill;
            this.gridAudit.ReadOnly              = true;
            this.gridAudit.AutoGenerateColumns   = false;
            this.gridAudit.AllowUserToAddRows    = false;
            this.gridAudit.AllowUserToDeleteRows = false;
            this.gridAudit.BackgroundColor       = System.Drawing.Color.White;
            this.gridAudit.BorderStyle           = System.Windows.Forms.BorderStyle.None;
            this.gridAudit.Font                  = new System.Drawing.Font("Segoe UI", 9f);

            // tabFiles
            this.tabFiles.Text    = "ไฟล์เอกสาร";
            this.tabFiles.Padding = new System.Windows.Forms.Padding(0);
            this.tabFiles.Controls.Add(this.gridFiles);

            // tabAudit
            this.tabAudit.Text    = "Audit Logs";
            this.tabAudit.Padding = new System.Windows.Forms.Padding(0);
            this.tabAudit.Controls.Add(this.gridAudit);

            // tabControl
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 9f);
            this.tabControl.TabPages.AddRange(new System.Windows.Forms.TabPage[] {
                this.tabFiles, this.tabAudit });

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(760, 520);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox         = true;
            this.MinimizeBox         = false;
            this.MinimumSize         = new System.Drawing.Size(600, 400);
            this.Name                = "PdpaReportForm";
            this.Text                = "PDPA Personal Data Report";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;

            // DockStyle order: Fill first, Bottom, Top last
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblSubject);

            this.tabControl.ResumeLayout(false);
            this.tabFiles.ResumeLayout(false);
            this.tabAudit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridAudit)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label          lblSubject;
        private System.Windows.Forms.TabControl     tabControl;
        private System.Windows.Forms.TabPage        tabFiles;
        private System.Windows.Forms.DataGridView   gridFiles;
        private System.Windows.Forms.TabPage        tabAudit;
        private System.Windows.Forms.DataGridView   gridAudit;
        private System.Windows.Forms.Button         btnClose;
    }
}
