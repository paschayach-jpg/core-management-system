using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IAuthorityOrgLookupController
    {
        IList<HrDivision> GetDivisions();
        IList<HrDepartment> GetDepartments();
        IList<HrPosition> GetPositions();
        IList<HrPositionLevel> GetPositionLevels();
        IList<HrPositionMapping> GetPositionMappings();
        IList<HrPositionMapping> GetPositionMappingsByDepartmentId(int departmentId);
        IList<string> GetPositionsByDepartmentCode(string departmentCode);
    }
}
