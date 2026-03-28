-- ============================================================
-- 013_Add_GetAllPermissionsByFolder.sql
-- Adds SP that returns ALL permission entries including revoked
-- for display in the "show all" toggle in PermissionForm
-- ============================================================

IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_GetAllPermissionsByFolder' AND schema_id = SCHEMA_ID('security'))
    DROP PROCEDURE security.sp_GetAllPermissionsByFolder;
GO

CREATE PROCEDURE security.sp_GetAllPermissionsByFolder
    @FolderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PermissionID, FolderID, PrincipalType, PrincipalID,
           CanRead, CanWrite, CanDelete, CanModify, CanFullControl, IsDeny,
           ExpiryDate, GrantedBy, GrantedDate, RevokedBy, RevokedDate, RevokeReason
    FROM   security.FolderPermission
    WHERE  FolderID = @FolderID
    ORDER  BY CASE WHEN RevokedDate IS NULL THEN 0 ELSE 1 END, GrantedDate DESC;
    -- Active entries first (0), then revoked (1)
END
GO
