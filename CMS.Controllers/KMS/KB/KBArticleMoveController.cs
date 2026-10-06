using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.KMS.KB
{
    public class KBArticleMoveController
    {
        private readonly IKBArticleMoveController _view;
        private readonly IKBArticleMoveService    _service;
        private readonly IKBCategoryService       _categoryService;
        private readonly UserContext              _user;

        private IList<KBCategory> _categories = new List<KBCategory>();
        private IList<KBArticle>  _candidates = new List<KBArticle>();

        public KBArticleMoveController(
            IKBArticleMoveController view,
            IKBArticleMoveService    service,
            IKBCategoryService       categoryService,
            UserContext              user)
        {
            if (view            == null) throw new ArgumentNullException("view");
            if (service         == null) throw new ArgumentNullException("service");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (user            == null) throw new ArgumentNullException("user");
            _view            = view;
            _service         = service;
            _categoryService = categoryService;
            _user            = user;
        }

        public void Initialize()
        {
            try
            {
                _categories = _categoryService.GetAll().ToList();
                _view.BindCategories(_categories);
                RefreshSource();
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลดหมวดหมู่ล้มเหลว: " + ex.Message);
            }
        }

        /// <summary>เรียกเมื่อเปลี่ยนโหมด / หมวดต้นทาง / ติ๊ก "รวมหมวดย่อย" — โหลดรายการบทความต้นทางใหม่</summary>
        public void RefreshSource()
        {
            try
            {
                var ids = _view.SelectedSourceCategoryIds;
                _candidates = (ids == null || ids.Count == 0)
                    ? new List<KBArticle>()
                    : _service.GetMoveCandidates(ids, _view.IncludeSubCategories);
                _view.BindSourceArticles(_candidates, _view.SourceWholeCategory);
                RefreshSummary();
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลดรายการบทความล้มเหลว: " + ex.Message);
            }
        }

        /// <summary>เรียกเมื่อติ๊กบทความ / เปลี่ยนหมวดปลายทาง — คำนวณสรุปและคำเตือนใหม่</summary>
        public void RefreshSummary()
        {
            try
            {
                var articleIds = GetSourceArticleIds();
                int? dest = _view.DestinationCategoryId;

                if (articleIds.Count == 0 || !dest.HasValue)
                {
                    _view.ShowSummary(
                        articleIds.Count == 0 ? "ยังไม่ได้เลือกบทความต้นทาง" : "เลือก " + articleIds.Count + " บทความ — ยังไม่ได้เลือกหมวดปลายทาง",
                        new List<string>(), false);
                    return;
                }

                var preview = _service.PreviewMove(articleIds, dest.Value);
                string path = CMS.Domain.Models.KBCategoryTree.GetPath(_categories, dest.Value, " › ");
                string text = string.Format("จะย้าย {0} บทความ ไปยัง \"{1}\"", preview.ToMoveCount, path);
                if (preview.AlreadyInDestinationCount > 0)
                    text += string.Format(" (ข้าม {0} บทความที่อยู่หมวดนี้แล้ว)", preview.AlreadyInDestinationCount);

                _view.ShowSummary(text, preview.Warnings, preview.ToMoveCount > 0);
            }
            catch (Exception ex)
            {
                _view.ShowSummary(ex.Message, new List<string>(), false);
            }
        }

        public void OnMoveClicked()
        {
            try
            {
                var articleIds = GetSourceArticleIds();
                int? dest = _view.DestinationCategoryId;
                if (articleIds.Count == 0 || !dest.HasValue) return;

                var preview = _service.PreviewMove(articleIds, dest.Value);
                string path = CMS.Domain.Models.KBCategoryTree.GetPath(_categories, dest.Value, " › ");
                string msg = string.Format("ยืนยันย้าย {0} บทความ ไปยัง \"{1}\" ?", preview.ToMoveCount, path);
                if (preview.Warnings.Count > 0)
                    msg += "\n\nข้อควรระวัง:\n- " + string.Join("\n- ", preview.Warnings);
                if (!_view.ConfirmMove(msg)) return;

                var result = _service.Move(articleIds, dest.Value, _user);
                _view.ShowSuccess(string.Format("ย้ายแล้ว {0} บทความ (ข้าม {1})", result.MovedCount, result.SkippedCount));
                RefreshSource();   // โหลดรายการใหม่ — บทความที่ย้ายแล้วหายจากหมวดต้นทาง
            }
            catch (Exception ex)
            {
                _view.ShowError("ย้ายบทความไม่สำเร็จ: " + ex.Message);
            }
        }

        private IList<int> GetSourceArticleIds()
        {
            if (_view.SourceWholeCategory)
                return _candidates.Select(a => a.ArticleID).ToList();

            var checkedIds = new HashSet<int>(_view.CheckedArticleIds ?? new List<int>());
            return _candidates.Where(a => checkedIds.Contains(a.ArticleID)).Select(a => a.ArticleID).ToList();
        }
    }
}
