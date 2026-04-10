// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Controllers.KMS.KB;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// หน้าหลัก Knowledge Base System (MOD-006) - KBMain
    /// Layout: SplitContainer — ซ้าย=Category TreeView, ขวา=Article List + Search
    /// รองรับ: Browse by Category, Full-text Search, Add/Edit/Archive Article, Version History
    /// </summary>
    public partial class KBMainForm : Form, IKBMainController
    {
        private KBMainController _presenter;
        private UserContext _user;

        // ── Viewer state ────────────────────────────────────────────────────────
        private ArticleViewerForm _activeViewer          = null;
        private int?              _activeViewerArticleId = null;

        // ── Dialog / child-form factories (injected by composition root) ────────
        /// <summary>Creates and returns a pre-wired ArticleViewerForm for a given article.</summary>
        private Func<KBArticle, ArticleViewerForm> _viewerFactory;
        /// <summary>Shows a pre-wired KBEditorDialog. Returns the dialog result.</summary>
        private Func<KBArticle, DialogResult> _editorFactory;
        /// <summary>Shows the pre-wired KBCategoryForm as a modal dialog.</summary>
        private Action _categorySettingsFactory;
        /// <summary>Creates, wires, and shows a pre-wired KBImportWizardForm. Returns DialogResult.</summary>
        private Func<DialogResult> _importWizardFactory;

        private static readonly byte?[] StatusFilterValues = { null, 1, 2, 3, 4 };

        public KBMainForm()
        {
            InitializeComponent();

            // pnlViewerHost is created here so the designer stays free of a Dock=Fill panel
            // that would block adding other controls. Runtime behaviour is identical.
            pnlViewerHost = new System.Windows.Forms.Panel
            {
                Name      = "pnlViewerHost",
                BackColor = System.Drawing.Color.White,
                Dock      = System.Windows.Forms.DockStyle.Fill,
                Visible   = false,
            };
            Controls.Add(pnlViewerHost);      // added after splitMain → sits behind it by default
            pnlViewerHost.BringToFront();      // move in front of splitMain (pnlHeader stays on top via Dock=Top)
            splitMain.BringToFront();          // restore splitMain as the visible default layer
        }

        /// <summary>
        /// Called by the composition root after the presenter is wired.
        /// Must be called before the form is shown.
        /// </summary>
        public void AttachPresenter(KBMainController presenter, UserContext user)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            if (user == null) throw new ArgumentNullException("user");
            _presenter = presenter;
            _user = user;
        }

        /// <summary>
        /// Inject the factories used to open child forms.
        /// Must be called before the form is shown.
        /// </summary>
        public void AttachFactories(
            Func<KBArticle, ArticleViewerForm> viewerFactory,
            Func<KBArticle, DialogResult>       editorFactory,
            Action                              categorySettingsFactory)
        {
            _viewerFactory           = viewerFactory;
            _editorFactory           = editorFactory;
            _categorySettingsFactory = categorySettingsFactory;
        }

        /// <summary>
        /// Inject the factory used to create and show the KB Import Wizard with its
        /// pre-wired presenter. Must be called before the form is shown.
        /// </summary>
        public void AttachImportWizardFactory(Func<DialogResult> factory)
        {
            _importWizardFactory = factory;
        }

        // ── Form events ──────────────────────────────────────────────────────────

        private void KBMainForm_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\n\nกรุณาเรียก AttachPresenter() ก่อนเปิดฟอร์ม",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            InitFilterCombo();
            _presenter.Initialize();
        }

        private void KBMainForm_Resize(object sender, EventArgs e) { ResizeArticleCards(); }

        // ── IKBMainView ───────────────────────────────────────────────────

        public string SearchKeyword      { get { return txtToolbarSearch.Text.Trim(); } }
        public int    StatusFilterIndex  { get { return cboStatus.SelectedIndex; } }

        public int? SelectedCategoryId  { get; set; }

        public string ArticleTitleText  { set { lblArticleTitle.Text = value; } }
        public string ArticleCountText  { set { lblPageInfo.Text = value; } }

        public void BindCategoryTree(
            IEnumerable<KBCategory>         categories,
            IDictionary<int, int>           articleCounts)
        {
            BuildCategoryTree(categories.ToList(), articleCounts);
        }

        public void RenderArticles(IEnumerable<KBArticle> articles)
        {
            flpArticles.SuspendLayout();
            flpArticles.Controls.Clear();
            foreach (var a in articles)
                flpArticles.Controls.Add(BuildArticleCard(a));
            flpArticles.ResumeLayout();
            ResizeArticleCards();
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowReport(string reportText)
        {
            MessageBox.Show(reportText, "รายงาน Knowledge Base",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Search / filter handlers ─────────────────────────────────────────────

        private void CboFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.LoadArticles();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (_presenter == null) return;
            if (e.KeyCode == Keys.Enter) _presenter.LoadArticles();
        }

        // ── Tree view selection ──────────────────────────────────────────────────

        private void TreeCategories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_presenter == null) return;
            if (e.Node == null) return;
            var cat = e.Node.Tag as KBCategory;
            _presenter.OnCategorySelected(cat?.CategoryID, cat?.CategoryName);
        }

        // ── Article card click ───────────────────────────────────────────────────

        private void OnArticleCardClick(KBArticle article)
        {
            _presenter.OnArticleClicked(article.ArticleID);
            ShowArticleViewer(article);
        }

        // ── Toolbar button handlers ──────────────────────────────────────────────

        private void BtnNewArticle_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (_editorFactory == null) return;
            if (_editorFactory(null) == DialogResult.OK)
                _presenter.LoadArticles();
        }

        private void BtnNewCategory_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _categorySettingsFactory?.Invoke();
            _presenter.LoadCategoryTree();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _categorySettingsFactory?.Invoke();
            _presenter.LoadCategoryTree();
        }

        private void BtnImportExcel_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (_importWizardFactory == null) return;
            if (_importWizardFactory() == DialogResult.OK)
            {
                // Refresh the article list after successful import
                _presenter.LoadArticles();
                MessageBox.Show("นำเข้าข้อมูลเรียบร้อยแล้ว", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.ShowReport();
        }

        // ── Viewer navigation (full-screen over splitMain) ───────────────────────

        private void ShowArticleViewer(KBArticle article)
        {
            if (_activeViewerArticleId.HasValue && _activeViewerArticleId.Value == article.ArticleID)
                return;

            CloseActiveViewer();

            var viewer = _viewerFactory != null ? _viewerFactory(article) : null;
            if (viewer == null) return;

            viewer.BackRequested  += OnViewerBackRequested;
            viewer.TopLevel        = false;
            viewer.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            viewer.Dock            = System.Windows.Forms.DockStyle.Fill;

            // Hide the split layout and show the full-screen host in its place
            splitMain.Visible      = false;
            pnlViewerHost.Controls.Add(viewer);
            pnlViewerHost.Visible  = true;
            pnlViewerHost.BringToFront();
            viewer.Show();

            _activeViewer          = viewer;
            _activeViewerArticleId = article.ArticleID;
        }

        private void OnViewerBackRequested(object sender, EventArgs e)
        {
            CloseActiveViewer();

            // Restore the normal split layout
            pnlViewerHost.Visible = false;
            splitMain.Visible     = true;

            if (_presenter != null)
                _presenter.LoadArticles();
        }

        private void CloseActiveViewer()
        {
            if (_activeViewer == null) return;
            _activeViewer.BackRequested -= OnViewerBackRequested;
            pnlViewerHost.Controls.Remove(_activeViewer);
            _activeViewer.Dispose();
            _activeViewer          = null;
            _activeViewerArticleId = null;
        }

        // ── Private UI helpers ────────────────────────────────────────────────────

        private void InitFilterCombo()
        {
            cboStatus.Items.Clear();
            cboStatus.Items.Add("ทุกสถานะ");
            cboStatus.Items.Add("📝 Draft");
            cboStatus.Items.Add("🔍 รอ Review");
            cboStatus.Items.Add("✅ Published");
            cboStatus.Items.Add("📦 Archived");
            cboStatus.SelectedIndex = 0;

            cboType.SelectedIndex = 0;
            cboDepartment.SelectedIndex = 0;
            cboSortBy.SelectedIndex = 0;
            cboPageSize.SelectedIndex = 0;
        }

        private void BuildCategoryTree(
            List<KBCategory>      categories,
            IDictionary<int, int> articleCounts)
        {
            treeCategories.BeginUpdate();
            treeCategories.Nodes.Clear();

            int totalCount = articleCounts.Values.Sum();
            string rootLabel = totalCount > 0
                ? string.Format("📚 ทั้งหมด ({0})", totalCount)
                : "📚 ทั้งหมด";

            var rootNode = new TreeNode(rootLabel) { Tag = null };
            rootNode.ForeColor = Color.FromArgb(0, 51, 102);
            rootNode.NodeFont  = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            treeCategories.Nodes.Add(rootNode);

            var roots = categories
                .Where(c => c.IsActive && !c.ParentCategoryID.HasValue)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName);
            foreach (var cat in roots)
                rootNode.Nodes.Add(BuildCategoryNode(cat, categories, articleCounts));

            treeCategories.EndUpdate();
            treeCategories.ExpandAll();
            treeCategories.SelectedNode = rootNode;
        }

        private TreeNode BuildCategoryNode(
            KBCategory            cat,
            List<KBCategory>      allCategories,
            IDictionary<int, int> articleCounts)
        {
            int    count = articleCounts.ContainsKey(cat.CategoryID) ? articleCounts[cat.CategoryID] : 0;
            string label = count > 0
                ? string.Format("📁 {0} ({1})", cat.CategoryName, count)
                : string.Format("📁 {0}", cat.CategoryName);

            var node = new TreeNode(label)
            {
                Tag       = cat,
                ForeColor = Color.FromArgb(30, 30, 30),
            };

            var children = allCategories
                .Where(c => c.IsActive && c.ParentCategoryID == cat.CategoryID)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName);
            foreach (var child in children)
                node.Nodes.Add(BuildCategoryNode(child, allCategories, articleCounts));

            return node;
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
                Height      = 90,
            };
            card.Click += (s, e) => OnArticleCardClick(article);

            var stripe = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = GetStatusColor(article.Status) };
            stripe.Click += (s, e) => OnArticleCardClick(article);
            card.Controls.Add(stripe);

            var pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 8, 10, 6) };
            pnlContent.Click += (s, e) => OnArticleCardClick(article);

            var lblCardTitle = new Label
            {
                AutoSize  = false, Dock = DockStyle.Top, Height = 22,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 51, 102),
                Text      = article.IsOfficialPolicy ? "🏛 " + article.Title : article.Title ?? "(ไม่มีชื่อ)",
                TextAlign = ContentAlignment.MiddleLeft,
            };
            lblCardTitle.Click += (s, e) => OnArticleCardClick(article);

            var lblMeta = new Label
            {
                AutoSize  = false, Dock = DockStyle.Top, Height = 18,
                Font      = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 110, 130),
                Text      = string.Format("{0}  |  {1}  |  Version {2}  |  {3}",
                    article.ArticleTypeName, article.StatusName,
                    article.CurrentVersion, article.CategoryName ?? "-"),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            lblMeta.Click += (s, e) => OnArticleCardClick(article);

            var lblSummary = new Label
            {
                AutoSize  = false, Dock = DockStyle.Top, Height = 20,
                Font      = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(60, 60, 60),
                Text      = article.Summary ?? string.Empty,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            lblSummary.Click += (s, e) => OnArticleCardClick(article);

            var lblStats = new Label
            {
                AutoSize  = false, Dock = DockStyle.Top, Height = 18,
                Font      = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(130, 140, 155),
                Text      = string.Format("👁 {0}   👍 {1}   🏷 {2}",
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
    }
}
