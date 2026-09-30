using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.KMS.KB
{
    public class KBArticleDisplayProfileConfigController
    {
        private readonly IKBArticleDisplayProfileConfigController _view;
        private readonly IKBArticleDisplayProfileService           _service;
        private readonly IKBCategoryService                        _categoryService;
        private readonly UserContext                                _user;

        public KBArticleDisplayProfileConfigController(
            IKBArticleDisplayProfileConfigController view,
            IKBArticleDisplayProfileService           service,
            IKBCategoryService                        categoryService,
            UserContext                                user)
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
            LoadProfiles();
            OnNewProfile();
        }

        public void LoadProfiles()
        {
            try { _view.BindProfileList(_service.GetAllProfiles()); }
            catch (Exception ex) { _view.ShowError("โหลดรายการ Profile ล้มเหลว: " + ex.Message); }
        }

        public void OnProfileSelected(KBArticleDisplayProfile profile)
        {
            if (profile == null) return;
            try
            {
                _view.EditingProfileId   = profile.ProfileID;
                _view.ProfileName        = profile.ProfileName;
                _view.ProfileDescription = profile.Description;
                _view.DeleteEnabled      = true;
                _view.RecordIsActive     = profile.IsActive;
                _view.BindColumnGrid(_service.GetProfileColumnGrid(profile.ProfileID));

                var assignedIds = _service.GetCategoriesUsingProfile(profile.ProfileID).Select(c => c.CategoryID);
                _view.BindCategoryAssignment(_categoryService.GetAllActive(), assignedIds);
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลดข้อมูล Profile ล้มเหลว: " + ex.Message);
            }
        }

        public void OnNewProfile()
        {
            try
            {
                _view.ClearForm();
                _view.BindColumnGrid(_service.GetProfileColumnGrid(null));
                _view.BindCategoryAssignment(_categoryService.GetAllActive(), new List<int>());
            }
            catch (Exception ex)
            {
                _view.ShowError("เตรียม Profile ใหม่ล้มเหลว: " + ex.Message);
            }
        }

        public void Save()
        {
            try
            {
                var profile = new KBArticleDisplayProfile
                {
                    ProfileID   = _view.EditingProfileId ?? 0,
                    ProfileName = _view.ProfileName,
                    Description = _view.ProfileDescription,
                };
                var rows = _view.ReadColumnGridEdits();

                int profileId = _service.SaveProfile(profile, rows, _user);

                // ── Reconcile category assignment: เทียบชุดที่ติ๊กไว้ใน UI กับที่ผูกอยู่จริงใน DB ──
                var currentlyAssigned = new HashSet<int>(
                    _service.GetCategoriesUsingProfile(profileId).Select(c => c.CategoryID));
                var nowAssigned = new HashSet<int>(_view.ReadAssignedCategoryIds());

                foreach (var categoryId in nowAssigned.Except(currentlyAssigned))
                    _service.AssignProfileToCategory(categoryId, profileId, _user);

                foreach (var categoryId in currentlyAssigned.Except(nowAssigned))
                    _service.AssignProfileToCategory(categoryId, null, _user);

                _view.ShowSuccess("บันทึก Profile สำเร็จ");
                LoadProfiles();
            }
            catch (Exception ex)
            {
                _view.ShowError("บันทึกล้มเหลว: " + ex.Message);
            }
        }

        public void Delete()
        {
            if (!_view.EditingProfileId.HasValue) return;
            try
            {
                _service.DeleteProfile(_view.EditingProfileId.Value, _user);
                _view.ShowSuccess("ปิดใช้งาน Profile สำเร็จ");
                _view.ClearForm();
                LoadProfiles();
            }
            catch (Exception ex)
            {
                _view.ShowError("ปิดใช้งานล้มเหลว: " + ex.Message);
            }
        }

        public void Activate()
        {
            if (!_view.EditingProfileId.HasValue) return;
            try
            {
                _service.ActivateProfile(_view.EditingProfileId.Value, _user);
                _view.ShowSuccess("เปิดใช้งาน Profile สำเร็จ");
                _view.ClearForm();
                LoadProfiles();
            }
            catch (Exception ex)
            {
                _view.ShowError("เปิดใช้งานล้มเหลว: " + ex.Message);
            }
        }
    }
}
