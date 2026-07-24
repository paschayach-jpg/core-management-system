using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Data.Interfaces
{
    public interface IProjectStateMemberRepository
    {
        DataTable GetByState(int i_proj_state_id);
        bool Insert(ProjectStateMemberModel m);
        bool Delete(int i_state_member_id);
    }
}
