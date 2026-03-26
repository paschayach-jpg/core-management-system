namespace CMS.Infrastructure
{
    /// <summary>
    /// Scan result returned by <see cref="ClamAVAdapter"/>.
    /// </summary>
    public sealed class ClamAVResult
    {
        /// <summary>True when the file contains no known threat.</summary>
        public bool IsClean { get; set; }

        /// <summary>Threat name reported by ClamAV, or null when clean.</summary>
        public string ThreatName { get; set; }

        /// <summary>True when the scan could not be completed due to a connection or timeout error.</summary>
        public bool ScannerError { get; set; }
    }
}
