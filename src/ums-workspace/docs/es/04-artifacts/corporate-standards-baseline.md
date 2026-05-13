# Línea Base de Estándares Corporativos — Proyecto UMS

- **Estado:** Referencia de Planificación
- **Fecha:** 2026-05-13
- **Fuente:** arc32_progresive_monolith (v1.0)

---

## Propósito

Este documento mapea cada Registro de Decisión Arquitectónica (ADR) del proyecto UMS contra su contraparte en la [Arquitectura de Referencia Corporativa (arc32)](https://github.com/beyondnetcode/arc32_progresive_monolith). Clasifica cada uno en tres categorías y define qué documentación es genuinamente local vs. heredable por referencia.

---

## 1. Leyenda de Clasificación

| Categoría | Distintivo | Significado | Acción |
|---|---|---|---|
| **BY_REFERENCE** | `[⬆️]` | El ADR de UMS es una adopción textual de un estándar corporativo. Sin matiz específico del proyecto. | Reducir el ADR de UMS a un stub que enlace a arc32. Mantener solo en `corporate-standards-baseline.md`. |
| **ADAPTED** | `[🔧]` | El ADR de UMS adopta el estándar corporativo pero lo modifica para el runtime .NET o necesidades específicas del dominio. | Mantener el ADR de UMS pero añadir cabecera `Based-on: arc32-ADR-NNN`. Documentar solo el delta. |
| **LOCAL** | `[📌]` | La decisión es específica del dominio UMS. No existe contraparte corporativa. | Mantener el ADR de UMS completo. |

---

## 2. Mapeo Completo de ADRs

### Tier 0: Núcleo Corporativo Universal (Independiente del Runtime)

| ADR UMS | Título | Fuente arc32 | Clasificación | Justificación |
|---|---|---|---|---|
| 0001 | Orquestación Monorepo (Nx) | arc32-0001 (core) | `[⬆️]` BY_REFERENCE | Misma herramienta, misma topología. La configuración de Nx es estándar. |
| 0005 | CI/CD Quality (CodeQL) | arc32-0005 (core) | `[⬆️]` BY_REFERENCE | La configuración del pipeline es estándar corporativo textual. |
| 0006 | Microservicios Futuros (Dapr) | arc32-0006 (core) | `[⬆️]` BY_REFERENCE | Dirección estratégica, sin desviación específica del proyecto. |
| 0009 | Strict Dependency Pinning | arc32-0009 (core) | `[⬆️]` BY_REFERENCE | Política de seguridad, adopción textual. |
| 0010 | Multi-Tenancy RLS Strategy | arc32-0010 (core) | `[🔧]` ADAPTED | Corporativo define híbrido agrupado. UMS añade modelo jerárquico (ADR-0034) y closure table. Mantener como adaptación. |
| 0011 | Tolerancia a Fallos y Resiliencia | arc32-0011 (core) | `[⬆️]` BY_REFERENCE | Patrones circuit breaker / retry son estándar. |
| 0013 | Topología Cloud y DR | arc32-0013 (core) | `[⬆️]` BY_REFERENCE | La estrategia de infraestructura es mandato corporativo. |
| 0014 | Caché Distribuida (Redis) | arc32-0014 (core) | `[⬆️]` BY_REFERENCE | La topología de caché es estándar. |
| 0015 | Arquitectura Orientada a Eventos | arc32-0015 (core) | `[⬆️]` BY_REFERENCE | El patrón de bus de eventos es estándar. |
| 0016 | Traza de Auditoría Inmutable | arc32-0016 (core) | `[🔧]` ADAPTED | Corporativo define row-level + ledger. UMS añade contexto jerárquico. Mantener como adaptación. |
| 0017 | Estrategia de Feature Flags | arc32-0017 (core) | `[⬆️]` BY_REFERENCE | El modelo de evaluación de flags es estándar. |
| 0018 | Testing Pyramid Quality Gates | arc32-0018 (core) | `[⬆️]` BY_REFERENCE | Los umbrales de cobertura son mandato corporativo. |
| 0019 | Patrones de Diseño Tácticos | arc32-0019 (core) | `[⬆️]` BY_REFERENCE | Result Pattern, Null Object, etc. son estándar. |
| 0020 | Estrategia de Abstracción IdP | arc32-0020 (core) | `[⬆️]` BY_REFERENCE | El patrón de adaptador IdP es estándar corporativo. |
| 0024 | Config & Feature Management | arc32-0024 (core) | `[⬆️]` BY_REFERENCE | La estrategia de plataforma de configuración es corporativa. |
| 0025 | Abstracción de Feature Flags | arc32-0025 (core) | `[⬆️]` BY_REFERENCE | La abstracción del proveedor es estándar. |
| 0028 | Infraestructura OSS Auto-gestionada | arc32-0028 (core) | `[⬆️]` BY_REFERENCE | La topología de despliegue es mandato corporativo. |
| 0030 | API Gateway (Kong) | arc32-0030 (core) | `[⬆️]` BY_REFERENCE | La selección de gateway es corporativa. |

### Tier 1: Específicos del Runtime (Node.js/.NET/React)

| ADR UMS | Título | Fuente arc32 | Clasificación | Justificación |
|---|---|---|---|---|
| **0002** | Clean Architecture | arc32-0002 (nodejs) | `[🔧]` ADAPTED | arc32-0002 define arquitectura NestJS. UMS usa .NET. Los principios son idénticos pero la implementación difiere. Mantener como referencia de adaptación .NET. |
| **0003** | Strict TypeScript Standards | arc32-0003 (nodejs) | `[🔧]` ADAPTED | arc32-0003 cubre Node.js/TS. UMS usa TS solo en frontend (React). Backend es C#. Mantener restringido al alcance del frontend. |
| **0004** | Frontend Offline Resilience | arc32-0004 (nodejs) | `[⬆️]` BY_REFERENCE | La gestión de estado React (Zustand + TanStack Query) coincide con el estándar corporativo para frontend. |
| **0007** | Observabilidad (OTel/Grafana) | arc32-0007 (nodejs) | `[🔧]` ADAPTED | Corporativo define OTel Node.js. UMS usa SDK .NET OpenTelemetry. Mismo protocolo (OTLP), diferente SDK. |
| **0008** | Evolución Progresiva BFF | arc32-0008 (nodejs) | `[🔧]` ADAPTED | Corporativo define BFF NestJS. UMS usa .NET 8 para BFF. Mismo patrón, diferente runtime. |
| **0012** | Autorización RBAC/ABAC | arc32-0012 (nodejs) | `[🔧]` ADAPTED | Corporativo define Guards + Decorators NestJS. UMS implementa via middleware .NET (ADR-0036, 0039). Fusionado en ADRs locales. |
| **0021** | Compilación de Grafo de Auth | arc32-0021 (nodejs) | `[🔧]` ADAPTED | Fusionado en ADR-0039 (Policy Compilation Engine). Implementación específica de UMS. |
| **0022** | Proyecciones Contextuales | arc32-0022 (nodejs) | `[🔧]` ADAPTED | Mismo concepto, implementación .NET. |
| **0023** | Límite del Kernel Centralizado | arc32-0023 (nodejs) | `[🔧]` ADAPTED | Diseño de dominio específico de UMS, pero sigue el principio corporativo de kernel centralizado. |
| **0026** | MFA Adaptive Auth | arc32-0026 (nodejs) | `[⬆️]` BY_REFERENCE | La estrategia MFA es independiente del runtime. Aplican las mismas políticas. |
| **0027** | Protocolo Dual REST/gRPC | arc32-0027 (nodejs) | `[🔧]` ADAPTED | Corporativo define configuración gRPC Node.js. UMS usa .NET gRPC (Grpc.AspNetCore). |
| **0029** | Primitivas DDD Tácticas | arc32-0029 (nodejs) | `[🔧]` ADAPTED | Corporativo define primitivas DDD en TS. UMS usa records de C#, value objects. |
| **0033** | Minimal APIs (.NET) | arc32-0048 (dotnet) | `[⬆️]` BY_REFERENCE | Contraparte directa .NET. Estrategia idéntica. |

### Tier 2: Decisiones Locales del Dominio UMS

| ADR UMS | Título | Fuente arc32 | Clasificación | Justificación |
|---|---|---|---|---|
| **0031** | Abstracción de Dominio de Identidad (Subject) | arc32-0031 (core, Schema Per Context) | `[📌]` LOCAL | arc32-0031 define schema-per-context. UMS-0031 define Subject como abstracción de identidad. Relacionado conceptualmente pero específico del dominio. Mantener. |
| **0032** | Organización como Límite Estratégico | — | `[📌]` LOCAL | Diseño de dominio específico de UMS. Sin equivalente corporativo. Define la Organización como raíz de gobernanza. Mantener. |
| **0034** | Multi-Inquilino Jerárquico (Closure + Taxonomy) | — | `[📌]` LOCAL | Elección de implementación específica del dominio. Corporativo define estrategia RLS; UMS define la extensión jerárquica. Mantener. |
| **0035** | Motor de Herencia de Políticas Híbrido | — | `[📌]` LOCAL | Herencia de 4 modos (MANDATORY/DEFAULT/OPT_IN/NONE) es específica de UMS. Mantener. |
| **0036** | Anti-Escalada de Privilegios | — | `[📌]` LOCAL | Pipeline de 5 invariantes es específico de UMS. Mantener. |
| **0037** | Particionamiento por Inquilino | arc32-0031 (core) | `[📌]` LOCAL | Relacionado con schema-per-concept corporativo pero implementación específica de UMS (particionamiento LIST por root_tenant_id). Mantener. |
| **0038** | Administración Delegada (Alcances Temporales) | — | `[📌]` LOCAL | El modelo DAG de delegación es específico de UMS. Sin equivalente corporativo. Mantener. |
| **0039** | Compilación de Políticas RBAC/ABAC | arc32-0021 (nodejs) | `[📌]` LOCAL | El diseño del motor de compilación de políticas es específico de UMS. Corporativo define el concepto pero no la implementación. Mantener. |
| **0040** | Estrategia de Token Federado | — | `[📌]` LOCAL | Token de modo dual (JWT + Opaco) es específico de UMS para contexto jerárquico. Sin equivalente corporativo. Mantener. |

---

## 3. Plan de Destricción / Consolidación

### Fase 1: Reducir 18 ADRs BY_REFERENCE a Stubs

Los siguientes ADRs de UMS deben reducirse a un stub de una página que contenga solo:

```markdown
# ADR-NNN: [Título]

* **Estado:** Aceptado (Incorporado por Referencia)
* **Fuente Corporativa:** [arc32-ADR-NNN](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/...)

## Decisión

Este proyecto adopta el estándar corporativo textualmente según lo definido en la fuente anterior. No se requiere adaptación específica del proyecto.

## Notas Específicas del Proyecto

- Detalles de implementación: ver `docs/es/04-artifacts/corporate-standards-baseline.md`
- Seguimiento de desviaciones: cualquier desviación futura DEBE registrarse como un nuevo ADR LOCAL que referencie este.
```

**ADRs a convertir en stub:**
0001, 0005, 0006, 0009, 0011, 0013, 0014, 0015, 0017, 0018, 0019, 0020, 0024, 0025, 0028, 0030, 0004, 0033

### Fase 2: Condensar 13 ADRs ADAPTED a Solo Delta

Los siguientes ADRs de UMS mantienen su archivo pero eliminan todo el contenido que duplica la fuente corporativa. Solo el delta (adaptación .NET, matiz específico de UMS) permanece.

**ADRs a condensar:**
0002, 0003, 0007, 0008, 0010, 0012, 0016, 0021, 0022, 0023, 0026, 0027, 0029

### Fase 3: Mantener 9 ADRs LOCALES Completos

0031, 0032, 0034, 0035, 0036, 0037, 0038, 0039, 0040

---

## 4. Inventario de Documentación Resultante

| Estado | Cantidad | Archivos |
|---|---|---|
| `[⬆️]` BY_REFERENCE (stub) | 18 | 0001, 0004, 0005, 0006, 0009, 0011, 0013, 0014, 0015, 0017, 0018, 0019, 0020, 0024, 0025, 0028, 0030, 0033 |
| `[🔧]` ADAPTED (delta) | 13 | 0002, 0003, 0007, 0008, 0010, 0012, 0016, 0021, 0022, 0023, 0026, 0027, 0029 |
| `[📌]` LOCAL (completo) | 9 | 0031, 0032, 0034, 0035, 0036, 0037, 0038, 0039, 0040 |
| **Total ADRs UMS** | **40** | Después de la consolidación |

---

## 5. Reglas de Gobernanza (En Adelante)

### Regla G1 — Jerarquía de Fuente de Verdad

```
arc32 (Referencia Corporativa)
  └── Cumplimiento obligatorio: TODOS los BY_REFERENCE
  └── Cumplimiento por defecto: TODOS los ADAPTED (solo desviaciones delta deben ser aprobadas)
       └── ADRs del Proyecto UMS
            └── Los ADRs LOCALES son vinculantes solo para UMS
            └── Si un ADR LOCAL luego se vuelve corporativo, debe promoverse a arc32
```

### Regla G2 — Creación de Nuevos ADRs

Antes de crear cualquier nuevo ADR de UMS:
1. Verificar si arc32 tiene un ADR corporativo relevante.
2. Si SÍ y UMS lo sigue textualmente → declarar `BY_REFERENCE`, NO crear un nuevo ADR de UMS.
3. Si SÍ pero UMS necesita adaptación → crear ADR ADAPTED con cabecera `Based-on:` y contenido solo delta.
4. Si NO → crear ADR LOCAL, considerar si debería proponerse a arc32.

### Regla G3 — Actualizaciones de la Referencia Corporativa

Cuando arc32 actualice un ADR que UMS referencie:
- `BY_REFERENCE`: Sin acción necesaria (la referencia está viva). Informar al equipo del cambio.
- `ADAPTED`: Revisar si el delta de adaptación sigue siendo válido. Actualizar si es necesario.
- `LOCAL`: No afectado.

### Regla G4 — Documentación vs. Código

- Este documento de línea base y los stubs/deltas de ADR son **gobernanza de diseño**, no código.
- La implementación real (código en `src/ums-workspace/apps/`) se rige por estas decisiones pero NO es documentación duplicada.
- Si una decisión de implementación cambia, el ADR DEBE actualizarse PRIMERO (gobernanza ADR-first).

---

## 6. Próximos Pasos

| Paso | Responsable | Esfuerzo |
|---|---|---|
| 1. Aprobar esta clasificación (PO + Arquitecto) | PO + Arquitecto | 1 sesión |
| 2. Convertir en stub 18 ADRs BY_REFERENCE | Tech Writer / AI | 1 día |
| 3. Condensar 13 ADRs ADAPTED a solo delta | Tech Writer / AI | 2 días |
| 4. Actualizar MASTER_INDEX.md para reflejar nuevo inventario de ADRs | Tech Writer | 0.5 día |
| 5. Establecer cadencia de sincronización con releases de arc32 | Arquitecto | Continuo |
