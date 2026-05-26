---
name: menu-action-standard
description: "Use when adding or wiring new CMS menu items/action keys in WinForms. Covers ActionKeyCatalog, MenuActionRegistry, MDI action routing, and validation in MenuManagementForm."
---

# Menu Action Standard (CMS WinForms)

Use this standard whenever a new form must be opened from dynamic menu actions.

## Required Updates

1. Add a new action key in `CMS.Presentation/Modules/DMS/MenuActions/MenuActionKeyCatalog.cs`.
2. Add symbol alias mapping in `ResolveConfiguredActionKey` when form class token differs from action key.
3. Add action dispatch case in `CMS.Presentation/Modules/DMS/MenuActions/MenuActionRegistry.cs`.
4. Add MDI route handling in `CMS.Presentation/MDI_Container.cs`:

- Add case in `HandleFolderMenuActionRequested`.
- Add `Open...` method that opens the target form.

5. Ensure new form is included in `CMS.Presentation/CMS.Presentation.csproj`.
6. Verify no compile errors in `CMS.Presentation`.

## Naming Rules

- Action key should use PascalCase and module prefix when needed, e.g. `WorkflowDelegation`.
- Form class names should map cleanly through `ResolveConfiguredActionKey`.
- Keep user-facing warning messages consistent with existing WFE wording.

## Validation Checklist

- Action key appears in Menu Management dropdown.
- Menu item click opens correct form from MDI.
- Unknown action key still fails safely (no crash).
- Existing action keys are unaffected.
