# Mapeo Historias Funcionales → Historias Técnicas

**Versión:** 1.0  
**Fecha:** 2026-05-14  
**Propósito:** Mapear cada Historia Funcional (FS) a sus Historias Técnicas (TS)  
**Alcance:** Las 16 historias funcionales en las 8 épicas

---

## EP-01: Tenant & Identidad

### FS-01: Corporate User Login

**Descripción Funcional:**  
Como usuario corporativo, puedo hacer login con email + contraseña para acceder a UMS.

**Historias Técnicas Asociadas:**
| TS | Título | Pts | Propósito |
|----|--------|-----|----------|
| TS-1.1 | Modelo de Dominio Jerarquía Tenant | 8 | Definir agregado User con contexto tenant |
| TS-1.2 | Infraestructura SQL Server (Schema & Índices) | 8 | Tabla User con PK compuesta, partición en root_tenant_id |
| TS-1.3 | Filtros Global EF Core (Capa Aplicación) | 8 | Aislamiento tenant a nivel aplicación vía ICurrentTenantResolver |
| TS-1.4 | Puertos & Adaptadores Registro | 8 | Implementaciones IPasswordHasher, IUserRepository |
| TS-1.5 | Endpoints API Autenticación | 8 | Endpoint POST /api/v1/auth/login + validación |
| TS-1.6 | Tests Aislamiento Multi-Tenant | 13 | Validar aislamiento login entre tenants (filtro EF + PK compuesta) |

**Alineación Criterios de Aceptación:**
- ✅ Usuario puede ingresar email + contraseña → manejado por TS-1.5 (validación de formulario)
- ✅ Contraseña verificada securely → manejado por TS-1.4 (BcryptPasswordHasher)
- ✅ Usuario aislado por tenant → manejado por TS-1.3 (filtro global EF Core, PRIMARIO) + TS-1.2 (enforcement PK compuesta)
- ✅ Login auditable → implícito en todos TS (audit columns)

---

### FS-02: Auto-Registro de Usuario

**Descripción Funcional:**  
Como nuevo usuario, puedo auto-registrarme con email + nombre + contraseña, recibir email de verificación.

**Historias Técnicas Asociadas:**
| TS | Título | Pts | Propósito |
|----|--------|-----|----------|
| TS-1.1 | Modelo de Dominio Jerarquía Tenant | 8 | Definir agregado User con estado de registro |
| TS-1.2 | Infraestructura SQL Server (Schema & Índices) | 8 | Tabla User con columna verified_at, PK compuesta |
| TS-1.3 | Filtros Global EF Core (Capa Aplicación) | 8 | Contexto tenant durante flujo registro |
| TS-1.4 | Puertos & Adaptadores Registro | 8 | IEmailService (Sendgrid), dominio service registro |
| TS-1.5 | Endpoints API Autenticación | 8 | Endpoint POST /api/v1/auth/register |
| TS-1.6 | Tests Aislamiento Multi-Tenant | 13 | Validar usuarios registrados aislados por tenant |

**Alineación Criterios de Aceptación:**
- ✅ Formulario acepta email, nombre, contraseña → TS-1.5 (FluentValidation)
- ✅ Contraseña hasheada antes de almacenamiento → TS-1.4 (BcryptPasswordHasher)
- ✅ Email verificación enviado → TS-1.4 (SendgridEmailAdapter)
- ✅ Usuarios no verificados no pueden login → TS-1.5 (lógica API check)
- ✅ Cada usuario aislado por tenant → TS-1.3 (filtro EF Core) + TS-1.2 (PK compuesta)

---

### FS-03: Onboarding de Organización

**Descripción Funcional:**  
Como admin org, puedo onboardear nueva organización con nombre + info contacto, creando tenant raíz + usuario admin inicial.

**Historias Técnicas Asociadas:**
| TS | Título | Pts | Propósito |
|----|--------|-----|----------|
| TS-1.1 | Modelo de Dominio Jerarquía Tenant | 8 | Agregado Tenant, patrón TenantClosure |
| TS-1.2 | Infraestructura SQL Server (Schema & Índices) | 8 | Tablas Tenant + TenantClosure, setup partición |
| TS-1.3 | Filtros Global EF Core (Capa Aplicación) | 8 | Contexto tenant durante onboarding |
| TS-1.4 | Puertos & Adaptadores Registro | 8 | TenantRepository, service creación usuario |
| TS-1.5 | Endpoints API Autenticación | 8 | Endpoint POST /api/v1/tenants (onboarding) |
| TS-1.6 | Tests Aislamiento Multi-Tenant | 13 | Validar nuevo tenant aislado, usuario admin scoped |

**Alineación Criterios de Aceptación:**
- ✅ Crear nueva organización → TS-1.1 + TS-1.5 (agregado Tenant + endpoint)
- ✅ Inicializar root_tenant_id (anchor tabla closure) → TS-1.2 (schema) + TS-1.1 (lógica dominio)
- ✅ Crear usuario admin scoped a tenant → TS-1.4 (service creación usuario)
- ✅ Verificar tenant/admin aislado de otros → TS-1.6 (filtro EF Core + tests)

---

## EP-02: Catálogo de Sistemas

### FS-04: Registrar Sistema y Definir Topología

**Descripción Funcional:**  
Como admin sistemas, puedo registrar un sistema (ej. CRM) y definir su topología (ambientes, endpoints).

**Historias Técnicas Asociadas:**
| TS | Título | Pts | Propósito |
|----|--------|-----|----------|
| TS-2.1 | Modelo de Dominio Catálogo Sistemas | 5 | Agregados System + SystemTopology |
| TS-2.2 | Tablas SQL Server Catálogo Sistemas | 8 | [console].[systems], [console].[system_topologies] |
| TS-2.3 | Repository & Service Topología | 5 | ISystemRepository, análisis topología |
| TS-2.4 | API Registro de Sistemas | 5 | POST /api/v1/systems, CRUD topología |
| TS-2.5 | Tests Integración Catálogo | 8 | Registro sistema, aislamiento, queries topología |

**Alineación Criterios de Aceptación:**
- ✅ Registrar sistema con nombre, tipo, base_url → TS-2.4 (API) + TS-2.1 (validación dominio)
- ✅ Definir ambientes (DEV, TEST, STAGING, PROD) → TS-2.1 (SystemTopology value object)
- ✅ Enumerar endpoints por ambiente → TS-2.2 (schema con topología JSON)
- ✅ Sistemas scoped a tenant → TS-2.2 (RLS vía root_tenant_id)

---

## EP-03: Autorización

### FS-05: Definir Política de Autorización

**Descripción Funcional:**  
Como admin seguridad, puedo definir políticas estilo XACML con reglas, condiciones y efectos.

**Historias Técnicas Asociadas:**
| TS | Título | Pts | Propósito |
|----|--------|-----|----------|
| TS-3.1 | Modelo de Dominio XACML | 13 | Agregados Policy, Rule, Condition |
| TS-3.2 | Implementación PEP/PDP/PAP/PIP | 21 | Motor decisión XACML completo |
| TS-3.3 | Tablas SQL Autorización | 13 | [authorization].[policies], [authorization].[rules] |
| TS-3.5 | API Gestión Políticas | 8 | POST /api/v1/authorization/policies (admin) |
| TS-3.6 | Unit Tests PDP | 13 | Lógica evaluación política (20+ escenarios) |

**Alineación Criterios de Aceptación:**
- ✅ Crear política con nombre, lista reglas → TS-3.1 (agregado Policy) + TS-3.5 (endpoint POST)
- ✅ Cada regla: efecto [ALLOW/DENY], condiciones, acciones, recursos → TS-3.1 (entidad Rule)
- ✅ Condiciones: attribute-based (user.role, system.name, time.hour) → TS-3.2 (resolución atributos PIP)
- ✅ Reglas evaluadas en orden (primer match gana o aggregación efectos) → TS-3.2 (lógica PDP)
- ✅ Políticas persistentes y queryables → TS-3.3 (tablas SQL)

---

### FS-06: Asignar Perfil de Autorización a Usuario

**Descripción Funcional:**  
Como admin seguridad, puedo crear perfiles (bundles de políticas) y asignarlas a usuarios.

**Historias Técnicas Asociadas:**
| TS | Título | Pts | Propósito |
|----|--------|-----|----------|
| TS-3.1 | Modelo de Dominio XACML | 13 | Agregado Profile, profile assignments |
| TS-3.2 | Implementación PEP/PDP/PAP/PIP | 21 | Compilación profile, evaluación permisos |
| TS-3.3 | Tablas SQL Autorización | 13 | [authorization].[profiles], [authorization].[profile_assignments] |
| TS-3.5 | API Gestión Políticas | 8 | POST /api/v1/authorization/profiles/{id}/assign |
| TS-3.7 | Tests Integración Autorización | 13 | Flujo asignación profile, herencia permisos |

**Alineación Criterios de Aceptación:**
- ✅ Crear profile con nombre, lista políticas → TS-3.1 (agregado Profile) + TS-3.5 (API)
- ✅ Asignar profile a usuario → TS-3.5 (endpoint POST assign) + TS-3.3 (tabla profile_assignments)
- ✅ Usuario hereda permisos de todas las políticas → TS-3.2 (modelo compilación PDP, ADR-0021)
- ✅ Asignación persistente → TS-3.3 (SQL)
- ✅ Verificar usuario gana permisos post-asignación → TS-3.7 (integration tests)

---

### FS-07: Evaluar Permisos de Usuario en Runtime

**Descripción Funcional:**  
En tiempo de request API, evaluar si usuario puede realizar acción en recurso (decisión PDP).

**Historias Técnicas Asociadas:**
| TS | Título | Pts | Propósito |
|----|--------|-----|----------|
| TS-3.2 | Implementación PEP/PDP/PAP/PIP | 21 | Motor decisión completo: resolución atributos, matching reglas, caching |
| TS-3.4 | Middleware Autorización | 8 | Middleware ASP.NET Core interceptando requests |
| TS-3.5 | API Gestión Políticas | 8 | POST /api/v1/authorization/check (endpoint test) |
| TS-3.6 | Unit Tests PDP | 13 | Lógica motor decisión (20+ escenarios) |
| TS-3.7 | Tests Integración Autorización | 13 | End-to-end request → decisión → allow/deny |

**Alineación Criterios de Aceptación:**
- ✅ Extraer usuario, sistema, acción, recurso del contexto request → TS-3.4 (middleware)
- ✅ Resolver atributos usuario (role, grupo, antigüedad) vía PIP → TS-3.2 (IAttributeRepository)
- ✅ Evaluar condiciones de políticas → TS-3.2 (evaluación PDP)
- ✅ Retornar decisión ALLOW o DENY + explicación → TS-3.2 (objeto decision)
- ✅ Enforcement decisión (permitir continuación o 403) → TS-3.4 (enforcement middleware)
- ✅ Cache de decisiones para performance → TS-3.2 (IAuthorizationCache)

---

## EP-04: Configuración

### FS-13: Definir Parámetros de Configuración Jerárquicos

**Descripción Funcional:**  
Como admin config, puedo definir parámetros a nivel tenant/sistema/ambiente con resolución jerárquica.

**Historias Técnicas Asociadas:**
| TS | Título | Pts | Propósito |
|----|--------|-----|----------|
| TS-4.1 | Modelo Dominio Config Jerárquica | 8 | Agregado ConfigurationParameter con scope |
| TS-4.2 | Tablas SQL Configuración | 5 | [configuration].[parameters], [configuration].[parameter_history] |
| TS-4.3 | Service Resolución Configuración | 5 | Resolución jerárquica + caching (ADR-0047) |
| TS-4.4 | Endpoints API Configuración | 5 | POST/GET/PATCH /api/v1/configuration/parameters |
| TS-4.5 | Tests Jerarquía Configuración | 8 | Resolución scope, comportamiento override, invalidación cache |

**Alineación Criterios de Aceptación:**
- ✅ Crear parámetro en scope (TENANT/SISTEMA/AMBIENTE) → TS-4.1 + TS-4.4 (API)
- ✅ Resolución jerárquica (AMBIENTE > SISTEMA > TENANT defaults) → TS-4.3 (service resolución)
- ✅ Comportamiento override: parámetro sistema overridea parámetro tenant → TS-4.3 (lógica resolución)
- ✅ Parametrizado por tenant → TS-4.2 (RLS vía root_tenant_id)
- ✅ Performance: caching con TTL (5 min) → TS-4.3 (Redis opcional)

---

## EP-05: Experiencia & Diagnósticos

### FS-08: Página Login Hosted + Diagnósticos

**Descripción Funcional:**  
Como usuario, veo página login branded. Como admin, veo dashboard diagnósticos con salud sistema + audit logs.

**Historias Técnicas Asociadas:**
| TS | Título | Pts | Propósito |
|----|--------|-----|----------|
| TS-1.5 | Endpoints API Autenticación | 8 | Endpoint login core |
| TS-5.1 | Página Login Hosted (React/Vite) | 13 | UI, formulario, validación, responsive, branding tenant |
| TS-5.2 | Dashboard Diagnósticos (React Admin) | 13 | Agregación métricas, gráficos, updates real-time |
| TS-5.3 | Endpoint Audit Log | 8 | GET /api/v1/audit/logs (queryable, paginated) |
| TS-5.4 | Endpoint Health Check | 5 | /health, /health/ready probes liveness/readiness |
| TS-5.5 | Tests Integración Frontend | 8 | E2E flujo login (Playwright), carga dashboard, precisión métricas |

**Alineación Criterios de Aceptación:**
- ✅ Página login branded (colores tenant de config) → TS-5.1 (página React + resolución config TS-4.3)
- ✅ Formulario: email, contraseña, remember-me → TS-5.1 (formulario React controlled, validación client+server)
- ✅ Mensajes error: credenciales inválidas, cuenta locked, errores network → TS-5.1 (manejo errores React)
- ✅ Widgets diagnósticos: contador tenants, contador usuarios, salud filtro EF Core, tiempos respuesta → TS-5.2 (métricas vía TanStack Query)
- ✅ Eventos audit queryables con filtros → TS-5.3 (filtrado API, pagination)
- ✅ Endpoint health para k8s/monitoring → TS-5.4 (respuestas 200/503)

---

## EP-06: Seguridad, Acceso Externo & Delegación

### FS-09: MFA Adaptativo & Autenticación Sin Contraseña

**Descripción Funcional:**  
Durante login, evaluar riesgo (6 factores). Si ALTO riesgo, challenge con MFA. Soportar FIDO2, magic link, app push.

---

### FS-10: Acceso B2B Externo & Flujo Aprobación

**Descripción Funcional:**  
Usuario externo partner solicita acceso a sistemas. Flujo aprobación (Security → Manager) otorga acceso.

---

### FS-14: Administración Delegada & Scopes

**Descripción Funcional:**  
Admin A puede delegar scopes específicos (read-only, sistema X solamente, role Y solamente) a Admin B por duración limitada.

---

## EP-07: Ciclo Cumplimiento

### FS-11: Upload & Validación Documentos Cumplimiento

**Descripción Funcional:**  
Usuario uploadea ID, pasaporte, certificación, etc. Sistema valida documento y almacena securely.

---

### FS-15: Reglas Notificación Expiración

**Descripción Funcional:**  
Admin define reglas: "Enviar EMAIL 30 días antes expiración documento, diariamente hasta 1 día antes."

---

### FS-16: Comportamiento Acceso en Expiración

**Descripción Funcional:**  
En expiración de documento, aplicar enforcement: WARNING (banner), SUSPEND (bloquear), o REVOKE (permanente).

---

## EP-08: IGA Avanzado

### FS-12: Promoción de Role & Tracking Madurez (EXPANDIDO 2→6 historias)

**Descripción Funcional:**  
Trackear madurez role de usuario (JUNIOR→INTERMEDIATE→SENIOR→LEAD→PRINCIPAL). Usuario solicita promoción cuando elegible. Security revisa impacto, aprueba/rechaza.

---

---

**Preparado por:** Arquitecto Principal  
**Fecha:** 2026-05-14  
**Estado:** ✅ **MAPEO COMPLETO**
