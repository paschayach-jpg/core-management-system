using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IShareInboxReadRepository
    {
        IList<ShareInboxReadKey> GetReadKeys(int userId);
        void MarkRead(int userId, byte shareKind, int permissionId);
        void MarkReadMany(int userId, IEnumerable<ShareInboxReadKey> keys);
    }
}
