# ADR-0033: Estrategia de Endpoints en APIs .NET (Minimal APIs vs. Controllers)

*   **Estado:** Propuesto
*   **Fecha:** 2026-05-13
*   **Autores:** Arquitecto Principal .NET

---

## 🏛️ 1. Contexto y Problema
En .NET 8, 9 y con el roadmap hacia .NET 10, las **Minimal APIs** ya son una tecnología completamente preparada para producción (*production-ready*). Microsoft las posiciona de forma inequívoca como el futuro definitivo de ASP.NET Core, especialmente para despliegues cloud-native.

Dada la infraestructura actual de UMS, necesitamos formalizar criterios claros de gobernanza técnica que definan cuándo adoptar cada modelo (Minimal APIs vs. Controllers tradicionales), evitando la proliferación de patrones caóticos y estableciendo estándares corporativos estrictos de diseño y desacoplamiento.

---

## 🎯 2. Decisión Arquitectónica
Adoptamos una **Estrategia Híbrida** gobernada por los siguientes criterios de selección explícitos:

### 🚀 2.1 Cuándo Usar Minimal APIs
Es el enfoque predeterminado y obligatorio para:
1.  **Nuevos Microservicios y Servicios Cloud-Native:** Donde la baja huella de memoria y latencia son críticas.
2.  **Servicios Serverless y Event-Driven:** Para optimizar los tiempos de arranque (*Cold Start*).
3.  **BFFs (Backend-for-Frontend):** Por su naturaleza directa y bajo acoplamiento.
4.  **Módulos basados en Vertical Slice Architecture (VSA):** Facilitando la cohesión espacial del flujo de la característica.

### 📦 2.2 Cuándo Mantener ASP.NET Controllers
Se permite el uso de controladores tradicionales únicamente en:
1.  **APIs Enterprise con Lógica de Filtros Compleja:** Donde la jerarquía heredada de `ActionFilters` provea valor real.
2.  **Módulos Legacy en Mantenimiento Activo:** Para evitar reescrituras masivas y riesgos operacionales innecesarios.
3.  **Servicios con Model Binding Avanzado:** Que dependan fuertemente del motor reflectivo clásico de MVC.

---

## 🛠️ 3. Estándares Mandatorios para Minimal APIs
Para prevenir el antipatrón de "Program.cs monolítico", todo endpoint desarrollado bajo Minimal API en la organización **DEBE** cumplir rigurosamente con:

*   **Handlers Aislados:** Se prohíben terminantemente las lambdas inline complejas. Los handlers deben definirse como métodos estáticos o de clase puros (Single Responsibility).
*   **Estructura por Característica:** Uso obligatorio de métodos de extensión de `IEndpointRouteBuilder` segmentados por módulo funcional (*Feature Module*).
*   **Agrupamiento Seguro:** Empleo de `MapGroup` por recurso para la inyección unificada de prefijos de ruta, versionamiento y políticas de seguridad (`Policies`) compartidas.
*   **Alineación con el SDK Base:** Consumo obligatorio del SDK Base corporativo que provee los helpers estandarizados para el registro de endpoints, políticas de versionamiento y telemetría de observabilidad.

---

## ⚖️ 4. Consecuencias y Trade-offs

### ✅ Beneficios
*   **Alto Rendimiento & AOT-Readiness:** Máxima velocidad de ejecución y compatibilidad garantizada con compilación Ahead-of-Time para optimización de recursos cloud.
*   **Adopción Incremental:** Transición fluida y segura sin necesidad de forzar costosas refactorizaciones en el código productivo actual.

### ⚠️ Retos y Mitigaciones
*   **Coexistencia de Dos Modelos:** Introduce complejidad inicial de navegación. *Mitigación:* Se proveerá documentación explícita en el proceso de Onboarding y plantillas oficiales en el monorepo.
*   **Brecha de Habilidades en el Equipo:** Los desarrolladores deben dominar con fluidez tanto la orientación a objetos de MVC como el flujo funcional de Minimal APIs.

---

## 🚀 5. Revisión y Roadmap de Evolución
La presente decisión será sujeta a una revisión técnica exhaustiva en el **Q2 del siguiente año**. El objetivo será evaluar si los Controladores Tradicionales pueden ser oficialmente marcados como deprecados para el desarrollo de cualquier nuevo proyecto o servicio dentro de la compañía.
