namespace Ums.Domain.Authorization.Graph;

/// <summary>
/// A registered action in the SystemSuite — the full catalogue of actions
/// the system can perform, returned in the graph so clients know what actions exist.
/// </summary>
public sealed record GraphAction(
    Guid   Id,
    string Code,
    string Name);
