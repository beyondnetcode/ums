using BeyondNetCode.Shell.Factory.Installer.Extensions;
using BeyondNetCode.Shell.Factory.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Ums.Application.Approvals.ApprovalRequest.Services;
using Ums.Application.Approvals.NotificationRule.Services;
using Ums.Application.Authorization.Graph.Serializers;
using Ums.Application.Authorization.Profile.Exporters;
using Ums.Domain.Identity.Tenant.IdentityProvider;
using Ums.Infrastructure.Aop;
using Ums.Infrastructure.Approvals.ApprovalRequest;
using Ums.Infrastructure.Approvals.NotificationRule;
using Ums.Infrastructure.Authorization.Graph;
using Ums.Infrastructure.Configuration.IdpResolution;
using Ums.Infrastructure.Identity.Auth;
using Ums.Domain.Identity.Auth;
using Ums.Infrastructure.Persistence.Authorization.Exporters;

namespace Ums.Infrastructure;

internal static class DependencyInjectionFactories
{
    public static IServiceCollection AddUmsFactories(this IServiceCollection services, bool isNotProduction)
    {
        services.AddFactory(builder =>
        {
            // --- Approvals ---
            builder.AddTransient<INotificationRecipientStrategy, EmailNotificationRecipientStrategy>();
            builder.AddTransient<INotificationRecipientStrategy, SmsNotificationRecipientStrategy>();
            builder.AddTransient<INotificationRecipientStrategy, InAppNotificationRecipientStrategy>();
            builder.AddSource<NotificationRecipientStrategyFactorySetup>();

            builder.AddTransient<IApprovalRequestCreationStrategy, ManualApprovalRequestCreationStrategy>();
            builder.AddTransient<IApprovalRequestCreationStrategy, AutoApproveApprovalRequestCreationStrategy>();
            builder.AddSource<ApprovalRequestCreationStrategyFactorySetup>();

            // --- Identity ---
            builder.AddTransient<IIdpResolutionStrategy, InternalBcryptIdpResolutionStrategy>();
            builder.AddTransient<IIdpResolutionStrategy, ZitadelIdpResolutionStrategy>();
            builder.AddTransient<IIdpResolutionStrategy, AzureAdIdpResolutionStrategy>();
            builder.AddTransient<IIdpResolutionStrategy, OktaIdpResolutionStrategy>();
            builder.AddTransient<IIdpResolutionStrategy, KeycloakIdpResolutionStrategy>();
            builder.AddTransient<IIdpResolutionStrategy, Auth0IdpResolutionStrategy>();
            builder.AddTransient<IIdpResolutionStrategy, GoogleIdpResolutionStrategy>();
            builder.AddTransient<IIdpResolutionStrategy, LdapIdpResolutionStrategy>();
            builder.AddTransient<IIdpResolutionStrategy, Saml2IdpResolutionStrategy>();
            builder.AddTransient<IIdpResolutionStrategy, GenericOidcIdpResolutionStrategy>();
            builder.AddSource<IdpResolutionStrategyFactorySetup>();

            // --- Authorization ---
            builder.AddTransient<IProfileExporter, ProfileJsonExporter>();
            builder.AddTransient<IProfileExporter, ProfileXmlExporter>();
            builder.AddTransient<IProfileExporter, ProfileYamlExporter>();
            builder.AddTransient<IProfileExporter, ProfileCsvExporter>();
            builder.AddSource<ProfileExportFactorySetup>();

            builder.AddTransient<IAuthorizationGraphSerializer, JsonAuthorizationGraphSerializer>();
            builder.AddTransient<IAuthorizationGraphSerializer, XmlAuthorizationGraphSerializer>();
            builder.AddTransient<IAuthorizationGraphSerializer, YamlAuthorizationGraphSerializer>();
            builder.AddTransient<IAuthorizationGraphSerializer, CsvAuthorizationGraphSerializer>();
            builder.AddSource<AuthorizationGraphSerializerFactorySetup>();

            // --- IDP Adapters ---
            if (isNotProduction)
            {
                
                builder.AddSource<IdpAuthAdapterStubFactorySetup>();
            }
            else
            {
                builder.AddSource<IdpAuthAdapterFactorySetup>();
            }
        });

        services.AddSingleton<IFactoryInterceptor, FactoryLoggingInterceptor>();

        return services;
    }
}
