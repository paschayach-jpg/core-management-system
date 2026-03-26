using System;
using System.Linq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class FilePermissionService : IFilePermissionService
    {
        private readonly IFilePermissionRepository _filePermissionRepository;
        private readonly IAuditService             _auditService;

        public FilePermissionService(
            IFilePermissionRepository filePermissionRepository,
            IAuditService             auditService = null)
        {
            if (filePermissionRepository == null) throw new ArgumentNullException("filePermissionRepository");
            _filePermissionRepository = filePermissionRepository;
            _auditService             = auditService;
        }

        public void SetFilePermission(SetFilePermissionRequest request, UserContext user)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (user    == null) throw new ArgumentNullException("user");

            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("การตั้งค่าสิทธิ์ไฟล์ต้องการสิทธิ์ระดับ Manager หรือ Admin");

            // Revoke existing entries for same principal+file+isDeny to avoid duplicates
            var existing = _filePermissionRepository.GetByFile(request.FileID)
                .Where(e => e.PrincipalType == request.PrincipalType
                         && e.PrincipalID   == request.PrincipalID
                         && e.IsDeny        == request.IsDeny)
                .ToList();

            foreach (var old in existing)
                _filePermissionRepository.Revoke(old.FilePermissionID, user.UserID, "แทนที่ด้วยรายการสิทธิ์ใหม่");

            var entry = new FilePermission
            {
                FileID         = request.FileID,
                PrincipalType  = request.PrincipalType,
                PrincipalID    = request.PrincipalID,
                CanRead        = request.CanRead,
                CanWrite       = request.CanWrite,
                CanDelete      = request.CanDelete,
                CanModify      = request.CanModify,
                CanFullControl = request.CanFullControl,
                IsDeny         = request.IsDeny,
                GrantedBy      = user.UserID,
                GrantedDate    = DateTime.Now
            };

            _filePermissionRepository.Insert(entry);

            if (_auditService != null)
            {
                try
                {
                    _auditService.Log(new AuditEntry
                    {
                        Module      = "FilePermission",
                        Action      = request.IsDeny ? "DENY" : "GRANT",
                        UserID      = user.UserID,
                        TableName   = "security.FilePermission",
                        RecordID    = request.FileID,
                        NewValue    = string.Format("Principal {0} (type {1}) on file {2}.",
                                          request.PrincipalID, request.PrincipalType, request.FileID),
                        MachineName = user.MachineName,
                        IPAddress   = user.IPAddress,
                        SessionID   = user.SessionID,
                        Result      = 1,
                        AuditDate   = DateTime.Now
                    });
                }
                catch { }
            }
        }

        public void RevokeFilePermission(int filePermissionId, UserContext user, string reason = null)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("การยกเลิกสิทธิ์ไฟล์ต้องการสิทธิ์ระดับ Manager หรือ Admin");

            _filePermissionRepository.Revoke(filePermissionId, user.UserID, reason ?? "ยกเลิกโดยผู้ดูแลระบบ");
        }
    }
}
