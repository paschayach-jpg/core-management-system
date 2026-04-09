using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IKBVersionRepository
    {
        IEnumerable<KBVersion> GetByArticle(int articleId);
        KBVersion GetById(int versionId);
        int Insert(KBVersion version);
        int GetNextVersionNumber(int articleId);
    }
}
