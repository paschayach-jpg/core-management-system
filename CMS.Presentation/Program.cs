using System;
using System.Windows.Forms;
using CMS.Data.Repositories;
using CMS.Domain.Models;
using CMS.Infrastructure;
using CMS.Services;

namespace CMS.Presentation
{
    static class Program
    {
        [STAThread]
        static void Main()
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

                var fileDocumentRepository   = new FileDocumentRepository(configManager);
                var filePermissionRepository = new FilePermissionRepository(configManager);
                var folderTemplateRepository = new FolderTemplateRepository(configManager);
                var quickAccessRepository = new QuickAccessRepository(configManager);

                var auditService        = new AuditService(auditRepository, loggingService);
                var notificationService = new NotificationService(notificationRepository, loggingService);
                var permissionService   = new PermissionService(permissionRepository, folderRepository, userAccountRepo, auditService, fileDocumentRepository, filePermissionRepository);
                var filePermissionService = new FilePermissionService(filePermissionRepository, auditService);
                var orgStructureService = new OrgStructureService(orgUnitRepository, auditService);
                var approvalService     = new ApprovalService(
                    approvalRepository,
                    folderRepository,
                    permissionService,
                    notificationRepository,
                    userAccountRepo,
                    auditService,
                    loggingService);

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
                    approvalRepository);

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

                // --- Employee API service ---
                var employeeApiService = new EmployeeApiService(configManager);

                // --- Upload checkpoint service ---
                var uploadCheckpointService = new UploadCheckpointService(loggingService);

                // --- Quick Access service ---
                var quickAccessService = new QuickAccessService(quickAccessRepository, folderRepository);

                // --- Resolve Windows identity ---
                AdUserInfo adUser;
                try
                {
                    adUser = adHelper.GetCurrentUser();
                }
                catch (Exception ex)
                {
                    loggingService.Error("Failed to resolve Windows identity.", ex);
                    MessageBox.Show(
                        "Unable to determine your Windows identity. Please contact your system administrator.",
                        "CMS - Authentication Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // --- Look up UserAccount in CMS database ---
                UserAccount account = userAccountRepo.GetByADUserName(adUser.AdIdentity);

                if (account == null)
                {
                    // Auto-register new user
                    try
                    {
                        var newAccount = new UserAccount
                        {
                            ADUserName = adUser.AdIdentity,
                            DisplayName = adUser.DisplayName ?? adUser.AdIdentity,
                            Email = null, // Will be set during PDPA consent
                            AppRole = 1, // Default to Officer role
                            OrgUnitID = 1, // Default OrgUnit - you may need to adjust this
                            IsActive = true,
                            ConsentGiven = false,
                            CreatedDate = DateTime.Now
                        };

                        int newUserId = userAccountRepo.Create(newAccount);
                        account = userAccountRepo.GetById(newUserId);

                        loggingService.Info($"Auto-registered new user: {adUser.AdIdentity}");
                        
                        MessageBox.Show(
                            $"Welcome! Your account has been automatically registered.\n\nUser: {adUser.DisplayName}\nRole: Officer (Default)\n\nYou will need to accept the PDPA policy to continue.",
                            "CMS - Account Created",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        loggingService.Error($"Failed to auto-register user {adUser.AdIdentity}", ex);
                        MessageBox.Show(
                            "Failed to register your account automatically. Please contact your system administrator.",
                            "CMS - Registration Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
                }

                if (!account.IsActive)
                {
                    MessageBox.Show(
                        "Your account has been deactivated. Please contact your system administrator.",
                        "CMS - Account Deactivated",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Update last login
                userAccountRepo.UpdateLastLogin(account.UserID);

                // --- Sync Employee Info from API (background, non-blocking) ---
                try
                {
                    SyncEmployeeInfoFromApi(account, userAccountRepo, employeeApiService);
                }
                catch { /* ไม่ block login ถ้า sync ไม่สำเร็จ */ }

                // --- PDPA Consent check ---
                if (!account.ConsentGiven)
                {
                    using (var pdpaDialog = new PdpaConsentDialog())
                    {
                        if (pdpaDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        {
                            // User declined — exit application
                            return;
                        }
                    }
                    // บันทึกการยืนยัน
                    userAccountRepo.UpdateConsent(account.UserID, account.Email, true);
                }

                // --- Build session context ---
                var userContext = new UserContext
                {
                    UserID      = account.UserID,
                    ADUserName  = account.ADUserName,
                    AppRole     = account.AppRole,
                    MachineName = adUser.WorkstationId
                };

                // --- Record login audit event ---
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

                // --- Fallback logout logger on unexpected process exit ---
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
                        loggingService.Error("Failed to record logout audit on ApplicationExit.", ex);
                    }
                };

                // --- Launch main form ---
                var mainForm = new MDI_Container(
                    loggingService,
                    auditService,
                    userContext,
                    notificationService,
                    approvalService,
                    orgStructureService,
                    folderService,
                    fileService,
                    configManager,
                    userAccountRepo,
                    permissionService,
                    permissionRepository,
                    folderRepository,
                    filePermissionService,
                    filePermissionRepository,
                    fileDocumentRepository,
                    folderTemplateRepository,
                    employeeApiService,
                    uploadCheckpointService,
                    quickAccessService);
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                loggingService.Fatal("Unhandled exception in application entry point.", ex);
                MessageBox.Show(
                    "An unexpected error occurred. Please contact your system administrator.\r\n\r\n" + ex.Message,
                    "CMS - Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Sync ข้อมูล Employee จาก API เข้า UserAccount
        /// Match ด้วย ADUserName = EmployeeCode
        /// เรียกทุกครั้งที่ login เพื่อให้ข้อมูลเป็นปัจจุบัน
        /// </summary>
        private static void SyncEmployeeInfoFromApi(
            UserAccount account,
            CMS.Data.Interfaces.IUserAccountRepository userAccountRepo,
            CMS.Services.Interfaces.IEmployeeApiService employeeApiService)
        {
            if (employeeApiService == null || account == null) return;

            try
            {
                // ลอง match ด้วย ADUserName (อาจเป็น employeeCode หรือ domain\username)
                string adUser = account.ADUserName ?? "";

                // ลอง exact match ก่อน
                var emp = employeeApiService.GetByCode(adUser);

                // ถ้าไม่เจอ ลอง strip domain prefix (domain\username → username)
                if (emp == null && adUser.Contains("\\"))
                {
                    string username = adUser.Substring(adUser.LastIndexOf('\\') + 1);
                    emp = employeeApiService.GetByCode(username);
                }

                if (emp == null) return; // ไม่พบข้อมูลพนักงาน

                var pos = emp.PrimaryPosition;
                if (pos == null) return;

                // Update เฉพาะถ้าข้อมูลเปลี่ยน
                bool needsUpdate =
                    account.EmployeeCode   != emp.EmployeeCode      ||
                    account.DepartmentId   != pos.DepartmentId      ||
                    account.DepartmentCode != pos.DepartmentCode    ||
                    account.DepartmentName != pos.DepartmentNameTh  ||
                    account.DivisionId     != pos.DivisionId        ||
                    account.DivisionCode   != pos.DivisionCode      ||
                    account.DivisionName   != pos.DivisionNameTh    ||
                    account.PositionName   != pos.PositionName;

                if (needsUpdate)
                {
                    userAccountRepo.UpdateEmployeeInfo(
                        account.UserID,
                        emp.EmployeeCode,
                        pos.DepartmentId,   pos.DepartmentCode, pos.DepartmentNameTh,
                        pos.DivisionId,     pos.DivisionCode,   pos.DivisionNameTh,
                        pos.PositionName);

                    System.Diagnostics.Debug.WriteLine(
                        string.Format("SyncEmployeeInfo: Updated user {0} → Dept:{1} Div:{2}",
                            account.ADUserName, pos.DepartmentNameTh, pos.DivisionNameTh));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SyncEmployeeInfo error: " + ex.Message);
                // ไม่ throw — ไม่ block login
            }
        }
    }
}
