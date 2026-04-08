# Requirements Document

## Introduction

This feature adds visual level indicators to each node in the TreeView on `MenuManagementForm` in the CMS WinForms application. Currently, the TreeView displays menu items (e.g., หน้าแรก, จัดการไฟล์, จัดการผู้ใช้, บันทึกการตรวจสอบ) without any visual cue about their depth in the hierarchy. Administrators managing the menu structure need to quickly identify each item's depth level (Level 0 = root, Level 1 = child, Level 2 = grandchild, Level 3 = great-grandchild) to understand and maintain the menu hierarchy correctly.

The change is purely presentational and confined to `MenuManagementForm` and `MenuTreeViewManager` in `CMS.Presentation`. No database schema changes, no service-layer changes, and no domain model changes are required.

## Glossary

- **MenuManagementForm**: The WinForms form (`MenuManagementForm.cs`) used by Admin users to create, edit, and delete menu items.
- **TreeView**: The `treeMenu` TreeView control on `MenuManagementForm` that displays the menu hierarchy.
- **TreeNode**: A single node in the TreeView representing one `MenuItem`.
- **Level Indicator**: A text prefix appended to a TreeNode's display text showing its depth (e.g., `[L0]`, `[L1]`, `[L2]`, `[L3]`).
- **Node Depth**: The zero-based depth of a TreeNode within the TreeView hierarchy. Root nodes have depth 0; their direct children have depth 1, and so on.
- **MenuTreeViewManager**: The helper class (`MenuTreeViewManager.cs`) responsible for populating and refreshing the TreeView.
- **MenuItem**: The domain model (`CMS.Domain.Models.MenuItem`) representing a menu entry, identified by `MenuItemID` and optionally linked to a parent via `ParentMenuItemID`.
- **Admin**: A user with `AppRole == 4`, the only role permitted to access `MenuManagementForm`.

## Requirements

### Requirement 1: Display Level Indicator on Each TreeNode

**User Story:** As an Admin, I want each menu item in the TreeView to show its hierarchy level, so that I can immediately understand the depth of any menu item without having to count parent nodes manually.

#### Acceptance Criteria

1. WHEN the TreeView is populated, THE MenuManagementForm SHALL prefix each TreeNode's display text with a level indicator in the format `[L{depth}]` where `{depth}` is the zero-based depth of the node (0 for root, 1 for child, 2 for grandchild, 3 for great-grandchild).
2. WHEN a root-level `MenuItem` (one with `ParentMenuItemID == null`) is rendered as a TreeNode, THE MenuManagementForm SHALL display the level indicator `[L0]` before the node's existing text.
3. WHEN a child `MenuItem` (depth 1) is rendered as a TreeNode, THE MenuManagementForm SHALL display the level indicator `[L1]` before the node's existing text.
4. WHEN a grandchild `MenuItem` (depth 2) is rendered as a TreeNode, THE MenuManagementForm SHALL display the level indicator `[L2]` before the node's existing text.
5. WHEN a great-grandchild `MenuItem` (depth 3) is rendered as a TreeNode, THE MenuManagementForm SHALL display the level indicator `[L3]` before the node's existing text.
6. THE MenuManagementForm SHALL derive the node depth from the TreeNode's position in the TreeView hierarchy at render time, without requiring any additional database column or domain model property.

### Requirement 2: Level Indicator Persists After Tree Refresh

**User Story:** As an Admin, I want the level indicators to remain accurate after any tree refresh operation, so that the display stays consistent when I add, edit, or delete menu items.

#### Acceptance Criteria

1. WHEN the TreeView is refreshed (via `RefreshMenuTree`), THE MenuManagementForm SHALL re-render all TreeNodes with correct level indicators reflecting the current hierarchy.
2. WHEN a new `MenuItem` is saved and the tree reloads, THE MenuManagementForm SHALL display the correct level indicator for the newly added node based on its actual depth in the refreshed tree.
3. WHEN a `MenuItem` is deleted and the tree reloads, THE MenuManagementForm SHALL display correct level indicators for all remaining nodes without stale or incorrect depth labels.

### Requirement 3: Level Indicator Format Consistency

**User Story:** As an Admin, I want the level indicator format to be uniform across all nodes, so that the display is predictable and easy to scan.

#### Acceptance Criteria

1. THE MenuManagementForm SHALL use the format `[L{depth}] {existingText}` (bracket-enclosed level tag followed by a single space, then the original node text) for every TreeNode in the TreeView.
2. THE MenuManagementForm SHALL preserve the existing node text content (including the active/inactive icon prefix `✓` or `✗` and the `MenuItem.MenuName`) unchanged after the level indicator is prepended.
3. IF a `MenuItem` exists at a depth greater than 3, THE MenuManagementForm SHALL still display the correct numeric depth in the level indicator (e.g., `[L4]`, `[L5]`) without truncating or capping the value.

### Requirement 4: No Impact on Existing Functionality

**User Story:** As an Admin, I want the level indicator to be a display-only change, so that all existing menu management operations (select, add, edit, delete, save) continue to work correctly.

#### Acceptance Criteria

1. WHEN a TreeNode with a level indicator is selected, THE MenuManagementForm SHALL populate the detail form fields using the `MenuItem` stored in `TreeNode.Tag`, not by parsing the node's display text.
2. THE MenuManagementForm SHALL store the original `MenuItem` object in `TreeNode.Tag` unchanged, so that save and delete operations are unaffected by the level indicator prefix.
3. WHEN the `MenuTreeViewManager` populates its own TreeView (used in `MDI_Container`), THE MenuTreeViewManager SHALL NOT display level indicators, as this feature applies only to the admin management view in `MenuManagementForm`.
