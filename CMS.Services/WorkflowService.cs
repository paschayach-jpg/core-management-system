using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using CMS.Data.Models;
using CMS.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Services
{
    /// <summary>
    /// Main workflow orchestration service implementation
    /// </summary>
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowTemplateRepository _templateRepository;
        private readonly IWorkflowStepRepository _stepRepository;
        private readonly IWorkflowStepInstanceRepository _stepInstanceRepository;
        private readonly IStepResolverService _stepResolverService;
        private readonly ISLACalculator _slaCalculator;
        private readonly IWorkflowNotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;

        public WorkflowService(
            IWorkflowRepository workflowRepository,
            IWorkflowTemplateRepository templateRepository,
            IWorkflowStepRepository stepRepository,
            IWorkflowStepInstanceRepository stepInstanceRepository,
            IStepResolverService stepResolverService,
            ISLACalculator slaCalculator,
            IWorkflowNotificationService notificationService,
            IAuditService auditService,
            ILoggingService loggingService)
        {
            _workflowRepository = workflowRepository;
            _templateRepository = templateRepository;
            _stepRepository = stepRepository;
            _stepInstanceRepository = stepInstanceRepository;
            _stepResolverService = stepResolverService;
            _slaCalculator = slaCalculator;
            _notificationService = notificationService;
            _auditService = auditService;
            _loggingService = loggingService;
        }

        public WorkflowInstanceResult CreateWorkflowInstance(CreateWorkflowInstanceRequest request, UserContext userContext)
        {
            try
            {
                // 1. Validate input
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.WorkflowDefinitionId <= 0)
                    throw new ArgumentException("Invalid workflow definition ID");

                // 2. Get template
                var template = _templateRepository.GetTemplate(request.WorkflowDefinitionId);
                if (template == null)
                    throw new Exception($"Workflow template {request.WorkflowDefinitionId} not found");

                if (!template.IsActive)
                    throw new Exception($"Workflow template {template.Name} is not active");

                // 3. Check permission
                if (!CanCreateWorkflowInstance(template, userContext))
                    throw new UnauthorizedAccessException("You do not have permission to create instances of this template");

                // 4. Create workflow instance
                var instance = new WorkflowInstance
                {
                    WorkflowDefinitionId = request.WorkflowDefinitionId,
                    TemplateVersion = template.Version,
                    InitiatedBy = userContext.ADUserName,
                    InitiatedDate = DateTime.Now,
                    Status = WorkflowInstanceStatus.Created,
                    InputParameters = request.InputParameters,
                    IsDeleted = false
                };

                var instanceId = _workflowRepository.CreateWorkflowInstance(instance);
                instance.WorkflowInstanceId = instanceId;

                // 5. Get first step from template
                var steps = _stepRepository.GetStepsByTemplate(request.WorkflowDefinitionId).ToList();
                var firstStep = steps.FirstOrDefault(s => s.StepType == WorkflowStepType.Start);

                if (firstStep == null)
                    throw new Exception("Template does not have a Start step");

                // 6. Create first step instance
                var firstStepInstance = new WorkflowStepInstance
                {
                    WorkflowInstanceId = instanceId,
                    WorkflowStepId = firstStep.WorkflowStepId,
                    AssignedTo = firstStep.AssignedUser ?? userContext.ADUserName,
                    AssignedDate = DateTime.Now,
                    Status = WorkflowStepInstanceStatus.Pending,
                    SLADeadline = DateTime.Now.AddHours(24), // Default SLA
                    SLAStatus = SLAStatus.OnTime,
                    IsDeleted = false
                };

                var stepInstanceId = _stepInstanceRepository.CreateStepInstance(firstStepInstance);
                firstStepInstance.WorkflowStepInstanceId = stepInstanceId;

                // 7. Update instance status
                instance.Status = WorkflowInstanceStatus.InProgress;
                instance.CurrentStepId = firstStep.WorkflowStepId;
                _workflowRepository.UpdateWorkflowInstance(instance);

                // 8. Send notification
                _notificationService.NotifyTaskAssignment(firstStepInstance, userContext);

                // 9. Audit log
                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowInstanceCreated",
                    TableName = "WorkflowInstance",
                    RecordID = instanceId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = $"Workflow instance created from template {template.Name}"
                });

                _loggingService.Info($"Workflow instance {instanceId} created by {userContext.ADUserName} from template {template.Name}");

                // 10. Return result
                return MapToResult(instance, template, steps);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error creating workflow instance: {ex.Message}", ex);
                throw;
            }
        }

        public WorkflowInstanceResult GetWorkflowInstance(int workflowInstanceId, UserContext userContext)
        {
            try
            {
                var instance = _workflowRepository.GetWorkflowInstance(workflowInstanceId);
                if (instance == null)
                    throw new Exception($"Workflow instance {workflowInstanceId} not found");

                // Check permission
                if (!CanViewWorkflowInstance(instance, userContext))
                    throw new UnauthorizedAccessException("You do not have permission to view this workflow instance");

                var template = _templateRepository.GetTemplate(instance.WorkflowDefinitionId);
                var steps = _stepRepository.GetStepsByTemplate(instance.WorkflowDefinitionId).ToList();
                var stepInstances = _stepInstanceRepository.GetStepInstancesByWorkflow(workflowInstanceId).ToList();

                return MapToResult(instance, template, steps, stepInstances);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting workflow instance {workflowInstanceId}: {ex.Message}", ex);
                throw;
            }
        }

        public PagedResult<WorkflowInstanceResult> GetWorkflowInstances(CMS.Services.DTOs.WorkflowInstanceFilter filter, UserContext userContext)
        {
            try
            {
                var dataFilter = new CMS.Data.Interfaces.WorkflowInstanceFilter
                {
                    WorkflowDefinitionId = filter.WorkflowDefinitionId,
                    Status = filter.Status.HasValue ? (byte?)filter.Status.Value : null,
                    InitiatedBy = filter.InitiatedBy,
                    FromDate = filter.FromDate,
                    ToDate = filter.ToDate,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                var result = _workflowRepository.GetWorkflowInstances(dataFilter);

                var mappedItems = result.Items.Select(instance =>
                {
                    var template = _templateRepository.GetTemplate(instance.WorkflowDefinitionId);
                    var steps = _stepRepository.GetStepsByTemplate(instance.WorkflowDefinitionId).ToList();
                    return MapToResult(instance, template, steps);
                }).ToList();

                return new PagedResult<WorkflowInstanceResult>
                {
                    Items = mappedItems,
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                };
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting workflow instances: {ex.Message}", ex);
                throw;
            }
        }

        public WorkflowStepInstanceResult CompleteWorkflowStep(CompleteWorkflowStepRequest request, UserContext userContext)
        {
            try
            {
                // 1. Get step instance
                var stepInstance = _stepInstanceRepository.GetStepInstance(request.WorkflowStepInstanceId);
                if (stepInstance == null)
                    throw new Exception($"Step instance {request.WorkflowStepInstanceId} not found");

                // 2. Check permission
                if (stepInstance.AssignedTo != userContext.ADUserName)
                    throw new UnauthorizedAccessException("You are not assigned to this step");

                // 3. Update step instance
                stepInstance.Status = WorkflowStepInstanceStatus.Completed;
                stepInstance.CompletedDate = DateTime.Now;
                stepInstance.CompletedBy = userContext.ADUserName;
                stepInstance.OutputData = request.OutputData;
                stepInstance.SLAStatus = _slaCalculator.EvaluateSLAStatus(stepInstance);

                _stepInstanceRepository.UpdateStepInstance(stepInstance);

                // 4. Get workflow instance
                var instance = _workflowRepository.GetWorkflowInstance(request.WorkflowInstanceId);

                // 5. Resolve next steps
                var currentStep = _stepRepository.GetStep(stepInstance.WorkflowStepId);
                var template = _templateRepository.GetTemplate(instance.WorkflowDefinitionId);
                var nextSteps = _stepResolverService.ResolveNextSteps(currentStep, request.OutputData, template);

                // 6. Create next step instances or complete workflow
                if (nextSteps.Count == 0 || nextSteps.Any(s => s.StepType == WorkflowStepType.End))
                {
                    instance.Status = WorkflowInstanceStatus.Completed;
                    instance.CompletedDate = DateTime.Now;
                    _workflowRepository.UpdateWorkflowInstance(instance);
                    _notificationService.NotifyWorkflowCompletion(instance, userContext);
                }
                else
                {
                    foreach (var nextStep in nextSteps)
                    {
                        var nextStepInstance = new WorkflowStepInstance
                        {
                            WorkflowInstanceId = request.WorkflowInstanceId,
                            WorkflowStepId = nextStep.WorkflowStepId,
                            AssignedTo = nextStep.AssignedUser ?? userContext.ADUserName,
                            AssignedDate = DateTime.Now,
                            Status = WorkflowStepInstanceStatus.Pending,
                            SLADeadline = DateTime.Now.AddHours(24),
                            SLAStatus = SLAStatus.OnTime,
                            IsDeleted = false
                        };

                        var nextStepInstanceId = _stepInstanceRepository.CreateStepInstance(nextStepInstance);
                        nextStepInstance.WorkflowStepInstanceId = nextStepInstanceId;
                        _notificationService.NotifyTaskAssignment(nextStepInstance, userContext);
                    }

                    instance.CurrentStepId = nextSteps.First().WorkflowStepId;
                    _workflowRepository.UpdateWorkflowInstance(instance);
                }

                // 7. Audit log
                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowStepCompleted",
                    TableName = "WorkflowStepInstance",
                    RecordID = request.WorkflowStepInstanceId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = $"Step completed with output: {request.OutputData}"
                });

                _loggingService.Info($"Workflow step {request.WorkflowStepInstanceId} completed by {userContext.ADUserName}");

                return MapToResult(stepInstance);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error completing workflow step: {ex.Message}", ex);
                throw;
            }
        }

        public WorkflowStepInstanceResult RejectWorkflowStep(RejectWorkflowStepRequest request, UserContext userContext)
        {
            try
            {
                var stepInstance = _stepInstanceRepository.GetStepInstance(request.WorkflowStepInstanceId);
                if (stepInstance == null)
                    throw new Exception($"Step instance {request.WorkflowStepInstanceId} not found");

                // Check permission
                if (stepInstance.AssignedTo != userContext.ADUserName)
                    throw new UnauthorizedAccessException("You are not assigned to this step");

                // Update step instance
                stepInstance.Status = WorkflowStepInstanceStatus.Rejected;
                stepInstance.CompletedDate = DateTime.Now;
                stepInstance.CompletedBy = userContext.ADUserName;
                stepInstance.OutputData = request.RejectionReason;

                _stepInstanceRepository.UpdateStepInstance(stepInstance);

                // Get workflow instance
                var instance = _workflowRepository.GetWorkflowInstance(request.WorkflowInstanceId);
                instance.Status = WorkflowInstanceStatus.Rejected;
                instance.CompletedDate = DateTime.Now;
                _workflowRepository.UpdateWorkflowInstance(instance);

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowStepRejected",
                    TableName = "WorkflowStepInstance",
                    RecordID = request.WorkflowStepInstanceId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = $"Step rejected: {request.RejectionReason}"
                });

                _loggingService.Info($"Workflow step {request.WorkflowStepInstanceId} rejected by {userContext.ADUserName}");

                return MapToResult(stepInstance);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error rejecting workflow step: {ex.Message}", ex);
                throw;
            }
        }

        public WorkflowInstanceResult SuspendWorkflowInstance(int workflowInstanceId, UserContext userContext)
        {
            try
            {
                var instance = _workflowRepository.GetWorkflowInstance(workflowInstanceId);
                if (instance == null)
                    throw new Exception($"Workflow instance {workflowInstanceId} not found");

                instance.Status = WorkflowInstanceStatus.Suspended;
                _workflowRepository.UpdateWorkflowInstance(instance);

                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowInstanceSuspended",
                    TableName = "WorkflowInstance",
                    RecordID = workflowInstanceId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now
                });

                var template = _templateRepository.GetTemplate(instance.WorkflowDefinitionId);
                var steps = _stepRepository.GetStepsByTemplate(instance.WorkflowDefinitionId).ToList();
                return MapToResult(instance, template, steps);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error suspending workflow instance: {ex.Message}", ex);
                throw;
            }
        }

        public WorkflowInstanceResult ResumeWorkflowInstance(int workflowInstanceId, UserContext userContext)
        {
            try
            {
                var instance = _workflowRepository.GetWorkflowInstance(workflowInstanceId);
                if (instance == null)
                    throw new Exception($"Workflow instance {workflowInstanceId} not found");

                instance.Status = WorkflowInstanceStatus.InProgress;
                _workflowRepository.UpdateWorkflowInstance(instance);

                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowInstanceResumed",
                    TableName = "WorkflowInstance",
                    RecordID = workflowInstanceId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now
                });

                var template = _templateRepository.GetTemplate(instance.WorkflowDefinitionId);
                var steps = _stepRepository.GetStepsByTemplate(instance.WorkflowDefinitionId).ToList();
                return MapToResult(instance, template, steps);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error resuming workflow instance: {ex.Message}", ex);
                throw;
            }
        }

        public List<WorkflowTaskResult> GetMyTasks(UserContext userContext)
        {
            try
            {
                var stepInstances = _stepInstanceRepository.GetStepInstancesByUser(userContext.ADUserName);
                var workflowCache  = new Dictionary<int, WorkflowInstance>();
                var templateStepCache = new Dictionary<int, IList<CMS.Domain.Models.WorkflowStep>>();
                var tasks = new List<WorkflowTaskResult>();

                foreach (var stepInstance in stepInstances)
                {
                    if (!workflowCache.TryGetValue(stepInstance.WorkflowInstanceId, out var instance))
                    {
                        instance = _workflowRepository.GetWorkflowInstance(stepInstance.WorkflowInstanceId);
                        if (instance == null)
                            continue;
                        workflowCache[instance.WorkflowInstanceId] = instance;
                    }

                    var step = _stepRepository.GetStep(stepInstance.WorkflowStepId);
                    var template = _templateRepository.GetTemplate(instance.WorkflowDefinitionId);

                    if (!templateStepCache.TryGetValue(instance.WorkflowDefinitionId, out var templateSteps))
                    {
                        templateSteps = _stepRepository.GetStepsByTemplate(instance.WorkflowDefinitionId);
                        templateStepCache[instance.WorkflowDefinitionId] = templateSteps;
                    }

                    tasks.Add(new WorkflowTaskResult
                    {
                        WorkflowStepInstanceId = stepInstance.WorkflowStepInstanceId,
                        WorkflowInstanceId = instance.WorkflowInstanceId,
                        WorkflowName = template?.Name ?? "ไม่ระบุ",
                        StepName = step?.StepName ?? "ไม่ระบุ",
                        StepDescription = step?.Description,
                        Status = stepInstance.Status,
                        AssignedDate = stepInstance.AssignedDate,
                        CompletedDate = stepInstance.CompletedDate,
                        SLADeadline = stepInstance.SLADeadline,
                        SLAStatus = stepInstance.SLAStatus,
                        CanDelegate = step?.AllowDelegation ?? false,
                        MaxDelegationDepth = step?.MaxDelegationDepth ?? 0,
                        TimeRemainingMinutes = stepInstance.SLADeadline == DateTime.MinValue
                            ? (int?)null
                            : (int)(stepInstance.SLADeadline - DateTime.Now).TotalMinutes,
                        InitiatedBy = instance.InitiatedBy,
                        StepOrder  = step?.SequenceNumber ?? 0,
                        TotalSteps = templateSteps?.Count ?? 0
                    });
                }

                return tasks
                    .OrderBy(t => t.Status == WorkflowStepInstanceStatus.Completed)
                    .ThenBy(t => t.SLADeadline)
                    .ToList();
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting my tasks: {ex.Message}", ex);
                throw;
            }
        }

        public PagedResult<WorkflowInstanceResult> GetAllWorkflows(CMS.Services.DTOs.WorkflowInstanceFilter filter, UserContext userContext)
        {
            return GetWorkflowInstances(filter, userContext);
        }

        // Helper methods
        private bool CanCreateWorkflowInstance(WorkflowDefinition template, UserContext userContext)
        {
            // TODO: Implement permission check based on user role
            return true;
        }

        private bool CanViewWorkflowInstance(WorkflowInstance instance, UserContext userContext)
        {
            // User can view if they initiated it or are assigned to a step
            if (instance.InitiatedBy == userContext.ADUserName)
                return true;

            // TODO: Check if user is assigned to any step
            return true;
        }

        private WorkflowInstanceResult MapToResult(WorkflowInstance instance, WorkflowDefinition template, List<WorkflowStep> steps, List<WorkflowStepInstance> stepInstances = null)
        {
            return new WorkflowInstanceResult
            {
                WorkflowInstanceId = instance.WorkflowInstanceId,
                WorkflowDefinitionId = instance.WorkflowDefinitionId,
                WorkflowName = template?.Name,
                TemplateName = template?.Name,
                TemplateVersion = instance.TemplateVersion,
                InitiatedBy = instance.InitiatedBy,
                InitiatedDate = instance.InitiatedDate,
                Status = instance.Status,
                CurrentStepId = instance.CurrentStepId,
                CurrentStepName = steps?.FirstOrDefault(s => s.WorkflowStepId == instance.CurrentStepId)?.StepName,
                TotalStepCount = steps?.Count(s => !s.IsDeleted) ?? 0,
                CompletedStepCount = stepInstances?.Count(si => si.Status == WorkflowStepInstanceStatus.Completed) ?? 0,
                CompletedDate = instance.CompletedDate,
                InputParameters = instance.InputParameters
            };
        }

        private WorkflowStepInstanceResult MapToResult(WorkflowStepInstance stepInstance)
        {
            return new WorkflowStepInstanceResult
            {
                WorkflowStepInstanceId = stepInstance.WorkflowStepInstanceId,
                WorkflowInstanceId = stepInstance.WorkflowInstanceId,
                WorkflowStepId = stepInstance.WorkflowStepId,
                AssignedTo = stepInstance.AssignedTo,
                AssignedDate = stepInstance.AssignedDate,
                Status = stepInstance.Status,
                CompletedDate = stepInstance.CompletedDate,
                CompletedBy = stepInstance.CompletedBy,
                OutputData = stepInstance.OutputData,
                SLADeadline = stepInstance.SLADeadline,
                SLAStatus = stepInstance.SLAStatus
            };
        }
    }
}
