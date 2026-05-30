using System;
using System.Linq;
using System.Reflection;
using Ums.Application.Common.Interfaces;
using BeyondNetCode.Shell.Aop;

namespace Ums.Application.Common.Aop;

public sealed class TenantValidationAspect : AbstractAspect<TenantValidationAspectAttribute>
{
    private readonly IUserContext _userContext;

    public TenantValidationAspect(IUserContext userContext)
    {
        _userContext = userContext;
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, PropertyInfo?> _tenantIdPropertyCache = new();

    public override void Apply(IJoinPoint joinPoint)
    {
        var attribute = GetAttribute(joinPoint);
        if (attribute is null)
        {
            Proceed(joinPoint);
            return;
        }

        var request = joinPoint.Arguments.FirstOrDefault(a => a is not System.Threading.CancellationToken);
        if (request is not null)
        {
            var requestType = request.GetType();
            var tenantIdProperty = _tenantIdPropertyCache.GetOrAdd(
                requestType,
                type => type.GetProperty("TenantId", BindingFlags.Instance | BindingFlags.Public));

            if (tenantIdProperty is not null)
            {
                var requestTenantId = tenantIdProperty.GetValue(request)?.ToString();
                var userTenantId = _userContext.TenantId;

                // Validate if both are present and not equal
                if (!string.IsNullOrWhiteSpace(requestTenantId) && 
                    !string.IsNullOrWhiteSpace(userTenantId) && 
                    !string.Equals(requestTenantId, userTenantId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException($"Tenant mismatch. User belongs to {userTenantId}, but request targets {requestTenantId}.");
                }
            }
        }

        Proceed(joinPoint);
    }

    private void Proceed(IJoinPoint joinPoint)
    {
        if (GetNext() is null)
            joinPoint.Proceed();
        else
            GetNext()!.Apply(joinPoint);
    }
}
