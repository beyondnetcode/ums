namespace Ums.Application.Common.Interfaces;

public interface IPasswordHashingService
{
    string Hash(string password);
}
