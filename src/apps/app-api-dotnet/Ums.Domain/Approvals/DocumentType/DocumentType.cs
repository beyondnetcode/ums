namespace Ums.Domain.Approvals.DocumentType;

public sealed class DocumentType : AggregateRoot<DocumentType, DocumentTypeProps>
{
    private DocumentType(DocumentTypeProps props) : base(props)
    {
    }

    public TenantId TenantId => Props.TenantId;
    public Code Code => Props.Code;
    public Name Name => Props.Name;
    public Description Description => Props.Description;

    public DocumentTypeId GetId() => DocumentTypeId.Load(Props.Id.GetValue());

    public static Result<DocumentType> Create(
        TenantId tenantId,
        Code code,
        Name name,
        Description description,
        ActorId createdBy)
    {
        var props = new DocumentTypeProps(IdValueObject.Create(), tenantId, code, name, description, createdBy);
        var documentType = new DocumentType(props);

        if (!documentType.IsValid())
        {
            return Result<DocumentType>.Failure(documentType.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<DocumentType>.Success(documentType);
    }

    public Result Update(Name name, Description description, ActorId updatedBy)
    {
        Props.Name = name;
        Props.Description = description;

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }
}
