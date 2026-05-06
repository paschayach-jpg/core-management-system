using System;
using System.Collections.Generic;

namespace CMS.Presentation.Modules.Workflow.Presenters.Interfaces
{
    public interface ISLAReportView
    {
        void DisplayMetrics(List<SLAMetricRow> metrics);
        void DisplayKpiSummary(SlaKpiSummary summary);
        void DisplayComplianceTrendChart(List<SLATrendData> trendData);
        void DisplayCycleTimeDistribution(List<CycleTimeBar> bars);
        void DisplayStatusDistributionChart(List<SLADistributionData> distributionData);
        void ExportToExcel(string filePath);
        void ShowError(string message);
        void ShowInfo(string message);
        void GetDateRange(out DateTime fromDate, out DateTime toDate);
        int? GetTemplateFilter();
        string GetUserFilter();
    }

    public class SLAMetricRow
    {
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public int Total { get; set; }
        public int OnTime { get; set; }
        public int AtRisk { get; set; }
        public int Overdue { get; set; }
        public double CompliancePercent { get; set; }
    }

    public class SlaKpiSummary
    {
        public double CompliancePercent { get; set; }
        public string ComplianceDelta { get; set; }
        public double AvgCycleTimeDays { get; set; }
        public string CycleTimeDelta { get; set; }
        public int OverdueCount { get; set; }
        public int TotalCount { get; set; }
        public int CompletedMtd { get; set; }
        public string PeriodLabel { get; set; }
    }

    public class SLATrendData
    {
        public string WeekLabel { get; set; }
        public double CompliancePercent { get; set; }
    }

    public class SLADistributionData
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }

    public class CycleTimeBar
    {
        public string Label { get; set; }
        public int Count { get; set; }
        public string Category { get; set; } // "pass" | "near" | "over"
    }
}
