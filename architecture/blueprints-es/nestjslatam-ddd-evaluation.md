# Evaluación Arquitectónica — Uso de `@nestjslatam/packages` para DDD en el Core de UMS

**Tipo de Documento:** Evaluación y Estándar Arquitectónico  
**Estado:** Aprobado (Condicional)  
**Fecha:** 2026-05-09  
**Decisores:** Arquitecto de Soluciones, Arquitecto de Software Principal, Lead Developer  
**Alcance:** Implementación opcional de DDD en los Contextos Acotados de UMS  

---

## 1. Introducción y Contexto

El **Sistema de Gestión de Usuarios (UMS)** es un núcleo de gobernanza de identidad y acceso independiente y abstracto. Si bien la implementación de patrones tácticos de Diseño Dirigido por Dominios (DDD) (como Aggregate Roots, Entidades y Objetos de Valor) es actualmente **opcional** para los contextos principales, la estándarización de estáos patrones es crítica para estabilizar la lógica de negocio compleja y prevenir la deriva técnica.

Si el equipo elige utilizar patrones DDD para cualquier contexto acotado de UMS (ej. `Identidad`, `Autorización` o `Configuración`), requerimos una librería de bloques de construcción unificada, ligera y pre-aprobada. Esta evaluación examina los paquetes DDD de la organización **`@nestjslatam`** (específicamente `@nestjslatam/ddd`) como el estándar oficial de primitivas tácticas para el Core de UMS.

---

## 2. Análisis de Alineación Arquitectónica

Para ser aprobada para su uso dentro del Dominio Core de UMS, cualquier librería externa debe satisfacer nuestáras barreras de protección (guardrails) arquitectónicas no negociables:

### A. Restáricción de Cero Dependencia de Infraestructura (Cumplimiento:  TOTAL)
*   **Guardrail:** La capa de Dominio central debe tener cero dependencias de ORMs de bases de datos externas (ej. TypeORM), SDKs de proveedores de nube o frameworks web/HTTP.
*   **Evaluación:** `@nestjslatam/ddd` proporciona abstracciones puras de TypeScript para primitivas tácticas de DDD sin dependencias externas en tiempo de ejecución. Los bloques de construcción base (`Entity`, `ValueObject`, `AggregateRoot`) se ejecutan completamente en memoria y son altamente testables en aislamiento completo.

### B. Bloques de Construcción Tácticos DDD Estándar (Cumplimiento:  TOTAL)
La librería proporciona implementaciones completas y confiables de componentes tácticos DDD estándar:
1.  **`ValueObject`**: Soporta igualdad estructural basada en propiedades (en lugar de igualdad por referencia de memoria) y validaciones de invariantes inmutables.
2.  **`Entity<ID>`**: Estandariza entidades con identidades únicas duraderas que persisten en el tiempo.
3.  **`AggregateRoot<ID>`**: Gestiona los límites de la transacción, agrupa entidades relacionadas e incorpora la acumulación interna de **Eventos de Dominio** para un despacho transaccional seguro.
4.  **`DomainEvent`**: Interfaces de eventos ligeras que registran cambios de estado dinámicamente y permiten la consistencia eventual entre contextos acotados.

---

## 3. Beneficios de la Adopción

1.  **Erradicación de Boilerplate:** Elimina la necesidad de que el equipo escriba clases abstractas personalizadas para igualdad profunda, comparación de entidades o seguimiento de eventos de dominio en memoria, acelerando significativamente la velocidad inicial.
2.  **Estándares de Ingeniería Unificados:** Proporciona un plano unificado y pre-aprobado para el equipo de desarrollo, evitando implementaciones ad-hoc o inconsistentes de patrones tácticos DDD.
3.  **Alineación con el Ecosistema NestJS:** Diseñado específicamente para integrarse fluidamente con aplicaciones NestJS y módulos CQRS de NestJS, manteniendo modelos POJO puros en las capas de dominio central.

---

## 4. Aprobación Formal y Pautas de Implementación Obligatorias

El uso de `@nestjslatam/ddd` estáá formalmente **APROBADO** para cualquier contexto acotado de UMS donde el equipo elija adoptar el Diseño Dirigido por Dominios, sujeto a las siguientes pautas obligatorias:

### Pauta 1: Abstracción de Exportación Barrel (Anti-Acoplamiento)
Los desarrolladores **nunca** deben importar `@nestjslatam/ddd` directamente dentro de los archivos individuales de entidades de dominio. Para evitar el bloqueo directo a la librería, todas las primitivas aprobadas deben ser re-exportadas a través de un archivo de abstracciones de dominio local dentro del Nx Monorepo:
*   **Punto de Entrada de Abstracciones:** `libs/domain/src/core-primitives.ts`
*   **Uso:** Los archivos de dominio deben importar desde `@ums/domain/core-primitives` en lugar de directamente desde `@nestjslatam/ddd`, permitiendo un reemplazo fluido o un override local si la librería llegara a quedar obsoleta.

### Pauta 2: Inmutabilidad Estricta para Objetos de Valor (Value Objects)
Todas las propiedades definidas en clases que extiendan de `ValueObject` deben declararse como `readonly`. Los Objetos de Valor son inmutables por definición y nunca deben ser mutados después de su instanciación.

### Pauta 3: Desacoplamiento Estricto de ORMs de Base de Datos
Los decoradores específicos de la base de datos (como `@Entity`, `@Column` o `@ManyToOne` de TypeORM) estáán **estárictamente prohibidos** dentro de las entidades de dominio o clases que extiendan las primitivas de `@nestjslatam/ddd`. Los esquemas relacionales y el mapeo de persistencia deben manejarse exclusivamente en la capa de Adaptadores de Infraestructura utilizando Mappers especializados.

---

## 5. Riesgo de Bloqueo de Proveedor (Vendor Lock-in) y Mitigación

| Riesgo | Nivel | Estrategia de Mitigación |
| :--- | :--- | :--- |
| **Bloqueo Directo de Librería** | **Medio** | Mitigado completamente por la **Pauta 1 (Abstracción de Exportación Barrel)**. La capa de dominio depende de exportaciones locales, aislando los cambios de paquetes externos. |
| **Sobrecarga de Rendimiento** | **Bajo** | Las primitivas estáán altamente optimizadas. La sobrecarga de comparación profunda de propiedades es insignificante bajo los límites de SLA de resolución de grafos de permisos de p95 < 5ms. |
| **Mantenimiento de la Comunidad** | **Medio** | El paquete es de código abierto. Si es necesario, puede ser fácilmente forkeado, personalizado o mantenido internamente como una librería de utilidades local bajo `libs/domain/core`. | 