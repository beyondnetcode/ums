# Authorization BC — Consistency Rules

> **Bounded Context:** `Ums.Domain.Authorization`
> **Aggregates:** `SystemSuite`, `Role`, `PermissionTemplate`, `Profile`

---

## Role

### State Machine

```
Active ──Deactivate()──► Inactive
Inactive ──Activate()──► Active
```

**Forbidden transitions:**
- `Active → Active` → broken rule `authorization.role_already_active`
- `Inactive → Inactive` → broken rule `authorization.role_already_inactive`

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Create()` | `HierarchyLevel >= 0` | `authorization.role_hierarchy_level_invalid` |
| `Create()` | `PromotionOrder >= 0` | `authorization.role_promotion_order_invalid` |
| `Create()` | Root role must have `HierarchyLevel = 0` | `authorization.root_role_hierarchy_level_invalid` |
| `Create()` | Child role must have `HierarchyLevel > 0` | `authorization.child_role_hierarchy_level_invalid` | ### Cross-Aggregate Dependency Guards

| Operation | Guard (count parameter) | Broken Rule |
|-----------|------------------------|-------------|
| `Deactivate(activeProfileCount, activeChildRoleCount)` | `activeProfileCount > 0` | `ROLE_HAS_ACTIVE_PROFILES` |
| `Deactivate(activeProfileCount, activeChildRoleCount)` | `activeChildRoleCount > 0` | `ROLE_HAS_ACTIVE_CHILD_ROLES` | ### Cascade Rules

| Event | Cascade |
|-------|---------|
| Role deactivated | No automatic cascade. Application layer must deactivate child roles and profiles before calling `Deactivate()`. | ### Orphan Risks

| Risk | Scenario | Mitigation |
|------|----------|-----------|
| Active Profile references Inactive Role | `Deactivate()` without resolving profiles | Pass `activeProfileCount` → application layer deactivates profiles first |
| Active child roles under Inactive parent | `Deactivate()` without deactivating children | Pass `activeChildRoleCount` → application layer deactivates children first |
| Cyclic hierarchy | `Update()` changes ParentRoleId creating a cycle | Application layer validates no cycle before calling `Update()` | ---

## PermissionTemplate

### State Machine

```
Draft ──Publish()──► Published ──Deprecate()──► Deprecated
```

**Forbidden transitions:**
- Any mutation on `Published` or `Deprecated` state → broken rule `authorization.template_not_draft`
- `Publish()` on `Published` → broken rule `authorization.template_already_published`
- `Deprecate()` on `Deprecated` → broken rule `authorization.template_already_deprecated`
- `Publish()` with empty items → broken rule `authorization.template_items_required`

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `AddItem()` / `RemoveItem()` / `SetItem*()` | Template must be in `Draft` state | `authorization.template_not_draft` |
| `Deprecate()` | Template must be in `Published` state | `authorization.template_not_published` |
| `Delete()` | Template must be `Draft` or `Deprecated` | `authorization.template_not_deletable` |
| `AddItem()` | Target+Action combination must be unique | `authorization.template_item_target_already_exists` |
| `Publish()` | Items collection must not be empty | `authorization.template_items_required` | ### Cross-Aggregate Dependency Guards

| Operation | Guard (count parameter) | Broken Rule |
|-----------|------------------------|-------------|
| `Delete(activeProfileCount)` | `activeProfileCount > 0` | `TEMPLATE_HAS_ACTIVE_PROFILES` | ### Cascade Rules

| Event | Cascade |
|-------|---------|
| Template deprecated | No cascade. Profiles that have this template remain valid (template items frozen at publication). New profiles cannot assign a deprecated template. | ### Orphan Risks

| Risk | Scenario | Mitigation |
|------|----------|-----------|
| Profile references Deprecated template | `AssignTemplate()` called with deprecated template | `AssignTemplate()` already validates `status == Published` |
| Delete template with active profiles | Template deleted while profiles depend on it | Pass `activeProfileCount` → application layer handles | ---

## Profile

### State Machine

```
Active ──Deactivate()──► Inactive
Inactive ──Activate()──► Active
```

**Forbidden transitions:**
- `Active → Active` → broken rule `authorization.profile_already_active`
- `Inactive → Inactive` → broken rule `authorization.profile_already_inactive`

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `AssignTemplate()` | Profile must be Active | `authorization.profile_already_inactive` |
| `AssignTemplate()` | Template TenantId must match Profile TenantId | `authorization.template_tenant_mismatch` |
| `AssignTemplate()` | Template must be `Published` | `authorization.template_not_published_for_profile` |
| `AssignTemplate()` | Template must not already be linked | `authorization.profile_template_already_linked` |
| `OverridePermission*()` / `DeactivatePermission()` / `ActivatePermission()` | Profile must be Active | `authorization.profile_already_inactive` |
| `OverridePermission*()` / `DeactivatePermission()` / `ActivatePermission()` | Permission must exist | `authorization.permission_not_found` | ### Cascade Rules

| Event | Cascade |
|-------|---------|
| `Deactivate()` | All owned `ProfilePermission` entities are deactivated inline. |
| `Activate()` | Permissions are NOT automatically re-activated — they retain their last state. | ### Child Entity Ownership

`ProfilePermission` methods (`OverrideAllow`, `OverrideDeny`, `OverrideNeutral`, `Deactivate`, `Activate`) are `public` in the entity class but**must only be called through `Profile`**. The entity being `public` is an implementation convenience — the aggregate boundary is enforced by convention and code review.

### Orphan Risks

| Risk | Scenario | Mitigation |
|------|----------|-----------|
| Active ProfilePermission on Inactive Profile | Pre-cascade-fix state | `Deactivate()` now cascades to all permissions |
| Profile references Inactive Role | Role deactivated after profile creation | Application layer must deactivate profile when role is deactivated | ---

## SystemSuite

### State Machine (Suite)

```
Active ──SetStatus(Inactive)──► Inactive
Inactive ──SetStatus(Active)──► Active
```

### State Machine (Module)

```
Active ──DeactivateModule()──► Inactive
Inactive ──ActivateModule()──► Active
```

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `AddModule()` | Module code must be unique | `system_suite.module_code_not_unique` |
| `ActivateModule()` | Module must not be active | `system_suite.module_already_active` |
| `DeactivateModule()` | Module must not be inactive | `system_suite.module_already_inactive` |
| `AddMenu()` | Module must be Active | `system_suite.module_inactive_cannot_add_menu` |
| `AddMenu()` | Menu code must be unique within module | `system_suite.menu_code_not_unique` |
| `AddSubMenu()` | SubMenu code must be unique within menu | `system_suite.submenu_code_not_unique` |
| `AddOption()` | Option code must be unique within submenu | `system_suite.option_code_not_unique` |
| `AddDomainResource()` DomainMethod | Parent must be provided | `authorization.domain_method_requires_parent` |
| `AddDomainResource()` DomainMethod | Parent resource must exist | `authorization.parent_resource_not_found` |
| `AddDomainResource()` DomainMethod | Parent must not itself be a DomainMethod | `authorization.domain_method_cannot_be_parent` | ### Cross-Aggregate Dependency Guards

| Operation | Guard (count parameter) | Broken Rule |
|-----------|------------------------|-------------|
| `RemoveDomainResource(templateItemCount)` | `templateItemCount > 0` | `DOMAIN_RESOURCE_HAS_TEMPLATE_ITEMS` |
| `RemoveModule(activeMenuCount)` | `activeMenuCount > 0` | `MODULE_HAS_ACTIVE_MENUS` | ### Cascade Rules

| Event | Cascade |
|-------|---------|
| `DeactivateModule()` | Module marks itself inactive. Active menus under the module are**not**automatically deactivated — application layer handles navigation consequences. |
| `SetStatus(Inactive)` for Suite | No automatic cascade to modules. Application layer must deactivate modules first. | ### Orphan Risks

| Risk | Scenario | Mitigation |
|------|----------|-----------|
| Active Menu inside Inactive Module | Module deactivated without cascading | Pass `activeMenuCount` to application layer; it cascades or warns |
| DomainResource with TemplateItems being removed | `RemoveDomainResource()` without checking templates | Pass `templateItemCount` → application enforces |
| DomainResource cycle | Child resource assigned its descendant as parent | Application layer validates acyclicity before `AddDomainResource()` | ---

*Part of [consistency-rules/index.md](./index.md)*
