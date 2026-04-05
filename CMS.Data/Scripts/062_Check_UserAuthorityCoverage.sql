-- ============================================================
-- Script 062: ตรวจสอบว่า UserAccount แต่ละคน
--             มี FolderAuthorityConfig รองรับหรือไม่
-- ============================================================

-- ── 1. Users ที่ match ด้วย LevelCode ────────────────────────
SELECT
    u.UserID,
    u.ADUserName,
    u.DisplayName,
    u.LevelCode,
    u.PositionName,
    u.DepartmentCode,
    u.DivisionCode,
    'LevelCode'   AS MatchedBy,
    c.ConfigLabel,
    COUNT(c.ConfigID) AS ConfigRows
FROM       core.UserAccount          u
INNER JOIN core.FolderAuthorityConfig c
        ON c.IsActive  = 1
       AND c.LevelCode = u.LevelCode
WHERE  u.IsActive  = 1
  AND  u.LevelCode IS NOT NULL
GROUP  BY u.UserID, u.ADUserName, u.DisplayName,
          u.LevelCode, u.PositionName, u.DepartmentCode,
          u.DivisionCode, c.ConfigLabel

UNION ALL

-- ── 2. Users ที่ไม่มี LevelCode แต่ match ด้วย PositionName ──
SELECT
    u.UserID,
    u.ADUserName,
    u.DisplayName,
    u.LevelCode,
    u.PositionName,
    u.DepartmentCode,
    u.DivisionCode,
    'PositionName' AS MatchedBy,
    c.ConfigLabel,
    COUNT(c.ConfigID) AS ConfigRows
FROM       core.UserAccount          u
INNER JOIN core.FolderAuthorityConfig c
        ON c.IsActive    = 1
       AND c.PositionName = u.PositionName
WHERE  u.IsActive  = 1
  AND (u.LevelCode IS NULL OR NOT EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig
        WHERE IsActive = 1 AND LevelCode = u.LevelCode))
  AND  u.PositionName IS NOT NULL
GROUP  BY u.UserID, u.ADUserName, u.DisplayName,
          u.LevelCode, u.PositionName, u.DepartmentCode,
          u.DivisionCode, c.ConfigLabel

UNION ALL

-- ── 3. Users ที่ไม่มี config รองรับเลย ──────────────────────
SELECT
    u.UserID,
    u.ADUserName,
    u.DisplayName,
    u.LevelCode,
    u.PositionName,
    u.DepartmentCode,
    u.DivisionCode,
    'NO MATCH ⚠️'  AS MatchedBy,
    NULL            AS ConfigLabel,
    0               AS ConfigRows
FROM  core.UserAccount u
WHERE u.IsActive = 1
  AND NOT EXISTS (
        SELECT 1 FROM core.FolderAuthorityConfig c
        WHERE c.IsActive = 1
          AND (   (u.LevelCode      IS NOT NULL AND c.LevelCode      = u.LevelCode)
               OR (u.PositionName   IS NOT NULL AND c.PositionName   = u.PositionName)
               OR (u.DepartmentCode IS NOT NULL AND c.DepartmentCode = u.DepartmentCode
                                               AND c.PositionName IS NULL)
               OR (u.DivisionCode   IS NOT NULL AND c.DivisionCode   = u.DivisionCode
                                               AND c.PositionName IS NULL
                                               AND c.DepartmentCode IS NULL)
              )
      )

ORDER BY MatchedBy, ADUserName;
GO

-- ── 4. สรุปจำนวน ─────────────────────────────────────────────
SELECT
    CASE
        WHEN u.LevelCode IS NOT NULL
         AND EXISTS (SELECT 1 FROM core.FolderAuthorityConfig
                     WHERE IsActive=1 AND LevelCode=u.LevelCode)
        THEN 'LevelCode'
        WHEN u.PositionName IS NOT NULL
         AND EXISTS (SELECT 1 FROM core.FolderAuthorityConfig
                     WHERE IsActive=1 AND PositionName=u.PositionName)
        THEN 'PositionName'
        WHEN u.DepartmentCode IS NOT NULL
         AND EXISTS (SELECT 1 FROM core.FolderAuthorityConfig
                     WHERE IsActive=1 AND DepartmentCode=u.DepartmentCode
                       AND PositionName IS NULL)
        THEN 'DepartmentCode'
        WHEN u.DivisionCode IS NOT NULL
         AND EXISTS (SELECT 1 FROM core.FolderAuthorityConfig
                     WHERE IsActive=1 AND DivisionCode=u.DivisionCode
                       AND PositionName IS NULL AND DepartmentCode IS NULL)
        THEN 'DivisionCode'
        ELSE 'NO MATCH ⚠️'
    END                    AS MatchedBy,
    COUNT(*)               AS UserCount
FROM  core.UserAccount u
WHERE u.IsActive = 1
GROUP BY
    CASE
        WHEN u.LevelCode IS NOT NULL
         AND EXISTS (SELECT 1 FROM core.FolderAuthorityConfig
                     WHERE IsActive=1 AND LevelCode=u.LevelCode)
        THEN 'LevelCode'
        WHEN u.PositionName IS NOT NULL
         AND EXISTS (SELECT 1 FROM core.FolderAuthorityConfig
                     WHERE IsActive=1 AND PositionName=u.PositionName)
        THEN 'PositionName'
        WHEN u.DepartmentCode IS NOT NULL
         AND EXISTS (SELECT 1 FROM core.FolderAuthorityConfig
                     WHERE IsActive=1 AND DepartmentCode=u.DepartmentCode
                       AND PositionName IS NULL)
        THEN 'DepartmentCode'
        WHEN u.DivisionCode IS NOT NULL
         AND EXISTS (SELECT 1 FROM core.FolderAuthorityConfig
                     WHERE IsActive=1 AND DivisionCode=u.DivisionCode
                       AND PositionName IS NULL AND DepartmentCode IS NULL)
        THEN 'DivisionCode'
        ELSE 'NO MATCH ⚠️'
    END
ORDER BY UserCount DESC;
GO

PRINT '✅ Coverage check เสร็จสมบูรณ์';
GO
