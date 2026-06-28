-- รันบน DB ก่อน deploy
-- สร้าง table + 2 SPs สำหรับบันทึก hidden node IDs ต่อ user

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserOrgUnitHiddenView' AND SCHEMA_NAME(schema_id) = 'EMS')
BEGIN
    CREATE TABLE EMS.UserOrgUnitHiddenView (
        UserID    INT NOT NULL,
        OrgUnitId INT NOT NULL,
        CONSTRAINT PK_UserOrgUnitHiddenView PRIMARY KEY (UserID, OrgUnitId)
    );
END
GO

-- ดึง OrgUnitId ที่ user ซ่อนไว้
IF OBJECT_ID('EMS.sp_GetHiddenOrgUnitIds', 'P') IS NOT NULL
    DROP PROCEDURE EMS.sp_GetHiddenOrgUnitIds;
GO

CREATE PROCEDURE EMS.sp_GetHiddenOrgUnitIds
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT OrgUnitId
    FROM EMS.UserOrgUnitHiddenView
    WHERE UserID = @UserId;
END
GO

-- บันทึก OrgUnitId ที่ซ่อน (แทนที่ทั้งหมดของ user นี้)
-- @XmlIds: XML รูปแบบ <ids><id>1</id><id>2</id></ids>  หรือ NULL = ล้างทั้งหมด
IF OBJECT_ID('EMS.sp_SaveHiddenOrgUnitIds', 'P') IS NOT NULL
    DROP PROCEDURE EMS.sp_SaveHiddenOrgUnitIds;
GO

CREATE PROCEDURE EMS.sp_SaveHiddenOrgUnitIds
    @UserId INT,
    @XmlIds XML = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    DELETE FROM EMS.UserOrgUnitHiddenView WHERE UserID = @UserId;

    IF @XmlIds IS NOT NULL
    BEGIN
        INSERT INTO EMS.UserOrgUnitHiddenView (UserID, OrgUnitId)
        SELECT @UserId, t.c.value('.', 'INT')
        FROM @XmlIds.nodes('/ids/id') AS t(c);
    END

    COMMIT TRANSACTION;
END
GO
