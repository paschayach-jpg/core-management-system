-- =============================================================================
-- MOD-006: ย้ายบทความกฎหมายไปหมวด "ปี" ของกฎหมายนั้น (non-destructive, รันซ้ำได้)
--
-- ใช้เมื่อกฎหมายฉบับหนึ่งแยกหมวดตามปีแล้ว เช่น
--     กฎหมาย (LW) > พระราชบัญญัติประกันชีวิต (PLI) > พระราชบัญญัติประกันชีวิต2535 (PLI2535)
--                                                   > พระราชบัญญัติประกันชีวิต2540 (PLI2540)
-- แต่บทความที่ import ไว้ก่อนหน้านี้ยังอยู่ที่หมวดกฎหมาย (PLI) — สคริปต์นี้ย้ายบทความไปหมวดปีตามปีใน ArticleCode
--     ปี พ.ศ. = '2' + ตัวที่ 8-10 ของ ArticleCode  (เช่น PLI0029535... -> 2535 -> หมวด PLI2535)
-- (การ upload Excel ซ้ำไม่ย้ายให้ เพราะ ArticleCode ที่มีอยู่แล้วจะถูกข้าม)
--
-- ย้ายเฉพาะบทความที่เข้าเงื่อนไขครบทุกข้อ:
--   1) บทความอยู่ในหมวดที่อยู่ใต้หมวดกฎหมาย 'LW' (ไม่แตะหมวดอื่น เช่น คำสั่ง/ประกาศ)
--   2) ArticleCode ขึ้นต้นด้วยรหัสหมวดนั้น และตัวที่ 8-10 เป็นตัวเลข
--   3) หมวดนั้นมีหมวดลูกโดยตรงที่ CategoryCode = [รหัสหมวด] + พ.ศ. 4 หลักของบทความ (เช่น PLI2535) และ IsActive = 1
-- บทความที่ไม่เข้าเงื่อนไข (เช่น ปีที่ยังไม่มีหมวดปี) ไม่ถูกแตะ — สร้างหมวดปีให้ครบแล้วรันสคริปต์นี้ซ้ำได้
-- แก้เฉพาะ KMS.t_KBArticle.CategoryID ไม่แตะคอลัมน์อื่น และทำใน transaction เดียว
-- =============================================================================

SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#Move') IS NOT NULL DROP TABLE #Move;

;WITH LawTree AS
(
    SELECT CategoryID
    FROM   KMS.m_KBCategory
    WHERE  CategoryCode = 'LW'

    UNION ALL

    SELECT c.CategoryID
    FROM   KMS.m_KBCategory c
    JOIN   LawTree t ON c.ParentCategoryID = t.CategoryID
)
SELECT a.ArticleID,
       a.CategoryID AS FromCategoryID,
       y.CategoryID AS ToCategoryID
INTO   #Move
FROM   KMS.t_KBArticle a
JOIN   LawTree          lt ON lt.CategoryID = a.CategoryID
JOIN   KMS.m_KBCategory g  ON g.CategoryID  = a.CategoryID
JOIN   KMS.m_KBCategory y  ON y.ParentCategoryID = g.CategoryID
                          AND y.IsActive = 1
                          AND y.CategoryCode = g.CategoryCode + '2' + SUBSTRING(a.ArticleCode, 8, 3)
WHERE  g.CategoryCode <> 'LW'
  AND  LEN(a.ArticleCode) >= 10
  AND  LEFT(a.ArticleCode, LEN(g.CategoryCode)) = g.CategoryCode
  AND  SUBSTRING(a.ArticleCode, 8, 3) NOT LIKE '%[^0-9]%'
OPTION (MAXRECURSION 32);

-- สรุปก่อนย้าย: จากหมวดไหน -> ไปหมวดปีไหน กี่บทความ
SELECT gf.CategoryCode AS FromCategoryCode,
       gt.CategoryCode AS ToCategoryCode,
       COUNT(*)        AS Articles
FROM   #Move m
JOIN   KMS.m_KBCategory gf ON gf.CategoryID = m.FromCategoryID
JOIN   KMS.m_KBCategory gt ON gt.CategoryID = m.ToCategoryID
GROUP  BY gf.CategoryCode, gt.CategoryCode
ORDER  BY gf.CategoryCode, gt.CategoryCode;

BEGIN TRY
    BEGIN TRAN;

    UPDATE a
    SET    a.CategoryID = m.ToCategoryID
    FROM   KMS.t_KBArticle a
    JOIN   #Move           m ON m.ArticleID = a.ArticleID;

    DECLARE @moved INT = @@ROWCOUNT;

    COMMIT TRAN;

    PRINT N'ย้ายบทความไปหมวดปีแล้ว ' + CAST(@moved AS NVARCHAR(20)) + N' รายการ';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH

DROP TABLE #Move;
GO
