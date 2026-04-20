using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Data.Interfaces;
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
        private readonly IKBEditorController     _view;
        private readonly IKBCategoryService _categoryService;
        private readonly IKBArticleService  _articleService;
        private readonly IKBArticleLinkRepository _articleLinkRepository;
        private readonly IKBVersionRepository _versionRepository;
        private readonly UserContext        _user;
        private readonly KBArticle         _editingArticle;

        /// <summary>
        /// Expose the editing article for search exclusion
        /// </summary>
        public KBArticle EditingArticle => _editingArticle;

        public KBEditorController(
            IKBEditorController     view,
            IKBCategoryService categoryService,
            IKBArticleService  articleService,
            IKBArticleLinkRepository articleLinkRepository,
            IKBVersionRepository versionRepository,
            UserContext        user,
            KBArticle          article)   // null = create new
        {
            if (view            == null) throw new ArgumentNullException("view");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (articleLinkRepository == null) throw new ArgumentNullException("articleLinkRepository");
            if (versionRepository == null) throw new ArgumentNullException("versionRepository");
            if (user            == null) throw new ArgumentNullException("user");

            _view            = view;
            _categoryService = categoryService;
            _articleService  = articleService;
            _articleLinkRepository = articleLinkRepository;
            _versionRepository = versionRepository;
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
                // Create mode
                _view.HeaderText           = "  ✏️ เพิ่มบทความใหม่";
                _view.ArticleCode          = string.Format("KB-{0}-NEW", DateTime.Now.Year);
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
                LoadVersionHistory(_editingArticle.ArticleID);
                LoadArticleLinks(_editingArticle.ArticleID);
                UpdateStatusButtons(_editingArticle.Status);
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

            _view.ViewCountText  = string.Format("👁  ดู {0} ครั้ง", a.ViewCount);
            _view.LikeCountText  = string.Format("👍  {0} Like", a.LikeCount);
            
            // Get latest version from KMS.t_KBVersion table
            string versionText = "v1.0";
            try
            {
                var latestVersion = _versionRepository.GetLatestByArticle(a.ArticleID);
                if (latestVersion != null)
                {
                    versionText = string.Format("v{0}.0", latestVersion.VersionNumber);
                }
                else if (a.CurrentVersion > 0)
                {
                    // Fallback to article's CurrentVersion if no version history exists
                    versionText = string.Format("v{0}.0", a.CurrentVersion);
                }
            }
            catch
            {
                // Fallback to article's CurrentVersion on error
                if (a.CurrentVersion > 0)
                {
                    versionText = string.Format("v{0}.0", a.CurrentVersion);
                }
            }
            
            _view.VersionText    = string.Format("Version: {0}", versionText);
            _view.CurVersionText = versionText;
            _view.AuthorText     = string.Format("สร้างโดย: {0}", a.CreatedBy ?? "—");
            _view.DateInfoText   = string.Format("วันที่: {0}", a.CreatedDate.ToString("dd/MM/yyyy"));
        }

        /// <summary>
        /// Load version history for the article
        /// </summary>
        private void LoadVersionHistory(int articleId)
        {
            try
            {
                var versions = _articleService.GetVersionHistory(articleId).ToList();
                _view.BindVersionHistory(versions);
                _view.VersionTabText = string.Format("🕐 ประวัติ Version ({0})", versions.Count);
            }
            catch
            {
                // Silently fail - version history is optional
            }
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
        /// Save as Draft (status stays at 1) or update existing
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
                    ExpiryDate       = null,
                    CreatedBy        = _user.AdIdentity,
                };

                int articleId;
                if (_editingArticle == null)
                {
                    // Create new article
                    articleId = _articleService.Create(article);
                }
                else
                {
                    // Update existing article
                    article.ArticleID = _editingArticle.ArticleID;
                    _articleService.Update(article, changeDesc);
                    articleId = _editingArticle.ArticleID;
                }

                // Save article links
                SaveArticleLinks(articleId);

                _view.CloseOk();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Save failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Advance status: Draft→Review or Review→Published
        /// </summary>
        public void Publish()
        {
            if (_editingArticle == null) return;

            byte targetStatus = _editingArticle.Status == 1 ? (byte)2 : (byte)3;
            _articleService.ChangeStatus(_editingArticle.ArticleID, targetStatus, _user.AdIdentity);
            _view.CloseOk();
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
                // Get article-to-article links (EntityType = 6)
                var links = _articleLinkRepository.GetLinksForArticle(articleId, (byte)KBEntityType.Article);
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
                // Insert article-to-article link (EntityType = 6)
                _articleLinkRepository.Insert(new KBArticleLink
                {
                    ArticleID = _editingArticle.ArticleID,
                    EntityType = (byte)KBEntityType.Article,
                    EntityID = relatedArticleId,
                    CreatedBy = _user.AdIdentity,
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
                // Find and delete the article-to-article link (EntityType = 6)
                var link = _articleLinkRepository.GetLinksForArticle(_editingArticle.ArticleID, (byte)KBEntityType.Article)
                    .FirstOrDefault(l => l.EntityID == relatedArticleId);
                
                if (link != null)
                {
                    _articleLinkRepository.Delete(link.LinkID);
                }
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
                // Delete all existing links for this article
                _articleLinkRepository.DeleteAllForArticle(articleId);

                // Insert links for Folders (EntityType = 1)
                var dgvRelated = _view.GetRelatedDataGridView();
                foreach (System.Windows.Forms.DataGridViewRow row in dgvRelated.Rows)
                {
                    if (row.Tag != null && row.Tag is int)
                    {
                        int entityId = (int)row.Tag;
                        _articleLinkRepository.Insert(new KBArticleLink
                        {
                            ArticleID = articleId,
                            EntityType = (byte)KBEntityType.Article,
                            EntityID = entityId,
                            CreatedBy = _user.AdIdentity,
                            CreatedDate = DateTime.Now
                        });
                    }
                }

                // Insert links for Tasks (EntityType = 2)
                var lstWorkflow = _view.GetWorkflowListView();
                foreach (System.Windows.Forms.ListViewItem item in lstWorkflow.Items)
                {
                    if (item.Tag != null && item.Tag is int)
                    {
                        int entityId = (int)item.Tag;
                        _articleLinkRepository.Insert(new KBArticleLink
                        {
                            ArticleID = articleId,
                            EntityType = (byte)KBEntityType.Task,
                            EntityID = entityId,
                            CreatedBy = _user.AdIdentity,
                            CreatedDate = DateTime.Now
                        });
                    }
                }

                // Insert links for Programs (EntityType = 3)
                var lstPrograms = _view.GetProgramsListView();
                foreach (System.Windows.Forms.ListViewItem item in lstPrograms.Items)
                {
                    if (item.Tag != null && item.Tag is int)
                    {
                        int entityId = (int)item.Tag;
                        _articleLinkRepository.Insert(new KBArticleLink
                        {
                            ArticleID = articleId,
                            EntityType = (byte)KBEntityType.Program,
                            EntityID = entityId,
                            CreatedBy = _user.AdIdentity,
                            CreatedDate = DateTime.Now
                        });
                    }
                }

                // Insert links for Laws (EntityType = 4)
                var lstDocs = _view.GetDocsListView();
                foreach (System.Windows.Forms.ListViewItem item in lstDocs.Items)
                {
                    if (item.Tag != null && item.Tag is int)
                    {
                        int entityId = (int)item.Tag;
                        _articleLinkRepository.Insert(new KBArticleLink
                        {
                            ArticleID = articleId,
                            EntityType = (byte)KBEntityType.Law,
                            EntityID = entityId,
                            CreatedBy = _user.AdIdentity,
                            CreatedDate = DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save article links: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Load article links from database and populate ListViews/DataGridViews
        /// </summary>
        private void LoadArticleLinks(int articleId)
        {
            try
            {
                // Load Related Articles (EntityType = 6) - Article-to-Article links
                var articleLinks = _articleLinkRepository.GetLinksForArticle(articleId, (byte)KBEntityType.Article);
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

                // Load Task links (EntityType = 2)
                var taskLinks = _articleLinkRepository.GetLinksForArticle(articleId, (byte)KBEntityType.Task);
                var dgvWorkflow = _view.GetWorkflowSearchDataGridView();
                dgvWorkflow.Rows.Clear();

                index = 1;
                foreach (var link in articleLinks)
                {
                    // Retrieve the full article details including Title using ArticleID (EntityID)
                    var linkedArticle = _articleService.GetById(link.EntityID);

                    if (linkedArticle != null)
                    {
                        int rowIndex = dgvWorkflow.Rows.Add(
                            index.ToString(),
                            linkedArticle.ArticleCode ?? "-",
                            linkedArticle.Title ?? "-",
                            linkedArticle.ArticleTypeName ?? "-"
                        );

                        dgvWorkflow.Rows[rowIndex].Tag = linkedArticle.ArticleID;
                        dgvWorkflow.Rows[rowIndex].DefaultCellStyle.BackColor =
                            index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                        index++;
                    }
                }

                // Load Program links (EntityType = 3)
                var programLinks = _articleLinkRepository.GetLinksForArticle(articleId, (byte)KBEntityType.Program);
                var dgvPrograms = _view.GetProgramsSearchDataGridView();
                dgvPrograms.Rows.Clear();

                index = 1;
                foreach (var link in articleLinks)
                {
                    // Retrieve the full article details including Title using ArticleID (EntityID)
                    var linkedArticle = _articleService.GetById(link.EntityID);

                    if (linkedArticle != null)
                    {
                        int rowIndex = dgvPrograms.Rows.Add(
                            index.ToString(),
                            linkedArticle.ArticleCode ?? "-",
                            linkedArticle.Title ?? "-",
                            linkedArticle.ArticleTypeName ?? "-"
                        );

                        dgvPrograms.Rows[rowIndex].Tag = linkedArticle.ArticleID;
                        dgvPrograms.Rows[rowIndex].DefaultCellStyle.BackColor =
                            index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                        index++;
                    }
                }

                // Load Document/Law links (EntityType = 4) - using Document list for now
                var lawLinks = _articleLinkRepository.GetLinksForArticle(articleId, (byte)KBEntityType.Project);
                var dgvDocs = _view.GetDocsSearchDataGridView();
                dgvDocs.Rows.Clear();

                index = 1;
                foreach (var link in articleLinks)
                {
                    // Retrieve the full article details including Title using ArticleID (EntityID)
                    var linkedArticle = _articleService.GetById(link.EntityID);

                    if (linkedArticle != null)
                    {
                        int rowIndex = dgvDocs.Rows.Add(
                            index.ToString(),
                            linkedArticle.ArticleCode ?? "-",
                            linkedArticle.Title ?? "-",
                            linkedArticle.ArticleTypeName ?? "-"
                        );

                        dgvDocs.Rows[rowIndex].Tag = linkedArticle.ArticleID;
                        dgvDocs.Rows[rowIndex].DefaultCellStyle.BackColor =
                            index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลดข้อมูลความเชื่อมโยงไม่สำเร็จ: " + ex.Message);
            }
        }
    }
}
