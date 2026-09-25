-- =============================================================================
-- MOD-006: KMS Excel Import Template Config
-- Template สำหรับกำหนด mapping คอลัมน์ Excel -> คอลัมน์ปลายทางใน KMS.t_KBArticle
-- =============================================================================

IF OBJECT_ID('KMS.m_KBImportTemplateColumn', 'U') IS NOT NULL DROP TABLE KMS.m_KBImportTemplateColumn;
GO
IF OBJECT_ID('KMS.m_KBImportTemplate', 'U') IS NOT NULL DROP TABLE KMS.m_KBImportTemplate;
GO

CREATE TABLE KMS.m_KBImportTemplate
(
    TemplateID   INT IDENTITY(1,1) NOT NULL,
    TemplateName NVARCHAR(200)     NOT NULL,                                    -- เช่น "กฎหมาย (มาตรา)"
    Description  NVARCHAR(500)     NULL,
    SortOrder    INT               NOT NULL CONSTRAINT DF_KMS_ImpTpl_Sort DEFAULT(0),
    IsActive     BIT               NOT NULL CONSTRAINT DF_KMS_ImpTpl_Act  DEFAULT(1),
    CreatedBy    NVARCHAR(100)     NOT NULL,
    CreatedDate  DATETIME          NOT NULL CONSTRAINT DF_KMS_ImpTpl_Cdt  DEFAULT(GETDATE()),
    UpdatedBy    NVARCHAR(100)     NULL,
    UpdatedDate  DATETIME          NULL,
    CONSTRAINT PK_KMS_ImpTpl      PRIMARY KEY CLUSTERED (TemplateID ASC),
    CONSTRAINT UQ_KMS_ImpTpl_Name UNIQUE (TemplateName)
);
GO

CREATE TABLE KMS.m_KBImportTemplateColumn
(
    TemplateColumnID INT           IDENTITY(1,1) NOT NULL,
    TemplateID       INT           NOT NULL,                                    -- FK -> KMS.m_KBImportTemplate
    TargetColumnName NVARCHAR(128) NOT NULL,                                    -- ชื่อคอลัมน์จริงใน KMS.t_KBArticle
    ExcelColumnRef   NVARCHAR(200) NULL,                                        -- คอลัมน์/หัวข้อฝั่ง Excel เช่น "C" หรือ "หัวข้อเรื่อง"
    DisplayOrder     INT           NOT NULL CONSTRAINT DF_KMS_ImpTplCol_Ord DEFAULT(0),
    CONSTRAINT PK_KMS_ImpTplCol        PRIMARY KEY CLUSTERED (TemplateColumnID ASC),
    CONSTRAINT UQ_KMS_ImpTplCol_Target UNIQUE (TemplateID, TargetColumnName),
    CONSTRAINT FK_KMS_ImpTplCol_Tpl    FOREIGN KEY (TemplateID) REFERENCES KMS.m_KBImportTemplate (TemplateID) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX IX_KMS_ImpTplCol_Tpl ON KMS.m_KBImportTemplateColumn (TemplateID, DisplayOrder);
GO
