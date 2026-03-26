using System;
using System.Collections.Generic;
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
        private readonly IOrgUnitRepository _orgUnitRepository;
        private readonly IAuditService      _auditService;

        public OrgStructureService(IOrgUnitRepository orgUnitRepository, IAuditService auditService = null)
        {
            if (orgUnitRepository == null) throw new ArgumentNullException("orgUnitRepository");
            _orgUnitRepository = orgUnitRepository;
            _auditService      = auditService;
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

            ValidateParentTier(request.OrgUnitType, request.ParentOrgUnitID);

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

            int newId = _orgUnitRepository.Insert(orgUnit);
            orgUnit.OrgUnitID = newId;

            Log(user, "OrgStructure", "CREATE", "core.OrgUnit", newId, 1,
                string.Format("Created OrgUnit '{0}' (type {1}).", orgUnit.OrgUnitNameTH, orgUnit.OrgUnitType));

            return orgUnit;
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

            _orgUnitRepository.Update(existing);

            Log(user, "OrgStructure", "UPDATE", "core.OrgUnit", existing.OrgUnitID, 1,
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

            var oldParentId = existing.ParentOrgUnitID;
            existing.ParentOrgUnitID = request.ParentOrgUnitID;

            _orgUnitRepository.Update(existing);

            Log(user, "OrgStructure", "MOVE", "core.OrgUnit", existing.OrgUnitID, 1,
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

            _orgUnitRepository.SoftDelete(orgUnitId, user.UserID);

            Log(user, "OrgStructure", "DELETE", "core.OrgUnit", orgUnitId, 1,
                string.Format("Deactivated OrgUnit '{0}'.", existing.OrgUnitNameTH));
        }

        public IEnumerable<OrgUnit> GetHierarchy()
        {
            return _orgUnitRepository.GetHierarchy();
        }

        public IEnumerable<OrgUnit> GetByConfidentialityLevel(ConfidentialityLevel level)
        {
            return _orgUnitRepository.GetByConfidentialityLevel(level);
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        private void ValidateParentTier(int orgUnitType, int? parentId)
        {
            switch (orgUnitType)
            {
                case 1: // SubCommittee — no parent
                    if (parentId.HasValue)
                        throw new ArgumentException("A Sub-Committee (type 1) must not have a parent OrgUnit.");
                    break;

                case 2: // Division — parent must be SubCommittee (1)
                    if (!parentId.HasValue)
                        throw new ArgumentException("A Division (type 2) requires a parent Sub-Committee.");

                    var divParent = _orgUnitRepository.GetById(parentId.Value);
                    if (divParent == null)
                        throw new ArgumentException(string.Format("Parent OrgUnit with ID {0} does not exist.", parentId.Value));
                    if (!divParent.IsActive)
                        throw new ArgumentException(string.Format("Parent OrgUnit '{0}' is inactive.", divParent.OrgUnitNameTH));
                    if (divParent.OrgUnitType != 1)
                        throw new ArgumentException(string.Format(
                            "A Division (type 2) can only be created under a Sub-Committee (type 1). " +
                            "The selected parent '{0}' is type {1}.", divParent.OrgUnitNameTH, divParent.OrgUnitType));
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
            try
            {
                _auditService.Log(new AuditEntry
                {
                    Module      = module,
                    Action      = action,
                    UserID      = user.UserID,
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
            catch { }
        }
    }
}
