using CMS.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CMS.Services.Notifications
{
    /// <summary>
    /// In-memory asynchronous notification queue implementation
    /// Supports Phase 13: Performance Optimization
    /// Requirement 12 AC1-7: Real-time notifications
    /// Requirement 21 AC1-5: Performance targets
    /// 
    /// Implementation Strategy:
    /// - Notifications enqueued immediately (non-blocking)
    /// - Background job processes queue periodically
    /// - Batch delivery to reduce notification service calls
    /// - Retry mechanism for failed deliveries
    /// - Thread-safe using lock mechanism
    /// 
    /// </summary>
    public class AsyncNotificationQueueImpl : IAsyncNotificationQueue
    {
        private readonly ILoggingService _loggingService;
        private Queue<NotificationMessage> _notificationQueue;
        private readonly object _lockObject = new object();
        private NotificationQueueStats _stats;

        // Configuration
        private const int MAX_RETRIES = 3;
        private const int BATCH_SIZE = 50;

        public AsyncNotificationQueueImpl(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _notificationQueue = new Queue<NotificationMessage>();
            _stats = new NotificationQueueStats();
        }

        public void EnqueueNotification(NotificationMessage notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            lock (_lockObject)
            {
                notification.CreatedDate = DateTime.UtcNow;
                notification.RetryCount = 0;
                _notificationQueue.Enqueue(notification);
                _loggingService.Debug($"Notification enqueued for {notification.RecipientUser}: {notification.Title}");
            }
        }

        public void EnqueueNotifications(IList<NotificationMessage> notifications)
        {
            if (notifications == null || notifications.Count == 0)
                return;

            lock (_lockObject)
            {
                foreach (var notification in notifications)
                {
                    notification.CreatedDate = DateTime.UtcNow;
                    notification.RetryCount = 0;
                    _notificationQueue.Enqueue(notification);
                }
                _loggingService.Debug($"Batch of {notifications.Count} notifications enqueued");
            }
        }

        public void ProcessQueue()
        {
            lock (_lockObject)
            {
                if (_notificationQueue.Count == 0)
                {
                    _loggingService.Debug("Notification queue is empty");
                    return;
                }

                _loggingService.Info($"Processing notification queue: {_notificationQueue.Count} items");

                var stopwatch = Stopwatch.StartNew();
                var batch = new List<NotificationMessage>();
                var failedNotifications = new List<NotificationMessage>();

                // Dequeue and process in batches
                while (_notificationQueue.Count > 0 && batch.Count < BATCH_SIZE)
                {
                    var notification = _notificationQueue.Dequeue();

                    try
                    {
                        // Attempt to deliver notification
                        if (DeliverNotification(notification))
                        {
                            _stats.ProcessedCount++;
                            _loggingService.Debug($"Notification delivered to {notification.RecipientUser}");
                        }
                        else
                        {
                            // Delivery failed - check retry count
                            if (notification.RetryCount < MAX_RETRIES)
                            {
                                notification.RetryCount++;
                                notification.LastRetryDate = DateTime.UtcNow;
                                failedNotifications.Add(notification);
                                _loggingService.Warn($"Notification delivery failed for {notification.RecipientUser}, retry {notification.RetryCount}/{MAX_RETRIES}");
                            }
                            else
                            {
                                _stats.FailedCount++;
                                _loggingService.Error($"Notification delivery failed after {MAX_RETRIES} retries for {notification.RecipientUser}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Error($"Error processing notification: {ex.Message}", ex);
                        _stats.FailedCount++;
                    }
                }

                // Re-enqueue failed notifications for retry
                foreach (var failedNotification in failedNotifications)
                {
                    _notificationQueue.Enqueue(failedNotification);
                }

                stopwatch.Stop();
                _stats.AverageProcessingTimeMs = stopwatch.ElapsedMilliseconds / (double)(_stats.ProcessedCount + _stats.FailedCount);
                _stats.LastProcessedDate = DateTime.UtcNow;

                _loggingService.Info($"Notification queue processing completed: {_stats.ProcessedCount} delivered, {failedNotifications.Count} retried, {_stats.FailedCount} failed in {stopwatch.ElapsedMilliseconds}ms");
            }
        }

        public NotificationQueueStats GetStats()
        {
            lock (_lockObject)
            {
                return new NotificationQueueStats
                {
                    QueuedCount = _notificationQueue.Count,
                    ProcessedCount = _stats.ProcessedCount,
                    FailedCount = _stats.FailedCount,
                    AverageProcessingTimeMs = _stats.AverageProcessingTimeMs,
                    LastProcessedDate = _stats.LastProcessedDate
                };
            }
        }

        public void ClearQueue()
        {
            lock (_lockObject)
            {
                _notificationQueue.Clear();
                _loggingService.Info("Notification queue cleared");
            }
        }

        /// <summary>
        /// Deliver a single notification
        /// </summary>
        private bool DeliverNotification(NotificationMessage notification)
        {
            try
            {
                // TODO: Integrate with MOD-007 (Notification System)
                // For now, just log the notification
                _loggingService.Info($"Notification delivered: {notification.Title} to {notification.RecipientUser}");
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error delivering notification: {ex.Message}", ex);
                return false;
            }
        }
    }
}
