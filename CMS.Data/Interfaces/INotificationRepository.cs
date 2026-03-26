using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface INotificationRepository
    {
        int Insert(Notification notification);
        IEnumerable<Notification> GetUnreadByUser(int userId);
        void MarkRead(int notificationId);
    }
}
