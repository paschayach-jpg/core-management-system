using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    public sealed class AuthorityOrgLookupController : IAuthorityOrgLookupController
    {
        private readonly IHrDivisionRepository _hrDivisionRepository;
        private readonly IHrDepartmentRepository _hrDepartmentRepository;
        private readonly IHrPositionRepository _hrPositionRepository;
        private readonly IHrPositionLevelRepository _hrPositionLevelRepository;
        private readonly IHrPositionMappingRepository _hrPositionMappingRepository;
        private readonly IUserAccountRepository _userAccountRepository;

        public AuthorityOrgLookupController(
            IHrDivisionRepository hrDivisionRepository = null,
            IHrDepartmentRepository hrDepartmentRepository = null,
            IHrPositionRepository hrPositionRepository = null,
            IHrPositionLevelRepository hrPositionLevelRepository = null,
            IHrPositionMappingRepository hrPositionMappingRepository = null,
            IUserAccountRepository userAccountRepository = null)
        {
            _hrDivisionRepository = hrDivisionRepository;
            _hrDepartmentRepository = hrDepartmentRepository;
            _hrPositionRepository = hrPositionRepository;
            _hrPositionLevelRepository = hrPositionLevelRepository;
            _hrPositionMappingRepository = hrPositionMappingRepository;
            _userAccountRepository = userAccountRepository;
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

        public IList<HrPositionMapping> GetPositionMappings()
        {
            if (_hrPositionMappingRepository == null)
                return new List<HrPositionMapping>();
            return _hrPositionMappingRepository.GetAll().ToList();
        }

        public IList<HrPositionMapping> GetPositionMappingsByDepartmentId(int departmentId)
        {
            if (_hrPositionMappingRepository == null)
                return new List<HrPositionMapping>();
            return _hrPositionMappingRepository.GetByDepartmentId(departmentId).ToList();
        }

        public IList<string> GetPositionsByDepartmentCode(string departmentCode)
        {
            if (_userAccountRepository == null || string.IsNullOrEmpty(departmentCode))
                return new List<string>();
            return _userAccountRepository.GetPositionsByDepartmentCode(departmentCode).ToList();
        }
    }
}
