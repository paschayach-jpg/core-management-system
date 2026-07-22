# แผนการปรับปรุง Project Module ให้เป็นรูปแบบเดียวกับ TB_Repair

## สรุปความแตกต่างหลัก

| หัวข้อ | TB_Repair (ต้นแบบ) | Project (ปัจจุบัน) | ต้องแก้ไข |
|---|---|---|---|
| Header คอลัมน์ | ดึงจาก DB ผ่าน `setHeaders(code, subcode)` | Hardcode บางส่วน, fallback ยังมี | ✅ ทำแล้วบางส่วน |
| Column mapping | `MapColumnHeaders` + `ApplyGridSettingsFromHeaders` | ทำแล้วใน Frm_Project_List | ✅ ทำแล้ว |
| IClaimService | ใช้ `ClaimServiceFactory.Create()` | เพิ่มแล้วใน Frm_Project_List | ✅ ทำแล้ว |
| code/subcode | อ่านจาก `AppSettings` | Hardcode ใน code | ✅ ทำแล้ว |
| Filter ComboBox | โหลดจากข้อมูลใน DataTable (distinct) | ใช้ hardcode items | ✅ ทำแล้ว |
| RowFilter | ใช้ `DefaultView.RowFilter` | ใช้ Search() ใหม่ทุกครั้ง | ✅ ทำแล้ว |
| Export Excel | มี `ExportToExcel` | ยังไม่มี | ✅ ทำแล้ว |
| Actions/Permission | ใช้ `GlobalData.MenuActions` | ยังไม่มี | ✅ ทำแล้ว |
| Row Header เลขรัน | ใช้ `dgv1_DataBindingComplete` | ลบออกแล้ว | ✅ ทำแล้ว |
| txtEClaim filter | filter ใน DefaultView | ยังไม่มี | ✅ ทำแล้ว |

---

## 1. ตารางใน DB ที่ต้องเพิ่ม/แก้ไข

### 1.1 ตาราง Header (มีอยู่แล้วในระบบ)
ต้องเพิ่มข้อมูล header สำหรับ Project ใน table ที่ `setHeaders` ใช้

```sql
-- ตรวจสอบ table ที่ setHeaders ใช้ (ปกติคือ m_header หรือ clm_header)
-- เพิ่ม rows สำหรับ code='PJ001', subcode='PJ001_1'

INSERT INTO [table_header] (code_id, sub_code_id, header_db, header_name, col_width, col_visible, col_order, col_format, col_align)
VALUES
  ('PJ001', 'PJ001_1', 'c_project_no',    'เลขที่โครงการ',    145, 1, 1,  '',             'L'),
  ('PJ001', 'PJ001_1', 'n_project_name',  'ชื่อโครงการ',      255, 1, 2,  '',             'L'),
  ('PJ001', 'PJ001_1', 'n_template_name', 'Template',         175, 1, 3,  '',             'L'),
  ('PJ001', 'PJ001_1', 'c_status',        'สถานะ',            108, 1, 4,  '',             'C'),
  ('PJ001', 'PJ001_1', 'i_pct_overall',   '% เสร็จ',           75, 1, 5,  '',             'C'),
  ('PJ001', 'PJ001_1', 'd_start_date',    'วันที่เริ่ม',       115, 1, 6,  'dd/MM/yyyy',  'C'),
  ('PJ001', 'PJ001_1', 'd_create_date',   'วันที่สร้าง',      125, 1, 7,  'dd/MM/yyyy',  'C'),
  ('PJ001', 'PJ001_1', 'i_create_user',   'สร้างโดย',          95, 1, 8,  '',             'L'),
  ('PJ001', 'PJ001_1', 'i_update_user',   'ผู้แก้ไขล่าสุด',   110, 1, 9,  '',             'L'),
  ('PJ001', 'PJ001_1', 'd_update_date',   'แก้ไขล่าสุด',      125, 1, 10, 'dd/MM/yyyy HH:mm', 'C'),
  ('PJ001', 'PJ001_1', 'i_project_id',    '',                   0,  0, 0,  '',             'L');
-- หมายเหตุ: ชื่อ table และ column จริงต้องตรวจสอบจาก setHeaders ใน ClaimModelLibrary.cs
```

### 1.2 ตาราง code/subcode (มีอยู่แล้วในระบบ)
```sql
-- เพิ่ม code สำหรับ Project ใน table ที่ getcode/getSubcode ใช้
INSERT INTO [table_code] (code_id, sub_code_id, sub_code_name, menu_code, seq)
VALUES
  ('PJ001', 'PJ001_1', 'โครงการทั้งหมด', 'bis-pj-001', 1);
```

### 1.3 ตาราง Menu/Actions (ถ้าต้องการ Permission)
```sql
-- เพิ่ม menu สำหรับ Project
INSERT INTO [table_menu] (menu_code, menu_desc, action_desc)
VALUES
  ('bis-pj-001', 'จัดการโครงการ', 'View'),
  ('bis-pj-001', 'จัดการโครงการ', 'Entry'),
  ('bis-pj-001', 'จัดการโครงการ', 'Export');
```

---

## 2. แก้ไข App.config

```xml
<!-- เพิ่มใน appSettings -->
<add key="code_project" value="PJ001" />
```

แล้วแก้ใน `Frm_Project_List.cs`:
```csharp
// จาก
private const string _code    = "PJ001";
private const string _subcode = "PJ001_1";

// เป็น
private readonly string _code    = ConfigurationManager.AppSettings["code_project"];
private readonly string _subcode; // โหลดจาก getSubcode() เหมือน TB_Repair
```

---

## 3. แก้ไข Frm_Project_List.cs

### 3.1 เพิ่ม Filter ComboBox (distinct จากข้อมูล)
เหมือน TB_Repair ที่โหลด ComboBox จากข้อมูลจริงใน DataTable:

```csharp
private void LoadFilterCombos(DataTable dt)
{
    if (dt.Rows.Count == 0) return;

    // cbbStatus — distinct จากข้อมูล
    var statusRows = dt.AsEnumerable()
        .GroupBy(r => r.Field<string>("c_status"))
        .Select(g => g.First())
        .OrderBy(r => r.Field<string>("c_status"))
        .CopyToDataTable();
    DataRow blankStatus = statusRows.NewRow();
    blankStatus["c_status"] = "--- ทุกสถานะ ---";
    statusRows.Rows.InsertAt(blankStatus, 0);
    // bind ไปยัง cbbStatus
}
```

### 3.2 เปลี่ยน Filter ให้ใช้ DefaultView.RowFilter แทน Search()
```csharp
private void Filter()
{
    var filters = new List<string>();

    if (cbbStatus.SelectedIndex != 0)
        filters.Add($"c_status = '{cbbStatus.SelectedValue}'");

    if (cbbTemplate.SelectedValue != null && cbbTemplate.SelectedValue != DBNull.Value)
        filters.Add($"i_template_id = {cbbTemplate.SelectedValue}");

    (dgvProjects.DataSource as DataTable).DefaultView.RowFilter = string.Join(" AND ", filters);
    txtTotal.Text = dgvProjects.Rows.Count.ToString("#,##0");
}
```

### 3.3 เพิ่ม txtSearch filter (เหมือน txtEClaim ใน TB_Repair)
```csharp
private void txtSearch_TextChanged(object sender, EventArgs e)
{
    if (dgvProjects.DataSource == null) return;
    string val = txtSearch.Text.Trim();
    (dgvProjects.DataSource as DataTable).DefaultView.RowFilter =
        string.IsNullOrEmpty(val) ? "" : $"c_project_no LIKE '%{val}%' OR n_project_name LIKE '%{val}%'";
    txtTotal.Text = dgvProjects.Rows.Count.ToString("#,##0");
}
```

### 3.4 เพิ่ม Export Excel
```csharp
private void btnExcel_Click(object sender, EventArgs e)
{
    _claimService.ExportDataGridViewToExcel(dgvProjects, _code, _subcode);
}
```

### 3.5 เพิ่ม Permission Control (ถ้าต้องการ)
```csharp
private void SetControlActions()
{
    var actions = GlobalData.MenuActions.Where(x => x.MenuCode == "bis-pj-001").ToList();
    foreach (var action in actions)
    {
        if (action.ActionDesc == "View")  { btnSearch.Enabled = true; }
        if (action.ActionDesc == "Entry") { btnAdd.Enabled = true; btnEdit.Enabled = true; btnDelete.Enabled = true; }
        if (action.ActionDesc == "Export") { btnExcel.Enabled = true; }
    }
}
```

---

## 4. สรุปลำดับการทำงาน

1. **ตรวจสอบชื่อ table จริง** ที่ `setHeaders` ใช้ใน `ClaimModelLibrary.cs` → `ClaimModelLibrary.setHeaders()`
2. **เพิ่มข้อมูล header** ใน DB สำหรับ `code='PJ001'`, `subcode='PJ001_1'`
3. **เพิ่ม code/subcode** ใน DB
4. **เพิ่ม key ใน App.config** (`code_project`)
5. **แก้ Frm_Project_List.cs** ตามข้อ 3.1–3.5
6. **ทดสอบ** ว่า header แสดงถูกต้อง

---

## 5. สิ่งที่ทำแล้ว (ไม่ต้องทำซ้ำ)

- ✅ `IClaimService` + `ClaimServiceFactory` ใน Frm_Project_List
- ✅ `setHeaders` + `MapColumnHeaders` + `ApplyGridSettingsFromHeaders`
- ✅ Hidden column `i_project_id`
- ✅ `GetSelectedProjectId()` อ่านจาก DataView
- ✅ ลบ running number column ออก
- ✅ `cbbStatus` + `cbbTemplate` wire event หลัง Load
- ✅ ตาราง `t_project_member`, `m_project_member_role`
- ✅ `i_estimated_days` ใน `t_project_state`
