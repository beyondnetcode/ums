# ADR-0039: RBAC/ABAC Híbrido con Motor de Compilación de Políticas

*   **Estado:** Propuesto
*   **Fecha:** 2026-05-13
*   **Autores:** Equipo de Arquitectura Senior & Product Owners

---

## 1. Contexto y Problema

El modelo de autorización actual del UMS (ADR-0012) propone RBAC/ABAC híbrido pero difiere los detalles de implementación. En un sistema multi-inquilino jerárquico con administración delegada y herencia de políticas, el motor de autorización debe:

1. **Compilar** permisos basados en roles (RBAC) y condiciones basadas en atributos (ABAC) en un grafo de políticas unificado.
2. **Resolver** la política efectiva para una tupla `(usuario, inquilino, recurso, acción, contexto)`.
3. **Evaluar** condiciones de atributos en tiempo de petición contra el contexto de sesión (tiempo, IP, dispositivo, geo, nivel de riesgo).
4. **Cachear** políticas compiladas para evitar sobrecarga de resolución repetida.
5. **Invalidar** políticas cacheadas cuando mutan los bindings de políticas o las concesiones de delegación.

Una simple búsqueda rol-permiso es insuficiente porque los permisos dependen de: el rol del usuario, las políticas heredadas del inquilino objetivo, el alcance de delegación del usuario y los atributos contextuales de la petición.

---

## 2. Decisión Arquitectónica

Implementaremos un **Motor de Compilación de Políticas** que pre-compila todas las políticas aplicables para un par usuario-inquilino en un grafo de permisos plano y evaluable en tiempo de escritura de caché. La evaluación en tiempo de petición es un escaneo lineal rápido del grafo compilado con verificaciones de condiciones ABAC.

### 2.1. Pipeline de Compilación

```
Usuario + Inquilino + Contexto
        │
        ▼
┌──────────────────────────┐
│ 1. Resolución de Roles   │ ← RBAC: coleccionar roles del usuario en el inquilino efectivo
└──────────┬───────────────┘
           ▼
┌──────────────────────────────────┐
│ 2. Colección de Policy Bindings  │ ← Recorrer cadena de ancestros vía closure table
└──────────┬───────────────────────┘
           ▼
┌──────────────────────────────────┐
│ 3. Aplicar Herencia de Políticas │ ← Resolver MANDATORY > DEFAULT > OPT_IN > NONE
└──────────┬───────────────────────┘
           ▼
┌──────────────────────────────────┐
│ 4. Filtro por Alcance Delegación │ ← Restringir al alcance de delegación efectivo
└──────────┬───────────────────────┘
           ▼
┌──────────────────────────────────┐
│ 5. Adjuntar Condiciones ABAC     │ ← Adjuntar predicados de atributos a cada permiso
└──────────┬───────────────────────┘
           ▼
┌──────────────────────────────────┐
│ 6. Compilación del Grafo         │ ← Construir lista plana ordenada de permisos
└──────────┬───────────────────────┘
           ▼
    CompiledPolicyGraph (cacheado)
```

### 2.2. Estructura del Grafo de Políticas Compilado

```csharp
public class CompiledPolicyGraph
{
    public Guid UserId { get; init; }
    public Guid EffectiveTenantId { get; init; }
    public Guid RootTenantId { get; init; }
    public DateTime CompiledAt { get; init; }
    public List<CompiledPermission> Permissions { get; init; } = new();
}

public class CompiledPermission
{
    public string ResourceCode { get; init; }
    public string ActionCode { get; init; }
    public string Effect { get; init; }       // "ALLOW" o "DENY"
    public int Priority { get; init; }
    public string Source { get; init; }       // Traza: "inherited:ENTERPRISE:policy_123"
    public AbacCondition? Condition { get; init; }
}

public class AbacCondition
{
    public string Attribute { get; init; }    // "time", "ip_range", "geo", etc.
    public string Operator { get; init; }     // "in", "between", "eq", "neq"
    public string Value { get; init; }
}
```

### 2.3. Algoritmo de Compilación

El compilador: (1) recolecta bindings de la cadena de ancestros, (2) aplica reglas de herencia (MANDATORY siempre gana, DEFAULT es anulable), (3) filtra por alcance de delegación, (4) expande políticas a permisos planos, (5) resuelve conflictos: DENY explícito siempre gana sobre ALLOW. El resultado es una lista plana de `CompiledPermission` ordenada por prioridad.

### 2.4. Evaluación en Tiempo de Petición

El evaluador: (1) obtiene el grafo compilado de caché (o lo compila en fallo de caché), (2) busca el permiso que coincide con `(resourceCode, actionCode)`, (3) si no encuentra, DENY por defecto, (4) si hay condición ABAC, la evalúa contra el contexto de la petición, (5) retorna ALLOW o DENY con la traza de origen.

### 2.5. Estrategia de Invalidación de Caché

La invalidación ocurre en dos eventos:
- **PolicyBindingMutatedEvent**: Invalida el caché para todo el subárbol del inquilino afectado.
- **DelegationMutatedEvent**: Invalida el caché para el usuario concesionario afectado.

La invalidación usa un patrón de Redis `KEYS compiled_policy:v2:*:{tenantId}` y elimina en lotes.

---

## 3. Consecuencias

### Positivas

*   **Evaluación rápida**: Escaneo lineal de una lista pre-compilada (típicamente < 100 permisos) con verificaciones ABAC opcionales.
*   **DENY dominante determinístico**: DENY explícito en cualquier nivel de la cadena de herencia siempre gana.
*   **Trazabilidad completa**: Cada `CompiledPermission` incluye un campo `Source` que documenta exactamente qué política y modo de herencia lo produjo.
*   **ABAC sin complejidad**: Las condiciones de atributos se adjuntan a permisos compilados, no a la definición de la política.

### Negativas

*   **Costo de compilación**: Compilar un grafo para una jerarquía profunda requiere 3-4 consultas DB. Mitigación: La compilación ocurre solo en fallo de caché.
*   **Amplitud de invalidación**: Una sola mutación de política invalida el caché para todo el subárbol afectado. Mitigación: Invalidación progresiva con contadores de versión.
*   **Explosión de permisos**: Un usuario con muchos roles podría tener 500+ permisos compilados. Mitigación: Las listas raramente exceden 200 entradas en la práctica.
