# CMS Solution — Project Intelligence

## Developer Context

You are a **.NET Full-Stack Developer** with deep expertise in the **insurance business domain**.
- **Runtime:** .NET Framework 4.5
- **UI:** Windows Forms (WinForms)
- **Database:** SQL Server 2019, accessed exclusively via **ADO.NET** (no EF, no LINQ-to-SQL)
- **Pattern:** MVP (Model-View-Presenter) with strict layered separation
- **Build tool:** MSBuild via Visual Studio 2022 Professional
- **Test framework:** NUnit + NSubstitute

> **Note:** The solution structure is currently incomplete. Projects may be missing folders,
> `.csproj` entries, or inter-project references. Always verify and add them when generating code.

---

## Architecture — Layer Flow

```
CMS.Presentation
      │  UI only: WinForms, event handlers, implements IXxxView
      │  ✅ References: CMS.Controllers, CMS.Domain
      │  ❌ Must NOT reference: CMS.Services, CMS.Data
      ▼
CMS.Controllers
      │  MVP Presenters: receives commands from Presentation, calls Services
      │  ✅ References: CMS.Services, CMS.Domain, CMS.Infrastructure
      ▼
CMS.Services
      │  All business logic: validates, orchestrates, applies rules
      │  ✅ References: CMS.Data, CMS.Domain, CMS.Infrastructure
      ▼
CMS.Data
      │  ADO.NET repositories: stored-procedure calls to SQL Server
      │  ✅ References: CMS.Domain, CMS.Infrastructure
      ▼
CMS.Domain          ← Models (POCOs), Enums — no dependencies on other CMS projects
CMS.Infrastructure  ← Cross-cutting: config, logging, AD, file system
```

**Strict rules:**
- `CMS.Presentation` must have **no `ProjectReference`** to `CMS.Services` or `CMS.Data`.
- `CMS.Presentation` individual form files must have **no `using CMS.Services` or `using CMS.Data`**.
- Individual form files receive a pre-built **Presenter** from `CMS.Controllers` — they never `new` up services or repositories.
- The only place services and repositories are instantiated is the **composition root** (e.g., `MDI_Container.cs`), which is the application entry point.

---

## Project Responsibilities

| Project | Responsibility | What lives here |
|---|---|---|
| **CMS.Presentation** | Display UI, handle event callbacks | Forms (`*.cs` + `*.Designer.cs`), implements `IXxxView` |
| **CMS.Controllers** | Receive UI commands, delegate to services | `XxxPresenter.cs`, `IXxxView.cs` interfaces |
| **CMS.Services** | Business logic, validation, caching | `XxxService.cs`, `IXxxService.cs`, DTOs in `DTOs/` |
| **CMS.Data** | DB access via ADO.NET stored procedures | `XxxRepository.cs` in `Repositories/`, `IXxxRepository.cs` in `Interfaces/` |
| **CMS.Domain** | Domain model classes (POCOs), Enums | `Models/`, `Enums/` |
| **CMS.Infrastructure** | Config, logging, AD, file system helpers | `ConfigManager`, `LoggingService`, `ActiveDirectoryHelper` |

---

## Module Naming Conventions

Modules define the top-level folder grouping inside each project:

| Module Key | Full Name | Example path in CMS.Controllers |
|---|---|---|
| `DMS` | Document Management System | `CMS.Controllers/DMS/Document/` |
| `KMS` | Knowledge Management System | `CMS.Controllers/KMS/KB/` |

Sub-modules sit inside the module folder (e.g., `KB` = Knowledge Base, under `KMS`).

---

## Naming Rules

### Domain Models — `CMS.Domain/Models/`
- No suffix — e.g., `KBArticle.cs`, `KBCategory.cs`, `Folder.cs`

### Repository Interfaces — `CMS.Data/Interfaces/`
- Suffix `Repository` — e.g., `IKBArticleRepository.cs`

### Repository Implementations — `CMS.Data/Repositories/`
- Suffix `Repository` — e.g., `KBArticleRepository.cs`

### Service Interfaces — `CMS.Services/Interfaces/`
- Suffix `Service` — e.g., `IKBArticleService.cs`

### Service Implementations — `CMS.Services/` (root)
- Suffix `Service` — e.g., `KBArticleService.cs`

### View Interfaces — `CMS.Controllers/{Module}/Interfaces/`
- Suffix `View` — e.g., `IKBCategoryView.cs`, `IArticleEditorDialogView.cs`

### Presenters — `CMS.Controllers/{Module}/{SubModule}/`
- Suffix `Presenter` or `Controller` — e.g., `KBCategoryPresenter.cs`

### Forms — `CMS.Presentation/Modules/{Module}/`
- Suffix `Form` for main windows, `Dialog` for modal dialogs
- Always two files per form: `XxxForm.cs` (code-behind) + `XxxForm.Designer.cs` (designer)

---

## Code Patterns

### Domain Model (`CMS.Domain/Models/`)

```csharp
namespace CMS.Domain.Models
{
    public class KBCategory
    {
        // Primary key — PascalCase with full suffix
        public int CategoryID { get; set; }

        // Core fields
        public string Name { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }

        // Alias for backward compatibility
        public int CategoryId
        {
            get { return CategoryID; }
            set { CategoryID = value; }
        }

        // Transient fields — populated by service layer, NOT stored in DB
        public string CreatedByName { get; set; }

        // Enum conversion (only when entity has a status/type int column)
        // public SomeEnum SomeField
        // {
        //     get { return (SomeEnum)SomeFieldInt; }
        //     set { SomeFieldInt = (int)value; }
        // }
    }
}
```

### Repository Interface (`CMS.Data/Interfaces/`)

```csharp
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IKBCategoryRepository
    {
        int Insert(KBCategory category);
        void Update(KBCategory category);
        void Delete(int categoryId);
        KBCategory GetById(int categoryId);
        IEnumerable<KBCategory> GetAll();
    }
}
```

### Repository Implementation (`CMS.Data/Repositories/`)

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
    public class KBCategoryRepository : IKBCategoryRepository
    {
        private readonly string _connectionString;

        public KBCategoryRepository(IConfigManager configManager)
        {
            _connectionString = configManager.Get<string>("ConnectionString");
        }

        public int Insert(KBCategory category)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("km.sp_InsertKBCategory", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", category.Name);
                    cmd.Parameters.AddWithValue("@Description",
                        (object)category.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy", category.CreatedBy);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public KBCategory GetById(int categoryId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("km.sp_GetKBCategoryById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapRow(reader);
                        return null;
                    }
                }
            }
        }

        public IEnumerable<KBCategory> GetAll()
        {
            var result = new System.Collections.Generic.List<KBCategory>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("km.sp_GetAllKBCategories", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            result.Add(MapRow(reader));
                    }
                }
            }
            return result;
        }

        public void Update(KBCategory category) { /* similar pattern */ }
        public void Delete(int categoryId)      { /* similar pattern */ }

        private KBCategory MapRow(SqlDataReader reader)
        {
            return new KBCategory
            {
                CategoryID  = Convert.ToInt32(reader["CategoryID"]),
                Name        = reader["Name"].ToString(),
                Description = reader["Description"] as string,
                CreatedBy   = Convert.ToInt32(reader["CreatedBy"])
            };
        }
    }
}
```

**Stored procedure naming:** `{schema}.sp_{VerbEntity}` — e.g., `km.sp_InsertKBCategory`, `storage.sp_GetFolderById`  
**Null parameters:** `(object)nullable ?? DBNull.Value`  
**Mapping:** always in a private `MapRow(SqlDataReader)` helper method.

### Service Interface (`CMS.Services/Interfaces/`)

```csharp
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IKBCategoryService
    {
        IEnumerable<KBCategory> GetAll(UserContext user);
        KBCategory GetById(int categoryId, UserContext user);
        void Create(KBCategory category, UserContext user);
        void Update(KBCategory category, UserContext user);
        void Delete(int categoryId, UserContext user);
    }
}
```

### Service Implementation (`CMS.Services/`)

```csharp
using System;
using System.Collections.Generic;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class KBCategoryService : IKBCategoryService
    {
        private readonly IKBCategoryRepository _categoryRepository;

        public KBCategoryService(IKBCategoryRepository categoryRepository)
        {
            if (categoryRepository == null) throw new ArgumentNullException("categoryRepository");
            _categoryRepository = categoryRepository;
        }

        public IEnumerable<KBCategory> GetAll(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return _categoryRepository.GetAll();
        }

        public KBCategory GetById(int categoryId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            var category = _categoryRepository.GetById(categoryId);
            if (category == null) throw new KeyNotFoundException("Category not found.");
            return category;
        }

        public void Create(KBCategory category, UserContext user)
        {
            if (category == null) throw new ArgumentNullException("category");
            if (user == null) throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Category name is required.");
            // All business rules live here, not in the presenter or form
            _categoryRepository.Insert(category);
        }

        public void Update(KBCategory category, UserContext user)
        {
            if (category == null) throw new ArgumentNullException("category");
            if (user == null) throw new ArgumentNullException("user");
            _categoryRepository.Update(category);
        }

        public void Delete(int categoryId, UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _categoryRepository.Delete(categoryId);
        }
    }
}
```

**Rules:**
- Null-check every constructor parameter and every public method parameter.
- All validation and business rules belong here — never in the Presenter or Form.
- Use session-level `Dictionary` caches with an `InvalidateXxxCache()` method where performance requires it.

### View Interface (`CMS.Controllers/{Module}/Interfaces/`)

```csharp
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.KMS.Interfaces
{
    public interface IKBCategoryView
    {
        // Presenter → View: push data into the form
        void ShowCategories(IEnumerable<KBCategory> categories);
        void ShowError(string message);
        void ShowInfo(string message);

        // View → Presenter: expose current UI selection (never expose controls directly)
        KBCategory SelectedCategory { get; }
    }
}
```

### Presenter (`CMS.Controllers/{Module}/{SubModule}/`)

```csharp
using System;
using CMS.Controllers.KMS.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.KMS.KB
{
    public class KBCategoryPresenter
    {
        private readonly IKBCategoryService _service;
        private readonly IKBCategoryView _view;
        private readonly UserContext _user;

        public KBCategoryPresenter(
            IKBCategoryService service,
            IKBCategoryView view,
            UserContext user)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (view == null) throw new ArgumentNullException("view");
            if (user == null) throw new ArgumentNullException("user");

            _service = service;
            _view = view;
            _user = user;
        }

        public void LoadCategories()
        {
            try
            {
                var categories = _service.GetAll(_user);
                _view.ShowCategories(categories);
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowError("Access denied: " + ex.Message);
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to load categories: " + ex.Message);
            }
        }

        public void AddCategory(KBCategory category)
        {
            try
            {
                _service.Create(category, _user);
                LoadCategories();
                _view.ShowInfo("Category created.");
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
        }

        public void EditCategory(KBCategory category)
        {
            try
            {
                _service.Update(category, _user);
                LoadCategories();
                _view.ShowInfo("Category updated.");
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
        }

        public void DeleteCategory(int categoryId)
        {
            try
            {
                _service.Delete(categoryId, _user);
                LoadCategories();
                _view.ShowInfo("Category deleted.");
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
        }
    }
}
```

### Form — Code-behind (`CMS.Presentation/Modules/{Module}/`)

```csharp
// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CMS.Controllers.KMS.Interfaces;
using CMS.Controllers.KMS.KB;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    public partial class KBCategoryForm : Form, IKBCategoryView
    {
        private KBCategoryPresenter _presenter;
        private readonly UserContext _user;
        private List<KBCategory> _categories = new List<KBCategory>();

        // Presenter is NOT created here; it is injected by the composition root
        public KBCategoryForm(UserContext user)
        {
            InitializeComponent();
            _user = user;
        }

        /// <summary>
        /// Called by the composition root (MDI_Container or factory) after the presenter
        /// is fully wired. Must be called before the form is shown.
        /// </summary>
        public void AttachPresenter(KBCategoryPresenter presenter)
        {
            _presenter = presenter;
        }

        // ── Form events ──────────────────────────────────────────────────
        private void KBCategoryForm_Load(object sender, EventArgs e)
        {
            _presenter.LoadCategories();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var category = new KBCategory
            {
                Name      = txtName.Text.Trim(),
                CreatedBy = _user.UserId
            };
            _presenter.AddCategory(category);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (SelectedCategory == null) { ShowInfo("Please select a category."); return; }
            var category  = SelectedCategory;
            category.Name = txtName.Text.Trim();
            _presenter.EditCategory(category);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (SelectedCategory == null) { ShowInfo("Please select a category."); return; }
            var confirm = MessageBox.Show(
                "Delete selected category?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
                _presenter.DeleteCategory(SelectedCategory.CategoryID);
        }

        // ── IKBCategoryView ──────────────────────────────────────────────
        public void ShowCategories(IEnumerable<KBCategory> categories)
        {
            _categories = new List<KBCategory>(categories);
            listViewCategories.Items.Clear();
            foreach (var cat in _categories)
                listViewCategories.Items.Add(new ListViewItem(cat.Name) { Tag = cat });
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public KBCategory SelectedCategory
        {
            get
            {
                if (listViewCategories.SelectedItems.Count == 0) return null;
                return listViewCategories.SelectedItems[0].Tag as KBCategory;
            }
        }
    }
}
```

### Composition Root (wiring — in `MDI_Container.cs` or a dedicated factory)

```csharp
// This is the ONLY place where services and repositories are instantiated.
// Lives in CMS.Presentation but is acceptable because it is the composition root.
private void OpenKBCategoryForm()
{
    var repo      = new KBCategoryRepository(_configManager);
    var service   = new KBCategoryService(repo);
    var form      = new KBCategoryForm(_currentUser);
    var presenter = new KBCategoryPresenter(service, form, _currentUser);
    form.AttachPresenter(presenter);
    form.Show();
}
```

### Form — Visual Designer (auto-generated, do not edit manually)

`KBCategoryForm.Designer.cs` — generated by Visual Studio Windows Forms Designer. Contains `InitializeComponent()` with all control declarations, layout, and event handler wiring.

---

## .csproj Reference Rules

Add a `<Compile Include="..." />` entry for every new source file.  
Add a `<ProjectReference>` when a project needs access to another.

**Allowed project references:**

```
CMS.Presentation  → CMS.Controllers, CMS.Domain, CMS.Infrastructure
CMS.Controllers   → CMS.Services, CMS.Domain, CMS.Infrastructure
CMS.Services      → CMS.Data, CMS.Domain, CMS.Infrastructure
CMS.Data          → CMS.Domain, CMS.Infrastructure
CMS.Domain        → (none)
CMS.Infrastructure→ CMS.Domain
```

> CMS.Presentation references CMS.Services and CMS.Data **only in the composition root** (`MDI_Container.cs` / bootstrapper). If those references don't already exist in the `.csproj`, add them with a comment.

---

## Build & Verification

```
msbuild CMS.sln /p:Configuration=Debug /nologo /v:minimal
```

Always build after generating code. Fix all errors before delivering.
