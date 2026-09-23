using System;
using System.Drawing;
using System.Windows.Forms;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// UserControl แสดงบทความ 1 รายการในรูปแบบ Card
    /// Layout ทั้งหมดอยู่ใน ArticleCardControl.Designer.cs
    /// Code-behind นี้มีเฉพาะ event handlers + data binding
    /// </summary>
    public partial class ArticleCardControl : UserControl
    {
        private KBArticle _article;

        // ── Public events ────────────────────────────────────────────────────────
        public event EventHandler<KBArticle> CardClicked;
        public event EventHandler<KBArticle> ReadClicked;
        public event EventHandler<KBArticle> EditClicked;
        public event EventHandler<KBArticle> ShareClicked;
        public event EventHandler<KBArticle> SendReviewClicked;
        public event EventHandler<KBArticle> LikeClicked;

        public ArticleCardControl()
        {
            InitializeComponent();
            flowLayoutPanel1.SizeChanged += (s, e) => UpdateCardHeight();
        }

        /// <summary>
        /// flowLayoutPanel1 เป็น AutoSize ตาม lblSummary แล้ว (ไม่ fix width/height) — ส่วนพี่น้องที่เหลือ
        /// ใน pnlContent (pHeaderRight/pnlMeta/pnlActions/panelFooter) ยังคงสูงคงที่ จึงรวมความสูงทั้งหมด
        /// แล้วปรับความสูงของการ์ดทั้งใบตาม เพื่อไม่ให้ summary ที่ยาวขึ้นโดนตัด/บีบซ่อนอยู่ใน panel ที่ fix สูงเท่าเดิม
        /// </summary>
        private void UpdateCardHeight()
        {
            int total = pnlContent.Padding.Top + pnlContent.Padding.Bottom
                      + pHeaderRight.Height + pnlMeta.Height + flowLayoutPanel1.Height
                      + pnlActions.Height + panelFooter.Height;

            if (total > 0 && Height != total)
                Height = total;
        }

        // ── Data binding ─────────────────────────────────────────────────────────

        public KBArticle Article
        {
            get { return _article; }
            set { _article = value; if (value != null) BindArticle(value); }
        }

        private void BindArticle(KBArticle a)
        {
            pnlStripe.BackColor   = GetStatusColor(a.Status);
            lblTitle.Text         = a.IsOfficialPolicy ? "🏛 " + a.Title : a.Title ?? "(ไม่มีชื่อ)";
            lblPublishedDate.Text = !string.IsNullOrEmpty(a.PublishedDate?.ToString()) ? a.PublishedDate.Value.ToString("dd/MM/yyyy") : "-";
            lblExpiryDate.Text    = !string.IsNullOrEmpty(a.ExpiryDate?.ToString()) ? a.ExpiryDate.Value.ToString("dd/MM/yyyy") : "-";
            lblArticleType.Text   = a.ArticleTypeName;
            lblContent.Text       = a.ContentHTML.Length >= 185 ? $"{a.ContentHTML.Substring(0, 185)}...read more" : a.ContentHTML;
            lblStatus.Text        = a.StatusName;
            lblVersion.Text       = "Version " + a.CurrentVersion;
            lblCategory.Text      = a.CategoryName ?? "-";
            lblTitleCategory.Text = a.TitleCategory ?? "-";
            lblSummary.Text       = a.Summary ?? string.Empty;
            lblShowEntity.Text    = a.LinkedEntities ?? "-";
            lblViewCount.Text     = a.ViewCount.ToString();
            lblLikeCount.Text     = a.LikeCount.ToString();
            lblTags.Text          = string.IsNullOrEmpty(a.Tags) ? "-" : a.Tags;
            btnSendReview.Visible = (a.Status == 1);

            UpdateCardHeight();
        }

        private static Color GetStatusColor(byte status)
        {
            switch (status)
            {
                case 1:  return Color.FromArgb(180, 180, 180); // Draft
                case 2:  return Color.FromArgb(255, 165, 0);   // Review
                case 3:  return Color.FromArgb(34,  139, 34);  // Published
                case 4:  return Color.FromArgb(139, 0,   0);   // Archived
                default: return Color.FromArgb(200, 200, 200);
            }
        }

        // ── Hover handlers (wired in Designer.cs) ───────────────────────────────

        private void OnCardMouseEnter(object sender, EventArgs e)
        {
            BackColor = Color.FromArgb(240, 246, 255);
        }

        private void OnCardMouseLeave(object sender, EventArgs e)
        {
            BackColor = Color.White;
        }

        // ── Click handlers (wired in Designer.cs) ───────────────────────────────

        private void OnCardClick(object sender, EventArgs e)
        {
            if (_article == null) return;
            if (CardClicked != null) CardClicked(this, _article);
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            if (_article == null) return;
            if (ReadClicked != null) ReadClicked(this, _article);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (_article == null) return;
            if (EditClicked != null) EditClicked(this, _article);
        }

        private void btnShare_Click(object sender, EventArgs e)
        {
            if (_article == null) return;
            if (ShareClicked != null) ShareClicked(this, _article);
        }

        private void btnSendReview_Click(object sender, EventArgs e)
        {
            if (_article == null) return;
            if (SendReviewClicked != null) SendReviewClicked(this, _article);
        }

        private void btnLike_Click(object sender, EventArgs e)
        {
            if (_article == null) return;
            
            // Optimistic UI update — increment immediately for responsiveness
            _article.LikeCount++;
            lblLikeCount.Text = _article.LikeCount.ToString();
            
            // Fire event so parent can persist to DB
            if (LikeClicked != null) LikeClicked(this, _article);
        }
    }
}
