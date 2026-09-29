-- =============================================================================
-- MOD-006: Article Display Profile — header label/การซ่อนแสดงคอลัมน์ของ dgvArticles/
-- ArticleCardControl (KBMainForm) แบบ dynamic ตาม CategoryID แทนการ hardcode ตายตัว
--
-- 1 Profile ผูกกับได้หลาย Category ผ่าน KMS.m_KBCategory.DisplayProfileID — หมวดย่อยที่ไม่ได้
-- ผูก Profile ของตัวเองจะ inherit จาก ancestor ที่ใกล้ที่สุดที่มีค่านี้ตั้งไว้ (ไล่ ParentCategoryID
-- ขึ้นไป เหมือน pattern GetEffectiveLayoutType ที่ใช้แยกกฎหมาย/ประกาศ-คำสั่งอยู่แล้ว)
--
-- Non-destructive ทั้งหมด — รันซ้ำได้ปลอดภัย (guard ด้วย IF NOT EXISTS/OBJECT_ID ทุกจุด)
-- =============================================================================

IF OBJECT_ID('KMS.m_KBArticleDisplayProfile', 'U') IS NULL
BEGIN
    CREATE TABLE KMS.m_KBArticleDisplayProfile
    (
        ProfileID   INT           IDENTITY(1,1) NOT NULL,
        ProfileName NVARCHAR(200) NOT NULL,                                  -- เช่น "กฎหมาย", "คำเตือนภัย"
        Description NVARCHAR(500) NULL,
        IsActive    BIT           NOT NULL CONSTRAINT DF_KMS_ArtDispProfile_Act DEFAULT(1),
        CreatedBy   NVARCHAR(100) NOT NULL,
        CreatedDate DATETIME      NOT NULL CONSTRAINT DF_KMS_ArtDispProfile_Cdt DEFAULT(GETDATE()),
        CONSTRAINT PK_KMS_ArtDispProfile      PRIMARY KEY CLUSTERED (ProfileID ASC),
        CONSTRAINT UQ_KMS_ArtDispProfile_Name UNIQUE (ProfileName)
    );
END
GO

IF OBJECT_ID('KMS.m_KBArticleDisplayProfileColumn', 'U') IS NULL
BEGIN
    CREATE TABLE KMS.m_KBArticleDisplayProfileColumn
    (
        ProfileColumnID INT           IDENTITY(1,1) NOT NULL,
        ProfileID       INT           NOT NULL,                              -- FK -> KMS.m_KBArticleDisplayProfile
        SlotKey         NVARCHAR(50)  NOT NULL,                              -- Title/TitleCategory/ContentHTML/PublishedDate/EffectiveDate/ExpiryDate/LinkedEntities
        DisplayLabel    NVARCHAR(200) NOT NULL,
        IsVisible       BIT           NOT NULL CONSTRAINT DF_KMS_ArtDispProfCol_Vis DEFAULT(1),
        CONSTRAINT PK_KMS_ArtDispProfCol      PRIMARY KEY CLUSTERED (ProfileColumnID ASC),
        CONSTRAINT UQ_KMS_ArtDispProfCol_Slot UNIQUE (ProfileID, SlotKey),
        CONSTRAINT FK_KMS_ArtDispProfCol_Prof FOREIGN KEY (ProfileID) REFERENCES KMS.m_KBArticleDisplayProfile (ProfileID) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('KMS.m_KBCategory') AND name = 'DisplayProfileID'
)
BEGIN
    ALTER TABLE KMS.m_KBCategory ADD DisplayProfileID INT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_KMS_KBCat_DisplayProfile'
)
BEGIN
    ALTER TABLE KMS.m_KBCategory
        ADD CONSTRAINT FK_KMS_KBCat_DisplayProfile
        FOREIGN KEY (DisplayProfileID) REFERENCES KMS.m_KBArticleDisplayProfile (ProfileID);
END
GO
