-- 1. ตารางเก็บ log การเปลี่ยนสถานะ Task (ใครเปลี่ยน, จากอะไรเป็นอะไร, % ตอนเปลี่ยน)
CREATE TABLE RGP.t_project_task_status_log (
    i_log_id        INT IDENTITY(1,1) PRIMARY KEY,
    i_task_id       INT NOT NULL,
    c_old_status    NVARCHAR(20) NULL,
    c_new_status    NVARCHAR(20) NOT NULL,
    i_pct_at_change INT NOT NULL,
    i_change_user   NVARCHAR(100) NULL,
    d_change_date   DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_task_status_log_task FOREIGN KEY (i_task_id)
        REFERENCES RGP.t_project_task (i_task_id)
);

-- 2. เปลี่ยนสถานะเดิม 'Cancelled' ที่มีอยู่แล้วในข้อมูลจริง ให้เป็น 'OnHold' ตามชื่อสถานะใหม่
UPDATE RGP.t_project_task SET c_status = 'OnHold' WHERE c_status = 'Cancelled';
