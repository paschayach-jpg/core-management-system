using System;

namespace CMS.Services
{
    /// <summary>
    /// Thrown by <see cref="KBExcelReader"/> when the Excel file cannot be opened
    /// or is not a valid .xlsx file.
    /// </summary>
    public class KBExcelReadException : Exception
    {
        /// <summary>The file path that could not be read.</summary>
        public string FilePath { get; }

        /// <summary>
        /// Initialises a new <see cref="KBExcelReadException"/>.
        /// </summary>
        /// <param name="filePath">Path of the file that caused the error.</param>
        /// <param name="reason">Human-readable description of the failure.</param>
        /// <param name="inner">Optional inner exception.</param>
        public KBExcelReadException(string filePath, string reason, Exception inner = null)
            : base(string.Format("ไม่สามารถอ่านไฟล์ '{0}': {1}", filePath, reason), inner)
        {
            FilePath = filePath;
        }
    }
}
