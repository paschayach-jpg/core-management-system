using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Services.SLA
{
    /// <summary>
    /// Optimized SLA calculator implementation with batch processing
    /// Supports Phase 13: Performance Optimization
    /// Requirement 7 AC1-7: SLA Management
    /// Requirement 21 AC4-5: Performance targets for SLA evaluation
    /// 
    /// Optimization Techniques:
    /// - Batch processing reduces database round-trips
    /// - SLA rule caching reduces lookups
    /// - Efficient escalation grouping
    /// - Minimized object allocations
    /// </summary>
    public class OptimizedSLACalculatorImpl : IOptimizedSLACalculator
    {
        private readonly IWorkflowStepRepository _stepRepository;
        private readonly ILoggingService _loggingService;
        private Dictionary<int, WFSLARule> _slaRuleCache;
        private readonly object _lockObject = new object();

        public OptimizedSLACalculatorImpl(
            IWorkflowStepRepository stepRepository,
            ILoggingService loggingService)
        {
            _stepRepository = stepRepository;
            _loggingService = loggingService;
            _slaRuleCache = new Dictionary<int, WFSLARule>();
        }

        public DateTime CalculateSLADeadline(WorkflowStepInstance stepInstance, WFSLARule slaRule)
        {
            if (stepInstance == null)
                throw new ArgumentNullException(nameof(stepInstance));

            if (slaRule == null)
                throw new ArgumentNullException(nameof(slaRule));

            // Calculate deadline: assignment date + SLA duration
            var deadline = stepInstance.AssignedDate.AddHours(slaRule.SLADurationHours);
            return deadline;
        }

        public SLAStatus EvaluateSLAStatus(WorkflowStepInstance stepInstance)
        {
            if (stepInstance == null)
                throw new ArgumentNullException(nameof(stepInstance));

            var now = DateTime.UtcNow;
            var timeRemaining = stepInstance.SLADeadline - now;

            // Overdue: deadline has passed
            if (timeRemaining.TotalMinutes < 0)
                return SLAStatus.Overdue;

            // At Risk: less than 25% of time remaining (warning threshold)
            // Assuming 75% warning threshold from design
            var slaRule = GetSLARuleForStep(stepInstance.WorkflowStepId);
            if (slaRule != null)
            {
                var totalDuration = TimeSpan.FromHours(slaRule.SLADurationHours);
                var warningThreshold = totalDuration.TotalMinutes * (slaRule.WarningThresholdPercent / 100.0);

                if (timeRemaining.TotalMinutes < warningThreshold)
                    return SLAStatus.AtRisk;
            }

            // On Time: still within SLA
            return SLAStatus.OnTime;
        }

        public IList<SLAEvaluationResult> BatchEvaluateSLAStatus(IList<WorkflowStepInstance> stepInstances)
        {
            if (stepInstances == null || stepInstances.Count == 0)
                return new List<SLAEvaluationResult>();

            var results = new List<SLAEvaluationResult>(stepInstances.Count);
            var now = DateTime.UtcNow;

            // Pre-load all SLA rules for the steps
            var stepIds = stepInstances.Select(s => s.WorkflowStepId).Distinct().ToList();
            var slaRules = new Dictionary<int, WFSLARule>();

            foreach (var stepId in stepIds)
            {
                var rule = GetSLARuleForStep(stepId);
                if (rule != null)
                    slaRules[stepId] = rule;
            }

            // Evaluate each step
            foreach (var stepInstance in stepInstances)
            {
                var timeRemaining = stepInstance.SLADeadline - now;
                var status = SLAStatus.OnTime;
                var requiresWarning = false;
                var requiresEscalation = false;

                if (timeRemaining.TotalMinutes < 0)
                {
                    status = SLAStatus.Overdue;
                    requiresEscalation = true;
                }
                else if (slaRules.ContainsKey(stepInstance.WorkflowStepId))
                {
                    var rule = slaRules[stepInstance.WorkflowStepId];
                    var totalDuration = TimeSpan.FromHours(rule.SLADurationHours);
                    var warningThreshold = totalDuration.TotalMinutes * (rule.WarningThresholdPercent / 100.0);

                    if (timeRemaining.TotalMinutes < warningThreshold)
                    {
                        status = SLAStatus.AtRisk;
                        requiresWarning = true;
                    }
                }

                results.Add(new SLAEvaluationResult
                {
                    WorkflowStepInstanceId = stepInstance.WorkflowStepInstanceId,
                    Status = status,
                    Deadline = stepInstance.SLADeadline,
                    MinutesRemaining = (int)timeRemaining.TotalMinutes,
                    RequiresWarning = requiresWarning,
                    RequiresEscalation = requiresEscalation
                });
            }

            _loggingService.Debug($"Batch evaluated SLA status for {stepInstances.Count} steps");
            return results;
        }

        public bool CheckAndEscalateSLA(WorkflowStepInstance stepInstance)
        {
            if (stepInstance == null)
                throw new ArgumentNullException(nameof(stepInstance));

            var status = EvaluateSLAStatus(stepInstance);
            if (status != SLAStatus.Overdue)
                return false;

            // Get SLA rule for escalation action
            var slaRule = GetSLARuleForStep(stepInstance.WorkflowStepId);
            if (slaRule == null)
                return false;

            // TODO: Execute escalation action based on slaRule.EscalationAction
            _loggingService.Warn($"SLA escalation triggered for step {stepInstance.WorkflowStepInstanceId}: {slaRule.EscalationAction}");
            return true;
        }

        public IList<SLAEscalationResult> BatchCheckAndEscalateSLA(IList<WorkflowStepInstance> stepInstances)
        {
            if (stepInstances == null || stepInstances.Count == 0)
                return new List<SLAEscalationResult>();

            var results = new List<SLAEscalationResult>();
            var now = DateTime.UtcNow;

            // Pre-load all SLA rules
            var stepIds = stepInstances.Select(s => s.WorkflowStepId).Distinct().ToList();
            var slaRules = new Dictionary<int, WFSLARule>();

            foreach (var stepId in stepIds)
            {
                var rule = GetSLARuleForStep(stepId);
                if (rule != null)
                    slaRules[stepId] = rule;
            }

            // Group steps by escalation action for batch processing
            var escalationGroups = new Dictionary<EscalationAction, List<WorkflowStepInstance>>();

            foreach (var stepInstance in stepInstances)
            {
                // Check if overdue
                if (stepInstance.SLADeadline <= now && slaRules.ContainsKey(stepInstance.WorkflowStepId))
                {
                    var rule = slaRules[stepInstance.WorkflowStepId];
                    var action = (EscalationAction)rule.EscalationAction;

                    if (!escalationGroups.ContainsKey(action))
                        escalationGroups[action] = new List<WorkflowStepInstance>();

                    escalationGroups[action].Add(stepInstance);
                }
            }

            // Process each escalation group
            foreach (var group in escalationGroups)
            {
                var action = group.Key;
                var steps = group.Value;

                _loggingService.Info($"Processing batch escalation: {action} for {steps.Count} steps");

                foreach (var stepInstance in steps)
                {
                    try
                    {
                        var rule = slaRules[stepInstance.WorkflowStepId];
                        var result = new SLAEscalationResult
                        {
                            WorkflowStepInstanceId = stepInstance.WorkflowStepInstanceId,
                            WorkflowInstanceId = stepInstance.WorkflowInstanceId,
                            AssignedTo = stepInstance.AssignedTo,
                            Action = action,
                            EscalationRecipient = rule.EscalationRecipient,
                            EscalationTime = now,
                            Success = true
                        };

                        // TODO: Execute escalation action
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Error($"Error escalating SLA for step {stepInstance.WorkflowStepInstanceId}: {ex.Message}", ex);
                        results.Add(new SLAEscalationResult
                        {
                            WorkflowStepInstanceId = stepInstance.WorkflowStepInstanceId,
                            WorkflowInstanceId = stepInstance.WorkflowInstanceId,
                            AssignedTo = stepInstance.AssignedTo,
                            Action = action,
                            EscalationTime = now,
                            Success = false,
                            ErrorMessage = ex.Message
                        });
                    }
                }
            }

            _loggingService.Info($"Batch escalation completed: {results.Count} escalations processed");
            return results;
        }

        public SLAComplianceMetrics GetSLAComplianceMetrics(DateTime fromDate, DateTime toDate)
        {
            // TODO: Implement using optimized stored procedure
            // sp_WF_GetSLAComplianceMetrics
            return new SLAComplianceMetrics
            {
                TotalTasks = 0,
                OnTimeTasks = 0,
                OverdueTasks = 0,
                AtRiskTasks = 0,
                CompliancePercentage = 0,
                AverageCompletionTimeMinutes = 0,
                EscalationCount = 0
            };
        }

        /// <summary>
        /// Get SLA rule for a step, with caching
        /// </summary>
        private WFSLARule GetSLARuleForStep(int workflowStepId)
        {
            lock (_lockObject)
            {
                if (_slaRuleCache.ContainsKey(workflowStepId))
                    return _slaRuleCache[workflowStepId];

                // TODO: Load from repository
                // var rule = _slaRuleRepository.GetRuleByStep(workflowStepId);
                // if (rule != null)
                //     _slaRuleCache[workflowStepId] = rule;
                // return rule;

                return null;
            }
        }

        /// <summary>
        /// Invalidate SLA rule cache
        /// </summary>
        public void InvalidateSLARuleCache()
        {
            lock (_lockObject)
            {
                _slaRuleCache.Clear();
                _loggingService.Debug("SLA rule cache invalidated");
            }
        }
    }
}
