# Estándar de Evidencia de Cierre de Gaps — Evolith UMS

> **Navegación Bilingüe:** [English](./ums-gap-closure-evidence-standard.md)

**Estado:** Activo
**Owner:** Evolith UMS Team
**Origen de Diseño:** réplica del [Gap Closure Evidence Standard](https://github.com/beyondnetcode/evolith_tracker/blob/main/docs/audit/tracker-gap-closure-evidence-standard.es.md) de Evolith Tracker, que a su vez replica el estándar de Evolith Core.
**Registro Machine-Readable:** [`ums-gap-closure-evidence.json`](./ums-gap-closure-evidence.json)

## 1. Propósito

Este estándar convierte un gap completado en una afirmación de gobernanza respaldada por evidencia. Una fila consistente en el board es necesaria pero no suficiente: el cierre debe ser reproducible desde la historia del repositorio y artefactos resolubles. Gobierna el [Board de Tracking de Gaps](./ums-gap-tracking.md) y el [Catálogo de Referencia de Gaps](./ums-gap-reference-catalog.md).

## 2. Registro de Cierre Requerido

Todo gap marcado `DONE` debería tener exactamente una entrada en el registro canónico con:

| Campo | Requisito |
|---|---|
| `id` | Identificador existente presente en board y catálogo (`DS-*`) |
| `closedAt` | Fecha ISO que no esté en el futuro |
| `closureCommit` | Commit de Git existente que contiene o establece el cierre |
| `evidence` | Uno o más archivos relativos al repo que demuestran el resultado |
| `validationCommands` | Uno o más comandos reproducibles usados para validar el resultado |
| `dependencyDisposition` | `none`, `satisfied`, `accepted-scope`, o `deferred` |
| `dependencyRationale` | Requerido siempre que la disposición no sea `none` |

`IN-PROGRESS` se reserva para gaps atendidos a nivel de spec/gobernanza cuya implementación de código/infraestructura sigue pendiente (refleja la distinción de Core entre una decisión documentada y una capacidad entregada).

## 3. Enforcement Semántico (previsto)

Un check `validate-tracking` (a cablear bajo `.harness/scripts/` o la CI de UMS) debería fallar cuando:

1. un gap completado no tiene registro de cierre;
2. un registro de cierre apunta a un gap, commit o archivo de evidencia inexistente;
3. una sección de catálogo completada contiene un criterio de aceptación `- [ ]` sin marcar;
4. los metadatos de cierre están incompletos, duplicados, fechados a futuro, o usan una disposición de dependencia no soportada;
5. el Board y el Catálogo difieren en el conjunto de IDs o el estado.

Los gaps pendientes, en progreso y diferidos no deben tener registros de cierre activos. La justificación histórica permanece en el catálogo.

## 4. Flujo de Cierre

1. Completar y validar el trabajo del alcance.
2. Commitear la evidencia de implementación o documentación.
3. Agregar el registro de cierre usando ese commit real.
4. Resolver cada checkbox de criterio de aceptación en el catálogo.
5. Cambiar el estado del board a `DONE`.
6. Correr validación de tracking, documentación y bilingüe.

Ningún commit placeholder, evidencia especulativa o checkbox eximido puede satisfacer el cierre.

## 5. Disposiciones de Dependencia

Los ítems `DS-*` llevan dependencias entre repositorios (notablemente sobre MMS como writer-of-record de tenants). Cuando un gap cierra con una dependencia sin resolver o de propiedad externa, se registra la disposición (`satisfied`, `accepted-scope`, o `deferred`) y una justificación — por ejemplo, cerrar `DS-12` (consumir el paquete de contratos compartido) depende de que MMS publique `Evolith.Messaging.Contracts`, y `DS-01` (migración de ownership de tenants) depende de MMS como autoridad a lo largo de la escalera M0–M4.

---
[Volver al Board de Tracking de Gaps](./ums-gap-tracking.md)
