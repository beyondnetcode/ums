# Functional Story 15: Configurar Reglas de Notificación por Vencimiento

## 1. Propósito de Negocio

Los administradores de cumplimiento necesitan advertir a los usuarios antes de que sus documentos requeridos expiren, para que puedan renovarlos a tiempo y evitar restricciónes de acceso innecesarias.

---

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Administrador de Cumplimiento** | Define tiempos y canales de notificación. |
| **Usuario** | Recibe recordatorios de renovación. |
| **Motor de Cumplimiento** | Aplica reglas activas cuando los documentos se acercan al vencimiento.
## 3. Precondiciones de Negocio

- El tipo de documento existe.
- El administrador tiene permiso para gestionar reglas de notificación.

---

## 4. Flujo Funcional Principal

1. El administrador selecciona el tipo de documento que requiere recordatorios.
2. El administrador define cuántos días antes del vencimiento debe notificarse al usuario.
3. El administrador selecciona uno o más canales de entrega.
4. El administrador proporciona una descripción clara de la regla y su impacto de negocio.
5. El sistema guarda la regla y la deja disponible para el procesamiento de cumplimiento.
6. Los usuarios reciben recordatorios según las reglas activas.

---

## 5. Flujos Alternativos y Excepciones

### A. Regla de Notificación Duplicada

Si ya existe una regla idéntica para el mismo documento, tenant, anticipación y canal, el sistema evita la duplicidad para prevenir exceso de notificaciones.

---

## 6. Reglas de Negocio

1. Las reglas de notificación deben configurarse por tenant y tipo de documento.
2. Pueden existir múltiples pasos de recordatorio para el mismo tipo de documento.
3. Toda regla debe incluir `code`, `value` y `description`.
4. El sistema debe preservar trazabilidad de las notificaciones enviadas.

---

## 7. Criterios de Aceptación

1. Un administrador puede configurar al menos tres niveles de recordatorio.
2. Las reglas duplicadas son bloqueadas.
3. Los usuarios reciben recordatorios antes del vencimiento cuando aplican reglas.
4. El comportamiento de notificación puede variar por tenant.

---

## 8. Requisitos Técnicos

> [!NOTE]
> En la implementación real de C# (base de código), `NotificationRule` es una Entidad hija encapsulada dentro del Agregado **[DocumentType](file:///d:/Users/aarroyo/personal/sources/ums/src/apps/app-api-dotnet/Ums.Domain/Approvals/DocumentType/DocumentType.cs)**, bajo el espacio de nombres unificado **[Ums.Domain.Approvals](file:///d:/Users/aarroyo/personal/sources/ums/src/apps/app-api-dotnet/Ums.Domain/Approvals/)**.

- Persistir reglas como parte del Agregado Root `DocumentType`.
- Campos obligatorios: `Code`, `Value` (JSON con tiempos y canales), `Description`.
- Aplicar unicidad por `Code`, `TenantId` y `DocumentTypeId`.
- Registrar trazabilidad de entrega de notificaciones.
- Soportar invalidación de caché cuando cambian reglas de notificación.

---

## 9. Trazabilidad

- Entidades: `DocumentType` (AR), `NotificationRule` (Entidad Hija), `UserDocument` (AR)
- ADRs: ADR-0045, ADR-0016
- Historias relacionadas: FS-11, FS-16
