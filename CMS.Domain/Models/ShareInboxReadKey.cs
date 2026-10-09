using System;

namespace CMS.Domain.Models
{
    public enum ShareInboxKind : byte
    {
        Folder = 1,
        File = 2
    }

    /// <summary>
    /// Model แทนสถานะอ่านแล้วของรายการแชร์ (Share Inbox Read Entry)
    /// </summary>
    public class ShareInboxReadKey
    {
        public int UserID { get; set; }
        public byte ShareKind { get; set; }
        public int PermissionID { get; set; }
        public DateTime ReadDate { get; set; }

        public ShareInboxReadKey() { }

        public ShareInboxReadKey(int userId, byte shareKind, int permissionId)
        {
            UserID = userId;
            ShareKind = shareKind;
            PermissionID = permissionId;
            ReadDate = DateTime.Now;
        }

        public ShareInboxReadKey(int userId, ShareInboxKind kind, int permissionId)
            : this(userId, (byte)kind, permissionId)
        {
        }
    }
}
