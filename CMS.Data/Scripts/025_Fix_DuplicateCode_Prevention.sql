-- ============================================================
-- Script 025: Fix Duplicate Code Prevention
-- Updates sp_InsertOrgUnit to prevent duplicate OrgUnitCode
-- ============================================================

-- Drop and recreate the stored procedure with duplicate check
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'core.sp_InsertOrgUnit') AND type in (N'P', N'PC'))
    DROP PROCEDURE core.sp_InsertOrgUnit;
GO

CREATE PROCEDURE core.sp_InsertOrgUnit
    @OrgUnitCode    NVARCHAR(20),
    @OrgUnitNameTH  NVARCHAR(200),
    @OrgUnitNameEN  NVARCHAR(200),
    @OrgUnitType    TINYINT,
    @ParentOrgUnitID INT,
    @SortOrder      INT,
    @CreatedBy      INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check for duplicate code
    IF EXISTS (SELECT 1 FROM core.OrgUnit WHERE OrgUnitCode = @OrgUnitCode AND IsDeleted = 0)
    BEGIN
        RAISERROR('รหัสหน่วยงาน %s มีอยู่ในระบบแล้ว', 16, 1, @OrgUnitCode);
        RETURN;
    END
    
    INSERT INTO core.OrgUnit (OrgUnitCode, OrgUnitNameTH, OrgUnitNameEN, OrgUnitType, ParentOrgUnitID, SortOrder, CreatedBy)
    VALUES (@OrgUnitCode, @OrgUnitNameTH, @OrgUnitNameEN, @OrgUnitType, @ParentOrgUnitID, @SortOrder, @CreatedBy);
    SELECT SCOPE_IDENTITY() AS NewOrgUnitID;
END
GO

PRINT 'Script 025: Updated sp_InsertOrgUnit with duplicate code prevention';