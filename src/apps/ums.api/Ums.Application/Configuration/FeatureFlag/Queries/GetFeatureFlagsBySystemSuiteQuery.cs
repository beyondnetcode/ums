using Ums.Application.Configuration.FeatureFlag.DTOs;

namespace Ums.Application.Configuration.FeatureFlag.Queries;

public sealed record GetFeatureFlagsBySystemSuiteQuery(Guid SystemSuiteId) : IQuery<IReadOnlyList<FeatureFlagDto>>;
