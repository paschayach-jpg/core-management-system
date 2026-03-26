using System;
using System.IO;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class VirusScanService : IVirusScanService
    {
        private const int    DefaultScanTimeoutSeconds = 30;
        private const string ScanTimeoutConfigKey      = "VirusScan.TimeoutSeconds";

        private readonly IClamAVAdapter          _clamAV;
        private readonly ILoggingService         _loggingService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserAccountRepository  _userAccountRepository;
        private readonly IAuditService           _auditService;
        private readonly IConfigManager          _configManager;

        public VirusScanService(
            IClamAVAdapter          clamAV,
            ILoggingService         loggingService,
            INotificationRepository notificationRepository,
            IUserAccountRepository  userAccountRepository,
            IAuditService           auditService,
            IConfigManager          configManager)
        {
            if (clamAV                 == null) throw new ArgumentNullException("clamAV");
            if (loggingService         == null) throw new ArgumentNullException("loggingService");
            if (notificationRepository == null) throw new ArgumentNullException("notificationRepository");
            if (userAccountRepository  == null) throw new ArgumentNullException("userAccountRepository");
            if (auditService           == null) throw new ArgumentNullException("auditService");
            if (configManager          == null) throw new ArgumentNullException("configManager");

            _clamAV                 = clamAV;
            _loggingService         = loggingService;
            _notificationRepository = notificationRepository;
            _userAccountRepository  = userAccountRepository;
            _auditService           = auditService;
            _configManager          = configManager;
        }

        public ScanResult Scan(Stream fileStream, string fileName)
        {
            if (fileStream == null) throw new ArgumentNullException("fileStream");
            if (fileName   == null) throw new ArgumentNullException("fileName");

            TimeSpan timeout = GetScanTimeout();
            ClamAVResult clamResult;

            try
            {
                clamResult = _clamAV.Scan(fileStream, timeout);
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "VirusScanService: ClamAV adapter threw an exception scanning '{0}'.", fileName), ex);
                HandleScannerUnavailable(fileName);
                return new ScanResult { ScannerError = true };
            }

            if (clamResult == null)
            {
                _loggingService.Error(string.Format(
                    "VirusScanService: ClamAV returned null result for '{0}'.", fileName));
                HandleScannerUnavailable(fileName);
                return new ScanResult { ScannerError = true };
            }

            if (clamResult.ScannerError)
            {
                HandleScannerUnavailable(fileName);
                RecordAudit(fileName, "ScanError", 2, "ClamAV scanner unavailable or timed out.");
                return new ScanResult { ScannerError = true };
            }

            if (!clamResult.IsClean)
            {
                RecordAudit(fileName, "VirusThreat", 2,
                    string.Format("Threat detected: {0}", clamResult.ThreatName));
                return new ScanResult { IsClean = false, ThreatName = clamResult.ThreatName };
            }

            RecordAudit(fileName, "VirusScanClean", 1, "File is clean.");
            return new ScanResult { IsClean = true };
        }

        private void HandleScannerUnavailable(string fileName)
        {
            _loggingService.Error(string.Format(
                "VirusScanService: ClamAV is unavailable while scanning '{0}'. Upload rejected.", fileName));

            try
            {
                // Admin=4
                var admins = _userAccountRepository.GetByRole(4);
                foreach (var admin in admins)
                {
                    _notificationRepository.Insert(new Notification
                    {
                        RecipientUserId = admin.UserID,
                        EventType       = "ScannerUnavailable",
                        RelatedEntityId = null,
                        MessageText     = string.Format(
                            "Virus scanner (ClamAV) is unavailable. Upload of file '{0}' was rejected.", fileName),
                        DeliveredAt = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error("VirusScanService: Failed to send scanner-unavailable notification.", ex);
            }
        }

        private void RecordAudit(string fileName, string action, int result, string details)
        {
            try
            {
                _auditService.Log(new AuditEntry
                {
                    Module      = "File",
                    Action      = action,
                    UserID      = 0,
                    TableName   = "storage.FileDocument",
                    NewValue    = details,
                    MachineName = Environment.MachineName,
                    Result      = result,
                    AuditDate   = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _loggingService.Error(string.Format(
                    "VirusScanService: Failed to write audit entry for scan of '{0}'.", fileName), ex);
            }
        }

        private TimeSpan GetScanTimeout()
        {
            try
            {
                int seconds = _configManager.Get<int>(ScanTimeoutConfigKey);
                return TimeSpan.FromSeconds(seconds > 0 ? seconds : DefaultScanTimeoutSeconds);
            }
            catch
            {
                return TimeSpan.FromSeconds(DefaultScanTimeoutSeconds);
            }
        }
    }
}
