using System;
using System.Collections.Generic;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow.Presenters
{
    /// <summary>
    /// Presenter for Template Approval form.
    /// Loads Draft templates, handles Approve / Reject actions, and logs audit.
    /// </summary>
    public class TemplateApprovalPresenter
    {
        private readonly ITemplateApprovalView _view;
        private readonly IWorkflowTemplateService _templateService;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;

        private int _currentPage = 1;
        private const int PageSize = 50;

        // Cache รายการที่โหลดล่าสุด เพื่อใช้ใน Approve/Reject
        private List<WorkflowTemplateResult> _pendingTemplates = new List<WorkflowTemplateResult>();

        public TemplateApprovalPresenter(
            ITemplateApprovalView view,
            IWorkflowTemplateService templateService,
            IAuditService auditService,
            ILoggingService loggingService,
            UserContext userContext)
        {
            _view            = view            ?? throw new ArgumentNullException("view");
            _templateService = templateService ?? throw new ArgumentNullException("templateService");
            _auditService    = auditService    ?? throw new ArgumentNullException("auditService");
            _loggingService  = loggingService  ?? throw new ArgumentNullException("loggingService");
            _userContext     = userContext      ?? throw new ArgumentNullException("userContext");
        }

        // ── Load ─────────────────────────────────────────────────────────────

        /// <summary>Load PendingApproval templates waiting for approval.</summary>
        public void LoadPendingTemplates()
        {
            try
            {
                var filter = new WorkflowTemplateFilter
                {
                    StatusLabel   = "PendingApproval",
                    PageNumber    = _currentPage,
                    PageSize      = PageSize,
                    SearchKeyword = _view.GetSearchFilter(),
                    Category      = _view.GetCategoryFilter()
                };

                var result = _templateService.GetTemplates(filter, _userContext);
                _pendingTemplates = new List<WorkflowTemplateResult>(result.Items);

                _view.DisplayPendingTemplates(result);
                _view.UpdatePendingCount(result.TotalCount);
                _loggingService.Info(string.Format("TemplateApproval: loaded {0} pending templates", result.TotalCount));
            }
            catch (Exception ex)
            {
                if (IsTableMissing(ex))
                {
                    _loggingService.Warn("TemplateApproval: WorkflowDefinition table not ready, showing empty list.");
                    _view.DisplayPendingTemplates(EmptyResult());
                    _view.UpdatePendingCount(0);
                    return;
                }
                _loggingService.Error("TemplateApproval LoadPending: " + ex.Message, ex);
                _view.ShowError("โหลดรายการไม่สำเร็จ: " + ex.Message);
            }
        }

        /// <summary>Search with current filter values.</summary>
        public void Search()
        {
            _currentPage = 1;
            LoadPendingTemplates();
        }

        // ── Actions ──────────────────────────────────────────────────────────

        /// <summary>Approve the selected template — sets ApprovalStatus = Active, IsActive = true.</summary>
        public void ApproveTemplate(WorkflowTemplateResult template)
        {
            if (template == null) { _view.ShowError("กรุณาเลือก Template ที่ต้องการอนุมัติ"); return; }

            try
            {
                _templateService.ApproveTemplate(template.WorkflowDefinitionId, _userContext);

                _loggingService.Info(string.Format("Template {0} approved by {1}", template.WorkflowDefinitionId, _userContext.ADUserName));
                _view.ShowInfo(string.Format("✔  อนุมัติ Template '{0}' เรียบร้อยแล้ว\n\nสถานะเปลี่ยนเป็น Active — สามารถสร้าง Workflow Instance ได้ทันที", template.Name));
                LoadPendingTemplates();
            }
            catch (Exception ex)
            {
                _loggingService.Error("ApproveTemplate: " + ex.Message, ex);
                _view.ShowError("อนุมัติไม่สำเร็จ: " + ex.Message);
            }
        }

        /// <summary>
        /// Reject the selected template — returns it to Draft status with a reason.
        /// Does NOT delete the template; the submitter can revise and re-submit.
        /// </summary>
        public void RejectTemplate(WorkflowTemplateResult template, string reason)
        {
            if (template == null) { _view.ShowError("กรุณาเลือก Template ที่ต้องการปฏิเสธ"); return; }

            try
            {
                _templateService.RejectTemplate(template.WorkflowDefinitionId, reason, _userContext);

                _loggingService.Info(string.Format("Template {0} rejected by {1}", template.WorkflowDefinitionId, _userContext.ADUserName));
                _view.ShowInfo(string.Format(
                    "✖  ปฏิเสธ Template '{0}' เรียบร้อยแล้ว\n\n" +
                    "สถานะเปลี่ยนกลับเป็น Draft\n" +
                    "ผู้ส่งสามารถแก้ไขและส่งอนุมัติใหม่ได้",
                    template.Name));
                LoadPendingTemplates();
            }
            catch (Exception ex)
            {
                _loggingService.Error("RejectTemplate: " + ex.Message, ex);
                _view.ShowError("ปฏิเสธไม่สำเร็จ: " + ex.Message);
            }
        }

        /// <summary>Returns total pending-approval count for sidebar badge.</summary>
        public int GetPendingCount()
        {
            try
            {
                var filter = new WorkflowTemplateFilter
                {
                    StatusLabel = "PendingApproval",
                    PageNumber  = 1,
                    PageSize    = 1,
                };
                return _templateService.GetTemplates(filter, _userContext).TotalCount;
            }
            catch { return 0; }
        }

        // ── Pagination ───────────────────────────────────────────────────────

        public void NextPage()     { _currentPage++; LoadPendingTemplates(); }
        public void PreviousPage() { if (_currentPage > 1) { _currentPage--; LoadPendingTemplates(); } }

        // ── Helpers ──────────────────────────────────────────────────────────

        private PagedResult<WorkflowTemplateResult> EmptyResult()
        {
            return new PagedResult<WorkflowTemplateResult>
            {
                Items      = new List<WorkflowTemplateResult>(),
                TotalCount = 0,
                PageNumber = _currentPage,
                PageSize   = PageSize
            };
        }

        private static bool IsTableMissing(Exception ex)
        {
            var msg = ex != null ? ex.Message : string.Empty;
            return msg.IndexOf("invalid object name", StringComparison.OrdinalIgnoreCase) >= 0
                && msg.IndexOf("workflow", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
