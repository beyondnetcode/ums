# ðŸ§ª Functional Story 9: AutenticaciÃ³n Adaptativa Multifactor y Sin ContraseÃ±a

Este documento especifica el flujo de transacciÃ³n detallado, actores, precondiciones, postcondiciones y manejo de excepciones para el registro y autenticaciÃ³n de usuarios empleando AutenticaciÃ³n Multifactor (MFA) y/o Passkeys Sin ContraseÃ±a (WebAuthn), bajo el control de la evaluaciÃ³n dinÃ¡mica y adaptativa de riesgos de la **estrategia spec-driven AI BMAD-METHOD**.

---

## ðŸ›ï¸ 1. DefiniciÃ³n del Caso de Uso

| Atributo | EspecificaciÃ³n |
| :--- | :--- |
| **Nombre** | AutenticaciÃ³n Adaptativa Multifactor y Sin ContraseÃ±a |
| **Actor Principal** | Usuario Final (ej. Operador B2B, Analista de Negocio), Sistema Cliente |
| **Precondiciones** | El usuario tiene una cuenta en el UMS. La polÃ­tica de seguridad del Tenant permite o exige MFA/Passkeys. |
| **Postcondiciones** | La identidad del usuario es verificada, se establece una sesiÃ³n segura y se retorna un grafo de autorizaciÃ³n hecho a la medida. |

---

## ðŸ”„ 2. Flujo de TransacciÃ³n

### A. Secuencia: Onboarding & Registro de MFA

Este flujo ocurre cuando un usuario inicia sesiÃ³n por primera vez o la polÃ­tica lo obliga a inscribir un nuevo segundo factor (TOTP/Passkey).

```mermaid
sequenceDiagram
    autonumber
    actor User as Usuario Final
    participant Portal as Portal Login UMS
    participant Gateway as API Gateway .NET 8 Auth
    participant MFA as Servicio MFA (PDP)
    participant DB as PostgreSQL

    User->>Portal: Enviar credenciales (Usuario/Clave)
    Portal->>Gateway: POST /v1/auth/login
    Gateway->>MFA: CheckEnforcePolicy(user_id, tenant_id)
    MFA-->>Gateway: Retorno: ONBOARDING_REQUIRED (Sin factores)
    Gateway-->>Portal: 403 Forbidden { state_token, status: "MFA_ONBOARDING_REQUIRED" }
    Portal->>User: Mostrar pantalla inscripciÃ³n MFA (TOTP o Passkey)
    
    alt ElecciÃ³n: TOTP (App Autenticador)
        User->>Portal: Seleccionar inscripciÃ³n TOTP
        Portal->>Gateway: POST /v1/auth/mfa/enroll { factor_type: "TOTP", state_token }
        Gateway->>MFA: GenerateOtpSecret(user_id)
        MFA-->>Gateway: Retornar clave secreta y URL otpauth
        Gateway-->>Portal: 200 OK { secret_key, qr_code_url }
        Portal->>User: Mostrar QR y solicitar cÃ³digo validaciÃ³n
        User->>Portal: Escanear QR e ingresar cÃ³digo 6 dÃ­gitos
        Portal->>Gateway: POST /v1/auth/mfa/confirm-enroll { factor_type: "TOTP", code, state_token }
        Gateway->>MFA: VerifyAndActivateFactor(code, secret)
        MFA->>DB: Guardar ENROLLED_MFA_FACTOR verificado (encriptado)
        MFA-->>Gateway: Retornar 8 CÃ³digos RecuperaciÃ³n
        Gateway-->>Portal: 200 OK { recovery_codes }
        Portal->>User: Entregar cÃ³digos de respaldo y finalizar sesiÃ³n
    end
```

### B. Secuencia: Inicio de SesiÃ³n Asertivo con Passkey (Passwordless)

Este flujo detalla cÃ³mo un usuario ingresa directamente utilizando sensores biomÃ©tricos nativos de su dispositivo (Huella, FaceID) o llaves de seguridad fÃ­sicas (FIDO2) sin emplear contraseÃ±as.

```mermaid
sequenceDiagram
    autonumber
    actor User as Usuario Final
    participant Portal as Portal App Cliente
    participant Gateway as API Gateway .NET 8 Auth
    participant WebAuthn as Servicio WebAuthn
    participant DB as PostgreSQL

    User->>Portal: Clic "Login con Passkey" e ingresar correo
    Portal->>Gateway: POST /v1/auth/passkey/assertion-options { email, tenant_id }
    Gateway->>WebAuthn: GenerateAssertionOptions(email)
    WebAuthn->>DB: Consultar claves pÃºblicas en ENROLLED_MFA_FACTOR
    DB-->>WebAuthn: Retornar referencias de clave pÃºblica
    WebAuthn->>WebAuthn: Generar desafÃ­o aleatorio seguro (32 bytes)
    WebAuthn-->>Portal: 200 OK { challenge, credential_ids }
    Portal->>User: Invoca browser credentials.get() (FaceID/Huella)
    User-->>Portal: Aprueba biomÃ©tricamente y firma el desafÃ­o
    Portal->>Gateway: POST /v1/auth/passkey/verify-assertion { assertion_response }
    Gateway->>WebAuthn: VerifyAssertion(assertion_response)
    WebAuthn->>WebAuthn: Validar firma con clave pÃºblica y desafÃ­o
    alt VerificaciÃ³n Exitosa
        WebAuthn-->>Gateway: Ã‰xito en VerificaciÃ³n
        Gateway-->>Portal: Emitir JWT + Grafo Permisos
        Portal-->>User: Acceso autorizado al Dashboard
    else VerificaciÃ³n Fallida
        WebAuthn-->>Gateway: Lanza InvalidSignatureException
        Gateway-->>Portal: 401 Unauthorized [ERR_PASSKEY_FAILED]
    end
```

---

## ðŸ›¡ï¸ 3. Flujos Alternativos y Manejo de Excepciones

### Flujo Alternativo A: RecuperaciÃ³n ante PÃ©rdida del Factor Primario (Auto-Servicio)
- Si el usuario pierde su dispositivo MFA (ej., telÃ©fono con TOTP) o no puede acceder a su biometrÃ­a:
    1. En la pantalla de MFA, el usuario hace clic en **"Usar CÃ³digo de RecuperaciÃ³n"**.
    2. EnvÃ­a uno de los cÃ³digos de respaldo alfabÃ©ticos de 8 caracteres guardados durante el registro inicial.
    3. El gateway somete la entrada a un hash Bcrypt y lo compara con los valores guardados en la tabla `RECOVERY_CODES`.
    4. Tras la validaciÃ³n, el gateway:
        - Marca el cÃ³digo de recuperaciÃ³n como `USED` (inactivÃ¡ndolo permanentemente).
        - Genera una sesiÃ³n temporal de vida corta.
        - Redirige al usuario directamente a la pantalla de administraciÃ³n de factores MFA para que registre un nuevo dispositivo.
        - Emite un evento `UserRecoveryCodeUsedEvent` al registro de auditorÃ­a.

### Flujo Alternativo B: IntervenciÃ³n de EvaluaciÃ³n de Riesgos Adaptativa (Step-Up)
- Si el contexto de login de un usuario se considera sospechoso (ej. inicio de sesiÃ³n desde un nuevo paÃ­s o una huella digital de navegador irreconocible):
    1. El motor `AdaptiveRiskEvaluator` califica el riesgo como `ALTO` (`HIGH`).
    2. El gateway intercepta el flujo normal y exige un desafÃ­o Step-Up MFA, ignorando cualquier estado de "Recordar Dispositivo".
    3. Se obliga al usuario a completar la autenticaciÃ³n con el factor mÃ¡s robusto disponible (ej., Passkeys biomÃ©tricas).
    4. Si tiene Ã©xito, se disminuye el nivel de riesgo, el nuevo dispositivo se marca como de confianza y la sesiÃ³n es establecida.
    5. Si falla el desafÃ­o luego de 3 intentos, el gateway aborta la autenticaciÃ³n, envÃ­a la amenaza a Grafana Loki y bloquea temporalmente la cuenta por 15 minutos.

### ExcepciÃ³n 1: SincronizaciÃ³n de Reloj Desfasada en TOTP
- Si un usuario envÃ­a un cÃ³digo TOTP vÃ¡lido que fracasa al validar debido a un desfase horario en su telÃ©fono:
    1. El servicio `MfaService` evalÃºa las ventanas de tiempo colindantes (Â±30 segundos).
    2. Si el cÃ³digo acierta en una de las ventanas adyacentes, el sistema acepta el login, emite un *warning* sobre la des-sincronizaciÃ³n y automÃ¡ticamente re-ajusta el *offset* temporal interno del servidor asociado a ese dispositivo.
    3. Si falla en todas las ventanas adyacentes, rechaza el intento con `401 Unauthorized` [cÃ³digo de error: `ERR_INVALID_MFA_CODE`].

---

## ðŸ“‹ 4. Referencia del Modelo Operativo Principal
Las capacidades multifactor y sin contraseÃ±as configurables por Tenant se encuentran completamente declaradas y versionadas bajo el **Modelo de ConfiguraciÃ³n de Comportamiento del Sistema**. Para mÃ¡s detalles de la especificaciÃ³n tÃ©cnica, visite **[mfa-passwordless-security-spec.md](../../04-artifacts/mfa-passwordless-security-spec.md)**.
