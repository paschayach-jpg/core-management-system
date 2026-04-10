# CMS Solution — Project Intelligence

## Overview

This is a **layered .NET Framework 4.5 Windows Forms application** implementing the **MVP (Model-View-Presenter)** pattern with full separation of concerns across 6 projects.

**Build tool:** MSBuild via Visual Studio 2022 Professional  
**Test framework:** NUnit + NSubstitute  
**Data access:** ADO.NET with SQL Server stored procedures (no EF/LINQ)  
**UI framework:** WinForms

---

## Architecture & Layer Responsibilities

```
CMS.Presentation     ← UI only: forms, event handlers, implements IXxxView
       ↓
CMS.Controllers      ← MVP Presenters: mediates view ↔ services; holds IXxxView interfaces
       ↓
CMS.Services         ← Business logic: orchestrates repositories, validates, caches
       ↓
CMS.Data             ← Repository pattern: ADO.NET stored-procedure calls
       ↓
CMS.Domain           ← Domain models (POCOs), enums — no dependencies on other CMS layers
CMS.Infrastructure   ← Cross-cutting: config, logging, AD, virus scan, network drive
```

**Strict rule:** each layer only depends on the layers below it. CMS.Presentation knows everything; CMS.Domain knows nothing.

---

## Project Responsibilities (MUST follow when adding code)

| Project | Responsibility | What lives here |
|---|---|---|
| **CMS.Presentation** | Display UI, handle event callbacks | Forms (`.cs` + `.Designer.cs`), implement `IXxxView` interfaces |
| **CMS.Controllers** | Receive UI commands, delegate to services | `XxxPresenter.cs`, `IXxxView.cs` interfaces, sub-controller classes |
| **CMS.Services** | Business logic, validation, caching | `XxxService.cs`, `IXxxService.cs` interfaces, DTOs in `DTOs/` |
| **CMS.Domain** | Mapping objects from DB / CRUD operations | Models in `Models/`, Enums in `Enums/` |
| **CMS.Data** | DB access via ADO.NET stored procedures | `XxxRepository.cs`, `IXxxRepository.cs` interfaces |
| **CMS.Infrastructure** | Config, logging, AD, file system helpers | `ConfigManager`, `LoggingService`, `ActiveDirectoryHelper`, etc. |

---

## Module Naming Conventions

The solution is divided into main modules. Use these identifiers consistently for folder names:

| Module Key | Full Name | Example Path |
|---|---|---|
| `DMS` | Document Management System | `CMS.Controllers/DMS/Document/` |
| `KMS` | Knowledge Management System | `CMS.Controllers/KMS/KB/` |

Sub-modules follow inside the main module folder (e.g., `KB` = Knowledge Base under KMS).

---

## Naming Rules

### View Interfaces (CMS.Controllers)
- Location: `CMS.Controllers\{MainModule}\Interfaces\`
- Suffix: `View` — e.g., `IArticleEditorDialogView`, `IKBCategoryView`, `IFolderView`

### Presenters / Controllers (CMS.Controllers)
- Location: `CMS.Controllers\{MainModule}\{SubModule}\`
- Suffix: `Presenter` or `Controller` — e.g., `ArticleEditorDialogPresenter.cs`, `KBCategoryPresenter.cs`

### Service Interfaces (CMS.Services)
- Location: `CMS.Services\Interfaces\`
- Suffix: `Service` — e.g., `IKBArticleService`, `IKBCategoryService`

### Service Implementations (CMS.Services)
- Location: `CMS.Services\` (project root)
- Suffix: `Service` — e.g., `KBArticleService.cs`, `KBCategoryService.cs`

### Repository Interfaces (CMS.Data)
- Location: `CMS.Data\Interfaces\`
- Suffix: `Repository` — e.g., `IKBArticleRepository`

### Repository Implementations (CMS.Data)
- Location: `CMS.Data\Repositories\`
- Suffix: `Repository` — e.g., `KBArticleRepository.cs`

### Domain Models (CMS.Domain)
- Location: `CMS.Domain\Models\`
- No suffix — e.g., `KBArticle.cs`, `KBCategory.cs`

### Presentation Forms (CMS.Presentation)
- Location: `CMS.Presentation\Modules\{ModuleKey}\`
- Suffix: `Form` for main forms, `Dialog` for dialogs — e.g., `KnowledgeBaseForm.cs`, `ArticleEditorDialog.cs`
- Each form has two files: `XxxForm.cs` (code-behind) + `XxxForm.Designer.cs` (visual designer)

---

## Code Patterns

### Domain Model (CMS.Domain/Models)
```csharp
public class KBArticle
{
    public int ArticleID { get; set; }
    public string Title { get; set; }
    public int CategoryID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }

    // Alias for compatibility
    public int ArticleId { get { return ArticleID; } set { ArticleID = value; } }

    // Transient (populated by service layer, not DB)
    public string CategoryName { get; set; }
    public string CreatedByName { get; set; }

    // Enum conversion property (if applicable)
    // public StatusEnum StatusEnum
    // {
    //     get { return (StatusEnum)Status; }
    //     set { Status = (int)value; }
    // }
}
```

### Repository Interface (CMS.Data/Interfaces)
```csharp
public interface IKBArticleRepository
{
    int Insert(KBArticle article);
    void Update(KBArticle article);
    void Delete(int articleId);
    KBArticle GetById(int articleId);
    IEnumerable<KBArticle> GetByCategoryId(int categoryId);
    IEnumerable<KBArticle> GetAll();
}
```

### Repository Implementation (CMS.Data/Repositories)
```csharp
public class KBArticleRepository : IKBArticleRepository
{
    private readonly string _connectionString;

    public KBArticleRepository(IConfigManager configManager)
    {
        _connectionString = configManager.Get<string>("ConnectionString");
    }

    public int Insert(KBArticle article)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand("km.sp_InsertKBArticle", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Title", article.Title);
                cmd.Parameters.AddWithValue("@CategoryID", article.CategoryID);
                cmd.Parameters.AddWithValue("@CreatedBy", article.CreatedBy);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
    // ...
}
```
**SP naming convention:** `{schema}.sp_{VerbEntity}` — e.g., `km.sp_InsertKBArticle`, `storage.sp_GetFolderById`  
**Null handling:** `(object)nullable ?? DBNull.Value` for optional parameters.

### Service Interface (CMS.Services/Interfaces)
```csharp
public interface IKBArticleService
{
    KBArticle GetArticleById(int articleId, UserContext user);
    IEnumerable<KBArticle> GetArticlesByCategory(int categoryId, UserContext user);
    KBArticle CreateArticle(CreateKBArticleRequest request, UserContext user);
    void UpdateArticle(KBArticle article, UserContext user);
    void DeleteArticle(int articleId, UserContext user);
}
```

### Service Implementation (CMS.Services)
```csharp
public class KBArticleService : IKBArticleService
{
    private readonly IKBArticleRepository _articleRepository;
    private readonly IKBCategoryRepository _categoryRepository;
    private readonly IAuditService _auditService;

    public KBArticleService(
        IKBArticleRepository articleRepository,
        IKBCategoryRepository categoryRepository,
        IAuditService auditService)
    {
        if (articleRepository == null) throw new ArgumentNullException("articleRepository");
        if (categoryRepository == null) throw new ArgumentNullException("categoryRepository");
        if (auditService == null) throw new ArgumentNullException("auditService");

        _articleRepository = articleRepository;
        _categoryRepository = categoryRepository;
        _auditService = auditService;
    }

    public KBArticle GetArticleById(int articleId, UserContext user)
    {
        if (user == null) throw new ArgumentNullException("user");
        var article = _articleRepository.GetById(articleId);
        if (article == null) throw new KeyNotFoundException("Article not found.");
        // Populate transient fields
        article.CategoryName = _categoryRepository.GetById(article.CategoryID)?.Name;
        return article;
    }
}
```
- Always null-check all constructor parameters.
- Use session-level `Dictionary` caching with an `InvalidateXxxCache()` method when needed.
- Business rules and validation live here, NOT in the presenter or form.

### View Interface (CMS.Controllers/{Module}/Interfaces)
```csharp
public interface IKBCategoryView
{
    void ShowCategories(IEnumerable<KBCategory> categories);
    void ShowError(string message);
    void ShowInfo(string message);

    KBCategory SelectedCategory { get; }
}
```
- Interface contains only the methods the presenter needs to talk to the form.
- Properties for currently selected items, never full UI controls.

### Presenter (CMS.Controllers/{Module}/{SubModule})
```csharp
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
            var categories = _service.GetAllCategories(_user);
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

    public void AddCategory(string name)
    {
        try
        {
            _service.CreateCategory(name, _user);
            LoadCategories(); // refresh
        }
        catch (Exception ex)
        {
            _view.ShowError("Failed to add category: " + ex.Message);
        }
    }
}
```

### Form — Code-behind (CMS.Presentation/Modules/{Module})
```csharp
public partial class KBCategoryForm : Form, IKBCategoryView
{
    private readonly IKBCategoryService _categoryService;
    private readonly UserContext _user;
    private KBCategoryPresenter _presenter;
    private List<KBCategory> _categories = new List<KBCategory>();

    public KBCategoryForm(IKBCategoryService categoryService, UserContext user)
    {
        InitializeComponent();
        _categoryService = categoryService;
        _user = user;
        _presenter = new KBCategoryPresenter(_categoryService, this, _user);
    }

    private void KBCategoryForm_Load(object sender, EventArgs e)
    {
        _presenter.LoadCategories();
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
        _presenter.AddCategory(txtCategoryName.Text);
    }

    // --- IKBCategoryView implementation ---
    public void ShowCategories(IEnumerable<KBCategory> categories)
    {
        _categories = categories.ToList();
        listViewCategories.Items.Clear();
        foreach (var cat in _categories)
        {
            var item = new ListViewItem(cat.Name) { Tag = cat };
            listViewCategories.Items.Add(item);
        }
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
```

### Form — Visual Designer (auto-generated, do not edit manually)
`KBCategoryForm.Designer.cs` — generated by Visual Studio Windows Forms Designer.  
Contains `InitializeComponent()` with all control declarations and property assignments.

---

## .csproj Reference Rules

When adding a new file to any project, ensure the `.csproj` includes a `<Compile Include="..." />` entry.

When a project needs to reference a new layer, add a `<ProjectReference>` pointing to the target `.csproj`.

**Current project dependency graph (allowed references only):**

```
CMS.Presentation  → CMS.Controllers, CMS.Services, CMS.Data, CMS.Domain, CMS.Infrastructure
CMS.Controllers   → CMS.Services, CMS.Data, CMS.Domain, CMS.Infrastructure
CMS.Services      → CMS.Data, CMS.Domain, CMS.Infrastructure
CMS.Data          → CMS.Domain, CMS.Infrastructure
CMS.Domain        → (none)
CMS.Infrastructure→ CMS.Domain
```

---

## Build & Verification

Build the full solution from the repo root using MSBuild:
```
msbuild CMS.sln /p:Configuration=Debug /nologo /v:minimal
```

Always build after generating code to confirm there are no compile errors.
