using System.Data;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IProjectService
    {
        string GenerateProjectNo();
        bool IsProjectNoExists(string c_project_no);
        int CreateProject(ProjectModel model);
        bool UpdateProject(ProjectModel model);
        bool UpdateLegalDictJson(int i_project_id, string json, string i_user);
        bool DeleteProject(int i_project_id, string i_user, out string errorMessage);
        bool CloseProject(int i_project_id, string i_user, out string errorMessage);
        DataTable SearchProjects(string c_project_no, string n_project_name, string c_status, string i_template_id, string d_from, string d_to);
        DataTable GetTemplates();
        DataTable GetProjectById(int i_project_id);
    }
}
