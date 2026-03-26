using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly string _connectionString;

        public NotificationRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int Insert(Notification notification)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_InsertNotification", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RecipientUserId", notification.RecipientUserId);
                    cmd.Parameters.AddWithValue("@EventType",       notification.EventType);
                    cmd.Parameters.AddWithValue("@RelatedEntityId", (object)notification.RelatedEntityId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MessageText",     notification.MessageText);
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        public IEnumerable<Notification> GetUnreadByUser(int userId)
        {
            var results = new List<Notification>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_GetUnreadNotificationsByUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            results.Add(MapNotification(reader));
                    }
                }
            }
            return results;
        }

        public void MarkRead(int notificationId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("core.sp_MarkNotificationRead", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NotificationId", notificationId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static Notification MapNotification(SqlDataReader reader)
        {
            return new Notification
            {
                NotificationId  = (int)reader["NotificationId"],
                RecipientUserId = (int)reader["RecipientUserId"],
                EventType       = (string)reader["EventType"],
                RelatedEntityId = reader["RelatedEntityId"] == DBNull.Value ? null : (string)reader["RelatedEntityId"],
                MessageText     = (string)reader["MessageText"],
                IsRead          = (bool)reader["IsRead"],
                DeliveredAt     = (DateTime)reader["DeliveredAt"],
                ReadAt          = reader["ReadAt"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ReadAt"]
            };
        }
    }
}
