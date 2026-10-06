-- =============================================================================
-- MOD-006: ขยายรหัสกฎหมายใน Code รวมจาก 3 เป็น 5 ตัว (PVI -> PVI00) แปลง CategoryCode และ ArticleCode ให้ตรงรูปแบบใหม่
-- (ไม่ลบข้อมูล เก็บค่าเดิมไว้ในตารางสำรอง, ทำใน transaction เดียว, รันซ้ำได้ — รันครั้งที่สองจะไม่ทำอะไร)
--
-- รูปแบบใหม่ (โปรแกรมเวอร์ชันที่อ่าน Code รวม 29 หลัก):
--   Code รวม = [รหัสกฎหมาย 5][เลขมาตรา 4][ปี 3]...[ท้าย 5]  ยาว 29 ตัว (กฎหมายลงท้าย 00000, ประเภทเอกสารอยู่ตัวที่ 25)
--   ArticleCode ที่เก็บ: กฎหมาย = 24 ตัวแรกของ Code รวม, เอกสารที่ไม่ใช่กฎหมาย = ครบ 29 ตัว
--
-- สคริปต์นี้ทำ:
--   1) KMS.m_KBCategory.CategoryCode (ยกเว้น 'LW' ที่ต้องคงเดิม):
--        หมวดกฎหมาย (ลูกโดยตรงของ LW) และหมวดประเภทเอกสาร (หมวดรากอื่นที่ไม่ใช่ LW)  ต่อท้าย '00'   PVI -> PVI00, OR -> OR00
--        หมวดปี (รหัสหมวดแม่เดิม + พ.ศ. 4 หลัก)  ->  รหัสหมวดแม่ใหม่ + พ.ศ.                         PLI2535 -> PLI002535
--   2) KMS.t_KBArticle.ArticleCode: แทรก '00' หลังรหัสกฎหมาย 3 ตัวแรก
--        กฎหมาย (ยาว 22) -> 24 ตัว,  ไม่ใช่กฎหมาย (ยาว 27 และไม่ลงท้าย 00000) -> 29 ตัว
--        เฉพาะบทความที่ 3 ตัวแรกตรงกับรหัสหมวดกฎหมายเดิม (ไม่แตะรหัส KB-yyyy-NNN ที่สร้างจากหน้า editor)
--   3) เก็บค่าเดิมไว้ใน KMS.bak_KBCodeExtend_Category / KMS.bak_KBCodeExtend_Article (วิธีย้อนกลับอยู่ท้ายไฟล์)
--
-- ข้อควรระวัง:
--   * รันพร้อมกับการติดตั้งโปรแกรมเวอร์ชันใหม่ และห้ามมีใคร import Excel/เพิ่มบทความระหว่างรัน
--   * ก่อนรันให้ SELECT LEN(ArticleCode), COUNT(*) FROM KMS.t_KBArticle GROUP BY LEN(ArticleCode) ดูว่ามีความยาวอื่นปนอยู่หรือไม่
--   * ถ้าพบหมวด/รหัสที่แปลงไม่ได้ หรือรหัสใหม่ชนกัน หรือคอลัมน์แคบเกินไป สคริปต์จะหยุดก่อนแก้ข้อมูลใดๆ พร้อมรายการที่มีปัญหา
--   * ระบบอื่นนอก MOD-006 ที่อ้าง ArticleCode/CategoryCode ตรงๆ ต้องปรับแยกต่างหาก
-- =============================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- ── 0) ความกว้างคอลัมน์ต้องรองรับรหัสใหม่ ─────────────────────────────────────
DECLARE @artLen INT = (SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS
                       WHERE TABLE_SCHEMA = 'KMS' AND TABLE_NAME = 't_KBArticle' AND COLUMN_NAME = 'ArticleCode');
DECLARE @catLen INT = (SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS
                       WHERE TABLE_SCHEMA = 'KMS' AND TABLE_NAME = 'm_KBCategory' AND COLUMN_NAME = 'CategoryCode');

IF @artLen IS NOT NULL AND @artLen <> -1 AND @artLen < 29
    THROW 50001, N'KMS.t_KBArticle.ArticleCode กว้างไม่ถึง 29 ตัว — ขยายคอลัมน์ก่อน (ALTER COLUMN ... VARCHAR(50) NOT NULL ให้ตรง collation เดิม) แล้วรันใหม่', 1;
IF @catLen IS NOT NULL AND @catLen <> -1 AND @catLen < 12
    THROW 50002, N'KMS.m_KBCategory.CategoryCode กว้างไม่ถึง 12 ตัว — ขยายคอลัมน์ก่อนแล้วรันใหม่', 1;

-- ── 1) ตารางสำรองค่าเดิม ───────────────────────────────────────────────────────
IF OBJECT_ID('KMS.bak_KBCodeExtend_Category', 'U') IS NULL
    CREATE TABLE KMS.bak_KBCodeExtend_Category
    (
        CategoryID  INT         NOT NULL PRIMARY KEY,
        OldCode     VARCHAR(50) NOT NULL,
        NewCode     VARCHAR(50) NOT NULL,
        ChangedDate DATETIME    NOT NULL DEFAULT GETDATE()
    );

IF OBJECT_ID('KMS.bak_KBCodeExtend_Article', 'U') IS NULL
    CREATE TABLE KMS.bak_KBCodeExtend_Article
    (
        ArticleID   INT         NOT NULL PRIMARY KEY,
        OldCode     VARCHAR(50) NOT NULL,
        NewCode     VARCHAR(50) NOT NULL,
        ChangedDate DATETIME    NOT NULL DEFAULT GETDATE()
    );

-- แปลงไปแล้ว (มีข้อมูลในตารางสำรอง) -> ไม่ทำซ้ำ
IF EXISTS (SELECT 1 FROM KMS.bak_KBCodeExtend_Category) OR EXISTS (SELECT 1 FROM KMS.bak_KBCodeExtend_Article)
BEGIN
    PRINT N'แปลงรหัสไปแล้ว (พบข้อมูลในตารางสำรอง) — ไม่ทำซ้ำ';
    RETURN;
END

-- ── 2) แผนที่รหัสหมวดเดิม -> ใหม่ ────────────────────────────────────────────────
IF OBJECT_ID('tempdb..#CatMap') IS NOT NULL DROP TABLE #CatMap;

SELECT c.CategoryID,
       LTRIM(RTRIM(c.CategoryCode))   AS OldCode,
       CAST(NULL AS VARCHAR(50))      AS NewCode,
       CAST('' AS VARCHAR(10))        AS Kind
INTO   #CatMap
FROM   KMS.m_KBCategory c
WHERE  UPPER(LTRIM(RTRIM(c.CategoryCode))) <> 'LW';

-- หมวดประเภทเอกสาร = หมวดรากอื่นที่ไม่ใช่ LW
UPDATE m
SET    m.NewCode = m.OldCode + '00', m.Kind = 'TYPE'
FROM   #CatMap m
JOIN   KMS.m_KBCategory c ON c.CategoryID = m.CategoryID
WHERE  c.ParentCategoryID IS NULL;

-- หมวดกฎหมาย = ลูกโดยตรงของ LW
UPDATE m
SET    m.NewCode = m.OldCode + '00', m.Kind = 'LAW'
FROM   #CatMap m
JOIN   KMS.m_KBCategory c ON c.CategoryID = m.CategoryID
JOIN   KMS.m_KBCategory p ON p.CategoryID = c.ParentCategoryID
WHERE  UPPER(LTRIM(RTRIM(p.CategoryCode))) = 'LW';

-- หมวดปี = [รหัสหมวดแม่เดิม] + พ.ศ. 4 หลัก  ->  [รหัสหมวดแม่ใหม่] + พ.ศ.
UPDATE m
SET    m.NewCode = pm.NewCode + RIGHT(m.OldCode, 4), m.Kind = 'YEAR'
FROM   #CatMap m
JOIN   KMS.m_KBCategory c  ON c.CategoryID = m.CategoryID
JOIN   #CatMap          pm ON pm.CategoryID = c.ParentCategoryID
WHERE  m.NewCode IS NULL
  AND  pm.Kind IN ('LAW', 'TYPE')
  AND  LEN(m.OldCode) = LEN(pm.OldCode) + 4
  AND  LEFT(m.OldCode, LEN(pm.OldCode)) = pm.OldCode
  AND  RIGHT(m.OldCode, 4) NOT LIKE '%[^0-9]%';

-- แปลงไม่ได้ (โครงสร้างไม่เข้ากฎ) -> หยุด
IF EXISTS (SELECT 1 FROM #CatMap WHERE NewCode IS NULL)
BEGIN
    SELECT N'หมวดที่แปลงรหัสไม่ได้' AS Problem, CategoryID, OldCode FROM #CatMap WHERE NewCode IS NULL;
    THROW 50003, N'มีหมวดที่ไม่เข้ากฎการแปลงรหัส (ดูรายการที่แสดง) — แก้เองหรือแจ้งผู้พัฒนา แล้วรันใหม่', 1;
END

-- รหัสใหม่ซ้ำกันเอง หรือชนกับหมวดอื่น (รวม LW) -> หยุด
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

-- ── 3) แผนที่รหัสบทความเดิม -> ใหม่ ────────────────────────────────────────────
IF OBJECT_ID('tempdb..#ArtMap') IS NOT NULL DROP TABLE #ArtMap;

SELECT a.ArticleID,
       a.ArticleCode                       AS OldCode,
       STUFF(a.ArticleCode, 4, 0, '00')    AS NewCode,
       CASE WHEN LEN(a.ArticleCode) = 22 THEN 'LAW' ELSE 'NONLAW' END AS Kind
INTO   #ArtMap
FROM   KMS.t_KBArticle a
WHERE  a.ArticleCode NOT LIKE 'KB-%'
  AND  (LEN(a.ArticleCode) = 22 OR (LEN(a.ArticleCode) = 27 AND RIGHT(a.ArticleCode, 5) <> '00000'))
  AND  EXISTS (SELECT 1 FROM #CatMap m
               WHERE m.Kind = 'LAW' AND LEN(m.OldCode) = 3 AND m.OldCode = LEFT(a.ArticleCode, 3));

IF EXISTS (SELECT NewCode FROM #ArtMap GROUP BY NewCode HAVING COUNT(*) > 1)
   OR EXISTS (SELECT 1 FROM #ArtMap m JOIN KMS.t_KBArticle x ON x.ArticleCode = m.NewCode WHERE x.ArticleID <> m.ArticleID)
BEGIN
    SELECT TOP 50 N'รหัสบทความใหม่ซ้ำ/ชน' AS Problem, m.ArticleID, m.OldCode, m.NewCode
    FROM   #ArtMap m
    WHERE  m.NewCode IN (SELECT NewCode FROM #ArtMap GROUP BY NewCode HAVING COUNT(*) > 1)
       OR  EXISTS (SELECT 1 FROM KMS.t_KBArticle x WHERE x.ArticleCode = m.NewCode AND x.ArticleID <> m.ArticleID);
    THROW 50005, N'รหัสบทความใหม่ซ้ำหรือชนกับบทความที่มีอยู่ — ตรวจรายการที่แสดงแล้วรันใหม่', 1;
END

-- ── 4) สรุปก่อนแก้ ────────────────────────────────────────────────────────────
SELECT Kind AS CategoryKind, COUNT(*) AS Categories FROM #CatMap GROUP BY Kind;
SELECT Kind AS ArticleKind,  COUNT(*) AS Articles   FROM #ArtMap GROUP BY Kind;

-- บทความที่ไม่ใช่ KB-yyyy-NNN แต่ไม่เข้ากฎแปลง (ไม่ถูกแตะ) — ตรวจดูว่าตั้งใจให้คงเดิมหรือไม่
SELECT LEN(a.ArticleCode) AS CodeLength, COUNT(*) AS NotConverted
FROM   KMS.t_KBArticle a
WHERE  a.ArticleCode NOT LIKE 'KB-%'
  AND  a.ArticleID NOT IN (SELECT ArticleID FROM #ArtMap)
GROUP  BY LEN(a.ArticleCode);

-- ── 5) แก้ข้อมูล (transaction เดียว) ───────────────────────────────────────────
BEGIN TRY
    BEGIN TRAN;

    INSERT INTO KMS.bak_KBCodeExtend_Category (CategoryID, OldCode, NewCode)
    SELECT CategoryID, OldCode, NewCode FROM #CatMap;

    INSERT INTO KMS.bak_KBCodeExtend_Article (ArticleID, OldCode, NewCode)
    SELECT ArticleID, OldCode, NewCode FROM #ArtMap;

    UPDATE c
    SET    c.CategoryCode = m.NewCode
    FROM   KMS.m_KBCategory c
    JOIN   #CatMap          m ON m.CategoryID = c.CategoryID;

    DECLARE @cats INT = @@ROWCOUNT;

    UPDATE a
    SET    a.ArticleCode = m.NewCode
    FROM   KMS.t_KBArticle a
    JOIN   #ArtMap         m ON m.ArticleID = a.ArticleID;

    DECLARE @arts INT = @@ROWCOUNT;

    COMMIT TRAN;

    PRINT N'แปลง CategoryCode แล้ว ' + CAST(@cats AS NVARCHAR(20)) + N' หมวด, ArticleCode แล้ว ' + CAST(@arts AS NVARCHAR(20)) + N' บทความ';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH

DROP TABLE #CatMap;
DROP TABLE #ArtMap;

-- =============================================================================
-- ย้อนกลับ (ใช้เมื่อจำเป็นเท่านั้น และต้องติดตั้งโปรแกรมเวอร์ชันเดิมคู่กัน) — คัดลอกไปรันทั้งชุดใน transaction:
--
--   BEGIN TRAN;
--   UPDATE c SET c.CategoryCode = b.OldCode FROM KMS.m_KBCategory c JOIN KMS.bak_KBCodeExtend_Category b ON b.CategoryID = c.CategoryID;
--   UPDATE a SET a.ArticleCode  = b.OldCode FROM KMS.t_KBArticle  a JOIN KMS.bak_KBCodeExtend_Article  b ON b.ArticleID  = a.ArticleID;
--   DELETE FROM KMS.bak_KBCodeExtend_Category;
--   DELETE FROM KMS.bak_KBCodeExtend_Article;
--   COMMIT TRAN;
--
-- หมวด/บทความที่สร้างหลังแปลงเป็นรูปแบบใหม่ไม่มีในตารางสำรอง ย้อนกลับแล้วต้องจัดการเอง
-- =============================================================================
