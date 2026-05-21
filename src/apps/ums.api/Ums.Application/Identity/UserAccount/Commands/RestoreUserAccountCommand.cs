using Ums.Application.Identity.UserAccount.DTOs;

namespace Ums.Application.Identity.UserAccount.Commands;

public sealed record RestoreUserAccountCommand(Guid UserAccountId) : ICommand;
