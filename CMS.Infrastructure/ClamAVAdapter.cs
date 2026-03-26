using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CMS.Infrastructure.Interfaces;

namespace CMS.Infrastructure
{
    /// <summary>
    /// Communicates with a ClamAV daemon over TCP using the INSTREAM protocol.
    /// Requirements 11.1–11.5.
    /// </summary>
    public class ClamAVAdapter : IClamAVAdapter
    {
        private const int ChunkSize = 4096;
        private const string OkResponse = "stream: OK";
        private const string FoundSuffix = " FOUND";

        private readonly string _host;
        private readonly int _port;

        /// <param name="config">
        /// Reads "ClamAV.Host" (default "localhost") and "ClamAV.Port" (default 3310).
        /// </param>
        public ClamAVAdapter(IConfigManager config)
        {
            if (config == null) throw new ArgumentNullException("config");

            try { _host = config.Get<string>("ClamAV.Host"); }
            catch { _host = "localhost"; }

            try { _port = config.Get<int>("ClamAV.Port"); }
            catch { _port = 3310; }
        }

        /// <summary>
        /// Scans <paramref name="stream"/> via the ClamAV INSTREAM protocol.
        /// If the scan does not complete within <paramref name="timeout"/>, returns
        /// <see cref="ClamAVResult.ScannerError"/> = true (Requirement 11.5).
        /// </summary>
        public ClamAVResult Scan(Stream stream, TimeSpan timeout)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            ClamAVResult result = null;
            Exception scanException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    result = PerformScan(stream);
                }
                catch (Exception ex)
                {
                    scanException = ex;
                }
            });

            thread.IsBackground = true;
            thread.Start();

            bool completed = thread.Join((int)timeout.TotalMilliseconds);

            if (!completed)
            {
                // Abort the blocking thread and treat as scanner error (Req 11.5)
                thread.Abort();
                return new ClamAVResult { ScannerError = true };
            }

            if (scanException != null)
                return new ClamAVResult { ScannerError = true };

            return result;
        }

        // -----------------------------------------------------------------------
        // Internals
        // -----------------------------------------------------------------------

        private ClamAVResult PerformScan(Stream stream)
        {
            using (var client = new TcpClient())
            {
                client.Connect(_host, _port);

                using (var netStream = client.GetStream())
                {
                    // 1. Send "zINSTREAM\0"
                    byte[] command = Encoding.ASCII.GetBytes("zINSTREAM\0");
                    netStream.Write(command, 0, command.Length);

                    // 2. Send file data in 4-byte big-endian length-prefixed chunks
                    byte[] buffer = new byte[ChunkSize];
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        byte[] lengthPrefix = ToBigEndianBytes(bytesRead);
                        netStream.Write(lengthPrefix, 0, 4);
                        netStream.Write(buffer, 0, bytesRead);
                    }

                    // 3. Send 4-byte zero to signal end of stream
                    netStream.Write(new byte[4], 0, 4);
                    netStream.Flush();

                    // 4. Read response
                    string response = ReadResponse(netStream);
                    return ParseResponse(response);
                }
            }
        }

        private static string ReadResponse(NetworkStream netStream)
        {
            var sb = new StringBuilder();
            byte[] buf = new byte[256];
            int read;
            while ((read = netStream.Read(buf, 0, buf.Length)) > 0)
            {
                sb.Append(Encoding.ASCII.GetString(buf, 0, read));
                // ClamAV terminates its response with a null byte
                if (buf[read - 1] == 0)
                    break;
            }
            return sb.ToString().TrimEnd('\0').Trim();
        }

        private static ClamAVResult ParseResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return new ClamAVResult { ScannerError = true };

            if (string.Equals(response, OkResponse, StringComparison.OrdinalIgnoreCase))
                return new ClamAVResult { IsClean = true };

            if (response.EndsWith(FoundSuffix, StringComparison.OrdinalIgnoreCase))
            {
                // Format: "stream: <ThreatName> FOUND"
                string threatName = response
                    .Substring("stream: ".Length,
                               response.Length - "stream: ".Length - FoundSuffix.Length)
                    .Trim();
                return new ClamAVResult { IsClean = false, ThreatName = threatName };
            }

            return new ClamAVResult { ScannerError = true };
        }

        private static byte[] ToBigEndianBytes(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }
    }
}
