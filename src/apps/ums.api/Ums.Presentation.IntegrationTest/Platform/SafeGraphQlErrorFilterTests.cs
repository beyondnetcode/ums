using HotChocolate;
using Ums.Globalization.Access;
using Ums.Presentation.GraphQL;

namespace Ums.Presentation.IntegrationTest.Platform;

public sealed class SafeGraphQlErrorFilterTests
{
    [Fact]
    public void OnError_ShouldNotExposeTechnicalMessageOrExtensions()
    {
        var sourceError = ErrorBuilder.New()
            .SetMessage("System.InvalidOperationException: private resolver detail")
            .SetException(new InvalidOperationException("private resolver stack"))
            .SetExtension("stackTrace", "private stack")
            .SetCode("UMS_INTERNAL_ERROR")
            .Build();

        IError responseError;
        using (CultureContext.Set("es"))
        {
            responseError = new SafeGraphQlErrorFilter().OnError(sourceError);
        }

        responseError.Message.Should().Be(
            "No se pudo completar la operación debido a un error inesperado. Intente nuevamente más tarde.");
        responseError.Code.Should().Be("UMS_INTERNAL_ERROR");
        var extensions = responseError.Extensions!;
        extensions.Should().NotContainKey("stackTrace");
        Guid.TryParse(extensions["errorId"]?.ToString(), out _).Should().BeTrue();
    }
}
