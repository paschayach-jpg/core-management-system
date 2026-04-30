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
            this.panelButton = new System.Windows.Forms.Panel();
            this.btnSendReview = new System.Windows.Forms.Button();
            this.btnShare = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnRead = new System.Windows.Forms.Button();
            this.pnlStats = new System.Windows.Forms.FlowLayoutPanel();
            this.lblViewIcon = new System.Windows.Forms.Label();
            this.lblViewCount = new System.Windows.Forms.Label();
            this.btnLike = new System.Windows.Forms.Button();
            this.lblLikeCount = new System.Windows.Forms.Label();
            this.lblTagsIcon = new System.Windows.Forms.Label();
            this.lblTags = new System.Windows.Forms.Label();
            this.pnlActions = new System.Windows.Forms.Panel();
            this.panelArticelItem = new System.Windows.Forms.Panel();
            this.lblSummary = new System.Windows.Forms.Label();
            this.pnlMeta = new System.Windows.Forms.FlowLayoutPanel();
            this.lblArticleType = new System.Windows.Forms.Label();
            this.lblMetaSep1 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblMetaSep2 = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblMetaSep3 = new System.Windows.Forms.Label();
            this.lblCategory = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlContent.SuspendLayout();
            this.panelFooter.SuspendLayout();
            this.panelButton.SuspendLayout();
            this.pnlStats.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.panelArticelItem.SuspendLayout();
            this.pnlMeta.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlStripe
            // 
            this.pnlStripe.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.pnlStripe.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlStripe.Location = new System.Drawing.Point(0, 0);
            this.pnlStripe.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlStripe.Name = "pnlStripe";
            this.pnlStripe.Size = new System.Drawing.Size(5, 134);
            this.pnlStripe.TabIndex = 0;
            this.pnlStripe.Click += new System.EventHandler(this.OnCardClick);
            this.pnlStripe.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.pnlStripe.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.panelFooter);
            this.pnlContent.Controls.Add(this.pnlActions);
            this.pnlContent.Controls.Add(this.pnlMeta);
            this.pnlContent.Controls.Add(this.lblTitle);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(5, 0);
            this.pnlContent.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(12, 6, 10, 5);
            this.pnlContent.Size = new System.Drawing.Size(868, 134);
            this.pnlContent.TabIndex = 1;
            this.pnlContent.Click += new System.EventHandler(this.OnCardClick);
            this.pnlContent.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.pnlContent.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // panelFooter
            // 
            this.panelFooter.Controls.Add(this.panelButton);
            this.panelFooter.Controls.Add(this.pnlStats);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFooter.Location = new System.Drawing.Point(12, 87);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new System.Drawing.Size(846, 39);
            this.panelFooter.TabIndex = 5;
            // 
            // panelButton
            // 
            this.panelButton.Controls.Add(this.btnSendReview);
            this.panelButton.Controls.Add(this.btnShare);
            this.panelButton.Controls.Add(this.btnEdit);
            this.panelButton.Controls.Add(this.btnRead);
            this.panelButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelButton.Location = new System.Drawing.Point(406, 0);
            this.panelButton.Name = "panelButton";
            this.panelButton.Size = new System.Drawing.Size(440, 39);
            this.panelButton.TabIndex = 5;
            // 
            // btnSendReview
            // 
            this.btnSendReview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnSendReview.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSendReview.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSendReview.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnSendReview.FlatAppearance.BorderSize = 0;
            this.btnSendReview.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(82)))), ((int)(((byte)(190)))), ((int)(((byte)(128)))));
            this.btnSendReview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendReview.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendReview.ForeColor = System.Drawing.Color.White;
            this.btnSendReview.Location = new System.Drawing.Point(327, 0);
            this.btnSendReview.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.btnSendReview.Name = "btnSendReview";
            this.btnSendReview.Size = new System.Drawing.Size(113, 39);
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
            this.btnShare.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnShare.ForeColor = System.Drawing.Color.White;
            this.btnShare.Location = new System.Drawing.Point(218, 0);
            this.btnShare.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.btnShare.Name = "btnShare";
            this.btnShare.Size = new System.Drawing.Size(109, 39);
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
            this.btnEdit.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEdit.ForeColor = System.Drawing.Color.White;
            this.btnEdit.Location = new System.Drawing.Point(109, 0);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(109, 39);
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
            this.btnRead.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRead.ForeColor = System.Drawing.Color.White;
            this.btnRead.Location = new System.Drawing.Point(0, 0);
            this.btnRead.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(109, 39);
            this.btnRead.TabIndex = 0;
            this.btnRead.Text = "👁️ Read";
            this.btnRead.UseVisualStyleBackColor = false;
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // pnlStats
            // 
            this.pnlStats.Controls.Add(this.lblViewIcon);
            this.pnlStats.Controls.Add(this.lblViewCount);
            this.pnlStats.Controls.Add(this.btnLike);
            this.pnlStats.Controls.Add(this.lblLikeCount);
            this.pnlStats.Controls.Add(this.lblTagsIcon);
            this.pnlStats.Controls.Add(this.lblTags);
            this.pnlStats.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlStats.Location = new System.Drawing.Point(0, 0);
            this.pnlStats.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlStats.Name = "pnlStats";
            this.pnlStats.Size = new System.Drawing.Size(406, 39);
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
            this.lblViewIcon.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblViewIcon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.lblViewIcon.Location = new System.Drawing.Point(0, 0);
            this.lblViewIcon.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.lblViewIcon.Name = "lblViewIcon";
            this.lblViewIcon.Padding = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.lblViewIcon.Size = new System.Drawing.Size(41, 37);
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
            this.lblViewCount.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblViewCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.lblViewCount.Location = new System.Drawing.Point(41, 0);
            this.lblViewCount.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.lblViewCount.Name = "lblViewCount";
            this.lblViewCount.Padding = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.lblViewCount.Size = new System.Drawing.Size(28, 37);
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
            this.btnLike.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLike.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.btnLike.Location = new System.Drawing.Point(69, 0);
            this.btnLike.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.btnLike.Name = "btnLike";
            this.btnLike.Padding = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.btnLike.Size = new System.Drawing.Size(49, 37);
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
            this.lblLikeCount.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLikeCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.lblLikeCount.Location = new System.Drawing.Point(118, 0);
            this.lblLikeCount.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.lblLikeCount.Name = "lblLikeCount";
            this.lblLikeCount.Padding = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.lblLikeCount.Size = new System.Drawing.Size(28, 37);
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
            this.lblTagsIcon.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTagsIcon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(150)))), ((int)(((byte)(100)))));
            this.lblTagsIcon.Location = new System.Drawing.Point(146, 0);
            this.lblTagsIcon.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.lblTagsIcon.Name = "lblTagsIcon";
            this.lblTagsIcon.Padding = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.lblTagsIcon.Size = new System.Drawing.Size(41, 37);
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
            this.lblTags.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTags.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTags.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(150)))), ((int)(((byte)(100)))));
            this.lblTags.Location = new System.Drawing.Point(187, 0);
            this.lblTags.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.lblTags.Name = "lblTags";
            this.lblTags.Padding = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.lblTags.Size = new System.Drawing.Size(24, 37);
            this.lblTags.TabIndex = 5;
            this.lblTags.Text = "-";
            this.lblTags.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTags.Click += new System.EventHandler(this.OnCardClick);
            this.lblTags.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblTags.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // pnlActions
            // 
            this.pnlActions.Controls.Add(this.panelArticelItem);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlActions.Location = new System.Drawing.Point(12, 54);
            this.pnlActions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.pnlActions.Size = new System.Drawing.Size(846, 33);
            this.pnlActions.TabIndex = 4;
            // 
            // panelArticelItem
            // 
            this.panelArticelItem.Controls.Add(this.lblSummary);
            this.panelArticelItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelArticelItem.Location = new System.Drawing.Point(0, 3);
            this.panelArticelItem.Name = "panelArticelItem";
            this.panelArticelItem.Size = new System.Drawing.Size(846, 30);
            this.panelArticelItem.TabIndex = 6;
            // 
            // lblSummary
            // 
            this.lblSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSummary.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblSummary.Location = new System.Drawing.Point(0, 0);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(846, 30);
            this.lblSummary.TabIndex = 2;
            this.lblSummary.Text = "สรุปบทความ...";
            this.lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblSummary.Click += new System.EventHandler(this.OnCardClick);
            this.lblSummary.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblSummary.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // pnlMeta
            // 
            this.pnlMeta.Controls.Add(this.lblArticleType);
            this.pnlMeta.Controls.Add(this.lblMetaSep1);
            this.pnlMeta.Controls.Add(this.lblStatus);
            this.pnlMeta.Controls.Add(this.lblMetaSep2);
            this.pnlMeta.Controls.Add(this.lblVersion);
            this.pnlMeta.Controls.Add(this.lblMetaSep3);
            this.pnlMeta.Controls.Add(this.lblCategory);
            this.pnlMeta.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMeta.Location = new System.Drawing.Point(12, 29);
            this.pnlMeta.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlMeta.Name = "pnlMeta";
            this.pnlMeta.Size = new System.Drawing.Size(846, 25);
            this.pnlMeta.TabIndex = 1;
            this.pnlMeta.WrapContents = false;
            this.pnlMeta.Click += new System.EventHandler(this.OnCardClick);
            this.pnlMeta.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.pnlMeta.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblArticleType
            // 
            this.lblArticleType.AutoSize = true;
            this.lblArticleType.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblArticleType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.lblArticleType.Location = new System.Drawing.Point(0, 2);
            this.lblArticleType.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblArticleType.Name = "lblArticleType";
            this.lblArticleType.Size = new System.Drawing.Size(35, 19);
            this.lblArticleType.TabIndex = 0;
            this.lblArticleType.Text = "SOP";
            this.lblArticleType.Click += new System.EventHandler(this.OnCardClick);
            this.lblArticleType.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblArticleType.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblMetaSep1
            // 
            this.lblMetaSep1.AutoSize = true;
            this.lblMetaSep1.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblMetaSep1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(195)))), ((int)(((byte)(205)))));
            this.lblMetaSep1.Location = new System.Drawing.Point(35, 2);
            this.lblMetaSep1.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblMetaSep1.Name = "lblMetaSep1";
            this.lblMetaSep1.Size = new System.Drawing.Size(28, 19);
            this.lblMetaSep1.TabIndex = 1;
            this.lblMetaSep1.Text = "  |  ";
            this.lblMetaSep1.Click += new System.EventHandler(this.OnCardClick);
            this.lblMetaSep1.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblMetaSep1.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.lblStatus.Location = new System.Drawing.Point(63, 2);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(40, 19);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Draft";
            this.lblStatus.Click += new System.EventHandler(this.OnCardClick);
            this.lblStatus.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblStatus.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblMetaSep2
            // 
            this.lblMetaSep2.AutoSize = true;
            this.lblMetaSep2.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblMetaSep2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(195)))), ((int)(((byte)(205)))));
            this.lblMetaSep2.Location = new System.Drawing.Point(103, 2);
            this.lblMetaSep2.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblMetaSep2.Name = "lblMetaSep2";
            this.lblMetaSep2.Size = new System.Drawing.Size(28, 19);
            this.lblMetaSep2.TabIndex = 3;
            this.lblMetaSep2.Text = "  |  ";
            this.lblMetaSep2.Click += new System.EventHandler(this.OnCardClick);
            this.lblMetaSep2.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblMetaSep2.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.lblVersion.Location = new System.Drawing.Point(131, 2);
            this.lblVersion.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(66, 19);
            this.lblVersion.TabIndex = 4;
            this.lblVersion.Text = "Version 1";
            this.lblVersion.Click += new System.EventHandler(this.OnCardClick);
            this.lblVersion.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblVersion.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblMetaSep3
            // 
            this.lblMetaSep3.AutoSize = true;
            this.lblMetaSep3.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblMetaSep3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(195)))), ((int)(((byte)(205)))));
            this.lblMetaSep3.Location = new System.Drawing.Point(197, 2);
            this.lblMetaSep3.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblMetaSep3.Name = "lblMetaSep3";
            this.lblMetaSep3.Size = new System.Drawing.Size(28, 19);
            this.lblMetaSep3.TabIndex = 5;
            this.lblMetaSep3.Text = "  |  ";
            this.lblMetaSep3.Click += new System.EventHandler(this.OnCardClick);
            this.lblMetaSep3.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblMetaSep3.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCategory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(100)))), ((int)(((byte)(130)))));
            this.lblCategory.Location = new System.Drawing.Point(225, 2);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(15, 19);
            this.lblCategory.TabIndex = 6;
            this.lblCategory.Text = "-";
            this.lblCategory.Click += new System.EventHandler(this.OnCardClick);
            this.lblCategory.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblCategory.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(102)))));
            this.lblTitle.Location = new System.Drawing.Point(12, 6);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(846, 23);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "ชื่อบทความ";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitle.Click += new System.EventHandler(this.OnCardClick);
            this.lblTitle.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.lblTitle.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            // 
            // ArticleCardControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlStripe);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.Name = "ArticleCardControl";
            this.Size = new System.Drawing.Size(873, 134);
            this.Click += new System.EventHandler(this.OnCardClick);
            this.MouseEnter += new System.EventHandler(this.OnCardMouseEnter);
            this.MouseLeave += new System.EventHandler(this.OnCardMouseLeave);
            this.pnlContent.ResumeLayout(false);
            this.panelFooter.ResumeLayout(false);
            this.panelButton.ResumeLayout(false);
            this.pnlStats.ResumeLayout(false);
            this.pnlStats.PerformLayout();
            this.pnlActions.ResumeLayout(false);
            this.panelArticelItem.ResumeLayout(false);
            this.pnlMeta.ResumeLayout(false);
            this.pnlMeta.PerformLayout();
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
        private System.Windows.Forms.Label           lblViewIcon;
        private System.Windows.Forms.Label           lblViewCount;
        private System.Windows.Forms.Button          btnLike;
        private System.Windows.Forms.Label           lblLikeCount;
        private System.Windows.Forms.Label           lblTagsIcon;
        private System.Windows.Forms.Label           lblTags;
        private System.Windows.Forms.Panel           pnlActions;
        private System.Windows.Forms.Button          btnSendReview;
        private System.Windows.Forms.Button          btnShare;
        private System.Windows.Forms.Button          btnEdit;
        private System.Windows.Forms.Button          btnRead;
        private System.Windows.Forms.FlowLayoutPanel pnlStats;
        private System.Windows.Forms.Panel panelButton;
        private System.Windows.Forms.Panel panelArticelItem;
        private System.Windows.Forms.Panel panelFooter;
    }
}
