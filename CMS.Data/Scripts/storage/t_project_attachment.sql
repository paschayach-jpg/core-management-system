-- =============================================================================
-- storage.t_project_attachment
-- Link table: RGP project  <->  DMS file (storage.FileDocument)
-- Mirrors the KMS.t_KBArticleAttachment pattern but lives in the `storage` schema.
-- Created manually on the SCS database (this script is a checked-in reference only).
-- =============================================================================
IF OBJECT_ID('storage.t_project_attachment', 'U') IS NULL
BEGIN
    CREATE TABLE storage.t_project_attachment
    (
        i_attach_id   INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_t_project_attachment PRIMARY KEY,
        i_project_id  INT           NOT NULL,   -- = RGP.t_project.i_project_id
        i_file_id     INT           NOT NULL,   -- FK -> storage.FileDocument.FileID
        i_create_user NVARCHAR(100) NOT NULL,
        d_create_date DATETIME      NOT NULL
            CONSTRAINT DF_t_project_attachment_date DEFAULT (GETDATE()),
        CONSTRAINT UQ_t_project_attachment UNIQUE (i_project_id, i_file_id)
    );

    CREATE INDEX IX_t_project_attachment_project
        ON storage.t_project_attachment (i_project_id);
END
GO
