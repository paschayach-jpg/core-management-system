using RG_Project.Login;
using RG_Project.Project.Forms;
using CMS.Infrastructure;
using CMS.Data.Repositories;
using CMS.Services;
using CMS.Controllers.RGP;
using System;
using System.Windows.Forms;

namespace RG_Project
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // ── Composition root: Infrastructure/Auth (รวมเข้ากับ CMS) ──
                var configManager = new ConfigManager();
                var userAccountRepo = new UserAccountRepository(configManager);
                var adAuthService = new AdAuthService(configManager);

                var login = new LoginFrmNew(adAuthService, userAccountRepo);
                login.LoginSuccess += () => { };
                login.ShowDialog();

                var userContext = login.CurrentUserContext;
                if (userContext != null)
                {
                    try
                    {
                        // ── Composition root: Data layer (โมดูล Project/RGP) ──
                        var projectRepo = new ProjectRepository(configManager);
                        var stateRepo = new ProjectStateRepository(configManager);
                        var taskRepo = new ProjectTaskRepository(configManager);
                        var progressRepo = new ProjectProgressRepository(configManager);
                        var docRepo = new ProjectDocumentRepository(configManager);
                        var templateRepo = new ProjectTemplateRepository(configManager);
                        var efilingRepo = new EFilingRepository(configManager);
                        var auditRepo = new ProjectAuditRepository(configManager);

                        // ── Composition root: Services layer ──
                        var auditService = new ProjectAuditService(auditRepo);
                        var projectService = new ProjectService(projectRepo, stateRepo, docRepo, templateRepo, auditService);
                        var stateService = new ProjectStateService(stateRepo, auditService);
                        var progressService = new ProjectProgressService(progressRepo, stateRepo, projectRepo);
                        var taskService = new ProjectTaskService(taskRepo, stateRepo, progressService, auditService);
                        var docService = new ProjectDocumentService(docRepo, efilingRepo);

                        // ── Composition root: Controllers layer (CMS.Controllers.RGP) ──
                        // หมายเหตุ: Frm_Project_List และฟอร์มลูกยังไม่รับ dependency พวกนี้ผ่าน constructor
                        // (จะแก้ทีละฟอร์มในสเต็ปถัดไป) — ตอนนี้แค่พิสูจน์ว่าประกอบ graph ได้ครบและ login ได้จริงก่อน
                        var projectController = new ProjectController(projectService);
                        var stateController = new ProjectStateController(stateService);
                        var taskController = new ProjectTaskController(taskService);
                        var progressController = new ProjectProgressController(progressService);
                        var docController = new ProjectDocumentController(docService);

                        Application.Run(new Frm_Project_List());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "เกิดข้อผิดพลาดในการโหลดหน้าหลัก: " + ex.Message,
                            "ข้อผิดพลาด",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "เกิดข้อผิดพลาดวิกฤต: " + ex.Message,
                    "Critical Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
