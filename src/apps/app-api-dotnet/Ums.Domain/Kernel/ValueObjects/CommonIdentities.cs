namespace Ums.Domain.Kernel.ValueObjects;

using Ums.Shell.Ddd;

public class TenantId : IdValueObject
{
    private TenantId(Guid value) : base(value) { }
    public static new TenantId Create() => new TenantId(Guid.NewGuid());
    public static new TenantId Load(Guid value) => new TenantId(value);
    public static new TenantId Load(string value) => new TenantId(Guid.Parse(value));
}

public class SystemSuiteId : IdValueObject
{
    private SystemSuiteId(Guid value) : base(value) { }
    public static new SystemSuiteId Create() => new SystemSuiteId(Guid.NewGuid());
    public static new SystemSuiteId Load(Guid value) => new SystemSuiteId(value);
    public static new SystemSuiteId Load(string value) => new SystemSuiteId(Guid.Parse(value));
}

public class UserId : IdValueObject
{
    private UserId(Guid value) : base(value) { }
    public static new UserId Create() => new UserId(Guid.NewGuid());
    public static new UserId Load(Guid value) => new UserId(value);
    public static new UserId Load(string value) => new UserId(Guid.Parse(value));
}

public class BranchId : IdValueObject
{
    private BranchId(Guid value) : base(value) { }
    public static new BranchId Create() => new BranchId(Guid.NewGuid());
    public static new BranchId Load(Guid value) => new BranchId(value);
    public static new BranchId Load(string value) => new BranchId(Guid.Parse(value));
}

public class OrganizationId : IdValueObject
{
    private OrganizationId(Guid value) : base(value) { }
    public static new OrganizationId Create() => new OrganizationId(Guid.NewGuid());
    public static new OrganizationId Load(Guid value) => new OrganizationId(value);
    public static new OrganizationId Load(string value) => new OrganizationId(Guid.Parse(value));
}
