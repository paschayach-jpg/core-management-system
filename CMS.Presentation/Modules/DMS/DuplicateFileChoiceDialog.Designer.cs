namespace CMS.Presentation
{
    partial class DuplicateFileChoiceDialog
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel _pnlHeader;
        private System.Windows.Forms.Label _lblTitle;
        private System.Windows.Forms.Label _lblSub;
        private System.Windows.Forms.Panel _pnlContent;
        private System.Windows.Forms.Label _lblSummary;
        private System.Windows.Forms.Panel _pnlSaveCopy;
        private System.Windows.Forms.Label _lblSaveCopyIcon;
        private System.Windows.Forms.Label _lblSaveCopyTitle;
        private System.Windows.Forms.Label _lblSaveCopyDesc;
        private System.Windows.Forms.Panel _pnlSkip;
        private System.Windows.Forms.Label _lblSkipIcon;
        private System.Windows.Forms.Label _lblSkipTitle;
        private System.Windows.Forms.Label _lblSkipDesc;
        private System.Windows.Forms.Label _lblListTitle;
        private System.Windows.Forms.ListBox _lstDuplicates;
        private System.Windows.Forms.Panel _pnlFooter;
        private System.Windows.Forms.Panel _line;
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
            this.components = new System.ComponentModel.Container();
            this._pnlHeader = new System.Windows.Forms.Panel();
            this._lblTitle = new System.Windows.Forms.Label();
            this._lblSub = new System.Windows.Forms.Label();
            this._pnlContent = new System.Windows.Forms.Panel();
            this._lblSummary = new System.Windows.Forms.Label();
            this._pnlSaveCopy = new System.Windows.Forms.Panel();
            this._lblSaveCopyIcon = new System.Windows.Forms.Label();
            this._lblSaveCopyTitle = new System.Windows.Forms.Label();
            this._lblSaveCopyDesc = new System.Windows.Forms.Label();
            this._pnlSkip = new System.Windows.Forms.Panel();
            this._lblSkipIcon = new System.Windows.Forms.Label();
            this._lblSkipTitle = new System.Windows.Forms.Label();
            this._lblSkipDesc = new System.Windows.Forms.Label();
            this._lblListTitle = new System.Windows.Forms.Label();
            this._lstDuplicates = new System.Windows.Forms.ListBox();
            this._pnlFooter = new System.Windows.Forms.Panel();
            this._line = new System.Windows.Forms.Panel();
            this._btnOk = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._pnlHeader.SuspendLayout();
            this._pnlContent.SuspendLayout();
            this._pnlSaveCopy.SuspendLayout();
            this._pnlSkip.SuspendLayout();
            this._pnlFooter.SuspendLayout();
            this.SuspendLayout();

            this._pnlHeader.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this._pnlHeader.Controls.Add(this._lblSub);
            this._pnlHeader.Controls.Add(this._lblTitle);
            this._pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this._pnlHeader.Height = 56;

            this._lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this._lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this._lblTitle.ForeColor = System.Drawing.Color.White;
            this._lblTitle.Height = 30;
            this._lblTitle.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this._lblTitle.Text = "พบไฟล์ซ้ำ";
            this._lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this._lblSub.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblSub.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblSub.ForeColor = System.Drawing.Color.FromArgb(180, 210, 255);
            this._lblSub.Padding = new System.Windows.Forms.Padding(18, 0, 0, 4);
            this._lblSub.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this._pnlContent.BackColor = System.Drawing.Color.Transparent;
            this._pnlContent.Controls.Add(this._lstDuplicates);
            this._pnlContent.Controls.Add(this._lblListTitle);
            this._pnlContent.Controls.Add(this._pnlSkip);
            this._pnlContent.Controls.Add(this._pnlSaveCopy);
            this._pnlContent.Controls.Add(this._lblSummary);
            this._pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlContent.Padding = new System.Windows.Forms.Padding(20, 16, 20, 10);

            this._lblSummary.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this._lblSummary.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70);
            this._lblSummary.Location = new System.Drawing.Point(20, 16);
            this._lblSummary.Size = new System.Drawing.Size(520, 22);

            this._pnlSaveCopy.BackColor = System.Drawing.Color.White;
            this._pnlSaveCopy.Controls.Add(this._lblSaveCopyIcon);
            this._pnlSaveCopy.Controls.Add(this._lblSaveCopyTitle);
            this._pnlSaveCopy.Controls.Add(this._lblSaveCopyDesc);
            this._pnlSaveCopy.Cursor = System.Windows.Forms.Cursors.Hand;
            this._pnlSaveCopy.Location = new System.Drawing.Point(20, 48);
            this._pnlSaveCopy.Size = new System.Drawing.Size(250, 128);
            this._pnlSaveCopy.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlSaveCopy_Paint);
            this._pnlSaveCopy.MouseEnter += new System.EventHandler(this.PnlSaveCopy_MouseEnter);
            this._pnlSaveCopy.MouseLeave += new System.EventHandler(this.PnlSaveCopy_MouseLeave);
            this._pnlSaveCopy.Click += new System.EventHandler(this.PnlSaveCopy_Click);
            this._pnlSaveCopy.DoubleClick += new System.EventHandler(this.PnlSaveCopy_DoubleClick);

            this._lblSaveCopyIcon.Font = new System.Drawing.Font("Segoe UI Symbol", 28F);
            this._lblSaveCopyIcon.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this._lblSaveCopyIcon.Location = new System.Drawing.Point(16, 14);
            this._lblSaveCopyIcon.Size = new System.Drawing.Size(56, 56);
            this._lblSaveCopyIcon.Text = "⧉";
            this._lblSaveCopyIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._lblSaveCopyIcon.Click += new System.EventHandler(this.PnlSaveCopy_Click);
            this._lblSaveCopyIcon.DoubleClick += new System.EventHandler(this.PnlSaveCopy_DoubleClick);
            this._lblSaveCopyIcon.MouseEnter += new System.EventHandler(this.PnlSaveCopy_MouseEnter);
            this._lblSaveCopyIcon.MouseLeave += new System.EventHandler(this.PnlSaveCopy_MouseLeave);

            this._lblSaveCopyTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._lblSaveCopyTitle.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this._lblSaveCopyTitle.Location = new System.Drawing.Point(16, 74);
            this._lblSaveCopyTitle.Size = new System.Drawing.Size(214, 22);
            this._lblSaveCopyTitle.Text = "บันทึกสำเนา";
            this._lblSaveCopyTitle.Click += new System.EventHandler(this.PnlSaveCopy_Click);
            this._lblSaveCopyTitle.DoubleClick += new System.EventHandler(this.PnlSaveCopy_DoubleClick);
            this._lblSaveCopyTitle.MouseEnter += new System.EventHandler(this.PnlSaveCopy_MouseEnter);
            this._lblSaveCopyTitle.MouseLeave += new System.EventHandler(this.PnlSaveCopy_MouseLeave);

            this._lblSaveCopyDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._lblSaveCopyDesc.ForeColor = System.Drawing.Color.FromArgb(90, 90, 90);
            this._lblSaveCopyDesc.Location = new System.Drawing.Point(16, 96);
            this._lblSaveCopyDesc.Size = new System.Drawing.Size(214, 26);
            this._lblSaveCopyDesc.Text = "อัพโหลดต่อและตั้งชื่อใหม่อัตโนมัติ เช่น copy(1), copy(2)";
            this._lblSaveCopyDesc.Click += new System.EventHandler(this.PnlSaveCopy_Click);
            this._lblSaveCopyDesc.DoubleClick += new System.EventHandler(this.PnlSaveCopy_DoubleClick);
            this._lblSaveCopyDesc.MouseEnter += new System.EventHandler(this.PnlSaveCopy_MouseEnter);
            this._lblSaveCopyDesc.MouseLeave += new System.EventHandler(this.PnlSaveCopy_MouseLeave);

            this._pnlSkip.BackColor = System.Drawing.Color.White;
            this._pnlSkip.Controls.Add(this._lblSkipIcon);
            this._pnlSkip.Controls.Add(this._lblSkipTitle);
            this._pnlSkip.Controls.Add(this._lblSkipDesc);
            this._pnlSkip.Cursor = System.Windows.Forms.Cursors.Hand;
            this._pnlSkip.Location = new System.Drawing.Point(290, 48);
            this._pnlSkip.Size = new System.Drawing.Size(250, 128);
            this._pnlSkip.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlSkip_Paint);
            this._pnlSkip.MouseEnter += new System.EventHandler(this.PnlSkip_MouseEnter);
            this._pnlSkip.MouseLeave += new System.EventHandler(this.PnlSkip_MouseLeave);
            this._pnlSkip.Click += new System.EventHandler(this.PnlSkip_Click);
            this._pnlSkip.DoubleClick += new System.EventHandler(this.PnlSkip_DoubleClick);

            this._lblSkipIcon.Font = new System.Drawing.Font("Segoe UI Symbol", 28F);
            this._lblSkipIcon.ForeColor = System.Drawing.Color.FromArgb(255, 160, 0);
            this._lblSkipIcon.Location = new System.Drawing.Point(16, 14);
            this._lblSkipIcon.Size = new System.Drawing.Size(56, 56);
            this._lblSkipIcon.Text = "↷";
            this._lblSkipIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._lblSkipIcon.Click += new System.EventHandler(this.PnlSkip_Click);
            this._lblSkipIcon.DoubleClick += new System.EventHandler(this.PnlSkip_DoubleClick);
            this._lblSkipIcon.MouseEnter += new System.EventHandler(this.PnlSkip_MouseEnter);
            this._lblSkipIcon.MouseLeave += new System.EventHandler(this.PnlSkip_MouseLeave);

            this._lblSkipTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._lblSkipTitle.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this._lblSkipTitle.Location = new System.Drawing.Point(16, 74);
            this._lblSkipTitle.Size = new System.Drawing.Size(214, 22);
            this._lblSkipTitle.Text = "ข้ามไฟล์ซ้ำ";
            this._lblSkipTitle.Click += new System.EventHandler(this.PnlSkip_Click);
            this._lblSkipTitle.DoubleClick += new System.EventHandler(this.PnlSkip_DoubleClick);
            this._lblSkipTitle.MouseEnter += new System.EventHandler(this.PnlSkip_MouseEnter);
            this._lblSkipTitle.MouseLeave += new System.EventHandler(this.PnlSkip_MouseLeave);

            this._lblSkipDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._lblSkipDesc.ForeColor = System.Drawing.Color.FromArgb(90, 90, 90);
            this._lblSkipDesc.Location = new System.Drawing.Point(16, 96);
            this._lblSkipDesc.Size = new System.Drawing.Size(214, 26);
            this._lblSkipDesc.Text = "ตัดไฟล์ที่ซ้ำออก แล้วอัพโหลดเฉพาะไฟล์ใหม่เท่านั้น";
            this._lblSkipDesc.Click += new System.EventHandler(this.PnlSkip_Click);
            this._lblSkipDesc.DoubleClick += new System.EventHandler(this.PnlSkip_DoubleClick);
            this._lblSkipDesc.MouseEnter += new System.EventHandler(this.PnlSkip_MouseEnter);
            this._lblSkipDesc.MouseLeave += new System.EventHandler(this.PnlSkip_MouseLeave);

            this._lblListTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this._lblListTitle.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this._lblListTitle.Location = new System.Drawing.Point(20, 192);
            this._lblListTitle.Size = new System.Drawing.Size(160, 22);
            this._lblListTitle.Text = "รายการไฟล์ที่ซ้ำ";

            this._lstDuplicates.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._lstDuplicates.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lstDuplicates.FormattingEnabled = true;
            this._lstDuplicates.HorizontalScrollbar = true;
            this._lstDuplicates.IntegralHeight = false;
            this._lstDuplicates.Location = new System.Drawing.Point(20, 218);
            this._lstDuplicates.Size = new System.Drawing.Size(520, 120);

            this._pnlFooter.BackColor = System.Drawing.Color.FromArgb(238, 240, 245);
            this._pnlFooter.Controls.Add(this._line);
            this._pnlFooter.Controls.Add(this._btnOk);
            this._pnlFooter.Controls.Add(this._btnCancel);
            this._pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._pnlFooter.Height = 52;
            this._pnlFooter.Resize += new System.EventHandler(this.PnlFooter_Resize);

            this._line.BackColor = System.Drawing.Color.FromArgb(210, 215, 225);
            this._line.Dock = System.Windows.Forms.DockStyle.Top;
            this._line.Height = 1;

            this._btnOk.Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this._btnOk.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this._btnOk.Enabled = false;
            this._btnOk.FlatAppearance.BorderSize = 0;
            this._btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOk.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this._btnOk.ForeColor = System.Drawing.Color.White;
            this._btnOk.Size = new System.Drawing.Size(90, 30);
            this._btnOk.Text = "ตกลง";
            this._btnOk.Click += new System.EventHandler(this.BtnOk_Click);

            this._btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this._btnCancel.BackColor = System.Drawing.Color.White;
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(190, 190, 190);
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this._btnCancel.Size = new System.Drawing.Size(80, 30);
            this._btnCancel.Text = "ยกเลิก";

            this.AcceptButton = this._btnOk;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.CancelButton = this._btnCancel;
            this.ClientSize = new System.Drawing.Size(560, 470);
            this.Controls.Add(this._pnlContent);
            this.Controls.Add(this._pnlFooter);
            this.Controls.Add(this._pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DuplicateFileChoiceDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "พบไฟล์ซ้ำ";
            this._pnlHeader.ResumeLayout(false);
            this._pnlContent.ResumeLayout(false);
            this._pnlSaveCopy.ResumeLayout(false);
            this._pnlSkip.ResumeLayout(false);
            this._pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}