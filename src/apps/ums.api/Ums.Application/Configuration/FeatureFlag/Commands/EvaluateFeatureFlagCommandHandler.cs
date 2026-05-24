using Ums.Application.Configuration.FeatureFlag.DTOs;

namespace Ums.Application.Configuration.FeatureFlag.Commands;

using Ums.Domain.Configuration;

public sealed class EvaluateFeatureFlagCommandHandler : ICommandHandler<EvaluateFeatureFlagCommand, EvaluateFeatureFlagResponse>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IUserContext _userContext;

    public EvaluateFeatureFlagCommandHandler(IFeatureFlagRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<EvaluateFeatureFlagResponse>> Handle(EvaluateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<EvaluateFeatureFlagResponse>.Failure("Authenticated user is required.");
        }

        if (!Guid.TryParse(_userContext.UserId, out var evaluatedBy))
        {
            return Result<EvaluateFeatureFlagResponse>.Failure("Authenticated user id must be a valid GUID.");
        }

        var featureFlag = await _repository.GetByIdAsync(request.FeatureFlagId, cancellationToken);
        if (featureFlag is null)
        {
            return Result<EvaluateFeatureFlagResponse>.Failure("Feature flag was not found.");
        }

        var context = request.Context.Trim();
        var evaluation = featureFlag.Evaluate(evaluatedBy, context);
        if (evaluation.IsFailure)
        {
            return Result<EvaluateFeatureFlagResponse>.Failure(evaluation.Error);
        }

        await _repository.UpdateAsync(featureFlag, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<EvaluateFeatureFlagResponse>.Success(new EvaluateFeatureFlagResponse(
            featureFlag.Props.Id.GetValue(),
            featureFlag.Props.FlagCode,
            evaluation.Value,
            context));
    }
}
