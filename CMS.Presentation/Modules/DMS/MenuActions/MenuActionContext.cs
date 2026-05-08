using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    internal sealed class MenuActionContext
    {
        public Form Owner { get; set; }
        public UserContext User { get; set; }
        public string MenuText { get; set; }
        public Folder SelectedFolder { get; set; }
        public FileDocument SelectedFile { get; set; }
        public List<FileDocument> SelectedFiles { get; set; }

        public IFolderService FolderService { get; set; }
        public IOrgStructureService OrgStructureService { get; set; }
        public IUserAccessOrgLookupController OrgLookupController { get; set; }
        public IApprovalService ApprovalService { get; set; }
        public IFolderTemplateController FolderTemplateController { get; set; }
        public IPermissionService PermissionService { get; set; }
        public IFolderSettingsController FolderSettingsController { get; set; }
        public IFolderPermissionLookupController FolderPermissionLookupController { get; set; }
        public IAuditService AuditService { get; set; }
        public IShareCommandController ShareCommandController { get; set; }
        public IShareManagementController ShareManagementController { get; set; }
        public IUserAccountLookupController UserAccountLookupController { get; set; }
        public IUserAdminController UserAdminController { get; set; }
        public IUserOrgVisibilityController UserOrgVisibilityController { get; set; }
            public IFolderReadLookupController FolderReadLookupController { get; set; }
        public IMenuManagementController MenuManagementController { get; set; }
        public IApprovalInboxController ApprovalInboxController { get; set; }
        public IRecycleBinController RecycleBinController { get; set; }
        public IShareLookupController ShareLookupController { get; set; }
        public IEmployeeApiService EmployeeApiService { get; set; }

        public Action SwitchToOrgView { get; set; }
        public Action SwitchToMainMenuView { get; set; }
        public Action TriggerArchive { get; set; }
        public Action TriggerChangeLevel { get; set; }
        public Action TriggerCopyStructure { get; set; }
        public Action ClearContentPanel { get; set; }
        public Action<string> SetContentInfo { get; set; }
        public Func<string, bool> RequestMdiAction { get; set; }
        public object KnowledgeBaseService { get; internal set; }
    }
}