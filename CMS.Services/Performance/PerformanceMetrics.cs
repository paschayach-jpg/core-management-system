using System;
using System.Collections.Generic;

namespace CMS.Services.Performance
{
    /// <summary>
    /// Performance metrics collection and reporting
    /// Supports Phase 13: Performance Optimization
    /// Requirement 21: Non-Functional Requirement - Performance
    /// 
    /// Tracks:
    /// - Workflow instance creation time
    /// - Dashboard load time
    /// - Search query response time
    /// - SLA evaluation time
    /// - Escalation processing time
    /// - Cache hit rates
    /// - Notification delivery time
    /// </summary>
    public class PerformanceMetrics
    {
        /// <summary>
        /// Workflow instance creation metrics
        /// Target: < 500ms (Requirement 21 AC1)
        /// </summary>
        public class WorkflowInstanceCreationMetrics
        {
            public int TotalCreations { get; set; }
            public double AverageTimeMs { get; set; }
            public double MinTimeMs { get; set; }
            public double MaxTimeMs { get; set; }
            public int SuccessCount { get; set; }
            public int FailureCount { get; set; }
            public double SuccessRate => TotalCreations > 0 ? (double)SuccessCount / TotalCreations : 0;
        }

        /// <summary>
        /// Dashboard load metrics
        /// Target: < 2 seconds (Requirement 21 AC2)
        /// </summary>
        public class DashboardLoadMetrics
        {
            public int TotalLoads { get; set; }
            public double AverageTimeMs { get; set; }
            public double MinTimeMs { get; set; }
            public double MaxTimeMs { get; set; }
            public int FastLoads { get; set; } // < 2000ms
            public int SlowLoads { get; set; } // >= 2000ms
            public double FastLoadPercentage => TotalLoads > 0 ? (double)FastLoads / TotalLoads * 100 : 0;
        }

        /// <summary>
        /// Search query metrics
        /// Target: < 1 second for up to 10,000 instances (Requirement 21 AC3)
        /// </summary>
        public class SearchQueryMetrics
        {
            public int TotalQueries { get; set; }
            public double AverageTimeMs { get; set; }
            public double MinTimeMs { get; set; }
            public double MaxTimeMs { get; set; }
            public int FastQueries { get; set; } // < 1000ms
            public int SlowQueries { get; set; } // >= 1000ms
            public double FastQueryPercentage => TotalQueries > 0 ? (double)FastQueries / TotalQueries * 100 : 0;
        }

        /// <summary>
        /// SLA evaluation metrics
        /// Target: Complete for all active instances within 5 minutes (Requirement 21 AC4)
        /// </summary>
        public class SLAEvaluationMetrics
        {
            public int TotalEvaluations { get; set; }
            public double AverageTimeMs { get; set; }
            public double MinTimeMs { get; set; }
            public double MaxTimeMs { get; set; }
            public int StepsEvaluated { get; set; }
            public double AverageTimePerStepMs => StepsEvaluated > 0 ? AverageTimeMs / StepsEvaluated : 0;
            public int WarningsTriggered { get; set; }
            public int EscalationsTriggered { get; set; }
        }

        /// <summary>
        /// Escalation processing metrics
        /// Target: Trigger escalation actions within 1 minute of SLA deadline (Requirement 21 AC5)
        /// </summary>
        public class EscalationProcessingMetrics
        {
            public int TotalEscalations { get; set; }
            public double AverageTimeMs { get; set; }
            public double MinTimeMs { get; set; }
            public double MaxTimeMs { get; set; }
            public int FastEscalations { get; set; } // < 60000ms (1 minute)
            public int SlowEscalations { get; set; } // >= 60000ms
            public double FastEscalationPercentage => TotalEscalations > 0 ? (double)FastEscalations / TotalEscalations * 100 : 0;
        }

        /// <summary>
        /// Cache performance metrics
        /// </summary>
        public class CachePerformanceMetrics
        {
            public int TotalRequests { get; set; }
            public int CacheHits { get; set; }
            public int CacheMisses { get; set; }
            public double HitRate => TotalRequests > 0 ? (double)CacheHits / TotalRequests * 100 : 0;
            public int CachedItemCount { get; set; }
            public double AverageHitTimeMs { get; set; }
            public double AverageMissTimeMs { get; set; }
        }

        /// <summary>
        /// Notification delivery metrics
        /// </summary>
        public class NotificationDeliveryMetrics
        {
            public int TotalNotifications { get; set; }
            public int SuccessfulDeliveries { get; set; }
            public int FailedDeliveries { get; set; }
            public double AverageDeliveryTimeMs { get; set; }
            public int QueuedNotifications { get; set; }
            public double SuccessRate => TotalNotifications > 0 ? (double)SuccessfulDeliveries / TotalNotifications * 100 : 0;
        }

        /// <summary>
        /// Database query metrics
        /// </summary>
        public class DatabaseQueryMetrics
        {
            public int TotalQueries { get; set; }
            public double AverageTimeMs { get; set; }
            public double MinTimeMs { get; set; }
            public double MaxTimeMs { get; set; }
            public int SlowQueries { get; set; } // > 1000ms
            public double SlowQueryPercentage => TotalQueries > 0 ? (double)SlowQueries / TotalQueries * 100 : 0;
        }

        /// <summary>
        /// Overall performance summary
        /// </summary>
        public class PerformanceSummary
        {
            public DateTime ReportDate { get; set; }
            public WorkflowInstanceCreationMetrics WorkflowCreation { get; set; }
            public DashboardLoadMetrics DashboardLoad { get; set; }
            public SearchQueryMetrics SearchQuery { get; set; }
            public SLAEvaluationMetrics SLAEvaluation { get; set; }
            public EscalationProcessingMetrics EscalationProcessing { get; set; }
            public CachePerformanceMetrics CachePerformance { get; set; }
            public NotificationDeliveryMetrics NotificationDelivery { get; set; }
            public DatabaseQueryMetrics DatabaseQuery { get; set; }

            /// <summary>
            /// Check if all performance targets are met
            /// </summary>
            public bool AllTargetsMet()
            {
                return WorkflowCreation.AverageTimeMs < 500 &&
                       DashboardLoad.FastLoadPercentage >= 95 &&
                       SearchQuery.FastQueryPercentage >= 95 &&
                       EscalationProcessing.FastEscalationPercentage >= 95 &&
                       CachePerformance.HitRate >= 80 &&
                       NotificationDelivery.SuccessRate >= 99;
            }

            /// <summary>
            /// Get performance report as formatted string
            /// </summary>
            public string GetReport()
            {
                var report = new System.Text.StringBuilder();
                report.AppendLine($"Performance Report - {ReportDate:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine("=".PadRight(80, '='));
                report.AppendLine();

                report.AppendLine("Workflow Instance Creation:");
                report.AppendLine($"  Average Time: {WorkflowCreation.AverageTimeMs:F2}ms (Target: <500ms)");
                report.AppendLine($"  Success Rate: {WorkflowCreation.SuccessRate:P2}");
                report.AppendLine();

                report.AppendLine("Dashboard Load:");
                report.AppendLine($"  Average Time: {DashboardLoad.AverageTimeMs:F2}ms (Target: <2000ms)");
                report.AppendLine($"  Fast Load %: {DashboardLoad.FastLoadPercentage:F2}% (Target: >95%)");
                report.AppendLine();

                report.AppendLine("Search Query:");
                report.AppendLine($"  Average Time: {SearchQuery.AverageTimeMs:F2}ms (Target: <1000ms)");
                report.AppendLine($"  Fast Query %: {SearchQuery.FastQueryPercentage:F2}% (Target: >95%)");
                report.AppendLine();

                report.AppendLine("SLA Evaluation:");
                report.AppendLine($"  Average Time: {SLAEvaluation.AverageTimeMs:F2}ms");
                report.AppendLine($"  Steps Evaluated: {SLAEvaluation.StepsEvaluated}");
                report.AppendLine($"  Warnings Triggered: {SLAEvaluation.WarningsTriggered}");
                report.AppendLine($"  Escalations Triggered: {SLAEvaluation.EscalationsTriggered}");
                report.AppendLine();

                report.AppendLine("Escalation Processing:");
                report.AppendLine($"  Average Time: {EscalationProcessing.AverageTimeMs:F2}ms (Target: <60000ms)");
                report.AppendLine($"  Fast Escalation %: {EscalationProcessing.FastEscalationPercentage:F2}% (Target: >95%)");
                report.AppendLine();

                report.AppendLine("Cache Performance:");
                report.AppendLine($"  Hit Rate: {CachePerformance.HitRate:F2}% (Target: >80%)");
                report.AppendLine($"  Cached Items: {CachePerformance.CachedItemCount}");
                report.AppendLine($"  Average Hit Time: {CachePerformance.AverageHitTimeMs:F2}ms");
                report.AppendLine();

                report.AppendLine("Notification Delivery:");
                report.AppendLine($"  Success Rate: {NotificationDelivery.SuccessRate:F2}% (Target: >99%)");
                report.AppendLine($"  Average Delivery Time: {NotificationDelivery.AverageDeliveryTimeMs:F2}ms");
                report.AppendLine($"  Queued Notifications: {NotificationDelivery.QueuedNotifications}");
                report.AppendLine();

                report.AppendLine("Database Query:");
                report.AppendLine($"  Average Time: {DatabaseQuery.AverageTimeMs:F2}ms");
                report.AppendLine($"  Slow Query %: {DatabaseQuery.SlowQueryPercentage:F2}%");
                report.AppendLine();

                report.AppendLine("Overall Status: " + (AllTargetsMet() ? "✓ ALL TARGETS MET" : "✗ TARGETS NOT MET"));

                return report.ToString();
            }
        }
    }
}
