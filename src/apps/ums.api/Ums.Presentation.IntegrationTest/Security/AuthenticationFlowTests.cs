using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Ums.Infrastructure.Persistence.Seeders;
using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Security;

public sealed class AuthenticationFlowTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthenticationFlowTests(UmsApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task RefreshToken_WithValidSessionCookie_ShouldReturnNewToken()
    {
        // 1. Iniciar sesión y capturar la cookie de sesión
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            tenantCode = "RANSA_PERU",
            username = "gerente.operaciones@ransa.pe",
            password = CoreDevDataSeeder.SuperAdminPassword,
            rememberMe = false,
        }, TestContext.Current.CancellationToken);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Capturar las cookies (la CookieAuthentication)
        var setCookieHeader = loginResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
        setCookieHeader.Should().NotBeNullOrEmpty();
        
        // 2. Extraer la cookie y configurar el HttpClient para la siguiente solicitud
        var cookieValue = setCookieHeader!.Split(';')[0];
        _client.DefaultRequestHeaders.Add("Cookie", cookieValue);

        // Opcional: obtener el Bearer Token para validar la respuesta original
        using var payload = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var initialToken = payload.RootElement.GetProperty("token").GetString();
        initialToken.Should().NotBeNullOrEmpty();

        // 3. Realizar el request de Refresh (usando la cookie)
        // El endpoint /refresh requiere [RequireAuthorization], por lo que valida la cookie
        var refreshResponse = await _client.PostAsync("/api/v1/auth/refresh", null, TestContext.Current.CancellationToken);
        
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Validar el nuevo token generado
        using var refreshPayload = JsonDocument.Parse(await refreshResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var newToken = refreshPayload.RootElement.GetProperty("token").GetString();
        
        newToken.Should().NotBeNullOrEmpty();
        newToken.Should().NotBe(initialToken, "El refresh token debe generar un JWT completamente nuevo.");
    }
}
