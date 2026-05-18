# Functional Story 12: Ejecutar Proceso de Promoción de Rol

## 1. Propósito de Negocio

UMS debe soportar evolución controlada de roles para que los usuarios avancen cuando cumplan criterios de antigüedad, cumplimiento, desempeño o aprobación. Las promociones deben ser explicables y auditables.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Evaluador de Promoción** | Detecta usuarios elegibles para promoción. |
| **Administrador Aprobador** | Revisa y aprueba o rechaza la promoción. |
| **Usuario** | Recibe el cambio de rol resultante. | ## 3. Precondiciones de Negocio

- La jerarquía de roles estáá definida.
- Los criterios de promoción estáán configurados.
- El usuario tiene un perfil activo elegible para promoción.

## 4. Flujo Funcional Principal

1. El sistema evalúa usuarios contra los criterios de promoción configurados.
2. Si un usuario cumple los criterios, el sistema lo marca como elegible para promoción.
3. El administrador responsable recibe la oportunidad de promoción.
4. El administrador revisa la evidencia y decide si aprueba.
5. Si se aprueba, el rol del usuario se actualiza.
6. El sistema registra la decisión de promoción y su evidencia.

## 5. Flujos Alternativos y Excepciones

### A. Criterios No Cumplidos

Si el usuario no cumple uno o más criterios, el sistema mantiene su rol actual y registra la razón.

### B. Promoción Rechazada

Si el administrador rechaza la promoción, el usuario permanece en el rol actual y se conserva el motivo.

## 6. Reglas de Negocio

1. La promoción solo puede avanzar hacia un rol de nivel superior.
2. Los requisitos obligatorios de cumplimiento deben satisfacerse antes de promover.
3. Puede requerirse aprobación manual según los criterios configurados.
4. Toda decisión de promoción debe ser trazable.

## 7. Criterios de Aceptación

1. Los usuarios elegibles pueden identificarse según criterios configurados.
2. Los usuarios con documentos obligatorios vencidos no son promovidos.
3. Las promociones aprobadas actualizan el rol efectivo del usuario.
4. Las promociones rechazadas conservan una razón clara.

## 8. Requisitos Técnicos

> [!NOTE]
> En la implementación real de C# (base de código), el motor de promociones está implementado mediante dos agregados independientes en el espacio de nombres **[Ums.Domain.IGA](file:///d:/Users/aarroyo/personal/sources/ums/src/apps/app-api-dotnet/Ums.Domain/IGA/)**:
> 1. **[RoleMaturityStatus](file:///d:/Users/aarroyo/personal/sources/ums/src/apps/app-api-dotnet/Ums.Domain/IGA/RoleMaturityStatus/RoleMaturityStatus.cs)**: Mantiene las capacitaciones, certificaciones, score de desempeño e invariantes de elegibilidad del usuario.
> 2. **[PromotionRequest](file:///d:/Users/aarroyo/personal/sources/ums/src/apps/app-api-dotnet/Ums.Domain/IGA/PromotionRequest/PromotionRequest.cs)**: Orquesta el flujo de aprobación transaccional y realiza análisis de riesgo automatizados.

- Monitorear la elegibilidad y métricas del usuario en el Agregado Root `RoleMaturityStatus`.
- Gestionar las etapas de la transacción de promoción y el análisis de impacto de riesgo en el Agregado Root `PromotionRequest` (con su entidad hija `PromotionImpactAnalysis`).
- Hacer cumplir las invariantes de elegibilidad (antigüedad mínima en nivel: Junior 6 meses, Intermediate 12 meses, Senior 18 meses, Lead 24 meses; score de desempeño >= 3.0; sin bloqueos de cumplimiento) antes del envío.
- Emitir Eventos de Dominio específicos: `PromotionRequestCreated`, `PromotionRequestSubmitted`, `PromotionRequestApproved`, `PromotionRequestExecuted`, `PromotionRequestVerified`.

## 9. Trazabilidad

- Entidades: `RoleMaturityStatus` (AR), `PromotionRequest` (AR), `PromotionImpactAnalysis` (Entidad Hija)
- ADRs: ADR-0046, ADR-0036
- Historias relacionadas: FS-11, FS-14
