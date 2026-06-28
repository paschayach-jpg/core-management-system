using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
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
                var levels = new[]
                {
                    ConfidentialityLevel.HC,
                    ConfidentialityLevel.SC,
                    ConfidentialityLevel.NC,
                    ConfidentialityLevel.PC
                };

                var allGrouped = new List<OrgUnit>();
                foreach (var level in levels)
                {
                    try
                    {
                        var units = _service.GetByConfidentialityLevel(level);
                        foreach (var u in units)
                        {
                            u.ConfidentialityLevelValue = (int)level;
                            allGrouped.Add(u);
                        }
                    }
                    catch
                    {
                        // SP อาจยังไม่มีใน DB — ข้ามไป
                    }
                }

                if (allGrouped.Count == 0)
                {
                    var all = _service.GetHierarchy().ToList();
                    foreach (var u in all.Where(x => x.IsActive))
                    {
                        var code = u.OrgUnitCode ?? u.Name ?? "";
                        if      (code.StartsWith("1HC", StringComparison.OrdinalIgnoreCase)) u.ConfidentialityLevelValue = (int)ConfidentialityLevel.HC;
                        else if (code.StartsWith("2SC", StringComparison.OrdinalIgnoreCase)) u.ConfidentialityLevelValue = (int)ConfidentialityLevel.SC;
                        else if (code.StartsWith("3NC", StringComparison.OrdinalIgnoreCase)) u.ConfidentialityLevelValue = (int)ConfidentialityLevel.NC;
                        else if (code.StartsWith("4PC", StringComparison.OrdinalIgnoreCase)) u.ConfidentialityLevelValue = (int)ConfidentialityLevel.PC;
                        allGrouped.Add(u);
                    }
                }

                _view.ShowConfidentialityView(allGrouped);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to load Confidentiality View: " + ex.Message);
            }
        }

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
            catch (ArgumentException ex) { _view.ShowError("Validation error: " + ex.Message); }
            catch (Exception ex)         { _view.ShowError("Failed to create OrgUnit: " + ex.Message); }
        }

        public void CreateOrgUnit(CreateOrgUnitRequest request)
        {
            try
            {
                if (request == null) throw new ArgumentNullException("request");
                _service.CreateOrgUnit(request, _user);
                _view.ShowInfo(string.Format("สร้างหน่วยงาน '{0}' สำเร็จ", request.OrgUnitNameTH));
                RefreshCurrentView();
            }
            catch (ArgumentException ex) { _view.ShowError("Validation error: " + ex.Message); }
            catch (Exception ex)         { _view.ShowError("Failed to create OrgUnit: " + ex.Message); }
        }

        public void CreateOrgUnitBatch(IEnumerable<CreateOrgUnitRequest> requests)
        {
            if (requests == null) return;
            var list = requests.Where(r => r != null).ToList();
            if (list.Count == 0) return;

            var saved  = new List<string>();
            var failed = new List<string>();

            foreach (var req in list)
            {
                try
                {
                    _service.CreateOrgUnit(req, _user);
                    saved.Add(req.OrgUnitNameTH ?? req.OrgUnitCode);
                }
                catch (Exception ex)
                {
                    failed.Add(string.Format("{0}: {1}", req.OrgUnitCode ?? req.OrgUnitNameTH, ex.Message));
                }
            }

            var sb = new System.Text.StringBuilder();
            if (saved.Count > 0)
                sb.AppendFormat("บันทึกสำเร็จ {0} รายการ:\n  • {1}\n",
                    saved.Count, string.Join("\n  • ", saved));
            if (failed.Count > 0)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.AppendFormat("ล้มเหลว {0} รายการ:\n  • {1}",
                    failed.Count, string.Join("\n  • ", failed));
            }

            if (saved.Count > 0)
                _view.ShowInfo(sb.ToString());
            else if (failed.Count > 0)
                _view.ShowError(sb.ToString());

            RefreshCurrentView();
        }

        // สร้าง Division ก่อน ได้ ID จริง แล้วค่อยสร้าง Departments ที่ผูกกับ Division นั้น
        public void CreateDivisionWithDepartments(
            CreateOrgUnitRequest divisionRequest,
            IList<CreateOrgUnitRequest> departmentRequests)
        {
            if (divisionRequest == null) return;

            OrgUnit newDivision;
            try
            {
                newDivision = _service.CreateOrgUnit(divisionRequest, _user);
            }
            catch (Exception ex)
            {
                _view.ShowError(string.Format("ไม่สามารถสร้างฝ่าย '{0}': {1}",
                    divisionRequest.OrgUnitNameTH, ex.Message));
                return;
            }

            var saved  = new List<string> { divisionRequest.OrgUnitNameTH ?? divisionRequest.OrgUnitCode };
            var failed = new List<string>();

            var depts = departmentRequests?.Where(r => r != null).ToList()
                        ?? new List<CreateOrgUnitRequest>();

            foreach (var deptReq in depts)
            {
                deptReq.ParentOrgUnitID = newDivision.OrgUnitId;
                try
                {
                    _service.CreateOrgUnit(deptReq, _user);
                    saved.Add(deptReq.OrgUnitNameTH ?? deptReq.OrgUnitCode);
                }
                catch (Exception ex)
                {
                    failed.Add(string.Format("{0}: {1}", deptReq.OrgUnitCode ?? deptReq.OrgUnitNameTH, ex.Message));
                }
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendFormat("สร้างสำเร็จ {0} หน่วยงาน:\n  • {1}\n",
                saved.Count, string.Join("\n  • ", saved));
            if (failed.Count > 0)
            {
                sb.AppendLine();
                sb.AppendFormat("ล้มเหลว {0} รายการ:\n  • {1}",
                    failed.Count, string.Join("\n  • ", failed));
                _view.ShowError(sb.ToString());
            }
            else
            {
                _view.ShowInfo(sb.ToString());
            }

            RefreshCurrentView();
        }

        public IEnumerable<CMS.Domain.Models.OrgUnitRelationship> LoadRelationships()
        {
            try   { return _service.GetAllActiveRelationships(); }
            catch { return new List<CMS.Domain.Models.OrgUnitRelationship>(); }
        }

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
            catch (ArgumentException ex)         { _view.ShowError("Validation error: " + ex.Message); }
            catch (InvalidOperationException ex) { _view.ShowError("Cannot update OrgUnit: " + ex.Message); }
            catch (Exception ex)                 { _view.ShowError("Failed to update OrgUnit: " + ex.Message); }
        }

        public void DeactivateOrgUnit(int orgUnitId)
        {
            try
            {
                _service.DeactivateOrgUnit(orgUnitId, _user);
                _view.ShowInfo("OrgUnit was deactivated successfully.");
                RefreshCurrentView();
            }
            catch (InvalidOperationException ex) { _view.ShowError(ex.Message); }
            catch (Exception ex)                 { _view.ShowError("Failed to deactivate OrgUnit: " + ex.Message); }
        }

        public bool HasActiveDependents(int orgUnitId)
        {
            try   { return _service.HasActiveDependents(orgUnitId); }
            catch { return false; }
        }

        public void DeleteOrgUnit(int orgUnitId)
        {
            try
            {
                _service.DeleteOrgUnit(orgUnitId, _user);
                _view.ShowInfo("หน่วยงานถูกลบเรียบร้อยแล้ว");
                RefreshCurrentView();
            }
            catch (InvalidOperationException ex)   { _view.ShowError(ex.Message); }
            catch (UnauthorizedAccessException ex) { _view.ShowError("ไม่มีสิทธิ์ในการลบหน่วยงาน: " + ex.Message); }
            catch (Exception ex)                   { _view.ShowError("ไม่สามารถลบหน่วยงานได้: " + ex.Message); }
        }

        public void ForceDeleteOrgUnit(int orgUnitId)
        {
            try
            {
                _service.ForceDeleteOrgUnit(orgUnitId, _user);
                _view.ShowInfo("ลบหน่วยงานพร้อมข้อมูลที่เกี่ยวข้องเรียบร้อยแล้ว");
                RefreshCurrentView();
            }
            catch (UnauthorizedAccessException ex) { _view.ShowError("ไม่มีสิทธิ์ในการลบหน่วยงาน: " + ex.Message); }
            catch (Exception ex)                   { _view.ShowError("ไม่สามารถลบหน่วยงานได้: " + ex.Message); }
        }

        public void MoveOrgUnit(int orgUnitId, int newParentId)
        {
            try
            {
                var request = new UpdateOrgUnitRequest
                {
                    OrgUnitID       = orgUnitId,
                    ParentOrgUnitID = newParentId
                };
                _service.MoveOrgUnit(request, _user);
                _view.ShowInfo("OrgUnit was moved successfully.");
                RefreshCurrentView();
            }
            catch (ArgumentException ex)         { _view.ShowError("Validation error: " + ex.Message); }
            catch (InvalidOperationException ex) { _view.ShowError("Cannot move OrgUnit: " + ex.Message); }
            catch (Exception ex)                 { _view.ShowError("Failed to move OrgUnit: " + ex.Message); }
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

        public IEnumerable<int> GetHiddenOrgUnitIds()
        {
            try   { return _service.GetHiddenOrgUnitIds(_user.UserID); }
            catch { return new List<int>(); }
        }

        public void SaveHiddenOrgUnitIds(IEnumerable<int> orgUnitIds)
        {
            try { _service.SaveHiddenOrgUnitIds(_user.UserID, orgUnitIds); }
            catch { /* ไม่ block UI ถ้า save ล้มเหลว */ }
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
