using Ums.Application.Common.Aop;
using Ums.Shell.Aop.Aspects;

namespace Ums.Application.Configuration.AppConfiguration.Commands;

using Ums.Application.Common.Interfaces;
using Ums.Domain.Configuration;

public sealed class ArchiveAppConfigurationCommandHandler : ICommandHandler<ArchiveAppConfigurationCommand>
{
    private readonly IAppConfigurationRepository _repository;
    private readonly IUserContext _userContext;

    public ArchiveAppConfigurationCommandHandler(IAppConfigurationRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(ArchiveAppConfigurationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result.Failure("Authenticated user is required.");
        }

        var appConfiguration = await _repository.GetByIdAsync(request.AppConfigurationId, cancellationToken);
        if (appConfiguration is null)
        {
            return Result.Failure("App configuration was not found.");
        }

        var result = appConfiguration.Archive(ActorId.Create(_userContext.UserId));
        if (result.IsFailure)
        {
            return result;
        }

        await _repository.UpdateAsync(appConfiguration, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
