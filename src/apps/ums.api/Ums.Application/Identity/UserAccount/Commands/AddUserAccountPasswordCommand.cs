using Ums.Application.Identity.UserAccount.DTOs;

namespace Ums.Application.Identity.UserAccount.Commands;

public sealed record AddUserAccountPasswordCommand(Guid UserAccountId, string PasswordHash)
    : ICommand<AddUserAccountPasswordResponse>;
