# Alcance y Límites del Producto - Sistema de Gestión de Usuarios (UMS)

## 1. Capacidades Incluidas
El Sistema de Gestión de Usuarios (UMS) es un **núcleo de seguridad soberano y abstracto, completamente desacoplado de cualquier suite de aplicaciones específica**. Está diseñado para gobernar identidades, configuraciones de comportamiento y permisos granulares para cualquier sistema descendente genérico, comunicándose puramente a través de:
- **APIs REST síncronas y servicios gRPC** para evaluar permisos, obtener configuraciones y autenticar contextos.
- **Buses de Mensajería Asíncronos (Eventos)** para difundir actualizaciones de configuración, trazas de auditoría y mutaciones de ciclo de vida.

El UMS gestiona las siguientes capacidades funcionales clave:

### A. Gobernanza de Organización y Multi-Tenancy
- Definición de **Organizaciones (Tenants)** y sus estructuras jerárquicas.
- Aislamiento estricto de base de datos mediante **Seguridad a Nivel de Fila (RLS) en PostgreSQL**.
- Administración delegada: permite a los administradores de tenant gestionar sus propios usuarios y asignaciones locales.

### B. Autorización Granular y Motor de Perfiles
- Gestión de **Sistemas Cliente** registrados en el UMS.
- Compilación en tiempo real del **Grafo de Permisos** jerárquico para los usuarios.
- Gestión de **Plantillas de Políticas** que vinculan bloques de autorización reutilizables a múltiples perfiles.
- Procesamiento de reglas de **Precedencia de Denegación Explícita** en el motor de autorización central.

### C. Inyección Dinámica de Layout de Interfaz
- Esquemas de metadatos dinámicos para **Menús, Opciones y Acciones** mapeados a roles del sistema.
- APIs que exponen estructuras de navegación personalizadas a los clientes basadas en los permisos activos.

### D. Auditoría de Negocio Inmutable
- Seguimiento automático de todas las mutaciones de datos críticas (creación, modificación, eliminación de usuarios/roles) mediante suscriptores de base de datos.
- Registros de auditoría seguros, aislados e inmutables.

### E. Portal Web Administrativo (Policy Administration Point - PAP)
- **Panel de Control Central**: Mantenimiento dinámico (CRUDs) de Organizaciones (Tenants), Sistemas, Módulos, Menús, Opciones, Acciones, Perfiles y Roles.
- **Monitores de Sesión Activa y Telemetría**: Auditoría en tiempo real de intentos de autenticación, ratios de acierto de caché y evicciones de caché Redis.
- **Visualizador de Grafo Resolver**: Visualización interactiva del grafo de autorización compilado para contextos específicos de usuario/tenant.

### F. Plataforma de Configuración y Feature Management
- **Motor de Configuración Multi-IdP**: Registro por tenant y sistema de Proveedores de Identidad con reglas de prioridad/respaldo, autenticación híbrida y gestión de credenciales cifradas.
- **Configuración de Comportamiento del Sistema**: Configuración JSON multi-tenant, versionada y auditable que controla la estrategia de autenticación, políticas de sesión, MFA, flujos de onboarding, branding y habilitación de módulos.
- **Marco de Feature Flags**: Motor de conmutación centralizado que soporta flags booleanos, de variante y porcentaje con segmentación multidimensional (tenant, org, rol, usuario, ambiente, sistema). Soporta estrategias de despliegue Canary y Beta.

### G. Portal de Login Hospedado Personalizable
- **Experiencia de Login Soberana**: Portal de autenticación centralizado y seguro alojado por UMS al que los sistemas cliente redirigen, con soporte para **WebAuthn/Passkeys** y autenticación multifactor adaptativa (TOTP, Email, SMS).
- **Inyección de CSS Dinámico y Branding**: Las pantallas de login se estilizan dinámicamente en tiempo de ejecución según el contexto del tenant o sistema usando logotipos, hojas de estilo y colores obtenidos del motor de configuración.
- **Emisión de Tokens Zero-Trust**: Procesa la autenticación de forma segura a través de IdPs federados o almacenes nativos, emitiendo JWTs firmados y estandarizados.

---

## 2. Capacidades Fuera de Alcance
Para evitar la expansión descontrolada del alcance y mantener el UMS especializado, los siguientes dominios están estrictamente **Fuera de Alcance**:

- **Almacén de Usuarios Soberano / Hashing de Contraseñas** *(Adaptador Opcional)*: Por defecto, la verificación de credenciales se delega a un Proveedor de Identidad externo (ej. Zitadel, Azure AD, Okta). El almacenamiento interno de credenciales basado en Bcrypt se soporta como un **adaptador opcional y conectable** (`IAuthenticationPort`).
- **Gestión de Facturación y Suscripciones**: El procesamiento de tarjetas de crédito, facturación de tenants y límites de niveles de suscripción son gestionados por un microservicio de Facturación separado.
- **Gateways Directos de Correo/SMS**: La entrega de notificaciones o mensajes de verificación se delega a adaptadores de Twilio/SendGrid; UMS solo dispara los eventos.
- **Operaciones de Dominio Transaccionales**: Las operaciones core de las aplicaciones (como planificación de fletes en TMS o stock en WMS) están completamente aisladas del UMS.
