# ADR-0057: Zustand + TanStack Query State Management

> **Promoted to Evolith:** This ADR has been elevated to [Evolith ADR-0045 — Zustand + TanStack Query State Management](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/nodejs/0045-zustand-tanstack-query-state-management.md). UMS retains this document as implementation reference.

| Field | Value |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-05-21 |
| **Context** | UMS Web App — State Management Strategy |
| **Deciders** | Architecture Team |

## Problem

React applications need to manage two fundamentally different types of state:
1. **Server state**: Data from APIs (tenants, branches, users) — needs caching, invalidation, deduplication
2. **Client state**: UI state (theme, language, notifications, auth session) — needs reactivity, persistence

Using a single solution for both leads to over-engineering (server state in Redux) or under-engineering (client state with manual fetch logic).

## Decision

Use a **dual-strategy** approach:

### Server State: TanStack Query (React Query)

```typescript
// Queries are cached, deduplicated, and auto-invalidated
const { data, isLoading } = useQuery({
  queryKey: ['tenants', page, filters],
  queryFn: () => tenantService.getTenants(page, filters),
  staleTime: 30_000,
});

// Mutations invalidate queries and show notifications
const createMutation = useNotifiedMutation({
  mutationFn: (data) => tenantService.createTenant(data),
  invalidateKeys: [['tenants']],
  successNotif: () => ({ title: 'Created', message: 'Tenant created' }),
  errorNotif: (err) => ({ title: 'Error', message: getHttpErrorMessage(err) }),
});
```

### Client State: Zustand

```typescript
// Simple, fast, TypeScript-first state management
export const useThemeStore = create<ThemeState>()(
  persist(
    (set) => ({
      isDarkMode: true,
      toggleDarkMode: () => set((s) => ({ isDarkMode: !s.isDarkMode })),
    }),
    { name: 'ums-theme' },
  ),
);
```

### Store Inventory

| Store | Purpose | Persistence |
|---|---|---|
| `auth.store` | User session, authentication state | No (session-only) |
| `theme.store` | Dark/light mode preference | Yes (localStorage) |
| `notification.store` | In-app notifications (cap: 50) | No (session-only) |
| `i18n.store` | Active language (`en`/`es`) | No (syncs with `document.lang`) |
| `devTools.store` | Dev-only overrides (user impersonation) | No (dev-only) |

### Rules

1. **Server data goes through TanStack Query**: Never store API responses in Zustand.
2. **UI state goes through Zustand**: Theme, language, notifications, modal open/close.
3. **No DOM manipulation in stores**: Stores are pure state. Components handle DOM side effects.
4. **Single source of truth**: Each piece of state lives in exactly one store.
5. **Dev tools are isolated**: `devTools.store` is development-only; production code uses `i18n.store` and `auth.store`.

### useNotifiedMutation Pattern

All mutations follow the same pattern via `useNotifiedMutation`:
1. Execute mutation function
2. Invalidate relevant query keys
3. Show success notification
4. Show error notification on failure

## Consequences

**Positive:**
- Automatic caching and deduplication of server data
- Simple, boilerplate-free client state
- Clear separation of concerns
- `persist` middleware for localStorage without custom code
- `useNotifiedMutation` eliminates mutation boilerplate

**Negative:**
- Two libraries to learn and maintain
- Query key management requires discipline
- Zustand stores are not serializable by default (except with `persist`)

## Implementation

- `src/application/stores/` — All Zustand stores
- `src/application/hooks/use-notified-mutation.ts` — Mutation factory
- `src/infrastructure/http/` — HTTP and GraphQL clients
- `vitest.config.ts` — Coverage thresholds for state code

## Related

- ADR-0055: GraphQL/REST Hybrid API Pattern
- ADR-0056: Clean Architecture Layer Boundaries
