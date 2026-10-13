using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CMS.Data.Repositories;
using CMS.Controllers;
using CMS.Controllers.DMS.Document;
using CMS.Domain.Models;
using CMS.Infrastructure;
using CMS.Services;
using CMS.Presentation.Services;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            var loggingService = new LoggingService();

            try
            {
                // --- Infrastructure / repositories ---
                var configManager = new ConfigManager();
                var adHelper = new ActiveDirectoryHelper();
                var userAccountRepo = new UserAccountRepository(configManager);
                var auditRepository = new AuditRepository(configManager);
                var notificationRepository = new NotificationRepository(configManager);
                var approvalRepository = new ApprovalRepository(configManager);
                var folderRepository = new FolderRepository(configManager);
                var permissionRepository = new PermissionRepository(configManager);
                var orgUnitRepository = new OrgUnitRepository(configManager);
                var hrDivisionRepository = new HrDivisionRepository(configManager);
                var hrDepartmentRepository = new HrDepartmentRepository(configManager);
                var hrPositionRepository = new HrPositionRepository(configManager);
                var hrPositionLevelRepository = new HrPositionLevelRepository(configManager);
                var hrPositionMappingRepository = new HrPositionMappingRepository(configManager);

                var fileDocumentRepository = new FileDocumentRepository(configManager);
                var filePermissionRepository = new FilePermissionRepository(configManager);
                var folderTemplateRepository = new FolderTemplateRepository(configManager);
                var quickAccessRepository = new QuickAccessRepository(configManager);
                var externalShareRepository = new ExternalShareRepository(configManager);
                var shareAccessLogRepository = new ShareAccessLogRepository(configManager);
                var shareTokenRepository     = new ShareTokenRepository(configManager);
                var kbDashboardRepository    = new KBDashboardRepository(configManager);
                var kbCategoryRepository    = new KBCategoryRepository(configManager);
                var kbArticleRepository     = new KBArticleRepository(configManager);
                var kbImportTemplateRepository = new KBImportTemplateRepository(configManager);
                var kbArticleDisplayProfileRepository = new KBArticleDisplayProfileRepository(configManager);
                var kbVersionRepository     = new KBVersionRepository(configManager);
                var kbArticleLinkRepository = new KBArticleLinkRepository(configManager);
                var kbArticleAttachmentRepository = new KBArticleAttachmentRepository(configManager);
                var subCommitteeRepository  = new SubCommitteeRepository(configManager);
                var folderQuotaConfigRepo   = new CMS.Data.Repositories.FolderQuotaConfigRepository(configManager);
                var folderQuotaRequestRepo  = new CMS.Data.Repositories.FolderQuotaChangeRequestRepository(configManager);

                var auditService = new AuditService(auditRepository, loggingService);
                var notificationService = new NotificationService(notificationRepository, loggingService);
                var permissionService = new PermissionService(permissionRepository, folderRepository, userAccountRepo, auditService, fileDocumentRepository, filePermissionRepository);
                var filePermissionService = new FilePermissionService(filePermissionRepository, auditService);

                var userOrgVisibilityRepo = new UserOrgVisibilityRepository(configManager);
                var userOrgVisibilityService = new UserOrgVisibilityService(userOrgVisibilityRepo, orgUnitRepository, auditService);

                var orgStructureService = new OrgStructureService(orgUnitRepository, auditService, userOrgVisibilityService);

                var networkShareConnector = new NetworkShareConnector(configManager);
                var networkDriveHelper = new NetworkDriveHelper(configManager, networkShareConnector);
                var clamAvAdapter = new ClamAVAdapter(configManager);
                var virusScanService = new VirusScanService(
                    clamAvAdapter,
                    loggingService,
                    notificationRepository,
                    userAccountRepo,
                    auditService,
                    configManager);

                var quotaService = new CMS.Services.FolderQuotaService(
                    folderQuotaConfigRepo,
                    folderQuotaRequestRepo,
                    folderRepository,
                    notificationService,
                    orgUnitRepository,
                    fileDocumentRepository);

                var folderService = new FolderService(
                    folderRepository,
                    fileDocumentRepository,
                    permissionRepository,
                    permissionService,
                    networkDriveHelper,
                    auditService,
                    folderTemplateRepository,
                    approvalRepository,
                    userOrgVisibilityService,
                    userAccountRepo,
                    quotaService);

                var folderAuthorityRepo = new FolderAuthorityRepository(configManager);
                var folderAuthorityService = new FolderAuthorityService(folderAuthorityRepo);

                // ผู้อนุมัติดาวน์โหลดที่ Admin ตั้งไว้ต่อแผนก/ฝ่าย
                var downloadApproverConfigRepo = new DownloadApproverConfigRepository(configManager);
                var downloadApproverConfigController = new DownloadApproverConfigController(
                    new DownloadApproverConfigService(downloadApproverConfigRepo, auditService));

                // ผู้อนุมัติตามแผนกเจ้าของโฟลเดอร์ (CanApprove ใน FolderAuthorityConfig / config ดาวน์โหลด)
                var approverResolver = new ApproverResolver(
                    folderRepository,
                    fileDocumentRepository,
                    orgUnitRepository,
                    userAccountRepo,
                    folderAuthorityService,
                    loggingService,
                    downloadApproverConfigRepo,
                    hrDepartmentRepository,
                    hrDivisionRepository);

                var approvalService = new ApprovalService(
                    approvalRepository,
                    folderRepository,
                    folderService,
                    permissionService,
                    notificationRepository,
                    userAccountRepo,
                    auditService,
                    loggingService,
                    externalShareRepository,
                    shareTokenRepository,
                    filePermissionRepository,
                    approverResolver);

                var fileService = new FileService(
                    fileDocumentRepository,
                    folderRepository,
                    orgUnitRepository,
                    virusScanService,
                    permissionService,
                    networkDriveHelper,
                    auditService,
                    notificationRepository,
                    userAccountRepo,
                    loggingService,
                    permissionRepository,
                    filePermissionRepository,
                    quotaService);

                // สิทธิ์ดาวน์โหลดได้ทันที (Admin / รายคน / Authority Config CanDownload) หรือต้องขออนุมัติ
                var downloadPolicyController = new DownloadPolicyController(new DownloadPolicyService(
                    new DownloadUserPermissionRepository(configManager),
                    fileDocumentRepository,
                    folderRepository,
                    approvalService,
                    fileService,
                    auditService,
                    loggingService,
                    userAccountRepo));

                var employeeApiService = new EmployeeApiService(configManager);
                var uploadCheckpointService = new UploadCheckpointService(loggingService);
                var quickAccessService = new QuickAccessService(quickAccessRepository, folderRepository);

                // ── File Sharing Services ─────────────────────────────────────
                var shareTokenService = new ShareTokenService(shareTokenRepository, loggingService);
                var shareAccessService = new ShareAccessService(
                    externalShareRepository,
                    shareAccessLogRepository,
                    shareTokenService,
                    fileDocumentRepository,
                    folderRepository,
                    auditService,
                    notificationService,
                    loggingService);

                var shareService = new ShareService(
                    externalShareRepository,
                    shareTokenRepository,
                    shareAccessLogRepository,
                    fileDocumentRepository,
                    folderRepository,
                    filePermissionRepository,
                    permissionRepository,
                    shareTokenService,
                    permissionService,
                    notificationService,
                    auditService,
                    loggingService,
                    approvalRepository,
                    userAccountRepo,
                    approverResolver);

                // ── KB Services (MOD-006) ─────────────────────────────────────
                var kbCategoryService = new KBCategoryService(kbCategoryRepository, auditService);
                var kbArticleService  = new KBArticleService(kbArticleRepository, kbVersionRepository, kbArticleLinkRepository, kbArticleAttachmentRepository, fileDocumentRepository, auditService);
                var kbImportTemplateService = new KBImportTemplateService(kbImportTemplateRepository, kbArticleRepository, kbCategoryRepository, auditService);
                var kbArticleDisplayProfileService = new KBArticleDisplayProfileService(kbArticleDisplayProfileRepository, kbCategoryRepository, auditService);
                var excelTemplatePreviewService = new ExcelTemplatePreviewService();
                var kbDashboardService = new KBDashboardService(kbDashboardRepository, loggingService);

                // ── Workflow Engine Services (MOD-008) ────────────────────────
                var workflowRepository = new WorkflowRepository(configManager);
                var workflowTemplateRepository = new WorkflowTemplateRepository(configManager);
                var workflowStepRepository = new WorkflowStepRepository(configManager);
                var workflowStepInstanceRepository = new WorkflowStepInstanceRepository(configManager);
                var delegationRepository = new DelegationRepository(configManager);
                var wfReportRepository = new WFReportRepository(configManager);

                var conditionEvaluatorService = new ConditionEvaluatorService(loggingService);
                var workflowNotificationService = new WorkflowNotificationService(auditService, loggingService);
                var slaCalculator = new SLACalculator(workflowStepInstanceRepository, wfReportRepository, workflowNotificationService, auditService, loggingService);
                var stepResolverService = new StepResolverService(workflowStepRepository, conditionEvaluatorService, loggingService);
                var workflowDelegationService = new WorkflowDelegationService(delegationRepository, workflowStepInstanceRepository, workflowNotificationService, auditService, loggingService);

                var workflowService = new WorkflowService(
                    workflowRepository,
                    workflowTemplateRepository,
                    workflowStepRepository,
                    workflowStepInstanceRepository,
                    stepResolverService,
                    slaCalculator,
                    workflowNotificationService,
                    auditService,
                    loggingService);

                IWorkflowTemplateService workflowTemplateService = null;
                try
                {
                    workflowTemplateService = new WorkflowTemplateService(
                        workflowTemplateRepository,
                        workflowRepository,
                        auditService,
                        loggingService);
                    loggingService.Info("workflowTemplateService created successfully");
                }
                catch (Exception ex)
                {
                    loggingService.Error($"ERROR creating workflowTemplateService: {ex.Message}\n{ex.StackTrace}", ex);
                    MessageBox.Show(
                        $"WARNING: Cannot create WorkflowTemplateService\n\n{ex.GetType().Name}\n{ex.Message}\n\nWorkflow features will be unavailable.\n\nDetails:\n{ex.StackTrace}",
                        "WARNING",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    // Don't throw - allow app to continue with workflowTemplateService = null
                }

                // ── AD Auth Service ───────────────────────────────────────────
                var adAuthService = new AdAuthService(configManager);

                // ── Menu API Service + Controller ─────────────────────────────
                var menuApiService  = new MenuApiService(configManager);
                var menuController  = new MenuController(menuApiService);

                // ─────────────────────────────────────────────────────────────
                // Parse command-line arguments
                // รูปแบบ:
                //   CMS.Presentation.exe /u:username /p:password   → login ด้วย credentials
                //   CMS.Presentation.exe /token:eyJhbGci...        → validate token จากโปรแกรมอื่น
                //   CMS.Presentation.exe (ไม่มี arg)               → แสดง LoginForm
                // ─────────────────────────────────────────────────────────────
                var startup = StartupArgs.Parse(args);

                AdAuthResult authResult = null;

                if (startup.Mode == AuthMode.Credentials)
                {
                    // โปรแกรมอื่นส่ง username/password มา — login ทันที
                    loggingService.Info("Startup mode: Credentials (from command-line)");
                    authResult = adAuthService.LoginAsync(startup.Username, startup.Password)
                                              .GetAwaiter().GetResult();

                    if (!authResult.Success)
                    {
                        MessageBox.Show(
                            "ข้อมูล Username/Password ที่ส่งมาไม่ถูกต้อง\n\n" + authResult.ErrorMessage,
                            "CMS - Authentication Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // โหลด MenuActions เหมือน Manual login
                    try
                    {
                        var mc = new MenuControl(menuController);
                        GlobalData.UserMenuModel = mc.Request(authResult.AccessToken).GetAwaiter().GetResult();
                    }
                    catch (Exception ex) { loggingService.Error("MenuControl.Request failed (Credentials mode): " + ex.Message, ex); }
                }
                else if (startup.Mode == AuthMode.Token)
                {
                    // โปรแกรมอื่นส่ง token มา — validate แล้วดึงข้อมูลพนักงาน
                    loggingService.Info("Startup mode: Token (from command-line)");
                    authResult = adAuthService.ValidateTokenAsync(startup.Token)
                                              .GetAwaiter().GetResult();

                    if (!authResult.Success)
                    {
                        MessageBox.Show(
                            "Token ที่ส่งมาไม่ valid หรือหมดอายุ\n\n" + authResult.ErrorMessage,
                            "CMS - Authentication Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // โหลด MenuActions เหมือน Manual login
                    try
                    {
                        var mc = new MenuControl(menuController);
                        GlobalData.UserMenuModel = mc.Request(authResult.AccessToken).GetAwaiter().GetResult();
                    }
                    catch (Exception ex) { loggingService.Error("MenuControl.Request failed (Token mode): " + ex.Message, ex); }
                }
                else
                {
                    // เปิดตรง — แสดง LoginForm ให้กรอก
                    loggingService.Info("Startup mode: Manual login (no args)");
                    using (var loginForm = new LoginForm(new LoginAuthController(adAuthService), new LoginAccountController(userAccountRepo), menuController))
                    {
                        if (loginForm.ShowDialog() != DialogResult.OK)
                            return; // ผู้ใช้ปิด form

                        authResult = loginForm.AuthResult;
                        startup.Username = loginForm.EnteredUsername;
                        // GlobalData.MenuActions ถูก set แล้วใน LoginForm.btnLogin_Click
                    }
                }

                // ─────────────────────────────────────────────────────────────
                // ค้นหา UserAccount จาก DB
                // ใช้ AdUserName จาก authResult ถ้ามี ไม่งั้นใช้ username ที่กรอก
                // ─────────────────────────────────────────────────────────────
                string adUserName = !string.IsNullOrEmpty(authResult.AdUserName)
                    ? authResult.AdUserName
                    : startup.Username;

                if (string.IsNullOrEmpty(adUserName))
                {
                    MessageBox.Show(
                        "ไม่สามารถระบุตัวตนผู้ใช้ได้ กรุณาติดต่อผู้ดูแลระบบ",
                        "CMS - Authentication Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Establish authenticated SMB session (connector already injected into networkDriveHelper).
                string networkShareMessage;
                if (!networkShareConnector.TryEnsureConnected(out networkShareMessage))
                {
                    loggingService.Error("Network share pre-connect failed: " + networkShareMessage, null);
                    MessageBox.Show(
                        "ไม่สามารถเชื่อมต่อ Network Share จากค่าที่ตั้งไว้ได้\r\n\r\n" + networkShareMessage,
                        "CMS - Network Share Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                UserAccount account = userAccountRepo.GetByADUserName(adUserName);

                if (account == null)
                {
                    // Auto-register ผู้ใช้ใหม่ — ใช้ข้อมูลจาก EmployeeInfo ที่ได้จาก AD API
                    try
                    {
                        var emp = authResult.EmployeeInfo;
                        var newAccount = new UserAccount
                        {
                            ADUserName = adUserName,
                            DisplayName = emp != null ? emp.FullNameTh : adUserName,
                            Email = null,
                            AppRole = 1,      // Default: Officer
                            OrgUnitID = 1,
                            IsActive = true,
                            ConsentGiven = false,
                            CreatedDate = DateTime.Now
                        };

                        int newUserId = userAccountRepo.Create(newAccount);
                        account = userAccountRepo.GetById(newUserId);
                        loggingService.Info("Auto-registered new user: " + adUserName);

                        MessageBox.Show(
                            string.Format("ยินดีต้อนรับ! บัญชีของคุณถูกสร้างขึ้นอัตโนมัติ\n\nผู้ใช้: {0}\nRole: Officer (ค่าเริ่มต้น)\n\nกรุณายืนยัน PDPA เพื่อดำเนินการต่อ",
                                newAccount.DisplayName),
                            "CMS - Account Created",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        loggingService.Error("Failed to auto-register user: " + adUserName, ex);
                        MessageBox.Show(
                            "ไม่สามารถสร้างบัญชีอัตโนมัติได้ กรุณาติดต่อผู้ดูแลระบบ",
                            "CMS - Registration Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (!account.IsActive)
                {
                    MessageBox.Show(
                        "บัญชีของคุณถูกระงับ กรุณาติดต่อผู้ดูแลระบบ",
                        "CMS - Account Deactivated",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update last login
                userAccountRepo.UpdateLastLogin(account.UserID);

                // Sync Employee Info (ถ้าได้ข้อมูลจาก AD API ให้ใช้โดยตรง)
                // ฝ่าย/แผนก ที่ admin กำหนดเป็นรหัส HR Master แล้วจะไม่ถูกเขียนทับด้วยรหัสจาก API
                bool keepHrOrg = HasHrDepartment(account, hrDepartmentRepository, loggingService);
                if (authResult.EmployeeInfo != null)
                {
                    SyncEmployeeInfoFromAuthResult(account, userAccountRepo, authResult.EmployeeInfo, keepHrOrg);
                }
                else
                {
                    try { SyncEmployeeInfoFromApi(account, userAccountRepo, employeeApiService, keepHrOrg); }
                    catch { /* ไม่ block login */ }
                }

                // ── PDPA Consent ──────────────────────────────────────────────
                if (!account.ConsentGiven)
                {
                    using (var pdpaDialog = new PdpaConsentDialog())
                    {
                        if (pdpaDialog.ShowDialog() != DialogResult.OK)
                            return;
                    }
                    userAccountRepo.UpdateConsent(account.UserID, account.Email, true);
                }

                // ── Reload account (รับ PositionName/DeptCode/DivCode ที่ Sync แล้ว) ──
                account = userAccountRepo.GetById(account.UserID) ?? account;

                // ── Build session context ──────────────────────────────────────
                var userContext = new UserContext
                {
                    UserID = account.UserID,
                    ADUserName = account.ADUserName,
                    AppRole = account.AppRole,
                    MachineName = System.Net.Dns.GetHostName(),
                    IPAddress = GetLocalIPAddress(),
                    LevelCode = account.LevelCode,
                    PositionName = account.PositionName,
                    DepartmentCode = account.DepartmentCode,
                    DepartmentName = account.DepartmentName,
                    DepartmentId = account.DepartmentId,
                    DivisionId   = account.DivisionId,
                    // Resolve Authority จาก ตำแหน่ง/แผนก/ฝ่าย ของพนักงาน
                    Authority = TryResolveAuthority(folderAuthorityService, account, loggingService),
                    // Pre-load ACL entries ครั้งเดียวตอน login เพื่อลด DB call ตลอด session
                    SessionAcl = TryPreloadSessionAcl(permissionRepository, account, loggingService)
                };

                // ── Audit LOGIN ───────────────────────────────────────────────
                auditService.Log(new AuditEntry
                {
                    Module = "Auth",
                    Action = "LOGIN",
                    UserID = userContext.UserID,
                    UserName = account.ADUserName,
                    TableName = "EMS.UserAccount",
                    RecordID = userContext.UserID,
                    MachineName = userContext.MachineName,
                    IPAddress = userContext.IPAddress,
                    Result = 1,
                    AuditDate = DateTime.Now
                });

                Application.ApplicationExit += (s, e) =>
                {
                    try
                    {
                        auditService.Log(new AuditEntry
                        {
                            Module = "Auth",
                            Action = "LOGOUT",
                            UserID = userContext.UserID,
                            UserName = userContext.ADUserName ?? string.Empty,
                            UserRole = userContext.AppRole,
                            TableName = "EMS.UserAccount",
                            RecordID = userContext.UserID,
                            MachineName = userContext.MachineName,
                            IPAddress = userContext.IPAddress,
                            Result = 1,
                            AuditDate = DateTime.Now
                        });
                    }
                    catch (Exception ex)
                    {
                        loggingService.Error("Failed to record logout audit.", ex);
                    }
                };
                // ── Menu management ──────────────────────────────────────
                var menuItemRepository = new CMS.Data.Repositories.MenuItemRepository(configManager);
                var menuService = new MenuService(menuItemRepository, loggingService, auditService);
                var folderTemplateController = new FolderTemplateController(folderTemplateRepository, folderService);
                var shareInboxReadRepository = new CMS.Data.Repositories.ShareInboxReadRepository(configManager);
                var shareLookupController = new ShareLookupController(permissionRepository, filePermissionRepository, folderRepository, fileDocumentRepository, userAccountRepo, shareInboxReadRepository);
                var accessShareController = new AccessShareController(shareAccessService);
                var shareManagementController = new ShareManagementController(shareService, approvalService);
                var shareCommandController = new ShareCommandController(shareService, permissionService);
                var folderSettingsController = new FolderSettingsController(folderRepository);
                var folderPermissionLookupController = new FolderPermissionLookupController(permissionRepository);
                var userAccountLookupController = new UserAccountLookupController(userAccountRepo);
                var userAccessOrgLookupController = new UserAccessOrgLookupController(orgStructureService);
                var userAdminController = new UserAdminController(userAccountRepo);
                var userAdminWithHrController = new UserAdminController(userAccountRepo, hrDivisionRepository, hrDepartmentRepository, hrPositionRepository, hrPositionLevelRepository, hrPositionMappingRepository);
                var menuManagementController = new MenuManagementController(menuService);
                var approvalInboxController = new ApprovalInboxController(approvalService);
                var recycleBinController = new RecycleBinController(folderService, orgStructureService, fileDocumentRepository);
                var fileUploadController = new FileUploadController(fileService, folderService);
                var fileBrowseController = new FileBrowseController(fileService);
                var fileCommandController = new FileCommandController(fileService);
                var uploadCheckpointController = new UploadCheckpointController(uploadCheckpointService);
                var folderReadLookupController = new FolderReadLookupController(folderRepository, folderService);
                var fileDocumentLookupController = new FileDocumentLookupController(fileDocumentRepository);
                var filePermissionAclController = new FilePermissionAclController(filePermissionRepository);
                var filePermissionCommandController = new FilePermissionCommandController(
                    filePermissionService,
                    filePermissionAclController,
                    folderPermissionLookupController,
                    auditService);
                var authorityConfigController = new AuthorityConfigController(folderAuthorityService);
                var authorityImportService = new AuthorityImportService(
                    folderAuthorityService, userAccountRepo, hrDepartmentRepository, hrDivisionRepository, auditService);
                var authorityImportController = new AuthorityImportController(authorityImportService);
                var authorityOrgLookupController = new AuthorityOrgLookupController(
                    hrDivisionRepository,
                    hrDepartmentRepository,
                    hrPositionRepository,
                    hrPositionLevelRepository,
                    hrPositionMappingRepository,
                    userAccountRepo);
                var orgMasterService = new CMS.Services.OrgMasterService(
                    hrDivisionRepository,
                    hrDepartmentRepository,
                    hrPositionRepository,
                    hrPositionLevelRepository,
                    hrPositionMappingRepository,
                    auditService);
                var orgMasterController = new CMS.Controllers.DMS.Document.OrgMasterController(orgMasterService);

                var employeeDirectoryController = new EmployeeDirectoryController(employeeApiService);
                var userOrgVisibilityController = new UserOrgVisibilityController(userOrgVisibilityService);
                var permissionAclLookupController = new PermissionAclLookupController(permissionRepository);

                // ── Workflow Engine Controllers (MOD-008) ─────────────────────
                var workflowController = new WorkflowController(workflowService, loggingService);
                var workflowTemplateController = new WorkflowTemplateController(workflowTemplateService, loggingService);
                var workflowDashboardController = new WorkflowDashboardController(workflowService, slaCalculator, loggingService);
                var workflowDelegationController = new WorkflowDelegationController(workflowDelegationService, loggingService);
                var workflowReportController = new WorkflowReportController(slaCalculator, loggingService);

                // ── กรณีที่ 3: Global handler — catch UI-thread exceptions หลัง login ──
                string _globalErrUser = account != null ? account.ADUserName : adUserName ?? string.Empty;
                Application.ThreadException += (s, ev) =>
                    GlobalErrorDialog.Show(ev.Exception, _globalErrUser);

                // ── RGP (โมดูล Project) — Data → Services → Controllers ───────
                var rgpProjectRepo = new CMS.Data.Repositories.ProjectRepository(configManager);
                var rgpStateRepo = new CMS.Data.Repositories.ProjectStateRepository(configManager);
                var rgpTaskRepo = new CMS.Data.Repositories.ProjectTaskRepository(configManager);
                var rgpProgressRepo = new CMS.Data.Repositories.ProjectProgressRepository(configManager);
                var rgpDocRepo = new CMS.Data.Repositories.ProjectDocumentRepository(configManager);
                var rgpTemplateRepo = new CMS.Data.Repositories.ProjectTemplateRepository(configManager);
                var rgpEfilingRepo = new CMS.Data.Repositories.EFilingRepository(configManager);
                var rgpAuditRepo = new CMS.Data.Repositories.ProjectAuditRepository(configManager);
                var rgpTaskStatusLogRepo = new CMS.Data.Repositories.ProjectTaskStatusLogRepository(configManager);

                var rgpAuditService = new CMS.Services.ProjectAuditService(rgpAuditRepo);
                var rgpProjectService = new CMS.Services.ProjectService(rgpProjectRepo, rgpStateRepo, rgpDocRepo, rgpTemplateRepo, rgpAuditService);
                var rgpStateService = new CMS.Services.ProjectStateService(rgpStateRepo, rgpAuditService);
                var rgpProgressService = new CMS.Services.ProjectProgressService(rgpProgressRepo, rgpStateRepo, rgpProjectRepo);
                var rgpTaskService = new CMS.Services.ProjectTaskService(rgpTaskRepo, rgpStateRepo, rgpProgressService, rgpAuditService, rgpTaskStatusLogRepo);
                var rgpDocService = new CMS.Services.ProjectDocumentService(rgpDocRepo, rgpEfilingRepo);

                var rgpProjectController = new CMS.Controllers.RGP.ProjectController(rgpProjectService);
                var rgpStateController = new CMS.Controllers.RGP.ProjectStateController(rgpStateService);
                var rgpTaskController = new CMS.Controllers.RGP.ProjectTaskController(rgpTaskService);
                var rgpProgressController = new CMS.Controllers.RGP.ProjectProgressController(rgpProgressService);
                var rgpDocController = new CMS.Controllers.RGP.ProjectDocumentController(rgpDocService);

                // ── Launch external upload form if requested ──────────────────
                if (startup.UploadMode)
                {
                    if (startup.FolderId <= 0)
                    {
                        MessageBox.Show("กรุณาระบุ Target Folder ID (/folderId:<id>)", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string fName = null;
                    int? defaultLevel = null;
                    try
                    {
                        var targetFolder = folderService.GetFolder(startup.FolderId, userContext);
                        if (targetFolder != null)
                        {
                            fName = targetFolder.FolderName;
                            defaultLevel = targetFolder.ConfidentialityLevel;
                        }
                    }
                    catch (Exception ex)
                    {
                        loggingService.Error("Failed to retrieve folder details for external upload", ex);
                    }

                    using (var uploadForm = new FrmExternalUpload(fileUploadController, userContext, startup.FolderId, fName, defaultLevel, uploadCheckpointController))
                    {
                        if (!string.IsNullOrEmpty(startup.FilePath))
                        {
                            uploadForm.PreSelectFile(startup.FilePath);
                        }

                        if (uploadForm.ShowDialog() == DialogResult.OK)
                        {
                            int? fileId = uploadForm.UploadedFileId;
                            if (fileId.HasValue)
                            {
                                Console.WriteLine($"UPLOAD_SUCCESS: {fileId.Value} | LEVEL: {uploadForm.SelectedLevel}");

                                if (!string.IsNullOrEmpty(startup.OutPath))
                                {
                                    try
                                    {
                                        File.WriteAllText(startup.OutPath, fileId.Value.ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        loggingService.Error("Failed to write UploadedFileId to outPath: " + startup.OutPath, ex);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("UPLOAD_CANCELLED");
                        }
                    }
                    return;
                }

                // ── Launch main form ──────────────────────────────────────────
                var mainForm = new MDI_Container(
                    logger: loggingService,
                    auditService: auditService,
                    currentUser: userContext,
                    notificationService: notificationService,
                    approvalService: approvalService,
                    folderAuthorityService: folderAuthorityService,
                    orgStructureService: orgStructureService,
                    folderService: folderService,
                    configManager: configManager,
                    permissionService: permissionService,
                    filePermissionService: filePermissionService,
                    folderTemplateController: folderTemplateController,
                    uploadCheckpointController: uploadCheckpointController,
                    quickAccessService: quickAccessService,
                    shareCommandController: shareCommandController,
                    shareLookupController: shareLookupController,
                    accessShareController: accessShareController,
                    shareManagementController: shareManagementController,
                    folderSettingsController: folderSettingsController,
                    folderPermissionLookupController: folderPermissionLookupController,
                    userAccountLookupController: userAccountLookupController,
                    userAccessOrgLookupController: userAccessOrgLookupController,
                    userAdminController: userAdminController,
                    userAdminWithHrController: userAdminWithHrController,
                    menuManagementController: menuManagementController,
                    approvalInboxController: approvalInboxController,
                    recycleBinController: recycleBinController,
                    fileUploadController: fileUploadController,
                    fileBrowseController: fileBrowseController,
                    fileCommandController: fileCommandController,
                    folderReadLookupController: folderReadLookupController,
                    fileDocumentLookupController: fileDocumentLookupController,
                    filePermissionAclController: filePermissionAclController,
                    filePermissionCommandController: filePermissionCommandController,
                    authorityOrgLookupController: authorityOrgLookupController,
                    orgMasterController: orgMasterController,
                    authorityConfigController: authorityConfigController,
                    authorityImportController: authorityImportController,
                    employeeDirectoryController: employeeDirectoryController,
                    userOrgVisibilityController: userOrgVisibilityController,
                    permissionAclLookupController: permissionAclLookupController,
                    employeeApiService: employeeApiService,
                    folderRepository: folderRepository,
                    kbCategoryService: kbCategoryService,
                    kbArticleService: kbArticleService,
                    kbImportTemplateService: kbImportTemplateService,
                    kbArticleDisplayProfileService: kbArticleDisplayProfileService,
                    excelTemplatePreviewService: excelTemplatePreviewService,
                    workflowService: workflowService,
                    slaCalculator: slaCalculator,
                    workflowTemplateService: workflowTemplateService,
                    workflowDelegationService: workflowDelegationService,
                    ldapUserSearchService: new CMS.Services.LdapUserSearchService(configManager),
                    kbVersionRepository: kbVersionRepository,
                    kbDashboardService: kbDashboardService,
                    kbCategoryRepository: kbCategoryRepository,
                    kbArticleRepository: kbArticleRepository,
                    kbArticleLinkRepository: kbArticleLinkRepository,
                    subCommitteeRepository: subCommitteeRepository,
                    hrDivisionRepository: hrDivisionRepository,
                    hrDepartmentRepository: hrDepartmentRepository,
                    quotaService: quotaService,
                    rgpProjectController: rgpProjectController,
                    rgpStateController: rgpStateController,
                    rgpTaskController: rgpTaskController,
                    rgpProgressController: rgpProgressController,
                    rgpDocController: rgpDocController,
                    networkDriveHelper: networkDriveHelper,
                    fileService: fileService,
                    downloadApproverConfigController: downloadApproverConfigController,
                    downloadPolicyController: downloadPolicyController);

                // Remove temp copies of opened files left over from earlier sessions.
                OfficeSecurityHelper.PurgeStaleTempFiles();

                mainForm.LaunchedByExternalProgram = startup.Mode != AuthMode.None;

                Application.Run(mainForm);

                // ถ้า user กด Logout:
                //   - เปิดตรง (ไม่มี args) → เปิดโปรแกรมใหม่โดยไม่ส่ง args เพื่อแสดง LoginForm
                //   - เปิดจากโปรแกรมอื่น (/u:/p:, /token:) → ปิดโปรแกรมเลย
                //     (Application.Restart ส่ง args เดิม → login user เดิมซ้ำอัตโนมัติ)
                if (mainForm.LogoutRequested && startup.Mode == AuthMode.None)
                    System.Diagnostics.Process.Start(Application.ExecutablePath);
            }
            catch (Exception ex)
            {
                Exception root = UnwrapInvocationException(ex);
                string details = BuildExceptionDetails(ex);

                loggingService.Fatal("Unhandled exception in application entry point. Root: " + root.Message, ex);
                TryWriteFatalLog(details);

                MessageBox.Show(
                    "เกิดข้อผิดพลาดที่ไม่คาดคิด กรุณาติดต่อผู้ดูแลระบบ\r\n\r\n"
                    + "สาเหตุหลัก: " + root.Message + "\r\n"
                    + "(รายละเอียดถูกบันทึกในไฟล์ fatal-startup-error.log)",
                    "CMS - Fatal Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// หา IP address จริงของเครื่องนี้ (primary network adapter)
        /// ใช้ UDP trick เพื่อให้ OS เลือก adapter หลัก โดยไม่ส่งข้อมูลจริง
        /// </summary>
        private static string GetLocalIPAddress()
        {
            // วิธีที่ 1: UDP trick — ให้ OS เลือก primary adapter โดยอัตโนมัติ
            try
            {
                using (var socket = new System.Net.Sockets.Socket(
                    System.Net.Sockets.AddressFamily.InterNetwork,
                    System.Net.Sockets.SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530); // ไม่ส่งข้อมูลจริง
                    var ep = socket.LocalEndPoint as System.Net.IPEndPoint;
                    if (ep != null)
                        return ep.Address.ToString();
                }
            }
            catch { }

            // วิธีที่ 2: ดึงจาก DNS hostname (กรณี network restricted)
            try
            {
                var hostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var addr in hostEntry.AddressList)
                {
                    if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                        && !System.Net.IPAddress.IsLoopback(addr))
                        return addr.ToString();
                }
            }
            catch { }

            return "127.0.0.1";
        }

        private static Exception UnwrapInvocationException(Exception ex)
        {
            var current = ex;
            while (current is TargetInvocationException && current.InnerException != null)
            {
                current = current.InnerException;
            }

            return current;
        }

        private static string BuildExceptionDetails(Exception ex)
        {
            var sb = new StringBuilder();
            int level = 0;
            var current = ex;

            while (current != null)
            {
                sb.AppendLine("=== Exception Level " + level + " ===");
                sb.AppendLine("Type: " + current.GetType().FullName);
                sb.AppendLine("Message: " + current.Message);
                sb.AppendLine("Source: " + (current.Source ?? "(null)"));
                sb.AppendLine("StackTrace:");
                sb.AppendLine(current.StackTrace ?? "(null)");
                sb.AppendLine();

                current = current.InnerException;
                level++;
            }

            return sb.ToString();
        }

        private static void TryWriteFatalLog(string details)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(baseDir, "fatal-startup-error.log");
                File.WriteAllText(path, details);
            }
            catch
            {
                // Last-resort path; swallowing avoids masking the original startup exception.
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Sync จาก EmployeeInfo ที่ได้จาก AD API โดยตรง (ไม่ต้องเรียก EmployeeApiService)
        // ─────────────────────────────────────────────────────────────────────
        private static System.Collections.Generic.Dictionary<int, CMS.Domain.Models.FolderAuthorityConfig>
            TryResolveAuthority(
                CMS.Services.Interfaces.IFolderAuthorityService svc,
                UserAccount account,
                LoggingService log)
        {
            try
            {
                return svc.ResolveForUser(account);
            }
            catch (Exception ex)
            {
                log.Error("FolderAuthority resolve failed — stored procedures may not be deployed yet.", ex);
                return new System.Collections.Generic.Dictionary<int, CMS.Domain.Models.FolderAuthorityConfig>();
            }
        }

        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<CMS.Domain.Models.FolderPermission>>
            TryPreloadSessionAcl(
                CMS.Data.Interfaces.IPermissionRepository permRepo,
                UserAccount account,
                LoggingService log)
        {
            try
            {
                var entries = permRepo.GetAllForUser(
                    account.UserID,
                    account.AppRole,
                    account.DepartmentId,
                    account.DivisionId);

                var dict = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<CMS.Domain.Models.FolderPermission>>();
                foreach (var e in entries)
                {
                    System.Collections.Generic.List<CMS.Domain.Models.FolderPermission> list;
                    if (!dict.TryGetValue(e.FolderID, out list))
                    {
                        list = new System.Collections.Generic.List<CMS.Domain.Models.FolderPermission>();
                        dict[e.FolderID] = list;
                    }
                    list.Add(e);
                }
                return dict;
            }
            catch (Exception ex)
            {
                log.Error("SessionAcl pre-load failed — falling back to on-demand permission checks.", ex);
                return new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<CMS.Domain.Models.FolderPermission>>();
            }
        }

        /// <summary>
        /// True when the user's department is already an HR Master code (EMS.m_Department), i.e. an admin
        /// assigned it in User Management. FolderAuthorityConfig is keyed by HR codes, while the Employee API
        /// sends its own codes (e.g. "01"/"05"), so overwriting would remove the user's level-based rights.
        /// </summary>
        private static bool HasHrDepartment(
            UserAccount account,
            CMS.Data.Interfaces.IHrDepartmentRepository hrDepartmentRepo,
            LoggingService log)
        {
            if (account == null || string.IsNullOrWhiteSpace(account.DepartmentCode) || hrDepartmentRepo == null)
                return false;
            try
            {
                string code = account.DepartmentCode.Trim();
                return System.Linq.Enumerable.Any(hrDepartmentRepo.GetAll(), d => d.IsActive
                    && string.Equals((d.DepartmentCode ?? "").Trim(), code, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                log.Error("HR department lookup failed — employee sync will use API org codes.", ex);
                return false;
            }
        }

        private static void SyncEmployeeInfoFromAuthResult(
            UserAccount account,
            CMS.Data.Interfaces.IUserAccountRepository userAccountRepo,
            CMS.Services.DTOs.EmployeeInfo emp,
            bool keepHrOrg)
        {
            if (emp == null || account == null) return;
            try
            {
                bool orgChanged = !keepHrOrg && (
                    account.DepartmentCode != emp.DepartmentCode ||
                    account.DepartmentName != emp.DepartmentNameTh ||
                    account.DivisionCode != emp.DivisionCode ||
                    account.DivisionName != emp.DivisionNameTh);
                bool needsUpdate =
                    account.EmployeeCode != emp.EmployeeCode ||
                    account.EmployeeFirstNameTh != emp.EmployeeFirstNameTh ||
                    account.EmployeeLastNameTh != emp.EmployeeLastNameTh ||
                    orgChanged ||
                    account.PositionName != emp.Position;

                if (needsUpdate)
                {
                    if (keepHrOrg)
                        userAccountRepo.UpdateEmployeeInfo(
                            account.UserID,
                            emp.EmployeeCode,
                            emp.EmployeeTitleNameEn, emp.EmployeeFirstNameEn, emp.EmployeeLastNameEn,
                            emp.EmployeeTitleNameTh, emp.EmployeeFirstNameTh, emp.EmployeeLastNameTh,
                            account.DepartmentId, account.DepartmentCode, account.DepartmentNameEn, account.DepartmentName,
                            account.DivisionId, account.DivisionCode, account.DivisionNameEn, account.DivisionName,
                            emp.Position);
                    else
                        userAccountRepo.UpdateEmployeeInfo(
                            account.UserID,
                            emp.EmployeeCode,
                            emp.EmployeeTitleNameEn, emp.EmployeeFirstNameEn, emp.EmployeeLastNameEn,
                            emp.EmployeeTitleNameTh, emp.EmployeeFirstNameTh, emp.EmployeeLastNameTh,
                            null, emp.DepartmentCode, emp.DepartmentNameEn, emp.DepartmentNameTh,
                            null, emp.DivisionCode, emp.DivisionNameEn, emp.DivisionNameTh,
                            emp.Position);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SyncEmployeeInfoFromAuthResult error: " + ex.Message);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Sync จาก EmployeeApiService (fallback เมื่อไม่มี EmployeeInfo จาก AD API)
        // ─────────────────────────────────────────────────────────────────────
        private static void SyncEmployeeInfoFromApi(
            UserAccount account,
            CMS.Data.Interfaces.IUserAccountRepository userAccountRepo,
            CMS.Services.Interfaces.IEmployeeApiService employeeApiService,
            bool keepHrOrg)
        {
            if (employeeApiService == null || account == null) return;
            try
            {
                string adUser = account.ADUserName ?? "";
                var emp = employeeApiService.GetByCode(adUser);
                if (emp == null && adUser.Contains("\\"))
                {
                    string username = adUser.Substring(adUser.LastIndexOf('\\') + 1);
                    emp = employeeApiService.GetByCode(username);
                }
                if (emp == null) return;

                var pos = emp.PrimaryPosition;
                if (pos == null) return;

                bool orgChanged = !keepHrOrg && (
                    account.DepartmentCode != pos.DepartmentCode ||
                    account.DepartmentName != pos.DepartmentNameTh ||
                    account.DivisionCode != pos.DivisionCode ||
                    account.DivisionName != pos.DivisionNameTh);
                bool needsUpdate =
                    account.EmployeeCode != emp.EmployeeCode ||
                    orgChanged ||
                    account.PositionName != pos.PositionName;

                if (needsUpdate)
                {
                    if (keepHrOrg)
                        userAccountRepo.UpdateEmployeeInfo(
                            account.UserID,
                            emp.EmployeeCode,
                            null, null, null,
                            null, null, null,
                            account.DepartmentId, account.DepartmentCode, account.DepartmentNameEn, account.DepartmentName,
                            account.DivisionId, account.DivisionCode, account.DivisionNameEn, account.DivisionName,
                            pos.PositionName);
                    else
                        userAccountRepo.UpdateEmployeeInfo(
                            account.UserID,
                            emp.EmployeeCode,
                            null, null, null,
                            null, null, null,
                            pos.DepartmentId, pos.DepartmentCode, pos.DepartmentNameEn, pos.DepartmentNameTh,
                            pos.DivisionId, pos.DivisionCode, pos.DivisionNameEn, pos.DivisionNameTh,
                            pos.PositionName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SyncEmployeeInfoFromApi error: " + ex.Message);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Command-line argument parser
    // ─────────────────────────────────────────────────────────────────────────

    internal enum AuthMode { None, Credentials, Token }

    internal class StartupArgs
    {
        public AuthMode Mode { get; private set; } = AuthMode.None;
        public string Username { get; set; }
        public string Password { get; private set; }
        public string Token { get; private set; }

        public bool UploadMode { get; private set; } = false;
        public int FolderId { get; private set; } = 0;
        public string FilePath { get; private set; }
        public string OutPath { get; private set; }

        /// <summary>
        /// Parse command-line args:
        ///   /u:username   — username
        ///   /p:password   — password
        ///   /token:xxxxx  — access token
        ///   /upload       — run in standalone upload form mode
        ///   /folderId:123 — target folder ID
        ///   /filePath:xxx — path of file to upload
        ///   /out:xxx      — path to write output file ID
        /// </summary>
        public static StartupArgs Parse(string[] args)
        {
            var result = new StartupArgs();
            if (args == null || args.Length == 0) return result;

            foreach (string arg in args)
            {
                if (arg.StartsWith("/u:", StringComparison.OrdinalIgnoreCase))
                    result.Username = arg.Substring(3);
                else if (arg.StartsWith("/p:", StringComparison.OrdinalIgnoreCase))
                    result.Password = arg.Substring(3);
                else if (arg.StartsWith("/token:", StringComparison.OrdinalIgnoreCase))
                    result.Token = arg.Substring(7);
                else if (arg.Equals("/upload", StringComparison.OrdinalIgnoreCase))
                    result.UploadMode = true;
                else if (arg.StartsWith("/folderId:", StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(arg.Substring(10), out int fid);
                    result.FolderId = fid;
                }
                else if (arg.StartsWith("/filePath:", StringComparison.OrdinalIgnoreCase))
                    result.FilePath = arg.Substring(10);
                else if (arg.StartsWith("/out:", StringComparison.OrdinalIgnoreCase))
                    result.OutPath = arg.Substring(5);
            }

            if (!string.IsNullOrEmpty(result.Token))
                result.Mode = AuthMode.Token;
            else if (!string.IsNullOrEmpty(result.Username) && !string.IsNullOrEmpty(result.Password))
                result.Mode = AuthMode.Credentials;

            return result;
        }
    }
}

