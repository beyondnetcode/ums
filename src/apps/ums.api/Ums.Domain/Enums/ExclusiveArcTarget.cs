namespace Ums.Domain.Enums;

public class ExclusiveArcTarget : DomainEnumeration
{
    public static readonly ExclusiveArcTarget SystemSuite = new(1, nameof(SystemSuite));
    public static readonly ExclusiveArcTarget Module = new(2, nameof(Module));
    public static readonly ExclusiveArcTarget Submodule = new(3, nameof(Submodule));
    public static readonly ExclusiveArcTarget Option = new(4, nameof(Option));

    private ExclusiveArcTarget(int id, string name) : base(id, name) { }
}
