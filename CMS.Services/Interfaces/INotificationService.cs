using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Manages in-system notifications: sending, retrieving unread, and marking as read.
    /// Satisfies Requirements 13.1–13.5.
    /// </summary>
    public interface INotificationService
    {
        void Send(NotificationRequest request);
        IEnumerable<Notification> GetUnread(int userId);
        void MarkRead(int notificationId, UserContext user);
    }
}
