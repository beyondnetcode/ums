# Matriz de Compatibilidad

> **Idioma:** [English](../../sdk/contracts/compatibility-matrix.md) | Español

Esta matriz lista cada versión publicada de paquete SDK contra cada versión emitida de schema `AuthorizationGraph`. Es la **fuente de verdad** para preguntas de soporte e integración.

CI actualiza este archivo automáticamente en cada release de SDK.

---

## 1. Leyenda

| Marcador | Significado |
|---|---|
| ✅ | **Accept** — match exacto o compatible. Sin warnings. |
| ⚠️ | **Accept con warning** — mismatch MINOR en cualquier dirección. Funcional pero loguea warnings estructurados. |
| ❌ | **Reject** — mismatch MAJOR. El SDK retorna `AUTH_205`. |
| — | No aplica (la versión del SDK no existía cuando se emitió la versión del schema, o viceversa). |

---

## 2. Versiones de Schema

| Versión | Lanzada | Estado | Notas |
|---|---|---|---|
| `1.0.0` | TBD (release inicial) | Activa | Baseline canónica; documentado en [auth-graph.md §8](../../domain-es/identity/auth-graph.md#8-ejemplo-json-de-respuesta) |

Futuras versiones se agregarán aquí.

---

## 3. Compatibilidad SDK .NET

| Schema → / SDK .NET ↓ | 1.0.0 |
|---|---|
| `Ums.Sdk.Contracts 1.0.x` (compat `>=1.0.0 <2.0.0`) | ✅ |
| `Ums.Sdk.Authorization 1.0.x` (compat `>=1.0.0 <2.0.0`) | ✅ |
| `Ums.Sdk.Authorization.Aop 1.0.x` (compat `>=1.0.0 <2.0.0`) | ✅ |
| `Ums.Sdk.Authorization.Testing 1.0.x` (compat `>=1.0.0 <2.0.0`) | ✅ |

## 4. Compatibilidad SDK TypeScript

| Schema → / SDK TS ↓ | 1.0.0 |
|---|---|
| `@ums/sdk-contracts 1.0.x` (compat `>=1.0.0 <2.0.0`) | ✅ |
| `@ums/sdk-authorization 1.0.x` (compat `>=1.0.0 <2.0.0`) | ✅ |
| `@ums/sdk-testing 1.0.x` (compat `>=1.0.0 <2.0.0`) | ✅ |

## 5. Compatibilidad SDK NestJS

| Schema → / SDK NestJS ↓ | 1.0.0 |
|---|---|
| `@ums/sdk-nestjs 1.0.x` (compat `>=1.0.0 <2.0.0`) | ✅ |

---

## 6. Compatibilidad del Servidor UMS

| Versión del servidor | Emite schema | Notas |
|---|---|---|
| Release inicial UMS API (Fase B en progreso) | `1.0.0` | Primer release con campo `schemaVersion`. Versiones anteriores del servidor no emiten versión de schema y los SDKs las rechazan. |

---

## 7. Cómo leer esta matriz

- **Eligiendo una versión de SDK para un cliente nuevo**: identifica la versión de schema que emite tu servidor UMS, luego elige el SDK más reciente en la columna marcada ✅.
- **Diagnosticando un error de runtime**: si tu SDK reporta `AUTH_205`, encuentra la fila para tu SDK y la columna para el schema que los logs del servidor están reportando — la matriz te dice si actualizar el SDK, el servidor, o abrir un ticket de soporte.
- **Planeando un upgrade**: un ⚠️ en la matriz significa que puedes correr esa combinación en producción pero deberías planear el upgrade. ❌ significa que no deberías desplegar esa combinación.

---

## 8. Referencias

- [Política de Versionado](./versioning.md)
- [ADR-0074: Política de Versionado del Schema](../../architecture/adrs/0074-auth-graph-schema-versioning.es.md)
- [Schema Overview](./schema-overview.md)
