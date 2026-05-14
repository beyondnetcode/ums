# ADR 0046: Gobernanza de Evolución y Promoción de Roles

## Estatus
Aprobado

## Contexto
A medida que los usuarios adquieren experiencia o completan certificaciones, suelen pasar de roles junior a posiciones más senior. Gestionar estas promociones manualmente es laborioso y propenso a sesgos o descuidos. Necesitamos un mecanismo formal para definir jerarquías de roles y automatizar la verificación de criterios para la promoción, manteniendo una supervisión administrativa estricta.

## Decisión
Implementaremos un **Motor de Evolución de Roles Basado en Flags**:

1.  **Roles Jerárquicos**:
    *   La entidad `ROLE` soporta un `ParentRoleId` auto-referencial para definir árboles organizacionales o de autoridad.
    *   **Lógica**: La promoción es estrictamente unidireccional (hacia arriba) basada en `HierarchyLevel` y `PromotionOrder`.

2.  **Criterios Activables (Flags)**:
    *   Las promociones se rigen por `ROLE_PROMOTION_CRITERIA`.
    *   En lugar de reglas fijas, utilizamos un enfoque de "Tabla de Flags":
        *   `FlagSeniority`: Verifica los días mínimos en el rol actual.
        *   `FlagCompliance`: Verifica si todos los documentos/certificaciones obligatorios son válidos.
        *   `FlagBusinessScore`: Verifica los rankings de desempeño.
        *   `FlagManualApproval`: Asegura que el proceso no pueda completarse sin intervención humana.

3.  **Promotion Watcher (Background)**:
    *   Un worker en segundo plano escanea periódicamente a los usuarios frente a los flags activos para su próximo rol potencial.
    *   Cuando se cumplen todos los flags activos, se dispara un `PromotionOpportunityEvent`.

4.  **Workflow de Aprobación Mandatorio**:
    *   Cumplir con los criterios automatizados solo convierte al usuario en "Candidato" (`CRITERIA_MET`).
    *   Siempre se inicializa una `APPROVAL_REQUEST` formal para finalizar la promoción.

## Consecuencias
*   **Positivo**: Fomenta el crecimiento profesional dentro de la plataforma. Asegura que las promociones se basen en datos objetivos y verificables. Reduce la carga administrativa.
*   **Negativo**: Complejo de configurar correctamente (requiere una definición cuidadosa de los niveles). Potencial de "candidatos fantasma" si los criterios son demasiado laxos.
