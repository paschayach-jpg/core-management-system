using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.DMS
{
    partial class OrgMasterImportDialog
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

        private Panel pnlHeader;
        private Label lblTitle;
        private Panel pnlTop;
        private Label lblFile;
        private TextBox txtFilePath;
        private Button btnBrowse;
        private Label lblSheet;
        private ComboBox cboSheet;
        private Button btnLoadPreview;
        private Button btnDownloadTemplate;
        private Panel pnlStatus;
        private Label lblSummary;
        private DataGridView dgvPreview;
        private Panel pnlBottom;
        private Button btnImport;
        private Button btnClose;

        private void InitializeComponent()
        {
            this.pnlHeader = new Panel();
            this.lblTitle = new Label();
            this.pnlTop = new Panel();
            this.lblFile = new Label();
            this.txtFilePath = new TextBox();
            this.btnBrowse = new Button();
            this.lblSheet = new Label();
            this.cboSheet = new ComboBox();
            this.btnLoadPreview = new Button();
            this.btnDownloadTemplate = new Button();
            this.pnlStatus = new Panel();
            this.lblSummary = new Label();
            this.dgvPreview = new DataGridView();
            this.pnlBottom = new Panel();
            this.btnImport = new Button();
            this.btnClose = new Button();

            this.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).BeginInit();
            this.pnlBottom.SuspendLayout();

            // Form
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.ClientSize = new Size(880, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "นำเข้าข้อมูล Master จาก Excel";
            this.Font = new Font("Tahoma", 9.5f);
            this.BackColor = Color.White;

            // Header
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Height = 44;
            this.pnlHeader.BackColor = Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.lblTitle.Dock = DockStyle.Fill;
            this.lblTitle.Padding = new Padding(16, 0, 0, 0);
            this.lblTitle.Text = "📥 ระบบนำเข้าข้อมูล Master โครงสร้างองค์กรจาก Excel";
            this.lblTitle.Font = new Font("Tahoma", 12f, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Top Panel (File selection & Actions)
            this.pnlTop.Dock = DockStyle.Top;
            this.pnlTop.Height = 85;
            this.pnlTop.BackColor = Color.FromArgb(244, 247, 251);
            this.pnlTop.Padding = new Padding(16, 12, 16, 8);

            this.lblFile.Text = "ไฟล์ Excel:";
            this.lblFile.Location = new Point(16, 14);
            this.lblFile.AutoSize = true;
            this.lblFile.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);

            this.txtFilePath.Location = new Point(90, 11);
            this.txtFilePath.Size = new Size(420, 23);
            this.txtFilePath.ReadOnly = true;

            this.btnBrowse.Text = "📁 เลือกไฟล์...";
            this.btnBrowse.Location = new Point(518, 10);
            this.btnBrowse.Size = new Size(100, 26);
            this.btnBrowse.BackColor = Color.White;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);

            this.btnDownloadTemplate.Text = "📑 โหลด Template";
            this.btnDownloadTemplate.Location = new Point(626, 10);
            this.btnDownloadTemplate.Size = new Size(130, 26);
            this.btnDownloadTemplate.BackColor = Color.FromArgb(230, 240, 255);
            this.btnDownloadTemplate.Click += new System.EventHandler(this.btnDownloadTemplate_Click);

            this.lblSheet.Text = "เลือก Sheet:";
            this.lblSheet.Location = new Point(16, 48);
            this.lblSheet.AutoSize = true;
            this.lblSheet.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);

            this.cboSheet.Location = new Point(90, 45);
            this.cboSheet.Size = new Size(250, 24);
            this.cboSheet.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboSheet.SelectedIndexChanged += new System.EventHandler(this.cboSheet_SelectedIndexChanged);

            this.btnLoadPreview.Text = "🔍 ตรวจสอบข้อมูล";
            this.btnLoadPreview.Location = new Point(348, 44);
            this.btnLoadPreview.Size = new Size(120, 26);
            this.btnLoadPreview.BackColor = Color.White;
            this.btnLoadPreview.Click += new System.EventHandler(this.btnLoadPreview_Click);

            this.pnlTop.Controls.AddRange(new Control[] {
                this.lblFile, this.txtFilePath, this.btnBrowse, this.btnDownloadTemplate,
                this.lblSheet, this.cboSheet, this.btnLoadPreview
            });

            // Status Panel
            this.pnlStatus.Dock = DockStyle.Top;
            this.pnlStatus.Height = 32;
            this.pnlStatus.BackColor = Color.FromArgb(235, 240, 248);
            this.pnlStatus.Padding = new Padding(16, 6, 16, 0);
            this.pnlStatus.Controls.Add(this.lblSummary);

            this.lblSummary.Dock = DockStyle.Fill;
            this.lblSummary.Text = "กรุณาเลือกไฟล์ Excel เพื่อตรวจสอบและนำเข้าข้อมูล";
            this.lblSummary.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);
            this.lblSummary.ForeColor = Color.FromArgb(31, 56, 100);

            // Preview Grid
            this.dgvPreview.Dock = DockStyle.Fill;
            this.dgvPreview.BackgroundColor = Color.White;
            this.dgvPreview.BorderStyle = BorderStyle.None;
            this.dgvPreview.AllowUserToAddRows = false;
            this.dgvPreview.AllowUserToDeleteRows = false;
            this.dgvPreview.ReadOnly = true;
            this.dgvPreview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvPreview.RowHeadersVisible = false;
            this.dgvPreview.EnableHeadersVisualStyles = false;
            this.dgvPreview.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 56, 100);
            this.dgvPreview.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            this.dgvPreview.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);
            this.dgvPreview.ColumnHeadersHeight = 30;

            // Bottom Panel
            this.pnlBottom.Dock = DockStyle.Bottom;
            this.pnlBottom.Size = new Size(880, 50); // real width before children anchor to Right, else they end up off-screen
            this.pnlBottom.BackColor = Color.FromArgb(240, 244, 249);
            this.pnlBottom.Padding = new Padding(16, 9, 16, 9);
            this.pnlBottom.Controls.Add(this.btnImport);
            this.pnlBottom.Controls.Add(this.btnClose);

            this.btnImport.Text = "📥 นำเข้าข้อมูล (Import)";
            this.btnImport.Size = new Size(160, 32);
            this.btnImport.Location = new Point(570, 9);
            this.btnImport.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnImport.BackColor = Color.FromArgb(30, 110, 50);
            this.btnImport.ForeColor = Color.White;
            this.btnImport.FlatStyle = FlatStyle.Flat;
            this.btnImport.Font = new Font("Tahoma", 9.5f, FontStyle.Bold);
            this.btnImport.Enabled = false;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);

            this.btnClose.Text = "✕ ปิด";
            this.btnClose.Size = new Size(90, 32);
            this.btnClose.Location = new Point(740, 9);
            this.btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.btnClose.BackColor = Color.White;
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // Add all to Form
            this.Controls.Add(this.dgvPreview);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlHeader);

            this.pnlHeader.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.pnlStatus.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
