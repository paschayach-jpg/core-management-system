using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Controllers.KMS.KB
{
    public class KBCategoryController
    {
        private readonly IKBCategoryController _view;
        private readonly IKBCategoryService    _service;
        private readonly IKBArticleService     _articleService;
        private readonly UserContext           _user;

        public KBCategoryController(
            IKBCategoryController view,
            IKBCategoryService    service,
            IKBArticleService     articleService,
            UserContext           user)
        {
            if (view           == null) throw new ArgumentNullException("view");
            if (service        == null) throw new ArgumentNullException("service");
            if (articleService == null) throw new ArgumentNullException("articleService");
            if (user           == null) throw new ArgumentNullException("user");
            _view           = view;
            _service        = service;
            _articleService = articleService;
            _user           = user;
        }

        public void Initialize() { LoadCategories(); }

        public void LoadCategories()
        {
            try { _view.BindCategories(_service.GetAll()); }
            catch (Exception ex) { _view.ShowError("Load categories failed: " + ex.Message); }
        }

        public void OnCategorySelected(KBCategory cat)
        {
            if (cat == null) return;
            _view.EditingId         = cat.CategoryID;
            _view.FormTitleText     = "แก้ไข Category";
            _view.CategoryCode      = cat.CategoryCode;
            _view.CategoryName      = cat.CategoryName;
            _view.Icon              = cat.Icon;
            _view.Description       = cat.Description;
            _view.SortOrder         = cat.SortOrder;
            _view.ParentCategoryId  = cat.ParentCategoryID;
            _view.DeleteEnabled     = cat.IsActive;
            _view.DeleteButtonText  = cat.IsActive ? "🗑️ ปิดใช้งาน" : "⛔ ปิดแล้ว";
            _view.BindCategoryDetails(cat);

            try
            {
                // The selected category + every subcategory under it, at any depth (กฎหมาย › PLI › PLI2535)
                var articles = _articleService.GetByCategory(cat.CategoryID).ToList();

                foreach (int childId in KBCategoryTree.GetDescendantIds(_service.GetAll(), cat.CategoryID))
                    articles.AddRange(_articleService.GetByCategory(childId));

                int total     = articles.Count;
                int published = articles.Count(a => a.Status == 3);                   // 3 = Published
                int draft     = articles.Count(a => a.Status == 1 || a.Status == 2);  // 1 = Draft, 2 = Review
                _view.BindCategoryStats(total, published, draft);

                // Show the 5 most recent articles (same pool as total count)
                var recent = articles.OrderByDescending(a => a.CreatedDate).Take(5);
                _view.BindArticleUsage(recent);
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลดข้อมูลบทความล้มเหลว: " + ex.Message);
            }
        }

        public void Save()
        {
            try
            {
                if (!_view.EditingId.HasValue)
                {
                    var cat = new KBCategory
                    {
                        ParentCategoryID = _view.ParentCategoryId,
                        CategoryCode     = _view.CategoryCode,
                        CategoryName     = _view.CategoryName,
                        Icon             = _view.Icon,
                        Description      = _view.Description,
                        SortOrder        = _view.SortOrder,
                        IsActive         = true,
                        CreatedBy        = _user.AdIdentity,
                    };
                    _service.Create(cat);
                    _view.ShowSuccess("เพิ่ม Category สำเร็จ");
                }
                else
                {
                    var cat = new KBCategory
                    {
                        CategoryID       = _view.EditingId.Value,
                        ParentCategoryID = _view.ParentCategoryId,
                        CategoryCode     = _view.CategoryCode,
                        CategoryName     = _view.CategoryName,
                        Icon             = _view.Icon,
                        Description      = _view.Description,
                        SortOrder        = _view.SortOrder,
                    };
                    _service.Update(cat);
                    _view.ShowSuccess("แก้ไข Category สำเร็จ");
                }
                LoadCategories();
                _view.ClearForm();
            }
            catch (Exception ex) { _view.ShowError("Save failed: " + ex.Message); }
        }

        public void Deactivate()
        {
            if (!_view.EditingId.HasValue) return;
            try
            {
                _service.Deactivate(_view.EditingId.Value);
                _view.ShowSuccess("ปิดใช้งาน Category สำเร็จ");
                LoadCategories();
                _view.ClearForm();
            }
            catch (Exception ex) { _view.ShowError("Deactivate failed: " + ex.Message); }
        }
    }
}
