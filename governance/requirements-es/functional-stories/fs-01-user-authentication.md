# Functional Story 1: Autenticación Corporativa vía IdP Externo

## 1. Propósito de Negocio

Los usuarios corporativos necesitan acceder a los sistemas cliente usando el proveedor de identidad confiable de su organización. UMS debe validar que el usuario estáé reconocido, activo y autorizado para iniciar una sesión segura sin exigir una contraseña separada administrada por cada aplicación.

---

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Usuario Corporativo** | Intenta iniciar sesión en un sistema cliente. |
| **Proveedor de Identidad Externo** | Confirma la identidad corporativa del usuario. |
| **UMS** | Valida la identidad contra registros activos y establece la sesión de aplicación. |
| **Administrador TI** | Puede usar acceso de emergencia cuando el proveedor externo no estáá disponible.
## 3. Precondiciones de Negocio

- El usuario existe en UMS y estáá vinculado a una referencia de identidad corporativa válida.
- La cuenta del usuario estáá activa.
- La organización tiene configurado un proveedor de identidad.

---

## 4. Flujo Funcional Principal

1. El usuario abre el portal cliente y selecciona inicio de sesión corporativo.
2. El usuario es redirigido al proveedor de identidad confiable de su organización.
3. El usuario se autentica con sus credenciales corporativas.
4. El proveedor de identidad confirma la identidad del usuario a UMS.
5. UMS verifica que la identidad pertenezca a un usuario registrado y activo.
6. UMS establece la sesión del usuario y aplica el contexto de tenant correcto.
7. El usuario ingresa a la aplicación cliente con los permisos asignados a sus perfiles.

---

## 5. Flujos Alternativos y Excepciones

### A. Proveedor de Identidad Externo No Disponible

Si el proveedor de identidad externo no estáá disponible, los usuarios estándar deben reintentar más tarde. Los administradores TI autorizados pueden usar una ruta de acceso local de emergencia cuando la política lo habilite.

### B. Identidad Corporativa No Vinculada o Inactiva

Si la identidad externa es válida pero no estáá vinculada a un usuario activo en UMS, el inicio de sesión se rechaza y se registra una advertencia de seguridad.

### C. Cuenta de Usuario Suspendida

Si el usuario existe pero estáá suspendido o terminado, el sistema bloquea el acceso y muestra un mensaje claro sobre el estado de la cuenta.

---

## 6. Reglas de Negocio

1. Una identidad corporativa por sí sola no basta; el usuario también debe estáar activo en UMS.
2. El acceso de emergencia se limita a administradores TI explícitamente autorizados.
3. Todo inicio de sesión fallido o bloqueado debe ser auditable.
4. El contexto de tenant debe establecerse antes de resolver permisos.

---

## 7. Criterios de Aceptación

1. Un usuario activo y vinculado puede iniciar sesión mediante el proveedor de identidad de su organización.
2. Un usuario sin registro activo en UMS no puede acceder a la aplicación.
3. Un usuario suspendido no puede iniciar sesión.
4. La indisponibilidad del proveedor no otorga acceso silenciosamente.
5. Los resultados de inicio de sesión quedan disponibles para auditoría y soporte.

---

## 8. Requisitos Técnicos

- Soportar OAuth 2.0 Authorization Code con PKCE para autenticación externa.
- Validar tokens firmados y claims obligatorios de identidad.
- Vincular el claim de identidad externa con la referencia de identidad en UMS.
- Establecer una sesión segura mediante cookies HTTP-only, SameSite o el mecanismo aprobado.
- Retornar fallo de autorización cuando la identidad no estáá vinculada o activa.
- Emitir eventos inmutables de auditoría para fallos y advertencias.

---

## 9. Trazabilidad

- Entidades: `USER_ACCOUNT`, `IDP_CONFIGURATION`, `PROFILE`
- ADRs: ADR-0020, ADR-0022, ADR-0026
- Historias relacionadas: FS-08, FS-09
