# Bounded Context Map

**Tipo:** DDD — Mapa de Contextos  
**Version:** 2.0 | **Fecha:** 2026-05-15 | **Estado:** Propuesto  
**Alcance:** Producto completo — FS-01 a FS-16  

> **Visualizacion interactiva:** [interactive-ddd-viewer.html](./interactive-ddd-viewer.html) — seccion "Bounded Context Map"

---

## Diagrama de Contextos

<details>
<summary>Ver codigo Mermaid (referencia)</summary>

```mermaid
graph TD
    subgraph Core["Nucleo del Producto"]
        A["BC-A: Identity\nums_identity\n.NET 10"]
        B["BC-B: Authorization\nums_authorization\n.NET 10"]
    end

    subgraph Supporting["Soporte Operacional"]
        C["BC-C: Configuration\nums_configuration\n.NET 10"]
        F["BC-F: Approvals\napprovals\n.NET 10"]
        H["BC-H: IGA\niga\n.NET 10"]
    end

    subgraph Generic["Genericos"]
        D["BC-D: Audit\naudit\n.NET 10"]
        G["BC-G: Cache\nRedis\nInfrastructura"]
        E["BC-E: Console PAP\nReact SPA\nSin entidades propias"]
    end

    A -->|"Customer-Supplier\nUser + Org + Branch claims"| B
    B -->|"Customer-Supplier\nSystemSuiteId ref para FeatureFlag scope"| C
    A -->|"Customer-Supplier\nTenant scope keys"| C
    A -->|"Customer-Supplier\nUserRegisteredEvent"| F
    A -->|"Customer-Supplier\nUserRegisteredEvent"| H
    A -->|"Conformist\nevents"| D

    C -->|"Customer-Supplier\nIdP config al gateway"| A
    B -->|"Read-Aside\nauth_graph cache"| G
    C -->|"Read-Aside\ncfg + flags cache"| G
    B -->|"Conformist\nevents"| D
    C -->|"Conformist\nevents"| D

    H -->|"Customer-Supplier\nPromotionApprovedEvent"| B
    H -->|"Customer-Supplier\npromocion requiere workflow"| F
    H -->|"Conformist\nevents"| D

    F -->|"Customer-Supplier\nprovisioning B2B"| A
    F -->|"Customer-Supplier\nasignacion Profile post-aprobacion"| B
    F -->|"Conformist\nevents"| D

    E -->|"Customer-Supplier\nREST APIs"| A
    E -->|"Customer-Supplier\nREST APIs"| B
    E -->|"Customer-Supplier\nREST APIs"| C
    E -->|"Customer-Supplier\nREST APIs"| F
```

</details>

---

## Catalogo de Contextos

| Codigo | Nombre | Schema SQL | Clasificacion | FS |
|--------|--------|-----------|---------------|-----|
| BC-A | Identity | `ums_identity` | Core | FS-01, FS-03, FS-08, FS-09 |
| BC-B | Authorization | `ums_authorization` | Core | FS-02, FS-04, FS-05, FS-06, FS-07 |
| BC-C | Configuration | `ums_configuration` | Supporting | FS-08, FS-09, FS-13 |
| BC-D | Audit | `audit` | Generic | Todos |
| BC-E | Console PAP | React SPA | Generic | Todos (UI) |
| BC-F | Approvals | `approvals` | Supporting | FS-10, FS-11, FS-12 |
| BC-G | Cache | Redis | Generic | BC-B, BC-C |
| BC-H | IGA | `iga` | Supporting | FS-12, FS-14 |

---

## Tabla de Relaciones

| Upstream | Downstream | Patron | Contrato |
|----------|-----------|--------|----------|
| Identity | Authorization | Customer-Supplier | User/Org/Branch claims via eventos o API |
| Authorization | Configuration | Customer-Supplier | SystemSuite.Id como scope FK de FeatureFlag |
| Identity | Configuration | Customer-Supplier | Tenant scope keys para aislamiento de config |
| Identity | Approvals | Customer-Supplier | Registro de usuario externo desencadena workflow |
| Identity | IGA | Customer-Supplier | `UserRegisteredEvent` para inicializar tracking |
| Identity | Audit | Conformist | Eventos inmutables appendeados al ledger |
| Configuration | Identity | Customer-Supplier | IdP config provista al Auth Gateway para routing |
| Authorization | Cache | Shared Kernel `ICachePort` | Read-aside; invalidacion en mutaciones |
| Configuration | Cache | Shared Kernel `IConfigCachePort` | Read-aside cfg + flags; TTL 60-900s |
| Authorization | Audit | Conformist | Eventos inmutables appendeados al ledger |
| Configuration | Audit | Conformist | Eventos inmutables appendeados al ledger |
| IGA | Authorization | Customer-Supplier | `PromotionApprovedEvent` actualiza Profile |
| IGA | Approvals | Customer-Supplier | Promocion requiere ApprovalRequest |
| IGA | Audit | Conformist | Eventos inmutables appendeados al ledger |
| Approvals | Identity | Customer-Supplier | `ApprovalResolvedEvent` activa UserAccount (ONBOARDING) |
| Approvals | Authorization | Customer-Supplier | `ApprovalResolvedEvent` asigna Profile (PROFILE_ASSIGNMENT) |
| Approvals | Audit | Conformist | Eventos inmutables appendeados al ledger |
| Console | Todos | Customer-Supplier | REST versionado; tratado como consumidor externo |

---

## Anti-Corruption Layers

| Frontera | Mecanismo | Motivo |
|---------|-----------|--------|
| Authorization — IdP externo | `IAuthenticationPort` Strategy Pattern | Evita acoplamiento con SDK de Zitadel/Okta |
| Configuration — Feature Flag Providers | `IFeatureFlagPort` Strategy Pattern | Evita LaunchDarkly/Unleash en dominio |
| Configuration — Secret Vault | `ISecretStorePort` Strategy Pattern | Evita AWS Secrets Manager / HashiCorp en dominio |
| Authorization — Redis | `ICachePort` | Evita cliente Redis en capa de dominio |
| Configuration — Redis | `IConfigCachePort` | Namespace separado de auth_graph |
| Authorization — Event Bus | `IEventBusPort` | Evita Kafka/RabbitMQ en use cases |

---

**[Indice DDD](./index.md)** | **[Siguiente: Lenguaje Ubicuo](./02-ubiquitous-language.md)**
