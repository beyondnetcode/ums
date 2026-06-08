# Functional Story 18: Gestionar la Contraseña Local de un Usuario

## 1. Propósito de Negocio

UMS debe permitir que un administrador autorizado establezca o rote la contraseña local de un usuario cuya estrategia de autenticación es interna, para que pueda acceder de forma segura cuando no se utiliza federación de identidad externa.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Administrador de Identidad** | Establece o rota una contraseña local para una cuenta elegible. |
| **Usuario** | Utiliza la credencial local después de recibirla por un canal seguro aprobado. |

## 3. Precondiciones de Negocio

- La cuenta de usuario existe en el tenant seleccionado.
- La cuenta usa autenticación interna y no está vinculada a un proveedor de identidad externo.
- El administrador está autorizado para gestionar credenciales de usuarios.

## 4. Flujo Funcional Principal

1. El administrador abre una cuenta de usuario y selecciona la vista Credenciales.
2. UMS indica si existe una contraseña local configurada y la fecha de su última rotación.
3. El administrador elige establecer o rotar la contraseña.
4. El administrador ingresa y confirma una contraseña temporal que cumple la regla indicada.
5. UMS guarda la nueva credencial local activa y marca cualquier credencial anterior como histórica.
6. UMS confirma que la contraseña fue actualizada sin mostrar valores secretos.

## 5. Flujos Alternativos y Excepciones

### A. Cuenta Federada

Si el usuario se autentica mediante un proveedor de identidad externo, UMS no ofrece mantenimiento de contraseña local e indica que la gestión corresponde al proveedor configurado.

### B. Contraseña Temporal Inválida

Si la contraseña no cumple la regla mínima indicada o la confirmación es distinta, UMS explica qué debe corregirse antes de guardar.

### C. Falla Durante la Rotación

Si la operación no puede completarse, UMS muestra una razón clara cuando está disponible y un identificador de soporte, sin exponer detalles técnicos.

## 6. Reglas de Negocio

1. Un usuario federado no debe tener una contraseña local activa.
2. Un usuario local puede tener como máximo una contraseña activa.
3. Establecer una nueva contraseña desactiva la credencial activa anterior y conserva su historial para auditoría.
4. Los valores secretos de contraseña y sus representaciones protegidas nunca se muestran ni se incluyen en mensajes operativos.
5. Una contraseña temporal debe contener al menos 12 caracteres.

## 7. Criterios de Aceptación

1. El administrador puede ver la gestión de contraseña en la vista Credenciales del usuario seleccionado.
2. Para una cuenta interna, el administrador puede establecer o rotar una contraseña local.
3. Para una cuenta federada, la interfaz explica que la gestión corresponde al proveedor externo y no permite ingreso local.
4. La interfaz muestra únicamente estado de la credencial y fecha de última rotación, nunca una contraseña o valor protegido.
5. Los errores de validación y operación son comprensibles e incluyen un ID de soporte cuando la API lo retorna.

## 8. Requisitos Técnicos

- `PasswordCredential` permanece como entidad propiedad del agregado `UserAccount` en el bounded context Identity.
- Los comandos son REST-first mediante `POST /user-accounts/{userAccountId}/passwords`; la solicitud lleva una contraseña temporal en texto claro sobre el transporte seguro y la API la protege con BCrypt antes de persistirla.
- Las consultas pueden exponer `hasActivePassword` y `passwordUpdatedAtUtc`; `PasswordHash` y los identificadores de credenciales históricas no deben exponerse al cliente web.
- SQL Server con EF Core persiste credenciales dentro del límite transaccional de `UserAccount`.
- El filtrado de tenant en aplicacion permanece como control primario; PostgreSQL row-level security y politicas de base de datos permanecen como resguardos secundarios.
- Los errores operativos siguen el estándar de respuesta segura y correlacionan el diagnóstico Serilog/Loki mediante `ErrorId`.
- La vista React está localizada en español e inglés y valida la regla mínima de contraseña antes de ejecutar el comando.

## 9. Trazabilidad

- Entidades: `UserAccount`, `PasswordCredential`, `IdentityProvider`
- Historias relacionadas: FS-01, FS-03, FS-08, FS-09
- ADR relacionado: ADR-0066 contrato de errores accionables para usuario
- Actualización de diagrama: `docs/domain-es/identity/password-credential.md`, ciclo de vida y propiedad de UserAccount, motivada por FS-18
