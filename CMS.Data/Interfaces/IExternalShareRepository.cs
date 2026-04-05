using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IExternalShareRepository
    {
        int Insert(ExternalShare externalShare);
        ExternalShare GetById(int externalShareId);
        ExternalShare GetByToken(Guid shareToken);
        IEnumerable<ExternalShare> GetBySharer(int sharerUserId);
        IEnumerable<ExternalShare> GetByStatus(string status);
        void Update(ExternalShare externalShare);
        IEnumerable<ExternalShare> GetAll();
    }
}
