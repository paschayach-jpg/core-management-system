using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IKBArticleLinkRepository
    {
        IEnumerable<KBArticleLink> GetByArticle(int articleId);
        int Insert(KBArticleLink link);
        void Delete(int linkId);
    }
}
