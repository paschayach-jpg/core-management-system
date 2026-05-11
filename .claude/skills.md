# CMS Solution — Developer Skills Guide
> C# .NET 4.5 · Windows Forms · SQL Server · ADO.NET · MVP Pattern

---

## 1. Layer Architecture

```
CMS.Presentation   ← Form (UI + event handlers only)
CMS.Controllers    ← Presenter (MVP logic, no DB, no UI controls)
CMS.Services       ← Business logic, helpers, processors
CMS.Data           ← Repositories (ADO.NET + SQL Server only)
CMS.Domain         ← POCOs + Enums (no logic, no dependencies)
CMS.Infrastructure ← Cross-cutting (logging, AD, config)
```

**Dependency direction (strict, one-way):**
```
Presentation → Controllers → Services → Data → Domain
                                              ↑
                                        Infrastructure
```

**Composition root:** `Program.cs` — only place that calls `new` on repos/services.  
**MDI host:** `MDI_Container` — creates forms and wires presenters. No form creates its own presenter.

---

## 2. Naming Conventions

| Artifact | Pattern | Example |
|---|---|---|
| Form | `<Feature>Form` | `KBImportWizardForm` |
| View contract (interface) | `I<Feature>Controller` | `IKBImportWizardController` |
| Presenter | `<Feature>Controller` | `KBImportWizardController` |
| Service interface | `I<Name>Service` | `IKBArticleService`, `IKBExcelReaderService` |
| Service / Helper / Processor | `<Name>Service` / `<Name>` | `KBArticleService`, `KBExcelReader` |
| Repository interface | `I<Entity>Repository` | `IKBArticleRepository` |
| Repository implementation | `<Entity>Repository` | `KBArticleRepository` |
| Domain model | `<EntityName>` | `KBArticle`, `KBImportRow` |
| Enum file | descriptive | `KBCategoryCodeName` |

**Interface suffix rule:** Every injectable class in `CMS.Services` — whether a Service, Helper, Reader, or Processor — must have an interface. The interface name always ends in `Service` (e.g., `IKBExcelReaderService`, `IKBImportProcessorService`).

---

## 3. File Locations

```
CMS.Domain/
  Enums/             ← all enums
  Models/            ← all POCOs

CMS.Data/
  Interfaces/        ← I<Entity>Repository.cs
  Repositories/      ← <Entity>Repository.cs

CMS.Services/
  Interfaces/        ← I<Name>Service.cs  (ALL injectable classes need one — no exceptions)
  <Name>Service.cs   ← service implementations
  <Name>.cs          ← helpers/readers/processors (implement their I<Name>Service interface)

CMS.Controllers/<Module>/
  Interfaces/        ← I<Feature>Controller.cs ONLY (view contract — never I<Feature>View.cs)
  <Feature>Controller.cs

CMS.Presentation/Modules/<Module>/
  <Feature>Form.cs           ← event handlers + view interface impl
  <Feature>Form.Designer.cs  ← all controls (auto-generated, may edit manually)
  <Feature>Form.resx
```

---

## 4. MVP Wiring Rules

**View contract** (`I<Feature>Controller`) defines properties the form exposes and methods the presenter calls back on the form.

```csharp
// CMS.Controllers/KMS/KB/Interfaces/IKBImportWizardController.cs
public interface IKBImportWizardController {
    string SelectedFilePath { get; }
    bool   OptSkipWarning   { get; }
    void   ShowValidationResult(KBImportValidationResult result);
    void   ShowError(string message);
    void   SetProgress(int percent, string label);
    void   ShowImportComplete(KBImportValidationResult summary);
    void   SetStatus(string text);
}
```

**Presenter** — constructor-injected; does NOT implement the view interface.

```csharp
public class KBImportWizardController {
    private readonly IKBImportWizardController _view;
    private readonly IKBExcelReaderService     _excelReader;   // interface, not concrete
    private readonly IKBImportProcessorService _processor;     // interface, not concrete

    public KBImportWizardController(
        IKBImportWizardController view,
        IKBExcelReaderService     excelReader,
        IKBImportProcessorService processor,
        UserContext               user) { ... }
}
```

**Form** — implements the view interface; calls presenter via `AttachPresenter`.

```csharp
public partial class KBImportWizardForm : Form, IKBImportWizardController {
    private KBImportWizardController _presenter;
    public void AttachPresenter(KBImportWizardController p) => _presenter = p;

    // view properties
    public string SelectedFilePath => txtFilePath.Text.Trim();

    // view methods (presenter pushes data here)
    public void ShowError(string msg) =>
        MessageBox.Show(msg, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);

    // event handlers only
    private void btnValidate_Click(object sender, EventArgs e) => _presenter.ValidateFile();
}
```

**MDI_Container wiring (two-step injection):**

```csharp
var form      = new KBImportWizardForm(_currentUser);
var presenter = new KBImportWizardController(
    form, _excelReader, _processor, _kbArticleService, _kbCategoryService, _currentUser);
form.AttachPresenter(presenter);
OpenChildForm(form);
```

---

## 5. Service & Helper Interface Rule

Every class injected into a controller **must** be referenced through its interface. Never use a concrete type as a constructor parameter or field type in a presenter.

```csharp
// ✅ correct
private readonly IKBExcelReaderService     _excelReader;
private readonly IKBImportProcessorService _processor;

// ❌ wrong
private readonly KBExcelReader    _excelReader;
private readonly KBImportProcessor _processor;
```

Helpers and processors live in `CMS.Services/` and implement their interface:

```csharp
// CMS.Services/Interfaces/IKBExcelReaderService.cs
public interface IKBExcelReaderService {
    IList<KBImportRow> ReadRows(string filePath);
}

// CMS.Services/KBExcelReader.cs
public class KBExcelReader : IKBExcelReaderService { ... }
```

---

## 6. Designer / Code-Behind Separation

| What | Where |
|---|---|
| Control field declaration | `Designer.cs` |
| `new ControlType()` | `Designer.cs` — inside `InitializeComponent()` |
| Property setup (Size, Dock, Font…) | `Designer.cs` |
| `Controls.Add(…)` | `Designer.cs` |
| Event handler wiring (`btn.Click +=`) | `Designer.cs` |
| Event handler implementation | `Form.cs` |
| View interface properties/methods | `Form.cs` |
| Business logic | **Never** in any Form file |

**Hard rule:** Code-behind must never instantiate controls (`new Label()`, `new DataGridView()`, etc.) or call `Controls.Add()`. All of that belongs in `Designer.cs`.

```csharp
// ✅ ALLOWED in Form.cs
private void btnSave_Click(object sender, EventArgs e) => _presenter.Save();
public string CategoryName => txtName.Text.Trim();
public void SetProgress(int pct, string msg) {
    progressBar.Value = pct;
    lblStatus.Text    = msg;
    Application.DoEvents();
}

// ❌ NOT ALLOWED in Form.cs
var lbl = new Label();           // ❌ no control creation
panel.Controls.Add(lbl);         // ❌ no dynamic control adding
var conn = new SqlConnection();  // ❌ no DB
if (txt.Text.Length < 3) { }    // ❌ no business validation
```

---

## 7. Fix Cascade Order

When changing any layer, always propagate changes **bottom-up**:

```
1. Domain → 2. Data → 3. Services → 4. Controllers → 5. Presentation → 6. Program.cs
```

| Layer changed | Must also update |
|---|---|
| Domain model (add/rename property) | Repository mapping, Service, Controller, Form binding |
| Repository interface | Implementation, Service constructor, Program.cs |
| Service interface | Implementation, Controller constructor, Program.cs |
| View contract interface | Presenter calls, Form implementation |

Never leave compile errors unresolved before moving to the next layer.

---

## 8. .csproj Registration

Every new `.cs` file **must** be registered in its project's `.csproj`. Missing entries are silently ignored by the compiler.

**Plain class or interface:**
```xml
<Compile Include="Interfaces\IKBExcelReaderService.cs" />
<Compile Include="Interfaces\IKBImportProcessorService.cs" />
```

**WinForms form (three linked entries):**
```xml
<Compile Include="Modules\KMS\KBImportWizardForm.cs">
  <SubType>Form</SubType>
</Compile>
<Compile Include="Modules\KMS\KBImportWizardForm.Designer.cs">
  <DependentUpon>KBImportWizardForm.cs</DependentUpon>
</Compile>
<EmbeddedResource Include="Modules\KMS\KBImportWizardForm.resx">
  <DependentUpon>KBImportWizardForm.cs</DependentUpon>
</EmbeddedResource>
```

Build after every change: **Ctrl+Shift+B** or `msbuild CMS.sln /t:Build /p:Configuration=Debug`.
