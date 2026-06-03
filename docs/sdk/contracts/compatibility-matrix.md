# Compatibility Matrix

> **Language:**English | [Español](../../sdk-es/contracts/compatibility-matrix.md)

This matrix lists every published SDK package version against every emitted `AuthorizationGraph` schema version. It is the**source of truth**for support and integration questions.

CI updates this file automatically on every SDK release.

---

## 1. Legend

| Marker | Meaning |
|---|---|
| | **Accept** — exact-match or compatible. No warnings. |
| | **Accept with warning** — MINOR mismatch in either direction. Functional but logs structured warnings. |
| | **Reject** — MAJOR mismatch. SDK returns `AUTH_205`. |
| — | Not applicable (SDK version did not exist when schema version was emitted, or vice versa). | ---

## 2. Schema Versions

| Version | Released | Status | Notes |
|---|---|---|---|
| `1.0.0` | TBD (initial release) | Active | Canonical baseline; documented in [auth-graph.md §8](../../domain/identity/auth-graph.md#8-ejemplo-json-de-respuesta) | Future versions will be appended here.

---

## 3. .NET SDK Compatibility

| Schema → / .NET SDK ↓ | 1.0.0 |
|---|---|
| `Ums.Sdk.Contracts 1.0.x` (compat `>=1.0.0 <2.0.0`) | |
| `Ums.Sdk.Authorization 1.0.x` (compat `>=1.0.0 <2.0.0`) | |
| `Ums.Sdk.Authorization.Aop 1.0.x` (compat `>=1.0.0 <2.0.0`) | |
| `Ums.Sdk.Authorization.Testing 1.0.x` (compat `>=1.0.0 <2.0.0`) | | ## 4. TypeScript SDK Compatibility

| Schema → / TS SDK ↓ | 1.0.0 |
|---|---|
| `@ums/sdk-contracts 1.0.x` (compat `>=1.0.0 <2.0.0`) | |
| `@ums/sdk-authorization 1.0.x` (compat `>=1.0.0 <2.0.0`) | |
| `@ums/sdk-testing 1.0.x` (compat `>=1.0.0 <2.0.0`) | | ## 5. NestJS SDK Compatibility

| Schema → / NestJS SDK ↓ | 1.0.0 |
|---|---|
| `@ums/sdk-nestjs 1.0.x` (compat `>=1.0.0 <2.0.0`) | | ---

## 6. UMS Server Compatibility

| Server version | Emits schema | Notes |
|---|---|---|
| UMS API initial release (Phase B in progress) | `1.0.0` | First release with `schemaVersion` field. Earlier server versions emit no schema version and SDKs reject them. | ---

## 7. How to read this matrix

- **Picking an SDK version for a new client**: identify the schema version your UMS server emits, then choose the latest SDK in the column marked .
- **Diagnosing a runtime error**: if your SDK reports `AUTH_205`, find the row for your SDK and the column for the schema your server logs are reporting — the matrix tells you whether to upgrade the SDK, the server, or open a support ticket.
- **Planning an upgrade**: a in the matrix means you can run that combination in production but should plan the upgrade. means you should not deploy that combination.

---

## 8. References

- [Versioning Policy](./versioning.md)
- [ADR-0074: Schema Versioning Policy](../../architecture/adrs/0074-auth-graph-schema-versioning.md)
- [Schema Overview](./schema-overview.md)
