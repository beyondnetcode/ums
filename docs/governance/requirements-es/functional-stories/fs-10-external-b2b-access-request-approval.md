# Functional Story 10: Flujo de Aprobación y Petición de Acceso Externo B2B

## 1. Propósito de Negocio

Los usuarios internos necesitan una forma controlada de solicitar acceso para socios externos de negocio, como clientes, proveedores y organizaciones aliadas. UMS debe asegurar que el acceso externo estáé justificado, aprobado, trazable y limitado al alcance correcto.

---

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Usuario Patrocinador** | Solicita y justifica el acceso para un usuario externo. |
| **Administrador PAP** | Revisa, aprueba o rechaza la solicitud. |
| **Usuario Externo** | Recibe el onboarding después de la aprobación.
## 3. Precondiciones de Negocio

- El patrocinador es un usuario corporativo interno autenticado.
- El patrocinador tiene permiso para solicitar acceso externo.
- El perfil solicitado estáá disponible para usuarios externos.

---

## 4. Flujo Funcional Principal

1. El patrocinador abre el área de gestión de accesos B2B e inicia una nueva solicitud externa.
2. El patrocinador identifica la organización externa. Si no existe, informa el nombre legal, código de referencia externo y tipo de organización.
3. El patrocinador ingresa el correo del usuario externo y selecciona un perfil permitido para externos.
4. El patrocinador registra una justificación de negocio obligatoria.
5. El sistema crea una solicitud pendiente y notifica a los aprobadores responsables.
6. Un Administrador PAP revisa la solicitud, la justificación, la organización destino y el perfil solicitado.
7. Si la solicitud corresponde, el Administrador PAP la aprueba.
8. El sistema provisiona o vincula la organización externa y prepara al usuario externo para onboarding.
9. El usuario externo recibe un mensaje seguro para completar su registro.

---

## 5. Flujos Alternativos y Excepciones

### A. Solicitud Rechazada

Si el Administrador PAP rechaza la solicitud, el patrocinador recibe el motivo del rechazo y no se otorga acceso externo.

### B. Perfil Solicitado No Permitido

Si el patrocinador solicita un perfil no permitido para usuarios externos, el sistema bloquea la solicitud y registra el intento de escalamiento de privilegios.

### C. Organización Externa Existente

Si la organización ya existe, el sistema vincula el nuevo usuario a la organización existente en lugar de crear un duplicado.

---

## 6. Reglas de Negocio

1. Todo acceso externo debe tener un patrocinador interno.
2. Todo acceso externo debe tener justificación de negocio.
3. Los usuarios externos no deben recibir perfiles administrativos internos.
4. La aprobación o rechazo debe ser trazable al administrador responsable.
5. Los usuarios externos deben permanecer aislados lógicamente dentro de la frontera de su organización.

---

## 7. Criterios de Aceptación

1. Un patrocinador puede enviar una solicitud externa completa.
2. Un Administrador PAP puede aprobar o rechazar la solicitud con un resultado visible.
3. Las solicitudes rechazadas no provisionan usuarios ni organizaciones.
4. Las organizaciones externas duplicadas se manejan sin crear registros conflictivos.
5. Los perfiles internos privilegiados no pueden asignarse a usuarios externos.

---

## 8. Requisitos Técnicos

- Persistir la solicitud como `EXTERNAL_ACCESS_REQUEST` / `APPROVAL_REQUEST` con estados pendiente, aprobado y rechazado.
- Registrar auditoría inmutable con patrocinador, aprobador, justificación, estado y timestaamps.
- Validar perfiles en el límite del servicio/API.
- Usar filtrado por tenant en capa de aplicación como control primario y PostgreSQL row-level security y politicas de base de datos como endurecimiento de infraestructura.
- Emitir eventos de aprovisionamiento y auditoría después de aprobar.
- Retornar fallo de autorización ante intentos de escalamiento de privilegios.

---

## 9. Trazabilidad

- Entidades: `APPROVAL_REQUEST`, `APPROVAL_WORKFLOW`, `USER_ACCOUNT`, `PROFILE`, `TENANT`
- ADRs: ADR-0031, ADR-0032, ADR-0038, ADR-0044
- Historias relacionadas: FS-03, FS-14
