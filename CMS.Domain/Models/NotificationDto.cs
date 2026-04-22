// =============================================
// CMS Notification System
// Models/NotificationDto.cs
// Production Ready for .NET Framework 4.5
// =============================================

using System;
using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Notification data from server
    /// </summary>
    [Serializable]
    public class NotificationDto
    {
        public long NotificationId { get; set; }
        public string AppCode { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string TypeNameTH { get; set; }
        public string IconClass { get; set; }
        public string ColorCode { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string RecipientUserAd { get; set; }
        public string RecipientType { get; set; }
        public string SenderUserAd { get; set; }
        public string SenderName { get; set; }
        public string ReferenceType { get; set; }
        public string ReferenceId { get; set; }
        public string ReferenceCode { get; set; }
        public string ActionUrl { get; set; }
        public int Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string Metadata { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Get priority enum
        /// </summary>
        public NotificationPriority PriorityLevel
        {
            get
            {
                if (Priority >= 4) return NotificationPriority.Urgent;
                if (Priority == 3) return NotificationPriority.High;
                if (Priority == 2) return NotificationPriority.Normal;
                return NotificationPriority.Low;
            }
        }

        /// <summary>
        /// Time ago display (Thai)
        /// </summary>
        public string TimeAgo
        {
            get
            {
                // Handle default/invalid DateTime values
                if (CreatedDate == DateTime.MinValue || CreatedDate == default(DateTime))
                {
                    return "เมื่อสักครู่";
                }

                var span = DateTime.Now - CreatedDate;
                
                // Handle future dates (clock skew or invalid data)
                if (span.TotalSeconds < 0)
                {
                    return "เมื่อสักครู่";
                }
                
                if (span.TotalSeconds < 60) return "เมื่อสักครู่";
                if (span.TotalMinutes < 60) return string.Format("{0} นาทีที่แล้ว", (int)span.TotalMinutes);
                if (span.TotalHours < 24) return string.Format("{0} ชั่วโมงที่แล้ว", (int)span.TotalHours);
                if (span.TotalDays < 7) return string.Format("{0} วันที่แล้ว", (int)span.TotalDays);
                if (span.TotalDays < 30) return string.Format("{0} สัปดาห์ที่แล้ว", (int)(span.TotalDays / 7));
                return CreatedDate.ToString("dd MMM yyyy");
            }
        }

        /// <summary>
        /// Get Thai name for notification type
        /// </summary>
        public static string GetTypeNameTH(string typeCode)
        {
            if (string.IsNullOrEmpty(typeCode)) return "การแจ้งเตือน";

            switch (typeCode.ToUpper())
            {
                case "ASSIGN": return "มอบหมายงาน";
                case "COMMENT": return "ความคิดเห็น";
                case "APPROVAL_DOCUMENT_SUBMIT": return "ขออนุมัติเอกสาร";
                case "APPROVAL_DOCUMENT_BORROW": return "ขอยืมเอกสาร";
                case "APPROVAL_OTHER": return "ขออนุมัติอื่นๆ";
                case "DEADLINE_REMINDER": return "แจ้งเตือนกำหนด";
                case "SYSTEM_ALERT": return "แจ้งเตือนระบบ";
                case "TRANSFER": return "โอนย้ายเอกสาร";
                case "REJECT": return "ปฏิเสธ";
                case "APPROVE": return "อนุมัติ";
                default: return "การแจ้งเตือน";
            }
        }

        /// <summary>
        /// Get icon for notification type
        /// </summary>
        public static string GetIconForType(string typeCode)
        {
            if (string.IsNullOrEmpty(typeCode)) return "🔔";

            switch (typeCode.ToUpper())
            {
                case "ASSIGN": return "📋";
                case "COMMENT": return "💬";
                case "APPROVAL_DOCUMENT_SUBMIT": return "📤";
                case "APPROVAL_DOCUMENT_BORROW": return "📚";
                case "APPROVAL_OTHER": return "✅";
                case "DEADLINE_REMINDER": return "⏰";
                case "SYSTEM_ALERT": return "⚠️";
                case "TRANSFER": return "📁";
                case "REJECT": return "❌";
                case "APPROVE": return "✔️";
                default: return "🔔";
            }
        }

        /// <summary>
        /// Get default color for notification type
        /// </summary>
        public static string GetColorForType(string typeCode)
        {
            if (string.IsNullOrEmpty(typeCode)) return "#2196F3";

            switch (typeCode.ToUpper())
            {
                case "ASSIGN": return "#2196F3";
                case "COMMENT": return "#9C27B0";
                case "APPROVAL_DOCUMENT_SUBMIT": return "#FF9800";
                case "APPROVAL_DOCUMENT_BORROW": return "#00BCD4";
                case "APPROVAL_OTHER": return "#4CAF50";
                case "DEADLINE_REMINDER": return "#F44336";
                case "SYSTEM_ALERT": return "#FF5722";
                case "TRANSFER": return "#607D8B";
                case "REJECT": return "#F44336";
                case "APPROVE": return "#4CAF50";
                default: return "#2196F3";
            }
        }
    }

    /// <summary>
    /// Notification priority levels
    /// </summary>
    public enum NotificationPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }

    /// <summary>
    /// Unread count per type
    /// </summary>
    [Serializable]
    public class UnreadCountDto
    {
        public string TypeCode { get; set; }
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// Connection state
    /// </summary>
    public enum NotificationConnectionState
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Reconnecting = 3,
        Error = 4
    }

    /// <summary>
    /// API Response wrapper
    /// </summary>
    [Serializable]
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }
    }

    /// <summary>
    /// Paged result for API responses
    /// </summary>
    [Serializable]
    public class PagedResultNotification<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }

        public PagedResultNotification()
        {
            Items = new List<T>();
        }
    }
}

    #region Event Args

    /// <summary>
    /// Event arguments for notification item events
    /// </summary>
    public class NotificationItemEventArgs : EventArgs
    {
        public long NotificationId { get; set; }
        public string ActionUrl { get; set; }
        public string ReferenceType { get; set; }
        public string ReferenceId { get; set; }
    }

    #endregion
