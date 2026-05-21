using Ums.Application.Common.Interfaces;
using Ums.Application.Identity.UserAccount.DTOs;
using Ums.Domain.Identity.UserAccount;

namespace Ums.Application.Identity.UserAccount.Queries;

public sealed class GetUserAccountByIdQueryHandler : IQueryHandler<GetUserAccountByIdQuery, UserAccountDto>
{
    private readonly IUserAccountRepository _userAccountRepository;

    public GetUserAccountByIdQueryHandler(IUserAccountRepository userAccountRepository)
    {
        _userAccountRepository = userAccountRepository;
    }

    public async Task<Result<UserAccountDto>> Handle(
        GetUserAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userAccount = await _userAccountRepository.GetByIdAsync(request.UserAccountId, cancellationToken);

        if (userAccount is null)
        {
            return Result<UserAccountDto>.Failure("User account not found.");
        }

        return Result<UserAccountDto>.Success(new UserAccountDto(
            userAccount.Props.Id.GetValue(),
            userAccount.Props.TenantId.GetValue(),
            userAccount.Props.BranchId?.GetValue(),
            userAccount.Props.Email.GetValue(),
            userAccount.Props.Category.ToString(),
            userAccount.Props.Status.ToString(),
            userAccount.Props.IdentityReference?.GetValue(),
            userAccount.Props.IdentityReferenceType?.ToString()));
    }
}
