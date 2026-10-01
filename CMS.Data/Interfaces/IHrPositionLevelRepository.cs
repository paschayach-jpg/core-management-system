using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IHrPositionLevelRepository
    {
        IEnumerable<HrPositionLevel> GetAll();
        bool Save(HrPositionLevel level);
        bool Delete(string levelCode);
    }
}
