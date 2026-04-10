# CMS Solution — Developer Skill Guide
> .NET 4.5 · Windows Forms · SQL Server 2019 · ADO.NET  
> Insurance Business Domain · Full-Stack Development

---

## 1. Technology Stack

| Layer | Technology |
|---|---|
| UI | .NET 4.5 Windows Forms (WinForms) |
| Database | SQL Server 2019 |
| Data Access | ADO.NET (SqlConnection / SqlCommand / SqlDataReader) |
| Architecture | 5-layer Clean Architecture (Presentation → Controller → Service → Data → Domain) |

---

## 2. Solution Structure

```
CMS.sln
├── CMS.Presentation      UI only — Forms + event handlers
├── CMS.Controllers       Presenter/Controller per form — bridges UI ↔ Services
├── CMS.Services          Business logic — all rules live here
├── CMS.Data              ADO.NET repositories — DB access only
├── CMS.Domain            POCO model classes (mapping objects)
└── CMS.Infrastructure    Cross-cutting concerns (config, logging, etc.)
```

---

## 3. Layer Responsibilities & Strict Rules

### 3.1 CMS.Presentation
- **ONLY** responsible for: UI rendering, control layout, event handlers, user input.
- **FORBIDDEN**: any direct reference to `CMS.Services` or `CMS.Data`.
- Allowed `using` statements: `CMS.Controllers.*`, `CMS.Domain.*`, `System.Windows.Forms`, BCL.
- Forms implement a **View Interface** defined in `CMS.Controllers` (e.g., `IKBCategoryView`).
- All business calls go through the attached Presenter (e.g., `_presenter.Save()`).
- First line comment convention:
  ```csharp
  // ✅ Allowed usings: CMS.Controllers, CMS.Domain
  // ❌ Forbidden usings: CMS.Services, CMS.Data
  ```

### 3.2 CMS.Controllers
- Receives commands from `CMS.Presentation` and delegates to `CMS.Services`.
- No direct DB calls. No business logic beyond orchestration.
- Holds the **View Interface** definitions consumed by Presentation forms.
- Named with `Presenter` or `Controller` suffix.
- Folder layout mirrors the module hierarchy (see §5.2).

### 3.3 CMS.Services
- **All business logic must reside here** — validation, authorization, audit logging, calculations.
- Receives tasks from Controllers; decides which Repository to call.
- Service classes: root of project, suffix `Service` (e.g., `KBCategoryService.cs`).
- Interface classes: `Interfaces\` sub-folder, prefix `I`, suffix `Service` (e.g., `IKBCategoryService.cs`).
- Always validate inputs and throw meaningful exceptions (`ArgumentNullException`, `ArgumentException`, `InvalidOperationException`, `UnauthorizedAccessException`).

### 3.4 CMS.Data
- Retrieves / persists data via raw ADO.NET (`SqlConnection`, `SqlCommand`, `SqlDataReader`).
- No business logic. No validation beyond null checks.
- Repository classes: `Repositories\` folder, suffix `Repository`.
- Interface classes: `Interfaces\` folder, prefix `I`, suffix `Repository`.
- Connection string obtained via `IConfigManager.Get<string>("ConnectionString")`.
- Always use `using` blocks for `SqlConnection`, `SqlCommand`, `SqlDataReader`.
- Null-safe reader helpers: `reader.IsDBNull(ord) ? defaultValue : reader.Get*(ord)`.

### 3.5 CMS.Domain
- Pure POCO / mapping objects. No methods with side effects.
- Model classes live in `Models\` folder.
- Enum types live in `Enums\` folder.
- Maps 1-to-1 with database tables/views; property names match column names.
- Display-helper properties (non-DB derived values) are allowed and must be documented with a comment.

---

## 4. Dependency Flow

```
CMS.Presentation
      │ uses interfaces defined in
      ▼
CMS.Controllers  ──────────────────▶  CMS.Services  ──────────────────▶  CMS.Data
      │                                    │                                  │
      └── references CMS.Domain ◀──────────┴──────────────────────────────────┘
```

**Allowed references per project:**

| Project | May reference |
|---|---|
| CMS.Presentation | CMS.Controllers, CMS.Domain |
| CMS.Controllers | CMS.Services (interfaces), CMS.Domain |
| CMS.Services | CMS.Data (interfaces), CMS.Domain, CMS.Infrastructure |
| CMS.Data | CMS.Domain, CMS.Infrastructure |
| CMS.Domain | *(none — pure POCOs)* |

---

## 5. Creating a New Form (Step-by-Step)

When asked to **"สร้าง form"**, **"create form"**, **"build form"**, **"สร้างหน้าจอ"**, **"สร้าง UI"**, or **"สร้างหน้า"**, follow every step below in order.

### 5.1 CMS.Domain — Model Class

**Location:** `CMS.Domain\Models\{ModelName}.cs`

```csharp
namespace CMS.Domain.Models
{
    /// <summary>Maps to {Schema}.{TableName} in the database.</summary>
    public class {ModelName}
    {
        public int    {ModelName}ID  { get; set; }
        public string PropertyName  { get; set; }
        // ... other properties matching DB columns
        // Display helpers (non-DB):
        public string DerivedDisplay { get; set; } // set by repository join
    }
}
```

Register in `CMS.Domain.csproj`:
```xml
<Compile Include="Models\{ModelName}.cs" />
```

---

### 5.2 CMS.Data — Interface + Repository

**Interface location:** `CMS.Data\Interfaces\I{ModelName}Repository.cs`

```csharp
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface I{ModelName}Repository
    {
        IEnumerable<{ModelName}> GetAll();
        {ModelName} GetById(int id);
        int Insert({ModelName} model);
        void Update({ModelName} model);
        void Delete(int id);   // soft-delete preferred: set IsActive = 0
    }
}
```

**Repository location:** `CMS.Data\Repositories\{ModelName}Repository.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Data.Repositories
{
    public class {ModelName}Repository : I{ModelName}Repository
    {
        private readonly string _connectionString;

        public {ModelName}Repository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public IEnumerable<{ModelName}> GetAll()
        {
            var results = new List<{ModelName}>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"SELECT ... FROM {Schema}.{TableName} ORDER BY ...";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read()) results.Add(Map(reader));
            }
            return results;
        }

        public int Insert({ModelName} model)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = @"
                    INSERT INTO {Schema}.{TableName} (Col1, Col2, ...)
                    VALUES (@p1, @p2, ...);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@p1", model.Property1);
                    cmd.Parameters.AddWithValue("@p2", (object)model.Property2 ?? DBNull.Value);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private static {ModelName} Map(SqlDataReader r)
        {
            return new {ModelName}
            {
                {ModelName}ID = r.GetInt32(r.GetOrdinal("{ModelName}ID")),
                PropertyName  = r.IsDBNull(r.GetOrdinal("PropertyName"))
                                    ? null
                                    : r.GetString(r.GetOrdinal("PropertyName")),
            };
        }
    }
}
```

Register in `CMS.Data.csproj`:
```xml
<Compile Include="Interfaces\I{ModelName}Repository.cs" />
<Compile Include="Repositories\{ModelName}Repository.cs" />
```

---

### 5.3 CMS.Services — Interface + Service

**Interface location:** `CMS.Services\Interfaces\I{ModelName}Service.cs`

```csharp
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface I{ModelName}Service
    {
        IEnumerable<{ModelName}> GetAll();
        {ModelName} GetById(int id);
        int Create({ModelName} model);
        void Update({ModelName} model);
        void Deactivate(int id);   // soft-delete preferred
    }
}
```

**Service location:** `CMS.Services\{ModelName}Service.cs`

```csharp
using System;
using System.Collections.Generic;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class {ModelName}Service : I{ModelName}Service
    {
        private readonly I{ModelName}Repository _repo;
        private readonly IAuditService          _auditService;

        public {ModelName}Service(
            I{ModelName}Repository repo,
            IAuditService          auditService)
        {
            if (repo         == null) throw new ArgumentNullException("repo");
            if (auditService == null) throw new ArgumentNullException("auditService");
            _repo         = repo;
            _auditService = auditService;
        }

        public IEnumerable<{ModelName}> GetAll() => _repo.GetAll();

        public {ModelName} GetById(int id) => _repo.GetById(id);

        public int Create({ModelName} model)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (string.IsNullOrWhiteSpace(model.RequiredField))
                throw new ArgumentException("RequiredField ต้องไม่ว่าง");

            int newId = _repo.Insert(model);

            _auditService.Log(new AuditEntry
            {
                Module    = "{ModuleName}",
                Action    = "CREATE_{MODEL_UPPER}",
                UserName  = model.CreatedBy ?? string.Empty,
                TableName = "{Schema}.{TableName}",
                RecordID  = newId,
                AuditDate = DateTime.Now,
                Result    = 1
            });

            return newId;
        }

        public void Update({ModelName} model)
        {
            if (model == null) throw new ArgumentNullException("model");
            _repo.Update(model);
        }

        public void Deactivate(int id) => _repo.Delete(id);
    }
}
```

Register in `CMS.Services.csproj`:
```xml
<Compile Include="Interfaces\I{ModelName}Service.cs" />
<Compile Include="{ModelName}Service.cs" />
```

---

### 5.4 CMS.Controllers — View Interface + Presenter

#### Folder layout

| What | Path example |
|---|---|
| Module root | `CMS.Controllers\{MODULE}\{SubModule}\` |
| View interface | `CMS.Controllers\{MODULE}\{SubModule}\Interfaces\I{FormName}View.cs` |
| Presenter class | `CMS.Controllers\{MODULE}\{SubModule}\{FormName}Presenter.cs` |

**Examples:**
```
CMS.Controllers\KMS\KB\Interfaces\IKBCategoryView.cs
CMS.Controllers\KMS\KB\KBCategoryPresenter.cs

CMS.Controllers\DMS\Document\Interfaces\IFolderView.cs
CMS.Controllers\DMS\Document\FolderPresenter.cs
```

**View Interface:**
```csharp
namespace CMS.Controllers.{MODULE}.{SubModule}.Interfaces
{
    public interface I{FormName}View
    {
        void BindList(System.Collections.Generic.IEnumerable<CMS.Domain.Models.{ModelName}> items);
        void ShowError(string message);
        void ShowInfo(string message);
        void ClearForm();
        // expose UI field values as properties:
        string FieldName { get; }
        int    SelectedId { get; }
    }
}
```

**Presenter:**
```csharp
using System;
using CMS.Controllers.{MODULE}.{SubModule}.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.{MODULE}.{SubModule}
{
    public class {FormName}Presenter
    {
        private readonly I{FormName}View  _view;
        private readonly I{ModelName}Service _service;
        private readonly UserContext       _user;

        public {FormName}Presenter(
            I{FormName}View      view,
            I{ModelName}Service  service,
            UserContext          user)
        {
            if (view    == null) throw new ArgumentNullException("view");
            if (service == null) throw new ArgumentNullException("service");
            if (user    == null) throw new ArgumentNullException("user");
            _view    = view;
            _service = service;
            _user    = user;
        }

        public void Initialize()
        {
            try   { _view.BindList(_service.GetAll()); }
            catch (Exception ex) { _view.ShowError("โหลดข้อมูลไม่สำเร็จ: " + ex.Message); }
        }

        public void Save()
        {
            try
            {
                var model = new {ModelName} { FieldName = _view.FieldName };
                _service.Create(model);
                Initialize();
                _view.ClearForm();
                _view.ShowInfo("บันทึกสำเร็จ");
            }
            catch (ArgumentException ex)      { _view.ShowError(ex.Message); }
            catch (InvalidOperationException ex) { _view.ShowError(ex.Message); }
            catch (Exception ex)              { _view.ShowError("เกิดข้อผิดพลาด: " + ex.Message); }
        }
    }
}
```

Register in `CMS.Controllers.csproj`:
```xml
<!-- View interface -->
<Compile Include="{MODULE}\{SubModule}\Interfaces\I{FormName}View.cs" />
<!-- Presenter -->
<Compile Include="{MODULE}\{SubModule}\{FormName}Presenter.cs" />
```

---

### 5.5 CMS.Presentation — Designer + Code-Behind

**Locations:**
```
CMS.Presentation\Modules\{MODULE}\{FormName}.Designer.cs
CMS.Presentation\Modules\{MODULE}\{FormName}.cs
CMS.Presentation\Modules\{MODULE}\{FormName}.resx       (only if images/resources needed)
```

**Code-behind template:**
```csharp
// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CMS.Controllers.{MODULE}.{SubModule}.Interfaces;
using CMS.Controllers.{MODULE}.{SubModule};
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.{MODULE}
{
    public partial class {FormName} : Form, I{FormName}View
    {
        private {FormName}Presenter _presenter;
        private readonly UserContext _user;

        public {FormName}(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
            InitializeComponent();
        }

        public void AttachPresenter({FormName}Presenter presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        // ── Form events ──────────────────────────────────────────────────────

        private void {FormName}_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\nกรุณาเรียก AttachPresenter() ก่อนเปิดฟอร์ม",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            _presenter.Initialize();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.Save();
        }

        // ── I{FormName}View implementation ───────────────────────────────────

        public string FieldName => txtFieldName.Text.Trim();
        public int    SelectedId => (lstItems.SelectedItems.Count > 0)
                                        ? ((int)lstItems.SelectedItems[0].Tag)
                                        : 0;

        public void BindList(IEnumerable<{ModelName}> items)
        {
            lstItems.Items.Clear();
            foreach (var item in items)
            {
                var lvi = new ListViewItem(item.DisplayName) { Tag = item.{ModelName}ID };
                lstItems.Items.Add(lvi);
            }
        }

        public void ClearForm()
        {
            txtFieldName.Clear();
            lstItems.SelectedItems.Clear();
        }

        public void ShowError(string message) =>
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void ShowInfo(string message) =>
            MessageBox.Show(message, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
```

**Designer template (minimal):**
```csharp
namespace CMS.Presentation.Modules.{MODULE}
{
    partial class {FormName}
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // TODO: add controls — Panel, ListView, TextBox, Button, Label
            this.SuspendLayout();
            // ... control declarations ...
            this.ResumeLayout(false);
        }
    }
}
```

Register in `CMS.Presentation.csproj`:
```xml
<Compile Include="Modules\{MODULE}\{FormName}.cs">
  <SubType>Form</SubType>
</Compile>
<Compile Include="Modules\{MODULE}\{FormName}.Designer.cs">
  <DependentUpon>{FormName}.cs</DependentUpon>
</Compile>
<!-- Only if resource file exists: -->
<EmbeddedResource Include="Modules\{MODULE}\{FormName}.resx">
  <DependentUpon>{FormName}.cs</DependentUpon>
</EmbeddedResource>
```

---

## 6. Module Naming Conventions

| Module Code | Full Name | Folder |
|---|---|---|
| DMS | Document Management System | `DMS\Document\` |
| KMS | Knowledge Management System | `KMS\KB\` |
| *(new)* | *(new module)* | `{MODULE}\{SubModule}\` |

**Controller Interfaces folder per module:**

| Module | Interface folder |
|---|---|
| DMS | `CMS.Controllers\DMS\Document\Interfaces\` |
| KMS | `CMS.Controllers\KMS\KB\Interfaces\` |
| *(new)* | `CMS.Controllers\{MODULE}\{SubModule}\Interfaces\` |

---

## 7. Naming Cheat-Sheet

| Artifact | Suffix/Prefix | Example |
|---|---|---|
| Form code-behind | `Form` or `Dialog` | `KBCategoryForm.cs` |
| View interface | `I` + `View` | `IKBCategoryView.cs` |
| Presenter/Controller | `Presenter` or `Controller` | `KBCategoryPresenter.cs` |
| Service interface | `I` + `Service` | `IKBCategoryService.cs` |
| Service class | `Service` | `KBCategoryService.cs` |
| Repository interface | `I` + `Repository` | `IKBCategoryRepository.cs` |
| Repository class | `Repository` | `KBCategoryRepository.cs` |
| Domain model | *(plain noun)* | `KBCategory.cs` |
| Domain enum | *(plain noun)* | `PermissionType.cs` |

---

## 8. ADO.NET Patterns

### Null-safe mapping helper
```csharp
private static T SafeGet<T>(SqlDataReader r, string col, T defaultValue = default(T))
{
    int ord = r.GetOrdinal(col);
    return r.IsDBNull(ord) ? defaultValue : (T)r.GetValue(ord);
}
```

### Parameterised query — always use parameters, never string concat
```csharp
cmd.Parameters.AddWithValue("@Name", (object)model.Name ?? DBNull.Value);
```

### Scalar insert (returns new ID)
```csharp
const string sql = @"INSERT INTO Schema.Table (Col) VALUES (@v); SELECT CAST(SCOPE_IDENTITY() AS INT);";
return (int)cmd.ExecuteScalar();
```

### Soft delete pattern
```csharp
const string sql = "UPDATE Schema.Table SET IsActive = 0 WHERE TableID = @id";
```

---

## 9. Error Handling Guidelines

| Exception type | When to throw |
|---|---|
| `ArgumentNullException` | Null constructor argument or null service parameter |
| `ArgumentException` | Empty/invalid required field value |
| `InvalidOperationException` | Business rule violation (duplicate, self-reference, etc.) |
| `UnauthorizedAccessException` | Permission check failure |

Presenter catch blocks must always call `_view.ShowError(ex.Message)` — never let exceptions bubble to WinForms unhandled.

---

## 10. .csproj Registration Checklist

Every new file **must** be added to the corresponding `.csproj` `<ItemGroup>`:

- [ ] `CMS.Domain.csproj` — model and enum classes
- [ ] `CMS.Data.csproj` — repository interface and class
- [ ] `CMS.Services.csproj` — service interface and class
- [ ] `CMS.Controllers.csproj` — view interface and presenter
- [ ] `CMS.Presentation.csproj` — form `.cs`, `.Designer.cs`, and `.resx` (if used)

Form entries in `CMS.Presentation.csproj` must use `<SubType>Form</SubType>` and `<DependentUpon>` to keep the IDE file grouping correct.

---

## 11. Quick Reference: Real Examples in Codebase

| Layer | File | Notes |
|---|---|---|
| Domain | `CMS.Domain\Models\KBCategory.cs` | Self-referencing hierarchy, display helper property |
| Data Interface | `CMS.Data\Interfaces\IKBCategoryRepository.cs` | CRUD interface |
| Repository | `CMS.Data\Repositories\KBCategoryRepository.cs` | Full ADO.NET CRUD |
| Service Interface | `CMS.Services\Interfaces\IKBCategoryService.cs` | Thin interface |
| Service | `CMS.Services\KBCategoryService.cs` | Validation + audit logging |
| View Interface | `CMS.Controllers\KMS\KB\Interfaces\IKBCategoryView.cs` | Used by presenter |
| Presenter | `CMS.Controllers\KMS\KB\KBCategoryPresenter.cs` | Orchestration only |
| Form code-behind | `CMS.Presentation\Modules\KMS\KBCategoryForm.cs` | Implements IKBCategoryView |
| Form designer | `CMS.Presentation\Modules\KMS\KBCategoryForm.Designer.cs` | InitializeComponent only |
