using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class NotificationController : INotificationController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService = null)
        {
            _notificationService = notificationService;
        }

        public IList<Notification> GetUnread(int userId)
        {
            if (_notificationService == null)
                return new List<Notification>();

            return _notificationService.GetUnread(userId).ToList();
        }

        public void MarkRead(int notificationId, UserContext user)
        {
            if (_notificationService == null || user == null)
                return;

            _notificationService.MarkRead(notificationId, user);
        }
    }
}
