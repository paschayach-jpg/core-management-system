using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IHrDepartmentRepository
    {
        IEnumerable<HrDepartment> GetAll();
        IEnumerable<HrDepartment> GetByDivisionID(int divisionID);
    }
}
