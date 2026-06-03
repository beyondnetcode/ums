namespace Ums.Application.Common.Notifications;

public static class NotificationTemplates
{
    public static UmsNotification PasswordReset(string recipient, string recipientName, string temporaryPassword) =>
        new(
            Recipient: recipient,
            Subject: "Restablecimiento de Contraseña — UMS",
            Body: $"""
                Hola {recipientName},

                Se ha generado una contraseña temporal para su cuenta:

                    {temporaryPassword}

                Esta clave es válida para un único inicio de sesión. Al ingresar, el sistema le solicitará establecer una nueva contraseña.

                Si no solicitó este cambio, contacte a su administrador de inmediato.

                — Equipo UMS
                """,
            RecipientName: recipientName
        );

    public static UmsNotification UserSignupRequestReceived(string tenantAdminEmail, string applicantEmail, string tenantName) =>
        new(
            Recipient: tenantAdminEmail,
            Subject: $"Nueva solicitud de acceso — {tenantName}",
            Body: $"""
                Se recibió una solicitud de registro para su organización:

                    Solicitante : {applicantEmail}
                    Organización: {tenantName}

                Ingrese al portal de administración para revisar y aprobar o rechazar la solicitud.

                — Equipo UMS
                """
        );

    public static UmsNotification UserSignupConfirmation(string applicantEmail, string tenantName) =>
        new(
            Recipient: applicantEmail,
            Subject: "Solicitud de acceso recibida — UMS",
            Body: $"""
                Su solicitud de acceso a {tenantName} fue recibida correctamente.

                El administrador de su organización la revisará y recibirá una notificación cuando sea aprobada.

                — Equipo UMS
                """
        );

    public static UmsNotification UserSignupApproved(string applicantEmail, string recipientName, string tenantName) =>
        new(
            Recipient: applicantEmail,
            Subject: $"Acceso aprobado — {tenantName}",
            Body: $"""
                Hola {recipientName},

                Su solicitud de acceso a {tenantName} fue aprobada.
                Ya puede ingresar al sistema con su correo electrónico.

                — Equipo UMS
                """,
            RecipientName: recipientName
        );

    public static UmsNotification TenantSignupRequestReceived(string superAdminEmail, string companyName, string contactEmail) =>
        new(
            Recipient: superAdminEmail,
            Subject: $"Nueva solicitud de onboarding — {companyName}",
            Body: $"""
                Se recibió una solicitud de registro de nuevo cliente:

                    Empresa  : {companyName}
                    Contacto : {contactEmail}

                Ingrese al panel de Super Admin para revisar y aprobar la solicitud.

                — Sistema UMS
                """
        );

    public static UmsNotification TenantSignupConfirmation(string contactEmail, string companyName) =>
        new(
            Recipient: contactEmail,
            Subject: "Solicitud de registro recibida — UMS",
            Body: $"""
                Hola,

                La solicitud de registro de {companyName} en la plataforma UMS fue recibida.

                Nuestro equipo la revisará y se pondrá en contacto a la brevedad para completar el proceso de onboarding.

                — Equipo BeyondNet Code
                """
        );

    public static UmsNotification UserSignupDenied(string applicantEmail, string recipientName, string tenantName, string? reason = null) =>
        new(
            Recipient: applicantEmail,
            Subject: $"Solicitud de acceso denegada — {tenantName}",
            Body: $"""
                Hola {recipientName},

                Su solicitud de acceso a {tenantName} fue denegada.
                {(reason is not null ? $"\nMotivo: {reason}\n" : string.Empty)}
                Si considera que esto es un error, comuníquese con el administrador de su organización.

                — Equipo UMS
                """,
            RecipientName: recipientName
        );

    public static UmsNotification ProfileRequestApproved(string applicantEmail, string recipientName, string tenantName, string systemName, string grantedRole) =>
        new(
            Recipient: applicantEmail,
            Subject: $"Perfil asignado — {tenantName}",
            Body: $"""
                Hola {recipientName},

                Su solicitud de perfil en {tenantName} fue aprobada.

                    Sistema: {systemName}
                    Rol    : {grantedRole}

                Ya puede acceder al sistema con los permisos asignados.

                — Equipo UMS
                """,
            RecipientName: recipientName
        );

    public static UmsNotification ProfileRequestDenied(string applicantEmail, string recipientName, string tenantName, string systemName, string? reason = null) =>
        new(
            Recipient: applicantEmail,
            Subject: $"Solicitud de perfil denegada — {tenantName}",
            Body: $"""
                Hola {recipientName},

                Su solicitud de perfil en {tenantName} para el sistema {systemName} fue denegada.
                {(reason is not null ? $"\nMotivo: {reason}\n" : string.Empty)}
                Puede enviar una nueva solicitud desde el portal o comunicarse con su administrador.

                — Equipo UMS
                """,
            RecipientName: recipientName
        );

    public static UmsNotification UserDocumentRejected(string recipientEmail, string recipientName, string documentType, string reason) =>
        new(
            Recipient: recipientEmail,
            Subject: $"Documento rechazado — {documentType}",
            Body: $"""
                Hola {recipientName},

                Su documento de tipo "{documentType}" fue rechazado por el revisor.

                Motivo: {reason}

                Por favor, cargue un nuevo documento válido para continuar con el proceso.

                — Equipo UMS
                """,
            RecipientName: recipientName
        );

    public static UmsNotification UserDocumentValidated(string recipientEmail, string recipientName, string documentType) =>
        new(
            Recipient: recipientEmail,
            Subject: $"Documento validado — {documentType}",
            Body: $"""
                Hola {recipientName},

                Su documento de tipo "{documentType}" fue validado correctamente.

                Ya puede continuar con los siguientes pasos del proceso.

                — Equipo UMS
                """,
            RecipientName: recipientName
        );

    public static UmsNotification TenantSignupApproved(string contactEmail, string companyName, string adminEmail, string temporaryPassword) =>
        new(
            Recipient: contactEmail,
            Subject: $"¡Bienvenido a UMS! — {companyName}",
            Body: $"""
                Hola,

                Su organización {companyName} fue incorporada exitosamente a la plataforma UMS.

                Credenciales de administrador:

                    Usuario   : {adminEmail}
                    Contraseña: {temporaryPassword}

                Por seguridad, cambie la contraseña en su primer inicio de sesión.

                — Equipo BeyondNet Code
                """
        );
}
