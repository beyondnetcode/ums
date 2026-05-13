# ðŸ§ª Functional Story 8: Autenticar vÃ­a PÃ¡gina de Inicio Personalizable

Este caso de uso detalla el flujo para centralizar la autenticaciÃ³n de usuarios mediante una pÃ¡gina de inicio de sesiÃ³n alojada (hosted) y segura en el UMS, que adapta dinÃ¡micamente su diseÃ±o (branding) y caracterÃ­sticas para cada inquilino y sistema.

---

## ðŸ›ï¸ 1. DefiniciÃ³n del Caso de Uso

| Atributo | EspecificaciÃ³n |
| :--- | :--- |
| **Nombre** | Autenticar vÃ­a PÃ¡gina de Inicio Personalizable |
| **Actor Principal** | Usuario Final, Sistema Cliente |
| **Precondiciones** | El sistema cliente estÃ¡ registrado en UMS y ha configurado URLs de redirecciÃ³n de retorno (callbacks). |
| **Postcondiciones** | El usuario es autenticado por el IdP seleccionado (o fallback nativo), y UMS lo redirige de vuelta a la aplicaciÃ³n cliente junto con un JWT firmado. |

---

## ðŸ”„ 2. Flujo de TransacciÃ³n

```mermaid
sequenceDiagram
    autonumber
    participant User as Usuario Final
    participant App as App Cliente
    participant HostedLogin as Portal UMS Login
    participant ConfigAPI as Servicio de ConfiguraciÃ³n
    participant IdP as Proveedor IdP (SSO/External)
    participant DB as DB Interna PostgreSQL (Bcrypt)

    User->>App: Clic botÃ³n "Iniciar SesiÃ³n"
    App->>HostedLogin: Redirige /oauth/v2/authorize?client_id={sys_id}&tenant_id={t_id}&redirect_uri={cb}
    HostedLogin->>ConfigAPI: Solicitar branding activo y config de sistema
    ConfigAPI-->>HostedLogin: Retornar metadata de branding
    HostedLogin->>HostedLogin: Renderizar CSS y Logo (CompilaciÃ³n HTML dinÃ¡mica)
    HostedLogin-->>User: Presentar Login Personalizado (Mostrando opciones)
    
    alt AutenticaciÃ³n Externa (IDP / SSO)
        User->>HostedLogin: Selecciona opciÃ³n SSO (ej., Entra ID)
        HostedLogin->>IdP: Delegar verificaciÃ³n
        IdP-->>HostedLogin: Ã‰xito en verificaciÃ³n y claims del usuario
    else AutenticaciÃ³n Interna (Credenciales Nativas)
        User->>HostedLogin: Ingresa Usuario y ContraseÃ±a Local
        HostedLogin->>DB: Validar hash Bcrypt
        DB-->>HostedLogin: ConfirmaciÃ³n de credenciales vÃ¡lidas
    end
    
    HostedLogin->>HostedLogin: Generar JWT firmado con contextos del tenant
    HostedLogin-->>User: RedirecciÃ³n 302 hacia redirect_uri?code={auth_code}
    User->>App: Entrega en endpoint callback con el cÃ³digo
```

### A. Flujo Principal
1. Un Usuario Final visita una aplicaciÃ³n registrada (ej. Portal SCM) y hace clic en "Iniciar SesiÃ³n".
2. La aplicaciÃ³n redirige el navegador del usuario hacia el Portal Centralizado de Login (Hosted) de UMS, enviando su `client_id` (ID Sistema), `tenant_id` y un `redirect_uri` verificado en los parÃ¡metros de consulta.
3. El Portal de Login UMS consulta el Servicio de ConfiguraciÃ³n para obtener el diseÃ±o de marca activo (logo, colores, clases CSS personalizadas) asociado con dicho Inquilino y Sistema.
4. El Servicio de ConfiguraciÃ³n resuelve la estructura utilizando el motor de resoluciÃ³n jerÃ¡rquico (aplicando sobreescrituras a nivel Tenant y Sistema).
5. La pÃ¡gina de login inyecta las propiedades de hojas de estilo, URL de logotipo y fuentes en el DOM en tiempo de ejecuciÃ³n.
6. El usuario visualiza una interfaz premium, alineada a la marca de la organizaciÃ³n, que muestra Ãºnicamente las opciones de IdP permitidas para ese Tenant (ej. "Ingresar con Microsoft Entra" o "Passkey sin ContraseÃ±a").
7. El usuario completa el flujo de autenticaciÃ³n. El Portal de Login UMS autentica al usuario contra el IdP configurado.
8. Tras la validaciÃ³n exitosa, UMS emite un JWT estÃ¡ndar y criptogrÃ¡ficamente firmado que incluye el grafo de permisos compilado y los alcances del tenant.
9. El Portal de Login redirige el navegador del usuario hacia la URL de retorno (callback `redirect_uri`) con un cÃ³digo de autorizaciÃ³n.

---

## âš™ï¸ 3. Opciones DinÃ¡micas de DiseÃ±o (Branding)

La pÃ¡gina alojada soporta las siguientes variables configurables almacenadas como JSON en la base de datos `SYSTEM_CONFIGURATION`:

```json
{
  "branding": {
    "theme": "dark",
    "primary_color": "#0F52BA",
    "logo_url": "https://cdn.logisticscorp.com/assets/logo.png",
    "custom_css_url": "https://cdn.logisticscorp.com/styles/login-override.css",
    "font_family": "Outfit, sans-serif"
  },
  "login_behaviors": {
    "show_passkey_option": true,
    "allow_remember_me": false
  }
}
```

---

## ðŸ›¡ï¸ 4. Manejo de Excepciones

### Flujo Alternativo A: URI de RedirecciÃ³n InvÃ¡lida
- Si la `redirect_uri` suministrada en la consulta no coincide con la lista blanca permitida registrada para ese sistema en el UMS, el flujo de login se aborta de inmediato. El portal muestra una pÃ¡gina segura `400 Bad Request` y levanta una alerta de seguridad.

### Flujo Alternativo B: Timeout al Obtener Branding
- Si el Servicio de ConfiguraciÃ³n no responde o agota el tiempo de espera, el Portal Login de UMS aplica el layout Global predeterminado (un tema neutro y oscuro) garantizando que los servicios de autenticaciÃ³n permanezcan completamente disponibles.
