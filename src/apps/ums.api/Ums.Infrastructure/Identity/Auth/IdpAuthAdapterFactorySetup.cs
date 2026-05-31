using BeyondNetCode.Shell.Factory.Impl;
using Ums.Application.Identity.Auth;
using Ums.Domain.Identity.Auth;

namespace Ums.Infrastructure.Identity.Auth;

/// <summary>
/// Shell.Factory registration for IIdpAuthAdapter.
/// Pattern: identical to IdpResolutionStrategyFactorySetup.
///
/// StubIdpAuthAdapter covers all strategies in non-Production.
/// Future adapters:
///   For&lt;IdpAuthAdapterCriteria, IIdpAuthAdapter&gt;().Create&lt;AzureAdIdpAuthAdapter&gt;().When(x => x.StrategyName == "AzureAd")
/// </summary>
internal sealed class IdpAuthAdapterFactorySetup : AbstractFactorySetupSource
{
    public IdpAuthAdapterFactorySetup()
    {
        // StubIdpAuthAdapter: accepts all strategies in dev/test environments.
        // Registration is guarded by DI — only registered when !IsProduction.
        For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
            .Create<StubIdpAuthAdapter>().When(_ => true);
    }
}
