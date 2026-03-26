using System;
using System.Diagnostics;
using CMS.Infrastructure.Interfaces;
using log4net;

namespace CMS.Infrastructure
{
    /// <summary>
    /// Wraps log4net for application logging.
    /// If a log4net write fails, falls back to the Windows Event Log (Requirement 16.5).
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LoggingService));
        private const string EventLogSource = "CMS";
        private const string EventLogName   = "Application";

        public void Debug(string message)
        {
            TryLog(() => Log.Debug(message), message, EventLogEntryType.Information);
        }

        public void Info(string message)
        {
            TryLog(() => Log.Info(message), message, EventLogEntryType.Information);
        }

        public void Warn(string message)
        {
            TryLog(() => Log.Warn(message), message, EventLogEntryType.Warning);
        }

        public void Error(string message, Exception ex = null)
        {
            TryLog(
                () => { if (ex != null) Log.Error(message, ex); else Log.Error(message); },
                ex != null ? string.Format("{0}\r\n{1}", message, ex) : message,
                EventLogEntryType.Error);
        }

        public void Fatal(string message, Exception ex = null)
        {
            TryLog(
                () => { if (ex != null) Log.Fatal(message, ex); else Log.Fatal(message); },
                ex != null ? string.Format("{0}\r\n{1}", message, ex) : message,
                EventLogEntryType.Error);
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private void TryLog(Action log4netWrite, string fallbackMessage, EventLogEntryType entryType)
        {
            try
            {
                log4netWrite();
            }
            catch
            {
                // log4net failed — write to Windows Event Log instead (Req 16.5)
                WriteToEventLog(fallbackMessage, entryType);
            }
        }

        private static void WriteToEventLog(string message, EventLogEntryType entryType)
        {
            try
            {
                if (!EventLog.SourceExists(EventLogSource))
                    EventLog.CreateEventSource(EventLogSource, EventLogName);

                EventLog.WriteEntry(EventLogSource, message, entryType);
            }
            catch
            {
                // Swallow — we must never throw from the logging path (Req 16.5)
            }
        }
    }
}
