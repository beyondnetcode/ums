using Ums.Application.Identity.UserAccount.DTOs;
using Ums.Domain.Identity.UserAccount;

namespace Ums.Application.Identity.UserAccount.Queries;

public sealed class GetUserAccountMfaEnrollmentsQueryHandler
    : IQueryHandler<GetUserAccountMfaEnrollmentsQuery, IReadOnlyList<MfaEnrollmentDto>>
{
    private readonly IUserAccountRepository _userAccountRepository;

    public GetUserAccountMfaEnrollmentsQueryHandler(IUserAccountRepository userAccountRepository)
    {
        _userAccountRepository = userAccountRepository;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<IReadOnlyList<MfaEnrollmentDto>>> Handle(
        GetUserAccountMfaEnrollmentsQuery request,
        CancellationToken cancellationToken)
    {
        var userAccount = await _userAccountRepository.GetByIdAsync(request.UserAccountId, cancellationToken);

        if (userAccount is null)
        {
            return Result<IReadOnlyList<MfaEnrollmentDto>>.Failure("User account was not found.");
        }

        var dtos = userAccount.MfaEnrollments
            .Select(e => new MfaEnrollmentDto(
                e.Id.GetValue(),
                e.Method.Name,
                e.Status.Name,
                e.Props.Audit.GetValue().CreatedAt))
            .ToList();

        return Result<IReadOnlyList<MfaEnrollmentDto>>.Success(dtos);
    }
}
