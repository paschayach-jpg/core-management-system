using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class SubCommitteeRepository : ISubCommitteeRepository
    {
        private readonly string _connectionString;

        public SubCommitteeRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<SubCommittee> GetAll()
        {
            var results = new List<SubCommittee>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT SubCommitteeID, OrgUnitID, SubCommitteeCode, SubCommitteeNameTH, " +
                    "       ISNULL(SubCommitteeNameEN,'') AS SubCommitteeNameEN, IsActive, IsDeleted, CreatedDate " +
                    "FROM core.m_SubCommittee " +
                    "WHERE IsActive = 1 AND IsDeleted = 0", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read()) results.Add(MapSubCommittee(reader));
                }
            }
            return results;
        }

        private static SubCommittee MapSubCommittee(SqlDataReader reader)
        {
            return new SubCommittee
            {
                SubCommitteeID   = Convert.ToInt32(reader["SubCommitteeID"]),
                OrgUnitID        = Convert.ToInt32(reader["OrgUnitID"]),
                SubCommitteeCode = reader["SubCommitteeCode"] == DBNull.Value ? string.Empty : Convert.ToString(reader["SubCommitteeCode"]),
                SubCommitteeNameTH = reader["SubCommitteeNameTH"] == DBNull.Value ? string.Empty : Convert.ToString(reader["SubCommitteeNameTH"]),
                SubCommitteeNameEN = reader["SubCommitteeNameEN"] == DBNull.Value ? string.Empty : Convert.ToString(reader["SubCommitteeNameEN"]),
                IsActive         = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                IsDeleted        = reader["IsDeleted"] != DBNull.Value && Convert.ToBoolean(reader["IsDeleted"]),
                CreatedDate      = reader["CreatedDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["CreatedDate"])
            };
        }
    }
}
