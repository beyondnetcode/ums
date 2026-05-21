using Ums.Application.Identity.UserAccount.DTOs;

namespace Ums.Application.Identity.UserAccount.Commands;

public sealed record CreateUserAccountCommand(
    Guid TenantId,
    Guid? BranchId,
    string Email,
    string Category,
    string? IdentityReference,
    string? IdentityReferenceType) : ICommand<CreateUserAccountResponse>;
