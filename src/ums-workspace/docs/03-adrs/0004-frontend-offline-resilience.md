# ADR 0004: Frontend State Management and React Query Offline Architecture

## Status
Accepted

## Date
2026-05-08

## Context
The UMS React web client needs to operate reliably, even when the backend API or PostgreSQL database are offline or during network disruptions. Traditional state management patterns often couple UI views tightly with live network endpoints, causing the dashboard to freeze or crash when the backend is unreachable.

## Decision
We decided to implement a highly resilient, offline-first state and data architecture:
1. Use **Zustand** as the lightweight, high-performance global state manager for client-only state (e.g., active user sessions, sidebar toggles, and modal states).
2. Use **TanStack React Query** (v5) to manage server state (fetching, caching, and synchronizing database queries).
3. Implement a **`localStorage` backup fallback** inside React Query hooks: if the API request fails due to offline status or network errors, the client automatically falls back to cached localStorage snapshots, rendering a simulator alert instead of breaking the dashboard.

## Consequences

### Positive (Pros)
* **High Availability**: The React client remains fully interactive even if the backend NestJS service is down.
* **Excellent UX**: Users are presented with simulated alerts showing active state, rather than blank pages or endless spinners.
* **Separation of Concerns**: Global client state (Zustand) is decoupled from asynchronous server cache (React Query).

### Negative (Cons)
* Requires managing manual synchronization between the live PostgreSQL DB and the browser's localStorage cache.
* Adds slight complexity when updating data, requiring optimistic updates in the cache.
