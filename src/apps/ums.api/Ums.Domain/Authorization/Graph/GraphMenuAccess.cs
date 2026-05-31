namespace Ums.Domain.Authorization.Graph;

/// <summary>
/// The complete SystemSuite menu hierarchy with effective permissions per option.
/// Each option carries the resolved effect (Allow/Deny/NotGranted) and its source
/// (Template or Override), giving the client system everything it needs to render
/// and enforce access at the UI level without re-querying UMS.
/// </summary>
public sealed record GraphMenuModule(
    Guid                      Id,
    string                    Code,
    string                    Name,
    int                       SortOrder,
    string                    Status,
    IReadOnlyList<GraphMenu>  Menus);

public sealed record GraphMenu(
    Guid                         Id,
    string                       Code,
    string                       Label,
    int                          SortOrder,
    IReadOnlyList<GraphSubMenu>  SubMenus);

public sealed record GraphSubMenu(
    Guid                          Id,
    string                        Code,
    string                        Label,
    int                           SortOrder,
    IReadOnlyList<GraphMenuOption> Options);

public sealed record GraphMenuOption(
    Guid             Id,
    string           Code,
    string           Label,
    string           ActionCode,
    AccessEffect Effect,
    PermissionSource Source);
