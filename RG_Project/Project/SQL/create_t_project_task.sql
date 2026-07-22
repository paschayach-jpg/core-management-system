-- ============================================================
-- MOD-009: Task Management
-- Table: t_project_task, t_project_task_comment
-- ผูกกับ t_project_state (1 State : N Tasks)
-- Assignee ดึงจาก t_project_state_member
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name='t_project_task' AND xtype='U')
BEGIN
    CREATE TABLE t_project_task (
        i_task_id           INT             IDENTITY(1,1)   NOT NULL,
        c_task_code         VARCHAR(20)                     NOT NULL,   -- TK-26-00001
        i_proj_state_id     INT                             NOT NULL,   -- FK → t_project_state
        i_project_id        INT                             NOT NULL,   -- FK → t_project (denorm)
        i_parent_task_id    INT                             NULL,       -- Subtask FK → self
        i_subtask_depth     INT                             NOT NULL DEFAULT 0,  -- 0=root, max 3
        n_title             NVARCHAR(300)                   NOT NULL,
        n_description       NVARCHAR(MAX)                   NULL,
        i_priority          INT                             NOT NULL DEFAULT 2,
            -- 1=Low 2=Medium 3=High 4=Critical
        c_status            VARCHAR(20)                     NOT NULL DEFAULT 'Todo',
            -- Todo/InProgress/InReview/Done/Cancelled
        i_pct_complete      INT                             NOT NULL DEFAULT 0
            CONSTRAINT CK_t_project_task_pct CHECK (i_pct_complete BETWEEN 0 AND 100),
        n_assignee          NVARCHAR(200)                   NULL,       -- n_member_name จาก state_member
        d_due_date          DATE                            NULL,
        d_start_date        DATE                            NULL,
        i_estimated_hours   INT                             NULL,
        i_actual_hours      INT                             NULL,
        n_extend_reason     NVARCHAR(500)                   NULL,       -- เหตุผลขยายวัน
        f_active            CHAR(1)                         NOT NULL DEFAULT 'Y',
        d_deleted_date      DATETIME                        NULL,       -- Soft delete
        d_recycle_expiry    DATETIME                        NULL,       -- +30 วัน
        i_create_user       VARCHAR(20)                     NOT NULL,
        d_create_date       DATETIME                        NOT NULL DEFAULT GETDATE(),
        i_update_user       VARCHAR(20)                     NULL,
        d_update_date       DATETIME                        NULL,
        CONSTRAINT PK_t_project_task PRIMARY KEY (i_task_id),
        CONSTRAINT FK_t_project_task_state
            FOREIGN KEY (i_proj_state_id) REFERENCES t_project_state(i_proj_state_id),
        CONSTRAINT FK_t_project_task_proj
            FOREIGN KEY (i_project_id) REFERENCES t_project(i_project_id),
        CONSTRAINT FK_t_project_task_parent
            FOREIGN KEY (i_parent_task_id) REFERENCES t_project_task(i_task_id)
    );
    CREATE INDEX IX_t_project_task_state   ON t_project_task(i_proj_state_id);
    CREATE INDEX IX_t_project_task_project ON t_project_task(i_project_id);
    CREATE INDEX IX_t_project_task_status  ON t_project_task(c_status);
    CREATE INDEX IX_t_project_task_active  ON t_project_task(f_active, d_recycle_expiry);
    PRINT 'Created: t_project_task';
END
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name='t_project_task_comment' AND xtype='U')
BEGIN
    CREATE TABLE t_project_task_comment (
        i_comment_id        INT             IDENTITY(1,1)   NOT NULL,
        i_task_id           INT                             NOT NULL,   -- FK → t_project_task
        i_parent_comment_id INT                             NULL,       -- Threading FK → self
        i_thread_depth      INT                             NOT NULL DEFAULT 0,  -- max 3
        n_content           NVARCHAR(MAX)                   NOT NULL,
        f_is_edited         CHAR(1)                         NOT NULL DEFAULT 'N',
        f_is_deleted        CHAR(1)                         NOT NULL DEFAULT 'N', -- soft delete
        i_create_user       VARCHAR(20)                     NOT NULL,
        d_create_date       DATETIME                        NOT NULL DEFAULT GETDATE(),
        d_edited_date       DATETIME                        NULL,
        CONSTRAINT PK_t_project_task_comment PRIMARY KEY (i_comment_id),
        CONSTRAINT FK_t_project_task_comment_task
            FOREIGN KEY (i_task_id) REFERENCES t_project_task(i_task_id),
        CONSTRAINT FK_t_project_task_comment_parent
            FOREIGN KEY (i_parent_comment_id) REFERENCES t_project_task_comment(i_comment_id)
    );
    CREATE INDEX IX_t_project_task_comment_task ON t_project_task_comment(i_task_id);
    PRINT 'Created: t_project_task_comment';
END
GO

-- Running Number สำหรับ Task Code
IF NOT EXISTS (SELECT 1 FROM RunningNumber WHERE running_type = 'TK')
BEGIN
    INSERT INTO RunningNumber (running_type, prefix, format, digit_number, current_number)
    VALUES ('TK', 'TK', 'PPP-YY-######', 6, 0);
    PRINT 'Inserted: RunningNumber TK';
END
GO
