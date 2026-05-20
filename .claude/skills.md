# CMS Solution — Developer Skills Guide
> C# .NET 4.5 · Windows Forms · SQL Server · ADO.NET · MVP Pattern

---

## 1. Architecture Overview

```
CMS.Presentation   ← Form (UI only, event handlers only)
CMS.Controllers    ← Presenter / Controller (MVP logic, no DB)
CMS.Services       ← Business logic (rules, validation, orchestration)
CMS.Data           ← Repository (ADO.NET, SQL Server only)
CMS.Domain         ← Models + Enums (no logic, no dependencies)
CMS.Infrastructure ← Cross-cutting (logging, AD, config)
```

**Dependency direction (strict, one-way):**
```
Presentation → Controllers → Services → Data → Domain
                                              ↑
                                        Infrastructure
```

**Wire-up location:** `CMS.Presentation/Program.cs`
All `new` for repos/services/controllers happens here. No `new` inside forms.

**Base MDI host:** `CMS.Presentation/MDI_Container` — all module forms open as MDI children.

---

## 2. Creating a New Form — Full Checklist

Follow the KBImportWizard (`KBImportWizardForm`) as the canonical example.

### Step 1 — Domain (CMS.Domain)

**Enums** → `CMS.Domain/Enums/<FeatureName>Enum.cs`
```csharp
public enum KBCategoryCodeName : byte { LW = 1 }
```

**Models** → `CMS.Domain/Models/<FeatureName>.cs`
- Plain POCO, no logic beyond computed read-only properties
- No service/repository references
```csharp
public class KBImportRow {
    public int RowNumber { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }   // "Valid" | "Warn" | "Error"
    // computed only:
    public string ArticleCode { get; }
}
```

---

### Step 2 — Data (CMS.Data)

**Interface** → `CMS.Data/Interfaces/I<Entity>Repository.cs`
```csharp
public interface IKBArticleRepository {
    IEnumerable<KBArticle> GetAll();
    KBArticle GetById(int id);
    int Insert(KBArticle article);
    void Update(KBArticle article);
}
```

**Implementation** → `CMS.Data/Repositories/<Entity>Repository.cs`
- Use ADO.NET only (`SqlConnection`, `SqlCommand`, `SqlDataReader`)
- `using` blocks for every connection
- No business logic here — pure SQL/mapping

---

### Step 3 — Services (CMS.Services)

**Interface** → `CMS.Services/Interfaces/I<Feature>Service.cs`
```csharp
public interface IKBArticleService {
    IEnumerable<KBArticle> GetAll();
    int Create(KBArticle article);
    void ChangeStatus(int id, byte status, string actionBy);
}
```

**Implementation** → `CMS.Services/<Feature>Service.cs`
- Inject repos via constructor
- All business rules, validation, orchestration live here
- Throws meaningful exceptions (not returning bool/null silently)

**Helper classes** (no interface needed) → `CMS.Services/<FeatureHelper>.cs`
```csharp
// Example: KBExcelReader, KBImportProcessor
public class KBImportProcessor {
    private readonly IKBCategoryRepository _categoryRepo;
    private readonly IKBArticleRepository _articleRepo;
    public KBImportProcessor(IKBCategoryRepository c, IKBArticleRepository a) { ... }
    public void ValidateRows(IList<KBImportRow> rows) { ... }
    public void InsertArticles(IList<KBImportRow> rows, string createdBy, ...) { ... }
}
```

---

### Step 4 — Controllers (CMS.Controllers)

**Two interfaces per feature:**

**View contract** → `CMS.Controllers/<Module>/Interfaces/I<Feature>View.cs`
(Properties the form exposes, methods the presenter calls)
```csharp
public interface IKBImportWizardView {
    string SelectedFilePath { get; }
    bool OptSkipWarning { get; }
    void ShowValidationResult(KBImportValidationResult result);
    void ShowImportComplete(KBImportValidationResult summary);
    void ShowError(string message);
    void SetStatus(string text);
}
```

**Controller contract** → `CMS.Controllers/<Module>/Interfaces/I<Feature>Controller.cs`
(Methods the form calls on user action)
```csharp
public interface IKBImportWizardController {
    void ValidateFile();
    void ExecuteImport();
    void Cancel();
}
```

**Presenter** → `CMS.Controllers/<Module>/<Feature>Controller.cs`
```csharp
public class KBImportWizardController : IKBImportWizardController {
    private readonly IKBImportWizardView _view;
    private readonly IKBArticleService _articleService;
    private readonly KBImportProcessor _processor;

    public KBImportWizardController(
        IKBImportWizardView view,
        IKBArticleService articleService,
        KBImportProcessor processor) { ... }

    public void ValidateFile() {
        var result = _processor.ValidateRows(...);
        _view.ShowValidationResult(result);
    }
}
```

---

### Step 5 — Presentation (CMS.Presentation)

**Two files per form (mandatory):**

#### `<FeatureForm>.cs` — Code-behind (event handlers ONLY)
```csharp
public partial class KBImportWizardForm : Form,
    IKBImportWizardView       // View contract
{
    private KBImportWizardController _presenter;

    // ctor: accept dependencies, set up form state
    public KBImportWizardForm(IKBArticleService articleService,
                               IKBCategoryService categoryService,
                               KBImportProcessor processor,
                               UserContext user) {
        InitializeComponent();
        _presenter = new KBImportWizardController(this, articleService, processor);
    }

    // --- IKBImportWizardView properties ---
    public string SelectedFilePath => txtFilePath.Text.Trim();
    public bool OptSkipWarning => chkSkipWarning.Checked;

    // --- IKBImportWizardView methods ---
    public void ShowValidationResult(KBImportValidationResult r) {
        lblSummary.Text = $"Valid: {r.ValidRecords} / Total: {r.TotalRecords}";
        previewControl.BindRows(r.Rows);
    }
    public void ShowError(string msg) => MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    public void SetStatus(string text) => lblStatus.Text = text;

    // --- Event handlers (ONLY thing allowed here) ---
    private void btnBrowse_Click(object sender, EventArgs e) => _presenter.BrowseFile();
    private void btnValidate_Click(object sender, EventArgs e) => _presenter.ValidateFile();
    private void btnImport_Click(object sender, EventArgs e) => _presenter.ExecuteImport();
    private void btnCancel_Click(object sender, EventArgs e) => Close();
}
```

#### `<FeatureForm>.Designer.cs` — Visual designer (auto-generated, never edit manually)
- All control declarations, `InitializeComponent()`, layout code
- VS designer writes this file — treat as read-only

---

### Step 6 — Wire up in Program.cs

```csharp
// 1. Repos
var kbArticleRepo     = new KBArticleRepository(connectionString);
var kbCategoryRepo    = new KBCategoryRepository(connectionString);

// 2. Services
var kbArticleService  = new KBArticleService(kbArticleRepo, auditService);
var kbCategoryService = new KBCategoryService(kbCategoryRepo, auditService);

// 3. Helpers
var kbImportProcessor = new KBImportProcessor(kbCategoryRepo, kbArticleRepo, kbVersionRepo);

// 4. Inject into MDI_Container (or form factory)
var mdi = new MDI_Container(..., kbArticleService, kbCategoryService, kbImportProcessor, ...);
```

---

## 3. Opening a Form from MDI_Container

```csharp
// Inside MDI_Container — open as MDI child
private void OpenKBImportWizard() {
    var form = new KBImportWizardForm(
        _kbArticleService, _kbCategoryService, _kbImportProcessor, _user);
    form.MdiParent = this;
    form.Show();
}
```

---

## 4. Error Fix / Revision Rules

### Before touching any code, identify ALL affected layers:
| Layer changed | Must also check |
|---|---|
| Domain Model (add/rename property) | Repository mapping, Service logic, Controller usage, Form binding |
| Repository interface | Repository implementation, Service constructor, Program.cs wire-up |
| Service interface | Service implementation, Controller constructor, Program.cs wire-up |
| Controller interface | Presenter implementation, Form constructor, Form's view-contract impl |
| Form (view contract) | Controller calls to view methods, matching interface |

### Fix cascade order (bottom-up):
```
1. Domain  →  2. Data  →  3. Services  →  4. Controllers  →  5. Presentation  →  6. Program.cs
```
Never skip a layer. A compile error in any layer means the fix is incomplete.

### Best practices when revising:
- **Add a property/method to an interface** → implement it in ALL classes that implement that interface
- **Rename a method** → use VS "Rename" (F2) to catch all callers across projects
- **Change constructor signature** → update Program.cs wire-up immediately
- **Move logic out of event handler** → put it in the presenter, call presenter method from handler
- **Never** put `SqlCommand` / business rules in a Form or Controller
- **Never** put UI calls (`MessageBox`, control references) in Services or Repositories

---

## 5. Naming Conventions

| Artifact | Convention | Example |
|---|---|---|
| Form class | `<Feature>Form` | `KBImportWizardForm` |
| View interface | `I<Feature>View` | `IKBImportWizardView` |
| Controller interface | `I<Feature>Controller` | `IKBImportWizardController` |
| Presenter class | `<Feature>Controller` | `KBImportWizardController` |
| Service interface | `I<Entity>Service` | `IKBArticleService` |
| Repository interface | `I<Entity>Repository` | `IKBArticleRepository` |
| Repository implementation | `<Entity>Repository` | `KBArticleRepository` |
| Enum file | `<Feature>Enum` or descriptive | `KBCategoryCodeName` |
| Domain model | `<EntityName>` | `KBArticle`, `KBImportRow` |
| Helper/Processor | `<Feature><Role>` | `KBExcelReader`, `KBImportProcessor` |

---

## 6. File Location Quick Reference

```
CMS.Domain/
  Enums/           ← ALL enums go here
  Models/          ← ALL domain models (POCOs) go here

CMS.Data/
  Interfaces/      ← I<Entity>Repository.cs
  Repositories/    ← <Entity>Repository.cs (ADO.NET)

CMS.Services/
  Interfaces/      ← I<Feature>Service.cs
  <Feature>Service.cs
  <Feature>Helper.cs / <Feature>Processor.cs (no interface needed)

CMS.Controllers/
  <Module>/
    Interfaces/    ← I<Feature>View.cs + I<Feature>Controller.cs
    <Feature>Controller.cs

CMS.Presentation/
  Modules/<Module>/
    <Feature>Form.cs          ← code-behind (event handlers only)
    <Feature>Form.Designer.cs ← designer (auto-generated)
    <Feature>Form.resx        ← resources
    <Feature>Control.cs       ← UserControls (same rules apply)

CMS.Presentation/
  Program.cs       ← ONLY place that calls `new` on repos/services
  MDI_Container.cs ← base MDI host, injects dependencies into child forms
```

---

## 7. Code-Behind Rules (Hard Rules)

```csharp
// ✅ ALLOWED in Form.cs
private void btnSave_Click(object sender, EventArgs e) => _presenter.Save();
private void dgv_CellClick(object sender, DataGridViewCellEventArgs e) => _presenter.SelectRow(e.RowIndex);
public string InputText => txtInput.Text.Trim();          // view property
public void ShowResult(MyModel m) => lblResult.Text = m.Name; // view method

// ❌ NOT ALLOWED in Form.cs
private void btnSave_Click(object sender, EventArgs e) {
    var conn = new SqlConnection(...);   // ❌ no DB in form
    if (txtName.Text.Length < 3) { }    // ❌ no validation logic in form
    var svc = new MyService();          // ❌ no new on services
}
```

The form **only** bridges user gestures → presenter calls, and presenter calls → control updates.
