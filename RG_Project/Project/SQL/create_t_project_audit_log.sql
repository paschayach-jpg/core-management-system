-- ตาราง audit log สำหรับโมดูล Project (เทียบเท่า AuditEntry/AuditLog ของ CMS)
-- ใช้โดย RG_Project.Data.Repositories.AuditRepository (implements IAuditRepository)
CREATE TABLE t_project_audit_log (
    i_audit_id      INT           IDENTITY(1,1) PRIMARY KEY,
    i_user_id       NVARCHAR(100) NOT NULL,
    n_module        NVARCHAR(100) NOT NULL,
    n_action        NVARCHAR(100) NOT NULL,
    n_table_name    NVARCHAR(200) NULL,
    i_record_id     INT           NULL,
    i_result        TINYINT       NOT NULL, -- 1 = success, 2 = fail
    n_message       NVARCHAR(500) NULL,
    n_machine_name  NVARCHAR(200) NULL,
    d_action_date   DATETIME      NOT NULL DEFAULT GETDATE()
);
