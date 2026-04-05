using System;
using System.Collections.Generic;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class FolderAuthorityService : IFolderAuthorityService
    {
        private readonly IFolderAuthorityRepository _repo;

        public FolderAuthorityService(IFolderAuthorityRepository repo)
        {
            if (repo == null) throw new ArgumentNullException("repo");
            _repo = repo;
        }

        public IEnumerable<FolderAuthorityConfig> GetAll()
        {
            return _repo.GetAll();
        }

        public Dictionary<int, FolderAuthorityConfig> ResolveForEmployee(
            string positionName, string departmentCode, string divisionCode, string levelCode = null)
        {
            var dict = new Dictionary<int, FolderAuthorityConfig>();
            var rows = _repo.Resolve(positionName, departmentCode, divisionCode, levelCode);
            foreach (var row in rows)
                dict[row.ConfidentialityLevel] = row;
            return dict;
        }

        public Dictionary<int, FolderAuthorityConfig> ResolveForUser(UserAccount user)
        {
            if (user == null) return new Dictionary<int, FolderAuthorityConfig>();
            return ResolveForEmployee(user.PositionName, user.DepartmentCode, user.DivisionCode, user.LevelCode);
        }

        public int Save(FolderAuthorityConfig config, int userId)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (string.IsNullOrEmpty(config.ConfigLabel))
                throw new ArgumentException("ConfigLabel ต้องไม่ว่าง");
            if (config.ConfidentialityLevel < 1 || config.ConfidentialityLevel > 4)
                throw new ArgumentException("ConfidentialityLevel ต้องเป็น 1-4");

            if (config.ConfigID == 0)
                return _repo.Insert(config, userId);

            _repo.Update(config, userId);
            return config.ConfigID;
        }

        public void Delete(string configLabel, int userId)
        {
            if (string.IsNullOrEmpty(configLabel)) throw new ArgumentNullException("configLabel");
            _repo.Delete(configLabel, userId);
        }
    }
}
