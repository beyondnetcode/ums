# ADR-0040: Estrategia de Token Federado para Contexto Jerárquico

*   **Estado:** Propuestao
*   **Fecha:** 2026-05-13
*   **Autores:** Equipo de Arquitectura Senior & Product Owners

---

## 1. Contexto y Problema

En un sistema multi-inquilino jerárquico con administración delegada y herencia de políticas, el token de autenticación debe transportar suficiente contexto para que los servicios downstream autoricen peticiones sin consultas repetidas a la base de datos. Sin embargo, incrustar el contexto jerárquico completo en un JWT provoca inflado del token.

Los siguientes claims crecen con la profundidad de la jerarquía de inquilinos y la longitud de la cadena de delegación:

- `root_tenant_id`, `effective_tenant_id`, `hierarchy_level`, `hierarchy_path`
- `user_type`, `roles[]`, `managed_tenants[]`
- `delegation_chain[]` con alcance y TTL
- `resolved_policies[]` para autorización rápida

A 7 niveles de jerarquía con 5 saltos de delegación y 50 políticas resueltas, un JWT autocontenido supera los 8KB — excediendo los límites de tamaño de cabecera HTTP en muchas configuraciones de proxy y gateway.

---

## 2. Decisión Arquitectónica

Adoptaremos una **estrategia de token de modo dual**: JWT para comunicación BFF-Frontend donde la latencia y el tamaño del payload son críticos, y Tokens Opacos de Referencia con Introspección para comunicación servicio-a-servicio y cross-boundary donde se requiere contexto completo.

### 2.1. Matriz de Estrategia de Token

| Escenario | Tipo de Token | Justificación |
|---|---|---|
| Frontend ↔ BFF (navegador) | JWT (claims mínimos) | Cabecera < 2KB, sin latencia de introspección |
| BFF ↔ Backend API (interno) | Opaco (token de referencia) | Contexto completo vía introspección, cabecera < 200 bytes |
| Backend ↔ Backend (gRPC) | Opaco + mTLS | Identidad de servicio + contexto completo de usuario |
| IdP Externo ↔ UMS | JWT (del IdP) | OAuth2/OIDC estándar, transformado en entrada |
| Tokens de máquina de larga duración | Opaco (PAT) | Revocable, acotado, auditable | ### 2.2. JWT para Frontend (Claims Mínimos)

```json
{
  "sub": "U-abc-123",
  "email": "admin@subsidiary.com",
  "root_tenant_id": "T-root-001",
  "effective_tenant_id": "T-sub-045",
  "hierarchy_level": 2,
  "user_type": "TENANT_ADMIN",
  "roles": ["subsidiary.admin"],
  "iat": 1700000000,
  "exp": 1700003600,
  "jti": "unique-token-id-456"
}
```

Tamaño: ~450 bytes. Claims limitados a identidad y contexto básico. Las decisiones de autorización se toman del lado del servidor tras introspección del token o consulta interna.

### 2.3. Token Opaco para Servicio-a-Servicio

El servicio `OpaqueTokenService`: (1) genera un UUID como ID de token, (2) serializa el contexto completo (usuario, inquilino, roles, políticas resueltas, cadena de delegación) en Redis con TTL de 1 hora, (3) retorna un token de referencia corto (`ums_ref_v2:{tokenId}` en base64).

La introspección: (1) parsea el token de referencia, (2) obtiene el contexto de Redis, (3) verifica expiración, (4) valida que las concesiones de delegación siguen activas (con cache de respaldo), (5) retorna el `TokenContext`.

### 2.4. Middleware de Introspección de Token

El middleware `TokenIntrospectionMiddleware`: (1) extrae la cabecera `Authorization: Bearer`, (2) detecta tipo de token (opaco vs JWT por prefijo), (3) para tokens opacos: llama a `IntrospectAsync()`, hidrata `HttpContext.Items` con todo el contexto, inyecta variables de sesión RLS (`SET LOCAL app.root_tenant_id`, `app.effective_tenant_id`, `app.user_id`), (4) para JWT: valida y extrae claims mínimos.

### 2.5. Revocación de Token

`TokenRevoker`: (1) invalida todos los contextos de token del usuario en Redis, (2) invalida las concesiones de delegación activas asociadas, (3) registra evento de auditoría.

---

## 3. Consecuencias

### Positivas

*   **JWT pequeño para frontend**: ~450 bytes vs 8KB+. Cabe dentro de los límites de cabecera HTTP.
*   **Contexto completo para servicios**: Los tokens opacos transportan la cadena de delegación completa, políticas resueltas e inquilinos gestionados.
*   **Revocación inmediata**: Eliminar de Redis = token invalidado al instante.
*   **Traza de auditoría**: Emisión, introspección y revocación de tokens se registran con contexto completo.

### Negativas

*   **Latencia de introspección**: Cada llamada servicio-a-servicio requiere una búsqueda en Redis (1-3ms). Mitigación: El contexto se cachea por el TTL del token; la introspección es una sola búsqueda de clave.
*   **Dependencia de Redis**: La validación de tokens depende de la disponibilidad de Redis. Mitigación: Caché en memoria local como respaldo L2; circuit breaker si Redis estáá caído.
*   **Gestión de estado**: Tokens revocados permanecen en Redis hasta el TTL. Mitigación: TTL igual al vencimiento del token (máximo 1 hora); revocación elimina proactivamente.
*   **Complejidad operativa**: Dos tipos de token incrementan la superficie de pruebas. Mitigación: La detección de tipo es una simple verificación de formato.
