# ðŸ§ª Technical Enabler 2: Resolver ConfiguraciÃ³n JerÃ¡rquica del Sistema

Este caso de uso detalla el flujo para calcular la configuraciÃ³n del sistema "efectiva" para una aplicaciÃ³n cliente, mediante la evaluaciÃ³n y fusiÃ³n de capas de sobrescritura (override) jerÃ¡rquicas.

---

## ðŸ›ï¸ 1. DefiniciÃ³n del Caso de Uso

| Atributo | EspecificaciÃ³n |
| :--- | :--- |
| **Nombre** | Resolver ConfiguraciÃ³n JerÃ¡rquica del Sistema |
| **Actor Principal** | Sistema Cliente (M2M) o API Gateway |
| **Precondiciones** | El sistema que solicita la configuraciÃ³n estÃ¡ registrado en el UMS. |
| **Postcondiciones** | Se devuelve y se almacena en cachÃ© un objeto JSON que representa la configuraciÃ³n efectiva (luego de aplicar todas las sobrescrituras jerÃ¡rquicas). |

---

## ðŸ”„ 2. Flujo de TransacciÃ³n

```mermaid
sequenceDiagram
    autonumber
    participant Client as App Cliente
    participant ConfigAPI as API Motor Config
    participant Cache as CachÃ© Redis (cfg:sys)
    participant Resolver as Estrategia de ResoluciÃ³n Config
    participant DB as PostgreSQL

    Client->>ConfigAPI: GET /v1/config/system/{system_id}?tenant_id=X&branch_id=Y&env=PROD
    ConfigAPI->>Cache: Consultar cachÃ© por hash de config
    alt Acierto en CachÃ©
        Cache-->>ConfigAPI: Retornar configuraciÃ³n cacheadad
    else Fallo en CachÃ©
        ConfigAPI->>Resolver: Solicitar resoluciÃ³n de config
        Resolver->>DB: Obtener config base (Nivel Global)
        Resolver->>DB: Obtener config override tenant (Nivel Tenant)
        Resolver->>DB: Obtener config override sistema (Nivel Sistema)
        Resolver->>DB: Obtener config override sede (Nivel Sede)
        Resolver->>DB: Obtener config override entorno (Nivel Env)
        
        Resolver->>Resolver: Aplicar FusiÃ³n Profunda (Global <- Tenant <- Sistema <- Sede <- Env)
        
        Resolver->>Cache: Guardar configuraciÃ³n efectiva (TTL = 300s)
        Resolver-->>ConfigAPI: Retornar payload configuraciÃ³n efectiva
    end
    ConfigAPI-->>Client: 200 OK { effective_config }
```

### A. Flujo Principal
1. Un sistema cliente (ej., Portal SCM) se inicia y solicita su configuraciÃ³n desde la API de UMS, proporcionando su `system_id`, `tenant_id` y su contexto de ejecuciÃ³n (ej., `branch_id`, `environment`).
2. La API de ConfiguraciÃ³n verifica en Redis si existe una configuraciÃ³n efectiva pre-calculada que coincida exactamente con este hash de contexto.
3. En caso de fallo de cachÃ©, se invoca la Estrategia de ResoluciÃ³n. Esta consulta la base de datos por todas las capas de configuraciÃ³n disponibles para dicho contexto.
4. El Motor realiza una **FusiÃ³n Profunda (Deep Merge)** comenzando desde la capa de menor prioridad (Global) y aplicando las sobrescrituras secuencialmente hasta llegar a la capa de mayor prioridad (Entorno).
5. El objeto final calculado se guarda en cachÃ© con un Tiempo de Vida (TTL) de 5 minutos.
6. El sistema cliente recibe el JSON con la configuraciÃ³n final y ajusta su comportamiento (ej., oculta el botÃ³n de MFA si `mfa_enabled=false` a nivel de Sistema, aunque estÃ© habilitado a nivel Tenant).

---

## âš™ï¸ 3. LÃ³gica de Precedencia de ResoluciÃ³n

La funciÃ³n de FusiÃ³n Profunda sigue esta precedencia estricta (Prioridad 1 sobreescribe a Prioridad 7):

1. **Nivel Entorno**: Restricciones dictadas por infraestructura (ej., `PROD` obliga a cookies seguras).
2. **Nivel Usuario**: PersonalizaciÃ³n en extremo (ej., `user_id_123` sobreescribe el tema visual).
3. **Nivel Rol**: Ajustes de configuraciÃ³n segÃºn el Perfil asignado.
4. **Nivel Sede (Branch)**: Sobrescrituras especÃ­ficas a una sede fÃ­sica (ej., `Terminal Callao` fuerza el IdP local).
5. **Nivel Sistema**: Sobrescrituras de aplicaciÃ³n (ej., `TMS` frente a `WMS`).
6. **Nivel Inquilino (Tenant)**: LÃ­nea base de toda la organizaciÃ³n (ej., `LogisticsCorp`).
7. **Nivel Global**: Valores predeterminados en duro del UMS.

### Ejemplo de FusiÃ³n Profunda (Deep Merge)

**ConfiguraciÃ³n Nivel Inquilino (Tenant):**
```json
{ "auth": { "mfa_enabled": true, "session_timeout": 3600 }, "branding": { "color": "#000" } }
```

**ConfiguraciÃ³n Nivel Sistema (Override):**
```json
{ "auth": { "mfa_enabled": false }, "modules_enabled": ["tracking"] }
```

**ConfiguraciÃ³n Efectiva Resultante:**
```json
{
  "auth": { "mfa_enabled": false, "session_timeout": 3600 },
  "branding": { "color": "#000" },
  "modules_enabled": ["tracking"]
}
```

---

## ðŸ›¡ï¸ 4. Manejo de Excepciones

### Flujo Alternativo A: ConfiguraciÃ³n Base Ausente
- Si no existe configuraciÃ³n para el Tenant o Sistema solicitado, el resolutor recae elegantemente al Nivel Global Predeterminado. No retorna error 404, asegurando que el sistema cliente reciba valores de respaldo seguros para seguir operando.

### Flujo Alternativo B: Error de Sintaxis durante FusiÃ³n
- Si un override JSON personalizado contiene sintaxis invÃ¡lida que rompe el proceso de fusiÃ³n profunda, el motor registra un error en el Contexto de AuditorÃ­a y omite esa capa especÃ­fica, continuando con las capas de menor prioridad.
