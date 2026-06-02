# ADR-0076: UTC Date Storage, Browser Timezone Detection, and Language Resolution

**Status:** Accepted  
**Date:** 2026-06-02  
**Context:** Evolith Platform — Root Standard (applies to all child systems: UMS, and future products)  
**Supersedes:** None  
**Related:** ADR-0052 (Immutable Audit Trail), ADR-0056 (Clean Architecture Frontend)

---

## Context

Distributed systems that span multiple countries and time zones must handle dates, times, and locale preferences consistently across all layers. Without an explicit standard, individual teams make incompatible choices: some store local times, some use browser-default formatting, some hardcode language identifiers. When data crosses system boundaries or is shown to users in different time zones, these inconsistencies cause display errors, audit discrepancies, and compliance failures.

Three independent concerns must be addressed together because they interact at session initialization:

1. **Date and time storage**: Should the database store UTC or local time?
2. **Timezone detection**: How does the system know where the user is, and which timezone to apply when displaying timestamps?
3. **Language resolution**: Which language should the UI use, and what is the priority chain when multiple sources provide a preference?

All Evolith child systems (current: UMS; future: billing, logistics, port operations, ERP integrations) must follow the same standard so that cross-system audit trails, delegations, and events are interpretable without time-zone ambiguity.

---

## Decisions

### D1 — All dates are stored and transmitted as UTC

**Rule:** Every date or timestamp stored in any database table, domain event, outbox message, or API response body **must** be UTC.

- **Backend (C#):** Use `DateTime.UtcNow` or `DateTimeOffset.UtcNow`. Never use `DateTime.Now`.
- **Column naming convention:** Suffix UTC columns with `UtcNow` at the domain level and `Utc` at the persistence level (e.g., `CreatedAtUtc`, `DeletedAtUtc`).
- **EF Core:** Register a `ValueConverter<DateTime, DateTime>` on all `DateTime` properties that forces `DateTimeKind.Utc` on read, preventing silent local-time storage on SQLite (which stores no timezone information).
- **API responses:** ISO 8601 strings must include the `Z` suffix (e.g., `"2026-06-02T15:30:00Z"`) to make UTC explicit to consumers.
- **Frontend:** Parse ISO strings with `new Date(isoString)` — JavaScript always interprets `Z`-suffixed strings as UTC internally. Never manually apply offsets to UTC values before storing.

**Rationale:** UTC is the only unambiguous anchor for distributed systems. Local times introduce DST gaps, wall-clock ambiguity, and cross-datacenter inconsistency. Storing UTC and converting at display time is the industry standard (IANA, RFC 3339, ISO 8601).

---

### D2 — Browser timezone is detected at session start and stored in the session

**Rule:** On login, the frontend detects the browser's IANA timezone using `Intl.DateTimeFormat().resolvedOptions().timeZone` and stores it in the authenticated session state. It is sent to the backend as the `X-Timezone` request header on every API call.

- **Detection:** `Intl.DateTimeFormat().resolvedOptions().timeZone` returns an IANA identifier (e.g., `"America/Lima"`, `"Europe/Madrid"`). This is supported in all modern browsers.
- **Fallback chain:**
  1. Browser-detected IANA timezone (primary — reflects actual user location)
  2. Tenant parameter `UI_TIMEZONE_DEFAULT` (configured by tenant admin, e.g., `"America/Lima"`)
  3. Hardcoded system default `"UTC"` (last resort only)
- **Precedence:** Browser detection always wins over tenant configuration. The tenant parameter serves as the default only when browser detection fails or the result is `undefined`.
- **Backend:** The `X-Timezone` header is validated against the IANA timezone database and stored in the request context. It is used for any server-side date formatting (e.g., report generation, email timestamps).
- **Display:** All date/time values shown to users are converted from their UTC storage value to the session timezone using `Intl.DateTimeFormat` with an explicit `timeZone` option.

**Rationale:** The browser knows the user's actual timezone without requiring manual configuration. Defaulting to the tenant timezone handles edge cases (kiosk terminals, SSO sessions from foreign browsers) while still providing a sensible default for most users in the same region as the tenant.

---

### D3 — Language resolution follows a strict priority chain

**Rule:** The UI language is resolved at session initialization in this order:

| Priority | Source | Mechanism |
|---|---|---|
| 1 (highest) | Browser `Accept-Language` HTTP header | Read by backend `CultureMiddleware`, validated against supported locales |
| 2 | Tenant parameter `UI_LANGUAGE_DEFAULT` | Read from in-memory configuration cache at login |
| 3 (lowest) | Platform hardcoded default `"es"` | Final fallback |

- **Backend `CultureMiddleware`** reads `Accept-Language`, extracts the primary language code (first 2 characters, lowercased: `"es-PE"` → `"es"`), validates it against the supported locale list, and sets `CultureInfo.CurrentCulture` for the request.
- **Login response:** The resolved language is included in `LoginSuccessResponse.Language` AND in `SessionParameters.DefaultLanguage`.
- **Frontend i18n store:** On successful login, the frontend reads `sessionParameters.defaultLanguage` from the response and calls `useI18nStore.setLanguage(lang)` to initialize the active UI language for the entire session.
- **Supported languages:** Systems declare their supported locale list explicitly. Unsupported language codes fall back to the next priority level. For UMS: `["es", "en"]`.
- **Date formatting:** All `formatDate`, `formatDateTime`, and `formatRelativeTime` calls **must** receive the active locale from the i18n store. Functions must not use a hardcoded default locale.

**Rationale:** Browser preference reflects what the user has configured at the OS level. Respecting it requires no manual configuration and covers the majority of cases. The tenant admin can control the fallback for environments where browser settings are not representative (shared kiosks, managed devices).

---

## Consequences

### Positive
- UTC storage eliminates all DST, clock-change, and cross-datacenter ambiguity.
- Sessions always display dates in the user's actual timezone without configuration.
- Language initialization is automatic; users see the system in their preferred language on first login.
- Cross-system audit trails (UMS → billing → logistics) share the same temporal reference frame.

### Negative / Trade-offs
- EF Core UTC converters add slight complexity to DbContext configuration.
- Browser timezone detection requires `Intl` API (available in all modern browsers; not a real constraint).
- The `X-Timezone` header adds a small overhead to every request (single string, negligible).
- Timezone display conversion (UTC → local) must be applied consistently; missing it in any component is a silent bug. Code review gates should check for raw UTC date display.

---

## Implementation Checklist (per child system)

- [ ] All `DateTime` properties in domain entities use `DateTime.UtcNow`.
- [ ] EF Core registers UTC `ValueConverter` for `DateTime` properties.
- [ ] API responses use ISO 8601 with `Z` suffix.
- [ ] `CultureMiddleware` reads `Accept-Language` and validates against supported locales.
- [ ] Login response includes `Language` (resolved) and `SessionParameters.DefaultTimezone`.
- [ ] Frontend detects `Intl.DateTimeFormat().resolvedOptions().timeZone` at login.
- [ ] Frontend stores timezone in session and sends `X-Timezone` header.
- [ ] Frontend i18n store is initialized from `sessionParameters.defaultLanguage` at login.
- [ ] All `formatDate`/`formatDateTime` calls pass the active locale and session timezone.

---

## References

- [IANA Time Zone Database](https://www.iana.org/time-zones)
- [RFC 3339 — Date and Time on the Internet](https://tools.ietf.org/html/rfc3339)
- [ISO 8601 — Date and time format](https://www.iso.org/iso-8601-date-and-time-format.html)
- [MDN: Intl.DateTimeFormat](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/DateTimeFormat)
- UMS ADR-0052: Immutable Audit Trail Enforcement
- UMS ADR-0056: Clean Architecture Frontend
