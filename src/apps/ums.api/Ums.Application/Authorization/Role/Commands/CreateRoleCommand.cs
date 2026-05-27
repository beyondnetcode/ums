using Ums.Application.Authorization.Role.DTOs;

namespace Ums.Application.Authorization.Role.Commands;

public sealed record CreateRoleCommand(
    Guid SystemSuiteId,
    string Code,
    string Value,
    string Description,
    Guid? ParentRoleId,
    int HierarchyLevel,
    int PromotionOrder) : ICommand<CreateRoleResponse>;
