using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    public sealed class UserAdminController : IUserAdminController
    {
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IHrDivisionRepository _hrDivisionRepository;
        private readonly IHrDepartmentRepository _hrDepartmentRepository;
        private readonly IHrPositionRepository _hrPositionRepository;
        private readonly IHrPositionLevelRepository _hrPositionLevelRepository;

        public UserAdminController(
            IUserAccountRepository userAccountRepository = null,
            IHrDivisionRepository hrDivisionRepository = null,
            IHrDepartmentRepository hrDepartmentRepository = null,
            IHrPositionRepository hrPositionRepository = null,
            IHrPositionLevelRepository hrPositionLevelRepository = null)
        {
            _userAccountRepository = userAccountRepository;
            _hrDivisionRepository = hrDivisionRepository;
            _hrDepartmentRepository = hrDepartmentRepository;
            _hrPositionRepository = hrPositionRepository;
            _hrPositionLevelRepository = hrPositionLevelRepository;
        }

        public IList<UserAccount> GetAllUsers()
        {
            if (_userAccountRepository == null)
                return new List<UserAccount>();

            return _userAccountRepository.GetAll().ToList();
        }

        public int CreateUser(UserAccount user)
        {
            if (_userAccountRepository == null || user == null)
                return 0;

            return _userAccountRepository.Create(user);
        }

        public void UpdateUser(UserAccount user)
        {
            if (_userAccountRepository == null || user == null)
                return;

            _userAccountRepository.Update(user);
        }

        public void DeactivateUser(int userId)
        {
            if (_userAccountRepository == null)
                return;

            _userAccountRepository.Deactivate(userId);
        }

        public void UpdateEmployeeInfo(
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
            string positionName)
        {
            if (_userAccountRepository == null)
                return;

            _userAccountRepository.UpdateEmployeeInfo(
                userId,
                employeeCode,
                titleNameEn,
                firstNameEn,
                lastNameEn,
                titleNameTh,
                firstNameTh,
                lastNameTh,
                departmentId,
                departmentCode,
                departmentNameEn,
                departmentNameTh,
                divisionId,
                divisionCode,
                divisionNameEn,
                divisionNameTh,
                positionName);
        }

        public IList<HrDivision> GetDivisions()
        {
            if (_hrDivisionRepository == null)
                return new List<HrDivision>();

            return _hrDivisionRepository.GetAll().ToList();
        }

        public IList<HrDepartment> GetDepartments()
        {
            if (_hrDepartmentRepository == null)
                return new List<HrDepartment>();

            return _hrDepartmentRepository.GetAll().ToList();
        }

        public IList<HrPosition> GetPositions()
        {
            if (_hrPositionRepository == null)
                return new List<HrPosition>();

            return _hrPositionRepository.GetAll().ToList();
        }

        public IList<HrPositionLevel> GetPositionLevels()
        {
            if (_hrPositionLevelRepository == null)
                return new List<HrPositionLevel>();

            return _hrPositionLevelRepository.GetAll().ToList();
        }
    }
}
