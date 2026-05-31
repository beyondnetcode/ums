# Resolución del Método de Autenticación — Cómo UMS Selecciona la Estrategia de Autenticación

> **Idioma:** [English](../../domain/identity/auth-method-resolution.md) | [Español](./auth-method-resolution.md)

**Bounded Context:** Identity (`Ums.Domain.Identity`)
**Propietario:** `AuthMethodResolverService` (capa Application)
**Estado:** Producción

---

## 1. Descripción General

UMS soporta dos métodos de autenticación por tenant:

| Método | Descripción |
|---|---|
| **Local** | Validación de credencial BCrypt. Hash de contraseña almacenado en la entidad `PasswordCredential`. |
| **IDP** | Autenticación federada delegada a un Proveedor de Identidad externo (Azure AD, Okta, SAML2, etc.). |

El método aplicable para un tenant dado se **resuelve en tiempo de login desde configuración**, nunca está hardcodeado. Esto significa que el método de autenticación puede cambiar sin redesplegar la aplicación.

---

## 2. Parámetro de Control

El parámetro `AUTH_USE_EXTERNAL_IDP` (Booleano, con scope de tenant) gobierna la selección del método:

| Valor | Resultado |
|---|---|
| `false` | `AuthMethod.Local()` — validación BCrypt |
| `true` + IDP activo | `AuthMethod.Idp(activeProvider)` — autenticación federada |
| `true` + sin IDP activo | `Result.Failure("AUTH_011")` — error de configuración |

Este parámetro reside en el `ParameterCatalog` y se carga en `IConfigurationProvider` al iniciar la aplicación. Los cambios aplicados mediante la UI del IdpPanel se persisten en la base de datos y disparan un refresco del proveedor — el nuevo método toma efecto en el **siguiente login** sin reiniciar el servicio.

---

## 3. Flujo de Resolución

```
LoginCommand recibido
        │
        ▼
IAuthMethodResolver.ResolveAsync(tenantId)
        │
        ├─ lee AUTH_USE_EXTERNAL_IDP desde IConfigurationProvider (en memoria)
        │
        ├── false → AuthMethod.Local
        │            │
        │            └─► ILocalAuthStrategy.AuthenticateAsync(email, password)
        │                    └─ BCrypt.Verify(hash, password)
        │
        └── true  → lee IdentityProvider activo desde el aggregate del tenant
                     │
                     ├── ninguno → Result.Failure("AUTH_011")
                     │
                     └── encontrado → AuthMethod.Idp(provider)
                                        │
                                        └─► IIdpAuthStrategy.AuthenticateAsync(provider, credentials)
                                                 └─ Shell.Factory resuelve IIdpAuthAdapter por nombre de estrategia
                                                      └─ adapter.AuthenticateAsync(credentials)
```

---

## 4. Puertos y Estrategias

### `IAuthMethodResolver`

Servicio de aplicación puro. Lee `IConfigurationProvider` — **sin consulta a BD por request**.

```csharp
Task<Result<AuthMethod>> ResolveAsync(Guid tenantId, CancellationToken ct);
```

### `ILocalAuthStrategy`

Valida credenciales BCrypt contra la `PasswordCredential` activa.

```csharp
Task<Result<AuthenticatedPrincipal>> AuthenticateAsync(
    string email, string password, Guid tenantId, CancellationToken ct);
```

### `IIdpAuthStrategy`

Despacha al `IIdpAuthAdapter` correcto por el campo `Strategy` del proveedor (ej. `AZURE_AD`, `OKTA`, `SAML2`, `GENERIC_OIDC`). Resuelto vía Shell.Factory.

```csharp
Task<Result<ExternalIdentity>> AuthenticateAsync(
    IdentityProvider provider, string credentials, CancellationToken ct);
```

### `IIdpAuthAdapter` (Puerto ACL — capa Infrastructure)

Una implementación por tipo de IDP. Registrado en `IdpAuthAdapterFactorySetup`.

```csharp
string StrategyName { get; }  // coincide con el valor de IdpStrategyHint
Task<Result<ExternalIdentity>> AuthenticateAsync(
    IdentityProvider provider, string credentials, CancellationToken ct);
```

---

## 5. Parametrización Global vs. Por Tenant

`IConfigurationProvider` combina parámetros de dos niveles:

| Nivel | Alcance | Precedencia |
|---|---|---|
| Default global | `TenantId = NULL` (raíz) | Menor |
| Override por tenant | `TenantId = <específico>` | Mayor |

Cuando un tenant no tiene un override explícito de `AUTH_USE_EXTERNAL_IDP`, aplica el default global. Esto significa que un default a nivel de plataforma (por ejemplo, autenticación Local) toma efecto para todos los tenants nuevos sin configuración manual, y los tenants individuales pueden sobreescribirlo de forma independiente.

---

## 6. Caché en Memoria

`IConfigurationProvider` se pobla una vez al inicio (o al refresco explícito) y sirve las búsquedas posteriores como lecturas O(1) en diccionario. El resolver por lo tanto agrega **latencia cero** por request de autenticación más allá de la lógica de capa de aplicación.

Cuando el toggle del IdpPanel dispara `UpdateAuthModeCommand`, el command handler:
1. Actualiza el parámetro en la base de datos
2. Llama a `IConfigurationProvider.RefreshAsync()` para re-poblar la caché en memoria
3. El nuevo método toma efecto para el siguiente login — sin reinicio requerido

---

## 7. Stub de Desarrollo

`StubIdpAuthAdapter` se registra en entornos no-Productivos. Acepta cualquier credencial prefijada con `MOCK-` y retorna una `ExternalIdentity` fabricada que coincide con el usuario UMS por sufijo de email. Esto permite pruebas de extremo a extremo del flujo IDP sin un proveedor externo real.

---

## 8. Relación con el Grafo de Autorización

Después de que el método de autenticación se resuelve y el usuario es autenticado, `AuthorizationGraphBuilderService` construye el `AuthorizationGraph` completo. La sección `authentication` del grafo registra:
- El método resuelto (`Local` o `IDP`)
- El nombre y estrategia del proveedor (cuando es IDP)
- `mfaRequired`, `issuedAt`, `sessionExpiresAt`

Ver [Grafo de Autorización](./auth-graph.md) para la estructura completa del grafo.

---

## 9. Códigos de Error

| Código | Disparador |
|---|---|
| AUTH_001 | Error de validación — campos requeridos faltantes |
| AUTH_002 | Tenant no encontrado |
| AUTH_003 | Tenant no activo |
| AUTH_004 | Usuario IDP sin `UserAccount` correspondiente en UMS |
| AUTH_005 | `UserAccount.Status != ACTIVE` |
| AUTH_006 | Credenciales inválidas (BCrypt Local) |
| AUTH_011 | `AUTH_USE_EXTERNAL_IDP = true` pero sin IDP activo configurado |
| AUTH_012 | Sin `IIdpAuthAdapter` registrado para el nombre de estrategia del proveedor |

---

## 10. Referencias

- [ADR-0072: Resolución Dinámica del Método de Autenticación](../../architecture/adrs/0072-dynamic-auth-method-resolution.es.md)
- [ADR-0071: Motor del Grafo de Autorización](../../architecture/adrs/0071-auth-graph-engine.es.md)
- [Grafo de Autorización](./auth-graph.md)
- [Aggregate Tenant](./tenant.md)
- [`AuthMethodResolverService`](../../../src/apps/ums.api/Ums.Application/Identity/Auth/AuthMethodResolverService.cs)
- [`IdpAuthAdapterFactorySetup`](../../../src/apps/ums.api/Ums.Infrastructure/Identity/Auth/IdpAuthAdapterFactorySetup.cs)
