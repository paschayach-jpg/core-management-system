namespace CMS.Presentation.Modules.KMS
{
    partial class ArticleCardControl
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
            this.pnlStripe = new System.Windows.Forms.Panel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.panelFooter = new System.Windows.Forms.Panel();
            this.pnlStats = new System.Windows.Forms.FlowLayoutPanel();
            this.lblViewIcon = new System.Windows.Forms.Label();
            this.lblViewCount = new System.Windows.Forms.Label();
            this.btnLike = new System.Windows.Forms.Button();
            this.lblLikeCount = new System.Windows.Forms.Label();
            this.lblTagsIcon = new System.Windows.Forms.Label();
            this.lblTags = new System.Windows.Forms.Label();
            this.panelButton = new System.Windows.Forms.Panel();
            this.btnSendReview = new System.Windows.Forms.Button();
            this.btnShare = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnRead = new System.Windows.Forms.Button();
            this.pnlActions = new System.Windows.Forms.Panel();
            this.panelArticelItem = new System.Windows.Forms.Panel();
            this.lblContent = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblSummary = new System.Windows.Forms.Label();
            this.pnlMeta = new System.Windows.Forms.FlowLayoutPanel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblMetaSep3 = new System.Windows.Forms.Label();
            this.lblArticleType = new System.Windows.Forms.Label();
            this.lblCategory = new System.Windows.Forms.Label();
            this.lblMetaSep2 = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblMetaSep1 = new System.Windows.Forms.Label();
            this.pHeaderRight = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblTitleCategory = new System.Windows.Forms.Label();
            this.pnlHeaderMeta = new System.Windows.Forms.FlowLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.lblShowEntity = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblPublishedDate = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblExpiryDate = new System.Windows.Forms.Label();
            this.pnlContent.SuspendLayout();
            this.panelFooter.SuspendLayout();
            this.pnlStats.SuspendLayout();
            this.panelButton.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.panelArticelItem.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.pnlMeta.SuspendLayout();
            this.pHeaderRight.SuspendLayout();
            this.pnlHeaderMeta.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlStripe
            // 
            this.pnlStripe.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.pnlStripe.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlStripe.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlStripe.Location = new System.Drawing.Point(0, 0);
            this.pnlStripe.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pnlStripe.Name = "pnlStripe";
            this.pnlStripe.Size = new System.Drawing.Size(4, 140);
            this.pnlStripe.TabIndex = 0;
            this.pnlStripe.Click += new System.EventHandler(this.OnCardClick);
            this.pnlStripe.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.pnlStripe.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.panelFooter);
            this.pnlContent.Controls.Add(this.pnlActions);
            this.pnlContent.Controls.Add(this.flowLayoutPanel1);
            this.pnlContent.Controls.Add(this.pnlMeta);
            this.pnlContent.Controls.Add(this.pHeaderRight);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlContent.Location = new System.Drawing.Point(4, 0);
            this.pnlContent.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(9, 5, 8, 4);
            this.pnlContent.Size = new System.Drawing.Size(662, 140);
            this.pnlContent.TabIndex = 1;
            this.pnlContent.Click += new System.EventHandler(this.OnCardClick);
            this.pnlContent.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.pnlContent.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // panelFooter
            // 
            this.panelFooter.Controls.Add(this.pnlStats);
            this.panelFooter.Controls.Add(this.panelButton);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFooter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelFooter.Location = new System.Drawing.Point(9, 105);
            this.panelFooter.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new System.Drawing.Size(645, 32);
            this.panelFooter.TabIndex = 5;
            // 
            // pnlStats
            // 
            this.pnlStats.Controls.Add(this.lblViewIcon);
            this.pnlStats.Controls.Add(this.lblViewCount);
            this.pnlStats.Controls.Add(this.btnLike);
            this.pnlStats.Controls.Add(this.lblLikeCount);
            this.pnlStats.Controls.Add(this.lblTagsIcon);
            this.pnlStats.Controls.Add(this.lblTags);
            this.pnlStats.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlStats.Location = new System.Drawing.Point(0, 0);
            this.pnlStats.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pnlStats.Name = "pnlStats";
            this.pnlStats.Size = new System.Drawing.Size(315, 32);
            this.pnlStats.TabIndex = 3;
            this.pnlStats.WrapContents = false;
            this.pnlStats.Click += new System.EventHandler(this.OnCardClick);
            this.pnlStats.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.pnlStats.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblViewIcon
            // 
            this.lblViewIcon.AutoSize = true;
            this.lblViewIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.lblViewIcon.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblViewIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblViewIcon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.lblViewIcon.Location = new System.Drawing.Point(0, 0);
            this.lblViewIcon.Margin = new System.Windows.Forms.Padding(0);
            this.lblViewIcon.Name = "lblViewIcon";
            this.lblViewIcon.Padding = new System.Windows.Forms.Padding(4, 2, 2, 2);
            this.lblViewIcon.Size = new System.Drawing.Size(31, 34);
            this.lblViewIcon.TabIndex = 0;
            this.lblViewIcon.Text = "👁";
            this.lblViewIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblViewIcon.Click += new System.EventHandler(this.OnCardClick);
            this.lblViewIcon.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblViewIcon.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblViewCount
            // 
            this.lblViewCount.AutoSize = true;
            this.lblViewCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.lblViewCount.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblViewCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblViewCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.lblViewCount.Location = new System.Drawing.Point(31, 0);
            this.lblViewCount.Margin = new System.Windows.Forms.Padding(0);
            this.lblViewCount.Name = "lblViewCount";
            this.lblViewCount.Padding = new System.Windows.Forms.Padding(4, 2, 2, 2);
            this.lblViewCount.Size = new System.Drawing.Size(24, 34);
            this.lblViewCount.TabIndex = 1;
            this.lblViewCount.Text = "0";
            this.lblViewCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblViewCount.Click += new System.EventHandler(this.OnCardClick);
            this.lblViewCount.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblViewCount.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // btnLike
            // 
            this.btnLike.AutoSize = true;
            this.btnLike.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.btnLike.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLike.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnLike.FlatAppearance.BorderSize = 0;
            this.btnLike.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.btnLike.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLike.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLike.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.btnLike.Location = new System.Drawing.Point(55, 0);
            this.btnLike.Margin = new System.Windows.Forms.Padding(0);
            this.btnLike.Name = "btnLike";
            this.btnLike.Padding = new System.Windows.Forms.Padding(4, 2, 2, 2);
            this.btnLike.Size = new System.Drawing.Size(40, 34);
            this.btnLike.TabIndex = 2;
            this.btnLike.Text = "👍";
            this.btnLike.UseVisualStyleBackColor = false;
            this.btnLike.Click += new System.EventHandler(this.btnLike_Click);
            // 
            // lblLikeCount
            // 
            this.lblLikeCount.AutoSize = true;
            this.lblLikeCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.lblLikeCount.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblLikeCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLikeCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.lblLikeCount.Location = new System.Drawing.Point(95, 0);
            this.lblLikeCount.Margin = new System.Windows.Forms.Padding(0);
            this.lblLikeCount.Name = "lblLikeCount";
            this.lblLikeCount.Padding = new System.Windows.Forms.Padding(4, 2, 2, 2);
            this.lblLikeCount.Size = new System.Drawing.Size(24, 34);
            this.lblLikeCount.TabIndex = 3;
            this.lblLikeCount.Text = "0";
            this.lblLikeCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLikeCount.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblLikeCount.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblTagsIcon
            // 
            this.lblTagsIcon.AutoSize = true;
            this.lblTagsIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.lblTagsIcon.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTagsIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTagsIcon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(150)))), ((int)(((byte)(100)))));
            this.lblTagsIcon.Location = new System.Drawing.Point(119, 0);
            this.lblTagsIcon.Margin = new System.Windows.Forms.Padding(0);
            this.lblTagsIcon.Name = "lblTagsIcon";
            this.lblTagsIcon.Padding = new System.Windows.Forms.Padding(4, 2, 2, 2);
            this.lblTagsIcon.Size = new System.Drawing.Size(31, 34);
            this.lblTagsIcon.TabIndex = 4;
            this.lblTagsIcon.Text = "🏷";
            this.lblTagsIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTagsIcon.Click += new System.EventHandler(this.OnCardClick);
            this.lblTagsIcon.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblTagsIcon.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblTags
            // 
            this.lblTags.AutoSize = true;
            this.lblTags.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.lblTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTags.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTags.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(150)))), ((int)(((byte)(100)))));
            this.lblTags.Location = new System.Drawing.Point(150, 0);
            this.lblTags.Margin = new System.Windows.Forms.Padding(0);
            this.lblTags.Name = "lblTags";
            this.lblTags.Padding = new System.Windows.Forms.Padding(4, 2, 2, 2);
            this.lblTags.Size = new System.Drawing.Size(20, 34);
            this.lblTags.TabIndex = 5;
            this.lblTags.Text = "-";
            this.lblTags.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTags.Click += new System.EventHandler(this.OnCardClick);
            this.lblTags.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblTags.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // panelButton
            // 
            this.panelButton.Controls.Add(this.btnSendReview);
            this.panelButton.Controls.Add(this.btnShare);
            this.panelButton.Controls.Add(this.btnEdit);
            this.panelButton.Controls.Add(this.btnRead);
            this.panelButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelButton.Location = new System.Drawing.Point(315, 0);
            this.panelButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panelButton.Name = "panelButton";
            this.panelButton.Size = new System.Drawing.Size(330, 32);
            this.panelButton.TabIndex = 5;
            // 
            // btnSendReview
            // 
            this.btnSendReview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnSendReview.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSendReview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSendReview.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnSendReview.FlatAppearance.BorderSize = 0;
            this.btnSendReview.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(82)))), ((int)(((byte)(190)))), ((int)(((byte)(128)))));
            this.btnSendReview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendReview.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendReview.ForeColor = System.Drawing.Color.White;
            this.btnSendReview.Location = new System.Drawing.Point(225, 0);
            this.btnSendReview.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.btnSendReview.Name = "btnSendReview";
            this.btnSendReview.Size = new System.Drawing.Size(105, 32);
            this.btnSendReview.TabIndex = 3;
            this.btnSendReview.Text = "📤 Send Review";
            this.btnSendReview.UseVisualStyleBackColor = false;
            this.btnSendReview.Visible = false;
            this.btnSendReview.Click += new System.EventHandler(this.btnSendReview_Click);
            // 
            // btnShare
            // 
            this.btnShare.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnShare.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShare.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnShare.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnShare.FlatAppearance.BorderSize = 0;
            this.btnShare.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(93)))), ((int)(((byte)(173)))), ((int)(((byte)(226)))));
            this.btnShare.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnShare.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnShare.ForeColor = System.Drawing.Color.White;
            this.btnShare.Location = new System.Drawing.Point(161, 0);
            this.btnShare.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.btnShare.Name = "btnShare";
            this.btnShare.Size = new System.Drawing.Size(64, 32);
            this.btnShare.TabIndex = 2;
            this.btnShare.Text = "🔗 Share";
            this.btnShare.UseVisualStyleBackColor = false;
            this.btnShare.Click += new System.EventHandler(this.btnShare_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(156)))), ((int)(((byte)(18)))));
            this.btnEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEdit.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnEdit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(156)))), ((int)(((byte)(18)))));
            this.btnEdit.FlatAppearance.BorderSize = 0;
            this.btnEdit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(176)))), ((int)(((byte)(65)))));
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEdit.ForeColor = System.Drawing.Color.White;
            this.btnEdit.Location = new System.Drawing.Point(82, 0);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(79, 32);
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Text = "✏️ Edit";
            this.btnEdit.UseVisualStyleBackColor = false;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnRead
            // 
            this.btnRead.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnRead.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRead.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnRead.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnRead.FlatAppearance.BorderSize = 0;
            this.btnRead.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(144)))), ((int)(((byte)(196)))));
            this.btnRead.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRead.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRead.ForeColor = System.Drawing.Color.White;
            this.btnRead.Location = new System.Drawing.Point(0, 0);
            this.btnRead.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(82, 32);
            this.btnRead.TabIndex = 0;
            this.btnRead.Text = "👁️ Read";
            this.btnRead.UseVisualStyleBackColor = false;
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // pnlActions
            // 
            this.pnlActions.Controls.Add(this.panelArticelItem);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlActions.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlActions.Location = new System.Drawing.Point(9, 78);
            this.pnlActions.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.pnlActions.Size = new System.Drawing.Size(645, 27);
            this.pnlActions.TabIndex = 4;
            // 
            // panelArticelItem
            // 
            this.panelArticelItem.Controls.Add(this.lblContent);
            this.panelArticelItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelArticelItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelArticelItem.Location = new System.Drawing.Point(0, 2);
            this.panelArticelItem.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panelArticelItem.Name = "panelArticelItem";
            this.panelArticelItem.Size = new System.Drawing.Size(645, 25);
            this.panelArticelItem.TabIndex = 6;
            // 
            // lblContent
            // 
            this.lblContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblContent.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblContent.Location = new System.Drawing.Point(0, 0);
            this.lblContent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblContent.Name = "lblContent";
            this.lblContent.Size = new System.Drawing.Size(645, 25);
            this.lblContent.TabIndex = 0;
            this.lblContent.Text = "เนื้อหา";
            this.lblContent.Click += new System.EventHandler(this.OnCardClick);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.lblSummary);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.flowLayoutPanel1.Location = new System.Drawing.Point(9, 56);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(645, 22);
            this.flowLayoutPanel1.TabIndex = 7;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // lblSummary
            // 
            this.lblSummary.AutoSize = true;
            this.lblSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.lblSummary.Location = new System.Drawing.Point(0, 2);
            this.lblSummary.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(97, 20);
            this.lblSummary.TabIndex = 2;
            this.lblSummary.Text = "สรุปบทความ...";
            this.lblSummary.Click += new System.EventHandler(this.OnCardClick);
            this.lblSummary.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblSummary.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // pnlMeta
            // 
            this.pnlMeta.Controls.Add(this.lblStatus);
            this.pnlMeta.Controls.Add(this.lblMetaSep3);
            this.pnlMeta.Controls.Add(this.lblArticleType);
            this.pnlMeta.Controls.Add(this.lblCategory);
            this.pnlMeta.Controls.Add(this.lblMetaSep2);
            this.pnlMeta.Controls.Add(this.lblVersion);
            this.pnlMeta.Controls.Add(this.lblMetaSep1);
            this.pnlMeta.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMeta.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlMeta.Location = new System.Drawing.Point(9, 32);
            this.pnlMeta.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pnlMeta.Name = "pnlMeta";
            this.pnlMeta.Size = new System.Drawing.Size(645, 24);
            this.pnlMeta.TabIndex = 1;
            this.pnlMeta.WrapContents = false;
            this.pnlMeta.Click += new System.EventHandler(this.OnCardClick);
            this.pnlMeta.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.pnlMeta.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.lblStatus.Location = new System.Drawing.Point(0, 2);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(45, 20);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Draft";
            this.lblStatus.Visible = false;
            this.lblStatus.Click += new System.EventHandler(this.OnCardClick);
            this.lblStatus.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblStatus.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblMetaSep3
            // 
            this.lblMetaSep3.AutoSize = true;
            this.lblMetaSep3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMetaSep3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(195)))), ((int)(((byte)(205)))));
            this.lblMetaSep3.Location = new System.Drawing.Point(45, 2);
            this.lblMetaSep3.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblMetaSep3.Name = "lblMetaSep3";
            this.lblMetaSep3.Size = new System.Drawing.Size(30, 20);
            this.lblMetaSep3.TabIndex = 5;
            this.lblMetaSep3.Text = "  |  ";
            this.lblMetaSep3.Visible = false;
            this.lblMetaSep3.Click += new System.EventHandler(this.OnCardClick);
            this.lblMetaSep3.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblMetaSep3.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblArticleType
            // 
            this.lblArticleType.AutoSize = true;
            this.lblArticleType.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblArticleType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.lblArticleType.Location = new System.Drawing.Point(75, 2);
            this.lblArticleType.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblArticleType.Name = "lblArticleType";
            this.lblArticleType.Size = new System.Drawing.Size(42, 20);
            this.lblArticleType.TabIndex = 0;
            this.lblArticleType.Text = "SOP";
            this.lblArticleType.Visible = false;
            this.lblArticleType.Click += new System.EventHandler(this.OnCardClick);
            this.lblArticleType.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblArticleType.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCategory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.lblCategory.Location = new System.Drawing.Point(117, 2);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(14, 20);
            this.lblCategory.TabIndex = 6;
            this.lblCategory.Text = "-";
            this.lblCategory.Click += new System.EventHandler(this.OnCardClick);
            this.lblCategory.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblCategory.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblMetaSep2
            // 
            this.lblMetaSep2.AutoSize = true;
            this.lblMetaSep2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMetaSep2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(195)))), ((int)(((byte)(205)))));
            this.lblMetaSep2.Location = new System.Drawing.Point(131, 2);
            this.lblMetaSep2.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblMetaSep2.Name = "lblMetaSep2";
            this.lblMetaSep2.Size = new System.Drawing.Size(30, 20);
            this.lblMetaSep2.TabIndex = 3;
            this.lblMetaSep2.Text = "  |  ";
            this.lblMetaSep2.Click += new System.EventHandler(this.OnCardClick);
            this.lblMetaSep2.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblMetaSep2.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.lblVersion.Location = new System.Drawing.Point(161, 2);
            this.lblVersion.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(76, 20);
            this.lblVersion.TabIndex = 4;
            this.lblVersion.Text = "Version 1";
            this.lblVersion.Click += new System.EventHandler(this.OnCardClick);
            this.lblVersion.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblVersion.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblMetaSep1
            // 
            this.lblMetaSep1.AutoSize = true;
            this.lblMetaSep1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMetaSep1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(195)))), ((int)(((byte)(205)))));
            this.lblMetaSep1.Location = new System.Drawing.Point(237, 2);
            this.lblMetaSep1.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblMetaSep1.Name = "lblMetaSep1";
            this.lblMetaSep1.Size = new System.Drawing.Size(30, 20);
            this.lblMetaSep1.TabIndex = 1;
            this.lblMetaSep1.Text = "  |  ";
            this.lblMetaSep1.Click += new System.EventHandler(this.OnCardClick);
            this.lblMetaSep1.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblMetaSep1.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // pHeaderRight
            // 
            this.pHeaderRight.Controls.Add(this.pnlHeaderMeta);
            this.pHeaderRight.Controls.Add(this.lblTitleCategory);
            this.pHeaderRight.Controls.Add(this.lblTitle);
            this.pHeaderRight.Dock = System.Windows.Forms.DockStyle.Top;
            this.pHeaderRight.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pHeaderRight.Location = new System.Drawing.Point(9, 5);
            this.pHeaderRight.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pHeaderRight.Name = "pHeaderRight";
            this.pHeaderRight.Padding = new System.Windows.Forms.Padding(0, 4, 4, 0);
            this.pHeaderRight.Size = new System.Drawing.Size(645, 27);
            this.pHeaderRight.TabIndex = 6;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoEllipsis = true;
            this.lblTitle.AutoSize = true;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblTitle.Location = new System.Drawing.Point(0, 4);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(78, 20);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "ชื่อบทความ";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitle.Click += new System.EventHandler(this.OnCardClick);
            this.lblTitle.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblTitle.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblTitleCategory
            // 
            this.lblTitleCategory.AutoSize = true;
            this.lblTitleCategory.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTitleCategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitleCategory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblTitleCategory.Location = new System.Drawing.Point(78, 4);
            this.lblTitleCategory.Margin = new System.Windows.Forms.Padding(0);
            this.lblTitleCategory.Name = "lblTitleCategory";
            this.lblTitleCategory.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.lblTitleCategory.Size = new System.Drawing.Size(74, 20);
            this.lblTitleCategory.TabIndex = 12;
            this.lblTitleCategory.Text = "category";
            this.lblTitleCategory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlHeaderMeta
            // 
            this.pnlHeaderMeta.AutoSize = true;
            this.pnlHeaderMeta.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlHeaderMeta.Controls.Add(this.label4);
            this.pnlHeaderMeta.Controls.Add(this.lblShowEntity);
            this.pnlHeaderMeta.Controls.Add(this.label2);
            this.pnlHeaderMeta.Controls.Add(this.lblPublishedDate);
            this.pnlHeaderMeta.Controls.Add(this.label3);
            this.pnlHeaderMeta.Controls.Add(this.lblExpiryDate);
            this.pnlHeaderMeta.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlHeaderMeta.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlHeaderMeta.Location = new System.Drawing.Point(180, 4);
            this.pnlHeaderMeta.Margin = new System.Windows.Forms.Padding(0);
            this.pnlHeaderMeta.Name = "pnlHeaderMeta";
            this.pnlHeaderMeta.Size = new System.Drawing.Size(461, 23);
            this.pnlHeaderMeta.TabIndex = 13;
            this.pnlHeaderMeta.WrapContents = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.label4.Location = new System.Drawing.Point(0, 2);
            this.label4.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(112, 20);
            this.label4.TabIndex = 11;
            this.label4.Text = "Org./Div./Dept.";
            // 
            // lblShowEntity
            // 
            this.lblShowEntity.AutoSize = true;
            this.lblShowEntity.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShowEntity.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.lblShowEntity.Location = new System.Drawing.Point(112, 2);
            this.lblShowEntity.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblShowEntity.Name = "lblShowEntity";
            this.lblShowEntity.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.lblShowEntity.Size = new System.Drawing.Size(31, 20);
            this.lblShowEntity.TabIndex = 10;
            this.lblShowEntity.Text = "xx";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.label2.Location = new System.Drawing.Point(143, 2);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "วันที่เผยแพร่";
            // 
            // lblPublishedDate
            // 
            this.lblPublishedDate.AutoSize = true;
            this.lblPublishedDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPublishedDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.lblPublishedDate.Location = new System.Drawing.Point(227, 0);
            this.lblPublishedDate.Margin = new System.Windows.Forms.Padding(0);
            this.lblPublishedDate.Name = "lblPublishedDate";
            this.lblPublishedDate.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.lblPublishedDate.Size = new System.Drawing.Size(85, 20);
            this.lblPublishedDate.TabIndex = 1;
            this.lblPublishedDate.Text = "published";
            this.lblPublishedDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.label3.Location = new System.Drawing.Point(312, 2);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 20);
            this.label3.TabIndex = 8;
            this.label3.Text = "วันที่เผยยกเลิก";
            // 
            // lblExpiryDate
            // 
            this.lblExpiryDate.AutoSize = true;
            this.lblExpiryDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExpiryDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.lblExpiryDate.Location = new System.Drawing.Point(410, 0);
            this.lblExpiryDate.Margin = new System.Windows.Forms.Padding(0);
            this.lblExpiryDate.Name = "lblExpiryDate";
            this.lblExpiryDate.Size = new System.Drawing.Size(51, 20);
            this.lblExpiryDate.TabIndex = 9;
            this.lblExpiryDate.Text = "expire";
            this.lblExpiryDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ArticleCardControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlStripe);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.Name = "ArticleCardControl";
            this.Size = new System.Drawing.Size(666, 140);
            this.Click += new System.EventHandler(this.OnCardClick);
            this.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            this.panelFooter.ResumeLayout(false);
            this.pnlStats.ResumeLayout(false);
            this.pnlStats.PerformLayout();
            this.panelButton.ResumeLayout(false);
            this.pnlActions.ResumeLayout(false);
            this.panelArticelItem.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.pnlMeta.ResumeLayout(false);
            this.pnlMeta.PerformLayout();
            this.pHeaderRight.ResumeLayout(false);
            this.pHeaderRight.PerformLayout();
            this.pnlHeaderMeta.ResumeLayout(false);
            this.pnlHeaderMeta.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel           pnlStripe;
        private System.Windows.Forms.Panel           pnlContent;
        private System.Windows.Forms.Label           lblTitle;
        private System.Windows.Forms.FlowLayoutPanel pnlMeta;
        private System.Windows.Forms.Label           lblArticleType;
        private System.Windows.Forms.Label           lblMetaSep1;
        private System.Windows.Forms.Label           lblStatus;
        private System.Windows.Forms.Label           lblMetaSep2;
        private System.Windows.Forms.Label           lblVersion;
        private System.Windows.Forms.Label           lblMetaSep3;
        private System.Windows.Forms.Label           lblCategory;
        private System.Windows.Forms.Label           lblSummary;
        private System.Windows.Forms.Panel           pnlActions;
        private System.Windows.Forms.Button          btnSendReview;
        private System.Windows.Forms.Button          btnShare;
        private System.Windows.Forms.Button          btnEdit;
        private System.Windows.Forms.Button          btnRead;
        private System.Windows.Forms.Panel panelArticelItem;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.FlowLayoutPanel pnlStats;
        private System.Windows.Forms.Label lblViewIcon;
        private System.Windows.Forms.Label lblViewCount;
        private System.Windows.Forms.Button btnLike;
        private System.Windows.Forms.Label lblLikeCount;
        private System.Windows.Forms.Label lblTagsIcon;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.Panel panelButton;
        private System.Windows.Forms.Label lblContent;
        private System.Windows.Forms.Panel pHeaderRight;
        private System.Windows.Forms.FlowLayoutPanel pnlHeaderMeta;
        private System.Windows.Forms.Label lblExpiryDate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblPublishedDate;
        private System.Windows.Forms.Label lblShowEntity;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblTitleCategory;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
