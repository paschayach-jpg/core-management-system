-- 073_Fix_AuditLog_SP.sql
-- แก้ไข audit.sp_QueryAuditLog ให้:
-- 1. อ้างอิง audit.AuditLog (ไม่ใช่ dbo.AuditLog)
-- 2. เพิ่ม @UserName filter (LIKE search)
-- 3. @Result รับค่า NULL ได้ (filter เฉพาะเมื่อระบุ)

USE [YOUR_DB_NAME];  -- เปลี่ยนเป็นชื่อ Database จริง
GO

IF OBJECT_ID('audit.sp_QueryAuditLog', 'P') IS NOT NULL
    DROP PROCEDURE audit.sp_QueryAuditLog;
GO

CREATE PROCEDURE audit.sp_QueryAuditLog
    @FromDate   DATETIME,
    @ToDate     DATETIME,
    @UserID     INT,
    @UserName   NVARCHAR(200),
    @Module     NVARCHAR(100),
    @Action     NVARCHAR(100),
    @Result     TINYINT,
    @PageNumber INT,
    @PageSize   INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    SELECT COUNT(*) OVER () AS TotalCount,
           AuditID, AuditDate, UserID, UserName, UserRole, Module, Action,
           TableName, RecordID, OldValue, NewValue, IPAddress, MachineName,
           SessionID, Result, ErrorMessage
    FROM   audit.AuditLog
    WHERE  AuditDate >= @FromDate AND AuditDate <= @ToDate
      AND  (@UserID   IS NULL OR UserID   = @UserID)
      AND  (@UserName IS NULL OR UserName LIKE '%' + @UserName + '%')
      AND  (@Module   IS NULL OR Module   = @Module)
      AND  (@Action   IS NULL OR Action   = @Action)
      AND  (@Result   IS NULL OR Result   = @Result)
    ORDER  BY AuditDate DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
