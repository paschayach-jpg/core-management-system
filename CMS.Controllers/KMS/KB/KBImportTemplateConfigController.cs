using System;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.KMS.KB
{
    public class KBImportTemplateConfigController
    {
        private readonly IKBImportTemplateConfigController _view;
        private readonly IKBImportTemplateService           _service;
        private readonly IExcelTemplatePreviewService        _excelPreviewService;
        private readonly UserContext                        _user;

        public KBImportTemplateConfigController(
            IKBImportTemplateConfigController view,
            IKBImportTemplateService           service,
            IExcelTemplatePreviewService       excelPreviewService,
            UserContext                        user)
        {
            if (view                == null) throw new ArgumentNullException("view");
            if (service              == null) throw new ArgumentNullException("service");
            if (excelPreviewService  == null) throw new ArgumentNullException("excelPreviewService");
            if (user                 == null) throw new ArgumentNullException("user");
            _view                = view;
            _service             = service;
            _excelPreviewService = excelPreviewService;
            _user                = user;
        }

        public void Initialize()
        {
            LoadTemplates();
            LoadDropdownOptions();
            // แสดง default grid (คอลัมน์เป้าหมายทั้งหมดจาก KMS.t_KBArticle) ทันทีที่เปิดฟอร์ม
            // โดยไม่ต้องรอให้ admin กดปุ่ม "Template ใหม่" ก่อน
            OnNewTemplate();
        }

        private void LoadDropdownOptions()
        {
            try
            {
                _view.BindCategoryOptions(_service.GetCategoryOptions());
                _view.BindArticleTypeOptions(_service.GetArticleTypeOptions());
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลดตัวเลือก Category/ArticleType ล้มเหลว: " + ex.Message);
            }
        }

        public void LoadTemplates()
        {
            try { _view.BindTemplateList(_service.GetAllTemplates()); }
            catch (Exception ex) { _view.ShowError("โหลดรายการ Template ล้มเหลว: " + ex.Message); }
        }

        public void OnTemplateSelected(KBImportTemplate template)
        {
            if (template == null) return;
            try
            {
                _view.EditingTemplateId    = template.TemplateID;
                _view.TemplateName         = template.TemplateName;
                _view.TemplateDescription  = template.Description;
                _view.DeleteEnabled        = template.IsActive;
                _view.BindColumnGrid(_service.GetTemplateColumnGrid(template.TemplateID));
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลดข้อมูล Template ล้มเหลว: " + ex.Message);
            }
        }

        public void OnNewTemplate()
        {
            try
            {
                _view.ClearForm();
                _view.BindColumnGrid(_service.GetTemplateColumnGrid(null));
            }
            catch (Exception ex)
            {
                _view.ShowError("เตรียม Template ใหม่ล้มเหลว: " + ex.Message);
            }
        }

        public void Save()
        {
            try
            {
                var template = new KBImportTemplate
                {
                    TemplateID   = _view.EditingTemplateId ?? 0,
                    TemplateName = _view.TemplateName,
                    Description  = _view.TemplateDescription,
                    SortOrder    = 0,
                };
                var rows = _view.ReadColumnGridEdits();

                _service.SaveTemplate(template, rows, _user);
                _view.ShowSuccess("บันทึก Template สำเร็จ");
                LoadTemplates();
            }
            catch (Exception ex)
            {
                _view.ShowError("บันทึกล้มเหลว: " + ex.Message);
            }
        }

        public void OnExcelFileSelected(string filePath)
        {
            try
            {
                var columns = _excelPreviewService.ReadColumnPreview(filePath);
                if (columns.Count == 0)
                    _view.ShowError("ไม่พบข้อมูลแถวหัวตารางในไฟล์ Excel นี้");
                _view.BindExcelPreview(columns);
            }
            catch (Exception ex)
            {
                _view.ShowError("อ่านไฟล์ Excel ไม่สำเร็จ: " + ex.Message);
            }
        }

        public void Delete()
        {
            if (!_view.EditingTemplateId.HasValue) return;
            try
            {
                _service.DeleteTemplate(_view.EditingTemplateId.Value, _user);
                _view.ShowSuccess("ปิดใช้งาน Template สำเร็จ");
                _view.ClearForm();
                LoadTemplates();
            }
            catch (Exception ex)
            {
                _view.ShowError("ปิดใช้งานล้มเหลว: " + ex.Message);
            }
        }
    }
}
