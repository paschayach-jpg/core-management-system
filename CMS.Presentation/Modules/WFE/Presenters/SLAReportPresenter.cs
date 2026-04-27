using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow.Presenters
{
    public class SLAReportPresenter
    {
        private readonly ISLAReportView _view;
        private readonly IWorkflowService _workflowService;
        private readonly ISLACalculator _slaCalculator;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;

        public SLAReportPresenter(
            ISLAReportView view,
            IWorkflowService workflowService,
            ISLACalculator slaCalculator,
            ILoggingService loggingService,
            UserContext userContext)
        {
            _view = view ?? throw new ArgumentNullException("view");
            _workflowService = workflowService ?? throw new ArgumentNullException("workflowService");
            _slaCalculator = slaCalculator ?? throw new ArgumentNullException("slaCalculator");
            _loggingService = loggingService ?? throw new ArgumentNullException("loggingService");
            _userContext = userContext ?? throw new ArgumentNullException("userContext");
        }

        public void GenerateReport()
        {
            try
            {
                _view.GetDateRange(out DateTime fromDate, out DateTime toDate);
                var templateId = _view.GetTemplateFilter();

                var items = LoadWorkflowInstancesSafely(templateId, fromDate, toDate);

                var metrics = BuildMetrics(items);
                _view.DisplayMetrics(metrics);

                var kpi = new SlaKpiSummary
                {
                    CompliancePercent = metrics.Count > 0 ? metrics.Average(m => m.CompliancePercent) : 0,
                    ComplianceDelta = "+2.1 ตามเป้า 90%",
                    AvgCycleTimeDays = 2.8,
                    CycleTimeDelta = "-0.4 จากเดือนก่อน",
                    OverdueCount = metrics.Sum(m => m.Overdue),
                    TotalCount = metrics.Sum(m => m.Total),
                    CompletedMtd = items.Count,
                    PeriodLabel = string.Format("เมษายน {0} (MTD)", fromDate.Year + 543),
                };
                _view.DisplayKpiSummary(kpi);

                _view.DisplayComplianceTrendChart(BuildTrendData());
                _view.DisplayCycleTimeDistribution(BuildCycleTimeBars());
                _view.DisplayStatusDistributionChart(BuildDistribution(items));

                _loggingService.Info(string.Format(
                    "Generated SLA report: {0:yyyy-MM-dd} – {1:yyyy-MM-dd}", fromDate, toDate));
            }
            catch (Exception ex)
            {
                _loggingService.Error("Error generating SLA report: " + ex.Message, ex);
                _view.ShowError("ไม่สามารถสร้างรายงานได้: " + ex.Message);
            }
        }

        private List<WorkflowInstanceResult> LoadWorkflowInstancesSafely(int? templateId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var filter = new WorkflowInstanceFilter
                {
                    WorkflowDefinitionId = templateId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    PageNumber = 1,
                    PageSize = 1000
                };

                var instances = _workflowService.GetWorkflowInstances(filter, _userContext);
                return instances != null && instances.Items != null
                    ? instances.Items.ToList()
                    : new List<WorkflowInstanceResult>();
            }
            catch (Exception ex)
            {
                // Temporary fallback: allow SLA UI to open even when workflow tables are not ready.
                _loggingService.Warn("SLA report fallback to mock data. Unable to load workflow instances: " + ex.Message);
                return new List<WorkflowInstanceResult>();
            }
        }

        public void ExportToExcel(string filePath)
        {
            try
            {
                _view.ExportToExcel(filePath);
                _view.ShowInfo("Export สำเร็จ: " + filePath);
                _loggingService.Info("SLA report exported to " + filePath);
            }
            catch (Exception ex)
            {
                _loggingService.Error("Error exporting: " + ex.Message, ex);
                _view.ShowError("Export ไม่สำเร็จ: " + ex.Message);
            }
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private List<SLAMetricRow> BuildMetrics(List<WorkflowInstanceResult> items)
        {
            // TODO: replace with real SLA calculation via _slaCalculator
            return new List<SLAMetricRow>
            {
                new SLAMetricRow { TemplateCode="WFD-001", TemplateName="Upload 1HC",      Total=28, OnTime=27, AtRisk=1,  Overdue=0, CompliancePercent=96 },
                new SLAMetricRow { TemplateCode="WFD-003", TemplateName="KB Article",      Total=52, OnTime=46, AtRisk=4,  Overdue=2, CompliancePercent=88 },
                new SLAMetricRow { TemplateCode="WFD-004", TemplateName="Review SOP",      Total=18, OnTime=17, AtRisk=1,  Overdue=0, CompliancePercent=94 },
                new SLAMetricRow { TemplateCode="WFD-005", TemplateName="ลา",              Total=68, OnTime=66, AtRisk=2,  Overdue=0, CompliancePercent=97 },
                new SLAMetricRow { TemplateCode="WFD-006", TemplateName="IT Landscape",    Total=24, OnTime=20, AtRisk=2,  Overdue=2, CompliancePercent=82 },
                new SLAMetricRow { TemplateCode="WFD-007", TemplateName="Onboarding",      Total=21, OnTime=16, AtRisk=3,  Overdue=2, CompliancePercent=78 },
            };
        }

        private List<SLATrendData> BuildTrendData()
        {
            var values = new double[] { 83, 85, 84, 87, 88, 86, 89, 89, 90, 90, 91, 91 };
            var result = new List<SLATrendData>();
            for (int i = 0; i < values.Length; i++)
                result.Add(new SLATrendData { WeekLabel = "W" + (i + 1), CompliancePercent = values[i] });
            return result;
        }

        private List<CycleTimeBar> BuildCycleTimeBars()
        {
            return new List<CycleTimeBar>
            {
                new CycleTimeBar { Label = "0–1",  Count = 8,  Category = "pass" },
                new CycleTimeBar { Label = "1–2",  Count = 42, Category = "pass" },
                new CycleTimeBar { Label = "2–3",  Count = 68, Category = "pass" },
                new CycleTimeBar { Label = "3–4",  Count = 38, Category = "near" },
                new CycleTimeBar { Label = "4–5",  Count = 21, Category = "near" },
                new CycleTimeBar { Label = "5–7",  Count = 12, Category = "over" },
                new CycleTimeBar { Label = "7–10", Count = 4,  Category = "over" },
                new CycleTimeBar { Label = "10+",  Count = 2,  Category = "over" },
            };
        }

        private List<SLADistributionData> BuildDistribution(List<WorkflowInstanceResult> items)
        {
            return new List<SLADistributionData>
            {
                new SLADistributionData { Status = "On Time",  Count = (int)(items.Count * 0.75) },
                new SLADistributionData { Status = "At Risk",  Count = (int)(items.Count * 0.15) },
                new SLADistributionData { Status = "Overdue",  Count = (int)(items.Count * 0.10) },
            };
        }
    }
}
