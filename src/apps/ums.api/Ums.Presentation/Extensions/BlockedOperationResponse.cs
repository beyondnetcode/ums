namespace Ums.Presentation.Extensions;

using Ums.Domain.Kernel;

/// <summary>
/// Structured API response returned when an operation is blocked by a dependency guard.
/// HTTP 409 Conflict.
/// </summary>
public sealed record BlockedOperationResponse(
    string ErrorCode,
    string Message,
    string BrokenRule,
    IReadOnlyList<BlockingDependency> BlockingDependencies);

/// <summary>
/// Maps error codes from DomainErrors to user-facing messages and broken-rule descriptions.
/// </summary>
internal static class BlockedOperationMessages
{
    private static readonly Dictionary<string, (string Message, string Rule)> _map = new()
    {
        [DomainErrors.Tenant.HasActiveUsers] = (
            "No se puede suspender el tenant porque tiene usuarios activos asociados.",
            "A tenant cannot be suspended while active users exist."),

        [DomainErrors.Tenant.HasActiveBranches] = (
            "No se puede suspender el tenant porque tiene sucursales activas.",
            "A tenant cannot be suspended while active branches exist."),

        [DomainErrors.Tenant.HasActiveIdpConfig] = (
            "No se puede suspender el tenant porque tiene proveedores de identidad activos.",
            "A tenant cannot be suspended while active identity providers exist."),

        [DomainErrors.UserAccount.HasActiveProfiles] = (
            "No se puede bloquear o eliminar el usuario porque tiene perfiles activos asignados.",
            "A user cannot be blocked or deleted while active profiles are assigned."),

        [DomainErrors.Authorization.RoleHasActiveProfiles] = (
            "No se puede desactivar el rol porque tiene perfiles activos asignados.",
            "A role cannot be deactivated while active profiles are assigned to it."),

        [DomainErrors.Authorization.RoleHasActiveChildRoles] = (
            "No se puede desactivar el rol porque tiene roles hijos activos.",
            "A role cannot be deactivated while active child roles depend on it."),

        [DomainErrors.Authorization.TemplateHasActiveProfiles] = (
            "No se puede deprecar el template porque tiene perfiles activos asociados.",
            "A permission template cannot be deprecated while active profiles are linked to it."),

        [DomainErrors.Authorization.DomainResourceHasTemplateItems] = (
            "No se puede eliminar el recurso de dominio porque está referenciado en uno o más templates.",
            "A domain resource cannot be removed while active permission template items reference it."),

        [DomainErrors.Authorization.ModuleHasActiveMenus] = (
            "No se puede eliminar el módulo porque tiene menús activos configurados.",
            "A module cannot be removed while active menus are configured within it."),
    };

    public static string GetMessage(string errorCode)
        => _map.TryGetValue(errorCode, out var v) ? v.Message : "La operación fue bloqueada por una dependencia activa.";

    public static string GetRule(string errorCode)
        => _map.TryGetValue(errorCode, out var v) ? v.Rule : "Operation blocked by active dependency.";
}
