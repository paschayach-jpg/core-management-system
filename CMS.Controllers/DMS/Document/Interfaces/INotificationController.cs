using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface INotificationController
    {
        IList<Notification> GetUnread(int userId);
        void MarkRead(int notificationId, UserContext user);
    }
}
