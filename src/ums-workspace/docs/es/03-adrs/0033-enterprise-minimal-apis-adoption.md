# ADR-0033: Estandarización de Minimal APIs para Servicios Empresariales en .NET 8/9+

*   **Estado:** Propuesto
*   **Fecha:** 2026-05-13
*   **Autores:** Arquitecto Principal .NET

---

## 🏛️ 1. Contexto y Problema
Con la madurez alcanzada por .NET 8 y .NET 9, las **Minimal APIs** han evolucionado de ser un mecanismo sintáctico simplificado a convertirse en el estándar tecnológico promovido por Microsoft para arquitecturas cloud-native y de alto rendimiento (incluyendo el soporte nativo para **Ahead-Of-Time - Native AOT**).

Tradicionalmente, la organización ha confiado en los **ASP.NET Controllers** clásicos basados en la herencia de `ControllerBase`. Sin embargo, estos introducen un overhead de reflexión considerable durante el escaneo de ensamblados en el arranque (bootstrap), son complejos de compilar bajo Native AOT sin complejas exclusiones, y promueven una estructura orientada a clases que a menudo separa físicamente la definición de las rutas del flujo de negocio, lo que entra en conflicto con filosofías emergentes de alta cohesión como **Vertical Slice Architecture**.

Es necesario decidir si adoptamos **Minimal APIs** como el estándar corporativo corporativo del monorepo de UMS o mantenemos el enfoque tradicional de **Controllers**.

---

## 🎯 2. Decisión Arquitectónica
Adoptamos un **Enfoque Híbrido con Tendencia Directa hacia la Adopción Total**, formalizando las siguientes directrices obligatorias:

1.  **Nuevos Microservicios y Servicios Serverless:** Es **obligatorio** el uso exclusivo de **Minimal APIs**. Esto garantiza una baja latencia de arranque (Cold Start), menor consumo de memoria RAM y total compatibilidad para optimizaciones de compilación Native AOT.
2.  **Módulos Monolíticos y Contextos Complejos Existentes:** Se permite la coexistencia técnica. Los nuevos endpoints agregados a servicios legacy deben implementarse de forma preferencial con Minimal APIs dentro del pipeline de enrutamiento existente, mientras se planifica la refactorización progresiva de los controladores tradicionales hacia filtros de endpoint.
3.  **Estándar de Organización de Código (Gobernanza):** Para evitar la degradación arquitectónica y el antipatrón de "Big Fat Program.cs", se establece el uso obligatorio del patrón de extensión `IEndpointRouteBuilder` para encapsular grupos de endpoints (`MapGroup`) organizados por Bounded Context, o en su defecto, librerías de empaquetado modular como `Carter` o `FastEndpoints`.
4.  **Desacoplamiento en Clean Architecture:** Los endpoints de Minimal API actuarán estrictamente como la delgada capa de infraestructura HTTP (puerto/adaptador de entrada), delegando de forma inmediata la ejecución a la capa de aplicación a través del mediador de comandos/consultas `ISender` (MediatR).
5.  **Tipado Fuerte y Documentación Automática:** Se exige el uso consistente de `TypedResults` en lugar del tipo genérico `Results` para garantizar que la documentación OpenAPI (Swagger) se deduzca con precisión absoluta en tiempo de compilación y para facilitar pruebas unitarias rápidas sin mockear el contexto HTTP.

---

## 📊 3. Comparativa Técnica Operacional

| Característica | Minimal APIs (.NET 8/9+) | ASP.NET Controllers |
| :--- | :--- | :--- |
| **Soporte Native AOT** | Nativo y optimizado de primera clase | Restringido (reflexión interna del framework) |
| **Rendimiento (Throughput)** | Ultra Alto (baja asignación de memoria) | Alto |
| **Boilerplate** | Mínimo (enfoque funcional directo) | Medio-Alto (clases, constructores redundantes) |
| **Curva de Aprendizaje** | Baja (simplificado), pero requiere disciplina | Muy familiar y estandarizado en la industria |
| **Observabilidad** | Nativa integrada con `OpenTelemetry` | Nativa integrada con `OpenTelemetry` |
| **Mecanismo Transversal** | Filtros de Endpoint (`IEndpointFilter`) | Filtros de Acción (`IActionFilter`) |

---

## ⚖️ 4. Riesgos, Trade-offs y Mitigaciones

### ⚠️ Riesgo: Mezcla de Lógica en el Punto de Entrada (Spaghetti Code)
La flexibilidad sintáctica de las Minimal APIs incita a incrustar lógica de negocio o consultas SQL directamente en el lambda del endpoint.
*   **Mitigación (M-01):** Regla estática en SonarQube/Linter que prohíbe lambdas de endpoint que excedan las 5 líneas de código. Su única responsabilidad debe ser: extraer datos del request, llamar al dispatcher (MediatR) y retornar el `IResult`.

### ⚠️ Riesgo: Pérdida de Estructura Visual del Proyecto
Pasar de un directorio `Controllers/` uniforme a múltiples extensiones estáticas dispersas puede reducir la navegabilidad de los desarrolladores nuevos.
*   **Mitigación (M-02):** Estructuración obligatoria bajo carpetas de Características (Feature Folders) dentro del proyecto WebAPI, donde cada `Endpoint` se define en el mismo directorio que el Request/Response que procesa (Vertical Slice).

---

## 🚀 5. Estrategia de Transición y Compatibilidad

1.  **Fase 1 (Corto Plazo):** Registrar el Middleware de enrutamiento de endpoints unificado en el proyecto principal de .NET 8 sin alterar los controladores heredados activos.
2.  **Fase 2 (Medio Plazo):** Implementar el siguiente Bounded Context a desarrollar en UMS usando exclusivamente el enfoque de Minimal API organizado modularmente mediante la interfaz `IEndpointRouteBuilder`.
3.  **Fase 3 (Largo Plazo):** Migrar puntos calientes de consumo (High-throughput routes) a Minimal API para poder activar selectivamente la compilación Native AOT y optimizar la densidad de pods en Kubernetes.
