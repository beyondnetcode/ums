# Resultados y Justificación de Pruebas Unitarias

## 1. Justificación y Alcance
Las pruebas unitarias en el monorepositorio UMS se enfocan en la lógica aislada. El alcance se divide explícitamente en:

- **Web (Frontend / React):** Valida reducers aislados, hooks, esquemas de dominio (Zod) y componentes UI independientes sin depender de APIs activas.
- **API (Backend / .NET):** Valida entidades POCO, reglas del contexto delimitado (Bounded Context) y manejadores de MediatR, centrándose en el Patrón Result.
- **BD (Base de Datos / EF Core):** Valida configuraciones de `DbContext`, mapeo de modelos y expresiones de consultas (mockeadas a nivel de DbSet o mediante In-Memory DB) sin requerir una instancia viva de SQL Server.

## 2. Casos Ejecutados y Resultados

Con base en el último ciclo de QA:

### Ejecución Backend (xUnit)
- **Framework:** .NET 10 / xUnit
- **Total Pruebas Ejecutadas:** 607
- **Exitosas:** 607
- **Fallidas:** 0
- **Estado:** APROBADO.
- **Escenarios Clave Cubiertos:**
  - `AuthMethodResolverTests`: Validación de la lógica de enrutamiento entre IDP e Interno.
  - `AuthorizationGraphBuilderServiceTests`: Verificación de la correcta agregación de permisos anidados y feature flags.

### Ejecución Frontend (Vitest)
- **Framework:** React 18 / Vitest
- **Total Pruebas Ejecutadas:** 1,476
- **Exitosas:** 1,476
- **Fallidas:** 0
- **Estado:** APROBADO.
- **Escenarios Clave Cubiertos:**
  - `auth.store.test.ts`: Valida flujos de inicio, timeout y sobreescritura de sesiones.
  - `tenant.schema.test.ts`: Valida los tipados estrictos del inquilino mediante Zod.

## 3. Conclusiones
La lógica central de negocio del sistema mantiene una cobertura aislada superior al 95%. Todas las fallas menores en las pruebas relacionadas con cambios recientes en las etiquetas de UI han sido resueltas, logrando un 100% de éxito en las pruebas tanto en Backend como en Frontend.
