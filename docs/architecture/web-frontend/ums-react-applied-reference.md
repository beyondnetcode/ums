# UMS React Applied Reference

> Language: [English](./ums-react-applied-reference.md) | [Espanol](./ums-react-applied-reference.es.md)

## 1. Purpose

This document maps the UMS React Web implementation to the Evolith Web Frontend Standard - React. It is evidence for the current product implementation, not a universal standard.

Reusable rules belong in Evolith. UMS-specific details remain here unless promoted through an Evolith ADR, governance standard, or canonical pattern.

## 2. Source scope

The applied reference covers:

```text
src/apps/ums.web-app
```

Observed implementation profile:

| Concern | UMS implementation |
|---|---|
| Runtime | React 18.3.1 |
| Build | Vite 5.4.10 |
| Language | TypeScript |
| Routing | React Router DOM 6.30.3 |
| Server state | TanStack React Query 5.100.9 |
| Client state | Zustand 5.0.13 |
| Runtime validation | Zod 4.4.3 |
| HTTP | Axios 1.16.0 |
| Styling | TailwindCSS 3.4.19 plus CSS variables |
| Mocking | MSW 2.14.6 |
| Unit/component tests | Vitest and Testing Library |
| E2E | Playwright |

## 3. Evolith to UMS mapping

| Evolith topic | UMS source evidence | Classification |
|---|---|---|
| App bootstrap | `src/apps/ums.web-app/src/main.tsx` centralizes React StrictMode, React Query provider, locale synchronization, request context configuration, and optional MSW startup. | Applied evidence |
| Server-state provider | `main.tsx` creates one `QueryClient` with shared default query options. | Candidate Evolith profile; exact defaults remain local |
| Routing | `src/apps/ums.web-app/src/App.tsx` uses `BrowserRouter`, route-level lazy imports, `Suspense`, `RouteLoader`, and fallback redirects. | Applied evidence |
| Error boundary | `App.tsx` wraps the routed application with `AppErrorBoundary`. | Reusable pattern |
| Layout shell | `src/apps/ums.web-app/src/presentation/shared/layouts/MainLayout.tsx` composes top app bar, nav rail, content region, notification behavior, and idle timeout integration. | Pattern reusable; concrete components and timeout are local |
| Material Design 3 tokens | `src/apps/ums.web-app/src/index.css` defines light/dark Material Design 3 role tokens as CSS variables. | Token governance candidate; concrete values are local |
| Tailwind token bridge | `src/apps/ums.web-app/tailwind.config.js` exposes `m3.*` semantic colors through `hsl(var(--token))`. | Boilerplate candidate |
| HTTP boundary | `src/apps/ums.web-app/src/infrastructure/http/httpClient.ts` centralizes Axios setup, request headers, CSRF, and normalized errors. | Reusable boundary pattern; headers are local |
| Request context | `src/apps/ums.web-app/src/infrastructure/http/request-context.ts` centralizes user and language request context and environment-based API base URL. | Reusable pattern; tenant placeholder is local technical debt |
| Testing profile | `src/apps/ums.web-app/package.json` declares Vitest, Testing Library, MSW, and Playwright. | Candidate quality gate profile |

## 4. Items that should remain UMS-local

| Item | Reason |
|---|---|
| Product routes such as `/tenants`, `/users`, `/delegations`, `/profiles`, and `/feature-flags` | UMS domain navigation, not reusable boilerplate. |
| Concrete dashboard screen names | UMS bounded-context implementation details. |
| `X-User-Id`, `X-Language`, and `X-Tenant-Id` header names | API-specific contract. Evolith should define the pattern, not the names. |
| `DEV_TENANT_ID` | Development placeholder and not suitable as an enterprise standard. |
| Indigo/Violet token values | Product branding choice. Evolith owns token roles, not these values. |
| Idle timeout value of 15 minutes | UMS security/session policy. Evolith can require configurability. |
| TopAppBar and NavRail component names | UMS component implementation. Evolith owns the application shell pattern. |

## 5. Items to promote to Evolith

| Candidate | Promotion target |
|---|---|
| React application folder structure with `domain`, `application`, `infrastructure`, and `presentation` | Evolith React boilerplate standard |
| Root provider composition pattern | Evolith React bootstrap guidance |
| Route-level lazy screen loading | Evolith React routing standard |
| Material Design 3 token roles via CSS variables | Evolith UI design-system standard |
| Tailwind semantic token mapping | Evolith React boilerplate standard |
| Centralized HTTP/request context boundary | Evolith data access standard |
| Testing stack profile with Vitest, Testing Library, MSW, and Playwright | Evolith frontend quality gates |

## 6. Items requiring ADR or formal standard promotion

| Item | Required action |
|---|---|
| React + Vite as default enterprise frontend baseline | Evolith ADR |
| TanStack React Query as server-state baseline | Evolith ADR or engineering standard |
| Zustand as lightweight client-state profile | Evolith ADR or optional profile |
| Material Design 3 token-based UI standard | Evolith UI/design-system standard and possible ADR |
| Zod as runtime validation profile | Evolith ADR or data-contract standard |
| MSW plus Playwright as standard testing profile | Evolith testing standard or ADR |

## 7. Current gaps and follow-up actions

| Gap | Recommendation |
|---|---|
| Development tenant identifier is hardcoded in request context | Replace with authenticated tenant resolution or environment-safe development adapter. |
| UMS applied reference depends on source conventions not yet fully enforced by automation | Extend documentation audit with frontend structure checks if required. |
| MD3 token values are product-local but not yet linked to an Evolith design-system ADR | Promote token governance to Evolith before making it mandatory. |
| Protected route pattern is not documented in this applied reference | Add when authorization guards are formalized in source. |

## 8. Validation checklist

Before changing UMS React Web architecture, validate:

- The change is classified as Evolith standard, UMS local decision, or promotion candidate.
- Shared patterns remain independent from UMS product language.
- Product-specific route names, headers, tenant IDs, and branding stay in UMS.
- Any reusable rule is proposed in Evolith first.
- English and Spanish documentation are updated together.
- Markdown remains UTF-8 clean and free from decorative icons.

---
[Back to Web Frontend Portal](./README.md)
