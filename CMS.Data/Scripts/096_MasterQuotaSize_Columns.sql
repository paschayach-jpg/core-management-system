/*
    Adds master quota fields used by FolderQuotaConfigRepository.
    quota_size stores bytes. The total usage of the four DMS bucket folders
    (1HC, 2SC, 3NC, 4PC) must not exceed this value for the OrgUnit.
*/

IF COL_LENGTH('EMS.m_SubCommittee', 'quota_size') IS NULL
BEGIN
    ALTER TABLE EMS.m_SubCommittee
    ADD quota_size BIGINT NOT NULL CONSTRAINT DF_m_SubCommittee_quota_size DEFAULT (0);
END
GO

IF COL_LENGTH('EMS.m_Division', 'quota_size') IS NULL
BEGIN
    ALTER TABLE EMS.m_Division
    ADD quota_size BIGINT NOT NULL CONSTRAINT DF_HR_Division_quota_size DEFAULT (0);
END
GO

IF COL_LENGTH('EMS.m_Department', 'quota_size') IS NULL
BEGIN
    ALTER TABLE EMS.m_Department
    ADD quota_size BIGINT NOT NULL CONSTRAINT DF_HR_Department_quota_size DEFAULT (0);
END
GO

IF OBJECT_ID('dms.FolderQuotaConfig', 'U') IS NOT NULL
BEGIN
    UPDATE sc
    SET sc.quota_size = q.QuotaBytes
    FROM EMS.m_SubCommittee sc
    INNER JOIN dms.FolderQuotaConfig q ON q.OrgUnitID = sc.OrgUnitID
    WHERE q.IsActive = 1;

    UPDATE d
    SET d.quota_size = q.QuotaBytes
    FROM EMS.m_Division d
    INNER JOIN EMS.OrgUnit o ON o.OrgUnitCode = d.DivisionCode AND o.OrgUnitType = 2
    INNER JOIN dms.FolderQuotaConfig q ON q.OrgUnitID = o.OrgUnitID
    WHERE q.IsActive = 1;

    UPDATE dep
    SET dep.quota_size = q.QuotaBytes
    FROM EMS.m_Department dep
    INNER JOIN EMS.OrgUnit o ON o.OrgUnitCode = dep.DepartmentCode AND o.OrgUnitType = 3
    INNER JOIN dms.FolderQuotaConfig q ON q.OrgUnitID = o.OrgUnitID
    WHERE q.IsActive = 1;
END
GO

