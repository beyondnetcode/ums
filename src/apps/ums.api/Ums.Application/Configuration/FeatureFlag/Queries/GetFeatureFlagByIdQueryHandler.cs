using Ums.Application.Configuration.FeatureFlag.DTOs;
using Ums.Domain.Authorization;

namespace Ums.Application.Configuration.FeatureFlag.Queries;

using Ums.Domain.Configuration;

public sealed class GetFeatureFlagByIdQueryHandler : IQueryHandler<GetFeatureFlagByIdQuery, FeatureFlagDto>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly ISystemSuiteRepository _systemSuiteRepository;

    public GetFeatureFlagByIdQueryHandler(
        IFeatureFlagRepository repository,
        ISystemSuiteRepository systemSuiteRepository)
    {
        _repository = repository;
        _systemSuiteRepository = systemSuiteRepository;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<FeatureFlagDto>> Handle(GetFeatureFlagByIdQuery request, CancellationToken cancellationToken)
    {
        var featureFlag = await _repository.GetByIdAsync(request.FeatureFlagId, cancellationToken);
        if (featureFlag is null)
        {
            return Result<FeatureFlagDto>.Failure("Feature flag was not found.");
        }

        var systemSuite = await _systemSuiteRepository.GetByIdAsync(featureFlag.Props.SystemSuiteId.GetValue(), cancellationToken);

        return Result<FeatureFlagDto>.Success(new FeatureFlagDto(
            featureFlag.Props.Id.GetValue(),
            featureFlag.Props.SystemSuiteId.GetValue(),
            systemSuite?.Props.Code.GetValue() ?? string.Empty,
            systemSuite?.Props.Name.GetValue() ?? string.Empty,
            featureFlag.Props.FlagCode,
            featureFlag.Props.FlagType.Name,
            featureFlag.Props.FlagTargets,
            featureFlag.Props.Status.Name,
            featureFlag.Props.LinkedResourceType?.Name,
            featureFlag.Props.LinkedResourceId?.GetValue(),
            featureFlag.Props.RolloutPercentage,
            featureFlag.Criteria.Select(c => new FeatureFlagCriteriaDto(
                c.Props.Id.GetValue(),
                c.CriteriaType,
                c.Operator,
                c.Value,
                c.CreatedAtUtc)).ToList()));
    }
}
