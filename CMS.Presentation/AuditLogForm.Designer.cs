namespace CMS.Presentation
{
    partial class AuditLogForm
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
            this.pnlHeader      = new System.Windows.Forms.Panel();
            this.lblTitle       = new System.Windows.Forms.Label();
            this.lblSubTitle    = new System.Windows.Forms.Label();
            this.pnlFilter      = new System.Windows.Forms.Panel();
            this.lblFrom        = new System.Windows.Forms.Label();
            this.dtpFrom        = new System.Windows.Forms.DateTimePicker();
            this.lblTo          = new System.Windows.Forms.Label();
            this.dtpTo          = new System.Windows.Forms.DateTimePicker();
            this.lblModule      = new System.Windows.Forms.Label();
            this.cmbModule      = new System.Windows.Forms.ComboBox();
            this.lblAction      = new System.Windows.Forms.Label();
            this.cmbAction      = new System.Windows.Forms.ComboBox();
            this.lblResult      = new System.Windows.Forms.Label();
            this.cmbResult      = new System.Windows.Forms.ComboBox();
            this.lblUserName    = new System.Windows.Forms.Label();
            this.txtUserName    = new System.Windows.Forms.TextBox();
            this.btnSearch      = new System.Windows.Forms.Button();
            this.btnClearFilter = new System.Windows.Forms.Button();
            this.btnExport      = new System.Windows.Forms.Button();
            this.pnlMain        = new System.Windows.Forms.SplitContainer();
            this.lvAudit        = new System.Windows.Forms.ListView();
            this.colID          = new System.Windows.Forms.ColumnHeader();
            this.colDate        = new System.Windows.Forms.ColumnHeader();
            this.colUser        = new System.Windows.Forms.ColumnHeader();
            this.colModule      = new System.Windows.Forms.ColumnHeader();
            this.colAction      = new System.Windows.Forms.ColumnHeader();
            this.colTable       = new System.Windows.Forms.ColumnHeader();
            this.colRecord      = new System.Windows.Forms.ColumnHeader();
            this.colResult      = new System.Windows.Forms.ColumnHeader();
            this.colIP          = new System.Windows.Forms.ColumnHeader();
            this.pnlDetail      = new System.Windows.Forms.Panel();
            this.lblDetailTitle = new System.Windows.Forms.Label();
            this.txtDetail      = new System.Windows.Forms.RichTextBox();
            this.pnlFooter      = new System.Windows.Forms.Panel();
            this.lblStatus      = new System.Windows.Forms.Label();
            this.btnPrev        = new System.Windows.Forms.Button();
            this.lblPageInfo    = new System.Windows.Forms.Label();
            this.btnNext        = new System.Windows.Forms.Button();

            this.pnlHeader.SuspendLayout();
            this.pnlFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlMain)).BeginInit();
            this.pnlMain.Panel1.SuspendLayout();
            this.pnlMain.Panel2.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlDetail.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // ── pnlHeader ────────────────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblSubTitle);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock     = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height   = 52;
            this.pnlHeader.Padding  = new System.Windows.Forms.Padding(14, 0, 0, 0);

            this.lblTitle.AutoSize  = false;
            this.lblTitle.Dock      = System.Windows.Forms.DockStyle.None;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location  = new System.Drawing.Point(14, 6);
            this.lblTitle.Size      = new System.Drawing.Size(400, 22);
            this.lblTitle.Text      = "📋  Audit Log — บันทึกกิจกรรมระบบ";

            this.lblSubTitle.AutoSize  = false;
            this.lblSubTitle.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(180, 200, 230);
            this.lblSubTitle.Location  = new System.Drawing.Point(16, 30);
            this.lblSubTitle.Size      = new System.Drawing.Size(500, 16);
            this.lblSubTitle.Text      = "ดูประวัติการใช้งานระบบ  •  เฉพาะ Manager / Admin เท่านั้น";

            // ── pnlFilter ────────────────────────────────────────────────────
            this.pnlFilter.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlFilter.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlFilter.Height    = 90;
            this.pnlFilter.Padding   = new System.Windows.Forms.Padding(10, 8, 10, 8);

            // Row 1: date range + module + action
            int y1 = 10, x = 10;

            this.lblFrom.AutoSize = true;
            this.lblFrom.Font     = new System.Drawing.Font("Segoe UI", 9f);
            this.lblFrom.Location = new System.Drawing.Point(x, y1 + 3);
            this.lblFrom.Text     = "ตั้งแต่";

            this.dtpFrom.Format   = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFrom.Location = new System.Drawing.Point(x + 46, y1);
            this.dtpFrom.Size     = new System.Drawing.Size(110, 22);
            this.dtpFrom.Font     = new System.Drawing.Font("Segoe UI", 9f);

            this.lblTo.AutoSize = true;
            this.lblTo.Font     = new System.Drawing.Font("Segoe UI", 9f);
            this.lblTo.Location = new System.Drawing.Point(x + 166, y1 + 3);
            this.lblTo.Text     = "ถึง";

            this.dtpTo.Format   = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpTo.Location = new System.Drawing.Point(x + 190, y1);
            this.dtpTo.Size     = new System.Drawing.Size(110, 22);
            this.dtpTo.Font     = new System.Drawing.Font("Segoe UI", 9f);

            this.lblModule.AutoSize = true;
            this.lblModule.Font     = new System.Drawing.Font("Segoe UI", 9f);
            this.lblModule.Location = new System.Drawing.Point(x + 316, y1 + 3);
            this.lblModule.Text     = "Module";

            this.cmbModule.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbModule.Font          = new System.Drawing.Font("Segoe UI", 9f);
            this.cmbModule.Location      = new System.Drawing.Point(x + 366, y1);
            this.cmbModule.Size          = new System.Drawing.Size(130, 22);

            this.lblAction.AutoSize = true;
            this.lblAction.Font     = new System.Drawing.Font("Segoe UI", 9f);
            this.lblAction.Location = new System.Drawing.Point(x + 506, y1 + 3);
            this.lblAction.Text     = "Action";

            this.cmbAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAction.Font          = new System.Drawing.Font("Segoe UI", 9f);
            this.cmbAction.Location      = new System.Drawing.Point(x + 550, y1);
            this.cmbAction.Size          = new System.Drawing.Size(130, 22);

            // Row 2: user + result + buttons
            int y2 = 44;

            this.lblUserName.AutoSize = true;
            this.lblUserName.Font     = new System.Drawing.Font("Segoe UI", 9f);
            this.lblUserName.Location = new System.Drawing.Point(x, y2 + 3);
            this.lblUserName.Text     = "ผู้ใช้";

            this.txtUserName.Font     = new System.Drawing.Font("Segoe UI", 9f);
            this.txtUserName.Location = new System.Drawing.Point(x + 36, y2);
            this.txtUserName.Size     = new System.Drawing.Size(170, 22);

            this.lblResult.AutoSize = true;
            this.lblResult.Font     = new System.Drawing.Font("Segoe UI", 9f);
            this.lblResult.Location = new System.Drawing.Point(x + 216, y2 + 3);
            this.lblResult.Text     = "ผลลัพธ์";

            this.cmbResult.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbResult.Font          = new System.Drawing.Font("Segoe UI", 9f);
            this.cmbResult.Location      = new System.Drawing.Point(x + 264, y2);
            this.cmbResult.Size          = new System.Drawing.Size(120, 22);

            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location  = new System.Drawing.Point(x + 396, y2 - 1);
            this.btnSearch.Size      = new System.Drawing.Size(90, 26);
            this.btnSearch.Text      = "🔍 ค้นหา";
            this.btnSearch.Click    += new System.EventHandler(this.btnSearch_Click);

            this.btnClearFilter.BackColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.btnClearFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearFilter.Font      = new System.Drawing.Font("Segoe UI", 9f);
            this.btnClearFilter.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.btnClearFilter.Location  = new System.Drawing.Point(x + 494, y2 - 1);
            this.btnClearFilter.Size      = new System.Drawing.Size(80, 26);
            this.btnClearFilter.Text      = "ล้างตัวกรอง";
            this.btnClearFilter.Click    += new System.EventHandler(this.btnClearFilter_Click);

            this.btnExport.BackColor = System.Drawing.Color.FromArgb(0, 128, 0);
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.Font      = new System.Drawing.Font("Segoe UI", 9f);
            this.btnExport.ForeColor = System.Drawing.Color.White;
            this.btnExport.Location  = new System.Drawing.Point(x + 582, y2 - 1);
            this.btnExport.Size      = new System.Drawing.Size(100, 26);
            this.btnExport.Text      = "📥 Export CSV";
            this.btnExport.Click    += new System.EventHandler(this.btnExport_Click);

            this.pnlFilter.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                this.lblFrom, this.dtpFrom, this.lblTo, this.dtpTo,
                this.lblModule, this.cmbModule, this.lblAction, this.cmbAction,
                this.lblUserName, this.txtUserName, this.lblResult, this.cmbResult,
                this.btnSearch, this.btnClearFilter, this.btnExport
            });

            // ── pnlMain (SplitContainer) ──────────────────────────────────────
            this.pnlMain.Dock        = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.pnlMain.SplitterDistance = 340;
            this.pnlMain.Panel1MinSize    = 200;
            this.pnlMain.Panel2MinSize    = 120;

            // ── lvAudit ───────────────────────────────────────────────────────
            this.lvAudit.Dock          = System.Windows.Forms.DockStyle.Fill;
            this.lvAudit.View          = System.Windows.Forms.View.Details;
            this.lvAudit.FullRowSelect = true;
            this.lvAudit.GridLines     = true;
            this.lvAudit.MultiSelect   = false;
            this.lvAudit.Font          = new System.Drawing.Font("Segoe UI", 9f);
            this.lvAudit.BorderStyle   = System.Windows.Forms.BorderStyle.None;
            this.lvAudit.BackColor     = System.Drawing.Color.White;

            this.colID.Text     = "ID";       this.colID.Width     = 60;
            this.colDate.Text   = "วันที่เวลา"; this.colDate.Width   = 130;
            this.colUser.Text   = "ผู้ใช้";    this.colUser.Width   = 120;
            this.colModule.Text = "Module";   this.colModule.Width = 90;
            this.colAction.Text = "Action";   this.colAction.Width = 100;
            this.colTable.Text  = "ตาราง";    this.colTable.Width  = 100;
            this.colRecord.Text = "RecordID"; this.colRecord.Width = 70;
            this.colResult.Text = "ผลลัพธ์";  this.colResult.Width = 80;
            this.colIP.Text     = "IP Address"; this.colIP.Width   = 110;

            this.lvAudit.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
            {
                this.colID, this.colDate, this.colUser, this.colModule,
                this.colAction, this.colTable, this.colRecord, this.colResult, this.colIP
            });

            this.lvAudit.SelectedIndexChanged += new System.EventHandler(this.lvAudit_SelectedIndexChanged);
            this.pnlMain.Panel1.Controls.Add(this.lvAudit);

            // ── pnlDetail ─────────────────────────────────────────────────────
            this.pnlDetail.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetail.BackColor = System.Drawing.Color.White;

            this.lblDetailTitle.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblDetailTitle.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblDetailTitle.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.lblDetailTitle.Height    = 24;
            this.lblDetailTitle.Padding   = new System.Windows.Forms.Padding(6, 4, 0, 0);
            this.lblDetailTitle.Text      = "รายละเอียด  (คลิกแถวเพื่อดู)";
            this.lblDetailTitle.BackColor = System.Drawing.Color.FromArgb(235, 240, 250);

            this.txtDetail.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.txtDetail.ReadOnly  = true;
            this.txtDetail.Font      = new System.Drawing.Font("Consolas", 9f);
            this.txtDetail.BackColor = System.Drawing.Color.White;
            this.txtDetail.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDetail.ScrollBars  = System.Windows.Forms.RichTextBoxScrollBars.Vertical;

            this.pnlDetail.Controls.Add(this.txtDetail);
            this.pnlDetail.Controls.Add(this.lblDetailTitle);
            this.pnlMain.Panel2.Controls.Add(this.pnlDetail);

            // ── pnlFooter ─────────────────────────────────────────────────────
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlFooter.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height    = 34;
            this.pnlFooter.Padding   = new System.Windows.Forms.Padding(10, 4, 10, 4);

            this.lblStatus.AutoSize  = false;
            this.lblStatus.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font      = new System.Drawing.Font("Segoe UI", 9f);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.lblStatus.Text      = "";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.btnPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrev.Font      = new System.Drawing.Font("Segoe UI", 9f);
            this.btnPrev.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnPrev.Size      = new System.Drawing.Size(60, 26);
            this.btnPrev.Text      = "◀ ก่อน";
            this.btnPrev.Click    += new System.EventHandler(this.btnPrev_Click);

            this.lblPageInfo.AutoSize  = false;
            this.lblPageInfo.Dock      = System.Windows.Forms.DockStyle.Right;
            this.lblPageInfo.Font      = new System.Drawing.Font("Segoe UI", 9f);
            this.lblPageInfo.Size      = new System.Drawing.Size(100, 26);
            this.lblPageInfo.Text      = "หน้า 1 / 1";
            this.lblPageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Font      = new System.Drawing.Font("Segoe UI", 9f);
            this.btnNext.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnNext.Size      = new System.Drawing.Size(60, 26);
            this.btnNext.Text      = "ถัดไป ▶";
            this.btnNext.Click    += new System.EventHandler(this.btnNext_Click);

            this.pnlFooter.Controls.Add(this.lblStatus);
            this.pnlFooter.Controls.Add(this.btnPrev);
            this.pnlFooter.Controls.Add(this.lblPageInfo);
            this.pnlFooter.Controls.Add(this.btnNext);

            // ── Form ──────────────────────────────────────────────────────────
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlFilter);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlFooter);

            this.Text          = "Audit Log";
            this.Font          = new System.Drawing.Font("Segoe UI", 9f);
            this.MinimumSize   = new System.Drawing.Size(900, 600);
            this.Size          = new System.Drawing.Size(1100, 700);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            this.pnlHeader.ResumeLayout(false);
            this.pnlFilter.ResumeLayout(false);
            this.pnlMain.Panel1.ResumeLayout(false);
            this.pnlMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlMain)).EndInit();
            this.pnlMain.ResumeLayout(false);
            this.pnlDetail.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        // ── Controls ─────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel           pnlHeader;
        private System.Windows.Forms.Label           lblTitle;
        private System.Windows.Forms.Label           lblSubTitle;
        private System.Windows.Forms.Panel           pnlFilter;
        private System.Windows.Forms.Label           lblFrom;
        private System.Windows.Forms.DateTimePicker  dtpFrom;
        private System.Windows.Forms.Label           lblTo;
        private System.Windows.Forms.DateTimePicker  dtpTo;
        private System.Windows.Forms.Label           lblModule;
        private System.Windows.Forms.ComboBox        cmbModule;
        private System.Windows.Forms.Label           lblAction;
        private System.Windows.Forms.ComboBox        cmbAction;
        private System.Windows.Forms.Label           lblResult;
        private System.Windows.Forms.ComboBox        cmbResult;
        private System.Windows.Forms.Label           lblUserName;
        private System.Windows.Forms.TextBox         txtUserName;
        private System.Windows.Forms.Button          btnSearch;
        private System.Windows.Forms.Button          btnClearFilter;
        private System.Windows.Forms.Button          btnExport;
        private System.Windows.Forms.SplitContainer  pnlMain;
        private System.Windows.Forms.ListView        lvAudit;
        private System.Windows.Forms.ColumnHeader    colID;
        private System.Windows.Forms.ColumnHeader    colDate;
        private System.Windows.Forms.ColumnHeader    colUser;
        private System.Windows.Forms.ColumnHeader    colModule;
        private System.Windows.Forms.ColumnHeader    colAction;
        private System.Windows.Forms.ColumnHeader    colTable;
        private System.Windows.Forms.ColumnHeader    colRecord;
        private System.Windows.Forms.ColumnHeader    colResult;
        private System.Windows.Forms.ColumnHeader    colIP;
        private System.Windows.Forms.Panel           pnlDetail;
        private System.Windows.Forms.Label           lblDetailTitle;
        private System.Windows.Forms.RichTextBox     txtDetail;
        private System.Windows.Forms.Panel           pnlFooter;
        private System.Windows.Forms.Label           lblStatus;
        private System.Windows.Forms.Button          btnPrev;
        private System.Windows.Forms.Label           lblPageInfo;
        private System.Windows.Forms.Button          btnNext;
    }
}
