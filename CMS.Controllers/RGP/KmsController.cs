using System.Data;
using CMS.Data.Interfaces;

namespace CMS.Controllers.RGP
{
    /// <summary>Thin facade เหนือ IKmsRepository (CMS_DB.KMS.*) สำหรับโมดูล Project</summary>
    public sealed class KmsController
    {
        private readonly IKmsRepository _repo;

        public KmsController() : this(RgpComposition.KmsRepository) { }

        public KmsController(IKmsRepository repo)
        {
            _repo = repo;
        }

        public DataTable GetLawTypes() => _repo.GetLawTypes();

        public DataTable GetCategoriesByParent(int parentCategoryId) => _repo.GetCategoriesByParent(parentCategoryId);

        public DataTable GetCategories() => _repo.GetCategories();

        public DataTable SearchArticles(string lawType = "", string categoryCode = "", string keyword = "", string status = "") =>
            _repo.SearchArticles(lawType, categoryCode, keyword, status);

        public DataTable GetArticleById(int articleId) => _repo.GetArticleById(articleId);
    }
}
