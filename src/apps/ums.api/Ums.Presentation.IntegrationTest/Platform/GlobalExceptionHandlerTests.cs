using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Ums.Presentation.Middleware;

namespace Ums.Presentation.IntegrationTest.Platform;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task ExceptionResponse_ShouldExposeReferenceWithoutTechnicalDetails()
    {
        var handler = new GlobalExceptionHandler(
            _ => throw new InvalidOperationException("System.Private.Stack.Detail"),
            NullLogger<GlobalExceptionHandler>.Instance);
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "corr-rest-123",
        };
        context.Response.Body = new MemoryStream();

        await handler.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(
            context.Response.Body,
            cancellationToken: TestContext.Current.CancellationToken);
        var response = document.RootElement;

        response.GetProperty("traceId").GetString().Should().Be("corr-rest-123");
        response.GetRawText().Should().NotContain("System.Private.Stack.Detail");
        response.GetRawText().Should().NotContain("stackTrace");
        response.GetRawText().Should().NotContain("exceptionType");
    }
}
