using Ums.Domain.Authorization.Graph;

namespace Ums.Application.Identity.Auth.Commands;

/// <summary>
/// Authenticate a user by tenant code + credentials.
/// Returns a complete AuthorizationGraph on success.
/// </summary>
public sealed record AuthenticateUserCommand(
    string  TenantCode,
    string  Username,
    string  Password,
    string  ClientIp,
    bool    RememberMe = false) : ICommand<AuthenticateUserResult>;

/// <summary>Result of a successful authentication — graph + raw JWT.</summary>
public sealed record AuthenticateUserResult(
    AuthorizationGraph Graph,
    string             Token,
    string             TokenType,
    int                ExpiresIn,      // seconds
    DateTime           IssuedAt,
    string             SerializedGraph,  // pre-serialized in the tenant's default format
    string             GraphFormat);     // "JSON" | "XML" | "YAML" | "CSV"
