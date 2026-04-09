using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.KMS
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

        private List<KBCategory>  _allCategories      = new List<KBCategory>();
        private List<KBArticle>   _currentArticles    = new List<KBArticle>();
        private int?              _selectedCategoryId = null;

        // ── Viewer state (Task 3) ──────────────────────────────────────────────
        private ArticleViewerForm _activeViewer          = null;
        private int?              _activeViewerArticleId = null;

        private static readonly byte?[] StatusFilterValues = { null, 1, 2, 3, 4 };
        private static readonly byte?[] TypeFilterValues   = { null, 1, 2, 3, 4, 5 };

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

        private void KnowledgeBaseForm_Load(object sender, EventArgs e)
        {
            InitFilterCombo();
            LoadCategoryTree();
        }

        private void KnowledgeBaseForm_Resize(object sender, EventArgs e) { ResizeArticleCards(); }

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
            // โหลดจำนวนบทความต่อ Category เพื่อแสดงใน TreeView
            var articleCounts = new System.Collections.Generic.Dictionary<int, int>();
            try
            {
                foreach (var a in _articleService.GetAll())
                {
                    if (!articleCounts.ContainsKey(a.CategoryID))
                        articleCounts[a.CategoryID] = 0;
                    articleCounts[a.CategoryID]++;
                }
            }
            catch { /* counts stay empty — non-critical */ }

            treeCategories.BeginUpdate();
            treeCategories.Nodes.Clear();

            int totalCount = 0;
            foreach (var v in articleCounts.Values) totalCount += v;
            string rootLabel = totalCount > 0
                ? string.Format("📚 ทั้งหมด ({0})", totalCount)
                : "📚 ทั้งหมด";

            var rootNode = new TreeNode(rootLabel) { Tag = null };
            rootNode.ForeColor = Color.FromArgb(0, 51, 102);
            rootNode.NodeFont  = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            treeCategories.Nodes.Add(rootNode);

            var roots = _allCategories.Where(c => c.IsActive && !c.ParentCategoryID.HasValue)
                                      .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName);
            foreach (var cat in roots)
                rootNode.Nodes.Add(BuildCategoryNode(cat, articleCounts));

            treeCategories.EndUpdate();
            treeCategories.ExpandAll();
            treeCategories.SelectedNode = rootNode;
        }

        private TreeNode BuildCategoryNode(KBCategory cat,
            System.Collections.Generic.Dictionary<int, int> articleCounts)
        {
            int count = articleCounts.ContainsKey(cat.CategoryID) ? articleCounts[cat.CategoryID] : 0;
            string label = count > 0
                ? string.Format("📁 {0} ({1})", cat.CategoryName, count)
                : string.Format("📁 {0}", cat.CategoryName);

            var node = new TreeNode(label)
            {
                Tag       = cat,
                ForeColor = Color.FromArgb(30, 30, 30),
            };
            var children = _allCategories.Where(c => c.IsActive && c.ParentCategoryID == cat.CategoryID)
                                         .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName);
            foreach (var child in children)
                node.Nodes.Add(BuildCategoryNode(child, articleCounts));
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

        private void LoadArticles()
        {
            try
            {
                string keyword      = txtSearch.Text.Trim();
                byte?  statusFilter = GetSelectedStatusFilter();

                if (string.IsNullOrEmpty(keyword) && !_selectedCategoryId.HasValue && !statusFilter.HasValue)
                    _currentArticles = _articleService.GetAll().ToList();
                else if (string.IsNullOrEmpty(keyword) && !statusFilter.HasValue && _selectedCategoryId.HasValue)
                    _currentArticles = _articleService.GetByCategory(_selectedCategoryId.Value).ToList();
                else
                    _currentArticles = _articleService.Search(
                        string.IsNullOrEmpty(keyword) ? null : keyword,
                        _selectedCategoryId, statusFilter, null).ToList();

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
                BackColor   = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor      = Cursors.Hand,
                Margin      = new Padding(0, 0, 0, 6),
                Tag         = article,
            };
            card.Click += (s, e) => OnArticleCardClick(article);

            var stripe = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = GetStatusColor(article.Status) };
            stripe.Click += (s, e) => OnArticleCardClick(article);
            card.Controls.Add(stripe);

            var pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 8, 10, 6) };
            pnlContent.Click += (s, e) => OnArticleCardClick(article);

            var lblCardTitle = new Label
            {
                AutoSize = false, Dock = DockStyle.Top, Height = 22,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 51, 102),
                Text = article.IsOfficialPolicy ? "🏛 " + article.Title : article.Title ?? "(ไม่มีชื่อ)",
                TextAlign = ContentAlignment.MiddleLeft,
            };
            lblCardTitle.Click += (s, e) => OnArticleCardClick(article);

            var lblMeta = new Label
            {
                AutoSize = false, Dock = DockStyle.Top, Height = 18,
                Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 110, 130),
                Text = string.Format("{0}  |  {1}  |  Version {2}  |  {3}",
                    article.ArticleTypeName, article.StatusName,
                    article.CurrentVersion, article.CategoryName ?? "-"),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            lblMeta.Click += (s, e) => OnArticleCardClick(article);

            var lblSummary = new Label
            {
                AutoSize = false, Dock = DockStyle.Top, Height = 20,
                Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(60, 60, 60),
                Text = article.Summary ?? string.Empty, TextAlign = ContentAlignment.MiddleLeft,
            };
            lblSummary.Click += (s, e) => OnArticleCardClick(article);

            var lblStats = new Label
            {
                AutoSize = false, Dock = DockStyle.Top, Height = 18,
                Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(130, 140, 155),
                Text = string.Format("👁 {0}   👍 {1}   🏷 {2}",
                    article.ViewCount, article.LikeCount,
                    string.IsNullOrEmpty(article.Tags) ? "-" : article.Tags),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            lblStats.Click += (s, e) => OnArticleCardClick(article);

            pnlContent.Controls.Add(lblStats);
            pnlContent.Controls.Add(lblSummary);
            pnlContent.Controls.Add(lblMeta);
            pnlContent.Controls.Add(lblCardTitle);
            card.Controls.Add(pnlContent);
            card.Height = 90;

            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(240, 246, 255);
            card.MouseLeave += (s, e) => card.BackColor = Color.White;
            return card;
        }

        private static Color GetStatusColor(byte status)
        {
            switch (status)
            {
                case 1:  return Color.FromArgb(180, 180, 180);
                case 2:  return Color.FromArgb(255, 165, 0);
                case 3:  return Color.FromArgb(34, 139, 34);
                case 4:  return Color.FromArgb(139, 0, 0);
                default: return Color.FromArgb(200, 200, 200);
            }
        }

        private void ResizeArticleCards()
        {
            int cardWidth = flpArticles.ClientSize.Width - 16;
            if (cardWidth < 200) return;
            foreach (Control c in flpArticles.Controls)
                if (c is Panel) c.Width = cardWidth;
        }

        private void OnArticleCardClick(KBArticle article)
        {
            try { _articleService.IncrementViewCount(article.ArticleID); }
            catch { /* non-critical */ }

            ShowArticleViewer(article);
        }

        // ── Viewer navigation (Task 3) ────────────────────────────────────────

        private void ShowArticleViewer(KBArticle article)
        {
            // Same article already open — do nothing
            if (_activeViewerArticleId.HasValue && _activeViewerArticleId.Value == article.ArticleID)
                return;

            // Different article open — dispose it first
            CloseActiveViewer();

            // Hide list panels
            pnlRightTop.Visible    = false;
            pnlSearch.Visible      = false;
            pnlRightFooter.Visible = false;
            pnlArticleList.Visible = false;

            // Create viewer and embed in panelMainRight
            var viewer = new ArticleViewerForm(_categoryService, _articleService, _user, article);
            viewer.BackRequested += OnViewerBackRequested;
            viewer.TopLevel       = false;
            viewer.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            viewer.Dock           = System.Windows.Forms.DockStyle.Fill;
            panelMainRight.Controls.Add(viewer);
            viewer.Show();

            _activeViewer          = viewer;
            _activeViewerArticleId = article.ArticleID;
        }

        private void OnViewerBackRequested(object sender, EventArgs e)
        {
            CloseActiveViewer();

            // Restore list panels
            pnlRightTop.Visible    = true;
            pnlSearch.Visible      = true;
            pnlRightFooter.Visible = true;
            pnlArticleList.Visible = true;

            // Refresh article list to pick up any edits made inside the viewer
            LoadArticles();
        }

        private void CloseActiveViewer()
        {
            if (_activeViewer == null) return;
            _activeViewer.BackRequested -= OnViewerBackRequested;
            panelMainRight.Controls.Remove(_activeViewer);
            _activeViewer.Dispose();
            _activeViewer          = null;
            _activeViewerArticleId = null;
        }

        private void BtnSearch_Click(object sender, EventArgs e) { LoadArticles(); }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) LoadArticles();
        }

        private void CboFilter_SelectedIndexChanged(object sender, EventArgs e) { LoadArticles(); }

        private void BtnNewArticle_Click(object sender, EventArgs e)
        {
            using (var dlg = new ArticleEditorDialog(_categoryService, _articleService, _user, null))
                if (dlg.ShowDialog(this) == DialogResult.OK) LoadArticles();
        }

        private void BtnNewCategory_Click(object sender, EventArgs e)
        {
            using (var frm = new KBCategoryForm(_categoryService, _user))
                frm.ShowDialog(this);
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
                "ฟังก์ชัน Import Excel อยู่ระหว่างการพัฒนา",
                "Import Excel — MOD-006", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            MessageBox.Show(BuildReportText(), "รายงาน Knowledge Base",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string BuildReportText()
        {
            try
            {
                var all = _articleService.GetAll().ToList();
                return string.Format(
                    "📊 สรุปรายงาน Knowledge Base\n\n" +
                    "บทความทั้งหมด : {0}\n📝 Draft : {1}\n🔍 รอ Review : {2}\n" +
                    "✅ Published : {3}\n📦 Archived : {4}\n\n👁 ยอดชมรวม : {5}\n👍 ยอด Like รวม : {6}",
                    all.Count,
                    all.Count(a => a.Status == 1), all.Count(a => a.Status == 2),
                    all.Count(a => a.Status == 3), all.Count(a => a.Status == 4),
                    all.Sum(a => a.ViewCount), all.Sum(a => a.LikeCount));
            }
            catch { return "ไม่สามารถโหลดรายงานได้"; }
        }
    }
}
