using Ums.Application.Approvals.NotificationRule.DTOs;

namespace Ums.Application.Approvals.NotificationRule.Commands;

public sealed record CreateNotificationRuleCommand(
    Guid TenantId, string Channel, string Recipient) : ICommand<CreateNotificationRuleResponse>;
