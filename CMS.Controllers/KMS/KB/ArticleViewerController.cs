using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Controllers.KMS.KB
{
    public class ArticleViewerController
    {
        private readonly IArticleViewerController _view;
        private readonly IKBArticleService        _articleService;
        private readonly IKBVersionRepository     _versionRepository;
        private readonly IKBArticleLinkRepository _articleLinkRepository;
        private readonly UserContext              _user;
        private KBArticle                        _article;

        public ArticleViewerController(
            IArticleViewerController view,
            IKBArticleService        articleService,
            IKBVersionRepository     versionRepository,
            IKBArticleLinkRepository articleLinkRepository,
            UserContext              user,
            KBArticle                article)
        {
            if (view                 == null) throw new ArgumentNullException("view");
            if (articleService       == null) throw new ArgumentNullException("articleService");
            if (versionRepository    == null) throw new ArgumentNullException("versionRepository");
            if (articleLinkRepository == null) throw new ArgumentNullException("articleLinkRepository");
            if (user                 == null) throw new ArgumentNullException("user");
            if (article              == null) throw new ArgumentNullException("article");
            _view = view; _articleService = articleService;
            _versionRepository = versionRepository;
            _articleLinkRepository = articleLinkRepository;
            _user = user; _article = article;
        }

        /// <summary>Push initial data into the view.</summary>
        public void Initialize()
        {
            ResolveLatestVersion(_article);
            _view.PopulateArticle(_article);
            LoadVersionHistory();
            LoadRelatedArticles(_article.ArticleID);
        }

        /// <summary>Re-fetch article from DB and refresh the view after an edit.</summary>
        public void RefreshAfterEdit()
        {
            try
            {
                var updated = _articleService.GetById(_article.ArticleID);
                if (updated != null)
                {
                    _article = updated;
                    ResolveLatestVersion(_article);
                    _view.PopulateArticle(_article);
                    LoadVersionHistory();
                    LoadRelatedArticles(_article.ArticleID);
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Refresh failed: " + ex.Message);
            }
        }

        private void LoadVersionHistory()
        {
            try
            {
                var versions = _articleService.GetVersionHistory(_article.ArticleID)
                                              .OrderByDescending(v => v.VersionNumber);
                _view.BindVersionHistory(versions);
            }
            catch { /* version history is non-critical */ }
        }

        /// <summary>
        /// Loads articles linked via KMS.t_KBArticleLink (EntityType=Article/6),
        /// joining with KMS.t_KBArticle using EntityID → ArticleID.
        /// </summary>
        private void LoadRelatedArticles(int articleId)
        {
            try
            {
                var links = _articleLinkRepository.GetLinksForArticle(
                    articleId, (byte)KBEntityType.Law);

                var related = new List<KBArticle>();
                foreach (var link in links)
                {
                    var a = _articleService.GetById(link.EntityID);
                    if (a != null)
                        related.Add(a);
                }

                _view.BindRelatedArticles(related);
            }
            catch { _view.BindRelatedArticles(new List<KBArticle>()); }
        }

        /// <summary>
        /// Resolves the latest VersionNumber from KMS.t_KBVersion and writes it
        /// back to article.CurrentVersion so the view always shows the DB value.
        /// </summary>
        private void ResolveLatestVersion(KBArticle article)
        {
            try
            {
                var latest = _versionRepository.GetLatestByArticle(article.ArticleID);
                if (latest != null)
                    article.CurrentVersion = latest.VersionNumber;
            }
            catch { /* non-critical — keep whatever CurrentVersion was already set */ }
        }
    }
}
