namespace Ums.Infrastructure.Services;

using Ums.Application.Common.Interfaces;

public sealed class BcryptPasswordHashingService : IPasswordHashingService
{
    private const int WorkFactor = 12;

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
}
