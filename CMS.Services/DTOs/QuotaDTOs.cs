using CMS.Domain.Enums;

namespace CMS.Services.DTOs
{
    public class QuotaUsageResult
    {
        public int    OrgUnitID        { get; set; }
        public string OrgUnitName      { get; set; }
        public long   QuotaBytes       { get; set; }
        public long   UsedBytes        { get; set; }
        public double UsedPercent      { get; set; }
        public QuotaAlertLevel Alert   { get; set; }

        public double QuotaGB  { get { return QuotaBytes / 1073741824.0; } }
        public double UsedGB   { get { return UsedBytes  / 1073741824.0; } }
        public double FreeGB   { get { return (QuotaBytes - UsedBytes) / 1073741824.0; } }
    }

    public class SubmitQuotaChangeRequest
    {
        public int    OrgUnitID            { get; set; }
        public long   RequestedQuotaBytes  { get; set; }
        public string Reason               { get; set; }
    }
}
