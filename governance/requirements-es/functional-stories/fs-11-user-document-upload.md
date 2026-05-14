# 📘 Functional Story 11: Cargar y Validar Documento de Usuario

Este documento especifica el flujo para la carga, registro de metadatos y validación de documentos requeridos para el cumplimiento (compliance) y la identidad del usuario.

---

## 🏛️ 1. Definición del Caso de Uso

| Atributo | Especificación |
| :--- | :--- |
| **Nombre** | Cargar y Validar Documento de Usuario |
| **Actor Principal** | Usuario / Administrador de Identidad |
| **Precondiciones** | El usuario existe y el `DOCUMENT_TYPE` está configurado en el sistema. |
| **Postcondiciones** | El documento se almacena en el servidor de archivos y el registro operativo es persistido con su fecha de vigencia. |

---

## 🔄 2. Flujo de Transacción

### A. Flujo Principal
1.  El actor selecciona el tipo de documento a cargar (ej. Identidad, Contrato, Certificado).
2.  El actor adjunta el archivo físico y proporciona la fecha de emisión (`IssueDate`) y expiración (`ExpirationDate`).
3.  El sistema genera un hash de integridad (`Checksum`) del archivo.
4.  El sistema almacena el archivo en el servidor de archivos/cloud storage y obtiene la ruta de acceso (`FileStoragePath`).
5.  El sistema registra la entidad `USER_DOCUMENT` vinculando el archivo al usuario y clasificándolo por su tipo.
6.  El sistema marca el estado del documento como `VALID` (si está dentro de rango de fecha) o `PENDING_RENEWAL`.

---

## 🛡️ 3. Flujos Alternativos y Manejo de Excepciones

### Flujo Alternativo A: Documento Expirado en Carga
*   Si la fecha de expiración ingresada es menor a la fecha actual, el sistema registra el documento con estado `EXPIRED` y dispara una notificación inmediata de regularización.

### Flujo Alternativo B: Error de Integridad
*   Si el cálculo del Checksum falla o se detecta un archivo corrupto, el sistema aborta la persistencia y solicita al usuario cargar el archivo nuevamente.

---

## 📋 4. Detalles de Implementación

### Entidades Involucradas
- `USER_DOCUMENT`
- `DOCUMENT_TYPE`

### Criterios de Aceptación
1.  El sistema debe rechazar archivos que superen el tamaño máximo configurado.
2.  La fecha de expiración debe ser obligatoriamente posterior a la fecha de emisión.
3.  El registro en la base de datos debe contener la ruta exacta del servidor de archivos para su recuperación posterior.
