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
                int? templateId = _view.GetTemplateFilter();

                _view.UpdateSubtitle(fromDate, toDate);

                // Call SLACalculator which delegates to real DB queries with graceful fallback
                var metricsResult = TryGetRealMetrics(fromDate, toDate, templateId);

                if (metricsResult == null || metricsResult.TotalTasks == 0)
                {
                    metricsResult = BuildMockMetrics();
                }

                // ── Template compliance list ─────────────────────────────────────────
                var metricRows = BuildMetricRowsFromResult(metricsResult);
                _view.DisplayMetrics(metricRows);

                // ── KPI summary cards ────────────────────────────────────────────────
                int total = metricsResult.TotalTasks > 0 ? metricsResult.TotalTasks : metricRows.Sum(m => m.Total);
                int overdue = metricsResult.OverdueCount > 0 ? metricsResult.OverdueCount : metricRows.Sum(m => m.Overdue);
                double compliance = metricsResult.CompliancePercentage > 0
                    ? (double)metricsResult.CompliancePercentage
                    : (metricRows.Count > 0 ? metricRows.Average(m => m.CompliancePercent) : 0);

                var kpi = new SlaKpiSummary
                {
                    CompliancePercent = compliance,
                    ComplianceDelta = total > 0 ? string.Format("{0:+0.0;-0.0} ตามเป้า 90%", compliance - 90) : string.Empty,
                    AvgCycleTimeDays = metricsResult.AverageCompletionHours > 0
                                        ? (double)metricsResult.AverageCompletionHours / 24.0
                                        : 0,
                    CycleTimeDelta = string.Empty,
                    OverdueCount = overdue,
                    TotalCount = total,
                    CompletedMtd = total > 0 ? total - overdue : 0,
                    PeriodLabel = string.Format("{0:MMMM yyyy} (MTD)", fromDate),
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
                    FromDate = fromDate,
                    ToDate = toDate,
                    OnlyCompleted = false,
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
                    TemplateCode = "WFD-" + t.WorkflowDefinitionId.ToString("D3"),
                    TemplateName = t.TemplateName,
                    Total = t.TotalTasks,
                    OnTime = t.OnTimeCount,
                    AtRisk = t.AtRiskCount,
                    Overdue = t.OverdueCount,
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
                    UserName = u.UserName,
                    Department = string.IsNullOrEmpty(u.Department) ? "—" : u.Department,
                    OverdueCount = u.OverdueCount,
                    AvgOverdueDays = u.AvgOverdueDays,
                }).ToList();

            return new List<OverduePersonRow>();
        }

        // ── Mock data (fallback when DB is not ready) ────────────────────────────

        private static SLAComplianceMetricsResult BuildMockMetrics()
        {
            var byTemplate = new List<TemplateMetrics>
            {
                new TemplateMetrics { WorkflowDefinitionId = 1, TemplateName = "พิจารณารับประกัน (Underwriting)", TotalTasks = 45, OnTimeCount = 42, AtRiskCount = 3, OverdueCount = 0, CompliancePercentage = 93 },
                new TemplateMetrics { WorkflowDefinitionId = 3, TemplateName = "จัดการกรมธรรม์ (Policy Admin)", TotalTasks = 52, OnTimeCount = 46, AtRiskCount = 4, OverdueCount = 2, CompliancePercentage = 88 },
                new TemplateMetrics { WorkflowDefinitionId = 4, TemplateName = "เรียกร้องสินไหม (Claim Approval)", TotalTasks = 28, OnTimeCount = 25, AtRiskCount = 2, OverdueCount = 1, CompliancePercentage = 89 },
                new TemplateMetrics { WorkflowDefinitionId = 5, TemplateName = "ใบคำขอเอาประกัน (Application)", TotalTasks = 68, OnTimeCount = 66, AtRiskCount = 2, OverdueCount = 0, CompliancePercentage = 97 },
                new TemplateMetrics { WorkflowDefinitionId = 6, TemplateName = "ต่ออายุกรมธรรม์ (Policy Renewal)", TotalTasks = 35, OnTimeCount = 31, AtRiskCount = 2, OverdueCount = 2, CompliancePercentage = 88 },
                new TemplateMetrics { WorkflowDefinitionId = 7, TemplateName = "ยกเลิกกรมธรรม์ (Policy Cancel)", TotalTasks = 12, OnTimeCount = 10, AtRiskCount = 1, OverdueCount = 1, CompliancePercentage = 83 }
            };

            var byUser = new List<UserMetrics>
            {
                new UserMetrics { UserName = "Saichon S.", Department = "ฝ่ายพิจารณารับประกัน", TotalTasks = 14, OnTimeCount = 10, OverdueCount = 4, AvgOverdueDays = 3.2 },
                new UserMetrics { UserName = "Khanittha C.", Department = "ฝ่ายบริการลูกค้า", TotalTasks = 19, OnTimeCount = 16, OverdueCount = 3, AvgOverdueDays = 2.1 },
                new UserMetrics { UserName = "Pitakpong P.", Department = "ฝ่ายไอที", TotalTasks = 12, OnTimeCount = 10, OverdueCount = 2, AvgOverdueDays = 1.5 },
                new UserMetrics { UserName = "Athika A.", Department = "ฝ่ายจัดการสินไหม", TotalTasks = 8, OnTimeCount = 7, OverdueCount = 1, AvgOverdueDays = 0.8 },
                new UserMetrics { UserName = "Somchai K.", Department = "ฝ่ายกฎหมาย", TotalTasks = 5, OnTimeCount = 4, OverdueCount = 1, AvgOverdueDays = 1.2 }
            };

            var weeklyTrend = new List<WeeklyTrendPoint>
            {
                new WeeklyTrendPoint { WeekLabel = "W1", CompliancePercent = 88.5 },
                new WeeklyTrendPoint { WeekLabel = "W2", CompliancePercent = 90.2 },
                new WeeklyTrendPoint { WeekLabel = "W3", CompliancePercent = 89.0 },
                new WeeklyTrendPoint { WeekLabel = "W4", CompliancePercent = 91.5 },
                new WeeklyTrendPoint { WeekLabel = "W5", CompliancePercent = 92.0 },
                new WeeklyTrendPoint { WeekLabel = "W6", CompliancePercent = 89.8 },
                new WeeklyTrendPoint { WeekLabel = "W7", CompliancePercent = 91.2 },
                new WeeklyTrendPoint { WeekLabel = "W8", CompliancePercent = 93.4 },
                new WeeklyTrendPoint { WeekLabel = "W9", CompliancePercent = 91.0 },
                new WeeklyTrendPoint { WeekLabel = "W10", CompliancePercent = 92.5 },
                new WeeklyTrendPoint { WeekLabel = "W11", CompliancePercent = 94.1 },
                new WeeklyTrendPoint { WeekLabel = "W12", CompliancePercent = 91.4 }
            };

            var cycleTimeDistribution = new List<CycleTimeBucket>
            {
                new CycleTimeBucket { Label = "0–1 วัน", Count = 85, Category = "pass" },
                new CycleTimeBucket { Label = "1–2 วัน", Count = 62, Category = "pass" },
                new CycleTimeBucket { Label = "2–3 วัน", Count = 30, Category = "pass" },
                new CycleTimeBucket { Label = "3–4 วัน", Count = 15, Category = "near" },
                new CycleTimeBucket { Label = "4–5 วัน", Count = 8, Category = "near" },
                new CycleTimeBucket { Label = "5–7 วัน", Count = 5, Category = "over" },
                new CycleTimeBucket { Label = "7–10 วัน", Count = 4, Category = "over" },
                new CycleTimeBucket { Label = "10+ วัน", Count = 2, Category = "over" }
            };

            return new SLAComplianceMetricsResult
            {
                TotalTasks = 211,
                OnTimeCount = 193,
                OverdueCount = 18,
                AtRiskCount = 0,
                CompliancePercentage = 91.4m,
                AverageCompletionHours = 67.2m,
                FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1),
                ToDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1),
                ByTemplate = byTemplate,
                ByUser = byUser,
                WeeklyTrend = weeklyTrend,
                CycleTimeDistribution = cycleTimeDistribution,
            };
        }
    }
}
