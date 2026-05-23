using Ums.Application.Configuration.FeatureFlag.DTOs;

namespace Ums.Application.Configuration.FeatureFlag.Queries;

public sealed record GetFeatureFlagByIdQuery(Guid FeatureFlagId) : IQuery<FeatureFlagDto>;
