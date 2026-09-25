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
        private readonly UserContext                        _user;

        public KBImportTemplateConfigController(
            IKBImportTemplateConfigController view,
            IKBImportTemplateService           service,
            UserContext                        user)
        {
            if (view    == null) throw new ArgumentNullException("view");
            if (service == null) throw new ArgumentNullException("service");
            if (user    == null) throw new ArgumentNullException("user");
            _view    = view;
            _service = service;
            _user    = user;
        }

        public void Initialize() { LoadTemplates(); }

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
                    TemplateID  = _view.EditingTemplateId ?? 0,
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
