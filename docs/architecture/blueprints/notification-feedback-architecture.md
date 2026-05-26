# Notification & Feedback Architecture

**Type:** Architecture Blueprint  
**Status:** Accepted · Implemented 2026-05-26  
**Runtime:** React 18 · TypeScript · TanStack Query v5 · Zustand · Axios  
**Code location:** `src/apps/ums.web-app/src`

---

## Purpose

Every user-initiated mutation (create, update, delete, status change) must produce visible,
actionable feedback. This blueprint defines how UMS surfaces business errors and confirmations
through a **dual-visibility system**: an ephemeral toast for immediate awareness, and a
persistent notification drawer for history and auditability.

The design solves three specific failure modes:

| Failure mode | Symptom | Root cause |
|---|---|---|
| Silent 400/409 | User retries not knowing the action failed | Error goes to store but drawer is closed |
| Generic "Something went wrong" | User cannot act on the error | Backend `detail` field not extracted |
| Duplicated boilerplate | Every hook re-implements success/error wiring | No shared mutation factory |

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│                                                             │
│   Component calls mutateAsync()                             │
│        │                                                    │
│        ▼                                                    │
│   useNotifiedMutation ──► mutationFn (service call)         │
│        │           ◄── onSuccess / onError (TanStack Query) │
│        │                                                    │
│        ▼                                                    │
│   getHttpErrorMessage(error, fallback)                      │
│        │  REST:    response.data.detail                     │
│        │           response.data.message                    │
│        │           error.message                            │
│        │  GraphQL: graphQLErrors[0].message                 │
│        │                                                    │
│        ▼                                                    │
│   useNotificationStore.addNotification()  ◄── Zustand       │
│        │                                                    │
│        ├──► ToastQueue (new notification detected)          │
│        │         floating, auto-dismiss, bottom-right       │
│        │         error: 6 s · warning: 5 s                  │
│        │         info: 4 s · success: 3.5 s                 │
│        │                                                    │
│        └──► NotificationCenter bell badge                   │
│                  M3Drawer, full history, max 50 entries      │
└─────────────────────────────────────────────────────────────┘
```

---

## Component Inventory

### `getHttpErrorMessage(error, fallback)` — Error Extraction
**File:** `src/application/errors/http-error.ts`

Normalizes any thrown value into a human-readable string. Reads, in priority order:

1. `error.graphQLErrors[0].message` — HotChocolate error
2. `error.response.data.detail` — RFC 7807 Problem Details (`detail` field)
3. `error.response.data.message` — legacy or custom field
4. `error.message` — Axios network error
5. `fallback` — the static string provided by the calling hook

This is the **only** place in the codebase that knows how to read a backend error. No component
or hook should parse `error.response` directly.

---

### `useNotifiedMutation` — Mutation Factory
**File:** `src/application/hooks/use-notified-mutation.ts`

```typescript
useNotifiedMutation<TData, TVariables>({
  mutationFn,       // (variables) => Promise<TData>
  invalidateKeys,   // QueryKey[] to invalidate on success
  successNotif,     // (data: TData) => { title, message, type? }
  errorNotif,       // (error: unknown) => { title, message, type? }
  options?,         // extra TanStack options (excluding onSuccess/onError)
})
```

`onError` always calls `getHttpErrorMessage(error, notif.message)` — the `errorNotif` only needs to
supply the **title** and a **fallback message**. The backend's actual error detail is extracted
automatically and shown in the toast/drawer.

**Rule:** Every mutation in UMS is created via this factory. Direct `useMutation` is not permitted
in feature hooks.

---

### `useNotificationStore` — Centralized State
**File:** `src/application/stores/notification.store.ts`

Zustand store. Single source of truth for notifications. Capped at 50 entries (FIFO eviction).

```typescript
interface AppNotification {
  id: string;          // crypto.randomUUID()
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  timestamp: string;   // ISO-8601
  read: boolean;
}
```

---

### `ToastQueue` — Ephemeral Visibility
**File:** `src/presentation/shared/components/ToastQueue.tsx`

Subscribes to the notification store via a `seenRef` Set to detect new entries without
re-rendering on every mark-as-read. Renders a stacked column of `ToastItem` components
at `fixed bottom-6 right-6 z-[100]`. Each toast:

- Slides in from the right (`translate-x-0 opacity-100`)
- Auto-dismisses after the type-specific delay
- Can be manually dismissed with the × button
- Stack cap: **4 visible toasts** (additional entries are still stored in `NotificationCenter`)

Auto-dismiss delays:

| Type | Delay | Rationale |
|---|---|---|
| `error` | 6 s | User must read and decide what to do |
| `warning` | 5 s | Action may be needed |
| `info` | 4 s | Informational, lower urgency |
| `success` | 3.5 s | Confirmation, no action required |

---

### `NotificationCenter` — Persistent History
**File:** `src/presentation/shared/components/NotificationCenter.tsx`

M3Drawer opened by the Bell icon in `TopAppBar`. Shows all notifications in reverse
chronological order. Provides mark-all-as-read and clear-all. Unread count is shown
as a pulsing badge on the Bell.

---

## Data Flow — Business Error (400)

```
1. User clicks "Add Module" with blank description
2. handleAddModule() calls addModuleMutation.mutateAsync(payload)
3. httpClient.POST /system-suites/{id}/modules → HTTP 400
   Body: { "detail": "Description: is required", "status": 400, ... }
4. TanStack Query catches the AxiosError → onError(error)
5. useNotifiedMutation.onError:
     title   = errorNotif(error).title         → "Error al Registrar Módulo"
     message = getHttpErrorMessage(error, ...)  → "Description: is required"
6. addNotification({ title, message, type: 'error' })
7. ToastQueue detects new entry (unseen ID) → renders ToastItem (6 s)
8. Bell badge increments by 1 (pulsing)
9. User reads toast: "Description: is required" → corrects the form
10. Toast auto-dismisses after 6 s
```

---

## Extension Points

### Adding a new mutation
```typescript
export const useAddMenu = (systemSuiteId: string, moduleId: string) => {
  const t = useI18n();
  return useNotifiedMutation({
    mutationFn: (payload: AddMenuPayload) =>
      systemSuiteService.addMenu(systemSuiteId, moduleId, payload),
    invalidateKeys: [['system-suites', systemSuiteId]],
    successNotif: () => ({ title: t.notifMenuAdded, message: t.notifMenuAddedMsg }),
    errorNotif: ()  => ({ title: t.notifMenuAddFailed, message: t.notifMenuAddFailedMsg }),
  });
};
```

### Extending error extraction
If the backend adds a `errors` array (FluentValidation field errors), update only
`getHttpErrorMessage`:

```typescript
// src/application/errors/http-error.ts
const fieldErrors = data?.errors;
if (fieldErrors && typeof fieldErrors === 'object') {
  const messages = Object.values(fieldErrors).flat().filter(Boolean);
  if (messages.length) return messages.join(' · ');
}
```

No other layer requires changes.

### Changing toast duration
Update `DISMISS_DELAY_MS` in `ToastQueue.tsx`. No other file is affected.

---

## Backend Contract

The backend's `GlobalExceptionHandler` and `Result.ToNoContent()` must return RFC 7807
Problem Details for all 4xx responses:

```json
{
  "type":   "https://httpstatuses.io/400",
  "title":  "Bad Request",
  "status": 400,
  "detail": "Description: is required",
  "instance": "/api/v1/system-suites/{id}/modules",
  "traceId": "..."
}
```

`getHttpErrorMessage` reads `response.data.detail`. If the backend returns a different
shape, update `asHttpError()` in `http-error.ts` — not the hooks or components.

---

## Testing

| Layer | What to test | Tool |
|---|---|---|
| `getHttpErrorMessage` | Extracts `detail`, `message`, falls back correctly for all error shapes | Vitest unit |
| `useNotifiedMutation` | Calls `addNotification` with correct title and extracted message on error | `renderHook` + MSW |
| `useNotificationStore` | `addNotification` prepends, caps at 50, `markAsRead` flips flag | Vitest unit |
| `ToastQueue` | Renders on new notification, auto-dismisses after delay, × removes immediately | RTL + fake timers |
| `NotificationCenter` | Badge count, drawer opens, mark-all-as-read clears badge | RTL |

---

## Related Documents

- [Observability Architecture Flow](./observability-architecture-flow.md)  
- [Shell Library Architecture](./shell-library-architecture.md)

---

**[Back to Blueprints Index](./index.md)** | **[Versión en Español](../../architecture/blueprints-es/notification-feedback-architecture.md)**
