using Ums.Application.Identity.UserAccount.DTOs;

namespace Ums.Application.Identity.UserAccount.Commands;

public sealed record EnrollUserAccountMfaCommand(Guid UserAccountId, string Method)
    : ICommand<EnrollUserAccountMfaResponse>;
