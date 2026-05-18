# Functional Story 11: Cargar y Validar Documento de Usuario

## 1. Propósito de Negocio

Los usuarios y administradores necesitan entregar documentos requeridos para que UMS valide identidad, cumplimiento y elegibilidad de acceso. El sistema debe mantener el estado documental entendible y trazable.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Usuario** | Carga su propio documento requerido. |
| **Administrador de Identidad** | Carga o revisa documentos en nombre de usuarios. |
| **Revisor de Cumplimiento** | Confirma si el documento es aceptable. | ## 3. Precondiciones de Negocio

- El usuario existe.
- El tipo de documento estáá configurado.
- El actor tiene permiso para cargar o revisar el documento.

## 4. Flujo Funcional Principal

1. El actor selecciona el tipo de documento requerido.
2. El actor carga el archivo y registra fechas de emisión y vencimiento.
3. El sistema valida que las fechas sean coherentes.
4. El sistema registra el documento y marca su estado inicial de cumplimiento.
5. El documento queda disponible para revisión y futuras verificaciones de cumplimiento.

## 5. Flujos Alternativos y Excepciones

### A. Documento Ya Vencido

Si el documento estáá vencido al momento de carga, el sistema lo registra como vencido e inicia el flujo de regularización.

### B. Archivo No Aceptable

Si el archivo estáá corrupto, no puede leerse o incumple reglas de carga, el sistema solicita cargar un archivo válido.

## 6. Reglas de Negocio

1. La fecha de vencimiento debe ser posterior a la fecha de emisión.
2. Todo documento requerido debe vincularse a un tipo de documento configurado.
3. Los documentos críticos pueden afectar el acceso al vencer.
4. Todo cambio de estado documental debe ser trazable.

## 7. Criterios de Aceptación

1. Un documento válido puede cargarse y vincularse al usuario.
2. Las fechas inválidas son rechazadas.
3. Los documentos vencidos quedan claramente marcados.
4. El documento puede usarse en flujos de cumplimiento y enforcement.

## 8. Requisitos Técnicos

> [!NOTE]
> En la implementación real de C# (base de código), los agregados de cumplimiento y aprobación están unificados bajo el espacio de nombres **[Ums.Domain.Approvals](file:///d:/Users/aarroyo/personal/sources/ums/src/apps/app-api-dotnet/Ums.Domain/Approvals/)**.

- Persistir metadatos en el Agregado Root `UserDocument`.
- Clasificar documentos mediante el Agregado Root `DocumentType`.
- Guardar ubicación del archivo y checksum para recuperación e integridad.
- Emitir eventos de dominio y auditoría por carga, validación, rechazo y cambios de estado.

## 9. Trazabilidad

- Entidades: `UserDocument` (AR), `DocumentType` (AR)
- ADRs: ADR-0045, ADR-0016
- Historias relacionadas: FS-15, FS-16
