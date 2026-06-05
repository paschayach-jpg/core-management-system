using System;
using System.Threading.Tasks;

namespace CMS.Services.Workflow.Engine
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message) : base(message) { }
    }

    public interface IConcurrencyGuard
    {
        // BR-008-10 / NFR-008-05: retry OCC สูงสุด 3 ครั้ง
        // operation คืน true=success, false=concurrency conflict → retry
        Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation);
    }

    // BR-008-10: OCC retry สูงสุด 3 ครั้ง, ห้าม silent override
    public class ConcurrencyGuard : IConcurrencyGuard
    {
        private const int MaxRetries = 3;

        public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
        {
            var lastException = default(Exception);

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (ConcurrencyException ex)
                {
                    lastException = ex;

                    if (attempt < MaxRetries)
                    {
                        // short back-off ก่อน retry
                        await Task.Delay(100 * attempt);
                        continue;
                    }
                }
            }

            throw new ConcurrencyException(
                string.Format("ข้อมูลถูกแก้ไขโดย process อื่น กรุณาลองใหม่ (WF-E007, attempts={0})", MaxRetries));
        }
    }
}
