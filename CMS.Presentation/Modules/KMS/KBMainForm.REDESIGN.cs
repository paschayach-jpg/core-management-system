// Enhanced BuildArticleCard method with modern design from HTML
private Panel BuildArticleCard(KBArticle article)
{
    var card = new Panel
    {
        BackColor   = Color.White,
        BorderStyle = BorderStyle.FixedSingle,
        Cursor      = Cursors.Hand,
        Margin      = new Padding(0, 0, 0, 8),
        Tag         = article,
        Height      = 110,
    };
    card.Click += (s, e) => OnArticleCardClick(article);

    // Left colored stripe based on status
    var stripe = new Panel 
    { 
        Dock = DockStyle.Left, 
        Width = 3, 
        BackColor = GetStatusColor(article.Status) 
    };
    stripe.Click += (s, e) => OnArticleCardClick(article);
    card.Controls.Add(stripe);

    var pnlContent = new Panel 
    { 
        Dock = DockStyle.Fill, 
        Padding = new Padding(12, 10, 12, 8) 
    };
    pnlContent.Click += (s, e) => OnArticleCardClick(article);

    // Card Header with badges
    var pnlCardHeader = new Panel 
    { 
        Dock = DockStyle.Top, 
        Height = 24 
    };
    pnlCardHeader.Click += (s, e) => OnArticleCardClick(article);

    // Type Badge (SOP, FAQ, Wiki, etc.)
    var lblTypeBadge = new Label
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
        Padding = new Padding(6, 2, 6, 2),
        Text = GetArticleTypeIcon(article.ArticleTypeName) + " " + article.ArticleTypeName,
        Location = new Point(0, 2),
    };
    SetTypeBadgeStyle(lblTypeBadge, article.ArticleTypeName);
    lblTypeBadge.Click += (s, e) => OnArticleCardClick(article);
    pnlCardHeader.Controls.Add(lblTypeBadge);

    // CRAF Badge (if applicable)
    if (article.ConfidentialityLevel > 0)
    {
        var lblCrafBadge = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            Padding = new Padding(5, 2, 5, 2),
            Text = GetCRAFText(article.ConfidentialityLevel),
            ForeColor = Color.White,
            BackColor = GetCRAFColor(article.ConfidentialityLevel),
            Location = new Point(lblTypeBadge.Right + 6, 2),
        };
        lblCrafBadge.Click += (s, e) => OnArticleCardClick(article);
        pnlCardHeader.Controls.Add(lblCrafBadge);
    }

    pnlContent.Controls.Add(pnlCardHeader);

    // Title
    var lblCardTitle = new Label
    {
        AutoSize  = false, 
        Dock = DockStyle.Top, 
        Height = 22,
        Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
        ForeColor = Color.FromArgb(26, 35, 50),
        Text      = article.IsOfficialPolicy ? "🏛 " + article.Title : article.Title ?? "(ไม่มีชื่อ)",
        TextAlign = ContentAlignment.MiddleLeft,
    };
    lblCardTitle.Click += (s, e) => OnArticleCardClick(article);
    pnlContent.Controls.Add(lblCardTitle);

    // Meta info
    var lblMeta = new Label
    {
        AutoSize  = false, 
        Dock = DockStyle.Top, 
        Height = 18,
        Font      = new Font("Segoe UI", 7.5f), 
        ForeColor = Color.FromArgb(127, 140, 141),
        Text      = string.Format("{0}  |  Version {1}  |  {2}  |  {3}",
            article.StatusName,
            article.CurrentVersion, 
            article.CategoryName ?? "-",
            article.UpdatedDate?.ToString("dd MMM yy") ?? article.CreatedDate.ToString("dd MMM yy")),
        TextAlign = ContentAlignment.MiddleLeft,
    };
    lblMeta.Click += (s, e) => OnArticleCardClick(article);
    pnlContent.Controls.Add(lblMeta);

    // Tags panel
    if (!string.IsNullOrEmpty(article.Tags))
    {
        var pnlTags = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 22,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = false,
        };
        pnlTags.Click += (s, e) => OnArticleCardClick(article);

        var tags = article.Tags.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var tag in tags.Take(5))
        {
            var lblTag = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 7f),
                Padding = new Padding(6, 2, 6, 2),
                Margin = new Padding(0, 2, 4, 0),
                Text = tag.Trim(),
                BackColor = Color.FromArgb(245, 247, 250),
                ForeColor = Color.FromArgb(74, 85, 104),
                BorderStyle = BorderStyle.FixedSingle,
            };
            lblTag.Click += (s, e) => OnArticleCardClick(article);
            pnlTags.Controls.Add(lblTag);
        }
        pnlContent.Controls.Add(pnlTags);
    }

    // Footer with stats
    var pnlFooter = new Panel
    {
        Dock = DockStyle.Top,
        Height = 20,
        Padding = new Padding(0, 4, 0, 0),
    };
    pnlFooter.Click += (s, e) => OnArticleCardClick(article);

    var lblStats = new Label
    {
        AutoSize  = false, 
        Dock = DockStyle.Fill,
        Font      = new Font("Segoe UI", 7.5f), 
        ForeColor = Color.FromArgb(127, 140, 141),
        Text      = string.Format("👁 {0} views   👍 {1} likes   ⭐ {2:0.0}",
            article.ViewCount, 
            article.LikeCount,
            article.AverageRating),
        TextAlign = ContentAlignment.MiddleLeft,
    };
    lblStats.Click += (s, e) => OnArticleCardClick(article);
    pnlFooter.Controls.Add(lblStats);

    // Status badge on the right
    var lblStatusBadge = new Label
    {
        AutoSize = true,
        Dock = DockStyle.Right,
        Font = new Font("Segoe UI", 7f, FontStyle.Bold),
        Padding = new Padding(6, 2, 6, 2),
        Text = article.StatusName,
        TextAlign = ContentAlignment.MiddleRight,
    };
    SetStatusBadgeStyle(lblStatusBadge, article.Status);
    lblStatusBadge.Click += (s, e) => OnArticleCardClick(article);
    pnlFooter.Controls.Add(lblStatusBadge);

    pnlContent.Controls.Add(pnlFooter);
    card.Controls.Add(pnlContent);

    // Hover effects
    card.MouseEnter += (s, e) => 
    {
        card.BackColor = Color.FromArgb(240, 246, 255);
        card.BorderStyle = BorderStyle.FixedSingle;
    };
    card.MouseLeave += (s, e) => 
    {
        card.BackColor = Color.White;
    };

    return card;
}

// Helper methods for styling
private string GetArticleTypeIcon(string typeName)
{
    if (typeName == null) return "📄";
    if (typeName.Contains("SOP")) return "📋";
    if (typeName.Contains("FAQ")) return "❓";
    if (typeName.Contains("Wiki")) return "📖";
    if (typeName.Contains("Guide") || typeName.Contains("แนวทาง")) return "📘";
    if (typeName.Contains("Template")) return "📝";
    return "📄";
}

private void SetTypeBadgeStyle(Label label, string typeName)
{
    if (typeName == null) typeName = "";
    
    if (typeName.Contains("SOP"))
    {
        label.BackColor = Color.FromArgb(253, 232, 216);
        label.ForeColor = Color.FromArgb(192, 57, 43);
    }
    else if (typeName.Contains("FAQ"))
    {
        label.BackColor = Color.FromArgb(254, 249, 231);
        label.ForeColor = Color.FromArgb(183, 119, 13);
    }
    else if (typeName.Contains("Wiki"))
    {
        label.BackColor = Color.FromArgb(232, 218, 239);
        label.ForeColor = Color.FromArgb(108, 52, 131);
    }
    else if (typeName.Contains("Guide") || typeName.Contains("แนวทาง"))
    {
        label.BackColor = Color.FromArgb(214, 234, 248);
        label.ForeColor = Color.FromArgb(31, 97, 141);
    }
    else if (typeName.Contains("Template"))
    {
        label.BackColor = Color.FromArgb(213, 245, 227);
        label.ForeColor = Color.FromArgb(30, 132, 73);
    }
    else
    {
        label.BackColor = Color.FromArgb(245, 247, 250);
        label.ForeColor = Color.FromArgb(74, 85, 104);
    }
}

private void SetStatusBadgeStyle(Label label, byte status)
{
    switch (status)
    {
        case 1: // Draft
            label.BackColor = Color.FromArgb(250, 219, 216);
            label.ForeColor = Color.FromArgb(146, 43, 33);
            break;
        case 2: // In Review
            label.BackColor = Color.FromArgb(254, 249, 231);
            label.ForeColor = Color.FromArgb(183, 119, 13);
            break;
        case 3: // Published
            label.BackColor = Color.FromArgb(213, 245, 227);
            label.ForeColor = Color.FromArgb(30, 132, 73);
            break;
        case 4: // Archived
            label.BackColor = Color.FromArgb(242, 243, 244);
            label.ForeColor = Color.FromArgb(127, 140, 141);
            break;
        default:
            label.BackColor = Color.FromArgb(245, 247, 250);
            label.ForeColor = Color.FromArgb(74, 85, 104);
            break;
    }
}

private string GetCRAFText(int level)
{
    switch (level)
    {
        case 1: return "1HC";
        case 2: return "2SC";
        case 3: return "3NC";
        case 4: return "4PC";
        default: return "";
    }
}

private Color GetCRAFColor(int level)
{
    switch (level)
    {
        case 1: return Color.FromArgb(231, 76, 60);   // Danger - Red
        case 2: return Color.FromArgb(230, 126, 34);  // Warning - Orange
        case 3: return Color.FromArgb(39, 174, 96);   // Success - Green
        case 4: return Color.FromArgb(127, 140, 141); // Archived - Gray
        default: return Color.FromArgb(127, 140, 141);
    }
}

private static Color GetStatusColor(byte status)
{
    switch (status)
    {
        case 1:  return Color.FromArgb(180, 180, 180);  // Draft - Gray
        case 2:  return Color.FromArgb(243, 156, 18);   // Review - Orange
        case 3:  return Color.FromArgb(39, 174, 96);    // Published - Green
        case 4:  return Color.FromArgb(127, 140, 141);  // Archived - Dark Gray
        default: return Color.FromArgb(200, 200, 200);
    }
}
