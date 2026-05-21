using Ums.Application.Approvals.NotificationRule.DTOs;

namespace Ums.Application.Approvals.NotificationRule.Queries;

public sealed record GetNotificationRuleByIdQuery(Guid NotificationRuleId) : IQuery<NotificationRuleDto>;
