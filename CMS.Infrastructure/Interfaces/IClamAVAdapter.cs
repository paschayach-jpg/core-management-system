using System;
using System.IO;

namespace CMS.Infrastructure.Interfaces
{
    /// <summary>
    /// Abstraction over the ClamAV daemon for virus scanning (Requirements 11.1–11.5).
    /// </summary>
    public interface IClamAVAdapter
    {
        /// <summary>
        /// Scans the given stream for malware.
        /// </summary>
        /// <param name="stream">File content to scan.</param>
        /// <param name="timeout">Maximum time to wait for a scan result.</param>
        /// <returns>A <see cref="ClamAVResult"/> describing the outcome.</returns>
        ClamAVResult Scan(Stream stream, TimeSpan timeout);
    }
}
