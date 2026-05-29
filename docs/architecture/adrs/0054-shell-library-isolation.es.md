# ADR-0054: Aislamiento de Shell Libraries para Patrones DDD, Factory, AOP y Bootstrapper

**Estado:** Aceptado
**Fecha:** 2026-05-15
**Enmendado:** 2026-05-24 — grafo de dependencias corregido; alcance extendido para incluir `Ums.Shell.Aop` y `Ums.Shell.Bootstrapper`
**Responsable de Decisión:** Arquitectura
**Relacionados:**
- [ADR-0060: Estrategia de Concerns Cross-Cutting AOP](./0060-aop-cross-cutting-concern-strategy.md)
- [Guías de Desarrollo de Shell Libraries](../shell-libraries/README.md)

---

## Contexto

UMS incluye librerías de infraestructura reutilizables bajo `src/libs/shell`. Estas librerías se originaron de fuentes externas o de referencia, pero UMS no debe exponer namespaces upstream ni convenciones de repositorio en código de aplicación.

El ADR-0029 anterior describía "Primitivas DDD nativas de C# (sin librería externa)". Esa redacción ya no es precisa: la posición correcta es que UMS **posee su superficie de dependencia de dominio** a través de las shell libraries `Ums.Shell.*`. Las capas de aplicación consumen la abstracción shell de UMS, no la identidad fuente upstream.

A partir de 2026-05-24, la capa shell incluye cuatro grupos de librerías:

| Grupo | Proyectos | Capas consumidoras |
|---|---|---|
| `Ums.Shell.Ddd` | `Ums.Shell.Ddd` · `Ums.Shell.Ddd.ValueObjects` | Domain (directo) |
| `Ums.Shell.Factory` | `Ums.Shell.Factory` · `Ums.Shell.Factory.Installer` | Domain (transitivo vía Ddd) · Infrastructure (directo) |
| `Ums.Shell.Aop` | `Ums.Shell.Aop` · `Ums.Shell.Aop.DispatchProxy` · `Ums.Shell.Aop.Aspects` · `Ums.Shell.Aop.Aspects.Logger.Serilog` · `Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer` | Application (contrato de atributos) · Infrastructure (wiring DI + adapters) |
| `Ums.Shell.Bootstrapper` | `Ums.Shell.Bootstrapper` · `Ums.Shell.Bootstrapper.DependencyInjection` · `Ums.Shell.Bootstrapper.AutoMapper` · `Ums.Shell.Bootstrapper.Observability` | Infrastructure · Presentation (startup) |

---

## Decisión

UMS adopta una estrategia de **Aislamiento de Shell Libraries** para todos los patrones compartidos de infraestructura:

1. Todos los ensamblados shell usan el namespace y convención de naming `Ums.Shell.*`.
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
  ├── Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer
  │     └── Ums.Shell.Aop.Aspects (transitivo)
  │           └── Ums.Shell.Aop (transitivo)
  │                 └── Ums.Shell.Aop.DispatchProxy (transitivo)
  └── Ums.Shell.Aop.Aspects.Logger.Serilog (transitivo vía installer)

Ums.Application
  ├── Ums.Domain
  └── Ums.Shell.Aop.Aspects    ← contrato de atributos solo (sin DI, sin proxy runtime)

Ums.Domain
  ├── Ums.Shell.Ddd
  │     └── Ums.Shell.Factory (transitivo vía Ddd)
  └── Ums.Shell.Ddd.ValueObjects
        └── Ums.Shell.Ddd (transitivo)
```

> **Corrección del ADR original (2026-05-15):** `Ums.Domain.csproj` previamente listaba una `<ProjectReference>` directa a `Ums.Shell.Factory`. Esto era una referencia redundante — `Ums.Shell.Ddd` ya depende de `Ums.Shell.Factory`, haciéndola disponible transitivamente. La referencia directa fue removida el 2026-05-24. El código de Domain debe acceder a abstracciones de factory solo a través de la capa shell DDD, no importando directamente el namespace de Factory.

### Reglas por capa

| Capa | Puede referenciar | NO puede referenciar |
|---|---|---|
| `Ums.Domain` | `Ums.Shell.Ddd`, `Ums.Shell.Ddd.ValueObjects` | Cualquier `Ums.Shell.Aop.*`, `Ums.Shell.Factory` (directo), `Ums.Shell.Bootstrapper.*` |
| `Ums.Application` | `Ums.Domain`, `Ums.Shell.Aop.Aspects` (contrato de atributos) | `Ums.Shell.Aop.DispatchProxy`, `Ums.Shell.Aop.*.Installer`, `Ums.Shell.Factory`, `Ums.Shell.Bootstrapper.*` |
| `Ums.Infrastructure` | Todo lo anterior + AOP installer + Bootstrapper | — |
| `Ums.Presentation` | Todas las capas + Bootstrapper para startup | — |

---

## Consecuencias

### Positivas

- UMS tiene una superficie de dependencia interna estable para patrones tácticos de DDD, Factory, AOP, y Bootstrapper.
- El código de Domain es puro de todas las preocupaciones de infraestructura — sin imports de AOP, DI, o logging.
- `Ums.Application` referencia solo el **contrato de atributo** de AOP (`Ums.Shell.Aop.Aspects`) — los handlers declaran intención cross-cutting vía atributos sin acoplamiento a la infraestructura de proxy.
- Cambios de fuente upstream pueden ser absorbidos dentro de `src/libs/shell` sin tocar las capas de aplicación.
- El patrón `IMelLogger` (interfaz marker en Application, adapter concreto en Infrastructure) demuestra cómo aplicar el mismo principio de aislamiento a adapters cross-cutting.

### Trade-offs

- La capa shell es una dependencia arquitectónica real y debe ser versionada y revisada correspondientemente.
- Alertas de seguridad y warnings de paquetes de dependencias shell (ej., CVE de `OpenTelemetry.Api` en Bootstrapper) afectan la salud del build de UMS.
- `Ums.Shell.Ddd` depende de `Ums.Shell.Factory` — este acoplamiento es intencional (la construcción DDD puede usar abstracciones de factory internamente) pero significa que agregar una dependencia de Factory a Domain es suficiente para arrastrar Factory a todas partes donde Domain es referenciado.

---

## Cumplimiento

Las siguientes verificaciones son obligatorias después de cualquier cambio en las referencias de shell libraries:

```bash
# 1. Build de la solución completa
dotnet build src/apps/ums.api/Ums.sln

# 2. Ejecutar todos los test suites de shell libraries
dotnet test src/libs/shell/aop/src/Ums.Shell.Aop.Tests/Ums.Shell.Aop.Tests.csproj --verbosity minimal
dotnet test src/libs/shell/factory/src/Ums.Shell.Factory.Test/Ums.Shell.Factory.Test.csproj --verbosity minimal

# 3. Verificar pureza del Domain (sin refs AOP en Domain)
grep -r "Ums.Shell.Aop" src/apps/ums.api/Ums.Domain/ --include="*.csproj"
# Esperado: sin output

# 4. Verificar que no hay ref directa a Factory en Domain (debe ser solo transitiva)
grep "Ums.Shell.Factory" src/apps/ums.api/Ums.Domain/Ums.Domain.csproj
# Esperado: sin output
```

---

## Superse / Clarifica

Este ADR clarifica ADR-0029. El estándar de implementación es:

> El código de dominio de UMS no debe depender directamente de librerías de patrones externas no gestionadas. Puede depender de shell libraries poseídas por UMS que encapsulan y normalizan esos patrones. Cada shell library tiene un contrato de capa consumidora definido; ver la tabla arriba.

---

**[Índice ADR](./index.md)** | **[Overview de Shell Libraries](../shell-libraries/README.md)** | **[ADR-0060 Estrategia AOP](./0060-aop-cross-cutting-concern-strategy.md)**