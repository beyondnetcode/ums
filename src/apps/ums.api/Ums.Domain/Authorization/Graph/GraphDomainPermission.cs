namespace Ums.Domain.Authorization.Graph;

/// <summary>
/// Authorization state for a domain resource (Aggregate or Entity) registered
/// in the SystemSuite. Each resource lists every system action with its resolved
/// effect, giving clients fine-grained access control at the domain model level.
/// </summary>
public sealed record GraphDomainPermission(
    Guid                              ResourceId,
    string                            ResourceType,   // "Aggregate" | "Entity"
    string                            ResourceCode,
    string                            ResourceName,
    Guid?                             ModuleId,
    IReadOnlyList<GraphDomainAction>  Actions);

public sealed record GraphDomainAction(
    Guid             ActionId,
    string           ActionCode,
    string           ActionName,
    AccessEffect Effect,
    PermissionSource Source);
