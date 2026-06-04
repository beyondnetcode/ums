using Ums.Application.Identity.UserAccount.DTOs;

namespace Ums.Application.Identity.UserAccount.Queries;

public sealed record GetUserAccountMfaEnrollmentsQuery(Guid UserAccountId)
    : IQuery<IReadOnlyList<MfaEnrollmentDto>>;
