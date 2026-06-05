using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using CMS.Data.Models;
using CMS.Infrastructure.Interfaces;
using Newtonsoft.Json.Linq;
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

                // 5. Get first step from template — fall back to JSON sync when DB rows missing
                var steps = _stepRepository.GetStepsByTemplate(request.WorkflowDefinitionId).ToList();

                if (steps.Count == 0 && !string.IsNullOrWhiteSpace(template.TemplateDefinition))
                    steps = SyncStepsFromTemplateDefinition(template);

                var firstStep = steps.FirstOrDefault(s => s.StepType == WorkflowStepType.Start);

                if (firstStep == null)
                    throw new Exception("Template does not have a Start step");

                // 6. Create first step instance
                var firstStepInstance = new WorkflowStepInstance
                {
                    WorkflowInstanceId = instanceId,
                    WorkflowStepId = firstStep.WorkflowStepId,
                    AssignedTo = !string.IsNullOrWhiteSpace(firstStep.AssignedUser) ? firstStep.AssignedUser : userContext.ADUserName,
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
                return MapToResult(instance, template, steps, new List<WorkflowStepInstance> { firstStepInstance });
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

                if (stepInstance.Status == WorkflowStepInstanceStatus.Completed)
                    throw new InvalidOperationException($"Step instance {request.WorkflowStepInstanceId} has already been completed");

                // 3. Update step instance
                stepInstance.Status = WorkflowStepInstanceStatus.Completed;
                stepInstance.CompletedDate = DateTime.Now;
                stepInstance.CompletedBy = userContext.ADUserName;
                stepInstance.OutputData = !string.IsNullOrEmpty(request.OutputData) ? request.OutputData : request.Remarks;
                stepInstance.SLAStatus = _slaCalculator.EvaluateSLAStatus(stepInstance);

                _stepInstanceRepository.UpdateStepInstance(stepInstance);

                // 4. Get workflow instance
                var instance = _workflowRepository.GetWorkflowInstance(request.WorkflowInstanceId);

                // 5. Resolve next steps
                var currentStep = _stepRepository.GetStep(stepInstance.WorkflowStepId);
                var template = _templateRepository.GetTemplate(instance.WorkflowDefinitionId);
                var nextSteps = _stepResolverService.ResolveNextSteps(currentStep, request.OutputData, template);

                // Fetch other active step instances for parallel checking
                var stepInstances = _stepInstanceRepository.GetStepInstancesByWorkflow(request.WorkflowInstanceId);
                var activeStep = stepInstances.FirstOrDefault(si =>
                    si.WorkflowStepInstanceId != request.WorkflowStepInstanceId &&
                    !si.IsDeleted &&
                    (si.Status == WorkflowStepInstanceStatus.Pending ||
                     si.Status == WorkflowStepInstanceStatus.InProgress ||
                     si.Status == WorkflowStepInstanceStatus.Delegated));
                bool hasActiveSteps = activeStep != null;

                // 6. Create next step instances or complete workflow
                if (currentStep?.StepType == WorkflowStepType.End)
                {
                    // Current step IS the End node
                    if (hasActiveSteps)
                    {
                        instance.CurrentStepId = activeStep.WorkflowStepId;
                        _workflowRepository.UpdateWorkflowInstance(instance);
                        _loggingService.Info($"Workflow step {request.WorkflowStepInstanceId} reached End step, but workflow {request.WorkflowInstanceId} remains InProgress because step {activeStep.WorkflowStepInstanceId} (Step {activeStep.WorkflowStepId}) is still active.");
                    }
                    else
                    {
                        // Current step IS the End node — workflow done
                        instance.Status = WorkflowInstanceStatus.Completed;
                        instance.CompletedDate = DateTime.Now;
                        instance.CurrentStepId = currentStep.WorkflowStepId;
                        _workflowRepository.UpdateWorkflowInstance(instance);
                        _notificationService.NotifyWorkflowCompletion(instance, userContext);
                    }
                }
                else if (nextSteps.Any(s => s.StepType == WorkflowStepType.End))
                {
                    // Next step resolves to End
                    var endStep = nextSteps.First(s => s.StepType == WorkflowStepType.End);
                    if (hasActiveSteps)
                    {
                        instance.CurrentStepId = activeStep.WorkflowStepId;
                        _workflowRepository.UpdateWorkflowInstance(instance);
                        _loggingService.Info($"Workflow step {request.WorkflowStepInstanceId} resolved to End step {endStep.WorkflowStepId}, but workflow {request.WorkflowInstanceId} remains InProgress because step {activeStep.WorkflowStepInstanceId} (Step {activeStep.WorkflowStepId}) is still active.");
                    }
                    else
                    {
                        // Next step resolves to End — workflow done
                        instance.Status = WorkflowInstanceStatus.Completed;
                        instance.CompletedDate = DateTime.Now;
                        instance.CurrentStepId = endStep.WorkflowStepId;
                        _workflowRepository.UpdateWorkflowInstance(instance);
                        _notificationService.NotifyWorkflowCompletion(instance, userContext);
                    }
                }
                else if (nextSteps.Count == 0)
                {
                    // No connection records exist (template created without connection data).
                    // Fall back: advance to the next step by SequenceNumber order.
                    var allSteps = _stepRepository.GetStepsByTemplate(instance.WorkflowDefinitionId)
                                                  .OrderBy(s => s.SequenceNumber).ToList();
                    var nextBySeq = allSteps
                        .SkipWhile(s => s.WorkflowStepId != currentStep?.WorkflowStepId)
                        .Skip(1)
                        .FirstOrDefault();

                    if (nextBySeq == null || nextBySeq.StepType == WorkflowStepType.End)
                    {
                        if (hasActiveSteps)
                        {
                            instance.CurrentStepId = activeStep.WorkflowStepId;
                            _workflowRepository.UpdateWorkflowInstance(instance);
                            _loggingService.Info($"Workflow step {request.WorkflowStepInstanceId} reached sequential End step, but workflow {request.WorkflowInstanceId} remains InProgress because step {activeStep.WorkflowStepInstanceId} (Step {activeStep.WorkflowStepId}) is still active.");
                        }
                        else
                        {
                            instance.Status = WorkflowInstanceStatus.Completed;
                            instance.CompletedDate = DateTime.Now;
                            instance.CurrentStepId = nextBySeq?.WorkflowStepId ?? currentStep?.WorkflowStepId;
                            _workflowRepository.UpdateWorkflowInstance(instance);
                            _notificationService.NotifyWorkflowCompletion(instance, userContext);
                        }
                    }
                    else
                    {
                        var fbInstance = new WorkflowStepInstance
                        {
                            WorkflowInstanceId = request.WorkflowInstanceId,
                            WorkflowStepId     = nextBySeq.WorkflowStepId,
                            AssignedTo         = !string.IsNullOrWhiteSpace(nextBySeq.AssignedUser) ? nextBySeq.AssignedUser : userContext.ADUserName,
                            AssignedDate       = DateTime.Now,
                            Status             = WorkflowStepInstanceStatus.Pending,
                            SLADeadline        = DateTime.Now.AddHours(24),
                            SLAStatus          = SLAStatus.OnTime,
                            IsDeleted          = false
                        };
                        var fbId = _stepInstanceRepository.CreateStepInstance(fbInstance);
                        fbInstance.WorkflowStepInstanceId = fbId;
                        _notificationService.NotifyTaskAssignment(fbInstance, userContext);
                        instance.CurrentStepId = nextBySeq.WorkflowStepId;
                        _workflowRepository.UpdateWorkflowInstance(instance);
                        _loggingService.Warn($"No connections from step {currentStep?.WorkflowStepId} — sequential fallback to step {nextBySeq.WorkflowStepId}");
                    }
                }
                else
                {
                    // Normal path: connection records exist
                    foreach (var nextStep in nextSteps)
                    {
                        var nextStepInstance = new WorkflowStepInstance
                        {
                            WorkflowInstanceId = request.WorkflowInstanceId,
                            WorkflowStepId     = nextStep.WorkflowStepId,
                            AssignedTo         = !string.IsNullOrWhiteSpace(nextStep.AssignedUser) ? nextStep.AssignedUser : userContext.ADUserName,
                            AssignedDate       = DateTime.Now,
                            Status             = WorkflowStepInstanceStatus.Pending,
                            SLADeadline        = DateTime.Now.AddHours(24),
                            SLAStatus          = SLAStatus.OnTime,
                            IsDeleted          = false
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
                var stepInstances = _stepInstanceRepository.GetStepInstancesByWorkflow(workflowInstanceId).ToList();
                return MapToResult(instance, template, steps, stepInstances);
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
                var stepInstances = _stepInstanceRepository.GetStepInstancesByWorkflow(workflowInstanceId).ToList();
                return MapToResult(instance, template, steps, stepInstances);
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

                // Deduplicate: one row per workflow instance.
                // When the same user is assigned to multiple steps in one workflow
                // (e.g. Start=Completed AND Approval=Pending), keep the most actionable step.
                var deduped = tasks
                    .GroupBy(t => t.WorkflowInstanceId)
                    .Select(g => g.OrderBy(t => StepPriority(t.Status)).First())
                    .ToList();

                return deduped
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

        public WorkflowInstanceResult CancelWorkflowInstance(int workflowInstanceId, string reason, UserContext userContext)
        {
            try
            {
                var instance = _workflowRepository.GetWorkflowInstance(workflowInstanceId);
                if (instance == null)
                    throw new Exception($"Workflow instance {workflowInstanceId} not found");

                if (instance.Status == WorkflowInstanceStatus.Completed)
                    throw new InvalidOperationException("ไม่สามารถยกเลิก workflow ที่เสร็จสิ้นแล้ว");

                if (instance.Status == WorkflowInstanceStatus.Cancelled)
                    throw new InvalidOperationException("Workflow นี้ถูกยกเลิกไปแล้ว");

                // Cancel all pending / in-progress step instances
                var stepInstances = _stepInstanceRepository.GetStepInstancesByWorkflow(workflowInstanceId).ToList();
                foreach (var si in stepInstances)
                {
                    if (si.Status == WorkflowStepInstanceStatus.Pending ||
                        si.Status == WorkflowStepInstanceStatus.InProgress)
                    {
                        si.Status        = WorkflowStepInstanceStatus.Rejected;
                        si.CompletedDate = DateTime.Now;
                        si.CompletedBy   = userContext.ADUserName;
                        si.OutputData    = string.IsNullOrWhiteSpace(reason)
                            ? "ยกเลิกโดยผู้ใช้"
                            : "ยกเลิก: " + reason;
                        _stepInstanceRepository.UpdateStepInstance(si);
                    }
                }

                instance.Status        = WorkflowInstanceStatus.Cancelled;
                instance.CompletedDate = DateTime.Now;
                _workflowRepository.UpdateWorkflowInstance(instance);

                _auditService.Log(new AuditEntry
                {
                    Action    = "WorkflowInstanceCancelled",
                    TableName = "WorkflowInstance",
                    RecordID  = workflowInstanceId,
                    UserName  = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue  = string.IsNullOrWhiteSpace(reason) ? "ยกเลิกโดยผู้ใช้" : reason
                });

                _loggingService.Info($"Workflow instance {workflowInstanceId} cancelled by {userContext.ADUserName}. Reason: {reason}");

                var template = _templateRepository.GetTemplate(instance.WorkflowDefinitionId);
                var steps    = _stepRepository.GetStepsByTemplate(instance.WorkflowDefinitionId).ToList();
                return MapToResult(instance, template, steps, stepInstances);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error cancelling workflow instance {workflowInstanceId}: {ex.Message}", ex);
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
            // Prefer the active (Pending/InProgress) step instance to resolve the current step,
            // because instance.CurrentStepId can be stale when the DB update is lagged or skipped.
            var activeSi = stepInstances?.FirstOrDefault(si =>
                si.Status == WorkflowStepInstanceStatus.Pending ||
                si.Status == WorkflowStepInstanceStatus.InProgress);
            int? resolvedStepId = activeSi?.WorkflowStepId ?? instance.CurrentStepId;

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
                CurrentStepId = resolvedStepId,
                CurrentStepName = steps?.FirstOrDefault(s => s.WorkflowStepId == resolvedStepId)?.StepName,
                TotalStepCount = steps?.Count(s => !s.IsDeleted) ?? 0,
                CompletedStepCount = stepInstances?.Count(si => si.Status == WorkflowStepInstanceStatus.Completed) ?? 0,
                CompletedDate = instance.CompletedDate,
                InputParameters = instance.InputParameters,
                StepHistory = stepInstances?.Select(si => new WorkflowStepInstanceResult
                {
                    WorkflowStepInstanceId = si.WorkflowStepInstanceId,
                    WorkflowInstanceId = si.WorkflowInstanceId,
                    WorkflowStepId = si.WorkflowStepId,
                    StepName = steps?.FirstOrDefault(s => s.WorkflowStepId == si.WorkflowStepId)?.StepName ?? "ขั้นตอนที่ไม่รู้จัก",
                    StepType = steps?.FirstOrDefault(s => s.WorkflowStepId == si.WorkflowStepId)?.StepType ?? WorkflowStepType.Approval,
                    AssignedTo = si.AssignedTo,
                    AssignedDate = si.AssignedDate,
                    Status = si.Status,
                    CompletedDate = si.CompletedDate,
                    CompletedBy = si.CompletedBy,
                    OutputData = si.OutputData,
                    SLADeadline = si.SLADeadline,
                    SLAStatus = si.SLAStatus,
                    Remarks = si.OutputData
                }).ToList()
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
                SLAStatus = stepInstance.SLAStatus,
                Remarks = stepInstance.OutputData
            };
        }

        // Parses TemplateDefinition JSON and persists step + connection rows so the engine can run.
        private List<WorkflowStep> SyncStepsFromTemplateDefinition(WorkflowDefinition template)
        {
            try
            {
                var obj = JObject.Parse(template.TemplateDefinition);
                var stepsArr = obj["Steps"] as JArray;
                if (stepsArr == null || stepsArr.Count == 0) return new List<WorkflowStep>();

                var synced  = new List<WorkflowStep>();
                var idMap   = new Dictionary<int, int>(); // designId → dbId
                int seq     = 1;

                foreach (var s in stepsArr)
                {
                    var designId = s["WorkflowStepId"]?.ToObject<int>() ?? seq;
                    var step = new WorkflowStep
                    {
                        WorkflowDefinitionId = template.WorkflowDefinitionId,
                        StepName             = s["StepName"]?.ToString() ?? $"Step {seq}",
                        StepType             = MapStringToStepType(s["StepType"]?.ToString()),
                        AssignedUser         = string.IsNullOrWhiteSpace(s["AssignedUser"]?.ToString()) ? null : s["AssignedUser"].ToString(),
                        Description          = s["Description"]?.ToString(),
                        SequenceNumber       = seq++,
                        SLADays              = s["SLADays"] != null && int.TryParse(s["SLADays"].ToString(), out int sd) && sd > 0 ? sd : (int?)null,
                        IsDeleted            = false,
                    };
                    var dbId = _stepRepository.CreateStep(step);
                    step.WorkflowStepId = dbId;
                    idMap[designId] = dbId;
                    synced.Add(step);
                }

                var connsArr = obj["Connections"] as JArray;
                if (connsArr != null)
                {
                    int connSeq = 1;
                    foreach (var c in connsArr)
                    {
                        var fromDesign = c["FromStepId"]?.ToObject<int>() ?? 0;
                        var toDesign   = c["ToStepId"]?.ToObject<int>() ?? 0;
                        if (!idMap.TryGetValue(fromDesign, out var fromDb) ||
                            !idMap.TryGetValue(toDesign, out var toDb)) continue;

                        _stepRepository.CreateConnection(new WorkflowConnection
                        {
                            WorkflowDefinitionId = template.WorkflowDefinitionId,
                            FromStepId           = fromDb,
                            ToStepId             = toDb,
                            ConditionExpression  = c["Condition"]?.ToString(),
                            SequenceNumber       = connSeq++,
                            IsDeleted            = false,
                        });
                    }
                }

                _loggingService.Info($"Synced {synced.Count} steps from TemplateDefinition JSON for template {template.WorkflowDefinitionId}");
                return synced;
            }
            catch (Exception ex)
            {
                _loggingService.Warn($"SyncStepsFromTemplateDefinition failed for template {template.WorkflowDefinitionId}: {ex.Message}");
                return new List<WorkflowStep>();
            }
        }

        private static WorkflowStepType MapStringToStepType(string typeStr)
        {
            if (string.IsNullOrWhiteSpace(typeStr)) return WorkflowStepType.Approval;
            switch (typeStr.ToLowerInvariant())
            {
                case "start":          return WorkflowStepType.Start;
                case "end":            return WorkflowStepType.End;
                case "approval":       return WorkflowStepType.Approval;
                case "review":         return WorkflowStepType.Review;
                case "notification":   return WorkflowStepType.Notification;
                case "decision":       return WorkflowStepType.Decision;
                case "parallel":       return WorkflowStepType.Parallel;
                case "merge":          return WorkflowStepType.Merge;
                case "sub-workflow":   return WorkflowStepType.Approval;
                case "system task":    return WorkflowStepType.Approval;
                case "wait timer":     return WorkflowStepType.Approval;
                default:               return WorkflowStepType.Approval;
            }
        }

        // Lower number = higher display priority in task list.
        private static int StepPriority(WorkflowStepInstanceStatus status)
        {
            switch (status)
            {
                case WorkflowStepInstanceStatus.InProgress: return 0;
                case WorkflowStepInstanceStatus.Pending:    return 1;
                case WorkflowStepInstanceStatus.Rejected:   return 2;
                case WorkflowStepInstanceStatus.Completed:  return 3;
                default:                                     return 4;
            }
        }
    }
}
