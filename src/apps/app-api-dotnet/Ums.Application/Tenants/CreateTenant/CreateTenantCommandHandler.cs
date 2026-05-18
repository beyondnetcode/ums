namespace Ums.Application.Tenants.CreateTenant;

using Ums.Application.Abstractions.Messaging;
using Ums.Application.Abstractions.Persistence;
using Ums.Application.Common;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Enums;
using Ums.Domain.Identity.Tenant;
using Ums.Domain.Kernel;
using Ums.Domain.Kernel.ValueObjects;

public sealed class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, CreateTenantResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<CreateTenantResponse>> Handle(
        CreateTenantCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<CreateTenantResponse>.Failure("Authenticated user is required to create a tenant.");
        }

        var code = Code.Create(request.Code);
        var existingTenant = await _tenantRepository.FindByCodeAsync(code, cancellationToken);
        if (existingTenant is not null)
        {
            return Result<CreateTenantResponse>.Failure("Tenant code already exists.");
        }

        var type = DomainEnumerationParser.FromName<OrganizationType>(request.Type)!;
        var idpStrategy = DomainEnumerationParser.FromName<IdpStrategy>(request.IdpStrategy) ?? IdpStrategy.InternalBcrypt;
        var companyReference = string.IsNullOrWhiteSpace(request.CompanyReference)
            ? null
            : CompanyReference.Create(request.CompanyReference);
        var parentTenantId = request.ParentTenantId.HasValue
            ? TenantId.Load(request.ParentTenantId.Value)
            : null;

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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateTenantResponse>.Success(
            new CreateTenantResponse(tenantResult.Value.Props.Id.GetValue()));
    }
}
