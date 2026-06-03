# ADR-0080: Previsualización del Auth Graph — Pipeline Interno vs Externo

**Estado:** Aceptado  
**Fecha:** 2026-06-03  
**Responsable:** Arquitectura  
**Relacionados:**
- [ADR-0071: Motor de Auth Graph](./0071-auth-graph-engine.md)
- [ADR-0072: Resolución Dinámica de Método de Auth](./0072-dynamic-auth-method-resolution.md)
- [ADR-0074: Versionado de Schema del Auth Graph](./0074-auth-graph-schema-versioning.md)

---

## Contexto

El portal UMS necesita que los administradores puedan previsualizar el auth graph para un perfil dado sin emitir una solicitud de autenticación real. Esto es necesario para:

- Validar que la asignación de template de permisos y rol de un perfil producen el grafo esperado antes de asignarlo a usuarios en producción.
- Depurar problemas de control de acceso durante sesiones de soporte.

El enfoque naive sería un constructor de grafo separado y simplificado para la previsualización. Sin embargo, tener dos constructores independientes significa que el preview puede divergir de la salida real, lo que anula su propósito diagnóstico.

---

## Decisión

### Regla: el preview usa el mismo `IAuthorizationGraphBuilder`

El endpoint interno de preview `GET /api/v1/profiles/{id}/auth-graph/preview` llama a `PreviewProfileAuthGraphCommandHandler`, que usa la misma inyección de `IAuthorizationGraphBuilder` que el flujo externo `POST /api/v1/client/authenticate`.

Existe **un solo** pipeline de construcción del grafo. El preview se distingue de la autenticación real únicamente por:

| Aspecto | Auth externo (`POST /client/authenticate`) | Preview interno (`GET /profiles/{id}/auth-graph/preview`) |
|---|---|---|
| Validación de credenciales | Sí — resuelve y valida credenciales | **Omitida** — el perfil se busca directamente por ID |
| Resolución del método de auth | Sí | No necesaria — el perfil ya existe |
| Evento de auditoría | `Auth.Success` / `Auth.Failure` | `Graph.Preview.Internal` |
| Cabeceras de respuesta | — | `X-Preview-Mode: internal-preview`, `X-Request-Id` |
| Control de acceso | Público (basado en token) | Requiere sesión de portal autenticada (`IUserContext.IsAuthenticated`) |

### Qué se preserva

- Resolución del formato del grafo (mismo `IAuthGraphFormatProvider` y `IFactory<..., IAuthorizationGraphSerializer>`).
- Override del formato mediante parámetro `?format=` — misma lógica que el endpoint externo.
- Parámetros del tenant (`tenantId`, `tenantCode`, `authMethodUsed`) propagados desde el perfil.
- Campo `requestId` en la respuesta para trazabilidad.

### Endpoint

```
GET /api/v1/profiles/{profileId:guid}/auth-graph/preview[?format=JSON|CBOR|...]
Autorización: cabeceras X-User-Id / X-Tenant-Id (sesión del portal)
```

Respuesta:
```json
{
  "format": "JSON",
  "graph": { ... },
  "requestId": "...",
  "previewMode": "internal-preview",
  "profileId": "...",
  "userId": "...",
  "tenantId": "...",
  "tenantCode": "...",
  "authMethodUsed": "..."
}
```

---

## Consecuencias

- **Positivo:** Los administradores ven exactamente lo que vería el llamador externo — sin riesgo de divergencia.
- **Positivo:** Una sola ruta de código para mantener y testear.
- **Negativo:** El preview hereda el costo completo de construcción del grafo; aceptable porque es una herramienta diagnóstica bajo demanda, no una ruta de alto tráfico.
- **Neutral:** La resolución de credenciales es el único paso exclusivo del flujo externo; omitirlo del preview no afecta la corrección del grafo.
