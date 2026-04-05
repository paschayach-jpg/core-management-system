using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IHrPositionRepository
    {
        IEnumerable<HrPosition> GetAll();
    }
}
