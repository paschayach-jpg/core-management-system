namespace CMS.Services.DTOs
{
    public enum DownloadDecision
    {
        /// <summary>ดาวน์โหลดได้ทันที (Admin / สิทธิ์รายคน / Authority Config)</summary>
        Direct = 1,

        /// <summary>มีคำขอที่อนุมัติแล้วและยังไม่ได้ใช้ — ดาวน์โหลดได้ 1 ครั้ง</summary>
        Approved = 2,

        /// <summary>ต้องส่งคำขออนุมัติก่อน</summary>
        NeedsApproval = 3
    }

    public class DownloadCheckResult
    {
        public DownloadDecision Decision { get; set; }

        /// <summary>RequestID ของคำขอที่อนุมัติแล้ว (เฉพาะ Decision = Approved)</summary>
        public int? ApprovalRequestId { get; set; }

        /// <summary>RequestID ของคำขอที่ยังรออนุมัติอยู่ (เฉพาะ Decision = NeedsApproval) — กันการส่งคำขอซ้ำ</summary>
        public int? PendingRequestId { get; set; }

        /// <summary>ระดับชั้นความลับของไฟล์ 1=HC, 2=SC, 3=NC, 4=PC</summary>
        public int ConfidentialityLevel { get; set; }

        /// <summary>เหตุผลที่ตัดสิน (ใช้ใน audit / ข้อความ)</summary>
        public string Reason { get; set; }

        public bool CanDownloadNow
        {
            get { return Decision == DownloadDecision.Direct || Decision == DownloadDecision.Approved; }
        }
    }
}
