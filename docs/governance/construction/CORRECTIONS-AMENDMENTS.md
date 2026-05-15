# Corrections & Amendments — Technical Stories

**Date:** 2026-05-14 (Revision 1)
**Status:** Corrections applied to TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md + FS-TO-TS-MAPPING.md

---

## Summary

During construction planning validation, **2 critical errors** were identified in initial technical story estimations. Both have been corrected below.

---

## Error #1: RLS Implementation Model (CRITICAL)

### What Was Written (INCORRECT)

**TS-1.2:** "SQL Server Infrastructure — Tenant & Identity Tables + RLS"
- Description emphasized SQL Server RLS as **primary enforcement mechanism**
- Mentioned "Layer 1 RLS: EF Core + SESSION_CONTEXT" and "Layer 2 RLS: SQL Server inline predicates"

**TS-1.3:** "EF Core DbConnectionInterceptor — SESSION_CONTEXT Setup"
- Described implementing SESSION_CONTEXT in SQL Server as the isolation mechanism

### What's Correct (per stack.md Section 6.1)

**Multi-tenancy Strategy:** "Logical Isolation (Application-Level) + Native Row-Level Security (RLS) Hardening"

**Phase 1 (PRIMARY):**
- Enforced at the **Application Layer** using EF Core Global Query Filters
- Requires mandatory `TenantId` denormalization in all functional entities
- O(1) filtering via WHERE clause (no database RLS required)

**Phase 2 (Optional Hardening):**
- Infrastructure-level protection using SQL Server `SESSION_CONTEXT` + Security Policies (iTVF)
- Prevents accidental data leaks if application layer fails
- **NOT required for basic functionality**

### Corrections Applied

**TS-1.2 (Updated):**
- **New Title:** "SQL Server Infrastructure — Tenant & Identity Tables (Schema & Indices)"
- **New Size:** 8 pts (reduced from 13 pts, schema only)
- **New Description:** Focus on composite keys (id, root_tenant_id) and indices, not RLS predicates
- **Added Note:** " RLS enforcement is PRIMARY at Application Layer (TS-1.3, EF Core filters); SQL Server RLS is optional hardening (Phase 2)"

**TS-1.3 (Updated):**
- **New Title:** "EF Core Global Query Filters — Application-Level Tenant Isolation (PRIMARY)"
- **New Size:** 8 pts (unchanged)
- **New Description:** Implement ICurrentTenantResolver + ModelBuilder global filters
- **Key AC:** "Filter applied automatically: no developer needs to add WHERE clauses"
- **Added:** Phase 2 note about optional SQL Server RLS

### Impact on Stories

| Story | Old Size | New Size | Reason |
|-------|----------|----------|--------|
| TS-1.2 | 13 pts | 8 pts | Schema only (RLS removed) |
| TS-1.3 | 5 pts | 8 pts | Primary isolation mechanism |
| **EP-01 Total** | 55 pts | **50 pts** | -5 pts net (RLS complexity reduced) |

### Reference Documentation

- **Source:** `/architecture/blueprints/stack.md` Section 6 (Multi-tenancy Strategy)
- **Quote:** "Phase 1 (Primary): Enforced at the Application Layer using EF Core Global Query Filters... Phase 2 (Hardening): Optional infrastructure-level protection using SQL Server SESSION_CONTEXT..."
- **ADR:** ADR-0048 (Closure Table), ADR-0049 (Partitioning) — both assume application-layer filtering

### Validation

- **RLS Tests (TS-1.6):** Now test EF Core filter isolation, not SQL Server RLS
- **Integration Tests:** Verify composite key enforcement + filter correctness
- **Architecture:** Aligns with "Hexagonal Architecture" mandate (domain layer has no DB dependencies)

---

## Error #2: Frontend Stack (CRITICAL)

### What Was Written (INCORRECT)

**TS-5.1:** "Hosted Login Page — Razor Pages Implementation"
- Described building Razor page (`Pages/Auth/Login.cshtml`)
- Mentioned server-side rendering with C# templating

**TS-5.2:** "Diagnostics Dashboard — Admin Interface"
- Described Razor page (`Pages/Admin/Diagnostics.cshtml`)
- Mentioned server-side HTML generation

**Profile 6:** "Frontend Engineer (Razor Pages, HTML/CSS/JS)"
- Listed ASP.NET Razor, Bootstrap, vanilla JavaScript as tech stack

### What's Correct (per dotnet-migration-and-tech-stack-plan.md)

**Frontend Stack:**
```
Frontend: React (v18+, Latest Stable) / Vite / Zustand / TanStack Query
```

**Architecture:**
```
Web[apps/web - React Portal] --> Pres[Ums.Presentation - Web API]
```

**Details from dotnet-migration-and-tech-stack-plan.md Section 2:**
- Project: `apps/web` (React Portal)
- Technology: React 18+, Vite build, Zustand state, TanStack Query for data fetching
- Integration: OpenAPI-generated client SDK for type-safe API calls

### Corrections Applied

**TS-5.1 (Updated):**
- **New Title:** "Hosted Login Page — React (Vite) Implementation"
- **New Skills:** Frontend (React/TypeScript), API Integration
- **New Tech Stack:** React hooks, TypeScript, Zustand, Zod validation, Tailwind CSS
- **Integration:** Calls POST /api/v1/auth/login (TS-1.5) with proper error handling

**TS-5.2 (Updated):**
- **New Title:** "Diagnostics Dashboard — React Admin Interface"
- **New Skills:** Frontend (React/TypeScript), Data Visualization
- **New Tech:** React, Recharts (charts), TanStack Query (polling), Tailwind CSS
- **Integration:** GET /api/v1/health, /api/v1/audit/logs, metrics endpoints

**Profile 6 (Updated):**
- **New Title:** "Frontend Engineer (React/TypeScript, Vite, TanStack)"
- **New Skills:** React 18+, TypeScript strict, Vite, Zustand, TanStack Query, Tailwind, Recharts
- **Removed:** Razor, Bootstrap, C# templating
- **Reasoning:** Monorepo strategy (Nx) allows shared types between React frontend and .NET backend

### Impact on Stories

| Story | Old Tech | New Tech | Skills Change |
|-------|----------|----------|----------------|
| TS-5.1 | Razor Pages | React/Vite | Backend → Frontend (React specialist) |
| TS-5.2 | Razor Pages | React/Vite | Backend → Frontend (React specialist) |
| Profile 6 | ASP.NET | React | Different skill profile entirely |

### Reference Documentation

- **Source:** `/architecture/blueprints/dotnet-migration-and-tech-stack-plan.md` Section 1 & 2
- **Quote:** "...while fully preserving the **React (Vite)** frontend application"
- **Tech Stack:** "Frontend: React (v18+, Latest Stable) / Vite / Zustand / TanStack Query"
- **Integration:** "Contract-First Doctrine: Integration with the React portal will utilize generated OpenAPI specifications"

### Validation

- **Monorepo:** .NET backend (Ums.sln) + React frontend (apps/web) share single Nx monorepo
- **Type Safety:** OpenAPI codegen ensures frontend & backend contract alignment
- **Dev Experience:** Shared TypeScript types between frontend (Zustand store) and backend DTOs
- **Build:** Vite handles React build, .NET handles backend, CI/CD handles both independently

### Cross-Document Updates

All references updated in:
- TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md (TS-5.1, TS-5.2, Profile 6)
- FS-TO-TS-MAPPING.md (FS-08)
- construction/README.md (warning added)

---

## Effort Impact Summary

| Aspect | Before | After | Change |
|--------|--------|-------|--------|
| **MVP Total Points** | 253 pts | **248 pts** | -5 pts (RLS moved to app layer) |
| **EP-01 Points** | 55 pts | **50 pts** | -5 pts (RLS complexity down) |
| **Frontend Profile** | 0.5 Backend (Razor) | 0.5 Frontend (React) | Skill type changed, effort same |
| **MVP Timeline** | 6-7 weeks | **6-7 weeks** | No change (points negligible) |
| **Team Composition** | Needs Razor expert | Needs React expert | Hiring profile changed |

---

## Action Items

**Engineering Lead:**
- [ ] Update sprint backlog: TS-1.2 now 8 pts (was 13), TS-1.3 now 8 pts (was 5)
- [ ] Adjust MVP velocity targets (248 pts / 6.25 FTE instead of 253 pts)
- [ ] Review RLS test strategy: focus on EF Core filters, optional SQL Server RLS

**Hiring:**
- [ ] Update job description for Profile 6: React specialist (NOT ASP.NET Razor)
- [ ] Seek: React 18+, TypeScript, Vite, Zustand, TanStack Query experience
- [ ] Remove: ASP.NET Razor, C# templating requirements for frontend

**QA:**
- [ ] Update TS-1.6 test scenarios: EF Core filter isolation (not SQL Server RLS)
- [ ] Update TS-5.5 test plan: Playwright for React (not Razor Page automation)
- [ ] Validate: Frontend tests use React Testing Library (not server-side rendering tests)

**Documentation:**
- [ ] Update SERVICE-IMPLEMENTATION-PLAN.md if it mentions Razor Pages
- [ ] Update TE-03-rls-sql-server-implementation.md: clarify Phase 1 vs Phase 2
- [ ] Cross-reference stack.md Section 6 in all RLS-related stories

---

## Validation Checklist

- [x] RLS model corrected (application-first, SQL Server optional)
- [x] Frontend stack corrected (React, not Razor)
- [x] All TS descriptions updated with correct tech
- [x] Profile 6 updated with React skills
- [x] FS-TO-TS-MAPPING.md updated for FS-08
- [x] Effort re-estimated (248 pts, minimal change)
- [x] Critical paths re-validated (no change)
- [x] Documentation references updated

---

## Questions?

See `/construction/README.md` FAQ section or contact Principal Architect for RLS architecture clarification.

---

**Approved by:** Principal Architect
**Date:** 2026-05-14
**Status:** **CORRECTIONS APPLIED & VALIDATED**

*All technical stories in TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md reflect these corrections.*
