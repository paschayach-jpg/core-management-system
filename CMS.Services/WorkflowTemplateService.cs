using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
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
    /// Workflow template management service implementation
    /// </summary>
    public class WorkflowTemplateService : IWorkflowTemplateService
    {
        private readonly IWorkflowTemplateRepository _templateRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;

        public WorkflowTemplateService(
            IWorkflowTemplateRepository templateRepository,
            IWorkflowRepository workflowRepository,
            IAuditService auditService,
            ILoggingService loggingService)
        {
            _templateRepository = templateRepository;
            _workflowRepository = workflowRepository;
            _auditService = auditService;
            _loggingService = loggingService;
        }

        public WorkflowTemplateResult CreateTemplate(CreateTemplateRequest request, UserContext userContext)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (string.IsNullOrEmpty(request.Name))
                    throw new ArgumentException("Template name is required");

                // Create template
                var template = new WorkflowDefinition
                {
                    Name = request.Name,
                    Description = request.Description,
                    Version = 1,
                    IsActive = request.IsActive,
                    CreatedBy = userContext.ADUserName,
                    CreatedDate = DateTime.Now,
                    ModifiedBy = userContext.ADUserName,
                    ModifiedDate = DateTime.Now,
                    IsDeleted = false,
                    TemplateDefinition = request.TemplateDefinition
                };

                var templateId = _templateRepository.CreateTemplate(template);
                template.WorkflowDefinitionId = templateId;

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowTemplateCreated",
                    TableName = "WorkflowDefinition",
                    RecordID = templateId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = $"Template created: {request.Name}"
                });

                _loggingService.Info($"Workflow template {templateId} created by {userContext.ADUserName}");

                return MapToResult(template);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error creating workflow template: {ex.Message}", ex);
                throw;
            }
        }

        public WorkflowTemplateResult UpdateTemplate(UpdateTemplateRequest request, UserContext userContext)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                // Get existing template
                var template = _templateRepository.GetTemplate(request.WorkflowDefinitionId);
                if (template == null)
                    throw new Exception($"Template {request.WorkflowDefinitionId} not found");

                // Check if template has active instances
                if (HasActiveInstances(request.WorkflowDefinitionId))
                    throw new Exception("Cannot modify template with active instances");

                // Update template
                template.Name = request.Name ?? template.Name;
                template.Description = request.Description ?? template.Description;
                template.IsActive = request.IsActive;
                template.Version++;
                template.ModifiedBy = userContext.ADUserName;
                template.ModifiedDate = DateTime.Now;
                template.TemplateDefinition = request.TemplateDefinition ?? template.TemplateDefinition;

                _templateRepository.UpdateTemplate(template);

                // Audit log
                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowTemplateUpdated",
                    TableName = "WorkflowDefinition",
                    RecordID = request.WorkflowDefinitionId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue = $"Template updated: {request.ChangeDescription}"
                });

                _loggingService.Info($"Workflow template {request.WorkflowDefinitionId} updated by {userContext.ADUserName}");

                return MapToResult(template);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error updating workflow template: {ex.Message}", ex);
                throw;
            }
        }

        public WorkflowTemplateResult GetTemplate(int templateId, UserContext userContext)
        {
            try
            {
                var template = _templateRepository.GetTemplate(templateId);
                if (template == null)
                    throw new Exception($"Template {templateId} not found");

                return MapToResult(template);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting workflow template: {ex.Message}", ex);
                throw;
            }
        }

        public PagedResult<WorkflowTemplateResult> GetTemplates(CMS.Services.DTOs.WorkflowTemplateFilter filter, UserContext userContext)
        {
            try
            {
                byte? approvalStatusFilter = null;
                if (!string.IsNullOrWhiteSpace(filter.StatusLabel))
                {
                    switch (filter.StatusLabel)
                    {
                        case "Draft":           approvalStatusFilter = (byte)WorkflowTemplateStatus.Draft;           break;
                        case "PendingApproval": approvalStatusFilter = (byte)WorkflowTemplateStatus.PendingApproval; break;
                        case "Active":          approvalStatusFilter = (byte)WorkflowTemplateStatus.Active;          break;
                        case "Inactive":        approvalStatusFilter = (byte)WorkflowTemplateStatus.Inactive;        break;
                        case "Deprecated":      approvalStatusFilter = (byte)WorkflowTemplateStatus.Deprecated;      break;
                    }
                }

                var dataFilter = new CMS.Data.Interfaces.WorkflowTemplateFilter
                {
                    IsActive       = approvalStatusFilter.HasValue ? (bool?)null : filter.IsActive,
                    NameContains   = filter.SearchKeyword,
                    PageNumber     = filter.PageNumber,
                    PageSize       = filter.PageSize,
                    ApprovalStatus = approvalStatusFilter,
                    Category       = filter.Category
                };

                var result = _templateRepository.GetTemplates(dataFilter);

                var mappedItems = result.Items.Select(MapToResult).ToList();

                return new PagedResult<WorkflowTemplateResult>
                {
                    Items = mappedItems,
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                };
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting workflow templates: {ex.Message}", ex);
                throw;
            }
        }

        public void DeleteTemplate(int templateId, UserContext userContext)
        {
            try
            {
                var template = _templateRepository.GetTemplate(templateId);
                if (template == null)
                    throw new Exception($"Template {templateId} not found");

                if (HasActiveInstances(templateId))
                    throw new Exception("Cannot delete template with active instances");

                // Use SoftDeleteTemplate — UpdateTemplate does NOT set IsDeleted in its SQL
                _templateRepository.SoftDeleteTemplate(templateId, userContext.ADUserName);

                _auditService.Log(new AuditEntry
                {
                    Action = "WorkflowTemplateDeleted",
                    TableName = "WorkflowDefinition",
                    RecordID = templateId,
                    UserName = userContext.ADUserName,
                    AuditDate = DateTime.Now
                });

                _loggingService.Info($"Workflow template {templateId} deleted by {userContext.ADUserName}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error deleting workflow template: {ex.Message}", ex);
                throw;
            }
        }

        public List<WorkflowTemplateVersionResult> GetTemplateVersions(int templateId, UserContext userContext)
        {
            try
            {
                var versions = _templateRepository.GetTemplateVersions(templateId);

                return versions.Select(v => new WorkflowTemplateVersionResult
                {
                    WorkflowDefinitionId = v.WorkflowDefinitionId,
                    Version = v.Version,
                    Name = v.Name,
                    CreatedBy = v.CreatedBy,
                    CreatedDate = v.CreatedDate,
                    IsActive = v.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error getting template versions: {ex.Message}", ex);
                throw;
            }
        }

        public TemplateComparisonResult CompareTemplateVersions(int templateId, int version1, int version2, UserContext userContext)
        {
            try
            {
                var v1 = _templateRepository.GetTemplateVersion(templateId, version1);
                var v2 = _templateRepository.GetTemplateVersion(templateId, version2);

                if (v1 == null || v2 == null)
                    throw new Exception("One or both template versions not found");

                // TODO: Implement detailed comparison logic
                return new TemplateComparisonResult
                {
                    WorkflowDefinitionId = templateId,
                    Version1 = version1,
                    Version2 = version2,
                    HasDifferences = false
                };
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error comparing template versions: {ex.Message}", ex);
                throw;
            }
        }

        public bool HasActiveInstances(int templateId)
        {
            try
            {
                var filter = new CMS.Data.Interfaces.WorkflowInstanceFilter
                {
                    WorkflowDefinitionId = templateId,
                    Status = (byte)(Domain.Enums.WorkflowInstanceStatus.InProgress),
                    PageSize = 1
                };

                var result = _workflowRepository.GetWorkflowInstances(filter);
                return result.TotalCount > 0;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error checking active instances: {ex.Message}", ex);
                return false;
            }
        }

        // ── New Approval Lifecycle Methods ───────────────────────────────────────

        public WorkflowTemplateResult SubmitForApproval(int templateId, UserContext userContext)
        {
            try
            {
                var template = _templateRepository.GetTemplate(templateId);
                if (template == null)
                    throw new Exception($"Template {templateId} not found");

                if (template.ApprovalStatus != WorkflowTemplateStatus.Draft)
                    throw new Exception($"เฉพาะ Template ที่เป็น Draft เท่านั้นที่สามารถส่งอนุมัติได้ (สถานะปัจจุบัน: {template.ApprovalStatus})");

                template.ApprovalStatus  = WorkflowTemplateStatus.PendingApproval;
                template.SubmittedBy     = userContext.ADUserName;
                template.SubmittedDate   = DateTime.Now;
                template.IsActive        = false;
                template.ModifiedBy      = userContext.ADUserName;
                template.ModifiedDate    = DateTime.Now;
                template.RejectionReason = null;

                _templateRepository.UpdateTemplate(template);

                _auditService.Log(new AuditEntry
                {
                    Action    = "WorkflowTemplateSubmittedForApproval",
                    TableName = "WorkflowDefinition",
                    RecordID  = templateId,
                    UserName  = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue  = $"Template '{template.Name}' v{template.Version} submitted for approval"
                });

                _loggingService.Info($"Template {templateId} submitted for approval by {userContext.ADUserName}");
                return MapToResult(template);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"SubmitForApproval error: {ex.Message}", ex);
                throw;
            }
        }

        public WorkflowTemplateResult ApproveTemplate(int templateId, UserContext userContext)
        {
            try
            {
                var template = _templateRepository.GetTemplate(templateId);
                if (template == null)
                    throw new Exception($"Template {templateId} not found");

                if (template.ApprovalStatus != WorkflowTemplateStatus.PendingApproval)
                    throw new Exception($"เฉพาะ Template ที่รออนุมัติเท่านั้นที่สามารถอนุมัติได้ (สถานะปัจจุบัน: {template.ApprovalStatus})");

                template.ApprovalStatus = WorkflowTemplateStatus.Active;
                template.IsActive       = true;
                template.ApprovedBy     = userContext.ADUserName;
                template.ApprovedDate   = DateTime.Now;
                template.ModifiedBy     = userContext.ADUserName;
                template.ModifiedDate   = DateTime.Now;

                _templateRepository.UpdateTemplate(template);

                _auditService.Log(new AuditEntry
                {
                    Action    = "WorkflowTemplateApproved",
                    TableName = "WorkflowDefinition",
                    RecordID  = templateId,
                    UserName  = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue  = $"Template '{template.Name}' v{template.Version} approved and published"
                });

                _loggingService.Info($"Template {templateId} approved by {userContext.ADUserName}");
                return MapToResult(template);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"ApproveTemplate error: {ex.Message}", ex);
                throw;
            }
        }

        public WorkflowTemplateResult RejectTemplate(int templateId, string reason, UserContext userContext)
        {
            try
            {
                var template = _templateRepository.GetTemplate(templateId);
                if (template == null)
                    throw new Exception($"Template {templateId} not found");

                if (template.ApprovalStatus != WorkflowTemplateStatus.PendingApproval)
                    throw new Exception($"เฉพาะ Template ที่รออนุมัติเท่านั้นที่สามารถปฏิเสธได้ (สถานะปัจจุบัน: {template.ApprovalStatus})");

                // Return to Draft — do NOT delete
                template.ApprovalStatus  = WorkflowTemplateStatus.Draft;
                template.IsActive        = false;
                template.RejectionReason = reason;
                template.ApprovedBy      = userContext.ADUserName;
                template.ApprovedDate    = DateTime.Now;
                template.ModifiedBy      = userContext.ADUserName;
                template.ModifiedDate    = DateTime.Now;

                _templateRepository.UpdateTemplate(template);

                _auditService.Log(new AuditEntry
                {
                    Action    = "WorkflowTemplateRejected",
                    TableName = "WorkflowDefinition",
                    RecordID  = templateId,
                    UserName  = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue  = $"Template '{template.Name}' v{template.Version} rejected. Reason: {reason ?? "-"}"
                });

                _loggingService.Info($"Template {templateId} rejected by {userContext.ADUserName}");
                return MapToResult(template);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"RejectTemplate error: {ex.Message}", ex);
                throw;
            }
        }

        public WorkflowTemplateResult PublishTemplate(int templateId, UserContext userContext)
        {
            try
            {
                var template = _templateRepository.GetTemplate(templateId);
                if (template == null)
                    throw new Exception($"Template {templateId} not found");

                template.ApprovalStatus = WorkflowTemplateStatus.Active;
                template.IsActive       = true;
                template.ModifiedBy     = userContext.ADUserName;
                template.ModifiedDate   = DateTime.Now;

                _templateRepository.UpdateTemplate(template);

                _auditService.Log(new AuditEntry
                {
                    Action    = "WorkflowTemplatePublished",
                    TableName = "WorkflowDefinition",
                    RecordID  = templateId,
                    UserName  = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue  = $"Template '{template.Name}' v{template.Version} published directly"
                });

                _loggingService.Info($"Template {templateId} published directly by {userContext.ADUserName}");
                return MapToResult(template);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"PublishTemplate error: {ex.Message}", ex);
                throw;
            }
        }

        public WorkflowTemplateResult DeactivateTemplate(int templateId, UserContext userContext)
        {
            try
            {
                var template = _templateRepository.GetTemplate(templateId);
                if (template == null)
                    throw new Exception($"Template {templateId} not found");

                template.ApprovalStatus = WorkflowTemplateStatus.Inactive;
                template.IsActive       = false;
                template.ModifiedBy     = userContext.ADUserName;
                template.ModifiedDate   = DateTime.Now;

                _templateRepository.UpdateTemplate(template);

                _auditService.Log(new AuditEntry
                {
                    Action    = "WorkflowTemplateDeactivated",
                    TableName = "WorkflowDefinition",
                    RecordID  = templateId,
                    UserName  = userContext.ADUserName,
                    AuditDate = DateTime.Now,
                    NewValue  = $"Template '{template.Name}' v{template.Version} deactivated"
                });

                _loggingService.Info($"Template {templateId} deactivated by {userContext.ADUserName}");
                return MapToResult(template);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"DeactivateTemplate error: {ex.Message}", ex);
                throw;
            }
        }

        // ── MapToResult ──────────────────────────────────────────────────────────

        private WorkflowTemplateResult MapToResult(WorkflowDefinition template)
        {
            return new WorkflowTemplateResult
            {
                WorkflowDefinitionId = template.WorkflowDefinitionId,
                Name            = template.Name,
                Description     = template.Description,
                Version         = template.Version,
                IsActive        = template.IsActive,
                ApprovalStatus  = template.ApprovalStatus,
                CreatedBy       = template.CreatedBy,
                CreatedDate     = template.CreatedDate,
                ModifiedBy      = template.ModifiedBy,
                ModifiedDate    = template.ModifiedDate,
                SubmittedBy     = template.SubmittedBy,
                SubmittedDate   = template.SubmittedDate,
                ApprovedBy      = template.ApprovedBy,
                ApprovedDate    = template.ApprovedDate,
                RejectionReason     = template.RejectionReason,
                TemplateDefinition  = template.TemplateDefinition,
                StepCount           = template.StepCount,
                TotalInstanceCount  = template.TotalInstanceCount,
                ActiveInstanceCount = template.ActiveInstanceCount,
                OwnerDepartment     = template.CreatedBy,
                Category            = template.Category
            };
        }
    }
}
