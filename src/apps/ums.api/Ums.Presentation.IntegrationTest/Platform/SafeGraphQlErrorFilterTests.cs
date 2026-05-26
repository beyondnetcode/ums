using HotChocolate;
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

        var responseError = new SafeGraphQlErrorFilter().OnError(sourceError);

        responseError.Message.Should().Be("The request could not be completed.");
        responseError.Code.Should().Be("UMS_INTERNAL_ERROR");
        responseError.Extensions.Should().NotContainKey("stackTrace");
    }
}
