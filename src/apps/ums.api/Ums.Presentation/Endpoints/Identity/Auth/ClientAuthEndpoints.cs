namespace Ums.Presentation.Endpoints.Identity.Auth;

using Ums.Application.Authorization.Graph;
using Ums.Application.Authorization.Graph.Serializers;
using Ums.Application.Identity.Auth.Commands;
using Ums.Domain.Authorization.Graph;
using Ums.Presentation.Services;
using BeyondNetCode.Shell.Factory.Interfaces;

/// <summary>
/// Public external API for client system authentication.
///
/// POST /api/v1/client/authenticate
///   - No session cookie (stateless for external systems)
///   - Returns the full AuthorizationGraph serialized in the tenant's configured format
///   - JWT embeds graph as claims (permissions, scopes, features)
///   - Format can be overridden via ?format=xml or Accept header
///
/// This endpoint is the primary integration point for downstream systems
/// that need to authenticate users and receive their full authorization context.
/// </summary>
public static class ClientAuthEndpoints
{
    public static void MapClientAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/client")
            .WithTags("Client Authentication");

        group.MapPost("/authenticate", HandleClientAuthenticateAsync)
            .WithName("ClientAuthenticate")
            .WithSummary("Authenticate a user and receive the full authorization graph")
            .AllowAnonymous()
            .Produces<ClientAuthResponse>(StatusCodes.Status200OK)
            .Produces<ClientAuthErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ClientAuthErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ClientAuthErrorResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleClientAuthenticateAsync(
        ClientAuthRequest      request,
        IMediator              mediator,
        IJwtTokenService       jwtService,
        IAuthGraphFormatProvider formatProvider,
        IFactory               factory,
        HttpContext            httpContext,
        CancellationToken      cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TenantCode) ||
            string.IsNullOrWhiteSpace(request.Username)   ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.Json(new ClientAuthErrorResponse("AUTH_001",
                "TenantCode, Username and Password are required.", null),
                statusCode: StatusCodes.Status400BadRequest);
        }

        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var command = new AuthenticateUserCommand(
            TenantCode: request.TenantCode.Trim().ToUpperInvariant(),
            Username:   request.Username.Trim(),
            Password:   request.Password,
            ClientIp:   clientIp,
            AccessScope: Ums.Domain.Identity.Auth.AuthAccessScope.ExternalApi,
            RememberMe: false);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = GetStatusCode(result.Error);
            return Results.Json(new ClientAuthErrorResponse(
                    ExtractCode(result.Error), CleanMessage(result.Error), null),
                statusCode: statusCode);
        }

        var authResult = result.Value;
        var graph      = authResult.Graph;

        // Resolve format: query param > Accept header > tenant default (already in authResult)
        var requestedFormat = request.Format?.ToUpperInvariant()
            ?? GetFormatFromAcceptHeader(httpContext.Request.Headers.Accept.ToString());
        var format     = authResult.GraphFormat;
        var serialized = authResult.SerializedGraph;

        // Override serialization if caller explicitly requests a different format
        if (!string.IsNullOrWhiteSpace(requestedFormat) &&
            requestedFormat != format)
        {
            var tenantId = graph.Context.Tenant.Id;
            var resolvedFormat = await formatProvider.ResolveFormatAsync(
                tenantId, requestedFormat, cancellationToken);

            if (resolvedFormat != format)
            {
                var criteria   = new GraphSerializationCriteria(resolvedFormat);
                var serializer = factory
                    .Create<GraphSerializationCriteria, IAuthorizationGraphSerializer>(criteria)
                    .SingleOrDefault();

                if (serializer is not null)
                {
                    format     = resolvedFormat;
                    serialized = serializer.Serialize(graph);
                }
            }
        }

        // Generate graph JWT
        var token = jwtService.GenerateSemanticGraphToken(graph);

        var response = new ClientAuthResponse(
            Token:       token,
            TokenType:   "Bearer",
            ExpiresIn:   authResult.ExpiresIn,
            IssuedAt:    authResult.IssuedAt,
            Format:      format,
            Graph:       serialized,
            RequestId:   httpContext.TraceIdentifier);  // correlates with audit record

        // Set Content-Type to match the graph format
        httpContext.Response.Headers["X-Graph-Format"] = format;
        return Results.Ok(response);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static int GetStatusCode(string error) => error switch
    {
        var e when e.StartsWith("AUTH_002") => StatusCodes.Status404NotFound,
        var e when e.StartsWith("AUTH_003") => StatusCodes.Status400BadRequest,
        var e when e.StartsWith("AUTH_004") => StatusCodes.Status404NotFound,
        var e when e.StartsWith("AUTH_005") => StatusCodes.Status401Unauthorized,
        var e when e.StartsWith("AUTH_006") => StatusCodes.Status401Unauthorized,
        var e when e.StartsWith("AUTH_011") => StatusCodes.Status503ServiceUnavailable,
        var e when e.StartsWith("AUTH_012") => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status401Unauthorized,
    };

    private static string ExtractCode(string error)
    {
        var idx = error.IndexOf(':');
        return idx > 0 ? error[..idx].Trim() : "AUTH_000";
    }

    private static string CleanMessage(string error)
    {
        var idx = error.IndexOf(':');
        return idx > 0 ? error[(idx + 1)..].Trim() : error;
    }

    private static string? GetFormatFromAcceptHeader(string acceptHeader) =>
        acceptHeader switch
        {
            var h when h.Contains("application/xml",  StringComparison.OrdinalIgnoreCase) => "XML",
            var h when h.Contains("text/yaml",        StringComparison.OrdinalIgnoreCase) => "YAML",
            var h when h.Contains("application/yaml", StringComparison.OrdinalIgnoreCase) => "YAML",
            var h when h.Contains("text/csv",         StringComparison.OrdinalIgnoreCase) => "CSV",
            _ => null,
        };
}

// ── Request / Response models ─────────────────────────────────────────────────

/// <summary>
/// Request body for POST /api/v1/client/authenticate.
/// </summary>
public record ClientAuthRequest(
    string   TenantCode,                    // e.g. "LOGISTICS_CORE"
    string   Username,                      // email or identity reference
    string   Password,                      // plaintext (Local) or MOCK-* (stub IDP)
    string?  Format           = null,       // override graph format: JSON|XML|YAML|CSV
    string[]? RequestedScopes = null);      // optional scope filter (future use)

/// <summary>
/// Response from POST /api/v1/client/authenticate.
/// The client system uses Token for subsequent API calls and Graph for
/// authorization decisions without re-querying UMS.
/// </summary>
public record ClientAuthResponse(
    string   Token,         // JWT embedding graph claims
    string   TokenType,     // "Bearer"
    int      ExpiresIn,     // seconds until token expiry
    DateTime IssuedAt,
    string   Format,        // serialization format used: "JSON"|"XML"|"YAML"|"CSV"
    string   Graph,         // full authorization graph serialized in Format
    string   RequestId);    // correlates with the audit record for traceability

public record ClientAuthErrorResponse(
    string   Code,
    string   Message,
    string?  SupportReferenceId);
