using System;
using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    /// <summary>
    /// Presenter (Controller) สำหรับ Project.
    /// Mediates between IProjectView และ IFolderService.
    /// Pattern: Form (View) --> ProjectPresenter (Controller) --> IFolderService (Service) --> Repository --> DB
    /// </summary>
    public class ProjectPresenter
    {
        private readonly IFolderService _folderService;
        private readonly IProjectView   _view;
        private readonly UserContext    _user;

        public ProjectPresenter(
            IFolderService folderService,
            IProjectView   view,
            UserContext    user)
        {
            if (folderService == null) throw new ArgumentNullException("folderService");
            if (view          == null) throw new ArgumentNullException("view");
            if (user          == null) throw new ArgumentNullException("user");

            _folderService = folderService;
            _view          = view;
            _user          = user;
        }

        // -----------------------------------------------------------------------
        // Load
        // -----------------------------------------------------------------------

        /// <summary>โหลดรายการ Project ทั้งหมดที่ User มีสิทธิ์เข้าถึง</summary>
        public void LoadProjects(int? orgUnitId = null)
        {
            try
            {
                var projects = _folderService.GetAllFolders(orgUnitId, _user);
                _view.ShowProjects(projects);
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowError("ไม่มีสิทธิ์เข้าถึง: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลด Project ไม่สำเร็จ: " + ex.Message);
            }
        }

        // -----------------------------------------------------------------------
        // CRUD
        // -----------------------------------------------------------------------

        /// <summary>สร้าง Project ใหม่</summary>
        public void CreateProject(string name, int orgUnitId, int confidentialityLevel = 4)
        {
            try
            {
                var request = new CMS.Services.DTOs.CreateFolderRequest
                {
                    ParentFolderId       = null,
                    OrgUnitId            = orgUnitId,
                    Name                 = name,
                    ConfidentialityLevel = confidentialityLevel
                };
                _folderService.CreateFolder(request, _user);
                _view.ShowInfo(string.Format("Project '{0}' สร้างสำเร็จ", name));
                LoadProjects(orgUnitId);
            }
            catch (ArgumentException ex)
            {
                _view.ShowError("ข้อมูลไม่ถูกต้อง: " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError("ไม่สามารถสร้าง Project ได้: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("เกิดข้อผิดพลาด: " + ex.Message);
            }
        }

        /// <summary>เปลี่ยนชื่อ Project</summary>
        public void RenameProject(int folderId, string newName, int? orgUnitId = null)
        {
            try
            {
                _folderService.RenameFolder(folderId, newName, _user);
                _view.ShowInfo(string.Format("เปลี่ยนชื่อ Project เป็น '{0}' สำเร็จ", newName));
                LoadProjects(orgUnitId);
            }
            catch (ArgumentException ex)
            {
                _view.ShowError("ข้อมูลไม่ถูกต้อง: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("ไม่สามารถเปลี่ยนชื่อ Project ได้: " + ex.Message);
            }
        }

        /// <summary>ลบ Project (soft delete)</summary>
        public void DeleteProject(int folderId, int? orgUnitId = null)
        {
            try
            {
                _folderService.DeleteFolder(folderId, _user);
                _view.ShowInfo("ลบ Project เรียบร้อยแล้ว");
                LoadProjects(orgUnitId);
            }
            catch (InvalidOperationException ex)
            {
                _view.ShowError("ไม่สามารถลบ Project ได้: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("เกิดข้อผิดพลาดในการลบ Project: " + ex.Message);
            }
        }
    }
}
