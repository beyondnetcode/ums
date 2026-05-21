import { sharedTranslations } from './namespaces/shared.translations';
import { identityTranslations } from './namespaces/identity.translations';

export type Language = 'es' | 'en';

const translations = {
  es: {
    ...sharedTranslations.es,
    ...identityTranslations.es,

    // Login Screen
    sessionCredentials: 'Credenciales de Sesión',
    devUsername: 'Usuario Desarrollador',
    emailAddress: 'Correo Electrónico',
    devRole: 'Rol de Permiso del Desarrollador',
    loginBtn: 'Ingresar al Portal',
    mockDevProfiles: 'Perfiles de Desarrollo',
    mockDevSubtitle: 'Cargue rápidamente cuentas pre-registradas para simular permisos y controles de acceso.',
    activeSessionTitle: 'Sesión de Desarrollador Activa',
    activeSessionSubtitle: 'Cada solicitud API es auditada y firmada con su contexto de tenant.',
    labelUsername: 'Usuario',
    labelEmail: 'Correo Electrónico',
    labelRole: 'Rol Asignado',
    labelXUserId: 'Valor X-User-Id',
    profileSwitched: 'Perfil Cambiado',
    profileSwitchedMsg: (name: string, id: string) => `X-User-Id asignado a '${name}' [ID: ${id}...].`,
    sessionAuth: 'Sesión Autenticada',
    sessionAuthMsg: (username: string, role: string) => `Ingresado como '${username}' con rol: ${role}.`,
    devProfile3: 'Auditor Estándar',

    // Profile Screen
    profileTitle: 'Perfil del Desarrollador y Diagnósticos',
    profileSubtitle: 'Estado del workspace, cabeceras de autorización y registros de telemetría.',
    securityRole: 'Rol de Seguridad',
    xUserIdHeader: 'Cabecera X-User-Id',
    xLanguageHeader: 'Cabecera X-Language',
    connectionState: 'Estado de Conexión',
    synced: 'Sincronizado con API Local',
    diagnosticTitle: 'Disparar Telemetría de Diagnóstico UI',
    workspaceHealth: 'Salud del Workspace',
    apiPort: 'Puerto API',
    dbStatus: 'Estado de Base de Datos',
    architecture: 'Arquitectura',
    auditTrail: 'Registro de Auditoría Reciente',
    noAudit: 'Sin notificaciones de auditoría.',
    devTestSuccess: 'Evento de Éxito en Desarrollo',
    devTestWarning: 'Evento de Advertencia en Desarrollo',
    devTestError: 'Evento de Error en Desarrollo',
    devTestInfo: 'Evento de Información en Desarrollo',
    devTestMsg: (time: string) => `Verificación de telemetría activada a las ${time}`,
  },

  en: {
    ...sharedTranslations.en,
    ...identityTranslations.en,

    // Login Screen
    sessionCredentials: 'Session credentials',
    devUsername: 'Developer Username',
    emailAddress: 'Email Address',
    devRole: 'Developer Permission Role',
    loginBtn: 'Log in to portal',
    mockDevProfiles: 'Mock Dev Profiles',
    mockDevSubtitle: 'Quickly load pre-registered system accounts to simulate permissions, security headers, and domain access controls.',
    activeSessionTitle: 'Secure Developer Session Active',
    activeSessionSubtitle: 'Every API request is audited and signed with your custom tenant context.',
    labelUsername: 'Username',
    labelEmail: 'Email Address',
    labelRole: 'Assigned Role',
    labelXUserId: 'X-User-Id Value',
    profileSwitched: 'Dev Profile Switched',
    profileSwitchedMsg: (name: string, id: string) => `Active X-User-Id mapped to '${name}' [ID: ${id}...].`,
    sessionAuth: 'Session Authenticated',
    sessionAuthMsg: (username: string, role: string) => `Successfully logged in as '${username}' with role: ${role}.`,
    devProfile3: 'Standard Auditor',

    // Profile Screen
    profileTitle: 'Developer Profile & Diagnostics',
    profileSubtitle: 'Live workspace status, authorization headers, and telemetry logs.',
    securityRole: 'Security Role',
    xUserIdHeader: 'X-User-Id Header',
    xLanguageHeader: 'X-Language Header',
    connectionState: 'Connection State',
    synced: 'Synced with Local API',
    diagnosticTitle: 'Trigger UI Diagnostic Telemetry',
    workspaceHealth: 'Workspace Health',
    apiPort: 'API Port Listener',
    dbStatus: 'Database Status',
    architecture: 'Architecture',
    auditTrail: 'Recent Audit Trail',
    noAudit: 'No audit notifications recorded.',
    devTestSuccess: 'Developer Success Event',
    devTestWarning: 'Developer Warning Event',
    devTestError: 'Developer Error Event',
    devTestInfo: 'Developer Info Event',
    devTestMsg: (time: string) => `Self-triggered development telemetry verification at ${time}`,
  },
} as const;

export type TranslationKeys = keyof typeof translations.es;
export type Translations = typeof translations.es;

export default translations;
