using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CMS.Data.Repositories
{
    public class KBDashboardRepository : IKBDashboardRepository
    {
        private readonly string _connectionString;

        public KBDashboardRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public KBDashboardStats GetDashboardStats()
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("sp_KB_GetDashboardStats", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new KBDashboardStats
                        {
                            TotalArticles = reader.GetInt32(reader.GetOrdinal("TotalArticles")),
                            MonthlyViews = reader.GetInt32(reader.GetOrdinal("MonthlyViews")),
                            AverageRating = reader.GetDecimal(reader.GetOrdinal("AverageRating")),
                            PublishedArticles = reader.GetInt32(reader.GetOrdinal("PublishedArticles")),
                            DraftArticles = reader.GetInt32(reader.GetOrdinal("DraftArticles")),
                            ReviewArticles = reader.GetInt32(reader.GetOrdinal("ReviewArticles")),
                            ArchivedArticles = reader.GetInt32(reader.GetOrdinal("ArchivedArticles")),
                            ExpiringArticles = reader.GetInt32(reader.GetOrdinal("ExpiringArticles")),
                            OverdueReviews = reader.GetInt32(reader.GetOrdinal("OverdueReviews")),
                            PendingApprovals = reader.GetInt32(reader.GetOrdinal("PendingApprovals")),
                            LastUpdated = DateTime.Now
                        };
                    }
                }
            }
            return new KBDashboardStats { LastUpdated = DateTime.Now };
        }

        public IEnumerable<KBMonthlyArticle> GetMonthlyArticles(int months)
        {
            var list = new List<KBMonthlyArticle>();
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("sp_KB_GetMonthlyArticles", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Months", months);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new KBMonthlyArticle
                        {
                            Year = reader.GetInt32(reader.GetOrdinal("Year")),
                            Month = reader.GetInt32(reader.GetOrdinal("Month")),
                            MonthName = reader.GetString(reader.GetOrdinal("MonthName")),
                            ArticleCount = reader.GetInt32(reader.GetOrdinal("ArticleCount"))
                        });
                    }
                }
            }
            return list;
        }

        public IEnumerable<KBTopArticle> GetTopArticles(int topN)
        {
            var list = new List<KBTopArticle>();
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("sp_KB_GetTopArticles", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TopN", topN);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new KBTopArticle
                        {
                            ArticleID = reader.GetInt32(reader.GetOrdinal("ArticleID")),
                            ArticleCode = reader.GetString(reader.GetOrdinal("ArticleCode")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            ArticleType = reader.GetString(reader.GetOrdinal("ArticleType")),
                            Category = reader.GetString(reader.GetOrdinal("Category")),
                            ViewCount = reader.GetInt32(reader.GetOrdinal("ViewCount")),
                            Rating = reader.GetDecimal(reader.GetOrdinal("Rating"))
                        });
                    }
                }
            }
            return list;
        }
    }
}
