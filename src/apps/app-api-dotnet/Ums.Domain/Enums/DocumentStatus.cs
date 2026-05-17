namespace Ums.Domain.Enums;

public class DocumentStatus : DomainEnumeration
{
    public static readonly DocumentStatus Valid = new(1, nameof(Valid));
    public static readonly DocumentStatus Expired = new(2, nameof(Expired));
    public static readonly DocumentStatus PendingRenewal = new(3, nameof(PendingRenewal));

    private DocumentStatus(int id, string name) : base(id, name) { }
}
