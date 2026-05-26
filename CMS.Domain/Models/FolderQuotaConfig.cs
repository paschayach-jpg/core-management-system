using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// กำหนด quota พื้นที่ folder ต่อ OrgUnit (Sub/Div/Dep)
    /// กรณี Div อยู่หลาย Sub ใช้ ParentOrgUnitID แยก config
    /// Maps to dms.FolderQuotaConfig
    /// </summary>
    public class FolderQuotaConfig
    {
        public int QuotaConfigID      { get; set; }

        /// <summary>OrgUnit ที่กำหนด quota (Sub=1 / Div=2 / Dep=3)</summary>
        public int OrgUnitID          { get; set; }

        /// <summary>Sub ที่ Div นี้สังกัด — null = ใช้ config ทั่วไป, not-null = เฉพาะ Sub นั้น</summary>
        public int? ParentOrgUnitID   { get; set; }

        /// <summary>ขนาด quota เป็น bytes (40 GB = 42_949_672_960)</summary>
        public long QuotaBytes        { get; set; }

        /// <summary>% ที่จะแจ้งเตือน Warning (default 80)</summary>
        public byte WarnThresholdPct  { get; set; }

        /// <summary>% ที่จะแจ้งเตือน Critical (default 95)</summary>
        public byte CriticalThresholdPct { get; set; }

        public bool IsActive          { get; set; }
        public int  CreatedBy         { get; set; }
        public DateTime CreatedDate   { get; set; }
        public int? UpdatedBy         { get; set; }
        public DateTime? UpdatedDate  { get; set; }

        // Transient — populate by service/presentation
        public string OrgUnitName     { get; set; }
        public string OrgUnitTypeName { get; set; }

        /// <summary>Quota เป็น GB สำหรับแสดง UI</summary>
        public double QuotaGB
        {
            get { return QuotaBytes / 1073741824.0; }
        }
    }
}
