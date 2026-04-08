using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// หน้าหลัก Knowledge Base System (MOD-006)
    /// Layout: SplitContainer — ซ้าย=Category TreeView, ขวา=Article List + Search
    /// รองรับ: Browse by Category, Full-text Search, Add/Edit/Archive Article, Version History
    /// </summary>
    public partial class KnowledgeBaseForm : Form
    {
        private readonly IKBCategoryService _categoryService;
        private readonly IKBArticleService  _articleService;
        private readonly UserContext        _user;

        private List<KBCategory> _allCategories = new List<KBCategory>();
        private List<KBArticle>  _currentArticles = new List<KBArticle>();
        private int?             _selectedCategoryId = null;

        // Status filter values  (index → status byte)
        private static readonly byte?[] StatusFilterValues =
            { null, 1, 2, 3, 4 };   // null=ทั้งหมด, Draft, Review, Published, Archived

        // Article type filter
        private static readonly byte?[] TypeFilterValues =
            { null, 1, 2, 3, 4, 5 };  // null=ทั้งหมด, SOP, FAQ, Guideline, Template, Policy

        public KnowledgeBaseForm(
            IKBCategoryService categoryService,
            IKBArticleService  articleService,
            UserContext        user)
        {
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (user            == null) throw new ArgumentNullException("user");
            _categoryService = categoryService;
            _articleService  = articleService;
            _user            = user;
            InitializeComponent();
        }

        // ── Form Load ─────────────────────────────────────────────────────────

        private void KnowledgeBaseForm_Load(object sender, EventArgs e)
        {
            InitFilterCombo();
            LoadCategoryTree();
        }

        private void KnowledgeBaseForm_Resize(object sender, EventArgs e)
        {
            ResizeArticleCards();
        }

        // ── Init ──────────────────────────────────────────────────────────────

        private void InitFilterCombo()
        {
            cboFilter.Items.Clear();
            cboFilter.Items.Add("ทุกสถานะ");
            cboFilter.Items.Add("📝 Draft");
            cboFilter.Items.Add("🔍 รอ Review");
            cboFilter.Items.Add("✅ Published");
            cboFilter.Items.Add("📦 Archived");
            cboFilter.SelectedIndex = 0;
        }

        // ── Category Tree ─────────────────────────────────────────────────────

        private void LoadCategoryTree()
        {
            try
            {
                _allCategories = _categoryService.GetAll().ToList();
                BuildCategoryTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดหมวดหมู่ไม่สำเร็จ:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuildCategoryTree()
        {
            treeCategories.BeginUpdate();
            treeCategories.Nodes.Clear();

            // Root node — Show all
            var rootNode = new TreeNode("📚 ทั้งหมด") { Tag = null };
            rootNode.ForeColor = Color.FromArgb(0, 51, 102);
            rootNode.NodeFont  = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            treeCategories.Nodes.Add(rootNode);

            // Build root categories (ParentCategoryID == null)
            var roots = _allCategories.Where(c => c.IsActive && !c.ParentCategoryID.HasValue)
                                      .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName);
            foreach (var cat in roots)
                rootNode.Nodes.Add(BuildCategoryNode(cat));

            treeCategories.EndUpdate();
            treeCategories.ExpandAll();
            treeCategories.SelectedNode = rootNode;
        }

        private TreeNode BuildCategoryNode(KBCategory cat)
        {
            string icon = string.IsNullOrEmpty(cat.Icon) ? "📁" : "📁";
            var node = new TreeNode(string.Format("{0} {1}", icon, cat.CategoryName))
            {
                Tag       = cat,
                ForeColor = Color.FromArgb(30, 30, 30),
            };

            // Recurse children
            var children = _allCategories.Where(c => c.IsActive && c.ParentCategoryID == cat.CategoryID)
                                         .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName);
            foreach (var child in children)
                node.Nodes.Add(BuildCategoryNode(child));

            return node;
        }

        private void TreeCategories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;
            var cat = e.Node.Tag as KBCategory;
            _selectedCategoryId = cat?.CategoryID;

            lblArticleTitle.Text = cat != null
                ? string.Format("  {0}", cat.CategoryName)
                : "  ทุกหมวดหมู่";

            LoadArticles();
        }

        // ── Article Loading ───────────────────────────────────────────────────

        private void LoadArticles()
        {
            try
            {
                string keyword   = txtSearch.Text.Trim();
                byte?  statusFilter = GetSelectedStatusFilter();

                if (string.IsNullOrEmpty(keyword) && !_selectedCategoryId.HasValue && !statusFilter.HasValue)
                    _currentArticles = _articleService.GetAll().ToList();
                else if (string.IsNullOrEmpty(keyword) && !statusFilter.HasValue && _selectedCategoryId.HasValue)
                    _currentArticles = _articleService.GetByCategory(_selectedCategoryId.Value).ToList();
                else
                    _currentArticles = _articleService.Search(
                        string.IsNullOrEmpty(keyword) ? null : keyword,
                        _selectedCategoryId,
                        statusFilter,
                        null).ToList();

                RenderArticleCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดบทความไม่สำเร็จ:\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private byte? GetSelectedStatusFilter()
        {
            int idx = cboFilter.SelectedIndex;
            if (idx < 0 || idx >= StatusFilterValues.Length) return null;
            return StatusFilterValues[idx];
        }

        // ── Article Cards ─────────────────────────────────────────────────────

        private void RenderArticleCards()
        {
            flpArticles.SuspendLayout();
            flpArticles.Controls.Clear();

            foreach (var article in _currentArticles)
                flpArticles.Controls.Add(BuildArticleCard(article));

            flpArticles.ResumeLayout();
            ResizeArticleCards();

            lblArticleCount.Text = string.Format("{0} บทความ", _currentArticles.Count);
        }

        private Panel BuildArticleCard(KBArticle article)
        {
            var card = new Panel
            {
                BackColor  = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor     = Cursors.Hand,
                Margin     = new Padding(0, 0, 0, 6),
                Tag        = article,
            };
            card.Click += (s, e) => OnArticleCardClick(article);

            // Colored left stripe per status
            var stripe = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 5,
                BackColor = GetStatusColor(article.Status),
            };
            stripe.Click += (s, e) => OnArticleCardClick(article);
            card.Controls.Add(stripe);

            // Content area
            var pnlContent = new Panel
            {
                Dock    = DockStyle.Fill,
                Padding = new Padding(12, 8, 10, 6),
            };
            pnlContent.Click += (s, e) => OnArticleCardClick(article);

            // Title
            var lblCardTitle = new Label
            {
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 22,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 51, 102),
                Text      = article.Title ?? "(ไม่มีชื่อ)",
                TextAlign = ContentAlignment.MiddleLeft,
            };
            lblCardTitle.Click += (s, e) => OnArticleCardClick(article);

            // Meta row: Type | Status | Version | Category
            var lblMeta = new Label
            {
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 18,
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(100, 110, 130),
                Text      = string.Format("{0}  |  {1}  |  Version {2}  |  {3}",
                    article.ArticleTypeName,
                    article.StatusName,
                    article.CurrentVersion,
                    article.CategoryName ?? "-"),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            lblMeta.Click += (s, e) => OnArticleCardClick(article);

            // Summary
            var lblSummary = new Label
            {
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 20,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(60, 60, 60),
                Text      = article.Summary ?? string.Empty,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            lblSummary.Click += (s, e) => OnArticleCardClick(article);

            // Tags + stats row
            string statsText = string.Format("👁 {0}   👍 {1}   🏷 {2}",
                article.ViewCount, article.LikeCount,
                string.IsNullOrEmpty(article.Tags) ? "-" : article.Tags);
            var lblStats = new Label
            {
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 18,
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(130, 140, 155),
                Text      = statsText,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            lblStats.Click += (s, e) => OnArticleCardClick(article);

            // Official Policy badge
            if (article.IsOfficialPolicy)
            {
                lblCardTitle.Text = "🏛 " + lblCardTitle.Text;
                lblCardTitle.ForeColor = Color.FromArgb(180, 80, 0);
            }

            // Add from bottom to top (DockStyle.Top stacks top-down)
            pnlContent.Controls.Add(lblStats);
            pnlContent.Controls.Add(lblSummary);
            pnlContent.Controls.Add(lblMeta);
            pnlContent.Controls.Add(lblCardTitle);

            card.Controls.Add(pnlContent);
            card.Height = 90;

            // Hover effect
            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(240, 246, 255);
            card.MouseLeave += (s, e) => card.BackColor = Color.White;

            return card;
        }

        private static Color GetStatusColor(byte status)
        {
            switch (status)
            {
                case 1:  return Color.FromArgb(180, 180, 180); // Draft - Gray
                case 2:  return Color.FromArgb(255, 165, 0);   // Review - Orange
                case 3:  return Color.FromArgb(34, 139, 34);   // Published - Green
                case 4:  return Color.FromArgb(139, 0, 0);     // Archived - Red
                default: return Color.FromArgb(200, 200, 200);
            }
        }

        private void ResizeArticleCards()
        {
            int cardWidth = flpArticles.ClientSize.Width - 16;
            if (cardWidth < 200) return;
            foreach (Control c in flpArticles.Controls)
            {
                if (c is Panel) c.Width = cardWidth;
            }
        }

        private void OnArticleCardClick(KBArticle article)
        {
            // Increment view count (fire-and-forget)
            try { _articleService.IncrementViewCount(article.ArticleID); }
            catch { /* non-critical */ }

            using (var dlg = new ArticleEditorDialog(
                _categoryService, _articleService, _user, article))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    LoadArticles();
            }
        }

        // ── Button Handlers ───────────────────────────────────────────────────

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadArticles();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) LoadArticles();
        }

        private void CboFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadArticles();
        }

        private void BtnNewArticle_Click(object sender, EventArgs e)
        {
            using (var dlg = new ArticleEditorDialog(
                _categoryService, _articleService, _user, null))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    LoadArticles();
            }
        }

        private void BtnNewCategory_Click(object sender, EventArgs e)
        {
            // เปิด KBCategoryForm เพื่อตั้งค่าหมวดหมู่
            using (var frm = new KBCategoryForm(_categoryService, _user))
                frm.ShowDialog(this);

            // Reload tree after managing categories
            LoadCategoryTree();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (var frm = new KBCategoryForm(_categoryService, _user))
                frm.ShowDialog(this);
            LoadCategoryTree();
        }

        private void BtnImportExcel_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "ฟังก์ชัน Import Excel อยู่ระหว่างการพัฒนา\n\n" +
                "รองรับการ Import บทความจาก Template Excel ที่ระบบกำหนด\n" +
                "ดาวน์โหลด Template: ใช้ปุ่ม Export ที่หน้า Reports",
                "Import Excel — MOD-006",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            var stats = BuildReportText();
            MessageBox.Show(stats, "รายงาน Knowledge Base",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string BuildReportText()
        {
            try
            {
                var all = _articleService.GetAll().ToList();
                return string.Format(
                    "📊 สรุปรายงาน Knowledge Base\n\n" +
                    "บทความทั้งหมด : {0}\n" +
                    "📝 Draft       : {1}\n" +
                    "🔍 รอ Review   : {2}\n" +
                    "✅ Published   : {3}\n" +
                    "📦 Archived    : {4}\n\n" +
                    "👁 ยอดชมรวม   : {5}\n" +
                    "👍 ยอด Like รวม: {6}",
                    all.Count,
                    all.Count(a => a.Status == 1),
                    all.Count(a => a.Status == 2),
                    all.Count(a => a.Status == 3),
                    all.Count(a => a.Status == 4),
                    all.Sum(a => a.ViewCount),
                    all.Sum(a => a.LikeCount));
            }
            catch
            {
                return "ไม่สามารถโหลดรายงานได้";
            }
        }
    }
}
