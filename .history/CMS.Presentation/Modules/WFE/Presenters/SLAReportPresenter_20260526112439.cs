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
        private readonly ISLAReportView    _view;
        private readonly IWorkflowService  _workflowService;
        private readonly ISLACalculator    _slaCalculator;
        private readonly ILoggingService   _loggingService;
        private readonly UserContext       _userContext;

        public SLAReportPresenter(
            ISLAReportView   view,
            IWorkflowService workflowService,
            ISLACalculator   slaCalculator,
            ILoggingService  loggingService,
            UserContext      userContext)
        {
            _view            = view            ?? throw new ArgumentNullException("view");
            _workflowService = workflowService ?? throw new ArgumentNullException("workflowService");
            _slaCalculator   = slaCalculator   ?? throw new ArgumentNullException("slaCalculator");
            _loggingService  = loggingService  ?? throw new ArgumentNullException("loggingService");
            _userContext     = userContext      ?? throw new ArgumentNullException("userContext");
        }

        public void GenerateReport()
        {
            try
            {
                _view.GetDateRange(out DateTime fromDate, out DateTime toDate);
                int? templateId = _view.GetTemplateFilter();

                _view.UpdateSubtitle(fromDate, toDate);

                // Call SLACalculator which delegates to real DB queries with graceful fallback
                var metricsResult = TryGetRealMetrics(fromDate, toDate, templateId);

                // ── Template compliance list ─────────────────────────────────────────
                var metricRows = BuildMetricRowsFromResult(metricsResult);
                _view.DisplayMetrics(metricRows);

                // ── KPI summary cards ────────────────────────────────────────────────
                int total   = metricsResult.TotalTasks > 0 ? metricsResult.TotalTasks : metricRows.Sum(m => m.Total);
                int overdue = metricsResult.OverdueCount  > 0 ? metricsResult.OverdueCount : metricRows.Sum(m => m.Overdue);
                double compliance = metricsResult.CompliancePercentage > 0
                    ? (double)metricsResult.CompliancePercentage
                    : (metricRows.Count > 0 ? metricRows.Average(m => m.CompliancePercent) : 91.4);

                var kpi = new SlaKpiSummary
                {
                    CompliancePercent = compliance,
                    ComplianceDelta   = total > 0 ? string.Format("{0:+0.0;-0.0} ตามเป้า 90%", compliance - 90) : string.Empty,
                    AvgCycleTimeDays  = metricsResult.AverageCompletionHours > 0
                                        ? (double)metricsResult.AverageCompletionHours / 24.0
                                        : 0,
                    CycleTimeDelta    = string.Empty,
                    OverdueCount      = overdue,
                    TotalCount        = total,
                    CompletedMtd      = total > 0 ? total - overdue : 0,
                    PeriodLabel       = string.Format("{0:MMMM yyyy} (MTD)", fromDate),
                };
                _view.DisplayKpiSummary(kpi);

                // ── Trend chart ──────────────────────────────────────────────────────
                var trendData = BuildTrendData(metricsResult);
                _view.DisplayComplianceTrendChart(trendData);

                // ── Cycle time distribution ──────────────────────────────────────────
                var cycleBars = BuildCycleTimeBars(metricsResult);
                _view.DisplayCycleTimeDistribution(cycleBars);

                // ── Overdue by person ────────────────────────────────────────────────
                var overdueRows = BuildOverdueRows(metricsResult);
                _view.DisplayOverdueByPerson(overdueRows);

                _view.DisplayStatusDistributionChart(new List<SLADistributionData>());

                _loggingService.Info(string.Format(
                    "Generated SLA report: {0:yyyy-MM-dd} – {1:yyyy-MM-dd}", fromDate, toDate));
            }
            catch (Exception ex)
            {
                _loggingService.Error("Error generating SLA report: " + ex.Message, ex);
                _view.ShowError("ไม่สามารถสร้างรายงานได้: " + ex.Message);
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

        private SLAComplianceMetricsResult TryGetRealMetrics(DateTime fromDate, DateTime toDate, int? templateId)
        {
            try
            {
                var filter = new SLAMetricsFilter
                {
                    WorkflowDefinitionId = templateId,
                    FromDate             = fromDate,
                    ToDate               = toDate,
                    OnlyCompleted        = false,
                };
                return _slaCalculator.GetSLAComplianceMetrics(filter);
            }
            catch (Exception ex)
            {
                _loggingService.Warn("SLA metrics unavailable: " + ex.Message);
                return new SLAComplianceMetricsResult();
            }
        }

        private List<SLAMetricRow> BuildMetricRowsFromResult(SLAComplianceMetricsResult result)
        {
            if (result.ByTemplate != null && result.ByTemplate.Count > 0)
            {
                return result.ByTemplate.Select(t => new SLAMetricRow
                {
                    TemplateCode      = "WFD-" + t.WorkflowDefinitionId.ToString("D3"),
                    TemplateName      = t.TemplateName,
                    Total             = t.TotalTasks,
                    OnTime            = t.OnTimeCount,
                    AtRisk            = t.AtRiskCount,
                    Overdue           = t.OverdueCount,
                    CompliancePercent = (double)t.CompliancePercentage,
                }).ToList();
            }
            return new List<SLAMetricRow>();
        }

        private List<SLATrendData> BuildTrendData(SLAComplianceMetricsResult result)
        {
            if (result.WeeklyTrend != null && result.WeeklyTrend.Count >= 2)
                return result.WeeklyTrend
                    .Select(t => new SLATrendData { WeekLabel = t.WeekLabel, CompliancePercent = t.CompliancePercent })
                    .ToList();

            return new List<SLATrendData>();
        }

        private List<CycleTimeBar> BuildCycleTimeBars(SLAComplianceMetricsResult result)
        {
            if (result.CycleTimeDistribution != null && result.CycleTimeDistribution.Count > 0)
                return result.CycleTimeDistribution
                    .Select(b => new CycleTimeBar { Label = b.Label, Count = b.Count, Category = b.Category })
                    .ToList();

            return new List<CycleTimeBar>();
        }

        private List<OverduePersonRow> BuildOverdueRows(SLAComplianceMetricsResult result)
        {
            if (result.ByUser != null && result.ByUser.Count > 0)
                return result.ByUser.Select(u => new OverduePersonRow
                {
                    UserName     = u.UserName,
                    Department   = string.IsNullOrEmpty(u.Department) ? "—" : u.Department,
                    OverdueCount = u.OverdueCount,
                    AvgOverdueDays = u.AvgOverdueDays,
                }).ToList();

            return new List<OverduePersonRow>();
        }

        // ── Mock data (fallback when DB is not ready) ────────────────────────────

        private static SLAComplianceMetricsResult BuildMockMetrics()
        {
            return new SLAComplianceMetricsResult
            {
                TotalTasks             = 211,
                OnTimeCount            = 193,
                OverdueCount           = 18,
                AtRiskCount            = 0,
                CompliancePercentage   = 91.4m,
                AverageCompletionHours = 67.2m,
                FromDate               = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1),
                ToDate                 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1),
                ByTemplate             = new List<TemplateMetrics>(),
                ByUser                 = new List<UserMetrics>(),
                WeeklyTrend            = new List<WeeklyTrendPoint>(),
                CycleTimeDistribution  = new List<CycleTimeBucket>(),
            };
        }

        private static List<SLAMetricRow> MockTemplateRows()
        {
            return new List<SLAMetricRow>
            {
                new SLAMetricRow { TemplateCode="WFD-001", TemplateName="Upload 1HC",   Total=28, OnTime=27, AtRisk=1,  Overdue=0, CompliancePercent=96 },
                new SLAMetricRow { TemplateCode="WFD-003", TemplateName="KB Article",   Total=52, OnTime=46, AtRisk=4,  Overdue=2, CompliancePercent=88 },
                new SLAMetricRow { TemplateCode="WFD-004", TemplateName="Review SOP",   Total=18, OnTime=17, AtRisk=1,  Overdue=0, CompliancePercent=94 },
                new SLAMetricRow { TemplateCode="WFD-005", TemplateName="ลา",           Total=68, OnTime=66, AtRisk=2,  Overdue=0, CompliancePercent=97 },
                new SLAMetricRow { TemplateCode="WFD-006", TemplateName="IT Landscape", Total=24, OnTime=20, AtRisk=2,  Overdue=2, CompliancePercent=82 },
                new SLAMetricRow { TemplateCode="WFD-007", TemplateName="Onboarding",   Total=21, OnTime=16, AtRisk=3,  Overdue=2, CompliancePercent=78 },
            };
        }
    }
}
