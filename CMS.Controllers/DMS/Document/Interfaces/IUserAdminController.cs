using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IUserAdminController
    {
        IList<UserAccount> GetAllUsers();
        int CreateUser(UserAccount user);
        void UpdateUser(UserAccount user);
        void DeactivateUser(int userId);
        void UpdateEmployeeInfo(
            int userId,
            string employeeCode,
            string titleNameEn,
            string firstNameEn,
            string lastNameEn,
            string titleNameTh,
            string firstNameTh,
            string lastNameTh,
            int? departmentId,
            string departmentCode,
            string departmentNameEn,
            string departmentNameTh,
            int? divisionId,
            string divisionCode,
            string divisionNameEn,
            string divisionNameTh,
            string positionName);

        IList<HrDivision> GetDivisions();
        IList<HrDepartment> GetDepartments();
        IList<HrPosition> GetPositions();
        IList<HrPositionLevel> GetPositionLevels();
    }
}
