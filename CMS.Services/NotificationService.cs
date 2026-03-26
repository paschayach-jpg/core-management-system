using System;
using System.Collections.Generic;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILoggingService         _loggingService;

        public NotificationService(
            INotificationRepository notificationRepository,
            ILoggingService         loggingService)
        {
            if (notificationRepository == null) throw new ArgumentNullException("notificationRepository");
            if (loggingService         == null) throw new ArgumentNullException("loggingService");

            _notificationRepository = notificationRepository;
            _loggingService         = loggingService;
        }

        public void Send(NotificationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (request.RecipientUserId <= 0)
                throw new ArgumentException("RecipientUserId must be greater than zero.", "request");
            if (string.IsNullOrEmpty(request.MessageText))
                throw new ArgumentException("MessageText must not be empty.", "request");

            try
            {
                var notification = new Notification
                {
                    RecipientUserId = request.RecipientUserId,
                    EventType       = request.EventType,
                    RelatedEntityId = request.RelatedEntityId,
                    MessageText     = request.MessageText,
                    IsRead          = false,
                    DeliveredAt     = DateTime.Now
                };

                _notificationRepository.Insert(notification);

                _loggingService.Info(string.Format(
                    "NotificationService.Send: Notification sent to user {0}, EventType='{1}'.",
                    request.RecipientUserId, request.EventType));
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "NotificationService.Send: Failed to send notification to user {0}.",
                    request.RecipientUserId), ex);
                throw;
            }
        }

        public IEnumerable<Notification> GetUnread(int userId)
        {
            try
            {
                return _notificationRepository.GetUnreadByUser(userId);
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "NotificationService.GetUnread: Failed to retrieve notifications for user {0}.", userId), ex);
                throw;
            }
        }

        public void MarkRead(int notificationId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");

            try
            {
                _notificationRepository.MarkRead(notificationId);

                _loggingService.Info(string.Format(
                    "NotificationService.MarkRead: Notification {0} marked as read by user {1}.",
                    notificationId, user.UserID));
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "NotificationService.MarkRead: Failed to mark notification {0} as read.", notificationId), ex);
                throw;
            }
        }
    }
}
