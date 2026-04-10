using CMS.Controllers.KMS.KB.Interfaces;
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
        private readonly UserContext        _user;
        private readonly KBArticle         _editingArticle;

        public KBEditorController(
            IKBEditorController     view,
            IKBCategoryService categoryService,
            IKBArticleService  articleService,
            UserContext        user,
            KBArticle          article)   // null = create new
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

                if (_editingArticle == null)
                {
                    // Create new article
                    _articleService.Create(article);
                }
                else
                {
                    // Update existing article
                    article.ArticleID = _editingArticle.ArticleID;
                    _articleService.Update(article, changeDesc);
                }

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
    }
}
