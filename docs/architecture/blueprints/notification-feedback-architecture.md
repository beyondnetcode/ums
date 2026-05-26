# Notification and Feedback Architecture

**Type:** Architecture Blueprint  
**Status:** Accepted, amended 2026-05-26
**Runtime:** React 18, TypeScript, TanStack Query v5, Zustand, Axios
**Code location:** `src/apps/ums.web-app/src`
**Governing decision:** [ADR-0066: Actionable User Error Contract and Correlated Diagnostics](../adrs/0066-actionable-user-error-contract.md)

---

## Purpose

Every user-initiated command must produce visible feedback that is both actionable and safe. UMS uses a dual-visibility system:

- An ephemeral toast communicates immediate success or failure.
- A notification center retains relevant feedback until it expires or the user explicitly removes it.

Feedback is not a technical diagnostic surface. The browser receives only content approved for users and a server-generated `errorId` GUID that support can search in Serilog and Grafana Loki.

## Problems Addressed

| Failure mode | User impact | Standard response |
|---|---|---|
| Silent command failure | User repeats an operation without knowing it failed | Always emit a success or error notification |
| Generic validation failure | User cannot correct input | Present an approved, actionable `userMessage` |
| Technical detail leakage | User sees stack, class, namespace, SQL, or internals | Display only approved fields; keep diagnostics in logs |
| Lost support correlation | Support cannot locate a failure report | Include server-generated `errorId` in response, header, and log |
| Ineffective close control | Dismissed message remains in active history | Manual close removes the notification entry |

## Core Rules

### 1. One Safe Extraction Boundary

`getHttpErrorMessage(error, fallback)` is the only client function that chooses user-facing error content. Components and feature hooks must not display raw response fields.

Allowed display sources:

1. REST `userMessage`, explicitly approved by the API boundary.
2. A localized message derived from `messageKey` and safe parameters when implemented.
3. A local fallback owned by the feature.

Forbidden display sources:

- Arbitrary REST `detail` or `message`.
- Raw GraphQL error messages.
- JavaScript or Axios exception messages.
- Stack traces, namespaces, class names, SQL details, token values, secret values, or PII.

### 2. Actionable Expected Errors

Validation and business-rule failures may be displayed when the API has classified them as safe. Examples include:

- "The module could not be registered because the Description field exceeds the 500-character maximum. Shorten it and try again."
- "The module could not be registered because another module already uses the same Code. Enter a different code."

An unexpected infrastructure or programming failure always receives generic guidance plus the tracking code.

### 3. Centralized Notification Flow

All feature mutations use `useNotifiedMutation`. It performs the operation, invalidates successful cache entries, selects approved feedback, and adds a notification to the single Zustand store.

| Responsibility | Component |
|---|---|
| Safe error extraction | `src/application/errors/http-error.ts` |
| Command notification wrapper | `src/application/hooks/use-notified-mutation.ts` |
| Notification state | `src/application/stores/notification.store.ts` |
| Ephemeral display | `src/presentation/shared/components/ToastQueue.tsx` |
| History display | `src/presentation/shared/components/NotificationCenter.tsx` |

### 4. Tracking Code Visibility

When a failed operation carries an `errorId`, the notification includes support guidance such as:

```text
If you need more details, contact an administrator and provide this error ID: <errorId>.
```

The code is safe to show because it is a support reference, not an internal exception description. It must correlate with the server log context.

### 5. Dismissal Semantics

| User interaction | Toast | Notification center |
|---|---|---|
| Automatic timeout | Removed from ephemeral display | May remain in history |
| Manual close button | Removed immediately | Corresponding active entry is removed |
| Clear all in center | Removed when present | All entries removed |

Manual dismissal is an explicit user intent to remove that feedback item, not merely to hide its animation.

## Backend Contract

REST commands return RFC 7807 Problem Details. A safe validation response can take this form:

```json
{
  "type": "https://httpstatuses.io/422",
  "title": "Validation Error",
  "status": 422,
  "detail": "The operation could not be completed because one or more fields are invalid.",
  "userMessage": "The module could not be registered because the Code field has an invalid format. Use letters, numbers, and underscores only, for example REPORTS_01. Entered value: DDDD-!.",
  "errorCode": "validation.invalid_format",
  "messageKey": "system_suite.module.code_invalid_format",
  "messageParameters": { "invalidValue": "DDDD-!" },
  "errorId": "<server-generated-guid>",
  "traceId": "<distributed-trace-id>"
}
```

Contract requirements:

- `detail` is safe but is not automatically displayable.
- `userMessage` is displayable only because the API boundary explicitly publishes it as safe.
- `errorCode`, `messageKey`, and parameters are the preferred multilingual evolution.
- `errorId` is generated once per failed request and appears in the payload, `X-Error-Id`, and Serilog event used by support.
- `traceId` remains available for distributed diagnostics and is distinct from `errorId`.
- Technical diagnostics are logged in the backend and never serialized for display.

GraphQL queries follow the same safety rule: unhandled resolver errors return safe client content and retain diagnostics in logs.

## Current UMS Implementation

UMS currently supports:

- `userMessage` for approved REST validation and business conflict feedback.
- Server-generated `errorId` in REST error payloads and `X-Error-Id`, prioritized by the web client for support.
- Safe localized GraphQL client error messages with `errorId` recorded in the corresponding server log.
- Removal of a notification from active state when the user presses its close button.

The target enhancement is to introduce stable `errorCode` or `messageKey` values and client-side localization rather than serving final language text from the API.

## Verification

| Layer | Required verification |
|---|---|
| API | Expected validation exposes safe actionable feedback, GUID `errorId`, matching `X-Error-Id`, and `traceId` |
| API | Unexpected errors expose no stack trace, exception type, namespace, SQL, secret, or PII |
| Web extraction | Only approved messages or local fallback are displayed |
| Mutation wrapper | Localized support guidance with `errorId` is included when available |
| Toast queue | Manual close removes the notification from active state |

## Related Documents

- [ADR-0066: Actionable User Error Contract and Correlated Diagnostics](../adrs/0066-actionable-user-error-contract.md)
- [ADR-0053: OpenTelemetry Observability](../adrs/0053-opentelemetry-observability.md)
- [ADR-0062: PII-Safe Serilog Configuration](../adrs/0062-pii-safe-serilog-configuration.md)

---

**[Back to Blueprints Index](./index.md)** | **[Version en Espanol](../blueprints-es/notification-feedback-architecture.md)**
