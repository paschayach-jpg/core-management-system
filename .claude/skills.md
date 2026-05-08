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

Follow `KBCategoryForm` / `KBCategoryController` as the canonical example.

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

**One interface per feature only:**

**View contract** → `CMS.Controllers/<Module>/Interfaces/I<Feature>Controller.cs`
This interface defines properties the form exposes AND methods the presenter calls on the form.
**Do NOT create `I<Feature>View.cs`** — the single `I<Feature>Controller.cs` is the view contract.

```csharp
public interface IKBCategoryController {
    // Properties the form exposes (presenter reads/writes these)
    string CategoryName { get; set; }
    int? EditingId { get; set; }
    bool DeleteEnabled { set; }
    // Methods the presenter calls on the form
    void BindCategories(IEnumerable<KBCategory> categories);
    void ShowError(string message);
    void ShowSuccess(string message);
    void ClearForm();
}
```

**Presenter** → `CMS.Controllers/<Module>/<Feature>Controller.cs`
The controller does **NOT** implement `I<Feature>Controller`. It accepts it as a constructor parameter named `view`.

```csharp
public class KBCategoryController {
    private readonly IKBCategoryController _view;
    private readonly IKBCategoryService    _service;

    public KBCategoryController(
        IKBCategoryController view,
        IKBCategoryService    service,
        UserContext           user) {
        _view    = view;
        _service = service;
    }

    public void Initialize() {
        try { _view.BindCategories(_service.GetAll()); }
        catch (Exception ex) { _view.ShowError(ex.Message); }
    }
}
```

**Rules:**
- `Interfaces/` folder contains `I<Feature>Controller.cs` **only** — never `I<Feature>View.cs`
- `<Feature>Controller` must **not** implement `I<Feature>Controller`
- The view interface (`I<Feature>Controller`) is injected through the controller constructor only
- The form is the concrete implementation of `I<Feature>Controller`

---

### Step 5 — Presentation (CMS.Presentation)

**Two files per form (mandatory):**

#### `<FeatureForm>.cs` — Code-behind (event handlers ONLY)

The form implements `I<Feature>Controller` (the view contract) and exposes an `AttachPresenter` method.
The constructor receives **only** `UserContext` (and other lightweight params); services are never passed to the form constructor.

```csharp
public partial class KBCategoryForm : Form,
    IKBCategoryController     // View contract — exposes properties/methods the presenter calls
{
    private KBCategoryController _presenter;
    private readonly UserContext _user;

    public KBCategoryForm(UserContext user) {
        if (user == null) throw new ArgumentNullException("user");
        _user = user;
        InitializeComponent();
    }

    // Called by MDI_Container (composition root) after the presenter is wired.
    // Must be called before the form is shown.
    public void AttachPresenter(KBCategoryController presenter) {
        if (presenter == null) throw new ArgumentNullException("presenter");
        _presenter = presenter;
    }

    // --- IKBCategoryController properties (form exposes control values) ---
    public string CategoryName {
        get { return txtName.Text.Trim(); }
        set { txtName.Text = value; }
    }

    // --- IKBCategoryController methods (presenter pushes data to form) ---
    public void BindCategories(IEnumerable<KBCategory> categories) { ... }
    public void ShowError(string msg) =>
        MessageBox.Show(msg, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);

    // --- Event handlers (ONLY thing allowed here) ---
    private void Form_Load(object sender, EventArgs e)       => _presenter.Initialize();
    private void btnSave_Click(object sender, EventArgs e)   => _presenter.Save();
    private void btnDelete_Click(object sender, EventArgs e) => _presenter.Deactivate();
}
```

#### `<FeatureForm>.Designer.cs` — Visual designer (all control definitions live here)
- All control field declarations, `InitializeComponent()`, layout code, styles
- VS designer writes this file. **You may edit it manually** when adding new controls — but follow the exact WinForms designer pattern:
  1. Declare the field in the `#region Component Fields` block at the bottom
  2. Instantiate with `this.controlName = new ControlType();` inside `InitializeComponent()`
  3. Add `SuspendLayout()` / `BeginInit()` at the top of the suspend block, and matching `ResumeLayout()` / `EndInit()` before the method ends
  4. Wire up all properties (Size, Location, Dock, Font, Text, etc.) between BeginInit and EndInit
  5. Add the control to its parent via `parentControl.Controls.Add(this.controlName)`
- Use `DataGridViewCellStyle` as a **local variable** inside `InitializeComponent()`, not as a field

**Rule: ALL control creation belongs in Designer.cs. Code-behind must never instantiate controls.**

---

### Step 6 — Wire up in MDI_Container (composition root for forms)

`Program.cs` wires repos → services and injects services into `MDI_Container`.
`MDI_Container` creates forms and their controllers using the two-step injection pattern.

**Program.cs** — repos and services only, pass to MDI_Container:
```csharp
var kbCategoryRepo    = new KBCategoryRepository(configManager);
var kbCategoryService = new KBCategoryService(kbCategoryRepo, auditService);
var kbArticleService  = new KBArticleService(kbArticleRepo, kbVersionRepo, auditService);

var mdi = new MDI_Container(...,
    kbCategoryService: kbCategoryService,
    kbArticleService:  kbArticleService, ...);
```

**MDI_Container** — two-step injection for every form/presenter pair:
```csharp
// Step 1: create form with minimal params (UserContext only, no services)
var form      = new KBCategoryForm(_currentUser);
// Step 2: create presenter, passing the form as the view (IKBCategoryController)
var presenter = new KBCategoryController(form, _kbCategoryService, _kbArticleService, _currentUser);
// Step 3: inject controller reference back into the form
form.AttachPresenter(presenter);
// Step 4: show
OpenChildForm(form);
```

**Rule:** Never create a presenter inside a form constructor. Forms never hold service references.

---

## 3. Opening a Form from MDI_Container

Always use the two-step injection pattern — form knows nothing about services:

```csharp
// Inside MDI_Container — open as MDI child
private void OpenKBCategoryForm() {
    var form      = new KBCategoryForm(_currentUser);
    var presenter = new KBCategoryController(
        form, _kbCategoryService, _kbArticleService, _currentUser);
    form.AttachPresenter(presenter);
    form.MdiParent = this;
    form.Show();
}
```

For factory patterns (forms created on demand inside another form):
```csharp
form.AttachFactories(
    categoryFormFactory: () => {
        var catForm      = new KBCategoryForm(_currentUser);
        var catPresenter = new KBCategoryController(
            catForm, _kbCategoryService, _kbArticleService, _currentUser);
        catForm.AttachPresenter(catPresenter);
        return catForm;
    });
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
| Form class | `<Feature>Form` | `KBCategoryForm` |
| View contract (interface) | `I<Feature>Controller` | `IKBCategoryController` |
| Presenter class | `<Feature>Controller` | `KBCategoryController` |
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
    Interfaces/    ← I<Feature>Controller.cs ONLY (view contract — no I<Feature>View.cs)
    <Feature>Controller.cs  (does NOT implement I<Feature>Controller)

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
// ✅ ALLOWED in Form.cs (code-behind)
private void btnSave_Click(object sender, EventArgs e) => _presenter.Save();
private void dgv_CellClick(object sender, DataGridViewCellEventArgs e) => _presenter.SelectRow(e.RowIndex);
public string InputText => txtInput.Text.Trim();             // view property
public void ShowResult(MyModel m) => lblResult.Text = m.Name; // view method
public void SetProgress(int pct, string msg) {               // view method updating designer controls
    progressBar.Value = pct;
    lblStatus.Text    = msg;
    Application.DoEvents();                                  // flush UI during synchronous work
}

// ❌ NOT ALLOWED in Form.cs
private void btnSave_Click(object sender, EventArgs e) {
    var conn = new SqlConnection(...);   // ❌ no DB in form
    if (txtName.Text.Length < 3) { }    // ❌ no business validation in form
    var svc = new MyService();          // ❌ no new on services
    var lbl = new Label();             // ❌ NEVER create controls in code-behind
    var dgv = new DataGridView();      // ❌ NEVER create controls in code-behind
    panel.Controls.Add(lbl);          // ❌ NEVER add controls dynamically in code-behind
}
```

The form **only** bridges user gestures → presenter calls, and presenter calls → control updates.

### Designer/Code-Behind Separation — Absolute Rules

| What | Where |
|---|---|
| Control field declaration | `Designer.cs` — `#region Component Fields` |
| Control instantiation (`new Label()`) | `Designer.cs` — inside `InitializeComponent()` |
| Control property setup (Size, Dock, Font…) | `Designer.cs` — inside `InitializeComponent()` |
| `Controls.Add(…)` | `Designer.cs` — inside `InitializeComponent()` |
| `SuspendLayout / ResumeLayout` | `Designer.cs` — wrapping `InitializeComponent()` body |
| Event handler wiring (`btn.Click += …`) | `Designer.cs` — inside `InitializeComponent()` |
| Event handler implementation | `Form.cs` (code-behind) |
| View interface properties/methods | `Form.cs` (code-behind) |
| Business logic | **Never** in any Form file — belongs in presenter/service |

**Violation example (from KBImportWizardForm history):**
A method `EnsureErrorDetailPanel()` was added to code-behind that called `new Panel()`, `new DataGridView()`, set styles, and called `Controls.Add()`. This violated the Designer/Code-Behind separation rule. The fix was to move ALL of this to `Designer.cs` and remove `EnsureErrorDetailPanel()` entirely from code-behind.

---

## 8. Project File (.csproj) and Build Rules

### Adding new files — always register in .csproj

Every new `.cs` file (form, class, interface, designer) **must** be added to the `.csproj` of its project immediately. A file that exists on disk but is missing from `.csproj` will be silently ignored by the compiler.

**WinForms form files** require three linked entries:
```xml
<Compile Include="Modules\KMS\KBCategoryForm.cs">
  <SubType>Form</SubType>
</Compile>
<Compile Include="Modules\KMS\KBCategoryForm.Designer.cs">
  <DependentUpon>KBCategoryForm.cs</DependentUpon>
</Compile>
<EmbeddedResource Include="Modules\KMS\KBCategoryForm.resx">
  <DependentUpon>KBCategoryForm.cs</DependentUpon>
</EmbeddedResource>
```

**Plain class / interface files** require a single entry:
```xml
<Compile Include="KMS\KB\Interfaces\IKBCategoryController.cs" />
```

### Build after every modification

After every code change, run a build to catch compile errors immediately:
```
msbuild CMS.sln /t:Build /p:Configuration=Debug
```
or use the VS keyboard shortcut **Ctrl+Shift+B**.

**Never** leave compile errors unresolved and move on to the next step. Fix cascade order: Domain → Data → Services → Controllers → Presentation → Program.cs.
