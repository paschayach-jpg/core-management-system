-- =============================================================================
-- MOD-006: ย้ายบทความที่ไม่ใช่กฎหมาย (AN/OR/SO/RG/CI/MR) ไปหมวด "ปี" ของประเภทเอกสาร (non-destructive, รันซ้ำได้)
--
-- ใช้เมื่อสร้างหมวดปีใต้หมวดประเภทเอกสารแล้ว เช่น
--     คำสั่ง (OR00) > OR002535, OR002551 ...        หนังสือเวียน (CI00) > CI002535 ...
-- แต่บทความที่ import ไว้ก่อนหน้านี้ยังอยู่ที่หมวดประเภท (OR00) — สคริปต์นี้ย้ายบทความไปหมวดปีตามปีใน ArticleCode
--     ปี พ.ศ. = '2' + ตัวที่ 10-12 ของ ArticleCode  (เช่น PVI000029535... -> 2535 -> OR00 + 2535 = หมวด OR002535)
--     (ตัวที่ 10-12 คือปีของกฎหมายแม่ — 24 ตัวแรกของ ArticleCode ของบทความที่ไม่ใช่กฎหมายคือรหัสมาตราของกฎหมายแม่)
-- (การ upload Excel ซ้ำไม่ย้ายให้ เพราะ ArticleCode ที่มีอยู่แล้วจะถูกข้าม)
--
-- ย้ายเฉพาะบทความที่เข้าเงื่อนไขครบทุกข้อ:
--   1) บทความอยู่ในหมวดหลัก (ไม่มี parent) ที่ไม่ใช่ 'LW' — คือหมวดประเภทเอกสาร (ไม่แตะบทความกฎหมาย)
--   2) ArticleCode ยาว 29 ตัว, 5 ตัวสุดท้ายไม่ใช่ '00000' และตัวที่ 10-12 เป็นตัวเลข
--   3) หมวดนั้นมีหมวดลูกโดยตรงที่ CategoryCode = [รหัสหมวดประเภท] + พ.ศ. 4 หลักของบทความ (เช่น OR002535) และ IsActive = 1
-- บทความที่ไม่เข้าเงื่อนไข (เช่น ปีที่ยังไม่มีหมวดปี) ไม่ถูกแตะ — สร้างหมวดปีให้ครบแล้วรันสคริปต์นี้ซ้ำได้
-- แก้เฉพาะ KMS.t_KBArticle.CategoryID ไม่แตะคอลัมน์อื่น และทำใน transaction เดียว
-- (รูปแบบรหัส 29 หลัก/CategoryCode 5 ตัว — ใช้หลังรันสคริปต์ 12 แล้วเท่านั้น)
-- =============================================================================

SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#Move') IS NOT NULL DROP TABLE #Move;

SELECT a.ArticleID,
       a.CategoryID AS FromCategoryID,
       y.CategoryID AS ToCategoryID
INTO   #Move
FROM   KMS.t_KBArticle a
JOIN   KMS.m_KBCategory g ON g.CategoryID = a.CategoryID
JOIN   KMS.m_KBCategory y ON y.ParentCategoryID = g.CategoryID
                         AND y.IsActive = 1
                         AND y.CategoryCode = g.CategoryCode + '2' + SUBSTRING(a.ArticleCode, 10, 3)
WHERE  g.ParentCategoryID IS NULL
  AND  g.CategoryCode <> 'LW'
  AND  LEN(a.ArticleCode) = 29
  AND  RIGHT(a.ArticleCode, 5) <> '00000'
  AND  SUBSTRING(a.ArticleCode, 10, 3) NOT LIKE '%[^0-9]%';

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
