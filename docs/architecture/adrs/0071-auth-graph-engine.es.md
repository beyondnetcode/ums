# ADR-0071: Motor del Grafo de Autorización — Motor Central de Autenticación y Autorización

**Estado:** Aceptado
**Fecha:** 2026-05-31
**Responsable de Decisión:** Arquitectura
**Relacionados:**
- [ADR-0054: Aislamiento de Shell Libraries](./0054-shell-library-isolation.md)
- [ADR-0061: Execution Context Accessor](./0061-execution-context-accessor.md)
- [ADR-0068: Alcance del Sistema de Feature Flags](./0068-feature-flag-system-scope.md)
- [ADR-0072: Resolución Dinámica del Método de Autenticación](./0072-dynamic-auth-method-resolution.es.md)
- [ADR-0077: Frontera de Autorizacion para Gestion del Portal del Tenant](./0077-tenant-portal-management-authorization-boundary.es.md)

---

## Contexto

El objetivo principal de UMS es actuar como el **motor central de autenticación y autorización** para los sistemas cliente multi-tenant. Antes de esta decisión, el endpoint de login (`POST /api/v1/auth/login`) retornaba un array vacío `Permissions: []` y realizaba únicamente validación BCrypt. Los sistemas cliente recibían un JWT sin datos de autorización accionables, obligándolos a implementar su propia lógica de permisos o re-consultar UMS en cada request.

El sistema necesita:
1. Autenticar usuarios vía Local (BCrypt) o IDP externo según la configuración del tenant, manteniendo el acceso de gestion del portal en modo local y el flujo de API externa alineado con el IDP del tenant
2. Construir un grafo de autorización completo y autocontenido desde el profile del usuario
3. Retornar el grafo al cliente para que pueda operar de forma autónoma sin consultas adicionales a UMS

---

## Decisión

### 1. AuthorizationGraph — Modelo de Lectura Inmutable y Autocontenido

Tras la autenticación, UMS construye un registro `AuthorizationGraph` que contiene:

```
AuthorizationGraph
├── context        — usuario, tenant, systemSuite, rol, profile, branch
├── authentication — método (Local|IDP), proveedor, mfaRequired, expiración
├── actions[]      — todas las acciones registradas en el SystemSuite
├── menuAccess[]   — árbol Módulo→Menú→SubMenú→Opción con AccessEffect por opción
├── domainPermissions[] — recursos de dominio con efecto por acción (Aggregate/Entity)
├── featureFlags[] — flags evaluados contra el contexto del usuario al momento de autenticación
├── effectiveConfig — parámetros resueltos por tenant (timeout de sesión, MFA, etc.)
├── scopes[]       — scopes OAuth2-style "resourceCode.actionCode" desde permisos Allow
├── generatedAt    — timestamp UTC
└── validUntil     — generatedAt + SESSION_TIMEOUT_MINUTES
```

El grafo es **inmutable** tras su construcción. Los sistemas cliente lo cachean durante la vigencia de la sesión y toman decisiones de autorización localmente.

### 2. Resolución de Permisos — Deny Gana, Override Tiene Precedencia

Para cada trío `(TargetType, TargetId, ActionId)`:
1. Encontrar `ProfilePermission` donde `IsActive = true`
2. Si `IsOverride = true` → usar valores del PP (Source = Override)
3. Si `IsOverride = false` → usar valores originales del TemplateItem (Source = Template)
4. `IsDenied = true` → `AccessEffect.Deny` (siempre gana sobre Allow)
5. `IsAllowed = true` → `AccessEffect.Allow`
6. Sin entrada → `AccessEffect.NotGranted` (denegación implícita)

### 3. Dos Endpoints Dedicados

- **`POST /api/v1/auth/login`** — endpoint existente, ahora retorna el grafo completo. Establece cookie de sesión para el frontend web (compatible con versiones anteriores). Retorna `LoginSuccessResponse` enriquecido con `AuthorizationGraph`.
- **`POST /api/v1/client/authenticate`** — nuevo endpoint stateless para sistemas cliente externos. Sin cookie. Retorna `ClientAuthResponse` con JWT + grafo serializado.

### 4. Serialización Dinámica del Grafo vía Shell.Factory

El grafo se serializa en el formato configurado por el tenant (parámetro `AUTH_GRAPH_DEFAULT_FORMAT`, default: JSON). Formatos soportados: JSON, XML, YAML, CSV.

Los clientes pueden sobrescribir mediante el query param `?format=xml` o el header `Accept`. Nuevos formatos se agregan registrando una implementación de `IAuthorizationGraphSerializer` en `AuthorizationGraphSerializerFactorySetup` — sin cambios en el código de aplicación.

---

## Justificación

### Por qué un grafo autocontenido en lugar de consultas bajo demanda

Los sistemas cliente necesitan verificar permisos múltiples veces por request (renderizado de menús, visibilidad de botones, guardas de acceso a API). Un grafo autocontenido elimina N round-trips a UMS por sesión. El grafo es válido por `SESSION_TIMEOUT_MINUTES` — la misma duración que el token de sesión.

### Por qué el grafo se construye desde el Profile y no desde los claims JWT

Los claims JWT tienen un límite de tamaño y son estáticos después de su emisión. El grafo es rico (cientos de permisos), necesita incluir datos dinámicos (feature flags) y debe reflejar el estado en el momento de la autenticación. El grafo se serializa por separado y se embebe en el body de la respuesta, no en el JWT (el JWT solo lleva claims de resumen compacto).

### Por qué AccessEffect y no PermissionEffect

`Ums.Domain.Enums.PermissionEffect` ya existe como DomainEnumeration con `Allow` y `Deny`. El grafo necesita un tercer valor `NotGranted` (denegación implícita — no existe entrada de permiso). Un nuevo enum `AccessEffect` en el namespace del grafo evita extender la enumeración existente.

---

## Consecuencias

### Positivas

- Los sistemas cliente reciben todo lo que necesitan en una única respuesta de autenticación
- La lógica de permisos está centralizada en UMS — los sistemas cliente no implementan reglas de acceso
- Las evaluaciones de feature flags ocurren al momento de la autenticación con el contexto completo del usuario
- El formato del grafo es configurable por tenant y extensible sin cambios de código
- El audit trail registra cada evento de autenticación con método, resultado e IP

### Compromisos

- El tamaño del grafo crece con el número de opciones y recursos de dominio del SystemSuite. Para suites con más de 500 opciones, el payload de respuesta puede ser grande. Una proyección de read-model (futuro) puede abordar esto.
- El grafo es un snapshot al momento de la autenticación. Los cambios de permisos toman efecto en el siguiente login. Los clientes con sesiones de larga duración pueden mantener grafos desactualizados.

---

## Implementación

| Componente | Ubicación |
|---|---|
| Registro `AuthorizationGraph` | `Ums.Domain/Authorization/Graph/` |
| Puerto `IAuthorizationGraphBuilder` | `Ums.Domain/Authorization/Graph/IAuthorizationGraphBuilder.cs` |
| `AuthorizationGraphBuilderService` | `Ums.Application/Authorization/Graph/AuthorizationGraphBuilderService.cs` |
| `IAuthorizationGraphSerializer` | `Ums.Application/Authorization/Graph/Serializers/` |
| Serializadores JSON/XML/YAML/CSV | `Ums.Infrastructure/Authorization/Graph/` |
| `AuthorizationGraphSerializerFactorySetup` | `Ums.Infrastructure/Authorization/Graph/` |
| `AuthGraphFormatProvider` | `Ums.Application/Authorization/Graph/AuthGraphFormatProvider.cs` |
| `POST /api/v1/auth/login` | `Ums.Presentation/Endpoints/Identity/Auth/AuthEndpoints.cs` |
| `POST /api/v1/client/authenticate` | `Ums.Presentation/Endpoints/Identity/Auth/ClientAuthEndpoints.cs` |

---

**[Registro ADR](./index.md)**
