using Ums.Application.Approvals.NotificationRule.DTOs;
using Ums.Application.Common.Aop;
using Ums.Shell.Aop.Aspects;

namespace Ums.Application.Approvals.NotificationRule.Commands;

using Ums.Application.Common.Interfaces;
using Ums.Domain;
using Ums.Domain.Approvals;
using Ums.Domain.Approvals.NotificationRule;
using Ums.Domain.Enums;

public sealed class CreateNotificationRuleCommandHandler : ICommandHandler<CreateNotificationRuleCommand, CreateNotificationRuleResponse>
{
    private readonly INotificationRuleRepository _repository;
    private readonly IUserContext _userContext;

    public CreateNotificationRuleCommandHandler(INotificationRuleRepository repository, IUserContext userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<CreateNotificationRuleResponse>> Handle(CreateNotificationRuleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result<CreateNotificationRuleResponse>.Failure("Authenticated user is required.");

        var channel = DomainEnumerationParser.FromName<NotificationChannel>(request.Channel)!;

        var result = NotificationRule.Create(
            TenantId.Load(request.TenantId),
            channel,
            TextValueObject.Create(request.Recipient),
            ActorId.Create(_userContext.UserId));

        if (result.IsFailure) return Result<CreateNotificationRuleResponse>.Failure(result.Error);

        await _repository.AddAsync(result.Value, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreateNotificationRuleResponse>.Success(new CreateNotificationRuleResponse(result.Value.Props.Id.GetValue()));
    }
}
