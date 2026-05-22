# Frontend Audit Playbook

## Purpose

Use this playbook when reviewing or implementing screens, reusable components, layouts, route structure, state management, accessibility, and design-system alignment.

## Mandatory Checks

1. Shared layout logic lives in reusable base layout and page-shell components.
2. Generic list, search, right-panel, footer actions, and common controls are reusable and domain-agnostic.
3. Domain-specific logic is injected through props, hooks, or composition rather than copied.
4. Accessibility is present for interactive controls.
5. Routing, state, services, and UI remain separated.
6. Shared components do not leak domain-specific business rules.
7. The resulting structure improves reuse across modules and future micro-frontend extraction if needed.

## Preferred Design Direction

- composition over inheritance
- reusable hooks
- thin screens
- Material Design 3 consistency
- common components in shared layers, domain components in feature layers
