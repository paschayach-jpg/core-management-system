using CMS.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Timers;

namespace CMS.Services.BackgroundJobs
{
    /// <summary>
    /// Simple background job scheduler implementation using System.Timers
    /// </summary>
    public class SimpleBackgroundJobScheduler : IBackgroundJobScheduler
    {
        private readonly Dictionary<string, Timer> _timers = new Dictionary<string, Timer>();
        private readonly Dictionary<string, bool> _isRunning = new Dictionary<string, bool>();
        private readonly ILoggingService _loggingService;

        public SimpleBackgroundJobScheduler(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        /// <summary>
        /// Schedule a background job to run at regular intervals
        /// </summary>
        public void ScheduleJob(string jobName, Action jobAction, TimeSpan interval)
        {
            try
            {
                if (string.IsNullOrEmpty(jobName))
                    throw new ArgumentNullException(nameof(jobName));

                if (jobAction == null)
                    throw new ArgumentNullException(nameof(jobAction));

                if (interval.TotalMilliseconds <= 0)
                    throw new ArgumentException("Interval must be greater than 0");

                // Create timer
                var timer = new Timer(interval.TotalMilliseconds);
                timer.Elapsed += (sender, e) =>
                {
                    try
                    {
                        _isRunning[jobName] = true;
                        jobAction();
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Error($"Error executing background job {jobName}: {ex.Message}", ex);
                    }
                    finally
                    {
                        _isRunning[jobName] = false;
                    }
                };

                timer.AutoReset = true;
                timer.Enabled = false; // Don't start yet

                _timers[jobName] = timer;
                _isRunning[jobName] = false;

                _loggingService.Info($"Background job '{jobName}' scheduled to run every {interval.TotalSeconds} seconds");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error scheduling background job {jobName}: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Start all scheduled jobs
        /// </summary>
        public void StartAll()
        {
            try
            {
                foreach (var kvp in _timers)
                {
                    kvp.Value.Enabled = true;
                    _loggingService.Info($"Background job '{kvp.Key}' started");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error starting background jobs: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Stop all scheduled jobs
        /// </summary>
        public void StopAll()
        {
            try
            {
                foreach (var kvp in _timers)
                {
                    kvp.Value.Enabled = false;
                    _loggingService.Info($"Background job '{kvp.Key}' stopped");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error stopping background jobs: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Check if a job is running
        /// </summary>
        public bool IsJobRunning(string jobName)
        {
            if (!_isRunning.ContainsKey(jobName))
                return false;

            return _isRunning[jobName];
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            StopAll();
            foreach (var timer in _timers.Values)
            {
                timer?.Dispose();
            }
            _timers.Clear();
        }
    }
}
