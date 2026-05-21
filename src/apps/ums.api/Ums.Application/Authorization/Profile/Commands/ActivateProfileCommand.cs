using Ums.Application.Authorization.Profile.DTOs;

namespace Ums.Application.Authorization.Profile.Commands;

public sealed record ActivateProfileCommand(Guid ProfileId) : ICommand;
