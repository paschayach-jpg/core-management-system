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
using CMS.Services;
using CMS.Presentation.Services;

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

        /// <summary>
        /// Navigation history stack — each entry is a viewer that was pushed aside
        /// when the user clicked a related article. Popped on btnBack.
        /// </summary>
        private readonly Stack<ArticleViewerForm> _viewerHistory = new Stack<ArticleViewerForm>();

        private int _currentPage = 1;

        // ── Category settings embedded panel state ───────────────────────────────
        private KBCategoryForm _activeCategoryForm = null;

        // ── Dialog / child-form factories (injected by composition root) ────────
        /// <summary>Creates and returns a pre-wired ArticleViewerForm for a given article.</summary>
        private Func<KBArticle, ArticleViewerForm> _viewerFactory;
        /// <summary>Shows a pre-wired KBEditorDialog. Returns the dialog result.</summary>
        private Func<KBArticle, DialogResult> _editorFactory;
        /// <summary>Creates and returns a pre-wired KBCategoryForm ready to be embedded.</summary>
        private Func<KBCategoryForm> _categoryFormFactory;
        /// <summary>Creates, wires, and shows a pre-wired KBImportWizardForm. Returns DialogResult.</summary>
        private Func<DialogResult> _importWizardFactory;
        /// <summary>Creates, wires, and shows a pre-wired KBDashboardForm. Returns DialogResult.</summary>
        private Func<DialogResult> _dashboardFactory;

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
            Controls.Add(pnlViewerHost);
            pnlViewerHost.BringToFront();

            // pnlCategoryHost hosts the embedded KBCategoryForm (same pattern as pnlViewerHost)
            pnlCategoryHost = new System.Windows.Forms.Panel
            {
                Name      = "pnlCategoryHost",
                BackColor = System.Drawing.Color.White,
                Dock      = System.Windows.Forms.DockStyle.Fill,
                Visible   = false,
            };
            Controls.Add(pnlCategoryHost);
            pnlCategoryHost.BringToFront();

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
            Func<KBCategoryForm>                categoryFormFactory)
        {
            _viewerFactory       = viewerFactory;
            _editorFactory       = editorFactory;
            _categoryFormFactory = categoryFormFactory;
        }

        /// <summary>
        /// Inject the factory used to create and show the KB Import Wizard with its
        /// pre-wired presenter. Must be called before the form is shown.
        /// </summary>
        public void AttachImportWizardFactory(Func<DialogResult> factory)
        {
            _importWizardFactory = factory;
        }

        /// <summary>
        /// Inject the factory used to create and show the KB Dashboard with its
        /// pre-wired presenter. Must be called before the form is shown.
        /// </summary>
        public void AttachDashboardFactory(Func<DialogResult> factory)
        {
            _dashboardFactory = factory;
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
            // Ensure ArticleCardControl widths are properly calculated after form layout is complete
            BeginInvoke(new Action(ResizeArticleCards));
        }

        private void KBMainForm_Resize(object sender, EventArgs e) { ResizeArticleCards(); }

        // ── IKBMainView ───────────────────────────────────────────────────

        public string SearchKeyword      { get { return txtToolbarSearch.Text.Trim(); } }
        public int    StatusFilterIndex  { get { return cboStatus.SelectedIndex; } }

        public int PageSize
        {
            get
            {
                switch (cboPageSize.SelectedIndex)
                {
                    case 1:  return 50;
                    case 2:  return 100;
                    default: return 20;
                }
            }
        }

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
            {
                var card = new ArticleCardControl();
                card.Article           = a;
                card.CardClicked      += (s, ev) => OnArticleCardClick(ev);
                card.ReadClicked      += (s, ev) => OnArticleCardClick(ev);
                card.EditClicked      += (s, ev) => OnEditClick(ev);
                card.ShareClicked     += (s, ev) => OnShareClick(ev);
                card.SendReviewClicked += (s, ev) => OnSendReviewClick(ev);
                card.LikeClicked      += (s, ev) => _presenter.LikeArticle(ev.ArticleID);
                flpArticles.Controls.Add(card);
            }
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

        public void RenderPagination(int totalCount, int currentPage, int pageSize)
        {
            _currentPage = currentPage;
            int totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling((double)totalCount / pageSize);

            int from = totalCount == 0 ? 0 : (currentPage - 1) * pageSize + 1;
            int to   = Math.Min(currentPage * pageSize, totalCount);
            lblPageInfo.Text = totalCount == 0
                ? "ไม่มีรายการ"
                : string.Format("แสดง {0}-{1} จาก {2} รายการ", from, to, totalCount);

            btnPrevPage.Enabled = currentPage > 1;
            btnNextPage.Enabled = currentPage < totalPages;

            int startPage = Math.Max(1, currentPage - 1);
            if (startPage + 2 > totalPages) startPage = Math.Max(1, totalPages - 2);

            var pageBtns = new Button[] { btnPage1, btnPage2, btnPage3 };
            for (int i = 0; i < pageBtns.Length; i++)
            {
                int pageNum = startPage + i;
                if (pageNum <= totalPages)
                {
                    pageBtns[i].Visible   = true;
                    pageBtns[i].Text      = pageNum.ToString();
                    pageBtns[i].Tag       = pageNum;
                    bool isCurrent        = pageNum == currentPage;
                    pageBtns[i].BackColor = isCurrent
                        ? Color.FromArgb(46, 116, 181)
                        : Color.White;
                    pageBtns[i].ForeColor = isCurrent
                        ? Color.White
                        : Color.FromArgb(26, 35, 50);
                }
                else
                {
                    pageBtns[i].Visible = false;
                }
            }

            pnlPaginationButtons.Visible = totalPages > 1;
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
            ShowCategorySettings();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            ShowCategorySettings();
        }

        private void BtnDashboard_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (_dashboardFactory == null) return;
            _dashboardFactory();
        }

        private void BtnImportExcel_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (_importWizardFactory == null) return;
            
            try
            {
                DialogResult result = _importWizardFactory();
                switch (result)
                {
                    case DialogResult.OK :
                    case DialogResult.Cancel :
                        _presenter.LoadCategoryTree();
                        _presenter.LoadArticles();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "เกิดข้อผิดพลาดในการนำเข้าข้อมูล:\n" + ex.Message,
                    "ข้อผิดพลาด",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.ShowReport();
        }

        private async void BtnTestNotification_Click(object sender, EventArgs e)
        {
            try
            {
                if (!NotificationManager.Instance.IsConnected)
                {
                    MessageBox.Show(
                        "Notification system is not connected.\nPlease check your connection.",
                        "Notification Test",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Get current user AD name
                string currentUserAd = _user != null ? _user.ADUserName : "unknown";
                
                // Send test notification to yourself
                var result = await NotificationManager.Instance.SendNotificationAsync(
                    typeCode: "ASSIGN",
                    title: "Test Notification from KMS",
                    message: "This is a test notification from Knowledge Management System. Click to view details.",
                    recipientUserAd: currentUserAd,
                    senderUserAd: currentUserAd,
                    referenceType: "kb_article",
                    referenceId: "TEST001",
                    referenceCode: "KB-TEST-001",
                    priority: 2
                );

                if (result != null)
                {
                    MessageBox.Show(
                        "Test notification sent successfully!\n\n" +
                        "Notification ID: " + result.NotificationId + "\n" +
                        "Recipient: " + currentUserAd + "\n\n" +
                        "You should see a popup notification shortly.",
                        "Notification Test",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Failed to send test notification.\nPlease check the logs for details.",
                        "Notification Test",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error sending test notification:\n\n" + ex.Message,
                    "Notification Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ── Category settings navigation (embedded over splitMain) ──────────────

        private void ShowCategorySettings()
        {
            CloseActiveCategoryForm();

            var catForm = _categoryFormFactory != null ? _categoryFormFactory() : null;
            if (catForm == null) return;

            catForm.BackRequested  += OnCategoryBackRequested;
            catForm.TopLevel        = false;
            catForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            catForm.Dock            = System.Windows.Forms.DockStyle.Fill;

            splitMain.Visible = false;
            pnlCategoryHost.Controls.Add(catForm);
            pnlCategoryHost.Visible = true;
            pnlCategoryHost.BringToFront();
            catForm.Show();

            _activeCategoryForm = catForm;
        }

        private void OnCategoryBackRequested(object sender, EventArgs e)
        {
            CloseActiveCategoryForm();

            pnlCategoryHost.Visible = false;
            splitMain.Visible       = true;

            if (_presenter != null)
                _presenter.LoadCategoryTree();
        }

        private void CloseActiveCategoryForm()
        {
            if (_activeCategoryForm == null) return;
            _activeCategoryForm.BackRequested -= OnCategoryBackRequested;
            pnlCategoryHost.Controls.Remove(_activeCategoryForm);
            _activeCategoryForm.Dispose();
            _activeCategoryForm = null;
        }

        // ── Viewer navigation (full-screen over splitMain) ───────────────────────

        private void ShowArticleViewer(KBArticle article)
        {
            // If the same article is already the active viewer, do nothing
            if (_activeViewerArticleId.HasValue && _activeViewerArticleId.Value == article.ArticleID)
                return;

            // Push the current viewer onto the history stack instead of disposing it,
            // so btnBack can return to it later
            if (_activeViewer != null)
            {
                _activeViewer.Hide();
                pnlViewerHost.Controls.Remove(_activeViewer);
                _viewerHistory.Push(_activeViewer);
                _activeViewer          = null;
                _activeViewerArticleId = null;
            }

            var viewer = _viewerFactory != null ? _viewerFactory(article) : null;
            if (viewer == null) return;

            viewer.BackRequested        += OnViewerBackRequested;
            viewer.ArticleOpenRequested += (s, a) => ShowArticleViewer(a);
            viewer.TopLevel        = false;
            viewer.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            viewer.Dock            = System.Windows.Forms.DockStyle.Fill;

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
            // Dispose the current viewer
            if (_activeViewer != null)
            {
                _activeViewer.BackRequested -= OnViewerBackRequested;
                pnlViewerHost.Controls.Remove(_activeViewer);
                _activeViewer.Dispose();
                _activeViewer          = null;
                _activeViewerArticleId = null;
            }

            // If there is a previous viewer on the stack, restore it
            if (_viewerHistory.Count > 0)
            {
                var previous = _viewerHistory.Pop();
                pnlViewerHost.Controls.Add(previous);
                previous.Show();

                _activeViewer          = previous;
                _activeViewerArticleId = previous.CurrentArticleId;
                return;
            }

            // Stack is empty — return to the article list
            pnlViewerHost.Visible = false;
            splitMain.Visible     = true;

            if (_presenter != null)
                _presenter.LoadArticles();

            // Force layout update and resize article cards
            splitMain.PerformLayout();
            flpArticles.PerformLayout();
            BeginInvoke(new Action(ResizeArticleCards));
        }

        private void CloseActiveViewer()
        {
            // Dispose the active viewer
            if (_activeViewer != null)
            {
                _activeViewer.BackRequested -= OnViewerBackRequested;
                pnlViewerHost.Controls.Remove(_activeViewer);
                _activeViewer.Dispose();
                _activeViewer          = null;
                _activeViewerArticleId = null;
            }

            // Dispose every viewer in the history stack
            while (_viewerHistory.Count > 0)
            {
                var old = _viewerHistory.Pop();
                old.Dispose();
            }
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

            cboType.SelectedIndex       = 0;
            cboDepartment.SelectedIndex = 0;
            cboSortBy.SelectedIndex     = 0;
            cboPageSize.SelectedIndex   = 0;

            // Wire up after initial values are set to avoid spurious loads
            btnPrevPage.Click  += BtnPrevPage_Click;
            btnNextPage.Click  += BtnNextPage_Click;
            btnPage1.Click     += BtnPageNumber_Click;
            btnPage2.Click     += BtnPageNumber_Click;
            btnPage3.Click     += BtnPageNumber_Click;
            cboPageSize.SelectedIndexChanged += CboPageSize_SelectedIndexChanged;
            btnClearFilter.Click += BtnClearFilter_Click;
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

        private void OnEditClick(KBArticle article)
        {
            if (_editorFactory == null) return;
            if (_editorFactory(article) == DialogResult.OK)
                _presenter.LoadArticles();
        }

        private void OnShareClick(KBArticle article)
        {
            MessageBox.Show(
                string.Format("ฟีเจอร์แชร์บทความ \"{0}\"\n\nจะพัฒนาในอนาคต", article.Title),
                "Share Article",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OnSendReviewClick(KBArticle article)
        {
            var confirm = MessageBox.Show(
                string.Format("ต้องการส่งบทความ \"{0}\" ขอ Review ใช่หรือไม่?", article.Title),
                "ยืนยันส่งขอ Review",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            if (_presenter != null)
            {
                _presenter.SendArticleForReview(article.ArticleID);
                _presenter.LoadArticles();
                MessageBox.Show(
                    "ส่งขอ Review เรียบร้อยแล้ว",
                    "สำเร็จ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void BtnPrevPage_Click(object sender, EventArgs e)
        {
            if (_presenter == null || _currentPage <= 1) return;
            _presenter.GoToPage(_currentPage - 1);
        }

        private void BtnNextPage_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.GoToPage(_currentPage + 1);
        }

        private void BtnPageNumber_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            var btn = sender as Button;
            if (btn != null && btn.Tag != null)
                _presenter.GoToPage((int)btn.Tag);
        }

        private void CboPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.LoadArticles();
        }

        private void BtnClearFilter_Click(object sender, EventArgs e)
        {
            cboStatus.SelectedIndex     = 0;
            cboType.SelectedIndex       = 0;
            cboDepartment.SelectedIndex = 0;
            cboSortBy.SelectedIndex     = 0;
            txtToolbarSearch.Text       = string.Empty;
            if (_presenter != null) _presenter.LoadArticles();
        }

        private void ResizeArticleCards()
        {
            int cardWidth = flpArticles.ClientSize.Width - 16;
            if (cardWidth < 200) return;
            foreach (Control c in flpArticles.Controls)
                if (c is ArticleCardControl) c.Width = cardWidth;
        }
    }
}
