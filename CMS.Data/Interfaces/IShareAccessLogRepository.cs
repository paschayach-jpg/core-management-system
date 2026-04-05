using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IShareAccessLogRepository
    {
        long Insert(ShareAccessLog accessLog);
        IEnumerable<ShareAccessLog> GetByExternalShare(int externalShareId);
        IEnumerable<ShareAccessLog> GetByToken(Guid shareToken);
        IEnumerable<ShareAccessLog> GetByDateRange(DateTime startDate, DateTime endDate);
    }
}
