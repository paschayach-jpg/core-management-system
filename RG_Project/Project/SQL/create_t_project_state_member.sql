CREATE TABLE t_project_state_member (
    i_state_member_id INT           IDENTITY(1,1) PRIMARY KEY,
    i_proj_state_id   INT           NOT NULL,
    i_project_id      INT           NOT NULL,
    n_member_name     NVARCHAR(200) NOT NULL,
    c_role            NVARCHAR(100) NOT NULL,
    i_create_user     NVARCHAR(100) NULL,
    d_create_date     DATETIME      NULL DEFAULT GETDATE(),
    CONSTRAINT FK_state_member_state FOREIGN KEY (i_proj_state_id)
        REFERENCES t_project_state(i_proj_state_id)
);
