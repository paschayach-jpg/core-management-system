# SKILL: Build a Form (CMS MVP — Strict 6-Layer Architecture)

Invoke this skill whenever the user says: "create a form", "build a screen", "add a UI",
"create a dialog", "build a view", or any equivalent request for a new WinForms screen.

---

## Architecture Constraint — Read Before Writing Any Code

```
CMS.Presentation  → only references CMS.Controllers + CMS.Domain
      ▼
CMS.Controllers   → only references CMS.Services + CMS.Domain
      ▼
CMS.Services      → only references CMS.Data + CMS.Domain + CMS.Infrastructure
      ▼
CMS.Data          → only references CMS.Domain + CMS.Infrastructure
      ▼
CMS.Domain / CMS.Infrastructure
```

**Non-negotiable rules:**
- `CMS.Presentation` form files must NEVER contain `using CMS.Services` or `using CMS.Data`.
- Forms receive a pre-built **Presenter** via `AttachPresenter()` — they never `new` up services or repositories.
- Services are only instantiated in the **composition root** (`MDI_Container.cs` or a dedicated factory in `CMS.Presentation`).

---

## Deliverables per Form

Every form task must produce **all** of these files — never skip a layer:

| # | File | Project | Location |
|---|---|---|---|
| 1 | `{EntityName}.cs` | CMS.Domain | `Models/` |
| 2 | `I{Entity}Repository.cs` | CMS.Data | `Interfaces/` |
| 3 | `{Entity}Repository.cs` | CMS.Data | `Repositories/` |
| 4 | `I{Form}Service.cs` | CMS.Services | `Interfaces/` |
| 5 | `{Form}Service.cs` | CMS.Services | root |
| 6 | `I{Form}View.cs` | CMS.Controllers | `{Module}/Interfaces/` |
| 7 | `{Form}Presenter.cs` | CMS.Controllers | `{Module}/{SubModule}/` |
| 8 | `{Form}Form.cs` | CMS.Presentation | `Modules/{Module}/` |
| 9 | `{Form}Form.Designer.cs` | CMS.Presentation | `Modules/{Module}/` |
| 10 | `.csproj` edits | all affected projects | `<Compile>` + `<ProjectReference>` |

---

## Step-by-Step Instructions

---

### STEP 1 — Clarify scope before writing code

Determine the following (ask the user or infer from context):

- **Module** — which top-level module? (`DMS`, `KMS`, or a new one?)
- **Sub-module** — e.g., `KB` (Knowledge Base), `Document`, `Claim`
- **FormBaseName** — e.g., `KBCategory`, `ClaimApproval`, `PolicyHolder`
- **EntityName** — the domain object, e.g., `KBCategory`
- **Form type** — main `Form` or modal `Dialog`?
- **Operations** — which actions does the form support? Add / Edit / Delete / View / Search
- **Existing files** — do the model, service, or repository already exist?

> If anything is ambiguous, ask before generating code.

---

### STEP 2 — Derive all file paths

Resolve every path from the naming rules before touching any file:

```
Example:
  Module       = KMS
  SubModule    = KB
  FormBaseName = KBCategory
  EntityName   = KBCategory

File paths:
  CMS.Domain/Models/KBCategory.cs
  CMS.Data/Interfaces/IKBCategoryRepository.cs
  CMS.Data/Repositories/KBCategoryRepository.cs
  CMS.Services/Interfaces/IKBCategoryService.cs
  CMS.Services/KBCategoryService.cs
  CMS.Controllers/KMS/Interfaces/IKBCategoryView.cs
  CMS.Controllers/KMS/KB/KBCategoryPresenter.cs
  CMS.Presentation/Modules/KMS/KBCategoryForm.cs
  CMS.Presentation/Modules/KMS/KBCategoryForm.Designer.cs
```

---

### STEP 3 — CMS.Domain: Domain Model

Skip this step if the model already exists.

**File:** `CMS.Domain/Models/{EntityName}.cs`

```csharp
namespace CMS.Domain.Models
{
    public class {EntityName}
    {
        // Primary key
        public int {EntityName}ID { get; set; }

        // Core fields — add all relevant columns
        public string Name { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }

        // Alias for backward compatibility
        public int {EntityName}Id
        {
            get { return {EntityName}ID; }
            set { {EntityName}ID = value; }
        }

        // Transient fields — populated by service layer, NOT columns in the DB
        public string CreatedByName { get; set; }

        // Enum conversion — add only if the entity has an int status/type column
        // public SomeStatusEnum StatusEnum
        // {
        //     get { return (SomeStatusEnum)StatusInt; }
        //     set { StatusInt = (int)value; }
        // }
    }
}
```

---

### STEP 4 — CMS.Data: Repository Interface

**File:** `CMS.Data/Interfaces/I{EntityName}Repository.cs`

```csharp
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface I{EntityName}Repository
    {
        int Insert({EntityName} entity);
        void Update({EntityName} entity);
        void Delete(int id);
        {EntityName} GetById(int id);
        IEnumerable<{EntityName}> GetAll();
        // Add query methods needed by the service, e.g.:
        // IEnumerable<{EntityName}> GetByParentId(int parentId);
    }
}
```

---

### STEP 5 — CMS.Data: Repository Implementation

**File:** `CMS.Data/Repositories/{EntityName}Repository.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure;

namespace CMS.Data.Repositories
{
    public class {EntityName}Repository : I{EntityName}Repository
    {
        private readonly string _connectionString;

        public {EntityName}Repository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int Insert({EntityName} entity)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("{schema}.sp_Insert{EntityName}", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", entity.Name);
                    cmd.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy);
                    // Nullable parameters: (object)entity.Field ?? DBNull.Value
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update({EntityName} entity)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("{schema}.sp_Update{EntityName}", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@{EntityName}ID", entity.{EntityName}ID);
                    cmd.Parameters.AddWithValue("@Name", entity.Name);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("{schema}.sp_Delete{EntityName}", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@{EntityName}ID", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public {EntityName} GetById(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("{schema}.sp_Get{EntityName}ById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@{EntityName}ID", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) return MapRow(reader);
                        return null;
                    }
                }
            }
        }

        public IEnumerable<{EntityName}> GetAll()
        {
            var list = new List<{EntityName}>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("{schema}.sp_GetAll{EntityName}s", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) list.Add(MapRow(reader));
                    }
                }
            }
            return list;
        }

        private {EntityName} MapRow(SqlDataReader reader)
        {
            return new {EntityName}
            {
                {EntityName}ID = Convert.ToInt32(reader["{EntityName}ID"]),
                Name           = reader["Name"].ToString(),
                CreatedBy      = Convert.ToInt32(reader["CreatedBy"])
                // Map all columns here
            };
        }
    }
}
```

**Rules:**
- Every DB call uses a stored procedure — no inline SQL.
- Stored procedure naming: `{schema}.sp_{Verb}{EntityName}` — e.g., `km.sp_InsertKBCategory`.
- Nullable columns: `(object)value ?? DBNull.Value`.
- Always map results in a private `MapRow(SqlDataReader)` method.

---

### STEP 6 — CMS.Services: Service Interface

**File:** `CMS.Services/Interfaces/I{FormBaseName}Service.cs`

```csharp
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface I{FormBaseName}Service
    {
        IEnumerable<{EntityName}> GetAll(UserContext user);
        {EntityName} GetById(int id, UserContext user);
        void Create({EntityName} entity, UserContext user);
        void Update({EntityName} entity, UserContext user);
        void Delete(int id, UserContext user);
        // Add only the methods actually needed by the presenter
    }
}
```

---

### STEP 7 — CMS.Services: Service Implementation

**File:** `CMS.Services/{FormBaseName}Service.cs`

```csharp
using System;
using System.Collections.Generic;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class {FormBaseName}Service : I{FormBaseName}Service
    {
        private readonly I{EntityName}Repository _{entityVar}Repository;
        // Add additional repository/service dependencies as needed

        public {FormBaseName}Service(I{EntityName}Repository {entityVar}Repository)
        {
            if ({entityVar}Repository == null)
                throw new ArgumentNullException("{entityVar}Repository");
            _{entityVar}Repository = {entityVar}Repository;
        }

        public IEnumerable<{EntityName}> GetAll(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return _{entityVar}Repository.GetAll();
        }

        public {EntityName} GetById(int id, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            var entity = _{entityVar}Repository.GetById(id);
            if (entity == null) throw new KeyNotFoundException("{EntityName} not found.");
            return entity;
        }

        public void Create({EntityName} entity, UserContext user)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (user == null) throw new ArgumentNullException("user");
            // ── Business rules and validation go HERE ──
            if (string.IsNullOrWhiteSpace(entity.Name))
                throw new ArgumentException("Name is required.");
            // ──────────────────────────────────────────
            entity.CreatedBy   = user.UserId;
            entity.CreatedDate = DateTime.Now;
            _{entityVar}Repository.Insert(entity);
        }

        public void Update({EntityName} entity, UserContext user)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (user == null) throw new ArgumentNullException("user");
            _{entityVar}Repository.Update(entity);
        }

        public void Delete(int id, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _{entityVar}Repository.Delete(id);
        }
    }
}
```

**Rules:**
- Null-check every constructor parameter and every public method parameter.
- All business rules, validation, and domain logic live here — never in the presenter or form.
- If multiple repositories or services are needed, inject all of them in the constructor.

---

### STEP 8 — CMS.Controllers: View Interface

**File:** `CMS.Controllers/{Module}/Interfaces/I{FormBaseName}View.cs`

```csharp
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.{Module}.Interfaces
{
    public interface I{FormBaseName}View
    {
        // Presenter → View: push data to form controls
        void Show{EntityName}List(IEnumerable<{EntityName}> items);
        void ShowError(string message);
        void ShowInfo(string message);

        // View → Presenter: expose selected UI state (never raw controls)
        {EntityName} Selected{EntityName} { get; }
    }
}
```

**Rules:**
- The interface contains only what the presenter needs — no ListViews, TextBoxes, or other control types.
- `ShowError` and `ShowInfo` are mandatory on every view interface.
- Selection properties are always read-only (`{ get; }`).

---

### STEP 9 — CMS.Controllers: Presenter

**File:** `CMS.Controllers/{Module}/{SubModule}/{FormBaseName}Presenter.cs`

```csharp
using System;
using CMS.Controllers.{Module}.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.{Module}.{SubModule}
{
    public class {FormBaseName}Presenter
    {
        private readonly I{FormBaseName}Service _service;
        private readonly I{FormBaseName}View _view;
        private readonly UserContext _user;

        public {FormBaseName}Presenter(
            I{FormBaseName}Service service,
            I{FormBaseName}View view,
            UserContext user)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (view == null)    throw new ArgumentNullException("view");
            if (user == null)    throw new ArgumentNullException("user");

            _service = service;
            _view    = view;
            _user    = user;
        }

        public void LoadData()
        {
            try
            {
                var items = _service.GetAll(_user);
                _view.Show{EntityName}List(items);
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowError("Access denied: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to load: " + ex.Message);
            }
        }

        public void Add({EntityName} entity)
        {
            try
            {
                _service.Create(entity, _user);
                LoadData();
                _view.ShowInfo("{EntityName} created successfully.");
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
        }

        public void Edit({EntityName} entity)
        {
            try
            {
                _service.Update(entity, _user);
                LoadData();
                _view.ShowInfo("{EntityName} updated successfully.");
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
        }

        public void Delete(int id)
        {
            try
            {
                _service.Delete(id, _user);
                LoadData();
                _view.ShowInfo("{EntityName} deleted successfully.");
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
        }
    }
}
```

**Rules:**
- Only add methods for actions the form actually supports.
- Always wrap every service call in try-catch; surface errors via `_view.ShowError()`.
- After any mutation, call a load method to refresh the view.
- The presenter never touches UI controls, never calls `MessageBox.Show`.

---

### STEP 10 — CMS.Presentation: Form Code-behind

**File:** `CMS.Presentation/Modules/{Module}/{FormBaseName}Form.cs`

```csharp
// ✅ Allowed: CMS.Controllers, CMS.Domain
// ❌ Forbidden: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CMS.Controllers.{Module}.Interfaces;
using CMS.Controllers.{Module}.{SubModule};
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.{Module}
{
    public partial class {FormBaseName}Form : Form, I{FormBaseName}View
    {
        private {FormBaseName}Presenter _presenter;
        private readonly UserContext _user;
        private List<{EntityName}> _items = new List<{EntityName}>();

        // Constructor has NO service/repository parameters
        public {FormBaseName}Form(UserContext user)
        {
            InitializeComponent();
            _user = user;
        }

        /// <summary>
        /// Inject the pre-built presenter from the composition root.
        /// Must be called before the form is shown.
        /// </summary>
        public void AttachPresenter({FormBaseName}Presenter presenter)
        {
            _presenter = presenter;
        }

        // ── Form events ──────────────────────────────────────────────────
        private void {FormBaseName}Form_Load(object sender, EventArgs e)
        {
            _presenter.LoadData();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var entity = new {EntityName}
            {
                Name      = txt{Field}.Text.Trim(),
                CreatedBy = _user.UserId
            };
            _presenter.Add(entity);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (Selected{EntityName} == null) { ShowInfo("Please select an item."); return; }
            var entity  = Selected{EntityName};
            entity.Name = txt{Field}.Text.Trim();
            _presenter.Edit(entity);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (Selected{EntityName} == null) { ShowInfo("Please select an item."); return; }
            if (MessageBox.Show("Delete selected item?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                _presenter.Delete(Selected{EntityName}.{EntityName}ID);
        }

        // ── I{FormBaseName}View ──────────────────────────────────────────
        public void Show{EntityName}List(IEnumerable<{EntityName}> items)
        {
            _items = new List<{EntityName}>(items);
            listView{EntityName}.Items.Clear();
            foreach (var item in _items)
                listView{EntityName}.Items.Add(new ListViewItem(item.Name) { Tag = item });
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public {EntityName} Selected{EntityName}
        {
            get
            {
                if (listView{EntityName}.SelectedItems.Count == 0) return null;
                return listView{EntityName}.SelectedItems[0].Tag as {EntityName};
            }
        }
    }
}
```

**Rules:**
- The form class is `partial` — it pairs with its `.Designer.cs`.
- No `using CMS.Services` or `using CMS.Data` — ever.
- The form does NOT create the presenter with `new XxxPresenter(...)`. It only stores and calls it.
- Event handlers only gather UI input and delegate to the presenter. No business logic.
- `ShowError` / `ShowInfo` always use `MessageBox.Show`.
- `SelectedXxx` reads from `Tag` set during the list-population method.

---

### STEP 11 — CMS.Presentation: Visual Designer

**File:** `CMS.Presentation/Modules/{Module}/{FormBaseName}Form.Designer.cs`

```csharp
namespace CMS.Presentation.Modules.{Module}
{
    partial class {FormBaseName}Form
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.listView{EntityName} = new System.Windows.Forms.ListView();
            this.txt{Field}           = new System.Windows.Forms.TextBox();
            this.btnAdd               = new System.Windows.Forms.Button();
            this.btnEdit              = new System.Windows.Forms.Button();
            this.btnDelete            = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // listView{EntityName}
            this.listView{EntityName}.FullRowSelect = true;
            this.listView{EntityName}.GridLines     = true;
            this.listView{EntityName}.Location      = new System.Drawing.Point(12, 12);
            this.listView{EntityName}.Size          = new System.Drawing.Size(560, 300);
            this.listView{EntityName}.View          = System.Windows.Forms.View.Details;
            this.listView{EntityName}.Columns.Add("Name", 540);

            // txt{Field}
            this.txt{Field}.Location = new System.Drawing.Point(12, 325);
            this.txt{Field}.Size     = new System.Drawing.Size(350, 23);

            // btnAdd
            this.btnAdd.Location = new System.Drawing.Point(370, 323);
            this.btnAdd.Size     = new System.Drawing.Size(60, 27);
            this.btnAdd.Text     = "Add";
            this.btnAdd.Click   += new System.EventHandler(this.btnAdd_Click);

            // btnEdit
            this.btnEdit.Location = new System.Drawing.Point(438, 323);
            this.btnEdit.Size     = new System.Drawing.Size(60, 27);
            this.btnEdit.Text     = "Edit";
            this.btnEdit.Click   += new System.EventHandler(this.btnEdit_Click);

            // btnDelete
            this.btnDelete.Location = new System.Drawing.Point(506, 323);
            this.btnDelete.Size     = new System.Drawing.Size(66, 27);
            this.btnDelete.Text     = "Delete";
            this.btnDelete.Click   += new System.EventHandler(this.btnDelete_Click);

            // {FormBaseName}Form
            this.ClientSize = new System.Drawing.Size(600, 370);
            this.Controls.Add(this.listView{EntityName});
            this.Controls.Add(this.txt{Field});
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnDelete);
            this.Name  = "{FormBaseName}Form";
            this.Text  = "{FormBaseName}";
            this.Load += new System.EventHandler(this.{FormBaseName}Form_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ListView listView{EntityName};
        private System.Windows.Forms.TextBox  txt{Field};
        private System.Windows.Forms.Button   btnAdd;
        private System.Windows.Forms.Button   btnEdit;
        private System.Windows.Forms.Button   btnDelete;
    }
}
```

**Rules:**
- `partial class` — same namespace and class name as the code-behind.
- Always include the standard `Dispose` + `IContainer components`.
- All event handler wiring is done inside `InitializeComponent()` with `+=`.
- Control field declarations appear at the bottom of the file.
- Scale the UI (more columns, panels, GroupBoxes, labels, etc.) to match the actual form design.

---

### STEP 12 — Composition Root Wiring

Add a method in `MDI_Container.cs` (or a `FormFactory` class) to wire the full object graph.  
This is the **only place** services and repositories are instantiated inside `CMS.Presentation`.

```csharp
// Example method in MDI_Container.cs
// CMS.Presentation is allowed to reference CMS.Services and CMS.Data ONLY here.
private void Open{FormBaseName}Form()
{
    // 1. Build from the bottom up
    var repo      = new {EntityName}Repository(_configManager);           // CMS.Data
    var service   = new {FormBaseName}Service(repo);                      // CMS.Services
    // 2. Create form first (it is the IView)
    var form      = new {FormBaseName}Form(_currentUser);                 // CMS.Presentation
    // 3. Create presenter with form as the view
    var presenter = new {FormBaseName}Presenter(service, form, _currentUser); // CMS.Controllers
    // 4. Inject presenter into form
    form.AttachPresenter(presenter);
    // 5. Open
    form.Show();
}
```

---

### STEP 13 — Update .csproj Files

Add a `<Compile Include="..." />` entry for every new file.

**CMS.Domain.csproj**
```xml
<Compile Include="Models\{EntityName}.cs" />
```

**CMS.Data.csproj**
```xml
<Compile Include="Interfaces\I{EntityName}Repository.cs" />
<Compile Include="Repositories\{EntityName}Repository.cs" />
```

**CMS.Services.csproj**
```xml
<Compile Include="Interfaces\I{FormBaseName}Service.cs" />
<Compile Include="{FormBaseName}Service.cs" />
```

**CMS.Controllers.csproj**
```xml
<Compile Include="{Module}\Interfaces\I{FormBaseName}View.cs" />
<Compile Include="{Module}\{SubModule}\{FormBaseName}Presenter.cs" />
```

**CMS.Presentation.csproj**
```xml
<Compile Include="Modules\{Module}\{FormBaseName}Form.cs">
  <SubType>Form</SubType>
</Compile>
<Compile Include="Modules\{Module}\{FormBaseName}Form.Designer.cs">
  <DependentUpon>{FormBaseName}Form.cs</DependentUpon>
</Compile>
```

**Adding a new `<ProjectReference>` (if not already present):**
```xml
<ProjectReference Include="..\CMS.Services\CMS.Services.csproj">
  <Project>{GUID-from-target-csproj}</Project>
  <Name>CMS.Services</Name>
</ProjectReference>
```
> Look up the GUID from the target project's `<ProjectGuid>` element in its `.csproj`.

---

### STEP 14 — Build and Verify

```bash
msbuild CMS.sln /p:Configuration=Debug /nologo /v:minimal
```

Fix all errors before delivering. Common failure causes:

| Symptom | Likely cause |
|---|---|
| `The type or namespace ... could not be found` | Missing `using`, wrong namespace, or missing `<ProjectReference>` |
| `does not implement interface member` | Method or property missing from the form's `IXxxView` implementation |
| `partial declarations must not specify different base classes` | Namespace or class name mismatch between `*.cs` and `*.Designer.cs` |
| `<Compile>` warning or missing type | File created on disk but not added to `.csproj` |

---

## Quick Reference Checklist

Before closing the task, confirm every item:

- [ ] `{EntityName}.cs` in `CMS.Domain/Models/`
- [ ] `I{EntityName}Repository.cs` in `CMS.Data/Interfaces/`
- [ ] `{EntityName}Repository.cs` in `CMS.Data/Repositories/`
- [ ] `I{FormBaseName}Service.cs` in `CMS.Services/Interfaces/`
- [ ] `{FormBaseName}Service.cs` in `CMS.Services/`
- [ ] `I{FormBaseName}View.cs` in `CMS.Controllers/{Module}/Interfaces/`
- [ ] `{FormBaseName}Presenter.cs` in `CMS.Controllers/{Module}/{SubModule}/`
- [ ] `{FormBaseName}Form.cs` in `CMS.Presentation/Modules/{Module}/`
- [ ] `{FormBaseName}Form.Designer.cs` in `CMS.Presentation/Modules/{Module}/`
- [ ] Form code-behind has NO `using CMS.Services` or `using CMS.Data`
- [ ] `AttachPresenter()` method exists on the form
- [ ] Composition root wiring added in `MDI_Container.cs` or factory
- [ ] All `.csproj` files updated with `<Compile>` entries
- [ ] Solution builds with zero errors
