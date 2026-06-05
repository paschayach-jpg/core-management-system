using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Exceptions;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class FolderQuotaService : IFolderQuotaService
    {
        private readonly IFolderQuotaConfigRepository       _configRepo;
        private readonly IFolderQuotaChangeRequestRepository _requestRepo;
        private readonly IFolderRepository                  _folderRepo;
        private readonly INotificationService               _notificationService;
        private readonly IOrgUnitRepository                 _orgUnitRepo;
        private readonly IFileDocumentRepository            _fileDocRepo;

        public FolderQuotaService(
            IFolderQuotaConfigRepository        configRepo,
            IFolderQuotaChangeRequestRepository requestRepo,
            IFolderRepository                   folderRepo,
            INotificationService                notificationService,
            IOrgUnitRepository                  orgUnitRepo,
            IFileDocumentRepository             fileDocRepo = null)
        {
            _configRepo          = configRepo;
            _requestRepo         = requestRepo;
            _folderRepo          = folderRepo;
            _notificationService = notificationService;
            _orgUnitRepo         = orgUnitRepo;
            _fileDocRepo         = fileDocRepo;
        }

        // ---------------------------------------------------------------
        // Usage
        // ---------------------------------------------------------------

        public QuotaUsageResult GetUsage(int orgUnitId, int? parentOrgUnitId = null)
        {
            var folders = _folderRepo.GetByOrgUnit(orgUnitId).Where(f => f != null && !f.IsDeleted).ToList();
            long usedBytes = CalculateQuotaBucketUsage(folders);

            var orgUnit = _orgUnitRepo.GetById(orgUnitId);
            var config = ResolveConfigForSelectedOrgUnit(orgUnit, orgUnitId, parentOrgUnitId);
            long quotaBytes = config != null ? config.QuotaBytes : 0;
            double pct = quotaBytes > 0 ? (double)usedBytes / quotaBytes * 100.0 : 0;

            return new QuotaUsageResult
            {
                OrgUnitID   = orgUnitId,
                OrgUnitName = orgUnit != null ? orgUnit.Name : orgUnitId.ToString(),
                QuotaBytes  = quotaBytes,
                UsedBytes   = usedBytes,
                UsedPercent = Math.Round(pct, 2),
                Alert       = EvaluateAlert(pct, config)
            };
        }

        public Dictionary<int, OrgUnitFileStat> GetAllFileStats()
        {
            return _configRepo.GetAllFileStats();
        }

        public IEnumerable<FileDocument> GetFilesByOrgUnit(int orgUnitId)
        {
            if (_fileDocRepo == null)
                return new List<FileDocument>();
            return _fileDocRepo.GetByOrgUnit(orgUnitId)
                .Where(f => f != null && f.FileStatus == 1)
                .OrderByDescending(f => f.UploadedDate)
                .ToList();
        }

        public IList<FileTypeUsage> GetFileTypeStats(int orgUnitId)
        {
            if (_fileDocRepo == null)
                return new List<FileTypeUsage>();

            var files = _fileDocRepo.GetByOrgUnit(orgUnitId)
                .Where(f => f != null && f.FileStatus == 1)
                .ToList();

            if (files.Count == 0)
                return new List<FileTypeUsage>();

            long grandTotal = files.Sum(f => f.FileSizeBytes);

            var grouped = files
                .GroupBy(f => string.IsNullOrWhiteSpace(f.FileExtension)
                    ? "อื่นๆ"
                    : f.FileExtension.TrimStart('.').ToUpperInvariant())
                .Select(g => new FileTypeUsage
                {
                    Extension  = g.Key,
                    FileCount  = g.Count(),
                    TotalBytes = g.Sum(f => f.FileSizeBytes),
                    Percent    = grandTotal > 0 ? Math.Round(g.Sum(f => f.FileSizeBytes) * 100.0 / grandTotal, 1) : 0
                })
                .OrderByDescending(x => x.TotalBytes)
                .ToList();

            return grouped;
        }

        public QuotaAlertLevel CheckAlert(int orgUnitId, int? parentOrgUnitId = null)
        {
            return GetUsage(orgUnitId, parentOrgUnitId).Alert;
        }

        public void EnforceQuota(int orgUnitId, int? parentOrgUnitId = null)
        {
            var usage = GetUsage(orgUnitId, parentOrgUnitId);
            if (usage.Alert == QuotaAlertLevel.Full)
                throw new QuotaExceededException(orgUnitId,
                    string.Format("พื้นที่ folder เต็ม ({0:F1} GB จาก {1:F1} GB) กรุณาขออนุมัติเพิ่ม quota",
                        usage.UsedGB, usage.QuotaGB));
        }

        public void SendAlertIfNeeded(int orgUnitId, int? parentOrgUnitId = null)
        {
            try
            {
                var usage = GetUsage(orgUnitId, parentOrgUnitId);
                if (usage.Alert == QuotaAlertLevel.Normal) return;

                // หา admin ของ OrgUnit นี้เพื่อแจ้งเตือน — ใช้ OrgUnitID เป็น RelatedEntityId
                string message = BuildAlertMessage(usage);
                _notificationService.Send(new NotificationRequest
                {
                    RecipientUserId = 0,   // 0 = broadcast to OrgUnit admins (resolved by NotificationService)
                    EventType       = "QUOTA_ALERT",
                    RelatedEntityId = orgUnitId.ToString(),
                    MessageText     = message
                });
            }
            catch
            {
                // fire-and-forget: ไม่ block flow หลักถ้า notification ล้มเหลว
            }
        }

        // ---------------------------------------------------------------
        // Config
        // ---------------------------------------------------------------

        public IEnumerable<FolderQuotaConfig> GetAllConfigs()
        {
            return _configRepo.GetAll();
        }

        public FolderQuotaConfig GetConfig(int orgUnitId, int? parentOrgUnitId = null)
        {
            var orgUnit = _orgUnitRepo.GetById(orgUnitId);
            return ResolveConfigForSelectedOrgUnit(orgUnit, orgUnitId, parentOrgUnitId);
        }

        public void SaveConfig(FolderQuotaConfig config, UserContext user)
        {
            if (config.QuotaBytes <= 0)
                throw new ArgumentException("QuotaBytes ต้องมากกว่า 0");
            if (config.WarnThresholdPct >= 100)
                throw new ArgumentException("WarnThresholdPct ต้องน้อยกว่า 100");
            if (config.CriticalThresholdPct <= config.WarnThresholdPct)
                throw new ArgumentException("CriticalThresholdPct ต้องมากกว่า WarnThresholdPct");

            config.CreatedBy  = user.UserID;
            config.UpdatedBy  = user.UserID;
            _configRepo.Save(config);
        }

        public void DeleteConfig(int quotaConfigId, string orgUnitTypeName, UserContext user)
        {
            _configRepo.Delete(quotaConfigId, orgUnitTypeName);
        }

        // ---------------------------------------------------------------
        // Change Request
        // ---------------------------------------------------------------

        public int SubmitChangeRequest(SubmitQuotaChangeRequest dto, UserContext user)
        {
            var existing = _configRepo.GetByOrgUnit(dto.OrgUnitID);
            long currentQuota = existing != null ? existing.QuotaBytes : 0;

            if (dto.RequestedQuotaBytes <= 0)
                throw new ArgumentException("ขนาด quota ที่ขอต้องมากกว่า 0");
            if (dto.RequestedQuotaBytes <= currentQuota)
                throw new ArgumentException("ขนาด quota ที่ขอต้องมากกว่า quota ปัจจุบัน");
            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new ArgumentException("กรุณาระบุเหตุผลในการขอเพิ่ม quota");

            var request = new FolderQuotaChangeRequest
            {
                OrgUnitID           = dto.OrgUnitID,
                CurrentQuotaBytes   = currentQuota,
                RequestedQuotaBytes = dto.RequestedQuotaBytes,
                Reason              = dto.Reason.Trim(),
                Status              = QuotaRequestStatus.Pending,
                RequestedBy         = user.UserID,
                RequestedDate       = DateTime.Now
            };
            return _requestRepo.Insert(request);
        }

        public IEnumerable<FolderQuotaChangeRequest> GetPendingRequests()
        {
            return _requestRepo.GetPending();
        }

        public IEnumerable<FolderQuotaChangeRequest> GetAllRequests()
        {
            return _requestRepo.GetAll();
        }

        public IEnumerable<FolderQuotaChangeRequest> GetRequestsByOrgUnit(int orgUnitId)
        {
            return _requestRepo.GetByOrgUnit(orgUnitId);
        }

        public void ApproveRequest(int requestId, string remark, UserContext user)
        {
            var req = _requestRepo.GetById(requestId);
            if (req == null) throw new ArgumentException("ไม่พบ Request ID " + requestId);
            if (req.Status != QuotaRequestStatus.Pending)
                throw new InvalidOperationException("Request นี้ไม่อยู่ในสถานะรอดำเนินการ");

            _requestRepo.UpdateStatus(requestId, QuotaRequestStatus.Approved, user.UserID, remark);

            // อัปเดต quota config ทันที
            // lookup by master code first (same as ResolveConfigForSelectedOrgUnit) so OrgUnitTypeName is always populated
            var orgUnit = _orgUnitRepo.GetById(req.OrgUnitID);
            FolderQuotaConfig config = null;
            if (orgUnit != null)
                config = _configRepo.GetByMasterCode(orgUnit.OrgUnitType, orgUnit.OrgUnitCode);
            if (config == null)
                config = _configRepo.GetByOrgUnit(req.OrgUnitID);
            if (config == null)
                config = new FolderQuotaConfig
                {
                    OrgUnitID            = req.OrgUnitID,
                    OrgUnitTypeName      = orgUnit != null ? OrgUnitTypeToName(orgUnit.OrgUnitType) : null,
                    WarnThresholdPct     = 80,
                    CriticalThresholdPct = 95
                };
            config.QuotaBytes = req.RequestedQuotaBytes;
            config.UpdatedBy  = user.UserID;
            config.CreatedBy  = config.CreatedBy == 0 ? user.UserID : config.CreatedBy;
            _configRepo.Save(config);

            // แจ้งผู้ขอ
            _notificationService.Send(new NotificationRequest
            {
                RecipientUserId = req.RequestedBy,
                EventType       = "QUOTA_REQUEST_APPROVED",
                RelatedEntityId = requestId.ToString(),
                MessageText     = string.Format("คำขอเพิ่ม quota พื้นที่ folder ของ {0} ได้รับการอนุมัติแล้ว (ใหม่: {1:F1} GB)",
                    req.OrgUnitName, req.RequestedQuotaBytes / 1073741824.0)
            });
        }

        public void RejectRequest(int requestId, string remark, UserContext user)
        {
            var req = _requestRepo.GetById(requestId);
            if (req == null) throw new ArgumentException("ไม่พบ Request ID " + requestId);
            if (req.Status != QuotaRequestStatus.Pending)
                throw new InvalidOperationException("Request นี้ไม่อยู่ในสถานะรอดำเนินการ");

            if (string.IsNullOrWhiteSpace(remark))
                throw new ArgumentException("กรุณาระบุเหตุผลในการปฏิเสธ");

            _requestRepo.UpdateStatus(requestId, QuotaRequestStatus.Rejected, user.UserID, remark);

            _notificationService.Send(new NotificationRequest
            {
                RecipientUserId = req.RequestedBy,
                EventType       = "QUOTA_REQUEST_REJECTED",
                RelatedEntityId = requestId.ToString(),
                MessageText     = string.Format("คำขอเพิ่ม quota พื้นที่ folder ของ {0} ถูกปฏิเสธ: {1}",
                    req.OrgUnitName, remark)
            });
        }

        public void CancelRequest(int requestId, UserContext user)
        {
            var req = _requestRepo.GetById(requestId);
            if (req == null) throw new ArgumentException("ไม่พบ Request ID " + requestId);
            if (req.RequestedBy != user.UserID)
                throw new UnauthorizedAccessException("ยกเลิกได้เฉพาะคำขอของตนเองเท่านั้น");
            if (req.Status != QuotaRequestStatus.Pending)
                throw new InvalidOperationException("ยกเลิกได้เฉพาะ Request ที่ยังรออนุมัติ");

            _requestRepo.UpdateStatus(requestId, QuotaRequestStatus.Cancelled, user.UserID, "ยกเลิกโดยผู้ขอ");
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static string OrgUnitTypeToName(int orgUnitType)
        {
            switch (orgUnitType)
            {
                case 1: return "Sub Committee";
                case 2: return "Division";
                case 3: return "Department";
                default: return null;
            }
        }

        private static QuotaAlertLevel EvaluateAlert(double usedPct, FolderQuotaConfig config)
        {
            if (config == null) return QuotaAlertLevel.Normal;
            if (usedPct >= 100)                       return QuotaAlertLevel.Full;
            if (usedPct >= config.CriticalThresholdPct) return QuotaAlertLevel.Critical;
            if (usedPct >= config.WarnThresholdPct)     return QuotaAlertLevel.Warning;
            return QuotaAlertLevel.Normal;
        }

        private FolderQuotaConfig ResolveConfigForSelectedOrgUnit(OrgUnit orgUnit, int orgUnitId, int? parentOrgUnitId)
        {
            if (orgUnit == null)
                return _configRepo.GetByOrgUnit(orgUnitId, parentOrgUnitId);

            if (orgUnit.OrgUnitType >= 1 && orgUnit.OrgUnitType <= 3)
            {
                // Primary: JOIN EMS.OrgUnit กับ master table โดยตรง (ไม่พึ่ง ID matching)
                var config = _configRepo.GetByEmsOrgUnitId(orgUnit.OrgUnitID, orgUnit.OrgUnitType);
                if (config != null && config.QuotaBytes > 0)
                    return config;

                // Fallback 1: lookup by code (ใช้เมื่อ OrgUnitCode ตรงกับ master code)
                if (!string.IsNullOrWhiteSpace(orgUnit.OrgUnitCode))
                {
                    config = _configRepo.GetByMasterCode(orgUnit.OrgUnitType, orgUnit.OrgUnitCode);
                    if (config != null && config.QuotaBytes > 0)
                        return config;
                }

                // Fallback 2: lookup by OrgUnitID ตรง (ใช้งานได้กรณี SubCommittee)
                config = _configRepo.GetByOrgUnit(orgUnitId, parentOrgUnitId);
                if (config != null && config.QuotaBytes > 0)
                    return config;

                return null;
            }

            return _configRepo.GetByOrgUnit(orgUnitId, parentOrgUnitId);
        }

        private static long CalculateQuotaBucketUsage(IList<Folder> folders)
        {
            if (folders == null || folders.Count == 0)
                return 0;

            var childrenByParent = folders
                .Where(f => f.ParentFolderID.HasValue)
                .GroupBy(f => f.ParentFolderID.Value)
                .ToDictionary(g => g.Key, g => g.ToList());
            var visitedRoots = new HashSet<int>();
            long total = 0;

            foreach (var root in folders.Where(IsQuotaBucketFolder))
            {
                if (!visitedRoots.Add(root.FolderID))
                    continue;
                total += Math.Max(0, root.FolderSizeBytes) + SumDescendantSize(root.FolderID, childrenByParent, new HashSet<int>());
            }

            return total;
        }

        private static bool IsQuotaBucketFolder(Folder folder)
        {
            if (folder == null || folder.IsDeleted || folder.IsArchived)
                return false;
            if (folder.ConfidentialityLevel < 1 || folder.ConfidentialityLevel > 4)
                return false;

            var name = (folder.FolderName ?? folder.Name ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name))
                return false;

            return name.StartsWith(LevelCode(folder.ConfidentialityLevel), StringComparison.OrdinalIgnoreCase);
        }

        private static long SumDescendantSize(int folderId, Dictionary<int, List<Folder>> childrenByParent, HashSet<int> visited)
        {
            if (!visited.Add(folderId))
                return 0;

            long total = 0;
            List<Folder> children;
            if (childrenByParent.TryGetValue(folderId, out children))
            {
                foreach (var child in children.Where(c => c != null && !c.IsDeleted && !c.IsArchived))
                    total += Math.Max(0, child.FolderSizeBytes) + SumDescendantSize(child.FolderID, childrenByParent, visited);
            }

            visited.Remove(folderId);
            return total;
        }

        private static string LevelCode(int level)
        {
            switch (level)
            {
                case 1: return "1HC";
                case 2: return "2SC";
                case 3: return "3NC";
                case 4: return "4PC";
                default: return level.ToString();
            }
        }

        private static string BuildAlertMessage(QuotaUsageResult usage)
        {
            switch (usage.Alert)
            {
                case QuotaAlertLevel.Full:
                    return string.Format("[QUOTA FULL] {0}: ใช้ {1:F1} GB / {2:F1} GB (100%)",
                        usage.OrgUnitName, usage.UsedGB, usage.QuotaGB);
                case QuotaAlertLevel.Critical:
                    return string.Format("[QUOTA วิกฤต] {0}: ใช้ {1:F1} GB / {2:F1} GB ({3:F1}%)",
                        usage.OrgUnitName, usage.UsedGB, usage.QuotaGB, usage.UsedPercent);
                default:
                    return string.Format("[QUOTA เตือน] {0}: ใช้ {1:F1} GB / {2:F1} GB ({3:F1}%)",
                        usage.OrgUnitName, usage.UsedGB, usage.QuotaGB, usage.UsedPercent);
            }
        }
    }
}
