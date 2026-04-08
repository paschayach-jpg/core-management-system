using System;

namespace CMS.Presentation
{
    partial class ArticleEditorDialog
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

            this.pnlHeader       = new System.Windows.Forms.Panel();
            this.lblHeader       = new System.Windows.Forms.Label();

            this.pnlMain         = new System.Windows.Forms.Panel();

            // Left meta panel
            this.pnlMeta         = new System.Windows.Forms.Panel();
            this.lblCode         = new System.Windows.Forms.Label();
            this.txtCode         = new System.Windows.Forms.TextBox();
            this.lblCategory     = new System.Windows.Forms.Label();
            this.cboCategory     = new System.Windows.Forms.ComboBox();
            this.lblType         = new System.Windows.Forms.Label();
            this.cboType         = new System.Windows.Forms.ComboBox();
            this.lblStatus       = new System.Windows.Forms.Label();
            this.cboStatus       = new System.Windows.Forms.ComboBox();
            this.lblTags         = new System.Windows.Forms.Label();
            this.txtTags         = new System.Windows.Forms.TextBox();
            this.lblExpiry       = new System.Windows.Forms.Label();
            this.dtpExpiry       = new System.Windows.Forms.DateTimePicker();
            this.chkNoExpiry     = new System.Windows.Forms.CheckBox();
            this.chkOfficialPolicy = new System.Windows.Forms.CheckBox();
            this.lblViewCount    = new System.Windows.Forms.Label();
            this.lblLikeCount    = new System.Windows.Forms.Label();

            // Divider
            this.pnlDivider      = new System.Windows.Forms.Panel();

            // Right content panel
            this.pnlContent      = new System.Windows.Forms.Panel();
            this.lblTitle        = new System.Windows.Forms.Label();
            this.txtTitle        = new System.Windows.Forms.TextBox();
            this.lblSummary      = new System.Windows.Forms.Label();
            this.txtSummary      = new System.Windows.Forms.TextBox();
            this.lblContent      = new System.Windows.Forms.Label();
            this.txtContent      = new System.Windows.Forms.RichTextBox();

            // Version history tab
            this.tabControl      = new System.Windows.Forms.TabControl();
            this.tabContent      = new System.Windows.Forms.TabPage();
            this.tabVersions     = new System.Windows.Forms.TabPage();
            this.lstVersions     = new System.Windows.Forms.ListView();
            this.colVerNum       = new System.Windows.Forms.ColumnHeader();
            this.colVerDate      = new System.Windows.Forms.ColumnHeader();
            this.colVerBy        = new System.Windows.Forms.ColumnHeader();
            this.colVerNote      = new System.Windows.Forms.ColumnHeader();

            // Bottom buttons
            this.pnlBottom       = new System.Windows.Forms.Panel();
            this.btnSave         = new System.Windows.Forms.Button();
            this.btnPublish      = new System.Windows.Forms.Button();
            this.btnArchive      = new System.Windows.Forms.Button();
            this.btnCancel       = new System.Windows.Forms.Button();
            this.lblChangeDesc   = new System.Windows.Forms.Label();
            this.txtChangeDesc   = new System.Windows.Forms.TextBox();

            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlMeta.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabContent.SuspendLayout();
            this.tabVersions.SuspendLayout();
            this.SuspendLayout();

            // ════════════════════════════════════════════
            // pnlHeader
            // ════════════════════════════════════════════
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 48;
            this.pnlHeader.Controls.Add(this.lblHeader);

            this.lblHeader.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Text      = "  📄 บทความ Knowledge Base";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ════════════════════════════════════════════
            // pnlBottom (buttons)
            // ════════════════════════════════════════════
            this.pnlBottom.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Height    = 52;
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(240, 244, 250);
            this.pnlBottom.Padding   = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.pnlBottom.Controls.Add(this.btnCancel);
            this.pnlBottom.Controls.Add(this.btnArchive);
            this.pnlBottom.Controls.Add(this.btnPublish);
            this.pnlBottom.Controls.Add(this.btnSave);
            this.pnlBottom.Controls.Add(this.txtChangeDesc);
            this.pnlBottom.Controls.Add(this.lblChangeDesc);

            this.lblChangeDesc.AutoSize  = false;
            this.lblChangeDesc.Dock      = System.Windows.Forms.DockStyle.Left;
            this.lblChangeDesc.Width     = 90;
            this.lblChangeDesc.Text      = "หมายเหตุ:";
            this.lblChangeDesc.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblChangeDesc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.txtChangeDesc.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.txtChangeDesc.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.txtChangeDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            this.btnSave.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnSave.Width     = 110;
            this.btnSave.Text      = "💾 บันทึก";
            this.btnSave.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnSave.Click    += new System.EventHandler(this.BtnSave_Click);

            this.btnPublish.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnPublish.Width     = 120;
            this.btnPublish.Text      = "✅ เผยแพร่";
            this.btnPublish.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnPublish.BackColor = System.Drawing.Color.FromArgb(34, 139, 34);
            this.btnPublish.ForeColor = System.Drawing.Color.White;
            this.btnPublish.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPublish.FlatAppearance.BorderSize = 0;
            this.btnPublish.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnPublish.Click    += new System.EventHandler(this.BtnPublish_Click);

            this.btnArchive.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnArchive.Width     = 110;
            this.btnArchive.Text      = "📦 Archive";
            this.btnArchive.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnArchive.BackColor = System.Drawing.Color.FromArgb(139, 69, 19);
            this.btnArchive.ForeColor = System.Drawing.Color.White;
            this.btnArchive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnArchive.FlatAppearance.BorderSize = 0;
            this.btnArchive.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnArchive.Visible   = false;
            this.btnArchive.Click    += new System.EventHandler(this.BtnArchive_Click);

            this.btnCancel.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Width     = 90;
            this.btnCancel.Text      = "ปิด";
            this.btnCancel.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(210, 210, 210);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Click    += new System.EventHandler(this.BtnCancel_Click);

            // ════════════════════════════════════════════
            // pnlMain (Fill)
            // ════════════════════════════════════════════
            this.pnlMain.Dock    = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Padding = new System.Windows.Forms.Padding(0);
            this.pnlMain.Controls.Add(this.tabControl);
            this.pnlMain.Controls.Add(this.pnlDivider);
            this.pnlMain.Controls.Add(this.pnlMeta);

            // ── pnlMeta (Left, 220px) ─────────────────
            this.pnlMeta.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlMeta.Width     = 220;
            this.pnlMeta.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlMeta.Padding   = new System.Windows.Forms.Padding(12, 12, 10, 8);

            // ── lblCode / txtCode  (metaY offset: 10 → 64) ──────────────────
            this.lblCode.Text      = "รหัสบทความ";
            this.lblCode.AutoSize  = false;
            this.lblCode.Location  = new System.Drawing.Point(12, 10);
            this.lblCode.Size      = new System.Drawing.Size(196, 18);
            this.lblCode.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCode.ForeColor = System.Drawing.Color.FromArgb(70, 90, 120);
            this.pnlMeta.Controls.Add(this.lblCode);

            this.txtCode.ReadOnly  = true;
            this.txtCode.BackColor = System.Drawing.Color.FromArgb(235, 238, 244);
            this.txtCode.Location  = new System.Drawing.Point(12, 30);
            this.txtCode.Size      = new System.Drawing.Size(196, 24);
            this.txtCode.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.pnlMeta.Controls.Add(this.txtCode);

            // ── lblCategory / cboCategory  (metaY offset: 64 → 118) ─────────
            this.lblCategory.Text      = "หมวดหมู่ *";
            this.lblCategory.AutoSize  = false;
            this.lblCategory.Location  = new System.Drawing.Point(12, 64);
            this.lblCategory.Size      = new System.Drawing.Size(196, 18);
            this.lblCategory.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCategory.ForeColor = System.Drawing.Color.FromArgb(70, 90, 120);
            this.pnlMeta.Controls.Add(this.lblCategory);

            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.Location  = new System.Drawing.Point(12, 84);
            this.cboCategory.Size      = new System.Drawing.Size(196, 24);
            this.cboCategory.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.pnlMeta.Controls.Add(this.cboCategory);

            // ── lblType / cboType  (metaY offset: 118 → 172) ────────────────
            this.lblType.Text      = "ประเภทบทความ *";
            this.lblType.AutoSize  = false;
            this.lblType.Location  = new System.Drawing.Point(12, 118);
            this.lblType.Size      = new System.Drawing.Size(196, 18);
            this.lblType.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblType.ForeColor = System.Drawing.Color.FromArgb(70, 90, 120);
            this.pnlMeta.Controls.Add(this.lblType);

            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.Location  = new System.Drawing.Point(12, 138);
            this.cboType.Size      = new System.Drawing.Size(196, 24);
            this.cboType.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.pnlMeta.Controls.Add(this.cboType);

            // ── lblStatus / cboStatus  (metaY offset: 172 → 226) ────────────
            this.lblStatus.Text      = "สถานะ";
            this.lblStatus.AutoSize  = false;
            this.lblStatus.Location  = new System.Drawing.Point(12, 172);
            this.lblStatus.Size      = new System.Drawing.Size(196, 18);
            this.lblStatus.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(70, 90, 120);
            this.pnlMeta.Controls.Add(this.lblStatus);

            this.cboStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStatus.Location  = new System.Drawing.Point(12, 192);
            this.cboStatus.Size      = new System.Drawing.Size(196, 24);
            this.cboStatus.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.pnlMeta.Controls.Add(this.cboStatus);

            // ── lblTags / txtTags  (metaY offset: 226 → 280) ────────────────
            this.lblTags.Text      = "Tags (คั่นด้วย ,)";
            this.lblTags.AutoSize  = false;
            this.lblTags.Location  = new System.Drawing.Point(12, 226);
            this.lblTags.Size      = new System.Drawing.Size(196, 18);
            this.lblTags.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTags.ForeColor = System.Drawing.Color.FromArgb(70, 90, 120);
            this.pnlMeta.Controls.Add(this.lblTags);

            this.txtTags.Location  = new System.Drawing.Point(12, 246);
            this.txtTags.Size      = new System.Drawing.Size(196, 24);
            this.txtTags.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.pnlMeta.Controls.Add(this.txtTags);

            // ── lblExpiry / dtpExpiry  (metaY offset: 280 → 334) ────────────
            this.lblExpiry.Text      = "วันหมดอายุ";
            this.lblExpiry.AutoSize  = false;
            this.lblExpiry.Location  = new System.Drawing.Point(12, 280);
            this.lblExpiry.Size      = new System.Drawing.Size(196, 18);
            this.lblExpiry.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblExpiry.ForeColor = System.Drawing.Color.FromArgb(70, 90, 120);
            this.pnlMeta.Controls.Add(this.lblExpiry);

            this.dtpExpiry.Format   = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpExpiry.MinDate  = DateTime.Today;
            this.dtpExpiry.Location = new System.Drawing.Point(12, 300);
            this.dtpExpiry.Size     = new System.Drawing.Size(196, 24);
            this.dtpExpiry.Font     = new System.Drawing.Font("Segoe UI", 8.5F);
            this.pnlMeta.Controls.Add(this.dtpExpiry);

            // ── chkNoExpiry  (offset: 334) ───────────────────────────────────
            this.chkNoExpiry.Location       = new System.Drawing.Point(12, 334);
            this.chkNoExpiry.Size           = new System.Drawing.Size(196, 20);
            this.chkNoExpiry.Text           = "ไม่มีวันหมดอายุ";
            this.chkNoExpiry.Font           = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chkNoExpiry.Checked        = true;
            this.chkNoExpiry.CheckedChanged += new System.EventHandler(this.ChkNoExpiry_CheckedChanged);
            this.pnlMeta.Controls.Add(this.chkNoExpiry);

            // ── chkOfficialPolicy  (offset: 360) ─────────────────────────────
            this.chkOfficialPolicy.Location = new System.Drawing.Point(12, 360);
            this.chkOfficialPolicy.Size     = new System.Drawing.Size(196, 20);
            this.chkOfficialPolicy.Text     = "🏛 Official Policy";
            this.chkOfficialPolicy.Font     = new System.Drawing.Font("Segoe UI", 8.5F);
            this.pnlMeta.Controls.Add(this.chkOfficialPolicy);

            // ── lblViewCount  (offset: 390) ──────────────────────────────────
            this.lblViewCount.Location  = new System.Drawing.Point(12, 390);
            this.lblViewCount.Size      = new System.Drawing.Size(196, 18);
            this.lblViewCount.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblViewCount.ForeColor = System.Drawing.Color.Gray;
            this.pnlMeta.Controls.Add(this.lblViewCount);

            // ── lblLikeCount  (offset: 410) ──────────────────────────────────
            this.lblLikeCount.Location  = new System.Drawing.Point(12, 410);
            this.lblLikeCount.Size      = new System.Drawing.Size(196, 18);
            this.lblLikeCount.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblLikeCount.ForeColor = System.Drawing.Color.Gray;
            this.pnlMeta.Controls.Add(this.lblLikeCount);

            // ── pnlDivider ────────────────────────────
            this.pnlDivider.Dock      = System.Windows.Forms.DockStyle.Left;
            this.pnlDivider.Width     = 1;
            this.pnlDivider.BackColor = System.Drawing.Color.FromArgb(200, 210, 225);

            // ── tabControl (Right content, Fill) ──────
            this.tabControl.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.tabControl.TabPages.Add(this.tabContent);
            this.tabControl.TabPages.Add(this.tabVersions);

            // tabContent
            this.tabContent.Text     = "📝 เนื้อหาบทความ";
            this.tabContent.Padding  = new System.Windows.Forms.Padding(8);
            this.tabContent.BackColor = System.Drawing.Color.White;
            this.tabContent.Controls.Add(this.pnlContent);

            this.pnlContent.Dock    = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlContent.BackColor = System.Drawing.Color.White;
            this.pnlContent.Controls.Add(this.txtContent);
            this.pnlContent.Controls.Add(this.lblContent);
            this.pnlContent.Controls.Add(this.txtSummary);
            this.pnlContent.Controls.Add(this.lblSummary);
            this.pnlContent.Controls.Add(this.txtTitle);
            this.pnlContent.Controls.Add(this.lblTitle);

            this.lblTitle.AutoSize  = false;
            this.lblTitle.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Height    = 20;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(70, 90, 120);
            this.lblTitle.Text      = "ชื่อบทความ *";

            this.txtTitle.Dock      = System.Windows.Forms.DockStyle.Top;
            this.txtTitle.Height    = 28;
            this.txtTitle.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.txtTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTitle.MaxLength = 500;

            this.lblSummary.AutoSize  = false;
            this.lblSummary.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblSummary.Height    = 20;
            this.lblSummary.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(70, 90, 120);
            this.lblSummary.Text      = "บทสรุปสั้น (Summary)";

            this.txtSummary.Dock      = System.Windows.Forms.DockStyle.Top;
            this.txtSummary.Height    = 54;
            this.txtSummary.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSummary.Multiline = true;
            this.txtSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSummary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSummary.MaxLength  = 1000;

            this.lblContent.AutoSize  = false;
            this.lblContent.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblContent.Height    = 20;
            this.lblContent.Font      = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblContent.ForeColor = System.Drawing.Color.FromArgb(70, 90, 120);
            this.lblContent.Text      = "เนื้อหาบทความ (HTML / Plain Text)";

            this.txtContent.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.txtContent.Font      = new System.Drawing.Font("Consolas", 9.5F);
            this.txtContent.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtContent.BackColor = System.Drawing.Color.FromArgb(252, 253, 255);
            this.txtContent.DetectUrls = false;

            // tabVersions
            this.tabVersions.Text      = "🕐 ประวัติ Version";
            this.tabVersions.BackColor = System.Drawing.Color.White;
            this.tabVersions.Controls.Add(this.lstVersions);

            this.lstVersions.Dock          = System.Windows.Forms.DockStyle.Fill;
            this.lstVersions.View          = System.Windows.Forms.View.Details;
            this.lstVersions.FullRowSelect = true;
            this.lstVersions.GridLines     = false;
            this.lstVersions.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.lstVersions.BorderStyle   = System.Windows.Forms.BorderStyle.None;
            this.lstVersions.HeaderStyle   = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;

            this.colVerNum.Text  = "Version";
            this.colVerNum.Width = 70;
            this.colVerDate.Text  = "วันที่บันทึก";
            this.colVerDate.Width = 130;
            this.colVerBy.Text  = "แก้ไขโดย";
            this.colVerBy.Width = 140;
            this.colVerNote.Text  = "หมายเหตุ";
            this.colVerNote.Width = 300;

            this.lstVersions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
            {
                this.colVerNum, this.colVerDate, this.colVerBy, this.colVerNote
            });

            // ════════════════════════════════════════════
            // Form
            // ════════════════════════════════════════════
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.White;
            this.ClientSize          = new System.Drawing.Size(900, 620);
            this.MinimumSize         = new System.Drawing.Size(760, 520);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlHeader);
            this.Font                = new System.Drawing.Font("Segoe UI", 9F);
            this.Name                = "ArticleEditorDialog";
            this.Text                = "แก้ไขบทความ — Knowledge Base";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load               += new System.EventHandler(this.ArticleEditorDialog_Load);

            this.pnlHeader.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlMeta.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabContent.ResumeLayout(false);
            this.tabVersions.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        // ── Declarations ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel          pnlHeader;
        private System.Windows.Forms.Label          lblHeader;
        private System.Windows.Forms.Panel          pnlMain;
        private System.Windows.Forms.Panel          pnlMeta;
        private System.Windows.Forms.Label          lblCode;
        private System.Windows.Forms.TextBox        txtCode;
        private System.Windows.Forms.Label          lblCategory;
        private System.Windows.Forms.ComboBox       cboCategory;
        private System.Windows.Forms.Label          lblType;
        private System.Windows.Forms.ComboBox       cboType;
        private System.Windows.Forms.Label          lblStatus;
        private System.Windows.Forms.ComboBox       cboStatus;
        private System.Windows.Forms.Label          lblTags;
        private System.Windows.Forms.TextBox        txtTags;
        private System.Windows.Forms.Label          lblExpiry;
        private System.Windows.Forms.DateTimePicker dtpExpiry;
        private System.Windows.Forms.CheckBox       chkNoExpiry;
        private System.Windows.Forms.CheckBox       chkOfficialPolicy;
        private System.Windows.Forms.Label          lblViewCount;
        private System.Windows.Forms.Label          lblLikeCount;
        private System.Windows.Forms.Panel          pnlDivider;
        private System.Windows.Forms.TabControl     tabControl;
        private System.Windows.Forms.TabPage        tabContent;
        private System.Windows.Forms.TabPage        tabVersions;
        private System.Windows.Forms.Panel          pnlContent;
        private System.Windows.Forms.Label          lblTitle;
        private System.Windows.Forms.TextBox        txtTitle;
        private System.Windows.Forms.Label          lblSummary;
        private System.Windows.Forms.TextBox        txtSummary;
        private System.Windows.Forms.Label          lblContent;
        private System.Windows.Forms.RichTextBox    txtContent;
        private System.Windows.Forms.ListView       lstVersions;
        private System.Windows.Forms.ColumnHeader   colVerNum;
        private System.Windows.Forms.ColumnHeader   colVerDate;
        private System.Windows.Forms.ColumnHeader   colVerBy;
        private System.Windows.Forms.ColumnHeader   colVerNote;
        private System.Windows.Forms.Panel          pnlBottom;
        private System.Windows.Forms.Label          lblChangeDesc;
        private System.Windows.Forms.TextBox        txtChangeDesc;
        private System.Windows.Forms.Button         btnSave;
        private System.Windows.Forms.Button         btnPublish;
        private System.Windows.Forms.Button         btnArchive;
        private System.Windows.Forms.Button         btnCancel;
    }
}
