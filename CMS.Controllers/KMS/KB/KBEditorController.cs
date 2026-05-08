using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CMS.Controllers.KMS.KB
{
    /// <summary>
    /// Presenter for KBEditor dialog (MOD-006)
    /// Redesigned from ArticleEditorDialogPresenter
    /// Handles: Create / Edit / Send Review / Publish / Archive + Version History
    /// </summary>
    public class KBEditorController
    {
        private readonly IKBEditorController _view;
        private readonly IKBCategoryService  _categoryService;
        private readonly IKBArticleService   _articleService;
        private readonly UserContext         _user;
        private KBArticle                   _editingArticle;  // Changed from readonly to allow updates

        /// <summary>
        /// Expose the editing article for search exclusion
        /// </summary>
        public KBArticle EditingArticle => _editingArticle;

        public KBEditorController(
            IKBEditorController view,
            IKBCategoryService  categoryService,
            IKBArticleService   articleService,
            UserContext         user,
            KBArticle           article)   // null = create new
        {
            if (view            == null) throw new ArgumentNullException("view");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (user            == null) throw new ArgumentNullException("user");

            _view            = view;
            _categoryService = categoryService;
            _articleService  = articleService;
            _user            = user;
            _editingArticle  = article;
        }

        /// <summary>
        /// Initialize the view with data
        /// </summary>
        public void Initialize()
        {
            LoadDropdowns();

            if (_editingArticle == null)
            {
                // Create mode - Generate next article code
                string nextCode = GenerateArticleCode();

                _view.HeaderText           = "  ✏️ เพิ่มบทความใหม่";
                _view.ArticleCode          = nextCode;
                _view.SelectedStatus       = 1;  // Default to Draft status
                _view.ArchiveButtonVisible = false;
                _view.PublishButtonText    = "📤 ส่งขอ Review";
                _view.VersionText          = "Version: 1.0";
                _view.AuthorText           = string.Format("สร้างโดย: {0}", _user.AdIdentity);
                _view.DateInfoText         = string.Format("วันที่: {0}", DateTime.Now.ToString("dd/MM/yyyy"));
            }
            else
            {
                // Edit mode
                _view.HeaderText = string.Format("  ✏️ แก้ไขบทความ: {0}", _editingArticle.Title ?? "");
                PopulateFields(_editingArticle);
                LoadArticleLinks(_editingArticle.ArticleID);
                UpdateStatusButtons(_editingArticle.Status);
            }
        }

        /// <summary>
        /// Generate next article code in format KB-YYYY-NNN
        /// </summary>
        private string GenerateArticleCode()
        {
            try
            {
                return _articleService.GenerateNextArticleCode();
            }
            catch (Exception ex)
            {
                // Fallback to default format if generation fails
                return string.Format("KB-{0}-001", DateTime.Now.Year);
            }
        }

        /// <summary>
        /// Load dropdown data (categories)
        /// </summary>
        private void LoadDropdowns()
        {
            try
            {
                _view.BindCategories(_categoryService.GetAllActive());
            }
            catch
            {
                _view.BindCategories(new List<KBCategory>());
            }
        }

        /// <summary>
        /// Populate view fields from article entity
        /// </summary>
        private void PopulateFields(KBArticle a)
        {
            _view.ArticleCode         = a.ArticleCode ?? string.Empty;
            _view.Title               = a.Title       ?? string.Empty;
            _view.Summary             = a.Summary     ?? string.Empty;
            _view.ContentHTML         = a.ContentHTML ?? string.Empty;
            _view.Tags                = a.Tags        ?? string.Empty;
            _view.SelectedCrafLevel   = a.CrafLevel   ?? "1HC - (High Confidential)";
            _view.SelectedCategoryId  = a.CategoryID;
            _view.SelectedArticleType = (byte)a.ArticleType;
            _view.SelectedStatus      = a.Status;

            if (a.ExpiryDate.HasValue)
                _view.ExpiryDate = a.ExpiryDate.Value;

            _view.ViewCountText  = string.Format("👁  ดู {0} ครั้ง", a.ViewCount);
            _view.LikeCountText  = string.Format("👍  {0} Like", a.LikeCount);

            // Get latest version from KMS.t_KBVersion table via service
            string versionText = "v1.0";
            try
            {
                int latestVer = _articleService.GetLatestVersionNumber(a.ArticleID);
                if (latestVer > 0)
                    versionText = string.Format("v{0}.0", latestVer);
                else if (a.CurrentVersion > 0)
                    versionText = string.Format("v{0}.0", a.CurrentVersion);
            }
            catch
            {
                if (a.CurrentVersion > 0)
                    versionText = string.Format("v{0}.0", a.CurrentVersion);
            }

            _view.VersionText    = string.Format("Version: {0}", versionText);
            _view.CurVersionText = versionText;
            _view.AuthorText     = string.Format("สร้างโดย: {0}", a.CreatedBy ?? "—");
            _view.DateInfoText   = string.Format("วันที่: {0}", a.CreatedDate.ToString("dd/MM/yyyy"));
        }

        /// <summary>
        /// Update button visibility and text based on article status
        /// </summary>
        private void UpdateStatusButtons(byte status)
        {
            bool isArchived  = (status == 4);
            bool isPublished = (status == 3);

            _view.SaveEnabled = !isArchived;

            if (status == 1 || status == 2)
            {
                // Draft or Review
                _view.PublishButtonText  = status == 1 ? "📤 ส่งขอ Review" : "✅ เผยแพร่";
                _view.PublishButtonColor = status == 1
                    ? Color.FromArgb(46, 116, 181)
                    : Color.FromArgb(39, 174, 96);
                _view.ArchiveButtonVisible = false;
            }
            else if (status == 3)
            {
                // Published
                _view.PublishButtonVisible = false;
                _view.ArchiveButtonVisible = true;
            }
            else if (status == 4)
            {
                // Archived
                _view.PublishButtonVisible = false;
                _view.ArchiveButtonVisible = false;
                _view.SaveEnabled          = false;
            }
        }

        /// <summary>
        /// Save as Draft (status = 1) - Create new or update existing
        /// </summary>
        public void Save(string changeDesc)
        {
            try
            {
                var article = new KBArticle
                {
                    ArticleCode      = _view.ArticleCode,
                    Title            = _view.Title,
                    Summary          = _view.Summary,
                    ContentHTML      = _view.ContentHTML,
                    CategoryID       = _view.SelectedCategoryId,
                    ArticleType      = _view.SelectedArticleType,
                    Status           = 1,  // Always save as Draft
                    CrafLevel        = _view.SelectedCrafLevel,
                    Tags             = _view.Tags,
                    ExpiryDate       = _view.ExpiryDate,
                    CreatedBy        = _user.AdIdentity,
                };

                int articleId;
                if (_editingArticle == null)
                {
                    // Create new article
                    articleId = _articleService.Create(article);

                    // Update _editingArticle so subsequent saves will update instead of create
                    article.ArticleID = articleId;
                    _editingArticle = article;

                    // Update view to reflect we're now editing an existing article
                    _view.HeaderText = string.Format("  ✏️ แก้ไขบทความ: {0}", article.Title ?? "");
                }
                else
                {
                    // Update existing article
                    article.ArticleID = _editingArticle.ArticleID;
                    _articleService.Update(article, changeDesc);
                    articleId = _editingArticle.ArticleID;

                    // Update _editingArticle with latest data
                    _editingArticle = article;
                }

                // Save article links
                SaveArticleLinks(articleId);

                _view.ShowInfo("บันทึก Draft เรียบร้อยแล้ว");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Save failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Send for Review (status = 2) - Create new or update existing, then change status
        /// </summary>
        public void SendReview()
        {
            try
            {
                var article = new KBArticle
                {
                    ArticleCode      = _view.ArticleCode,
                    Title            = _view.Title,
                    Summary          = _view.Summary,
                    ContentHTML      = _view.ContentHTML,
                    CategoryID       = _view.SelectedCategoryId,
                    ArticleType      = _view.SelectedArticleType,
                    Status           = 2,  // Send for Review
                    CrafLevel        = _view.SelectedCrafLevel,
                    Tags             = _view.Tags,
                    ExpiryDate       = _view.ExpiryDate,
                    CreatedBy        = _user.AdIdentity,
                };

                int articleId;
                if (_editingArticle == null)
                {
                    // Create new article with Status = 2
                    articleId = _articleService.Create(article);

                    // Update _editingArticle
                    article.ArticleID = articleId;
                    _editingArticle = article;
                }
                else
                {
                    // Update existing article and change status to Review
                    article.ArticleID = _editingArticle.ArticleID;
                    _articleService.Update(article, "ส่งขอ Review");
                    articleId = _editingArticle.ArticleID;

                    // Update _editingArticle
                    _editingArticle = article;
                }

                // Save article links
                SaveArticleLinks(articleId);

                _view.ShowInfo("ส่งขอ Review เรียบร้อยแล้ว");
                _view.CloseOk();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Send review failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Publish article (status = 3) - Only for existing articles in Review status
        /// </summary>
        public void Publish()
        {
            if (_editingArticle == null)
            {
                _view.ShowError("ไม่สามารถเผยแพร่บทความที่ยังไม่ได้บันทึกได้");
                return;
            }

            try
            {
                // Change status to Published
                _articleService.ChangeStatus(_editingArticle.ArticleID, 3, _user.AdIdentity);
                _editingArticle.Status = 3;

                _view.ShowInfo("เผยแพร่บทความเรียบร้อยแล้ว");
                _view.CloseOk();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Publish failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Move to Archived status
        /// </summary>
        public void Archive()
        {
            if (_editingArticle == null) return;

            _articleService.ChangeStatus(_editingArticle.ArticleID, 4, _user.AdIdentity);
            _view.CloseOk();
        }

        /// <summary>
        /// Search published articles for inline search in Related tab
        /// </summary>
        public IEnumerable<KBArticle> SearchPublishedArticles(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<KBArticle>();

            return _articleService.SearchPublishedArticles(searchText);
        }

        /// <summary>
        /// Get all active categories from KB
        /// </summary>
        public IEnumerable<KBCategory> GetAllCategories()
        {
            try
            {
                return _categoryService.GetAllActive();
            }
            catch
            {
                return Enumerable.Empty<KBCategory>();
            }
        }

        /// <summary>
        /// Get all published articles from KB
        /// </summary>
        public IEnumerable<KBArticle> GetAllPublishedArticles()
        {
            try
            {
                // Get all articles with Status = 3 (Published)
                return _articleService.Search(null, null, 3, null);
            }
            catch
            {
                return Enumerable.Empty<KBArticle>();
            }
        }

        /// <summary>
        /// Search articles with filters
        /// </summary>
        public IEnumerable<KBArticle> SearchArticles(string searchText, int? categoryId, byte? status, byte? articleType)
        {
            try
            {
                return _articleService.Search(searchText, categoryId, status, articleType);
            }
            catch
            {
                return Enumerable.Empty<KBArticle>();
            }
        }

        /// <summary>
        /// Get linked articles for the current article
        /// </summary>
        public IEnumerable<KBArticle> GetLinkedArticles(int articleId)
        {
            try
            {
                // Get article-to-article links (EntityType = Law, matching ArticleViewerController)
                var links = _articleService.GetArticleLinks(articleId, (byte)KBEntityType.Law);
                var linkedIds = links.Select(l => l.EntityID).ToList();

                if (!linkedIds.Any())
                    return Enumerable.Empty<KBArticle>();

                // Retrieve full article details for each linked article
                var linkedArticles = new List<KBArticle>();
                foreach (var id in linkedIds)
                {
                    var article = _articleService.GetById(id);
                    if (article != null)
                        linkedArticles.Add(article);
                }

                return linkedArticles;
            }
            catch
            {
                return Enumerable.Empty<KBArticle>();
            }
        }

        /// <summary>
        /// Add a related article link
        /// </summary>
        public void AddRelatedArticleLink(int relatedArticleId)
        {
            if (_editingArticle == null)
                throw new InvalidOperationException("Cannot add link: No article is being edited");

            try
            {
                _articleService.AddArticleLink(new KBArticleLink
                {
                    ArticleID   = _editingArticle.ArticleID,
                    EntityType  = (byte)KBEntityType.Law,
                    EntityID    = relatedArticleId,
                    CreatedBy   = _user.AdIdentity,
                    CreatedDate = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to add related article link: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove a related article link
        /// </summary>
        public void RemoveRelatedArticleLink(int relatedArticleId)
        {
            if (_editingArticle == null)
                throw new InvalidOperationException("Cannot remove link: No article is being edited");

            try
            {
                _articleService.RemoveArticleLink(
                    _editingArticle.ArticleID, relatedArticleId, (byte)KBEntityType.Law);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to remove related article link: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Save article links to database
        /// </summary>
        private void SaveArticleLinks(int articleId)
        {
            try
            {
                var links = new List<KBArticleLink>();

                // Related Articles (EntityType = Law, matching ArticleViewerController)
                var dgvRelated = _view.GetRelatedDataGridView();
                foreach (System.Windows.Forms.DataGridViewRow row in dgvRelated.Rows)
                {
                    if (row.Tag != null && row.Tag is int)
                        links.Add(new KBArticleLink
                        {
                            EntityType  = (byte)KBEntityType.Law,
                            EntityID    = (int)row.Tag,
                            CreatedBy   = _user.AdIdentity,
                            CreatedDate = DateTime.Now
                        });
                }

                // Workflows (EntityType = Task)
                var dgvWorkflow = _view.GetWorkflowLinkedDataGridView();
                foreach (System.Windows.Forms.DataGridViewRow row in dgvWorkflow.Rows)
                {
                    if (row.Tag != null && row.Tag is int)
                        links.Add(new KBArticleLink
                        {
                            EntityType  = (byte)KBEntityType.Task,
                            EntityID    = (int)row.Tag,
                            CreatedBy   = _user.AdIdentity,
                            CreatedDate = DateTime.Now
                        });
                }

                // Programs (EntityType = Program)
                var dgvPrograms = _view.GetProgramsLinkedDataGridView();
                foreach (System.Windows.Forms.DataGridViewRow row in dgvPrograms.Rows)
                {
                    if (row.Tag != null && row.Tag is int)
                        links.Add(new KBArticleLink
                        {
                            EntityType  = (byte)KBEntityType.Program,
                            EntityID    = (int)row.Tag,
                            CreatedBy   = _user.AdIdentity,
                            CreatedDate = DateTime.Now
                        });
                }

                // Documents (EntityType = Law)
                var dgvDocs = _view.GetDocsLinkedDataGridView();
                foreach (System.Windows.Forms.DataGridViewRow row in dgvDocs.Rows)
                {
                    if (row.Tag != null && row.Tag is int)
                        links.Add(new KBArticleLink
                        {
                            EntityType  = (byte)KBEntityType.Law,
                            EntityID    = (int)row.Tag,
                            CreatedBy   = _user.AdIdentity,
                            CreatedDate = DateTime.Now
                        });
                }

                _articleService.SaveArticleLinks(articleId, links);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save article links: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Load article links from database and populate DataGridViews
        /// </summary>
        private void LoadArticleLinks(int articleId)
        {
            try
            {
                // Load Related Articles (EntityType = Law, matching ArticleViewerController)
                var articleLinks = _articleService.GetArticleLinks(articleId, (byte)KBEntityType.Law);
                var dgvRelated = _view.GetRelatedDataGridView();
                dgvRelated.Rows.Clear();

                int index = 1;
                foreach (var link in articleLinks)
                {
                    // Retrieve the full article details including Title using ArticleID (EntityID)
                    var linkedArticle = _articleService.GetById(link.EntityID);

                    if (linkedArticle != null)
                    {
                        int rowIndex = dgvRelated.Rows.Add(
                            index.ToString(),
                            linkedArticle.ArticleCode ?? "-",
                            linkedArticle.Title ?? "-",
                            linkedArticle.ArticleTypeName ?? "-"
                        );

                        dgvRelated.Rows[rowIndex].Tag = linkedArticle.ArticleID;
                        dgvRelated.Rows[rowIndex].DefaultCellStyle.BackColor =
                            index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                        index++;
                    }
                }

                // Load Workflow links (EntityType = Task)
                var taskLinks = _articleService.GetArticleLinks(articleId, (byte)KBEntityType.Task);
                var dgvWorkflow = _view.GetWorkflowLinkedDataGridView();
                dgvWorkflow.Rows.Clear();

                index = 1;
                foreach (var link in taskLinks)
                {
                    // For workflows, we would retrieve workflow details from workflow system
                    // For now, using mock data structure similar to articles
                    int rowIndex = dgvWorkflow.Rows.Add(
                        index.ToString(),
                        string.Format("WF-{0:D3}", link.EntityID),
                        "Workflow Name",  // TODO: Get from workflow system
                        "Active"
                    );

                    dgvWorkflow.Rows[rowIndex].Tag = link.EntityID;
                    dgvWorkflow.Rows[rowIndex].DefaultCellStyle.BackColor =
                        index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    index++;
                }

                // Load Program links (EntityType = Program)
                var programLinks = _articleService.GetArticleLinks(articleId, (byte)KBEntityType.Program);
                var dgvPrograms = _view.GetProgramsLinkedDataGridView();
                dgvPrograms.Rows.Clear();

                index = 1;
                foreach (var link in programLinks)
                {
                    // For programs, we would retrieve program details from program system
                    // For now, using mock data structure
                    int rowIndex = dgvPrograms.Rows.Add(
                        index.ToString(),
                        string.Format("PRG-{0:D3}", link.EntityID),
                        "Program Name",  // TODO: Get from program system
                        "Active"
                    );

                    dgvPrograms.Rows[rowIndex].Tag = link.EntityID;
                    dgvPrograms.Rows[rowIndex].DefaultCellStyle.BackColor =
                        index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    index++;
                }

                // Load Document/Law links (EntityType = Law)
                var lawLinks = _articleService.GetArticleLinks(articleId, (byte)KBEntityType.Law);
                var dgvDocs = _view.GetDocsLinkedDataGridView();
                dgvDocs.Rows.Clear();

                index = 1;
                foreach (var link in lawLinks)
                {
                    // For documents, we would retrieve document details from DMS
                    // For now, using mock data structure
                    int rowIndex = dgvDocs.Rows.Add(
                        index.ToString(),
                        string.Format("DOC-{0:D3}", link.EntityID),
                        "1.2 MB",
                        DateTime.Now.ToString("dd/MM/yyyy")
                    );

                    dgvDocs.Rows[rowIndex].Tag = link.EntityID;
                    dgvDocs.Rows[rowIndex].DefaultCellStyle.BackColor =
                        index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    index++;
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลดข้อมูลความเชื่อมโยงไม่สำเร็จ: " + ex.Message);
            }
        }
    }
}
