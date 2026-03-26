namespace CMS.Domain.Models
{
    /// <summary>
    /// Input model for sending a notification to a user.
    /// </summary>
    public class NotificationRequest
    {
        public int RecipientUserId { get; set; }
        public string EventType { get; set; }

        /// <summary>Optional identifier of the related entity (folder, file, approval request, etc.).</summary>
        public string RelatedEntityId { get; set; }

        public string MessageText { get; set; }
    }
}
