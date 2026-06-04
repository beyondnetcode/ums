using Ums.Domain.Identity.UserManagementDelegation;

namespace Ums.Application.Common.Interfaces;

public interface IUserManagementDelegationAccessService
{
    Task<Result> EnsureCanExecuteAsync(
        Guid targetTenantId,
        DelegatedAction action,
        Guid? targetScopeId = null,
        CancellationToken cancellationToken = default);
}
