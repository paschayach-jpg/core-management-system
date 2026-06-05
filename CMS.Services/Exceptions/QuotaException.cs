using System;

namespace CMS.Services.Exceptions
{
    public class QuotaExceededException : Exception
    {
        public int OrgUnitID { get; }

        public QuotaExceededException(int orgUnitId, string message)
            : base(message)
        {
            OrgUnitID = orgUnitId;
        }
    }
}
