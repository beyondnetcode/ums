# ADR-0033: Standardizing Minimal APIs for Enterprise Services in .NET 8/9+

*   **Status:** Proposed
*   **Date:** 2026-05-13
*   **Authors:** Principal .NET Architect

---

## 🏛️ 1. Context and Problem
With the maturity reached by .NET 8 and .NET 9, **Minimal APIs** have evolved from being a simplified syntactic mechanism to becoming the technology standard promoted by Microsoft for cloud-native and high-performance architectures (including native support for **Ahead-Of-Time - Native AOT**).

Traditionally, the organization has relied on classic **ASP.NET Controllers** based on inheritance from `ControllerBase`. However, these introduce considerable reflection overhead during assembly scanning at startup (bootstrap), are complex to compile under Native AOT without extensive exclusions, and promote a class-oriented structure that often physically separates route definitions from the business flow, conflicting with emerging high-cohesion philosophies like **Vertical Slice Architecture**.

It is necessary to decide whether to adopt **Minimal APIs** as the corporate enterprise standard for the UMS monorepo or to maintain the traditional **Controllers** approach.

---

## 🎯 2. Architectural Decision
We adopt a **Hybrid Approach leaning towards Total Adoption**, formalizing the following mandatory guidelines:

1.  **New Microservices and Serverless Services:** The exclusive use of **Minimal APIs** is **mandatory**. This ensures low startup latency (Cold Start), reduced memory footprint, and full compatibility for future Native AOT compilation optimizations.
2.  **Monolithic Modules and Existing Complex Contexts:** Technical coexistence is allowed. New endpoints added to legacy services should preferentially be implemented with Minimal APIs within the existing routing pipeline, while progressive refactoring of traditional controllers towards endpoint filters is planned.
3.  **Code Organization Standard (Governance):** To avoid architectural degradation and the "Big Fat Program.cs" anti-pattern, the use of the `IEndpointRouteBuilder` extension pattern is mandatory to encapsulate endpoint groups (`MapGroup`) organized by Bounded Context, or failing that, modular packaging libraries like `Carter` or `FastEndpoints`.
4.  **Decoupling in Clean Architecture:** Minimal API endpoints shall act strictly as the thin HTTP infrastructure layer (input port/adapter), immediately delegating execution to the application layer via the command/query dispatcher `ISender` (MediatR).
5.  **Strong Typing and Automatic Documentation:** Consistent use of `TypedResults` instead of the generic `Results` type is required to ensure OpenAPI (Swagger) documentation is inferred with absolute precision at compile-time and to facilitate fast unit testing without mocking HTTP contexts.

---

## 📊 3. Technical Operational Comparison

| Feature | Minimal APIs (.NET 8/9+) | ASP.NET Controllers |
| :--- | :--- | :--- |
| **Native AOT Support** | Native and optimized first-class | Restricted (heavy framework reflection) |
| **Performance (Throughput)** | Ultra High (lower allocations) | High |
| **Boilerplate** | Minimal (direct functional approach) | Medium-High (classes, redundant constructors) |
| **Learning Curve** | Low (simplified), but requires discipline | Highly familiar and industry-standard |
| **Observability** | Native and integrated with `OpenTelemetry`| Native and integrated with `OpenTelemetry`|
| **Cross-Cutting Mechanism**| Endpoint Filters (`IEndpointFilter`) | Action Filters (`IActionFilter`) |

---

## ⚖️ 4. Risks, Trade-offs, and Mitigations

### ⚠️ Risk: Mixing Logic at the Entry Point (Spaghetti Code)
The syntactic flexibility of Minimal APIs encourages embedding business logic or SQL queries directly into the endpoint lambda.
*   **Mitigation (M-01):** Static rule in SonarQube/Linter prohibiting endpoint lambdas exceeding 5 lines of code. Their single responsibility must be: extract request data, call the dispatcher (MediatR), and return the `IResult`.

### ⚠️ Risk: Loss of Project Visual Structure
Moving from a uniform `Controllers/` directory to multiple scattered static extensions can reduce navigability for new developers.
*   **Mitigation (M-02):** Mandatory structuring under Feature Folders within the WebAPI project, where each `Endpoint` is defined in the same directory as the Request/Response it processes (Vertical Slice).

---

## 🚀 5. Transition and Compatibility Strategy

1.  **Phase 1 (Short Term):** Register the unified endpoint routing middleware in the main .NET 8 project without altering existing active legacy controllers.
2.  **Phase 2 (Medium Term):** Implement the next Bounded Context to be developed in UMS using exclusively the Minimal API approach organized modularly via the `IEndpointRouteBuilder` interface.
3.  **Phase 3 (Long Term):** Migrate high-throughput consumption points to Minimal APIs to selectively enable Native AOT compilation and optimize pod density in Kubernetes.
