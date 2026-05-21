using Ums.Application.Approvals.NotificationRule.DTOs;
using Ums.Domain.Approvals;
using Ums.Domain.Approvals.NotificationRule;

namespace Ums.Application.Approvals.NotificationRule.Queries;

public sealed class GetNotificationRuleByIdQueryHandler : IQueryHandler<GetNotificationRuleByIdQuery, NotificationRuleDto>
{
    private readonly INotificationRuleRepository _repository;

    public GetNotificationRuleByIdQueryHandler(INotificationRuleRepository repository) => _repository = repository;

    public async Task<Result<NotificationRuleDto>> Handle(GetNotificationRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.NotificationRuleId, cancellationToken);
        if (entity is null) return Result<NotificationRuleDto>.Failure("Notification rule not found.");

        return Result<NotificationRuleDto>.Success(new NotificationRuleDto(
            entity.Props.Id.GetValue(), entity.Props.TenantId.GetValue(), entity.Props.Channel.ToString(),
            entity.Props.Recipient.GetValue(), entity.Props.IsActive));
    }
}
