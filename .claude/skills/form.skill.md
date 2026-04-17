# CMS Solution — Developer Skill Guide
> .NET 4.5 · Windows Forms · SQL Server 2019 · ADO.NET · Insurance Business Domain

---

## 1. Technology Stack & Architecture

| Layer | Technology |
|---|---|
| UI | .NET 4.5 Windows Forms |
| Database | SQL Server 2019 |
| Data Access | ADO.NET (SqlConnection / SqlCommand / SqlDataReader) |
| Architecture | 5-layer Clean Architecture |

```
CMS.sln
├── CMS.Presentation      UI only — Forms + event handlers
├── CMS.Controllers       Presenter per form — bridges UI ↔ Services
├── CMS.Services          Business logic — all rules live here
├── CMS.Data              ADO.NET repositories — DB access only
├── CMS.Domain            POCO model classes
└── CMS.Infrastructure    Cross-cutting concerns (config, logging)
```

---

## 2. Layer Rules (Strict)

| Project | May reference | Forbidden |
|---|---|---|
| CMS.Presentation | CMS.Controllers, CMS.Domain | CMS.Services, CMS.Data |
| CMS.Controllers | CMS.Services (interfaces), CMS.Domain | CMS.Data |
| CMS.Services | CMS.Data (interfaces), CMS.Domain, CMS.Infrastructure | — |
| CMS.Data | CMS.Domain, CMS.Infrastructure | — |
| CMS.Domain | *(none — pure POCOs)* | — |

**Dependency flow:**
```
Presentation → Controllers → Services → Data
                    └──── Domain ◄────────┘
```

- Forms implement a **View Interface** from `CMS.Controllers` (e.g., `IKBCategoryView`)
- All business calls go through the Presenter (e.g., `_presenter.Save()`)
- Services: all validation, authorization, audit logging
- Repositories: raw ADO.NET only — no business logic
- Always use `using` blocks for SqlConnection/SqlCommand/SqlDataReader
- Null-safe reads: `r.IsDBNull(ord) ? defaultValue : r.Get*(ord)`
- Always use parameterized queries — never string concat

---

## 3. Creating a New Form

**Trigger:** "สร้าง form", "create form", "สร้างหน้าจอ", "สร้าง UI"

Follow steps 3.1 → 3.5 in order. Register every file in the corresponding `.csproj`.

---

### 3.1 CMS.Domain — Model

**Path:** `CMS.Domain\Models\{ModelName}.cs`

```csharp
namespace CMS.Domain.Models
{
    public class {ModelName}
    {
        public int    {ModelName}ID { get; set; }
        public string PropertyName  { get; set; }
        // Display helpers (non-DB derived): comment required
        public string DerivedDisplay { get; set; }
    }
}
```

`.csproj`: `<Compile Include="Models\{ModelName}.cs" />`

---

### 3.2 CMS.Data — Interface + Repository

**Interface path:** `CMS.Data\Interfaces\I{ModelName}Repository.cs`

```csharp
public interface I{ModelName}Repository
{
    IEnumerable<{ModelName}> GetAll();
    {ModelName} GetById(int id);
    int  Insert({ModelName} model);   // returns new ID via SCOPE_IDENTITY()
    void Update({ModelName} model);
    void Delete(int id);              // soft-delete: SET IsActive = 0
}
```

**Repository path:** `CMS.Data\Repositories\{ModelName}Repository.cs`

```csharp
public class {ModelName}Repository : I{ModelName}Repository
{
    private readonly string _connectionString;

    public {ModelName}Repository(IConfigManager configManager)
        => _connectionString = configManager.Get<string>("ConnectionString");

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
                INSERT INTO {Schema}.{TableName} (Col1, Col2)
                VALUES (@p1, @p2);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@p1", model.Property1);
                cmd.Parameters.AddWithValue("@p2", (object)model.Property2 ?? DBNull.Value);
                return (int)cmd.ExecuteScalar();
            }
        }
    }

    // Update / Delete follow the same using-block pattern

    private static {ModelName} Map(SqlDataReader r)
    {
        int idOrd   = r.GetOrdinal("{ModelName}ID");
        int nameOrd = r.GetOrdinal("PropertyName");
        return new {ModelName}
        {
            {ModelName}ID = r.GetInt32(idOrd),
            PropertyName  = r.IsDBNull(nameOrd) ? null : r.GetString(nameOrd),
        };
    }
}
```

`.csproj`:
```xml
<Compile Include="Interfaces\I{ModelName}Repository.cs" />
<Compile Include="Repositories\{ModelName}Repository.cs" />
```

---

### 3.3 CMS.Services — Interface + Service

**Interface path:** `CMS.Services\Interfaces\I{ModelName}Service.cs`

```csharp
public interface I{ModelName}Service
{
    IEnumerable<{ModelName}> GetAll();
    {ModelName} GetById(int id);
    int  Create({ModelName} model);
    void Update({ModelName} model);
    void Deactivate(int id);
}
```

**Service path:** `CMS.Services\{ModelName}Service.cs`

```csharp
public class {ModelName}Service : I{ModelName}Service
{
    private readonly I{ModelName}Repository _repo;
    private readonly IAuditService          _auditService;

    public {ModelName}Service(I{ModelName}Repository repo, IAuditService auditService)
    {
        if (repo         == null) throw new ArgumentNullException("repo");
        if (auditService == null) throw new ArgumentNullException("auditService");
        _repo = repo; _auditService = auditService;
    }

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

    public IEnumerable<{ModelName}> GetAll() => _repo.GetAll();
    public {ModelName} GetById(int id)        => _repo.GetById(id);
    public void Update({ModelName} model)     => _repo.Update(model);
    public void Deactivate(int id)            => _repo.Delete(id);
}
```

`.csproj`:
```xml
<Compile Include="Interfaces\I{ModelName}Service.cs" />
<Compile Include="{ModelName}Service.cs" />
```

---

### 3.4 CMS.Controllers — View Interface + Presenter

**Folder layout:**
```
CMS.Controllers\{MODULE}\{SubModule}\Interfaces\I{FormName}View.cs
CMS.Controllers\{MODULE}\{SubModule}\{FormName}Presenter.cs
```

Examples: `KMS\KB\`, `DMS\Document\`

**View Interface:**
```csharp
public interface I{FormName}View
{
    void BindList(IEnumerable<{ModelName}> items);
    void ShowError(string message);
    void ShowInfo(string message);
    void ClearForm();
    string FieldName   { get; }   // expose UI field values as properties
    int    SelectedId  { get; }
}
```

**Presenter:**
```csharp
public class {FormName}Presenter
{
    private readonly I{FormName}View     _view;
    private readonly I{ModelName}Service _service;
    private readonly UserContext          _user;

    public {FormName}Presenter(I{FormName}View view, I{ModelName}Service service, UserContext user)
    {
        if (view    == null) throw new ArgumentNullException("view");
        if (service == null) throw new ArgumentNullException("service");
        if (user    == null) throw new ArgumentNullException("user");
        _view = view; _service = service; _user = user;
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
        catch (ArgumentException ex)         { _view.ShowError(ex.Message); }
        catch (InvalidOperationException ex) { _view.ShowError(ex.Message); }
        catch (Exception ex)                 { _view.ShowError("เกิดข้อผิดพลาด: " + ex.Message); }
    }
}
```

`.csproj`:
```xml
<Compile Include="{MODULE}\{SubModule}\Interfaces\I{FormName}View.cs" />
<Compile Include="{MODULE}\{SubModule}\{FormName}Presenter.cs" />
```

---

### 3.5 CMS.Presentation — Form

**Paths:**
```
CMS.Presentation\Modules\{MODULE}\{FormName}.cs
CMS.Presentation\Modules\{MODULE}\{FormName}.Designer.cs
```

**Code-behind:**
```csharp
// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
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

        private void {FormName}_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show("Presenter ยังไม่ได้ถูก Attach\nกรุณาเรียก AttachPresenter() ก่อนเปิดฟอร์ม",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close(); return;
            }
            _presenter.Initialize();
        }

        private void BtnSave_Click(object sender, EventArgs e) => _presenter?.Save();

        // ── I{FormName}View implementation ──
        public string FieldName  => txtFieldName.Text.Trim();
        public int    SelectedId => lstItems.SelectedItems.Count > 0
                                        ? (int)lstItems.SelectedItems[0].Tag : 0;

        public void BindList(IEnumerable<{ModelName}> items)
        {
            lstItems.Items.Clear();
            foreach (var item in items)
                lstItems.Items.Add(new ListViewItem(item.DisplayName) { Tag = item.{ModelName}ID });
        }

        public void ClearForm()  { txtFieldName.Clear(); lstItems.SelectedItems.Clear(); }
        public void ShowError(string message) =>
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        public void ShowInfo(string message) =>
            MessageBox.Show(message, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
```

`.csproj`:
```xml
<Compile Include="Modules\{MODULE}\{FormName}.cs">
  <SubType>Form</SubType>
</Compile>
<Compile Include="Modules\{MODULE}\{FormName}.Designer.cs">
  <DependentUpon>{FormName}.cs</DependentUpon>
</Compile>
<!-- If resource file exists: -->
<EmbeddedResource Include="Modules\{MODULE}\{FormName}.resx">
  <DependentUpon>{FormName}.cs</DependentUpon>
</EmbeddedResource>
```

---

## 4. Naming Cheat-Sheet

| Artifact | Convention | Example |
|---|---|---|
| Form code-behind | `{Name}Form` / `{Name}Dialog` | `KBCategoryForm.cs` |
| View interface | `I{Name}View` | `IKBCategoryView.cs` |
| Presenter | `{Name}Presenter` | `KBCategoryPresenter.cs` |
| Service interface | `I{Name}Service` | `IKBCategoryService.cs` |
| Service class | `{Name}Service` | `KBCategoryService.cs` |
| Repository interface | `I{Name}Repository` | `IKBCategoryRepository.cs` |
| Repository class | `{Name}Repository` | `KBCategoryRepository.cs` |
| Domain model | plain noun | `KBCategory.cs` |
| Domain enum | plain noun | `PermissionType.cs` |

**Module codes:**

| Code | Full Name | Controller folder |
|---|---|---|
| DMS | Document Management System | `DMS\Document\` |
| KMS | Knowledge Management System | `KMS\KB\` |

---

## 5. Error Handling

| Exception | When |
|---|---|
| `ArgumentNullException` | Null constructor arg or null service param |
| `ArgumentException` | Empty/invalid required field |
| `InvalidOperationException` | Business rule violation (duplicate, self-reference, etc.) |
| `UnauthorizedAccessException` | Permission check failure |

Presenter catch blocks must call `_view.ShowError(ex.Message)` — never let exceptions bubble to WinForms unhandled.

---

## 6. .csproj Registration Checklist

Every new file **must** be added to the corresponding `.csproj`:

- [ ] `CMS.Domain.csproj` — model and enum
- [ ] `CMS.Data.csproj` — repository interface + class
- [ ] `CMS.Services.csproj` — service interface + class
- [ ] `CMS.Controllers.csproj` — view interface + presenter
- [ ] `CMS.Presentation.csproj` — form `.cs` (SubType=Form), `.Designer.cs` (DependentUpon), `.resx` if used

---

## 7. Real Examples in Codebase

| Layer | File |
|---|---|
| Domain | `CMS.Domain\Models\KBCategory.cs` |
| Data Interface | `CMS.Data\Interfaces\IKBCategoryRepository.cs` |
| Repository | `CMS.Data\Repositories\KBCategoryRepository.cs` |
| Service Interface | `CMS.Services\Interfaces\IKBCategoryService.cs` |
| Service | `CMS.Services\KBCategoryService.cs` |
| View Interface | `CMS.Controllers\KMS\KB\Interfaces\IKBCategoryView.cs` |
| Presenter | `CMS.Controllers\KMS\KB\KBCategoryPresenter.cs` |
| Form | `CMS.Presentation\Modules\KMS\KBCategoryForm.cs` |
