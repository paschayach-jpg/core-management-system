using System;

namespace CMS.Services.BackgroundJobs
{
    /// <summary>
    /// Interface for background job scheduling
    /// </summary>
    public interface IBackgroundJobScheduler
    {
        /// <summary>
        /// Schedule a background job to run at regular intervals
        /// </summary>
        void ScheduleJob(string jobName, Action jobAction, TimeSpan interval);

        /// <summary>
        /// Start all scheduled jobs
        /// </summary>
        void StartAll();

        /// <summary>
        /// Stop all scheduled jobs
        /// </summary>
        void StopAll();

        /// <summary>
        /// Check if a job is running
        /// </summary>
        bool IsJobRunning(string jobName);
    }
}
