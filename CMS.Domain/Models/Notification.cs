using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// An in-system notification delivered to a user's notification inbox.
    /// </summary>
    public class Notification
    {
        public int NotificationId { get; set; }
        public int RecipientUserId { get; set; }
        public string EventType { get; set; }

        /// <summary>Identifier of the related entity (folder, file, approval request, etc.).</summary>
        public string RelatedEntityId { get; set; }

        public string MessageText { get; set; }
        public bool IsRead { get; set; }
        public DateTime DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
