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
            UserContext        user,
            KBArticle          article)   // null = create new
        {
            if (view            == null) throw new ArgumentNullException("view");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (articleLinkRepository == null) throw new ArgumentNullException("articleLinkRepository");
            if (user            == null) throw new ArgumentNullException("user");

            _view            = view;
            _categoryService = categoryService;
            _articleService  = articleService;
            _articleLinkRepository = articleLinkRepository;
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
                _view.NoExpiry             = true;
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
            _view.IsOfficialPolicy    = a.IsOfficialPolicy;
            _view.SelectedCategoryId  = a.CategoryID;
            _view.SelectedArticleType = (byte)a.ArticleType;
            _view.SelectedStatus      = a.Status;
            _view.NoExpiry            = !a.ExpiryDate.HasValue;

            if (a.ExpiryDate.HasValue)
                _view.ExpiryDate = a.ExpiryDate.Value;

            _view.ViewCountText  = string.Format("👁  ดู {0} ครั้ง", a.ViewCount);
            _view.LikeCountText  = string.Format("👍  {0} Like", a.LikeCount);
            _view.VersionText    = string.Format("Version: {0}.0", a.CurrentVersion > 0 ? a.CurrentVersion : 1);
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
                    IsOfficialPolicy = _view.IsOfficialPolicy,
                    Tags             = _view.Tags,
                    ExpiryDate       = _view.NoExpiry ? (DateTime?)null : _view.ExpiryDate,
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
        /// Save article links to database
        /// </summary>
        private void SaveArticleLinks(int articleId)
        {
            try
            {
                // Delete all existing links for this article
                _articleLinkRepository.DeleteAllForArticle(articleId);

                // Insert links for Folders (EntityType = 1)
                var lstRelated = _view.GetRelatedListView();
                foreach (System.Windows.Forms.ListViewItem item in lstRelated.Items)
                {
                    if (item.Tag != null && item.Tag is int)
                    {
                        int entityId = (int)item.Tag;
                        _articleLinkRepository.Insert(new KBArticleLink
                        {
                            ArticleID = articleId,
                            EntityType = (byte)KBEntityType.Folder,
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
        /// Load article links from database and populate ListViews
        /// </summary>
        private void LoadArticleLinks(int articleId)
        {
            try
            {
                // Note: The old implementation had "Related" articles linking to other articles,
                // but the database schema (KMS.t_KBArticleLink) only supports linking to entities
                // (Folder/Task/Program/Law/Project), not to other articles.
                // The "Related" section is commented out until the schema is clarified.

                // Load Folder links (EntityType = 1)
                var folderLinks = _articleLinkRepository.GetLinksForArticle(articleId, (byte)KBEntityType.Folder);
                var lstRelated = _view.GetRelatedListView();
                lstRelated.Items.Clear();

                foreach (var link in folderLinks)
                {
                    // Note: You may need to query folder details from appropriate service
                    var item = new System.Windows.Forms.ListViewItem(link.EntityID.ToString());
                    item.SubItems.Add("Folder"); // Placeholder
                    item.SubItems.Add("-"); // Placeholder
                    item.Tag = link.EntityID;
                    lstRelated.Items.Add(item);
                }

                // Load Task links (EntityType = 2)
                var taskLinks = _articleLinkRepository.GetLinksForArticle(articleId, (byte)KBEntityType.Task);
                var lstWorkflow = _view.GetWorkflowListView();
                lstWorkflow.Items.Clear();

                foreach (var link in taskLinks)
                {
                    // Note: You may need to query task details from appropriate service
                    var item = new System.Windows.Forms.ListViewItem(link.EntityID.ToString());
                    item.SubItems.Add("Task"); // Placeholder
                    item.SubItems.Add("-"); // Placeholder
                    item.Tag = link.EntityID;
                    lstWorkflow.Items.Add(item);
                }

                // Load Program links (EntityType = 3)
                var programLinks = _articleLinkRepository.GetLinksForArticle(articleId, (byte)KBEntityType.Program);
                var lstPrograms = _view.GetProgramsListView();
                lstPrograms.Items.Clear();

                foreach (var link in programLinks)
                {
                    // Note: You may need to query program details from appropriate service
                    var item = new System.Windows.Forms.ListViewItem(link.EntityID.ToString());
                    item.SubItems.Add("Program"); // Placeholder
                    item.SubItems.Add("-"); // Placeholder
                    item.Tag = link.EntityID;
                    lstPrograms.Items.Add(item);
                }

                // Load Law links (EntityType = 4) - using Document list for now
                var lawLinks = _articleLinkRepository.GetLinksForArticle(articleId, (byte)KBEntityType.Law);
                var lstDocs = _view.GetDocsListView();
                lstDocs.Items.Clear();

                foreach (var link in lawLinks)
                {
                    // Note: You may need to query law details from appropriate service
                    var item = new System.Windows.Forms.ListViewItem(link.EntityID.ToString());
                    item.SubItems.Add("Law"); // Placeholder
                    item.SubItems.Add("-"); // Placeholder
                    item.Tag = link.EntityID;
                    lstDocs.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลดข้อมูลความเชื่อมโยงไม่สำเร็จ: " + ex.Message);
            }
        }
    }
}
