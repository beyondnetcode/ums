# Functional Story 8: Autenticar vía Página de Inicio Personalizable

## 1. Propósito de Negocio

Los sistemas cliente necesitan una experiencia centralizada de inicio de sesión que pueda reflejar la marca de cada tenant o sistema, manteniendo la autenticación gobernada por UMS.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Usuario Final** | Inicia sesión desde un sistema cliente. |
| **Sistema Cliente** | Redirige usuarios al login hospedado y recibe el resultado. |
| **Administrador de Tenant/Sistema** | Configura branding y comportamiento de login. |

## 3. Precondiciones de Negocio

- El sistema cliente está registrado en UMS.
- Las ubicaciones de retorno están configuradas para el sistema cliente.
- Existen configuraciones de branding/login o se usan valores por defecto.

## 4. Flujo Funcional Principal

1. El usuario inicia login desde un sistema cliente.
2. El usuario es enviado a la página hospedada de UMS.
3. UMS muestra el branding y opciones de inicio de sesión correspondientes.
4. El usuario completa autenticación mediante el método configurado.
5. UMS devuelve al usuario al sistema cliente.
6. El sistema cliente recibe el contexto autenticado y continúa la experiencia del usuario.

## 5. Flujos Alternativos y Excepciones

### A. Branding No Configurado

Si no existe branding personalizado, UMS usa la experiencia por defecto aprobada.

### B. Ubicación de Retorno Inválida

Si el sistema cliente solicita una ubicación de retorno no aprobada, UMS bloquea la redirección.

## 6. Reglas de Negocio

1. El login hospedado debe soportar branding por tenant y sistema.
2. Solo pueden usarse ubicaciones de retorno aprobadas.
3. La página de login no debe exponer branding o configuración de otro tenant.
4. La selección del método de autenticación sigue la política configurada por tenant/sistema.

## 7. Criterios de Aceptación

1. Los usuarios ven la experiencia correcta para su contexto tenant/sistema.
2. Si no hay branding, se usan valores por defecto.
3. Las ubicaciones de retorno no autorizadas son rechazadas.
4. Una autenticación exitosa devuelve al usuario al sistema cliente.

## 8. Requisitos Técnicos

- Resolver branding y comportamiento de login desde configuración de sistema.
- Soportar IdP configurado y estrategias de fallback nativo.
- Validar ubicaciones de redirect/callback.
- Emitir resultado de sesión/token aprobado tras autenticación.
- Auditar intentos exitosos y fallidos de login hospedado.

## 9. Trazabilidad

- Entidades: `SYSTEM_CONFIGURATION`, `IDP_CONFIGURATION`, `FEATURE_FLAG`
- ADRs: ADR-0020, ADR-0022
- Historias relacionadas: FS-01, FS-09, FS-13
