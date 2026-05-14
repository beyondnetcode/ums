# Functional Story 9: Autenticación Adaptativa Multifactor y Sin Contraseña

## 1. Propósito de Negocio

UMS debe reforzar la autenticación cuando el riesgo o la política del tenant lo requiera, permitiendo acceso moderno sin contraseña cuando esté permitido.

## 2. Actores

| Actor | Responsabilidad |
| :--- | :--- |
| **Usuario Final** | Completa verificación adicional o inicio sin contraseña. |
| **Administrador de Tenant** | Configura la política de autenticación. |
| **UMS** | Evalúa política y riesgo antes de otorgar acceso. |

## 3. Precondiciones de Negocio

- El usuario tiene cuenta en UMS.
- La política del tenant permite o exige MFA/passwordless.
- El usuario tiene o puede registrar un método aprobado de verificación.

## 4. Flujo Funcional Principal

1. El usuario inicia autenticación.
2. UMS evalúa la política del tenant y el contexto de riesgo actual.
3. Si se requiere verificación adicional, UMS solicita al usuario completar el desafío aprobado.
4. Si el acceso sin contraseña está permitido, UMS permite autenticarse con el método registrado.
5. Tras verificación exitosa, UMS establece la sesión del usuario.
6. El usuario continúa hacia el sistema destino con los permisos correspondientes.

## 5. Flujos Alternativos y Excepciones

### A. Registro MFA Requerido

Si el usuario no tiene método registrado y la política exige uno, UMS guía al usuario por el registro antes de otorgar acceso.

### B. Verificación Fallida

Si el usuario falla el desafío, UMS bloquea el acceso y registra el intento.

### C. Riesgo Elevado

Si el contexto de riesgo es elevado, UMS exige verificación más fuerte antes de permitir acceso.

## 6. Reglas de Negocio

1. La política del tenant determina si MFA o passwordless es opcional, obligatorio o deshabilitado.
2. El riesgo elevado puede exigir verificación adicional.
3. Una verificación fallida no debe crear sesión válida.
4. Registro e intentos de autenticación deben ser auditables.

## 7. Criterios de Aceptación

1. Los usuarios pueden completar MFA cuando es requerido.
2. Los usuarios pueden usar acceso sin contraseña cuando está permitido y registrado.
3. La verificación fallida bloquea la creación de sesión.
4. El riesgo elevado activa verificación más fuerte.

## 8. Requisitos Técnicos

- Soportar mecanismos MFA y passwordless aprobados, incluyendo WebAuthn/passkeys cuando esté configurado.
- Evaluar política de seguridad tenant/sistema antes de emitir sesión.
- Persistir estado de enrolamiento y resultados de verificación.
- Emitir eventos de auditoría por registro, desafío, éxito, fallo y escalamiento de riesgo.
- Retornar fallo de autorización cuando la verificación requerida no se completa.

## 9. Trazabilidad

- Entidades: `USER_ACCOUNT`, `IDP_CONFIGURATION`, `SYSTEM_CONFIGURATION`
- ADRs: ADR-0026
- Historias relacionadas: FS-01, FS-08
