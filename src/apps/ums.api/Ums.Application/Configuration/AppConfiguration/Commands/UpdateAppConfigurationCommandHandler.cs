
namespace Ums.Application.Configuration.AppConfiguration.Commands;

using Ums.Domain.Configuration;

public sealed class UpdateAppConfigurationCommandHandler : ICommandHandler<UpdateAppConfigurationCommand>
{
    private readonly IAppConfigurationRepository _repository;
    private readonly IUserContext _userContext;

    public UpdateAppConfigurationCommandHandler(IAppConfigurationRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(UpdateAppConfigurationCommand request, CancellationToken cancellationToken)
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

        var result = appConfiguration.Update(
            ConfigurationValue.Create(request.Value),
            Description.Create(request.Description),
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure)
        {
            return result;
        }

        // REC-10: Pass RowVersion from If-Match header to enforce ETag-based optimistic locking.
        // The repository sets EF Core's original value so DbUpdateConcurrencyException is raised
        // if a concurrent modification occurred since the client fetched the ETag.
        await _repository.UpdateAsync(appConfiguration, request.RowVersion, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}
