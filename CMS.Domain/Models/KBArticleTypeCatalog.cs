using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>
    /// รายการค่า ArticleType คงที่ของ KMS.t_KBArticle (1-5) พร้อมคำอธิบายภาษาอังกฤษ
    /// สำหรับ dropdown เลือกค่าในหน้า config Excel Import Template — ค่าตัวเลขต้องตรงกับ
    /// KBArticle.ArticleType (ดูคำอธิบายภาษาไทยที่ KBArticle.ArticleTypeName)
    /// </summary>
    public static class KBArticleTypeCatalog
    {
        public static IList<KBArticleTypeOption> GetOptions()
        {
            return new List<KBArticleTypeOption>
            {
                new KBArticleTypeOption { ArticleType = 1, Description = "SOP" },
                new KBArticleTypeOption { ArticleType = 2, Description = "FAQ" },
                new KBArticleTypeOption { ArticleType = 3, Description = "Guideline" },
                new KBArticleTypeOption { ArticleType = 4, Description = "Template" },
                new KBArticleTypeOption { ArticleType = 5, Description = "Policy (กฎหมาย/บทความ)" },
            };
        }
    }
}
