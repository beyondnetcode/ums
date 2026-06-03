namespace Ums.Application.Authorization.AssignmentRule.Commands;

using Ums.Application.Authorization.AssignmentRule.DTOs;

public sealed record CreateAssignmentRuleCommand(
    Guid TenantId,
    Guid TemplateId,
    Guid RoleId,
    int Priority) : ICommand<CreateAssignmentRuleResponse>;
