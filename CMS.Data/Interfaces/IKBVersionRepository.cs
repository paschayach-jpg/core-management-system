using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IKBVersionRepository
    {
        IEnumerable<KBVersion> GetByArticle(int articleId);
        KBVersion GetById(int versionId);
        KBVersion GetLatestByArticle(int articleId);
        int Insert(KBVersion version);
        int GetNextVersionNumber(int articleId);
    }
}
