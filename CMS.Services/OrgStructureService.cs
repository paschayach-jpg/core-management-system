using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Implements IOrgStructureService: manages the 3-tier OrgUnit hierarchy.
    /// OrgUnitType: 1=SubCommittee, 2=Division, 3=Department
    /// </summary>
    public class OrgStructureService : IOrgStructureService
    {
        private readonly IOrgUnitRepository        _orgUnitRepository;
        private readonly IAuditService             _auditService;
        private readonly IUserOrgVisibilityService _visibilityService;

        public OrgStructureService(
            IOrgUnitRepository        orgUnitRepository,
            IAuditService             auditService      = null,
            IUserOrgVisibilityService visibilityService = null)
        {
            if (orgUnitRepository == null) throw new ArgumentNullException("orgUnitRepository");
            _orgUnitRepository = orgUnitRepository;
            _auditService      = auditService;
            _visibilityService = visibilityService;
        }

        public OrgUnit CreateOrgUnit(CreateOrgUnitRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user    == null) throw new ArgumentNullException("user");

            // Admin=4 only
            if (user.AppRole != 4)
                throw new UnauthorizedAccessException("Creating an OrgUnit requires Admin role.");

            if (string.IsNullOrWhiteSpace(request.OrgUnitNameTH))
                throw new ArgumentException("OrgUnit name (TH) is required.");

            // A Division keeps its HR code and exists once. Adding it under another SubCommittee links
            // the existing row through EMS.OrgUnitRelationship instead of inserting a copy under a
            // generated code (which broke the HR mapping and looked like the add had failed).
            if (request.OrgUnitType == (int)OrgUnitTier.Division && request.ParentOrgUnitID.HasValue)
            {
                OrgUnit existing = null;
                if (request.LinkExistingOrgUnitId.HasValue)
                    existing = _orgUnitRepository.GetById(request.LinkExistingOrgUnitId.Value);
                else if (!string.IsNullOrWhiteSpace(request.OrgUnitCode))
                    existing = _orgUnitRepository.GetByCode(request.OrgUnitCode.Trim());

                if (existing != null && !existing.IsDeleted && existing.OrgUnitType == (int)OrgUnitTier.Division)
                    return LinkExistingDivision(existing, request.ParentOrgUnitID.Value, user);
            }

            // Standalone Division (no parent): always a separate row, even when the HR code already exists
            // under a SubCommittee — the code falls back to a generated one below.

            // Server-side code generation fallback if code is empty or conflicts
            if (string.IsNullOrWhiteSpace(request.OrgUnitCode) || _orgUnitRepository.ExistsByCode(request.OrgUnitCode))
            {
                if (!string.IsNullOrWhiteSpace(request.OrgUnitCode))
                {
                    // Log the conflict for debugging
                    System.Diagnostics.Debug.WriteLine($"Code conflict detected: {request.OrgUnitCode} already exists. Generating new code.");
                }
                
                request.OrgUnitCode = GenerateUniqueCode(request.OrgUnitType);
                System.Diagnostics.Debug.WriteLine($"Server-side generated code: {request.OrgUnitCode}");
            }

            ValidateParentTier(request.OrgUnitType, request.ParentOrgUnitID);
            ValidateNameUnique(request.OrgUnitNameTH.Trim(), request.ParentOrgUnitID, excludeId: null);

            var orgUnit = new OrgUnit
            {
                OrgUnitType    = request.OrgUnitType,
                OrgUnitCode    = request.OrgUnitCode,
                OrgUnitNameTH  = request.OrgUnitNameTH.Trim(),
                OrgUnitNameEN  = request.OrgUnitNameEN,
                ParentOrgUnitID = request.ParentOrgUnitID,
                IsActive       = true,
                IsDeleted      = false,
                CreatedBy      = user.UserID,
                CreatedDate    = DateTime.Now
            };

            // Retry mechanism for race conditions
            int maxRetries = 3;
            int attempt = 0;
            
            while (attempt < maxRetries)
            {
                try
                {
                    int newId = _orgUnitRepository.Insert(orgUnit);
                    orgUnit.OrgUnitID = newId;

                    if (orgUnit.OrgUnitType == (int)OrgUnitTier.SubCommittee)
                    {
                        _orgUnitRepository.InsertSubCommitteeRecord(
                            newId,
                            orgUnit.OrgUnitCode,
                            orgUnit.OrgUnitNameTH,
                            orgUnit.OrgUnitNameEN,
                            user.ADUserName);
                    }

                    Log(user, "OrgStructure", "CREATE", "EMS.OrgUnit", newId, 1,
                        string.Format("Created OrgUnit '{0}' (type {1}).", orgUnit.OrgUnitNameTH, orgUnit.OrgUnitType));

                    PropagateVisibility(newId, orgUnit.ParentOrgUnitID, user);

                    return orgUnit;
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("มีอยู่ในระบบแล้ว"))
                {
                    attempt++;
                    if (attempt >= maxRetries)
                    {
                        throw new InvalidOperationException(
                            string.Format("ไม่สามารถสร้างรหัสหน่วยงานที่ไม่ซ้ำได้หลังจากพยายาม {0} ครั้ง", maxRetries), ex);
                    }
                    
                    // Generate a new code and try again
                    orgUnit.OrgUnitCode = GenerateUniqueCode(request.OrgUnitType);
                    System.Diagnostics.Debug.WriteLine($"Retry {attempt}: Generated new code {orgUnit.OrgUnitCode}");
                }
            }

            // This should never be reached, but just in case
            throw new InvalidOperationException("ไม่สามารถสร้างหน่วยงานได้");
        }

        public OrgUnit UpdateOrgUnit(UpdateOrgUnitRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user    == null) throw new ArgumentNullException("user");

            if (user.AppRole != 4)
                throw new UnauthorizedAccessException("Updating an OrgUnit requires Admin role.");

            if (string.IsNullOrWhiteSpace(request.OrgUnitNameTH))
                throw new ArgumentException("OrgUnit name (TH) is required.");

            var existing = _orgUnitRepository.GetById(request.OrgUnitID);
            if (existing == null)
                throw new InvalidOperationException(string.Format(
                    "OrgUnit with ID {0} was not found.", request.OrgUnitID));

            if (!existing.IsActive)
                throw new InvalidOperationException(string.Format(
                    "OrgUnit '{0}' is inactive and cannot be updated.", existing.OrgUnitNameTH));

            existing.OrgUnitNameTH = request.OrgUnitNameTH.Trim();
            existing.OrgUnitNameEN = request.OrgUnitNameEN;

            ValidateNameUnique(existing.OrgUnitNameTH, existing.ParentOrgUnitID, excludeId: existing.OrgUnitID);

            _orgUnitRepository.Update(existing);

            Log(user, "OrgStructure", "UPDATE", "EMS.OrgUnit", existing.OrgUnitID, 1,
                string.Format("Updated OrgUnit name to '{0}'.", existing.OrgUnitNameTH));

            return existing;
        }

        public OrgUnit MoveOrgUnit(UpdateOrgUnitRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user    == null) throw new ArgumentNullException("user");

            if (user.AppRole != 4)
                throw new UnauthorizedAccessException("Moving an OrgUnit requires Admin role.");

            var existing = _orgUnitRepository.GetById(request.OrgUnitID);
            if (existing == null)
                throw new InvalidOperationException(string.Format(
                    "OrgUnit with ID {0} was not found.", request.OrgUnitID));

            if (!existing.IsActive)
                throw new InvalidOperationException(string.Format(
                    "OrgUnit '{0}' is inactive and cannot be moved.", existing.OrgUnitNameTH));

            // Validate the new parent
            ValidateParentTier(existing.OrgUnitType, request.ParentOrgUnitID);
            ValidateNameUnique(existing.OrgUnitNameTH, request.ParentOrgUnitID, excludeId: existing.OrgUnitID);

            var oldParentId = existing.ParentOrgUnitID;
            existing.ParentOrgUnitID = request.ParentOrgUnitID;

            _orgUnitRepository.Update(existing);

            Log(user, "OrgStructure", "MOVE", "EMS.OrgUnit", existing.OrgUnitID, 1,
                string.Format("Moved OrgUnit '{0}' from parent {1} to parent {2}.", 
                    existing.OrgUnitNameTH, oldParentId ?? 0, request.ParentOrgUnitID ?? 0));

            return existing;
        }

        public void DeactivateOrgUnit(int orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            if (user.AppRole != 4)
                throw new UnauthorizedAccessException("Deactivating an OrgUnit requires Admin role.");

            var existing = _orgUnitRepository.GetById(orgUnitId);
            if (existing == null)
                throw new InvalidOperationException(string.Format(
                    "OrgUnit with ID {0} was not found.", orgUnitId));

            if (!existing.IsActive) return; // already inactive — idempotent

            if (_orgUnitRepository.HasActiveDependents(orgUnitId))
                throw new InvalidOperationException(string.Format(
                    "Cannot deactivate OrgUnit '{0}' (ID {1}): one or more active Folders or " +
                    "UserAccounts still reference this unit.",
                    existing.OrgUnitNameTH, orgUnitId));

            _orgUnitRepository.Deactivate(orgUnitId);

            Log(user, "OrgStructure", "DELETE", "EMS.OrgUnit", orgUnitId, 1,
                string.Format("Deactivated OrgUnit '{0}'.", existing.OrgUnitNameTH));
        }

        public void DeleteOrgUnit(int orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            if (user.AppRole != 4)
                throw new UnauthorizedAccessException("Deleting an OrgUnit requires Admin role.");

            var existing = _orgUnitRepository.GetById(orgUnitId);
            if (existing == null)
                throw new InvalidOperationException(string.Format(
                    "OrgUnit with ID {0} was not found.", orgUnitId));

            if (existing.IsDeleted) return; // already deleted — idempotent

            if (_orgUnitRepository.HasActiveDependents(orgUnitId))
                throw new InvalidOperationException(string.Format(
                    "Cannot delete OrgUnit '{0}' (ID {1}): one or more active Folders or " +
                    "UserAccounts still reference this unit.",
                    existing.OrgUnitNameTH, orgUnitId));

            _orgUnitRepository.SoftDelete(orgUnitId, user.UserID);

            Log(user, "OrgStructure", "DELETE", "EMS.OrgUnit", orgUnitId, 1,
                string.Format("Deleted OrgUnit '{0}'.", existing.OrgUnitNameTH));
        }

        public void ForceDeleteOrgUnit(int orgUnitId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            if (user.AppRole != 4)
                throw new UnauthorizedAccessException("Force-deleting an OrgUnit requires Admin role.");

            var existing = _orgUnitRepository.GetById(orgUnitId);
            if (existing == null)
                throw new InvalidOperationException(string.Format(
                    "OrgUnit with ID {0} was not found.", orgUnitId));

            if (existing.IsDeleted) return;

            _orgUnitRepository.ForceDeleteWithDependents(orgUnitId, user.UserID);

            Log(user, "OrgStructure", "FORCE_DELETE", "EMS.OrgUnit", orgUnitId, 1,
                string.Format("Force-deleted OrgUnit '{0}' with all dependents.", existing.OrgUnitNameTH));
        }

        public bool HasActiveDependents(int orgUnitId)
        {
            return _orgUnitRepository.HasActiveDependents(orgUnitId);
        }

        public IEnumerable<OrgUnit> GetHierarchy()
        {
            return _orgUnitRepository.GetHierarchy();
        }

        public IEnumerable<OrgUnit> GetVisibleHierarchy(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            if (_visibilityService != null)
                return _visibilityService.GetVisibleOrgUnits(user.UserID, user);

            // Fallback: no visibility service wired — Admin-only internal use
            return _orgUnitRepository.GetHierarchy();
        }

        public IEnumerable<OrgUnit> GetByConfidentialityLevel(ConfidentialityLevel level)
        {
            return _orgUnitRepository.GetByConfidentialityLevel(level);
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// ตรวจสอบว่าชื่อ OrgUnitNameTH ไม่ซ้ำภายใต้ Parent เดียวกัน
        /// excludeId ใช้กรณี Update/Move เพื่อยกเว้นตัวเองออกจากการตรวจ
        /// </summary>
        private void ValidateNameUnique(string nameTH, int? parentId, int? excludeId)
        {
            if (_orgUnitRepository.ExistsByNameUnderParent(nameTH, parentId, excludeId))
            {
                var parentDesc = parentId.HasValue
                    ? string.Format("ภายใต้หน่วยงาน ID {0}", parentId.Value)
                    : "ระดับ Sub-Committee";

                throw new InvalidOperationException(string.Format(
                    "ชื่อหน่วยงาน '{0}' มีอยู่แล้ว{1} กรุณาใช้ชื่ออื่น",
                    nameTH, parentDesc));
            }
        }

        private void ValidateParentTier(int orgUnitType, int? parentId)
        {
            switch (orgUnitType)
            {
                case 1: // SubCommittee — no parent
                    if (parentId.HasValue)
                        throw new ArgumentException("A Sub-Committee (type 1) must not have a parent OrgUnit.");
                    break;

                case 2: // Division — parent is optional (can belong to multiple SubCommittees via relationship table)
                    if (parentId.HasValue)
                    {
                        var divParent = _orgUnitRepository.GetById(parentId.Value);
                        if (divParent == null)
                            throw new ArgumentException(string.Format("Parent OrgUnit with ID {0} does not exist.", parentId.Value));
                        if (!divParent.IsActive)
                            throw new ArgumentException(string.Format("Parent OrgUnit '{0}' is inactive.", divParent.OrgUnitNameTH));
                        if (divParent.OrgUnitType != 1)
                            throw new ArgumentException(string.Format(
                                "A Division (type 2) can only be created under a Sub-Committee (type 1). " +
                                "The selected parent '{0}' is type {1}.", divParent.OrgUnitNameTH, divParent.OrgUnitType));
                    }
                    // Note: Division can have no parent and be linked to SubCommittees via OrgUnitRelationship table
                    break;

                case 3: // Department — parent must be Division (2)
                    if (!parentId.HasValue)
                        throw new ArgumentException("A Department (type 3) requires a parent Division.");

                    var deptParent = _orgUnitRepository.GetById(parentId.Value);
                    if (deptParent == null)
                        throw new ArgumentException(string.Format("Parent OrgUnit with ID {0} does not exist.", parentId.Value));
                    if (!deptParent.IsActive)
                        throw new ArgumentException(string.Format("Parent OrgUnit '{0}' is inactive.", deptParent.OrgUnitNameTH));
                    if (deptParent.OrgUnitType != 2)
                        throw new ArgumentException(string.Format(
                            "A Department (type 3) can only be created under a Division (type 2). " +
                            "The selected parent '{0}' is type {1}.", deptParent.OrgUnitNameTH, deptParent.OrgUnitType));
                    break;

                default:
                    throw new ArgumentException(string.Format("Unknown OrgUnitType value: {0}.", orgUnitType));
            }
        }

        private void Log(UserContext user, string module, string action, string tableName,
                         int recordId, int result, string newValue = null)
        {
            if (_auditService == null) return;
            _auditService.Log(new AuditEntry
            {
                Module      = module,
                Action      = action,
                UserID      = user.UserID,
                UserName    = user.ADUserName ?? string.Empty,
                UserRole    = user.AppRole,
                TableName   = tableName,
                RecordID    = recordId,
                NewValue    = newValue,
                MachineName = user.MachineName,
                IPAddress   = user.IPAddress,
                SessionID   = user.SessionID,
                Result      = result,
                AuditDate   = DateTime.Now
            });
        }

        public void LinkDivisionToSubCommittee(int divisionId, int subCommitteeId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.AppRole != 4)
                throw new UnauthorizedAccessException("Linking OrgUnits requires Admin role.");

            var division = _orgUnitRepository.GetById(divisionId);
            if (division == null || division.IsDeleted)
                throw new InvalidOperationException(string.Format("Division ID {0} ไม่พบในระบบ", divisionId));
            if (division.OrgUnitType != 2)
                throw new ArgumentException("OrgUnit ที่ระบุต้องเป็น Division");

            var subCommittee = _orgUnitRepository.GetById(subCommitteeId);
            if (subCommittee == null || subCommittee.IsDeleted)
                throw new InvalidOperationException(string.Format("SubCommittee ID {0} ไม่พบในระบบ", subCommitteeId));
            if (subCommittee.OrgUnitType != 1)
                throw new ArgumentException("Parent ที่ระบุต้องเป็น SubCommittee");

            _orgUnitRepository.LinkDivisionToSubCommittee(divisionId, subCommitteeId, user.UserID);

            Log(user, "OrgStructure", "LINK", "EMS.OrgUnitRelationship", divisionId, 1,
                string.Format("Linked Division '{0}' to SubCommittee '{1}'.",
                    division.OrgUnitNameTH, subCommittee.OrgUnitNameTH));
        }

        public IEnumerable<OrgUnitRelationship> GetAllActiveRelationships()
        {
            return _orgUnitRepository.GetAllActiveRelationships();
        }

        public void UnlinkDivisionFromSubCommittee(int divisionId, int subCommitteeId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.AppRole != 4)
                throw new UnauthorizedAccessException("Unlinking OrgUnits requires Admin role.");

            if (!_orgUnitRepository.UnlinkDivisionFromSubCommittee(divisionId, subCommitteeId))
                throw new InvalidOperationException("ไม่พบการเชื่อมโยงระหว่างฝ่ายกับคณะกรรมการนี้ (อาจถูกยกเลิกไปแล้ว)");

            var division = _orgUnitRepository.GetById(divisionId);
            var subCommittee = _orgUnitRepository.GetById(subCommitteeId);
            Log(user, "OrgStructure", "UNLINK", "EMS.OrgUnitRelationship", divisionId, 1,
                string.Format("Unlinked Division '{0}' from SubCommittee '{1}'.",
                    division != null ? division.OrgUnitNameTH : divisionId.ToString(),
                    subCommittee != null ? subCommittee.OrgUnitNameTH : subCommitteeId.ToString()));
        }

        public void DetachDivisionToStandalone(int divisionId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.AppRole != 4)
                throw new UnauthorizedAccessException("Detaching a Division requires Admin role.");

            var division = _orgUnitRepository.GetById(divisionId);
            if (division == null || division.IsDeleted || division.OrgUnitType != (int)OrgUnitTier.Division)
                throw new InvalidOperationException("ไม่พบฝ่ายที่ต้องการแยก");
            if (!division.IsActive)
                throw new InvalidOperationException(string.Format(
                    "ฝ่าย '{0}' ถูกปิดใช้งาน กรุณาเปิดใช้งานก่อน", division.OrgUnitNameTH));

            ValidateNameUnique(division.OrgUnitNameTH, null, division.OrgUnitID);

            var oldParentId = division.ParentOrgUnitID;
            int unlinked = _orgUnitRepository.DetachDivisionToStandalone(divisionId);

            Log(user, "OrgStructure", "MOVE", "EMS.OrgUnit", divisionId, 1,
                string.Format("Detached Division '{0}' ({1}) to standalone: parent {2} → none, {3} SubCommittee link(s) removed.",
                    division.OrgUnitNameTH, division.OrgUnitCode, oldParentId.HasValue ? oldParentId.Value.ToString() : "none", unlinked));
        }

        /// <summary>
        /// Links an existing Division to <paramref name="subCommitteeId"/> and returns it with
        /// <see cref="OrgUnit.LinkedToExisting"/> set. When it already belongs there, nothing changes
        /// and it is returned with <see cref="OrgUnit.AlreadyUnderParent"/> set so the caller can add
        /// the departments it is missing.
        /// </summary>
        private OrgUnit LinkExistingDivision(OrgUnit division, int subCommitteeId, UserContext user)
        {
            if (!division.IsActive)
                throw new InvalidOperationException(string.Format(
                    "ฝ่าย '{0}' ({1}) มีอยู่ในระบบแต่ถูกปิดใช้งาน กรุณาเปิดใช้งานก่อนเชื่อมโยง",
                    division.OrgUnitNameTH, division.OrgUnitCode));

            if (division.ParentOrgUnitID == subCommitteeId
                || _orgUnitRepository.IsDivisionLinkedToSubCommittee(division.OrgUnitID, subCommitteeId))
            {
                division.AlreadyUnderParent = true;
                return division;
            }

            LinkDivisionToSubCommittee(division.OrgUnitID, subCommitteeId, user);
            division.LinkedToExisting = true;

            // Users who see the SubCommittee now see the linked Division and its departments.
            PropagateVisibility(division.OrgUnitID, subCommitteeId, user);
            foreach (var dept in _orgUnitRepository.GetHierarchy()
                         .Where(u => u.OrgUnitType == (int)OrgUnitTier.Department && !u.IsDeleted
                                  && u.ParentOrgUnitID == division.OrgUnitID))
                PropagateVisibility(dept.OrgUnitID, division.OrgUnitID, user);

            return division;
        }

        /// <summary>
        /// Auto-grants visibility of a new/linked OrgUnit (see IUserOrgVisibilityService.PropagateToNewOrgUnit).
        /// Never fails the create/link: a failure is audited and the unit can still be granted manually.
        /// </summary>
        private void PropagateVisibility(int orgUnitId, int? parentOrgUnitId, UserContext user)
        {
            if (_visibilityService == null) return;
            try
            {
                _visibilityService.PropagateToNewOrgUnit(orgUnitId, parentOrgUnitId, user);
            }
            catch (Exception ex)
            {
                Log(user, "OrgStructure", "GRANT", "security.UserOrgVisibility", orgUnitId, 2,
                    "Auto-grant visibility failed: " + ex.Message);
            }
        }

        /// <summary>Server-side code generation with database-level uniqueness guarantee</summary>
        private string GenerateUniqueCode(int orgUnitType)
        {
            string prefix;
            switch (orgUnitType)
            {
                case 1: prefix = "SC";   break;  // SubCommittee
                case 2: prefix = "DIV";  break;  // Division
                default: prefix = "DEPT"; break; // Department
            }

            // Get all existing codes with this prefix from database
            var allUnits = _orgUnitRepository.GetHierarchy();
            var existingCodes = new HashSet<string>(allUnits
                .Where(u => u.OrgUnitCode != null && u.OrgUnitCode.StartsWith(prefix + "-"))
                .Select(u => u.OrgUnitCode));

            // Extract numbers and find the next available
            var existingNumbers = new List<int>();
            foreach (var code in existingCodes)
            {
                var parts = code.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int number))
                {
                    existingNumbers.Add(number);
                }
            }

            int nextNumber = existingNumbers.Count > 0 ? existingNumbers.Max() + 1 : 1;
            
            // Generate and verify uniqueness
            string newCode;
            int attempts = 0;
            do
            {
                newCode = string.Format("{0}-{1:D3}", prefix, nextNumber);
                if (!_orgUnitRepository.ExistsByCode(newCode))
                    break;
                    
                nextNumber++;
                attempts++;
            }
            while (attempts < 100); // Prevent infinite loop

            // Fallback to timestamp if we can't find a unique number
            if (attempts >= 100)
            {
                var timestamp = DateTime.Now.ToString("MMddHHmm");
                newCode = string.Format("{0}-{1}", prefix, timestamp);
            }

            return newCode;
        }
    }
}
