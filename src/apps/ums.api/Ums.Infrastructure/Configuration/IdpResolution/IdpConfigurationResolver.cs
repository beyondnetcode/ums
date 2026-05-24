using Ums.Application.Configuration.IdpConfiguration.DTOs;
using Ums.Application.Configuration.IdpConfiguration.Services;
using Ums.Domain.Configuration;
using Ums.Domain.Configuration.IdpConfiguration;
using Ums.Domain.Enums;
using Ums.Shell.Factory.Interfaces;

namespace Ums.Infrastructure.Configuration.IdpResolution;

internal sealed class IdpConfigurationResolver : IIdpConfigurationResolver
{
    private readonly IIdpConfigurationRepository _repository;
    private readonly IFactory _factory;

    public IdpConfigurationResolver(IIdpConfigurationRepository repository, IFactory factory)
    {
        _repository = repository;
        _factory = factory;
    }

    public async Task<Result<ResolvedIdpConfigurationDto>> ResolveAsync(
        Guid tenantId,
        Guid? systemSuiteId,
        string? emailDomain,
        string? providerType,
        CancellationToken cancellationToken = default)
    {
        var configurations = await _repository.GetByTenantIdAsync(tenantId, cancellationToken);

        var candidates = configurations
            .Where(configuration => configuration.Status == IdpConfigStatus.Active)
            .Where(configuration => !systemSuiteId.HasValue || configuration.SystemSuiteId.GetValue() == systemSuiteId.Value)
            .Where(configuration => string.IsNullOrWhiteSpace(providerType) || string.Equals(configuration.ProviderType.Name, providerType, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (candidates.Count == 0)
        {
            return Result<ResolvedIdpConfigurationDto>.Failure("Active IdP configuration not found.");
        }

        var normalizedDomain = NormalizeDomain(emailDomain);
        var domainMatchedCandidates = string.IsNullOrWhiteSpace(normalizedDomain)
            ? []
            : candidates.Where(configuration => MatchesDomain(configuration, normalizedDomain)).ToList();

        var domainMatched = domainMatchedCandidates.Count > 0;
        var selected = (domainMatched ? domainMatchedCandidates : candidates)
            .OrderBy(configuration => configuration.ResolutionPriority)
            .ThenByDescending(configuration => configuration.Version)
            .First();

        var strategy = _factory.Create<IdpResolutionStrategyCriteria, IIdpResolutionStrategy>(
            new IdpResolutionStrategyCriteria(selected.ProviderType.Name))
            .SingleOrDefault();

        if (strategy is null)
        {
            return Result<ResolvedIdpConfigurationDto>.Failure($"No IdP resolution strategy is registered for provider type '{selected.ProviderType.Name}'.");
        }

        return Result<ResolvedIdpConfigurationDto>.Success(strategy.Resolve(new IdpResolutionContext(selected, domainMatched)));
    }

    private static bool MatchesDomain(IdpConfiguration configuration, string normalizedDomain)
        => configuration.Props.DomainHints.Any(hint => string.Equals(hint.Trim(), normalizedDomain, StringComparison.OrdinalIgnoreCase));

    private static string? NormalizeDomain(string? emailDomain)
    {
        if (string.IsNullOrWhiteSpace(emailDomain))
        {
            return null;
        }

        var trimmed = emailDomain.Trim();
        var atIndex = trimmed.IndexOf('@');
        return atIndex >= 0 ? trimmed[(atIndex + 1)..].Trim().ToLowerInvariant() : trimmed.ToLowerInvariant();
    }
}
