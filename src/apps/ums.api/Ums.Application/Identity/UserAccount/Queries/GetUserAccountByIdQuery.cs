using Ums.Application.Identity.UserAccount.DTOs;

namespace Ums.Application.Identity.UserAccount.Queries;

public sealed record GetUserAccountByIdQuery(Guid UserAccountId) : IQuery<UserAccountDto>;
