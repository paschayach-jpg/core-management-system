using System;
using System.Collections.Generic;

namespace CMS.Services.Notifications
{
    /// <summary>
    /// Interface for asynchronous notification queue
    /// Supports Phase 13: Performance Optimization
    /// Requirement 12 AC1-7: Real-time notifications
    /// Requirement 21 AC1-5: Performance targets
    /// 
    /// Decouples notification delivery from workflow operations
    /// Allows notifications to be sent asynchronously without blocking workflow execution
    /// </summary>
    public interface IAsyncNotificationQueue
    {
        /// <summary>
        /// Enqueue a notification for asynchronous delivery
        /// </summary>
        void EnqueueNotification(NotificationMessage notification);

        /// <summary>
        /// Enqueue multiple notifications for batch delivery
        /// </summary>
        void EnqueueNotifications(IList<NotificationMessage> notifications);

        /// <summary>
        /// Process queued notifications (called by background job)
        /// </summary>
        void ProcessQueue();

        /// <summary>
        /// Get queue statistics for monitoring
        /// </summary>
        NotificationQueueStats GetStats();

        /// <summary>
        /// Clear the queue (for testing/maintenance)
        /// </summary>
        void ClearQueue();
    }

    /// <summary>
    /// Notification message for async delivery
    /// </summary>
    public class NotificationMessage
    {
        public int NotificationId { get; set; }
        public string RecipientUser { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public int? WorkflowInstanceId { get; set; }
        public int? WorkflowStepInstanceId { get; set; }
        public DateTime? DueDate { get; set; }
        public string ActionUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public int RetryCount { get; set; }
        public DateTime? LastRetryDate { get; set; }
    }

    /// <summary>
    /// Notification types
    /// </summary>
    public enum NotificationType
    {
        TaskAssignment = 1,
        SLAWarning = 2,
        SLAEscalation = 3,
        WorkflowCompletion = 4,
        TaskDelegation = 5,
        TaskRejection = 6
    }

    /// <summary>
    /// Queue statistics for monitoring
    /// </summary>
    public class NotificationQueueStats
    {
        public int QueuedCount { get; set; }
        public int ProcessedCount { get; set; }
        public int FailedCount { get; set; }
        public double AverageProcessingTimeMs { get; set; }
        public DateTime LastProcessedDate { get; set; }
    }
}
