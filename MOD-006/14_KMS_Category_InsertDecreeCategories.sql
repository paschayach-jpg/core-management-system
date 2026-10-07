-- =============================================================================
-- MOD-006: เพิ่มหมวดพระราชกฤษฎีกา (13 หมวด) ลง KMS.m_KBCategory (non-destructive, รันซ้ำได้)
--
-- ข้อมูลจากไฟล์ หมวดหมู่พระราชกฤษฎีกา3.xlsx (sheet แรก, ข้ามแถวที่ 2 และแถว "รวม"): คอลัมน์ A = CategoryCode, B = CategoryName
-- ทุกหมวด: ParentCategoryID = CategoryID ของหมวด "พระราชกฤษฎีกา" (CategoryCode = 'DDD00' — KBImportLayoutTypeCatalog.DecreeRootCategoryCode), Icon = '1', SortOrder = 17..29 (เรียงตามไฟล์), IsActive = 1, CreatedBy = 'system', CreatedDate = GETDATE()
-- ตรวจ CategoryCode ซ้ำก่อน insert — ถ้ามีอยู่แล้วให้ข้าม ไม่แก้ไขแถวเดิม
-- ไม่ลบหรือแก้ข้อมูลเดิม; ทำงานใน transaction เดียว ถ้า error จะ rollback ทั้งหมด
-- รองรับ CategoryID ทั้งแบบ IDENTITY และแบบ INT ธรรมดา (MAX+1) เหมือน KBCategoryRepository.Insert
-- เงื่อนไขก่อนรัน: ต้องมีหมวด 'DDD00' (พระราชกฤษฎีกา) อยู่แล้ว และคอลัมน์ CategoryName กว้างพอ (ชื่อยาวสุด 102 ตัวอักษร)
-- =============================================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID('KMS.m_KBCategory', 'U') IS NULL
    THROW 50001, N'ไม่พบตาราง KMS.m_KBCategory', 1;

DECLARE @ParentID INT = (SELECT TOP 1 CategoryID FROM KMS.m_KBCategory WHERE CategoryCode = 'DDD00');
IF @ParentID IS NULL
    THROW 50002, N'ไม่พบหมวดแม่ CategoryCode = DDD00 (พระราชกฤษฎีกา) — สร้างหมวดนี้ใต้ LW ก่อนรัน', 1;

IF COL_LENGTH('KMS.m_KBCategory', 'CategoryName') <> -1
   AND COL_LENGTH('KMS.m_KBCategory', 'CategoryName') < 102 * 2
    THROW 50003, N'KMS.m_KBCategory.CategoryName กว้างไม่พอ (ต้องรองรับ 102 ตัวอักษร) — ขยายคอลัมน์ก่อนแล้วรันใหม่', 1;

DECLARE @Src TABLE
(
    Ord          INT IDENTITY(1,1) NOT NULL,
    CategoryCode VARCHAR(20)       NOT NULL,
    CategoryName NVARCHAR(300)     NOT NULL,
    SortOrder    INT               NOT NULL
);

INSERT INTO @Src (CategoryCode, CategoryName, SortOrder)
VALUES
        ('DED00', N'ออกตามความในประมวลรัษฎากร ว่าด้วยการกำหนดค่าใช้จ่ายที่ยอมให้หักจากเงินได้พึงประเมิน', 17),
        ('SBT00', N'ออกตามความในประมวลรัษฎากร ว่าด้วยการกำหนดกิจการที่ได้รับยกเว้นภาษีธุรกิจเฉพาะ', 18),
        ('VOP00', N'ออกตามความในประมวลรัษฎากร ว่าด้วยการกำหนดให้กิจการที่ได้รับยกเว้นภาษีมูลค่าเพิ่มเสียภาษีมูลค่าเพิ่มได้', 19),
        ('EXM00', N'ออกตามความในประมวลรัษฎากร ว่าด้วยการยกเว้นรัษฎากร', 20),
        ('REX00', N'ออกตามความในประมวลรัษฎากร ว่าด้วยการลดอัตราและยกเว้นรัษฎากร', 21),
        ('VEX00', N'ออกตามความในประมวลรัษฎากร ว่าด้วยการยกเว้นภาษีมูลค่าเพิ่ม', 22),
        ('VRD00', N'ออกตามความในประมวลรัษฎากร ว่าด้วยการลดอัตราภาษีมูลค่าเพิ่ม', 23),
        ('RRD00', N'ออกตามความในประมวลรัษฎากร ว่าด้วยการลดอัตรารัษฎากร', 24),
        ('SSO00', N'ออกตามความในกฎหมายว่าด้วยการประกันสังคม', 25),
        ('DPR00', N'ออกตามความในกฎหมายว่าด้วยการคุ้มครองข้อมูลส่วนบุคคล', 26),
        ('ETR00', N'ออกตามความในกฎหมายว่าด้วยธุรกรรมทางอิเล็กทรอนิกส์', 27),
        ('PRD00', N'พระราชทานอภัยโทษ', 28),
        ('BRP00', N'ให้ใช้พระราชบัญญัติล้มละลาย', 29);

DECLARE @IsIdentity BIT = ISNULL(COLUMNPROPERTY(OBJECT_ID('KMS.m_KBCategory'), 'CategoryID', 'IsIdentity'), 0);
DECLARE @Code VARCHAR(20), @Name NVARCHAR(300), @Sort INT, @NewID INT;
DECLARE @Inserted INT = 0, @Skipped INT = 0;

BEGIN TRANSACTION;

DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT CategoryCode, CategoryName, SortOrder FROM @Src ORDER BY Ord;
OPEN c;
FETCH NEXT FROM c INTO @Code, @Name, @Sort;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT 1 FROM KMS.m_KBCategory WITH (UPDLOCK, HOLDLOCK) WHERE CategoryCode = @Code)
    BEGIN
        SET @Skipped += 1;
        PRINT N'ข้าม (มีอยู่แล้ว): ' + @Code;
    END
    ELSE
    BEGIN
        IF @IsIdentity = 1
        BEGIN
            INSERT INTO KMS.m_KBCategory
                (ParentCategoryID, CategoryCode, CategoryName, Icon, Description, SortOrder, IsActive, CreatedBy, CreatedDate)
            VALUES
                (@ParentID, @Code, @Name, '1', NULL, @Sort, 1, 'system', GETDATE());
        END
        ELSE
        BEGIN
            SELECT @NewID = ISNULL(MAX(CategoryID), 0) + 1 FROM KMS.m_KBCategory WITH (UPDLOCK, HOLDLOCK);
            INSERT INTO KMS.m_KBCategory
                (CategoryID, ParentCategoryID, CategoryCode, CategoryName, Icon, Description, SortOrder, IsActive, CreatedBy, CreatedDate)
            VALUES
                (@NewID, @ParentID, @Code, @Name, '1', NULL, @Sort, 1, 'system', GETDATE());
        END
        SET @Inserted += 1;
    END
    FETCH NEXT FROM c INTO @Code, @Name, @Sort;
END
CLOSE c;
DEALLOCATE c;

COMMIT TRANSACTION;

PRINT N'เพิ่มใหม่ ' + CAST(@Inserted AS NVARCHAR(10)) + N' หมวด, ข้าม ' + CAST(@Skipped AS NVARCHAR(10)) + N' หมวด';
GO
