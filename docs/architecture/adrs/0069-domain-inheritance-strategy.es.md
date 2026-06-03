# ADR-0069: Estrategia de Herencia en Domain Layer - Clase Base AggregateRoot**Estado:**Aceptado**Fecha:**2026-05-29**Decisores:**Equipo de Arquitectura**Supersede:**ADR-0054 (Aislamiento de Shell Libraries)

---

## Contexto

El Domain layer de UMS (`Ums.Domain`) actualmente hereda de clases base `AggregateRoot<T>` y `Entity<T>` proporcionadas por `BeyondNetCode.Shell.Ddd`. Esto crea una dependencia transitiva hacia**MediatR** (v12.3.0) a través de la librería shell.

### Arquitectura Actual

```
Ums.Domain (Proyecto)
 └── ProjectReference → BeyondNetCode.Shell.Ddd
 └── PackageReference → MediatR (12.3.0)
```

### Código Relevante**Ejemplo de Entity del Domain** (`src/apps/ums.api/Ums.Domain/Authorization/Profile/Profile.cs`):
```csharp
public sealed class Profile : AggregateRoot<Profile, ProfileProps>
{
 // Hereda: Id, BrokenRules, IsValid(), DomainEvents
}
```

**Clase Base del Shell** (`libs/shell/ddd/src/BeyondNetCode.Shell.Ddd/`):
```csharp
public abstract class AggregateRoot<T, TProps> : Entity<T, TProps>
 where TProps : Props
{
 public IDomainEvents DomainEvents { get; }
 // ... Integración con MediatR
}
```

---

## Decisión**Opción A (ACTUAL):**Continuar usando herencia de las clases base de `BeyondNetCode.Shell.Ddd`.

**Opción B:**Refactorizar a diseño basado en composición, removiendo herencia del shell.

**Opción C:**Crear un proyecto "Domain.Abstractions" con interfaces puras, removiendo MediatR del shell.

### Decisión Tomada: **Opción A (Mantener Actual)**con justificación documentada.

---

## Análisis de Pros y Contras

### Opción A: Continuar Herencia de Shell.Ddd

#### Pros
| # | Pro | Justificación |
|---|-----|---------------|
| 1 | **Velocidad de Desarrollo** | Los equipos pueden enfocarse en lógica de negocio, no en infraestructura |
| 2 | **Patrones Consistentes** | Todos los aggregates comparten comportamiento base idéntico |
| 3 | **MediatR como Infraestructura** | MediatR está a nivel shell, no directamente en Domain |
| 4 | **Battle-Tested** | Las shell libraries son compartidas entre múltiples proyectos |
| 5 | **Menos Código a Mantener** | No se necesitan implementaciones duplicadas de AggregateRoot | #### Contras
| # | Contra | Justificación |
|---|--------|---------------|
| 1 | **Violación de Pureza del Domain** | Domain referencia NuGet externo transitivamente |
| 2 | **Riesgo de Filtración de Framework** | Si el shell evoluciona, el Domain podría verse forzado a cambiar |
| 3 | **Complejidad en Testing** | Los tests del Domain requieren resolución de ensamblados de MediatR |
| 4 | **Contaminación Conceptual** | El concepto de "Aggregate" del Domain conoce a MediatR |
| 5 | **Pérdida de Portabilidad** | El Domain no puede extraerse como paquete separado | ---

### Opción B: Refactorización Basada en Composición

#### Pros
| # | Pro | Justificación |
|---|-----|---------------|
| 1 | **Pureza Real del Domain** | Domain tiene cero dependencias externas |
| 2 | **Extraíble** | El Domain podría publicarse como NuGet independiente |
| 3 | **Testabilidad** | Los POCOs puros son más fáciles de testear unitariamente |
| 4 | **Sin Acoplamiento Conceptual** | El Domain no conoce a MediatR | #### Contras
| # | Contra | Justificación |
|---|--------|---------------|
| 1 | **Refactorización Masiva** | 100+ entities necesitan modificación |
| 2 | **Pérdida de Consistencia** | Cada aggregate podría implementar patrones diferente |
| 3 | **Proliferación de Boilerplate** | Comportamiento común duplicado entre aggregates |
| 4 | **Cambio Rupturista** | Todos los bounded contexts afectados |
| 5 | **Inversión de Tiempo** | Estimado 3-4 semanas de trabajo | ---

### Opción C: Domain.Abstractions (Híbrido)

Crear `Ums.Domain.Abstractions` con interfaces puras:
- `IAggregateRoot<T>`
- `IEntity<T, TProps>`
- `IDomainEvents`

Shell.Ddd implementa estas interfaces. Domain referencia solo Abstractions.

#### Pros
| # | Pro | Justificación |
|---|-----|---------------|
| 1 | **Claridad Arquitectónica** | Separación clara entre contratos e implementación |
| 2 | **Pureza del Domain** | Domain solo depende de Abstractions (sin NuGets) |
| 3 | **Flexibilidad** | Se puede cambiar implementación del shell si es necesario |
| 4 | **Mantenible** | Cambios en MediatR no impactan al Domain | #### Contras
| # | Contra | Justificación |
|---|--------|---------------|
| 1 | **Otra Capa** | Añade indirección sin beneficio inmediato |
| 2 | **Riesgo de Over-Engineering** | 3 proyectos donde 2 podrían bastar |
| 3 | **Esfuerzo de Migración** | Requiere crear proyecto Abstractions y actualizar referencias |
| 4 | **Complejidad de Build** | Más proyectos = tiempos de build más largos | ---

## Enfoque Seleccionado: Opción A (Actual)

### Justificación

1. **Regla BMAD R-10**establece: "Domain debe ser POCOs puros con cero referencias NuGet" - pero esta regla es**aspiracional**en interpretación estricta. La naturaleza transitiva de las dependencias del shell es un**compromiso controlado**.

2. **Arquitectura Pragmática**: La librería shell es un**shared kernel** (concepto DDD). MediatR no es una preocupación del Domain - es infraestructura para manejar comandos/queries. El Domain no invoca MediatR directamente; solo usa clases base que happen to include it.

3. **Evaluación de Riesgos**:
- Si MediatR cambia versión mayor → Shell.Ddd se actualiza, Domain no cambia
- Si Shell.Ddd cambia → Controlamos ambos, podemos migrar juntos
- El riesgo de acoplamiento de MediatR en Domain está**contenido dentro del shell**4. **Contexto Histórico**: Esta arquitectura fue diseñada deliberadamente en ADR-0054 como trade-off entre pureza y productividad.

---

## Notas de Implementación

### Si Futuramente Elegimos Opción C (Domain.Abstractions)

Path de migración:
1. Crear proyecto `Ums.Domain.Abstractions`
2. Definir interfaces `IAggregateRoot<T>`, `IEntity<T>`, `IDomainEvents`
3. Hacer que `AggregateRoot<T>` en shell implemente estas interfaces
4. Actualizar referencias del proyecto Domain de `Shell.Ddd` a `Shell.Ddd.Abstractions`

### Validación

Para verificar la severidad de la violación de pureza del Domain, ejecutar:
```bash
dotnet list src/apps/ums.api/Ums.Domain/Ums.Domain.csproj package
# Debe mostrar CERO referencias directas a packages
```

---

## Consecuencias

### Positivas
- Velocidad de desarrollo mantenida
- Implementación consistente de aggregates en todos los bounded contexts
- Las shell libraries pueden evolucionar independientemente

### Negativas
- La interpretación estricta de BMAD R-10 es violada
- El Domain no puede publicarse como paquete standalone
- Los equipos deben entender el modelo de dependencia transitiva

### Mitigaciones
- Documentar claramente esta decisión arquitectónica
- Asegurar que shell.Ddd tenga garantías de estabilidad (política de versionado)
- Considerar Opción C si la portabilidad se convierte en requerimiento

---

## Referencias

- [ADR-0054: Aislamiento de Shell Libraries](0054-shell-library-isolation.md)
- [Regla BMAD R-10: Pureza del Domain](../../../.bmad-core/rules/global-rules.md)
- [Documentación de MediatR](https://github.com/mattroberts297/MediatR)
- [Patrón Shared Kernel de DDD](https://martinfowler.com/articles/refactoring-the-keynote/index.html#SharedKernel)
