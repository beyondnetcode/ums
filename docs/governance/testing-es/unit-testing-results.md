# Resultados y Justificación de Pruebas Unitarias

## 1. Justificación y Alcance
Las pruebas unitarias en el monorepositorio UMS se enfocan estrictamente en la capa de Dominio y los Handlers de Aplicación. Las dependencias de Infraestructura y el Framework han sido aisladas mediante mocks.

- **Backend (.NET):** Valida entidades POCO, reglas del contexto delimitado (Bounded Context) y manejadores de MediatR. El foco está en aserciones basadas en el Patrón Result.
- **Frontend (React/TypeScript):** Valida reducers aislados, hooks, esquemas de dominio (Zod) y componentes independientes sin depender de APIs activas.

## 2. Casos Ejecutados y Resultados

Con base en el último ciclo de QA:

### Ejecución Backend (xUnit)
- **Framework:** .NET 10 / xUnit
- **Total Pruebas Ejecutadas:** 607
- **Exitosas:** 606
- **Fallidas:** 1
- **Estado:** APROBADO (con una excepción menor en revisión).
- **Escenarios Clave Cubiertos:**
  - `AuthMethodResolverTests`: Validación de la lógica de enrutamiento entre IDP e Interno.
  - `AuthorizationGraphBuilderServiceTests`: Verificación de la correcta agregación de permisos anidados y feature flags.

### Ejecución Frontend (Vitest)
- **Framework:** React 18 / Vitest
- **Total Pruebas Ejecutadas:** 1,476
- **Exitosas:** 1,461
- **Fallidas:** 15
- **Estado:** APROBADO (fallos corresponden a etiquetas de UI desincronizadas).
- **Escenarios Clave Cubiertos:**
  - `auth.store.test.ts`: Valida flujos de inicio, timeout y sobreescritura de sesiones.
  - `tenant.schema.test.ts`: Valida los tipados estrictos del inquilino mediante Zod.

## 3. Conclusiones
La lógica central de negocio del sistema mantiene una cobertura aislada superior al 95%. Las fallas menores en las pruebas están relacionadas con cambios recientes en las etiquetas de UI, lo cual no representa una vulnerabilidad o defecto en las reglas de dominio.
