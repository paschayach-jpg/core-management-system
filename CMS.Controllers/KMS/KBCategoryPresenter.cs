using CMS.Controllers.KBS.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
using System;
using System.Linq;

namespace CMS.Controllers.KBS
{
    public class KBCategoryPresenter
    {
        private readonly IKBCategoryView    _view;
        private readonly IKBCategoryService _service;
        private readonly UserContext        _user;

        public KBCategoryPresenter(IKBCategoryView view, IKBCategoryService service, UserContext user)
        {
            if (view == null) throw new ArgumentNullException("view");
            if (service == null) throw new ArgumentNullException("service");
            if (user == null) throw new ArgumentNullException("user");
            _view = view; _service = service; _user = user;
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
            _view.EditingId = cat.CategoryID;
            _view.FormTitleText = "Edit Category";
            _view.CategoryCode = cat.CategoryCode;
            _view.CategoryName = cat.CategoryName;
            _view.Icon = cat.Icon;
            _view.Description = cat.Description;
            _view.SortOrder = cat.SortOrder;
            _view.ParentCategoryId = cat.ParentCategoryID;
            _view.DeleteEnabled = cat.IsActive;
            _view.DeleteButtonText = cat.IsActive ? "Deactivate" : "Already Inactive";
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
                        CategoryCode = _view.CategoryCode,
                        CategoryName = _view.CategoryName,
                        Icon = _view.Icon,
                        Description = _view.Description,
                        SortOrder = _view.SortOrder,
                        IsActive = true,
                        CreatedBy = _user.AdIdentity,
                    };
                    _service.Create(cat);
                    _view.ShowSuccess("Category added successfully");
                }
                else
                {
                    var cat = new KBCategory
                    {
                        CategoryID = _view.EditingId.Value,
                        ParentCategoryID = _view.ParentCategoryId,
                        CategoryCode = _view.CategoryCode,
                        CategoryName = _view.CategoryName,
                        Icon = _view.Icon,
                        Description = _view.Description,
                        SortOrder = _view.SortOrder,
                    };
                    _service.Update(cat);
                    _view.ShowSuccess("Category updated successfully");
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
                _view.ShowSuccess("Category deactivated");
                LoadCategories();
                _view.ClearForm();
            }
            catch (Exception ex) { _view.ShowError("Deactivate failed: " + ex.Message); }
        }
    }
}