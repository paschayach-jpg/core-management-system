-- ============================================================
-- 068_Fix_Notification_SPs.sql
-- สร้าง core.Notification table และ Stored Procedures
-- ที่จำเป็นสำหรับระบบ Notification
-- ใช้ IF NOT EXISTS / CREATE OR ALTER เพื่อ run ซ้ำได้ปลอดภัย
-- ============================================================

-- ============================================================
-- 0. สร้าง Schemas ที่จำเป็น (ถ้ายังไม่มี)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'core')
    EXEC('CREATE SCHEMA core');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'storage')
    EXEC('CREATE SCHEMA storage');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'security')
    EXEC('CREATE SCHEMA security');
GO

-- ============================================================
-- 1. สร้าง table core.Notification (ถ้ายังไม่มี)
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = 'core' AND t.name = 'Notification'
)
BEGIN
    -- สร้าง table โดยไม่มี FK ก่อน แล้วค่อยเพิ่ม FK ถ้า UserAccount มีอยู่
    CREATE TABLE core.Notification (
        NotificationId  INT           IDENTITY(1,1) NOT NULL,
        RecipientUserId INT           NOT NULL,
        EventType       NVARCHAR(100) NOT NULL,
        RelatedEntityId NVARCHAR(100) NULL,
        MessageText     NVARCHAR(MAX) NOT NULL,
        IsRead          BIT           NOT NULL CONSTRAINT DF_core_Notif_IsRead    DEFAULT 0,
        DeliveredAt     DATETIME      NOT NULL CONSTRAINT DF_core_Notif_Delivered DEFAULT GETDATE(),
        ReadAt          DATETIME      NULL,
        CONSTRAINT PK_core_Notification PRIMARY KEY (NotificationId)
    );

    CREATE INDEX IX_core_Notif_User ON core.Notification(RecipientUserId, IsRead);

    -- เพิ่ม FK เฉพาะเมื่อ core.UserAccount มีอยู่
    IF EXISTS (
        SELECT 1 FROM sys.tables t
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
        WHERE s.name = 'core' AND t.name = 'UserAccount'
    )
    BEGIN
        ALTER TABLE core.Notification
            ADD CONSTRAINT FK_core_Notification_User
            FOREIGN KEY (RecipientUserId) REFERENCES core.UserAccount(UserID);
        PRINT 'Added FK to core.UserAccount';
    END
    ELSE
        PRINT 'WARNING: core.UserAccount not found — FK skipped. Run schema scripts first.';

    PRINT 'Created table core.Notification';
END
ELSE
    PRINT 'Table core.Notification already exists — skipped';
GO

-- ============================================================
-- 2. core.sp_InsertNotification
-- ============================================================
IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
    WHERE s.name = 'core' AND p.name = 'sp_InsertNotification'
)
    DROP PROCEDURE core.sp_InsertNotification;
GO

CREATE PROCEDURE core.sp_InsertNotification
    @RecipientUserId INT,
    @EventType       NVARCHAR(100),
    @RelatedEntityId NVARCHAR(100) = NULL,
    @MessageText     NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO core.Notification (RecipientUserId, EventType, RelatedEntityId, MessageText)
    VALUES (@RecipientUserId, @EventType, @RelatedEntityId, @MessageText);
    SELECT SCOPE_IDENTITY() AS NewNotificationId;
END
GO

-- ============================================================
-- 3. core.sp_GetUnreadNotificationsByUser
-- ============================================================
IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
    WHERE s.name = 'core' AND p.name = 'sp_GetUnreadNotificationsByUser'
)
    DROP PROCEDURE core.sp_GetUnreadNotificationsByUser;
GO

CREATE PROCEDURE core.sp_GetUnreadNotificationsByUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT NotificationId, RecipientUserId, EventType, RelatedEntityId,
           MessageText, IsRead, DeliveredAt, ReadAt
    FROM   core.Notification
    WHERE  RecipientUserId = @UserId
      AND  IsRead = 0
    ORDER  BY DeliveredAt DESC;
END
GO

-- ============================================================
-- 4. core.sp_MarkNotificationRead
-- ============================================================
IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
    WHERE s.name = 'core' AND p.name = 'sp_MarkNotificationRead'
)
    DROP PROCEDURE core.sp_MarkNotificationRead;
GO

CREATE PROCEDURE core.sp_MarkNotificationRead
    @NotificationId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE core.Notification
    SET    IsRead = 1,
           ReadAt = GETDATE()
    WHERE  NotificationId = @NotificationId;
END
GO

PRINT 'Script 068 completed successfully';
