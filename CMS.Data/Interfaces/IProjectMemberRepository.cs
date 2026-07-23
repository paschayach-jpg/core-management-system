using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Data.Interfaces
{
    public interface IProjectMemberRepository
    {
        DataTable GetByProject(int i_project_id);
        bool Insert(ProjectMemberModel m);
        bool Delete(int i_member_id);
    }
}
