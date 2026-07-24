using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Data.Interfaces
{
    public interface IProjectRepository
    {
        string GenerateProjectNo();
        DataTable SearchProjects(string c_project_no, string n_project_name, string c_status, string i_template_id, string d_from, string d_to);
        DataTable GetProjectById(int i_project_id);
        int InsertProject(ProjectModel model);
        bool UpdateProject(ProjectModel model);
        bool UpdateLegalDictJson(int i_project_id, string json, string i_user);
        bool DeleteProject(int i_project_id, string i_user);
        bool CloseProject(int i_project_id, string i_user);
        bool UpdateProjectStatus(int i_project_id, string c_status, string i_user);
        bool HasActiveProgress(int i_project_id);
        bool IsProjectNoExists(string c_project_no);
        bool AllRequiredStatesCompleted(int i_project_id);
    }
}
