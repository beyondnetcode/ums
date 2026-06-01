using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ums.Sdk.Authorization;
using Ums.Sdk.Contracts;

namespace Ums.Sdk.Authorization.AspNetCore;

/// <summary>
/// Middleware that extracts the AuthorizationGraph from an incoming bearer JWT and stores it on
/// <c>HttpContext.Items["UmsAuthGraph"]</c> so the <see cref="HttpContextAuthGraphAccessor"/>
/// (and any downstream validator or AOP aspect) can read it.
///
/// The JWT is expected to be a 3-segment compact serialization. The payload section is base64url-
/// decoded as JSON and one of its top-level claims is treated as the serialized graph: either an
/// embedded object (when the claim is itself the graph) or a string containing the JSON to
/// re-parse (when the server packaged the graph as a string claim).
/// </summary>
public sealed class UmsAuthGraphMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    private readonly RequestDelegate _next;
    private readonly IOptions<UmsAuthGraphMiddlewareOptions> _options;
    private readonly ILogger<UmsAuthGraphMiddleware>? _logger;

    public UmsAuthGraphMiddleware(
        RequestDelegate next,
        IOptions<UmsAuthGraphMiddlewareOptions> options,
        ILogger<UmsAuthGraphMiddleware>? logger = null)
    {
        _next = next;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var opt = _options.Value;
        var graph = ExtractGraph(context, opt);

        if (graph is not null)
        {
            if (string.IsNullOrWhiteSpace(graph.SchemaVersion))
            {
                if (opt.RejectIncompatibleGraphs)
                {
                    await Reject(context, "AUTH_204", "AuthorizationGraph payload does not declare a schemaVersion.").ConfigureAwait(false);
                    return;
                }
                _logger?.LogWarning("UmsAuthGraphMiddleware: incoming graph lacks schemaVersion — proceeding without binding.");
            }
            else if (!SchemaVersion.IsSupported(graph.SchemaVersion))
            {
                if (opt.RejectIncompatibleGraphs)
                {
                    await Reject(context, "AUTH_205",
                        $"Server emitted schemaVersion '{graph.SchemaVersion}' which is outside SDK compatibility.").ConfigureAwait(false);
                    return;
                }
                _logger?.LogWarning("UmsAuthGraphMiddleware: incompatible schemaVersion '{Version}' — proceeding without binding.", graph.SchemaVersion);
            }
            else if (opt.RejectExpiredGraphs && graph.ValidUntil <= DateTimeOffset.UtcNow)
            {
                await Reject(context, "AUTH_201", "AuthorizationGraph has expired.").ConfigureAwait(false);
                return;
            }
            else
            {
                context.Items[HttpContextAuthGraphAccessor.ItemsKey] = graph;
            }
        }

        await _next(context).ConfigureAwait(false);
    }

    private AuthorizationGraph? ExtractGraph(HttpContext context, UmsAuthGraphMiddlewareOptions opt)
    {
        string? token = ExtractBearerToken(context);
        if (token is null) return null;

        // Three-segment compact JWT.
        var segments = token.Split('.');
        if (segments.Length < 2) return null;

        byte[] payload;
        try
        {
            payload = Base64UrlDecode(segments[1]);
        }
        catch
        {
            _logger?.LogWarning("UmsAuthGraphMiddleware: JWT payload is not valid base64url.");
            return null;
        }

        JsonElement payloadJson;
        try
        {
            payloadJson = JsonSerializer.Deserialize<JsonElement>(payload, JsonOptions);
        }
        catch
        {
            _logger?.LogWarning("UmsAuthGraphMiddleware: JWT payload is not valid JSON.");
            return null;
        }

        if (!payloadJson.TryGetProperty(opt.JwtBodyClaim, out var graphClaim))
            return null;

        try
        {
            return graphClaim.ValueKind switch
            {
                JsonValueKind.Object => graphClaim.Deserialize<AuthorizationGraph>(JsonOptions),
                JsonValueKind.String => JsonSerializer.Deserialize<AuthorizationGraph>(graphClaim.GetString()!, JsonOptions),
                _ => null
            };
        }
        catch (JsonException ex)
        {
            _logger?.LogWarning(ex, "UmsAuthGraphMiddleware: failed to deserialize '{Claim}' claim.", opt.JwtBodyClaim);
            return null;
        }
    }

    private static string? ExtractBearerToken(HttpContext context)
    {
        string? header = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(header)) return null;
        const string prefix = "Bearer ";
        if (!header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;
        var token = header[prefix.Length..].Trim();
        return token.Length == 0 ? null : token;
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var normalized = input.Replace('-', '+').Replace('_', '/');
        switch (normalized.Length % 4)
        {
            case 2: normalized += "=="; break;
            case 3: normalized += "=";  break;
            case 1: throw new FormatException("Invalid base64url length.");
        }
        return Convert.FromBase64String(normalized);
    }

    private static async Task Reject(HttpContext context, string code, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { code, message });
        await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(body)).ConfigureAwait(false);
    }
}
