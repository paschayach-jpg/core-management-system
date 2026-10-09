-- ============================================================================
-- Script: share_inbox_read.sql
-- Description: ตารางเก็บสถานะอ่านแล้วของรายการแชร์ (Share Inbox Read Status)
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'security')
BEGIN
    EXEC('CREATE SCHEMA security AUTHORIZATION dbo;');
END
GO

IF OBJECT_ID('security.ShareInboxRead', 'U') IS NULL
BEGIN
    CREATE TABLE security.ShareInboxRead (
        UserID       INT      NOT NULL,
        ShareKind    TINYINT  NOT NULL,   -- 1 = FolderPermission, 2 = FilePermission
        PermissionID INT      NOT NULL,   -- FolderPermission.PermissionID / FilePermission.FilePermissionID
        ReadDate     DATETIME NOT NULL CONSTRAINT DF_ShareInboxRead_ReadDate DEFAULT GETDATE(),
        CONSTRAINT PK_ShareInboxRead PRIMARY KEY CLUSTERED (UserID, ShareKind, PermissionID)
    );

    CREATE NONCLUSTERED INDEX IX_ShareInboxRead_User 
    ON security.ShareInboxRead (UserID, ShareKind, PermissionID);

    -- Baseline: ใส่แถว "อ่านแล้ว" ให้ permission (PrincipalType=1: User) ที่ GrantedDate < DATEADD(day, -7, GETDATE())
    -- เพื่อไม่ให้ user เจอตัวเลขค้างเก่าเป็นร้อยตอนเปิดใช้งานครั้งแรก
    
    -- 1. FolderPermission เก่า (> 7 วัน)
    IF OBJECT_ID('security.FolderPermission', 'U') IS NOT NULL
    BEGIN
        INSERT INTO security.ShareInboxRead (UserID, ShareKind, PermissionID, ReadDate)
        SELECT DISTINCT 
            fp.PrincipalID AS UserID,
            1 AS ShareKind,
            fp.PermissionID,
            GETDATE() AS ReadDate
        FROM security.FolderPermission fp
        WHERE fp.PrincipalType = 1 -- User
          AND fp.GrantedDate IS NOT NULL
          AND fp.GrantedDate < DATEADD(day, -7, GETDATE())
          AND NOT EXISTS (
              SELECT 1 FROM security.ShareInboxRead r
              WHERE r.UserID = fp.PrincipalID AND r.ShareKind = 1 AND r.PermissionID = fp.PermissionID
          );
    END

    -- 2. FilePermission เก่า (> 7 วัน)
    IF OBJECT_ID('security.FilePermission', 'U') IS NOT NULL
    BEGIN
        INSERT INTO security.ShareInboxRead (UserID, ShareKind, PermissionID, ReadDate)
        SELECT DISTINCT 
            fp.PrincipalID AS UserID,
            2 AS ShareKind,
            fp.FilePermissionID AS PermissionID,
            GETDATE() AS ReadDate
        FROM security.FilePermission fp
        WHERE fp.PrincipalType = 1 -- User
          AND fp.GrantedDate IS NOT NULL
          AND fp.GrantedDate < DATEADD(day, -7, GETDATE())
          AND NOT EXISTS (
              SELECT 1 FROM security.ShareInboxRead r
              WHERE r.UserID = fp.PrincipalID AND r.ShareKind = 2 AND r.PermissionID = fp.FilePermissionID
          );
    END
END
GO
