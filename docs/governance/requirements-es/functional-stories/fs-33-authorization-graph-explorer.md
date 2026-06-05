# FS-33: Explorador de Grafo de Autorizacion con Simulacion What-If

> **Estado:** Pendiente de implementacion

## 1. Proposito de Negocio

Los administradores de autorizacion y los equipos de soporte necesitan entender el impacto de un cambio antes de ponerlo en produccion. UMS debe permitir explorar el grafo efectivo de autorizacion, comparar el acceso actual y el propuesto, y previsualizar el resultado de un cambio de perfil, plantilla o paquete.

## 2. Actores

| Actor | Responsabilidad |
|---|---|
| Administrador de Autorizacion | Verifica el grafo efectivo antes de aprobar cambios. |
| Ingeniero de Soporte | Diagnostica problemas de acceso usando una simulacion segura. |
| Auditor | Revisa por que el acceso cambio o no cambio. |

## 3. Precondiciones de Negocio

- El perfil, plantilla o paquete objetivo existe o esta siendo propuesto.
- El actor tiene acceso de diagnostico al alcance del tenant relevante.
- El grafo de autorizacion actual puede resolverse para el alcance seleccionado.

## 4. Flujo Funcional Principal

1. El actor abre el explorador de grafo para un perfil, plantilla o paquete de acceso.
2. El sistema renderiza el grafo efectivo actual.
3. El actor propone un cambio y activa una simulacion what-if.
4. El sistema muestra el grafo esperado despues del cambio.
5. El actor compara el acceso actual vs propuesto e identifica permisos nuevos o removidos.
6. El actor usa el resultado para aprobar, ajustar o rechazar el cambio.

## 5. Flujos Alternativos y Excepciones

### A. No Se Puede Resolver el Grafo

Si el grafo no se puede resolver para el alcance seleccionado, el sistema muestra la razon y no cambia nada.

### B. El Cambio Propuesto Amplia Demasiado el Acceso

Si el cambio propuesto amplia el acceso mas alla del alcance previsto, el sistema resalta las rutas de riesgo para que el actor ajuste la solicitud.

### C. Contexto Incompleto

Si la simulacion no tiene suficiente contexto para evaluar el grafo, el sistema lo hace visible en lugar de adivinar.

## 6. Reglas de Negocio

| Regla | Descripcion |
|---|---|
| BR-01 | El explorador no debe cambiar el acceso real por si mismo. |
| BR-02 | Los grafos actual y propuesto deben distinguirse claramente. |
| BR-03 | La vista previa debe explicar las rutas efectivas que otorgan acceso. |
| BR-04 | Un cambio que incrementa el acceso debe verse antes de aprobarse. |
| BR-05 | La vista previa del grafo debe ser auditable como accion de diagnostico. |

## 7. Criterios de Aceptacion

| # | Criterio de Aceptacion |
|---|---|
| 1 | Un actor autorizado puede previsualizar el grafo efectivo actual. |
| 2 | El actor puede simular un cambio propuesto antes de aprobarlo. |
| 3 | El explorador muestra la diferencia entre el acceso actual y el propuesto. |
| 4 | La vista previa no modifica el acceso real. |
| 5 | El resultado puede usarse para apoyar aprobacion, ajuste o rechazo. |

## 8. Requisitos Tecnicos

- Reutilizar el motor de grafo de autorizacion tanto para la evaluacion en vivo como para la simulada.
- Soportar una vista diff de solo lectura que compare el grafo resuelto antes y despues del cambio propuesto.
- Mantener el endpoint o la consulta de vista previa aislado de las operaciones de escritura.
- Emitir eventos de auditoria de diagnostico para el acceso de vista previa y simulacion.
- Preservar el alcance del tenant y los permisos durante la resolucion del grafo.

## 9. Trazabilidad

| Tipo | Referencias |
|---|---|
| Entidades de Dominio | `Profile`, `PermissionTemplate`, `Role`, `SystemSuite`, `AuditRecord` |
| Historias Funcionales | FS-07, FS-24, FS-29 |
| ADRs | ADR-0021, ADR-0071, ADR-0074 |
