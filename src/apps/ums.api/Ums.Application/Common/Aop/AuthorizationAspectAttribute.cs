using System;
using BeyondNetCode.Shell.Aop.Aspects;

namespace Ums.Application.Common.Aop;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public sealed class AuthorizationAspectAttribute : AbstractAspectAttribute
{
    public string? ResourceCode { get; set; }
    public string? ActionCode { get; set; }

    public AuthorizationAspectAttribute() { }

    public AuthorizationAspectAttribute(string resourceCode, string actionCode)
    {
        ResourceCode = resourceCode;
        ActionCode = actionCode;
    }
}
