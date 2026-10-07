-- =============================================================================
-- MOD-006: ขยาย CategoryCode ของหมวดประเภทเอกสารที่ไม่ใช่กฎหมายจาก 4 เป็น 5 ตัว โดยเติม '0' (OR00 -> OR000) พร้อมแก้รหัสหมวดปีลูกตาม
-- (ไม่ลบข้อมูล เก็บค่าเดิมไว้ในตารางสำรอง, ทำใน transaction เดียว, รันซ้ำได้ — รันครั้งที่สองจะไม่ทำอะไร)
--
-- ทำไมต้องแก้หมวดปีด้วย: โปรแกรมสร้างรหัสหมวดปีจาก [CategoryCode ของหมวดประเภท] + พ.ศ. 4 หลัก (KBImportCategoryResolver.TryResolveNonLawArticle)
--   ถ้าแก้เฉพาะหมวดแม่ หมวดปีเดิม (OR002535) จะหาไม่เจอ import ทุกแถว Error และ wizard จะแนะนำให้สร้างหมวดปีซ้ำ
--
-- สคริปต์นี้ทำ (เฉพาะหมวดรากที่ ParentCategoryID IS NULL และรหัสตรงรายการด้านล่างเท่านั้น):
--   1) หมวดประเภท   OR00 -> OR000, AN00 -> AN000, CI00 -> CI000, RG00 -> RG000, SO00 -> SO000, MR00 -> MR000
--   2) หมวดปีลูกโดยตรง  [รหัสแม่เดิม]+พ.ศ. 4 หลัก -> [รหัสแม่ใหม่]+พ.ศ.   เช่น OR002535 -> OR0002535
--   3) เก็บค่าเดิมไว้ใน KMS.bak_KBTypeCode5_Category (วิธีย้อนกลับอยู่ท้ายไฟล์)
-- ไม่แตะ: ArticleCode / CategoryID / ParentCategoryID / ImportTypeDigit / บทความ (NonLaw ArticleCode ไม่ได้ใช้ CategoryCode ของหมวดประเภท)
--
-- ข้อควรระวัง:
--   * รันพร้อมกับการใช้โปรแกรมเวอร์ชันปัจจุบัน (โปรแกรมไม่ hardcode รหัสหมวดประเภท จึงไม่ต้องแก้โค้ด) และห้ามมีใคร import Excel ระหว่างรัน
--   * ถ้าพบหมวดลูกที่ไม่ใช่รูปแบบหมวดปี รหัสใหม่ซ้ำ/ชนหมวดอื่น หรือคอลัมน์แคบเกินไป สคริปต์จะหยุดก่อนแก้ข้อมูลใดๆ พร้อมรายการที่มีปัญหา
--   * ระบบอื่นนอก MOD-006 ที่อ้าง CategoryCode ตรงๆ ต้องปรับแยกต่างหาก
--   * หมวดประเภทที่สร้างใหม่หลังจากนี้ ให้ตั้งรหัส 5 ตัวเองที่หน้าหมวดหมู่ (โปรแกรมต่อรหัสปีให้อัตโนมัติ)
-- =============================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID('KMS.m_KBCategory', 'U') IS NULL
    THROW 50001, N'ไม่พบตาราง KMS.m_KBCategory', 1;

-- ── 1) ตารางสำรองค่าเดิม (สะสมได้หลายครั้ง) ────────────────────────────────────
IF OBJECT_ID('KMS.bak_KBTypeCode5_Category', 'U') IS NULL
    CREATE TABLE KMS.bak_KBTypeCode5_Category
    (
        CategoryID  INT         NOT NULL,
        OldCode     VARCHAR(50) NOT NULL,
        NewCode     VARCHAR(50) NOT NULL,
        Kind        VARCHAR(10) NOT NULL,
        ChangedDate DATETIME    NOT NULL DEFAULT GETDATE()
    );

-- ── 2) แผนที่รหัสหมวดเดิม -> ใหม่ ────────────────────────────────────────────────
IF OBJECT_ID('tempdb..#Types') IS NOT NULL DROP TABLE #Types;
IF OBJECT_ID('tempdb..#CatMap') IS NOT NULL DROP TABLE #CatMap;
IF OBJECT_ID('tempdb..#Unmapped') IS NOT NULL DROP TABLE #Unmapped;

CREATE TABLE #Types (OldCode VARCHAR(50) NOT NULL, NewCode VARCHAR(50) NOT NULL);
CREATE TABLE #CatMap (CategoryID INT NOT NULL, OldCode VARCHAR(50) NOT NULL, NewCode VARCHAR(50) NOT NULL, Kind VARCHAR(10) NOT NULL);

INSERT INTO #Types (OldCode, NewCode)
VALUES ('OR00', 'OR000'), ('AN00', 'AN000'), ('CI00', 'CI000'),
       ('RG00', 'RG000'), ('SO00', 'SO000'), ('MR00', 'MR000');

-- หมวดประเภท = หมวดรากที่รหัสตรงรายการ (หมวดที่แปลงแล้วจะไม่ตรง จึงรันซ้ำได้)
INSERT INTO #CatMap (CategoryID, OldCode, NewCode, Kind)
SELECT c.CategoryID, LTRIM(RTRIM(c.CategoryCode)), t.NewCode, 'TYPE'
FROM   KMS.m_KBCategory c
JOIN   #Types t ON t.OldCode = UPPER(LTRIM(RTRIM(c.CategoryCode)))
WHERE  c.ParentCategoryID IS NULL;

IF NOT EXISTS (SELECT 1 FROM #CatMap)
BEGIN
    PRINT N'ไม่พบหมวดประเภทที่ต้องแปลง (แปลงไปแล้วหรือไม่มีรหัสตรงรายการ) — ไม่ทำอะไร';
    RETURN;
END

-- หมวดปี = ลูกโดยตรงของหมวดประเภท รหัส [รหัสแม่เดิม] + ตัวเลข 4 หลัก
INSERT INTO #CatMap (CategoryID, OldCode, NewCode, Kind)
SELECT c.CategoryID, LTRIM(RTRIM(c.CategoryCode)), p.NewCode + RIGHT(LTRIM(RTRIM(c.CategoryCode)), 4), 'YEAR'
FROM   KMS.m_KBCategory c
JOIN   #CatMap p ON p.CategoryID = c.ParentCategoryID AND p.Kind = 'TYPE'
WHERE  LEN(LTRIM(RTRIM(c.CategoryCode))) = LEN(p.OldCode) + 4
  AND  UPPER(LEFT(LTRIM(RTRIM(c.CategoryCode)), LEN(p.OldCode))) = UPPER(p.OldCode)
  AND  RIGHT(LTRIM(RTRIM(c.CategoryCode)), 4) NOT LIKE '%[^0-9]%';

-- ── 3) ตรวจก่อนแก้ ───────────────────────────────────────────────────────────
-- ลูกหลานของหมวดประเภทที่ไม่เข้ารูปแบบหมวดปี (เหลือไม่ถูกแปลง) -> หยุด
;WITH d AS
(
    SELECT c.CategoryID, c.CategoryCode
    FROM   KMS.m_KBCategory c
    WHERE  c.ParentCategoryID IN (SELECT CategoryID FROM #CatMap WHERE Kind = 'TYPE')
    UNION ALL
    SELECT c.CategoryID, c.CategoryCode
    FROM   KMS.m_KBCategory c
    JOIN   d ON c.ParentCategoryID = d.CategoryID
)
SELECT CategoryID, CategoryCode INTO #Unmapped FROM d WHERE CategoryID NOT IN (SELECT CategoryID FROM #CatMap);

IF EXISTS (SELECT 1 FROM #Unmapped)
BEGIN
    SELECT N'หมวดลูกที่ไม่เข้ารูปแบบหมวดปี (ไม่ถูกแปลง)' AS Problem, CategoryID, CategoryCode FROM #Unmapped;
    THROW 50002, N'มีหมวดใต้หมวดประเภทที่ไม่ใช่รูปแบบ [รหัสแม่]+พ.ศ. 4 หลัก (ดูรายการที่แสดง) — แก้เองหรือแจ้งผู้พัฒนา แล้วรันใหม่', 1;
END

-- ความกว้างคอลัมน์ต้องรองรับรหัสใหม่
DECLARE @catLen INT = (SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS
                       WHERE TABLE_SCHEMA = 'KMS' AND TABLE_NAME = 'm_KBCategory' AND COLUMN_NAME = 'CategoryCode');
DECLARE @maxNew INT = (SELECT MAX(LEN(NewCode)) FROM #CatMap);

IF @catLen IS NOT NULL AND @catLen <> -1 AND @catLen < @maxNew
    THROW 50003, N'KMS.m_KBCategory.CategoryCode แคบเกินไปสำหรับรหัสใหม่ — ขยายคอลัมน์ก่อนแล้วรันใหม่', 1;

-- รหัสใหม่ซ้ำกันเอง หรือชนกับหมวดอื่นที่ไม่ได้ถูกแปลง -> หยุด
IF EXISTS (SELECT NewCode FROM #CatMap GROUP BY NewCode HAVING COUNT(*) > 1)
   OR EXISTS (SELECT 1 FROM #CatMap m JOIN KMS.m_KBCategory x ON UPPER(LTRIM(RTRIM(x.CategoryCode))) = UPPER(m.NewCode)
              WHERE x.CategoryID <> m.CategoryID AND x.CategoryID NOT IN (SELECT CategoryID FROM #CatMap))
BEGIN
    SELECT N'รหัสหมวดใหม่ซ้ำ' AS Problem, NewCode FROM #CatMap GROUP BY NewCode HAVING COUNT(*) > 1;
    SELECT N'รหัสหมวดใหม่ชนหมวดอื่น' AS Problem, m.CategoryID, m.NewCode
    FROM   #CatMap m
    JOIN   KMS.m_KBCategory x ON UPPER(LTRIM(RTRIM(x.CategoryCode))) = UPPER(m.NewCode)
    WHERE  x.CategoryID <> m.CategoryID AND x.CategoryID NOT IN (SELECT CategoryID FROM #CatMap);
    THROW 50004, N'รหัสหมวดใหม่ซ้ำหรือชนกับหมวดที่มีอยู่ — ตรวจรายการที่แสดงแล้วรันใหม่', 1;
END

-- ── 4) สรุปก่อนแก้ ────────────────────────────────────────────────────────────
SELECT CategoryID, Kind, OldCode, NewCode FROM #CatMap ORDER BY Kind DESC, OldCode;

-- ── 5) แก้ข้อมูล (transaction เดียว) ───────────────────────────────────────────
BEGIN TRY
    BEGIN TRAN;

    INSERT INTO KMS.bak_KBTypeCode5_Category (CategoryID, OldCode, NewCode, Kind)
    SELECT CategoryID, OldCode, NewCode, Kind FROM #CatMap;

    UPDATE c
    SET    c.CategoryCode = m.NewCode
    FROM   KMS.m_KBCategory c
    JOIN   #CatMap          m ON m.CategoryID = c.CategoryID;

    DECLARE @cats INT = @@ROWCOUNT;

    COMMIT TRAN;

    PRINT N'แปลง CategoryCode แล้ว ' + CAST(@cats AS NVARCHAR(20)) + N' หมวด (หมวดประเภท + หมวดปี)';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH

DROP TABLE #Unmapped;
DROP TABLE #CatMap;
DROP TABLE #Types;

-- =============================================================================
-- ย้อนกลับ (ใช้เมื่อจำเป็นเท่านั้น) — คัดลอกไปรันทั้งชุดใน transaction:
--
--   BEGIN TRAN;
--   UPDATE c SET c.CategoryCode = b.OldCode FROM KMS.m_KBCategory c JOIN KMS.bak_KBTypeCode5_Category b ON b.CategoryID = c.CategoryID;
--   DELETE FROM KMS.bak_KBTypeCode5_Category;
--   COMMIT TRAN;
--
-- หมวดปีที่สร้างหลังแปลงเป็นรูปแบบใหม่ไม่มีในตารางสำรอง ย้อนกลับแล้วต้องจัดการเอง
-- =============================================================================
