using Ums.Application.Identity.UserAccount.DTOs;

namespace Ums.Application.Identity.UserAccount.Commands;

using Ums.Domain.Identity.UserAccount;
using Ums.Domain.Enums;

public sealed class CreateUserAccountCommandHandler : ICommandHandler<CreateUserAccountCommand, CreateUserAccountResponse>
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserContext _userContext;

    public CreateUserAccountCommandHandler(
        IUserAccountRepository userAccountRepository,
        IUserContext userContext)
    {
        _userAccountRepository = userAccountRepository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<CreateUserAccountResponse>> Handle(
        CreateUserAccountCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<CreateUserAccountResponse>.Failure("Authenticated user is required to create a user account.");
        }

        if (!string.IsNullOrWhiteSpace(_userContext.TenantId) &&
            !string.Equals(request.TenantId.ToString(), _userContext.TenantId, StringComparison.OrdinalIgnoreCase))
        {
            return Result<CreateUserAccountResponse>.Failure(
                $"Tenant mismatch. User belongs to tenant '{_userContext.TenantId}', but request targets '{request.TenantId}'.");
        }

        var email = Email.Create(request.Email);
        var existingUser = await _userAccountRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser is not null)
        {
            return Result<CreateUserAccountResponse>.Failure("User email already exists.");
        }

        var category = DomainEnumerationParser.FromName<UserCategory>(request.Category)!;
        var identityReference = string.IsNullOrWhiteSpace(request.IdentityReference)
            ? null
            : IdentityReference.Create(request.IdentityReference);
        var identityReferenceType = string.IsNullOrWhiteSpace(request.IdentityReferenceType)
            ? null
            : DomainEnumerationParser.FromName<IdentityReferenceType>(request.IdentityReferenceType);
        var branchId = request.BranchId.HasValue
            ? BranchId.Load(request.BranchId.Value)
            : null;

        var userAccountResult = UserAccount.Create(
            TenantId.Load(request.TenantId),
            email,
            category,
            identityReference,
            identityReferenceType,
            ActorId.Create(_userContext.UserId),
            branchId);

        if (userAccountResult.IsFailure)
        {
            return Result<CreateUserAccountResponse>.Failure(userAccountResult.Error);
        }

        await _userAccountRepository.AddAsync(userAccountResult.Value, cancellationToken);
        await _userAccountRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreateUserAccountResponse>.Success(
            new CreateUserAccountResponse(userAccountResult.Value.Props.Id.GetValue()));
    }
}
