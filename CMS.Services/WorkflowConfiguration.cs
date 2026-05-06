using System;
using System.Configuration;
using CMS.Infrastructure.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Workflow Engine Configuration
    /// Manages all configuration settings for the workflow engine
    /// </summary>
    public class WorkflowConfiguration
    {
        private readonly IConfigService _configService;

        public WorkflowConfiguration(IConfigService configService)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        }

        /// <summary>
        /// Default SLA duration in hours
        /// </summary>
        public int DefaultSLAHours
        {
            get
            {
                var value = _configService.Get<string>("WorkflowSLADefaultHours");
                return int.TryParse(value, out var result) ? result : 24;
            }
        }

        /// <summary>
        /// SLA warning threshold percentage (0-100)
        /// When time remaining falls below this percentage, warning is sent
        /// </summary>
        public int SLAWarningThresholdPercent
        {
            get
            {
                var value = _configService.Get<string>("WorkflowSLAWarningThresholdPercent");
                return int.TryParse(value, out var result) ? result : 75;
            }
        }

        /// <summary>
        /// Background job interval in minutes for SLA processing
        /// </summary>
        public int SLAJobIntervalMinutes
        {
            get
            {
                var value = _configService.Get<string>("WorkflowSLAJobIntervalMinutes");
                return int.TryParse(value, out var result) ? result : 5;
            }
        }

        /// <summary>
        /// Whether workflow notifications are enabled
        /// </summary>
        public bool NotificationEnabled
        {
            get
            {
                var value = _configService.Get<string>("WorkflowNotificationEnabled");
                return bool.TryParse(value, out var result) ? result : true;
            }
        }

        /// <summary>
        /// Whether email notifications are enabled
        /// </summary>
        public bool EmailNotificationEnabled
        {
            get
            {
                var value = _configService.Get<string>("WorkflowNotificationEmailEnabled");
                return bool.TryParse(value, out var result) ? result : true;
            }
        }

        /// <summary>
        /// Whether in-app notifications are enabled
        /// </summary>
        public bool InAppNotificationEnabled
        {
            get
            {
                var value = _configService.Get<string>("WorkflowNotificationInAppEnabled");
                return bool.TryParse(value, out var result) ? result : true;
            }
        }

        /// <summary>
        /// Whether workflow escalation is enabled
        /// </summary>
        public bool EscalationEnabled
        {
            get
            {
                var value = _configService.Get<string>("WorkflowEscalationEnabled");
                return bool.TryParse(value, out var result) ? result : true;
            }
        }

        /// <summary>
        /// Whether to automatically reject tasks when SLA deadline is exceeded
        /// </summary>
        public bool AutoRejectOnOverdue
        {
            get
            {
                var value = _configService.Get<string>("WorkflowAutoRejectOnOverdue");
                return bool.TryParse(value, out var result) ? result : false;
            }
        }

        /// <summary>
        /// Maximum delegation depth (how many times a task can be delegated)
        /// </summary>
        public int MaxDelegationDepth => 1;

        /// <summary>
        /// Maximum number of parallel steps allowed in a workflow
        /// </summary>
        public int MaxParallelSteps => 10;

        /// <summary>
        /// Maximum workflow instance history to keep (in days)
        /// </summary>
        public int WorkflowHistoryRetentionDays => 365;

        /// <summary>
        /// Validate configuration settings
        /// </summary>
        public bool Validate(out string errorMessage)
        {
            errorMessage = null;

            if (DefaultSLAHours <= 0)
            {
                errorMessage = "DefaultSLAHours must be greater than 0";
                return false;
            }

            if (SLAWarningThresholdPercent < 0 || SLAWarningThresholdPercent > 100)
            {
                errorMessage = "SLAWarningThresholdPercent must be between 0 and 100";
                return false;
            }

            if (SLAJobIntervalMinutes <= 0)
            {
                errorMessage = "SLAJobIntervalMinutes must be greater than 0";
                return false;
            }

            if (MaxDelegationDepth < 0)
            {
                errorMessage = "MaxDelegationDepth must be greater than or equal to 0";
                return false;
            }

            if (MaxParallelSteps <= 0)
            {
                errorMessage = "MaxParallelSteps must be greater than 0";
                return false;
            }

            return true;
        }
    }
}
