using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Data.Interfaces
{
    public interface IKmsRepository
    {
        DataTable GetLawTypes();
        DataTable GetCategoriesByParent(int parentCategoryId);
        DataTable GetCategories();
        DataTable SearchArticles(string lawType = "", string categoryCode = "", string keyword = "", string status = "");
        DataTable GetArticleById(int articleId);
    }
}
