# Functional Story 17: Mantener Roles de una Suite del Sistema

## 1. Proposito de Negocio

UMS debe permitir que los administradores de seguridad mantengan el catalogo de roles perteneciente a cada suite del sistema, para que perfiles y plantillas de permisos referencien responsabilidades comprensibles y gobernadas.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Administrador de Seguridad** | Crea y mantiene roles y su jerarquia en una suite del sistema. |
| **Dueno del Sistema** | Confirma el significado de los roles y el orden de promocion. |

## 3. Precondiciones de Negocio

- Existe una suite del sistema registrada.
- El administrador esta autorizado para mantener catalogos de autorizacion.

## 4. Flujo Funcional Principal

1. El administrador abre una suite del sistema y selecciona su vista Roles.
2. El administrador registra un rol con codigo, valor visible y descripcion.
3. El administrador puede seleccionar un rol padre para representar la jerarquia de responsabilidades.
4. UMS muestra el rol registrado y su estado activo dentro de la suite.
5. El administrador puede corregir los datos descriptivos o jerarquicos y activar o desactivar el rol.

## 5. Flujos Alternativos y Excepciones

### A. Codigo Duplicado

Si el codigo ya existe en la suite seleccionada, UMS rechaza el registro e identifica el codigo duplicado para que el administrador lo corrija.

### B. Jerarquia Invalida

Si el padre seleccionado pertenece a otra suite o genera un ciclo, UMS rechaza el cambio y explica que regla jerarquica debe corregirse.

## 6. Reglas de Negocio

1. Cada rol pertenece a una sola suite del sistema y a un tenant.
2. Cada rol debe definir `code`, `value` y `description`.
3. El codigo de rol es unico dentro de su suite del sistema.
4. Un rol raiz tiene nivel jerarquico `0`; un hijo esta exactamente un nivel debajo del padre seleccionado.
5. Una jerarquia no puede contener ciclos.
6. Los roles desactivados permanecen trazables y no se tratan como nuevas asignaciones activas.

## 7. Criterios de Aceptacion

1. El administrador puede ver Roles como vista hija de una suite seleccionada.
2. El administrador puede registrar, editar, activar y desactivar un rol.
3. Los codigos duplicados y las jerarquias invalidas muestran una razon clara para corregir.
4. Las operaciones fallidas muestran un ID de error para soporte sin exponer detalles tecnicos.
5. La interfaz no requiere ingresar GUID de rol o rol padre.

## 8. Requisitos Tecnicos

- Agregado de dominio: `Ums.Domain.Authorization.Role.Role`.
- Los comandos usan endpoints REST anidados bajo `/system-suites/{systemSuiteId}/roles`.
- Las consultas usan GraphQL `rolesBySystemSuite(systemSuiteId)`.
- SQL Server persiste `Roles` con FK a `SystemSuites` y relacion propia opcional al padre.
- El filtrado de tenant en la aplicacion es obligatorio; los controles de base de datos son resguardos secundarios.
- Los comandos de rol usan Result Pattern y emiten eventos de ciclo de vida del rol.
- Las fallas de validacion o negocio retornan causas aptas para el usuario; los detalles tecnicos inesperados quedan solo en logs Serilog/Loki correlacionados por `ErrorId`.
- La vista React se localiza en espanol e ingles y valida las respuestas en tiempo de ejecucion.

## 9. Trazabilidad

- Entidades: `SystemSuite`, `Role`, `PermissionTemplate`, `Profile`
- Historias relacionadas: FS-02, FS-04, FS-05, FS-12
- Estandares: catalogo `code/value/description`, respuesta de errores segura para usuario, regla de aislamiento por tenant
