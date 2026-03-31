using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Presenters
{
    public class OrgStructurePresenter
    {
        private readonly IOrgStructureService _service;
        private readonly IOrgStructureView    _view;
        private readonly UserContext          _user;

        private bool _isConfidentialityView;

        public OrgStructurePresenter(
            IOrgStructureService service,
            IOrgStructureView    view,
            UserContext          user)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (view    == null) throw new ArgumentNullException("view");
            if (user    == null) throw new ArgumentNullException("user");

            _service = service;
            _view    = view;
            _user    = user;
        }

        public void LoadOrganizationView()
        {
            try
            {
                IEnumerable<OrgUnit> units = _service.GetHierarchy();
                var unitsList = units.ToList();
                
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"LoadOrganizationView: Loaded {unitsList.Count} units");
                var divisionCount = unitsList.Count(u => u.Tier == OrgUnitTier.Division && u.IsActive);
                var subCommitteeCount = unitsList.Count(u => u.Tier == OrgUnitTier.SubCommittee && u.IsActive);
                System.Diagnostics.Debug.WriteLine($"Active SubCommittees: {subCommitteeCount}, Active Divisions: {divisionCount}");
                
                _view.ShowOrganizationView(unitsList);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to load Organization View: " + ex.Message);
            }
        }

        public void LoadConfidentialityView()
        {
            try
            {
                // Aggregate all units across all confidentiality levels
                var all = _service.GetHierarchy();
                _view.ShowConfidentialityView(all);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to load Confidentiality View: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates a new OrgUnit. Accepts the friendly OrgUnitTier enum and single name.
        /// </summary>
        public void CreateOrgUnit(string name, string code, OrgUnitTier tier, int? parentId)
        {
            try
            {
                var request = new CreateOrgUnitRequest
                {
                    OrgUnitCode     = code,
                    OrgUnitNameTH   = name,
                    OrgUnitNameEN   = name,
                    OrgUnitType     = (int)tier,
                    ParentOrgUnitID = parentId
                };
                _service.CreateOrgUnit(request, _user);
                _view.ShowInfo(string.Format("สร้างหน่วยงาน '{0}' สำเร็จ", name));
                RefreshCurrentView();
            }
            catch (ArgumentException ex)
            {
                _view.ShowError("Validation error: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to create OrgUnit: " + ex.Message);
            }
        }

        /// <summary>Creates a new OrgUnit from a CreateOrgUnitRequest DTO.</summary>
        public void CreateOrgUnit(CreateOrgUnitRequest request)
        {
            try
            {
                if (request == null) throw new ArgumentNullException("request");
                _service.CreateOrgUnit(request, _user);
                _view.ShowInfo(string.Format("สร้างหน่วยงาน '{0}' สำเร็จ", request.OrgUnitNameTH));
                RefreshCurrentView();
            }
            catch (ArgumentException ex)
            {
                _view.ShowError("Validation error: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to create OrgUnit: " + ex.Message);
            }
        }

        /// <summary>Updates the name of an existing OrgUnit.</summary>
        public void UpdateOrgUnit(int orgUnitId, string newName)
        {
            try
            {
                var request = new UpdateOrgUnitRequest
                {
                    OrgUnitID     = orgUnitId,
                    OrgUnitNameTH = newName,
                    OrgUnitNameEN = newName
                };
                _service.UpdateOrgUnit(request, _user);
                _view.ShowInfo(string.Format("OrgUnit was renamed to ''{0}'' successfully.", newName));
                RefreshCurrentView();
            }
            catch (ArgumentException ex)
            {
                _view.ShowError("Validation error: " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError("Cannot update OrgUnit: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to update OrgUnit: " + ex.Message);
            }
        }

        public void DeactivateOrgUnit(int orgUnitId)
        {
            try
            {
                _service.DeactivateOrgUnit(orgUnitId, _user);
                _view.ShowInfo("OrgUnit was deactivated successfully.");
                RefreshCurrentView();
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to deactivate OrgUnit: " + ex.Message);
            }
        }

        public void DeleteOrgUnit(int orgUnitId)
        {
            try
            {
                _service.DeleteOrgUnit(orgUnitId, _user);
                _view.ShowInfo("หน่วยงานถูกลบเรียบร้อยแล้ว");
                RefreshCurrentView();
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowError("ไม่มีสิทธิ์ในการลบหน่วยงาน: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("ไม่สามารถลบหน่วยงานได้: " + ex.Message);
            }
        }

        /// <summary>Moves an OrgUnit to a new parent.</summary>
        public void MoveOrgUnit(int orgUnitId, int newParentId)
        {
            try
            {
                var request = new UpdateOrgUnitRequest
                {
                    OrgUnitID     = orgUnitId,
                    ParentOrgUnitID = newParentId
                };
                _service.MoveOrgUnit(request, _user);
                _view.ShowInfo("OrgUnit was moved successfully.");
                RefreshCurrentView();
            }
            catch (ArgumentException ex)
            {
                _view.ShowError("Validation error: " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError("Cannot move OrgUnit: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to move OrgUnit: " + ex.Message);
            }
        }

        public void SetActiveView(bool isConfidentialityView)
        {
            _isConfidentialityView = isConfidentialityView;
        }

        public IEnumerable<OrgUnit> GetAllUnits()
        {
            try
            {
                return _service.GetHierarchy();
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to get all units: " + ex.Message);
                return new List<OrgUnit>();
            }
        }

        private void RefreshCurrentView()
        {
            if (_isConfidentialityView)
                LoadConfidentialityView();
            else
                LoadOrganizationView();
        }
    }
}