using Ums.Application.Identity.UserAccount.DTOs;

namespace Ums.Application.Identity.UserAccount.Commands;

public sealed record AddPasswordCommand(Guid UserAccountId, string PasswordHash) : ICommand<AddPasswordResponse>;
