using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IHrDivisionRepository
    {
        IEnumerable<HrDivision> GetAll();
        int Save(HrDivision division);
        bool Delete(int divisionId);
    }
}
