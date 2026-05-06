namespace CMS.Presentation
{
    partial class FolderCreationChoiceDialog
    {
        private System.ComponentModel.IContainer components = null;

        // ── Controls ─────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel  _pnlHeader;
        private System.Windows.Forms.Label  _lblTitle;
        private System.Windows.Forms.Label  _lblSub;
        private System.Windows.Forms.Panel  _pnlCards;
        private System.Windows.Forms.Panel  _pnlTemplate;
        private System.Windows.Forms.Label  _lblTemplateIcon;
        private System.Windows.Forms.Label  _lblTemplateTitle;
        private System.Windows.Forms.Label  _lblTemplateDesc;
        private System.Windows.Forms.Panel  _pnlBlank;
        private System.Windows.Forms.Label  _lblBlankIcon;
        private System.Windows.Forms.Label  _lblBlankTitle;
        private System.Windows.Forms.Label  _lblBlankDesc;
        private System.Windows.Forms.Panel  _pnlFooter;
        private System.Windows.Forms.Panel  _line;
        private System.Windows.Forms.Button _btnOk;
        private System.Windows.Forms.Button _btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components        = new System.ComponentModel.Container();
            this._pnlHeader        = new System.Windows.Forms.Panel();
            this._lblTitle         = new System.Windows.Forms.Label();
            this._lblSub           = new System.Windows.Forms.Label();
            this._pnlCards         = new System.Windows.Forms.Panel();
            this._pnlTemplate      = new System.Windows.Forms.Panel();
            this._lblTemplateIcon  = new System.Windows.Forms.Label();
            this._lblTemplateTitle = new System.Windows.Forms.Label();
            this._lblTemplateDesc  = new System.Windows.Forms.Label();
            this._pnlBlank         = new System.Windows.Forms.Panel();
            this._lblBlankIcon     = new System.Windows.Forms.Label();
            this._lblBlankTitle    = new System.Windows.Forms.Label();
            this._lblBlankDesc     = new System.Windows.Forms.Label();
            this._pnlFooter        = new System.Windows.Forms.Panel();
            this._line             = new System.Windows.Forms.Panel();
            this._btnOk            = new System.Windows.Forms.Button();
            this._btnCancel        = new System.Windows.Forms.Button();
            this._pnlHeader.SuspendLayout();
            this._pnlCards.SuspendLayout();
            this._pnlTemplate.SuspendLayout();
            this._pnlBlank.SuspendLayout();
            this._pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // ── _pnlHeader ────────────────────────────────────────────────────
            this._pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this._pnlHeader.Height    = 52;
            this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this._pnlHeader.Controls.Add(this._lblSub);
            this._pnlHeader.Controls.Add(this._lblTitle);

            // ── _lblTitle ─────────────────────────────────────────────────────
            this._lblTitle.Text      = "วิธีสร้าง Folder";
            this._lblTitle.Dock      = System.Windows.Forms.DockStyle.Top;
            this._lblTitle.Height    = 30;
            this._lblTitle.ForeColor = System.Drawing.Color.White;
            this._lblTitle.Font      = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold);
            this._lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblTitle.Padding   = new System.Windows.Forms.Padding(16, 0, 0, 0);

            // ── _lblSub ───────────────────────────────────────────────────────
            this._lblSub.Text      = "";
            this._lblSub.Dock      = System.Windows.Forms.DockStyle.Fill;
            this._lblSub.ForeColor = System.Drawing.Color.FromArgb(180, 210, 255);
            this._lblSub.Font      = new System.Drawing.Font("Segoe UI", 9f);
            this._lblSub.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblSub.Padding   = new System.Windows.Forms.Padding(18, 0, 0, 4);

            // ── _pnlCards ─────────────────────────────────────────────────────
            this._pnlCards.Dock      = System.Windows.Forms.DockStyle.Fill;
            this._pnlCards.BackColor = System.Drawing.Color.Transparent;
            this._pnlCards.Padding   = new System.Windows.Forms.Padding(20, 16, 20, 8);
            this._pnlCards.Controls.Add(this._pnlTemplate);
            this._pnlCards.Controls.Add(this._pnlBlank);

            // ── _pnlTemplate ──────────────────────────────────────────────────
            this._pnlTemplate.BackColor  = System.Drawing.Color.White;
            this._pnlTemplate.Cursor     = System.Windows.Forms.Cursors.Hand;
            this._pnlTemplate.Padding    = new System.Windows.Forms.Padding(14);
            this._pnlTemplate.Location   = new System.Drawing.Point(20, 16);
            this._pnlTemplate.Size       = new System.Drawing.Size(200, 130);
            this._pnlTemplate.Anchor     = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this._pnlTemplate.Controls.Add(this._lblTemplateIcon);
            this._pnlTemplate.Controls.Add(this._lblTemplateTitle);
            this._pnlTemplate.Controls.Add(this._lblTemplateDesc);
            this._pnlTemplate.Paint       += new System.Windows.Forms.PaintEventHandler(this.PnlTemplate_Paint);
            this._pnlTemplate.MouseEnter  += new System.EventHandler(this.PnlTemplate_MouseEnter);
            this._pnlTemplate.MouseLeave  += new System.EventHandler(this.PnlTemplate_MouseLeave);
            this._pnlTemplate.Click       += new System.EventHandler(this.PnlTemplate_Click);
            this._pnlTemplate.DoubleClick += new System.EventHandler(this.PnlTemplate_DoubleClick);

            // ── _lblTemplateIcon ──────────────────────────────────────────────
            this._lblTemplateIcon.Text      = "📋";
            this._lblTemplateIcon.Font      = new System.Drawing.Font("Segoe UI", 28f);
            this._lblTemplateIcon.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this._lblTemplateIcon.AutoSize  = false;
            this._lblTemplateIcon.Location  = new System.Drawing.Point(14, 14);
            this._lblTemplateIcon.Size      = new System.Drawing.Size(56, 56);
            this._lblTemplateIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._lblTemplateIcon.MouseEnter  += new System.EventHandler(this.PnlTemplate_MouseEnter);
            this._lblTemplateIcon.MouseLeave  += new System.EventHandler(this.PnlTemplate_MouseLeave);
            this._lblTemplateIcon.Click       += new System.EventHandler(this.PnlTemplate_Click);
            this._lblTemplateIcon.DoubleClick += new System.EventHandler(this.PnlTemplate_DoubleClick);

            // ── _lblTemplateTitle ─────────────────────────────────────────────
            this._lblTemplateTitle.Text      = "📋  จาก Template";
            this._lblTemplateTitle.Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this._lblTemplateTitle.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this._lblTemplateTitle.AutoSize  = false;
            this._lblTemplateTitle.Location  = new System.Drawing.Point(14, 74);
            this._lblTemplateTitle.Size      = new System.Drawing.Size(172, 22);
            this._lblTemplateTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblTemplateTitle.MouseEnter  += new System.EventHandler(this.PnlTemplate_MouseEnter);
            this._lblTemplateTitle.MouseLeave  += new System.EventHandler(this.PnlTemplate_MouseLeave);
            this._lblTemplateTitle.Click       += new System.EventHandler(this.PnlTemplate_Click);
            this._lblTemplateTitle.DoubleClick += new System.EventHandler(this.PnlTemplate_DoubleClick);

            // ── _lblTemplateDesc ──────────────────────────────────────────────
            this._lblTemplateDesc.Text      = "สร้าง Sub-folder อัตโนมัติตามโครงสร้าง Template ที่กำหนดไว้";
            this._lblTemplateDesc.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            this._lblTemplateDesc.ForeColor = System.Drawing.Color.FromArgb(90, 90, 90);
            this._lblTemplateDesc.AutoSize  = false;
            this._lblTemplateDesc.Location  = new System.Drawing.Point(14, 96);
            this._lblTemplateDesc.Size      = new System.Drawing.Size(172, 30);
            this._lblTemplateDesc.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this._lblTemplateDesc.MouseEnter  += new System.EventHandler(this.PnlTemplate_MouseEnter);
            this._lblTemplateDesc.MouseLeave  += new System.EventHandler(this.PnlTemplate_MouseLeave);
            this._lblTemplateDesc.Click       += new System.EventHandler(this.PnlTemplate_Click);
            this._lblTemplateDesc.DoubleClick += new System.EventHandler(this.PnlTemplate_DoubleClick);

            // ── _pnlBlank ─────────────────────────────────────────────────────
            this._pnlBlank.BackColor  = System.Drawing.Color.White;
            this._pnlBlank.Cursor     = System.Windows.Forms.Cursors.Hand;
            this._pnlBlank.Padding    = new System.Windows.Forms.Padding(14);
            this._pnlBlank.Location   = new System.Drawing.Point(240, 16);
            this._pnlBlank.Size       = new System.Drawing.Size(200, 130);
            this._pnlBlank.Anchor     = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this._pnlBlank.Controls.Add(this._lblBlankIcon);
            this._pnlBlank.Controls.Add(this._lblBlankTitle);
            this._pnlBlank.Controls.Add(this._lblBlankDesc);
            this._pnlBlank.Paint       += new System.Windows.Forms.PaintEventHandler(this.PnlBlank_Paint);
            this._pnlBlank.MouseEnter  += new System.EventHandler(this.PnlBlank_MouseEnter);
            this._pnlBlank.MouseLeave  += new System.EventHandler(this.PnlBlank_MouseLeave);
            this._pnlBlank.Click       += new System.EventHandler(this.PnlBlank_Click);
            this._pnlBlank.DoubleClick += new System.EventHandler(this.PnlBlank_DoubleClick);

            // ── _lblBlankIcon ─────────────────────────────────────────────────
            this._lblBlankIcon.Text      = "📁";
            this._lblBlankIcon.Font      = new System.Drawing.Font("Segoe UI", 28f);
            this._lblBlankIcon.ForeColor = System.Drawing.Color.FromArgb(255, 160, 0);
            this._lblBlankIcon.AutoSize  = false;
            this._lblBlankIcon.Location  = new System.Drawing.Point(14, 14);
            this._lblBlankIcon.Size      = new System.Drawing.Size(56, 56);
            this._lblBlankIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._lblBlankIcon.MouseEnter  += new System.EventHandler(this.PnlBlank_MouseEnter);
            this._lblBlankIcon.MouseLeave  += new System.EventHandler(this.PnlBlank_MouseLeave);
            this._lblBlankIcon.Click       += new System.EventHandler(this.PnlBlank_Click);
            this._lblBlankIcon.DoubleClick += new System.EventHandler(this.PnlBlank_DoubleClick);

            // ── _lblBlankTitle ────────────────────────────────────────────────
            this._lblBlankTitle.Text      = "📁  Blank Folder";
            this._lblBlankTitle.Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this._lblBlankTitle.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this._lblBlankTitle.AutoSize  = false;
            this._lblBlankTitle.Location  = new System.Drawing.Point(14, 74);
            this._lblBlankTitle.Size      = new System.Drawing.Size(172, 22);
            this._lblBlankTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblBlankTitle.MouseEnter  += new System.EventHandler(this.PnlBlank_MouseEnter);
            this._lblBlankTitle.MouseLeave  += new System.EventHandler(this.PnlBlank_MouseLeave);
            this._lblBlankTitle.Click       += new System.EventHandler(this.PnlBlank_Click);
            this._lblBlankTitle.DoubleClick += new System.EventHandler(this.PnlBlank_DoubleClick);

            // ── _lblBlankDesc ─────────────────────────────────────────────────
            this._lblBlankDesc.Text      = "สร้าง Folder เปล่า — กำหนดชื่อและระดับเองได้อิสระ";
            this._lblBlankDesc.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            this._lblBlankDesc.ForeColor = System.Drawing.Color.FromArgb(90, 90, 90);
            this._lblBlankDesc.AutoSize  = false;
            this._lblBlankDesc.Location  = new System.Drawing.Point(14, 96);
            this._lblBlankDesc.Size      = new System.Drawing.Size(172, 30);
            this._lblBlankDesc.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this._lblBlankDesc.MouseEnter  += new System.EventHandler(this.PnlBlank_MouseEnter);
            this._lblBlankDesc.MouseLeave  += new System.EventHandler(this.PnlBlank_MouseLeave);
            this._lblBlankDesc.Click       += new System.EventHandler(this.PnlBlank_Click);
            this._lblBlankDesc.DoubleClick += new System.EventHandler(this.PnlBlank_DoubleClick);

            // ── _pnlFooter ────────────────────────────────────────────────────
            this._pnlFooter.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this._pnlFooter.Height    = 52;
            this._pnlFooter.BackColor = System.Drawing.Color.FromArgb(238, 240, 245);
            this._pnlFooter.Controls.Add(this._line);
            this._pnlFooter.Controls.Add(this._btnOk);
            this._pnlFooter.Controls.Add(this._btnCancel);
            this._pnlFooter.Resize    += new System.EventHandler(this.PnlFooter_Resize);

            // ── _line ─────────────────────────────────────────────────────────
            this._line.Dock      = System.Windows.Forms.DockStyle.Top;
            this._line.Height    = 1;
            this._line.BackColor = System.Drawing.Color.FromArgb(210, 215, 225);

            // ── _btnOk ────────────────────────────────────────────────────────
            this._btnOk.Text      = "ตกลง";
            this._btnOk.Size      = new System.Drawing.Size(90, 30);
            this._btnOk.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this._btnOk.ForeColor = System.Drawing.Color.White;
            this._btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOk.Font      = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this._btnOk.Enabled   = false;
            this._btnOk.Anchor    = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this._btnOk.FlatAppearance.BorderSize = 0;
            this._btnOk.Click    += new System.EventHandler(this.BtnOk_Click);

            // ── _btnCancel ────────────────────────────────────────────────────
            this._btnCancel.Text         = "ยกเลิก";
            this._btnCancel.Size         = new System.Drawing.Size(80, 30);
            this._btnCancel.BackColor    = System.Drawing.Color.White;
            this._btnCancel.ForeColor    = System.Drawing.Color.FromArgb(60, 60, 60);
            this._btnCancel.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Anchor       = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this._btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(190, 190, 190);

            // ── Form ──────────────────────────────────────────────────────────
            this.Text            = "วิธีสร้าง Folder";
            this.ClientSize      = new System.Drawing.Size(480, 310);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = System.Drawing.Color.FromArgb(245, 247, 250);
            this.Font            = new System.Drawing.Font("Segoe UI", 9f);
            this.Name            = "FolderCreationChoiceDialog";
            this.AcceptButton    = this._btnOk;
            this.CancelButton    = this._btnCancel;
            this.Controls.Add(this._pnlCards);
            this.Controls.Add(this._pnlFooter);
            this.Controls.Add(this._pnlHeader);

            this._pnlHeader.ResumeLayout(false);
            this._pnlCards.ResumeLayout(false);
            this._pnlTemplate.ResumeLayout(false);
            this._pnlBlank.ResumeLayout(false);
            this._pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
