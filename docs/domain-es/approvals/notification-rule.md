# NotificationRule — Arquitectura de Agregado

**Contexto Delimitado:** Aprobaciones  
**Raíz de Agregado:** `NotificationRule`  
**Módulo:** `Ums.Domain.Approvals.NotificationRule`  
**Estado:** Producción

---

## 1. Visión General del Agregado

### Propósito
`NotificationRule` es un Aggregate Root independiente que define cuándo, cómo y a quién se notifica por vencimiento o incumplimiento documental. Se reutiliza desde otros agregados, pero no depende del ciclo de vida de `DocumentType`.

### Responsabilidad de Negocio
- Definir umbrales de notificación previos al vencimiento.
- Configurar canales permitidos por regla.
- Mantener el ciclo de vida de la regla de forma independiente.

### Raíz de Agregado
`NotificationRule` no es entidad hija de `DocumentType`. `DocumentType` solo puede referenciarla por identificador.

### Invariantes
1. `DaysBefore` debe ser mayor que cero.
2. La colección de canales no puede ser nula ni vacía.
3. El código debe ser único dentro del scope configurado.

### Comandos / Casos de Uso
| Comando | Descripción |
|---|---|
| `ConfigureNotificationRuleCommand` | Crear o actualizar la regla independiente |
| `RemoveNotificationRuleCommand` | Eliminar la regla independiente |

---

