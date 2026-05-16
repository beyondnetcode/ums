namespace Ums.Domain.Identity;

using Ums.Domain.Common;
using Ums.Domain.Enums;
using Ums.Domain.Events;
using Ums.Shell.Ddd;
using Ums.Shell.Ddd.Interfaces;
using Ums.Shell.Ddd.ValueObjects.Common;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Domain.Identity.ValueObjects;

public class TenantProps : IProps
{
    public IdValueObject Id { get; private set; }
    public Code Code { get; private set; }
    public Name Name { get; private set; }
    public OrganizationType Type { get; private set; }
    public IdpStrategy IdpStrategy { get; private set; }
    public Value? CompanyReference { get; private set; }
    public TenantId? ParentTenantId { get; private set; }
    public TenantStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public TenantProps(IdValueObject id, Code code, Name name, OrganizationType type, IdpStrategy idpStrategy, Value? companyReference, TenantId? parentTenantId)
    {
        Id = id;
        Code = code;
        Name = name;
        Type = type;
        IdpStrategy = idpStrategy;
        CompanyReference = companyReference;
        ParentTenantId = parentTenantId;
        Status = TenantStatus.Active;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class Tenant : AggregateRoot<Tenant, TenantProps>
{
    private readonly List<Branch> _branches = new();

    private Tenant(TenantProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new TenantCreatedEvent(Props.Id.GetValue(), Props.Code.GetValue(), Props.Name.GetValue()), true);
        }
    }

    public string Code => Props.Code.GetValue();
    public string Name => Props.Name.GetValue();
    public OrganizationType Type => Props.Type;
    public IdpStrategy IdpStrategy => Props.IdpStrategy;
    public string? CompanyReference => Props.CompanyReference?.GetValue();
    public Guid? ParentTenantId => Props.ParentTenantId?.GetValue();
    public TenantStatus Status => Props.Status;
    public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();

    public static Result<Tenant> Create(
        string code,
        string name,
        OrganizationType type,
        IdpStrategy idpStrategy = null!,
        string? companyReference = null,
        Guid? parentTenantId = null)
    {
        idpStrategy ??= IdpStrategy.InternalBcrypt;
        
        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);
        if (string.IsNullOrWhiteSpace(name))
            return Result<Tenant>.Failure(DomainErrors.NameRequired);

        var id = IdValueObject.Create();
        var props = new TenantProps(
            id,
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            type,
            idpStrategy,
            companyReference != null ? global::Ums.Domain.Kernel.ValueObjects.Value.Create(companyReference.Trim()) : null,
            parentTenantId.HasValue ? global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(parentTenantId.Value) : null);

        var tenant = new Tenant(props);
        return Result<Tenant>.Success(tenant);
    }

    public Result AddBranch(string code, string name, string? geofencingMetadata = null)
    {
        var branchResult = Branch.Create(Props.Id.GetValue(), code, name, geofencingMetadata);
        if (branchResult.IsFailure)
            return Result.Failure(branchResult.Error);

        if (_branches.Any(branch => branch.Code == branchResult.Value.Code))
            return Result.Failure("Branch code must be unique inside the tenant.");

        _branches.Add(branchResult.Value);
        DomainEvents.ApplyChange(new BranchCreatedEvent(Props.Id.GetValue(), branchResult.Value.GetId(), branchResult.Value.Code), true);
        Props.Audit.Update("system");
        return Result.Success();
    }

    public Result Suspend()
    {
        if (Props.Status == TenantStatus.Archived)
            return Result.Failure("Archived tenants cannot be suspended.");

        Props.Status = TenantStatus.Suspended;
        Props.Audit.Update("system");
        return Result.Success();
    }

    public Result Activate()
    {
        if (Props.Status == TenantStatus.Archived)
            return Result.Failure("Archived tenants cannot be activated.");

        Props.Status = TenantStatus.Active;
        Props.Audit.Update("system");
        return Result.Success();
    }
}

public class BranchProps : IProps
{
    public IdValueObject Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public Code Code { get; private set; }
    public Name Name { get; private set; }
    public Value? GeofencingMetadata { get; private set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public BranchProps(IdValueObject id, TenantId tenantId, Code code, Name name, Value? geofencingMetadata)
    {
        Id = id;
        TenantId = tenantId;
        Code = code;
        Name = name;
        GeofencingMetadata = geofencingMetadata;
        IsActive = true;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class Branch : Entity<Branch, BranchProps>
{
    private Branch(BranchProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public string Code => Props.Code.GetValue();
    public string Name => Props.Name.GetValue();
    public string? GeofencingMetadata => Props.GeofencingMetadata?.GetValue();
    public bool IsActive => Props.IsActive;

    public Guid GetId() => Props.Id.GetValue();

    public static Result<Branch> Create(Guid tenantId, string code, string name, string? geofencingMetadata = null)
    {
        if (tenantId == Guid.Empty)
            return Result<Branch>.Failure(DomainErrors.TenantRequired);

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);
        if (string.IsNullOrWhiteSpace(name))
            return Result<Branch>.Failure(DomainErrors.NameRequired);

        var props = new BranchProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            geofencingMetadata != null ? global::Ums.Domain.Kernel.ValueObjects.Value.Create(geofencingMetadata) : null);

        return Result<Branch>.Success(new Branch(props));
    }
}

public class UserAccountProps : IProps
{
    public IdValueObject Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public EmailAddress Email { get; private set; }
    public IdentityReference IdentityReference { get; private set; }
    public Value? PasswordHash { get; private set; }
    public UserAccountStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public UserAccountProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.BranchId? branchId, EmailAddress email, global::Ums.Domain.Identity.ValueObjects.IdentityReference identityReference, global::Ums.Domain.Kernel.ValueObjects.Value? passwordHash)
    {
        Id = id;
        TenantId = tenantId;
        BranchId = branchId;
        Email = email;
        IdentityReference = identityReference;
        PasswordHash = passwordHash;
        Status = UserAccountStatus.Pending;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class UserAccount : AggregateRoot<UserAccount, UserAccountProps>
{
    private readonly List<UserProfileAssignment> _profileAssignments = new();

    private UserAccount(UserAccountProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new UserRegisteredEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.Email.GetValue()), true);
        }
    }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid? BranchId => Props.BranchId?.GetValue();
    public string Email => Props.Email.GetValue();
    public string IdentityReference => Props.IdentityReference.GetValue();
    public string? PasswordHash => Props.PasswordHash?.GetValue();
    public UserAccountStatus Status => Props.Status;
    public IReadOnlyCollection<UserProfileAssignment> ProfileAssignments => _profileAssignments.AsReadOnly();

    public static Result<UserAccount> Register(Guid tenantId, string email, string identityReference, Guid? branchId = null, string? passwordHash = null)
    {
        if (tenantId == Guid.Empty)
            return Result<UserAccount>.Failure(DomainErrors.TenantRequired);

        var emailResult = global::Ums.Domain.Kernel.ValueObjects.EmailAddress.Create(email);
        if (emailResult.IsFailure) return Result<UserAccount>.Failure(emailResult.Error);
        var emailValue = emailResult.Value;
        
        if (string.IsNullOrWhiteSpace(identityReference))
            return Result<UserAccount>.Failure("Identity reference is required.");

        var props = new UserAccountProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            branchId.HasValue ? global::Ums.Domain.Kernel.ValueObjects.BranchId.Load(branchId.Value) : null,
            emailValue,
            global::Ums.Domain.Identity.ValueObjects.IdentityReference.Create(identityReference.Trim()),
            passwordHash != null ? global::Ums.Domain.Kernel.ValueObjects.Value.Create(passwordHash) : null);

        var user = new UserAccount(props);
        return Result<UserAccount>.Success(user);
    }

    public Result Activate()
    {
        if (Props.Status == UserAccountStatus.Terminated)
            return Result.Failure("Terminated users cannot be activated.");

        Props.Status = UserAccountStatus.Active;
        DomainEvents.ApplyChange(new UserActivatedEvent(Props.TenantId.GetValue(), Props.Id.GetValue()), true);
        Props.Audit.Update("system");
        return Result.Success();
    }

    public Result Block(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("Block reason is required.");

        Props.Status = UserAccountStatus.Blocked;
        DomainEvents.ApplyChange(new UserBlockedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), reason.Trim()), true);
        Props.Audit.Update("system");
        return Result.Success();
    }

    public Result AssignProfile(Guid profileId, Guid? branchId = null)
    {
        if (profileId == Guid.Empty)
            return Result.Failure("Profile identifier is required.");

        if (_profileAssignments.Any(item => item.ProfileId == profileId && item.BranchId == branchId))
            return Result.Failure("Profile is already assigned to the user in this scope.");

        var assignmentProps = new UserProfileAssignmentProps(
            IdValueObject.Create(),
            Props.TenantId,
            IdValueObject.Load(Props.Id.GetValue()),
            global::Ums.Domain.Authorization.ValueObjects.ProfileId.Load(profileId),
            branchId.HasValue ? global::Ums.Domain.Kernel.ValueObjects.BranchId.Load(branchId.Value) : null);
        
        _profileAssignments.Add(new UserProfileAssignment(assignmentProps));
        Props.Audit.Update("system");
        return Result.Success();
    }
}

public class UserProfileAssignmentProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public IdValueObject UserAccountId { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.ProfileId ProfileId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.BranchId? BranchId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }

    public UserProfileAssignmentProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, IdValueObject userAccountId, global::Ums.Domain.Authorization.ValueObjects.ProfileId profileId, global::Ums.Domain.Kernel.ValueObjects.BranchId? branchId)
    {
        Id = id;
        TenantId = tenantId;
        UserAccountId = userAccountId;
        ProfileId = profileId;
        BranchId = branchId;
        AssignedAt = DateTimeOffset.UtcNow;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class UserProfileAssignment : Entity<UserProfileAssignment, UserProfileAssignmentProps>
{
    internal UserProfileAssignment(UserProfileAssignmentProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid UserAccountId => Props.UserAccountId.GetValue();
    public Guid ProfileId => Props.ProfileId.GetValue();
    public Guid? BranchId => Props.BranchId?.GetValue();
    public DateTimeOffset AssignedAt => Props.AssignedAt;
}
