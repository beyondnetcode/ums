using System;
using System.Linq;
using Ums.Application.Common.Interfaces;
using BeyondNetCode.Shell.Aop;

namespace Ums.Application.Common.Aop;

public sealed class AuthorizationAspect : AbstractAspect<AuthorizationAspectAttribute>
{
    private readonly IUserContext _userContext;

    public AuthorizationAspect(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public override void Apply(IJoinPoint joinPoint)
    {
        var attribute = GetAttribute(joinPoint);

        // If not on the interface method, check the MediatR Request (args[0])
        if (attribute is null && joinPoint.Arguments != null && joinPoint.Arguments.Length > 0 && joinPoint.Arguments[0] != null)
        {
            attribute = (AuthorizationAspectAttribute?)Attribute.GetCustomAttribute(joinPoint.Arguments[0]!.GetType(), typeof(AuthorizationAspectAttribute));
        }

        // Check the target class as fallback
        if (attribute is null && joinPoint.TargetType != null)
        {
            attribute = (AuthorizationAspectAttribute?)Attribute.GetCustomAttribute(joinPoint.TargetType, typeof(AuthorizationAspectAttribute));
        }

        if (attribute is null)
        {
            Console.WriteLine($"[AOP] No AuthorizationAspectAttribute found on {joinPoint.TargetType?.Name}");
            Proceed(joinPoint);
            return;
        }

        // Convención sobre Configuración: Inferir permisos basados en el nombre de la clase o método
        var resourceCode = attribute.ResourceCode;
        var actionCode = attribute.ActionCode;

        if (string.IsNullOrEmpty(resourceCode) || string.IsNullOrEmpty(actionCode))
        {
            var targetName = joinPoint.TargetType.Name;
            // Ej: CreateUserCommandHandler -> "user", "create"
            if (targetName.EndsWith("CommandHandler"))
            {
                var cleanName = targetName.Replace("CommandHandler", "");
                if (cleanName.StartsWith("Create")) { actionCode ??= "create"; resourceCode ??= cleanName.Substring(6).ToLowerInvariant(); }
                else if (cleanName.StartsWith("Update")) { actionCode ??= "update"; resourceCode ??= cleanName.Substring(6).ToLowerInvariant(); }
                else if (cleanName.StartsWith("Delete")) { actionCode ??= "delete"; resourceCode ??= cleanName.Substring(6).ToLowerInvariant(); }
            }
            else if (targetName.EndsWith("QueryHandler"))
            {
                var cleanName = targetName.Replace("QueryHandler", "");
                if (cleanName.StartsWith("Get")) { actionCode ??= "read"; resourceCode ??= cleanName.Substring(3).ToLowerInvariant(); }
                else if (cleanName.StartsWith("List")) { actionCode ??= "read"; resourceCode ??= cleanName.Substring(4).ToLowerInvariant(); }
            }
        }

        if (!string.IsNullOrEmpty(resourceCode) && !string.IsNullOrEmpty(actionCode))
        {
            var permission = $"{resourceCode}:{actionCode}";
            Console.WriteLine($"[AOP] Evaluating permission '{permission}' for {joinPoint.TargetType?.Name}");
            
            if (!_userContext.HasPermission(permission))
            {
                Console.WriteLine($"[AOP] Access DENIED for '{permission}'");
                throw new UnauthorizedAccessException($"Access denied. Required permission: '{permission}'.");
            }
            Console.WriteLine($"[AOP] Access GRANTED for '{permission}'");
        }
        else
        {
            Console.WriteLine($"[AOP] Resource/Action code could not be inferred for {joinPoint.TargetType?.Name}");
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
