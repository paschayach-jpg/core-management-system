using System;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// คำขอแชร์ไฟล์/โฟลเดอร์ภายในของผู้ใช้ (workflow.ApprovalRequest) ที่ยังไม่ได้กลายเป็นสิทธิ์จริง
    /// — รออนุมัติ / ไม่อนุมัติ / ถอนคำขอ / หมดอายุ. คำขอที่อนุมัติแล้วไม่อยู่ในรายการนี้
    /// </summary>
    public class InternalShareRequestItem
    {
        public int RequestID { get; set; }

        /// <summary>true = คำขอแชร์โฟลเดอร์ (ใช้ FolderID), false = คำขอแชร์ไฟล์ (ใช้ FileID)</summary>
        public bool IsFolder { get; set; }
        public int FileID { get; set; }
        public int FolderID { get; set; }
        public int PrincipalType { get; set; }
        public int PrincipalID { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanDownload { get; set; }

        /// <summary>วันหมดอายุของการแชร์ที่ขอ (null = ไม่มีกำหนด)</summary>
        public DateTime? ShareExpiry { get; set; }

        public DateTime RequestedDate { get; set; }

        /// <summary>วันหมดอายุของคำขอ (ถ้ายังรออนุมัติเลยวันนี้ = เกินกำหนด)</summary>
        public DateTime? RequestExpiry { get; set; }

        /// <summary>1=Pending, 3=Rejected, 4=Expired/withdrawn</summary>
        public int RequestStatus { get; set; }

        public DateTime? ResolvedDate { get; set; }
        public string ResolveRemark { get; set; }

        /// <summary>ผู้ขอถอนคำขอเอง (status 4 ที่ ResolveRemark ขึ้นต้นด้วย "Withdrawn")</summary>
        public bool IsWithdrawn { get; set; }

        public bool IsPending { get { return RequestStatus == 1; } }
    }
}
