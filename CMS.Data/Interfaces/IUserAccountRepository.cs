using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IUserAccountRepository
    {
        UserAccount GetByADUserName(string adUserName);
        UserAccount GetById(int userId);
        IEnumerable<UserAccount> GetByRole(int appRole);
        IEnumerable<UserAccount> GetAll();
        int Create(UserAccount account);
        void Update(UserAccount account);
        void UpdateLastLogin(int userId);
        void UpdateConsent(int userId, string email, bool consentGiven);
        void Deactivate(int userId);

        /// <summary>ดึง users ทั้งหมดที่อยู่ใน department เดียวกัน</summary>
        IEnumerable<UserAccount> GetByDepartmentId(int departmentId);

        /// <summary>ดึง users ทั้งหมดที่อยู่ใน division เดียวกัน</summary>
        IEnumerable<UserAccount> GetByDivisionId(int divisionId);

        /// <summary>อัพเดทข้อมูล Employee API (department/division/position)</summary>
        void UpdateEmployeeInfo(int userId, string employeeCode,
            int? departmentId, string departmentCode, string departmentName,
            int? divisionId, string divisionCode, string divisionName,
            string positionName);
    }
}
