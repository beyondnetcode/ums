# Authorization BC — Aggregate Architecture

> **Language:** [English](./index.md) | [Español](../../domain-es/authorization/index.md)

**Bounded Context:** Authorization (`Ums.Domain.Authorization`)  
**Aggregate Roots:** `SystemSuite`, `PermissionTemplate`, `Profile`

---

### Application Suite Model
The suite structures govern the navigational and action menus of the system:
- [SystemSuite](./system-suite.md) (Aggregate Root) — Top-level system applications (e.g. Admin Portal, Branch Portal).
- [Module](./module.md) (Owned Entity) — Modular functional sections within a suite.
- [Menu](./menu.md) (Owned Entity) — Graphical menu interfaces.
- [SubMenu](./sub-menu.md) (Owned Entity) — Nested submenu blocks.
- [Option](./option.md) (Owned Entity) — Specific screen/view configuration anchors.
- [Action](./action.md) (Owned Entity) — Fine-grained action tokens (e.g., READ, WRITE, EXPORT) to secure individual behaviors.

### Permissions & Templates
- [PermissionTemplate](./permission-template.md) (Aggregate Root) — Reusable, standardized permission packs.
- [PermissionTemplateItem](./permission-template-item.md) (Owned Entity) — Specific action mappings defined within a template.

### Security Profiles
- [Profile](./profile.md) (Aggregate Root) — Roles assigned to users scoped by context (GLOBAL, TENANT, or BRANCH).
- [ProfilePermission](./profile-permission.md) (Owned Entity) — Specific allowed actions mapped to a profile.

---

**[Back to Domain Index](../index.md)**
