# Menu Action Debug Checklist — CMS WinForms

ใช้ checklist นี้เมื่อบันทึก action key ใน Menu Management แล้วแต่เมนูไม่เปิด form

---

## สาเหตุที่พบบ่อย (เรียงตาม Priority)

### 1. ❗ ImportFile Registry ไม่มี action key (ROOT CAUSE ที่พบจริง)

`MDI_Container` ใช้ `FolderForm` จาก namespace `CMS.Presentation.Modules.ImportFile`  
ดูได้จาก `MDI_Container.cs` บรรทัดแรก: `using CMS.Presentation.Modules.ImportFile;`

มี **2 ชุดไฟล์คู่ขนาน** ที่ต้องอัปเดต **พร้อมกัน** ทุกครั้ง:

| ไฟล์ชุด DMS (ต้องอัปเดต) | ไฟล์ชุด ImportFile (ต้องอัปเดต) |
|--------------------------|--------------------------------|
| `Modules/DMS/MenuActions/MenuActionKeyCatalog.cs` | `Modules/ImportFile/MenuActions/MenuActionKeyCatalog.cs` |
| `Modules/DMS/MenuActions/MenuActionRegistry.cs`   | `Modules/ImportFile/MenuActions/MenuActionRegistry.cs`   |

**วิธีตรวจสอบ:**
```
grep -n "SLAReport" CMS.Presentation/Modules/ImportFile/MenuActions/MenuActionKeyCatalog.cs
grep -n "SLAReport" CMS.Presentation/Modules/ImportFile/MenuActions/MenuActionRegistry.cs
```
ถ้าไม่มีผลลัพธ์ → ต้องเพิ่ม (ดู template ด้านล่าง)

---

### 2. ❗ Services เป็น null — form ไม่เปิด แต่ไม่มี error

`OpenSLAReport()` ใน `MDI_Container.cs` ตรวจ `_workflowService == null || _slaCalculator == null`  
ถ้า null → แสดง MessageBox เตือน แล้ว return (form ไม่เปิด)

**วิธีตรวจสอบ:** ดูที่จุด MDI_Container ถูก `new` ขึ้นมา — ตรวจว่า `workflowService` และ `slaCalculator` ถูก pass เข้าไปหรือไม่

---

### 3. ❗ Action key ใน DB ไม่ตรง

ใน Menu Management UI ต้องตั้งค่า Action Key ให้ตรงกับ key ที่ลงทะเบียนไว้ใน Catalog

**วิธีตรวจสอบ:** เปิด Menu Management → เลือก menu item → ดู dropdown Action Key → ต้องเห็น key ที่ต้องการ (เช่น `SLAReport`)

ถ้า key ไม่ขึ้นใน dropdown → Catalog ยังไม่มี key นั้น

---

### 4. ❗ `RequestMdiAction` delegate ไม่ถูก wire

`TryRequestMdiAction` จะ return `false` ถ้า `context.RequestMdiAction == null`  
→ fallback: แสดง MessageBox แทนที่จะเปิด form

**วิธีตรวจสอบ:** ค้นหา `RequestMdiAction = ` ใน FolderForm ของ ImportFile  
ต้องมี `RequestMdiAction = OnMenuActionRequested`

---

## Template เพิ่ม action key ใหม่ (ต้องทำ 4 ไฟล์)

### ขั้นตอนที่ 1 — DMS Catalog + ImportFile Catalog (ทั้งสองไฟล์)

```csharp
// ใน BuiltInActionKeys array
"NewActionKey",

// ใน ResolveConfiguredActionKey
if (string.Equals(normalized, "newactionkeyform", StringComparison.OrdinalIgnoreCase))
    return "NewActionKey";
```

### ขั้นตอนที่ 2 — DMS Registry + ImportFile Registry (ทั้งสองไฟล์)

```csharp
case "NewActionKey":
    if (!TryRequestMdiAction(context, "NewActionKey"))
        MessageBox.Show("กรุณาเปิด ... ผ่าน MDI Container", "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
    return true;
```

### ขั้นตอนที่ 3 — MDI_Container.cs

```csharp
// ใน HandleFolderMenuActionRequested switch
case "NewActionKey":
    OpenNewActionKey();
    return true;

// method
public void OpenNewActionKey()
{
    if (_requiredService == null)
    {
        MessageBox.Show("Service is not available.", "...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }
    OpenChildForm(new NewActionKeyForm(_requiredService, _currentUser));
}
```

---

## Checklist ก่อน Test

- [ ] BuiltInActionKeys มี key ใหม่ใน **ทั้ง DMS และ ImportFile** Catalog
- [ ] ResolveConfiguredActionKey มี alias ทั้งใน **DMS และ ImportFile** Catalog
- [ ] switch case ใน **DMS และ ImportFile** Registry
- [ ] `case "..."` ใน `HandleFolderMenuActionRequested` ใน MDI_Container
- [ ] `Open...()` method ใน MDI_Container ตรวจ null service
- [ ] Build สำเร็จ (ไม่มี compile error)
- [ ] ใน Menu Management UI — Action Key dropdown มี key ใหม่ขึ้น
- [ ] คลิกเมนู → form เปิดได้จริง

---

## ข้อผิดพลาดที่เกิดขึ้นจริงใน session

**Session 2026-04-28:** เพิ่ม `SLAReport` ใน DMS files ครบ แต่ลืม ImportFile files  
→ form ไม่เปิด เพราะ MDI_Container ใช้ `ImportFile.FolderForm` ซึ่งใช้ ImportFile registry  
→ fix: เพิ่ม `WorkflowDelegation`, `SLAReport`, `WorkflowMonitor` ใน ImportFile Catalog + Registry  
→ เพิ่ม `#sym:` prefix handling ใน ImportFile Catalog's `NormalizeToken` (DMS มีแต่ ImportFile ไม่มี)
