using Ums.Application.Configuration.IdpConfiguration.DTOs;
using Ums.Application.Configuration.IdpConfiguration.Services;

namespace Ums.Application.Configuration.IdpConfiguration.Queries;

public sealed class ResolveIdpConfigurationQueryHandler : IQueryHandler<ResolveIdpConfigurationQuery, ResolvedIdpConfigurationDto>
{
    private readonly IIdpConfigurationResolver _resolver;

    public ResolveIdpConfigurationQueryHandler(IIdpConfigurationResolver resolver)
    {
        _resolver = resolver;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public Task<Result<ResolvedIdpConfigurationDto>> Handle(ResolveIdpConfigurationQuery request, CancellationToken cancellationToken)
        => _resolver.ResolveAsync(request.TenantId, request.SystemSuiteId, request.EmailDomain, request.ProviderType, cancellationToken);
}
