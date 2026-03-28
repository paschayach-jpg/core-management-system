-- ============================================================
-- Script: 024_OrgUnit_ManyToMany_Relationship.sql
-- Purpose: Create Many-to-Many relationship between SubCommittee and Division
-- ============================================================

-- Create junction table for SubCommittee-Division relationship
CREATE TABLE core.OrgUnitRelationship (
    RelationshipID      INT IDENTITY(1,1)   NOT NULL,
    SubCommitteeID      INT                 NOT NULL,
    DivisionID          INT                 NOT NULL,
    IsActive            BIT                 NOT NULL    CONSTRAINT DF_core_OrgUnitRel_IsActive DEFAULT 1,
    CreatedBy           INT                 NOT NULL,
    CreatedDate         DATETIME            NOT NULL    CONSTRAINT DF_core_OrgUnitRel_CreatedDate DEFAULT GETDATE(),
    
    CONSTRAINT PK_core_OrgUnitRelationship  PRIMARY KEY (RelationshipID),
    CONSTRAINT UQ_core_OrgUnitRel_Unique    UNIQUE (SubCommitteeID, DivisionID),
    CONSTRAINT FK_core_OrgUnitRel_SubComm   FOREIGN KEY (SubCommitteeID) REFERENCES core.OrgUnit(OrgUnitID),
    CONSTRAINT FK_core_OrgUnitRel_Division  FOREIGN KEY (DivisionID) REFERENCES core.OrgUnit(OrgUnitID),
    
    -- Ensure SubCommitteeID points to SubCommittee (Type=1)
    CONSTRAINT CK_core_OrgUnitRel_SubComm   CHECK (
        EXISTS (SELECT 1 FROM core.OrgUnit WHERE OrgUnitID = SubCommitteeID AND OrgUnitType = 1)
    ),
    
    -- Ensure DivisionID points to Division (Type=2)  
    CONSTRAINT CK_core_OrgUnitRel_Division  CHECK (
        EXISTS (SELECT 1 FROM core.OrgUnit WHERE OrgUnitID = DivisionID AND OrgUnitType = 2)
    )
);
GO

-- Create indexes for performance
CREATE INDEX IX_core_OrgUnitRel_SubCommittee ON core.OrgUnitRelationship (SubCommitteeID);
CREATE INDEX IX_core_OrgUnitRel_Division ON core.OrgUnitRelationship (DivisionID);
GO

-- Create stored procedures for managing relationships

-- Get Divisions for a SubCommittee
CREATE PROCEDURE core.sp_GetDivisionsBySubCommittee
    @SubCommitteeID INT
AS
BEGIN
    SELECT d.*, r.RelationshipID
    FROM core.OrgUnit d
    INNER JOIN core.OrgUnitRelationship r ON d.OrgUnitID = r.DivisionID
    WHERE r.SubCommitteeID = @SubCommitteeID 
      AND r.IsActive = 1
      AND d.IsActive = 1 
      AND d.IsDeleted = 0
    ORDER BY d.OrgUnitNameTH;
END
GO

-- Get SubCommittees for a Division
CREATE PROCEDURE core.sp_GetSubCommitteesByDivision
    @DivisionID INT
AS
BEGIN
    SELECT sc.*, r.RelationshipID
    FROM core.OrgUnit sc
    INNER JOIN core.OrgUnitRelationship r ON sc.OrgUnitID = r.SubCommitteeID
    WHERE r.DivisionID = @DivisionID 
      AND r.IsActive = 1
      AND sc.IsActive = 1 
      AND sc.IsDeleted = 0
    ORDER BY sc.OrgUnitNameTH;
END
GO

-- Add relationship
CREATE PROCEDURE core.sp_AddOrgUnitRelationship
    @SubCommitteeID INT,
    @DivisionID INT,
    @CreatedBy INT
AS
BEGIN
    -- Check if relationship already exists
    IF EXISTS (SELECT 1 FROM core.OrgUnitRelationship 
               WHERE SubCommitteeID = @SubCommitteeID AND DivisionID = @DivisionID)
    BEGIN
        -- Reactivate if exists but inactive
        UPDATE core.OrgUnitRelationship 
        SET IsActive = 1, CreatedBy = @CreatedBy, CreatedDate = GETDATE()
        WHERE SubCommitteeID = @SubCommitteeID AND DivisionID = @DivisionID;
    END
    ELSE
    BEGIN
        -- Insert new relationship
        INSERT INTO core.OrgUnitRelationship (SubCommitteeID, DivisionID, CreatedBy)
        VALUES (@SubCommitteeID, @DivisionID, @CreatedBy);
    END
END
GO

-- Remove relationship
CREATE PROCEDURE core.sp_RemoveOrgUnitRelationship
    @SubCommitteeID INT,
    @DivisionID INT
AS
BEGIN
    UPDATE core.OrgUnitRelationship 
    SET IsActive = 0
    WHERE SubCommitteeID = @SubCommitteeID AND DivisionID = @DivisionID;
END
GO

PRINT 'OrgUnit Many-to-Many relationship tables and procedures created successfully.';