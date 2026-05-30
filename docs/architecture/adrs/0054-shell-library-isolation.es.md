# ADR-0054: Aislamiento de Shell Libraries para Patrones DDD, Factory, AOP y Bootstrapper

**Estado:** Aceptado
**Fecha:** 2026-05-15
**Enmendado:** 2026-05-24 — grafo de dependencias corregido; alcance extendido para incluir `BeyondNetCode.Shell.Aop` y `BeyondNetCode.Shell.Bootstrapper`
**Responsable de Decisión:** Arquitectura
**Relacionados:**
- [ADR-0060: Estrategia de Concerns Cross-Cutting AOP](./0060-aop-cross-cutting-concern-strategy.md)
- [Guías de Desarrollo de Shell Libraries](../shell-libraries/README.md)

---

## Contexto

UMS incluye librerías de infraestructura reutilizables bajo `src/libs/shell`. Estas librerías se originaron de fuentes externas o de referencia, pero UMS no debe exponer namespaces upstream ni convenciones de repositorio en código de aplicación.

El ADR-0029 anterior describía "Primitivas DDD nativas de C# (sin librería externa)". Esa redacción ya no es precisa: la posición correcta es que UMS **posee su superficie de dependencia de dominio** a través de las shell libraries `BeyondNetCode.Shell.*`. Las capas de aplicación consumen la abstracción shell de UMS, no la identidad fuente upstream.

A partir de 2026-05-24, la capa shell incluye cuatro grupos de librerías:

| Grupo | Proyectos | Capas consumidoras |
|---|---|---|
| `BeyondNetCode.Shell.Ddd` | `BeyondNetCode.Shell.Ddd` · `BeyondNetCode.Shell.Ddd.ValueObjects` | Domain (directo) |
| `BeyondNetCode.Shell.Factory` | `BeyondNetCode.Shell.Factory` · `BeyondNetCode.Shell.DI` | Domain (transitivo vía Ddd) · Infrastructure (directo) |
| `BeyondNetCode.Shell.Aop` | `BeyondNetCode.Shell.Aop` · `BeyondNetCode.Shell.DispatchProxy` · `BeyondNetCode.Shell.Aspects` · `BeyondNetCode.Shell.Logger.Serilog` · `BeyondNetCode.Shell.DI` | Application (contrato de atributos) · Infrastructure (wiring DI + adapters) |
| `BeyondNetCode.Shell.Bootstrapper` | `BeyondNetCode.Shell.Bootstrapper` · `BeyondNetCode.Shell.DI` · `BeyondNetCode.Shell.AutoMapper` · `BeyondNetCode.Shell.Observability` | Infrastructure · Presentation (startup) |

---

## Decisión

UMS adopta una estrategia de **Aislamiento de Shell Libraries** para todos los patrones compartidos de infraestructura:

1. Todos los ensamblados shell usan el namespace y convención de naming `BeyondNetCode.Shell.*`.
2. Las capas de aplicación no deben referenciar namespaces upstream como `BeyondNet.*` o nombres de repositorio fuente como `csdevlib.*`.
3. Las shell libraries deben compilar cross-platform y apuntar al baseline .NET estable actual usado por la API (`net10.0`).
4. La dirección de dependencias es estrictamente enforce como se describe abajo.

### Grafo de referencias autorizado

```
Ums.Presentation
  ├── Ums.Application
  └── Ums.Infrastructure

Ums.Infrastructure
  ├── Ums.Application
  ├── Ums.Domain
  ├── BeyondNetCode.Shell.DI
  │     └── BeyondNetCode.Shell.Aspects (transitivo)
  │           └── BeyondNetCode.Shell.Aop (transitivo)
  │                 └── BeyondNetCode.Shell.DispatchProxy (transitivo)
  └── BeyondNetCode.Shell.Logger.Serilog (transitivo vía installer)

Ums.Application
  ├── Ums.Domain
  └── BeyondNetCode.Shell.Aspects    ← contrato de atributos solo (sin DI, sin proxy runtime)

Ums.Domain
  ├── BeyondNetCode.Shell.Ddd
  │     └── BeyondNetCode.Shell.Factory (transitivo vía Ddd)
  └── BeyondNetCode.Shell.Ddd.ValueObjects
        └── BeyondNetCode.Shell.Ddd (transitivo)
```

> **Corrección del ADR original (2026-05-15):** `Ums.Domain.csproj` previamente listaba una `<ProjectReference>` directa a `BeyondNetCode.Shell.Factory`. Esto era una referencia redundante — `BeyondNetCode.Shell.Ddd` ya depende de `BeyondNetCode.Shell.Factory`, haciéndola disponible transitivamente. La referencia directa fue removida el 2026-05-24. El código de Domain debe acceder a abstracciones de factory solo a través de la capa shell DDD, no importando directamente el namespace de Factory.

### Reglas por capa

| Capa | Puede referenciar | NO puede referenciar |
|---|---|---|
| `Ums.Domain` | `BeyondNetCode.Shell.Ddd`, `BeyondNetCode.Shell.Ddd.ValueObjects` | Cualquier `BeyondNetCode.Shell.Aop.*`, `BeyondNetCode.Shell.Factory` (directo), `BeyondNetCode.Shell.Bootstrapper.*` |
| `Ums.Application` | `Ums.Domain`, `BeyondNetCode.Shell.Aspects` (contrato de atributos) | `BeyondNetCode.Shell.DispatchProxy`, `BeyondNetCode.Shell.Aop.*.Installer`, `BeyondNetCode.Shell.Factory`, `BeyondNetCode.Shell.Bootstrapper.*` |
| `Ums.Infrastructure` | Todo lo anterior + AOP installer + Bootstrapper | — |
| `Ums.Presentation` | Todas las capas + Bootstrapper para startup | — |

---

## Consecuencias

### Positivas

- UMS tiene una superficie de dependencia interna estable para patrones tácticos de DDD, Factory, AOP, y Bootstrapper.
- El código de Domain es puro de todas las preocupaciones de infraestructura — sin imports de AOP, DI, o logging.
- `Ums.Application` referencia solo el **contrato de atributo** de AOP (`BeyondNetCode.Shell.Aspects`) — los handlers declaran intención cross-cutting vía atributos sin acoplamiento a la infraestructura de proxy.
- Cambios de fuente upstream pueden ser absorbidos dentro de `src/libs/shell` sin tocar las capas de aplicación.
- El patrón `IMelLogger` (interfaz marker en Application, adapter concreto en Infrastructure) demuestra cómo aplicar el mismo principio de aislamiento a adapters cross-cutting.

### Trade-offs

- La capa shell es una dependencia arquitectónica real y debe ser versionada y revisada correspondientemente.
- Alertas de seguridad y warnings de paquetes de dependencias shell (ej., CVE de `OpenTelemetry.Api` en Bootstrapper) afectan la salud del build de UMS.
- `BeyondNetCode.Shell.Ddd` depende de `BeyondNetCode.Shell.Factory` — este acoplamiento es intencional (la construcción DDD puede usar abstracciones de factory internamente) pero significa que agregar una dependencia de Factory a Domain es suficiente para arrastrar Factory a todas partes donde Domain es referenciado.

---

## Cumplimiento

Las siguientes verificaciones son obligatorias después de cualquier cambio en las referencias de shell libraries:

```bash
# 1. Build de la solución completa
dotnet build src/apps/ums.api/Ums.sln

# 2. Ejecutar todos los test suites de shell libraries
dotnet test src/libs/shell/aop/src/BeyondNetCode.Shell.Aop.Tests/BeyondNetCode.Shell.Aop.Tests.csproj --verbosity minimal
dotnet test src/libs/shell/factory/src/BeyondNetCode.Shell.Factory.Test/BeyondNetCode.Shell.Factory.Test.csproj --verbosity minimal

# 3. Verificar pureza del Domain (sin refs AOP en Domain)
grep -r "BeyondNetCode.Shell.Aop" src/apps/ums.api/Ums.Domain/ --include="*.csproj"
# Esperado: sin output

# 4. Verificar que no hay ref directa a Factory en Domain (debe ser solo transitiva)
grep "BeyondNetCode.Shell.Factory" src/apps/ums.api/Ums.Domain/Ums.Domain.csproj
# Esperado: sin output
```

---

## Superse / Clarifica

Este ADR clarifica ADR-0029. El estándar de implementación es:

> El código de dominio de UMS no debe depender directamente de librerías de patrones externas no gestionadas. Puede depender de shell libraries poseídas por UMS que encapsulan y normalizan esos patrones. Cada shell library tiene un contrato de capa consumidora definido; ver la tabla arriba.

---

**[Índice ADR](./index.md)** | **[Overview de Shell Libraries](../shell-libraries/README.md)** | **[ADR-0060 Estrategia AOP](./0060-aop-cross-cutting-concern-strategy.md)**