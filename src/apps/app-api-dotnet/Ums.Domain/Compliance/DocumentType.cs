namespace Ums.Domain.Compliance;

public sealed class DocumentType : ParametricCatalogEntity<DocumentType, DocumentTypeProps>
{
    private DocumentType(DocumentTypeProps props) : base(props) { }

    public bool RequiresExpirationDate => Props.RequiresExpirationDate;
    public bool BlocksAccessOnExpiration => Props.BlocksAccessOnExpiration;

    public static Result<DocumentType> Create(Guid tenantId, string code, string value, string description, bool requiresExpirationDate, bool blocksAccessOnExpiration, string version = "1.0.0")
    {
        var props = new DocumentTypeProps
        {
            RequiresExpirationDate = requiresExpirationDate,
            BlocksAccessOnExpiration = blocksAccessOnExpiration
        };

        var documentType = new DocumentType(props);
        var result = documentType.SetCatalogFields(tenantId, code, value, description, version);
        
        return result.IsFailure ? Result<DocumentType>.Failure(result.Error) : Result<DocumentType>.Success(documentType);
    }
}
