using System.IO;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Business logic service that scans file streams for malware using the ClamAV adapter.
    /// Satisfies Requirements 11.1–11.6.
    /// </summary>
    public interface IVirusScanService
    {
        /// <summary>
        /// Scans the given file stream for malware before it is written to the network drive.
        /// Returns a <see cref="ScanResult"/> indicating clean, threat, or scanner error.
        /// </summary>
        /// <param name="fileStream">The file content to scan.</param>
        /// <param name="fileName">The original file name (used for audit/logging context).</param>
        ScanResult Scan(Stream fileStream, string fileName);
    }
}
