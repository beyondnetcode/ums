using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ums.Sdk.Authorization;
using Ums.Sdk.Contracts;

namespace Ums.Sdk.Client;

/// <summary>
/// Default implementation of <see cref="IUmsAuthClient"/> backed by <see cref="HttpClient"/>.
/// Validates the server's <c>schemaVersion</c> against the SDK's compatibility range and
/// returns <c>AUTH_205</c> when the server runs a MAJOR the SDK cannot interpret.
/// </summary>
public sealed class UmsAuthClient : IUmsAuthClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _http;
    private readonly UmsSdkClientOptions _options;

    public UmsAuthClient(HttpClient http, IOptions<UmsSdkClientOptions> options)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _options = options.Value;
        if (_options.BaseAddress is null && _http.BaseAddress is null)
            throw new InvalidOperationException(
                "UmsSdkClientOptions.BaseAddress must be set, or HttpClient.BaseAddress must be configured upstream.");

        if (_http.BaseAddress is null)
            _http.BaseAddress = _options.BaseAddress;
        _http.Timeout = _options.Timeout;
    }

    public async Task<Result<ClientAuthResult>> AuthenticateAsync(ClientAuthRequest request, CancellationToken ct = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync(_options.AuthenticatePath, request, JsonOptions, ct).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            return Result<ClientAuthResult>.Failure(UmsErrorCodes.IdpCallFailed, ex.Message);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            return Result<ClientAuthResult>.Failure(UmsErrorCodes.IdpCallFailed, $"HTTP timeout after {_options.Timeout}: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await SafeReadAsync(response, ct).ConfigureAwait(false);
            var code = MapHttpStatusToErrorCode(response.StatusCode);
            return Result<ClientAuthResult>.Failure(code, body);
        }

        ClientAuthResult? parsed;
        try
        {
            parsed = await response.Content.ReadFromJsonAsync<ClientAuthResult>(JsonOptions, ct).ConfigureAwait(false);
        }
        catch (JsonException ex)
        {
            return Result<ClientAuthResult>.Failure(UmsErrorCodes.AuthGraphMalformed, ex.Message);
        }

        if (parsed is null)
            return Result<ClientAuthResult>.Failure(UmsErrorCodes.AuthGraphMalformed, "Response body was null after deserialization.");

        if (string.IsNullOrWhiteSpace(parsed.Graph.SchemaVersion))
            return Result<ClientAuthResult>.Failure(UmsErrorCodes.AuthGraphSchemaMissing,
                "Server response does not carry a schemaVersion field.");

        if (!SchemaVersion.IsSupported(parsed.Graph.SchemaVersion))
            return Result<ClientAuthResult>.Failure(UmsErrorCodes.AuthGraphSchemaUnsupported,
                $"Server emitted schemaVersion '{parsed.Graph.SchemaVersion}' which is outside SDK compatibility " +
                $"({SchemaVersion.CompatibilityMinInclusive} ≤ x < {SchemaVersion.CompatibilityMaxExclusive}).");

        return Result<ClientAuthResult>.Success(parsed);
    }

    private static async Task<string> SafeReadAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var text = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(text) ? response.ReasonPhrase ?? "(no body)" : text;
        }
        catch
        {
            return response.ReasonPhrase ?? "(no body)";
        }
    }

    private static string MapHttpStatusToErrorCode(System.Net.HttpStatusCode status) => status switch
    {
        System.Net.HttpStatusCode.BadRequest => UmsErrorCodes.ValidationError,
        System.Net.HttpStatusCode.NotFound => UmsErrorCodes.TenantNotFound,
        System.Net.HttpStatusCode.Unauthorized => UmsErrorCodes.InvalidCredentials,
        System.Net.HttpStatusCode.Forbidden => UmsErrorCodes.UserNotActive,
        System.Net.HttpStatusCode.Locked => UmsErrorCodes.AccountLocked,
        _ => UmsErrorCodes.IdpCallFailed
    };
}
