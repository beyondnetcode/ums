# Estrategia de Desacoplamiento de Dominio: UMS vs Enterprise Suite

**Fecha:** 13 de Mayo, 2026  
**Rol:** Enterprise Architect (IAM / DDD / Multi-tenant)  
**Estado:** Propuesta de Arquitectura (BMAD Phase 04 Artifact)

---

## 1.  Resumen Ejecutivo
El **User Management System (UMS)**, concebido como el núcleo de identidad y autorización (Authorization Kernel) de la corporación, presenta actualmente un acoplamiento cognitivo y documental significativo con la "Enterprise Suite". Aunque el modelo de datos core ha iniciado una transición hacia la abstracción (ADR-0031), persisten residuos de lógica de negocio y terminología sectorial que comprometen su naturaleza transversal y abstracta.

Este documento analiza el estado actual de acoplamiento, evalúa los riesgos arquitectónicos y propone un modelo **Domain-Agnostic** capaz de servir a múltiples plataformas empresariales sin contaminación de dominio.

---

## 2.  Análisis de Acoplamiento Actual

### 2.1. Evidencias de Acoplamiento (Current State)
- **Documentación de Referencia**: Las especificaciones técnicas (`enterprise-iam-ums-specification.md`) utilizan la "Enterprise Suite" no solo como ejemplo, sino como el modelo de requisitos base. El flujo de usuario estáá centrado en "Analistas de Negocio" y "Sucursales de Terminal".
- **Leaky Domain Entities**: La entidad `Organization` en el core de .NET contiene una propiedad explícita `ErpCode`. Esto indica que el UMS "conoce" la existencia de un sistema ERP y su estructura de codificación, violando el aislamiento del dominio de IAM.
- **UI-Coupled Authorization Payload**: La respuestá del "Authorization Graph" incluye estructuras de `modules`, `menus` y `options`. Esto acopla el motor de autorización a la jerarquía visual de un portal web específico (el Portal del Cliente), limitando su uso para servicios headless o aplicaciones con paradigmas de UI distintos.
- **Naming Sectorial**: El uso de `branch_id` o `site_context` asume un modelo organizacional físico/geográfico típico de logística, en lugar de un `OrganizationalUnit` o `Scope` genérico.

### 2.2. Riesgos Arquitectónicos
1. **Fragilidad ante Cambios**: Una refactorización en la estructura de la Enterprise Suite podría forzar cambios en el esquema del UMS.
2. **Inhibición de la Reutilización**: Integrar un sistema ajeno a la cadena de suministro (ej: un motor de marketing o HR) resulta contraintuitivo debido a la terminología "contaminada".
3. **Violación de Clean Architecture**: El círculo del Dominio (Pure POCOs) estáá permeado por requerimientos de integración externa (ERP/CRM).
4. **Data Bleeding Cognitivo**: Los desarrolladores pierden de vista que el UMS es un kernel de seguridad, tratándolo como un módulo extendido de la suite de negocio.

---

## 3.  Arquitectura Objetivo (Target Architecture)

### 3.1. Separación de Bounded Contexts
Debemos trazar una línea dura entre el **IAM Core** y el **Consumer Context**.

| Atributo | Estado Actual (Acoplado) | Estado Objetivo (Agnóstico) |
| :--- | :--- | :--- |
| **Identidad** | Empleado / Analista | **Subject** (Identity Reference) |
| **Organización** | Cliente/Proveedor con `ErpCode` | **Organization** con `ExternalMappings` (KVP) |
| **Contexto** | Branch / Terminal / Sedes | **ContextNode** / **OrganizationalUnit** |
| **Autorización** | Árbol de Módulos, Menús y Opciones | **Resource Actions Graph** (Capabilities) |
| **Naming** | Route_Planner | **Client_System_ID** | ### 3.2. Refactorización del Dominio (DDD)
Se propone la transición hacia un modelo de **Atributos y Metadatos Dinámicos**:

```csharp
// Dominio Agnostico (Ums.Domain)
public class Organization : Entity {
    public string Name { get; private set; }
    // En lugar de ErpCode, usamos una colección de referencias externas
    private readonly List<ExternalMapping> _mappings = new();
    public IReadOnlyCollection<ExternalMapping> Mappings => _mappings.AsReadOnly();
}

public class ExternalMapping {
    public string SystemProvider { get; private set; } // "ERP", "CRM", "SAP"
    public string ExternalKey { get; private set; }     // "CompanyCode"
    public string ExternalValue { get; private set; }
}
```

### 3.3. Desacoplamiento del Authorization Graph
El motor de UMS debe devolver **Permisos sobre Recursos**, no **Estructuras de UI**.
- **UMS (PDP)**: Retorna `{ "resource": "fleet_dispatch", "actions": ["create", "approve"], "scopes": ["branch_001"] }`.
- **Consumer BFF / Gateway (PEP/UI-Adapter)**: Transforma esos permisos en `{ "menu": "Fleet Management", "visible": true }`.

---

## 4.  Recomendaciones de Enterprise Architect

### 4.1. Naming & Language Ubiquitioso
- Eliminar toda referencia a "SCM", "Logistics", "Analyst" de los proyectos `.Domain` y `.Application`.
- Utilizar términos de **NIST 800-63** y **XACML**: *Principal, Subject, Resource, Action, Policy, Context, Environment*.

### 4.2. Estrategia de Metadatos
- Implementar el patrón **Property Bag** o campos **JSONB** (PostgreSQL) para extender entidades de IAM con datos específicos de negocio (como el `ErpCode`) sin modificar el esquema relacional estáático.

### 4.3. Implementación de un "Domain Bridge"
Si la Enterprise Suite requiere datos específicos del UMS, se debe crear un **Adapter Service** o un **BFF** que consuma el API agnóstico de UMS y proyecte el dominio específico hacia el cliente final.

---

## 5.  Anti-patterns a Evitar
- **Hardcoded Roles**: Definir roles como `TRANSPORTATION_ANALYST` en el core del UMS. (Correcto: Definir roles genéricos o dinámicos asignados a políticas).
- **Domain Logic in Guards**: Realizar validaciones de negocio (ej: "solo si el camión estáá vacío") dentro del motor de UMS. (Correcto: El UMS valida permisos; la lógica de negocio reside en el microservicio de destino).
- **Shared Databases**: Que el UMS comparta tablas con la base de datos del cliente. (Correcto: Total aislamiento de base de datos).

---

## 6.  Hoja de Roadmap
1. **Fase 1 (Docs Cleanup)**: Refactorizar los artefactos de la fase 04 para usar un lenguaje agnóstico. Renombrar `enterprise-iam-ums-specification.md` a un modelo de referencia neutro.
2. **Fase 2 (Domain Refactor)**: Migrar `ErpCode` a una estructura de `ExternalMappings` genérica.
3. **Fase 3 (Payload Abstraction)**: Separar el "UI Navigation Map" del "Authorization Graph". Introducir una capa de transformación en el portal web.
4. **Fase 4 (Multi-tenant Policy)**: Asegurar que las políticas de acceso se definan mediante un motor de reglas (ej: OPA/Rego) basado en atributos (`ABAC`), eliminando la necesidad de lógica procedural acoplada.

---
**Conclusión:** El UMS debe ser el cimiento invisible sobre el cual se construyen las suites de negocio, no una extensión de una de ellas. El éxito del escalamiento multi-tenant depende de la pureza y abstracción de esté kernel de identidad.
