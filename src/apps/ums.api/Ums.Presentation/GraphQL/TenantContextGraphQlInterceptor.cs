namespace Ums.Presentation.GraphQL;

using HotChocolate.AspNetCore;
using HotChocolate.Execution;

/// <summary>
/// Propagates the HTTP request's IServiceProvider to HotChocolate's operation execution.
///
/// HotChocolate creates its own DI scope for resolvers, separate from the ASP.NET Core
/// HTTP request scope. This means scoped services initialized by middleware (like ITenantContext,
/// which is populated by TenantContextMiddleware) would be uninitialized when injected into
/// MediatR handlers resolved from HotChocolate's scope.
///
/// By setting the HTTP request's RequestServices as the operation's service provider,
/// all handlers resolved via MediatR share the same scoped instances (including the
/// already-initialized ITenantContext), ensuring tenant isolation applies to GraphQL queries.
/// </summary>
public sealed class TenantContextGraphQlInterceptor : DefaultHttpRequestInterceptor
{
    public override ValueTask OnCreateAsync(
        HttpContext context,
        IRequestExecutor requestExecutor,
        OperationRequestBuilder requestBuilder,
        CancellationToken cancellationToken)
    {
        requestBuilder.SetServices(context.RequestServices);

        return base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
    }
}
