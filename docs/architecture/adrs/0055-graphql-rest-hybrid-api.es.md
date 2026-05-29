# ADR-0055: Patrón de API Híbrida GraphQL/REST

| Campo | Valor |
|---|---|
| **Estado** | Aceptado |
| **Fecha** | 2026-05-21 |
| **Contexto** | UMS Web App — Estrategia de Comunicación de API |
| **Decisores** | Equipo de Arquitectura |

## Problema

El sistema UMS necesita soportar tanto consulta flexible de datos (con relaciones anidadas, filtrado, y selección de campos) como semántica clara de comandos transaccionales. Usar un único patrón de API para ambos lleva a either over-fetching (REST) o semántica de mutación poco clara (GraphQL).

## Decisión

Adoptar un patrón híbrido de **GraphQL para Queries, REST para Commands**:

- **GraphQL (HotChocolate)**: Todas las operaciones de lectura (queries). Los clientes solicitan exactamente los campos que necesitan, con relaciones anidadas en un único round-trip.
- **REST Minimal APIs**: Todas las operaciones de escritura (commands/transacciones). Semántica HTTP clara (POST, PUT, DELETE) con códigos de estado explícitos y garantías de idempotencia.

### Implementación Lado Cliente

```
Frontend (React)
├── GraphQL Client (graphql-request v7)
│   ├── Todas las queries usan URL absoluta: `${window.location.origin}/graphql`
│   ├── Queries tipadas generadas desde schema
│   └── Cacheado vía TanStack Query
│
└── REST Client (Axios vía httpClient.ts)
    ├── Todas las mutaciones (POST, PUT, DELETE)
    ├── Inyección de token CSRF para requests que cambian estado
    ├── Headers de dev (X-User-Id, X-Language, X-Tenant-Id)
    └── Normalización de errores vía interceptors
```

### Justificación

1. **Flexibilidad de query**: GraphQL permite al frontend solicitar exactamente lo que cada pantalla necesita sin over-fetching ni requests N+1.
2. **Semántica transaccional clara**: REST provee códigos de estado HTTP bien entendidos, claves de idempotencia, y semántica de retry para mutaciones.
3. **Protección CSRF**: Los endpoints REST están naturalmente protegidos vía tokens CSRF; las subscriptions/queries de GraphQL son solo lectura y CSRF-safe.
4. **Integración TanStack Query**: Las queries de GraphQL cachean naturalmente con el sistema de query keys de TanStack Query.

### Consecuencias

**Positivas:**
- Tamaños de payload reducidos (selección de campos)
- Single round-trip para datos anidados
- Separación clara de concerns de lectura vs escritura
- Frontera natural de protección CSRF

**Negativas:**
- Dos clientes de API que mantener
- Los desarrolladores deben saber qué patrón usar
- Se necesita configuración de proxy de Vite para ambos `/api` y `/graphql`

## Implementación

- `src/infrastructure/http/httpClient.ts` — Instancia Axios para comandos REST
- `src/infrastructure/http/graphqlClient.ts` — Cliente GraphQL para queries
- `src/infrastructure/http/csrf.ts` — Gestión de token CSRF
- `vite.config.ts` — Proxy `/api` y `/graphql` al backend

## Alternativas Consideradas

### Alternativa 1: Niveles de API Separados (Query Tier + Command Tier)

Separar queries y commands en dos servicios de API desplegados independientemente — un servicio GraphQL dedicado para queries y un servicio REST dedicado para commands.

**Rechazada porque:**

- UMS es un modular monolith. Dividir en niveles de despliegue antes de que se cumplan los criterios de extracción viola ADR-0054 (Aislamiento de Shell Libraries) y el playbook de evolución del modular monolith.
- CQRS separa *modelos* de lectura y escritura, no *unidades de despliegue*. La separación existente ya está enforce a tres niveles: protocolo (GraphQL vs REST), código (handlers distintos, clientes distintos), y routing (`/graphql` vs `/api/v1/...`).
- El costo operacional se duplica: dos Dockerfiles, dos health checks, dos políticas de scaling, dos sets de connection pools — sin beneficio medible en la carga actual.

**Cuándo esta decisión debe revisarse:**

Esta alternativa se vuelve válida cuando se cumpla cualquiera de las siguientes condiciones:

| Trigger | Explicación |
|---|---|
| Throughput de lectura consistentemente 10x el de escritura | Scaling horizontal independiente del query tier se justifica |
| Equipos separados poseen las superficies de query vs command | La Ley de Conway hace la división natural, no forzada |
| Se inicia migración hacia microservicios | La separación de niveles es un paso prerequisito |
| Requerimientos tecnológicos incompatibles emergen | ej., query tier necesita un runtime o estrategia de caching diferente |

**Consideración específica para SaaS — aislamiento de carga de tenants:**

En un contexto SaaS multi-tenant, queries GraphQL pesadas de un tenant grande podrían impactar la latencia de commands (login, provisioning) si ambos comparten el mismo proceso. Este riesgo se mitiga en la arquitectura actual por:

1. Límites de complejidad de query de GraphQL enforce a nivel de schema HotChocolate.
2. Timeouts diferenciados por tipo de operación.
3. Rate limiting por tenant a nivel de API Gateway (ver [TE-07: YARP API Gateway](../blueprints/technical-enablers/te-07-yarp-api-gateway.md)).

La separación de niveles permanece como el camino de escalada correcto si estos controles prueban ser insuficientes a escala.

---

### Alternativa 2: GraphQL para Queries y Mutaciones

Usar GraphQL exclusivamente — queries y mutaciones — eliminando la capa REST.

**Rechazada porque:**

- La semántica de mutación de GraphQL no mapea limpiamente a las convenciones de idempotencia, retry, y código de estado HTTP requeridas para operaciones de comando.
- La protección CSRF requiere manejo explícito para mutaciones GraphQL; los endpoints REST POST/PUT/DELETE obtienen esta frontera naturalmente.
- REST es el estándar establecido para webhooks callbacks, integraciones externas, y clientes móviles que pueden no soportar un cliente GraphQL.

---

## Relacionados

- ADR-0056: Gestión de Estado con Zustand + TanStack Query
- ADR-0058: Evolución de API Gateway — YARP para SaaS Multi-Cliente
- ADR-0007 de Evolith: Patrón de API Gateway
- [TE-07: YARP API Gateway](../blueprints/technical-enablers/te-07-yarp-api-gateway.md)