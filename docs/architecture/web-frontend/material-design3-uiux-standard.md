# Material Design 3 UI/UX Design Specification

This document establishes the UI/UX design standards for the React web applications of the User Management System (UMS). These guidelines align our implementations with the principles of Material Design 3 (M3), ensuring a clean, minimal, professional, consistent, and highly accessible user experience.

---

## 1. Visual Theme and Semantic Design Tokens

The application interface is built upon the Material Design 3 semantic color palette, mapped through HSL-based CSS variables in the theme system. This ensures robust support for light and dark modes while maintaining high-contrast boundaries.

### 1.1. Core Palette Mapping

| Design Token Role | CSS Variable | HSL Mapping Function | Light Mode Role | Dark Mode Role |
|---|---|---|---|---|
| Primary | `--color-m3-primary` | `hsl(var(--color-m3-primary))` | Focus brand accents | Active brand highlights |
| Primary Container | `--color-m3-primary-container` | `hsl(var(--color-m3-primary-container))` | Selected lists background | Accent card backgrounds |
| Secondary | `--color-m3-secondary` | `hsl(var(--color-m3-secondary))` | Muted text, descriptors | Subtitle elements |
| Surface | `--color-m3-surface` | `hsl(var(--color-m3-surface))` | Screen backgrounds | Pane/module backgrounds |
| Surface Container | `--color-m3-surface-container` | `hsl(var(--color-m3-surface-container))` | Main layout container panels | Inner drawer layers |
| Outline | `--color-m3-outline` | `hsl(var(--color-m3-outline))` | High-contrast control borders | Subtle separator borders |
| Error | `--color-m3-error` | `hsl(var(--color-m3-error))` | Destructive actions, errors | Negative validation state |

### 1.2. Interactive State Overlays
All interactive elements (buttons, row items, grid items) must implement visual state feedback using CSS transition rules:
- **Hover State:** Apply a overlay background layer using `opacity: 0.08` of the state color or shift brightness slightly.
- **Focus State:** Display a high-contrast focus outline using `outline: 3px solid hsl(var(--color-m3-primary))` and `outline-offset: 2px` on `focus-visible`.
- **Active (Pressed) State:** Increase state overlay opacity to `0.12`.
- **Disabled State:** Render controls with `opacity: 0.38` and strip hover/active interaction listeners.

---

## 2. Layout Grid and Spatial Scale

UMS enforces a standardized grid layout based on consistent responsive break points and spacing increments to keep modules structurally cohesive.

### 2.1. Spacing Increments
To achieve robust layout alignments, all padding, margins, and grid gaps must use values derived from an 8px base scale:
- **4px (0.25rem):** Micro-spacers (e.g., label to input spacing).
- **8px (0.5rem):** Compact items (e.g., cell padding, list gaps).
- **12px (0.75rem):** Mid-spacers (e.g., card internal margins, form gaps).
- **16px (1.0rem):** Main padding (e.g., page gutters, standard gaps).
- **24px (1.5rem):** Spacious gutters (e.g., section separators).

### 2.2. Gestalt Proximity Rules for Forms
- The vertical space between a field label and its associated input control must be strictly less than the space separating one form group from the next.
- Recommended form layout values:
  - Spacing from label to input: 4px (0.25rem).
  - Spacing from input to helper/error text: 4px (0.25rem).
  - Spacing between adjacent form groups: 16px (1.0rem).

---

## 3. Typographic Scale

Typography must maintain absolute semantic consistency by mapping directly to the following Material Design 3 type scales using relative units (`rem`):

| M3 Scale Role | CSS Class Equivalent | Font Size | Font Weight | Line Height | Case Usage |
|---|---|---|---|---|---|
| Headline Medium | `.text-m3-headline-medium` | 1.75rem (28px) | Semi-Bold (600) | 1.3 | Standard |
| Title Large | `.text-m3-title-large` | 1.375rem (22px) | Medium (500) | 1.3 | Standard |
| Title Medium | `.text-m3-title-medium` | 1.0rem (16px) | Medium (500) | 1.4 | Standard |
| Body Medium | `.text-m3-body-medium` | 0.875rem (14px) | Regular (400) | 1.5 | Standard |
| Label Large | `.text-m3-label-large` | 0.875rem (14px) | Medium (500) | 1.4 | Sentence |
| Label Small | `.text-m3-label-small` | 0.6875rem (11px) | Medium (500) | 1.5 | Uppercase |

---

## 4. Common UI Component Standards

All UMS modules must utilize these standardized presentation components rather than implementing ad-hoc styling variations.

### 4.1. Page Dashboard Shell (`PageDashboardShell` & `MasterDetailLayout`)
- **Structure:** Encapsulates the entire viewport space. Divides layout into a navigation zone, main listing (master), and a detail inspector panel (detail).
- **Splitter Element:** The splitter bar must have an accessible description (`splitterLabel="Resize panel"`). It should support smooth mouse drag resizing and collapse smoothly.

### 4.2. Action Buttons (`M3Button`)
- **Filled Variant:** For primary actions on a screen (e.g., "Register tenant", "Save changes"). Maximum one primary filled button per dashboard pane.
- **Outlined Variant:** For secondary actions (e.g., "Cancel", "Edit profile", "Reset filters").
- **Text Variant:** For inline row options or modal cancellation keys.
- **Destructive Class:** High-risk actions (e.g., "Deactivate", "Block") must use HSL error colors on hover and press states.

### 4.3. Text Inputs (`M3TextField`)
- **Semantic Structure:** Wrapped in `<label>` or linked explicitly using `for` and `id` parameters to assist assistive readers.
- **Autofill Config:** Autofills must be mapped explicitly to maximize user experience:
  - Email fields → `autocomplete="username"` or `autocomplete="email"`.
  - Password fields → `autocomplete="current-password"` (login) or `autocomplete="new-password"` (creation).
  - Normal text inputs → `autocomplete="off"` or explicit contextual roles.
- **Interactive Outlines:** Border elements must have high visual contrast against surface panels (minimum contrast ratio of 4.5:1).

### 4.4. Dialogs (`M3Dialog` & `ConfirmDialog`)
- **Scrim Overlay:** Darkened surface backdrop (`opacity: 0.32` to `0.5`) with blur effect (`backdrop-filter: blur(4px)`) to keep focus centered.
- **Modal Closure:** Scrim click should safely trigger dismissal callbacks, unless forced transaction is active.

---

## 5. Standard Form Constraints and Validation Timing

Form elements must implement non-intrusive validation constraints, avoiding premature error warnings while the user types:

| Interaction Phase | Validation Event | Behavior Expected | Purpose |
|---|---|---|---|
| **Active typing** | `input` | Clear existing error states immediately. | Prevent blocking or distracting the user. |
| **Exiting control** | `blur` / `focusout` | Perform validation checks and render errors. | Contextual validation once user finishes. |
| **Form submission** | `submit` | Check all inputs, block payload, route focus. | Gatekeeper: intercept bad payload. |

---

## 6. Responsive and Accessibility Checklist

All frontend React screens must pass this automated and manual checklist prior to deployment:

- `[ ]` **Minimum Tap Targets:** All buttons and interactive inputs must have a minimum clickable height of `48px` on mobile layouts.
- `[ ]` **Autofill Optimization:** Inputs use correct `inputmode` configurations (e.g. `inputmode="numeric"` for digit codes).
- `[ ]` **Keyboard Navigation:** Tab loops flow sequentially. Modals capture focus until closed.
- `[ ]` **Visual Contrast:** All text content satisfies a minimum contrast ratio of 4.5:1 against backdrop panels.
- `[ ]` **Focus Indicators:** Focus-visible states trigger solid high-contrast borders with no hidden outlines.

---

## 7. CQRS Architectural Notes and TODOs

> [!IMPORTANT]
> **REST vs GraphQL Migration Strategy (TODO):**
> As decided, all CQRS query operations in frontend services are temporarily routed through REST endpoints (via `httpClient`) instead of GraphQL endpoints. This temporary measure is put in place to avoid active schema/infrastructure issues.
> - **TODO:** Revisit GraphQL query performance and schema alignment at a later phase to migrate reads back to GraphQL where appropriate.
> - **Affected components:** `userAccountService` and other core identity query wrappers.
