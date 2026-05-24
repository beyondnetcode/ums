using Ums.Application.Common.Aop;
using Ums.Shell.Aop.Aspects;

namespace Ums.Application.Configuration.FeatureFlag.Commands;

using Ums.Application.Common.Interfaces;
using Ums.Domain.Configuration;

public sealed class ArchiveFeatureFlagCommandHandler : ICommandHandler<ArchiveFeatureFlagCommand>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IUserContext _userContext;

    public ArchiveFeatureFlagCommandHandler(IFeatureFlagRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(ArchiveFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result.Failure("Authenticated user is required.");
        }

        var featureFlag = await _repository.GetByIdAsync(request.FeatureFlagId, cancellationToken);
        if (featureFlag is null)
        {
            return Result.Failure("Feature flag was not found.");
        }

        var result = featureFlag.Archive(ActorId.Create(_userContext.UserId));
        if (result.IsFailure)
        {
            return result;
        }

        await _repository.UpdateAsync(featureFlag, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
