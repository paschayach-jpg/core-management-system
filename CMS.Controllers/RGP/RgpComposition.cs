using CMS.Data.Interfaces;
using CMS.Data.Repositories;
using CMS.Infrastructure;
using CMS.Infrastructure.Interfaces;
using CMS.Services;
using CMS.Services.Interfaces;

namespace CMS.Controllers.RGP
{
    /// <summary>
    /// จุดประกอบ dependency แบบ lazy-singleton สำหรับโมดูล Project (RGP) เท่านั้น
    /// ใช้เป็น fallback สำหรับจุดที่ Form เดิม `new XxxController()` แบบ ad-hoc กลางเมธอด
    /// (ไม่ได้รับ dependency ผ่าน constructor จาก MDI_Container) — เทียบเท่ากับที่ ConectionDB เดิม
    /// เคยทำ (static shared instance) แต่เปลี่ยนมาใช้ CMS.Infrastructure.ConfigManager แทน
    /// ทางที่ถูกต้องกว่าคือ thread dependency ผ่าน constructor ทุกจุด — งานที่เหลือในอนาคต
    /// </summary>
    internal static class RgpComposition
    {
        private static readonly object _lock = new object();

        private static IConfigManager _configManager;
        private static IProjectService _projectService;
        private static IProjectStateService _stateService;
        private static IProjectTaskService _taskService;
        private static IProjectProgressService _progressService;
        private static IProjectDocumentService _docService;
        private static IProjectAttachmentService _attachmentService;
        private static IProjectMemberRepository _memberRepo;
        private static IProjectMemberRoleRepository _memberRoleRepo;
        private static IProjectStateMemberRepository _stateMemberRepo;
        private static IProjectNotifyLogRepository _notifyLogRepo;
        private static IProjectTaskStatusLogRepository _taskStatusLogRepo;
        private static IProjectTemplateRepository _templateRepo;
        private static ITemplateDocRepository _templateDocRepo;
        private static IEmployeeRepository _employeeRepo;
        private static IKmsRepository _kmsRepo;

        public static IConfigManager ConfigManager
        {
            get
            {
                lock (_lock)
                {
                    return _configManager ?? (_configManager = new ConfigManager());
                }
            }
        }

        public static IProjectService ProjectService
        {
            get
            {
                lock (_lock)
                {
                    if (_projectService != null) return _projectService;
                    var cm = ConfigManager;
                    var auditService = new ProjectAuditService(new ProjectAuditRepository(cm));
                    _projectService = new ProjectService(
                        new ProjectRepository(cm),
                        StateRepoInternal(cm),
                        new ProjectDocumentRepository(cm),
                        new ProjectTemplateRepository(cm),
                        auditService);
                    return _projectService;
                }
            }
        }

        public static IProjectStateService StateService
        {
            get
            {
                lock (_lock)
                {
                    if (_stateService != null) return _stateService;
                    var cm = ConfigManager;
                    var auditService = new ProjectAuditService(new ProjectAuditRepository(cm));
                    _stateService = new ProjectStateService(StateRepoInternal(cm), auditService);
                    return _stateService;
                }
            }
        }

        public static IProjectProgressService ProgressService
        {
            get
            {
                lock (_lock)
                {
                    if (_progressService != null) return _progressService;
                    var cm = ConfigManager;
                    _progressService = new ProjectProgressService(
                        new ProjectProgressRepository(cm), StateRepoInternal(cm), new ProjectRepository(cm));
                    return _progressService;
                }
            }
        }

        public static IProjectTaskService TaskService
        {
            get
            {
                lock (_lock)
                {
                    if (_taskService != null) return _taskService;
                    var cm = ConfigManager;
                    var auditService = new ProjectAuditService(new ProjectAuditRepository(cm));
                    _taskService = new ProjectTaskService(
                        new ProjectTaskRepository(cm), StateRepoInternal(cm), ProgressService, auditService,
                        TaskStatusLogRepository);
                    return _taskService;
                }
            }
        }

        public static IProjectDocumentService DocService
        {
            get
            {
                lock (_lock)
                {
                    if (_docService != null) return _docService;
                    var cm = ConfigManager;
                    _docService = new ProjectDocumentService(new ProjectDocumentRepository(cm), new EFilingRepository(cm));
                    return _docService;
                }
            }
        }

        public static IProjectAttachmentService AttachmentService
        {
            get
            {
                lock (_lock)
                {
                    return _attachmentService ?? (_attachmentService =
                        new ProjectAttachmentService(new ProjectAttachmentRepository(ConfigManager)));
                }
            }
        }

        public static IProjectMemberRepository MemberRepository
        {
            get { lock (_lock) { return _memberRepo ?? (_memberRepo = new ProjectMemberRepository(ConfigManager)); } }
        }

        public static IProjectMemberRoleRepository MemberRoleRepository
        {
            get { lock (_lock) { return _memberRoleRepo ?? (_memberRoleRepo = new ProjectMemberRoleRepository(ConfigManager)); } }
        }

        public static IProjectStateMemberRepository StateMemberRepository
        {
            get { lock (_lock) { return _stateMemberRepo ?? (_stateMemberRepo = StateMemberRepoInternal(ConfigManager)); } }
        }

        public static IProjectNotifyLogRepository NotifyLogRepository
        {
            get { lock (_lock) { return _notifyLogRepo ?? (_notifyLogRepo = new ProjectNotifyLogRepository(ConfigManager)); } }
        }

        public static IProjectTaskStatusLogRepository TaskStatusLogRepository
        {
            get { lock (_lock) { return _taskStatusLogRepo ?? (_taskStatusLogRepo = new ProjectTaskStatusLogRepository(ConfigManager)); } }
        }

        public static IProjectTemplateRepository TemplateRepository
        {
            get { lock (_lock) { return _templateRepo ?? (_templateRepo = new ProjectTemplateRepository(ConfigManager)); } }
        }

        public static ITemplateDocRepository TemplateDocRepository
        {
            get { lock (_lock) { return _templateDocRepo ?? (_templateDocRepo = new TemplateDocRepository(ConfigManager)); } }
        }

        public static IEmployeeRepository EmployeeRepository
        {
            get { lock (_lock) { return _employeeRepo ?? (_employeeRepo = new EmployeeRepository(ConfigManager)); } }
        }

        public static IKmsRepository KmsRepository
        {
            get { lock (_lock) { return _kmsRepo ?? (_kmsRepo = new KmsRepository(ConfigManager)); } }
        }

        private static IProjectStateRepository _stateRepoCache;
        private static IProjectStateRepository StateRepoInternal(IConfigManager cm) =>
            _stateRepoCache ?? (_stateRepoCache = new ProjectStateRepository(cm));

        private static IProjectStateMemberRepository StateMemberRepoInternal(IConfigManager cm) =>
            new ProjectStateMemberRepository(cm);
    }
}
