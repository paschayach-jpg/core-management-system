-- เพิ่มคอลัมน์ DepartmentCode ใน t_project_member
ALTER TABLE t_project_member
ADD c_department_code NVARCHAR(10) NULL;
