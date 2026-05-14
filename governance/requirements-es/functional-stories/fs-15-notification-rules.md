# 📘 Functional Story 15: Configurar Reglas de Notificación por Vencimiento

Este documento especifica el flujo para parametrizar las alertas preventivas que se envían a los usuarios antes de que su documentación expire.

---

## 🏛️ 1. Definición del Caso de Uso

| Atributo | Especificación |
| :--- | :--- |
| **Nombre** | Configurar Reglas de Notificación por Vencimiento |
| **Actor Principal** | Administrador de Cumplimiento (Compliance) |
| **Precondiciones** | El `DOCUMENT_TYPE` está configurado. |
| **Postcondiciones** | Las reglas de notificación son persistidas y listas para ser procesadas por el motor de alertas. |

---

## 🔄 2. Flujo de Transacción

### A. Flujo Principal
1.  El administrador selecciona el tipo de documento a parametrizar.
2.  Define el número de días de anticipación para la alerta (ej. 30 días, 15 días, 5 días).
3.  Selecciona el canal de envío (Email, In-App, SMS).
4.  El sistema permite añadir múltiples pasos de notificación para el mismo documento.
5.  El sistema persiste las reglas en la tabla `NOTIFICATION_RULE`.
6.  El proceso en segundo plano (Worker) utilizará estas reglas para comparar contra la `ExpirationDate` de los documentos de los usuarios y disparar las alertas.

---

## 🛡️ 3. Flujos Alternativos y Manejo de Excepciones

### Flujo Alternativo A: Duplicidad de Reglas
*   Si se intenta configurar una regla idéntica (mismo día y canal) para el mismo Tenant/Documento, el sistema solicita modificar el parámetro para evitar spam de notificaciones.

---

## 📋 4. Detalles de Implementación

### Entidades Involucradas
- `NOTIFICATION_RULE`
- `DOCUMENT_TYPE`

### Criterios de Aceptación
1.  El sistema debe permitir configurar al menos 3 niveles de alerta por defecto.
2.  Las notificaciones deben ser filtrables por Tenant para permitir políticas regionales diferenciadas.
3.  El sistema debe registrar la trazabilidad de cada notificación enviada con éxito.
