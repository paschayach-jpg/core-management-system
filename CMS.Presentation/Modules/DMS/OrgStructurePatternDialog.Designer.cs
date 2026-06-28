namespace CMS.Presentation
{
    partial class OrgStructurePatternDialog
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel  _pnlHeader;
        private System.Windows.Forms.Label  _lblTitle;
        private System.Windows.Forms.Label  _lblSub;
        private System.Windows.Forms.Panel  _pnlCards;
        private System.Windows.Forms.Panel  _pnlDivDep;
        private System.Windows.Forms.Label  _lblDivDepIcon;
        private System.Windows.Forms.Label  _lblDivDepTitle;
        private System.Windows.Forms.Label  _lblDivDepDesc;
        private System.Windows.Forms.Panel  _pnlLevel0;
        private System.Windows.Forms.Label  _lblLevel0Icon;
        private System.Windows.Forms.Label  _lblLevel0Title;
        private System.Windows.Forms.Label  _lblLevel0Desc;
        private System.Windows.Forms.Panel  _pnlFooter;
        private System.Windows.Forms.Panel  _line;
        private System.Windows.Forms.Button _btnOk;
        private System.Windows.Forms.Button _btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components    = new System.ComponentModel.Container();
            this._pnlHeader    = new System.Windows.Forms.Panel();
            this._lblTitle     = new System.Windows.Forms.Label();
            this._lblSub       = new System.Windows.Forms.Label();
            this._pnlCards     = new System.Windows.Forms.Panel();
            this._pnlDivDep    = new System.Windows.Forms.Panel();
            this._lblDivDepIcon  = new System.Windows.Forms.Label();
            this._lblDivDepTitle = new System.Windows.Forms.Label();
            this._lblDivDepDesc  = new System.Windows.Forms.Label();
            this._pnlLevel0    = new System.Windows.Forms.Panel();
            this._lblLevel0Icon  = new System.Windows.Forms.Label();
            this._lblLevel0Title = new System.Windows.Forms.Label();
            this._lblLevel0Desc  = new System.Windows.Forms.Label();
            this._pnlFooter    = new System.Windows.Forms.Panel();
            this._line         = new System.Windows.Forms.Panel();
            this._btnOk        = new System.Windows.Forms.Button();
            this._btnCancel    = new System.Windows.Forms.Button();
            this._pnlHeader.SuspendLayout();
            this._pnlCards.SuspendLayout();
            this._pnlDivDep.SuspendLayout();
            this._pnlLevel0.SuspendLayout();
            this._pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // ── _pnlHeader ────────────────────────────────────────────────────
            this._pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this._pnlHeader.Height    = 52;
            this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this._pnlHeader.Controls.Add(this._lblSub);
            this._pnlHeader.Controls.Add(this._lblTitle);

            // ── _lblTitle ─────────────────────────────────────────────────────
            this._lblTitle.Text      = "เลือกรูปแบบโครงสร้าง";
            this._lblTitle.Dock      = System.Windows.Forms.DockStyle.Top;
            this._lblTitle.Height    = 30;
            this._lblTitle.ForeColor = System.Drawing.Color.White;
            this._lblTitle.Font      = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold);
            this._lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblTitle.Padding   = new System.Windows.Forms.Padding(16, 0, 0, 0);

            // ── _lblSub ───────────────────────────────────────────────────────
            this._lblSub.Text      = "เลือกรูปแบบที่ต้องการใช้สำหรับการสร้างโครงสร้างหน่วยงานนี้";
            this._lblSub.Dock      = System.Windows.Forms.DockStyle.Fill;
            this._lblSub.ForeColor = System.Drawing.Color.FromArgb(180, 210, 255);
            this._lblSub.Font      = new System.Drawing.Font("Segoe UI", 9f);
            this._lblSub.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblSub.Padding   = new System.Windows.Forms.Padding(18, 0, 0, 4);

            // ── _pnlCards ─────────────────────────────────────────────────────
            this._pnlCards.Dock      = System.Windows.Forms.DockStyle.Fill;
            this._pnlCards.BackColor = System.Drawing.Color.Transparent;
            this._pnlCards.Padding   = new System.Windows.Forms.Padding(20, 16, 20, 8);
            this._pnlCards.Controls.Add(this._pnlDivDep);
            this._pnlCards.Controls.Add(this._pnlLevel0);

            // ── _pnlDivDep ────────────────────────────────────────────────────
            this._pnlDivDep.BackColor  = System.Drawing.Color.White;
            this._pnlDivDep.Cursor     = System.Windows.Forms.Cursors.Hand;
            this._pnlDivDep.Padding    = new System.Windows.Forms.Padding(14);
            this._pnlDivDep.Location   = new System.Drawing.Point(20, 16);
            this._pnlDivDep.Size       = new System.Drawing.Size(200, 140);
            this._pnlDivDep.Anchor     = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this._pnlDivDep.Controls.Add(this._lblDivDepIcon);
            this._pnlDivDep.Controls.Add(this._lblDivDepTitle);
            this._pnlDivDep.Controls.Add(this._lblDivDepDesc);
            this._pnlDivDep.Paint       += new System.Windows.Forms.PaintEventHandler(this.PnlDivDep_Paint);
            this._pnlDivDep.MouseEnter  += new System.EventHandler(this.PnlDivDep_MouseEnter);
            this._pnlDivDep.MouseLeave  += new System.EventHandler(this.PnlDivDep_MouseLeave);
            this._pnlDivDep.Click       += new System.EventHandler(this.PnlDivDep_Click);
            this._pnlDivDep.DoubleClick += new System.EventHandler(this.PnlDivDep_DoubleClick);

            // ── _lblDivDepIcon ────────────────────────────────────────────────
            this._lblDivDepIcon.Text      = "\U0001f3e2";
            this._lblDivDepIcon.Font      = new System.Drawing.Font("Segoe UI", 28f);
            this._lblDivDepIcon.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this._lblDivDepIcon.AutoSize  = false;
            this._lblDivDepIcon.Location  = new System.Drawing.Point(14, 14);
            this._lblDivDepIcon.Size      = new System.Drawing.Size(56, 56);
            this._lblDivDepIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._lblDivDepIcon.MouseEnter  += new System.EventHandler(this.PnlDivDep_MouseEnter);
            this._lblDivDepIcon.MouseLeave  += new System.EventHandler(this.PnlDivDep_MouseLeave);
            this._lblDivDepIcon.Click       += new System.EventHandler(this.PnlDivDep_Click);
            this._lblDivDepIcon.DoubleClick += new System.EventHandler(this.PnlDivDep_DoubleClick);

            // ── _lblDivDepTitle ───────────────────────────────────────────────
            this._lblDivDepTitle.Text      = "Sub → Div → Dep  (3 ระดับ)";
            this._lblDivDepTitle.Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this._lblDivDepTitle.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this._lblDivDepTitle.AutoSize  = false;
            this._lblDivDepTitle.Location  = new System.Drawing.Point(14, 74);
            this._lblDivDepTitle.Size      = new System.Drawing.Size(172, 22);
            this._lblDivDepTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblDivDepTitle.MouseEnter  += new System.EventHandler(this.PnlDivDep_MouseEnter);
            this._lblDivDepTitle.MouseLeave  += new System.EventHandler(this.PnlDivDep_MouseLeave);
            this._lblDivDepTitle.Click       += new System.EventHandler(this.PnlDivDep_Click);
            this._lblDivDepTitle.DoubleClick += new System.EventHandler(this.PnlDivDep_DoubleClick);

            // ── _lblDivDepDesc ────────────────────────────────────────────────
            this._lblDivDepDesc.Text      = "SubCommittee เป็น root — Division อยู่ใต้ Sub — Department อยู่ใต้ Division";
            this._lblDivDepDesc.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            this._lblDivDepDesc.ForeColor = System.Drawing.Color.FromArgb(90, 90, 90);
            this._lblDivDepDesc.AutoSize  = false;
            this._lblDivDepDesc.Location  = new System.Drawing.Point(14, 98);
            this._lblDivDepDesc.Size      = new System.Drawing.Size(172, 36);
            this._lblDivDepDesc.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this._lblDivDepDesc.MouseEnter  += new System.EventHandler(this.PnlDivDep_MouseEnter);
            this._lblDivDepDesc.MouseLeave  += new System.EventHandler(this.PnlDivDep_MouseLeave);
            this._lblDivDepDesc.Click       += new System.EventHandler(this.PnlDivDep_Click);
            this._lblDivDepDesc.DoubleClick += new System.EventHandler(this.PnlDivDep_DoubleClick);

            // ── _pnlLevel0 ────────────────────────────────────────────────────
            this._pnlLevel0.BackColor  = System.Drawing.Color.White;
            this._pnlLevel0.Cursor     = System.Windows.Forms.Cursors.Hand;
            this._pnlLevel0.Padding    = new System.Windows.Forms.Padding(14);
            this._pnlLevel0.Location   = new System.Drawing.Point(240, 16);
            this._pnlLevel0.Size       = new System.Drawing.Size(200, 140);
            this._pnlLevel0.Anchor     = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this._pnlLevel0.Controls.Add(this._lblLevel0Icon);
            this._pnlLevel0.Controls.Add(this._lblLevel0Title);
            this._pnlLevel0.Controls.Add(this._lblLevel0Desc);
            this._pnlLevel0.Paint       += new System.Windows.Forms.PaintEventHandler(this.PnlLevel0_Paint);
            this._pnlLevel0.MouseEnter  += new System.EventHandler(this.PnlLevel0_MouseEnter);
            this._pnlLevel0.MouseLeave  += new System.EventHandler(this.PnlLevel0_MouseLeave);
            this._pnlLevel0.Click       += new System.EventHandler(this.PnlLevel0_Click);
            this._pnlLevel0.DoubleClick += new System.EventHandler(this.PnlLevel0_DoubleClick);

            // ── _lblLevel0Icon ────────────────────────────────────────────────
            this._lblLevel0Icon.Text      = "≡";
            this._lblLevel0Icon.Font      = new System.Drawing.Font("Segoe UI", 28f);
            this._lblLevel0Icon.ForeColor = System.Drawing.Color.FromArgb(80, 160, 80);
            this._lblLevel0Icon.AutoSize  = false;
            this._lblLevel0Icon.Location  = new System.Drawing.Point(14, 14);
            this._lblLevel0Icon.Size      = new System.Drawing.Size(56, 56);
            this._lblLevel0Icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._lblLevel0Icon.MouseEnter  += new System.EventHandler(this.PnlLevel0_MouseEnter);
            this._lblLevel0Icon.MouseLeave  += new System.EventHandler(this.PnlLevel0_MouseLeave);
            this._lblLevel0Icon.Click       += new System.EventHandler(this.PnlLevel0_Click);
            this._lblLevel0Icon.DoubleClick += new System.EventHandler(this.PnlLevel0_DoubleClick);

            // ── _lblLevel0Title ───────────────────────────────────────────────
            this._lblLevel0Title.Text      = "Level 0  (ระดับเดียว)";
            this._lblLevel0Title.Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this._lblLevel0Title.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this._lblLevel0Title.AutoSize  = false;
            this._lblLevel0Title.Location  = new System.Drawing.Point(14, 74);
            this._lblLevel0Title.Size      = new System.Drawing.Size(172, 22);
            this._lblLevel0Title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblLevel0Title.MouseEnter  += new System.EventHandler(this.PnlLevel0_MouseEnter);
            this._lblLevel0Title.MouseLeave  += new System.EventHandler(this.PnlLevel0_MouseLeave);
            this._lblLevel0Title.Click       += new System.EventHandler(this.PnlLevel0_Click);
            this._lblLevel0Title.DoubleClick += new System.EventHandler(this.PnlLevel0_DoubleClick);

            // ── _lblLevel0Desc ────────────────────────────────────────────────
            this._lblLevel0Desc.Text      = "โนดทุกตัวอยู่ระดับเดียวกัน — ไม่มีเลเยอร์ย่อย";
            this._lblLevel0Desc.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            this._lblLevel0Desc.ForeColor = System.Drawing.Color.FromArgb(90, 90, 90);
            this._lblLevel0Desc.AutoSize  = false;
            this._lblLevel0Desc.Location  = new System.Drawing.Point(14, 98);
            this._lblLevel0Desc.Size      = new System.Drawing.Size(172, 36);
            this._lblLevel0Desc.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this._lblLevel0Desc.MouseEnter  += new System.EventHandler(this.PnlLevel0_MouseEnter);
            this._lblLevel0Desc.MouseLeave  += new System.EventHandler(this.PnlLevel0_MouseLeave);
            this._lblLevel0Desc.Click       += new System.EventHandler(this.PnlLevel0_Click);
            this._lblLevel0Desc.DoubleClick += new System.EventHandler(this.PnlLevel0_DoubleClick);

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
            this.Text            = "เลือกรูปแบบโครงสร้าง";
            this.ClientSize      = new System.Drawing.Size(480, 320);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = System.Drawing.Color.FromArgb(245, 247, 250);
            this.Font            = new System.Drawing.Font("Segoe UI", 9f);
            this.Name            = "OrgStructurePatternDialog";
            this.AcceptButton    = this._btnOk;
            this.CancelButton    = this._btnCancel;
            this.Controls.Add(this._pnlCards);
            this.Controls.Add(this._pnlFooter);
            this.Controls.Add(this._pnlHeader);

            this._pnlHeader.ResumeLayout(false);
            this._pnlCards.ResumeLayout(false);
            this._pnlDivDep.ResumeLayout(false);
            this._pnlLevel0.ResumeLayout(false);
            this._pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
