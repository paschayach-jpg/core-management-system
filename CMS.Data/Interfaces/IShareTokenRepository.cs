using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IShareTokenRepository
    {
        int Insert(ShareToken shareToken);
        ShareToken GetById(int shareTokenId);
        ShareToken GetByTokenValue(Guid tokenValue);
        void Update(ShareToken shareToken);
        IEnumerable<ShareToken> GetByExternalShare(int externalShareId);
    }
}
