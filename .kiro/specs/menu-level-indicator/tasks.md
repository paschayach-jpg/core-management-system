# Implementation Plan: Menu Level Indicator

## Overview

Modify three methods in `MenuManagementForm.cs` to thread a `depth` integer through the tree-building call chain and prefix each `TreeNode`'s text with `[L{depth}] `.

## Tasks

- [x] 1. Update `CreateTreeNode` to accept and apply depth
  - Change signature from `CreateTreeNode(MenuItem menu)` to `CreateTreeNode(MenuItem menu, int depth)`
  - Prepend `$"[L{depth}] "` to the existing `"{icon} {menu.MenuName}"` text
  - Keep `TreeNode.Tag = menu` unchanged
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 3.1, 3.2, 3.3, 4.1, 4.2_

  - [ ]* 1.1 Write property test PT-1: prefix correctness
    - **Property 1: Level indicator prefix is correct for any depth**
    - Generate random `MenuItem` and random `depth` in [0, 10]; assert `node.Text.StartsWith($"[L{depth}] ")`
    - Use FsCheck via NuGet in a new test project targeting .NET Framework 4.5
    - **Validates: Requirements 1.1, 1.2, 1.3, 1.4, 1.5, 2.1, 3.1, 3.3**

  - [ ]* 1.2 Write property test PT-2: original text preserved
    - **Property 2: Original node text is preserved after prefixing**
    - Generate random `MenuItem` (varying `IsActive`, `MenuName`) and random `depth`; assert full text equals `$"[L{depth}] {icon} {menu.MenuName}"`
    - **Validates: Requirements 3.1, 3.2**

  - [ ]* 1.3 Write property test PT-3: Tag holds original MenuItem
    - **Property 3: TreeNode.Tag holds the original MenuItem**
    - Generate random `MenuItem` and random `depth`; assert `node.Tag` is the same instance passed in
    - **Validates: Requirements 4.1, 4.2**

- [x] 2. Update `RefreshMenuTree` to pass `depth: 0`
  - Change the `CreateTreeNode(menu)` call inside the root-node loop to `CreateTreeNode(menu, 0)`
  - No other changes to this method
  - _Requirements: 1.2, 2.1_

- [x] 3. Update `LoadChildNodes` to accept and propagate depth
  - Change signature from `LoadChildNodes(TreeNode parentNode, int parentMenuItemID)` to `LoadChildNodes(TreeNode parentNode, int parentMenuItemID, int depth)`
  - Change the `CreateTreeNode(child)` call to `CreateTreeNode(child, depth)`
  - Change the recursive `LoadChildNodes(node, child.MenuItemID)` call to `LoadChildNodes(node, child.MenuItemID, depth + 1)`
  - Update the call site in `RefreshMenuTree` from `LoadChildNodes(node, menu.MenuItemID)` to `LoadChildNodes(node, menu.MenuItemID, 1)`
  - _Requirements: 1.3, 1.4, 1.5, 2.1, 3.3_

  - [ ]* 3.1 Write unit test UT-2: RefreshMenuTree wiring smoke test
    - Construct a two-level `_allMenus` list (one root, one child); call `RefreshMenuTree`; assert root node text starts with `[L0]` and child node text starts with `[L1]`
    - **Validates: Requirements 1.2, 1.3, 2.1**

- [x] 4. Checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Verify `MenuTreeViewManager` is untouched
  - Confirm `MenuTreeViewManager.cs` has no changes
  - _Requirements: 4.3_

  - [ ]* 5.1 Write unit test UT-1: MenuTreeViewManager produces no level indicator
    - Call `MenuTreeViewManager.CreateTreeNode` with a sample `MenuItem`; assert the resulting `TreeNode.Text` does NOT contain `"[L"`
    - **Validates: Requirements 4.3**

## Notes

- Tasks marked with `*` are optional and can be skipped for a faster MVP
- All changes are confined to `CMS.Presentation/MenuManagementForm.cs`
- `TreeNode.Tag` must remain the raw `MenuItem` instance — no parsing of display text anywhere
- FsCheck targets .NET Framework 4.5 and is available via NuGet
