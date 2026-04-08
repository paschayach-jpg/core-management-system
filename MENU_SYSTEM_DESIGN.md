# ระบบจัดการเมนู (Menu System Design)

## 📋 ภาพรวม

ระบบจัดการเมนูแบบพลวัตสำหรับแสดงใน `_treeMainMenu` ของ CMS ที่สนับสนุน:
- ✓ เก็บข้อมูลเมนูในฐานข้อมูล (Database-driven)
- ✓ โครงสร้างลำดับชั้น (Hierarchical structure)
- ✓ การควบคุมตามบทบาท (Role-based visibility)
- ✓ CRUD operations ที่สมบูรณ์
- ✓ Audit logging สำหรับการเปลี่ยนแปลงเมนู

---

## 🏗️ สถาปัตยกรรมของระบบ

### ชั้นต่างๆ ของระบบ

```
┌─────────────────────────────────────┐
│   UI Layer - TreeView Manager       │  MenuTreeViewManager.cs
│   (TreeView presentation)           │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Service Layer - Menu Service      │  MenuService.cs
│   (Business logic & validation)     │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Repository Layer                  │  MenuItemRepository.cs
│   (Data access & persistence)       │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Database Layer                    │  Menu_Schema_Migration.sql
│   (core.MenuItem table)             │
└─────────────────────────────────────┘
```

---

## 📁 ไฟล์ที่เกี่ยวข้อง

| ไฟล์ |용途 | ตำแหน่ง |
|------|------|---------|
| MenuItem.cs | Model สำหรับเก็บข้อมูลเมนู | CMS.Domain/Models/ |
| IMenuItemRepository.cs | Interface ของ Repository | CMS.Data/Interfaces/ |
| MenuItemRepository.cs | Implementation ของ Repository | CMS.Data/Repositories/ |
| IMenuService.cs | Interface ของ Service | CMS.Services/Interfaces/ |
| MenuService.cs | Implementation ของ Service | CMS.Services/ |
| MenuTreeViewManager.cs | TreeView UI Manager | CMS.Presentation/ |
| Menu_Schema_Migration.sql | ฐานข้อมูลและ Stored Procedures | DB_Scripts/ |

---

## 🗄️ โครงสร้างฐานข้อมูล

### ตาราง: core.MenuItem

```sql
CREATE TABLE core.MenuItem
(
    MenuItemID            INT PRIMARY KEY IDENTITY(1,1),
    ParentMenuItemID      INT NULL,              -- ID ของเมนูแม่ (null = root)
    MenuName              NVARCHAR(255) NOT NULL, -- ชื่อเมนู (เช่น "จัดการไฟล์")
    MenuKey               NVARCHAR(100) UNIQUE,  -- Key สำหรับการเรียก (เช่น "FILE_MGMT")
    FormClassName         NVARCHAR(500),        -- ชื่อ class ของ Form (เช่น "CMS.Presentation.Modules.FileForm")
    IconKey               NVARCHAR(100),        -- Key ของไอคอน
    DisplayOrder          INT DEFAULT 0,        -- ลำดับการแสดงผล (น้อย = แสดงบน)
    IsActive              BIT DEFAULT 1,        -- ว่าใช้งานอยู่หรือไม่
    RequiredAppRole       INT NULL,             -- บทบาทที่ต้อง (1=User, 2=Manager, 3=Approver, 4=Admin)
    Description           NVARCHAR(500),        -- คำบรรยายเมนู (tooltip)
    ModuleCode            NVARCHAR(50),         -- รหัสโมดูล (สำหรับ audit logging)
    Url                   NVARCHAR(500),        -- URL สำหรับเมนูเว็บ
    CreatedDate           DATETIME DEFAULT GETDATE(),
    CreatedByUserID       INT NOT NULL,         -- ผู้สร้าง
    ModifiedDate          DATETIME,
    ModifiedByUserID      INT,                  -- ผู้แก้ไข
    CONSTRAINT FK_MenuItem_Parent FOREIGN KEY (ParentMenuItemID) 
        REFERENCES core.MenuItem(MenuItemID) ON DELETE CASCADE
);
```

### ดัชนี (Indexes)

```sql
CREATE INDEX IX_MenuItem_ParentID ON core.MenuItem(ParentMenuItemID);
CREATE INDEX IX_MenuItem_MenuKey ON core.MenuItem(MenuKey);
CREATE INDEX IX_MenuItem_IsActive ON core.MenuItem(IsActive);
CREATE INDEX IX_MenuItem_DisplayOrder ON core.MenuItem(DisplayOrder);
CREATE INDEX IX_MenuItem_RequiredRole ON core.MenuItem(RequiredAppRole);
```

---

## 🔧 ข้อมูลเมนูตัวอย่าง

| MenuItemID | ParentID | MenuName | MenuKey | DisplayOrder | RequiredAppRole |
|---|---|---|---|---|---|
| 1 | NULL | หน้าแรก | HOME | 1 | 1 |
| 2 | NULL | จัดการไฟล์ | FILE_MANAGEMENT | 2 | 1 |
| 3 | NULL | จัดการผู้ใช้ | USER_MANAGEMENT | 3 | 4 |
| 4 | NULL | บันทึกการตรวจสอบ | AUDIT_LOG | 4 | 4 |
| 5 | 2 | อัพโหลดไฟล์ | FILE_UPLOAD | 1 | 1 |
| 6 | 2 | ตรวจสอบไฟล์ | FILE_REVIEW | 2 | 2 |

---

## 💻 ตัวอย่างการใช้งาน

### 1️⃣ ตั้งค่า (Setup) ใน MDI_Container

```csharp
private MenuTreeViewManager _menuTreeManager;
private IMenuService _menuService;
private IMenuItemRepository _menuRepository;

// ใน Constructor หรือ Load event
private void InitializeMenuSystem()
{
    // Inject dependencies (สมมติว่ามี DI container)
    _menuRepository = new MenuItemRepository(_configManager);
    _menuService = new MenuService(_menuRepository, _logger, _auditService);
    
    // สร้าง TreeView Manager
    _menuTreeManager = new MenuTreeViewManager(_treeMainMenu, _menuService, _currentUser);
    
    // Subscribe to menu selection events
    _menuTreeManager.MenuSelected += MenuTree_MenuSelected;
    
    // โหลดเมนู
    _menuTreeManager.PopulateMenuTree();
}
```

### 2️⃣ จัดการการเลือกเมนู

```csharp
private void MenuTree_MenuSelected(object sender, MenuSelectedEventArgs e)
{
    var selectedMenu = e.SelectedMenu;
    
    if (e.IsDoubleClick && !string.IsNullOrEmpty(selectedMenu.FormClassName))
    {
        // เปิด Form ตาม FormClassName
        OpenModuleForm(selectedMenu);
    }
}

private void OpenModuleForm(MenuItem menu)
{
    try
    {
        // Dynamically load form class
        Type formType = Type.GetType(menu.FormClassName);
        if (formType != null && typeof(Form).IsAssignableFrom(formType))
        {
            var form = (Form)Activator.CreateInstance(formType, _currentUser, _serviceContainer);
            form.MdiParent = this;
            form.Show();
            
            // Audit log
            _auditService.LogAction(
                userID: _currentUser.UserID,
                action: "OPEN_MENU",
                resource: menu.MenuKey,
                module: menu.ModuleCode,
                details: $"Opened module: {menu.MenuName}"
            );
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"ไม่สามารถเปิดโมดูล: {ex.Message}", "Error");
    }
}
```

### 3️⃣ Refresh เมนู (เช่นเมื่ออัปเดตสิทธิ์)

```csharp
public void RefreshUserMenuAccess()
{
    // เมื่อสิทธิ์ของผู้ใช้เปลี่ยนแปลง
    _menuTreeManager.RefreshMenuTree();
}
```

### 4️⃣ การสร้างเมนูใหม่ (Admin only)

```csharp
public void CreateNewMenu(string menuName, string menuKey, int? parentID, int displayOrder)
{
    if (_currentUser.AppRole != 4) // Not Admin
    {
        MessageBox.Show("เฉพาะผู้ดูแลระบบเท่านั้นที่สามารถสร้างเมนูได้");
        return;
    }
    
    var newMenu = new MenuItem
    {
        ParentMenuItemID = parentID,
        MenuName = menuName,
        MenuKey = menuKey,
        DisplayOrder = displayOrder,
        IsActive = true,
        RequiredAppRole = 1, // All users
        CreatedByUserID = _currentUser.UserID
    };
    
    try
    {
        int newMenuID = _menuService.CreateMenu(newMenu, _currentUser);
        MessageBox.Show($"สร้างเมนูสำเร็จ (ID: {newMenuID})", "Success");
        
        // Refresh tree
        _menuTreeManager.RefreshMenuTree();
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}", "Error");
    }
}
```

### 5️⃣ อัปเดตข้อมูลเมนู

```csharp
public void UpdateMenuDetails(MenuItem menu, string newName, int newDisplayOrder)
{
    if (_currentUser.AppRole != 4) // Not Admin
    {
        MessageBox.Show("เฉพาะผู้ดูแลระบบเท่านั้นที่สามารถแก้ไขเมนูได้");
        return;
    }
    
    menu.MenuName = newName;
    menu.DisplayOrder = newDisplayOrder;
    
    try
    {
        _menuService.UpdateMenu(menu, _currentUser);
        MessageBox.Show("อัปเดตเมนูสำเร็จ", "Success");
        _menuTreeManager.RefreshMenuTree();
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}", "Error");
    }
}
```

### 6️⃣ ลบเมนู

```csharp
public void DeleteMenu(int menuItemID)
{
    if (_currentUser.AppRole != 4)
    {
        MessageBox.Show("เฉพาะผู้ดูแลระบบเท่านั้นที่สามารถลบเมนูได้");
        return;
    }
    
    if (MessageBox.Show("ยืนยันการลบเมนูนี้?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
    {
        try
        {
            _menuService.DeleteMenu(menuItemID, _currentUser);
            MessageBox.Show("ลบเมนูสำเร็จ", "Success");
            _menuTreeManager.RefreshMenuTree();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error");
        }
    }
}
```

---

## 🔐 การควบคุมตามบทบาท (Role-Based Access)

### AppRole Values
- **1** = User (ผู้ใช้ทั่วไป)
- **2** = Manager (ผู้จัดการ)
- **3** = Approver (ผู้อนุมัติ)
- **4** = Admin (ผู้ดูแลระบบ)

### ตัวอย่างการตั้งค่าสิทธิ์

```csharp
// เมนูสำหรับทุกบทบาท
var publicMenu = new MenuItem 
{ 
    MenuName = "หน้าแรก", 
    RequiredAppRole = null  // null = ทุกคนเห็น
};

// เมนูสำหรับ Manager และสูงขึ้น
var managerMenu = new MenuItem 
{ 
    MenuName = "รายงาน", 
    RequiredAppRole = 2  // 2 = Manager
};

// เมนูสำหรับ Admin เท่านั้น
var adminMenu = new MenuItem 
{ 
    MenuName = "จัดการผู้ใช้", 
    RequiredAppRole = 4  // 4 = Admin
};
```

---

## 📝 StoredProcedures

### sp_CreateMenuItem
```sql
EXEC core.sp_CreateMenuItem
    @MenuName = 'จัดการไฟล์',
    @MenuKey = 'FILE_MGMT',
    @FormClassName = 'CMS.Presentation.Modules.FileForm',
    @DisplayOrder = 2,
    @RequiredAppRole = 1,
    @CreatedByUserID = 1;
```

### sp_UpdateMenuItem
```sql
EXEC core.sp_UpdateMenuItem
    @MenuItemID = 5,
    @MenuName = 'جديد اسم الطلب',
    @IsActive = 1,
    @ModifiedByUserID = 1;
```

### sp_DeleteMenuItem
```sql
EXEC core.sp_DeleteMenuItem @MenuItemID = 5;
```

---

## 🎯 Dependency Injection Setup

```csharp
// ใน DI Container หรือ Program.cs
services.AddScoped<IMenuItemRepository, MenuItemRepository>();
services.AddScoped<IMenuService, MenuService>();
services.AddTransient<MenuTreeViewManager>();
```

---

## 📊 Audit Logging

ระบบเมนูบันทึกการเปลี่ยนแปลงทั้งหมด:

```
Action: CREATE_MENU
Resource: MenuItem:5
Module: MenuManagement
Details: Created menu: File Management
Timestamp: 2026-04-08 10:30:45
User: Admin (ID: 1)

Action: UPDATE_MENU
Resource: MenuItem:5
Module: MenuManagement
...

Action: DELETE_MENU
Resource: MenuItem:5
...
```

---

## ⚙️ ขั้นตอนการติดตั้ง

### 1. สร้างตาราง

```sql
-- รัน DB_Scripts/Menu_Schema_Migration.sql
sqlcmd -S your_server -d SCS -i Menu_Schema_Migration.sql
```

### 2. เพิ่ม Models ไปยัง CMS.Domain

- [MenuItem.cs](../CMS.Domain/Models/MenuItem.cs)

### 3. เพิ่ม Repository ไปยัง CMS.Data

- [IMenuItemRepository.cs](../CMS.Data/Interfaces/IMenuItemRepository.cs)
- [MenuItemRepository.cs](../CMS.Data/Repositories/MenuItemRepository.cs)

### 4. เพิ่ม Service ไปยัง CMS.Services

- [IMenuService.cs](../CMS.Services/Interfaces/IMenuService.cs)
- [MenuService.cs](../CMS.Services/MenuService.cs)

### 5. เพิ่ม UI Manager ไปยัง CMS.Presentation

- [MenuTreeViewManager.cs](../CMS.Presentation/MenuTreeViewManager.cs)

### 6. ตั้งค่า MDI_Container

```csharp
// ใน MDI_Container.cs
private void InitializeMenuSystem()
{
    // ... (ดู section ตัวอย่างการใช้งาน)
}
```

---

## 🧪 ทดสอบระบบ

### Query ทดสอบ

```sql
-- ดูเมนูทั้งหมด
SELECT * FROM core.MenuItem ORDER BY DisplayOrder;

-- ดูเมนูที่ active
SELECT * FROM core.MenuItem WHERE IsActive = 1;

-- ดูเมนู for role 2 (Manager)
SELECT * FROM core.MenuItem 
WHERE IsActive = 1 AND (RequiredAppRole IS NULL OR RequiredAppRole <= 2)
ORDER BY DisplayOrder;

-- นับเมนูตามบทบาท
SELECT 
    COUNT(*) AS TotalMenus,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveMenus,
    SUM(CASE WHEN RequiredAppRole = 4 THEN 1 ELSE 0 END) AS AdminOnly
FROM core.MenuItem;
```

---

## 🚀 ฟีเจอร์ขั้นสูง

### Dynamic Form Loading

```csharp
private Form LoadFormByClassName(string className, UserContext user)
{
    try
    {
        Type type = Type.GetType(className);
        if (type != null && typeof(Form).IsAssignableFrom(type))
        {
            // สมมติว่ามี Constructor ที่ยอมรับ UserContext และ service container
            return (Form)Activator.CreateInstance(type, user, _serviceContainer);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Cannot load form: {ex.Message}");
    }
    return null;
}
```

### Menu Caching

```csharp
private Dictionary<int, MenuItem> _menuCache = new Dictionary<int, MenuItem>();

public void CacheMenuItems()
{
    var allMenus = _menuService.GetAllMenusForManagement(_currentUser);
    foreach (var menu in allMenus)
    {
        _menuCache[menu.MenuItemID] = menu;
    }
}
```

---

## 📞 การแก้ไขปัญหา

| ปัญหา | สาเหตุ | วิธีแก้ |
|------|--------|--------|
| เมนูไม่ปรากฏ | IsActive = 0 หรือ RequiredAppRole สูง | ตรวจสอบเงื่อนไขในฐานข้อมูล |
| Form ไม่เปิด | FormClassName ผิดหรือ Class ไม่มี | ตรวจสอบชื่อ class ว่าถูกต้อง |
| Null Reference Exception | Menu ที่เลือกเป็น null | ตรวจสอบว่ามีเมนูอยู่ |
| Permission Denied | User role ต่ำ | ขอให้ Admin เปลี่ยนสิทธิ์ |

---

## 📚 รูปแบบการใช้งาน

```csharp
// Pattern 1: Show menu tree
_menuTreeManager.PopulateMenuTree();

// Pattern 2: Handle selection
var selectedMenu = _menuTreeManager.GetSelectedMenu();

// Pattern 3: Expand/Collapse
_menuTreeManager.ExpandAllNodes();

// Pattern 4: Find by key
_menuTreeManager.SelectMenuByKey("FILE_MANAGEMENT");

// Pattern 5: Get all visible
var visibleMenus = _menuTreeManager.GetAllVisibleMenus();

// Pattern 6: Manage (Admin only)
_menuService.CreateMenu(newMenu, _currentUser);
_menuService.UpdateMenu(updatedMenu, _currentUser);
_menuService.DeleteMenu(menuID, _currentUser);
```

---

## ✅ Checklist การติดตั้ง

- [ ] สร้างตาราง MenuItem ในฐานข้อมูล
- [ ] สร้าง Stored Procedures
- [ ] เพิ่ม Model MenuItem
- [ ] เพิ่ม Repository Interface และ Implementation
- [ ] เพิ่ม Service Interface และ Implementation
- [ ] เพิ่ม MenuTreeViewManager
- [ ] ตั้งค่า DI Container
- [ ] ตั้งค่า MDI_Container
- [ ] ทดสอบเมนูพื้นฐาน
- [ ] ทดสอบการควบคุมตามบทบาท
- [ ] ทดสอบ CRUD operations

---

**จำหลัง**: ระบบนี้สนับสนุนการจัดการเมนูแบบพลวัต ช่วยให้ง่ายต่อการเพิ่มเมนูใหม่ โดยไม่ต้องแก้ไขโค้ด
