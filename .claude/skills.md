# CMS Solution — Developer Guide
> C# .NET 4.5 · WinForms · SQL Server · ADO.NET · MVP

---

## Architecture

```
Presentation → Controllers → Services → Data → Domain
                                              ↑
                                        Infrastructure
```

| Project | Role |
|---|---|
| `CMS.Presentation` | Forms, event handlers only |
| `CMS.Controllers` | Presenter / MVP logic, no DB |
| `CMS.Services` | Business rules, validation, orchestration |
| `CMS.Data` | ADO.NET repositories, SQL only |
| `CMS.Domain` | POCOs + Enums, no logic |
| `CMS.Infrastructure` | Logging, AD, config |

- All `new` for repos/services happens in `Program.cs` only. No `new` inside forms.
- All module forms open as MDI children of `MDI_Container`.

---

## New Form Checklist

**1. Domain** — `CMS.Domain/Models/<Entity>.cs`, `CMS.Domain/Enums/<Feature>Enum.cs`
- Plain POCO, no logic beyond computed read-only properties

**2. Data** — `CMS.Data/Interfaces/I<Entity>Repository.cs` + `Repositories/<Entity>Repository.cs`
- ADO.NET only (`SqlConnection`/`SqlCommand`/`SqlDataReader`), `using` on every connection
- No business logic, pure SQL + mapping

**3. Services** — `CMS.Services/Interfaces/I<Feature>Service.cs` + `<Feature>Service.cs`
- Inject repos via constructor; all rules and validation live here
- Throw meaningful exceptions, never return `bool`/`null` silently
- Helper classes (no interface needed): `CMS.Services/<Feature>Processor.cs`

**4. Controllers** — two interfaces per feature:
- `I<Feature>View.cs` — properties + methods the form exposes to the presenter
- `I<Feature>Controller.cs` — methods the form calls on user action
- `<Feature>Controller.cs` — presenter; injects view + services via constructor

**5. Presentation** — `CMS.Presentation/Modules/<Module>/`
- `<Feature>Form.cs` — event handlers only; implements the View interface
- `<Feature>Form.Designer.cs` — auto-generated; never edit manually

**6. Wire up** — `Program.cs` → `MDI_Container.cs` in that order

```csharp
// Program.cs
var articleRepo    = new KBArticleRepository(config);
var articleService = new KBArticleService(articleRepo, auditService);
var mdi            = new MDI_Container(..., articleService, ...);
```

```csharp
// Form.cs — allowed
private void btnSave_Click(object sender, EventArgs e) => _presenter.Save();
public string InputText => txtInput.Text.Trim();

// Form.cs — not allowed
private void btnSave_Click(object sender, EventArgs e) {
    var conn = new SqlConnection(...);   // ❌ no DB in form
    if (txtName.Text.Length < 3) { }    // ❌ no validation in form
}
```

---

## Fix / Revision Rules

### Layer cascade — bottom-up, never skip
```
Domain → Data → Services → Controllers → Presentation → Program.cs
```

| Layer changed | Also update |
|---|---|
| Domain model (add/rename property) | Repo mapping, Service, Controller, Form binding |
| Repository interface | Implementation, Service ctor, Program.cs |
| Service interface | Implementation, Controller ctor, Program.cs |
| Controller / View interface | Presenter, Form ctor, Form view-contract impl |

- Add to interface → implement in every class that implements it
- Change constructor signature → update Program.cs immediately
- Rename → use VS F2 (Rename) to catch all callers

### Hard rules
- No `SqlCommand` / business logic in Forms or Controllers
- No `MessageBox` / control references in Services or Repositories
- No `new` on services inside forms

---

## Troubleshooting Mindset

### Before writing any fix
1. **State the mechanism in one sentence.** If you cannot, keep investigating — do not write code yet.
2. **Map runtime constraints first.** For third-party runtimes (browser engine, COM, native API): document no-op conditions, known side effects, and threading rules before touching code.
3. **Enumerate side effects of the proposed fix.** Ask: what system state does this change? What invariant can it violate?

### During diagnosis
- **One specific value fails while all others pass** → structural collision in the runtime, not a code bug. Characterize *why* that value is different before fixing anything.
- **Same root cause reappears after each patch** → you are treating symptoms. Stop and identify the actual constraint.
- **Fix introduces a new regression** → the approach has uncontrolled side effects.
- **Two independent attempts, same failure class** → the approach is wrong. Step back, list alternatives, evaluate each against the runtime constraints, then choose.

### Before declaring success
Test the full matrix, not just the reported case:
- [ ] Exact failing value now works
- [ ] All previously working values still work
- [ ] Rapid / repeated use does not freeze or deadlock
- [ ] Selection / focus / UI state remains consistent throughout

---

## Naming Conventions

| Artifact | Convention | Example |
|---|---|---|
| Form | `<Feature>Form` | `KBImportWizardForm` |
| View interface | `I<Feature>View` | `IKBImportWizardView` |
| Controller interface | `I<Feature>Controller` | `IKBImportWizardController` |
| Presenter class | `<Feature>Controller` | `KBImportWizardController` |
| Service interface | `I<Entity>Service` | `IKBArticleService` |
| Repository interface | `I<Entity>Repository` | `IKBArticleRepository` |
| Repository impl | `<Entity>Repository` | `KBArticleRepository` |
| Domain model | `<EntityName>` | `KBArticle` |
| Helper / Processor | `<Feature><Role>` | `KBImportProcessor` |

---

## File Locations

```
CMS.Domain/
  Enums/                    ← all enums
  Models/                   ← all POCOs

CMS.Data/
  Interfaces/               ← I<Entity>Repository.cs
  Repositories/             ← <Entity>Repository.cs

CMS.Services/
  Interfaces/               ← I<Feature>Service.cs
  <Feature>Service.cs
  <Feature>Processor.cs     ← helpers (no interface needed)

CMS.Controllers/<Module>/
  Interfaces/               ← I<Feature>View.cs + I<Feature>Controller.cs
  <Feature>Controller.cs

CMS.Presentation/
  Modules/<Module>/
    <Feature>Form.cs        ← event handlers only
    <Feature>Form.Designer.cs
  Program.cs                ← only place that calls new on repos/services
  MDI_Container.cs          ← injects deps into child forms
```
