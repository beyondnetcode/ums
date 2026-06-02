# ADR-0072: Resolución Dinámica del Método de Autenticación — Desde Configuración, No Desde Código

**Estado:** Aceptado
**Fecha:** 2026-05-31
**Responsable de Decisión:** Arquitectura
**Relacionados:**
- [ADR-0071: Motor del Grafo de Autorización](./0071-auth-graph-engine.es.md)
- [ADR-0054: Aislamiento de Shell Libraries](./0054-shell-library-isolation.md)
- [ADR-0077: Frontera de Autorizacion para Gestion del Portal del Tenant](./0077-tenant-portal-management-authorization-boundary.es.md)

---

## Contexto

UMS soporta autenticación Local (BCrypt) e IDP externo por tenant. Anteriormente, el endpoint de login usaba únicamente BCrypt de forma incondicional — una estrategia hardcodeada que no reflejaba el método de autenticación configurado del tenant. Ahora el flujo de gestion del portal y el flujo de la API externa requieren comportamientos distintos aunque pertenezcan al mismo tenant.

El método de autenticación del tenant está controlado por `AUTH_USE_EXTERNAL_IDP` (un parámetro en el ParameterCatalog, con scope de tenant). Cuando es `true`, el tenant usa Proveedores de Identidad externos (Azure AD, Okta, etc.); cuando es `false`, se usa BCrypt local. Este cambio de configuración debe tomar efecto sin desplegar nuevo código.

---

## Decisión

### 1. Puerto IAuthMethodResolver

`IAuthMethodResolver.ResolveAsync(tenantId, scope)` lee `AUTH_USE_EXTERNAL_IDP` desde `IConfigurationProvider` (ya en memoria) — **cero consultas a la BD por request**. Retorna:
- `AuthMethod.Local()` cuando es false
- `AuthMethod.Idp(activeProvider)` cuando es true y existe un IDP activo
- `Result.Failure("AUTH_011")` cuando es true pero no hay IDP activo configurado

Para `AuthAccessScope.PortalManagement`, el resolver siempre devuelve `AuthMethod.Local()` y no requiere la configuracion IDP del tenant.

El resolver es un servicio de aplicación puro — sin dependencias de infraestructura.

### 2. Patrón Strategy a través de Puertos de Dominio

```
ILocalAuthStrategy  — valida credenciales BCrypt (capa Application)
IIdpAuthStrategy    — despacha al IIdpAuthAdapter correcto por nombre de estrategia
IIdpAuthAdapter     — puerto ACL por tipo de proveedor (capa Infrastructure)
```

`IIdpAuthStrategy` usa Shell.Factory para resolver el `IIdpAuthAdapter` correcto (mismo patrón que `IdpResolutionStrategyFactorySetup`). Nuevos adaptadores IDP se agregan registrando una nueva implementación — sin cambios en el flujo de autenticación.

### 3. Adaptador IDP Stub para Desarrollo

`StubIdpAuthAdapter` acepta credenciales que comienzan con el prefijo `MOCK-` y retorna una `ExternalIdentity` válida. Se registra únicamente en entornos no-Productivos, permitiendo pruebas de extremo a extremo del flujo IDP sin un proveedor externo real.

### 4. Códigos de Error de Autenticación

| Código | Significado |
|---|---|
| AUTH_001 | Error de validación (campos faltantes) |
| AUTH_002 | Tenant no encontrado |
| AUTH_003 | Tenant no activo |
| AUTH_004 | Usuario IDP sin cuenta UMS |
| AUTH_005 | Cuenta de usuario no activa |
| AUTH_006 | Credenciales inválidas (Local) |
| AUTH_011 | Modo IDP configurado pero sin proveedor activo |
| AUTH_012 | Sin adaptador IDP registrado para la estrategia |

---

## Justificación

### Por qué leer desde IConfigurationProvider y no desde la BD

`IConfigurationProvider` carga todos los valores de configuración al inicio y los cachea en memoria. Leer desde él es una búsqueda en diccionario — sin round-trip a la BD. Cuando se cambia el modo de autenticación de un tenant (por ejemplo, mediante el toggle del IdpPanel), la configuración se actualiza en la BD y el proveedor se refresca. El cambio toma efecto en el siguiente login sin reiniciar el servicio.

### Por qué no hardcodear BCrypt como default

Hardcodear crea un acoplamiento implícito entre el flujo de autenticación y una implementación específica. Cualquier nueva estrategia de autenticación (SAML2, OAuth2, personalizada) requeriría modificar el command handler. El patrón strategy permite agregar nuevos métodos registrando un nuevo adaptador.

---

## Consecuencias

### Positivas

- Agregar un nuevo adaptador IDP solo requiere implementar `IIdpAuthAdapter` y registrarlo en `IdpAuthAdapterFactorySetup`
- Los cambios de método de autenticación toman efecto inmediatamente sin despliegue de código
- El adaptador stub habilita pruebas completas del flujo IDP en desarrollo
- El método de autenticación queda registrado en el audit trail para trazabilidad
- El acceso de gestion del portal sigue siendo utilizable aunque la configuracion externa IDP del tenant cambie o este temporalmente indisponible

### Compromisos

- El flujo IDP requiere una `UserAccount` válida en UMS que coincida con el email asertado por el IDP. El provisionamiento just-in-time está fuera del alcance.

---

## Implementación

| Componente | Ubicación |
|---|---|
| Puerto `IAuthMethodResolver` | `Ums.Domain/Identity/Auth/IAuthMethodResolver.cs` |
| `AuthMethodResolverService` | `Ums.Application/Identity/Auth/AuthMethodResolverService.cs` |
| Puerto `ILocalAuthStrategy` | `Ums.Domain/Identity/Auth/ILocalAuthStrategy.cs` |
| `LocalAuthStrategyService` | `Ums.Application/Identity/Auth/LocalAuthStrategyService.cs` |
| Puerto `IIdpAuthStrategy` | `Ums.Domain/Identity/Auth/IIdpAuthStrategy.cs` |
| Puerto ACL `IIdpAuthAdapter` | `Ums.Domain/Identity/Auth/IIdpAuthAdapter.cs` |
| `IdpAuthStrategyDispatcher` | `Ums.Infrastructure/Identity/Auth/IdpAuthStrategyDispatcher.cs` |
| `StubIdpAuthAdapter` | `Ums.Infrastructure/Identity/Auth/StubIdpAuthAdapter.cs` |
| `IdpAuthAdapterFactorySetup` | `Ums.Infrastructure/Identity/Auth/IdpAuthAdapterFactorySetup.cs` |

---

**[Registro ADR](./index.md)**
