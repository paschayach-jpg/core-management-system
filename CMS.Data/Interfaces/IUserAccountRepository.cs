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

        /// <summary>ดึง distinct PositionName ทั้งหมดใน DepartmentCode นั้น</summary>
        IEnumerable<string> GetPositionsByDepartmentCode(string departmentCode);

        /// <summary>ดึง users ทั้งหมดที่อยู่ใน division เดียวกัน</summary>
        IEnumerable<UserAccount> GetByDivisionId(int divisionId);

        /// <summary>อัพเดทข้อมูลพนักงานจาก AD API (ชื่อ TH/EN, แผนก, ฝ่าย, ตำแหน่ง)</summary>
        void UpdateEmployeeInfo(int userId, string employeeCode,
            string titleNameEn, string firstNameEn, string lastNameEn,
            string titleNameTh, string firstNameTh, string lastNameTh,
            int? departmentId, string departmentCode, string departmentNameEn, string departmentNameTh,
            int? divisionId,   string divisionCode,   string divisionNameEn,   string divisionNameTh,
            string positionName);

        /// <summary>Update LastLoginDate + LastLoginIP</summary>
        void UpdateLastLoginWithIP(int userId, string ipAddress);

        /// <summary>Upsert: Insert ถ้าใหม่ / Update ถ้ามีแล้ว คืน UserID และ isNew flag</summary>
        int RegisterOrUpdate(UserAccount account, string ipAddress, out bool isNew);
    }
}
