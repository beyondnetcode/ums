namespace Ums.Application.Approvals.NotificationRule.Commands;

using Ums.Domain;
using Ums.Domain.Approvals;
using Ums.Domain.Enums;

public sealed class UpdateNotificationRuleChannelCommandHandler : ICommandHandler<UpdateNotificationRuleChannelCommand>
{
    private readonly INotificationRuleRepository _repository;
    private readonly IUserContext _userContext;

    public UpdateNotificationRuleChannelCommandHandler(
        INotificationRuleRepository repository,
        IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(UpdateNotificationRuleChannelCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result.Failure("Authenticated user is required to update a notification rule channel.");
        }

        var channel = DomainEnumerationParser.FromName<NotificationChannel>(request.NewChannel);
        if (channel is null)
        {
            return Result.Failure($"Notification channel '{request.NewChannel}' is not supported.");
        }

        var entity = await _repository.GetByIdAsync(request.NotificationRuleId, cancellationToken);
        if (entity is null)
        {
            return Result.Failure("Notification rule not found.");
        }

        var result = entity.UpdateChannel(channel, ActorId.Create(_userContext.UserId));
        if (result.IsFailure)
        {
            return result;
        }

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result.Success();
    }
}
