namespace Ums.Presentation.GraphQL;

using HotChocolate.Execution.Configuration;
using Ums.Presentation.GraphQL.Approvals;
using Ums.Presentation.GraphQL.Audit;
using Ums.Presentation.GraphQL.Authorization;
using Ums.Presentation.GraphQL.Configuration;
using Ums.Presentation.GraphQL.Identity;

public static class GraphQlServiceCollectionExtensions
{
    /// <summary>
    /// REC-11: GraphQL introspection is restricted to Development environment only.
    /// In Staging/Production, introspection queries are rejected with 400 to prevent
    /// schema enumeration by unauthorized clients via __schema or __type queries.
    /// </summary>
    public static IRequestExecutorBuilder AddUmsGraphQl(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        var builder = services
            .AddGraphQLServer()
            .AddHttpRequestInterceptor<TenantContextGraphQlInterceptor>()
            .AddQueryType(d => d.Name("Query"))
            .AddTypeExtension<TenantQueries>()
            .AddTypeExtension<UserAccountQueries>()
            .AddTypeExtension<DelegationQueries>()
            .AddTypeExtension<ProfileQueries>()
            .AddTypeExtension<SystemSuiteQueries>()
            .AddTypeExtension<PermissionTemplateQueries>()
            .AddTypeExtension<RoleQueries>()
            .AddTypeExtension<AuditRecordQueries>()
            .AddTypeExtension<ApprovalWorkflowQueries>()
            .AddTypeExtension<ApprovalRequestQueries>()
            .AddTypeExtension<DocumentTypeQueries>()
            .AddTypeExtension<UserDocumentQueries>()
            .AddTypeExtension<AccessEnforcementPolicyQueries>()
            .AddTypeExtension<NotificationRuleQueries>()
            .AddTypeExtension<AppConfigurationQueries>()
            .AddTypeExtension<FeatureFlagQueries>()
            .AddTypeExtension<IdpConfigurationQueries>()
            .AddErrorFilter<SafeGraphQlErrorFilter>()
            .AddMaxExecutionDepthRule(12)
            .ModifyRequestOptions(options =>
            {
                options.ExecutionTimeout = TimeSpan.FromSeconds(10);
                options.IncludeExceptionDetails = false;
            });

        // REC-11: Disable introspection outside Development so attackers cannot
        // enumerate the full schema surface via __schema or __type queries.
        // DisableIntrospection() is an extension on IRequestExecutorBuilder
        // from HotChocolate.Execution.Configuration.RequestExecutorBuilderExtensions.
        if (!environment.IsDevelopment())
        {
            builder.DisableIntrospection();
        }

        return builder;
    }
}
