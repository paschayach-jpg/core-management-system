using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IFolderAuthorityRepository
    {
        /// <summary>ดึงทั้งหมด (ทุก row ทุก Level)</summary>
        IEnumerable<FolderAuthorityConfig> GetAll();

        /// <summary>
        /// Resolve 4 rows (HC/SC/NC/PC) สำหรับพนักงาน 1 คน
        /// Priority: LevelCode > PositionName > DepartmentCode > DivisionCode
        /// </summary>
        IEnumerable<FolderAuthorityConfig> Resolve(string positionName, string departmentCode, string divisionCode, string levelCode = null);

        int  Insert(FolderAuthorityConfig config, int createdBy);
        void Update(FolderAuthorityConfig config, int updatedBy);

        /// <summary>Soft delete ทั้ง group (ทุก Level ของ ConfigLabel เดียวกัน)</summary>
        void Delete(string configLabel, int deletedBy);
    }
}
