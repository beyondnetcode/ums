using Ums.Application.Identity.Tenant.DTOs;

namespace Ums.Application.Identity.Tenant.Commands;

using Ums.Domain.Identity.Tenant;

public sealed class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, CreateTenantResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContext _userContext;

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<CreateTenantResponse>> Handle(
        CreateTenantCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<CreateTenantResponse>.Failure("Authenticated user is required to create a tenant.");
        }

        var code = Code.Create(request.Code);
        var existingTenant = await _tenantRepository.GetByCodeAsync(code.GetValue(), cancellationToken);
        if (existingTenant is not null)
        {
            return Result<CreateTenantResponse>.Failure("Tenant code already exists.");
        }

        var type = DomainEnumerationParser.FromName<OrganizationType>(request.Type)!;
        var idpStrategy = DomainEnumerationParser.FromName<IdpStrategy>(request.IdpStrategy) ?? IdpStrategy.InternalBcrypt;
        var companyReference = string.IsNullOrWhiteSpace(request.CompanyReference)
            ? null
            : CompanyReference.Create(request.CompanyReference);
        var parentTenantId = TenantId.Load(Guid.NewGuid());

        var tenantResult = Tenant.Create(
            code,
            Name.Create(request.Name),
            type,
            ActorId.Create(_userContext.UserId),
            idpStrategy,
            companyReference,
            parentTenantId);

        if (tenantResult.IsFailure)
        {
            return Result<CreateTenantResponse>.Failure(tenantResult.Error);
        }

        await _tenantRepository.AddAsync(tenantResult.Value, cancellationToken);
        await _tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreateTenantResponse>.Success(
            new CreateTenantResponse(tenantResult.Value.Props.Id.GetValue()));
    }
}
