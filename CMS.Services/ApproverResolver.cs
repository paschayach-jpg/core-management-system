using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Picks approvers by the folder's owning department (EMS.OrgUnit.OrgUnitCode == UserAccount.DepartmentCode):
    ///   1) active users of that department whose resolved FolderAuthorityConfig grants CanApprove
    ///      at the folder's ConfidentialityLevel
    ///   2) otherwise the same check for users of the parent division
    ///   3) otherwise active Admins (AppRole 4)
    /// The requester is never picked in tiers 1–2, and in tier 3 only when they are the sole Admin.
    /// Authority is resolved per user through IFolderAuthorityService.ResolveForUser — the same rule
    /// used at login — so approver eligibility cannot drift from the user's own folder permissions.
    /// </summary>
    public class ApproverResolver : IApproverResolver
    {
        private const int AdminRole             = 4;
        private const int OrgUnitTypeDivision   = 2;
        private const int OrgUnitTypeDepartment = 3;

        private readonly IFolderRepository       _folderRepository;
        private readonly IFileDocumentRepository _fileDocumentRepository;
        private readonly IOrgUnitRepository      _orgUnitRepository;
        private readonly IUserAccountRepository  _userAccountRepository;
        private readonly IFolderAuthorityService _folderAuthorityService;
        private readonly ILoggingService         _loggingService;
        private readonly IDownloadApproverConfigRepository _downloadApproverConfigRepository;
        private readonly IHrDepartmentRepository _hrDepartmentRepository;
        private readonly IHrDivisionRepository   _hrDivisionRepository;

        public ApproverResolver(
            IFolderRepository       folderRepository,
            IFileDocumentRepository fileDocumentRepository,
            IOrgUnitRepository      orgUnitRepository,
            IUserAccountRepository  userAccountRepository,
            IFolderAuthorityService folderAuthorityService,
            ILoggingService         loggingService,
            IDownloadApproverConfigRepository downloadApproverConfigRepository = null,
            IHrDepartmentRepository hrDepartmentRepository = null,
            IHrDivisionRepository   hrDivisionRepository   = null)
        {
            if (folderRepository       == null) throw new ArgumentNullException("folderRepository");
            if (fileDocumentRepository == null) throw new ArgumentNullException("fileDocumentRepository");
            if (orgUnitRepository      == null) throw new ArgumentNullException("orgUnitRepository");
            if (userAccountRepository  == null) throw new ArgumentNullException("userAccountRepository");
            if (folderAuthorityService == null) throw new ArgumentNullException("folderAuthorityService");
            if (loggingService         == null) throw new ArgumentNullException("loggingService");

            _folderRepository       = folderRepository;
            _fileDocumentRepository = fileDocumentRepository;
            _orgUnitRepository      = orgUnitRepository;
            _userAccountRepository  = userAccountRepository;
            _folderAuthorityService = folderAuthorityService;
            _loggingService         = loggingService;
            _downloadApproverConfigRepository = downloadApproverConfigRepository;
            _hrDepartmentRepository = hrDepartmentRepository;
            _hrDivisionRepository   = hrDivisionRepository;
        }

        public IList<UserAccount> GetConfiguredDownloadApprovers(int fileId, UserContext requester)
        {
            return GetConfiguredApprovers(DownloadApproverConfig.TypeDownload, fileId, requester);
        }

        public IList<UserAccount> GetConfiguredShareApprovers(int fileId, UserContext requester)
        {
            return GetConfiguredApprovers(DownloadApproverConfig.TypeShare, fileId, requester);
        }

        public IList<UserAccount> GetConfiguredApprovers(string approvalType, int fileId, UserContext requester)
        {
            var result = new List<UserAccount>();
            if (requester == null || _downloadApproverConfigRepository == null)
                return result;

            string type = string.IsNullOrWhiteSpace(approvalType) ? DownloadApproverConfig.TypeDownload : approvalType.Trim();

            List<DownloadApproverConfig> all;
            try
            {
                all = _downloadApproverConfigRepository.GetAll(type).ToList();
            }
            catch (Exception ex)
            {
                // ตารางยังไม่ถูกสร้าง หรือ DB error → ใช้กฎ fallback แทน
                _loggingService.Error(string.Format("ApproverResolver: Failed to load approver config for type '{0}'.", type), ex);
                return result;
            }
            if (all.Count == 0)
                return result;

            string deptCode, deptName, divCode, divName;
            ResolveFileOrg(fileId, requester.UserID, out deptCode, out deptName, out divCode, out divName);

            // config ใช้รหัส HR Master; OrgUnit / ผู้ใช้บางคนใช้รหัสชุดอื่น → จับคู่ด้วยรหัส หรือ ชื่อ HR ของรหัสนั้น
            var deptNamesByCode = HrDepartmentNames();
            var divNamesByCode  = HrDivisionNames();

            // แผนกก่อน แล้วต่อด้วยฝ่าย (แต่ละกลุ่มเรียงตาม SortOrder)
            var configs = all
                .Where(c => c.ScopeType == DownloadApproverConfig.ScopeDepartment
                         && OrgMatches(c.OrgCode, deptCode, deptName, deptNamesByCode))
                .OrderBy(c => c.SortOrder)
                .Concat(all
                    .Where(c => c.ScopeType == DownloadApproverConfig.ScopeDivision
                             && OrgMatches(c.OrgCode, divCode, divName, divNamesByCode))
                    .OrderBy(c => c.SortOrder))
                .ToList();

            if (configs.Count == 0)
                return result;

            var usersById = _userAccountRepository.GetAll().ToDictionary(u => u.UserID);
            var seen = new HashSet<int>();
            foreach (var cfg in configs)
            {
                UserAccount u;
                if (cfg.ApproverUserID == requester.UserID || !seen.Add(cfg.ApproverUserID)) continue;
                if (usersById.TryGetValue(cfg.ApproverUserID, out u) && u.IsActive)
                    result.Add(u);
            }
            return result;
        }

        /// <summary>
        /// แผนก/ฝ่าย ที่เป็นเจ้าของไฟล์: ไล่ OrgUnit ของโฟลเดอร์ขึ้นไปหา Department (type 3) และ Division (type 2).
        /// หาไม่ได้เลย → ใช้แผนก/ฝ่ายของผู้ขอ
        /// </summary>
        private void ResolveFileOrg(int fileId, int requesterId,
            out string deptCode, out string deptName, out string divCode, out string divName)
        {
            deptCode = deptName = divCode = divName = null;

            var file    = _fileDocumentRepository.GetById(fileId);
            var folder  = file != null ? _folderRepository.GetById(file.FolderID) : null;
            var orgUnit = folder != null ? _orgUnitRepository.GetById(folder.OrgUnitID) : null;

            for (int depth = 0; orgUnit != null && depth < 10; depth++)
            {
                if (orgUnit.OrgUnitType == OrgUnitTypeDepartment && deptCode == null && deptName == null)
                {
                    deptCode = orgUnit.OrgUnitCode;
                    deptName = orgUnit.OrgUnitNameTH;
                }
                else if (orgUnit.OrgUnitType == OrgUnitTypeDivision && divCode == null && divName == null)
                {
                    divCode = orgUnit.OrgUnitCode;
                    divName = orgUnit.OrgUnitNameTH;
                }

                orgUnit = orgUnit.ParentOrgUnitID.HasValue
                    ? _orgUnitRepository.GetById(orgUnit.ParentOrgUnitID.Value)
                    : null;
            }

            if (deptCode == null && deptName == null && divCode == null && divName == null)
            {
                var requester = _userAccountRepository.GetById(requesterId);
                if (requester != null)
                {
                    deptCode = requester.DepartmentCode;
                    deptName = requester.DepartmentName;
                    divCode  = requester.DivisionCode;
                    divName  = requester.DivisionName;
                }
            }
        }

        private static bool OrgMatches(string configCode, string orgCode, string orgName,
            Dictionary<string, List<string>> hrNamesByCode)
        {
            if (SameCode(configCode, orgCode))
                return true;

            List<string> names;
            return !string.IsNullOrWhiteSpace(orgName)
                && configCode != null
                && hrNamesByCode.TryGetValue(configCode.Trim(), out names)
                && names.Any(n => SameCode(n, orgName));
        }

        private Dictionary<string, List<string>> HrDepartmentNames()
        {
            var map = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            if (_hrDepartmentRepository == null) return map;
            try
            {
                foreach (var d in _hrDepartmentRepository.GetAll().Where(d => !string.IsNullOrWhiteSpace(d.DepartmentCode)))
                    AddNames(map, d.DepartmentCode, d.DepartmentName, d.DepartmentNameTh, d.DepartmentNameEn);
            }
            catch (Exception ex)
            {
                _loggingService.Error("ApproverResolver: Failed to load HR departments.", ex);
            }
            return map;
        }

        private Dictionary<string, List<string>> HrDivisionNames()
        {
            var map = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            if (_hrDivisionRepository == null) return map;
            try
            {
                foreach (var d in _hrDivisionRepository.GetAll().Where(d => !string.IsNullOrWhiteSpace(d.DivisionCode)))
                    AddNames(map, d.DivisionCode, d.DivisionName);
            }
            catch (Exception ex)
            {
                _loggingService.Error("ApproverResolver: Failed to load HR divisions.", ex);
            }
            return map;
        }

        private static void AddNames(Dictionary<string, List<string>> map, string code, params string[] names)
        {
            List<string> list;
            if (!map.TryGetValue(code.Trim(), out list))
                map[code.Trim()] = list = new List<string>();
            list.AddRange(names.Where(n => !string.IsNullOrWhiteSpace(n)));
        }

        public IList<UserAccount> ResolveForFile(int fileId, int requesterId)
        {
            var file = _fileDocumentRepository.GetById(fileId);
            if (file == null)
                return GetAdmins(requesterId);

            return ResolveForFolder(file.FolderID, requesterId);
        }

        public IList<UserAccount> ResolveForFolder(int folderId, int requesterId)
        {
            var folder = _folderRepository.GetById(folderId);
            if (folder == null)
                return GetAdmins(requesterId);

            string departmentCode, divisionCode;
            ResolveOrgCodes(folder.OrgUnitID, out departmentCode, out divisionCode);

            var candidates = _userAccountRepository.GetAll()
                .Where(u => u.IsActive && u.UserID != requesterId)
                .ToList();

            int level = folder.ConfidentialityLevel;

            var approvers = WithApproveRight(
                candidates.Where(u => SameCode(u.DepartmentCode, departmentCode)), level);

            // Division tier: skip users already checked in the department tier.
            if (approvers.Count == 0)
                approvers = WithApproveRight(
                    candidates.Where(u => SameCode(u.DivisionCode, divisionCode)
                                       && !SameCode(u.DepartmentCode, departmentCode)), level);

            if (approvers.Count == 0)
                approvers = GetAdmins(requesterId).ToList();

            return approvers;
        }

        /// <summary>
        /// Folder's OrgUnit → (department code, division code). A department folder also yields its
        /// parent division; a division folder yields only the division code.
        /// </summary>
        private void ResolveOrgCodes(int orgUnitId, out string departmentCode, out string divisionCode)
        {
            departmentCode = null;
            divisionCode   = null;

            var orgUnit = _orgUnitRepository.GetById(orgUnitId);
            if (orgUnit == null) return;

            if (orgUnit.OrgUnitType == OrgUnitTypeDepartment)
            {
                departmentCode = orgUnit.OrgUnitCode;
                if (orgUnit.ParentOrgUnitID.HasValue)
                {
                    var parent = _orgUnitRepository.GetById(orgUnit.ParentOrgUnitID.Value);
                    if (parent != null && parent.OrgUnitType == OrgUnitTypeDivision)
                        divisionCode = parent.OrgUnitCode;
                }
            }
            else if (orgUnit.OrgUnitType == OrgUnitTypeDivision)
            {
                divisionCode = orgUnit.OrgUnitCode;
            }
        }

        private List<UserAccount> WithApproveRight(IEnumerable<UserAccount> users, int confidentialityLevel)
        {
            var result = new List<UserAccount>();
            foreach (var u in users)
            {
                try
                {
                    var authority = _folderAuthorityService.ResolveForUser(u);
                    FolderAuthorityConfig cfg;
                    if (authority.TryGetValue(confidentialityLevel, out cfg) && cfg != null && cfg.CanApprove)
                        result.Add(u);
                }
                catch (Exception ex)
                {
                    _loggingService.Error(string.Format(
                        "ApproverResolver: Failed to resolve authority for user {0}.", u.UserID), ex);
                }
            }
            return result;
        }

        public IList<UserAccount> GetDepartmentMembersForFile(int fileId, UserContext requester)
        {
            if (requester == null) throw new ArgumentNullException("requester");

            string departmentCode = null, departmentName = null;

            var file = _fileDocumentRepository.GetById(fileId);
            var folder = file != null ? _folderRepository.GetById(file.FolderID) : null;
            var orgUnit = folder != null ? _orgUnitRepository.GetById(folder.OrgUnitID) : null;
            if (orgUnit != null && orgUnit.OrgUnitType == OrgUnitTypeDepartment)
            {
                departmentCode = orgUnit.OrgUnitCode;
                departmentName = orgUnit.OrgUnitNameTH;
            }

            if (string.IsNullOrWhiteSpace(departmentCode) && string.IsNullOrWhiteSpace(departmentName))
            {
                departmentCode = requester.DepartmentCode;
                departmentName = requester.DepartmentName;
            }

            // Match by code OR name: some users still carry Employee API codes (e.g. "01") instead of
            // the HR Master code (e.g. "20024") for the same department.
            return _userAccountRepository.GetAll()
                .Where(u => u.IsActive && u.UserID != requester.UserID
                         && (SameCode(u.DepartmentCode, departmentCode) || SameCode(u.DepartmentName, departmentName)))
                .ToList();
        }

        public IList<UserAccount> GetAdmins(int requesterId)
        {
            var admins = _userAccountRepository.GetByRole(AdminRole)
                .Where(a => a.IsActive)
                .ToList();

            var others = admins.Where(a => a.UserID != requesterId).ToList();
            return others.Any() ? others : admins;
        }

        private static bool SameCode(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
