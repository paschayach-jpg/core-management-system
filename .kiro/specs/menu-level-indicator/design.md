# Design Document: Menu Level Indicator

## Overview

This feature adds a visual depth indicator (`[L0]`, `[L1]`, `[L2]`, …) to every `TreeNode` displayed in the `treeMenu` control on `MenuManagementForm`. The change is purely presentational and confined to `MenuManagementForm.cs` in `CMS.Presentation`. No database, domain model, service, or repository changes are required.

The core problem is that `TreeNode.Level` is only reliable after a node has been added to the tree, but we need the level text at construction time. The solution is to pass an explicit `depth` integer into `CreateTreeNode` and increment it recursively through `LoadChildNodes`.

## Architecture

The change touches exactly two private methods and one call site inside `MenuManagementForm`. No new classes, interfaces, or files are introduced.

```
MenuManagementForm
  RefreshMenuTree()          ← passes depth=0 to CreateTreeNode for root nodes
  CreateTreeNode(menu, depth) ← prefixes "[L{depth}] " to node text
  LoadChildNodes(node, id)   ← passes depth+1 when recursing
```

`MenuTreeViewManager` is explicitly out of scope — it serves the end-user navigation tree in `MDI_Container` and must not show level indicators.

## Components and Interfaces

### Modified: `MenuManagementForm.CreateTreeNode`

Current signature:
```csharp
private TreeNode CreateTreeNode(MenuItem menu)
```

New signature:
```csharp
private TreeNode CreateTreeNode(MenuItem menu, int depth)
```

The method prepends `[L{depth}] ` to the existing text. `TreeNode.Tag` continues to hold the raw `MenuItem` object — nothing else changes.

### Modified: `MenuManagementForm.RefreshMenuTree`

Passes `depth: 0` when calling `CreateTreeNode` for root nodes.

### Modified: `MenuManagementForm.LoadChildNodes`

Receives the current `depth` and passes `depth + 1` when recursing into children.

New signature:
```csharp
private void LoadChildNodes(TreeNode parentNode, int parentMenuItemID, int depth)
```

### Unchanged: `MenuTreeViewManager`

`MenuTreeViewManager.CreateTreeNode` has a different signature, different styling logic (ForeColor, ToolTipText, icon), and serves a different purpose. It is not modified.

## Data Models

No domain model changes. `MenuItem` gains no new property. Depth is a transient rendering concern computed from the call stack during tree construction.

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system — essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Level indicator prefix is correct for any depth

*For any* `MenuItem` and any non-negative integer `depth`, calling `CreateTreeNode(menu, depth)` SHALL produce a `TreeNode` whose `Text` starts with `"[L{depth}] "`.

**Validates: Requirements 1.1, 1.2, 1.3, 1.4, 1.5, 2.1, 3.1, 3.3**

### Property 2: Original node text is preserved after prefixing

*For any* `MenuItem` (with any combination of `IsActive` and `MenuName`) and any non-negative integer `depth`, the `TreeNode.Text` produced by `CreateTreeNode(menu, depth)` SHALL equal `"[L{depth}] {icon} {menu.MenuName}"` where `{icon}` is `✓` when `IsActive` is true and `✗` otherwise.

**Validates: Requirements 3.1, 3.2**

### Property 3: TreeNode.Tag holds the original MenuItem

*For any* `MenuItem` and any non-negative integer `depth`, the `TreeNode` produced by `CreateTreeNode(menu, depth)` SHALL have its `Tag` property set to the exact same `MenuItem` instance that was passed in.

**Validates: Requirements 4.1, 4.2**

## Error Handling

- `depth` is always a non-negative integer derived from the recursive call chain starting at 0. No validation is needed; it cannot be negative by construction.
- `menu.MenuName` may be null or empty in malformed data. The existing text-building logic (`$"{icon} {menu.MenuName}"`) already handles this gracefully — the level prefix does not change that behavior.
- No new exception paths are introduced.

## Testing Strategy

This feature involves a pure helper method (`CreateTreeNode`) with no I/O, no external dependencies, and clear input/output behavior. Property-based testing is appropriate.

**PBT library**: [FsCheck](https://fscheck.github.io/FsCheck/) (available for .NET Framework 4.5 via NuGet, integrates with NUnit/xUnit).

### Property-Based Tests

Each test runs a minimum of 100 iterations.

**Test tag format**: `Feature: menu-level-indicator, Property {N}: {property_text}`

| Test | Property | Generator inputs |
|------|----------|-----------------|
| PT-1 | Property 1 — prefix correctness | Random `MenuItem`, random `depth` in [0, 10] |
| PT-2 | Property 2 — original text preserved | Random `MenuItem` (varying `IsActive`, `MenuName`), random `depth` |
| PT-3 | Property 3 — Tag holds MenuItem | Random `MenuItem`, random `depth` |

Generators needed:
- `Arb.MenuItem` — generates `MenuItem` with arbitrary `MenuName` (including empty/whitespace/unicode), arbitrary `IsActive`
- `Arb.Depth` — generates `int` in [0, 10] (covers depths beyond 3 per requirement 3.3)

### Unit / Example-Based Tests

- **UT-1**: `MenuTreeViewManager.CreateTreeNode` with a sample `MenuItem` produces text that does NOT contain `"[L"` — verifies requirement 4.3.
- **UT-2**: `RefreshMenuTree` with a two-level menu list produces root nodes with `[L0]` and child nodes with `[L1]` — integration smoke test for the wiring.

### What is NOT tested

- Database behavior — no DB changes exist.
- `MenuTreeViewManager` tree population beyond UT-1 — out of scope.
- UI rendering / visual appearance — not machine-verifiable.
