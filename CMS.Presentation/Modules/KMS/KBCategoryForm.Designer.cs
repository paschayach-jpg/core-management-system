namespace CMS.Presentation.Modules.KMS
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

            // ── Header ──────────────────────────────────────────────────────────
            this.pnlHeader              = new System.Windows.Forms.Panel();
            this.btnBack                = new System.Windows.Forms.Button();
            this.lblHeader              = new System.Windows.Forms.Label();

            // ── Main split ──────────────────────────────────────────────────────
            this.pnlMain                = new System.Windows.Forms.Panel();
            this.pnlLeft                = new System.Windows.Forms.Panel();
            this.pnlLeftHeader          = new System.Windows.Forms.Panel();
            this.lblListTitle           = new System.Windows.Forms.Label();
            this.btnNew                 = new System.Windows.Forms.Button();
            this.lstCategories          = new System.Windows.Forms.ListView();
            this.colCode                = new System.Windows.Forms.ColumnHeader();
            this.colName                = new System.Windows.Forms.ColumnHeader();
            this.colParent              = new System.Windows.Forms.ColumnHeader();
            this.colSort                = new System.Windows.Forms.ColumnHeader();
            this.colStatus              = new System.Windows.Forms.ColumnHeader();
            this.divider                = new System.Windows.Forms.Panel();

            // ── Right scrollable panel ───────────────────────────────────────────
            this.pnlRight               = new System.Windows.Forms.Panel();

            // ── Edit Card ───────────────────────────────────────────────────────
            this.pnlEditCard            = new System.Windows.Forms.Panel();
            this.pnlEditCardHeader      = new System.Windows.Forms.Panel();
            this.lblEditCardHeader      = new System.Windows.Forms.Label();
            this.pnlEditCardBody        = new System.Windows.Forms.Panel();
            this.lblName                = new System.Windows.Forms.Label();
            this.txtName                = new System.Windows.Forms.TextBox();
            this.lblIcon                = new System.Windows.Forms.Label();
            this.cboIcon                = new System.Windows.Forms.ComboBox();
            this.lblIconPreview         = new System.Windows.Forms.Label();
            this.lblIconFullPreview     = new System.Windows.Forms.Label();
            this.lblSortOrder           = new System.Windows.Forms.Label();
            this.numSortOrder           = new System.Windows.Forms.NumericUpDown();
            this.lblStatusLabel         = new System.Windows.Forms.Label();
            this.cboStatus              = new System.Windows.Forms.ComboBox();
            this.lblParent              = new System.Windows.Forms.Label();
            this.cboParent              = new System.Windows.Forms.ComboBox();
            this.lblDescription         = new System.Windows.Forms.Label();
            this.txtDescription         = new System.Windows.Forms.TextBox();
            this.pnlButtons             = new System.Windows.Forms.Panel();
            this.btnSave                = new System.Windows.Forms.Button();
            this.btnDelete              = new System.Windows.Forms.Button();
            this.btnClear               = new System.Windows.Forms.Button();
            this.lblDirtyHint           = new System.Windows.Forms.Label();

            // ── Spacers ─────────────────────────────────────────────────────────
            this.pnlSpacer1             = new System.Windows.Forms.Panel();
            this.pnlSpacer2             = new System.Windows.Forms.Panel();
            this.pnlSpacer3             = new System.Windows.Forms.Panel();

            // ── Details Card ────────────────────────────────────────────────────
            this.pnlDetailsCard         = new System.Windows.Forms.Panel();
            this.pnlDetailsCardHeader   = new System.Windows.Forms.Panel();
            this.lblDetailsCardHeader   = new System.Windows.Forms.Label();
            this.pnlDetailsCardBody     = new System.Windows.Forms.Panel();
            this.tblStats               = new System.Windows.Forms.TableLayoutPanel();
            this.pnlStatTotal           = new System.Windows.Forms.Panel();
            this.lblStatTotalNum        = new System.Windows.Forms.Label();
            this.lblStatTotalLabel      = new System.Windows.Forms.Label();
            this.pnlStatPublished       = new System.Windows.Forms.Panel();
            this.lblStatPublishedNum    = new System.Windows.Forms.Label();
            this.lblStatPublishedLabel  = new System.Windows.Forms.Label();
            this.pnlStatDraft           = new System.Windows.Forms.Panel();
            this.lblStatDraftNum        = new System.Windows.Forms.Label();
            this.lblStatDraftLabel      = new System.Windows.Forms.Label();
            this.pnlInfoSep             = new System.Windows.Forms.Panel();
            this.lblInfoCreatedByKey    = new System.Windows.Forms.Label();
            this.lblInfoCreatedByVal    = new System.Windows.Forms.Label();
            this.lblInfoCreatedDateKey  = new System.Windows.Forms.Label();
            this.lblInfoCreatedDateVal  = new System.Windows.Forms.Label();
            this.lblInfoUpdatedKey      = new System.Windows.Forms.Label();
            this.lblInfoUpdatedVal      = new System.Windows.Forms.Label();
            this.lblInfoStatusKey       = new System.Windows.Forms.Label();
            this.lblInfoStatusVal       = new System.Windows.Forms.Label();
            this.lblInfoPathKey         = new System.Windows.Forms.Label();
            this.lblInfoPathVal         = new System.Windows.Forms.Label();

            // ── Article Usage Card ──────────────────────────────────────────────
            this.pnlArticleCard         = new System.Windows.Forms.Panel();
            this.pnlArticleCardHeader   = new System.Windows.Forms.Panel();
            this.lblArticleCardHeader   = new System.Windows.Forms.Label();
            this.lstArticleUsage        = new System.Windows.Forms.ListView();
            this.colArtCode             = new System.Windows.Forms.ColumnHeader();
            this.colArtTitle            = new System.Windows.Forms.ColumnHeader();
            this.colArtType             = new System.Windows.Forms.ColumnHeader();
            this.colArtStatus           = new System.Windows.Forms.ColumnHeader();
            this.colArtAuthor           = new System.Windows.Forms.ColumnHeader();

            // ── Warning Box ─────────────────────────────────────────────────────
            this.pnlWarnBox             = new System.Windows.Forms.Panel();
            this.lblWarnIcon            = new System.Windows.Forms.Label();
            this.lblWarnText            = new System.Windows.Forms.Label();

            // ── Suspend layout ───────────────────────────────────────────────────
            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.pnlLeftHeader.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.pnlEditCard.SuspendLayout();
            this.pnlEditCardHeader.SuspendLayout();
            this.pnlEditCardBody.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlDetailsCard.SuspendLayout();
            this.pnlDetailsCardHeader.SuspendLayout();
            this.pnlDetailsCardBody.SuspendLayout();
            this.tblStats.SuspendLayout();
            this.pnlStatTotal.SuspendLayout();
            this.pnlStatPublished.SuspendLayout();
            this.pnlStatDraft.SuspendLayout();
            this.pnlArticleCard.SuspendLayout();
            this.pnlArticleCardHeader.SuspendLayout();
            this.pnlWarnBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.numSortOrder).BeginInit();
            this.SuspendLayout();

            // ════════════════════════════════════════════════════════════════════
            // pnlHeader
            // ════════════════════════════════════════════════════════════════════
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 50;
            // Add lblHeader first (lower z → Fill), then btnBack (higher z → Left)
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Controls.Add(this.btnBack);

            this.btnBack.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBack.Width = 90;
            this.btnBack.Text = "← กลับ";
            this.btnBack.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnBack.BackColor = System.Drawing.Color.FromArgb(20, 75, 130);
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);

            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Text = "  \uD83D\uDCDA ตั้งค่า Category — Knowledge Base (MOD-006)";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ════════════════════════════════════════════════════════════════════
            // pnlMain
            // ════════════════════════════════════════════════════════════════════
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            // Add pnlRight first (Fill), divider, then pnlLeft last (Left claims first)
            this.pnlMain.Controls.Add(this.pnlRight);
            this.pnlMain.Controls.Add(this.divider);
            this.pnlMain.Controls.Add(this.pnlLeft);

            // ════════════════════════════════════════════════════════════════════
            // pnlLeft
            // ════════════════════════════════════════════════════════════════════
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Width = 370;
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            // lstCategories (Fill) added first, pnlLeftHeader (Top) added last
            this.pnlLeft.Controls.Add(this.lstCategories);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);

            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Height = 44;
            this.pnlLeftHeader.BackColor = System.Drawing.Color.FromArgb(235, 240, 248);
            this.pnlLeftHeader.Controls.Add(this.lblListTitle);
            this.pnlLeftHeader.Controls.Add(this.btnNew);

            this.lblListTitle.AutoSize = false;
            this.lblListTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblListTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblListTitle.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.lblListTitle.Text = "  รายการ Category";
            this.lblListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.btnNew.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnNew.Width = 100;
            this.btnNew.Text = "+ เพิ่มใหม่";
            this.btnNew.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnNew.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnNew.ForeColor = System.Drawing.Color.White;
            this.btnNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNew.FlatAppearance.BorderSize = 0;
            this.btnNew.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNew.Click += new System.EventHandler(this.BtnNew_Click);

            this.lstCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstCategories.View = System.Windows.Forms.View.Details;
            this.lstCategories.FullRowSelect = true;
            this.lstCategories.GridLines = false;
            this.lstCategories.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstCategories.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstCategories.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstCategories.MultiSelect = false;
            this.lstCategories.SelectedIndexChanged += new System.EventHandler(this.LstCategories_SelectedIndexChanged);
            this.colCode.Text = "รหัส";         this.colCode.Width = 70;
            this.colName.Text = "ชื่อ Category"; this.colName.Width = 130;
            this.colParent.Text = "Parent";       this.colParent.Width = 80;
            this.colSort.Text = "ลำดับ";          this.colSort.Width = 45;
            this.colStatus.Text = "สถานะ";        this.colStatus.Width = 55;
            this.lstCategories.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                { this.colCode, this.colName, this.colParent, this.colSort, this.colStatus });

            // ════════════════════════════════════════════════════════════════════
            // divider
            // ════════════════════════════════════════════════════════════════════
            this.divider.Dock = System.Windows.Forms.DockStyle.Left;
            this.divider.Width = 1;
            this.divider.BackColor = System.Drawing.Color.FromArgb(200, 210, 225);

            // ════════════════════════════════════════════════════════════════════
            // pnlRight (scrollable container)
            // Cards are added bottom-to-top so that the last added (EditCard)
            // gets the highest z-order and docks to the top first.
            // ════════════════════════════════════════════════════════════════════
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.AutoScroll = true;
            this.pnlRight.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlRight.Padding = new System.Windows.Forms.Padding(12, 12, 20, 12);
            this.pnlRight.Controls.Add(this.pnlWarnBox);
            this.pnlRight.Controls.Add(this.pnlSpacer3);
            this.pnlRight.Controls.Add(this.pnlArticleCard);
            this.pnlRight.Controls.Add(this.pnlSpacer2);
            this.pnlRight.Controls.Add(this.pnlDetailsCard);
            this.pnlRight.Controls.Add(this.pnlSpacer1);
            this.pnlRight.Controls.Add(this.pnlEditCard);    // added last → docks to top

            // ════════════════════════════════════════════════════════════════════
            // EDIT CARD
            // ════════════════════════════════════════════════════════════════════
            this.pnlEditCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEditCard.Height = 356;
            this.pnlEditCard.BackColor = System.Drawing.Color.White;
            this.pnlEditCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // Body (Fill) added first, header (Top) added last
            this.pnlEditCard.Controls.Add(this.pnlEditCardBody);
            this.pnlEditCard.Controls.Add(this.pnlEditCardHeader);

            // Edit card header
            this.pnlEditCardHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEditCardHeader.Height = 36;
            this.pnlEditCardHeader.BackColor = System.Drawing.Color.FromArgb(235, 242, 255);
            this.pnlEditCardHeader.Controls.Add(this.lblEditCardHeader);

            this.lblEditCardHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEditCardHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblEditCardHeader.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.lblEditCardHeader.Text = "  \u270F\uFE0F เพิ่ม Category ใหม่";
            this.lblEditCardHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblEditCardHeader.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);

            // Edit card body (absolute-positioned form fields)
            this.pnlEditCardBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEditCardBody.BackColor = System.Drawing.Color.White;
            // pnlButtons (Bottom) added first, then form controls
            this.pnlEditCardBody.Controls.Add(this.pnlButtons);

            // Form controls (absolute positions)
            // lx=16, ix=150, ry=14, rh=38 — all pre-calculated as literals

            // Row 0 — ชื่อหมวดหมู่  (y = ry+4 = 18, input y = ry = 14)
            this.lblName.AutoSize = false;
            this.lblName.Location = new System.Drawing.Point(16, 18);
            this.lblName.Size = new System.Drawing.Size(130, 22);
            this.lblName.Text = "ชื่อหมวดหมู่ *";
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.pnlEditCardBody.Controls.Add(this.lblName);

            this.txtName.Location = new System.Drawing.Point(150, 14);
            this.txtName.Size = new System.Drawing.Size(300, 26);
            this.txtName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtName.MaxLength = 100;
            this.txtName.TextChanged += new System.EventHandler(this.TxtName_TextChanged);
            this.pnlEditCardBody.Controls.Add(this.txtName);

            // Row 1 — ไอคอน  (y = ry+rh+4 = 56, input y = ry+rh = 52)
            this.lblIcon.AutoSize = false;
            this.lblIcon.Location = new System.Drawing.Point(16, 56);
            this.lblIcon.Size = new System.Drawing.Size(130, 22);
            this.lblIcon.Text = "ไอคอน *";
            this.lblIcon.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.pnlEditCardBody.Controls.Add(this.lblIcon);

            this.cboIcon.Location = new System.Drawing.Point(150, 52);
            this.cboIcon.Size = new System.Drawing.Size(130, 26);
            this.cboIcon.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboIcon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboIcon.Items.AddRange(new object[]
            {
                "\u2753 คำถาม", "\uD83D\uDCCB รายการ (SOP)", "\uD83D\uDCC4 เอกสาร",
                "\uD83D\uDCBB IT/ระบบ", "\uD83D\uDCD6 คู่มือ", "\uD83D\uDCC2 โฟลเดอร์",
                "\u2696\uFE0F กฎ/ข้อบังคับ", "\uD83D\uDD14 ประกาศ", "\uD83D\uDCA1 ความรู้"
            });
            this.cboIcon.SelectedIndexChanged += new System.EventHandler(this.CboIcon_SelectedIndexChanged);
            this.pnlEditCardBody.Controls.Add(this.cboIcon);

            // lblIconPreview x = ix+136 = 286, y = ry+rh-3 = 49
            this.lblIconPreview.AutoSize = false;
            this.lblIconPreview.Location = new System.Drawing.Point(286, 49);
            this.lblIconPreview.Size = new System.Drawing.Size(32, 32);
            this.lblIconPreview.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.lblIconPreview.Text = "\u2753";
            this.lblIconPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.pnlEditCardBody.Controls.Add(this.lblIconPreview);

            // lblIconFullPreview x = ix+174 = 324, y = ry+rh+4 = 56
            this.lblIconFullPreview.AutoSize = false;
            this.lblIconFullPreview.Location = new System.Drawing.Point(324, 56);
            this.lblIconFullPreview.Size = new System.Drawing.Size(200, 22);
            this.lblIconFullPreview.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblIconFullPreview.ForeColor = System.Drawing.Color.FromArgb(0, 80, 180);
            this.lblIconFullPreview.Text = "\u2753 ชื่อ Category";
            this.pnlEditCardBody.Controls.Add(this.lblIconFullPreview);

            // Row 2 — ลำดับแสดงผล  (y = ry+rh*2+4 = 94, input y = ry+rh*2 = 90)
            this.lblSortOrder.AutoSize = false;
            this.lblSortOrder.Location = new System.Drawing.Point(16, 94);
            this.lblSortOrder.Size = new System.Drawing.Size(130, 22);
            this.lblSortOrder.Text = "ลำดับแสดงผล *";
            this.lblSortOrder.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.pnlEditCardBody.Controls.Add(this.lblSortOrder);

            this.numSortOrder.Location = new System.Drawing.Point(150, 90);
            this.numSortOrder.Size = new System.Drawing.Size(80, 26);
            this.numSortOrder.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.numSortOrder.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numSortOrder.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            this.numSortOrder.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.numSortOrder.ValueChanged += new System.EventHandler(this.NumSortOrder_ValueChanged);
            this.pnlEditCardBody.Controls.Add(this.numSortOrder);

            // Row 3 — สถานะ  (y = ry+rh*3+4 = 132, input y = ry+rh*3 = 128)
            this.lblStatusLabel.AutoSize = false;
            this.lblStatusLabel.Location = new System.Drawing.Point(16, 132);
            this.lblStatusLabel.Size = new System.Drawing.Size(130, 22);
            this.lblStatusLabel.Text = "สถานะ *";
            this.lblStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.pnlEditCardBody.Controls.Add(this.lblStatusLabel);

            this.cboStatus.Location = new System.Drawing.Point(150, 128);
            this.cboStatus.Size = new System.Drawing.Size(180, 26);
            this.cboStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStatus.Items.AddRange(new object[] { "\u25CF Active (ใช้งาน)", "\u25CB Inactive (ปิดการใช้งาน)" });
            this.cboStatus.SelectedIndex = 0;
            this.cboStatus.Enabled = false;      // display-only; status changes via btnDelete (Deactivate)
            this.pnlEditCardBody.Controls.Add(this.cboStatus);

            // Row 4 — หมวดหมู่แม่  (y = ry+rh*4+4 = 170, input y = ry+rh*4 = 166)
            this.lblParent.AutoSize = false;
            this.lblParent.Location = new System.Drawing.Point(16, 170);
            this.lblParent.Size = new System.Drawing.Size(130, 22);
            this.lblParent.Text = "หมวดหมู่แม่";
            this.lblParent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.pnlEditCardBody.Controls.Add(this.lblParent);

            this.cboParent.Location = new System.Drawing.Point(150, 166);
            this.cboParent.Size = new System.Drawing.Size(280, 26);
            this.cboParent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboParent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboParent.SelectedIndexChanged += new System.EventHandler(this.CboParent_SelectedIndexChanged);
            this.pnlEditCardBody.Controls.Add(this.cboParent);

            // Row 5 — คำอธิบาย  (y = ry+rh*5+4 = 208, input y = ry+rh*5 = 204)
            this.lblDescription.AutoSize = false;
            this.lblDescription.Location = new System.Drawing.Point(16, 208);
            this.lblDescription.Size = new System.Drawing.Size(130, 22);
            this.lblDescription.Text = "คำอธิบาย";
            this.lblDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.pnlEditCardBody.Controls.Add(this.lblDescription);

            this.txtDescription.Location = new System.Drawing.Point(150, 204);
            this.txtDescription.Size = new System.Drawing.Size(300, 56);
            this.txtDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtDescription.Multiline = true;
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.MaxLength = 300;
            this.txtDescription.TextChanged += new System.EventHandler(this.TxtDescription_TextChanged);
            this.pnlEditCardBody.Controls.Add(this.txtDescription);

            // pnlButtons (Dock=Bottom inside pnlEditCardBody)
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Height = 50;
            this.pnlButtons.BackColor = System.Drawing.Color.White;
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
            this.pnlButtons.Controls.Add(this.lblDirtyHint);
            this.pnlButtons.Controls.Add(this.btnClear);
            this.pnlButtons.Controls.Add(this.btnDelete);
            this.pnlButtons.Controls.Add(this.btnSave);

            this.btnSave.Location = new System.Drawing.Point(16, 9);
            this.btnSave.Size = new System.Drawing.Size(110, 32);
            this.btnSave.Text = "\uD83D\uDCBE บันทึก";
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);

            this.btnDelete.Location = new System.Drawing.Point(134, 9);
            this.btnDelete.Size = new System.Drawing.Size(120, 32);
            this.btnDelete.Text = "\uD83D\uDDD1\uFE0F ลบหมวดหมู่";
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(200, 50, 50);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.Enabled = false;
            this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);

            this.btnClear.Location = new System.Drawing.Point(262, 9);
            this.btnClear.Size = new System.Drawing.Size(100, 32);
            this.btnClear.Text = "\u274C ยกเลิก";
            this.btnClear.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnClear.BackColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.btnClear.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.FlatAppearance.BorderSize = 0;
            this.btnClear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);

            this.lblDirtyHint.AutoSize = false;
            this.lblDirtyHint.Location = new System.Drawing.Point(370, 14);
            this.lblDirtyHint.Size = new System.Drawing.Size(240, 20);
            this.lblDirtyHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblDirtyHint.ForeColor = System.Drawing.Color.FromArgb(200, 140, 0);
            this.lblDirtyHint.Text = "\u26A0\uFE0F มีการแก้ไขที่ยังไม่ได้บันทึก";
            this.lblDirtyHint.Visible = false;

            // ════════════════════════════════════════════════════════════════════
            // Spacers
            // ════════════════════════════════════════════════════════════════════
            this.pnlSpacer1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSpacer1.Height = 12;
            this.pnlSpacer1.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);

            this.pnlSpacer2.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSpacer2.Height = 12;
            this.pnlSpacer2.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);

            this.pnlSpacer3.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSpacer3.Height = 12;
            this.pnlSpacer3.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);

            // ════════════════════════════════════════════════════════════════════
            // DETAILS CARD
            // ════════════════════════════════════════════════════════════════════
            this.pnlDetailsCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDetailsCard.Height = 248;
            this.pnlDetailsCard.BackColor = System.Drawing.Color.White;
            this.pnlDetailsCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDetailsCard.Controls.Add(this.pnlDetailsCardBody);
            this.pnlDetailsCard.Controls.Add(this.pnlDetailsCardHeader);

            this.pnlDetailsCardHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDetailsCardHeader.Height = 36;
            this.pnlDetailsCardHeader.BackColor = System.Drawing.Color.FromArgb(235, 242, 255);
            this.pnlDetailsCardHeader.Controls.Add(this.lblDetailsCardHeader);

            this.lblDetailsCardHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDetailsCardHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDetailsCardHeader.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.lblDetailsCardHeader.Text = "  \uD83D\uDCCA รายละเอียดหมวดหมู่";
            this.lblDetailsCardHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblDetailsCardHeader.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);

            this.pnlDetailsCardBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetailsCardBody.BackColor = System.Drawing.Color.White;

            // ── Stats row (TableLayoutPanel with 3 equal columns) ────────────────
            this.tblStats.Location = new System.Drawing.Point(12, 10);
            this.tblStats.Size = new System.Drawing.Size(450, 68);
            this.tblStats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.tblStats.ColumnCount = 3;
            this.tblStats.RowCount = 1;
            this.tblStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tblStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tblStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.tblStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblStats.BackColor = System.Drawing.Color.White;
            this.tblStats.Padding = new System.Windows.Forms.Padding(0);
            this.tblStats.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None;
            this.tblStats.Controls.Add(this.pnlStatTotal,     0, 0);
            this.tblStats.Controls.Add(this.pnlStatPublished, 1, 0);
            this.tblStats.Controls.Add(this.pnlStatDraft,     2, 0);
            this.pnlDetailsCardBody.Controls.Add(this.tblStats);

            // Stat: Total
            this.pnlStatTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStatTotal.BackColor = System.Drawing.Color.FromArgb(235, 242, 255);
            this.pnlStatTotal.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.pnlStatTotal.Controls.Add(this.lblStatTotalNum);
            this.pnlStatTotal.Controls.Add(this.lblStatTotalLabel);

            this.lblStatTotalNum.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatTotalNum.Height = 32;
            this.lblStatTotalNum.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblStatTotalNum.ForeColor = System.Drawing.Color.FromArgb(0, 80, 180);
            this.lblStatTotalNum.Text = "—";
            this.lblStatTotalNum.TextAlign = System.Drawing.ContentAlignment.BottomCenter;

            this.lblStatTotalLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatTotalLabel.Height = 22;
            this.lblStatTotalLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblStatTotalLabel.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lblStatTotalLabel.Text = "บทความทั้งหมด";
            this.lblStatTotalLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;

            // Stat: Published
            this.pnlStatPublished.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStatPublished.BackColor = System.Drawing.Color.FromArgb(224, 244, 232);
            this.pnlStatPublished.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.pnlStatPublished.Controls.Add(this.lblStatPublishedNum);
            this.pnlStatPublished.Controls.Add(this.lblStatPublishedLabel);

            this.lblStatPublishedNum.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatPublishedNum.Height = 32;
            this.lblStatPublishedNum.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblStatPublishedNum.ForeColor = System.Drawing.Color.FromArgb(30, 132, 73);
            this.lblStatPublishedNum.Text = "—";
            this.lblStatPublishedNum.TextAlign = System.Drawing.ContentAlignment.BottomCenter;

            this.lblStatPublishedLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatPublishedLabel.Height = 22;
            this.lblStatPublishedLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblStatPublishedLabel.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lblStatPublishedLabel.Text = "\u2705 Published";
            this.lblStatPublishedLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;

            // Stat: Draft/Review
            this.pnlStatDraft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStatDraft.BackColor = System.Drawing.Color.FromArgb(255, 248, 225);
            this.pnlStatDraft.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.pnlStatDraft.Controls.Add(this.lblStatDraftNum);
            this.pnlStatDraft.Controls.Add(this.lblStatDraftLabel);

            this.lblStatDraftNum.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatDraftNum.Height = 32;
            this.lblStatDraftNum.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblStatDraftNum.ForeColor = System.Drawing.Color.FromArgb(190, 130, 0);
            this.lblStatDraftNum.Text = "—";
            this.lblStatDraftNum.TextAlign = System.Drawing.ContentAlignment.BottomCenter;

            this.lblStatDraftLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatDraftLabel.Height = 22;
            this.lblStatDraftLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblStatDraftLabel.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.lblStatDraftLabel.Text = "\uD83D\uDD34 Draft / Review";
            this.lblStatDraftLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;

            // ── Separator ────────────────────────────────────────────────────────
            this.pnlInfoSep.Location = new System.Drawing.Point(12, 84);
            this.pnlInfoSep.Size = new System.Drawing.Size(450, 1);
            this.pnlInfoSep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlInfoSep.BackColor = System.Drawing.Color.FromArgb(220, 228, 240);
            this.pnlDetailsCardBody.Controls.Add(this.pnlInfoSep);

            // ── Info rows (absolute positions)
            // kx=12, vx=130, infoY=92, infoH=22 — all pre-calculated as literals
            // keyW = vx-kx-4 = 114

            // Row 0: สร้างโดย  y=92
            this.lblInfoCreatedByKey.AutoSize = false;
            this.lblInfoCreatedByKey.Location = new System.Drawing.Point(12, 92);
            this.lblInfoCreatedByKey.Size = new System.Drawing.Size(114, 22);
            this.lblInfoCreatedByKey.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoCreatedByKey.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
            this.lblInfoCreatedByKey.Text = "สร้างโดย:";
            this.lblInfoCreatedByKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblInfoCreatedByVal.AutoSize = false;
            this.lblInfoCreatedByVal.Location = new System.Drawing.Point(130, 92);
            this.lblInfoCreatedByVal.Size = new System.Drawing.Size(340, 22);
            this.lblInfoCreatedByVal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfoCreatedByVal.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoCreatedByVal.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.lblInfoCreatedByVal.Text = "—";
            this.lblInfoCreatedByVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // Row 1: วันที่สร้าง  y=114
            this.lblInfoCreatedDateKey.AutoSize = false;
            this.lblInfoCreatedDateKey.Location = new System.Drawing.Point(12, 114);
            this.lblInfoCreatedDateKey.Size = new System.Drawing.Size(114, 22);
            this.lblInfoCreatedDateKey.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoCreatedDateKey.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
            this.lblInfoCreatedDateKey.Text = "วันที่สร้าง:";
            this.lblInfoCreatedDateKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblInfoCreatedDateVal.AutoSize = false;
            this.lblInfoCreatedDateVal.Location = new System.Drawing.Point(130, 114);
            this.lblInfoCreatedDateVal.Size = new System.Drawing.Size(340, 22);
            this.lblInfoCreatedDateVal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfoCreatedDateVal.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoCreatedDateVal.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.lblInfoCreatedDateVal.Text = "—";
            this.lblInfoCreatedDateVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // Row 2: แก้ไขล่าสุด  y=136
            this.lblInfoUpdatedKey.AutoSize = false;
            this.lblInfoUpdatedKey.Location = new System.Drawing.Point(12, 136);
            this.lblInfoUpdatedKey.Size = new System.Drawing.Size(114, 22);
            this.lblInfoUpdatedKey.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoUpdatedKey.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
            this.lblInfoUpdatedKey.Text = "แก้ไขล่าสุด:";
            this.lblInfoUpdatedKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblInfoUpdatedVal.AutoSize = false;
            this.lblInfoUpdatedVal.Location = new System.Drawing.Point(130, 136);
            this.lblInfoUpdatedVal.Size = new System.Drawing.Size(340, 22);
            this.lblInfoUpdatedVal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfoUpdatedVal.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoUpdatedVal.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.lblInfoUpdatedVal.Text = "—";
            this.lblInfoUpdatedVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // Row 3: สถานะ  y=158
            this.lblInfoStatusKey.AutoSize = false;
            this.lblInfoStatusKey.Location = new System.Drawing.Point(12, 158);
            this.lblInfoStatusKey.Size = new System.Drawing.Size(114, 22);
            this.lblInfoStatusKey.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoStatusKey.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
            this.lblInfoStatusKey.Text = "สถานะ:";
            this.lblInfoStatusKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblInfoStatusVal.AutoSize = false;
            this.lblInfoStatusVal.Location = new System.Drawing.Point(130, 158);
            this.lblInfoStatusVal.Size = new System.Drawing.Size(340, 22);
            this.lblInfoStatusVal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfoStatusVal.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoStatusVal.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.lblInfoStatusVal.Text = "—";
            this.lblInfoStatusVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // Row 4: Path  y=180
            this.lblInfoPathKey.AutoSize = false;
            this.lblInfoPathKey.Location = new System.Drawing.Point(12, 180);
            this.lblInfoPathKey.Size = new System.Drawing.Size(114, 22);
            this.lblInfoPathKey.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoPathKey.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
            this.lblInfoPathKey.Text = "Path:";
            this.lblInfoPathKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblInfoPathVal.AutoSize = false;
            this.lblInfoPathVal.Location = new System.Drawing.Point(130, 180);
            this.lblInfoPathVal.Size = new System.Drawing.Size(340, 22);
            this.lblInfoPathVal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfoPathVal.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoPathVal.ForeColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.lblInfoPathVal.Text = "—";
            this.lblInfoPathVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.pnlDetailsCardBody.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                this.lblInfoCreatedByKey,  this.lblInfoCreatedByVal,
                this.lblInfoCreatedDateKey,this.lblInfoCreatedDateVal,
                this.lblInfoUpdatedKey,    this.lblInfoUpdatedVal,
                this.lblInfoStatusKey,     this.lblInfoStatusVal,
                this.lblInfoPathKey,       this.lblInfoPathVal,
            });

            // ════════════════════════════════════════════════════════════════════
            // ARTICLE USAGE CARD
            // ════════════════════════════════════════════════════════════════════
            this.pnlArticleCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlArticleCard.Height = 218;
            this.pnlArticleCard.BackColor = System.Drawing.Color.White;
            this.pnlArticleCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlArticleCard.Controls.Add(this.lstArticleUsage);
            this.pnlArticleCard.Controls.Add(this.pnlArticleCardHeader);

            this.pnlArticleCardHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlArticleCardHeader.Height = 36;
            this.pnlArticleCardHeader.BackColor = System.Drawing.Color.FromArgb(235, 242, 255);
            this.pnlArticleCardHeader.Controls.Add(this.lblArticleCardHeader);

            this.lblArticleCardHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblArticleCardHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblArticleCardHeader.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.lblArticleCardHeader.Text = "  \uD83D\uDCDA บทความในหมวดหมู่นี้ (แสดง 5 รายการล่าสุด)";
            this.lblArticleCardHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblArticleCardHeader.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);

            this.lstArticleUsage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstArticleUsage.View = System.Windows.Forms.View.Details;
            this.lstArticleUsage.FullRowSelect = true;
            this.lstArticleUsage.GridLines = true;
            this.lstArticleUsage.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstArticleUsage.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lstArticleUsage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstArticleUsage.MultiSelect = false;
            this.colArtCode.Text = "รหัส";        this.colArtCode.Width = 90;
            this.colArtTitle.Text = "ชื่อบทความ"; this.colArtTitle.Width = 200;
            this.colArtType.Text = "ประเภท";      this.colArtType.Width = 70;
            this.colArtStatus.Text = "สถานะ";      this.colArtStatus.Width = 80;
            this.colArtAuthor.Text = "ผู้เขียน";   this.colArtAuthor.Width = 80;
            this.lstArticleUsage.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                { this.colArtCode, this.colArtTitle, this.colArtType, this.colArtStatus, this.colArtAuthor });

            // ════════════════════════════════════════════════════════════════════
            // WARNING BOX
            // ════════════════════════════════════════════════════════════════════
            this.pnlWarnBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlWarnBox.Height = 70;
            this.pnlWarnBox.BackColor = System.Drawing.Color.FromArgb(255, 251, 240);
            this.pnlWarnBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlWarnBox.Padding = new System.Windows.Forms.Padding(8);
            this.pnlWarnBox.Controls.Add(this.lblWarnText);
            this.pnlWarnBox.Controls.Add(this.lblWarnIcon);

            this.lblWarnIcon.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblWarnIcon.Width = 36;
            this.lblWarnIcon.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblWarnIcon.Text = "\u26A0\uFE0F";
            this.lblWarnIcon.TextAlign = System.Drawing.ContentAlignment.TopCenter;

            this.lblWarnText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWarnText.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblWarnText.ForeColor = System.Drawing.Color.FromArgb(100, 60, 0);
            this.lblWarnText.Text =
                "ข้อกำหนดการลบหมวดหมู่: ลบได้เฉพาะหมวดหมู่ที่ไม่มีบทความ และไม่มีหมวดหมู่ย่อยอยู่ภายใน\r\n" +
                "หากต้องการลบหมวดหมู่ที่มีบทความ ต้องย้ายบทความทั้งหมดออกก่อน";

            // ════════════════════════════════════════════════════════════════════
            // Form
            // ════════════════════════════════════════════════════════════════════
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(960, 600);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(820, 500);
            this.Name = "KBCategoryForm";
            this.Text = "ตั้งค่า Category — Knowledge Base";
            this.Load += new System.EventHandler(this.KBCategoryForm_Load);

            // ── ResumeLayout ─────────────────────────────────────────────────────
            this.pnlHeader.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeftHeader.ResumeLayout(false);
            this.pnlEditCard.ResumeLayout(false);
            this.pnlEditCardHeader.ResumeLayout(false);
            this.pnlEditCardBody.ResumeLayout(false);
            this.pnlEditCardBody.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.pnlDetailsCard.ResumeLayout(false);
            this.pnlDetailsCardHeader.ResumeLayout(false);
            this.pnlDetailsCardBody.ResumeLayout(false);
            this.tblStats.ResumeLayout(false);
            this.pnlStatTotal.ResumeLayout(false);
            this.pnlStatPublished.ResumeLayout(false);
            this.pnlStatDraft.ResumeLayout(false);
            this.pnlArticleCard.ResumeLayout(false);
            this.pnlArticleCardHeader.ResumeLayout(false);
            this.pnlWarnBox.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.numSortOrder).EndInit();
            this.ResumeLayout(false);
        }

        // ── Field declarations ────────────────────────────────────────────────────

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlLeftHeader;
        private System.Windows.Forms.Label lblListTitle;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.ListView lstCategories;
        private System.Windows.Forms.ColumnHeader colCode;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colParent;
        private System.Windows.Forms.ColumnHeader colSort;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.Panel divider;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Panel pnlEditCard;
        private System.Windows.Forms.Panel pnlEditCardHeader;
        private System.Windows.Forms.Label lblEditCardHeader;
        private System.Windows.Forms.Panel pnlEditCardBody;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblIcon;
        private System.Windows.Forms.ComboBox cboIcon;
        private System.Windows.Forms.Label lblIconPreview;
        private System.Windows.Forms.Label lblIconFullPreview;
        private System.Windows.Forms.Label lblSortOrder;
        private System.Windows.Forms.NumericUpDown numSortOrder;
        private System.Windows.Forms.Label lblStatusLabel;
        private System.Windows.Forms.ComboBox cboStatus;
        private System.Windows.Forms.Label lblParent;
        private System.Windows.Forms.ComboBox cboParent;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblDirtyHint;
        private System.Windows.Forms.Panel pnlSpacer1;
        private System.Windows.Forms.Panel pnlSpacer2;
        private System.Windows.Forms.Panel pnlSpacer3;
        private System.Windows.Forms.Panel pnlDetailsCard;
        private System.Windows.Forms.Panel pnlDetailsCardHeader;
        private System.Windows.Forms.Label lblDetailsCardHeader;
        private System.Windows.Forms.Panel pnlDetailsCardBody;
        private System.Windows.Forms.TableLayoutPanel tblStats;
        private System.Windows.Forms.Panel pnlStatTotal;
        private System.Windows.Forms.Label lblStatTotalNum;
        private System.Windows.Forms.Label lblStatTotalLabel;
        private System.Windows.Forms.Panel pnlStatPublished;
        private System.Windows.Forms.Label lblStatPublishedNum;
        private System.Windows.Forms.Label lblStatPublishedLabel;
        private System.Windows.Forms.Panel pnlStatDraft;
        private System.Windows.Forms.Label lblStatDraftNum;
        private System.Windows.Forms.Label lblStatDraftLabel;
        private System.Windows.Forms.Panel pnlInfoSep;
        private System.Windows.Forms.Label lblInfoCreatedByKey;
        private System.Windows.Forms.Label lblInfoCreatedByVal;
        private System.Windows.Forms.Label lblInfoCreatedDateKey;
        private System.Windows.Forms.Label lblInfoCreatedDateVal;
        private System.Windows.Forms.Label lblInfoUpdatedKey;
        private System.Windows.Forms.Label lblInfoUpdatedVal;
        private System.Windows.Forms.Label lblInfoStatusKey;
        private System.Windows.Forms.Label lblInfoStatusVal;
        private System.Windows.Forms.Label lblInfoPathKey;
        private System.Windows.Forms.Label lblInfoPathVal;
        private System.Windows.Forms.Panel pnlArticleCard;
        private System.Windows.Forms.Panel pnlArticleCardHeader;
        private System.Windows.Forms.Label lblArticleCardHeader;
        private System.Windows.Forms.ListView lstArticleUsage;
        private System.Windows.Forms.ColumnHeader colArtCode;
        private System.Windows.Forms.ColumnHeader colArtTitle;
        private System.Windows.Forms.ColumnHeader colArtType;
        private System.Windows.Forms.ColumnHeader colArtStatus;
        private System.Windows.Forms.ColumnHeader colArtAuthor;
        private System.Windows.Forms.Panel pnlWarnBox;
        private System.Windows.Forms.Label lblWarnIcon;
        private System.Windows.Forms.Label lblWarnText;
    }
}
