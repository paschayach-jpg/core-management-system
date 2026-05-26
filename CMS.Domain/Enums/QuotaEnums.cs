namespace CMS.Domain.Enums
{
    public enum QuotaRequestStatus
    {
        Pending   = 1,
        Approved  = 2,
        Rejected  = 3,
        Cancelled = 4
    }

    public enum QuotaAlertLevel
    {
        Normal   = 0,
        Warning  = 1,
        Critical = 2,
        Full     = 3
    }
}
