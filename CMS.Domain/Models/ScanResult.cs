namespace CMS.Domain.Models
{
    /// <summary>
    /// The result returned by the VirusScanService after scanning a file stream.
    /// </summary>
    public class ScanResult
    {
        public bool IsClean { get; set; }

        /// <summary>Threat name reported by ClamAV; null when IsClean is true.</summary>
        public string ThreatName { get; set; }

        /// <summary>True when the scanner itself failed or timed out.</summary>
        public bool ScannerError { get; set; }
    }
}
