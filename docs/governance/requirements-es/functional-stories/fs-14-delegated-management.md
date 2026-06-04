# FS-14: Delegación Controlada de Gestión UMS dentro del Tenant

## 1. Propósito de Negocio

Un usuario autorizado del tenant, con acceso vigente al sistema UMS, debe poder delegar a otro usuario del mismo tenant capacidades limitadas de gestión de UMS. Esta delegación permite operar tareas administrativas sin abrir escalamiento de privilegios ni administración cross-tenant.

La delegación aplica únicamente al scope interno de UMS y debe mantenerse dentro de los permisos originales del delegante.

## 2. Alcance Funcional

- La delegación ocurre siempre dentro del mismo tenant.
- El usuario delegante debe ser un usuario autorizado del tenant con acceso vigente a UMS.
- El usuario receptor debe pertenecer al mismo tenant y ser elegible para recibir permisos UMS.
- La delegación puede incluir permisos para crear usuarios, gestionar usuarios, asignar perfiles permitidos, modificar permisos permitidos dentro del alcance del delegante, y revocar o bloquear usuarios si el delegante posee esas capacidades.
- La delegación puede requerir aprobación según el tipo de permiso, el nivel de riesgo o la política del tenant.
- La delegación debe ser aprobada, trazada, auditada y revocable.

## 3. Fuera de Alcance

- Delegación cross-tenant.
- Delegación global de administración fuera del tenant.
- Delegación por `Organization`, `Department`, `System` o `Team` si esos scopes no están habilitados explícitamente para esta historia.
- Otorgar permisos que el delegante no posee.
- Convertir la delegación en un mecanismo para saltar el modelo de aprobación de UMS.

## 4. Actores

| Actor | Responsabilidad |
|---|---|
| Delegante | Concede, modifica o revoca una delegación limitada dentro de su propio alcance UMS. |
| Receptor | Ejecuta acciones de gestión UMS dentro de los permisos delegados y del tenant correspondiente. |
| Aprobador autorizado | Aprueba la delegación cuando la política del tenant lo exige. |
| Administrador superior autorizado | Puede revocar o suspender delegaciones por control o seguridad. |
| Auditor | Verifica quién delegó, qué se delegó, cuándo, por cuánto tiempo y bajo qué resultado. |

## 5. Precondiciones de Negocio

- El delegante pertenece al mismo tenant que el receptor.
- El delegante tiene acceso vigente y efectivo a UMS.
- El delegante posee los permisos que pretende delegar.
- El receptor es elegible para recibir permisos UMS.
- No existe una restricción activa que invalide la delegación por pérdida de acceso, desactivación o revocación previa.
- La política del tenant define si la delegación requiere aprobación, vigencia temporal o validación adicional.

## 6. Flujo Funcional Principal

1. El delegante abre la gestión de delegaciones UMS dentro del tenant.
2. El delegante selecciona al usuario receptor del mismo tenant.
3. El delegante define los permisos UMS a delegar.
4. El sistema valida que cada permiso solicitado exista en el perfil efectivo del delegante.
5. El sistema rechaza cualquier intento de escalamiento, auto-delegación, ciclo o delegación cross-tenant.
6. Si la política lo requiere, la delegación pasa por un flujo de aprobación.
7. Si la delegación es aprobada o activada, el receptor obtiene únicamente los permisos delegados.
8. El receptor puede ejecutar las capacidades autorizadas dentro del tenant y del alcance delegado.
9. El delegante puede modificar la delegación para agregar o quitar permisos siempre que no exceda su propio alcance.
10. El delegante o un administrador superior autorizado puede revocar la delegación.

## 7. Flujos Alternativos y Excepciones

### A. Permiso no poseído por el delegante

Si el delegante intenta otorgar un permiso que no tiene en su perfil efectivo, el sistema rechaza la operación.

### B. Receptor no elegible

Si el receptor no pertenece al mismo tenant, está bloqueado, desactivado o no cumple la elegibilidad definida, el sistema rechaza la delegación.

### C. Aprobación requerida

Si la política del tenant exige aprobación, la delegación permanece pendiente hasta obtener decisión final. La aprobación puede aceptar, limitar o rechazar la solicitud.

### D. Revocación o pérdida de autoridad del delegante

Si el delegante pierde permisos, se desactiva, cambia de perfil o pierde acceso vigente a UMS, las delegaciones derivadas deben revisarse, suspenderse o revocarse según la regla definida por la política del tenant.

### E. Ciclo o auto-delegación

Si la operación produce auto-delegación, ciclo de delegación o encadenamiento inseguro, el sistema la rechaza.

## 8. Reglas de Negocio

| Regla | Descripción |
|---|---|
| BR-01 | La delegación solo aplica al scope interno de UMS dentro del tenant. |
| BR-02 | No se permite delegación cross-tenant. |
| BR-03 | El delegante solo puede delegar permisos que ya posee en su perfil UMS efectivo. |
| BR-04 | El receptor debe pertenecer al mismo tenant y ser elegible para recibir permisos UMS. |
| BR-05 | La delegación no puede otorgar más autoridad que la del delegante. |
| BR-06 | Toda delegación debe ser trazable por tenant, delegante, receptor, permisos delegados, vigencia y estado. |
| BR-07 | La delegación puede requerir aprobación según la política o sensibilidad del permiso. |
| BR-08 | Toda creación, modificación, aprobación, activación, rechazo, revocación, suspensión o expiración debe quedar auditada. |
| BR-09 | La delegación debe poder revocarse por el delegante o por un administrador superior autorizado. |
| BR-10 | Deben prevenirse ciclos, auto-delegación y escalamiento inseguro. |
| BR-11 | Si el delegante pierde acceso o permisos, las delegaciones derivadas deben revisarse o suspenderse. |
| BR-12 | La delegación puede limitarse adicionalmente por tiempo. |

## 9. Criterios de Aceptación

| # | Criterio de Aceptación |
|---|---|
| 1 | El sistema permite delegar gestión UMS solo dentro del mismo tenant. |
| 2 | El sistema impide la delegación cross-tenant. |
| 3 | El sistema impide que el delegado reciba permisos que el delegante no posee. |
| 4 | El receptor solo obtiene capacidades UMS explícitamente delegadas. |
| 5 | La delegación puede requerir aprobación cuando la política del tenant lo define. |
| 6 | El delegante puede modificar la delegación sin exceder su propio alcance. |
| 7 | La delegación puede ser revocada por el delegante o por un administrador superior autorizado. |
| 8 | La delegación queda auditada en todas sus transiciones de estado. |
| 9 | El sistema previene auto-delegación, ciclos y escalamiento inseguro. |
| 10 | Si el delegante pierde acceso o permisos, las delegaciones derivadas se revisan o se suspenden según la política. |

## 10. Requisitos de Auditoría y Trazabilidad

- Registrar tenant, delegante, receptor, permisos delegados, alcance, vigencia, motivo y estado.
- Registrar quién aprobó, activó, modificó, revocó, rechazó o expiró la delegación.
- Registrar fecha y hora de cada transición de estado.
- Conservar evidencia del permiso original del delegante al momento de crear o modificar la delegación.
- Mantener trazabilidad de suspensión o revisión forzada cuando el delegante pierda autoridad.

## 11. Requisitos Técnicos

- Si existe una estructura técnica previa como `UserManagementDelegation`, debe alinearse a este alcance mínimo: tenant, usuario delegante, usuario receptor, permisos UMS delegados, aprobación, auditoría y revocación.
- El modelo debe validar que el conjunto de permisos delegados sea subconjunto de los permisos efectivos del delegante.
- El modelo debe soportar estado de aprobación, activación, rechazo, revocación, expiración y suspensión.
- La lógica de prevención de ciclos y auto-delegación debe resolverse en la capa de aplicación y reforzarse en el dominio.
- No introducir scopes adicionales como Organization, Department, System o Team salvo que una historia futura los habilite explícitamente.

## 12. Trazabilidad

| Tipo | Referencias |
|---|---|
| Entidades | `UserManagementDelegation`, `UserAccount`, `Profile` |
| Permisos UMS relacionados | Gestión de usuarios, bloqueo/revocación, asignación de perfiles, modificación de permisos permitidos |
| Eventos de dominio | Creación, modificación, aprobación, activación, rechazo, revocación y expiración de la delegación |
| Historias relacionadas | FS-10, FS-12, FS-21, FS-22, FS-24 |
| ADRs | ADR-0038, ADR-0044 |
