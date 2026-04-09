using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Input DTO used by callers to submit an audit event to AuditService.Log().
    /// Maps to AuditLog for persistence.
    /// </summary>
    public class AuditEntry
    {
        public string Module { get; set; }
        public string Action { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int? UserRole { get; set; }
        public string TableName { get; set; }
        public int? RecordID { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public int Result { get; set; }
        public string ErrorMessage { get; set; }
        public string IPAddress { get; set; }
        public string MachineName { get; set; }
        public string SessionID { get; set; }
        private DateTime _auditDate;
        public DateTime AuditDate
        {
            get { return _auditDate == default(DateTime) ? DateTime.Now : _auditDate; }
            set { _auditDate = value; }
        }

        // -----------------------------------------------------------------------
        // Aliases used by presentation / test layers
        // -----------------------------------------------------------------------

        /// <summary>Alias for Action.</summary>
        public string EventType
        {
            get { return Action; }
            set { Action = value; }
        }

        /// <summary>Alias for UserID.</summary>
        public int ActorUserId
        {
            get { return UserID; }
            set { UserID = value; }
        }

        /// <summary>Alias for TableName.</summary>
        public string TargetEntityType
        {
            get { return TableName; }
            set { TableName = value; }
        }

        /// <summary>Alias for RecordID as string.</summary>
        public string TargetEntityId
        {
            get { return RecordID.HasValue ? RecordID.Value.ToString() : null; }
            set
            {
                int parsed;
                RecordID = int.TryParse(value, out parsed) ? (int?)parsed : null;
            }
        }

        /// <summary>Alias for Result as string (Success/Failure/Denied).</summary>
        public string Outcome
        {
            get
            {
                switch (Result)
                {
                    case 1: return "Success";
                    case 2: return "Failure";
                    case 3: return "Denied";
                    default: return Result.ToString();
                }
            }
            set
            {
                switch (value)
                {
                    case "Success": Result = 1; break;
                    case "Failure": Result = 2; break;
                    case "Denied":  Result = 3; break;
                    default:
                        int parsed;
                        Result = int.TryParse(value, out parsed) ? parsed : 1;
                        break;
                }
            }
        }

        /// <summary>Alias for MachineName.</summary>
        public string WorkstationId
        {
            get { return MachineName; }
            set { MachineName = value; }
        }

        /// <summary>Alias for AuditDate.</summary>
        public DateTime EventTimestamp
        {
            get { return AuditDate; }
            set { AuditDate = value; }
        }
    }
}