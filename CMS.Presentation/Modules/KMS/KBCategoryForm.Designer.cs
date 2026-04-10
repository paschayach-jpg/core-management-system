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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.btnBack = new System.Windows.Forms.Button();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.pnlWarnBox = new System.Windows.Forms.Panel();
            this.lblWarnText = new System.Windows.Forms.Label();
            this.lblWarnIcon = new System.Windows.Forms.Label();
            this.pnlSpacer3 = new System.Windows.Forms.Panel();
            this.pnlArticleCard = new System.Windows.Forms.Panel();
            this.lstArticleUsage = new System.Windows.Forms.ListView();
            this.colArtCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colArtTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colArtType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colArtStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colArtAuthor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlArticleCardHeader = new System.Windows.Forms.Panel();
            this.lblArticleCardHeader = new System.Windows.Forms.Label();
            this.pnlSpacer2 = new System.Windows.Forms.Panel();
            this.pnlDetailsCard = new System.Windows.Forms.Panel();
            this.pnlDetailsCardBody = new System.Windows.Forms.Panel();
            this.tblStats = new System.Windows.Forms.TableLayoutPanel();
            this.pnlStatTotal = new System.Windows.Forms.Panel();
            this.lblStatTotalNum = new System.Windows.Forms.Label();
            this.lblStatTotalLabel = new System.Windows.Forms.Label();
            this.pnlStatPublished = new System.Windows.Forms.Panel();
            this.lblStatPublishedNum = new System.Windows.Forms.Label();
            this.lblStatPublishedLabel = new System.Windows.Forms.Label();
            this.pnlStatDraft = new System.Windows.Forms.Panel();
            this.lblStatDraftNum = new System.Windows.Forms.Label();
            this.lblStatDraftLabel = new System.Windows.Forms.Label();
            this.pnlInfoSep = new System.Windows.Forms.Panel();
            this.lblInfoCreatedByKey = new System.Windows.Forms.Label();
            this.lblInfoCreatedByVal = new System.Windows.Forms.Label();
            this.lblInfoCreatedDateKey = new System.Windows.Forms.Label();
            this.lblInfoCreatedDateVal = new System.Windows.Forms.Label();
            this.lblInfoUpdatedKey = new System.Windows.Forms.Label();
            this.lblInfoUpdatedVal = new System.Windows.Forms.Label();
            this.lblInfoStatusKey = new System.Windows.Forms.Label();
            this.lblInfoStatusVal = new System.Windows.Forms.Label();
            this.lblInfoPathKey = new System.Windows.Forms.Label();
            this.lblInfoPathVal = new System.Windows.Forms.Label();
            this.pnlDetailsCardHeader = new System.Windows.Forms.Panel();
            this.lblDetailsCardHeader = new System.Windows.Forms.Label();
            this.pnlSpacer1 = new System.Windows.Forms.Panel();
            this.pnlEditCard = new System.Windows.Forms.Panel();
            this.pnlEditCardBody = new System.Windows.Forms.Panel();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.lblDirtyHint = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblIcon = new System.Windows.Forms.Label();
            this.cboIcon = new System.Windows.Forms.ComboBox();
            this.lblIconPreview = new System.Windows.Forms.Label();
            this.lblIconFullPreview = new System.Windows.Forms.Label();
            this.lblSortOrder = new System.Windows.Forms.Label();
            this.numSortOrder = new System.Windows.Forms.NumericUpDown();
            this.lblStatusLabel = new System.Windows.Forms.Label();
            this.cboStatus = new System.Windows.Forms.ComboBox();
            this.lblParent = new System.Windows.Forms.Label();
            this.cboParent = new System.Windows.Forms.ComboBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.pnlEditCardHeader = new System.Windows.Forms.Panel();
            this.lblEditCardHeader = new System.Windows.Forms.Label();
            this.divider = new System.Windows.Forms.Panel();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.treeCategories = new System.Windows.Forms.TreeView();
            this.pnlLeftFooter = new System.Windows.Forms.Panel();
            this.btnNewSub = new System.Windows.Forms.Button();
            this.btnNewRoot = new System.Windows.Forms.Button();
            this.pnlLeftSearch = new System.Windows.Forms.Panel();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.pnlLeftHeader = new System.Windows.Forms.Panel();
            this.lblListTitle = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.pnlWarnBox.SuspendLayout();
            this.pnlArticleCard.SuspendLayout();
            this.pnlArticleCardHeader.SuspendLayout();
            this.pnlDetailsCard.SuspendLayout();
            this.pnlDetailsCardBody.SuspendLayout();
            this.tblStats.SuspendLayout();
            this.pnlStatTotal.SuspendLayout();
            this.pnlStatPublished.SuspendLayout();
            this.pnlStatDraft.SuspendLayout();
            this.pnlDetailsCardHeader.SuspendLayout();
            this.pnlEditCard.SuspendLayout();
            this.pnlEditCardBody.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSortOrder)).BeginInit();
            this.pnlEditCardHeader.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.pnlLeftFooter.SuspendLayout();
            this.pnlLeftSearch.SuspendLayout();
            this.pnlLeftHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Controls.Add(this.btnBack);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1552, 50);
            this.pnlHeader.TabIndex = 1;
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(90, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(1462, 50);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "  📚 ตั้งค่า Category — Knowledge Base (MOD-006)";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnBack
            // 
            this.btnBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(75)))), ((int)(((byte)(130)))));
            this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBack.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.Location = new System.Drawing.Point(0, 0);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(90, 50);
            this.btnBack.TabIndex = 1;
            this.btnBack.Text = "← กลับ";
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.pnlRight);
            this.pnlMain.Controls.Add(this.divider);
            this.pnlMain.Controls.Add(this.pnlLeft);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 50);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(1552, 651);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlRight
            // 
            this.pnlRight.AutoScroll = true;
            this.pnlRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlRight.Controls.Add(this.pnlWarnBox);
            this.pnlRight.Controls.Add(this.pnlSpacer3);
            this.pnlRight.Controls.Add(this.pnlArticleCard);
            this.pnlRight.Controls.Add(this.pnlSpacer2);
            this.pnlRight.Controls.Add(this.pnlDetailsCard);
            this.pnlRight.Controls.Add(this.pnlSpacer1);
            this.pnlRight.Controls.Add(this.pnlEditCard);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(371, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Padding = new System.Windows.Forms.Padding(12, 12, 20, 12);
            this.pnlRight.Size = new System.Drawing.Size(1181, 651);
            this.pnlRight.TabIndex = 0;
            // 
            // pnlWarnBox
            // 
            this.pnlWarnBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(251)))), ((int)(((byte)(240)))));
            this.pnlWarnBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlWarnBox.Controls.Add(this.lblWarnText);
            this.pnlWarnBox.Controls.Add(this.lblWarnIcon);
            this.pnlWarnBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlWarnBox.Location = new System.Drawing.Point(12, 870);
            this.pnlWarnBox.Name = "pnlWarnBox";
            this.pnlWarnBox.Padding = new System.Windows.Forms.Padding(8);
            this.pnlWarnBox.Size = new System.Drawing.Size(1128, 70);
            this.pnlWarnBox.TabIndex = 0;
            // 
            // lblWarnText
            // 
            this.lblWarnText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWarnText.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblWarnText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(0)))));
            this.lblWarnText.Location = new System.Drawing.Point(44, 8);
            this.lblWarnText.Name = "lblWarnText";
            this.lblWarnText.Size = new System.Drawing.Size(1074, 52);
            this.lblWarnText.TabIndex = 0;
            this.lblWarnText.Text = "ข้อกำหนดการลบหมวดหมู่: ลบได้เฉพาะหมวดหมู่ที่ไม่มีบทความ และไม่มีหมวดหมู่ย่อยอยู่ภ" +
    "ายใน\r\nหากต้องการลบหมวดหมู่ที่มีบทความ ต้องย้ายบทความทั้งหมดออกก่อน";
            // 
            // lblWarnIcon
            // 
            this.lblWarnIcon.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblWarnIcon.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblWarnIcon.Location = new System.Drawing.Point(8, 8);
            this.lblWarnIcon.Name = "lblWarnIcon";
            this.lblWarnIcon.Size = new System.Drawing.Size(36, 52);
            this.lblWarnIcon.TabIndex = 1;
            this.lblWarnIcon.Text = "⚠️";
            this.lblWarnIcon.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pnlSpacer3
            // 
            this.pnlSpacer3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlSpacer3.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSpacer3.Location = new System.Drawing.Point(12, 858);
            this.pnlSpacer3.Name = "pnlSpacer3";
            this.pnlSpacer3.Size = new System.Drawing.Size(1128, 12);
            this.pnlSpacer3.TabIndex = 1;
            // 
            // pnlArticleCard
            // 
            this.pnlArticleCard.BackColor = System.Drawing.Color.White;
            this.pnlArticleCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlArticleCard.Controls.Add(this.lstArticleUsage);
            this.pnlArticleCard.Controls.Add(this.pnlArticleCardHeader);
            this.pnlArticleCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlArticleCard.Location = new System.Drawing.Point(12, 640);
            this.pnlArticleCard.Name = "pnlArticleCard";
            this.pnlArticleCard.Size = new System.Drawing.Size(1128, 218);
            this.pnlArticleCard.TabIndex = 2;
            // 
            // lstArticleUsage
            // 
            this.lstArticleUsage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstArticleUsage.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colArtCode,
            this.colArtTitle,
            this.colArtType,
            this.colArtStatus,
            this.colArtAuthor});
            this.lstArticleUsage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstArticleUsage.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lstArticleUsage.FullRowSelect = true;
            this.lstArticleUsage.GridLines = true;
            this.lstArticleUsage.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstArticleUsage.HideSelection = false;
            this.lstArticleUsage.Location = new System.Drawing.Point(0, 36);
            this.lstArticleUsage.MultiSelect = false;
            this.lstArticleUsage.Name = "lstArticleUsage";
            this.lstArticleUsage.Size = new System.Drawing.Size(1126, 180);
            this.lstArticleUsage.TabIndex = 0;
            this.lstArticleUsage.UseCompatibleStateImageBehavior = false;
            this.lstArticleUsage.View = System.Windows.Forms.View.Details;
            // 
            // colArtCode
            // 
            this.colArtCode.Text = "รหัส";
            this.colArtCode.Width = 90;
            // 
            // colArtTitle
            // 
            this.colArtTitle.Text = "ชื่อบทความ";
            this.colArtTitle.Width = 200;
            // 
            // colArtType
            // 
            this.colArtType.Text = "ประเภท";
            this.colArtType.Width = 70;
            // 
            // colArtStatus
            // 
            this.colArtStatus.Text = "สถานะ";
            this.colArtStatus.Width = 80;
            // 
            // colArtAuthor
            // 
            this.colArtAuthor.Text = "ผู้เขียน";
            this.colArtAuthor.Width = 80;
            // 
            // pnlArticleCardHeader
            // 
            this.pnlArticleCardHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.pnlArticleCardHeader.Controls.Add(this.lblArticleCardHeader);
            this.pnlArticleCardHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlArticleCardHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlArticleCardHeader.Name = "pnlArticleCardHeader";
            this.pnlArticleCardHeader.Size = new System.Drawing.Size(1126, 36);
            this.pnlArticleCardHeader.TabIndex = 1;
            // 
            // lblArticleCardHeader
            // 
            this.lblArticleCardHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblArticleCardHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblArticleCardHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblArticleCardHeader.Location = new System.Drawing.Point(0, 0);
            this.lblArticleCardHeader.Name = "lblArticleCardHeader";
            this.lblArticleCardHeader.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.lblArticleCardHeader.Size = new System.Drawing.Size(1126, 36);
            this.lblArticleCardHeader.TabIndex = 0;
            this.lblArticleCardHeader.Text = "  📚 บทความในหมวดหมู่นี้ (แสดง 5 รายการล่าสุด)";
            this.lblArticleCardHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlSpacer2
            // 
            this.pnlSpacer2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlSpacer2.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSpacer2.Location = new System.Drawing.Point(12, 628);
            this.pnlSpacer2.Name = "pnlSpacer2";
            this.pnlSpacer2.Size = new System.Drawing.Size(1128, 12);
            this.pnlSpacer2.TabIndex = 3;
            // 
            // pnlDetailsCard
            // 
            this.pnlDetailsCard.BackColor = System.Drawing.Color.White;
            this.pnlDetailsCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDetailsCard.Controls.Add(this.pnlDetailsCardBody);
            this.pnlDetailsCard.Controls.Add(this.pnlDetailsCardHeader);
            this.pnlDetailsCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDetailsCard.Location = new System.Drawing.Point(12, 380);
            this.pnlDetailsCard.Name = "pnlDetailsCard";
            this.pnlDetailsCard.Size = new System.Drawing.Size(1128, 248);
            this.pnlDetailsCard.TabIndex = 4;
            // 
            // pnlDetailsCardBody
            // 
            this.pnlDetailsCardBody.BackColor = System.Drawing.Color.White;
            this.pnlDetailsCardBody.Controls.Add(this.tblStats);
            this.pnlDetailsCardBody.Controls.Add(this.pnlInfoSep);
            this.pnlDetailsCardBody.Controls.Add(this.lblInfoCreatedByKey);
            this.pnlDetailsCardBody.Controls.Add(this.lblInfoCreatedByVal);
            this.pnlDetailsCardBody.Controls.Add(this.lblInfoCreatedDateKey);
            this.pnlDetailsCardBody.Controls.Add(this.lblInfoCreatedDateVal);
            this.pnlDetailsCardBody.Controls.Add(this.lblInfoUpdatedKey);
            this.pnlDetailsCardBody.Controls.Add(this.lblInfoUpdatedVal);
            this.pnlDetailsCardBody.Controls.Add(this.lblInfoStatusKey);
            this.pnlDetailsCardBody.Controls.Add(this.lblInfoStatusVal);
            this.pnlDetailsCardBody.Controls.Add(this.lblInfoPathKey);
            this.pnlDetailsCardBody.Controls.Add(this.lblInfoPathVal);
            this.pnlDetailsCardBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetailsCardBody.Location = new System.Drawing.Point(0, 36);
            this.pnlDetailsCardBody.Name = "pnlDetailsCardBody";
            this.pnlDetailsCardBody.Size = new System.Drawing.Size(1126, 210);
            this.pnlDetailsCardBody.TabIndex = 0;
            // 
            // tblStats
            // 
            this.tblStats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tblStats.BackColor = System.Drawing.Color.White;
            this.tblStats.ColumnCount = 3;
            this.tblStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tblStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tblStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.tblStats.Controls.Add(this.pnlStatTotal, 0, 0);
            this.tblStats.Controls.Add(this.pnlStatPublished, 1, 0);
            this.tblStats.Controls.Add(this.pnlStatDraft, 2, 0);
            this.tblStats.Location = new System.Drawing.Point(16, 10);
            this.tblStats.Name = "tblStats";
            this.tblStats.RowCount = 1;
            this.tblStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblStats.Size = new System.Drawing.Size(1092, 68);
            this.tblStats.TabIndex = 0;
            // 
            // pnlStatTotal
            // 
            this.pnlStatTotal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.pnlStatTotal.Controls.Add(this.lblStatTotalNum);
            this.pnlStatTotal.Controls.Add(this.lblStatTotalLabel);
            this.pnlStatTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStatTotal.Location = new System.Drawing.Point(0, 0);
            this.pnlStatTotal.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.pnlStatTotal.Name = "pnlStatTotal";
            this.pnlStatTotal.Size = new System.Drawing.Size(357, 68);
            this.pnlStatTotal.TabIndex = 0;
            // 
            // lblStatTotalNum
            // 
            this.lblStatTotalNum.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatTotalNum.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblStatTotalNum.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(80)))), ((int)(((byte)(180)))));
            this.lblStatTotalNum.Location = new System.Drawing.Point(0, 22);
            this.lblStatTotalNum.Name = "lblStatTotalNum";
            this.lblStatTotalNum.Size = new System.Drawing.Size(357, 32);
            this.lblStatTotalNum.TabIndex = 0;
            this.lblStatTotalNum.Text = "—";
            this.lblStatTotalNum.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblStatTotalLabel
            // 
            this.lblStatTotalLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatTotalLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblStatTotalLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblStatTotalLabel.Location = new System.Drawing.Point(0, 0);
            this.lblStatTotalLabel.Name = "lblStatTotalLabel";
            this.lblStatTotalLabel.Size = new System.Drawing.Size(357, 22);
            this.lblStatTotalLabel.TabIndex = 1;
            this.lblStatTotalLabel.Text = "บทความทั้งหมด";
            this.lblStatTotalLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pnlStatPublished
            // 
            this.pnlStatPublished.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(244)))), ((int)(((byte)(232)))));
            this.pnlStatPublished.Controls.Add(this.lblStatPublishedNum);
            this.pnlStatPublished.Controls.Add(this.lblStatPublishedLabel);
            this.pnlStatPublished.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStatPublished.Location = new System.Drawing.Point(366, 0);
            this.pnlStatPublished.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.pnlStatPublished.Name = "pnlStatPublished";
            this.pnlStatPublished.Size = new System.Drawing.Size(357, 68);
            this.pnlStatPublished.TabIndex = 1;
            // 
            // lblStatPublishedNum
            // 
            this.lblStatPublishedNum.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatPublishedNum.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblStatPublishedNum.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(132)))), ((int)(((byte)(73)))));
            this.lblStatPublishedNum.Location = new System.Drawing.Point(0, 22);
            this.lblStatPublishedNum.Name = "lblStatPublishedNum";
            this.lblStatPublishedNum.Size = new System.Drawing.Size(357, 32);
            this.lblStatPublishedNum.TabIndex = 0;
            this.lblStatPublishedNum.Text = "—";
            this.lblStatPublishedNum.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblStatPublishedLabel
            // 
            this.lblStatPublishedLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatPublishedLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblStatPublishedLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblStatPublishedLabel.Location = new System.Drawing.Point(0, 0);
            this.lblStatPublishedLabel.Name = "lblStatPublishedLabel";
            this.lblStatPublishedLabel.Size = new System.Drawing.Size(357, 22);
            this.lblStatPublishedLabel.TabIndex = 1;
            this.lblStatPublishedLabel.Text = "✅ Published";
            this.lblStatPublishedLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pnlStatDraft
            // 
            this.pnlStatDraft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(248)))), ((int)(((byte)(225)))));
            this.pnlStatDraft.Controls.Add(this.lblStatDraftNum);
            this.pnlStatDraft.Controls.Add(this.lblStatDraftLabel);
            this.pnlStatDraft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStatDraft.Location = new System.Drawing.Point(732, 0);
            this.pnlStatDraft.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.pnlStatDraft.Name = "pnlStatDraft";
            this.pnlStatDraft.Size = new System.Drawing.Size(360, 68);
            this.pnlStatDraft.TabIndex = 2;
            // 
            // lblStatDraftNum
            // 
            this.lblStatDraftNum.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatDraftNum.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblStatDraftNum.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(130)))), ((int)(((byte)(0)))));
            this.lblStatDraftNum.Location = new System.Drawing.Point(0, 22);
            this.lblStatDraftNum.Name = "lblStatDraftNum";
            this.lblStatDraftNum.Size = new System.Drawing.Size(360, 32);
            this.lblStatDraftNum.TabIndex = 0;
            this.lblStatDraftNum.Text = "—";
            this.lblStatDraftNum.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // lblStatDraftLabel
            // 
            this.lblStatDraftLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatDraftLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblStatDraftLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblStatDraftLabel.Location = new System.Drawing.Point(0, 0);
            this.lblStatDraftLabel.Name = "lblStatDraftLabel";
            this.lblStatDraftLabel.Size = new System.Drawing.Size(360, 22);
            this.lblStatDraftLabel.TabIndex = 1;
            this.lblStatDraftLabel.Text = "🔴 Draft / Review";
            this.lblStatDraftLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pnlInfoSep
            // 
            this.pnlInfoSep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlInfoSep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(228)))), ((int)(((byte)(240)))));
            this.pnlInfoSep.Location = new System.Drawing.Point(12, 84);
            this.pnlInfoSep.Name = "pnlInfoSep";
            this.pnlInfoSep.Size = new System.Drawing.Size(1376, 1);
            this.pnlInfoSep.TabIndex = 1;
            // 
            // lblInfoCreatedByKey
            // 
            this.lblInfoCreatedByKey.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoCreatedByKey.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.lblInfoCreatedByKey.Location = new System.Drawing.Point(12, 92);
            this.lblInfoCreatedByKey.Name = "lblInfoCreatedByKey";
            this.lblInfoCreatedByKey.Size = new System.Drawing.Size(114, 22);
            this.lblInfoCreatedByKey.TabIndex = 2;
            this.lblInfoCreatedByKey.Text = "สร้างโดย:";
            this.lblInfoCreatedByKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInfoCreatedByVal
            // 
            this.lblInfoCreatedByVal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfoCreatedByVal.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoCreatedByVal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lblInfoCreatedByVal.Location = new System.Drawing.Point(130, 92);
            this.lblInfoCreatedByVal.Name = "lblInfoCreatedByVal";
            this.lblInfoCreatedByVal.Size = new System.Drawing.Size(1266, 22);
            this.lblInfoCreatedByVal.TabIndex = 3;
            this.lblInfoCreatedByVal.Text = "—";
            this.lblInfoCreatedByVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInfoCreatedDateKey
            // 
            this.lblInfoCreatedDateKey.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoCreatedDateKey.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.lblInfoCreatedDateKey.Location = new System.Drawing.Point(12, 114);
            this.lblInfoCreatedDateKey.Name = "lblInfoCreatedDateKey";
            this.lblInfoCreatedDateKey.Size = new System.Drawing.Size(114, 22);
            this.lblInfoCreatedDateKey.TabIndex = 4;
            this.lblInfoCreatedDateKey.Text = "วันที่สร้าง:";
            this.lblInfoCreatedDateKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInfoCreatedDateVal
            // 
            this.lblInfoCreatedDateVal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfoCreatedDateVal.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoCreatedDateVal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lblInfoCreatedDateVal.Location = new System.Drawing.Point(130, 114);
            this.lblInfoCreatedDateVal.Name = "lblInfoCreatedDateVal";
            this.lblInfoCreatedDateVal.Size = new System.Drawing.Size(1266, 22);
            this.lblInfoCreatedDateVal.TabIndex = 5;
            this.lblInfoCreatedDateVal.Text = "—";
            this.lblInfoCreatedDateVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInfoUpdatedKey
            // 
            this.lblInfoUpdatedKey.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoUpdatedKey.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.lblInfoUpdatedKey.Location = new System.Drawing.Point(12, 136);
            this.lblInfoUpdatedKey.Name = "lblInfoUpdatedKey";
            this.lblInfoUpdatedKey.Size = new System.Drawing.Size(114, 22);
            this.lblInfoUpdatedKey.TabIndex = 6;
            this.lblInfoUpdatedKey.Text = "แก้ไขล่าสุด:";
            this.lblInfoUpdatedKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInfoUpdatedVal
            // 
            this.lblInfoUpdatedVal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfoUpdatedVal.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoUpdatedVal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lblInfoUpdatedVal.Location = new System.Drawing.Point(130, 136);
            this.lblInfoUpdatedVal.Name = "lblInfoUpdatedVal";
            this.lblInfoUpdatedVal.Size = new System.Drawing.Size(1266, 22);
            this.lblInfoUpdatedVal.TabIndex = 7;
            this.lblInfoUpdatedVal.Text = "—";
            this.lblInfoUpdatedVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInfoStatusKey
            // 
            this.lblInfoStatusKey.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoStatusKey.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.lblInfoStatusKey.Location = new System.Drawing.Point(12, 158);
            this.lblInfoStatusKey.Name = "lblInfoStatusKey";
            this.lblInfoStatusKey.Size = new System.Drawing.Size(114, 22);
            this.lblInfoStatusKey.TabIndex = 8;
            this.lblInfoStatusKey.Text = "สถานะ:";
            this.lblInfoStatusKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInfoStatusVal
            // 
            this.lblInfoStatusVal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfoStatusVal.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoStatusVal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lblInfoStatusVal.Location = new System.Drawing.Point(130, 158);
            this.lblInfoStatusVal.Name = "lblInfoStatusVal";
            this.lblInfoStatusVal.Size = new System.Drawing.Size(1266, 22);
            this.lblInfoStatusVal.TabIndex = 9;
            this.lblInfoStatusVal.Text = "—";
            this.lblInfoStatusVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInfoPathKey
            // 
            this.lblInfoPathKey.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoPathKey.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.lblInfoPathKey.Location = new System.Drawing.Point(12, 180);
            this.lblInfoPathKey.Name = "lblInfoPathKey";
            this.lblInfoPathKey.Size = new System.Drawing.Size(114, 22);
            this.lblInfoPathKey.TabIndex = 10;
            this.lblInfoPathKey.Text = "Path:";
            this.lblInfoPathKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInfoPathVal
            // 
            this.lblInfoPathVal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfoPathVal.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoPathVal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lblInfoPathVal.Location = new System.Drawing.Point(130, 180);
            this.lblInfoPathVal.Name = "lblInfoPathVal";
            this.lblInfoPathVal.Size = new System.Drawing.Size(1266, 22);
            this.lblInfoPathVal.TabIndex = 11;
            this.lblInfoPathVal.Text = "—";
            this.lblInfoPathVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlDetailsCardHeader
            // 
            this.pnlDetailsCardHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.pnlDetailsCardHeader.Controls.Add(this.lblDetailsCardHeader);
            this.pnlDetailsCardHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDetailsCardHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlDetailsCardHeader.Name = "pnlDetailsCardHeader";
            this.pnlDetailsCardHeader.Size = new System.Drawing.Size(1126, 36);
            this.pnlDetailsCardHeader.TabIndex = 1;
            // 
            // lblDetailsCardHeader
            // 
            this.lblDetailsCardHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDetailsCardHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDetailsCardHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblDetailsCardHeader.Location = new System.Drawing.Point(0, 0);
            this.lblDetailsCardHeader.Name = "lblDetailsCardHeader";
            this.lblDetailsCardHeader.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.lblDetailsCardHeader.Size = new System.Drawing.Size(1126, 36);
            this.lblDetailsCardHeader.TabIndex = 0;
            this.lblDetailsCardHeader.Text = "  📊 รายละเอียดหมวดหมู่";
            this.lblDetailsCardHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlSpacer1
            // 
            this.pnlSpacer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlSpacer1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSpacer1.Location = new System.Drawing.Point(12, 368);
            this.pnlSpacer1.Name = "pnlSpacer1";
            this.pnlSpacer1.Size = new System.Drawing.Size(1128, 12);
            this.pnlSpacer1.TabIndex = 5;
            // 
            // pnlEditCard
            // 
            this.pnlEditCard.BackColor = System.Drawing.Color.White;
            this.pnlEditCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlEditCard.Controls.Add(this.pnlEditCardBody);
            this.pnlEditCard.Controls.Add(this.pnlEditCardHeader);
            this.pnlEditCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEditCard.Location = new System.Drawing.Point(12, 12);
            this.pnlEditCard.Name = "pnlEditCard";
            this.pnlEditCard.Size = new System.Drawing.Size(1128, 356);
            this.pnlEditCard.TabIndex = 6;
            // 
            // pnlEditCardBody
            // 
            this.pnlEditCardBody.BackColor = System.Drawing.Color.White;
            this.pnlEditCardBody.Controls.Add(this.pnlButtons);
            this.pnlEditCardBody.Controls.Add(this.lblName);
            this.pnlEditCardBody.Controls.Add(this.txtName);
            this.pnlEditCardBody.Controls.Add(this.lblIcon);
            this.pnlEditCardBody.Controls.Add(this.cboIcon);
            this.pnlEditCardBody.Controls.Add(this.lblIconPreview);
            this.pnlEditCardBody.Controls.Add(this.lblIconFullPreview);
            this.pnlEditCardBody.Controls.Add(this.lblSortOrder);
            this.pnlEditCardBody.Controls.Add(this.numSortOrder);
            this.pnlEditCardBody.Controls.Add(this.lblStatusLabel);
            this.pnlEditCardBody.Controls.Add(this.cboStatus);
            this.pnlEditCardBody.Controls.Add(this.lblParent);
            this.pnlEditCardBody.Controls.Add(this.cboParent);
            this.pnlEditCardBody.Controls.Add(this.lblDescription);
            this.pnlEditCardBody.Controls.Add(this.txtDescription);
            this.pnlEditCardBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEditCardBody.Location = new System.Drawing.Point(0, 36);
            this.pnlEditCardBody.Name = "pnlEditCardBody";
            this.pnlEditCardBody.Size = new System.Drawing.Size(1126, 318);
            this.pnlEditCardBody.TabIndex = 0;
            // 
            // pnlButtons
            // 
            this.pnlButtons.BackColor = System.Drawing.Color.White;
            this.pnlButtons.Controls.Add(this.lblDirtyHint);
            this.pnlButtons.Controls.Add(this.btnClear);
            this.pnlButtons.Controls.Add(this.btnDelete);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 268);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
            this.pnlButtons.Size = new System.Drawing.Size(1126, 50);
            this.pnlButtons.TabIndex = 0;
            // 
            // lblDirtyHint
            // 
            this.lblDirtyHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblDirtyHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(140)))), ((int)(((byte)(0)))));
            this.lblDirtyHint.Location = new System.Drawing.Point(370, 14);
            this.lblDirtyHint.Name = "lblDirtyHint";
            this.lblDirtyHint.Size = new System.Drawing.Size(240, 20);
            this.lblDirtyHint.TabIndex = 0;
            this.lblDirtyHint.Text = "⚠️ มีการแก้ไขที่ยังไม่ได้บันทึก";
            this.lblDirtyHint.Visible = false;
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.btnClear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClear.FlatAppearance.BorderSize = 0;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnClear.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnClear.Location = new System.Drawing.Point(262, 9);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(100, 32);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "❌ ยกเลิก";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.Enabled = false;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(134, 9);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(120, 32);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "🗑️ ลบหมวดหมู่";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(16, 9);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(110, 32);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "💾 บันทึก";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // lblName
            // 
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblName.Location = new System.Drawing.Point(16, 18);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(130, 22);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "ชื่อหมวดหมู่ *";
            // 
            // txtName
            // 
            this.txtName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtName.Location = new System.Drawing.Point(150, 14);
            this.txtName.MaxLength = 100;
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(300, 27);
            this.txtName.TabIndex = 2;
            this.txtName.TextChanged += new System.EventHandler(this.TxtName_TextChanged);
            // 
            // lblIcon
            // 
            this.lblIcon.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblIcon.Location = new System.Drawing.Point(16, 56);
            this.lblIcon.Name = "lblIcon";
            this.lblIcon.Size = new System.Drawing.Size(130, 22);
            this.lblIcon.TabIndex = 3;
            this.lblIcon.Text = "ไอคอน *";
            // 
            // cboIcon
            // 
            this.cboIcon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboIcon.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboIcon.Items.AddRange(new object[] {
            "❓ คำถาม",
            "📋 รายการ (SOP)",
            "📄 เอกสาร",
            "💻 IT/ระบบ",
            "📖 คู่มือ",
            "📂 โฟลเดอร์",
            "⚖️ กฎ/ข้อบังคับ",
            "🔔 ประกาศ",
            "💡 ความรู้"});
            this.cboIcon.Location = new System.Drawing.Point(150, 52);
            this.cboIcon.Name = "cboIcon";
            this.cboIcon.Size = new System.Drawing.Size(130, 28);
            this.cboIcon.TabIndex = 4;
            this.cboIcon.SelectedIndexChanged += new System.EventHandler(this.CboIcon_SelectedIndexChanged);
            // 
            // lblIconPreview
            // 
            this.lblIconPreview.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.lblIconPreview.Location = new System.Drawing.Point(286, 49);
            this.lblIconPreview.Name = "lblIconPreview";
            this.lblIconPreview.Size = new System.Drawing.Size(32, 32);
            this.lblIconPreview.TabIndex = 5;
            this.lblIconPreview.Text = "❓";
            this.lblIconPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblIconFullPreview
            // 
            this.lblIconFullPreview.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblIconFullPreview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(80)))), ((int)(((byte)(180)))));
            this.lblIconFullPreview.Location = new System.Drawing.Point(324, 56);
            this.lblIconFullPreview.Name = "lblIconFullPreview";
            this.lblIconFullPreview.Size = new System.Drawing.Size(200, 22);
            this.lblIconFullPreview.TabIndex = 6;
            this.lblIconFullPreview.Text = "❓ ชื่อ Category";
            // 
            // lblSortOrder
            // 
            this.lblSortOrder.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSortOrder.Location = new System.Drawing.Point(16, 94);
            this.lblSortOrder.Name = "lblSortOrder";
            this.lblSortOrder.Size = new System.Drawing.Size(130, 22);
            this.lblSortOrder.TabIndex = 7;
            this.lblSortOrder.Text = "ลำดับแสดงผล *";
            // 
            // numSortOrder
            // 
            this.numSortOrder.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.numSortOrder.Location = new System.Drawing.Point(150, 90);
            this.numSortOrder.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numSortOrder.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSortOrder.Name = "numSortOrder";
            this.numSortOrder.Size = new System.Drawing.Size(80, 27);
            this.numSortOrder.TabIndex = 8;
            this.numSortOrder.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSortOrder.ValueChanged += new System.EventHandler(this.NumSortOrder_ValueChanged);
            // 
            // lblStatusLabel
            // 
            this.lblStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatusLabel.Location = new System.Drawing.Point(16, 132);
            this.lblStatusLabel.Name = "lblStatusLabel";
            this.lblStatusLabel.Size = new System.Drawing.Size(130, 22);
            this.lblStatusLabel.TabIndex = 9;
            this.lblStatusLabel.Text = "สถานะ *";
            // 
            // cboStatus
            // 
            this.cboStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStatus.Enabled = false;
            this.cboStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboStatus.Items.AddRange(new object[] {
            "● Active (ใช้งาน)",
            "○ Inactive (ปิดการใช้งาน)"});
            this.cboStatus.Location = new System.Drawing.Point(150, 128);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Size = new System.Drawing.Size(180, 28);
            this.cboStatus.TabIndex = 10;
            // 
            // lblParent
            // 
            this.lblParent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblParent.Location = new System.Drawing.Point(16, 170);
            this.lblParent.Name = "lblParent";
            this.lblParent.Size = new System.Drawing.Size(130, 22);
            this.lblParent.TabIndex = 11;
            this.lblParent.Text = "หมวดหมู่แม่";
            // 
            // cboParent
            // 
            this.cboParent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboParent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboParent.Location = new System.Drawing.Point(150, 166);
            this.cboParent.Name = "cboParent";
            this.cboParent.Size = new System.Drawing.Size(280, 28);
            this.cboParent.TabIndex = 12;
            this.cboParent.SelectedIndexChanged += new System.EventHandler(this.CboParent_SelectedIndexChanged);
            // 
            // lblDescription
            // 
            this.lblDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDescription.Location = new System.Drawing.Point(16, 208);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(130, 22);
            this.lblDescription.TabIndex = 13;
            this.lblDescription.Text = "คำอธิบาย";
            // 
            // txtDescription
            // 
            this.txtDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtDescription.Location = new System.Drawing.Point(150, 204);
            this.txtDescription.MaxLength = 300;
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(300, 56);
            this.txtDescription.TabIndex = 14;
            this.txtDescription.TextChanged += new System.EventHandler(this.TxtDescription_TextChanged);
            // 
            // pnlEditCardHeader
            // 
            this.pnlEditCardHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.pnlEditCardHeader.Controls.Add(this.lblEditCardHeader);
            this.pnlEditCardHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEditCardHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlEditCardHeader.Name = "pnlEditCardHeader";
            this.pnlEditCardHeader.Size = new System.Drawing.Size(1126, 36);
            this.pnlEditCardHeader.TabIndex = 1;
            // 
            // lblEditCardHeader
            // 
            this.lblEditCardHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEditCardHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblEditCardHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblEditCardHeader.Location = new System.Drawing.Point(0, 0);
            this.lblEditCardHeader.Name = "lblEditCardHeader";
            this.lblEditCardHeader.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.lblEditCardHeader.Size = new System.Drawing.Size(1126, 36);
            this.lblEditCardHeader.TabIndex = 0;
            this.lblEditCardHeader.Text = "  ✏️ เพิ่ม Category ใหม่";
            this.lblEditCardHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // divider
            // 
            this.divider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(210)))), ((int)(((byte)(225)))));
            this.divider.Dock = System.Windows.Forms.DockStyle.Left;
            this.divider.Location = new System.Drawing.Point(370, 0);
            this.divider.Name = "divider";
            this.divider.Size = new System.Drawing.Size(1, 651);
            this.divider.TabIndex = 1;
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.pnlLeft.Controls.Add(this.treeCategories);
            this.pnlLeft.Controls.Add(this.pnlLeftFooter);
            this.pnlLeft.Controls.Add(this.pnlLeftSearch);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(370, 651);
            this.pnlLeft.TabIndex = 2;
            // 
            // treeCategories
            // 
            this.treeCategories.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.treeCategories.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeCategories.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.treeCategories.FullRowSelect = true;
            this.treeCategories.HotTracking = true;
            this.treeCategories.Indent = 16;
            this.treeCategories.Location = new System.Drawing.Point(0, 80);
            this.treeCategories.Name = "treeCategories";
            this.treeCategories.ShowLines = false;
            this.treeCategories.ShowRootLines = false;
            this.treeCategories.Size = new System.Drawing.Size(370, 527);
            this.treeCategories.TabIndex = 0;
            this.treeCategories.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeCategories_AfterSelect);
            // 
            // pnlLeftFooter
            // 
            this.pnlLeftFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(248)))));
            this.pnlLeftFooter.Controls.Add(this.btnNewSub);
            this.pnlLeftFooter.Controls.Add(this.btnNewRoot);
            this.pnlLeftFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLeftFooter.Location = new System.Drawing.Point(0, 607);
            this.pnlLeftFooter.Name = "pnlLeftFooter";
            this.pnlLeftFooter.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlLeftFooter.Size = new System.Drawing.Size(370, 44);
            this.pnlLeftFooter.TabIndex = 1;
            // 
            // btnNewSub
            // 
            this.btnNewSub.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(145)))), ((int)(((byte)(210)))));
            this.btnNewSub.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewSub.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNewSub.FlatAppearance.BorderSize = 0;
            this.btnNewSub.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewSub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnNewSub.ForeColor = System.Drawing.Color.White;
            this.btnNewSub.Location = new System.Drawing.Point(170, 6);
            this.btnNewSub.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.btnNewSub.Name = "btnNewSub";
            this.btnNewSub.Size = new System.Drawing.Size(192, 32);
            this.btnNewSub.TabIndex = 0;
            this.btnNewSub.Text = "+ หมวดหมู่ย่อย";
            this.btnNewSub.UseVisualStyleBackColor = false;
            this.btnNewSub.Click += new System.EventHandler(this.BtnNewSub_Click);
            // 
            // btnNewRoot
            // 
            this.btnNewRoot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnNewRoot.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNewRoot.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnNewRoot.FlatAppearance.BorderSize = 0;
            this.btnNewRoot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewRoot.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnNewRoot.ForeColor = System.Drawing.Color.White;
            this.btnNewRoot.Location = new System.Drawing.Point(8, 6);
            this.btnNewRoot.Name = "btnNewRoot";
            this.btnNewRoot.Size = new System.Drawing.Size(162, 32);
            this.btnNewRoot.TabIndex = 1;
            this.btnNewRoot.Text = "+ หมวดหมู่หลัก";
            this.btnNewRoot.UseVisualStyleBackColor = false;
            this.btnNewRoot.Click += new System.EventHandler(this.BtnNewRoot_Click);
            // 
            // pnlLeftSearch
            // 
            this.pnlLeftSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(248)))));
            this.pnlLeftSearch.Controls.Add(this.txtSearch);
            this.pnlLeftSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftSearch.Location = new System.Drawing.Point(0, 44);
            this.pnlLeftSearch.Name = "pnlLeftSearch";
            this.pnlLeftSearch.Padding = new System.Windows.Forms.Padding(8, 6, 8, 4);
            this.pnlLeftSearch.Size = new System.Drawing.Size(370, 36);
            this.pnlLeftSearch.TabIndex = 2;
            // 
            // txtSearch
            // 
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.txtSearch.Location = new System.Drawing.Point(8, 6);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(354, 27);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.Text = "🔍 ค้นหาหมวดหมู่...";
            this.txtSearch.TextChanged += new System.EventHandler(this.TxtSearch_TextChanged);
            this.txtSearch.Enter += new System.EventHandler(this.TxtSearch_Enter);
            this.txtSearch.Leave += new System.EventHandler(this.TxtSearch_Leave);
            // 
            // pnlLeftHeader
            // 
            this.pnlLeftHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.pnlLeftHeader.Controls.Add(this.lblListTitle);
            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlLeftHeader.Name = "pnlLeftHeader";
            this.pnlLeftHeader.Size = new System.Drawing.Size(370, 44);
            this.pnlLeftHeader.TabIndex = 3;
            // 
            // lblListTitle
            // 
            this.lblListTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblListTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblListTitle.ForeColor = System.Drawing.Color.White;
            this.lblListTitle.Location = new System.Drawing.Point(0, 0);
            this.lblListTitle.Name = "lblListTitle";
            this.lblListTitle.Size = new System.Drawing.Size(370, 44);
            this.lblListTitle.TabIndex = 0;
            this.lblListTitle.Text = "  📂 โครงสร้างหมวดหมู่";
            this.lblListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // KBCategoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1552, 701);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(820, 500);
            this.Name = "KBCategoryForm";
            this.Text = "ตั้งค่า Category — Knowledge Base";
            this.Load += new System.EventHandler(this.KBCategoryForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            this.pnlWarnBox.ResumeLayout(false);
            this.pnlArticleCard.ResumeLayout(false);
            this.pnlArticleCardHeader.ResumeLayout(false);
            this.pnlDetailsCard.ResumeLayout(false);
            this.pnlDetailsCardBody.ResumeLayout(false);
            this.tblStats.ResumeLayout(false);
            this.pnlStatTotal.ResumeLayout(false);
            this.pnlStatPublished.ResumeLayout(false);
            this.pnlStatDraft.ResumeLayout(false);
            this.pnlDetailsCardHeader.ResumeLayout(false);
            this.pnlEditCard.ResumeLayout(false);
            this.pnlEditCardBody.ResumeLayout(false);
            this.pnlEditCardBody.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numSortOrder)).EndInit();
            this.pnlEditCardHeader.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeftFooter.ResumeLayout(false);
            this.pnlLeftSearch.ResumeLayout(false);
            this.pnlLeftSearch.PerformLayout();
            this.pnlLeftHeader.ResumeLayout(false);
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
        private System.Windows.Forms.Panel pnlLeftSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Panel pnlLeftFooter;
        private System.Windows.Forms.Button btnNewRoot;
        private System.Windows.Forms.Button btnNewSub;
        private System.Windows.Forms.TreeView treeCategories;
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
