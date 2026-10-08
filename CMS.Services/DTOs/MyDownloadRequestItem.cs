using System;

namespace CMS.Services.DTOs
{
    public enum MyDownloadRequestStatus
    {
        Pending   = 1,
        Approved  = 2,
        Rejected  = 3,
        Withdrawn = 4,
        Expired   = 5,
        Downloaded = 6
    }

    /// <summary>1 แถวในหน้า "คำขอดาวน์โหลดของฉัน"</summary>
    public class MyDownloadRequestItem
    {
        public int RequestID { get; set; }
        public int FileID { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string FolderName { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        /// <summary>เหตุผลที่ผู้ขอกรอก</summary>
        public string Reason { get; set; }

        /// <summary>ผู้อนุมัติที่ส่งคำขอไป</summary>
        public string ApproverName { get; set; }

        /// <summary>ผู้ที่อนุมัติ/ปฏิเสธจริง และหมายเหตุ</summary>
        public string ResolvedByName { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string ResolveRemark { get; set; }

        public MyDownloadRequestStatus Status { get; set; }
        public string StatusText { get; set; }

        /// <summary>รออนุมัติแต่เลยวันหมดอายุแล้ว</summary>
        public bool IsOverdue { get; set; }

        /// <summary>ไฟล์ถูกลบไปแล้ว</summary>
        public bool FileMissing { get; set; }
    }
}
