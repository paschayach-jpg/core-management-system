# SKILL: Build a Form (CMS MVP Pattern)

Use this skill whenever the user says: "create a form", "build a screen", "add a UI", "create a dialog", or anything that requires a new WinForms screen in the CMS solution.

---

## What This Skill Produces

For every new form, you must create **all of the following** — never skip a layer:

| # | File | Project | Purpose |
|---|---|---|---|
| 1 | `XxxForm.cs` | CMS.Presentation | Code-behind (implements IXxxView) |
| 2 | `XxxForm.Designer.cs` | CMS.Presentation | Visual designer (InitializeComponent) |
| 3 | `XxxPresenter.cs` | CMS.Controllers | MVP Presenter |
| 4 | `IXxxView.cs` | CMS.Controllers | View contract interface |
| 5 | `IXxxService.cs` | CMS.Services | Service interface |
| 6 | `XxxService.cs` | CMS.Services | Service implementation |
| 7 | `XxxModel.cs` | CMS.Domain | Domain model (if new entity needed) |
| 8 | `.csproj` edits | All affected | Add `<Compile>` entries + `<ProjectReference>` if needed |

---

## Step-by-Step Instructions

### STEP 1 — Clarify Before Starting

Ask the user (or infer from context) the following before writing any code:

- **Module:** Which main module does this belong to? (`DMS`, `KMS`, or new?)
- **Sub-module / feature area:** e.g., `KB` (Knowledge Base), `Document`, `Category`
- **Form type:** Full form (`Form`) or a dialog (`Dialog`)?
- **Entity / data:** What domain object does this form manage?
- **Actions:** What operations does the form support? (Add, Edit, Delete, View, Search, etc.)
- **Existing services/repos:** Does a service or repository already exist for this entity?

---

### STEP 2 — Determine File Paths

Use the naming rules below to derive every file path before writing any code:

```
Given:
  Module       = KMS
  SubModule    = KB
  FormBaseName = KBCategory   (e.g., for a category management form)

Produces:
  CMS.Presentation/Modules/KMS/KBCategoryForm.cs
  CMS.Presentation/Modules/KMS/KBCategoryForm.Designer.cs
  CMS.Controllers/KMS/KB/KBCategoryPresenter.cs
  CMS.Controllers/KMS/Interfaces/IKBCategoryView.cs
  CMS.Services/Interfaces/IKBCategoryService.cs
  CMS.Services/KBCategoryService.cs
  CMS.Domain/Models/KBCategory.cs           ← only if model doesn't exist yet
```

**Naming rules:**
- View interface:       `I{FormBaseName}View`          in `CMS.Controllers\{Module}\Interfaces\`
- Presenter:           `{FormBaseName}Presenter`       in `CMS.Controllers\{Module}\{SubModule}\`
- Service interface:   `I{FormBaseName}Service`        in `CMS.Services\Interfaces\`
- Service class:       `{FormBaseName}Service`         in `CMS.Services\`
- Domain model:        `{EntityName}`                  in `CMS.Domain\Models\`
- Form code-behind:    `{FormBaseName}Form.cs`         in `CMS.Presentation\Modules\{Module}\`
- Form designer:       `{FormBaseName}Form.Designer.cs`

---

### STEP 3 — CMS.Domain Model

Create `CMS.Domain/Models/{EntityName}.cs` **only if the model does not already exist**.

```csharp
namespace CMS.Domain.Models
{
    public class {EntityName}
    {
        // Primary key
        public int {EntityName}ID { get; set; }

        // Core fields
        public string Name { get; set; }
        // ... add relevant fields

        // Alias for backward compatibility
        public int {EntityName}Id
        {
            get { return {EntityName}ID; }
            set { {EntityName}ID = value; }
        }

        // Transient fields (populated by service layer, not stored in DB)
        public string CreatedByName { get; set; }

        // Enum conversion properties (only if the entity has status/type int fields)
        // public SomeEnum SomeEnumValue
        // {
        //     get { return (SomeEnum)SomeIntField; }
        //     set { SomeIntField = (int)value; }
        // }
    }
}
```

---

### STEP 4 — CMS.Services: Interface

Create `CMS.Services/Interfaces/I{FormBaseName}Service.cs`:

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
    }
}
```

Adjust the method signatures to match the actual operations the form supports. Not every CRUD method is always needed.

---

### STEP 5 — CMS.Services: Implementation

Create `CMS.Services/{FormBaseName}Service.cs`:

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
        // Add more dependencies as needed

        public {FormBaseName}Service(I{EntityName}Repository {entityVar}Repository)
        {
            if ({entityVar}Repository == null) throw new ArgumentNullException("{entityVar}Repository");
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
            // Add validation rules here
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

**Key rules:**
- All constructor parameters must be null-checked (`throw new ArgumentNullException`).
- All public method parameters must also be null-checked.
- Business validation belongs here, NOT in the presenter or form.
- If data is needed from multiple repositories, inject all of them in the constructor.

---

### STEP 6 — CMS.Controllers: View Interface

Create `CMS.Controllers/{Module}/Interfaces/I{FormBaseName}View.cs`:

```csharp
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.{Module}.Interfaces
{
    public interface I{FormBaseName}View
    {
        // Display methods — presenter calls these to push data to the form
        void Show{EntityName}List(IEnumerable<{EntityName}> items);
        void ShowError(string message);
        void ShowInfo(string message);

        // Selection properties — presenter reads these to get current UI state
        {EntityName} Selected{EntityName} { get; }
    }
}
```

**Rules:**
- The interface only exposes what the presenter needs. Never expose raw UI controls (ListViews, TextBoxes, etc.).
- `ShowError` and `ShowInfo` are always present.
- Read-only properties for currently selected items.

---

### STEP 7 — CMS.Controllers: Presenter

Create `CMS.Controllers/{Module}/{SubModule}/{FormBaseName}Presenter.cs`:

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
            if (view == null) throw new ArgumentNullException("view");
            if (user == null) throw new ArgumentNullException("user");

            _service = service;
            _view = view;
            _user = user;
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
                _view.ShowError("Failed to load data: " + ex.Message);
            }
        }

        public void Add({EntityName} entity)
        {
            try
            {
                _service.Create(entity, _user);
                LoadData(); // refresh list after mutation
                _view.ShowInfo("{EntityName} created successfully.");
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to create: " + ex.Message);
            }
        }

        public void Edit({EntityName} entity)
        {
            try
            {
                _service.Update(entity, _user);
                LoadData();
                _view.ShowInfo("{EntityName} updated successfully.");
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to update: " + ex.Message);
            }
        }

        public void Delete(int id)
        {
            try
            {
                _service.Delete(id, _user);
                LoadData();
                _view.ShowInfo("{EntityName} deleted successfully.");
            }
            catch (Exception ex)
            {
                _view.ShowError("Failed to delete: " + ex.Message);
            }
        }
    }
}
```

**Rules:**
- Only include presenter methods for the actions the form actually needs.
- Always wrap service calls in try-catch; report errors via `_view.ShowError()`.
- After any mutation (Add/Edit/Delete), call a load method to refresh the view.
- Presenter never directly accesses UI controls or creates MessageBox calls.

---

### STEP 8 — CMS.Presentation: Form Code-behind

Create `CMS.Presentation/Modules/{Module}/{FormBaseName}Form.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CMS.Controllers.{Module}.Interfaces;
using CMS.Controllers.{Module}.{SubModule};
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.{Module}
{
    public partial class {FormBaseName}Form : Form, I{FormBaseName}View
    {
        private readonly I{FormBaseName}Service _{serviceVar}Service;
        private readonly UserContext _user;
        private {FormBaseName}Presenter _presenter;

        // Backing state
        private List<{EntityName}> _items = new List<{EntityName}>();

        public {FormBaseName}Form(I{FormBaseName}Service {serviceVar}Service, UserContext user)
        {
            InitializeComponent();
            _{serviceVar}Service = {serviceVar}Service;
            _user = user;
            _presenter = new {FormBaseName}Presenter(_{serviceVar}Service, this, _user);
        }

        // ─── Form events ───────────────────────────────────────────────
        private void {FormBaseName}Form_Load(object sender, EventArgs e)
        {
            _presenter.LoadData();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Build entity from UI inputs
            var entity = new {EntityName}
            {
                Name = txt{FieldName}.Text.Trim(),
                CreatedBy = _user.UserId
            };
            _presenter.Add(entity);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (Selected{EntityName} == null) { ShowInfo("Please select an item."); return; }
            var entity = Selected{EntityName};
            entity.Name = txt{FieldName}.Text.Trim();
            _presenter.Edit(entity);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (Selected{EntityName} == null) { ShowInfo("Please select an item."); return; }
            var confirm = MessageBox.Show(
                "Are you sure you want to delete this item?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
                _presenter.Delete(Selected{EntityName}.{EntityName}ID);
        }

        // ─── I{FormBaseName}View implementation ────────────────────────
        public void Show{EntityName}List(IEnumerable<{EntityName}> items)
        {
            _items = new List<{EntityName}>(items);
            listView{EntityName}.Items.Clear();
            foreach (var item in _items)
            {
                var lvi = new ListViewItem(item.Name) { Tag = item };
                listView{EntityName}.Items.Add(lvi);
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
- The form class `partial` — it pairs with the `.Designer.cs`.
- The form constructor takes services as parameters — it must not create services itself (`new XxxService()` is forbidden in the form).
- The form creates the presenter by passing `this` as the view.
- Event handlers (`_Click`, `_Load`) only collect UI input and call a presenter method — no business logic.
- `ShowError` and `ShowInfo` always use `MessageBox.Show`.
- The `SelectedXxx` property reads from the UI control's `Tag` (which was set during `ShowXxxList`).

---

### STEP 9 — CMS.Presentation: Visual Designer

Create `CMS.Presentation/Modules/{Module}/{FormBaseName}Form.Designer.cs`:

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
            this.txt{FieldName} = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // listView{EntityName}
            this.listView{EntityName}.Location = new System.Drawing.Point(12, 12);
            this.listView{EntityName}.Size = new System.Drawing.Size(560, 300);
            this.listView{EntityName}.View = System.Windows.Forms.View.Details;
            this.listView{EntityName}.FullRowSelect = true;
            this.listView{EntityName}.GridLines = true;
            this.listView{EntityName}.Columns.Add("Name", 540);

            // txt{FieldName}
            this.txt{FieldName}.Location = new System.Drawing.Point(12, 325);
            this.txt{FieldName}.Size = new System.Drawing.Size(350, 23);

            // btnAdd
            this.btnAdd.Location = new System.Drawing.Point(370, 323);
            this.btnAdd.Size = new System.Drawing.Size(60, 27);
            this.btnAdd.Text = "Add";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);

            // btnEdit
            this.btnEdit.Location = new System.Drawing.Point(438, 323);
            this.btnEdit.Size = new System.Drawing.Size(60, 27);
            this.btnEdit.Text = "Edit";
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);

            // btnDelete
            this.btnDelete.Location = new System.Drawing.Point(506, 323);
            this.btnDelete.Size = new System.Drawing.Size(66, 27);
            this.btnDelete.Text = "Delete";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);

            // {FormBaseName}Form
            this.ClientSize = new System.Drawing.Size(600, 370);
            this.Controls.Add(this.listView{EntityName});
            this.Controls.Add(this.txt{FieldName});
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnDelete);
            this.Name = "{FormBaseName}Form";
            this.Text = "{FormBaseName}";
            this.Load += new System.EventHandler(this.{FormBaseName}Form_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Control declarations
        private System.Windows.Forms.ListView listView{EntityName};
        private System.Windows.Forms.TextBox txt{FieldName};
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
    }
}
```

**Rules:**
- The designer file is `partial class`, same namespace and class name as the code-behind.
- Always include the standard `Dispose` override and `IContainer components`.
- `InitializeComponent()` must wire event handlers using `+=`.
- Control declarations go at the bottom as `private` fields.
- Adapt controls to match the actual form design (more columns, panels, labels, etc. as needed).

---

### STEP 10 — Update .csproj Files

After creating every file, add `<Compile Include="..." />` entries to the appropriate `.csproj` files.

**CMS.Domain.csproj** (if a new model was created):
```xml
<Compile Include="Models\{EntityName}.cs" />
```

**CMS.Services.csproj**:
```xml
<Compile Include="Interfaces\I{FormBaseName}Service.cs" />
<Compile Include="{FormBaseName}Service.cs" />
```

**CMS.Controllers.csproj**:
```xml
<Compile Include="{Module}\Interfaces\I{FormBaseName}View.cs" />
<Compile Include="{Module}\{SubModule}\{FormBaseName}Presenter.cs" />
```

**CMS.Presentation.csproj**:
```xml
<Compile Include="Modules\{Module}\{FormBaseName}Form.cs">
  <SubType>Form</SubType>
</Compile>
<Compile Include="Modules\{Module}\{FormBaseName}Form.Designer.cs">
  <DependentUpon>{FormBaseName}Form.cs</DependentUpon>
</Compile>
```

If a project needs a new inter-project reference, add:
```xml
<ProjectReference Include="..\CMS.Services\CMS.Services.csproj">
  <Project>{GUID}</Project>
  <Name>CMS.Services</Name>
</ProjectReference>
```

> Look up the actual GUID from the referenced project's `.csproj` `<ProjectGuid>` element.

---

### STEP 11 — Build & Verify

After writing all files, build the solution to confirm no compile errors:

```bash
msbuild CMS.sln /p:Configuration=Debug /nologo /v:minimal
```

If there are errors, fix them before finishing. Common issues:
- Missing `using` directives
- Namespace mismatches between files
- `<Compile>` entry missing from a `.csproj`
- Missing `<ProjectReference>` for a newly depended-on project

---

## Quick Reference Checklist

Before marking the task done, confirm all boxes are ticked:

- [ ] `{EntityName}.cs` created in `CMS.Domain/Models/` (or confirmed existing)
- [ ] `I{FormBaseName}Service.cs` created in `CMS.Services/Interfaces/`
- [ ] `{FormBaseName}Service.cs` created in `CMS.Services/`
- [ ] `I{FormBaseName}View.cs` created in `CMS.Controllers/{Module}/Interfaces/`
- [ ] `{FormBaseName}Presenter.cs` created in `CMS.Controllers/{Module}/{SubModule}/`
- [ ] `{FormBaseName}Form.cs` created in `CMS.Presentation/Modules/{Module}/`
- [ ] `{FormBaseName}Form.Designer.cs` created in `CMS.Presentation/Modules/{Module}/`
- [ ] All relevant `.csproj` files updated with `<Compile>` entries
- [ ] Solution builds without errors
