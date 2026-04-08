using System;
using System.Windows.Forms;
using CMS.Data.Interfaces;
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

        public IFolderService FolderService { get; set; }
        public IFileService FileService { get; set; }
        public IOrgStructureService OrgStructureService { get; set; }
        public IApprovalService ApprovalService { get; set; }
        public IFolderTemplateRepository FolderTemplateRepository { get; set; }
        public IQuickAccessService QuickAccessService { get; set; }
        public IPermissionService PermissionService { get; set; }
        public IFolderRepository FolderRepository { get; set; }
        public IPermissionRepository PermissionRepository { get; set; }
        public IAuditService AuditService { get; set; }
        public IShareService ShareService { get; set; }
        public IUserAccountRepository UserAccountRepository { get; set; }
        public IMenuService MenuService { get; set; }

        public Action SwitchToOrgView { get; set; }
        public Action SwitchToMainMenuView { get; set; }
        public Action TriggerArchive { get; set; }
        public Action TriggerChangeLevel { get; set; }
        public Action TriggerCopyStructure { get; set; }
        public Action ClearContentPanel { get; set; }
        public Action<string> SetContentInfo { get; set; }
        public Func<string, bool> RequestMdiAction { get; set; }
    }
}