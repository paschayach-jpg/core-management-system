// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using CMS.Controllers.KMS.KB;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Domain.Models;
using CMS.Presentation.Services;
using CMS.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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

        // ── Article cache for grid / card dual-render ────────────────────────────
        private List<KBArticle> _cachedArticles    = new List<KBArticle>();
        private bool            _tab2NeedsRefresh  = true;

        // ── Suppress AfterSelect during programmatic tree population ─────────────
        private bool _suppressCategorySelect = false;

        // ── Category settings embedded panel state ───────────────────────────────
        private KBCategoryForm _activeCategoryForm = null;

        // ── Editor embedded panel state ───────────────────────────────────────────
        private KBEditorDialog _activeEditorForm = null;

        // ── Import wizard embedded panel state ───────────────────────────────────
        private KBImportWizardForm _activeImportForm = null;

        // ── Loading overlay ────────────────────────────────────────────────────────
        private CMS.Presentation.UserControls.LoadingOverlayControl _loadingOverlay;

        // ── Dialog / child-form factories (injected by composition root) ────────
        /// <summary>Creates and returns a pre-wired ArticleViewerForm for a given article.</summary>
        private Func<KBArticle, ArticleViewerForm> _viewerFactory;
        /// <summary>Creates and returns a pre-wired KBEditorDialog ready to be embedded.</summary>
        private Func<KBArticle, KBEditorDialog> _editorFactory;
        /// <summary>Creates and returns a pre-wired KBCategoryForm ready to be embedded.</summary>
        private Func<KBCategoryForm> _categoryFormFactory;
        /// <summary>Creates and returns a pre-wired KBImportWizardForm ready to be embedded.</summary>
        private Func<KBImportWizardForm> _importWizardFactory;
        /// <summary>Creates, wires, and shows a pre-wired KBDashboardForm. Returns DialogResult.</summary>
        private Func<DialogResult> _dashboardFactory;


        public KBMainForm()
        {
            InitializeComponent();
            InitializeArticleGrid();

            // pnlViewerHost is created here so the designer stays free of a Dock=Fill panel
            // that would block adding other controls. Runtime behaviour is identical.
            // ทำหน้าที่ เป็น Panel ที่ทำหน้าที่เป็น "full-screen layer" สำหรับแสดง ArticleViewerForm (หน้าอ่านบทความ) ทับบน splitMain
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

            // pnlEditorHost hosts the embedded KBEditorDialog (same pattern as pnlCategoryHost)
            pnlEditorHost = new System.Windows.Forms.Panel
            {
                Name      = "pnlEditorHost",
                BackColor = System.Drawing.Color.White,
                Dock      = System.Windows.Forms.DockStyle.Fill,
                Visible   = false,
            };
            Controls.Add(pnlEditorHost);
            pnlEditorHost.BringToFront();

            // pnlImportHost hosts the embedded KBImportWizardForm (same pattern as pnlCategoryHost)
            pnlImportHost = new System.Windows.Forms.Panel
            {
                Name      = "pnlImportHost",
                BackColor = System.Drawing.Color.White,
                Dock      = System.Windows.Forms.DockStyle.Fill,
                Visible   = false,
            };
            Controls.Add(pnlImportHost);
            pnlImportHost.BringToFront();

            splitMain.BringToFront();          // restore splitMain as the visible default layer

            // LoadingOverlayControl — sits in splitMain.Panel2 over panelMainRight.
            // Dock=None keeps it from disturbing panelMainRight's Dock=Fill layout.
            _loadingOverlay = new CMS.Presentation.UserControls.LoadingOverlayControl
            {
                Dock = System.Windows.Forms.DockStyle.None,
            };
            splitMain.Panel2.Controls.Add(_loadingOverlay);

            // Keep overlay sized to match the right panel whenever it resizes
            splitMain.Panel2.SizeChanged += (s, ev) =>
            {
                if (_loadingOverlay.Visible)
                    _loadingOverlay.Bounds = splitMain.Panel2.ClientRectangle;
            };
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
            Func<KBArticle, KBEditorDialog>    editorFactory,
            Func<KBCategoryForm>               categoryFormFactory)
        {
            _viewerFactory       = viewerFactory;
            _editorFactory       = editorFactory;
            _categoryFormFactory = categoryFormFactory;
        }

        /// <summary>
        /// Inject the factory used to create and show the KB Import Wizard with its
        /// pre-wired presenter. Must be called before the form is shown.
        /// </summary>
        public void AttachImportWizardFactory(Func<KBImportWizardForm> factory)
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
        }

        private void KBMainForm_Resize(object sender, EventArgs e) { ResizeArticleCards(); }

        // ── IKBMainView ───────────────────────────────────────────────────

        public string SearchKeyword      { get { return txtToolbarSearch.Text.Trim(); } }
        public byte?  TypeFilter         { get { return cboType.SelectedValue as byte?; } }
        public byte?  StatusFilter       { get { return cboStatus.SelectedValue as byte?; } }
        public int    SearchFieldId      { get { return cboSearchField.SelectedIndex >= 0 ? cboSearchField.SelectedIndex + 1 : 1; } }

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

        public int?   SelectedCategoryId   { get; set; }
        public string SelectedArticleTitle { get; set; }

        public void ShowLoading()
        {
            _loadingOverlay.Bounds = splitMain.Panel2.ClientRectangle;
            Cursor = Cursors.WaitCursor;
            _loadingOverlay.ShowOverlay();
        }

        public void HideLoading()
        {
            _loadingOverlay.HideOverlay();
            Cursor = Cursors.Default;
        }

        public string ArticleTitleText  { set { lblArticleTitle.Text = value; } }
        public string ArticleCountText  { set { lblPageInfo.Text = value; } }

        public void BindCategoryTree(
            IEnumerable<KBCategory>                    categories,
            IDictionary<int, int>                      articleCounts,
            IDictionary<int, IDictionary<string, int>> titlesByCategory)
        {
            BuildCategoryTree(categories.ToList(), articleCounts, titlesByCategory);
        }

        /// <summary>Tag object placed on title leaf-nodes in treeCategories.</summary>
        private sealed class TitleNode
        {
            public int    CategoryId   { get; set; }
            public string CategoryName { get; set; }
            public string Title        { get; set; }
        }

        public void RenderArticles(IEnumerable<KBArticle> articles)
        {
            _cachedArticles = articles.ToList();

            // Tab1 (grid) always renders immediately
            PopulateGrid(_cachedArticles);

            // Tab2 (cards) renders only when it is the active tab — otherwise mark dirty
            if (tabArticles.SelectedTab == tabPage2)
                PopulateCards(_cachedArticles);
            else
                _tab2NeedsRefresh = true;
        }

        private void PopulateGrid(List<KBArticle> articles)
        {
            dgvArticles.SuspendLayout();
            dgvArticles.Rows.Clear();

            for (int i = 0; i < articles.Count; i++)
            {
                var a = articles[i];
                dgvArticles.Rows.Add(
                    0,
                    //a.ArticleCode,
                    a.CategoryName,
                    a.Title,
                    a.Summary,
                    a.ContentHTML.Length >= 200 ? $"{a.ContentHTML.Substring(0, 200)}...read more" : a.ContentHTML,
                    a.PublishedDate,
                    a.ExpiryDate,
                    a.LinkedEntities,
                    a.Tags,
                    //a.ArticleTypeName,
                    //a.StatusName,
                    a.CurrentVersion > 0 ? "v" + a.CurrentVersion : "-",
                    a.ViewCount,
                    
                    "👁️ Read",
                    "✏️ Edit",
                    "🔗 Share",
                    a.Status == 1 ? "📤 Review" : ""
                );
            }
            //dgvArticles.Sort(dgvArticles.Columns["colCode"], ListSortDirection.Ascending);
            dgvArticles.ResumeLayout();
        }

        private void PopulateCards(List<KBArticle> articles)
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
            _tab2NeedsRefresh = false;
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

        // ── Tab selection ────────────────────────────────────────────────────────

        private void TabArticles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabArticles.SelectedTab == tabPage2 && _tab2NeedsRefresh)
                PopulateCards(_cachedArticles);
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
            if (_presenter == null || e.Node == null || _suppressCategorySelect) return;

            // Title leaf-node → filter grid to that specific title
            var tn = e.Node.Tag as TitleNode;
            if (tn != null)
            {
                _presenter.OnTitleSelected(tn.CategoryId, tn.CategoryName, tn.Title);
                return;
            }

            // Category node → existing behaviour
            var cat = e.Node.Tag as KBCategory;
            if (cat == null) return;  // root node — no specific category selected
            _presenter.OnCategorySelected(cat.CategoryID, cat.CategoryName);
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
            ShowEditorEmbedded(null);
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
            ShowImportWizardEmbedded();
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

        // ── Editor embedded navigation (full-screen over splitMain) ──────────────

        private void ShowEditorEmbedded(KBArticle article)
        {
            CloseActiveEditorForm();

            var editorForm = _editorFactory != null ? _editorFactory(article) : null;
            if (editorForm == null) return;

            editorForm.BackRequested  += OnEditorBackRequested;
            editorForm.TopLevel        = false;
            editorForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            editorForm.Dock            = System.Windows.Forms.DockStyle.Fill;

            splitMain.Visible = false;
            pnlEditorHost.Controls.Add(editorForm);
            pnlEditorHost.Visible = true;
            pnlEditorHost.BringToFront();
            editorForm.Show();

            _activeEditorForm = editorForm;
        }

        private void OnEditorBackRequested(object sender, EventArgs e)
        {
            CloseActiveEditorForm();

            pnlEditorHost.Visible = false;
            splitMain.Visible     = true;

            if (_presenter != null)
            {
                _presenter.LoadCategoryTree();
                _presenter.LoadArticles();
            }
        }

        private void CloseActiveEditorForm()
        {
            if (_activeEditorForm == null) return;
            _activeEditorForm.BackRequested -= OnEditorBackRequested;
            pnlEditorHost.Controls.Remove(_activeEditorForm);
            _activeEditorForm.Dispose();
            _activeEditorForm = null;
        }

        // ── Import wizard embedded navigation (full-screen over splitMain) ────────

        private void ShowImportWizardEmbedded()
        {
            CloseActiveImportForm();

            var importForm = _importWizardFactory != null ? _importWizardFactory() : null;
            if (importForm == null) return;

            importForm.BackRequested  += OnImportBackRequested;
            importForm.TopLevel        = false;
            importForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            importForm.Dock            = System.Windows.Forms.DockStyle.Fill;

            splitMain.Visible = false;
            pnlImportHost.Controls.Add(importForm);
            pnlImportHost.Visible = true;
            pnlImportHost.BringToFront();
            importForm.Show();

            _activeImportForm = importForm;
        }

        private void OnImportBackRequested(object sender, EventArgs e)
        {
            CloseActiveImportForm();

            pnlImportHost.Visible = false;
            splitMain.Visible     = true;

            if (_presenter != null)
            {
                _presenter.LoadCategoryTree();
                _presenter.LoadArticles();
            }
        }

        private void CloseActiveImportForm()
        {
            if (_activeImportForm == null) return;
            _activeImportForm.BackRequested -= OnImportBackRequested;
            pnlImportHost.Controls.Remove(_activeImportForm);
            _activeImportForm.Dispose();
            _activeImportForm = null;
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
            // cboType — code 5 = Wiki Page
            cboType.DataSource = new[]
            {
                new { Code = (byte?)null, Description = "ทุกประเภท" },
                new { Code = (byte?)5,    Description = "Wiki Page" },
            };
            cboType.DisplayMember = "Description";
            cboType.ValueMember   = "Code";
            cboType.SelectedIndex = 0;

            // cboStatus — search by code (1=Draft, 2=Review, 3=Published, 4=Archived)
            cboStatus.DataSource = new[]
            {
                new { Code = (byte?)null, Description = "ทุกสถานะ"  },
                new { Code = (byte?)1,    Description = "Draft"     },
                new { Code = (byte?)2,    Description = "Review"    },
                new { Code = (byte?)3,    Description = "Published" },
                new { Code = (byte?)4,    Description = "Archived"  },
            };
            cboStatus.DisplayMember = "Description";
            cboStatus.ValueMember   = "Code";
            cboStatus.SelectedIndex = 0;

            cboType.SelectedIndex       = 0;
            cboDepartment.SelectedIndex = 0;
            cboSortBy.SelectedIndex     = 0;
            cboPageSize.SelectedIndex   = 0;

            cboSearchField.DataSource = new[]
            {
                new { Id = 1, Description = "ชื่อบทความ"    },
                new { Id = 2, Description = "เนื้อหาบทความ" },
                new { Id = 3, Description = "Tag"           },
                new { Id = 4, Description = "Summary"       },
            };
            cboSearchField.DisplayMember = "Description";
            cboSearchField.ValueMember   = "Id";
            cboSearchField.SelectedIndex = 0;

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
            List<KBCategory>                           categories,
            IDictionary<int, int>                      articleCounts,
            IDictionary<int, IDictionary<string, int>> titlesByCategory)
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
                rootNode.Nodes.Add(BuildCategoryNode(cat, categories, articleCounts, titlesByCategory));

            treeCategories.EndUpdate();
            rootNode.Expand();

            _suppressCategorySelect  = true;
            treeCategories.SelectedNode = rootNode;
            _suppressCategorySelect  = false;
        }

        private TreeNode BuildCategoryNode(
            KBCategory                                 cat,
            List<KBCategory>                           allCategories,
            IDictionary<int, int>                      articleCounts,
            IDictionary<int, IDictionary<string, int>> titlesByCategory)
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

            // ── Child categories (existing structure) ────────────────────────────
            var children = allCategories
                .Where(c => c.IsActive && c.ParentCategoryID == cat.CategoryID)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName);
            foreach (var child in children)
                node.Nodes.Add(BuildCategoryNode(child, allCategories, articleCounts, titlesByCategory));

            // ── Title leaf-nodes: "📄 Title (N)" ────────────────────────────────
            IDictionary<string, int> titleMap;
            if (titlesByCategory != null && titlesByCategory.TryGetValue(cat.CategoryID, out titleMap))
            {
                foreach (var kvp in titleMap.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
                {
                    string nodeLabel = string.Format("📄 {0} ({1})", kvp.Key, kvp.Value);
                    var titleNode = new TreeNode(nodeLabel)
                    {
                        Tag       = new TitleNode
                                    {
                                        CategoryId   = cat.CategoryID,
                                        CategoryName = cat.CategoryName,
                                        Title        = kvp.Key,   // raw title, no count
                                    },
                        ForeColor = Color.FromArgb(50, 80, 140),
                    };
                    node.Nodes.Add(titleNode);
                }
            }

            return node;
        }

        private void OnEditClick(KBArticle article)
        {
            ShowEditorEmbedded(article);
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
            SelectedArticleTitle        = null;  // also clear any active title-subcategory filter
            if (_presenter != null) _presenter.LoadArticles();
        }

        private void ResizeArticleCards()
        {
            int cardWidth = flpArticles.ClientSize.Width - 16;
            if (cardWidth < 200) return;
            foreach (Control c in flpArticles.Controls)
                if (c is ArticleCardControl) c.Width = cardWidth;
        }

        // ── DataGridView setup ────────────────────────────────────────────────────

        private void InitializeArticleGrid()
        {
            dgvArticles.AutoGenerateColumns = false;
            dgvArticles.EnableHeadersVisualStyles = false;
            dgvArticles.ColumnHeadersHeight = 32;
            dgvArticles.RowTemplate.Height = 36;
            dgvArticles.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 228, 240);
            dgvArticles.DefaultCellStyle.SelectionForeColor = Color.FromArgb(26, 35, 50);

            var hdrStyle = dgvArticles.ColumnHeadersDefaultCellStyle;
            hdrStyle.BackColor        = Color.FromArgb(240, 244, 249);
            hdrStyle.ForeColor        = Color.FromArgb(30, 58, 95);
            //hdrStyle.Font             = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            hdrStyle.SelectionBackColor = Color.FromArgb(240, 244, 249);
            hdrStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dgvArticles.DefaultCellStyle.Font                = new Font("Segoe UI", 8.5f);            

            var ctr = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter };
            var lft = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft,
                                                   Padding  = new Padding(4, 0, 0, 0) };

            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn {
                Name = "colRowNum", 
                HeaderText = "#", 
                Width = 25, 
                ReadOnly = true,
                DefaultCellStyle = ctr,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            //dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            //{
            //    Name = "colCode",
            //    HeaderText = "รหัส",
            //    //Width = 0,
            //    ReadOnly = true,
            //    Visible = false
            //});
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCategory",
                HeaderText = "หมวดหมู่",
                ReadOnly = true,
                DefaultCellStyle = lft,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn {
                Name = "colTitle", 
                HeaderText = "ชื่อบทความ", 
                ReadOnly = true,
                DefaultCellStyle = lft,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCode",
                HeaderText = "summary",
                Width = 105,
                ReadOnly = true,
                DefaultCellStyle = lft,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "colContentHtml",
                HeaderText = "รายละเอียด",
                ReadOnly = true,
                Width = 600,
                DefaultCellStyle = new DataGridViewCellStyle{
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(4, 0, 0, 0),
                    WrapMode = DataGridViewTriState.True
                }
            });
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colPublishedDate",
                HeaderText = "วันที่เผยแพร่",
                ReadOnly = true,
                DefaultCellStyle = { Format = "dd/MM/yyyy" },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colExpiryDate",
                HeaderText = "วันที่ยกเลิก",
                ReadOnly = true,
                DefaultCellStyle = { Format = "dd/MM/yyyy" },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colOrgDivDept",
                HeaderText = "Org./Div./Dept.",
                ReadOnly = true,
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(4, 0, 0, 0),
                    WrapMode = DataGridViewTriState.True,

                }
            });
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTags",
                HeaderText = "Tags",
                ReadOnly = true,
                DefaultCellStyle = lft,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            });
            //dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            //{
            //    Name = "colType",
            //    HeaderText = "ประเภท",
            //    Width = 78,
            //    ReadOnly = true,
            //    DefaultCellStyle = ctr
            //});
            //dgvArticles.Columns.Add(new DataGridViewTextBoxColumn {
            //    Name = "colStatus", HeaderText = "สถานะ", Width = 90, ReadOnly = true,
            //    DefaultCellStyle = ctr });

            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn {
                Name = "colVersion", 
                HeaderText = "Ver.", 
                Width = 50, 
                ReadOnly = true,
                DefaultCellStyle = ctr,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            });
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn {
                Name = "colViews",
                HeaderText = "👁",
                Width = 48,
                ReadOnly = true,
                DefaultCellStyle = ctr,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            });
            dgvArticles.Columns.Add(new DataGridViewButtonColumn {
                Name = "colBtnRead", 
                HeaderText = "", 
                Width = 74, 
                FlatStyle = FlatStyle.Flat,
                UseColumnTextForButtonValue = false,
                DefaultCellStyle = new DataGridViewCellStyle {
                    BackColor = Color.FromArgb(46, 116, 181), 
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8f),
                    Alignment = DataGridViewContentAlignment.MiddleCenter 
                },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new DataGridViewButtonColumn {
                Name = "colBtnEdit", 
                HeaderText = "", 
                Width = 74, 
                FlatStyle = FlatStyle.Flat,
                UseColumnTextForButtonValue = false,
                DefaultCellStyle = new DataGridViewCellStyle {
                    BackColor = Color.FromArgb(204, 92, 30), 
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8f),
                    Alignment = DataGridViewContentAlignment.MiddleCenter 
                },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new DataGridViewButtonColumn {
                Name = "colBtnShare", 
                HeaderText = "", 
                Width = 74, 
                FlatStyle = FlatStyle.Flat,
                UseColumnTextForButtonValue = false,
                DefaultCellStyle = new DataGridViewCellStyle {
                    BackColor = Color.FromArgb(40, 140, 210), 
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8f),
                    Alignment = DataGridViewContentAlignment.MiddleCenter 
                },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new DataGridViewButtonColumn {
                Name = "colBtnReview", 
                HeaderText = "", 
                Width = 84, 
                FlatStyle = FlatStyle.Flat,
                UseColumnTextForButtonValue = false,
                DefaultCellStyle = new DataGridViewCellStyle {
                    BackColor = Color.FromArgb(39, 174, 96), 
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8f),
                    Alignment = DataGridViewContentAlignment.MiddleCenter 
                },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            dgvArticles.CellDoubleClick       += DgvArticles_CellDoubleClick;
            dgvArticles.CellClick       += DgvArticles_CellClick;
            dgvArticles.CellFormatting += DgvArticles_CellFormatting;
        }

        // ── DataGridView events ───────────────────────────────────────────────────

        private void DgvArticles_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _cachedArticles.Count) return;
            var article = _cachedArticles[e.RowIndex];
            string col  = dgvArticles.Columns[e.ColumnIndex].Name;

            if (col != "colContentHtml" && col != "colBtnRead" && col != "colBtnEdit" && col != "colBtnShare" && col != "colBtnReview" && article.Status == 1) OnArticleCardClick(article);
        }
        private void DgvArticles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _cachedArticles.Count) return;
            var article = _cachedArticles[e.RowIndex];
            string col = dgvArticles.Columns[e.ColumnIndex].Name;

            if (col == "colContentHtml") OnArticleCardClick(article);
            else if (col == "colBtnRead") OnArticleCardClick(article);
            else if (col == "colBtnEdit") OnEditClick(article);
            else if (col == "colBtnShare") OnShareClick(article);
            else if (col == "colBtnReview" && article.Status == 1) OnSendReviewClick(article);
        }

        private void DgvArticles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _cachedArticles.Count) return;
            var article = _cachedArticles[e.RowIndex];
            string col  = dgvArticles.Columns[e.ColumnIndex].Name;

            if (col == "colRowNum")
            {
                e.Value = ((_currentPage - 1) * PageSize + e.RowIndex + 1).ToString();
                e.FormattingApplied = true;
            }
            else if (col == "colStatus")
            {
                switch (article.Status)
                {
                    case 1: e.CellStyle.ForeColor = Color.FromArgb(100, 100, 100); break; // Draft
                    case 2: e.CellStyle.ForeColor = Color.FromArgb(180, 90, 0);    break; // Review
                    case 3: e.CellStyle.ForeColor = Color.FromArgb(0, 128, 64);    break; // Published
                    case 4: e.CellStyle.ForeColor = Color.FromArgb(139, 0, 0);     break; // Archived
                }
                e.FormattingApplied = true;
            }
            else if (col == "colBtnReview" && article.Status != 1)
            {
                e.CellStyle.BackColor = Color.FromArgb(235, 237, 240);
                e.CellStyle.ForeColor = Color.FromArgb(180, 180, 180);
                e.FormattingApplied   = true;
            }
        }

        private void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.LoadArticles();
        }
    }
}
