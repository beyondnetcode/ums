# UMS — Propuesta de Diseño UX/UI

**Rol:** Lead UX/UI Designer & Product Designer Senior  
**Versión:** 1.0 — 2026-05-15  
**Audiencia:** Equipo frontend, Product Owner, Tech Lead  
**Estado:** Propuesta lista para revisión y validación

---

## Tabla de Contenidos

1. [Resumen UX del Producto](#1-resumen-ux-del-producto)
2. [Personas y Objetivos de Usuario](#2-personas-y-objetivos-de-usuario)
3. [Mapa de Módulos Principales](#3-mapa-de-módulos-principales)
4. [Flujos Críticos de Usuario](#4-flujos-críticos-de-usuario)
5. [Arquitectura de Información](#5-arquitectura-de-información)
6. [Navegación Propuesta](#6-navegación-propuesta)
7. [Wireframes Descriptivos](#7-wireframes-descriptivos)
8. [Componentes UI Recomendados](#8-componentes-ui-recomendados)
9. [Estados de Pantalla](#9-estados-de-pantalla)
10. [Reglas de Accesibilidad y Usabilidad](#10-reglas-de-accesibilidad-y-usabilidad)
11. [Propuesta Responsive](#11-propuesta-responsive)
12. [Priorización MVP vs Futuras Mejoras](#12-priorización-mvp-vs-futuras-mejoras)
13. [Riesgos UX y Recomendaciones](#13-riesgos-ux-y-recomendaciones)
14. [Supuestos y Preguntas Abiertas](#14-supuestos-y-preguntas-abiertas)

---

## 1. Resumen UX del Producto

### Naturaleza del sistema

UMS es un **portal de administración de identidades y autorización** de clase enterprise (B2B SaaS multi-tenant). No es un producto de consumo masivo: sus usuarios son administradores IT, analistas de seguridad, aprobadores de accesos y —marginalmente— usuarios finales B2B que completan formularios de onboarding.

La experiencia de usuario tiene tres capas funcionalmente distintas con audiencias distintas:

| Capa | Interfaz | Audiencia principal |
|------|----------|---------------------|
| **PAP — Policy Administration Point** | Portal administrativo complejo | Admins de plataforma, admins de tenant |
| **Portal de aprobaciones** | Bandeja de tareas + formularios de decisión | Aprobadores, sponsors |
| **Login Portal** | Pantalla de autenticación personalizable por tenant | Usuarios finales B2B |

### Principio de diseño central

> **Complejidad bajo la superficie, claridad en la superficie.**

UMS gestiona conceptos técnicamente densos (grafos de permisos, jerarquías de roles, templates, RLS). El diseño debe **esconder la complejidad técnica** tras interfaces orientadas a tareas de negocio: "Aprobar acceso de este usuario", "Ver qué permisos tiene esta persona", "Bloquear acceso por documento vencido".

### Tono y estilo visual

- Enterprise, profesional, sobrio — no consumer/playful
- Alta densidad de información con estructura clara (no minimalismo extremo)
- Color primario neutro (azul corporativo profundo), acentos de estado (verde éxito, ámbar advertencia, rojo peligro)
- Tipografía: sistema sans-serif (Inter o equivalente), monoespacio para IDs y JSONs técnicos

---

## 2. Personas y Objetivos de Usuario

### Persona 1 — Platform Super Admin

**Contexto:** Arquitecto o DevOps engineer del equipo que opera UMS como plataforma. Acceso ilimitado. Responsable de registrar sistemas cliente, configurar IdPs globales y monitorear la salud de la plataforma.

**Objetivos primarios:**
- Registrar y configurar nuevos sistemas cliente (Suite → Módulos → Menús → Acciones)
- Gestionar la jerarquía root de tenants
- Monitorear métricas de autenticación, errores y cache

**Frustraciones frecuentes:**
- Interfaces que no muestran el impacto en cascada de un cambio de configuración
- Falta de un visualizador del grafo de permisos compilado en tiempo real

**Frecuencia de uso:** Diaria pero en sesiones cortas (configuración incremental)

---

### Persona 2 — Tenant Admin (IT Admin del cliente B2B)

**Contexto:** Responsable IT de una empresa cliente (ej: Logistics Corp). Gestiona usuarios de su organización, asigna perfiles, aprueba solicitudes de acceso y revisa documentos de cumplimiento.

**Objetivos primarios:**
- Ver y gestionar sus usuarios con sus estados (ACTIVE / BLOCKED / PENDING)
- Asignar y revocar perfiles de autorización
- Revisar y aprobar solicitudes de acceso externo (B2B)
- Controlar el cumplimiento documental de su equipo

**Frustraciones frecuentes:**
- "¿Por qué este usuario no puede ver este módulo?" — necesita diagnóstico de permisos sin ayuda técnica
- Navegar jerarquías de tenant complejas (múltiples sedes/ramas)

**Frecuencia de uso:** Alta, 2-4 veces por día

---

### Persona 3 — Aprobador (Approver)

**Contexto:** Manager o Jefe de área que recibe solicitudes de aprobación: onboarding de nuevo usuario, solicitud de acceso externo, promoción de rol. No es técnico; opera principalmente desde la bandeja de aprobaciones.

**Objetivos primarios:**
- Ver solicitudes pendientes con contexto suficiente para decidir
- Aprobar, rechazar o escalar en el menor número de clics posible
- Delegar aprobaciones cuando está ausente

**Frustraciones frecuentes:**
- Recibir notificaciones sin suficiente contexto (¿quién pidió, qué acceso, por qué?)
- No saber si una solicitud está a punto de vencer (SLA)

**Frecuencia de uso:** Esporádica, picos en momentos de onboarding masivo

---

### Persona 4 — Sponsor User (Usuario Patrocinador)

**Contexto:** Empleado interno que solicita acceso externo para un contratista o partner B2B. Inicia el flujo de External Access Request.

**Objetivos primarios:**
- Iniciar una solicitud de acceso para un usuario externo con justificación clara
- Hacer seguimiento del estado de la solicitud
- Adjuntar documentos de soporte

**Frecuencia de uso:** Baja, pero de alto impacto cuando ocurre

---

### Persona 5 — Compliance Officer / IGA Analyst

**Contexto:** Analista de seguridad o cumplimiento que supervisa el estado documental de todos los usuarios, la validez de los accesos y el proceso de promoción de roles.

**Objetivos primarios:**
- Ver el cuadro de mando de cumplimiento documental (documentos próximos a vencer, vencidos)
- Validar documentos subidos por usuarios
- Supervisar procesos de promoción de rol en curso
- Generar reportes de auditoría

**Frecuencia de uso:** Diaria, monitoreo continuo

---

### Persona 6 — Usuario Final B2B (End User)

**Contexto:** Empleado de empresa cliente (ej: operador de montacargas en Logistics Corp) que debe autenticarse y, en ocasiones, subir documentos o completar su perfil de onboarding.

**Objetivos primarios:**
- Autenticarse con el mínimo de fricción (Passkey, SSO o Magic Link)
- Subir documentos requeridos por su empleador
- Ver el estado de su solicitud de acceso

**Frecuencia de uso:** Puntual (onboarding) + recurrente (login diario)

---

## 3. Mapa de Módulos Principales

```
UMS Portal
├── Dashboard Global
│
├── IDENTITY (Identidad)
│   ├── Tenants (Organizaciones)
│   │   ├── Árbol jerárquico de tenants
│   │   └── Detalle de tenant + configuración IdP
│   ├── Usuarios
│   │   ├── Lista filtrable de usuarios
│   │   ├── Detalle de usuario (perfil, estado, documentos, permisos)
│   │   └── Registro de nuevo usuario
│   └── Sedes (Branches)
│
├── AUTHORIZATION (Autorización)
│   ├── Sistemas (System Suites)
│   │   ├── Lista de sistemas registrados
│   │   └── Topología: Módulos → Menús → Opciones → Acciones
│   ├── Roles
│   │   ├── Jerarquía de roles por sistema
│   │   └── Criterios de promoción (IGA)
│   ├── Templates de Permisos
│   │   ├── Lista + estado (DRAFT / PUBLISHED / DEPRECATED)
│   │   └── Editor de permisos del template
│   └── Perfiles
│       ├── Lista de perfiles activos
│       └── Asignación de templates a perfiles
│
├── APPROVALS (Aprobaciones)
│   ├── Bandeja de Aprobaciones (Inbox)
│   │   ├── Pendientes (con indicador SLA)
│   │   ├── Historial (aprobadas / rechazadas / vencidas)
│   │   └── Detalle de solicitud + formulario de decisión
│   └── Configuración de Workflows
│       └── Definición de flujos, aprobadores, SLA
│
├── IGA (Governance)
│   ├── Procesos de Promoción
│   │   ├── Lista de procesos activos por estado
│   │   └── Detalle: criterios cumplidos / pendientes
│   └── Delegaciones
│       ├── Delegaciones activas
│       └── Crear / revocar delegación
│
├── COMPLIANCE (Cumplimiento)
│   ├── Dashboard de Cumplimiento
│   │   ├── Documentos próximos a vencer
│   │   └── Usuarios con documentos críticos vencidos
│   ├── Tipos de Documento
│   │   ├── Catálogo con criticidad y políticas
│   │   └── Reglas de notificación
│   ├── Documentos de Usuario
│   │   ├── Bandeja de validación (PENDING_REVIEW)
│   │   └── Historial de documentos por usuario
│   └── Políticas de Acceso por Expiración
│
├── CONFIGURATION (Configuración)
│   ├── Parámetros del Sistema (jerarquía ENV > SYSTEM > TENANT)
│   ├── Feature Flags
│   │   └── Toggles con targeting multi-dimensional
│   └── Configuración IdP (por tenant)
│
├── AUDIT (Auditoría)
│   ├── Log de Eventos de Dominio
│   ├── Log de Eventos de Seguridad
│   └── Cambios de Permisos
│
└── HERRAMIENTAS
    ├── Visualizador del Grafo de Permisos
    └── Monitor de Sesiones Activas
```

---

## 4. Flujos Críticos de Usuario

### Flujo 1 — Onboarding de Usuario Interno (FS-01, FS-05, FS-06)

```
[Admin de Tenant]
       │
       ▼
  Dashboard → Usuarios → Nuevo Usuario
       │
       ▼
  Formulario: nombre, email, categoría (INTERNAL), sede
       │
       ▼
  Sistema asigna: status=PENDING
  Auto-asignación de template si aplica regla (FS-06)
       │
       ├─ [SIN workflow de aprobación] → status=ACTIVE automáticamente
       │
       └─ [CON workflow de aprobación] → Notificación al aprobador
                    │
                    ▼
           Bandeja del Aprobador → Revisar → Aprobar
                    │
                    ▼
           status=ACTIVE → Notificación al usuario registrado
```

**Pantallas involucradas:** Dashboard, Lista de Usuarios, Formulario de Usuario, Detalle de Usuario, Bandeja de Aprobaciones, Detalle de Solicitud

---

### Flujo 2 — Solicitud de Acceso Externo B2B (FS-10)

```
[Sponsor User]
       │
       ▼
  Aprobaciones → Nueva Solicitud → Tipo: B2B Access Request
       │
       ▼
  Formulario: datos del usuario externo, organización,
              justificación, sistema solicitado,
              documentos adjuntos requeridos
       │
       ▼
  Solicitud creada → status=PENDING
  Notificación al aprobador designado por workflow
       │
       ▼
  [Aprobador]
  Bandeja → Solicitud con risk_score visible
  → Revisar contexto, documentos, sponsor
  → Decisión: APPROVE / REJECT / ESCALATE / DELEGATE
       │
       ├─ APPROVED → Usuario B2B creado, perfil asignado, notificación
       └─ REJECTED → Notificación al sponsor con motivo
```

**Pantallas involucradas:** Bandeja de Aprobaciones, Formulario de Nueva Solicitud, Detalle de Solicitud con panel de decisión, Historial de Solicitudes

---

### Flujo 3 — Validación de Documento de Cumplimiento (FS-11, FS-16)

```
[Usuario Final]
       │
       ▼
  Mi Perfil → Documentos → Subir Documento
  (tipo, archivo, fecha emisión, fecha vencimiento)
       │
       ▼
  status=PENDING_REVIEW

[Compliance Officer]
       │
       ▼
  Compliance → Bandeja de Validación → Documento pendiente
  → Previsualización + metadatos
  → VALIDAR o RECHAZAR (con motivo)
       │
       ├─ VALID → Documento activo, acceso habilitado si era bloqueante
       └─ REJECTED → Notificación al usuario, debe resubir
              │
              ▼
       [Sistema — background worker cada hora]
       Si fecha_vencimiento <= hoy:
         status=EXPIRED →
           EnforcementAction según política:
             BLOCK_ACCESS / NOTIFY_ONLY / DOWNGRADE_ROLE / SUSPEND
```

**Pantallas involucradas:** Mi Perfil (sección Documentos), Bandeja de Validación de Documentos, Dashboard de Cumplimiento (alerta de expiración)

---

### Flujo 4 — Diagnóstico de Permisos (FS-07)

```
[Admin / Compliance Officer]
       │
       ▼
  Herramientas → Visualizador de Grafo de Permisos
       │
       ▼
  Seleccionar: Usuario + Sistema + [Sede opcional]
       │
       ▼
  Visualización del grafo compilado:
  - Perfiles activos del usuario
  - Templates vinculados a cada perfil
  - ALLOW / DENY por acción (con fuente del permiso)
  - Regla de Deny Dominance destacada si aplica
       │
       ▼
  [Opcional] Exportar reporte en JSON / CSV
```

**Pantallas involucradas:** Herramientas → Visualizador de Grafo

---

### Flujo 5 — Proceso de Promoción de Rol IGA (FS-12)

```
[Sistema — background worker continuo]
  Evaluando: UserPromotionProcess.status=EVALUATING
  Verifica flags: seniority, compliance docs, business score
       │
       ├─ No cumplido → continúa EVALUATING (visible en IGA)
       └─ Cumplido → status=CRITERIA_MET → Crea ApprovalRequest
              │
[Aprobador IGA]
              ▼
  Bandeja → Solicitud de tipo ROLE_PROMOTION
  → Ver: usuario, rol actual, rol destino, criterios cumplidos
  → APPROVE / REJECT
              │
              ├─ APPROVED → status=PROMOTED, perfil actualizado
              └─ REJECTED → status=EVALUATING (reinicia ciclo)
```

**Pantallas involucradas:** IGA → Procesos de Promoción (lista + detalle), Bandeja de Aprobaciones

---

### Flujo 6 — Login Portal Personalizado (FS-08, FS-09)

```
[Usuario Final B2B]
       │
       ▼
  Downstream App → Redirect a UMS Login Portal
  (URL incluye: tenant_id, system_id, redirect_uri)
       │
       ▼
  Login Portal muestra:
  - Logo y branding del tenant
  - Métodos de auth disponibles:
      • Passkey / FIDO2
      • Magic Link
      • Email + MFA
      • SSO (si tenant tiene IdP externo)
       │
       ▼
  Sistema calcula risk_score (6 factores):
  score < 20 → login directo
  score 20-40 → MFA recomendado / requerido
  score > 70 → MFA + Security Review
       │
       ▼
  Auth exitosa → JWT firmado → Redirect a downstream app
```

**Pantallas involucradas:** Login Portal (pantalla única, altamente customizable por tenant)

---

## 5. Arquitectura de Información

### Jerarquía de navegación de 3 niveles

```
Nivel 1 (Módulo)    Nivel 2 (Sección)         Nivel 3 (Vista de detalle)
─────────────────   ──────────────────────     ──────────────────────────
Identity        →   Tenants              →      Detalle de Tenant
                    Usuarios             →      Detalle de Usuario
                    Sedes                →      Detalle de Sede

Authorization   →   Sistemas             →      Topología del Sistema
                    Roles                →      Detalle de Rol + Jerarquía
                    Templates            →      Editor de Template
                    Perfiles             →      Detalle de Perfil

Approvals       →   Bandeja              →      Detalle de Solicitud
                    Configuración        →      Editor de Workflow

IGA             →   Promociones          →      Detalle de Proceso
                    Delegaciones         →      Detalle de Delegación

Compliance      →   Dashboard            →      (vista de cuadro de mando)
                    Tipos de Documento   →      Detalle de Tipo
                    Documentos           →      Detalle de Documento
                    Políticas            →      Editor de Política

Configuration   →   Parámetros           →      Editor de Parámetro
                    Feature Flags        →      Editor de Flag
                    Configuración IdP    →      Editor de IdP

Audit           →   Eventos de Dominio   →      Detalle de Evento
                    Eventos de Seguridad →      Detalle de Evento
                    Cambios de Permisos  →      Detalle de Cambio

Herramientas    →   Grafo de Permisos    →      Vista del grafo
                    Monitor de Sesiones  →      Vista de sesiones
```

### Relaciones de entidad clave (para navegación contextual)

- **Usuario ↔ Tenants:** Un usuario pertenece a una organización; las organizaciones son tenants
- **Usuario ↔ Perfiles:** Un usuario puede tener múltiples perfiles por sede/rol
- **Perfil ↔ Templates:** Un perfil agrupa múltiples templates de permisos
- **Template ↔ Sistema:** Los templates referencian acciones de sistemas específicos
- **Solicitud ↔ Usuario:** Las solicitudes de aprobación están vinculadas a un usuario objetivo
- **Documento ↔ Usuario:** Los documentos pertenecen a usuarios y afectan su status de acceso

Navegación contextual requerida: al ver el detalle de un usuario, deben existir accesos rápidos a sus perfiles, sus documentos y sus solicitudes de aprobación pendientes sin salir de la vista.

---

## 6. Navegación Propuesta

### Shell principal (authenticated)

```
┌─────────────────────────────────────────────────────────────────┐
│  [UMS Logo]    [Tenant Switcher ▼]              [🔔 3] [Avatar ▼] │
├────────────┬────────────────────────────────────────────────────┤
│            │                                                    │
│  SIDEBAR   │              CONTENT AREA                         │
│            │                                                    │
│ ● Dashboard│                                                    │
│            │                                                    │
│ IDENTITY   │                                                    │
│  Tenants   │                                                    │
│  Usuarios  │                                                    │
│  Sedes     │                                                    │
│            │                                                    │
│ AUTHORIZ.  │                                                    │
│  Sistemas  │                                                    │
│  Roles     │                                                    │
│  Templates │                                                    │
│  Perfiles  │                                                    │
│            │                                                    │
│ APPROVALS  │                                                    │
│  Bandeja ⁽³⁾│                                                   │
│  Workflows │                                                    │
│            │                                                    │
│ IGA        │                                                    │
│  Promoc.   │                                                    │
│  Delegac.  │                                                    │
│            │                                                    │
│ COMPLIANCE │                                                    │
│  Dashboard │                                                    │
│  Documentos│                                                    │
│  Políticas │                                                    │
│            │                                                    │
│ CONFIG     │                                                    │
│  Parámetros│                                                    │
│  Feat. Flags│                                                   │
│  IdP       │                                                    │
│            │                                                    │
│ AUDIT      │                                                    │
│  Eventos   │                                                    │
│            │                                                    │
│ HERRAM.    │                                                    │
│  Grafo     │                                                    │
│  Sesiones  │                                                    │
│            │                                                    │
└────────────┴────────────────────────────────────────────────────┘
```

### Reglas de navegación

1. **Sidebar colapsable:** En pantallas < 1280px el sidebar colapsa a íconos únicamente
2. **Breadcrumbs obligatorios** en vistas de nivel 3 (detalle): `Identity > Usuarios > Maria García`
3. **Tenant Switcher** solo visible para Super Admin; los Tenant Admins ven únicamente su tenant
4. **Contador de pendientes** en "Bandeja" del sidebar actualizado en tiempo real (WebSocket o polling 30s)
5. **Notificaciones:** campana en header con dropdown de últimas 5 alertas (documentos vencidos, solicitudes SLA crítico)
6. **Perfil de usuario:** avatar en header → Mis datos, Mis delegaciones, Cerrar sesión

### Roles y visibilidad del sidebar

| Sección sidebar | Super Admin | Tenant Admin | Approver | Compliance | IGA Analyst |
|----------------|-------------|--------------|----------|------------|-------------|
| Identity       | ✅ completo  | ✅ su tenant  | ❌        | 👁 lectura  | 👁 lectura   |
| Authorization  | ✅ completo  | ✅ parcial    | ❌        | 👁 lectura  | 👁 lectura   |
| Approvals      | ✅           | ✅            | ✅         | ❌          | ✅           |
| IGA            | ✅           | ✅            | ❌         | ❌          | ✅           |
| Compliance     | ✅           | ✅            | ❌         | ✅          | 👁 lectura   |
| Configuration  | ✅           | ✅ parcial    | ❌         | ❌          | ❌           |
| Audit          | ✅           | ✅ su tenant  | ❌         | ✅          | 👁 lectura   |
| Herramientas   | ✅           | ✅            | ❌         | ✅          | ✅           |

---

## 7. Wireframes Descriptivos

### 7.1 — Dashboard Global

```
┌─────────────────────────────────────────────────────────────────┐
│  Dashboard                                       Hoy: 15 May 2026│
├───────────┬───────────┬───────────┬─────────────────────────────┤
│ USUARIOS  │ PENDIENTES│ DOCS      │  ALERTAS CRÍTICAS           │
│ ACTIVOS   │ APROBAR   │ POR VENCER│                             │
│           │           │           │  ⚠ 3 usuarios con docs      │
│  1,247    │     8     │    15     │  CRÍTICOS vencidos           │
│ ↑ 12 hoy  │ 2 SLA < 1h│ < 30 días │  [Ver Compliance]           │
│           │           │           │                             │
│ [Ver todos│[Ir Bandeja│[Ver Docs] │  ⚠ SLA vencido: Solicitud  │
│  usuarios]│  ]        │           │  #REQ-0041 — B2B Access     │
│           │           │           │  [Ir a solicitud]           │
└───────────┴───────────┴───────────┴─────────────────────────────┤
│                                                                  │
│  ACTIVIDAD RECIENTE                        SOLICITUDES PENDIENTES│
│  ─────────────────                         ──────────────────────│
│  • maria.garcia activada       hace 5m     #REQ-0041 B2B Access  │
│  • Template "Logistics" publ.  hace 1h       SLA: ⚠ vence en 1h  │
│  • juan.perez BLOCKED          hace 2h     #REQ-0039 Onboarding  │
│    (doc CRITICAL vencido)                    SLA: vence en 4h    │
│  • Sistema "Route Planner"     hace 3h     #REQ-0038 Promoción   │
│    actualizado                               SLA: vence mañana   │
│  [Ver audit log completo]                  [Ver todos →]         │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

---

### 7.2 — Lista de Usuarios

```
┌──────────────────────────────────────────────────────────────────┐
│  Identity > Usuarios                                             │
│                                                [+ Nuevo Usuario] │
├──────────────────────────────────────────────────────────────────┤
│  🔍 Buscar por nombre, email...    [Tenant ▼] [Status ▼] [Cat ▼]│
│                                               [Exportar CSV]    │
├────┬──────────────────┬─────────────────┬────────┬──────┬───────┤
│    │ NOMBRE           │ EMAIL           │ STATUS │ CAT  │ACCIONES│
├────┼──────────────────┼─────────────────┼────────┼──────┼───────┤
│ ⚪ │ Maria García      │ m.garcia@co.com │ ACTIVE │ INT  │ ⋯     │
│ ⚪ │ Juan Pérez        │ j.perez@co.com  │ BLOCKED│ EXT  │ ⋯     │
│    │                  │                 │ ⚠ Doc  │      │       │
│ ⚪ │ Carlos Lima       │ c.lima@b2b.com  │ PENDING│ B2B  │ ⋯     │
│    │                  │                 │ ⏳ Apro│      │       │
│ ⚪ │ Ana Torres        │ a.torres@co.com │ ACTIVE │ INT  │ ⋯     │
├────┴──────────────────┴─────────────────┴────────┴──────┴───────┤
│  Mostrando 1-25 de 1,247    [< Prev]  Pág 1 de 50  [Next >]     │
└──────────────────────────────────────────────────────────────────┘
```

**Notas de implementación:**
- Badge de status con color semántico: ACTIVE=verde, BLOCKED=rojo, PENDING=ámbar
- BLOCKED muestra subtítulo con razón (Doc vencido / Manual)
- Fila clicable → detalle de usuario
- Menú ⋯ (kebab): Ver, Editar, Bloquear/Desbloquear, Asignar Perfil
- Filtros persistentes en URL (shareables)

---

### 7.3 — Detalle de Usuario

```
┌──────────────────────────────────────────────────────────────────┐
│  Identity > Usuarios > Maria García                   [Editar]   │
├──────────────────────────────────────────────────────────────────┤
│  ┌────────┐   Maria García                    Status: ● ACTIVE   │
│  │ Avatar │   maria.garcia@logistics.com      Cat: INTERNAL      │
│  │  MG    │   Tenant: Logistics Corp          Sede: Callao       │
│  └────────┘   Creado: 2026-01-10              Último login: Hoy  │
│                                                                  │
│  ──────────────────────────────────────────────────────────────  │
│  TABS: [Perfiles] [Documentos] [Solicitudes] [Sesiones] [Audit]  │
│  ──────────────────────────────────────────────────────────────  │
│                                                                  │
│  [TAB ACTIVO: PERFILES]                    [+ Asignar Perfil]    │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ Perfil: Logistics Analyst                                │   │
│  │ Sede: Callao Terminal    Sistema: Route Planner          │   │
│  │ Scope: BRANCH_SCOPED     Templates: 3 asignados          │   │
│  │                                          [Ver] [Revocar] │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ Perfil: WMS Basic Access                                 │   │
│  │ Sede: —               Sistema: WMS                       │   │
│  │ Scope: ORG_WIDE         Templates: 1 asignado            │   │
│  │                                          [Ver] [Revocar] │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ─────────────────────────────── ACCIONES RÁPIDAS ─────────────  │
│  [Ver Grafo de Permisos]    [Bloquear Usuario]    [Reset MFA]    │
└──────────────────────────────────────────────────────────────────┘
```

**TAB: Documentos**
```
│  [TAB ACTIVO: DOCUMENTOS]               [+ Subir Documento]      │
│                                                                  │
│  ┌──────────────┬──────────────────┬────────────┬──────────────┐ │
│  │ TIPO         │ CRITICIDAD       │ VENCE      │ STATUS       │ │
│  ├──────────────┼──────────────────┼────────────┼──────────────┤ │
│  │ Carnet Sanid.│ 🔴 CRITICAL      │ 2026-06-01 │ ● VALID      │ │
│  │ ID Nacional  │ 🟡 HIGH          │ 2028-12-31 │ ● VALID      │ │
│  │ Cert. Seguri.│ 🟠 MEDIUM        │ 2026-05-30 │ ⚠ VENCE 15d  │ │
│  └──────────────┴──────────────────┴────────────┴──────────────┘ │
```

---

### 7.4 — Bandeja de Aprobaciones

```
┌──────────────────────────────────────────────────────────────────┐
│  Approvals > Bandeja de Aprobaciones                             │
├──────────────────────────────────────────────────────────────────┤
│  TABS: [Pendientes (8)] [En progreso (2)] [Historial] [Delegadas]│
├──────────────────────────────────────────────────────────────────┤
│  🔍 Buscar...     [Tipo ▼] [Prioridad ▼]      Ordenar: SLA ▲    │
├────┬──────────────┬──────────────┬──────────┬────────────────────┤
│SLA │ ID / TIPO    │ SOLICITANTE  │ SUJETO   │ ACCIONES          │
├────┼──────────────┼──────────────┼──────────┼────────────────────┤
│⚠1h │ REQ-0041     │ Luis Ramos   │ ext.b2b@ │ [Revisar]         │
│    │ B2B Access   │ (Sponsor)    │ corp.com │                   │
├────┼──────────────┼──────────────┼──────────┼────────────────────┤
│ 4h │ REQ-0039     │ Ana Torres   │ new.user │ [Revisar]         │
│    │ Onboarding   │              │ @co.com  │                   │
├────┼──────────────┼──────────────┼──────────┼────────────────────┤
│ 1d │ REQ-0038     │ Sistema IGA  │ j.perez  │ [Revisar]         │
│    │ Rol Promoc.  │ (Automático) │ @co.com  │                   │
├────┴──────────────┴──────────────┴──────────┴────────────────────┤
│  8 solicitudes pendientes · 2 con SLA crítico (<2h)              │
└──────────────────────────────────────────────────────────────────┘
```

---

### 7.5 — Detalle de Solicitud de Aprobación

```
┌──────────────────────────────────────────────────────────────────┐
│  Approvals > Bandeja > REQ-0041              ⚠ SLA: vence en 1h │
├──────────────────────────────────────────────────────────────────┤
│  SOLICITUD DE ACCESO EXTERNO B2B                                 │
│  ─────────────────────────────────────────────────────────────── │
│  Usuario solicitado:  external.user@contractor.com               │
│  Organización:        FastFreight Logistics SA                   │
│  Sistema:             Route Planner                              │
│  Perfil solicitado:   Freight Analyst (ORG_WIDE)                 │
│  Sponsor:             Luis Ramos · Operations Manager            │
│  Justificación:       "Proveedor contratado para Q2-2026.        │
│                        Requiere acceso para gestión de rutas."   │
│                                                                  │
│  RISK SCORE: ██████████░░ 68/100 (RECOMEND. REVISIÓN)           │
│    · Geographic Anomaly: IP fuera del país (+25)                 │
│    · Device: dispositivo nuevo (+20)                             │
│    · Login Frequency: primera vez (+23)                          │
│                                                                  │
│  DOCUMENTOS ADJUNTOS:                                            │
│    📎 Contrato-FastFreight-2026.pdf  [Ver]                       │
│    📎 ID-Carlos-Lima.pdf             [Ver]                       │
│                                                                  │
│  HISTORIAL:                                                      │
│    2026-05-15 09:00  Solicitud creada por Luis Ramos             │
│    2026-05-15 10:00  Asignada a ti para aprobación              │
│  ─────────────────────────────────────────────────────────────── │
│                                                                  │
│  TU DECISIÓN:                                                    │
│  [Motivo / Comentario (requerido si rechaza o escala)...]        │
│                                                                  │
│  [✅ APROBAR]  [❌ RECHAZAR]  [⬆ ESCALAR]  [↗ DELEGAR]         │
└──────────────────────────────────────────────────────────────────┘
```

---

### 7.6 — Editor de Topology de Sistema (FS-04)

```
┌──────────────────────────────────────────────────────────────────┐
│  Authorization > Sistemas > Route Planner        [Publicar] [···]│
├──────────────────────────────────────────────────────────────────┤
│  TOPOLOGÍA DE MÓDULOS                              [+ Módulo]    │
│  ─────────────────────────────────────────────────────────────── │
│  ▼ 📦 Planning Module                                    [+ Menú]│
│    ▼ 📑 Route Menu                                    [+ Opción] │
│      ├─ 📄 Route List View                               [+ Acción│
│      │    └─ ⚡ route.read                               ]       │
│      │    └─ ⚡ route.create                                      │
│      └─ 📄 Route Detail View                                      │
│           └─ ⚡ route.read                                        │
│           └─ ⚡ route.edit                                        │
│           └─ ⚡ route.delete                                      │
│  ▶ 📦 Reporting Module                                   [+ Menú]│
│  ▶ 📦 Admin Module                                       [+ Menú]│
│  ─────────────────────────────────────────────────────────────── │
│  💡 Arrastra para reordenar módulos y menús                      │
└──────────────────────────────────────────────────────────────────┘
```

---

### 7.7 — Visualizador del Grafo de Permisos (FS-07)

```
┌──────────────────────────────────────────────────────────────────┐
│  Herramientas > Visualizador de Grafo de Permisos                │
├──────────────────────────────────────────────────────────────────┤
│  Usuario: [Maria García ▼]   Sistema: [Route Planner ▼]         │
│  Sede:    [Callao Terminal ▼]                      [Analizar]   │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  RESULTADO: PERMISOS COMPILADOS                                  │
│  ─────────────────────────────────────────────────────────────── │
│                                                                  │
│  PERFIL: Logistics Analyst (Callao)                              │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │ Template: Analyst Base v2.1 (PUBLISHED)                 │    │
│  │   ✅ route.read        ✅ route.create                   │    │
│  │   ❌ route.delete      (DENY explícito)                  │    │
│  │                                                         │    │
│  │ Template: Reporting Access v1.0 (PUBLISHED)             │    │
│  │   ✅ report.view       ✅ report.export                  │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                  │
│  PERFIL: WMS Basic Access (ORG_WIDE)                             │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │ Template: WMS Reader v1.0 (PUBLISHED)                   │    │
│  │   ✅ wms.read                                           │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                  │
│  ⚠ DENY DOMINANCE activo en: route.delete                       │
│     Fuente: Template "Analyst Base v2.1" — regla explícita       │
│                                                                  │
│  RESUMEN: 5 acciones permitidas · 1 denegada explícitamente      │
│  [Exportar JSON]  [Exportar CSV]                                 │
└──────────────────────────────────────────────────────────────────┘
```

---

### 7.8 — Dashboard de Cumplimiento

```
┌──────────────────────────────────────────────────────────────────┐
│  Compliance > Dashboard                                          │
├──────────────────────────────────────────────────────────────────┤
│  ┌───────────────┬───────────────┬──────────────┬───────────────┐│
│  │ DOCS VÁLIDOS  │ EN REVISIÓN   │ POR VENCER   │ VENCIDOS      ││
│  │               │               │ (< 30 días)  │ CRÍTICOS      ││
│  │    1,203      │      12       │     15       │      3        ││
│  │               │               │ ⚠ Atención  │ 🔴 URGENTE   ││
│  └───────────────┴───────────────┴──────────────┴───────────────┘│
│                                                                  │
│  USUARIOS BLOQUEADOS POR DOCUMENTOS          [Ver todos]         │
│  ─────────────────────────────────────────────────────────────── │
│  🔴 Juan Pérez — Carnet Sanitario vencido 2026-05-01            │
│     Enforcement: BLOCK_ACCESS · [Ver usuario]                    │
│  🔴 Carlos Ríos — ID Nacional vencido 2026-04-15                │
│     Enforcement: BLOCK_ACCESS · [Ver usuario]                    │
│                                                                  │
│  PRÓXIMOS A VENCER (< 30 días)               [Ver todos]         │
│  ─────────────────────────────────────────────────────────────── │
│  🟡 Maria García — Cert. Seguridad · Vence 2026-05-30 (15 días) │
│  🟡 Ana Torres — Carnet Sanitario · Vence 2026-06-01 (17 días)  │
│                                                                  │
│  BANDEJA DE VALIDACIÓN (12 documentos)       [Ver bandeja]       │
└──────────────────────────────────────────────────────────────────┘
```

---

### 7.9 — Login Portal (FS-08, FS-09)

```
┌──────────────────────────────────────────────────────────────────┐
│                                                                  │
│            [LOGO DEL TENANT — dinámico]                          │
│                                                                  │
│       Accede a  Route Planner — Logistics Corp                   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                                                          │   │
│  │   📧 Email                                               │   │
│  │   ┌────────────────────────────────────────────────┐    │   │
│  │   │ usuario@empresa.com                            │    │   │
│  │   └────────────────────────────────────────────────┘    │   │
│  │                                                          │   │
│  │   [    🔑  Continuar con Passkey / FIDO2    ]           │   │
│  │   [    ✉️  Continuar con Magic Link          ]           │   │
│  │   [    🏢  Continuar con SSO corporativo     ]          │   │
│  │                                                          │   │
│  │   ──────────── o usa contraseña ────────────            │   │
│  │   [    Ingresar con Email + Contraseña      ]           │   │
│  │                                                          │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│       ¿Problemas para acceder? Contacta a tu administrador       │
│                                                                  │
│  ──────────────────────────────────────────────────────────────  │
│  Powered by UMS · Política de Privacidad · Términos de Uso       │
└──────────────────────────────────────────────────────────────────┘
```

**Variante MFA requerida (score > 40):**
```
│  Verificación adicional requerida                                │
│  Hemos detectado un inicio de sesión desde una ubicación nueva.  │
│                                                                  │
│   [    📱 Verificar con App Authenticator   ]                    │
│   [    📧 Enviar código a m***@empresa.com  ]                    │
│   [    💬 Enviar SMS a +51 9** *** **89     ]                    │
```

---

### 7.10 — Mi Perfil / Self-service (Usuario Final)

```
┌──────────────────────────────────────────────────────────────────┐
│  Mi Perfil                                                       │
├──────────────────────────────────────────────────────────────────┤
│  ┌────────┐  Carlos Lima                                         │
│  │  CL    │  carlos.lima@fastfreight.com                         │
│  └────────┘  Organización: FastFreight Logistics                 │
│              Status: ● ACTIVE                                    │
│                                                                  │
│  TABS: [Mis Accesos] [Mis Documentos] [Seguridad]                │
│                                                                  │
│  [TAB: MIS DOCUMENTOS]                  [+ Subir Documento]      │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │ ● Carnet Sanitario          VALID     Vence: 2026-12-01   │  │
│  │ ⚠ Certificado de Seguridad  PENDING   Subido ayer          │  │
│  │   └─ En revisión por el equipo de compliance              │  │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │ + Subir nuevo documento                                    │  │
│  │   Tipo: [Seleccionar tipo ▼]                               │  │
│  │   Archivo: [Seleccionar archivo...]                        │  │
│  │   Fecha emisión: [__/__/____]                              │  │
│  │   Fecha vencimiento: [__/__/____]                          │  │
│  │                                          [Subir]           │  │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 8. Componentes UI Recomendados

### Design tokens base

```yaml
Colors:
  primary-900: "#1e2a4a"   # Azul profundo — header, sidebar
  primary-700: "#2d4080"   # Azul medio — botones primarios
  primary-100: "#e8edff"   # Azul claro — fondos de selección

  success: "#16a34a"       # Verde — ACTIVE, VALID, APPROVED
  warning: "#d97706"       # Ámbar — PENDING, por vencer, SLA warning
  danger:  "#dc2626"       # Rojo — BLOCKED, EXPIRED, REJECTED, SLA crítico
  neutral: "#6b7280"       # Gris — texto secundario, bordes

Typography:
  font-family: "Inter", system-ui, sans-serif
  font-mono:   "JetBrains Mono", "Fira Code", monospace  # para IDs, JSON
  size-base: 14px
  size-sm: 12px
  size-lg: 16px
  size-xl: 20px

Spacing: 4px base grid (4, 8, 12, 16, 24, 32, 48, 64)

Border-radius: 6px (inputs, cards), 4px (badges), 8px (modals)
```

### Componentes fundamentales

| Componente | Uso en UMS | Nota |
|-----------|-----------|------|
| `StatusBadge` | Estado de usuario, documento, solicitud | Colores semánticos + ícono |
| `SlaCountdown` | SLA en bandeja de aprobaciones | Se vuelve rojo < 2h |
| `RiskScoreBar` | Puntaje de riesgo en aprobaciones | Barra con breaks: 20/40/70 |
| `DataTable` | Todas las listas (usuarios, perfiles, docs) | Sort, filter, export |
| `FilterBar` | Encima de cada DataTable | Tags visibles por filtro activo |
| `ConfirmDialog` | Acciones destructivas (bloquear, revocar) | Requiere texto de confirmación |
| `TabPanel` | Detalle de usuario, sistemas, etc. | URL hash por tab |
| `TreeView` | Jerarquía de tenant, topología de módulos | Expandable/colapsable |
| `PermissionMatrix` | Editor de templates | Grid Recurso × Acción con toggle ALLOW/DENY |
| `GraphViewer` | Visualizador de permisos compilados | Expandable por perfil/template |
| `FileUploader` | Carga de documentos | Drag & drop, validación de tipo/tamaño |
| `NotificationToast` | Feedback de acciones | Stack, auto-dismiss 5s |
| `ApprovalDecisionForm` | Formulario en detalle de solicitud | Motivo requerido en REJECT/ESCALATE |
| `ActivityFeed` | Historial en detalle de usuario | Timeline vertical |
| `TenantSwitcher` | Header — solo Super Admin | Search + recent |
| `BrandingPreview` | Editor de branding del Login Portal | Preview en tiempo real |

### Patrón de formularios

- **Labels sobre inputs** (no placeholders como labels)
- Validación inline en blur, no solo en submit
- Mensajes de error específicos: "El email ya existe en este tenant" vs "Campo requerido"
- Botón primario siempre al final derecho, secundario a su izquierda
- Formularios destructivos (bloquear usuario): modal de confirmación con texto de confirmación (`Escribe "BLOQUEAR" para confirmar`)

---

## 9. Estados de Pantalla

### Estado: Carga (Loading)

- **Listas:** Skeleton rows (no spinner global) — 5 filas placeholder de altura consistente
- **Detalle de usuario/solicitud:** Skeleton del layout completo, incluye avatar y tabs
- **Grafo de permisos:** Spinner centrado + mensaje "Compilando grafo de permisos..."
- **Dashboard widgets:** Skeleton numérico en cada KPI card

**Regla:** Ningún skeleton debe mostrar por más de 3 segundos. Si supera ese tiempo, mostrar error de timeout con opción de reintentar.

---

### Estado: Vacío (Empty State)

| Contexto | Mensaje | CTA |
|---------|---------|-----|
| Lista de usuarios (sin resultados de búsqueda) | "No se encontraron usuarios con estos filtros" | [Limpiar filtros] |
| Lista de usuarios (tenant sin usuarios) | "Aún no hay usuarios registrados en este tenant" | [+ Crear primer usuario] |
| Bandeja de aprobaciones | "No tienes solicitudes pendientes · Todo al día ✓" | — |
| Tab documentos (usuario sin docs) | "Este usuario no ha subido documentos" | [Subir documento] |
| Historial de audit | "No hay eventos registrados para este período" | [Ampliar rango de fechas] |
| Grafo de permisos (sin perfiles) | "Este usuario no tiene perfiles asignados" | [Asignar perfil] |

---

### Estado: Error

```
┌──────────────────────────────────────────────────────────────────┐
│                                                                  │
│               ⚠  Error al cargar los usuarios                    │
│                                                                  │
│  No se pudo obtener la información. Puede ser un problema        │
│  de conectividad temporal.                                       │
│                                                                  │
│  Código de error: [UUID del trace si disponible]                 │
│                                                                  │
│           [Reintentar]      [Volver al Dashboard]                │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

- El error de **validación en formulario** se muestra inline debajo del campo afectado
- El error de **acción asíncrona** (aprobar solicitud fallida) aparece como toast rojo + log en detalle
- Nunca mostrar stack traces al usuario; sí incluir `traceId` para soporte

---

### Estado: Éxito

- Acciones de formulario: Toast verde "Usuario creado exitosamente" + redirección automática a detalle tras 1.5s
- Aprobaciones: Toast verde "Solicitud aprobada · El usuario recibirá notificación" + actualización del badge de pendientes
- Acciones destructivas: Toast con opción de deshacer en ventana de 5s cuando sea técnicamente reversible

---

### Estado: Permisos Insuficientes

```
┌──────────────────────────────────────────────────────────────────┐
│                                                                  │
│               🔒  Sin acceso a esta sección                      │
│                                                                  │
│  No tienes permisos para ver Configuración > Feature Flags.      │
│                                                                  │
│  Si crees que deberías tener acceso, contacta a tu              │
│  administrador de tenant.                                        │
│                                                                  │
│           [Volver al Dashboard]                                  │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

- Los ítems del sidebar sin acceso se **ocultan** (no se muestran grises) para reducir la confusión
- Las acciones en tabla (ej: botón "Editar") sin permisos se **ocultan**, no se muestran deshabilitadas
- Excepción: botones de acción que el usuario normalmente tiene pero no para un registro específico (ej: bloquear un usuario con más privilegios) → mostrar deshabilitado con tooltip "No tienes permisos para esta acción sobre este usuario"

---

### Estado: SLA Crítico (específico de Aprobaciones)

```
┌──────────────────────────────────────────────────────────────────┐
│  🔴 ATENCIÓN: Solicitud REQ-0041 vence en 47 minutos             │
│  Si no hay decisión, la solicitud expirará automáticamente.      │
│  El usuario externo NO obtendrá acceso.           [Revisar ahora]│
└──────────────────────────────────────────────────────────────────┘
```

Banner fijo al tope del contenido (no modal) cuando hay solicitudes con SLA < 2h.

---

## 10. Reglas de Accesibilidad y Usabilidad

### WCAG 2.1 AA — Requisitos mínimos

| Regla | Implementación en UMS |
|-------|----------------------|
| **Contraste de color** | Ratio mínimo 4.5:1 para texto normal, 3:1 para texto grande. Verificar todos los badges de status |
| **Navegación por teclado** | Tab order lógico en formularios. Modales atrapa el foco. Escape cierra modal |
| **Etiquetas ARIA** | `aria-label` en íconos sin texto. `aria-live` en toasts y contadores de pendientes |
| **Roles semánticos** | `<table>` para DataTables con `scope` en headers. `<nav>` para sidebar. `<main>` para content area |
| **Estados de focus** | Outline visible (2px azul) en todos los elementos interactivos. Nunca `outline: none` sin alternativa |
| **Mensajes de error** | Asociados al input via `aria-describedby`. No solo con color |
| **Skip link** | "Ir al contenido principal" como primer elemento focusable de la página |
| **Textos alternativos** | Avatares con `alt="Foto de {nombre}"`. Íconos decorativos con `aria-hidden` |

### Reglas de usabilidad enterprise

1. **Confirmación de acciones destructivas:** Bloquear usuario, revocar perfil, rechazar solicitud siempre requieren confirmación explícita
2. **Persistencia de filtros:** Filtros de lista se guardan en URL (query params) para poder compartir búsquedas y soportar back del navegador
3. **Feedback inmediato:** Toda acción que demora > 300ms muestra indicador de carga en el botón (`[Guardando...]`)
4. **Prevención de pérdida de datos:** Formularios con cambios no guardados muestran confirmación al navegar fuera
5. **Tooltips en íconos:** Todos los íconos de acción (kebab menu items) tienen tooltip al hover
6. **Texto de ayuda en campos complejos:** Campos como "Risk Score Threshold" deben tener `?` con explicación inline
7. **Búsqueda por múltiples criterios:** La búsqueda en listas debe funcionar por nombre, email e ID simultáneamente
8. **Acceso por URL directa:** Todas las vistas de detalle tienen URL propia y permanente (deep linking)

---

## 11. Propuesta Responsive

### Breakpoints

| Breakpoint | Ancho | Comportamiento |
|-----------|-------|----------------|
| `xs` | < 640px | Mobile — solo Login Portal y Mi Perfil/Self-service |
| `sm` | 640–1024px | Tablet — sidebar colapsado, tablas adaptadas |
| `md` | 1024–1280px | Laptop — sidebar colapsado por defecto |
| `lg` | 1280–1536px | Desktop — sidebar expandido, layout completo |
| `xl` | > 1536px | Wide — posible panel lateral adicional en detalle |

### Estrategia por módulo

**Portal Administrativo (PAP)** — optimizado para desktop (lg/xl):
- El sidebar colapsa en `md` y menores → solo íconos + tooltip
- Las DataTables en `sm` colapsan a tarjetas apiladas (card list view)
- El Editor de Template en `sm` usa layout de una columna con scroll horizontal para la matriz
- El Visualizador de Grafo en `sm` no se muestra; banner: "Para mejor experiencia, usa un monitor"

**Bandeja de Aprobaciones** — soporte tablet (sm+):
- En `sm`: lista de solicitudes en tarjetas full-width, sin columnas
- El detalle de solicitud en modal full-screen en `sm`
- El formulario de decisión anclado al pie de la pantalla en mobile

**Login Portal** — mobile-first:
- Diseño centrado vertical y horizontal con máximo 480px de ancho
- Botones full-width
- Logo y branding adaptable a pantalla pequeña
- MFA challenge optimizado para ingreso rápido en mobile

**Mi Perfil / Self-service** — mobile-first:
- Tabs convertidos a accordion en xs/sm
- File uploader con opción de cámara en mobile (captura de documento)
- Feedback de upload en modal full-screen

### Consideraciones táctiles (touch)

- Área mínima de toque: 44×44px para todos los elementos interactivos
- Menú kebab (⋯) en tablas → acciones en bottom sheet en mobile
- Drag & drop de topología de módulos → solo en desktop; en tablet/mobile usar botones de reordenar

---

## 12. Priorización MVP vs Futuras Mejoras

### MVP — Sprint 1-3 (funcionalidades bloqueantes)

| Pantalla / Funcionalidad | FS | Justificación |
|--------------------------|----|-|
| Login Portal (básico, con email+pass) | FS-01, FS-08 | Punto de entrada de todo usuario |
| Gestión de Usuarios (CRUD + status) | FS-01 | Core del sistema |
| Gestión de Tenants | FS-03 | Prerequisito de todo lo demás |
| Gestión de Sistemas y Topología | FS-04 | Prerequisito de autorización |
| Templates de Permisos (CRUD) | FS-02 | Core de autorización |
| Perfiles (asignación manual) | FS-05 | Core de autorización |
| Bandeja de Aprobaciones (básica) | FS-10 | Desbloqueante para B2B |
| Dashboard básico (KPIs principales) | — | Orientación al admin |

### MVP — Sprint 4-6 (funcionalidades de alto valor)

| Pantalla / Funcionalidad | FS | Justificación |
|--------------------------|----|-|
| Subida y Validación de Documentos | FS-11 | Requerido por Compliance |
| Dashboard de Cumplimiento | FS-15, FS-16 | Visibilidad crítica |
| Solicitud de Acceso B2B completa | FS-10 | Con risk score visible |
| Auto-asignación de template | FS-06 | Reduce carga administrativa |
| Configuración IdP por tenant | FS-03 | Para clientes enterprise |
| Procesos de Promoción IGA | FS-12 | Governance de roles |
| Delegación de administración | FS-14 | Escalabilidad de admins |

### Post-MVP (mejoras progresivas)

| Funcionalidad | FS | Esfuerzo | Valor |
|--------------|----|-|-|
| Visualizador Grafo de Permisos interactivo | FS-07 | Alto | Alto |
| Login Portal con Passkeys/FIDO2 | FS-09 | Alto | Alto |
| MFA Adaptativo con Risk Scoring | FS-09 | Muy alto | Alto |
| Feature Flags con UI de targeting | FS-13 | Medio | Medio |
| Configuración jerárquica de parámetros | FS-13 | Medio | Medio |
| Monitor de sesiones en tiempo real | — | Medio | Medio |
| Bulk operations en listas (bloqueo masivo) | — | Bajo | Medio |
| Exportación avanzada (audit reports) | — | Bajo | Medio |
| Branding editor del Login Portal | FS-08 | Medio | Bajo |
| Dark mode | — | Bajo | Bajo |

---

## 13. Riesgos UX y Recomendaciones

### Riesgo 1 — Sobrecarga cognitiva en la pantalla de Topología de Sistema

**Problema:** El modelo Sistema → Módulo → Menú → Opción → Acción tiene hasta 5 niveles de profundidad. Un sistema real (ej: Route Planner) puede tener 50+ nodos.

**Recomendación:** Implementar el TreeView con lazy loading de nodos. Añadir búsqueda instantánea dentro del árbol. Ofrecer vista "tabla plana" alternativa con filtro por nivel. Prioridad: Alta.

---

### Riesgo 2 — Confusión entre Template, Perfil y Rol

**Problema:** Los tres conceptos son jerárquicamente distintos (Rol → Template → Perfil) pero los admins IT pueden confundirlos. Esta confusión puede llevar a asignaciones de permisos erróneas.

**Recomendación:** Incluir tooltips educativos inline en cada concepto. Crear un flujo guiado (wizard) para la primera configuración de autorización. Documentación contextual: panel "¿Cómo funciona?" colapsable en la primera vez que el admin accede a Templates. Prioridad: Alta.

---

### Riesgo 3 — El Grafo de Permisos es técnico para usuarios no técnicos

**Problema:** El Visualizador de Grafo (FS-07) muestra conceptos como "Deny Dominance", "Branch Scope Precedence" y nombres técnicos de acciones. Los Tenant Admins sin formación técnica pueden malinterpretarlo.

**Recomendación:** Ofrecer dos vistas: "Vista simplificada" (¿Puede este usuario hacer X en Y módulo? Sí/No) y "Vista técnica" con el detalle del grafo. Prioridad: Media.

---

### Riesgo 4 — Bandeja de Aprobaciones bajo alta carga

**Problema:** En momentos de onboarding masivo (ej: nueva empresa cliente con 200 usuarios), la bandeja puede tener decenas de solicitudes simultáneas, muchas con SLA crítico.

**Recomendación:** Implementar aprobación en lote para solicitudes del mismo tipo (ej: "Aprobar todos los onboardings de FastFreight"). Ordenamiento inteligente por defecto: SLA más crítico primero. Vista de "modo concentrado" que solo muestra la solicitud más urgente. Prioridad: Alta.

---

### Riesgo 5 — Acciones de alto impacto sin contexto de impacto

**Problema:** "Revocar perfil" o "Deprecar template" pueden tener efectos en cascada que el admin no ve antes de confirmar (ej: deprecar un template que está en uso por 47 perfiles).

**Recomendación:** En el modal de confirmación de acciones de alto impacto, mostrar el impacto calculado: "Este template está actualmente asignado a 47 perfiles en 3 tenants. Deprecarlo no revocará los perfiles existentes, pero nuevas asignaciones no podrán usar esta versión." Prioridad: Alta.

---

### Riesgo 6 — Seguridad UX: exposición inadvertida de información sensible

**Problema:** La pantalla de Visualizador de Grafo, si está mal controlada por permisos, podría revelar la estructura de permisos de otros usuarios a quienes no deben tener esa visibilidad.

**Recomendación:** El Visualizador debe requerir un permiso explícito (`diagnostic.permissions.read`). Auditar cada consulta al grafo (quién consultó el grafo de quién). El resultado no debe ser cacheable en el browser. Prioridad: Crítica.

---

### Riesgo 7 — Login Portal: branding dinámico vs tiempo de carga

**Problema:** El Login Portal carga assets de branding (logo, CSS, colores) desde la API de configuración en tiempo real. Si la API es lenta, el usuario ve un login sin branding o con flash de contenido sin estilo.

**Recomendación:** Pre-renderizar el branding via CDN con TTL corto (5 min). Establecer branding default (UMS genérico) como fallback instantáneo. Implementar skeleton del logo antes de cargar el asset real. Prioridad: Alta.

---

## 14. Supuestos y Preguntas Abiertas

### Supuestos declarados

| ID | Supuesto | Base |
|----|---------|------|
| S-01 | El portal administrativo es una SPA (Single Page Application) con React o similar — no server-rendered pages | Inferido de la arquitectura Hexagonal y el stack .NET API + frontend separado |
| S-02 | El Login Portal es una aplicación separada del PAP, servida en un subdominio diferente | Documentado en FS-08: "hosted login portal" con redirect desde downstream apps |
| S-03 | Los admins siempre operan desde desktop (1280px+); los usuarios finales B2B pueden operar desde mobile | Basado en naturaleza de operaciones y stakeholder profiles |
| S-04 | Las notificaciones in-app se implementan en la primera iteración como badge + dropdown; no como notificaciones push del browser | No hay mención a Web Push API en la documentación |
| S-05 | El tenant switcher solo es visible para Super Admins; los Tenant Admins no ven datos de otros tenants | Consistente con arquitectura RLS y scope de acceso |
| S-06 | El idioma del portal es configurable por tenant (multilenguaje), pero la MVP solo requiere español | No hay mención de i18n obligatoria en los FS del MVP |

### Preguntas abiertas para el Product Owner

| ID | Pregunta | Impacto si no se decide |
|----|---------|------------------------|
| Q-01 | ¿El admin de tenant puede ver/editar usuarios de sedes hijo o solo de su sede? | Afecta el árbol de navegación y los filtros de listas |
| Q-02 | ¿El flujo de onboarding de usuario interno requiere siempre aprobación, o solo cuando el workflow lo configura? | Afecta el diseño del formulario de nuevo usuario y el flujo de estados |
| Q-03 | ¿Existen notificaciones por email al usuario cuando su solicitud es aprobada/rechazada en el MVP? | Afecta la confirmación UX post-decisión del aprobador |
| Q-04 | ¿El editor de templates de permisos es un grid Acción×Recurso o una lista plana? ¿Cuántas acciones puede tener un sistema en producción? | Crítico para el diseño del editor — puede ser muy complejo |
| Q-05 | ¿Los aprobadores pueden agregar comentarios internos (no visibles al solicitante) en una solicitud? | Afecta el formulario de decisión |
| Q-06 | ¿El Visualizador de Grafo de Permisos es accesible desde el detalle de usuario directamente, o solo desde Herramientas? | Afecta la navegación contextual |
| Q-07 | ¿Existe un límite en el número de perfiles que un usuario puede tener simultáneamente? | Afecta el diseño del tab "Perfiles" en detalle de usuario |
| Q-08 | ¿El bulk upload de usuarios (CSV) está en el scope del MVP? | Afecta la pantalla de Lista de Usuarios |
| Q-09 | ¿El Login Portal debe soportar modo oscuro / modo alto contraste desde el MVP? | Afecta el esfuerzo de implementación del theming |
| Q-10 | ¿El Compliance Officer puede rechazar un documento con una razón libre o seleccionando de un catálogo de razones predefinidas? | Afecta el formulario de validación de documentos |

---

**[Volver al Master Index](../../MASTER_INDEX.md)** | **[Product Vision](./product-vision.md)** | **[Functional Stories Index](../requirements/functional-stories/index.md)**
