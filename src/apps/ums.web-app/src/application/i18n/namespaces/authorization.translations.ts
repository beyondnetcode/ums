export const authorizationTranslations = {
  es: {
    // Context labels
    authorizationContext: 'Contexto de Autorización',
    systemSuites: 'Suites del Sistema',
    systemSuiteMaintenance: 'Mantenimiento de Suites del Sistema',
    systemSuiteMaintenanceSubtitle: 'Explore y gestione los suites del sistema, sus módulos y catálogos de acciones.',

    // System Suite Form
    createSystemSuiteTitle: 'Crear Nuevo Suite del Sistema',
    systemSuiteCode: 'Código del Suite',
    systemSuiteCodeHelper: 'Solo caracteres alfanuméricos en mayúsculas y guiones bajos.',
    systemSuiteName: 'Nombre del Suite',
    systemSuiteNameHelper: 'Nombre descriptivo del suite del sistema.',
    registerSystemSuiteBtn: 'Registrar Suite',

    // System Suite list / controls
    bySystemSuiteId: 'Por ID del Suite (GUID)',
    noSystemSuitesFound: 'No se encontraron suites del sistema con el criterio seleccionado.',
    noSystemSuitesTitle: 'Sin suites del sistema',
    selectSystemSuiteToView: 'Seleccione un suite del sistema en el panel para cargar sus detalles.',
    systemSuiteDetails: 'Detalles del Suite',

    // Tabs
    modules: 'Módulos',
    actions: 'Acciones',
    noModulesConfigured: 'No hay módulos configurados para este suite del sistema.',
    noActionsConfigured: 'No hay acciones configuradas para este suite del sistema.',

    // Status controls
    maintenanceBtn: 'Mantenimiento',
    deprecateBtn: 'Descontinuar',
    maintenance: 'Mantenimiento',
    deprecated: 'Descontinuado',

    // Notifications
    notifSystemSuiteCreated: 'Suite del Sistema Creado',
    notifSystemSuiteCreatedMsg: (id: string) => `Suite del sistema registrado con ID: ${id}.`,
    notifSystemSuiteCreateFailed: 'Error al Crear Suite del Sistema',
    notifSystemSuiteUpdated: 'Suite del Sistema Actualizado',
    notifSystemSuiteUpdatedMsg: (name: string) => `Los datos del suite '${name}' fueron actualizados.`,
    notifStatusChanged: 'Estado Cambiado',
    notifStatusChangedMsg: 'El estado del suite del sistema fue actualizado.',
    notifStatusChangeFailed: 'Error al Cambiar Estado',
    notifStatusChangeFailedMsg: 'No se pudo actualizar el estado del suite del sistema.',
    notifStatusSetTo: (status: string) => `Estado del suite actualizado a ${status}.`,
  },
  en: {
    // Context labels
    authorizationContext: 'Authorization Context',
    systemSuites: 'System Suites',
    systemSuiteMaintenance: 'System Suite Maintenance',
    systemSuiteMaintenanceSubtitle: 'Explore and manage system suites, their modules, and action catalogs.',

    // System Suite Form
    createSystemSuiteTitle: 'Create New System Suite',
    systemSuiteCode: 'System Suite Code',
    systemSuiteCodeHelper: 'Uppercase alphanumeric characters and underscores only.',
    systemSuiteName: 'System Suite Name',
    systemSuiteNameHelper: 'Descriptive name for the system suite.',
    registerSystemSuiteBtn: 'Register System Suite',

    // System Suite list / controls
    bySystemSuiteId: 'By System Suite ID (GUID)',
    noSystemSuitesFound: 'No system suites found with the selected criteria.',
    noSystemSuitesTitle: 'No system suites',
    selectSystemSuiteToView: 'Select a system suite in the panel to load its details.',
    systemSuiteDetails: 'System Suite Details',

    // Tabs
    modules: 'Modules',
    actions: 'Actions',
    noModulesConfigured: 'No modules configured for this system suite.',
    noActionsConfigured: 'No actions configured for this system suite.',

    // Status controls
    maintenanceBtn: 'Maintenance',
    deprecateBtn: 'Deprecate',
    maintenance: 'Maintenance',
    deprecated: 'Deprecated',

    // Notifications
    notifSystemSuiteCreated: 'System Suite Created',
    notifSystemSuiteCreatedMsg: (id: string) => `System suite registered with ID: ${id}.`,
    notifSystemSuiteCreateFailed: 'System Suite Creation Failed',
    notifSystemSuiteUpdated: 'System Suite Updated',
    notifSystemSuiteUpdatedMsg: (name: string) => `System suite '${name}' data was updated successfully.`,
    notifStatusChanged: 'Status Changed',
    notifStatusChangedMsg: 'The system suite status was successfully updated.',
    notifStatusChangeFailed: 'Status Change Failed',
    notifStatusChangeFailedMsg: 'Could not update the system suite status.',
    notifStatusSetTo: (status: string) => `The system suite status was successfully set to ${status}.`,
  },
} as const;
