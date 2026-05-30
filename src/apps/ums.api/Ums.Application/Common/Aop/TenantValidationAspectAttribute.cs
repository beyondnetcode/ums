using System;
using BeyondNetCode.Shell.Aop.Aspects;

namespace Ums.Application.Common.Aop;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public sealed class TenantValidationAspectAttribute : AbstractAspectAttribute
{
}
