using CMS.Controllers.KBS.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CMS.Controllers.KBS
{
    public class ArticleEditorDialogPresenter
    {
        private readonly IArticleEditorDialogView _view;
        private readonly IKBCategoryService       _categoryService;
        private readonly IKBArticleService        _articleService;
        private readonly UserContext              _user;
        private readonly KBArticle               _editingArticle;

        public ArticleEditorDialogPresenter(IArticleEditorDialogView view, IKBCategoryService categoryService, IKBArticleService articleService, UserContext user, KBArticle article)
        {
            if (view == null) throw new ArgumentNullException("view");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (articleService == null) throw new ArgumentNullException("articleService");
            if (user == null) throw new ArgumentNullException("user");
            _view = view; _categoryService = categoryService; _articleService = articleService; _user = user; _editingArticle = article;
        }

        public void Initialize()
        {
            LoadDropdowns();
            if (_editingArticle == null)
            {
                _view.HeaderText = "  Add New Article";
                _view.ArticleCode = string.Format("KB-{0}-NEW", DateTime.Now.Year);
                _view.NoExpiry = true;
                _view.ArchiveButtonVisible = false;
                _view.PublishButtonText = "Submit Review";
            }
            else
            {
                _view.HeaderText = "  Edit Article: " + (_editingArticle.Title ?? "");
                PopulateFields(_editingArticle);
                LoadVersionHistory(_editingArticle.ArticleID);
                UpdateStatusButtons(_editingArticle.Status);
            }
        }

        private void LoadDropdowns()
        {
            try { _view.BindCategories(_categoryService.GetAllActive()); }
            catch { _view.BindCategories(new List<KBCategory>()); }
        }

        private void PopulateFields(KBArticle a)
        {
            _view.ArticleCode = a.ArticleCode ?? string.Empty;
            _view.Title = a.Title ?? string.Empty;
            _view.Summary = a.Summary ?? string.Empty;
            _view.ContentHTML = a.ContentHTML ?? string.Empty;
            _view.Tags = a.Tags ?? string.Empty;
            _view.IsOfficialPolicy = a.IsOfficialPolicy;
            _view.SelectedCategoryId = a.CategoryID;
            _view.SelectedArticleType = (byte)a.ArticleType;
            _view.SelectedStatus = a.Status;
            _view.NoExpiry = !a.ExpiryDate.HasValue;
            if (a.ExpiryDate.HasValue) _view.ExpiryDate = a.ExpiryDate.Value;
            _view.ViewCountText = string.Format("Views: {0}", a.ViewCount);
            _view.LikeCountText = string.Format("Likes: {0}", a.LikeCount);
        }

        private void LoadVersionHistory(int articleId)
        {
            try
            {
                var versions = _articleService.GetVersionHistory(articleId).ToList();
                _view.BindVersionHistory(versions);
                _view.VersionTabText = string.Format("Version History ({0})", versions.Count);
            }
            catch { }
        }

        private void UpdateStatusButtons(byte status)
        {
            bool isArchived = (status == 4);
            _view.SaveEnabled = !isArchived;
            if (status == 1 || status == 2)
            {
                _view.PublishButtonText = status == 1 ? "Submit Review" : "Publish";
                _view.PublishButtonColor = status == 1 ? Color.FromArgb(0, 120, 215) : Color.FromArgb(34, 139, 34);
                _view.ArchiveButtonVisible = false;
            }
            else if (status == 3)
            {
                _view.PublishButtonVisible = false;
                _view.ArchiveButtonVisible = true;
            }
            else if (status == 4)
            {
                _view.PublishButtonVisible = false;
                _view.ArchiveButtonVisible = false;
                _view.SaveEnabled = false;
            }
        }

        public void Save(string changeDesc)
        {
            try
            {
                var article = new KBArticle
                {
                    ArticleCode = _view.ArticleCode,
                    Title = _view.Title,
                    Summary = _view.Summary,
                    ContentHTML = _view.ContentHTML,
                    CategoryID = _view.SelectedCategoryId,
                    ArticleType = _view.SelectedArticleType,
                    Status = 1,
                    IsOfficialPolicy = _view.IsOfficialPolicy,
                    Tags = _view.Tags,
                    ExpiryDate = _view.NoExpiry ? (DateTime?)null : _view.ExpiryDate,
                    CreatedBy = _user.AdIdentity,
                };
                if (_editingArticle == null)
                    _articleService.Create(article);
                else
                {
                    article.ArticleID = _editingArticle.ArticleID;
                    _articleService.Update(article, changeDesc);
                }
                _view.CloseOk();
            }
            catch (Exception ex) { throw new InvalidOperationException("Save failed: " + ex.Message, ex); }
        }

        public void Publish()
        {
            if (_editingArticle == null) return;
            byte target = _editingArticle.Status == 1 ? (byte)2 : (byte)3;
            _articleService.ChangeStatus(_editingArticle.ArticleID, target, _user.AdIdentity);
            _view.CloseOk();
        }

        public void Archive()
        {
            if (_editingArticle == null) return;
            _articleService.ChangeStatus(_editingArticle.ArticleID, 4, _user.AdIdentity);
            _view.CloseOk();
        }
    }
}
