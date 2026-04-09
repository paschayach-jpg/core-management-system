using System;
using System.Windows.Forms;
using CMS.Data.Repositories;
using CMS.Controllers.DMS.Document;
using CMS.Domain.Models;
using CMS.Infrastructure;
using CMS.Services;
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

            var loggingService = new LoggingService();

            try
            {
                // --- Infrastructure / repositories ---
                var configManager          = new ConfigManager();
                var adHelper               = new ActiveDirectoryHelper();
                var userAccountRepo        = new UserAccountRepository(configManager);
                var auditRepository        = new AuditRepository(configManager);
                var notificationRepository = new NotificationRepository(configManager);
                var approvalRepository     = new ApprovalRepository(configManager);
                var folderRepository       = new FolderRepository(configManager);
                var permissionRepository   = new PermissionRepository(configManager);
                var orgUnitRepository      = new OrgUnitRepository(configManager);
                var hrDivisionRepository      = new HrDivisionRepository(configManager);
                var hrDepartmentRepository    = new HrDepartmentRepository(configManager);
                var hrPositionRepository      = new HrPositionRepository(configManager);
                var hrPositionLevelRepository = new HrPositionLevelRepository(configManager);
                var hrPositionMappingRepository = new HrPositionMappingRepository(configManager);

                var fileDocumentRepository   = new FileDocumentRepository(configManager);
                var filePermissionRepository = new FilePermissionRepository(configManager);
                var folderTemplateRepository = new FolderTemplateRepository(configManager);
                var quickAccessRepository    = new QuickAccessRepository(configManager);
                var externalShareRepository  = new ExternalShareRepository(configManager);
                var shareAccessLogRepository = new ShareAccessLogRepository(configManager);
                var shareTokenRepository     = new ShareTokenRepository(configManager);
                var kbCategoryRepository     = new KBCategoryRepository(configManager);
                var kbArticleRepository      = new KBArticleRepository(configManager);
                var kbVersionRepository      = new KBVersionRepository(configManager);
                var kbArticleLinkRepository  = new KBArticleLinkRepository(configManager);

                var auditService        = new AuditService(auditRepository, loggingService);
                var notificationService = new NotificationService(notificationRepository, loggingService);
                var permissionService   = new PermissionService(permissionRepository, folderRepository, userAccountRepo, auditService, fileDocumentRepository, filePermissionRepository);
                var filePermissionService = new FilePermissionService(filePermissionRepository, auditService);

                var userOrgVisibilityRepo    = new UserOrgVisibilityRepository(configManager);
                var userOrgVisibilityService = new UserOrgVisibilityService(userOrgVisibilityRepo, orgUnitRepository, auditService);

                var orgStructureService = new OrgStructureService(orgUnitRepository, auditService, userOrgVisibilityService);
                
                var networkDriveHelper  = new NetworkDriveHelper(configManager);
                var clamAvAdapter       = new ClamAVAdapter(configManager);
                var virusScanService    = new VirusScanService(
                    clamAvAdapter,
                    loggingService,
                    notificationRepository,
                    userAccountRepo,
                    auditService,
                    configManager);

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
                    userAccountRepo);

                var approvalService     = new ApprovalService(
                    approvalRepository,
                    folderRepository,
                    folderService,
                    permissionService,
                    notificationRepository,
                    userAccountRepo,
                    auditService,
                    loggingService);

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
                    filePermissionRepository);

                var employeeApiService      = new EmployeeApiService(configManager);
                var uploadCheckpointService = new UploadCheckpointService(loggingService);
                var quickAccessService      = new QuickAccessService(quickAccessRepository, folderRepository);

                var folderAuthorityRepo    = new FolderAuthorityRepository(configManager);
                var folderAuthorityService = new FolderAuthorityService(folderAuthorityRepo);

                // ── File Sharing Services ─────────────────────────────────────
                var shareTokenService   = new ShareTokenService(shareTokenRepository, loggingService);
                var shareAccessService  = new ShareAccessService(
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
                    loggingService);

                // ── KB Services (MOD-006) ─────────────────────────────────────
                var kbCategoryService = new KBCategoryService(kbCategoryRepository, auditService);
                var kbArticleService  = new KBArticleService(kbArticleRepository, kbVersionRepository, auditService);

                // ── AD Auth Service ───────────────────────────────────────────
                var adAuthService = new AdAuthService(configManager);

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
                }
                else
                {
                    // เปิดตรง — แสดง LoginForm ให้กรอก
                    loggingService.Info("Startup mode: Manual login (no args)");
                    using (var loginForm = new LoginForm(adAuthService, new LoginAccountController(userAccountRepo)))
                    {
                        if (loginForm.ShowDialog() != DialogResult.OK)
                            return; // ผู้ใช้ปิด form

                        authResult             = loginForm.AuthResult;
                        startup.Username       = loginForm.EnteredUsername;
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

                UserAccount account = userAccountRepo.GetByADUserName(adUserName);

                if (account == null)
                {
                    // Auto-register ผู้ใช้ใหม่ — ใช้ข้อมูลจาก EmployeeInfo ที่ได้จาก AD API
                    try
                    {
                        var emp = authResult.EmployeeInfo;
                        var newAccount = new UserAccount
                        {
                            ADUserName  = adUserName,
                            DisplayName = emp != null ? emp.FullNameTh : adUserName,
                            Email       = null,
                            AppRole     = 1,      // Default: Officer
                            OrgUnitID   = 1,
                            IsActive    = true,
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
                if (authResult.EmployeeInfo != null)
                {
                    SyncEmployeeInfoFromAuthResult(account, userAccountRepo, authResult.EmployeeInfo);
                }
                else
                {
                    try { SyncEmployeeInfoFromApi(account, userAccountRepo, employeeApiService); }
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
                    UserID         = account.UserID,
                    ADUserName     = account.ADUserName,
                    AppRole        = account.AppRole,
                    MachineName    = System.Net.Dns.GetHostName(),
                    LevelCode      = account.LevelCode,
                    PositionName   = account.PositionName,
                    DepartmentCode = account.DepartmentCode,
                    DepartmentName = account.DepartmentName,
                    // Resolve Authority จาก ตำแหน่ง/แผนก/ฝ่าย ของพนักงาน
                    Authority      = TryResolveAuthority(folderAuthorityService, account, loggingService)
                };

                // ── Audit LOGIN ───────────────────────────────────────────────
                auditService.Log(new AuditEntry
                {
                    Module      = "Auth",
                    Action      = "LOGIN",
                    UserID      = userContext.UserID,
                    UserName    = account.DisplayName,
                    TableName   = "core.UserAccount",
                    RecordID    = userContext.UserID,
                    MachineName = userContext.MachineName,
                    IPAddress   = userContext.IPAddress,
                    Result      = 1,
                    AuditDate   = DateTime.Now
                });

                Application.ApplicationExit += (s, e) =>
                {
                    try
                    {
                        auditService.Log(new AuditEntry
                        {
                            Module      = "Auth",
                            Action      = "LOGOUT",
                            UserID      = userContext.UserID,
                            UserName    = userContext.ADUserName ?? string.Empty,
                            UserRole    = userContext.AppRole,
                            TableName   = "core.UserAccount",
                            RecordID    = userContext.UserID,
                            MachineName = userContext.MachineName,
                            IPAddress   = userContext.IPAddress,
                            Result      = 1,
                            AuditDate   = DateTime.Now
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
                var folderTemplateController = new FolderTemplateController(folderTemplateRepository);
                var shareLookupController = new ShareLookupController(permissionRepository, filePermissionRepository, folderRepository, fileDocumentRepository, userAccountRepo);
                var folderSettingsController = new FolderSettingsController(folderRepository);
                var folderPermissionLookupController = new FolderPermissionLookupController(permissionRepository);
                var userAccountLookupController = new UserAccountLookupController(userAccountRepo);
                var userAdminController = new UserAdminController(userAccountRepo);
                var userAdminWithHrController = new UserAdminController(userAccountRepo, hrDivisionRepository, hrDepartmentRepository, hrPositionRepository, hrPositionLevelRepository);
                var folderReadLookupController = new FolderReadLookupController(folderRepository);
                var fileDocumentLookupController = new FileDocumentLookupController(fileDocumentRepository);
                var filePermissionAclController = new FilePermissionAclController(filePermissionRepository);
                var authorityOrgLookupController = new AuthorityOrgLookupController(
                    hrDivisionRepository,
                    hrDepartmentRepository,
                    hrPositionRepository,
                    hrPositionMappingRepository,
                    userAccountRepo);
                // ── Launch main form ──────────────────────────────────────────
                var mainForm = new MDI_Container(
                    logger: loggingService,
                    auditService: auditService,
                    currentUser: userContext,
                    notificationService: notificationService,
                    approvalService: approvalService,
                    orgStructureService: orgStructureService,
                    folderService: folderService,
                    fileService: fileService,
                    configManager: configManager,
                    permissionService: permissionService,
                    folderRepository: folderRepository,
                    filePermissionService: filePermissionService,
                    folderTemplateController: folderTemplateController,
                    employeeApiService: employeeApiService,
                    uploadCheckpointService: uploadCheckpointService,
                    quickAccessService: quickAccessService,
                    userOrgVisibilityService: userOrgVisibilityService,
                    folderAuthorityService: folderAuthorityService,
                    shareService: shareService,
                    shareAccessService: shareAccessService,
                    menuService: menuService,
                    shareLookupController: shareLookupController,
                    folderSettingsController: folderSettingsController,
                    folderPermissionLookupController: folderPermissionLookupController,
                    userAccountLookupController: userAccountLookupController,
                    userAdminController: userAdminController,
                    userAdminWithHrController: userAdminWithHrController,
                    folderReadLookupController: folderReadLookupController,
                    fileDocumentLookupController: fileDocumentLookupController,
                    filePermissionAclController: filePermissionAclController,
                    authorityOrgLookupController: authorityOrgLookupController,
                    shareService,
                    shareAccessService,
                    kbCategoryService,
                    kbArticleService);
                Application.Run(mainForm);

                // ถ้า user กด Logout → restart process เพื่อแสดง LoginForm ใหม่
                if (mainForm.LogoutRequested)
                    Application.Restart();
            }
            catch (Exception ex)
            {
                loggingService.Fatal("Unhandled exception in application entry point.", ex);
                MessageBox.Show(
                    "เกิดข้อผิดพลาดที่ไม่คาดคิด กรุณาติดต่อผู้ดูแลระบบ\r\n\r\n" + ex.Message,
                    "CMS - Fatal Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private static void SyncEmployeeInfoFromAuthResult(
            UserAccount account,
            CMS.Data.Interfaces.IUserAccountRepository userAccountRepo,
            EmployeeInfo emp)
        {
            if (emp == null || account == null) return;
            try
            {
                bool needsUpdate =
                    account.EmployeeCode        != emp.EmployeeCode        ||
                    account.EmployeeFirstNameTh != emp.EmployeeFirstNameTh ||
                    account.EmployeeLastNameTh  != emp.EmployeeLastNameTh  ||
                    account.DepartmentCode      != emp.DepartmentCode      ||
                    account.DepartmentName      != emp.DepartmentNameTh    ||
                    account.DivisionCode        != emp.DivisionCode        ||
                    account.DivisionName        != emp.DivisionNameTh      ||
                    account.PositionName        != emp.Position;

                if (needsUpdate)
                {
                    userAccountRepo.UpdateEmployeeInfo(
                        account.UserID,
                        emp.EmployeeCode,
                        emp.EmployeeTitleNameEn, emp.EmployeeFirstNameEn, emp.EmployeeLastNameEn,
                        emp.EmployeeTitleNameTh, emp.EmployeeFirstNameTh, emp.EmployeeLastNameTh,
                        null, emp.DepartmentCode, emp.DepartmentNameEn, emp.DepartmentNameTh,
                        null, emp.DivisionCode,   emp.DivisionNameEn,   emp.DivisionNameTh,
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
            CMS.Services.Interfaces.IEmployeeApiService employeeApiService)
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

                bool needsUpdate =
                    account.EmployeeCode   != emp.EmployeeCode      ||
                    account.DepartmentCode != pos.DepartmentCode    ||
                    account.DepartmentName != pos.DepartmentNameTh  ||
                    account.DivisionCode   != pos.DivisionCode      ||
                    account.DivisionName   != pos.DivisionNameTh    ||
                    account.PositionName   != pos.PositionName;

                if (needsUpdate)
                {
                    userAccountRepo.UpdateEmployeeInfo(
                        account.UserID,
                        emp.EmployeeCode,
                        null, null, null,
                        null, null, null,
                        pos.DepartmentId, pos.DepartmentCode, pos.DepartmentNameEn, pos.DepartmentNameTh,
                        pos.DivisionId,   pos.DivisionCode,   pos.DivisionNameEn,   pos.DivisionNameTh,
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
        public AuthMode Mode     { get; private set; } = AuthMode.None;
        public string   Username { get; set; }
        public string   Password { get; private set; }
        public string   Token    { get; private set; }

        /// <summary>
        /// Parse command-line args:
        ///   /u:username   — username
        ///   /p:password   — password
        ///   /token:xxxxx  — access token
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
            }

            if (!string.IsNullOrEmpty(result.Token))
                result.Mode = AuthMode.Token;
            else if (!string.IsNullOrEmpty(result.Username) && !string.IsNullOrEmpty(result.Password))
                result.Mode = AuthMode.Credentials;

            return result;
        }
    }
}
