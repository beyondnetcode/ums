using Ums.Application.Authorization.Profile.DTOs;

namespace Ums.Application.Authorization.Profile.Commands;

public sealed record DeactivateProfileCommand(Guid ProfileId) : ICommand;
