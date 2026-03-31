using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IUserOrgVisibilityRepository
    {
        IEnumerable<UserOrgVisibility> GetByUser(int userId);
        IEnumerable<UserOrgVisibility> GetByOrgUnit(int orgUnitId);
        void Grant(UserOrgVisibility record);   // upsert — no duplicate key exception
        void Revoke(int userId, int orgUnitId); // silent no-op if row not found
    }
}
