using RG_Project.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RG_Project.Project.Controllers
{
    /// <summary>
    /// DAL สำหรับ KMS.m_KBCategory และ KMS.t_KBArticle ใน CMS_DB
    /// </summary>
    public class KmsController
    {
        /// <summary>ดึงประเภทกฎหมาย (ParentCategoryID IS NULL)</summary>
        public DataTable GetLawTypes()
        {
            var dt = new DataTable();
            var conn = new ConectionDB();
            conn.Connect('n');
            using (var cmd = new SqlCommand(
                @"SELECT CategoryID, CategoryCode, CategoryName
                  FROM CMS_DB.KMS.m_KBCategory
                  WHERE ParentCategoryID IS NULL AND IsActive = 1
                  ORDER BY SortOrder, CategoryCode",
                conn.connection))
            using (var r = cmd.ExecuteReader())
                dt.Load(r);
            conn.Disconnect();
            return dt;
        }

        /// <summary>ดึงหมวดกฎหมายตาม ParentCategoryID</summary>
        public DataTable GetCategoriesByParent(int parentCategoryId)
        {
            var dt = new DataTable();
            var conn = new ConectionDB();
            conn.Connect('n');
            using (var cmd = new SqlCommand(
                @"SELECT CategoryID, CategoryCode, CategoryName
                  FROM CMS_DB.KMS.m_KBCategory
                  WHERE ParentCategoryID = @parentId AND IsActive = 1
                  ORDER BY SortOrder, CategoryCode",
                conn.connection))
            {
                cmd.Parameters.AddWithValue("@parentId", parentCategoryId);
                using (var r = cmd.ExecuteReader()) dt.Load(r);
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>ดึงหมวดกฎหมายทั้งหมด (legacy)</summary>
        public DataTable GetCategories()
        {
            var dt = new DataTable();
            var conn = new ConectionDB();
            conn.Connect('n');
            using (var cmd = new SqlCommand(
                @"SELECT CategoryID, CategoryCode, CategoryName, LawType
                  FROM CMS_DB.KMS.m_KBCategory
                  ORDER BY LawType, CategoryCode",
                conn.connection))
            using (var r = cmd.ExecuteReader())
                dt.Load(r);
            conn.Disconnect();
            return dt;
        }

        /// <summary>ค้นหาบทความ — กรองตาม type, category, keyword, สถานะ</summary>
        public DataTable SearchArticles(string lawType = "", string categoryCode = "",
                                        string keyword = "", string status = "")
        {
            var dt = new DataTable();
            var conn = new ConectionDB();
            conn.Connect('n');
            string sql = @"
                SELECT a.ArticleID, a.ArticleCode,
                       c.CategoryCode, c.CategoryName, c.LawType,
                       a.Title, a.Section, a.ArticleType, a.Status,
                       a.ConfidentialityLevel, a.PublishedDate, a.ExpiryDate,
                       a.Tags, a.CreatedBy
                FROM CMS_DB.KMS.t_KBArticle a
                JOIN CMS_DB.KMS.m_KBCategory c ON c.CategoryID = a.CategoryID
                WHERE 1=1
                  AND (@lawType    = '' OR c.LawType      = @lawType)
                  AND (@catCode    = '' OR c.CategoryCode  = @catCode)
                  AND (@status     = '' OR a.Status        = @status)
                  AND (@kw         = '' OR a.ArticleCode LIKE '%'+@kw+'%'
                                       OR a.Title        LIKE '%'+@kw+'%'
                                       OR a.Section      LIKE '%'+@kw+'%'
                                       OR a.Tags         LIKE '%'+@kw+'%')
                ORDER BY c.LawType, c.CategoryCode, a.ArticleCode";
            using (var cmd = new SqlCommand(sql, conn.connection))
            {
                cmd.Parameters.AddWithValue("@lawType", lawType ?? "");
                cmd.Parameters.AddWithValue("@catCode",  categoryCode ?? "");
                cmd.Parameters.AddWithValue("@status",   status ?? "");
                cmd.Parameters.AddWithValue("@kw",       keyword ?? "");
                using (var r = cmd.ExecuteReader()) dt.Load(r);
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>ดึงรายละเอียดบทความเต็ม</summary>
        public DataTable GetArticleById(int articleId)
        {
            var dt = new DataTable();
            var conn = new ConectionDB();
            conn.Connect('n');
            using (var cmd = new SqlCommand(
                @"SELECT a.*, c.CategoryCode, c.CategoryName, c.LawType
                  FROM CMS_DB.KMS.t_KBArticle a
                  JOIN CMS_DB.KMS.m_KBCategory c ON c.CategoryID = a.CategoryID
                  WHERE a.ArticleID = @id",
                conn.connection))
            {
                cmd.Parameters.AddWithValue("@id", articleId);
                using (var r = cmd.ExecuteReader()) dt.Load(r);
            }
            conn.Disconnect();
            return dt;
        }
    }
}
