# ระบบเมนู - ไก่ด์การใช้งานอย่างรวดเร็ว

## 🎯 สารบัญ

1. [โครงสร้างข้อมูลเมนู](#โครงสร้างข้อมูลเมนู)
2. [โฟลว์การทำงาน](#โฟลว์การทำงาน)
3. [ส่วนประกอบหลัก](#ส่วนประกอบหลัก)
4. [ตัวอย่างบูรณาการ](#ตัวอย่างบูรณาการ)
5. [SQL Queries ที่ใช้บ่อย](#sql-queries-ที่ใช้บ่อย)

---

## โครงสร้างข้อมูลเมนู

### ตาราง MenuItem

```
┌─────────────────────────────────────────────────────────────┐
│              core.MenuItem                                  │
├─────────────────────────────────────────────────────────────┤
│ MenuItemID (PK)        │ int NOT NULL PRIMARY KEY            │
│ ParentMenuItemID (FK)  │ int NULL → MenuItem.MenuItemID      │
│ MenuName               │ nvarchar(255) NOT NULL              │
│ MenuKey (UNIQUE)       │ nvarchar(100) UNIQUE                │
│ FormClassName          │ nvarchar(500) NULL                  │
│ IconKey                │ nvarchar(100) NULL                  │
│ DisplayOrder           │ int DEFAULT 0                       │
│ IsActive               │ bit DEFAULT 1                       │
│ RequiredAppRole        │ int NULL (1,2,3,4)                  │
│ Description            │ nvarchar(500) NULL                  │
│ ModuleCode             │ nvarchar(50) NULL                   │
│ Url                    │ nvarchar(500) NULL                  │
│ CreatedDate            │ datetime DEFAULT GETDATE()          │
│ CreatedByUserID (FK)   │ int NOT NULL                         │
│ ModifiedDate           │ datetime NULL                       │
│ ModifiedByUserID (FK)  │ int NULL                             │
└─────────────────────────────────────────────────────────────┘
```

### Relationships
```
MenuItem ─┐
          ├─→ MenuItem (Self-join: ParentMenuItemID)
          │
MenuItem ─┐
          └─→ UserAccount (CreatedByUserID, ModifiedByUserID)
```

---

## โฟลว์การทำงาน

### 1️⃣ โฟลว์ยอดนิยม: ผู้ใช้เลือกเมนู

```
User Opens MDI_Container
    ↓
InitializeMenuSystem() called
    ↓
MenuTreeViewManager.PopulateMenuTree()
    ↓
MenuService.GetRootMenus(userContext)
    ├─ Filter by RequiredAppRole
    └─ Filter by IsActive = 1
    ↓
MenuItemRepository.GetRootMenuItems()
    ↓
Query: SELECT * FROM MenuItem WHERE ParentMenuItemID IS NULL
    ↓
Build TreeView Nodes
    ├─ Root nodes
    └─ Child nodes (recursive)
    ↓
Display TreeView (_treeMainMenu)
    ↓
User clicks node
    ↓
MenuTree_MenuSelected() event fires
    ↓
Open Form (if FormClassName provided)
```

### 2️⃣ โฟลว์: สร้างเมนูใหม่ (Admin)

```
Admin: CreateMenu()
    ↓
Validate: Role == Admin (4)
    ↓
Validate: MenuName, MenuKey required
    ↓
Validate: MenuKey unique
    ↓
MenuService.CreateMenu()
    ↓
MenuItemRepository.CreateMenuItem()
    ↓
EXEC sp_CreateMenuItem
    ├─ Validate inputs
    ├─ INSERT record
    └─ Return new ID
    ↓
AuditService.LogAction()
    ├─ Action: CREATE_MENU
    └─ Log to AuditLog table
    ↓
_menuTreeManager.RefreshMenuTree()
    ↓
TreeView updated with new menu
```

### 3️⃣ โฟลว์: อัปเดตเมนู (Admin)

```
Admin: UpdateMenuDetails()
    ↓
Validate: Role == Admin
    ↓
Modify MenuItem object
    ↓
MenuService.UpdateMenu()
    ↓
MenuItemRepository.UpdateMenuItem()
    ↓
EXEC sp_UpdateMenuItem
    ├─ Validate inputs
    ├─ Check MenuKey uniqueness
    └─ UPDATE record
    ↓
AuditService.LogAction()
    ↓
RefreshMenuTree()
```

---

## ส่วนประกอบหลัก

### Component 1: Model (MenuItem.cs)

```csharp
public class MenuItem
{
    public int MenuItemID { get; set; }
    public int? ParentMenuItemID { get; set; }  // Parent menu
    public string MenuName { get; set; }         // Display name
    public string MenuKey { get; set; }          // Unique key
    public string FormClassName { get; set; }    // Full class name
    public string IconKey { get; set; }          // Icon reference
    public int DisplayOrder { get; set; }        // Sort order
    public bool IsActive { get; set; }           // Active flag
    public int? RequiredAppRole { get; set; }    // Min required role
    public string Description { get; set; }      // Tooltip
    public string ModuleCode { get; set; }       // Module identifier
    public string Url { get; set; }              // URL for web menus
    public DateTime CreatedDate { get; set; }
    public int CreatedByUserID { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int? ModifiedByUserID { get; set; }
    public virtual List<MenuItem> ChildMenuItems { get; set; }
    public virtual MenuItem ParentMenuItem { get; set; }
}
```

### Component 2: Repository (MenuItemRepository.cs)

```csharp
// CRUD Methods
int CreateMenuItem(MenuItem item)          // Create
MenuItem GetMenuItemByID(int id)            // Read
void UpdateMenuItem(MenuItem item)          // Update
void DeleteMenuItem(int id)                 // Delete

// Search Methods
List<MenuItem> GetAllMenuItems()
List<MenuItem> GetRootMenuItems()
List<MenuItem> GetChildMenuItems(int parentID)
MenuItem GetMenuItemByKey(string key)
List<MenuItem> GetMenuItemsByRole(int role)

// Utility Methods
List<MenuItem> GetMenuHierarchy()
bool MenuKeyExists(string key)
void UpdateMenuItemOrder(int id, int order)
```

### Component 3: Service (MenuService.cs)

```csharp
// User-facing methods
List<MenuItem> GetVisibleMenus(UserContext user)
List<MenuItem> GetRootMenus(UserContext user)
List<MenuItem> GetChildMenus(int parentID, UserContext user)
MenuItem GetMenuByKey(string key, UserContext user)

// Admin methods (Auth required)
int CreateMenu(MenuItem menu, UserContext admin)
void UpdateMenu(MenuItem menu, UserContext admin)
void DeleteMenu(int id, UserContext admin)
void SetMenuActive(int id, bool active, UserContext admin)
void ReorderMenus(List<MenuItemOrder> orders, UserContext admin)

// Utility
bool IsMenuKeyUnique(string key, int? excludeID = null)
MenuStatistics GetMenuStatistics()
```

### Component 4: UI Manager (MenuTreeViewManager.cs)

```csharp
// Initialization
void PopulateMenuTree()        // Load from DB
void RefreshMenuTree()          // Reload

// Navigation
void ExpandAllNodes()
void CollapseAllNodes()
bool SelectMenuByKey(string key)
MenuItem GetSelectedMenu()

// Data
List<MenuItem> GetAllVisibleMenus()
void RegisterIcon(string key, Image image)
Image GetIcon(string key)

// Events
event MenuSelected            // Fires when menu item selected
```

---

## ตัวอย่างบูรณาการ

### ขั้นตอนที่ 1: ตั้งค่า DI Container

```csharp
// Program.cs หรือ Setup method
public static void RegisterMenuServices(IServiceCollection services)
{
    // Repositories
    services.AddScoped<IMenuItemRepository, MenuItemRepository>();
    
    // Services
    services.AddScoped<IMenuService, MenuService>();
    
    // UI Managers
    services.AddTransient<MenuTreeViewManager>();
}
```

### ขั้นตอนที่ 2: ตั้งค่าใน MDI_Container

```csharp
using CMS.Services.Interfaces;

public partial class MDI_Container : Form
{
    private MenuTreeViewManager _menuTreeManager;
    private IMenuService _menuService;
    private IMenuItemRepository _menuRepository;
    
    public MDI_Container(
        IMenuService menuService,
        IMenuItemRepository menuRepository,
        // ... other dependencies
    )
    {
        _menuService = menuService;
        _menuRepository = menuRepository;
        InitializeComponent();
    }
    
    private void MDI_Container_Load(object sender, EventArgs e)
    {
        InitializeMenuSystem();
    }
    
    private void InitializeMenuSystem()
    {
        try
        {
            // Create manager and wire events
            _menuTreeManager = new MenuTreeViewManager(
                _treeMainMenu,      // TreeView control
                _menuService,       // Service
                _currentUser        // Current user
            );
            
            _menuTreeManager.MenuSelected += MenuTree_MenuSelected;
            
            // Load menus
            _menuTreeManager.PopulateMenuTree();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing menu: {ex.Message}");
        }
    }
}
```

### ขั้นตอนที่ 3: จัดการการเลือกเมนู

```csharp
private void MenuTree_MenuSelected(object sender, MenuSelectedEventArgs e)
{
    var menu = e.SelectedMenu;
    
    // Single click - update status
    if (!e.IsDoubleClick)
    {
        statusLabel.Text = $"Selected: {menu.MenuName}";
        return;
    }
    
    // Double click - open form
    if (!string.IsNullOrEmpty(menu.FormClassName))
    {
        OpenModule(menu);
    }
}

private void OpenModule(MenuItem menu)
{
    try
    {
        Type formType = Type.GetType(menu.FormClassName);
        
        if (formType == null)
        {
            MessageBox.Show($"Cannot find form: {menu.FormClassName}");
            return;
        }
        
        if (!typeof(Form).IsAssignableFrom(formType))
        {
            MessageBox.Show($"Type is not a Form: {menu.FormClassName}");
            return;
        }
        
        // Create instance with dependency injection
        var form = (Form)Activator.CreateInstance(
            formType, 
            _currentUser,
            _serviceProvider  // Optional: pass service container
        );
        
        form.MdiParent = this;
        form.Show();
        
        // Audit log
        _auditService.LogAction(
            userID: _currentUser.UserID,
            action: "OPEN_MODULE",
            resource: menu.MenuKey,
            module: menu.ModuleCode ?? "MenuSystem",
            details: $"Opened: {menu.MenuName}"
        );
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error opening module: {ex.Message}");
    }
}
```

### ขั้นตอนที่ 4: โค้ด Admin - สร้างเมนู

```csharp
private void btnCreateMenu_Click(object sender, EventArgs e)
{
    if (_currentUser.AppRole != 4) // Admin only
    {
        MessageBox.Show("Admin only");
        return;
    }
    
    var newMenu = new MenuItem
    {
        ParentMenuItemID = null,
        MenuName = txtMenuName.Text,
        MenuKey = txtMenuKey.Text,
        FormClassName = txtFormClass.Text,
        DisplayOrder = (int)spinOrder.Value,
        IsActive = chkActive.Checked,
        RequiredAppRole = cmbRole.SelectedIndex > 0 ? cmbRole.SelectedIndex : (int?)null,
        Description = txtDescription.Text,
        ModuleCode = txtModule.Text,
        CreatedByUserID = _currentUser.UserID
    };
    
    try
    {
        int newID = _menuService.CreateMenu(newMenu, _currentUser);
        MessageBox.Show($"Created successfully (ID: {newID})");
        _menuTreeManager.RefreshMenuTree();
        ClearForm();
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}");
    }
}
```

---

## SQL Queries ที่ใช้บ่อย

### ดูเมนูทั้งหมด
```sql
SELECT 
    MenuItemID,
    MenuName,
    MenuKey,
    DisplayOrder,
    CASE IsActive WHEN 1 THEN 'Active' ELSE 'Inactive' END AS Status,
    CASE 
        WHEN RequiredAppRole IS NULL THEN 'All'
        WHEN RequiredAppRole = 1 THEN 'User'
        WHEN RequiredAppRole = 2 THEN 'Manager'
        WHEN RequiredAppRole = 3 THEN 'Approver'
        WHEN RequiredAppRole = 4 THEN 'Admin'
    END AS 'Min Role'
FROM core.MenuItem
ORDER BY DisplayOrder;
```

### ดูเมนูลำดับชั้น
```sql
WITH MenuCTE AS (
    SELECT 
        MenuItemID,
        CAST(MenuName AS NVARCHAR(MAX)) AS HierarchyName,
        ParentMenuItemID,
        DisplayOrder,
        1 AS Level
    FROM core.MenuItem
    WHERE ParentMenuItemID IS NULL
    
    UNION ALL
    
    SELECT 
        m.MenuItemID,
        REPLICATE('  ', c.Level) + m.MenuName,
        m.ParentMenuItemID,
        m.DisplayOrder,
        c.Level + 1
    FROM core.MenuItem m
    INNER JOIN MenuCTE c ON m.ParentMenuItemID = c.MenuItemID
)
SELECT 
    MenuItemID,
    HierarchyName AS 'Menu Structure',
    DisplayOrder,
    Level
FROM MenuCTE
ORDER BY Level, DisplayOrder;
```

### ดูเมนูตามบทบาท
```sql
-- Show menus visible to Manager (AppRole = 2)
SELECT 
    MenuItemID,
    MenuName,
    MenuKey,
    RequiredAppRole,
    DisplayOrder
FROM core.MenuItem
WHERE IsActive = 1
  AND (RequiredAppRole IS NULL OR RequiredAppRole <= 2)
ORDER BY DisplayOrder;
```

### นับจำนวนเมนู
```sql
SELECT 
    COUNT(*) AS TotalMenus,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveMenus,
    SUM(CASE WHEN ParentMenuItemID IS NULL THEN 1 ELSE 0 END) AS RootMenus,
    MAX(ModifiedDate) AS LastModified
FROM core.MenuItem;
```

### หาเมนูที่ inactive
```sql
SELECT 
    MenuItemID,
    MenuName,
    IsActive,
    ModifiedDate,
    ModifiedByUserID
FROM core.MenuItem
WHERE IsActive = 0
ORDER BY ModifiedDate DESC;
```

### ตรวจสอบโครงสร้างความเป็นพ่อแม่
```sql
SELECT 
    m.MenuItemID,
    m.MenuName AS 'Child Menu',
    ISNULL(p.MenuName, 'ROOT') AS 'Parent Menu'
FROM core.MenuItem m
LEFT JOIN core.MenuItem p ON m.ParentMenuItemID = p.MenuItemID
ORDER BY p.MenuItemID, m.DisplayOrder;
```

---

## 🔄 วัฏจักรชีวิตของการใช้งานเมนู

```
┌─────────────────────────────┐
│   Application Startup       │
├─────────────────────────────┤
│ 1. Load MenuService         │
│ 2. Load MenuTreeViewManager │
│ 3. Call PopulateMenuTree()  │
└──────────────┬──────────────┘
               ↓
┌─────────────────────────────┐
│   User Using Application    │
├─────────────────────────────┤
│ • Browse tree nodes         │
│ • Select menu items         │
│ • Open forms                │
│ • Perform actions           │
└──────────────┬──────────────┘
               ↓
┌─────────────────────────────┐
│   Rights Changed / Update   │
├─────────────────────────────┤
│ 1. Call RefreshMenuTree()   │
│ 2. Reload from database     │
│ 3. Re-filter by role        │
└──────────────┬──────────────┘
               ↓
┌─────────────────────────────┐
│   Application Shutdown      │
├─────────────────────────────┤
│ • Connection closed         │
│ • Resources released        │
└─────────────────────────────┘
```

---

## 📋 Development Checklist

```
☐ Database Setup
  ☐ Run Menu_Schema_Migration.sql
  ☐ Verify tables created
  ☐ Insert sample data

☐ Code Implementation
  ☐ Add MenuItem.cs model
  ☐ Add IMenuItemRepository interface
  ☐ Add MenuItemRepository implementation
  ☐ Add IMenuService interface
  ☐ Add MenuService implementation
  ☐ Add MenuTreeViewManager class

☐ Configuration
  ☐ Register in DI container
  ☐ Add to MDI_Container
  ☐ Wire event handlers

☐ Testing
  ☐ Test PopulateMenuTree()
  ☐ Test menu selection
  ☐ Test role-based filtering
  ☐ Test form opening
  ☐ Test CRUD operations (Admin)
  ☐ Test audit logging

☐ Documentation
  ☐ Update README
  ☐ Document MenuKey values
  ☐ Document FormClassName values
  ☐ Create admin menu management UI
```

---

**หมายเหตุ**: ระบบนี้ได้รับการออกแบบให้เป็น modular และ scalable สำหรับการจัดการเมนูแบบพลวัน
