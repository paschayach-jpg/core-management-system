-- ============================================================
-- Project Registry Module - Database Schema
-- Database: Claim DB ('c' connection)
-- Created: 2026-02-23
-- ============================================================

-- ============================================================
-- MASTER TABLES
-- ============================================================

-- Master: Project Templates
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name='m_project_template' AND xtype='U')
BEGIN
    CREATE TABLE m_project_template (
        i_template_id   INT             IDENTITY(1,1)   NOT NULL,
        c_template_code VARCHAR(20)                     NOT NULL,
        n_template_name NVARCHAR(200)                   NOT NULL,
        c_description   NVARCHAR(500)                   NULL,
        f_active        CHAR(1)                         NOT NULL DEFAULT 'Y',
        i_create_user   VARCHAR(20)                     NOT NULL,
        d_create_date   DATETIME                        NOT NULL DEFAULT GETDATE(),
        i_update_user   VARCHAR(20)                     NULL,
        d_update_date   DATETIME                        NULL,
        CONSTRAINT PK_m_project_template PRIMARY KEY (i_template_id),
        CONSTRAINT UQ_m_project_template_code UNIQUE (c_template_code)
    );
    PRINT 'Created: m_project_template';
END

-- Master: States ของแต่ละ Template (ordered sequence)
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name='m_template_state' AND xtype='U')
BEGIN
    CREATE TABLE m_template_state (
        i_state_id      INT             IDENTITY(1,1)   NOT NULL,
        i_template_id   INT                             NOT NULL,
        n_state_name    NVARCHAR(200)                   NOT NULL,
        i_seq_no        INT                             NOT NULL,
        c_description   NVARCHAR(500)                   NULL,
        f_required      CHAR(1)                         NOT NULL DEFAULT 'Y',
        i_create_user   VARCHAR(20)                     NOT NULL,
        d_create_date   DATETIME                        NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_m_template_state PRIMARY KEY (i_state_id),
        CONSTRAINT FK_m_template_state_tmpl
            FOREIGN KEY (i_template_id) REFERENCES m_project_template(i_template_id)
    );
    CREATE INDEX IX_m_template_state_tmpl ON m_template_state(i_template_id, i_seq_no);
    PRINT 'Created: m_template_state';
END

-- ============================================================
-- TRANSACTIONAL TABLES
-- ============================================================

-- Transactional: Project หลัก
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name='t_project' AND xtype='U')
BEGIN
    CREATE TABLE t_project (
        i_project_id    INT             IDENTITY(1,1)   NOT NULL,
        c_project_no    VARCHAR(20)                     NOT NULL,
        n_project_name  NVARCHAR(300)                   NOT NULL,
        i_template_id   INT                             NULL,
        c_status        VARCHAR(20)                     NOT NULL DEFAULT 'Open',
        c_description   NVARCHAR(1000)                  NULL,
        d_start_date    DATE                            NULL,
        d_due_date      DATE                            NULL,
        d_close_date    DATETIME                        NULL,
        f_active        CHAR(1)                         NOT NULL DEFAULT 'Y',
        i_create_user   VARCHAR(20)                     NOT NULL,
        d_create_date   DATETIME                        NOT NULL DEFAULT GETDATE(),
        i_update_user   VARCHAR(20)                     NULL,
        d_update_date   DATETIME                        NULL,
        CONSTRAINT PK_t_project PRIMARY KEY (i_project_id),
        CONSTRAINT UQ_t_project_no UNIQUE (c_project_no),
        CONSTRAINT FK_t_project_tmpl
            FOREIGN KEY (i_template_id) REFERENCES m_project_template(i_template_id)
    );
    CREATE INDEX IX_t_project_status ON t_project(c_status);
    CREATE INDEX IX_t_project_create  ON t_project(d_create_date DESC);
    PRINT 'Created: t_project';
END

-- Transactional: States ต่อ Project (copy จาก Template + เพิ่มเองได้)
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name='t_project_state' AND xtype='U')
BEGIN
    CREATE TABLE t_project_state (
        i_proj_state_id INT             IDENTITY(1,1)   NOT NULL,
        i_project_id    INT                             NOT NULL,
        i_state_id      INT                             NULL,
        n_state_name    NVARCHAR(200)                   NOT NULL,
        i_seq_no        INT                             NOT NULL,
        c_description   NVARCHAR(500)                   NULL,
        f_required      CHAR(1)                         NOT NULL DEFAULT 'Y',
        c_status        VARCHAR(20)                     NOT NULL DEFAULT 'Pending',
        i_create_user   VARCHAR(20)                     NOT NULL,
        d_create_date   DATETIME                        NOT NULL DEFAULT GETDATE(),
        i_update_user   VARCHAR(20)                     NULL,
        d_update_date   DATETIME                        NULL,
        CONSTRAINT PK_t_project_state PRIMARY KEY (i_proj_state_id),
        CONSTRAINT FK_t_project_state_proj
            FOREIGN KEY (i_project_id) REFERENCES t_project(i_project_id),
        CONSTRAINT FK_t_project_state_tmpl
            FOREIGN KEY (i_state_id) REFERENCES m_template_state(i_state_id)
    );
    CREATE INDEX IX_t_project_state_proj ON t_project_state(i_project_id, i_seq_no);
    PRINT 'Created: t_project_state';
END

-- Transactional: Progress log ต่อ State (append-only, latest = current)
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name='t_project_progress' AND xtype='U')
BEGIN
    CREATE TABLE t_project_progress (
        i_progress_id   INT             IDENTITY(1,1)   NOT NULL,
        i_proj_state_id INT                             NOT NULL,
        i_project_id    INT                             NOT NULL,
        c_status        VARCHAR(20)                     NOT NULL,
        n_remark        NVARCHAR(1000)                  NULL,
        i_pct_complete  INT                             NOT NULL DEFAULT 0
            CONSTRAINT CK_t_project_progress_pct CHECK (i_pct_complete BETWEEN 0 AND 100),
        d_action_date   DATETIME                        NOT NULL DEFAULT GETDATE(),
        i_action_user   VARCHAR(20)                     NOT NULL,
        CONSTRAINT PK_t_project_progress PRIMARY KEY (i_progress_id),
        CONSTRAINT FK_t_project_progress_state
            FOREIGN KEY (i_proj_state_id) REFERENCES t_project_state(i_proj_state_id),
        CONSTRAINT FK_t_project_progress_proj
            FOREIGN KEY (i_project_id) REFERENCES t_project(i_project_id)
    );
    CREATE INDEX IX_t_project_progress_state
        ON t_project_progress(i_proj_state_id, d_action_date DESC);
    CREATE INDEX IX_t_project_progress_proj
        ON t_project_progress(i_project_id);
    PRINT 'Created: t_project_progress';
END

-- Transactional: Document Workflow ต่อ State
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name='t_project_doc_workflow' AND xtype='U')
BEGIN
    CREATE TABLE t_project_doc_workflow (
        i_doc_wf_id     INT             IDENTITY(1,1)   NOT NULL,
        i_proj_state_id INT                             NOT NULL,
        i_project_id    INT                             NOT NULL,
        n_doc_name      NVARCHAR(300)                   NOT NULL,
        c_doc_type      VARCHAR(50)                     NULL,
        f_required      CHAR(1)                         NOT NULL DEFAULT 'Y',
        c_doc_status    VARCHAR(20)                     NOT NULL DEFAULT 'Pending',
        n_remark        NVARCHAR(500)                   NULL,
        i_seq_no        INT                             NOT NULL DEFAULT 1,
        i_create_user   VARCHAR(20)                     NOT NULL,
        d_create_date   DATETIME                        NOT NULL DEFAULT GETDATE(),
        i_update_user   VARCHAR(20)                     NULL,
        d_update_date   DATETIME                        NULL,
        CONSTRAINT PK_t_project_doc_workflow PRIMARY KEY (i_doc_wf_id),
        CONSTRAINT FK_t_project_doc_wf_state
            FOREIGN KEY (i_proj_state_id) REFERENCES t_project_state(i_proj_state_id),
        CONSTRAINT FK_t_project_doc_wf_proj
            FOREIGN KEY (i_project_id) REFERENCES t_project(i_project_id)
    );
    CREATE INDEX IX_t_project_doc_wf_state ON t_project_doc_workflow(i_proj_state_id);
    PRINT 'Created: t_project_doc_workflow';
END

-- Transactional: E-Filing Request ต่อ Project State
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name='t_project_efiling_request' AND xtype='U')
BEGIN
    CREATE TABLE t_project_efiling_request (
        i_efiling_req_id INT            IDENTITY(1,1)   NOT NULL,
        i_proj_state_id  INT                            NOT NULL,
        i_project_id     INT                            NOT NULL,
        c_efiling_ref_no VARCHAR(100)                   NULL,
        n_doc_name       NVARCHAR(300)                  NOT NULL,
        c_request_status VARCHAR(20)                    NOT NULL DEFAULT 'Requested',
        d_request_date   DATETIME                       NOT NULL DEFAULT GETDATE(),
        d_receive_date   DATETIME                       NULL,
        n_remark         NVARCHAR(500)                  NULL,
        i_request_user   VARCHAR(20)                    NOT NULL,
        i_update_user    VARCHAR(20)                    NULL,
        d_update_date    DATETIME                       NULL,
        CONSTRAINT PK_t_project_efiling_request PRIMARY KEY (i_efiling_req_id),
        CONSTRAINT FK_t_project_efiling_state
            FOREIGN KEY (i_proj_state_id) REFERENCES t_project_state(i_proj_state_id),
        CONSTRAINT FK_t_project_efiling_proj
            FOREIGN KEY (i_project_id) REFERENCES t_project(i_project_id)
    );
    CREATE INDEX IX_t_project_efiling_proj ON t_project_efiling_request(i_project_id);
    PRINT 'Created: t_project_efiling_request';
END

-- ============================================================
-- RUNNING NUMBER
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM RunningNumber WHERE running_type = 'PRJ')
BEGIN
    INSERT INTO RunningNumber (running_type, prefix, format, digit_number, current_number)
    VALUES ('PRJ', 'PRJ', 'PPP-YYMM-######', 6, 0);
    PRINT 'Inserted: RunningNumber PRJ';
END

-- ============================================================
-- SEED DATA - ตัวอย่าง Template
-- ============================================================
DECLARE @tmpl_id INT;

IF NOT EXISTS (SELECT 1 FROM m_project_template WHERE c_template_code = 'TMPL001')
BEGIN
    INSERT INTO m_project_template (c_template_code, n_template_name, c_description, f_active, i_create_user)
    VALUES ('TMPL001', N'โครงการทั่วไป', N'Template สำหรับโครงการทั่วไป', 'Y', 'SYSTEM');
    SET @tmpl_id = SCOPE_IDENTITY();

    INSERT INTO m_template_state (i_template_id, n_state_name, i_seq_no, c_description, f_required, i_create_user)
    VALUES
        (@tmpl_id, N'วางแผน (Planning)',       1, N'กำหนดเป้าหมายและวางแผนงาน',      'Y', 'SYSTEM'),
        (@tmpl_id, N'ดำเนินการ (Execution)',   2, N'ลงมือทำงานตามแผน',               'Y', 'SYSTEM'),
        (@tmpl_id, N'ทดสอบ (Testing)',         3, N'ทดสอบผลงาน',                     'Y', 'SYSTEM'),
        (@tmpl_id, N'ส่งมอบ (Delivery)',       4, N'ส่งมอบงาน',                       'Y', 'SYSTEM'),
        (@tmpl_id, N'ปิดโครงการ (Closing)',    5, N'สรุปและปิดโครงการ',               'Y', 'SYSTEM');
    PRINT 'Inserted: Template TMPL001 with 5 states';
END

IF NOT EXISTS (SELECT 1 FROM m_project_template WHERE c_template_code = 'TMPL002')
BEGIN
    INSERT INTO m_project_template (c_template_code, n_template_name, c_description, f_active, i_create_user)
    VALUES ('TMPL002', N'โครงการเรียกร้องสินไหม', N'Template สำหรับโครงการเรียกร้องสินไหม', 'Y', 'SYSTEM');
    SET @tmpl_id = SCOPE_IDENTITY();

    INSERT INTO m_template_state (i_template_id, n_state_name, i_seq_no, c_description, f_required, i_create_user)
    VALUES
        (@tmpl_id, N'รับแจ้งเหตุ',             1, N'บันทึกและรับแจ้งเหตุการณ์',       'Y', 'SYSTEM'),
        (@tmpl_id, N'สำรวจความเสียหาย',        2, N'สำรวจและประเมินความเสียหาย',      'Y', 'SYSTEM'),
        (@tmpl_id, N'รวบรวมเอกสาร',           3, N'รวบรวมเอกสารหลักฐาน',            'Y', 'SYSTEM'),
        (@tmpl_id, N'พิจารณาอนุมัติ',          4, N'พิจารณาอนุมัติการจ่ายสินไหม',    'Y', 'SYSTEM'),
        (@tmpl_id, N'จ่ายสินไหม',             5, N'ดำเนินการจ่ายสินไหมทดแทน',       'Y', 'SYSTEM'),
        (@tmpl_id, N'ปิดงาน',                 6, N'ปิดงานและจัดเก็บเอกสาร',          'Y', 'SYSTEM');
    PRINT 'Inserted: Template TMPL002 with 6 states';
END

-- ============================================================
-- m_template_doc: Mapping Template State → Master Document
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name='m_template_doc' AND xtype='U')
BEGIN
    CREATE TABLE m_template_doc (
        i_template_doc_id  INT           IDENTITY(1,1)  NOT NULL,
        i_template_id      INT                          NOT NULL,  -- FK → m_project_template
        i_state_id         INT                          NOT NULL,  -- FK → m_template_state
        i_doc_id           INT                          NOT NULL,  -- Ref → BUI_RG.dbo.m_document.doc_id
        n_doc_name         NVARCHAR(300)                NOT NULL,  -- Snapshot จาก m_document.doc_name
        c_doc_type         VARCHAR(50)                  NULL,       -- Snapshot จาก m_document.doc_code
        f_required         CHAR(1)                      NOT NULL DEFAULT 'Y',
        i_seq_no           INT                          NOT NULL DEFAULT 1,
        n_remark           NVARCHAR(500)                NULL,
        i_create_user      VARCHAR(20)                  NULL,
        d_create_date      DATETIME                     NOT NULL DEFAULT GETDATE(),
        i_update_user      VARCHAR(20)                  NULL,
        d_update_date      DATETIME                     NULL,
        CONSTRAINT PK_m_template_doc       PRIMARY KEY (i_template_doc_id),
        CONSTRAINT FK_m_template_doc_tmpl  FOREIGN KEY (i_template_id)
                                           REFERENCES m_project_template(i_template_id),
        CONSTRAINT FK_m_template_doc_state FOREIGN KEY (i_state_id)
                                           REFERENCES m_template_state(i_state_id),
        CONSTRAINT UQ_m_template_doc       UNIQUE (i_state_id, i_doc_id)
    );
    CREATE INDEX IX_m_template_doc_state ON m_template_doc (i_state_id);
    CREATE INDEX IX_m_template_doc_tmpl  ON m_template_doc (i_template_id);
    PRINT 'Created: m_template_doc';
END

-- ============================================================
-- t_project_doc_workflow: เพิ่ม i_doc_id สำหรับ trace origin
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 't_project_doc_workflow' AND COLUMN_NAME = 'i_doc_id'
)
BEGIN
    ALTER TABLE t_project_doc_workflow
        ADD i_doc_id INT NULL;  -- NULL = manual entry, NOT NULL = copied from template
    PRINT 'Altered: t_project_doc_workflow added i_doc_id';
END

PRINT 'Database setup completed successfully.';
GO
