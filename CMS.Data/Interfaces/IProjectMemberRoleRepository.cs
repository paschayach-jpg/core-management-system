using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Data.Interfaces
{
    public interface IProjectMemberRoleRepository
    {
        DataTable GetAll();
        DataTable GetActive();
        bool Insert(string roleName, string userName);
        bool Update(int roleId, string roleName, string fActive, string userName);
        bool Delete(int roleId);
        DataTable GetSubCommittees();
        DataTable GetDivisions();
        DataTable GetDepartments();
    }
}
