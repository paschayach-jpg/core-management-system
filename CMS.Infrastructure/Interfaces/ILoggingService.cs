using System;

namespace CMS.Infrastructure.Interfaces
{
    /// <summary>
    /// Application logging abstraction wrapping log4net.
    /// Falls back to Windows Event Log if log4net write fails (Requirement 16.5).
    /// </summary>
    public interface ILoggingService
    {
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message, Exception ex = null);
        void Fatal(string message, Exception ex = null);
    }
}
