-- Read-only diagnostic: why a file's download dialog does / does not use workflow.DownloadApproverConfig.
-- Set @FileName, run, and compare the folder's OrgUnit codes/names with the configured OrgCode values.
DECLARE @FileName NVARCHAR(400) = N'DataDictionary_NewEFiling.docx';

-- 1) File → folder → OrgUnit chain (type 3 = Department, 2 = Division, 1 = SubCommittee)
;WITH chain AS (
    SELECT ou.OrgUnitID, ou.OrgUnitCode, ou.OrgUnitNameTH, ou.OrgUnitType, ou.ParentOrgUnitID, 0 AS Depth
    FROM   storage.FileDocument f
    JOIN   storage.Folder       fo ON fo.FolderID   = f.FolderID
    JOIN   EMS.OrgUnit          ou ON ou.OrgUnitID  = fo.OrgUnitID
    WHERE  f.OriginalFileName = @FileName AND f.FileStatus <> 3
    UNION ALL
    SELECT p.OrgUnitID, p.OrgUnitCode, p.OrgUnitNameTH, p.OrgUnitType, p.ParentOrgUnitID, c.Depth + 1
    FROM   chain c JOIN EMS.OrgUnit p ON p.OrgUnitID = c.ParentOrgUnitID
    WHERE  c.Depth < 10
)
SELECT * FROM chain ORDER BY Depth;

-- 2) Configured approvers with the HR name of each OrgCode
SELECT c.ScopeType, c.OrgCode,
       COALESCE(d.DepartmentName, v.DivisionName) AS HrOrgName,
       c.SortOrder, u.ADUserName, u.DisplayName
FROM   workflow.DownloadApproverConfig c
JOIN   EMS.UserAccount u ON u.UserID = c.ApproverUserID
LEFT JOIN EMS.m_Department d ON c.ScopeType = 'D' AND d.DepartmentCode = c.OrgCode
LEFT JOIN EMS.m_Division   v ON c.ScopeType = 'V' AND v.DivisionCode   = c.OrgCode
WHERE  c.IsActive = 1
ORDER BY c.ScopeType, c.OrgCode, c.SortOrder;
