namespace Ums.Application.IGA.PromotionRequest.Commands;

using Ums.Domain.IGA;

public sealed class AddImpactAnalysisCommandHandler : ICommandHandler<AddImpactAnalysisCommand>
{
    private readonly IPromotionRequestRepository _repository;
    private readonly IUserContext _userContext;

    public AddImpactAnalysisCommandHandler(IPromotionRequestRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(AddImpactAnalysisCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var entity = await _repository.GetByIdAsync(request.PromotionRequestId, cancellationToken);
        if (entity is null) return Result.Failure("Promotion request not found.");

        var actor = ActorId.Create(_userContext.UserId);
        var result = entity.AddImpactAnalysis(
            request.RiskScore,
            TextValueObject.Create(request.RiskLevel),
            request.NewPermissionsCount,
            request.RemovedPermissionsCount,
            request.AffectedSystemsCount,
            request.ConflictingPermissions is not null ? TextValueObject.Create(request.ConflictingPermissions) : null,
            request.RiskFactors is not null ? TextValueObject.Create(request.RiskFactors) : null,
            request.SuggestedMitigations is not null ? TextValueObject.Create(request.SuggestedMitigations) : null,
            actor);

        if (result.IsFailure) return result;

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
