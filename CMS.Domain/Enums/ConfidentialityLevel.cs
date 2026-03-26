namespace CMS.Domain.Enums
{
    /// <summary>
    /// Data classification levels per CRAF requirements.
    /// HC=Highly Confidential, SC=Secret, NC=Non-Confidential, PC=Public/Common
    /// </summary>
    public enum ConfidentialityLevel
    {
        HC = 1,
        SC = 2,
        NC = 3,
        PC = 4
    }
}
