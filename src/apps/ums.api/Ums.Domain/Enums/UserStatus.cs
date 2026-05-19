namespace Ums.Domain.Enums;

public class UserStatus : DomainEnumeration
{
    public static readonly UserStatus Pending = new(1, nameof(Pending));
    public static readonly UserStatus Active = new(2, nameof(Active));
    public static readonly UserStatus Blocked = new(3, nameof(Blocked));

    private UserStatus(int id, string name) : base(id, name) { }
}
