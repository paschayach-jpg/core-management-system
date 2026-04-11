using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CMS.Data.Repositories
{
    /// <summary>
    /// KBDashboardRepository - Returns mock data for KMS Dashboard
    /// TODO: Replace mock data with actual database queries when tables are populated
    /// </summary>
    public class KBDashboardRepository : IKBDashboardRepository
    {
        private readonly string _connectionString;

        public KBDashboardRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        /// <summary>
        /// Returns mock dashboard statistics
        /// </summary>
        public KBDashboardStats GetDashboardStats()
        {
            // Mock data for dashboard statistics
            return new KBDashboardStats
            {
                TotalArticles = 156,
                MonthlyViews = 2847,
                AverageRating = 4.3m,
                PublishedArticles = 98,
                DraftArticles = 23,
                ReviewArticles = 12,
                ArchivedArticles = 23,
                ExpiringArticles = 5,
                OverdueReviews = 8,
                PendingApprovals = 12,
                LastUpdated = DateTime.Now
            };

            /* TODO: Replace with actual database query
            var stats = new KBDashboardStats { LastUpdated = DateTime.Now };

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Query 1: Total articles count
                var totalArticlesQuery = @"
                    SELECT COUNT(*) 
                    FROM KMS.t_KBArticle";

                using (var cmd = new SqlCommand(totalArticlesQuery, conn))
                {
                    stats.TotalArticles = (int)cmd.ExecuteScalar();
                }

                // Query 2: Monthly views
                var monthlyViewsQuery = @"
                    SELECT ISNULL(SUM(ViewCount), 0)
                    FROM KMS.t_KBArticle";

                using (var cmd = new SqlCommand(monthlyViewsQuery, conn))
                {
                    stats.MonthlyViews = (int)cmd.ExecuteScalar();
                }

                // Query 3: Average rating
                var avgRatingQuery = @"
                    SELECT ISNULL(AVG(CAST(LikeCount AS DECIMAL(10,2))), 0)
                    FROM KMS.t_KBArticle
                    WHERE Status = 3";

                using (var cmd = new SqlCommand(avgRatingQuery, conn))
                {
                    stats.AverageRating = (decimal)cmd.ExecuteScalar();
                }

                // Query 4: Articles by status
                var statusQuery = @"
                    SELECT Status, COUNT(*) AS Count
                    FROM KMS.t_KBArticle
                    GROUP BY Status";

                using (var cmd = new SqlCommand(statusQuery, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var status = reader.GetByte(0);
                        var count = reader.GetInt32(1);

                        switch (status)
                        {
                            case 1: stats.DraftArticles = count; break;
                            case 2: stats.ReviewArticles = count; break;
                            case 3: stats.PublishedArticles = count; break;
                            case 4: stats.ArchivedArticles = count; break;
                        }
                    }
                }

                // Query 5: Expiring articles
                var expiringQuery = @"
                    SELECT COUNT(*)
                    FROM KMS.t_KBArticle
                    WHERE Status = 3
                      AND ExpiryDate IS NOT NULL
                      AND ExpiryDate <= DATEADD(DAY, 30, GETDATE())
                      AND ExpiryDate > GETDATE()";

                using (var cmd = new SqlCommand(expiringQuery, conn))
                {
                    stats.ExpiringArticles = (int)cmd.ExecuteScalar();
                }

                // Query 6: Overdue reviews
                var overdueReviewsQuery = @"
                    SELECT COUNT(*)
                    FROM KMS.t_KBArticle
                    WHERE Status = 3
                      AND ReviewDueDate IS NOT NULL
                      AND ReviewDueDate < GETDATE()";

                using (var cmd = new SqlCommand(overdueReviewsQuery, conn))
                {
                    stats.OverdueReviews = (int)cmd.ExecuteScalar();
                }

                // Query 7: Pending approvals
                stats.PendingApprovals = stats.ReviewArticles;
            }

            return stats;
            */
        }

        /// <summary>
        /// Returns mock monthly article data
        /// </summary>
        public IEnumerable<KBMonthlyArticle> GetMonthlyArticles(int months)
        {
            // Mock data for monthly article trends
            var list = new List<KBMonthlyArticle>();
            var currentDate = DateTime.Now;

            for (int i = months - 1; i >= 0; i--)
            {
                var date = currentDate.AddMonths(-i);
                var random = new Random(date.Month + date.Year * 12);
                
                list.Add(new KBMonthlyArticle
                {
                    Year = date.Year,
                    Month = date.Month,
                    MonthName = date.ToString("MMMM"),
                    ArticleCount = random.Next(5, 25)
                });
            }

            return list;

            /* TODO: Replace with actual database query
            var list = new List<KBMonthlyArticle>();

            var query = @"
                SELECT 
                    YEAR(CreatedDate) AS Year,
                    MONTH(CreatedDate) AS Month,
                    DATENAME(MONTH, CreatedDate) AS MonthName,
                    COUNT(*) AS ArticleCount
                FROM KMS.t_KBArticle
                WHERE CreatedDate >= DATEADD(MONTH, -@Months, GETDATE())
                GROUP BY YEAR(CreatedDate), MONTH(CreatedDate), DATENAME(MONTH, CreatedDate)
                ORDER BY YEAR(CreatedDate), MONTH(CreatedDate)";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Months", months);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new KBMonthlyArticle
                        {
                            Year = reader.GetInt32(0),
                            Month = reader.GetInt32(1),
                            MonthName = reader.GetString(2),
                            ArticleCount = reader.GetInt32(3)
                        });
                    }
                }
            }

            return list;
            */
        }

        /// <summary>
        /// Returns mock top articles data
        /// </summary>
        public IEnumerable<KBTopArticle> GetTopArticles(int topN)
        {
            // Mock data for top articles
            var list = new List<KBTopArticle>
            {
                new KBTopArticle
                {
                    ArticleID = 1,
                    ArticleCode = "KB-2024-001",
                    Title = "วิธีการ Upload ไฟล์เข้าระบบ DMS",
                    ArticleType = "SOP",
                    Category = "เทคโนโลยีสารสนเทศ",
                    ViewCount = 1245,
                    Rating = 4.8m
                },
                new KBTopArticle
                {
                    ArticleID = 2,
                    ArticleCode = "KB-2024-015",
                    Title = "แนวทางการจัดการสิทธิ์การเข้าถึงเอกสาร",
                    ArticleType = "Guideline",
                    Category = "การจัดการเอกสาร",
                    ViewCount = 987,
                    Rating = 4.6m
                },
                new KBTopArticle
                {
                    ArticleID = 3,
                    ArticleCode = "KB-2024-023",
                    Title = "FAQ: คำถามที่พบบ่อยเกี่ยวกับระบบ CMS",
                    ArticleType = "FAQ",
                    Category = "เทคโนโลยีสารสนเทศ",
                    ViewCount = 856,
                    Rating = 4.5m
                },
                new KBTopArticle
                {
                    ArticleID = 4,
                    ArticleCode = "KB-2024-008",
                    Title = "นโยบายความปลอดภัยข้อมูลองค์กร",
                    ArticleType = "Policy",
                    Category = "นโยบายและระเบียบ",
                    ViewCount = 734,
                    Rating = 4.7m
                },
                new KBTopArticle
                {
                    ArticleID = 5,
                    ArticleCode = "KB-2024-042",
                    Title = "Template: แบบฟอร์มขออนุมัติเอกสาร",
                    ArticleType = "Template",
                    Category = "แบบฟอร์ม",
                    ViewCount = 623,
                    Rating = 4.4m
                },
                new KBTopArticle
                {
                    ArticleID = 6,
                    ArticleCode = "KB-2024-031",
                    Title = "ขั้นตอนการจัดเก็บเอกสารตามระดับความลับ",
                    ArticleType = "SOP",
                    Category = "การจัดการเอกสาร",
                    ViewCount = 589,
                    Rating = 4.3m
                },
                new KBTopArticle
                {
                    ArticleID = 7,
                    ArticleCode = "KB-2024-019",
                    Title = "แนวทางการใช้งาน Recycle Bin",
                    ArticleType = "Guideline",
                    Category = "เทคโนโลยีสารสนเทศ",
                    ViewCount = 512,
                    Rating = 4.2m
                },
                new KBTopArticle
                {
                    ArticleID = 8,
                    ArticleCode = "KB-2024-055",
                    Title = "การตั้งค่า Notification และการแจ้งเตือน",
                    ArticleType = "SOP",
                    Category = "เทคโนโลยีสารสนเทศ",
                    ViewCount = 478,
                    Rating = 4.1m
                },
                new KBTopArticle
                {
                    ArticleID = 9,
                    ArticleCode = "KB-2024-067",
                    Title = "FAQ: การแชร์ไฟล์ภายในและภายนอกองค์กร",
                    ArticleType = "FAQ",
                    Category = "การจัดการเอกสาร",
                    ViewCount = 445,
                    Rating = 4.5m
                },
                new KBTopArticle
                {
                    ArticleID = 10,
                    ArticleCode = "KB-2024-073",
                    Title = "นโยบาย PDPA และการคุ้มครองข้อมูลส่วนบุคคล",
                    ArticleType = "Policy",
                    Category = "นโยบายและระเบียบ",
                    ViewCount = 421,
                    Rating = 4.9m
                }
            };

            // Return only the requested number of articles
            return list.GetRange(0, Math.Min(topN, list.Count));

            /* TODO: Replace with actual database query
            var list = new List<KBTopArticle>();

            var query = @"
                SELECT TOP (@TopN)
                    a.ArticleID,
                    a.ArticleCode,
                    a.Title,
                    CASE a.ArticleType
                        WHEN 1 THEN 'SOP'
                        WHEN 2 THEN 'FAQ'
                        WHEN 3 THEN 'Guideline'
                        WHEN 4 THEN 'Template'
                        WHEN 5 THEN 'Policy'
                        ELSE 'Unknown'
                    END AS ArticleType,
                    ISNULL(c.CategoryName, '-') AS Category,
                    a.ViewCount,
                    CAST(a.LikeCount AS DECIMAL(10,2)) AS Rating
                FROM KMS.t_KBArticle a
                LEFT JOIN KMS.m_KBCategory c ON a.CategoryID = c.CategoryID
                WHERE a.Status = 3
                ORDER BY a.ViewCount DESC, a.LikeCount DESC";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@TopN", topN);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new KBTopArticle
                        {
                            ArticleID = reader.GetInt32(0),
                            ArticleCode = reader.GetString(1),
                            Title = reader.GetString(2),
                            ArticleType = reader.GetString(3),
                            Category = reader.GetString(4),
                            ViewCount = reader.GetInt32(5),
                            Rating = reader.GetDecimal(6)
                        });
                    }
                }
            }

            return list;
            */
        }
    }
}
