using System.Text.Json.Serialization;
using Ums.Sdk.Contracts;

namespace Ums.Sdk.Client;

/// <summary>
/// Successful response from <c>POST /api/v1/client/authenticate</c>. Carries the JWT used for
/// subsequent calls, the deserialized graph, and bookkeeping fields.
/// </summary>
public sealed record ClientAuthResult(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("tokenType")] string TokenType,
    [property: JsonPropertyName("expiresIn")] int ExpiresInSeconds,
    [property: JsonPropertyName("issuedAt")] DateTimeOffset IssuedAt,
    [property: JsonPropertyName("format")] string Format,
    [property: JsonPropertyName("graph")] AuthorizationGraph Graph,
    [property: JsonPropertyName("requestId")] Guid RequestId);
