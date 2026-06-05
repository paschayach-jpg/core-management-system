using System;
using CMS.Domain.Enums;

namespace CMS.Domain.Models
{
    /// <summary>
    /// คำขออนุมัติเปลี่ยนขนาด quota พื้นที่ folder
    /// Maps to dms.FolderQuotaChangeRequest
    /// </summary>
    public class FolderQuotaChangeRequest
    {
        public int    RequestID            { get; set; }
        public int    OrgUnitID            { get; set; }
        public long   CurrentQuotaBytes    { get; set; }
        public long   RequestedQuotaBytes  { get; set; }
        public string Reason               { get; set; }
        public QuotaRequestStatus Status   { get; set; }
        public int    RequestedBy          { get; set; }
        public DateTime RequestedDate      { get; set; }
        public int?   ReviewedBy           { get; set; }
        public DateTime? ReviewedDate      { get; set; }
        public string ReviewRemark         { get; set; }

        // Transient
        public string OrgUnitName          { get; set; }
        public string RequestedByName      { get; set; }
        public string ReviewedByName       { get; set; }

        public double CurrentQuotaGB
        {
            get { return CurrentQuotaBytes / 1073741824.0; }
        }

        public double RequestedQuotaGB
        {
            get { return RequestedQuotaBytes / 1073741824.0; }
        }

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case QuotaRequestStatus.Pending:   return "รอดำเนินการ";
                    case QuotaRequestStatus.Approved:  return "อนุมัติ";
                    case QuotaRequestStatus.Rejected:  return "ปฏิเสธ";
                    case QuotaRequestStatus.Cancelled: return "ยกเลิก";
                    default:                           return "-";
                }
            }
        }
    }
}
