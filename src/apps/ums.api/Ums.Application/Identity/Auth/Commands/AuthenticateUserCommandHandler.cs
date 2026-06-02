using Ums.Application.Authorization.Graph;
using Ums.Application.Authorization.Graph.Serializers;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Authorization.Graph;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Auth;

namespace Ums.Application.Identity.Auth.Commands;

/// <summary>
/// Orchestrates the complete authentication flow:
///   1. Resolve tenant by TenantCode
///   2. Validate user status
///   3. IAuthMethodResolver → Local or IDP
///   4. Execute Local (BCrypt) or IDP auth strategy
///   5. IAuthorizationGraphBuilder.BuildAsync → full graph
///   6. Serialize graph in tenant's configured format (Shell.Factory)
///   7. Record audit event (IAuditService)
/// </summary>
public sealed class AuthenticateUserCommandHandler
    : ICommandHandler<AuthenticateUserCommand, AuthenticateUserResult>
{
    private readonly ITenantRepository          _tenantRepo;
    private readonly IUserAccountRepository     _userRepo;
    private readonly IAuthMethodResolver        _methodResolver;
    private readonly ILocalAuthStrategy         _localStrategy;
    private readonly IIdpAuthStrategy           _idpStrategy;
    private readonly IAuthorizationGraphBuilder _graphBuilder;
    private readonly IAuthGraphFormatProvider   _formatProvider;
    private readonly IAuthorizationGraphSerializer _defaultSerializer;
    private readonly IAuthAuditService          _auditService;

    public AuthenticateUserCommandHandler(
        ITenantRepository          tenantRepo,
        IUserAccountRepository     userRepo,
        IAuthMethodResolver        methodResolver,
        ILocalAuthStrategy         localStrategy,
        IIdpAuthStrategy           idpStrategy,
        IAuthorizationGraphBuilder graphBuilder,
        IAuthGraphFormatProvider   formatProvider,
        IAuthorizationGraphSerializer defaultSerializer,
        IAuthAuditService          auditService)
    {
        _tenantRepo        = tenantRepo;
        _userRepo          = userRepo;
        _methodResolver    = methodResolver;
        _localStrategy     = localStrategy;
        _idpStrategy       = idpStrategy;
        _graphBuilder      = graphBuilder;
        _formatProvider    = formatProvider;
        _defaultSerializer = defaultSerializer;
        _auditService      = auditService;
    }

    public async Task<Result<AuthenticateUserResult>> Handle(
        AuthenticateUserCommand command,
        CancellationToken       cancellationToken)
    {
        var tenantId = Guid.Empty;
        var userId   = Guid.Empty;

        try
        {
            // ── 1. Tenant ──────────────────────────────────────────────────────
            var tenant = await _tenantRepo.GetByCodeAsync(
                command.TenantCode.ToUpperInvariant(), cancellationToken);

            if (tenant is null)
            {
                await RecordFailureAsync(tenantId, userId, command, "AUTH_002: Tenant not found",
                    "Unknown", cancellationToken);
                return Result<AuthenticateUserResult>.Failure("AUTH_002: Tenant not found.");
            }

            if (tenant.Props.Status != Domain.Enums.TenantStatus.Active)
            {
                await RecordFailureAsync(tenantId, userId, command, "AUTH_003: Tenant inactive",
                    "Unknown", cancellationToken);
                return Result<AuthenticateUserResult>.Failure("AUTH_003: Tenant is not active.");
            }

            tenantId = tenant.Props.Id.GetValue();

            // ── 2. Auth method resolution ──────────────────────────────────────
            var methodResult = await _methodResolver.ResolveAsync(
                tenantId,
                command.AccessScope,
                cancellationToken);
            if (methodResult.IsFailure)
            {
                await RecordFailureAsync(tenantId, userId, command, methodResult.Error,
                    "Unknown", cancellationToken);
                return Result<AuthenticateUserResult>.Failure(methodResult.Error);
            }

            var authMethod = methodResult.Value;
            var methodName = authMethod.Type.ToString();

            // ── 3. Authenticate ────────────────────────────────────────────────
            if (authMethod.Type == AuthMethodType.Local)
            {
                return await AuthenticateLocalAsync(
                    command, tenant, tenantId, authMethod, methodName, cancellationToken);
            }
            else
            {
                return await AuthenticateIdpAsync(
                    command, tenant, tenantId, authMethod, methodName, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            await RecordFailureAsync(tenantId, userId, command,
                $"Unexpected error: {ex.Message}", "Unknown", cancellationToken);
            return Result<AuthenticateUserResult>.Failure(
                "An unexpected error occurred during authentication.");
        }
    }

    // ── Local Auth Flow ───────────────────────────────────────────────────────

    private async Task<Result<AuthenticateUserResult>> AuthenticateLocalAsync(
        AuthenticateUserCommand command,
        Ums.Domain.Identity.Tenant.Tenant tenant,
        Guid              tenantId,
        AuthMethod        authMethod,
        string            methodName,
        CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(
            Email.Create(command.Username), cancellationToken);

        if (user is null || user.Props.TenantId.GetValue() != tenantId)
        {
            await RecordFailureAsync(tenantId, Guid.Empty, command,
                "AUTH_006: Invalid credentials", methodName, cancellationToken);
            return Result<AuthenticateUserResult>.Failure("AUTH_006: Invalid username or password.");
        }

        if (user.Props.Status != Domain.Enums.UserStatus.Active)
        {
            await RecordFailureAsync(tenantId, user.Props.Id.GetValue(), command,
                "AUTH_005: User not active", methodName, cancellationToken);
            return Result<AuthenticateUserResult>.Failure(
                "AUTH_005: User account is not active. Contact your administrator.");
        }

        var authResult = _localStrategy.Authenticate(user, command.Password);
        if (authResult.IsFailure)
        {
            user.RecordAuthenticationAttempt(false, authResult.Error,
                command.ClientIp, ActorId.Create("auth:system"));
            await _userRepo.UpdateAsync(user, cancellationToken);
            await _userRepo.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            await RecordFailureAsync(tenantId, user.Props.Id.GetValue(), command,
                "AUTH_006: Invalid credentials", methodName, cancellationToken);
            return Result<AuthenticateUserResult>.Failure("AUTH_006: Invalid username or password.");
        }

        user.RecordAuthenticationAttempt(true, "Login successful",
            command.ClientIp, ActorId.Create("auth:system"));
        await _userRepo.UpdateAsync(user, cancellationToken);
        await _userRepo.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return await BuildResultAsync(
            user, tenantId, tenant.Props.Code.GetValue(),
            authMethod, command, methodName, cancellationToken);
    }

    // ── IDP Auth Flow ─────────────────────────────────────────────────────────

    private async Task<Result<AuthenticateUserResult>> AuthenticateIdpAsync(
        AuthenticateUserCommand command,
        Ums.Domain.Identity.Tenant.Tenant tenant,
        Guid              tenantId,
        AuthMethod        authMethod,
        string            methodName,
        CancellationToken cancellationToken)
    {
        var idpResult = await _idpStrategy.AuthenticateAsync(
            tenantId, command.Password, authMethod.Provider!, cancellationToken);

        if (idpResult.IsFailure)
        {
            await RecordFailureAsync(tenantId, Guid.Empty, command,
                idpResult.Error, methodName, cancellationToken);
            return Result<AuthenticateUserResult>.Failure(idpResult.Error);
        }

        var externalId = idpResult.Value;
        var user = await _userRepo.GetByEmailAsync(
            Email.Create(externalId.Email), cancellationToken);

        if (user is null)
        {
            await RecordFailureAsync(tenantId, Guid.Empty, command,
                "AUTH_004: IDP user has no UMS account", methodName, cancellationToken);
            return Result<AuthenticateUserResult>.Failure(
                "AUTH_004: Authenticated IDP user has no UMS account. Contact your administrator.");
        }

        if (user.Props.Status != Domain.Enums.UserStatus.Active)
        {
            await RecordFailureAsync(tenantId, user.Props.Id.GetValue(), command,
                "AUTH_005: User not active", methodName, cancellationToken);
            return Result<AuthenticateUserResult>.Failure(
                "AUTH_005: User account is not active. Contact your administrator.");
        }

        user.RecordAuthenticationAttempt(true, "IDP login successful",
            command.ClientIp, ActorId.Create("auth:system"));
        await _userRepo.UpdateAsync(user, cancellationToken);
        await _userRepo.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return await BuildResultAsync(
            user, tenantId, tenant.Props.Code.GetValue(),
            authMethod, command, methodName, cancellationToken);
    }

    // ── Shared Result Builder ─────────────────────────────────────────────────

    private async Task<Result<AuthenticateUserResult>> BuildResultAsync(
        Ums.Domain.Identity.UserAccount.UserAccount user,
        Guid              tenantId,
        string            tenantCode,
        AuthMethod        authMethod,
        AuthenticateUserCommand command,
        string            methodName,
        CancellationToken cancellationToken)
    {
        var graphResult = await _graphBuilder.BuildAsync(user, tenantId, authMethod, cancellationToken);
        if (graphResult.IsFailure)
        {
            await RecordFailureAsync(tenantId, user.Props.Id.GetValue(), command,
                graphResult.Error, methodName, cancellationToken);
            return Result<AuthenticateUserResult>.Failure(graphResult.Error);
        }

        var graph  = graphResult.Value;
        var format = await _formatProvider.GetDefaultFormatAsync(tenantId, cancellationToken);
        var serialized = _defaultSerializer.Serialize(graph);

        await _auditService.RecordAuthEventAsync(new AuthAuditEvent(
            UserId:      user.Props.Id.GetValue(),
            TenantId:    tenantId,
            TenantCode:  tenantCode,
            AuthMethod:  methodName,
            EventType:   "Auth.Login.Success",
            Succeeded:   true,
            ClientIp:    command.ClientIp,
            IdpProvider: authMethod.Provider?.Props.Name.GetValue()), cancellationToken);

        return Result<AuthenticateUserResult>.Success(new AuthenticateUserResult(
            Graph:           graph,
            Token:           string.Empty,
            TokenType:       "Bearer",
            ExpiresIn:       graph.EffectiveConfig.AccessTokenDurationMs / 1000,
            IssuedAt:        graph.GeneratedAt,
            SerializedGraph: serialized,
            GraphFormat:     format));
    }

    private async Task RecordFailureAsync(
        Guid              tenantId,
        Guid              userId,
        AuthenticateUserCommand command,
        string            reason,
        string            methodName,
        CancellationToken cancellationToken)
    {
        try
        {
            await _auditService.RecordAuthEventAsync(new AuthAuditEvent(
                UserId:        userId,
                TenantId:      tenantId,
                TenantCode:    command.TenantCode,
                AuthMethod:    methodName,
                EventType:     "Auth.Login.Failure",
                Succeeded:     false,
                ClientIp:      command.ClientIp,
                FailureReason: reason), cancellationToken);
        }
        catch
        {
            // Audit errors must not surface as authentication errors
        }
    }
}
