using BeyondNetCode.Shell.Factory.Impl;
using Ums.Application.Identity.Auth;
using Ums.Domain.Enums;
using Ums.Domain.Identity.Auth;

namespace Ums.Infrastructure.Identity.Auth;

/// <summary>
/// Shell.Factory registration for IIdpAuthAdapter — Production configuration.
///
/// Routes each IDP strategy name to its concrete adapter.
/// Each .When() condition matches the exact <c>IdpStrategy.Name</c> value so the
/// factory discriminates correctly when the dispatcher passes
/// <c>new IdpAuthAdapterCriteria(provider.Props.Strategy.Name)</c>.
///
/// Pattern: identical to IdpResolutionStrategyFactorySetup.
///
/// To add a real adapter for a strategy (e.g. AzureAd):
///   1. Implement <c>AzureAdIdpAuthAdapter : IIdpAuthAdapter</c>
///   2. Add the .Create line below and register the class in DependencyInjection.cs
/// </summary>
internal sealed class IdpAuthAdapterFactorySetup : AbstractFactorySetupSource
{
    public IdpAuthAdapterFactorySetup()
    {
        // Production adapters registered per strategy.
        // Until a real adapter is implemented for a strategy, remove its comment
        // and add the class. An unregistered strategy returns null → AUTH_012.

        // For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
        //     .Create<AzureAdIdpAuthAdapter>()
        //     .When(x => x.StrategyName == IdpStrategy.AzureAd.Name);

        // For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
        //     .Create<OktaIdpAuthAdapter>()
        //     .When(x => x.StrategyName == IdpStrategy.Okta.Name);

        // For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
        //     .Create<ZitadelIdpAuthAdapter>()
        //     .When(x => x.StrategyName == IdpStrategy.Zitadel.Name);

        // For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
        //     .Create<GenericOidcIdpAuthAdapter>()
        //     .When(x => x.StrategyName == IdpStrategy.GenericOidc.Name);
    }
}

/// <summary>
/// Shell.Factory registration for IIdpAuthAdapter — Development / Test override.
///
/// Registers <see cref="StubIdpAuthAdapter"/> for every known strategy so the
/// full IDP auth flow can be tested end-to-end with MOCK-* credentials.
/// This setup is registered ONLY in non-Production environments (see DependencyInjection.cs).
///
/// The stub succeeds only when credentials start with "MOCK-", so even if accidentally
/// loaded in Production it will not authenticate real credentials.
/// </summary>
internal sealed class IdpAuthAdapterStubFactorySetup : AbstractFactorySetupSource
{
    public IdpAuthAdapterStubFactorySetup()
    {
        // One entry per strategy so routing still works correctly — the stub is chosen
        // per strategy name, not via a catch-all that hides factory routing bugs.
        For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
            .Create<StubIdpAuthAdapter>()
            .When(x => x.StrategyName == IdpStrategy.AzureAd.Name);

        For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
            .Create<StubIdpAuthAdapter>()
            .When(x => x.StrategyName == IdpStrategy.Okta.Name);

        For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
            .Create<StubIdpAuthAdapter>()
            .When(x => x.StrategyName == IdpStrategy.Zitadel.Name);

        For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
            .Create<StubIdpAuthAdapter>()
            .When(x => x.StrategyName == IdpStrategy.Keycloak.Name);

        For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
            .Create<StubIdpAuthAdapter>()
            .When(x => x.StrategyName == IdpStrategy.Auth0.Name);

        For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
            .Create<StubIdpAuthAdapter>()
            .When(x => x.StrategyName == IdpStrategy.Google.Name);

        For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
            .Create<StubIdpAuthAdapter>()
            .When(x => x.StrategyName == IdpStrategy.Ldap.Name);

        For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
            .Create<StubIdpAuthAdapter>()
            .When(x => x.StrategyName == IdpStrategy.Saml2.Name);

        For<IdpAuthAdapterCriteria, IIdpAuthAdapter>()
            .Create<StubIdpAuthAdapter>()
            .When(x => x.StrategyName == IdpStrategy.GenericOidc.Name);
    }
}
