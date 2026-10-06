using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IHrPositionMappingRepository
    {
        IEnumerable<HrPositionMapping> GetAll();
        IEnumerable<HrPositionMapping> GetByDepartmentId(int departmentId);
        IEnumerable<HrPositionMapping> GetByDivisionId(int divisionId);
        int Save(HrPositionMapping mapping);
        bool Delete(int mappingId);
    }
}
