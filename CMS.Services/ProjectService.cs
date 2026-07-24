using System;
using System.Data;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IProjectStateRepository _stateRepo;
        private readonly IProjectDocumentRepository _docRepo;
        private readonly IProjectTemplateRepository _templateRepo;
        private readonly IProjectAuditService _auditService;

        public ProjectService(
            IProjectRepository projectRepo,
            IProjectStateRepository stateRepo,
            IProjectDocumentRepository docRepo,
            IProjectTemplateRepository templateRepo,
            IProjectAuditService auditService)
        {
            _projectRepo = projectRepo;
            _stateRepo = stateRepo;
            _docRepo = docRepo;
            _templateRepo = templateRepo;
            _auditService = auditService;
        }

        /// <summary>
        /// สร้าง Running Number จากตาราง RunningNumber WHERE running_type = 'PRJ'
        /// รองรับ format เช่น PPP-YY-###### และ reset counter เมื่อขึ้นปีใหม่
        /// SQL/transaction ย้ายไปอยู่ที่ ProjectRepository.GenerateProjectNo() แล้ว — ไม่ throw MessageBox ในชั้นนี้อีกต่อไป
        /// </summary>
        public string GenerateProjectNo()
        {
            return _projectRepo.GenerateProjectNo();
        }

        /// <summary>
        /// ตรวจสอบว่า Project No. มีในระบบแล้วหรือไม่
        /// </summary>
        public bool IsProjectNoExists(string c_project_no)
        {
            return _projectRepo.IsProjectNoExists(c_project_no);
        }

        /// <summary>
        /// สร้าง Project ใหม่: Insert → Copy States จาก Template
        /// ถ้า model.c_project_no ว่าง จะ Auto Generate ให้อัตโนมัติ
        /// คืนค่า i_project_id ใหม่ หรือ -1 ถ้า fail
        /// </summary>
        public int CreateProject(ProjectModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.c_project_no))
                {
                    model.c_project_no = GenerateProjectNo();
                    if (string.IsNullOrEmpty(model.c_project_no))
                        return -1;
                }

                model.c_status = "Open";
                int newId = _projectRepo.InsertProject(model);

                if (newId > 0 && model.i_template_id.HasValue)
                {
                    // 1. Copy States จาก template → t_project_state
                    _stateRepo.InsertStatesFromTemplate(newId, model.i_template_id.Value, model.i_create_user);

                    // 2. Copy Document Workflow จาก m_template_doc → t_project_doc_workflow (auto-populate)
                    _docRepo.InsertDocWorkflowFromTemplate(newId, model.i_create_user);
                }

                _auditService.Log(new ProjectAuditEntry
                {
                    UserID = model.i_create_user,
                    Module = "Project",
                    Action = newId > 0 ? "CREATE_PROJECT" : "CREATE_PROJECT_FAILED",
                    TableName = "t_project",
                    RecordID = newId > 0 ? newId : (int?)null,
                    Result = newId > 0 ? 1 : 2,
                });

                return newId;
            }
            catch (Exception ex)
            {
                _auditService.Log(new ProjectAuditEntry
                {
                    UserID = model.i_create_user,
                    Module = "Project",
                    Action = "CREATE_PROJECT_FAILED",
                    TableName = "t_project",
                    Result = 2,
                    Message = ex.Message,
                });
                return -1;
            }
        }

        /// <summary>
        /// Update Project header
        /// </summary>
        public bool UpdateProject(ProjectModel model)
        {
            bool ok = _projectRepo.UpdateProject(model);
            _auditService.Log(new ProjectAuditEntry
            {
                UserID = model.i_update_user,
                Module = "Project",
                Action = ok ? "UPDATE_PROJECT" : "UPDATE_PROJECT_FAILED",
                TableName = "t_project",
                RecordID = model.i_project_id,
                Result = ok ? 1 : 2,
            });
            return ok;
        }

        public bool UpdateLegalDictJson(int i_project_id, string json, string i_user) =>
            _projectRepo.UpdateLegalDictJson(i_project_id, json, i_user);

        /// <summary>
        /// ลบ Project (soft-delete) — blocked ถ้ามี progress แล้ว
        /// </summary>
        public bool DeleteProject(int i_project_id, string i_user, out string errorMessage)
        {
            errorMessage = "";
            if (_projectRepo.HasActiveProgress(i_project_id))
            {
                errorMessage = "ไม่สามารถลบโครงการได้ เนื่องจากมีการอัปเดตความคืบหน้าแล้ว";
                return false;
            }

            bool ok = _projectRepo.DeleteProject(i_project_id, i_user);
            _auditService.Log(new ProjectAuditEntry
            {
                UserID = i_user,
                Module = "Project",
                Action = ok ? "DELETE_PROJECT" : "DELETE_PROJECT_FAILED",
                TableName = "t_project",
                RecordID = i_project_id,
                Result = ok ? 1 : 2,
            });
            return ok;
        }

        /// <summary>
        /// ปิด Project — blocked ถ้า required states ยังไม่ครบ Completed
        /// </summary>
        public bool CloseProject(int i_project_id, string i_user, out string errorMessage)
        {
            errorMessage = "";
            if (!_projectRepo.AllRequiredStatesCompleted(i_project_id))
            {
                errorMessage = "ยังมีขั้นตอนที่จำเป็น (Required) ยังไม่เสร็จสมบูรณ์";
                return false;
            }

            bool ok = _projectRepo.CloseProject(i_project_id, i_user);
            _auditService.Log(new ProjectAuditEntry
            {
                UserID = i_user,
                Module = "Project",
                Action = ok ? "CLOSE_PROJECT" : "CLOSE_PROJECT_FAILED",
                TableName = "t_project",
                RecordID = i_project_id,
                Result = ok ? 1 : 2,
            });
            return ok;
        }

        /// <summary>
        /// ค้นหาโครงการ
        /// </summary>
        public DataTable SearchProjects(string c_project_no, string n_project_name,
            string c_status, string i_template_id, string d_from, string d_to)
        {
            return _projectRepo.SearchProjects(c_project_no, n_project_name,
                c_status, i_template_id, d_from, d_to);
        }

        /// <summary>
        /// ดึง Templates ทั้งหมด (f_active='Y') สำหรับ ComboBox
        /// SQL ย้ายไปอยู่ที่ ProjectTemplateRepository.GetActiveTemplates() แล้ว
        /// </summary>
        public DataTable GetTemplates()
        {
            return _templateRepo.GetActiveTemplates();
        }

        /// <summary>
        /// ดึงข้อมูล Project เดียว
        /// </summary>
        public DataTable GetProjectById(int i_project_id)
        {
            return _projectRepo.GetProjectById(i_project_id);
        }
    }
}
