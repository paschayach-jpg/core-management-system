-- ============================================================
-- Migration: เพิ่ม field สำหรับ Legal Dictionary
-- ============================================================

-- 1. flag ใน m_project_template
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('m_project_template') AND name = 'f_is_legal_dict'
)
    ALTER TABLE m_project_template ADD f_is_legal_dict CHAR(1) NOT NULL DEFAULT 'N';

-- 2. JSON payload ใน t_project
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('t_project') AND name = 'n_legal_dict_json'
)
    ALTER TABLE t_project ADD n_legal_dict_json NVARCHAR(MAX) NULL;
