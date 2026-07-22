-- ============================================================
-- เพิ่ม column i_ref_project_id ใน t_project_state
-- เพื่อให้แต่ละขั้นตอนสามารถ ref โครงการอื่นได้ (1:1)
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_project_state' AND COLUMN_NAME = 'i_ref_project_id'
)
BEGIN
    ALTER TABLE t_project_state
        ADD i_ref_project_id INT NULL
        CONSTRAINT FK_t_project_state_ref_proj
            FOREIGN KEY REFERENCES t_project(i_project_id);
    PRINT 'Altered: t_project_state added i_ref_project_id';
END
GO
