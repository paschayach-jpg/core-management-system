using System.Data;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.RGP
{
    /// <summary>
    /// Thin facade เหนือ IProjectService สำหรับ Presentation ของโมดูล Project (RGP)
    /// เทียบเท่า KBController ของโมดูล DMS/KMS
    /// </summary>
    public sealed class ProjectController
    {
        private readonly IProjectService _projectService;

        /// <summary>ใช้เมื่อไม่ได้รับ dependency มาจาก composition root — ประกอบเองแบบ lazy-singleton</summary>
        public ProjectController() : this(RgpComposition.ProjectService) { }

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        public string GenerateProjectNo() => _projectService.GenerateProjectNo();

        public bool IsProjectNoExists(string c_project_no) => _projectService.IsProjectNoExists(c_project_no);

        public int CreateProject(ProjectModel model) => _projectService.CreateProject(model);

        public bool UpdateProject(ProjectModel model) => _projectService.UpdateProject(model);

        public bool UpdateLegalDictJson(int i_project_id, string json, string i_user) =>
            _projectService.UpdateLegalDictJson(i_project_id, json, i_user);

        public bool DeleteProject(int i_project_id, string i_user, out string errorMessage) =>
            _projectService.DeleteProject(i_project_id, i_user, out errorMessage);

        public bool CloseProject(int i_project_id, string i_user, out string errorMessage) =>
            _projectService.CloseProject(i_project_id, i_user, out errorMessage);

        public DataTable SearchProjects(string c_project_no, string n_project_name,
            string c_status, string i_template_id, string d_from, string d_to) =>
            _projectService.SearchProjects(c_project_no, n_project_name, c_status, i_template_id, d_from, d_to);

        public DataTable GetTemplates() => _projectService.GetTemplates();

        public DataTable GetProjectById(int i_project_id) => _projectService.GetProjectById(i_project_id);
    }
}
