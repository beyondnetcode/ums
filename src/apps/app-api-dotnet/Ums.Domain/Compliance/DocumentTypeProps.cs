namespace Ums.Domain.Compliance;

public class DocumentTypeProps : ParametricCatalogProps
{
    public bool RequiresExpirationDate { get; set; }
    public bool BlocksAccessOnExpiration { get; set; }

    public DocumentTypeProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}
