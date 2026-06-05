# FS-35: Salud Continua de Acceso y Recomendaciones

> **Estado:** Pendiente de implementacion

## 1. Proposito de Negocio

Los equipos de seguridad y gobierno necesitan una vista continua de la calidad del acceso, no solo aprobaciones puntuales. UMS debe calcular la salud del acceso, resaltar accesos obsoletos o excesivos y recomendar la siguiente mejor accion de gobierno.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| Administrador de Seguridad | Revisa las senales de salud y los umbrales. |
| Administrador IGA | Usa las recomendaciones para lanzar acciones de limpieza o revision. |
| Auditor | Revisa la tendencia de salud y el historial de remediacion. |

## 3. Precondiciones de Negocio

- UMS tiene suficientes datos de acceso, auditoria y revision para evaluar senales de salud.
- El tenant tiene una politica que define que es saludable, riesgoso o obsoleto.
- El actor puede acceder al area de diagnostico de gobierno.

## 4. Flujo Funcional Principal

1. El actor abre el panel de salud de acceso.
2. El sistema calcula un puntaje de salud usando senales como acceso obsoleto, acceso no usado, revisiones vencidas y roles con privilegios excesivos.
3. El actor inspecciona las razones detras del puntaje.
4. El sistema recomienda una siguiente accion, como abrir una campana de revision, recortar un paquete o expirar una concesion privilegiada.
5. El actor puede usar la recomendacion para lanzar el siguiente paso de gobierno.

## 5. Flujos Alternativos y Excepciones

### A. Datos Insuficientes

Si el sistema no tiene suficientes senales para calcular un puntaje confiable, muestra esa limitacion de forma clara.

### B. Umbral de Riesgo Critico

Si el puntaje cruza un umbral critico, el sistema resalta las cuentas o paquetes que requieren revision inmediata.

### C. No Se Requiere Remediacion

Si el acceso esta saludable, el sistema lo explica y mantiene el puntaje visible para monitoreo de tendencia.

## 6. Reglas de Negocio

| Regla | Descripcion |
|---|---|
| BR-01 | El puntaje de salud del acceso debe ser explicable. |
| BR-02 | Las recomendaciones deben derivarse de senales observables de acceso y auditoria. |
| BR-03 | El sistema no debe remover acceso automaticamente salvo que la politica lo permita explicitamente. |
| BR-04 | Las tendencias de alto riesgo deben verse antes de convertirse en incidentes. |
| BR-05 | El puntaje de salud debe ser auditable y repetible con el mismo conjunto de datos. |

## 7. Criterios de Aceptacion

| # | Criterio de Aceptacion |
|---|---|
| 1 | Un actor puede ver un puntaje de salud para el acceso dentro de un tenant o alcance. |
| 2 | El sistema explica las senales que influyeron en el puntaje. |
| 3 | El sistema recomienda una siguiente accion de gobierno. |
| 4 | El actor puede usar la recomendacion para lanzar una revision o un flujo de limpieza. |
| 5 | Los estados de acceso saludables y riesgosos son visibles en el tiempo. |

## 8. Requisitos Tecnicos

- Calcular la salud del acceso a partir de auditoria, asignacion, revision y vencimiento.
- Proveer reglas y umbrales de puntuacion explicables.
- Soportar recomendaciones sin cambiar automaticamente el acceso.
- Permitir que los resultados de salud alimenten campanas de revision, limpieza de paquetes y expiracion de accesos privilegiados.
- Preservar el aislamiento por tenant y el recalculo deterministico con las mismas entradas.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Entidades de Dominio | `AuditRecord`, `Profile`, `Role`, `ApprovalRequest`, `AccessEnforcementPolicy` |
| Historias Funcionales | FS-16, FS-28, FS-31, FS-32 |
| ADRs | ADR-0016, ADR-0033, ADR-0066 |
