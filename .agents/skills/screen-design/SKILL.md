---
name: screen-design
description: Design practical application screens, UI layouts, user flows, and responsive interface states. Use when Codex needs to create, redesign, review, or implement a screen for desktop, web, WinForms, internal tools, dashboards, CRUD forms, settings pages, reports, or data-entry workflows, especially when layout, component choice, visual hierarchy, usability, accessibility, or responsive behavior matter.
---

# Screen Design

## Overview

Use this skill to turn a user goal into a usable screen design and, when asked, an implementation that fits the existing app. Prefer practical workflow clarity over decorative presentation.

## Workflow

1. Identify the screen's job: primary user, task, data being viewed or changed, success state, and likely failure states.
2. Inspect the existing app before designing: framework, design system, spacing, colors, typography, icons, form patterns, navigation, and validation behavior.
3. Choose the screen structure around the user's workflow:
   - Use dense, scan-friendly layouts for admin, CMS, ERP, CRM, reporting, and operational tools.
   - Use clear step-by-step flows for setup, import, review, approvals, checkout, and destructive actions.
   - Use dashboards only when the user needs comparison, monitoring, or prioritization.
4. Define states before styling: empty, loading, populated, validation errors, permission denied, partial success, destructive confirmation, and offline or API failure when relevant.
5. Implement with the app's existing components and conventions. Add new abstractions only when repeated UI logic or behavior genuinely needs one.
6. Verify the result visually and functionally at the expected viewport sizes. Fix overflow, overlap, cramped controls, missing focus states, and inaccessible contrast before finishing.

## Layout Standards

- Put the core working surface on the first screen. Do not add a landing page unless the user explicitly needs marketing or onboarding.
- Give primary actions predictable locations: top-right for page-level commands, inline near table rows for row actions, bottom-right for modal confirmation.
- Keep forms aligned and scannable. Group related fields with section labels, not decorative cards inside cards.
- Use tables or grids when users compare many records. Use cards only for repeated items, summaries, modals, or genuinely framed tools.
- Keep typography proportional to context. Use compact headings inside panels, dialogs, sidebars, and toolbars.
- Reserve visual emphasis for decision points, status, changed data, validation, and primary actions.
- Avoid one-note palettes. Use color to encode status or emphasis, not as the whole design idea.
- Ensure controls have stable dimensions so hover states, icons, counters, and dynamic labels do not shift layout.

## Component Choices

- Use icons for common tool actions such as save, edit, delete, refresh, search, filter, import, export, undo, redo, zoom, and settings.
- Use buttons for clear commands, segmented controls for modes, toggles or checkboxes for binary settings, sliders or numeric inputs for numeric values, tabs for sibling views, and menus for option sets.
- Prefer inline validation for forms. Use summaries only when errors span multiple sections or the workflow is long.
- Use confirmation dialogs for destructive, expensive, or hard-to-reverse actions. Include the affected object name or count.
- Use progressive disclosure for advanced fields, filters, dangerous actions, or rarely used settings.

## Accessibility And Responsiveness

- Maintain keyboard reachability, visible focus states, labels for inputs, and sufficient contrast.
- Do not rely on color alone for meaning. Pair status colors with text, icons, or position.
- Ensure text never overlaps controls or adjacent content. Let text wrap, truncate intentionally, or resize containers.
- Test at desktop and narrow mobile widths for web apps. For desktop-only apps, test realistic window sizes and high-DPI scaling.
- Keep critical actions reachable without horizontal scrolling.

## Output Expectations

When designing only, provide:

- Screen purpose and primary workflow.
- Layout sections in visual order.
- Key components and states.
- Responsive behavior and accessibility notes.

When implementing, also:

- Follow local code style and component conventions.
- Reuse existing icons, spacing tokens, color variables, and form helpers.
- Run available checks or visual verification. For local web frontends, use the in-app Browser when the target URL is known.
