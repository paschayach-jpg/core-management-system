using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IHrPositionMappingRepository
    {
        /// <summary>ดึงทุก mapping พร้อม join ชื่อ Position/Department/Division</summary>
        IEnumerable<HrPositionMapping> GetAll();

        /// <summary>ดึง Position ที่สังกัด Department นั้น (ตาม DepartmentID)</summary>
        IEnumerable<HrPositionMapping> GetByDepartmentId(int departmentId);

        /// <summary>ดึง Position ที่สังกัด Division นั้น (ตาม DivisionID)</summary>
        IEnumerable<HrPositionMapping> GetByDivisionId(int divisionId);
    }
}
